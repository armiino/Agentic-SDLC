using AgenticSdlc.Host.FullWorkflow.Backlog;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.PbiUpdate;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow.Pipeline;

// U2: Knoten-Gruppen, damit Assemble lesbar bleibt (je Gruppe eine Zeile im Aufrufer).
internal sealed record FrontNodes(
    PipelineEntryExecutor Entry,
    ClarifyEntryExecutor ClarifyEntry, // A′ Schritt 2: dritter typisierter Eingang (clarify-Batch → pbiFinalize)
    ReprojectEntryExecutor ReprojectEntry, // ONLY-STEWARD: vierter Eingang (Recovery) — Core→Delta → Forward-Schwanz
    LedgerIntakeExecutor LedgerIntake, ExecutorBinding LedgerCapsule, LedgerSummaryExecutor LedgerSummary, // Schritt 5 ②: sichtbare Kapsel statt Wrapper
    AdjudicationGateRequestExecutor AdjudicationRequest, RequestPort AdjudicationPort, AdjudicationApplyExecutor AdjudicationApply, AdjudicationEmptyGateResponder AdjudicationEmptyGate, // H2 + R-50
    AdjudicationRefineExecutor AdjudicationRefine, // A10: pending-Gap-Claims facettieren (geteilte Naht, No-op ohne pending)
    BaselineStageExecutor Baselines,
    ProjectStateBuildExecutor Delta, BranchDetectorExecutor Branch);

internal sealed record BootstrapNodes(
    CoreBootstrapStageExecutor CoreBootstrap, ClusterBridgeExecutor ClusterBridge,
    ClusterAgentExecutor ClusterAgent, ClusterGateExecutor ClusterGate, ClusterRepairExecutor ClusterRepair, ClusterReviewExecutor ClusterReview,
    ClusterFinalizeExecutor ClusterFinalize, ClusterGateRequestExecutor ClusterGateRequest, RequestPort ClusterPort,
    ClusterComposedApplyExecutor ClusterApply, ClusterEmptyGateResponder ClusterEmptyGate, BacklogBridgeExecutor BacklogBridge,
    ClarifyAgentExecutor ClarifyAgent, BacklogGateExecutor BacklogGate, BacklogRepairExecutor BacklogRepair, BacklogFinalizeExecutor BacklogFinalize,
    BacklogGateRequestExecutor BacklogGateRequest, RequestPort BacklogPort,
    BacklogComposedApplyExecutor BacklogApply, CoreSeedBacklogExecutor Seed);

internal sealed record OperationalNodes(
    IngestBridgeExecutor IngestBridge, AspectIngestionRouterExecutor AspectRouter, // R-11 A1b: Registry-Router (laut-parkend)
    IngestionHitlResolveExecutor IngestResolve, IngestionGateExecutor IngestGate, IngestionRepairExecutor IngestRepair,
    IngestionHitlFinalizeExecutor IngestFinalize, RequestPort IngestPort, IngestComposedApplyExecutor IngestApply,
    ArchIngestBridgeExecutor ArchBridge, IngestionHitlResolveExecutor ArchResolve, IngestionGateExecutor ArchGate, // R-11 A1d: arch-Strip
    IngestionRepairExecutor ArchRepair, IngestionHitlFinalizeExecutor ArchFinalize, RequestPort ArchPort, ArchComposedApplyExecutor ArchApply,
    DecisionScanExecutor DecisionScan, RequestPort DecisionPort, DecisionComposedApplyExecutor DecisionApply,
    IngestPbiBridgeExecutor Bridge,
    PbiUpdateDeriveExecutor PbiDerive, PbiUpdateMakerExecutor PbiMaker, PbiUpdateGateExecutor PbiGate,
    PbiUpdateRepairExecutor PbiRepair, PbiAlignExecutor PbiAlign, PbiUpdateHitlFinalizeExecutor PbiFinalize, RequestPort PbiPort,
    PbiUpdateApplyExecutor PbiApply,
    PbiUpdateEmptyGateResponder PbiEmptyGate,   // R-50
    IngestionEmptyGateResponder IngestEmptyGate, IngestionEmptyGateResponder ArchEmptyGate);   // R-50-Vervollständigung

// R-11 A2-2: der Klassifikations-Strip (EIN geteilter Strip, ZWEI Bahnen-Bridges — T2.22).
internal sealed record ArchClassifyNodes(
    AgenticSdlc.Host.FullWorkflow.ArchClassify.OperationalClassifyBridgeExecutor OpBridge,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.BootstrapClassifyBridgeExecutor BootBridge,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyMakerExecutor Maker,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyGateExecutor Gate,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyRepairExecutor Repair,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyFinalizeExecutor Finalize,
    RequestPort Port,
    AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyApplyExecutor Apply);

// R-11 A5 (06.08.): der ADR-Strip (EIN geteilter Strip, ZWEI Bahnen-Bridges — 10. Gate adr-gate).
internal sealed record AdrNodes(
    AgenticSdlc.Host.FullWorkflow.Adr.AdrOperationalBridgeExecutor OpBridge,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrBootstrapBridgeExecutor BootBridge,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrMakerExecutor Maker,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrGateExecutor Gate,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrRepairExecutor Repair,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrFinalizeExecutor Finalize,
    RequestPort Port,
    AgenticSdlc.Host.FullWorkflow.Adr.AdrApplyExecutor Apply);

internal sealed record ForwardNodes(
    BootstrapForwardBridgeExecutor BootstrapBridge, OperationalForwardBridgeExecutor OperationalBridge,
    SnapshotExecutor Snapshot,
    GithubForwardSeedExecutor Seed, GithubForwardMakerExecutor Maker, GithubForwardGateExecutor Gate,
    GithubForwardRepairExecutor Repair, GithubForwardHitlFinalizeExecutor Finalize, RequestPort HumanGate,
    GithubForwardApplyExecutor Apply,
    GithubForwardEmptyGateResponder EmptyGate);   // R-50

/// <summary>
/// U2 — die EINE Bauzeit-Orchestrierungsstelle der GANZEN Kette (ein-graph-vereinheitlichung §7).
/// Der komplette Graph steht hier: Front → Branch (Typ-Routing) → Bootstrap-Zweig | Betriebs-Zweig →
/// gemeinsames Forward-Ende. 7 Human-Gates (RequestPorts; decision-gate/Tor 2 seit R-14 G1), Conditional Edges, keine Orchestrierung außerhalb
/// (Design-Regel: keine CLI-Aufrufe aus Executors; der Runner startet nur noch EINEN Stream).
/// Zweig-Verdrahtung per AddTo aus den Standalone-Workflows (eine Quelle, keine Kopie).
/// </summary>
internal static class PipelineFullWorkflow
{
    public static Workflow Assemble(FrontNodes front, BootstrapNodes boot, OperationalNodes op, ArchClassifyNodes classify, AdrNodes adr, ForwardNodes fwd)
    {
        // Schritt 5 ③ (05.08.): der Start ist ein TYPISIERTER Eingangs-Dispatcher (Transkript | Delta) —
        // MAF-nativ per Input-Typ-Routing; der Delta-Einstieg dockt am existierenden BranchDetector an.
        var b = new WorkflowBuilder(front.Entry)
            .WithName("pipeline-full")
            .WithDescription("(Transkript | Delta) -> Entry -> [Ledger -> Adjudikation -> Refine -> Baselines -> Delta ->] [Branch] -> "
                           + "Bootstrap (core-bootstrap -> Cluster+Gate -> PBIs+Gate -> Seed) | Betrieb (Ingest+Gate -> Pbi+Gate) "
                           + "-> Snapshot -> Forward+Gate -> Dry-Run/Apply.");

        // Eingangs-Vertrag: Transkript -> Front | fertiges Delta -> direkt Branch (beide Bahnen via Detector) |
        // clarify-Batch -> ClarifyEntry (A′ Schritt 2). Typ-Routing: der Entry sendet je nach gesetztem Feld genau EINEN Typ.
        b.AddEdge(front.Entry, front.LedgerIntake);
        b.AddEdge(front.Entry, front.Branch);
        b.AddEdge(front.Entry, front.ClarifyEntry);
        b.AddEdge(front.Entry, front.ReprojectEntry);
        // A′ Schritt 2: ClarifyEntry erzeugt+validiert den PBI-Plan und speist bei Pass den VORHANDENEN Schwanz ab
        // pbiFinalize (-> pbiPort HUMAN -> pbiApply -> Forward). Kein neues Gate/Apply/Forward. Non-Pass = terminaler Output.
        b.AddEdge(front.ClarifyEntry, op.PbiFinalize);
        // ONLY-STEWARD Recovery: ReprojectEntry leitet das Sync-Delta AUS DEM CORE ab und emittiert dieselbe ForwardPrep
        // wie die beiden Bridges → speist den VORHANDENEN Forward-Schwanz (Snapshot -> Maker -> forward-gate -> Apply).
        b.AddEdge(front.ReprojectEntry, fwd.Snapshot);

        // Schritt 5 ② (05.08.): die Ledger-Stufe ist eine SICHTBARE gebundene Kapsel (BindGateFree) statt des
        // Wrapper-Executors mit innerem Zweitmotor — ihre 8 Schritte sind Bürger des Ein-Graphen (Checkpoints,
        // Events). Intake adaptiert TranscriptInput -> Transkript-String; der Summary-Knoten führt die geteilte
        // Auswertung (Quality-Gate/Trace) und speist die Adjudikation wie zuvor.
        b.AddEdge(front.LedgerIntake, front.LedgerCapsule);
        b.AddEdge(front.LedgerCapsule, front.LedgerSummary);

        // Front (geteilt) — H2: Adjudikation als Request -> [adjudication-gate] -> Apply
        b.AddEdge(front.LedgerSummary, front.AdjudicationRequest);
        b.AddEdge(front.AdjudicationRequest, front.AdjudicationPort);       // Items > 0
        b.AddEdge(front.AdjudicationRequest, front.AdjudicationEmptyGate);  // leere Queue -> LAUTER Skip (R-50)
        b.AddEdge(front.AdjudicationEmptyGate, front.AdjudicationApply);
        b.AddEdge(front.AdjudicationPort, front.AdjudicationApply);
        // A10: Refine zwischen Apply und Baselines — hebt NEU gemintete Gap-Claims (facetStatus=pending)
        // auf Pipeline-Niveau, bevor die Baseline-Ableitung liest; ohne pending reiner Durchreich-Knoten.
        b.AddEdge(front.AdjudicationApply, front.AdjudicationRefine);
        b.AddEdge(front.AdjudicationRefine, front.Baselines);
        b.AddEdge(front.Baselines, front.Delta);
        b.AddEdge(front.Delta, front.Branch);

        // Branch: Typ-Routing (BootstrapDelta | OperationalDelta)
        b.AddEdge(front.Branch, boot.CoreBootstrap);
        b.AddEdge(front.Branch, op.IngestBridge);

        // Bootstrap-Zweig: core-bootstrap -> [A2: Klassifikation der geseedeten arch-Items] -> Cluster (+Gate) -> ...
        b.AddEdge(boot.CoreBootstrap, classify.BootBridge);
        b.AddEdge(classify.BootBridge, adr.BootBridge);            // Typ CoreBootstrapOutput (Leer-Skip -> ADR-Strip)
        b.AddEdge(classify.BootBridge, classify.Maker);            // Typ ArchClassifyWork
        b.AddEdge(boot.ClusterBridge, boot.ClusterAgent);
        // R-33 S2: Loop-Kanten aus der EINEN Quelle (ReClarifyClusterWorkflow.AddTo) — CLI und Ein-Graph identisch.
        ReClarifyClusterWorkflow.AddTo(b, boot.ClusterAgent, boot.ClusterGate, boot.ClusterRepair, boot.ClusterReview, boot.ClusterFinalize);
        b.AddEdge(boot.ClusterFinalize, boot.ClusterGateRequest);
        b.AddEdge(boot.ClusterGateRequest, boot.ClusterPort);      // Ops > 0
        b.AddEdge(boot.ClusterGateRequest, boot.ClusterEmptyGate); // 0 Ops -> LAUTER Skip (R-50)
        b.AddEdge(boot.ClusterEmptyGate, boot.ClusterApply);
        b.AddEdge(boot.ClusterPort, boot.ClusterApply);
        b.AddEdge(boot.ClusterApply, boot.BacklogBridge);
        b.AddEdge(boot.BacklogBridge, boot.ClarifyAgent);
        // R-33 S1: Loop-Kanten aus der EINEN Quelle (ReClarifyBacklogWorkflow.AddTo) — CLI und Ein-Graph identisch.
        ReClarifyBacklogWorkflow.AddTo(b, boot.ClarifyAgent, boot.BacklogGate, boot.BacklogRepair, boot.BacklogFinalize);
        b.AddEdge(boot.BacklogFinalize, boot.BacklogGateRequest);
        b.AddEdge(boot.BacklogGateRequest, boot.BacklogPort);
        b.AddEdge(boot.BacklogPort, boot.BacklogApply);
        b.AddEdge(boot.BacklogApply, boot.Seed);

        // Betriebs-Zweig: Ingest -> [Gate] -> Apply -> Bridge -> PbiUpdate -> [Gate] -> Apply (eine Quelle: AddTo)
        // R-11 A1b: der Aspekt-Router sitzt VOR dem Kern — req läuft durch, Unregistriertes wird LAUT geparkt.
        b.AddEdge(op.IngestBridge, op.AspectRouter);
        b.AddEdge(op.AspectRouter, op.IngestResolve);
        PipelineComposedWorkflow.AddTo(b, op.IngestResolve, op.IngestGate, op.IngestRepair, op.IngestFinalize,
            op.IngestPort, op.IngestApply,
            op.ArchBridge, op.ArchResolve, op.ArchGate, op.ArchRepair, op.ArchFinalize, op.ArchPort, op.ArchApply,
            op.DecisionScan, op.DecisionPort, op.DecisionApply,
            op.Bridge, op.PbiDerive, op.PbiMaker, op.PbiGate, op.PbiRepair,
            op.PbiAlign, op.PbiFinalize, op.PbiPort, op.PbiApply, op.PbiEmptyGate,
            op.IngestEmptyGate, op.ArchEmptyGate);

        // R-11 A2-2 — der geteilte Klassifikations-Strip (Loop-Kanten aus der EINEN Quelle) + Betriebs-Anker:
        // archApply -> OpBridge -> (Report-Skip -> DecisionScan | Work -> Maker); Apply re-emittiert den
        // Bahnen-Passagier TYP-GENAU (Report -> DecisionScan · CoreBootstrapOutput -> ClusterBridge).
        b.AddEdge(op.ArchApply, classify.OpBridge);
        b.AddEdge(classify.OpBridge, adr.OpBridge);                // Typ IngestionApplyReport (Leer-Skip -> ADR-Strip)
        b.AddEdge(classify.OpBridge, classify.Maker);              // Typ ArchClassifyWork
        AgenticSdlc.Host.FullWorkflow.ArchClassify.ArchClassifyWorkflow.AddTo(b, classify.Maker, classify.Gate, classify.Repair, classify.Finalize, classify.Port, classify.Apply);
        b.AddEdge(classify.Apply, adr.OpBridge);                   // Typ IngestionApplyReport
        b.AddEdge(classify.Apply, adr.BootBridge);                 // Typ CoreBootstrapOutput
        // A5: der ADR-Strip — hinter der Klassifikation (frisch bestätigte design-Rollen), vor DecisionScan/Cluster.
        b.AddEdge(adr.OpBridge, op.DecisionScan);                  // Typ IngestionApplyReport (Leer-Skip)
        b.AddEdge(adr.OpBridge, adr.Maker);                        // Typ AdrWork
        b.AddEdge(adr.BootBridge, boot.ClusterBridge);             // Typ CoreBootstrapOutput (Leer-Skip)
        b.AddEdge(adr.BootBridge, adr.Maker);                      // Typ AdrWork
        AgenticSdlc.Host.FullWorkflow.Adr.AdrWorkflow.AddTo(b, adr.Maker, adr.Gate, adr.Repair, adr.Finalize, adr.Port, adr.Apply);
        b.AddEdge(adr.Apply, op.DecisionScan);                     // Typ IngestionApplyReport
        b.AddEdge(adr.Apply, boot.ClusterBridge);                  // Typ CoreBootstrapOutput

        // Gemeinsames Ende: beide Zweige -> Forward-Prep -> Snapshot -> Forward (+Gate) -> Apply (Dry-Run-Default)
        b.AddEdge(boot.Seed, fwd.BootstrapBridge);
        b.AddEdge(op.PbiApply, fwd.OperationalBridge);
        b.AddEdge(fwd.BootstrapBridge, fwd.Snapshot);
        b.AddEdge(fwd.OperationalBridge, fwd.Snapshot);
        b.AddEdge(fwd.Snapshot, fwd.Seed);
        GithubForwardHitlWorkflow.AddTo(b, fwd.Seed, fwd.Maker, fwd.Gate, fwd.Repair, fwd.Finalize, fwd.HumanGate, fwd.Apply, fwd.EmptyGate);

        b.WithOutputFrom(front.ClarifyEntry);      // A′ Schritt 2: Non-Pass = terminaler Klartext-Output (bewusst gestoppt)
        b.WithOutputFrom(front.ReprojectEntry);    // ONLY-STEWARD: „keine gemappten PBIs" = terminaler Klartext-Output
        b.WithOutputFrom(front.LedgerSummary);     // Fehler-String (Gate/Step-Outputs) — Schritt 5 ②; v1-Wrapper hatte den Yield nie deklariert
        b.WithOutputFrom(front.Baselines);         // R-75 (09.09.): SP2/R-32-Yields (fehlgeschlagen / Checker-Rest->Mensch) waren NIE deklariert — Crash statt lautem Ende (gleiche Klasse wie der v1-Wrapper-Fall)
        b.WithOutputFrom(boot.CoreBootstrap);      // Fehler-String (Core existiert) + Info-Output
        b.WithOutputFrom(boot.Seed);               // BacklogSeedOutput = Bootstrap-Mitte fertig
        b.WithOutputFrom(op.IngestBridge);         // Fehler-String (Core fehlt bei erzwungenem operational)
        b.WithOutputFrom(fwd.BootstrapBridge);     // ForwardSkipped
        b.WithOutputFrom(fwd.OperationalBridge);   // ForwardSkipped
        return b.Build();
    }
}
