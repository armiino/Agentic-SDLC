namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// DISK-12 / Iteration B22: steuert PRO ARTEFAKTTYP, welche Fehlerkategorien überhaupt erzeugt UND
/// bewertet werden. Eine hier deaktivierte Kategorie wird gar nicht erst generiert (kein LLM-Call)
/// und taucht im Score mit 0 auf.
/// </summary>
/// <remarks>
/// Hintergrund: Der Call-1-Split (B21) hob den Recall — und legte damit frei, dass <c>FALSE_CLAIM</c>
/// nicht zum Artefakttyp <c>open-questions</c> passt: ein offene-Fragen-Dokument stellt legitim
/// Optionen/Beispiele ("z. B. WORM-Bucket …") zur Diskussion; der FALSE_CLAIM-Judge wertet diese als
/// erfundene Claims (8 False Positives, errorScore 0→16). Lösung: Kategorie-Aktivierung je Artefakttyp
/// statt global. Default deaktiviert FALSE_CLAIM für open-questions; per run-config überschreibbar
/// (<c>jury.categories</c>), damit die aktive Policy reproduzierbar im Snapshot landet (FORSCH-2).
/// Gilt für Generierung UND Verifikation; die globale <see cref="JuryVerificationPolicy"/> steuert
/// weiterhin, OB eine (aktivierte) Kategorie einen zweiten Verifikations-Pass bekommt.
/// </remarks>
public sealed class JuryCategoryProfile
{
    public static readonly IReadOnlyList<string> AllCategories =
        new[] { "FALSE_CLAIM", "FALSE_CERTAINTY", "MISSING_TOPIC" };

    /// <summary>Bekannte Artefakttypen — für den Config-Snapshot (<see cref="Describe"/>).</summary>
    public static readonly IReadOnlyList<string> KnownArtifactTypes =
        new[] { "requirements", "risks", "architecture", "open-questions", "generic" };

    // Code-Defaults je Artefakttyp. Fehlt ein Typ -> alle Kategorien aktiv.
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Defaults =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            // open-questions: FALSE_CLAIM aus (Optionen/Beispiele in offenen Fragen sind keine Claims).
            ["open-questions"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "FALSE_CERTAINTY", "MISSING_TOPIC" }
        };

    private readonly IReadOnlyDictionary<string, IReadOnlySet<string>> _overrides;

    /// <param name="configOverrides">
    /// Optionale run-config-Overrides (Artefakttyp -> erlaubte Kategorie-Keys). Überschreibt den
    /// Code-Default für genau diesen Typ. null/leer -> nur Code-Defaults.
    /// </param>
    public JuryCategoryProfile(IReadOnlyDictionary<string, IReadOnlyList<string>>? configOverrides = null)
    {
        if (configOverrides is null || configOverrides.Count == 0)
        {
            _overrides = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        var map = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (type, cats) in configOverrides)
        {
            if (string.IsNullOrWhiteSpace(type) || cats is null)
                continue;

            var set = new HashSet<string>(
                cats.Select(NormalizeKey).Where(k => AllCategories.Contains(k)),
                StringComparer.OrdinalIgnoreCase);
            map[NormalizeType(type)] = set;
        }

        _overrides = map;
    }

    /// <summary>Kanonischer Artefakttyp aus dem Dateinamen (analog <c>JuryPrompts.ProfileFor</c>).</summary>
    public static string ArtifactType(string artifactName)
    {
        var n = (artifactName ?? string.Empty).ToLowerInvariant();
        if (n.Contains("open") || n.Contains("question")) return "open-questions";
        if (n.Contains("requirement")) return "requirements";
        if (n.Contains("risk")) return "risks";
        if (n.Contains("arch")) return "architecture";
        return "generic";
    }

    /// <summary>Aktive Kategorien für ein konkretes Artefakt (per Dateiname).</summary>
    public IReadOnlySet<string> EnabledFor(string artifactName)
        => EnabledForType(ArtifactType(artifactName));

    public bool IsEnabled(string artifactName, string categoryKey)
        => EnabledFor(artifactName).Contains(NormalizeKey(categoryKey));

    /// <summary>Effektive Kategorien je bekanntem Artefakttyp — für den Config-Snapshot (Reproduzierbarkeit).</summary>
    public IReadOnlyDictionary<string, string[]> Describe()
        => KnownArtifactTypes.ToDictionary(
            t => t,
            t => EnabledForType(t).OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            StringComparer.OrdinalIgnoreCase);

    private IReadOnlySet<string> EnabledForType(string type)
    {
        if (_overrides.TryGetValue(type, out var ov)) return ov;
        if (Defaults.TryGetValue(type, out var def)) return def;
        return new HashSet<string>(AllCategories, StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeKey(string k) => k.Trim().ToUpperInvariant().Replace('-', '_').Replace(' ', '_');
    private static string NormalizeType(string t) => t.Trim().ToLowerInvariant();
}
