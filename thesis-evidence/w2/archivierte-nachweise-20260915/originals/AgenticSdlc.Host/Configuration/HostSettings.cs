using AgenticSdlc.Host.FullWorkflow.Ledger;

namespace AgenticSdlc.Host.Configuration;

/// <summary>
/// Zentrale Runtimeconfig für den Host.
/// </summary>
/// <remarks>
/// Die Klasse kombiniert die "nicht"-geheime `run-config.json` mit lokalen Umgebungsvariablen aus `.env`.
/// Dadurch bleiben Forschungsparameter wie Phase, Modell und Observability sichtbar,
///  während Secrets weiterhin lokal bleiben
/// </remarks>
public sealed record HostSettings(
    string RepoRoot,
    bool OtelEnabled,
    bool OtelSensitive,
    bool OtelRawEnabled,
    bool InnerCycleLogging,
    int LlmPreviewChars,
    int ResponseTextMaxChars,
    ReasoningCapture ReasoningCapture,
    string AgentPhase,
    string Phase2ContextStrategy,
    IReadOnlyDictionary<string, string> Prompts,
    string LlmProvider,
    string ModelId,
    string OllamaBaseUrl,
    string OpenRouterBaseUrl,
    string? OpenRouterApiKey,
    int LlmNetworkTimeoutSeconds,
    bool JuryEnabled,
    string? JuryJudgeModel,
    bool JuryStructuredOutput,
    bool JurySplitGeneration,
    bool JuryVerifyFalseClaim,
    bool JuryVerifyFalseCertainty,
    bool JuryVerifyMissingTopic,
    bool JuryVerifyCustom,
    int JuryMissingTopicBatchSize,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? JuryCategoriesByArtifact,
    bool Phase2BWriteArtifacts,
    IReadOnlyDictionary<string, IReadOnlyList<string>>? Phase2BReads,
    AdjudicationMode LedgerAdjudicationMode,
    bool LedgerAdjudicationOpenBrowser,
    // L3 Open-World-Review: "file" (Datei-Paket) | "interactive" (Review-UI). Default file.
    string L3ReviewMode,
    bool L3ReviewOpenBrowser,
    // L3 Kandidaten-Generierung: "selective" | "exhaustive" | "research" | "coverage" | "agentic-coverage". Default selective.
    string L3CandidateMode,
    // L3 resolve_provenance-Tool an Gen/Resolve anhängen (Herkunft rückverfolgen). Default false (baseline-neutral).
    bool L3ProvenanceTool,
    // Optionaler Ledger-consumable.json-Pfad, damit resolve_provenance sourceClaimIds bis zum Claim-Text auflöst.
    string? L3LedgerRun,
    // Evidenz-Agent (Kapitel B) — nur relevant bei AgentPhase=phase2_evidence.
    string EvidenceSource,
    string EvidenceArtifact,
    string? EvidenceTranscript,
    string? EvidenceLedgerRun,
    int EvidenceRepetitions,
    // M1b: Steward-Memory-Default aus run-config (CLI --memory überstimmt; K6-Modi).
    string? StewardMemoryMode = null,
    bool StewardGithubLive = true,
    // Block-H-Fund 17.08.: das Projekt-Repo (run-config fullworkflow.repo = die EINE Quelle) — damit der
    // Steward sein Repo KENNT (Prompt-Variable {{projektRepo}}) statt den Autor danach zu fragen.
    string? GithubRepo = null)
{
    /// <summary>
    /// Erstellt die Settings aus den aktuell gesetzten Umgebungsvariablen.
    /// </summary>
    public static HostSettings FromRuntimeConfig(RunConfig config, string repoRoot)
    {
        // Phase und Strategie zuerst auflösen, damit die Prompt-Auswahl die aktivePhase->Strategie-Section ziehen kann.
        
        var agentPhase = ReadConfigString(config.AgentPhase, "AGENT_PHASE", "phase1").Trim().ToLowerInvariant();
        var phase2Strategy = ReadConfigString(config.Phase2ContextStrategy, "PHASE2_CONTEXT_STRATEGY", "message_passing").Trim().ToLowerInvariant();
        // Evidenz-Agent-Arm (Kapitel B): wählt bei agentPhase=phase2_evidence die Prompt-Section (analog Strategie bei phase2_1).
        var evidenceSource = (string.IsNullOrWhiteSpace(config.EvidenceAgent.Source) ? "transcript" : config.EvidenceAgent.Source).Trim().ToLowerInvariant();

        var settings = new HostSettings(
            RepoRoot: repoRoot,
            OtelEnabled: config.Observability.EnableOtel ?? ReadFlag("ENABLE_OTEL"),
            OtelSensitive: config.Observability.EnableOtelSensitive ?? ReadFlag("ENABLE_OTEL_SENSITIVE"),
            OtelRawEnabled: config.Observability.EnableOtelRaw ?? ReadFlag("ENABLE_OTEL_RAW"),
            InnerCycleLogging: config.Observability.InnerCycleLogging ?? ReadFlag("INNER_CYCLE_LOGGING"),
            LlmPreviewChars: Math.Clamp(config.LlmPreview.Chars ?? ReadInt("LLM_PREVIEW_CHARS", 800), 100, 8000),
            // W1a: Volltext-Kappung für response-text.md; 0 = unbegrenzt (Forschungs-Default). Getrennt vom Event-Preview.
            ResponseTextMaxChars: Math.Max(0, config.Observability.ResponseTextMaxChars ?? ReadInt("RESPONSE_TEXT_MAX_CHARS", 0)),
            // W1a: reasoning-Steuerung; Default enforced. off = Baseline-vergleichbar (M-1).
            ReasoningCapture: ReasoningCaptureParser.Parse(config.Observability.CaptureReasoning ?? Environment.GetEnvironmentVariable("CAPTURE_REASONING")),
            AgentPhase: agentPhase,
            Phase2ContextStrategy: phase2Strategy,
            Prompts: BuildPromptSelection(config.Prompts, agentPhase, phase2Strategy, evidenceSource),
            LlmProvider: ReadConfigString(config.LlmProvider, "LLM_PROVIDER", "ollama").Trim().ToLowerInvariant(),
            ModelId: ReadConfigString(config.AgentModel, "AGENT_MODEL", "qwen2.5:14b"),
            OllamaBaseUrl: ReadString("OLLAMA_BASE_URL", "http://localhost:11434/"),
            OpenRouterBaseUrl: ReadString("OPENROUTER_BASE_URL", "https://openrouter.ai/api/v1"),
            OpenRouterApiKey: Environment.GetEnvironmentVariable("OPENROUTER_API_KEY"),
            // OpenAI-/OpenRouter-SDK-Default-NetworkTimeout = 100 s; Reasoning-Modelle (z. B. gpt-5.4)
            // brauchen oft mehrere Minuten -> sonst "Retry failed after 4 tries / exceeded timeout 0:01:40".
            // Default 600 s; per LLM_NETWORK_TIMEOUT_SECONDS steuerbar (Clamp 30..3600).
            LlmNetworkTimeoutSeconds: Math.Clamp(ReadInt("LLM_NETWORK_TIMEOUT_SECONDS", 600), 30, 3600),
            JuryEnabled: config.Jury.Enabled ?? false,
            JuryJudgeModel: string.IsNullOrWhiteSpace(config.Jury.JudgeModel) ? null : config.Jury.JudgeModel.Trim(),
            JuryStructuredOutput: config.Jury.StructuredOutput ?? true,
            // DISK-12/G3: Call-1-Split pro Kategorie. Default true (validiert B20).
            JurySplitGeneration: config.Jury.SplitGeneration ?? true,
            // Konservative DISK-7-Defaults, falls der verification-Block fehlt.
            JuryVerifyFalseClaim: config.Jury.Verification?.FalseClaim ?? false,
            JuryVerifyFalseCertainty: config.Jury.Verification?.FalseCertainty ?? true,
            JuryVerifyMissingTopic: config.Jury.Verification?.MissingTopic ?? true,
            JuryVerifyCustom: config.Jury.Verification?.Custom ?? false,
            // DISK-9: MISSING_TOPIC-Batch begrenzen (Default 8, min. 1) gegen Verifier-Kollaps bei großen Listen.
            JuryMissingTopicBatchSize: Math.Max(1, config.Jury.Verification?.MissingTopicBatchSize ?? 8),
            // DISK-12/B22: optionale Kategorie-Overrides je Artefakttyp (null = nur Code-Defaults).
            JuryCategoriesByArtifact: BuildJuryCategories(config.Jury.Categories),
            Phase2BWriteArtifacts: config.Phase2BState.WriteArtifacts ?? true,
            Phase2BReads: BuildPhase2BReads(config.Phase2BState.Reads),
            // Ledger-Adjudikation (Plan §4): Default skip = Baseline-neutral.
            LedgerAdjudicationMode: AdjudicationModeParser.Parse(config.Ledger.AdjudicationMode),
            LedgerAdjudicationOpenBrowser: config.Ledger.AdjudicationOpenBrowser ?? true,
            // L3-Review: Default file (kein UI-Zwang); "interactive" schaltet die Review-UI ein.
            L3ReviewMode: string.Equals(config.L3.ReviewMode?.Trim(), "interactive", StringComparison.OrdinalIgnoreCase) ? "interactive" : "file",
            L3ReviewOpenBrowser: config.L3.ReviewOpenBrowser ?? true,
            // L3-Kandidaten: Default selective; agentic-coverage = Tool-Agent analog L2-Agentic.
            L3CandidateMode: config.L3.CandidateMode?.Trim().ToLowerInvariant() is "exhaustive" or "research" or "coverage" or "agentic-coverage" ? config.L3.CandidateMode!.Trim().ToLowerInvariant() : "selective",
            // L3-Provenienz-Tool: Default false (baseline-neutral); "true" hängt resolve_provenance an Gen + Resolve.
            L3ProvenanceTool: config.L3.ProvenanceTool ?? false,
            L3LedgerRun: string.IsNullOrWhiteSpace(config.L3.LedgerRun) ? null : config.L3.LedgerRun.Trim(),
            // Evidenz-Agent (Kapitel B): Defaults source=transcript (Arm A), artifact=requirements (B-Minimal-Bar).
            EvidenceSource: evidenceSource,
            // M1b: Steward-Memory-Default je Projekt (CLI --memory überstimmt).
            StewardMemoryMode: string.IsNullOrWhiteSpace(config.Steward.Memory) ? null : config.Steward.Memory.Trim(),
            StewardGithubLive: config.Steward.GithubLive ?? true,
            GithubRepo: string.IsNullOrWhiteSpace(config.FullWorkflow.Repo) ? null : config.FullWorkflow.Repo.Trim(),
            EvidenceArtifact: (string.IsNullOrWhiteSpace(config.EvidenceAgent.Artifact) ? "requirements" : config.EvidenceAgent.Artifact).Trim().ToLowerInvariant(),
            EvidenceTranscript: string.IsNullOrWhiteSpace(config.EvidenceAgent.Transcript) ? null : config.EvidenceAgent.Transcript.Trim(),
            EvidenceLedgerRun: string.IsNullOrWhiteSpace(config.EvidenceAgent.LedgerRun) ? null : config.EvidenceAgent.LedgerRun.Trim(),
            EvidenceRepetitions: Math.Clamp(config.EvidenceAgent.Repetitions ?? 1, 1, 20)
        );

        return settings;
    }

    // DISK-12/B22: run-config-Kategorie-Overrides (Artefakttyp -> Kategorien) in den Settings-Typ wandeln.
    private static IReadOnlyDictionary<string, IReadOnlyList<string>>? BuildJuryCategories(
        IDictionary<string, List<string>>? configured)
    {
        if (configured is null || configured.Count == 0)
            return null;

        var map = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var (artifactType, categories) in configured)
        {
            if (string.IsNullOrWhiteSpace(artifactType) || categories is null)
                continue;

            map[artifactType.Trim()] = categories
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToArray();
        }

        return map.Count == 0 ? null : map;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>>? BuildPhase2BReads(
        IDictionary<string, List<string>>? configuredReads)
    {
        if (configuredReads is null || configuredReads.Count == 0)
            return null;

        var reads = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var (agentName, keys) in configuredReads)
        {
            if (string.IsNullOrWhiteSpace(agentName) || keys is null)
                continue;

            reads[agentName.Trim()] = keys
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .ToArray();
        }

        return reads.Count == 0 ? null : reads;
    }

    /// <summary>
    /// Liefert den in `run-config.json` ausgewählten Promptnamen für einen Agenten.
    /// </summary>
    /// <remarks>
    /// Der Host arbeitet dadurch nicht mehr mit phase-spezifischen Prompt-Properties.
    /// Jeder Runner kennt seine Agentnamen und fragt hier nur noch generisch nach,
    /// welcher Prompt für diesen Agenten aktiv ist.
    /// Fehlt ein Eintrag, wird bewusst früh abgebrochen, damit ein Run nicht
    /// versehentlich mit einem falschen oder nicht dokumentierten Prompt startet.
    /// </remarks>
    public string GetPromptName(string agentName)
    {
        if (Prompts.TryGetValue(agentName, out var promptName) && !string.IsNullOrWhiteSpace(promptName))
            return promptName;

        throw new InvalidOperationException(
            $"No prompt configured for agent '{agentName}'. Add an entry under 'prompts' in run-config.json.");
    }

    private static bool ReadFlag(string key, bool defaultValue = false)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return value is null
            ? defaultValue
            : string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
              || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadString(string key, string defaultValue)
        => Environment.GetEnvironmentVariable(key) ?? defaultValue;

    private static string ReadConfigString(string? configValue, string envKey, string defaultValue)
        => string.IsNullOrWhiteSpace(configValue) ? ReadString(envKey, defaultValue) : configValue;

    /// <summary>
    /// Löst die aktive Prompt-Auswahl (Agent -> Promptname) aus der nach Phase -> Strategie -> Agent gruppierten Config auf.
    /// </summary>
    /// <remarks>
    /// Die Code-Defaults halten bestehende/teilweise Configs lauffaehig. Darüber wird die Section der aktiven Phase gelegt: 
    /// fürr phase2_1 die der aktiven <paramref name="phase2Strategy"/>, sonst die Pseudo-Strategie "default".
    ///  Das Ergebnis ist ein flaches Agent->Name-Dict; GetPromptName bleibt dadurch unverändert
    /// und alle Aufrufer (Runner, Factories, Snapshot) funktionieren weiter.
    /// </remarks>
    private static IReadOnlyDictionary<string, string> BuildPromptSelection(
        IDictionary<string, Dictionary<string, Dictionary<string, string>>>? configuredPrompts,
        string agentPhase,
        string phase2Strategy,
        string evidenceSource)
    {
        var prompts = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Phase1SinglePass"] = "Phase1_3Prompt",
            ["Phase2ContextAgent"] = "ContextPrompt2",
            ["Phase2RequirementsAgent"] = "RequirementsPrompt4",
            ["Phase2RisksAgent"] = "RisksPrompt2",
            ["Phase2ArchitectureAgent"] = "ArchitecturePrompt2",
            ["Phase2OpenQuestionsAgent"] = "OpenQuestionsPrompt2",
            // Evidenz-Agent (Kapitel B): Default Arm-A-Prompt; run-config wählt pro Arm (transcript|ledger).
            ["EvidenceRequirementsAgent"] = "RequirementsFromTranscript1",
            ["EvidenceRisksAgent"] = "RisksFromTranscript1",
            // architecture/open-questions: nur Ledger-Arm gebaut (Fan-out Arm B); Default = Ledger-Prompt.
            ["EvidenceArchitectureAgent"] = "ArchitectureFromLedger1",
            ["EvidenceOpenQuestionsAgent"] = "OpenQuestionsFromLedger1"
        };

        if (configuredPrompts is null)
            return prompts;

        // phase2_1 wählt die Section der aktiven Kontextstrategie; phase2_evidence die des aktiven Arms;
        // alle anderen Phasen "default".
        var strategyKey = agentPhase switch
        {
            "phase2_1" => phase2Strategy,
            "phase2_evidence" => evidenceSource,
            _ => "default"
        };

        if (configuredPrompts.TryGetValue(agentPhase, out var byStrategy) && byStrategy is not null
            && byStrategy.TryGetValue(strategyKey, out var byAgent) && byAgent is not null)
        {
            foreach (var (agentName, promptName) in byAgent)
            {
                if (string.IsNullOrWhiteSpace(agentName) || string.IsNullOrWhiteSpace(promptName))
                    continue;

                prompts[agentName.Trim()] = promptName.Trim();
            }
        }

        return prompts;
    }

    private static int ReadInt(string key, int defaultValue)
        => int.TryParse(Environment.GetEnvironmentVariable(key), out var value) ? value : defaultValue;
}
