namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>CLI-Kommandos des Analysten — registriert im Host-Dispatch (Muster IngestionCommands).</summary>
public static class AnalystCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // 1g-B: die Luecken-Analyse als bewusster Akt (K13: dieselbe Naht wie das Steward-Seil run_core_analysis).
        map["core-analysis"] = (args, settings, repoRoot) => AnalystRunner.RunAsync(args, settings, repoRoot);
    }
}
