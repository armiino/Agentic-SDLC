using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;

/// <summary>
/// Baut den Checker-Repair-Zyklus als eigenständigen MAF-<see cref="Microsoft.Agents.AI.Workflows.Workflow"/> aus
/// drei typisierten Custom-Executoren. Bewusst OHNE Maker: dieser Workflow bekommt ein bereits erzeugtes Artefakt
/// und liefert das geprüfte/reparierte zurück. Das macht ihn (a) standalone lauffähig (Runner auf einer .md) und
/// (b) per <c>BindAsExecutor</c> als EIN wiederverwendbarer Knoten hinter jeden Generator-Agenten einhängbar
/// (Kapitel C: <c>reqAgent → [CheckerRepair] → risksAgent → [CheckerRepair] → …</c>).
/// </summary>
/// <remarks>
/// Graph (Message-Passing, ein Zyklus — der Loop lebt INNEN, sodass der äußere Graph azyklisch bleiben kann):
/// <code>
///   (CheckArtifactMessage)──► Checker ──(Decision==Repair)──► Repair ──┐
///                                │                                      │  (Loop-Back, Iteration+1)
///                                └──(Decision!=Repair)──► Finalize       ▼
///                                     (Pass|MaxIterations|HumanReview)  Checker
/// </code>
/// Zwei KONDITIONALE Kanten am Checker filtern auf <see cref="CheckVerdictMessage.Decision"/>
/// (MAF: <c>AddEdge&lt;T&gt;(source, target, Func&lt;T,bool&gt;)</c>). Die Rückkante <c>Repair → Checker</c> ist ein
/// echter Zyklus — MAF-Workflows sind zyklisch (Superstep-Modell). Die Iterationsschranke wandert in der Message
/// (<see cref="CheckArtifactMessage.Iteration"/>) und wird im Checker terminal (<c>MaxIterationsReached</c>)
/// ausgewertet → garantierte Terminierung. Start-Executor = Checker → Eingangstyp des Workflows ist
/// <see cref="CheckArtifactMessage"/>; Ausgabetyp ist <see cref="CheckerRepairResult"/> (Finalize-YieldOutput).
/// </remarks>
public static class CheckerRepairWorkflow
{
    public const string WorkflowName = "CheckerRepair";

    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        CheckerExecutor checker, RepairExecutor repair, FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(checker)
            .WithName(WorkflowName)
            .WithDescription("Checker (MC0 + C7-Vote) →[Repair-Loop]/[Finalize]. Eigenständiger, quell-generischer "
                           + "Grounding-Prüfer; konditionale Kanten + Loop-Back, bounded Iterationen.");

        // Konditionale Verzweigung am Checker (auf der Decision im Verdikt):
        builder.AddEdge<CheckVerdictMessage>(checker, repair, m => m is not null && m.Decision == ContractDecision.Repair);
        builder.AddEdge<CheckVerdictMessage>(checker, finalize, m => m is not null && m.Decision != ContractDecision.Repair);

        // Loop-Back: Repair schickt das reparierte Artefakt (Iteration+1) zurück in den Checker.
        builder.AddEdge(repair, checker);

        return builder.Build();
    }
}
