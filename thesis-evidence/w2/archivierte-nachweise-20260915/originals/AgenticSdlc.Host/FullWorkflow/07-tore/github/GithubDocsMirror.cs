namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// Doc-Diff-Quelle (Phase-1i ②, 23.08.): lokaler Spiegel des ZULETZT PUBLIZIERTEN Inhalts je Doc —
/// geschrieben im selben Apply-Moment wie der Core-Stempel (contentHash). Damit kann die Forward-Review
/// bei UPSERT_FILE einen ehrlichen Diff „publiziert → neu" zeigen, ohne GitHub zu fragen. Kein
/// Wahrheits-Zustand (reiner Cache des eigenen letzten Writes); fehlt der Spiegel (Erst-Publikation
/// oder Vor-Spiegel-Ära), sagt die Anzeige das ehrlich statt zu raten.
/// </summary>
public static class GithubDocsMirror
{
    public const string Root = "state/github-docs-mirror";

    private static string PathFor(string repoRoot, string relPath)
        => System.IO.Path.Combine(repoRoot, Root, relPath.Replace('/', System.IO.Path.DirectorySeparatorChar));

    public static string? Read(string repoRoot, string relPath)
    {
        var p = PathFor(repoRoot, relPath);
        return File.Exists(p) ? File.ReadAllText(p) : null;
    }

    public static void Write(string repoRoot, string relPath, string publishedContent)
    {
        var p = PathFor(repoRoot, relPath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(p)!);
        File.WriteAllText(p, publishedContent);
    }

    /// <summary>Kompakter, deterministischer Sektions-Diff (Überschriften-Ebene #/##/###) für die
    /// Review-Karte: Versions-Sprung + welche Sektionen neu/entfallen/geändert sind. Kein Zeilen-Diff —
    /// die Karte soll orientieren, nicht ersetzen.</summary>
    public static string DiffSummary(string? published, string neu)
    {
        var vAlt = published is null ? null : VersionOf(published);
        var vNeu = VersionOf(neu);
        var version = vAlt is null
            ? (vNeu is null ? null : $"Version {vNeu} (Erst-Publikation oder ohne lokalen Spiegel)")
            : $"Version {vAlt} → {vNeu ?? "?"}";
        if (published is null)
            return version ?? "kein lokaler Spiegel des publizierten Stands (Vor-Spiegel-Ära) — Voll-Inhalt prüfen";

        var alt = Sections(published);
        var jetzt = Sections(neu);
        var teile = new List<string>();
        if (version is not null) teile.Add(version);
        var neue = jetzt.Keys.Where(k => !alt.ContainsKey(k)).ToList();
        var weg = alt.Keys.Where(k => !jetzt.ContainsKey(k)).ToList();
        var geaendert = jetzt.Keys.Where(k => alt.TryGetValue(k, out var a) && !string.Equals(a, jetzt[k], StringComparison.Ordinal)).ToList();
        if (neue.Count > 0) teile.Add($"neue Sektion(en): {string.Join(", ", neue)}");
        if (weg.Count > 0) teile.Add($"entfallen: {string.Join(", ", weg)}");
        if (geaendert.Count > 0) teile.Add($"geändert: {string.Join(", ", geaendert)}");
        if (neue.Count == 0 && weg.Count == 0 && geaendert.Count == 0) teile.Add("nur Kopf-/Detailänderungen ohne Sektions-Verschiebung");
        return string.Join(" · ", teile);
    }

    private static string? VersionOf(string content)
    {
        var m = System.Text.RegularExpressions.Regex.Match(content, @"> Version: (\d+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    private static Dictionary<string, string> Sections(string content)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        string current = "(Kopf)";
        var sb = new System.Text.StringBuilder();
        foreach (var line in content.Split('\n'))
        {
            // Versions-/Stand-Kopfzeilen ändern sich bei JEDEM Save — sie zählen nicht als Inhalts-Diff
            // (der Versions-Sprung wird separat gemeldet; SameBody-Geist aus R-72).
            if (line.TrimStart().StartsWith("> Version:", StringComparison.Ordinal)) continue;
            if (line.StartsWith("#", StringComparison.Ordinal) && line.TrimStart('#').StartsWith(" ", StringComparison.Ordinal))
            {
                result[current] = sb.ToString(); sb.Clear();
                current = line.Trim();
            }
            else sb.Append(line).Append('\n');
        }
        result[current] = sb.ToString();
        return result;
    }
}
