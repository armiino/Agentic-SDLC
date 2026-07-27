using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// W1e' Typed Messages — die Stufengrenzen-Verträge der pipeline-full-Kette. Reine Datencontainer (Pfade +
// Kennzahlen), damit metrics.json ein Lauf-PROTOKOLL wird (Finalize sammelt je Knoten) statt nachträglichem
// Pfad-Zusammensuchen. Wiederverwendete Verträge (Ingest/Pbi/Forward Request/Response, ProjectStateDocument als
// MeetingDelta) sind NICHT hier — die existieren bereits als Executor-Messages.

/// <summary>Seed: das Transkript (Pfad + Inhalt) — Eingang des Graphen.</summary>
public sealed record TranscriptInput(
    [property: JsonPropertyName("transcriptPath")] string TranscriptPath,
    [property: JsonPropertyName("transcriptText")] string TranscriptText);

/// <summary>01-ledger-Output: validierter Ledger + Miss-Signal + Zählwerte.</summary>
public sealed record LedgerStageOutput(
    [property: JsonPropertyName("validatedPath")] string ValidatedPath,
    [property: JsonPropertyName("missSignalPath")] string? MissSignalPath,
    [property: JsonPropertyName("claimCount")] int ClaimCount,
    [property: JsonPropertyName("pendingCount")] int PendingCount);

/// <summary>Nach Adjudikation: der consumable Ledger (Input für 02-baselines).</summary>
public sealed record ConsumableLedgerOutput(
    [property: JsonPropertyName("consumablePath")] string ConsumablePath,
    [property: JsonPropertyName("claimCount")] int ClaimCount);

/// <summary>02-baselines-Output: req/arch-Artefaktpfade + Zählwerte (Input für 04-delta).</summary>
public sealed record BaselineStageOutput(
    [property: JsonPropertyName("artifactPaths")] IReadOnlyList<string> ArtifactPaths,
    [property: JsonPropertyName("requirementCount")] int RequirementCount,
    [property: JsonPropertyName("architectureCount")] int ArchitectureCount);

/// <summary>04-delta-Kennzahl-Message (das ProjectStateDocument selbst fließt separat als Ingest-Input).</summary>
public sealed record MeetingDeltaOutput(
    [property: JsonPropertyName("projectStatePath")] string ProjectStatePath,
    [property: JsonPropertyName("itemCount")] int ItemCount,
    [property: JsonPropertyName("relationCount")] int RelationCount);

/// <summary>U0 (Ein-Graph): das Delta, geroutet in den Bootstrap-Zweig (Core leer / mode=bootstrap).</summary>
public sealed record BootstrapDelta([property: JsonPropertyName("delta")] Delta.ProjectStateDocument Delta);

/// <summary>U0 (Ein-Graph): das Delta, geroutet in den Betriebs-Zweig (Core existiert / mode=operational).</summary>
public sealed record OperationalDelta([property: JsonPropertyName("delta")] Delta.ProjectStateDocument Delta);

/// <summary>Bootstrap-Zweig B1: Core geseedet + L4-Baseline-Triplet materialisiert (Input für re-clarify cluster).</summary>
public sealed record CoreBootstrapOutput(
    [property: JsonPropertyName("baselineDir")] string BaselineDir,
    [property: JsonPropertyName("baselinePath")] string BaselinePath,
    [property: JsonPropertyName("coreItems")] int CoreItems,
    [property: JsonPropertyName("requirements")] int Requirements);

/// <summary>Bootstrap-Zweig B3: Ergebnis des deterministischen Cluster-Apply (Input für clarify, B4).</summary>
public sealed record ClusterApplyOutput(
    [property: JsonPropertyName("appliedClustersPath")] string AppliedClustersPath,
    [property: JsonPropertyName("applied")] int Applied,
    [property: JsonPropertyName("skipped")] int Skipped,
    [property: JsonPropertyName("gatePass")] bool GatePass,
    [property: JsonPropertyName("clusters")] int Clusters);

/// <summary>Bootstrap-Zweig B4: Ergebnis des deterministischen Backlog-Apply (Input für core-seed-backlog).</summary>
public sealed record BacklogApplyOutput(
    [property: JsonPropertyName("appliedBacklogPath")] string AppliedBacklogPath,
    [property: JsonPropertyName("pbisBefore")] int PbisBefore,
    [property: JsonPropertyName("pbisAfter")] int PbisAfter,
    [property: JsonPropertyName("dropped")] int Dropped,
    [property: JsonPropertyName("gatePass")] bool GatePass);

/// <summary>Bootstrap-Zweig B4: Backlog im Core — der Bootstrap ist inhaltlich komplett (B5 = initial-sync).</summary>
public sealed record BacklogSeedOutput(
    [property: JsonPropertyName("featuresAdded")] int FeaturesAdded,
    [property: JsonPropertyName("pbisAdded")] int PbisAdded,
    [property: JsonPropertyName("relationsAdded")] int RelationsAdded,
    [property: JsonPropertyName("coreItemsBefore")] int CoreItemsBefore,
    [property: JsonPropertyName("coreItemsAfter")] int CoreItemsAfter);

/// <summary>Snapshot vor Forward (R-16): frischer github-snapshot + Issue-Zahl.</summary>
public sealed record GithubSnapshotOutput(
    [property: JsonPropertyName("snapshotPath")] string SnapshotPath,
    [property: JsonPropertyName("issueCount")] int IssueCount);

// ---- metrics.json (Messkontrakt v1) — Finalize schreibt es; W2-Rohformat ----

public sealed record PipelineMetrics(
    [property: JsonPropertyName("runId")] string RunId,
    [property: JsonPropertyName("benchmarkVersion")] string BenchmarkVersion,
    [property: JsonPropertyName("policyProfile")] string PolicyProfile,
    [property: JsonPropertyName("models")] IReadOnlyDictionary<string, string> Models,
    [property: JsonPropertyName("stages")] IReadOnlyList<StageMetric> Stages,
    [property: JsonPropertyName("humanGates")] IReadOnlyList<HumanGateMetric> HumanGates,
    [property: JsonPropertyName("coreItemsBefore")] int CoreItemsBefore,
    [property: JsonPropertyName("coreItemsAfter")] int CoreItemsAfter,
    [property: JsonPropertyName("issuesCreated")] int IssuesCreated,
    [property: JsonPropertyName("issuesUpdated")] int IssuesUpdated,
    [property: JsonPropertyName("violations")] IReadOnlyList<string> Violations,
    [property: JsonPropertyName("recall")] RecallMetric? Recall);

public sealed record StageMetric(
    [property: JsonPropertyName("stage")] string Stage,
    [property: JsonPropertyName("wallMs")] long WallMs,
    [property: JsonPropertyName("attempts")] int Attempts,
    [property: JsonPropertyName("gate")] GateMetric? Gate,
    [property: JsonPropertyName("tokens")] TokenMetric Tokens);

public sealed record GateMetric(
    [property: JsonPropertyName("pass")] bool Pass,
    [property: JsonPropertyName("errors")] int Errors,
    [property: JsonPropertyName("warnings")] int Warnings);

public sealed record TokenMetric(
    [property: JsonPropertyName("in")] long In,
    [property: JsonPropertyName("out")] long Out);

public sealed record HumanGateMetric(
    [property: JsonPropertyName("gate")] string Gate,
    [property: JsonPropertyName("answeredBy")] string AnsweredBy);

public sealed record RecallMetric(
    [property: JsonPropertyName("value")] double Value,
    [property: JsonPropertyName("referencePath")] string ReferencePath);
