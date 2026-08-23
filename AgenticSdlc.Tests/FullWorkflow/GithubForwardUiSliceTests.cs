using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Phase-1i ② (23.08.): Forward-UI im Ein-Graph + Doc-Diff + Relevanz-Anzeige — Pins.
public sealed class GithubForwardUiSliceTests
{
    private static GithubForwardOp Op(string kind, string pbiId = "PBI-001", string? title = null, string? body = null)
        => new(kind, pbiId, null, title, body, null, null, null, null, "test", "deterministic");

    private static GithubForwardPlanDocument Plan(params GithubForwardOp[] ops)
        => new(1, "plan-1", DateTime.UnixEpoch, "run", "o/r", [.. ops]);

    [Fact]
    public void Relevanz_NoChange_faellt_aus_der_Liste_opIds_bleiben_Plan_Indizes()
    {
        var plan = Plan(
            Op(GithubForwardKind.NoChange),
            Op(GithubForwardKind.UpdateIssue, "PBI-002"),
            Op(GithubForwardKind.NoChange, "PBI-003"),
            Op(GithubForwardKind.UpsertFile, "doc:docs/x.md", title: "docs/x.md", body: "# X\n\n> Version: 2 · Stand: x\n\nInhalt"));
        var session = GithubForwardReviewAdapter.BuildSession("r", plan);
        Assert.Equal(2, session.Items.Count);
        Assert.Equal(["op-1", "op-3"], session.Items.Select(i => i.ItemId).ToList());   // Indizes = Plan-Wahrheit
        Assert.Contains("2× geprüft ohne Änderungsbedarf", session.Subtitle);
        Assert.Contains("2 Änderungs-Operation(en)", session.Subtitle);
    }

    [Fact]
    public void Relevanz_alles_in_sync_sagt_es_ehrlich()
    {
        var session = GithubForwardReviewAdapter.BuildSession("r", Plan(Op(GithubForwardKind.NoChange)));
        Assert.Empty(session.Items);
        Assert.Contains("Alles in sync — 1× geprüft", session.Subtitle);
    }

    [Fact]
    public void DocDiff_ohne_Spiegel_ehrlich_mit_Spiegel_Sektions_Diff()
    {
        var alt = "# Doc\n\n> Version: 3 · Stand: x\n\n## A\nalt\n\n## B\nbleibt\n";
        var neu = "# Doc\n\n> Version: 4 · Stand: y\n\n## A\nneu\n\n## C\nzugang\n\n## B\nbleibt\n";
        var ohne = GithubDocsMirror.DiffSummary(null, neu);
        Assert.Contains("Version 4", ohne);
        var mit = GithubDocsMirror.DiffSummary(alt, neu);
        Assert.Contains("Version 3 → 4", mit);
        Assert.Contains("neue Sektion(en): ## C", mit);
        Assert.DoesNotContain("entfallen", mit);                                       // nichts ist weg
        Assert.DoesNotContain("## B", mit);                                            // Unverändertes bleibt still
        Assert.Contains("geändert: ## A", mit);
        Assert.DoesNotContain("# Doc", mit.Replace("## ", ""));                        // Kopf-/Versions-Zeile ist kein Inhalts-Diff
    }

    [Fact]
    public void Mirror_Roundtrip_und_UpsertItem_traegt_DocDiff_Note()
    {
        var repoRoot = Directory.CreateTempSubdirectory("fwdui-").FullName;
        try
        {
            GithubDocsMirror.Write(repoRoot, "docs/x.md", "# Doc\n\n> Version: 1 · Stand: x\n\n## A\nalt\n");
            Assert.Contains("## A", GithubDocsMirror.Read(repoRoot, "docs/x.md"));

            var plan = Plan(Op(GithubForwardKind.UpsertFile, "doc:docs/x.md", title: "docs/x.md",
                body: "# Doc\n\n> Version: 2 · Stand: y\n\n## A\nneu\n"));
            var session = GithubForwardReviewAdapter.BuildSession("r", plan, repoRoot: repoRoot);
            var note = session.Items.Single().Notes.Single(n => n.Label.StartsWith("Doc-Diff", StringComparison.Ordinal));
            Assert.Contains("Version 1 → 2", note.Text);
            Assert.Contains("geändert: ## A", note.Text);
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }

    [Fact]
    public void Pipeline_Stufe_wird_erkannt_fuer_die_Resume_Kette()
    {
        Assert.Equal("20260823_x", AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.TryGetPipelineRunId(
            Path.Combine("runs", "fullworkflow", "20260823_x", "07-github")));
        Assert.Null(AgenticSdlc.Host.FullWorkflow.PbiUpdate.PbiUpdateReviewRunner.TryGetPipelineRunId(
            Path.Combine("runs", "github-forward", "20260823_x", "plan")));
    }

    [Fact]
    public void Steward_Faehigkeitsliste_kennt_das_forward_gate()
    {
        Assert.Contains("github-forward-gate", AgenticSdlc.Host.Steward.StewardRunTools.SupportedUiGates);
    }
}
