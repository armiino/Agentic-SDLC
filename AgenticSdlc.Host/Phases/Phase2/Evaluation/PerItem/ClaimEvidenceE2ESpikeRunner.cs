using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// Claim-Pilot Stufe 2 — END-TO-END-Spike (ImplementClaimAnsatz §10): testet die volle Kette
/// <c>Claim → EvidenceSelector (AUTO) → ClaimEvidenceVerifier</c>. Im Gegensatz zum Stufe-1-Spike
/// (`claim-grounding-spike`, manuelle Evidenz) wählt hier das Modell die Evidenz selbst aus dem ganzen
/// Transkript. Kernfrage: hält die automatische Evidence-Auswahl die Verifier-Gewinne der manuellen Evidenz?
/// Vergleicht je Case gegen (a) das Handlabel und (b) — falls vorhanden — das manuelle Stufe-1-Ergebnis.
/// Aufruf: <c>claim-evidence-e2e-spike &lt;fixture.json&gt; &lt;transkript.txt&gt; [judge]</c>.
/// </summary>
public static class ClaimEvidenceE2ESpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: claim-evidence-e2e-spike <fixture.json> <transcript.txt> [judgeModelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[e2e] Fixture fehlt: {fixturePath}"); return 2; }

        var transcriptPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, "input", "transcripts", args[2]);
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[e2e] Transkript fehlt: {transcriptPath}"); return 2; }

        var judgeArg = args.Length >= 4 ? args[3] : null;
        var judgeSettings =
            judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var envelope = JsonSerializer.Deserialize<FixtureEnvelope>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var cases = envelope?.Cases ?? new List<ClaimEvidenceCase>();
        if (cases.Count == 0) { Console.Error.WriteLine("[e2e] keine cases."); return 2; }

        var turns = TranscriptSegmenter.Segment(await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false));

        // Manuelles Stufe-1-Ergebnis laden (Obergrenze), falls vorhanden → Vergleich auto vs. manuell.
        var manualLabels = LoadManualLabels(repoRoot, fixturePath, modelSlug);

        var client = ChatClientFactory.Create(judgeSettings);
        var selector = new EvidenceSelector(client, settings.JuryStructuredOutput);
        var verifier = new ClaimEvidenceVerifier(client, settings.JuryStructuredOutput);

        Console.WriteLine($"[e2e] Fixture: {Path.GetRelativePath(repoRoot, fixturePath)}  Transkript: {Path.GetFileName(transcriptPath)} ({turns.Count} Turns)  Judge: {judgeSettings.ModelId}");

        var results = new List<object>();
        int matchExpected = 0, sameAsManual = 0, manualComparable = 0;

        foreach (var c in cases)
        {
            var selection = await selector.SelectAsync(c.Claim, c.Artifact, turns, CancellationToken.None).ConfigureAwait(false);
            var autoEvidence = EvidenceSelector.ToEvidence(selection, turns);
            var autoCase = c with { Evidence = autoEvidence };
            var verdict = await verifier.VerifyAsync(autoCase, CancellationToken.None).ConfigureAwait(false);

            if (verdict.MatchesExpected) matchExpected++;
            string? manual = manualLabels.GetValueOrDefault(c.Id);
            string flip = "";
            if (manual is not null)
            {
                manualComparable++;
                if (string.Equals(manual, verdict.Label, StringComparison.Ordinal)) sameAsManual++;
                else flip = $"  (manual={manual} → auto={verdict.Label})";
            }

            results.Add(new
            {
                c.Id, c.Artifact, c.Claim, c.ExpectedLabel,
                selectedTurns = selection.Turns,
                selectionReason = selection.Reason,
                autoEvidence,
                verdict = new { verdict.Verdict, verdict.Label, verdict.MatchesExpected, verdict.Support, verdict.ModalityPreserved, verdict.StatusPreserved, verdict.ScopePreserved, verdict.Reason },
                manualLabel = manual
            });

            var mark = verdict.MatchesExpected ? "OK" : "MISS";
            Console.WriteLine($"[{mark}] {c.Id}: exp={c.ExpectedLabel} auto={verdict.Label}/{verdict.Verdict} turns=[{string.Join(",", selection.Turns)}]{flip}");
        }

        var payload = new
        {
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            transcript = Path.GetFileName(transcriptPath),
            judge = judgeSettings.ModelId,
            mode = "end-to-end (auto evidence selection)",
            total = cases.Count,
            matchesExpected = matchExpected,
            accuracyVsHandlabel = Math.Round(matchExpected / (double)cases.Count, 4),
            comparedToManual = manualComparable,
            sameAsManual,
            results
        };
        var outDir = Path.Combine(repoRoot, "thesis-evidence", "claim-grounding-spike");
        Directory.CreateDirectory(outDir);
        var transcriptSlug = Path.GetFileNameWithoutExtension(transcriptPath);
        var outFile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(fixturePath)}.e2e.{transcriptSlug}.{modelSlug}.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[e2e] vs Handlabel: {matchExpected}/{cases.Count}" +
            (manualComparable > 0 ? $"  ·  gleich wie manuelle Evidenz: {sameAsManual}/{manualComparable}" : "") +
            $"  -> {Path.GetRelativePath(repoRoot, outFile)}");
        return 0;
    }

    /// <summary>Liest die manuellen Stufe-1-Labels (id → label) aus dem `claim-grounding-spike`-Output, falls da.</summary>
    private static Dictionary<string, string> LoadManualLabels(string repoRoot, string fixturePath, string modelSlug)
    {
        var manualPath = Path.Combine(repoRoot, "thesis-evidence", "claim-grounding-spike",
            $"{Path.GetFileNameWithoutExtension(fixturePath)}.{modelSlug}.json");
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(manualPath)) return map;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(manualPath));
            foreach (var r in doc.RootElement.GetProperty("results").EnumerateArray())
                if (r.TryGetProperty("id", out var id) && r.TryGetProperty("verdict", out var v) && v.TryGetProperty("label", out var l))
                    map[id.GetString()!] = l.GetString()!;
        }
        catch { /* Vergleich optional */ }
        return map;
    }

    private sealed record FixtureEnvelope([property: JsonPropertyName("cases")] List<ClaimEvidenceCase> Cases);
}
