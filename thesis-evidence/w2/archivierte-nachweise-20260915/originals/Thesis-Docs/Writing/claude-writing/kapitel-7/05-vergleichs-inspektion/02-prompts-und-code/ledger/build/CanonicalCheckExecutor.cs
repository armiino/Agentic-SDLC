using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Schritt 5 ④ (05.08.) — der deterministische CHECKER des Kanonisierungs-Loops (Maker-Checker-Repair,
/// R-33-Form): prüft den Canonical-Draft gegen die Kanonisierungs-Invarianten
/// (<see cref="LedgerQualityGate.EvaluateCanonicalStage"/>, geteilte Quelle mit dem End-Audit) und entscheidet
/// typisiert (<see cref="GateLoop.Decide"/>): Pass → Trace-Apply + step-02-Write + weiter zur Facet-Validierung ·
/// Repair → <see cref="CanonicalGateVerdict"/> an den Repair-Knoten (Kanten-Prädikat) · MaxAttempts/HumanReview →
/// LAUTER Terminal-Yield (wie der bisherige GATE-FAILED-Tod — nur eben erst NACH Reparatur-Versuchen).
/// </summary>
/// <remarks>
/// Repairable ist bewusst NUR <c>CANDIDATE_SILENTLY_DROPPED</c> (der Scope des existierenden Coverage-Repairers —
/// Verhaltens-Erhalt, kein neues Modellverhalten); alle anderen Fehlerklassen enden wie bisher terminal, jetzt
/// mit ehrlicher Diagnose. Die <c>GateAttempt</c>-Historie (gate/canonical-gate.json) ist der W2-Messdraht
/// („hat GateFeedback geholfen?") an der teuersten Stufe.
/// </remarks>
[SendsMessage(typeof(CanonicalGateVerdict))]
[SendsMessage(typeof(CanonicalLedgerMessage))]
[YieldsOutput(typeof(string))]
internal sealed class CanonicalCheckExecutor(RunContext run, int maxAttempts = 2)
    : Executor<CandidateAndCanonicalLedgerMessage>("LedgerCanonicalCheck")
{
    public const string RepairableCode = "CANDIDATE_SILENTLY_DROPPED";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(
        CandidateAndCanonicalLedgerMessage message, IWorkflowContext context, CancellationToken ct = default)
    {
        var report = LedgerQualityGate.EvaluateCanonicalStage(message.Candidates, message.CanonicalDraft);
        var hasRepairable = report.Violations.Any(x => x.Severity == "error" && x.Code == RepairableCode);
        var decision = GateLoop.Decide(report.Pass, hasRepairable, message.Attempt, maxAttempts);

        var errors = report.Violations.Where(x => x.Severity == "error")
            .Select(x => $"{x.Code}: {x.Message} [{string.Join(",", x.Ids)}]").ToList();
        var history = (message.History ?? [])
            .Append(new GateAttempt(message.Attempt, message.Source, report.Pass, decision.ToString(), errors, DateTime.UtcNow))
            .ToList();

        run.AppendEvent(new
        {
            type = "LEDGER_CANONICAL_GATE",
            runId = run.RunId,
            attempt = message.Attempt,
            source = message.Source,
            pass = report.Pass,
            decision = decision.ToString(),
            errors = report.ErrorCount,
            timestampUtc = DateTime.UtcNow
        });

        // W2-Messdraht + Diagnose-Anker: letzter Stand + VOLLE Attempt-Historie (bei jedem Durchlauf überschrieben).
        var gateDir = Path.Combine(run.RunDir, "gate");
        Directory.CreateDirectory(gateDir);
        await File.WriteAllTextAsync(Path.Combine(gateDir, "canonical-gate.json"),
            JsonSerializer.Serialize(new { pass = report.Pass, decision = decision.ToString(), attempts = history, report }, Json), ct)
            .ConfigureAwait(false);

        if (decision == GateDecision.Repair)
        {
            await context.SendMessageAsync(new CanonicalGateVerdict(
                message.Candidates, message.CanonicalDraft, report, decision, message.Attempt, history)).ConfigureAwait(false);
            return;
        }

        // Terminal (Pass ODER MaxAttempts/HumanReview): Unit-Trace stempeln + step-02 als Audit-Artefakt
        // schreiben — exakt der bisherige Schreib-Ort/Inhalt (Verhaltens-Erhalt), Draft auch im Fehlerfall.
        var canonical = LedgerSourceUnitTrace.ApplyFromCandidates(message.Candidates, message.CanonicalDraft);
        LedgerRunArtifacts.WriteStep(
            run,
            "step-02-canonical",
            output: new SemanticLedgerFixture(canonical),
            metrics: new
            {
                stage = "canonical-gate",
                candidate = message.Candidates.Count,
                canonical = canonical.Count,
                attempt = message.Attempt,
                pass = report.Pass,
                decision = decision.ToString(),
                errors = report.ErrorCount
            });

        if (decision == GateDecision.Pass)
        {
            await context.SendMessageAsync(new CanonicalLedgerMessage(canonical)).ConfigureAwait(false);
            return;
        }

        await context.YieldOutputAsync(
            $"01-ledger Kanonisierungs-Gate: {decision} nach {message.Attempt} Versuch(en) — {report.ErrorCount} Fehler " +
            $"(gate/canonical-gate.json). Lauf: {run.RunId}").ConfigureAwait(false);
    }
}
