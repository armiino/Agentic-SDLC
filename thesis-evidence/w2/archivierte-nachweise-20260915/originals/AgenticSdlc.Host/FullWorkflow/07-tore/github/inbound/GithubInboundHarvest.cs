using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Globalization;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;

/// <summary>
/// C2c (08.08.2026, §13 c2-inbound-plan) — die EINE Ernte-Quelle (Zwei-Bahnen-Bauregel): Snapshot-Auflösung
/// + Frische-Schutz + Detect + (optional) Agent-Drafting + Delta-Bau + Artefakte. Konsumenten: die
/// Zwischenbahn-CLI `github-inbound` UND der Ein-Graph-Eingang `pipeline-full run --from-github` — beide
/// Bahnen ernten IDENTISCH, wie beim Ledger (CLI-Bahn + Front im Graph).
/// </summary>
public static class GithubInboundHarvest
{
    private const string SourceName = "AgenticSdlc.Host";
    private const string AgentName = "GithubInboundAgent";
    private static readonly JsonSerializerOptions Json = FullWorkflow.JsonFiles.Json;
    private static readonly TimeSpan FreshnessLimit = TimeSpan.FromHours(24);

    public sealed record HarvestResult(
        GithubInboundReport Report,
        IReadOnlyList<GithubInboundDraft> Drafts,
        ProjectStateDocument? Delta,
        string SnapshotRel);

    /// <summary>Exit ≠ 0 = LAUT gescheitert (Meldung bereits auf stderr). createOtel=false, wenn der Aufrufer
    /// (pipeline-full-Faden) bereits einen Exporter hält — sonst doppelte Spans.</summary>
    public static async Task<(int ExitCode, HarvestResult? Result)> RunAsync(
        HostSettings settings, string repoRoot, RunContext run, string? issuesArg, bool draft, string outDir,
        bool createOtel = true,
        // C2d: optionaler Kommentar-Snapshot — dann destilliert die Ernte AUCH die Diskussionen (Fan-in:
        // Body-Edits, Neu-Issues UND Kommentare münden in EIN Delta an dieselben Tore).
        string? commentsArg = null)
    {
        var snapshotPath = !string.IsNullOrWhiteSpace(issuesArg)
            ? (Path.IsPathRooted(issuesArg) ? issuesArg : Path.Combine(repoRoot, issuesArg))
            : GithubSnapshotLocator.FindLatest(repoRoot);
        if (snapshotPath is null)
        { Console.Error.WriteLine("[github-inbound] kein Issue-Snapshot (erst: github-snapshot issues --repo <owner/name>)."); return (2, null); }

        // Frische-Schutz (§2, R-16-Linie): Ernte NIE still auf altem Stand — LAUT abbrechen statt raten.
        if (SnapshotAge(snapshotPath) is { } a && a > FreshnessLimit)
        {
            Console.Error.WriteLine($"[github-inbound] SNAPSHOT_STALE: Snapshot ist {(int)a.TotalHours}h alt (>24h) — "
                + "frisch ziehen: github-snapshot issues --repo <owner/name>.");
            return (2, null);
        }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        { Console.Error.WriteLine("[github-inbound] Core fehlt."); return (2, null); }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        var issues = JsonSerializer.Deserialize<List<GithubIssueSnapshot>>(
            await File.ReadAllTextAsync(snapshotPath).ConfigureAwait(false), Json) ?? [];

        var report = GithubInboundDetect.Detect(core, issues);
        var snapshotRel = Path.GetRelativePath(repoRoot, snapshotPath);
        await File.WriteAllTextAsync(Path.Combine(outDir, "harvest-report.json"),
            JsonSerializer.Serialize(new { snapshot = snapshotRel, report }, Json)).ConfigureAwait(false);

        var byCat = report.Finds.GroupBy(f => f.Category).ToDictionary(g => g.Key, g => g.Count());
        Console.WriteLine($"[github-inbound] issues={report.TotalIssues} unchanged={report.Unchanged} "
            + $"drift={byCat.GetValueOrDefault(GithubInboundCategory.MappedDrift)} "
            + $"neu={byCat.GetValueOrDefault(GithubInboundCategory.UnmappedNew)} "
            + $"nic={byCat.GetValueOrDefault(GithubInboundCategory.NicOptOut)} "
            + $"unstamped={byCat.GetValueOrDefault(GithubInboundCategory.UnknownStamp)} skipped={report.Skipped.Count}");
        foreach (var f in report.Finds.Where(f => f.Category is GithubInboundCategory.MappedDrift or GithubInboundCategory.UnmappedNew or GithubInboundCategory.AdoptedDrift))
            Console.WriteLine($"[github-inbound]   {f.Category} #{f.IssueNumber} {(f.PbiId is null ? "" : $"({f.PbiId}) ")}{(f.AdoptedItemId is null ? "" : $"({f.AdoptedItemId}) ")}— {f.Details[0]}");

        var harvestable = report.Finds
            .Where(f => f.Category is GithubInboundCategory.MappedDrift or GithubInboundCategory.UnmappedNew or GithubInboundCategory.AdoptedDrift).ToList();

        // C2d: Kommentar-Funde über die geteilte Distill-Naht (Anker-Filter, NiC-Opt-out, Core-Kontext).
        IReadOnlyList<GithubCommentFind> commentFinds = [];
        if (!string.IsNullOrWhiteSpace(commentsArg))
        {
            var commentsPath = Path.IsPathRooted(commentsArg) ? commentsArg : Path.Combine(repoRoot, commentsArg);
            if (!File.Exists(commentsPath))
            { Console.Error.WriteLine($"[github-inbound] Kommentar-Snapshot nicht gefunden: {commentsPath}"); return (2, null); }
            var comments = JsonSerializer.Deserialize<List<GithubIssueCommentSnapshot>>(
                await File.ReadAllTextAsync(commentsPath).ConfigureAwait(false), Json) ?? [];
            commentFinds = GithubCommentDistill.Collect(core, issues, comments);
            await File.WriteAllTextAsync(Path.Combine(outDir, "comment-finds.json"),
                JsonSerializer.Serialize(commentFinds, Json)).ConfigureAwait(false);
            Console.WriteLine($"[github-inbound] kommentare={comments.Count} neue-kommentar-funde={commentFinds.Count} (Anker-gefiltert)");
        }

        if (!draft || (harvestable.Count == 0 && commentFinds.Count == 0))
        {
            if (draft) Console.WriteLine("[github-inbound] keine Ernte-Funde — kein Drafting noetig.");
            return (0, new HarvestResult(report, [], null, snapshotRel));
        }

        // Der bewusste LLM-Schritt: InboundAgent deutet die Issue-Funde (Haus-Muster: Prompt + Save-Once-Tool).
        using var otel = createOtel
            ? OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
                Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
                settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null)
            : null;
        IReadOnlyList<GithubInboundDraft> drafts = [];
        if (harvestable.Count > 0)
        {
            var prompt = PromptProvider.Load(repoRoot, "phase2_evidence", AgentName, "GithubInboundAgent1",
                new Dictionary<string, string> { ["runId"] = run.RunId });
            var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(settings), settings, run, AgentName, SourceName);
            Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
                client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                    .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            try { drafts = await GithubInboundDraftAgent.DraftAsync(factory, harvestable).ConfigureAwait(false); }
            catch (InvalidOperationException ex) { Console.Error.WriteLine($"[github-inbound] {ex.Message}"); return (4, null); }
        }

        // C2d: der ZWEITE bewusste Deutungs-Schritt — der Destillat-Agent für die Diskussionen (eigener
        // Prompt/Zaum; gleiche Observability-Nähte im selben Lauf-Faden).
        IReadOnlyList<GithubCommentDraft> commentDrafts = [];
        if (commentFinds.Count > 0)
        {
            const string DistillAgentName = "GithubCommentDistillAgent";
            var distillPrompt = PromptProvider.Load(repoRoot, "phase2_evidence", DistillAgentName, "GithubCommentDistillAgent1",
                new Dictionary<string, string> { ["runId"] = run.RunId });
            var distillClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(settings), settings, run, DistillAgentName, SourceName);
            Func<IReadOnlyList<AITool>, AIAgent> distillFactory = tools =>
                distillClient.AsAIAgent(instructions: distillPrompt, name: DistillAgentName, tools: [.. tools])
                    .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();
            try { commentDrafts = await GithubCommentDistill.DraftAsync(distillFactory, commentFinds).ConfigureAwait(false); }
            catch (InvalidOperationException ex) { Console.Error.WriteLine($"[github-inbound] {ex.Message}"); return (4, null); }
            await File.WriteAllTextAsync(Path.Combine(outDir, "comment-drafts.json"),
                JsonSerializer.Serialize(commentDrafts, Json)).ConfigureAwait(false);

            // §3-4 Sonderfall: clarify_answer → C4-Bahn (eigener gated Folge-Lauf, bewusste ①-Grenze).
            var clarifyAnswers = GithubCommentDistill.ToClarifyAnswers(commentDrafts);
            if (clarifyAnswers.Count > 0)
            {
                var answersPath = Path.Combine(outDir, "sweep-answers.json");
                await File.WriteAllTextAsync(answersPath, JsonSerializer.Serialize(clarifyAnswers, Json)).ConfigureAwait(false);
                Console.WriteLine($"[github-inbound] clarify_answer={clarifyAnswers.Count} -> C4-Bahn: "
                    + $"clarify-sweep run --answers {Path.GetRelativePath(repoRoot, answersPath)}");
            }
        }

        // Fan-in: Issue-Drafts + Kommentar-Destillate → EIN Delta an dieselben Tore (geteilte Konvertierung).
        var deltaDrafts = drafts.Concat(GithubCommentDistill.ToInboundDrafts(commentDrafts)).ToList();
        var deltaFinds = harvestable.Concat(GithubCommentDistill.ToSyntheticFinds(commentFinds)).ToList();
        var delta = GithubInboundDeltaBuilder.Build(deltaDrafts, deltaFinds, issues, run.RunId);
        await File.WriteAllTextAsync(Path.Combine(outDir, "inbound-drafts.json"),
            JsonSerializer.Serialize(deltaDrafts, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "inbound-delta.json"),
            JsonSerializer.Serialize(delta, ProjectStateJson.Options)).ConfigureAwait(false);

        var parked = deltaDrafts.Count(d => d.Disposition == GithubInboundDisposition.OpenQuestion);
        var noise = drafts.Count(d => d.Disposition == GithubInboundDisposition.Noise)
                    + commentDrafts.Count(d => d.Disposition == GithubCommentDisposition.Noise);
        Console.WriteLine($"[github-inbound] drafts={deltaDrafts.Count} -> delta-items={delta.Items.Count}"
            + (commentDrafts.Count > 0 ? $" · kommentar-destillate={commentDrafts.Count}" : "")
            + (parked > 0 ? $" · open_question={parked} (9i: faehrt im Delta zur 9g-Schiene -> DEC-Topf, ingest-Gate entscheidet)" : "")
            + (noise > 0 ? $" · noise={noise} (nur Report)" : ""));
        return (0, new HarvestResult(report, deltaDrafts, delta.Items.Count > 0 ? delta : null, snapshotRel));
    }

    private static TimeSpan? SnapshotAge(string snapshotPath)
    {
        var summaryPath = Path.Combine(Path.GetDirectoryName(snapshotPath)!, GithubSnapshotLocator.SummaryFileName);
        if (File.Exists(summaryPath))
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(summaryPath));
                if (doc.RootElement.TryGetProperty("timestampUtc", out var t)
                    && DateTime.TryParse(t.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var ts))
                    return DateTime.UtcNow - ts;
            }
            catch (JsonException) { /* unlesbar → Datei-Zeit als Fallback */ }
        }
        return DateTime.UtcNow - File.GetLastWriteTimeUtc(snapshotPath);
    }
}
