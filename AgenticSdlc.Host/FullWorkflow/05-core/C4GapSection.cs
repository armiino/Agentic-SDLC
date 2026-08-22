using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// C4-Kreislauf (⚖ Autor 22.08.): die Lücken-Sektion des C4 ist KEIN Agent-Text mehr, sondern eine
/// DETERMINISTISCHE Projektion des DEC-Topfs — gefiltert auf Architektur-Unklarheiten
/// (<see cref="DecisionAspectMeta"/>). Drei ehrliche Zustände je DEC:
///   offen  → „… (DEC-x, offen)"
///   zu + Wahrheits-Nachweis (answersDecision) ODER bewusster Verzicht (NO_TRUTH_NEEDED) → Zeile weg
///   zu OHNE beides → „⚠ geklärt ohne Architektur-Nachweis" — die Lücke kann der Karte nie still entkommen.
/// Der Section-Replace fasst NUR diese Sektion an (Marker-Kopf bis Datei-Ende) — die Diagramme darüber
/// bleiben Redaktions-Artefakt des Autors.
/// </summary>
public static class C4GapSection
{
    public const string Header = "## Offene Architektur-Lücken";
    // R-71: der Beleg-Stand — welche ARCH/ADR-Belege der AUTOR beim letzten C4-Save gesehen hat (unsichtbarer
    // HTML-Kommentar). Der Frische-Wächter meldet nur das DELTA dazu, nie den Voll-Bestand (Diagramm-
    // Vollständigkeit ist Deutung, kein Wächter-Thema).
    public const string BelegStandPrefix = "<!-- c4-beleg-stand: ";

    public static string Render(ProjectStateDocument core, IReadOnlyCollection<string>? belegStand = null)
    {
        var archDecs = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase)
                        && DecisionAspectMeta.IsArchitecture(i))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();
        // R-71: ziel-behaftete (Widerspruchs-)DECs wurden über die Auflösungs-Maschine verhandelt — ihre
        // Schließung ist dort legitimiert (ADOPT/REFINE/KEEP am Ziel), sie brauchen KEINEN answersDecision-
        // Anker und bekommen nie ein ⚠. Ziel-Signal = EXAKT die Ziel-Semantik der Derivation (beide Quellen):
        // contradicts(_resolved)-Kante ODER targetEntityId-Metadatum.
        var targeted = core.Relations
            .Where(r => r.RelationType is Decision.DecisionRelations.Contradicts or Decision.DecisionRelations.ContradictsResolved)
            .Select(r => r.FromId)
            .ToHashSet(StringComparer.Ordinal);
        bool IsTargeted(ProjectStateItem dec)
            => targeted.Contains(dec.ItemId) || !string.IsNullOrWhiteSpace(dec.Metadata.GetValueOrDefault("targetEntityId"));

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Header).AppendLine();
        sb.AppendLine("> Diese Sektion wird DETERMINISTISCH aus dem Entscheidungs-Topf gerendert (aspekt-markierte");
        sb.AppendLine("> Entscheidungen) — nicht von Hand und nicht vom Zeichen-Agenten pflegen.");
        if (belegStand is { Count: > 0 })
            sb.AppendLine(BelegStandPrefix + string.Join("|", belegStand.OrderBy(x => x, StringComparer.Ordinal)) + " -->");
        sb.AppendLine();

        var lines = 0;
        foreach (var dec in archDecs)
        {
            if (dec.ReadStatus().IsOpenDecision)
            { sb.AppendLine($"- {dec.Text} ({dec.ItemId}, offen)"); lines++; continue; }
            // Geschlossen: sauber mit Wahrheits-Nachweis, dokumentiertem Verzicht ODER Ziel-Auflösung (R-71).
            var waived = string.Equals(dec.Metadata.GetValueOrDefault(DecisionAnswerMeta.WaivedKey), "true", StringComparison.OrdinalIgnoreCase);
            if (waived || IsTargeted(dec) || DecisionAnswerMeta.HasAnsweringTruth(core, dec.ItemId)) continue;
            sb.AppendLine($"- ⚠ geklärt ohne Architektur-Nachweis (nur Begründung): {dec.Text} ({dec.ItemId})"); lines++;
        }
        if (lines == 0) sb.AppendLine("_(keine offenen Architektur-Lücken)_");
        return sb.ToString().TrimEnd() + "\n";
    }

    /// <summary>Aktive Beleg-Menge (ARCH-Ids + ADR-Dateien) — die Währung des Beleg-Stands.</summary>
    public static IReadOnlyCollection<string> AktiveBelege(string repoRoot, ProjectStateDocument core)
    {
        var set = core.Items
            .Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Validity == Validity.Active)
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);
        var adrDir = Path.Combine(repoRoot, "docs", "adr");
        if (Directory.Exists(adrDir))
            foreach (var f in Directory.EnumerateFiles(adrDir, "*.md").Select(Path.GetFileName)
                         .Where(f => f is not null && !f.Equals("README.md", StringComparison.OrdinalIgnoreCase)))
                set.Add("adr:" + f);
        return set;
    }

    /// <summary>Beleg-Stand aus einer c4.md lesen — null, wenn (noch) keiner gestempelt ist.</summary>
    public static IReadOnlyCollection<string>? ReadBelegStand(string content)
    {
        var idx = content.IndexOf(BelegStandPrefix, StringComparison.Ordinal);
        if (idx < 0) return null;
        var end = content.IndexOf(" -->", idx, StringComparison.Ordinal);
        if (end < 0) return null;
        return content[(idx + BelegStandPrefix.Length)..end]
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>Ersetzt/ergänzt die Sektion in docs/c4.md (idempotent; ohne c4-Datei ein No-op).
    /// stampBelegStand=true NUR beim Autor-Save (der Zeichner hatte den Bestand im Input — „gesehen");
    /// der Abgleich-Refresh ERHÄLT den vorhandenen Stempel, damit die Frische-Meldung nicht selbst
    /// verstummt, bevor der Autor je ein Update gemacht hat.</summary>
    public static void Ensure(string repoRoot, ProjectStateDocument core, bool stampBelegStand = false)
    {
        var path = Path.Combine(repoRoot, AuthoredDocument.Resolve("c4")!.RelPath);
        if (!File.Exists(path)) return;
        var content = File.ReadAllText(path);
        var idx = content.IndexOf(Header, StringComparison.Ordinal);
        var body = idx >= 0 ? content[..idx].TrimEnd() : content.TrimEnd();
        var beleg = stampBelegStand ? AktiveBelege(repoRoot, core) : ReadBelegStand(content);
        var next = body + "\n\n" + Render(core, beleg);
        if (!string.Equals(next, content, StringComparison.Ordinal)) File.WriteAllText(path, next);   // idempotent, kein mtime-Churn
    }
}
