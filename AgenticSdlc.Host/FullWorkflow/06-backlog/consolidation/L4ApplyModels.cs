using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public sealed record L4ApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourcePlanPath")] string SourcePlanPath,
    [property: JsonPropertyName("sourceDecisionsPath")] string SourceDecisionsPath,
    [property: JsonPropertyName("effectiveOperations")] int EffectiveOperations,
    [property: JsonPropertyName("requirements")] int Requirements,
    [property: JsonPropertyName("openDecisions")] int OpenDecisions,
    [property: JsonPropertyName("traceLinks")] int TraceLinks,
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("edited")] int Edited,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("revise")] int Revise,
    [property: JsonPropertyName("autoFallbackKeeps")] int AutoFallbackKeeps,
    [property: JsonPropertyName("gateReportPath")] string GateReportPath,
    [property: JsonPropertyName("outputs")] IReadOnlyList<string> Outputs);
