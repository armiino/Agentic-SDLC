using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// B4 (pipeline-full-bootstrap-plan §5/§7): das backlog-review-gate ist EDIT-fähig — der Resolver liefert
// BacklogHumanDecision-Listen statt accept/reject. Semantik: accept-all = leere Liste (Apply: fehlender
// Entscheid = accept, keine Edits); replay = aufgezeichnete Entscheide 1:1 inkl. EditedPbiJson; interactive = Pause.
public sealed class BacklogReviewResolverTests
{
    [Fact]
    public void AcceptAll_ist_leere_liste_keine_edits()
    {
        var r = BacklogReviewResolver.Resolve(new GatePolicy(GatePolicyKind.AcceptAll));
        Assert.NotNull(r);
        Assert.Empty(r!);
    }

    [Fact]
    public void Interactive_ist_null_pause()
        => Assert.Null(BacklogReviewResolver.Resolve(GatePolicy.Interactive));

    [Fact]
    public void Replay_liefert_entscheide_inklusive_EditedPbiJson()
    {
        var path = Path.Combine(Path.GetTempPath(), $"backlog-replay-{Guid.NewGuid():N}.json");
        var file = new BacklogHumanDecisionsFile("run-x", "tester",
        [
            new BacklogHumanDecision("PBI-001", "accept", null, null),
            new BacklogHumanDecision("PBI-002", "edit", """{"pbiId":"PBI-002"}""", "geschärft"),
            new BacklogHumanDecision("PBI-003", "reject", null, "doppelt"),
        ]);
        File.WriteAllText(path, JsonSerializer.Serialize(file));
        try
        {
            var r = BacklogReviewResolver.Resolve(new GatePolicy(GatePolicyKind.Replay, path))!;
            Assert.Equal(3, r.Count);
            var edit = r.Single(d => d.PbiId == "PBI-002");
            Assert.Equal("edit", edit.Decision);
            Assert.Contains("PBI-002", edit.EditedPbiJson); // Edits werden 1:1 wieder eingespielt
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void Replay_ohne_datei_faellt_auf_leere_liste_zurueck()
    {
        var r = BacklogReviewResolver.Resolve(new GatePolicy(GatePolicyKind.Replay, Path.Combine(Path.GetTempPath(), "fehlt.json")));
        Assert.NotNull(r); // kein Pause-Deadlock im unbeaufsichtigten Lauf — leere Liste = alles accept
        Assert.Empty(r!);
    }
}
