using System.Text;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

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
}
