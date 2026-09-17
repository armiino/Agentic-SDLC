using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>9e-light (21.08.): die GATE-ANTWORT-QUELLEN des zentralen Responders — Entscheid-Dateien je Gate, interactive-inline-Loop, UI-Start, Forward-Entscheid-Mengen (inkl. R-60 overwrite). Reines Datei-Verschieben, null Semantik-Änderung.</summary>
public static partial class PipelineFullRunner
{
    // H1: Gate-spezifische Review-Anleitung für die Pause (bestehende UIs zeigen auf die Faden-Ordner).
    // A3 (07.08.): die Gate→Anleitung-Quelle wohnt jetzt im geteilten Status-Kern (PipelineRunStatusReader)
    // — Pause-Meldung, status-CLI und Steward-Tools sprechen damit IDENTISCH. Hier nur Delegation.
    private static IReadOnlyList<string> ReviewHints(string gate, RunContext run)
        => PipelineRunStatusReader.NextRequiredAction(gate, run.RunId);

    // H1: interactive-Antwortquellen je Gate — 1) human-decisions.json (aus den bestehenden Review-UIs, im
    // Faden-Ordner des Gates), 2) Resume-CLI-Flags. Liefert false => Pause. Semantik je Gate = exakt die
    // jeweilige Apply-/Replay-Semantik (cluster: apply-OpIds; backlog: Entscheide inkl. Edits; sonst accept-Ids).
    private static async Task<bool> TryAnswerInteractiveAsync(
        StreamingRun handle, RequestInfoEvent req, string portId, RunContext run, FullWorkflowSettings fw, ResumeAnswers? answers)
    {
        // R-29: Flags einmalig (TakeFor/TakeAcceptAll konsumieren) — Entscheid-Dateien bleiben mehrfach nutzbar.
        List<string>? Flags(IEnumerable<string> all) => answers?.TakeFor(all);

        async Task<bool> AnswerAsync<T>(T payload, object evtFields) where T : notnull
        {
            await handle.SendResponseAsync(req.Request.CreateResponse(payload)).ConfigureAwait(false);
            run.AppendEvent(evtFields);
            return true;
        }

        switch (portId)
        {
            case "adjudication-gate" when req.Request.TryGetDataAs<AdjudicationReviewRequest>(out var a) && a is not null:
            {
                // H2 interactive: die vom Menschen/UI gefüllte queue.json (ledger-adjudicate-ui <QueuePath>)
                // ist die Entscheid-Quelle; --accept-all = System-Vorschlag-Heuristik (EXPERIMENT).
                var recorded = AdjudicationIo.LoadQueueActions(a.QueuePath);
                AdjudicationActionResolution? res = null;
                if (recorded is not null)
                    res = AdjudicationResolver.ResolveActions(a.Items, new GatePolicy(GatePolicyKind.Replay, a.QueuePath), recorded);
                else if (answers?.TakeAcceptAll() == true)
                    res = AdjudicationResolver.ResolveActions(a.Items, new GatePolicy(GatePolicyKind.AcceptAll));
                if (res is null) return false;
                var actions = res.FilledItems.ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
                return await AnswerAsync(new AdjudicationReviewResponse(actions, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = recorded is not null ? "queue.json" : "cli-flags", accepted = res.AcceptedCount, rejected = res.RejectedCount, unmatched = res.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "cluster-review-gate" when req.Request.TryGetDataAs<ClusterReviewRequest>(out var c) && c is not null:
            {
                var map = ClusterReviewReplay.Load(Path.Combine(run.RunDir, "06-backlog", "clusters", "human-decisions.json"));
                var accepted = map is not null
                    ? c.Ops.Where(o => map.TryGetValue(o.OpId, out var apply) && apply).Select(o => o.OpId).ToList()
                    : Flags(c.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                return await AnswerAsync(new ClusterReviewResponse(accepted, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = map is not null ? "human-decisions" : "cli-flags", accepted = accepted.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "backlog-review-gate" when req.Request.TryGetDataAs<BacklogReviewRequest>(out var b) && b is not null:
            {
                var decisions = BacklogReviewResolver.LoadDecisions(Path.Combine(run.RunDir, "06-backlog", "backlog", "human-decisions.json"));
                if (decisions is null && answers?.TakeAcceptAll() == true) decisions = []; // leere Liste = alle accept (CLI-Semantik)
                if (decisions is null) return false;
                return await AnswerAsync(new BacklogReviewResponse(decisions, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", decisions = decisions.Count, pbis = b.PbiIds.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "arch-classify-gate" when req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRequest>(out var cq) && cq is not null:
            {
                // interactive: 1) classify-decisions.json (U2-Edit-UI, A2-3) · 2) --accept-all (Vorschlag 1:1) · sonst Pause.
                var decisionsPath = Path.Combine(run.RunDir, "07-arch-classify", "classify-decisions.json");
                if (File.Exists(decisionsPath))
                {
                    var file = JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyDecisionsFile>(
                        await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), HitlShell.Json);
                    if (file is not null)
                        return await AnswerAsync(AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewAdapter.ToResponse(file, "human (review-ui)"),
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = "classify-decisions", accepted = file.Decisions.Count, deferred = cq.Items.Count - file.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                }
                if (answers?.TakeAcceptAll() != true) return false;
                var acc = cq.Items.Select(i => new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyProposal(i.ItemId, i.ProposedRoles, i.Rationale, i.ProposedTargets?.Select(t => t.Id).ToList())).ToList();
                return await AnswerAsync(new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewResponse(acc, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = acc.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "adr-gate" when req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRequest>(out var aq) && aq is not null:
            {
                // interactive: 1) adr-decisions.json (U5-Abnahme-UI) · 2) --accept-all (Entwuerfe 1:1) · sonst Pause.
                var adrDecisionsPath = Path.Combine(run.RunDir, "07-adr", "adr-decisions.json");
                if (File.Exists(adrDecisionsPath))
                {
                    var file = JsonSerializer.Deserialize<AgenticSdlc.Host.FullWorkflow.Adr.AdrDecisionsFile>(
                        await File.ReadAllTextAsync(adrDecisionsPath).ConfigureAwait(false), HitlShell.Json);
                    if (file is not null)
                        return await AnswerAsync(AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewAdapter.ToResponse(file, "human (review-ui)"),
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = "adr-decisions", accepted = file.Decisions.Count, deferred = aq.Items.Count - file.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                }
                if (answers?.TakeAcceptAll() != true) return false;
                var accAdr = aq.Items.Select(i => i.Draft).ToList();
                return await AnswerAsync(new AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewResponse(accAdr, "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accAdr.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "ingest-gate" or "arch-ingest-gate" when req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null:
            {
                // R-44 (09.08.): Datei-Vertrag zuerst (UI/Steward-Chat schreiben ingest-gate-decisions.json) —
                // vorher war Tor 1 beim resume NUR per accept-all beantwortbar (kein per-Item-Entscheid).
                var stageDir = string.Equals(portId, "arch-ingest-gate", StringComparison.Ordinal) ? "07-arch-ingest" : "07-ingest";
                var fileDecision = Ingestion.IngestGateDecisions.TryLoadAny(Path.Combine(run.RunDir, stageDir));   // 3b-2: Chat-Vertrag ODER UI-Datei
                var accepted = fileDecision?.Accepted ?? Flags(ir.Ops.Select(o => o.IncomingItemId));
                if (accepted is null) return false;
                return await AnswerAsync(new IngestionReviewResponse(accepted, fileDecision?.Reviewer ?? "author (interactive)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, source = fileDecision is null ? "flags" : "decisions-file", timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "decision-gate" when req.Request.TryGetDataAs<PipelineDecisionReviewRequest>(out var dq) && dq is not null:
            {
                // G1-b: die UI (decision-gate-review) schreibt decision-gate-decisions.json — die ist die Antwort
                // (DECs ohne Eintrag werden sicher VERTAGT). Ohne Datei: Flags koennen keine Outcomes erfinden ->
                // --accept-all/'a' = laut defer-all; sonst warten (Inline-Loop/UI).
                var decisionsPath = Path.Combine(run.RunDir, "07-decision", "decision-gate-decisions.json");
                if (File.Exists(decisionsPath))
                {
                    var file = JsonSerializer.Deserialize<Decision.PipelineDecisionDecisionsFile>(
                        await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), JsonFiles.Json);
                    if (file is not null)
                    {
                        var response = Decision.PipelineDecisionReviewAdapter.ToResponse(file, dq);
                        var resolvedCount = response.Resolutions.Count(r => string.Equals(r.Action, DecisionStage.ActionResolve, StringComparison.OrdinalIgnoreCase));
                        return await AnswerAsync(response,
                            new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", resolved = resolvedCount, deferred = response.Resolutions.Count - resolvedCount, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
                    }
                }
                // R-55 (18.08., Block-L-Live-Fund): Defer-all ist ein EXPERIMENT-Akt und feuert NUR noch bei
                // explizitem --accept-all. Vorher genügte das bloße Antwort-Objekt (beim Resume IMMER da) —
                // das „warten auf die UI" darunter war totes Recht, Tor 2 wurde stumm vertagt, BEVOR der
                // Autor die decision-gate-UI je sah (seine Entscheide lagen unangewendet in der Datei).
                // Jetzt: ohne Datei + ohne Flag ⇒ unbeantwortet ⇒ R-52-Mechanik re-pausiert sauber.
                if (answers?.TakeAcceptAll() != true) return false;   // warten: UI nutzen (decision-gate-review), resume liest die Datei
                Console.WriteLine($"[{Cmd}] decision-gate: {dq.Decisions.Count} offene Entscheidung(en) — per --accept-all alle VERTAGT (Experiment-Akt; interaktiv: decision-gate-review).");
                return await AnswerAsync(DecisionStage.DeferAll(dq, "author (resume/defer-all)"),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", source = "accept-all-flag", deferred = dq.Decisions.Count, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "pbi-gate" when req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr) && pr is not null:
            {
                // R-44b (09.08.): Datei-Vertrag zuerst — die pbi-update-Review-UI kann auf 07-pbi-update zeigen
                // (pbi-change-plan.json liegt dort) und schreibt human-decisions.json; vorher nur Flags.
                IReadOnlyList<string>? accepted = null; var reviewer = "author (interactive)";
                IReadOnlyList<PbiAlignment>? aligns = null;   // R-26-C-Fix: akzeptierte Angleichungen MITFÜHREN (sonst fällt needs_clarify im Graphen nie)
                var pbiStage = Path.Combine(run.RunDir, "07-pbi-update");
                var pbiDecisionsPath = Path.Combine(pbiStage, "human-decisions.json");
                if (File.Exists(pbiDecisionsPath) && File.Exists(Path.Combine(pbiStage, "pbi-change-plan.json")))
                {
                    var stagePlan = JsonSerializer.Deserialize<PbiUpdate.PbiStateChangePlanDocument>(
                        await File.ReadAllTextAsync(Path.Combine(pbiStage, "pbi-change-plan.json")).ConfigureAwait(false), JsonFiles.Json)!;
                    var df = JsonSerializer.Deserialize<PbiUpdate.PbiUpdateDecisionsFile>(
                        await File.ReadAllTextAsync(pbiDecisionsPath).ConfigureAwait(false), JsonFiles.Json)!;
                    (accepted, aligns, reviewer) = ResolvePbiGateDecisions(stagePlan, df);
                }
                accepted ??= Flags(pr.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                return await AnswerAsync(new PbiUpdateReviewResponse(accepted, reviewer, aligns),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, alignments = aligns?.Count ?? 0, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
            case "github-forward-gate" when req.Request.TryGetDataAs<ForwardReviewRequest>(out var f) && f is not null:
            {
                // Zwei-Bahnen (17.08., E8-Live-Fund): Steward-decide_gate schreibt github-forward-decisions.json,
                // die Review-UI (github-forward-review) human-decisions.json — beide gelten (Schema identisch).
                var sets = LoadForwardDecisions(Path.Combine(run.RunDir, "07-github", "github-forward-decisions.json"))
                           ?? LoadForwardDecisions(Path.Combine(run.RunDir, "07-github", "human-decisions.json"));
                var accepted = sets?.Accepted ?? Flags(f.Ops.Select(o => o.OpId));
                if (accepted is null) return false;
                // Execute-Flag bleibt Policy-gebunden (fw.Execute) — auch interactive kein Write ohne execute=true.
                // R-60: overwrite kommt NUR aus expliziten Datei-Entscheiden — Flags/accept-all setzen es nie.
                return await AnswerAsync(new ForwardReviewResponse(accepted, fw.Execute, "author (interactive)", sets?.Overwrite ?? []),
                    new { type = "GATE_ANSWERED", gate = portId, policy = "Interactive", accepted = accepted.Count, overwrite = sets?.Overwrite.Count ?? 0, execute = fw.Execute, timestampUtc = DateTime.UtcNow }).ConfigureAwait(false);
            }
        }
        return false;
    }

    // interactive-inline (Host-Muster A): am Gate stehen bleiben, UI optional öffnen, auf Entscheide warten.
    // ENTER = Entscheid-Datei erneut prüfen; 'a'+ENTER = accept-all (einmalig); stdin-EOF (kein Terminal,
    // z. B. Hintergrund-Lauf) => false => sauberer Fallback auf die durable Pause.
    private static async Task<bool> InlineAnswerLoopAsync(
        StreamingRun handle, RequestInfoEvent req, string portId, RunContext run, FullWorkflowSettings fw, bool openUi)
    {
        Console.WriteLine($"[{Cmd}] Gate '{portId}' interactive-inline — der Lauf WARTET auf deine Entscheide:");
        foreach (var line in ReviewHints(portId, run)) Console.WriteLine(line);
        if (openUi) StartUi(portId, run);

        while (true)
        {
            Console.Write($"[{Cmd}] Entscheide gespeichert? ENTER = prüfen · 'a'+ENTER = accept-all · 'p'+ENTER = pausieren: ");
            var input = Console.ReadLine();
            if (input is null) // kein Terminal (EOF) -> durable Pause statt Endlosschleife
            {
                Console.WriteLine($"[{Cmd}] Kein Terminal (stdin EOF) — falle auf durable Pause zurück.");
                return false;
            }
            if (input.Trim().Equals("p", StringComparison.OrdinalIgnoreCase)) return false;

            var oneShot = input.Trim().Equals("a", StringComparison.OrdinalIgnoreCase)
                ? new ResumeAnswers(acceptAll: true, acceptIds: [])
                : null;
            if (await TryAnswerInteractiveAsync(handle, req, portId, run, fw, oneShot).ConfigureAwait(false))
                return true;
            Console.WriteLine($"[{Cmd}] Noch keine Entscheide gefunden (Datei fehlt/leer) — UI nutzen und erneut ENTER.");
        }
    }

    // UI-Prozess für das Gate starten (eigene Host-Instanz, gleiche Binary). Best effort — Fehler nur melden.
    private static void StartUi(string gate, RunContext run)
    {
        var uiArgs = gate switch
        {
            "adjudication-gate" => $"ledger-adjudicate-ui runs/fullworkflow/{run.RunId}/01-ledger/queue.json",
            "cluster-review-gate" => $"l4-re-clarify-review runs/fullworkflow/{run.RunId}/06-backlog/clusters",
            "backlog-review-gate" => $"l4-re-clarify-backlog-review runs/fullworkflow/{run.RunId}/06-backlog/backlog",
            "decision-gate" => $"decision-gate-review runs/fullworkflow/{run.RunId}/07-decision",
            _ => null,
        };
        if (uiArgs is null)
        {
            Console.WriteLine($"[{Cmd}] --open-ui: für '{gate}' gibt es (noch) keine direkte UI — Entscheid per Datei/Flags.");
            return;
        }
        try
        {
            var repoRoot = Path.GetFullPath(Path.Combine(run.RunDir, "..", "..", ".."));
            var psi = new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!, uiArgs)
            {
                WorkingDirectory = repoRoot,
                UseShellExecute = false,
            };
            System.Diagnostics.Process.Start(psi);
            Console.WriteLine($"[{Cmd}] --open-ui: gestartet: {Path.GetFileName(Environment.ProcessPath!)} {uiArgs}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{Cmd}] --open-ui fehlgeschlagen ({ex.Message}) — UI manuell starten: {uiArgs}");
        }
    }

    // Entscheide der Forward-Review (bekanntes GithubForwardDecisionsFile-Format) → akzeptierte OpIds
    // + R-60: 'overwrite' = bewusstes Drift-Überschreiben (eigener Entscheid-Wert, NIE aus apply abgeleitet).
    private sealed record ForwardDecisionSets(List<string> Accepted, List<string> Overwrite);
    private static ForwardDecisionSets? LoadForwardDecisions(string path)
    {
        if (!File.Exists(path)) return null;
        var file = JsonSerializer.Deserialize<GithubForwardDecisionsFile>(File.ReadAllText(path), HitlShell.Json);
        if (file is null) return null;
        return new ForwardDecisionSets(
            [.. file.Decisions.Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId)],
            [.. file.Decisions.Where(d => string.Equals(d.Decision, "overwrite", StringComparison.OrdinalIgnoreCase)).Select(d => d.OpId)]);
    }
    // U2: Betriebs-Zweig (Ingest->Pbi) und Forward sind KEIN Runner-Code mehr — sie leben als Kanten in
    // PipelineFullWorkflow.Assemble (IngestBridge/ForwardBridges/SnapshotExecutor). Historie:
    // RunBackHalfAsync + RunForwardAsync (Phasen 2/3) — ersetzt am 27.07. (ein-graph-vereinheitlichung U2).
}
