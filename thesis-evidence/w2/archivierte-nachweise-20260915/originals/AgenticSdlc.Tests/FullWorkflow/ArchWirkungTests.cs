using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 ③ A3 (06.08., §5.7): die WIRKUNGS-Seite der Rahmen — E-8-Trigger generalisiert (Wahrheits-Link =
// covers ∪ constrained_by; Rahmen-Änderungen wecken PBIs), geteilte ConstraintSwap-Naht (pbi-update UND
// Tor 2), Issue-Sektion „Technische Rahmenbedingungen", PBI-Weckruf-Kontext (beide Wahrheits-Seiten in der
// Review), AffectedItems byAspect (2. Hop über Rahmen-Kanten). Alles deterministisch, LLM-frei.
public sealed class ArchWirkungTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted") =>
        new ProjectStateItem(id, type, text, "test", null, 1, "r", null, null, null, null, ["SL-1"], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateItem Pbi(string id, string title) => Item(id, "pbi", title) with
    {
        Pbi = new PbiPayload("Ziel", title, ["AK 1"], ["REQ-1"], [], null, null, null, null)
    };

    private static ProjectStateRelation Rel(string from, string to, string type)
        => new(from, to, type, "test", new Dictionary<string, string>());

    private static ProjectStateDocument Core(ProjectStateItem[] items, ProjectStateRelation[]? rels = null)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], rels?.ToList() ?? [], [], []);

    private static ProjectStateDocument RahmenCore() => Core(
        [Item("ARCH-01", "architecture", "Firestore ist gesetzt"), Item("ARCH-02", "architecture", "Ersatz: lokale DB"),
         Item("REQ-1", "requirement", "Anforderung"), Pbi("PBI-1", "Bewohnerprofil anlegen")],
        [Rel("PBI-1", "ARCH-01", "constrained_by"), Rel("PBI-1", "REQ-1", "covers"), Rel("ARCH-02", "ARCH-01", "supersedes")]);

    // ---- E-8: die Ableitung weckt constrained_by-PBIs bei Rahmen-Änderungen ----

    [Fact]
    public void Derivation_weckt_Rahmen_PBIs_bei_REFINE_SUPERSEDE_CONTRADICT_auf_arch()
    {
        var core = Core(
            [Item("ARCH-01", "architecture", "Rahmen"), Item("ARCH-02", "architecture", "Ersatz"),
             Item("DEC-001", "decision", "Widerspruch", status: "open_decision"), Pbi("PBI-1", "PBI")],
            [Rel("PBI-1", "ARCH-01", "constrained_by"), Rel("ARCH-02", "ARCH-01", "supersedes"), Rel("DEC-001", "ARCH-01", "contradicts")]);

        var refine = PbiUpdateDerivation.Derive(core, [new("IN-1", StateChangeKind.Refine, "ARCH-01", "refined")]);
        Assert.Equal((PbiUpdateKind.MarkChanged, "PBI-1", "ARCH-01"),
            (refine.DeterministicOps.Single().Kind, refine.DeterministicOps.Single().PbiId, refine.DeterministicOps.Single().RequirementId));

        var supersede = PbiUpdateDerivation.Derive(core, [new("IN-2", StateChangeKind.Supersede, "ARCH-02", "superseded")]);
        var sup = supersede.DeterministicOps.Single();
        Assert.Equal((PbiUpdateKind.SupersedePbi, "PBI-1", "ARCH-01", "ARCH-02"), (sup.Kind, sup.PbiId, sup.RequirementId, sup.ReplacementRequirementId));

        var contradict = PbiUpdateDerivation.Derive(core, [new("IN-3", StateChangeKind.Contradict, "DEC-001", "contradicted")]);
        var blk = contradict.DeterministicOps.Single();
        Assert.Equal((PbiUpdateKind.BlockPbi, "PBI-1", "DEC-001"), (blk.Kind, blk.PbiId, blk.OpenDecisionRef));
    }

    [Fact]
    public void Neues_arch_Item_wird_NIE_unplaced_Wahrheit_ist_keine_Arbeit()
    {
        var core = Core([Item("ARCH-09", "architecture", "neuer Rahmen"), Item("REQ-9", "requirement", "neue Anforderung")]);

        var r = PbiUpdateDerivation.Derive(core,
            [new("IN-1", StateChangeKind.New, "ARCH-09", "added"), new("IN-2", StateChangeKind.New, "REQ-9", "added")]);

        Assert.Equal("REQ-9", Assert.Single(r.Unplaced).RequirementId);   // req geht zum Placement, arch nicht (A4)
    }

    // ---- Geteilte Naht: PbiUpdateApply macht beim arch-Ziel den ConstraintSwap (kein covers-Unfall) ----

    [Fact]
    public void Apply_SupersedePbi_mit_arch_Ziel_zieht_constrained_by_um_statt_covers_zu_schreiben()
    {
        var core = RahmenCore();
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "run",
            [new(PbiUpdateKind.SupersedePbi, "ARCH-01", "PBI-1", null, "ARCH-02", null, "Rahmen ersetzt")], null);

        var (result, _) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run");

        var hist = Assert.Single(result.Relations, r => r.RelationType == "constrained_by_superseded");
        Assert.Equal(("PBI-1", "ARCH-01", "ARCH-02"), (hist.FromId, hist.ToId, hist.Metadata["movedTo"]));
        Assert.Equal("ARCH-02", Assert.Single(result.Relations, r => r.RelationType == "constrained_by").ToId);
        Assert.DoesNotContain(result.Relations, r => r.RelationType == "covers" && r.ToId.StartsWith("ARCH"));
        var pbi = result.Items.Single(i => i.ItemId == "PBI-1");
        Assert.Equal(["REQ-1"], pbi.Pbi!.LinkedRequirementIds);           // req-Coverage unberührt
        Assert.Equal(Blocker.NeedsClarify, pbi.ReadStatus().Blocker);     // Weckruf
        Assert.DoesNotContain(CoreKangal.Check(result).Warnings, w => w.Code == "UNKNOWN_RELATION_TYPE");
    }

    [Fact]
    public void ConstraintSwap_ist_idempotent_und_meldet_fehlende_Kante()
    {
        var relations = new List<ProjectStateRelation> { Rel("PBI-1", "ARCH-01", "constrained_by") };

        Assert.True(ConstraintSwap.Swap("PBI-1", "ARCH-01", "ARCH-02", relations, "test"));
        Assert.False(ConstraintSwap.Swap("PBI-1", "ARCH-01", "ARCH-02", relations, "test"));   // Kante ist schon Historie
        Assert.Single(relations, r => r.RelationType == "constrained_by");
        Assert.False(ConstraintSwap.Swap("PBI-9", "ARCH-01", "ARCH-02", relations, "test"));   // nie da gewesen
    }

    // ---- Issue-Sektion: die Rahmen erscheinen im deterministischen Body ----

    [Fact]
    public void GithubSync_View_und_Issue_Body_tragen_die_Rahmen_Sektion()
    {
        var view = CoreViews.GithubSync(RahmenCore());
        var entry = view.Entries.Single(e => e.PbiId == "PBI-1");
        Assert.Equal("ARCH-01 — Firestore ist gesetzt", Assert.Single(entry.Constraints!));

        var body = GithubIssueTemplate.Render(entry, "test");
        Assert.Contains("### Technische Rahmenbedingungen\n\n> - ARCH-01 — Firestore ist gesetzt", body);
        Assert.Contains("### Abgedeckte Requirements\n\n> `REQ-1`", body);   // bestehende Sektionen unverändert

        // Nachzug ③ (20.08.): Rahmen ist PFLICHT-Sektion — ohne Constraints erscheint der Kopf LEER
        // (leer = leer, Autor-Entscheid), nicht mehr gar nicht.
        Assert.Contains("### Technische Rahmenbedingungen\n\n> &nbsp;\n",
            GithubIssueTemplate.Render(entry with { Constraints = null }, "test"));
    }

    // ---- PBI-Weckruf: die Review-Card zeigt beide Wahrheits-Seiten ----

    [Fact]
    public void PbiUpdate_Review_zeigt_die_Rahmen_des_betroffenen_PBIs_und_benennt_arch_Ausloeser()
    {
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "run",
            [new(PbiUpdateKind.MarkChanged, "ARCH-01", "PBI-1", null, null, null, "Rahmen geändert")], null);

        var s = PbiUpdateReviewAdapter.BuildSession("run-1", plan, RahmenCore());

        var item = Assert.Single(s.Items);
        Assert.Contains("technischer Rahmen wurde geändert", item.Summary);
        Assert.Contains(item.Notes, n => n.Label.StartsWith("Technische Rahmen") && n.Text.Contains("ARCH-01 — Firestore"));
    }

    // ---- A4 (work-Rolle -> Arbeit): Scan, Placement-Feed, Apply mit covers->arch (E-R2) ----

    private static ProjectStateItem WorkArch(string id, string text = "Push-Infrastruktur aufbauen") =>
        Item(id, "architecture", text) with { Architecture = new ArchitecturePayload(["work", "constraint"], "weil") };

    [Fact]
    public void UncoveredWorkArchs_zaehlt_nur_aktive_ungedeckte_work_Items_constrained_by_ist_keine_Deckung()
    {
        var core = Core(
            [WorkArch("ARCH-10"), WorkArch("ARCH-11"), WorkArch("ARCH-12") with { Architecture = new(["design"], "kein work") },
             Pbi("PBI-1", "PBI")],
            [Rel("PBI-1", "ARCH-11", "covers"),                       // umgesetzt -> raus
             Rel("PBI-1", "ARCH-10", "constrained_by")]);             // nur Wirkung -> BLEIBT Arbeit

        Assert.Equal(["ARCH-10"], PbiUpdateDerivation.UncoveredWorkArchs(core).Select(i => i.ItemId));
    }

    [Fact]
    public void Derive_fuettert_work_Archs_NUR_in_arch_aktiven_Laeufen_ins_Placement()
    {
        var core = Core([WorkArch("ARCH-10"), Item("REQ-9", "requirement", "req")]);

        var archRun = PbiUpdateDerivation.Derive(core, [new("IN-1", StateChangeKind.New, "ARCH-10", "added")]);
        var u = Assert.Single(archRun.Unplaced, x => x.Aspect == "architecture");
        Assert.Equal("ARCH-10", u.RequirementId);

        // Smoke-Schutz (Lehre Classify-Bridge): req-/Leer-Laeufe zuenden den Placement-Maker NICHT fuer den Rueckstau.
        var reqRun = PbiUpdateDerivation.Derive(core, [new("IN-2", StateChangeKind.New, "REQ-9", "added")]);
        Assert.DoesNotContain(reqRun.Unplaced, x => x.Aspect == "architecture");
        Assert.Empty(PbiUpdateDerivation.Derive(core, []).Unplaced);
    }

    [Fact]
    public void NewPbi_mit_arch_Ziel_schreibt_covers_arch_und_die_Seed_Regel_bleibt_req_only()
    {
        var core = Core([WorkArch("ARCH-10"), Item("FC-1", "feature", "Benachrichtigungen") with
            { Feature = new FeaturePayload("Benachrichtigungen", null, [], []) }]);
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "run",
            [new(PbiUpdateKind.NewPbi, "ARCH-10", null, "FC-1", null, null, "technische Arbeit")], null);

        var (result, report) = PbiUpdateApply.Apply(core, plan, new HashSet<int> { 0 }, "run");

        var pbiId = Assert.Single(report.NewPbis);
        var covers = Assert.Single(result.Relations, r => r.RelationType == "covers");
        Assert.Equal((pbiId, "ARCH-10"), (covers.FromId, covers.ToId));                       // E-R2 lebt
        Assert.Equal("architecture", result.Items.Single(i => i.ItemId == pbiId).Metadata["fromAspect"]);
        // Seed-Regel-Waechter: das ARCH-Wahrheits-Item bekommt NIE eine Feature-Kante (nur das Work-PBI).
        Assert.DoesNotContain(result.Relations, r => r.RelationType == "part_of_feature" && r.FromId == "ARCH-10");
        Assert.Contains(result.Relations, r => r.RelationType == "part_of_feature" && r.FromId == pbiId);
        var kangal = CoreKangal.Check(result);
        Assert.DoesNotContain(kangal.Errors, e => e.Code.StartsWith("I1_"));
    }

    [Fact]
    public void GithubSync_trennt_req_Deckung_und_Architektur_Arbeit_im_Body()
    {
        var pbi = Pbi("PBI-1", "Push bauen");
        var core = Core([WorkArch("ARCH-10"), Item("REQ-1", "requirement", "Anforderung"), pbi],
            [Rel("PBI-1", "REQ-1", "covers"), Rel("PBI-1", "ARCH-10", "covers")]);

        var entry = CoreViews.GithubSync(core).Entries.Single();
        Assert.Equal(["REQ-1"], entry.CoveredRequirementIds);                                 // REIN req
        Assert.Equal("ARCH-10 — Push-Infrastruktur aufbauen", Assert.Single(entry.CoveredArchitecture!));

        var body = GithubIssueTemplate.Render(entry, "test");
        Assert.Contains("### Umgesetzte Architektur-Arbeit\n\n> - ARCH-10", body);
        Assert.Contains("### Abgedeckte Requirements\n\n> `REQ-1`", body);
    }

    [Fact]
    public void PbiUpdate_Review_zeigt_die_Herkunft_technischer_Arbeit()
    {
        var core = Core([WorkArch("ARCH-10"), Item("FC-1", "feature", "F")]);
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "run",
            [new(PbiUpdateKind.NewPbi, "ARCH-10", null, "FC-1", null, null, "Arbeit")], null);

        var s = PbiUpdateReviewAdapter.BuildSession("run-1", plan, core);

        Assert.Contains(Assert.Single(s.Items).Notes,
            n => n.Label == "Herkunft" && n.Text.Contains("Architektur-Item ARCH-10"));
    }

    // ---- byAspect: der 2. Hop bringt die Rahmen-Seite in den Blast-Radius ----

    [Fact]
    public void AffectedItems_zaehlt_Rahmen_via_zweitem_Hop_und_gruppiert_byAspect()
    {
        var view = CoreViews.AffectedItems(RahmenCore(), new HashSet<string>(StringComparer.Ordinal) { "REQ-1" });

        Assert.Contains("PBI-1", view.Pbis.Select(i => i.ItemId));                       // Hop 1: covers
        Assert.Contains("ARCH-01", view.Architecture!.Select(i => i.ItemId));            // Hop 2: constrained_by
    }
}
