using System.Text;
using AgenticSdlc.Host.FullWorkflow.Ledger.Core;

namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// Projiziert die freigegebenen Ledger-Claims lesbar für einen Konsumenten-Agenten (Arm B): id + Facetten +
/// Proposition + die ECHTE Transkript-Evidenz + Notes. Ein <b>gemeinsames</b>, deterministisches Modul, damit
/// der Direkt-Chat-Runner (<see cref="EvidenceAgentRunner"/>) und der MAF-Maker-Executor byte-identisch dieselbe
/// Quelle sehen (Experiment-Kontrolle: die Maker-Eingabe ist über beide Ausführungsformen exakt gleich).
/// </summary>
/// <remarks>
/// Die Evidenz ist v.a. für ADJ-GAP-Claims wichtig, deren Proposition nur eine Meta-Beschreibung ist (der konkrete
/// Inhalt steckt im Zitat). Die Facetten sind die adjudizierte Wahrheit; die id ist die Quellenangabe, die im
/// Artefakt zitiert wird. <paramref name="dispositionKey"/> wählt die Ziel-Artefakt-Disposition (requirements|risks|…).
/// </remarks>
public static class EvidenceLedgerProjection
{
    public static string Project(IReadOnlyList<SemanticLedgerEntry> claims, string dispositionKey)
    {
        var sb = new StringBuilder();
        foreach (var c in claims)
        {
            var disp = c.Disposition is not null && c.Disposition.TryGetValue(dispositionKey, out var d)
                ? d.Applicability : "?";
            sb.Append("- [").Append(c.Id).Append("] ")
              .Append("kind=").Append(c.Kind)
              .Append(" status=").Append(c.Status)
              .Append(" modality=").Append(c.Modality)
              .Append(" timeScope=").Append(c.TimeScope ?? "?")
              .Append(' ').Append(dispositionKey).Append('=').Append(disp).AppendLine();
            sb.Append("  proposition: ").AppendLine(c.Proposition);

            var quotes = (c.Evidence ?? [])
                .Select(e => e.Quote).Where(q => !string.IsNullOrWhiteSpace(q)).ToList();
            if (quotes.Count > 0)
                sb.Append("  evidence: ").AppendLine(string.Join(" | ", quotes));
            if (!string.IsNullOrWhiteSpace(c.Notes))
                sb.Append("  notes: ").AppendLine(c.Notes);
        }
        return sb.ToString();
    }
}
