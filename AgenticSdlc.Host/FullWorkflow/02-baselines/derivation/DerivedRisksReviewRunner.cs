using System.Text.Json;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using AgenticSdlc.HumanReview;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// I-d-CLI: <c>derive-review &lt;derived-risks.json&gt; &lt;inference-check-report.json&gt;
///   &lt;requirements.artifact.json&gt; [--no-browser] [--dry-run]</c>.
/// Schließt den Derivation-Strang: der Mensch begutachtet die abgeleiteten Risiken (mit I-c-Verdikt + Anker-Kontext)
/// im generischen <c>AgenticSdlc.HumanReview</c>-UI und entscheidet approve/reject/needs_revision. Ergebnis:
/// <c>approved-derived-risks.json</c> (nur approved) + <c>review-decisions.json</c> (alle Entscheidungen, Audit).
/// Exit: 0 = fertig/dry-run, 1 = abgebrochen, 2 = Usage/IO.
/// </summary>
/// <remarks>
/// Reuse des generischen Review-UI (kein neues Frontend). Der interaktive Lauf blockiert bis „Fertig"/„Abbrechen"
/// (Mensch-Aktion, wie die Ledger-Adjudikations-UI) — daher hier nicht E2E-automattestbar; <c>--dry-run</c> baut die
/// Session deterministisch (kein Server) als Selbsttest.
/// </remarks>
public static class DerivedRisksReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, HostSettings settings, string repoRoot)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: derive-review <derived-risks.json> <inference-check-report.json> <requirements.artifact.json> [--no-browser] [--dry-run]");
            return 2;
        }

        var derivedPath = Resolve(repoRoot, args[1]);
        var reportPath = Resolve(repoRoot, args[2]);
        var basePath = Resolve(repoRoot, args[3]);
        if (derivedPath is null || !File.Exists(derivedPath)) { Console.Error.WriteLine($"[derive-review] derived-risks fehlt: {args[1]}"); return 2; }
        if (reportPath is null || !File.Exists(reportPath)) { Console.Error.WriteLine($"[derive-review] inference-check-report fehlt: {args[2]}"); return 2; }
        if (basePath is null || !File.Exists(basePath)) { Console.Error.WriteLine($"[derive-review] baseline fehlt: {args[3]}"); return 2; }

        var openBrowser = !args.Contains("--no-browser");
        var dryRun = args.Contains("--dry-run");

        var derived = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath).ConfigureAwait(false), Json);
        var report = JsonSerializer.Deserialize<InferenceCheckReport>(await File.ReadAllTextAsync(reportPath).ConfigureAwait(false), Json);
        var baseline = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(basePath).ConfigureAwait(false), Json);
        if (derived is null || derived.Items.Count == 0) { Console.Error.WriteLine("[derive-review] derived-risks leer."); return 2; }
        if (baseline is null || baseline.Items.Count == 0) { Console.Error.WriteLine("[derive-review] baseline leer."); return 2; }

        var verdictsById = (report?.Verdicts ?? []).ToDictionary(v => v.ItemId, v => v, StringComparer.Ordinal);
        var baselineById = baseline.Items.ToDictionary(i => i.ItemId, i => i, StringComparer.Ordinal);
        var derivedById = derived.Items.ToDictionary(i => i.ItemId, i => i, StringComparer.Ordinal);

        var session = DerivedRisksReviewAdapter.BuildSession(derived, verdictsById);

        var run = new RunContext(RunId.New(), "derivation");
        run.EnsureFolders();
        run.WriteConfig(new
        {
            workflow = "DerivedRisksReview", runId = run.RunId,
            derived = Path.GetRelativePath(repoRoot, derivedPath),
            report = Path.GetRelativePath(repoRoot, reportPath),
            baseline = Path.GetRelativePath(repoRoot, basePath),
            items = session.Items.Count, timestampUtc = DateTime.UtcNow
        });
        Console.WriteLine($"[derive-review] runId={run.RunId} items={session.Items.Count}");

        if (dryRun)
        {
            Console.WriteLine("[derive-review] --dry-run: Session gebaut (kein Server).");
            Console.WriteLine($"  Titel: {session.Title}");
            Console.WriteLine($"  Felder: {string.Join(", ", session.FieldSchema.Select(f => f.FieldKey))}");
            foreach (var it in session.Items.Take(5))
                Console.WriteLine($"    {it.ItemId} [{it.Badge}] anker={it.ContextBlocks.Count} vorschlag={it.FieldValues.FirstOrDefault()?.Value}  {Trunc(it.Summary)}");
            Console.WriteLine($"[derive-review] run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
            return 0;
        }

        var options = new ReviewServerOptions
        {
            Session = session,
            OpenBrowser = openBrowser,
            ResolveContext = (_, resolverKey) => Task.FromResult(DerivedRisksReviewAdapter.ResolveContext(resolverKey, baselineById)),
            RecomputeResolved = DerivedRisksReviewAdapter.Resolved,
            IsComplete = s => s.Items.All(DerivedRisksReviewAdapter.Resolved),
            OnItemSaved = _ => Task.CompletedTask
        };

        Console.WriteLine("[derive-review] Review-UI startet (blockiert bis Fertig/Abbrechen)...");
        var result = await LocalReviewServerHost.RunAsync(options).ConfigureAwait(false);

        var (approved, decisions) = DerivedRisksReviewAdapter.Apply(result.Session, derivedById);
        var approvedDoc = derived with { Items = approved };

        await File.WriteAllTextAsync(Path.Combine(run.RunDir, "approved-derived-risks.json"),
            JsonSerializer.Serialize(approvedDoc, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(run.RunDir, "review-decisions.json"),
            JsonSerializer.Serialize(new { outcome = result.Outcome.ToString(), decisions }, Json)).ConfigureAwait(false);

        run.AppendEvent(new
        {
            type = "DERIVED_RISKS_REVIEWED", runId = run.RunId, outcome = result.Outcome.ToString(),
            total = derived.Items.Count, approved = approved.Count, timestampUtc = DateTime.UtcNow
        });

        Console.WriteLine($"[derive-review] outcome={result.Outcome}  approved={approved.Count}/{derived.Items.Count}");
        Console.WriteLine($"[derive-review] -> approved-derived-risks.json + review-decisions.json  run -> {Path.GetRelativePath(repoRoot, run.RunDir)}");
        return result.Outcome == ReviewOutcome.Finished ? 0 : 1;
    }

    private static string Trunc(string s) => s.Length <= 80 ? s : s[..80] + "…";
    private static string? Resolve(string repoRoot, string? p)
        => string.IsNullOrWhiteSpace(p) ? null : (Path.IsPathRooted(p) ? p : Path.Combine(repoRoot, p));
}
