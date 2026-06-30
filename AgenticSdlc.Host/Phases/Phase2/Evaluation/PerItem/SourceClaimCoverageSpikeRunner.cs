using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter SourceClaim-Coverage-Spike. Kein produktiver ReviewAxis-Umbau.
/// Aufruf:
/// <c>source-claim-coverage-spike &lt;fixture.json&gt; &lt;phase&gt; &lt;runId&gt; [judge]</c>
/// </summary>
public static class SourceClaimCoverageSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: source-claim-coverage-spike <fixture.json> <phase> <runId> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[source-claim-coverage] Fixture fehlt: {fixturePath}");
            return 2;
        }

        var phase = args[2];
        var runId = args[3];
        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[source-claim-coverage] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        var judgeArg = args.Length >= 5 ? args[4] : null;
        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = envelope?.Cases ?? new List<SourceClaimCoverageCase>();
        if (cases.Count == 0)
        {
            Console.Error.WriteLine("[source-claim-coverage] keine cases.");
            return 2;
        }

        var classifier = new SourceClaimCoverageClassifier(
            ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);

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

        Console.WriteLine($"[source-claim-coverage] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)}  Cases={cases.Count}  Run={phase}/{runId}  Judge={judgeSettings.ModelId}");

        var results = new List<object>();
        var matches = 0;
        foreach (var c in cases)
        {
            var artifactText = await ArtifactText(c.Artifact).ConfigureAwait(false);
            if (artifactText is null)
            {
                Console.WriteLine($"[MISS-FILE] {c.Id}: Artefakt fehlt: {c.Artifact}");
                results.Add(new
                {
                    c.Id,
                    c.Artifact,
                    c.SourceClaim,
                    c.ExpectedVerdict,
                    verdict = new SourceClaimCoverageVerdict(c.Id, "unclassified", "none", false, "", "artifact missing", "Artefakt fehlt"),
                    c.TopicCoverageBaseline,
                    c.DirectReviewBaseline,
                    c.Note
                });
                continue;
            }

            var verdict = await classifier.ClassifyAsync(c, artifactText, CancellationToken.None).ConfigureAwait(false);
            if (verdict.MatchesExpected) matches++;
            Console.WriteLine($"[{(verdict.MatchesExpected ? "OK" : "MISS")}] {c.Id} ({c.Artifact}) exp={c.ExpectedVerdict} got={verdict.Verdict}/{verdict.Support}");

            results.Add(new
            {
                c.Id,
                c.Artifact,
                c.SourceClaim,
                c.ExpectedVerdict,
                c.RequiredTreatment,
                c.Evidence,
                verdict,
                c.TopicCoverageBaseline,
                c.DirectReviewBaseline,
                c.Note
            });
        }

        var byArtifact = results
            .GroupBy(r => (string)r.GetType().GetProperty("Artifact")!.GetValue(r)!)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            phase,
            runId,
            judge = judgeSettings.ModelId,
            mode = "source-claim-coverage-spike",
            total = cases.Count,
            matchesExpected = matches,
            accuracyVsFixture = Math.Round(matches / (double)cases.Count, 4),
            casesByArtifact = byArtifact,
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"{Path.GetFileNameWithoutExtension(fixturePath)}.{phase}_{runId}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-claim-coverage] vs Fixture: {matches}/{cases.Count} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
}
