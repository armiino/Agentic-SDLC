using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Mcp;
using AgenticSdlc.Host.Phases.Phase2.Evaluation;
using AgenticSdlc.Host.Phases.Phase2.Validation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenTelemetry;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using WorkflowRun = Microsoft.Agents.AI.Workflows.Run;

namespace AgenticSdlc.Host.Phases.Phase2;

/// <summary>
/// Führt den Phase-2.1-Run aus (wie auch phase 1 runner)
/// </summary>
/// <remarks>
/// Phase 2.1 ist der erste Schritt, in dem das Microsoft Agent Framework nicht
/// nur als einzelner Agent mit Tools genutzt wird, sondern als Workflow-System (DAG)
/// hier gerichteter graph..
/// Dieser Runner bleibt bewusst nah am Framework:
/// 1. MCP-Tools werden geladen.
/// 2. Specialist Agents werden erzeugt.
/// 3. Ein Workflow wird mit <see cref="WorkflowBuilder"/> gebaut.
/// 4. Der Workflow wird mit <see cref="InProcessExecution"/> ausgeführt.
///
/// Nach dem Workflow führt der Host eine minimale deterministische Validation
/// aus und schreibt bei Erfolg ein Approval-Artefakt als nachweis.
/// </remarks>
public sealed class Phase2Runner
{
    private readonly HostSettings _settings;
    private readonly RunContext _run;
    private readonly string _sourceName;
    private readonly ActivitySource _activitySource;
    private readonly string _repoRoot;

    public Phase2Runner(
        HostSettings settings,
        RunContext run,
        string sourceName,
        ActivitySource activitySource,
        string repoRoot)
    {
        _settings = settings;
        _run = run;
        _sourceName = sourceName;
        _activitySource = activitySource;
        _repoRoot = repoRoot;
    }

    /// <summary>
    /// Startet Phase-2.1-Workflow und liefert einen Prozess-Exit-Code
    /// </summary>
    public async Task<int> RunAsync()
    {
        EnsurePhaseFolders();

        _run.AppendEvent(new { type = "RUN_STARTED", runId = _run.RunId, phase = Phase2Artifacts.PhaseName, timestampUtc = DateTime.UtcNow });
        _run.AppendEvent(new { type = "WORKFLOW_STARTED", runId = _run.RunId, workflow = Phase2Artifacts.PhaseName, timestampUtc = DateTime.UtcNow });

        /*
         * Baggage hilft, spätere OpenTelemetry-Spans dem Phase-2.1-Workflow zuzuordnen. 
         * Das ist für die Arbeit wichtig, weil Phase 2.1 sich explizit dem MAF-Workflow richtet
         */
        Baggage.SetBaggage("run.id", _run.RunId);
        Baggage.SetBaggage("phase", Phase2Artifacts.PhaseName);
        Baggage.SetBaggage("workflow.name", Phase2Artifacts.PhaseName);

        using var runSpan = _activitySource.StartActivity("run.phase2_1.workflow", ActivityKind.Internal);
        runSpan?.SetTag("run.id", _run.RunId);
        runSpan?.SetTag("phase", Phase2Artifacts.PhaseName);
        runSpan?.SetTag("llm.provider", _settings.LlmProvider);
        runSpan?.SetTag("gen_ai.request.model", _settings.ModelId);
        runSpan?.SetTag("phase2.context_strategy", _settings.Phase2ContextStrategy);
        runSpan?.SetTag("phase2.prompts.context", _settings.GetPromptName(Phase2AgentFactory.ContextAgentName));
        runSpan?.SetTag("phase2.prompts.requirements", _settings.GetPromptName(Phase2AgentFactory.RequirementsAgentName));
        runSpan?.SetTag("phase2.prompts.risks", _settings.GetPromptName(Phase2AgentFactory.RisksAgentName));
        runSpan?.SetTag("phase2.prompts.architecture", _settings.GetPromptName(Phase2AgentFactory.ArchitectureAgentName));
        runSpan?.SetTag("phase2.prompts.open_questions", _settings.GetPromptName(Phase2AgentFactory.OpenQuestionsAgentName));
        runSpan?.SetTag("workflow.name", Phase2Artifacts.PhaseName);

        try
        {
            await using var localMcp = await ConnectLocalMcpAsync();

            var githubConnectError = await TryDiscoverGitHubToolsAsync();
            var localTools = await LoadLocalToolsAsync(localMcp, githubConnectError);
            var agents = CreateAgents(localTools);
            var workflow = _settings.Phase2ContextStrategy switch
            {
                "artifact_state" => Phase2B.Phase2BWorkflow.Build(
                    agents, _run, Phase2B.Phase2BStatePolicy.FromConfig(_settings), _repoRoot),
                "message_passing" => Phase2Workflow.Build(agents),
                var other => throw new InvalidOperationException(
                    $"Phase 2.1 context strategy '{other}' is not implemented. Use 'message_passing' or 'artifact_state'.")
            };

            var workflowRun = await RunWorkflowAsync(workflow);
            var hasFailedExecutor = RecordWorkflowEvents(workflowRun);
            var status = await workflowRun.GetStatusAsync(CancellationToken.None);

            _run.AppendEvent(new
            {
                type = "WORKFLOW_FINISHED",
                runId = _run.RunId,
                workflow = Phase2Artifacts.PhaseName,
                workflowSessionId = workflowRun.SessionId,
                status = status.ToString(),
                hasFailedExecutor,
                timestampUtc = DateTime.UtcNow
            });

            if (hasFailedExecutor)
            {
                runSpan?.SetStatus(ActivityStatusCode.Error, "At least one Phase 2.1 workflow executor failed.");
                WriteDiagnosis(new
                {
                    runId = _run.RunId,
                    phase = Phase2Artifacts.PhaseName,
                    status = "FAILED",
                    rootCause = new
                    {
                        code = "PHASE2_WORKFLOW_EXECUTOR_FAILED",
                        message = "The MAF workflow emitted an ExecutorFailedEvent."
                    },
                    timestampUtc = DateTime.UtcNow
                });
                CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);
                return 3;
            }

            var validator = new Phase2ArtifactValidator(
                run: _run,
                activitySource: _activitySource,
                repoRoot: _repoRoot);

            var validationExitCode = validator.Validate();
            if (validationExitCode != 0)
            {
                CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);
                return validationExitCode;
            }

            var approvalRecorder = new Phase2ApprovalRecorder(_run);
            approvalRecorder.RecordApprovalIfNeeded();

            _run.AppendEvent(new
            {
                type = "PHASE2_APPROVAL_COMPLETED",
                runId = _run.RunId,
                reason = "Phase 2.1 workflow, deterministic validation and host approval completed.",
                timestampUtc = DateTime.UtcNow
            });

            CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);

            if (_settings.JuryEnabled)
            {
                _run.AppendEvent(new { type = "JURY_STARTED", runId = _run.RunId, judgeModel = _settings.JuryJudgeModel ?? _settings.ModelId, timestampUtc = DateTime.UtcNow });
                try
                {
                    var jury = new Phase2JuryRunner(_settings, _run, _repoRoot);
                    await jury.RunAsync();
                }
                catch (Exception ex)
                {
                    _run.AppendEvent(new { type = "JURY_FAILED", runId = _run.RunId, error = ex.Message, errorType = ex.GetType().FullName, timestampUtc = DateTime.UtcNow });
                }
            }

            _run.AppendEvent(new { type = "RUN_FINISHED", runId = _run.RunId, status = "completed", timestampUtc = DateTime.UtcNow });
            return 0;
        }
        catch (Exception ex)
        {
            runSpan?.AddException(ex);
            runSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _run.AppendEvent(new
            {
                type = "RUN_FAILED",
                runId = _run.RunId,
                phase = Phase2Artifacts.PhaseName,
                reason = "Phase 2.1 workflow run threw exception.",
                errorType = ex.GetType().FullName,
                error = ex.Message,
                timestampUtc = DateTime.UtcNow
            });

            WriteDiagnosis(new
            {
                runId = _run.RunId,
                phase = Phase2Artifacts.PhaseName,
                status = "FAILED",
                rootCause = new
                {
                    code = "PHASE2_WORKFLOW_EXCEPTION",
                    message = "The Phase 2.1 MAF workflow failed before validation could run.",
                    errorType = ex.GetType().FullName,
                    error = ex.Message
                },
                timestampUtc = DateTime.UtcNow
            });

            CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);

            Console.Error.WriteLine("RUN FAILED - Phase 2.1 workflow exception:");
            Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return 3;
        }
    }

    private void EnsurePhaseFolders()
    {
        Directory.CreateDirectory(Phase2Artifacts.StateDir(_run));
        Directory.CreateDirectory(Phase2Artifacts.ValidationDir(_run));
    }

    private async Task<McpClient> ConnectLocalMcpAsync()
    {
        var localMcp = await McpConnections.ConnectLocalAsync();
        _run.AppendEvent(new { type = "MCP_LOCAL_CONNECTED", runId = _run.RunId, timestampUtc = DateTime.UtcNow });
        return localMcp;
    }

    private async Task<string?> TryDiscoverGitHubToolsAsync()
    {
        // GitHub-Tools werden in Phase 2.1 nur discovered, aber NICHT an Agenten übergeben.
        // Grund: GitHub-Aktionen (Issue Create, Push) sind erst später vorgeshen und erfordern explizites Approval. 
        // Frühes Discovery prüft die Verbindung und protokolliert den Status im tool-discovery.json
        try
        {
            await using var githubMcp = await McpConnections.ConnectGitHubAsync();
            _run.AppendEvent(new { type = "MCP_GITHUB_CONNECTED", runId = _run.RunId, timestampUtc = DateTime.UtcNow });

            _ = await githubMcp.ListToolsAsync().ConfigureAwait(false);
            _run.AppendEvent(new { type = "MCP_GITHUB_DISCOVERED", runId = _run.RunId, timestampUtc = DateTime.UtcNow });
            return null;
        }
        catch (Exception ex)
        {
            _run.AppendEvent(new { type = "MCP_GITHUB_FAILED", runId = _run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return ex.Message;
        }
    }

    private async Task<List<AITool>> LoadLocalToolsAsync(McpClient localMcp, string? githubConnectError)
    {
        var localTools = (await localMcp.ListToolsAsync().ConfigureAwait(false))
            .Cast<AITool>()
            .ToList();

        _run.WriteToolDiscovery(new
        {
            runId = _run.RunId,
            phase = Phase2Artifacts.PhaseName,
            local = new { count = localTools.Count, tools = localTools.Select(t => t.Name).ToArray() },
            github = new { error = githubConnectError }
        });

        _run.AppendEvent(new { type = "TOOLS_DISCOVERED", runId = _run.RunId, localCount = localTools.Count, timestampUtc = DateTime.UtcNow });
        return localTools;
    }

    private Phase2Agents CreateAgents(IReadOnlyList<AITool> localTools)
    {
        var agents = Phase2AgentFactory.CreateSpecialists(
            settings: _settings,
            run: _run,
            sourceName: _sourceName,
            localTools: localTools);

        foreach (var agent in EnumerateAgents(agents))
        {
            var agentStartedEvent = new
            {
                type = "AGENT_STARTED",
                runId = _run.RunId,
                phase = Phase2Artifacts.PhaseName,
                agentName = agent.Name,
                promptName = ResolvePromptName(agent.Name),
                allowedTools = localTools.Select(t => t.Name).ToArray(),
                timestampUtc = DateTime.UtcNow
            };

            _run.AppendEvent(agentStartedEvent);
            _run.AppendAgentEvent(agent.Name, agentStartedEvent);
        }

        return agents;
    }

    private async Task<WorkflowRun> RunWorkflowAsync(Workflow workflow)
    {
        /*
         * InProcessExecution ist die lokale Ausführungsumgebung von MAF.
         * Sie führt den Workflow ohne externen Workflow-Host aus und gibt einen
         * Run mit WorkflowEvents zurück. Genau diese Events sind für Phase 2.1
         * der Nachweis, dass ein MAF-Workflow ausgeführt wurde.
         */
        return await InProcessExecution.Default.RunAsync(
            workflow,
            "Begin.",
            _run.RunId,
            CancellationToken.None);
    }

    private bool RecordWorkflowEvents(WorkflowRun workflowRun)
    {
        var hasFailedExecutor = false;
        var outputBuffers = new Dictionary<string, StringBuilder>(StringComparer.Ordinal);

        foreach (var workflowEvent in workflowRun.OutgoingEvents)
        {
            if (workflowEvent is ExecutorCompletedEvent completed)
            {
                _run.AppendEvent(new
                {
                    type = "EXECUTOR_FINISHED",
                    runId = _run.RunId,
                    executorId = completed.ExecutorId,
                    eventType = workflowEvent.GetType().Name,
                    data = SafeWorkflowEventData(completed.Data),
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            if (workflowEvent is ExecutorFailedEvent failed)
            {
                hasFailedExecutor = true;
                _run.AppendEvent(new
                {
                    type = "EXECUTOR_FAILED",
                    runId = _run.RunId,
                    executorId = failed.ExecutorId,
                    eventType = workflowEvent.GetType().Name,
                    error = failed.Data?.Message,
                    errorType = failed.Data?.GetType().FullName,
                    timestampUtc = DateTime.UtcNow
                });
                continue;
            }

            if (workflowEvent is WorkflowOutputEvent output)
            {
                /*
                 * MAF liefert Agent-Antworten als WorkflowOutputEvent. Bei Agenten
                 * können diese Events tokenweise auftreten. (im run beobachtet) Für die globale
                 * Timeline wären tausende einzelne WORKFLOW_OUTPUT-Zeilen nur Noise. 
                 * Deshalb werden sie jetzt pro Executor gesammelt und  am
                 * Ende in eine kompakte Summary geschrieben + eine Textdatei im Agent-Logordner.
                 */
                var text = SafeWorkflowEventText(output.Data);
                if (!string.IsNullOrEmpty(text))
                {
                    if (!outputBuffers.TryGetValue(output.ExecutorId, out var buffer))
                    {
                        buffer = new StringBuilder();
                        outputBuffers[output.ExecutorId] = buffer;
                    }

                    buffer.Append(text);
                }
                continue;
            }

            _run.AppendEvent(new
            {
                type = "WORKFLOW_EVENT",
                runId = _run.RunId,
                eventType = workflowEvent.GetType().Name,
                data = SafeWorkflowEventData(workflowEvent.Data),
                timestampUtc = DateTime.UtcNow
            });
        }

        WriteWorkflowOutputSummaries(outputBuffers);

        return hasFailedExecutor;
    }

    private static IEnumerable<AIAgent> EnumerateAgents(Phase2Agents agents)
    {
        yield return agents.Context;
        yield return agents.Requirements;
        yield return agents.Risks;
        yield return agents.Architecture;
        yield return agents.OpenQuestions;
    }

    private string ResolvePromptName(string? agentName)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            return "unknown";

        try
        {
            return _settings.GetPromptName(agentName);
        }
        catch (InvalidOperationException)
        {
            return "unknown";
        }
    }

    private static object? SafeWorkflowEventData(object? data)
    {
        if (data is null)
            return null;

        if (data is string or int or long or bool or double or decimal)
            return data;

        return data.ToString();
    }

    private void WriteWorkflowOutputSummaries(IReadOnlyDictionary<string, StringBuilder> outputBuffers)
    {
        foreach (var (executorId, buffer) in outputBuffers)
        {
            var outputText = buffer.ToString();
            var agentName = ResolveAgentNameFromExecutorId(executorId);
            var file = _run.WriteAgentTextFile(agentName, "workflow-output.txt", outputText);

            _run.AppendEvent(new
            {
                type = "WORKFLOW_OUTPUT_SUMMARY",
                runId = _run.RunId,
                executorId,
                agentName,
                charCount = outputText.Length,
                preview = TruncateForLog(outputText, 1000),
                file,
                timestampUtc = DateTime.UtcNow
            });

            _run.AppendAgentEvent(agentName, new
            {
                type = "WORKFLOW_OUTPUT_SUMMARY",
                runId = _run.RunId,
                executorId,
                agentName,
                charCount = outputText.Length,
                preview = TruncateForLog(outputText, 1000),
                file,
                timestampUtc = DateTime.UtcNow
            });
        }
    }

    private static string? SafeWorkflowEventText(object? data)
    {
        if (data is null)
            return null;

        return data switch
        {
            string text => text,
            _ => data.ToString()
        };
    }

    private static string? ResolveAgentNameFromExecutorId(string executorId)
    {
        // MAF setzt die ExecutorId als "<AgentName>_<Suffix>" (z.B. "Phase2ContextAgent_0").
        // Wir extrahieren den Teil vor dem ersten '_' um den Agent-Namen zu erhalten.
        // Wenn kein '_' vorhanden ist, gilt die gesamte ExecutorId als Agent-Name.
        // Ändert MAF das Format, liefert diese Methode lautlos falsche Namen..
        // das wäre in workflow-output.txt und WORKFLOW_OUTPUT_SUMMARY sichtbar.
        var separatorIndex = executorId.IndexOf('_', StringComparison.Ordinal);
        return separatorIndex <= 0 ? executorId : executorId[..separatorIndex];
    }

    private static string TruncateForLog(string value, int maxChars)
    {
        if (value.Length <= maxChars)
            return value;

        return value[..maxChars] + $"... <truncated len={value.Length}>";
    }

    private void WriteDiagnosis(object diagnosis)
    {
        File.WriteAllText(
            Path.Combine(_run.LogsDir, "diagnosis.json"),
            JsonSerializer.Serialize(diagnosis, new JsonSerializerOptions { WriteIndented = true })
        );
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        if (!Directory.Exists(sourceDir))
            return;

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var rel = Path.GetRelativePath(sourceDir, file);
            var dest = Path.Combine(targetDir, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(file, dest, overwrite: true);
        }
    }
}
