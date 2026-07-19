using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.Observability;
using AgenticSdlc.Host.Phases.Phase1;
using AgenticSdlc.Host.Phases.Phase2;
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

// Offline-Evaluator (isolierter Bewertungs-Pfad): bewertet ein bestehendes Artefakt mit dem
// Evaluator, ohne neuen Generierungs-Run / Workflow / Run-Ordner / Change-Note
// ziel: Re-Scoren alter Runs oder zum Testen eines anderen Judge-Modells (evtl später weg..)
if (args.Length > 0 && string.Equals(args[0], "eval-offline", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.OfflineEvaluatorRunner.RunAsync(args, settings, repoRoot);
    return;
}

// DISK-14 Phase 1 (additiv, isoliert): Artefakte deterministisch in Pruefeinheiten zerlegen (kein LLM).
// Verwerfen des Per-Item-Ansatzes = PerItem-Ordner loeschen + diese Zeile entfernen.
if (args.Length > 0 && string.Equals(args[0], "parse-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.UnitParseRunner.RunAsync(args, repoRoot);
    return;
}

// DISK-14 Phase 2 (additiv, isoliert): Per-Item-Klassifikation → paralleler GroundingScore (echter LLM-Call)
// Verwerfen = PerItem-Ordner löschen + diese Zeile entfernen.
if (args.Length > 0 && string.Equals(args[0], "classify-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClassifyUnitsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// DISK-14 MISSING/Coverage-Achse (additiv, isoliert): Transkript-Turns -> covered/missing je Artefakt
if (args.Length > 0 && string.Equals(args[0], "coverage-units", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.CoverageRunner.RunAsync(args, settings, repoRoot);
    return;
}

// PILOTtest (isoliert): echter MAF-Agent als Reviewer (vs. post-hoc IEvaluator). Verwerfen = ReviewAgent-Ordner + diese Zeile.
if (args.Length > 0 && string.Equals(args[0], "review-agent", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.ReviewAgent.ReviewAgentRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Z4.2-light (additiv, isoliert, kein LLM): achsen-spezifische *.review.json eines Runs zu EINEM mergen.
if (args.Length > 0 && string.Equals(args[0], "merge-review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.Review.ReviewMergeRunner.RunAsync(args, repoRoot);
    return;
}

// Z6.2 (DISK-COV): Topics eines Transkripts EINMAL extrahieren (LLM) → eingefrorene Fixture input/topics/.
if (args.Length > 0 && string.Equals(args[0], "extract-topics", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ExtractTopicsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Z6.3 (DISK-COV): topic-basierte Coverage gegen ein Artefakt → <base>.topic-coverage.<model>.review.json.
if (args.Length > 0 && string.Equals(args[0], "coverage-topics", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.CoverageTopicsRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Z7 (DISK-COV-2, kein LLM): Topic-Fixture-Audit (formale Checks + Jury-Cross-Check/Capture-Recapture).
if (args.Length > 0 && string.Equals(args[0], "topic-audit", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.TopicAuditRunner.RunAsync(args, repoRoot);
    return;
}

// Z8 (DISK-COV-5): begrenzter Topic-Completeness-Verifier (1 LLM-Pass) → input/topics/<base>.verify-candidates.json.
if (args.Length > 0 && string.Equals(args[0], "topic-verify", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.TopicVerifyRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Z10 / v02 (Call 2, separat): relevantFor je Topic separat klassifizieren → Sidecar
// input/topics/<base>.relevance.<model>.json (Frozen-Fixture bleibt unberührt). Test B: v01 vs v02.
if (args.Length > 0 && string.Equals(args[0], "classify-topic-relevance", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClassifyTopicRelevanceRunner.RunAsync(args, settings, repoRoot);
    return;
}

// D1 / R0: DirectTranscriptReview — Artefakt direkt gegen Roh-Transkript (ohne Fixture) → thesis-evidence/.
if (args.Length > 0 && string.Equals(args[0], "review-direct", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ReviewDirectRunner.RunAsync(args, settings, repoRoot);
    return;
}

// B47-Spike: lokaler Claim-Evidence-Grounding-Test (isoliert, nicht Teil der produktiven GroundingAxis).
if (args.Length > 0 && string.Equals(args[0], "claim-grounding-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClaimGroundingSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Claim-Pilot Stufe 2: END-TO-END (Claim → AUTO-EvidenceSelector → Verifier). Testet den Engpass Evidence-Auswahl.
if (args.Length > 0 && string.Equals(args[0], "claim-evidence-e2e-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClaimEvidenceE2ESpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Claim-Pilot Stufe 3: ClaimSplitter (Unit → atomare Claims); mit Transkript volle Kette split→select→verify.
if (args.Length > 0 && string.Equals(args[0], "claim-split-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ClaimSplitSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Spike: source-native Claims gegen Artefakt prüfen (Quelle -> Artefakt), isoliert.
if (args.Length > 0 && string.Equals(args[0], "source-claim-coverage-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimCoverageSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Matrix-Spike: kuratierte SourceClaims gegen alle Artefakte pruefen; not_applicable bleibt sichtbar.
if (args.Length > 0 && string.Equals(args[0], "source-claim-coverage-matrix", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimCoverageMatrixRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Matrix-v2-Spike: Applicability und Coverage getrennt gegen adjudizierte Matrix pruefen.
if (args.Length > 0 && string.Equals(args[0], "source-claim-coverage-matrix-v2", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimCoverageMatrixV2Runner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Matrix-Batch-Spike: alle SourceClaims gegen ein Artefakt in einem Call pruefen.
if (args.Length > 0 && string.Equals(args[0], "source-claim-coverage-matrix-batch", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimCoverageMatrixBatchRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Evidence-first Semantic-Ledger-Spike: bestaetigte Quellsemantik -> traceable Artefakt -> lokale Verifikation.
if (args.Length > 0 && string.Equals(args[0], "evidence-first-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.EvidenceFirstSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Human-Artefakt-Spike: evidence-first claims.json -> lesbares Markdown mit sichtbaren SourceClaim-Refs.
if (args.Length > 0 && string.Equals(args[0], "human-artifact-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.HumanArtifactSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Human-Artefakt-Projektion: claims.json -> Markdown entlang sichtbarer SourceClaim-Refs deterministisch pruefen.
if (args.Length > 0 && string.Equals(args[0], "human-artifact-projection", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.HumanArtifactProjectionRunner.RunAsync(args, repoRoot);
    return;
}

// Semantic-Ledger-Extraction-Spike: Transkript -> facettierter Ledger -> Recall gegen bestaetigte Fixture.
if (args.Length > 0 && string.Equals(args[0], "semantic-ledger-extraction-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SemanticLedgerExtractionSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Semantic-Ledger-Extract (NUR Extraktion+Canonicalization, keine Fixture/kein Match):
// Kandidaten-Ledger fuer ein NEUES Transkript erzeugen, aus dem dann manuell eine Fixture bestaetigt wird.
if (args.Length > 0 && string.Equals(args[0], "semantic-ledger-extract", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SemanticLedgerExtractRunner.RunAsync(args, settings, repoRoot);
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

// Coverage-Spike Stufe 2: Transcript -> auto SourceClaims -> Recall + E2E Coverage gegen Fixture, isoliert.
if (args.Length > 0 && string.Equals(args[0], "source-claim-extraction-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimExtractionSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Spike Stufe 3: Transcript einmal -> globaler SourceClaim-Ledger -> Recall + E2E Coverage, isoliert.
if (args.Length > 0 && string.Equals(args[0], "source-claim-ledger-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.GlobalSourceClaimExtractionSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Spike Stufe 4: Global SourceClaim Ledger -> ArtifactObligation -> Recall/Kandidatenreduktion, isoliert.
if (args.Length > 0 && string.Equals(args[0], "source-claim-obligation-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ArtifactObligationSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Spike Stufe 5: Transcript einmal -> artefaktspezifische SourceObligations -> Recall, isoliert.
if (args.Length > 0 && string.Equals(args[0], "source-obligation-extraction-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceObligationExtractionSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// Coverage-Spike Stufe 6: Global SourceClaim Ledger -> konservative Selection -> Recall/Kandidatenreduktion.
if (args.Length > 0 && string.Equals(args[0], "source-claim-selection-spike", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.SourceClaimSelectionSpikeRunner.RunAsync(args, settings, repoRoot);
    return;
}

// C1: zusammengesetzter Review (Grounding + Coverage) → EIN ReviewResult pro Artefakt (offline, ReviewService).
if (args.Length > 0 && string.Equals(args[0], "review", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await AgenticSdlc.Host.Phases.Phase2.Evaluation.PerItem.ReviewRunner.RunAsync(args, settings, repoRoot);
    return;
}

if (args.Length > 0)
{
    Console.Error.WriteLine($"Unknown command '{args[0]}'. Der normale Phase-Runner startet nur ohne CLI-Command.");
    Console.Error.WriteLine("Beispiele: l3, l3-review, l3-apply, l3-revise, project-state-build, core-seed, ingest-requirements, ingest-review, ingest-apply, core-baseline, core-seed-backlog, core-view, l4-baseline, l4-consolidation, l4-review, l4-apply, l4-quality, l4-requirements-doc, l4-completion, l4-completion-review, l4-completion-apply, requirements-readiness, l4-issuplanning, l4-issuplanning-review, l4-issuplanning-apply, github-reconciliation, github-reconciliation-review, github-reconciliation-apply, github-snapshot, github-write, operationalization-audit, open-requirements-review, open-requirements-apply, clarification-agent, clarification-agent-review, clarification-agent-apply, ledger-build, ledger-adjudicate-ui, review.");
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
        "phase2_1" => settings.Phase2ContextStrategy switch
        {
            "artifact_state" => "phase2B",
            "independent_source_reads" => "phase2C",
            _ => "phase2_1"   // message_passing (A)
        },
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
    jury = new
    {
        enabled = settings.JuryEnabled,
        judgeModel = settings.JuryJudgeModel,
        structuredOutput = settings.JuryStructuredOutput,
        // DISK-12/G3: Call-1 pro Kategorie gesplittet (Mess-Instrument-Parameter, einfrieren für Vergleiche).
        splitGeneration = settings.JurySplitGeneration,
        // DISK-12/B22: effektive aktive Kategorien je Artefakttyp (Default-Profil + run-config-Override).
        categories = new AgenticSdlc.Host.Phases.Phase2.Evaluation.JuryCategoryProfile(
            settings.JuryCategoriesByArtifact).Describe(),
        // DISK-7: aktive Verifikations-Policy pro Kategorie (für reproduzierbare A/B/C-Vergleiche).
        verification = new
        {
            falseClaim = settings.JuryVerifyFalseClaim,
            falseCertainty = settings.JuryVerifyFalseCertainty,
            missingTopic = settings.JuryVerifyMissingTopic,
            custom = settings.JuryVerifyCustom,
            // DISK-9: Batch-Limit des MISSING_TOPIC-Verifiers (Mess-Instrument-Parameter, einfrieren für Vergleiche).
            missingTopicBatchSize = settings.JuryMissingTopicBatchSize
        }
    },
    // Nur für Phase 2.1B (artifact_state) relevant: dokumentiert die aktive Shared-State-Policy.
    phase2BState = (settings.AgentPhase == "phase2_1" && settings.Phase2ContextStrategy == "artifact_state")
        ? new
        {
            writeArtifacts = settings.Phase2BWriteArtifacts,
            reads = settings.Phase2BReads
        }
        : null,
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

Environment.ExitCode = settings.AgentPhase switch
{
    "phase1" => await RunPhase1Async(),
    "phase2_1" => await RunPhase2_1Async(),
    "phase2_evidence" => await RunPhase2EvidenceAsync(),
    _ => UnknownPhase(settings.AgentPhase, run)
};

async Task<int> RunPhase1Async()
{
    var runner = new Phase1Runner(
        settings: settings,
        run: run,
        sourceName: sourceName,
        activitySource: activitySource,
        repoRoot: repoRoot
    );

    return await runner.RunAsync();
}

async Task<int> RunPhase2_1Async()
{
    var runner = new Phase2Runner(
        settings: settings,
        run: run,
        sourceName: sourceName,
        activitySource: activitySource,
        repoRoot: repoRoot
    );

    return await runner.RunAsync();
}

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
        allowedPhases = new[] { "phase1", "phase2_1", "phase2_evidence" },
        timestampUtc = DateTime.UtcNow
    });

    Console.Error.WriteLine($"RUN FAILED - Unknown AGENT_PHASE '{phase}'. Allowed values: phase1, phase2_1, phase2_evidence.");
    return 4;
}

static string ResolvePhaseName(string phase)
    => phase switch
    {
        "phase1" => Phase1Artifacts.PhaseName,
        "phase2_1" => Phase2Artifacts.PhaseName,
        _ => phase
    };

static object ResolvePromptConfig(HostSettings settings)
{
    if (settings.AgentPhase == "phase1")
    {
        return new
        {
            phase1 = new
            {
                agent = "Phase1SinglePass",
                promptName = settings.GetPromptName("Phase1SinglePass")
            }
        };
    }

    if (settings.AgentPhase == "phase2_1")
    {
        return new
        {
            phase2_1 = new
            {
                contextStrategy = settings.Phase2ContextStrategy,
                agents = new
                {
                    context = settings.GetPromptName(Phase2AgentFactory.ContextAgentName),
                    requirements = settings.GetPromptName(Phase2AgentFactory.RequirementsAgentName),
                    risks = settings.GetPromptName(Phase2AgentFactory.RisksAgentName),
                    architecture = settings.GetPromptName(Phase2AgentFactory.ArchitectureAgentName),
                    openQuestions = settings.GetPromptName(Phase2AgentFactory.OpenQuestionsAgentName)
                }
            }
        };
    }

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
