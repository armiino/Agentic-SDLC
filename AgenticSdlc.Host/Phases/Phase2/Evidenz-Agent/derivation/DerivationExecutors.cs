using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>Stufe 1: der GENERATOR — ein echter <see cref="AIAgent"/> (Prompt/Persona + Middleware-Pipeline).
/// Liest die Quell-Items, erzeugt Roh-Ableitungen (JSON). Semantische Generierung → Agent (MAF-Regel).</summary>
[SendsMessage(typeof(GeneratedDerivation))]
internal sealed class DerivationGenerateExecutor : Executor<ArtifactDocument>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public DerivationSpec Spec { get; }
    private readonly AIAgent _agent;
    private readonly RunContext _run;

    public DerivationGenerateExecutor(AIAgent agent, DerivationSpec spec, RunContext run)
        : base($"DerivationGenerate-{spec.Id}")
    {
        _agent = agent;
        Spec = spec;
        _run = run;
    }

    public override async ValueTask HandleAsync(ArtifactDocument source, IWorkflowContext context, CancellationToken ct = default)
    {
        var user = BuildUser(source.Items);
        var response = await _agent.RunAsync([new ChatMessage(ChatRole.User, user)], cancellationToken: ct).ConfigureAwait(false);
        var (decision, raw) = Parse(response.Text);

        _run.AppendEvent(new { type = "DERIVATION_GENERATED", runId = _run.RunId, spec = Spec.Id, decision, count = raw.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new GeneratedDerivation(source, raw, decision)).ConfigureAwait(false);
    }

    private static string BuildUser(IReadOnlyList<ArtifactItem> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GEPRÜFTE QUELL-ITEMS ({items.Count}) — leite hieraus (und NUR hieraus) ab:");
        sb.AppendLine();
        foreach (var it in items) sb.Append("- ").Append(it.ItemId).Append(": ").AppendLine(it.Text);
        return sb.ToString();
    }

    // Tolerant: akzeptiert "items" ODER "risks" als Array-Key (Prompt-Kompatibilität).
    private static (string Decision, IReadOnlyList<RawDerivedItem> Items) Parse(string? text)
    {
        var json = Extract(text);
        if (json is null) return ("unknown", []);
        try
        {
            var r = JsonSerializer.Deserialize<RawResult>(json, Json);
            return (r?.Decision ?? "unknown", r?.Items ?? r?.Risks ?? []);
        }
        catch (JsonException) { return ("unknown", []); }
    }

    private static string? Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{'); var e = text.LastIndexOf('}');
        return s < 0 || e <= s ? null : text.Substring(s, e - s + 1);
    }

    private sealed record RawResult(
        [property: JsonPropertyName("decision")] string? Decision,
        [property: JsonPropertyName("items")] IReadOnlyList<RawDerivedItem>? Items,
        [property: JsonPropertyName("risks")] IReadOnlyList<RawDerivedItem>? Risks);
}

/// <summary>Stufe 2: ANKER-VALIDIERUNG — deterministisch (C1'/C2'-Analog, kein LLM → korrekt KEIN Agent). Nur an
/// existierende Quell-ids verankerte Items kommen durch; vergibt stabile Item-IDs.</summary>
[SendsMessage(typeof(AnchoredDerivation))]
internal sealed class DerivationAnchorExecutor : Executor<GeneratedDerivation>
{
    private readonly DerivationSpec _spec;
    private readonly RunContext _run;

    public DerivationAnchorExecutor(DerivationSpec spec, RunContext run) : base($"DerivationAnchor-{spec.Id}")
    {
        _spec = spec;
        _run = run;
    }

    public override async ValueTask HandleAsync(GeneratedDerivation msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var sourceIds = msg.Source.Items.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var valid = new List<ArtifactItem>();
        var invalid = new List<InvalidAnchor>();
        var n = 0;
        foreach (var r in msg.Raw)
        {
            var anchors = (r.SourceArtifactItemIds ?? []).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
            var bad = anchors.Where(a => !sourceIds.Contains(a)).ToList();
            if (anchors.Count > 0 && bad.Count == 0)
            {
                n++;
                valid.Add(new ArtifactItem(
                    $"{_spec.ItemIdPrefix}-{n:D2}", ArtifactOrigin.Derived, r.Text,
                    SourceClaimIds: [], SourceArtifactItemIds: anchors,
                    Assumptions: r.Assumptions ?? [], DerivationRationale: r.Rationale));
            }
            else
            {
                invalid.Add(new InvalidAnchor(r.Text, anchors, bad, anchors.Count == 0 ? "MISSING_ANCHOR" : "UNKNOWN_ANCHOR"));
            }
        }

        _run.AppendEvent(new { type = "DERIVATION_ANCHORED", runId = _run.RunId, spec = _spec.Id, valid = valid.Count, invalid = invalid.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AnchoredDerivation(msg.Source, valid, invalid, msg.Decision)).ConfigureAwait(false);
    }
}

/// <summary>Stufe 3: INFERENCE-CHECK — bounded Judge (LLM) über <see cref="InferenceChecker"/>. Hängt je Item das
/// Relevanz-/Widerspruchs-Verdikt an, baut das Ziel-<see cref="ArtifactDocument"/>, schreibt die Ergebnisse auf
/// Platte und yieldet das <see cref="DerivationResult"/>.</summary>
[YieldsOutput(typeof(DerivationResult))]
internal sealed class DerivationCheckExecutor : Executor<AnchoredDerivation>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly InferenceChecker _checker;
    private readonly DerivationSpec _spec;
    private readonly string _model;
    private readonly RunContext _run;

    public DerivationCheckExecutor(InferenceChecker checker, DerivationSpec spec, string model, RunContext run)
        : base($"DerivationCheck-{spec.Id}")
    {
        _checker = checker;
        _spec = spec;
        _model = model;
        _run = run;
    }

    public override async ValueTask HandleAsync(AnchoredDerivation msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var baselineById = msg.Source.Items.ToDictionary(i => i.ItemId, i => i, StringComparer.Ordinal);
        var report = await _checker.CheckAsync(msg.Items, baselineById, ct).ConfigureAwait(false);

        var producer = new ProducerMetadata(_run.RunId, _model, _spec.PromptName);
        var doc = new ArtifactDocument(
            ArtifactId: _spec.ItemIdPrefix, ArtifactType: _spec.TargetArtifactType, Version: 1,
            Stage: ArtifactDocument.StageDerivation, Producer: producer, Items: msg.Items);

        // Ergebnisse auf Platte (Wahrheitsquelle, unabhängig vom Output-Event).
        await File.WriteAllTextAsync(Path.Combine(_run.RunDir, $"{_spec.TargetArtifactType}.derived.json"), JsonSerializer.Serialize(doc, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(_run.RunDir, "inference-check-report.json"), JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(_run.RunDir, "derivation-report.json"),
            JsonSerializer.Serialize(new { spec = _spec.Id, decision = msg.Decision, anchoredValid = msg.Items.Count, invalidAnchor = msg.Invalid.Count, invalid = msg.Invalid }, Json), ct).ConfigureAwait(false);

        _run.AppendEvent(new { type = "DERIVATION_CHECKED", runId = _run.RunId, spec = _spec.Id, items = msg.Items.Count, pass = report.Pass, byVerdict = report.ByVerdict, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DerivationResult(doc, report.Verdicts, msg.Invalid, msg.Decision)).ConfigureAwait(false);
    }
}
