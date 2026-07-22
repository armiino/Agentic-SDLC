using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R3a-Netz: die geteilte Dreifach-Logik der Human-Gates (--accept-all | --accept Liste | human-decisions.json).
public sealed class HumanDecisionsTests
{
    private static Task<IReadOnlyList<string>> FromFileNotExpected(string _)
        => throw new InvalidOperationException("Datei-Zweig darf hier nicht laufen.");

    [Fact]
    public async Task AcceptAll_liefert_alle_Ids_ohne_Dateizugriff()
    {
        var result = await HumanDecisions.ResolveAcceptedAsync(
            acceptAll: true, acceptList: null, decisionsPath: "/nicht/vorhanden.json",
            allIds: () => ["op-0", "op-1"], acceptedFromFile: FromFileNotExpected);
        Assert.Equal(["op-0", "op-1"], result);
    }

    [Fact]
    public async Task AcceptListe_wird_getrimmt_und_gesplittet()
    {
        var result = await HumanDecisions.ResolveAcceptedAsync(
            acceptAll: false, acceptList: " op-0 ,op-2, ", decisionsPath: "/nicht/vorhanden.json",
            allIds: () => throw new InvalidOperationException("allIds darf hier nicht laufen."),
            acceptedFromFile: FromFileNotExpected);
        Assert.Equal(["op-0", "op-2"], result);
    }

    [Fact]
    public async Task Datei_Zweig_laeuft_nur_wenn_Datei_existiert_und_bekommt_den_Pfad()
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"hd-test-{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(tmp, "{}");
        try
        {
            string? seenPath = null;
            var result = await HumanDecisions.ResolveAcceptedAsync(
                acceptAll: false, acceptList: null, decisionsPath: tmp,
                allIds: () => [],
                acceptedFromFile: p => { seenPath = p; return Task.FromResult<IReadOnlyList<string>>(["op-1"]); });
            Assert.Equal(tmp, seenPath);
            Assert.Equal(["op-1"], result);
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public async Task Ohne_Entscheidung_kommt_null()
    {
        var result = await HumanDecisions.ResolveAcceptedAsync(
            acceptAll: false, acceptList: null, decisionsPath: "/nicht/vorhanden.json",
            allIds: () => [], acceptedFromFile: FromFileNotExpected);
        Assert.Null(result);
    }
}
