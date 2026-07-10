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
            Console.Error.WriteLine($"Usage: derive <specId> <source1.artifact.json> [source2.artifact.json …] [model] [--dry-run]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
            return 2;
        }

        var specId = args[1];
        if (!DerivationRegistry.TryGet(specId, out var spec))
        {
            Console.Error.WriteLine($"[derive] unbekannte specId '{specId}' (verfügbar: {string.Join(", ", DerivationRegistry.Specs.Keys)}).");
            return 2;
        }

        var dryRun = args.Contains("--dry-run");
        var reflect = args.Contains("--reflect");     // REFLECT: account-verify + VERBINDLICHE externe Abnahme (Reflection-Pattern, bounded Loop, externer Gate).
        var accountVerify = args.Contains("--account-verify") || reflect; // reflect impliziert das account-verify-Toolset + die beiden Checks.
        var verify = args.Contains("--verify") && !accountVerify;   // VERIFY-LOOP: Explorer + Selbstkorrektur (verify_derived) gegen eine Definition of Done.
        var account = args.Contains("--account") && !accountVerify; // ACCOUNTABLE: Explorer + Coverage-Rechenschaft (account_uncovered) + sichtbares Reasoning.
        var explore = args.Contains("--explore");     // EXPLORER: Ziel-only-Prompt + Entdeckungs-Tools; Agent entdeckt seine Umwelt selbst.
        var agentic = args.Contains("--agentic") || explore || verify || account || accountVerify;   // Explorer/Verify/Account(+Verify) sind agentische Modi.
        var narrate = args.Contains("--narrate");   // DIAGNOSE: Narrations-Pflicht-Prompt (Reasoning-Text vor jedem Tool-Call). NICHT die Mess-Default.
        if (agentic && !spec.SupportsAgentic)
        {
            Console.Error.WriteLine($"[derive] spec '{spec.Id}' hat keinen agentischen Modus (kein AgenticPromptName). Ohne --agentic laufen lassen oder AgenticPromptName in der Registry ergänzen.");
            return 2;
        }
        if (explore && !spec.SupportsExplorer)
        {
            Console.Error.WriteLine($"[derive] spec '{spec.Id}' hat keinen Explorer-Modus (kein AgenticExplorerPromptName).");
            return 2;
        }
        if (verify && !spec.SupportsVerify)
        {
            Console.Error.WriteLine($"[derive] spec '{spec.Id}' hat keinen Verify-Modus (kein AgenticVerifyPromptName).");
            return 2;
        }
        if (account && !spec.SupportsAccountable)
        {
            Console.Error.WriteLine($"[derive] spec '{spec.Id}' hat keinen Accountable-Modus (kein AgenticAccountablePromptName).");
            return 2;
        }
        if (accountVerify && !spec.SupportsAccountVerify)
        {
            Console.Error.WriteLine($"[derive] spec '{spec.Id}' hat keinen Account-Verify-Modus (kein AgenticAccountVerifyPromptName).");
            return 2;
        }
        if (narrate && !agentic)
            Console.WriteLine("[derive] HINWEIS: --narrate wirkt nur mit --agentic (Diagnose-Prompt). Ignoriert.");
        var useDiagnostic = agentic && narrate && !explore && !verify && !account && !accountVerify && !string.IsNullOrWhiteSpace(spec.AgenticDiagnosticPromptName);

        // Optional: --ledger <consumable.json> (Drill-down-Claim-Texte), --env <dir> (Explorer-Umwelt = ALLE
        // baselines/*/artifact.json darunter) und --posthoc-judge <model> (UNABHÄNGIGER Nach-Prüfungs-Judge, anderes
        // Modell als der In-Loop-verify_derived — bricht die Zirkularität: finalR1/dodPass gegen einen Judge, gegen den
        // der Agent NICHT optimiert hat). Ihre WERT-Indizes werden beim Positional-Parsing übersprungen.
        string? ledgerArg = null, envArg = null, postHocJudgeArg = null;
        var skipValueIndex = new HashSet<int>();
        for (var i = 2; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], "--ledger", StringComparison.Ordinal)) { ledgerArg = args[i + 1]; skipValueIndex.Add(i + 1); }
            else if (string.Equals(args[i], "--env", StringComparison.Ordinal)) { envArg = args[i + 1]; skipValueIndex.Add(i + 1); }
            else if (string.Equals(args[i], "--posthoc-judge", StringComparison.Ordinal)) { postHocJudgeArg = args[i + 1]; skipValueIndex.Add(i + 1); }
        }

        // Positional nach specId: existierende Dateien = Quellen (1..N, Multi-Source); erstes Nicht-File/Nicht-Flag = Modell.
        var sourcePaths = new List<string>();
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (skipValueIndex.Contains(i)) continue;   // --ledger/--env-Werte sind keine Quellen.
            var a = args[i];
            if (a.StartsWith("--", StringComparison.Ordinal)) continue;
            var resolved = Resolve(repoRoot, a);
            if (resolved is not null && File.Exists(resolved)) sourcePaths.Add(resolved);
            else if (modelArg is null) modelArg = a;
        }
        var ledgerPath = Resolve(repoRoot, ledgerArg);
        var ledgerClaims = LedgerClaimIndex.LoadOrEmpty(ledgerPath);

        var sources = new List<ArtifactDocument>();
        var seenTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in sourcePaths)
        {
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(sp).ConfigureAwait(false), Json);
            if (doc is null || doc.Items.Count == 0) { Console.Error.WriteLine($"[derive] Quelle leer/nicht lesbar: {Path.GetRelativePath(repoRoot, sp)}"); return 2; }
            if (seenTypes.Add(doc.ArtifactType)) sources.Add(doc);
        }
        // --env: alle baselines/*/artifact.json (oder flach *.artifact.json) unter dir in die Umwelt aufnehmen (dedup je Typ).
        var envDir = Resolve(repoRoot, envArg);
        if (envDir is not null && Directory.Exists(envDir))
        {
            var envFiles = Directory.EnumerateFiles(envDir, "artifact.json", SearchOption.AllDirectories)
                .Concat(Directory.EnumerateFiles(envDir, "*.artifact.json", SearchOption.AllDirectories))
                .Distinct().OrderBy(p => p, StringComparer.Ordinal);
            foreach (var ef in envFiles)
            {
                try
                {
                    var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(ef).ConfigureAwait(false), Json);
                    if (doc is not null && doc.Items.Count > 0 && seenTypes.Add(doc.ArtifactType)) sources.Add(doc);
                }
                catch (JsonException) { /* skip unlesbare */ }
            }
        }
        if (sources.Count == 0)
        {
            Console.Error.WriteLine($"[derive] keine Quell-/Umwelt-Artefakte gefunden (erwartet 1..N *.artifact.json positional oder via --env <dir>).");
            return 2;
        }
        var sourceSet = new SourceArtifactSet(sources);

        // Typ-Abgleich Quelle(n) ↔ Spec (nur Hinweis; läuft trotzdem — der Prompt bestimmt die Semantik).
        var providedTypes = sources.Select(s => s.ArtifactType).ToList();
        var missing = spec.SourceArtifactTypes.Where(t => !providedTypes.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();
        var extra = providedTypes.Where(t => !spec.SourceArtifactTypes.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList();
        if (missing.Count > 0 || extra.Count > 0)
            Console.WriteLine($"[derive] HINWEIS: Quelltypen [{string.Join(",", providedTypes)}] != Spec [{spec.SourceLabel}] (fehlt: [{string.Join(",", missing)}], extra: [{string.Join(",", extra)}]) — läuft trotzdem.");

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;
        // Post-hoc-Judge: standardmäßig = In-Loop-Judge (rückwärtskompatibel); mit --posthoc-judge <model> ein ANDERES
        // Modell für die unabhängige Nach-Prüfung (bricht die verify_derived-Zirkularität).
        var postHocSettings = !string.IsNullOrWhiteSpace(postHocJudgeArg) ? settings with { ModelId = postHocJudgeArg!.Trim() } : judgeSettings;
        var independentPostHoc = !string.Equals(postHocSettings.ModelId, judgeSettings.ModelId, StringComparison.Ordinal);

        var run = new RunContext(RunId.New(), "derivation");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = $"Derivation-{spec.Id}", runId = run.RunId, spec = spec.Id,
            sources = sourcePaths.Select(p => Path.GetRelativePath(repoRoot, p)).ToArray(),
            sourceTypes = providedTypes, sourceItems = sourceSet.TotalItemCount,
            provider = settings.LlmProvider, generatorModel = genSettings.ModelId, checkerModel = judgeSettings.ModelId,
            inLoopJudgeModel = judgeSettings.ModelId, postHocJudgeModel = postHocSettings.ModelId, independentPostHoc,
            mode = reflect ? "explore-account-verify-reflect" : accountVerify ? "explore-account-verify" : verify ? "explore-verify" : account ? "explore-account" : explore ? "explore" : agentic ? "agentic" : "structured", diagnostic = useDiagnostic,
            env = envDir is not null ? Path.GetRelativePath(repoRoot, envDir) : null, envArtifacts = sources.Select(s => s.ArtifactType).ToArray(),
            ledger = ledgerPath is not null ? Path.GetRelativePath(repoRoot, ledgerPath) : null, ledgerClaims = ledgerClaims.Count,
            prompt = !agentic ? spec.PromptName : accountVerify ? spec.AgenticAccountVerifyPromptName : verify ? spec.AgenticVerifyPromptName : account ? spec.AgenticAccountablePromptName : explore ? spec.AgenticExplorerPromptName : useDiagnostic ? spec.AgenticDiagnosticPromptName : spec.AgenticPromptName,
            target = spec.TargetArtifactType, timestampUtc = DateTime.UtcNow
        });
        var modeLabel = reflect ? "explore-account-verify-reflect" : accountVerify ? "explore-account-verify" : verify ? "explore-verify" : account ? "explore-account" : explore ? "explore" : agentic ? "agentic" : "structured";
        Console.WriteLine($"[derive] runId={run.RunId} spec={spec.Id} mode={modeLabel} ([{spec.SourceLabel}]->{spec.TargetArtifactType}) umwelt=[{string.Join(",", sources.Select(s => s.ArtifactType))}] items={sourceSet.TotalItemCount} genModel={genSettings.ModelId} checkModel={judgeSettings.ModelId}");
        if (reflect) Console.WriteLine($"[derive] REFLECT: account-verify-tools + VERBINDLICHE externe Abnahme (Reflection-Pattern) — externer Gate (det. Anker/Coverage + unabhängiger Judge), bounded Loop (max 1 Retry), Kritik wird bei fail explizit wieder eingespeist. Kein Force auf den Tool-Call.");
        else if (accountVerify) Console.WriteLine($"[derive] account-verify-tools: explorer-set + account_uncovered + check_accountability (Coverage-Feedback) + verify_derived (Treue-Feedback) — geschlossene Schleife VOR save, Host meldet nur zurück (ledger claims: {ledgerClaims.Count}).");
        else if (verify) Console.WriteLine($"[derive] verify-loop-tools: explorer-set + verify_derived (Selbstprüfung des Entwurfs gegen Definition of Done, Selbstkorrektur; ledger claims: {ledgerClaims.Count}). Ziel-only-Prompt.");
        else if (account) Console.WriteLine($"[derive] accountable-tools: explorer-set + account_uncovered (Coverage-Rechenschaft: jedes Item genutzt ODER begründet verworfen; ledger claims: {ledgerClaims.Count}). Sichtbares Reasoning.");
        else if (explore) Console.WriteLine($"[derive] explorer-tools: list_artifacts/search_items/get_item + get_baseline_items/check_anchor/save_derived + drill-down (ledger claims: {ledgerClaims.Count}). Ziel-only-Prompt, Umwelt selbst-entdeckt.");
        else if (agentic) Console.WriteLine($"[derive] agentic-tools: get_baseline_items/check_anchor/save_derived + drill-down get_source_claims/find_related_items (ledger claims: {ledgerClaims.Count}){(useDiagnostic ? " · DIAGNOSE: Narrations-Prompt v3 (--narrate)" : "")}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Generator = echter AIAgent (Prompt + Pipeline + ToolLogger). Check = bounded Judge (Pipeline-Client).
        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, $"DerivationGenerate-{spec.Id}", SourceName);
        var checkClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, $"DerivationCheck-{spec.Id}", SourceName);
        // Unabhängiger Post-hoc-Judge (nur wenn --posthoc-judge ein anderes Modell nennt) — eigener Pipeline-Scope.
        var postHocClient = independentPostHoc
            ? AgentChatPipelineBuilder.Build(ChatClientFactory.Create(postHocSettings), settings, run, $"DerivationPostHoc-{spec.Id}", SourceName)
            : checkClient;
        if (independentPostHoc) Console.WriteLine($"[derive] UNABHÄNGIGER Post-hoc-Judge: {postHocSettings.ModelId} (≠ In-Loop-verify {judgeSettings.ModelId}) — finalR1/dodPass gegen einen nicht-optimierten Judge.");

        var promptName = !agentic ? spec.PromptName
            : accountVerify ? spec.AgenticAccountVerifyPromptName!
            : verify ? spec.AgenticVerifyPromptName!
            : account ? spec.AgenticAccountablePromptName!
            : explore ? spec.AgenticExplorerPromptName!
            : useDiagnostic ? spec.AgenticDiagnosticPromptName!
            : spec.AgenticPromptName!;
        var prompt = PromptProvider.Load(repoRoot, Phase, spec.AgentName, promptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        // checker = In-Loop-Judge (verify_derived, das Selbst-Prüfwerkzeug des Agenten). postHocChecker = die UNABHÄNGIGE
        // Nach-Prüfung (Authorität). Ohne --posthoc-judge sind beide identisch (rückwärtskompatibel).
        var checker = new InferenceChecker(checkClient, settings.JuryStructuredOutput);
        var postHocChecker = independentPostHoc ? new InferenceChecker(postHocClient, settings.JuryStructuredOutput) : checker;
        var outScope = $"derivations/{spec.Id}";

        Microsoft.Agents.AI.Workflows.Workflow workflow;
        if (agentic)
        {
            // Stufe 1: der Agent bekommt schmale Tools (get_baseline_items/check_anchor/save_derived) und arbeitet selbst.
            // Die Tools schließen über die Quell-Items ab → Agent wird pro Lauf mit gebundenen Tools gebaut (Factory).
            Func<IReadOnlyList<AITool>, AIAgent> agentFactory = tools =>
            {
                var a = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: [.. tools]);
                return a.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            };
            var agenticExec = new DerivationAgenticExecutor(agentFactory, checker, postHocChecker, spec, genSettings.ModelId, run, run.OutputDir(outScope), ledgerClaims, explore, verify, account, accountVerify, postHocSettings.ModelId, independentPostHoc, reflect);
            workflow = DerivationWorkflow.BuildAgentic(agenticExec, spec);
        }
        else
        {
            AIAgent agent = genClient.AsAIAgent(instructions: prompt, name: spec.AgentName, tools: []);
            agent = agent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            var generate = new DerivationGenerateExecutor(agent, spec, run);
            var anchor = new DerivationAnchorExecutor(spec, run);
            var check = new DerivationCheckExecutor(postHocChecker, spec, genSettings.ModelId, run, outScope);
            workflow = DerivationWorkflow.Build(generate, anchor, check);
        }

        if (dryRun)
        {
            Console.WriteLine(agentic
                ? $"[derive] --dry-run --agentic: Graph Build()-bar (EIN Agent-Knoten mit Tools; unabhängige Assurance danach) für spec '{spec.Id}'. Kein LLM."
                : $"[derive] --dry-run: Graph Build()-bar (Generate[Agent] → AnchorValidate[det] → InferenceCheck[Judge]) für spec '{spec.Id}'. Kein LLM.");
            Console.WriteLine($"[derive] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine("[derive] running derivation workflow...");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, sourceSet, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[derive] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Wahrheit von Disk (der Check-Executor schreibt).
        var derivedPath = Path.Combine(run.RunDir, "derivations", spec.Id, "derived.json");
        var reportPath = Path.Combine(run.RunDir, "derivations", spec.Id, "derivation-report.json");
        int items = -1, invalid = -1;
        if (File.Exists(derivedPath)) { using var d = JsonDocument.Parse(await File.ReadAllTextAsync(derivedPath).ConfigureAwait(false)); items = d.RootElement.GetProperty("items").GetArrayLength(); }
        if (File.Exists(reportPath)) { using var d = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false)); invalid = d.RootElement.GetProperty("invalidAnchor").GetInt32(); }
        Console.WriteLine($"[derive] fertig: {spec.TargetArtifactType} abgeleitet, anker-gültig={items}, anker-ungültig={invalid}");
        Console.WriteLine($"[derive] -> derivations/{spec.Id}/derived.json + inference-check-report.json  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
