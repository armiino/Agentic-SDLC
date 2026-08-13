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
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SubmitIngestGateDecisionsAsync, "submit_ingest_gate_decisions",
            "C5 (Tor 1 im Chat): schreibt die diktierten Ingest-Entscheide fuer den PAUSIERTEN Lauf — decisions je "
            + "Item (incomingItemId, decision apply|reject, reason PFLICHT bei reject; ALLE Items = Sammel-Akt). "
            + "Der resume beantwortet damit das Gate. Braucht Autor-Zustimmung.")),
        new ApprovalRequiredAIFunction(AIFunctionFactory.Create(SubmitDecisionGateResolutionsAsync, "submit_decision_gate_resolutions",
            "C5 (decision-gate im Chat): je offener Entscheidung action resolve (outcome KEEP_ORIGINAL|ADOPT_NEW|"
            + "REFINE; bei REFINE newStatement) oder defer (vertagen) — ALLE DECs. Schreibt "
            + "decision-gate-decisions.json; resume beantwortet das Gate. Braucht Autor-Zustimmung.")),
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
    private static readonly string[] ChatGates = ["github-forward-gate", "ingest-gate", "arch-ingest-gate", "decision-gate"];

    private string RunDir(string runId) => Path.Combine(repoRoot, "runs", "fullworkflow", runId);

    private async Task<string> GetPausedGateAsync(string runId)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        var gate = status?.PausedGate;
        if (gate is null || !ChatGates.Contains(gate, StringComparer.OrdinalIgnoreCase))
            return JsonSerializer.Serialize(new { error = "GATE_NOT_SUPPORTED", pausedGate = gate,
                hint = $"Chat-faehig: {string.Join(", ", ChatGates)}; andere Gates: Review-UI (nextRequiredAction)." }, Json);

        // C5-Schritt2 (09.08.): ingest/arch-ingest — Vorlage aus dem persistierten plan.json der Stufe.
        if (gate is "ingest-gate" or "arch-ingest-gate")
        {
            var stage = gate == "arch-ingest-gate" ? "07-arch-ingest" : "07-ingest";
            var pPath = Path.Combine(RunDir(runId), stage, "plan.json");
            if (!File.Exists(pPath)) return JsonSerializer.Serialize(new { error = "PLAN_MISSING", pPath }, Json);
            using var pDoc = JsonDocument.Parse(await File.ReadAllTextAsync(pPath).ConfigureAwait(false));
            var items = pDoc.RootElement.GetProperty("operations").EnumerateArray().Select(op => new
            {
                incomingItemId = op.GetProperty("incomingItemId").GetString(),
                kind = op.GetProperty("kind").GetString(),
                statement = op.GetProperty("statement").GetString(),
                targetEntityId = op.TryGetProperty("targetEntityId", out var t) ? t.GetString() : null,
                featureKey = op.TryGetProperty("featureKey", out var f) ? f.GetString() : null,
                rationale = op.TryGetProperty("rationale", out var r) ? r.GetString() : null,
            }).ToList();
            return JsonSerializer.Serialize(new { runId, gate, items,
                hinweis = "Je Item apply|reject (reject IMMER mit Begruendung — P2a); submit_ingest_gate_decisions, danach resume_run." }, Json);
        }

        // decision-gate — Vorlage aus dem persistierten decision-gate-request.json (Aufloesung ENTSTEHT am Gate).
        if (gate == "decision-gate")
        {
            var rPath = Path.Combine(RunDir(runId), "07-decision", "decision-gate-request.json");
            if (!File.Exists(rPath)) return JsonSerializer.Serialize(new { error = "REQUEST_MISSING", rPath }, Json);
            using var rDoc = JsonDocument.Parse(await File.ReadAllTextAsync(rPath).ConfigureAwait(false));
            var decs = rDoc.RootElement.GetProperty("decisions").EnumerateArray().Select(d => new
            {
                decisionId = d.GetProperty("decisionId").GetString(),
                frage = d.GetProperty("decisionText").GetString(),
                bestehendeWahrheit = d.TryGetProperty("targetRequirementText", out var tr) ? tr.GetString() : null,
                meetingVorschlag = d.TryGetProperty("proposedStatement", out var ps) ? ps.GetString() : null,
                blockiertePbis = d.TryGetProperty("blockedPbis", out var bp) ? bp.EnumerateArray().Select(x => x.GetString()).ToList() : [],
            }).ToList();
            return JsonSerializer.Serialize(new { runId, gate, decisions = decs,
                hinweis = "Je DEC: resolve (outcome KEEP_ORIGINAL|ADOPT_NEW|REFINE, bei REFINE newStatement) ODER defer (vertagen = gueltiger Ausgang). submit_decision_gate_resolutions, danach resume_run." }, Json);
        }

        var planPath = Path.Combine(RunDir(runId), "07-github", "github-forward-plan.json");
        if (!File.Exists(planPath))
            return JsonSerializer.Serialize(new { error = "PLAN_MISSING", planPath }, Json);

        // Warn-Note „ungeerntete GitHub-Arbeit" (13.08.): vom SnapshotExecutor geschrieben, wenn die Detect-Engine
        // erntbare Funde sah — der Autor entscheidet informiert („erst ernten?"), bevor er den Write freigibt.
        string? warnung = null;
        var notePath = Path.Combine(RunDir(runId), "07-github", "unharvested-note.json");
        if (File.Exists(notePath))
        {
            using var nDoc = JsonDocument.Parse(await File.ReadAllTextAsync(notePath).ConfigureAwait(false));
            warnung = nDoc.RootElement.TryGetProperty("text", out var nt) ? nt.GetString() : null;
        }

        // Steward-UX-Fund 11.08. („warum sehe ich die aenderung nicht?"): der VOLLE Inhalt, der ans Issue ginge —
        // aus dem sync-delta der pbi-update-Stufe (Statement/AK/Rahmen je PBI), damit der Autor SIEHT, was sich aendert.
        var syncByPbi = await LoadSyncDeltaByPbiAsync(runId).ConfigureAwait(false);
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(planPath).ConfigureAwait(false));
        var ops = doc.RootElement.GetProperty("operations").EnumerateArray().Select((op, i) =>
        {
            var pbiId = op.GetProperty("pbiId").GetString();
            JsonElement? entry = pbiId is not null && syncByPbi.TryGetValue(pbiId, out var e) ? e : null;
            return new
            {
                opId = $"op-{i}",
                kind = op.GetProperty("kind").GetString(),
                pbiId,
                targetIssueNumber = op.TryGetProperty("targetIssueNumber", out var t) && t.ValueKind == JsonValueKind.Number ? t.GetInt32() : (int?)null,
                title = op.TryGetProperty("title", out var ti) ? ti.GetString() : null,
                // Der Inhalt, der bei apply ans Issue geschrieben wuerde (Vorher-Stand = aktuelles Issue, s. UI):
                inhalt = entry is null ? null : new
                {
                    statement = GetStr(entry.Value, "statement"),
                    akzeptanzkriterien = GetStrList(entry.Value, "acceptanceCriteria"),
                    rahmen = GetStrList(entry.Value, "constraints"),
                },
                rationale = op.TryGetProperty("rationale", out var r) ? r.GetString() : null,
            };
        }).ToList();
        return JsonSerializer.Serialize(new { runId, gate = SupportedGraphGate, warnung, ops,
            hinweis = "Je Op steht unter `inhalt` der VOLLE Inhalt (Titel + Statement + Akzeptanzkriterien + Rahmen), der ans Issue geschrieben wuerde — "
                    + "lies ihn dem Autor vor. Vorher/Nachher gegen den aktuellen Issue-Stand: open_gate_ui. Nur apply|skip je Op; GitHub-WRITE erst beim resume + NUR mit execute-Policy."
                    + (warnung is null ? "" : " ⚠ `warnung` DEM AUTOR VORLESEN: drueben liegt ungeerntete Arbeit — Sequenz anbieten: betroffene Ops skippen -> Ernte (run_pipeline_from_github) -> Rest via run_reproject.") }, Json);
    }

    // Steward-UX (11.08.): der bei Apply an die Issues zu schreibende Inhalt je PBI — aus dem sync-delta der pbi-update-Stufe.
    private async Task<Dictionary<string, JsonElement>> LoadSyncDeltaByPbiAsync(string runId)
    {
        var path = Path.Combine(RunDir(runId), "07-pbi-update", "applied", "github-sync-delta.json");
        var map = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (!File.Exists(path)) return map;
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
        if (doc.RootElement.TryGetProperty("entries", out var entries) && entries.ValueKind == JsonValueKind.Array)
            foreach (var e in entries.EnumerateArray())
                if (e.TryGetProperty("pbiId", out var p) && p.GetString() is { } pid)
                    map[pid] = e.Clone();
        return map;
    }

    private static string? GetStr(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static IReadOnlyList<string> GetStrList(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array
            ? v.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => s.Length > 0).ToList() : [];

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

    private async Task<string> SubmitIngestGateDecisionsAsync(string runId, IReadOnlyList<FullWorkflow.Ingestion.IngestGateDecision> decisions)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        var gate = status?.PausedGate;
        if (gate is not ("ingest-gate" or "arch-ingest-gate"))
            return JsonSerializer.Serialize(new { error = "GATE_NOT_SUPPORTED", pausedGate = gate }, Json);
        var stage = gate == "arch-ingest-gate" ? "07-arch-ingest" : "07-ingest";
        using var pDoc = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(RunDir(runId), stage, "plan.json")).ConfigureAwait(false));
        var validIds = pDoc.RootElement.GetProperty("operations").EnumerateArray()
            .Select(op => op.GetProperty("incomingItemId").GetString()!).ToHashSet(StringComparer.Ordinal);

        var errors = new List<string>();
        foreach (var d in decisions)
        {
            if (!validIds.Contains(d.IncomingItemId)) errors.Add($"unbekannte incomingItemId '{d.IncomingItemId}'");
            if (!string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase) && !string.Equals(d.Decision, "reject", StringComparison.OrdinalIgnoreCase))
                errors.Add($"{d.IncomingItemId}: decision muss apply|reject sein.");
            if (string.Equals(d.Decision, "reject", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(d.Reason))
                errors.Add($"{d.IncomingItemId}: reject OHNE Begruendung (P2a).");
        }
        foreach (var missing in validIds.Except(decisions.Select(d => d.IncomingItemId), StringComparer.Ordinal))
            errors.Add($"{missing}: KEINE Entscheidung — Sammel-Akt; Vertagen = nicht submitten (Lauf bleibt pausiert).");
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "DECISIONS_INVALID", details = errors }, Json);

        var file = new FullWorkflow.Ingestion.IngestGateDecisionsFile(runId, "author via steward-chat", decisions);
        await File.WriteAllTextAsync(Path.Combine(RunDir(runId), stage, FullWorkflow.Ingestion.IngestGateDecisions.FileName),
            JsonSerializer.Serialize(file, Json)).ConfigureAwait(false);
        return JsonSerializer.Serialize(new { saved = true, runId, gate,
            hint = $"Entscheide gespeichert — naechster Schritt (zustimmungspflichtig): resume_run(\"{runId}\")." }, Json);
    }

    private async Task<string> SubmitDecisionGateResolutionsAsync(string runId, IReadOnlyList<FullWorkflow.Pipeline.PipelineDecisionResolution> resolutions)
    {
        var status = await FullWorkflow.Pipeline.PipelineRunStatusReader.ReadAsync(repoRoot, runId).ConfigureAwait(false);
        if (status?.PausedGate != "decision-gate")
            return JsonSerializer.Serialize(new { error = "GATE_NOT_SUPPORTED", pausedGate = status?.PausedGate }, Json);
        using var rDoc = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(RunDir(runId), "07-decision", "decision-gate-request.json")).ConfigureAwait(false));
        var validIds = rDoc.RootElement.GetProperty("decisions").EnumerateArray()
            .Select(d => d.GetProperty("decisionId").GetString()!).ToHashSet(StringComparer.Ordinal);

        var errors = new List<string>();
        foreach (var r in resolutions)
        {
            if (!validIds.Contains(r.DecisionId)) errors.Add($"unbekannte decisionId '{r.DecisionId}'");
            var resolve = string.Equals(r.Action, "resolve", StringComparison.OrdinalIgnoreCase);
            if (!resolve && !string.Equals(r.Action, "defer", StringComparison.OrdinalIgnoreCase))
                errors.Add($"{r.DecisionId}: action muss resolve|defer sein.");
            if (resolve && r.Outcome is not ("KEEP_ORIGINAL" or "ADOPT_NEW" or "REFINE"))
                errors.Add($"{r.DecisionId}: resolve braucht outcome KEEP_ORIGINAL|ADOPT_NEW|REFINE.");
            // Kollegen-Glättung 09.08.: ADOPT_NEW braucht den Text ebenfalls explizit (die tiefere Schicht
            // blockt sonst später) — der Agent nimmt ihn wörtlich aus der Vorlage (meetingVorschlag).
            if (resolve && r.Outcome is "REFINE" or "ADOPT_NEW" && string.IsNullOrWhiteSpace(r.NewStatement))
                errors.Add($"{r.DecisionId}: {r.Outcome} OHNE newStatement (bei ADOPT_NEW = meetingVorschlag wörtlich übernehmen).");
        }
        foreach (var missing in validIds.Except(resolutions.Select(r => r.DecisionId), StringComparer.Ordinal))
            errors.Add($"{missing}: KEIN Eintrag — Sammel-Akt (defer = gueltiger Ausgang, explizit angeben).");
        if (errors.Count > 0) return JsonSerializer.Serialize(new { error = "DECISIONS_INVALID", details = errors }, Json);

        var file = new FullWorkflow.Decision.PipelineDecisionDecisionsFile(runId, "author via steward-chat", resolutions);
        await File.WriteAllTextAsync(Path.Combine(RunDir(runId), "07-decision", "decision-gate-decisions.json"),
            JsonSerializer.Serialize(file, Json)).ConfigureAwait(false);
        var deferred = resolutions.Count(r => string.Equals(r.Action, "defer", StringComparison.OrdinalIgnoreCase));
        return JsonSerializer.Serialize(new { saved = true, runId, resolved = resolutions.Count - deferred, deferred,
            hint = $"naechster Schritt (zustimmungspflichtig): resume_run(\"{runId}\")." }, Json);
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
