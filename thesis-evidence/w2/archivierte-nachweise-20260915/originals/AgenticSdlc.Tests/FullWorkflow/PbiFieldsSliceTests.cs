using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Slice S Teil 2 (21.08.): Prio + Schätzung als gated PBI-Felder — EINE Feld-Naht (PbiFields),
// SET-Ops durch die pbi-update-Bahn (Gate prüft Wert, Apply schreibt NUR Metadata, kein Escalate),
// Drafting-Vorschlag beim Entstehen, Projektion in Label-Familie/Footer, Steward-Plan-Bauer.
public sealed class PbiFieldsSliceTests
{
    private static ProjectStateItem Pbi(string id, string title, string status = "active") => new ProjectStateItem(
        id, "pbi", title, "test", null, 3,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "Ziel", Title: title, AcceptanceCriteria: ["AK"], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "ready", Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Feature(string id) => new ProjectStateItem(
        id, "feature", id, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Req(string id) => new ProjectStateItem(
        id, "requirement", $"{id} Text", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation SetOp(string kind, string pbiId, string? value)
        => new(kind, "", pbiId, null, null, null, "Autor-Diktat", Value: value);

    [Fact]
    public void PbiFields_normalisiert_deutsche_Eingaben_und_verwirft_Ungueltiges()
    {
        Assert.Equal("high", PbiFields.NormalizePriority("hoch"));
        Assert.Equal("medium", PbiFields.NormalizePriority(" MEDIUM "));
        Assert.Null(PbiFields.NormalizePriority("dringend"));
        Assert.Equal("M", PbiFields.NormalizeEstimate("m"));
        Assert.Null(PbiFields.NormalizeEstimate("XL"));
        Assert.Equal("hoch", PbiFields.PriorityDe("high"));
    }

    [Fact]
    public void Gate_verlangt_gueltige_Werte_fuer_Set_Ops()
    {
        var core = Core(Pbi("PBI-1", "Eins"));
        var bad = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "r",
            [SetOp(PbiUpdateKind.SetPriority, "PBI-1", "urgent"), SetOp(PbiUpdateKind.SetEstimate, "PBI-1", null)]);
        var report = PbiUpdateGate.Check(core, bad, new HashSet<string>());
        Assert.False(report.Pass);
        Assert.Equal(2, report.Errors.Count(e => e.Code == "FIELD_VALUE_INVALID"));

        var good = new PbiStateChangePlanDocument(1, "p2", DateTime.UnixEpoch, "r",
            [SetOp(PbiUpdateKind.SetPriority, "PBI-1", "high"), SetOp(PbiUpdateKind.SetEstimate, "PBI-1", "L")]);
        Assert.True(PbiUpdateGate.Check(core, good, new HashSet<string>()).Pass);
    }

    [Fact]
    public void Apply_setzt_Felder_ohne_Status_Wirkung()
    {
        var core = Core(Pbi("PBI-1", "Eins"));
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "r",
            [SetOp(PbiUpdateKind.SetPriority, "PBI-1", "high"), SetOp(PbiUpdateKind.SetEstimate, "PBI-1", "M")]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0, 1 }, "r");

        var pbi = Assert.Single(result.Items, i => i.ItemId == "PBI-1");
        Assert.Equal("high", pbi.Metadata[PbiFields.MetaPriority]);
        Assert.Equal("M", pbi.Metadata[PbiFields.MetaEstimate]);
        Assert.Equal("active", pbi.Status);                       // KEIN needs_clarify durch Feld-Pflege
        Assert.Equal(4, pbi.Version);                             // ein Merge, eine Version
        Assert.Equal(["PBI-1"], report.UpdatedPbis);
    }

    [Fact]
    public void Apply_uebernimmt_Draft_Prio_und_Schaetzung_bei_NEW_PBI_und_verwirft_Ungueltiges()
    {
        var core = Core(Feature("FC-01"), Req("REQ-2"));
        var draft = new PbiAlignment(null, "Neues PBI", "Statement", ["AK"], "create", ["REQ-2"],
            TargetRequirementId: "REQ-2", ProposedPriority: "high", ProposedEstimate: "XXL");
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "r",
            [new PbiStateChangeOperation("NEW_PBI", "REQ-2", null, "FC-01", null, null, "neu")], [draft]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "r", [draft]);

        var pbi = Assert.Single(result.Items, i => i.ItemId == report.NewPbis.Single());
        Assert.Equal("high", pbi.Metadata[PbiFields.MetaPriority]);
        Assert.False(pbi.Metadata.ContainsKey(PbiFields.MetaEstimate));   // ungültiger Agent-Wert → verworfen
    }

    [Fact]
    public void AcceptedAlignments_edit_uebersteuert_Prio_und_Schaetzung()
    {
        var proposal = new PbiAlignment("PBI-1", "T", null, null, "align", ["REQ-1"],
            ProposedPriority: "low", ProposedEstimate: "S");
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "r",
            [SetOp(PbiUpdateKind.MarkChanged, "PBI-1", null)], [proposal]);
        var decided = PbiUpdateApplyExec.AcceptedAlignments(plan,
            [new PbiAlignmentDecision("PBI-1", "edit", null, null, null, null, EditedPriority: "high", EditedEstimate: "M")]);
        var a = Assert.Single(decided);
        Assert.Equal("high", a.ProposedPriority);
        Assert.Equal("M", a.ProposedEstimate);
    }

    [Fact]
    public void Labels_pflegen_die_prio_Familie_und_erhalten_Fremd_Labels()
    {
        var entry = new GithubSyncEntry("PBI-1", "T", "active", "ready", [], false, null, Priority: "high");
        var labels = GithubIssueLabels.For(entry, ["team-label", "prio:low", "pbi"]);
        Assert.Contains("prio:high", labels);
        Assert.DoesNotContain("prio:low", labels);                 // alte Familie getilgt
        Assert.Contains("team-label", labels);                     // Fremd-Label bleibt (R-30-Merge)
        Assert.DoesNotContain("prio:", GithubIssueLabels.For(entry with { Priority = null }, []));
    }

    [Fact]
    public void Footer_traegt_Prio_und_Schaetzung_nur_wenn_gesetzt_und_parst_zurueck()
    {
        var plain = new GithubSyncEntry("PBI-1", "T", "active", "ready", [], false, null);
        Assert.DoesNotContain("Prio", GithubIssueTemplate.Render(plain, "q"));

        var body = GithubIssueTemplate.Render(plain with { Priority = "high", Estimate = "M" }, "q");
        Assert.Contains("· Prio hoch · Schätzung M · Quelle: q", body);
        var parsed = GithubIssueTemplate.Parse(body);
        Assert.Equal("hoch", parsed.Prio);
        Assert.Equal("M", parsed.Estimate);
    }

    [Fact]
    public void PbiFieldsPlan_baut_Set_Ops_aus_deutschen_Wuenschen_und_meldet_Fehler_laut()
    {
        var core = Core(Pbi("PBI-1", "Eins"));
        var (plan, errors) = PbiFieldsPlan.Build(core,
            [
                new PbiFieldWish("PBI-1", Priority: "hoch", Estimate: "m", Begruendung: "wichtig"),
                new PbiFieldWish("PBI-9", Priority: "hoch"),                 // kein PBI
                new PbiFieldWish("PBI-1", Estimate: "XXL"),                  // ungültig
            ], "run-1");

        Assert.Equal(2, plan.Operations.Count);
        Assert.Equal("high", plan.Operations[0].Value);
        Assert.Equal("M", plan.Operations[1].Value);
        Assert.Equal(2, errors.Count);
        Assert.True(PbiUpdateGate.Check(core, plan, new HashSet<string>()).Pass);   // Plan besteht sein eigenes Gate
    }

    [Fact]
    public void Pending_Frische_pbi_fields_haengt_nicht_an_needs_clarify()
    {
        var core = Core(Pbi("PBI-1", "Eins"));   // active — für clarify-sweep wäre das ÜBERHOLT
        var payload = System.Text.Json.JsonSerializer.SerializeToElement(new { ok = true });
        var (withPending, _) = PendingReviewRegistry.Register(core, PbiFieldsPlan.Bahn, "cmd", payload, ["PBI-1"], "run-1");

        var entry = Assert.Single(PendingReviewRegistry.ListOpen(withPending));
        Assert.False(entry.Ueberholt);

        // Versions-Anker gilt weiter: zwischenzeitliche Änderung am PBI ⇒ überholt.
        var changed = withPending with
        { Items = [.. withPending.Items.Select(i => i.ItemId == "PBI-1" ? i with { Version = i.Version + 1 } : i)] };
        Assert.True(Assert.Single(PendingReviewRegistry.ListOpen(changed)).Ueberholt);
    }

    [Fact]
    public void ReviewAdapter_zeigt_Set_Ops_verstaendlich()
    {
        var core = Core(Pbi("PBI-1", "Besuchsliste"));
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "r",
            [SetOp(PbiUpdateKind.SetPriority, "PBI-1", "high"), SetOp(PbiUpdateKind.SetEstimate, "PBI-1", "L")]);
        var session = PbiUpdateReviewAdapter.BuildSession("run", plan, core);
        Assert.Contains(session.Items, i => i.Summary.Contains("Priorität → hoch"));
        Assert.Contains(session.Items, i => i.Summary.Contains("Schätzung → L"));
    }
}
