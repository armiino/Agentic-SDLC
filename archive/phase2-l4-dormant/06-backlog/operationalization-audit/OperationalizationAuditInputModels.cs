using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

// Typ-Schnitt 2026-07-23: operationalization-audit (LIVE, via open-requirements) liest historisch
// Reconciliation-/DryRun-Artefakte. Diese drei Input-Typen wurden aus research/ (jetzt archive/research/)
// in die Live-Seite gehoben, damit research aus dem Build fallen kann. Grab-Bag-Charakter dokumentiert (Log #6).
public sealed record GithubActionPlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceAcceptedIssuePlanPath")] string SourceAcceptedIssuePlanPath,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("actions")] IReadOnlyList<GithubActionPlanItem> Actions)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubActionPlanItem(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("acceptanceCriteria")] IReadOnlyList<string> AcceptanceCriteria,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("reason")] string Reason,
    [property: JsonPropertyName("requiresHumanReview")] bool RequiresHumanReview,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, object?> Metadata);

public sealed record GithubWriteDryRunOperation(
    [property: JsonPropertyName("actionId")] string ActionId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("issuePlanId")] string IssuePlanId,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("wouldCall")] string WouldCall,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("labels")] IReadOnlyList<string> Labels,
    [property: JsonPropertyName("blocked")] bool Blocked,
    [property: JsonPropertyName("blockReason")] string? BlockReason,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds);

public sealed record GithubActionPlanGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<GithubActionPlanGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<GithubActionPlanGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record GithubActionPlanGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("actionId")] string? ActionId,
    [property: JsonPropertyName("issuePlanId")] string? IssuePlanId);

public sealed record GithubWriteDryRunDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("readyForExecute")] bool ReadyForExecute,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubWriteDryRunOperation> Operations,
    [property: JsonPropertyName("summary")] GithubWriteDryRunSummary Summary)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubWriteDryRunSummary(
    [property: JsonPropertyName("actions")] int Actions,
    [property: JsonPropertyName("create")] int Create,
    [property: JsonPropertyName("update")] int Update,
    [property: JsonPropertyName("link")] int Link,
    [property: JsonPropertyName("reopen")] int Reopen,
    [property: JsonPropertyName("noChange")] int NoChange,
    [property: JsonPropertyName("needsReview")] int NeedsReview,
    [property: JsonPropertyName("blocked")] int Blocked);
