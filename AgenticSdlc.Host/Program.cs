using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Run;
using System.Diagnostics;

var repoRoot = FindRepoRoot();

DotNetEnv.Env.Load(
    Path.Combine(repoRoot, ".env"),
    new DotNetEnv.LoadOptions(
        setEnvVars: true,
        clobberExistingVars: true,
        onlyExactPath: true
    )
);

var runtimeConfig = RunConfig.Load(repoRoot);
var settings = HostSettings.FromRuntimeConfig(runtimeConfig, repoRoot);
if (args.Length > 0)
    args[0] = args[0].Trim();

// ============================================================================================================
// CLI-KOMMANDO-DISPATCH — LANDKARTE (aktualisiert Move 3, 2026-07-22).
// Die frueheren Evaluation-/Phase-Kommandos (eval-offline, parse-units, review*, topic*, claim*, source-claim*,
// evidence-first-spike, human-artifact*, semantic-ledger-extract*, review + AGENT_PHASE phase1/phase2_1) sind
// ARCHIVIERT (archive/, ausserhalb des Builds) — lauffaehig ueber die Stations-Tags:
//   git checkout v-s0-phase1 | v-s1-phase2-dag | v-s3-evaluator-review
//
// [LIVE] aktiver Strang (lebender Core + Tor 1/2/3), inkl. der neuen MAF-nativen Human-Gates (*-hitl):
//   Core:   core-bootstrap-first-transcript, core-seed, core-baseline, core-seed-backlog, core-view, project-state-build
//   Tor 1:  ingest-requirements, ingest-review, ingest-apply, ingest-requirements-hitl
//   Place:  pbi-update, pbi-update-review, pbi-update-apply, pbi-update-hitl
//   Tor 2:  decision-resolve, decision-resolve-agent, decision-review, decision-apply, decision-resolve-hitl, decision-unblock-test
//   Tor 3:  github-map, github-read, github-snapshot, github-write, github-forward, github-forward-hitl,
//           github-forward-review, github-forward-apply, github-forward-compare, github-forward-rerun-test,
//           github-reverse, github-reverse-review, github-reverse-apply
//
// [SUPPORT] L4 / Backlog-Qualitaet + Nebenpfade (aktuell, KEIN Spike):
//   l4-baseline, l4-consolidation, l4-review, l4-apply, l4-quality, l4-requirements-doc,
//   l4-completion, l4-completion-review, l4-completion-apply, requirements-readiness,
//   l4-issuplanning, l4-issuplanning-review, l4-issuplanning-apply,
//   clarification-agent, clarification-agent-review, clarification-agent-apply,
//   open-requirements-review, open-requirements-apply, operationalization-audit (?)
//
// [FRONT] Produkt-Front (Ledger-Pfad, intendierter Transkript->MeetingDelta-Weg — s. lokale PRODUCT-CAPABILITY-MAP):
//   Ledger:           ledger-build, ledger-build-units, ledger-reference-template, ledger-adjudicate,
//                     ledger-adjudicate-apply, ledger-adjudicate-ui, ledger-adjudicate-refine, ledger-validate,
//                     facet-validation-eval, ledger-reference-recall, ledger-reference-recall-fast, ledger-cite-fidelity
//   Front-Mitte:      recipe, baseline-fanout, artifact-branch, assign-artifact-ids (consumable -> Baselines)
//   L3 (dormant):     l3, l3-apply, l3-revise, l3-review
//
// [CAPSTONE/RESEARCH — Kommandos noch aufrufbar, Code in Place bzw. research/]:
//   Contract/Derivation: contract-check, contract-critic, contract-repair, evidence-chain, derive, derive-metrics,
//                        checker-repair-workflow, inference-check, derive-risks, derive-review
//   Tor-3-Vorlaeufer (research/, abgeloest durch github-forward): github-reconciliation(-review|-apply), github-write
//   Verifikations-Spike (S0/S-9): spike-hitl
// ============================================================================================================

// S0 (Worklist 20.07): Verifikations-Spike fuer MAF-nativen Human-Gate (RequestPort) + durables, prozessuebergreifendes
// Checkpoint/Resume (FileSystemJsonCheckpointStore). Wegwerf/isoliert, kein LLM, kein Core-Zugriff.
if (args.Length > 0 && string.Equals(args[0], "spike-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.HitlSpike.HitlSpikeRunner.RunAsync(args, repoRoot);
    return;
}

// Ledger L1: Transkript -> Candidate Ledger -> Canonical Ledger als ECHTER MAF-Workflow (runs/ledger/<runId>).
// Optional mit Fixture: matcht den kanonischen Ledger gegen die Fixture = Baseline-Reproduktion (Exit-Kriterium L1).
if (args.Length > 0 && string.Equals(args[0], "ledger-build", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerBuildRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Ledger High-Coverage-MVP: Transkript -> deterministische Atomic Units -> unit-aware Candidate Ledger
// -> bestehende Canonical/Repair/Facet-Kette. Additiv; normaler ledger-build bleibt unveraendert.
if (args.Length > 0 && string.Equals(args[0], "ledger-build-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerBuildUnitsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Ledger-Referenz-Vorlage (deterministisch, KEIN LLM): Transkript + Kandidaten-Ledger -> segmentweise
// Annotations-Vorlage zum Bau eines hand-vollständigen Referenz-Ledgers (Completeness/Recall-Messung).
if (args.Length > 0 && string.Equals(args[0], "ledger-reference-template", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerReferenceTemplateRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Adjudikation Schritt 1 (deterministisch, kein LLM): review_required + Misses -> Adjudikations-Queue.
if (args.Length > 0 && string.Equals(args[0], "ledger-adjudicate", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerAdjudicateRunner.RunPrepareAsync(args, repoRoot);
    return;
}
// Adjudikation apply: ausgefüllte Queue -> adjudicated-ledger + consumable + AdjudicationCompletenessGate.
if (args.Length > 0 && string.Equals(args[0], "ledger-adjudicate-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerAdjudicateRunner.RunApplyAsync(args, repoRoot);
    return;
}
// Adjudikation Interactive/Re-Launch (§9): lokale Review-UI über der queue.json (Autosave), Finish -> apply.
if (args.Length > 0 && string.Equals(args[0], "ledger-adjudicate-ui", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerAdjudicateUiRunner.RunAsync(args, repoRoot);
    return;
}
// Adjudikation A10 (Refine): neu geminteten Claims (facetStatus=pending) volle Facetten zuweisen (deterministischer
// Filter, LLM nur auf den pending-Claims) -> consumable-Claims auf Pipeline-Niveau.
if (args.Length > 0 && string.Equals(args[0], "ledger-adjudicate-refine", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerAdjudicateRefineRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L3 realer Test: bestehenden Ledger mit dem FacetValidator prüfen (evidence- oder transcript-Kontext).
if (args.Length > 0 && string.Equals(args[0], "ledger-validate", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerValidateRunner.RunAsync(args, settings, repoRoot);
    return;
}

// MC0 (Maker-Checker): deterministischer Contract-Checker requirements.md + consumable.json -> contract-report.json.
if (args.Length > 0 && string.Equals(args[0], "contract-check", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.ContractCheckRunner.RunAsync(args, settings, repoRoot);
    return;
}

// C7 (MC3): bounded Evidence-Support-Critic (LLM) — Detail-Deckung je Zeile gegen das zitierte Claim-Paket.
if (args.Length > 0 && string.Equals(args[0], "contract-critic", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.ContractCriticRunner.RunAsync(args, settings, repoRoot);
    return;
}

// MC2: bounded Repair-Loop (k-Vote-Critic -> Repair -> re-check, max N) auf einem Artefakt.
if (args.Length > 0 && string.Equals(args[0], "contract-repair", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.ContractRepairRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Volle MAF-Komposition: Ledger -> [Fan-out] -> SelectBaseline -> [Derivation] (mehrstufig BindAsExecutor).
if (args.Length > 0 && string.Equals(args[0], "evidence-chain", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Chain.EvidenceChainRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Rezept-Assembler (§10): deklaratives Rezept -> Graph zur Laufzeit (Baseline build|load -> 0..N Ableitungen).
if (args.Length > 0 && string.Equals(args[0], "recipe", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Recipes.RecipeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Derivation-Familie (verallgemeinert): config-gesteuerter Ableitungs-Workflow (Generate[Agent]->Anchor->Check).
if (args.Length > 0 && string.Equals(args[0], "derive", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation.DerivationRunner.RunAsync(args, settings, repoRoot);
    return;
}

// B0: Ableitungsgüte-Aggregator über MEHRERE Läufe (Mittel + Spannweite je Modus; --judge = R3 Scope-Creep).
if (args.Length > 0 && string.Equals(args[0], "derive-metrics", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation.DerivationMetricsAggregator.RunAsync(args, settings, repoRoot);
    return;
}

// L3 Open-World-Ableitung: Kandidaten generieren → verankern → in 4 Klassen routen → Human-Review-Paket (Prepare-Phase).
if (args.Length > 0 && string.Equals(args[0], "l3", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3.L3Runner.RunAsync(args, settings, repoRoot);
    return;
}

// L3 Apply-Phase (Workflow 2): menschliche Entscheidungen (accept/edit/reject) deterministisch anwenden → Promotion + Provenienz.
if (args.Length > 0 && string.Equals(args[0], "l3-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3.L3ApplyRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L3 Reflect-Sub-Workflow (NEEDS_REVISION): Kandidaten per Feedback überarbeiten (Self-Refine) → neu klassifizieren, bounded.
if (args.Length > 0 && string.Equals(args[0], "l3-revise", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3.L3ReviseRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L3 Human-Review (config l3.reviewMode: file|interactive): Review-UI → human-decisions.json (von apply/revise konsumiert).
if (args.Length > 0 && string.Equals(args[0], "l3-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L3.L3ReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Project State: JSON-first fachlicher Projektzustand aus L1/L2-Artefakten + akzeptierten L3-Promotions.
if (args.Length > 0 && string.Equals(args[0], "project-state-build", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.ProjectState.ProjectStateBuildRunner.RunAsync(args, repoRoot);
    return;
}

// Core (die lebende Projektwahrheit): einmaliger Seed aus einem ProjectState-Rebuild.
// Bootstrap / Start bei null (B1): erstes Transkript, noch kein Core. Modus-Erkennung + deterministische
// Orchestrierung (project-state-build -> core-seed -> core-baseline). Danach agentische Backlog-Stufe (mit Gates).
if (args.Length > 0 && string.Equals(args[0], "core-bootstrap-first-transcript", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreBootstrapRunner.RunAsync(args, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "core-seed", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreSeedRunner.RunAsync(args, repoRoot);
    return;
}

// Requirement-Ingestion: neues MeetingDelta gegen den Core aufloesen (Resolver-Agent -> Gate -> StateChangePlan).
if (args.Length > 0 && string.Equals(args[0], "ingest-requirements", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.IngestionRequirementsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// HumanReview der Ingestion-Operationen (apply/skip je Operation).
// S6 (Worklist 20.07) — A: EIN MAF-Lauf ueber zwei Stufen (ingest -> pbi-update) mit zwei Human-Gates,
// prozessuebergreifend resumebar (start -> resume Gate1 -> resume Gate2). Komposition der S4-Stufen.
if (args.Length > 0 && string.Equals(args[0], "pipeline-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Pipeline.PipelineComposedRunner.RunAsync(args, settings, repoRoot);
    return;
}

// S4 (Worklist 20.07): ingestion (Tor 1) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
// Additiv/parallel zum klassischen ingest-requirements / ingest-review / ingest-apply (die bleiben unveraendert).
if (args.Length > 0 && string.Equals(args[0], "ingest-requirements-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.IngestionHitlRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "ingest-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.IngestionReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Deterministischer Apply: akzeptierte Operationen in den Core (Upsert-by-Identity) + Delta + affected-view.
if (args.Length > 0 && string.Equals(args[0], "ingest-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.IngestionApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Inc 1b: Core -> L4-Applied-Triplet, damit re-clarify (cluster->PBIs->issues) den lebenden Core konsumiert.
if (args.Length > 0 && string.Equals(args[0], "core-baseline", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreBaselineRunner.RunAsync(args, repoRoot);
    return;
}

// Inc 1c-1: Cluster (Features) + PBIs eines re-clarify-Laufs als persistente Core-Entitaeten in den Core heben.
if (args.Length > 0 && string.Equals(args[0], "core-seed-backlog", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreSeedBacklogRunner.RunAsync(args, repoRoot);
    return;
}

// Inc 1c-2: arbeitsfaehige Views auf den Core (active-backlog | archive | github-sync).
if (args.Length > 0 && string.Equals(args[0], "core-view", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreViewRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.1: PBI<->Issue-Mapping als Core-Relation (implemented_by_issue) pflegen = Dedup-Basis fuer den
// naechsten GitHub-Delta-Lauf (nicht mehr nur Run-Artefakt). Deterministisch, kein LLM, kein GitHub-Call.
if (args.Length > 0 && string.Equals(args[0], "github-map", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Core.CoreGithubMapRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.2: GitHub-READ als deterministische Queries gegen einen reproduzierbaren Issue-Snapshot (kein LLM,
// kein Token). Dasselbe Verhalten bekommt der Forward-Maker (T3.3) ueber GithubReadTools. Nur lesend.
if (args.Length > 0 && string.Equals(args[0], "github-read", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubReadRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.3: Forward-Maker — github-sync-Delta gegen GitHub. Deterministischer Vorfilter + agentischer Rest
// (Suche -> LINK/CREATE) -> GithubForwardPlan -> Gate (inkl. Rev-3-Invariante). Kein Write (Review/Apply folgen).
if (args.Length > 0 && string.Equals(args[0], "github-forward", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardRunner.RunAsync(args, settings, repoRoot);
    return;
}

// S1 (Worklist 20.07): github-forward als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint.
// Additiv/parallel zum klassischen github-forward / -review / -apply (die bleiben unveraendert).
if (args.Length > 0 && string.Equals(args[0], "github-forward-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardHitlRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "github-forward-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Tor 3 / T3.4: gated Write-Apply — der EINZIGE echte GitHub-Write. Safe-by-default (dry-run ohne --execute);
// fuehrt NUR akzeptierte Ops aus und schreibt das Mapping (implemented_by_issue, T3.1) in den Core zurueck.
if (args.Length > 0 && string.Equals(args[0], "github-forward-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.5: Reverse GitHub-Feedback-Ingestion (E4). GitHub-Zustand wird NIE auto-Wahrheit — geschlossenes Issue
// erzeugt einen gepruefsten StateChange-Vorschlag; `done` entsteht NUR nach menschlicher Verifikation. Maker/Review/Apply.
if (args.Length > 0 && string.Equals(args[0], "github-reverse", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubReverseRunner.RunAsync(args, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "github-reverse-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubReverseReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "github-reverse-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubReverseApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.6: Vergleichspfad-Harness — deterministischer Matcher (und optional ein Agent-Plan) gegen Drift-Fixtures
// mit Gold-Labels; misst Dedup-Recall/Precision (wo schlaegt der Agent den Keyword-Matcher, wo reicht Determinismus).
if (args.Length > 0 && string.Equals(args[0], "github-forward-compare", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardCompareRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 3 / T3.7: Sprint-Re-Run-Test — derselbe Delta zweimal durch den Forward-Vorfilter. Beweis: der zweite Lauf
// erzeugt KEINE Duplikat-Issues (alles gemappt -> UPDATE/HOLD). Deterministisch, kein LLM, kein GitHub, kein Core.
if (args.Length > 0 && string.Equals(args[0], "github-forward-rerun-test", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Tor3.GithubForwardRerunTest.RunAsync(args, repoRoot);
    return;
}

// Tor 2 / T2.1: Decision-Ingestion (deterministischer Kern). Stakeholder-Auflösung einer Open Decision -> Core
// auflösen (DEC resolved, contradicts->contradicts_resolved) + betroffene PBIs entblocken. Maker/Review/Apply.
if (args.Length > 0 && string.Equals(args[0], "decision-resolve", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionResolveRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 2 / T2.2: agentischer Resolver — freie Stakeholder-Antwort -> strukturierte Auflösung; danach dieselbe
// deterministische T2.1-Kette (Derivation/Gate/Apply). Der 4. agentische Knoten.
if (args.Length > 0 && string.Equals(args[0], "decision-resolve-agent", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionResolveAgentRunner.RunAsync(args, settings, repoRoot);
    return;
}

// S4 (Worklist 20.07): decision (Tor 2) als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
// Additiv/parallel zum klassischen decision-resolve(-agent) / -review / -apply (die bleiben unveraendert).
if (args.Length > 0 && string.Equals(args[0], "decision-resolve-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionHitlRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "decision-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "decision-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Tor 2 / T2.3: Kreis-Test (Tor 2 -> Tor 3). Blockiertes PBI -> decision-apply -> github-forward-Seed sieht
// UPDATE/CREATE statt HOLD_BLOCKED. Deterministisch, kein LLM, kein GitHub, kein echter Core.
if (args.Length > 0 && string.Equals(args[0], "decision-unblock-test", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Decision.DecisionUnblockTest.RunAsync(args, repoRoot);
    return;
}

// Inc 1c-3: incrementeller PBI-Update (affected-view/Delta -> nur betroffene PBIs). Maker / Review / Apply.
if (args.Length > 0 && string.Equals(args[0], "pbi-update", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate.PbiUpdateRunner.RunAsync(args, settings, repoRoot);
    return;
}
// S4 (Worklist 20.07): pbi-update als EIN MAF-Lauf mit MAF-nativem Human-Gate (RequestPort) + Checkpoint + UI.
// Additiv/parallel zum klassischen pbi-update / -review / -apply (die bleiben unveraendert).
if (args.Length > 0 && string.Equals(args[0], "pbi-update-hitl", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate.PbiUpdateHitlRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0 && string.Equals(args[0], "pbi-update-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate.PbiUpdateReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}
if (args.Length > 0 && string.Equals(args[0], "pbi-update-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.PbiUpdate.PbiUpdateApplyRunner.RunAsync(args, repoRoot);
    return;
}

// L4-v1: Project State -> kanonische Requirements-Baseline + Traceability-Projektionen.
if (args.Length > 0 && string.Equals(args[0], "l4-baseline", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4BaselineRunner.RunAsync(args, repoRoot);
    return;
}

// L4 ConsolidationPlan: Seed/Check fuer agentische Konsolidierung mit deterministischem Gate.
if (args.Length > 0 && string.Equals(args[0], "l4-consolidation", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4ConsolidationRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Human-Review: ConsolidationPlan-Operationen mit generischer HumanReview-UI autorisieren.
if (args.Length > 0 && string.Equals(args[0], "l4-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4ReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Apply: freigegebenen ConsolidationPlan deterministisch zur kanonischen Requirements-Baseline anwenden.
if (args.Length > 0 && string.Equals(args[0], "l4-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4ApplyRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Quality: kanonische Baseline auf Rueckfuehrbarkeit und Operationalisierbarkeit pruefen.
if (args.Length > 0 && string.Equals(args[0], "l4-quality", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4QualityRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Requirements Document: kanonische Baseline deterministisch als RE-Dokument rendern.
if (args.Length > 0 && string.Equals(args[0], "l4-requirements-doc", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.RequirementsDocumentRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Completion: Adequacy-Feedback + kontrollierte DISK/OpenDecision-Proposals vor Readiness/IssuePlanning.
if (args.Length > 0 && string.Equals(args[0], "l4-completion", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4CompletionRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Completion Human-Review: Completion-Proposals mit generischer HumanReview-UI autorisieren.
if (args.Length > 0 && string.Equals(args[0], "l4-completion-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4CompletionReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Completion Apply: akzeptierte Completion-Proposals deterministisch in einen erweiterten L4-Stand uebernehmen.
if (args.Length > 0 && string.Equals(args[0], "l4-completion-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.L4CompletionApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Requirements Readiness: L4-Baseline + Quality deterministisch fuer Issue Planning vorbereiten.
if (args.Length > 0 && string.Equals(args[0], "requirements-readiness", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.RequirementsReadinessRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Issue Planning: plan-only Agentenknoten auf Readiness-gefiltertem Input; kein GitHub-Write.
if (args.Length > 0 && string.Equals(args[0], "l4-issuplanning", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.IssuePlanningRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Issue Planning Human-Review: IssuePlanItems mit generischer HumanReview-UI autorisieren.
if (args.Length > 0 && string.Equals(args[0], "l4-issuplanning-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.IssuePlanningReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Issue Planning Apply: Human-Decisions deterministisch zu accepted-issue-plan materialisieren.
if (args.Length > 0 && string.Equals(args[0], "l4-issuplanning-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.IssuePlanningApplyRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Re-Clarify (RE Backlog Structuring): kanonische Requirements -> Feature-Cluster -> Product Backlog.
// `cluster` = agentische Feature-Cluster-Bildung (Maker + Coverage-Gate + ReviewAgent).
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Re-Clarify Cluster Review: HumanReview der vorgeschlagenen Cluster-Korrekturen (Operationen).
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyClusterReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Re-Clarify Apply: akzeptierte Cluster-Operationen deterministisch anwenden + Coverage-Recheck.
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyClusterApplyRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Re-Clarify Backlog Review: HumanReview der Product Backlog Items.
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-backlog-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyBacklogReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L4 Re-Clarify Backlog Apply: akzeptierte/edited PBIs deterministisch als ProductBacklogView materialisieren.
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-backlog-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyBacklogApplyRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Re-Clarify IssuePlan: ProductBacklogView -> accepted-issue-plan (deterministisch, pbiId primaer).
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-issueplan", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyBacklogIssuePlanRunner.RunAsync(args, repoRoot);
    return;
}

// L4 Re-Clarify Backlog Doc: lesbare Markdown-Projektion des Product Backlog.
if (args.Length > 0 && string.Equals(args[0], "l4-re-clarify-backlog-doc", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ReClarifyBacklogDocRunner.RunAsync(args, repoRoot);
    return;
}

// GitHub Reconciliation: accepted IssuePlan -> plan-only GitHubActionPlan; kein GitHub-Write.
if (args.Length > 0 && string.Equals(args[0], "github-reconciliation", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.GithubReconciliationRunner.RunAsync(args, settings, repoRoot);
    return;
}

// GitHub Reconciliation Human-Review: GitHubActionPlanItems mit generischer HumanReview-UI autorisieren.
if (args.Length > 0 && string.Equals(args[0], "github-reconciliation-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.GithubReconciliationReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// GitHub Reconciliation Apply: Human-Decisions deterministisch zu accepted-github-action-plan materialisieren.
if (args.Length > 0 && string.Equals(args[0], "github-reconciliation-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.GithubReconciliationApplyRunner.RunAsync(args, repoRoot);
    return;
}

// GitHub Snapshot: read-only GitHub Issues in ein reproduzierbares Reconciliation-Input-Format normalisieren.
if (args.Length > 0 && string.Equals(args[0], "github-snapshot", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.GithubIssueSnapshotRunner.RunAsync(args, repoRoot);
    return;
}

// GitHub Write: deterministischer Dry-Run ueber akzeptiertem GitHubActionPlan; keine echten GitHub-Writes.
if (args.Length > 0 && string.Equals(args[0], "github-write", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.GithubWriteDryRunRunner.RunAsync(args, repoRoot);
    return;
}

// Operationalization Audit: prueft Traceability von L4/Readiness bis GitHub-Dry-Run vor echten Writes.
if (args.Length > 0 && string.Equals(args[0], "operationalization-audit", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.OperationalizationAuditRunner.RunAsync(args, repoRoot);
    return;
}

// Open Requirements Review: klassifiziert nicht operationalisierte Requirements fuer Klaerungsarbeit.
if (args.Length > 0 && string.Equals(args[0], "open-requirements-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.OpenRequirementsReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Open Requirements Apply: materialisiert Review-Entscheidungen zu ClarificationPlanningInput.
if (args.Length > 0 && string.Equals(args[0], "open-requirements-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.OpenRequirementsApplyRunner.RunAsync(args, repoRoot);
    return;
}

// Clarification Agent: offene Requirements plan-only als Klaerungs-/Breakdown-Arbeit weiterfuehren; Resolve-Modus ist als Contract vorbereitet.
if (args.Length > 0 && string.Equals(args[0], "clarification-agent", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ClarificationAgentRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Clarification Agent Human-Review: ClarificationPlanItems mit generischer HumanReview-UI autorisieren.
if (args.Length > 0 && string.Equals(args[0], "clarification-agent-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ClarificationPlanningReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Clarification Agent Apply: Human-Decisions deterministisch zu accepted-clarification-plan materialisieren.
if (args.Length > 0 && string.Equals(args[0], "clarification-agent-apply", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4.ClarificationPlanningApplyRunner.RunAsync(args, repoRoot);
    return;
}

// A2 (Demonstration): Nicht-dekorativ-Beleg — supported-Rate der ledger-geerdeten requirements.md gegen den consumable.
if (args.Length > 0 && string.Equals(args[0], "ledger-cite-fidelity", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Fidelity.LedgerCiteFidelityRunner.RunAsync(args, settings, repoRoot);
    return;
}

// I-d: Human-Review der abgeleiteten Risiken (generisches HumanReview-UI) -> approved-derived-risks.json.
if (args.Length > 0 && string.Equals(args[0], "derive-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation.DerivedRisksReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

// I-c: Inference-Checker — semantischer Relevanz-/Nicht-Widerspruchs-Check der abgeleiteten Risiken gegen ihre Anker.
if (args.Length > 0 && string.Equals(args[0], "inference-check", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation.InferenceCheckRunner.RunAsync(args, settings, repoRoot);
    return;
}

// I-b: erster Derivation-Agent — leitet aus der geprüften Requirements-Baseline neue, verankerte Risiken ab.
if (args.Length > 0 && string.Equals(args[0], "derive-risks", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Derivation.DerivedRisksRunner.RunAsync(args, settings, repoRoot);
    return;
}

// E-d: Fan-out des Ledgers auf mehrere Artefakt-Zweige (parallel) -> Fan-in-Barrier -> Verified Baseline Set.
if (args.Length > 0 && string.Equals(args[0], "baseline-fanout", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.FanOut.BaselineFanOutRunner.RunAsync(args, settings, repoRoot);
    return;
}

// E-c: EIN komponierter Artefakt-Zweig (EvidenceBaselineAgent -> [CheckerRepair via BindAsExecutor] -> AssignIds).
if (args.Length > 0 && string.Equals(args[0], "artifact-branch", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Branch.ArtifactBranchRunner.RunAsync(args, settings, repoRoot);
    return;
}

// I-a: deterministisches ID-Gate — geprüftes Baseline-Artefakt -> ArtifactDocument mit stabilen Item-IDs (artifact.json).
if (args.Length > 0 && string.Equals(args[0], "assign-artifact-ids", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.Artifacts.ArtifactIdGateRunner.RunAsync(args, settings, repoRoot);
    return;
}

// MC1/C3: eigenständiger Checker-Repair als echter MAF-Workflow (Checker -> [Repair-Loop] -> Finalize).
// Quell-generisch, per BindAsExecutor als Knoten hinter jeden Generator-Agenten einhängbar (Kapitel C).
if (args.Length > 0 && string.Equals(args[0], "checker-repair-workflow", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.MakerChecker.Workflow.CheckerRepairRunner.RunAsync(args, settings, repoRoot);
    return;
}

// L3-B: Selective-Metrics für den FacetValidator gegen die autor-bestätigte Fixture (correct vs perturbed).
if (args.Length > 0 && string.Equals(args[0], "facet-validation-eval", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.FacetValidationEvalRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Ledger-Referenz-Recall (DETERMINISTISCH, kein LLM): Segment-Overlap-Screen, gratis. Misses verlässlich.
if (args.Length > 0 && string.Equals(args[0], "ledger-reference-recall-fast", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerReferenceRecallFastRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Ledger-Referenz-Recall: misst, ob ein Auto-Ledger die Claims einer Referenz findet (Completeness/(B)-Frage).
if (args.Length > 0 && string.Equals(args[0], "ledger-reference-recall", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Ledger.LedgerReferenceRecallRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0)
{
    Console.Error.WriteLine($"Unknown command '{args[0]}'. Der normale Phase-Runner startet nur ohne CLI-Command.");
    // Gruppiert (S5, Worklist 20.07) — vollstaendige Landkarte s. Banner am Dispatch-Anfang. Alle bleiben aufrufbar.
    Console.Error.WriteLine("[LIVE] core-seed, core-view, project-state-build, ingest-requirements(-hitl), ingest-review, ingest-apply, "
        + "pbi-update(-hitl), pbi-update-review, pbi-update-apply, decision-resolve(-agent|-hitl), decision-review, decision-apply, "
        + "github-forward(-hitl), github-forward-review, github-forward-apply, github-reverse(-review|-apply), github-snapshot, github-read.");
    Console.Error.WriteLine("[SUPPORT/L4] l4-baseline, l4-consolidation, l4-review, l4-apply, l4-quality, l4-requirements-doc, "
        + "l4-completion(-review|-apply), requirements-readiness, l4-issuplanning(-review|-apply), clarification-agent(-review|-apply), "
        + "open-requirements-review, open-requirements-apply, operationalization-audit.");
    Console.Error.WriteLine("[FROZEN/Historie — nur Referenz] l3(-review|-apply|-revise), ledger-build, ledger-adjudicate-ui, ledger-validate, "
        + "semantic-ledger-extract, eval-offline, review, review-agent, contract-check, recipe, derive, evidence-chain, "
        + "github-reconciliation(-review|-apply), spike-hitl (u.a. — s. Banner).");
    Environment.ExitCode = 2;
    return;
}

var runId = RunId.New();
// Output-Ordner unter runs/. Logisch bleibt es Phase 2.1 (AgentPhase); die Strategie-Varianten
// bekommen aber eigene Unterordner, damit A/B/C-Runs auf der Platte getrennt liegen
var run = new RunContext(runId, ResolveRunFolder(settings));
run.EnsureFolders();
CleanDocsFolder();

// runs/<folder>/<runId>. Strategie B/C erhalten eigene Ordner.. A bleibt aus Historie-Gruenden in phase2_1.
static string ResolveRunFolder(HostSettings settings)
    => settings.AgentPhase switch
    {
        // phase2_1/2B/2C-Ordnerwahl archiviert (Move 3, 2026-07-22).
        // Kapitel B (Evidenz-Agent): runs/phase2evidenz-agent/<arm>/<runId> — Arme (ledger|transcript) getrennt.
        "phase2_evidence" => $"phase2evidenz-agent/{settings.EvidenceSource}",
        _ => settings.AgentPhase
    };

var sourceName = "AgenticSdlc.Host";
var activitySource = new ActivitySource(sourceName);
var tracesPath = Path.Combine(run.LogsDir, "otel-traces.jsonl");
var rawTracesPath = Path.Combine(run.LogsDir, "otel-traces.raw.jsonl");
var metricsPath = Path.Combine(run.LogsDir, "otel-metrics.jsonl");

// Providers leben bis Run-Ende:
using var otel = AgenticSdlc.Host.Observability.OtelRunExporters.TryCreate(
    enabled: settings.OtelEnabled,
    sourceName: sourceName,
    tracesPath: tracesPath,
    metricsPath: metricsPath,
    rawTracesPath: settings.OtelRawEnabled ? rawTracesPath : null
);

// Code-Stand-Stempel: gegen welchen Git-Commit lief dieser Run, war der Arbeitsbaum dirty?
// Macht run -> Code rekonstruierbar, ohne pro Run committen zu müssen (siehe commit-rules.md)
var codeVersion = AgenticSdlc.Host.Run.GitStamp.Capture(run, repoRoot);
if (codeVersion.Available)
{
    var state = codeVersion.Dirty
        ? $"DIRTY ({codeVersion.ChangedFiles} Datei(en); Diff -> {codeVersion.TrackedDiffFile})"
        : "clean";
    Console.WriteLine($"[git] {codeVersion.ShortCommit} @ {codeVersion.Branch} — {state}");
}

var config = new
{
    runId,
    phase = ResolvePhaseName(settings.AgentPhase),
    phaseSelector = settings.AgentPhase,
    // Code-Stand dieses Runs (Commit/Branch/dirty). Bei dirty liegt der Diff unter code-version/.
    codeVersion,
    phase2ContextStrategy = settings.AgentPhase == "phase2_1" ? settings.Phase2ContextStrategy : null,
    prompts = ResolvePromptConfig(settings),
    llmProvider = settings.LlmProvider,
    model = settings.ModelId,
    observability = new
    {
        otelEnabled = settings.OtelEnabled,
        otelSensitive = settings.OtelSensitive,
        otelRawEnabled = settings.OtelRawEnabled,
        innerCycleLogging = settings.InnerCycleLogging
    },
    llmPreview = new
    {
        chars = settings.LlmPreviewChars,
    },
    // jury-Config archiviert mit Evaluation (Move 3, 2026-07-22) — Jury war S-3-Messinstrument.
    ollamaBaseUrl = settings.OllamaBaseUrl,
    openRouterBaseUrl = settings.LlmProvider == "openrouter" ? settings.OpenRouterBaseUrl : null,
    timestampUtc = DateTime.UtcNow
};
run.WriteConfig(config);

WriteRunChangeNote(run);

//unter changes.txt immer nachvollziehbar warum der Run gestartet wurde "welche neuerungen"
static void WriteRunChangeNote(RunContext run)
{
    Console.WriteLine();
    Console.WriteLine("Run change note (what is new in this run?).");
    Console.WriteLine("Type a short note and press Enter. Leave empty for 'nichts neues in diesem Run'.");
    Console.Write("> ");

    var note = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(note))
        note = "nichts neues in diesem Run";

    var content =
        $"""
         runId: {run.RunId} timestampUtc: {DateTime.UtcNow:O}
        {note}
        """;

    // Speichern unter runs/<phase>/<runId>/logs/changes.txt
    File.WriteAllText(run.ChangesPath, content);

    //auch als Event (damit es in events.jsonl auffindbar ist)
    run.AppendEvent(new { type = "RUN_CHANGE_NOTE", runId = run.RunId, note, timestampUtc = DateTime.UtcNow });
}

// "phase1" / "phase2_1" sind archiviert (archive/phase1, archive/phase2-dag, archive/phase2b — Move 3, 2026-07-22).
// Lauffaehige Historie: git checkout v-s0-phase1 / v-s1-phase2-dag / v-s3-evaluator-review.
Environment.ExitCode = settings.AgentPhase switch
{
    "phase2_evidence" => await RunPhase2EvidenceAsync(),
    _ => UnknownPhase(settings.AgentPhase, run)
};

// Kapitel B (Evidenz-Agent): eigener, DÜNNER Runner; komponiert die vorhandenen Bausteine
// (Phase2AgentFactory / Pipeline), lässt Phase2Runner unangetastet. E0 = Gerüst-Durchstich.
async Task<int> RunPhase2EvidenceAsync()
{
    var runner = new AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.EvidenceAgentRunner(
        settings: settings,
        run: run,
        sourceName: sourceName,
        activitySource: activitySource,
        repoRoot: repoRoot
    );

    return await runner.RunAsync();
}

static int UnknownPhase(string phase, RunContext run)
{
    run.AppendEvent(new
    {
        type = "RUN_FAILED",
        runId = run.RunId,
        reason = "Unknown AGENT_PHASE.",
        phase,
        allowedPhases = new[] { "phase2_evidence" },
        timestampUtc = DateTime.UtcNow
    });

    Console.Error.WriteLine($"RUN FAILED - Unknown AGENT_PHASE '{phase}'. Allowed: phase2_evidence. (phase1/phase2_1 archiviert -> Tags v-s0/v-s1/v-s3.)");
    return 4;
}

static string ResolvePhaseName(string phase)
    => phase; // phase1/phase2_1-Namensaufloesung archiviert (Move 3); einzig verbliebene Phase: phase2_evidence.

static object ResolvePromptConfig(HostSettings settings)
{
    // phase1/phase2_1-Prompt-Konfig archiviert (Move 3). Verhalten fuer phase2_evidence unveraendert (wie vorher Fallback).
    return new { unknownPhase = settings.AgentPhase };
}

//docs muss vor jedem run "geleert" werden damit keine alten Daten ausversehen bleiben oder sich etwas vermischt.
static void CleanDocsFolder()
{
    var docsDir = "docs";

    if (!Directory.Exists(docsDir))
        return;

    var files = Directory.GetFiles(docsDir, "*.md", SearchOption.TopDirectoryOnly);

    foreach (var file in files)
    {
        var name = Path.GetFileName(file);

        if (name.Equals(".gitkeep", StringComparison.OrdinalIgnoreCase))
            continue;

        File.Delete(file);
    }
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);

    for (int i = 0; i < 10 && dir is not null; i++)
    {
        var hasGit = Directory.Exists(Path.Combine(dir.FullName, ".git"));
        var hasSlnx = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.slnx"));
        var hasSln = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.sln"));

        if (hasGit || hasSlnx || hasSln)
            return dir.FullName;

        dir = dir.Parent;
    }

    return Directory.GetCurrentDirectory();
}
