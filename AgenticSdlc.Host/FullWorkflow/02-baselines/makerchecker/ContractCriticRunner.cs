using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Ledger;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker;

/// <summary>
/// C7 / MC3-CLI: <c>contract-critic &lt;requirements.md&gt; &lt;consumable.json&gt; [model] [out.json]</c>.
/// Der bounded Evidence-Support-Critic (LLM, temp=0). Prüft je Zeile die Detail-Deckung gegen das ZITIERTE Paket —
/// KEIN Rohtranskript. Delta-Vorfilter spart die reinen Propositions-Umschreibungen (0 LLM). Schreibt
/// <c>critic-report.json</c>. Exit: 0 = alle gedeckt, 1 = ungedeckte Details/Verstärkungen, 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
public static class ContractCriticRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: contract-critic <artifact.md> <consumable.json> [model] [out.json] [--repeat k] [--min-votes n]");
            return 2;
        }

        var reqPath = Resolve(repoRoot, args[1]);
        var consPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(reqPath)) { Console.Error.WriteLine($"[contract-critic] requirements fehlt: {reqPath}"); return 2; }
        if (!File.Exists(consPath)) { Console.Error.WriteLine($"[contract-critic] consumable fehlt: {consPath}"); return 2; }

        string? modelArg = null, outArg = null;
        var repeat = 1;
        var minVotes = 0;
        for (var i = 3; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--repeat", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                { repeat = ParseInt(args[++i], repeat); }
            else if (string.Equals(args[i], "--min-votes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                { minVotes = ParseInt(args[++i], minVotes); }
            else if (args[i].StartsWith("--", StringComparison.Ordinal))
                { /* unbekanntes Flag ignorieren */ }
            else if (modelArg is null) { modelArg = args[i]; }
            else if (outArg is null) { outArg = args[i]; }
        }
        var outPath = outArg is not null ? Resolve(repoRoot, outArg)
            : Path.Combine(Path.GetDirectoryName(reqPath) ?? ".", "critic-report.json");

        var markdown = await File.ReadAllTextAsync(reqPath).ConfigureAwait(false);
        var ledger = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(consPath).ConfigureAwait(false), Json);
        if (ledger is null || ledger.Claims.Count == 0)
        {
            Console.Error.WriteLine("[contract-critic] consumable leer/nicht lesbar.");
            return 2;
        }

        // Modell-Auflösung wie beim Refine-Runner: [model]-Arg > Jury-Judge-Modell > Default-Modell.
        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var critic = new ContractCritic(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
        Console.WriteLine($"[contract-critic] artifact={Path.GetRelativePath(repoRoot, reqPath)}  claims={ledger.Claims.Count}  model={judgeSettings.ModelId}  repeat={repeat}");

        try
        {
            if (repeat > 1)
            {
                var vote = await critic.CheckWithVoteAsync(markdown, ledger, repeat, minVotes, CancellationToken.None).ConfigureAwait(false);
                await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(vote, Json)).ConfigureAwait(false);
                PrintVoteSummary(vote, Path.GetRelativePath(repoRoot, outPath));
                return vote.Pass ? 0 : 1;
            }

            var report = await critic.CheckAsync(markdown, ledger, CancellationToken.None).ConfigureAwait(false);
            await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);
            PrintSummary(report, Path.GetRelativePath(repoRoot, outPath));
            return report.Pass ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[contract-critic] LLM-Prüfung fehlgeschlagen: {ex.Message}");
            return 4;
        }
    }

    private static void PrintVoteSummary(CriticVoteReport r, string outRel)
    {
        Console.WriteLine($"[contract-critic] k={r.K} min-votes={r.MinViolationVotes} | Zeilen={r.RequirementLines} " +
                          $"Vorfilter(0 LLM)={r.SkippedByPrefilter} LLM/Lauf={r.LlmCheckedPerRun} calls_total={r.TotalLlmCalls}");
        var flipped = r.Votes.Where(v => v.Votes.Count > 1).ToList();
        Console.WriteLine($"[contract-critic] PASS={r.Pass}  bestätigte violations={r.Violations.Count}  (Zeilen mit uneinigen Stimmen: {flipped.Count})");
        foreach (var v in r.Votes.Where(v => v.AggregatedVerdict is C7Verdict.EvidenceUnsupportedDetail or C7Verdict.FacetOverstated))
        {
            Console.WriteLine($"    ! L{v.LineNumber} [{v.AggregatedVerdict}] {FormatVotes(v.Votes)}  {v.Line}");
            if (!string.IsNullOrWhiteSpace(v.UnsupportedSpan)) Console.WriteLine($"        span: {v.UnsupportedSpan}");
        }
        foreach (var v in flipped.Where(v => v.AggregatedVerdict == C7Verdict.Supported && v.ViolationVotes > 0))
            Console.WriteLine($"    ~ L{v.LineNumber} ausgevotet {FormatVotes(v.Votes)}  {Trunc(v.Line)}");
        Console.WriteLine($"[contract-critic] report -> {outRel}");
    }

    private static string FormatVotes(IReadOnlyDictionary<string, int> d)
        => "{" + string.Join(",", d.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}")) + "}";

    private static string Trunc(string s) => s.Length <= 70 ? s : s[..70] + "…";

    private static int ParseInt(string s, int fallback) => int.TryParse(s, out var v) ? v : fallback;

    private static void PrintSummary(CriticReport report, string outRel)
    {
        Console.WriteLine($"[contract-critic] Zeilen={report.RequirementLines} zitiert={report.CitedCandidates} " +
                          $"| Vorfilter-supported(0 LLM)={report.SkippedByPrefilter} LLM-geprüft={report.LlmChecked} in {report.LlmCalls} Call(s)");
        Console.WriteLine($"[contract-critic] PASS={report.Pass}  violations={report.Violations.Count}");

        var byVerdict = report.Verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count());
        foreach (var kv in byVerdict.OrderBy(k => k.Key.ToString()))
            Console.WriteLine($"    {kv.Key}: {kv.Value}");

        foreach (var v in report.Verdicts.Where(v => v.Verdict is C7Verdict.EvidenceUnsupportedDetail or C7Verdict.FacetOverstated))
        {
            Console.WriteLine($"    ! L{v.LineNumber} [{v.Verdict}] {v.Line}");
            if (!string.IsNullOrWhiteSpace(v.UnsupportedSpan)) Console.WriteLine($"        span: {v.UnsupportedSpan}");
            if (!string.IsNullOrWhiteSpace(v.Rationale)) Console.WriteLine($"        grund: {v.Rationale}");
        }
        Console.WriteLine($"[contract-critic] report -> {outRel}");
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
