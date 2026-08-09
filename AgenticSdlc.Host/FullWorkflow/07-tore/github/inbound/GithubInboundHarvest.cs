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
        bool createOtel = true)
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
        foreach (var f in report.Finds.Where(f => f.Category is GithubInboundCategory.MappedDrift or GithubInboundCategory.UnmappedNew))
            Console.WriteLine($"[github-inbound]   {f.Category} #{f.IssueNumber} {(f.PbiId is null ? "" : $"({f.PbiId}) ")}— {f.Details[0]}");

        var harvestable = report.Finds
            .Where(f => f.Category is GithubInboundCategory.MappedDrift or GithubInboundCategory.UnmappedNew).ToList();
        if (!draft || harvestable.Count == 0)
        {
            if (draft) Console.WriteLine("[github-inbound] keine Ernte-Funde — kein Drafting noetig.");
            return (0, new HarvestResult(report, [], null, snapshotRel));
        }

        // Der bewusste LLM-Schritt: InboundAgent deutet die Funde (Haus-Muster: Prompt + Save-Once-Tool).
        using var otel = createOtel
            ? OtelRunExporters.TryCreate(settings.OtelEnabled, SourceName,
                Path.Combine(run.LogsDir, "otel-traces.jsonl"), Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
                settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null)
            : null;
        var prompt = PromptProvider.Load(repoRoot, "phase2_evidence", AgentName, "GithubInboundAgent1",
            new Dictionary<string, string> { ["runId"] = run.RunId });
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(settings), settings, run, AgentName, SourceName);
        Func<IReadOnlyList<AITool>, AIAgent> factory = tools =>
            client.AsAIAgent(instructions: prompt, name: AgentName, tools: [.. tools])
                .AsBuilder().Use(new ToolCallLoggerMiddleware(run).InvokeAsync).Build();

        IReadOnlyList<GithubInboundDraft> drafts;
        try { drafts = await GithubInboundDraftAgent.DraftAsync(factory, harvestable).ConfigureAwait(false); }
        catch (InvalidOperationException ex) { Console.Error.WriteLine($"[github-inbound] {ex.Message}"); return (4, null); }

        var delta = GithubInboundDeltaBuilder.Build(drafts, harvestable, issues, run.RunId);
        await File.WriteAllTextAsync(Path.Combine(outDir, "inbound-drafts.json"),
            JsonSerializer.Serialize(drafts, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "inbound-delta.json"),
            JsonSerializer.Serialize(delta, ProjectStateJson.Options)).ConfigureAwait(false);

        var parked = drafts.Count(d => d.Disposition == GithubInboundDisposition.OpenQuestion);
        var noise = drafts.Count(d => d.Disposition == GithubInboundDisposition.Noise);
        Console.WriteLine($"[github-inbound] drafts={drafts.Count} -> delta-items={delta.Items.Count}"
            + (parked > 0 ? $" · open_question={parked} (Fragen-Schienen-Anschluss = 9i-Spur, sichtbar in inbound-drafts.json)" : "")
            + (noise > 0 ? $" · noise={noise} (nur Report)" : ""));
        return (0, new HarvestResult(report, drafts, delta, snapshotRel));
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
