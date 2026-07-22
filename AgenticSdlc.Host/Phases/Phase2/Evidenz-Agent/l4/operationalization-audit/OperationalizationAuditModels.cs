using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record OperationalizationAuditDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("sourcePaths")] OperationalizationAuditSourcePaths SourcePaths,
    [property: JsonPropertyName("summary")] OperationalizationAuditSummary Summary,
    [property: JsonPropertyName("requirements")] IReadOnlyList<OperationalizationRequirementAudit> Requirements,
    [property: JsonPropertyName("findings")] IReadOnlyList<OperationalizationAuditFinding> Findings)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record OperationalizationAuditSourcePaths(
    [property: JsonPropertyName("canonicalBaselinePath")] string? CanonicalBaselinePath,
    [property: JsonPropertyName("readinessReportPath")] string? ReadinessReportPath,
    [property: JsonPropertyName("issuePlanningInputPath")] string IssuePlanningInputPath,
    [property: JsonPropertyName("acceptedIssuePlanPath")] string AcceptedIssuePlanPath,
    [property: JsonPropertyName("acceptedGithubActionPlanPath")] string AcceptedGithubActionPlanPath,
    [property: JsonPropertyName("githubWriteDryRunPath")] string GithubWriteDryRunPath,
    [property: JsonPropertyName("acceptedClarificationPlanPath")] string? AcceptedClarificationPlanPath);

public sealed record OperationalizationAuditSummary(
    [property: JsonPropertyName("canonicalRequirements")] int CanonicalRequirements,
    [property: JsonPropertyName("readinessItems")] int ReadinessItems,
    [property: JsonPropertyName("issuePlanningInputItems")] int IssuePlanningInputItems,
    [property: JsonPropertyName("issuePlanItems")] int IssuePlanItems,
    [property: JsonPropertyName("githubActions")] int GithubActions,
    [property: JsonPropertyName("clarificationPlanItems")] int ClarificationPlanItems,
    [property: JsonPropertyName("dryRunOperations")] int DryRunOperations,
    [property: JsonPropertyName("coveredIssuePlanningInputItems")] int CoveredIssuePlanningInputItems,
    [property: JsonPropertyName("uncoveredIssuePlanningInputItems")] int UncoveredIssuePlanningInputItems,
    [property: JsonPropertyName("requirementsWithDeliveryCoverage")] int RequirementsWithDeliveryCoverage,
    [property: JsonPropertyName("requirementsWithClarificationCoverage")] int RequirementsWithClarificationCoverage,
    [property: JsonPropertyName("requirementsWithoutOperationalCoverage")] int RequirementsWithoutOperationalCoverage,
    [property: JsonPropertyName("requirementsByGithubOperation")] IReadOnlyDictionary<string, int> RequirementsByGithubOperation,
    [property: JsonPropertyName("blockedRequirements")] int BlockedRequirements,
    [property: JsonPropertyName("noChangeRequirements")] int NoChangeRequirements,
    [property: JsonPropertyName("findingsBySeverity")] IReadOnlyDictionary<string, int> FindingsBySeverity,
    [property: JsonPropertyName("readyForGithubWrite")] bool ReadyForGithubWrite);

public sealed record OperationalizationRequirementAudit(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("readiness")] string? Readiness,
    [property: JsonPropertyName("inIssuePlanningInput")] bool InIssuePlanningInput,
    [property: JsonPropertyName("issuePlanIds")] IReadOnlyList<string> IssuePlanIds,
    [property: JsonPropertyName("clarificationPlanIds")] IReadOnlyList<string> ClarificationPlanIds,
    [property: JsonPropertyName("githubActionIds")] IReadOnlyList<string> GithubActionIds,
    [property: JsonPropertyName("githubOperations")] IReadOnlyList<string> GithubOperations,
    [property: JsonPropertyName("dryRunOperations")] IReadOnlyList<string> DryRunOperations,
    [property: JsonPropertyName("targetIssueNumbers")] IReadOnlyList<int> TargetIssueNumbers,
    [property: JsonPropertyName("operationalizationStatus")] string OperationalizationStatus,
    [property: JsonPropertyName("findingCodes")] IReadOnlyList<string> FindingCodes);

public sealed record OperationalizationAuditFinding(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("requirementId")] string? RequirementId,
    [property: JsonPropertyName("issuePlanId")] string? IssuePlanId,
    [property: JsonPropertyName("githubActionId")] string? GithubActionId);
