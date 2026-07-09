using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Branch;

/// <summary>
/// Terminale Stufe eines Artefakt-Zweigs (E-c): nimmt das <see cref="CheckerRepairResult"/> des gebundenen
/// CheckerRepair-Subworkflows, vergibt über das deterministische <see cref="ArtifactIdGate"/> stabile Item-IDs und
/// yieldet das strukturierte <see cref="ArtifactDocument"/> (schreibt es zusätzlich als <c>artifact.json</c> = E-e).
/// </summary>
/// <remarks>
/// Der Eingangstyp <see cref="CheckerRepairResult"/> ist bewusst der AUSGABETYP des gebundenen Subworkflows (dessen
/// Finalize-YieldOutput), sodass die Kante <c>[CheckerRepair] → AssignIds</c> MAF-nativ passt. KEIN LLM (rein
/// deterministisch). Der Output <see cref="ArtifactDocument"/> ist der Zweig-Ausgabetyp — im späteren Fan-in (E-d)
/// die eingesammelte, verifizierte Baseline. Verwendet die reparierte <c>Markdown</c> aus dem CheckerRepair, nicht
/// den Maker-Rohtext.
/// </remarks>
[YieldsOutput(typeof(ArtifactDocument))]
internal sealed class ArtifactAssignIdsExecutor : Executor<CheckerRepairResult>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly RunContext _run;
    private readonly string _artifactType;
    private readonly string _model;
    private readonly string _outDir;

    public ArtifactAssignIdsExecutor(RunContext run, string artifactType, string model, string? outputScope = null)
        : base($"AssignIds-{artifactType}")
    {
        _run = run;
        _artifactType = artifactType;
        _model = model;
        _outDir = run.OutputDir(outputScope);
    }

    public override async ValueTask HandleAsync(
        CheckerRepairResult result, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var producer = new ProducerMetadata(_run.RunId, _model, PromptVersion: null);
        var doc = ArtifactIdGate.Assign(result.Markdown, _artifactType, producer);

        var outFile = Path.Combine(_outDir, "artifact.json");
        await File.WriteAllTextAsync(outFile, JsonSerializer.Serialize(doc, Json), cancellationToken).ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "BRANCH_IDS_ASSIGNED",
            runId = _run.RunId,
            artifact = _artifactType,
            artifactId = doc.ArtifactId,
            items = doc.Items.Count,
            checkerDecision = result.Decision.ToString(),
            timestampUtc = DateTime.UtcNow
        });

        await context.YieldOutputAsync(doc).ConfigureAwait(false);
    }
}
