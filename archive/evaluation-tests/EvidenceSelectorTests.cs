using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

/// <summary>
/// Claim-Pilot Stufe 2: deterministische Parse-/Validierungslogik des EvidenceSelectors (Turn-Indizes
/// parsen, Code-Fence-robust, Bad-JSON → null) + die Index-Validierung im ToEvidence-Pfad ist über die
/// SelectAsync-Range-Filterung abgedeckt (Halluzinationen verworfen). Der eigentliche Auswahl-Call ist ein Run.
/// </summary>
public sealed class EvidenceSelectorTests
{
    [Fact]
    public void ParseSelection_Reads_Turns_And_Reason()
    {
        var sel = EvidenceSelector.ParseSelection("{\"turns\":[3,7,7,1],\"reason\":\"EU-only offen\"}");
        Assert.NotNull(sel);
        Assert.Equal(new[] { 3, 7, 7, 1 }, sel!.Turns);   // Dedup/Sortierung passiert erst in SelectAsync
        Assert.Equal("EU-only offen", sel.Reason);
    }

    [Fact]
    public void ParseSelection_Empty_List_Is_Valid()
    {
        var sel = EvidenceSelector.ParseSelection("{\"turns\":[],\"reason\":\"nichts relevant\"}");
        Assert.NotNull(sel);
        Assert.Empty(sel!.Turns);
    }

    [Fact]
    public void ParseSelection_Bad_Json_Returns_Null()
    {
        Assert.Null(EvidenceSelector.ParseSelection("kein json"));
        Assert.Null(EvidenceSelector.ParseSelection("{\"reason\":\"x\"}"));   // kein turns-Key
    }

    [Fact]
    public void ParseSelection_Extracts_From_Code_Fence()
    {
        var sel = EvidenceSelector.ParseSelection("```json\n{\"turns\":[2],\"reason\":\"r\"}\n```");
        Assert.NotNull(sel);
        Assert.Equal(new[] { 2 }, sel!.Turns);
    }

    [Fact]
    public void ToEvidence_Maps_Selected_Turns_To_Spans()
    {
        var turns = new[]
        {
            new TranscriptTurn(0, "Anna", "EU only."),
            new TranscriptTurn(1, "Ben", "Nicht dasselbe."),
            new TranscriptTurn(2, "Clara", "DSGVO.")
        };
        var spans = EvidenceSelector.ToEvidence(new EvidenceSelection(new[] { 0, 2 }, "r"), turns);
        Assert.Equal(2, spans.Count);
        Assert.Contains("[Anna] (T0): EU only.", spans);
        Assert.Contains("[Clara] (T2): DSGVO.", spans);
    }
}
