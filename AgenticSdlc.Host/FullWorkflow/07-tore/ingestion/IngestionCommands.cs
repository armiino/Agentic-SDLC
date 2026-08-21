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
        // R-11 A1d: dieselbe HITL-Mechanik mit dem Architecture-Profil (geteilte Naht, eigener runs/arch-ingestion-Ordner).
        map["arch-classify-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyReviewRunner.RunAsync(args, settings, repoRoot);
        map["adr-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Adr.AdrReviewRunner.RunAsync(args, settings, repoRoot);
        map["ingest-architecture-hitl"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionHitlRunner.RunAsync(
            args, settings, repoRoot, AgenticSdlc.Host.FullWorkflow.Core.AspectIngestionProfile.Architecture, "ingest-architecture-hitl", "arch-ingestion");

        map["ingest-review"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionReviewRunner.RunAsync(args, settings, repoRoot);

        // Deterministischer Apply: akzeptierte Operationen in den Core (Upsert-by-Identity) + Delta + affected-view.
        map["ingest-apply"] = (args, settings, repoRoot) => AgenticSdlc.Host.FullWorkflow.Core.IngestionApplyRunner.RunAsync(args, repoRoot);

        // 1g-A (19.08.): Anforderungsdokument = Core-Projektion (docs/anforderungen.md) — CLI-Haut über der
        // geteilten RunAsync-Naht (K13; Steward-Tool nutzt DIESELBE).
        map["requirements-doc"] = async (args, _, repoRoot) =>
        {
            var (path, version, items) = await RequirementsDocumentProjection.RunAsync(repoRoot).ConfigureAwait(false);
            Console.WriteLine($"[requirements-doc] Version {version} geschrieben ({items} Core-Items): {Path.GetRelativePath(repoRoot, path)}");
            return 0;
        };
    }
}
