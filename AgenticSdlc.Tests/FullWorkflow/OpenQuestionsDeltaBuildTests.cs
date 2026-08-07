using AgenticSdlc.Host.FullWorkflow.Artifacts;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 9g S1: die Fragen-Spur bis zum Meeting-Delta — Bestellzettel (CollectArtifacts, Pflicht- vs. Zusatz-Spur) und
// Delta-Build (open-questions-artifact.json -> open_question-Items mit voller Provenienz). LLM-frei: das Artefakt
// kommt als Fixture in exakt dem Format, das ID-Gate/Recipe schreiben (ArtifactDocument).
public sealed class OpenQuestionsDeltaBuildTests : IDisposable
{
    private readonly string _dir = Directory.CreateTempSubdirectory("oq-delta-").FullName;

    public void Dispose() { try { Directory.Delete(_dir, recursive: true); } catch { /* best effort */ } }

    private string WriteArtifact(string type, string prefix, params (string Id, string Text, string Claim)[] items)
    {
        var doc = new ArtifactDocument(
            ArtifactId: prefix, ArtifactType: type, Version: 1, Stage: ArtifactDocument.StageEvidenceBaseline,
            Producer: new ProducerMetadata("recipe-run-1", "test-model", null),
            Items: items.Select(i => new ArtifactItem(i.Id, ArtifactOrigin.Extracted, i.Text, [i.Claim], [])).ToList());
        var dir = Path.Combine(_dir, "baselines", type);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "artifact.json");
        File.WriteAllText(path, JsonSerializer.Serialize(doc, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        return path;
    }

    [Fact]
    public async Task Delta_Build_traegt_Fragen_als_open_question_Items_mit_voller_Provenienz()
    {
        var reqPath = WriteArtifact("requirements", "REQ", ("REQ-01", "Angehörige erhalten Leserechte", "claim-1"));
        var oqPath = WriteArtifact("open-questions", "OQ", ("OQ-01", "Dürfen Angehörige selbst Inhalte eintragen?", "claim-7"));

        var delta = await ProjectStateBuilder.BuildAsync(_dir, [reqPath, oqPath], l3RunDir: null);

        Assert.Equal(2, delta.Items.Count);
        var oq = Assert.Single(delta.Items, i => i.ItemType == "open_question");
        Assert.Equal("OQ-01", oq.ItemId);
        Assert.Contains("eintragen", oq.Text);
        Assert.Equal("recipe-run-1", oq.SourceRunId);                          // Delta-Lauf-Provenienz (wie requirements)
        Assert.Contains("claim-7", oq.SourceClaimIds);                         // Beleg-Kette bis zum Ledger-Claim
        Assert.Contains(delta.Relations, r => r.FromId == "OQ-01" && r.ToId == "claim-7" && r.RelationType == "evidenced_by_ledger_claim");
        Assert.Single(delta.Items, i => i.ItemType == "requirement");          // Pflicht-Spur unangetastet daneben
    }

    [Fact]
    public void CollectArtifacts_nimmt_beide_Spuren_wenn_vorhanden()
    {
        // R-11 A1c: dritte Bestellzettel-Spur architecture (Zusatz, laut-tolerant wie open-questions).
        WriteArtifact("requirements", "REQ", ("REQ-01", "text", "c1"));
        WriteArtifact("open-questions", "OQ", ("OQ-01", "frage?", "c2"));
        WriteArtifact("architecture", "ARCH", ("ARCH-01", "arch", "c3"));

        var (paths, missing) = BaselineStageExecutor.CollectArtifacts(_dir);

        Assert.Equal(3, paths.Count);
        Assert.Empty(missing);
        Assert.EndsWith(Path.Combine("requirements", "artifact.json"), paths[0]);   // Pflicht-Spur zuerst (Bestell-Reihenfolge)
        Assert.EndsWith(Path.Combine("open-questions", "artifact.json"), paths[1]);
        Assert.EndsWith(Path.Combine("architecture", "artifact.json"), paths[2]);
    }

    [Fact]
    public void CollectArtifacts_fehlende_Fragen_Spur_ist_tolerant_und_wird_benannt()
    {
        WriteArtifact("requirements", "REQ", ("REQ-01", "text", "c1"));

        var (paths, missing) = BaselineStageExecutor.CollectArtifacts(_dir);

        Assert.Single(paths);                                                  // Kern-Kette laeuft weiter
        Assert.Equal(["open-questions", "architecture"], missing);             // aber LAUT benannt (Warn-Event beim Aufrufer; A1c: +architecture)
    }

    [Fact]
    public void CollectArtifacts_fehlende_Pflicht_Spur_stoppt_hart()
    {
        WriteArtifact("open-questions", "OQ", ("OQ-01", "frage?", "c2"));      // nur die Zusatz-Spur existiert

        var (paths, missing) = BaselineStageExecutor.CollectArtifacts(_dir);

        Assert.Empty(paths);                                                   // leere Liste = harter Stopp beim Aufrufer
        Assert.Empty(missing);
    }
}
