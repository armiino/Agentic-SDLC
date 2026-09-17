using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// T2.1 — Decision-Ingestion, deterministischer Kern (KEIN LLM). Eingabe: strukturierte Auflösung(en) einer offenen
// Decision (decisionId + Outcome + optional neue Aussage). Daraus wird deterministisch ein StateChange-Plan
// abgeleitet (RESOLVE_DECISION + Downstream), der über Gate → HumanReview → Apply in den Core wandert.
public sealed record DecisionResolutionInput(
    [property: JsonPropertyName("resolutions")] IReadOnlyList<DecisionResolutionRequest> Resolutions);

public sealed record DecisionResolutionRequest(
    [property: JsonPropertyName("decisionId")] string DecisionId,
    [property: JsonPropertyName("outcome")] string Outcome,               // KEEP_ORIGINAL | ADOPT_NEW | REFINE
    [property: JsonPropertyName("newStatement")] string? NewStatement,    // Pflicht bei ADOPT_NEW/REFINE
    [property: JsonPropertyName("rationale")] string? Rationale);

public sealed record DecisionResolutionPlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("operations")] IReadOnlyList<DecisionResolutionOp> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

// Ein Op je Decision — die Derivation hat das Downstream (Ziel-Requirement + betroffene PBIs) schon berechnet,
// damit der Apply rein mechanisch ist.
public sealed record DecisionResolutionOp(
    [property: JsonPropertyName("decisionId")] string DecisionId,
    [property: JsonPropertyName("outcome")] string Outcome,
    [property: JsonPropertyName("targetRequirementId")] string? TargetRequirementId,   // 9g: null = zielloses Frage-DEC (nur KEEP/defer)
    [property: JsonPropertyName("newStatement")] string? NewStatement,
    [property: JsonPropertyName("affectedPbis")] IReadOnlyList<string> AffectedPbis,
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record DecisionResolutionGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<DecisionResolutionGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<DecisionResolutionGateIssue> Warnings);

public sealed record DecisionResolutionGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("decisionId")] string? DecisionId,
    // R7: reparierbar = Resolver-Agent kann per GateFeedback fixen (Outcome/neue Aussage); Input-/State-Fakten = hard.
    [property: JsonPropertyName("repairability")] string Repairability = Core.Repairability.Hard);

public sealed record DecisionResolutionDecisionsFile(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("reviewer")] string Reviewer,
    [property: JsonPropertyName("decisions")] IReadOnlyList<DecisionReviewDecision> Decisions);

public sealed record DecisionReviewDecision(
    [property: JsonPropertyName("opId")] string OpId,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("reason")] string? Reason);

public sealed record DecisionResolutionApplyReport(
    [property: JsonPropertyName("resolved")] IReadOnlyList<string> Resolved,
    [property: JsonPropertyName("supersededRequirements")] IReadOnlyList<string> SupersededRequirements,
    [property: JsonPropertyName("refinedRequirements")] IReadOnlyList<string> RefinedRequirements,
    [property: JsonPropertyName("newRequirements")] IReadOnlyList<string> NewRequirements,
    [property: JsonPropertyName("unblockedPbis")] IReadOnlyList<string> UnblockedPbis,
    [property: JsonPropertyName("swappedPbis")] IReadOnlyList<string> SwappedPbis,
    [property: JsonPropertyName("skipped")] IReadOnlyList<string> Skipped);

public static class DecisionOutcome
{
    public const string KeepOriginal = "KEEP_ORIGINAL";
    public const string AdoptNew = "ADOPT_NEW";
    public const string Refine = "REFINE";
    // C4-Kreislauf (22.08.): das ehrliche „Verwerfen" einer ARCHITEKTUR-Unklarheit — geklärt, aber BEWUSST
    // ohne Festlegung (Begründung Pflicht). Wirkt wie KEEP (schließen, keine Mutation) + stempelt den
    // Verzicht als Metadatum an die DEC (§3-Projektion: saubere Schließung statt ⚠). NUR für
    // aspect-markierte ziellose DECs erlaubt (Derivation/Gate wachen).
    public const string NoTruthNeeded = "NO_TRUTH_NEEDED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
        { KeepOriginal, AdoptNew, Refine, NoTruthNeeded };

    // Outcomes, die eine neue Aussage brauchen.
    public static readonly IReadOnlySet<string> RequireNewStatement = new HashSet<string>(StringComparer.Ordinal)
        { AdoptNew, Refine };
}

public static class DecisionRelations
{
    public const string Contradicts = "contradicts";
    // Rev 2: aufgelöst, NICHT hart gelöscht — bleibt als Historie im Graph, klärt aber den abgeleiteten Block
    // (github-sync/PbiUpdateDerivation filtern auf "contradicts").
    public const string ContradictsResolved = "contradicts_resolved";
    public const string Supersedes = "supersedes";
}

public static class DecisionStatus
{
    public const string Open = "open_decision";
    public const string Resolved = "resolved";
}
