using System.Text.Json;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// Schreibt die Pro-Stufe-Outputs des Ledger-Workflows nach <c>runs/ledger/&lt;runId&gt;/step-NN-name/</c>.
/// </summary>
/// <remarks>
/// Trennung der Beweis-Tiers (vgl. observability-pipeline.md): Die LLM-Eingabe/-Ausgabe pro Executor
/// landet automatisch über die Observability-Middleware (AgentChatPipelineBuilder) in
/// <c>logs/agents/&lt;executor&gt;/</c> (input-context, chat-decision, response-text, otel). HIER liegen
/// dagegen die fachlichen Stufen-Ergebnisse (geparster Output + Kennzahlen) als zitierbare Artefakte —
/// analog zur Run-Snapshot-Disziplin des Projekts.
/// </remarks>
internal static class LedgerRunArtifacts
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>Legt einen Stufen-Ordner an und schreibt <c>output.json</c> + <c>metrics.json</c>.</summary>
    public static string WriteStep(RunContext run, string stepFolder, object output, object metrics)
    {
        var dir = Path.Combine(run.RunDir, stepFolder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "output.json"), JsonSerializer.Serialize(output, Json));
        File.WriteAllText(Path.Combine(dir, "metrics.json"), JsonSerializer.Serialize(metrics, Json));
        return dir;
    }

    public static T? ReadStepOutput<T>(RunContext run, string stepFolder)
    {
        var path = Path.Combine(run.RunDir, stepFolder, "output.json");
        if (!File.Exists(path)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), Json);
    }
}
