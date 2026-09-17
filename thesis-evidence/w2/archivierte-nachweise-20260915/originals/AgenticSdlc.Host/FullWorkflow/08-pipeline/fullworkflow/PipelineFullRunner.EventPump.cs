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

/// <summary>9e-light (21.08., mechanischer Neuzuschnitt): der STREAMING-TREIBER — Event-Pumpe, zentraler Gate-Responder-Einstieg, Pause-Mechanik (Checkpoint → pointer.json → Exit 6). Reines Datei-Verschieben aus PipelineFullRunner.cs, null Semantik-Änderung.</summary>
public static partial class PipelineFullRunner
{
    // H1: Resume-Antwortquellen (CLI-Flags beim `resume` — Muster pipeline-hitl `--accept-all`/`--accept`).
    // R-29: Die Flags gelten für GENAU EIN Gate (das pausierte) — danach verbraucht, damit weitere
    // interactive-Gates wieder sauber pausieren (pipeline-hitl: „responded"-Semantik). Entscheid-DATEIEN
    // (human-decisions.json/queue.json) sind davon unberührt und wirken an jedem Gate.
    internal sealed class ResumeAnswers(bool acceptAll, IReadOnlyList<string> acceptIds)
    {
        private bool _consumed;
        public List<string>? TakeFor(IEnumerable<string> allIds)
        {
            if (_consumed) return null;
            var result = acceptAll ? allIds.ToList() : acceptIds.Count > 0 ? acceptIds.ToList() : null;
            if (result is not null) _consumed = true;
            return result;
        }
        public bool TakeAcceptAll()
        {
            if (_consumed || !acceptAll) return false;
            _consumed = true;
            return true;
        }
    }

    // Gemeinsamer Streaming-Treiber: EIN Stream (run) bzw. Restore (resume, H1) + zentraler Gate-Responder +
    // Output-Ernte + Pause-Mechanik (HitlShell-Muster: Checkpoint mit offenem Gate sichern -> pointer.json -> Exit 6).
    // internal für den R-52-Wächter-Test (Resume ohne Antwort muss TERMINIEREN, nie hängen).
    internal static async Task<int> RunWorkflowStreamingAsync<TInput>(
        Workflow workflow, TInput input, string streamRunId, RunContext run, FullWorkflowSettings fw,
        CheckpointManager manager, CancellationToken ct,
        CheckpointInfo? restoreFrom = null, ResumeAnswers? answers = null, bool openUi = false, Action<object>? onOutput = null) where TInput : notnull
    {
        var exit = 0;
        string? pausedGate = null;
        CheckpointInfo? pendingCp = null;
        var pauseReady = false;
        var stillPaused = false;   // R-52: Resume traf ein unbeantwortetes Gate — die BESTEHENDE Pause gilt fort

        await using var handle = restoreFrom is null
            ? await InProcessExecution.RunStreamingAsync(workflow, input, manager, streamRunId).ConfigureAwait(false)
            : await InProcessExecution.OpenStreamingAsync(workflow, manager, streamRunId).ConfigureAwait(false);
        if (restoreFrom is not null)
            await handle.RestoreCheckpointAsync(restoreFrom).ConfigureAwait(false);

        await foreach (var evt in handle.WatchStreamAsync(ct).ConfigureAwait(false))
        {
            if (evt is WorkflowOutputEvent { Data: { } outputData }) onOutput?.Invoke(outputData);
            // H1: Checkpoints mitschneiden; sobald ein interactive-Gate offen ist UND der SuperStep mit dem
            // pending Request gesichert wurde, sauber aussteigen (exakt HitlShell.StartAsync-Mechanik).
            if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
            {
                if (info.Checkpoint is { } cp) pendingCp = cp;
                if (pausedGate is not null && info.HasPendingRequests && pendingCp is not null) pauseReady = true;
            }
            if (pauseReady) break;
            switch (evt)
            {
                case RequestInfoEvent req:
                {
                    // Zentraler Gate-Responder (R-25): Dispatch per PortId, Antwort aus der Policy via GateResponder.
                    var portId = req.Request.PortInfo.PortId;
                    var gatePolicy = fw.GateFor(portId);
                    var answered = false;
                    if (portId == "adjudication-gate" && req.Request.TryGetDataAs<AdjudicationReviewRequest>(out var ar) && ar is not null)
                    {
                        // H2: aktions-typisiertes Gate — eigener Resolver (accept-all = System-Vorschlag-Heuristik;
                        // replay = aufgezeichnete queue.json per ItemId, unmatcht ⇒ reject geloggt).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var recorded = gatePolicy.Kind == GatePolicyKind.Replay ? AdjudicationIo.LoadQueueActions(gatePolicy.ReplayPath) : null;
                            var res = AdjudicationResolver.ResolveActions(ar.Items, gatePolicy, recorded);
                            var actions = res.FilledItems.ToDictionary(i => i.ItemId, i => i.Action!, StringComparer.Ordinal);
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AdjudicationReviewResponse(actions, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = res.AcceptedCount, rejected = res.RejectedCount, unmatched = res.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "arch-classify-gate" && req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRequest>(out var cq) && cq is not null)
                    {
                        // accept-all/replay: Agent-Vorschlag 1:1 uebernehmen (U2-Korrektur ist der interactive Weg).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var accepted = cq.Items.Select(i => new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyProposal(i.ItemId, i.ProposedRoles, i.Rationale, i.ProposedTargets?.Select(t => t.Id).ToList())).ToList();
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewResponse(accepted, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = accepted.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "adr-gate" && req.Request.TryGetDataAs<AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRequest>(out var aq) && aq is not null)
                    {
                        // accept-all/replay: Agent-Entwuerfe 1:1 uebernehmen (U5-Edit ist der interactive Weg).
                        if (gatePolicy.Kind is GatePolicyKind.AcceptAll or GatePolicyKind.Replay)
                        {
                            var acceptedDrafts = aq.Items.Select(i => i.Draft).ToList();
                            await handle.SendResponseAsync(req.Request.CreateResponse(new AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewResponse(acceptedDrafts, $"author (pipeline-full/{gatePolicy.Kind})"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = acceptedDrafts.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId is "ingest-gate" or "arch-ingest-gate" && req.Request.TryGetDataAs<IngestionReviewRequest>(out var ir) && ir is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, ir.Ops.Select(o => new GateItem(o.IncomingItemId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new IngestionReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "decision-gate" && req.Request.TryGetDataAs<PipelineDecisionReviewRequest>(out var dq) && dq is not null)
                    {
                        // R-14 G1 Governance-Politik (Autor 04.08.): accept-all/replay koennen keine Wahrheits-
                        // Konflikte entscheiden (Outcome/neuer Text waeren ERFUNDEN) -> Experiment-Modi vertagen
                        // ALLE Entscheidungen (defer-all): DECs parken sichtbar, der Block haelt, Replay bleibt konstant.
                        if (gatePolicy.Kind != GatePolicyKind.Interactive)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(DecisionStage.DeferAll(dq, $"author (pipeline-full/{gatePolicy.Kind}/defer-all)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), deferred = dq.Decisions.Count, timestampUtc = DateTime.UtcNow });
                            Console.WriteLine($"[{Cmd}] decision-gate: {dq.Decisions.Count} offene Entscheidung(en) VERTAGT (Politik {gatePolicy.Kind}: Experiment-Modi entscheiden keine Wahrheits-Konflikte).");
                            answered = true;
                        }
                    }
                    else if (portId == "pbi-gate" && req.Request.TryGetDataAs<PbiUpdateReviewRequest>(out var pr) && pr is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, pr.Ops.Select(o => new GateItem(o.OpId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new PbiUpdateReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "cluster-review-gate" && req.Request.TryGetDataAs<ClusterReviewRequest>(out var crq) && crq is not null)
                    {
                        // B3: erstes Gate mit echter Replay-Datei-Anbindung (human-decisions.json per OpId; unmatcht => reject).
                        var r = GateResponder.Resolve(gatePolicy, crq.Ops.Select(o => new GateItem(o.OpId)).ToList(), ClusterReviewReplay.Load(gatePolicy.ReplayPath));
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new ClusterReviewResponse(r.AcceptedItemIds, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, rejected = r.RejectedItemIds.Count, unmatched = r.UnmatchedItemIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "backlog-review-gate" && req.Request.TryGetDataAs<BacklogReviewRequest>(out var brq) && brq is not null)
                    {
                        // B4: EDIT-fähiges Gate — eigener Resolver (accept-all = keine Edits; replay = Entscheide
                        // inkl. EditedPbiJson 1:1; fehlender Entscheid = accept, exakt die CLI-Apply-Semantik).
                        var decisions = BacklogReviewResolver.Resolve(gatePolicy);
                        if (decisions is not null)
                        {
                            await handle.SendResponseAsync(req.Request.CreateResponse(new BacklogReviewResponse(decisions, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), decisions = decisions.Count, pbis = brq.PbiIds.Count, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    else if (portId == "github-forward-gate" && req.Request.TryGetDataAs<ForwardReviewRequest>(out var frq) && frq is not null)
                    {
                        var r = GateResponder.Resolve(gatePolicy, frq.Ops.Select(o => new GateItem(o.OpId)).ToList());
                        if (r.Outcome == GateOutcome.Resolved)
                        {
                            // Execute-Flag = fw.Execute: Default false => DRY-RUN (kein realer GitHub-Write). Gates sind heilig.
                            await handle.SendResponseAsync(req.Request.CreateResponse(new ForwardReviewResponse(r.AcceptedItemIds, fw.Execute, "author (pipeline-full)"))).ConfigureAwait(false);
                            run.AppendEvent(new { type = "GATE_ANSWERED", gate = portId, policy = gatePolicy.Kind.ToString(), accepted = r.AcceptedItemIds.Count, execute = fw.Execute, timestampUtc = DateTime.UtcNow });
                            answered = true;
                        }
                    }
                    // H1: interactive — erst die Entscheid-Quellen versuchen (human-decisions.json des Gates
                    // aus den bestehenden Review-UIs; beim resume zusätzlich --accept-all/--accept).
                    // inline (Host-Muster A): am Gate STEHEN BLEIBEN, UI optional öffnen, auf Entscheide warten.
                    // Sonst: durable PAUSE (Host-Muster B).
                    if (!answered && gatePolicy.Kind is GatePolicyKind.Interactive or GatePolicyKind.InteractiveInline)
                    {
                        answered = await TryAnswerInteractiveAsync(handle, req, portId, run, fw, answers).ConfigureAwait(false);
                        if (!answered && gatePolicy.Kind == GatePolicyKind.InteractiveInline)
                            answered = await InlineAnswerLoopAsync(handle, req, portId, run, fw, openUi).ConfigureAwait(false);
                        if (!answered)
                        {
                            pausedGate = portId;
                            // R-52 (Block-E-Live-Fund, 2× Hänger): beim Resume schreitet ohne Antwort KEIN
                            // Superstep voran — es entsteht nie ein neuer Checkpoint, auf den die Pause-
                            // Materialisierung warten könnte. Der RESTAURIERTE Checkpoint IST der Pause-
                            // Zustand: sofort sauber erneut pausieren statt ewig auf den Stream zu warten.
                            if (restoreFrom is not null && pendingCp is null)
                            {
                                pendingCp = restoreFrom;
                                stillPaused = true;
                                pauseReady = true;
                            }
                            else
                            {
                                Console.WriteLine($"[{Cmd}] Gate '{portId}' — pausiere (Checkpoint mit offenem Gate wird gesichert)…");
                            }
                        }
                    }
                    if (!answered && pausedGate is null)
                    {
                        Console.Error.WriteLine($"[{Cmd}] Gate '{portId}' unbekannt/nicht parsebar — nicht beantwortbar.");
                        exit = 5;
                    }
                    break;
                }
                case WorkflowOutputEvent { Data: GithubForwardApplyReport far }:
                    Console.WriteLine($"[{Cmd}] PIPELINE FERTIG (Forward): dryRun={far.DryRun} executed={far.Executed} success={far.Success}.");
                    break;
                case WorkflowOutputEvent { Data: PbiUpdateApplyReport rep }:
                    Console.WriteLine($"[{Cmd}] pbi-update applied newPbis={rep.NewPbis.Count} updatedPbis={rep.UpdatedPbis.Count}");
                    break;
                case WorkflowOutputEvent { Data: ProjectStateDocument psd }:
                    Console.WriteLine($"[{Cmd}] 04-delta OK: items={psd.Items.Count} relations={psd.Relations.Count}");
                    break;
                case WorkflowOutputEvent { Data: CoreBootstrapOutput boot }:
                    Console.WriteLine($"[{Cmd}] core-bootstrap OK: coreItems={boot.CoreItems} (requirements={boot.Requirements}) baseline={boot.BaselinePath}");
                    break;
                case WorkflowOutputEvent { Data: ClusterApplyOutput ca }:
                    Console.WriteLine($"[{Cmd}] cluster-apply OK: applied={ca.Applied} skipped={ca.Skipped} clusters={ca.Clusters} gate={(ca.GatePass ? "pass" : "FAIL")}");
                    break;
                case WorkflowOutputEvent { Data: BacklogSeedOutput bs }:
                    Console.WriteLine($"[{Cmd}] BOOTSTRAP FERTIG (Backlog im Core): features+={bs.FeaturesAdded} pbis+={bs.PbisAdded} relations+={bs.RelationsAdded} core {bs.CoreItemsBefore}->{bs.CoreItemsAfter}");
                    break;
                case WorkflowOutputEvent { Data: ForwardSkipped fs }:
                    // R-50: der Skip ist ein LEGITIMES Lauf-Ende — als FERTIG benennen (Smoke/Autor-Lesbarkeit).
                    Console.WriteLine($"[{Cmd}] PIPELINE FERTIG (Forward übersprungen): {fs.Reason}");
                    break;
                case WorkflowOutputEvent { Data: string s }:
                    Console.Error.WriteLine($"[{Cmd}] {s}");
                    exit = 4;
                    break;
                case ExecutorFailedEvent failed:
                    Console.Error.WriteLine($"[{Cmd}] executor failed: {failed.ExecutorId} {failed.Data?.Message}");
                    exit = 3;
                    break;
                case WorkflowErrorEvent err:
                    Console.Error.WriteLine($"[{Cmd}] workflow error: {err.Exception?.Message}");
                    exit = 3;
                    break;
            }
            if (pauseReady) break;   // R-52: sofortiger Ausstieg — beim Resume kommt kein weiteres Stream-Event
        }

        // H1: PAUSE materialisieren — pointer.json (HitlShell-Format, Mode = Gate) + klare Weiter-Anleitung.
        if (pausedGate is not null)
        {
            if (pendingCp is null)
            {
                Console.Error.WriteLine($"[{Cmd}] PAUSE fehlgeschlagen: kein Checkpoint mit offenem Gate erzeugt.");
                return 4;
            }
            var checkpointDir = run.OutputDir("checkpoints");
            await HitlShell.WritePointerAsync(checkpointDir,
                new HitlPointer(run.RunId, pendingCp.SessionId, pendingCp.CheckpointId, pausedGate, null, null, DateTime.UtcNow)).ConfigureAwait(false);
            run.AppendEvent(new { type = stillPaused ? "PIPELINE_STILL_PAUSED" : "PIPELINE_PAUSED", gate = pausedGate, checkpointId = pendingCp.CheckpointId, timestampUtc = DateTime.UtcNow });
            Console.WriteLine(stillPaused
                ? $"[{Cmd}] UNVERÄNDERT PAUSIERT am Gate '{pausedGate}' — kein Entscheid gefunden (R-52); Checkpoint bleibt gültig. checkpointId={pendingCp.CheckpointId}"
                : $"[{Cmd}] PAUSIERT am Gate '{pausedGate}'. checkpointId={pendingCp.CheckpointId}");
            foreach (var line in ReviewHints(pausedGate, run)) Console.WriteLine(line);
            Console.WriteLine($"[{Cmd}] Fortsetzen: pipeline-full resume {run.RunId}   (oder: … resume {run.RunId} --accept-all)");
            return 6;
        }
        return exit;
    }
}
