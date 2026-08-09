using AgenticSdlc.Host.FullWorkflow.Core;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// C2a-1 (08.08.2026, §12 c2-inbound-plan) — DIE geteilte Struktur-Naht des kanonischen Issue-Bodys:
/// <see cref="Render"/> (Ersteller: Forward-UPDATE + Initial-Sync-CREATE — wörtlich der bisherige
/// GithubIssueBodySections.Build, E0.1c/E0.2/R-23) und <see cref="Parse"/> (Leser: Inbound-Detect, C2a-4)
/// wohnen als PAAR in dieser Klasse — Ersteller und Leser können nicht driften (Roundtrip-Pinning
/// `Parse(Render(x)) == x`). Struktur morgen ändern = NUR diese Klasse + ihre Tests.
/// Aufbau nach Autor-Direktive „sauberer Output": erst der INHALT (Statement = das WARUM,
/// Akzeptanzkriterien, Requirements), interne Projekt-Zustände erst als deklarierte
/// Sync-Metadaten-Fußzeile — statt als kryptische Kopfzeile.
/// </summary>
public static class GithubIssueTemplate
{
    // Die formalen Sektions-Marker (§8): EINE Definition für Render UND Parse.
    private const string AkHeader = "Akzeptanzkriterien:";
    private const string ConstraintsHeader = "Technische Rahmenbedingungen:";
    private const string ArchHeader = "Umgesetzte Architektur-Arbeit:";
    private const string ReqsPrefix = "Abgedeckte Requirements: ";
    private const string FooterDivider = "---";
    private const string FooterPrefix = "Sync-Metadaten: ";

    public static string Render(GithubSyncEntry e, string quelle)
    {
        var sb = new System.Text.StringBuilder();
        if (!string.IsNullOrWhiteSpace(e.Statement)) sb.Append(e.Statement).Append("\n\n");
        if (e.AcceptanceCriteria is { Count: > 0 })
        {
            sb.Append(AkHeader).Append('\n');
            foreach (var c in e.AcceptanceCriteria) sb.Append("- ").Append(c).Append('\n');
            sb.Append('\n');
        }
        // ③ A3 (06.08.): die WIRKUNG der constraint-Rolle — der Entwickler sieht die Rahmen im Issue,
        // ohne den Core zu kennen (Endpunkt der ①-Relation constrained_by).
        if (e.Constraints is { Count: > 0 })
        {
            sb.Append(ConstraintsHeader).Append('\n');
            foreach (var c in e.Constraints) sb.Append("- ").Append(c).Append('\n');
            sb.Append('\n');
        }
        // A4/E-R2: die work-Herkunft des PBIs - umgesetzte Architektur-Arbeit als eigene Zeile.
        if (e.CoveredArchitecture is { Count: > 0 })
        {
            sb.Append(ArchHeader).Append('\n');
            foreach (var a in e.CoveredArchitecture) sb.Append("- ").Append(a).Append('\n');
            sb.Append('\n');
        }
        var reqs = e.CoveredRequirementIds.Count == 0 ? "-" : string.Join(", ", e.CoveredRequirementIds);
        sb.Append(ReqsPrefix).Append(reqs).Append("\n\n");
        var readiness = string.IsNullOrWhiteSpace(e.Readiness) ? "-" : e.Readiness;
        sb.Append(FooterDivider).Append('\n')
          .Append($"{FooterPrefix}PBI {e.PbiId} · Status {e.Status} · Readiness {readiness} · Quelle: {quelle}");
        return sb.ToString();
    }

    /// <summary>
    /// Leser-Hälfte (C2a-4-Grundlage): zerlegt einen Issue-Body entlang der Sektions-Marker.
    /// Alles, was KEINER Sektion zuzuordnen ist, landet in <see cref="ParsedIssueBody.FreeText"/> —
    /// die künftige Ernte-Fläche für den Agent-Fallback (F1-Fließtext), nie stiller Verlust.
    /// </summary>
    public static ParsedIssueBody Parse(string? body)
    {
        var statement = new List<string>();
        var ak = new List<string>(); var constraints = new List<string>(); var arch = new List<string>();
        var reqs = new List<string>(); var freeText = new List<string>();
        string? pbiId = null, status = null, readiness = null, quelle = null;

        List<string>? section = null;      // aktive Listen-Sektion (AK/Rahmen/Arch)
        var beforeSections = true;         // Statement-Zone = alles vor dem ersten Marker
        var sawDivider = false;

        foreach (var raw in (body ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line == AkHeader) { section = ak; beforeSections = false; continue; }
            if (line == ConstraintsHeader) { section = constraints; beforeSections = false; continue; }
            if (line == ArchHeader) { section = arch; beforeSections = false; continue; }
            if (line.StartsWith(ReqsPrefix, StringComparison.Ordinal))
            {
                var v = line[ReqsPrefix.Length..].Trim();
                if (v != "-") reqs.AddRange(v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                section = null; beforeSections = false; continue;
            }
            if (line == FooterDivider) { sawDivider = true; section = null; beforeSections = false; continue; }
            if (sawDivider && line.StartsWith(FooterPrefix, StringComparison.Ordinal))
            {
                foreach (var part in line[FooterPrefix.Length..].Split('·', StringSplitOptions.TrimEntries))
                {
                    if (part.StartsWith("PBI ", StringComparison.Ordinal)) pbiId = part[4..];
                    else if (part.StartsWith("Status ", StringComparison.Ordinal)) status = part[7..];
                    else if (part.StartsWith("Readiness ", StringComparison.Ordinal)) readiness = part[10..];
                    else if (part.StartsWith("Quelle: ", StringComparison.Ordinal)) quelle = part[8..];
                }
                continue;
            }

            if (beforeSections) { statement.Add(line); continue; }
            if (line.Length == 0) continue;
            if (section is not null && line.StartsWith("- ", StringComparison.Ordinal)) { section.Add(line[2..]); continue; }
            freeText.Add(line);   // menschliche Abweichung — Ernte-Fläche, kein stiller Verlust
        }

        var stmt = string.Join("\n", statement).Trim();
        return new ParsedIssueBody(stmt.Length == 0 ? null : stmt, ak, constraints, arch, reqs,
            pbiId, status, readiness, quelle, freeText);
    }
}

/// <summary>Leser-Sicht des kanonischen Bodys (§8-Sektionen + Sync-Fuß + FreeText-Ernte-Rest).</summary>
public sealed record ParsedIssueBody(
    string? Statement,
    IReadOnlyList<string> AcceptanceCriteria,
    IReadOnlyList<string> Constraints,
    IReadOnlyList<string> CoveredArchitecture,
    IReadOnlyList<string> CoveredRequirementIds,
    string? PbiId,
    string? Status,
    string? Readiness,
    string? Quelle,
    IReadOnlyList<string> FreeText);
