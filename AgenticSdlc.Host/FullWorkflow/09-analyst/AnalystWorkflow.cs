using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B (core-analyst-design §5.0/§7): der Analyse-Graph — deterministischer MAF-Workflow, dessen
/// Maker-Knoten Agenten sind. Offizielles Concurrent-Muster (diverse Perspektiven): Fan-out auf die
/// Linsen (Kanten-Prädikate je Lens-Key, Muster BaselineFanOut), Fan-in-BARRIER sammelt alle, Merge/Dedup
/// deterministisch, Kritiker als Substanz-Filter, Persist schreibt Report + Delta. KEIN Gate — der Lauf
/// mutiert nichts; Wahrheits-Wirkung nur über den getrennten Tor-Lauf (--from-delta).
/// State-Isolation (§7.3-Guidance): Graph wird JE LAUF gebaut — Core/Vorgänger-Keys reisen per KONSTRUKTOR
/// in die Executors dieses einen Laufs, nie über statischen Zustand.
/// </summary>
public static class AnalystWorkflow
{
    /// <summary>Workflow-Eingang: reiner Auslöser — alles Nötige trägt der je Lauf gebaute Graph selbst.</summary>
    public sealed record Trigger;

    public static Workflow Build(
        ProjectStateDocument core, IReadOnlySet<string> vorgaengerKeys,
        Func<IReadOnlyList<AITool>, AIAgent> lensFactory,
        Func<IReadOnlyList<AITool>, AIAgent> kritikerFactory,
        RunContext run, string outDir,
        // Slice S ①: freigegebene Personas (Autor-Artefakt) — vorhanden ⇒ fünfte Linse „Persona-Abdeckung".
        string? personasContent = null)
    {
        var lensDefs = AnalystLenses.For(personasContent);
        var dispatch = new AnalystDispatchExecutor(core, run, lensDefs, personasContent ?? "");
        var lenses = lensDefs.Select(l => new AnalystLensExecutor(l, core, lensFactory, run)).ToList();
        var merge = new AnalystMergeExecutor(core, vorgaengerKeys, run, lenses.Count);
        var kritiker = new AnalystKritikerExecutor(kritikerFactory, run);
        var persist = new AnalystPersistExecutor(run, outDir);

        var builder = new WorkflowBuilder(dispatch)
            .WithName("CoreAnalysis")
            .WithDescription("Collect ─AnalystWork je Linse─► [4 Linsen-Maker] ─barrier─► Merge/Dedup ─► Kritiker ─► Report+Delta");
        foreach (var lens in lenses)
        {
            var key = lens.Lens.Key;
            builder.AddEdge<AnalystWork>(dispatch, lens, m => m is not null && string.Equals(m.Lens.Key, key, StringComparison.Ordinal));
        }
        builder.AddFanInBarrierEdge([.. lenses.Select(l => (ExecutorBinding)l)], merge);
        builder.AddEdge(merge, kritiker);
        builder.AddEdge(kritiker, persist);
        builder.WithOutputFrom(persist);
        return builder.Build();
    }
}

// ── Dispatch = die Collect-Stufe (§5.0 ①, LLM-frei): Digest + Kollektor-Funde, dann Fan-out ──

[SendsMessage(typeof(AnalystWork))]
internal sealed class AnalystDispatchExecutor(ProjectStateDocument core, RunContext run,
    IReadOnlyList<AnalystLens> lenses, string personaKontext)
    : Executor<AnalystWorkflow.Trigger>("AnalystDispatch")
{
    public override async ValueTask HandleAsync(AnalystWorkflow.Trigger _, IWorkflowContext context, CancellationToken ct = default)
    {
        var digest = AnalystCollect.Digest(core);
        var kollektor = AnalystCollect.KollektorFunde(core);
        run.AppendEvent(new { type = "ANALYST_START", runId = run.RunId, linsen = lenses.Count,
            digestZeilen = digest.Count(c => c == '\n'), timestampUtc = DateTime.UtcNow });
        foreach (var lens in lenses)
            await context.SendMessageAsync(new AnalystWork(lens, digest, kollektor,
                Kontext: string.Equals(lens.Key, AnalystLenses.Persona.Key, StringComparison.Ordinal) ? personaKontext : ""))
                .ConfigureAwait(false);
    }
}

// ── Linse: EIN Maker-Agent mit check-vor-save-Werkzeugkasten (Ebene-1-Form-Checker im Loop) ──

[SendsMessage(typeof(AnalystLensResult))]
internal sealed class AnalystLensExecutor(AnalystLens lens, ProjectStateDocument core,
    Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<AnalystWork>($"Lens-{lens.Key}")
{
    public AnalystLens Lens { get; } = lens;

    public override async ValueTask HandleAsync(AnalystWork work, IWorkflowContext context, CancellationToken ct = default)
    {
        var tools = new AnalystLensTools(work.Lens, core);
        var agent = agentFactory(tools.Build());
        var sb = new StringBuilder()
            .AppendLine($"DEINE LINSE: {work.Lens.Titel}")
            .AppendLine($"CHECKLISTE: {work.Lens.Checkliste}");
        // Slice S ①: linsen-eigenes Material (Persona-Abdeckung: die freigegebenen Personas).
        if (work.Kontext.Length > 0)
            sb.AppendLine().AppendLine("== FREIGEGEBENE PERSONAS (Abdeckungs-Grundlage — nur 'Belegt'-Zonen zaehlen) ==").AppendLine(work.Kontext);
        var auftrag = sb
            .AppendLine().AppendLine("== BERECHNETE LUECKEN-KANDIDATEN (deterministisch) ==").AppendLine(work.KollektorFunde)
            .AppendLine("== AKTIVE WAHRHEIT (Digest — Details via get_core_item/search_core) ==").AppendLine(work.Digest)
            .ToString();
        await agent.RunAsync([new ChatMessage(ChatRole.User, auftrag)], cancellationToken: ct).ConfigureAwait(false);

        var findings = tools.Saved ?? [];
        run.AppendEvent(new { type = "ANALYST_LENS_DONE", runId = run.RunId, linse = work.Lens.Key,
            funde = findings.Count, checkRounds = tools.CheckRounds, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AnalystLensResult(work.Lens.Key, findings)).ConfigureAwait(false);
    }
}

// ── Merge/Dedup: deterministisch gegen Linsen-Dubletten, Core, Entscheidungen, R-35-Ablehnungen ──

[SendsMessage(typeof(AnalystMerged))]
internal sealed class AnalystMergeExecutor(ProjectStateDocument core, IReadOnlySet<string> vorgaengerKeys,
    RunContext run, int expected) : Executor<AnalystLensResult>("AnalystMerge")
{
    private readonly List<AnalystLensResult> _results = [];
    private readonly object _gate = new();   // Barrier kann im selben Superstep nebenläufig zustellen (Beleg 5b745e)

    public override async ValueTask HandleAsync(AnalystLensResult result, IWorkflowContext context, CancellationToken ct = default)
    {
        bool complete;
        lock (_gate) { _results.Add(result); complete = _results.Count == expected; }
        if (!complete) return;

        var (coreKeys, decKeys, rejKeys) = AnalystCollect.GedaechtnisKeys(core);
        var kandidaten = new List<AnalystFinding>();
        var vorentschieden = new List<AnalystReportEintrag>();
        var gesehen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var f in _results.OrderBy(r => r.LensKey, StringComparer.Ordinal).SelectMany(r => r.Findings))
        {
            var key = IdentityKey.From(f.Statement);
            var status = !gesehen.Add(key) ? AnalystStatus.DedupLinsen
                : coreKeys.Contains(key) ? AnalystStatus.DedupCore
                : decKeys.Contains(key) ? AnalystStatus.DedupDecision
                : rejKeys.Contains(key) ? AnalystStatus.DedupRejection
                : null!;
            if (status is null) kandidaten.Add(f);
            else vorentschieden.Add(new AnalystReportEintrag(f, status, "deterministischer Abgleich (Fan-in)"));
        }

        run.AppendEvent(new { type = "ANALYST_MERGED", runId = run.RunId, kandidaten = kandidaten.Count,
            dedup = vorentschieden.Count, timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AnalystMerged(kandidaten, vorentschieden, vorgaengerKeys,
            RequirementsDocumentProjection.Fingerprint(core))).ConfigureAwait(false);
    }
}

// ── Kritiker: Substanz-Filter (Präzedenz Cluster-Kritiker) — Aussortiertes bleibt SICHTBAR ──

[SendsMessage(typeof(AnalystJudged))]
internal sealed class AnalystKritikerExecutor(Func<IReadOnlyList<AITool>, AIAgent> agentFactory, RunContext run)
    : Executor<AnalystMerged>("AnalystKritiker")
{
    public override async ValueTask HandleAsync(AnalystMerged merged, IWorkflowContext context, CancellationToken ct = default)
    {
        var eintraege = new List<AnalystReportEintrag>(merged.Vorentschieden);
        var verdicts = new Dictionary<int, AnalystVerdict>();
        if (merged.Kandidaten.Count > 0)
        {
            var tools = new AnalystKritikerTools(merged.Kandidaten.Count);
            var agent = agentFactory(tools.Build());
            var vorlage = string.Join("\n", merged.Kandidaten.Select((f, i) =>
                $"[{i}] ({f.Linse}, {f.Kategorie}) {f.Statement}\n    Herleitung: {f.Herleitung} (Anker: {string.Join(",", f.AnkerIds)})"));
            await agent.RunAsync([new ChatMessage(ChatRole.User, vorlage)], cancellationToken: ct).ConfigureAwait(false);
            foreach (var v in tools.Saved ?? []) verdicts[v.Index] = v;
        }

        for (var i = 0; i < merged.Kandidaten.Count; i++)
        {
            var f = merged.Kandidaten[i];
            var v = verdicts.GetValueOrDefault(i);
            if (v is { Behalten: false })
            { eintraege.Add(new AnalystReportEintrag(f, AnalystStatus.KritikerAussortiert, v.Grund)); continue; }
            // Kritiker-Ausfall = fail-open MIT Vermerk (nie still verlieren) — behalten, der Mensch entscheidet an Tor 1.
            var neuheit = merged.VorgaengerKeys.Contains(IdentityKey.From(f.Statement))
                ? AnalystStatus.WeiterhinOffen : AnalystStatus.Neu;
            eintraege.Add(new AnalystReportEintrag(f, neuheit, v is null ? "Kritiker ohne Urteil — fail-open behalten" : null));
        }

        run.AppendEvent(new { type = "ANALYST_JUDGED", runId = run.RunId,
            behalten = eintraege.Count(e => e.Status is AnalystStatus.Neu or AnalystStatus.WeiterhinOffen),
            aussortiert = eintraege.Count(e => e.Status == AnalystStatus.KritikerAussortiert), timestampUtc = DateTime.UtcNow });
        await context.SendMessageAsync(new AnalystJudged(eintraege, merged.CoreFingerprint)).ConfigureAwait(false);
    }
}

// ── Persist: Report (json + lesbares md) + Delta (AF-Vertrag) — die Artefakte SIND das Ergebnis ──

internal sealed class AnalystPersistExecutor(RunContext run, string outDir) : Executor<AnalystJudged, string>("AnalystPersist")
{
    public override async ValueTask<string> HandleAsync(AnalystJudged judged, IWorkflowContext context, CancellationToken ct = default)
    {
        Directory.CreateDirectory(outDir);
        var report = new AnalystReport(run.RunId, DateTime.UtcNow, judged.CoreFingerprint, judged.Eintraege);
        await File.WriteAllTextAsync(Path.Combine(outDir, "report.json"),
            JsonSerializer.Serialize(report, JsonFiles.Json), ct).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "report.md"), RenderReport(report), ct).ConfigureAwait(false);

        var insDelta = report.InsDelta;
        string? deltaPath = null;
        if (insDelta.Count > 0)
        {
            deltaPath = Path.Combine(outDir, "delta.json");
            var delta = AnalystDeltaBuilder.Build([.. insDelta.Select(e => e.Fund)], run.RunId);
            await File.WriteAllTextAsync(deltaPath, JsonSerializer.Serialize(delta, ProjectStateJson.Options), ct).ConfigureAwait(false);
        }

        run.AppendEvent(new { type = "ANALYST_DONE", runId = run.RunId, insDelta = insDelta.Count,
            aussortiert = report.Eintraege.Count - insDelta.Count, deltaPath, timestampUtc = DateTime.UtcNow });
        return JsonSerializer.Serialize(new
        { runId = run.RunId, insDelta = insDelta.Count, report = Path.Combine(outDir, "report.md"), deltaPath }, JsonFiles.Json);
    }

    internal static string RenderReport(AnalystReport report)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Analyse-Report — Lauf {report.RunId}");
        sb.AppendLine();
        sb.AppendLine($"> Stand: {report.GeneratedUtc:yyyy-MM-dd HH:mm} UTC · Core-Fingerabdruck: {report.CoreFingerprint}");
        sb.AppendLine("> Funde sind HYPOTHESEN — Wahrheit werden sie erst nach deiner Freigabe an Tor 1 (Delta → run_pipeline_from_delta).");
        void Block(string titel, IEnumerable<AnalystReportEintrag> es)
        {
            var list = es.ToList();
            sb.AppendLine().AppendLine($"## {titel} ({list.Count})").AppendLine();
            if (list.Count == 0) { sb.AppendLine("_(keine)_"); return; }
            foreach (var e in list)
            {
                sb.AppendLine($"- **[{e.Fund.Linse} · {e.Fund.Kategorie} · {e.Fund.Disposition}]** {e.Fund.Statement}");
                sb.AppendLine($"  - Herleitung: {e.Fund.Herleitung} _(Anker: {string.Join(", ", e.Fund.AnkerIds)})_");
                if (e.Grund is { Length: > 0 }) sb.AppendLine($"  - Vermerk: {e.Grund}");
            }
        }
        Block("NEU", report.Eintraege.Where(e => e.Status == AnalystStatus.Neu));
        Block("WEITERHIN OFFEN (bereits im letzten Report)", report.Eintraege.Where(e => e.Status == AnalystStatus.WeiterhinOffen));
        Block("Aussortiert (Kritiker — kein stiller Cap)", report.Eintraege.Where(e => e.Status == AnalystStatus.KritikerAussortiert));
        Block("Bereits bekannt (deterministischer Abgleich: Core/Entscheidung/Ablehnung/Linsen-Dublette)",
            report.Eintraege.Where(e => e.Status.StartsWith("dedup_", StringComparison.Ordinal)));
        return sb.ToString();
    }
}
