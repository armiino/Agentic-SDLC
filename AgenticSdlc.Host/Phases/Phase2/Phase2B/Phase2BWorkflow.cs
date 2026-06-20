using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Baut den Phase-2.1B-Workflow (`artifact_state`): dieselbe lineare Reihenfolge wie 2.1A, aber die
/// Agenten sind in Custom-Executors verpackt, die den Kontext über den MAF-Shared-State weiterreichen.
/// </summary>
/// <remarks>
/// Context -> Requirements -> Risks -> Architecture -> OpenQuestions.
/// Transport zwischen den Executors = MAF-Shared-State (Scope <see cref="Phase2BState.Scope"/>); die
/// Edges tragen nur die typisierte Trigger-Message <see cref="Phase2BHandoff"/>. Welche Keys ein
/// Specialist liest und ob er sein Artefakt in den State schreibt, steuert die <paramref name="policy"/>.
/// </remarks>
public static class Phase2BWorkflow
{
    public static Workflow Build(
        Phase2Agents agents,
        RunContext run,
        Phase2BStatePolicy policy,
        string repoRoot)
    {
        var stateLog = new Phase2BStateAccessLogger(run);

        var context = new ContextStateExecutor(agents.Context, run, repoRoot, stateLog);

        var requirements = new SpecialistStateExecutor(
            agents.Requirements,
            Phase2AgentFactory.RequirementsAgentName,
            ownArtifactKey: Phase2BState.RequirementsKey,
            docRelPath: "docs/requirements.md",
            readKeys: policy.ReadsFor(Phase2AgentFactory.RequirementsAgentName),
            writeArtifacts: policy.WriteArtifacts,
            isLast: false,
            run, repoRoot, stateLog);

        var risks = new SpecialistStateExecutor(
            agents.Risks,
            Phase2AgentFactory.RisksAgentName,
            ownArtifactKey: Phase2BState.RisksKey,
            docRelPath: "docs/risks.md",
            readKeys: policy.ReadsFor(Phase2AgentFactory.RisksAgentName),
            writeArtifacts: policy.WriteArtifacts,
            isLast: false,
            run, repoRoot, stateLog);

        var architecture = new SpecialistStateExecutor(
            agents.Architecture,
            Phase2AgentFactory.ArchitectureAgentName,
            ownArtifactKey: Phase2BState.ArchitectureKey,
            docRelPath: "docs/architecture.md",
            readKeys: policy.ReadsFor(Phase2AgentFactory.ArchitectureAgentName),
            writeArtifacts: policy.WriteArtifacts,
            isLast: false,
            run, repoRoot, stateLog);

        var openQuestions = new SpecialistStateExecutor(
            agents.OpenQuestions,
            Phase2AgentFactory.OpenQuestionsAgentName,
            ownArtifactKey: Phase2BState.OpenQuestionsKey,
            docRelPath: "docs/open-questions.md",
            readKeys: policy.ReadsFor(Phase2AgentFactory.OpenQuestionsAgentName),
            writeArtifacts: policy.WriteArtifacts,
            isLast: true,
            run, repoRoot, stateLog);

        var builder = new WorkflowBuilder(context)
            .WithName("Phase2.1B")
            .WithDescription("MAF-Workflow mit Shared-State-Transport (artifact_state) für frühe SDLC-Artefakte.");

        builder.AddEdge(context, requirements);
        builder.AddEdge(requirements, risks);
        builder.AddEdge(risks, architecture);
        builder.AddEdge(architecture, openQuestions);

        return builder.Build();
    }
}
