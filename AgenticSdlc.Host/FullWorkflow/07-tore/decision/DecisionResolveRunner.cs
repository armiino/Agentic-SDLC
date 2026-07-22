using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision;

// CLI: decision-resolve <resolution-input.json>
// T2.1 Maker (deterministisch, KEIN LLM) als MAF-Workflow: Derive -> Gate -> Finalize (dieselben Executor-Knoten,
// die der agentische decision-resolve-agent hinter dem Maker wiederverwendet).
public static class DecisionResolveRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: decision-resolve <resolution-input.json>"); return 2; }
        var inputPath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(inputPath)) { Console.Error.WriteLine($"[decision-resolve] Input nicht gefunden: {inputPath}"); return 2; }
        var input = await LoadAsync<DecisionResolutionInput>(inputPath).ConfigureAwait(false);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[decision-resolve] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var run = new RunContext(RunId.New(), "decision");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        var workflow = DecisionResolveWorkflow.BuildDeterministic(
            new DecisionDeriveExecutor(run), new DecisionGateExecutor(run), new DecisionFinalizeExecutor(run));
        // Deterministisch: maxAttempts=1 -> das geteilte Gate gibt nie Repair (kein Maker/Repair-Knoten in diesem Graph).
        var ctx = new DecisionWfContext(core, outDir, MaxAttempts: 1, StakeholderAnswer: null);

        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new DecisionResolveInputMsg(ctx, input, Attempt: 1, Source: "maker", History: []), run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) { Console.Error.WriteLine($"[decision-resolve] Ausfuehrung fehlgeschlagen: {ex.Message}"); return 4; }

        return await ReportAsync(repoRoot, outDir, "decision-resolve").ConfigureAwait(false);
    }

    // Gemeinsame Ergebnis-Ausgabe fuer decision-resolve und decision-resolve-agent.
    internal static async Task<int> ReportAsync(string repoRoot, string outDir, string tag)
    {
        var summaryPath = Path.Combine(outDir, "decision-summary.json");
        if (!File.Exists(summaryPath)) { Console.Error.WriteLine($"[{tag}] Ausfuehrung unvollstaendig: summary fehlt."); return 4; }
        using var summary = JsonDocument.Parse(await File.ReadAllTextAsync(summaryPath).ConfigureAwait(false));
        var s = summary.RootElement;
        var gatePass = s.GetProperty("gatePass").GetBoolean();
        var byOutcome = string.Join(", ", s.GetProperty("byOutcome").EnumerateObject().Select(p => $"{p.Name}={p.Value.GetInt32()}"));
        Console.WriteLine($"[{tag}] resolutions={s.GetProperty("resolutions").GetInt32()} ops={s.GetProperty("ops").GetInt32()} problems={s.GetProperty("problems").GetInt32()} gate={(gatePass ? "pass" : "fail")} errors={s.GetProperty("gateErrors").GetInt32()} attempts={s.GetProperty("attempts").GetInt32()} finalDecision={s.GetProperty("finalDecision").GetString()}");
        if (byOutcome.Length > 0) Console.WriteLine($"[{tag}] byOutcome: {byOutcome}");
        Console.WriteLine($"[{tag}] -> {Path.GetRelativePath(repoRoot, outDir)} (danach: decision-review)");
        return gatePass ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }
}
