using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter SourceClaim-Extraction-E2E-Spike:
/// Transcript -> auto SourceClaims -> Recall gegen Hand-Fixture -> Coverage fuer gematchte Claims.
/// Kein produktiver Axis-Umbau.
/// </summary>
public static class SourceClaimExtractionSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 5)
        {
            Console.Error.WriteLine("Usage: source-claim-extraction-spike <fixture.json> <transcript.txt> <phase> <runId> [judgeModelOverride] [wide]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var transcriptPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, "input", "transcripts", args[2]);
        var phase = args[3];
        var runId = args[4];
        string? judgeArg = null;
        var wideRecall = false;
        foreach (var a in args.Skip(5))
        {
            if (string.Equals(a, "wide", StringComparison.OrdinalIgnoreCase)) wideRecall = true;
            else judgeArg = a;
        }

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[source-claim-extract] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[source-claim-extract] Transkript fehlt: {transcriptPath}"); return 2; }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[source-claim-extract] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = envelope?.Cases ?? new List<SourceClaimCoverageCase>();
        if (cases.Count == 0) { Console.Error.WriteLine("[source-claim-extract] keine cases."); return 2; }

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var client = ChatClientFactory.Create(judgeSettings);
        var extractor = new SourceClaimExtractor(client, settings.JuryStructuredOutput);
        var matcher = new SourceClaimRecallMatcher(client, settings.JuryStructuredOutput);
        var coverage = new SourceClaimCoverageClassifier(client, settings.JuryStructuredOutput);

        var extractionMode = wideRecall ? "wide-recall" : "bounded-top";
        Console.WriteLine($"[source-claim-extract] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Cases={cases.Count} Transcript={Path.GetFileName(transcriptPath)} Run={phase}/{runId} Judge={judgeSettings.ModelId} Mode={extractionMode}");

        var artifactTypes = cases.Select(c => c.Artifact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var extractedByArtifact = new Dictionary<string, IReadOnlyList<ExtractedSourceClaim>>(StringComparer.OrdinalIgnoreCase);

        foreach (var artifact in artifactTypes)
        {
            var claims = await extractor.ExtractAsync(artifact, transcript, wideRecall, CancellationToken.None).ConfigureAwait(false);
            extractedByArtifact[artifact] = claims;
            Console.WriteLine($"[extract] {artifact}: {claims.Count} claims");
        }

        var artifactCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        async Task<string?> ArtifactText(string artifact)
        {
            if (artifactCache.TryGetValue(artifact, out var text)) return text;
            var path = Path.Combine(docsDir, artifact.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ? artifact : $"{artifact}.md");
            if (!File.Exists(path)) return null;
            text = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            artifactCache[artifact] = text;
            return text;
        }

        var results = new List<object>();
        int exact = 0, partial = 0, missed = 0, e2eMatches = 0;

        foreach (var c in cases)
        {
            var extracted = extractedByArtifact.GetValueOrDefault(c.Artifact) ?? Array.Empty<ExtractedSourceClaim>();
            var recall = await matcher.MatchAsync(c, extracted, CancellationToken.None).ConfigureAwait(false);
            if (recall.Verdict == "exact") exact++;
            else if (recall.Verdict == "partial") partial++;
            else missed++;

            SourceClaimCoverageVerdict? cov = null;
            ExtractedSourceClaim? matchedClaim = null;
            if (recall.Verdict is "exact" or "partial" && recall.MatchedIds.Count > 0)
            {
                matchedClaim = extracted.FirstOrDefault(e => string.Equals(e.Id, recall.MatchedIds[0], StringComparison.OrdinalIgnoreCase));
                var artifactText = await ArtifactText(c.Artifact).ConfigureAwait(false);
                if (matchedClaim is not null && artifactText is not null)
                {
                    var extractedCase = new SourceClaimCoverageCase(
                        c.Id,
                        c.Artifact,
                        matchedClaim.SourceClaim,
                        c.ExpectedVerdict,
                        matchedClaim.RequiredTreatment,
                        matchedClaim.Evidence,
                        c.TopicCoverageBaseline,
                        c.DirectReviewBaseline,
                        c.Note);
                    cov = await coverage.ClassifyAsync(extractedCase, artifactText, CancellationToken.None).ConfigureAwait(false);
                    if (cov.MatchesExpected) e2eMatches++;
                }
            }

            Console.WriteLine($"[{recall.Verdict.ToUpperInvariant()}] {c.Id} ({c.Artifact})" +
                              (cov is not null ? $" e2e={cov.Verdict}/{cov.Support} match={cov.MatchesExpected}" : ""));

            results.Add(new
            {
                c.Id,
                c.Artifact,
                c.SourceClaim,
                c.ExpectedVerdict,
                c.RequiredTreatment,
                recall,
                matchedClaim,
                coverageVerdict = cov,
                c.TopicCoverageBaseline,
                c.DirectReviewBaseline,
                c.Note
            });
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            transcript = Path.GetFileName(transcriptPath),
            phase,
            runId,
            judge = judgeSettings.ModelId,
            mode = "source-claim-extraction-spike",
            extractionMode,
            total = cases.Count,
            extractedByArtifact,
            recall = new
            {
                exact,
                partial,
                missed,
                exactRate = Math.Round(exact / (double)cases.Count, 4),
                exactOrPartial = exact + partial,
                exactOrPartialRate = Math.Round((exact + partial) / (double)cases.Count, 4)
            },
            e2e = new
            {
                matchesExpected = e2eMatches,
                matchRateAllCases = Math.Round(e2eMatches / (double)cases.Count, 4)
            },
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"source-claim-extraction-spike.{extractionMode}.{Path.GetFileNameWithoutExtension(transcriptPath)}.{phase}_{runId}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-claim-extract] recall exact={exact}/{cases.Count} exact+partial={exact + partial}/{cases.Count} e2e={e2eMatches}/{cases.Count} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
}
