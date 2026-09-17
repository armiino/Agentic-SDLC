using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-49 (17.08., Fund Block G1, Lauf 20260817_093809): der Issue-/Kommentar-Pull lief ohne explicitToken ANONYM
// (Fallback GITHUB_TOKEN/GH_TOKEN — beide ungesetzt) ⇒ seit dem Privat-Schalten des Repos 404 fuer
// from-github-Auto-Pull, pull_github_snapshot und die CLI. Wächter: die Lese-Token-Kette rät nie anonym und
// hat dieselbe Priorität wie MCP/post_comment; und ein Frühausstieg ist im Status EHRLICH als ABGEBROCHEN lesbar.
public sealed class GithubReadTokenTests
{
    [Fact]
    public void Lese_Token_NUR_der_aktuell_genutzte_Projekt_Token_keine_Fallback_Kette()
    {
        // ⚖ Autor 17.08.: keine Überraschungs-Tokens — explizit (Konfig-Naht fullworkflow.tokenEnv) oder
        // GITHUB_AGENTIC_REFACTOR_TOKEN; Alt-/Generik-Tokens (TEST/GITHUB/GH) werden bewusst IGNORIERT.
        var env = new Dictionary<string, string?>
        {
            ["GITHUB_AGENTIC_REFACTOR_TOKEN"] = "tok-agentic",
            ["GITHUB_TEST_TOKEN"] = "tok-test",
            ["GITHUB_TOKEN"] = "tok-generic",
        };
        string? Env(string k) => env.GetValueOrDefault(k);

        Assert.Equal("explizit", GithubIssueSnapshotRunner.ResolveReadToken("explizit", Env));   // Konfig-Naht gewinnt immer
        Assert.Equal("tok-agentic", GithubIssueSnapshotRunner.ResolveReadToken(null, Env));      // sonst NUR der Projekt-Token
        env["GITHUB_AGENTIC_REFACTOR_TOKEN"] = null;
        Assert.Null(GithubIssueSnapshotRunner.ResolveReadToken(null, Env));                      // TEST/GITHUB da, aber IGNORIERT → laut scheitern statt raten
    }

    [Fact]
    public async Task StatusReader_meldet_PIPELINE_ABORTED_als_ehrliches_ABGEBROCHEN()
    {
        var repo = Directory.CreateTempSubdirectory("r49-").FullName;
        var runDir = Path.Combine(repo, "runs", "fullworkflow", "r-dead");
        Directory.CreateDirectory(Path.Combine(runDir, "logs"));
        File.WriteAllText(Path.Combine(runDir, "logs", "events.jsonl"),
            """{"type":"PIPELINE_ABORTED","runId":"r-dead","reason":"Issue-Pull von o/r fehlgeschlagen (Token/Netz/Repo pruefen)."}""" + "\n");

        var s = await PipelineRunStatusReader.ReadAsync(repo, "r-dead");
        Assert.Equal(PipelineRunState.Aborted, s!.State);                                        // kein „aktiv oder abgebrochen?"-Raten mehr
        Assert.Contains(s.NextRequiredAction, a => a.Contains("ABGEBROCHEN") && a.Contains("Issue-Pull"));
        Assert.Contains(s.NextRequiredAction, a => a.Contains("NEU starten"));
    }
}
