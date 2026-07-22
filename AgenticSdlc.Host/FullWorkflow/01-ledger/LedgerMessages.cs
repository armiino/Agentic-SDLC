using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

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

/// <summary>Transportiert den Candidate-Ledger und den Canonical-Draft in die Coverage-Reparatur.</summary>
public sealed record CandidateAndCanonicalLedgerMessage(
    IReadOnlyList<SemanticLedgerEntry> Candidates,
    IReadOnlyList<SemanticLedgerEntry> CanonicalDraft);

/// <summary>Kanonischer Ledger (Stufe 2 → Stufe 3), inkl. Cluster-Trace an den Einträgen.</summary>
public sealed record CanonicalLedgerMessage(IReadOnlyList<SemanticLedgerEntry> Entries);

/// <summary>
/// Validierter Ledger (Stufe 3 → optionaler Adjudikationsschritt). Trägt den validierten Ledger typisiert
/// über die Edge; der Adjudikations-Executor operiert für Prepare/Apply zusätzlich auf den kanonischen
/// On-Disk-Artefakten des Runs (step-03/step-03b), damit die bestehenden Runner 1:1 wiederverwendbar bleiben.
/// </summary>
public sealed record ValidatedLedgerMessage(ValidatedLedger Ledger);
