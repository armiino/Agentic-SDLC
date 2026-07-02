using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.Ledger;

/// <summary>
/// Stufe 2b: repariert deterministisch erkannte Coverage-Luecken der Canonicalization mit einem gezielten
/// LLM-Repair-Call und schreibt danach den finalen <c>step-02-canonical</c>.
/// </summary>
[SendsMessage(typeof(CanonicalLedgerMessage))]
internal sealed class CanonicalCoverageRepairExecutor : Executor<CandidateAndCanonicalLedgerMessage>
{
    public const string ExecutorName = "LedgerCanonicalCoverageRepair";

    private readonly CanonicalCoverageRepairer _repairer;
    private readonly RunContext _run;

    public CanonicalCoverageRepairExecutor(CanonicalCoverageRepairer repairer, RunContext run)
        : base(ExecutorName)
    {
        _repairer = repairer;
        _run = run;
    }

    public override async ValueTask HandleAsync(
        CandidateAndCanonicalLedgerMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var missing = FindMissingCandidateIds(message.Candidates, message.CanonicalDraft);
        var repaired = missing.Count == 0
            ? message.CanonicalDraft
            : await _repairer
                .RepairAsync(message.Candidates, message.CanonicalDraft, missing, cancellationToken)
                .ConfigureAwait(false);

        var remainingMissing = FindMissingCandidateIds(message.Candidates, repaired);

        LedgerRunArtifacts.WriteStep(
            _run,
            "step-02-canonical",
            output: new SemanticLedgerFixture(repaired),
            metrics: new
            {
                stage = "canonical-coverage-repair",
                candidate = message.Candidates.Count,
                canonicalDraft = message.CanonicalDraft.Count,
                canonical = repaired.Count,
                missingBeforeRepair = missing.Count,
                missingAfterRepair = remainingMissing.Count,
                repaired = missing.Count > 0,
                missingCandidateIds = remainingMissing
            });

        await context
            .SendMessageAsync(new CanonicalLedgerMessage(repaired))
            .ConfigureAwait(false);
    }

    private static IReadOnlyList<string> FindMissingCandidateIds(
        IReadOnlyList<SemanticLedgerEntry> candidates,
        IReadOnlyList<SemanticLedgerEntry> canonical)
    {
        var candidateIds = candidates
            .Select(c => c.Id)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        var referenced = canonical
            .SelectMany(c => c.CandidateIds ?? [])
            .ToHashSet(StringComparer.Ordinal);

        return candidateIds.Where(id => !referenced.Contains(id)).ToList();
    }
}
