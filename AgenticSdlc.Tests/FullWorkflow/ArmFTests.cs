using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.ArmF;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// W2 Arm F v2 "missionsgleich" (Konzept §11/§12.2 v7, 05.09.): kanonischer Claim-Bestand MIT
// Pflicht-Quellenbilanz (je AU genau eine Disposition, deterministisch validiert) + Ebene-C-Zähler.
// Tool-Loop LLM-frei mit skriptetem Client bewiesen (Muster HandoffSpikeTests).
public sealed class ArmFTests
{
    private static IReadOnlyList<AtomicUnit> Units() =>
    [
        new("AU-0001", "test.txt", 0, "Leitung", "Wir brauchen eine Suche über alle Profile."),
        new("AU-0002", "test.txt", 1, "Pflege", "Und die No-Go-Seite braucht ein Stopp-Symbol."),
        new("AU-0003", "test.txt", 2, "Leitung", "Hat jeder einen Kaffee?"),
    ];

    private static ArmFStatement Claim(string id = "C-001", string text = "Die App bietet eine profilübergreifende Suche.",
        string refId = "AU-0001") => new(id, text, "requirement", [refId], "explicit", "none");

    private static IReadOnlyList<ArmFBilanzEintrag> BilanzOk(string claimId = "C-001") =>
    [
        new("AU-0001", "used", [claimId]),
        new("AU-0002", "unresolved"),
        new("AU-0003", "non_relevant"),
    ];

    // ── Vertrag: Claim-Bestand ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Submit_ohne_existierende_AU_Referenz_wird_abgewiesen()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var antwort = Submit(tools, [Claim() with { SourceUnitIds = ["AU-9999"] }], BilanzOk());
        Assert.Contains("RESULT_INVALID", antwort);
        Assert.Contains("AU-9999", antwort);
        Assert.Null(tools.Submitted);
    }

    [Fact]
    public void Submit_prueft_Ids_Typen_Derivation_Unsicherheit_Leere_und_Duplikate()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var antwort = Submit(tools, new[]
        {
            Claim("C-001") with { Type = "wish", Derivation = "guessed", Uncertainty = "maybe" },
            Claim("C-002") with { Statement = " " },
            Claim("") ,                       // id fehlt
            Claim("C-004"), Claim("C-004"),   // id doppelt + exaktes Statement-Duplikat
        }, BilanzOk());
        Assert.Contains("wish", antwort);
        Assert.Contains("guessed", antwort);
        Assert.Contains("maybe", antwort);
        Assert.Contains("statement leer", antwort);
        Assert.Contains("id PFLICHT", antwort);
        Assert.Contains("doppelt vergeben", antwort);
        Assert.Contains("Duplikat", antwort);
        Assert.Null(tools.Submitted);
    }

    // ── Vertrag: Quellenbilanz (F-v2) ──────────────────────────────────────────────────────────

    [Fact]
    public void Bilanz_muss_jede_AU_genau_einmal_disponieren()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var fehltEine = Submit(tools, [Claim()], [new("AU-0001", "used", ["C-001"]), new("AU-0002", "unresolved")]);
        Assert.Contains("AU-0003", fehltEine);
        Assert.Contains("fehlt", fehltEine);

        var doppelt = Submit(tools, [Claim()],
            [new("AU-0001", "used", ["C-001"]), new("AU-0001", "unresolved"), new("AU-0002", "unresolved"), new("AU-0003", "non_relevant")]);
        Assert.Contains("mehrfach disponiert", doppelt);
        Assert.Null(tools.Submitted);
    }

    [Fact]
    public void Bilanz_used_verlangt_existierende_ClaimIds_und_Rest_darf_keine_tragen()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var ohneClaim = Submit(tools, [Claim()],
            [new("AU-0001", "used"), new("AU-0002", "unresolved"), new("AU-0003", "non_relevant")]);
        Assert.Contains("verlangt mind. 1 claimId", ohneClaim);

        var unbekannt = Submit(tools, [Claim()],
            [new("AU-0001", "used", ["C-999"]), new("AU-0002", "unresolved"), new("AU-0003", "non_relevant")]);
        Assert.Contains("C-999", unbekannt);
        Assert.Contains("existiert nicht", unbekannt);

        var widerspruch = Submit(tools, [Claim()],
            [new("AU-0001", "used", ["C-001"]), new("AU-0002", "unresolved", ["C-001"]), new("AU-0003", "non_relevant")]);
        Assert.Contains("widerspruechliche Disposition", widerspruch);
        Assert.Null(tools.Submitted);
    }

    [Fact]
    public void Gueltiger_Submit_speichert_Bestand_UND_Bilanz_und_zweiter_ist_ALREADY_SUBMITTED()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var erste = Submit(tools, [Claim()], BilanzOk());
        Assert.Contains("\"submitted\":true", erste.Replace(" ", ""));
        Assert.NotNull(tools.Submitted);
        Assert.Equal(3, tools.SubmittedBilanz!.Count);

        var zweite = Submit(tools, [Claim("C-002", "Anders.", "AU-0002")], BilanzOk("C-002"));
        Assert.Contains("ALREADY_SUBMITTED", zweite);
        Assert.Single(tools.Submitted!);
    }

    // ── Ebene-C-Prozessprofil (§14.5) ──────────────────────────────────────────────────────────

    [Fact]
    public void Selbstrevision_zaehlt_nur_veraenderte_Drafts()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        Validate(tools, [Claim()]);
        Validate(tools, [Claim()]);                                       // identisch → keine Revision
        Validate(tools, [Claim(), Claim("C-002", "Zweite Aussage.", "AU-0002")]);
        Submit(tools, [Claim()], BilanzOk());                             // wieder verändert
        Assert.Equal(2, tools.SelfRevisions);
        Assert.Equal(3, tools.ValidationCalls);
        Assert.Equal(1, tools.Submissions);
    }

    [Fact]
    public void Budget_blockt_Lesen_und_Pruefen_aber_nie_die_Abgabe()
    {
        var tools = new ArmFTools(Units(), toolBudget: 2);
        Read(tools); Read(tools);
        Assert.Contains("BUDGET_EXHAUSTED", Read(tools));
        Assert.True(tools.BudgetExhausted);
        Assert.Equal(2, tools.SourceReads);

        var abgabe = Submit(tools, [Claim()], BilanzOk());
        Assert.Contains("\"submitted\":true", abgabe.Replace(" ", ""));
    }

    [Fact]
    public void ReadTranscript_liefert_kanonische_AU_Zeilen_im_Bereich()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        Assert.Equal("[AU-0002] Pflege: Und die No-Go-Seite braucht ein Stopp-Symbol.", Read(tools, 2, 2));
    }

    // ── LLM-freier Tool-Loop (In-Process, Scripted-Client) ─────────────────────────────────────

    private sealed class ScriptedArmFClient : IChatClient
    {
        private int _runde;
        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        {
            _runde++;
            ChatMessage antwort = _runde switch
            {
                1 => Call("read_transcript", new() { ["fromUnit"] = 1, ["toUnit"] = 2 }),
                2 => Call("validate_draft", new() { ["statements"] = Statements("AU-9999") }),
                3 => Call("submit_result", new() { ["statements"] = Statements("AU-0001"), ["quellenbilanz"] = Bilanz() }),
                _ => new ChatMessage(ChatRole.Assistant, "FERTIG."),
            };
            return Task.FromResult(new ChatResponse(antwort));
        }
        private ChatMessage Call(string tool, Dictionary<string, object?> args)
            => new(ChatRole.Assistant, [new FunctionCallContent($"call-{_runde}", tool, args)]);
        private static JsonElement Statements(string unitId) => JsonSerializer.SerializeToElement(new[]
        { new { id = "C-001", statement = "Die App bietet eine profilübergreifende Suche.", type = "requirement",
                sourceUnitIds = new[] { unitId }, derivation = "explicit", uncertainty = "none" } });
        private static JsonElement Bilanz() => JsonSerializer.SerializeToElement(new object[]
        { new { auId = "AU-0001", disposition = "used", claimIds = new[] { "C-001" } },
          new { auId = "AU-0002", disposition = "unresolved" },
          new { auId = "AU-0003", disposition = "non_relevant" } });
        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
        public object? GetService(Type serviceType, object? serviceKey = null) => null;
        public void Dispose() { }
    }

    [Fact]
    public async Task Tool_Loop_faehrt_Lesen_Pruefen_Korrigieren_Abgeben_mit_Bilanz()
    {
        var tools = new ArmFTools(Units(), toolBudget: 40);
        var rounds = new ModelRoundCounter(new ScriptedArmFClient());
        var agent = new ChatClientBuilder(rounds).UseFunctionInvocation().Build()
            .AsAIAgent(instructions: "Testlauf.", name: "ArmFAgent", tools: [.. tools.Build()]);

        await agent.RunAsync([new ChatMessage(ChatRole.User, "los")]);

        Assert.NotNull(tools.Submitted);
        Assert.Equal(3, tools.SubmittedBilanz!.Count);
        Assert.Equal(4, rounds.Rounds);
        Assert.Equal(3, tools.ToolCalls);
        Assert.Equal(1, tools.SourceReads);
        Assert.Equal(1, tools.ValidationCalls);
        Assert.Equal(1, tools.Submissions);
        Assert.Equal(1, tools.SelfRevisions);
        Assert.False(tools.BudgetExhausted);
    }

    // ── Ebene-B-Token-Summe ────────────────────────────────────────────────────────────────────

    [Fact]
    public void SumTokens_liest_chat_Spans_auch_mit_String_Tags_und_meldet_fehlenden_Beleg_als_null()
    {
        var dir = Directory.CreateTempSubdirectory("armf-otel");
        try
        {
            var path = Path.Combine(dir.FullName, "otel-traces.jsonl");
            File.WriteAllLines(path,
            [
                """{"name":"chat gpt","tags":{"gen_ai.usage.input_tokens":100,"gen_ai.usage.output_tokens":"25"}}""",
                """{"name":"chat gpt","tags":{"gen_ai.usage.input_tokens":40,"gen_ai.usage.output_tokens":5}}""",
                """{"name":"invoke_tool","tags":{"gen_ai.usage.input_tokens":999}}""",
                "nicht-json",
            ]);
            Assert.Equal((140L, 30L), ArmFRunner.SumTokens(path));
            Assert.Equal(((long?)null, (long?)null), ArmFRunner.SumTokens(Path.Combine(dir.FullName, "fehlt.jsonl")));
        }
        finally { dir.Delete(recursive: true); }
    }

    // ── Helfer (AIFunction-Schicht — bindet Argumente wie im echten Loop) ──────────────────────

    private static string Rufe(ArmFTools tools, string name, AIFunctionArguments args)
    {
        var tool = tools.Build().OfType<AIFunction>().Single(t => t.Name == name);
        var raw = tool.InvokeAsync(args).GetAwaiter().GetResult();
        return JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(raw))!;
    }
    private static string Submit(ArmFTools t, IReadOnlyList<ArmFStatement> s, IReadOnlyList<ArmFBilanzEintrag> b)
        => Rufe(t, "submit_result", new AIFunctionArguments { ["statements"] = s, ["quellenbilanz"] = b });
    private static string Validate(ArmFTools t, IReadOnlyList<ArmFStatement> s)
        => Rufe(t, "validate_draft", new AIFunctionArguments { ["statements"] = s });
    private static string Read(ArmFTools t, int? from = null, int? to = null)
    {
        var a = new AIFunctionArguments();
        if (from is not null) a["fromUnit"] = from;
        if (to is not null) a["toUnit"] = to;
        return Rufe(t, "read_transcript", a);
    }
}
