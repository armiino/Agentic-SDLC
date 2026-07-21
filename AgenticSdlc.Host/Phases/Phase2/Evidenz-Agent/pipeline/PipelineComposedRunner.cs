using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Pipeline;

// S6 (Worklist 20.07) — A: EIN MAF-Lauf ueber ZWEI Stufen (ingest -> pbi-update) mit ZWEI Human-Gates,
// prozessuebergreifend resumebar. Ablauf: start -> resume (Gate 1) -> resume (Gate 2).
//   pipeline-hitl start  <meeting-delta project-state.json> [model] [--max-attempts n]
//   pipeline-hitl resume <pipeline-run> [--accept-all | --accept ID,ID]
// Die Stufen-HITL-Runner (ingest-requirements-hitl, pbi-update-hitl) bleiben eigenstaendig; A ist die Komposition.
public static class PipelineComposedRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => await StartAsync(args, settings, repoRoot).ConfigureAwait(false),
            "resume" => await ResumeAsync(args, settings, repoRoot).ConfigureAwait(false),
            _ => Usage(),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  pipeline-hitl start  <meeting-delta project-state.json> [model] [--max-attempts n]");
        Console.Error.WriteLine("  pipeline-hitl resume <pipeline-run> [--accept-all | --accept ID,ID]   (je Aufruf ein Gate)");
        return 2;
    }

    private static Workflow BuildWorkflow(
        RunContext run, string repoRoot, string ingestOutDir, string pbiOutDir, int maxAttempts,
        Func<IReadOnlyList<AITool>, AIAgent> ingestFactory, Func<IReadOnlyList<AITool>, AIAgent> pbiFactory)
    {
        ICandidateRetriever retriever = new ShowAllRequirementRetriever();
        var ingestPort = RequestPort.Create<IngestionReviewRequest, IngestionReviewResponse>("ingest-gate");
        var pbiPort = RequestPort.Create<PbiUpdateReviewRequest, PbiUpdateReviewResponse>("pbi-gate");
        return PipelineComposedWorkflow.Build(
            new IngestionHitlResolveExecutor(ingestFactory, retriever, run), new IngestionGateExecutor(run), new IngestionRepairExecutor(ingestFactory, retriever, run),
            new IngestionHitlFinalizeExecutor(run, ingestOutDir), ingestPort, new IngestComposedApplyExecutor(run, repoRoot, ingestOutDir),
            new IngestPbiBridgeExecutor(run, repoRoot, pbiOutDir, maxAttempts),
            new PbiUpdateDeriveExecutor(run), new PbiUpdateMakerExecutor(pbiFactory, run), new PbiUpdateGateExecutor(run), new PbiUpdateRepairExecutor(pbiFactory, run),
            new PbiUpdateHitlFinalizeExecutor(run), pbiPort, new PbiUpdateApplyExecutor(run, repoRoot, pbiOutDir));
    }

    // ---------------- START (Prozess A): bis zum Gate 1 (Ingest), Checkpoint, Pause. ----------------
    private static async Task<int> StartAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var maxAttempts = 2;
        string? deltaToken = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pipeline-hitl] unbekanntes Argument: {a}"); return 2; }
            if (deltaToken is null) deltaToken = a; else modelArg ??= a;
        }
        if (deltaToken is null) return Usage();

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pipeline-hitl] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var deltaPath = Path.IsPathRooted(deltaToken) ? deltaToken : Path.Combine(repoRoot, deltaToken);
        if (!File.Exists(deltaPath)) { Console.Error.WriteLine($"[pipeline-hitl] MeetingDelta nicht gefunden: {deltaPath}"); return 2; }
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;
        var deltaRel = Path.GetRelativePath(repoRoot, deltaPath);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "pipeline");
        run.EnsureFolders();
        var ingestOutDir = run.OutputDir("ingest");
        var pbiOutDir = run.OutputDir("pbi-update");
        var checkpointDir = run.OutputDir("checkpoints");

        var workflow = BuildWorkflow(run, repoRoot, ingestOutDir, pbiOutDir, maxAttempts,
            AgentFactory(repoRoot, settings, genSettings, run, "RequirementIngestionAgent", "RequirementIngestionAgent1"),
            AgentFactory(repoRoot, settings, genSettings, run, "PbiPlacementAgent", "PbiPlacementAgent1"));

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        Console.WriteLine($"[pipeline-hitl] start runId={run.RunId} model={genSettings.ModelId} maxAttempts={maxAttempts} (Ingest -> [Gate1] -> Apply -> Bridge -> PbiUpdate -> [Gate2] -> Apply)");
        CheckpointInfo? pending = null;
        try
        {
            await using var runHandle = await InProcessExecution.RunStreamingAsync(workflow, new IngestionResolveInput(delta, core, deltaRel, maxAttempts), manager, run.RunId).ConfigureAwait(false);
            await foreach (var evt in runHandle.WatchStreamAsync().ConfigureAwait(false))
            {
                if (evt is RequestInfoEvent) Console.WriteLine("[pipeline-hitl] Gate 1 (Ingest) erreicht.");
                if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
                {
                    if (info.Checkpoint is { } cp) pending = cp;
                    if (info.HasPendingRequests && pending is not null) break;
                }
                if (evt is WorkflowOutputEvent outEvt && outEvt.Data is IngestionResult manual)
                {
                    Console.WriteLine($"[pipeline-hitl] Ingest-Gate NICHT bestanden ({manual.FinalDecision}) - Pipeline-Abbruch.");
                    return 1;
                }
            }
        }
        catch (Exception ex) { Console.Error.WriteLine($"[pipeline-hitl] fehlgeschlagen: {ex.Message}"); run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message }); return 4; }

        if (pending is null) { Console.Error.WriteLine("[pipeline-hitl] kein Checkpoint mit offenem Gate erzeugt."); return 4; }
        await WritePointer(checkpointDir, run.RunId, pending).ConfigureAwait(false);
        Console.WriteLine($"[pipeline-hitl] PAUSIERT an Gate 1. Fortsetzen: pipeline-hitl resume {run.RunId} --accept-all");
        return 0;
    }

    // ---------------- RESUME (Prozess B/C): auf das offene Gate antworten; pausiert am naechsten Gate oder terminiert. ----------------
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        string? runToken = null, acceptList = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--accept-all", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--accept", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { acceptList = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pipeline-hitl] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var runDir = ResolveRunDir(repoRoot, runToken);
        if (runDir is null) { Console.Error.WriteLine($"[pipeline-hitl] Lauf '{runToken}' nicht gefunden."); return 2; }
        var runId = Path.GetFileName(runDir);
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (!File.Exists(pointerPath)) { Console.Error.WriteLine("[pipeline-hitl] pointer.json fehlt - kein pausierter Lauf."); return 2; }
        var pointer = await LoadAsync<PointerFile>(pointerPath).ConfigureAwait(false);

        var acceptListItems = string.IsNullOrWhiteSpace(acceptList) ? [] : acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        var run = new RunContext(runId, "pipeline");
        var ingestOutDir = run.OutputDir("ingest");
        var pbiOutDir = run.OutputDir("pbi-update");
        Func<IReadOnlyList<AITool>, AIAgent> noAgent = _ => throw new InvalidOperationException("Maker darf beim Resume nicht laufen.");
        var workflow = BuildWorkflow(run, repoRoot, ingestOutDir, pbiOutDir, 2, noAgent, noAgent);

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, Json);
        Console.WriteLine($"[pipeline-hitl] resume runId={runId}");
        await using var runHandle = await InProcessExecution.OpenStreamingAsync(workflow, manager, runId).ConfigureAwait(false);
        await runHandle.RestoreCheckpointAsync(new CheckpointInfo(pointer.SessionId, pointer.CheckpointId)).ConfigureAwait(false);

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var responded = false;
        CheckpointInfo? nextPending = null;
        await foreach (var evt in runHandle.WatchStreamAsync(cts.Token).ConfigureAwait(false))
        {
            if (evt is RequestInfoEvent req && !responded)
            {
                // Port-Identitaet (nicht Datentyp!) entscheidet: die Request-JSON-Formen sind kompatibel, TryGetDataAs
                // wuerde falsch matchen. PortId ist eindeutig ("ingest-gate" / "pbi-gate").
                var portId = req.Request.PortInfo.PortId;
                if (string.Equals(portId, "ingest-gate", StringComparison.Ordinal) && req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir))
                {
                    var acc = acceptAll ? ir!.Ops.Select(o => o.IncomingItemId).ToList() : acceptListItems;
                    await runHandle.SendResponseAsync(req.Request.CreateResponse(new IngestionReviewResponse(acc, "author (cli)"))).ConfigureAwait(false);
                    Console.WriteLine($"[pipeline-hitl] Gate 1 (Ingest) beantwortet: {acc.Count} akzeptiert.");
                }
                else if (string.Equals(portId, "pbi-gate", StringComparison.Ordinal) && req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr))
                {
                    var acc = acceptAll ? pr!.Ops.Select(o => o.OpId).ToList() : acceptListItems;
                    await runHandle.SendResponseAsync(req.Request.CreateResponse(new PbiUpdateReviewResponse(acc, "author (cli)"))).ConfigureAwait(false);
                    Console.WriteLine($"[pipeline-hitl] Gate 2 (PbiUpdate) beantwortet: {acc.Count} akzeptiert.");
                }
                responded = true;
            }
            else if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
            {
                if (info.Checkpoint is { } cp) nextPending = cp;
                if (responded && info.HasPendingRequests && nextPending is not null) break; // naechstes Gate
            }
            else if (evt is WorkflowOutputEvent outEvt)
            {
                if (outEvt.Data is PbiUpdateApplyReport report)
                {
                    Console.WriteLine($"[pipeline-hitl] PIPELINE FERTIG. pbi-update applied: newPbis={report.NewPbis.Count} updatedPbis={report.UpdatedPbis.Count}");
                    return 0;
                }
                if (outEvt.Data is PbiUpdateWfResult manual)
                {
                    Console.WriteLine($"[pipeline-hitl] Pbi-Gate NICHT bestanden ({manual.FinalDecision}) - Abbruch nach Ingest-Apply.");
                    return 1;
                }
            }
        }

        if (nextPending is not null && responded)
        {
            await WritePointer(checkpointDir, runId, nextPending).ConfigureAwait(false);
            Console.WriteLine($"[pipeline-hitl] PAUSIERT am naechsten Gate. Fortsetzen: pipeline-hitl resume {runId} --accept-all");
            return 0;
        }
        Console.Error.WriteLine("[pipeline-hitl] Resume beendet ohne naechstes Gate/Terminal (Timeout?).");
        return 4;
    }

    private static Func<IReadOnlyList<AITool>, AIAgent> AgentFactory(string repoRoot, HostSettings settings, HostSettings genSettings, RunContext run, string agentName, string promptName)
    {
        var prompt = PromptProvider.Load(repoRoot, Phase, agentName, promptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, agentName, SourceName);
        return tools => client.AsAIAgent(instructions: prompt, name: agentName, tools: [.. tools]).AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
    }

    private static async Task WritePointer(string checkpointDir, string runId, CheckpointInfo cp)
        => await File.WriteAllTextAsync(Path.Combine(checkpointDir, "pointer.json"),
            JsonSerializer.Serialize(new PointerFile(runId, cp.SessionId, cp.CheckpointId, DateTime.UtcNow), Json)).ConfigureAwait(false);

    private static string? ResolveRunDir(string repoRoot, string token)
    {
        var direct = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, "runs", "pipeline", token);
        if (Directory.Exists(direct)) return direct;
        var root = Path.Combine(repoRoot, "runs", "pipeline");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateDirectories(root).FirstOrDefault(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private sealed record PointerFile(string RunId, string SessionId, string CheckpointId, DateTime SavedUtc);
}
