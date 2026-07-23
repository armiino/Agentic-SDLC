using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Front-Netz (Ledger-Deterministik): die deterministischen Raender der LLM-Knoten — Parsing und
// Normalisierung von Recall-Verdicts und Facet-Validations. Genau hier entscheidet sich bei einem
// E2E-Fehler "Mechanik kaputt" vs "Modell antwortet daneben".
public sealed class LedgerVerdictParsingTests
{
    [Fact]
    public void RecallVerdict_Parse_zieht_JSON_aus_Rauschen_und_Normalized_glaettet_Werte()
    {
        var text = """
            Hier ist meine Bewertung:
            { "verdict": " Exact ", "matchedIds": [" L-1 ", "", "L-2"],
              "propositionMatch": "exact", "statusMatch": "PARTIAL", "modalityMatch": "vielleicht",
              "scopeMatch": "exact", "timeScopeMatch": "exact", "evidenceMatch": "exact",
              "dispositionMatch": "exact", "reason": "  passt  " }
            Ende.
            """;

        var verdict = SemanticLedgerRecallMatcher.Parse(text)?.Normalized();

        Assert.NotNull(verdict);
        Assert.Equal("exact", verdict!.Verdict);
        Assert.Equal(["L-1", "L-2"], verdict.MatchedIds);          // getrimmt, Leere raus
        Assert.Equal("partial", verdict.StatusMatch);              // case-insensitiv
        Assert.Equal("missed", verdict.ModalityMatch);             // Erfindung -> missed
        Assert.Equal("passt", verdict.Reason);
    }

    [Fact]
    public void RecallVerdict_Parse_liefert_null_bei_Muell()
    {
        Assert.Null(SemanticLedgerRecallMatcher.Parse(null));
        Assert.Null(SemanticLedgerRecallMatcher.Parse("kein json weit und breit"));
        Assert.Null(SemanticLedgerRecallMatcher.Parse("{ kaputtes: json"));
        Assert.Null(SemanticLedgerRecallMatcher.Parse("{ nicht: \"valide\" }"));
    }

    [Fact]
    public void NormalizeVerdict_akzeptiert_nur_die_vier_offiziellen_Verdicts()
    {
        Assert.Equal("grounded", EntryValidation.NormalizeVerdict(" Grounded "));
        Assert.Equal("overstated", EntryValidation.NormalizeVerdict("overstated"));
        var ex = Assert.Throws<InvalidOperationException>(() => EntryValidation.NormalizeVerdict("plausible"));
        Assert.Contains("LEDGER_FACET_INVALID_VERDICT", ex.Message);
    }

    [Fact]
    public void FacetIssue_Normalized_verwirft_taxonomiefremde_Repairs_und_behaelt_offizielle()
    {
        Assert.Null(new FacetIssue("Status", "decided", "p", "planned").Normalized().Suggested);
        Assert.Equal("open", new FacetIssue("status", "decided", "p", " OPEN ").Normalized().Suggested);
        // scope ist Freitext (keine geschlossene Facette) -> Vorschlag bleibt, nur normalisiert:
        Assert.Equal("payment-team", new FacetIssue("scope", "global", "p", "Payment-Team").Normalized().Suggested);
        Assert.Equal("status", new FacetIssue(" Status ", "x", "p", null).Normalized().Facet);
    }

    [Fact]
    public void DeriveStatus_nur_grounded_ist_approved_alles_andere_braucht_Review()
    {
        static string Derive(string verdict) => ValidatedLedgerEntry.DeriveStatus(new EntryValidation("id", verdict, [], "r"));
        Assert.Equal("approved", Derive("grounded"));
        Assert.Equal("review_required", Derive("partial"));
        Assert.Equal("review_required", Derive("overstated"));
        Assert.Equal("review_required", Derive("unsupported"));
    }
}
