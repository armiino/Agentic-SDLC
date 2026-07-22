using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.MakerChecker;

/// <summary>
/// Datenmodell des Maker-Checker-Contracts (MC0). Exakt nach <c>makerchecker/Contract.md</c> §2–§6.
/// Bewusst frei von MAF/Workflow-Abhängigkeiten: das ist die reine Vertrags-/Report-Form, die der
/// deterministische <see cref="ContractChecker"/> erzeugt und die später ein Executor transportiert.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContractSeverity { Info, Warning, Error }

/// <summary>Deterministisch aus den Violations abgeleitete Weiter-Entscheidung (Contract.md §3).
/// Grundlage der späteren MAF-Condition am ContractCheckerExecutor.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContractDecision { Pass, Repair, HumanReview, MaxIterationsReached }

/// <summary>Die kleinste prüfbare Vertragsverletzung (Contract.md §4). Nur <see cref="ContractSeverity.Error"/>
/// blockiert den Pass; <see cref="ContractSeverity.Warning"/> ist sichtbar, aber kein Repair-Grund.</summary>
public sealed record ContractViolation(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("severity")] ContractSeverity Severity,
    [property: JsonPropertyName("repairable")] bool Repairable,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("lineNumber")] int? LineNumber,
    [property: JsonPropertyName("artifactQuote")] string? ArtifactQuote,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("ledgerFacets")] IReadOnlyDictionary<string, string> LedgerFacets,
    [property: JsonPropertyName("suggestedAction")] string? SuggestedAction);

/// <summary>Kleine, gezielte Aufgabe für den späteren Repair-Executor (Contract.md §6): patcht nur die
/// betroffene Zeile, nie das ganze Artefakt. In MC0 nur erzeugt (kein Repair-Lauf).</summary>
public sealed record RepairItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("violationCode")] string ViolationCode,
    [property: JsonPropertyName("lineNumber")] int? LineNumber,
    [property: JsonPropertyName("currentText")] string CurrentText,
    [property: JsonPropertyName("claimIds")] IReadOnlyList<string> ClaimIds,
    [property: JsonPropertyName("instruction")] string Instruction,
    [property: JsonPropertyName("allowedFacetBounds")] IReadOnlyDictionary<string, string> AllowedFacetBounds,
    [property: JsonPropertyName("evidenceQuotes")] IReadOnlyList<string> EvidenceQuotes);

/// <summary>Das eine Ergebnis des Checkers (Contract.md §2). <c>Pass=true</c> = keine blockierenden
/// (Error-)Verstöße. Deterministisch, reproduzierbar, auditierbar — das Review-Zertifikat.</summary>
public sealed record ContractCheckReport(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("iteration")] int Iteration,
    [property: JsonPropertyName("decision")] ContractDecision Decision,
    [property: JsonPropertyName("violations")] IReadOnlyList<ContractViolation> Violations,
    [property: JsonPropertyName("repairItems")] IReadOnlyList<RepairItem> RepairItems,
    [property: JsonPropertyName("checks")] IReadOnlyDictionary<string, object> Checks);

/// <summary>Kanonische Violation-Codes (Contract.md §5). MC0 = die deterministischen; FACET_OVERSTATED
/// startet als Warning-Heuristik (bekannt hohe False-Positive-Rate bei starken Modellen → nicht blockierend,
/// spätere Bestätigung durch den bounded Critic MC3). EVIDENCE_UNSUPPORTED_DETAIL ist bewusst NICHT in MC0.</summary>
public static class ContractCodes
{
    public const string MissingCitation = "MISSING_CITATION";       // C1
    public const string UnknownClaimId = "UNKNOWN_CLAIM_ID";        // C2
    public const string WrongDisposition = "WRONG_DISPOSITION";     // C4
    public const string RequiredClaimUnused = "REQUIRED_CLAIM_UNUSED"; // C5
    public const string FacetOverstated = "FACET_OVERSTATED";       // C3 (Heuristik, Warning) / C7 (Critic, Error)
    public const string EvidenceUnsupportedDetail = "EVIDENCE_UNSUPPORTED_DETAIL"; // C7 (Critic, Error)
}
