namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Geteilte Mechanik der system-verwalteten Doc-Sektionen (Leitfaden-Slice 22.08., aus C4GapSection
/// extrahiert — EINE Quelle für C4-Lücken UND Story-Map-Tafel): ① Section-Replace ab Marker-Kopf bis
/// Datei-Ende (die Redaktions-Zone darüber bleibt unangetastet) ② Stempel-Kommentar als die EINE
/// maschinenlesbare Zeile im Doc (Beleg-Stand/Zuordnung) — das Sichtbare wird immer daraus regeneriert,
/// nie zurückgeparst.
/// </summary>
public static class ManagedDocSection
{
    /// <summary>Ersetzt die Sektion ab <paramref name="header"/> (bzw. hängt sie an) — idempotent.</summary>
    public static string Replace(string content, string header, string renderedSection)
    {
        var idx = content.IndexOf(header, StringComparison.Ordinal);
        var body = idx >= 0 ? content[..idx].TrimEnd() : content.TrimEnd();
        return body + "\n\n" + renderedSection;
    }

    /// <summary>Payload des Stempel-Kommentars „&lt;prefix&gt;… --&gt;" — null, wenn keiner existiert.</summary>
    public static string? ReadStamp(string content, string prefix)
    {
        var idx = content.IndexOf(prefix, StringComparison.Ordinal);
        if (idx < 0) return null;
        var end = content.IndexOf(" -->", idx, StringComparison.Ordinal);
        if (end < 0) return null;
        return content[(idx + prefix.Length)..end];
    }
}
