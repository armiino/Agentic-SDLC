using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Chain;

/// <summary>
/// Bindeglied der vollen Komposition: nimmt das <see cref="VerifiedBaselineSet"/> des Fan-outs und wählt die
/// EINE Baseline aus, aus der abgeleitet werden soll (z. B. requirements) — als typisiertes
/// <see cref="ArtifactDocument"/> für den nachgelagerten Derivation-Knoten. Die vollen Items liest es von der Platte
/// (der Fan-out-Collector hat <c>{type}.artifact.json</c> in denselben Run geschrieben — „Disk = Wahrheit", wie in
/// LedgerBuildRunner; das VerifiedBaselineSet selbst trägt nur den Index).
/// </summary>
[SendsMessage(typeof(ArtifactDocument))]
internal sealed class SelectBaselineExecutor : Executor<VerifiedBaselineSet>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly string _artifactType;
    private readonly RunContext _run;

    public SelectBaselineExecutor(string artifactType, RunContext run) : base($"SelectBaseline-{artifactType}")
    {
        _artifactType = artifactType;
        _run = run;
    }

    public override async ValueTask HandleAsync(VerifiedBaselineSet set, IWorkflowContext context, CancellationToken ct = default)
    {
        var path = Path.Combine(_run.RunDir, $"{_artifactType}.artifact.json");
        if (!File.Exists(path))
            throw new InvalidOperationException($"[chain] Baseline '{_artifactType}.artifact.json' nicht im Run — Fan-out enthielt den Typ nicht?");

        var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json)
                  ?? throw new InvalidOperationException($"[chain] {_artifactType}.artifact.json nicht lesbar.");

        _run.AppendEvent(new { type = "BASELINE_SELECTED", runId = _run.RunId, artifactType = _artifactType, items = doc.Items.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(doc).ConfigureAwait(false);
    }
}

/// <summary>
/// E-Chain — die volle MAF-Komposition: <c>Ledger ─► [Fan-out] ─► SelectBaseline ─► [Derivation]</c>, wobei
/// Fan-out UND Derivation je ein per <c>BindAsExecutor</c> gebundener Sub-Workflow sind (die selbst wieder
/// gebundene Sub-Workflows enthalten — CheckerRepair im Zweig). Mehrstufige Wiederverwendung derselben Bausteine.
/// </summary>
/// <remarks>
/// Input = Ledger-Projektion (<see cref="string"/>, der Fan-out-Start-Typ). Output = <see cref="Derivation.DerivationResult"/>
/// (Derivation-YieldOutput). Kein Adapter nötig: die Sub-Workflow-I/O-Typen passen an den Kanten (Fan-out out =
/// VerifiedBaselineSet → Select in; Select out = ArtifactDocument → Derivation in; Derivation out = DerivationResult).
/// </remarks>
public static class EvidenceChainWorkflow
{
    public const string WorkflowName = "EvidenceChain";

    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        Microsoft.Agents.AI.Workflows.Workflow fanOut,
        Microsoft.Agents.AI.Workflows.Workflow derivation,
        string sourceArtifactType,
        RunContext run)
    {
        var fanOutNode = fanOut.BindAsExecutor("BaselineFanOut");
        var select = new SelectBaselineExecutor(sourceArtifactType, run);
        var derivationNode = derivation.BindAsExecutor("Derivation");

        var builder = new WorkflowBuilder(fanOutNode)
            .WithName(WorkflowName)
            .WithDescription($"Ledger → [Fan-out] → Select({sourceArtifactType}) → [Derivation] (mehrstufig BindAsExecutor).");

        builder.AddEdge(fanOutNode, select);
        builder.AddEdge(select, derivationNode);
        builder.WithOutputFrom(derivationNode);
        return builder.Build();
    }
}
