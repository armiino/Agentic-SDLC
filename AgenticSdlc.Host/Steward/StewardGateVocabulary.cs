using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// Selbstbeschreibende Chat-Gates (18.08., Politur): die Gate-Vorlagen liefern ihre Entscheidungs-Optionen
/// SELBST mit (`optionen`: code + label) — die Labels kommen wörtlich aus den Review-Adaptern (= dieselbe
/// E0-Quelle, aus der die UI rendert), NICHT aus einer abgeschriebenen Prompt-Tabelle. Diese Klasse ist die
/// EINZIGE Stelle, die die Bahn-Code-Abweichungen des Chat-Vertrags kennt:
///   · Ingest-Chat spricht `reject` statt UI-`skip` (C5-Vertrag; die R-57-Naht liest beide als Nicht-Übernahme).
///   · decision-Chat spricht `resolve/&lt;OUTCOME&gt;`|`defer` statt der UI-Dropdown-Codes keep/adopt/refine/defer.
/// Ändert die UI ein Label, wandert es automatisch in die Chat-Vorlage mit — kein Prompt-Drift mehr.
/// </summary>
internal static class StewardGateVocabulary
{
    internal sealed record ChatOption(string Code, string Label);

    internal static IReadOnlyList<ChatOption> ForIngest(string kind) =>
        [.. IngestionReviewAdapter.DecisionOptions(kind)
            .Select(o => new ChatOption(o.Value == "skip" ? "reject" : o.Value, o.Label))];

    internal static IReadOnlyList<ChatOption> ForDecision() =>
        [.. PipelineDecisionReviewAdapter.ResolutionOptions.Select(o => new ChatOption(o.Value switch
        {
            PipelineDecisionReviewAdapter.ChoiceKeep => "resolve/KEEP_ORIGINAL",
            PipelineDecisionReviewAdapter.ChoiceAdopt => "resolve/ADOPT_NEW",
            PipelineDecisionReviewAdapter.ChoiceRefine => "resolve/REFINE",
            _ => "defer",
        }, o.Label))];

    internal static IReadOnlyList<ChatOption> ForForward() => Project(GithubForwardReviewAdapter.OpOptions);
    internal static IReadOnlyList<ChatOption> ForPbiOps() => Project(PbiUpdateReviewAdapter.OpDecisionOptions);
    internal static IReadOnlyList<ChatOption> ForPbiAlignments() => Project(PbiUpdateReviewAdapter.AlignmentOptions);

    private static IReadOnlyList<ChatOption> Project(IEnumerable<ReviewOption> options)
        => [.. options.Select(o => new ChatOption(o.Value, o.Label))];
}
