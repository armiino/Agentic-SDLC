using System.Text.Json.Serialization;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

// ── L3 Open-World-Ableitung: Datenverträge + Edge-Payloads ───────────────────────────────────────────────────────
// Siehe docs/L3-openworld-plan.md. v1 = linearer Prepare-Pfad:
//   CandidateGen(Agent) → AnchorResolve(Agent) → Validate(det) → SupportJudge(Judge) → Routing(det) → Finalize.
// Die Klasse eines Kandidaten (§2.3) entsteht AUS deterministischer Anker-Existenz × semantischem Judge-Verdikt
// (§2.4-Aggregation im Routing-Executor — NICHT im Prompt).

/// <summary>Epistemischer Status eines Kandidaten nach Aggregation (§2.3).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum L3Class
{
    /// <summary>≥1 Anker trägt die Ableitung, kein Widerspruch → regulärer Derived-Item-Pfad.</summary>
    SupportedAnchored,
    /// <summary>≥1 autoritativer Anker widerspricht → menschliche Konfliktentscheidung (nicht auto-reject).</summary>
    Contradicted,
    /// <summary>Kein Anker trägt eindeutig, aber ≥1 unklar → Human-Review (Reflect nur bei NEEDS_REVISION).</summary>
    WeakOrUncertain,
    /// <summary>Kein gültiger Anker ODER alle unrelated → echter Open-World-Kandidat → Open-World-Human-Review.</summary>
    Unreferenced
}

/// <summary>Ein generierter Vorschlag OHNE Pflicht-Anker (§3 Phase A). Anker kommen erst in der Resolution.</summary>
public sealed record L3Candidate(
    [property: JsonPropertyName("candidateId")] string CandidateId,
    [property: JsonPropertyName("targetType")] string TargetType,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("rationale")] string? Rationale,
    [property: JsonPropertyName("assumptions")] IReadOnlyList<string> Assumptions,
    // Research-Modus (L3-Über-Agent): der Generator deklariert, WAS der Kandidat sein will —
    // "extension" (verankerbare Erweiterung eines Bestehenden) | "gap" (unausgesprochenes/fehlendes Thema, open-world).
    // BasedOn = die Item-ids, auf denen die Idee RECHERCHIERT wurde (Behauptung, KEIN finaler Anker — Resolver/Judge
    // entscheiden unabhängig; vgl. retrieved⊇used). Bei selective/exhaustive null/leer → rückwärtskompatibel.
    [property: JsonPropertyName("intent")] string? Intent = null,
    [property: JsonPropertyName("basedOn")] IReadOnlyList<string>? BasedOn = null,
    // Coverage/Gap-Modus (L3-Über-Agent, v10): additive Analyse-Felder. GapCategory = kontrollierte Prüflinsen-Kategorie
    // (data-lifecycle | security | actors-permissions | …), aggregierbar für die intent×class-Cross-Tab. ImpactIfMissing
    // = Projektbezug einer Lücke (verhindert generische Best-Practice-Kandidaten). RequiresHumanDecision = AGENTEN-Claim
    // (offener Entscheidungsbedarf) — reine Metadata, überschreibt NICHT das deterministische Routing (Class-Gate).
    [property: JsonPropertyName("gapCategory")] string? GapCategory = null,
    [property: JsonPropertyName("impactIfMissing")] string? ImpactIfMissing = null,
    [property: JsonPropertyName("requiresHumanDecision")] bool? RequiresHumanDecision = null);

/// <summary>Ein vom Agenten vorgeschlagener Anker mit behaupteter Beziehung + Begründung (§3 Datenvertrag).</summary>
public sealed record ProposedAnchor(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("relation")] string Relation,   // elaborates | depends_on | mitigates | conflicts_with | relates_to
    [property: JsonPropertyName("reason")] string Reason);

/// <summary>Ergebnis der Anchor-Resolution je Kandidat (0..N Anker, ODER expliziter „kein Anker"-Grund).</summary>
public sealed record L3AnchorResolution(
    [property: JsonPropertyName("proposedAnchors")] IReadOnlyList<ProposedAnchor> ProposedAnchors,
    [property: JsonPropertyName("noAnchorReason")] string? NoAnchorReason);

/// <summary>Ein Anker nach deterministischer Validierung (§2.1) + semantischem Judge (§4). <see cref="Exists"/> false =
/// UNKNOWN_ANCHOR (Defekt); <see cref="Verdict"/> null, solange nicht bewertet (weil nicht existent).</summary>
public sealed record AnchorAssessment(
    [property: JsonPropertyName("itemId")] string ItemId,
    [property: JsonPropertyName("relation")] string Relation,
    [property: JsonPropertyName("exists")] bool Exists,
    [property: JsonPropertyName("verdict")] InferenceVerdictKind? Verdict,
    [property: JsonPropertyName("rationale")] string? Rationale);

/// <summary>Ein Kandidat mit finalem Routing (§2.4). <see cref="KeptAnchorIds"/> = die tragenden Anker (dekorative sind
/// verworfen); <see cref="UnknownAnchorIds"/> = referenzierte, aber nicht existierende IDs (Defekt-Audit).</summary>
public sealed record L3RoutedCandidate(
    [property: JsonPropertyName("candidate")] L3Candidate Candidate,
    [property: JsonPropertyName("anchors")] IReadOnlyList<AnchorAssessment> Anchors,
    [property: JsonPropertyName("noAnchorReason")] string? NoAnchorReason,
    [property: JsonPropertyName("class")] L3Class Class,
    [property: JsonPropertyName("keptAnchorIds")] IReadOnlyList<string> KeptAnchorIds,
    [property: JsonPropertyName("unknownAnchorIds")] IReadOnlyList<string> UnknownAnchorIds);

// ── Edge-Payloads (typisierte MAF-Messages) ──────────────────────────────────────────────────────────────────────

public sealed record L3Candidates(SourceArtifactSet Env, IReadOnlyList<L3Candidate> Items);
public sealed record ResolvedCandidate(L3Candidate Candidate, L3AnchorResolution Resolution);
public sealed record L3Resolved(SourceArtifactSet Env, IReadOnlyList<ResolvedCandidate> Items);
public sealed record AnchorCheck(ProposedAnchor Anchor, bool Exists);
public sealed record ValidatedCandidate(L3Candidate Candidate, IReadOnlyList<AnchorCheck> Anchors, string? NoAnchorReason);
public sealed record L3Validated(SourceArtifactSet Env, IReadOnlyList<ValidatedCandidate> Items);
public sealed record JudgedCandidate(L3Candidate Candidate, IReadOnlyList<AnchorAssessment> Anchors, string? NoAnchorReason);
public sealed record L3Judged(SourceArtifactSet Env, IReadOnlyList<JudgedCandidate> Items);
public sealed record L3Routed(IReadOnlyList<L3RoutedCandidate> Items);

// ── Reflect-Sub-Workflow (NEEDS_REVISION, §5.3/§9) ───────────────────────────────────────────────────────────────
/// <summary>Ein zu revidierender Kandidat + menschliches Feedback + Zahl bisheriger Revisionen (bounded).</summary>
public sealed record L3ReviseItem(L3Candidate PrevCandidate, string Feedback, int PriorRevisions);
/// <summary>Eingabe des Revise-Sub-Workflows: Umwelt + die (im Cap liegenden) zu revidierenden Kandidaten.</summary>
public sealed record L3ReviseSet(SourceArtifactSet Env, IReadOnlyList<L3ReviseItem> Items);

/// <summary>Terminales Laufresultat (YieldOutput). Der Prepare-Pfad endet am Human-Review-Paket.</summary>
public sealed record L3Result(
    [property: JsonPropertyName("items")] IReadOnlyList<L3RoutedCandidate> Items,
    [property: JsonPropertyName("routingReportPath")] string RoutingReportPath,
    [property: JsonPropertyName("reviewPackagePath")] string ReviewPackagePath);
