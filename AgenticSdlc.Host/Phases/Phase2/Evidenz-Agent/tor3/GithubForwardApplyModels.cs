using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.4 — Bericht des gated Write-Apply. dryRun=true: nur Vorschau (kein GitHub-Write, kein Core-Change).
// executed=true: die akzeptierten Ops wurden ausgefuehrt; das Mapping wurde ueber T3.1 in den Core geschrieben.
public sealed record GithubForwardApplyReport(
    [property: JsonPropertyName("dryRun")] bool DryRun,
    [property: JsonPropertyName("executed")] bool Executed,
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("sourcePlanId")] string SourcePlanId,
    [property: JsonPropertyName("startedUtc")] DateTime StartedUtc,
    [property: JsonPropertyName("completedUtc")] DateTime CompletedUtc,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubForwardApplyOp> Operations,
    [property: JsonPropertyName("summary")] GithubForwardApplySummary Summary);

public sealed record GithubForwardApplyOp(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("resultIssueNumber")] int? ResultIssueNumber,
    [property: JsonPropertyName("resultUrl")] string? ResultUrl,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("message")] string? Message);

public sealed record GithubForwardApplySummary(
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("created")] int Created,
    [property: JsonPropertyName("updated")] int Updated,
    [property: JsonPropertyName("commented")] int Commented,
    [property: JsonPropertyName("linked")] int Linked,
    [property: JsonPropertyName("flagged")] int Flagged,
    [property: JsonPropertyName("held")] int Held,
    [property: JsonPropertyName("noChange")] int NoChange,
    [property: JsonPropertyName("skipped")] int Skipped,
    [property: JsonPropertyName("rejected")] int Rejected,
    [property: JsonPropertyName("failed")] int Failed);
