using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.FanOut;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Load;

/// <summary>Schritt 5 ⑤ (05.08.) — die typisierte BESTELLUNG des Load-Modus als Workflow-INPUT
/// (R-37-Muster „der Input IST die Daten"; ersetzt den informationsfreien "LOAD"-String-Sentinel +
/// die Konstruktions-Zeit-Bindung von Typen/Quelle).</summary>
public sealed record LoadBaselineRequest(IReadOnlyList<string> ArtifactTypes, string SourceRunDir);

/// <summary>
/// Bau-Punkt 3 (<c>mode:load</c>): Baseline-Quelle OHNE frischen Fan-out. Liest je bestelltem Quelltyp ein
/// bereits vorhandenes <c>artifact.json</c> (aus einem früheren Run) von der Platte, kopiert es in den aktuellen
/// Run (damit der restliche Graph — <see cref="Chain.SelectBaselineExecutor"/> → Derivation — byte-identisch zum
/// Build-Modus läuft) und sendet das <see cref="VerifiedBaselineSet"/>. Ermöglicht „Artefakte sind schon da →
/// nur ableiten" (später entscheiden / Sprint). Deterministisch, kein LLM → korrekt KEIN Agent.
/// </summary>
/// <remarks>
/// Kontrakt-gleich mit dem Fan-out-Collector (<see cref="BaselineCollectorExecutor"/>): schreibt dieselben
/// <c>{type}.artifact.json</c> + <c>baseline-set.json</c> in den Run und liefert denselben Ausgabetyp. Dadurch ist die
/// Baseline-Quelle im Chain-Graphen austauschbar (build ↔ load), ohne den nachgelagerten Graphen zu ändern.
/// </remarks>
[SendsMessage(typeof(VerifiedBaselineSet))]
internal sealed class LoadBaselineExecutor : Executor<LoadBaselineRequest>
{
    public const string ExecutorName = "LoadBaseline";
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    private readonly RunContext _run;

    public LoadBaselineExecutor(RunContext run) : base(ExecutorName) => _run = run;

    public override async ValueTask HandleAsync(LoadBaselineRequest request, IWorkflowContext context, CancellationToken ct = default)
    {
        var entries = new List<BaselineEntry>(request.ArtifactTypes.Count);
        foreach (var type in request.ArtifactTypes)
        {
            // Neues Layout: baselines/{type}/artifact.json. Fallback auf altes flaches {type}.artifact.json,
            // damit vor dem Struktur-Umbau eingefrorene Runs (z. B. d3682c) weiter ladbar bleiben.
            var srcPath = Path.Combine(request.SourceRunDir, "baselines", type, "artifact.json");
            if (!File.Exists(srcPath)) srcPath = Path.Combine(request.SourceRunDir, $"{type}.artifact.json");
            if (!File.Exists(srcPath))
                throw new InvalidOperationException($"[load] artifact.json für '{type}' nicht im Quell-Run '{request.SourceRunDir}' (weder baselines/{type}/ noch flach).");

            var text = await File.ReadAllTextAsync(srcPath, ct).ConfigureAwait(false);
            var doc = JsonSerializer.Deserialize<ArtifactDocument>(text, Json)
                      ?? throw new InvalidOperationException($"[load] artifact.json für '{type}' nicht lesbar.");
            if (doc.Items.Count == 0)
                throw new InvalidOperationException($"[load] artifact.json für '{type}' hat 0 Items.");

            // Kopie in den aktuellen Run ins neue Layout (Disk = Wahrheit; SelectBaseline liest baselines/{type}/).
            var destFile = Path.Combine(_run.OutputDir($"baselines/{type}"), "artifact.json");
            await File.WriteAllTextAsync(destFile, text, ct).ConfigureAwait(false);
            entries.Add(new BaselineEntry(doc.ArtifactId, doc.ArtifactType, doc.Items.Count, $"baselines/{type}/artifact.json"));
            _run.AppendEvent(new
            {
                type = "BASELINE_LOADED", runId = _run.RunId, artifact = type, items = doc.Items.Count,
                from = request.SourceRunDir, timestampUtc = DateTime.UtcNow
            });
        }

        var set = new VerifiedBaselineSet(entries.OrderBy(e => e.ArtifactType, StringComparer.Ordinal).ToList());
        await File.WriteAllTextAsync(
            Path.Combine(_run.RunDir, "baseline-set.json"), JsonSerializer.Serialize(set, Json), ct).ConfigureAwait(false);
        _run.AppendEvent(new
        {
            type = "BASELINE_SET_READY", runId = _run.RunId, mode = "load", count = set.Artifacts.Count,
            artifacts = set.Artifacts.Select(a => a.ArtifactType).ToArray(), timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(set).ConfigureAwait(false);
    }
}
