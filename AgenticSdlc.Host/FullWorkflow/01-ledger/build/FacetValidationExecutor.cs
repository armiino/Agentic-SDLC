using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Stufe 3 des Ledger-Workflows: chunked Per-Item-Validierung der kanonischen Einträge gegen das Transcript
/// (Verdict + Facet-Issues + Repair-Vorschlag), Freigabe-Status deterministisch abgeleitet.
/// </summary>
/// <remarks>
/// Wrappt <see cref="FacetValidator"/> (Batches à ~8, fixer Nenner, VOLLES Transcript); schreibt
/// <c>step-03-facet-validation/output.json</c> (validierter Ledger) + metrics.json (Verdict-/Issue-
/// Verteilung — Deskriptiv, keine Gold-Bewertung; Selective-Metriken laufen separat).
///
/// Terminierung ist config-abhängig (Plan §4): OHNE Adjudikation (<c>forwardToAdjudication=false</c>,
/// Default = heutiges Verhalten) terminiert die Stufe und yieldet die Zusammenfassung. MIT Adjudikation
/// sendet sie stattdessen den validierten Ledger als <see cref="ValidatedLedgerMessage"/> an den
/// nachgelagerten <see cref="HumanAdjudicationExecutor"/>. Beide Ausgangstypen sind deklariert; gesendet
/// wird immer nur genau einer (Send-Zeit-Check von MAF).
/// </remarks>
[YieldsOutput(typeof(string))]
[SendsMessage(typeof(ValidatedLedgerMessage))]
internal sealed class FacetValidationExecutor : Executor<CanonicalLedgerMessage>
{
    public const string ExecutorName = "LedgerFacetValidation";

    private readonly FacetValidator _validator;
    private readonly string _transcript;
    private readonly RunContext _run;
    private readonly bool _forwardToAdjudication;

    public FacetValidationExecutor(FacetValidator validator, string transcript, RunContext run,
        bool forwardToAdjudication = false)
        : base(ExecutorName)
    {
        _validator = validator;
        _transcript = transcript;
        _run = run;
        _forwardToAdjudication = forwardToAdjudication;
    }

    public override async ValueTask HandleAsync(
        CanonicalLedgerMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var verdicts = await _validator
            .ValidateAllAsync(message.Entries, _transcript, cancellationToken)
            .ConfigureAwait(false);

        var byId = verdicts.ToDictionary(v => v.Id, v => v, StringComparer.Ordinal);
        var validated = message.Entries.Select(e =>
        {
            var v = byId.TryGetValue(e.Id, out var found)
                ? found
                : new EntryValidation(e.Id, "partial", [], "no verdict").Normalized();
            return new ValidatedLedgerEntry(e, v, ValidatedLedgerEntry.DeriveStatus(v));
        }).ToList();

        var issues = validated.SelectMany(v => v.Validation.FacetIssues).ToList();
        var metrics = new
        {
            stage = "facet-validation",
            batchSize = _validator.BatchSize,
            entries = validated.Count,
            verdict = new
            {
                grounded = validated.Count(v => v.Validation.Verdict == "grounded"),
                partial = validated.Count(v => v.Validation.Verdict == "partial"),
                overstated = validated.Count(v => v.Validation.Verdict == "overstated"),
                unsupported = validated.Count(v => v.Validation.Verdict == "unsupported")
            },
            claimStatus = new
            {
                approved = validated.Count(v => v.ClaimStatus == "approved"),
                review_required = validated.Count(v => v.ClaimStatus == "review_required")
            },
            facetIssues = new
            {
                total = issues.Count,
                byFacet = issues.GroupBy(i => i.Facet).ToDictionary(g => g.Key, g => g.Count()),
                withRepair = issues.Count(i => i.Suggested is not null)
            }
        };

        var validatedLedger = new ValidatedLedger(validated);
        LedgerRunArtifacts.WriteStep(_run, "step-03-facet-validation", validatedLedger, metrics);

        var summary = $"Ledger L3 fertig: {validated.Count} Claims validiert (grounded={metrics.verdict.grounded}, partial={metrics.verdict.partial}, overstated={metrics.verdict.overstated}, unsupported={metrics.verdict.unsupported}; {issues.Count} Facet-Issues)";

        if (_forwardToAdjudication)
            await context.SendMessageAsync(new ValidatedLedgerMessage(validatedLedger)).ConfigureAwait(false);
        else
            await context.YieldOutputAsync(summary).ConfigureAwait(false);
    }
}
