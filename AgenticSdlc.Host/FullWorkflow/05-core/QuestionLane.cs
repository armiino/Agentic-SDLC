namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Risiken-Rampe (Slice S ④, ⚖ Autor 21.08. „Risiken gehen nicht verloren — sie werden Entscheidungen"):
/// DIE eine Definition der Unklarheits-QUERSCHNITTS-SPUR an Tor 1. Offene Fragen (9g) UND Meeting-Risiken
/// fahren dieselbe Schiene: kein eigener Wahrheits-Typ, sondern Frage-Vokabular (OPEN_QUESTION/
/// ALREADY_DECIDED) → offene DEC am decision-gate (Auflösung: akzeptieren/mitigieren→ADOPT/klären).
/// Bewusste Grenze: das ist ein Risiko-EINGANG, kein Risiko-Register (kein Impact/Owner/Tracking).
/// Konsumenten: IngestionGate/-Tools (Coverage + Sichtbarkeit), AspectIngestionRouter (Querschnitt),
/// CoreSeeder (Bootstrap-Bahn), MeetingQuestionMint (Herkunfts-Prägung).
/// </summary>
public static class QuestionLane
{
    public const string Question = "open_question";
    public const string Risk = "risk";

    public static bool Carries(string? itemType)
        => string.Equals(itemType, Question, StringComparison.OrdinalIgnoreCase)
           || string.Equals(itemType, Risk, StringComparison.OrdinalIgnoreCase);

    public static bool IsRisk(string? itemType) => string.Equals(itemType, Risk, StringComparison.OrdinalIgnoreCase);
}
