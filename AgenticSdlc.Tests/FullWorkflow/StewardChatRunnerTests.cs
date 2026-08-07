using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;
using AgenticSdlc.Host.Steward;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// C1b (07.08.) — die Agent-Hülle LLM-frei: BuildAgent trägt Prompt+Tools, der Session-Store persistiert
// über die offizielle MAF-API und ein Folge-Zug sieht den restaurierten Verlauf (I-1-Muster, jetzt am
// ECHTEN Steward-Bau statt am Spike-Agenten).
public sealed class StewardChatRunnerTests
{
    private sealed class EchoCountClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                $"ok ({messages.Count()} Nachrichten, {options?.Tools?.Count ?? 0} Tools)")));
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    [Fact]
    public async Task Huelle_traegt_Tools_und_Session_ueberlebt_den_Datei_Roundtrip()
    {
        // Repo-Wurzel hochlaufen (PromptProvider liest relativ zur Wurzel, nicht aus dem Test-Output).
        var repoRoot = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoRoot, "AgenticSdlc.Host", "Prompts")))
            repoRoot = Path.GetDirectoryName(repoRoot) ?? throw new InvalidOperationException("Repo-Wurzel nicht gefunden");
        var settings = HostSettings.FromRuntimeConfig(new RunConfig(), repoRoot);
        var run = new RunContext(RunId.New(), "test-steward"); run.EnsureFolders();
        var agent = StewardChatRunner.BuildAgent(new EchoCountClient(), settings, run, repoRoot);

        var path = Path.Combine(Directory.CreateTempSubdirectory("steward-sess-").FullName, "s.json");
        var session = await StewardChatRunner.LoadOrCreateSessionAsync(agent, path);
        var first = await agent.RunAsync("Hallo.", session);
        Assert.Contains("8 Tools", first.Text);                              // C1a-Pipeline + C3-Core/GitHub + C1c-Start

        await StewardChatRunner.SaveSessionAsync(agent, session, path);
        Assert.True(File.Exists(path));

        var resumed = await StewardChatRunner.LoadOrCreateSessionAsync(agent, path);
        var second = await agent.RunAsync("Weiter.", resumed);
        var n = int.Parse(second.Text.Split('(')[1].Split(' ')[0]);
        Assert.True(n >= 3, $"Verlauf nicht restauriert (nur {n} Nachrichten).");   // K1-Beweis am echten Bau
    }

    [Fact]
    public void SessionWache_zeigt_Groesse_und_warnt_ab_Schwelle()
    {
        Assert.Equal("", StewardChatRunner.SessionSizeNote(Path.Combine(Path.GetTempPath(), "gibt-es-nicht.json")));

        var dir = Directory.CreateTempSubdirectory("c3-wache-").FullName;
        var small = Path.Combine(dir, "klein.json");
        File.WriteAllText(small, new string('x', 10 * 1024));
        Assert.Contains("Session 10 KB", StewardChatRunner.SessionSizeNote(small));
        Assert.DoesNotContain("⚠", StewardChatRunner.SessionSizeNote(small));

        var big = Path.Combine(dir, "gross.json");
        File.WriteAllText(big, new string('x', 200 * 1024));
        Assert.Contains("⚠", StewardChatRunner.SessionSizeNote(big));            // C3-Wache: Wachstum wird laut
        Assert.Contains("neue Session empfohlen", StewardChatRunner.SessionSizeNote(big));
    }
}
