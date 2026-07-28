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
public sealed record PbiAlignTarget(
    string PbiId, string? CurrentTitle, string? CurrentStatement, IReadOnlyList<string> CurrentAcceptance,
    IReadOnlyList<PbiAlignTrigger> Triggers);

public sealed record PbiAlignTrigger(string RequirementId, string NewText, string? OldText);

// Deterministisch (kein LLM, testbar): welche PBIs brauchen eine Angleichung + mit welchem Kontext.
public static class PbiAlignTargets
{
    public static IReadOnlyList<PbiAlignTarget> Collect(PbiStateChangePlanDocument plan, ProjectStateDocument core)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var targets = new List<PbiAlignTarget>();
        foreach (var g in plan.Operations
                     .Where(o => (o.Kind == PbiUpdateKind.MarkChanged || o.Kind == PbiUpdateKind.SupersedePbi) && o.PbiId is not null)
                     .GroupBy(o => o.PbiId!, StringComparer.Ordinal))
        {
            if (!byId.TryGetValue(g.Key, out var pbi) || pbi.Pbi is null) continue;
            var triggers = new List<PbiAlignTrigger>();
            foreach (var op in g)
            {
                // SUPERSEDE: die NEUE Anforderung ist der Ersatz; die alte ist das ersetzte Requirement.
                // MARK_CHANGED: dieselbe Anforderung wurde verfeinert; die alte Fassung steht in der History.
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
            "Speichert je PBI GENAU EINE Angleichung (pbiId, proposedTitle, proposedStatement, "
            + "proposedAcceptanceCriteria, rationale, triggerRequirementIds). Genau einmal aufrufen."),
    ];

    private string GetTargets()
    {
        var rows = targets.Select(t => new
        {
            pbiId = t.PbiId,
            aktuellerTitel = t.CurrentTitle,
            aktuellesStatement = t.CurrentStatement,
            aktuelleAkzeptanzkriterien = t.CurrentAcceptance,
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
