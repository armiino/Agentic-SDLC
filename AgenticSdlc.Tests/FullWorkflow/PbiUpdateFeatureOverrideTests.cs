using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B1 (Fall-C-Bündelung, Lösungsweg B, deterministisch): der Mensch korrigiert im Review die Feature-Zuordnung eines
// NEW_PBI auf ein anderes BESTEHENDES Feature. Getestet ohne LLM/Dateisystem: Session-Bau (Optionen + Default),
// FeatureOverrides (nur echte Änderungen), Plan-Rewrite (nur existierende Ziele), Round-Trip bis in den Core-Apply
// und die Feature-Landkarte (ResolveReference).
public sealed class PbiUpdateFeatureOverrideTests
{
    private static ProjectStateItem Req(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateItem Feature(string id, string label) => new ProjectStateItem(
        id, "feature", label, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateItem Pbi(string id, string title) => new ProjectStateItem(
        id, "pbi", title, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"));

    private static ProjectStateDocument Core(IReadOnlyList<ProjectStateItem> items, IReadOnlyList<ProjectStateRelation>? rels = null)
        => new("p", 3, DateTime.UnixEpoch, [], [.. items], rels ?? [], [], []);

    private static ProjectStateRelation PartOf(string pbi, string feature)
        => new(pbi, feature, "part_of_feature", "test", new Dictionary<string, string>());

    private static PbiStateChangeOperation NewPbi(string req, string featureId)
        => new("NEW_PBI", req, null, featureId, null, null, $"eigene Einheit für {req}");

    private static PbiStateChangeOperation MarkChanged(string pbiId)
        => new("MARK_CHANGED", "REQ-X", pbiId, null, null, null, "verfeinert");

    private static PbiStateChangeOperation NewFeatureOp(string req, string label)
        => new("NEW_FEATURE", req, null, null, null, null, "eigenständiges neues Thema", ProposedFeatureLabel: label);

    private static PbiAlignment CreateDraft(string req, string title, string statement, IReadOnlyList<string> ak)
        => new(null, title, statement, ak, $"draft {req}", [req], TargetRequirementId: req);

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(1, "p1", DateTime.UnixEpoch, "run", ops, null);

    private static PbiStateChangePlanDocument PlanA(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment>? aligns)
        => new(1, "p1", DateTime.UnixEpoch, "run", ops, aligns);

    private static void SetField(ReviewItem it, string key, string value)
        => it.FieldValues = it.FieldValues.Where(f => f.FieldKey != key).Append(new ReviewFieldValue(key, value)).ToList();

    private static string FieldOf(ReviewItem it, string key)
        => it.FieldValues.FirstOrDefault(f => f.FieldKey == key)?.Value ?? "";

    [Fact]
    public void BuildSession_bietet_alle_bestehenden_Features_als_Dropdown_Optionen()
    {
        var core = Core([Req("REQ-1", "Neue Anforderung"), Feature("FC-01", "Bewohner"), Feature("FC-02", "Medikation")]);
        var session = PbiUpdateReviewAdapter.BuildSession("run", Plan(NewPbi("REQ-1", "FC-01")), core);

        var spec = Assert.Single(session.FieldSchema, f => f.FieldKey == PbiUpdateReviewAdapter.FieldFeature);
        Assert.Equal(ReviewInputType.Dropdown, spec.InputType);
        Assert.NotNull(spec.Options);
        Assert.Equal(["FC-01", "FC-02"], spec.Options!.Select(o => o.Value));
        Assert.Contains(spec.Options, o => o.Label == "FC-01 — Bewohner");
    }

    [Fact]
    public void NewPbi_Item_traegt_vorgeschlagenes_Feature_als_Default_und_den_Kind_Traeger()
    {
        var core = Core([Req("REQ-1", "Neue Anforderung"), Feature("FC-01", "Bewohner")]);
        var session = PbiUpdateReviewAdapter.BuildSession("run", Plan(NewPbi("REQ-1", "FC-01")), core);

        var item = Assert.Single(session.Items);
        Assert.Equal("FC-01", FieldOf(item, PbiUpdateReviewAdapter.FieldFeature));   // Default = Vorschlag
        Assert.Equal(PbiUpdateKind.NewPbi, FieldOf(item, PbiUpdateReviewAdapter.FieldKind)); // steuert VisibleWhen
    }

    [Fact]
    public void Nicht_NewPbi_Item_traegt_keine_Feature_Zuordnung()
    {
        var core = Core([Pbi("PBI-001", "Bestehend"), Feature("FC-01", "Bewohner")]);
        var session = PbiUpdateReviewAdapter.BuildSession("run", Plan(MarkChanged("PBI-001")), core);

        var item = Assert.Single(session.Items);
        Assert.Equal("", FieldOf(item, PbiUpdateReviewAdapter.FieldFeature));
        Assert.Equal(PbiUpdateKind.MarkChanged, FieldOf(item, PbiUpdateReviewAdapter.FieldKind));
    }

    [Fact]
    public void FeatureOverrides_meldet_nur_echt_geaenderte_NewPbi_Zuordnungen()
    {
        var plan = Plan(NewPbi("REQ-1", "FC-01"), NewPbi("REQ-2", "FC-01"), MarkChanged("PBI-001"));
        var decisions = new List<PbiUpdateDecision>
        {
            new("op-0", "apply", null, FeatureId: "FC-02"),   // geändert -> Override
            new("op-1", "apply", null, FeatureId: "FC-01"),   // unverändert -> kein Override
            new("op-2", "apply", null, FeatureId: "FC-09"),   // kein NEW_PBI -> ignoriert
        };

        var overrides = PbiUpdateApplyExec.FeatureOverrides(plan, decisions);

        var o = Assert.Single(overrides);
        Assert.Equal("op-0", o.OpId);
        Assert.Equal("FC-02", o.FeatureId);
    }

    [Fact]
    public void ApplyFeatureOverrides_schreibt_existierendes_Ziel_um_und_ignoriert_unbekanntes()
    {
        var core = Core([Feature("FC-01", "Bewohner"), Feature("FC-02", "Medikation")]);
        var plan = Plan(NewPbi("REQ-1", "FC-01"), NewPbi("REQ-2", "FC-01"));

        var rewritten = PbiUpdateApplyExec.ApplyFeatureOverrides(plan,
            [new PbiFeatureOverride("op-0", "FC-02"), new PbiFeatureOverride("op-1", "FC-99")], core);

        Assert.Equal("FC-02", rewritten.Operations[0].FeatureId);   // existiert -> umgeschrieben
        Assert.Equal("FC-01", rewritten.Operations[1].FeatureId);   // FC-99 unbekannt -> Vorschlag behalten
    }

    [Fact]
    public void RoundTrip_geaendertes_Feld_haengt_das_PBI_ans_gewaehlte_Feature()
    {
        // Session -> Feld ändern -> Adapter.Apply (Decision.FeatureId) -> FeatureOverrides -> Plan-Rewrite -> Core-Apply.
        var core = Core([Req("REQ-1", "Neue Anforderung"), Feature("FC-01", "Bewohner"), Feature("FC-02", "Medikation")]);
        var plan = Plan(NewPbi("REQ-1", "FC-01"));
        var session = PbiUpdateReviewAdapter.BuildSession("run", plan, core);

        var item = session.Items[0];
        SetField(item, PbiUpdateReviewAdapter.FieldDecision, "apply");
        SetField(item, PbiUpdateReviewAdapter.FieldFeature, "FC-02");   // Mensch korrigiert die Zuordnung

        var decisions = PbiUpdateReviewAdapter.Apply("run", session);
        Assert.Equal("FC-02", Assert.Single(decisions.Decisions).FeatureId);

        var overrides = PbiUpdateApplyExec.FeatureOverrides(plan, decisions.Decisions);
        var rewritten = PbiUpdateApplyExec.ApplyFeatureOverrides(plan, overrides, core);
        var (result, report) = PbiUpdateApply.Apply(core, rewritten, new HashSet<int> { 0 }, "run");

        var pbiId = Assert.Single(report.NewPbis);
        Assert.Contains(result.Relations, r => r.FromId == pbiId && r.ToId == "FC-02" && r.RelationType == "part_of_feature");
        Assert.DoesNotContain(result.Relations, r => r.FromId == pbiId && r.ToId == "FC-01");
    }

    [Fact]
    public void ResolveReference_listet_die_PBIs_eines_Features_und_null_bei_unbekannt()
    {
        var core = Core(
            [Feature("FC-01", "Bewohner-Verwaltung"), Pbi("PBI-001", "Stammdaten"), Pbi("PBI-002", "Kontakte")],
            [PartOf("PBI-001", "FC-01"), PartOf("PBI-002", "FC-01")]);

        var details = PbiUpdateReviewAdapter.ResolveReference("FC-01", core);
        Assert.NotNull(details);
        Assert.Equal("Bewohner-Verwaltung", details!.Title);
        Assert.Contains(details.Notes, n => n.Text == "Stammdaten");
        Assert.Contains(details.Notes, n => n.Text == "Kontakte");

        Assert.Null(PbiUpdateReviewAdapter.ResolveReference("FC-99", core));
    }

    // ---- B2: auch vorgeschlagene NEUE Features (NEW_FEATURE-Ops desselben Plans) wählbar → Kopplung ----

    [Fact]
    public void BuildSession_bietet_auch_die_im_Plan_vorgeschlagenen_neuen_Features()
    {
        var core = Core([Req("REQ-1", "X"), Req("REQ-2", "Y"), Feature("FC-01", "Bewohner")]);
        var plan = PlanA([NewPbi("REQ-1", "FC-01"), NewFeatureOp("REQ-2", "Medikationsbereich")], null);
        var session = PbiUpdateReviewAdapter.BuildSession("run", plan, core);

        var opts = session.FieldSchema.Single(f => f.FieldKey == PbiUpdateReviewAdapter.FieldFeature).Options!;
        Assert.Contains(opts, o => o.Value == "FC-01");                                              // bestehend
        Assert.Contains(opts, o => o.Value == PbiUpdateReviewAdapter.ProposedPrefix + "Medikationsbereich"); // vorgeschlagen
        Assert.Contains(opts, o => o.Label == "🆕 NEU: Medikationsbereich");
    }

    [Fact]
    public void FeatureOverrides_erkennt_ein_vorgeschlagenes_neues_Feature_als_Ziel()
    {
        var plan = PlanA([NewPbi("REQ-1", "FC-01"), NewFeatureOp("REQ-2", "Medikationsbereich")], null);
        var decisions = new List<PbiUpdateDecision>
        {
            new("op-0", "apply", null, FeatureId: PbiUpdateReviewAdapter.ProposedPrefix + "Medikationsbereich"),
        };

        var o = Assert.Single(PbiUpdateApplyExec.FeatureOverrides(plan, decisions));
        Assert.Equal("op-0", o.OpId);
        Assert.Null(o.FeatureId);
        Assert.Equal("Medikationsbereich", o.ProposedFeatureLabel);
    }

    [Fact]
    public void ApplyFeatureOverrides_konvertiert_zu_NewFeature_nur_bei_bekanntem_Label()
    {
        var core = Core([Feature("FC-01", "Bewohner")]);
        var plan = PlanA([NewPbi("REQ-1", "FC-01"), NewFeatureOp("REQ-2", "Medikationsbereich")], null);

        var rewritten = PbiUpdateApplyExec.ApplyFeatureOverrides(plan,
            [new PbiFeatureOverride("op-0", ProposedFeatureLabel: "Medikationsbereich")], core);
        Assert.Equal(PbiUpdateKind.NewFeature, rewritten.Operations[0].Kind);   // konvertiert
        Assert.Equal("Medikationsbereich", rewritten.Operations[0].ProposedFeatureLabel);
        Assert.Null(rewritten.Operations[0].FeatureId);

        var keep = PbiUpdateApplyExec.ApplyFeatureOverrides(plan,
            [new PbiFeatureOverride("op-0", ProposedFeatureLabel: "Gibtsnicht")], core);
        Assert.Equal(PbiUpdateKind.NewPbi, keep.Operations[0].Kind);            // unbekanntes Label -> Vorschlag behalten
    }

    [Fact]
    public void ResolveReference_zeigt_die_geplanten_Anforderungen_eines_vorgeschlagenen_Features()
    {
        var core = Core([Req("REQ-2", "Medikamenten-Übersicht pro Bewohner")]);
        var plan = PlanA([NewFeatureOp("REQ-2", "Medikationsbereich")], null);

        var details = PbiUpdateReviewAdapter.ResolveReference(PbiUpdateReviewAdapter.ProposedPrefix + "Medikationsbereich", core, plan);
        Assert.NotNull(details);
        Assert.Contains("Medikationsbereich", details!.Title);
        Assert.Contains(details.Notes, n => n.Text.Contains("Medikamenten-Übersicht"));

        // Ohne Plan kann ein proposed-Ziel nicht aufgelöst werden.
        Assert.Null(PbiUpdateReviewAdapter.ResolveReference(PbiUpdateReviewAdapter.ProposedPrefix + "X", core));
    }

    [Fact]
    public void RoundTrip_B2_koppelt_das_NewPbi_in_das_vorgeschlagene_neue_Feature()
    {
        // Der eigentliche Fix „3/4 in FC-12 statt FC-14": ein NEW_FEATURE + ein zunächst falsch als NEW_PBI (FC-01)
        // platziertes Requirement. Der Mensch hängt das NEW_PBI ins vorgeschlagene neue Feature -> EIN Feature, ZWEI PBIs.
        var core = Core([Req("REQ-A", "Med-Übersicht"), Req("REQ-B", "Med-Erinnerung"), Feature("FC-01", "Bewohner")]);
        var dA = CreateDraft("REQ-A", "Übersicht der Medikamente", "Statement A", ["AK-A"]);
        var dB = CreateDraft("REQ-B", "Erinnerung an Gaben", "Statement B", ["AK-B"]);
        var plan = PlanA([NewFeatureOp("REQ-A", "Medikationsbereich"), NewPbi("REQ-B", "FC-01")], [dA, dB]);

        var overrides = new List<PbiFeatureOverride> { new("op-1", ProposedFeatureLabel: "Medikationsbereich") };
        var rewritten = PbiUpdateApplyExec.ApplyFeatureOverrides(plan, overrides, core);
        Assert.Equal(PbiUpdateKind.NewFeature, rewritten.Operations[1].Kind);   // NEW_PBI -> NEW_FEATURE konvertiert

        var (result, report) = PbiUpdateApply.Apply(core, rewritten, new HashSet<int> { 0, 1 }, "run", [dA, dB]);

        // GENAU EIN neues Feature (Label-Gruppierung greift), mit ZWEI PBIs.
        var feature = Assert.Single(result.Items, i => i.ItemType == "feature" && i.ItemId != "FC-01");
        Assert.Equal("Medikationsbereich", feature.Feature!.Label);
        Assert.Equal(2, report.NewPbis.Count);
        foreach (var pid in report.NewPbis)
            Assert.Contains(result.Relations, r => r.FromId == pid && r.ToId == feature.ItemId && r.RelationType == "part_of_feature");

        // FC-01 blieb unberührt — das NEW_PBI wurde NICHT dort angelegt.
        Assert.DoesNotContain(result.Relations, r => r.ToId == "FC-01" && r.RelationType == "part_of_feature");
    }
}
