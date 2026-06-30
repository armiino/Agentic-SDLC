using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Kleiner Coverage-v2-Spike:
/// Transcript einmal -> artefaktspezifische SourceObligations -> Recall gegen 20 Hand-Cases.
/// </summary>
public static class SourceObligationExtractionSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: source-obligation-extraction-spike <fixture.json> <transcript.txt> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var transcriptPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, "input", "transcripts", args[2]);
        var judgeArg = args.Length >= 4 ? args[3] : null;

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[source-obligation-extract] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[source-obligation-extract] Transkript fehlt: {transcriptPath}"); return 2; }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = envelope?.Cases ?? [];
        if (cases.Count == 0) { Console.Error.WriteLine("[source-obligation-extract] keine cases."); return 2; }

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var client = ChatClientFactory.Create(judgeSettings);
        var extractor = new SourceObligationExtractor(client, settings.JuryStructuredOutput);
        var matcher = new SourceClaimRecallMatcher(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[source-obligation-extract] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Cases={cases.Count} Transcript={Path.GetFileName(transcriptPath)} Judge={judgeSettings.ModelId}");

        var obligations = await extractor.ExtractAsync(transcript, CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"[source-obligation-extract] obligations: {obligations.Count}");

        var artifactTypes = cases.Select(c => c.Artifact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var candidateSummary = artifactTypes.ToDictionary(
            a => a,
            a => obligations.Count(o => string.Equals(o.Artifact, a, StringComparison.OrdinalIgnoreCase)),
            StringComparer.OrdinalIgnoreCase);
        foreach (var kv in candidateSummary.OrderBy(kv => kv.Key))
            Console.WriteLine($"[source-obligation-extract] {kv.Key}: {kv.Value}");

        var results = new List<object>();
        int exact = 0, partial = 0, missed = 0;

        foreach (var c in cases)
        {
            var candidates = obligations
                .Where(o => string.Equals(o.Artifact, c.Artifact, StringComparison.OrdinalIgnoreCase))
                .Select(ToExtracted)
                .ToList();

            var recall = await matcher.MatchAsync(c, candidates, CancellationToken.None).ConfigureAwait(false);
            if (recall.Verdict == "exact") exact++;
            else if (recall.Verdict == "partial") partial++;
            else missed++;

            Console.WriteLine($"[{recall.Verdict.ToUpperInvariant()}] {c.Id} ({c.Artifact}) candidates={candidates.Count}");

            results.Add(new
            {
                c.Id,
                c.Artifact,
                c.SourceClaim,
                c.RequiredTreatment,
                candidateCount = candidates.Count,
                recall,
                matchedObligation = recall.MatchedIds.Count > 0
                    ? obligations.FirstOrDefault(o => string.Equals(o.Id, recall.MatchedIds[0], StringComparison.OrdinalIgnoreCase))
                    : null,
                c.Note
            });
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            transcript = Path.GetFileName(transcriptPath),
            judge = judgeSettings.ModelId,
            mode = "source-obligation-extraction-spike",
            total = cases.Count,
            obligations = obligations.Count,
            candidateSummary,
            sourceObligations = obligations,
            recall = new
            {
                exact,
                partial,
                missed,
                exactRate = Math.Round(exact / (double)cases.Count, 4),
                exactOrPartial = exact + partial,
                exactOrPartialRate = Math.Round((exact + partial) / (double)cases.Count, 4)
            },
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"source-obligation-extraction-spike.{Path.GetFileNameWithoutExtension(transcriptPath)}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-obligation-extract] recall exact={exact}/{cases.Count} exact+partial={exact + partial}/{cases.Count} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private static ExtractedSourceClaim ToExtracted(SourceObligation o)
        => new(o.Id, o.Artifact, o.SourceClaim, o.RequiredTreatment, o.Evidence, o.Kind, o.Importance);

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
}
