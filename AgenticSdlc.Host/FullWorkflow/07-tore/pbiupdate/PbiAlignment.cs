using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

// R-26-C (A1, Slice 3): der PBI-Angleichungs-Agent. Nach der deterministischen Ableitung (MARK_CHANGED/
// SUPERSEDE = "PBI betroffen") schlaegt er je betroffenem PBI den angeglichenen INHALT vor (Titel/Statement/
// Akzeptanzkriterien), sodass er zur neuen Anforderung passt. Governance unveraendert: der Agent schlaegt NUR
// vor (save_alignments); die menschliche Freigabe im PBI-Update-Review + der deterministische Apply schreiben
// erst in den Core (needs_clarify -> active).

// Das Ziel einer Angleichung: ein betroffenes PBI + seine aktuelle Fassung + die ausloesenden Anforderungen.
// O3b (create): fuer NEW_PBI existiert noch kein PBI — dann ist PbiId null und die create-Felder (Ziel-Requirement,
// Ziel-Feature + Label, Titel der bestehenden Feature-PBIs als Duplikat-Kontext) tragen den Kontext.
public sealed record PbiAlignTarget(
    string? PbiId, string? CurrentTitle, string? CurrentStatement, IReadOnlyList<string> CurrentAcceptance,
    IReadOnlyList<PbiAlignTrigger> Triggers,
    string? TargetRequirementId = null, string? TargetFeatureId = null, string? FeatureLabel = null,
    IReadOnlyList<string>? SiblingPbiTitles = null);

public sealed record PbiAlignTrigger(string RequirementId, string NewText, string? OldText);

// Deterministisch (kein LLM, testbar): welche PBIs brauchen eine Angleichung + mit welchem Kontext.
public static class PbiAlignTargets
{
    public static IReadOnlyList<PbiAlignTarget> Collect(PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var targets = new List<PbiAlignTarget>();
        foreach (var g in plan.Operations
                     .Where(o => (o.Kind == PbiUpdateKind.MarkChanged || o.Kind == PbiUpdateKind.SupersedePbi || o.Kind == PbiUpdateKind.ExtendPbi) && o.PbiId is not null)
                     .GroupBy(o => o.PbiId!, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(g.Key, out var pbi) || pbi.Pbi is null) continue;
            var triggers = new List<PbiAlignTrigger>();
            foreach (var op in g)
            {
                // SUPERSEDE: die NEUE Anforderung ist der Ersatz; die alte ist das ersetzte Requirement.
                // MARK_CHANGED: dieselbe Anforderung wurde verfeinert; die alte Fassung steht in der History.
                // EXTEND_PBI (O3a, extend-Modus): eine NEUE Anforderung kommt zum PBI hinzu — keine alte Fassung
                // (oldText = null); der Agent erweitert den PBI-Inhalt, damit er sie mit abdeckt.
                var newReqId = op.Kind == PbiUpdateKind.SupersedePbi ? op.ReplacementRequirementId ?? op.RequirementId : op.RequirementId;
                var newReq = byId.GetValueOrDefault(newReqId);
                var oldText = op.Kind == PbiUpdateKind.SupersedePbi
                    ? byId.GetValueOrDefault(op.RequirementId)?.Text
                    : PreviousText(newReq);
                if (triggers.Any(t => string.Equals(t.RequirementId, newReqId, StringComparison.Ordinal))) continue;
                triggers.Add(new PbiAlignTrigger(newReqId, newReq?.Text ?? newReqId, oldText));
            }
            targets.Add(new PbiAlignTarget(g.Key, pbi.Pbi.Title, pbi.Pbi.Goal, pbi.Pbi.AcceptanceCriteria, triggers));
        }

        // O3b (create): NEW_PBI hat noch KEIN PBI — ein Draft-Ziel je neuer Anforderung, adressiert ueber die
        // Requirement-ID + das Ziel-Feature (Label + Titel der bestehenden Feature-PBIs als Duplikat-Kontext).
        foreach (var op in plan.Operations.Where(o => o.Kind == PbiUpdateKind.NewPbi && o.FeatureId is not null))
        {
            var feature = byId.GetValueOrDefault(op.FeatureId!);
            var newReq = byId.GetValueOrDefault(op.RequirementId);
            var siblings = core.Relations
                .Where(r => string.Equals(r.RelationType, "part_of_feature", StringComparison.Ordinal)
                            && string.Equals(r.ToId, op.FeatureId, StringComparison.Ordinal))
                .Select(r => byId.GetValueOrDefault(r.FromId))
                .Where(i => i is not null && string.Equals(i!.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
                .Select(i => i!.Pbi?.Title ?? i.Text)
                .ToList();
            targets.Add(new PbiAlignTarget(
                PbiId: null, CurrentTitle: null, CurrentStatement: null, CurrentAcceptance: [],
                Triggers: [new PbiAlignTrigger(op.RequirementId, newReq?.Text ?? op.RequirementId, null)],
                TargetRequirementId: op.RequirementId, TargetFeatureId: op.FeatureId,
                FeatureLabel: feature?.Feature?.Label ?? feature?.Text, SiblingPbiTitles: siblings));
        }

        // O4/O5 (create fuer NEW_FEATURE, im isolierten UI-E2E aufgedeckt): das Feature existiert noch NICHT
        // (kein FeatureId, dafuer proposedFeatureLabel). Ohne dieses Draft-Ziel bekaeme der Agent nichts ->
        // 0 Drafts -> O4b skippt jede NEW_FEATURE-Op. TargetFeatureId bleibt null (Feature wird erst angelegt).
        foreach (var op in plan.Operations.Where(o => o.Kind == PbiUpdateKind.NewFeature && !string.IsNullOrWhiteSpace(o.ProposedFeatureLabel)))
        {
            var newReq = byId.GetValueOrDefault(op.RequirementId);
            targets.Add(new PbiAlignTarget(
                PbiId: null, CurrentTitle: null, CurrentStatement: null, CurrentAcceptance: [],
                Triggers: [new PbiAlignTrigger(op.RequirementId, newReq?.Text ?? op.RequirementId, null)],
                TargetRequirementId: op.RequirementId, TargetFeatureId: null,
                FeatureLabel: op.ProposedFeatureLabel, SiblingPbiTitles: []));
        }
        return targets;
    }

    private static string? PreviousText(ProjectStateItem? item)
    {
        var prev = item?.History?.LastOrDefault()?.Text;
        return string.IsNullOrWhiteSpace(prev) || string.Equals(prev, item!.Text, StringComparison.Ordinal) ? null : prev;
    }
}

// Tools des Angleichungs-Agenten. Wie PbiPlacementTools: erkunden (get_alignment_targets) + genau einmal
// speichern (save_alignments). Der Agent schreibt NIE direkt in den Core.
internal sealed class PbiAlignTools(IReadOnlyList<PbiAlignTarget> targets, RunContext run)
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
    private IReadOnlyList<PbiAlignment>? _saved;

    public bool Saved => _saved is not null;
    public IReadOnlyList<PbiAlignment>? SavedAlignments => _saved;

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(GetTargets, "get_alignment_targets",
            "Die betroffenen PBIs (pbiId, aktueller Titel/Statement/Akzeptanzkriterien) plus die ausloesenden "
            + "Anforderungen (requirementId, neueFassung, alteFassung)."),
        AIFunctionFactory.Create(SaveAlignments, "save_alignments",
            "Speichert je Ziel GENAU EINEN Draft (proposedTitle, proposedStatement, proposedAcceptanceCriteria, "
            + "rationale, triggerRequirementIds). Bei bestehendem PBID: pbiId setzen. Bei einem NEUEN PBI "
            + "(zielRequirementId war gesetzt, pbiId war null): pbiId LEER lassen und stattdessen targetRequirementId "
            + "(+ targetFeatureId) setzen. Genau einmal aufrufen."),
    ];

    private string GetTargets()
    {
        var rows = targets.Select(t => new
        {
            pbiId = t.PbiId,
            aktuellerTitel = t.CurrentTitle,
            aktuellesStatement = t.CurrentStatement,
            aktuelleAkzeptanzkriterien = t.CurrentAcceptance,
            // O3b (create): wenn pbiId null ist, ist dies ein NEUES PBI fuer zielRequirementId im zielFeature.
            zielRequirementId = t.TargetRequirementId,
            zielFeatureId = t.TargetFeatureId,
            zielFeatureLabel = t.FeatureLabel,
            bestehendePbisImFeature = t.SiblingPbiTitles,
            geaenderteAnforderungen = t.Triggers.Select(tr => new { requirementId = tr.RequirementId, neueFassung = tr.NewText, alteFassung = tr.OldText })
        }).ToArray();
        run.AppendEvent(new { type = "PBI_ALIGN_TARGETS", runId = run.RunId, returned = rows.Length, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(rows, Json);
    }

    private string SaveAlignments(PbiAlignment[] alignments)
    {
        if (Saved) return "ALREADY_SAVED: save_alignments darf nur einmal aufgerufen werden.";
        _saved = (alignments ?? []).ToList();
        run.AppendEvent(new { type = "PBI_ALIGN_SAVE", runId = run.RunId, alignments = _saved.Count, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new { saved = true, alignments = _saved.Count }, Json);
    }
}
