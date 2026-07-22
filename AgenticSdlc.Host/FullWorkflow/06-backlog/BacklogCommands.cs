namespace AgenticSdlc.Host.FullWorkflow.Backlog;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class BacklogCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // L4-v1: Project State -> kanonische Requirements-Baseline + Traceability-Projektionen.
        map["l4-baseline"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4BaselineRunner.RunAsync(args, repoRoot);

        // L4 ConsolidationPlan: Seed/Check fuer agentische Konsolidierung mit deterministischem Gate.
        map["l4-consolidation"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4ConsolidationRunner.RunAsync(args, settings, repoRoot);

        // L4 Human-Review: ConsolidationPlan-Operationen mit generischer HumanReview-UI autorisieren.
        map["l4-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4ReviewRunner.RunAsync(args, settings, repoRoot);

        // L4 Apply: freigegebenen ConsolidationPlan deterministisch zur kanonischen Requirements-Baseline anwenden.
        map["l4-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4ApplyRunner.RunAsync(args, repoRoot);

        // L4 Quality: kanonische Baseline auf Rueckfuehrbarkeit und Operationalisierbarkeit pruefen.
        map["l4-quality"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4QualityRunner.RunAsync(args, repoRoot);

        // L4 Requirements Document: kanonische Baseline deterministisch als RE-Dokument rendern.
        map["l4-requirements-doc"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.RequirementsDocumentRunner.RunAsync(args, repoRoot);

        // L4 Completion: Adequacy-Feedback + kontrollierte DISK/OpenDecision-Proposals vor Readiness/IssuePlanning.
        map["l4-completion"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4CompletionRunner.RunAsync(args, settings, repoRoot);

        // L4 Completion Human-Review: Completion-Proposals mit generischer HumanReview-UI autorisieren.
        map["l4-completion-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4CompletionReviewRunner.RunAsync(args, settings, repoRoot);

        // L4 Completion Apply: akzeptierte Completion-Proposals deterministisch in einen erweiterten L4-Stand uebernehmen.
        map["l4-completion-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.L4CompletionApplyRunner.RunAsync(args, repoRoot);

        // Requirements Readiness: L4-Baseline + Quality deterministisch fuer Issue Planning vorbereiten.
        map["requirements-readiness"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.RequirementsReadinessRunner.RunAsync(args, repoRoot);

        // L4 Issue Planning: plan-only Agentenknoten auf Readiness-gefiltertem Input; kein GitHub-Write.
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

        // GitHub Snapshot: read-only GitHub Issues in ein reproduzierbares Reconciliation-Input-Format normalisieren.

        // Operationalization Audit: prueft Traceability von L4/Readiness bis GitHub-Dry-Run vor echten Writes.
        map["operationalization-audit"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.OperationalizationAuditRunner.RunAsync(args, repoRoot);

        // Open Requirements Review: klassifiziert nicht operationalisierte Requirements fuer Klaerungsarbeit.
        map["open-requirements-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.OpenRequirementsReviewRunner.RunAsync(args, settings, repoRoot);

        // Open Requirements Apply: materialisiert Review-Entscheidungen zu ClarificationPlanningInput.
        map["open-requirements-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.OpenRequirementsApplyRunner.RunAsync(args, repoRoot);

        // Clarification Agent: offene Requirements plan-only als Klaerungs-/Breakdown-Arbeit weiterfuehren; Resolve-Modus ist als Contract vorbereitet.
        map["clarification-agent"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ClarificationAgentRunner.RunAsync(args, settings, repoRoot);

        // Clarification Agent Human-Review: ClarificationPlanItems mit generischer HumanReview-UI autorisieren.
        map["clarification-agent-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ClarificationPlanningReviewRunner.RunAsync(args, settings, repoRoot);

        // Clarification Agent Apply: Human-Decisions deterministisch zu accepted-clarification-plan materialisieren.
        map["clarification-agent-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Backlog.ClarificationPlanningApplyRunner.RunAsync(args, repoRoot);
    }
}
