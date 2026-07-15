using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// L3-Prepare-Workflow (Workflow 1, §5.3): der MAF-native lineare Pfad
/// <c>CandidateGen[Agent] → AnchorResolve[Agent] → AnchorValidate[det] → SupportJudge[Judge] → Routing[det] → Finalize</c>.
/// Eingabe = <see cref="Derivation.SourceArtifactSet"/> (Umwelt-Graph), Ausgabe = <see cref="L3Result"/>. Endet am
/// Human-Review-Paket; die Klassen-Verzweigung + der Human-Loop (accept/edit/reject, NEEDS_REVISION→Reflect) sind die
/// Apply-Phase (Workflow 2) und bewusst noch nicht Teil von v1. Per <c>WithOutputFrom</c>/<c>BindAsExecutor</c> als EIN
/// Knoten in größere Graphen einhängbar.
/// </summary>
public static class L3Workflow
{
    public const string WorkflowName = "L3-OpenWorld-Prepare";

    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        L3CandidateGenExecutor gen,
        L3AnchorResolveExecutor resolve,
        L3AnchorValidateExecutor validate,
        L3SupportJudgeExecutor judge,
        L3RoutingExecutor routing,
        L3FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(gen)
            .WithName(WorkflowName)
            .WithDescription("Open-World-Kandidaten: generieren (ohne Pflicht-Anker) → Anker suchen → validieren → "
                           + "je Anker Tragfähigkeit → deterministisch in 4 Klassen routen → Human-Review-Paket.");

        builder.AddEdge(gen, resolve);
        builder.AddEdge(resolve, validate);
        builder.AddEdge(validate, judge);
        builder.AddEdge(judge, routing);
        builder.AddEdge(routing, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }

    /// <summary>Coverage-Repair-Variante (Schritt 4): identischer Downstream-Graph, aber der Start-Knoten ist der
    /// <see cref="L3CoverageGenExecutor"/> mit knoten-internem Repair-Loop (measure = 0 Runden, repair = N Runden).</summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        L3CoverageGenExecutor gen,
        L3AnchorResolveExecutor resolve,
        L3AnchorValidateExecutor validate,
        L3SupportJudgeExecutor judge,
        L3RoutingExecutor routing,
        L3FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(gen)
            .WithName(WorkflowName)
            .WithDescription("Coverage-Generierung mit knoten-internem Repair-Loop → Anker suchen → validieren → "
                           + "Tragfähigkeit → 4-Klassen-Routing → Human-Review-Paket.");

        builder.AddEdge(gen, resolve);
        builder.AddEdge(resolve, validate);
        builder.AddEdge(validate, judge);
        builder.AddEdge(judge, routing);
        builder.AddEdge(routing, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }

    /// <summary>Agentische Coverage-Variante: identischer Downstream-Graph, aber der Start-Knoten ist ein echter
    /// Tool-Agent, der seine Umwelt selbst erkundet und Kandidaten per <c>save_l3_candidates</c> schreibt.</summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow Build(
        L3AgenticCoverageExecutor gen,
        L3AnchorResolveExecutor resolve,
        L3AnchorValidateExecutor validate,
        L3SupportJudgeExecutor judge,
        L3RoutingExecutor routing,
        L3FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(gen)
            .WithName(WorkflowName)
            .WithDescription("Agentische Coverage-Analyse mit Tools → Anker suchen → validieren → Tragfähigkeit → "
                           + "4-Klassen-Routing → Human-Review-Paket.");

        builder.AddEdge(gen, resolve);
        builder.AddEdge(resolve, validate);
        builder.AddEdge(validate, judge);
        builder.AddEdge(judge, routing);
        builder.AddEdge(routing, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }

    /// <summary>
    /// Komponenten-Variante (Plan §11.1): startet bei <c>AnchorValidate</c> mit KONTROLLIERTEN Test-Kandidaten
    /// (<see cref="L3Resolved"/>) — überspringt die beiden nondeterministischen Agenten (Generierung + Resolution).
    /// Erlaubt einen isolierten, reproduzierbaren Test der Klassifikation (alle 4 Klassen gezielt provozieren). Nur der
    /// Support-Judge bleibt ein LLM.
    /// </summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow BuildFromResolved(
        L3AnchorValidateExecutor validate,
        L3SupportJudgeExecutor judge,
        L3RoutingExecutor routing,
        L3FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(validate)
            .WithName($"{WorkflowName}-FromCandidates")
            .WithDescription("Kontrollierte Test-Kandidaten: validieren → je Anker Tragfähigkeit → 4-Klassen-Routing → "
                           + "Human-Review-Paket (ohne Generierungs-/Resolutions-Agenten).");

        builder.AddEdge(validate, judge);
        builder.AddEdge(judge, routing);
        builder.AddEdge(routing, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }

    /// <summary>
    /// Reflect-Sub-Workflow (NEEDS_REVISION, §5.3/§9): <c>Revise[Agent] → AnchorResolve[Agent] → Validate → Judge →
    /// Routing → Finalize</c>. Der Revise-Agent überarbeitet den vorigen Entwurf per Feedback (Self-Refine), danach läuft
    /// der revidierte Kandidat durch die WIEDERVERWENDETE Klassifikations-Pipeline und wird neu geroutet. Eingabe =
    /// <see cref="L3ReviseSet"/>, Ausgabe = <see cref="L3Result"/> (revidiertes Review-Paket via Finalize-Suffix).
    /// </summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow BuildRevise(
        L3ReviseExecutor revise,
        L3AnchorResolveExecutor resolve,
        L3AnchorValidateExecutor validate,
        L3SupportJudgeExecutor judge,
        L3RoutingExecutor routing,
        L3FinalizeExecutor finalize)
    {
        var builder = new WorkflowBuilder(revise)
            .WithName($"{WorkflowName}-Revise")
            .WithDescription("NEEDS_REVISION: Entwurf per Feedback überarbeiten (Self-Refine) → neu verankern → validieren "
                           + "→ Tragfähigkeit → 4-Klassen-Routing → revidiertes Human-Review-Paket. Bounded (Runner-Schranke).");

        builder.AddEdge(revise, resolve);
        builder.AddEdge(resolve, validate);
        builder.AddEdge(validate, judge);
        builder.AddEdge(judge, routing);
        builder.AddEdge(routing, finalize);
        builder.WithOutputFrom(finalize);
        return builder.Build();
    }
}
