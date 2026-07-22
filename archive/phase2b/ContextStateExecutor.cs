using AgenticSdlc.Host.Phases.Phase2.Validation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Custom Executor, der den ContextAgent aufruft und dessen Kontext in den MAF-Shared-State legt.
/// </summary>
/// <remarks>
/// Verhalten des ContextAgent ist identisch zu 2.1A: er liest das Transkript und schreibt
/// <c>state/context.md</c> via fs_write. Anschliessend liest der Executor diese Datei EINMALIG
/// (Host-Brücke File→State) und schreibt den Inhalt unter <see cref="Phase2BState.ContextKey"/> in
/// den Shared State (Scope <see cref="Phase2BState.Scope"/>). Downstream-Specialists lesen den
/// Kontext ausschliesslich aus dem State, nie aus der Datei. context.md bleibt Audit-Snapshot.
/// </remarks>
[SendsMessage(typeof(Phase2BHandoff))]
internal sealed class ContextStateExecutor : Executor<string>
{
    private readonly AIAgent _agent;
    private readonly RunContext _run;
    private readonly string _repoRoot;
    private readonly Phase2BStateAccessLogger _stateLog;

    public ContextStateExecutor(
        AIAgent agent,
        RunContext run,
        string repoRoot,
        Phase2BStateAccessLogger stateLog)
        : base(Phase2AgentFactory.ContextAgentName)
    {
        _agent = agent;
        _run = run;
        _repoRoot = repoRoot;
        _stateLog = stateLog;
    }

    public override async ValueTask HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 1. ContextAgent ausfuehren (liest Transkript, schreibt state/context.md via fs_write).
        await _agent
            .RunAsync([new ChatMessage(ChatRole.User, "Begin.")], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // 2. Artifact-Gate (§3a): context.md muss existieren + nicht leer sein.
        var contextRelPath = Phase2Artifacts.ContextPath(_run).Replace('\\', '/');
        var contextAbsPath = Path.Combine(_repoRoot, contextRelPath);
        if (!File.Exists(contextAbsPath))
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_MISSING: {Phase2AgentFactory.ContextAgentName} did not write {contextRelPath}");

        var contextContent = await File.ReadAllTextAsync(contextAbsPath, cancellationToken).ConfigureAwait(false);
        var gate = ArtifactQualityGate.Evaluate(contextContent);
        if (gate.Verdict == ArtifactQualityVerdict.Empty)
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_EMPTY: {Phase2AgentFactory.ContextAgentName} wrote an empty {contextRelPath}");
        if (gate.Verdict == ArtifactQualityVerdict.Invalid)
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_INVALID: {Phase2AgentFactory.ContextAgentName} wrote an unusable {contextRelPath} ({gate.Reason})");

        // K4: Overwrite-Diagnose (nur Logging).
        _stateLog.LogOverwriteDiagnosis(Phase2AgentFactory.ContextAgentName, contextRelPath, contextContent.Length);

        // 3. Host-Brücke File→State: Kontext in den Shared State legen (Transport für Downstream).
        // Nur gültiger Inhalt gelangt in den State (Gate hat oben bestanden).
        await context
            .QueueStateUpdateAsync(Phase2BState.ContextKey, contextContent, scopeName: Phase2BState.Scope, cancellationToken)
            .ConfigureAwait(false);
        _stateLog.LogWrite(Phase2AgentFactory.ContextAgentName, Phase2BState.ContextKey, contextContent);

        // 4. Trigger an den ersten Specialist; Nutzlast liegt im State.
        await context
            .SendMessageAsync(new Phase2BHandoff([Phase2BState.ContextKey]))
            .ConfigureAwait(false);
    }
}
