using System.Collections.Concurrent;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Branch;
using AgenticSdlc.Host.FullWorkflow.FanOut;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-37 (MAF-native Form): der ECHTE Fan-out-Graph (Dispatch → prädikat-geroutete BranchSource-Kanten → Barrier →
// Collector) läuft hier in-process mit Fake-Zweigen — LLM-frei. Getestet wird die PRODUKTIONS-Verdrahtung:
// jeder Zweig bekommt GENAU EINE, MIT SEINEM Dispositions-Schlüssel projizierte Quelle (inkl. not_applicable-
// Filter), und der Collector sammelt alle Zweig-Outputs zum VerifiedBaselineSet ein.
public sealed class BaselineFanOutRoutingTests
{
    [YieldsOutput(typeof(ArtifactDocument))]
    private sealed class FakeBranchExecutor : Executor<BranchSource>
    {
        private readonly string _type;
        private readonly ConcurrentDictionary<string, List<string>> _received;

        public FakeBranchExecutor(string type, ConcurrentDictionary<string, List<string>> received)
            : base($"FakeBranch-{type}")
        {
            _type = type;
            _received = received;
        }

        public override async ValueTask HandleAsync(BranchSource source, IWorkflowContext context, CancellationToken ct = default)
        {
            _received.GetOrAdd(_type, _ => []).Add($"{source.ArtifactType}{source.SourceBlock}");
            // Wie der echte Zweig-Endpunkt (ArtifactAssignIdsExecutor): YieldOutput — DAS forwarded der gebundene
            // Subworkflow an die Fan-in-Barrier (SendMessage bliebe im Zweig haengen).
            await context.YieldOutputAsync(new ArtifactDocument(
                ArtifactId: _type.ToUpperInvariant(), ArtifactType: _type, Version: 1,
                Stage: ArtifactDocument.StageEvidenceBaseline,
                Producer: new ProducerMetadata("test-run", "fake", null), Items: [])).ConfigureAwait(false);
        }
    }

    private static SemanticLedgerEntry Claim(string id, string reqApplicability, string oqApplicability) => new(
        Id: id, Proposition: $"{id} Aussage", Kind: "requirement", Status: "open", Modality: "must",
        Scope: "pflege", TimeScope: null, Evidence: [new SemanticLedgerEvidence("transcript", "Zitat")],
        Disposition: new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new(reqApplicability, "requirement"),
            ["open-questions"] = new(oqApplicability, "question"),
        },
        RiskLevel: "low", Notes: null);

    [Fact]
    public async Task Jeder_Zweig_bekommt_genau_seine_dispositions_gekeyte_Quelle()
    {
        var ledger = new ConsumableLedger("test", [
            Claim("nur-anforderung", reqApplicability: "required", oqApplicability: "not_applicable"),
            Claim("nur-frage", reqApplicability: "not_applicable", oqApplicability: "required"),
        ], 0, "test");

        var received = new ConcurrentDictionary<string, List<string>>();
        var run = new RunContext(RunId.New(), "test-fanout-routing");
        run.EnsureFolders();

        Microsoft.Agents.AI.Workflows.Workflow FakeBranch(string type)
        {
            var fake = new FakeBranchExecutor(type, received);
            return new WorkflowBuilder(fake).WithName($"Fake-{type}").WithOutputFrom(fake).Build();
        }

        var workflow = BaselineFanOutWorkflow.Build([
            ("requirements", "requirements", FakeBranch("requirements")),
            ("open-questions", "open-questions", FakeBranch("open-questions")),
        ], run);

        var wfRun = await InProcessExecution.Default.RunAsync(workflow, ledger, run.RunId, CancellationToken.None);

        // R-18-Stil auch im Test: Fehler-Events duerfen nicht still bleiben (Fan-out meldet Executor-Fehler als Events).
        var failures = wfRun.OutgoingEvents
            .Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e switch
            {
                ExecutorFailedEvent f => $"{f.ExecutorId}: {f.Data?.Message}",
                WorkflowErrorEvent w => $"{w.GetType().Name}: {w.Exception?.Message}",
                _ => e.ToString() ?? ""
            }).ToList();
        Assert.True(failures.Count == 0, "Workflow-Fehler:\n" + string.Join("\n", failures));

        // Routing: genau EINE Quelle je Zweig, mit dem EIGENEN ArtifactType etikettiert.
        var req = Assert.Single(received["requirements"]).Split('');
        var oq = Assert.Single(received["open-questions"]).Split('');
        Assert.Equal("requirements", req[0]);
        Assert.Equal("open-questions", oq[0]);

        // Inhalt: jede Spur sieht nur ihre Claims (not_applicable der EIGENEN Spur ist herausgefiltert)
        // und traegt ihre EIGENE Dispositions-Annotation.
        Assert.Contains("nur-anforderung", req[1]);
        Assert.DoesNotContain("nur-frage", req[1]);
        Assert.Contains("requirements=required", req[1]);

        Assert.Contains("nur-frage", oq[1]);
        Assert.DoesNotContain("nur-anforderung", oq[1]);
        Assert.Contains("open-questions=required", oq[1]);

        // Fan-in: der Collector hat BEIDE Zweig-Outputs zum Set eingesammelt (baseline-set.json).
        var setPath = Path.Combine(run.RunDir, "baseline-set.json");
        Assert.True(File.Exists(setPath));
        var set = JsonSerializer.Deserialize<VerifiedBaselineSet>(File.ReadAllText(setPath), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(2, set.Artifacts.Count);
    }
}
