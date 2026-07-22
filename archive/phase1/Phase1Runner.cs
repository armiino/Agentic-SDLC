using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Mcp;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenTelemetry;
using System.Diagnostics;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase1;

/// <summary>
/// Führt den kompletten Phase-1-Run aus.
/// </summary>
/// <remarks>
/// Der Runner ist bewusst nicht für die technische Erstellung des Agenten
/// verantwortlich. Das erledigt <see cref="Phase1AgentFactory"/>.
///
/// Diese Klasse beschreibt stattdessen den Ablauf einer Phase:
/// MCP verbinden, Tools entdecken, Agent starten, Fehler protokollieren,
/// Pflichtartefakte validieren, Approval-Datei erzeugen und am Ende einen Snapshot der erzeugten Dokumente sichern.
/// </remarks>
public sealed class Phase1Runner
{
    private const string AgentName = "Phase1SinglePass";

    private readonly HostSettings _settings;
    private readonly RunContext _run;
    private readonly string _sourceName;
    private readonly ActivitySource _activitySource;
    private readonly string _repoRoot;

    public Phase1Runner(
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
    /// Startet Phase 1 und liefert den Prozess-Exit-Code zurück.
    /// </summary>
    /// <remarks>
    /// Rückgabewerte bleiben identisch zur vorherigen Program.cs-Logik:
    /// 0 = erfolgreich, 2 = Pflichtartefakte fehlen, 3 = Agent-/Providerfehler.
    /// </remarks>
    public async Task<int> RunAsync()
    {
        _run.AppendEvent(new { type = "RUN_STARTED", runId = _run.RunId, timestampUtc = DateTime.UtcNow });

        /*
         * Baggage setzt Kontextinformationen, die OpenTelemetry an untergeordnete
         * Spans weiterreichen kann. So ist später in den OTel-Logs erkennbar,
         * zu welchem Run, welcher Phase und welchem Agenten ein technischer Vorgang gehört.
         */
        Baggage.SetBaggage("run.id", _run.RunId);
        Baggage.SetBaggage("phase", Phase1Artifacts.PhaseName);
        Baggage.SetBaggage("agent.name", AgentName);

        using var runSpan = _activitySource.StartActivity("run.phase1.level1", ActivityKind.Internal);
        runSpan?.SetTag("run.id", _run.RunId);
        runSpan?.SetTag("phase", Phase1Artifacts.PhaseName);
        runSpan?.SetTag("agent.name", AgentName);
        runSpan?.SetTag("agent.prompt.name", _settings.GetPromptName(AgentName));
        runSpan?.SetTag("llm.provider", _settings.LlmProvider);
        runSpan?.SetTag("gen_ai.request.model", _settings.ModelId);

        await using var localMcp = await ConnectLocalMcpAsync();

        var githubConnectError = await TryDiscoverGitHubToolsAsync();
        var localTools = await LoadLocalToolsAsync(localMcp, githubConnectError);
        var agent = CreateAgent(localTools);

        var agentExitCode = await RunAgentAsync(agent, runSpan);
        if (agentExitCode != 0)
            return agentExitCode;

        var validator = new Phase1ArtifactValidator(
            run: _run,
            activitySource: _activitySource,
            repoRoot: _repoRoot
        );

        var validationExitCode = validator.ValidateRequiredDocs();
        if (validationExitCode != 0)
            return validationExitCode;

        var approvalRecorder = new Phase1ApprovalRecorder(_run);
        approvalRecorder.RecordApprovalIfNeeded();

        CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);

        _run.AppendEvent(new { type = "RUN_FINISHED", runId = _run.RunId, status = "completed", timestampUtc = DateTime.UtcNow });
        return 0;
    }

    private async Task<string?> TryDiscoverGitHubToolsAsync()
    {
        /*
         * GitHub-MCP ist in Phase 1 nur vorbereitet. Ein Fehler darf den Run
         * deshalb nicht abbrechen. Er wird protokolliert und später in
         * tool-discovery.json sichtbar gemacht.
         */
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

    private async Task<McpClient> ConnectLocalMcpAsync()
    {
        /*
         * Der lokale MCP-Server stellt die Dateisystem-Tools bereit.
         * Diese Verbindung bleibt für den gesamten Phase-1-Run offen, damit die
         * entdeckten Tools vom Agenten ausgeführt werden koennen.
         */
        var localMcp = await McpConnections.ConnectLocalAsync();
        _run.AppendEvent(new { type = "MCP_LOCAL_CONNECTED", runId = _run.RunId, timestampUtc = DateTime.UtcNow });
        return localMcp;
    }

    private async Task<List<AITool>> LoadLocalToolsAsync(McpClient localMcp, string? githubConnectError)
    {
        /*
         * Die lokalen MCP-Tools werden in Microsoft.Extensions.AI-Tools
         * umgewandelt. Erst dadurch kann der MAF-Agent diese Funktionen als
         * echte Tools verwenden.
         */
        var localTools = (await localMcp.ListToolsAsync().ConfigureAwait(false))
            .Cast<AITool>()
            .ToList();

        _run.WriteToolDiscovery(new
        {
            runId = _run.RunId,
            local = new { count = localTools.Count, tools = localTools.Select(t => t.Name).ToArray() },
            github = new { error = githubConnectError }
        });

        _run.AppendEvent(new { type = "TOOLS_DISCOVERED", runId = _run.RunId, localCount = localTools.Count, timestampUtc = DateTime.UtcNow });
        return localTools;
    }

    private AIAgent CreateAgent(IReadOnlyList<AITool> localTools)
    {
        var agent = Phase1AgentFactory.CreateSinglePassAgent(
            settings: _settings,
            run: _run,
            sourceName: _sourceName,
            runId: _run.RunId,
            localTools: localTools
        );

        _run.AppendEvent(new
        {
            type = "AGENT_STARTED",
            runId = _run.RunId,
            agentName = AgentName,
            promptName = _settings.GetPromptName(AgentName),
            allowedTools = localTools.Select(t => t.Name).ToArray(),
            timestampUtc = DateTime.UtcNow
        });

        return agent;
    }

    private async Task<int> RunAgentAsync(AIAgent agent, Activity? runSpan)
    {
        /*
         * MaxOutputTokens begrenzt die Antwortlänge.
         * Diese Einstellung wurde eingeführt, weil lokale Ollama-Streams bei sehr langen Antworten instabil werden können.
         * Die eigentliche Agentenlogik bleibt davon unberührt.
         */
        var runOptions = new ChatClientAgentRunOptions(new ChatOptions
        {
            Temperature = 0.2f,
            MaxOutputTokens = 2048
        });

        try
        {
            var result = await agent.RunAsync("Begin.", options: runOptions);
            Console.WriteLine(result);
            return 0;
        }
        catch (Exception ex)
        {
            /*
             * Provider-, Streaming- oder Frameworkfehler werden als Run-Fehler gespeichert.
             * Dadurch endet der Prozess nicht mehr als: 
             * unhandled exception, sondern mit diagnosis.json und verwertbarem Exit-Code.
             */
            runSpan?.AddException(ex);
            runSpan?.SetStatus(ActivityStatusCode.Error, ex.Message);

            _run.AppendEvent(new
            {
                type = "RUN_FAILED",
                runId = _run.RunId,
                reason = "Agent run threw exception before DoD validation",
                errorType = ex.GetType().FullName,
                error = ex.Message,
                timestampUtc = DateTime.UtcNow
            });

            var diagnosis = new
            {
                runId = _run.RunId,
                phase = Phase1Artifacts.PhaseName,
                status = "FAILED",
                rootCause = new
                {
                    code = "AGENT_RUN_EXCEPTION",
                    message = "The agent run failed before artifact validation could complete.",
                    errorType = ex.GetType().FullName,
                    error = ex.Message
                },
                hints = new[]
                {
                    "If error mentions Done=true, Ollama/OllamaSharp returned an incomplete chat stream.",
                    "Check events.jsonl for the last successful tool call before CHAT_FAILED.",
                    "This is a provider/runtime failure, not an fs_write validation failure."
                },
                timestampUtc = DateTime.UtcNow
            };

            WriteDiagnosis(diagnosis);
            CopyDirectory(Path.Combine(_repoRoot, "docs"), _run.DocsSnapshotDir);

            Console.Error.WriteLine("RUN FAILED - agent run exception:");
            Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return 3;
        }
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
