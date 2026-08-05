using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.FullWorkflow.FanOut;
using AgenticSdlc.Host.FullWorkflow.Load;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Chain;

/// <summary>
/// Volle MAF-Komposition (CLI: <c>evidence-chain &lt;specId&gt; [model] [--dry-run]</c>): ein durchgängiger
/// Workflow <c>Ledger → [Fan-out] → SelectBaseline → [Derivation]</c>, in dem Fan-out und Derivation je ein
/// gebundener Sub-Workflow sind. Fan-outet JE Quelltyp der Spec eine Baseline (1..N, Multi-Source) und leitet aus der
/// via SelectBaseline gezogenen Teilmenge ab. Runs unter <c>runs/evidence-chain/&lt;runId&gt;/</c>. Exit: 0 = ok/dry-run, 2 = Usage/IO, 3 = Workflow-Fehler,
/// 4 = Konfig/LLM-Fehler.
/// </summary>
/// <remarks>
/// Reiner Zusammenbau vorhandener Bausteine: Fan-out-Zweig via <see cref="BaselineFanOutRunner.BuildBranch"/> +
/// <see cref="BaselineFanOutWorkflow.Build"/>; Derivation via <see cref="DerivationWorkflow.Build"/>; Verkettung via
/// <see cref="EvidenceChainWorkflow.Build"/>. Mehrstufiges BindAsExecutor (Chain → Fan-out → Zweig → CheckerRepair;
/// Chain → Derivation). Human-Review bleibt separat (`derive-review`).
/// </remarks>
public static class EvidenceChainRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine($"Usage: evidence-chain <specId> [model] [--from-run <runId|pfad>] [--reflect|--reflect-graph] [--dry-run]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
            return 2;
        }
        var specId = args[1];
        if (!DerivationRegistry.TryGet(specId, out var spec)) { Console.Error.WriteLine($"[chain] unbekannte specId '{specId}'."); return 2; }
        var sourceTypes = spec.SourceArtifactTypes;

        // Args: erstes Nicht-Flag nach specId = Modell; --from-run <runId|pfad> schaltet auf mode:load (kein Fan-out/Ledger).
        string? modelArg = null, fromRunArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--from-run", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) fromRunArg = args[++i];
            else if (a.StartsWith("--", StringComparison.Ordinal)) { /* z. B. --dry-run */ }
            else if (modelArg is null) modelArg = a;
        }
        var dryRun = args.Contains("--dry-run");
        var loadMode = fromRunArg is not null;
        // Derivations-FORM (config-schaltbar, drop-in): strukturiert (Default) ODER der agentische Reflect-Mechanismus
        // (A-agentic-12…20) als Chain-Knoten. --reflect = node-intern (imperativer Loop), --reflect-graph = edge-native
        // (MAF-Zyklus). Beide sind typgleich (SourceArtifactSet → DerivationResult) → BindAsExecutor bleibt identisch.
        var reflectGraph = args.Contains("--reflect-graph");
        var reflect = args.Contains("--reflect") || reflectGraph;
        if (reflect && !spec.SupportsAccountVerify)
        {
            Console.Error.WriteLine($"[chain] spec '{spec.Id}' hat keinen Reflect-Modus (kein AgenticAccountVerifyPromptName). Ohne --reflect laufen lassen.");
            return 2;
        }
        int k = 3, minVotes = 0, maxIter = 3;

        // Quellen-Vorbedingungen je Modus.
        string? sourceDir = null, consPath = null;
        ConsumableLedger? ledger = null;
        if (loadMode)
        {
            // mode:load — Baseline aus vorhandenem Run (Pfad ODER runId unter runs/). Kein Ledger/Fan-out nötig.
            sourceDir = ResolveExistingDir(repoRoot, fromRunArg!) ?? ResolveRunDir(repoRoot, fromRunArg!);
            if (sourceDir is null) { Console.Error.WriteLine($"[chain] --from-run: Quell-Run '{fromRunArg}' nicht gefunden (Pfad oder runId unter runs/)."); return 2; }
            var missing = sourceTypes.Where(t => !BaselineExists(sourceDir, t)).ToList();
            if (missing.Count > 0)
            { Console.Error.WriteLine($"[chain] --from-run: fehlende Baseline(s) in '{Path.GetRelativePath(repoRoot, sourceDir)}': {string.Join(", ", missing.Select(t => $"{t}.artifact.json"))}."); return 2; }
        }
        else
        {
            // mode:build — jeder Quelltyp braucht einen Evidence-Agent; Ledger (consumable) als Fan-out-Quelle.
            var unknown = sourceTypes.Where(t => !BaselineFanOutRunner.ArtifactMap.ContainsKey(t)).ToList();
            if (unknown.Count > 0)
            { Console.Error.WriteLine($"[chain] kein Evidence-Agent für Quelltyp(en): {string.Join(", ", unknown)} (verfügbar: {string.Join(", ", BaselineFanOutRunner.ArtifactMap.Keys)})."); return 2; }
            if (!string.Equals(settings.EvidenceSource, "ledger", StringComparison.OrdinalIgnoreCase))
            { Console.Error.WriteLine("[chain] Arm B: setze evidenceAgent.source=ledger in run-config.json."); return 4; }
            consPath = Resolve(repoRoot, settings.EvidenceLedgerRun);
            if (consPath is null || !File.Exists(consPath)) { Console.Error.WriteLine($"[chain] consumable.json fehlt: '{settings.EvidenceLedgerRun}'."); return 2; }
            ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
            if (ledger is null || ledger.Claims.Count == 0) { Console.Error.WriteLine("[chain] consumable leer."); return 2; }
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var run = new RunContext(RunId.New(), "evidence-chain");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = EvidenceChainWorkflow.WorkflowName, runId = run.RunId, spec = spec.Id,
            mode = loadMode ? "load" : "build",
            derivationForm = reflect ? (reflectGraph ? "reflect-graph" : "reflect") : "structured",
            sourceTypes, target = spec.TargetArtifactType,
            consumable = consPath is not null ? Path.GetRelativePath(repoRoot, consPath) : null,
            claims = ledger?.Claims.Count ?? 0,
            fromRun = sourceDir is not null ? Path.GetRelativePath(repoRoot, sourceDir) : null,
            provider = settings.LlmProvider, generatorModel = genSettings.ModelId, checkerModel = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine(loadMode
            ? $"[chain] runId={run.RunId} spec={spec.Id}  Load({spec.SourceLabel}) → Select → [{spec.Id}]  from={Path.GetRelativePath(repoRoot, sourceDir!)}  genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}"
            : $"[chain] runId={run.RunId} spec={spec.Id}  Ledger → [Fan-out {spec.SourceLabel}] → Select → [{spec.Id}]  genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var judgeBase = ChatClientFactory.Create(judgeSettings);

        // Derivation-Workflow — modus-unabhängig verdrahtet; die FORM (strukturiert vs. agentischer Reflect) ist
        // config-schaltbar und wird per BindAsExecutor als EIN Knoten (SourceArtifactSet → DerivationResult) in die
        // Chain gehängt. Beide Formen sind typgleich → drop-in (EvidenceChainWorkflow bleibt unverändert).
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
        var checkClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"DerivationCheck-{spec.Id}", SourceName);
        var judgeSystemPrompt = string.IsNullOrWhiteSpace(spec.JudgePromptName) ? null : PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.JudgePromptName!, new Dictionary<string, string>());
        var checker = new InferenceChecker(checkClient, settings.JuryStructuredOutput, systemPrompt: judgeSystemPrompt, reasoning: settings.ReasoningCapture);
        var outScope = $"derivations/{spec.Id}";

        Microsoft.Agents.AI.Workflows.Workflow derivation;
        if (reflect)
        {
            // Der in A-agentic-12…20 geschlossene Selbstkorrektur-Mechanismus als Chain-Derivationsknoten: der Agent
            // liest/verankert/schreibt selbst (account-verify-Tools), externer Gate + bounded Loop (max 1 Retry); die
            // Provenienz (agent/ + checks/ + reflect/) landet im Chain-Run. Drill-down-Claims = der Chain-eigene Ledger
            // (consumable; build-Modus, sonst leer). Post-hoc-Judge = In-Loop-Judge (Chain hat kein --posthoc-judge).
            var ledgerClaims = LedgerClaimIndex.LoadOrEmpty(consPath);
            var reflectPrompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.AgenticAccountVerifyPromptName!, new Dictionary<string, string> { ["runId"] = run.RunId });
            Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
            {
                var a = genClient.AsAIAgent(instructions: reflectPrompt, name: spec.AgentName, tools: [.. tools]);
                return a.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            };
            var outDir = run.OutputDir(outScope);
            if (reflectGraph)
            {
                // Edge-native: der zyklische Reflect-Sub-Workflow wird selbst per BindAsExecutor als EIN Chain-Knoten gebunden.
                var deps = new ReflectGraphDeps(agentFactory, checker, checker, spec, genSettings.ModelId, run, outDir,
                    ledgerClaims, MaxRetries: 1, PostHocJudgeModel: judgeSettings.ModelId, IndependentPostHoc: false);
                derivation = ReflectGraphWorkflow.Build(deps);
            }
            else
            {
                var agenticExec = new DerivationAgenticExecutor(agentFactory, checker, checker, spec, genSettings.ModelId,
                    run, outDir, ledgerClaims, explorer: false, verify: false, accountable: false, accountVerify: true,
                    postHocJudgeModel: judgeSettings.ModelId, independentPostHoc: false, reflect: true);
                derivation = DerivationWorkflow.BuildAgentic(agenticExec, spec);
            }
            Console.WriteLine($"[chain] Derivation-FORM: {(reflectGraph ? "reflect-graph (edge-native MAF-Zyklus)" : "reflect (node-intern)")} — Agent + externer Gate + bounded Loop; Provenienz agent/+checks/+reflect/.");
        }
        else
        {
            var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
            AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
            agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            derivation = DerivationWorkflow.Build(
                new DerivationGenerateExecutor(agent, spec, run),
                new DerivationAnchorExecutor(spec, run),
                new DerivationCheckExecutor(checker, spec, genSettings.ModelId, run, outScope));
        }

        // Baseline-Quelle: Fan-out (build) ODER LoadBaseline (mode:load) — der Rest des Graphen (Select → Derivation) ist identisch.
        Microsoft.Agents.AI.Workflows.Workflow chain;
        if (loadMode)
        {
            var load = new LoadBaselineExecutor(run);
            chain = EvidenceChainWorkflow.BuildFromLoad(load, derivation, sourceTypes, run);
        }
        else
        {
            var makerBase = ChatClientFactory.Create(settings);
            var branches = new List<(string ArtifactType, string DispositionKey, Microsoft.Agents.AI.Workflows.Workflow Branch)>();
            foreach (var t in sourceTypes)
            {
                var (agentName, disposition) = BaselineFanOutRunner.ArtifactMap[t];
                var branch = BaselineFanOutRunner.BuildBranch(
                    t, agentName, disposition, ledger!, k, minVotes, maxIter,
                    settings, judgeSettings, makerBase, judgeBase, run, repoRoot);
                branches.Add((t, disposition, branch));
            }
            var fanOut = BaselineFanOutWorkflow.Build(branches, run);
            chain = EvidenceChainWorkflow.Build(fanOut, derivation, sourceTypes, run);
        }

        if (dryRun)
        {
            Console.WriteLine(loadMode
                ? $"[chain] --dry-run: Chain Build()-bar (Load({spec.SourceLabel}) → Select({spec.SourceLabel}) → [Derivation {spec.Id}], mode:load). Kein LLM."
                : $"[chain] --dry-run: Chain Build()-bar (Ledger → [Fan-out {spec.SourceLabel}] → Select({spec.SourceLabel}) → [Derivation {spec.Id}]). Kein LLM.");
            Console.WriteLine($"[chain] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine(loadMode
            ? "[chain] running chain (load -> select -> derivation)..."
            : "[chain] running full chain (ledger -> fan-out -> select -> derivation)...");
        try
        {
            // R-37/⑤ (MAF-native Form): der Workflow-Input IST die Daten — build: das Consumable (Fan-out-Dispatch
            // projiziert je Spur); load: die typisierte Bestellung (LoadBaselineRequest statt "LOAD"-Sentinel).
            if (loadMode)
                await InProcessExecution.Default.RunAsync(chain, new LoadBaselineRequest(sourceTypes, sourceDir!), run.RunId, CancellationToken.None).ConfigureAwait(false);
            else
                await InProcessExecution.Default.RunAsync(chain, ledger!, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[chain] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk: je Quelltyp eine baseline (baselines/{type}/artifact.json) + Ableitung (derivations/{spec}/derived.json).
        var baseParts = sourceTypes.Select(t => $"{t}={Count(Path.Combine(run.RunDir, "baselines", t, "artifact.json"), "items")}");
        var derivedItems = Count(Path.Combine(run.RunDir, "derivations", spec.Id, "derived.json"), "items");
        Console.WriteLine($"[chain] fertig: Baselines [{string.Join(", ", baseParts)}] items → {spec.TargetArtifactType} abgeleitet={derivedItems} items");
        Console.WriteLine($"[chain] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static int Count(string path, string prop)
    {
        if (!File.Exists(path)) return -1;
        try { using var d = JsonDocument.Parse(File.ReadAllText(path)); return d.RootElement.GetProperty(prop).GetArrayLength(); }
        catch (Exception) { return -1; }
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    // Akzeptiert neues (baselines/{type}/artifact.json) UND altes flaches Layout (Rückwärtskompatibilität beim Laden).
    internal static bool BaselineExists(string runDir, string type)
        => File.Exists(Path.Combine(runDir, "baselines", type, "artifact.json"))
        || File.Exists(Path.Combine(runDir, $"{type}.artifact.json"));

    private static string? ResolveExistingDir(string repoRoot, string p)
    {
        var full = Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
        return Directory.Exists(full) ? full : null;
    }

    // Sucht unter runs/&lt;family&gt;/&lt;runId&gt; nach einem Verzeichnis, dessen Name das Token enthält (Kurz-RunId erlaubt).
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
