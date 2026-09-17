using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-14 G1-a: Tor 2 als operativer Schritt zwischen Ingest-Apply und Pbi-Bridge. Getestet wird die PURE Logik
// (DecisionStage) + die Kollegen-Merge-Semantik (04.08.) + das selbsttragende Zusammenspiel mit der
// PbiUpdateDerivation (frischer Core: Flip => kein BLOCK; synthetische Ops => MARK_CHANGED/SUPERSEDE_PBI).
public sealed class DecisionStageTests
{
    // Kanal-Ehrlichkeit (Autor-Fund 20./21.08.): CONTRADICT-DECs nennen ihren ECHTEN Kanal —
    // Analyst-erschlossen bzw. GitHub-geerntet, nicht pauschal „Meeting".
    [Fact]
    public void OriginOf_nennt_den_echten_Kanal_bei_Analyst_und_Github_Contradicts()
    {
        ProjectStateItem Dec(Dictionary<string, string> meta) => new ProjectStateItem(
            "DEC-9", "decision", "Widerspruch zu REQ-1: …", "INGESTION_CONTRADICTION", null, 1,
            "run-1", null, null, null, null, [], [], meta).WithStatus(CoreStatus.From("open_decision"));

        var analyst = DecisionStage.OriginOf(Dec(new(StringComparer.Ordinal)
        { [AnalystOriginMeta.Linse] = "arch", ["ingestedFrom"] = "CA-1" }));
        Assert.Contains("vom Core-Analysten ERSCHLOSSEN", analyst);
        Assert.Contains("Analyst-Fund CA-1", analyst);
        Assert.DoesNotContain("Meeting", analyst);

        var github = DecisionStage.OriginOf(Dec(new(StringComparer.Ordinal)
        { [GithubOriginMeta.IssueNumber] = "45", ["ingestedFrom"] = "GH-45" }));
        Assert.Contains("aus der GitHub-Ernte", github);
        Assert.Contains("GitHub-Item GH-45", github);

        var meeting = DecisionStage.OriginOf(Dec(new(StringComparer.Ordinal) { ["ingestedFrom"] = "M1-REQ-1" }));
        Assert.Contains("im Meeting", meeting);
        Assert.Contains("Meeting-Item M1-REQ-1", meeting);
    }

    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Item(string id, string type, string status, string text = "") => new ProjectStateItem(
        id, type, text.Length > 0 ? text : $"{id} Text", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Pbi(string id, string status, params string[] decRefs) => new ProjectStateItem(
        id, "pbi", $"{id} Titel", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: null, Title: $"{id} Titel", AcceptanceCriteria: [], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: decRefs, PriorityRank: null, Readiness: status, Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static ProjectStateRelation Rel(string from, string to, string type)
        => new(from, to, type, "test", new Dictionary<string, string>());

    private static ProjectStateDocument Core(IReadOnlyList<ProjectStateRelation> relations, params ProjectStateItem[] items)
        => new("p", 4, T, [], [.. items], relations, [], []);

    // Der Standard-Fall: offene DEC (Widerspruch zu REQ-1) + blockiertes PBI, das REQ-1 deckt.
    private static ProjectStateDocument ConflictCore() => Core(
        [Rel("DEC-001", "REQ-1", "contradicts"), Rel("PBI-1", "REQ-1", "covers"), Rel("PBI-1", "FC-01", "part_of_feature")],
        Item("FC-01", "feature", "active"),
        Item("REQ-1", "requirement", "accepted", "Archivierung nach vierzehn Tagen"),
        Item("DEC-001", "decision", "open_decision", "Widerspruch zu REQ-1: Archivierung nach sieben Tagen"),
        Pbi("PBI-1", "blocked_by_decision", "DEC-001"));

    private static IngestionApplyReport Original(params AppliedOperation[] ops)
        => new([.. ops], [], new IngestionDeltaSummary(0, 0, 0, 0, ops.Length, 0, 0));

    [Fact]
    public void Scan_findet_offene_DECs_mit_vollem_Kontext_auch_geparkte()
    {
        var views = DecisionStage.BuildViews(ConflictCore());
        var v = Assert.Single(views);
        Assert.Equal("DEC-001", v.DecisionId);
        Assert.Equal("REQ-1", v.TargetRequirementId);
        Assert.Equal("Archivierung nach vierzehn Tagen", v.TargetRequirementText);
        Assert.Equal(["PBI-1"], v.BlockedPbis);
        Assert.Equal("Archivierung nach sieben Tagen", v.ProposedStatement);   // Prefill aus dem DEC-Text
    }

    [Fact]
    public void Scan_ignoriert_aufgeloeste_DECs()
    {
        var core = Core([], Item("DEC-001", "decision", "resolved"));
        Assert.Empty(DecisionStage.BuildViews(core));
    }

    [Fact]
    public void DeferAll_vertagt_jede_Entscheidung()
    {
        var req = new PipelineDecisionReviewRequest("r1", DecisionStage.BuildViews(ConflictCore()));
        var resp = DecisionStage.DeferAll(req, "test");
        Assert.All(resp.Resolutions, r => Assert.Equal(DecisionStage.ActionDefer, r.Action));
        Assert.Empty(DecisionStage.ToInput(resp.Resolutions).Resolutions);   // defer erzeugt KEINE Resolutions
    }

    // ---- Die Kollegen-Merge-Semantik (04.08.), Fall fuer Fall — als voller purer Durchstich:
    //      Views -> Resolution -> Derive -> Gate -> Apply -> MergeReport.

    private static (ProjectStateDocument Core, IngestionApplyReport Merged) ResolveRound(string outcome, string? newStatement)
    {
        var core = ConflictCore();
        var input = DecisionStage.ToInput([new PipelineDecisionResolution("DEC-001", DecisionStage.ActionResolve, outcome, newStatement, "Team-Entscheid")]);
        var (ops, problems) = DecisionResolutionDerivation.Derive(core, input);
        Assert.Empty(problems);
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion, "plan-1", T, ops);
        Assert.True(DecisionResolutionGate.Check(core, plan).Pass);
        var (updated, applied) = DecisionResolutionApply.Apply(core, plan, Enumerable.Range(0, ops.Count).ToHashSet(), "run-x");
        var merged = DecisionStage.MergeReport(Original(new AppliedOperation("IN-1", StateChangeKind.Contradict, "DEC-001", "contradicted")), plan, applied);
        return (updated, merged);
    }

    [Fact]
    public void Keep_Original_DEC_resolved_PBI_entblockt_KEIN_neues_Delta()
    {
        var (core, merged) = ResolveRound(DecisionOutcome.KeepOriginal, null);

        Assert.False(core.Items.First(i => i.ItemId == "DEC-001").ReadStatus().IsOpenDecision);
        Assert.Equal("active", core.Items.First(i => i.ItemId == "PBI-1").Status);          // entblockt, Wahrheit blieb
        Assert.Contains(core.Relations, r => r.RelationType == "contradicts_resolved");     // Flip, nicht geloescht
        Assert.Single(merged.Applied);                                                       // KEIN synthetisches Delta
    }

    [Fact]
    public void Refine_synthetisches_REFINE_Delta_aufs_Ziel_Requirement()
    {
        var (core, merged) = ResolveRound(DecisionOutcome.Refine, "Archivierung nach vierzehn Tagen, DSGVO-konform");

        var req = core.Items.First(i => i.ItemId == "REQ-1");
        Assert.Equal(2, req.Version);                                                        // v+1, Alt in History
        var synth = Assert.Single(merged.Applied, a => a.Kind == StateChangeKind.Refine);
        Assert.Equal("REQ-1", synth.EntityId);                                               // Kollegen-Fall 2
    }

    [Fact]
    public void AdoptNew_Swap_ist_schon_erledigt_synthetisches_REFINE_aufs_NEUE_Requirement()
    {
        // Korrektur der Skizze (am Code bewiesen): der Tor-2-Apply SWAPPT die Deckung selbst — das PBI deckt
        // bereits das neue REQ. Uebrig bleibt die INHALTS-Angleichung => REFINE-foermiges Delta aufs neue REQ.
        var (core, merged) = ResolveRound(DecisionOutcome.AdoptNew, "Archivierung nach sieben Tagen");

        Assert.Equal("superseded", core.Items.First(i => i.ItemId == "REQ-1").Status);
        var newReq = core.Items.First(i => i.ItemType == "requirement" && i.ItemId != "REQ-1");
        Assert.Contains(core.Relations, r => r.FromId == "PBI-1" && r.ToId == newReq.ItemId && r.RelationType == "covers");   // Swap war schon da
        var synth = Assert.Single(merged.Applied, a => a.Kind == StateChangeKind.Refine);
        Assert.Equal(newReq.ItemId, synth.EntityId);
        Assert.Equal("adopted_new", synth.Outcome);
    }

    // ---- Das selbsttragende Zusammenspiel mit der Placement-Derivation (frischer Core).

    [Fact]
    public void Nach_Aufloesung_leitet_Derivation_KEIN_BLOCK_mehr_ab()
    {
        var (core, merged) = ResolveRound(DecisionOutcome.KeepOriginal, null);
        var result = PbiUpdateDerivation.Derive(core, merged.Applied);
        Assert.DoesNotContain(result.DeterministicOps, o => o.Kind == PbiUpdateKind.BlockPbi);   // Flip wirkt
    }

    [Fact]
    public void Synthetisches_REFINE_ergibt_MARK_CHANGED_am_deckenden_PBI()
    {
        var (core, merged) = ResolveRound(DecisionOutcome.Refine, "Neuer Text");
        var result = PbiUpdateDerivation.Derive(core, merged.Applied);
        var op = Assert.Single(result.DeterministicOps, o => o.Kind == PbiUpdateKind.MarkChanged);
        Assert.Equal("PBI-1", op.PbiId);                                                     // Align-Ziel im selben Lauf
    }

    [Fact]
    public void AdoptNew_ergibt_MARK_CHANGED_am_bereits_geswappten_PBI()
    {
        var (core, merged) = ResolveRound(DecisionOutcome.AdoptNew, "Sieben Tage");
        var result = PbiUpdateDerivation.Derive(core, merged.Applied);
        var op = Assert.Single(result.DeterministicOps, o => o.Kind == PbiUpdateKind.MarkChanged);
        Assert.Equal("PBI-1", op.PbiId);                                                     // Align-Ziel: Inhalt ans NEUE REQ
        Assert.DoesNotContain(result.DeterministicOps, o => o.Kind == PbiUpdateKind.SupersedePbi);   // Swap war schon erledigt
    }
}
