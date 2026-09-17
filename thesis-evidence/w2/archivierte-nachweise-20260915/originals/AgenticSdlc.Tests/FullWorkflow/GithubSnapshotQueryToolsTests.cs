using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C3 (07.08.) — GitHub-Lesen aus dem NEUESTEN Snapshot (geteilter Locator), LLM-frei: Treffer-Suche,
// Repo-Stempel + Frische in jeder Antwort (R-16-Bewusstsein), SNAPSHOT_NOT_FOUND laut.
public sealed class GithubSnapshotQueryToolsTests
{
    private static string Repo(bool withSnapshot = true, string? timestampUtc = null)
    {
        var repo = Directory.CreateTempSubdirectory("c3-gh-").FullName;
        if (!withSnapshot) return repo;
        var dir = Path.Combine(repo, "runs", "github-snapshot", "20260807_000000_test", "issues");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "github-issues-snapshot.json"), """
            [{"issueNumber":1,"url":"u1","title":"Schriftgröße einstellbar","body":"Ziel ...","state":"open","labels":["pbi"],"milestone":null,"updatedUtc":"2026-08-01T10:00:00Z"},
             {"issueNumber":2,"url":"u2","title":"Login","body":"...","state":"closed","labels":[],"milestone":null,"updatedUtc":"2026-08-02T10:00:00Z"}]
            """);
        File.WriteAllText(Path.Combine(dir, "github-issues-snapshot-summary.json"),
            $$"""{"repository":"armiino/Agentic-GitHub-refactor","timestampUtc":"{{timestampUtc ?? DateTime.UtcNow.ToString("O")}}"}""");
        return repo;
    }

    private static async Task<JsonElement> InvokeAsync(string repo, IDictionary<string, object?>? args = null)
    {
        var fn = new GithubSnapshotQueryTools(repo).Build().OfType<AIFunction>().Single();
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args ?? new Dictionary<string, object?>()));
        return JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public async Task suche_findet_Treffer_auch_ueber_Umlaut_Varianz_und_traegt_Repo_Stempel()
    {
        // R-41: Query in ASCII-Form („schriftgroesse") MUSS den Umlaut-Titel („Schriftgröße") treffen.
        var res = await InvokeAsync(Repo(), new Dictionary<string, object?> { ["query"] = "schriftgroesse" });
        Assert.Equal("armiino/Agentic-GitHub-refactor", res.GetProperty("repository").GetString());
        Assert.Equal(1, res.GetProperty("total").GetInt32());
        Assert.Equal(1, res.GetProperty("issues")[0].GetProperty("issueNumber").GetInt32());
        Assert.Equal(JsonValueKind.Null, res.GetProperty("staleNote").ValueKind);   // frisch

        var open = await InvokeAsync(Repo(), new Dictionary<string, object?> { ["state"] = "open" });
        Assert.Equal(1, open.GetProperty("total").GetInt32());
    }

    [Fact]
    public async Task alter_Snapshot_bekommt_staleNote_und_ohne_Snapshot_laut()
    {
        var stale = await InvokeAsync(Repo(timestampUtc: "2026-08-01T00:00:00Z"));
        Assert.Contains("frisch ziehen", stale.GetProperty("staleNote").GetString());

        var miss = await InvokeAsync(Repo(withSnapshot: false));
        Assert.Equal("SNAPSHOT_NOT_FOUND", miss.GetProperty("error").GetString());
    }
}
