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
    private readonly InferenceChecker _checker;
    private readonly DerivationSpec _spec;
    private readonly string _model;
    private readonly RunContext _run;
    private readonly string _outDir;
    private readonly IReadOnlyDictionary<string, LedgerClaim> _ledgerClaims;
    private readonly bool _explorer;
    private readonly bool _verify;
    private readonly bool _accountable;

    public DerivationAgenticExecutor(
        Func<IReadOnlyList<AITool>, AIAgent> agentFactory, InferenceChecker checker, DerivationSpec spec,
        string model, RunContext run, string outDir, IReadOnlyDictionary<string, LedgerClaim>? ledgerClaims = null,
        bool explorer = false, bool verify = false, bool accountable = false)
        : base($"DerivationAgentic-{spec.Id}")
    {
        _agentFactory = agentFactory;
        _checker = checker;
        _spec = spec;
        _model = model;
        _run = run;
        _outDir = outDir;
        _ledgerClaims = ledgerClaims ?? new Dictionary<string, LedgerClaim>();
        _explorer = explorer;
        _verify = verify;
        _accountable = accountable;
    }

    private string ModeLabel => _verify ? "explore-verify" : _accountable ? "explore-account" : _explorer ? "explore" : "agentic";

    public override async ValueTask HandleAsync(SourceArtifactSet sources, IWorkflowContext context, CancellationToken ct = default)
    {
        // Verify bekommt den Checker als In-Loop-Werkzeug (verify_derived); Explorer/agentic/account ohne.
        var tools = new DerivationTools(sources, _spec, _run, _outDir, _model, _ledgerClaims, _verify ? _checker : null);
        // Toolset: verify = Explorer + verify_derived; account = Explorer + account_uncovered; explore = Entdeckungs-Set; sonst Basis.
        var toolSet = _verify ? tools.BuildVerify() : _accountable ? tools.BuildAccountable() : _explorer ? tools.BuildExplorer() : tools.Build();
        var agent = _agentFactory(toolSet);

        var task = new StringBuilder();
        if (_verify || _explorer || _accountable)
        {
            task.AppendLine("Beginne. Deine Umwelt ist der verifizierte Projektzustand — sie wird dir NICHT vorab genannt.");
            task.AppendLine("Entdecke sie zuerst mit list_artifacts, konsultiere selbst, was du für dein Ziel brauchst, und speichere mit save_derived, wenn du genug Evidenz hast.");
            if (_verify) task.AppendLine("Prüfe deinen Entwurf VOR dem Speichern mit verify_derived und überarbeite schwache Items, bis deine Definition of Done erfüllt ist.");
            if (_accountable) task.AppendLine("Rechenschaft: JEDES Quell-Item muss am Ende entweder Anker eines Risikos ODER via account_uncovered mit Grund verworfen sein. Begründe vor jeder Tool-Entscheidung kurz, warum.");
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

        // (b) bounded Inference-Check (unverändert: Treue je Item gegen SEINE Anker).
        var report = await _checker.CheckAsync(doc.Items, sources.ItemsById(), ct).ConfigureAwait(false);

        await WriteReportsAsync(doc, report.Verdicts, invalid, sources, tools, "agentic", ct).ConfigureAwait(false);
        _run.AppendEvent(new { type = "AGENTIC_DERIVATION_CHECKED", runId = _run.RunId, spec = _spec.Id, items = doc.Items.Count, invalidAnchor = invalid.Count, pass = report.Pass, byVerdict = report.ByVerdict, retrieved = tools.RetrievedItemIds.Count, timestampUtc = DateTime.UtcNow });
        await context.YieldOutputAsync(new DerivationResult(doc, report.Verdicts, invalid, "agentic")).ConfigureAwait(false);
    }

    private async Task WriteReportsAsync(
        ArtifactDocument doc, IReadOnlyList<InferenceVerdict> verdicts, IReadOnlyList<InvalidAnchor> invalid,
        SourceArtifactSet sources, DerivationTools tools, string decision, CancellationToken ct)
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
        if (_verify)
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
        if (_accountable)
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
        }

        await File.WriteAllTextAsync(Path.Combine(_outDir, "derivation-report.json"), JsonSerializer.Serialize(new
        {
            spec = _spec.Id, mode = ModeLabel, decision,
            anchoredValid = doc.Items.Count - invalid.Count, invalidAnchor = invalid.Count, invalid,
            retrievedItemIds = tools.RetrievedItemIds.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
            usedItemIds = used,
            retrievedCount = tools.RetrievedItemIds.Count, usedCount = used.Length,
            metrics, verify = verifyBlock, coverage = coverageBlock
        }, Json), ct).ConfigureAwait(false);
    }
}
