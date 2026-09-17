using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// CLI: github-forward <pbi-update-run> [model] [--issues <snapshot.json>] [--repo owner/name] [--dry-run]
// Forward-Maker als MAF-Workflow (Seed -> Maker -> Gate -> Finalize, austauschbare Executor-Knoten, siehe
// GithubForwardWorkflow). Ergebnis: GithubForwardPlan -> Review (T3.3) -> gated Apply (T3.4).
public static class GithubForwardRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "GithubForwardAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        var maxAttempts = 2;
        string? token = null, modelArg = null, issuesArg = null, repoArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--issues", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { issuesArg = args[++i]; continue; }
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (string.Equals(a, "--max-attempts", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out var ma)) { maxAttempts = Math.Max(1, ma); i++; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) { Usage(); return 2; }

        // 1) github-sync-Delta des pbi-update-Laufs.
        var planDir = PbiUpdateReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = planDir is null ? null : Path.Combine(planDir, "applied", "github-sync-delta.json");
        if (deltaPath is null || !File.Exists(deltaPath))
        {
            Console.Error.WriteLine($"[github-forward] github-sync-delta.json fuer '{token}' nicht gefunden - erst pbi-update-apply fahren.");
            return 2;
        }
        var delta = await LoadAsync<GithubSyncDeltaDocument>(deltaPath).ConfigureAwait(false);
        var sourcePbiUpdateRun = Path.GetFileName(Path.GetDirectoryName(planDir!) ?? planDir!);

        // 2) Core-Mappings (Dedup-Basis, T3.1).
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-forward] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        // 3) Issue-Snapshot (T3.2): explizit oder juengster github-snapshot; sonst leer.
        var snapshotPath = ResolveSnapshotPath(repoRoot, issuesArg);
        GithubSnapshotGuard.Verify(snapshotPath, repoArg);   // B1/R-16: fremde/ungestempelte Snapshots LAUT ablehnen
        IReadOnlyList<GithubIssueSnapshot> issues = [];
        if (snapshotPath is not null) issues = await GithubReadSource.LoadAsync(snapshotPath).ConfigureAwait(false);
        else Console.WriteLine("[github-forward] WARN: kein Issue-Snapshot - unmapped PBIs koennen nur CREATE (keine LINK-Kandidaten).");

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "github-forward");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        var snapshotRel = snapshotPath is null ? null : Path.GetRelativePath(repoRoot, snapshotPath);

        using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
            Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Agent-Factory (Maker-Knoten): Instruktionen + Middleware gebacken, Tools zur Laufzeit.
        var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "GithubForwardAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var workflow = GithubForwardWorkflow.Build(
            new GithubForwardSeedExecutor(run, repoRoot),
            new GithubForwardMakerExecutor(factory, run),
            new GithubForwardGateExecutor(run),
            new GithubForwardRepairExecutor(factory, run),
            new GithubForwardFinalizeExecutor(run));

        var ctx = new GithubForwardWfContext(core, delta.Entries, mappingByPbi, issues, repoArg, sourcePbiUpdateRun, outDir, snapshotRel, dryRun, maxAttempts);

        Console.WriteLine($"[github-forward] running runId={run.RunId} deltaPbis={delta.Entries.Count} model={genSettings.ModelId} dryRun={dryRun} maxAttempts={maxAttempts} (Seed->Maker->Gate->[Repair]/Finalize)");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, ctx, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-forward] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var summaryPath = Path.Combine(outDir, "github-forward-summary.json");
        if (!File.Exists(summaryPath)) { Console.Error.WriteLine("[github-forward] Ausfuehrung unvollstaendig: summary fehlt."); return 4; }
        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
        var s = summary.RootElement;
        var gatePass = s.GetProperty("gatePass").GetBoolean();
        Console.WriteLine($"[github-forward] deltaPbis={s.GetProperty("deltaPbis").GetInt32()} deterministic={s.GetProperty("deterministic").GetInt32()} unmapped={s.GetProperty("unmapped").GetInt32()} agentOps={s.GetProperty("agentOps").GetInt32()} ops={s.GetProperty("operations").GetInt32()} gate={(gatePass ? "pass" : "fail")} errors={s.GetProperty("gateErrors").GetInt32()} attempts={s.GetProperty("attempts").GetInt32()} finalDecision={s.GetProperty("finalDecision").GetString()}");
        var gate = await LoadAsync<GithubForwardGateReport>(Path.Combine(outDir, "github-forward-gate-report.json")).ConfigureAwait(false);
        foreach (var e in gate.Errors) Console.WriteLine($"[github-forward]   ERROR {e.Code} {e.PbiId}: {e.Message}");
        Console.WriteLine($"[github-forward] -> {Path.GetRelativePath(repoRoot, outDir)} (danach: github-forward-review)");
        return gatePass ? 0 : 1;
    }

    private static string? ResolveSnapshotPath(string repoRoot, string? issuesArg)
    {
        if (!string.IsNullOrWhiteSpace(issuesArg))
            return Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg);
        var root = Path.Combine(repoRoot, "runs", "github-snapshot");
        if (!Directory.Exists(root)) return null;
        return Directory.EnumerateFiles(root, "github-issues-snapshot.json", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, JsonFiles.Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: github-forward <pbi-update-run> [modelId] [--issues <snapshot.json>] [--repo owner/name] [--dry-run]");
}
