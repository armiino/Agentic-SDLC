using AgenticSdlc.Host.FullWorkflow.Adr;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A5 (06.08., E-3): der deterministische ADR-Kern — Scan (nur aktive design-Items ohne ADR),
// Nummern aus dem CORE, MADR-Renderer, Gate (Coverage/Pflichtfelder/Related), Apply (Datei+Payload,
// Idempotenz „zweimal = keine doppelte ADR", Status-Folge nach ②-Ablösung), Index + Übersicht.
public sealed class AdrProjectionTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") =>
        new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, ["SL-1"], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Design(string id, string text = "Firestore als Datenbank", string? adrId = null, string? adrStatus = null, string status = "accepted") =>
        Item(id, "architecture", text, status) with { Architecture = new ArchitecturePayload(["design", "constraint"], "weil", adrId, adrStatus) };

    private static ProjectStateDocument Core(ProjectStateItem[] items, ProjectStateRelation[]? rels = null)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], rels?.ToList() ?? [], [], []);

    private static AdrDraft Draft(string itemId = "ARCH-39", string title = "Firestore als Datenbank") =>
        new(itemId, title, "Wir brauchen eine Persistenz für Bewohnerdaten.", "Wir nutzen Firestore.",
            "Bindet datenhaltende PBIs an Firestore.", ["REQ-70"]);

    [Fact]
    public void Pending_sind_nur_AKTIVE_design_Items_OHNE_AdrId()
    {
        var core = Core([
            Design("ARCH-39"),
            Design("ARCH-40", adrId: "ADR-0001", adrStatus: AdrStatus.Accepted),          // schon projiziert
            Design("ARCH-41", status: "superseded"),                                       // nicht aktiv
            Item("ARCH-42", "architecture", "nur Rahmen") with { Architecture = new(["constraint"], "x") }]);

        Assert.Equal(["ARCH-39"], AdrProjection.PendingAdrItems(core).Select(i => i.ItemId));
        Assert.Equal(2, AdrProjection.NextAdrNumber(core));                                // aus dem CORE
    }

    [Fact]
    public void Gate_prueft_Coverage_Pflichtfelder_und_Related_Existenz()
    {
        var core = Core([Design("ARCH-39"), Design("ARCH-40"), Item("REQ-70", "requirement", "Firestore festgelegt")]);
        var pending = AdrProjection.PendingAdrItems(core);

        var ok = AdrGate.Check(pending, [Draft("ARCH-39"), Draft("ARCH-40", "Zweite Entscheidung")], core);
        Assert.True(ok.Pass, string.Join("|", ok.Errors.Select(e => e.Code)));

        var bad = AdrGate.Check(pending,
            [Draft("ARCH-39") with { Context = " ", RelatedIds = ["PBI-1"] }], core);
        Assert.Contains(bad.Errors, e => e.Code == "MISSING_DRAFT" && e.ItemId == "ARCH-40");
        Assert.Contains(bad.Errors, e => e.Code == "MISSING_FIELD");
        Assert.Contains(bad.Errors, e => e.Code == "UNKNOWN_RELATED");                     // PBI ist kein Wahrheits-Item
        Assert.All(bad.Errors, e => Assert.Equal(Repairability.Repairable, e.Repairability));
    }

    [Fact]
    public void Apply_schreibt_MADR_Datei_stempelt_Payload_und_ist_idempotent()
    {
        var core = Core([Design("ARCH-39"), Item("REQ-70", "requirement", "Firestore ist festgelegt")]);

        var (updated, files, adrIds) = AdrGate.Apply(core, [Draft()], "docs/adr");

        Assert.Equal(["ADR-0001"], adrIds);
        var item = updated.Items.Single(i => i.ItemId == "ARCH-39");
        Assert.Equal(("ADR-0001", AdrStatus.Accepted), (item.Architecture!.AdrId, item.Architecture.AdrStatus));

        var adr = files.Single(f => f.Path.EndsWith("0001-firestore-als-datenbank.md"));
        Assert.Contains("# ADR-0001: Firestore als Datenbank", adr.Content);
        Assert.Contains("Status: accepted", adr.Content);
        Assert.Contains("Core-Item: ARCH-39", adr.Content);
        Assert.Contains("## Kontext", adr.Content);
        Assert.Contains("- REQ-70 — Firestore ist festgelegt", adr.Content);               // Split-Entscheid: verlinkt
        Assert.Contains(files, f => f.Path.EndsWith(Path.Combine("docs", "adr", "README.md")));
        Assert.Contains(files, f => f.Path.EndsWith(Path.Combine("docs", "architecture.md")));

        // Idempotenz: zweiter Apply mit demselben Entwurf prägt NICHTS neu (Item hat schon eine AdrId).
        var (again, files2, adrIds2) = AdrGate.Apply(updated, [Draft()], "docs/adr");
        Assert.Empty(adrIds2);
        Assert.Equal("ADR-0001", again.Items.Single(i => i.ItemId == "ARCH-39").Architecture!.AdrId);
        Assert.DoesNotContain(files2, f => f.Path.EndsWith("0002-firestore-als-datenbank.md"));
    }

    [Fact]
    public void Abloesung_zieht_den_ADR_Status_deterministisch_nach()
    {
        // ②-Bahn hat ARCH-39 abgelöst (ARCH-77 supersedes ARCH-39); ARCH-77 hat sein ADR schon.
        var core = Core(
            [Design("ARCH-39", adrId: "ADR-0001", adrStatus: AdrStatus.Accepted, status: "superseded"),
             Design("ARCH-77", "Lokale DB mit Sync", adrId: "ADR-0002", adrStatus: AdrStatus.Accepted)],
            [new("ARCH-77", "ARCH-39", "supersedes", "decision-tor2", new Dictionary<string, string>())]);

        var follow = Assert.Single(AdrProjection.StatusFollowUps(core));
        Assert.Equal(("ARCH-39", "ADR-0001", AdrStatus.Superseded, "ADR-0002"),
            (follow.ItemId, follow.AdrId, follow.NewStatus, follow.SupersededByAdrId));

        var (updated, files, _) = AdrGate.Apply(core, [], "docs/adr");
        Assert.Equal(AdrStatus.Superseded, updated.Items.Single(i => i.ItemId == "ARCH-39").Architecture!.AdrStatus);
        Assert.Contains("| ADR-0001 |", files.Single(f => f.Path.EndsWith("README.md")).Content);
    }

    [Fact]
    public void Uebersicht_gruppiert_nach_Rollen_mit_ADR_Link_und_Relationen()
    {
        var pbi = Item("PBI-1", "pbi", "Bewohnerprofil") with
        { Pbi = new PbiPayload(null, "Bewohnerprofil", [], [], [], null, null, null, null) };
        var work = Item("ARCH-22", "architecture", "Push-Infrastruktur") with { Architecture = new(["work"], "x") };
        var core = Core(
            [Design("ARCH-39", adrId: "ADR-0001", adrStatus: AdrStatus.Accepted), work, pbi],
            [new("PBI-1", "ARCH-39", "constrained_by", "t", new Dictionary<string, string>())]);

        var md = AdrProjection.RenderOverview(core);
        Assert.Contains("## Entscheidungen (design)", md);
        Assert.Contains("ADR: ADR-0001 (accepted)", md);
        Assert.Contains("bindet: PBI-1 (Bewohnerprofil)", md);
        Assert.Contains("noch nicht als PBI platziert", md);                               // work ohne covers
    }
}
