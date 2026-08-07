using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Steward I-1 (07.08.) — Fundament-Spike VOR C1 (k-entscheidungen ⚖ + Kollegen-Hinweis):
// (a) Session-Persistenz über die OFFIZIELLE API (CreateSessionAsync → SerializeSession → Datei →
//     DeserializeSessionAsync → Folge-Run mit erhaltenem Verlauf),
// (b) ApprovalRequiredAIFunction-Roundtrip (Run endet mit ToolApprovalRequestContent OHNE Ausführung;
//     CreateResponse(true) zurückgeben → Tool läuft),
// (c) die #5189-Kante: Serialize/Deserialize ZWISCHEN Approval-Anfrage und -Antwort — Verhalten PINNEN.
// Technik: Fake-IChatClient (geskriptete Antworten) — LLM-frei, aber ECHTE ChatClientAgent-Pipeline.
public sealed class StewardFoundationSpikeTests
{
    /// <summary>Skript-Client: ruft beim ERSTEN Zug das Tool, antwortet danach mit „fertig (n Nachrichten)".</summary>
    private sealed class ScriptedChatClient : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            var list = messages.ToList();
            var toolAlreadyRan = list.Any(m => m.Contents.OfType<FunctionResultContent>().Any());
            var tool = options?.Tools?.OfType<AIFunction>().FirstOrDefault();
            if (tool is not null && !toolAlreadyRan)
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                    [new FunctionCallContent("call-1", tool.Name, new Dictionary<string, object?>())])));
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, $"fertig ({list.Count} Nachrichten)")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    [Fact]
    public async Task Session_Persistenz_Roundtrip_ueber_die_offizielle_API()
    {
        var agent = new ScriptedChatClient().AsAIAgent(instructions: "Test", name: "StewardSpike");
        var session = await agent.CreateSessionAsync();
        await agent.RunAsync("Mein Name ist Armino.", session);

        // Offizieller Persist-Weg: Serialize → (Datei) → Deserialize (k-entscheidungen K1, Learn/Session-Doc).
        var json = await agent.SerializeSessionAsync(session);
        var path = Path.Combine(Directory.CreateTempSubdirectory("steward-").FullName, "session.json");
        await File.WriteAllTextAsync(path, json.GetRawText());
        var resumed = await agent.DeserializeSessionAsync(System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(path)).RootElement);

        var second = await agent.RunAsync("Folgefrage.", resumed);
        // Der Skript-Client zählt die Nachrichten: >2 beweist, dass der ERSTE Zug im restaurierten Verlauf liegt.
        Assert.Contains("Nachrichten", second.Text);
        var n = int.Parse(second.Text.Split('(')[1].Split(' ')[0]);
        Assert.True(n >= 3, $"Verlauf ging beim Persist-Roundtrip verloren (nur {n} Nachrichten).");
    }

    [Fact]
    public async Task Approval_Roundtrip_Tool_laeuft_ERST_nach_Zustimmung()
    {
        var invoked = 0;
        var tool = AIFunctionFactory.Create(() => { invoked++; return "AUSGEFÜHRT"; }, "starte_lauf",
            "Startet einen teuren Lauf.");
        var agent = new ScriptedChatClient().AsAIAgent(instructions: "Test", name: "StewardSpike",
            tools: [new ApprovalRequiredAIFunction(tool)]);
        var session = await agent.CreateSessionAsync();

        var first = await agent.RunAsync("Starte bitte den Lauf.", session);
        var request = first.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().Single();
        Assert.Equal(0, invoked);                                          // NICHT ausgeführt vor Zustimmung

        var second = await agent.RunAsync(new ChatMessage(ChatRole.User, [request.CreateResponse(true)]), session);
        Assert.Equal(1, invoked);                                          // Zustimmung ⇒ genau EINE Ausführung
        Assert.Contains("Nachrichten", second.Text);
    }

    [Fact]
    public async Task Kante_5189_Approval_Antwort_NACH_Session_Persist_Roundtrip()
    {
        var invoked = 0;
        var tool = AIFunctionFactory.Create(() => { invoked++; return "AUSGEFÜHRT"; }, "starte_lauf", "Startet.");
        var agent = new ScriptedChatClient().AsAIAgent(instructions: "Test", name: "StewardSpike",
            tools: [new ApprovalRequiredAIFunction(tool)]);
        var session = await agent.CreateSessionAsync();
        var first = await agent.RunAsync("Starte bitte den Lauf.", session);
        var request = first.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().Single();

        // #5189-Kante: Persist-Roundtrip ZWISCHEN Anfrage und Antwort (Steward-Neustart mitten im Approval).
        var json = await agent.SerializeSessionAsync(session);
        var resumed = await agent.DeserializeSessionAsync(json);
        await agent.RunAsync(new ChatMessage(ChatRole.User, [request.CreateResponse(true)]), resumed);

        Assert.Equal(1, invoked);   // PINNT das Verhalten: klappt es, ist C5-Neustart-sicher; bricht es, dokumentieren!
    }
}
