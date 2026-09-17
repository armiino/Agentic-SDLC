using AgenticSdlc.Host.FullWorkflow.Derivation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Gap;

/// <summary>
/// Agentischer L3-Coverage-Startknoten: ein echter Agent erkundet die L1/L2-Umwelt per Tools und schreibt seine
/// Kandidaten selbst. Die bestehende Resolve/Validate/Judge/Routing-Pipeline bleibt die unabhängige Prüfung.
/// </summary>
[SendsMessage(typeof(L3Candidates))]
internal sealed class L3AgenticCoverageExecutor(
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory,
    CoverageSpec coverageSpec,
    RunContext run,
    string model,
    IReadOnlyDictionary<string, LedgerClaim>? ledgerClaims = null,
    SourceArtifactSet? toolEnv = null)
    : Executor<SourceArtifactSet>("L3-AgenticCoverage")
{
    public override async ValueTask HandleAsync(SourceArtifactSet env, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new L3AgenticTools(toolEnv ?? env, coverageSpec, run, run.RunDir, model, ledgerClaims);
        var agent = agentFactory(tools.Build());
        var task = """
                   Beginne. Deine Umwelt ist der verifizierte L1/L2-Projektzustand, aber sie wird dir NICHT vorab
                   vollständig in den Prompt kopiert. Entdecke sie zuerst mit list_artifacts und lies danach selbst die
                   Items, Claims und Zusammenhänge, die du für eine belastbare Requirements-Engineering-Gap-Analyse
                   brauchst.

                   Ziel: Formuliere die wesentlichen L3-Kandidaten, die eine Requirements-Engineer:in vor Projektstart
                   als fehlende, offene oder zu ergänzende Anforderungen/Entscheidungen markieren würde.

                   Arbeitsregeln:
                   - Nutze get_coverage_lenses, damit gapCategory exakt aus dem Linsenkatalog stammt.
                   - Nutze basedOn fuer die Umwelt-Items, die deine Idee fachlich ausgelöst haben. Das sind noch keine
                     finalen Anker; die Anchor-Resolution prueft spaeter unabhaengig.
                   - Markiere offene Entscheidungen mit requiresHumanDecision=true.
                   - Pruefe deinen Entwurf mindestens einmal mit check_lens_coverage und check_lens_adequacy.
                   - Ueberarbeite selbst, wenn Linsen fehlen oder Oberflaechenwarnungen auftreten.
                   - Rufe save_l3_candidates genau einmal am Ende auf.
                   """;

        await agent.RunAsync([new ChatMessage(ChatRole.User, task)], cancellationToken: ct).ConfigureAwait(false);

        if (!tools.Saved)
        {
            run.AppendEvent(new { type = "L3_AGENTIC_CANDIDATES_NOT_SAVED", runId = run.RunId, timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(new L3Candidates(env, [])).ConfigureAwait(false);
            return;
        }

        run.AppendEvent(new
        {
            type = "L3_AGENTIC_CANDIDATES_SAVED",
            runId = run.RunId,
            candidates = tools.SavedCandidates.Count,
            retrieved = tools.RetrievedItemIds.Count,
            coverageCheckRounds = tools.CoverageCheckRounds,
            adequacyCheckRounds = tools.AdequacyCheckRounds,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(new L3Candidates(env, tools.SavedCandidates)).ConfigureAwait(false);
    }
}
