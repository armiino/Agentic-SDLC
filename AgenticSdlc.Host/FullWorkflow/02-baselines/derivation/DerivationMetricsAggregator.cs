using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Llm;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// B0 — OFFLINE-Aggregator der Ableitungsgüte über MEHRERE Läufe. Liest je Lauf den deterministischen Metrik-Vektor
/// (N1/N2/R1/R2) aus <c>derivations/&lt;spec&gt;/derivation-report.json</c> (dort beim Ableiten gestempelt), gruppiert
/// nach <c>mode</c> (structured · agentic · explore) und bildet je Metrik Mittelwert + Spannweite (min/max) über die
/// Wiederholungen (M2). So wird der Arm-Vergleich mit STREUUNG lesbar — ein Unterschied zählt nur bei
/// nicht-überlappenden Spannen (Entscheidungsregel in <c>docs/B0-Ableitungsguete-Metrik.md</c>).
/// </summary>
/// <remarks>
/// Deterministisch + LLM-frei by default (nur Arithmetik über bereits gestempelte Werte → beliebig re-aggregierbar).
/// <c>--judge</c> ergänzt R3 (Scope-Creep, <see cref="ScopeCreepChecker"/>): rekonstruiert je Lauf die Quellen aus der
/// <c>config.json</c> (sources + optional env) und prüft die abgeleiteten Items — die EINZIGE Judge-Dimension.
/// CLI: <c>derive-metrics &lt;spec&gt; &lt;runDir…&gt; [--judge] [model]</c>.
/// </remarks>
public static class DerivationMetricsAggregator
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly JsonSerializerOptions Read = new(JsonSerializerDefaults.Web);

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine($"Usage: derive-metrics <specId> <runDir1> [runDir2 …] [--judge] [model]   (specs: {string.Join(", ", DerivationRegistry.Specs.Keys)})");
            return 2;
        }
        var specId = args[1];
        if (!DerivationRegistry.TryGet(specId, out var spec))
        {
            Console.Error.WriteLine($"[derive-metrics] unbekannte specId '{specId}' (verfügbar: {string.Join(", ", DerivationRegistry.Specs.Keys)}).");
            return 2;
        }
        var judge = args.Contains("--judge");

        // Positional nach specId: existierende Verzeichnisse = Läufe; erstes Nicht-Dir/Nicht-Flag = Modell.
        var runDirs = new List<string>();
        string? modelArg = null;
        for (var i = 2; i < args.Length; i++)
        {
            var a = args[i];
            if (a.StartsWith("--", StringComparison.Ordinal)) continue;
            var resolved = Path.IsPathRooted(a) ? a : Path.Combine(repoRoot, a);
            if (Directory.Exists(resolved)) runDirs.Add(resolved);
            else if (modelArg is null) modelArg = a;
        }
        if (runDirs.Count == 0)
        {
            Console.Error.WriteLine("[derive-metrics] keine Run-Verzeichnisse gefunden (erwartet runs/derivation/<runId> …).");
            return 2;
        }

        // R3-Judge-Client (nur bei --judge): Jury-Modell bevorzugt, sonst Default/Override. Kein Run-Logging nötig (offline).
        ScopeCreepChecker? scopeChecker = null;
        if (judge)
        {
            var judgeSettings = modelArg is not null ? settings with { ModelId = modelArg }
                : !string.IsNullOrWhiteSpace(settings.JuryJudgeModel) ? settings with { ModelId = settings.JuryJudgeModel! } : settings;
            scopeChecker = new ScopeCreepChecker(ChatClientFactory.Create(judgeSettings), settings.JuryStructuredOutput);
            Console.WriteLine($"[derive-metrics] R3-Judge AN (model={judgeSettings.ModelId}).");
        }

        var perRun = new List<RunMetrics>();
        foreach (var dir in runDirs.OrderBy(d => d, StringComparer.Ordinal))
        {
            var reportPath = Path.Combine(dir, "derivations", spec.Id, "derivation-report.json");
            if (!File.Exists(reportPath)) { Console.WriteLine($"[derive-metrics] übersprungen (kein report): {Path.GetRelativePath(repoRoot, reportPath)}"); continue; }

            using var d = JsonDocument.Parse(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false));
            var root = d.RootElement;
            if (!root.TryGetProperty("metrics", out var mEl)) { Console.WriteLine($"[derive-metrics] übersprungen (kein metrics-Block, alter Lauf?): {Path.GetRelativePath(repoRoot, dir)}"); continue; }
            var m = mEl.Deserialize<DeterministicMetrics>(Read)!;
            var mode = root.TryGetProperty("mode", out var modeEl) ? modeEl.GetString() ?? "unknown" : "unknown";
            int? retrieved = root.TryGetProperty("retrievedCount", out var rc) ? rc.GetInt32() : null;
            int? usedCnt = root.TryGetProperty("usedCount", out var uc) ? uc.GetInt32() : null;

            // Verify-Arm-Kennzahlen (nur im explore-verify-Report vorhanden).
            int? rounds = null; double? deltaR1 = null; bool? dodPass = null;
            if (root.TryGetProperty("verify", out var vEl) && vEl.ValueKind == JsonValueKind.Object)
            {
                if (vEl.TryGetProperty("rounds", out var rr) && rr.ValueKind == JsonValueKind.Number) rounds = rr.GetInt32();
                if (vEl.TryGetProperty("deltaR1", out var dr) && dr.ValueKind == JsonValueKind.Number) deltaR1 = dr.GetDouble();
                if (vEl.TryGetProperty("dodPass", out var dp) && (dp.ValueKind is JsonValueKind.True or JsonValueKind.False)) dodPass = dp.GetBoolean();
            }

            // Accountable-Arm-Kennzahlen (nur im explore-account-Report vorhanden).
            double? coveredRate = null, dismissedRate = null, unaccountedRate = null, collisionRate = null;
            bool? coverageComplete = null, selfAccountingClean = null;
            if (root.TryGetProperty("coverage", out var cEl) && cEl.ValueKind == JsonValueKind.Object
                && cEl.TryGetProperty("total", out var tot) && tot.ValueKind == JsonValueKind.Number && tot.GetInt32() > 0)
            {
                double total = tot.GetInt32();
                if (cEl.TryGetProperty("covered", out var cv)) coveredRate = Math.Round(cv.GetInt32() / total, 4);
                if (cEl.TryGetProperty("accounted", out var ac)) dismissedRate = Math.Round(ac.GetInt32() / total, 4);
                if (cEl.TryGetProperty("unaccounted", out var un)) unaccountedRate = Math.Round(un.GetInt32() / total, 4);
                if (cEl.TryGetProperty("coverageComplete", out var cc) && (cc.ValueKind is JsonValueKind.True or JsonValueKind.False)) coverageComplete = cc.GetBoolean();
                // collisionRate = Anteil der Anker, die der Agent GLEICHZEITIG verworfen hat (Disjunktheits-Verletzung).
                if (cEl.TryGetProperty("collisionRate", out var cr) && cr.ValueKind == JsonValueKind.Number) collisionRate = cr.GetDouble();
                if (cEl.TryGetProperty("selfAccountingClean", out var sc) && (sc.ValueKind is JsonValueKind.True or JsonValueKind.False)) selfAccountingClean = sc.GetBoolean();
            }

            // Account-Verify-Arm: ΔCoverage (Selbstkorrektur via In-Loop-Feedback) — nur im explore-account-verify-Report.
            int? acctRounds = null; double? deltaUnaccounted = null;
            if (root.TryGetProperty("coverageDelta", out var cdEl) && cdEl.ValueKind == JsonValueKind.Object)
            {
                if (cdEl.TryGetProperty("rounds", out var ar) && ar.ValueKind == JsonValueKind.Number) acctRounds = ar.GetInt32();
                if (cdEl.TryGetProperty("deltaUnaccounted", out var du) && du.ValueKind == JsonValueKind.Number) deltaUnaccounted = du.GetInt32();
            }

            // Reflect-Arm-Kennzahlen (nur im …-reflect / …-reflect-graph-Report vorhanden): verbindliche, bounded Selbstkorrektur.
            int? reflectRounds = null; bool? needsRepair = null, overCorrected = null, firstDraftGatePass = null;
            if (root.TryGetProperty("reflect", out var rfEl) && rfEl.ValueKind == JsonValueKind.Object)
            {
                if (rfEl.TryGetProperty("rounds", out var rfr) && rfr.ValueKind == JsonValueKind.Number) reflectRounds = rfr.GetInt32();
                if (rfEl.TryGetProperty("needsRepair", out var nr) && (nr.ValueKind is JsonValueKind.True or JsonValueKind.False)) needsRepair = nr.GetBoolean();
                if (rfEl.TryGetProperty("overCorrected", out var oc) && (oc.ValueKind is JsonValueKind.True or JsonValueKind.False)) overCorrected = oc.GetBoolean();
                if (rfEl.TryGetProperty("firstDraftGatePass", out var fg) && (fg.ValueKind is JsonValueKind.True or JsonValueKind.False)) firstDraftGatePass = fg.GetBoolean();
            }

            double? r3 = null;
            if (scopeChecker is not null)
                r3 = await JudgeR3Async(dir, spec, scopeChecker, repoRoot).ConfigureAwait(false);

            perRun.Add(new RunMetrics(Path.GetFileName(dir), mode, m, retrieved, usedCnt, r3, rounds, deltaR1, dodPass,
                coveredRate, dismissedRate, unaccountedRate, coverageComplete, collisionRate, selfAccountingClean,
                acctRounds, deltaUnaccounted, reflectRounds, needsRepair, overCorrected, firstDraftGatePass));
        }
        if (perRun.Count == 0) { Console.Error.WriteLine("[derive-metrics] keine auswertbaren Läufe (Metrik-Block fehlt überall)."); return 2; }

        // Nach Modus gruppieren → je Arm Mittel + Spannweite über die Wiederholungen.
        var byMode = perRun.GroupBy(r => r.Mode, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => Aggregate(g.ToList()));

        var summary = new { spec = spec.Id, generatedAtUtc = DateTime.UtcNow, runs = perRun.Count, judged = judge, byMode };
        var outDir = Path.Combine(repoRoot, "runs", "derivation", "_metrics");
        Directory.CreateDirectory(outDir);
        var outPath = Path.Combine(outDir, $"{spec.Id}-{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
        await File.WriteAllTextAsync(outPath, JsonSerializer.Serialize(summary, Json)).ConfigureAwait(false);

        PrintTable(spec.Id, byMode, judge);
        Console.WriteLine($"[derive-metrics] Zusammenfassung -> {Path.GetRelativePath(repoRoot, outPath)}");
        return 0;
    }

    private static async Task<double?> JudgeR3Async(string runDir, DerivationSpec spec, ScopeCreepChecker checker, string repoRoot)
    {
        var derivedPath = Path.Combine(runDir, "derivations", spec.Id, "derived.json");
        if (!File.Exists(derivedPath)) return null;
        var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath).ConfigureAwait(false), Read);
        if (doc is null || doc.Items.Count == 0) return 0.0;

        // Quellen aus der config.json rekonstruieren (sources + optional env) → Anker-Texte für den Judge.
        var baselineById = await LoadSourcesAsync(runDir, repoRoot).ConfigureAwait(false);
        var report = await checker.CheckAsync(doc.Items, baselineById, CancellationToken.None).ConfigureAwait(false);
        return report.InventedRate;
    }

    private static async Task<IReadOnlyDictionary<string, ArtifactItem>> LoadSourcesAsync(string runDir, string repoRoot)
    {
        var map = new Dictionary<string, ArtifactItem>(StringComparer.Ordinal);
        var configPath = Path.Combine(runDir, "config.json");
        if (!File.Exists(configPath)) return map;
        var paths = new List<string>();
        string? envRel = null;
        using (var c = JsonDocument.Parse(await File.ReadAllTextAsync(configPath).ConfigureAwait(false)))
        {
            if (c.RootElement.TryGetProperty("sources", out var s) && s.ValueKind == JsonValueKind.Array)
                paths.AddRange(s.EnumerateArray().Select(e => e.GetString()).Where(x => !string.IsNullOrWhiteSpace(x))!);
            if (c.RootElement.TryGetProperty("env", out var e) && e.ValueKind == JsonValueKind.String) envRel = e.GetString();
        }
        var docs = new List<ArtifactDocument>();
        foreach (var rel in paths)
        {
            var abs = Path.IsPathRooted(rel!) ? rel! : Path.Combine(repoRoot, rel!);
            if (File.Exists(abs) && JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(abs).ConfigureAwait(false), Read) is { } dd) docs.Add(dd);
        }
        if (!string.IsNullOrWhiteSpace(envRel))
        {
            var envAbs = Path.IsPathRooted(envRel) ? envRel : Path.Combine(repoRoot, envRel);
            if (Directory.Exists(envAbs))
                foreach (var ef in Directory.EnumerateFiles(envAbs, "artifact.json", SearchOption.AllDirectories))
                    try { if (JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(ef).ConfigureAwait(false), Read) is { } dd) docs.Add(dd); }
                    catch (JsonException) { /* skip */ }
        }
        foreach (var it in docs.SelectMany(x => x.Items)) map.TryAdd(it.ItemId, it);
        return map;
    }

    private static ModeAggregate Aggregate(IReadOnlyList<RunMetrics> runs)
    {
        Stat S(Func<RunMetrics, double> f) => Stat.Of(runs.Select(f));
        var withRu = runs.Where(r => r.RetrievedCount is not null && r.UsedCount is not null).ToList();
        var withR3 = runs.Where(r => r.R3 is not null).ToList();
        var withV = runs.Where(r => r.VerifyRounds is not null).ToList();
        var withD = runs.Where(r => r.DeltaR1 is not null).ToList();
        var withDod = runs.Where(r => r.DodPass is not null).ToList();
        var withCov = runs.Where(r => r.CoveredRate is not null).ToList();
        var withCC = runs.Where(r => r.CoverageComplete is not null).ToList();
        var withColl = runs.Where(r => r.CollisionRate is not null).ToList();
        var withSac = runs.Where(r => r.SelfAccountingClean is not null).ToList();
        var withAcctR = runs.Where(r => r.AcctRounds is not null).ToList();
        var withDeltaUn = runs.Where(r => r.DeltaUnaccounted is not null).ToList();
        var withRf = runs.Where(r => r.ReflectRounds is not null).ToList();
        return new ModeAggregate(
            N: runs.Count,
            RunIds: runs.Select(r => r.RunId).ToArray(),
            D1_Items: S(r => r.Metrics.Items),
            N1_AvgAnchorsPerItem: S(r => r.Metrics.N1_AvgAnchorsPerItem),
            N1_CrossArtifactRate: S(r => r.Metrics.N1_CrossArtifactRate),
            N2_NonTriviality: S(r => r.Metrics.N2_AvgNonTriviality),
            R1_FidelityViolationRate: S(r => r.Metrics.R1_FidelityViolationRate),
            R2_InvalidAnchorRate: S(r => r.Metrics.R2_InvalidAnchorRate),
            R3_ScopeCreepRate: withR3.Count > 0 ? Stat.Of(withR3.Select(r => r.R3!.Value)) : null,
            D2_RetrievedUsedRatio: withRu.Count > 0 ? Stat.Of(withRu.Select(r => r.UsedCount!.Value == 0 ? 0.0 : (double)r.RetrievedCount!.Value / r.UsedCount!.Value)) : null,
            VerifyRounds: withV.Count > 0 ? Stat.Of(withV.Select(r => (double)r.VerifyRounds!.Value)) : null,
            DeltaR1: withD.Count > 0 ? Stat.Of(withD.Select(r => r.DeltaR1!.Value)) : null,
            DodPassRate: withDod.Count > 0 ? Stat.Of(withDod.Select(r => r.DodPass!.Value ? 1.0 : 0.0)) : null,
            CoveredRate: withCov.Count > 0 ? Stat.Of(withCov.Select(r => r.CoveredRate!.Value)) : null,
            DismissedRate: withCov.Count > 0 ? Stat.Of(withCov.Select(r => r.DismissedRate!.Value)) : null,
            UnaccountedRate: withCov.Count > 0 ? Stat.Of(withCov.Select(r => r.UnaccountedRate!.Value)) : null,
            CoverageCompleteRate: withCC.Count > 0 ? Stat.Of(withCC.Select(r => r.CoverageComplete!.Value ? 1.0 : 0.0)) : null,
            CollisionRate: withColl.Count > 0 ? Stat.Of(withColl.Select(r => r.CollisionRate!.Value)) : null,
            SelfAccountingCleanRate: withSac.Count > 0 ? Stat.Of(withSac.Select(r => r.SelfAccountingClean!.Value ? 1.0 : 0.0)) : null,
            AcctRounds: withAcctR.Count > 0 ? Stat.Of(withAcctR.Select(r => (double)r.AcctRounds!.Value)) : null,
            DeltaUnaccounted: withDeltaUn.Count > 0 ? Stat.Of(withDeltaUn.Select(r => r.DeltaUnaccounted!.Value)) : null,
            ReflectRounds: withRf.Count > 0 ? Stat.Of(withRf.Select(r => (double)r.ReflectRounds!.Value)) : null,
            LoopFireRate: withRf.Count > 0 ? Stat.Of(withRf.Select(r => r.ReflectRounds!.Value > 1 ? 1.0 : 0.0)) : null,
            NeedsRepairRate: withRf.Count > 0 ? Stat.Of(withRf.Select(r => r.NeedsRepair is true ? 1.0 : 0.0)) : null,
            FirstDraftGatePassRate: withRf.Count > 0 ? Stat.Of(withRf.Select(r => r.FirstDraftGatePass is true ? 1.0 : 0.0)) : null,
            OverCorrectedRate: withRf.Count > 0 ? Stat.Of(withRf.Select(r => r.OverCorrected is true ? 1.0 : 0.0)) : null);
    }

    private static void PrintTable(string spec, IReadOnlyDictionary<string, ModeAggregate> byMode, bool judged)
    {
        Console.WriteLine();
        Console.WriteLine($"=== B0 Ableitungsgüte · spec={spec} · Mittel [min–max] über Wiederholungen ===");
        Console.WriteLine($"{"Metrik",-26}" + string.Join("", byMode.Keys.Select(k => $"{k,-24}")));
        void Row(string label, Func<ModeAggregate, Stat?> f) =>
            Console.WriteLine($"{label,-26}" + string.Join("", byMode.Values.Select(a => $"{Fmt(f(a)),-24}")));
        Console.WriteLine($"{"n (Läufe)",-26}" + string.Join("", byMode.Values.Select(a => $"{a.N,-24}")));
        Console.WriteLine("-- NUTZEN (soll Agency heben) --");
        Row("N1 Ø Anker/Item", a => a.N1_AvgAnchorsPerItem);
        Row("N1 cross-artifact-Rate", a => a.N1_CrossArtifactRate);
        Row("N2 Nicht-Trivialität", a => a.N2_NonTriviality);
        Console.WriteLine("-- RISIKO (darf Agency NICHT heben) --");
        Row("R1 Treue-Verletzung", a => a.R1_FidelityViolationRate);
        Row("R2 Anker-Defekt", a => a.R2_InvalidAnchorRate);
        if (judged) Row("R3 Scope-Creep (Judge)", a => a.R3_ScopeCreepRate);
        if (byMode.Values.Any(a => a.DeltaR1 is not null || a.VerifyRounds is not null))
        {
            Console.WriteLine("-- VERIFY-ARM (Selbstkorrektur) --");
            Row("ΔR1 Korrektur-Gewinn", a => a.DeltaR1);
            Row("verify-Runden", a => a.VerifyRounds);
            Row("DoD-pass-Rate", a => a.DodPassRate);
        }
        if (byMode.Values.Any(a => a.CoveredRate is not null))
        {
            Console.WriteLine("-- ACCOUNTABLE-ARM (Coverage-Rechenschaft) --");
            Row("covered-Rate", a => a.CoveredRate);
            Row("dismissed-Rate", a => a.DismissedRate);
            Row("unaccounted-Rate", a => a.UnaccountedRate);
            Row("collision-Rate", a => a.CollisionRate);          // Anker, die zugleich verworfen wurden (Disjunktheit)
            Row("coverage-complete", a => a.CoverageCompleteRate); // host-disjungierte Sicht
            Row("self-acct-clean", a => a.SelfAccountingCleanRate); // EHRLICHE Sicht (lückenlos UND kollisionsfrei)
            if (byMode.Values.Any(a => a.DeltaUnaccounted is not null || a.AcctRounds is not null))
            {
                Row("Δunaccounted (Selbstkorr.)", a => a.DeltaUnaccounted); // >0 = Lücken via Feedback selbst geschlossen
                Row("acct-Runden", a => a.AcctRounds);                       // 0 = check_accountability nicht genutzt
            }
        }
        if (byMode.Values.Any(a => a.ReflectRounds is not null))
        {
            Console.WriteLine("-- REFLECT-ARM (verbindliche Selbstkorrektur, bounded) --");
            Row("reflect-Runden", a => a.ReflectRounds);                 // 1 = kein Retry, 2 = Loop feuerte einmal
            Row("loop-feuer-Rate", a => a.LoopFireRate);                 // Anteil Läufe mit >1 Runde (Retry gefeuert)
            Row("needsRepair-Rate", a => a.NeedsRepairRate);             // Anteil, der bounded aufgab (final fail, ehrlich markiert)
            Row("firstDraft-pass-Rate", a => a.FirstDraftGatePassRate);  // Anteil, der round 0 direkt bestand
            Row("over-correction-Rate", a => a.OverCorrectedRate);       // „silent killer": Loop verschlechterte schon-korrektes
        }
        Console.WriteLine("-- DESKRIPTIV (nicht gewertet) --");
        Row("D1 Item-Anzahl", a => a.D1_Items);
        Row("D2 retrieved/used", a => a.D2_RetrievedUsedRatio);
        Console.WriteLine();
    }

    private static string Fmt(Stat? s) => s is null ? "—" : $"{s.Mean:0.###} [{s.Min:0.###}–{s.Max:0.###}]";

    private sealed record RunMetrics(string RunId, string Mode, DeterministicMetrics Metrics, int? RetrievedCount, int? UsedCount, double? R3,
        int? VerifyRounds, double? DeltaR1, bool? DodPass,
        double? CoveredRate, double? DismissedRate, double? UnaccountedRate, bool? CoverageComplete,
        double? CollisionRate, bool? SelfAccountingClean,
        int? AcctRounds, double? DeltaUnaccounted,
        int? ReflectRounds, bool? NeedsRepair, bool? OverCorrected, bool? FirstDraftGatePass);
}

/// <summary>Mittelwert + Spannweite einer Metrik über die Wiederholungen (M2). Nicht-überlappende Spannen = echter Effekt.</summary>
public sealed record Stat(double Mean, double Min, double Max, int N)
{
    public static Stat Of(IEnumerable<double> xs)
    {
        var l = xs.ToList();
        return l.Count == 0 ? new Stat(0, 0, 0, 0) : new Stat(Math.Round(l.Average(), 4), l.Min(), l.Max(), l.Count);
    }
}

/// <summary>Aggregat EINES Arms (Modus) über seine Wiederholungen.</summary>
public sealed record ModeAggregate(
    int N,
    IReadOnlyList<string> RunIds,
    Stat D1_Items,
    Stat N1_AvgAnchorsPerItem,
    Stat N1_CrossArtifactRate,
    Stat N2_NonTriviality,
    Stat R1_FidelityViolationRate,
    Stat R2_InvalidAnchorRate,
    Stat? R3_ScopeCreepRate,
    Stat? D2_RetrievedUsedRatio,
    Stat? VerifyRounds,
    Stat? DeltaR1,
    Stat? DodPassRate,
    Stat? CoveredRate,
    Stat? DismissedRate,
    Stat? UnaccountedRate,
    Stat? CoverageCompleteRate,
    Stat? CollisionRate,
    Stat? SelfAccountingCleanRate,
    Stat? AcctRounds,
    Stat? DeltaUnaccounted,
    Stat? ReflectRounds,
    Stat? LoopFireRate,
    Stat? NeedsRepairRate,
    Stat? FirstDraftGatePassRate,
    Stat? OverCorrectedRate);
