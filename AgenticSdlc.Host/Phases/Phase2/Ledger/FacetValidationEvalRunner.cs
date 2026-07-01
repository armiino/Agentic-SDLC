using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// L3-B: kontrollierte Messung des <see cref="FacetValidator"/> gegen die autor-bestätigte Fixture —
/// korrekte vs. perturbierte Facetten, batched (fixer Nenner) → sehr wenige Calls.
/// </summary>
/// <remarks>
/// Design: Die Gold-Fixture-Einträge sind korrekt. (1) Korrekt-Set = unverändert → Erwartung verdict=grounded.
/// (2) Perturbiert-Set = je Eintrag Status/Modality/TimeScope auf plausibel-falsche Werte geflippt →
/// Erwartung verdict ∈ {overstated,unsupported,partial} (NICHT grounded). Beide Sets laufen als Batches
/// (Default 8) → bei 11 Einträgen ~2 + ~2 = ~4 Calls statt 22. Kontext = Volltranscript.
/// Metriken: detectionRate (perturbiert korrekt als nicht-grounded erkannt), falseAlarmRate
/// (korrekt fälschlich als nicht-grounded), plus Verdict-Verteilung.
/// Befehl: <c>facet-validation-eval &lt;fixture.json&gt; &lt;transcript.txt&gt; [model] [out.json]</c>.
/// </remarks>
public static class FacetValidationEvalRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private static readonly Dictionary<string, Dictionary<string, string>> Flip = new()
    {
        ["status"] = new() { ["decided"] = "open", ["open"] = "decided", ["undecided"] = "decided", ["required"] = "optional", ["optional"] = "required", ["uncertain"] = "decided", ["rejected"] = "decided", ["desired"] = "required" },
        ["modality"] = new() { ["must"] = "desired", ["desired"] = "must", ["must_clarify"] = "must", ["must_consider"] = "must", ["must_note"] = "must", ["optional"] = "must" },
        ["timeScope"] = new() { ["mvp"] = "later_possible", ["later_possible"] = "mvp", ["mvp_or_later_unclear"] = "mvp", ["null"] = "mvp" }
    };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: facet-validation-eval <fixture.json> <transcript.txt> [model] [out.json]");
            return 2;
        }

        var fixturePath = Resolve(repoRoot, args[1]);
        var transcriptPath = Resolve(repoRoot, args[2]);
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[facet-eval] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[facet-eval] Transcript fehlt: {transcriptPath}"); return 2; }

        var modelArg = args.Length >= 4 ? args[3] : null;
        var outPath = args.Length >= 5 ? Resolve(repoRoot, args[4])
            : Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike", $"facet-validation-eval.{Path.GetFileNameWithoutExtension(fixturePath)}.json");

        var gold = JsonSerializer.Deserialize<SemanticLedgerFixture>(await File.ReadAllTextAsync(fixturePath), Json)?.Entries.ToList() ?? [];
        if (gold.Count == 0) { Console.Error.WriteLine("[facet-eval] leere Fixture."); return 2; }
        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var validator = new FacetValidator(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        // Perturbiert-Set: eindeutige ids (Suffix), damit sich die Batches nicht überschneiden.
        var perturbed = gold.Select(g =>
        {
            var e = g with { Id = g.Id + "__PERT" };
            foreach (var facet in new[] { "status", "modality", "timeScope" })
            {
                var w = Perturb(facet, GoldValue(g, facet));
                if (w is not null) e = With(e, facet, w);
            }
            return e;
        }).ToList();

        Console.WriteLine($"[facet-eval] fixture={Path.GetRelativePath(repoRoot, fixturePath)} gold={gold.Count} model={judgeSettings.ModelId} (batched)");

        var correctV = await validator.ValidateAllAsync(gold, transcript, CancellationToken.None).ConfigureAwait(false);
        var perturbedV = await validator.ValidateAllAsync(perturbed, transcript, CancellationToken.None).ConfigureAwait(false);

        int grCorrect = correctV.Count(v => v.Verdict == "grounded");
        int notGrPert = perturbedV.Count(v => v.Verdict != "grounded");
        var payload = new
        {
            mode = "facet-validation-eval (batched correct-vs-perturbed, author fixture, full transcript)",
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            model = judgeSettings.ModelId,
            goldEntries = gold.Count,
            calls = $"~{(int)Math.Ceiling(gold.Count / (double)FacetValidator.DefaultBatchSize) * 2} (batched, {FacetValidator.DefaultBatchSize}/Batch)",
            metrics = new
            {
                detectionRate = Math.Round(notGrPert / (double)gold.Count, 4),   // perturbiert korrekt als nicht-grounded erkannt
                falseAlarmRate = Math.Round((gold.Count - grCorrect) / (double)gold.Count, 4), // korrekt fälschlich nicht-grounded
                correctGroundedRate = Math.Round(grCorrect / (double)gold.Count, 4)
            },
            verdictDistribution = new
            {
                correct = correctV.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count()),
                perturbed = perturbedV.GroupBy(v => v.Verdict).ToDictionary(g => g.Key, g => g.Count())
            },
            correct = correctV,
            perturbed = perturbedV
        };
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(payload, Json)).ConfigureAwait(false);

        Console.WriteLine($"[facet-eval] detectionRate(perturbiert erkannt)={payload.metrics.detectionRate} falseAlarmRate(korrekt fälschlich)={payload.metrics.falseAlarmRate}");
        Console.WriteLine($"[facet-eval] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static string GoldValue(SemanticLedgerEntry e, string facet) => facet.ToLowerInvariant() switch
    {
        "status" => e.Status, "modality" => e.Modality, "scope" => e.Scope, "timescope" => e.TimeScope ?? "null", _ => ""
    };

    private static string? Perturb(string facet, string value)
    {
        var key = facet.ToLowerInvariant() == "timescope" ? "timeScope" : facet.ToLowerInvariant();
        return Flip.TryGetValue(key, out var map) && map.TryGetValue(value.Trim().ToLowerInvariant(), out var w) ? w : null;
    }

    private static SemanticLedgerEntry With(SemanticLedgerEntry e, string facet, string value) => facet.ToLowerInvariant() switch
    {
        "status" => e with { Status = value },
        "modality" => e with { Modality = value },
        "scope" => e with { Scope = value },
        "timescope" => e with { TimeScope = value },
        _ => e
    };

    private static string Resolve(string repoRoot, string p) => Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p);
}
