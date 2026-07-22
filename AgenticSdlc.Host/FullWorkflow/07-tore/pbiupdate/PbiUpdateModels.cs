using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// Inc 1c-3: incrementeller PBI-Update. Aus der affected-items-view + dem Ingestion-Delta werden NUR betroffene
// PBIs gezielt aktualisiert (stabile IDs). Sparse: unberuehrte PBIs erscheinen NICHT im Plan.
public sealed record PbiStateChangePlanDocument(
    [property: JsonPropertyName("schemaVersion")] int SchemaVersion,
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("createdUtc")] DateTime CreatedUtc,
    [property: JsonPropertyName("sourceIngestionRun")] string SourceIngestionRun,
    [property: JsonPropertyName("operations")] IReadOnlyList<PbiStateChangeOperation> Operations)
{
    public const int CurrentSchemaVersion = 1;
}

public sealed record PbiStateChangeOperation(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("pbiId")] string? PbiId,                    // Ziel-PBI (EXTEND/MARK/BLOCK/SUPERSEDE)
    [property: JsonPropertyName("featureId")] string? FeatureId,            // NEW_PBI
    [property: JsonPropertyName("replacementRequirementId")] string? ReplacementRequirementId, // SUPERSEDE_PBI
    [property: JsonPropertyName("openDecisionRef")] string? OpenDecisionRef, // BLOCK_PBI
    [property: JsonPropertyName("rationale")] string Rationale);

public sealed record PbiUpdateGateReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<PbiUpdateGateIssue> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<PbiUpdateGateIssue> Warnings);

public sealed record PbiUpdateGateIssue(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] string Severity,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("requirementId")] string? RequirementId,
    [property: JsonPropertyName("pbiId")] string? PbiId,
    // R7: reparierbar = Platzierungs-Agent kann per GateFeedback fixen; sonst hard/needs_human. Loop-Primitive: Core.
    [property: JsonPropertyName("repairability")] string Repairability = Core.Repairability.Hard);

public static class PbiUpdateKind
{
    public const string NewPbi = "NEW_PBI";
    public const string ExtendPbi = "EXTEND_PBI";
    public const string MarkChanged = "MARK_CHANGED";
    public const string BlockPbi = "BLOCK_PBI";
    public const string SupersedePbi = "SUPERSEDE_PBI";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.Ordinal) { NewPbi, ExtendPbi, MarkChanged, BlockPbi, SupersedePbi };

    // Operationen mit Ziel-PBI (bestehend).
    public static readonly IReadOnlySet<string> RequirePbi =
        new HashSet<string>(StringComparer.Ordinal) { ExtendPbi, MarkChanged, BlockPbi, SupersedePbi };

    // Platzierung neuer Requirements (agentisch): EXTEND vs NEW.
    public static readonly IReadOnlySet<string> Placement =
        new HashSet<string>(StringComparer.Ordinal) { NewPbi, ExtendPbi };
}

// PBI-Status-Praezedenz beim Merge mehrerer Ursachen (MULTI_CAUSE_MERGE).
public static class PbiStatus
{
    public const string Active = "active";
    public const string NeedsClarify = "needs_clarify";
    public const string BlockedByDecision = "blocked_by_decision";
    public const string Superseded = "superseded";

    private static readonly Dictionary<string, int> Rank = new(StringComparer.OrdinalIgnoreCase)
    {
        [Active] = 0, [NeedsClarify] = 1, [Superseded] = 2, [BlockedByDecision] = 3
    };

    public static string Max(string a, string b) => Rank.GetValueOrDefault(a, 0) >= Rank.GetValueOrDefault(b, 0) ? a : b;
}
