using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Kleiner Spike fuer evidence-native Grounding: prueft explizite Claim-Evidence-Paare statt Markdown-Units
/// gegen das gesamte Transkript. Additiv, isoliert, nicht Teil der produktiven GroundingAxis.
/// </summary>
public static class ClaimGroundingSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: claim-grounding-spike <fixture.json> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(fixturePath))
        {
            Console.Error.WriteLine($"[claim-grounding-spike] Fixture nicht gefunden: {fixturePath}");
            return 2;
        }

        var judgeArg = args.Length >= 3 ? args[2] : null;
        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        var fixture = await LoadFixtureAsync(fixturePath).ConfigureAwait(false);
        if (fixture.Cases.Count == 0)
        {
            Console.Error.WriteLine("[claim-grounding-spike] Fixture enthaelt keine cases.");
            return 2;
        }

        var verifier = new ClaimEvidenceVerifier(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
        var results = new List<object>();
        var matches = 0;

        Console.WriteLine($"[claim-grounding-spike] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)}");
        Console.WriteLine($"[claim-grounding-spike] Judge: {judgeSettings.LlmProvider} / {judgeSettings.ModelId}");

        foreach (var testCase in fixture.Cases)
        {
            var verdict = await verifier.VerifyAsync(testCase, CancellationToken.None).ConfigureAwait(false);
            if (verdict.MatchesExpected) matches++;

            results.Add(new
            {
                testCase.Id,
                testCase.Artifact,
                testCase.Claim,
                testCase.ExpectedLabel,
                testCase.OriginHandLabel,
                testCase.Note,
                verdict = new
                {
                    verdict.Verdict,
                    verdict.Label,
                    verdict.MatchesExpected,
                    verdict.Support,
                    verdict.ModalityPreserved,
                    verdict.StatusPreserved,
                    verdict.ScopePreserved,
                    verdict.TemporalContextPreserved,
                    verdict.Reason
                },
                evidence = testCase.Evidence
            });

            var mark = verdict.MatchesExpected ? "OK" : "MISS";
            Console.WriteLine(
                $"[{mark}] {testCase.Id}: expected={testCase.ExpectedLabel} got={verdict.Label}/{verdict.Verdict} " +
                $"support={verdict.Support} reason={Truncate(verdict.Reason, 100)}");
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            judge = new { provider = judgeSettings.LlmProvider, model = judgeSettings.ModelId },
            total = fixture.Cases.Count,
            matches,
            accuracy = fixture.Cases.Count == 0 ? 0 : Math.Round(matches / (double)fixture.Cases.Count, 4),
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "claim-grounding-spike");
        Directory.CreateDirectory(outDir);
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(fixturePath)}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[claim-grounding-spike] {matches}/{fixture.Cases.Count} match -> {Path.GetRelativePath(repoRoot, outFile)}");
        return matches == fixture.Cases.Count ? 0 : 1;
    }

    private static async Task<FixtureEnvelope> LoadFixtureAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var fixture = await JsonSerializer.DeserializeAsync<FixtureEnvelope>(stream, JsonOptions).ConfigureAwait(false);
        return fixture ?? new FixtureEnvelope([]);
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "...";

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<ClaimEvidenceCase> Cases);
}
