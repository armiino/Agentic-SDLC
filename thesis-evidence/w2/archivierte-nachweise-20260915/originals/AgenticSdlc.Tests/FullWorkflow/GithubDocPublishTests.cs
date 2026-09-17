using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Slice S Teil 1 (21.08., Bauplan team-sichtbarkeit-slice.md): Doc-Publish als One-way-Projektion —
// Seed nur bei Hash-Abweichung (In-Sync-Muster), Stempel im proposals-Ledger, Gate ohne Coverage-Zwang.
public sealed class GithubDocPublishTests
{
    private static ProjectStateDocument EmptyCore() => new("p", 4, DateTime.UnixEpoch, [], [], [], [], []);

    private static string TempRepo(out string reqDoc)
    {
        var root = Path.Combine(Path.GetTempPath(), "docpub-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs", "adr"));
        reqDoc = Path.Combine(root, "docs", "anforderungen.md");
        File.WriteAllText(reqDoc, "# Anforderungsdokument\n\n> Version: 3\n");
        File.WriteAllText(Path.Combine(root, "docs", "adr", "0001-test.md"), "# ADR 0001\n");
        return root;
    }

    [Fact]
    public void Seed_stempelt_und_wird_in_sync_und_meldet_Aenderungen()
    {
        var root = TempRepo(out var reqDoc);
        try
        {
            var core = EmptyCore();

            // Erstlauf: beide Docs werden publiziert, Inhalt trägt den GENERIERT-Kopf.
            var ops = GithubDocPublish.SeedOps(root, core);
            Assert.Equal(2, ops.Count);
            Assert.All(ops, o => Assert.Equal(GithubForwardKind.UpsertFile, o.Kind));
            var req = Assert.Single(ops, o => o.Title == "docs/anforderungen.md");
            Assert.StartsWith(GithubDocPublish.Header, req.Body);

            // Stempel wie nach echtem Apply → In-Sync ⇒ KEINE Ops (Issues-NoChange-Muster).
            core = GithubDocPublish.ApplyStamps(core,
                [.. ops.Select(o => (o.Title!, GithubProjectionHash.Compute(o.Body!), "sha-1"))], "o/r", "plan-1");
            Assert.Empty(GithubDocPublish.SeedOps(root, core));
            Assert.Equal("sha-1", GithubDocPublish.StampOf(core, "docs/anforderungen.md")!["sha"]);

            // Inhalt ändert sich ⇒ GENAU das eine Doc kommt wieder.
            File.AppendAllText(reqDoc, "\nNeue Zeile.\n");
            var again = GithubDocPublish.SeedOps(root, core);
            Assert.Equal("docs/anforderungen.md", Assert.Single(again).Title);

            // Stempel-Upsert ist idempotent je Pfad (ein Proposal, keine Duplikate).
            core = GithubDocPublish.ApplyStamps(core, [("docs/anforderungen.md", "h2", "sha-2")], "o/r", "plan-2");
            Assert.Single(core.Proposals, p => p.PayloadPath == "docs/anforderungen.md");
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void UpsertFile_zaehlt_als_Write_und_besteht_das_Gate_ohne_Coverage()
    {
        var doc = new GithubForwardOp(GithubForwardKind.UpsertFile, "doc:docs/anforderungen.md", null,
            "docs/anforderungen.md", "Inhalt", null, null, null, "doc docs/anforderungen.md", "r", "deterministic");
        var plan = new GithubForwardPlanDocument(1, "p1", DateTime.UnixEpoch, "src", "o/r", [doc]);

        // Der erste Vermerk-only-Bug (R-62b) darf sich für Docs nicht wiederholen: Write ⇒ Client nötig.
        Assert.True(GithubForwardApply.RequiresClient(plan, new HashSet<string> { "op-0" }, new HashSet<string>()));

        // Gate: Doc-Ops verlangen kein Issue-Ziel und zählen nicht in die PBI-Coverage.
        var ok = GithubForwardGate.Check(plan, [], []);
        Assert.True(ok.Pass, string.Join("; ", ok.Errors.Select(e => e.Code)));
    }
}
