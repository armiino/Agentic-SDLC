using System.Text;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class RequirementsDocumentRenderer
{
    private static readonly (string Key, string Title)[] SectionOrder =
    [
        ("scope", "Scope und Nicht-Ziele"),
        ("roles", "Nutzerrollen und Berechtigungen"),
        ("functional", "Funktionale Anforderungen"),
        ("ui_ux", "UI/UX Anforderungen"),
        ("data_privacy", "Daten, Datenschutz und Lebenszyklus"),
        ("non_functional", "Nicht-funktionale Anforderungen"),
        ("platform", "Plattform und technische Rahmenbedingungen"),
        ("process", "Prozess- und Vorgehensanforderungen"),
        ("needs_breakdown", "Anforderungen mit Breakdown-Bedarf"),
        ("deferred", "Deferred / Optional"),
        ("open_decisions", "Offene Entscheidungen")
    ];

    public static string Render(
        CanonicalRequirementsBaseline baseline,
        L4ProvenanceMap? provenance,
        RequirementsReadinessReport? readiness)
    {
        var readinessByReq = readiness?.Items.ToDictionary(i => i.RequirementId, StringComparer.Ordinal)
                             ?? new Dictionary<string, RequirementReadinessItem>(StringComparer.Ordinal);
        var provenanceByReq = provenance?.Requirements.ToDictionary(p => p.RequirementId, StringComparer.Ordinal)
                              ?? new Dictionary<string, L4RequirementProvenance>(StringComparer.Ordinal);
        var classified = baseline.Requirements
            .Select(req => new ClassifiedRequirement(
                Requirement: req,
                Section: Classify(req, readinessByReq.GetValueOrDefault(req.RequirementId)),
                Readiness: readinessByReq.GetValueOrDefault(req.RequirementId),
                Provenance: provenanceByReq.GetValueOrDefault(req.RequirementId)))
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# Requirements Document");
        sb.AppendLine();
        sb.AppendLine($"Project: `{baseline.ProjectId}`");
        sb.AppendLine($"Baseline: `{baseline.BaselineId}`");
        sb.AppendLine($"Created UTC: `{baseline.CreatedUtc:O}`");
        sb.AppendLine($"Source ProjectState: `{baseline.SourceProjectStatePath}`");
        sb.AppendLine();
        sb.AppendLine("## Überblick");
        sb.AppendLine();
        sb.AppendLine($"- Requirements gesamt: `{baseline.Requirements.Count}`");
        sb.AppendLine($"- Aktive Requirements: `{baseline.Requirements.Count(r => r.Status == "active")}`");
        sb.AppendLine($"- Offene Entscheidungen: `{baseline.OpenDecisions.Count}`");
        if (readiness is not null)
        {
            sb.AppendLine($"- Ready for issue planning: `{readiness.Summary.ReadyForIssuePlanning}`");
            sb.AppendLine($"- Needs decision: `{readiness.Summary.NeedsDecision}`");
            sb.AppendLine($"- Needs breakdown: `{readiness.Summary.NeedsBreakdown}`");
            sb.AppendLine($"- Deferred / optional: `{readiness.Summary.DeferredOrOptional}`");
            sb.AppendLine($"- Traceability blockiert: `{readiness.Summary.BlockedByTraceability}`");
        }
        sb.AppendLine();
        sb.AppendLine("Dieses Dokument ist eine deterministisch gerenderte Projektion der kanonischen L4-Baseline. "
                      + "Die maschinenlesbare Wahrheit bleibt `canonical-requirements-baseline.json`.");
        sb.AppendLine();

        foreach (var (key, title) in SectionOrder)
        {
            var items = classified
                .Where(c => c.Section == key)
                .OrderBy(c => SortKey(c.Requirement.RequirementId), StringComparer.Ordinal)
                .ToList();
            if (items.Count == 0) continue;

            sb.AppendLine($"## {title}");
            sb.AppendLine();
            foreach (var item in items)
                RenderRequirement(sb, item);
        }

        RenderTraceabilitySummary(sb, baseline, provenance);
        return sb.ToString();
    }

    private static void RenderRequirement(StringBuilder sb, ClassifiedRequirement item)
    {
        var req = item.Requirement;
        sb.AppendLine($"### {req.RequirementId} - {req.Title}");
        sb.AppendLine();
        sb.AppendLine(req.Text);
        sb.AppendLine();
        sb.AppendLine($"- Status: `{req.Status}`");
        sb.AppendLine($"- Readiness: `{item.Readiness?.Readiness ?? "unknown"}`");
        sb.AppendLine($"- Source items: {FormatIds(req.SourceItemIds)}");
        if (item.Provenance is not null)
        {
            sb.AppendLine($"- L4 operation: `{item.Provenance.L4Operation ?? "unknown"}`"
                          + (string.IsNullOrWhiteSpace(item.Provenance.L4OperationId) ? "" : $" (`{item.Provenance.L4OperationId}`)"));
            if (item.Provenance.LedgerClaimIds.Count > 0)
                sb.AppendLine($"- Ledger claims: {FormatIds(item.Provenance.LedgerClaimIds)}");
            if (item.Provenance.L3CandidateIds.Count > 0)
                sb.AppendLine($"- L3 candidates: {FormatIds(item.Provenance.L3CandidateIds)}");
            if (item.Provenance.HumanDecisionIds.Count > 0)
                sb.AppendLine($"- Human decisions: {FormatIds(item.Provenance.HumanDecisionIds)}");
            if (item.Provenance.ReplacesProjectItemIds.Count > 0)
                sb.AppendLine($"- Replaces: {FormatIds(item.Provenance.ReplacesProjectItemIds)}");
        }
        if (item.Readiness?.Reasons.Count > 0)
        {
            sb.AppendLine("- Readiness notes:");
            foreach (var reason in item.Readiness.Reasons)
                sb.AppendLine($"  - `{reason.Code}` ({reason.Severity}): {reason.Message}");
        }
        sb.AppendLine();
    }

    private static void RenderTraceabilitySummary(StringBuilder sb, CanonicalRequirementsBaseline baseline, L4ProvenanceMap? provenance)
    {
        sb.AppendLine("## Traceability Summary");
        sb.AppendLine();
        sb.AppendLine("| Requirement | Project Items | Ledger Claims | L3 Candidates | Human Decisions |");
        sb.AppendLine("|---|---|---|---|---|");
        var byReq = provenance?.Requirements.ToDictionary(p => p.RequirementId, StringComparer.Ordinal)
                    ?? new Dictionary<string, L4RequirementProvenance>(StringComparer.Ordinal);
        foreach (var req in baseline.Requirements.OrderBy(r => SortKey(r.RequirementId), StringComparer.Ordinal))
        {
            byReq.TryGetValue(req.RequirementId, out var prov);
            var projectItems = prov?.SourceProjectItems.Select(i => i.ItemId).Distinct(StringComparer.Ordinal).ToList() ?? req.SourceItemIds.ToList();
            sb.Append("| ").Append(Code(req.RequirementId)).Append(' ');
            sb.Append("| ").Append(FormatIds(projectItems)).Append(' ');
            sb.Append("| ").Append(FormatIds(prov?.LedgerClaimIds ?? [])).Append(' ');
            sb.Append("| ").Append(FormatIds(prov?.L3CandidateIds ?? [])).Append(' ');
            sb.Append("| ").Append(FormatIds(prov?.HumanDecisionIds ?? [])).AppendLine(" |");
        }
        sb.AppendLine();
    }

    private static string Classify(CanonicalRequirement requirement, RequirementReadinessItem? readiness)
    {
        if (requirement.Status == "open_decision" || readiness?.Readiness == "needs_decision") return "open_decisions";
        if (readiness?.Readiness == "needs_breakdown") return "needs_breakdown";
        if (readiness?.Readiness == "deferred_or_optional") return "deferred";

        var text = $"{requirement.Title} {requirement.Text}".ToLowerInvariant();
        if (ContainsAny(text, ["nicht-ziel", "nicht ziel", "scope", "aus scope", "ausschliessen", "ausschließen", "internen gebrauch"])) return "scope";
        if (ContainsAny(text, ["rolle", "rollen", "account", "admin", "rechte", "zugriff", "user", "angehoerige", "angehörige", "bewohner-account"])) return "roles";
        if (ContainsAny(text, ["datenschutz", "einwilligung", "loesch", "lösch", "aufbewahrung", "export", "lebenszyklus", "daten"])) return "data_privacy";
        if (ContainsAny(text, ["login", "screen", "appbar", "button", "seite", "profil", "logo", "visuell", "ui", "ux"])) return "ui_ux";
        if (ContainsAny(text, ["schnell", "barriere", "qualitaet", "qualität", "performance", "sicher", "robust", "nutzbarkeit", "animation", "ablenkend", "eingabemethode", "sprachbefehl", "beeintraechtigt", "beeinträchtigt"])) return "non_functional";
        if (ContainsAny(text, ["flutter", "dart", "android", "ios", "plattform", "entwicklungsumgebung", "entwicklungswerkzeug", "versionen", "tool", "werkzeug"])) return "platform";
        if (ContainsAny(text, ["analyse", "beteiligten", "nutzer", "vorgehensweise", "prozess"])) return "process";
        return "functional";
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> terms)
        => terms.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));

    private static string FormatIds(IEnumerable<string> ids)
    {
        var values = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();
        return values.Count == 0 ? "" : string.Join(", ", values.Select(Code));
    }

    private static string Code(string value) => $"`{value}`";

    private static string SortKey(string itemId)
    {
        var prefix = new string(itemId.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(itemId.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        return $"{prefix}{(int.TryParse(digits, out var n) ? n : 0):D6}:{itemId}";
    }

    private sealed record ClassifiedRequirement(
        CanonicalRequirement Requirement,
        string Section,
        RequirementReadinessItem? Readiness,
        L4RequirementProvenance? Provenance);
}
