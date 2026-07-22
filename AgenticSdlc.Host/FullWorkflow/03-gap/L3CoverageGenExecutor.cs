using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// COVERAGE-GENERATOR mit knoten-internem, GEBUNDENEM Repair-Loop (Schritt 4). Pass 1 = freie (measure-)Generierung;
/// danach — solange <c>maxRepairRounds</c> nicht erschöpft UND Linsen leer sind — ein GEZIELTER Repair-Pass nur auf die
/// leeren Linsen (Hinweise aus dem <see cref="CoverageSpec"/>). Der Agent DARF eine Linse begründet leer lassen (echtes
/// N/A, kein Füllstoff) → verbleibende Leerstellen sind ein ehrliches Signal, kein Fehler. <c>maxRepairRounds=0</c> ⇒
/// measure-Baseline (Einzelpass, kein Loop).
/// </summary>
/// <remarks>
/// Knoten-interne Loop-Form (wie die Derivation-Familie sie neben der edge-nativen ReflectGraph-Form führt) — bewusst
/// proportionaler als ein eigener MAF-Zyklus mit neuen Message-Typen; die Iterationsschranke ist hart. Der Downstream-
/// Graph (Resolve → Validate → Judge → Routing → Finalize) bleibt unverändert. Der Delta-Beleg (Abdeckung initial→final)
/// liegt in <c>coverage-repair-trace.json</c> + den <c>L3_COVERAGE_ROUND</c>-Events.
/// </remarks>
[SendsMessage(typeof(L3Candidates))]
internal sealed class L3CoverageGenExecutor(AIAgent agent, CoverageSpec spec, int maxRepairRounds, RunContext run)
    : Executor<SourceArtifactSet>("L3-CoverageGen")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(SourceArtifactSet env, IWorkflowContext context, CancellationToken ct = default)
    {
        // Pass 1: freie (measure-)Generierung.
        var response = await agent.RunAsync([new ChatMessage(ChatRole.User, L3Prompts.BuildEnvUser(env, forGeneration: true))], cancellationToken: ct).ConfigureAwait(false);
        var cores = L3CandidateParsing.ParseCores(response.Text);
        var seen = new HashSet<string>(cores.Select(c => L3CandidateParsing.NormText(c.Text)), StringComparer.Ordinal);
        var cov = L3LensCoverage.Evaluate(spec, L3CandidateParsing.AssignIds(cores));
        var initialAddressed = cov.AddressedLenses;
        var trace = new List<object> { RoundEntry(0, cores.Count, 0, cov) };
        LogRound(0, cores.Count, 0, cov);

        var round = 0;
        while (round < maxRepairRounds && cov.UnaddressedLensIds.Count > 0)
        {
            round++;
            var missing = cov.UnaddressedLensIds
                .Select(id => spec.TryGetLens(id, out var l) ? l : null)
                .Where(l => l is not null).Select(l => l!).ToList();
            var repairUser = L3Prompts.BuildCoverageRepairUser(env, L3CandidateParsing.AssignIds(cores), missing);
            var repairResp = await agent.RunAsync([new ChatMessage(ChatRole.User, repairUser)], cancellationToken: ct).ConfigureAwait(false);

            var added = 0;
            foreach (var m in L3CandidateParsing.ParseCores(repairResp.Text))
                if (seen.Add(L3CandidateParsing.NormText(m.Text))) { cores.Add(m); added++; }

            cov = L3LensCoverage.Evaluate(spec, L3CandidateParsing.AssignIds(cores));
            trace.Add(RoundEntry(round, cores.Count, added, cov));
            LogRound(round, cores.Count, added, cov);
            if (added == 0) break; // Runde brachte nichts Neues (Agent erklärt die Restlinsen implizit für N/A) → Abbruch.
        }

        var items = L3CandidateParsing.AssignIds(cores);
        if (maxRepairRounds > 0)
        {
            var tracePath = Path.Combine(run.RunDir, "coverage-repair-trace.json");
            await File.WriteAllTextAsync(tracePath, JsonSerializer.Serialize(new
            {
                specId = spec.Id, maxRepairRounds, roundsRun = round,
                initialAddressed, finalAddressed = cov.AddressedLenses, totalLenses = cov.TotalLenses,
                mandatoryComplete = cov.MandatoryComplete, remainingUnaddressed = cov.UnaddressedLensIds, rounds = trace
            }, Json), ct).ConfigureAwait(false);
        }
        run.AppendEvent(new { type = "L3_CANDIDATES_GENERATED", runId = run.RunId, count = items.Count, repairRoundsRun = round, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Candidates(env, items)).ConfigureAwait(false);
    }

    private void LogRound(int round, int total, int added, LensCoverageReport cov)
        => run.AppendEvent(new
        {
            type = "L3_COVERAGE_ROUND", runId = run.RunId, round, totalCandidates = total, added,
            addressed = cov.AddressedLenses, totalLenses = cov.TotalLenses,
            unaddressed = cov.UnaddressedLensIds, mandatoryUnaddressed = cov.MandatoryUnaddressedLensIds, timestampUtc = DateTime.UtcNow
        });

    private static object RoundEntry(int round, int total, int added, LensCoverageReport cov)
        => new { round, totalCandidates = total, added, addressed = cov.AddressedLenses, unaddressed = cov.UnaddressedLensIds };
}
