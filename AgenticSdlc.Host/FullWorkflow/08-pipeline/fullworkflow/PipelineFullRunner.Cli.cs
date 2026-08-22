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

/// <summary>9e-light (21.08.): die CLI-VERBEN jenseits von run — status/start/resume + Lauf-Artefakt-Lader. Reines Datei-Verschieben, null Semantik-Änderung.</summary>
public static partial class PipelineFullRunner
{
    // H1: `status` — die EINE Antwort auf „wo ist der letzte Stand?". Quelle der Wahrheit: pointer.json
    // (existiert NUR bei pausiertem Lauf; nach Abschluss gelöscht) + die PIPELINE_*-Events in logs/events.jsonl.
    private static async Task<int> StatusAsync(string[] args, string repoRoot)
    {
        var root = Path.Combine(repoRoot, "runs", "fullworkflow");
        var runToken = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        var dirs = runToken is not null
            ? new[] { Path.IsPathRooted(runToken) ? runToken : Path.Combine(root, runToken) }
            : Directory.Exists(root) ? Directory.GetDirectories(root).OrderBy(d => d, StringComparer.Ordinal).ToArray() : [];

        // A3 (07.08.): status ist nur noch RENDERER über dem geteilten Kern (PipelineRunStatusReader) —
        // dieselbe typisierte Wahrheit, die auch die Steward-Tools (get_run_status) bekommen.
        var paused = 0;
        foreach (var dir in dirs)
        {
            var st = await PipelineRunStatusReader.ReadAsync(repoRoot, dir).ConfigureAwait(false);
            if (st is null) { Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {dir}"); return 2; }
            if (st.State == PipelineRunState.Paused)
            {
                paused++;
                Console.WriteLine($"[{Cmd}] PAUSIERT  {st.RunId}  gate={st.PausedGate}  seit={st.PausedSinceUtc:u}  checkpointId={st.CheckpointId}");
                foreach (var line in st.NextRequiredAction) Console.WriteLine(line);
            }
            else if (runToken is not null)
            {
                Console.WriteLine($"[{Cmd}] {st.RunId}: kein pointer.json — {(st.State == PipelineRunState.Finished ? "FERTIG (metrics.json vorhanden)" : "NICHT pausiert")}. Letztes Pipeline-Event:");
                Console.WriteLine($"  {st.LastPipelineEvent ?? "(keine events.jsonl)"}");
            }
        }
        if (runToken is null)
            Console.WriteLine(paused == 0
                ? $"[{Cmd}] Keine pausierten Läufe unter runs/fullworkflow/."
                : $"[{Cmd}] {paused} pausierte(r) Lauf/Läufe.");

        // R-14 D4: der Parkplatz ist IMMER sichtbar („unsichtbar = rottet") + Kangal-Integritätszeile (read-only) —
        // status ist die eine Seite für „wie geht es der Wahrheit": fachlich Offenes getrennt von strukturell Lautem.
        var coreRepo = new JsonCoreRepository(repoRoot);
        if (await coreRepo.ExistsAsync().ConfigureAwait(false))
        {
            var core = await coreRepo.LoadAsync().ConfigureAwait(false);
            Console.WriteLine($"[{Cmd}] Core ({core.Items.Count} Items):");
            foreach (var line in CoreParkplatz.RenderLines(CoreParkplatz.Count(core), CoreKangal.Check(core)))
                Console.WriteLine(line);
        }
        return 0;
    }

    private static async Task<int> StartAsync(string[] args, string repoRoot)
    {
        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);

        // Transkript: CLI-Arg > run-config.fullworkflow.transcript. Im Skeleton optional (nur validiert, nicht gelesen).
        var transcriptArg = args.Length > 2 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : fw.Transcript;
        string? transcriptPath = null;
        if (!string.IsNullOrWhiteSpace(transcriptArg))
        {
            transcriptPath = Path.IsPathRooted(transcriptArg) ? transcriptArg : Path.Combine(repoRoot, transcriptArg);
            if (!File.Exists(transcriptPath))
            {
                Console.Error.WriteLine($"[{Cmd}] Transkript nicht gefunden: {transcriptPath}");
                return 2;
            }
        }

        var run = new RunContext(RunId.New(), "fullworkflow");
        run.EnsureFolders();
        var plan = Plan(fw);

        // Faden-Unterordner (distinct) + checkpoints — Design-Note §7.
        foreach (var folder in plan.Select(p => p.Folder).Distinct(StringComparer.Ordinal))
            run.OutputDir(folder);
        run.OutputDir("checkpoints");

        run.WriteConfig(new
        {
            command = Cmd,
            runId = run.RunId,
            transcript = transcriptPath is null ? null : Path.GetRelativePath(repoRoot, transcriptPath),
            execute = fw.Execute,
            policyProfile = fw.PolicyProfile.Kind.ToString(),
            l3Enabled = fw.L3Enabled,
            models = fw.Models,
            maxAttempts = fw.MaxAttempts,
            skeleton = true,
            timestampUtc = DateTime.UtcNow
        });

        run.AppendEvent(new { type = "PIPELINE_START", runId = run.RunId, mode = "skeleton", execute = fw.Execute, stages = plan.Count, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] start runId={run.RunId} mode=SKELETON execute={fw.Execute} policyProfile={fw.PolicyProfile.Kind} l3={(fw.L3Enabled ? "on" : "off")}");
        Console.WriteLine($"[{Cmd}] Flow ({plan.Count} Stufen):");
        for (var i = 0; i < plan.Count; i++)
        {
            var st = plan[i];
            var isDryRunGithub = st.Id == "07-github" && !fw.Execute;
            var gateStr = st.Gate is null ? "" : $"  Gate={st.Gate} policy={fw.GateFor(st.Gate).Kind}";
            var dryStr = isDryRunGithub ? "  (DRY-RUN)" : "";
            run.AppendEvent(new
            {
                type = "STAGE_PLANNED",
                index = i + 1,
                stage = st.Id,
                folder = st.Folder,
                gate = st.Gate,
                policy = st.Gate is null ? null : fw.GateFor(st.Gate).Kind.ToString(),
                dryRun = isDryRunGithub,
                timestampUtc = DateTime.UtcNow
            });
            Console.WriteLine($"  [{i + 1}/{plan.Count}] {st.Id}{gateStr}{dryStr}");
        }
        run.AppendEvent(new { type = "PIPELINE_SKELETON_DONE", note = "Graph-Ausfuehrung folgt Bauplan-Schritt 3-4", timestampUtc = DateTime.UtcNow });

        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{run.RunId}/  (Ordner + config.json + events.jsonl; KEIN LLM, KEIN Write)");
        await Task.CompletedTask.ConfigureAwait(false);
        return 0;
    }

    // H1: Resume eines PAUSIERTEN Laufs — Checkpoint restaurieren (identischer Graph via BuildGraph), das
    // re-emittierte Gate beantworten (human-decisions.json des Gates ODER --accept-all/--accept id1,id2),
    // weiterfahren; ggf. am NÄCHSTEN Gate erneut pausieren. Muster: pipeline-hitl (bewiesen) — durch den
    // Ein-Graph gibt es genau EINEN Restore-Punkt, kein Phasen-Pointer.
    private static async Task<int> ResumeAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var acceptAll = args.Contains("--accept-all", StringComparer.OrdinalIgnoreCase);
        string? runToken = null, acceptList = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--accept-all", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--open-ui", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--accept", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { acceptList = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[{Cmd}] unbekanntes Argument: {a}"); return 2; }
            runToken ??= a;
        }
        if (runToken is null) return Usage();

        var runDir = Path.IsPathRooted(runToken) ? runToken : Path.Combine(repoRoot, "runs", "fullworkflow", runToken);
        if (!Directory.Exists(runDir)) { Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {runDir}"); return 2; }
        var runId = Path.GetFileName(runDir.TrimEnd('/', '\\'));
        var checkpointDir = Path.Combine(runDir, "checkpoints");
        var pointer = await HitlShell.LoadPointerAsync(Cmd, checkpointDir).ConfigureAwait(false);
        if (pointer is null) return 2;

        var config = RunConfig.Load(repoRoot);
        var fw = FullWorkflowSettings.FromConfig(config.FullWorkflow);
        var run = new RunContext(runId, "fullworkflow");

        // Faden-Exporter auch beim Resume (nach dem Gate können weitere LLM-Stufen laufen).
        using var fadenOtel = OtelRunExporters.TryCreate(
            settings.OtelEnabled, "AgenticSdlc.Host",
            Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        // Schritt 5 ② / H1: das Konstruktions-Transkript der Ledger-Kapsel aus der Lauf-config rekonstruieren
        // (identischer Graph!). Delta-Läufe haben keins (Kapsel bleibt unangesprochen). Der Intake-Guard
        // (LEDGER_TRANSCRIPT_MISMATCH) fängt eine zwischenzeitlich editierte Transkript-Datei laut ab.
        var (resumeTranscriptText, resumeTranscriptName) = await LoadRunTranscriptAsync(run, repoRoot).ConfigureAwait(false);
        var (workflow, _, _, _) = BuildGraph(run, settings, fw, repoRoot, resumeTranscriptText, resumeTranscriptName,
            archCatchup: await LoadRunArchCatchupAsync(run).ConfigureAwait(false));
        var answers = new ResumeAnswers(acceptAll,
            string.IsNullOrWhiteSpace(acceptList) ? [] : acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList());

        using var store = new FileSystemJsonCheckpointStore(new DirectoryInfo(checkpointDir));
        var manager = CheckpointManager.CreateJson(store, HitlShell.Json);
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(fw.TimeoutMinutes));

        run.AppendEvent(new { type = "PIPELINE_RESUME", runId, gate = pointer.Mode, checkpointId = pointer.CheckpointId, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[{Cmd}] resume runId={runId} gate={pointer.Mode ?? "?"} checkpointId={pointer.CheckpointId}");

        var exit = await RunWorkflowStreamingAsync<TranscriptInput>(
            workflow, default!, runId, run, fw, manager, cts.Token,
            restoreFrom: new CheckpointInfo(pointer.SessionId, pointer.CheckpointId), answers: answers,
            openUi: args.Contains("--open-ui")).ConfigureAwait(false);

        if (exit == 6)
        {
            Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{runId}/  (erneut PAUSIERT — nächstes Gate)");
            return 0;
        }

        // Pointer-Lebenszyklus: nach echtem Lauf-Ende den Pause-Zeiger LÖSCHEN — `status` zeigt sonst einen
        // fertigen Lauf als pausiert, und ein versehentliches resume würde ein beantwortetes Gate restaurieren.
        var pointerPath = Path.Combine(checkpointDir, "pointer.json");
        if (File.Exists(pointerPath))
        {
            File.Delete(pointerPath);
            run.AppendEvent(new { type = "PIPELINE_POINTER_CLEARED", runId, timestampUtc = DateTime.UtcNow });
        }

        run.AppendEvent(new { type = "PIPELINE_RUN_DONE", runId, exit, timestampUtc = DateTime.UtcNow });
        fadenOtel?.ForceFlush();
        var coreRepo = new JsonCoreRepository(repoRoot);
        var coreAfter = await coreRepo.ExistsAsync().ConfigureAwait(false)
            ? (await coreRepo.LoadAsync().ConfigureAwait(false)).Items.Count : 0;
        await MetricsFinalizer.WriteAsync(run, repoRoot, fw, settings, coreAfter, cts.Token).ConfigureAwait(false);
        Console.WriteLine($"[{Cmd}] Faden: runs/fullworkflow/{runId}/  exit={exit}");
        return exit;
    }

    // Schritt 5 ② / H1: Transkript-Recovery fürs Resume — liest den `transcript`-Pfad aus der config.json des
    // Laufs (seit ③ geschrieben) und lädt den Text neu. Delta-Läufe (transcript=null) liefern leer.
    private static async Task<(string Text, string SourceName)> LoadRunTranscriptAsync(RunContext run, string repoRoot)
    {
        if (!File.Exists(run.ConfigPath)) return ("", "");
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(run.ConfigPath).ConfigureAwait(false));
        if (!doc.RootElement.TryGetProperty("transcript", out var t) || t.ValueKind != JsonValueKind.String) return ("", "");
        var rel = t.GetString()!;
        var path = Path.IsPathRooted(rel) ? rel : Path.Combine(repoRoot, rel);
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"[{Cmd}] WARNUNG: Transkript des Laufs nicht mehr auffindbar: {path} (Kapsel-Konstruktion mit leerem Transkript — Intake-Guard greift, falls die Front noch läuft).");
            return ("", Path.GetFileName(path));
        }
        return (await File.ReadAllTextAsync(path).ConfigureAwait(false), Path.GetFileName(path));
    }

    // 1d Catch-up-Schalter: der Scope ist ein LAUF-Fakt (config.json des Laufs) — Resume rebaut den Graph
    // mit demselben Scope wie der Start, egal welche CLI-Flags der Resume-Aufruf trägt.
    private static async Task<bool> LoadRunArchCatchupAsync(RunContext run)
    {
        if (!File.Exists(run.ConfigPath)) return false;
        using var doc = JsonDocument.Parse(await File.ReadAllTextAsync(run.ConfigPath).ConfigureAwait(false));
        return doc.RootElement.TryGetProperty("archCatchup", out var c) && c.ValueKind == JsonValueKind.True;
    }
}
