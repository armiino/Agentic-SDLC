using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record GithubReconciliationApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("acceptedPlanId")] string AcceptedPlanId,
    [property: JsonPropertyName("acceptedIssuePlanItems")] int AcceptedIssuePlanItems,
    [property: JsonPropertyName("sourceActions")] int SourceActions,
    [property: JsonPropertyName("acceptedActions")] int AcceptedActions,
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("edited")] int Edited,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("revisionRequested")] int RevisionRequested,
    [property: JsonPropertyName("missingDecisions")] IReadOnlyList<string> MissingDecisions,
    [property: JsonPropertyName("gate")] GithubActionPlanGateReport Gate,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc);
