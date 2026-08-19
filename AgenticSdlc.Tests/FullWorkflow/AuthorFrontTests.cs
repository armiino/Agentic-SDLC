using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Steward;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 3c Autor-Front (09.08.) — Diktat → Delta im Meeting-Ketten-Vertrag: Validierung LAUT, §5-Achsen gesetzt,
// Herkunft wörtlich, Datei über die PIPELINE-Lade-Naht lesbar (= --from-delta-kompatibel bewiesen);
// dazu read_run_report (K12-③ Chronik-Read) und der sourceRunId-Filter (Wahrheits-seitige Lauf-Bilanz).
public sealed class AuthorFrontTests
{
    [Fact]
    public void Builder_validiert_LAUT_und_baut_den_Ketten_Vertrag()
    {
        var (_, errs) = AuthorFrontDeltaBuilder.Build([new AuthorStatement(" ", "requirement")], "s1");
        Assert.Contains(errs, e => e.Contains("leerer Text"));
        (_, errs) = AuthorFrontDeltaBuilder.Build([new AuthorStatement("X", "wunsch")], "s1");
        Assert.Contains(errs, e => e.Contains("disposition"));

        var (delta, ok) = AuthorFrontDeltaBuilder.Build(
        [
            new AuthorStatement("Das System muss Berichte als PDF exportieren.", "requirement", "Autor-Wunsch"),
            new AuthorStatement("Wir setzen auf SQLite als lokale Datenbank.", "architecture"),
        ], "af-test");
        Assert.Empty(ok);
        Assert.Equal(2, delta!.Items.Count);
        var req = delta.Items[0];
        Assert.Equal(("AF-1", "requirement", "AuthorFront"), (req.ItemId, req.ItemType, req.Origin));
        Assert.Equal("author via steward-chat", req.Metadata["quelle"]);
        Assert.Equal("baseline", req.Status);                                  // §5-Achsen gesetzt (kein Guard-Wurf)
        Assert.Equal("architecture", delta.Items[1].ItemType);
        Assert.Equal("dictated_by", delta.Provenance[0].Links[0].Relation);
        Assert.Single(delta.Sources);
    }

    [Fact]
    public async Task Steward_Diktat_Datei_ist_pipeline_ladbar_und_Report_Filter_greifen()
    {
        var repo = Directory.CreateTempSubdirectory("af-").FullName;
        var settings = AgenticSdlc.Host.Configuration.HostSettings.FromRuntimeConfig(new AgenticSdlc.Host.Configuration.RunConfig(), repo);
        // Feil ② (Abnahme 4.0, Vertragswechsel): sessionName ist ein Harness-Fakt (ctor), kein Tool-Parameter
        // mehr — das Modell erfand vorher eigene Werte und der Herkunfts-Stempel log.
        var tools = new StewardRunTools(repo, settings, (a, cb) => Task.FromResult(0), sessionName: "af-probe");
        var save = tools.Build().OfType<AIFunction>().Single(f => f.Name == "save_author_statements");
        var raw = await save.InvokeAsync(new AIFunctionArguments(new Dictionary<string, object?>
        {
            ["statements"] = new[] { new AuthorStatement("PDF-Export für Berichte.", "requirement") },
        }));
        var res = JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
        Assert.True(res.GetProperty("saved").GetBoolean());
        var deltaPath = Path.Combine(repo, res.GetProperty("deltaPath").GetString()!);

        // DER Beweis: die Datei lädt über DIESELBE Naht wie --from-delta (JsonProjectStateRepository).
        var loaded = (await JsonProjectStateRepository.LoadAsync(deltaPath)).Document;
        Assert.Equal("PDF-Export für Berichte.", loaded.Items.Single().Text);
        Assert.Equal("af-probe", loaded.Items.Single().SourceRunId);

        // read_run_report (Fixture) + sourceRunId-Filter (Fixture-Core).
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "r-af", "07-ingest", "applied");
        Directory.CreateDirectory(runDir);
        File.WriteAllText(Path.Combine(runDir, "run-report.json"), """{"applied":[{"incomingItemId":"AF-1","entityId":"REQ-90","outcome":"added"}],"skipped":[]}""");
        var read = new StewardReadTools(repo).Build().OfType<AIFunction>().Single(f => f.Name == "read_run_report");
        var rep = JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(
            await read.InvokeAsync(new AIFunctionArguments(new Dictionary<string, object?> { ["runId"] = "r-af" }))))!).RootElement;
        // read_run_report ist jetzt general (stages.<stufe>): der Ingest-Report liegt unter stages.ingest.
        Assert.Equal("REQ-90", rep.GetProperty("stages").GetProperty("ingest").GetProperty("applied")[0].GetProperty("entityId").GetString());

        var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
            [loaded.Items.Single() with { ItemId = "REQ-90", SourceRunId = "r-af" },
             loaded.Items.Single() with { ItemId = "REQ-01", SourceRunId = "anderer" }], [], [], []);
        var list = new AgenticSdlc.Host.FullWorkflow.Core.CoreQueryTools(new FakeRepo(core)).Build()
            .OfType<AIFunction>().Single(f => f.Name == "list_core_items");
        var filtered = JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(
            await list.InvokeAsync(new AIFunctionArguments(new Dictionary<string, object?> { ["sourceRunId"] = "r-af" }))))!).RootElement;
        Assert.Equal(1, filtered.GetProperty("total").GetInt32());
        Assert.Equal("REQ-90", filtered.GetProperty("items")[0].GetProperty("itemId").GetString());
    }

    private sealed class FakeRepo(ProjectStateDocument core) : AgenticSdlc.Host.FullWorkflow.Core.ICoreRepository
    {
        public Task<bool> ExistsAsync(CancellationToken ct = default) => Task.FromResult(true);
        public Task<ProjectStateDocument> LoadAsync(CancellationToken ct = default) => Task.FromResult(core);
        public Task SaveAsync(ProjectStateDocument c, CancellationToken ct = default) => throw new NotSupportedException();
    }
}
