using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Decision;

/// <summary>D2-Antrag vom pbi-Gate: „das ist nicht meine Entscheidung — Stakeholder nötig" (Frage = Pflicht-Begründung).</summary>
public sealed record PbiDecisionRequest(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("pbiId")] string? PbiId,
    [property: JsonPropertyName("question")] string Question);

// R-14 D2 (r14-entscheidung.md §9/§10): der „→ Entscheidung"-Knopf am pbi-Gate. Deterministische Praegung einer
// OFFENEN Entscheidung aus einem menschlichen Klaerungs-Antrag — dieselbe DEC-Muenze wie der Ingest-CONTRADICT,
// aber BEWUSST OHNE contradicts-Kante (es ist ein Klaerungs-Antrag, kein Meeting-Widerspruch; Aufloesung/Scan
// nutzen den verifizierten targetEntityId-Fallback, der Flip-Schritt uebergeht graceful). Das antragende PBI wird
// geschuetzt geblockt. Beim naechsten Lauf legt das decision-gate den Antrag vor (Herkunft: pbi-Gate).
public static class DecisionRequestMint
{
    public const string Origin = "PBI_CLARIFICATION_REQUEST";

    public static (ProjectStateDocument Core, IReadOnlyList<string> Minted, IReadOnlyList<string> Skipped) Mint(
        ProjectStateDocument core, IReadOnlyList<PbiDecisionRequest> requests, string sourceRun)
    {
        var order = core.Items.Select(i => i.ItemId).ToList();
        var byId = core.Items.ToDictionary(i => i.ItemId, StringComparer.Ordinal);
        var nextDec = MaxSuffix(order) + 1;
        var minted = new List<string>();
        var skipped = new List<string>();

        foreach (var r in requests)
        {
            if (!byId.ContainsKey(r.RequirementId)) { skipped.Add($"{r.RequirementId}: Ziel nicht im Core"); continue; }

            var decId = $"DEC-{nextDec++:D3}";
            var meta = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DecisionTargetMeta.Key] = r.RequirementId,
                ["requestReason"] = r.Question,
            };
            if (!string.IsNullOrWhiteSpace(r.PbiId)) meta["requestedFromPbi"] = r.PbiId!;

            var text = $"Klärungsbedarf zu {r.RequirementId}: {r.Question}";
            var dec = new ProjectStateItem(
                ItemId: decId, ItemType: "decision", Text: text, Origin: Origin, Stage: null, Version: 1,
                SourceRunId: sourceRun, SourceArtifactId: null, SourceArtifactType: null, SourceDecisionId: null,
                SourceCandidateId: null, SourceClaimIds: [], SourceArtifactItemIds: [], Metadata: meta,
                IdentityKey: IdentityKey.From(text), History: []).WithStatus(CoreStatus.From("open_decision"));
            byId[decId] = dec;
            order.Add(decId);
            minted.Add(decId);

            // Das antragende PBI blocken (DEC-Ref + Status) — Schutz bis zur Aufloesung, wie beim Ingest-BLOCK.
            if (!string.IsNullOrWhiteSpace(r.PbiId) && byId.TryGetValue(r.PbiId!, out var pbi) && pbi.Pbi is not null)
            {
                var decRefs = pbi.Pbi.OpenDecisionRefs.Contains(decId)
                    ? pbi.Pbi.OpenDecisionRefs
                    : pbi.Pbi.OpenDecisionRefs.Append(decId).ToList();
                byId[r.PbiId!] = pbi
                    .WithStatus(pbi.ReadStatus().Escalate(Blocker.BlockedByDecision), $"blocked via {decId} (Klärungs-Antrag vom pbi-Gate)")
                    with { Version = pbi.Version + 1, Pbi = pbi.Pbi with { OpenDecisionRefs = decRefs } };
            }
        }

        if (minted.Count == 0) return (core, minted, skipped);
        var items = order.Select(id => byId[id]).ToList();
        return (core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = items }, minted, skipped);
    }

    private static int MaxSuffix(IEnumerable<string> ids)
        => ids.Where(id => id.StartsWith("DEC-", StringComparison.Ordinal))
            .Select(id => id["DEC-".Length..])
            .Where(s => s.Length > 0 && s.All(char.IsDigit))
            .Select(int.Parse).DefaultIfEmpty(0).Max();
}
