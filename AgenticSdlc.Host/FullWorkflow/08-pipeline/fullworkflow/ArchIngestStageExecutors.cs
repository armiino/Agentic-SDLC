using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// R-11 A1d (05.08.) — Einstieg des ARCH-Strips, SERIELL nach dem req-Apply (Sequenz-Entscheid §A1c+A1d):
/// lädt Core FRISCH (der req-Apply hat ihn soeben mutiert) und das Meeting-Delta über den Datei-Vertrag
/// <c>04-delta/project-state.json</c>. <b>Leer-Skip per Typ-Routing</b> (DecisionScan-Muster): 0 arch-Items
/// ⇒ der req-Report reist DIREKT weiter zum DecisionScan — kein Gate-Stopp, kein LLM. Sonst startet der
/// geteilte Ingestion-Kern mit dem <see cref="AspectIngestionProfile.Architecture"/>-Profil.
/// </summary>
[SendsMessage(typeof(IngestionApplyReport))]
[SendsMessage(typeof(IngestionResolveInput))]
internal sealed class ArchIngestBridgeExecutor(RunContext run, string repoRoot, int maxAttempts) : Executor<IngestionApplyReport>("PipelineArchBridge")
{
    public override async ValueTask HandleAsync(IngestionApplyReport reqReport, IWorkflowContext context, CancellationToken ct = default)
    {
        var deltaPath = Path.Combine(run.OutputDir("04-delta"), "project-state.json");
        var delta = (await JsonProjectStateRepository.LoadAsync(deltaPath).ConfigureAwait(false)).Document;
        var archCount = delta.Items.Count(AspectIngestionProfile.Architecture.Matches);

        if (archCount == 0)
        {
            run.AppendEvent(new { type = "ARCH_INGEST_SKIPPED", runId = run.RunId, reason = "no-incoming", timestampUtc = DateTime.UtcNow });
            await context.SendMessageAsync(reqReport).ConfigureAwait(false);   // Typ-Routing: direkt zum DecisionScan
            return;
        }

        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ARCH_INGEST_START", runId = run.RunId, incoming = archCount, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new IngestionResolveInput(delta, core, deltaPath, maxAttempts)).ConfigureAwait(false);
    }
}

/// <summary>
/// R-11 A1d — APPLY des ARCH-Strips (komponiert): wendet den arch-Plan mit dem Architecture-Profil an
/// (ARCH-IDs, itemType architecture; CONTRADICT mintet DECs in den EINEN Topf) und reicht danach den
/// REQ-Report als Passagier weiter (Datei-Vertrag <c>07-ingest/applied/delta.json</c> — kein Message-Schleppen
/// durch den Port-Roundtrip nötig): der DecisionScan sieht so req- UND arch-DECs im SELBEN Lauf.
/// </summary>
[SendsMessage(typeof(IngestionApplyReport))]
internal sealed class ArchComposedApplyExecutor(RunContext run, string repoRoot, string archOutDir, string ingestOutDir) : Executor<IngestionReviewResponse>("PipelineArchApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public override async ValueTask HandleAsync(IngestionReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        var plan = JsonSerializer.Deserialize<StateChangePlanDocument>(
                       await File.ReadAllTextAsync(Path.Combine(archOutDir, "plan.json"), ct).ConfigureAwait(false), Json)
                   ?? throw new InvalidOperationException($"arch-Plan nicht lesbar: {archOutDir}/plan.json");
        var accepted = resp.AcceptedIncomingIds.ToHashSet(StringComparer.Ordinal);
        var report = await IngestionApplyExec.ExecuteAsync(archOutDir, plan, accepted, repoRoot, run.RunId, ct,
            profile: AspectIngestionProfile.Architecture).ConfigureAwait(false);
        run.AppendEvent(new { type = "PIPELINE_ARCH_APPLIED", runId = run.RunId, applied = report.Applied.Count, timestampUtc = DateTime.UtcNow });

        // Passagier: der REQ-Report (Datei-Vertrag des req-Apply) trägt den Faden weiter — ③ E-8: die
        // arch-Applied-Ops reisen MIT (vereinigte Applied/Skipped-Listen), damit die pbi-update-Ableitung
        // Rahmen-Änderungen sieht (constrained_by-PBIs wecken). Die Delta-Summary bleibt die des req-Laufs
        // (Zähl-Semantik je Aspekt; die arch-Zahlen stehen im PIPELINE_ARCH_APPLIED-Event + arch-Report).
        var reqReportPath = Path.Combine(ingestOutDir, "applied", "delta.json");
        var reqReport = JsonSerializer.Deserialize<IngestionApplyReport>(
                            await File.ReadAllTextAsync(reqReportPath, ct).ConfigureAwait(false), Json)
                        ?? throw new InvalidOperationException($"req-Report nicht lesbar: {reqReportPath}");
        var merged = reqReport with
        {
            Applied = [.. reqReport.Applied, .. report.Applied],
            Skipped = [.. reqReport.Skipped, .. report.Skipped]
        };
        // R-40 (Endform, Abschluss-E2E 145812): der arch-Apply schreibt den EINEN Lauf-Report-Vertrag FORT
        // (vereinigt req+arch) — der decision-gate-Roundtrip liest nur diese Datei; ohne den Fortschrieb
        // verloren E-8/A4 die arch-Ops. delta.json bleibt das per-Aspekt-Beleg-Artefakt.
        await File.WriteAllTextAsync(Path.Combine(ingestOutDir, "applied", "run-report.json"),
            JsonSerializer.Serialize(merged, Json), ct).ConfigureAwait(false);
        await context.SendMessageAsync(merged).ConfigureAwait(false);
    }
}
