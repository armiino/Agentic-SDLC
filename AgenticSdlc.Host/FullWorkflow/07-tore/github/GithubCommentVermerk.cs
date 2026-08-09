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

        return meetingDelta.Items
            .Where(i => i.Metadata.ContainsKey(GithubCommentMeta.AnchorKey) && GithubOriginMeta.IssueNumberOf(i) is not null)
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
                var body = "**Kommentar-Verarbeitung** (Lauf `" + runId + "`):\n\n" + string.Join("\n", lines);
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
        var json = FullWorkflow.JsonFiles.Json;
        var reportPath = Path.Combine(runDir, "07-ingest", "applied", "run-report.json");
        var deltaPath = Path.Combine(runDir, "04-delta", "project-state.json");
        if (!File.Exists(reportPath) || !File.Exists(deltaPath)) return [];
        try
        {
            var report = JsonSerializer.Deserialize<IngestionApplyReport>(File.ReadAllText(reportPath), json);
            var delta = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(deltaPath), ProjectStateJson.Options);
            if (report is null || delta is null) return [];
            var decisionsPath = Path.Combine(runDir, "07-ingest", "human-decisions.json");
            var decisions = File.Exists(decisionsPath)
                ? JsonSerializer.Deserialize<IngestionHumanDecisionsFile>(File.ReadAllText(decisionsPath), json)?.Decisions ?? []
                : [];
            return Derive(delta, report, decisions, runId);
        }
        catch (Exception ex)
        {
            // LAUT statt still: kaputte Artefakte kosten den Vermerk, nie den Lauf.
            Console.Error.WriteLine($"[github-forward] C2d-Vermerk-Ableitung fehlgeschlagen (kein Vermerk): {ex.Message}");
            return [];
        }
    }

    private static string Trunc(string s) => s.Length <= 100 ? s : s[..100] + "…";
}
