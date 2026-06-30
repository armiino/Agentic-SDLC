using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Human-Artefakt-Spike: claims.json -> lesbares Markdown mit sichtbaren SourceClaim-Refs -> Verifikation.</summary>
public static class HumanArtifactSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: human-artifact-spike <fixture.json> <claims.json> <artifact> [modelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var claimsPath = Path.IsPathRooted(args[2]) ? args[2] : Path.Combine(repoRoot, args[2]);
        var artifact = args[3];
        var modelArg = args.Length >= 5 ? args[4] : null;

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[human-artifact] Fixture fehlt: {fixturePath}"); return 2; }
        if (!File.Exists(claimsPath)) { Console.Error.WriteLine($"[human-artifact] Claims fehlen: {claimsPath}"); return 2; }

        var fixture = JsonSerializer.Deserialize<SemanticLedgerFixture>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var entries = (fixture?.Entries ?? [])
            .Where(e => e.Disposition.ContainsKey(artifact))
            .ToList();
        var sourceClaims = entries.Select(e => e.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var claims = JsonSerializer.Deserialize<List<GeneratedArtifactClaim>>(
            await File.ReadAllTextAsync(claimsPath).ConfigureAwait(false), JsonOptions)?
            .Select(c => c.Normalized())
            .ToList() ?? [];
        if (entries.Count == 0 || claims.Count == 0) { Console.Error.WriteLine("[human-artifact] leere Eingaben."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var client = ChatClientFactory.Create(judgeSettings);

        Console.WriteLine($"[human-artifact] Claims={Path.GetRelativePath(repoRoot, claimsPath)} Artifact={artifact} Model={judgeSettings.ModelId}");

        var generator = new HumanArtifactGenerator(client, settings.JuryStructuredOutput);
        var extractor = new ArtifactClaimExtractor(client, settings.JuryStructuredOutput);
        var verifier = new SemanticPreservationVerifier(client, settings.JuryStructuredOutput);

        var human = await generator.GenerateAsync(claims, artifact, CancellationToken.None).ConfigureAwait(false);
        if (human is null) { Console.Error.WriteLine("[human-artifact] generation parse failed."); return 1; }

        var extractedClaims = await extractor.ExtractAsync(artifact, human.Markdown, CancellationToken.None).ConfigureAwait(false);
        var verification = await verifier.VerifyAsync(entries, extractedClaims, artifact, CancellationToken.None).ConfigureAwait(false);
        var completeness = SemanticLedgerChecks.CheckVerifierCompleteness(entries, verification);
        var metrics = SemanticLedgerChecks.Metrics(entries, extractedClaims, verification, artifact);

        var markdownRefs = sourceClaims
            .Where(id => human.Markdown.Contains($"[{id}]", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingVisibleRefs = sourceClaims
            .Where(id => !markdownRefs.Contains(id))
            .ToList();
        var unknownVisibleRefs = human.UsedSourceClaimIds
            .Where(id => !sourceClaims.Contains(id))
            .ToList();

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike");
        Directory.CreateDirectory(outDir);
        var sourceStem = Path.GetFileNameWithoutExtension(claimsPath).Replace(".claims", "");
        var prefix = $"human-artifact.{artifact}.{sourceStem}.{modelSlug}";
        var mdPath = Path.Combine(outDir, $"{prefix}.md");
        var extractedPath = Path.Combine(outDir, $"{prefix}.extracted-claims.json");
        var verificationPath = Path.Combine(outDir, $"{prefix}.verification.json");

        await File.WriteAllTextAsync(mdPath, human.Markdown).ConfigureAwait(false);
        await File.WriteAllTextAsync(extractedPath, JsonSerializer.Serialize(extractedClaims, JsonOptions)).ConfigureAwait(false);

        var payload = new
        {
            mode = "human-artifact-spike",
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            sourceClaims = Path.GetRelativePath(repoRoot, claimsPath),
            artifact,
            model = judgeSettings.ModelId,
            markdown = Path.GetRelativePath(repoRoot, mdPath),
            extractedClaims = Path.GetRelativePath(repoRoot, extractedPath),
            metrics,
            completeness,
            traceability = new
            {
                expectedSourceClaimIds = sourceClaims.Order().ToList(),
                declaredUsedSourceClaimIds = human.UsedSourceClaimIds,
                visibleMarkdownRefs = markdownRefs.Order().ToList(),
                missingVisibleRefs,
                unknownVisibleRefs
            },
            human.ReadabilityNotes,
            verification
        };
        await File.WriteAllTextAsync(verificationPath, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        Console.WriteLine($"[human-artifact] markdown -> {Path.GetRelativePath(repoRoot, mdPath)}");
        Console.WriteLine($"[human-artifact] verification -> {Path.GetRelativePath(repoRoot, verificationPath)}");
        Console.WriteLine($"[human-artifact] refs missing={missingVisibleRefs.Count} unknown={unknownVisibleRefs.Count}");
        return 0;
    }
}
