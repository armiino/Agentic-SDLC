namespace AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow;

/// <summary>
/// Typisierte Edge-Payloads des eigenständigen Checker-Repair-Workflows (Transport = Message-Passing, wie im
/// Ledger-Workflow, NICHT Shared State). Der Zyklus braucht nur zwei interne Nachrichtentypen + ein Ergebnis.
/// </summary>
/// <remarks>
/// <see cref="CheckArtifactMessage"/> ist der Eingangstyp des <b>Start-Executors</b> (Checker) UND der Loop-Back-Typ:
/// sowohl der externe Aufrufer (Standalone-Runner ODER der vorgelagerte Agent-Knoten in der späteren Kette) als auch
/// der Repair-Executor senden ihn. Dadurch hat der Checker genau einen Input-Typ und zwei eingehende Kanten desselben
/// Typs. <see cref="CheckVerdictMessage"/> trägt die deterministisch abgeleitete Weiter-Entscheidung; zwei
/// KONDITIONALE Kanten am Checker filtern darauf (Repair vs. Finalize). <see cref="CheckerRepairResult"/> ist der
/// terminale Output — als <c>YieldOutput</c>-Typ ist er zugleich der Ausgabetyp, wenn der ganze Workflow per
/// <c>BindAsExecutor</c> als EIN Knoten in einen größeren Graphen eingehängt wird.
/// </remarks>
public sealed record CheckArtifactMessage(string Markdown, int Iteration);

/// <summary>Das Checker-Verdikt, geroutet an Repair (reparierbar) ODER Finalize (terminal). Trägt beide
/// Teil-Reports (deterministisches MC0 + k-Vote-C7), damit Repair die bestätigten Zeilen-Verstöße patchen und
/// Finalize das Zertifikat schreiben kann — ohne erneut zu prüfen.</summary>
public sealed record CheckVerdictMessage(
    string Markdown,
    int Iteration,
    ContractDecision Decision,
    ContractCheckReport Structural,
    CriticVoteReport Evidence);

/// <summary>Terminales Ergebnis des Workflows: das finale (ggf. reparierte) Artefakt + die Abschluss-Entscheidung.
/// Als typisierter Output ist er direkt weiterverwendbar, wenn der Workflow als Knoten gebunden wird.</summary>
public sealed record CheckerRepairResult(
    string Markdown,
    ContractDecision Decision,
    int Iterations,
    int Mc0Errors,
    int C7Residual);
