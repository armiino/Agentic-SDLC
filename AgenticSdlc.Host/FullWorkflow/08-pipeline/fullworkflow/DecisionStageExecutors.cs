using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Decision;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// R-14 G1-a (r14-entscheidung.md §10): Tor 2 als operativer Graph-Schritt ZWISCHEN Ingest-Apply und Pbi-Bridge.
// Die Aufloesung ENTSTEHT am Gate (edit-faehiges E0.8-UI, G1-b) — hier laufen nur die bestehenden BAUSTEINE:
// DecisionResolutionDerivation -> DecisionResolutionGate -> DecisionApplyExec (Idempotenz, Save -> Kangal I6).
// KEIN LLM im Pfad (D2). Der Scan sammelt ALLE offenen DECs (auch geparkte frueherer Laeufe = D4-Default).
// Nach der Aufloesung traegt sich die Kette selbst: PbiUpdateDerivation liest contradicts FRISCH (Flip => kein
// BLOCK mehr); synthetische Applied-Ops (REFINE/ADOPT) geben dem Align-Agenten die Ziele NOCH IM SELBEN LAUF.

/// <summary>decision-gate-Anfrage: eine offene Entscheidung je Item, mit vollem Kontext fuer den Menschen.
/// Origin = Klartext-Herkunft (woher der Widerspruch stammt; optional, alte Artefakte bleiben lesbar).</summary>
public sealed record PipelineDecisionItemView(
    string DecisionId, string DecisionText, string? TargetRequirementId, string TargetRequirementText,
    IReadOnlyList<string> BlockedPbis, string ProposedStatement, string? Origin = null,
    // C4-Kreislauf (22.08., additiv): Aspekt-Färbung der DEC — steuert NUR die dritte Auflöse-Option.
    string? Aspect = null);

public sealed record PipelineDecisionReviewRequest(string RunId, IReadOnlyList<PipelineDecisionItemView> Decisions);

/// <summary>Je DEC: action=resolve (mit outcome KEEP_ORIGINAL|ADOPT_NEW|REFINE) ODER action=defer (bleibt geparkt).</summary>
public sealed record PipelineDecisionResolution(string DecisionId, string Action, string? Outcome, string? NewStatement, string? Reason);

public sealed record PipelineDecisionReviewResponse(IReadOnlyList<PipelineDecisionResolution> Resolutions, string Reviewer);

// Die pure Logik des Schritts (testbar; Executors bleiben duenn).
public static class DecisionStage
{
    public const string ActionResolve = "resolve";
    public const string ActionDefer = "defer";

    // Alle OFFENEN Decisions als Review-Views (Ziel via contradicts-Relation, Fallback metadata; Prefill aus dem
    // DEC-Text, der das Meeting-Statement traegt: "Widerspruch zu REQ-x: <statement>").
    public static IReadOnlyList<PipelineDecisionItemView> BuildViews(ProjectStateDocument core)
    {
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        return core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase) && i.ReadStatus().IsOpenDecision)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(dec =>
            {
                var target = core.Relations
                    .FirstOrDefault(r => string.Equals(r.RelationType, DecisionRelations.Contradicts, StringComparison.Ordinal)
                                         && string.Equals(r.FromId, dec.ItemId, StringComparison.Ordinal))?.ToId
                    ?? dec.Metadata.GetValueOrDefault("targetEntityId");
                var targetText = target is not null && byId.TryGetValue(target, out var t) ? t.Text : "";
                var blocked = core.Items
                    .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                                && (i.Pbi?.OpenDecisionRefs.Contains(dec.ItemId) ?? false))
                    .Select(i => i.ItemId).OrderBy(x => x, StringComparer.Ordinal).ToList();
                return new PipelineDecisionItemView(dec.ItemId, dec.Text, target, targetText, blocked, ProposedStatementOf(dec.Text), OriginOf(dec),
                    Aspect: Core.DecisionAspectMeta.IsArchitecture(dec) ? Core.DecisionAspectMeta.Architecture : null);
            })
            .ToList();
    }

    // Klartext-Herkunft: WOHER stammt diese Entscheidung? Drei Zufluesse in den EINEN DEC-Topf:
    // Ingest-CONTRADICT (Widerspruch), D2-Knopf (Klaerungs-Antrag), 9g Meeting-Frage (zielloses DEC).
    // Kanal-Ehrlichkeit (Autor-Fund 20./21.08., 1g-Abnahme): CONTRADICT-DECs kommen NICHT nur aus
    // Meetings — Analyst-erschlossene und GitHub-geerntete Widersprueche muessen ihren echten Kanal
    // nennen (die Wahrheit traegt ihn laengst in der Metadata; nur der Wortlaut log).
    internal static string OriginOf(ProjectStateItem dec)
    {
        var kanal = dec.Metadata.ContainsKey(AnalystOriginMeta.Linse)
            ? "vom Core-Analysten ERSCHLOSSEN (nicht gesagt)"
            : GithubOriginMeta.IssueNumberOf(dec) is not null
                ? "aus der GitHub-Ernte"
                : "im Meeting";
        var origin = dec.Origin switch
        {
            "INGESTION_CONTRADICTION" => $"Widerspruch — {kanal}, am Ingest-Gate von dir als echter Konflikt bestätigt",
            Decision.DecisionRequestMint.Origin => "Klärungs-Antrag — am pbi-Gate von dir als Stakeholder-Frage beantragt",
            Decision.MeetingQuestionMint.Origin => "Offene Frage — im Meeting gestellt, am Ingest-Gate von dir aufgenommen",
            Decision.MeetingQuestionMint.OriginAuthor => "Offene Frage — von dir diktiert (Autor-Front), am Ingest-Gate aufgenommen",
            Decision.MeetingQuestionMint.OriginGithub => "Offene Frage — aus der GitHub-Ernte, am Ingest-Gate aufgenommen",
            Decision.MeetingQuestionMint.OriginAnalyst => "Offene Frage — vom Core-Analysten ERSCHLOSSEN (nicht gesagt), am Ingest-Gate von dir aufgenommen",
            // Slice S ④: Risiken-Rampe — das Risiko IST die Entscheidung (akzeptieren / mitigieren→ADOPT / klären).
            Decision.MeetingQuestionMint.OriginRisk => "RISIKO — im Meeting benannt, am Ingest-Gate von dir aufgenommen: akzeptieren, mitigieren (Übernahme als Anforderung) oder klären",
            Decision.MeetingQuestionMint.OriginRiskAuthor => "RISIKO — von dir diktiert (Autor-Front): akzeptieren, mitigieren oder klären",
            Decision.MeetingQuestionMint.OriginRiskGithub => "RISIKO — aus der GitHub-Ernte: akzeptieren, mitigieren oder klären",
            _ => dec.Origin,
        };
        var itemLabel = dec.Metadata.ContainsKey(AnalystOriginMeta.Linse) ? "Analyst-Fund"
            : GithubOriginMeta.IssueNumberOf(dec) is not null ? "GitHub-Item" : "Meeting-Item";
        var run = dec.SourceRunId is { Length: > 0 } r ? $" · Lauf {r}" : "";
        var incoming = dec.Metadata.GetValueOrDefault("ingestedFrom") is { Length: > 0 } inc ? $" · {itemLabel} {inc}" : "";
        return $"{origin}{run}{incoming}";
    }

    // Governance-Politik (Autor + Kollege, 04.08.): accept-all/replay koennen keine Wahrheits-Konflikte entscheiden
    // (Outcome/neuer Text waeren ERFUNDEN) -> Experiment-Modi vertagen ALLE Entscheidungen. Sichtbar geparkt,
    // der Block haelt (belegt §9); Replay-Konstanz bleibt.
    public static PipelineDecisionReviewResponse DeferAll(PipelineDecisionReviewRequest request, string reviewer)
        => new(request.Decisions.Select(d => new PipelineDecisionResolution(d.DecisionId, ActionDefer, null, null, null)).ToList(), reviewer);

    public static DecisionResolutionInput ToInput(IReadOnlyList<PipelineDecisionResolution> resolutions)
        => new(resolutions
            .Where(r => string.Equals(r.Action, ActionResolve, StringComparison.OrdinalIgnoreCase))
            .Select(r => new DecisionResolutionRequest(r.DecisionId, r.Outcome ?? "", r.NewStatement, r.Reason))
            .ToList());

    // Report-Merge (Semantik 04.08., am Code korrigiert): KEEP => kein Delta (Unblock hat der Apply schon getan) ·
    // REFINE => synthetisches Delta Kind=REFINE aufs ZIEL-Requirement (=> Placement leitet MARK_CHANGED ab) ·
    // ADOPT_NEW => ebenfalls Kind=REFINE, aber aufs NEUE Requirement: den Coverage-SWAP hat der Tor-2-Apply
    // bereits selbst erledigt (RequirementSwap) — ein SUPERSEDE-Delta faende nichts mehr; was bleibt, ist die
    // INHALTS-Angleichung der (schon geswappten) PBIs => MARK_CHANGED gegen das neue Requirement (Align-Ziel).
    public static IngestionApplyReport MergeReport(
        IngestionApplyReport original, DecisionResolutionPlanDocument plan, DecisionResolutionApplyReport applied)
    {
        // superseded[i] <-> newRequirements[i] werden im Apply in derselben Iteration angehaengt (index-gepaart).
        var newReqOfTarget = applied.SupersededRequirements
            .Zip(applied.NewRequirements, (oldReq, newReq) => (oldReq, newReq))
            .ToDictionary(x => x.oldReq, x => x.newReq, StringComparer.Ordinal);

        var synthetic = new List<AppliedOperation>();
        foreach (var op in plan.Operations)
        {
            if (op.TargetRequirementId is null) continue;   // 9g: zielloses Frage-DEC hat nie synthetische Deltas
            if (string.Equals(op.Outcome, DecisionOutcome.Refine, StringComparison.Ordinal)
                && applied.RefinedRequirements.Contains(op.TargetRequirementId))
                synthetic.Add(new AppliedOperation(op.DecisionId, StateChangeKind.Refine, op.TargetRequirementId, "refined"));
            else if (string.Equals(op.Outcome, DecisionOutcome.AdoptNew, StringComparison.Ordinal)
                     && newReqOfTarget.TryGetValue(op.TargetRequirementId, out var newReq))
                synthetic.Add(new AppliedOperation(op.DecisionId, StateChangeKind.Refine, newReq, "adopted_new"));
            // KEEP_ORIGINAL: kein Delta noetig — Unblock ist erledigt, die Wahrheit blieb.
        }

        return synthetic.Count == 0
            ? original
            : original with { Applied = original.Applied.Concat(synthetic).ToList() };
    }

    public static string ProposedStatementOf(string decisionText)
    {
        var idx = decisionText.IndexOf(": ", StringComparison.Ordinal);
        return idx > 0 && idx + 2 < decisionText.Length ? decisionText[(idx + 2)..] : decisionText;
    }
}

// SCAN: offene Decisions? 0 -> Report unveraendert weiter (Typ-Routing, Muster BranchDetector). >0 -> Request an
// den decision-gate-Port (auch geparkte DECs frueherer Laeufe erscheinen — D4-Default).
[SendsMessage(typeof(PipelineDecisionReviewRequest))]
[SendsMessage(typeof(IngestionApplyReport))]
internal sealed class DecisionScanExecutor(RunContext run, string repoRoot, string outDir) : Executor<IngestionApplyReport>("PipelineDecisionScan")
{
    private static readonly JsonSerializerOptions ScanJson = JsonFiles.Json;

    public override async ValueTask HandleAsync(IngestionApplyReport report, IWorkflowContext context, CancellationToken ct = default)
    {
        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        var views = DecisionStage.BuildViews(core);
        run.AppendEvent(new { type = "PIPELINE_DECISION_SCAN", runId = run.RunId, open = views.Count, timestampUtc = DateTime.UtcNow });
        if (views.Count == 0)
        {
            await context.SendMessageAsync(report).ConfigureAwait(false);
            return;
        }
        // G1-b: die Anfrage als Run-Artefakt persistieren — Datenquelle der Review-UI (decision-gate-review).
        var request = new PipelineDecisionReviewRequest(run.RunId, views);
        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "decision-gate-request.json"), JsonSerializer.Serialize(request, ScanJson), ct).ConfigureAwait(false);
        Console.WriteLine($"[pipeline-full] {views.Count} offene Entscheidung(en) im Core -> decision-gate (Tor 2).");
        await context.SendMessageAsync(request).ConfigureAwait(false);
    }
}

// APPLY (komponiert): baut aus den resolve-Antworten die Resolutions -> Derive -> Gate (mechanisch) -> gated Apply
// (DecisionApplyExec: Idempotenz-Marker, Save -> Kangal). Danach Original-Ingest-Report + synthetische Ops an die
// Pbi-Bridge. Nur-defer => Original-Report unveraendert (DECs parken sichtbar weiter).
[SendsMessage(typeof(IngestionApplyReport))]
[YieldsOutput(typeof(string))]
internal sealed class DecisionComposedApplyExecutor(RunContext run, string repoRoot, string ingestOutDir, string outDir)
    : Executor<PipelineDecisionReviewResponse>("PipelineDecisionApply")
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public override async ValueTask HandleAsync(PipelineDecisionReviewResponse resp, IWorkflowContext context, CancellationToken ct = default)
    {
        // R-40 (Endform): der EINE Lauf-Report-Vertrag (req-Apply initial, arch-Apply fortgeschrieben) —
        // keine Vorrang-Magie. Fallback delta.json NUR fuer Alt-Laeufe vor R-40 (Resume-Kompatibilitaet).
        var runReportPath = Path.Combine(ingestOutDir, "applied", "run-report.json");
        var reportPath = File.Exists(runReportPath) ? runReportPath : Path.Combine(ingestOutDir, "applied", "delta.json");
        var original = JsonSerializer.Deserialize<IngestionApplyReport>(await File.ReadAllTextAsync(reportPath, ct).ConfigureAwait(false), Json)
                       ?? throw new InvalidOperationException($"Ingest-Report nicht lesbar: {reportPath}");

        var input = DecisionStage.ToInput(resp.Resolutions);
        var deferred = resp.Resolutions.Count - input.Resolutions.Count;
        if (input.Resolutions.Count == 0)
        {
            run.AppendEvent(new { type = "PIPELINE_DECISION_DEFERRED", runId = run.RunId, deferred, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
            Console.WriteLine($"[pipeline-full] decision-gate: alle {deferred} Entscheidung(en) vertagt — DECs parken sichtbar weiter.");
            await context.SendMessageAsync(original).ConfigureAwait(false);
            return;
        }

        var core = await new JsonCoreRepository(repoRoot).LoadAsync(ct).ConfigureAwait(false);
        var (ops, problems) = DecisionResolutionDerivation.Derive(core, input);
        var plan = new DecisionResolutionPlanDocument(DecisionResolutionPlanDocument.CurrentSchemaVersion,
            $"decision-plan-{run.RunId}", DateTime.UtcNow, ops);
        var gate = DecisionResolutionGate.Check(core, plan);

        Directory.CreateDirectory(outDir);
        await File.WriteAllTextAsync(Path.Combine(outDir, "decision-plan.json"), JsonSerializer.Serialize(plan, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "decision-gate-report.json"), JsonSerializer.Serialize(gate, Json), ct).ConfigureAwait(false);

        // Mechanische Gate-Fehler hier = UI-/Input-Bug (der Mensch hat am Gate schon entschieden) -> lauter Stopp
        // (SP2-Muster), nichts wird appliziert.
        if (!gate.Pass || problems.Count > 0)
        {
            run.AppendEvent(new { type = "PIPELINE_DECISION_GATE_FAILED", runId = run.RunId, errors = gate.Errors.Count, problems, timestampUtc = DateTime.UtcNow });
            await context.YieldOutputAsync(
                $"07-decision: Aufloesungs-Plan mechanisch ungueltig (errors={gate.Errors.Count}, problems={problems.Count}) — nichts appliziert; siehe {Path.Combine(outDir, "decision-gate-report.json")}").ConfigureAwait(false);
            return;
        }

        var accepted = Enumerable.Range(0, plan.Operations.Count).ToHashSet();
        run.AppendEvent(new { type = "PIPELINE_DECISION_APPLY_START", runId = run.RunId, resolutions = input.Resolutions.Count, deferred, reviewer = resp.Reviewer, timestampUtc = DateTime.UtcNow });
        var applied = await DecisionApplyExec.ExecuteAsync(outDir, plan, accepted, repoRoot, run.RunId, ct).ConfigureAwait(false);

        var merged = DecisionStage.MergeReport(original, plan, applied);
        run.AppendEvent(new
        {
            type = "PIPELINE_DECISION_APPLIED",
            runId = run.RunId,
            resolved = applied.Resolved.Count,
            unblocked = applied.UnblockedPbis.Count,
            newRequirements = applied.NewRequirements.Count,
            syntheticOps = merged.Applied.Count - original.Applied.Count,
            deferred,
            timestampUtc = DateTime.UtcNow
        });
        await context.SendMessageAsync(merged).ConfigureAwait(false);
    }
}
