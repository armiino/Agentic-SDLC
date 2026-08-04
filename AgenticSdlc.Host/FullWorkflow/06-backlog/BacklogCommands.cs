namespace AgenticSdlc.Host.FullWorkflow.Backlog;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Die Alt-L4-Kette (baseline/consolidation/quality/readiness/completion/clarification/openrequirements/
/// operationalization-audit/requirements-doc) wurde am 04.08. archiviert — Genealogie + WARUM:
/// docs/aktiv/backlog-genealogie.md · archive/phase2-l4-dormant/README.md.
/// </summary>
public static class BacklogCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // L4 Issue Planning: plan-only Agentenknoten; kein GitHub-Write.
        map["l4-issuplanning"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.IssuePlanningRunner.RunAsync(args, settings, repoRoot);

        // L4 Issue Planning Human-Review: IssuePlanItems mit generischer HumanReview-UI autorisieren.
        map["l4-issuplanning-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.IssuePlanningReviewRunner.RunAsync(args, settings, repoRoot);

        // L4 Issue Planning Apply: Human-Decisions deterministisch zu accepted-issue-plan materialisieren.
        map["l4-issuplanning-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.IssuePlanningApplyRunner.RunAsync(args, repoRoot);

        // L4 Re-Clarify (RE Backlog Structuring): kanonische Requirements -> Feature-Cluster -> Product Backlog.
        // `cluster` = agentische Feature-Cluster-Bildung (Maker + Coverage-Gate + ReviewAgent).
        map["l4-re-clarify"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyRunner.RunAsync(args, settings, repoRoot);

        // L4 Re-Clarify Cluster Review: HumanReview der vorgeschlagenen Cluster-Korrekturen (Operationen).
        map["l4-re-clarify-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyClusterReviewRunner.RunAsync(args, settings, repoRoot);

        // L4 Re-Clarify Apply: akzeptierte Cluster-Operationen deterministisch anwenden + Coverage-Recheck.
        map["l4-re-clarify-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyClusterApplyRunner.RunAsync(args, repoRoot);

        // L4 Re-Clarify Backlog Review: HumanReview der Product Backlog Items.
        map["l4-re-clarify-backlog-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyBacklogReviewRunner.RunAsync(args, settings, repoRoot);

        // L4 Re-Clarify Backlog Apply: akzeptierte/edited PBIs deterministisch als ProductBacklogView materialisieren.
        map["l4-re-clarify-backlog-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyBacklogApplyRunner.RunAsync(args, repoRoot);

        // L4 Re-Clarify IssuePlan: ProductBacklogView -> accepted-issue-plan (deterministisch, pbiId primaer).
        map["l4-re-clarify-issueplan"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyBacklogIssuePlanRunner.RunAsync(args, repoRoot);

        // L4 Re-Clarify Backlog Doc: lesbare Markdown-Projektion des Product Backlog.
        map["l4-re-clarify-backlog-doc"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ReClarifyBacklogDocRunner.RunAsync(args, repoRoot);
    }
}
