namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class IssuePlanFactory
{
    public static IssuePlanDocument CreateOneIssuePerRequirement(IssuePlanningInput input, string sourceIssuePlanningInputPath)
    {
        var items = input.Items
            .OrderBy(i => i.RequirementId, StringComparer.Ordinal)
            .Select((item, index) => new IssuePlanItem(
                IssuePlanId: $"IPLAN-{index + 1:D3}",
                Operation: "CREATE",
                Title: MakeIssueTitle(item.Title),
                Description: BuildDescription(item),
                SourceRequirementIds: [item.RequirementId],
                AcceptanceCriteria: BuildAcceptanceCriteria(item),
                Labels: BuildLabels(item),
                Dependencies: [],
                Rationale: "Deterministischer Seed: ein Issue pro ready Requirement.",
                RequiresHumanReview: true,
                Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["seed"] = "one_issue_per_requirement",
                    ["l4OperationId"] = item.Provenance?.L4OperationId ?? ""
                })
                {
                    KnownContext = BuildKnownContext(item),
                    ImplementationHints = BuildImplementationHints(item),
                    OpenQuestions = BuildOpenQuestions(item),
                    Readiness = "ready_for_dev_with_context"
                })
            .ToList();

        return new IssuePlanDocument(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: input.ProjectId,
            BaselineId: input.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourceIssuePlanningInputPath,
            Items: items);
    }

    private static string MakeIssueTitle(string title)
    {
        var normalized = Normalize(title);
        if (normalized.Length <= 90) return normalized;
        var cut = normalized.LastIndexOf(' ', Math.Min(90, normalized.Length - 1));
        return normalized[..(cut > 40 ? cut : 90)].TrimEnd('.', ',', ';') + "...";
    }

    private static string BuildDescription(IssuePlanningInputItem item)
    {
        var provenance = item.Provenance;
        var sourceItems = item.SourceItemIds.Count == 0 ? "keine" : string.Join(", ", item.SourceItemIds);
        var ledgerClaims = provenance is null || provenance.LedgerClaimIds.Count == 0 ? "keine" : string.Join(", ", provenance.LedgerClaimIds);
        return $"""
               Requirement: {item.RequirementId}

               {item.Text}

               Source items: {sourceItems}
               Ledger claims: {ledgerClaims}
               """;
    }

    private static IReadOnlyList<string> BuildAcceptanceCriteria(IssuePlanningInputItem item)
        =>
        [
            $"Die Umsetzung erfuellt {item.RequirementId}: {Normalize(item.Title)}.",
            "Die Umsetzung ist anhand der verknuepften Requirement-Quelle rueckpruefbar."
        ];

    private static IReadOnlyList<string> BuildKnownContext(IssuePlanningInputItem item)
    {
        var context = new List<string>
        {
            $"Akzeptiertes Requirement {item.RequirementId}: {Normalize(item.Text)}"
        };
        if (item.SourceItemIds.Count > 0)
            context.Add($"Rueckfuehrbar auf Source Items: {string.Join(", ", item.SourceItemIds)}.");
        if (!string.IsNullOrWhiteSpace(item.OriginSummary))
            context.Add($"Herkunft: {Normalize(item.OriginSummary)}");
        return context;
    }

    private static IReadOnlyList<string> BuildImplementationHints(IssuePlanningInputItem item)
    {
        var text = $"{item.Title} {item.Text}".ToLowerInvariant();
        var hints = new List<string>();
        if (ContainsAny(text, ["login", "screen", "seite", "profil", "button", "navigation"]))
            hints.Add("Frontend/UI: View, Navigation, relevante Zustaende und Benutzerinteraktion konkretisieren.");
        if (ContainsAny(text, ["account", "admin", "rechte", "zugriff", "rollen", "berechtigung"]))
            hints.Add("Backend/Auth: Zugriff und Berechtigungspruefung an der fachlichen Rollenlogik ausrichten.");
        if (ContainsAny(text, ["bilder", "foto", "upload", "medien", "visuell"]))
            hints.Add("Daten/Medien: Quelle, Anzeige und Speicher-/Validierungsregeln fuer visuelle Inhalte klaeren oder bewusst begrenzen.");
        if (hints.Count == 0)
            hints.Add("Umsetzung in Frontend/Backend/Datenmodell soweit konkretisieren, wie es fuer dieses Requirement erforderlich ist.");
        return hints;
    }

    private static IReadOnlyList<string> BuildOpenQuestions(IssuePlanningInputItem item)
    {
        var questions = new List<string>();
        var text = $"{item.Title} {item.Text}".ToLowerInvariant();
        if (ContainsAny(text, ["bilder", "foto", "upload", "medien"]))
            questions.Add("Sind Erfassung, Upload, Speicherung und Bearbeitung von Bildern Teil dieses Issues oder nur die Anzeige vorhandener Bilder?");
        if (ContainsAny(text, ["rolle", "account", "zugriff", "rechte", "berechtigung"]))
            questions.Add("Welche Rollen duerfen diese Funktion lesen, erstellen, bearbeiten oder loeschen?");
        if (ContainsAny(text, ["seite", "screen", "profil", "navigation"]))
            questions.Add("Welche konkreten UI-Zustaende, leere Datenlagen und Fehlersituationen muessen im MVP abgedeckt werden?");
        return questions;
    }

    private static IReadOnlyList<string> BuildLabels(IssuePlanningInputItem item)
    {
        var text = $"{item.Title} {item.Text}".ToLowerInvariant();
        var labels = new List<string> { "requirements" };
        if (ContainsAny(text, ["login", "screen", "appbar", "seite", "profil", "button"])) labels.Add("frontend");
        if (ContainsAny(text, ["account", "admin", "rechte", "zugriff", "rollen"])) labels.Add("access-control");
        if (ContainsAny(text, ["android", "ios", "flutter", "dart"])) labels.Add("platform");
        return labels.Distinct(StringComparer.Ordinal).ToList();
    }

    private static bool ContainsAny(string text, IReadOnlyList<string> terms)
        => terms.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string value)
        => string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
