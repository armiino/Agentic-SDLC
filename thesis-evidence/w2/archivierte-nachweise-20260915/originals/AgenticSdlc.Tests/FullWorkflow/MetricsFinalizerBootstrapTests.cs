using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-28: im Bootstrap-Zweig fehlen die Betriebs-Anker (STAGE_BACKHALF_START/PIPELINE_BRIDGE) — vorher wurden
// dadurch Ingest/Pbi fensterlos mit dem GANZEN Faden doppelt bef llt (B6-Lauf 20260727_052405: 2×312k Tokens
// für nie gelaufene Stufen). Jetzt: eigene 06-backlog-Stufen mit Fenstern, Ingest/Pbi entfallen.
public sealed class MetricsFinalizerBootstrapTests
{
    private static string Chat(string startUtc, int tin, int tout) =>
        System.Text.Json.JsonSerializer.Serialize(new
        {
            name = "chat openai/test",
            startTimeUtc = startUtc,
            tags = new Dictionary<string, string>
            {
                ["gen_ai.usage.input_tokens"] = tin.ToString(),
                ["gen_ai.usage.output_tokens"] = tout.ToString(),
            },
        });

    [Fact]
    public void Bootstrap_lauf_bekommt_backlog_stufen_und_keine_doppelzaehlung()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "maf-metrics-boot-" + Guid.NewGuid().ToString("N"));
        var runDir = Path.Combine(repoRoot, "runs", "fullworkflow", "20260101_100000_boot");
        Directory.CreateDirectory(Path.Combine(runDir, "logs"));
        try
        {
            var events = new[]
            {
                """{"type":"PIPELINE_START","timestampUtc":"2026-01-01T10:00:00Z"}""",
                """{"type":"STAGE_BOOTSTRAP_START","timestampUtc":"2026-01-01T10:01:00Z"}""",
                """{"type":"PIPELINE_CLUSTER_APPLIED","timestampUtc":"2026-01-01T10:05:00Z"}""",
                """{"type":"STAGE_FORWARD_START","timestampUtc":"2026-01-01T10:09:00Z"}""",
                """{"type":"PIPELINE_RUN_DONE","timestampUtc":"2026-01-01T10:10:00Z"}""",
            };
            File.WriteAllLines(Path.Combine(runDir, "logs", "events.jsonl"), events);
            File.WriteAllLines(Path.Combine(runDir, "logs", "otel-traces.jsonl"),
            [
                Chat("2026-01-01T10:02:00Z", 100, 10),  // Cluster-Fenster
                Chat("2026-01-01T10:06:00Z", 200, 20),  // Clarify-Fenster
            ]);

            var metrics = MetricsFinalizer.Build(runDir, repoRoot,
                FullWorkflowSettings.FromConfig(new FullWorkflowConfig()), "test-model", coreItemsAfter: 71);

            var names = metrics.Stages.Select(s => s.Stage).ToList();
            Assert.Contains("06-backlog-cluster", names);
            Assert.Contains("06-backlog-clarify", names);
            Assert.DoesNotContain("07-ingest", names);
            Assert.DoesNotContain("07-pbi-update", names);

            var cluster = metrics.Stages.Single(s => s.Stage == "06-backlog-cluster");
            var clarify = metrics.Stages.Single(s => s.Stage == "06-backlog-clarify");
            Assert.Equal(100, cluster.Tokens.In);
            Assert.Equal(200, clarify.Tokens.In);
            // Gesamtsumme = einmal gezählt, keine Doppelzuschreibung.
            Assert.Equal(300, metrics.Stages.Sum(s => s.Tokens.In));
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }
}
