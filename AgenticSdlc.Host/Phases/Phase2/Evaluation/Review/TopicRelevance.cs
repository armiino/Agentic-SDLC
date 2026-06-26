namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

/// <summary>
/// Z10 / v02 (Call 2, separat): die SEPARAT klassifizierte Artefakt-Relevanz EINES Topics. Spiegelt
/// <see cref="TopicItem.RelevantFor"/>, wird aber NICHT in die eingefrorene Fixture geschrieben — sie liegt
/// als Sidecar daneben (<c>input/topics/&lt;base&gt;.relevance.&lt;model&gt;.json</c>), damit v01 (relevantFor
/// aus dem breiten Extraktions-Call) sauberer Fallback bleibt und v01↔v02 fair vergleichbar ist (Test B).
/// <see cref="TopicId"/> ist der Join-Schluessel zur Fixture; <see cref="Reasons"/> haelt je gewaehltem
/// Artefakttyp die Begruendung (Auditierbarkeit der Zuordnung).
/// </summary>
public sealed record TopicRelevance(
    string TopicId,
    IReadOnlyList<string> RelevantFor,                 // requirements | risks | architecture | open-questions
    IReadOnlyDictionary<string, string> Reasons);      // artifactType -> Begruendung

/// <summary>
/// Z10 / v02: Ergebnis EINES separaten relevantFor-Klassifikations-Passes ueber eine bestehende
/// Topic-Fixture, plus Provenienz. <see cref="ExtractModel"/> = Modell der Primaerextraktion (aus der
/// Fixture, also die v01-relevantFor-Quelle), <see cref="ClassifyModel"/> = Modell dieses Call 2,
/// <see cref="PromptVersion"/> = ausgelagerte Prompt-Version. Die Fixture selbst bleibt unveraendert.
/// </summary>
public sealed record TopicRelevanceSet(
    string Transcript,
    string ExtractModel,
    string ClassifyModel,
    string PromptVersion,
    IReadOnlyList<TopicRelevance> Topics);
