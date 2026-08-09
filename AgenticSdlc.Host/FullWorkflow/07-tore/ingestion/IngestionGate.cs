using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// Deterministisches Gate der Requirement-Ingestion (plan-increment1 §1.4): kein stiller Fehler,
// kein Halluzinations-Match. Prueft die Operationen gegen MeetingDelta (Coverage) und Core (Ziele).
public static class IngestionGate
{
    // A1a: profile = Aspekt-Naht (Default Requirement — Alt-Aufrufer/Tests unverändert; Graph reicht explizit).
    public static IngestionGateReport Check(ProjectStateDocument meetingDelta, ProjectStateDocument core, StateChangePlanDocument plan, AspectIngestionProfile? profile = null)
    {
        profile ??= AspectIngestionProfile.Requirement;
        var errors = new List<IngestionGateIssue>();
        var warnings = new List<IngestionGateIssue>();

        var incomingReqIds = meetingDelta.Items
            .Where(i => IsAspect(i, profile))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        // 9g: die Fragen-Spur — eingehende open_question-Items sind vollwertige Coverage-Buerger
        // (jede Frage braucht genau eine Operation), aber mit eigenem, kleinerem Op-Vokabular.
        // 9i: NUR im Frage-tragenden Strip (profile.CarriesQuestionLane) — im anderen Strip sind Fragen
        // weder Coverage-Pflicht noch erlaubtes Op-Ziel (QUESTION_KIND_MISMATCH wacht).
        var incomingQuestionIds = meetingDelta.Items
            .Where(i => profile.CarriesQuestionLane
                        && string.Equals(i.ItemType, "open_question", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        var incomingIds = incomingReqIds.Concat(incomingQuestionIds).ToHashSet(StringComparer.Ordinal);

        var coreReqIds = core.Items
            .Where(i => IsAspect(i, profile))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        var coreDecisionIds = core.Items
            .Where(i => string.Equals(i.ItemType, "decision", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        // ② E-R4 (06.08.): die WAHRHEITS-Items (requirement|architecture) — nur sie duerfen widersprochen
        // werden, und CONTRADICT darf als EINZIGE Operation den Aspekt queren.
        var truthById = core.Items
            .Where(i => string.Equals(i.ItemType, "requirement", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(i => i.ItemId, i => i, StringComparer.Ordinal);

        var coreFeatureIds = core.Items
            .Where(i => string.Equals(i.ItemType, "feature", StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var op in plan.Operations)
        {
            if (!incomingIds.Contains(op.IncomingItemId))
                errors.Add(Issue("UNKNOWN_INCOMING", "error", $"Operation referenziert unbekanntes incoming Item '{op.IncomingItemId}'.", op.IncomingItemId, op.TargetEntityId));

            if (!StateChangeKind.All.Contains(op.Kind))
                errors.Add(Issue("UNKNOWN_KIND", "error", $"Unbekannte Operation '{op.Kind}' fuer '{op.IncomingItemId}'.", op.IncomingItemId, null));

            // 9g: Kategorien-Wache Frage<->Anforderung (beide Richtungen): OPEN_QUESTION nur fuer eingehende
            // Fragen; eingehende Fragen nur mit dem Frage-Vokabular (OPEN_QUESTION/ALREADY_DECIDED).
            if (string.Equals(op.Kind, StateChangeKind.OpenQuestion, StringComparison.Ordinal)
                && !incomingQuestionIds.Contains(op.IncomingItemId))
                errors.Add(Issue("QUESTION_KIND_MISMATCH", "error", $"'{op.Kind}' ist nur fuer eingehende offene Fragen erlaubt ('{op.IncomingItemId}' ist keine).", op.IncomingItemId, null));
            if (incomingQuestionIds.Contains(op.IncomingItemId)
                && StateChangeKind.All.Contains(op.Kind)
                && !StateChangeKind.ForQuestions.Contains(op.Kind))
                errors.Add(Issue("QUESTION_KIND_MISMATCH", "error", $"Eingehende Frage '{op.IncomingItemId}' erlaubt nur OPEN_QUESTION/ALREADY_DECIDED, nicht '{op.Kind}'.", op.IncomingItemId, null));

            var requiresReqTarget = StateChangeKind.RequireTarget.Contains(op.Kind);
            var requiresDecisionTarget = StateChangeKind.RequireDecisionTarget.Contains(op.Kind);
            var forbidsTarget = StateChangeKind.ForbidTarget.Contains(op.Kind);
            var hasTarget = !string.IsNullOrWhiteSpace(op.TargetEntityId);

            if ((requiresReqTarget || requiresDecisionTarget) && !hasTarget)
                errors.Add(Issue("TARGET_REQUIRED", "error", $"'{op.Kind}' braucht targetEntityId ('{op.IncomingItemId}').", op.IncomingItemId, null));
            if (forbidsTarget && hasTarget)
                errors.Add(Issue("TARGET_FORBIDDEN", "error", $"'{op.Kind}' darf kein targetEntityId haben ('{op.IncomingItemId}').", op.IncomingItemId, op.TargetEntityId));
            if (hasTarget && requiresDecisionTarget && !coreDecisionIds.Contains(op.TargetEntityId!))
                errors.Add(Issue("UNKNOWN_TARGET", "error", $"targetEntityId '{op.TargetEntityId}' ist keine bestehende Open Decision.", op.IncomingItemId, op.TargetEntityId));
            else if (hasTarget && !requiresDecisionTarget && string.Equals(op.Kind, StateChangeKind.Contradict, StringComparison.Ordinal))
            {
                // ② E-R4: CONTRADICT darf QUEREN — Ziel muss ein AKTIVES Wahrheits-Item sein (eigen- ODER quer-aspektig).
                if (!truthById.TryGetValue(op.TargetEntityId!, out var truth))
                    errors.Add(Issue("UNKNOWN_TARGET", "error", $"targetEntityId '{op.TargetEntityId}' ist kein Wahrheits-Item (requirement|architecture) im Core.", op.IncomingItemId, op.TargetEntityId));
                else if (truth.ReadStatus().Validity != Validity.Active)
                    errors.Add(Issue("CONTRADICT_TARGET_INACTIVE", "error", $"CONTRADICT-Ziel '{op.TargetEntityId}' ist nicht aktiv (status={truth.Status}) — widersprich der AKTIVEN Wahrheit oder nutze ALREADY_DECIDED.", op.IncomingItemId, op.TargetEntityId));
            }
            else if (hasTarget && !requiresDecisionTarget && !coreReqIds.Contains(op.TargetEntityId!))
                errors.Add(truthById.ContainsKey(op.TargetEntityId!)
                    ? Issue("CROSS_ASPECT_FORBIDDEN", "error", $"'{op.Kind}' ist strikt eigen-aspektig ({profile.Aspect}) — '{op.TargetEntityId}' gehoert zum anderen Wahrheits-Aspekt; nur CONTRADICT darf queren.", op.IncomingItemId, op.TargetEntityId)
                    : Issue("UNKNOWN_TARGET", "error", $"targetEntityId '{op.TargetEntityId}' existiert nicht im Core.", op.IncomingItemId, op.TargetEntityId));

            // R-36 v2: featureKey ist nur noch ein HINWEIS fuer die Placement-Stufe (IngestionApply schreibt keine
            // part_of_feature-Relation mehr; die Kante entsteht deterministisch im pbi-update-Apply). Ein nicht
            // aufloesbarer Hinweis ist darum kein Blocker mehr — aber sichtbar (Warnung im Review).
            if (string.Equals(op.Kind, StateChangeKind.NewRelated, StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(op.FeatureKey)
                && !coreFeatureIds.Contains(op.FeatureKey!))
                warnings.Add(Issue("UNKNOWN_FEATURE", "warning", $"featureKey '{op.FeatureKey}' ist kein bestehendes Core-Feature ('{op.IncomingItemId}') — nur Hinweis, keine Relation.", op.IncomingItemId, op.FeatureKey));

            if (op.ClaimIds.Count == 0)
                warnings.Add(Issue("MISSING_EVIDENCE", "warning", $"Operation '{op.IncomingItemId}' ohne claimIds (Beleg).", op.IncomingItemId, op.TargetEntityId));

            // R-35: relatedRejectionId ist ein HINWEIS (rendert die Wiedervorlage-Note) — ein nicht aufloesbarer
            // Verweis blockt nicht (fail-open, wie der featureKey-Hinweis), wird aber sichtbar gemacht.
            if (!string.IsNullOrWhiteSpace(op.RelatedRejectionId)
                && !core.Proposals.Any(p => string.Equals(p.ProposalType, IngestionRejections.ProposalType, StringComparison.Ordinal)
                                            && string.Equals(p.ProposalId, op.RelatedRejectionId, StringComparison.Ordinal)))
                warnings.Add(Issue("UNKNOWN_REJECTION_REF", "warning", $"relatedRejectionId '{op.RelatedRejectionId}' ist keine bekannte Ablehnung ('{op.IncomingItemId}') — Note entfaellt.", op.IncomingItemId, op.RelatedRejectionId));
        }

        // Coverage: jedes eingehende Requirement genau eine Operation.
        var opsByIncoming = plan.Operations
            .GroupBy(o => o.IncomingItemId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);

        foreach (var (id, count) in opsByIncoming.Where(kv => kv.Value > 1))
            errors.Add(Issue("DUPLICATE_OP", "error", $"incoming Item '{id}' hat {count} Operationen (genau eine erlaubt).", id, null));

        foreach (var id in incomingIds.Where(id => !opsByIncoming.ContainsKey(id)))
            errors.Add(Issue("UNPLACED_INCOMING", "error", $"incoming Item '{id}' hat keine Operation.", id, null));

        // Mehrere Operationen auf dasselbe Ziel -> Warnung (nicht zwingend falsch, aber pruefenswert).
        foreach (var g in plan.Operations
                     .Where(o => !string.IsNullOrWhiteSpace(o.TargetEntityId))
                     .GroupBy(o => o.TargetEntityId!, StringComparer.Ordinal)
                     .Where(g => g.Count() > 1))
            warnings.Add(Issue("MULTIPLE_OPS_SAME_TARGET", "warning", $"{g.Count()} Operationen zeigen auf '{g.Key}'.", null, g.Key));

        var pass = errors.Count == 0;
        return new IngestionGateReport(pass, pass ? "accept" : "block", errors, warnings);
    }

    private static bool IsAspect(ProjectStateItem i, AspectIngestionProfile profile) => profile.Matches(i);

    private static IngestionGateIssue Issue(string code, string severity, string message, string? incoming, string? target)
        => new(code, severity, message, incoming, target, RepairabilityOf(code));

    // R7: reparierbar = Plan-Qualitaet des Resolvers (per GateFeedback fixbar); UNKNOWN_KIND = needs_human.
    private static readonly IReadOnlyDictionary<string, string> Classification = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["UNPLACED_INCOMING"] = Core.Repairability.Repairable,
        ["DUPLICATE_OP"] = Core.Repairability.Repairable,
        ["MULTIPLE_OPS_SAME_TARGET"] = Core.Repairability.Repairable,
        ["UNKNOWN_TARGET"] = Core.Repairability.Repairable,
        ["CONTRADICT_TARGET_INACTIVE"] = Core.Repairability.Repairable,
        ["CROSS_ASPECT_FORBIDDEN"] = Core.Repairability.Repairable,
        ["TARGET_REQUIRED"] = Core.Repairability.Repairable,
        ["TARGET_FORBIDDEN"] = Core.Repairability.Repairable,
        ["UNKNOWN_INCOMING"] = Core.Repairability.Repairable,
        ["UNKNOWN_FEATURE"] = Core.Repairability.Repairable,
        ["UNKNOWN_REJECTION_REF"] = Core.Repairability.Repairable,
        ["QUESTION_KIND_MISMATCH"] = Core.Repairability.Repairable,
        ["UNKNOWN_KIND"] = Core.Repairability.NeedsHuman,
    };

    private static string RepairabilityOf(string code) => Classification.GetValueOrDefault(code, Core.Repairability.Hard);

    public static Core.GateDecision Decide(IngestionGateReport report, int attempt, int maxAttempts)
        => Core.GateLoop.Decide(report.Pass, report.Errors.Any(e => string.Equals(e.Repairability, Core.Repairability.Repairable, StringComparison.Ordinal)), attempt, maxAttempts);
}
