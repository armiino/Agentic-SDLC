using System.Collections.Concurrent;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Schritt 5 ④ (05.08.): der Kanonisierungs-Loop in R-33-Form — deterministischer CanonicalCheck,
// Verdict-typisierte Kanten, Repair mit wörtlichem Gate-Befund, Loop-back, MaxAttempts-Schranke.
// LLM-frei: der Repairer ist ein deterministischer Fake an der ICanonicalCoverageRepairer-Naht;
// die Loop-MECHANIK läuft auf dem ECHTEN Mini-Graphen in-process (Lehre R-37: Build()-bar beweist nichts).
public sealed class LedgerCanonicalLoopTests
{
    private static readonly IReadOnlyDictionary<string, ArtifactDisposition> ValidDisposition =
        new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new("required", "requirement"),
            ["architecture"] = new("not_applicable", "consciously_omitted"),
            ["risks"] = new("not_applicable", "consciously_omitted"),
            ["open-questions"] = new("not_applicable", "consciously_omitted")
        };

    private static SemanticLedgerEntry Entry(string id, IReadOnlyList<string>? candidateIds = null, string relation = "standalone",
        IReadOnlyList<SemanticLedgerEvidence>? evidence = null)
        => new(id, $"Aussage {id}", "requirement", "required", "must", "global", "mvp",
            evidence ?? [new SemanticLedgerEvidence("t.txt", $"Zitat {id}")], ValidDisposition, "low", null,
            CandidateIds: candidateIds, AssumedRelation: candidateIds is null ? null : relation);

    // ---- Pure Checker-Teilmenge (geteilte Quelle mit dem End-Audit) ----

    [Fact]
    public void EvaluateCanonicalStage_findet_verlorenen_Candidate_und_besteht_bei_voller_Deckung()
    {
        var candidates = new[] { Entry("SL-001"), Entry("SL-002") };

        var lückenhaft = LedgerQualityGate.EvaluateCanonicalStage(candidates, [Entry("C-1", ["SL-001"])]);
        Assert.False(lückenhaft.Pass);
        Assert.Contains(lückenhaft.Violations, v => v.Code == "CANDIDATE_SILENTLY_DROPPED" && v.Ids.Contains("SL-002"));

        var gedeckt = LedgerQualityGate.EvaluateCanonicalStage(candidates,
            [Entry("C-1", ["SL-001"]), Entry("C-2", ["SL-002"])]);
        Assert.True(gedeckt.Pass);
    }

    // ---- Loop-Mechanik am echten Mini-Graphen ----

    private sealed class FakeRepairer(Func<IReadOnlyList<string>, IReadOnlyList<SemanticLedgerEntry>, IReadOnlyList<SemanticLedgerEntry>> repair)
        : ICanonicalCoverageRepairer
    {
        public int Calls;
        public Task<IReadOnlyList<SemanticLedgerEntry>> RepairAsync(
            IReadOnlyList<SemanticLedgerEntry> candidates, IReadOnlyList<SemanticLedgerEntry> canonicalDraft,
            IReadOnlyList<string> missingCandidateIds, CancellationToken ct)
        { Calls++; return Task.FromResult(repair(missingCandidateIds, canonicalDraft)); }
    }

    [YieldsOutput(typeof(string))]
    private sealed class FakeFacetReceiver(ConcurrentBag<int> hits) : Executor<CanonicalLedgerMessage>("FakeFacet")
    {
        public override ValueTask HandleAsync(CanonicalLedgerMessage m, IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(m.Entries.Count); return ValueTask.CompletedTask; }
    }

    private static async Task<(IReadOnlyList<object> Outputs, ConcurrentBag<int> FacetHits, IReadOnlyList<string> Failures, RunContext Run)>
        RunLoopAsync(FakeRepairer repairer, CandidateAndCanonicalLedgerMessage input)
    {
        var run = new RunContext(RunId.New(), "test-canonical-loop"); run.EnsureFolders();
        var hits = new ConcurrentBag<int>();
        var check = new CanonicalCheckExecutor(run, maxAttempts: 2);
        var repair = new CanonicalCoverageRepairExecutor(repairer, run);
        var facet = new FakeFacetReceiver(hits);

        var b = new WorkflowBuilder(check).WithName("CanonicalLoopTest");
        b.AddEdge<CanonicalGateVerdict>(check, repair, m => m is not null && m.Decision == GateDecision.Repair);
        b.AddEdge(repair, check);
        b.AddEdge(check, facet);
        b.WithOutputFrom(check);

        var wfRun = await InProcessExecution.Default.RunAsync(b.Build(), input, run.RunId, CancellationToken.None);
        var outputs = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data!).ToList();
        var failures = wfRun.OutgoingEvents
            .Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e switch
            {
                ExecutorFailedEvent f => $"{f.ExecutorId}: {f.Data?.Message}",
                WorkflowErrorEvent w => w.Exception?.Message ?? "WorkflowError",
                _ => ""
            }).ToList();
        return (outputs, hits, failures, run);
    }

    [Fact]
    public async Task Repair_schliesst_die_Luecke_und_der_zweite_Versuch_besteht()
    {
        var candidates = new[] { Entry("SL-001"), Entry("SL-002") };
        var draft = new[] { Entry("C-1", ["SL-001"]) };                        // SL-002 still verloren
        var repairer = new FakeRepairer((missing, canonical) =>
            [.. canonical, Entry("C-2", [.. missing])]);                       // Fake schließt exakt die Lücke

        var (outputs, facetHits, failures, run) = await RunLoopAsync(repairer, new(candidates, draft));

        Assert.Empty(failures);
        Assert.Equal(1, repairer.Calls);                                       // genau EIN Repair-Feuer
        Assert.Equal(2, Assert.Single(facetHits));                             // Pass beim 2. Versuch → Facet bekam 2 Einträge
        Assert.Empty(outputs);                                                 // kein Terminal-Fehler-Yield
        // W2-Messdraht: 2 Attempts protokolliert (maker→Repair, repair→Pass).
        var gate = await File.ReadAllTextAsync(Path.Combine(run.RunDir, "gate", "canonical-gate.json"));
        Assert.Contains("\"attemptNumber\": 2", gate);
        Assert.Contains("\"source\": \"repair\"", gate);
        Assert.True(File.Exists(Path.Combine(run.RunDir, "step-02-canonical", "output.json")));
    }

    [Fact]
    public async Task Wirkungsloser_Repair_endet_LAUT_terminal_nach_MaxAttempts()
    {
        var candidates = new[] { Entry("SL-001"), Entry("SL-002") };
        var draft = new[] { Entry("C-1", ["SL-001"]) };
        var repairer = new FakeRepairer((_, canonical) => canonical);          // Repair bewirkt NICHTS

        var (outputs, facetHits, failures, run) = await RunLoopAsync(repairer, new(candidates, draft));

        Assert.Empty(failures);
        Assert.Equal(1, repairer.Calls);                                       // maxAttempts=2: 1 Repair, dann Schranke
        Assert.Empty(facetHits);                                               // Facet-Stufe NIE erreicht (kein stilles Weiterfließen)
        var terminal = Assert.Single(outputs.OfType<string>());
        Assert.Contains("MaxAttemptsReached", terminal);
        Assert.True(File.Exists(Path.Combine(run.RunDir, "step-02-canonical", "output.json")));   // Audit-Draft trotzdem da
    }

    [Fact]
    public async Task Nicht_reparierbarer_Fehler_geht_ohne_Repair_Call_zum_Menschen()
    {
        // Verstoß OHNE Coverage-Lücke (Eintrag ohne Evidence) — Repairability greift nicht: HumanReview sofort.
        var candidates = new[] { Entry("SL-001") };
        var draft = new[] { Entry("C-1", ["SL-001"], evidence: []) };
        var repairer = new FakeRepairer((_, canonical) => canonical);

        var (outputs, facetHits, failures, _) = await RunLoopAsync(repairer, new(candidates, draft));

        Assert.Empty(failures);
        Assert.Equal(0, repairer.Calls);                                       // kein sinnloses LLM-Feuer
        Assert.Empty(facetHits);
        Assert.Contains("HumanReview", Assert.Single(outputs.OfType<string>()));
    }

    [Fact]
    public async Task Sauberer_Draft_passiert_im_ersten_Versuch_unveraendert()
    {
        var candidates = new[] { Entry("SL-001") };
        var draft = new[] { Entry("C-1", ["SL-001"]) };
        var repairer = new FakeRepairer((_, canonical) => canonical);

        var (outputs, facetHits, failures, run) = await RunLoopAsync(repairer, new(candidates, draft));

        Assert.Empty(failures);
        Assert.Equal(0, repairer.Calls);
        Assert.Equal(1, Assert.Single(facetHits));                             // Pass-Pfad wie bisher
        Assert.Empty(outputs);
        Assert.Contains("\"gatePass\": true", await File.ReadAllTextAsync(Path.Combine(run.RunDir, "gate", "canonical-gate.json")));
    }
}
