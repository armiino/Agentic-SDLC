using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1e' Bauplan-Schritt 1: friert die run-config.fullworkflow-DEFAULTS + die Gate-Policy-Parsing-Regeln ein.
// Governance-kritisch: Default darf NIE "write" oder "accept-all" sein (Gates sind heilig).
public sealed class PipelineFullConfigTests
{
    [Fact]
    public void Leere_Config_liefert_governance_sichere_Defaults()
    {
        var s = FullWorkflowSettings.FromConfig(new FullWorkflowConfig());

        Assert.False(s.Execute);                                  // KEIN externer Write
        Assert.Equal(GatePolicyKind.Interactive, s.PolicyProfile.Kind); // pausieren, nicht auto-apply
        Assert.False(s.L3Enabled);                                // v1: l3 aus
        Assert.Empty(s.Models);
        Assert.Empty(s.MaxAttempts);
        Assert.Empty(s.Gates);
    }

    [Fact]
    public void Null_Config_ist_gleichwertig_zu_leerer_Config()
    {
        var s = FullWorkflowSettings.FromConfig(null);
        Assert.False(s.Execute);
        Assert.Equal(GatePolicyKind.Interactive, s.PolicyProfile.Kind);
        Assert.False(s.L3Enabled);
    }

    [Theory]
    [InlineData(null, GatePolicyKind.Interactive)]
    [InlineData("", GatePolicyKind.Interactive)]
    [InlineData("interactive", GatePolicyKind.Interactive)]
    [InlineData("accept-all", GatePolicyKind.AcceptAll)]
    [InlineData("acceptall", GatePolicyKind.AcceptAll)]
    [InlineData("replay", GatePolicyKind.Replay)]
    [InlineData("replay:decisions/e2e.json", GatePolicyKind.Replay)]
    [InlineData("etwas-unbekanntes", GatePolicyKind.Interactive)] // Fallback = sicherste Wahl
    public void GatePolicy_Parse_erkennt_die_drei_Arten(string? raw, GatePolicyKind expected)
    {
        Assert.Equal(expected, GatePolicy.Parse(raw).Kind);
    }

    [Fact]
    public void Replay_traegt_den_Pfad_mit_leerer_Pfad_wird_null()
    {
        Assert.Equal("decisions/e2e.json", GatePolicy.Parse("replay:decisions/e2e.json").ReplayPath);
        Assert.Null(GatePolicy.Parse("replay").ReplayPath);
        Assert.Null(GatePolicy.Parse("replay:").ReplayPath);
    }

    [Fact]
    public void GateFor_bevorzugt_expliziten_Gate_Eintrag_vor_PolicyProfile()
    {
        var cfg = new FullWorkflowConfig
        {
            PolicyProfile = "replay:e2e.json",
            Gates = new() { ["github-forward-gate"] = new GateConfig { Policy = "interactive" } }
        };
        var s = FullWorkflowSettings.FromConfig(cfg);

        Assert.Equal(GatePolicyKind.Interactive, s.GateFor("github-forward-gate").Kind); // eigener Eintrag gewinnt
        Assert.Equal(GatePolicyKind.Replay, s.GateFor("adjudication-gate").Kind);        // sonst PolicyProfile
    }

    [Fact]
    public void Stages_ohne_l3_Schluessel_bedeutet_l3_aus()
    {
        var s = FullWorkflowSettings.FromConfig(new FullWorkflowConfig { Stages = new() { ["andere"] = "on" } });
        Assert.False(s.L3Enabled);
    }

    [Fact]
    public void L3_kann_explizit_eingeschaltet_werden()
    {
        var s = FullWorkflowSettings.FromConfig(new FullWorkflowConfig { Stages = new() { ["l3"] = "on" } });
        Assert.True(s.L3Enabled);
    }

    [Fact]
    public void Execute_true_und_Modelle_werden_uebernommen()
    {
        var cfg = new FullWorkflowConfig
        {
            Execute = true,
            Models = new() { ["01-ledger"] = "openai/gpt-5.4", ["02-baselines"] = "openai/gpt-4.1-mini" },
            MaxAttempts = new() { ["02-baselines"] = 3 }
        };
        var s = FullWorkflowSettings.FromConfig(cfg);

        Assert.True(s.Execute);
        Assert.Equal("openai/gpt-4.1-mini", s.Models["02-baselines"]);
        Assert.Equal(3, s.MaxAttempts["02-baselines"]);
    }
}
