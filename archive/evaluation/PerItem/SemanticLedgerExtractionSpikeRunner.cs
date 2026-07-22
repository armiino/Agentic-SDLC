using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Testet Transkript -> Semantic Ledger gegen eine bestaetigte Semantic-Ledger-Fixture.</summary>
public static class SemanticLedgerExtractionSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: semantic-ledger-extraction-spike <fixture.json> <transcript.txt> [modelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var transcriptPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var modelArg = args.Length >= 4 ? args[3] : null;
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[semantic-ledger] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[semantic-ledger] Transkript fehlt: {transcriptPath}"); return 2; }

        var fixture = JsonSerializer.Deserialize<SemanticLedgerFixture>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var expected = fixture?.Entries.ToList() ?? [];
        if (expected.Count == 0) { Console.Error.WriteLine("[semantic-ledger] leere Fixture."); return 2; }

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var client = ChatClientFactory.Create(judgeSettings);

        Console.WriteLine($"[semantic-ledger] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Transcript={Path.GetRelativePath(repoRoot, transcriptPath)} Expected={expected.Count} Model={judgeSettings.ModelId}");

        var extractor = new SemanticLedgerExtractor(client, settings.JuryStructuredOutput);
        var canonicalizer = new SemanticLedgerCanonicalizer(client, settings.JuryStructuredOutput);
        var matcher = new SemanticLedgerRecallMatcher(client, settings.JuryStructuredOutput);

        Console.WriteLine("[semantic-ledger] extracting ledger...");
        var extracted = await extractor.ExtractAsync(transcript, CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"[semantic-ledger] extracted={extracted.Count}");

        Console.WriteLine("[semantic-ledger] canonicalizing ledger...");
        var canonical = await canonicalizer.CanonicalizeAsync(extracted, CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"[semantic-ledger] canonical={canonical.Count}");

        var matches = new List<object>();
        var exact = 0;
        var partial = 0;
        var missed = 0;
        var facetExact = new Dictionary<string, int>
        {
            ["proposition"] = 0,
            ["status"] = 0,
            ["modality"] = 0,
            ["scope"] = 0,
            ["timeScope"] = 0,
            ["evidence"] = 0,
            ["disposition"] = 0
        };

        foreach (var exp in expected)
        {
            var verdict = await matcher.MatchAsync(exp, canonical, CancellationToken.None).ConfigureAwait(false);
            if (verdict.Verdict == "exact") exact++;
            else if (verdict.Verdict == "partial") partial++;
            else missed++;

            if (verdict.PropositionMatch == "exact") facetExact["proposition"]++;
            if (verdict.StatusMatch == "exact") facetExact["status"]++;
            if (verdict.ModalityMatch == "exact") facetExact["modality"]++;
            if (verdict.ScopeMatch == "exact") facetExact["scope"]++;
            if (verdict.TimeScopeMatch == "exact") facetExact["timeScope"]++;
            if (verdict.EvidenceMatch == "exact") facetExact["evidence"]++;
            if (verdict.DispositionMatch == "exact") facetExact["disposition"]++;

            Console.WriteLine($"[{verdict.Verdict.ToUpperInvariant()}] {exp.Id} -> {string.Join(",", verdict.MatchedIds)} :: {verdict.Reason}");
            matches.Add(new { expected = exp, verdict });
        }

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike");
        Directory.CreateDirectory(outDir);
        var transcriptStem = Path.GetFileNameWithoutExtension(transcriptPath);
        var prefix = $"semantic-ledger-extraction.{transcriptStem}.{modelSlug}";
        var extractedPath = Path.Combine(outDir, $"{prefix}.extracted.json");
        var canonicalPath = Path.Combine(outDir, $"{prefix}.canonical.json");
        var resultPath = Path.Combine(outDir, $"{prefix}.matches.json");

        await File.WriteAllTextAsync(extractedPath, JsonSerializer.Serialize(extracted, JsonOptions)).ConfigureAwait(false);
        await File.WriteAllTextAsync(canonicalPath, JsonSerializer.Serialize(canonical, JsonOptions)).ConfigureAwait(false);
        var payload = new
        {
            mode = "semantic-ledger-extraction-spike",
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            transcript = Path.GetRelativePath(repoRoot, transcriptPath),
            model = judgeSettings.ModelId,
            extracted = Path.GetRelativePath(repoRoot, extractedPath),
            canonical = Path.GetRelativePath(repoRoot, canonicalPath),
            expected = expected.Count,
            extractedCount = extracted.Count,
            canonicalCount = canonical.Count,
            metrics = new
            {
                exact,
                partial,
                missed,
                exactRecall = Math.Round(exact / (double)Math.Max(1, expected.Count), 4),
                exactOrPartialRecall = Math.Round((exact + partial) / (double)Math.Max(1, expected.Count), 4),
                facetExact = facetExact.ToDictionary(kv => kv.Key, kv => Math.Round(kv.Value / (double)Math.Max(1, expected.Count), 4))
            },
            matches
        };
        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[semantic-ledger] exact={exact}/{expected.Count} partial={partial}/{expected.Count} missed={missed}/{expected.Count}");
        Console.WriteLine($"[semantic-ledger] extracted -> {Path.GetRelativePath(repoRoot, extractedPath)}");
        Console.WriteLine($"[semantic-ledger] canonical -> {Path.GetRelativePath(repoRoot, canonicalPath)}");
        Console.WriteLine($"[semantic-ledger] matches -> {Path.GetRelativePath(repoRoot, resultPath)}");
        return 0;
    }
}
