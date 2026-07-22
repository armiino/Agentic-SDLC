using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>V2 Matrix-Spike gegen adjudizierte 80-Zellen-Reference-Matrix.</summary>
public static class SourceClaimCoverageMatrixV2Runner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private static readonly string[] ArtifactTypes =
    [
        "architecture",
        "requirements",
        "risks",
        "open-questions"
    ];

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 5)
        {
            Console.Error.WriteLine("Usage: source-claim-coverage-matrix-v2 <fixture.json> <adjudication.json> <phase> <runId> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var adjudicationPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var phase = args[3];
        var runId = args[4];
        var judgeArg = args.Length >= 6 ? args[5] : null;

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[matrix-v2] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(adjudicationPath)) { Console.Error.WriteLine($"[matrix-v2] Adjudication fehlt: {adjudicationPath}"); return 2; }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir)) { Console.Error.WriteLine($"[matrix-v2] Snapshots nicht gefunden: {docsDir}"); return 2; }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var fixture = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions)?.Cases ?? [];
        var goldCells = JsonSerializer.Deserialize<AdjudicationEnvelope>(
            await File.ReadAllTextAsync(adjudicationPath).ConfigureAwait(false), JsonOptions)?.Cells ?? [];
        var goldByKey = goldCells.ToDictionary(c => $"{c.Id}|{c.Artifact}", StringComparer.OrdinalIgnoreCase);
        if (fixture.Count == 0 || goldByKey.Count == 0) { Console.Error.WriteLine("[matrix-v2] leere Eingaben."); return 2; }

        var judge = new SourceClaimCoverageMatrixJudgeV2(
            ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        var artifactCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        async Task<string?> ArtifactText(string artifact)
        {
            if (artifactCache.TryGetValue(artifact, out var text)) return text;
            var path = Path.Combine(docsDir, $"{artifact}.md");
            if (!File.Exists(path)) return null;
            text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            artifactCache[artifact] = text;
            return text;
        }

        Console.WriteLine($"[matrix-v2] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Cells={fixture.Count * ArtifactTypes.Length} Judge={judgeSettings.ModelId}");

        var results = new List<object>();
        var appOk = 0;
        var covOk = 0;
        var bothOk = 0;
        var total = 0;
        var coverageApplicableOk = 0;
        var coverageApplicableTotal = 0;
        var partialPred = 0;
        var partialGold = 0;
        var partialGoldPredPartial = 0;
        var confusion = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in fixture)
        {
            foreach (var artifact in ArtifactTypes)
            {
                var artifactText = await ArtifactText(artifact).ConfigureAwait(false);
                if (artifactText is null) continue;
                var key = $"{c.Id}|{artifact}";
                if (!goldByKey.TryGetValue(key, out var gold)) continue;

                var matrixCase = c with
                {
                    Artifact = artifact,
                    ExpectedVerdict = gold.Coverage,
                    RequiredTreatment = $"Bewerte diesen SourceClaim fuer {artifact}.md. Trenne Applicability und Coverage. Gold-Hinweis ist nicht gegeben; entscheide aus SourceClaim, Evidence und Artefakt."
                };

                var verdict = await judge.ClassifyAsync(matrixCase, artifactText, CancellationToken.None).ConfigureAwait(false);
                total++;
                var appMatch = verdict.Applicability == gold.Applicability;
                var covMatch = verdict.Coverage == gold.Coverage;
                if (appMatch) appOk++;
                if (covMatch) covOk++;
                if (appMatch && covMatch) bothOk++;
                if (gold.Applicability is "required" or "optional")
                {
                    coverageApplicableTotal++;
                    if (covMatch) coverageApplicableOk++;
                }
                if (verdict.Coverage == "partial") partialPred++;
                if (gold.Coverage == "partial")
                {
                    partialGold++;
                    if (verdict.Coverage == "partial") partialGoldPredPartial++;
                }
                confusion[$"{gold.Coverage}->{verdict.Coverage}"] = confusion.GetValueOrDefault($"{gold.Coverage}->{verdict.Coverage}") + 1;

                Console.WriteLine($"[{(appMatch && covMatch ? "OK" : "MISS")}] {c.Id}->{artifact} app {gold.Applicability}/{verdict.Applicability} cov {gold.Coverage}/{verdict.Coverage}");

                results.Add(new
                {
                    c.Id,
                    artifact,
                    c.SourceClaim,
                    gold,
                    verdict,
                    appMatch,
                    covMatch
                });
            }
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            adjudication = Path.GetRelativePath(repoRoot, adjudicationPath),
            phase,
            runId,
            judge = judgeSettings.ModelId,
            mode = "source-claim-coverage-matrix-v2",
            total,
            metrics = new
            {
                applicabilityAccuracy = Math.Round(appOk / (double)Math.Max(1, total), 4),
                coverageAccuracyAll = Math.Round(covOk / (double)Math.Max(1, total), 4),
                bothAccuracy = Math.Round(bothOk / (double)Math.Max(1, total), 4),
                coverageAccuracyApplicable = Math.Round(coverageApplicableOk / (double)Math.Max(1, coverageApplicableTotal), 4),
                partialPredicted = partialPred,
                partialGold,
                partialRecall = Math.Round(partialGoldPredPartial / (double)Math.Max(1, partialGold), 4),
                partialOveruse = partialPred - partialGoldPredPartial
            },
            confusion,
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir, $"source-claim-coverage-matrix-v2.{phase}_{runId}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[matrix-v2] app={appOk}/{total} cov={covOk}/{total} both={bothOk}/{total} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
    private sealed record AdjudicationEnvelope([property: JsonPropertyName("cells")] List<GoldCell> Cells);
    private sealed record GoldCell(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("artifact")] string Artifact,
        [property: JsonPropertyName("applicability")] string Applicability,
        [property: JsonPropertyName("coverage")] string Coverage,
        [property: JsonPropertyName("note")] string Note);
}
