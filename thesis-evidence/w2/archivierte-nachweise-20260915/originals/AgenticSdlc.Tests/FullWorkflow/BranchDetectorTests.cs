using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// U0 (ein-graph-vereinheitlichung §7): der Branch als Graph-Knoten — der pure Kern packt das Delta
// typisiert ein (Typ-Routing der Kanten übernimmt dann die Verzweigung). Die Entscheidungs-REGEL selbst
// (UseBootstrapBranch) ist in PipelineModeTests vollständig wahrheitstabellen-getestet.
public sealed class BranchDetectorTests
{
    private static readonly ProjectStateDocument Delta = new(
        "branch-test", ProjectStateDocument.CurrentSchemaVersion, DateTime.UnixEpoch,
        Sources: [], Items: [], Relations: [], Provenance: [], Proposals: []);

    [Fact]
    public void Leerer_core_im_auto_modus_wird_BootstrapDelta()
    {
        var wrapped = BranchDetector.Wrap(PipelineMode.Auto, coreExists: false, Delta);
        var boot = Assert.IsType<BootstrapDelta>(wrapped);
        Assert.Same(Delta, boot.Delta); // dasselbe Delta, nur typisiert eingepackt
    }

    [Fact]
    public void Existierender_core_im_auto_modus_wird_OperationalDelta()
    {
        var wrapped = BranchDetector.Wrap(PipelineMode.Auto, coreExists: true, Delta);
        Assert.Same(Delta, Assert.IsType<OperationalDelta>(wrapped).Delta);
    }

    [Fact]
    public void Erzwungene_modi_ignorieren_die_core_lage()
    {
        Assert.IsType<BootstrapDelta>(BranchDetector.Wrap(PipelineMode.Bootstrap, coreExists: true, Delta));
        Assert.IsType<OperationalDelta>(BranchDetector.Wrap(PipelineMode.Operational, coreExists: false, Delta));
    }
}
