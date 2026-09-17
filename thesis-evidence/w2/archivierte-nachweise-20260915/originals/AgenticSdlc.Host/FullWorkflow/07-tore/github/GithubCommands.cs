namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class GithubCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Tor 3 / T3.2: GitHub-READ als deterministische Queries gegen einen reproduzierbaren Issue-Snapshot (kein LLM,
        // kein Token). Dasselbe Verhalten bekommt der Forward-Maker (T3.3) ueber GithubReadTools. Nur lesend.
        // R6: github-snapshot an seinen ehrlichen Ort gezogen (lag route-bedingt in BacklogCommands).
        map["github-snapshot"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubIssueSnapshotRunner.RunAsync(args, repoRoot);

        map["github-read"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubReadRunner.RunAsync(args, repoRoot);

        // Tor 3 / T3.3: Forward-Maker — github-sync-Delta gegen GitHub. Deterministischer Vorfilter + agentischer Rest
        // (Suche -> LINK/CREATE) -> GithubForwardPlan -> Gate (inkl. Rev-3-Invariante). Kein Write (Review/Apply folgen).
        map["github-forward"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardRunner.RunAsync(args, settings, repoRoot);

        // S1 (Worklist 20.07): github-forward als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint.
        // Additiv/parallel zum klassischen github-forward / -review / -apply (die bleiben unveraendert).
        map["github-forward-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardHitlRunner.RunAsync(args, settings, repoRoot);

        map["github-forward-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardReviewRunner.RunAsync(args, settings, repoRoot);

        // Tor 3 / T3.4: gated Write-Apply — der EINZIGE echte GitHub-Write. Safe-by-default (dry-run ohne --execute);
        // fuehrt NUR akzeptierte Ops aus und schreibt das Mapping (implemented_by_issue, T3.1) in den Core zurueck.
        map["github-forward-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardApplyRunner.RunAsync(args, repoRoot);

        // Tor 3 / T3.5: Reverse GitHub-Feedback-Ingestion (E4). GitHub-Zustand wird NIE auto-Wahrheit — geschlossenes Issue
        // erzeugt einen gepruefsten StateChange-Vorschlag; `done` entsteht NUR nach menschlicher Verifikation. Maker/Review/Apply.
        // C2a-4 (08.08.): deterministische Inbound-Ernte-Erkennung (Zwischenbahn; Ein-Graph-Eingang = C2c).
        map["github-inbound"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound.GithubInboundRunner.RunAsync(args, settings, repoRoot);

        // C2d ① (09.08.): Kommentar-Destillat-Zwischenbahn — Diskussionsraum als Evidenz, Wahrheit nur via Gates.
        map["github-comment-distill"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound.GithubCommentDistillRunner.RunAsync(args, settings, repoRoot);

        map["github-reverse"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubReverseRunner.RunAsync(args, repoRoot);

        map["github-reverse-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubReverseReviewRunner.RunAsync(args, settings, repoRoot);

        map["github-reverse-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubReverseApplyRunner.RunAsync(args, repoRoot);

        // Tor 3 / T3.6: Vergleichspfad-Harness — deterministischer Matcher (und optional ein Agent-Plan) gegen Drift-Fixtures
        // mit Gold-Labels; misst Dedup-Recall/Precision (wo schlaegt der Agent den Keyword-Matcher, wo reicht Determinismus).
        map["github-forward-compare"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardCompareRunner.RunAsync(args, repoRoot);

        // Tor 3 / T3.7: Sprint-Re-Run-Test — derselbe Delta zweimal durch den Forward-Vorfilter. Beweis: der zweite Lauf
        // erzeugt KEINE Duplikat-Issues (alles gemappt -> UPDATE/HOLD). Deterministisch, kein LLM, kein GitHub, kein Core.
        map["github-forward-rerun-test"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Tore.Github.GithubForwardRerunTest.RunAsync(args, repoRoot);
    }
}
