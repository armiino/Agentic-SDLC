using System.Text;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B §5.0 ①: die deterministische Collect-Stufe (LLM-frei) — der zweischichtige Such-Algorithmus.
/// DIGEST = die AKTIVE Wahrheit als kompakte Übersicht (eine Zeile je Item; Blick nur auf Gültiges).
/// KOLLEKTOREN = berechenbare Lücken-Arten (erweitert das bewiesene ArchGapCollector-Muster).
/// Die Linsen denken auf dieser Grundlage und bohren per Lese-Tools gezielt nach.
/// </summary>
public static class AnalystCollect
{
    public static string Digest(ProjectStateDocument core)
    {
        var featureLabel = core.Items.Where(i => Is(i, "feature"))
            .ToDictionary(i => i.ItemId, i => i.Feature?.Label ?? i.Text, StringComparer.Ordinal);
        var featureOf = core.Relations.Where(r => r.RelationType == "part_of_feature")
            .GroupBy(r => r.FromId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().ToId, StringComparer.Ordinal);

        var sb = new StringBuilder();
        foreach (var i in core.Items.Where(Aktiv).OrderBy(i => i.ItemId, StringComparer.Ordinal))
        {
            var feature = featureOf.TryGetValue(i.ItemId, out var fc)
                ? $", {featureLabel.GetValueOrDefault(fc, fc)}" : "";
            var rollen = i.Architecture?.Roles is { Count: > 0 } r ? $", Rollen: {string.Join("+", r)}" : "";
            sb.AppendLine($"{i.ItemId} [{i.ItemType}{feature}{rollen}] {Truncate(i.Text, 160)}");
        }
        return sb.ToString();
    }

    /// <summary>Berechenbare Lücken-Kandidaten als Klartext-Grundlage für die Linsen-Prompts.</summary>
    public static string KollektorFunde(ProjectStateDocument core)
    {
        var sb = new StringBuilder();
        var arch = ArchGapCollector.Collect(core);
        Block(sb, "PBIs ohne technischen Rahmen (constrained_by fehlt)", arch.ArbeitOhneRahmen.Select(e => $"{e.ItemId} — {e.Titel}"));
        Block(sb, "design-Architektur ohne ADR", arch.DesignOhneAdr.Select(e => $"{e.ItemId} — {e.Titel}"));

        var covered = core.Relations.Where(r => r.RelationType == "covers").Select(r => r.ToId)
            .ToHashSet(StringComparer.Ordinal);
        Block(sb, "Aktive Anforderungen ohne deckendes Arbeitspaket (kein covers)",
            core.Items.Where(i => Aktiv(i) && Is(i, "requirement") && !covered.Contains(i.ItemId))
                .Select(i => $"{i.ItemId} — {Truncate(i.Text, 90)}"));

        Block(sb, "Offene Entscheidungen (laufende Klärungen — NICHT erneut vorschlagen)",
            core.Items.Where(i => Is(i, "decision") && i.ReadStatus().IsOpenDecision)
                .Select(i => $"{i.ItemId} — {Truncate(i.Text, 90)}"));
        return sb.Length == 0 ? "(keine deterministischen Lücken-Kandidaten)" : sb.ToString();
    }

    /// <summary>Das Dedup-GEDÄCHTNIS (§5.0 Blick vs. Gedächtnis): erinnert wird gegen ALLES —
    /// aktive Items, ALLE Entscheidungen (auch geklärte) und die R-35-Ablehnungen.</summary>
    public static (IReadOnlySet<string> CoreKeys, IReadOnlySet<string> DecisionKeys, IReadOnlySet<string> RejectionKeys)
        GedaechtnisKeys(ProjectStateDocument core)
    {
        var coreKeys = core.Items.Where(i => Aktiv(i) && !Is(i, "decision"))
            .Select(i => i.IdentityKey ?? IdentityKey.From(i.Text)).ToHashSet(StringComparer.Ordinal);
        var decKeys = core.Items.Where(i => Is(i, "decision"))
            .Select(i => i.IdentityKey ?? IdentityKey.From(i.Text)).ToHashSet(StringComparer.Ordinal);
        var rejKeys = IngestionRejections.Of(core)
            .Select(p => p.Metadata.GetValueOrDefault("identityKey") ?? "")
            .Where(k => k.Length > 0).ToHashSet(StringComparer.Ordinal);
        return (coreKeys, decKeys, rejKeys);
    }

    private static void Block(StringBuilder sb, string titel, IEnumerable<string> zeilen)
    {
        var list = zeilen.ToList();
        if (list.Count == 0) return;
        sb.AppendLine($"## {titel} ({list.Count})");
        foreach (var z in list) sb.AppendLine($"- {z}");
        sb.AppendLine();
    }

    private static bool Aktiv(ProjectStateItem i) => i.ReadStatus().Validity == Validity.Active;
    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
}
