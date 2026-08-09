using AgenticSdlc.Host.FullWorkflow.Pipeline;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// R-14 G1-b (= E0.8): die Review-UI des operativen decision-gate. Liest die vom DecisionScan persistierte
// Anfrage (decision-gate-request.json), laesst den Menschen EDITIEREN (Ausgang + neuer Text + Begruendung;
// vertagen erlaubt) und schreibt decision-gate-decisions.json — die liest der pipeline-full-Resume und
// beantwortet damit den Port. CLI: decision-gate-review <fullworkflow-run|dir> [--no-browser]
public static class DecisionGateReviewRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    /// <summary>K13-Façade (3b): typisierter UI-Start für den Steward — pausierter Lauf per runId.</summary>
    public static Task<int> RunForRunAsync(string runId, Configuration.HostSettings settings, string repoRoot)
        => RunAsync(["decision-gate-review", runId], settings, repoRoot);

    public static async Task<int> RunAsync(string[] args, Configuration.HostSettings settings, string repoRoot)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: decision-gate-review <fullworkflow-run|07-decision-dir> [--no-browser]");
            return 2;
        }

        var dir = ResolveDir(repoRoot, args[1]);
        var requestPath = dir is null ? null : Path.Combine(dir, "decision-gate-request.json");
        if (requestPath is null || !File.Exists(requestPath))
        {
            Console.Error.WriteLine($"[decision-gate-review] decision-gate-request.json nicht gefunden fuer '{args[1]}' — pausiert der Lauf am decision-gate?");
            return 2;
        }

        var request = JsonSerializer.Deserialize<PipelineDecisionReviewRequest>(await File.ReadAllTextAsync(requestPath).ConfigureAwait(false), Json)
                      ?? throw new InvalidOperationException($"Anfrage nicht lesbar: {requestPath}");
        var decisionsPath = Path.Combine(dir!, "decision-gate-decisions.json");

        var session = PipelineDecisionReviewAdapter.BuildSession(request.RunId, request);
        if (session.Items.Count == 0)
        {
            Console.WriteLine("[decision-gate-review] keine offenen Entscheidungen.");
            return 0;
        }

        if (File.Exists(decisionsPath))
        {
            var existing = JsonSerializer.Deserialize<PipelineDecisionDecisionsFile>(await File.ReadAllTextAsync(decisionsPath).ConfigureAwait(false), Json);
            PipelineDecisionReviewAdapter.MergeExistingDecisions(session, existing);
        }

        // Kontext-Klicks (Ziel-REQ/PBIs ansehen): Core LIVE + read-only laden — der Mensch sieht den heutigen Stand.
        var coreRepo = new Core.JsonCoreRepository(repoRoot);
        var core = await coreRepo.ExistsAsync().ConfigureAwait(false) ? await coreRepo.LoadAsync().ConfigureAwait(false) : null;

        var (file, outcome) = await ReviewUiFlow.RunAsync(
            session, decisionsPath,
            PipelineDecisionReviewAdapter.Resolved,
            resolveContext: core is null ? null : (_, key) => Task.FromResult(PipelineDecisionReviewAdapter.RenderItemContext(key, core)),
            apply: s => PipelineDecisionReviewAdapter.Apply(request.RunId, s),
            openBrowser: !args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

        var resolved = file.Resolutions.Count(r => string.Equals(r.Action, DecisionStage.ActionResolve, StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"[decision-gate-review] {outcome}: {resolved} aufgeloest, {file.Resolutions.Count - resolved} vertagt -> {Path.GetRelativePath(repoRoot, decisionsPath)}");

        // 3b-② (Autor 09.08.): „Fertig" kettet die deterministische Fortsetzung — der resume beantwortet den
        // Port aus der eben geschriebenen Datei und faehrt die FOLGESTUFEN (inkl. LLM-Kosten, deshalb LAUT).
        // --no-resume = Inspektions-Opt-out; Cancelled ⇒ Pause bleibt.
        if (args.Contains("--no-resume", StringComparer.OrdinalIgnoreCase)
            || outcome != AgenticSdlc.HumanReview.ReviewOutcome.Finished)
        {
            Console.WriteLine($"[decision-gate-review] Weiter: pipeline-full resume {request.RunId}");
            return 0;
        }
        Console.WriteLine($"[decision-gate-review] R-43: resume {request.RunId} laeuft automatisch an (Folgestufen inkl. LLM) …");
        return await Pipeline.PipelineFullRunner.RunAsync(["pipeline-full", "resume", request.RunId], settings, repoRoot, null).ConfigureAwait(false);
    }

    private static string? ResolveDir(string repoRoot, string token)
    {
        var direct = Path.IsPathRooted(token) ? token : Path.Combine(repoRoot, token);
        if (File.Exists(Path.Combine(direct, "decision-gate-request.json"))) return direct;
        foreach (var root in new[] { Path.Combine(repoRoot, "runs", "fullworkflow"), Path.Combine(repoRoot, "runs", "pipeline") })
        {
            if (!Directory.Exists(root)) continue;
            var hit = Directory.EnumerateDirectories(root)
                .Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase))
                .Select(d => Path.Combine(d, "07-decision"))
                .FirstOrDefault(d => File.Exists(Path.Combine(d, "decision-gate-request.json")));
            if (hit is not null) return hit;
        }
        return null;
    }
}
