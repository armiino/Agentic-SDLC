using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Adr;

/// <summary>Die ADR-Status-Werte (Wire englisch, Haus-Regel; MADR/Nygard-Lebenszyklus).</summary>
public static class AdrStatus
{
    public const string Proposed = "proposed";
    public const string Accepted = "accepted";
    public const string Deprecated = "deprecated";
    public const string Superseded = "superseded";
}

/// <summary>Ein ADR-Entwurf des Autor-Agenten: strukturierte Felder (der Renderer macht daraus die Datei —
/// der Agent schreibt NIE Markdown-Dateien). RelatedIds = Wahrheits-Items (req/arch), die das ADR nennt
/// (Split-Entscheid 06.08.: z. B. REQ-70 bleibt und wird verlinkt).</summary>
public sealed record AdrDraft(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("consequences")] string Consequences,
    [property: JsonPropertyName("relatedIds")] IReadOnlyList<string>? RelatedIds = null,
    [property: JsonPropertyName("alternatives")] string? Alternatives = null);

/// <summary>Eine deterministische Status-Folge: das Core-Item wurde abgelöst (②-Bahn) — sein ADR folgt
/// („superseded by …"), OHNE Agent und OHNE Gate (reine Projektion bereits autorisierter Wahrheit).</summary>
public sealed record AdrStatusFollowUp(string ItemId, string AdrId, string NewStatus, string? SupersededByAdrId);

/// <summary>
/// R-11 A5 (06.08., E-3) — der DETERMINISTISCHE Kern der ADR-Projektion: Core = Wahrheit, ADR-Datei = Projektion
/// (analog PBI→Issue). Scan (wer braucht ein ADR / eine Status-Folge), Nummernvergabe aus dem CORE (nie aus
/// Dateien), MADR-Renderer aus strukturierten Feldern, Index + Architektur-Übersicht. Idempotent: ein Item mit
/// AdrId wird nie erneut vorgelegt („zweimal = keine doppelte ADR", T2.21).
/// </summary>
public static class AdrProjection
{
    /// <summary>Die zu projizierende Menge: AKTIVE design-Items OHNE AdrId.</summary>
    public static IReadOnlyList<ProjectStateItem> PendingAdrItems(ProjectStateDocument core)
        => core.Items
            .Where(IsArch)
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .Where(i => i.Architecture?.Roles.Contains("design", StringComparer.Ordinal) == true)
            .Where(i => string.IsNullOrWhiteSpace(i.Architecture?.AdrId))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();

    /// <summary>Status-Folgen: abgelöste Items MIT ADR, deren AdrStatus noch nicht folgt. Der Nachfolger
    /// (supersedes-Relation) liefert — falls schon projiziert — die „superseded by"-Nummer.</summary>
    public static IReadOnlyList<AdrStatusFollowUp> StatusFollowUps(ProjectStateDocument core)
    {
        var supersededBy = core.Relations
            .Where(r => string.Equals(r.RelationType, "supersedes", StringComparison.Ordinal))
            .GroupBy(r => r.ToId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().FromId, StringComparer.Ordinal);
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);

        return core.Items
            .Where(IsArch)
            .Where(i => !string.IsNullOrWhiteSpace(i.Architecture?.AdrId))
            .Where(i => i.ReadStatus().Validity == Validity.Superseded
                        && !string.Equals(i.Architecture!.AdrStatus, AdrStatus.Superseded, StringComparison.Ordinal))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new AdrStatusFollowUp(i.ItemId, i.Architecture!.AdrId!, AdrStatus.Superseded,
                supersededBy.TryGetValue(i.ItemId, out var succ) && byId.TryGetValue(succ, out var s)
                    ? s.Architecture?.AdrId : null))
            .ToList();
    }

    /// <summary>Nächste freie Nummer — aus dem CORE (Wahrheit), nie aus dem Datei-Ordner.</summary>
    public static int NextAdrNumber(ProjectStateDocument core)
        => core.Items
            .Where(IsArch)
            .Select(i => i.Architecture?.AdrId)
            .Where(id => id is not null && id.StartsWith("ADR-", StringComparison.Ordinal))
            .Select(id => int.TryParse(id!["ADR-".Length..], out var n) ? n : 0)
            .DefaultIfEmpty(0).Max() + 1;

    public static string AdrIdFor(int number) => $"ADR-{number:D4}";

    /// <summary>Dateiname „0007-titel-als-slug.md" (stabil aus Nummer + Titel).</summary>
    public static string FileNameFor(string adrId, string title)
    {
        var slug = new string(title.ToLowerInvariant()
                .Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
                .Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())
            .Trim('-');
        while (slug.Contains("--", StringComparison.Ordinal)) slug = slug.Replace("--", "-");
        return $"{adrId["ADR-".Length..]}-{(slug.Length > 60 ? slug[..60].Trim('-') : slug)}.md";
    }

    /// <summary>Der MADR-Renderer — deterministisch aus dem strukturierten Entwurf (Beleg, kein freier Text).</summary>
    public static string Render(AdrDraft draft, string adrId, string status, ProjectStateDocument core, string? supersededByAdrId = null)
    {
        var textById = core.Items.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
        var sb = new StringBuilder();
        sb.AppendLine($"# {adrId}: {draft.Title}");
        sb.AppendLine();
        sb.AppendLine($"Status: {status}{(supersededByAdrId is null ? "" : $" (superseded by {supersededByAdrId})")}");
        sb.AppendLine($"Core-Item: {draft.ItemId}");
        sb.AppendLine();
        sb.AppendLine("## Kontext").AppendLine().AppendLine(draft.Context.Trim()).AppendLine();
        sb.AppendLine("## Entscheidung").AppendLine().AppendLine(draft.Decision.Trim()).AppendLine();
        sb.AppendLine("## Konsequenzen").AppendLine().AppendLine(draft.Consequences.Trim()).AppendLine();
        if (!string.IsNullOrWhiteSpace(draft.Alternatives))
            sb.AppendLine("## Betrachtete Alternativen").AppendLine().AppendLine(draft.Alternatives!.Trim()).AppendLine();
        if (draft.RelatedIds is { Count: > 0 })
        {
            sb.AppendLine("## Verwandte Core-Items").AppendLine();
            foreach (var id in draft.RelatedIds)
                sb.AppendLine($"- {id} — {textById.GetValueOrDefault(id, "(unbekannt)")}");
            sb.AppendLine();
        }
        return sb.ToString();
    }

    /// <summary>Der ADR-Index (docs/adr/README.md) — deterministisch aus dem Core (Status + Supersede-Kette).</summary>
    public static string RenderIndex(ProjectStateDocument core)
    {
        var rows = core.Items
            .Where(i => IsArch(i) && !string.IsNullOrWhiteSpace(i.Architecture?.AdrId))
            .OrderBy(i => i.Architecture!.AdrId, StringComparer.Ordinal)
            .ToList();
        var sb = new StringBuilder();
        sb.AppendLine("# Architecture Decision Records");
        sb.AppendLine();
        sb.AppendLine("> Projektion aus dem Core (nie von Hand editieren — Änderungen laufen über die Meeting-/Decision-Bahn).");
        sb.AppendLine();
        sb.AppendLine("| ADR | Titel/Fakt | Status | Core-Item |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var i in rows)
            sb.AppendLine($"| {i.Architecture!.AdrId} | {Truncate(i.Text, 90)} | {i.Architecture.AdrStatus ?? AdrStatus.Proposed} | {i.ItemId} |");
        if (rows.Count == 0) sb.AppendLine("| _(noch keine ADRs)_ | | | |");
        return sb.ToString();
    }

    /// <summary>docs/architecture.md — die Autor-gewünschte Gesamt-Übersicht (06.08.): ALLE aktiven arch-Items
    /// nach Rollen gruppiert, mit ADR-Link, bindenden PBIs (constrained_by) und umsetzender Arbeit (covers).
    /// Rein deterministisch aus autorisierten Relationen — wird bei jedem Apply regeneriert, driftet nie.</summary>
    public static string RenderOverview(ProjectStateDocument core)
    {
        var titleById = core.Items.ToDictionary(i => i.ItemId, i => i.Pbi?.Title ?? i.Text, StringComparer.Ordinal);
        List<string> Rel(string type, string archId, bool from) => core.Relations
            .Where(r => string.Equals(r.RelationType, type, StringComparison.Ordinal)
                        && string.Equals(from ? r.FromId : r.ToId, archId, StringComparison.Ordinal))
            .Select(r => from ? r.ToId : r.FromId).OrderBy(x => x, StringComparer.Ordinal).ToList();

        var active = core.Items.Where(i => IsArch(i) && i.ReadStatus().Validity == Validity.Active)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("# Architektur-Übersicht");
        sb.AppendLine();
        sb.AppendLine("> Projektion aus dem Core (Stand = letzter Apply). Rollen: constraint = Rahmen (wirkt in Issues) · work = Arbeit (wird PBI) · design = Entscheidung (wird ADR).");
        foreach (var (role, label) in new[] { ("design", "Entscheidungen (design)"), ("constraint", "Rahmen (constraint)"), ("work", "Arbeit (work)") })
        {
            var items = active.Where(i => i.Architecture?.Roles.Contains(role, StringComparer.Ordinal) == true).ToList();
            sb.AppendLine().AppendLine($"## {label}").AppendLine();
            if (items.Count == 0) { sb.AppendLine("_(keine)_"); continue; }
            foreach (var i in items)
            {
                sb.AppendLine($"- **{i.ItemId}** — {Truncate(i.Text, 140)}");
                if (role == "design" && !string.IsNullOrWhiteSpace(i.Architecture?.AdrId))
                    sb.AppendLine($"  - ADR: {i.Architecture!.AdrId} ({i.Architecture.AdrStatus ?? AdrStatus.Proposed})");
                if (role == "constraint")
                {
                    var pbis = Rel("constrained_by", i.ItemId, from: false);
                    if (pbis.Count > 0) sb.AppendLine($"  - bindet: {string.Join(" · ", pbis.Select(p => $"{p} ({Truncate(titleById.GetValueOrDefault(p, "?"), 50)})"))}");
                }
                if (role == "work")
                {
                    var pbis = Rel("covers", i.ItemId, from: false);
                    sb.AppendLine(pbis.Count > 0
                        ? $"  - umgesetzt durch: {string.Join(" · ", pbis)}"
                        : "  - noch nicht als PBI platziert");
                }
            }
        }
        var superseded = core.Items.Where(i => IsArch(i) && i.ReadStatus().Validity == Validity.Superseded
                                               && !string.IsNullOrWhiteSpace(i.Architecture?.AdrId)).ToList();
        if (superseded.Count > 0)
        {
            sb.AppendLine().AppendLine("## Abgelöst (Historie)").AppendLine();
            foreach (var i in superseded)
                sb.AppendLine($"- {i.ItemId} — {i.Architecture!.AdrId} ({i.Architecture.AdrStatus})");
        }
        return sb.ToString();
    }

    private static bool IsArch(ProjectStateItem i)
        => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase);

    private static string Truncate(string v, int max)
    {
        var t = string.Join(' ', (v ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return t.Length <= max ? t : t[..max] + "…";
    }
}
