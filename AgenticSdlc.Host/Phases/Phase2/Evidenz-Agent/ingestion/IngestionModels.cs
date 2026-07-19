using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// StateChangePlan = die vom Resolver vorgeschlagenen OPERATIONEN auf dem Core (plan-core-ingestion §6),
// vor Gate/HumanReview/Apply. Wiederverwendung des operationsbasierten Musters (wie ClusterOperation).
public sealed record StateChangePlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceMeetingDeltaPath")] string SourceMeetingDeltaPath,
    [property: JsonPropertyName("operations")] IReadOnlyList<StateChangeOperation> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

// Eine Operation je eingehendem Requirement. targetEntityId nur bei RESTATE/REFINE/SUPERSEDE/CONTRADICT.
public sealed record StateChangeOperation(
    [property: JsonPropertyName("incomingItemId")] string IncomingItemId,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("statement")] string Statement,
    [property: JsonPropertyName("targetEntityId")] string? TargetEntityId,
    [property: JsonPropertyName("featureKey")] string? FeatureKey,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record IngestionGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<IngestionGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<IngestionGateIssue> Warnings);

public sealed record IngestionGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("incomingItemId")] string? IncomingItemId,
    [property: JsonPropertyName("targetEntityId")] string? TargetEntityId);

// Das Operations-Vokabular (plan-core-ingestion §6). Klein starten; aus echten Meeting-Daten wachsen.
public static class StateChangeKind
{
    public const string Restate = "RESTATE";        // identisch -> Provenienz an bestehende Entitaet
    public const string Refine = "REFINE";          // konkretisiert DIESELBE Anforderung -> neue Version
    public const string New = "NEW";                // nichts Passendes -> neue Entitaet
    public const string NewRelated = "NEW_RELATED"; // fachlich NEU, aber im selben Feature (featureKey)
    public const string Supersede = "SUPERSEDE";    // ersetzt eine alte Anforderung
    public const string Contradict = "CONTRADICT";  // widerspricht bestehender Wahrheit -> Open Decision
    public const string AlreadyDecided = "ALREADY_DECIDED"; // schon als Open Decision erfasst -> No-Op (P2a)

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { Restate, Refine, New, NewRelated, Supersede, Contradict, AlreadyDecided };

    // Operationen, die eine bestehende Entitaet referenzieren MUESSEN (Requirement-Ziel).
    public static readonly IReadOnlySet<string> RequireTarget =
        new HashSet<string>(StringComparer.Ordinal) { Restate, Refine, Supersede, Contradict };

    // Operationen, die eine bestehende DECISION-Entitaet (DEC-*) referenzieren MUESSEN.
    public static readonly IReadOnlySet<string> RequireDecisionTarget =
        new HashSet<string>(StringComparer.Ordinal) { AlreadyDecided };

    // Operationen, die KEINE bestehende Entitaet referenzieren duerfen.
    public static readonly IReadOnlySet<string> ForbidTarget =
        new HashSet<string>(StringComparer.Ordinal) { New, NewRelated };
}
