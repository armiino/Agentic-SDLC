using System.Text;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

internal static class GithubWriteExecutor
{
    public static async Task<GithubWriteExecutionDocument> ExecuteAsync(
        GithubActionPlanDocument acceptedPlan,
        IGithubIssueClient client,
        GithubWriteExecuteOptions options,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.Repository))
            throw new InvalidOperationException("Repository fehlt.");

        var dryRun = GithubWriteDryRunFactory.Create(acceptedPlan);
        if (!dryRun.ReadyForExecute)
            throw new InvalidOperationException("Accepted GitHubActionPlan ist nicht write-ready. Fuehre github-write dry-run aus und klaere Blocker.");

        var started = DateTime.UtcNow;
        var operations = new List<GithubWriteExecutionOperation>();
        var mappings = new List<GithubIssueMapping>();

        foreach (var action in acceptedPlan.Actions.OrderBy(a => a.ActionId, StringComparer.Ordinal))
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                switch (action.Operation.ToUpperInvariant())
                {
                    case "CREATE":
                        var created = await client.CreateIssueAsync(
                            options.Repository,
                            action.Title,
                            BuildBody(action),
                            action.Labels,
                            ct).ConfigureAwait(false);
                        operations.Add(Success(action, created.IssueNumber, created.IssueUrl, "created"));
                        mappings.Add(Mapping(action, acceptedPlan, options.Repository, created.IssueNumber, created.IssueUrl, "CREATE"));
                        break;

                    case "UPDATE":
                        if (action.TargetIssueNumber is null) throw new InvalidOperationException("UPDATE braucht targetIssueNumber.");
                        var updated = await client.UpdateIssueAsync(
                            options.Repository,
                            action.TargetIssueNumber.Value,
                            action.Title,
                            BuildBody(action),
                            action.Labels,
                            ct).ConfigureAwait(false);
                        operations.Add(Success(action, updated.IssueNumber, updated.IssueUrl, "updated"));
                        mappings.Add(Mapping(action, acceptedPlan, options.Repository, updated.IssueNumber, updated.IssueUrl, "UPDATE"));
                        break;

                    case "REOPEN":
                        if (action.TargetIssueNumber is null) throw new InvalidOperationException("REOPEN braucht targetIssueNumber.");
                        var reopened = await client.ReopenIssueAsync(
                            options.Repository,
                            action.TargetIssueNumber.Value,
                            ct).ConfigureAwait(false);
                        operations.Add(Success(action, reopened.IssueNumber, reopened.IssueUrl, "reopened"));
                        mappings.Add(Mapping(action, acceptedPlan, options.Repository, reopened.IssueNumber, reopened.IssueUrl, "REOPEN"));
                        break;

                    case "LINK":
                        if (action.TargetIssueNumber is null) throw new InvalidOperationException("LINK braucht targetIssueNumber.");
                        operations.Add(Success(action, action.TargetIssueNumber, null, "linked"));
                        mappings.Add(Mapping(action, acceptedPlan, options.Repository, action.TargetIssueNumber.Value, null, "LINK"));
                        break;

                    case "NO_CHANGE":
                        operations.Add(new GithubWriteExecutionOperation(
                            ActionId: action.ActionId,
                            Operation: "NO_CHANGE",
                            IssuePlanId: action.IssuePlanId,
                            SourceRequirementIds: action.SourceRequirementIds,
                            TargetIssueNumber: action.TargetIssueNumber,
                            ResultIssueNumber: null,
                            ResultIssueUrl: null,
                            Status: "skipped",
                            Message: "NO_CHANGE erzeugt keinen GitHub-Write."));
                        break;

                    default:
                        throw new InvalidOperationException($"Operation darf nicht geschrieben werden: {action.Operation}");
                }
            }
            catch (Exception ex)
            {
                operations.Add(new GithubWriteExecutionOperation(
                    ActionId: action.ActionId,
                    Operation: action.Operation.ToUpperInvariant(),
                    IssuePlanId: action.IssuePlanId,
                    SourceRequirementIds: action.SourceRequirementIds,
                    TargetIssueNumber: action.TargetIssueNumber,
                    ResultIssueNumber: null,
                    ResultIssueUrl: null,
                    Status: "failed",
                    Message: ex.Message));
            }
        }

        var summary = new GithubWriteExecutionSummary(
            Actions: operations.Count,
            Created: CountStatus(operations, "created"),
            Updated: CountStatus(operations, "updated"),
            Linked: CountStatus(operations, "linked"),
            Reopened: CountStatus(operations, "reopened"),
            NoChange: operations.Count(o => o.Operation.Equals("NO_CHANGE", StringComparison.OrdinalIgnoreCase)),
            Skipped: CountStatus(operations, "skipped"),
            Failed: CountStatus(operations, "failed"));

        return new GithubWriteExecutionDocument(
            SchemaVersion: GithubWriteExecutionDocument.CurrentSchemaVersion,
            SourcePlanId: acceptedPlan.PlanId,
            Repository: options.Repository,
            StartedUtc: started,
            CompletedUtc: DateTime.UtcNow,
            Success: summary.Failed == 0,
            Operations: operations,
            Mappings: mappings,
            Summary: summary);
    }

    private static GithubWriteExecutionOperation Success(GithubActionPlanItem action, int? issueNumber, string? issueUrl, string status)
        => new(
            ActionId: action.ActionId,
            Operation: action.Operation.ToUpperInvariant(),
            IssuePlanId: action.IssuePlanId,
            SourceRequirementIds: action.SourceRequirementIds,
            TargetIssueNumber: action.TargetIssueNumber,
            ResultIssueNumber: issueNumber,
            ResultIssueUrl: issueUrl,
            Status: status,
            Message: null);

    private static GithubIssueMapping Mapping(
        GithubActionPlanItem action,
        GithubActionPlanDocument plan,
        string repository,
        int issueNumber,
        string? issueUrl,
        string operation)
        => new(
            IssuePlanId: action.IssuePlanId,
            SourceRequirementIds: action.SourceRequirementIds,
            Repository: repository,
            IssueNumber: issueNumber,
            IssueUrl: issueUrl,
            Operation: operation,
            CreatedFromPlanId: plan.PlanId,
            SyncedUtc: DateTime.UtcNow);

    private static string BuildBody(GithubActionPlanItem action)
    {
        var criteria = action.AcceptanceCriteria.Count == 0
            ? "- keine expliziten Acceptance Criteria"
            : string.Join('\n', action.AcceptanceCriteria.Select(c => "- " + c));
        var sources = action.SourceRequirementIds.Count == 0
            ? "keine"
            : string.Join(", ", action.SourceRequirementIds);
        var knownContext = MetadataList(action, "knownContext");
        var implementationHints = MetadataList(action, "implementationHints");
        var openQuestions = MetadataList(action, "openQuestions");
        var dependencies = MetadataList(action, "dependencies");
        var relatedClarificationIds = MetadataList(action, "relatedClarificationIds");
        var readiness = MetadataString(action, "readiness");

        var sb = new StringBuilder();
        sb.AppendLine(action.Body.Trim());
        sb.AppendLine();
        AppendOptionalSection(sb, "Bereits geklaert", knownContext);
        AppendOptionalSection(sb, "Erwartete Umsetzung", implementationHints);
        AppendOptionalSection(sb, "Offen / zu klaeren", openQuestions);
        AppendOptionalSection(sb, "Abhaengigkeiten", dependencies);
        AppendOptionalSection(sb, "Verwandte Klaerungen", relatedClarificationIds);
        if (!string.IsNullOrWhiteSpace(readiness))
        {
            sb.AppendLine("## Readiness");
            sb.AppendLine(readiness);
            sb.AppendLine();
        }
        sb.AppendLine("## Acceptance Criteria");
        sb.AppendLine(criteria);
        sb.AppendLine();
        sb.AppendLine("## Traceability");
        sb.AppendLine($"- Action: {action.ActionId}");
        sb.AppendLine($"- IssuePlanItem: {action.IssuePlanId}");
        sb.AppendLine($"- Source requirements: {sources}");
        sb.AppendLine();
        sb.AppendLine("## Rationale");
        sb.AppendLine(action.Reason);
        return sb.ToString().TrimEnd();
    }

    private static void AppendOptionalSection(StringBuilder sb, string title, IReadOnlyList<string> values)
    {
        if (values.Count == 0) return;
        sb.AppendLine($"## {title}");
        foreach (var value in values.Where(v => !string.IsNullOrWhiteSpace(v)))
            sb.AppendLine("- " + value.Trim());
        sb.AppendLine();
    }

    private static IReadOnlyList<string> MetadataList(GithubActionPlanItem action, string key)
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

    private static string? MetadataString(GithubActionPlanItem action, string key)
    {
        if (!action.Metadata.TryGetValue(key, out var value) || value is null) return null;
        return value switch
        {
            string text => string.IsNullOrWhiteSpace(text) ? null : text,
            JsonElement { ValueKind: JsonValueKind.String } element => string.IsNullOrWhiteSpace(element.GetString()) ? null : element.GetString(),
            _ => null
        };
    }

    private static int CountStatus(IReadOnlyList<GithubWriteExecutionOperation> operations, string status)
        => operations.Count(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
}
