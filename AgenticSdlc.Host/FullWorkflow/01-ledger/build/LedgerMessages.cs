using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Typisierte Edge-Payloads des Ledger-Workflows (Transport = Message-Passing, NICHT Shared State).
/// </summary>
/// <remarks>
/// Begründung der Transportwahl: siehe NextStep/LedgerExecutorWorkflow-smallVersion.md
/// (§„Transport zwischen Executoren"). Der Ledger ist eine lineare Transform-Pipeline — der Payload IST
/// das transformierte Objekt, also wandert er direkt als typisierte Message über die Edge. Shared State
/// (wie in Phase2B, wo State der Forschungsgegenstand war) wird hier bewusst NICHT genutzt.
/// </remarks>
public sealed record CandidateLedgerMessage(IReadOnlyList<SemanticLedgerEntry> Entries);

public sealed record AtomicUnitsMessage(IReadOnlyList<AtomicUnit> Units);

public sealed record UnitAwareCandidateLedgerMessage(
    IReadOnlyList<AtomicUnit> Units,
    IReadOnlyList<SemanticLedgerEntry> Entries);

public sealed record UnitCoverageReviewRequestMessage(
    IReadOnlyList<AtomicUnit> Units,
    IReadOnlyList<SemanticLedgerEntry> Entries,
    UnitCoverageGateResult Coverage);

public sealed record UnusedUnitTriageResultMessage(
    IReadOnlyList<AtomicUnit> Units,
    IReadOnlyList<SemanticLedgerEntry> Entries,
    UnitCoverageGateResult Coverage,
    IReadOnlyList<UnusedUnitTriageItem> Triage);

/// <summary>Transportiert den Candidate-Ledger und den Canonical-Draft in den Kanonisierungs-Check.
/// Schritt 5 ④: Attempt/Source/History reisen IN der Message (R-33-Muster — der Graph bleibt stateless);
/// die Canonicalization sendet mit den Defaults (Attempt 1, maker), der Repair mit Attempt+1/repair.</summary>
public sealed record CandidateAndCanonicalLedgerMessage(
    IReadOnlyList<SemanticLedgerEntry> Candidates,
    IReadOnlyList<SemanticLedgerEntry> CanonicalDraft,
    int Attempt = 1,
    string Source = "maker",
    IReadOnlyList<GateAttempt>? History = null);

/// <summary>Schritt 5 ④ — das typisierte Verdict des Kanonisierungs-Checks (Kanten-Prädikate routen darauf:
/// Repair → Repair-Knoten; Pass/Terminal entscheidet der Checker selbst).</summary>
public sealed record CanonicalGateVerdict(
    IReadOnlyList<SemanticLedgerEntry> Candidates,
    IReadOnlyList<SemanticLedgerEntry> CanonicalDraft,
    GateResult Report,
    GateDecision Decision,
    int Attempt,
    IReadOnlyList<GateAttempt> History);

/// <summary>Schritt 5 ④ / R-3 — Compare-Ergebnis (sanitisiert + vervollständigt) auf dem Weg zum SICHTBAREN
/// Referenz-Repair-Knoten; trägt die Compare-Stufen-Zähler für die unveränderte step-01d-Metrik durch.</summary>
public sealed record UnusedCompareDraftMessage(
    IReadOnlyList<AtomicUnit> RelevantUnits,
    IReadOnlyList<SemanticLedgerEntry> Candidates,
    IReadOnlyList<UnusedUnitLedgerCompareItem> Items,
    int ComparedByModel,
    int AutoCompletedNeedsHuman,
    IReadOnlyList<string> RemovedRelatedCandidateIds);

/// <summary>Kanonischer Ledger (Stufe 2 → Stufe 3), inkl. Cluster-Trace an den Einträgen.</summary>
public sealed record CanonicalLedgerMessage(IReadOnlyList<SemanticLedgerEntry> Entries);

/// <summary>
/// Validierter Ledger (Stufe 3 → optionaler Adjudikationsschritt). Trägt den validierten Ledger typisiert
/// über die Edge; der Adjudikations-Executor operiert für Prepare/Apply zusätzlich auf den kanonischen
/// On-Disk-Artefakten des Runs (step-03/step-03b), damit die bestehenden Runner 1:1 wiederverwendbar bleiben.
/// </summary>
public sealed record ValidatedLedgerMessage(ValidatedLedger Ledger);
