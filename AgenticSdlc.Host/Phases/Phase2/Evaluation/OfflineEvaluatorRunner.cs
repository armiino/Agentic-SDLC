using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using Microsoft.Extensions.AI.Evaluation;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation;

/// <summary>
/// Offline-Evaluator: bewertet ein BEREITS erzeugtes Artefakt eines bestehenden Runs mit dem
/// <see cref="Evaluator"/> (Synthesis) — OHNE Neugenerierung.
/// </summary>
/// <remarks>
/// Anwendungsfaelle: einen alten/eingefrorenen Run nach einer Jury-/Prompt-Aenderung neu scoren oder ein
/// anderes Judge-Modell testen, ohne Generierungskosten. Ergebnisformat ist identisch zur Auto-Jury
/// (<see cref="Phase2JuryRunner"/>) — gemeinsame Logik in <see cref="EvaluatorOutput"/>, kein Drift.
/// Die Ausgabedatei traegt denselben Namen wie die Auto-Jury (<c>&lt;artefakt&gt;.synth.&lt;modell&gt;.evaluator.json</c>),
/// ein Re-Score mit demselben Judge ueberschreibt also den vorhandenen Bericht; ein anderer Judge erzeugt
/// eine eigene Datei (anderer modelSlug).
///
/// Aufruf:
///   dotnet run --project AgenticSdlc.Host -- eval-offline &lt;phase&gt; &lt;runId&gt; &lt;artifactFileName&gt; [transcriptName] [judgeModelOverride]
/// Beispiel:
///   ... -- eval-offline phase2B 20260614_135735_5b6776 requirements.md T9999_chaos.txt openai/gpt-4.1
/// Achtung: echter LLM-Call (Kosten) — nur auf explizite Ausfuehrung.
/// </remarks>
public static class OfflineEvaluatorRunner
{
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: eval-offline <phase> <runId> <artifactFileName> [transcriptName] [judgeModelOverride]");
            return 2;
        }

        var phase = args[1];
        var runId = args[2];
        var artifactFileName = args[3];

        var runDir = Path.Combine(repoRoot, "runs", phase, runId);
        var artifactPath = Path.Combine(runDir, "snapshots", "docs", artifactFileName);
        if (!File.Exists(artifactPath))
        {
            Console.Error.WriteLine($"[eval-offline] Artefakt nicht gefunden: {artifactPath}");
            return 2;
        }

        var transcriptName = args.Length >= 5 ? args[4] : FindFirstTranscript(repoRoot);
        if (transcriptName is null)
        {
            Console.Error.WriteLine("[eval-offline] Kein Transkript unter input/transcripts/ gefunden.");
            return 2;
        }

        var transcriptPath = Path.Combine(repoRoot, "input", "transcripts", transcriptName);
        if (!File.Exists(transcriptPath))
        {
            Console.Error.WriteLine($"[eval-offline] Transkript nicht gefunden: {transcriptPath}");
            return 2;
        }

        var artifactText = await File.ReadAllTextAsync(artifactPath).ConfigureAwait(false);
        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);

        // Optionaler Judge-Modell-Override (6. Argument): anderes Judge-Modell testen, ohne run-config.json
        // zu aendern. Sonst gilt das Modell aus den Settings.
        var judgeSettings = args.Length >= 6 ? settings with { ModelId = args[5] } : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');

        var chatClient = ChatClientFactory.Create(judgeSettings);
        var chatConfiguration = new ChatConfiguration(chatClient);

        var juryDir = Path.Combine(runDir, "jury");
        Directory.CreateDirectory(juryDir);
        var baseName = Path.GetFileNameWithoutExtension(artifactFileName);
        Console.WriteLine($"[eval-offline] Bewerte '{artifactFileName}' aus {phase}/{runId}");
        Console.WriteLine($"[eval-offline] Transkript: {transcriptName}");
        var verification = new JuryVerificationPolicy(
            FalseClaim: settings.JuryVerifyFalseClaim,
            FalseCertainty: settings.JuryVerifyFalseCertainty,
            MissingTopic: settings.JuryVerifyMissingTopic,
            Custom: settings.JuryVerifyCustom,
            MissingTopicBatchSize: settings.JuryMissingTopicBatchSize);
        var categoryProfile = new JuryCategoryProfile(settings.JuryCategoriesByArtifact);

        var juryPrompts = JuryPromptLoader.Load(repoRoot);

        Console.WriteLine($"[eval-offline] Judge: {judgeSettings.LlmProvider} / {judgeSettings.ModelId} (structuredOutput={settings.JuryStructuredOutput})");
        Console.WriteLine($"[eval-offline] Verification: falseClaim={verification.FalseClaim} falseCertainty={verification.FalseCertainty} missingTopic={verification.MissingTopic} custom={verification.Custom}");

        var fileBase = $"{baseName}.synth.{modelSlug}";
        var evaluator = new Evaluator(
            EvaluationScheme.Synthesis, transcript, artifactFileName,
            useStructuredOutput: settings.JuryStructuredOutput,
            verification: verification,
            prompts: juryPrompts,
            splitGeneration: settings.JurySplitGeneration,
            categoryProfile: categoryProfile);

        var result = await evaluator
            .EvaluateAsync(artifactText, chatConfiguration, null, CancellationToken.None)
            .ConfigureAwait(false);

        var summary = BuildSummary(phase, runId, artifactFileName, transcriptName, judgeSettings, evaluator, result);
        var outFile = Path.Combine(juryDir, $"{fileBase}.evaluator.json");
        await File.WriteAllTextAsync(
                outFile,
                JsonSerializer.Serialize(summary, new JsonSerializerOptions { WriteIndented = true }))
            .ConfigureAwait(false);

        await EvaluatorOutput
            .WriteRawFailIfNeededAsync(juryDir, fileBase, evaluator, CancellationToken.None)
            .ConfigureAwait(false);

        // DISK-9: rohe Verifier-Antworten pro Kategorie sichern (Vorher/Nachher-Kalibrierung).
        await EvaluatorOutput
            .WriteVerifierRawLogsAsync(juryDir, fileBase, evaluator, CancellationToken.None)
            .ConfigureAwait(false);

        // DISK-12: rohe Generierungs-Antworten pro Kategorie sichern (Call-1-Recall pruefbar).
        await EvaluatorOutput
            .WriteGenerationRawLogsAsync(juryDir, fileBase, evaluator, CancellationToken.None)
            .ConfigureAwait(false);

        var score = EvaluatorOutput.ReadScore(result);
        var counts = string.Join("  ", evaluator.Categories
            .Select(c => $"{c.Label}={result.Get<NumericMetric>(c.MetricName).Value}"));

        Console.WriteLine();
        Console.WriteLine($"[eval-offline][synthesis] status={score.EvaluationStatus}  ErrorScore={score.ErrorScore}  needsRepair={score.NeedsRepair}");
        Console.WriteLine($"[eval-offline][synthesis] {counts}");
        if (score.EvaluationStatus != "ok")
            Console.WriteLine($"[eval-offline][synthesis] WARN: evaluation_failed -> {fileBase}.rawfail.txt (parseError: {evaluator.LastParseError})");
        Console.WriteLine($"[eval-offline][synthesis] Bericht: {Path.GetRelativePath(repoRoot, outFile)}");

        return 0;
    }

    private static object BuildSummary(
        string phase,
        string runId,
        string artifact,
        string transcriptName,
        HostSettings judgeSettings,
        Evaluator evaluator,
        EvaluationResult result)
    {
        var score = EvaluatorOutput.ReadScore(result);

        return new
        {
            evaluator = "OfflineEvaluatorRunner",
            scheme = evaluator.Scheme.ToString(),
            phase,
            runId,
            artifact,
            transcript = transcriptName,
            judge = new { provider = judgeSettings.LlmProvider, model = judgeSettings.ModelId },
            timestampUtc = DateTime.UtcNow,
            evaluationStatus = score.EvaluationStatus,
            errorScore = score.ErrorScore,
            needsRepair = score.NeedsRepair,
            errorScoreReason = score.Reason,
            parseError = evaluator.LastParseError,
            categories = EvaluatorOutput.BuildCategories(evaluator, result)
        };
    }

    private static string? FindFirstTranscript(string repoRoot)
    {
        var dir = Path.Combine(repoRoot, "input", "transcripts");
        if (!Directory.Exists(dir))
            return null;

        var file = Directory.GetFiles(dir, "*.txt").OrderBy(f => f, StringComparer.Ordinal).FirstOrDefault();
        return file is null ? null : Path.GetFileName(file);
    }
}
