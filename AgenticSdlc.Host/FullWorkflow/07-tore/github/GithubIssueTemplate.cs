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
    // Projektions-Nachzug ③ (Autor-⚖ 20.08.): Markdown-Köpfe (###) statt Doppelpunkt-Zeilen —
    // erlaubt, WEIL Render und Parse dieselben Konstanten teilen (Roundtrip-Pin); Übergangs-Regel:
    // erst ernten, dann Stil + der EINE Reproject (Alt-Bodies matchen ihre Alt-Stempel, die
    // Drift-Erkennung läuft über Hash-Vergleich, nie über diesen Parser).
    private const string AkHeader = "### Akzeptanzkriterien";
    private const string ConstraintsHeader = "### Technische Rahmenbedingungen";
    private const string ArchHeader = "### Umgesetzte Architektur-Arbeit";
    private const string ReqsHeader = "### Abgedeckte Requirements";
    // Blockquote-Stil (Autor-⚖ 20.08. spät): Sektions-Inhalt steht als "> "-Block unter dem Kopf,
    // ein NACKTES ">" ist der sichtbare Leer-Marker („hier gehört etwas hin"). Der Parser liest
    // tolerant MIT und OHNE ">"-Klammer — ein Kollege, der das ">" vergisst, verliert nichts.
    private const string Quote = "> ";
    // "> &nbsp;" statt nacktem ">": GitHub rendert den leeren Block sonst UNSICHTBAR — der sichtbare
    // graue Balken IST der gewollte „hier gehört etwas hin"-Marker (Autor-Feile 20.08. spät).
    private const string EmptyQuote = "> &nbsp;";
    private const string LegacyEmptyQuote = ">";
    private const string FooterDivider = "---";
    private const string FooterPrefix = "Sync-Metadaten: ";
    // Legacy-LESBARKEIT (Abnahme-Fund 20.08., Ernte-Lauf 151033/#45): Alt-Stil-Bodies existieren in
    // freier Wildbahn (drift-gesperrte Issues) und in Beleg-Läufen — der Parser versteht die Alt-Köpfe
    // WEITER, sonst mis-beschreibt die Ernte einen editierten Alt-Body („AK ENTFERNT", obwohl nur der
    // Kopf nicht erkannt wurde). Geschrieben wird ausschließlich der neue Stil.
    private const string LegacyAkHeader = "Akzeptanzkriterien:";
    private const string LegacyConstraintsHeader = "Technische Rahmenbedingungen:";
    private const string LegacyArchHeader = "Umgesetzte Architektur-Arbeit:";
    private const string LegacyReqsPrefix = "Abgedeckte Requirements: ";
    private const string LegacyReqsBold = "**Abgedeckte Requirements:** ";

    public static string Render(GithubSyncEntry e, string quelle)
    {
        var sb = new System.Text.StringBuilder();
        // ③ Pflicht-Sektionen (Autor-⚖: leer = leer, KEIN Platzhalter): Statement/AK/Rahmen erscheinen
        // IMMER — eine leere Sektion zeigt die Lücke ehrlich, statt sie zu verstecken.
        if (!string.IsNullOrWhiteSpace(e.Statement)) sb.Append(e.Statement).Append("\n\n");
        AppendBlock(sb, AkHeader, (e.AcceptanceCriteria ?? []).Select(c => "- " + c));
        // ③ A3 (06.08.): die WIRKUNG der constraint-Rolle — der Entwickler sieht die Rahmen im Issue,
        // ohne den Core zu kennen (Endpunkt der ①-Relation constrained_by).
        AppendBlock(sb, ConstraintsHeader, (e.Constraints ?? []).Select(c => "- " + c));
        // A4/E-R2: die work-Herkunft des PBIs — bleibt BEDINGT (Herkunfts-Info, keine Lücke).
        if (e.CoveredArchitecture is { Count: > 0 })
            AppendBlock(sb, ArchHeader, e.CoveredArchitecture.Select(a => "- " + a));
        AppendBlock(sb, ReqsHeader, e.CoveredRequirementIds.Count == 0
            ? []
            : [string.Join(", ", e.CoveredRequirementIds.Select(r => $"`{r}`"))]);
        var readiness = string.IsNullOrWhiteSpace(e.Readiness) ? "-" : e.Readiness;
        sb.Append(FooterDivider).Append('\n')
          .Append($"{FooterPrefix}PBI {e.PbiId} · Status {e.Status} · Readiness {readiness} · Quelle: {quelle}");
        return sb.ToString();
    }

    private static void AppendBlock(System.Text.StringBuilder sb, string header, IEnumerable<string> lines)
    {
        sb.Append(header).Append("\n\n");
        var any = false;
        foreach (var l in lines) { sb.Append(Quote).Append(l).Append('\n'); any = true; }
        if (!any) sb.Append(EmptyQuote).Append('\n');
        sb.Append('\n');
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
        var inReqs = false;                // eigene Sektions-Art: Requirements-Block (backtick-IDs, kommasepariert)
        var beforeSections = true;         // Statement-Zone = alles vor dem ersten Marker
        var sawDivider = false;

        foreach (var raw in (body ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line == AkHeader || line == LegacyAkHeader) { section = ak; inReqs = false; beforeSections = false; continue; }
            if (line == ConstraintsHeader || line == LegacyConstraintsHeader) { section = constraints; inReqs = false; beforeSections = false; continue; }
            if (line == ArchHeader || line == LegacyArchHeader) { section = arch; inReqs = false; beforeSections = false; continue; }
            if (line == ReqsHeader) { section = null; inReqs = true; beforeSections = false; continue; }
            if (line.StartsWith(LegacyReqsBold, StringComparison.Ordinal) || line.StartsWith(LegacyReqsPrefix, StringComparison.Ordinal))
            {
                var v = line[(line.StartsWith(LegacyReqsBold, StringComparison.Ordinal) ? LegacyReqsBold.Length : LegacyReqsPrefix.Length)..].Trim();
                if (v != "-") reqs.AddRange(v.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
                section = null; inReqs = false; beforeSections = false; continue;
            }
            if (line == FooterDivider) { sawDivider = true; section = null; inReqs = false; beforeSections = false; continue; }
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
            // Blockquote-Klammer abstreifen; "> &nbsp;" (und das nackte ">") ist der Leer-Marker.
            if (line == EmptyQuote || line == LegacyEmptyQuote) continue;
            var content = line.StartsWith(Quote, StringComparison.Ordinal) ? line[Quote.Length..].TrimEnd() : line;
            if (content.Length == 0 || content == "&nbsp;") continue;
            if (inReqs)
            {
                foreach (var id in content.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                {
                    var v = id.Trim('`').Trim();
                    if (v.Length > 0 && v != "-") reqs.Add(v);
                }
                continue;
            }
            if (section is not null && content.StartsWith("- ", StringComparison.Ordinal)) { section.Add(content[2..]); continue; }
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
