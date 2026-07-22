using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>
/// NUR Extraktion+Canonicalization eines Semantic Ledgers aus einem Transkript — ohne Fixture, ohne Match.
/// Zweck: Kandidaten-Ledger fuer ein NEUES Transkript erzeugen, aus dem dann manuell eine bestaetigte
/// Fixture abgeleitet wird (Generalisierungstest des evidence-first-Substrats, vgl.
/// NextStep/ZUSATZ-entscheidungsnotiz-ledger-review.md, Schritt 1).
/// </summary>
public static class SemanticLedgerExtractRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: semantic-ledger-extract <transcript.txt> [modelOverride]");
            return 2;
        }

        var transcriptPath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var modelArg = args.Length >= 3 ? args[2] : null;
        if (!File.Exists(transcriptPath)) { Console.Error.WriteLine($"[ledger-extract] Transkript fehlt: {transcriptPath}"); return 2; }

        var transcript = await File.ReadAllTextAsync(transcriptPath).ConfigureAwait(false);
        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var client = ChatClientFactory.Create(judgeSettings);

        Console.WriteLine($"[ledger-extract] Transcript={Path.GetRelativePath(repoRoot, transcriptPath)} Model={judgeSettings.ModelId}");

        var extractor = new SemanticLedgerExtractor(client, settings.JuryStructuredOutput);
        var canonicalizer = new SemanticLedgerCanonicalizer(client, settings.JuryStructuredOutput);

        Console.WriteLine("[ledger-extract] extracting ledger...");
        var extracted = await extractor.ExtractAsync(transcript, CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"[ledger-extract] extracted={extracted.Count}");

        Console.WriteLine("[ledger-extract] canonicalizing ledger...");
        var canonical = await canonicalizer.CanonicalizeAsync(extracted, CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine($"[ledger-extract] canonical={canonical.Count}");

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike");
        Directory.CreateDirectory(outDir);
        var transcriptStem = Path.GetFileNameWithoutExtension(transcriptPath);
        var prefix = $"semantic-ledger-extract.{transcriptStem}.{modelSlug}";
        var extractedPath = Path.Combine(outDir, $"{prefix}.extracted.json");
        var canonicalPath = Path.Combine(outDir, $"{prefix}.canonical.json");

        await File.WriteAllTextAsync(extractedPath, JsonSerializer.Serialize(extracted, JsonOptions)).ConfigureAwait(false);
        await File.WriteAllTextAsync(canonicalPath, JsonSerializer.Serialize(canonical, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[ledger-extract] extracted -> {Path.GetRelativePath(repoRoot, extractedPath)}");
        Console.WriteLine($"[ledger-extract] canonical -> {Path.GetRelativePath(repoRoot, canonicalPath)}");
        Console.WriteLine("[ledger-extract] NOTE: kein Fixture-Match. Naechster Schritt: ~10 kritische canonical-Eintraege");
        Console.WriteLine("[ledger-extract]       manuell zu input/eval-labels/semantic-ledger-interview-spike.json bestaetigen.");
        return 0;
    }
}