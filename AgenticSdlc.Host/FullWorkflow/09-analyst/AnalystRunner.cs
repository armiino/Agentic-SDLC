using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B: der geteilte Runner-Kern der Core-Analyse (K13: CLI `core-analysis` und Steward-Seil
/// `run_core_analysis` sind Häute über GENAU dieser Naht). Baut den Graph JE LAUF (State-Isolation),
/// führt in-process aus (Fehler-Events LAUT — R-18) und liefert die Artefakt-Pfade zurück.
/// Der Lauf liest nur (kein Gate, kein Wahrheits-Write) — Ergebnis = runs/core-analysis/&lt;id&gt;/
/// report.md + report.json + delta.json (für den GETRENNTEN Tor-Lauf via --from-delta).
/// </summary>
public static class AnalystRunner
{
    public sealed record Ergebnis(string RunId, int InsDelta, string ReportPath, string? DeltaPath);

    public static async Task<Ergebnis> RunCoreAnalysisAsync(HostSettings settings, string repoRoot)
    {
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Core fehlt (state/core) — nichts zu analysieren.");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var run = new RunContext(RunId.New(), "core-analysis");
        run.EnsureFolders();
        var outDir = run.OutputDir("analysis");
        Console.WriteLine($"[core-analysis] Lauf {run.RunId}: {AnalystLenses.All.Count} Linsen über {core.Items.Count} Core-Items (nur Lesen — Wahrheits-Wirkung erst im Tor-Lauf).");

        // NEU vs. WEITERHIN OFFEN: IdentityKeys des jüngsten Vorgänger-Reports (leer beim ersten Lauf).
        var vorgaengerKeys = LadeVorgaengerKeys(repoRoot, run.RunId);

        var lensFactory = PipelineAgents.Factory(repoRoot, settings, settings, run, "CoreAnalystAgent", "CoreAnalystAgent1");
        var kritikerFactory = PipelineAgents.Factory(repoRoot, settings, settings, run, "CoreAnalystKritiker", "CoreAnalystKritiker1");
        var workflow = AnalystWorkflow.Build(core, vorgaengerKeys, lensFactory, kritikerFactory, run, outDir);

        var wfRun = await InProcessExecution.Default
            .RunAsync(workflow, new AnalystWorkflow.Trigger(), run.RunId, CancellationToken.None).ConfigureAwait(false);
        // R-18: Fehler-Events IMMER behandeln — sonst „endet" der Lauf still.
        var fehler = wfRun.OutgoingEvents.FirstOrDefault(e => e is ExecutorFailedEvent or WorkflowErrorEvent);
        if (fehler is not null)
            throw new InvalidOperationException($"Analyse-Lauf {run.RunId} FEHLGESCHLAGEN: {fehler}");

        var output = wfRun.OutgoingEvents.OfType<WorkflowOutputEvent>().Select(e => e.Data).OfType<string>().Single();
        using var doc = JsonDocument.Parse(output);
        var ergebnis = new Ergebnis(run.RunId,
            doc.RootElement.GetProperty("insDelta").GetInt32(),
            doc.RootElement.GetProperty("report").GetString()!,
            doc.RootElement.TryGetProperty("deltaPath", out var d) && d.ValueKind == JsonValueKind.String ? d.GetString() : null);
        Console.WriteLine($"[core-analysis] FERTIG: {ergebnis.InsDelta} Fund(e) im Delta — Report: {Path.GetRelativePath(repoRoot, ergebnis.ReportPath)}"
            + (ergebnis.DeltaPath is null ? " (kein Delta — nichts vorzuschlagen)" : $" · Tor-Lauf: pipeline-full run --from-delta {Path.GetRelativePath(repoRoot, ergebnis.DeltaPath)}"));
        return ergebnis;
    }

    /// <summary>CLI-Haut: `core-analysis run` (bewusster LLM-Akt — Kosten ≈ eine Tor-1-Resolver-Runde).</summary>
    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 2 || !string.Equals(args[1], "run", StringComparison.OrdinalIgnoreCase))
        { Console.Error.WriteLine("Usage: core-analysis run   (Lücken-Analyse über den ganzen Core; LLM-Kosten!)"); return 2; }
        await RunCoreAnalysisAsync(settings, repoRoot).ConfigureAwait(false);
        return 0;
    }

    /// <summary>Klasse-Regel (Abnahme-Fund 20.08.): die Funde eines Analyse-Laufs LESBAR fürs Gespräch —
    /// geteilte Projektion für das run_core_analysis-Ergebnis UND das read_analysis_report-Lese-Tool.
    /// `index` (1-basiert) ist der stabile Anker für die Auswahl (curate_analysis_delta).</summary>
    public static object LiesFunde(string repoRoot, string runId)
    {
        var path = Path.Combine(repoRoot, "runs", "core-analysis", runId, "analysis", "report.json");
        if (!File.Exists(path)) return new { error = "REPORT_MISSING", runId };
        var report = JsonSerializer.Deserialize<AnalystReport>(File.ReadAllText(path), JsonFiles.Json)!;
        var deltaRel = Path.Combine("runs", "core-analysis", runId, "analysis", "delta.json");
        var auswahlRel = Path.Combine("runs", "core-analysis", runId, "analysis", "delta-auswahl.json");
        return new
        {
            runId,
            // Selbstbeschreibung (20.08., Autor-Fund „muesste ich den Pfad kennen?"): die Tor-Lauf-Pfade
            // reisen MIT — auch eine frische Session kann ohne Pfad-Wissen weiterarbeiten.
            deltaPath = File.Exists(Path.Combine(repoRoot, deltaRel)) ? deltaRel : null,
            auswahlDeltaPath = File.Exists(Path.Combine(repoRoot, auswahlRel)) ? auswahlRel : null,
            insDelta = report.InsDelta.Select((e, i) => new
            { index = i + 1, status = e.Status, e.Fund.Linse, e.Fund.Kategorie, e.Fund.Disposition, e.Fund.Statement, e.Fund.Herleitung, anker = e.Fund.AnkerIds }),
            aussortiert = report.Eintraege.Where(e => e.Status == AnalystStatus.KritikerAussortiert)
                .Select(e => new { e.Fund.Statement, grund = e.Grund }),
            bereitsBekannt = report.Eintraege.Where(e => e.Status.StartsWith("dedup_", StringComparison.Ordinal))
                .Select(e => new { e.Status, e.Fund.Statement }),
        };
    }

    /// <summary>AUSWAHL ≠ Bearbeitung (Autor-⚖ 20.08.): kuratiertes Teil-Delta aus GEWÄHLTEN Funden
    /// (1-basierte indices aus LiesFunde) — Statements WÖRTLICH, Herkunft CoreAnalyst intakt, das
    /// Voll-Delta bleibt unberührter Beleg. Nicht Gewähltes ist NICHT abgelehnt (kein R-35-Eintrag) —
    /// es bleibt offen und erscheint in der nächsten Analyse als WEITERHIN OFFEN. Text-Änderungen/
    /// Ergänzungen laufen NIE hierüber, sondern als Diktat (Herkunft AuthorFront).</summary>
    public static (string Path, int Count, IReadOnlyList<string> Fehler) CurateDelta(
        string repoRoot, string runId, IReadOnlyList<int> indices)
    {
        var reportPath = Path.Combine(repoRoot, "runs", "core-analysis", runId, "analysis", "report.json");
        if (!File.Exists(reportPath)) return ("", 0, [$"REPORT_MISSING: {runId}"]);
        var insDelta = JsonSerializer.Deserialize<AnalystReport>(File.ReadAllText(reportPath), JsonFiles.Json)!.InsDelta;

        var fehler = indices.Where(i => i < 1 || i > insDelta.Count)
            .Select(i => $"index {i} ungueltig (1..{insDelta.Count})").ToList();
        if (indices.Count == 0) fehler.Add("keine indices — Auswahl braucht mindestens einen Fund.");
        if (fehler.Count > 0) return ("", 0, fehler);

        var auswahl = indices.Distinct().OrderBy(i => i).Select(i => insDelta[i - 1].Fund).ToList();
        var delta = AnalystDeltaBuilder.Build(auswahl, runId);
        var path = Path.Combine(repoRoot, "runs", "core-analysis", runId, "analysis", "delta-auswahl.json");
        File.WriteAllText(path, JsonSerializer.Serialize(delta, ProjectStateJson.Options));
        return (path, auswahl.Count, []);
    }

    internal static IReadOnlySet<string> LadeVorgaengerKeys(string repoRoot, string currentRunId)
    {
        var root = Path.Combine(repoRoot, "runs", "core-analysis");
        if (!Directory.Exists(root)) return new HashSet<string>(StringComparer.Ordinal);
        var letzter = Directory.EnumerateDirectories(root)
            .Where(d => !string.Equals(Path.GetFileName(d), currentRunId, StringComparison.Ordinal))
            .Select(d => Path.Combine(d, "analysis", "report.json"))
            .Where(File.Exists)
            .OrderByDescending(p => p, StringComparer.Ordinal)   // RunIds sind zeitlich sortierbar (yyyyMMdd_HHmmss_…)
            .FirstOrDefault();
        if (letzter is null) return new HashSet<string>(StringComparer.Ordinal);

        var report = JsonSerializer.Deserialize<AnalystReport>(File.ReadAllText(letzter), JsonFiles.Json)!;
        return report.InsDelta.Select(e => IdentityKey.From(e.Fund.Statement)).ToHashSet(StringComparer.Ordinal);
    }
}
