using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Matrix-Spike: kuratierte SourceClaims gegen alle Zielartefakte pruefen.
/// Ziel: not_applicable/partial/missing/contradicted sichtbar machen statt vorher ueber relevantFor zu gaten.
/// </summary>
public static class SourceClaimCoverageMatrixRunner
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
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: source-claim-coverage-matrix <fixture.json> <phase> <runId> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var phase = args[2];
        var runId = args[3];
        var judgeArg = args.Length >= 5 ? args[4] : null;

        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[source-claim-matrix] Fixture fehlt: {fixturePath}");
            return 2;
        }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[source-claim-matrix] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = envelope?.Cases ?? [];
        if (cases.Count == 0)
        {
            Console.Error.WriteLine("[source-claim-matrix] keine cases.");
            return 2;
        }

        var classifier = new SourceClaimCoverageClassifier(
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

        Console.WriteLine($"[source-claim-matrix] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Cases={cases.Count} Matrix={cases.Count * ArtifactTypes.Length} Run={phase}/{runId} Judge={judgeSettings.ModelId}");

        var results = new List<object>();
        var targetMatches = 0;
        var targetTotal = 0;
        var nonTargetTotal = 0;
        var nonTargetNotApplicable = 0;
        var verdictCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in cases)
        {
            foreach (var artifact in ArtifactTypes)
            {
                var artifactText = await ArtifactText(artifact).ConfigureAwait(false);
                if (artifactText is null)
                {
                    Console.WriteLine($"[MISS-FILE] {c.Id}: Artefakt fehlt: {artifact}");
                    continue;
                }

                var isTarget = string.Equals(c.Artifact, artifact, StringComparison.OrdinalIgnoreCase);
                var matrixCase = isTarget
                    ? c
                    : c with
                    {
                        Artifact = artifact,
                        ExpectedVerdict = "not_applicable",
                        RequiredTreatment = $"Pruefe, ob dieser SourceClaim in {artifact}.md als Coverage-Pflicht behandelt werden muss. Wenn der Claim fuer dieses Artefakt nicht sinnvoll verpflichtend ist, waehle not_applicable."
                    };

                var verdict = await classifier.ClassifyAsync(matrixCase, artifactText, CancellationToken.None).ConfigureAwait(false);
                verdictCounts[verdict.Verdict] = verdictCounts.GetValueOrDefault(verdict.Verdict) + 1;

                if (isTarget)
                {
                    targetTotal++;
                    if (verdict.MatchesExpected) targetMatches++;
                }
                else
                {
                    nonTargetTotal++;
                    if (verdict.Verdict == "not_applicable") nonTargetNotApplicable++;
                }

                Console.WriteLine($"[{(isTarget ? "TARGET" : "X")}] {c.Id} -> {artifact} got={verdict.Verdict}/{verdict.Support}" +
                                  (isTarget ? $" exp={c.ExpectedVerdict} match={verdict.MatchesExpected}" : ""));

                results.Add(new
                {
                    c.Id,
                    sourceArtifact = c.Artifact,
                    targetArtifact = artifact,
                    isFixtureTarget = isTarget,
                    c.SourceClaim,
                    expectedVerdict = isTarget ? c.ExpectedVerdict : null,
                    c.Evidence,
                    verdict,
                    c.TopicCoverageBaseline,
                    c.DirectReviewBaseline,
                    c.Note
                });
            }
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            phase,
            runId,
            judge = judgeSettings.ModelId,
            mode = "source-claim-coverage-matrix",
            sourceClaims = cases.Count,
            artifacts = ArtifactTypes,
            totalCells = cases.Count * ArtifactTypes.Length,
            target = new
            {
                total = targetTotal,
                matchesExpected = targetMatches,
                accuracyVsFixture = Math.Round(targetMatches / (double)Math.Max(1, targetTotal), 4)
            },
            nonTarget = new
            {
                total = nonTargetTotal,
                notApplicable = nonTargetNotApplicable,
                notApplicableRate = Math.Round(nonTargetNotApplicable / (double)Math.Max(1, nonTargetTotal), 4)
            },
            verdictCounts,
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"source-claim-coverage-matrix.{phase}_{runId}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-claim-matrix] target={targetMatches}/{targetTotal}; nonTarget not_applicable={nonTargetNotApplicable}/{nonTargetTotal} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
}
