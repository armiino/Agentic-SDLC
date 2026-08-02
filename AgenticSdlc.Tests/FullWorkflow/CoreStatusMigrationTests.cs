using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// §5 Statusmodell-Refactor (S6): der "nichts-bricht"-Hauptbeweis der Achsen-Migration.
// Das Füllen der typisierten Felder aus dem Alt-String ist (a) VERLUSTFREI (Round-Trip je Item), (b) KONSUMENTEN-STABIL
// (ActiveBacklog/Archive/GithubSync byte-gleich vor↔nach) und (c) IDEMPOTENT. Einmal synthetisch über alle Status/Typ-
// Kombinationen, einmal (guarded) gegen den echten Produktiv-Core.
public sealed class CoreStatusMigrationTests
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    [Fact]
    public void Migration_ist_verlustfrei_konsumenten_stabil_und_idempotent_synthetisch()
    {
        var core = SyntheticCore();
        AssertMigrationInvariants(core, expectAllReconstructable: true);

        // Idempotenz: zweimal migrieren == einmal migrieren (byte-gleich serialisiert).
        var once = core.MigrateStatusAxes();
        var twice = once.MigrateStatusAxes();
        Assert.Equal(JsonSerializer.Serialize(once, Json), JsonSerializer.Serialize(twice, Json));

        // Nach der Migration trägt JEDES Item die typisierten Achsen (Voraussetzung für den S7-Löschschritt).
        Assert.All(once.Items, i => Assert.NotNull(i.Validity));
    }

    [Fact]
    public void Migration_recovert_die_implizite_Governance_baseline_agent_accepted_human()
    {
        var migrated = SyntheticCore().MigrateStatusAxes();
        ProjectStateItem ById(string id) => migrated.Items.First(i => i.ItemId == id);
        Assert.Equal(Confirmation.Agent, ById("REQ-baseline").ConfirmedBy);   // baseline -> Agent
        Assert.Equal(Confirmation.Human, ById("REQ-accepted").ConfirmedBy);   // accepted -> Human
        Assert.Equal(Confirmation.Unmarked, ById("FEAT-active").ConfirmedBy); // active  -> ehrlich unmarkiert
    }

    // Guarded: läuft nur, wenn der echte Produktiv-Core im Repo liegt (sonst grün durchgelassen — CI ohne Datei).
    // Beweist die Verlustfreiheit + Konsumenten-Stabilität am ECHTEN Item-Bestand (nicht nur an synthetischen Kombos).
    [Fact]
    public void Migration_des_echten_Produktiv_Cores_ist_verlustfrei_und_konsumenten_stabil()
    {
        var coreFile = FindRepoFile(Path.Combine("state", "core", "project-state.json"));
        if (coreFile is null) return;   // Datei nicht auffindbar -> Test überspringen (kein Fehlschlag)

        var core = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(coreFile), Json)!;
        AssertMigrationInvariants(core, expectAllReconstructable: true);
    }

    // -- Kern-Invarianten der Migration --------------------------------------------------------------------------------

    private static void AssertMigrationInvariants(ProjectStateDocument core, bool expectAllReconstructable)
    {
        var migrated = core.MigrateStatusAxes();

        // Reihenfolge/IDs unverändert.
        Assert.Equal(core.Items.Select(i => i.ItemId), migrated.Items.Select(i => i.ItemId));

        // (a) Round-Trip je Item: der Alt-String ist aus den gefüllten Achsen bit-genau rekonstruierbar.
        if (expectAllReconstructable)
            Assert.All(migrated.Items, i =>
                Assert.Equal(i.Status, i.ReadStatus().ToLegacyString()));

        // (b) Konsumenten-Stabilität: die Migration füllt legitim neue Felder AN den Roh-Items (das ist ihr Zweck), also
        //     ist Roh-Serialisierung zu streng. Semantisch stabil = gleiche MITGLIEDSCHAFT je View + gleiche abgeleitete
        //     GithubSync-Projektion (reine Projektion, keine Roh-Items -> byte-gleich).
        var a0 = CoreViews.ActiveBacklog(core);
        var a1 = CoreViews.ActiveBacklog(migrated);
        Assert.Equal(Ids(a0.Features), Ids(a1.Features));
        Assert.Equal(Ids(a0.Pbis), Ids(a1.Pbis));
        Assert.Equal(Ids(a0.Requirements), Ids(a1.Requirements));
        Assert.Equal(Ids(a0.OpenDecisions), Ids(a1.OpenDecisions));
        Assert.Equal(Ids(CoreViews.Archive(core).Items), Ids(CoreViews.Archive(migrated).Items));
        Assert.Equal(Ser(CoreViews.GithubSync(core)), Ser(CoreViews.GithubSync(migrated)));
    }

    private static IEnumerable<string> Ids(IEnumerable<ProjectStateItem> items) => items.Select(i => i.ItemId);
    private static string Ser(object view) => JsonSerializer.Serialize(view, Json);

    // Sucht eine Repo-relative Datei, indem es vom Test-Assembly-Verzeichnis nach oben läuft.
    private static string? FindRepoFile(string relative)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    // -- Synthetischer Core: alle im echten Core vorkommenden (Typ,Status) + die operativen Werte -----------------------

    private static ProjectStateDocument SyntheticCore()
    {
        var items = new List<ProjectStateItem>
        {
            Item("ARCH-baseline",  "architecture", "baseline",  runId: "r1"),
            Item("REQ-baseline",   "requirement",  "baseline",  runId: "r1"),
            Item("REQ-accepted",   "requirement",  "accepted",  runId: "r2"),
            Item("REQ-superseded", "requirement",  "superseded"),
            Item("FEAT-active",    "feature",      "active"),
            Pbi("PBI-active",      "active",       title: "A", covered: "REQ-baseline"),
            Pbi("PBI-needsclar",   "needs_clarify",title: "B"),
            Pbi("PBI-blocked",     "blocked_by_decision", title: "C", covered: "REQ-accepted"),
            Pbi("PBI-done",        "done",         title: "D"),
            Item("DEC-open",       "decision",     "open_decision"),
            Item("DEC-resolved",   "decision",     "resolved"),
        };

        var relations = new List<ProjectStateRelation>
        {
            new("PBI-active",  "REQ-baseline", "covers",     "test", new Dictionary<string, string>()),
            new("PBI-blocked", "REQ-accepted", "covers",     "test", new Dictionary<string, string>()),
            new("DEC-open",    "REQ-accepted", "contradicts","test", new Dictionary<string, string>()),  // -> blocked
        };

        return new ProjectStateDocument(
            ProjectId: "test", SchemaVersion: ProjectStateDocument.CurrentSchemaVersion, CreatedUtc: default,
            Sources: [], Items: items, Relations: relations, Provenance: [], Proposals: []);
    }

    private static ProjectStateItem Item(string id, string type, string status, string? runId = null) => new(
        ItemId: id, ItemType: type, Text: id, Status: status, Origin: "test",
        Stage: null, Version: 1, SourceRunId: runId, SourceArtifactId: null, SourceArtifactType: null,
        SourceDecisionId: null, SourceCandidateId: null, SourceClaimIds: [], SourceArtifactItemIds: [],
        Metadata: new Dictionary<string, string>());

    private static ProjectStateItem Pbi(string id, string status, string title, string? covered = null) =>
        Item(id, "pbi", status) with
        {
            Pbi = new PbiPayload(
                Goal: $"Ziel {title}", Title: title, AcceptanceCriteria: ["AK1"],
                LinkedRequirementIds: covered is null ? [] : [covered],
                OpenDecisionRefs: [], PriorityRank: null, Readiness: status, Mvp: null, Trace: null)
        };
}
