using System.Text;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Deterministischer Such-ANKER fuer Retrieval/Dedup - NICHT das Identitaets-Urteil (das faellt im
// Resolver + Gate + HumanReview, plan-core-ingestion §4). Normalisiert einen Requirement-Text auf einen
// stabilen Schluessel: klein, ohne Satzzeichen, ohne haeufige Formel-Praefixe, Whitespace kollabiert.
public static class IdentityKey
{
    private static readonly string[] Prefixes =
    [
        "die app soll", "das system soll", "die anwendung soll", "der nutzer soll",
        "die app muss", "das system muss", "es soll", "es muss"
    ];

    public static string From(string? text)
    {
        var lower = (text ?? string.Empty).ToLowerInvariant();

        // Satzzeichen -> Space; nur Buchstaben/Ziffern/Space behalten.
        var sb = new StringBuilder(lower.Length);
        foreach (var ch in lower)
            sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');

        var collapsed = string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Haeufige Formel-Praefixe nur am Anfang entfernen (Anker soll den Kern der Aussage tragen).
        foreach (var p in Prefixes)
        {
            if (collapsed.StartsWith(p + " ", StringComparison.Ordinal))
            {
                collapsed = collapsed[(p.Length + 1)..];
                break;
            }
        }

        return collapsed;
    }
}
