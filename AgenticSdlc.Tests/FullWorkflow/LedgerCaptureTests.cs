using System.Text.Json.Nodes;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W2 L/LCR-Capture (Konzept §11/§12.0-3): rein lesender, deterministischer Export der Messpunkte aus
// bestehenden Ledger-Run-Artefakten. Die Tests beweisen die ARCHITEKTONISCHE Grenze: L = Maker-DRAFT
// (nicht der reparierte Endstand), LCR = validierte Claims + Miss-Signal + QC-Protokolle, und die
// HITL-Messgrenze: step-03b-adjudicated wird NIE gelesen.
public sealed class LedgerCaptureTests
{
    private const string RunId = "20260904_120000_test01";

    private static string Fixture(bool mitCanonicalGate = true, bool mitDraft = true)
    {
        var repo = Directory.CreateTempSubdirectory("lcap-").FullName;
        var run = Path.Combine(repo, "runs", "ledger", RunId);

        if (mitDraft)
            Write(run, "step-02-canonical-draft/output.json",
                """{"entries":[{"id":"CL-001","proposition":"DRAFT-Fassung der Aussage","sourceUnitIds":["AU-0001"]}]}""");
        Write(run, "step-02-canonical/output.json",
            """{"entries":[{"id":"CL-001","proposition":"REPARIERTE-Fassung der Aussage","sourceUnitIds":["AU-0001"]},{"id":"CL-002","proposition":"Vom Repair ergänzt","sourceUnitIds":["AU-0002"]}]}""");
        Write(run, "step-03-facet-validation/output.json",
            """{"entries":[{"entry":{"id":"CL-001"},"validation":{"verdict":"grounded"},"claimStatus":"approved"}]}""");
        Write(run, "step-01d-unused-unit-ledger-compare/output.json",
            $$"""{"items":[{"unitId":"AU-0007","verdict":"missing_claim","reason":"fehlt"},{"unitId":"AU-0008","verdict":"needs_human","reason":"{{UnusedUnitCompareRepair.DowngradePrefix}}keine ID genannt"}]}""");
        if (mitCanonicalGate)
            Write(run, "gate/canonical-gate.json",
                """{"pass":true,"attempts":[{"attemptNumber":1,"gatePass":false},{"attemptNumber":2,"gatePass":true}]}""");
        Write(run, "gate/ledger-quality.json", """{"pass":true,"errorCount":0}""");
        // HITL-Messgrenze: liegt im Run, darf aber NIE in den Capture fließen.
        Write(run, "step-03b-adjudicated/consumable.json", """{"claims":[{"id":"ADJUDICATED_POISON"}]}""");
        return repo;
    }

    private static void Write(string runDir, string relative, string json)
    {
        var path = Path.Combine(runDir, relative.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
    }

    [Fact]
    public void L_ist_der_Maker_Draft_nicht_der_reparierte_Endstand()
    {
        var repo = Fixture();
        LedgerCaptureRunner.Capture(repo, RunId);

        var l = File.ReadAllText(Path.Combine(repo, "runs", "ledger", RunId, "capture", "l-machine.json"));
        Assert.Contains("DRAFT-Fassung", l);
        Assert.DoesNotContain("REPARIERTE-Fassung", l);
        Assert.DoesNotContain("Vom Repair ergänzt", l);
    }

    [Fact]
    public void LCR_traegt_Verdikte_MissSignal_und_QC_Protokolle()
    {
        var repo = Fixture();
        var report = LedgerCaptureRunner.Capture(repo, RunId);

        var lcr = JsonNode.Parse(File.ReadAllText(Path.Combine(repo, "runs", "ledger", RunId, "capture", "lcr-machine.json")))!;
        Assert.Equal("approved", lcr["entries"]![0]!["claimStatus"]!.GetValue<string>());
        Assert.Equal(2, lcr["missSignal"]!.AsArray().Count);
        Assert.Equal(2, lcr["qc"]!["canonicalGate"]!["attempts"]!.AsArray().Count);
        Assert.Equal(1, lcr["qc"]!["referenceRepairDowngrades"]!.GetValue<int>());
        Assert.Equal(1, report.LEntries);
        Assert.Equal(1, report.LcrEntries);
        Assert.Equal(2, report.MissItems);
        Assert.Equal(1, report.ReferenceRepairDowngrades);
        Assert.Equal(2, report.CanonicalAttempts);
    }

    [Fact]
    public void HITL_Messgrenze_step03b_wird_nie_gelesen()
    {
        var repo = Fixture();
        var report = LedgerCaptureRunner.Capture(repo, RunId);

        var captureDir = Path.Combine(repo, "runs", "ledger", RunId, "capture");
        foreach (var file in Directory.GetFiles(captureDir))
            Assert.DoesNotContain("ADJUDICATED_POISON", File.ReadAllText(file));
        Assert.True(report.PreAdjudication);
        Assert.DoesNotContain(report.Artifacts, a => a.Path.Contains("step-03b", StringComparison.Ordinal));
    }

    [Fact]
    public void Fehlendes_canonical_gate_wird_toleriert_und_als_Hinweis_vermerkt()
    {
        var repo = Fixture(mitCanonicalGate: false);
        var report = LedgerCaptureRunner.Capture(repo, RunId);

        Assert.Null(report.CanonicalAttempts);
        Assert.Contains(report.Notes, n => n.Contains("canonical-gate.json fehlt", StringComparison.Ordinal));
    }

    [Fact]
    public void Fehlender_Draft_scheitert_LAUT()
    {
        var repo = Fixture(mitDraft: false);
        var ex = Assert.Throws<InvalidOperationException>(() => LedgerCaptureRunner.Capture(repo, RunId));
        Assert.Contains("step-02-canonical-draft", ex.Message);
    }

    [Fact]
    public void Wiederholter_Capture_ist_bit_stabil_und_Hashes_stimmen()
    {
        var repo = Fixture();
        LedgerCaptureRunner.Capture(repo, RunId);
        var captureDir = Path.Combine(repo, "runs", "ledger", RunId, "capture");
        var erste = File.ReadAllBytes(Path.Combine(captureDir, "l-machine.json"));

        var report = LedgerCaptureRunner.Capture(repo, RunId);
        Assert.Equal(erste, File.ReadAllBytes(Path.Combine(captureDir, "l-machine.json")));

        var draftPath = Path.Combine(repo, "runs", "ledger", RunId, "step-02-canonical-draft", "output.json");
        var erwartet = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(draftPath)));
        Assert.Equal(erwartet, report.Artifacts.Single(a => a.Role == "l-source").Sha256);
    }
}
