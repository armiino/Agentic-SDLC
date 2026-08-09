using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-33 S4: der Wachhund am Core-Port (CoreKangal). EINE Prüf-Logik, an SaveAsync verdrahtet — Fehler
// (I1/I5) brechen den Save ab, BEVOR irgendetwas persistiert wird; I2/I3/I4/I6 sind laute Warnungen.
// Extern-Ziele (Claims/gh#n/HDEC/CAND) sind by design und bleiben still.
public sealed class CoreKangalTests
{
    private static ProjectStateItem Item(string id, string type, string status = "active") => new ProjectStateItem(
        id, type, $"{id} Text", "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateRelation Rel(string from, string to, string type)
        => new(from, to, type, "test", new Dictionary<string, string>());

    private static ProjectStateDocument Core(IReadOnlyList<ProjectStateRelation> relations, params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], relations, [], []);

    // Konsistenter Mini-Core: Feature + gedecktes REQ + PBI im Feature.
    private static ProjectStateDocument HealthyCore(params ProjectStateRelation[] extra)
        => Core(
            [Rel("PBI-1", "FC-01", "part_of_feature"), Rel("REQ-1", "FC-01", "part_of_feature"), Rel("PBI-1", "REQ-1", "covers"), .. extra],
            Item("FC-01", "feature"), Item("PBI-1", "pbi"), Item("REQ-1", "requirement", "accepted"));

    [Fact]
    public void Gesunder_Core_passiert_ohne_Befund()
    {
        var report = CoreKangal.Check(HealthyCore());
        Assert.True(report.Pass);
        Assert.Empty(report.Warnings);
    }

    [Fact]
    public void I1_Relation_auf_String_Ziel_ist_Fehler()
    {
        // exakt die R-36-Klasse: part_of_feature auf einen featureKey-STRING statt ein Feature-Item.
        var report = CoreKangal.Check(HealthyCore(Rel("REQ-1", "no-go", "part_of_feature")));
        Assert.False(report.Pass);
        Assert.Contains(report.Errors, e => e.Code == "I1_TARGET_INVALID");
    }

    [Fact]
    public void I1_Quelle_muss_Core_Item_sein()
    {
        var report = CoreKangal.Check(HealthyCore(Rel("GEIST-1", "REQ-1", "covers")));
        Assert.Contains(report.Errors, e => e.Code == "I1_SOURCE_MISSING");
    }

    [Fact]
    public void Externe_Ziele_sind_by_design_still()
    {
        var report = CoreKangal.Check(HealthyCore(
            Rel("PBI-1", "gh#39", "implemented_by_issue"),
            Rel("REQ-1", "step01-claim-007", "evidenced_by_ledger_claim"),
            Rel("REQ-1", "HDEC-3", "accepted_by_human_decision"),
            Rel("REQ-1", "CAND-2", "promoted_from_l3_candidate")));
        Assert.True(report.Pass);
        Assert.Empty(report.Warnings);
    }

    [Fact]
    public void I5_zwei_Issue_Mappings_je_PBI_sind_Fehler()
    {
        var report = CoreKangal.Check(HealthyCore(
            Rel("PBI-1", "gh#1", "implemented_by_issue"),
            Rel("PBI-1", "gh#2", "implemented_by_issue")));
        Assert.Contains(report.Errors, e => e.Code == "I5_MULTIPLE_ISSUE_MAPPINGS");
    }

    [Fact]
    public void I5b_ein_Issue_von_zwei_PBIs_gemappt_ist_Fehler()
    {
        // C2a-3 (Spiegel-Invariante §15): die Gegenrichtung des 1:1 — zwei PBIs kämpfen sonst um denselben Body.
        var core = Core(
            [Rel("PBI-1", "FC-01", "part_of_feature"), Rel("PBI-2", "FC-01", "part_of_feature"),
             Rel("PBI-1", "REQ-1", "covers"), Rel("REQ-1", "FC-01", "part_of_feature"),
             Rel("PBI-1", "gh#7", "implemented_by_issue"), Rel("PBI-2", "gh#7", "implemented_by_issue")],
            Item("FC-01", "feature"), Item("PBI-1", "pbi"), Item("PBI-2", "pbi"), Item("REQ-1", "requirement", "accepted"));
        var report = CoreKangal.Check(core);
        Assert.Contains(report.Errors, e => e.Code == "I5B_ISSUE_MAPPED_TWICE" && e.Message.Contains("gh#7"));
    }

    [Fact]
    public void I2_und_I3_sind_Warnungen_kein_Block()
    {
        // PBI ohne Feature + neues aktives REQ ohne Deckung (der Übergangszustand nach einem Ingest-Apply).
        var core = Core(
            [Rel("PBI-1", "REQ-1", "covers")],
            Item("PBI-1", "pbi"), Item("REQ-1", "requirement", "accepted"), Item("REQ-2", "requirement", "accepted"));
        var report = CoreKangal.Check(core);

        Assert.True(report.Pass);   // Warnungen blocken NIE — sonst würde der Kangal die lebende Kette strangulieren
        Assert.Contains(report.Warnings, w => w.Code == "I2_PBI_WITHOUT_FEATURE");
        Assert.Contains(report.Warnings, w => w.Code == "I3_ACTIVE_REQ_UNCOVERED" && w.Message.Contains("REQ-2"));
    }

    [Fact]
    public void I3_superseded_Requirements_brauchen_keine_Deckung()
    {
        var report = CoreKangal.Check(Core([], Item("REQ-1", "requirement", "superseded")));
        Assert.DoesNotContain(report.Warnings, w => w.Code == "I3_ACTIVE_REQ_UNCOVERED");
    }

    [Fact]
    public void I4_covers_auf_superseded_ist_Warnung()
    {
        var core = Core(
            [Rel("PBI-1", "FC-01", "part_of_feature"), Rel("PBI-1", "REQ-1", "covers")],
            Item("FC-01", "feature"), Item("PBI-1", "pbi"), Item("REQ-1", "requirement", "superseded"));
        var report = CoreKangal.Check(core);
        Assert.True(report.Pass);
        Assert.Contains(report.Warnings, w => w.Code == "I4_COVERS_SUPERSEDED");
    }

    [Fact]
    public void I6_nicht_offene_Decision_mit_contradicts_ist_Warnung()
    {
        var core = Core(
            [Rel("DEC-001", "REQ-1", "contradicts")],
            Item("DEC-001", "decision", "resolved"), Item("REQ-1", "requirement", "accepted"));
        var report = CoreKangal.Check(core);
        Assert.Contains(report.Warnings, w => w.Code == "I6_CONTRADICTS_LIFECYCLE");
    }

    [Fact]
    public async Task SaveAsync_wirft_bei_I1_und_schreibt_NICHTS()
    {
        var repoRoot = Directory.CreateTempSubdirectory("kangal-test").FullName;
        var repo = new JsonCoreRepository(repoRoot);
        var broken = HealthyCore(Rel("REQ-1", "no-go", "part_of_feature"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repo.SaveAsync(broken));
        Assert.StartsWith("CORE_KANGAL", ex.Message);
        Assert.False(File.Exists(CorePaths.CoreFile(repoRoot)));   // nichts persistiert, auch kein Snapshot-Ordner
        Assert.False(Directory.Exists(Path.Combine(repoRoot, "state", "core", "history")));

        await repo.SaveAsync(HealthyCore());                       // gesunder Core geht durch dieselbe Tür
        Assert.True(File.Exists(CorePaths.CoreFile(repoRoot)));
    }

    // Grün-Nachweis am ECHTEN Produktiv-Core (read-only): der Kangal darf keine legitimen Saves blockieren.
    // Läuft nur, wenn der Repo-Core auffindbar ist (im CI/fremden Checkout still übersprungen).
    [Fact]
    public void Echter_Core_passiert_den_Kangal_fehlerfrei()
    {
        var dir = AppContext.BaseDirectory;
        string? file = null;
        for (var d = new DirectoryInfo(dir); d is not null; d = d.Parent)
        {
            var candidate = Path.Combine(d.FullName, "state", "core", "project-state.json");
            if (File.Exists(candidate)) { file = candidate; break; }
        }
        if (file is null) return;   // kein echter Core in Reichweite -> kein Urteil

        var core = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(file), ProjectStateJson.Options)!;
        var report = CoreKangal.Check(core);

        Assert.True(report.Pass, string.Join(" | ", report.Errors.Select(e => $"{e.Code}: {e.Message}")));
        Assert.Empty(report.Warnings);   // Stand nach R-36-Heilung: auch warnungsfrei erwartet
    }
}
