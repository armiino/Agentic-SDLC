using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C2b (08.08., c2-inbound-plan §2/§5) — LLM-frei: die Save-Once-Validierung des InboundAgent-Tools (jeder
// Fund genau ein Draft, Dispositions-Vokabular hart) und der deterministische Delta-Builder (Meeting-Ketten-
// Vertrag; Evidenz + §11-Stempel-Anker in Metadata; open_question/noise bewusst NICHT im Delta).
public sealed class GithubInboundDraftingTests
{
    private static GithubInboundFind Find(int n, string category, string? pbiId = null) =>
        new(category, n, $"Issue {n}", pbiId, ["Detail"], GithubIssueTemplate.Parse("Als Nutzer möchte ich X."));

    private static GithubIssueSnapshot Issue(int n) =>
        new(n, $"https://gh/{n}", $"Issue {n}", $"Body von {n}", "open", [], null, null);

    private static async Task<string> SaveAsync(GithubInboundDraftTools tools, params GithubInboundDraft[] drafts)
    {
        var fn = tools.Build().OfType<AIFunction>().Single(f => f.Name == "save_inbound_drafts");
        var raw = await fn.InvokeAsync(new AIFunctionArguments(new Dictionary<string, object?> { ["drafts"] = drafts }));
        return JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!;
    }

    [Fact]
    public async Task SaveTool_validiert_hart_jeder_Fund_genau_ein_Draft_und_Vokabular()
    {
        var tools = new GithubInboundDraftTools([Find(20, GithubInboundCategory.UnmappedNew), Find(12, GithubInboundCategory.MappedDrift, "PBI-1")]);

        var fehlend = await SaveAsync(tools, new GithubInboundDraft(20, "requirement", "Text", "weil"));
        Assert.Contains("KEINEN Draft", fehlend);                                  // #12 fehlt → nicht gespeichert
        Assert.False(tools.Saved);

        var falschesVokabular = await SaveAsync(tools,
            new GithubInboundDraft(20, "wunsch", "Text", "weil"),
            new GithubInboundDraft(12, "requirement", "Text", "weil"));
        Assert.Contains("unbekannte disposition", falschesVokabular);

        var ok = await SaveAsync(tools,
            new GithubInboundDraft(20, "requirement", "Das System muss PDF exportieren.", "F2-Neu-Issue"),
            new GithubInboundDraft(12, "noise", "", "nur Dank"));
        Assert.StartsWith("OK", ok);
        Assert.True(tools.Saved);

        var doppelt = await SaveAsync(tools, new GithubInboundDraft(20, "requirement", "X", "y"));
        Assert.Contains("bereits gespeichert", doppelt);                           // Save-Once
    }

    [Fact]
    public void DeltaBuilder_baut_den_Meeting_Ketten_Vertrag_mit_Evidenz_und_Stempel_Ankern()
    {
        var finds = new[] { Find(20, GithubInboundCategory.UnmappedNew), Find(12, GithubInboundCategory.MappedDrift, "PBI-1"),
            Find(21, GithubInboundCategory.UnmappedNew) };
        var drafts = new[]
        {
            new GithubInboundDraft(20, "requirement", "Das System muss PDF exportieren.", "F2"),
            new GithubInboundDraft(12, "architecture", "Offline-Sync erfolgt über lokale Queue.", "F1-arch"),
            new GithubInboundDraft(21, "open_question", "Welche Rollen brauchen Export?", "Frage"),
        };

        var delta = GithubInboundDeltaBuilder.Build(drafts, finds, [Issue(20), Issue(12), Issue(21)], "run-1");

        Assert.Equal(3, delta.Items.Count);                                        // 9i: open_question fährt MIT (9g-Schiene)
        var question = delta.Items.Single(i => i.ItemId == "GH-21");
        Assert.Equal("open_question", question.ItemType);
        Assert.Equal("21", question.Metadata[GithubOriginMeta.IssueNumber]);       // Herkunft auch an der Frage
        var req = delta.Items.Single(i => i.ItemId == "GH-20");
        Assert.Equal("requirement", req.ItemType);
        Assert.Equal("Das System muss PDF exportieren.", req.Text);
        Assert.Equal("GithubInbound", req.Origin);
        Assert.Equal(GithubProjectionHash.Compute("Body von 20"), req.Metadata["harvestedBodyHash"]);   // §11-Anker
        Assert.Equal("20", req.Metadata["githubIssueNumber"]);                     // §9-Adoption-Anker

        var arch = delta.Items.Single(i => i.ItemId == "GH-12");
        Assert.Equal("architecture", arch.ItemType);
        Assert.Equal("PBI-1", arch.Metadata["mappedPbiId"]);                       // F1: betroffenes PBI reist mit

        Assert.Equal(3, delta.Sources.Count);                                      // je Issue eine Quelle
        Assert.All(delta.Provenance, p => Assert.Equal("harvested_from", p.Links.Single().Relation));
    }
}
