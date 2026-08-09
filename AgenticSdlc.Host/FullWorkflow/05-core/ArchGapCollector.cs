using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Freeze-Schritt 3 / 9k(b) (09.08.2026) — der ARCH-LÜCKEN-Katalog: die arch-Schwester des Klärungs-Sweeps.
/// Anders als needs_clarify (Blocker-Flag) ist Architektur-Unklarheit RELATIONAL und seit R-11
/// DETERMINISTISCH messbar: ① aktive PBIs ohne jeden technischen Rahmen (keine constrained_by-Kante) ·
/// ② design-Rollen ohne ADR · ③ unklassifizierte arch-Items (keine Rollen). Kein LLM, kein Urteil, kein
/// Schreiben. Der ANTWORT-Weg ist die EXISTIERENDE Autor-Front (disposition architecture → Delta →
/// arch-Tor-1 → classify → adr) — dieser Katalog liefert nur den Anlass + Kontext fürs Gespräch.
/// </summary>
public static class ArchGapCollector
{
    public static ArchGapKatalog Collect(ProjectStateDocument core)
    {
        var constrainedPbis = core.Relations
            .Where(r => string.Equals(r.RelationType, ConstraintSwap.Relation, StringComparison.Ordinal))
            .Select(r => r.FromId).ToHashSet(StringComparer.Ordinal);
        var featureOf = core.Relations
            .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal))
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var ohneRahmen = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Validity == Validity.Active
                        && !constrainedPbis.Contains(i.ItemId))
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new ArchGapEintrag(i.ItemId, i.Pbi?.Title ?? i.Text, featureOf.GetValueOrDefault(i.ItemId)))
            .ToList();

        var archs = core.Items
            .Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Validity == Validity.Active)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal).ToList();
        var designOhneAdr = archs
            .Where(a => (a.Architecture?.Roles ?? []).Contains("design", StringComparer.OrdinalIgnoreCase)
                        && string.IsNullOrWhiteSpace(a.Architecture?.AdrId))
            .Select(a => new ArchGapEintrag(a.ItemId, a.Text, null)).ToList();
        var unklassifiziert = archs
            .Where(a => a.Architecture is null || a.Architecture.Roles.Count == 0)
            .Select(a => new ArchGapEintrag(a.ItemId, a.Text, null)).ToList();

        return new ArchGapKatalog(ohneRahmen.Count + designOhneAdr.Count + unklassifiziert.Count,
            ohneRahmen, designOhneAdr, unklassifiziert);
    }
}

public sealed record ArchGapEintrag(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("titel")] string Titel,
    [property: JsonPropertyName("feature")] string? Feature);

public sealed record ArchGapKatalog(
    [property: JsonPropertyName("offeneLuecken")] int OffeneLuecken,
    [property: JsonPropertyName("arbeitOhneRahmen")] IReadOnlyList<ArchGapEintrag> ArbeitOhneRahmen,
    [property: JsonPropertyName("designOhneAdr")] IReadOnlyList<ArchGapEintrag> DesignOhneAdr,
    [property: JsonPropertyName("unklassifiziert")] IReadOnlyList<ArchGapEintrag> Unklassifiziert);
