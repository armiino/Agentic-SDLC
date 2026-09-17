using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1g-A (19.08., core-analyst-design §3/§4-1): das Anforderungsdokument als deterministische Core-Projektion.
// Grundsatz-Pins: Dokument ERSTELLT nie Inhalt (nur aktive Wahrheit wird gerendert) · professionelle
// Struktur (Funktional je Feature / NFR je Merkmal via analystKategorie / Rahmen / Offenes) · Kopfzeile
// Version+Stand+Fingerabdruck (jede Fassung einem Wahrheits-Stand zuordenbar) · Ehrlichkeits-Vermerk.
public sealed class RequirementsDocumentProjectionTests
{
    private static ProjectStateItem Item(string id, string type, string text, string status = "accepted",
        IReadOnlyDictionary<string, string>? meta = null) => new ProjectStateItem(
        id, type, text, "MEETING", null, 1, "run-1", null, null, null, null, [], [],
        meta is null ? new Dictionary<string, string>() : new Dictionary<string, string>(meta))
        .WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument MiniCore()
    {
        var feature = Item("FC-01", "feature", "Besuchsplanung") with
        { Feature = new FeaturePayload("Besuchsplanung", null, [], []) };
        var reqA = Item("REQ-01", "requirement", "Angehörige können Besuche anmelden.");
        var reqV2 = Item("REQ-02", "requirement", "Erinnerung um 18 Uhr am Vortag.") with { Version = 2 };
        var nfr = Item("REQ-03", "requirement", "Push-Versand nur nach Einwilligung der Angehörigen.",
            meta: new Dictionary<string, string> { [RequirementsDocumentProjection.KategorieKey] = "nfr:privacy" });
        var alt = Item("REQ-04", "requirement", "Alte, abgelöste Fassung.", status: "superseded");
        var ohneFeature = Item("REQ-05", "requirement", "Protokoll-Export als PDF.");
        var rahmen = Item("ARCH-01", "architecture", "Lokale Datenhaltung mit SQLite.") with
        { Architecture = new ArchitecturePayload(["constraint"], "gesetzt") };
        var offen = Item("DEC-01", "decision", "Wie lange werden Bemerkungen aufbewahrt?", status: "open_decision");
        var geklaert = Item("DEC-02", "decision", "Geklärt: nur Admins.", status: "resolved");

        return new ProjectStateDocument("pflege-app", 4, DateTime.UnixEpoch, [],
            [feature, reqA, reqV2, nfr, alt, ohneFeature, rahmen, offen, geklaert],
            [
                new("REQ-01", "FC-01", "part_of_feature", "t", new Dictionary<string, string>()),
                new("REQ-02", "FC-01", "part_of_feature", "t", new Dictionary<string, string>()),
            ], [], []);
    }

    [Fact]
    public void Rendert_die_professionelle_Struktur_aus_der_aktiven_Wahrheit()
    {
        var md = RequirementsDocumentProjection.Render(MiniCore(), version: 3, new DateTime(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc));

        // Kopf: Version + Stand + Fingerabdruck (Wahrheits-Stand-Anker).
        Assert.Contains("Version: 3 · Stand: 2026-08-19 12:00 UTC · Core: 9 Items · Fingerabdruck: ", md);

        // 1. Funktional je Feature — inkl. Versions-Ausweis; NFR-Item NICHT hier (wandert in Abschnitt 2).
        Assert.Contains("### Besuchsplanung (FC-01)", md);
        Assert.Contains("- Angehörige können Besuche anmelden. (REQ-01)", md);
        Assert.Contains("(REQ-02, v2)", md);
        Assert.Contains("### Ohne Feature-Zuordnung", md);
        Assert.Contains("(REQ-05)", md);

        // 2. NFR je Qualitätsmerkmal + Ehrlichkeits-Vermerk (Kategorien-Nachzug offen).
        Assert.Contains("## 2. Nicht-funktionale Anforderungen", md);
        Assert.Contains("Kategorisierung des Alt-Bestands steht aus", md);
        Assert.Contains("### privacy", md);
        Assert.Contains("Einwilligung der Angehörigen. (REQ-03)", md);
        var nfrSection = md[md.IndexOf("## 2.")..md.IndexOf("## 3.")];
        Assert.DoesNotContain("REQ-01", nfrSection);

        // 3. Rahmen (constraint-Rolle) · 4. Offenes (nur open_decision — Dokument weist Offenheit aus).
        Assert.Contains("- Lokale Datenhaltung mit SQLite. (ARCH-01)", md);
        Assert.Contains("- Wie lange werden Bemerkungen aufbewahrt? (DEC-01)", md);
        Assert.DoesNotContain("DEC-02", md);

        // Grundsatz: Dokument erstellt nie Inhalt — Abgelöstes erscheint nicht.
        Assert.DoesNotContain("REQ-04", md);
        Assert.DoesNotContain("abgelöste Fassung", md);
    }

    [Fact]
    public void Version_zaehlt_aus_der_Vorgaenger_Kopfzeile_fort()
    {
        Assert.Equal(1, RequirementsDocumentProjection.NextVersion(null));
        var v3 = RequirementsDocumentProjection.Render(MiniCore(), 3, DateTime.UnixEpoch);
        Assert.Equal(4, RequirementsDocumentProjection.NextVersion(v3));
    }

    [Fact]
    public void Fingerabdruck_ist_deterministisch_und_aendert_sich_mit_der_Wahrheit()
    {
        var core = MiniCore();
        Assert.Equal(RequirementsDocumentProjection.Fingerprint(core), RequirementsDocumentProjection.Fingerprint(core));
        var mutiert = core with { Items = [.. core.Items, Item("REQ-99", "requirement", "Neu.")] };
        Assert.NotEqual(RequirementsDocumentProjection.Fingerprint(core), RequirementsDocumentProjection.Fingerprint(mutiert));
    }
}
