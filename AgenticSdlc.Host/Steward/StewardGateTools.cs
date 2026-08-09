using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.Steward;

/// <summary>
/// C5a (09.08.2026, c5-plan §3) — GESPRÄCH ALS GATE-KANAL für Registry-Pendings (pbi-update): das
/// Governance-DOPPELSCHLOSS: (1) `get_pending_review` legt das Gate TREU vor (dieselbe Adapter-Naht wie
/// die UI = E0-Wissen: Wirkung/Herkunft/Warn-Notes wörtlich); (2) `submit_gate_decisions`
/// (ApprovalRequired) schreibt die IDENTISCHE Entscheidungs-Datei wie die UI (EIN Vertrag; nur
/// reviewer=`author via steward-chat`) und kettet den R-43-Apply (schließt die Registry). Inhalt = Autor-
/// Diktat, MAF-Approval = typisierter Bestätigungs-Akt — der Steward bestätigt NIE selbst (9k(d)-Regel 3).
/// P2a wird IM Tool erzwungen: Nicht-apply ohne Begründung ⇒ Validierungsfehler zurück an den Agenten.
/// </summary>
public sealed class StewardGateTools(string repoRoot, Func<string, Task<int>>? applyFromPlanDir = null)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    // K13-1: typisierte Apply-Naht (planDir) statt CLI-String-Args; Test-injizierbar.
    private readonly Func<string, Task<int>> _apply =
        applyFromPlanDir ?? (planDir => PbiUpdateApplyRunner.ApplyFromPlanDirAsync(planDir, repoRoot));

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetPendingReviewAsync, "get_pending_review",
            "C5: laedt EIN wartendes Gate (proposalId aus pendingReviews) als TREUE Vorlage — je Item summary, "
            + "notes (Wirkung/Herkunft/WARNUNGEN — dem Autor WOERTLICH vorlegen, nichts kuerzen) und die "
            + "Angleichungs-Vorschlaege. Lies daraus vor und sammle die Entscheidungen des Autors ein."),
        AIFunctionFactory.Create(GetPausedGateAsync, "get_paused_gate",
            "C5b: laedt das PAUSIERTE Ein-Graph-Gate eines Laufs (aktuell: github-forward-gate) als treue "
            + "Vorlage — je Op kind/pbiId/issue/rationale. Lies vor und sammle apply|skip je Op ein."),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SubmitPausedGateDecisionsAsync, "submit_paused_gate_decisions",
            "C5b (Gate-Antwort fuer den PAUSIERTEN Lauf): schreibt die vom Autor diktierten forward-Entscheide "
            + "(opId, decision apply|skip — ALLE Ops, Sammel-Akt) als github-forward-decisions.json; der resume "
            + "beantwortet damit den Port. GitHub-Write NUR mit execute-Policy. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SubmitGateDecisionsAsync, "submit_gate_decisions",
            "C5 (WAHRHEITS-WIRKSAM, ein bewusster Sammel-Akt): reicht die vom Autor DIKTIERTEN Gate-Entscheidungen "
            + "ein — decisions je Op (opId, decision apply|skip, reason PFLICHT bei nicht-apply) + alignmentDecisions "
            + "je PBI (accept|edit|skip; bei edit die editierten Felder). Schreibt die Entscheidungs-Datei "
            + "(reviewer=author via steward-chat) und kettet den Apply (R-43). Braucht Autor-Zustimmung.")),
    ];

    private async Task<string> GetPendingReviewAsync(string proposalId)
    {
        var (core, proposal, entry, error) = await LoadOpenAsync(proposalId).ConfigureAwait(false);
        if (error is not null) return error;
        var plan = proposal!.Payload!.Value.Deserialize<PbiStateChangePlanDocument>(Json)!;
        var session = PbiUpdateReviewAdapter.BuildSession("pending", plan, core!);
        return JsonSerializer.Serialize(new
        {
            proposalId,
            ueberholt = entry!.Ueberholt ? entry.UeberholtGrund : null,
            items = session.Items.Select((it, i) => new
            {
                opId = $"op-{i}",
                summary = it.Summary,
                badge = it.Badge,
                notes = it.Notes.Select(n => new { kind = n.Kind.ToString(), label = n.Label, text = n.Text }),
            }),
            alignments = (plan.Alignments ?? []).Select(a => new
            { a.PbiId, a.ProposedTitle, a.ProposedStatement, a.ProposedAcceptanceCriteria, a.Rationale }),
        }, Json);
    }

    private async Task<string> SubmitGateDecisionsAsync(string proposalId,
        IReadOnlyList<PbiUpdateDecision> decisions, IReadOnlyList<PbiAlignmentDecision>? alignmentDecisions = null)
    {
        var (_, proposal, entry, error) = await LoadOpenAsync(proposalId).ConfigureAwait(false);
        if (error is not null) return error;
        var plan = proposal!.Payload!.Value.Deserialize<PbiStateChangePlanDocument>(Json)!;

        var validOpIds = Enumerable.Range(0, plan.Operations.Count).Select(i => $"op-{i}").ToHashSet(StringComparer.Ordinal);
        var errors = new List<string>();
        foreach (var d in decisions)
        {
            if (!validOpIds.Contains(d.OpId)) errors.Add($"unbekannte opId '{d.OpId}' (gueltig: {string.Join(",", validOpIds)})");
            // P2a: jede Nicht-Uebernahme braucht eine Begruendung — auch im Chat-Kanal.
            if (!string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(d.Reason))
                errors.Add($"{d.OpId}: decision '{d.Decision}' OHNE Begruendung (P2a) — Autor nach dem Grund fragen.");
        }
        // Sammel-Akt heißt VOLLSTÄNDIG: unentschiedene Ops würden sonst still als „nicht übernommen"
        // mitgeschlossen (Registry-Close ist ganz-oder-gar-nicht). Vertagen = NICHT submitten (Eintrag bleibt offen).
        foreach (var missing in validOpIds.Except(decisions.Select(d => d.OpId), StringComparer.Ordinal))
            errors.Add($"{missing}: KEINE Entscheidung — der Submit ist ein vollstaendiger Sammel-Akt; Vertagen = nicht submitten.");
        foreach (var a in alignmentDecisions ?? [])
            if (string.Equals(a.Decision, "edit", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrWhiteSpace(a.EditedTitle) && string.IsNullOrWhiteSpace(a.EditedStatement)
                && a.EditedAcceptanceCriteria is not { Count: > 0 })
                errors.Add($"{a.PbiId}: alignment 'edit' ohne editierte Felder.");
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "DECISIONS_INVALID", details = errors }, Json);

        if (entry!.Ueberholt)
            Console.WriteLine($"[steward-gate] ⚠ ÜBERHOLT: {entry.UeberholtGrund}");

        // Materialisieren über DIESELBE Naht wie der UI-Kanal; Entscheidungs-Datei = IDENTISCHER Vertrag.
        var planDir = await PendingReviewMaterializer.MaterializeAsync(proposal).ConfigureAwait(false);
        var file = new PbiUpdateDecisionsFile(
            Path.GetFileName(Path.GetDirectoryName(planDir))!, "author via steward-chat", decisions, alignmentDecisions);
        await File.WriteAllTextAsync(Path.Combine(planDir, "human-decisions.json"),
            JsonSerializer.Serialize(file, Json)).ConfigureAwait(false);

        var exit = await _apply(planDir).ConfigureAwait(false);
        return JsonSerializer.Serialize(exit == 0
            ? new { applied = true, proposalId, planDir = Path.GetRelativePath(repoRoot, planDir), hint = "Registry geschlossen; get_core_overview zeigt den neuen Stand." }
            : (object)new { error = "APPLY_FAILED", exitCode = exit, planDir = Path.GetRelativePath(repoRoot, planDir) }, Json);
    }

    // ── C5b-1 (09.08.): EIN-GRAPH-Gate im Chat — github-forward-gate. Kern-Einsicht: die durable Pause
    // externalisiert das Gate bereits auf DATEIEN (plan + decisions; resume answert den Port daraus) —
    // der 5. Responder braucht KEINE Port-Chirurgie (R-38 unberührt). Execute bleibt Policy-gebunden.
    private const string SupportedGraphGate = "github-forward-gate";

    private async Task<string> GetPausedGateAsync(string runId)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        if (status is null || !string.Equals(status.PausedGate, SupportedGraphGate, StringComparison.OrdinalIgnoreCase))
            return JsonSerializer.Serialize(new { error = "GATE_NOT_SUPPORTED", pausedGate = status?.PausedGate,
                hint = $"C5b-1 kann nur {SupportedGraphGate}; andere Gates: Review-UI (nextRequiredAction)." }, Json);
        var planPath = Path.Combine(repoRoot, "runs", "fullworkflow", runId, "07-github", "github-forward-plan.json");
        if (!File.Exists(planPath))
            return JsonSerializer.Serialize(new { error = "PLAN_MISSING", planPath }, Json);
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(planPath).ConfigureAwait(false));
        var ops = doc.RootElement.GetProperty("operations").EnumerateArray().Select((op, i) => new
        {
            opId = $"op-{i}",
            kind = op.GetProperty("kind").GetString(),
            pbiId = op.GetProperty("pbiId").GetString(),
            targetIssueNumber = op.TryGetProperty("targetIssueNumber", out var t) && t.ValueKind == JsonValueKind.Number ? t.GetInt32() : (int?)null,
            title = op.TryGetProperty("title", out var ti) ? ti.GetString() : null,
            rationale = op.TryGetProperty("rationale", out var r) ? r.GetString() : null,
        }).ToList();
        return JsonSerializer.Serialize(new { runId, gate = SupportedGraphGate, ops,
            hinweis = "Nur apply|skip je Op; GitHub-WRITE passiert erst beim resume und NUR mit execute-Policy." }, Json);
    }

    private async Task<string> SubmitPausedGateDecisionsAsync(string runId, IReadOnlyList<GithubForwardDecision> decisions)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        if (status is null || !string.Equals(status.PausedGate, SupportedGraphGate, StringComparison.OrdinalIgnoreCase))
            return JsonSerializer.Serialize(new { error = "GATE_NOT_SUPPORTED", pausedGate = status?.PausedGate }, Json);
        var dir = Path.Combine(repoRoot, "runs", "fullworkflow", runId, "07-github");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(dir, "github-forward-plan.json")).ConfigureAwait(false));
        var validOpIds = Enumerable.Range(0, doc.RootElement.GetProperty("operations").GetArrayLength())
            .Select(i => $"op-{i}").ToHashSet(StringComparer.Ordinal);

        var errors = new List<string>();
        foreach (var d in decisions)
        {
            if (!validOpIds.Contains(d.OpId)) errors.Add($"unbekannte opId '{d.OpId}'");
            if (!string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(d.Decision, "skip", StringComparison.OrdinalIgnoreCase))
                errors.Add($"{d.OpId}: decision muss apply|skip sein.");
            // P2a auch am Graph-Gate: skip OHNE Begruendung waere ein Chat-Bypass der UI-Regel.
            if (string.Equals(d.Decision, "skip", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(d.Reason))
                errors.Add($"{d.OpId}: skip OHNE Begruendung (P2a) — Autor nach dem Grund fragen.");
        }
        foreach (var missing in validOpIds.Except(decisions.Select(d => d.OpId), StringComparer.Ordinal))
            errors.Add($"{missing}: KEINE Entscheidung — vollstaendiger Sammel-Akt; Vertagen = nicht submitten (Lauf bleibt pausiert).");
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "DECISIONS_INVALID", details = errors }, Json);

        var file = new GithubForwardDecisionsFile(runId, "author via steward-chat", decisions);
        await File.WriteAllTextAsync(Path.Combine(dir, "github-forward-decisions.json"),
            JsonSerializer.Serialize(file, Json)).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { saved = true, runId,
            hint = $"Entscheide gespeichert — naechster Schritt (zustimmungspflichtig): resume_run(\"{runId}\"); GitHub-Write nur mit execute-Policy." }, Json);
    }

    private async Task<(ProjectStateDocument? Core, ProjectStateProposal? Proposal, PendingReviewRegistry.PendingEntry? Entry, string? Error)>
        LoadOpenAsync(string proposalId)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            return (null, null, null, JsonSerializer.Serialize(new { error = "CORE_NOT_FOUND" }, Json));
        var core = await repo.LoadAsync().ConfigureAwait(false);
        var proposal = core.Proposals.FirstOrDefault(p => string.Equals(p.ProposalId, proposalId, StringComparison.OrdinalIgnoreCase)
            && p.Status == PendingReviewRegistry.StatusOpen);
        var entry = PendingReviewRegistry.ListOpen(core).FirstOrDefault(e => string.Equals(e.ProposalId, proposalId, StringComparison.OrdinalIgnoreCase));
        return proposal?.Payload is null || entry is null
            ? (null, null, null, JsonSerializer.Serialize(new { error = "PENDING_NOT_FOUND", proposalId, hint = "offene Eintraege: get_core_overview → pendingReviews" }, Json))
            : (core, proposal, entry, null);
    }
}
