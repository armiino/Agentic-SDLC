using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Claim-Pilot Stufe 3 — ClaimSplitter-Spike (ImplementClaimAnsatz §10). Zerlegt Markdown-Units in atomare
/// Claims. MIT Transkript-Argument läuft die VOLLE Kette <c>Unit → ClaimSplitter → EvidenceSelector → Verifier</c>
/// pro atomarem Claim → zeigt, ob Atomisierung das Compound-Problem (2d7b09-BESTELLUEBERSICHT) auflöst.
/// OHNE Transkript: nur Split (isolierte Atomisierungs-Qualität). Isoliert, additiv.
/// Aufruf: <c>claim-split-spike &lt;fixture.json&gt; [transkript.txt] [judge]</c>.
/// </summary>
public static class ClaimSplitSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: claim-split-spike <fixture.json> [transcript.txt] [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[split] Fixture fehlt: {fixturePath}"); return 2; }

        string? transcriptArg = null, judgeArg = null;
        foreach (var a in args.Skip(2))
        {
            if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptArg = a;
            else judgeArg = a;
        }

        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var units = envelope?.Units ?? new List<SplitUnit>();
        if (units.Count == 0) { Console.Error.WriteLine("[split] keine units."); return 2; }

        IReadOnlyList<TranscriptTurn> turns = Array.Empty<TranscriptTurn>();
        string? transcriptName = null;
        if (transcriptArg is not null)
        {
            var tp = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, "input", "transcripts", transcriptArg);
            if (!File.Exists(tp)) { Console.Error.WriteLine($"[split] Transkript fehlt: {tp}"); return 2; }
            turns = TranscriptSegmenter.Segment(await File.ReadAllTextAsync(tp).ConfigureAwait(false));
            transcriptName = Path.GetFileNameWithoutExtension(tp);
        }
        var fullChain = turns.Count > 0;

        var client = ChatClientFactory.Create(judgeSettings);
        var splitter = new ClaimSplitter(client, settings.JuryStructuredOutput);
        var selector = new EvidenceSelector(client, settings.JuryStructuredOutput);
        var verifier = new ClaimEvidenceVerifier(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[split] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)}  Modus: {(fullChain ? "VOLLE KETTE (split→select→verify)" : "nur split")}  Judge: {judgeSettings.ModelId}");

        var results = new List<object>();
        foreach (var u in units)
        {
            var atomics = await splitter.SplitDetailedAsync(u.Unit, u.Artifact, CancellationToken.None).ConfigureAwait(false);
            Console.WriteLine($"\n[UNIT] {u.Id} ({u.Artifact}) → {atomics.Count} atomare Claims");

            var claimResults = new List<object>();
            for (var i = 0; i < atomics.Count; i++)
            {
                var atomic = atomics[i];
                var claim = atomic.NormalizedClaim;
                if (!fullChain)
                {
                    Console.WriteLine($"   C{i + 1}: {claim}");
                    claimResults.Add(new
                    {
                        claim,
                        atomic.ArtifactQuote,
                        facets = atomic.Facets,
                        atomic.SplitReason
                    });
                    continue;
                }

                var sel = await selector.SelectAsync(claim, u.Artifact, turns, CancellationToken.None).ConfigureAwait(false);
                var evidence = EvidenceSelector.ToEvidence(sel, turns);
                var verdict = await verifier.VerifyAsync(
                    new ClaimEvidenceCase(
                        $"{u.Id}-C{i + 1}",
                        u.Artifact,
                        claim,
                        "n/a",
                        evidence,
                        ArtifactQuote: atomic.ArtifactQuote,
                        Facets: atomic.Facets.ToDictionary()),
                    CancellationToken.None).ConfigureAwait(false);
                Console.WriteLine($"   C{i + 1} [{verdict.Label}/{verdict.Verdict}] turns=[{string.Join(",", sel.Turns)}]: {claim}");
                claimResults.Add(new
                {
                    claim,
                    atomic.ArtifactQuote,
                    facets = atomic.Facets,
                    atomic.SplitReason,
                    selectedTurns = sel.Turns,
                    verdict.Label,
                    verdict.Verdict,
                    verdict.Support,
                    verdict.Reason
                });
            }

            results.Add(new { u.Id, u.Artifact, u.Unit, u.Note, atomCount = atomics.Count, claims = claimResults });
        }

        var payload = new { fixture = Path.GetRelativePath(repoRoot, fixturePath), transcript = transcriptName, judge = judgeSettings.ModelId, fullChain, units = results };
        var outDir = Path.Combine(repoRoot, "thesis-evidence", "claim-grounding-spike");
        Directory.CreateDirectory(outDir);
        var transcriptSlug = transcriptName is not null ? $"{transcriptName}." : "";
        var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(fixturePath)}.{transcriptSlug}{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);
        Console.WriteLine($"\n[split] {units.Count} Units zerlegt -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    private sealed record SplitUnit(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("artifact")] string Artifact,
        [property: JsonPropertyName("unit")] string Unit,
        [property: JsonPropertyName("note")] string? Note = null);

    private sealed record FixtureEnvelope([property: JsonPropertyName("units")] List<SplitUnit> Units);
}
