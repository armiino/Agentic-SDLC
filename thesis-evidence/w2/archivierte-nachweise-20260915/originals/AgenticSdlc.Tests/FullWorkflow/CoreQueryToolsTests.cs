using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Microsoft.Extensions.AI;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C3 (07.08.) — der geteilte Core-Lese-Werkzeugkasten (⚖ K5/K7), LLM-frei direkt invoked gegen einen
// In-Memory-Fake der ICoreRepository-Naht: Overview, Liste (Filter + Superseded-Default), Einzel-Item mit
// Beziehungen beider Richtungen. Kein Tool schreibt (der Fake hat gar kein funktionierendes Save).
public sealed class CoreQueryToolsTests
{
    private sealed class FakeRepo(ProjectStateDocument? core) : ICoreRepository
    {
        public Task<bool> ExistsAsync(CancellationToken ct = default) => Task.FromResult(core is not null);
        public Task<ProjectStateDocument> LoadAsync(CancellationToken ct = default) => Task.FromResult(core!);
        public Task SaveAsync(ProjectStateDocument c, CancellationToken ct = default) => throw new NotSupportedException("read-only");
    }

    private static ProjectStateItem Item(string id, string type, string text, string status = "active") => new ProjectStateItem(
        id, type, text, "test", null, 1,
        "run", null, null, null, null, [], [], new Dictionary<string, string>()).WithStatus(CoreStatus.From(status));

    private static ProjectStateDocument Core() => new("p", 4, DateTime.UnixEpoch, [],
        [Item("FC-01", "feature", "Medikamentenplan"),
         Item("PBI-001", "pbi", "Als Nutzer moechte ich Medikamente erfassen"),
         Item("PBI-002", "pbi", "Als Nutzer moechte ich Erinnerungen erhalten"),
         Item("REQ-1", "requirement", "Das System muss Medikamente speichern", "accepted"),
         Item("REQ-9", "requirement", "Alte ersetzte Anforderung", "superseded")],
        [new("PBI-001", "REQ-1", "covers", "test", new Dictionary<string, string>()),
         new("PBI-001", "FC-01", "part_of_feature", "test", new Dictionary<string, string>())],
        [], []);

    private static async Task<JsonElement> InvokeAsync(ICoreRepository repo, string name, IDictionary<string, object?>? args = null)
    {
        var fn = new CoreQueryTools(repo).Build().OfType<AIFunction>().Single(f => f.Name == name);
        var raw = await fn.InvokeAsync(new AIFunctionArguments(args ?? new Dictionary<string, object?>()));
        return JsonDocument.Parse(JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!).RootElement;
    }

    [Fact]
    public async Task overview_zaehlt_je_Typ_und_traegt_Kangal()
    {
        var ov = await InvokeAsync(new FakeRepo(Core()), "get_core_overview");
        Assert.Equal(5, ov.GetProperty("items").GetInt32());
        Assert.Equal(2, ov.GetProperty("byType").GetProperty("pbi").GetInt32());
        Assert.True(ov.TryGetProperty("kangal", out _));

        var miss = await InvokeAsync(new FakeRepo(null), "get_core_overview");
        Assert.Equal("CORE_NOT_FOUND", miss.GetProperty("error").GetString());
    }

    [Fact]
    public async Task list_filtert_nach_Typ_und_Query_und_verbirgt_Superseded_per_Default()
    {
        var repo = new FakeRepo(Core());

        var pbis = await InvokeAsync(repo, "list_core_items", new Dictionary<string, object?> { ["itemType"] = "pbi" });
        Assert.Equal(2, pbis.GetProperty("total").GetInt32());
        Assert.Equal("PBI-001", pbis.GetProperty("items")[0].GetProperty("itemId").GetString());

        var reqs = await InvokeAsync(repo, "list_core_items", new Dictionary<string, object?> { ["itemType"] = "requirement" });
        Assert.Equal(1, reqs.GetProperty("total").GetInt32());                 // REQ-9 (superseded) verborgen

        var all = await InvokeAsync(repo, "list_core_items", new Dictionary<string, object?> { ["itemType"] = "requirement", ["includeSuperseded"] = true });
        Assert.Equal(2, all.GetProperty("total").GetInt32());

        var hits = await InvokeAsync(repo, "list_core_items", new Dictionary<string, object?> { ["query"] = "erinnerung" });
        Assert.Equal(1, hits.GetProperty("total").GetInt32());
        Assert.Equal("PBI-002", hits.GetProperty("items")[0].GetProperty("itemId").GetString());
    }

    [Fact]
    public async Task list_filtert_nach_Status_in_EINEM_Aufruf()
    {
        // C3-Nachschliff (Chat-Beobachtung 07.08.): Status-Fragen = EIN Listen-Aufruf statt 20 Einzel-Gets.
        var core = Core() with { Items = [.. Core().Items, Item("PBI-003", "pbi", "Rechtemodell klaeren", "needs_clarify")] };
        var repo = new FakeRepo(core);

        var nc = await InvokeAsync(repo, "list_core_items", new Dictionary<string, object?> { ["itemType"] = "pbi", ["status"] = "needs_clarify" });
        Assert.Equal(1, nc.GetProperty("total").GetInt32());
        Assert.Equal("PBI-003", nc.GetProperty("items")[0].GetProperty("itemId").GetString());
    }

    [Fact]
    public async Task get_item_liefert_Beziehungen_beider_Richtungen_und_ITEM_NOT_FOUND_laut()
    {
        var repo = new FakeRepo(Core());

        var req = await InvokeAsync(repo, "get_core_item", new Dictionary<string, object?> { ["itemId"] = "REQ-1" });
        Assert.Equal("REQ-1", req.GetProperty("item").GetProperty("itemId").GetString());
        var rel = req.GetProperty("relations")[0];
        Assert.Equal("covers", rel.GetProperty("relationType").GetString());
        Assert.Equal("incoming", rel.GetProperty("direction").GetString());     // PBI-001 covers REQ-1
        Assert.Contains("Medikamente erfassen", rel.GetProperty("otherText").GetString());

        var pbi = await InvokeAsync(repo, "get_core_item", new Dictionary<string, object?> { ["itemId"] = "PBI-001" });
        Assert.Equal(2, pbi.GetProperty("relations").GetArrayLength());          // covers + part_of_feature, outgoing

        var miss = await InvokeAsync(repo, "get_core_item", new Dictionary<string, object?> { ["itemId"] = "REQ-404" });
        Assert.Equal("ITEM_NOT_FOUND", miss.GetProperty("error").GetString());
    }
}
