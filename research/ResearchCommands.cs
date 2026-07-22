namespace AgenticSdlc.Host.FullWorkflow.Backlog;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class ResearchCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // GitHub Reconciliation: accepted IssuePlan -> plan-only GitHubActionPlan; kein GitHub-Write.
        map["github-reconciliation"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.GithubReconciliationRunner.RunAsync(args, settings, repoRoot);

        // GitHub Reconciliation Human-Review: GitHubActionPlanItems mit generischer HumanReview-UI autorisieren.
        map["github-reconciliation-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.GithubReconciliationReviewRunner.RunAsync(args, settings, repoRoot);

        // GitHub Reconciliation Apply: Human-Decisions deterministisch zu accepted-github-action-plan materialisieren.
        map["github-reconciliation-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.GithubReconciliationApplyRunner.RunAsync(args, repoRoot);

        // GitHub Write: deterministischer Dry-Run ueber akzeptiertem GitHubActionPlan; keine echten GitHub-Writes.
        map["github-write"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.GithubWriteDryRunRunner.RunAsync(args, repoRoot);
    }
}
