using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

// T2.1 — die deterministische State-Maschine der Decision-Ingestion. Für jede freigegebene Auflösung:
//   RESOLVE_DECISION (DEC open_decision -> resolved, History + resolutionOutcome; contradicts -> contradicts_resolved,
//   NICHT gelöscht) + Outcome-Downstream (KEEP_ORIGINAL / ADOPT_NEW: superseded + neues Requirement + Swap via
//   RequirementSwap / REFINE: Requirement neu formulieren) + UNBLOCK_PBI (DEC-Ref raus, Status je Outcome).
// `done` gibt es hier nicht; `done` ist Reverse-Tor (E4). Core NUR über den Port.
public static class DecisionResolutionApply
{
    public static (ProjectStateDocument Core, DecisionResolutionApplyReport Report) Apply(
        ProjectStateDocument core, DecisionResolutionPlanDocument plan, ISet<int> acceptedIndices, string sourceRun)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var nextReq = MaxSuffix(order, "REQ") + 1;

        var resolved = new List<string>();
        var superseded = new List<string>();
        var refined = new List<string>();
        var newReqs = new List<string>();
        var unblocked = new List<string>();
        var swapped = new List<string>();
        var skipped = new List<string>();

        var accepted = plan.Operations.Where((_, i) => acceptedIndices.Contains(i)).ToList();

        foreach (var op in accepted)
        {
            if (!byId.TryGetValue(op.DecisionId, out var dec) || !dec.ReadStatus().IsOpenDecision)
            { skipped.Add($"{op.DecisionId}: nicht (mehr) offen"); continue; }
            if (!byId.TryGetValue(op.TargetRequirementId, out var target))
            { skipped.Add($"{op.DecisionId}: Ziel-Requirement fehlt"); continue; }

            var unblockStatus = string.Equals(op.Outcome, DecisionOutcome.KeepOriginal, StringComparison.Ordinal)
                ? PbiStatus.Active : PbiStatus.NeedsClarify;
            string? newReqId = null;

            // 1) Outcome-Downstream am Ziel-Requirement.
            switch (op.Outcome)
            {
                case DecisionOutcome.Refine:
                    byId[target.ItemId] = target with
                    {
                        Text = op.NewStatement!,
                        Version = target.Version + 1,
                        IdentityKey = IdentityKey.From(op.NewStatement!),
                        History = Hist(target, $"REFINE via {op.DecisionId}")
                    };
                    refined.Add(target.ItemId);
                    break;
                case DecisionOutcome.AdoptNew:
                    // §5-S3: zentrale Status-Naht (setzt neue Felder + Alt-String synchron; History-Notiz inklusive).
                    byId[target.ItemId] = target.WithStatus(CoreStatus.From("superseded"), $"superseded via {op.DecisionId} (ADOPT_NEW)");
                    newReqId = $"REQ-{nextReq++:D2}";
                    AddItem(order, byId, NewRequirement(newReqId, op.NewStatement!, target, sourceRun, op.DecisionId));
                    relations.Add(new ProjectStateRelation(newReqId, target.ItemId, DecisionRelations.Supersedes, "decision-tor2", new Dictionary<string, string>()));
                    superseded.Add(target.ItemId);
                    newReqs.Add(newReqId);
                    break;
                case DecisionOutcome.KeepOriginal:
                    break; // Ziel-Requirement bleibt unverändert.
            }

            // 2) Widerspruch auflösen — contradicts -> contradicts_resolved (Rev 2: nicht hart löschen).
            var ci = relations.FindIndex(r => string.Equals(r.RelationType, DecisionRelations.Contradicts, StringComparison.Ordinal)
                && string.Equals(r.FromId, op.DecisionId, StringComparison.Ordinal) && string.Equals(r.ToId, target.ItemId, StringComparison.Ordinal));
            if (ci >= 0)
            {
                var meta = new Dictionary<string, string>(relations[ci].Metadata, StringComparer.Ordinal)
                { ["resolutionOutcome"] = op.Outcome, ["resolvedUtc"] = DateTime.UtcNow.ToString("O"), ["decisionId"] = op.DecisionId };
                relations[ci] = relations[ci] with { RelationType = DecisionRelations.ContradictsResolved, Metadata = meta };
            }

            // 3) DEC resolved (History + resolutionOutcome).
            var decMeta = new Dictionary<string, string>(dec.Metadata, StringComparer.Ordinal)
            { ["resolutionOutcome"] = op.Outcome, ["resolvedUtc"] = DateTime.UtcNow.ToString("O") };
            // §5-S7 (Option A): Status ist Projektion — der Resolved-Zustand wird über die Decision-Achse gesetzt
            // (ToLegacyString prüft Decision zuerst → "resolved", bit-identisch). Governance/Rest-Achsen bleiben erhalten.
            byId[op.DecisionId] = dec.WithStatus(dec.ReadStatus() with { Decision = DecisionState.Resolved }) with
            {
                Version = dec.Version + 1,
                Metadata = decMeta,
                History = Hist(dec, $"resolved={op.Outcome}; war contradicts {target.ItemId}")
            };
            resolved.Add(op.DecisionId);

            // 4) UNBLOCK_PBI + (ADOPT_NEW) Coverage-Swap je betroffenem PBI.
            foreach (var pbiId in op.AffectedPbis)
            {
                if (!byId.TryGetValue(pbiId, out var pbi) || pbi.Pbi is null) continue;
                var links = pbi.Pbi.LinkedRequirementIds.ToList();
                var decRefs = pbi.Pbi.OpenDecisionRefs.ToList();
                var reasons = new List<string>();
                var changed = false;

                if (string.Equals(op.Outcome, DecisionOutcome.AdoptNew, StringComparison.Ordinal) && links.Contains(target.ItemId))
                {
                    RequirementSwap.SwapCoverage(pbiId, target.ItemId, newReqId, links, relations, "decision-tor2");
                    reasons.Add($"coverage-swap {target.ItemId}->{newReqId}");
                    swapped.Add(pbiId);
                    changed = true;
                }

                var wasBlocked = decRefs.Remove(op.DecisionId);
                if (wasBlocked) { reasons.Add($"unblock {op.DecisionId}"); changed = true; }

                var newStatus = wasBlocked
                    ? (decRefs.Count > 0 ? PbiStatus.BlockedByDecision : unblockStatus)
                    : (string.Equals(op.Outcome, DecisionOutcome.KeepOriginal, StringComparison.Ordinal) ? pbi.Status : pbi.ReadStatus().Escalate(Blocker.NeedsClarify).ToLegacyString());   // §5-S5: Escalate statt PbiStatus.Max
                if (!string.Equals(newStatus, pbi.Status, StringComparison.Ordinal)) { reasons.Add($"status {pbi.Status}->{newStatus}"); changed = true; }

                if (!changed) continue;
                // §5-S3: zentrale Status-Naht — Alt-String bleibt (via Max) die Rechen-Grundlage, neue Felder werden
                // daraus abgeleitet (`From`); der `Max`-Hack selbst fliegt erst in S5. History-Notiz inklusive.
                byId[pbiId] = pbi
                    .WithStatus(CoreStatus.From(newStatus), string.Join(" | ", reasons))
                    with { Version = pbi.Version + 1, Pbi = pbi.Pbi with { LinkedRequirementIds = links, OpenDecisionRefs = decRefs } };
                if (wasBlocked) unblocked.Add(pbiId);
            }
        }

        var items = order.Select(id => byId[id]).ToList();
        var updated = core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items, Relations = relations };
        return (updated, new DecisionResolutionApplyReport(resolved, superseded, refined, newReqs, unblocked.Distinct(StringComparer.Ordinal).ToList(), swapped.Distinct(StringComparer.Ordinal).ToList(), skipped));
    }

    private static IReadOnlyList<ProjectStateItemVersion> Hist(ProjectStateItem it, string note)
        => (it.History ?? []).Append(new ProjectStateItemVersion(
            it.Version, it.Text, it.Status, it.Origin, it.SourceRunId, it.SourceClaimIds, DateTime.UtcNow, note)).ToList();

    private static ProjectStateItem NewRequirement(string id, string text, ProjectStateItem from, string sourceRun, string decisionId)
        => new ProjectStateItem(
            ItemId: id, ItemType: "requirement", Text: text, Origin: "decision-tor2",
            Stage: from.Stage, Version: 1, SourceRunId: sourceRun, SourceArtifactId: null, SourceArtifactType: null,
            SourceDecisionId: decisionId, SourceCandidateId: null, SourceClaimIds: [], SourceArtifactItemIds: [],
            Metadata: new Dictionary<string, string>(StringComparer.Ordinal) { ["adoptedFromDecision"] = decisionId },
            IdentityKey: IdentityKey.From(text), History: []).WithStatus(CoreStatus.From("accepted"));   // §5-S7: Status→Achsen

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    { byId[item.ItemId] = item; order.Add(item.ItemId); }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal)).Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit)).Select(int.Parse).DefaultIfEmpty(0).Max();
    }
}
