using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-31 / Invariante I7: jede INHALTS-Mutation des Ingest-Applys traegt den AUSLOESER-Lauf (ingestRunId) als
// sourceRunId am Item — nicht mehr den geerbten Delta-/Recipe-Lauf (Runbook-Befund REQ-08/REQ-56: der
// verursachende Ingest-Lauf war am Item nicht auffindbar). Der Delta-Lauf bleibt via `ingestedFromRun`
// auffindbar; reine Provenienz-Merges (RESTATE) lassen die Provenance unberuehrt.
public sealed class IngestionApplyProvenanceTests
{
    private const string IngestRun = "20260804_ingest_run";
    private const string DeltaRun = "20260801_recipe_run";

    private static ProjectStateItem Item(string id, string type, string text, string run = "baseline-run") => new ProjectStateItem(
        id, type, text, "test", null, 1,
        run, null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(
            type == "decision" ? "open_decision" : "accepted"));

    private static ProjectStateItem Incoming(string id, string text) => new ProjectStateItem(
        id, "requirement", text, "MEETING", null, 1,
        DeltaRun, null, null, null, null, ["claim-7"], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    private static ProjectStateDocument Doc(params ProjectStateItem[] items)
        => new("p", 4, DateTime.UnixEpoch, [], [.. items], [], [], []);

    private static StateChangePlanDocument Plan(params StateChangeOperation[] ops)
        => new(StateChangePlanDocument.CurrentSchemaVersion, "plan-1", DateTime.UnixEpoch, "delta-path", [.. ops]);

    private static (ProjectStateDocument, IngestionApplyReport) Run(ProjectStateDocument core, ProjectStateDocument delta, StateChangeOperation op)
    {
        var (updated, report, _) = IngestionApply.Apply(core, delta, Plan(op),
            new HashSet<string>(StringComparer.Ordinal) { op.IncomingItemId }, IngestRun);
        return (updated, report);
    }

    [Fact]
    public void New_traegt_Ingest_Lauf_als_sourceRunId_und_Delta_Lauf_als_Metadatum()
    {
        var (updated, report) = Run(Doc(Item("REQ-1", "requirement", "bestehend")), Doc(Incoming("IN-1", "neue Anforderung")),
            new StateChangeOperation("IN-1", StateChangeKind.New, "neue Anforderung", null, null, [], "neu"));

        var added = updated.Items.Single(i => i.ItemId == Assert.Single(report.Applied).EntityId);
        Assert.Equal(IngestRun, added.SourceRunId);
        Assert.Equal("IN-1", added.Metadata["ingestedFrom"]);
        Assert.Equal(DeltaRun, added.Metadata["ingestedFromRun"]);
    }

    [Fact]
    public void Refine_setzt_Ingest_Lauf_und_friert_den_alten_Lauf_in_der_History_ein()
    {
        var (updated, _) = Run(Doc(Item("REQ-1", "requirement", "alte Fassung", run: "baseline-113115")),
            Doc(Incoming("IN-1", "praezisierte Fassung")),
            new StateChangeOperation("IN-1", StateChangeKind.Refine, "praezisierte Fassung", "REQ-1", null, [], "refine"));

        var req = updated.Items.Single(i => i.ItemId == "REQ-1");
        Assert.Equal(IngestRun, req.SourceRunId);                              // die NEUE Fassung = Ausloeser-Lauf
        var frozen = Assert.Single(req.History!);
        Assert.Equal("baseline-113115", frozen.SourceRunId);                   // die alte Herkunft luegt nie (History)
        Assert.Contains(IngestRun, frozen.Note);                               // Note nennt Lauf + Incoming
        Assert.Contains("IN-1", frozen.Note);
    }

    // Z4-Pin (Abnahme-Fund 20.08.): REFINE uebernahm vorher NUR die Ziel-Metadata — analystKategorie des
    // Incomings starb still am Apply und die NFR-Sektion des Anforderungsdokuments blieb leer.
    [Fact]
    public void Refine_merged_die_Analyst_Herkunft_des_Incomings_in_die_neue_Fassung()
    {
        var incoming = Incoming("IN-1", "praezisierte Fassung") with
        {
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AnalystOriginMeta.Kategorie] = "nfr:security",
                [AnalystOriginMeta.Herleitung] = "abgeleitet aus REQ-Bestand",
                [AnalystOriginMeta.Linse] = "nfr"
            }
        };
        var (updated, _) = Run(Doc(Item("REQ-1", "requirement", "alte Fassung")), Doc(incoming),
            new StateChangeOperation("IN-1", StateChangeKind.Refine, "praezisierte Fassung", "REQ-1", null, [], "refine"));

        var req = updated.Items.Single(i => i.ItemId == "REQ-1");
        Assert.Equal("nfr:security", req.Metadata[AnalystOriginMeta.Kategorie]);   // Z4: NFR-Sektion lebt auch nach REFINE
        Assert.Equal("nfr", req.Metadata[AnalystOriginMeta.Linse]);
    }

    [Fact]
    public void Supersede_neue_REQ_und_Contradict_DEC_tragen_den_Ingest_Lauf()
    {
        var (afterSupersede, supersedeReport) = Run(Doc(Item("REQ-1", "requirement", "alt")),
            Doc(Incoming("IN-1", "ersetzende Aussage")),
            new StateChangeOperation("IN-1", StateChangeKind.Supersede, "ersetzende Aussage", "REQ-1", null, [], "supersede"));
        var newReq = afterSupersede.Items.Single(i => i.ItemId == Assert.Single(supersedeReport.Applied).EntityId);
        Assert.Equal(IngestRun, newReq.SourceRunId);
        Assert.Equal(DeltaRun, newReq.Metadata["ingestedFromRun"]);

        var (afterContradict, contradictReport) = Run(Doc(Item("REQ-1", "requirement", "vierzehn Tage")),
            Doc(Incoming("IN-2", "dreissig Tage")),
            new StateChangeOperation("IN-2", StateChangeKind.Contradict, "dreissig Tage", "REQ-1", null, [], "widerspruch"));
        var dec = afterContradict.Items.Single(i => i.ItemId == Assert.Single(contradictReport.Applied).EntityId);
        Assert.Equal(IngestRun, dec.SourceRunId);
        Assert.Equal(DeltaRun, dec.Metadata["ingestedFromRun"]);
        Assert.Equal("REQ-1", dec.Metadata["targetEntityId"]);
    }

    [Fact]
    public void Restate_ist_reiner_Claim_Merge_und_laesst_die_Provenance_unberuehrt()
    {
        var (updated, _) = Run(Doc(Item("REQ-1", "requirement", "bestehend", run: "baseline-113115")),
            Doc(Incoming("IN-1", "gleiche Aussage")),
            new StateChangeOperation("IN-1", StateChangeKind.Restate, "", "REQ-1", null, ["claim-9"], "restate"));

        var req = updated.Items.Single(i => i.ItemId == "REQ-1");
        Assert.Equal("baseline-113115", req.SourceRunId);                      // Struktur-/Merge-Regel: keine Umschreibung
        Assert.Contains("claim-9", req.SourceClaimIds);
    }

    [Theory]
    [InlineData("runs/ingestion/20260804_101010_abc123/plan", "20260804_101010_abc123")]
    [InlineData("runs/ingestion/20260804_101010_abc123", "20260804_101010_abc123")]
    public void RunIdFromPlanDir_versteht_beide_CLI_Konventionen(string planDir, string expected)
        => Assert.Equal(expected, IngestionApplyExec.RunIdFromPlanDir(planDir.Replace('/', Path.DirectorySeparatorChar)));
}
