using System.Text;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;
using AgenticSdlc.Host.Run;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3;

/// <summary>
/// Optionales, read-only Agent-Tool für die L3-Agenten: <c>resolve_provenance(itemId)</c> verfolgt die HERKUNFT eines
/// Umwelt-Items rückwärts durch die id-verknüpfte Kette. So kann der Generator/Resolver — WENN er will — nachsehen,
/// „woher kommt eigentlich dieses Requirement", bevor er daran anknüpft (User-Wunsch).
/// </summary>
/// <remarks>
/// Bewusst gegen die In-Memory-Abstraktion <see cref="SourceArtifactSet"/> (id-Lookup) gebaut, NICHT gegen Dateipfade:
/// wird die Umwelt später eine Datenbank, tauscht man nur den Loader, der <c>SourceArtifactSet</c> füllt — dieses Tool
/// bleibt unverändert. Die Kette ist rein id-basiert (<c>sourceArtifactItemIds</c> → Upstream-Item;
/// <c>sourceClaimIds</c> → Ledger-Claim → Evidenz), also durchläuft der Walk genau diese Verweise.
///
/// Philosophie wie <see cref="DerivationTools"/> (IST_Soll09.7): der Agent arbeitet mit fachlichen Identitäten
/// (itemId), nicht mit Pfaden, und das Tool ist READ-ONLY — es ERWEITERT den Evidenzraum NICHT, es ERKLÄRT nur
/// vorhandene Items. Mess-Hygiene: das Tool ist config-gated (Default aus), damit die eingefrorenen Gen-Baselines
/// (ohne Tool) reproduzierbar bleiben.
/// </remarks>
internal sealed class L3ProvenanceTool
{
    // Zyklen sind durch das visited-Set ausgeschlossen; die Tiefe begrenzt nur die Ausgabegröße bei langen Ketten.
    private const int MaxDepth = 5;

    private readonly IReadOnlyDictionary<string, ArtifactItem> _byId;
    private readonly IReadOnlyDictionary<string, LedgerClaim> _claims;
    private readonly RunContext _run;

    public L3ProvenanceTool(SourceArtifactSet env, IReadOnlyDictionary<string, LedgerClaim> claims, RunContext run)
    {
        _byId = env.ItemsById();
        _claims = claims;
        _run = run;
    }

    public IReadOnlyList<AITool> Build() =>
    [
        AIFunctionFactory.Create(ResolveProvenance, "resolve_provenance",
            "Verfolgt die HERKUNFT eines Umwelt-Items rückwärts durch die id-verknüpfte Kette: abgeleitete Items → ihre "
            + "Upstream-Items (sourceArtifactItemIds), extrahierte Items → ihre Ledger-Claims (sourceClaimIds, die "
            + "Evidenz). Nutze es, wenn du wissen willst, WORAUS ein bestehendes Item stammt, bevor du daran anknüpfst. "
            + "Read-only — es erweitert den Evidenzraum NICHT."),
    ];

    /// <summary>Löst die Herkunft von <paramref name="itemId"/> auf: rekursiver Rückwärts-Walk über die geladene Umwelt
    /// (+ optional den Ledger-Claim-Index). Verweise außerhalb der geladenen Umwelt werden ehrlich als solche markiert
    /// (später über die DB auflösbar).</summary>
    private string ResolveProvenance(string itemId)
    {
        var id = itemId?.Trim() ?? string.Empty;
        _run.AppendEvent(new { type = "L3_TOOL_RESOLVE_PROVENANCE", runId = _run.RunId, itemId = id, ledgerLoaded = _claims.Count > 0, timestampUtc = DateTime.UtcNow });
        if (!_byId.ContainsKey(id)) return $"UNBEKANNT: {id} ist nicht in der geladenen Umwelt.";

        var sb = new StringBuilder();
        sb.AppendLine($"Herkunft von {id}:");
        Walk(id, 0, new HashSet<string>(StringComparer.Ordinal), sb);
        return sb.ToString();
    }

    private void Walk(string id, int depth, HashSet<string> visited, StringBuilder sb)
    {
        var indent = new string(' ', depth * 2);
        if (!visited.Add(id)) { sb.AppendLine($"{indent}- {id}: (bereits aufgelöst — Zyklus vermieden)"); return; }
        if (!_byId.TryGetValue(id, out var item))
        {
            sb.AppendLine($"{indent}- {id}: (nicht in der geladenen Umwelt — später über die DB/den vollen Graphen auflösbar)");
            return;
        }

        sb.AppendLine($"{indent}- {id} [{item.Origin}]: {Trim(item.Text)}");
        if (depth >= MaxDepth) { sb.AppendLine($"{indent}  … (max. Tiefe erreicht — weiter oben abgeschnitten)"); return; }

        var upstream = item.SourceArtifactItemIds ?? [];
        var claims = item.SourceClaimIds ?? [];

        if (upstream.Count > 0)
        {
            sb.AppendLine($"{indent}  abgeleitet aus {upstream.Count} Upstream-Item(s):");
            foreach (var u in upstream) Walk(u, depth + 1, visited, sb);
        }
        if (claims.Count > 0)
        {
            sb.AppendLine($"{indent}  extrahiert aus {claims.Count} Ledger-Claim(s) (die Evidenz):");
            foreach (var cid in claims)
                sb.AppendLine(_claims.TryGetValue(cid, out var c)
                    ? $"{indent}    · {cid} [{c.Kind}/{c.Status}]: {Trim(c.Proposition)}"
                    : $"{indent}    · {cid}: (Claim-Text nicht geladen — nur die id; Ledger via l3.ledgerRun setzen)");
        }
        if (upstream.Count == 0 && claims.Count == 0)
            sb.AppendLine($"{indent}  (keine Herkunftsangaben — Wurzel/manuell erfasst)");
    }

    private static string Trim(string s) => s.Length <= 160 ? s : s[..157] + "…";
}
