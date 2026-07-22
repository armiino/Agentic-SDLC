using System.Text;
using AgenticSdlc.Host.FullWorkflow.Derivation;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// Baut die USER-Nachrichten (Umwelt-Kontext) für die L3-Agenten. Die Persona/Instruktion kommt getrennt aus den
/// Prompt-Dateien (PromptProvider) — hier nur der lauf-spezifische Umwelt-Graph. Analog zu
/// <see cref="DerivationGenerateExecutor"/>.
/// </summary>
internal static class L3Prompts
{
    /// <summary>Umwelt-Items je Artefakt gruppiert (mit stabiler ItemId), damit der Agent sieht, worauf er verankern kann.</summary>
    public static string BuildEnvUser(SourceArtifactSet env, bool forGeneration)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"PROJEKT-UMWELT ({env.TotalItemCount} Items aus {env.Sources.Count} Artefakt(en)):");
        foreach (var src in env.Sources)
        {
            sb.AppendLine();
            sb.AppendLine($"[{src.ArtifactType}] ({src.Items.Count} Items):");
            foreach (var it in src.Items) sb.Append("- ").Append(it.ItemId).Append(": ").AppendLine(it.Text);
        }
        if (forGeneration)
        {
            sb.AppendLine();
            sb.AppendLine("Erzeuge NEUE Kandidaten-Items, die das Projekt plausibel erweitern (noch OHNE Anker).");
        }
        return sb.ToString();
    }

    /// <summary>Umwelt + die zu verankernden Kandidaten (mit CandidateId), für die Anchor-Resolution-Phase.</summary>
    public static string BuildResolveUser(SourceArtifactSet env, IReadOnlyList<L3Candidate> candidates)
    {
        var sb = new StringBuilder(BuildEnvUser(env, forGeneration: false));
        sb.AppendLine();
        sb.AppendLine($"ZU VERANKERNDE KANDIDATEN ({candidates.Count}) — suche je Kandidat mögliche Anker aus der Umwelt (mit Beziehung + Begründung) ODER begründe, dass keiner trägt:");
        foreach (var c in candidates)
        {
            sb.Append("- ").Append(c.CandidateId).Append(": ").AppendLine(c.Text);
            if (!string.IsNullOrWhiteSpace(c.Rationale)) sb.Append("    (begründung: ").Append(c.Rationale).AppendLine(")");
        }
        return sb.ToString();
    }

    /// <summary>Repair-Pass (Schritt 4): Umwelt + bereits erzeugte Kandidaten + die noch UNBEHANDELTEN Linsen (mit
    /// Repair-Hinweis). Der Agent darf eine Linse begründet leer lassen (echtes N/A, kein Füllstoff).</summary>
    public static string BuildCoverageRepairUser(SourceArtifactSet env, IReadOnlyList<L3Candidate> prior, IReadOnlyList<CoverageLens> missing)
    {
        var sb = new StringBuilder(BuildEnvUser(env, forGeneration: false));
        sb.AppendLine();
        sb.AppendLine($"DU HAST BEREITS {prior.Count} KANDIDATEN ERZEUGT (gapCategory: Text):");
        foreach (var c in prior) sb.Append("- ").Append(c.GapCategory ?? "?").Append(": ").AppendLine(c.Text);
        sb.AppendLine();
        sb.AppendLine("Folgende Prüf-Aspekte sind BISLANG UNBEHANDELT:");
        foreach (var l in missing) sb.Append("* `").Append(l.Id).Append("` — ").AppendLine(l.RepairHint);
        sb.AppendLine();
        sb.AppendLine("Prüfe JE Aspekt gezielt auf ALLE wesentlichen, voneinander UNABHÄNGIGEN Defizite — erzeuge nicht "
            + "nur einen beliebigen Kandidaten, um den Aspekt formal zu schließen. Ein Aspekt darf durchaus MEHRERE "
            + "Kandidaten ergeben, wenn das Projekt es hergibt (gapCategory = der Aspekt-Bezeichner). Ist der Bereich für "
            + "DIESES Projekt bereits ausreichend behandelt ODER nicht anwendbar, LASS IHN WEG (kein Füllstoff) — eine "
            + "geprüfte, aber bewusst leere Perspektive ist ein gültiges Ergebnis, kein Zwang zu einem neuen Requirement. "
            + "Antworte NUR mit den NEUEN Kandidaten im selben JSON-Format.");
        return sb.ToString();
    }

    /// <summary>Umwelt + je Kandidat der vorige Entwurf (y_t) + das menschliche Feedback (fb_t) für die Self-Refine-Revision.</summary>
    public static string BuildReviseUser(SourceArtifactSet env, IReadOnlyList<L3ReviseItem> items)
    {
        var sb = new StringBuilder(BuildEnvUser(env, forGeneration: false));
        sb.AppendLine();
        sb.AppendLine($"ZU REVIDIERENDE KANDIDATEN ({items.Count}) — überarbeite je Kandidat den bisherigen Entwurf GEMÄSS dem menschlichen Feedback. Behalte die CandidateId als Bezug:");
        foreach (var it in items)
        {
            sb.Append("- ").Append(it.PrevCandidate.CandidateId).Append(" (bisher): ").AppendLine(it.PrevCandidate.Text);
            sb.Append("    feedback: ").AppendLine(string.IsNullOrWhiteSpace(it.Feedback) ? "(kein konkretes Feedback — präzisiere/verankere den Vorschlag)" : it.Feedback);
        }
        return sb.ToString();
    }
}
