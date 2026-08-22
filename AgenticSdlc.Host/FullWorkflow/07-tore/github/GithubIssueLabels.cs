using System.Text.RegularExpressions;
using AgenticSdlc.Host.FullWorkflow.Core;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// Projektions-Nachzug ④ (Autor-⚖ 20.08.): Issue-Labels als GEPFLEGTE Status-Projektion statt
/// eingefrorener Schöpfungs-Vermerke. EINE Berechnungs-Quelle für Create UND Update: die
/// System-Label-Familie wird bei jedem Write NEU aus den Status-Achsen des PBIs abgeleitet
/// („gelöst ⇒ Label weg" ist damit automatisch — der Status steht im Body-Footer, jeder
/// Statuswechsel erzeugt einen Hash-Diff und damit den pflegenden UPDATE).
/// R-30 bleibt gewahrt: GitHubs PATCH ersetzt die komplette Liste — deshalb MERGE: Fremd-Labels
/// (von Menschen gesetzt) bleiben erhalten, nur die eigene Familie wird neu gesetzt; Altlasten
/// der Juli-Schöpfungswellen (initial-sync, REQ-nn) werden dabei getilgt.
/// Dokumentierte Grenze: blockierte PBIs sind write-gehalten (HOLD_BLOCKED) — ihr Label-Stand
/// friert bis zum Unblock; der erste Write danach pflegt nach.
/// </summary>
public static class GithubIssueLabels
{
    public const string Pbi = "pbi";
    public const string NeedsClarify = "needs-clarify";
    // Slice S Teil 2 (21.08.): Prio als gepflegtes Familien-Label (prio:high|medium|low aus dem PBI-Feld) —
    // gleiche Mechanik wie needs-clarify: je Write neu berechnet, „geändert/entfernt ⇒ Label folgt" automatisch.
    public const string PrioPrefix = "prio:";
    private const string LegacyInitialSync = "initial-sync";
    private static readonly Regex LegacyReqLabel = new(@"^REQ-\d+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Label-Liste eines Writes: Fremd-Labels erhalten + System-Familie aus dem PBI-Status neu.</summary>
    public static IReadOnlyList<string> For(GithubSyncEntry entry, IEnumerable<string>? existing = null)
    {
        var result = (existing ?? []).Where(l => !IsSystemManaged(l)).ToList();
        result.Add(Pbi);
        if (string.Equals(entry.Status, "needs_clarify", StringComparison.OrdinalIgnoreCase))
            result.Add(NeedsClarify);
        if (PbiFields.NormalizePriority(entry.Priority) is { } prio)
            result.Add(PrioPrefix + prio);
        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool IsSystemManaged(string label)
        => string.Equals(label, Pbi, StringComparison.OrdinalIgnoreCase)
           || string.Equals(label, NeedsClarify, StringComparison.OrdinalIgnoreCase)
           || string.Equals(label, LegacyInitialSync, StringComparison.OrdinalIgnoreCase)
           || label.StartsWith(PrioPrefix, StringComparison.OrdinalIgnoreCase)
           || LegacyReqLabel.IsMatch(label);
}
