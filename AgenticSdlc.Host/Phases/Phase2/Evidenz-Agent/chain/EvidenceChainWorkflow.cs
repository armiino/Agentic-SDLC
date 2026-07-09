using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Load;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Chain;

/// <summary>
/// Bindeglied der vollen Komposition: nimmt das <see cref="VerifiedBaselineSet"/> des Fan-outs und wählt die
/// Baseline(s) aus, aus der/denen abgeleitet werden soll — als <see cref="SourceArtifactSet"/> für den nachgelagerten
/// Derivation-Knoten. Die vollen Items liest es von der Platte (der Fan-out-Collector hat <c>{type}.artifact.json</c>
/// in denselben Run geschrieben — „Disk = Wahrheit", wie in LedgerBuildRunner; das VerifiedBaselineSet selbst trägt
/// nur den Index). Mehrere Typen ⇒ Multi-Source-Set (dann müssen alle Zweige im Fan-out gelaufen sein).
/// </summary>
[SendsMessage(typeof(SourceArtifactSet))]
internal sealed class SelectBaselineExecutor : Executor<VerifiedBaselineSet>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly IReadOnlyList<string> _artifactTypes;
    private readonly RunContext _run;

    // idSuffix macht den Executor eindeutig, wenn mehrere Ableitungen (Rezept) dieselbe Quell-Teilmenge wählen.
    public SelectBaselineExecutor(IReadOnlyList<string> artifactTypes, RunContext run, string? idSuffix = null)
        : base($"SelectBaseline-{string.Join("+", artifactTypes)}{(idSuffix is null ? "" : "-" + idSuffix)}")
    {
        _artifactTypes = artifactTypes;
        _run = run;
    }

    public override async ValueTask HandleAsync(VerifiedBaselineSet set, IWorkflowContext context, CancellationToken ct = default)
    {
        var docs = new List<ArtifactDocument>(_artifactTypes.Count);
        foreach (var artifactType in _artifactTypes)
        {
            var path = Path.Combine(_run.RunDir, "baselines", artifactType, "artifact.json");
            if (!File.Exists(path))
                throw new InvalidOperationException($"[chain] Baseline 'baselines/{artifactType}/artifact.json' nicht im Run — Fan-out enthielt den Typ nicht?");

            docs.Add(JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(path, ct).ConfigureAwait(false), Json)
                     ?? throw new InvalidOperationException($"[chain] {artifactType}.artifact.json nicht lesbar."));
        }

        _run.AppendEvent(new { type = "BASELINE_SELECTED", runId = _run.RunId, artifactTypes = _artifactTypes, items = docs.Sum(d => d.Items.Count), timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new SourceArtifactSet(docs)).ConfigureAwait(false);
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
        IReadOnlyList<string> sourceArtifactTypes,
        RunContext run)
    {
        var fanOutNode = fanOut.BindAsExecutor("BaselineFanOut");
        var select = new SelectBaselineExecutor(sourceArtifactTypes, run);
        var derivationNode = derivation.BindAsExecutor("Derivation");

        var builder = new WorkflowBuilder(fanOutNode)
            .WithName(WorkflowName)
            .WithDescription($"Ledger → [Fan-out] → Select({string.Join("+", sourceArtifactTypes)}) → [Derivation] (mehrstufig BindAsExecutor).");

        builder.AddEdge(fanOutNode, select);
        builder.AddEdge(select, derivationNode);
        builder.WithOutputFrom(derivationNode);
        return builder.Build();
    }

    /// <summary>
    /// Bau-Punkt 3 (<c>mode:load</c>): dieselbe Kette OHNE frischen Fan-out — die Baseline-Quelle ist ein
    /// <see cref="LoadBaselineExecutor"/> (liest vorhandene Artefakte). Downstream (<c>Select → [Derivation]</c>)
    /// ist identisch zum Build-Modus; nur der Startknoten ist getauscht. Input = trivialer Trigger-String.
    /// </summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow BuildFromLoad(
        LoadBaselineExecutor load,
        Microsoft.Agents.AI.Workflows.Workflow derivation,
        IReadOnlyList<string> sourceArtifactTypes,
        RunContext run)
    {
        var select = new SelectBaselineExecutor(sourceArtifactTypes, run);
        var derivationNode = derivation.BindAsExecutor("Derivation");

        var builder = new WorkflowBuilder(load)
            .WithName($"{WorkflowName}-Load")
            .WithDescription($"LoadBaseline({string.Join("+", sourceArtifactTypes)}) → Select → [Derivation] (mode:load, kein Fan-out).");

        builder.AddEdge(load, select);
        builder.AddEdge(select, derivationNode);
        builder.WithOutputFrom(derivationNode);
        return builder.Build();
    }
}
