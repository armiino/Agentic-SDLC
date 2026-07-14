using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

// v1-Prepare-Pipeline (linear, MAF-nativ). Verzweigung je Klasse + Human-Loop = Apply-Phase (Workflow 2, §5.3) → future.

/// <summary>Phase A (§3): der GENERATOR-Agent erzeugt neue Kandidaten OHNE Pflicht-Anker aus der Umwelt. Echter
/// <see cref="AIAgent"/> (Prompt/Persona + Middleware-Pipeline). Semantische Generierung → Agent (MAF-Regel).</summary>
[SendsMessage(typeof(L3Candidates))]
internal sealed class L3CandidateGenExecutor(AIAgent agent, RunContext run) : Executor<SourceArtifactSet>("L3-CandidateGen")
{
    public override async ValueTask HandleAsync(SourceArtifactSet env, IWorkflowContext context, CancellationToken ct = default)
    {
        var response = await agent.RunAsync([new ChatMessage(ChatRole.User, L3Prompts.BuildEnvUser(env, forGeneration: true))], cancellationToken: ct).ConfigureAwait(false);
        var items = L3CandidateParsing.AssignIds(L3CandidateParsing.ParseCores(response.Text));
        run.AppendEvent(new { type = "L3_CANDIDATES_GENERATED", runId = run.RunId, count = items.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Candidates(env, items)).ConfigureAwait(false);
    }
}

/// <summary>Phase B (§3): der RESOLUTION-Agent sucht — GETRENNT von der Generierung — mögliche Anker je Kandidat
/// (Beziehungstyp + Begründung) ODER begründet, dass keiner tragfähig ist. Verhindert dekoratives Pflicht-Ankern.</summary>
[SendsMessage(typeof(L3Resolved))]
internal sealed class L3AnchorResolveExecutor(AIAgent agent, RunContext run) : Executor<L3Candidates>("L3-AnchorResolve")
{
    public override async ValueTask HandleAsync(L3Candidates msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var resolved = new List<ResolvedCandidate>(msg.Items.Count);
        if (msg.Items.Count > 0)
        {
            var user = L3Prompts.BuildResolveUser(msg.Env, msg.Items);
            var response = await agent.RunAsync([new ChatMessage(ChatRole.User, user)], cancellationToken: ct).ConfigureAwait(false);
            var byId = (L3Json.Deserialize<RawResolutions>(response.Text)?.Resolutions ?? [])
                .Where(r => !string.IsNullOrWhiteSpace(r.CandidateId))
                .ToDictionary(r => r.CandidateId!, r => r, StringComparer.Ordinal);

            foreach (var c in msg.Items)
            {
                var anchors = new List<ProposedAnchor>();
                string? noAnchorReason = null;
                if (byId.TryGetValue(c.CandidateId, out var r))
                {
                    anchors = (r.ProposedAnchors ?? [])
                        .Where(a => !string.IsNullOrWhiteSpace(a.ItemId))
                        .Select(a => new ProposedAnchor(a.ItemId!.Trim(), string.IsNullOrWhiteSpace(a.Relation) ? "relates_to" : a.Relation!.Trim(), a.Reason ?? ""))
                        .ToList();
                    noAnchorReason = anchors.Count == 0 ? (r.NoAnchorReason ?? "kein Anker vorgeschlagen") : null;
                }
                else noAnchorReason = "keine Resolution vom Agenten (fail-open → open-world).";
                resolved.Add(new ResolvedCandidate(c, new L3AnchorResolution(anchors, noAnchorReason)));
            }
        }
        run.AppendEvent(new { type = "L3_ANCHORS_RESOLVED", runId = run.RunId, candidates = resolved.Count, withAnchors = resolved.Count(r => r.Resolution.ProposedAnchors.Count > 0), timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Resolved(msg.Env, resolved)).ConfigureAwait(false);
    }

    private sealed record RawResolutions([property: JsonPropertyName("resolutions")] IReadOnlyList<RawResolution>? Resolutions);
    private sealed record RawResolution(
        [property: JsonPropertyName("candidateId")] string? CandidateId,
        [property: JsonPropertyName("proposedAnchors")] IReadOnlyList<RawAnchor>? ProposedAnchors,
        [property: JsonPropertyName("noAnchorReason")] string? NoAnchorReason);
    private sealed record RawAnchor(
        [property: JsonPropertyName("itemId")] string? ItemId,
        [property: JsonPropertyName("relation")] string? Relation,
        [property: JsonPropertyName("reason")] string? Reason);
}

/// <summary>Phase C (§2.1): DETERMINISTISCHE Anker-Existenzprüfung gegen die Umwelt. Kein LLM → korrekt KEIN Agent.
/// Nicht existierende IDs (UNKNOWN_ANCHOR) werden markiert, aber (v1) nicht repariert — das Routing behandelt sie.</summary>
[SendsMessage(typeof(L3Validated))]
internal sealed class L3AnchorValidateExecutor(RunContext run) : Executor<L3Resolved>("L3-AnchorValidate")
{
    public override async ValueTask HandleAsync(L3Resolved msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var envIds = msg.Env.ItemsById().Keys.ToHashSet(StringComparer.Ordinal);
        var validated = msg.Items.Select(rc => new ValidatedCandidate(
            rc.Candidate,
            rc.Resolution.ProposedAnchors.Select(a => new AnchorCheck(a, envIds.Contains(a.ItemId))).ToList(),
            rc.Resolution.NoAnchorReason)).ToList();

        var unknown = validated.Sum(v => v.Anchors.Count(a => !a.Exists));
        run.AppendEvent(new { type = "L3_ANCHORS_VALIDATED", runId = run.RunId, candidates = validated.Count, unknownAnchors = unknown, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Validated(msg.Env, validated)).ConfigureAwait(false);
    }
}

/// <summary>Phase D (§4): SEMANTISCHE Tragfähigkeit JE (Kandidat, existierender Anker) über den wiederverwendeten
/// <see cref="InferenceChecker"/>-Kern (L3-Maßstab). Kandidat×Anker wird zu einem Paar-Item expandiert (ref =
/// „CAND#ANKER") → ein Verdikt pro Paar, driftfrei mit dem bestehenden Judge.</summary>
[SendsMessage(typeof(L3Judged))]
internal sealed class L3SupportJudgeExecutor(InferenceChecker judge, RunContext run) : Executor<L3Validated>("L3-SupportJudge")
{
    public override async ValueTask HandleAsync(L3Validated msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var baseline = msg.Env.ItemsById();
        var pairItems = new List<ArtifactItem>();
        foreach (var vc in msg.Items)
            foreach (var a in vc.Anchors.Where(a => a.Exists))
                pairItems.Add(new ArtifactItem($"{vc.Candidate.CandidateId}#{a.Anchor.ItemId}", ArtifactOrigin.Extracted,
                    vc.Candidate.Text, SourceClaimIds: [], SourceArtifactItemIds: [a.Anchor.ItemId]));

        var verdictByRef = pairItems.Count > 0
            ? (await judge.CheckAsync(pairItems, baseline, ct).ConfigureAwait(false)).Verdicts.ToDictionary(v => v.ItemId, v => v, StringComparer.Ordinal)
            : new Dictionary<string, InferenceVerdict>(StringComparer.Ordinal);

        var judged = msg.Items.Select(vc => new JudgedCandidate(
            vc.Candidate,
            vc.Anchors.Select(a =>
            {
                if (!a.Exists) return new AnchorAssessment(a.Anchor.ItemId, a.Anchor.Relation, false, null, "UNKNOWN_ANCHOR: ID nicht in der Umwelt.");
                var v = verdictByRef.GetValueOrDefault($"{vc.Candidate.CandidateId}#{a.Anchor.ItemId}");
                return new AnchorAssessment(a.Anchor.ItemId, a.Anchor.Relation, true, v?.Verdict ?? InferenceVerdictKind.Unclear, v?.Rationale);
            }).ToList(),
            vc.NoAnchorReason)).ToList();

        run.AppendEvent(new { type = "L3_SUPPORT_JUDGED", runId = run.RunId, pairs = pairItems.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Judged(msg.Env, judged)).ConfigureAwait(false);
    }
}

/// <summary>Phase E (§2.4): DETERMINISTISCHE Aggregation je Kandidat → genau eine Routing-Klasse. Kein LLM.</summary>
[SendsMessage(typeof(L3Routed))]
internal sealed class L3RoutingExecutor(RunContext run) : Executor<L3Judged>("L3-Routing")
{
    public override async ValueTask HandleAsync(L3Judged msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var routed = msg.Items.Select(j => L3Routing.Route(j.Candidate, j.Anchors, j.NoAnchorReason)).ToList();
        var byClass = routed.GroupBy(r => r.Class).ToDictionary(g => g.Key.ToString(), g => g.Count());
        run.AppendEvent(new { type = "L3_ROUTED", runId = run.RunId, total = routed.Count, byClass, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new L3Routed(routed)).ConfigureAwait(false);
    }
}

/// <summary>Finalize-Join (§5.2): schreibt Routing-Report + Human-Review-Paket (accept/edit/reject) auf Platte und
/// yieldet das terminale <see cref="L3Result"/>. Der Prepare-Pfad endet HIER; das deterministische Apply ist Workflow 2.</summary>
[YieldsOutput(typeof(L3Result))]
internal sealed class L3FinalizeExecutor(RunContext run, string outSuffix = "", CoverageSpec? coverageSpec = null) : Executor<L3Routed>("L3-Finalize")
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public override async ValueTask HandleAsync(L3Routed msg, IWorkflowContext context, CancellationToken ct = default)
    {
        var byClass = msg.Items.GroupBy(r => r.Class).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var routingPath = Path.Combine(run.RunDir, $"routing-report{outSuffix}.json");
        await File.WriteAllTextAsync(routingPath, JsonSerializer.Serialize(new { total = msg.Items.Count, byClass, items = msg.Items }, Json), ct).ConfigureAwait(false);

        // Human-Review-Paket = die Kandidaten, die eine menschliche Entscheidung brauchen (nicht SUPPORTED_ANCHORED).
        // v1-Operationen: accept | edit | reject (§6). CONTRADICTED wird NICHT auto-verworfen → Konfliktentscheidung.
        var needsHuman = msg.Items.Where(r => r.Class != L3Class.SupportedAnchored).Select(r => new
        {
            candidateId = r.Candidate.CandidateId,
            r.Class,
            text = r.Candidate.Text,
            r.Candidate.TargetType,
            r.Candidate.Intent,
            r.Candidate.BasedOn,
            r.Candidate.GapCategory,
            r.Candidate.ImpactIfMissing,
            r.Candidate.RequiresHumanDecision,
            r.Candidate.Rationale,
            r.Candidate.Assumptions,
            anchors = r.Anchors,
            r.NoAnchorReason,
            r.UnknownAnchorIds,
            allowedOperations = new[] { "accept", "edit", "reject" },
            note = r.Class switch
            {
                L3Class.Contradicted => "Widerspruch zu bestehendem Item — Konfliktentscheidung, nicht automatisch verwerfen.",
                L3Class.Unreferenced => "Open-World: kein tragfähiger Anker — nur mit menschlicher Autorisierung übernehmen.",
                _ => "Schwach/unsicher gestützt — Mensch entscheidet (NEEDS_REVISION triggert Reflect)."
            }
        }).ToList();
        var reviewPath = Path.Combine(run.RunDir, $"human-review-package{outSuffix}.json");
        await File.WriteAllTextAsync(reviewPath, JsonSerializer.Serialize(new
        {
            runId = run.RunId, generatedUtc = DateTime.UtcNow,
            summary = byClass, autoSupported = byClass.GetValueOrDefault(nameof(L3Class.SupportedAnchored), 0),
            needsHumanCount = needsHuman.Count, items = needsHuman
        }, Json), ct).ConfigureAwait(false);

        // Lens-Coverage-Report (Schritt 3, nur wenn ein CoverageSpec aktiv = coverage-Modus): deterministisch, measure-only
        // (kein Loop — das wäre Schritt 4). Macht leere/Pflicht-leere Linsen sichtbar statt sie lautlos verschwinden zu lassen.
        if (coverageSpec is not null)
        {
            var coverage = L3LensCoverage.Evaluate(coverageSpec, msg.Items.Select(r => r.Candidate).ToList());
            var coveragePath = Path.Combine(run.RunDir, $"lens-coverage-report{outSuffix}.json");
            await File.WriteAllTextAsync(coveragePath, JsonSerializer.Serialize(coverage, Json), ct).ConfigureAwait(false);
            run.AppendEvent(new
            {
                type = "L3_LENS_COVERAGE", runId = run.RunId, spec = coverage.SpecId,
                addressed = coverage.AddressedLenses, total = coverage.TotalLenses,
                unaddressed = coverage.UnaddressedLensIds, mandatoryUnaddressed = coverage.MandatoryUnaddressedLensIds,
                coverageComplete = coverage.CoverageComplete, mandatoryComplete = coverage.MandatoryComplete,
                untagged = coverage.UntaggedCandidates, unknownCategories = coverage.UnknownCategories, timestampUtc = DateTime.UtcNow
            });
        }

        run.AppendEvent(new { type = "L3_FINALIZED", runId = run.RunId, byClass, needsHuman = needsHuman.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new L3Result(msg.Items, routingPath, reviewPath)).ConfigureAwait(false);
    }
}

/// <summary>Tolerante JSON-Extraktion (erstes '{' bis letztes '}') — wie in der Derivation-Familie.</summary>
internal static class L3Json
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static T? Deserialize<T>(string? text) where T : class
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var s = text.IndexOf('{'); var e = text.LastIndexOf('}');
        if (s < 0 || e <= s) return null;
        try { return JsonSerializer.Deserialize<T>(text.Substring(s, e - s + 1), Options); }
        catch (JsonException) { return null; }
    }
}
