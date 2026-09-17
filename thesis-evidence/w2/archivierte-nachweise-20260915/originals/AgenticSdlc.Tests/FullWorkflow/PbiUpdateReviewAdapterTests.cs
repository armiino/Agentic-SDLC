using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Adapter-Roundtrip (Placement-Stufe): Backlog-Aenderungs-Entscheidungen verlustfrei tragen.
// E0.3 (27.07., R-20): Items starten OHNE Vorentscheid; Begruendung nur beim skip Pflicht; Anreicherung
// loest IDs zu Text auf und zeigt Vorher/Nachher aus der Core-History.
public sealed class PbiUpdateReviewAdapterTests
{
    private static readonly ProjectStateDocument EmptyCore =
        new("p", 3, DateTime.UnixEpoch, [], [], [], [], []);

    private static PbiStateChangeOperation Op(string kind = "EXTEND_PBI", string req = "REQ-1", string? pbi = "PBI-1")
        => new(Kind: kind, RequirementId: req, PbiId: pbi, FeatureId: null,
               ReplacementRequirementId: null, OpenDecisionRef: null, Rationale: "weil");

    private static PbiStateChangePlanDocument Plan(params PbiStateChangeOperation[] ops)
        => new(SchemaVersion: 1, PlanId: "p1", CreatedUtc: DateTime.UnixEpoch, SourceIngestionRun: "src", Operations: ops);

    private static ProjectStateItem Req(string id, string text, string? prev = null) => new ProjectStateItem(
        id, "requirement", text, "test", null, 2,
        null, null, null, null, null, [], [], new Dictionary<string, string>(),
        History: prev is null ? null : [new ProjectStateItemVersion(1, prev, "accepted", "test", null, [], DateTime.UnixEpoch, null)]).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateItem PbiItem(string id, string title, string status = "needs_clarify") => new ProjectStateItem(
        id, "pbi", title, "test", null, 1,
        null, null, null, null, null, [], [], new Dictionary<string, string>(),
        Pbi: new PbiPayload(Goal: "Als Team will ich …", Title: title, AcceptanceCriteria: ["AK 1"], LinkedRequirementIds: [],
            OpenDecisionRefs: [], PriorityRank: null, Readiness: "backlog_ready", Mvp: null, Trace: null)).WithStatus(CoreStatus.From(status));

    private static string FieldOf(ReviewItem it, string key) => it.FieldValues.First(f => f.FieldKey == key).Value ?? "";

    private static void Set(ReviewItem it, string key, string value)
    {
        it.FieldValues.RemoveAll(f => f.FieldKey == key);
        it.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void BuildSession_erzeugt_Items_ohne_Vorentscheid()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op("NEW_PBI", "REQ-2", pbi: null)), EmptyCore);
        Assert.Equal(["op-0", "op-1"], session.Items.Select(i => i.ItemId));
        Assert.All(session.Items, i => Assert.Equal("", FieldOf(i, PbiUpdateReviewAdapter.FieldDecision)));
        Assert.All(session.Items, i => Assert.False(i.Resolved));
    }

    [Fact]
    public void ExtendPbi_zeigt_den_Angleichungs_Draft_Block()   // O3a
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [], [PbiItem("PBI-1", "Alter Titel")], [], [], []);
        var align = new PbiAlignment("PBI-1", "Titel inkl. Export", null, ["AK: Export als PDF"], "um REQ-2 erweitert", ["REQ-2"]);
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "src", [Op("EXTEND_PBI", "REQ-2", "PBI-1")], [align]);

        var it = PbiUpdateReviewAdapter.BuildSession("r1", plan, core).Items[0];

        // O3a: EXTEND_PBI muss ebenfalls den Angleichungsblock tragen (frueher nur MARK_CHANGED/SUPERSEDE).
        Assert.Equal("yes", FieldOf(it, PbiUpdateReviewAdapter.FieldHasAlign));
        Assert.Equal("PBI-1", FieldOf(it, PbiUpdateReviewAdapter.FieldAlignPbiId));
    }

    [Fact]
    public void NewPbi_mit_Create_Draft_zeigt_den_Draft_Block()   // O3b
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [new ProjectStateItem("FC-01", "feature", "Export", "test", null, 1,
                null, null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("active"))], [], [], []);
        var draft = new PbiAlignment(null, "PDF-Export", "Als X will ich Daten exportieren", ["Export als PDF"],
            "neues PBI aus REQ-9", ["REQ-9"], TargetRequirementId: "REQ-9", TargetFeatureId: "FC-01");
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "src",
            [Op("NEW_PBI", "REQ-9", pbi: null)], [draft]);

        var it = PbiUpdateReviewAdapter.BuildSession("r1", plan, core).Items[0];

        // O3b: NEW_PBI trägt den create-Draft; der DraftKey ist die Ziel-Requirement (kein PbiId).
        Assert.Equal("yes", FieldOf(it, PbiUpdateReviewAdapter.FieldHasAlign));
        Assert.Equal("REQ-9", FieldOf(it, PbiUpdateReviewAdapter.FieldAlignPbiId));
        // O3b-Polish (Gate-Sinn): der vorgeschlagene neue PBI-Inhalt MUSS sichtbar sein, auch ohne bestehendes PBI.
        Assert.Contains(it.Notes, n => n.Label.Contains("Neues PBI") && n.Text.Contains("PDF-Export"));
    }

    [Fact]
    public void NewFeature_zeigt_neues_Feature_Label_und_den_PBI_Draft_Block()   // O4b
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [], [Req("REQ-9", "Übergabe-Notiz pro Schicht")], [], [], []);
        var draft = new PbiAlignment(null, "Schicht-Übergabe-Notiz", "Als Pflegekraft will ich …", ["AK: Notiz je Schicht"],
            "neues Feature-PBI", ["REQ-9"], TargetRequirementId: "REQ-9");
        var op = new PbiStateChangeOperation("NEW_FEATURE", "REQ-9", null, null, null, null, "neues Thema",
            ProposedFeatureLabel: "Schichtübergabe");
        var plan = new PbiStateChangePlanDocument(1, "p1", DateTime.UnixEpoch, "src", [op], [draft]);

        var it = PbiUpdateReviewAdapter.BuildSession("r1", plan, core).Items[0];

        // Gate-Sinn: der Mensch sieht das neue Feature-Label UND den PBI-Draft, bevor er es in den Core übernimmt.
        Assert.Equal("yes", FieldOf(it, PbiUpdateReviewAdapter.FieldHasAlign));
        Assert.Equal("REQ-9", FieldOf(it, PbiUpdateReviewAdapter.FieldAlignPbiId));
        Assert.Contains(it.Notes, n => n.Text.Contains("Schichtübergabe"));
        Assert.Contains(it.Notes, n => n.Label.Contains("Neues PBI") && n.Text.Contains("Schicht-Übergabe-Notiz"));
    }

    [Fact]
    public void Resolved_apply_ohne_Begruendung_skip_nur_mit()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        var it = session.Items[0];

        Set(it, PbiUpdateReviewAdapter.FieldDecision, "apply");
        Assert.True(PbiUpdateReviewAdapter.Resolved(it));

        Set(it, PbiUpdateReviewAdapter.FieldDecision, "skip");
        Assert.False(PbiUpdateReviewAdapter.Resolved(it));
        Set(it, PbiUpdateReviewAdapter.FieldReason, "bewusst nicht uebernehmen");
        Assert.True(PbiUpdateReviewAdapter.Resolved(it));
    }

    [Fact]
    public void Apply_und_Merge_sind_ein_verlustfreier_Roundtrip()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        Set(session.Items[0], PbiUpdateReviewAdapter.FieldDecision, "apply");
        Set(session.Items[1], PbiUpdateReviewAdapter.FieldDecision, "skip");
        Set(session.Items[1], PbiUpdateReviewAdapter.FieldReason, "Duplikat");

        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        Assert.Equal(["apply", "skip"], file.Decisions.Select(d => d.Decision));

        var fresh = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op(), Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(fresh, file);
        Assert.Equal("skip", FieldOf(fresh.Items[1], PbiUpdateReviewAdapter.FieldDecision));
        Assert.True(fresh.Items[1].Resolved);
    }

    [Fact]
    public void Unbekannte_OpIds_in_der_Datei_werden_ignoriert()
    {
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op()), EmptyCore);
        PbiUpdateReviewAdapter.MergeExistingDecisions(session,
            new PbiUpdateDecisionsFile("r1", "x", [new PbiUpdateDecision("op-99", "skip", null)]));
        Assert.Equal("", FieldOf(session.Items[0], PbiUpdateReviewAdapter.FieldDecision));
    }

    // E0.3: MARK_CHANGED löst IDs zu Text auf, zeigt Vorher/Nachher aus der History und warnt vor needs_clarify.
    [Fact]
    public void MarkChanged_zeigt_Text_VorherNachher_und_needs_clarify_Warnung()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [Req("REQ-1", "Archivierung nach vierzehn Tagen", prev: "Archivierung nach sieben Tagen"),
             PbiItem("PBI-1", "Automatische Archivierung")], [], [], []);

        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op("MARK_CHANGED")), core);
        var notes = session.Items[0].Notes;

        Assert.Contains(notes, n => n.Label.StartsWith("Betroffenes PBI") && n.Text.Contains("Automatische Archivierung"));
        var diff = Assert.Single(notes, n => n.Label.Contains("Vorher/Nachher"));
        Assert.Contains("− Archivierung nach sieben Tagen", diff.Text);
        Assert.Contains("+ Archivierung nach vierzehn Tagen", diff.Text);
        Assert.Contains(notes, n => n.Label == "Danach: geändert · ungeklärt" && n.Kind == ReviewNoteKind.Warning && n.Text.Contains("ungeklaert"));
        // MARK_CHANGED ist deterministisch -> die Leersatz-Rationale wird NICHT als Platzierungs-Begruendung gezeigt.
        Assert.DoesNotContain(notes, n => n.Label.StartsWith("Warum diese Zuordnung"));
    }

    // E0.3-Retrofit: Badge + Wirkung sind Klartext (kein rohes Op-Enum), Glossar ist gruppiert.
    [Fact]
    public void Klartext_Badge_Wirkung_und_gruppiertes_Glossar()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [Req("REQ-1", "X"), PbiItem("PBI-1", "Titel")], [], [], []);
        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(Op("MARK_CHANGED")), core);
        var it = session.Items[0];

        Assert.Equal("Als geändert markieren", it.Badge);           // nicht "MARK_CHANGED"
        Assert.Contains(it.Notes, n => n.Label == "Wirkung" && n.Text.Contains("geändert · ungeklärt"));
        Assert.Equal(["apply", "skip"], it.FieldOptions[PbiUpdateReviewAdapter.FieldDecision].Select(o => o.Value));
        Assert.All(session.Glossary, g => Assert.False(string.IsNullOrEmpty(g.Group)));   // alle Begriffe kategorisiert
    }

    // E0.3: bei EXTEND/NEW (agentische Platzierung) wird die Rationale als handlungsleitende Begruendung gezeigt.
    [Fact]
    public void Extend_zeigt_agentische_Platzierungs_Begruendung()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [],
            [Req("REQ-65", "Zugriffsprotokoll nur fuer Admins"), PbiItem("PBI-2", "Interne Accounts")], [], [], []);
        var op = new PbiStateChangeOperation("EXTEND_PBI", "REQ-65", "PBI-2", null, null, null,
            "REQ-65 ist eine Rechte-Vorgabe und passt zum bestehenden Rollen-PBI statt zu einem neuen.");

        var session = PbiUpdateReviewAdapter.BuildSession("r1", Plan(op), core);
        Assert.Contains(session.Items[0].Notes,
            n => n.Label.StartsWith("Warum diese Zuordnung") && n.Text.Contains("Rechte-Vorgabe"));
    }

    private static PbiStateChangePlanDocument PlanWithAlign(IReadOnlyList<PbiStateChangeOperation> ops, IReadOnlyList<PbiAlignment> aligns)
        => new(1, "p1", DateTime.UnixEpoch, "src", ops, aligns);

    // R-26-C: die Angleichung wird DIREKT am zugehörigen MARK_CHANGED-Op-Item eingebettet (gleicher Kontext),
    // die Edit-Felder mit dem Vorschlag vorbefüllt.
    [Fact]
    public void Alignment_wird_am_MarkChanged_Op_eingebettet()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [], [PbiItem("PBI-1", "Alter Titel")], [], [], []);
        var align = new PbiAlignment("PBI-1", "Neuer Titel", "Als X …", ["AK neu"], "an REQ-1 angeglichen", ["REQ-1"]);
        var session = PbiUpdateReviewAdapter.BuildSession("r1", PlanWithAlign([Op("MARK_CHANGED")], [align]), core);

        var op = Assert.Single(session.Items);  // EIN Item — Struktur + Angleichung zusammen
        Assert.Equal("yes", FieldOf(op, PbiUpdateReviewAdapter.FieldHasAlign));
        Assert.Equal("PBI-1", FieldOf(op, PbiUpdateReviewAdapter.FieldAlignPbiId));
        Assert.Equal("Neuer Titel", FieldOf(op, PbiUpdateReviewAdapter.FieldAlignTitle));  // vorbefüllt
        Assert.Contains(op.Notes, n => n.Label.Contains("AKTUELL") && n.Text.Contains("Alter Titel"));
        Assert.Contains(op.Notes, n => n.Label.Contains("ANGEGLICHEN") && n.Text.Contains("Neuer Titel"));
        Assert.False(op.Resolved);
    }

    // R-26-C — der "nicht heimlich vermischen"-Kontrakt: Struktur und Angleichung sind UNABHÄNGIGE Felder.
    // Angleichung startet auf 'accept' (Autor-Wunsch: Vorschlag ist der Normalfall); Struktur hat KEINEN Default
    // (Durchwink-Schutz). Struktur apply + Angleichung skip ist möglich — der Apply trennt beide sauber.
    [Fact]
    public void Struktur_und_Angleichung_sind_unabhaengig()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [], [PbiItem("PBI-1", "Alter Titel")], [], [], []);
        var align = new PbiAlignment("PBI-1", "Neuer Titel", null, null, "x", ["REQ-1"]);
        var session = PbiUpdateReviewAdapter.BuildSession("r1", PlanWithAlign([Op("MARK_CHANGED")], [align]), core);
        var op = session.Items[0];

        Assert.Equal("accept", FieldOf(op, PbiUpdateReviewAdapter.FieldAlignDecision)); // Default Übernehmen
        Assert.Equal("", FieldOf(op, PbiUpdateReviewAdapter.FieldDecision));            // Struktur kein Default
        Assert.False(op.Resolved);                                                     // Struktur noch offen

        Set(op, PbiUpdateReviewAdapter.FieldDecision, "apply");
        Assert.True(PbiUpdateReviewAdapter.Resolved(op));                              // beide entschieden (Angleichung default accept)

        // Angleichung unabhängig auf skip drehen — Struktur bleibt apply, Apply trennt sauber.
        Set(op, PbiUpdateReviewAdapter.FieldAlignDecision, "skip");
        Set(op, PbiUpdateReviewAdapter.FieldReason, "erst später klären");
        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        Assert.Equal("apply", file.Decisions[0].Decision);
        Assert.Equal("skip", file.AlignmentDecisions![0].Decision);
    }

    // R-26-C: Roundtrip — beide Entscheidungen am selben Item; Apply trennt sie sauber, Merge stellt sie wieder her.
    [Fact]
    public void Alignment_Apply_und_Merge_Roundtrip()
    {
        var core = new ProjectStateDocument("p", 3, DateTime.UnixEpoch, [], [PbiItem("PBI-1", "Alter Titel")], [], [], []);
        var align = new PbiAlignment("PBI-1", "Neuer Titel", null, null, "angeglichen", ["REQ-1"]);
        var plan = PlanWithAlign([Op("MARK_CHANGED")], [align]);
        var session = PbiUpdateReviewAdapter.BuildSession("r1", plan, core);

        var op = session.Items[0];
        Set(op, PbiUpdateReviewAdapter.FieldDecision, "apply");
        Set(op, PbiUpdateReviewAdapter.FieldAlignDecision, "accept");

        var file = PbiUpdateReviewAdapter.Apply("r1", session);
        Assert.Equal("apply", file.Decisions[0].Decision);
        var ad = Assert.Single(file.AlignmentDecisions!);
        Assert.Equal("PBI-1", ad.PbiId);
        Assert.Equal("accept", ad.Decision);

        var fresh = PbiUpdateReviewAdapter.BuildSession("r1", plan, core);
        PbiUpdateReviewAdapter.MergeExistingDecisions(fresh, file);
        var merged = fresh.Items[0];
        Assert.Equal("apply", FieldOf(merged, PbiUpdateReviewAdapter.FieldDecision));
        Assert.Equal("accept", FieldOf(merged, PbiUpdateReviewAdapter.FieldAlignDecision));
        Assert.True(merged.Resolved);
    }
}
