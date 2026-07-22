using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;

namespace AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem;

/// <summary>Evidence-first Semantic-Ledger-Spike mit Vergleich gegen bestehenden freien Run.</summary>
public static class EvidenceFirstSpikeRunner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: evidence-first-spike <fixture.json> <artifact> [phase] [runId] [modelOverride]");
            return 2;
        }

        var fixturePath = Path.IsPathRooted(args[1]) ? args[1] : Path.Combine(repoRoot, args[1]);
        var artifact = args[2];
        var phase = args.Length >= 4 && !args[3].Contains('/') ? args[3] : "phase2_1";
        var runId = args.Length >= 5 && !args[4].Contains('/') ? args[4] : "20260612_133345_2d7b09";
        var modelArg = args.Length >= 6 ? args[5]
            : args.Length >= 4 && args[3].Contains('/') ? args[3]
            : args.Length >= 5 && args[4].Contains('/') ? args[4]
            : null;

        if (!File.Exists(fixturePath)) { Console.Error.WriteLine($"[evidence-first] Fixture fehlt: {fixturePath}"); return 2; }

        var docsDir = Path.Combine(repoRoot, "runs", phase, runId, "snapshots", "docs");
        var oldArtifactPath = Path.Combine(docsDir, $"{artifact}.md");
        if (!File.Exists(oldArtifactPath)) { Console.Error.WriteLine($"[evidence-first] Vergleichsartefakt fehlt: {oldArtifactPath}"); return 2; }

        var fixture = JsonSerializer.Deserialize<SemanticLedgerFixture>(
            await File.ReadAllTextAsync(fixturePath).ConfigureAwait(false), JsonOptions);
        var entries = fixture?.Entries ?? [];
        if (entries.Count == 0) { Console.Error.WriteLine("[evidence-first] leere Fixture."); return 2; }

        var scopedEntries = entries
            .Where(e => e.Disposition.ContainsKey(artifact))
            .ToList();
        if (scopedEntries.Count == 0) { Console.Error.WriteLine($"[evidence-first] keine Eintraege fuer Artefakt {artifact}."); return 2; }

        var judgeSettings =
            modelArg is not null ? settings with { ModelId = modelArg }
            : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! }
            : settings;
        var modelSlug = judgeSettings.ModelId.Replace('/', '_').Replace(':', '_');
        var client = ChatClientFactory.Create(judgeSettings);

        var outDir = Path.Combine(repoRoot, "thesis-evidence", "evidence-first-spike");
        Directory.CreateDirectory(outDir);

        Console.WriteLine($"[evidence-first] Fixture={Path.GetRelativePath(repoRoot, fixturePath)} Artifact={artifact} Entries={scopedEntries.Count} Model={judgeSettings.ModelId}");

        var oldText = await File.ReadAllTextAsync(oldArtifactPath).ConfigureAwait(false);

        var extractor = new ArtifactClaimExtractor(client, settings.JuryStructuredOutput);
        var generator = new EvidenceFirstArtifactGenerator(client, settings.JuryStructuredOutput);
        var verifier = new SemanticPreservationVerifier(client, settings.JuryStructuredOutput);

        Console.WriteLine("[evidence-first] extracting claims from old artifact...");
        var oldClaims = await extractor.ExtractAsync(artifact, oldText, CancellationToken.None).ConfigureAwait(false);

        Console.WriteLine("[evidence-first] generating evidence-first artifact...");
        var generated = await generator.GenerateAsync(scopedEntries, artifact, CancellationToken.None).ConfigureAwait(false);
        if (generated is null) { Console.Error.WriteLine("[evidence-first] generation parse failed."); return 1; }

        Console.WriteLine("[evidence-first] verifying old artifact claims...");
        var oldVerification = await verifier.VerifyAsync(scopedEntries, oldClaims, artifact, CancellationToken.None).ConfigureAwait(false);

        Console.WriteLine("[evidence-first] verifying evidence-first artifact claims...");
        var newVerification = await verifier.VerifyAsync(scopedEntries, generated.Claims, artifact, CancellationToken.None).ConfigureAwait(false);

        var oldCompleteness = SemanticLedgerChecks.CheckVerifierCompleteness(scopedEntries, oldVerification);
        var newCompleteness = SemanticLedgerChecks.CheckVerifierCompleteness(scopedEntries, newVerification);
        var oldDisposition = SemanticLedgerChecks.CheckDispositionCoverage(scopedEntries, oldClaims, artifact);
        var newDisposition = SemanticLedgerChecks.CheckDispositionCoverage(scopedEntries, generated.Claims, artifact);
        var oldMetrics = SemanticLedgerChecks.Metrics(scopedEntries, oldClaims, oldVerification, artifact);
        var newMetrics = SemanticLedgerChecks.Metrics(scopedEntries, generated.Claims, newVerification, artifact);

        var prefix = $"evidence-first-v2.{artifact}.{phase}_{runId}.{modelSlug}";
        var generatedMdPath = Path.Combine(outDir, $"{prefix}.md");
        var generatedClaimsPath = Path.Combine(outDir, $"{prefix}.claims.json");
        var oldClaimsPath = Path.Combine(outDir, $"{prefix}.old-claims.json");
        var verificationPath = Path.Combine(outDir, $"{prefix}.verification.json");
        // Pro-Run-Zusammenfassung SCOPED benennen, damit die kumulative, handgepflegte evidence.md
        // (von mehreren Runs/Abschnitten) NICHT bei jedem Lauf ueberschrieben wird.
        var evidencePath = Path.Combine(outDir, $"{prefix}.evidence.md");

        await File.WriteAllTextAsync(generatedMdPath, generated.Markdown).ConfigureAwait(false);
        await File.WriteAllTextAsync(generatedClaimsPath, JsonSerializer.Serialize(generated.Claims, JsonOptions)).ConfigureAwait(false);
        await File.WriteAllTextAsync(oldClaimsPath, JsonSerializer.Serialize(oldClaims, JsonOptions)).ConfigureAwait(false);

        var payload = new
        {
            mode = "evidence-first-spike",
            fixture = Path.GetRelativePath(repoRoot, fixturePath),
            artifact,
            phase,
            runId,
            model = judgeSettings.ModelId,
            oldArtifact = Path.GetRelativePath(repoRoot, oldArtifactPath),
            generatedArtifact = Path.GetRelativePath(repoRoot, generatedMdPath),
            generatedClaims = Path.GetRelativePath(repoRoot, generatedClaimsPath),
            oldClaims = Path.GetRelativePath(repoRoot, oldClaimsPath),
            entries = scopedEntries.Count,
            oldMetrics,
            newMetrics,
            oldCompleteness,
            newCompleteness,
            oldDisposition,
            newDisposition,
            oldVerification,
            newVerification
        };
        await File.WriteAllTextAsync(verificationPath, JsonSerializer.Serialize(payload, JsonOptions)).ConfigureAwait(false);

        await File.WriteAllTextAsync(evidencePath, BuildEvidenceMarkdown(
            fixturePath, artifact, phase, runId, judgeSettings.ModelId, oldArtifactPath, generatedMdPath,
            generatedClaimsPath, oldClaimsPath, verificationPath, oldMetrics, newMetrics,
            oldCompleteness.Count, newCompleteness.Count), CancellationToken.None).ConfigureAwait(false);

        Console.WriteLine($"[evidence-first] generated -> {Path.GetRelativePath(repoRoot, generatedMdPath)}");
        Console.WriteLine($"[evidence-first] verification -> {Path.GetRelativePath(repoRoot, verificationPath)}");
        Console.WriteLine($"[evidence-first] evidence -> {Path.GetRelativePath(repoRoot, evidencePath)}");
        return 0;
    }

    private static string BuildEvidenceMarkdown(
        string fixturePath,
        string artifact,
        string phase,
        string runId,
        string model,
        string oldArtifactPath,
        string generatedMdPath,
        string generatedClaimsPath,
        string oldClaimsPath,
        string verificationPath,
        object oldMetrics,
        object newMetrics,
        int oldCompletenessIssues,
        int newCompletenessIssues)
        => $"""
        # Evidence-first Semantic-Ledger-Spike

        ## Run

        ```text
        dotnet run --project AgenticSdlc.Host -- \
          evidence-first-spike \
          {fixturePath} \
          {artifact} \
          {phase} \
          {runId} \
          {model}
        ```

        ## Inputs

        ```text
        Fixture:      {fixturePath}
        Old artifact: {oldArtifactPath}
        Artifact:     {artifact}
        Model:        {model}
        ```

        ## Outputs

        ```text
        Generated artifact: {generatedMdPath}
        Generated claims:   {generatedClaimsPath}
        Old claims:         {oldClaimsPath}
        Verification:       {verificationPath}
        ```

        ## Metrics

        Old artifact:

        ```json
        {JsonSerializer.Serialize(oldMetrics, JsonOptions)}
        ```

        Evidence-first artifact:

        ```json
        {JsonSerializer.Serialize(newMetrics, JsonOptions)}
        ```

        Completeness issues:

        ```text
        Old artifact verifier issues:        {oldCompletenessIssues}
        Evidence-first verifier issues:      {newCompletenessIssues}
        ```

        ## Interpretation Template

        Dieser Spike misst Semantic Preservation und Disposition Coverage gegen eine bestaetigte Fixture.
        Er misst nicht die Vollstaendigkeit der automatischen Ledger-Extraction.
        """;
}
