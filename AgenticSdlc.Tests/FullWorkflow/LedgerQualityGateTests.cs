using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Front-Netz (Ledger-Deterministik): das Quality-Gate ist der deterministische Integritaets-Beweis der
// Pipeline (kein LLM). Diese Tests frieren die Invarianten I0-I6 pro Violation-Code ein — genau die
// Stellen, an denen ein E2E-Fehler zwischen Mechanik und Modell unterschieden wird.
public sealed class LedgerQualityGateTests
{
    private static IReadOnlyDictionary<string, ArtifactDisposition> Disposition() =>
        new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new("required", "requirement"),
            ["architecture"] = new("context", "assumption"),
            ["risks"] = new("not_applicable", "consciously_omitted"),
            ["open-questions"] = new("optional", "open_question")
        };

    private static SemanticLedgerEntry Candidate(string id) => new(
        id, $"Aussage {id}", "requirement", "decided", "must", "global", "mvp",
        [new SemanticLedgerEvidence("transcript", $"Zitat {id}")], Disposition(), "low", null);

    private static SemanticLedgerEntry Canonical(string id, params string[] candidateIds) => Candidate(id) with
    {
        CandidateIds = candidateIds,
        AssumedRelation = candidateIds.Length == 1 ? "standalone" : "same_proposition"
    };

    private static ValidatedLedgerEntry Validated(SemanticLedgerEntry e, string verdict = "grounded")
    {
        var validation = new EntryValidation(e.Id, verdict, [], "ok");
        return new(e, validation, ValidatedLedgerEntry.DeriveStatus(validation));
    }

    private static IReadOnlyList<string> Codes(GateResult r) => r.Violations.Select(x => x.Code).ToList();

    [Fact]
    public void Gruener_Pfad_konsistenter_Ledger_passiert_ohne_Violations()
    {
        var candidates = new[] { Candidate("C-1"), Candidate("C-2") };
        var canonical = new[] { Canonical("K-1", "C-1", "C-2") };
        var result = LedgerQualityGate.Evaluate(candidates, canonical, [Validated(canonical[0])]);

        Assert.True(result.Pass);
        Assert.Empty(result.Violations);
        Assert.Equal(2, (int)result.Checks["candidatesReferenced"]);
        Assert.Equal(1, (int)result.Checks["approved"]);
    }

    [Fact]
    public void Still_verschwundener_Candidate_ist_ein_Error()
    {
        var candidates = new[] { Candidate("C-1"), Candidate("C-2") };
        var canonical = new[] { Canonical("K-1", "C-1") };
        var result = LedgerQualityGate.Evaluate(candidates, canonical, [Validated(canonical[0])]);

        Assert.False(result.Pass);
        var drop = Assert.Single(result.Violations, x => x.Code == "CANDIDATE_SILENTLY_DROPPED");
        Assert.Equal(["C-2"], drop.Ids);
    }

    [Fact]
    public void Kanonisch_ohne_ClusterTrace_Relation_und_Evidence_meldet_alle_drei_Pflichten()
    {
        var canonical = new[] { Candidate("K-1") with { Evidence = [] } }; // CandidateIds/AssumedRelation = null
        var result = LedgerQualityGate.Evaluate([], canonical, [Validated(canonical[0])]);

        Assert.False(result.Pass);
        Assert.Contains("CANONICAL_WITHOUT_CLUSTER_TRACE", Codes(result));
        Assert.Contains("MISSING_ASSUMED_RELATION", Codes(result));
        Assert.Contains("CANONICAL_WITHOUT_EVIDENCE", Codes(result));
    }

    [Fact]
    public void Relations_Kardinalitaet_standalone_genau_eine_Merge_mindestens_zwei()
    {
        var candidates = new[] { Candidate("C-1"), Candidate("C-2"), Candidate("C-3") };
        var badStandalone = Canonical("K-1", "C-1", "C-2") with { AssumedRelation = "standalone" };
        var badMerge = Canonical("K-2", "C-3") with { AssumedRelation = "refines" };
        var result = LedgerQualityGate.Evaluate(candidates, [badStandalone, badMerge],
            [Validated(badStandalone), Validated(badMerge)]);

        Assert.Contains("STANDALONE_RELATION_CARDINALITY", Codes(result));
        Assert.Contains("MERGE_RELATION_CARDINALITY", Codes(result));
    }

    [Fact]
    public void Taxonomie_wird_erzwungen_inklusive_Rasierklingen_Regel_fuer_required()
    {
        var candidates = new[] { Candidate("C-1"), Candidate("C-2") };
        var badEnums = Canonical("K-1", "C-1") with { Status = "planned", Modality = "should" };
        // status=required (externe Pflicht) vertraegt keine weiche Modalitaet:
        var badRequired = Canonical("K-2", "C-2") with { Status = "required", Modality = "desired" };
        var result = LedgerQualityGate.Evaluate(candidates, [badEnums, badRequired],
            [Validated(badEnums), Validated(badRequired)]);

        Assert.Contains("INVALID_STATUS", Codes(result));
        Assert.Contains("INVALID_MODALITY", Codes(result));
        var razor = Assert.Single(result.Violations, x => x.Code == "INCONSISTENT_REQUIRED_MODALITY");
        Assert.Equal(["K-2"], razor.Ids);
    }

    [Fact]
    public void Validated_muss_canonical_exakt_decken_beide_Richtungen()
    {
        var candidates = new[] { Candidate("C-1") };
        var canonical = new[] { Canonical("K-1", "C-1") };
        var fremd = Validated(Canonical("K-99", "C-1") with { CandidateIds = ["C-1"] });
        var result = LedgerQualityGate.Evaluate(candidates, canonical, [fremd]);

        Assert.Contains("CANONICAL_NOT_VALIDATED", Codes(result));
        Assert.Contains("VALIDATION_WITHOUT_CANONICAL", Codes(result));
    }

    [Fact]
    public void Folgenloser_Repair_ist_nur_Warning_und_laesst_das_Gate_passieren()
    {
        var candidates = new[] { Candidate("C-1") };
        var canonical = new[] { Canonical("K-1", "C-1") };
        var validation = new EntryValidation("K-1", "grounded",
            [new FacetIssue("status", "decided", "kein echtes Problem", "decided")], "ok");
        var result = LedgerQualityGate.Evaluate(candidates, canonical,
            [new ValidatedLedgerEntry(canonical[0], validation, ValidatedLedgerEntry.DeriveStatus(validation))]);

        Assert.True(result.Pass);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(1, result.WarningCount);
        Assert.Contains("REPAIR_EQUALS_OBSERVED", Codes(result));
    }

    [Fact]
    public void Doppelte_Ids_und_mehrfach_referenzierte_Candidates_sind_Errors()
    {
        var candidates = new[] { Candidate("C-1"), Candidate("C-1") };
        var canonical = new[] { Canonical("K-1", "C-1"), Canonical("K-2", "C-1") };
        var result = LedgerQualityGate.Evaluate(candidates, canonical,
            [Validated(canonical[0]), Validated(canonical[1])]);

        Assert.Contains("DUPLICATE_CANDIDATE_ID", Codes(result));
        Assert.Contains("CANDIDATE_REFERENCED_MULTIPLE_TIMES", Codes(result));
    }

    [Fact]
    public void Unbekannte_Candidate_Referenz_ist_ein_Error()
    {
        var candidates = new[] { Candidate("C-1") };
        var canonical = new[] { Canonical("K-1", "C-1", "C-GHOST") };
        var result = LedgerQualityGate.Evaluate(candidates, canonical, [Validated(canonical[0])]);

        var unknown = Assert.Single(result.Violations, x => x.Code == "UNKNOWN_CANDIDATE_REFERENCE");
        Assert.Equal(["C-GHOST"], unknown.Ids);
    }
}
