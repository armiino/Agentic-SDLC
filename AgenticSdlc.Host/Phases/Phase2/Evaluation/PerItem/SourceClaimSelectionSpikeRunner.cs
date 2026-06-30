using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Spike: Global SourceClaim Ledger -> konservative Selection -> Recall/Kandidatenreduktion.
/// </summary>
public static class SourceClaimSelectionSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: source-claim-selection-spike <ledger-spike.json> <fixture.json> [judgeModelOverride]");
            return 2;
        }

        var ledgerPath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var fixturePath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var judgeArg = args.Length >= 4 ? args[3] : null;

        if (!File.Exists(ledgerPath)) { Console.Error.WriteLine($"[source-claim-selection] Ledger fehlt: {ledgerPath}"); return 2; }
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[source-claim-selection] Fixture fehlt: {fixturePath}"); return 2; }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var ledgerEnvelope = JsonSerializer.Deserialize<LedgerEnvelope>(
            await File.ReadAllTextAsync(ledgerPath).ConfigureAwait(false), JsonOptions);
        var ledger = ledgerEnvelope?.Ledger ?? [];
        if (ledger.Count == 0) { Console.Error.WriteLine("[source-claim-selection] Ledger leer."); return 2; }

        var fixtureEnvelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = fixtureEnvelope?.Cases ?? [];
        if (cases.Count == 0) { Console.Error.WriteLine("[source-claim-selection] keine cases."); return 2; }

        var client = ChatClientFactory.Create(judgeSettings);
        var selector = new SourceClaimSelectionClassifier(client, settings.JuryStructuredOutput);
        var matcher = new SourceClaimRecallMatcher(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[source-claim-selection] Ledger={Path.GetFileName(ledgerPath)} Claims={ledger.Count} Cases={cases.Count} Judge={judgeSettings.ModelId}");

        var artifactTypes = cases.Select(c => c.Artifact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var selectionsByArtifact = new Dictionary<string, IReadOnlyList<SourceClaimSelection>>(StringComparer.OrdinalIgnoreCase);

        foreach (var artifact in artifactTypes)
        {
            var selections = await selector.ClassifyAsync(artifact, ledger, CancellationToken.None).ConfigureAwait(false);
            selectionsByArtifact[artifact] = selections;
            var grouped = selections
                .GroupBy(s => s.Selection)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}={g.Count()}");
            Console.WriteLine($"[selection] {artifact}: {string.Join(" ", grouped)}");
        }

        int mustExact = 0, mustPartial = 0, mustMissed = 0;
        int shouldExact = 0, shouldPartial = 0, shouldMissed = 0;
        var results = new List<object>();

        foreach (var c in cases)
        {
            var selections = selectionsByArtifact.GetValueOrDefault(c.Artifact) ?? [];
            var mustIds = selections
                .Where(s => s.Selection == "must_check")
                .Select(s => s.SourceClaimId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var mustShouldIds = selections
                .Where(s => s.Selection is "must_check" or "should_check")
                .Select(s => s.SourceClaimId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var mustCandidates = ledger.Where(l => mustIds.Contains(l.Id)).Select(l => ToExtracted(l, c.Artifact)).ToList();
            var mustShouldCandidates = ledger.Where(l => mustShouldIds.Contains(l.Id)).Select(l => ToExtracted(l, c.Artifact)).ToList();

            var mustRecall = await matcher.MatchAsync(c, mustCandidates, CancellationToken.None).ConfigureAwait(false);
            Count(mustRecall.Verdict, ref mustExact, ref mustPartial, ref mustMissed);

            var mustShouldRecall = await matcher.MatchAsync(c, mustShouldCandidates, CancellationToken.None).ConfigureAwait(false);
            Count(mustShouldRecall.Verdict, ref shouldExact, ref shouldPartial, ref shouldMissed);

            Console.WriteLine($"[{c.Id}] {c.Artifact} must={mustCandidates.Count} must+should={mustShouldCandidates.Count} mustRecall={mustRecall.Verdict} mustShouldRecall={mustShouldRecall.Verdict}");

            results.Add(new
            {
                c.Id,
                c.Artifact,
                c.SourceClaim,
                mustCount = mustCandidates.Count,
                mustShouldCount = mustShouldCandidates.Count,
                mustRecall,
                mustShouldRecall,
                c.Note
            });
        }

        var candidateSummary = artifactTypes.ToDictionary(
            a => a,
            a =>
            {
                var oldRelevant = ledger.Count(l => l.RelevantFor.Contains(a, StringComparer.OrdinalIgnoreCase));
                var selections = selectionsByArtifact.GetValueOrDefault(a) ?? [];
                return new
                {
                    oldRelevantFor = oldRelevant,
                    must = selections.Count(s => s.Selection == "must_check"),
                    mustShould = selections.Count(s => s.Selection is "must_check" or "should_check"),
                    contextOnly = selections.Count(s => s.Selection == "context_only"),
                    skip = selections.Count(s => s.Selection == "skip")
                };
            },
            StringComparer.OrdinalIgnoreCase);

        var payload = new
        {
            ledger = Path.GetRelativePath(repoRoot, ledgerPath),
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            judge = judgeSettings.ModelId,
            mode = "source-claim-selection-spike",
            total = cases.Count,
            sourceClaims = ledger.Count,
            candidateSummary,
            selectionsByArtifact,
            mustRecall = Summary(mustExact, mustPartial, mustMissed, cases.Count),
            mustShouldRecall = Summary(shouldExact, shouldPartial, shouldMissed, cases.Count),
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"source-claim-selection-spike.{Path.GetFileNameWithoutExtension(ledgerPath)}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-claim-selection] must exact={mustExact}/{cases.Count} exact+partial={mustExact + mustPartial}/{cases.Count}; must+should exact={shouldExact}/{cases.Count} exact+partial={shouldExact + shouldPartial}/{cases.Count} -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private static ExtractedSourceClaim ToExtracted(GlobalSourceClaim c, string artifact)
        => new(c.Id, artifact, c.SourceClaim, c.RequiredTreatment, c.Evidence, c.Kind, c.Priority);

    private static void Count(string verdict, ref int exact, ref int partial, ref int missed)
    {
        if (verdict == "exact") exact++;
        else if (verdict == "partial") partial++;
        else missed++;
    }

    private static object Summary(int exact, int partial, int missed, int total) => new
    {
        exact,
        partial,
        missed,
        exactRate = Math.Round(exact / (double)total, 4),
        exactOrPartial = exact + partial,
        exactOrPartialRate = Math.Round((exact + partial) / (double)total, 4)
    };

    private sealed record LedgerEnvelope([property: JsonPropertyName("ledger")] List<GlobalSourceClaim> Ledger);
    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<SourceClaimCoverageCase> Cases);
}
