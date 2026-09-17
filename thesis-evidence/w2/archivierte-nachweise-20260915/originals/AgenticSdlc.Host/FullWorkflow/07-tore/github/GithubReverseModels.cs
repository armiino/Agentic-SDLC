using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// T3.5 — Reverse GitHub-Feedback-Ingestion (plan-tor3 §6, E4). GitHub-Zustand wird NIE automatisch Wahrheit:
// ein geschlossenes Issue erzeugt einen VORSCHLAG (StateChange), der erst nach menschlicher Verifikation greift.
// `done` entsteht AUSSCHLIESSLICH ueber ein freigegebenes PBI_DONE hier — nie aus issue-closed allein.
public sealed record GithubReversePlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("snapshot")] string? Snapshot,
    [property: JsonPropertyName("operations")] IReadOnlyList<GithubReverseOp> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record GithubReverseOp(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("issueNumber")] int IssueNumber,
    [property: JsonPropertyName("currentPbiStatus")] string CurrentPbiStatus,
    [property: JsonPropertyName("proposedPbiStatus")] string? ProposedPbiStatus,
    // E4: PBI_DONE ist IMMER verifikationspflichtig — der Mensch bestaetigt, dass die Arbeit wirklich fertig ist.
    [property: JsonPropertyName("requiresVerification")] bool RequiresVerification,
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record GithubReverseGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<GithubReverseGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<GithubReverseGateIssue> Warnings);

public sealed record GithubReverseGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("pbiId")] string? PbiId);

public sealed record GithubReverseDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<GithubReverseDecision> Decisions);

public sealed record GithubReverseDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason);

public sealed record GithubReverseApplyReport(
    [property: JsonPropertyName("accepted")] int Accepted,
    [property: JsonPropertyName("markedDone")] IReadOnlyList<string> MarkedDone,
    [property: JsonPropertyName("mappingsClosed")] IReadOnlyList<string> MappingsClosed,
    [property: JsonPropertyName("flagged")] IReadOnlyList<string> Flagged,
    [property: JsonPropertyName("skipped")] IReadOnlyList<string> Skipped);

public static class GithubReverseKind
{
    // Geschlossenes Issue + aktives/needs_clarify-PBI -> Vorschlag "done" (verifikationspflichtig, E4).
    public const string PbiDone = "PBI_DONE";
    // Geschlossenes Issue + PBI bereits done/superseded -> nur den Mapping-Status auf closed nachziehen (risikoarm).
    public const string MappingSyncClosed = "MAPPING_SYNC_CLOSED";
    // Wieder geoeffnetes Issue, aber PBI done -> Drift-Flag (Mensch entscheidet, kein Auto-Change).
    public const string FlagReopened = "FLAG_REOPENED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        { PbiDone, MappingSyncClosed, FlagReopened };
}
