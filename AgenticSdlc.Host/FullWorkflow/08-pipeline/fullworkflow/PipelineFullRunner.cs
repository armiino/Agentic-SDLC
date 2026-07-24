using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' — <c>pipeline-full</c>: die GANZE Kette als EIN durabler MAF-Graph mit zentralem Gate-Responder.
/// </summary>
/// <remarks>
/// Bauplan-Schritt 2 (Run-Faden + minimale Shell): <c>start</c> liest <c>run-config.fullworkflow</c>, legt den
/// Faden-Ordner + Unterordner + config-Snapshot an und schreibt den Stufen-Plan als Events (SKELETON, KEIN LLM,
/// KEIN externer Write). Der eigentliche Graph-Bau + Gate-Responder folgen in Schritt 3-4; <c>resume</c> ist
/// bis dahin ein bewusster No-op. Der Stufen-Plan (<see cref="Plan"/>) ist die EINE Quelle für Ordnernamen,
/// Events und die Konsolen-Flow-Leiste.
/// </remarks>
public static class PipelineFullRunner
{
    private const string Cmd = "pipeline-full";

    private sealed record StagePlan(string Id, string Folder, string? Gate);

    private static IReadOnlyList<StagePlan> Plan(FullWorkflowSettings s)
    {
        var stages = new List<StagePlan>
        {
            new("01-ledger",     "01-ledger",     null),                // Quality-Gate det. (kein RequestPort)
            new("adjudication",  "01-ledger",     "adjudication-gate"), // NEUER RequestPort-Gate (Schritt 3)
            new("02-baselines",  "02-baselines",  null),                // Checker intern
        };
        if (s.L3Enabled) stages.Add(new("03-gap", "03-gap", null));     // v1: per Default aus
        stages.Add(new("04-delta",      "04-delta",      null));        // det. Executor (Spike 1)
        stages.Add(new("07-ingest",     "07-ingest",     "ingest-gate"));
        stages.Add(new("07-pbi-update", "07-pbi-update", "pbi-gate"));
        stages.Add(new("snapshot",      "07-github",     null));        // Pflicht VOR Forward (R-16)
        stages.Add(new("07-github",     "07-github",     "github-forward-gate"));
        return stages;
    }

    public static Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => StartAsync(args, repoRoot),
            "resume" => ResumeAsync(args, repoRoot),
            _ => Task.FromResult(Usage()),
        };
    }

    private static int Usage()
    {
        Console.Error.WriteLine("Usage:");
        Console.Error.WriteLine("  pipeline-full start [<transcript.txt>]   (Faden + Skeleton; Transkript sonst aus run-config.fullworkflow.transcript)");
        Console.Error.WriteLine("  pipeline-full resume <runId>             (Graph-Bau folgt Bauplan-Schritt 4)");
        return 2;
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

    private static async Task<int> ResumeAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3) return Usage();
        var runToken = args[2];
        var runDir = Path.IsPathRooted(runToken) ? runToken : Path.Combine(repoRoot, "runs", "fullworkflow", runToken);
        if (!Directory.Exists(runDir))
        {
            Console.Error.WriteLine($"[{Cmd}] Lauf nicht gefunden: {runDir}");
            return 2;
        }
        Console.WriteLine($"[{Cmd}] resume runId={Path.GetFileName(runDir)} — Graph/Checkpoints noch nicht gebaut (Bauplan-Schritt 4). Skeleton-Resume ist ein No-op.");
        await Task.CompletedTask.ConfigureAwait(false);
        return 0;
    }
}
