using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

// GETEILTE Apply-Ausfuehrung (S1, Worklist 20.07): DER EINZIGE Punkt mit echtem GitHub-Write + Core-Mapping.
// Wird von ZWEI Aufrufern genutzt, damit es KEIN Duplikat gibt und A/B-Paritaet garantiert ist:
//   (1) GithubForwardApplyRunner   — CLI-Pfad (Entscheidungen aus human-decisions.json)
//   (2) GithubForwardApplyExecutor — MAF-HITL-Pfad (Entscheidungen aus der RequestPort-Response)
// Safe-by-default: OHNE execute nur Vorschau (kein Write, kein Core-Change). Rev-3 wird HIER, am irreversiblen
// Rand, nochmals erzwungen: CREATE ohne searchedQueries+searchEvidence wird abgelehnt (unabhaengig vom Gate).
public static class GithubForwardApply
{
    private const string UserAgent = "Agentic-SDLC";
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    // Fuehrt den Plan gegen die akzeptierten OpIds aus, schreibt applied/ + (bei execute) das Core-Mapping und
    // gibt den Report zurueck. planDir = das Verzeichnis mit github-forward-plan.json (dort entsteht applied/).
    public static async Task<GithubForwardApplyReport> ExecuteAsync(
        string planDir, GithubForwardPlanDocument plan, ISet<string> accepted,
        bool execute, string repoRoot, string? repository, string? tokenEnv, CancellationToken ct = default)
    {
        // Braucht ueberhaupt ein echter Write stattfinden?
        var writingKinds = new HashSet<string>(StringComparer.Ordinal)
            { GithubForwardKind.CreateIssue, GithubForwardKind.UpdateIssue, GithubForwardKind.Comment };
        var hasWrite = plan.Operations.Where((_, i) => accepted.Contains($"op-{i}")).Any(o => writingKinds.Contains(o.Kind));

        GithubRestIssueClient? client = null;
        HttpClient? http = null;
        if (execute && hasWrite)
        {
            if (string.IsNullOrWhiteSpace(repository)) throw new InvalidOperationException("--repo owner/name fehlt (fuer echte Writes).");
            var tok = ResolveToken(tokenEnv);
            if (string.IsNullOrWhiteSpace(tok)) throw new InvalidOperationException($"Token fehlt (Env {tokenEnv ?? "GITHUB_TEST_TOKEN/githubtoken"}).");
            http = new HttpClient();
            client = new GithubRestIssueClient(http, tok, UserAgent);
        }

        // S3 IDEMPOTENZ: aktuellen Core EINMAL vorab laden (nur bei execute). Der Core ist der kanonische "schon
        // angewendet?"-Beleg — ist ein PBI bereits gemappt, wird der (irreversible) GitHub-Write NICHT wiederholt.
        // CoreGithubMapping.Apply selbst ist fuer LINK bereits idempotent; die Luecke sind CREATE_ISSUE/COMMENT
        // (append-artige Writes). Derselbe Core wird am Ende fuer den Mapping-Write wiederverwendet (kein Re-Read).
        JsonCoreRepository? coreRepo = null;
        ProjectStateDocument? core = null;
        IReadOnlyDictionary<string, GithubMappingRecord> mappingByPbi = new Dictionary<string, GithubMappingRecord>(StringComparer.Ordinal);
        if (execute)
        {
            coreRepo = new JsonCoreRepository(repoRoot);
            if (await coreRepo.ExistsAsync().ConfigureAwait(false))
            {
                core = await coreRepo.LoadAsync().ConfigureAwait(false);
                mappingByPbi = CoreGithubMapping.ByPbi(core);
            }
        }

        // "Schon angewendet?" — CREATE: jedes bestehende Mapping zaehlt (Issue existiert bereits).
        // LINK: idempotent, wenn bereits auf DASSELBE Issue gemappt (ein anderes Ziel = echte Aenderung).
        // R-21 (E11-Fund 2026-07-23): UPDATE/COMMENT sind hier AUSGENOMMEN — bei ihnen ist das Mapping auf
        // dasselbe Issue die VORAUSSETZUNG des Ops, nicht der Beweis seiner Anwendung; der alte Kurzschluss
        // machte den gesamten Update-Pfad zu totem Code (EXECUTED … alreadyApplied=2, updated=0).
        bool AlreadyApplied(GithubForwardOp o)
        {
            if (!execute || !mappingByPbi.TryGetValue(o.PbiId, out var m)) return false;
            if (string.Equals(o.Kind, GithubForwardKind.CreateIssue, StringComparison.Ordinal)) return true;
            if (string.Equals(o.Kind, GithubForwardKind.UpdateIssue, StringComparison.Ordinal)
                || string.Equals(o.Kind, GithubForwardKind.Comment, StringComparison.Ordinal)) return false;
            return o.TargetIssueNumber is not null && m.IssueNumber == o.TargetIssueNumber.Value;
        }

        var started = DateTime.UtcNow;
        var reportOps = new List<GithubForwardApplyOp>();
        var mappingOps = new List<GithubMappingOp>();
        int created = 0, updated = 0, commented = 0, linked = 0, flagged = 0, held = 0, noChange = 0, skipped = 0, rejected = 0, failed = 0, alreadyApplied = 0;

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

            // S3: idempotenter Kurzschluss VOR jedem Write/Mapping — Re-Run/Resume-twice erzeugt kein Duplikat.
            if (AlreadyApplied(op))
            {
                reportOps.Add(Op(opId, op, mappingByPbi[op.PbiId].IssueNumber, null, "already-applied", "idempotent: PBI bereits im Core gemappt — kein erneuter Write."));
                alreadyApplied++;
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
                        var r = await client!.CreateIssueAsync(repository!, op.Title!, op.Body!, op.Labels ?? [], ct).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, r.IssueNumber, r.IssueUrl, "created", null));
                        // C2a-2: Schreib-Stempel = Hash des GESENDETEN Titels/Bodys (Drift-Anker, §7 c2-inbound-plan).
                        mappingOps.Add(new GithubMappingOp(op.PbiId, r.IssueNumber, r.IssueUrl, repository, GithubMappingKind.Link, "CREATE",
                            ProjectedTitleHash: GithubProjectionHash.Compute(op.Title), ProjectedBodyHash: GithubProjectionHash.Compute(op.Body)));
                        created++;
                        break;
                    }
                    case GithubForwardKind.UpdateIssue:
                    {
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "UPDATE ohne targetIssueNumber")); failed++; break; }
                        if (!execute) { reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "would-update", null)); updated++; break; }
                        // R-30: op.Labels null ⇒ Labels nicht anfassen (?? [] haette sie auf GitHub GELEERT).
                        var r = await client!.UpdateIssueAsync(repository!, op.TargetIssueNumber.Value, op.Title ?? "", op.Body ?? "", op.Labels, ct).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, r.IssueNumber, r.IssueUrl, "updated", null));
                        // C2a-2: Schreib-Stempel exakt über dem, was gesendet wurde (op.Title/Body wie im Call).
                        mappingOps.Add(new GithubMappingOp(op.PbiId, op.TargetIssueNumber.Value, r.IssueUrl, repository, GithubMappingKind.Link, "UPDATE",
                            ProjectedTitleHash: GithubProjectionHash.Compute(op.Title ?? ""), ProjectedBodyHash: GithubProjectionHash.Compute(op.Body ?? "")));
                        updated++;
                        break;
                    }
                    case GithubForwardKind.Comment:
                    {
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "COMMENT ohne targetIssueNumber")); failed++; break; }
                        if (!execute) { reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "would-comment", null)); commented++; break; }
                        var r = await client!.CreateCommentAsync(repository!, op.TargetIssueNumber.Value, op.Body ?? op.Rationale, ct).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, r.IssueUrl, "commented", null));
                        mappingOps.Add(new GithubMappingOp(op.PbiId, op.TargetIssueNumber.Value, null, repository, GithubMappingKind.Link, "COMMENT"));
                        commented++;
                        break;
                    }
                    case GithubForwardKind.NoteComment:
                    {
                        // C2d §3-5: Abschluss-Vermerk am Ursprungs-Issue — NUR der Kommentar, BEWUSST kein
                        // Mapping-Write (im Gegensatz zu COMMENT): der Betreff ist ein Wahrheits-Item, kein PBI.
                        if (op.TargetIssueNumber is null) { reportOps.Add(Op(opId, op, null, null, "failed", "NOTE_COMMENT ohne targetIssueNumber")); failed++; break; }
                        if (!execute) { reportOps.Add(Op(opId, op, op.TargetIssueNumber, null, "would-comment", null)); commented++; break; }
                        var note = await client!.CreateCommentAsync(repository!, op.TargetIssueNumber.Value, op.Body ?? op.Rationale, ct).ConfigureAwait(false);
                        reportOps.Add(Op(opId, op, op.TargetIssueNumber, note.IssueUrl, "commented", null));
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
                    case GithubForwardKind.HoldClarify:
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

        // Mapping in den Core zurueckschreiben — nur bei echter Ausfuehrung, ueber den vorab geladenen Core (S3:
        // derselbe Core, gegen den die Idempotenz geprueft wurde -> kein Re-Read, keine Race).
        if (execute && mappingOps.Count > 0)
        {
            if (core is null || coreRepo is null) { Console.Error.WriteLine("[github-forward-apply] Core fehlt - Mapping nicht persistiert."); }
            else
            {
                await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json), ct).ConfigureAwait(false);
                var (updatedCore, mapReport) = CoreGithubMapping.Apply(core, mappingOps);
                await coreRepo.SaveAsync(updatedCore).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(appliedDir, "mapping-apply-report.json"), JsonSerializer.Serialize(mapReport, Json), ct).ConfigureAwait(false);
            }
        }

        var success = failed == 0 && rejected == 0;
        var report = new GithubForwardApplyReport(
            DryRun: !execute, Executed: execute, Success: success, Repository: repository, SourcePlanId: plan.PlanId,
            StartedUtc: started, CompletedUtc: DateTime.UtcNow, Operations: reportOps,
            Summary: new GithubForwardApplySummary(accepted.Count, created, updated, commented, linked, flagged, held, noChange, skipped, rejected, failed, alreadyApplied));
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-forward-apply-report.json"), JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        return report;
    }

    private static GithubForwardApplyOp Op(string opId, GithubForwardOp op, int? resultNumber, string? resultUrl, string status, string? message)
        => new(opId, op.Kind, op.PbiId, op.TargetIssueNumber, resultNumber, resultUrl, status, message);

    // Mirror der Snapshot-Runner-Logik: expliziter Env-Name, sonst GITHUB_TEST_TOKEN mit Fallback githubtoken.
    private static string? ResolveToken(string? tokenEnv)
    {
        if (!string.IsNullOrWhiteSpace(tokenEnv)) return Environment.GetEnvironmentVariable(tokenEnv);
        return Environment.GetEnvironmentVariable("GITHUB_TEST_TOKEN") ?? Environment.GetEnvironmentVariable("githubtoken");
    }
}
