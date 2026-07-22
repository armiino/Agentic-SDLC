namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// CLI-Kommandos dieses Kettenglieds — registriert im Host-Dispatch (R1, 2026-07-22).
/// Frueher: if-Kette in Program.cs; Verhalten unveraendert.
/// </summary>
public static class IngestionCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
    {
        // Requirement-Ingestion: neues MeetingDelta gegen den Core aufloesen (Resolver-Agent -> Gate -> StateChangePlan).
        map["ingest-requirements"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionRequirementsRunner.RunAsync(args, settings, repoRoot);

        // S4 (Worklist 20.07): ingestion (Tor 1) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
        // Additiv/parallel zum klassischen ingest-requirements / ingest-review / ingest-apply (die bleiben unveraendert).
        map["ingest-requirements-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionHitlRunner.RunAsync(args, settings, repoRoot);

        map["ingest-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionReviewRunner.RunAsync(args, settings, repoRoot);

        // Deterministischer Apply: akzeptierte Operationen in den Core (Upsert-by-Identity) + Delta + affected-view.
        map["ingest-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionApplyRunner.RunAsync(args, repoRoot);
    }
}
