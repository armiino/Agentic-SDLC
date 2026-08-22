using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Analyst;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Core;

/// <summary>
/// Autor-Artefakte, Endform (⚖ Autor 21.08. „Rezepte raus aus dem Steward-Prompt"): der ENTWURF kommt von
/// einem eigenen Drafting-Agenten — Haus-Konvention (PbiAlignmentAgent/Linsen-Muster): EIN Agent, je Art
/// eine eigene, versionierte Prompt-Datei (`ArtifactDraftAgent/{Vision,Personas,Glossar,C4}1.txt`). Der
/// Steward ist nur Orchestrator (Tool-Aufruf ⚿ → Vorlage → Iteration per hinweise → Freigabe-Save ⚿).
/// GETEILTE Naht (Zwei-Bahnen-Vorsorge): der geparkte C4-Auto-Knoten im Graph ruft später DIESELBE
/// DraftAsync-Naht. Input ist deterministisch eingesammelt (Digest + Art-Spezifika + aktueller Stand +
/// Autor-Hinweise); jeder Entwurf hinterlässt einen Beleg-Lauf (`runs/artifact-draft/&lt;id&gt;/`).
/// </summary>
public static class AuthoredDocDrafting
{
    public const string AgentName = "ArtifactDraftAgent";

    public static string PromptNameFor(string artKey) => artKey switch
    {
        "vision" => "Vision1",
        "personas" => "Personas1",
        "glossar" => "Glossar1",
        "c4" => "C41",
        _ => throw new ArgumentException($"Unbekannte Artefakt-Art '{artKey}'.", nameof(artKey)),
    };

    public static async Task<(string Draft, string RunId)> DraftAsync(
        HostSettings settings, string repoRoot, string artKey, string? hinweise = null,
        Func<IReadOnlyList<AITool>, AIAgent>? agentFactory = null, CancellationToken ct = default)
    {
        var art = AuthoredDocument.Resolve(artKey)
                  ?? throw new ArgumentException($"Unbekannte Artefakt-Art '{artKey}'.", nameof(artKey));
        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
            throw new InvalidOperationException("Core fehlt (state/core) — kein Entwurf ohne Projektwahrheit.");
        var core = await repo.LoadAsync().ConfigureAwait(false);

        var run = new RunContext(RunId.New(), "artifact-draft");
        run.EnsureFolders();
        var outDir = run.OutputDir("draft");
        var input = BuildInput(repoRoot, core, art, hinweise);
        await File.WriteAllTextAsync(Path.Combine(outDir, "input.md"), input, ct).ConfigureAwait(false);

        var factory = agentFactory ?? Pipeline.PipelineAgents.Factory(repoRoot, settings, settings, run, AgentName, PromptNameFor(art.Key));
        var agent = factory([]);   // Single-Shot-Maker ohne Tools: Input trägt alles, Beleg = input.md/draft.md
        var response = await agent.RunAsync([new ChatMessage(ChatRole.User, input)], cancellationToken: ct).ConfigureAwait(false);
        var draft = response.Text?.Trim() ?? "";
        if (draft.Length == 0) throw new InvalidOperationException($"Drafting-Agent lieferte keinen Entwurf ({art.Key}).");

        await File.WriteAllTextAsync(Path.Combine(outDir, "draft.md"), draft, ct).ConfigureAwait(false);
        run.AppendEvent(new { type = "ARTIFACT_DRAFT", runId = run.RunId, art = art.Key,
            update = AuthoredDocument.Read(repoRoot, art.Key) is not null, hinweise = hinweise ?? "", timestampUtc = DateTime.UtcNow });
        return (draft, run.RunId);
    }

    /// <summary>Frische-Notiz (Autor-Frage 21.08. „vielleicht weiß ich nicht, dass ein neues ADR da ist"):
    /// deterministischer Abgleich Artefakt gegen Wahrheit — welche aktiven ARCH-Items/ADR-Dateien erwähnt
    /// das freigegebene C4 NIRGENDS (weder als Kasten-Beleg noch in den Lücken)? null = kein C4 (kein
    /// Genörgel vor der Erst-Erstellung) oder alles berücksichtigt. Konsument: get_core_overview
    /// (Steward-Lage) — der Steward bietet dann das Update aktiv an. Vorstufe des geparkten Auto-Knotens.</summary>
    public static string? C4FrischeNotiz(string repoRoot, ProjectStateDocument core)
    {
        var content = AuthoredDocument.Read(repoRoot, "c4");
        if (content is null) return null;

        // R-71 (22.08., „30 Belege"-Rauschen): NIE den Voll-Bestand gegen das Diagramm halten — welche
        // Rahmen einen Kasten verdienen, ist DEUTUNG. Gemessen wird das DELTA zum Beleg-Stand-Stempel
        // (was der Zeichner beim letzten Autor-Save gesehen hat): nur NEUES seit dem Stand wird gemeldet.
        var stand = C4GapSection.ReadBelegStand(content);
        var aktuell = C4GapSection.AktiveBelege(repoRoot, core);
        var neu = stand is null ? new List<string>() : aktuell.Where(b => !stand.Contains(b)).OrderBy(x => x, StringComparer.Ordinal).ToList();

        // ⑤ beidseitig: zitierte Belege, die nicht mehr aktive Wahrheit sind (abgelöst/zurückgezogen).
        var tot = core.Items
            .Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Validity != Validity.Active
                        && content.Contains(i.ItemId, StringComparison.OrdinalIgnoreCase))
            .Select(i => i.ItemId)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();

        var teile = new List<string>();
        if (stand is null)
            teile.Add("trägt noch keinen Beleg-Stand — das nächste Autor-Update (draft→Freigabe) stempelt ihn; danach meldet die Frische nur Neues");
        else if (neu.Count > 0)
            teile.Add($"{neu.Count} NEUE Architektur-Beleg(e) seit dem letzten C4-Stand: {string.Join(", ", neu)}");
        if (tot.Count > 0)
            teile.Add($"zitiert {tot.Count} ABGELÖSTE Beleg(e): {string.Join(", ", tot)}");
        if (teile.Count == 0) return null;
        return $"C4 (docs/c4.md) {string.Join(" · ", teile)} — Update anbieten (draft_authored_doc('c4')).";
    }

    /// <summary>Deterministischer Input: Auftrag + aktueller Stand (Update-Fall) + Autor-Hinweise +
    /// Digest der aktiven Wahrheit; für C4 zusätzlich die vollen ARCH-Texte + ADR-Dateien (Beleg-Pflicht).</summary>
    internal static string BuildInput(string repoRoot, ProjectStateDocument core, AuthoredDocument.Art art, string? hinweise)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"ARTEFAKT: {art.Titel} (art={art.Key}, Ziel-Datei {art.RelPath})");

        var current = AuthoredDocument.Read(repoRoot, art.Key);
        sb.AppendLine().AppendLine(current is null
            ? "== AKTUELLER STAND == (noch keiner — Erst-Entwurf)"
            : "== AKTUELLER STAND (UPDATE-Fall: Unveraendertes woertlich uebernehmen, nur Belegtes aendern) ==");
        if (current is not null) sb.AppendLine(current);

        if (!string.IsNullOrWhiteSpace(hinweise))
            sb.AppendLine().AppendLine("== AUTOR-HINWEISE (verbindlich) ==").AppendLine(hinweise.Trim());

        if (string.Equals(art.Key, "c4", StringComparison.Ordinal))
        {
            sb.AppendLine().AppendLine("== ARCHITEKTUR-WAHRHEIT (volle Texte — Beleg-Pflicht je Kasten) ==");
            foreach (var i in core.Items.Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase)
                                                    && i.ReadStatus().Validity == Validity.Active))
                sb.AppendLine($"{i.ItemId}: {i.Text}");
            // ⑤ (Autor-Review „Kanten sind die weichste Stelle"): die WAHRHEITS-KANTEN als Grundlage —
            // damit Verbindungen BELEGT statt nur erschlossen werden koennen.
            sb.AppendLine().AppendLine("== WAHRHEITS-KANTEN (constrained_by: PBI→Rahmen · covers: PBI→Architektur-Arbeit) ==");
            var titles = core.Items.ToDictionary(i => i.ItemId, i => i.Pbi?.Title ?? i.Text, StringComparer.Ordinal);
            var archIds = core.Items.Where(i => string.Equals(i.ItemType, "architecture", StringComparison.OrdinalIgnoreCase))
                .Select(i => i.ItemId).ToHashSet(StringComparer.Ordinal);
            foreach (var r in core.Relations.Where(r =>
                         string.Equals(r.RelationType, "constrained_by", StringComparison.Ordinal)
                         || (string.Equals(r.RelationType, "covers", StringComparison.Ordinal) && archIds.Contains(r.ToId))))
                sb.AppendLine($"{r.FromId} ({Trunc(titles.GetValueOrDefault(r.FromId, "?"), 60)}) —{r.RelationType}→ {r.ToId}");
            var adrDir = Path.Combine(repoRoot, "docs", "adr");
            if (Directory.Exists(adrDir))
                foreach (var f in Directory.EnumerateFiles(adrDir, "*.md").OrderBy(x => x, StringComparer.Ordinal)
                             .Where(f => !Path.GetFileName(f).Equals("README.md", StringComparison.OrdinalIgnoreCase)))
                    sb.AppendLine().AppendLine($"--- ADR {Path.GetFileName(f)} ---").AppendLine(File.ReadAllText(f));
        }

        sb.AppendLine().AppendLine("== AKTIVE WAHRHEIT (Digest, eine Zeile je Item) ==").AppendLine(AnalystCollect.Digest(core));
        return sb.ToString();
    }

    private static string Trunc(string s, int max) => s.Length <= max ? s : s[..max] + "…";
}
