using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Welches Kategorienschema die Jury verwendet. Wird testweise verglichen
/// (siehe phase2notes/Architekturentscheidungen.md, Kalibrierungsrunde).
/// </summary>
public enum EvaluationScheme
{
    /// <summary>3 Kategorien (Rubrik v2.0): Falsche Behauptung / Falsche Sicherheit / Fehlendes Thema.</summary>
    ThreeCategory,
    /// <summary>2 Kategorien: Nicht gedeckt (vorhanden+falsch/zu sicher) / Fehlt (vollständig abwesend).</summary>
    TwoCategory,
    /// <summary>1 Liste: nur "Fehler", ohne Kategorisierung.</summary>
    SingleList,
    /// <summary>4 Kategorien, phasen-geführt (FALSE_CLAIM / FALSE_CERTAINTY / MISSING_TOPIC /
    /// INCOMPLETE_COVERAGE). Requirements-spezifisch; adressiert Open-Points-Falsch-Positive.</summary>
    PhasedFour,
    /// <summary>Synthese: 3 Kategorien (FALSE_CLAIM / FALSE_CERTAINTY / MISSING_TOPIC) mit
    /// Open-Points-Schutz + Hedge-Regel, OHNE INCOMPLETE_COVERAGE; „könnte-vollständiger"-Findings
    /// explizit verboten. Requirements-spezifisch.</summary>
    Synthesis
}

/// <summary>Eine Kategorie des aktiven Schemas: Modell-Key, Metrik-Name, Anzeige-Label.</summary>
public sealed record EvaluationCategory(string Key, string MetricName, string Label);

/// <summary>
/// DISK-9 (M4/F6): rohe Verifier-Antwort eines Kategorie-/Chunk-Calls samt gerendertem Kandidaten-Batch.
/// Persistiert als .raw.json damit Verifier-Fehler
/// (LLM vs. Prompt vs. Mapping vs. Batchgröße) ohne erneuten Run nachvollziehbar sind.
/// </summary>
public sealed record VerifierRawLog(
    string Category, int ChunkIndex, int CandidateCount, string RenderedCandidates, string RawResponse);

/// <summary>
/// LLM-as-judge Evaluator nach <c>phase2notes/qualitaetsrubrik.md</c>
/// (Fehler-Such-Ansatz, keine Gesamtnote). Schema-fähig: 3 / 2 / 1 Kategorien.
/// </summary>
/// <remarks>
/// MAF/MEAI-nativ: implementiert <see cref="IEvaluator"/>; gibt je Kategorie eine
/// <see cref="NumericMetric"/> (Anzahl) zurück, jedes Finding als <see cref="EvaluationDiagnostic"/>
/// (KRITISCH/MITTEL/GERING -> Error/Warning/Informational). Kontext-isoliert: sieht nur
/// Artefakt (modelResponse) + Transkript (ctor), keine Generierungs-Conversation.
/// Severity ist bewusst nur ein weiches Feld (Modell ankert empirisch auf MITTEL).
///
/// Synthese-Schema: kategoriespezifische zweite Verifikation (DISK-7), gesteuert durch
/// <see cref="JuryVerificationPolicy"/>. Pro aktivierter Kategorie ein eigener Batch-LLM-Call
/// mit eigenem Prompt (verschiedene Fragen je Kategorie), nur wenn Kandidaten existieren.
/// Einheitliches Verdikt für alle Kategorien: confirmed (zählt voll) / partial (eine Severity-Stufe
/// niedriger) / rejected (zählt nicht, bleibt transparent sichtbar).
///   - MISSING_TOPIC : „Ist das angeblich fehlende Thema doch (auch paraphrasiert) abgedeckt?"
///     (rejected = abgedeckt, partial = nur erwähnt/eine Facette, confirmed = wirklich absent).
///     Begründung der Dreistufigkeit: Run 92e6a3 flaggte Mehrsprachigkeit/Mehrwährung als
///     MISSING_TOPIC, obwohl in Assumptions erwähnt — eine binäre Logik trennte „erwähnt aber
///     unausgearbeitet" nicht von „vollständig abgedeckt".
///   - FALSE_CERTAINTY: „Markiert das Artefakt die Unsicherheit bereits selbst?" — mit
///     deterministischem Open-Marker-Precheck auf dem artifactQuote (K5). Adressiert den größten
///     False-Positive-Treiber (Artefakt sagt „offen/optional", Judge zählt „zu sicher").
///   - FALSE_CLAIM   : „Ist die Aussage durch das Transkript gedeckt?" — bewusst KONSERVATIV
///     (rejected nur bei klarer Transkript-Deckung; im Zweifel confirmed), damit echte
///     Halluzinationen nicht maskiert werden. Default aus (teurer Transkript-Re-Read).
/// </remarks>
public sealed class Evaluator : IEvaluator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /*
     * Strukturierte Ausgabe (response_format = json_schema): zwingt den Judge zu syntaktisch gültigem
     * JSON in fester Form. Behebt die Wurzelursache der Parsefehler (B4/B5/B6): freie Quotes/Backslashes
     * in artifactQuote/transcriptEvidence, die das Modell nicht sauber JSON-escaped. category/severity
     * bleiben Strings (nicht enum), damit dasselbe Schema fuer ALLE EvaluationScheme-Varianten gilt
     * (Synthese nutzt FALSE_CLAIM/…, ThreeCategory "1"/"2"/"3"). Nur die Form wird erzwungen, nicht die Werte.
     */
    private const string FindingsSchemaJson = """
        {
          "type": "object",
          "properties": {
            "findings": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "category": { "type": "string" },
                  "severity": { "type": "string" },
                  "artifactQuote": { "type": "string" },
                  "transcriptEvidence": { "type": "string" },
                  "reason": { "type": "string" }
                },
                "required": ["category", "severity", "artifactQuote", "transcriptEvidence", "reason"],
                "additionalProperties": false
              }
            }
          },
          "required": ["findings"],
          "additionalProperties": false
        }
        """;

    private const string VerificationSchemaJson = """
        {
          "type": "object",
          "properties": {
            "results": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "index": { "type": "integer" },
                  "verdict": { "type": "string" },
                  "evidence": { "type": "string" }
                },
                "required": ["index", "verdict", "evidence"],
                "additionalProperties": false
              }
            }
          },
          "required": ["results"],
          "additionalProperties": false
        }
        """;

    // .Clone() entkoppelt das JsonElement vom (disposed) JsonDocument.
    private static readonly ChatResponseFormat FindingsResponseFormat =
        ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(FindingsSchemaJson).RootElement.Clone(),
            "sdlc_findings",
            "Fehlerliste der SDLC-Artefakt-Pruefung.");

    private static readonly ChatResponseFormat VerificationResponseFormat =
        ChatResponseFormat.ForJsonSchema(
            JsonDocument.Parse(VerificationSchemaJson).RootElement.Clone(),
            "sdlc_finding_verification",
            "Kategoriespezifische Verifikation von Judge-Findings (confirmed/partial/rejected).");

    /*
     * K5 (DISK-7): Deterministischer Offenheitsmarker-Precheck für FALSE_CERTAINTY.
     * Geprüft wird AUSSCHLIESSLICH der artifactQuote des Findings (= die konkrete Claim-Stelle),
     * NICHT der gesamte Artefakttext.. sonst wuerden entfernte Open-Point-Abschnitte fälschlich
     * als Entlastung genutzt. Wortliste mit Wortgrenzen, damit "offen" nicht in "offenbar"/"offensichtlich" matcht.
     */
    private static readonly Regex OpenMarkerWordRegex = new(
        @"\b(offen|offene[nrs]?|optional|spaeter|später|ausstehend|vorlaeufig|vorläufig|unklar|tbd|ggf)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly string[] OpenMarkerPhrases =
    [
        "nicht final", "noch nicht", "noch offen", "wird noch", "zu klaeren", "zu klären",
        "to be defined", "to be determined", "entscheidung ausstehend", "noch zu entscheiden",
        "annahme"
    ];

    /// <summary>Name der Gesamt-Score-Metrik (Trigger für die Phase-3-Repair-Condition).</summary>
    public const string ErrorScoreMetricName = "ErrorScore";

    // Platzhalter-Schwelle: needsRepair, wenn ErrorScore >= dieser Wert (oder ein KRITISCH-Fehler).
    // In Phase 3 wird die eigentliche Schwelle als Workflow-Policy gesetzt; hier nur ein Default.
    private const double RepairScoreThreshold = 4;

    private readonly string _transcript;
    private readonly string _artifactName;
    private readonly string _systemPrompt;
    private readonly JuryVerificationPolicy _verification;
    private readonly bool _verificationActive; // nur Synthese-Schema hat die FALSE_CLAIM/FALSE_CERTAINTY/MISSING_TOPIC-Keys
    private readonly bool _useStructuredOutput;
    private readonly bool _splitGeneration; // DISK-12/G3: Call 1 pro Kategorie statt 3-in-1 (nur Synthese)
    private readonly IReadOnlySet<string> _enabledCategoryKeys; // DISK-12/B22: aktive Kategorien je Artefakttyp
    private readonly JuryPrompts? _prompts; // ausgelagerte Judge-Prompts; für Synthese Pflicht, für Legacy-Schemata null

    // DISK-9 (M4/F6): rohe Verifier-Antworten des letzten Laufs, pro Kategorie+Chunk, zur Persistenz.
    private readonly List<VerifierRawLog> _verifierRawLogs = new();

    // DISK-12/G3: rohe Generierungs-Antworten des letzten Laufs, pro Kategorie (nur im Split-Modus).
    private readonly List<VerifierRawLog> _generationRawLogs = new();

    public EvaluationScheme Scheme { get; }
    public IReadOnlyList<EvaluationCategory> Categories { get; }

    /// <summary>Rohe Haupt-Judge-Antwort des letzten <see cref="EvaluateAsync"/> (für Persistenz/Debug).</summary>
    public string? LastRawJudgeResponse { get; private set; }

    /// <summary>True, wenn die Haupt-Judge-Antwort des letzten Laufs nicht als JSON parsebar war.</summary>
    public bool LastEvaluationFailed { get; private set; }

    /// <summary>Parse-Fehlermeldung des letzten Laufs (null = ok).</summary>
    public string? LastParseError { get; private set; }

    /// <summary>
    /// DISK-9 (M4/F6): rohe Verifier-Antworten des letzten <see cref="EvaluateAsync"/>, pro Kategorie
    /// und Chunk. Macht direkt prüfbar, ob ein Verifier-Fehler vom LLM, Prompt, Mapping oder Batch kam.
    /// </summary>
    public IReadOnlyList<VerifierRawLog> LastVerifierRawLogs => _verifierRawLogs;

    /// <summary>
    /// DISK-12/G3: rohe Generierungs-Antworten des letzten <see cref="EvaluateAsync"/>, pro Kategorie
    /// (nur im Split-Modus). Leer im 3-in-1-Modus (dann steht die Rohantwort in LastRawJudgeResponse).
    /// </summary>
    public IReadOnlyList<VerifierRawLog> LastGenerationRawLogs => _generationRawLogs;

    public Evaluator(
        EvaluationScheme scheme,
        string transcript,
        string artifactName,
        bool useStructuredOutput = true,
        JuryVerificationPolicy? verification = null,
        JuryPrompts? prompts = null,
        bool splitGeneration = true,
        JuryCategoryProfile? categoryProfile = null)
    {
        Scheme = scheme;
        _transcript = transcript ?? throw new ArgumentNullException(nameof(transcript));
        _artifactName = string.IsNullOrWhiteSpace(artifactName) ? "artifact" : artifactName;
        // null = Legacy-Aufrufer ohne Policy -> bewusst nur MISSING_TOPIC (altes Verhalten, kein
        // stiller Verhaltenswechsel). Die Runner reichen die Config-Policy explizit durch.
        _verification = verification ?? JuryVerificationPolicy.MissingOnly;
        _verificationActive = scheme == EvaluationScheme.Synthesis;
        _useStructuredOutput = useStructuredOutput;
        _splitGeneration = splitGeneration;
        // DISK-12/B22: aktive Kategorien für DIESES Artefakt (Default-Profil + run-config-Override).
        // null = alle Kategorien (Legacy/Nicht-Synthese-Aufrufer ohne Profil).
        _enabledCategoryKeys = (categoryProfile ?? new JuryCategoryProfile()).EnabledFor(_artifactName);
        _prompts = prompts;

        if (scheme == EvaluationScheme.Synthesis)
        {
            // Synthese nutzt feste 3 Kategorien + einen artefakt-spezifisch zusammengebauten Prompt
            // aus den ausgelagerten Dateien (Header + Profil + Body).
            _prompts = prompts ?? throw new ArgumentNullException(
                nameof(prompts),
                "Das Synthese-Schema benötigt ausgelagerte JuryPrompts (JuryPromptLoader.Load(repoRoot)).");
            Categories = SynthesisCategories;
            _systemPrompt = _prompts.BuildSynthesisPrompt(_artifactName);
        }
        else
        {
            (Categories, _systemPrompt) = Configure(scheme);
        }
    }

    public IReadOnlyCollection<string> EvaluationMetricNames => Categories.Select(c => c.MetricName).ToArray();

    public async ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages,
        ChatResponse modelResponse,
        ChatConfiguration? chatConfiguration = null,
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = default)
    {
        if (chatConfiguration is null)
        {
            throw new ArgumentNullException(
                nameof(chatConfiguration),
                "Evaluator benötigt eine ChatConfiguration (LLM-as-judge).");
        }

        var artifactText = modelResponse.Text;
        _verifierRawLogs.Clear();    // DISK-9: Rohlogs des vorherigen Laufs verwerfen.
        _generationRawLogs.Clear();  // DISK-12: dito für Generierungs-Rohlogs.

        // Call 1 (Finding-Generierung). DISK-12/G3: im Split-Modus pro Kategorie ein fokussierter Call
        // (höherer Recall schwacher Modelle, B20); sonst ein 3-in-1-Call (Legacy/Nicht-Synthese).
        IReadOnlyList<Finding> findings;
        string? parseError;
        if (_verificationActive && _splitGeneration)
            (findings, parseError) = await GenerateSplitAsync(
                    artifactText, chatConfiguration.ChatClient, cancellationToken)
                .ConfigureAwait(false);
        else
            (findings, parseError) = await GenerateSingleAsync(
                    artifactText, chatConfiguration.ChatClient, cancellationToken)
                .ConfigureAwait(false);

        // Parse-Status offenlegen, damit der Runner sie persistieren kann (LastRawJudgeResponse wird
        // in den Generate*-Methoden gesetzt).
        LastParseError = parseError;
        LastEvaluationFailed = parseError is not null;

        // DISK-12/B22: Findings deaktivierter Kategorien verwerfen (greift im 3-in-1-Pfad, der
        // alle 3 Kategorien generiert; im Split sind sie schon übersprungen -> hier No-op, aber defensiv).
        findings = FilterEnabledCategories(findings);

        // Kategoriespezifische Verifikation (DISK-7): pro AKTIVIERTER Kategorie ein eigener Batch-Call,
        // nur wenn Kandidaten existieren. Einheitliches Verdikt confirmed/partial/rejected.
        // Nur für das Synthese-Instrument (nur dieses hat die passenden Kategorie-Keys + Verifier-Prompts).
        var verdictMap = new Dictionary<int, FindingVerdict>();
        if (_verificationActive && _verification.Custom)
        {
            // Custom-Modus: EIN ausgelagerter Verifier prüft ALLE Kandidaten kategorie-übergreifend
            // (ersetzt die drei eingebauten Verifier; kein Precheck).
            var candidateIndices = new List<int>();
            for (var i = 0; i < findings.Count; i++)
                if (Categories.Any(c => MatchesCategory(findings[i].Category, c.Key)))
                    candidateIndices.Add(i);

            if (candidateIndices.Count > 0)
            {
                var verdicts = await VerifyCustomAsync(
                        findings, candidateIndices, artifactText,
                        chatConfiguration.ChatClient, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var (index, verdict) in verdicts)
                    verdictMap[index] = verdict;
            }
        }
        else if (_verificationActive)
        {
            foreach (var category in Categories)
            {
                // DISK-12/B22: deaktivierte Kategorie hat ohnehin keine Findings (gefiltert) -> kein Verifier-Call.
                if (!_verification.IsEnabled(category.Key) || !_enabledCategoryKeys.Contains(category.Key))
                    continue;

                var candidateIndices = new List<int>();
                for (var i = 0; i < findings.Count; i++)
                    if (MatchesCategory(findings[i].Category, category.Key))
                        candidateIndices.Add(i);

                if (candidateIndices.Count == 0)
                    continue;

                var verdicts = await VerifyCategoryAsync(
                        category.Key, findings, candidateIndices, artifactText,
                        chatConfiguration.ChatClient, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var (index, verdict) in verdicts)
                    verdictMap[index] = verdict;
            }
        }

        var metrics = new List<EvaluationMetric>();
        var matchedIndices = new HashSet<int>();
        // allKept enthält sowohl unveränderte als auch herabgestufte Findings (mit effektiver Severity).
        var allKept = new List<Finding>();

        foreach (var category in Categories)
        {
            // DISK-12/B22: Kategorie für diesen Artefakttyp aktiv? (Nicht-Synthese: immer aktiv.)
            var categoryEnabled = !_verificationActive || _enabledCategoryKeys.Contains(category.Key);

            // Im Custom-Modus sind alle Synthese-Kategorien durch den Custom-Verifier abgedeckt.
            var verificationEnabled = categoryEnabled && _verificationActive
                && (_verification.Custom || _verification.IsEnabled(category.Key));

            var kept = new List<Finding>();      // confirmed (verifiziert) ODER unverifiziert -> volles Gewicht
            var downgraded = new List<(Finding Original, Finding Effective, string Evidence)>();
            var dropped = new List<(Finding Finding, string Evidence)>();
            int confirmedCount = 0, partialCount = 0, rejectedCount = 0, unverifiedCount = 0;

            for (var i = 0; i < findings.Count; i++)
            {
                if (!MatchesCategory(findings[i].Category, category.Key))
                    continue;

                matchedIndices.Add(i);

                if (verificationEnabled && verdictMap.TryGetValue(i, out var v))
                {
                    switch (v.Verdict)
                    {
                        case "rejected":
                            dropped.Add((findings[i], v.Evidence));
                            rejectedCount++;
                            break;
                        case "partial":
                            downgraded.Add((findings[i],
                                findings[i] with { Severity = DowngradeSeverity(findings[i].Severity) },
                                v.Evidence));
                            partialCount++;
                            break;
                        default: // confirmed
                            kept.Add(findings[i]);
                            confirmedCount++;
                            break;
                    }
                }
                else
                {
                    // Verifikation deaktiviert -> unverifiziert. (Aktiv-aber-kein-Verdikt kann nicht auftreten: 
                    // VerifyCategoryAsync liefert für jeden Kandidaten ein Verdikt.)
                    kept.Add(findings[i]);
                    unverifiedCount++;
                }
            }

            var totalSurvived = kept.Count + downgraded.Count;
            var reason = !categoryEnabled
                ? $"{category.Label} für diesen Artefakttyp deaktiviert (DISK-12/B22) — nicht generiert/bewertet."
                : verificationEnabled
                    ? $"{category.Label} (Verifikation aktiv): {confirmedCount} confirmed; "
                      + $"{partialCount} partial (herabgestuft); {rejectedCount} rejected (verworfen)."
                    : $"{category.Label} (Verifikation aus): {unverifiedCount} unverifiziert gezählt.";

            var metric = new NumericMetric(category.MetricName, value: totalSurvived, reason: reason);

            metric.AddOrUpdateMetadata("categoryEnabled", categoryEnabled ? "true" : "false");
            metric.AddOrUpdateMetadata("verificationEnabled", verificationEnabled ? "true" : "false");
            metric.AddOrUpdateMetadata("confirmed", confirmedCount.ToString());
            metric.AddOrUpdateMetadata("partial", partialCount.ToString());
            metric.AddOrUpdateMetadata("rejected", rejectedCount.ToString());
            metric.AddOrUpdateMetadata("unverified", unverifiedCount.ToString());

            foreach (var finding in kept)
                metric.AddDiagnostics(ToDiagnostic(finding.Severity, FormatFinding(finding)));

            allKept.AddRange(kept);

            // Herabgestufte Findings: sichtbar mit neuem Severity-Label und Hinweis auf Original.
            foreach (var (original, effective, evidence) in downgraded)
            {
                metric.AddDiagnostics(ToDiagnostic(effective.Severity,
                    $"HERABGESTUFT [{original.Severity}→{effective.Severity}]"
                    + $" (partial: \"{Truncate(evidence, 160)}\"): {FormatFinding(effective)}"));
                allKept.Add(effective);
            }

            // Verworfene Findings bleiben transparent sichtbar (Evidenz-Disziplin), zählen nicht.
            foreach (var (finding, evidence) in dropped)
                metric.AddDiagnostics(EvaluationDiagnostic.Informational(
                    $"VERWORFEN (rejected: \"{Truncate(evidence, 160)}\"): {FormatFinding(finding)}"));

            metrics.Add(metric);
        }

        var firstMetric = (NumericMetric)metrics[0];

        // Findings mit unbekannter Kategorie nicht verlieren -> sichtbar als Info-Diagnostic.
        for (var i = 0; i < findings.Count; i++)
        {
            if (!matchedIndices.Contains(i))
            {
                firstMetric.AddDiagnostics(EvaluationDiagnostic.Informational(
                    $"[UNZUGEORDNET category='{findings[i].Category}'] {FormatFinding(findings[i])}"));
            }
        }

        if (parseError is not null)
            firstMetric.AddDiagnostics(EvaluationDiagnostic.Error($"Judge-Antwort nicht als JSON geparst: {parseError}"));

        // Deterministischer Gesamt-Score (nur verifizierte Findings) -> Trigger für Phase-3-Repair.
        var errorScore = allKept.Sum(f => SeverityWeight(f.Severity));
        var criticalCount = allKept.Count(f =>
            string.Equals(f.Severity?.Trim(), "KRITISCH", StringComparison.OrdinalIgnoreCase));

        // Parse-Fehler des Haupt-Judge: die Evaluation hat faktisch NICHT stattgefunden. Dann ist errorScore=0 
        // KEIN sauberes Ergebnis, sondern evaluation_failed. needsRepair darf in diesem Fall nicht false sein 
        // (sonst wird ein evtl. fehlerhaftes Artefakt still durchgewunken vgl. Run 20260613_200805_b4fb45,
        //  wo risks.md durch einen Judge-Parsefehler faelschlich errorScore=0 erhielt).
        var evaluationFailed = parseError is not null;
        var evaluationStatus = evaluationFailed ? "failed" : "ok";
        var needsRepair = evaluationFailed || criticalCount > 0 || errorScore >= RepairScoreThreshold;

        var scoreMetric = new NumericMetric(
            ErrorScoreMetricName,
            value: errorScore,
            reason: evaluationFailed
                ? $"EVALUATION_FAILED: Judge-Antwort nicht als JSON parsebar ({parseError}). "
                  + "errorScore ist NICHT aussagekraeftig; needsRepair=true (konservativ, kein stiller Pass)."
                : $"Gewichtete Summe verifizierter Fehler (KRITISCH=3, MITTEL=2, GERING=1) = {errorScore}; "
                  + $"KRITISCH={criticalCount}; needsRepair={needsRepair} (Schwelle {RepairScoreThreshold}). "
                  + "Herabgestufte (partial) Findings zaehlen mit reduzierter Severitygewichtung.");
        scoreMetric.AddOrUpdateMetadata("evaluationStatus", evaluationStatus);
        scoreMetric.AddOrUpdateMetadata("needsRepair", needsRepair ? "true" : "false");
        scoreMetric.AddOrUpdateMetadata("criticalCount", criticalCount.ToString());
        metrics.Add(scoreMetric);

        return new EvaluationResult(metrics.ToArray());
    }

    private static double SeverityWeight(string? severity)
        => severity?.Trim().ToUpperInvariant() switch
        {
            "KRITISCH" => 3,
            "MITTEL" => 2,
            _ => 1
        };

    // KRITISCH→MITTEL, MITTEL→GERING, GERING bleibt GERING.
    // Anwendung: partial-Findings aus dem Verifikations-Pass zählen, aber mit reduziertem Gewicht.
    private static string DowngradeSeverity(string? severity)
        => severity?.Trim().ToUpperInvariant() switch
        {
            "KRITISCH" => "MITTEL",
            "MITTEL" => "GERING",
            _ => "GERING"
        };

    private static string FormatFinding(Finding f)
        => $"[{f.Severity}] Artefakt: \"{f.ArtifactQuote}\" | Transkript: \"{f.TranscriptEvidence}\" | {f.Reason}";

    private static bool MatchesCategory(string? findingCategory, string key)
    {
        if (string.IsNullOrWhiteSpace(findingCategory))
            return false;

        var norm = Normalize(findingCategory);
        var k = Normalize(key);
        return norm == k || norm.Contains(k, StringComparison.Ordinal);
    }

    private static string Normalize(string s)
        => s.Trim().ToUpperInvariant().Replace(' ', '_').Replace('-', '_');

    private static EvaluationDiagnostic ToDiagnostic(string? severity, string message)
        => severity?.Trim().ToUpperInvariant() switch
        {
            "KRITISCH" => EvaluationDiagnostic.Error(message),
            "MITTEL" => EvaluationDiagnostic.Warning(message),
            _ => EvaluationDiagnostic.Informational(message)
        };

    private static (IReadOnlyList<Finding> Findings, string? ParseError) ParseFindings(string? judgeText)
    {
        if (string.IsNullOrWhiteSpace(judgeText))
            return (Array.Empty<Finding>(), "Leere Judge-Antwort.");

        var json = ExtractJsonObject(judgeText);
        if (json is null)
            return (Array.Empty<Finding>(), "Kein JSON-Objekt in der Antwort gefunden.");

        try
        {
            var parsed = JsonSerializer.Deserialize<FindingsEnvelope>(json, JsonOptions);
            return ((IReadOnlyList<Finding>?)parsed?.Findings ?? Array.Empty<Finding>(), null);
        }
        catch (JsonException ex)
        {
            return (Array.Empty<Finding>(), ex.Message);
        }
    }

    // Lenient: schneidet ein evtl. in ```json ... ``` oder Fließtext eingebettetes Objekt heraus.
    private static string? ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
            return null;

        return text.Substring(start, end - start + 1);
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "…";

    /// <summary>
    /// DISK-12/B22: verwirft Findings, deren Kategorie für diesen Artefakttyp deaktiviert ist.
    /// No-op außerhalb des Synthese-Schemas und im Split-Pfad (dort schon übersprungen).. 
    /// greift vorallem im 3-in-1-Pfad, der immer alle 3 Kategorien generiert.
    /// </summary>
    private IReadOnlyList<Finding> FilterEnabledCategories(IReadOnlyList<Finding> findings)
    {
        if (!_verificationActive || findings.Count == 0)
            return findings;

        return findings
            .Where(f => _enabledCategoryKeys.Any(k => MatchesCategory(f.Category, k)))
            .ToList();
    }

    /// <summary>3-in-1-Generierung (Legacy/Nicht-Synthese): ein Call für alle Kategorien.</summary>
    private async Task<(IReadOnlyList<Finding> Findings, string? ParseError)> GenerateSingleAsync(
        string artifactText, IChatClient client, CancellationToken cancellationToken)
    {
        var raw = await RunGenerationCallAsync(_systemPrompt, artifactText, client, cancellationToken)
            .ConfigureAwait(false);
        LastRawJudgeResponse = raw;
        return ParseFindings(raw);
    }

    /// <summary>
    /// DISK-12/G3: pro Kategorie ein fokussierter Generierungs-Call. Jeder Judge sieht nur SEINE
    /// Kategorie → höherer Recall schwacher Modelle (B20). Findings werden mit der INTENDIERTEN
    /// Kategorie getaggt (kein Kategorie-Drift), zusammengeführt und dedupliziert. Ein Parse-Fehler
    /// einer Kategorie macht die Messung unvollständig → wird als parseError gemeldet (kein stiller Pass).
    /// Der nachgelagerte Per-Kategorie-Verifier (DISK-7/8/9) bleibt unverändert für die Precision.
    /// </summary>
    private async Task<(IReadOnlyList<Finding> Findings, string? ParseError)> GenerateSplitAsync(
        string artifactText, IChatClient client, CancellationToken cancellationToken)
    {
        var merged = new List<Finding>();
        var rawParts = new List<string>();
        var failures = new List<string>();

        foreach (var category in SynthesisCategories)
        {
            // DISK-12/B22: für diesen Artefakttyp deaktivierte Kategorie gar nicht erst generieren (kein Call).
            if (!_enabledCategoryKeys.Contains(category.Key))
                continue;

            var systemPrompt = _prompts!.BuildCategoryGenerationPrompt(_artifactName, category.Key);
            var raw = await RunGenerationCallAsync(systemPrompt, artifactText, client, cancellationToken)
                .ConfigureAwait(false);

            _generationRawLogs.Add(new VerifierRawLog(category.Key, 0, 0, string.Empty, raw));
            rawParts.Add($"// GENERATION {category.Key}\n{raw}");

            var (catFindings, catError) = ParseFindings(raw);
            if (catError is not null)
            {
                failures.Add($"{category.Key}: {catError}");
                continue;
            }

            // Mit der intendierten Kategorie taggen (der fokussierte Judge soll nur diese liefern).
            foreach (var f in catFindings)
                merged.Add(f with { Category = category.Key });
        }

        LastRawJudgeResponse = string.Join("\n\n", rawParts);
        var deduped = DedupAndReconcile(merged);
        return (deduped, failures.Count > 0 ? string.Join(" | ", failures) : null);
    }

    /// <summary>Ein Generierungs-Call (System-Prompt + Artefakt/Transkript), strukturierte Ausgabe.</summary>
    private async Task<string> RunGenerationCallAsync(
        string systemPrompt, string artifactText, IChatClient client, CancellationToken cancellationToken)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, BuildUserPrompt(artifactText))
        };

        var options = new ChatOptions { Temperature = 0.0f };
        if (_useStructuredOutput)
            options.ResponseFormat = FindingsResponseFormat;

        var response = await client.GetResponseAsync(messages, options, cancellationToken)
            .ConfigureAwait(false);
        return response.Text ?? string.Empty;
    }

    /// <summary>
    /// DISK-12: konservatives Dedup nach dem Merge der Kategorie-Judges. Entfernt exakte Duplikate
    /// (z. B. dieselbe Zahl zweimal) und löst die FALSE_CLAIM/FALSE_CERTAINTY-Kollision auf: kommt
    /// dieselbe Artefaktstelle in beiden vor, gewinnt FALSE_CERTAINTY (Thema vorhanden → Grenzregel der
    /// Prompts). Bewusst nur EXAKTE Zitat-Übereinstimmung- Overlap bleibt dem Verifier überlassen.
    /// </summary>
    private static IReadOnlyList<Finding> DedupAndReconcile(List<Finding> findings)
    {
        static string QuoteKey(Finding f) => Normalize(f.ArtifactQuote ?? string.Empty);
        static string TopicKey(Finding f) => Normalize(f.TranscriptEvidence ?? string.Empty);

        // FALSE_CERTAINTY hat Vorrang vor FALSE_CLAIM auf derselben Artefaktstelle (außer "nicht vorhanden").
        var certaintyQuotes = findings
            .Where(f => MatchesCategory(f.Category, "FALSE_CERTAINTY"))
            .Select(QuoteKey)
            .Where(k => k.Length > 0 && k != Normalize("nicht vorhanden"))
            .ToHashSet(StringComparer.Ordinal);

        var result = new List<Finding>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var f in findings)
        {
            // MISSING dedupt über das Transkript-Thema (artifactQuote ist immer "nicht vorhanden").
            var isMissing = MatchesCategory(f.Category, "MISSING_TOPIC");
            var key = Normalize(f.Category ?? string.Empty) + "|" + (isMissing ? TopicKey(f) : QuoteKey(f));
            if (!seen.Add(key))
                continue;

            // FALSE_CLAIM verwerfen, wenn dieselbe Stelle bereits als FALSE_CERTAINTY gemeldet ist.
            if (MatchesCategory(f.Category, "FALSE_CLAIM") && certaintyQuotes.Contains(QuoteKey(f)))
                continue;

            result.Add(f);
        }

        return result;
    }

    /// <summary>
    /// Kategoriespezifische Batch-Verifikation (DISK-7): ein LLM-Call prüft alle Kandidaten EINER
    /// Kategorie und liefert pro Kandidat ein Verdikt confirmed / partial / rejected.
    /// </summary>
    /// <remarks>
    /// Kontext je nach Frage:
    ///   FALSE_CLAIM    → Transkript (Grundlage/Widerspruch prüfen),
    ///   MISSING_TOPIC / FALSE_CERTAINTY → Artefakt (Abdeckung / Offenheitsmarkierung prüfen).
    /// FALSE_CERTAINTY läuft zuerst durch den deterministischen Open-Marker-Precheck (K5);
    /// eindeutig hedged-Fälle werden ohne LLM-Call direkt 'rejected' und sparen Tokens.
    /// Konservativ: Parse-Fehler oder fehlende Antworten → betroffene Kandidaten gelten als
    /// 'confirmed' (kein stilles Verwerfen eines evtl. echten Fehlers; DISK-7 K3).
    /// </remarks>
    /// <returns>Dictionary: globaler Finding-Index → Verdikt für JEDEN übergebenen Kandidaten.</returns>
    private async Task<Dictionary<int, FindingVerdict>> VerifyCategoryAsync(
        string categoryKey,
        IReadOnlyList<Finding> findings,
        List<int> candidateIndices,
        string artifactText,
        IChatClient client,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, FindingVerdict>();

        // K5: Deterministischer Precheck NUR für FALSE_CERTAINTY, NUR auf dem artifactQuote.
        var toLlm = new List<int>();
        foreach (var gi in candidateIndices)
        {
            if (categoryKey == "FALSE_CERTAINTY" && QuoteHasOpenMarker(findings[gi].ArtifactQuote))
                result[gi] = new FindingVerdict(
                    "rejected",
                    $"Precheck: Offenheitsmarker im Artefakt-Zitat (\"{Truncate(findings[gi].ArtifactQuote ?? string.Empty, 120)}\").");
            else
                toLlm.Add(gi);
        }

        if (toLlm.Count == 0)
            return result; // alles per Precheck erledigt -> kein LLM-Call

        var systemPrompt = categoryKey switch
        {
            "MISSING_TOPIC" => _prompts!.MissingVerification,
            "FALSE_CERTAINTY" => _prompts!.FalseCertaintyVerification,
            "FALSE_CLAIM" => _prompts!.FalseClaimVerification,
            _ => _prompts!.MissingVerification
        };

        // FALSE_CLAIM braucht das Transkript (Grundlage/Widerspruch); MISSING/FALSE_CERTAINTY das Artefakt.
        var contextBlock = categoryKey == "FALSE_CLAIM"
            ? $"TRANSKRIPT (vollständig):\n{_transcript}"
            : $"ARTEFAKT (vollständig):\n{artifactText}";

        // DISK-9 (M1/F1): NUR MISSING_TOPIC wird auf die konfigurierte Batchgröße gechunkt. Große
        // Kandidatenlisten in einem einzigen Call lassen den Verifier die Trennschärfe verlieren
        // (Run 639979: 23/23 fälschlich confirmed). Andere Kategorien: ein Call (kein Confound).
        var batchSize = categoryKey == "MISSING_TOPIC"
            ? Math.Max(1, _verification.MissingTopicBatchSize)
            : toLlm.Count;

        for (var offset = 0; offset < toLlm.Count; offset += batchSize)
        {
            var chunk = toLlm.GetRange(offset, Math.Min(batchSize, toLlm.Count - offset));
            await RunVerifierAsync(
                    categoryKey, offset / batchSize, systemPrompt, contextBlock,
                    chunk, findings, client, result, cancellationToken)
                .ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Custom-Verifier (DISK-7, <c>jury.verification.custom</c>): EIN ausgelagerter Prompt prüft
    /// ALLE Kandidaten kategorie-übergreifend in einem Batch. Kontext: Transkript UND Artefakt;
    /// KEIN Precheck. Nur zum Experimentieren/Kalibrieren.
    /// </summary>
    private async Task<Dictionary<int, FindingVerdict>> VerifyCustomAsync(
        IReadOnlyList<Finding> findings,
        List<int> candidateIndices,
        string artifactText,
        IChatClient client,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, FindingVerdict>();
        var contextBlock =
            $"TRANSKRIPT (vollständig):\n{_transcript}\n\nARTEFAKT (vollständig):\n{artifactText}";

        await RunVerifierAsync(
                "CUSTOM", 0, _prompts!.CustomVerification, contextBlock,
                candidateIndices, findings, client, result, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Gemeinsamer LLM-Verifikations-Call: rendert die Kandidaten als nummerierten Batch, ruft den
    /// Judge mit System-Prompt + Kontext auf und trägt die Verdikte in <paramref name="result"/> ein.
    /// Nicht beantwortete / parse-fehlerhafte Kandidaten → konservativ 'confirmed' (DISK-7 K3).
    /// </summary>
    private async Task RunVerifierAsync(
        string categoryKey,
        int chunkIndex,
        string systemPrompt,
        string contextBlock,
        List<int> indices,
        IReadOnlyList<Finding> findings,
        IChatClient client,
        Dictionary<int, FindingVerdict> result,
        CancellationToken cancellationToken)
    {
        if (indices.Count == 0)
            return;

        var candidates = string.Join("\n", indices.Select((gi, k) =>
        {
            var f = findings[gi];
            return $"{k}: ARTEFAKT: \"{f.ArtifactQuote}\" | TRANSKRIPT: \"{f.TranscriptEvidence}\" | GRUND: {f.Reason}";
        }));

        var messages = new[]
        {
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, $"{contextBlock}\n\nZU PRÜFENDE BEFUNDE:\n{candidates}")
        };

        var verifyOptions = new ChatOptions { Temperature = 0.0f };
        if (_useStructuredOutput)
            verifyOptions.ResponseFormat = VerificationResponseFormat;

        try
        {
            var response = await client
                .GetResponseAsync(messages, verifyOptions, cancellationToken)
                .ConfigureAwait(false);

            // DISK-9 (M4/F6): Rohantwort + gerenderten Kandidaten-Batch für spätere Kalibrierung sichern.
            _verifierRawLogs.Add(new VerifierRawLog(categoryKey, chunkIndex, indices.Count, candidates, response.Text));

            var json = ExtractJsonObject(response.Text);
            if (json is not null)
            {
                var parsed = JsonSerializer.Deserialize<VerificationEnvelope>(json, JsonOptions);
                if (parsed?.Results is not null)
                {
                    foreach (var r in parsed.Results)
                    {
                        if (r.Index < 0 || r.Index >= indices.Count)
                            continue;
                        result[indices[r.Index]] = new FindingVerdict(
                            NormalizeVerdict(r.Verdict),
                            r.Evidence ?? string.Empty);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Konservativ: Parse-Fehler -> betroffene Kandidaten bleiben unbeantwortet -> unten 'confirmed'.
        }

        // Jeder nicht beantwortete Kandidat -> konservativ 'confirmed' (kein stiller Pass).
        foreach (var gi in indices)
            if (!result.ContainsKey(gi))
                result[gi] = new FindingVerdict("confirmed", "Verifikation ohne Verdikt -> konservativ confirmed.");
    }

    // Vereinheitlicht beliebige Verifier-Ausgaben auf confirmed/partial/rejected.
    // DISK-9 (M5/F2): MISSING denkt jetzt in covered/partial/absent (entschärft die "confirmed =
    // absent"-Kollision, die nach sicherem Default klang). Mapping: covered→rejected, absent→confirmed.
    // Altes MISSING-Vokabular (sufficient/none) bleibt für Robustheit gemappt.
    private static string NormalizeVerdict(string? verdict)
        => verdict?.Trim().ToLowerInvariant() switch
        {
            "rejected" => "rejected",
            "sufficient" => "rejected",
            "covered" => "rejected",
            "partial" => "partial",
            "confirmed" => "confirmed",
            "none" => "confirmed",
            "absent" => "confirmed",
            _ => "confirmed"
        };

    // K5: prüft NUR den artifactQuote (= konkrete Claim-Stelle) auf Offenheitsmarker.
    private static bool QuoteHasOpenMarker(string? quote)
    {
        if (string.IsNullOrWhiteSpace(quote))
            return false;

        if (OpenMarkerWordRegex.IsMatch(quote))
            return true;

        var lower = quote.ToLowerInvariant();
        foreach (var phrase in OpenMarkerPhrases)
            if (lower.Contains(phrase, StringComparison.Ordinal))
                return true;

        return false;
    }

    private string BuildUserPrompt(string artifactText) => $$"""
        TRANSKRIPT (Referenz):
        {{_transcript}}

        ZU PRÜFENDES ARTEFAKT ({{_artifactName}}):
        {{artifactText}}
        """;

    private static (IReadOnlyList<EvaluationCategory> Categories, string SystemPrompt) Configure(EvaluationScheme scheme)
        => scheme switch
        {
            EvaluationScheme.ThreeCategory => (
                new List<EvaluationCategory>
                {
                    new("1", "FalseClaim", "Kat.1 Falsche Behauptung"),
                    new("2", "FalseCertainty", "Kat.2 Falsche Sicherheit"),
                    new("3", "MissingTopic", "Kat.3 Fehlendes Thema")
                },
                ThreeCategoryPrompt + JsonFormatBlock),

            EvaluationScheme.TwoCategory => (
                new List<EvaluationCategory>
                {
                    new("NICHT_GEDECKT", "Unsupported", "Nicht gedeckt"),
                    new("FEHLT", "Missing", "Fehlt")
                },
                TwoCategoryPrompt + JsonFormatBlock),

            EvaluationScheme.PhasedFour => (
                new List<EvaluationCategory>
                {
                    new("FALSE_CLAIM", "FalseClaim", "FALSE_CLAIM"),
                    new("FALSE_CERTAINTY", "FalseCertainty", "FALSE_CERTAINTY"),
                    new("MISSING_TOPIC", "MissingTopic", "MISSING_TOPIC"),
                    new("INCOMPLETE_COVERAGE", "IncompleteCoverage", "INCOMPLETE_COVERAGE")
                },
                // Eigener, vollständiger Prompt inkl. eigenem Ausgabeformat -> KEIN JsonFormatBlock anhängen.
                PhasedRequirementsPrompt),

            _ => (
                new List<EvaluationCategory> { new("FEHLER", "Error", "Fehler") },
                SingleListPrompt + JsonFormatBlock)
        };

    private const string ThreeCategoryPrompt = """
        Du bist Qualitätsprüfer für SDLC-Artefakte aus einem Stakeholder-Meeting.
        Vergleiche jede wesentliche Aussage des Artefakts gegen das Transkript und finde
        Fehler in drei Kategorien:

        category "1" = FALSCHE BEHAUPTUNG: Aussage widerspricht dem Transkript oder kommt
          darin gar nicht vor (erfunden). Nur sachliche Falschheit.
        category "2" = FALSCHE SICHERHEIT: Aussage wird als entschieden/gesetzt dargestellt,
          ist im Transkript aber offen, strittig oder unentschieden.
        category "3" = FEHLENDES THEMA: ein für das Artefakt wichtiges Transkript-Thema fehlt
          im Artefakt vollständig (artifactQuote = "nicht vorhanden").

        Jeder Fehler in GENAU EINE Kategorie. Keine Doppelzählung. Nur substanzielle Fehler.
        Antworte NUR mit JSON im unten genannten Format.
        """;

    private const string TwoCategoryPrompt = """
        Du bist Qualitätsprüfer für SDLC-Artefakte aus einem Stakeholder-Meeting.
        Vergleiche das Artefakt gegen das Transkript und finde Fehler in zwei Kategorien:

        category "NICHT_GEDECKT" = eine Aussage STEHT im Artefakt, ist aber durch das
          Transkript nicht gedeckt: erfunden ODER als entschieden dargestellt, obwohl im
          Transkript offen/strittig.
        category "FEHLT" = ein wichtiges Transkript-Thema FEHLT im Artefakt vollständig
          (artifactQuote = "nicht vorhanden").

        Die Unterscheidung ist mechanisch: Steht die Aussage IM Artefakt -> NICHT_GEDECKT;
        fehlt sie -> FEHLT. Jeder Fehler in GENAU EINE Kategorie. Nur substanzielle Fehler.
        Antworte NUR mit JSON im unten genannten Format.
        """;

    private const string SingleListPrompt = """
        Du bist Qualitätsprüfer für SDLC-Artefakte aus einem Stakeholder-Meeting.
        Vergleiche das Artefakt gegen das Transkript und liste ALLE substanziellen Fehler:
        Aussagen, die durch das Transkript nicht gedeckt sind (erfunden oder als entschieden
        dargestellt, obwohl offen), sowie wichtige Transkript-Themen, die im Artefakt fehlen.

        Setze category immer auf "FEHLER". Nur substanzielle Fehler, keine Stil-Nitpicks.
        Antworte NUR mit JSON im unten genannten Format.
        """;

    private const string PhasedRequirementsPrompt = """
        Du bist Qualitätsprüfer für SDLC-Requirements-Artefakte aus einem Stakeholder-Meeting.

        Du erhältst:
        * das originale Stakeholder-Transkript (Ground Truth)
        * ein daraus erzeugtes requirements.md

        Deine Aufgabe ist ausschließlich die Qualitätsprüfung des Requirements-Artefakts.
        Du darfst keine Anforderungen ergänzen, verbessern oder umschreiben.
        Du darfst ausschließlich Fehler identifizieren.

        ## ZIEL DES ARTEFAKTS
        Das Artefakt soll die im Stakeholder-Meeting besprochenen Anforderungen korrekt und
        nachvollziehbar dokumentieren. Besonders relevant: Functional/Non-functional Requirements,
        Rollen und Berechtigungen, Compliance, Datenschutz, Sicherheit, Integrationen,
        Schnittstellen, Betriebsanforderungen, Constraints, Annahmen, offene Punkte mit Einfluss
        auf Anforderungen.

        ## PHASE 1 – THEMEN IDENTIFIZIEREN
        Identifiziere wichtige Anforderungen/Themen aus dem Transkript (z. B. fachliche
        Anforderungen, Rollen, Workflows, Integrationen, Security, Datenschutz, Compliance,
        Reporting, Exportfunktionen, Betriebsanforderungen, Skalierung, Monitoring, Rate Limiting,
        Audit Logging). Verwende ausschließlich Themen, die tatsächlich im Transkript vorkommen.

        ## PHASE 2 – GESAMTES ARTEFAKT DURCHSUCHEN
        Für jedes relevante Thema musst du das GESAMTE Requirements-Artefakt durchsuchen.
        Ein Thema gilt als VORHANDEN, wenn es irgendwo erwähnt wird. Dazu zählen ausdrücklich:
        Functional/Non-functional Requirements, Constraints, Assumptions, Open Questions,
        Known Limitations, Future Considerations, Traceability, Anhänge.
        Ein Thema gilt NICHT als fehlend, nur weil: es nicht im Hauptteil steht, als offene Frage
        dokumentiert wurde, als Annahme dokumentiert wurde, als Risiko beschrieben wurde, noch
        nicht final entschieden wurde oder nur kurz beschrieben wurde.

        ## PHASE 3 – STATUS BESTIMMEN
        Bestimme für jedes Thema genau einen Status:
        PRESENT  = Thema wird im Artefakt erwähnt.
        PARTIAL  = Thema wird erwähnt, aber wesentliche Aspekte fehlen.
        ABSENT   = Thema kommt im gesamten Artefakt überhaupt nicht vor.

        ## PHASE 4 – KLASSIFIKATION
        Erst nach der Statusbestimmung darf ein Finding erzeugt werden.

        ## FALSE_CLAIM
        Nur wenn eine Aussage im Artefakt dem Transkript widerspricht ODER im Transkript nicht
        vorkommt (erfundene Anforderungen/Zahlen/Technologien/Rollen/Integrationen).

        ## FALSE_CERTAINTY
        Nur wenn das Artefakt etwas als entschieden darstellt UND das Transkript dieselbe Sache als
        offen, strittig oder unentschieden beschreibt. Der Inhalt kann korrekt sein; der Fehler liegt
        ausschließlich in der Sicherheit der Darstellung.

        ## MISSING_TOPIC
        Nur wenn der Status ABSENT ist. Vor MISSING_TOPIC musst du prüfen:
        1. Kommt das Thema irgendwo im Artefakt vor?
        2. Kommt es in Assumptions vor?
        3. Kommt es in Open Questions vor?
        4. Kommt es in Constraints vor?
        5. Kommt es in Traceability vor?
        Wenn EINE dieser Fragen mit JA beantwortet wird: MISSING_TOPIC ist VERBOTEN.
        artifactQuote muss "nicht vorhanden" sein.

        ## INCOMPLETE_COVERAGE
        Nur wenn der Status PARTIAL ist: das Thema ist vorhanden, aber wesentliche Aspekte fehlen
        (z. B. Security erwähnt, aber wichtige Security-Anforderungen fehlen; Rollen erwähnt, aber
        wichtige Rollen fehlen). Wenn das Thema vorhanden ist, darf NIEMALS MISSING_TOPIC verwendet werden.

        ## SCHWEREGRAD
        KRITISCH = Kernanforderung/Compliance/Datenschutz/Security betroffen oder führt zu falschen
        Folgeentscheidungen. MITTEL = fachlich relevant, beeinflusst Planung/Verständnis.
        GERING = kleinere fachliche Ungenauigkeit, geringe Auswirkung.

        ## STRIKTE REGELN
        Jeder Fehler genau eine Kategorie. Keine Doppelzählungen. Keine Stil-/Format-/Sprachkritik.
        Keine Verbesserungsvorschläge. Keine Vollständigkeitswünsche ohne Transkriptbeleg. Jede
        Feststellung muss durch Transkript UND Artefakt belegbar sein. Vor MISSING_TOPIC muss das
        gesamte Artefakt durchsucht werden.

        ## AUSGABEFORMAT
        Antworte ausschließlich mit gültigem JSON:
        {
          "findings": [
            {
              "category": "FALSE_CLAIM | FALSE_CERTAINTY | MISSING_TOPIC | INCOMPLETE_COVERAGE",
              "severity": "KRITISCH | MITTEL | GERING",
              "artifactQuote": "woertliches Zitat aus dem Artefakt oder 'nicht vorhanden'",
              "transcriptEvidence": "woertliches Zitat aus dem Transkript oder 'nicht vorhanden'",
              "reason": "ein Satz, warum dies ein Fehler ist"
            }
          ]
        }
        Keine Fehler: {"findings": []}. Gib niemals Text außerhalb des JSON-Objekts aus.
        """;

    private static readonly IReadOnlyList<EvaluationCategory> SynthesisCategories = new List<EvaluationCategory>
    {
        new("FALSE_CLAIM", "FalseClaim", "FALSE_CLAIM"),
        new("FALSE_CERTAINTY", "FalseCertainty", "FALSE_CERTAINTY"),
        new("MISSING_TOPIC", "MissingTopic", "MISSING_TOPIC")
    };

    // Hinweis: Die Synthese-Prompts (Header / Profile / Body) und die drei Verifier-Prompts sind
    // ausgelagert nach AgenticSdlc.Host/Prompts/jury/*.txt und werden über JuryPrompts/JuryPromptLoader
    // geladen (DISK-7). Hier verbleiben nur die Legacy-Schema-Prompts (ThreeCategory/Two/Single/Phased)
    // als historische Kalibrierungsvarianten, die NICHT vom aktiven Jury-Pfad genutzt werden.

    // Gemeinsamer Format-Block (an jeden Schema-Prompt angehängt).
    private const string JsonFormatBlock = """

        Schwere je Fehler: KRITISCH (Kernanforderung / falsche Grundlage), MITTEL (relevant),
        GERING (kleine Ungenauigkeit). Gib KEINE Gesamtnote.

        Format (NUR dieses JSON-Objekt, kein Text drumherum):
        {
          "findings": [
            {
              "category": "<einer der oben genannten category-Werte>",
              "severity": "KRITISCH | MITTEL | GERING",
              "artifactQuote": "woertliches Zitat aus dem Artefakt (oder 'nicht vorhanden')",
              "transcriptEvidence": "woertliches Zitat aus dem Transkript (oder 'nicht vorhanden')",
              "reason": "ein Satz, warum das ein Fehler ist"
            }
          ]
        }
        Kein Fehler gefunden: {"findings": []}.
        """;

    private sealed record VerificationEnvelope(
        [property: JsonPropertyName("results")] List<VerificationResult>? Results);

    private sealed record VerificationResult(
        [property: JsonPropertyName("index")] int Index,
        [property: JsonPropertyName("verdict")] string? Verdict,
        [property: JsonPropertyName("evidence")] string? Evidence);

    // Trägt das vereinheitlichte Verifikations-Ergebnis: Verdikt + belegendes Zitat.
    private sealed record FindingVerdict(string Verdict, string Evidence);

    private sealed record Finding(
        [property: JsonPropertyName("category")] string? Category,
        [property: JsonPropertyName("severity")] string? Severity,
        [property: JsonPropertyName("artifactQuote")] string? ArtifactQuote,
        [property: JsonPropertyName("transcriptEvidence")] string? TranscriptEvidence,
        [property: JsonPropertyName("reason")] string? Reason);

    private sealed record FindingsEnvelope(
        [property: JsonPropertyName("findings")] List<Finding>? Findings);
}
