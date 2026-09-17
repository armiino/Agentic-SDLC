using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// A3 (07.08.) — der Workflow-Tool-Vertrag: EIN typisierter Status-Kern aus den bestehenden Run-Artefakten
// (pointer.json/events/metrics), gerendert vom status-CLI UND konsumiert von den Steward-Tools.
public sealed class PipelineRunStatusReaderTests
{
    private static string Repo(out string runDir, string runId = "20260807_000000_abc123")
    {
        var repo = Directory.CreateTempSubdirectory("a3-repo-").FullName;
        runDir = Path.Combine(repo, "runs", "fullworkflow", runId);
        Directory.CreateDirectory(Path.Combine(runDir, "checkpoints"));
        Directory.CreateDirectory(Path.Combine(runDir, "logs"));
        return repo;
    }

    [Fact]
    public async Task Pausierter_Lauf_liefert_Gate_Checkpoint_und_die_ReviewHints_als_nextRequiredAction()
    {
        var repo = Repo(out var runDir);
        File.WriteAllText(Path.Combine(runDir, "checkpoints", "pointer.json"),
            """{"runId":"20260807_000000_abc123","sessionId":"s","checkpointId":"cp-42","mode":"adr-gate","savedUtc":"2026-08-07T10:00:00Z"}""");
        File.WriteAllText(Path.Combine(runDir, "logs", "events.jsonl"), """{"type":"PIPELINE_PAUSED"}""" + "\n");

        var st = await PipelineRunStatusReader.ReadAsync(repo, "20260807_000000_abc123");

        Assert.Equal(PipelineRunState.Paused, st!.State);
        Assert.Equal(("adr-gate", "cp-42"), (st.PausedGate, st.CheckpointId));
        Assert.Contains(st.NextRequiredAction, l => l.Contains("adr-review 20260807_000000_abc123"));
        Assert.Contains(st.NextRequiredAction, l => l.Contains("pipeline-full resume"));
        Assert.NotNull(st.Artifacts.PointerPath);
        // Vertrags-Identität: dieselben Zeilen wie die EINE Gate→Anleitung-Quelle (Runner delegiert dorthin).
        // 1b-Rest (18.08., Vertragserweiterung): Zeile 0 = „Checkpoint n von max. m"-Kompass; danach
        // unverändert die ReviewHints + Weiter-Zeile (eine Gate→Anleitung-Quelle bleibt).
        Assert.Contains("Checkpoint 5 von max. 8 (Betrieb)", st.NextRequiredAction[0]);
        Assert.Equal(PipelineRunStatusReader.NextRequiredAction("adr-gate", st.RunId),
                     st.NextRequiredAction.Skip(1).Take(st.NextRequiredAction.Count - 2));
    }

    [Fact]
    public async Task Fertiger_und_unklarer_Lauf_werden_ehrlich_unterschieden()
    {
        var repo = Repo(out var runDir);
        File.WriteAllText(Path.Combine(runDir, "logs", "events.jsonl"), """{"type":"PIPELINE_DONE"}""" + "\n");
        File.WriteAllText(Path.Combine(runDir, "logs", "metrics.json"), "{}");
        var fertig = await PipelineRunStatusReader.ReadAsync(repo, "20260807_000000_abc123");
        Assert.Equal(PipelineRunState.Finished, fertig!.State);
        Assert.Empty(fertig.NextRequiredAction);
        Assert.Contains("PIPELINE_DONE", fertig.LastPipelineEvent);

        File.Delete(Path.Combine(runDir, "logs", "metrics.json"));
        var unklar = await PipelineRunStatusReader.ReadAsync(repo, "20260807_000000_abc123");
        Assert.Equal(PipelineRunState.NotPausedNotFinished, unklar!.State);   // Artefakte können mehr nicht wissen — ehrlich
        Assert.Null(await PipelineRunStatusReader.ReadAsync(repo, "gibt-es-nicht"));
    }
}
