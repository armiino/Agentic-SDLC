using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-37: not_applicable-Claims duerfen die Spur-Projektion NICHT erreichen (deterministisch statt Prompt-Verbot) —
// der Mess-Lauf 20260805_110317 bewies, dass der Agent sonst festgelegte Inhalte als Fragen umformuliert.
public sealed class EvidenceLedgerProjectionFilterTests
{
    private static SemanticLedgerEntry Claim(string id, string oqApplicability) => new(
        Id: id, Proposition: $"{id} Aussage", Kind: "requirement", Status: "open", Modality: "must",
        Scope: "pflege", TimeScope: null, Evidence: [new SemanticLedgerEvidence("transcript", "Zitat")],
        Disposition: new Dictionary<string, ArtifactDisposition>
        {
            ["requirements"] = new("required", "requirement"),
            ["open-questions"] = new(oqApplicability, "question"),
        },
        RiskLevel: "low", Notes: null);

    [Fact]
    public void NotApplicable_Claims_erreichen_die_Spur_nicht()
    {
        var text = EvidenceLedgerProjection.Project(
            [Claim("c-frage", "required"), Claim("c-festgelegt", "not_applicable")], "open-questions");

        Assert.Contains("c-frage", text);
        Assert.DoesNotContain("c-festgelegt", text);                            // was der Agent nie sieht, kann er nicht fragen
    }

    [Fact]
    public void Andere_Spur_sieht_denselben_Claim_weiterhin()
    {
        var text = EvidenceLedgerProjection.Project([Claim("c-festgelegt", "not_applicable")], "requirements");
        Assert.Contains("c-festgelegt", text);                                  // Filter ist spur-spezifisch, kein Global-Drop
    }
}
