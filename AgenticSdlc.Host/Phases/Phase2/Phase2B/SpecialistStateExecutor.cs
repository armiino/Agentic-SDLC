using System.Text;
using AgenticSdlc.Host.Phases.Phase2.Validation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Phase2B;

/// <summary>
/// Custom Executor, der einen Specialist-Agent über den MAF-Shared-State mit Kontext versorgt.
/// </summary>
/// <remarks>
/// Liest die laut Policy vorgesehenen State-Keys (Context + ggf. Upstream-Artefakte), injiziert sie
/// in die Eingabe des Agenten (frischer Lauf, KEINE Conversation-Akkumulation), ruft den Agenten auf
/// (der sein docs/*.md via fs_write schreibt) und legt — wenn aktiviert — sein Ergebnis unter dem
/// eigenen Key zurück in den State. Fehlende Read-Keys werden geloggt und übersprungen (robust gegen
/// writeArtifacts:false). Artifact-Gate (§3a) bricht fail-fast ab, wenn das Pflichtartefakt fehlt/leer ist.
/// </remarks>
[SendsMessage(typeof(Phase2BHandoff))]
[YieldsOutput(typeof(string))]
internal sealed class SpecialistStateExecutor : Executor<Phase2BHandoff>
{
    private readonly AIAgent _agent;
    private readonly string _agentName;
    private readonly string _ownArtifactKey;
    private readonly string _docRelPath;
    private readonly IReadOnlyList<string> _readKeys;
    private readonly bool _writeArtifacts;
    private readonly bool _isLast;
    private readonly RunContext _run;
    private readonly string _repoRoot;
    private readonly Phase2BStateAccessLogger _stateLog;

    public SpecialistStateExecutor(
        AIAgent agent,
        string agentName,
        string ownArtifactKey,
        string docRelPath,
        IReadOnlyList<string> readKeys,
        bool writeArtifacts,
        bool isLast,
        RunContext run,
        string repoRoot,
        Phase2BStateAccessLogger stateLog)
        : base(agentName)
    {
        _agent = agent;
        _agentName = agentName;
        _ownArtifactKey = ownArtifactKey;
        _docRelPath = docRelPath;
        _readKeys = readKeys;
        _writeArtifacts = writeArtifacts;
        _isLast = isLast;
        _run = run;
        _repoRoot = repoRoot;
        _stateLog = stateLog;
    }

    public override async ValueTask HandleAsync(
        Phase2BHandoff message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // 1. State laut Policy lesen. Fehlende Keys: loggen + überspringen (robust gegen writeArtifacts:false).
        var injectedSections = new List<(string Label, string Content)>();
        foreach (var key in _readKeys)
        {
            var value = await context
                .ReadStateAsync<string>(key, scopeName: Phase2BState.Scope, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(value))
            {
                _stateLog.LogMissingRead(_agentName, key);
                continue;
            }

            _stateLog.LogRead(_agentName, key, value);
            injectedSections.Add((Phase2BState.LabelForKey(key), value));
        }

        // 2. Eingabenachricht aus dem gelesenen State bauen (Transport = Shared State, nicht Datei).
        var userMessage = BuildInjectedMessage(injectedSections);

        // 3. Agent ausfuehren (frischer Lauf; schreibt sein docs/*.md via fs_write).
        await _agent
            .RunAsync([new ChatMessage(ChatRole.User, userMessage)], cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // 4. Artifact-Gate (§3a): Pflichtartefakt muss existieren + nicht leer sein.
        var docAbsPath = Path.Combine(_repoRoot, _docRelPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(docAbsPath))
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_MISSING: {_agentName} did not write {_docRelPath}");

        var artifactContent = await File.ReadAllTextAsync(docAbsPath, cancellationToken).ConfigureAwait(false);
        var gate = ArtifactQualityGate.Evaluate(artifactContent);
        if (gate.Verdict == ArtifactQualityVerdict.Empty)
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_EMPTY: {_agentName} wrote an empty {_docRelPath}");
        if (gate.Verdict == ArtifactQualityVerdict.Invalid)
            throw new InvalidOperationException(
                $"PHASE2B_ARTIFACT_INVALID: {_agentName} wrote an unusable {_docRelPath} ({gate.Reason})");

        // K4: Overwrite-Diagnose (nur Logging).
        _stateLog.LogOverwriteDiagnosis(_agentName, _docRelPath, artifactContent.Length);

        // 5. Ergebnis (optional) in den State legen, damit Downstream es lesen kann.
        // Nur gültiger Inhalt gelangt in den State (Gate hat oben bestanden).
        var availableKeys = new List<string>(message.AvailableKeys);
        if (_writeArtifacts)
        {
            await context
                .QueueStateUpdateAsync(_ownArtifactKey, artifactContent, scopeName: Phase2BState.Scope, cancellationToken)
                .ConfigureAwait(false);
            _stateLog.LogWrite(_agentName, _ownArtifactKey, artifactContent);
            if (!availableKeys.Contains(_ownArtifactKey))
                availableKeys.Add(_ownArtifactKey);
        }

        // 6. Letzter Executor terminiert den Workflow; sonst Trigger an den naechsten.
        if (_isLast)
            await context.YieldOutputAsync($"Phase 2.1B abgeschlossen. State-Keys: {string.Join(", ", availableKeys)}").ConfigureAwait(false);
        else
            await context.SendMessageAsync(new Phase2BHandoff(availableKeys)).ConfigureAwait(false);
    }

    private string BuildInjectedMessage(IReadOnlyList<(string Label, string Content)> sections)
    {
        var sb = new StringBuilder();

        if (sections.Count == 0)
        {
            sb.AppendLine("Hinweis: Es wurde kein Kontext aus dem Shared State bereitgestellt.");
        }
        else
        {
            foreach (var (label, content) in sections)
            {
                sb.AppendLine($"## {label} (aus MAF Shared State)");
                sb.AppendLine(content);
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Deine Aufgabe");
        sb.AppendLine(
            $"Erzeuge dein Pflichtartefakt gemaess deiner Rolle und schreibe es mit fs_write nach {_docRelPath}. " +
            "Der oben bereitgestellte Kontext stammt aus dem MAF-Shared-State — lies dafuer keine Dateien per fs_read.");

        return sb.ToString();
    }
}
