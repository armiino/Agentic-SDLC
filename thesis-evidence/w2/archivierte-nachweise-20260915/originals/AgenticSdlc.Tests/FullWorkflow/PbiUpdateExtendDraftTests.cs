using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// O3a (extend-Modus): EXTEND_PBI traegt jetzt einen Draft. PbiAlignTargets.Collect sammelt es als Ziel (ohne
// alte Fassung); der bestehende Apply-Uebernahme-Code (R-26-C) uebernimmt Title/Goal/AK und klaert
// needs_clarify -> active. Ohne akzeptierten Draft bleibt es ehrlich needs_clarify (nur der Link kommt hinzu).
public sealed class PbiUpdateExtendDraftTests
{
    private static ProjectStateItem Pbi(string id, string title, string status = "needs_clarify") => new ProjectStateItem(
        id, "pbi", title, "test", null, 3,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "altes Ziel", Title: title, AcceptanceCriteria: ["alt AK"], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "needs_clarify", Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation Extend(string pbi, string req)
        => new("EXTEND_PBI", req, pbi, null, null, null, $"{req} kommt hinzu");

    private static PbiStateChangePlanDocument Plan(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment>? aligns = null)
        => new(1, "p1", DateTime.UnixEpoch, "extend-run", ops, aligns);

    [Fact]
    public void Collect_sammelt_ExtendPbi_als_Draft_Ziel_ohne_alte_Fassung()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"), Req("REQ-2", "PDF-Export der Daten"));
        var targets = PbiAlignTargets.Collect(Plan([Extend("PBI-1", "REQ-2")]), core);

        var t = Assert.Single(targets);
        Assert.Equal("PBI-1", t.PbiId);
        var trig = Assert.Single(t.Triggers);
        Assert.Equal("REQ-2", trig.RequirementId);
        Assert.Equal("PDF-Export der Daten", trig.NewText);
        Assert.Null(trig.OldText);   // extend: keine alte Fassung (im Gegensatz zu MARK_CHANGED/SUPERSEDE)
    }

    [Fact]
    public void Akzeptierter_Extend_Draft_erweitert_PBI_und_wird_active()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"), Req("REQ-2", "PDF-Export"));
        var align = new PbiAlignment("PBI-1", "Titel inkl. Export", "Als X will ich … plus Export",
            ["alt AK", "AK: Export als PDF"], "um REQ-2 (Export) erweitert", ["REQ-2"]);
        var plan = Plan([Extend("PBI-1", "REQ-2")], [align]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "extend-run", [align]);
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("active", pbi.Status);                       // needs_clarify -> active
        Assert.Equal("Titel inkl. Export", pbi.Pbi!.Title);
        Assert.Equal(["alt AK", "AK: Export als PDF"], pbi.Pbi.AcceptanceCriteria);
        Assert.Contains("REQ-2", pbi.Pbi.LinkedRequirementIds);   // extend haengt die neue Anforderung an
        Assert.Equal("extend-run", pbi.SourceRunId);              // R-31: neue Fassung, neuer Ursprung
        Assert.Contains("PBI-1", report.UpdatedPbis);
    }

    [Fact]
    public void Extend_ohne_Draft_bleibt_ehrlich_needs_clarify()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"), Req("REQ-2", "PDF-Export"));
        var plan = Plan([Extend("PBI-1", "REQ-2")]);   // keine Alignments

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "extend-run");
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("needs_clarify", pbi.Status);                // ehrlich ungeklaert
        Assert.Contains("REQ-2", pbi.Pbi!.LinkedRequirementIds);  // Link trotzdem hinzugefuegt (Struktur)
        Assert.Equal("Alter Titel", pbi.Pbi.Title);               // Inhalt unberuehrt
        Assert.Equal("baseline-run", pbi.SourceRunId);            // reine Struktur-Op erbt keinen neuen Ursprung
    }
}
