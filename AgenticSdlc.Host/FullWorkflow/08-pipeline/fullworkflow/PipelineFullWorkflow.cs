using AgenticSdlc.Host.FullWorkflow.Delta;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' — die EINE Bauzeit-Orchestrierungsstelle der ganzen Kette (Design-Note §2/§8). Der komplette Graph
/// steht hier auf einem Bildschirm: Knoten + Kanten (inkl. Conditional Edges) + Gates. NIRGENDS sonst wird
/// orchestriert (keine CLI-Aufrufe aus Executors).
/// </summary>
/// <remarks>
/// Aufbau stufenweise (Schritt 4): <b>Slice v0</b> = Seed(transcript) -> 01-ledger -> Terminal — beweist, dass
/// pipeline-full einen ECHTEN MAF-Graphen ausführt (nicht mehr Skeleton). Die weiteren Stufen (Adjudikations-Gate,
/// Recipe, Delta, Ingest/Pbi/Forward mit ihren Gates + Conditional Edges) werden hier iterativ ergänzt — nach
/// dem Muster von <see cref="PipelineComposedWorkflow"/> (Ingest/Pbi ist bereits die Vorlage).
/// </remarks>
internal static class PipelineFullWorkflow
{
    /// <summary>
    /// Slice v1: Seed(transcript) -> 01-ledger -> Adjudikations-Gate -> Terminal(consumable). Ledger yieldet bei
    /// Fehler terminal (String); Adjudikation liefert den consumable terminal (bzw. Pause-Hinweis interactive).
    /// Nächste Slices hängen weitere Kanten an (Recipe, Delta, Ingest/Pbi/Forward).
    /// </summary>
    public static Workflow Assemble(
        LedgerWrapperExecutor ledger,
        AdjudicationStageExecutor adjudication,
        BaselineStageExecutor baselines,
        ProjectStateBuildExecutor delta)
    {
        return new WorkflowBuilder(ledger)
            .WithName("pipeline-full")
            .WithDescription("Transkript -> 01-ledger -> Adjudikation -> 02-baselines -> 04-delta (Slice v2; weitere Stufen folgen).")
            .AddEdge(ledger, adjudication)
            .AddEdge(adjudication, baselines)
            .AddEdge(baselines, delta)
            .WithOutputFrom(delta)
            .Build();
    }
}
