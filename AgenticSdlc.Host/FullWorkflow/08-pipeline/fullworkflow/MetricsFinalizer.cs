using System.Globalization;
using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.Run;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

/// <summary>
/// W1e' Bauplan-Schritt 5 — der Metrics-Finalizer. REINER SAMMLER (deterministisch, kein LLM): liest die bereits
/// vorhandenen Faden-Artefakte + Sub-Run-Telemetrie und befüllt das vorhandene <see cref="PipelineMetrics"/>-Record.
/// Erfindet NICHTS neu — Middlewares/Stufen liefern die Daten schon.
/// </summary>
/// <remarks>
/// Token-Zuordnung (design-note §9, „Finalizer-Attribution"): Ledger/Baselines aus den Sub-Run-otel-Dateien (über die
/// <c>STAGE_*_DONE</c>-Pointer <c>ledgerRunId</c>/<c>recipeRun</c>); Ingest/Pbi/Forward aus der Faden-otel-Datei,
/// gefenstert nach Event-Zeitstempeln (<c>STAGE_BACKHALF_START</c> → <c>PIPELINE_BRIDGE</c> → <c>STAGE_FORWARD_START</c>).
/// Braucht den geflushten Faden-Exporter (Aufruf NACH dessen Dispose).
/// </remarks>
public static class MetricsFinalizer
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    /// <summary>Baut die Metriken aus dem Faden-Ordner und schreibt <c>metrics.json</c> in <c>logs/</c>. Fehler beim
    /// Sammeln dürfen den Lauf NICHT kippen (Metrics sind Analyse, nicht Governance) — daher defensiv.</summary>
    public static async Task WriteAsync(RunContext run, string repoRoot, FullWorkflowSettings fw, HostSettings settings,
        int coreItemsAfter, CancellationToken ct = default)
    {
        try
        {
            var metrics = Build(run.RunDir, repoRoot, fw, settings.ModelId, coreItemsAfter);
            var path = Path.Combine(run.LogsDir, "metrics.json");
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(metrics, Json), ct).ConfigureAwait(false);
            run.AppendEvent(new { type = "METRICS_WRITTEN", runId = run.RunId, stages = metrics.Stages.Count, path = "logs/metrics.json", timestampUtc = DateTime.UtcNow });
            Console.WriteLine($"[pipeline-full] metrics.json geschrieben ({metrics.Stages.Count} Stufen).");
        }
        catch (Exception ex)
        {
            run.AppendEvent(new { type = "METRICS_FAILED", runId = run.RunId, error = ex.Message, timestampUtc = DateTime.UtcNow });
            Console.Error.WriteLine($"[pipeline-full] metrics.json fehlgeschlagen (Lauf NICHT betroffen): {ex.Message}");
        }
    }

    /// <summary>Pure Aggregation über einen ABGESCHLOSSENEN Faden-Ordner — testbar gegen Referenz-Läufe.</summary>
    public static PipelineMetrics Build(string runDir, string repoRoot, FullWorkflowSettings fw, string defaultModel, int coreItemsAfter)
    {
        var events = ReadEvents(Path.Combine(runDir, "logs", "events.jsonl"));
        var config = ReadConfig(Path.Combine(runDir, "config.json"));

        // --- Zeit-Anker aus den Stufen-Events (fehlt einer, bleibt das Fenster offen → best effort) ---
        DateTime? t0 = EventTime(events, "PIPELINE_START");
        DateTime? tLedger = EventTime(events, "STAGE_LEDGER_DONE");
        DateTime? tAdj = EventTime(events, "STAGE_ADJUDICATION_DONE");
        DateTime? tBaselines = EventTime(events, "STAGE_BASELINES_DONE");
        DateTime? tDelta = EventTime(events, "DELTA_BUILT");
        DateTime? tBackhalf = EventTime(events, "STAGE_BACKHALF_START");
        DateTime? tBridge = EventTime(events, "PIPELINE_BRIDGE");
        DateTime? tForward = EventTime(events, "STAGE_FORWARD_START");
        DateTime? tEnd = EventTime(events, "GITHUB_FWD_DONE") ?? EventTime(events, "PIPELINE_RUN_DONE");
        // R-28: Bootstrap-Zweig hat eigene Stufen/Fenster (Ingest/Pbi laufen dort NIE — ihre Betriebs-Anker fehlen,
        // und ohne Anker würde SumTokens fensterlos den GANZEN Faden doppelt zuschreiben).
        DateTime? tBoot = EventTime(events, "STAGE_BOOTSTRAP_START");
        DateTime? tClusterApplied = EventTime(events, "PIPELINE_CLUSTER_APPLIED") ?? EventTime(events, "CLUSTER_GATE_REQUEST");
        var bootstrapBranch = tBoot.HasValue;

        var fadenOtel = Path.Combine(runDir, "logs", "otel-traces.jsonl");
        var ledgerRunId = EventString(events, "STAGE_LEDGER_DONE", "ledgerRunId");
        var recipeRun = EventString(events, "STAGE_BASELINES_DONE", "recipeRun");

        var stages = new List<StageMetric>();

        // 01-ledger: Token aus Sub-Run (ganze Datei); Gate/Attempts aus dem Stufen-Event.
        stages.Add(new StageMetric("01-ledger", Wall(t0, tLedger),
            Attempts: 1,
            Gate: EventBool(events, "STAGE_LEDGER_DONE", "gatePass") is { } lp ? new GateMetric(lp, EventInt(events, "STAGE_LEDGER_DONE", "needsHuman") ?? 0, 0) : null,
            Tokens: SumTokens(SubRunOtel(repoRoot, "ledger", ledgerRunId), null, null)));

        // 02-baselines: Token aus Recipe-Sub-Run.
        stages.Add(new StageMetric("02-baselines", Wall(tAdj ?? tLedger, tBaselines),
            Attempts: 1, Gate: null,
            Tokens: SumTokens(SubRunOtel(repoRoot, "recipe", recipeRun), null, null)));

        // 04-delta: deterministisch (kein LLM) → 0 Token, kein Gate.
        stages.Add(new StageMetric("04-delta", Wall(tBaselines, tDelta ?? tBackhalf), 1, null, new TokenMetric(0, 0)));

        if (bootstrapBranch)
        {
            // 06-backlog-cluster: Faden-otel gefenstert [Bootstrap .. Cluster-Applied] (Maker+ReviewAgent).
            stages.Add(new StageMetric("06-backlog-cluster", Wall(tBoot, tClusterApplied),
                Attempts: 1,
                Gate: ReadGate(runDir, Path.Combine("06-backlog", "clusters"), "cluster-gate-report.json"),
                Tokens: SumTokens(fadenOtel, tBoot, tClusterApplied)));

            // 06-backlog-clarify: [Cluster-Applied .. Forward] (ClarifyAgent + det. Applies/Seed).
            stages.Add(new StageMetric("06-backlog-clarify", Wall(tClusterApplied, tForward ?? tEnd),
                Attempts: 1,
                Gate: ReadGate(runDir, Path.Combine("06-backlog", "backlog"), "backlog-gate-report.json"),
                Tokens: SumTokens(fadenOtel, tClusterApplied, tForward ?? tEnd)));
        }
        else
        {
            // 07-ingest: Faden-otel gefenstert [Backhalf .. Bridge]; Gate/Attempts aus Report/Summary.
            stages.Add(new StageMetric("07-ingest", Wall(tBackhalf, tBridge),
                Attempts: SummaryInt(runDir, "07-ingest", "ingestion-summary.json", "attempts") ?? 1,
                Gate: ReadGate(runDir, "07-ingest", "ingestion-gate-report.json"),
                Tokens: SumTokens(fadenOtel, tBackhalf, tBridge)));

            // 07-pbi-update: Faden-otel gefenstert [Bridge .. Forward].
            stages.Add(new StageMetric("07-pbi-update", Wall(tBridge, tForward),
                Attempts: SummaryInt(runDir, "07-pbi-update", "pbi-update-summary.json", "attempts") ?? 1,
                Gate: ReadGate(runDir, "07-pbi-update", "pbi-update-gate-report.json"),
                Tokens: SumTokens(fadenOtel, tBridge, tForward)));
        }

        // 07-github (Forward): Faden-otel gefenstert [Forward .. Ende].
        stages.Add(new StageMetric("07-github", Wall(tForward, tEnd),
            Attempts: SummaryInt(runDir, "07-github", "github-forward-summary.json", "attempts") ?? 1,
            Gate: ReadGate(runDir, "07-github", "github-forward-gate-report.json"),
            Tokens: SumTokens(fadenOtel, tForward, tEnd)));

        // --- Human-Gates: GATE_ANSWERED-Events ---
        var humanGates = events
            .Where(e => e.Type == "GATE_ANSWERED")
            .Select(e => new HumanGateMetric(
                Str(e.Element, "gate") ?? Str(e.Element, "portId") ?? "?",
                Str(e.Element, "policy") ?? Str(e.Element, "answeredBy") ?? fw.PolicyProfile.Kind.ToString()))
            .ToList();

        // --- Issues aus dem Forward-Summary (byKind) ---
        var byKind = ReadForwardByKind(runDir);
        var issuesCreated = byKind.GetValueOrDefault("CREATE_ISSUE", 0);
        var issuesUpdated = byKind.GetValueOrDefault("UPDATE_ISSUE", 0);

        // --- Core-Items vorher (Artefakt) / nachher (vom Aufrufer, Live-Core vor Restore) ---
        var coreBefore = CoreCount(Path.Combine(runDir, "07-ingest", "applied", "core-before.json")) ?? 0;

        // --- Violations: gesammelte Gate-Fehlercodes als System-internes QA-Signal (Messkontrakt M-3 (i)) ---
        var violations = stages
            .Where(s => s.Gate is { Errors: > 0 })
            .Select(s => $"{s.Stage}:gate-errors={s.Gate!.Errors}")
            .ToList();

        var models = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var kv in fw.Models) models[kv.Key] = kv.Value;
        models["default"] = defaultModel;

        return new PipelineMetrics(
            RunId: Path.GetFileName(runDir.TrimEnd('/', '\\')),
            BenchmarkVersion: Str(config, "transcript") ?? "unknown",
            PolicyProfile: Str(config, "policyProfile") ?? fw.PolicyProfile.Kind.ToString(),
            Models: models,
            Stages: stages,
            HumanGates: humanGates,
            CoreItemsBefore: coreBefore,
            CoreItemsAfter: coreItemsAfter,
            IssuesCreated: issuesCreated,
            IssuesUpdated: issuesUpdated,
            Violations: violations,
            Recall: null); // M-4: Gold-Referenz noch nicht erhoben.
    }

    // ---------------- Helpers ----------------

    private readonly record struct Ev(string Type, DateTime? Time, JsonElement Element);

    private static List<Ev> ReadEvents(string path)
    {
        var list = new List<Ev>();
        if (!File.Exists(path)) return list;
        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            JsonElement el;
            try { el = JsonDocument.Parse(line).RootElement.Clone(); } catch { continue; }
            var type = Str(el, "type");
            if (type is null) continue;
            list.Add(new Ev(type, ParseTime(Str(el, "timestampUtc")), el));
        }
        return list;
    }

    private static JsonElement ReadConfig(string path)
    {
        if (!File.Exists(path)) return default;
        try { return JsonDocument.Parse(File.ReadAllText(path)).RootElement.Clone(); } catch { return default; }
    }

    private static DateTime? EventTime(List<Ev> events, string type) => events.FirstOrDefault(e => e.Type == type).Time;
    private static string? EventString(List<Ev> events, string type, string key)
    { var e = events.FirstOrDefault(x => x.Type == type); return e.Type is null ? null : Str(e.Element, key); }
    private static bool? EventBool(List<Ev> events, string type, string key)
    { var e = events.FirstOrDefault(x => x.Type == type); return e.Type is null ? null : Bool(e.Element, key); }
    private static int? EventInt(List<Ev> events, string type, string key)
    { var e = events.FirstOrDefault(x => x.Type == type); return e.Type is null ? null : Int(e.Element, key); }

    private static long Wall(DateTime? from, DateTime? to)
        => from.HasValue && to.HasValue && to.Value >= from.Value ? (long)(to.Value - from.Value).TotalMilliseconds : 0;

    private static string SubRunOtel(string repoRoot, string stufe, string? runId)
        => runId is null ? "" : Path.Combine(repoRoot, "runs", stufe, runId, "logs", "otel-traces.jsonl");

    /// <summary>Summiert gen_ai-Token über alle chat-Spans; optionales [from,to)-Zeitfenster (nach startTimeUtc).</summary>
    private static TokenMetric SumTokens(string otelPath, DateTime? from, DateTime? to)
    {
        if (string.IsNullOrEmpty(otelPath) || !File.Exists(otelPath)) return new TokenMetric(0, 0);
        long tin = 0, tout = 0;
        foreach (var line in File.ReadLines(otelPath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            JsonElement s;
            try { s = JsonDocument.Parse(line).RootElement; } catch { continue; }
            var name = Str(s, "name");
            if (name is null || !name.StartsWith("chat", StringComparison.Ordinal)) continue;
            if (from.HasValue || to.HasValue)
            {
                var st = ParseTime(Str(s, "startTimeUtc"));
                if (!st.HasValue) continue;
                if (from.HasValue && st.Value < from.Value) continue;
                if (to.HasValue && st.Value >= to.Value) continue;
            }
            if (s.TryGetProperty("tags", out var tags) && tags.ValueKind == JsonValueKind.Object)
            {
                tin += LongTag(tags, "gen_ai.usage.input_tokens");
                tout += LongTag(tags, "gen_ai.usage.output_tokens");
            }
        }
        return new TokenMetric(tin, tout);
    }

    private static GateMetric? ReadGate(string runDir, string stufe, string file)
    {
        var path = Path.Combine(runDir, stufe, file);
        if (!File.Exists(path)) return null;
        try
        {
            var d = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
            var pass = Bool(d, "pass") ?? Bool(d, "gatePass") ?? false;
            var errors = CountOrInt(d, "errors", "gateErrors");
            var warnings = CountOrInt(d, "warnings", "gateWarnings");
            return new GateMetric(pass, errors, warnings);
        }
        catch { return null; }
    }

    private static int? SummaryInt(string runDir, string stufe, string file, string key)
    {
        var path = Path.Combine(runDir, stufe, file);
        if (!File.Exists(path)) return null;
        try { return Int(JsonDocument.Parse(File.ReadAllText(path)).RootElement, key); } catch { return null; }
    }

    private static IReadOnlyDictionary<string, int> ReadForwardByKind(string runDir)
    {
        var path = Path.Combine(runDir, "07-github", "github-forward-summary.json");
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        if (!File.Exists(path)) return result;
        try
        {
            var d = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
            if (d.TryGetProperty("byKind", out var bk) && bk.ValueKind == JsonValueKind.Object)
                foreach (var p in bk.EnumerateObject())
                    if (p.Value.ValueKind == JsonValueKind.Number) result[p.Name] = p.Value.GetInt32();
        }
        catch { /* best effort */ }
        return result;
    }

    private static int? CoreCount(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var d = JsonDocument.Parse(File.ReadAllText(path)).RootElement;
            if (d.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array) return items.GetArrayLength();
            if (d.TryGetProperty("Items", out var items2) && items2.ValueKind == JsonValueKind.Array) return items2.GetArrayLength();
            return null;
        }
        catch { return null; }
    }

    // --- JSON-Zugriff (null-tolerant) ---
    private static string? Str(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static bool? Bool(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) && (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False) ? v.GetBoolean() : null;

    private static int? Int(JsonElement e, string key)
        => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

    private static int CountOrInt(JsonElement e, string arrayOrIntKey, string fallbackIntKey)
    {
        if (e.TryGetProperty(arrayOrIntKey, out var v))
        {
            if (v.ValueKind == JsonValueKind.Array) return v.GetArrayLength();
            if (v.ValueKind == JsonValueKind.Number) return v.GetInt32();
        }
        return Int(e, fallbackIntKey) ?? 0;
    }

    private static long LongTag(JsonElement tags, string key)
    {
        if (!tags.TryGetProperty(key, out var v)) return 0;
        return v.ValueKind switch
        {
            JsonValueKind.Number => v.TryGetInt64(out var n) ? n : 0,
            JsonValueKind.String => long.TryParse(v.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : 0,
            _ => 0
        };
    }

    private static DateTime? ParseTime(string? s)
        => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var t) ? t : null;
}
