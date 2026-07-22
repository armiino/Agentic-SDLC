using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2;

/// <summary>
/// Baut den Phase-2.1-Workflow mit "MAF-WorkflowBuilder"
/// </summary>
/// <remarks>
/// Phase 2.1 untersucht gezielt den MAF-Workflow-Mechanismus:
/// mehrere Specialist Agents (oder executor wie man will) werden als gerichteter, azyklischer Workflow verbunden (DAG).
///
/// Der Workflow ist bewusst erstmal linear und ohne Conditions oder Repair-Loops:
///
/// Context
/// -> Requirements
/// -> Risks
/// -> Architecture
/// -> OpenQuestions
///
/// Diese Reduktion ist methodisch wichtig. In Phase 2.1 soll nur der Effekt von
/// Workflow-Struktur und Rollentrennung beobachtet werden. Conditional Edges,
/// Repair-Loops und Manager-Entscheidungen werden erst in zukunft getestet
///
/// MAF-Bezug:
/// Der Microsoft Agent Framework WorkflowBuilder konstruiert einen gerichteten
/// Graphen aus Executors und Edges. Wenn <c>AIAgent</c>-Instanzen übergeben
/// werden, bindet das Framework diese als Agent-Executors
/// </remarks>
public static class Phase2Workflow
{
    /// <summary>
    /// Erstellt den Phase-2.1-Workflow in der festgelegten Specialist-Reihenfolge.
    /// </summary>
    public static Workflow Build(Phase2Agents agents)
    {
        var builder = new WorkflowBuilder(agents.Context)
            .WithName("Phase2.1")
            .WithDescription("Statischer MAF-Workflow für frühe SDLC-Artefakte mit Specialist Agents.");

        builder.AddEdge(agents.Context, agents.Requirements);
        builder.AddEdge(agents.Requirements, agents.Risks);
        builder.AddEdge(agents.Risks, agents.Architecture);
        builder.AddEdge(agents.Architecture, agents.OpenQuestions);

        return builder.Build();
    }
}
