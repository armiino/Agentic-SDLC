using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Adr;

public sealed record AdrGateIssue(string Code, string Repairability, string Message, string? ItemId);
public sealed record AdrGateReport(bool Pass, IReadOnlyList<AdrGateIssue> Errors)
{
    public bool HasRepairable => Errors.Any(e => e.Repairability == Repairability.Repairable);
}

/// <summary>
/// R-11 A5 — das DETERMINISTISCHE Gate des ADR-Autor-Loops (GateLoop-Form): prüft die Agent-Entwürfe gegen
/// die zu projizierende Menge (Coverage: genau EIN Entwurf je pending design-Item), Pflichtfelder (MADR),
/// Related-Existenz (Wahrheits-Items im Core). Quell-agnostisch: die Menge kommt aus dem CORE (T2.21).
/// </summary>
public static class AdrGate
{
    public static AdrGateReport Check(IReadOnlyList<ProjectStateItem> pending, IReadOnlyList<AdrDraft> drafts,
        ProjectStateDocument core)
    {
        var errors = new List<AdrGateIssue>();
        var pendingIds = pending.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var truthIds = core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);

        var byId = drafts.GroupBy(d => d.ItemId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        foreach (var id in pendingIds.Where(id => !byId.ContainsKey(id)))
            errors.Add(new("MISSING_DRAFT", Repairability.Repairable, $"design-Item '{id}' hat keinen ADR-Entwurf.", id));
        foreach (var (id, count) in byId.Where(kv => kv.Value > 1))
            errors.Add(new("DUPLICATE_DRAFT", Repairability.Repairable, $"'{id}' hat {count} Entwürfe (genau einer erlaubt).", id));

        foreach (var d in drafts)
        {
            if (!pendingIds.Contains(d.ItemId))
                errors.Add(new("UNKNOWN_TARGET", Repairability.Repairable, $"Entwurf für '{d.ItemId}' — kein pending design-Item.", d.ItemId));
            foreach (var (field, value) in new[] { ("title", d.Title), ("context", d.Context), ("decision", d.Decision), ("consequences", d.Consequences) })
                if (string.IsNullOrWhiteSpace(value))
                    errors.Add(new("MISSING_FIELD", Repairability.Repairable, $"'{d.ItemId}': Pflichtfeld '{field}' ist leer (MADR).", d.ItemId));
            foreach (var r in (d.RelatedIds ?? []).Where(r => !truthIds.Contains(r)))
                errors.Add(new("UNKNOWN_RELATED", Repairability.Repairable, $"'{d.ItemId}': related '{r}' ist kein Wahrheits-Item (requirement|architecture) im Core.", d.ItemId));
        }
        return new AdrGateReport(errors.Count == 0, errors);
    }

    public static GateDecision Decide(AdrGateReport report, int attempt, int maxAttempts)
        => GateLoop.Decide(report.Pass, report.HasRepairable, attempt, maxAttempts);

    /// <summary>Der deterministische APPLY: schreibt je akzeptiertem Entwurf die ADR-Datei (Status accepted —
    /// die Gate-Abnahme IST die Entscheidung), stempelt AdrId/AdrStatus ins Item, zieht Status-Folgen nach
    /// („superseded by", inkl. Datei-Neurender aus persistiertem Entwurf falls vorhanden — sonst nur Index),
    /// und regeneriert Index + Architektur-Übersicht. Idempotent: Items mit AdrId werden nie neu nummeriert.</summary>
    public static (ProjectStateDocument Core, IReadOnlyList<(string Path, string Content)> Files, IReadOnlyList<string> AdrIds)
        Apply(ProjectStateDocument core, IReadOnlyList<AdrDraft> accepted, string adrDirRelative)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var files = new List<(string, string)>();
        var adrIds = new List<string>();
        var next = AdrProjection.NextAdrNumber(core);

        foreach (var d in accepted)
        {
            if (!byId.TryGetValue(d.ItemId, out var item) || item.Architecture is null) continue;
            if (!string.IsNullOrWhiteSpace(item.Architecture.AdrId)) continue;   // Idempotenz: nie doppelt
            var adrId = AdrProjection.AdrIdFor(next++);
            byId[d.ItemId] = item with { Architecture = item.Architecture with { AdrId = adrId, AdrStatus = AdrStatus.Accepted } };
            files.Add((Path.Combine(adrDirRelative, AdrProjection.FileNameFor(adrId, d.Title)),
                AdrProjection.Render(d, adrId, AdrStatus.Accepted, core)));
            adrIds.Add(adrId);
        }

        var updated = core with { Items = core.Items.Select(i => byId[i.ItemId]).ToList() };

        // Status-Folgen (②-Ablösung → ADR folgt): reine Projektion, kein Gate (Wahrheit ist schon autorisiert).
        foreach (var f in AdrProjection.StatusFollowUps(updated))
        {
            var item = byId[f.ItemId];
            byId[f.ItemId] = item with { Architecture = item.Architecture! with { AdrStatus = f.NewStatus } };
        }
        updated = updated with { Items = updated.Items.Select(i => byId[i.ItemId]).ToList() };

        files.Add((Path.Combine(adrDirRelative, "README.md"), AdrProjection.RenderIndex(updated)));
        files.Add((Path.Combine(Path.GetDirectoryName(adrDirRelative) ?? "docs", "architecture.md"), AdrProjection.RenderOverview(updated)));
        return (updated, files, adrIds);
    }
}
