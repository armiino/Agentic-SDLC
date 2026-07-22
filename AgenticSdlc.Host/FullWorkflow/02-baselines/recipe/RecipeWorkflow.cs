using AgenticSdlc.Host.FullWorkflow.Chain;
using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.FullWorkflow.FanOut;
using AgenticSdlc.Host.FullWorkflow.Load;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Recipes;

/// <summary>
/// Der Rezept-Assembler (§10): montiert aus einer Baseline-Quelle + 0..N Ableitungen EINEN MAF-Graphen zur Laufzeit
/// — nicht ein handgeschriebener Workflow je Use-Case, sondern derselbe Bauplan aus den vorhandenen Bausteinen.
/// </summary>
/// <remarks>
/// Graph:
/// <code>
///   [ Baseline-Quelle ]              build → gebundener Fan-out | load → LoadBaselineExecutor
///          │  (VerifiedBaselineSet)
///          ├─fan-out─► SelectSubset(sources₁) ─► [Derivation₁]   (BindAsExecutor, WithOutputFrom)
///          └─fan-out─► SelectSubset(sources₂) ─► [Derivation₂]
/// </code>
/// Reine Wiederverwendung: die Baseline-Quelle ist austauschbar (build ↔ load), jede Ableitung ist der gebundene
/// generische Derivation-Workflow, <c>SelectSubset</c> = <see cref="SelectBaselineExecutor"/> mit der Quell-Teilmenge.
/// Fan-out broadcastet dasselbe <c>VerifiedBaselineSet</c> an alle Ableitungs-Zweige (jeder filtert seine Quellen).
/// </remarks>
public static class RecipeWorkflow
{
    public const string WorkflowName = "Recipe";

    /// <summary>Baseline via Fan-out (mode:build) — der Fan-out-Workflow wird hier als Knoten gebunden.</summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow BuildFromFanOut(
        Microsoft.Agents.AI.Workflows.Workflow fanOut,
        IReadOnlyList<(DerivationSpec Spec, Microsoft.Agents.AI.Workflows.Workflow Derivation)> derivations,
        RunContext run)
        => Assemble(fanOut.BindAsExecutor(BaselineFanOutWorkflow.WorkflowName), derivations, run, "build");

    /// <summary>Baseline via Load (mode:load) — der LoadBaselineExecutor ist die Quelle.</summary>
    internal static Microsoft.Agents.AI.Workflows.Workflow BuildFromLoad(
        LoadBaselineExecutor load,
        IReadOnlyList<(DerivationSpec Spec, Microsoft.Agents.AI.Workflows.Workflow Derivation)> derivations,
        RunContext run)
        => Assemble(load, derivations, run, "load");

    private static Microsoft.Agents.AI.Workflows.Workflow Assemble(
        ExecutorBinding baselineNode,
        IReadOnlyList<(DerivationSpec Spec, Microsoft.Agents.AI.Workflows.Workflow Derivation)> derivations,
        RunContext run,
        string mode)
    {
        var builder = new WorkflowBuilder(baselineNode)
            .WithName($"{WorkflowName}-{mode}")
            .WithDescription($"Rezept ({mode}): Baseline → {derivations.Count} Ableitung(en) "
                           + $"[{string.Join(", ", derivations.Select(d => d.Spec.Id))}] (Assembler §10).");

        // Baseline-only-Rezept (keine Ableitung, z. B. „nur der requirements-Agent") → Baseline ist terminal.
        if (derivations.Count == 0)
        {
            builder.WithOutputFrom(baselineNode);
            return builder.Build();
        }

        var selects = new List<ExecutorBinding>(derivations.Count);
        var outputs = new List<ExecutorBinding>(derivations.Count);
        foreach (var (spec, deriv) in derivations)
        {
            ExecutorBinding select = new SelectBaselineExecutor(spec.SourceArtifactTypes, run, spec.Id);
            ExecutorBinding derivationNode = deriv.BindAsExecutor($"Derivation-{spec.Id}");
            builder.AddEdge(select, derivationNode);   // Teilmenge → Ableitung
            selects.Add(select);
            outputs.Add(derivationNode);
        }

        builder.AddFanOutEdge(baselineNode, selects);        // Baseline → alle SelectSubset (Broadcast)
        builder.WithOutputFrom(outputs.ToArray());           // jede Ableitung ist eine Workflow-Ausgabe
        return builder.Build();
    }
}
