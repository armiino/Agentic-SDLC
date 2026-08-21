using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// 1g-Abnahme-Vorprüfungs-Fund (19.08., die „Besteller"-Falle): Analyst-Herkunfts-Metadata müssen den
/// gated Tor-1-Apply ÜBERLEBEN (Muster GithubOriginMeta) — sonst stirbt `analystKategorie` am Apply und
/// die NFR-Sektion des Anforderungsdokuments bliebe trotz Adoption für immer leer; Herleitung/Linse sind
/// zudem der Beleg „Autor-Freigabe auf Analyst-Vorschlag" (§2 der zwei Vollständigkeits-Begriffe).
/// No-op für Meeting-/Autor-/GitHub-Incomings — die geteilte Naht bleibt neutral.
/// </summary>
public static class AnalystOriginMeta
{
    public const string Kategorie = RequirementsDocumentProjection.KategorieKey;   // "analystKategorie"
    public const string Herleitung = "herleitung";
    public const string Linse = "linse";

    private static readonly string[] Keys = [Kategorie, Herleitung, Linse];

    public static void CarryOver(ProjectStateItem incoming, IDictionary<string, string> meta)
    {
        foreach (var key in Keys)
            if (incoming.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                meta[key] = value;
    }
}
