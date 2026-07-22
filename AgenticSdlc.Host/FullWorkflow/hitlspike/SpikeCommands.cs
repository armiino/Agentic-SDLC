namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.HitlSpike;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class SpikeCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // S0 (Worklist 20.07): Verifikations-Spike fuer MAF-nativen Human-Gate (RequestPort) + durables, prozessuebergreifendes
        // Checkpoint/Resume (FileSystemJsonCheckpointStore). Wegwerf/isoliert, kein LLM, kein Core-Zugriff.
        map["spike-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.HitlSpike.HitlSpikeRunner.RunAsync(args, repoRoot);
    }
}
