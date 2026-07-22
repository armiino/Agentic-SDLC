using AgenticSdlc.Host.Phases.Phase2.Evaluation.Review;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// C1: Grounding-Achse als <see cref="IReviewAxis"/>. Kapselt die Per-Item-Pipeline (B25–B28):
/// deterministisch in Units parsen (<see cref="ArtifactUnitParser"/>) → gegen das Transkript
/// klassifizieren (<see cref="UnitClassifier"/>) → in den Vertrag mappen (<see cref="PerItemReviewMapper"/>).
/// Identische Logik wie das <c>classify-units</c>-Kommando, nur als wiederverwendbare Achse statt CLI.
/// </summary>
public sealed class GroundingAxis : IReviewAxis
{
    private readonly UnitClassifier _classifier;
    private readonly string _judgeModel;

    public GroundingAxis(UnitClassifier classifier, string judgeModel)
    {
        _classifier = classifier;
        _judgeModel = judgeModel;
    }

    public string Name => "grounding";

    public async Task<ReviewResult?> EvaluateAsync(ReviewSubject subject, CancellationToken cancellationToken = default)
    {
        var units = ArtifactUnitParser.Parse(subject.ArtifactText);
        if (units.Count == 0) return null;   // kein prüfbarer Inhalt → Achse nicht anwendbar

        var verdicts = await _classifier
            .ClassifyAsync(subject.Transcript, subject.ArtifactType, units, cancellationToken)
            .ConfigureAwait(false);
        var byIndex = verdicts.ToDictionary(v => v.Index);

        // Der Klassifizierer liefert pro Unit-Index ein Verdikt (fehlende → unclassified, vgl. classify-units).
        var perItemUnits = units
            .Select(u => new PerItemUnit(u.Section, u.Text, byIndex[u.Index].Verdict, byIndex[u.Index].Reason))
            .ToList();

        return PerItemReviewMapper.Map(subject.ArtifactName, subject.ArtifactType, perItemUnits, coverage: null, _judgeModel);
    }
}
