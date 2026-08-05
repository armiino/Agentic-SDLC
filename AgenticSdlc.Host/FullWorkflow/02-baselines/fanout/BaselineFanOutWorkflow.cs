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
///   (ConsumableLedger) ─► Dispatch ─BranchSource je Spur─► [Branch-requirements] ─┐
///                                                     [Branch-risks]        ─┤ Fan-in-Barrier
///                                                       …                    ─┘        │
///                                                                                      ▼
///                                                                           Collector ─► VerifiedBaselineSet
/// </code>
/// MAF-nativ (R-37): der Dispatch sendet je Zweig SEINE dispositions-gekeyte <c>BranchSource</c> über
/// prädikat-geroutete Kanten; <c>AddFanInBarrierEdge(branches, collector)</c> hält die Zweig-Outputs, bis
/// JEDER ein <see cref="Artifacts.ArtifactDocument"/> geliefert hat, und streamt sie an den Collector. Jeder Zweig
/// deklariert intern <c>WithOutputFrom(AssignIds)</c>, damit sein <c>ArtifactDocument</c> als gebundener Knoten an
/// die Barrier weitergereicht wird.
/// </remarks>
public static class BaselineFanOutWorkflow
{
    public const string WorkflowName = "BaselineFanOut";

    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        IReadOnlyList<(string ArtifactType, string DispositionKey, Microsoft.Agents.AI.Workflows.Workflow Branch)> branches, RunContext run)
    {
        // R-37 (MAF-native Form): der Dispatch prägt je Spur eine typisierte BranchSource (eigener Dispositions-
        // Schlüssel); die Kanten-Prädikate routen sie an genau ihren Zweig — dasselbe Muster wie die
        // Conditional-Edge-Loops (ReClarify/GateLoop). Ein fremd geroutetes Paket wäre im Maker ein LAUTER Fehler.
        var dispatch = new BaselineFanOutDispatchExecutor(branches.Select(b => (b.ArtifactType, b.DispositionKey)).ToList());
        var collector = new BaselineCollectorExecutor(run, branches.Count);

        // Jeden E-c-Zweig als EINEN Knoten binden (Node-ID nach Artefakttyp → distinkt).
        var branchNodes = branches
            .Select(b => b.Branch.BindAsExecutor($"Branch-{b.ArtifactType}"))
            .ToList();

        var builder = new WorkflowBuilder(dispatch)
            .WithName(WorkflowName)
            .WithDescription($"Consumable ─dispatch(BranchSource je Spur)─► [{string.Join(", ", branches.Select(b => b.ArtifactType))}] "
                           + "─fan-in-barrier─► VerifiedBaselineSet (jeder Zweig = gebundener ArtifactBranch).");

        for (var i = 0; i < branches.Count; i++)
        {
            var type = branches[i].ArtifactType;
            builder.AddEdge<Branch.BranchSource>(dispatch, branchNodes[i],
                m => m is not null && string.Equals(m.ArtifactType, type, StringComparison.Ordinal));
        }
        builder.AddFanInBarrierEdge(branchNodes, collector);
        builder.WithOutputFrom(collector);

        return builder.Build();
    }
}
