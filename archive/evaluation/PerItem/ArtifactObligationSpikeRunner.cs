using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Isolierter Spike: Global SourceClaim Ledger -> ArtifactObligation -> Recall gegen 20 Hand-Cases.
/// Testet, ob required/optional/context_only das breite relevantFor sinnvoll reduziert.
/// </summary>
public static class ArtifactObligationSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: source-claim-obligation-spike <ledger-spike.json> <fixture.json> [judgeModelOverride]");
            return 2;
        }

        var ledgerPath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var fixturePath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var judgeArg = args.Length >= 4 ? args[3] : null;

        if (!File.Exists(ledgerPath)) { Console.Error.WriteLine($"[source-claim-obligation] Ledger fehlt: {ledgerPath}"); return 2; }
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[source-claim-obligation] Fixture fehlt: {fixturePath}"); return 2; }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var ledgerEnvelope = JsonSerializer.Deserialize<LedgerEnvelope>(
            await File.ReadAllTextAsync(ledgerPath).ConfigureAwait(false), JsonOptions);
        var ledger = ledgerEnvelope?.Ledger ?? [];
        if (ledger.Count == 0) { Console.Error.WriteLine("[source-claim-obligation] Ledger leer."); return 2; }

        var fixtureEnvelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = fixtureEnvelope?.Cases ?? [];
        if (cases.Count == 0) { Console.Error.WriteLine("[source-claim-obligation] keine cases."); return 2; }

        var client = ChatClientFactory.Create(judgeSettings);
        var classifier = new ArtifactObligationClassifier(client, settings.JuryStructuredOutput);
        var matcher = new SourceClaimRecallMatcher(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[source-claim-obligation] Ledger={Path.GetFileName(ledgerPath)} Claims={ledger.Count} Cases={cases.Count} Judge={judgeSettings.ModelId}");

        var artifactTypes = cases.Select(c => c.Artifact).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var obligationsByArtifact = new Dictionary<string, IReadOnlyList<ArtifactObligation>>(StringComparer.OrdinalIgnoreCase);

        foreach (var artifact in artifactTypes)
        {
            var obligations = await classifier.ClassifyAsync(artifact, ledger, CancellationToken.None).ConfigureAwait(false);
            obligationsByArtifact[artifact] = obligations;
            var grouped = obligations
                .GroupBy(o => o.Necessity)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}={g.Count()}");
            Console.WriteLine($"[obligation] {artifact}: {string.Join(" ", grouped)}");
        }

        var results = new List<object>();
        int relevantExact = 0, relevantPartial = 0, relevantMissed = 0;
        int requiredExact = 0, requiredPartial = 0, requiredMissed = 0;

        foreach (var c in cases)
        {
            var relevantCandidates = ledger
                .Where(l => l.RelevantFor.Contains(c.Artifact, StringComparer.OrdinalIgnoreCase))
                .Select(l => ToExtracted(l, c.Artifact))
                .ToList();

            var artifactObligations = obligationsByArtifact.GetValueOrDefault(c.Artifact) ?? [];
            var actionableIds = artifactObligations
                .Where(o => o.Necessity is "required" or "optional" or "unclear")
                .Select(o => o.SourceClaimId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var requiredIds = artifactObligations
                .Where(o => o.Necessity == "required")
                .Select(o => o.SourceClaimId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var actionableCandidates = ledger
                .Where(l => actionableIds.Contains(l.Id))
                .Select(l => ToExtracted(l, c.Artifact))
                .ToList();
            var requiredCandidates = ledger
                .Where(l => requiredIds.Contains(l.Id))
                .Select(l => ToExtracted(l, c.Artifact))
                .ToList();

            var actionableRecall = await matcher.MatchAsync(c, actionableCandidates, CancellationToken.None).ConfigureAwait(false);
            Count(actionableRecall.Verdict, ref relevantExact, ref relevantPartial, ref relevantMissed);

            var requiredRecall = await matcher.MatchAsync(c, requiredCandidates, CancellationToken.None).ConfigureAwait(false);
            Count(requiredRecall.Verdict, ref requiredExact, ref requiredPartial, ref requiredMissed);

            Console.WriteLine($"[{c.Id}] {c.Artifact} relevantFor={relevantCandidates.Count} actionable={actionableCandidates.Count} required={requiredCandidates.Count} actionable={actionableRecall.Verdict} requiredOnly={requiredRecall.Verdict}");

            results.Add(new
            {
                c.Id,
                c.Artifact,
                c.SourceClaim,
                relevantForCount = relevantCandidates.Count,
                actionableCount = actionableCandidates.Count,
                requiredCount = requiredCandidates.Count,
                actionableRecall,
                requiredRecall,
                c.Note
            });
        }

        var candidateSummary = artifactTypes.ToDictionary(
            a => a,
            a =>
            {
                var relevant = ledger.Count(l => l.RelevantFor.Contains(a, StringComparer.OrdinalIgnoreCase));
                var obs = obligationsByArtifact.GetValueOrDefault(a) ?? [];
                return new
                {
                    relevantFor = relevant,
                    required = obs.Count(o => o.Necessity == "required"),
                    actionable = obs.Count(o => o.Necessity is "required" or "optional" or "unclear"),
                    contextOnly = obs.Count(o => o.Necessity == "context_only"),
                    notApplicable = obs.Count(o => o.Necessity == "not_applicable")
                };
            },
            StringComparer.OrdinalIgnoreCase);

        var payload = new
        {
            ledger = Path.GetRelativePath(repoRoot, ledgerPath),
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            judge = judgeSettings.ModelId,
            mode = "source-claim-obligation-spike",
            total = cases.Count,
            sourceClaims = ledger.Count,
            candidateSummary,
            obligationsByArtifact,
            actionableRecall = Summary(relevantExact, relevantPartial, relevantMissed, cases.Count),
            requiredOnlyRecall = Summary(requiredExact, requiredPartial, requiredMissed, cases.Count),
            results
        };

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "source-claim-coverage-spike");
        Directory.CreateDirectory(outDir);
        var outFile = Path.Combine(outDir,
            $"source-claim-obligation-spike.{Path.GetFileNameWithoutExtension(ledgerPath)}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[source-claim-obligation] actionable exact={relevantExact}/{cases.Count} exact+partial={relevantExact + relevantPartial}/{cases.Count}; required exact={requiredExact}/{cases.Count} exact+partial={requiredExact + requiredPartial}/{cases.Count} -> {Path.GetRelativePath(repoRoot, outFile)}");
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
