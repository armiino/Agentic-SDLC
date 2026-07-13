using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// L3 Open-World-Ableitung (CLI: <c>l3 &lt;env1.artifact.json&gt; [env2 …] [model] [--dry-run]</c>). Fährt den
/// Prepare-Workflow (§5.3, Workflow 1): aus dem Umwelt-Graphen neue Kandidaten generieren, verankern, in 4 Klassen
/// routen und ein Human-Review-Paket erzeugen. Runs unter <c>runs/l3/&lt;runId&gt;/</c>. Exit: 0 = ok/dry-run,
/// 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Zwei echte Agenten (Generierung + Anker-Resolution, getrennt = §3), ein wiederverwendeter Judge-Kern
/// (<see cref="InferenceChecker"/> mit L3-Maßstab) und deterministische Executors (Validierung, Routing, Finalize).
/// Logger-Pipeline + ToolCallLogger wie in der Derivation-Familie. Der Human-Loop (Apply, accept/edit/reject) ist die
/// separate Phase 2 und noch nicht Teil von v1.
/// </remarks>
public static class L3Runner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "L3OpenWorldAgent";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: l3 <env1.artifact.json> [env2.artifact.json …] [model] [--dry-run]");
            return 2;
        }
        var dryRun = args.Contains("--dry-run");

        // --candidates <file>: kontrollierte Test-Kandidaten (Plan §11.1) — überspringt Generierung + Resolution und
        // fährt nur die deterministische Klassifikation + den Judge (alle 4 Klassen gezielt provozierbar).
        string? candidatesArg = null;
        var skipValue = new HashSet<int>();
        for (var i = 1; i < args.Length - 1; i++)
            if (string.Equals(args[i], "--candidates", StringComparison.Ordinal)) { candidatesArg = args[i + 1]; skipValue.Add(i + 1); }

        // Positional nach dem Befehl: existierende Dateien = Umwelt-Artefakte (1..N); erstes Nicht-File/Nicht-Flag = Modell.
        var sourcePaths = new List<string>();
        string? modelArg = null;
        for (var i = 1; i < args.Length; i++)
        {
            if (skipValue.Contains(i)) continue;
            var a = args[i];
            if (a.StartsWith("--", StringComparison.Ordinal)) continue;
            var resolved = Resolve(repoRoot, a);
            if (resolved is not null && File.Exists(resolved)) sourcePaths.Add(resolved);
            else if (modelArg is null) modelArg = a;
        }
        var injectPath = candidatesArg is not null ? Resolve(repoRoot, candidatesArg) : null;
        var inject = injectPath is not null;
        if (inject && !File.Exists(injectPath!)) { Console.Error.WriteLine($"[l3] --candidates: Datei fehlt: '{candidatesArg}'."); return 2; }

        var sources = new List<ArtifactDocument>();
        var seenTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var sp in sourcePaths)
        {
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(sp).ConfigureAwait(false), Json);
            if (doc is null || doc.Items.Count == 0) { Console.Error.WriteLine($"[l3] Umwelt-Artefakt leer/nicht lesbar: {Path.GetRelativePath(repoRoot, sp)}"); return 2; }
            if (seenTypes.Add(doc.ArtifactType)) sources.Add(doc);
        }
        if (sources.Count == 0) { Console.Error.WriteLine("[l3] keine Umwelt-Artefakte gefunden (erwartet 1..N *.artifact.json positional)."); return 2; }
        var env = new SourceArtifactSet(sources);

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var run = new RunContext(RunId.New(), "l3");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = L3Workflow.WorkflowName, runId = run.RunId,
            env = sourcePaths.Select(p => Path.GetRelativePath(repoRoot, p)).ToArray(), envTypes = sources.Select(s => s.ArtifactType).ToArray(),
            envItems = env.TotalItemCount, provider = settings.LlmProvider, generatorModel = genSettings.ModelId, judgeModel = judgeSettings.ModelId,
            mode = inject ? "from-candidates" : "generate", candidates = inject ? Path.GetRelativePath(repoRoot, injectPath!) : null,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[l3] runId={run.RunId}  umwelt=[{string.Join(",", sources.Select(s => s.ArtifactType))}] items={env.TotalItemCount}  genModel={genSettings.ModelId} judgeModel={judgeSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Judge (immer) — wiederverwendeter InferenceChecker-Kern mit L3-Maßstab, über die Standard-Pipeline.
        var judgeClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "L3-SupportJudge", SourceName);
        var judgePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3SupportJudge1", new Dictionary<string, string>());
        var judge = new InferenceChecker(judgeClient, settings.JuryStructuredOutput, systemPrompt: judgePrompt);

        Microsoft.Agents.AI.Workflows.Workflow workflow;
        L3Resolved? injected = null;
        if (inject)
        {
            injected = new L3Resolved(env, await LoadTestCandidatesAsync(injectPath!).ConfigureAwait(false));
            workflow = L3Workflow.BuildFromResolved(new L3AnchorValidateExecutor(run), new L3SupportJudgeExecutor(judge, run), new L3RoutingExecutor(run), new L3FinalizeExecutor(run));
            Console.WriteLine($"[l3] mode=from-candidates: {injected.Items.Count} kontrollierte Test-Kandidaten (Generierung + Resolution übersprungen).");
        }
        else
        {
            // Zwei Agenten (Generierung + Anker-Resolution getrennt) — beide über Standard-Pipeline + ToolCallLogger.
            var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L3-CandidateGen", SourceName);
            var resolveClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, run, "L3-AnchorResolve", SourceName);
            var genPrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3CandidateGen1", new Dictionary<string, string> { ["runId"] = run.RunId });
            var resolvePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3AnchorResolve1", new Dictionary<string, string> { ["runId"] = run.RunId });
            AIAgent genAgent = genClient.AsAIAgent(instructions: genPrompt, name: AgentName, tools: []);
            genAgent = genAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            AIAgent resolveAgent = resolveClient.AsAIAgent(instructions: resolvePrompt, name: AgentName, tools: []);
            resolveAgent = resolveAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            workflow = L3Workflow.Build(
                new L3CandidateGenExecutor(genAgent, run),
                new L3AnchorResolveExecutor(resolveAgent, run),
                new L3AnchorValidateExecutor(run),
                new L3SupportJudgeExecutor(judge, run),
                new L3RoutingExecutor(run),
                new L3FinalizeExecutor(run));
        }

        if (dryRun)
        {
            Console.WriteLine("[l3] --dry-run: Graph Build()-bar (Validate[det] → SupportJudge[Judge] → Routing[det] → Finalize, davor Generierung+Resolution außer bei --candidates). Kein LLM.");
            Console.WriteLine($"[l3] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        Console.WriteLine("[l3] running open-world prepare workflow...");
        try
        {
            if (inject) await InProcessExecution.Default.RunAsync(workflow, injected!, run.RunId, CancellationToken.None).ConfigureAwait(false);
            else await InProcessExecution.Default.RunAsync(workflow, env, run.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l3] Ausführung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var routingPath = Path.Combine(run.RunDir, "routing-report.json");
        if (File.Exists(routingPath))
        {
            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(routingPath).ConfigureAwait(false));
            var total = d.RootElement.GetProperty("total").GetInt32();
            var byClass = d.RootElement.GetProperty("byClass");
            Console.WriteLine($"[l3] fertig: {total} Kandidaten geroutet — {string.Join(", ", byClass.EnumerateObject().Select(p => $"{p.Name}={p.Value.GetInt32()}"))}");
        }
        Console.WriteLine($"[l3] -> routing-report.json + human-review-package.json  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    /// <summary>Lädt kontrollierte Test-Kandidaten (mit vorgegebenen Ankern) → <see cref="ResolvedCandidate"/>-Liste,
    /// als hätte die Resolution sie geliefert (Plan §11.1). CandidateId aus der Datei ODER T{n} als Fallback.</summary>
    private static async Task<IReadOnlyList<ResolvedCandidate>> LoadTestCandidatesAsync(string path)
    {
        var file = JsonSerializer.Deserialize<TestCandidatesFile>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
        var result = new List<ResolvedCandidate>();
        var n = 0;
        foreach (var c in file?.Candidates ?? [])
        {
            if (string.IsNullOrWhiteSpace(c.Text)) continue;
            n++;
            var id = string.IsNullOrWhiteSpace(c.CandidateId) ? $"T{n:D2}" : c.CandidateId!.Trim();
            var candidate = new L3Candidate(id, string.IsNullOrWhiteSpace(c.TargetType) ? "requirement" : c.TargetType!.Trim(),
                c.Text!.Trim(), c.Rationale, c.Assumptions ?? []);
            var anchors = (c.ProposedAnchors ?? [])
                .Where(a => !string.IsNullOrWhiteSpace(a.ItemId))
                .Select(a => new ProposedAnchor(a.ItemId!.Trim(), string.IsNullOrWhiteSpace(a.Relation) ? "relates_to" : a.Relation!.Trim(), a.Reason ?? ""))
                .ToList();
            result.Add(new ResolvedCandidate(candidate, new L3AnchorResolution(anchors, anchors.Count == 0 ? "Test-Kandidat ohne Anker" : null)));
        }
        return result;
    }

    private sealed record TestCandidatesFile(
        [property: System.Text.Json.Serialization.JsonPropertyName("candidates")] IReadOnlyList<TestCandidate>? Candidates);
    private sealed record TestCandidate(
        [property: System.Text.Json.Serialization.JsonPropertyName("candidateId")] string? CandidateId,
        [property: System.Text.Json.Serialization.JsonPropertyName("targetType")] string? TargetType,
        [property: System.Text.Json.Serialization.JsonPropertyName("text")] string? Text,
        [property: System.Text.Json.Serialization.JsonPropertyName("rationale")] string? Rationale,
        [property: System.Text.Json.Serialization.JsonPropertyName("assumptions")] IReadOnlyList<string>? Assumptions,
        [property: System.Text.Json.Serialization.JsonPropertyName("proposedAnchors")] IReadOnlyList<TestAnchor>? ProposedAnchors);
    private sealed record TestAnchor(
        [property: System.Text.Json.Serialization.JsonPropertyName("itemId")] string? ItemId,
        [property: System.Text.Json.Serialization.JsonPropertyName("relation")] string? Relation,
        [property: System.Text.Json.Serialization.JsonPropertyName("reason")] string? Reason);
}
