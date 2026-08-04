using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

public sealed record PbiUpdateApplyReport(
    IReadOnlyList<string> NewPbis,
    IReadOnlyList<string> UpdatedPbis,
    IReadOnlyDictionary<string, string> FinalStatus,
    int RelationsAdded,
    int RelationsRemoved,
    IReadOnlyList<string> Skipped);

// Deterministischer Update-by-Identity der akzeptierten PBI-Operationen (plan-increment1c3 §7). Mehrere Ops pro PBI
// werden geordnet zusammengefuehrt (MULTI_CAUSE_MERGE): EINE Version++, Status per Praezedenz, alle Gruende in History.
// Unberuehrte PBIs bleiben unangetastet. NEW_PBI praegt eine stabile PBI-<n>. SUPERSEDE_PBI swappt das Requirement.
public static class PbiUpdateApply
{
    public static (ProjectStateDocument Core, PbiUpdateApplyReport Report) Apply(
        ProjectStateDocument core, PbiStateChangePlanDocument plan, ISet<int> acceptedIndices, string sourceRun,
        IReadOnlyList<PbiAlignment>? acceptedAlignments = null)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var relations = core.Relations.ToList();
        var nextPbi = MaxSuffix(order, "PBI") + 1;
        int relAdded = 0, relRemoved = 0;
        var newPbis = new List<string>();
        var updated = new List<string>();
        var skipped = new List<string>();
        // R-26-C: akzeptierte/edierte Angleichungen je bestehendem PBI (letzte gewinnt). Leer => kein Alignment.
        var alignByPbi = (acceptedAlignments ?? [])
            .Where(a => a.PbiId is not null)
            .GroupBy(a => a.PbiId!, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        // O3b (create): Drafts fuer NEW_PBI sind ueber die Ziel-Requirement adressiert (kein PbiId).
        var alignByRequirement = (acceptedAlignments ?? [])
            .Where(a => a.PbiId is null && a.TargetRequirementId is not null)
            .GroupBy(a => a.TargetRequirementId!, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        var accepted = plan.Operations.Where((_, i) => acceptedIndices.Contains(i)).ToList();

        // R-36 v2: REQ→Feature wird DETERMINISTISCH aus der bestaetigten Deckung abgeleitet — die Seed-Regel
        // („requirement→part_of_feature aus den PBIs abgeleitet", CoreBacklogSeeder) auch im Betrieb. Nie
        // agent-benannt, feuert erst NACH der Gate-/Human-Bestaetigung, idempotent gegen Doppel-Kanten.
        void EnsureReqFeature(string? reqId, string? featureId)
        {
            if (string.IsNullOrWhiteSpace(reqId) || string.IsNullOrWhiteSpace(featureId) || !byId.ContainsKey(featureId!)) return;
            if (relations.Any(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                                   && string.Equals(r.FromId, reqId, StringComparison.Ordinal)
                                   && string.Equals(r.ToId, featureId, StringComparison.Ordinal))) return;
            relations.Add(new ProjectStateRelation(reqId!, featureId!, "part_of_feature", "pbi-update", new Dictionary<string, string>())); relAdded++;
        }

        // Feature eines bestehenden PBI: primaer die part_of_feature-Relation, Fallback Metadatum featureId.
        string? FeatureOf(ProjectStateItem pbi)
            => relations.FirstOrDefault(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                                             && string.Equals(r.FromId, pbi.ItemId, StringComparison.Ordinal))?.ToId
               ?? pbi.Metadata.GetValueOrDefault("featureId");

        // 1) NEW_PBI: neue Core-PBI-Entitaet je Operation.
        foreach (var op in accepted.Where(o => string.Equals(o.Kind, PbiUpdateKind.NewPbi, StringComparison.Ordinal)))
        {
            if (op.FeatureId is null || !byId.ContainsKey(op.FeatureId)) { skipped.Add($"NEW_PBI {op.RequirementId}: feature '{op.FeatureId}' unbekannt"); continue; }
            var id = $"PBI-{nextPbi++:D3}";
            var skeletonTitle = byId.TryGetValue(op.RequirementId, out var rq) ? Truncate(rq.Text, 90) : op.RequirementId;

            // O3b (create): akzeptierter Draft macht aus dem Skelett ein volles PBI (Titel/Goal/AK) + active;
            // ohne Draft bleibt es das ehrliche Skelett (needs_clarify). Leere Draft-Felder => Skelett-Wert.
            var draft = alignByRequirement.GetValueOrDefault(op.RequirementId);
            var title = !string.IsNullOrWhiteSpace(draft?.ProposedTitle) ? draft!.ProposedTitle!.Trim() : skeletonTitle;
            var goal = string.IsNullOrWhiteSpace(draft?.ProposedStatement) ? null : draft!.ProposedStatement!.Trim();
            var acceptance = draft?.ProposedAcceptanceCriteria is { Count: > 0 } ac ? ac : (IReadOnlyList<string>)[];
            var readiness = draft is not null ? PbiStatus.Active : PbiStatus.NeedsClarify;

            var payload = new PbiPayload(
                Goal: goal, Title: title, AcceptanceCriteria: acceptance, LinkedRequirementIds: [op.RequirementId],
                OpenDecisionRefs: [], PriorityRank: null, Readiness: readiness, Mvp: null, Trace: null);
            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            { ["sourceRunId"] = sourceRun, ["createdFromRequirement"] = op.RequirementId, ["featureId"] = op.FeatureId };
            AddItem(order, byId, new ProjectStateItem(
                ItemId: id, ItemType: "pbi", Text: title, Origin: "pbi-update", Stage: null, Version: 1,
                SourceRunId: sourceRun, SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null, SourceCandidateId: null,
                SourceClaimIds: [], SourceArtifactItemIds: [], Metadata: meta, IdentityKey: null, History: [], Feature: null, Pbi: payload).WithStatus(CoreStatus.From(readiness)));   // §5-S7: Status→Achsen
            relations.Add(new ProjectStateRelation(id, op.FeatureId, "part_of_feature", "pbi-update", new Dictionary<string, string>())); relAdded++;
            relations.Add(RequirementSwap.Covers(id, op.RequirementId, "pbi-update")); relAdded++;
            EnsureReqFeature(op.RequirementId, op.FeatureId);   // R-36 v2: abgeleitete REQ→Feature-Kante
            newPbis.Add(id);
        }

        // 2) Bestehende PBIs: mehrere Ursachen geordnet zusammenfuehren.
        foreach (var g in accepted.Where(o => PbiUpdateKind.RequirePbi.Contains(o.Kind) && o.PbiId is not null)
                     .GroupBy(o => o.PbiId!, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(g.Key, out var pbi) || pbi.Pbi is null) { skipped.Add($"{g.Key}: kein PBI"); continue; }

            var links = pbi.Pbi.LinkedRequirementIds.ToList();
            var decRefs = pbi.Pbi.OpenDecisionRefs.ToList();
            var status = pbi.ReadStatus();   // §5-S5: typisiert; Blocker-Eskalation via Escalate statt PbiStatus.Max
            var reasons = new List<string>();

            foreach (var op in g)
            {
                reasons.Add($"{op.Kind}: {op.Rationale}");
                switch (op.Kind)
                {
                    case PbiUpdateKind.ExtendPbi:
                        if (!links.Contains(op.RequirementId)) { links.Add(op.RequirementId); relations.Add(RequirementSwap.Covers(pbi.ItemId, op.RequirementId, "pbi-update")); relAdded++; }
                        EnsureReqFeature(op.RequirementId, FeatureOf(pbi));   // R-36 v2: REQ erbt das Feature des PBI
                        status = status.Escalate(Blocker.NeedsClarify);
                        break;
                    case PbiUpdateKind.MarkChanged:
                        status = status.Escalate(Blocker.NeedsClarify);
                        break;
                    case PbiUpdateKind.BlockPbi:
                        if (op.OpenDecisionRef is not null && !decRefs.Contains(op.OpenDecisionRef)) decRefs.Add(op.OpenDecisionRef);
                        status = status.Escalate(Blocker.BlockedByDecision);
                        break;
                    case PbiUpdateKind.SupersedePbi:
                        // Geteilter Wahrheitsuebergang (T2.0) — identisch fuer pbi-update und Tor 2 ADOPT_NEW.
                        var (swAdded, swRemoved) = RequirementSwap.SwapCoverage(
                            pbi.ItemId, op.RequirementId, op.ReplacementRequirementId, links, relations, "pbi-update");
                        relAdded += swAdded; relRemoved += swRemoved;
                        EnsureReqFeature(op.ReplacementRequirementId, FeatureOf(pbi));   // R-36 v2: Ersatz-REQ erbt das Feature
                        status = status.Escalate(Blocker.NeedsClarify);
                        break;
                }
            }

            // R-26-C: akzeptierte Angleichung uebernimmt den PBI-Inhalt und klaert needs_clarify -> active.
            // Leere Vorschlagsfelder = Original behalten. Blocked bleibt blocked (staerker als needs_clarify).
            var title = pbi.Pbi.Title;
            var goal = pbi.Pbi.Goal;
            var acceptance = pbi.Pbi.AcceptanceCriteria;
            var aligned = false;
            if (alignByPbi.TryGetValue(pbi.ItemId, out var align))
            {
                if (!string.IsNullOrWhiteSpace(align.ProposedTitle)) title = align.ProposedTitle!.Trim();
                if (!string.IsNullOrWhiteSpace(align.ProposedStatement)) goal = align.ProposedStatement!.Trim();
                if (align.ProposedAcceptanceCriteria is { Count: > 0 } ac) acceptance = ac;
                if (status.Blocker == Blocker.NeedsClarify) status = status with { Blocker = Blocker.None };   // R-26-C: needs_clarify -> active
                reasons.Add($"ALIGN(R-26-C): {align.Rationale}");
                aligned = true;
            }

            // §5-S3: zentrale Status-Naht — Alt-String bleibt (via Max) die Rechen-Grundlage, neue Felder abgeleitet
            // (`From`; der `Max`-Hack fliegt erst in S5). History-Notiz inklusive.
            byId[pbi.ItemId] = pbi
                .WithStatus(status, string.Join(" | ", reasons))
                with
                {
                    // R-31: bei inhaltlicher Angleichung traegt die neue Fassung den ausloesenden Lauf als Ursprung
                    // (statt den alten Baseline-Lauf zu erben); reine Struktur-Ops lassen die Provenance unberuehrt.
                    Text = title,
                    Version = pbi.Version + 1,
                    SourceRunId = aligned ? sourceRun : pbi.SourceRunId,
                    Pbi = pbi.Pbi with { Title = title, Goal = goal, AcceptanceCriteria = acceptance, LinkedRequirementIds = links, OpenDecisionRefs = decRefs }
                };
            updated.Add(pbi.ItemId);
        }

        var items = order.Select(id => byId[id]).ToList();
        var coreUpdated = core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items, Relations = relations };

        // O4b (Fall C): akzeptierte NEW_FEATURE-Ops NACH den normalen Ops -> Seeder-Adapter (neues Feature + PBI).
        var (coreFinal, featurePbis, featureRels) = ApplyNewFeatures(coreUpdated, accepted, alignByRequirement, sourceRun, skipped);
        newPbis.AddRange(featurePbis);
        relAdded += featureRels;

        var byIdFinal = coreFinal.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var finalStatus = newPbis.Concat(updated).Where(byIdFinal.ContainsKey)
            .ToDictionary(id => id, id => byIdFinal[id].Status, StringComparer.Ordinal);
        return (coreFinal, new PbiUpdateApplyReport(newPbis, updated, finalStatus, relAdded, relRemoved, skipped));
    }

    // O4b (Fall C): legt neue Features + ihre PBIs an — via den geprueften CoreBacklogSeeder (Option 3). Der
    // PBI-Inhalt kommt aus dem O3b-Create-Draft (per RequirementId). Reihenfolge: NACH den normalen Ops.
    // WICHTIG (Kollegen-Fund): mehrere NEW_FEATURE-Ops mit DEMSELBEN proposedFeatureLabel gehoeren zu EINEM neuen
    // Feature (mit mehreren PBIs), NICHT zu N separaten Features — der Placement-Agent liefert pro Requirement
    // eine eigene Op mit gleichem Label. Governance: nur Ops MIT akzeptiertem Draft werden PBIs; eine Gruppe ohne
    // jeden Draft legt KEIN Feature an (kein aktives leeres Skelett).
    private static (ProjectStateDocument Core, List<string> NewPbis, int RelationsAdded) ApplyNewFeatures(
        ProjectStateDocument core, List<PbiStateChangeOperation> accepted,
        IReadOnlyDictionary<string, PbiAlignment> alignByRequirement, string sourceRun, List<string> skipped)
    {
        var ops = accepted.Where(o => string.Equals(o.Kind, PbiUpdateKind.NewFeature, StringComparison.Ordinal)
                                      && !string.IsNullOrWhiteSpace(o.ProposedFeatureLabel)).ToList();
        if (ops.Count == 0) return (core, [], 0);

        var current = core;
        var addedPbis = new List<string>();
        var relsAdded = 0;
        foreach (var group in ops.GroupBy(o => o.ProposedFeatureLabel!.Trim(), StringComparer.Ordinal))
        {
            var withDraft = new List<(PbiStateChangeOperation Op, PbiAlignment Draft)>();
            foreach (var op in group)
            {
                var draft = alignByRequirement.GetValueOrDefault(op.RequirementId);
                if (draft is null)
                    skipped.Add($"NEW_FEATURE {op.RequirementId}: kein akzeptierter Create-Draft — nicht angelegt.");
                else
                    withDraft.Add((op, draft));
            }
            if (withDraft.Count == 0) continue;   // ganze Label-Gruppe ohne Draft -> kein leeres Feature

            var fcId = NextFeatureId(current);
            var fcCompact = fcId.Replace("-", "");
            var reqIds = withDraft.Select(x => x.Op.RequirementId).ToList();
            var cluster = new FeatureCluster(fcId, fcId, group.Key, reqIds, [], withDraft[0].Op.Rationale);
            var clusterSet = new FeatureClusterSet(FeatureClusterSet.CurrentSchemaVersion, $"cs-{fcId}",
                core.ProjectId, "op", DateTime.UtcNow, "pbi-update", [cluster]);

            // Ein PBI je Op der Gruppe — alle im selben Feature (PBI-<fcCompact>-nn -> ResolveFeatureId -> fcId).
            var pbiItems = withDraft.Select((x, idx) => new ProductBacklogItem(
                $"PBI-{fcCompact}-{idx + 1:D2}", $"ik-{fcId}-{idx + 1}", 1, "delivery",
                x.Draft.ProposedTitle ?? x.Op.RequirementId, [x.Op.RequirementId])
            {
                Goal = x.Draft.ProposedStatement,
                AcceptanceCriteria = x.Draft.ProposedAcceptanceCriteria ?? [],
                Traceability = new PbiTraceability([], [], [x.Op.RequirementId], [])
            }).ToList();
            var backlog = new ProductBacklogDocument(ProductBacklogDocument.CurrentSchemaVersion, $"bk-{fcId}",
                core.ProjectId, "op", DateTime.UtcNow, "pbi-update", pbiItems);

            var before = current.Items.Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
            var (seeded, report) = CoreBacklogSeeder.Seed(current, clusterSet, backlog, sourceRun);
            current = seeded;
            relsAdded += report.RelationsAdded;
            addedPbis.AddRange(seeded.Items
                .Where(i => !before.Contains(i.ItemId) && string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
                .Select(i => i.ItemId));
        }
        return (current, addedPbis, relsAdded);
    }

    // Freie FC-<n>-ID (numerisch, kollisionssicher gegen bestehende Features inkl. Suffix-Varianten wie FC-13A).
    private static string NextFeatureId(ProjectStateDocument core)
    {
        var existing = core.Items.Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
        var max = existing.Where(id => id.StartsWith("FC-", StringComparison.Ordinal))
            .Select(id => new string(id["FC-".Length..].TakeWhile(char.IsDigit).ToArray()))
            .Where(s => s.Length > 0).Select(int.Parse).DefaultIfEmpty(0).Max();
        string candidate;
        do { candidate = $"FC-{++max:D2}"; } while (existing.Contains(candidate));
        return candidate;
    }

    private static void AddItem(List<string> order, Dictionary<string, ProjectStateItem> byId, ProjectStateItem item)
    { byId[item.ItemId] = item; order.Add(item.ItemId); }

    private static int MaxSuffix(IEnumerable<string> ids, string prefix)
    {
        var p = prefix + "-";
        return ids.Where(id => id.StartsWith(p, StringComparison.Ordinal)).Select(id => id[p.Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit)).Select(int.Parse).DefaultIfEmpty(0).Max();
    }

    private static string Truncate(string value, int max)
    {
        var t = string.Join(' ', (value ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return t.Length <= max ? t : t[..max] + "...";
    }
}
