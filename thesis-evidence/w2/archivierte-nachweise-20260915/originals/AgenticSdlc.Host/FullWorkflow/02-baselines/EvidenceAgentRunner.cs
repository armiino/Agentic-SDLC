using System.Diagnostics;
using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;
using AgenticSdlc.Host.FullWorkflow.Ledger;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// Kapitel B (Evidenz-Agent): Artefakt-Generierung aus dem Ledger (Arm B) vs. Rohtranskript (Arm A),
/// gemessen auf Treue statt Güte. Bewusst ein <b>dünner</b>, eigener Runner: er komponiert die bestehenden
/// Bausteine (<see cref="ChatClientFactory"/>, <see cref="AgentChatPipelineBuilder"/>, <see cref="PromptProvider"/>)
/// und lässt <see cref="Phase2Runner"/> / Ledger / Phase2B unangetastet.
/// </summary>
/// <remarks>
/// Transport-Entscheidung (bewusst, für Experiment-Kontrolle): <b>Direkt-Chat</b> statt MCP-fs — die Quelle wird
/// als Nutzer-Nachricht übergeben, das Artefakt aus <c>response.Text</c> geschrieben (wie <c>ReviewAgentRunner</c>).
/// Deterministische Input/Output-Kontrolle, k-Läufe trivial, isoliert die Variable (Ledger vs. Transkript) von
/// Tool-Use-Rauschen. Der Reuse liegt beim <b>Agenten</b> (Prompt + Modell + Pipeline), nicht beim Transport.
///
/// <b>E1 (dieser Stand):</b> Arm A (transcript) real → <c>requirements.md</c>. Arm B (ledger) folgt in E2
/// (Consumer-Contract-Prompt + consumable-Projektion).
/// </remarks>
public sealed class EvidenceAgentRunner
{
    // Artefakt -> (Agent-Prompt-Ordner, Ledger-Disposition-Key). Additiv erweiterbar (architecture/open-questions später).
    private static readonly IReadOnlyDictionary<string, (string Agent, string Disposition)> ArtifactMap =
        new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
        {
            ["requirements"] = ("EvidenceRequirementsAgent", "requirements"),
            ["risks"] = ("EvidenceRisksAgent", "risks"),
            ["architecture"] = ("EvidenceArchitectureAgent", "architecture"),
            ["open-questions"] = ("EvidenceOpenQuestionsAgent", "open-questions"),
        };
    private const string SharedCorePrompt = "_shared-core"; // geteilter Kern (Aufgabe/Format/Treue) — beide Arme identisch
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HostSettings _settings;
    private readonly RunContext _run;
    private readonly string _sourceName;
    private readonly ActivitySource _activitySource;
    private readonly string _repoRoot;

    public EvidenceAgentRunner(
        HostSettings settings, RunContext run, string sourceName, ActivitySource activitySource, string repoRoot)
    {
        _settings = settings;
        _run = run;
        _sourceName = sourceName;
        _activitySource = activitySource;
        _repoRoot = repoRoot;
    }

    public async Task<int> RunAsync()
    {
        const string phase = "phase2_evidence";
        var arm = _settings.EvidenceSource;         // transcript (Arm A) | ledger (Arm B)
        var artifact = _settings.EvidenceArtifact;  // requirements (B-Minimal-Bar)

        _run.AppendEvent(new { type = "RUN_STARTED", runId = _run.RunId, phase, arm, artifact, timestampUtc = DateTime.UtcNow });
        using var span = _activitySource.StartActivity("run.phase2_evidence", ActivityKind.Internal);
        span?.SetTag("run.id", _run.RunId);
        span?.SetTag("evidence.arm", arm);

        Console.WriteLine($"[evidence-agent] arm={arm} artifact={artifact} repetitions={_settings.EvidenceRepetitions}");
        Console.WriteLine($"[evidence-agent] runDir={Path.GetRelativePath(_repoRoot, _run.RunDir)}");

        if (!ArtifactMap.TryGetValue(artifact, out var map))
        {
            Console.Error.WriteLine($"[evidence-agent] artifact='{artifact}' nicht unterstützt (verfügbar: {string.Join(", ", ArtifactMap.Keys)}).");
            return Fail("UNSUPPORTED_ARTIFACT");
        }
        var agentName = map.Agent;
        var dispositionKey = map.Disposition;

        // Quelle auflösen. E1 = Arm A (transcript). Arm B (ledger) folgt in E2.
        string sourceBlock;
        if (arm == "transcript")
        {
            var path = Resolve(_settings.EvidenceTranscript);
            if (path is null || !File.Exists(path))
            {
                Console.Error.WriteLine($"[evidence-agent] Transkript fehlt/ungesetzt: '{_settings.EvidenceTranscript}'. Setze evidenceAgent.transcript.");
                return Fail("TRANSCRIPT_MISSING");
            }
            var transcript = await File.ReadAllTextAsync(path).ConfigureAwait(false);
            sourceBlock = $"TRANSKRIPT:\n{transcript}";
            Console.WriteLine($"[evidence-agent] source=transcript  '{_settings.EvidenceTranscript}' ({transcript.Length} Zeichen)");
        }
        else // ledger (Arm B): die freigegebene consumable.json als Quelle projizieren
        {
            var path = Resolve(_settings.EvidenceLedgerRun);
            if (path is null || !File.Exists(path))
            {
                Console.Error.WriteLine($"[evidence-agent] consumable.json fehlt/ungesetzt: '{_settings.EvidenceLedgerRun}'. Setze evidenceAgent.ledgerRun.");
                return Fail("CONSUMABLE_MISSING");
            }
            var consumable = JsonSerializer.Deserialize<ConsumableLedger>(await File.ReadAllTextAsync(path).ConfigureAwait(false), Json);
            if (consumable is null || consumable.Claims.Count == 0)
            {
                Console.Error.WriteLine("[evidence-agent] consumable.json leer/nicht lesbar.");
                return Fail("CONSUMABLE_EMPTY");
            }
            sourceBlock = "EVIDENCE-LEDGER (freigegebene Claims):\n\n" + ProjectLedger(consumable.Claims, dispositionKey);
            Console.WriteLine($"[evidence-agent] source=ledger  '{_settings.EvidenceLedgerRun}' ({consumable.Claims.Count} Claims)");
        }

        // Agent bauen (Reuse der Bausteine; KEINE MCP-Tools = Direkt-Chat).
        // Prompt-Parität: Arm-Header (arm-spezifisch) + GETEILTER KERN (byte-identisch für beide Arme).
        var vars = new Dictionary<string, string> { ["runId"] = _run.RunId };
        var header = PromptProvider.Load(_repoRoot, phase, agentName, _settings.GetPromptName(agentName), vars);
        var core = PromptProvider.Load(_repoRoot, phase, agentName, SharedCorePrompt, vars);
        var instructions = header.TrimEnd() + "\n\n" + core;
        var agent = BuildAgent(instructions, agentName);

        var k = _settings.EvidenceRepetitions;
        for (var i = 1; i <= k; i++)
        {
            var response = await agent
                .RunAsync([new ChatMessage(ChatRole.User, sourceBlock)], cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
            var text = response.Text ?? string.Empty;

            var fileName = k == 1 ? $"{artifact}.md" : $"{artifact}.{i:D2}.md";
            var outFile = Path.Combine(_run.RunDir, fileName);
            await File.WriteAllTextAsync(outFile, text).ConfigureAwait(false);
            Console.WriteLine($"[evidence-agent] {i}/{k} -> {Path.GetRelativePath(_repoRoot, outFile)} ({text.Length} Zeichen)");
        }

        _run.AppendEvent(new { type = "RUN_COMPLETED", runId = _run.RunId, phase, arm, artifact, repetitions = k, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[evidence-agent] fertig — {k} Lauf/Läufe unter {Path.GetRelativePath(_repoRoot, _run.RunDir)}");
        return 0;
    }

    private AIAgent BuildAgent(string instructions, string agentName)
    {
        IChatClient baseChat = ChatClientFactory.Create(_settings);
        IChatClient chat = AgentChatPipelineBuilder.Build(baseChat, _settings, _run, agentName, _sourceName);
        AIAgent baseAgent = chat.AsAIAgent(instructions: instructions, name: agentName, tools: []);
        var toolLogger = new ToolCallLoggerMiddleware(_run);
        return baseAgent.AsBuilder().Use(toolLogger.InvokeAsync).Build();
    }

    /// <summary>Projiziert die freigegebenen Claims lesbar für den Agenten. Delegiert an das gemeinsame
    /// <see cref="EvidenceLedgerProjection"/>-Modul, damit Direkt-Chat-Runner und MAF-Maker-Executor
    /// byte-identisch dieselbe Quelle sehen (Experiment-Kontrolle).</summary>
    private static string ProjectLedger(IReadOnlyList<SemanticLedgerEntry> claims, string dispositionKey)
        => EvidenceLedgerProjection.Project(claims, dispositionKey);

    private string? Resolve(string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(_repoRoot, p));

    private int Fail(string reason)
    {
        _run.AppendEvent(new { type = "RUN_FAILED", runId = _run.RunId, reason, timestampUtc = DateTime.UtcNow });
        return 4;
    }
}
