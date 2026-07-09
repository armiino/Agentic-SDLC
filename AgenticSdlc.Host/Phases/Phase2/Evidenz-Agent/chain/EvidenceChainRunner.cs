using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;
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
/// gebundener Sub-Workflow sind. Baut die Baseline für den vom Spec geforderten Quelltyp und leitet daraus ab.
/// Runs unter <c>runs/evidence-chain/&lt;runId&gt;/</c>. Exit: 0 = ok/dry-run, 2 = Usage/IO, 3 = Workflow-Fehler,
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
            Console.Error.WriteLine($"Usage: evidence-chain <specId> [model] [--dry-run]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
            return 2;
        }
        var specId = args[1];
        if (!DerivationRegistry.TryGet(specId, out var spec)) { Console.Error.WriteLine($"[chain] unbekannte specId '{specId}'."); return 2; }
        if (spec.IsMultiSource)
        {
            Console.Error.WriteLine($"[chain] spec '{spec.Id}' ist Multi-Source ({spec.SourceLabel}); die Kette baut aktuell nur den Einzelquell-Fan-out. "
                                  + "Nutze `derive <specId> <quelle1.artifact.json> <quelle2.artifact.json> …` (Multi-Source läuft dort), bis SelectSubset/LoadBaseline in der Kette stehen.");
            return 2;
        }
        var sourceType = spec.PrimarySourceArtifactType;
        if (!BaselineFanOutRunner.ArtifactMap.TryGetValue(sourceType, out var extract))
        { Console.Error.WriteLine($"[chain] kein Evidence-Agent für Quelltyp '{sourceType}'."); return 2; }

        if (!string.Equals(settings.EvidenceSource, "ledger", StringComparison.OrdinalIgnoreCase))
        { Console.Error.WriteLine("[chain] Arm B: setze evidenceAgent.source=ledger in run-config.json."); return 4; }

        string? modelArg = args.Length >= 3 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        var dryRun = args.Contains("--dry-run");
        int k = 3, minVotes = 0, maxIter = 3;

        var consPath = Resolve(repoRoot, settings.EvidenceLedgerRun);
        if (consPath is null || !File.Exists(consPath)) { Console.Error.WriteLine($"[chain] consumable.json fehlt: '{settings.EvidenceLedgerRun}'."); return 2; }
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0) { Console.Error.WriteLine("[chain] consumable leer."); return 2; }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var run = new RunContext(RunId.New(), "evidence-chain");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = EvidenceChainWorkflow.WorkflowName, runId = run.RunId, spec = spec.Id,
            sourceType, target = spec.TargetArtifactType,
            consumable = Path.GetRelativePath(repoRoot, consPath), claims = ledger.Claims.Count,
            provider = settings.LlmProvider, generatorModel = genSettings.ModelId, checkerModel = judgeSettings.ModelId,
            k, minVotes, maxIterations = maxIter, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[chain] runId={run.RunId} spec={spec.Id}  Ledger → [{sourceType}] → Select → [{spec.Id}]  genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var makerBase = ChatClientFactory.Create(settings);
        var judgeBase = ChatClientFactory.Create(judgeSettings);

        // 1) Fan-out mit genau dem Zweig, den die Ableitung als Quelle braucht (Reuse BuildBranch).
        var branch = BaselineFanOutRunner.BuildBranch(
            sourceType, extract.Agent, extract.Disposition, ledger, k, minVotes, maxIter,
            settings, judgeSettings, makerBase, judgeBase, run, repoRoot);
        var fanOut = BaselineFanOutWorkflow.Build([(sourceType, branch)], run);

        // 2) Derivation-Workflow (Generator = echter Agent, Check = Judge).
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
        var checkClient = AgentChatPipelineBuilder.Build(judgeBase, settings, run, $"DerivationCheck-{spec.Id}", SourceName);
        var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, spec.PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
        agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
        var derivation = DerivationWorkflow.Build(
            new DerivationGenerateExecutor(agent, spec, run),
            new DerivationAnchorExecutor(spec, run),
            new DerivationCheckExecutor(new InferenceChecker(checkClient, settings.JuryStructuredOutput), spec, genSettings.ModelId, run));

        // 3) Verketten.
        var chain = EvidenceChainWorkflow.Build(fanOut, derivation, [sourceType], run);

        if (dryRun)
        {
            Console.WriteLine($"[chain] --dry-run: Chain Build()-bar (Ledger → [Fan-out {sourceType}] → Select → [Derivation {spec.Id}]). Kein LLM.");
            Console.WriteLine($"[chain] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        var sourceBlock = "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + EvidenceLedgerProjection.Project(ledger.Claims, extract.Disposition);
        Console.WriteLine("[chain] running full chain (ledger -> fan-out -> select -> derivation)...");
        try
        {
            await InProcessExecution.Default.RunAsync(chain, sourceBlock, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[chain] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk: baseline ({sourceType}.artifact.json) + Ableitung ({target}.derived.json).
        var basePath = Path.Combine(run.RunDir, $"{sourceType}.artifact.json");
        var derivedPath = Path.Combine(run.RunDir, $"{spec.TargetArtifactType}.derived.json");
        int baseItems = Count(basePath, "items"), derivedItems = Count(derivedPath, "items");
        Console.WriteLine($"[chain] fertig: Baseline {sourceType}={baseItems} items → {spec.TargetArtifactType} abgeleitet={derivedItems} items");
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
}
