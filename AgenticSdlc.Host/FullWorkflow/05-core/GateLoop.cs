using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;

// Geteilte Primitive des gate-getriebenen Maker-Checker-Repair-Loops (R7) — EINE Entscheidungsform fuer alle
// agentischen Knoten (GithubForward, pbi-update, Decision, Requirement-Resolver), Form des CheckerRepairWorkflow.
public enum GateDecision { Pass, Repair, HumanReview, MaxAttemptsReached }

public static class Repairability
{
    public const string Repairable = "repairable"; // agentische Entscheidung, per GateFeedback fixbar
    public const string Hard = "hard";             // Infrastruktur/Schema -> kein Repair
    public const string NeedsHuman = "needs_human"; // fachlich strittig -> Mensch
}

// Eine Attempt-Zeile pro Gate-Durchlauf (Nachvollziehbarkeit + Thesis: hat GateFeedback geholfen?).
public sealed record GateAttempt(
    [property: JsonPropertyName("attemptNumber")] int AttemptNumber,
    [property: JsonPropertyName("source")] string Source,          // "maker" | "repair"
    [property: JsonPropertyName("gatePass")] bool GatePass,
    [property: JsonPropertyName("decision")] string Decision,
    [property: JsonPropertyName("errors")] IReadOnlyList<string> Errors,
    [property: JsonPropertyName("timestampUtc")] DateTime TimestampUtc);

public static class GateLoop
{
    // Exakt CheckerExecutor.Decide: pass -> Pass; Schranke -> MaxAttempts; reparierbar -> Repair; sonst -> Human.
    public static GateDecision Decide(bool pass, bool hasRepairable, int attempt, int maxAttempts)
        => pass ? GateDecision.Pass
            : attempt >= maxAttempts ? GateDecision.MaxAttemptsReached
            : hasRepairable ? GateDecision.Repair
            : GateDecision.HumanReview;
}
