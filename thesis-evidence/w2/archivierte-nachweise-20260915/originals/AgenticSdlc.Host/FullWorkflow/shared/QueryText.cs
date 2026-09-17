namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// C3/R-41 (07.08.2026) — die EINE Text-Match-Naht der Lese-Werkzeugkästen (CoreQueryTools,
/// GithubSnapshotQueryTools). Deutsche Texte tragen Umlaute, Agenten-Queries oft die ASCII-Form
/// („Schriftgroesse" vs. „Schriftgröße") — deshalb wird BEIDSEITIG gefaltet (ä→ae, ö→oe, ü→ue, ß→ss)
/// und case-insensitiv verglichen. Deterministisch am Werkzeug statt Prompt-Hoffnung im Modell.
/// </summary>
public static class QueryText
{
    public static bool Contains(string? haystack, string? needle)
        => needle is null || (haystack is not null && Fold(haystack).Contains(Fold(needle), StringComparison.OrdinalIgnoreCase));

    public static string Fold(string s) => s
        .Replace("ä", "ae").Replace("Ä", "Ae")
        .Replace("ö", "oe").Replace("Ö", "Oe")
        .Replace("ü", "ue").Replace("Ü", "Ue")
        .Replace("ß", "ss");

    /// <summary>R-58: Stichwort-Zerlegung für Recall-first-Suchen — faltet Umlaute, trennt an ALLEM
    /// Nicht-Buchstabigen (auch Bindestrich: „Besuchs-Erinnerung" → besuchs, erinnerung) und wirft
    /// Kurz-Tokens (&lt;4) ab. Bewusst NUR für kleine Warn-Bestände (Rejections) — die große Wahrheits-Suche
    /// (list_core_items) bleibt UND-strikt (R-41: kein OR-Raten im 200+-Item-Store).</summary>
    public static IReadOnlyList<string> Tokens(string? query)
        => string.IsNullOrWhiteSpace(query) ? []
           : Fold(query).ToLowerInvariant()
               .Split([' ', '-', '\u2011', ',', '.', ';', ':', '?', '!', '\'', '"', '„', '"', '(', ')', '/', '…'],
                      StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Where(t => t.Length >= 4).Distinct().ToList();
}
