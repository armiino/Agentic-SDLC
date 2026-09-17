using AgenticSdlc.Host.FullWorkflow;
using System.Text.Json;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R2-Netz: der geteilte HitlPointer muss die drei historischen pointer.json-Formate byte-kompatibel
// schreiben/lesen (ingestion/pbi ohne Extras · decision mit mode · github-forward mit repository/tokenEnv).
public sealed class HitlPointerTests
{
    private static readonly DateTime T = new(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ohne_optionale_Felder_werden_keine_Null_Keys_geschrieben()
    {
        var json = JsonSerializer.Serialize(new HitlPointer("r1", "s1", "c1", null, null, null, T), JsonFiles.Json);
        Assert.DoesNotContain("mode", json);
        Assert.DoesNotContain("repository", json);
        Assert.DoesNotContain("tokenEnv", json);
        Assert.Contains("\"runId\": \"r1\"", json);
    }

    [Fact]
    public void Decision_Variante_schreibt_mode()
    {
        var json = JsonSerializer.Serialize(new HitlPointer("r1", "s1", "c1", "agent", null, null, T), JsonFiles.Json);
        Assert.Contains("\"mode\": \"agent\"", json);
        Assert.DoesNotContain("repository", json);
    }

    [Fact]
    public void Legacy_Ingestion_Pointer_ohne_mode_ist_lesbar()
    {
        var legacy = """{ "runId":"r1", "sessionId":"s1", "checkpointId":"c1", "savedUtc":"2026-07-21T10:00:00Z" }""";
        var p = JsonSerializer.Deserialize<HitlPointer>(legacy, JsonFiles.Json)!;
        Assert.Equal("c1", p.CheckpointId);
        Assert.Null(p.Mode);
        Assert.Null(p.Repository);
    }

    [Fact]
    public void Legacy_GithubForward_Pointer_mit_repository_tokenEnv_ist_lesbar()
    {
        var legacy = """{ "runId":"r1", "sessionId":"s1", "checkpointId":"c1", "repository":"o/n", "tokenEnv":"GH", "savedUtc":"2026-07-21T10:00:00Z" }""";
        var p = JsonSerializer.Deserialize<HitlPointer>(legacy, JsonFiles.Json)!;
        Assert.Equal("o/n", p.Repository);
        Assert.Equal("GH", p.TokenEnv);
        Assert.Null(p.Mode);
    }
}
