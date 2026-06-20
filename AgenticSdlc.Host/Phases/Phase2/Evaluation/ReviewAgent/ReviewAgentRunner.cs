using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Mcp;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.ReviewAgent;

/// <summary>
/// PILOT (isoliert, leicht verwerfbar): ein ECHTER MAF-<see cref="AIAgent"/> als Reviewer — im
/// Gegensatz zur post-hoc <c>Evaluator : IEvaluator</c>-Jury. Der Agent bekommt fs_read (MCP) und die
/// Aufgabe, EIN Artefakt gegen das Transkript zu prüfen; pro Artefakt ein eigener Agent-Run (4 Runs).
/// Nutzt die normale Observability-Pipeline → Reasoning + Tool-Calls landen in den Run-Logs.
/// </summary>
/// <remarks>
/// Zweck: empirischer Vergleich „Agent-Reviewer" vs. „bounded IEvaluator" (Stabilität, Reasoning,
/// Kosten). Vollständig additiv: eigener Ordner <c>ReviewAgent/</c> + eigenes Kommando + eigener
/// Run-Ordner <c>runs/reviewpilot/</c>. Verwerfen = Ordner löschen + die Dispatch-Zeile in Program.cs.
/// </remarks>
public static class ReviewAgentRunner
{
    private const string SourceName = "AgenticSdlc.Host.ReviewPilot";

    private static readonly string[] DefaultArtifacts =
        ["requirements.md", "risks.md", "architecture.md", "open-questions.md"];

    private const string Instructions = """
        Du bist ein erfahrener Senior-Reviewer fuer fruehe SDLC-Artefakte. Deine Aufgabe: EIN Artefakt
        gegen das originale Stakeholder-Transkript (Ground Truth) pruefen und Fehler finden.

        Vorgehen (denke schrittweise und nachvollziehbar):
        1. Lies ZUERST das vollstaendige Transkript mit dem Tool fs_read (der Pfad wird dir genannt).
           Verlass dich nicht auf Annahmen — pruefe gegen das, was wirklich im Transkript steht.
        2. Pruefe das gegebene Artefakt systematisch auf drei Fehlerarten:
           - FALSE_CLAIM: eine Aussage, die dem Transkript widerspricht oder darin gar nicht vorkommt
             (erfundene Zahlen / Technologien / Fakten).
           - FALSE_CERTAINTY: ein im Transkript OFFENES/strittiges Thema wird als entschieden
             dargestellt. Markiert das Artefakt die Offenheit selbst ("offen", "Annahme", "TBD",
             "noch zu klaeren"), ist es KEIN Fehler.
           - MISSING_TOPIC: ein im Transkript besprochenes, fuer diesen Artefakttyp relevantes Thema
             fehlt im Artefakt komplett.
        3. Fuer jeden Befund: nenne die Artefakt-Stelle, den Transkript-Beleg (oder "nicht vorhanden"),
           die Fehlerart und eine kurze Begruendung. Erfinde keine Befunde; im Zweifel KEIN Befund.
           Beispiele/Optionen in offenen Fragen sind keine FALSE_CLAIMs.

        Gib am Ende eine kurze Zusammenfassung: Anzahl Befunde je Kategorie + Gesamteinschaetzung.
        """;

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: review-agent <phase> <runId> [artifactFileName] [judgeModelOverride] [transcript.txt]");
            return 2;
        }

        var phase = args[1];
        var sourceRunId = args[2];
        string? artifactArg = null, judgeArg = null, transcriptName = null;
        foreach (var a in args.Skip(3))
        {
            if (a.EndsWith(".md", StringComparison.OrdinalIgnoreCase)) artifactArg = a;
            else if (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)) transcriptName = a;
            else judgeArg = a;
        }

        var docsDir = Path.Combine(repoRoot, "runs", phase, sourceRunId, "snapshots", "docs");
        if (!Directory.Exists(docsDir))
        {
            Console.Error.WriteLine($"[review-agent] Snapshots nicht gefunden: {docsDir}");
            return 2;
        }

        transcriptName ??= FirstTranscript(repoRoot);
        if (transcriptName is null)
        {
            Console.Error.WriteLine("[review-agent] Kein Transkript unter input/transcripts/ gefunden.");
            return 2;
        }
        var transcriptRelPath = $"input/transcripts/{transcriptName}";

        var agentSettings = judgeArg is not null ? settings with { ModelId = judgeArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;

        // Eigener, isolierter Run-Ordner fuer die Observe-Logs des Pilots.
        var run = new RunContext(RunId.New(), "reviewpilot");
        run.EnsureFolders();
        var reviewsDir = Path.Combine(run.RunDir, "reviews");
        Directory.CreateDirectory(reviewsDir);

        Console.WriteLine($"[review-agent] Judge-Agent: {agentSettings.LlmProvider} / {agentSettings.ModelId}");
        Console.WriteLine($"[review-agent] Quelle: runs/{phase}/{sourceRunId}  Transkript: {transcriptRelPath}");
        Console.WriteLine($"[review-agent] Observe-Logs: {run.RunDir}");

        await using var localMcp = await McpConnections.ConnectLocalAsync().ConfigureAwait(false);
        var localTools = (await localMcp.ListToolsAsync().ConfigureAwait(false)).Cast<AITool>().ToList();
        Console.WriteLine($"[review-agent] MCP-Tools: {string.Join(", ", localTools.Select(t => t.Name))}");

        var artifacts = artifactArg is not null ? new[] { artifactArg } : DefaultArtifacts;

        foreach (var artifact in artifacts)
        {
            var path = Path.Combine(docsDir, artifact);
            if (!File.Exists(path)) { Console.WriteLine($"[review-agent] uebersprungen (fehlt): {artifact}"); continue; }
            var artifactText = await File.ReadAllTextAsync(path).ConfigureAwait(false);

            var agentName = $"ReviewAgent_{Path.GetFileNameWithoutExtension(artifact)}";
            var agent = BuildAgent(agentSettings, run, agentName, localTools);

            var userMessage =
                $"ARTEFAKT-TYP: {artifact}\n" +
                $"TRANSKRIPT-PFAD (mit fs_read lesen): {transcriptRelPath}\n\n" +
                $"ZU PRUEFENDES ARTEFAKT:\n{artifactText}";

            Console.WriteLine($"\n[review-agent] >>> {artifact} ({agentName}) ...");
            var response = await agent
                .RunAsync([new ChatMessage(ChatRole.User, userMessage)], cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);

            var outFile = Path.Combine(reviewsDir, $"{Path.GetFileNameWithoutExtension(artifact)}.review.md");
            await File.WriteAllTextAsync(outFile, response.Text ?? string.Empty).ConfigureAwait(false);
            Console.WriteLine($"[review-agent] Review: {Path.GetRelativePath(repoRoot, outFile)} ({(response.Text ?? "").Length} Zeichen)");
        }

        run.AppendEvent(new { type = "REVIEW_PILOT_COMPLETED", runId = run.RunId, sourceRunId, judge = agentSettings.ModelId, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"\n[review-agent] Fertig. Reviews + Reasoning-Logs unter {run.RunDir}");
        return 0;
    }

    // Gleiche Pipeline wie die Specialists (Phase2AgentFactory): Observability-Middleware + ToolCallLogger.
    private static AIAgent BuildAgent(HostSettings settings, RunContext run, string agentName, IReadOnlyList<AITool> tools)
    {
        IChatClient baseChat = ChatClientFactory.Create(settings);
        IChatClient chat = AgentChatPipelineBuilder.Build(baseChat, settings, run, agentName, SourceName);
        AIAgent baseAgent = chat.AsAIAgent(instructions: Instructions, name: agentName, tools: [.. tools]);
        var toolLogger = new ToolCallLoggerMiddleware(run);
        return baseAgent.AsBuilder().Use(toolLogger.InvokeAsync).Build();
    }

    private static string? FirstTranscript(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "input", "transcripts");
        if (!Directory.Exists(dir)) return null;
        var f = Directory.GetFiles(dir, "*.txt").OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
        return f is null ? null : Path.GetFileName(f);
    }
}
