namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Autor-Artefakte (Slice S, ⚖ 21.08. „Drei-Klassen-Ordnung"): REDAKTIONS-Artefakte — der Steward
/// ENTWIRFT aus der Projektwahrheit (Rezepte je Art im Steward-Prompt), der Autor redigiert und gibt im
/// Chat FREI; gespeichert wird ausschließlich die freigegebene Fassung samt ehrlicher Entwurfs-Herkunft.
/// Bewusst KEINE Core-Projektion (kein Fingerprint): Willens-/Deutungs-Erklärungen, nicht ableitbar.
/// Die Publikation ist bahn-neutral (Doc-Publish nimmt jede existierende Art in JEDEM Forward mit).
/// FESTE Whitelist — der Steward kann NIE beliebige Dateien schreiben. Neue Art = eine Zeile hier
/// + Entwurfs-Rezept im Prompt. Jeder Save ersetzt den ganzen Text (git/Contents-API versionieren).
/// </summary>
public static class AuthoredDocument
{
    public sealed record Art(string Key, string RelPath, string Titel);

    public static readonly IReadOnlyList<Art> Arten =
    [
        new("vision", "docs/vision.md", "Produktvision"),
        new("personas", "docs/personas.md", "Personas"),
        new("glossar", "docs/glossar.md", "Glossar"),
        new("c4", "docs/c4.md", "Architektur-Landkarte (C4)"),
    ];

    public const string EntwurfAutor = "Autor-Diktat";
    public const string EntwurfSteward = "Steward aus der Projektwahrheit (Core)";

    public static Art? Resolve(string? artKey)
        => Arten.FirstOrDefault(a => string.Equals(a.Key, artKey?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Freigegebene Fassung schreiben (Version/Stand/Herkunfts-Kopf automatisch).</summary>
    public static async Task<(string Path, int Version)> SaveAsync(string repoRoot, string artKey, string text, string? entwurf = null)
    {
        var art = Resolve(artKey) ?? throw new ArgumentException(
            $"Unbekannte Artefakt-Art '{artKey}' (erlaubt: {string.Join("|", Arten.Select(a => a.Key))}).", nameof(artKey));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Artefakt-Text fehlt.", nameof(text));
        var target = Path.Combine(repoRoot, art.RelPath);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var version = RequirementsDocumentProjection.NextVersion(File.Exists(target) ? File.ReadAllText(target) : null);
        var herkunft = string.IsNullOrWhiteSpace(entwurf) ? EntwurfAutor : entwurf.Trim();
        var content = $"# {art.Titel}\n\n> Version: {version} · Stand: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC\n"
                      + $"> Freigabe: Autor (Steward-Chat) · Entwurf: {herkunft}\n\n"
                      + StripStampedHeader(text, art.Titel).Trim() + "\n";
        await File.WriteAllTextAsync(target, content).ConfigureAwait(false);
        // C4-Kreislauf: die Lücken-Sektion ist Systemsache — direkt nach jedem Save deterministisch
        // anfügen/ersetzen (die freigegebene Fassung enthält sie nicht; ohne Core kein Abschnitt).
        if (string.Equals(art.Key, "c4", StringComparison.Ordinal))
        {
            var repo = new JsonCoreRepository(repoRoot);
            if (await repo.ExistsAsync().ConfigureAwait(false))
                // R-71: der Autor-Save STEMPELT den Beleg-Stand (der Zeichner hatte den Bestand im Input) —
                // die Frische-Meldung misst ab jetzt nur noch das Delta dazu.
                C4GapSection.Ensure(repoRoot, await repo.LoadAsync().ConfigureAwait(false), stampBelegStand: true);
        }
        return (target, version);
    }

    /// <summary>R-69 (21.08., c4 v2 mit Doppel-Kopf): der Update-Zyklus füttert dem Drafting-Agenten den
    /// Stand INKLUSIVE System-Kopf — übernimmt er ihn „wörtlich", stünde er doppelt. Deterministisch
    /// abstreifen statt Prompt-Hoffnung: ein führender „# Titel" + seine „>"-Stempel-Zeilen fallen weg
    /// (wiederholt, falls mehrere Alt-Köpfe gestapelt sind); Inhalt darunter bleibt unangetastet.</summary>
    internal static string StripStampedHeader(string text, string titel)
    {
        var lines = text.Trim().Split('\n').ToList();
        while (lines.Count > 0 && string.Equals(lines[0].Trim(), $"# {titel}", StringComparison.Ordinal))
        {
            lines.RemoveAt(0);
            while (lines.Count > 0 && (lines[0].Trim().Length == 0 || lines[0].TrimStart().StartsWith('>'))) lines.RemoveAt(0);
        }
        return string.Join('\n', lines);
    }

    /// <summary>Aktueller Stand einer Art — null, wenn noch nicht erstellt.</summary>
    public static string? Read(string repoRoot, string artKey)
    {
        var art = Resolve(artKey);
        if (art is null) return null;
        var path = Path.Combine(repoRoot, art.RelPath);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }
}
