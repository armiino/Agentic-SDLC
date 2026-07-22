using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI.Evaluation;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Führt den <see cref="Evaluator"/> (Synthesis-Schema) nach einem erfolgreichen
/// Phase-2.1-Run automatisch über alle erzeugten Artefakte aus.
/// </summary>
/// <remarks>
/// Läuft offline nach Workflow + Validation, greift auf die bereits kopierten
/// Snapshots in <c>snapshots/docs/</c> zu.
/// Ergebnisse landen in <c>jury/...evaluator.json</c>.
/// Konfigurierbar über <c>run-config.json</c>: <c>jury.enabled</c> und <c>jury.judgeModel</c>.
/// Ein Jury-Fehler bricht den Run nicht ab..der Workflow-Exit-Code bleibt 0.
/// </remarks>
public sealed class Phase2JuryRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static readonly string[] ArtifactFileNames =
    [
        "requirements.md",
        "risks.md",
        "architecture.md",
        "open-questions.md"
    ];

    private readonly HostSettings _settings;
    private readonly RunContext _run;
    private readonly string _repoRoot;

    public Phase2JuryRunner(HostSettings settings, RunContext run, string repoRoot)
    {
        _settings = settings;
        _run = run;
        _repoRoot = repoRoot;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var transcript = LoadTranscript();
        if (transcript is null)
        {
            _run.AppendEvent(new
            {
                type = "JURY_SKIPPED",
                runId = _run.RunId,
                reason = "Kein Transkript unter input/transcripts/ gefunden.",
                timestampUtc = DateTime.UtcNow
            });
            return;
        }

        var judgeSettings = string.IsNullOrWhiteSpace(_settings.JuryJudgeModel)
            ? _settings
            : _settings with { ModelId = _settings.JuryJudgeModel };

        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var chatClient = ChatClientFactory.Create(judgeSettings);
        var chatConfiguration = new ChatConfiguration(chatClient);

        Directory.CreateDirectory(_run.JuryDir);

        // Ausgelagerte Judge-Prompts einmal laden (DISK-7).
        var juryPrompts = JuryPromptLoader.Load(_repoRoot);
        var verification = new JuryVerificationPolicy(
            FalseClaim: _settings.JuryVerifyFalseClaim,
            FalseCertainty: _settings.JuryVerifyFalseCertainty,
            MissingTopic: _settings.JuryVerifyMissingTopic,
            Custom: _settings.JuryVerifyCustom,
            MissingTopicBatchSize: _settings.JuryMissingTopicBatchSize);
        var categoryProfile = new JuryCategoryProfile(_settings.JuryCategoriesByArtifact);

        var scores = new List<object>();

        foreach (var artifactFileName in ArtifactFileNames)
        {
            var artifactPath = Path.Combine(_run.DocsSnapshotDir, artifactFileName);
            if (!File.Exists(artifactPath))
                continue;

            var artifactText = await File.ReadAllTextAsync(artifactPath, cancellationToken).ConfigureAwait(false);
            var baseName = Path.GetFileNameWithoutExtension(artifactFileName);

            var evaluator = new Evaluator(
                EvaluationScheme.Synthesis, transcript, artifactFileName,
                useStructuredOutput: _settings.JuryStructuredOutput,
                verification: verification,
                prompts: juryPrompts,
                splitGeneration: _settings.JurySplitGeneration,
                categoryProfile: categoryProfile);

            var result = await evaluator
                .EvaluateAsync(artifactText, chatConfiguration, null, cancellationToken)
                .ConfigureAwait(false);

            var summary = BuildSummary(artifactFileName, judgeSettings, evaluator, result);

            var outFile = Path.Combine(_run.JuryDir, $"{baseName}.synth.{modelSlug}.evaluator.json");
            await File.WriteAllTextAsync(
                outFile,
                JsonSerializer.Serialize(summary, JsonOptions),
                cancellationToken)
                .ConfigureAwait(false);

            // Bei nicht parsebarer Judge-Antwort: rohe Antwort persistieren (B8) — gemeinsame Logik.
            await EvaluatorOutput
                .WriteRawFailIfNeededAsync(_run.JuryDir, $"{baseName}.synth.{modelSlug}", evaluator, cancellationToken)
                .ConfigureAwait(false);

            // DISK-9: rohe Verifier-Antworten pro Kategorie sichern (Kalibrierungs-Evidenz).
            await EvaluatorOutput
                .WriteVerifierRawLogsAsync(_run.JuryDir, $"{baseName}.synth.{modelSlug}", evaluator, cancellationToken)
                .ConfigureAwait(false);

            // DISK-12: rohe Generierungs-Antworten pro Kategorie sichern (Call-1-Recall pruefbar).
            await EvaluatorOutput
                .WriteGenerationRawLogsAsync(_run.JuryDir, $"{baseName}.synth.{modelSlug}", evaluator, cancellationToken)
                .ConfigureAwait(false);

            var score = EvaluatorOutput.ReadScore(result);
            scores.Add(new
            {
                mode = "synthesis",
                artifact = artifactFileName,
                evaluationStatus = score.EvaluationStatus,
                errorScore = score.ErrorScore,
                needsRepair = score.NeedsRepair,
                file = Path.GetRelativePath(_repoRoot, outFile).Replace('\\', '/')
            });
        }

        _run.AppendEvent(new
        {
            type = "JURY_COMPLETED",
            runId = _run.RunId,
            judgeModel = judgeSettings.ModelId,
            artifacts = scores,
            timestampUtc = DateTime.UtcNow
        });
    }

    private string? LoadTranscript()
    {
        var transcriptDir = Path.Combine(_repoRoot, "input", "transcripts");
        if (!Directory.Exists(transcriptDir))
            return null;

        var file = Directory.GetFiles(transcriptDir, "*.txt")
            .OrderBy(f => f, StringComparer.Ordinal)
            .FirstOrDefault();

        return file is null ? null : File.ReadAllText(file);
    }

    private static object BuildSummary(
        string artifactFileName,
        HostSettings judgeSettings,
        Evaluator evaluator,
        EvaluationResult result)
    {
        var score = EvaluatorOutput.ReadScore(result);

        return new
        {
            evaluator = "Phase2JuryRunner",
            scheme = evaluator.Scheme.ToString(),
            artifact = artifactFileName,
            judge = new { provider = judgeSettings.LlmProvider, model = judgeSettings.ModelId },
            timestampUtc = DateTime.UtcNow,
            // evaluationStatus="failed" => errorScore ist NICHT aussagekraeftig (Judge nicht parsebar).
            evaluationStatus = score.EvaluationStatus,
            errorScore = score.ErrorScore,
            needsRepair = score.NeedsRepair,
            errorScoreReason = score.Reason,
            parseError = evaluator.LastParseError,
            categories = EvaluatorOutput.BuildCategories(evaluator, result)
        };
    }

}
