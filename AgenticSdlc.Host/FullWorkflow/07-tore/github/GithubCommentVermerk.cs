using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// C2d §3-5 (09.08.2026) — der ABSCHLUSS-VERMERK: erst NACH dem Wahrheits-Entscheid (gated Apply) bekommt
/// das Ursprungs-Issue eine Antwort — „✔ eingepflegt als …" bzw. „✕ bewusst nicht übernommen, weil …".
/// Abgeleitet wird DETERMINISTISCH aus dem Lauf-Report (R-40) + Delta (Kommentar-Herkunft) +
/// human-decisions (Ablehnungs-Begründungen) — nie aus dem Destillat selbst (GitHub bestätigt nie Unwahres).
/// Die Vermerke werden als NOTE_COMMENT-Ops in den Forward-Plan geseedet: gated + execute-policy wie jeder
/// GitHub-Write. Ein Vermerk je Issue (mehrere Aussagen = mehrere Zeilen im selben Kommentar).
/// Zwei-Bahnen-Grenze: nur Läufe mit diesen Artefakten (Ein-Graph-Faden) erzeugen Vermerke — die
/// Standalone-Forward-Bahn hat keinen Ingest-Report und bleibt vermerk-frei.
/// </summary>
public static class GithubCommentVermerk
{
    // Echo-Schutz (Autor-Fund 20.08. spät, Lauf 165512: das System destillierte seine EIGENEN
    // Vermerk-Kommentare als Ernte-Beute — GH-7/GH-12 waren wörtliche Zitate aus „✔ Eingepflegt…").
    // EINE Marker-Definition: der Vermerk-Body beginnt mit diesem Präfix, und der Kommentar-Collector
    // schließt genau daran deterministisch aus. Legacy = Alt-Vermerke vom 13./17.08.
    public const string BodyMarker = "**Verarbeitungs-Vermerk**";
    public const string LegacyBodyMarker = "**Kommentar-Verarbeitung**";

    public static bool IsSystemVermerk(string? commentBody)
        => commentBody is not null
           && (commentBody.StartsWith(BodyMarker, StringComparison.Ordinal)
               || commentBody.StartsWith(LegacyBodyMarker, StringComparison.Ordinal));

    public static IReadOnlyList<GithubForwardOp> Derive(
        ProjectStateDocument meetingDelta,
        IngestionApplyReport report,
        IReadOnlyList<IngestionHumanDecision> decisions,
        string runId)
    {
        var appliedByIncoming = report.Applied
            .GroupBy(a => a.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var reasonByIncoming = decisions
            .Where(d => !string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .GroupBy(d => d.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Reason ?? "", StringComparer.Ordinal);

        // Projektions-Nachzug ② (Autor-⚖ 20.08.): der Vermerk gilt für JEDE GitHub-Herkunft — nicht
        // mehr nur Kommentar-Destillate, auch geerntete Body-Edits (Beleg REJ-012/#45: die Ablehnung
        // blieb für den Editierenden stumm). Kommentare überleben den Reproject (der schreibt nur
        // den Body) — die Historie Vorschlag→Prüfung→Entscheid bleibt damit dauerhaft im Thread.
        return meetingDelta.Items
            .Where(i => GithubOriginMeta.IssueNumberOf(i) is not null)
            .GroupBy(i => GithubOriginMeta.IssueNumberOf(i)!.Value)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var lines = new List<string>();
                string? subject = null;
                foreach (var item in g.OrderBy(i => i.ItemId, StringComparer.Ordinal))
                {
                    if (appliedByIncoming.TryGetValue(item.ItemId, out var applied))
                    {
                        subject ??= applied.EntityId;
                        lines.Add($"✔ Eingepflegt in die Projektwahrheit: {applied.EntityId} ({applied.Kind}) — „{Trunc(item.Text)}“");
                    }
                    else if (reasonByIncoming.TryGetValue(item.ItemId, out var reason))
                    {
                        lines.Add($"✕ Bewusst nicht übernommen — Begründung: {reason} — „{Trunc(item.Text)}“");
                    }
                    // weder applied noch begruendet abgelehnt (z. B. Experiment-Modus ohne decisions-Datei):
                    // KEIN Vermerk fuer dieses Item — GitHub bestaetigt nie Unbelegtes.
                }
                if (lines.Count == 0) return null;
                var body = BodyMarker + " (Lauf `" + runId + "`):\n\n" + string.Join("\n", lines);
                return new GithubForwardOp(
                    GithubForwardKind.NoteComment, subject ?? $"gh#{g.Key}", g.Key, null, body, null, null, null,
                    Anchor: $"comment-distill {runId} -> gh#{g.Key}",
                    Rationale: "C2d-Abschluss-Vermerk: Diskussions-Strang hoerbar geschlossen (nach gated Apply).",
                    Origin: "deterministic");
            })
            .Where(op => op is not null)
            .Select(op => op!)
            .ToList();
    }

    /// <summary>Datei-Haut für den Forward-Seed im Ein-Graph: lädt die Lauf-Artefakte, wenn vorhanden —
    /// sonst leer (Standalone-Forward-Bahn, dokumentierte Grenze).</summary>
    public static IReadOnlyList<GithubForwardOp> TryDeriveFromRun(string runDir, string runId)
    {
        var deltaPath = Path.Combine(runDir, "04-delta", "project-state.json");
        if (!File.Exists(deltaPath)) return [];
        try
        {
            var delta = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(deltaPath), ProjectStateJson.Options);
            if (delta is null) return [];
            // ② (20.08.): BEIDE Aspekt-Strips liefern Vermerk-Stoff — der arch-Strip (geerntete
            // Rahmen-Edits!) war vorher unsichtbar; Reports und Entscheide werden gemerged.
            var reports = new List<IngestionApplyReport>();
            var decisions = new List<IngestionHumanDecision>();
            foreach (var strip in new[] { "07-ingest", "07-arch-ingest" })
            {
                var report = ReadJson<IngestionApplyReport>(Path.Combine(runDir, strip, "applied", "run-report.json"));
                if (report is null) continue;
                reports.Add(report);
                // Entscheide je Bahn: UI schreibt human-decisions.json, der Chat-Weg ingest-gate-decisions.json —
                // beide tragen dieselbe Form (incomingItemId/decision/reason).
                foreach (var file in new[] { "human-decisions.json", "ingest-gate-decisions.json" })
                    if (ReadJson<IngestionHumanDecisionsFile>(Path.Combine(runDir, strip, file)) is { } f)
                        decisions.AddRange(f.Decisions);
            }
            if (reports.Count == 0) return [];
            var merged = reports[0] with
            {
                Applied = [.. reports.SelectMany(r => r.Applied)],
                Skipped = [.. reports.SelectMany(r => r.Skipped)]
            };
            return Derive(delta, merged, decisions, runId);
        }
        catch (Exception ex)
        {
            // LAUT statt still: kaputte Artefakte kosten den Vermerk, nie den Lauf.
            Console.Error.WriteLine($"[github-forward] C2d-Vermerk-Ableitung fehlgeschlagen (kein Vermerk): {ex.Message}");
            return [];
        }
    }

    private static T? ReadJson<T>(string path) where T : class
        => File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), FullWorkflow.JsonFiles.Json) : null;

    private static string Trunc(string s) => s.Length <= 100 ? s : s[..100] + "…";
}
