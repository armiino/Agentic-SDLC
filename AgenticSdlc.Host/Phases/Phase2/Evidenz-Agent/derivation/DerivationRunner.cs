using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// Verallgemeinerte Derivation-CLI: <c>derive &lt;specId&gt; &lt;source.artifact.json&gt; [model] [--dry-run]</c>.
/// Führt die config-gesteuerte Derivation-Familie als bindbaren MAF-Workflow aus
/// (Generate[Agent] → AnchorValidate[det] → InferenceCheck[Judge]). <c>specId</c> ∈ DerivationRegistry
/// (z. B. <c>derived-risks</c>, <c>requirements-gap</c>). Runs unter <c>runs/derivation/&lt;runId&gt;/</c>.
/// Exit: 0 = ok/dry-run, 2 = Usage/IO, 3 = Workflow-Fehler, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Generator = echter AIAgent (Prompt/Persona + Pipeline); Anker-Check = deterministisch; Inference-Check = bounded
/// Judge. Der Workflow ist per BindAsExecutor als EIN Derivation-Knoten in größere Graphen einhängbar. Der
/// Human-Review-Schritt bleibt separat (`derive-review`, generisch) — der Mensch als natürlicher Halt.
/// </remarks>
public static class DerivationRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine($"Usage: derive <specId> <source.artifact.json> [model] [--dry-run]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
            return 2;
        }

        var specId = args[1];
        if (!DerivationRegistry.TryGet(specId, out var spec))
        {
            Console.Error.WriteLine($"[derive] unbekannte specId '{specId}' (verfügbar: {string.Join(", ", DerivationRegistry.Specs.Keys)}).");
            return 2;
        }

        var sourcePath = Resolve(repoRoot, args[2]);
        if (sourcePath is null || !File.Exists(sourcePath)) { Console.Error.WriteLine($"[derive] Quelle fehlt: {args[2]}"); return 2; }

        string? modelArg = args.Length >= 4 && !args[3].StartsWith("--", StringComparison.Ordinal) ? args[3] : null;
        var dryRun = args.Contains("--dry-run");

        var source = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(sourcePath).ConfigureAwait(false), Json);
        if (source is null || source.Items.Count == 0) { Console.Error.WriteLine("[derive] Quelle leer/nicht lesbar (erwartet artifact.json mit items)."); return 2; }
        if (!string.Equals(source.ArtifactType, spec.SourceArtifactType, StringComparison.OrdinalIgnoreCase))
            Console.WriteLine($"[derive] HINWEIS: Quelltyp '{source.ArtifactType}' != erwartet '{spec.SourceArtifactType}' (spec {spec.Id}) — läuft trotzdem.");

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var run = new RunContext(RunId.New(), "derivation");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = $"Derivation-{spec.Id}", runId = run.RunId, spec = spec.Id,
            source = Path.GetRelativePath(repoRoot, sourcePath), sourceItems = source.Items.Count,
            provider = settings.LlmProvider, generatorModel = genSettings.ModelId, checkerModel = judgeSettings.ModelId,
            prompt = spec.PromptName, target = spec.TargetArtifactType, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[derive] runId={run.RunId} spec={spec.Id} ({spec.SourceArtifactType}->{spec.TargetArtifactType}) source={source.Items.Count} items genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Generator = echter AIAgent (Prompt + Pipeline + ToolLogger). Check = bounded Judge (Pipeline-Client).
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
        var checkClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, $"DerivationCheck-{spec.Id}", SourceName);

        var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
        agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        var checker = new InferenceChecker(checkClient, settings.JuryStructuredOutput);
        var generate = new DerivationGenerateExecutor(agent, spec, run);
        var anchor = new DerivationAnchorExecutor(spec, run);
        var check = new DerivationCheckExecutor(checker, spec, genSettings.ModelId, run);
        var workflow = DerivationWorkflow.Build(generate, anchor, check);

        if (dryRun)
        {
            Console.WriteLine($"[derive] --dry-run: Graph Build()-bar (Generate[Agent] → AnchorValidate[det] → InferenceCheck[Judge]) für spec '{spec.Id}'. Kein LLM.");
            Console.WriteLine($"[derive] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine("[derive] running derivation workflow...");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, source, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[derive] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk (der Check-Executor schreibt).
        var derivedPath = Path.Combine(run.RunDir, $"{spec.TargetArtifactType}.derived.json");
        var reportPath = Path.Combine(run.RunDir, "derivation-report.json");
        int items = -1, invalid = -1;
        if (File.Exists(derivedPath)) { using var d = JsonDocument.Parse(await File.ReadAllTextAsync(derivedPath).ConfigureAwait(false)); items = d.RootElement.GetProperty("items").GetArrayLength(); }
        if (File.Exists(reportPath)) { using var d = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false)); invalid = d.RootElement.GetProperty("invalidAnchor").GetInt32(); }
        Console.WriteLine($"[derive] fertig: {spec.TargetArtifactType} abgeleitet, anker-gültig={items}, anker-ungültig={invalid}");
        Console.WriteLine($"[derive] -> {spec.TargetArtifactType}.derived.json + inference-check-report.json  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
