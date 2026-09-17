using AgenticSdlc.Host.Configuration;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>W1e': Gate-Policy-Art. `replay` trägt zusätzlich einen Pfad zu den gespeicherten Entscheiden.</summary>
public enum GatePolicyKind
{
    /// <summary>DURABLE Pause: Checkpoint + pointer.json, Prozess ENDET; weiter via `pipeline-full resume`.</summary>
    Interactive,
    /// <summary>Alle Ops akzeptieren — deklarierter EXPERIMENT-Modus.</summary>
    AcceptAll,
    /// <summary>Gespeicherte Entscheide (queue/decision-log) per ItemId einspielen — Standard für N≥3.</summary>
    Replay,
    /// <summary>INLINE: Prozess bleibt am Gate stehen (Host-Muster A), öffnet optional die UI (`--open-ui`)
    /// und wartet auf die Entscheid-Datei; ohne Terminal degradiert er zur durablen Pause. Checkpoints
    /// laufen weiter (Crash-Schutz bleibt).</summary>
    InteractiveInline
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
        if (lower is "interactive-inline" or "inline") return new GatePolicy(GatePolicyKind.InteractiveInline);
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

/// <summary>Bootstrap-Plan B0: Phasen-Wahl der Hinterhälfte. `Auto` = Core-Detektion (Produktweg).</summary>
public enum PipelineMode
{
    /// <summary>Core existiert → Betriebs-Zweig, sonst Bootstrap-Zweig (ExistsAsync-Detektion).</summary>
    Auto,
    /// <summary>Erzwungener Bootstrap-Zweig (leer → Core → Backlog) — kontrollierte Läufe/Ablation.</summary>
    Bootstrap,
    /// <summary>Erzwungener Betriebs-Zweig (bestehender Core → update).</summary>
    Operational
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
    IReadOnlyDictionary<string, GatePolicy> Gates,
    PipelineMode Mode = PipelineMode.Auto,
    int TimeoutMinutes = 20)
{
    /// <summary>B0: `fullworkflow.mode` lesen; unbekannt/leer → Auto (Detektion = sicherste Wahl).</summary>
    public static PipelineMode ParseMode(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        "bootstrap" => PipelineMode.Bootstrap,
        "operational" => PipelineMode.Operational,
        _ => PipelineMode.Auto,
    };

    /// <summary>
    /// B0: die EINE Phasen-Wahl-Regel (pure Funktion, testbar). Der Runner füttert sie mit dem
    /// `ICoreRepository.ExistsAsync`-Ergebnis — Detektion passiert genau einmal, vor der Hinterhälfte.
    /// </summary>
    public static bool UseBootstrapBranch(PipelineMode mode, bool coreExists) => mode switch
    {
        PipelineMode.Bootstrap => true,
        PipelineMode.Operational => false,
        _ => !coreExists,
    };
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
            Mode: ParseMode(cfg.Mode),
            TimeoutMinutes: cfg.TimeoutMinutes is > 0 ? cfg.TimeoutMinutes.Value : 20,
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
