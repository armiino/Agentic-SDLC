using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record IssuePlanningApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("acceptedPlanId")] string AcceptedPlanId,
    [property: JsonPropertyName("inputItems")] int InputItems,
    [property: JsonPropertyName("sourceItems")] int SourceItems,
    [property: JsonPropertyName("acceptedItems")] int AcceptedItems,
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("edited")] int Edited,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("revisionRequested")] int RevisionRequested,
    [property: JsonPropertyName("missingDecisions")] IReadOnlyList<string> MissingDecisions,
    [property: JsonPropertyName("gate")] IssuePlanGateReport Gate,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc);
