using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Batch-Matrix-Spike: alle Fixture-SourceClaims gegen genau ein Artefakt in einem LLM-Call.</summary>
public static class SourceClaimCoverageMatrixBatchRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 6)
        {
            Console.Error.WriteLine("Usage: source-claim-coverage-matrix-batch <fixture.json> <adjudication.json> <phase> <runId> <artifact> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var adjudicationPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var phase = args[3];
        var runId = args[4];
        var artifact = args[5];
        var judgeArg = args.Length >= 7 ? args[6] : null;

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[matrix-batch] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(adjudicationPath)) { Console.Error.WriteLine($"[matrix-batch] Adjudication fehlt: {adjudicationPath}"); return 2; }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        var artifactPath = Path.Combine(docsDir, $"{artifact}.md");
        if (!File.Exists(artifactPath)) { Console.Error.WriteLine($"[matrix-batch] Artefakt fehlt: {artifactPath}"); return 2; }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var fixture = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions)?.Cases ?? [];
        var goldCells = JsonSerializer.Deserialize<AdjudicationEnvelope>(
            await File.ReadAllTextAsync(adjudicationPath).ConfigureAwait(false), JsonOptions)?.Cells ?? [];
        var goldById = goldCells
            .Where(c => string.Equals(c.Artifact, artifact, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(c => c.Id, StringComparer.OrdinalIgnoreCase);
        if (fixture.Count == 0 || goldById.Count == 0) { Console.Error.WriteLine("[matrix-batch] leere Eingaben."); return 2; }

        var artifactText = await File.ReadAllTextAsync(artifactPath).ConfigureAwait(false);
        var judge = new SourceClaimCoverageMatrixBatchJudge(
            ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

        Console.WriteLine($"[matrix-batch] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Artifact={artifact} Claims={fixture.Count} Judge={judgeSettings.ModelId}");

        var verdicts = await judge.ClassifyAsync(fixture, artifact, artifactText, CancellationToken.None).ConfigureAwait(false);
        var verdictById = verdicts.ToDictionary(v => v.Id, StringComparer.OrdinalIgnoreCase);

        var results = new List<object>();
        var appOk = 0;
        var covOk = 0;
        var bothOk = 0;
        var total = 0;
        var confusion = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in fixture)
        {
            total++;
            var gold = goldById[c.Id];
            var verdict = verdictById.TryGetValue(c.Id, out var found)
                ? found
                : new SourceClaimMatrixBatchItem(c.Id, "unclear", "unclear", "none", "", [], [], "missing batch item");

            var appMatch = verdict.Applicability == gold.Applicability;
            var covMatch = verdict.Coverage == gold.Coverage;
            if (appMatch) appOk++;
            if (covMatch) covOk++;
            if (appMatch && covMatch) bothOk++;
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

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            adjudication = Path.GetRelativePath(repoRoot, adjudicationPath),
            phase,
            runId,
            artifact,
            judge = judgeSettings.ModelId,
            mode = "source-claim-coverage-matrix-batch",
            total,
            metrics = new
            {
                applicabilityAccuracy = Math.Round(appOk / (double)Math.Max(1, total), 4),
                coverageAccuracyAll = Math.Round(covOk / (double)Math.Max(1, total), 4),
                bothAccuracy = Math.Round(bothOk / (double)Math.Max(1, total), 4)
            },
            confusion,
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir, $"source-claim-coverage-matrix-batch.{artifact}.{phase}_{runId}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[matrix-batch] app={appOk}/{total} cov={covOk}/{total} both={bothOk}/{total} -> {Path.GetRelativePath(repoRoot, outFile)}");
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
