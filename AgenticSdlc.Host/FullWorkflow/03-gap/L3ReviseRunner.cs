using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// L3 Reflect-Sub-Workflow für NEEDS_REVISION (CLI: <c>l3-revise &lt;l3-run|runId&gt; [--decisions &lt;file&gt;] [model]</c>).
/// Liest die menschlichen Entscheidungen mit <c>"decision":"revise"</c> (+ <c>reason</c> = Feedback) und startet — BOUNDED
/// (max 1 Revision) — den MAF-Sub-Workflow: Revise[Agent] → AnchorResolve[Agent] → Validate → Judge → Routing → Finalize.
/// Der revidierte Kandidat wird NEU klassifiziert; das Ergebnis landet in <c>routing-report-rev&lt;n&gt;.json</c> +
/// <c>human-review-package-rev&lt;n&gt;.json</c>. Über der Schranke → <c>needs-human-resolution.json</c>.
/// Exit: 0 = ok, 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
public static class L3ReviseRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "L3OpenWorldAgent";
    private const int MaxRevisions = 1;
    private static readonly Regex RevSuffix = new(@"-rev(\d+)$", RegexOptions.Compiled);

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2) { Console.Error.WriteLine("Usage: l3-revise <l3-run-dir|runId> [--decisions <human-decisions.json>] [model]"); return 2; }
        var runDir = ResolveRunDir(repoRoot, args[1]);
        if (runDir is null) { Console.Error.WriteLine($"[l3-revise] L3-Lauf '{args[1]}' nicht gefunden."); return 2; }

        string? decisionsArg = null, modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--decisions", StringComparison.Ordinal) && i + 1 < args.Length) decisionsArg = args[++i];
            else if (!args[i].StartsWith("--", StringComparison.Ordinal) && modelArg is null) modelArg = args[i];
        }
        var decisionsPath = decisionsArg is not null ? Resolve(repoRoot, decisionsArg) : Path.Combine(runDir, "human-decisions.json");
        if (decisionsPath is null || !File.Exists(decisionsPath)) { Console.Error.WriteLine($"[l3-revise] Entscheidungsdatei fehlt: '{decisionsPath}'."); return 2; }

        var routingPath = Path.Combine(runDir, "routing-report.json");
        if (!File.Exists(routingPath)) { Console.Error.WriteLine("[l3-revise] routing-report.json fehlt — erst `l3` (Prepare) laufen lassen."); return 2; }
        // Prepare-Report + alle bisherigen rev-Reports mergen (spätere gewinnen) → revidierte Kandidaten (…-revN) sind
        // auffindbar, damit die Bounded-Schranke bei einer weiteren Runde greift.
        var byId = new Dictionary<string, L3RoutedCandidate>(StringComparer.Ordinal);
        foreach (var rp in new[] { routingPath }.Concat(Directory.EnumerateFiles(runDir, "routing-report-rev*.json").OrderBy(f => f, StringComparer.Ordinal)))
            foreach (var it in JsonSerializer.Deserialize<RoutingReport>(await File.ReadAllTextAsync(rp).ConfigureAwait(false), Json)?.Items ?? [])
                byId[it.Candidate.CandidateId] = it;
        var decisions = JsonSerializer.Deserialize<HumanDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Json);

        // Env aus dem config.json des Laufs rekonstruieren (die Umwelt, gegen die revidiert/verankert wird).
        var env = await LoadEnvAsync(runDir, repoRoot).ConfigureAwait(false);
        if (env is null) { Console.Error.WriteLine("[l3-revise] Umwelt (config.json 'env') nicht rekonstruierbar."); return 2; }

        var toRevise = new List<L3ReviseItem>();
        var needsHuman = new List<object>();
        foreach (var dec in (decisions?.Decisions ?? []).Where(d => string.Equals(d.Decision?.Trim(), "revise", StringComparison.OrdinalIgnoreCase)))
        {
            if (!byId.TryGetValue(dec.CandidateId, out var cand)) { Console.Error.WriteLine($"[l3-revise] HINWEIS: unbekannter Kandidat '{dec.CandidateId}' — übersprungen."); continue; }
            var prior = RevSuffix.Match(dec.CandidateId) is { Success: true } m ? int.Parse(m.Groups[1].Value) : 0;
            if (prior >= MaxRevisions)
                needsHuman.Add(new { candidateId = dec.CandidateId, priorRevisions = prior, status = "NEEDS_HUMAN_RESOLUTION", text = cand.Candidate.Text, reason = "Revisionsschranke erreicht — menschliche Auflösung nötig." });
            else
                toRevise.Add(new L3ReviseItem(cand.Candidate, dec.Reason ?? "", prior));
        }

        if (needsHuman.Count > 0)
            await File.WriteAllTextAsync(Path.Combine(runDir, "needs-human-resolution.json"), JsonSerializer.Serialize(needsHuman, Json)).ConfigureAwait(false);
        if (toRevise.Count == 0)
        {
            Console.WriteLine($"[l3-revise] keine revidierbaren Kandidaten (revise-Entscheidungen: {toRevise.Count}, über Schranke: {needsHuman.Count}).");
            return 0;
        }

        var genSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;
        var judgeSettings = !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;

        var round = toRevise.Max(i => i.PriorRevisions) + 1;
        var suffix = $"-rev{round}";
        var origRun = ReuseRunContext(runDir);

        var genClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, origRun, "L3-Revise", SourceName);
        var resolveClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(genSettings), settings, origRun, "L3-AnchorResolve", SourceName);
        var judgeClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, origRun, "L3-SupportJudge", SourceName);

        var revisePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3Revise1", new Dictionary<string, string>());
        var resolvePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3AnchorResolve1", new Dictionary<string, string>());
        var judgePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, "L3SupportJudge1", new Dictionary<string, string>());

        AIAgent reviseAgent = genClient.AsAIAgent(instructions: revisePrompt, name: AgentName, tools: []);
        reviseAgent = reviseAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(origRun).InvokeAsync).Build();
        AIAgent resolveAgent = resolveClient.AsAIAgent(instructions: resolvePrompt, name: AgentName, tools: []);
        resolveAgent = resolveAgent.AsBuilder().Use(new ToolCallLoggerMiddleware(origRun).InvokeAsync).Build();
        var judge = new InferenceChecker(judgeClient, settings.JuryStructuredOutput, systemPrompt: judgePrompt);

        var workflow = L3Workflow.BuildRevise(
            new L3ReviseExecutor(reviseAgent, origRun),
            new L3AnchorResolveExecutor(resolveAgent, origRun),
            new L3AnchorValidateExecutor(origRun),
            new L3SupportJudgeExecutor(judge, origRun),
            new L3RoutingExecutor(origRun),
            new L3FinalizeExecutor(origRun, suffix));

        Console.WriteLine($"[l3-revise] runId={Path.GetFileName(runDir)}  revise={toRevise.Count} (runde {round}), über-schranke={needsHuman.Count}  genModel={genSettings.ModelId} judgeModel={judgeSettings.ModelId}");
        try
        {
            await InProcessExecution.Default.RunAsync(workflow, new L3ReviseSet(env, toRevise), origRun.RunId, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[l3-revise] Ausführung fehlgeschlagen: {ex.Message}");
            return 4;
        }

        var outReport = Path.Combine(runDir, $"routing-report{suffix}.json");
        if (File.Exists(outReport))
        {
            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(outReport).ConfigureAwait(false));
            Console.WriteLine($"[l3-revise] revidiert+neu klassifiziert: {string.Join(", ", d.RootElement.GetProperty("byClass").EnumerateObject().Select(p => $"{p.Name}={p.Value.GetInt32()}"))}");
        }
        Console.WriteLine($"[l3-revise] -> routing-report{suffix}.json + human-review-package{suffix}.json{(needsHuman.Count > 0 ? " + needs-human-resolution.json" : "")}  run -> {Path.GetRelativePath(repoRoot, runDir)}");
        return 0;
    }

    private static async Task<SourceArtifactSet?> LoadEnvAsync(string runDir, string repoRoot)
    {
        var configPath = Path.Combine(runDir, "config.json");
        if (!File.Exists(configPath)) return null;
        using var cfg = JsonDocument.Parse(await File.ReadAllTextAsync(configPath).ConfigureAwait(false));
        if (!cfg.RootElement.TryGetProperty("env", out var envEl) || envEl.ValueKind != JsonValueKind.Array) return null;
        var sources = new List<ArtifactDocument>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in envEl.EnumerateArray())
        {
            var rel = p.GetString();
            if (string.IsNullOrWhiteSpace(rel)) continue;
            var full = Path.IsPathRooted(rel) ? rel : Path.Combine(repoRoot, rel);
            if (!File.Exists(full)) continue;
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(full).ConfigureAwait(false), Json);
            if (doc is not null && doc.Items.Count > 0 && seen.Add(doc.ArtifactType)) sources.Add(doc);
        }
        return sources.Count > 0 ? new SourceArtifactSet(sources) : null;
    }

    // RunContext, der auf einen EXISTIERENDEN Lauf-Ordner zeigt (Ausgaben/Events in den Original-Lauf schreiben).
    private static RunContext ReuseRunContext(string runDir) => new(Path.GetFileName(runDir), "l3");

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));

    private static string? ResolveRunDir(string repoRoot, string token)
    {
        var full = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (Directory.Exists(full)) return full;
        var l3Root = Path.Combine(repoRoot, "runs", "l3");
        return Directory.Exists(l3Root) ? Directory.EnumerateDirectories(l3Root).FirstOrDefault(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)) : null;
    }

    private sealed record RoutingReport(
        [property: System.Text.Json.Serialization.JsonPropertyName("items")] IReadOnlyList<L3RoutedCandidate> Items);
}
