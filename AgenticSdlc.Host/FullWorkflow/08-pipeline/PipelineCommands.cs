namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Pipeline;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class PipelineCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // HumanReview der Ingestion-Operationen (apply/skip je Operation).
        // S6 (Worklist 20.07) — A: EIN MAF-Lauf ueber zwei Stufen (ingest -> pbi-update) mit zwei Human-Gates,
        // prozessuebergreifend resumebar (start -> resume Gate1 -> resume Gate2). Komposition der S4-Stufen.
        map["pipeline-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Pipeline.PipelineComposedRunner.RunAsync(args, settings, repoRoot);
    }
}
