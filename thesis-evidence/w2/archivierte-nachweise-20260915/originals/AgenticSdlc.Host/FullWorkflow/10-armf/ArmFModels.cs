using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.ArmF;

/// <summary>
/// W2 Arm F (Evaluationskonzept §11/§12): der GEMEINSAME bewertbare Outputvertrag des freien
/// Ziel-Agenten. Bewusst NEUTRAL — atomare Aussage · fachlicher Typ · Quellenreferenz (kanonische
/// AU-Locators) · explizit/abgeleitet · Unsicherheit. KEINE Ledger-internen Felder (Claims/Units-
/// Beziehungen, Checker-Verdicts, Candidate-IDs) — sonst wäre F ein Workflow im Prompt (§12.0-7).
/// </summary>
public sealed record ArmFStatement(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("statement")] string Statement,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("sourceUnitIds")] IReadOnlyList<string>? SourceUnitIds,
    [property: JsonPropertyName("derivation")] string Derivation,
    [property: JsonPropertyName("uncertainty")] string Uncertainty);

/// <summary>F-v2 (Konzept §11, 05.09.): vollständige Quellenbilanz — je AU genau eine Disposition.
/// `used` verlangt existierende Claim-IDs; `non_relevant`/`unresolved` tragen keine.</summary>
public sealed record ArmFBilanzEintrag(
    [property: JsonPropertyName("auId")] string AuId,
    [property: JsonPropertyName("disposition")] string Disposition,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string>? ClaimIds = null);

public sealed record ArmFResult(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("variant")] string Variant,
    [property: JsonPropertyName("statements")] IReadOnlyList<ArmFStatement> Statements,
    [property: JsonPropertyName("quellenbilanz")] IReadOnlyList<ArmFBilanzEintrag> Quellenbilanz);

/// <summary>
/// Ebene C (Evaluationskonzept §14.5): das rein BEOBACHTBARE Prozessprofil des Laufs —
/// wie hat der Agent den eingeräumten Handlungsspielraum tatsächlich genutzt? Selbstrevision =
/// ein späterer Draft (validate/submit) mit verändertem Inhalt gegenüber dem vorherigen —
/// NIE aus verborgenem Reasoning abgeleitet.
/// </summary>
public sealed record ArmFProcessProfile(
    [property: JsonPropertyName("modelRounds")] int ModelRounds,
    [property: JsonPropertyName("toolCalls")] int ToolCalls,
    [property: JsonPropertyName("sourceReads")] int SourceReads,
    [property: JsonPropertyName("validationCalls")] int ValidationCalls,
    [property: JsonPropertyName("submissions")] int Submissions,
    [property: JsonPropertyName("selfRevisions")] int SelfRevisions,
    [property: JsonPropertyName("toolBudget")] int ToolBudget,
    [property: JsonPropertyName("toolBudgetExhausted")] bool ToolBudgetExhausted,
    [property: JsonPropertyName("stopReason")] string StopReason);

/// <summary>Ebene B (Maschinenaufwand): Tokens stammen aus den OTel-Spans des Laufs (gen_ai.usage.*),
/// null wenn OTel im Lauf nicht aktiv war — dann trägt der Lauf keinen Token-Beleg (laut, nicht geraten).</summary>
public sealed record ArmFMetrics(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("wallMs")] long WallMs,
    [property: JsonPropertyName("statements")] int Statements,
    [property: JsonPropertyName("inputTokens")] long? InputTokens,
    [property: JsonPropertyName("outputTokens")] long? OutputTokens);
