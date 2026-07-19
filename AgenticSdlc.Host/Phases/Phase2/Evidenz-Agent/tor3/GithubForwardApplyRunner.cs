using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3;

// CLI: github-forward-apply <github-forward-run> [--execute] [--repo owner/name] [--token-env NAME]
//
// T3.4 — der EINZIGE Punkt mit echtem GitHub-Write. Safe-by-default: OHNE --execute nur Vorschau (kein Write,
// kein Core-Change). Mit --execute werden NUR die vom Menschen akzeptierten Ops ausgefuehrt (human-decisions.json
// Pflicht) und das Mapping ueber T3.1 (implemented_by_issue) in den Core zurueckgeschrieben (schliesst die Dedup-
// Schleife). Rev-3 wird HIER, am irreversiblen Rand, nochmals erzwungen: CREATE ohne searchedQueries+searchEvidence
// wird abgelehnt (nicht geschrieben) — unabhaengig vom frueheren Gate.
public static class GithubForwardApplyRunner
{
    private const string UserAgent = "Agentic-SDLC";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2) { Usage(); return 2; }
        var execute = args.Contains("--execute", StringComparer.OrdinalIgnoreCase);
        string? token = null, repoArg = null, tokenEnv = null;
        for (var i = 1; i < args.Length; i++)
        {
            var a = args[i];
            if (string.Equals(a, "--execute", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(a, "--repo", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { repoArg = args[++i]; continue; }
            if (string.Equals(a, "--token-env", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) { tokenEnv = args[++i]; continue; }
            if (a.StartsWith("--", StringComparison.Ordinal)) { Console.Error.WriteLine($"[github-forward-apply] unbekanntes Argument: {a}"); return 2; }
            token ??= a;
        }
        if (token is null) { Usage(); return 2; }

        var planDir = GithubForwardReviewRunner.ResolvePlanDir(repoRoot, token);
        if (planDir is null) { Console.Error.WriteLine($"[github-forward-apply] Lauf '{token}' nicht gefunden."); return 2; }
        var planPath = Path.Combine(planDir, "github-forward-plan.json");
        var decisionsPath = Path.Combine(planDir, "human-decisions.json");
        if (!File.Exists(planPath)) { Console.Error.WriteLine("[github-forward-apply] github-forward-plan.json fehlt."); return 2; }
        if (!File.Exists(decisionsPath)) { Console.Error.WriteLine("[github-forward-apply] human-decisions.json fehlt - erst github-forward-review fahren."); return 2; }

        var plan = await LoadAsync<GithubForwardPlanDocument>(planPath).ConfigureAwait(false);
        var decisions = await LoadAsync<GithubForwardDecisionsFile>(decisionsPath).ConfigureAwait(false);
        var accepted = decisions.Decisions
            .Where(d => string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
            .Select(d => d.OpId).ToHashSet(StringComparer.Ordinal);
        var repository = repoArg ?? plan.Repository;

        // Braucht ueberhaupt ein echter Write stattfinden?
        var writingKinds = new HashSet<string>(StringComparer.Ordinal)
            { GithubForwardKind.CreateIssue, GithubForwardKind.UpdateIssue, GithubForwardKind.Comment };
        var hasWrite = plan.Operations.Where((_, i) => accepted.Contains($"op-{i}")).Any(o => writingKinds.Contains(o.Kind));

        GithubRestIssueClient? client = null;
        HttpClient? http = null;
        if (execute && hasWrite)
        {
            if (string.IsNullOrWhiteSpace(repository)) { Console.Error.WriteLine("[github-forward-apply] --repo owner/name fehlt (fuer echte Writes)."); return 2; }
            var tok = ResolveToken(tokenEnv);
            if (string.IsNullOrWhiteSpace(tok)) { Console.Error.WriteLine($"[github-forward-apply] Token fehlt (Env {tokenEnv ?? "GITHUB_TEST_TOKEN/githubtoken"}). Wird nicht persistiert."); return 2; }
            http = new HttpClient();
            client = new GithubRestIssueClient(http, tok, UserAgent);
        }

        var started = DateTime.UtcNow;
        var reportOps = new List<GithubForwardApplyOp>();
        var mappingOps = new List<GithubMappingOp>();
        int created = 0, updated = 0, commented = 0, linked = 0, flagged = 0, held = 0, noChange = 0, skipped = 0, rejected = 0, failed = 0;

        for (var i = 0; i < plan.Operations.Count; i++)
        {
            var op = plan.Operations[i];
            var opId = $"op-{i}";
            if (!accepted.Contains(opId))
            {
                reportOps.Add(Op(opId, op, null, null, "skipped", "nicht akzeptiert (skip/keine Entscheidung)"));
                skipped++;
                continue;
            }

            try
            {
                switch (op.Kind)
                {
                    case GithubForwardKind.CreateIssue:
                    {
                        // Rev-3 am irreversiblen Rand: ohne ausgefuehrte Suche kein CREATE.
                        if (op.SearchedQueries is null || op.SearchedQueries.Count == 0 || string.IsNullOrWhiteSpace(op.SearchEvidence))
                        { reportOps.Add(Op(opId, op, null, null, "rejected", "CREATE_WITHOUT_SEARCH_EVIDENCE")); rejected++; break; }
                        if (string.IsNullOrWhiteSpace(op.Title) || string.IsNullOrWhiteSpace(op.Body))
                        { reportOps.Add(Op(opId, op, null, null, "rejected", "CREATE_INCOMPLETE (Titel/Body fehlt)")); rejected++; break; }

                        if (!execute) { reportOps.Add(Op(opId, op, null, null, "would-create", null)); created++; break; }
                        var r = await client!.CreateIssueAsync(repository!, op.Title!, op.Body!, op.Labels ?? [], CancellationToken.None).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, r.IssueNumber, r.IssueUrl, "created", null));
                        mappingOps.Add(new GithubMappingOp(op.PbiId, r.IssueNumber, r.IssueUrl, repository, GithubMappingKind.Link, "CREATE"));
                        created++;
                        break;
                    }
                    case GithubForwardKind.UpdateIssue:
                    {
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "UPDATE ohne targetIssueNumber")); failed++; break; }
                        if (!execute) { reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "would-update", null)); updated++; break; }
                        var r = await client!.UpdateIssueAsync(repository!, op.TargetIssueNumber.Value, op.Title ?? "", op.Body ?? "", op.Labels ?? [], CancellationToken.None).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, r.IssueNumber, r.IssueUrl, "updated", null));
                        mappingOps.Add(new GithubMappingOp(op.PbiId, op.TargetIssueNumber.Value, r.IssueUrl, repository, GithubMappingKind.Link, "UPDATE"));
                        updated++;
                        break;
                    }
                    case GithubForwardKind.Comment:
                    {
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "COMMENT ohne targetIssueNumber")); failed++; break; }
                        if (!execute) { reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "would-comment", null)); commented++; break; }
                        var r = await client!.CreateCommentAsync(repository!, op.TargetIssueNumber.Value, op.Body ?? op.Rationale, CancellationToken.None).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, r.IssueUrl, "commented", null));
                        mappingOps.Add(new GithubMappingOp(op.PbiId, op.TargetIssueNumber.Value, null, repository, GithubMappingKind.Link, "COMMENT"));
                        commented++;
                        break;
                    }
                    case GithubForwardKind.Link:
                    {
                        // Kein GitHub-Write — nur das (menschlich bestaetigte) Mapping in den Core.
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "LINK ohne targetIssueNumber")); failed++; break; }
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, execute ? "linked" : "would-link", null));
                        if (execute) mappingOps.Add(new GithubMappingOp(op.PbiId, op.TargetIssueNumber.Value, null, repository, GithubMappingKind.Link, "LINK"));
                        linked++;
                        break;
                    }
                    case GithubForwardKind.FlagDrift:
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "flagged", op.Rationale)); flagged++; break;
                    case GithubForwardKind.HoldBlocked:
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "held", op.Rationale)); held++; break;
                    case GithubForwardKind.NoChange:
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "noChange", null)); noChange++; break;
                    default:
                        reportOps.Add(Op(opId, op, null, null, "skipped", $"unbekannte Operation {op.Kind}")); skipped++; break;
                }
            }
            catch (Exception ex)
            {
                reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "failed", ex.Message));
                failed++;
            }
        }

        http?.Dispose();

        var appliedDir = Path.Combine(planDir, "applied");
        Directory.CreateDirectory(appliedDir);

        // Mapping in den Core zurueckschreiben — nur bei echter Ausfuehrung, ueber den Port + Audit-Snapshot.
        if (execute && mappingOps.Count > 0)
        {
            var coreRepo = new JsonCoreRepository(repoRoot);
            if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[github-forward-apply] Core fehlt - Mapping nicht persistiert."); }
            else
            {
                var core = await coreRepo.LoadAsync().ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json)).ConfigureAwait(false);
                var (updatedCore, mapReport) = CoreGithubMapping.Apply(core, mappingOps);
                await coreRepo.SaveAsync(updatedCore).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(appliedDir, "mapping-apply-report.json"), JsonSerializer.Serialize(mapReport, Json)).ConfigureAwait(false);
            }
        }

        var success = failed == 0 && rejected == 0;
        var report = new GithubForwardApplyReport(
            DryRun: !execute, Executed: execute, Success: success, Repository: repository, SourcePlanId: plan.PlanId,
            StartedUtc: started, CompletedUtc: DateTime.UtcNow, Operations: reportOps,
            Summary: new GithubForwardApplySummary(accepted.Count, created, updated, commented, linked, flagged, held, noChange, skipped, rejected, failed));
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-forward-apply-report.json"), JsonSerializer.Serialize(report, Json)).ConfigureAwait(false);

        Console.WriteLine(execute
            ? $"[github-forward-apply] EXECUTED repo={repository} accepted={accepted.Count} created={created} updated={updated} commented={commented} linked={linked} rejected={rejected} failed={failed}"
            : $"[github-forward-apply] DRY-RUN (kein GitHub-Write, kein Core-Change) accepted={accepted.Count} wouldCreate={created} wouldUpdate={updated} wouldComment={commented} wouldLink={linked} rejected={rejected}");
        Console.WriteLine($"[github-forward-apply] flagged={flagged} held={held} noChange={noChange} skipped={skipped} mappings={mappingOps.Count}");
        foreach (var o in reportOps.Where(x => x.Status is "rejected" or "failed")) Console.WriteLine($"[github-forward-apply]   {o.Status.ToUpperInvariant()} {o.OpId} {o.PbiId}: {o.Message}");
        if (!execute) Console.WriteLine("[github-forward-apply] -> --execute --repo owner/name zum Ausfuehren (Token via Env).");
        Console.WriteLine($"[github-forward-apply] -> {Path.GetRelativePath(repoRoot, appliedDir)}");
        return success ? 0 : 1;
    }

    private static GithubForwardApplyOp Op(string opId, GithubForwardOp op, int? resultNumber, string? resultUrl, string status, string? message)
        => new(opId, op.Kind, op.PbiId, op.TargetIssueNumber, resultNumber, resultUrl, status, message);

    // Mirror der Snapshot-Runner-Logik: expliziter Env-Name, sonst GITHUB_TEST_TOKEN mit Fallback githubtoken.
    private static string? ResolveToken(string? tokenEnv)
    {
        if (!string.IsNullOrWhiteSpace(tokenEnv)) return Environment.GetEnvironmentVariable(tokenEnv);
        return Environment.GetEnvironmentVariable("GITHUB_TEST_TOKEN") ?? Environment.GetEnvironmentVariable("githubtoken");
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json) ?? throw new InvalidOperationException($"Datei nicht lesbar: {path}");
    }

    private static void Usage()
        => Console.Error.WriteLine("Usage: github-forward-apply <github-forward-run|dir> [--execute] [--repo owner/name] [--token-env NAME]");
}
