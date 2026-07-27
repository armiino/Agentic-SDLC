using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B0 (pipeline-full-bootstrap-plan §7): fullworkflow.mode-Parsing + die pure Phasen-Wahl-Regel
// Betrieb vs. Bootstrap. Der Runner füttert UseBootstrapBranch mit dem ExistsAsync-Ergebnis;
// die Regel selbst ist hier vollständig abgedeckt (Wahrheitstabelle).
public sealed class PipelineModeTests
{
    [Theory]
    [InlineData(null, PipelineMode.Auto)]
    [InlineData("", PipelineMode.Auto)]
    [InlineData("  auto  ", PipelineMode.Auto)]
    [InlineData("Bootstrap", PipelineMode.Bootstrap)]
    [InlineData("OPERATIONAL", PipelineMode.Operational)]
    [InlineData("quatsch", PipelineMode.Auto)] // unbekannt → Detektion (sicherste Wahl)
    public void ParseMode_liest_config_strings(string? raw, PipelineMode expected)
        => Assert.Equal(expected, FullWorkflowSettings.ParseMode(raw));

    [Theory]
    [InlineData(PipelineMode.Auto, true, false)]
    [InlineData(PipelineMode.Auto, false, true)]
    [InlineData(PipelineMode.Bootstrap, true, true)]
    [InlineData(PipelineMode.Bootstrap, false, true)]
    [InlineData(PipelineMode.Operational, true, false)]
    [InlineData(PipelineMode.Operational, false, false)]
    public void UseBootstrapBranch_Wahrheitstabelle(PipelineMode mode, bool coreExists, bool expected)
        => Assert.Equal(expected, FullWorkflowSettings.UseBootstrapBranch(mode, coreExists));

    [Fact]
    public void FromConfig_ohne_mode_ist_auto()
        => Assert.Equal(PipelineMode.Auto, FullWorkflowSettings.FromConfig(new FullWorkflowConfig()).Mode);

    [Fact]
    public void FromConfig_liest_mode()
        => Assert.Equal(PipelineMode.Bootstrap,
            FullWorkflowSettings.FromConfig(new FullWorkflowConfig { Mode = "bootstrap" }).Mode);

    [Fact]
    public void FromConfig_timeout_default_20_minuten()
        => Assert.Equal(20, FullWorkflowSettings.FromConfig(new FullWorkflowConfig()).TimeoutMinutes);

    [Fact]
    public void FromConfig_liest_timeoutMinutes()
        => Assert.Equal(90, FullWorkflowSettings.FromConfig(new FullWorkflowConfig { TimeoutMinutes = 90 }).TimeoutMinutes);

    [Theory]
    [InlineData("interactive-inline", GatePolicyKind.InteractiveInline)]
    [InlineData("inline", GatePolicyKind.InteractiveInline)]
    [InlineData("interactive", GatePolicyKind.Interactive)]
    public void GatePolicy_parse_kennt_inline(string raw, GatePolicyKind expected)
        => Assert.Equal(expected, GatePolicy.Parse(raw).Kind);
}
