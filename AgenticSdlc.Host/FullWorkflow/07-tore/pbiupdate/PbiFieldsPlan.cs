using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

/// <summary>Ein Autor-Wunsch „setze Prio/Schätzung von PBI-x" (Steward-Diktat). Mindestens EIN Feld gesetzt.</summary>
public sealed record PbiFieldWish(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("priority")] string? Priority = null,
    [property: JsonPropertyName("estimate")] string? Estimate = null,
    [property: JsonPropertyName("begruendung")] string? Begruendung = null);

/// <summary>
/// Slice S Teil 2 (⚖ Autor 21.08.) — das DETERMINISTISCHE Feld-Setz-Seil: Autor-Wünsche („Prio von PBI-12
/// auf hoch") → SET_PRIORITY/SET_ESTIMATE-Plan → Pending-Registry (C4d/C5-Weg: warten im Core, Vorlage via
/// pbi-update-review/open_review_ui, der GATED Apply schreibt und schließt). Kein LLM, keine Status-Wirkung —
/// aber volle Governance: KEIN Feld ändert sich ohne Autor-Entscheid am Gate. Konsumenten: Steward-Tool
/// `propose_pbi_fields` (Chat-Bahn); die CLI-Werkbank kann denselben Kern rufen.
/// </summary>
public static class PbiFieldsPlan
{
    public const string Bahn = "pbi-fields";

    /// <summary>Wünsche → validierter Plan. Ungültige Wünsche werden LAUT zurückgegeben, nie still verworfen.</summary>
    public static (PbiStateChangePlanDocument Plan, IReadOnlyList<string> Errors) Build(
        ProjectStateDocument core, IReadOnlyList<PbiFieldWish> wishes, string runId)
    {
        var pbiIds = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var ops = new List<PbiStateChangeOperation>();
        var errors = new List<string>();
        foreach (var w in wishes)
        {
            if (!pbiIds.Contains(w.PbiId)) { errors.Add($"{w.PbiId}: kein PBI im Core."); continue; }
            var rationale = string.IsNullOrWhiteSpace(w.Begruendung) ? "Autor-Diktat via Steward." : w.Begruendung!.Trim();
            var any = false;
            if (!string.IsNullOrWhiteSpace(w.Priority))
            {
                if (PbiFields.NormalizePriority(w.Priority) is { } prio)
                { ops.Add(SetOp(PbiUpdateKind.SetPriority, w.PbiId, prio, rationale)); any = true; }
                else errors.Add($"{w.PbiId}: Prio '{w.Priority}' ungültig (hoch|mittel|niedrig).");
            }
            if (!string.IsNullOrWhiteSpace(w.Estimate))
            {
                if (PbiFields.NormalizeEstimate(w.Estimate) is { } est)
                { ops.Add(SetOp(PbiUpdateKind.SetEstimate, w.PbiId, est, rationale)); any = true; }
                else errors.Add($"{w.PbiId}: Schätzung '{w.Estimate}' ungültig (S|M|L).");
            }
            if (!any && string.IsNullOrWhiteSpace(w.Priority) && string.IsNullOrWhiteSpace(w.Estimate))
                errors.Add($"{w.PbiId}: weder Prio noch Schätzung angegeben.");
        }
        var plan = new PbiStateChangePlanDocument(PbiStateChangePlanDocument.CurrentSchemaVersion,
            $"pbi-fields-{runId}", DateTime.UtcNow, runId, ops);
        return (plan, errors);
    }

    private static PbiStateChangeOperation SetOp(string kind, string pbiId, string value, string rationale)
        => new(kind, RequirementId: "", PbiId: pbiId, FeatureId: null, ReplacementRequirementId: null,
            OpenDecisionRef: null, Rationale: rationale, Value: value);

    /// <summary>Plan bauen + als Beleg-Kopie ablegen + in der Pending-Registry registrieren (Kangal-gedeckter
    /// Core-Save). Rückgabe: proposalId fürs Gate (open_review_ui / pbi-update-review --pending).</summary>
    public static async Task<(string ProposalId, int Ops, IReadOnlyList<string> Errors)> RegisterAsync(
        string repoRoot, IReadOnlyList<PbiFieldWish> wishes)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false)) throw new InvalidOperationException("Core fehlt (state/core).");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var run = new RunContext(RunId.New(), Bahn);
        var (plan, errors) = Build(core, wishes, run.RunId);
        if (plan.Operations.Count == 0) return ("", 0, errors);

        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        await File.WriteAllTextAsync(Path.Combine(outDir, "pbi-change-plan.json"),
            JsonSerializer.Serialize(plan, JsonFiles.Json)).ConfigureAwait(false);

        var payload = JsonSerializer.SerializeToElement(plan, JsonFiles.Json);
        var (registered, proposalId) = PendingReviewRegistry.Register(core, Bahn,
            $"pbi-update-review --pending PEND-{run.RunId}", payload,
            plan.Operations.Where(o => o.PbiId is not null).Select(o => o.PbiId!).Distinct(StringComparer.Ordinal).ToList(),
            run.RunId);
        await repo.SaveAsync(registered).ConfigureAwait(false);
        return (proposalId, plan.Operations.Count, errors);
    }
}
