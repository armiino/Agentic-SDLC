using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Checkpointing;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.HitlSpike;

// S0 — Verifikations-Spike (Worklist 20.07, Step S0). ZWECK: mit Bordmitteln beweisen, dass ein MAF-Workflow
//   (1) an einem Human-Gate (RequestPort) pausiert,
//   (2) den State durabel auf Platte legt (FileSystemJsonCheckpointStore) und
//   (3) in einem KOMPLETT NEUEN Prozess fortgesetzt werden kann (ResumeStreamingAsync),
//       wobei der offene Request erneut als RequestInfoEvent hochkommt und per SendResponseAsync beantwortet wird.
//
// Das ist die tragende Annahme des gesamten B/A-Plans (siehe Decision 20.07 §1). Bewusst WEGWERF/ISOLIERT:
// deterministisch (kein LLM), kein Core-Zugriff, eigener Run-Ordner. Nicht als Referenzarchitektur gedacht —
// nur als Nachweis. Aufruf:
//   dotnet run -- spike-hitl start [dir]
//   (Prozess endet am Gate) danach in EINEM NEUEN Prozess:
//   dotnet run -- spike-hitl resume <antwortZahl> [dir]
// dir default = runs/spike-hitl/current.

// Minimaler Ziel-Executor: nimmt die menschliche Antwort (int) und terminiert den Workflow mit einem Output-String.
// [YieldsOutput] MUSS deklariert werden (rc1: undeklariertes Yield/Send scheitert zur Laufzeit — vgl. [SendsMessage]).
[YieldsOutput(typeof(string))]
internal sealed class HitlSpikeFinalExecutor() : Executor<int>("HitlSpikeFinal")
{
    public override async ValueTask HandleAsync(int answer, IWorkflowContext context, CancellationToken ct = default)
        => await context.YieldOutputAsync($"Antwort vom Menschen erhalten: {answer}", ct).ConfigureAwait(false);
}

public static class HitlSpikeRunner
{
    private const string GatePortId = "hitl-spike-gate";
    private static readonly JsonSerializerOptions Wire = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    // Der Graph MUSS in beiden Prozessen identisch gebaut werden (Resume rehydriert in denselben Graphen).
    //   RequestPort<string,int>  --(int-Antwort)-->  Final(Executor<int>)  --YieldOutput(string)-->  Ende
    private static Workflow BuildWorkflow()
    {
        var gate = RequestPort.Create<string, int>(GatePortId);
        var final = new HitlSpikeFinalExecutor();
        return new WorkflowBuilder(gate)
            .AddEdge(gate, final)
            .WithOutputFrom(final)
            .Build();
    }

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        var sub = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        return sub switch
        {
            "start" => await StartAsync(ResolveDir(repoRoot, args, dirArgIndex: 2)).ConfigureAwait(false),
            "resume" => await ResumeAsync(args, repoRoot).ConfigureAwait(false),
            _ => Usage(),
        };
    }

    private static int Usage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  spike-hitl start [dir]            -> laeuft bis zum Human-Gate, checkpointet, endet");
        Console.WriteLine("  spike-hitl resume <antwort> [dir] -> NEUER Prozess: resumt vom Checkpoint, antwortet");
        return 2;
    }

    private static string ResolveDir(string repoRoot, string[] args, int dirArgIndex)
        => args.Length > dirArgIndex && !string.IsNullOrWhiteSpace(args[dirArgIndex])
            ? Path.GetFullPath(args[dirArgIndex])
            : Path.Combine(repoRoot, "runs", "spike-hitl", "current");

    // --- Prozess A: bis zum Gate laufen, Checkpoint auf Platte, Zeiger persistieren, beenden. ---
    private static async Task<int> StartAsync(string dir)
    {
        var checkpointDir = new DirectoryInfo(Path.Combine(dir, "checkpoints"));
        checkpointDir.Create();
        using var store = new FileSystemJsonCheckpointStore(checkpointDir);
        var manager = CheckpointManager.CreateJson(store, Wire);
        var workflow = BuildWorkflow();

        var sessionId = $"hitl-spike-{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}";
        Console.WriteLine($"[start] sessionId={sessionId}  dir={dir}");

        await using var run = await InProcessExecution
            .RunStreamingAsync(workflow, "Bitte eine Zahl liefern (Human-Gate).", manager, sessionId)
            .ConfigureAwait(false);

        CheckpointInfo? pending = null;
        await foreach (var evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
            if (evt is RequestInfoEvent req)
            {
                req.Request.TryGetDataAs<string>(out var prompt);
                Console.WriteLine($"[start] Human-Gate erreicht. requestId={req.Request.RequestId} prompt=\"{prompt}\"");
            }
            // Checkpoints entstehen am Superstep-Ende; erst wenn der Superstep offene Requests meldet, pausieren wir.
            if (evt is SuperStepCompletedEvent step && step.CompletionInfo is { } info)
            {
                if (info.Checkpoint is { } cp) pending = cp;
                if (info.HasPendingRequests && pending is not null)
                {
                    break;
                }
            }
        }

        if (pending is null)
        {
            Console.WriteLine("[start] FEHLER: kein Checkpoint mit offenem Request erzeugt.");
            return 1;
        }

        var pointerPath = Path.Combine(dir, "pointer.json");
        await File.WriteAllTextAsync(pointerPath, JsonSerializer.Serialize(
            new PointerFile(pending.SessionId, pending.CheckpointId, DateTime.UtcNow), Wire)).ConfigureAwait(false);

        Console.WriteLine($"[start] pausiert. checkpointId={pending.CheckpointId} sessionId={pending.SessionId}");
        Console.WriteLine($"[start] Zeiger: {pointerPath}");
        Console.WriteLine("[start] Prozess endet jetzt. Fortsetzen mit:  spike-hitl resume <antwort> " +
            (dir.Contains("spike-hitl") ? "" : dir));
        return 0;
    }

    // --- Prozess B (NEU): Zeiger lesen, vom Checkpoint rehydrieren, Request beantworten, terminieren. ---
    private static async Task<int> ResumeAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3 || !int.TryParse(args[2], out var answer))
        {
            Console.WriteLine("[resume] FEHLER: Antwort-Zahl fehlt. Usage: spike-hitl resume <antwort> [dir]");
            return 2;
        }
        var dir = ResolveDir(repoRoot, args, dirArgIndex: 3);
        var pointerPath = Path.Combine(dir, "pointer.json");
        if (!File.Exists(pointerPath))
        {
            Console.WriteLine($"[resume] FEHLER: kein pointer.json unter {dir}. Erst 'spike-hitl start' laufen lassen.");
            return 1;
        }

        var pointer = JsonSerializer.Deserialize<PointerFile>(await File.ReadAllTextAsync(pointerPath).ConfigureAwait(false), Wire)!;
        var checkpointDir = new DirectoryInfo(Path.Combine(dir, "checkpoints"));
        using var store = new FileSystemJsonCheckpointStore(checkpointDir);
        var manager = CheckpointManager.CreateJson(store, Wire);
        var workflow = BuildWorkflow();

        // Cross-Process: CheckpointInfo aus dem persistierten Zeiger rekonstruieren (Ctor = sessionId, checkpointId).
        var checkpoint = new CheckpointInfo(pointer.SessionId, pointer.CheckpointId);
        Console.WriteLine($"[resume] NEUER Prozess. rehydriere sessionId={pointer.SessionId} checkpointId={pointer.CheckpointId}");

        // Muster: fresh streaming run oeffnen (an dieselbe sessionId gebunden) -> Checkpoint restoren -> Stream beobachten.
        await using var run = await InProcessExecution
            .OpenStreamingAsync(workflow, manager, pointer.SessionId)
            .ConfigureAwait(false);
        Console.WriteLine("[resume] run geoeffnet, restore Checkpoint ...");
        await run.RestoreCheckpointAsync(checkpoint).ConfigureAwait(false);
        Console.WriteLine("[resume] Checkpoint restored, beobachte Stream (Timeout 30s) ...");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var answered = false;
        await foreach (var evt in run.WatchStreamAsync(cts.Token).ConfigureAwait(false))
        {
            Console.WriteLine($"[resume]   event: {evt.GetType().Name}");
            if (evt is RequestInfoEvent req)
            {
                Console.WriteLine($"[resume] offener Request re-emittiert: requestId={req.Request.RequestId} -> antworte {answer}");
                await run.SendResponseAsync(req.Request.CreateResponse(answer)).ConfigureAwait(false);
                answered = true;
            }
            else if (evt is WorkflowOutputEvent outEvt)
            {
                Console.WriteLine($"[resume] Workflow-Output: {outEvt.Data}");
                Console.WriteLine("[resume] ERFOLG: Cross-Process Human-Gate durchlaufen (pausiert -> neuer Prozess -> resume -> antwort -> terminiert).");
                return 0;
            }
        }

        Console.WriteLine(answered
            ? "[resume] FEHLER: geantwortet, aber kein WorkflowOutputEvent."
            : "[resume] FEHLER: kein Request re-emittiert (Resume hat den offenen Gate nicht wiederhergestellt).");
        return 1;
    }

    private sealed record PointerFile(string SessionId, string CheckpointId, DateTime SavedUtc);
}
