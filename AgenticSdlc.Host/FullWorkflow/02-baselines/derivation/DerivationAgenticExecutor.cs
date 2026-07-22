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
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen
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

            // GATE (extern, deterministisch + unabhängiger Judge) — geteilte Logik (driftfrei mit der edge-nativen Form).
            var gate = await ReflectPipeline.EvaluateGateAsync(sources, sourceIds, doc, tools, _postHocChecker, ct).ConfigureAwait(false);
            invalid = gate.Invalid; verdicts = gate.Verdicts; fails = gate.Fails;
            gatePass = gate.GatePass;
            if (round == 0) { firstDraftR1 = gate.R1; firstDraftClean = gate.Coverage.SelfAccountingClean; firstDraftGatePass = gatePass; }
            gateHistory.Add(new { round, gatePass, fails });
            _run.AppendEvent(new { type = "REFLECT_GATE", runId = _run.RunId, spec = _spec.Id, round, gatePass, failCount = fails.Count, r1 = gate.R1, selfClean = gate.Coverage.SelfAccountingClean, timestampUtc = DateTime.UtcNow });

            if (gatePass || round == _maxRetries) break;
            // Self-Refine (arXiv:2303.17651): den Entwurf y_t + per-Item-Feedback fb_t explizit zurückspeisen
            // → konditionierte Regeneration mit Erhalt der abgenommenen Items (chirurgisch), kein Neustart.
            taskText = BuildReviseTask(doc, gate.FlaggedByItem, gate.Coverage);
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

    // Task-Texte: geteilt via ReflectPipeline (driftfrei mit der edge-nativen Form).
    private string BuildAccountVerifyTask(SourceArtifactSet sources) => ReflectPipeline.BuildAccountVerifyTask();
    private string BuildReviseTask(ArtifactDocument prevDraft, IReadOnlyDictionary<string, List<string>> flaggedByItem, CoverageReport cov)
        => ReflectPipeline.BuildReviseTask(prevDraft, flaggedByItem, cov);
    private string BuildNoDraftTask(IReadOnlyList<string> fails) => ReflectPipeline.BuildNoDraftTask(fails);

    // Snapshot + Item-Diff: geteilt via ReflectPipeline (driftfrei mit der edge-nativen Form).
    private string SnapshotRound(int round, string derivedPath) => ReflectPipeline.SnapshotRound(_run, _outDir, round, derivedPath);
    private void EmitReviseDiff(int fromRound, int toRound, ArtifactDocument prev, ArtifactDocument cur, string curSnapshotRel)
        => ReflectPipeline.EmitReviseDiff(_run, _spec.Id, _outDir, Json, fromRound, toRound, prev, cur, curSnapshotRel);

    private Task WriteReportsAsync(
        ArtifactDocument doc, IReadOnlyList<InferenceVerdict> verdicts, IReadOnlyList<InvalidAnchor> invalid,
        SourceArtifactSet sources, DerivationTools tools, string decision, CancellationToken ct, object? reflectBlock = null)
        => ReflectPipeline.WriteReportsAsync(doc, verdicts, invalid, sources, tools, decision, _run, _spec, _outDir,
            ModeLabel, WantsVerify, WantsCoverage, _accountVerify, _independentPostHoc, _postHocJudgeModel, Json, reflectBlock, ct);

}
