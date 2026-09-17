namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

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
        // W1e': die GANZE Kette als EIN durabler MAF-Graph. Schritt 2 = Run-Faden + Skeleton (ohne LLM).
        map["pipeline-full"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Pipeline.PipelineFullRunner.RunAsync(args, settings, repoRoot);
    }
}
