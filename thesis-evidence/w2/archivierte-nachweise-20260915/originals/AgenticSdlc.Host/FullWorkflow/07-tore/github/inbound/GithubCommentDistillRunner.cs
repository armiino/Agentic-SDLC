using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Globalization;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

// CLI: github-comment-distill [--comments <issue-comments.json>] [--draft] — die C2d-Zwischenbahn
// (c2d-plan §3): deterministischer Collector (LLM-frei) + bewusster --draft-Schritt (Destillat-Agent).
// Wahrheits-Drafts -> comment-delta.json (naechster Schritt: pipeline-full run --from-delta);
// clarify_answer-Drafts -> sweep-answers.json (naechster Schritt: clarify-sweep run --answers, C4-Bahn).
// Der Runner schreibt NIE Wahrheit — alles faehrt an die bestehenden Gates.
public static class GithubCommentDistillRunner
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string AgentName = "GithubCommentDistillAgent";
    private static readonly JsonSerializerOptions Json = FullWorkflow.JsonFiles.Json;
    private static readonly TimeSpan FreshnessLimit = TimeSpan.FromHours(24);

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        string? commentsArg = null; var draft = false;
        for (var i = 1; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--comments", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) commentsArg = args[++i];
            else if (string.Equals(args[i], "--draft", StringComparison.OrdinalIgnoreCase)) draft = true;
        }
        return await RunDistillAsync(settings, repoRoot, commentsArg, draft).ConfigureAwait(false);
    }

    /// <summary>K13: typisierte Naht (Steward-Seil + CLI-Haut). Exit ≠ 0 = LAUT gescheitert.</summary>
    public static async Task<int> RunDistillAsync(HostSettings settings, string repoRoot, string? commentsArg, bool draft)
    {
        var commentsPath = !string.IsNullOrWhiteSpace(commentsArg)
            ? (Path.IsPathRooted(commentsArg) ? commentsArg : Path.Combine(repoRoot, commentsArg))
            : GithubSnapshotLocator.FindLatestComments(repoRoot);
        if (commentsPath is null)
        { Console.Error.WriteLine("[comment-distill] kein Kommentar-Snapshot (erst: github-snapshot comments --repo <owner/name>)."); return 2; }

        // Frische-Schutz (R-16-Linie, wie die Ernte): NIE still auf altem Diskussions-Stand destillieren.
        if (SnapshotAge(commentsPath) is { } age && age > FreshnessLimit)
        {
            Console.Error.WriteLine($"[comment-distill] COMMENTS_STALE: Kommentar-Snapshot ist {(int)age.TotalHours}h alt (>24h) — "
                + "frisch ziehen: github-snapshot comments --repo <owner/name>.");
            return 2;
        }

        var issuesPath = GithubSnapshotLocator.FindLatest(repoRoot);
        if (issuesPath is null)
        { Console.Error.WriteLine("[comment-distill] kein Issue-Snapshot (Kontext/NiC-Filter) — erst: github-snapshot issues."); return 2; }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[comment-distill] Core fehlt."); return 2; }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var comments = JsonSerializer.Deserialize<List<GithubIssueCommentSnapshot>>(
            await File.ReadAllTextAsync(commentsPath).ConfigureAwait(false), Json) ?? [];
        var issues = JsonSerializer.Deserialize<List<GithubIssueSnapshot>>(
            await File.ReadAllTextAsync(issuesPath).ConfigureAwait(false), Json) ?? [];

        var run = new RunContext(RunId.New(), "github-comment-distill");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        var finds = GithubCommentDistill.Collect(core, issues, comments);
        await File.WriteAllTextAsync(Path.Combine(outDir, "comment-finds.json"),
            JsonSerializer.Serialize(finds, Json)).ConfigureAwait(false);
        run.AppendEvent(new { type = "COMMENT_DISTILL_COLLECTED", runId = run.RunId, finds = finds.Count, comments = comments.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[comment-distill] kommentare={comments.Count} neue-funde={finds.Count} (Anker-gefiltert, NiC raus)");

        if (finds.Count == 0 || !draft)
        {
            if (draft) Console.WriteLine("[comment-distill] nichts Neues seit den Ankern — kein Destillat noetig.");
            else if (finds.Count > 0) Console.WriteLine("[comment-distill] Destillat = bewusster LLM-Schritt: --draft");
            return 0;
        }

        // Der bewusste LLM-Schritt (Haus-Muster: Prompt + Save-Once-Zaum; Denk-/Tool-Faden in den Lauf-Logs).
        using var otel = OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
            Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
        var prompt = PromptProvider.Load(repoRoot, "phase2_evidence", AgentName, "GithubCommentDistillAgent1",
            new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(settings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        IReadOnlyList<GithubCommentDraft> drafts;
        try { drafts = await GithubCommentDistill.DraftAsync(factory, finds).ConfigureAwait(false); }
        catch (InvalidOperationException ex) { Console.Error.WriteLine($"[comment-distill] {ex.Message}"); return 4; }
        await File.WriteAllTextAsync(Path.Combine(outDir, "comment-drafts.json"),
            JsonSerializer.Serialize(drafts, Json)).ConfigureAwait(false);

        // Wahrheits-Drafts -> Delta im Meeting-Ketten-Vertrag (9i-Schiene inklusive; Anker reist als Metadata).
        // Geteilte Konvertierung mit dem --from-github-Harvest (Zwei-Bahnen-Regel).
        var truthDrafts = GithubCommentDistill.ToInboundDrafts(drafts);
        var syntheticFinds = GithubCommentDistill.ToSyntheticFinds(finds);
        string? deltaRel = null;
        if (truthDrafts.Count > 0)
        {
            var delta = GithubInboundDeltaBuilder.Build(truthDrafts, syntheticFinds, issues, run.RunId);
            var deltaPath = Path.Combine(outDir, "comment-delta.json");
            await File.WriteAllTextAsync(deltaPath, JsonSerializer.Serialize(delta, Delta.ProjectStateJson.Options)).ConfigureAwait(false);
            deltaRel = Path.GetRelativePath(repoRoot, deltaPath);
        }

        // §3-4 Sonderfall: clarify_answer -> C4-Bahn (eigener gated Folge-Lauf, bewusste ①-Grenze).
        var clarifyAnswers = GithubCommentDistill.ToClarifyAnswers(drafts);
        string? answersRel = null;
        if (clarifyAnswers.Count > 0)
        {
            var answersPath = Path.Combine(outDir, "sweep-answers.json");
            await File.WriteAllTextAsync(answersPath, JsonSerializer.Serialize(clarifyAnswers, Json)).ConfigureAwait(false);
            answersRel = Path.GetRelativePath(repoRoot, answersPath);
        }

        var noise = drafts.Count(d => d.Disposition == GithubCommentDisposition.Noise);
        run.AppendEvent(new { type = "COMMENT_DISTILL_DRAFTED", runId = run.RunId, drafts = drafts.Count, truth = truthDrafts.Count, clarify = clarifyAnswers.Count, noise, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[comment-distill] drafts={drafts.Count} wahrheit={truthDrafts.Count} clarify={clarifyAnswers.Count} noise={noise}");
        if (deltaRel is not null) Console.WriteLine($"[comment-distill] naechster Schritt: pipeline-full run --from-delta {deltaRel}");
        if (answersRel is not null) Console.WriteLine($"[comment-distill] naechster Schritt (C4-Bahn): clarify-sweep run --answers {answersRel}");
        return 0;
    }

    private static TimeSpan? SnapshotAge(string commentsPath)
    {
        var summaryPath = Path.Combine(Path.GetDirectoryName(commentsPath)!, "issue-comments-summary.json");
        if (!File.Exists(summaryPath)) return null;   // Alt-/Hand-Artefakt ohne Summary: kein Frische-Urteil moeglich
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(summaryPath));
            if (doc.RootElement.TryGetProperty("timestampUtc", out var t)
                && DateTime.TryParse(t.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var ts))
                return DateTime.UtcNow - ts;
        }
        catch { /* kaputte Summary = kein Urteil, aber kein Absturz */ }
        return null;
    }
}
