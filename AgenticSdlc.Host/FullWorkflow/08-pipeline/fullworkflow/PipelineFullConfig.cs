using AgenticSdlc.Host.Configuration;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>W1e': Gate-Policy-Art. `replay` trägt zusätzlich einen Pfad zu den gespeicherten Entscheiden.</summary>
public enum GatePolicyKind
{
    /// <summary>Pausieren + Checkpoint, auf den Menschen warten (wie heute; UI wie ledger-adjudicate-ui).</summary>
    Interactive,
    /// <summary>Alle Ops akzeptieren — deklarierter EXPERIMENT-Modus.</summary>
    AcceptAll,
    /// <summary>Gespeicherte Entscheide (queue/decision-log) per ItemId einspielen — Standard für N≥3.</summary>
    Replay
}

/// <summary>
/// Aufgelöste Gate-Policy. <see cref="Parse"/> liest die run-config-Strings
/// (<c>interactive</c> | <c>accept-all</c> | <c>replay:&lt;pfad&gt;</c>).
/// </summary>
public sealed record GatePolicy(GatePolicyKind Kind, string? ReplayPath = null)
{
    public static readonly GatePolicy Interactive = new(GatePolicyKind.Interactive);

    public static GatePolicy Parse(string? raw)
    {
        var s = raw?.Trim();
        if (string.IsNullOrWhiteSpace(s)) return Interactive;

        var lower = s.ToLowerInvariant();
        if (lower is "interactive") return Interactive;
        if (lower is "accept-all" or "acceptall") return new GatePolicy(GatePolicyKind.AcceptAll);
        if (lower.StartsWith("replay", StringComparison.Ordinal))
        {
            var idx = s.IndexOf(':');
            var path = idx >= 0 ? s[(idx + 1)..].Trim() : null;
            return new GatePolicy(GatePolicyKind.Replay, string.IsNullOrWhiteSpace(path) ? null : path);
        }
        // Unbekannt → sicherste Governance-Wahl (Gates sind heilig): pausieren.
        return Interactive;
    }
}

/// <summary>
/// W1e': aufgelöste `run-config.fullworkflow`-Einstellungen. Defaults angewandt; nur vom pipeline-full-Runner
/// + zentralem Gate-Responder gelesen.
/// </summary>
public sealed record FullWorkflowSettings(
    string? Transcript,
    string? Repo,
    string? TokenEnv,
    bool Execute,
    GatePolicy PolicyProfile,
    IReadOnlyDictionary<string, string> Models,
    IReadOnlyDictionary<string, int> MaxAttempts,
    IReadOnlyDictionary<string, string> Stages,
    IReadOnlyDictionary<string, GatePolicy> Gates)
{
    /// <summary>v1: l3 ist per Default AUS (Betriebs-Zyklus braucht es nicht).</summary>
    public bool L3Enabled =>
        Stages.TryGetValue("l3", out var v) && !string.Equals(v, "off", StringComparison.OrdinalIgnoreCase);

    /// <summary>Policy je Gate: expliziter Gate-Eintrag &gt; PolicyProfile (Standard).</summary>
    public GatePolicy GateFor(string gateName)
        => Gates.TryGetValue(gateName, out var p) ? p : PolicyProfile;

    public static FullWorkflowSettings FromConfig(FullWorkflowConfig? cfg)
    {
        cfg ??= new FullWorkflowConfig();
        return new FullWorkflowSettings(
            Transcript: Trim(cfg.Transcript),
            Repo: Trim(cfg.Repo),
            TokenEnv: Trim(cfg.TokenEnv),
            Execute: cfg.Execute ?? false, // Default: KEIN externer Write
            PolicyProfile: GatePolicy.Parse(cfg.PolicyProfile),
            Models: cfg.Models is null ? Empty<string>() : new Dictionary<string, string>(cfg.Models),
            MaxAttempts: cfg.MaxAttempts is null ? Empty<int>() : new Dictionary<string, int>(cfg.MaxAttempts),
            // Fehlt der stages-Block ganz → v1-Default l3=off.
            Stages: cfg.Stages is null
                ? new Dictionary<string, string> { ["l3"] = "off" }
                : new Dictionary<string, string>(cfg.Stages),
            Gates: cfg.Gates is null
                ? new Dictionary<string, GatePolicy>()
                : cfg.Gates.ToDictionary(kv => kv.Key, kv => GatePolicy.Parse(kv.Value?.Policy)));
    }

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static IReadOnlyDictionary<string, T> Empty<T>() => new Dictionary<string, T>();
}
