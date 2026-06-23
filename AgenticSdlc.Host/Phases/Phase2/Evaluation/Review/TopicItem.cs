namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Stabiles, eingefrorenes Topic — die ledger-fähige Coverage-Vorstufe (DISK-COV V1, todos2 §2a).
/// EINMAL pro Transkript extrahiert und als Fixture eingefroren; über Artefakte/Runs wiederverwendet →
/// stabile IDs, kein Turn-Rauschen, kein Over-Counting. Später ohne Vertrags-Änderung zu einem
/// EvidenceItem erweiterbar.
/// </summary>
public sealed record TopicItem(
    string TopicId,
    string Summary,
    string Status,                        // z. B. resolved | unresolved | decision_open
    IReadOnlyList<int> SourceTurns,
    IReadOnlyList<string> RelevantFor);   // requirements | risks | architecture | open-questions

/// <summary>Coverage-Verdikt eines <see cref="TopicItem"/> gegen EIN Artefakt.</summary>
public sealed record TopicVerdict(
    string TopicId,
    string Verdict,                       // covered | partial | missing | not_applicable | unclassified
    string Reason);

/// <summary>Eingefrorene Topic-Fixture eines Transkripts (Z6.2): einmal extrahiert, wiederverwendet.</summary>
public sealed record TopicSet(
    string Transcript,
    string Model,
    IReadOnlyList<TopicItem> Topics);
