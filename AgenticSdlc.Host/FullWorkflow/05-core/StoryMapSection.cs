using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Story-Map-Tafel (Leitfaden-Slice 22.08., Endform + Szenario-Katalog: `Thesis-Docs/aktiv/leitfaden-abdeckung.md
/// §4/§4a`): das Doc hat ZWEI Zonen — die Erzähl-Zone (Redaktion des Autors/Drafting-Agenten: Reise,
/// Schritt-Sätze, MVP-Vorschlag; das System fasst sie NIE an) und DIESE eine deterministische Tafel-Sektion.
/// Die Tafel wird komplett aus der Zuordnungs-Zeile (Stempel-Kommentar) + dem Core regeneriert:
/// Kärtchen = PBI-Story (`goal`) · Prio · ✓ fertig (Issue-Kreislauf) · ⚠ in Klärung; nicht zugeordnete
/// aktive PBIs landen SICHTBAR im Sammelbecken (S5/S6 — nie still einsortiert, nie still verloren).
/// Hand-Edits in der Tafel heilen beim nächsten Berühren (S7, H-Test-Muster des C4-§3).
/// </summary>
public static class StoryMapSection
{
    public const string Header = "## Story-Map-Tafel";
    public const string StampPrefix = "<!-- storymap-zuordnung: ";
    public const string SammelbeckenTitel = "### Noch nicht eingeordnet";

    /// <summary>Ein Reise-Schritt mit seinen zugeordneten Kärtchen (Reihenfolge = Autor-abgenommene Redaktion).</summary>
    public sealed record Schritt(
        [property: JsonPropertyName("key")] string Key,
        [property: JsonPropertyName("titel")] string Titel,
        [property: JsonPropertyName("pbiIds")] IReadOnlyList<string> PbiIds);

    /// <summary>Die maschinenlesbare Wahrheit des Docs: Zuordnung + Personas-Stand (Reise-Wächter, S12).</summary>
    public sealed record Zuordnung(
        [property: JsonPropertyName("personasVersion")] int PersonasVersion,
        [property: JsonPropertyName("schritte")] IReadOnlyList<Schritt> Schritte);

    private static readonly JsonSerializerOptions Json = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Zuordnung aus dem Doc lesen — null bei fehlendem/unlesbarem Stempel (S6: ehrliche
    /// Degradation beim Render, kein Raten; der nächste ⚿-Save stellt die Zeile wieder her).</summary>
    public static Zuordnung? ReadZuordnung(string content)
    {
        var payload = ManagedDocSection.ReadStamp(content, StampPrefix);
        if (payload is null) return null;
        try { return JsonSerializer.Deserialize<Zuordnung>(payload, Json); }
        catch (JsonException) { return null; }
    }

    /// <summary>Beleg-Wachen S2–S4 — LAUT statt still: unbekannte/artfremde Ids, Doppel-Zuordnung,
    /// leere Reise verweigern den Save. Abgelöste (nicht mehr aktive) Ids sind KEIN Fehler — sie werden
    /// beim Render deterministisch entfernt (S1), sonst würde jede Ablösung alte Saves invalidieren.</summary>
    public static void Validate(Zuordnung zuordnung, ProjectStateDocument core)
    {
        var fehler = new List<string>();
        if (zuordnung.Schritte.Count == 0) fehler.Add("Reise ohne Schritte (mindestens 1 Schritt).");
        var pbis = core.Items.Where(i => IsPbi(i)).Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var gesehen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var s in zuordnung.Schritte)
        {
            if (string.IsNullOrWhiteSpace(s.Titel)) fehler.Add($"Schritt '{s.Key}': Titel fehlt.");
            foreach (var id in s.PbiIds)
            {
                if (!pbis.Contains(id)) fehler.Add($"Schritt '{s.Key}': '{id}' ist kein PBI im Core.");
                else if (!gesehen.Add(id)) fehler.Add($"'{id}' ist mehrfach zugeordnet (eine Karte, ein Platz).");
            }
        }
        if (fehler.Count > 0)
            throw new ArgumentException("Story-Map-Zuordnung ungültig: " + string.Join(" · ", fehler));
    }

    /// <summary>Die Tafel — deterministisch aus Zuordnung + Core; stabile Ordnung, nichts Volatiles (R-63).</summary>
    public static string Render(ProjectStateDocument core, Zuordnung? zuordnung)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(Header).AppendLine();
        sb.AppendLine("> Diese Tafel wird DETERMINISTISCH aus Zuordnung + Projektwahrheit gerendert (Story-Satz,");
        sb.AppendLine("> Prio und ✓-Stand kommen live aus dem Core) — nicht von Hand und nicht vom Agenten pflegen.");
        if (zuordnung is not null)
            sb.AppendLine(StampPrefix + JsonSerializer.Serialize(zuordnung, Json) + " -->");
        sb.AppendLine();

        var zugeordnet = new HashSet<string>(StringComparer.Ordinal);
        if (zuordnung is null)
            sb.AppendLine("_(keine Zuordnung im Doc — alle Kärtchen unten; der nächste freigegebene Entwurf stellt die Reise her)_").AppendLine();
        else
            foreach (var schritt in zuordnung.Schritte)
            {
                sb.AppendLine($"### {schritt.Titel}").AppendLine();
                var rows = schritt.PbiIds
                    .Select(id => core.Items.FirstOrDefault(i => IsPbi(i) && string.Equals(i.ItemId, id, StringComparison.Ordinal)))
                    .Where(i => i is not null && i.ReadStatus().Validity == Validity.Active)   // S1: Abgelöstes fällt raus
                    .Cast<ProjectStateItem>().ToList();
                schritt.PbiIds.ToList().ForEach(id => zugeordnet.Add(id));
                AppendKarten(sb, rows);
            }

        var becken = core.Items
            .Where(i => IsPbi(i) && i.ReadStatus().Validity == Validity.Active
                        && i.ReadStatus().Progress != Progress.Done          // S10: Fertiges wartet nicht
                        && !zugeordnet.Contains(i.ItemId))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();
        if (becken.Count > 0)
        {
            sb.AppendLine(SammelbeckenTitel).AppendLine();
            AppendKarten(sb, becken);
        }
        return sb.ToString().TrimEnd() + "\n";
    }

    private static void AppendKarten(System.Text.StringBuilder sb, IReadOnlyList<ProjectStateItem> pbis)
    {
        if (pbis.Count == 0) { sb.AppendLine("_(keine Kärtchen)_").AppendLine(); return; }
        sb.AppendLine("| PBI | Story | Prio | Stand |");
        sb.AppendLine("| --- | --- | --- | --- |");
        foreach (var p in pbis)
        {
            var status = p.ReadStatus();
            // S8: für Unklares wird KEINE Story erfunden — der Anforderungs-Text + ehrlicher Marker.
            var story = !string.IsNullOrWhiteSpace(p.Pbi?.Goal) ? p.Pbi!.Goal!
                : p.Text + (status.Blocker == Blocker.NeedsClarify ? " — ⚠ in Klärung" : "");
            var stand = status.Progress == Progress.Done ? "✓ fertig"
                : status.Blocker == Blocker.BlockedByDecision ? "⏸ blockiert" : "";
            sb.AppendLine($"| {p.ItemId} | {Cell(story)} | {Cell(p.Metadata.GetValueOrDefault(PbiFields.MetaPriority))} | {stand} |");
        }
        sb.AppendLine();
    }

    private static string Cell(string? s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Replace("|", "\\|").Replace("\n", " ").Trim();

    private static bool IsPbi(ProjectStateItem i) => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase);

    /// <summary>Tafel im Doc nachziehen (idempotent; ohne storymap-Datei ein No-op) — hängt an der
    /// EINEN Refresh-Naht (RefreshDeterministicProjections) + am Save; die Erzähl-Zone bleibt unberührt.</summary>
    public static void Ensure(string repoRoot, ProjectStateDocument core)
    {
        var path = Path.Combine(repoRoot, AuthoredDocument.Resolve("storymap")!.RelPath);
        if (!File.Exists(path)) return;
        var content = File.ReadAllText(path);
        var next = ManagedDocSection.Replace(content, Header, Render(core, ReadZuordnung(content)));
        if (!string.Equals(next, content, StringComparison.Ordinal)) File.WriteAllText(path, next);
    }

    /// <summary>Aktuelle Personas-Version (Reise-Grundlage) — 0, wenn es (noch) keine Personas gibt.</summary>
    public static int CurrentPersonasVersion(string repoRoot)
    {
        var content = AuthoredDocument.Read(repoRoot, "personas");
        if (content is null) return 0;
        var m = System.Text.RegularExpressions.Regex.Match(content, @"> Version: (\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : 0;
    }

    /// <summary>Frische-Notiz (Lage-Frage, pull): NUR Handlungsleitendes — uneingeordnete Kärtchen (S5),
    /// fehlende Zuordnung (S6), Personas neuer als die Reise (S12). null = nichts zu melden/kein Doc.</summary>
    public static string? FrischeNotiz(string repoRoot, ProjectStateDocument core)
    {
        var content = AuthoredDocument.Read(repoRoot, "storymap");
        if (content is null) return null;
        var zuordnung = ReadZuordnung(content);

        var teile = new List<string>();
        if (zuordnung is null)
            teile.Add("hat keine (lesbare) Zuordnungs-Zeile — der nächste freigegebene Entwurf stellt sie her");
        else
        {
            var zugeordnet = zuordnung.Schritte.SelectMany(s => s.PbiIds).ToHashSet(StringComparer.Ordinal);
            var wartend = core.Items.Count(i => IsPbi(i) && i.ReadStatus().Validity == Validity.Active
                                                && i.ReadStatus().Progress != Progress.Done && !zugeordnet.Contains(i.ItemId));
            if (wartend > 0) teile.Add($"{wartend} PBI(s) warten auf Einordnung in die Reise");
            var personas = CurrentPersonasVersion(repoRoot);
            if (personas > zuordnung.PersonasVersion)
                teile.Add($"Personas sind neuer (v{personas}) als die Reise-Grundlage (v{zuordnung.PersonasVersion})");
        }
        if (teile.Count == 0) return null;
        return $"Story Map (docs/storymap.md) {string.Join(" · ", teile)} — Update anbieten (draft_authored_doc('storymap')).";
    }
}
