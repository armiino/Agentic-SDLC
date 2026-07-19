using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate;

// CLI: pbi-update <ingestion-run> [model] [--dry-run]
// Maker: deterministische Ableitung (MARK/BLOCK/SUPERSEDE) + agentische Platzierung (EXTEND vs NEW_PBI) neuer
// Requirements -> PbiStateChangePlan -> Gate. Konsumiert die affected-items-view/das Delta eines Ingestion-Laufs.
public static class PbiUpdateRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "PbiPlacementAgent";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var dryRun = args.Contains("--dry-run");
        string? token = null, modelArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[pbi-update] unbekanntes Argument: {a}"); return 2; }
            if (token is null) token = a; else modelArg ??= a;
        }
        if (token is null) { Usage(); return 2; }

        var ingestPlanDir = IngestionReviewRunner.ResolvePlanDir(repoRoot, token);
        var deltaPath = ingestPlanDir is null ? null : Path.Combine(ingestPlanDir, "applied", "delta.json");
        if (deltaPath is null || !File.Exists(deltaPath))
        {
            Console.Error.WriteLine($"[pbi-update] Ingestion-Delta (applied/delta.json) fuer '{token}' nicht gefunden - erst ingest-apply fahren.");
            return 2;
        }
        var delta = await LoadAsync<IngestionApplyReport>(deltaPath).ConfigureAwait(false);
        var sourceIngestionRun = Path.GetFileName(Path.GetDirectoryName(ingestPlanDir!) ?? ingestPlanDir!);

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[pbi-update] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var derived = PbiUpdateDerivation.Derive(core, delta.Applied);
        var placements = new List<PbiStateChangeOperation>();

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "pbi-update");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        if (derived.Unplaced.Count > 0 && !dryRun)
        {
            using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
                Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
                settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
            var prompt = PromptProvider.Load(repoRoot, Phase, AgentName, "PbiPlacementAgent1", new Dictionary<string, string> { ["runId"] = run.RunId });
            var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AgentName, SourceName);
            var tools = new PbiPlacementTools(derived.Unplaced, core, run);
            var agent = client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools.Build()])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            var task = """
                       Ordne jedes NEUE Requirement (get_unplaced_requirements) GENAU EINEM PBI zu:
                       - EXTEND_PBI (mit pbiId), wenn ein bestehendes PBI dieselbe fachliche Aufgabe abdeckt.
                       - NEW_PBI (mit featureId), wenn es ein eigenes PBI braucht.
                       Nutze list_features / get_feature_pbis / get_pbi. featureHint ist nur ein Hinweis - waehle ein
                       ECHTES Feature (featureId) bzw. ein echtes PBI. Belege jede Platzierung (rationale). Speichere
                       genau einmal mit save_placements (requirementId + kind + pbiId ODER featureId).
                       """;
            try { await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: CancellationToken.None).ConfigureAwait(false); }
            catch (Exception ex) { Console.Error.WriteLine($"[pbi-update] Platzierungs-Agent fehlgeschlagen: {ex.Message}"); return 4; }
            placements = (tools.SavedPlacements ?? []).ToList();
        }

        var operations = derived.DeterministicOps.Concat(placements).ToList();
        var plan = new PbiStateChangePlanDocument(
            PbiStateChangePlanDocument.CurrentSchemaVersion, $"pbi-change-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            DateTime.UtcNow, sourceIngestionRun, operations);

        var unplacedIds = derived.Unplaced.Select(u => u.RequirementId).ToHashSet(StringComparer.Ordinal);
        var gate = PbiUpdateGate.Check(core, plan, unplacedIds);

        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "pbi-change-plan.json"), JsonSerializer.Serialize(plan, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "pbi-update-gate-report.json"), JsonSerializer.Serialize(gate, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "pbi-update-summary.json"), JsonSerializer.Serialize(new
        {
            run.RunId,
            sourceIngestionRun,
            deterministic = derived.DeterministicOps.Count,
            unplaced = derived.Unplaced.Count,
            placements = placements.Count,
            operations = operations.Count,
            gatePass = gate.Pass,
            gateErrors = gate.Errors.Count,
            byKind = operations.GroupBy(o => o.Kind, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal),
            dryRun,
            timestampUtc = DateTime.UtcNow
        }, Json)).ConfigureAwait(false);

        Console.WriteLine($"[pbi-update] deterministic={derived.DeterministicOps.Count} unplaced={derived.Unplaced.Count} placements={placements.Count} operations={operations.Count} gate={(gate.Pass ? "pass" : "fail")} errors={gate.Errors.Count}");
        Console.WriteLine($"[pbi-update] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return gate.Pass ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage() => Console.Error.WriteLine("Usage: pbi-update <ingestion-run> [modelId] [--dry-run]");
}
