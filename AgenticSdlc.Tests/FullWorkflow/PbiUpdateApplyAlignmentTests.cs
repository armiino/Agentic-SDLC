using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-26-C (A1, deterministischer Kern, kein LLM): PbiUpdateApply übernimmt akzeptierte Angleichungen
// (Titel/Statement/AK) beim PBI-Merge, klärt needs_clarify -> active und verankert die Provenance neu (R-31).
// Ohne Alignment bleibt alles wie bisher (Regressionsschutz für den bestehenden Struktur-Pfad).
public sealed class PbiUpdateApplyAlignmentTests
{
    private static ProjectStateItem Pbi(string id, string title, string status = "needs_clarify") => new(
        id, "pbi", title, status, "test", null, 3,
        "baseline-run", null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "altes Ziel", Title: title, AcceptanceCriteria: ["alt AK"], LinkedRequirementIds: ["REQ-1"],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "needs_clarify", Mvp: null, Trace: null));

    private static ProjectStateDocument Core(params ProjectStateItem[] items)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static PbiStateChangeOperation MarkChanged(string pbi = "PBI-1", string req = "REQ-1")
        => new("MARK_CHANGED", req, pbi, null, null, null, $"Requirement {req} verfeinert");

    private static PbiStateChangePlanDocument Plan(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment>? aligns = null)
        => new(1, "p1", DateTime.UnixEpoch, "ingest-run", ops, aligns);

    [Fact]
    public void Akzeptiertes_Alignment_gleicht_PBI_an_und_klaert_needs_clarify()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"));
        var align = new PbiAlignment("PBI-1", "Neuer Titel (14 Tage)", "Als X will ich 14 Tage", ["AK neu: 14 Tage"],
            "An REQ-1 (14 Tage) angeglichen", ["REQ-1"]);
        var plan = Plan([MarkChanged()], [align]);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "align-run", [align]);
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("active", pbi.Status);                          // needs_clarify -> active
        Assert.Equal("Neuer Titel (14 Tage)", pbi.Pbi!.Title);
        Assert.Equal("Neuer Titel (14 Tage)", pbi.Text);             // Text folgt dem Titel
        Assert.Equal("Als X will ich 14 Tage", pbi.Pbi.Goal);
        Assert.Equal(["AK neu: 14 Tage"], pbi.Pbi.AcceptanceCriteria);
        Assert.Equal(4, pbi.Version);                                // 3 -> 4 (EINE Version, nicht 2)
        Assert.Equal("align-run", pbi.SourceRunId);                  // R-31: neuer Ursprung
        Assert.Contains(pbi.History!, v => v.Text == "Alter Titel"); // alte Fassung in der History erhalten
        Assert.Contains("PBI-1", report.UpdatedPbis);
    }

    [Fact]
    public void Leere_Vorschlagsfelder_behalten_das_Original()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"));
        var align = new PbiAlignment("PBI-1", "Nur Titel neu", null, null, "nur Titel", ["REQ-1"]);
        var plan = Plan([MarkChanged()], [align]);

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "align-run", [align]);
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("Nur Titel neu", pbi.Pbi!.Title);
        Assert.Equal("altes Ziel", pbi.Pbi.Goal);          // Statement leer => Original
        Assert.Equal(["alt AK"], pbi.Pbi.AcceptanceCriteria); // AK leer => Original
    }

    [Fact]
    public void Ohne_Alignment_bleibt_needs_clarify_und_alte_Provenance()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel"));
        var plan = Plan([MarkChanged()]);

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "align-run");
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("needs_clarify", pbi.Status);       // unverändert markiert
        Assert.Equal("Alter Titel", pbi.Pbi!.Title);     // Inhalt unberührt
        Assert.Equal("baseline-run", pbi.SourceRunId);   // R-31: reine Struktur-Op erbt keinen neuen Ursprung
    }

    [Fact]
    public void Blocked_bleibt_blocked_trotz_Alignment()
    {
        var core = Core(Pbi("PBI-1", "Alter Titel", status: "blocked_by_decision"));
        var align = new PbiAlignment("PBI-1", "Neuer Titel", null, null, "angeglichen", ["REQ-1"]);
        var plan = Plan([MarkChanged()], [align]);

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "align-run", [align]);
        var pbi = result.Items.First(i => i.ItemId == "PBI-1");

        Assert.Equal("blocked_by_decision", pbi.Status); // Alignment klärt nur needs_clarify, nicht blocked
        Assert.Equal("Neuer Titel", pbi.Pbi!.Title);      // Inhalt wird trotzdem angeglichen
    }

    // R-26-C: Decisions -> Alignments-Extraktion (Governance: ohne explizite accept/edit wird NICHT angeglichen).
    [Fact]
    public void AcceptedAlignments_respektiert_accept_edit_skip_und_default_nicht_angleichen()
    {
        var proposal = new PbiAlignment("PBI-1", "Vorschlag-Titel", "Vorschlag-Statement", ["Vorschlag-AK"],
            "Agent-Begründung", ["REQ-1"]);
        var plan = Plan([MarkChanged()], [proposal]);

        // accept: Vorschlag 1:1 übernommen
        var accept = PbiUpdateApplyExec.AcceptedAlignments(plan,
            [new PbiAlignmentDecision("PBI-1", "accept", null, null, null, null)]);
        Assert.Single(accept);
        Assert.Equal("Vorschlag-Titel", accept[0].ProposedTitle);

        // edit: editierte Felder gewinnen, leere fallen auf den Vorschlag zurück
        var edit = PbiUpdateApplyExec.AcceptedAlignments(plan,
            [new PbiAlignmentDecision("PBI-1", "edit", "Editierter Titel", null, ["AK editiert"], "passt")]);
        Assert.Single(edit);
        Assert.Equal("Editierter Titel", edit[0].ProposedTitle);
        Assert.Equal("Vorschlag-Statement", edit[0].ProposedStatement); // leer => Vorschlag
        Assert.Equal(["AK editiert"], edit[0].ProposedAcceptanceCriteria);

        // skip und keine Entscheidung => nichts angleichen
        Assert.Empty(PbiUpdateApplyExec.AcceptedAlignments(plan,
            [new PbiAlignmentDecision("PBI-1", "skip", null, null, null, "verworfen")]));
        Assert.Empty(PbiUpdateApplyExec.AcceptedAlignments(plan, null));
        Assert.Empty(PbiUpdateApplyExec.AcceptedAlignments(plan, []));
    }

    // R-26-C Slice 3 (deterministisch, kein LLM): der Agent-Input — welche PBIs brauchen Angleichung, mit
    // welchem Vorher/Nachher. MARK_CHANGED zieht die alte Fassung aus der History.
    [Fact]
    public void AlignTargets_sammelt_MarkChanged_mit_Vorher_Nachher()
    {
        var req = new ProjectStateItem("REQ-1", "requirement", "Archivierung nach vierzehn Tagen", "accepted", "test", null, 2,
            "run", null, null, null, null, [], [], new Dictionary<string, string>(),
            History: [new ProjectStateItemVersion(1, "Archivierung nach sieben Tagen", "accepted", "test", null, [], DateTime.UnixEpoch, null)]);
        var core = Core(req, Pbi("PBI-1", "Automatische Archivierung"));
        var plan = Plan([MarkChanged("PBI-1", "REQ-1")]);

        var t = Assert.Single(PbiAlignTargets.Collect(plan, core));
        Assert.Equal("PBI-1", t.PbiId);
        Assert.Equal("Automatische Archivierung", t.CurrentTitle);
        var trig = Assert.Single(t.Triggers);
        Assert.Equal("REQ-1", trig.RequirementId);
        Assert.Equal("Archivierung nach vierzehn Tagen", trig.NewText);
        Assert.Equal("Archivierung nach sieben Tagen", trig.OldText);
    }

    // O3a + O3b: Collect sammelt EXTEND_PBI (extend, mit PbiId) UND NEW_PBI (create, mit TargetRequirementId,
    // PbiId null). MARK_CHANGED/SUPERSEDE bleiben align (in eigenen Tests abgedeckt).
    [Fact]
    public void AlignTargets_sammelt_EXTEND_und_NEW()
    {
        var core = Core(Pbi("PBI-1", "X"));
        var plan = Plan([
            new PbiStateChangeOperation("EXTEND_PBI", "REQ-1", "PBI-1", null, null, null, "x"),
            new PbiStateChangeOperation("NEW_PBI", "REQ-2", null, "FC-01", null, null, "y")]);

        var targets = PbiAlignTargets.Collect(plan, core);
        Assert.Equal(2, targets.Count);
        Assert.Contains(targets, t => t.PbiId == "PBI-1");                                   // extend
        Assert.Contains(targets, t => t.PbiId is null && t.TargetRequirementId == "REQ-2");  // create
    }

    // R-26-C Slice 3: SUPERSEDE nimmt das ERSATZ-Requirement als neue Fassung, das ersetzte als alte Fassung.
    [Fact]
    public void AlignTargets_SUPERSEDE_nimmt_Ersatz_als_neue_altes_als_alte_Fassung()
    {
        var oldReq = new ProjectStateItem("REQ-1", "requirement", "Archivierung nach sieben Tagen", "superseded", "test", null, 1,
            "run", null, null, null, null, [], [], new Dictionary<string, string>());
        var newReq = new ProjectStateItem("REQ-2", "requirement", "Archivierung nach vierzehn Tagen", "accepted", "test", null, 1,
            "run", null, null, null, null, [], [], new Dictionary<string, string>());
        var core = Core(oldReq, newReq, Pbi("PBI-1", "Automatische Archivierung"));
        var plan = Plan([new PbiStateChangeOperation("SUPERSEDE_PBI", "REQ-1", "PBI-1", null, "REQ-2", null, "REQ-1 ersetzt durch REQ-2")]);

        var t = Assert.Single(PbiAlignTargets.Collect(plan, core));
        var trig = Assert.Single(t.Triggers);
        Assert.Equal("REQ-2", trig.RequirementId);                       // neue Fassung = Ersatz
        Assert.Equal("Archivierung nach vierzehn Tagen", trig.NewText);
        Assert.Equal("Archivierung nach sieben Tagen", trig.OldText);    // alte Fassung = ersetztes Requirement
    }
}
