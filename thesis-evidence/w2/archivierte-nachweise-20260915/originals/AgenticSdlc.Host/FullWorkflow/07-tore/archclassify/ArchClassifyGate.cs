using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.ArchClassify;

/// <summary>R-11 A2 — die Konsum-Rollen (Wire englisch, Haus-Regel; deutsche Labels nur in der UI).</summary>
public static class ArchRoles
{
    public const string Constraint = "constraint"; // wirkt IN den Issues der betroffenen PBIs (A3)
    public const string Work = "work";             // wird ein EIGENES technisches PBI (A4)
    public const string Design = "design";         // wird ein ADR-Dokument (A5)
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal) { Constraint, Work, Design };
}

/// <summary>Ein aktives PBI als Ziel-Option (id + Titel) — reist in den Strip-Messages und in der
/// Review-Anfrage, damit Agent UND Mensch dieselbe Ziel-Menge sehen (① gebündeltes Wirkungs-Gate).</summary>
public sealed record ArchPbiOption(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title);

/// <summary>Ein Klassifikations-Vorschlag des Agenten: Rollen (1..N, T2.14 komponierend) + Begründung.</summary>
public sealed record ArchClassifyProposal(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName("rationale")] string Rationale,
    // R-11 ① (06.08., gebündeltes Wirkungs-Gate): vorgeschlagene Ziel-PBIs (constraint-Wirkung; bei work reine
    // Anzeige „gehört zu" — die PBI-Erzeugung bleibt A4). Optional: Alt-Pläne bleiben lesbar.
    [property: JsonPropertyName("targetPbiIds")] IReadOnlyList<string>? TargetPbiIds = null);

public sealed record ArchClassifyGateIssue(string Code, string Repairability, string Message, string? ItemId);
public sealed record ArchClassifyGateReport(bool Pass, IReadOnlyList<ArchClassifyGateIssue> Errors)
{
    public bool HasRepairable => Errors.Any(e => e.Repairability == Repairability.Repairable);
}

/// <summary>
/// R-11 A2 — das DETERMINISTISCHE Gate des Klassifikations-Loops (GateLoop-Form, D-4): prüft die
/// Agent-Vorschläge gegen die unklassifizierten Core-arch-Items. Quell-agnostisch (T2.21): die
/// Item-Menge kommt aus dem CORE (aktive architecture-Items ohne Payload), nicht aus dem Delta.
/// </summary>
public static class ArchClassifyGate
{
    /// <summary>Die zu klassifizierende Menge: aktive architecture-Items OHNE Rollen-Payload (Idempotenz:
    /// bereits Klassifiziertes wird nie erneut vorgelegt — „zweimal = gleicher Core", T2.21).</summary>
    public static IReadOnlyList<ProjectStateItem> Unclassified(ProjectStateDocument core)
        => core.Items
            .Where(i => AspectIngestionProfile.Architecture.Matches(i))
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .Where(i => i.Architecture is null)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .ToList();

    /// <summary>Die Ziel-Menge des Wirkungs-Gates (①): aktive PBIs mit Titel — EINE Quelle für Bridges
    /// (Work-Message), Gate-Prüfung und Review-UI.</summary>
    public static IReadOnlyList<ArchPbiOption> ActivePbis(ProjectStateDocument core)
        => core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(i => new ArchPbiOption(i.ItemId, i.Pbi?.Title ?? i.Text))
            .ToList();

    // A2-①: activePbiIds = deterministische Ziel-Prüfmenge (leere Menge erlaubt = Alt-Aufrufer/Tests ohne Ziele).
    public static ArchClassifyGateReport Check(IReadOnlyList<ProjectStateItem> unclassified, IReadOnlyList<ArchClassifyProposal> proposals,
        IReadOnlySet<string>? activePbiIds = null)
    {
        var errors = new List<ArchClassifyGateIssue>();
        var targetIds = unclassified.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);

        // Coverage: GENAU EIN Vorschlag je unklassifiziertem Item (Muster IngestionGate).
        var byId = proposals.GroupBy(p => p.ItemId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        foreach (var id in targetIds.Where(id => !byId.ContainsKey(id)))
            errors.Add(new("MISSING_CLASSIFICATION", Repairability.Repairable, $"arch-Item '{id}' hat keinen Rollen-Vorschlag.", id));
        foreach (var (id, count) in byId.Where(kv => kv.Value > 1))
            errors.Add(new("DUPLICATE_CLASSIFICATION", Repairability.Repairable, $"arch-Item '{id}' hat {count} Vorschläge (genau einer erlaubt).", id));
        foreach (var p in proposals.Where(p => !targetIds.Contains(p.ItemId)))
            errors.Add(new("UNKNOWN_TARGET", Repairability.Repairable, $"Vorschlag für '{p.ItemId}' — kein unklassifiziertes aktives arch-Item.", p.ItemId));

        foreach (var p in proposals)
        {
            if (p.Roles.Count == 0)
                errors.Add(new("NO_ROLE", Repairability.Repairable, $"'{p.ItemId}': mindestens eine Rolle erforderlich (Rollen komponieren, T2.14).", p.ItemId));
            foreach (var r in p.Roles.Where(r => !ArchRoles.All.Contains(r)))
                errors.Add(new("INVALID_ROLE", Repairability.Repairable, $"'{p.ItemId}': Rolle '{r}' ausserhalb {{constraint|work|design}}.", p.ItemId));
            if (p.Roles.Distinct(StringComparer.Ordinal).Count() != p.Roles.Count)
                errors.Add(new("DUPLICATE_ROLE", Repairability.Repairable, $"'{p.ItemId}': Rollen doppelt genannt.", p.ItemId));
            if (string.IsNullOrWhiteSpace(p.Rationale))
                errors.Add(new("MISSING_RATIONALE", Repairability.Repairable, $"'{p.ItemId}': Begründung fehlt (Beleg-Pflicht).", p.ItemId));

            // ① Ziel-PBIs: deterministisch prüfbar — jedes genannte Ziel muss ein AKTIVES PBI sein.
            foreach (var t in (p.TargetPbiIds ?? []).Where(t => activePbiIds is not null && !activePbiIds.Contains(t)))
                errors.Add(new("UNKNOWN_TARGET_PBI", Repairability.Repairable, $"'{p.ItemId}': Ziel-PBI '{t}' existiert nicht oder ist nicht aktiv.", p.ItemId));
            if (p.Roles.Contains(ArchRoles.Constraint, StringComparer.Ordinal) && (p.TargetPbiIds is null || p.TargetPbiIds.Count == 0))
                errors.Add(new("CONSTRAINT_WITHOUT_TARGETS", Repairability.Repairable, $"'{p.ItemId}': Rolle constraint ohne Ziel-PBIs — wen schränkt der Rahmen ein? (Falls wirklich niemanden: Rolle überdenken.)", p.ItemId));
        }

        return new ArchClassifyGateReport(errors.Count == 0, errors);
    }

    /// <summary>Exakt die GateLoop-Entscheidungsform (R7/R-33).</summary>
    public static GateDecision Decide(ArchClassifyGateReport report, int attempt, int maxAttempts)
        => GateLoop.Decide(report.Pass, report.HasRepairable, attempt, maxAttempts);

    /// <summary>Der deterministische Apply-Kern (①, gebündeltes Wirkungs-Gate): schreibt die autorisierten
    /// Rollen als typisiertes Payload UND — nur bei Rolle constraint — die vom Menschen bestätigten
    /// <c>constrained_by</c>-Relationen (PBI→ARCH; Spec I1, Kangal wacht am Save). Idempotent: bereits
    /// klassifizierte Items bleiben unberührt, doppelte Relationen entstehen nicht; unbekannte Ziel-PBIs
    /// werden NIE still geschrieben (Gate prüft; der Apply filtert defensiv erneut).</summary>
    public static ProjectStateDocument Apply(ProjectStateDocument core, IReadOnlyList<ArchClassifyProposal> accepted)
    {
        var byId = accepted.ToDictionary(p => p.ItemId, StringComparer.Ordinal);
        var items = core.Items.Select(i =>
            byId.TryGetValue(i.ItemId, out var p) && i.Architecture is null
                ? i with { Architecture = new ArchitecturePayload(p.Roles.Distinct(StringComparer.Ordinal).ToList(), p.Rationale) }
                : i).ToList();

        var activePbis = items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Where(i => i.ReadStatus().Validity == Validity.Active)
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);
        var existing = core.Relations
            .Where(r => r.RelationType == "constrained_by")
            .Select(r => (r.FromId, r.ToId))
            .ToHashSet();
        var relations = core.Relations.ToList();
        foreach (var p in accepted.Where(p => p.Roles.Contains(ArchRoles.Constraint, StringComparer.Ordinal)))
            foreach (var pbiId in (p.TargetPbiIds ?? []).Where(activePbis.Contains))
                if (existing.Add((pbiId, p.ItemId)))
                    relations.Add(new ProjectStateRelation(pbiId, p.ItemId, "constrained_by", "arch-classify",
                        new Dictionary<string, string>()));

        return core with { Items = items, Relations = relations };
    }
}
