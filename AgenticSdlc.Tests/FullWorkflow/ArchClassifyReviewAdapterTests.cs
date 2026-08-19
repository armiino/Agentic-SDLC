using AgenticSdlc.Host.FullWorkflow.ArchClassify;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A2-3 (06.08.): die U2-Review-UI — Vorbelegung aus dem Agent-Vorschlag (Korrektur-Modus),
// ja/nein-Rollen-Roundtrip, alle-nein = vertagt (Item fehlt in der Entscheid-Datei), Merge + ToResponse.
public sealed class ArchClassifyReviewAdapterTests
{
    private static ArchClassifyReviewRequest Request() => new("run-1",
    [
        new("ARCH-1", "Firestore als Datenbank", [ArchRoles.Design, ArchRoles.Constraint], "Technologie-Festlegung",
            [new ArchPbiOption("PBI-3", "Datenzugriff kapseln")]),
        new("ARCH-2", "Migrationsskript nötig", [ArchRoles.Work], "echte Arbeit")
    ], ActivePbis: [new("PBI-3", "Datenzugriff kapseln"), new("PBI-4", "Login bauen")]);

    private static void Set(HumanReview.ReviewItem item, string key, string value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new HumanReview.ReviewFieldValue(key, value));
    }

    [Fact]
    public void Session_ist_vorbelegt_aber_uebernehmen_ist_ein_expliziter_Akt()
    {
        var s = ArchClassifyReviewAdapter.BuildSession("run-1", Request());

        var a1 = s.Items.Single(i => i.ItemId == "ARCH-1");
        Assert.Equal("ja", a1.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldConstraint).Value);
        Assert.Equal("nein", a1.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldWork).Value);
        Assert.Equal("ja", a1.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldDesign).Value);
        // 1f-① (Vertragswechsel, Block-E-Fund „Durchwink-Falle"): die Vorbelegung ist nur der VORSCHLAG —
        // resolved wird ein Item erst durch den expliziten Übernehmen-Akt (leerer Default; E0).
        Assert.False(a1.Resolved);
        Assert.Equal("", a1.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldUebernehmen).Value);
        Set(a1, ArchClassifyReviewAdapter.FieldUebernehmen, ArchClassifyReviewAdapter.Ja);
        Assert.True(ArchClassifyReviewAdapter.Resolved(a1));
        // Der EINE deklarierte Bulk bestätigt (setzt NUR uebernehmen) — die Rollen kommen je Item aus der Vorbelegung.
        Assert.Equal(ArchClassifyReviewAdapter.FieldUebernehmen, s.BulkAction!.Set.Single().FieldKey);
        Assert.Equal("design+constraint", a1.Badge);
        // U2v2 Ziel-Kette: Chips-Feld vorbelegt mit den Vorschlags-IDs; KEINE Extra-Note bei constraint
        // (die Chips zeigen es) und KEIN Kontext-Button mehr (rechts ist die PBI-Liste).
        Assert.Equal("PBI-3", a1.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets).Value);
        Assert.Empty(a1.ContextBlocks);
        Assert.DoesNotContain(a1.Notes, n => n.Label.Contains("Betroffene PBIs"));
        Assert.Empty(s.Items.Single(i => i.ItemId == "ARCH-2").FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets).Value ?? "");

        // Das Ziel-Feld ist ein ReferenceList-Feld: Options = Katalog der rechten Liste, nur bei constraint=ja sichtbar.
        var targets = s.FieldSchema.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets);
        Assert.Equal(HumanReview.ReviewInputType.ReferenceList, targets.InputType);
        Assert.Equal(2, targets.Options!.Count);
        Assert.Equal("PBI-3 — Datenzugriff kapseln", targets.Options[0].Label);
        Assert.Equal("Aktive PBIs", targets.CatalogTitle);
        Assert.Equal((ArchClassifyReviewAdapter.FieldConstraint, "ja"), (targets.VisibleWhen!.FieldKey, targets.VisibleWhen.Values.Single()));
    }

    [Fact]
    public void Work_ohne_constraint_zeigt_Zuordnung_als_Anzeige_Note()
    {
        var s = ArchClassifyReviewAdapter.BuildSession("run-1", new ArchClassifyReviewRequest("run-1",
            [new("ARCH-3", "Migrationsskript", [ArchRoles.Work], "arbeit", [new ArchPbiOption("PBI-4", "Login bauen")])]));
        Assert.Contains(s.Items.Single().Notes, n => n.Label.StartsWith("Gehört fachlich zu") && n.Text.Contains("PBI-4 — Login bauen"));
    }

    [Fact]
    public void Korrektur_und_Vertagen_landen_korrekt_in_der_Entscheid_Datei()
    {
        var s = ArchClassifyReviewAdapter.BuildSession("run-1", Request());
        Set(s.Items[0], ArchClassifyReviewAdapter.FieldWork, "ja");        // Mensch ergänzt work
        Set(s.Items[0], ArchClassifyReviewAdapter.FieldTargets, "PBI-3\nPBI-4");   // Mensch ergänzt ein Ziel
        Set(s.Items[0], ArchClassifyReviewAdapter.FieldUebernehmen, "ja"); // 1f-①: der explizite Akt
        Set(s.Items[1], ArchClassifyReviewAdapter.FieldWork, "nein");      // Mensch vertagt ARCH-2 (alle nein)
        Set(s.Items[1], ArchClassifyReviewAdapter.FieldUebernehmen, "ja"); // bestätigt + alle-nein = trotzdem vertagt

        var file = ArchClassifyReviewAdapter.Apply("run-1", s);

        var d1 = Assert.Single(file.Decisions);                            // ARCH-2 fehlt = vertagt
        Assert.Equal("ARCH-1", d1.ItemId);
        Assert.Equal([ArchRoles.Constraint, ArchRoles.Work, ArchRoles.Design], d1.Roles);
        Assert.Equal(["PBI-3", "PBI-4"], d1.TargetPbiIds);                 // ① korrigierte Ziele in der Datei

        var resp = ArchClassifyReviewAdapter.ToResponse(file, "test");
        Assert.Single(resp.Accepted);
        Assert.Equal("ARCH-1", resp.Accepted[0].ItemId);
        Assert.Equal(["PBI-3", "PBI-4"], resp.Accepted[0].TargetPbiIds);   // ① ...und in der Gate-Antwort
    }

    [Fact]
    public void Merge_stellt_fruehere_Entscheide_wieder_her()
    {
        var s = ArchClassifyReviewAdapter.BuildSession("run-1", Request());
        var earlier = new ArchClassifyDecisionsFile("run-1", [new("ARCH-2", [ArchRoles.Design], "doch design", ["PBI-4"])]);

        ArchClassifyReviewAdapter.MergeExistingDecisions(s, earlier);

        var a2 = s.Items.Single(i => i.ItemId == "ARCH-2");
        Assert.Equal("nein", a2.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldWork).Value);
        Assert.Equal("ja", a2.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldDesign).Value);
        Assert.Equal("doch design", a2.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldRationale).Value);
        Assert.Equal("PBI-4", a2.FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets).Value);
        // 1f-①: ein Datei-Eintrag IST eine frühere Übernahme (Apply schreibt nur Bestätigtes) → resolved.
        Assert.True(a2.Resolved);
        Assert.False(s.Items.Single(i => i.ItemId == "ARCH-1").Resolved);   // unentschieden bleibt unentschieden
    }

    [Fact]
    public void Merge_ohne_Ziel_Wissen_loescht_die_Vorbelegung_NICHT()
    {
        // Alt-Datei (vor ①): TargetPbiIds fehlt (null) — der System-Vorschlag im Ziel-Feld muss überleben.
        var s = ArchClassifyReviewAdapter.BuildSession("run-1", Request());
        ArchClassifyReviewAdapter.MergeExistingDecisions(s, new ArchClassifyDecisionsFile("run-1",
            [new("ARCH-1", [ArchRoles.Constraint], "alt")]));
        Assert.Equal("PBI-3", s.Items.Single(i => i.ItemId == "ARCH-1")
            .FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets).Value);

        // Bewusst geleert (leere Liste = echter Human-Stand) — Leere gewinnt.
        ArchClassifyReviewAdapter.MergeExistingDecisions(s, new ArchClassifyDecisionsFile("run-1",
            [new("ARCH-1", [ArchRoles.Constraint], "alt", [])]));
        Assert.Equal("", s.Items.Single(i => i.ItemId == "ARCH-1")
            .FieldValues.Single(f => f.FieldKey == ArchClassifyReviewAdapter.FieldTargets).Value);
    }

    // ---- U2v2: PBI-Detail fürs rechte Panel (pure Funktion am Mini-Core) ----

    private static ProjectStateItem Item(string id, string type, string text) =>
        new ProjectStateItem(
            id, type, text, "MEETING", null, 1, "r", null, null, null, null, ["SL-1"], [],
            new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument MiniCore()
    {
        var pbi = Item("PBI-1", "pbi", "PBI-Text") with
        {
            Pbi = new PbiPayload(
                "Ziel des PBI", "Bewohnerprofil", ["AK 1", "AK 2"], [], [], null, null, null, null)
        };
        return new("p", 4, DateTime.UnixEpoch, [],
            [pbi, Item("FC-1", "feature", "Profil-Feature"), Item("REQ-1", "requirement", "Req-Text"), Item("ARCH-9", "architecture", "Rahmen-Text")],
            [
                new("PBI-1", "FC-1", "part_of_feature", "t", new Dictionary<string, string>()),
                new("PBI-1", "REQ-1", "covers", "t", new Dictionary<string, string>()),
                new("PBI-1", "ARCH-9", "constrained_by", "t", new Dictionary<string, string>())
            ], [], []);
    }

    [Fact]
    public void PbiReferenz_liefert_Titel_Feature_und_lazy_Kontexte()
    {
        var core = MiniCore();
        var d = ArchClassifyReviewAdapter.BuildPbiReference(core, "PBI-1")!;
        Assert.Equal("PBI-1 — Bewohnerprofil", d.Title);
        Assert.Equal("Ziel des PBI", d.Summary);
        Assert.Contains(d.Notes, n => n.Label == "Feature" && n.Text == "Profil-Feature");
        Assert.Equal(["aks", "reqs", "archs"], d.ContextBlocks.Select(c => c.ResolverKey).ToList());

        Assert.Contains("AK 1", ArchClassifyReviewAdapter.ResolvePbiReferenceContext(core, "PBI-1", "aks"));
        Assert.Contains("REQ-1 — Req-Text", ArchClassifyReviewAdapter.ResolvePbiReferenceContext(core, "PBI-1", "reqs"));
        Assert.Contains("ARCH-9 — Rahmen-Text", ArchClassifyReviewAdapter.ResolvePbiReferenceContext(core, "PBI-1", "archs"));
        Assert.Null(ArchClassifyReviewAdapter.BuildPbiReference(core, "PBI-99"));
    }
}
