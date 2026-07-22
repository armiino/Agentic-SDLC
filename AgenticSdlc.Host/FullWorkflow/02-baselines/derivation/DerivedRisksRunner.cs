using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Prompts;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// I-b-CLI: <c>derive-risks &lt;requirements.artifact.json&gt; [model] [out.json]</c>.
/// Erster Derivation-Schritt: liest die geprüfte, id-adressierte Requirements-Baseline (E-c/E-d-Output) und leitet
/// NEUE, an REQ-ids verankerte Risiken ab (<c>origin=derived</c>). Runs unter <c>runs/derivation/&lt;runId&gt;/</c>.
/// Exit: 0 = ok (auch no_new_risk), 2 = Usage/IO, 4 = LLM-Fehler.
/// </summary>
/// <remarks>
/// Enthält bereits die DETERMINISTISCHE Anker-Validierung (C1'/C2'-Analog, Vorschau auf den Inference-Checker I-c):
/// ein abgeleitetes Risiko ohne Anker oder mit einer id, die nicht in der Baseline existiert, ist ungültig verankert
/// und wandert NICHT ins konsumierbare <c>derived-risks.json</c> (bleibt aber im <c>derivation-report.json</c> zur
/// Audit-Sicht — Reviewer §11.4). Der semantische Relevanz-/Widerspruchs-Check (LLM) ist NICHT hier, sondern I-c.
/// Middleware/Logging wie überall (AgentChatPipelineBuilder → logs/agents/EvidenceDerivedRisksAgent).
/// </remarks>
public static class DerivedRisksRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private const string SourceName = "AgenticSdlc.Host";
    private const string Phase = "phase2_evidence";
    private const string AgentName = "EvidenceDerivedRisksAgent";
    private const string PromptName = "DerivedRisksFromRequirements1";

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: derive-risks <requirements.artifact.json> [model] [out.json]");
            return 2;
        }

        var basePath = Resolve(repoRoot, args[1]);
        if (basePath is null || !File.Exists(basePath))
        {
            Console.Error.WriteLine($"[derive-risks] Baseline fehlt: {args[1]}");
            return 2;
        }

        string? modelArg = args.Length >= 3 && !args[2].StartsWith("--", StringComparison.Ordinal) ? args[2] : null;
        string? outArg = args.Length >= 4 ? args[3] : null;

        var baseline = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(basePath).ConfigureAwait(false), Json);
        if (baseline is null || baseline.Items.Count == 0)
        {
            Console.Error.WriteLine("[derive-risks] Baseline leer/nicht lesbar (erwartet artifact.json mit items).");
            return 2;
        }

        var derivSettings = modelArg is not null ? settings with { ModelId = modelArg } : settings;

        var run = new RunContext(RunId.New(), "derivation");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "DerivedRisks",
            runId = run.RunId,
            baseline = Path.GetRelativePath(repoRoot, basePath),
            baselineArtifactId = baseline.ArtifactId,
            baselineItems = baseline.Items.Count,
            provider = settings.LlmProvider,
            model = derivSettings.ModelId,
            prompt = PromptName,
            timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[derive-risks] runId={run.RunId} baseline={baseline.ArtifactId}({baseline.Items.Count} items) model={derivSettings.ModelId}");

        using var otel = OtelRunExporters.TryCreate(
            enabled: settings.OtelEnabled, sourceName: SourceName,
            tracesPath: Path.Combine(run.LogsDir, "otel-traces.jsonl"),
            metricsPath: Path.Combine(run.LogsDir, "otel-metrics.jsonl"),
            rawTracesPath: settings.OtelRawEnabled ? Path.Combine(run.LogsDir, "otel-traces.raw.jsonl") : null);

        var client = AgentChatPipelineBuilder.Build(ChatClientFactory.Create(derivSettings), settings, run, AgentName, SourceName);
        var systemPrompt = PromptProvider.Load(repoRoot, Phase, AgentName, PromptName, new Dictionary<string, string> { ["runId"] = run.RunId });
        var agent = new DerivedRisksAgent(client, systemPrompt, settings.JuryStructuredOutput);

        string decision;
        IReadOnlyList<RawDerivedRisk> raw;
        try
        {
            (decision, raw) = await agent.DeriveAsync(baseline.Items, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[derive-risks] Ableitung fehlgeschlagen: {ex.Message}");
            run.AppendEvent(new { type = "RUN_FAILED", runId = run.RunId, reason = ex.Message, timestampUtc = DateTime.UtcNow });
            return 4;
        }

        // Deterministische Anker-Validierung (C1'/C2'-Analog): Anker vorhanden UND alle ids in der Baseline.
        var baselineIds = baseline.Items.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var valid = new List<ArtifactItem>();
        var invalid = new List<object>();
        var n = 0;
        foreach (var r in raw)
        {
            var anchors = (r.SourceArtifactItemIds ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            var badIds = anchors.Where(a => !baselineIds.Contains(a)).ToList();
            var anchored = anchors.Count > 0 && badIds.Count == 0;
            if (anchored)
            {
                n++;
                valid.Add(new ArtifactItem(
                    ItemId: $"DRISK-{n:D2}", Origin: ArtifactOrigin.Derived, Text: r.Text,
                    SourceClaimIds: [], SourceArtifactItemIds: anchors,
                    Assumptions: r.Assumptions ?? [], DerivationRationale: r.Rationale));
            }
            else
            {
                invalid.Add(new { r.Text, anchors, badIds, reason = anchors.Count == 0 ? "MISSING_ANCHOR" : "UNKNOWN_ANCHOR" });
            }
        }

        var producer = new ProducerMetadata(run.RunId, derivSettings.ModelId, PromptName);
        var doc = new ArtifactDocument(
            ArtifactId: "DRISK", ArtifactType: "derived-risks", Version: 1,
            Stage: ArtifactDocument.StageDerivation, Producer: producer, Items: valid);

        var outPath = outArg is not null ? Resolve(repoRoot, outArg)! : Path.Combine(run.RunDir, "derived-risks.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(doc, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(run.RunDir, "derivation-report.json"),
            JsonSerializer.Serialize(new { decision, total = raw.Count, anchoredValid = valid.Count, invalidAnchor = invalid.Count, invalid }, Json)).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "DERIVED_RISKS", runId = run.RunId, decision,
            total = raw.Count, anchoredValid = valid.Count, invalidAnchor = invalid.Count, timestampUtc = DateTime.UtcNow
        });

        Console.WriteLine($"[derive-risks] decision={decision}  abgeleitet={raw.Count}  anker-gültig={valid.Count}  anker-ungültig={invalid.Count}");
        foreach (var it in valid.Take(3))
            Console.WriteLine($"    {it.ItemId} <- [{string.Join(",", it.SourceArtifactItemIds)}]  {Trunc(it.Text)}");
        Console.WriteLine($"[derive-risks] -> {Path.GetRelativePath(repoRoot, outPath)}  (report: derivation-report.json)");
        Console.WriteLine($"[derive-risks] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return 0;
    }

    private static string Trunc(string s) => s.Length <= 90 ? s : s[..90] + "…";
    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
