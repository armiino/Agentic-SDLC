using AgenticSdlc.Host.FullWorkflow.MakerChecker.Workflow;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Branch;

/// <summary>
/// Start-Stufe eines Artefakt-Zweigs (E-c): der Evidence-Baseline-Agent erzeugt das Artefakt aus der ZWEIG-EIGENEN
/// Ledger-Projektion (Workflow-Input = <see cref="BranchSource"/>, R-37 MAF-nativ) und schickt es als
/// <see cref="CheckArtifactMessage"/> (Iteration 1) an die nachgelagerte, gebundene CheckerRepair-Stufe.
/// </summary>
/// <remarks>
/// Bewusst DÜNN — kapselt nur den fertig konfigurierten <see cref="AIAgent"/> (Prompt/Modell/Observability-Pipeline
/// baut der Runner). Der Ausgabetyp <see cref="CheckArtifactMessage"/> ist bewusst der EINGANGSTYP des
/// CheckerRepair-Subworkflows (Start = <c>CheckerExecutor</c>): so passt die Kante <c>Maker → [CheckerRepair]</c>
/// MAF-nativ (der gebundene Subworkflow akzeptiert genau den Input-Typ seines Start-Executors). Der Node-Name trägt
/// den Artefakttyp, damit im späteren Fan-out (E-d) mehrere Zweige distinkte IDs haben.
/// </remarks>
/// <summary>R-37 (MAF-native Form): die zweig-eigene, dispositions-gekeyte Ledger-Projektion als TYPISIERTE
/// Nachricht. Der Fan-out-Dispatch praegt je Spur eine <see cref="BranchSource"/>; Kanten-Praedikate routen sie
/// an genau ihren Zweig — die Quelle IST der Nachrichtenfluss (kein Broadcast-Sentinel, kein Override).</summary>
public sealed record BranchSource(string ArtifactType, string SourceBlock);

[SendsMessage(typeof(CheckArtifactMessage))]
internal sealed class ArtifactBranchMakerExecutor : Executor<BranchSource>
{
    private readonly AIAgent _agent;
    private readonly RunContext _run;
    private readonly string _artifactType;
    private readonly string _outDir;

    public ArtifactBranchMakerExecutor(AIAgent agent, RunContext run, string artifactType, string? outputScope = null)
        : base($"BranchMaker-{artifactType}")
    {
        _agent = agent;
        _run = run;
        _artifactType = artifactType;
        _outDir = run.OutputDir(outputScope);
    }

    public override async ValueTask HandleAsync(
        BranchSource source, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // R-18-Stil: eine fremde Quelle ist ein Verdrahtungsfehler — LAUT scheitern statt still falsch arbeiten.
        if (!string.Equals(source.ArtifactType, _artifactType, StringComparison.Ordinal))
            throw new InvalidOperationException($"BRANCH_SOURCE_MISMATCH: Zweig '{_artifactType}' erhielt Quelle fuer '{source.ArtifactType}'.");

        var response = await _agent
            .RunAsync([new ChatMessage(ChatRole.User, source.SourceBlock)], cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var markdown = response.Text ?? string.Empty;

        var outFile = Path.Combine(_outDir, "maker.md");
        await File.WriteAllTextAsync(outFile, markdown, cancellationToken).ConfigureAwait(false);

        _run.AppendEvent(new
        {
            type = "BRANCH_MAKER_PRODUCED",
            runId = _run.RunId,
            artifact = _artifactType,
            chars = markdown.Length,
            timestampUtc = DateTime.UtcNow
        });

        await context.SendMessageAsync(new CheckArtifactMessage(markdown, Iteration: 1)).ConfigureAwait(false);
    }
}
