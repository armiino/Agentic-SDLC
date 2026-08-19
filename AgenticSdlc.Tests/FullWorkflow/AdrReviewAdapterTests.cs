using AgenticSdlc.Host.FullWorkflow.Adr;
using AgenticSdlc.Host.FullWorkflow.ArchClassify;
using AgenticSdlc.HumanReview;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-11 A5/U5 (06.08.): die ADR-Abnahme-UI — Vorbelegung (Korrektur-Modus), Related als ReferenceList
// (U2v2-Wiederverwendung), freigeben=nein = vertagt, Edit-Roundtrip in die Entscheid-Datei, lazy Vorschau.
public sealed class AdrReviewAdapterTests
{
    private static AdrReviewRequest Request() => new("run-1",
    [
        new(new AdrDraft("ARCH-39", "Firestore als Datenbank", "Kontext.", "Wir nutzen Firestore.", "Folgen.", ["REQ-70"]),
            "Firestore ist gesetzt", "# ADR-XXXX: Firestore als Datenbank\n..."),
        new(new AdrDraft("ARCH-40", "Zweite Entscheidung", "K.", "E.", "F."), "Zweiter Fakt", "# ADR-XXXX: Zweite...")
    ], Truth: [new ArchPbiOption("REQ-70", "Firestore festgelegt"), new ArchPbiOption("ARCH-39", "Firestore ist gesetzt")]);

    private static void Set(ReviewItem item, string key, string value)
    {
        item.FieldValues.RemoveAll(f => f.FieldKey == key);
        item.FieldValues.Add(new ReviewFieldValue(key, value));
    }

    [Fact]
    public void Session_ist_vorbelegt_mit_leerem_Entscheid_ReferenceList_und_Bulk()
    {
        var s = AdrReviewAdapter.BuildSession("run-1", Request());

        var a = s.Items.Single(i => i.ItemId == "ARCH-39");
        // E0-Leere-Default: Prosa vorbelegt, der ENTSCHEID startet leer (Freigabe = bewusster Akt/Sammel-Knopf).
        Assert.False(a.Resolved);
        Assert.Equal("", a.FieldValues.Single(f => f.FieldKey == AdrReviewAdapter.FieldFreigeben).Value);
        Assert.NotNull(s.BulkAction);
        Assert.Equal(AdrReviewAdapter.FieldFreigeben, s.BulkAction!.Set.Single().FieldKey);
        Assert.Equal("Firestore als Datenbank", a.FieldValues.Single(f => f.FieldKey == AdrReviewAdapter.FieldTitel).Value);
        Assert.Equal("REQ-70", a.FieldValues.Single(f => f.FieldKey == AdrReviewAdapter.FieldRelated).Value);

        var related = s.FieldSchema.Single(f => f.FieldKey == AdrReviewAdapter.FieldRelated);
        Assert.Equal(ReviewInputType.ReferenceList, related.InputType);
        Assert.Equal("Core-Items", related.CatalogTitle);
        Assert.Equal(2, related.Options!.Count);
        Assert.Equal("vorschau:ARCH-39", a.ContextBlocks.Single().ResolverKey);

        // 1c-② (18.08./19.08., Rückstau-Fund + Akt-8-Fund „Knopf blieb gesperrt"): Teil-Abnahme ist eine
        // DEKLARIERTE Session-Eigenschaft — EINE Quelle für Server-Erlaubnis UND Fertig-Knopf; fehlend =
        // vertagt (Datei-Semantik unten gepinnt). „Alle vertagen"-Bulk wäre identisch mit Fertig — bewusst keiner.
        Assert.True(s.AllowPartialFinish);
        Assert.Contains("Teil-Abnahme", s.Subtitle);
    }

    [Fact]
    public void Edit_und_Vertagen_landen_korrekt_in_Datei_und_Gate_Antwort()
    {
        var s = AdrReviewAdapter.BuildSession("run-1", Request());
        Set(s.Items[0], AdrReviewAdapter.FieldFreigeben, "ja");                                            // expliziter Entscheid
        Set(s.Items[0], AdrReviewAdapter.FieldEntscheidung, "Wir nutzen Firestore MIT lokalem Cache.");   // Edit
        Set(s.Items[0], AdrReviewAdapter.FieldRelated, "REQ-70\nARCH-39");                                 // Related ergänzt
        Set(s.Items[1], AdrReviewAdapter.FieldFreigeben, "nein");                                          // vertagt

        var file = AdrReviewAdapter.Apply("run-1", s);

        var d = Assert.Single(file.Decisions).Draft;                       // ARCH-40 fehlt = vertagt
        Assert.Equal(("ARCH-39", "Wir nutzen Firestore MIT lokalem Cache."), (d.ItemId, d.Decision));
        Assert.Equal(["REQ-70", "ARCH-39"], d.RelatedIds);
        Assert.Null(d.Alternatives);                                       // leer bleibt null

        var resp = AdrReviewAdapter.ToResponse(file, "test");
        Assert.Equal("Wir nutzen Firestore MIT lokalem Cache.", Assert.Single(resp.Accepted).Decision);
    }

    [Fact]
    public void Merge_stellt_fruehere_Edits_wieder_her_und_Vorschau_loest_auf()
    {
        var s = AdrReviewAdapter.BuildSession("run-1", Request());
        var earlier = new AdrDecisionsFile("run-1",
            [new(new AdrDraft("ARCH-40", "Editierter Titel", "K2.", "E2.", "F2."))]);

        AdrReviewAdapter.MergeExistingDecisions(s, earlier);

        Assert.Equal("Editierter Titel", s.Items.Single(i => i.ItemId == "ARCH-40")
            .FieldValues.Single(f => f.FieldKey == AdrReviewAdapter.FieldTitel).Value);
        Assert.Contains("Firestore als Datenbank", AdrReviewAdapter.ResolvePreview("vorschau:ARCH-39", Request()));
    }
}
