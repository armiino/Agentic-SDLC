namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;

/// <summary>
/// Typisierte Edge-Payloads des Maker-Checker-Repair-Workflows (Transport = Message-Passing, wie im
/// Ledger-Workflow, NICHT Shared State). Der Zyklus braucht bewusst nur zwei Nachrichtentypen.
/// </summary>
/// <remarks>
/// <see cref="CheckArtifactMessage"/> ist der GEMEINSAME Eingangstyp des Checkers: sowohl der Maker (Start,
/// Iteration 1) als auch der Repair-Executor (Loop-Back, Iteration n+1) senden ihn. Dadurch hat der Checker
/// genau einen Input-Typ (<c>Executor&lt;CheckArtifactMessage&gt;</c>) und zwei eingehende Kanten desselben
/// Typs — MAF routet beide an denselben Handler. <see cref="CheckVerdictMessage"/> trägt die deterministisch
/// abgeleitete Weiter-Entscheidung; zwei KONDITIONALE Kanten am Checker filtern darauf (Repair vs. Finalize).
/// </remarks>
public sealed record CheckArtifactMessage(string Markdown, int Iteration);

/// <summary>
/// Das Checker-Verdikt, geroutet an Repair (reparierbar) ODER Finalize (terminal). Trägt beide Teil-Reports
/// (deterministisches MC0 + k-Vote-C7) mit, damit Repair die bestätigten Zeilen-Verstöße patchen und Finalize
/// das Zertifikat schreiben kann — ohne erneut zu prüfen.
/// </summary>
public sealed record CheckVerdictMessage(
    string Markdown,
    int Iteration,
    ContractDecision Decision,
    ContractCheckReport Structural,
    CriticVoteReport Evidence);
