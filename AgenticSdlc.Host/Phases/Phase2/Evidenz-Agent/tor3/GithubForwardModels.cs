using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// T3.3 — Forward-Maker: der github-sync-Delta wird gegen GitHub abgeglichen. Der Plan ist ein VORSCHLAG
// (kein Write): erst Gate, dann HumanReview, dann gated Apply (T3.4). Ein Op je Delta-PBI.
public sealed record GithubForwardPlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourcePbiUpdateRun")] string SourcePbiUpdateRun,
    [property: JsonPropertyName("repository")] string? Repository,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubForwardOp> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubForwardOp(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("targetIssueNumber")] int? TargetIssueNumber,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("body")] string? Body,
    [property: JsonPropertyName("labels")] IReadOnlyList<string>? Labels,
    // Rev-3-Invariante: CREATE_ISSUE MUSS die tatsaechlich ausgefuehrten Suchbegriffe + eine Ergebnis-Evidenz
    // ("keine plausiblen Treffer") mitfuehren — sonst lehnt das Gate ab (CREATE_WITHOUT_SEARCH_EVIDENCE).
    [property: JsonPropertyName("searchedQueries")] IReadOnlyList<string>? SearchedQueries,
    [property: JsonPropertyName("searchEvidence")] string? SearchEvidence,
    // Belegpflicht: Anker der Zuordnung (mapping ODER Requirement-IDs/Suchtreffer).
    [property: JsonPropertyName("anchor")] string? Anchor,
    [property: JsonPropertyName("rationale")] string Rationale,
    [property: JsonPropertyName("origin")] string Origin);   // "deterministic" | "agent"

public sealed record GithubForwardGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<GithubForwardGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<GithubForwardGateIssue> Warnings);

public sealed record GithubForwardGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("pbiId")] string? PbiId);

public sealed record GithubForwardDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<GithubForwardDecision> Decisions);

public sealed record GithubForwardDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason);

// Der github-sync-Delta, den pbi-update-apply schreibt: { newPbis, updatedPbis, entries[] }.
public sealed record GithubSyncDeltaDocument(
    [property: JsonPropertyName("newPbis")] IReadOnlyList<string> NewPbis,
    [property: JsonPropertyName("updatedPbis")] IReadOnlyList<string> UpdatedPbis,
    [property: JsonPropertyName("entries")] IReadOnlyList<GithubSyncEntry> Entries);

public static class GithubForwardKind
{
    public const string CreateIssue = "CREATE_ISSUE";
    public const string UpdateIssue = "UPDATE_ISSUE";
    public const string Comment = "COMMENT";
    public const string Link = "LINK";
    public const string NoChange = "NO_CHANGE";
    public const string FlagDrift = "FLAG_DRIFT";
    public const string HoldBlocked = "HOLD_BLOCKED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        { CreateIssue, UpdateIssue, Comment, Link, NoChange, FlagDrift, HoldBlocked };

    // Ops, die auf ein bestehendes Issue zeigen muessen.
    public static readonly IReadOnlySet<string> RequireIssueTarget = new HashSet<string>(StringComparer.Ordinal)
        { UpdateIssue, Comment, Link, FlagDrift };

    // Die zwei agentischen Ausgaenge des unmapped-Falls (Suche → LINK oder CREATE).
    public static readonly IReadOnlySet<string> Agentic = new HashSet<string>(StringComparer.Ordinal)
        { Link, CreateIssue };
}
