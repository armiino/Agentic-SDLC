using AgenticSdlc.Host.FullWorkflow.ArchClassify;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A2-1 (05.08.): der deterministische Kern der Rollen-Klassifikation — Unclassified-Scan (quell-agnostisch,
// idempotent), Gate (GateLoop-Form, Coverage/Rollen/Beleg), Apply (typisiertes Payload, nur berührte Items).
public sealed class ArchClassifyGateTests
{
    private static ProjectStateItem Item(string id, string type = "architecture", string status = "accepted") => new ProjectStateItem(
        id, type, $"Text {id}", "MEETING", null, 1,
        "r", null, null, null, null, ["SL-1"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static ArchClassifyProposal P(string id, string[] roles, string rationale = "weil", string[]? targets = null)
        => new(id, roles, rationale, targets);

    [Fact]
    public void Unclassified_nimmt_nur_aktive_arch_Items_ohne_Payload()
    {
        var classified = Item("ARCH-1") with { Architecture = new ArchitecturePayload([ArchRoles.Design], "r") };
        var superseded = Item("ARCH-2", status: "superseded");
        var req = Item("REQ-1", type: "requirement");
        var open_ = Item("ARCH-3");

        var u = ArchClassifyGate.Unclassified(Doc(classified, superseded, req, open_));

        Assert.Equal(["ARCH-3"], u.Select(i => i.ItemId));              // Idempotenz + Aspekt- + Aktiv-Filter
    }

    [Fact]
    public void Gate_prueft_Coverage_Rollen_und_Beleg()
    {
        var u = ArchClassifyGate.Unclassified(Doc(Item("ARCH-1"), Item("ARCH-2")));

        var pbis = new HashSet<string>(StringComparer.Ordinal) { "PBI-1" };
        var ok = ArchClassifyGate.Check(u, [P("ARCH-1", [ArchRoles.Design, ArchRoles.Constraint], targets: ["PBI-1"]), P("ARCH-2", [ArchRoles.Work])], pbis);
        Assert.True(ok.Pass, string.Join(";", ok.Errors.Select(e => e.Code)));   // Rollen KOMPONIEREN (T2.14); constraint mit Ziel

        // ① Ziel-Regeln: constraint OHNE Ziele + unbekanntes Ziel-PBI = laute, reparierbare Fehler.
        var badTargets = ArchClassifyGate.Check(u, [P("ARCH-1", [ArchRoles.Constraint]), P("ARCH-2", [ArchRoles.Constraint], targets: ["PBI-99"])], pbis);
        Assert.Contains(badTargets.Errors, e => e.Code == "CONSTRAINT_WITHOUT_TARGETS" && e.ItemId == "ARCH-1");
        Assert.Contains(badTargets.Errors, e => e.Code == "UNKNOWN_TARGET_PBI" && e.ItemId == "ARCH-2");

        var bad = ArchClassifyGate.Check(u, [P("ARCH-1", []), P("ARCH-9", ["banane"]), P("ARCH-2", [ArchRoles.Work], rationale: " ")]);
        Assert.False(bad.Pass);
        Assert.Contains(bad.Errors, e => e.Code == "NO_ROLE" && e.ItemId == "ARCH-1");
        Assert.Contains(bad.Errors, e => e.Code == "UNKNOWN_TARGET" && e.ItemId == "ARCH-9");
        Assert.Contains(bad.Errors, e => e.Code == "INVALID_ROLE");
        Assert.Contains(bad.Errors, e => e.Code == "MISSING_RATIONALE" && e.ItemId == "ARCH-2");
        Assert.True(bad.HasRepairable);                                  // alles agentisch reparierbar (GateLoop)
    }

    [Fact]
    public void Apply_schreibt_Payload_nur_auf_berührte_und_ist_idempotent()
    {
        var core = Doc(Item("ARCH-1"), Item("ARCH-2"), Item("REQ-1", type: "requirement"));

        var once = ArchClassifyGate.Apply(core, [P("ARCH-1", [ArchRoles.Design, ArchRoles.Design])]); // Dup wird dedupliziert
        var a1 = once.Items.Single(i => i.ItemId == "ARCH-1");
        Assert.Equal([ArchRoles.Design], a1.Architecture!.Roles);
        Assert.Null(once.Items.Single(i => i.ItemId == "ARCH-2").Architecture);
        Assert.Null(once.Items.Single(i => i.ItemId == "REQ-1").Architecture);

        // zweimal = gleicher Core (bereits Klassifiziertes wird nicht überschrieben; Scan liefert es nicht mehr).
        var twice = ArchClassifyGate.Apply(once, [P("ARCH-1", [ArchRoles.Work])]);
        Assert.Equal([ArchRoles.Design], twice.Items.Single(i => i.ItemId == "ARCH-1").Architecture!.Roles);
        Assert.DoesNotContain(ArchClassifyGate.Unclassified(twice), i => i.ItemId == "ARCH-1");
    }

    [Fact]
    public void Apply_schreibt_constrained_by_nur_bei_constraint_Rolle_idempotent_und_Kangal_gruen()
    {
        var pbi = Item("PBI-1", type: "pbi");
        var core = Doc(Item("ARCH-1"), Item("ARCH-2"), pbi);

        var applied = ArchClassifyGate.Apply(core,
        [
            P("ARCH-1", [ArchRoles.Constraint, ArchRoles.Design], targets: ["PBI-1", "PBI-99"]),   // PBI-99 wird defensiv gefiltert
            P("ARCH-2", [ArchRoles.Work], targets: ["PBI-1"])                                       // work-only: KEINE Relation (Anzeige-Kontext)
        ]);

        var rel = Assert.Single(applied.Relations);
        Assert.Equal(("PBI-1", "ARCH-1", "constrained_by"), (rel.FromId, rel.ToId, rel.RelationType));
        // Spec/Wächter-Kopplung: der Kangal akzeptiert die neue Relation (pbi -> architecture).
        var kangal = AgenticSdlc.Host.FullWorkflow.Core.CoreKangal.Check(applied);
        Assert.DoesNotContain(kangal.Errors, e => e.Code.StartsWith("I1_"));

        // Idempotenz: erneuter Apply derselben Bestätigung erzeugt KEINE Doppel-Relation.
        var again = ArchClassifyGate.Apply(applied, [P("ARCH-1", [ArchRoles.Constraint], targets: ["PBI-1"])]);
        Assert.Single(again.Relations);
    }
}
