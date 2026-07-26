using System.Text;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W1e' Schritt 5 — der MetricsFinalizer ist ein reiner Sammler. Dieser Test baut ein synthetisches Faden-Fixture
// (Events mit Zeitstempeln + Faden-otel + Sub-Run-otel + Gate-Report + Forward-Summary + core-before) und prüft die
// Kern-Aggregation: Token-Zuordnung per Sub-Run-Pointer (Ledger/Recipe) UND per Zeit-Fenster (Ingest/Pbi/Forward),
// inkl. dem entscheidenden Fall „Ledger-Duplikat im Faden VOR dem Ingest-Fenster wird NICHT doppelt gezählt".
public sealed class MetricsFinalizerTests
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

    private sealed record Fixture(string RepoRoot, string RunDir);

    private static Fixture BuildFixture()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "maf-metrics-test-" + Guid.NewGuid().ToString("N"));
        var runDir = Path.Combine(repoRoot, "runs", "fullworkflow", "20260101_100000_test");
        Directory.CreateDirectory(Path.Combine(runDir, "logs"));
        Directory.CreateDirectory(Path.Combine(runDir, "07-ingest", "applied"));
        Directory.CreateDirectory(Path.Combine(runDir, "07-github"));

        // Stufen-Events mit Zeitstempeln (definieren die Fenster).
        var events = new[]
        {
            """{"type":"PIPELINE_START","timestampUtc":"2026-01-01T10:00:00Z"}""",
            """{"type":"STAGE_LEDGER_DONE","ledgerRunId":"L1","gatePass":true,"needsHuman":0,"timestampUtc":"2026-01-01T10:00:10Z"}""",
            """{"type":"STAGE_ADJUDICATION_DONE","timestampUtc":"2026-01-01T10:00:11Z"}""",
            """{"type":"STAGE_BASELINES_DONE","recipeRun":"R1","timestampUtc":"2026-01-01T10:00:20Z"}""",
            """{"type":"STAGE_BACKHALF_START","timestampUtc":"2026-01-01T10:00:30Z"}""",
            """{"type":"PIPELINE_BRIDGE","timestampUtc":"2026-01-01T10:00:40Z"}""",
            """{"type":"STAGE_FORWARD_START","timestampUtc":"2026-01-01T10:00:50Z"}""",
            """{"type":"GITHUB_FWD_DONE","timestampUtc":"2026-01-01T10:00:55Z"}""",
            """{"type":"GATE_ANSWERED","gate":"ingest-gate","policy":"AcceptAll","accepted":4,"rejected":0,"timestampUtc":"2026-01-01T10:00:39Z"}""",
        };
        File.WriteAllText(Path.Combine(runDir, "logs", "events.jsonl"), string.Join("\n", events));

        // Faden-otel: 1 Ledger-Duplikat VOR Backhalf (darf NICHT ins Ingest-Fenster) + je 1 Span pro Fenster.
        var faden = new[]
        {
            Chat("2026-01-01T10:00:05Z", 9999, 9999), // Ledger-Duplikat — vor tBackhalf → ignoriert
            Chat("2026-01-01T10:00:35Z", 100, 10),    // Ingest-Fenster [30,40)
            Chat("2026-01-01T10:00:45Z", 200, 20),    // Pbi-Fenster [40,50)
            Chat("2026-01-01T10:00:52Z", 50, 5),      // Forward-Fenster [50,55)
        };
        File.WriteAllText(Path.Combine(runDir, "logs", "otel-traces.jsonl"), string.Join("\n", faden));

        // Sub-Run-otel (Ledger/Recipe) — ganze Datei zählt.
        WriteSubRun(repoRoot, "ledger", "L1", Chat("2026-01-01T10:00:03Z", 1000, 100));
        WriteSubRun(repoRoot, "recipe", "R1", Chat("2026-01-01T10:00:15Z", 2000, 200));

        File.WriteAllText(Path.Combine(runDir, "config.json"),
            """{"transcript":"input/transcripts/demo.txt","policyProfile":"interactive"}""");
        File.WriteAllText(Path.Combine(runDir, "07-ingest", "ingestion-gate-report.json"),
            """{"pass":true,"decision":"accept","errors":[],"warnings":["w1"]}""");
        File.WriteAllText(Path.Combine(runDir, "07-ingest", "ingestion-summary.json"),
            """{"attempts":2}""");
        File.WriteAllText(Path.Combine(runDir, "07-ingest", "applied", "core-before.json"),
            """{"items":[{"id":"a"},{"id":"b"},{"id":"c"}]}""");
        File.WriteAllText(Path.Combine(runDir, "07-github", "github-forward-summary.json"),
            """{"byKind":{"CREATE_ISSUE":1,"UPDATE_ISSUE":2,"HOLD_CLARIFY":1}}""");

        return new Fixture(repoRoot, runDir);
    }

    private static void WriteSubRun(string repoRoot, string stufe, string runId, string chatLine)
    {
        var dir = Path.Combine(repoRoot, "runs", stufe, runId, "logs");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "otel-traces.jsonl"), chatLine);
    }

    [Fact]
    public void Aggregiert_Token_pro_Stufe_ueber_SubRun_UND_Zeitfenster()
    {
        var fx = BuildFixture();
        try
        {
            var m = MetricsFinalizer.Build(fx.RunDir, fx.RepoRoot,
                FullWorkflowSettings.FromConfig(null), "test-model", coreItemsAfter: 5);

            var byStage = m.Stages.ToDictionary(s => s.Stage);
            Assert.Equal(6, m.Stages.Count);

            // Sub-Run-Pointer:
            Assert.Equal((1000, 100), (byStage["01-ledger"].Tokens.In, byStage["01-ledger"].Tokens.Out));
            Assert.Equal((2000, 200), (byStage["02-baselines"].Tokens.In, byStage["02-baselines"].Tokens.Out));
            // Zeit-Fenster — und der KERN: das 9999-Duplikat bei 10:00:05 zählt NICHT ins Ingest-Fenster.
            Assert.Equal((100, 10), (byStage["07-ingest"].Tokens.In, byStage["07-ingest"].Tokens.Out));
            Assert.Equal((200, 20), (byStage["07-pbi-update"].Tokens.In, byStage["07-pbi-update"].Tokens.Out));
            Assert.Equal((50, 5), (byStage["07-github"].Tokens.In, byStage["07-github"].Tokens.Out));
            Assert.Equal((0, 0), (byStage["04-delta"].Tokens.In, byStage["04-delta"].Tokens.Out));
        }
        finally { Directory.Delete(fx.RepoRoot, recursive: true); }
    }

    [Fact]
    public void Traegt_Gate_HumanGate_Issues_Core_korrekt()
    {
        var fx = BuildFixture();
        try
        {
            var m = MetricsFinalizer.Build(fx.RunDir, fx.RepoRoot,
                FullWorkflowSettings.FromConfig(null), "test-model", coreItemsAfter: 5);

            var ingest = m.Stages.First(s => s.Stage == "07-ingest");
            Assert.True(ingest.Gate!.Pass);
            Assert.Equal(0, ingest.Gate.Errors);
            Assert.Equal(1, ingest.Gate.Warnings); // aus dem warnings[]-Array gezählt

            var hg = Assert.Single(m.HumanGates);
            Assert.Equal("ingest-gate", hg.Gate);
            Assert.Equal("AcceptAll", hg.AnsweredBy); // die ECHTE Policy, nicht der Profil-Fallback

            Assert.Equal(1, m.IssuesCreated);
            Assert.Equal(2, m.IssuesUpdated);
            Assert.Equal(3, m.CoreItemsBefore);
            Assert.Equal(5, m.CoreItemsAfter);
            Assert.Equal("input/transcripts/demo.txt", m.BenchmarkVersion);
        }
        finally { Directory.Delete(fx.RepoRoot, recursive: true); }
    }

    [Fact]
    public void Fehlende_Artefakte_werfen_nicht()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "maf-metrics-empty-" + Guid.NewGuid().ToString("N"));
        var runDir = Path.Combine(repoRoot, "runs", "fullworkflow", "empty");
        Directory.CreateDirectory(Path.Combine(runDir, "logs"));
        File.WriteAllText(Path.Combine(runDir, "logs", "events.jsonl"), "");
        try
        {
            var m = MetricsFinalizer.Build(runDir, repoRoot,
                FullWorkflowSettings.FromConfig(null), "test-model", coreItemsAfter: 0);
            Assert.Equal(6, m.Stages.Count);          // Stufen existieren, nur leer
            Assert.All(m.Stages, s => Assert.Equal(0, s.Tokens.In));
            Assert.Empty(m.HumanGates);
            Assert.Equal("unknown", m.BenchmarkVersion);
        }
        finally { Directory.Delete(repoRoot, recursive: true); }
    }
}
