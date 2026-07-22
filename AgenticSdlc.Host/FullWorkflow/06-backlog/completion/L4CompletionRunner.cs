using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public static class L4CompletionRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AdequacyAgentName = "L4AdequacyFeedbackAgent";
    private const string CompletionAgentName = "L4CompletionAgent";
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        return args[1].ToLowerInvariant() switch
        {
            "agent" => await RunAgentAsync(args, settings, repoRoot).ConfigureAwait(false),
            "check" => await RunCheckAsync(args, repoRoot).ConfigureAwait(false),
            _ => UnknownMode(args[1])
        };
    }

    private static async Task<int> RunAgentAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
        string? inputArg = null;
        string? outputDir = null;
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputDir = args[++i];
                continue;
            }
            if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase)) continue;
            if (arg.StartsWith("--", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"[l4-completion] unbekanntes Argument: {arg}");
                Usage();
                return 2;
            }
            if (inputArg is null) inputArg = arg;
            else modelArg ??= arg;
        }

        var resolved = ResolveAppliedDir(repoRoot, inputArg ?? "");
        if (resolved is null)
        {
            Console.Error.WriteLine($"[l4-completion] L4 applied dir nicht gefunden: {inputArg}");
            return 2;
        }

        var input = await LoadInputAsync(repoRoot, resolved).ConfigureAwait(false);
        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var run = new RunContext(RunId.New(), "l4-completion");
        run.EnsureFolders();
        var outDir = outputDir is null ? run.OutputDir("completion") : ResolvePath(repoRoot, outputDir);
        run.WriteConfig(new
        {
            workflow = L4CompletionWorkflow.WorkflowName,
            runId = run.RunId,
            appliedDir = Path.GetRelativePath(repoRoot, resolved.AppliedDir),
            provider = settings.LlmProvider,
            model = genSettings.ModelId,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            dryRun,
            timestampUtc = DateTime.UtcNow
        });

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var adequacyPrompt = PromptProvider.Load(repoRoot, Phase, AdequacyAgentName, "L4AdequacyFeedbackAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });
        var completionPrompt = PromptProvider.Load(repoRoot, Phase, CompletionAgentName, "L4CompletionAgent1", new Dictionary<string, string>
        {
            ["runId"] = run.RunId
        });

        var adequacyClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, AdequacyAgentName, SourceName);
        var completionClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, CompletionAgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> adequacyFactory = tools =>
            adequacyClient.AsAIAgent(instructions: adequacyPrompt, name: AdequacyAgentName, tools: [.. tools])
                .AsBuilder()
                .Use(new ToolCallLoggerMiddleware(run).InvokeAsync)
                .Build();
        Func<IReadOnlyList<AITool>, AIAgent> completionFactory = tools =>
            completionClient.AsAIAgent(instructions: completionPrompt, name: CompletionAgentName, tools: [.. tools])
                .AsBuilder()
                .Use(new ToolCallLoggerMiddleware(run).InvokeAsync)
                .Build();

        var workflow = L4CompletionWorkflow.Build(
            new L4AdequacyFeedbackAgentExecutor(adequacyFactory, run),
            new L4CompletionAgentExecutor(completionFactory, run),
            new L4CompletionGateExecutor(run),
            new L4CompletionFinalizeExecutor(run, outDir));

        if (dryRun)
        {
            Console.WriteLine("[l4-completion] --dry-run: Graph Build()-bar (AdequacyFeedbackAgent[Tools] -> CompletionAgent[Tools] -> Gate[det] -> Finalize). Kein LLM.");
            Console.WriteLine($"[l4-completion] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine($"[l4-completion] running workflow runId={run.RunId} model={genSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, input, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l4-completion] Ausfuehrung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var reportPath = Path.Combine(outDir, "l4-completion-gate-report.json");
        if (!File.Exists(reportPath))
        {
            Console.Error.WriteLine("[l4-completion] kein GateReport erzeugt.");
            return 4;
        }

        var report = JsonSerializer.Deserialize<L4CompletionGateReport>(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false), Json);
        if (report is not null)
            Console.WriteLine($"[l4-completion] gate={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[l4-completion] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return report?.Pass == true ? 0 : 1;
    }

    private static async Task<int> RunCheckAsync(string[] args, string repoRoot)
    {
        if (args.Length < 5)
        {
            Usage();
            return 2;
        }

        var resolved = ResolveAppliedDir(repoRoot, args[2]);
        if (resolved is null)
        {
            Console.Error.WriteLine($"[l4-completion] L4 applied dir nicht gefunden: {args[2]}");
            return 2;
        }

        var adequacyPath = ResolvePath(repoRoot, args[3]);
        var proposalsPath = ResolvePath(repoRoot, args[4]);
        if (!File.Exists(adequacyPath) || !File.Exists(proposalsPath))
        {
            Console.Error.WriteLine("[l4-completion] adequacy/proposals Datei nicht gefunden.");
            return 2;
        }

        var input = await LoadInputAsync(repoRoot, resolved).ConfigureAwait(false);
        var adequacy = await LoadAsync<L4AdequacyReport>(adequacyPath).ConfigureAwait(false);
        var proposals = await LoadAsync<L4CompletionProposalDocument>(proposalsPath).ConfigureAwait(false);
        var report = L4CompletionGate.Check(input, adequacy, proposals);
        var outputPath = Path.Combine(Path.GetDirectoryName(proposalsPath) ?? ".", "l4-completion-gate-report.json");
        await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine($"[l4-completion] gate={report.Decision} pass={report.Pass} errors={report.Errors.Count} warnings={report.Warnings.Count}");
        Console.WriteLine($"[l4-completion] -> {Path.GetRelativePath(repoRoot, outputPath)}");
        return report.Pass ? 0 : 1;
    }

    private static async Task<L4CompletionInput> LoadInputAsync(string repoRoot, ResolvedAppliedDir resolved)
    {
        var baseline = await LoadAsync<CanonicalRequirementsBaseline>(Path.Combine(resolved.AppliedDir, "canonical-requirements-baseline.json")).ConfigureAwait(false);
        var provenancePath = Path.Combine(resolved.AppliedDir, "provenance-map.json");
        var readinessPath = Path.Combine(resolved.AppliedDir, "requirements-readiness.json");
        var docPath = Path.Combine(resolved.AppliedDir, "requirements-document.md");
        if (!File.Exists(docPath))
            throw new FileNotFoundException("requirements-document.md fehlt. Fuehre zuerst l4-requirements-doc aus.", docPath);
        var provenance = File.Exists(provenancePath) ? await LoadAsync<L4ProvenanceMap>(provenancePath).ConfigureAwait(false) : null;
        var readiness = File.Exists(readinessPath) ? await LoadAsync<RequirementsReadinessReport>(readinessPath).ConfigureAwait(false) : null;
        var doc = await File.ReadAllTextAsync(docPath).ConfigureAwait(false);
        return new L4CompletionInput(
            Baseline: baseline,
            Provenance: provenance,
            Readiness: readiness,
            RequirementsDocumentText: doc,
            SourceAppliedDir: Path.GetRelativePath(repoRoot, resolved.AppliedDir),
            SourceRequirementsDocumentPath: Path.GetRelativePath(repoRoot, docPath));
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static ResolvedAppliedDir? ResolveAppliedDir(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && string.Equals(Path.GetFileName(full), "canonical-requirements-baseline.json", StringComparison.OrdinalIgnoreCase))
            return new ResolvedAppliedDir(Path.GetDirectoryName(full) ?? repoRoot);
        if (Directory.Exists(full))
        {
            if (File.Exists(Path.Combine(full, "canonical-requirements-baseline.json"))) return new ResolvedAppliedDir(full);
            var applied = Path.Combine(full, "consolidation", "applied");
            if (File.Exists(Path.Combine(applied, "canonical-requirements-baseline.json"))) return new ResolvedAppliedDir(applied);
        }

        var l4Root = Path.Combine(repoRoot, "runs", "l4");
        if (!Directory.Exists(l4Root)) return null;
        foreach (var runDir in Directory.EnumerateDirectories(l4Root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var applied = Path.Combine(runDir, "consolidation", "applied");
            if (File.Exists(Path.Combine(applied, "canonical-requirements-baseline.json"))) return new ResolvedAppliedDir(applied);
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[l4-completion] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: l4-completion agent <l4-applied-dir|l4RunId> [model] [--out <dir>] [--dry-run]");
        Console.Error.WriteLine("       l4-completion check <l4-applied-dir|l4RunId> <l4-adequacy-report.json> <l4-completion-proposals.json>");
    }

    private sealed record ResolvedAppliedDir(string AppliedDir);
}
