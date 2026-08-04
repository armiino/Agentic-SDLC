using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Backlog;

public sealed record L4CompletionApplyReport(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("sourceCompletionDir")] string SourceCompletionDir,
    [property: JsonPropertyName("sourceBaselinePath")] string SourceBaselinePath,
    [property: JsonPropertyName("sourceProposalsPath")] string SourceProposalsPath,
    [property: JsonPropertyName("sourceDecisionsPath")] string SourceDecisionsPath,
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("edited")] int Edited,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("revise")] int Revise,
    [property: JsonPropertyName("addedOpenDecisions")] int AddedOpenDecisions,
    [property: JsonPropertyName("markedNeedsBreakdown")] int MarkedNeedsBreakdown,
    [property: JsonPropertyName("requirements")] int Requirements,
    [property: JsonPropertyName("openDecisions")] int OpenDecisions,
    [property: JsonPropertyName("traceLinks")] int TraceLinks,
    [property: JsonPropertyName("outputs")] IReadOnlyList<string> Outputs);
