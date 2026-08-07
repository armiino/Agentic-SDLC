using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 ② E-R4 (06.08., §5.7): Cross-Aspekt-Widersprüche BEIDSEITIG — LESEN ja (Quer-Sicht-Tool),
// MATCHEN nein (CROSS_ASPECT_FORBIDDEN), NUR CONTRADICT quert (Ziel aktiv + Wahrheits-Aspekt) →
// DEC im EINEN Topf → Resolution prägt im ASPEKT des Ziels (R-39: supersedes aspekt-gleich) und
// zieht constrained_by deterministisch um (Autor-Entscheid: Umzug sofort, Weckruf/Angleichung = ③/E-8).
public sealed class CrossAspectContradictTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") =>
        new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, ["SL-1"], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Incoming(string id, string text, string type = "requirement") =>
        new ProjectStateItem(id, type, text, "MEETING", null, 1, "delta-run", null, null, null, null, ["claim-1"], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(ProjectStateItem[] items, ProjectStateRelation[]? relations = null)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], relations?.ToList() ?? [], [], []);

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", DateTime.UnixEpoch, "delta", [.. ops]);

    private static StateChangeOperation Op(string kind, string? target)
        => new("IN-1", kind, "Aussage", target, null, ["claim-1"], "test");

    // ---- Gate: die drei neuen Regeln ----

    [Fact]
    public void Contradict_darf_queren_wenn_das_Ziel_eine_AKTIVE_Wahrheit_des_anderen_Aspekts_ist()
    {
        var core = Doc([Item("ARCH-1", "architecture", "Firestore ist gesetzt")]);
        var delta = Doc([Incoming("IN-1", "Wir brauchen eine lokale Datenbank ohne Cloud")]);

        var report = IngestionGate.Check(delta, core, Plan(Op(StateChangeKind.Contradict, "ARCH-1")), AspectIngestionProfile.Requirement);

        Assert.True(report.Pass, string.Join(" | ", report.Errors.Select(e => e.Code + ":" + e.Message)));
    }

    [Fact]
    public void Contradict_gegen_inaktive_Wahrheit_wird_LAUT_abgewiesen()
    {
        var core = Doc([Item("ARCH-1", "architecture", "alt", status: "superseded")]);
        var delta = Doc([Incoming("IN-1", "Widerspruch")]);

        var report = IngestionGate.Check(delta, core, Plan(Op(StateChangeKind.Contradict, "ARCH-1")), AspectIngestionProfile.Requirement);

        Assert.Contains(report.Errors, e => e.Code == "CONTRADICT_TARGET_INACTIVE");
    }

    [Fact]
    public void Matchen_bleibt_strikt_eigen_aspektig_Quer_Ziel_ergibt_CROSS_ASPECT_FORBIDDEN()
    {
        var core = Doc([Item("ARCH-1", "architecture", "Rahmen")]);
        var delta = Doc([Incoming("IN-1", "Verfeinerung")]);

        var report = IngestionGate.Check(delta, core, Plan(Op(StateChangeKind.Refine, "ARCH-1")), AspectIngestionProfile.Requirement);

        var err = Assert.Single(report.Errors, e => e.Code == "CROSS_ASPECT_FORBIDDEN");
        Assert.Equal(Repairability.Repairable, err.Repairability);         // R-33: der Repair-Loop kann das heilen
    }

    // ---- Quer-Lese-Tool: beide Profile sehen die Wahrheit des jeweils anderen Aspekts ----

    [Fact]
    public void Beide_Resolver_tragen_das_Quer_Lese_Tool_des_anderen_Wahrheits_Aspekts()
    {
        var run = new RunContext(RunId.New(), "test-cross"); run.EnsureFolders();
        var core = Doc([Item("REQ-1", "requirement", "req"), Item("ARCH-1", "architecture", "arch")]);
        var delta = Doc([Incoming("IN-1", "x")]);
        var retriever = new ShowAllRequirementRetriever();

        string[] NamesFor(AspectIngestionProfile p) =>
            new IngestionTools(delta, core, retriever, run, p).Build().OfType<AIFunction>().Select(f => f.Name).ToArray();

        Assert.Contains("list_core_architecture", NamesFor(AspectIngestionProfile.Requirement));
        Assert.Contains("list_core_requirements", NamesFor(AspectIngestionProfile.Architecture));
    }

    // ---- Apply: Cross-CONTRADICT münzt das DEC in den EINEN Topf (aspekt-agnostisch) ----

    [Fact]
    public void Cross_Contradict_muenzt_ein_DEC_mit_contradicts_Relation_auf_das_arch_Ziel()
    {
        var core = Doc([Item("ARCH-1", "architecture", "Firestore ist gesetzt")]);
        var delta = Doc([Incoming("IN-1", "lokale Datenbank ohne Cloud")]);

        var (updated, report, _) = IngestionApply.Apply(core, delta, Plan(Op(StateChangeKind.Contradict, "ARCH-1")),
            new HashSet<string>(StringComparer.Ordinal) { "IN-1" }, "ingest-run");

        var dec = updated.Items.Single(i => i.ItemType == "decision");
        Assert.True(dec.ReadStatus().IsOpenDecision);
        Assert.Equal("ARCH-1", dec.Metadata["targetEntityId"]);
        var rel = Assert.Single(updated.Relations, r => r.RelationType == "contradicts");
        Assert.Equal((dec.ItemId, "ARCH-1"), (rel.FromId, rel.ToId));
        Assert.Single(report.Applied);
    }

    // ---- Resolution: Derivation + Apply auf ein arch-Ziel (der ②-Kern-Beweis) ----

    private static ProjectStateDocument CoreWithArchConflict()
    {
        var pbi = Item("PBI-1", "pbi", "PBI") with
        {
            Pbi = new PbiPayload(null, "PBI Titel", [], [], ["DEC-001"], null, null, null, null)
        };
        return Doc(
        [
            Item("ARCH-01", "architecture", "Firestore ist gesetzt"),
            Item("DEC-001", "decision", "Widerspruch zu ARCH-01: lokale DB", status: "open_decision"),
            pbi.WithStatus(CoreStatus.From("blocked_by_decision"))
        ],
        [
            new("DEC-001", "ARCH-01", "contradicts", "ingestion", new Dictionary<string, string>()),
            new("PBI-1", "ARCH-01", "constrained_by", "arch-classify", new Dictionary<string, string>())
        ]);
    }

    [Fact]
    public void Derivation_zaehlt_constrained_by_PBIs_zu_den_Betroffenen_eines_arch_Ziels()
    {
        var (ops, problems) = DecisionResolutionDerivation.Derive(CoreWithArchConflict(),
            new DecisionResolutionInput([new("DEC-001", DecisionOutcome.AdoptNew, "Lokale DB mit Sync", null)]));

        Assert.Empty(problems);
        var op = Assert.Single(ops);
        Assert.Equal("ARCH-01", op.TargetRequirementId);
        Assert.Contains("PBI-1", op.AffectedPbis);
    }

    [Fact]
    public void AdoptNew_auf_arch_Ziel_praegt_ARCH_Item_und_zieht_constrained_by_um_Kangal_gruen()
    {
        var core = CoreWithArchConflict();
        var (ops, _) = DecisionResolutionDerivation.Derive(core,
            new DecisionResolutionInput([new("DEC-001", DecisionOutcome.AdoptNew, "Lokale DB mit Sync", null)]));
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "p1", DateTime.UnixEpoch, ops);

        var (updated, report) = DecisionResolutionApply.Apply(core, plan, new HashSet<int> { 0 }, "res-run");

        // Neu-Item im ASPEKT des Ziels (nicht REQ!), supersedes aspekt-gleich (R-39-konform).
        var newId = Assert.Single(report.NewRequirements);
        var neu = updated.Items.Single(i => i.ItemId == newId);
        Assert.Equal("architecture", neu.ItemType);
        Assert.StartsWith("ARCH-", newId);
        Assert.Equal(Validity.Superseded, updated.Items.Single(i => i.ItemId == "ARCH-01").ReadStatus().Validity);
        var sup = Assert.Single(updated.Relations, r => r.RelationType == "supersedes");
        Assert.Equal((newId, "ARCH-01"), (sup.FromId, sup.ToId));

        // constrained_by-UMZUG: alte Kante = Historie-Form mit movedTo, neue lebende Kante aufs Neu-ARCH.
        var hist = Assert.Single(updated.Relations, r => r.RelationType == "constrained_by_superseded");
        Assert.Equal(("PBI-1", "ARCH-01", newId), (hist.FromId, hist.ToId, hist.Metadata["movedTo"]));
        var live = Assert.Single(updated.Relations, r => r.RelationType == "constrained_by");
        Assert.Equal(("PBI-1", newId), (live.FromId, live.ToId));

        // PBI entblockt mit Weckruf-Status (Angleichung = ③/E-8), DEC resolved, Kangal akzeptiert den Endstand.
        Assert.Equal(Blocker.NeedsClarify, updated.Items.Single(i => i.ItemId == "PBI-1").ReadStatus().Blocker);
        Assert.False(updated.Items.Single(i => i.ItemId == "DEC-001").ReadStatus().IsOpenDecision);
        var kangal = CoreKangal.Check(updated);
        Assert.DoesNotContain(kangal.Errors, e => e.Code.StartsWith("I1_"));
        Assert.DoesNotContain(kangal.Warnings, w => w.Code == "UNKNOWN_RELATION_TYPE");
    }
}
