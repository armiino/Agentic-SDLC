using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Artifacts;

/// <summary>
/// I-a-CLI: <c>assign-artifact-ids &lt;artifact.md&gt; [--type requirements|risks|architecture|open-questions]
///   [out.json]</c>.
/// Deterministisches ID-Gate: parst ein geprüftes Baseline-Artefakt und schreibt das strukturierte
/// <see cref="ArtifactDocument"/> mit stabilen Item-IDs nach <c>artifact.json</c> (= zugleich der E-e-Kern).
/// KEIN LLM, kein Run-Dir — reine, reproduzierbare Transformation. Exit: 0 = ok, 2 = Usage/IO.
/// </summary>
/// <remarks>
/// Standalone-Utility zum Bauen/Testen; produktiv wird dieselbe <see cref="ArtifactIdGate.Assign"/>-Logik als
/// Executor hinter den CheckerRepair-Knoten gehängt (E-c/E-d), wo <see cref="ProducerMetadata"/> aus dem echten
/// Maker-Kontext kommt. Hier best-effort: runId = neuer Gate-Lauf, model = Config-Modell.
/// </remarks>
public static class ArtifactIdGateRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: assign-artifact-ids <artifact.md> [--type requirements|risks|architecture|open-questions] [out.json]");
            return 2;
        }

        var artPath = Resolve(repoRoot, args[1]);
        if (artPath is null || !File.Exists(artPath))
        {
            Console.Error.WriteLine($"[assign-artifact-ids] artifact fehlt: {args[1]}");
            return 2;
        }

        var artifactType = settings.EvidenceArtifact;
        string? outArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--type", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                artifactType = args[++i].Trim().ToLowerInvariant();
            else if (args[i].StartsWith("--", StringComparison.Ordinal)) { /* ignorieren */ }
            else if (outArg is null) outArg = args[i];
        }

        var markdown = await File.ReadAllTextAsync(artPath).ConfigureAwait(false);
        var producer = new ProducerMetadata(RunId.New(), settings.ModelId, PromptVersion: null);
        var doc = ArtifactIdGate.Assign(markdown, artifactType, producer);

        var outPath = outArg is not null
            ? Resolve(repoRoot, outArg)!
            : Path.Combine(Path.GetDirectoryName(artPath) ?? ".", "artifact.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(doc, Json)).ConfigureAwait(false);

        Console.WriteLine($"[assign-artifact-ids] type={doc.ArtifactType} artifactId={doc.ArtifactId} items={doc.Items.Count} stage={doc.Stage}");
        Console.WriteLine($"[assign-artifact-ids] -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
