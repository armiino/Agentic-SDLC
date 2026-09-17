// MEAI001: die gelieferten IChatReducer-Implementierungen sind in M.E.AI 10.6.0 EXPERIMENTELL markiert —
// bewusster Einsatz (⚖ K6/M1-Spike-Fund #1): der Slot ist stabil, die Implementierung gepinnt; DIESE Tests
// sind der Upgrade-Stolperdraht, der Änderungen laut macht.
#pragma warning disable MEAI001
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Steward M1-Spike (07.08., ⚖ K6, R-38-Methodik) — die drei undokumentierten Kanten VOR dem Bau, LLM-frei
// auf der ECHTEN ChatClientAgent-Pipeline mit dem offiziellen Reducer-Slot
// (ChatClientAgentOptions.ChatHistoryProvider ← InMemoryChatHistoryProvider{ChatReducer}):
// E1  Reducer wirkt als SCHIEBEFENSTER und ÜBERLEBT unsere Datei-Persistenz (Serialize/Deserialize),
// E2  Reduktion frisst KEINE schwebende ToolApprovalRequest (die #5189-Nachbarschaft),
// E3  Session unter NEUEM Modus öffnen (andere Agent-Config) — Verhalten PINNEN (offizielle Warnung!).
public sealed class StewardMemorySpikeTests
{
    /// <summary>Skript-Client wie I-1: meldet die Anzahl der beim Modell ANKOMMENDEN Nachrichten.</summary>
    private sealed class CountingClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var list = messages.ToList();
            var toolAlreadyRan = list.Any(m => m.Contents.OfType<FunctionResultContent>().Any());
            var tool = options?.Tools?.OfType<AIFunction>().FirstOrDefault();
            if (tool is not null && !toolAlreadyRan && list.Any(m => m.Text.Contains("Starte", StringComparison.Ordinal)))
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                    [new FunctionCallContent("call-1", tool.Name, new Dictionary<string, object?>())])));
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"ok ({list.Count} Nachrichten)")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    private static AIAgent Agent(IChatReducer? reducer, params AITool[] tools) =>
        new CountingClient().AsAIAgent(new ChatClientAgentOptions
        {
            Name = "MemorySpike",
            ChatOptions = new ChatOptions { Instructions = "Test", Tools = tools.Length == 0 ? null : [.. tools] },
            ChatHistoryProvider = reducer is null ? null
                : new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions { ChatReducer = reducer }),
        });

    private static int Count(string text) => int.Parse(text.Split('(')[1].Split(' ')[0]);

    [Fact]
    public async Task E1_Schiebefenster_wirkt_und_ueberlebt_den_Persist_Roundtrip()
    {
        var agent = Agent(new MessageCountingChatReducer(4));
        var session = await agent.CreateSessionAsync();

        var lastText = "";
        for (var i = 1; i <= 6; i++) lastText = (await agent.RunAsync($"Zug {i}.", session)).Text;
        // 6 Züge = 12 Verlaufs-Nachrichten ohne Reducer; MIT Fenster N=4 kommen ≤ 4+1 (System) beim Modell an.
        Assert.True(Count(lastText) <= 5, $"Fenster wirkt nicht ({Count(lastText)} Nachrichten beim Modell).");

        var json = await agent.SerializeSessionAsync(session);
        var resumed = await agent.DeserializeSessionAsync(json);
        var after = await agent.RunAsync("Nach Persist.", resumed);
        Assert.True(Count(after.Text) <= 5, $"Fenster nach Persist verloren ({Count(after.Text)}).");
    }

    [Fact]
    public async Task E2_Reduktion_frisst_keine_schwebende_Zustimmungsanfrage()
    {
        var invoked = 0;
        var tool = AIFunctionFactory.Create(() => { invoked++; return "AUSGEFÜHRT"; }, "starte_lauf", "Startet.");
        var agent = Agent(new MessageCountingChatReducer(2), new ApprovalRequiredAIFunction(tool));
        var session = await agent.CreateSessionAsync();

        for (var i = 1; i <= 6; i++) await agent.RunAsync($"Zug {i}.", session);   // Fenster mehrfach überrollt
        var first = await agent.RunAsync("Starte bitte den Lauf.", session);
        var request = first.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().Single();
        Assert.Equal(0, invoked);

        var second = await agent.RunAsync(new ChatMessage(ChatRole.User, [request.CreateResponse(true)]), session);
        Assert.Equal(1, invoked);   // PINNT: Zustimmung funktioniert auch unter kleinstem Fenster (N=2)
        Assert.Contains("Nachrichten", second.Text);
    }

    [Fact]
    public async Task E3_Bestehende_Session_unter_neuem_Modus_oeffnen_Verhalten_gepinnt()
    {
        // Session OHNE Reducer anlegen (heutige C1-Config) ...
        var plain = Agent(reducer: null);
        var session = await plain.CreateSessionAsync();
        for (var i = 1; i <= 4; i++) await plain.RunAsync($"Zug {i}.", session);
        var json = await plain.SerializeSessionAsync(session);

        // ... und unter NEUER Config (mit Reducer) öffnen — die offizielle Warnung sagt „nicht reusen";
        // dieser Test PINNT das echte Verhalten auf unserem Stand (bricht es beim Upgrade, wird es LAUT).
        var reduced = Agent(new MessageCountingChatReducer(3));
        var resumed = await reduced.DeserializeSessionAsync(json);
        var after = await reduced.RunAsync("Nach Moduswechsel.", resumed);
        Assert.True(Count(after.Text) <= 4, $"Moduswechsel ohne Wirkung ({Count(after.Text)} Nachrichten).");
    }
}
