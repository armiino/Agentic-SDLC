using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;
using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Branch;

/// <summary>
/// E-c — komponiert EINEN Artefakt-Zweig MAF-nativ: der Evidence-Baseline-Agent, der gebundene CheckerRepair-
/// Subworkflow und das deterministische ID-Gate werden zu einem sauberen linearen Außengraphen verkettet. Der
/// Repair-Zyklus lebt INNEN im gebundenen Subworkflow (versteckt); der Zweig selbst ist azyklisch und damit im
/// späteren Fan-out (E-d) frei adressierbar/wiederverwendbar.
/// </summary>
/// <remarks>
/// Graph (typisierte Kanten, ein Artefakttyp):
/// <code>
///   (string = Ledger-Projektion) ─► BranchMaker ─CheckArtifactMessage─► [CheckerRepair].BindAsExecutor
///                                                       ─CheckerRepairResult─► AssignIds ─► (ArtifactDocument)
/// </code>
/// MAF-nativ per offizieller Sub-Workflow-Semantik: ein via <c>Workflow.BindAsExecutor</c> gebundener Subworkflow
/// akzeptiert den Input-Typ seines Start-Executors (<c>CheckerExecutor : Executor&lt;CheckArtifactMessage&gt;</c>)
/// und sendet den Output-Typ seines Finalize (<see cref="CheckerRepairResult"/>). Deshalb passen die Kanten
/// Maker→[CheckerRepair]→AssignIds ohne Adapter. Der Zweig ist selbst wieder ein <see cref="Microsoft.Agents.AI.Workflows.Workflow"/>
/// (Start = Maker, Input <see cref="string"/>, Output <see cref="Artifacts.ArtifactDocument"/>) und damit für E-d
/// erneut per <c>BindAsExecutor</c> als EIN Fan-out-Knoten bindbar.
/// </remarks>
public static class ArtifactBranchWorkflow
{
    /// <summary>Baut den Zweig für genau einen Artefakttyp. <paramref name="dispositionKey"/> steuert den quell-
    /// generischen Checker; <paramref name="artifactType"/> keyt die Node-IDs (distinkt im Fan-out).</summary>
    public static Microsoft.Agents.AI.Workflows.Workflow Build(
        AIAgent agent,
        ContractCritic critic,
        ContractRepair repair,
        ConsumableLedger ledger,
        string artifactType,
        string dispositionKey,
        int k,
        int minVotes,
        int maxIterations,
        string model,
        RunContext run)
    {
        // Jeder Zweig schreibt in seinen eigenen Unterordner baselines/{type} → keine Kollision im Fan-out/Rezept.
        var scope = $"baselines/{artifactType}";

        // 1) CheckerRepair-Subworkflow (der Zyklus) bauen — wie im Standalone-Fall, nur hier eingebettet.
        var checker = new CheckerExecutor(critic, ledger, dispositionKey, k, minVotes, maxIterations, run, scope);
        var repairExec = new RepairExecutor(repair, ledger, run, scope);
        var finalize = new FinalizeExecutor(run, artifactType, scope);
        var checkerRepair = CheckerRepairWorkflow.Build(checker, repairExec, finalize);

        // 2) Subworkflow als EIN Knoten binden (Zyklus versteckt).
        var checkerRepairNode = checkerRepair.BindAsExecutor($"CheckerRepair-{artifactType}");

        // 3) Zweig-Stufen davor/danach.
        var maker = new ArtifactBranchMakerExecutor(agent, run, artifactType, scope);
        var assignIds = new ArtifactAssignIdsExecutor(run, artifactType, model, scope);

        // 4) Linearer Zweig-Außengraph.
        var builder = new WorkflowBuilder(maker)
            .WithName($"ArtifactBranch-{artifactType}")
            .WithDescription($"EvidenceBaselineAgent → [CheckerRepair] → AssignIds für '{artifactType}' "
                           + "(Repair-Zyklus im gebundenen Subworkflow versteckt).");

        builder.AddEdge(maker, checkerRepairNode);
        builder.AddEdge(checkerRepairNode, assignIds);

        // Zweig-Output deklarieren (dieselbe MAF-Regel wie im CheckerRepair): erst dadurch reicht der als
        // BindAsExecutor gebundene Zweig sein ArtifactDocument an die nachgelagerte Fan-in-Barrier (E-d) weiter
        // — und surfaced den Output auch im Standalone-Lauf.
        builder.WithOutputFrom(assignIds);

        return builder.Build();
    }
}
