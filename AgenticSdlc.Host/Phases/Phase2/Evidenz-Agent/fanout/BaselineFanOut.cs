using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut;

/// <summary>Ein Eintrag im Verified Baseline Set: das Ergebnis EINES Artefakt-Zweigs (Index; die vollen Items
/// liegen je Zweig in <c>{type}.artifact.json</c>).</summary>
public sealed record BaselineEntry(
    [property: JsonPropertyName("artifactId")] string ArtifactId,
    [property: JsonPropertyName("artifactType")] string ArtifactType,
    [property: JsonPropertyName("items")] int Items,
    [property: JsonPropertyName("file")] string File);

/// <summary>Das Fan-in-Ergebnis: die eingesammelten, geprüften Baselines aller Zweige. Input der späteren
/// Derivation-Schicht (I-b).</summary>
public sealed record VerifiedBaselineSet(
    [property: JsonPropertyName("artifacts")] IReadOnlyList<BaselineEntry> Artifacts);

/// <summary>
/// Start-/Fan-out-Stufe: nimmt die Ledger-Projektion (Workflow-Input <see cref="string"/>) und reicht sie über die
/// <c>AddFanOutEdge</c> an ALLE Artefakt-Zweige weiter (die laufen dann parallel, jeder liest UNABHÄNGIG dieselbe
/// geschlossene Quelle — „Variante C richtig").
/// </summary>
[SendsMessage(typeof(string))]
internal sealed class BaselineFanOutDispatchExecutor : Executor<string>
{
    public const string ExecutorName = "BaselineFanOutDispatch";

    public BaselineFanOutDispatchExecutor() : base(ExecutorName) { }

    public override async ValueTask HandleAsync(
        string source, IWorkflowContext context, CancellationToken cancellationToken = default)
        => await context.SendMessageAsync(source).ConfigureAwait(false);
}

/// <summary>
/// Fan-in-Ziel hinter der <c>AddFanInBarrierEdge</c>: die Barrier hält die Zweig-Outputs, bis JEDER Zweig ein
/// <see cref="ArtifactDocument"/> geliefert hat, und streamt sie dann hierher. Dieser Executor akkumuliert sie bis
/// <see cref="_expected"/> und yieldet das <see cref="VerifiedBaselineSet"/> (+ schreibt <c>baseline-set.json</c>).
/// </summary>
/// <remarks>
/// Bewusst zustandsbehaftet (Akkumulation über die je-Zweig-Zustellungen) — zulässig, weil pro Lauf eine frische
/// Instanz existiert und die Zustellung an einen einzelnen Executor sequenziell erfolgt (MAF-Superstep). Für
/// PARALLELE Läufe desselben Workflows wäre das nicht stateless — hier nicht der Fall (ein Lauf = eine Instanz).
/// </remarks>
[YieldsOutput(typeof(VerifiedBaselineSet))]
internal sealed class BaselineCollectorExecutor : Executor<ArtifactDocument>
{
    public const string ExecutorName = "BaselineCollector";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly RunContext _run;
    private readonly int _expected;
    private readonly List<BaselineEntry> _entries = [];

    public BaselineCollectorExecutor(RunContext run, int expected) : base(ExecutorName)
    {
        _run = run;
        _expected = expected;
    }

    public override async ValueTask HandleAsync(
        ArtifactDocument doc, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        _entries.Add(new BaselineEntry(doc.ArtifactId, doc.ArtifactType, doc.Items.Count, $"{doc.ArtifactType}.artifact.json"));
        _run.AppendEvent(new
        {
            type = "BASELINE_COLLECTED",
            runId = _run.RunId,
            artifact = doc.ArtifactType,
            artifactId = doc.ArtifactId,
            items = doc.Items.Count,
            collected = _entries.Count,
            expected = _expected,
            timestampUtc = DateTime.UtcNow
        });

        if (_entries.Count < _expected) return;

        var set = new VerifiedBaselineSet(_entries.OrderBy(e => e.ArtifactType, StringComparer.Ordinal).ToList());
        await File.WriteAllTextAsync(
            Path.Combine(_run.RunDir, "baseline-set.json"), JsonSerializer.Serialize(set, Json), cancellationToken).ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "BASELINE_SET_READY",
            runId = _run.RunId,
            count = set.Artifacts.Count,
            artifacts = set.Artifacts.Select(a => a.ArtifactType).ToArray(),
            timestampUtc = DateTime.UtcNow
        });

        await context.YieldOutputAsync(set).ConfigureAwait(false);
    }
}
