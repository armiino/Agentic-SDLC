using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Load;
using AgenticSdlc.Host.Phases.Phase2.Ledger;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Recipes;

/// <summary>
/// Der Rezept-Assembler-Runner (CLI: <c>recipe &lt;recipe.json&gt; [model] [--dry-run]</c>): liest ein deklaratives
/// <see cref="Recipe"/>, validiert es und montiert daraus zur Laufzeit den MAF-Graphen (Baseline build|load →
/// 0..N Ableitungen). Verallgemeinert <c>evidence-chain</c> auf mehrere Ableitungen und beide Baseline-Modi.
/// Runs unter <c>runs/recipe/&lt;runId&gt;/</c>. Exit: 0 = ok/dry-run, 2 = Usage/IO/Validierung, 3 = Workflow-Fehler,
/// 4 = Konfig/LLM-Fehler.
/// </summary>
/// <remarks>
/// Spec-referenziert: eine Ableitung nennt eine registrierte <see cref="DerivationSpec"/>-Id; deren Quelltypen müssen
/// ⊆ baseline.artifacts sein. Ziele je Rezept müssen distinkt sein (sonst <c>{target}.derived.json</c>-Kollision).
/// Später kann ein Manager-Agent genau diese Rezept-Struktur ausgeben — die Grund-Config bleibt die Wahrheit.
/// </remarks>
public static class RecipeRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private static readonly JsonSerializerOptions JsonRead = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: recipe <recipe.json> [model] [--dry-run]"); return 2; }
        var recipePath = Resolve(repoRoot, args[1]);
        if (recipePath is null || !File.Exists(recipePath)) { Console.Error.WriteLine($"[recipe] Rezept-Datei fehlt: {args[1]}"); return 2; }
        string? modelArg = args.Length >= 3 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        var dryRun = args.Contains("--dry-run");

        Recipe? recipe;
        try { recipe = JsonSerializer.Deserialize<Recipe>(await File.ReadAllTextAsync(recipePath).ConfigureAwait(false), JsonRead); }
        catch (JsonException ex) { Console.Error.WriteLine($"[recipe] JSON ungültig: {ex.Message}"); return 2; }
        if (recipe?.Baseline is null) { Console.Error.WriteLine("[recipe] 'baseline' fehlt."); return 2; }

        var mode = (recipe.Baseline.Mode ?? "build").Trim().ToLowerInvariant();
        if (mode is not ("build" or "load")) { Console.Error.WriteLine($"[recipe] baseline.mode '{mode}' ungültig (build|load)."); return 2; }
        var loadMode = mode == "load";

        var baselineArtifacts = (recipe.Baseline.Artifacts ?? [])
            .Select(a => a.Trim().ToLowerInvariant()).Where(a => a.Length > 0).Distinct().ToList();
        if (baselineArtifacts.Count == 0) { Console.Error.WriteLine("[recipe] baseline.artifacts leer."); return 2; }

        // Ableitungen auflösen (Registry).
        var specs = new List<DerivationSpec>();
        foreach (var d in recipe.Derivations ?? [])
        {
            if (!DerivationRegistry.TryGet(d.Spec, out var spec))
            { Console.Error.WriteLine($"[recipe] unbekannte derivation spec '{d.Spec}' (verfügbar: {string.Join(", ", DerivationRegistry.Specs.Keys)})."); return 2; }
            specs.Add(spec);
        }

        // Validierung: jede Ableitungs-Quelle ⊆ baseline.artifacts; Ziele distinkt.
        foreach (var spec in specs)
        {
            var missing = spec.SourceArtifactTypes.Where(t => !baselineArtifacts.Contains(t)).ToList();
            if (missing.Count > 0)
            { Console.Error.WriteLine($"[recipe] spec '{spec.Id}': Quelle(n) [{string.Join(",", missing)}] nicht in baseline.artifacts [{string.Join(",", baselineArtifacts)}]."); return 2; }
        }
        var dupTargets = specs.GroupBy(s => s.TargetArtifactType, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupTargets.Count > 0)
        { Console.Error.WriteLine($"[recipe] mehrere Ableitungen mit gleichem Ziel: [{string.Join(", ", dupTargets)}] → derived.json-Kollision. Distinkte Ziele je Rezept."); return 2; }

        // Modus-Vorbedingungen.
        string? sourceDir = null, consPath = null;
        ConsumableLedger? ledger = null;
        if (loadMode)
        {
            if (string.IsNullOrWhiteSpace(recipe.Baseline.FromRun)) { Console.Error.WriteLine("[recipe] mode:load braucht baseline.fromRun."); return 2; }
            sourceDir = ResolveExistingDir(repoRoot, recipe.Baseline.FromRun!) ?? ResolveRunDir(repoRoot, recipe.Baseline.FromRun!);
            if (sourceDir is null) { Console.Error.WriteLine($"[recipe] fromRun '{recipe.Baseline.FromRun}' nicht gefunden (Pfad oder runId unter runs/)."); return 2; }
            var miss = baselineArtifacts.Where(t => !Chain.EvidenceChainRunner.BaselineExists(sourceDir, t)).ToList();
            if (miss.Count > 0) { Console.Error.WriteLine($"[recipe] fromRun: fehlende Baseline(s) in '{Path.GetRelativePath(repoRoot, sourceDir)}': {string.Join(", ", miss.Select(t => $"baselines/{t}/artifact.json"))}."); return 2; }
        }
        else
        {
            var unknown = baselineArtifacts.Where(t => !BaselineFanOutRunner.ArtifactMap.ContainsKey(t)).ToList();
            if (unknown.Count > 0) { Console.Error.WriteLine($"[recipe] kein Evidence-Agent für Quelltyp(en): {string.Join(", ", unknown)} (verfügbar: {string.Join(", ", BaselineFanOutRunner.ArtifactMap.Keys)})."); return 2; }
            if (!string.Equals(settings.EvidenceSource, "ledger", StringComparison.OrdinalIgnoreCase)) { Console.Error.WriteLine("[recipe] Arm B: setze evidenceAgent.source=ledger in run-config.json."); return 4; }
            consPath = Resolve(repoRoot, settings.EvidenceLedgerRun);
            if (consPath is null || !File.Exists(consPath)) { Console.Error.WriteLine($"[recipe] consumable.json fehlt: '{settings.EvidenceLedgerRun}'."); return 2; }
            ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
            if (ledger is null || ledger.Claims.Count == 0) { Console.Error.WriteLine("[recipe] consumable leer."); return 2; }
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;
        int k = 3, minVotes = 0, maxIter = 3;

        var run = new RunContext(RunId.New(), "recipe");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = RecipeWorkflow.WorkflowName, runId = run.RunId, mode,
            baselineArtifacts,
            derivations = specs.Select(s => new { s.Id, sources = s.SourceArtifactTypes, target = s.TargetArtifactType }),
            consumable = consPath is not null ? Path.GetRelativePath(repoRoot, consPath) : null,
            claims = ledger?.Claims.Count ?? 0,
            fromRun = sourceDir is not null ? Path.GetRelativePath(repoRoot, sourceDir) : null,
            recipe = Path.GetRelativePath(repoRoot, recipePath),
            provider = settings.LlmProvider, generatorModel = genSettings.ModelId, checkerModel = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[recipe] runId={run.RunId} mode={mode} baseline=[{string.Join(",", baselineArtifacts)}] derivations=[{string.Join(",", specs.Select(s => s.Id))}] genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var judgeBase = ChatClientFactory.Create(judgeSettings);

        // Ableitungs-Workflows bauen (je Spec: echter Agent + deterministischer Anker + bounded Judge).
        var derivations = new List<(DerivationSpec Spec, Microsoft.Agents.AI.Workflows.Workflow Derivation)>();
        foreach (var spec in specs)
        {
            var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
            var checkClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"DerivationCheck-{spec.Id}", SourceName);
            var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
            var judgeSystemPrompt = string.IsNullOrWhiteSpace(spec.JudgePromptName) ? null : PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.JudgePromptName!, new Dictionary<string, string>());
            AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
            agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            var deriv = DerivationWorkflow.Build(
                new DerivationGenerateExecutor(agent, spec, run),
                new DerivationAnchorExecutor(spec, run),
                new DerivationCheckExecutor(new InferenceChecker(checkClient, settings.JuryStructuredOutput, systemPrompt: judgeSystemPrompt), spec, genSettings.ModelId, run, $"derivations/{spec.Id}"));
            derivations.Add((spec, deriv));
        }

        // Baseline-Quelle + Assembler.
        Microsoft.Agents.AI.Workflows.Workflow workflow;
        if (loadMode)
        {
            var load = new LoadBaselineExecutor(baselineArtifacts, sourceDir!, run);
            workflow = RecipeWorkflow.BuildFromLoad(load, derivations, run);
        }
        else
        {
            var makerBase = ChatClientFactory.Create(settings);
            var branches = new List<(string ArtifactType, Microsoft.Agents.AI.Workflows.Workflow Branch)>();
            foreach (var t in baselineArtifacts)
            {
                var (agentName, disposition) = BaselineFanOutRunner.ArtifactMap[t];
                branches.Add((t, BaselineFanOutRunner.BuildBranch(
                    t, agentName, disposition, ledger!, k, minVotes, maxIter,
                    settings, judgeSettings, makerBase, judgeBase, run, repoRoot)));
            }
            var fanOut = BaselineFanOutWorkflow.Build(branches, run);
            workflow = RecipeWorkflow.BuildFromFanOut(fanOut, derivations, run);
        }

        if (dryRun)
        {
            Console.WriteLine($"[recipe] --dry-run: Rezept Build()-bar (mode:{mode}, Baseline [{string.Join(",", baselineArtifacts)}] → {derivations.Count} Ableitung(en) [{string.Join(",", specs.Select(s => s.Id))}]). Kein LLM.");
            Console.WriteLine($"[recipe] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        var runInput = loadMode
            ? "LOAD"
            : "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + EvidenceLedgerProjection.Project(ledger!.Claims, baselineArtifacts[0]);
        Console.WriteLine($"[recipe] running recipe (mode:{mode})...");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, runInput, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[recipe] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk: baselines/{type}/artifact.json + je Ableitung derivations/{spec}/derived.json.
        var baseParts = baselineArtifacts.Select(t => $"{t}={Count(Path.Combine(run.RunDir, "baselines", t, "artifact.json"))}");
        Console.WriteLine($"[recipe] fertig: Baselines [{string.Join(", ", baseParts)}]");
        foreach (var spec in specs)
            Console.WriteLine($"[recipe]   {spec.Id}: derivations/{spec.Id}/derived.json = {Count(Path.Combine(run.RunDir, "derivations", spec.Id, "derived.json"))} items");
        Console.WriteLine($"[recipe] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static int Count(string path)
    {
        if (!File.Exists(path)) return -1;
        try { using var d = JsonDocument.Parse(File.ReadAllText(path)); return d.RootElement.GetProperty("items").GetArrayLength(); }
        catch (Exception) { return -1; }
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    private static string? ResolveExistingDir(string repoRoot, string p)
    {
        var full = Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
        return Directory.Exists(full) ? full : null;
    }

    private static string? ResolveRunDir(string repoRoot, string token)
    {
        var runsRoot = Path.Combine(repoRoot, "runs");
        if (!Directory.Exists(runsRoot)) return null;
        foreach (var family in Directory.EnumerateDirectories(runsRoot))
            foreach (var d in Directory.EnumerateDirectories(family))
                if (Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase))
                    return d;
        return null;
    }
}
