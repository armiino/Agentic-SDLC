using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record GithubIssueQualityFinding(
    string Code,
    string Severity,
    string Message,
    string ActionId,
    string IssuePlanId);

public static class GithubIssueQualityRules
{
    public static IReadOnlyList<GithubIssueQualityFinding> Check(GithubActionPlanItem action)
    {
        if (!IsWriteOperation(action.Operation))
        {
            return [];
        }

        var findings = new List<GithubIssueQualityFinding>();
        var title = action.Title.Trim();
        var body = action.Body.Trim();
        var knownContext = MetadataList(action, "knownContext");
        var implementationHints = MetadataList(action, "implementationHints");
        var openQuestions = MetadataList(action, "openQuestions");
        var readiness = MetadataString(action, "readiness");
        var isClarification = IsClarificationAction(action, readiness);
        var hasHumanDecision = action.Reason.Contains("Menschliche Entscheidung", StringComparison.OrdinalIgnoreCase);

        if (title.Length < 12)
        {
            findings.Add(Error("thin_title", "GitHub-Schreibaktion hat keinen belastbaren Titel.", action));
        }

        if (body.Length < 80)
        {
            findings.Add(Error("thin_body", "GitHub-Schreibaktion hat einen zu duennen Body; Entwickler koennen daraus keinen Scope ableiten.", action));
        }

        if (action.SourceRequirementIds.Count == 0)
        {
            findings.Add(Error("missing_traceability", "GitHub-Schreibaktion hat keine sourceRequirementIds.", action));
        }

        if (action.AcceptanceCriteria.Count < 2)
        {
            findings.Add(Error("thin_acceptance_criteria", "GitHub-Schreibaktion braucht mindestens zwei Acceptance Criteria oder explizite Klaerungs-Abnahmekriterien.", action));
        }

        if (knownContext.Count == 0)
        {
            findings.Add(Error("missing_known_context", "GitHub-Schreibaktion muss den bereits geklaerten Kontext aus der Kette mitnehmen.", action));
        }

        if (string.IsNullOrWhiteSpace(readiness))
        {
            findings.Add(Error("missing_readiness", "GitHub-Schreibaktion braucht eine Readiness-Einschaetzung.", action));
        }

        if (isClarification)
        {
            if (openQuestions.Count == 0 && !ContainsClarificationSignal(body))
            {
                findings.Add(Error("clarification_without_question", "Klaerungs-/DISK-Issue muss explizite offene Fragen oder eine klare Entscheidungsfrage enthalten.", action));
            }
        }
        else
        {
            if (implementationHints.Count == 0)
            {
                findings.Add(Error("missing_implementation_hints", "Dev-Issue braucht Umsetzungshinweise, damit nicht nur ein grober Titel nach GitHub geschrieben wird.", action));
            }

            if (string.Equals(readiness, "needs_refinement", StringComparison.OrdinalIgnoreCase) && !hasHumanDecision)
            {
                findings.Add(Error("unmarked_refinement_issue", "needs_refinement darf nicht als normales Dev-Issue geschrieben werden; als DISK/Klaerung markieren oder menschlich entscheiden.", action));
            }
        }

        return findings;
    }

    public static IReadOnlyList<string> MetadataList(GithubActionPlanItem action, string key)
    {
        if (!action.Metadata.TryGetValue(key, out var value) || value is null) return [];
        return value switch
        {
            IReadOnlyList<string> strings => strings.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
            IEnumerable<string> strings => strings.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
            JsonElement { ValueKind: JsonValueKind.Array } array => array.EnumerateArray()
                .Select(e => e.ValueKind == JsonValueKind.String ? e.GetString() : e.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!)
                .ToArray(),
            JsonElement { ValueKind: JsonValueKind.String } single when !string.IsNullOrWhiteSpace(single.GetString()) => [single.GetString()!],
            string single when !string.IsNullOrWhiteSpace(single) => [single],
            _ => []
        };
    }

    public static string? MetadataString(GithubActionPlanItem action, string key)
    {
        if (!action.Metadata.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            string text => string.IsNullOrWhiteSpace(text) ? null : text,
            JsonElement { ValueKind: JsonValueKind.String } element => string.IsNullOrWhiteSpace(element.GetString()) ? null : element.GetString(),
            _ => null
        };
    }

    private static bool IsWriteOperation(string operation)
        => operation.Equals("CREATE", StringComparison.OrdinalIgnoreCase)
           || operation.Equals("UPDATE", StringComparison.OrdinalIgnoreCase);

    private static bool IsClarificationAction(GithubActionPlanItem action, string? readiness)
        => action.Title.StartsWith("DISK:", StringComparison.OrdinalIgnoreCase)
           || action.Title.StartsWith("Klaerung:", StringComparison.OrdinalIgnoreCase)
           || action.Title.StartsWith("Klärung:", StringComparison.OrdinalIgnoreCase)
           || action.Labels.Any(l => l.Equals("clarification", StringComparison.OrdinalIgnoreCase)
                                     || l.Equals("discussion", StringComparison.OrdinalIgnoreCase))
           || string.Equals(readiness, "needs_refinement", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsClarificationSignal(string text)
        => text.Contains("klaer", StringComparison.OrdinalIgnoreCase)
           || text.Contains("klär", StringComparison.OrdinalIgnoreCase)
           || text.Contains("entscheid", StringComparison.OrdinalIgnoreCase)
           || text.Contains("offen", StringComparison.OrdinalIgnoreCase);

    private static GithubIssueQualityFinding Error(string code, string message, GithubActionPlanItem action)
        => new(code, "error", message, action.ActionId, action.IssuePlanId);
}
