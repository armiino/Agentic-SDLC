namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Deterministische Projektion ProductBacklogView -> IssuePlan (plan-pb §8).
// Die PBIs sind bereits geklaert + human-reviewed, daher ist die Issue-Abbildung mechanisch:
// 1 PBI -> 1 IssuePlanItem, pbiId PRIMAER (Metadata), sourceRequirementIds SEKUNDAER.
// Der IssuePlanner "repariert" hier kein RE mehr - er bildet nur operativ ab. (Buendeln N->1 = optional agentisch.)
public static class ReClarifyBacklogToIssuePlan
{
    public static IssuePlanDocument Project(ProductBacklogDocument backlog, string sourcePath, bool forceCreate = false)
    {
        var items = new List<IssuePlanItem>();
        var n = 0;
        foreach (var pbi in backlog.Items)
        {
            n++;
            var op = forceCreate ? "CREATE"
                : IsRetiredType(pbi.Type) ? "NO_CHANGE"
                : string.Equals(pbi.Readiness, "blocked_by_decision", StringComparison.OrdinalIgnoreCase) ? "NEEDS_REVIEW"
                : "CREATE";

            var openQuestions = pbi.OpenDecisions
                .Select(o => $"[{o.Kind}{(o.BlocksScope ? "/BLOCKS" : "")}] {o.Question}")
                .ToList();

            items.Add(new IssuePlanItem(
                IssuePlanId: $"IPLAN-{n:000}",
                Operation: op,
                Title: pbi.Title,
                Description: string.IsNullOrWhiteSpace(pbi.Goal) ? pbi.Title : pbi.Goal!,
                SourceRequirementIds: pbi.RequirementIds,
                AcceptanceCriteria: pbi.AcceptanceCriteria,
                Labels: [],
                Dependencies: pbi.Dependencies,
                Rationale: $"Aus PBI {pbi.PbiId} (type={pbi.Type}, mvp={pbi.Mvp ?? "-"}, rank={pbi.PriorityRank?.ToString() ?? "-"}).",
                RequiresHumanReview: op == "NEEDS_REVIEW",
                Metadata: new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    ["pbiId"] = pbi.PbiId,
                    ["identityKey"] = pbi.IdentityKey,
                    ["mvp"] = pbi.Mvp,
                    ["priorityRank"] = pbi.PriorityRank,
                    ["type"] = pbi.Type
                })
            {
                // bereits geklaerter Kontext = belegte Claims (fallback: inScope)
                KnownContext = pbi.Traceability?.Claims is { Count: > 0 } claims ? claims : (pbi.Scope?.InScope ?? []),
                // Umsetzungshinweise = konkreter In-Scope (fallback: Akzeptanzkriterien / Statement),
                // damit das GitHub-Issue-Quality-Gate nicht nur einen Titel bekommt.
                ImplementationHints = pbi.Scope?.InScope is { Count: > 0 } inScope ? inScope
                    : pbi.AcceptanceCriteria.Count > 0 ? pbi.AcceptanceCriteria
                    : [string.IsNullOrWhiteSpace(pbi.Goal) ? pbi.Title : pbi.Goal!],
                OpenQuestions = openQuestions,
                Readiness = pbi.Readiness
            });
        }

        return new IssuePlanDocument(
            SchemaVersion: IssuePlanDocument.CurrentSchemaVersion,
            PlanId: $"issue-plan-{DateTime.UtcNow:yyyyMMdd_HHmmss}",
            ProjectId: backlog.ProjectId,
            BaselineId: backlog.BaselineId,
            CreatedUtc: DateTime.UtcNow,
            SourceIssuePlanningInputPath: sourcePath,
            Items: items);
    }

    private static bool IsRetiredType(string? type)
        => string.Equals(type, "deferred", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "out_of_scope", StringComparison.OrdinalIgnoreCase);
}
