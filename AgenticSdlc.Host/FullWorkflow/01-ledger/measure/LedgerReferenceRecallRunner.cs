using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Misst den <b>Recall</b> eines Auto-Ledgers gegen einen Referenz-Ledger: Findet die automatische
/// Extraktion die Claims, die laut Referenz im Transkript stehen? (= die (B)-Frage „fehlen wirklich Sachen").
/// </summary>
/// <remarks>
/// Reine Mess-Achse, KEIN Teil des Workflows. Pro Referenz-Eintrag wird via
/// <see cref="SemanticLedgerRecallMatcher"/> der beste Match im Auto-Ledger gesucht (exact/partial/missed
/// + Facetten). Validität hängt an der Referenz: gegen die **autor-bestätigte** 11er-Fixture ist die Zahl
/// NICHT zirkulär; gegen einen modell-entworfenen Voll-Referenz-Ledger ist sie vorläufig (LLM-gegen-LLM),
/// bis der Autor adjudiziert hat. Beides wird im Output (`referenceStatus`) vermerkt.
/// Befehl: <c>ledger-reference-recall &lt;reference.json&gt; &lt;auto-ledger.json&gt; [model] [out.json]</c>.
/// </remarks>
public static class LedgerReferenceRecallRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: ledger-reference-recall <reference.json> <auto-ledger.json> [model] [out.json]");
            return 2;
        }

        var refPath = Resolve(repoRoot, args[1]);
        var autoPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(refPath)) { Console.Error.WriteLine($"[ref-recall] Referenz fehlt: {refPath}"); return 2; }
        if (!File.Exists(autoPath)) { Console.Error.WriteLine($"[ref-recall] Auto-Ledger fehlt: {autoPath}"); return 2; }

        var modelArg = args.Length >= 4 ? args[3] : null;
        var outPath = args.Length >= 5
            ? Resolve(repoRoot, args[4])
            : Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike",
                $"ledger-reference-recall.{Path.GetFileNameWithoutExtension(refPath)}.json");

        var reference = LoadEntries(refPath);
        var auto = LoadEntries(autoPath);
        if (reference.Count == 0) { Console.Error.WriteLine("[ref-recall] leere Referenz."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        // Heuristik fürs Protokoll: enthält der Dateiname "reference"/"draft" -> modell-entworfen (vorläufig),
        // sonst (z. B. die autor-bestätigten *-spike.json Fixtures) -> autor-bestätigt (nicht zirkulär).
        var refLower = Path.GetFileName(refPath).ToLowerInvariant();
        var referenceStatus = refLower.Contains("draft") || refLower.Contains("reference-template")
            ? "model-drafted (provisional, NOT author-adjudicated -> recall is circular until reviewed)"
            : "author-confirmed-or-external (recall is non-circular w.r.t. this reference)";

        // W1b: Judge-Call observability-verdrahtet (ChatDecisionLogger + InputContext + OTel) über AgentChatPipelineBuilder.Build.
        var run = new RunContext(RunId.New(), "ledger-reference-recall");
        run.EnsureFolders();
        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled,
            sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);
        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "SemanticLedgerRecallMatcher", SourceName);
        var matcher = new SemanticLedgerRecallMatcher(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[ref-recall] reference={Path.GetRelativePath(repoRoot, refPath)} ({reference.Count}) auto={Path.GetRelativePath(repoRoot, autoPath)} ({auto.Count}) model={judgeSettings.ModelId}");
        Console.WriteLine($"[ref-recall] referenceStatus: {referenceStatus}");

        var matches = new List<object>();
        int exact = 0, partial = 0, missed = 0;
        var facetExact = new Dictionary<string, int>
        {
            ["proposition"] = 0, ["status"] = 0, ["modality"] = 0, ["scope"] = 0,
            ["timeScope"] = 0, ["evidence"] = 0, ["disposition"] = 0
        };

        foreach (var exp in reference)
        {
            var v = await matcher.MatchAsync(exp, auto, CancellationToken.None).ConfigureAwait(false);
            if (v.Verdict == "exact") exact++; else if (v.Verdict == "partial") partial++; else missed++;
            if (v.PropositionMatch == "exact") facetExact["proposition"]++;
            if (v.StatusMatch == "exact") facetExact["status"]++;
            if (v.ModalityMatch == "exact") facetExact["modality"]++;
            if (v.ScopeMatch == "exact") facetExact["scope"]++;
            if (v.TimeScopeMatch == "exact") facetExact["timeScope"]++;
            if (v.EvidenceMatch == "exact") facetExact["evidence"]++;
            if (v.DispositionMatch == "exact") facetExact["disposition"]++;
            Console.WriteLine($"[{v.Verdict.ToUpperInvariant()}] {exp.Id} -> {string.Join(",", v.MatchedIds)} :: {v.Reason}");
            matches.Add(new { expected = exp, verdict = v });
        }

        var payload = new
        {
            mode = "ledger-reference-recall",
            reference = Path.GetRelativePath(repoRoot, refPath),
            referenceStatus,
            autoLedger = Path.GetRelativePath(repoRoot, autoPath),
            model = judgeSettings.ModelId,
            referenceCount = reference.Count,
            autoCount = auto.Count,
            metrics = new
            {
                exact, partial, missed,
                recallExact = Math.Round(exact / (double)reference.Count, 4),
                recallExactOrPartial = Math.Round((exact + partial) / (double)reference.Count, 4),
                facetExact = facetExact.ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value / (double)reference.Count, 4))
            },
            matches
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(payload, Json)).ConfigureAwait(false);

        Console.WriteLine($"[ref-recall] exact={exact}/{reference.Count} partial={partial}/{reference.Count} missed={missed}/{reference.Count}");
        Console.WriteLine($"[ref-recall] recall(exact-or-partial)={Math.Round((exact + partial) / (double)reference.Count, 4)}");
        Console.WriteLine($"[ref-recall] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static IReadOnlyList<SemanticLedgerEntry> LoadEntries(string path)
    {
        var text = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(text);
        return doc.RootElement.ValueKind == JsonValueKind.Array
            ? JsonSerializer.Deserialize<List<SemanticLedgerEntry>>(text, Json) ?? []
            : JsonSerializer.Deserialize<SemanticLedgerFixture>(text, Json)?.Entries ?? [];
    }

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
