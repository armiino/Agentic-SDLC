using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

// Produkt-Modell des l4-re-clarify-Knotens: ein Product Backlog als Menge stabiler PBIs.
// Siehe docs/17.07/plan-pb.md (Datenmodell) und plan-re-backlog.md (Prozess).
// PBI != Requirement != Issue: das PBI ist die priorisierbare fachliche Planungseinheit.

public sealed record ProductBacklogDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("backlogId")] string BacklogId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourcePath")] string SourcePath,
    [property: JsonPropertyName("items")] IReadOnlyList<ProductBacklogItem> Items)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record ProductBacklogItem(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("identityKey")] string IdentityKey,
    [property: JsonPropertyName("version")] int Version,
    // delivery | clarification | deferred | out_of_scope
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("requirementIds")] IReadOnlyList<string> RequirementIds)
{
    // Story-Form: Als <Rolle> will ich <Ziel>, damit <Nutzen>.
    [property: JsonPropertyName("goal")]
    public string? Goal { get; init; }

    [property: JsonPropertyName("scope")]
    public PbiScope Scope { get; init; } = new([], []);

    // Testbar/ueberpruefbar, format-agnostisch (Volere Fit Criterion).
    [property: JsonPropertyName("acceptanceCriteria")]
    public IReadOnlyList<string> AcceptanceCriteria { get; init; } = [];

    [property: JsonPropertyName("openDecisions")]
    public IReadOnlyList<PbiOpenDecision> OpenDecisions { get; init; } = [];

    // mvp | required_for_mvp | later | out_of_scope | undecided (Vorschlag, human-autorisiert)
    [property: JsonPropertyName("mvp")]
    public string? Mvp { get; init; }

    // Geordnetes Backlog, nicht nur Inventar (Vorschlag, human-autorisiert).
    [property: JsonPropertyName("priorityRank")]
    public int? PriorityRank { get; init; }

    [property: JsonPropertyName("dependencies")]
    public IReadOnlyList<string> Dependencies { get; init; } = [];

    // backlog_ready | ready_with_nonblocking_questions | blocked_by_decision
    [property: JsonPropertyName("readiness")]
    public string? Readiness { get; init; }

    [property: JsonPropertyName("traceability")]
    public PbiTraceability? Traceability { get; init; }

    [property: JsonPropertyName("provenance")]
    public PbiProvenance? Provenance { get; init; }
}

public sealed record PbiScope(
    [property: JsonPropertyName("inScope")] IReadOnlyList<string> InScope,
    [property: JsonPropertyName("outOfScope")] IReadOnlyList<string> OutOfScope);

public sealed record PbiOpenDecision(
    [property: JsonPropertyName("question")] string Question,
    // engineering_default | stakeholder_decision
    [property: JsonPropertyName("kind")] string Kind,
    // Evidenz-Achse: stated | derived | not_stated | weakly_inferred
    [property: JsonPropertyName("evidence")] string Evidence,
    // Aufloesungs-Achse: resolved | partial | proposed_default | open_decision | not_applicable
    [property: JsonPropertyName("resolution")] string Resolution,
    // Trifft die Frage den PBI-Kern? -> steuert readiness (blocked_by_decision)
    [property: JsonPropertyName("blocksScope")] bool BlocksScope)
{
    [property: JsonPropertyName("proposedResolution")]
    public string? ProposedResolution { get; init; }
}

public sealed record PbiTraceability(
    [property: JsonPropertyName("canonicalRequirementIds")] IReadOnlyList<string> CanonicalRequirementIds,
    [property: JsonPropertyName("l3")] IReadOnlyList<string> L3,
    [property: JsonPropertyName("l1Req")] IReadOnlyList<string> L1Req,
    [property: JsonPropertyName("claims")] IReadOnlyList<string> Claims);

public sealed record PbiProvenance(
    [property: JsonPropertyName("featureContextId")] string? FeatureContextId,
    [property: JsonPropertyName("clarifyRunId")] string? ClarifyRunId,
    [property: JsonPropertyName("humanDecisionIds")] IReadOnlyList<string> HumanDecisionIds,
    [property: JsonPropertyName("derivationHistory")] IReadOnlyList<PbiDerivationEntry> DerivationHistory);

public sealed record PbiDerivationEntry(
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("runId")] string RunId,
    // created | revised | recut | superseded
    [property: JsonPropertyName("change")] string Change);

public sealed record ReClarifyGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<ReClarifyGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<ReClarifyGateIssue> Warnings,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

public sealed record ReClarifyGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    // generischer Bezug: pbiId (PBI-Gate) oder clusterId (Cluster-Gate)
    [property: JsonPropertyName("subjectId")] string? SubjectId,
    [property: JsonPropertyName("requirementIds")] IReadOnlyList<string> RequirementIds);

// Analyse-Granularitaet (ASSEMBLE, agentisch): der Cluster-Agent gruppiert Requirements zu
// Feature-Clustern. coreRequirementIds = das Feature selbst; crossCuttingRequirementIds = Regeln,
// die dieses Feature MIT-betreffen, aber Kern eines anderen Clusters sind (mehrfach referenzierbar,
// keine Verschmelzung). Grundlage fuer den spaeteren CLARIFY/CUT-Schritt (-> PBIs).
public sealed record FeatureClusterSet(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("clusterSetId")] string ClusterSetId,
    [property: JsonPropertyName("projectId")] string ProjectId,
    [property: JsonPropertyName("baselineId")] string BaselineId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceBaselinePath")] string SourceBaselinePath,
    [property: JsonPropertyName("clusters")] IReadOnlyList<FeatureCluster> Clusters)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record FeatureCluster(
    [property: JsonPropertyName("clusterId")] string ClusterId,
    [property: JsonPropertyName("identityKey")] string IdentityKey,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("coreRequirementIds")] IReadOnlyList<string> CoreRequirementIds,
    [property: JsonPropertyName("crossCuttingRequirementIds")] IReadOnlyList<string> CrossCuttingRequirementIds,
    [property: JsonPropertyName("rationale")] string? Rationale);

// Feedback des zweiten Agenten (Checker) ueber die vorgeschlagenen Cluster.
// findings = Diagnose (Prosa); operations = konkrete, anwendbare Fixes (die UI zeigt sie als Vorschlaege).
public sealed record ClusterReviewReport(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("reviewId")] string ReviewId,
    [property: JsonPropertyName("verdict")] string Verdict,     // approve | revise
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("findings")] IReadOnlyList<ClusterReviewFinding> Findings)
{
    public const int CurrentSchemaVersion = 1;

    [property: JsonPropertyName("operations")]
    public IReadOnlyList<ClusterOperation> Operations { get; init; } = [];
}

public sealed record ClusterReviewFinding(
    [property: JsonPropertyName("clusterId")] string? ClusterId,
    // wrong_merge | wrong_split | miscategorized_crosscutting | granularity | other
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("severity")] string Severity,   // error | warning | info
    [property: JsonPropertyName("message")] string Message);

// Eine konkrete, deterministisch anwendbare Cluster-Korrektur. Der Mensch adjudiziert sie (apply/skip);
// der ReClarifyClusterApply fuehrt akzeptierte Operationen aus und prueft Coverage neu.
public sealed record ClusterOperation(
    [property: JsonPropertyName("opId")] string OpId,
    // add_crosscutting | remove_crosscutting | move_core | new_cluster | merge_clusters
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("rationale")] string? Rationale)
{
    [property: JsonPropertyName("requirementId")]
    public string? RequirementId { get; init; }

    [property: JsonPropertyName("fromClusterId")]
    public string? FromClusterId { get; init; }

    [property: JsonPropertyName("toClusterId")]
    public string? ToClusterId { get; init; }

    [property: JsonPropertyName("newClusterId")]
    public string? NewClusterId { get; init; }

    [property: JsonPropertyName("label")]
    public string? Label { get; init; }

    [property: JsonPropertyName("identityKey")]
    public string? IdentityKey { get; init; }

    [property: JsonPropertyName("coreRequirementIds")]
    public IReadOnlyList<string> CoreRequirementIds { get; init; } = [];
}
