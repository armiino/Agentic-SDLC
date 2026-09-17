using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Story-Map-Tafel (Leitfaden-Slice 22.08.): Pins = der Szenario-Katalog S1–S13 aus
// Thesis-Docs/aktiv/leitfaden-abdeckung.md §4a — jede Zeile dort hat hier ihren Test. S14 (Verhaltens-Erhalt
// der extrahierten Section-Mechanik) beweisen die UNVERÄNDERTEN C4LoopTests/AuthoredDocDraftingTests.
public sealed class StoryMapSectionTests
{
    private static readonly DateTime T = DateTime.UnixEpoch;

    private static ProjectStateItem Pbi(string id, string text, string status = "accepted",
        string? goal = null, Dictionary<string, string>? meta = null) => new ProjectStateItem(
            id, "pbi", text, "re-clarify", null, 1, "run", null, null, null, null, [], [],
            meta ?? new Dictionary<string, string>(),
            Pbi: goal is null ? null : new PbiPayload(goal, text, [], [], [], null, null, null, null))
        .WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items) => new("p", 4, T, [], [.. items], [], [], []);

    private static StoryMapSection.Zuordnung Z(params StoryMapSection.Schritt[] schritte) => new(1, [.. schritte]);
    private static StoryMapSection.Schritt Schritt(string key, params string[] pbis) => new(key, key, [.. pbis]);

    // ── Wachen S2–S4 ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void S2_unbekannte_oder_artfremde_Id_verweigert_laut()
    {
        var core = Doc(Pbi("PBI-001", "a"),
            new ProjectStateItem("REQ-1", "requirement", "r", "M", null, 1, "run", null, null, null, null, [], [],
                new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted")));
        var ex1 = Assert.Throws<ArgumentException>(() => StoryMapSection.Validate(Z(Schritt("s", "PBI-999")), core));
        Assert.Contains("PBI-999", ex1.Message);
        var ex2 = Assert.Throws<ArgumentException>(() => StoryMapSection.Validate(Z(Schritt("s", "REQ-1")), core));
        Assert.Contains("REQ-1", ex2.Message);                                 // REQ ist kein Kärtchen
    }

    [Fact]
    public void S3_Doppel_Zuordnung_verweigert()
    {
        var core = Doc(Pbi("PBI-001", "a"));
        var ex = Assert.Throws<ArgumentException>(() =>
            StoryMapSection.Validate(Z(Schritt("s1", "PBI-001"), Schritt("s2", "PBI-001")), core));
        Assert.Contains("mehrfach", ex.Message);
    }

    [Fact]
    public void S4_leere_Reise_verweigert_und_gueltige_Zuordnung_passiert()
    {
        var core = Doc(Pbi("PBI-001", "a"));
        Assert.Throws<ArgumentException>(() => StoryMapSection.Validate(Z(), core));
        StoryMapSection.Validate(Z(Schritt("s", "PBI-001")), core);            // kein Throw
    }

    // ── Render: S1/S5/S8/S9/S10 ───────────────────────────────────────────────────────────────

    [Fact]
    public void S1_abgeloeste_Id_faellt_aus_der_Tafel_Nachfolger_landet_im_Sammelbecken()
    {
        var core = Doc(
            Pbi("PBI-001", "alt", status: "superseded"),
            Pbi("PBI-002", "Nachfolger", goal: "Als PO will ich X, damit Y."));
        var tafel = StoryMapSection.Render(core, Z(Schritt("s", "PBI-001")));
        Assert.DoesNotContain("| PBI-001 |", tafel);                           // Abgelöstes: raus
        Assert.Contains(StoryMapSection.SammelbeckenTitel, tafel);
        Assert.Contains("| PBI-002 |", tafel);                                 // Nachfolger: sichtbar wartend
    }

    [Fact]
    public void S5_nicht_zugeordnete_aktive_PBIs_landen_im_Sammelbecken()
    {
        var core = Doc(Pbi("PBI-001", "a", goal: "Als PO will ich A."), Pbi("PBI-002", "b"));
        var tafel = StoryMapSection.Render(core, Z(Schritt("s", "PBI-001")));
        var becken = tafel[tafel.IndexOf(StoryMapSection.SammelbeckenTitel, StringComparison.Ordinal)..];
        Assert.Contains("| PBI-002 |", becken);
        Assert.DoesNotContain("| PBI-001 |", becken);
    }

    [Fact]
    public void S8_needs_clarify_rendert_Text_mit_Klaerungs_Marker_statt_erfundener_Story()
    {
        var core = Doc(Pbi("PBI-001", "Es ist unklar, ob X.", status: "needs_clarify"));
        var tafel = StoryMapSection.Render(core, Z(Schritt("s", "PBI-001")));
        Assert.Contains("⚠ in Klärung", tafel);
        Assert.Contains("Es ist unklar, ob X.", tafel);
    }

    [Fact]
    public void S9_done_bekommt_Haken_und_Render_ist_deterministisch_idempotent()
    {
        var core = Doc(Pbi("PBI-001", "a", status: "done", goal: "Als PO will ich A."));
        var z = Z(Schritt("s", "PBI-001"));
        var tafel = StoryMapSection.Render(core, z);
        Assert.Contains("✓ fertig", tafel);
        Assert.Equal(tafel, StoryMapSection.Render(core, z));                  // nichts Volatiles (R-63-Klasse)
    }

    [Fact]
    public void S10_geschlossenes_unzugeordnetes_PBI_wartet_nicht_im_Sammelbecken()
    {
        var core = Doc(Pbi("PBI-001", "fertig und nie eingeordnet", status: "done"));
        var tafel = StoryMapSection.Render(core, Z(Schritt("s")));
        Assert.DoesNotContain(StoryMapSection.SammelbeckenTitel, tafel);
    }

    // ── Zuordnungs-Zeile: Roundtrip + S6 ──────────────────────────────────────────────────────

    [Fact]
    public void Zuordnung_ueberlebt_den_Render_Roundtrip_woertlich()
    {
        var core = Doc(Pbi("PBI-001", "a"), Pbi("PBI-002", "b"));
        var z = new StoryMapSection.Zuordnung(3, [Schritt("ankommen", "PBI-002", "PBI-001")]);
        var gelesen = StoryMapSection.ReadZuordnung(StoryMapSection.Render(core, z));
        Assert.NotNull(gelesen);
        Assert.Equal(3, gelesen!.PersonasVersion);
        Assert.Equal(["PBI-002", "PBI-001"], gelesen.Schritte.Single().PbiIds); // Reihenfolge = Redaktion, bleibt
    }

    [Fact]
    public void S6_ohne_oder_mit_kaputter_Zeile_degradiert_ehrlich_alles_ins_Sammelbecken()
    {
        var core = Doc(Pbi("PBI-001", "a"));
        Assert.Null(StoryMapSection.ReadZuordnung("kein Stempel"));
        Assert.Null(StoryMapSection.ReadZuordnung(StoryMapSection.StampPrefix + "{kaputt -->"));
        var tafel = StoryMapSection.Render(core, null);
        Assert.Contains("keine Zuordnung", tafel);
        Assert.Contains("| PBI-001 |", tafel);                                 // nichts verschwindet
    }

    // ── S7: Ensure heilt Hand-Edits, Erzähl-Zone bleibt unangetastet ──────────────────────────

    [Fact]
    public void S7_Ensure_regeneriert_die_Tafel_und_laesst_die_Erzaehl_Zone_stehen()
    {
        var repoRoot = Directory.CreateTempSubdirectory("storymap-").FullName;
        try
        {
            var core = Doc(Pbi("PBI-001", "a", goal: "Als PO will ich A."));
            var pfad = Path.Combine(repoRoot, "docs", "storymap.md");
            Directory.CreateDirectory(Path.GetDirectoryName(pfad)!);
            var erzaehlung = "# User Story Map\n\nMeine Reise-Prosa.";
            File.WriteAllText(pfad, erzaehlung + "\n\n" + StoryMapSection.Render(core, Z(Schritt("s", "PBI-001")))
                                    + "\n- HAND-MANIPULATION: Fake-Kärtchen (PBI-999)\n");
            StoryMapSection.Ensure(repoRoot, core);
            var inhalt = File.ReadAllText(pfad);
            Assert.Contains("Meine Reise-Prosa.", inhalt);                     // Zone A unberührt
            Assert.DoesNotContain("HAND-MANIPULATION", inhalt);                // Zone B geheilt
            var vorher = inhalt;
            StoryMapSection.Ensure(repoRoot, core);
            Assert.Equal(vorher, File.ReadAllText(pfad));                      // idempotent
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }

    // ── S12 + Frische-Notiz ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Frische_meldet_nur_Handlungsleitendes_und_verstummt_wenn_nichts_wartet()
    {
        var repoRoot = Directory.CreateTempSubdirectory("storymap-").FullName;
        try
        {
            var core = Doc(Pbi("PBI-001", "a"), Pbi("PBI-002", "b"));
            Assert.Null(StoryMapSection.FrischeNotiz(repoRoot, core));         // kein Doc → kein Genörgel

            var pfad = Path.Combine(repoRoot, "docs", "storymap.md");
            Directory.CreateDirectory(Path.GetDirectoryName(pfad)!);
            File.WriteAllText(pfad, "x\n\n" + StoryMapSection.Render(core, new StoryMapSection.Zuordnung(1,
                [Schritt("s", "PBI-001")])));
            var notiz = StoryMapSection.FrischeNotiz(repoRoot, core);
            Assert.Contains("1 PBI(s) warten auf Einordnung", notiz);

            File.WriteAllText(pfad, "x\n\n" + StoryMapSection.Render(core, new StoryMapSection.Zuordnung(1,
                [Schritt("s", "PBI-001", "PBI-002")])));
            Assert.Null(StoryMapSection.FrischeNotiz(repoRoot, core));         // alles eingeordnet → still
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }

    [Fact]
    public void S12_neuere_Personas_werden_gemeldet()
    {
        var repoRoot = Directory.CreateTempSubdirectory("storymap-").FullName;
        try
        {
            var core = Doc(Pbi("PBI-001", "a"));
            Directory.CreateDirectory(Path.Combine(repoRoot, "docs"));
            File.WriteAllText(Path.Combine(repoRoot, "docs", "personas.md"), "# Personas\n\n> Version: 4 · Stand: x\n");
            File.WriteAllText(Path.Combine(repoRoot, "docs", "storymap.md"), "x\n\n"
                + StoryMapSection.Render(core, new StoryMapSection.Zuordnung(2, [Schritt("s", "PBI-001")])));
            var notiz = StoryMapSection.FrischeNotiz(repoRoot, core);
            Assert.Contains("Personas sind neuer (v4)", notiz);
            Assert.Equal(4, StoryMapSection.CurrentPersonasVersion(repoRoot));
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }

    // ── S13: typisierter Draft-Vertrag an der Tool-Grenze ─────────────────────────────────────

    [Fact]
    public void S13_Compose_validiert_und_komponiert_Erzaehlung_plus_Tafel()
    {
        var repoRoot = Directory.CreateTempSubdirectory("storymap-").FullName;
        try
        {
            var core = Doc(Pbi("PBI-001", "a", goal: "Als PO will ich A."));
            var doc = AuthoredDocDrafting.ComposeStoryMapDraft(repoRoot, core,
                """{"erzaehlung":"# User Story Map\n\nReise.","schritte":[{"key":"s","titel":"Schritt","pbiIds":["PBI-001"]}]}""");
            Assert.Contains("Reise.", doc);
            Assert.Contains(StoryMapSection.Header, doc);
            Assert.Contains("| PBI-001 |", doc);
            Assert.NotNull(StoryMapSection.ReadZuordnung(doc));

            Assert.Throws<InvalidOperationException>(() => AuthoredDocDrafting.ComposeStoryMapDraft(repoRoot, core, "kein json"));
            var ex = Assert.Throws<ArgumentException>(() => AuthoredDocDrafting.ComposeStoryMapDraft(repoRoot, core,
                """{"erzaehlung":"x","schritte":[{"key":"s","titel":"S","pbiIds":["PBI-999"]}]}"""));
            Assert.Contains("PBI-999", ex.Message);                            // Halluzination stirbt an der Tool-Grenze
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }

    // ── Save-Naht: Wachen + Tafel-Regeneration + Whitelist ────────────────────────────────────

    [Fact]
    public void Storymap_ist_Whitelist_Art_und_BuildInput_traegt_die_Kaertchen()
    {
        Assert.NotNull(AuthoredDocument.Resolve("storymap"));
        var repoRoot = Directory.CreateTempSubdirectory("storymap-").FullName;
        try
        {
            Directory.CreateDirectory(Path.Combine(repoRoot, "docs"));
            File.WriteAllText(Path.Combine(repoRoot, "docs", "personas.md"), "# Personas\n\n> Version: 1 · Stand: x\n\nPaula.");
            var core = Doc(
                Pbi("PBI-001", "a", goal: "Als PO will ich A.", meta: new() { ["featureId"] = "FC-01", ["priority"] = "hoch" }),
                Pbi("PBI-002", "Es ist unklar.", status: "needs_clarify"));
            var input = AuthoredDocDrafting.BuildInput(repoRoot, core, AuthoredDocument.Resolve("storymap")!, null);
            Assert.Contains("Paula.", input);                                  // Personas als Reise-Grundlage
            Assert.Contains("PBI-001 (Feature FC-01, Prio hoch)", input);
            Assert.Contains("[in Klärung]", input);
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }
}
