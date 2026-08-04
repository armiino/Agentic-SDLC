using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Recipes;

// SP2-Fix (= R-32): das Checker-Verdikt eines Baseline-Artefakts lesbar machen. Der CheckerRepair-Workflow
// schreibt es nach final-report.json ("Rest -> Mensch" bei MaxIterationsReached/HumanReview) — bisher hat es
// im operativen Pfad niemand geoeffnet. Pure Leser-Klasse (testbar); Konsument: BaselineStageExecutor.
public static class BaselineFinalReport
{
    public sealed record Verdict(string Decision, bool Pass);

    public const string FileName = "final-report.json";

    // Liest das Verdikt neben artifact.json. null = Report fehlt/unlesbar (Alt-Laeufe: fail-open, der
    // Aufrufer entscheidet laut). "Pass" kommt aus GateDecision.ToString() des CheckerExecutors.
    public static Verdict? TryRead(string artifactDir)
    {
        var path = Path.Combine(artifactDir, FileName);
        if (!File.Exists(path)) return null;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var decision = doc.RootElement.TryGetProperty("decision", out var d) ? d.GetString() : null;
            if (string.IsNullOrWhiteSpace(decision)) return null;
            var pass = doc.RootElement.TryGetProperty("pass", out var p) && p.ValueKind is JsonValueKind.True;
            return new Verdict(decision!, pass);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // "Rest -> Mensch"? Alles ausser einem bestandenen Checker ist aufzudecken (MaxIterationsReached/HumanReview).
    public static bool NeedsHuman(Verdict? verdict)
        => verdict is not null && !string.Equals(verdict.Decision, "Pass", StringComparison.Ordinal);
}
