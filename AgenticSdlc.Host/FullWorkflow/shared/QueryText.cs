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

    private static string Fold(string s) => s
        .Replace("ä", "ae").Replace("Ä", "Ae")
        .Replace("ö", "oe").Replace("Ö", "Oe")
        .Replace("ü", "ue").Replace("Ü", "Ue")
        .Replace("ß", "ss");
}
