using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.FanOut;

/// <summary>
/// E-d — der Fan-out/Fan-in-Außengraph: der Ledger wird an mehrere Artefakt-Zweige verteilt (parallel), deren
/// geprüfte Baselines eine Barrier einsammelt. MAF-nativ mit <c>AddFanOutEdge</c> + <c>AddFanInBarrierEdge</c>.
/// Jeder Zweig ist der E-c-<c>ArtifactBranchWorkflow</c>, hier SELBST per <c>BindAsExecutor</c> als EIN Knoten
/// gebunden — die Wiederverwendung des Bausteins über zwei Bindungs-Ebenen.
/// </summary>
/// <remarks>
/// Graph:
/// <code>
///   (string=Ledger-Projektion) ─► Dispatch ─fan-out─► [Branch-requirements]  ─┐
///                                                     [Branch-risks]        ─┤ Fan-in-Barrier
///                                                       …                    ─┘        │
///                                                                                      ▼
///                                                                           Collector ─► VerifiedBaselineSet
/// </code>
/// MAF-nativ: <c>AddFanOutEdge(dispatch, branches)</c> sendet die (identische) Ledger-Projektion an jeden Zweig
/// (jeder liest die Quelle UNABHÄNGIG); <c>AddFanInBarrierEdge(branches, collector)</c> hält die Zweig-Outputs, bis
/// JEDER ein <see cref="Artifacts.ArtifactDocument"/> geliefert hat, und streamt sie an den Collector. Jeder Zweig
/// deklariert intern <c>WithOutputFrom(AssignIds)</c>, damit sein <c>ArtifactDocument</c> als gebundener Knoten an
/// die Barrier weitergereicht wird.
/// </remarks>
public static class BaselineFanOutWorkflow
{
    public const string WorkflowName = "BaselineFanOut";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        IReadOnlyList<(string ArtifactType, Microsoft.Agents.AI.Workflows.Workflow Branch)> branches, RunContext run)
    {
        var dispatch = new BaselineFanOutDispatchExecutor();
        var collector = new BaselineCollectorExecutor(run, branches.Count);

        // Jeden E-c-Zweig als EINEN Knoten binden (Node-ID nach Artefakttyp → distinkt).
        var branchNodes = branches
            .Select(b => b.Branch.BindAsExecutor($"Branch-{b.ArtifactType}"))
            .ToList();

        var builder = new WorkflowBuilder(dispatch)
            .WithName(WorkflowName)
            .WithDescription($"Ledger ─fan-out─► [{string.Join(", ", branches.Select(b => b.ArtifactType))}] "
                           + "─fan-in-barrier─► VerifiedBaselineSet (jeder Zweig = gebundener ArtifactBranch).");

        builder.AddFanOutEdge(dispatch, branchNodes);
        builder.AddFanInBarrierEdge(branchNodes, collector);
        builder.WithOutputFrom(collector);

        return builder.Build();
    }
}
