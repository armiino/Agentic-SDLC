using AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;
using Xunit;

namespace AgenticSdlc.Tests.Review;

public sealed class EvidenceFirstSpikeTests
{
    [Fact]
    public void Generator_Parse_Reads_Claims()
    {
        var text = """
            {
              "markdown": "## Requirements\n- SAP write remains open. <!-- SC-SAP-WRITE -->",
              "claims": [
                {
                  "artifactClaimId": "REQ-001",
                  "artifact": "requirements",
                  "text": "SAP write remains open.",
                  "sourceClaimIds": ["SC-SAP-WRITE"],
                  "status": "undecided",
                  "modality": "open",
                  "scope": "mvp",
                  "timeScope": "mvp",
                  "assumption": false
                }
              ]
            }
            """;

        var parsed = EvidenceFirstArtifactGenerator.Parse(text)!;

        Assert.Contains("SAP", parsed.Markdown);
        Assert.Single(parsed.Claims);
        Assert.Equal("SC-SAP-WRITE", parsed.Claims[0].SourceClaimIds.Single());
    }

    [Fact]
    public void Verifier_Parse_Derives_Misrepresented_From_Status_Shift()
    {
        var text = """
            {
              "items": [
                {
                  "sourceClaimId": "SC-SAP-WRITE",
                  "artifactClaimIds": ["REQ-001", "REQ-002"],
                  "representation": "present",
                  "proposition": "preserved",
                  "status": "shifted",
                  "modality": "preserved",
                  "scope": "preserved",
                  "timeScope": "preserved",
                  "violations": ["STATUS_SHIFT"],
                  "overallVerdict": "PRESERVED",
                  "missingFacets": [],
                  "shiftedFacets": ["status"],
                  "reason": "Open became planned."
                }
              ]
            }
            """;

        var item = SemanticPreservationVerifier.Parse(text)!.Items.Single().Normalized();

        Assert.Equal("MISREPRESENTED", item.OverallVerdict);
        Assert.Contains("STATUS_SHIFT", item.Violations);
        Assert.Equal(2, item.ArtifactClaimIds.Count);
    }

    [Fact]
    public void Disposition_Check_Finds_Missing_Required_Claim()
    {
        var entry = new SemanticLedgerEntry(
            "SC-1",
            "Claim",
            "decision",
            "undecided",
            "open",
            "mvp",
            "mvp",
            [],
            new Dictionary<string, ArtifactDisposition>
            {
                ["requirements"] = new("required", "open_decision")
            },
            "high",
            null);

        var checks = SemanticLedgerChecks.CheckDispositionCoverage([entry], [], "requirements");

        Assert.Single(checks);
        Assert.Equal("required_disposition_missing", checks[0].Verdict);
    }

    [Fact]
    public void Completeness_Check_Finds_Missing_Verifier_Item()
    {
        var entry = new SemanticLedgerEntry(
            "SC-1",
            "Claim",
            "decision",
            "undecided",
            "open",
            "mvp",
            "mvp",
            [],
            new Dictionary<string, ArtifactDisposition>(),
            "high",
            null);

        var issues = SemanticLedgerChecks.CheckVerifierCompleteness([entry], []);

        Assert.Contains(issues, i => i.Code == "missing_source_claim_id");
    }

    [Fact]
    public void HumanArtifact_Parse_Reads_Result()
    {
        var text = """
            {
              "markdown": "# Requirements\n- SAP write remains open. [SC-SAP-WRITE]",
              "usedSourceClaimIds": ["SC-SAP-WRITE"],
              "readabilityNotes": "Readable."
            }
            """;

        var parsed = HumanArtifactGenerator.Parse(text)!;

        Assert.Contains("[SC-SAP-WRITE]", parsed.Markdown);
        Assert.Equal("SC-SAP-WRITE", parsed.UsedSourceClaimIds.Single());
    }

    [Fact]
    public void Projection_Check_Finds_Refs_And_Unreferenced_Bullets()
    {
        var claims = new[]
        {
            new GeneratedArtifactClaim(
                "REQ-001",
                "requirements",
                "SAP write remains open.",
                ["SC-SAP-WRITE"],
                "undecided",
                "open",
                "mvp",
                "mvp")
        };
        var markdown = """
            # Requirements
            - SAP write remains open. [SC-SAP-WRITE]
            - Unreferenced business statement.
            """;

        var report = HumanArtifactProjectionChecker.Check(claims, markdown);

        Assert.Empty(report.MissingRefs);
        Assert.Empty(report.UnknownRefs);
        Assert.Single(report.UnreferencedBullets);
        Assert.Single(report.ClaimResults);
    }

    [Fact]
    public void SemanticLedgerExtractor_Parse_Reads_Faceted_Entry()
    {
        var text = """
            {
              "entries": [
                {
                  "id": "SC-1",
                  "proposition": "SAP write is open.",
                  "kind": "decision",
                  "status": "undecided",
                  "modality": "open",
                  "scope": "mvp",
                  "timeScope": "mvp",
                  "evidence": [{ "source": "T.txt", "quote": "Ben: open" }],
                  "disposition": {
                    "requirements": { "applicability": "required", "representationMode": "open_decision" },
                    "architecture": { "applicability": "required", "representationMode": "open_decision" },
                    "risks": { "applicability": "required", "representationMode": "risk_reference" },
                    "open-questions": { "applicability": "required", "representationMode": "question" }
                  },
                  "riskLevel": "high",
                  "notes": "Keep open."
                }
              ]
            }
            """;

        var entries = SemanticLedgerExtractor.Parse(text);

        Assert.Single(entries);
        Assert.Equal("undecided", entries[0].Status);
        Assert.True(entries[0].Disposition.ContainsKey("requirements"));
    }

    [Fact]
    public void SemanticLedgerRecallMatcher_Parse_Normalizes()
    {
        var text = """
            {
              "verdict": "EXACT",
              "matchedIds": ["SC-1"],
              "propositionMatch": "exact",
              "statusMatch": "partial",
              "modalityMatch": "exact",
              "scopeMatch": "exact",
              "timeScopeMatch": "exact",
              "evidenceMatch": "partial",
              "dispositionMatch": "missed",
              "reason": "ok"
            }
            """;

        var verdict = SemanticLedgerRecallMatcher.Parse(text)!.Normalized();

        Assert.Equal("exact", verdict.Verdict);
        Assert.Equal("partial", verdict.StatusMatch);
        Assert.Equal("missed", verdict.DispositionMatch);
    }
}
