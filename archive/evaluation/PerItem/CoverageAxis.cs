using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// C1: Coverage-Achse als <see cref="IReviewAxis"/>. Gated die eingefrorenen Topics auf Artefakt-Relevanz,
/// klassifiziert die relevanten via <see cref="TopicCoverageClassifier"/> (covered/partial/missing/…) und
/// mappt über <see cref="TopicCoverageMapper"/>. Identische Logik wie das <c>coverage-topics</c>-Kommando.
/// Liefert <c>null</c>, wenn für diesen Artefakttyp kein Topic relevant ist (Achse nicht anwendbar).
/// </summary>
public sealed class CoverageAxis : IReviewAxis
{
    private readonly TopicCoverageClassifier _classifier;
    private readonly string _judgeModel;

    public CoverageAxis(TopicCoverageClassifier classifier, string judgeModel)
    {
        _classifier = classifier;
        _judgeModel = judgeModel;
    }

    public string Name => "coverage";

    public async Task<ReviewResult?> EvaluateAsync(ReviewSubject subject, CancellationToken cancellationToken = default)
    {
        var relevant = subject.Topics
            .Where(t => t.RelevantFor.Any(r => string.Equals(r, subject.ArtifactType, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        if (relevant.Count == 0) return null;   // keine relevanten Topics → Achse nicht anwendbar

        var verdicts = await _classifier
            .ClassifyAsync(subject.ArtifactText, relevant, cancellationToken)
            .ConfigureAwait(false);

        return TopicCoverageMapper.Map(subject.ArtifactName, subject.ArtifactType, relevant, verdicts, _judgeModel);
    }
}
