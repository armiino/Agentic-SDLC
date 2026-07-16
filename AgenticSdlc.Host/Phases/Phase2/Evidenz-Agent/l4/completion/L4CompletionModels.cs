using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record L4CompletionInput(
    CanonicalRequirementsBaseline Baseline,
    L4ProvenanceMap? Provenance,
    RequirementsReadinessReport? Readiness,
    string RequirementsDocumentText,
    string SourceAppliedDir,
    string SourceRequirementsDocumentPath);

public sealed record L4AdequacyReport(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("reportId")] string ReportId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceRequirementsDocumentPath")] string SourceRequirementsDocumentPath,
    [property: JsonPropertyName("findings")] IReadOnlyList<L4AdequacyFinding> Findings,
    [property: JsonPropertyName("overallAssessment")] string OverallAssessment)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record L4AdequacyFinding(
    [property: JsonPropertyName("findingId")] string FindingId,
    [property: JsonPropertyName("section")] string Section,
    [property: JsonPropertyName("adequacy")] string Adequacy,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("whyItMatters")] string WhyItMatters,
    [property: JsonPropertyName("relatedRequirementIds")] IReadOnlyList<string> RelatedRequirementIds,
    [property: JsonPropertyName("recommendedAction")] string RecommendedAction,
    [property: JsonPropertyName("requiresHumanDecision")] bool RequiresHumanDecision);

public sealed record L4CompletionProposalDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("proposalId")] string ProposalId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceAdequacyReportId")] string SourceAdequacyReportId,
    [property: JsonPropertyName("sourceRequirementsDocumentPath")] string SourceRequirementsDocumentPath,
    [property: JsonPropertyName("items")] IReadOnlyList<L4CompletionProposalItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record L4CompletionProposalItem(
    [property: JsonPropertyName("proposalItemId")] string ProposalItemId,
    [property: JsonPropertyName("operation")] string Operation,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("problem")] string Problem,
    [property: JsonPropertyName("suggestedResolution")] string SuggestedResolution,
    [property: JsonPropertyName("whyItMatters")] string WhyItMatters,
    [property: JsonPropertyName("evidenceState")] string EvidenceState,
    [property: JsonPropertyName("sourceRequirementIds")] IReadOnlyList<string> SourceRequirementIds,
    [property: JsonPropertyName("sourceFindingIds")] IReadOnlyList<string> SourceFindingIds,
    [property: JsonPropertyName("requiresHumanDecision")] bool RequiresHumanDecision,
    [property: JsonPropertyName("metadata")] IReadOnlyDictionary<string, string> Metadata);

public sealed record L4CompletionGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<L4CompletionGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<L4CompletionGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record L4CompletionGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("itemId")] string? ItemId,
    [property: JsonPropertyName("sourceIds")] IReadOnlyList<string> SourceIds);
