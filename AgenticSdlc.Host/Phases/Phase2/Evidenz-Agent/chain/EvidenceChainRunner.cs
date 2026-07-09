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

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Chain;

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
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine($"Usage: evidence-chain <specId> [model] [--from-run <runId|pfad>] [--dry-run]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
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
        int k = 3, minVotes = 0, maxIter = 3;

        // Quellen-Vorbedingungen je Modus.
        string? sourceDir = null, consPath = null;
        ConsumableLedger? ledger = null;
        if (loadMode)
        {
            // mode:load — Baseline aus vorhandenem Run (Pfad ODER runId unter runs/). Kein Ledger/Fan-out nötig.
            sourceDir = ResolveExistingDir(repoRoot, fromRunArg!) ?? ResolveRunDir(repoRoot, fromRunArg!);
            if (sourceDir is null) { Console.Error.WriteLine($"[chain] --from-run: Quell-Run '{fromRunArg}' nicht gefunden (Pfad oder runId unter runs/)."); return 2; }
            var missing = sourceTypes.Where(t => !File.Exists(Path.Combine(sourceDir, $"{t}.artifact.json"))).ToList();
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

        // Derivation-Workflow (Generator = echter Agent, Check = Judge) — modus-unabhängig.
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
        var checkClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"DerivationCheck-{spec.Id}", SourceName);
        var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
        agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        var derivation = DerivationWorkflow.Build(
            new DerivationGenerateExecutor(agent, spec, run),
            new DerivationAnchorExecutor(spec, run),
            new DerivationCheckExecutor(new InferenceChecker(checkClient, settings.JuryStructuredOutput), spec, genSettings.ModelId, run));

        // Baseline-Quelle: Fan-out (build) ODER LoadBaseline (mode:load) — der Rest des Graphen (Select → Derivation) ist identisch.
        Microsoft.Agents.AI.Workflows.Workflow chain;
        if (loadMode)
        {
            var load = new LoadBaselineExecutor(sourceTypes, sourceDir!, run);
            chain = EvidenceChainWorkflow.BuildFromLoad(load, derivation, sourceTypes, run);
        }
        else
        {
            var makerBase = ChatClientFactory.Create(settings);
            var branches = new List<(string ArtifactType, Microsoft.Agents.AI.Workflows.Workflow Branch)>();
            foreach (var t in sourceTypes)
            {
                var (agentName, disposition) = BaselineFanOutRunner.ArtifactMap[t];
                var branch = BaselineFanOutRunner.BuildBranch(
                    t, agentName, disposition, ledger!, k, minVotes, maxIter,
                    settings, judgeSettings, makerBase, judgeBase, run, repoRoot);
                branches.Add((t, branch));
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

        // Fan-out reicht EINE disposition-annotierte Projektion an ALLE Zweige; jeder Zweig-Prompt liest seine Disposition.
        // Load-Modus braucht keinen Ledger-Input → trivialer Trigger.
        var runInput = loadMode
            ? "LOAD"
            : "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + EvidenceLedgerProjection.Project(ledger!.Claims, spec.PrimarySourceArtifactType);
        Console.WriteLine(loadMode
            ? "[chain] running chain (load -> select -> derivation)..."
            : "[chain] running full chain (ledger -> fan-out -> select -> derivation)...");
        try
        {
            await InProcessExecution.Default.RunAsync(chain, runInput, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[chain] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk: je Quelltyp eine baseline ({type}.artifact.json) + Ableitung ({target}.derived.json).
        var baseParts = sourceTypes.Select(t => $"{t}={Count(Path.Combine(run.RunDir, $"{t}.artifact.json"), "items")}");
        var derivedItems = Count(Path.Combine(run.RunDir, $"{spec.TargetArtifactType}.derived.json"), "items");
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
