using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// §5 Statusmodell-Refactor (S2): beweist die VERLUSTFREIHEIT des Alt↔Neu-Mappings (CoreStatus).
// Round-Trip alt→neu→alt bit-gleich + korrekte Achsen-Zuordnung + LAUTES Scheitern bei unbekanntem Wert.
public sealed class CoreStatusMappingTests
{
    // Die 5 heute im 194er-Core vorkommenden + die 2 operativen Werte (open_decision/blocked_by_decision) + done/resolved.
    [Theory]
    [InlineData("baseline")]
    [InlineData("accepted")]
    [InlineData("active")]
    [InlineData("needs_clarify")]
    [InlineData("blocked_by_decision")]
    [InlineData("done")]
    [InlineData("superseded")]
    [InlineData("open_decision")]
    [InlineData("resolved")]
    public void RoundTrip_alt_neu_alt_ist_bit_gleich(string legacy)
        => Assert.Equal(legacy, CoreStatus.From(legacy).ToLegacyString());

    [Fact]
    public void Baseline_und_accepted_sind_beide_aktiv_nur_andere_Governance()
    {
        var baseline = CoreStatus.From("baseline");
        var accepted = CoreStatus.From("accepted");
        Assert.Equal(Validity.Active, baseline.Validity);
        Assert.Equal(Validity.Active, accepted.Validity);
        Assert.Equal(Confirmation.Agent, baseline.ConfirmedBy);   // <- der einzige Unterschied
        Assert.Equal(Confirmation.Human, accepted.ConfirmedBy);
    }

    [Fact]
    public void Active_traegt_keine_Governance_ehrlich_unmarkiert()
        => Assert.Equal(Confirmation.Unmarked, CoreStatus.From("active").ConfirmedBy);

    [Fact]
    public void Blocker_liegt_in_eigenem_Feld_nicht_in_der_Gueltigkeit()
    {
        var nc = CoreStatus.From("needs_clarify");
        Assert.Equal(Validity.Active, nc.Validity);              // weiterhin aktiv ...
        Assert.Equal(Blocker.NeedsClarify, nc.Blocker);         // ... nur blockiert
        Assert.Equal(Blocker.BlockedByDecision, CoreStatus.From("blocked_by_decision").Blocker);
    }

    [Fact]
    public void Done_ist_Fortschritt_nicht_Gueltigkeit_und_faellt_aus_der_aktiven_Sicht()
    {
        var done = CoreStatus.From("done");
        Assert.Equal(Validity.Active, done.Validity);           // done-PBI bleibt gueltige Wahrheit ...
        Assert.Equal(Progress.Done, done.Progress);            // ... nur die Arbeit ist fertig (done != superseded)
        Assert.True(done.IsArchived);
    }

    [Fact]
    public void Superseded_faellt_aus_der_aktiven_Sicht_active_nicht()
    {
        Assert.True(CoreStatus.From("superseded").IsArchived);
        Assert.False(CoreStatus.From("active").IsArchived);
    }

    [Fact]
    public void Open_decision_setzt_den_DecisionState()
    {
        Assert.True(CoreStatus.From("open_decision").IsOpenDecision);
        Assert.Equal(DecisionState.Resolved, CoreStatus.From("resolved").Decision);
    }

    [Fact]
    public void SourceRunId_wandert_in_den_Governance_Audit_Verweis()
        => Assert.Equal("run-123", CoreStatus.From("accepted", "run-123").ConfirmedInRun);

    [Fact]
    public void Unbekannter_Alt_Status_scheitert_LAUT_statt_still_falsch_zu_mappen()
        => Assert.Throws<ArgumentOutOfRangeException>(() => CoreStatus.From("voellig_unbekannt"));

    [Fact]
    public void Mapping_ist_gross_klein_und_whitespace_tolerant()
        => Assert.Equal(Validity.Superseded, CoreStatus.From("  SUPERSEDED  ").Validity);

    // S2-Serialisierung: neue Enum-Felder als STRING (nicht als Zahl) + null-Felder werden WEGGELASSEN (WhenWritingNull),
    // damit un-migrierte Items die JSON nicht zumüllen. Prüft die JsonConverter-/JsonIgnore-Entscheidungen.
    [Fact]
    public void Neue_Statusfelder_serialisieren_als_String_und_lassen_Nulls_weg()
    {
        var item = new ProjectStateItem(
            ItemId: "REQ-1", ItemType: "requirement", Text: "x", Status: "active", Origin: "Extracted",
            Stage: null, Version: 1, SourceRunId: null, SourceArtifactId: null, SourceArtifactType: null,
            SourceDecisionId: null, SourceCandidateId: null, SourceClaimIds: [], SourceArtifactItemIds: [],
            Metadata: new Dictionary<string, string>())
        {
            Validity = Validity.Active, Blocker = Blocker.None, ConfirmedBy = Confirmation.Agent,
            // Progress/ConfirmedInRun/Decision bewusst null -> müssen aus der JSON verschwinden
        };

        var json = JsonSerializer.Serialize(item, JsonFiles.Json);

        Assert.Contains("\"validity\": \"Active\"", json);        // String, nicht 0
        Assert.Contains("\"confirmedBy\": \"Agent\"", json);
        Assert.DoesNotContain("\"progress\"", json);              // null -> weggelassen
        Assert.DoesNotContain("\"decision\"", json);
        Assert.DoesNotContain("\"confirmedInRun\"", json);

        // Deserialisierung erhält die Felder (Round-Trip über JSON).
        var back = JsonSerializer.Deserialize<ProjectStateItem>(json, JsonFiles.Json)!;
        Assert.Equal(Validity.Active, back.Validity);
        Assert.Null(back.Progress);
    }

    // S4-Lese-Naht: ReadStatus nutzt die typisierten Felder wenn gesetzt, sonst leitet es transitorisch aus dem Alt-String ab.
    [Fact]
    public void ReadStatus_leitet_aus_dem_Alt_String_ab_wenn_keine_Felder_gesetzt()
        => Assert.Equal(Blocker.NeedsClarify, Item("needs_clarify").ReadStatus().Blocker);   // neue Felder null -> From(Status)

    [Fact]
    public void ReadStatus_bevorzugt_gesetzte_Felder_vor_dem_Alt_String()
    {
        var item = Item("active") with { Validity = Validity.Superseded };   // Feld gesetzt, Alt-String abweichend
        Assert.Equal(Validity.Superseded, item.ReadStatus().Validity);       // das Feld gewinnt
    }

    // S5: Escalate MUSS das alte PbiStatus.Max-Ranking (blocked>superseded>needs_clarify>active) bit-gleich reproduzieren.
    [Theory]
    [InlineData("active", "needs_clarify", "needs_clarify")]
    [InlineData("needs_clarify", "needs_clarify", "needs_clarify")]
    [InlineData("blocked_by_decision", "needs_clarify", "blocked_by_decision")]
    [InlineData("superseded", "needs_clarify", "superseded")]
    [InlineData("active", "blocked_by_decision", "blocked_by_decision")]
    [InlineData("needs_clarify", "blocked_by_decision", "blocked_by_decision")]
    [InlineData("superseded", "blocked_by_decision", "blocked_by_decision")]
    [InlineData("blocked_by_decision", "blocked_by_decision", "blocked_by_decision")]
    public void Escalate_reproduziert_das_alte_Max_Ranking(string baseStatus, string added, string expected)
    {
        var addedBlocker = added == "blocked_by_decision" ? Blocker.BlockedByDecision : Blocker.NeedsClarify;
        Assert.Equal(expected, CoreStatus.From(baseStatus).Escalate(addedBlocker).ToLegacyString());
    }

    [Fact]
    public void Escalate_bewahrt_Governance_und_Audit_bei_Eskalation()
    {
        var basis = CoreStatus.From("accepted", "run-9");   // Human, run-9
        var eskaliert = basis.Escalate(Blocker.NeedsClarify);
        Assert.Equal(Blocker.NeedsClarify, eskaliert.Blocker);
        Assert.Equal(Confirmation.Human, eskaliert.ConfirmedBy);   // Governance erhalten
        Assert.Equal("run-9", eskaliert.ConfirmedInRun);           // Audit erhalten
    }

    private static ProjectStateItem Item(string status) => new(
        ItemId: "X", ItemType: "pbi", Text: "x", Status: status, Origin: "test",
        Stage: null, Version: 1, SourceRunId: null, SourceArtifactId: null, SourceArtifactType: null,
        SourceDecisionId: null, SourceCandidateId: null, SourceClaimIds: [], SourceArtifactItemIds: [],
        Metadata: new Dictionary<string, string>());
}
