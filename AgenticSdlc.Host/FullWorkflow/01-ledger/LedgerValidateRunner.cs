using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Validiert einen BESTEHENDEN Ledger (JSON) mit dem <see cref="FacetValidator"/> — für den realen Test:
/// den Validator auf einen echten Auto-Ledger loslassen und sehen, was er flaggt (kein Gold nötig).
/// </summary>
/// <remarks>
/// Kontext-Modus über das 2. Argument:
///   <c>-</c>         → EVIDENZ-BASIERT (jeder Eintrag nur gegen seine eigene Evidence);
///   &lt;transcript&gt; → Volltranscript-Prüfung.
/// Befehl: <c>ledger-validate &lt;ledger.json&gt; &lt;transcript.txt|-&gt; [model] [out.json]</c>.
/// </remarks>
public static class LedgerValidateRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: ledger-validate <ledger.json> <transcript.txt|-> [model] [out.json]");
            return 2;
        }

        var ledgerPath = Resolve(repoRoot, args[1]);
        if (!File.Exists(ledgerPath)) { Console.Error.WriteLine($"[ledger-validate] Ledger fehlt: {ledgerPath}"); return 2; }

        var evidenceOnly = args[2] == "-";
        var transcript = "";
        if (!evidenceOnly)
        {
            var tp = Resolve(repoRoot, args[2]);
            if (!File.Exists(tp)) { Console.Error.WriteLine($"[ledger-validate] Transcript fehlt: {tp}"); return 2; }
            transcript = await File.ReadAllTextAsync(tp).ConfigureAwait(false);
        }

        var modelArg = args.Length >= 4 ? args[3] : null;
        var mode = evidenceOnly ? "evidence" : "transcript";
        var outPath = args.Length >= 5 ? Resolve(repoRoot, args[4])
            : Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike",
                $"ledger-validate.{Path.GetFileNameWithoutExtension(ledgerPath)}.{mode}.json");

        var entries = LoadEntries(ledgerPath);
        if (entries.Count == 0) { Console.Error.WriteLine("[ledger-validate] leerer Ledger."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var validator = new FacetValidator(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        Console.WriteLine($"[ledger-validate] ledger={Path.GetRelativePath(repoRoot, ledgerPath)} entries={entries.Count} mode={mode} model={judgeSettings.ModelId}");

        var verdicts = await validator.ValidateAllAsync(entries, transcript, CancellationToken.None).ConfigureAwait(false);
        var byId = verdicts.ToDictionary(v => v.Id, v => v, StringComparer.Ordinal);
        var validated = entries.Select(e =>
        {
            var v = byId.TryGetValue(e.Id, out var f) ? f : new EntryValidation(e.Id, "partial", [], "no verdict").Normalized();
            return new ValidatedLedgerEntry(e, v, ValidatedLedgerEntry.DeriveStatus(v));
        }).ToList();

        var issues = validated.SelectMany(v => v.Validation.FacetIssues).ToList();
        var payload = new
        {
            mode = $"ledger-validate ({mode}-context)",
            ledger = Path.GetRelativePath(repoRoot, ledgerPath),
            model = judgeSettings.ModelId,
            entries = validated.Count,
            verdict = new
            {
                grounded = validated.Count(v => v.Validation.Verdict == "grounded"),
                partial = validated.Count(v => v.Validation.Verdict == "partial"),
                overstated = validated.Count(v => v.Validation.Verdict == "overstated"),
                unsupported = validated.Count(v => v.Validation.Verdict == "unsupported")
            },
            facetIssues = new
            {
                total = issues.Count,
                byFacet = issues.GroupBy(i => i.Facet).ToDictionary(g => g.Key, g => g.Count()),
                withRepair = issues.Count(i => i.Suggested is not null)
            },
            items = validated
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(payload, Json)).ConfigureAwait(false);

        var vd = payload.verdict;
        Console.WriteLine($"[ledger-validate] grounded={vd.grounded} partial={vd.partial} overstated={vd.overstated} unsupported={vd.unsupported} | {issues.Count} Facet-Issues");
        Console.WriteLine($"[ledger-validate] -> {Path.GetRelativePath(repoRoot, outPath)}");
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
