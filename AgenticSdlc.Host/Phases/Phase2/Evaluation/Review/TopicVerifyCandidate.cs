namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z8 (DISK-COV-5): ein vom begrenzten Completeness-Verifier gemeldeter <b>Kandidat</b> für ein
/// fachliches Thema, das in der Topic-Fixture fehlen könnte. Bewusst nur ein Kandidat, KEINE Wahrheit:
/// der Verifier kann selbst irren oder zu fein melden → niemals blind als reguläres Topic anhängen
/// (Provenienz bleibt über <see cref="TopicVerifyResult.VerifyModel"/> + den eigenen Dateinamen sichtbar).
/// Enthält absichtlich KEINE Scores/Severity/Repair-Entscheidung (Scope-Verengung ggü. der alten Jury).
/// </summary>
public sealed record MissingTopicCandidate(
    string Summary,
    IReadOnlyList<int> SourceTurns,
    IReadOnlyList<string> SuggestedRelevantFor,   // requirements | risks | architecture | open-questions
    string Reason);

/// <summary>
/// Ergebnis eines EINZELNEN Verify-Passes (Stop-Regel, DISK-COV-5): die Kandidatenliste plus Provenienz.
/// <see cref="ExtractModel"/> = Modell der Primärextraktion (aus der Fixture), <see cref="VerifyModel"/> =
/// Modell dieses Checks. Für den Capture-Recapture-Effekt sollten sich beide unterscheiden.
/// </summary>
public sealed record TopicVerifyResult(
    string Transcript,
    string ExtractModel,
    string VerifyModel,
    IReadOnlyList<MissingTopicCandidate> Candidates);
