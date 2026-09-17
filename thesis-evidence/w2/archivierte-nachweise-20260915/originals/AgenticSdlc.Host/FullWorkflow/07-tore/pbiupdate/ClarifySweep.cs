using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.Run;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.PbiUpdate;

/// <summary>
/// C4a (09.08.2026, c4-klaerungs-sweep-plan §2/§6) — der deterministische KATALOG-COLLECTOR: je
/// needs_clarify-PBI die Lücken-DIAGNOSE aus dem Core (kein LLM, kein Urteil, kein Schreiben). Der Katalog
/// ist das, was der Steward dem Autor im Chat vorlegt (C4c); die Antworten fahren dann über die BESTEHENDE
/// pbi-update-Bahn (C4b). Zweig-agnostisch by construction: liest NUR den Core (§4).
/// </summary>
public static class ClarifySweepCollector
{
    public static ClarifySweepKatalog Collect(ProjectStateDocument core)
    {
        var textById = core.Items.ToDictionary(i => i.ItemId, i => i.Text, StringComparer.Ordinal);
        var entries = core.Items
            .Where(i => string.Equals(i.ItemType, "pbi", StringComparison.OrdinalIgnoreCase)
                        && i.ReadStatus().Blocker == Blocker.NeedsClarify)
            .OrderBy(i => i.ItemId, StringComparer.Ordinal)
            .Select(p =>
            {
                var luecken = new List<string>();
                if (string.IsNullOrWhiteSpace(p.Pbi?.Goal)) luecken.Add("STATEMENT_FEHLT: kein Story-Statement (Ziel/Warum) vorhanden.");
                if (p.Pbi?.AcceptanceCriteria is not { Count: > 0 }) luecken.Add("AK_LEER: keine Akzeptanzkriterien vorhanden.");
                var note = p.History?.LastOrDefault()?.Note;
                if (!string.IsNullOrWhiteSpace(note)) luecken.Add($"HISTORIE: letzte Änderungs-Notiz \"{note}\"");
                if (luecken.Count == 0) luecken.Add("UNSPEZIFISCH: als needs_clarify markiert ohne diagnostizierbare Einzel-Lücke — Klärung frei formulieren.");
                var reqs = (p.Pbi?.LinkedRequirementIds ?? [])
                    .Select(id => new ClarifySweepReqKontext(id, textById.GetValueOrDefault(id, "(nicht im Core)")))
                    .ToList();
                return new ClarifySweepEntry(p.ItemId, p.Pbi?.Title ?? p.Text, p.Origin, luecken, p.Pbi?.Goal, p.Pbi?.AcceptanceCriteria ?? [], reqs);
            })
            .ToList();
        return new ClarifySweepKatalog(entries.Count, entries);
    }
}

public sealed record ClarifySweepReqKontext(
    [property: JsonPropertyName("requirementId")] string RequirementId,
    [property: JsonPropertyName("text")] string Text);

public sealed record ClarifySweepEntry(
    [property: JsonPropertyName("pbiId")] string PbiId,
    [property: JsonPropertyName("titel")] string Titel,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("luecken")] IReadOnlyList<string> Luecken,
    [property: JsonPropertyName("statement")] string? Statement,
    [property: JsonPropertyName("akzeptanzkriterien")] IReadOnlyList<string> Akzeptanzkriterien,
    [property: JsonPropertyName("verlinkteRequirements")] IReadOnlyList<ClarifySweepReqKontext> VerlinkteRequirements);

public sealed record ClarifySweepKatalog(
    [property: JsonPropertyName("offeneKlaerungen")] int OffeneKlaerungen,
    [property: JsonPropertyName("eintraege")] IReadOnlyList<ClarifySweepEntry> Eintraege);

// CLI: clarify-sweep collect  — die Betriebs-Bahn (§3; run --answers = C4b, Steward-Seil = C4c).
public static class ClarifySweepRunner
{
    public static async Task<int> RunAsync(string[] args, Configuration.HostSettings settings, string repoRoot)
    {
        var mode = args.Length > 1 ? args[1].ToLowerInvariant() : "";
        if (mode == "run") return await ClarifySweepAnswersRunner.RunAsync(args, settings, repoRoot).ConfigureAwait(false);
        if (mode != "collect")
        { Console.Error.WriteLine("Usage: clarify-sweep collect | clarify-sweep run --answers <sweep-answers.json>"); return 2; }

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[clarify-sweep] Core fehlt."); return 2; }
        var katalog = ClarifySweepCollector.Collect(await repo.LoadAsync().ConfigureAwait(false));

        var run = new RunContext(RunId.New(), "clarify-sweep");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");
        await File.WriteAllTextAsync(Path.Combine(outDir, "sweep-katalog.json"),
            JsonSerializer.Serialize(katalog, JsonFiles.Json)).ConfigureAwait(false);

        Console.WriteLine($"[clarify-sweep] offene Klaerungen={katalog.OffeneKlaerungen}");
        foreach (var e in katalog.Eintraege.Take(5))
            Console.WriteLine($"[clarify-sweep]   {e.PbiId} — {e.Luecken[0]}");
        if (katalog.OffeneKlaerungen > 5) Console.WriteLine($"[clarify-sweep]   … (+{katalog.OffeneKlaerungen - 5} weitere im Katalog)");
        Console.WriteLine($"[clarify-sweep] -> {Path.GetRelativePath(repoRoot, outDir)}/sweep-katalog.json");
        return 0;
    }
}

/// <summary>C4b — eine Autor-Antwort aus dem Chat (Quelle-Pflicht: „author via steward-chat", §8-Naht).</summary>
public sealed record ClarifySweepAnswer(
    [property: System.Text.Json.Serialization.JsonPropertyName("pbiId")] string PbiId,
    [property: System.Text.Json.Serialization.JsonPropertyName("antwort")] string Antwort,
    [property: System.Text.Json.Serialization.JsonPropertyName("quelle")] string Quelle = "author via steward-chat",
    [property: System.Text.Json.Serialization.JsonPropertyName("sessionName")] string? SessionName = null,
    // C2d §3-4: expliziter Herkunfts-Ref-Override (z. B. `gh-comment:<issue>#<commentId>`) — null = Chat-Form.
    [property: System.Text.Json.Serialization.JsonPropertyName("answerRef")] string? AnswerRef = null);

/// <summary>
/// C4b — deterministischer Plan-Bau aus Antworten: MARK_CHANGED-Ops mit der EIGENEN Herkunfts-Naht
/// (AuthorAnswerRef `chat:&lt;session&gt;#&lt;n&gt;`, §8-Leitplanke: kein Fake-REQ; RequirementId bleibt leer).
/// Validierung LAUT: unbekannte pbiId / leere Antwort / PBI nicht (mehr) needs_clarify ⇒ Skip mit Grund.
/// </summary>
public static class ClarifySweepPlanBuilder
{
    public static (PbiStateChangePlanDocument Plan, IReadOnlyList<PbiAlignTarget> Targets, IReadOnlyList<string> Skipped)
        Build(Delta.ProjectStateDocument core, IReadOnlyList<ClarifySweepAnswer> answers, string runId)
    {
        var byId = core.Items.GroupBy(i => i.ItemId).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        var ops = new List<PbiStateChangeOperation>();
        var targets = new List<PbiAlignTarget>();
        var skipped = new List<string>();
        var n = 0;
        foreach (var a in answers)
        {
            n++;
            var pbi = byId.GetValueOrDefault(a.PbiId);
            if (pbi is null || !string.Equals(pbi.ItemType, "pbi", StringComparison.OrdinalIgnoreCase))
            { skipped.Add($"{a.PbiId}: kein PBI im Core (UNKNOWN_PBI)"); continue; }
            if (string.IsNullOrWhiteSpace(a.Antwort))
            { skipped.Add($"{a.PbiId}: leere Antwort"); continue; }
            if (pbi.ReadStatus().Blocker != Delta.Blocker.NeedsClarify)
            { skipped.Add($"{a.PbiId}: nicht (mehr) needs_clarify — evtl. zwischenzeitlich geklärt (Meeting-Koexistenz §4)"); continue; }

            var answerRef = a.AnswerRef ?? $"chat:{a.SessionName ?? "steward"}#{n}";
            ops.Add(new PbiStateChangeOperation(PbiUpdateKind.MarkChanged, RequirementId: "", PbiId: a.PbiId,
                FeatureId: null, ReplacementRequirementId: null, OpenDecisionRef: null,
                Rationale: $"Klärungs-Sweep: {a.Quelle}", AuthorAnswerRef: answerRef, AuthorAnswerText: a.Antwort.Trim()));
            targets.Add(new PbiAlignTarget(pbi.ItemId, pbi.Pbi?.Title ?? pbi.Text, pbi.Pbi?.Goal,
                pbi.Pbi?.AcceptanceCriteria ?? [],
                [new PbiAlignTrigger(answerRef, a.Antwort.Trim(), null)]));
        }
        var plan = new PbiStateChangePlanDocument(1, $"sweep-{runId}", DateTime.UtcNow, runId, ops, null);
        return (plan, targets, skipped);
    }
}

// C4b — `clarify-sweep run --answers <file>`: Antworten → Plan (det.) → Alignment-Drafting (der EINE
// LLM-Schritt, EXISTIERENDE PbiAlign-Naht: gleiche Tools/Prompt wie R-26-C) → pbi-change-plan.json →
// Übergabe an die UNVERÄNDERTE pbi-update-Review-Bahn (Gate entscheidet, Apply löscht Blocker).
public static class ClarifySweepAnswersRunner
{
    public static async Task<int> RunAsync(string[] args, Configuration.HostSettings settings, string repoRoot)
    {
        string? answersArg = null;
        for (var i = 2; i < args.Length; i++)
            if (string.Equals(args[i], "--answers", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) answersArg = args[++i];
        if (answersArg is null) { Console.Error.WriteLine("[clarify-sweep] --answers <sweep-answers.json> fehlt."); return 2; }
        return await RunFromAnswersAsync(answersArg, settings, repoRoot).ConfigureAwait(false);
    }

    /// <summary>K13-2: typisierte Naht — Sweep aus einer Antworten-Datei, ohne CLI-Args (Steward + CLI-Haut).</summary>
    public static async Task<int> RunFromAnswersAsync(string answersArg, Configuration.HostSettings settings, string repoRoot)
    {
        var answersPath = Path.IsPathRooted(answersArg) ? answersArg : Path.Combine(repoRoot, answersArg);
        if (!File.Exists(answersPath)) { Console.Error.WriteLine($"[clarify-sweep] Antworten nicht gefunden: {answersPath}"); return 2; }

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false)) { Console.Error.WriteLine("[clarify-sweep] Core fehlt."); return 2; }
        var core = await repo.LoadAsync().ConfigureAwait(false);

        // §4 Kollisions-Regel: pausierter pipeline-full-Lauf am pbi-gate ⇒ LAUT warnen statt parallel feuern.
        var paused = await Pipeline.PipelineRunStatusReader.ReadPausedAsync(repoRoot).ConfigureAwait(false);
        foreach (var p in paused.Where(p => string.Equals(p.PausedGate, "pbi-gate", StringComparison.OrdinalIgnoreCase)))
            Console.WriteLine($"[clarify-sweep] ⚠ KOLLISION: Lauf {p.RunId} pausiert am pbi-gate — erst dort entscheiden oder bewusst parallel fahren.");

        var answers = JsonSerializer.Deserialize<List<ClarifySweepAnswer>>(
            await File.ReadAllTextAsync(answersPath).ConfigureAwait(false), JsonFiles.Json) ?? [];

        var run = new RunContext(RunId.New(), "clarify-sweep");
        run.EnsureFolders();
        var outDir = run.OutputDir("plan");

        // A′ Schritt 1 (steward/graph-entry-vs-werkbank.md §9): Plan-Erzeugung + Alignment laufen über den GETEILTEN
        // Kern (ClarifyEntryPlan) mit der GETEILTEN LLM-Naht (PbiAnswerAlignment) — dieselbe Naht nutzt ab Schritt 2
        // der durable Graph-Eingang (Zwei-Bahnen-Regel; keine Kopie). Werkbank-Verhalten unverändert (gleiche Ops/Targets).
        var (final, _, skipped) = await ClarifyEntryPlan.AssembleAsync(
            core, answers, run.RunId, PbiAnswerAlignment.Llm(settings, repoRoot, run)).ConfigureAwait(false);
        foreach (var sk in skipped) Console.WriteLine($"[clarify-sweep] SKIP {sk}");
        if (final.Operations.Count == 0)
        { Console.Error.WriteLine("[clarify-sweep] keine gueltigen Antworten — nichts zu fahren."); return skipped.Count > 0 ? 3 : 2; }
        if (final.Alignments is null or { Count: 0 })
            Console.WriteLine("[clarify-sweep] ⚠ Agent lieferte keine Alignments — Gate zeigt die Antworten trotzdem (needs_clarify bleibt bis edit).");

        await File.WriteAllTextAsync(Path.Combine(outDir, "pbi-change-plan.json"),
            JsonSerializer.Serialize(final, JsonFiles.Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "sweep-answers-used.json"),
            JsonSerializer.Serialize(answers, JsonFiles.Json)).ConfigureAwait(false);

        // C4d (§11/K12): der wartende Vorschlag wird MIT NUTZLAST im Core registriert („Core = System,
        // runs = Chronik" — die Dateien oben sind die Beleg-Kopie). Kangal läuft im SaveAsync mit.
        var payload = JsonSerializer.SerializeToElement(final, JsonFiles.Json);
        var (registered, proposalId) = Core.PendingReviewRegistry.Register(core, "clarify-sweep",
            $"pbi-update-review --pending PEND-{run.RunId}", payload,
            final.Operations.Where(o => o.PbiId is not null).Select(o => o.PbiId!).ToList(), run.RunId);
        await repo.SaveAsync(registered).ConfigureAwait(false);

        Console.WriteLine($"[clarify-sweep] ops={final.Operations.Count} alignments={final.Alignments?.Count ?? 0} skips={skipped.Count} · registriert: {proposalId}");
        Console.WriteLine($"[clarify-sweep] -> {Path.GetRelativePath(repoRoot, outDir)} (Beleg-Kopie)");
        Console.WriteLine($"[clarify-sweep] naechster Schritt (Gate): pbi-update-review --pending {proposalId}");
        return 0;
    }
}
