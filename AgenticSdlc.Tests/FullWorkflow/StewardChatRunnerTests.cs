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
        Assert.Contains("29 Tools", first.Text);                             // 27 + C2d-Seile (pull_issue_comments, run_comment_distill)

        await StewardChatRunner.SaveSessionAsync(agent, session, path);
        Assert.True(File.Exists(path));

        var resumed = await StewardChatRunner.LoadOrCreateSessionAsync(agent, path);
        var second = await agent.RunAsync("Weiter.", resumed);
        var n = int.Parse(second.Text.Split('(')[1].Split(' ')[0]);
        Assert.True(n >= 3, $"Verlauf nicht restauriert (nur {n} Nachrichten).");   // K1-Beweis am echten Bau
    }

    [Fact]
    public async Task M1_memory_count_wirkt_als_Schiebefenster_an_der_ECHTEN_Huelle()
    {
        var repoRoot = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoRoot, "AgenticSdlc.Host", "Prompts")))
            repoRoot = Path.GetDirectoryName(repoRoot) ?? throw new InvalidOperationException("Repo-Wurzel nicht gefunden");
        var settings = HostSettings.FromRuntimeConfig(new RunConfig(), repoRoot);
        var run = new RunContext(RunId.New(), "test-steward"); run.EnsureFolders();
        var agent = StewardChatRunner.BuildAgent(new EchoCountClient(), settings, run, repoRoot, memory: "count:3");

        var session = await agent.CreateSessionAsync();
        var text = "";
        for (var i = 1; i <= 6; i++) text = (await agent.RunAsync($"Zug {i}.", session)).Text;
        var n = int.Parse(text.Split('(')[1].Split(' ')[0]);
        Assert.True(n <= 4, $"Fenster count:3 wirkt nicht an der Hülle ({n} Nachrichten beim Modell).");
    }

    [Fact]
    public void M1_fresh_rotiert_statt_zu_loeschen_und_unbekannter_Modus_ist_LAUT()
    {
        var dir = Directory.CreateTempSubdirectory("m1-fresh-").FullName;
        var path = Path.Combine(dir, "s.json");
        Assert.Null(StewardChatRunner.RotateForFresh(path));                    // nichts da = nichts zu rotieren

        File.WriteAllText(path, "{}");
        var prev = StewardChatRunner.RotateForFresh(path);
        Assert.False(File.Exists(path));                                        // frisch: alter Stand ist weg ...
        Assert.True(File.Exists(prev));                                         // ... aber NICHT gelöscht (rotiert)

        var ex = Assert.Throws<ArgumentException>(() => StewardChatRunner.CreateReducer("vergiss-alles", new EchoCountClient()));
        Assert.Contains("count[:N] | summarize", ex.Message);
    }

    [Fact]
    public void M1b_memory_Default_kommt_aus_run_config_und_mappt_in_die_Settings()
    {
        var cfg = new RunConfig { Steward = { Memory = "  count:12 " } };
        var settings = HostSettings.FromRuntimeConfig(cfg, Directory.GetCurrentDirectory());
        Assert.Equal("count:12", settings.StewardMemoryMode);                 // getrimmt gemappt

        var leer = HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());
        Assert.Null(leer.StewardMemoryMode);                                  // kein Default = wie bisher

        // Präzedenz gepinnt: CLI-Flag ÜBERSTIMMT den Config-Default (Kollegen-Nachzug 09.08.).
        Assert.Equal(("count:5", false), StewardChatRunner.ResolveMemory("count:5", settings));
        Assert.Equal(("count:12", true), StewardChatRunner.ResolveMemory(null, settings));
        Assert.Equal((null, false), StewardChatRunner.ResolveMemory(null, leer));
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
