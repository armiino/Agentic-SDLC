using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// U2 (ein-graph-vereinheitlichung §7): die Stufengrenzen Betriebs-Zweig + Forward als Graph-Knoten —
// ersetzt die Runner-Phasen RunBackHalfAsync/RunForwardAsync. Muster: IngestPbiBridgeExecutor
// (Artefakte/Core FRISCH laden, typisierte Message senden; Core bleibt System-of-Record).

/// <summary>Forward-Vorbereitung: das zu synchronisierende Delta + Herkunft (initial-sync vs. update).</summary>
public sealed record ForwardPrep(GithubSyncDeltaDocument Delta, bool InitialSync);

/// <summary>Forward wird sauber übersprungen (nichts zu synchronisieren) — terminaler Info-Output.</summary>
public sealed record ForwardSkipped(string Reason);

// BRIDGE: Betriebs-Zweig-Einstieg. OperationalDelta -> Ingest-Input (Core FRISCH laden).
// Übernimmt das STAGE_BACKHALF_START-Event (Metrik-Anker der Betriebs-Fenster).
[SendsMessage(typeof(IngestionResolveInput))]
internal sealed class IngestBridgeExecutor(RunContext run, string repoRoot, int maxAttempts) : Executor<OperationalDelta>("PipelineIngestBridge")
{
    public override async ValueTask HandleAsync(OperationalDelta input, IWorkflowContext context, CancellationToken ct = default)
    {
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            // Erzwungener operational-Modus ohne Core = Config-Fehler -> LAUT (vorher: stiller Skip).
            await context.SendMessageAsync("Core fehlt — Betriebs-Zweig nicht fahrbar (mode=operational auf leerem Core?).").ConfigureAwait(false);
            return;
        }
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);
        var deltaRel = Path.Combine("runs", "fullworkflow", run.RunId, "04-delta", "project-state.json");

        run.AppendEvent(new { type = "STAGE_BACKHALF_START", coreItems = core.Items.Count, deltaItems = input.Delta.Items.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] Betriebs-Zweig: Ingest -> [ingest-gate] -> Apply -> Bridge -> PbiUpdate -> [pbi-gate] -> Apply (Core: {core.Items.Count} items)");
        await context.SendMessageAsync(new IngestionResolveInput(input.Delta, core, deltaRel, maxAttempts)).ConfigureAwait(false);
    }
}

// BRIDGE: Bootstrap -> Forward. Baut das det. initial-sync-Delta (R-15/R-27) aus dem frisch geseedeten Core.
[SendsMessage(typeof(ForwardPrep))]
[SendsMessage(typeof(ForwardSkipped))]
internal sealed class BootstrapForwardBridgeExecutor(RunContext run, string repoRoot) : Executor<BacklogSeedOutput>("PipelineBootstrapForwardBridge")
{
    public override async ValueTask HandleAsync(BacklogSeedOutput input, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var delta = GithubInitialSync.BuildDelta(core);
        var initPath = Path.Combine(run.OutputDir("07-github"), "github-sync-delta.json");
        await File.WriteAllTextAsync(initPath, JsonSerializer.Serialize(delta, HitlShell.Json), ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "STAGE_INITIAL_SYNC_DELTA", newPbis = delta.NewPbis.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] initial-sync (R-15, det.): {delta.NewPbis.Count} unmapped PBIs -> Voll-CREATE-Delta.");

        if (delta.NewPbis.Count == 0)
        {
            await context.SendMessageAsync(new ForwardSkipped("Keine unmapped PBIs — Forward übersprungen.")).ConfigureAwait(false);
            return;
        }
        await context.SendMessageAsync(new ForwardPrep(delta, InitialSync: true)).ConfigureAwait(false);
    }
}

// BRIDGE: Betrieb -> Forward. Nimmt das update-Delta aus dem Pbi-Apply (Datei), sonst sauberer Skip.
[SendsMessage(typeof(ForwardPrep))]
[SendsMessage(typeof(ForwardSkipped))]
internal sealed class OperationalForwardBridgeExecutor(RunContext run, string pbiOutDir) : Executor<PbiUpdateApplyReport>("PipelineOperationalForwardBridge")
{
    public override async ValueTask HandleAsync(PbiUpdateApplyReport report, IWorkflowContext context, CancellationToken ct = default)
    {
        var deltaPath = Path.Combine(pbiOutDir, "applied", "github-sync-delta.json");
        if (!File.Exists(deltaPath))
        {
            await context.SendMessageAsync(new ForwardSkipped("Kein github-sync-delta — Forward übersprungen (nichts zu synchronisieren).")).ConfigureAwait(false);
            return;
        }
        var delta = await HitlShell.LoadAsync<GithubSyncDeltaDocument>(deltaPath).ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_FORWARD_BRIDGE", deltaPbis = delta.Entries.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new ForwardPrep(delta, InitialSync: false)).ConfigureAwait(false);
    }
}

// SNAPSHOT (Design-Note Knoten 10, R-16): frischer READ-ONLY GET des realen Repos VOR Forward; ohne
// repo/token bleibt der Snapshot bewusst leer (Dry-Run plant dann reine CREATEs, kein Token nötig).
// Baut den GithubForwardWfContext und trägt das STAGE_FORWARD_START-Event (Metrik-Anker).
[SendsMessage(typeof(GithubForwardWfContext))]
internal sealed class SnapshotExecutor(RunContext run, string repoRoot, FullWorkflowSettings fw, string outDir, int maxAttempts) : Executor<ForwardPrep>("PipelineSnapshot")
{
    public override async ValueTask HandleAsync(ForwardPrep prep, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync().ConfigureAwait(false);
        var mappingByPbi = CoreGithubMapping.ByPbi(core);

        IReadOnlyList<GithubIssueSnapshot> issues = [];
        string? snapshotRel = null;
        var token = string.IsNullOrWhiteSpace(fw.TokenEnv) ? null : Environment.GetEnvironmentVariable(fw.TokenEnv!);
        if (!string.IsNullOrWhiteSpace(fw.Repo) && !string.IsNullOrWhiteSpace(token))
        {
            try
            {
                issues = await GithubIssueSnapshotRunner.LoadWithGitHubApiAsync(fw.Repo!, 500, token).ConfigureAwait(false);
                var snapshotPath = Path.Combine(outDir, "github-snapshot.json");
                await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(issues, HitlShell.Json), ct).ConfigureAwait(false);
                snapshotRel = Path.GetRelativePath(repoRoot, snapshotPath);
                run.AppendEvent(new { type = "STAGE_SNAPSHOT_DONE", repository = fw.Repo, issues = issues.Count, timestampUtc = DateTime.UtcNow });
                Console.WriteLine($"[pipeline-full] Snapshot (R-16): {issues.Count} Issues von {fw.Repo} geholt (read-only, kein Write).");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[pipeline-full] Snapshot-GET fehlgeschlagen: {ex.Message} -> leerer Snapshot (Forward flaggt evtl. Drift).");
            }
        }
        else
        {
            Console.WriteLine("[pipeline-full] Kein fullworkflow.repo/tokenEnv (oder Token leer) -> leerer Snapshot; Forward-Gate braucht echten Snapshot (R-16).");
        }

        var dryRun = !fw.Execute;
        // R-26: unbeaufsichtigt (accept-all) kein Auto-Issue fuer neue unklare PBIs -> parken (HOLD_CLARIFY).
        var holdUnclear = fw.GateFor("github-forward-gate").Kind == GatePolicyKind.AcceptAll;
        run.AppendEvent(new { type = "STAGE_FORWARD_START", deltaPbis = prep.Delta.Entries.Count, dryRun, initialSync = prep.InitialSync, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline-full] Forward (DRY-RUN={dryRun}): {prep.Delta.Entries.Count} PBI-Ops -> GitHub-Plan (realer Write nur bei execute=true)");

        await context.SendMessageAsync(new GithubForwardWfContext(
            core, prep.Delta.Entries, mappingByPbi, issues, fw.Repo, "pipeline-full", outDir, snapshotRel,
            dryRun, maxAttempts, holdUnclear, prep.InitialSync)).ConfigureAwait(false);
    }
}
