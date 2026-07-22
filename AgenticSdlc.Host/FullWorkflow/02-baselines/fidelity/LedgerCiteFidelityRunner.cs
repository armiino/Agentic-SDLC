using System.Text.Json;
using System.Text.RegularExpressions;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Fidelity;

/// <summary>
/// A2 (Capstone-Rahmung „Demonstration"): der NICHT-DEKORATIV-Beleg für den direkten, ledger-geerdeten Arm B.
/// CLI: <c>ledger-cite-fidelity &lt;requirements.md&gt; &lt;consumable.json&gt; [judgeModel]</c>.
/// <para>Der direkte Arm B schreibt <c>requirements.md</c> mit INLINE-Zitaten je Bullet (<c>… [claim_id, claim_id]</c>).
/// Dieser Runner (1) parst die Bullets deterministisch in (Aussage, Anker-Claim-IDs), (2) projiziert den consumable-
/// Ledger als Baseline (Claim-ID → Claim-Text) und (3) fährt den <b>unveränderten</b> <see cref="InferenceChecker"/>
/// mit einem Requirement-Treue-Maßstab (<c>RequirementsCiteFidelityJudge1</c>) darüber. Ergebnis = <b>supported-Rate</b>
/// = Anteil Anforderungen, deren Zitat die Claim TREU wiedergibt (nicht bloß existiert).</para>
/// </summary>
/// <remarks>
/// Kein neues Mess-Instrument: der Entailment-Kern (supported/contradicts/unrelated) wird wiederverwendet (driftfrei).
/// Der deterministische Teil (Anker existiert) ist der Anker-/ID-Check; der supported-Rate-Teil ist der SEMANTISCHE
/// „nicht dekorativ"-Beleg. EHRLICH: der Judge ist ein fehlbarer LLM-Filter (A4 = manuelle Stichprobe benennt das).
/// Exit: 0 = ok, 2 = Usage/IO, 4 = LLM-Fehler.
/// </remarks>
public static class LedgerCiteFidelityRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "EvidenceRequirementsAgent";
    private const string JudgePromptName = "RequirementsCiteFidelityJudge1";

    // Ein Anforderungs-Bullet mit optionalem Inline-Zitat am Ende:  "- <text> [id, id]".
    private static readonly Regex BulletRx = new(@"^\s*[-*]\s+(?<text>.*?)\s*(?:\[(?<ids>[^\]]+)\])?\s*$", RegexOptions.Compiled);

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: ledger-cite-fidelity <requirements.md> <consumable.json> [judgeModel] [--probe]");
            return 2;
        }
        var mdPath = Resolve(repoRoot, args[1]);
        var consPath = Resolve(repoRoot, args[2]);
        var judgeModelArg = args.Length > 3 && !args[3].StartsWith("--", StringComparison.Ordinal) ? args[3].Trim() : null;
        var probe = args.Contains("--probe");
        if (consPath is null || !File.Exists(consPath)) { Console.Error.WriteLine($"[cite-fidelity] consumable.json fehlt: '{args[2]}'."); return 2; }
        // Diskriminierungs-Test: belegt, dass der Checker Untreue AUCH fängt (sonst ist eine supported-Rate von 100 % ein
        // Gummistempel). Braucht nur den consumable; requirements.md wird ignoriert.
        if (probe) return await RunProbeAsync(consPath, judgeModelArg, settings, repoRoot).ConfigureAwait(false);
        if (mdPath is null || !File.Exists(mdPath)) { Console.Error.WriteLine($"[cite-fidelity] requirements.md fehlt: '{args[1]}'."); return 2; }

        // (1) Bullets deterministisch parsen → zitiert (mit Anker-Claim-IDs) vs. unzitiert.
        var cited = new List<ArtifactItem>();
        var uncited = new List<string>();
        var n = 0;
        foreach (var raw in await File.ReadAllLinesAsync(mdPath).ConfigureAwait(false))
        {
            var line = raw.TrimEnd();
            if (line.TrimStart().StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;   // Überschriften/Leerzeilen
            var m = BulletRx.Match(line);
            if (!m.Success) continue;   // kein Bullet (Fließtext) → ignorieren
            var text = m.Groups["text"].Value.Trim();
            if (string.IsNullOrWhiteSpace(text)) continue;
            var ids = m.Groups["ids"].Success
                ? m.Groups["ids"].Value.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList()
                : [];
            if (ids.Count == 0) { uncited.Add(text); continue; }
            n++;
            // Anker der Claim-IDs liegen in SourceArtifactItemIds (das Feld, das der InferenceChecker liest); zusätzlich
            // in SourceClaimIds (semantisch korrekt: es SIND Ledger-Claim-IDs).
            cited.Add(new ArtifactItem($"REQ-{n:D3}", ArtifactOrigin.Extracted, text, SourceClaimIds: ids, SourceArtifactItemIds: ids));
        }

        var totalBullets = cited.Count + uncited.Count;
        if (cited.Count == 0)
        {
            // Arm-A-Fall (illustrative Folie): keine Inline-Zitate → NICHTS ist nachvollziehbar/cite-prüfbar. Das IST der
            // Befund („was du bei freier Generierung aufgibst"), kein Fehler → deterministischer Report, kein Judge.
            var runA = new RunContext(RunId.New(), "fidelity");
            runA.EnsureFolders();
            var reportA = new
            {
                requirements = Path.GetRelativePath(repoRoot, mdPath), consumable = Path.GetRelativePath(repoRoot, consPath),
                totalBullets, citedBullets = 0, uncitedBullets = uncited.Count, citedRate = 0.0,
                note = "keine Inline-Zitate — nicht cite-prüfbar (Traceability = 0).", uncitedStatements = uncited
            };
            var pathA = Path.Combine(runA.RunDir, "cite-fidelity-report.json");
            await File.WriteAllTextAsync(pathA, JsonSerializer.Serialize(reportA, Json)).ConfigureAwait(false);
            runA.AppendEvent(new { type = "CITE_FIDELITY_UNCITED", runId = runA.RunId, totalBullets, cited = 0, timestampUtc = DateTime.UtcNow });
            Console.WriteLine($"[cite-fidelity] runId={runA.RunId}  bullets={totalBullets} zitiert=0 → Traceability = 0 (nicht cite-prüfbar).  -> {Path.GetRelativePath(repoRoot, pathA)}");
            return 0;
        }

        // (2) consumable-Ledger als Baseline (Claim-ID → Claim-Text als ArtifactItem).
        var claims = LedgerClaimIndex.LoadOrEmpty(consPath);
        var baselineById = claims.ToDictionary(
            kv => kv.Key,
            kv => new ArtifactItem(kv.Key, ArtifactOrigin.Extracted, kv.Value.Proposition, SourceClaimIds: [], SourceArtifactItemIds: []),
            StringComparer.Ordinal);

        // Deterministisch: welche zitierten Anker sind NICHT im consumable auflösbar (dekorativ auf ID-Ebene)?
        var unresolvable = cited
            .SelectMany(it => it.SourceArtifactItemIds.Where(id => !baselineById.ContainsKey(id)).Select(id => new { item = it.ItemId, anchor = id }))
            .ToList();

        var run = new RunContext(RunId.New(), "fidelity");
        run.EnsureFolders();
        var judgeSettings = !string.IsNullOrWhiteSpace(judgeModelArg) ? settings with { ModelId = judgeModelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        run.WriteConfig(new
        {
            command = "ledger-cite-fidelity", runId = run.RunId,
            requirements = Path.GetRelativePath(repoRoot, mdPath), consumable = Path.GetRelativePath(repoRoot, consPath),
            provider = settings.LlmProvider, judgeModel = judgeSettings.ModelId,
            citedBullets = cited.Count, uncitedBullets = uncited.Count, ledgerClaims = claims.Count, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[cite-fidelity] runId={run.RunId}  zitiert={cited.Count} unzitiert={uncited.Count}  ledger-claims={claims.Count}  judge={judgeSettings.ModelId}");
        if (unresolvable.Count > 0) Console.WriteLine($"[cite-fidelity] HINWEIS: {unresolvable.Count} zitierte Anker nicht im consumable auflösbar (dekorativ auf ID-Ebene).");

        // (3) InferenceChecker-Kern (unverändert) mit Requirement-Treue-Maßstab.
        var checkClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "LedgerCiteFidelity", SourceName);
        var judgePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, JudgePromptName, new Dictionary<string, string>());
        var checker = new InferenceChecker(checkClient, settings.JuryStructuredOutput, systemPrompt: judgePrompt);

        InferenceCheckReport report;
        try
        {
            report = await checker.CheckAsync(cited, baselineById, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[cite-fidelity] Judge-Lauf fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var supported = report.ByVerdict.TryGetValue(nameof(InferenceVerdictKind.Supported), out var s) ? s : 0;
        var supportedRate = Math.Round((double)supported / cited.Count, 4);
        var outObj = new
        {
            requirements = Path.GetRelativePath(repoRoot, mdPath), consumable = Path.GetRelativePath(repoRoot, consPath),
            judgeModel = judgeSettings.ModelId,
            totalBullets, citedBullets = cited.Count, uncitedBullets = uncited.Count,
            citedRate = Math.Round((double)cited.Count / totalBullets, 4),
            supported, supportedRate,
            byVerdict = report.ByVerdict,
            unresolvableAnchors = unresolvable,
            uncitedStatements = uncited,
            flagged = report.Flagged,
            verdicts = report.Verdicts
        };
        var reportPath = Path.Combine(run.RunDir, "cite-fidelity-report.json");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(outObj, Json)).ConfigureAwait(false);
        run.AppendEvent(new { type = "CITE_FIDELITY_DONE", runId = run.RunId, cited = cited.Count, supported, supportedRate, byVerdict = report.ByVerdict, timestampUtc = DateTime.UtcNow });

        Console.WriteLine($"[cite-fidelity] supported-Rate = {supportedRate:P1} ({supported}/{cited.Count})  verdikte={string.Join(", ", report.ByVerdict.Select(kv => $"{kv.Key}={kv.Value}"))}");
        Console.WriteLine($"[cite-fidelity] -> {Path.GetRelativePath(repoRoot, reportPath)}");
        return 0;
    }

    /// <summary>
    /// Adversarialer Diskriminierungs-Test (A4-artig): konstruiert deterministisch 3 ABSICHTLICH untreue Anforderungen
    /// aus echten consumable-Claims — deren zitierte Anker AUFLÖSEN (bestehen den deterministischen ID-Check), sodass NUR
    /// der Judge sie fangen kann. Bestanden = alle 3 geflaggt (contradicts/unrelated). Erst das macht eine supported-Rate
    /// von 100 % belastbar (kein Gummistempel).
    /// </summary>
    private static async Task<int> RunProbeAsync(string consPath, string? judgeModelArg, HostSettings settings, string repoRoot)
    {
        var claims = LedgerClaimIndex.LoadOrEmpty(consPath);
        if (claims.Count < 2) { Console.Error.WriteLine("[cite-fidelity --probe] consumable braucht ≥2 Claims."); return 2; }
        var list = claims.Values.ToList();

        // Baseline = voller consumable → die zitierten Anker sind IMMER auflösbar (der ID-Check würde NICHT flaggen);
        // ein Flag kann also nur semantisch (vom Judge) kommen — genau das wollen wir testen.
        var baselineById = claims.ToDictionary(
            kv => kv.Key,
            kv => new ArtifactItem(kv.Key, ArtifactOrigin.Extracted, kv.Value.Proposition, SourceClaimIds: [], SourceArtifactItemIds: []),
            StringComparer.Ordinal);

        // Probe 1 — Modalitäts-Eskalation: einen GENUIN unverbindlichen Claim (Wunsch/offen) als harte Zusage behaupten.
        // Marker müssen unzweideutig Optionalität signalisieren — NICHT "geklärt werden"/"muss" (das sind feste Pflichten,
        // die eine feste Zusage NICHT eskalieren, sondern bestätigen würden → falsch-negative Probe, run f03817).
        string[] hedges = ["gewünscht", "bleibt offen", "bleibt unklar", "ob und", "ob und wann", "für später"];
        var firm = new[] { "muss", "müssen", "geklärt werden" };
        var hedged = list.FirstOrDefault(c =>
                        hedges.Any(h => c.Proposition.Contains(h, StringComparison.OrdinalIgnoreCase)) &&
                        !firm.Any(f => c.Proposition.Contains(f, StringComparison.OrdinalIgnoreCase)))
                     ?? list.FirstOrDefault(c => hedges.Any(h => c.Proposition.Contains(h, StringComparison.OrdinalIgnoreCase)))
                     ?? list[0];
        // Probe 2 — erfundene Konkretheit: unbelegte Zahl/Frist/Norm an einen realen Claim hängen.
        var c2 = list.FirstOrDefault(c => c.Id != hedged.Id) ?? list[0];
        // Probe 3 — dekorativer Anker: Text von Claim X, Zitat aber auf einen unverwandten Claim Y.
        var cx = list[0];
        var cy = list[^1].Id != cx.Id ? list[^1] : list[1];

        // Sauberer Modalitäts-Flip: weiche Tokens ERSETZEN (kein weicher Rest im Text, sonst ankert der Judge am
        // semantischen Kern und ignoriert das harte Framing — runs f03817/2737b0).
        var modalityText = hedged.Proposition;
        (string From, string To)[] flips =
        [
            ("ob und in welchem umfang dies umgesetzt wird, bleibt offen", "dies wird vollständig und verbindlich umgesetzt"),
            ("ob und wann dies umgesetzt wird, bleibt unklar", "dies wird verbindlich umgesetzt"),
            ("bleibt offen", "ist verbindlich entschieden"), ("bleibt unklar", "ist verbindlich entschieden"),
            ("gewünscht", "verbindlich zugesagt"), ("gesucht", "verbindlich festgelegt"), ("für später", "sofort verpflichtend"),
        ];
        foreach (var (from, to) in flips)
            modalityText = Regex.Replace(modalityText, Regex.Escape(from), to, RegexOptions.IgnoreCase);
        if (string.Equals(modalityText, hedged.Proposition, StringComparison.Ordinal))
            modalityText = "Es ist verbindlich zugesagt und wird garantiert vollständig umgesetzt (ohne Vorbehalt): " + hedged.Proposition;

        var probes = new (string Id, string Kind, string Text, string Anchor, string Expect)[]
        {
            ("PROBE-MODALITY", "Modalitäts-Eskalation (Wunsch/offen → harte Zusage)",
                modalityText, hedged.Id, "contradicts"),
            ("PROBE-SPECIFICS", "Erfundene Konkretheit (Zahl/Frist/Norm)",
                c2.Proposition + " Dies muss innerhalb von exakt 24 Stunden, zu Kosten von höchstens 5 Euro pro Nutzer und gemäß DIN-Norm 9241-110 erfolgen.",
                c2.Id, "contradicts"),
            ("PROBE-DECOY", "Dekorativer/falscher Anker (Text X, Zitat Y)",
                cx.Proposition, cy.Id, "unrelated"),
        };

        var items = probes.Select(p => new ArtifactItem(p.Id, ArtifactOrigin.Extracted, p.Text, SourceClaimIds: [p.Anchor], SourceArtifactItemIds: [p.Anchor])).ToList();

        var run = new RunContext(RunId.New(), "fidelity");
        run.EnsureFolders();
        var judgeSettings = !string.IsNullOrWhiteSpace(judgeModelArg) ? settings with { ModelId = judgeModelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        run.WriteConfig(new
        {
            command = "ledger-cite-fidelity --probe", runId = run.RunId,
            consumable = Path.GetRelativePath(repoRoot, consPath), provider = settings.LlmProvider, judgeModel = judgeSettings.ModelId,
            probes = probes.Length, ledgerClaims = claims.Count, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[cite-fidelity --probe] runId={run.RunId}  probes={items.Count}  judge={judgeSettings.ModelId} — erwartet: ALLE geflaggt (nicht supported).");

        var checkClient = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(judgeSettings), settings, run, "LedgerCiteFidelityProbe", SourceName);
        var judgePrompt = PromptProvider.Load(repoRoot, Phase, AgentName, JudgePromptName, new Dictionary<string, string>());
        var checker = new InferenceChecker(checkClient, settings.JuryStructuredOutput, systemPrompt: judgePrompt);

        InferenceCheckReport report;
        try
        {
            report = await checker.CheckAsync(items, baselineById, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[cite-fidelity --probe] Judge-Lauf fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        var byId = report.Verdicts.ToDictionary(v => v.ItemId, v => v, StringComparer.Ordinal);
        var rows = probes.Select(p =>
        {
            var v = byId.TryGetValue(p.Id, out var vv) ? vv : null;
            var kind = v?.Verdict ?? InferenceVerdictKind.Unclear;
            var flagged = kind is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated;
            return new { p.Id, p.Kind, anchor = p.Anchor, expected = p.Expect, actual = kind.ToString(), flagged, rationale = v?.Rationale ?? "" };
        }).ToList();
        var flaggedCount = rows.Count(r => r.flagged);
        var discriminationPass = flaggedCount == rows.Count;

        var outObj = new
        {
            command = "ledger-cite-fidelity --probe", consumable = Path.GetRelativePath(repoRoot, consPath), judgeModel = judgeSettings.ModelId,
            discriminationPass, flagged = flaggedCount, total = rows.Count, probes = rows
        };
        var reportPath = Path.Combine(run.RunDir, "cite-fidelity-probe-report.json");
        await File.WriteAllTextAsync(reportPath, JsonSerializer.Serialize(outObj, Json)).ConfigureAwait(false);
        run.AppendEvent(new { type = "CITE_FIDELITY_PROBE_DONE", runId = run.RunId, discriminationPass, flagged = flaggedCount, total = rows.Count, timestampUtc = DateTime.UtcNow });

        foreach (var r in rows)
            Console.WriteLine($"  {(r.flagged ? "✓ FLAGGED" : "✗ MISSED ")}  {r.Id} [{r.actual}] (erwartet {r.expected})  {r.Kind}");
        Console.WriteLine($"[cite-fidelity --probe] Diskriminierung: {(discriminationPass ? "BESTANDEN" : "NICHT bestanden")} — {flaggedCount}/{rows.Count} geflaggt.  -> {Path.GetRelativePath(repoRoot, reportPath)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
