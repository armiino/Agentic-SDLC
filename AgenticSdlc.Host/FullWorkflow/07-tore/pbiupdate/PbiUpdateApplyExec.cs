using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// S4/S3-analog: Idempotenz-Marker. Haelt die planId, die zuletzt erfolgreich angewendet wurde. Ein zweiter Apply
// desselben Plans (Doppel-Resume / Re-Run) ist dann ein No-Op — kein zweiter Core-Write (kein NEW_PBI-Duplikat,
// keine Version/History-Drift). Plan-Ebene, weil pbi-update-apply pro Run einen unveraenderlichen Plan hat.
public sealed record PbiUpdateAppliedMarker(
    [property: JsonPropertyName("planId")] string PlanId,
    [property: JsonPropertyName("appliedUtc")] DateTime AppliedUtc);

// GETEILTE Apply-Ausfuehrung fuer pbi-update (S4, Worklist 20.07): der deterministische Punkt, der die akzeptierten
// PBI-Operationen in den Core schreibt und das github-sync-Delta erzeugt. Genutzt von ZWEI Aufrufern (Paritaet):
//   (1) PbiUpdateApplyRunner    — CLI (Entscheidungen aus human-decisions.json)
//   (2) PbiUpdateApplyExecutor  — MAF-HITL (Entscheidungen aus der RequestPort-Response)
// Core NUR ueber den Repository-Port + Audit-Snapshot (core-before.json).
public static class PbiUpdateApplyExec
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<PbiUpdateApplyReport> ExecuteAsync(
        string planDir, PbiStateChangePlanDocument plan, ISet<int> accepted, string repoRoot,
        IReadOnlyList<PbiAlignment>? acceptedAlignments = null,
        IReadOnlyList<PbiFeatureOverride>? featureOverrides = null,
        IReadOnlyList<Decision.PbiDecisionRequest>? decisionRequests = null, CancellationToken ct = default)
    {
        var appliedDir = Path.Combine(planDir, "applied");
        var markerPath = Path.Combine(appliedDir, "applied.marker");
        var reportPath = Path.Combine(appliedDir, "pbi-update-apply-report.json");

        // S3-analoge Idempotenz: dieser Plan wurde bereits angewendet -> bestehenden Report zurueckgeben, KEIN
        // zweiter Core-Write (verhindert NEW_PBI-Duplikate + Version/History-Drift bei Doppel-Resume/Re-Run).
        if (File.Exists(markerPath) && File.Exists(reportPath))
        {
            var prev = JsonSerializer.Deserialize<PbiUpdateAppliedMarker>(await File.ReadAllTextAsync(markerPath, ct).ConfigureAwait(false), Json);
            if (prev is not null && string.Equals(prev.PlanId, plan.PlanId, StringComparison.Ordinal))
            {
                Console.WriteLine($"[pbi-update-apply] bereits angewendet (idempotent): Plan {plan.PlanId} — kein erneuter Core-Write.");
                return JsonSerializer.Deserialize<PbiUpdateApplyReport>(await File.ReadAllTextAsync(reportPath, ct).ConfigureAwait(false), Json)!;
            }
        }

        var coreRepo = new JsonCoreRepository(repoRoot);
        if (!await coreRepo.ExistsAsync().ConfigureAwait(false)) throw new InvalidOperationException("Core fehlt.");
        var core = await coreRepo.LoadAsync().ConfigureAwait(false);

        // B1: vom Menschen korrigierte NEW_PBI-Feature-Zuordnungen VOR dem Apply in den Plan einweben. Nur auf ein
        // EXISTIERENDES Feature (defensiv: Dropdown liefert nur bestehende — ungültiges Ziel => Vorschlag behalten,
        // statt die Op fallen zu lassen). PlanId bleibt gleich => Idempotenz-Marker unberührt.
        plan = ApplyFeatureOverrides(plan, featureOverrides, core);

        Directory.CreateDirectory(appliedDir);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "core-before.json"), JsonSerializer.Serialize(core, Json), ct).ConfigureAwait(false);

        var (updated, report) = PbiUpdateApply.Apply(core, plan, accepted, plan.SourceIngestionRun, acceptedAlignments);

        // R-14 D2: „→ Entscheidung"-Antraege im SELBEN Save mitpraegen (ein Snapshot, ein Kangal-Pass):
        // offene DEC je Antrag + antragendes PBI geblockt; das decision-gate legt sie beim naechsten Lauf vor.
        if (decisionRequests is { Count: > 0 })
        {
            var (withDecs, minted, mintSkipped) = Decision.DecisionRequestMint.Mint(updated, decisionRequests, plan.SourceIngestionRun);
            updated = withDecs;
            if (minted.Count > 0)
                Console.WriteLine($"[pbi-update-apply] R-14 D2: {minted.Count} offene Entscheidung(en) geprägt: {string.Join(", ", minted)} — Vorlage am decision-gate beim nächsten Lauf.");
            foreach (var sk in mintSkipped) Console.WriteLine($"[pbi-update-apply]   antrag übersprungen: {sk}");
            await File.WriteAllTextAsync(Path.Combine(appliedDir, "decision-requests.json"),
                JsonSerializer.Serialize(new { requests = decisionRequests, minted, skipped = mintSkipped }, Json), ct).ConfigureAwait(false);
        }

        await coreRepo.SaveAsync(updated).ConfigureAwait(false);

        // github-sync-Delta: nur die betroffenen (neuen/aktualisierten) PBIs.
        var touched = report.NewPbis.Concat(report.UpdatedPbis).ToHashSet(StringComparer.Ordinal);
        var syncDelta = CoreViews.GithubSync(updated).Entries.Where(e => touched.Contains(e.PbiId)).ToList();
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "github-sync-delta.json"),
            JsonSerializer.Serialize(new { newPbis = report.NewPbis, updatedPbis = report.UpdatedPbis, entries = syncDelta }, Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(appliedDir, "pbi-update-apply-report.json"), JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);
        // Idempotenz-Marker ZULETZT (nach erfolgreichem Core-Write) — so bleibt ein mitten abgebrochener Lauf
        // wiederholbar; erst ein vollstaendiger Apply markiert den Plan als erledigt.
        await File.WriteAllTextAsync(markerPath, JsonSerializer.Serialize(new PbiUpdateAppliedMarker(plan.PlanId, DateTime.UtcNow), Json), ct).ConfigureAwait(false);
        return report;
    }

    // akzeptiert = op-<i> mit decision=apply (fehlende Entscheidung -> default apply). Geteilt zwischen CLI + HITL.
    public static HashSet<int> AcceptedFromDecisions(PbiStateChangePlanDocument plan, IReadOnlyList<PbiUpdateDecision> decisions)
    {
        var byOp = decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var accepted = new HashSet<int>();
        for (var i = 0; i < plan.Operations.Count; i++)
            if (!byOp.TryGetValue($"op-{i}", out var d) || string.Equals(d.Decision, "apply", StringComparison.OrdinalIgnoreCase))
                accepted.Add(i);
        return accepted;
    }

    // R-26-C: die akzeptierten/edierten Angleichungen aus den Entscheidungen ziehen. GOVERNANCE: ohne explizite
    // accept/edit-Entscheidung wird NICHT angeglichen (Inhalts-Mutation an der Wahrheit braucht Freigabe; kein
    // stilles Auto-Apply wie bei den Struktur-Ops). skip => der PBI bleibt ehrlich needs_clarify.
    public static List<PbiAlignment> AcceptedAlignments(PbiStateChangePlanDocument plan, IReadOnlyList<PbiAlignmentDecision>? decisions)
    {
        // O3b: Match über DraftKey (bestehende PbiId für align/extend ODER Ziel-Requirement für create).
        // Die Decision trägt diesen Key im FieldAlignPbiId → PbiAlignmentDecision.PbiId.
        var proposals = (plan.Alignments ?? []).GroupBy(a => a.DraftKey, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var byKey = (decisions ?? []).GroupBy(d => d.PbiId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var result = new List<PbiAlignment>();
        foreach (var (key, proposal) in proposals)
        {
            if (!byKey.TryGetValue(key, out var d)) continue;                         // keine Freigabe => nicht angleichen
            if (string.Equals(d.Decision, "skip", StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(d.Decision, "edit", StringComparison.OrdinalIgnoreCase))
                result.Add(proposal with
                {
                    ProposedTitle = Override(d.EditedTitle, proposal.ProposedTitle),
                    ProposedStatement = Override(d.EditedStatement, proposal.ProposedStatement),
                    ProposedAcceptanceCriteria = d.EditedAcceptanceCriteria is { Count: > 0 } ? d.EditedAcceptanceCriteria : proposal.ProposedAcceptanceCriteria
                });
            else if (string.Equals(d.Decision, "accept", StringComparison.OrdinalIgnoreCase))
                result.Add(proposal);
        }
        return result;
    }

    // B1: die vom Menschen geänderten Feature-Zuordnungen der NEW_PBI-Ops aus den Entscheidungen ziehen. Nur echte
    // Änderungen (abweichend vom Vorschlag) werden zum Override — unveränderte Felder ändern nichts. Persistiert in
    // human-decisions.json (Decision.FeatureId); der CLI- UND der HITL-Pfad rechnen darüber (Parität).
    public static List<PbiFeatureOverride> FeatureOverrides(PbiStateChangePlanDocument plan, IReadOnlyList<PbiUpdateDecision> decisions)
    {
        var byOp = decisions.GroupBy(d => d.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var result = new List<PbiFeatureOverride>();
        for (var i = 0; i < plan.Operations.Count; i++)
        {
            var op = plan.Operations[i];
            if (!string.Equals(op.Kind, PbiUpdateKind.NewPbi, StringComparison.Ordinal)) continue;
            if (!byOp.TryGetValue($"op-{i}", out var d) || string.IsNullOrWhiteSpace(d.FeatureId)) continue;
            // B2: Sentinel-Wert "proposed:<Label>" => Ziel ist ein im Plan vorgeschlagenes NEUES Feature (immer eine
            // Änderung, da op.FeatureId ein echter FC-Id/null ist). Sonst B1: bestehendes Feature, nur bei echter Änderung.
            if (d.FeatureId!.StartsWith(PbiUpdateReviewAdapter.ProposedPrefix, StringComparison.Ordinal))
                result.Add(new PbiFeatureOverride($"op-{i}", ProposedFeatureLabel: d.FeatureId[PbiUpdateReviewAdapter.ProposedPrefix.Length..]));
            else if (!string.Equals(d.FeatureId, op.FeatureId, StringComparison.Ordinal))
                result.Add(new PbiFeatureOverride($"op-{i}", FeatureId: d.FeatureId));
        }
        return result;
    }

    // B1/B2: die Overrides in den Plan einweben — nur NEW_PBI-Ops.
    //   B1: Ziel = bestehendes Feature (existiert im Core) -> op.FeatureId umschreiben.
    //   B2: Ziel = im Plan vorgeschlagenes neues Feature (Label passt zu einer NEW_FEATURE-Op) -> die Op wird zu
    //       NEW_FEATURE mit diesem Label konvertiert; ApplyNewFeatures gruppiert sie dann per Label mit dem/den
    //       schon vorgeschlagenen NEW_FEATURE-Op(s) -> EIN Feature, mehrere PBIs (die eigentliche Kopplung).
    // Ungültiges Ziel (Feature/Label nicht vorhanden) -> Vorschlag behalten, Op fällt nie. PlanId unverändert.
    public static PbiStateChangePlanDocument ApplyFeatureOverrides(
        PbiStateChangePlanDocument plan, IReadOnlyList<PbiFeatureOverride>? featureOverrides, ProjectStateDocument core)
    {
        if (featureOverrides is not { Count: > 0 }) return plan;
        var featureIds = core.Items.Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var proposedLabels = plan.Operations
            .Where(o => string.Equals(o.Kind, PbiUpdateKind.NewFeature, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(o.ProposedFeatureLabel))
            .Select(o => o.ProposedFeatureLabel!.Trim()).ToHashSet(StringComparer.Ordinal);
        var byOp = featureOverrides.GroupBy(o => o.OpId, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        return plan with
        {
            Operations = [.. plan.Operations.Select((op, i) =>
            {
                if (!string.Equals(op.Kind, PbiUpdateKind.NewPbi, StringComparison.Ordinal)) return op;
                if (!byOp.TryGetValue($"op-{i}", out var o)) return op;
                var label = o.ProposedFeatureLabel?.Trim();
                if (!string.IsNullOrWhiteSpace(label) && proposedLabels.Contains(label))   // B2: in vorgeschlagenes neues Feature
                    return op with { Kind = PbiUpdateKind.NewFeature, FeatureId = null, ProposedFeatureLabel = label };
                if (!string.IsNullOrWhiteSpace(o.FeatureId) && featureIds.Contains(o.FeatureId))   // B1: in bestehendes Feature
                    return op with { FeatureId = o.FeatureId };
                return op;
            })]
        };
    }

    private static string? Override(string? edited, string? original) => string.IsNullOrWhiteSpace(edited) ? original : edited.Trim();
}
