using System.Text.Json;
using Microsoft.Extensions.AI.Evaluation;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Gemeinsame Ausgabe-/Auswertungslogik für den <see cref="Evaluator"/>, geteilt zwischen der
/// Auto-Jury (<see cref="Phase2JuryRunner"/>) und dem Offline-Evaluator (<see cref="OfflineEvaluatorRunner"/>).
/// </summary>
/// <remarks>
/// Vor der Zentralisierung hatten beide Runner eigene Kopien der Summary-/Score-Logik — bei Änderungen
/// (B8 evaluation_failed, B9 strukturierte Ausgabe) driftete der Offline-Pfad. Diese Klasse ist die
/// EINZIGE Stelle für: Kategorien-Block, Score-Auslese (inkl. evaluationStatus/needsRepair) und
/// Roh-Antwort-Persistenz bei Parse-Fehlern.
/// </remarks>
internal static class EvaluatorOutput
{
    /// <summary>Baut den Kategorien-Block (Counts, Reason, Findings als Diagnostics) für die Summary-JSON.</summary>
    public static List<object> BuildCategories(Evaluator evaluator, EvaluationResult result)
    {
        var cats = new List<object>();
        foreach (var c in evaluator.Categories)
        {
            var metric = result.Get<NumericMetric>(c.MetricName);
            var diagnostics = metric.Diagnostics ?? Enumerable.Empty<EvaluationDiagnostic>();

            cats.Add(new
            {
                key = c.Key,
                label = c.Label,
                metric = c.MetricName,
                count = metric.Value,
                reason = metric.Reason,
                // DISK-7: pro Kategorie ausweisen, was der Verifikations-Pass gemacht hat.
                verification = new
                {
                    enabled = ReadMeta(metric, "verificationEnabled") == "true",
                    confirmed = ReadMetaInt(metric, "confirmed"),
                    partial = ReadMetaInt(metric, "partial"),
                    rejected = ReadMetaInt(metric, "rejected"),
                    unverified = ReadMetaInt(metric, "unverified")
                },
                findings = diagnostics
                    .Select(d => new { severity = d.Severity.ToString(), message = d.Message })
                    .ToList()
            });
        }

        return cats;
    }

    private static string? ReadMeta(NumericMetric metric, string key)
        => metric.Metadata?.TryGetValue(key, out var v) == true ? v : null;

    private static int ReadMetaInt(NumericMetric metric, string key)
        => int.TryParse(ReadMeta(metric, key), out var n) ? n : 0;

    /// <summary>Liest die ErrorScore-Metrik samt Status-Metadaten (B8) aus.</summary>
    public static EvaluatorScore ReadScore(EvaluationResult result)
    {
        var m = result.Get<NumericMetric>(Evaluator.ErrorScoreMetricName);
        var status = m.Metadata?.TryGetValue("evaluationStatus", out var es) == true && es is not null ? es : "ok";
        var needsRepair = m.Metadata?.TryGetValue("needsRepair", out var nr) == true
                          && string.Equals(nr, "true", StringComparison.OrdinalIgnoreCase);
        return new EvaluatorScore(m.Value, status, needsRepair, m.Reason);
    }

    /// <summary>
    /// Persistiert die rohe Judge-Antwort als <c>&lt;fileBase&gt;.rawfail.txt</c>, falls die Evaluation
    /// fehlschlug (B8). Sonst No-op.
    /// </summary>
    public static async Task WriteRawFailIfNeededAsync(
        string juryDir, string fileBase, Evaluator evaluator, CancellationToken cancellationToken)
    {
        if (!evaluator.LastEvaluationFailed)
            return;

        var rawFile = Path.Combine(juryDir, $"{fileBase}.rawfail.txt");
        await File.WriteAllTextAsync(
            rawFile,
            $"// EVALUATION_FAILED parseError={evaluator.LastParseError}\n\n{evaluator.LastRawJudgeResponse}",
            cancellationToken)
            .ConfigureAwait(false);
    }

    private static readonly JsonSerializerOptions RawLogJsonOptions = new() { WriteIndented = true };

    // DISK-12: löscht vorhandene Rohlog-Dateien zu einem fileBase, damit ein Re-Score keine stale
    // Dateien (z. B. einer nun deaktivierten Kategorie) zurücklässt. Best effort.
    private static void DeleteStale(string juryDir, string searchPattern)
    {
        if (!Directory.Exists(juryDir))
            return;
        foreach (var file in Directory.GetFiles(juryDir, searchPattern))
        {
            try { File.Delete(file); } catch (IOException) { /* best effort */ }
        }
    }

    /// <summary>
    /// DISK-9 (M4/F6): Persistiert die rohen Verifier-Antworten pro Kategorie als
    /// <c>.raw.json</c> (eine Datei je Kategorie, alle Chunks).
    /// Macht direkt prüfbar, ob ein Verifier-Fehler vom LLM, Prompt, Mapping oder von der Batchgröße
    /// kam. No-op, wenn keine Verifikation lief.
    /// </summary>
    public static async Task WriteVerifierRawLogsAsync(
        string juryDir, string fileBase, Evaluator evaluator, CancellationToken cancellationToken)
    {
        // DISK-12: stale Rohlogs eines früheren Re-Scores entfernen (zB wenn eine Kategorie jetzt deaktiviert ist),
        //  damit die Dateien den AKTUELLEN Lauf widerspiegeln.
        DeleteStale(juryDir, $"{fileBase}.verify-*.raw.json");

        foreach (var byCategory in evaluator.LastVerifierRawLogs.GroupBy(l => l.Category))
        {
            var slug = byCategory.Key.ToLowerInvariant().Replace('_', '-');
            var payload = new
            {
                category = byCategory.Key,
                chunks = byCategory
                    .OrderBy(l => l.ChunkIndex)
                    .Select(l => new
                    {
                        chunkIndex = l.ChunkIndex,
                        candidateCount = l.CandidateCount,
                        renderedCandidates = l.RenderedCandidates,
                        rawResponse = l.RawResponse
                    })
                    .ToList()
            };

            var outFile = Path.Combine(juryDir, $"{fileBase}.verify-{slug}.raw.json");
            await File.WriteAllTextAsync(
                outFile, JsonSerializer.Serialize(payload, RawLogJsonOptions), cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// DISK-12/G3: Persistiert die rohen Generierungs-Antworten pro Kategorie (nur im Split-Modus belegt). 
    /// Macht den Call-1-Recall pro Kategorie direkt prüfbar. No-op im 3-in-1-Modus (Liste leer).
    /// </summary>
    public static async Task WriteGenerationRawLogsAsync(
        string juryDir, string fileBase, Evaluator evaluator, CancellationToken cancellationToken)
    {
        // DISK-12: stale Generierungs-Rohlogs eines früheren Re-Scores entfernen (z. B. deaktivierte
        // oder im 3-in-1-Modus nicht erzeugte Kategorie), damit nichts Irreführendes liegen bleibt.
        DeleteStale(juryDir, $"{fileBase}.generate-*.raw.json");

        foreach (var log in evaluator.LastGenerationRawLogs)
        {
            var slug = log.Category.ToLowerInvariant().Replace('_', '-');
            var payload = new { category = log.Category, rawResponse = log.RawResponse };
            var outFile = Path.Combine(juryDir, $"{fileBase}.generate-{slug}.raw.json");
            await File.WriteAllTextAsync(
                outFile, JsonSerializer.Serialize(payload, RawLogJsonOptions), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}

/// <summary>Aufbereitete ErrorScore-Metrik: Wert + Status (ok/failed) + Repair-Flag + Begründung.</summary>
internal sealed record EvaluatorScore(double? ErrorScore, string EvaluationStatus, bool NeedsRepair, string? Reason);
