using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Einmaliger Seed: hebt einen bestehenden ProjectState-Rebuild als Ausgangs-WAHRHEIT in den Core.
// Deterministisch, kein LLM: setzt je Item einen identityKey (Retrieval-Anker) + leere Historie und
// stellt Version>=1 sicher. Relations/Provenance/Proposals/Sources bleiben unveraendert erhalten.
// 9g (Zwei-Bahnen-Regel): open_question-Items werden NICHT als Items geseedet, sondern ueber den GETEILTEN
// MeetingQuestionMint zu offenen DECs — EIN Unklarheits-Zuhause, auch in der Bootstrap-Bahn. Governance-Deckung:
// das Adjudikations-Gate hat die Frage-Claims freigegeben, bevor sie das Delta erreichen.
public static class CoreSeeder
{
    public sealed record Report(int Total, int Requirements, int Architecture, int Other, int WithIdentityKey, int QuestionDecs = 0);

    public static (ProjectStateDocument Core, Report Report) Seed(ProjectStateDocument source, string sourceRun = "core-seed")
    {
        // Slice S ④: Risiken fahren dieselbe Unklarheits-Spur wie Fragen (QuestionLane) — auch im Bootstrap.
        var questions = source.Items.Where(i => QuestionLane.Carries(i.ItemType)).ToList();
        var items = source.Items.Where(i => !QuestionLane.Carries(i.ItemType)).Select(SeedItem).ToList();
        var core = source with
        {
            SchemaVersion = ProjectStateDocument.CurrentSchemaVersion,
            Items = items
        };

        var questionDecs = 0;
        if (questions.Count > 0)
        {
            var (withDecs, minted, _) = MeetingQuestionMint.Mint(core, questions, sourceRun);
            core = withDecs;
            questionDecs = minted.Count;
        }

        var report = new Report(
            Total: core.Items.Count,
            Requirements: items.Count(i => Is(i, "requirement")),
            Architecture: items.Count(i => Is(i, "architecture")),
            Other: items.Count(i => !Is(i, "requirement") && !Is(i, "architecture")),
            WithIdentityKey: core.Items.Count(i => !string.IsNullOrEmpty(i.IdentityKey)),
            QuestionDecs: questionDecs);

        return (core, report);
    }

    private static ProjectStateItem SeedItem(ProjectStateItem item) => item with
    {
        Version = item.Version <= 0 ? 1 : item.Version,
        IdentityKey = string.IsNullOrWhiteSpace(item.IdentityKey) ? IdentityKey.From(item.Text) : item.IdentityKey,
        History = item.History ?? []
    };

    private static bool Is(ProjectStateItem i, string type) => string.Equals(i.ItemType, type, StringComparison.OrdinalIgnoreCase);
}
