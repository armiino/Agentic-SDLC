using System.Text;
using System.Text.Json;
using AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation;

/// <summary>
/// AGENTISCHER Derivationsmodus (Stufe 1, IST_Soll09.7): EIN Executor-Knoten, in dem ein echter <see cref="AIAgent"/>
/// mit schmalen Tools SELBST arbeitet — er entscheidet, welche Quell-Items er via <c>get_baseline_items</c> liest,
/// verankert (optional per <c>check_anchor</c>) und schreibt sein <c>derived.json</c> selbst per <c>save_derived</c>.
/// Der Host stellt nur die Umwelt (gescopte Tools) und prüft DANACH unabhängig — er diktiert nicht.
/// </summary>
/// <remarks>
/// Kontrast zum strukturierten Modus (<see cref="DerivationGenerateExecutor"/> → Anchor → Check): dort füttert der
/// Host die Items rein und schreibt das Ergebnis; hier holt und schreibt der Agent selbst. Nach dem Agentenlauf liest
/// dieser Executor das vom Agenten geschriebene <c>derived.json</c> ZURÜCK und lässt die UNABHÄNGIGE Assurance laufen:
/// (a) deterministische Anker-Validierung (zeigen die Anker auf echte Quell-IDs?), (b) den bounded Inference-Check.
/// Er RE-SCHREIBT <c>derived.json</c> NICHT (Stufe 1: der Agent besitzt das Artefakt); die Prüfergebnisse sind
/// separate Reports. Provenienz: <c>retrievedItemIds</c> (alles Betrachtete) vs. die tatsächlich verankerten
/// <c>sourceArtifactItemIds</c>.
/// </remarks>
[YieldsOutput(typeof(DerivationResult))]
internal sealed class DerivationAgenticExecutor : Executor<SourceArtifactSet>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private static readonly JsonSerializerOptions Read = new(JsonSerializerDefaults.Web);

    private readonly Func<IReadOnlyList<AITool>, AIAgent> _agentFactory;
    private readonly InferenceChecker _checker;         // In-Loop-Judge (verify_derived, Selbst-Prüfwerkzeug des Agenten)
    private readonly InferenceChecker _postHocChecker;  // UNABHÄNGIGE Nach-Prüfung (Authorität; = _checker wenn kein --posthoc-judge)
    private readonly string _postHocJudgeModel;
    private readonly bool _independentPostHoc;
    private readonly DerivationSpec _spec;
    private readonly string _model;
    private readonly RunContext _run;
    private readonly string _outDir;
    private readonly IReadOnlyDictionary<string, LedgerClaim> _ledgerClaims;
    private readonly bool _explorer;
    private readonly bool _verify;
    private readonly bool _accountable;
    private readonly bool _accountVerify;
    private readonly bool _reflect;
    private readonly int _maxRetries;

    public DerivationAgenticExecutor(
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory, InferenceChecker checker, InferenceChecker postHocChecker, DerivationSpec spec,
        string model, RunContext run, string outDir, IReadOnlyDictionary<string, LedgerClaim>? ledgerClaims = null,
        bool explorer = false, bool verify = false, bool accountable = false, bool accountVerify = false,
        string? postHocJudgeModel = null, bool independentPostHoc = false, bool reflect = false, int maxRetries = 1)
        : base($"DerivationAgentic-{spec.Id}")
    {
        _agentFactory = agentFactory;
        _checker = checker;
        _postHocChecker = postHocChecker;
        _postHocJudgeModel = postHocJudgeModel ?? model;
        _independentPostHoc = independentPostHoc;
        _reflect = reflect;
        _maxRetries = Math.Max(0, maxRetries);
        _spec = spec;
        _model = model;
        _run = run;
        _outDir = outDir;
        _ledgerClaims = ledgerClaims ?? new Dictionary<string, LedgerClaim>();
        _explorer = explorer;
        _verify = verify;
        _accountable = accountable;
        _accountVerify = accountVerify;
    }

    // Der kombinierte Arm braucht BEIDE Auswertungen: Treue (verify) UND Coverage (accountable).
    private bool WantsVerify => _verify || _accountVerify;
    private bool WantsCoverage => _accountable || _accountVerify;

    private string ModeLabel => _reflect ? "explore-account-verify-reflect" : _accountVerify ? "explore-account-verify" : _verify ? "explore-verify" : _accountable ? "explore-account" : _explorer ? "explore" : "agentic";

    public override async ValueTask HandleAsync(SourceArtifactSet sources, IWorkflowContext context, CancellationToken ct = default)
    {
        // REFLECT-Arm: verbindliche externe Abnahme (Reflection-Pattern) mit bounded Loop — separater Pfad, lässt die
        // validierten Nicht-reflect-Arme unberührt. Belege/Begründung: siehe iteration-note A-agentic-12.
        if (_reflect) { await HandleReflectAsync(sources, context, ct).ConfigureAwait(false); return; }
        // verify/account-verify bekommen den Checker als In-Loop-Werkzeug (verify_derived); Explorer/agentic/account ohne.
        var tools = new DerivationTools(sources, _spec, _run, _outDir, _model, _ledgerClaims, WantsVerify ? _checker : null);
        // Toolset: account-verify = Explorer + account_uncovered + check_accountability + verify_derived (geschlossene Schleife);
        // verify = Explorer + verify_derived; account = Explorer + account_uncovered; explore = Entdeckungs-Set; sonst Basis.
        var toolSet = _accountVerify ? tools.BuildAccountVerify() : _verify ? tools.BuildVerify() : _accountable ? tools.BuildAccountable() : _explorer ? tools.BuildExplorer() : tools.Build();
        var agent = _agentFactory(toolSet);

        var task = new StringBuilder();
        if (_verify || _explorer || _accountable || _accountVerify)
        {
            task.AppendLine("Beginne. Deine Umwelt ist der verifizierte Projektzustand — sie wird dir NICHT vorab genannt.");
            task.AppendLine("Entdecke sie zuerst mit list_artifacts, konsultiere selbst, was du für dein Ziel brauchst, und speichere mit save_derived, wenn du genug Evidenz hast.");
            if (WantsVerify) task.AppendLine("Prüfe deinen Entwurf VOR dem Speichern mit verify_derived und überarbeite schwache Items, bis deine Definition of Done erfüllt ist.");
            if (WantsCoverage) task.AppendLine("Rechenschaft: JEDES Quell-Item muss am Ende entweder Anker eines Risikos ODER via account_uncovered mit Grund verworfen sein. Begründe vor jeder Tool-Entscheidung kurz, warum.");
            if (_accountVerify) task.AppendLine("Prüfe VOR dem Speichern mit check_accountability, ob etwas UNBEHANDELT ist oder du ein Item zugleich verankerst und verwirfst (Kollision); behebe beides selbst und speichere erst, wenn der Stand sauber ist.");
        }
        else
        {
            task.AppendLine($"Verfügbare geprüfte Quell-Artefakttypen: {string.Join(", ", sources.Sources.Select(s => $"{s.ArtifactType} ({s.Items.Count} Items)"))}.");
            task.AppendLine("Lies mit get_baseline_items(artifactType) die Typen, die du für die Ableitung brauchst; leite ab; verankere jedes Item an echte itemId(s); rufe am Ende save_derived GENAU EINMAL auf.");
        }

        _run.AppendEvent(new { type = "AGENTIC_DERIVATION_START", runId = _run.RunId, spec = _spec.Id, explorer = _explorer, sources = sources.Sources.Select(s => s.ArtifactType).ToArray(), sourceItems = sources.TotalItemCount, timestampUtc = DateTime.UtcNow });
        await agent.RunAsync([new ChatMessage(ChatRole.User, task.ToString())], cancellationToken: ct).ConfigureAwait(false);

        // Stufe 1: WAHRHEIT = was der Agent geschrieben hat. Zurücklesen, NICHT re-schreiben.
        var derivedPath = Path.Combine(_outDir, "derived.json");
        if (!tools.Saved || !File.Exists(derivedPath))
        {
            _run.AppendEvent(new { type = "AGENTIC_DERIVATION_NO_OUTPUT", runId = _run.RunId, spec = _spec.Id, saved = tools.Saved, fileExists = File.Exists(derivedPath), timestampUtc = DateTime.UtcNow });
            var empty = new ArtifactDocument(_spec.ItemIdPrefix, _spec.TargetArtifactType, 1, ArtifactDocument.StageDerivation, new ProducerMetadata(_run.RunId, _model, _spec.AgenticPromptName ?? _spec.PromptName), []);
            await WriteReportsAsync(empty, [], [], sources, tools, "agent_no_output", ct).ConfigureAwait(false);
            await context.YieldOutputAsync(new DerivationResult(empty, [], [], "agent_no_output")).ConfigureAwait(false);
            return;
        }

        var doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath, ct).ConfigureAwait(false), Read)
                  ?? new ArtifactDocument(_spec.ItemIdPrefix, _spec.TargetArtifactType, 1, ArtifactDocument.StageDerivation, new ProducerMetadata(_run.RunId, _model, null), []);

        // (a) UNABHÄNGIGE deterministische Anker-Validierung (annotiert, prunt derived.json NICHT).
        var sourceIds = sources.ItemsById().Keys.ToHashSet(StringComparer.Ordinal);
        var invalid = new List<InvalidAnchor>();
        foreach (var it in doc.Items)
        {
            var anchors = it.SourceArtifactItemIds ?? [];
            var bad = anchors.Where(a => !sourceIds.Contains(a)).ToList();
            if (anchors.Count == 0) invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "MISSING_ANCHOR"));
            else if (bad.Count > 0) invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "UNKNOWN_ANCHOR"));
        }

        // (b) UNABHÄNGIGE bounded Inference-Nach-Prüfung (Treue je Item gegen SEINE Anker) — mit dem Post-hoc-Judge,
        // gegen den der Agent NICHT im Loop optimiert hat (bricht die verify_derived-Zirkularität, wenn --posthoc-judge).
        var report = await _postHocChecker.CheckAsync(doc.Items, sources.ItemsById(), ct).ConfigureAwait(false);

        await WriteReportsAsync(doc, report.Verdicts, invalid, sources, tools, "agentic", ct).ConfigureAwait(false);
        _run.AppendEvent(new { type = "AGENTIC_DERIVATION_CHECKED", runId = _run.RunId, spec = _spec.Id, items = doc.Items.Count, invalidAnchor = invalid.Count, pass = report.Pass, byVerdict = report.ByVerdict, retrieved = tools.RetrievedItemIds.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DerivationResult(doc, report.Verdicts, invalid, "agentic")).ConfigureAwait(false);
    }

    // REFLECTION-PATTERN mit EXTERNEM Gate (Reflection = Konzept; Conditional-Edge-Semantik hier executor-gehostet, weil
    // der „Reviser" derselbe Agent ist). Critic = der bestehende EXTERNE Verifier-Stack (deterministische Anker-/Coverage-
    // Gates + unabhängiger Post-hoc-Judge), NICHT der Selbst-Check des Agenten (Self-Bias, Panickssery 2024). Loop feuert
    // NUR bei fail; bounded (Forschung: Runde 2 marginal, Runde 3 negativ). Memory: externe Kritik wird EXPLIZIT wieder
    // eingespeist (Entwurf liegt auf Disk, Befund als Text) — kein AgentSession-Thread nötig (in rc1 undokumentiert; und
    // bei EXTERNER Kritik ist explizites Re-Feed korrekt, vgl. Coding-Agent: Code + Test-Output neu lesen).
    private async Task HandleReflectAsync(SourceArtifactSet sources, IWorkflowContext context, CancellationToken ct)
    {
        var baseTask = BuildAccountVerifyTask(sources);
        var sourceIds = sources.ItemsById().Keys.ToHashSet(StringComparer.Ordinal);

        DerivationTools tools = null!;
        var doc = new ArtifactDocument(_spec.ItemIdPrefix, _spec.TargetArtifactType, 1, ArtifactDocument.StageDerivation, new ProducerMetadata(_run.RunId, _model, _spec.AgenticAccountVerifyPromptName), []);
        List<InvalidAnchor> invalid = [];
        IReadOnlyList<InferenceVerdict> verdicts = [];
        var gatePass = false;
        var firstDraftGatePass = false;
        double? firstDraftR1 = null; bool? firstDraftClean = null;
        var gateHistory = new List<object>();
        var taskText = baseTask;
        var roundsRun = 0;
        ArtifactDocument? prevDoc = null; var prevRound = -1;   // für den strukturierten Runden-Item-Diff (ARTIFACT_REVISED)

        for (var round = 0; round <= _maxRetries; round++)
        {
            roundsRun = round + 1;
            tools = new DerivationTools(sources, _spec, _run, _outDir, _model, _ledgerClaims, _checker);
            var agent = _agentFactory(tools.BuildAccountVerify());
            _run.AppendEvent(new { type = "REFLECT_ROUND_START", runId = _run.RunId, spec = _spec.Id, round, timestampUtc = DateTime.UtcNow });
            await agent.RunAsync([new ChatMessage(ChatRole.User, taskText)], cancellationToken: ct).ConfigureAwait(false);

            var derivedPath = Path.Combine(_outDir, "derived.json");
            var fails = new List<string>();
            if (!tools.Saved || !File.Exists(derivedPath))
            {
                fails.Add("Kein Artefakt gespeichert (save_derived nicht aufgerufen).");
                gatePass = false; gateHistory.Add(new { round, gatePass, fails });
                if (round == 0) { firstDraftR1 = null; firstDraftClean = false; }
                _run.AppendEvent(new { type = "REFLECT_GATE", runId = _run.RunId, spec = _spec.Id, round, gatePass, failCount = fails.Count, r1 = (double?)null, selfClean = false, note = "no_save", timestampUtc = DateTime.UtcNow });
                if (round == _maxRetries) break;
                taskText = BuildNoDraftTask(fails); continue;   // kein Entwurf vorhanden → nichts zu erhalten.
            }

            doc = JsonSerializer.Deserialize<ArtifactDocument>(await File.ReadAllTextAsync(derivedPath, ct).ConfigureAwait(false), Read) ?? doc;

            // Per-Runde-Snapshot (die on-disk derived.json wird von der nächsten Runde überschrieben) + struktureller
            // Item-Diff gegen den vorigen gespeicherten Entwurf → macht die „Item-Stabilität" maschinell belegbar.
            var snapRel = SnapshotRound(round, derivedPath);
            if (prevDoc is not null) EmitReviseDiff(prevRound, round, prevDoc, doc, snapRel);
            prevDoc = doc; prevRound = round;

            // GATE (extern, deterministisch + unabhängiger Judge):
            // flaggedByItem = per-Item-Kritik fb_t (itemId → Gründe) für das chirurgische Re-Feed (Self-Refine).
            invalid = [];
            var flaggedByItem = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            void Flag(string id, string reason) { if (!flaggedByItem.TryGetValue(id, out var l)) { l = []; flaggedByItem[id] = l; } l.Add(reason); }
            foreach (var it in doc.Items)
            {
                var anchors = it.SourceArtifactItemIds ?? [];
                var bad = anchors.Where(a => !sourceIds.Contains(a)).ToList();
                if (anchors.Count == 0) { invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "MISSING_ANCHOR")); Flag(it.ItemId, "kein Anker gesetzt — verankere an einem realen Quell-Item oder verwirf begründet."); }
                else if (bad.Count > 0) { invalid.Add(new InvalidAnchor(it.Text, anchors, bad, "UNKNOWN_ANCHOR")); Flag(it.ItemId, $"unbekannte Anker: {string.Join(", ", bad)} — ersetze durch reale Quell-IDs."); }
            }
            verdicts = (await _postHocChecker.CheckAsync(doc.Items, sources.ItemsById(), ct).ConfigureAwait(false)).Verdicts;
            var cov = DerivationCoverage.Evaluate(sources, doc.Items, tools.AccountedItemIds, tools.Dismissals);
            var badV = verdicts.Where(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated).ToList();
            foreach (var v in badV) Flag(v.ItemId, $"unabhängiger Prüfer: verdict={v.Verdict} — {v.Rationale}");
            var r1 = doc.Items.Count == 0 ? 0.0 : Math.Round((double)badV.Count / doc.Items.Count, 4);

            if (invalid.Count > 0) fails.Add($"{invalid.Count} ungültige Anker: {string.Join(", ", invalid.SelectMany(i => i.BadIds).Distinct())}.");
            if (badV.Count > 0) fails.Add($"{badV.Count} nicht-tragende Risiken (verdict∈contradicts/unrelated): {string.Join(", ", badV.Select(v => v.ItemId))}.");
            if (!cov.SelfAccountingClean) fails.Add($"Rechenschaft unsauber — unbehandelt: [{string.Join(", ", cov.UnaccountedItemIds)}], Kollision: [{string.Join(", ", cov.CollisionItemIds)}].");

            gatePass = fails.Count == 0;
            if (round == 0) { firstDraftR1 = r1; firstDraftClean = cov.SelfAccountingClean; firstDraftGatePass = gatePass; }
            gateHistory.Add(new { round, gatePass, fails });
            _run.AppendEvent(new { type = "REFLECT_GATE", runId = _run.RunId, spec = _spec.Id, round, gatePass, failCount = fails.Count, r1, selfClean = cov.SelfAccountingClean, timestampUtc = DateTime.UtcNow });

            if (gatePass || round == _maxRetries) break;
            // Self-Refine (arXiv:2303.17651): den Entwurf y_t + per-Item-Feedback fb_t explizit zurückspeisen
            // → konditionierte Regeneration mit Erhalt der abgenommenen Items (chirurgisch), kein Neustart.
            taskText = BuildReviseTask(doc, flaggedByItem, cov);
        }

        var finalBad = verdicts.Count(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated);
        var finalR1 = doc.Items.Count == 0 ? 0.0 : Math.Round((double)finalBad / doc.Items.Count, 4);
        var overCorrected = firstDraftR1 is double fd && finalR1 > fd;   // „silent killer": Loop hat schon-korrektes verschlechtert?
        var reflectBlock = new
        {
            rounds = roundsRun, maxRetries = _maxRetries,
            firstDraftGatePass, finalGatePass = gatePass, needsRepair = !gatePass,
            firstDraftR1, finalR1, overCorrected, gateHistory
        };
        var decision = gatePass ? "reflect_pass" : "reflect_needsRepair";
        await WriteReportsAsync(doc, verdicts, invalid, sources, tools, decision, ct, reflectBlock).ConfigureAwait(false);
        _run.AppendEvent(new { type = "REFLECT_DONE", runId = _run.RunId, spec = _spec.Id, rounds = roundsRun, finalGatePass = gatePass, needsRepair = !gatePass, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DerivationResult(doc, verdicts, invalid, decision)).ConfigureAwait(false);
    }

    private string BuildAccountVerifyTask(SourceArtifactSet sources)
    {
        var t = new StringBuilder();
        t.AppendLine("Beginne. Deine Umwelt ist der verifizierte Projektzustand — sie wird dir NICHT vorab genannt.");
        t.AppendLine("Entdecke sie zuerst mit list_artifacts, konsultiere selbst, was du für dein Ziel brauchst, und speichere mit save_derived, wenn du genug Evidenz hast.");
        t.AppendLine("Prüfe deinen Entwurf VOR dem Speichern mit verify_derived und überarbeite schwache Items, bis deine Definition of Done erfüllt ist.");
        t.AppendLine("Rechenschaft: JEDES Quell-Item muss am Ende entweder Anker eines Risikos ODER via account_uncovered mit Grund verworfen sein. Begründe vor jeder Tool-Entscheidung kurz, warum.");
        t.AppendLine("Prüfe VOR dem Speichern mit check_accountability, ob etwas UNBEHANDELT ist oder du ein Item zugleich verankerst und verwirfst (Kollision); behebe beides selbst und speichere erst, wenn der Stand sauber ist.");
        return t.ToString();
    }

    // Self-Refine (arXiv:2303.17651): y_{t+1} = M(p_refine ∥ x ∥ y_t ∥ fb_t). Wir geben dem Agenten seinen
    // vorherigen Entwurf y_t (die derived.json, hier aus doc rekonstruiert) UND das per-Item-Feedback fb_t
    // explizit zurück — der Entwurf ist Tool-erzeugter Zustand auf Disk, kein Konversationstext, deshalb muss er
    // explizit zurückgereicht werden (Coding-Agent-Muster „maintain artifacts the agent can directly read").
    // Erhalt-Anweisung = chirurgisch: abgenommene Items bleiben, nur beanstandete werden geändert/verworfen.
    private string BuildReviseTask(ArtifactDocument prevDraft, IReadOnlyDictionary<string, List<string>> flaggedByItem, CoverageReport cov)
    {
        var t = new StringBuilder();
        t.AppendLine("Dein vorheriger Entwurf wurde EXTERN und unabhängig geprüft und ist noch NICHT abgenommen.");
        t.AppendLine("Er ist noch gespeichert; dies ist dein AUSGANGSPUNKT — beginne NICHT bei null.");
        t.AppendLine();
        t.AppendLine("=== DEIN VORHERIGER ENTWURF (überarbeite genau diesen) ===");
        foreach (var it in prevDraft.Items)
        {
            var anchors = it.SourceArtifactItemIds is { Count: > 0 } a ? string.Join(", ", a) : "—";
            var flagged = flaggedByItem.TryGetValue(it.ItemId, out var reasons);
            t.AppendLine($"[{(flagged ? "BEANSTANDET" : "ABGENOMMEN")}] {it.ItemId}  (Anker: {anchors})");
            t.AppendLine($"    {it.Text}");
            if (flagged) foreach (var r in reasons!) t.AppendLine($"    → {r}");
        }
        t.AppendLine();
        if (!cov.SelfAccountingClean)
        {
            t.AppendLine("=== RECHENSCHAFT (noch unsauber) ===");
            if (cov.UnaccountedItemIds.Count > 0) t.AppendLine($"Unbehandelte Quell-Items (verankern ODER via account_uncovered begründet verwerfen): [{string.Join(", ", cov.UnaccountedItemIds)}]");
            if (cov.CollisionItemIds.Count > 0) t.AppendLine($"Kollision (zugleich verankert UND verworfen — mit account_uncovered dismiss=false zurücknehmen): [{string.Join(", ", cov.CollisionItemIds)}]");
            t.AppendLine();
        }
        t.AppendLine("=== AUFTRAG ===");
        t.AppendLine("Behalte die ABGENOMMENEN Items UNVERÄNDERT. Ändere/ersetze/verwirf NUR die BEANSTANDETEN Items (bessere tragende Anker, oder begründet verwerfen). Stelle lückenlose UND kollisionsfreie Rechenschaft her.");
        t.AppendLine("Entdecke bei Bedarf die Umwelt erneut mit list_artifacts, und speichere die vollständige finale Fassung (abgenommene + korrigierte Items) mit save_derived GENAU EINMAL.");
        return t.ToString();
    }

    // Sonderfall: der vorige Durchlauf hat gar nicht gespeichert → es gibt kein y_t zu erhalten.
    private string BuildNoDraftTask(IReadOnlyList<string> fails)
    {
        var t = new StringBuilder();
        t.AppendLine("Dein vorheriger Durchlauf hat KEIN Artefakt gespeichert. Befunde:");
        foreach (var f in fails) t.AppendLine($"- {f}");
        t.AppendLine("Führe die Ableitung erneut aus: entdecke die Umwelt mit list_artifacts, leite die tragenden Risiken ab, stelle lückenlose UND kollisionsfreie Rechenschaft her, und speichere mit save_derived GENAU EINMAL.");
        return t.ToString();
    }

    // Sichert den in dieser Runde gespeicherten Entwurf, bevor die nächste Runde die on-disk derived.json überschreibt.
    // Ablage in einem sprechenden Archiv-Ordner NEBEN dem Artefakt (reflect-archive/derived.round-NN.json).
    private string SnapshotRound(int round, string derivedPath)
    {
        var archiveDir = Path.Combine(_outDir, "reflect-archive");
        Directory.CreateDirectory(archiveDir);
        var dest = Path.Combine(archiveDir, $"derived.round-{round:00}.json");
        File.Copy(derivedPath, dest, overwrite: true);
        return Path.GetRelativePath(_run.RunDir, dest).Replace('\\', '/');
    }

    // Struktureller Item-Diff zwischen zwei gespeicherten Runden. Item-IDENTITÄT via normalisiertem Text, NICHT via
    // itemId — die IDs sind positionsbasiert und rutschen beim Entfernen eines Items (round-0 DRISK-06 → round-1 DRISK-05).
    // Exakter Text-Match ist der STRENGE Stabilitäts-Test: eine minimal umformulierte „behaltene" Zeile zählt bewusst als
    // removed+added, nicht als kept. Analog zu Phase2B ARTIFACT_SUSPICIOUS_OVERWRITE, aber semantisch statt zähl-basiert.
    private void EmitReviseDiff(int fromRound, int toRound, ArtifactDocument prev, ArtifactDocument cur, string curSnapshotRel)
    {
        static string Key(ArtifactItem i) => i.Text.Trim();
        static string Anch(ArtifactItem i) => string.Join(",", (i.SourceArtifactItemIds ?? []).OrderBy(x => x, StringComparer.Ordinal));
        static string Short(string s) => s.Length <= 100 ? s : s[..100] + "…";

        var prevByText = prev.Items.GroupBy(Key).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var curByText = cur.Items.GroupBy(Key).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var kept = new List<string>(); var removed = new List<string>(); var added = new List<string>();
        var changedAnchors = new List<object>();
        foreach (var (text, pit) in prevByText)
        {
            if (curByText.TryGetValue(text, out var cit))
            {
                if (Anch(pit) == Anch(cit)) kept.Add(Short(text));
                else changedAnchors.Add(new { text = Short(text), from = Anch(pit), to = Anch(cit) });
            }
            else removed.Add(Short(text));
        }
        foreach (var (text, _) in curByText)
            if (!prevByText.ContainsKey(text)) added.Add(Short(text));

        var evt = new
        {
            type = "ARTIFACT_REVISED", runId = _run.RunId, spec = _spec.Id, fromRound, toRound,
            prevCount = prev.Items.Count, curCount = cur.Items.Count,
            keptCount = kept.Count, removedCount = removed.Count, addedCount = added.Count, changedAnchorsCount = changedAnchors.Count,
            kept, removed, added, changedAnchors, snapshot = curSnapshotRel,
            note = "Item-Identität via normalisiertem Text (IDs sind positionsbasiert); exakter Text-Match = strenger Stabilitäts-Test.",
            timestampUtc = DateTime.UtcNow
        };
        _run.AppendEvent(evt);
        File.WriteAllText(Path.Combine(_outDir, "reflect-archive", $"revise.round-{fromRound:00}-to-{toRound:00}.json"), JsonSerializer.Serialize(evt, Json));
    }

    private async Task WriteReportsAsync(
        ArtifactDocument doc, IReadOnlyList<InferenceVerdict> verdicts, IReadOnlyList<InvalidAnchor> invalid,
        SourceArtifactSet sources, DerivationTools tools, string decision, CancellationToken ct, object? reflectBlock = null)
    {
        var byVerdict = verdicts.GroupBy(v => v.Verdict).ToDictionary(g => g.Key.ToString(), g => g.Count());
        var report = new InferenceCheckReport(
            Pass: verdicts.All(v => v.Verdict is not (InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated)),
            Total: doc.Items.Count, ByVerdict: byVerdict, Verdicts: verdicts,
            Flagged: verdicts.Where(v => v.Verdict is InferenceVerdictKind.Contradicts or InferenceVerdictKind.Unrelated).ToList());

        await File.WriteAllTextAsync(Path.Combine(_outDir, "inference-check-report.json"), JsonSerializer.Serialize(report, Json), ct).ConfigureAwait(false);

        // usedItemIds = tatsächlich als Prämisse verankert; retrievedItemIds = alles Betrachtete (Tool-Provenienz).
        var used = doc.Items.SelectMany(i => i.SourceArtifactItemIds ?? []).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray();
        // B0: deterministischer Metrik-Vektor (N1/N2/R1/R2) je Lauf — gleiche Funktion wie im strukturierten Arm (driftfrei).
        // Agentisch: defekte Items werden GESCHRIEBEN und nur markiert (invalid ⊆ doc) → Nenner R2 = doc.Items.Count.
        var metrics = DerivationMetrics.ComputeDeterministic(doc, sources, invalid, verdicts, doc.Items.Count);

        // Verify-Arm: DoD-Scorecard (neutral, kein Zwang) + ΔR1 (Korrektur-Gewinn) + Runden.
        object? verifyBlock = null;
        if (WantsVerify)
        {
            var dod = DerivationDoD.Evaluate(doc, sources, verdicts);
            await File.WriteAllTextAsync(Path.Combine(_outDir, "dod-report.json"), JsonSerializer.Serialize(dod, Json), ct).ConfigureAwait(false);
            var finalR1 = metrics.R1_FidelityViolationRate;
            verifyBlock = new
            {
                rounds = tools.VerifyRounds,
                firstDraftR1 = tools.FirstDraftR1,
                finalR1,
                deltaR1 = tools.FirstDraftR1 is double f ? Math.Round(f - finalR1, 4) : (double?)null,
                dodPass = dod.Pass, dodPassed = dod.Passed, dodTotal = dod.Total, dodFailures = dod.FailuresByCriterion
            };
        }

        // Accountable-Arm: Coverage-Rechenschaft (closed-world) — jedes Quell-Item genutzt oder begründet verworfen.
        object? coverageBlock = null;
        object? coverageDeltaBlock = null;
        if (WantsCoverage)
        {
            var cov = DerivationCoverage.Evaluate(sources, doc.Items, tools.AccountedItemIds, tools.Dismissals);
            await File.WriteAllTextAsync(Path.Combine(_outDir, "coverage-report.json"), JsonSerializer.Serialize(cov, Json), ct).ConfigureAwait(false);
            coverageBlock = new
            {
                cov.Total, cov.Covered, cov.Accounted, cov.Unaccounted, cov.CoverageComplete,
                cov.DismissedRaw, cov.Collisions, cov.SelfAccountingClean,
                collisionRate = cov.Covered > 0 ? Math.Round((double)cov.Collisions / cov.Covered, 4) : 0.0,
                dismissalGroups = tools.Dismissals.Count
            };

            // Account-Verify: ΔCoverage = Selbstkorrektur-Gewinn (Roh-Entwurf beim 1. check_accountability vs. final).
            // Positives Δ = der Agent hat via In-Loop-Feedback Lücken/Kollisionen selbst geschlossen. Das ist das Signal
            // (nicht der Endzustand, der über die Schleife trivial sauber würde). rounds=0 → Feedback nicht genutzt.
            if (_accountVerify)
                coverageDeltaBlock = new
                {
                    rounds = tools.AccountabilityRounds,
                    firstDraftUnaccounted = tools.FirstDraftUnaccounted,
                    finalUnaccounted = cov.Unaccounted,
                    deltaUnaccounted = tools.FirstDraftUnaccounted is int fu ? fu - cov.Unaccounted : (int?)null,
                    firstDraftCollisions = tools.FirstDraftCollisions,
                    finalCollisions = cov.Collisions,
                    deltaCollisions = tools.FirstDraftCollisions is int fc ? fc - cov.Collisions : (int?)null,
                    selfAccountingClean = cov.SelfAccountingClean
                };
        }

        await File.WriteAllTextAsync(Path.Combine(_outDir, "derivation-report.json"), JsonSerializer.Serialize(new
        {
            spec = _spec.Id, mode = ModeLabel, decision,
            anchoredValid = doc.Items.Count - invalid.Count, invalidAnchor = invalid.Count, invalid,
            retrievedItemIds = tools.RetrievedItemIds.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            usedItemIds = used,
            retrievedCount = tools.RetrievedItemIds.Count, usedCount = used.Length,
            // independentPostHoc = wurde finalR1/dodPass von einem anderen Judge geprüft als dem In-Loop-verify_derived?
            // (bricht Zirkularität). postHocJudgeModel = welches Modell die unabhängige Nach-Prüfung fuhr.
            independentPostHoc = _independentPostHoc, postHocJudgeModel = _postHocJudgeModel,
            metrics, verify = verifyBlock, coverage = coverageBlock, coverageDelta = coverageDeltaBlock, reflect = reflectBlock
        }, Json), ct).ConfigureAwait(false);
    }
}
