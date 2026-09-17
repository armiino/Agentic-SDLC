using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// 1g-E (19.08., core-analyst-design/Abschluss-Liste): HANDOFF-SPIKE — die letzte unerprobte Zelle der
// MAF-Feature-Matrix, LLM-frei bewiesen (R-38-Lehre: Docs behaupten, der Spike beweist; Muster
// StewardFoundationSpikeTests). Quelle: learn.microsoft.com/agent-framework/workflows/orchestrations/handoff
// (C#: AgentWorkflowBuilder.CreateHandoffBuilderWith + WithHandoffs; Handoff = injiziertes TOOL, dessen
// Aufruf die Kontrolle VOLL an den Ziel-Agenten übergibt; Mechanik wird aus der History gefiltert).
// Szenario: steward_triage → core_analyst (das Manager-Paar der Thesis). BEWUSSTE GRENZE: der Spike beweist
// das MUSTER — der Steward bleibt agent-as-tools (K7); Handoff-vs-Delegation = möglicher Nach-W2-Messarm.
public sealed class HandoffSpikeTests
{
    /// <summary>Skripteter Client: sieht die (vom Framework injizierten) Tools und antwortet deterministisch —
    /// Triage ruft das Handoff-Tool Richtung Analyst, der Analyst antwortet mit Text.</summary>
    private sealed class ScriptedHandoffClient(string role, List<string> seenToolNames, List<string> seenUserTexts) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            // Spike-Frage 2 (Laufzeit-Befund): ALLE Tool-SORTEN erfassen — das Handoff-Tool könnte eine
            // Declaration-only-AITool sein (der Executor fängt den Call selbst ab, kein lokales Invoke).
            foreach (var t in options?.Tools ?? []) seenToolNames.Add($"{role}:{t.GetType().Name}:{t.Name}:{t.Description}");
            seenUserTexts.AddRange(messages.Where(m => m.Role == ChatRole.User).Select(m => $"{role}:{m.Text}"));
            seenUserTexts.AddRange(messages.SelectMany(m => m.Contents.OfType<FunctionCallContent>())
                .Select(c => $"{role}:SAW-CALL:{c.Name}"));

            if (role == "triage")
            {
                var handoff = (options?.Tools ?? [])
                    .FirstOrDefault(t => t.Name.Contains("analyst", StringComparison.OrdinalIgnoreCase)
                                      || t.Name.Contains("handoff", StringComparison.OrdinalIgnoreCase));
                if (handoff is null)
                    return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                        $"KEIN-HANDOFF-TOOL (gesehen: {string.Join(",", (options?.Tools ?? []).Select(t => t.Name))} | instr: {options?.Instructions})")));
                var call = new ChatMessage(ChatRole.Assistant,
                    [new FunctionCallContent("call-1", handoff.Name,
                        new Dictionary<string, object?> { ["targetId"] = "core-analyst", ["reason"] = "Analyse-Auftrag" })]);
                return Task.FromResult(new ChatResponse(call));
            }
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant,
                "ANALYST-ANTWORT: Lücke gefunden — Einwilligung fehlt.")));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => Stream(messages, options, cancellationToken);
        private async IAsyncEnumerable<ChatResponseUpdate> Stream(IEnumerable<ChatMessage> messages, ChatOptions? options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
        {
            var response = await GetResponseAsync(messages, options, ct).ConfigureAwait(false);
            foreach (var m in response.Messages)
                yield return new ChatResponseUpdate(m.Role, m.Contents) { MessageId = m.MessageId };
        }
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    [Fact]
    public async Task Handoff_Spike_Triage_uebergibt_an_den_Analysten_und_Kontext_reist_mit()
    {
        var toolNames = new List<string>();
        var userTexts = new List<string>();

        var triage = new ScriptedHandoffClient("triage", toolNames, userTexts).AsAIAgent(new ChatClientAgentOptions
        {
            Id = "steward-triage", Name = "steward_triage",
            Description = "Routet Anliegen an den passenden Spezialisten",
            ChatOptions = new() { Instructions = "Route immer weiter." },
        });
        var analyst = new ScriptedHandoffClient("analyst", toolNames, userTexts).AsAIAgent(new ChatClientAgentOptions
        {
            Id = "core-analyst", Name = "core_analyst",
            Description = "Findet Lücken im Projektstand",
            ChatOptions = new() { Instructions = "Analysiere." },
        });

        // Spike-Frage 1: existiert die offizielle Builder-API auf 1.15.0?
        var workflow = AgentWorkflowBuilder.CreateHandoffBuilderWith(triage)
            .WithHandoffs(triage, [analyst])
            .Build();

        List<ChatMessage> messages = [new(ChatRole.User, "Analysiere den Core auf Lücken.")];
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        List<ChatMessage>? output = null;
        var executorIds = new List<string>();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent u) executorIds.Add(u.ExecutorId);   // 1.15-Name (Doku nennt teils AgentRunUpdateEvent)
            else if (evt is WorkflowOutputEvent o) { output = o.As<List<ChatMessage>>(); break; }
            else if (evt is WorkflowErrorEvent err) Assert.Fail($"WorkflowErrorEvent: {err.Data}");
        }

        // Frage 3: die Übergabe fand statt — der ANALYST hat geantwortet und seine Antwort ist im Output.
        Assert.NotNull(output);
        Assert.Contains(output!, m => m.Text?.Contains("ANALYST-ANTWORT") == true);

        // Frage 2 (Laufzeit-BEFUND, weicht von der Doku-Erzählung ab — Spike-Wert!): das injizierte Tool
        // ist eine Declaration-only-AIFunctionDeclaration (kein lokales Invoke — der HandoffAgentExecutor
        // fängt den Call ab) und heißt POSITIONAL `handoff_to_<n>`, NICHT nach dem Ziel-Agenten; die
        // ZIEL-ERKENNUNG fürs Modell läuft über die DESCRIPTION (= Description des Ziel-Agenten).
        Assert.Contains(toolNames, n => n.StartsWith("triage:", StringComparison.Ordinal)
                                     && n.Contains("FunctionDeclaration", StringComparison.Ordinal)
                                     && n.Contains("handoff_to", StringComparison.Ordinal));
        Assert.Contains(toolNames, n => n.StartsWith("triage:", StringComparison.Ordinal)
                                     && n.Contains("Lücken im Projektstand", StringComparison.Ordinal));

        // Frage 4: der Kontext reist mit — der Analyst sah die ursprüngliche User-Frage.
        Assert.Contains(userTexts, t => t.StartsWith("analyst:", StringComparison.Ordinal) && t.Contains("Lücken"));

        // Frage 5 (Laufzeit-BEFUND, präziser als die Doku-Erzählung): die Handoff-Mechanik wird aus der
        // AGENTEN-SICHT gefiltert (der Analyst sah den Call NIE — Doku-Behauptung „filtered before
        // forwarding" stimmt) — aber der WORKFLOW-OUTPUT enthält den Call noch (Konsument muss selbst
        // filtern, z. B. bevor er den Verlauf persistiert/anzeigt). Beides gepinnt:
        Assert.DoesNotContain(userTexts, t => t.StartsWith("analyst:SAW-CALL", StringComparison.Ordinal));
        Assert.Contains(output!, m => m.Contents.OfType<FunctionCallContent>()
            .Any(c => c.Name.Contains("handoff", StringComparison.OrdinalIgnoreCase)));
    }
}
