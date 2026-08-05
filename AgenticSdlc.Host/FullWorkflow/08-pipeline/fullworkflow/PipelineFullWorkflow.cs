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
    LedgerIntakeExecutor LedgerIntake, ExecutorBinding LedgerCapsule, LedgerSummaryExecutor LedgerSummary, // Schritt 5 ②: sichtbare Kapsel statt Wrapper
    AdjudicationGateRequestExecutor AdjudicationRequest, RequestPort AdjudicationPort, AdjudicationApplyExecutor AdjudicationApply, // H2: echter RequestPort
    BaselineStageExecutor Baselines,
    ProjectStateBuildExecutor Delta, BranchDetectorExecutor Branch);

internal sealed record BootstrapNodes(
    CoreBootstrapStageExecutor CoreBootstrap, ClusterBridgeExecutor ClusterBridge,
    ClusterAgentExecutor ClusterAgent, ClusterGateExecutor ClusterGate, ClusterRepairExecutor ClusterRepair, ClusterReviewExecutor ClusterReview,
    ClusterFinalizeExecutor ClusterFinalize, ClusterGateRequestExecutor ClusterGateRequest, RequestPort ClusterPort,
    ClusterComposedApplyExecutor ClusterApply, BacklogBridgeExecutor BacklogBridge,
    ClarifyAgentExecutor ClarifyAgent, BacklogGateExecutor BacklogGate, BacklogRepairExecutor BacklogRepair, BacklogFinalizeExecutor BacklogFinalize,
    BacklogGateRequestExecutor BacklogGateRequest, RequestPort BacklogPort,
    BacklogComposedApplyExecutor BacklogApply, CoreSeedBacklogExecutor Seed);

internal sealed record OperationalNodes(
    IngestBridgeExecutor IngestBridge,
    IngestionHitlResolveExecutor IngestResolve, IngestionGateExecutor IngestGate, IngestionRepairExecutor IngestRepair,
    IngestionHitlFinalizeExecutor IngestFinalize, RequestPort IngestPort, IngestComposedApplyExecutor IngestApply,
    DecisionScanExecutor DecisionScan, RequestPort DecisionPort, DecisionComposedApplyExecutor DecisionApply,
    IngestPbiBridgeExecutor Bridge,
    PbiUpdateDeriveExecutor PbiDerive, PbiUpdateMakerExecutor PbiMaker, PbiUpdateGateExecutor PbiGate,
    PbiUpdateRepairExecutor PbiRepair, PbiAlignExecutor PbiAlign, PbiUpdateHitlFinalizeExecutor PbiFinalize, RequestPort PbiPort,
    PbiUpdateApplyExecutor PbiApply);

internal sealed record ForwardNodes(
    BootstrapForwardBridgeExecutor BootstrapBridge, OperationalForwardBridgeExecutor OperationalBridge,
    SnapshotExecutor Snapshot,
    GithubForwardSeedExecutor Seed, GithubForwardMakerExecutor Maker, GithubForwardGateExecutor Gate,
    GithubForwardRepairExecutor Repair, GithubForwardHitlFinalizeExecutor Finalize, RequestPort HumanGate,
    GithubForwardApplyExecutor Apply);

/// <summary>
/// U2 — die EINE Bauzeit-Orchestrierungsstelle der GANZEN Kette (ein-graph-vereinheitlichung §7).
/// Der komplette Graph steht hier: Front → Branch (Typ-Routing) → Bootstrap-Zweig | Betriebs-Zweig →
/// gemeinsames Forward-Ende. 7 Human-Gates (RequestPorts; decision-gate/Tor 2 seit R-14 G1), Conditional Edges, keine Orchestrierung außerhalb
/// (Design-Regel: keine CLI-Aufrufe aus Executors; der Runner startet nur noch EINEN Stream).
/// Zweig-Verdrahtung per AddTo aus den Standalone-Workflows (eine Quelle, keine Kopie).
/// </summary>
internal static class PipelineFullWorkflow
{
    public static Workflow Assemble(FrontNodes front, BootstrapNodes boot, OperationalNodes op, ForwardNodes fwd)
    {
        // Schritt 5 ③ (05.08.): der Start ist ein TYPISIERTER Eingangs-Dispatcher (Transkript | Delta) —
        // MAF-nativ per Input-Typ-Routing; der Delta-Einstieg dockt am existierenden BranchDetector an.
        var b = new WorkflowBuilder(front.Entry)
            .WithName("pipeline-full")
            .WithDescription("(Transkript | Delta) -> Entry -> [Ledger -> Adjudikation -> Baselines -> Delta ->] [Branch] -> "
                           + "Bootstrap (core-bootstrap -> Cluster+Gate -> PBIs+Gate -> Seed) | Betrieb (Ingest+Gate -> Pbi+Gate) "
                           + "-> Snapshot -> Forward+Gate -> Dry-Run/Apply.");

        // Eingangs-Vertrag: Transkript -> Front | fertiges Delta -> direkt Branch (beide Bahnen via Detector).
        b.AddEdge(front.Entry, front.LedgerIntake);
        b.AddEdge(front.Entry, front.Branch);

        // Schritt 5 ② (05.08.): die Ledger-Stufe ist eine SICHTBARE gebundene Kapsel (BindGateFree) statt des
        // Wrapper-Executors mit innerem Zweitmotor — ihre 8 Schritte sind Bürger des Ein-Graphen (Checkpoints,
        // Events). Intake adaptiert TranscriptInput -> Transkript-String; der Summary-Knoten führt die geteilte
        // Auswertung (Quality-Gate/Trace) und speist die Adjudikation wie zuvor.
        b.AddEdge(front.LedgerIntake, front.LedgerCapsule);
        b.AddEdge(front.LedgerCapsule, front.LedgerSummary);

        // Front (geteilt) — H2: Adjudikation als Request -> [adjudication-gate] -> Apply
        b.AddEdge(front.LedgerSummary, front.AdjudicationRequest);
        b.AddEdge(front.AdjudicationRequest, front.AdjudicationPort);
        b.AddEdge(front.AdjudicationPort, front.AdjudicationApply);
        b.AddEdge(front.AdjudicationApply, front.Baselines);
        b.AddEdge(front.Baselines, front.Delta);
        b.AddEdge(front.Delta, front.Branch);

        // Branch: Typ-Routing (BootstrapDelta | OperationalDelta)
        b.AddEdge(front.Branch, boot.CoreBootstrap);
        b.AddEdge(front.Branch, op.IngestBridge);

        // Bootstrap-Zweig: core-bootstrap -> Cluster (+Gate) -> Clarify (+Gate) -> Seed
        b.AddEdge(boot.CoreBootstrap, boot.ClusterBridge);
        b.AddEdge(boot.ClusterBridge, boot.ClusterAgent);
        // R-33 S2: Loop-Kanten aus der EINEN Quelle (ReClarifyClusterWorkflow.AddTo) — CLI und Ein-Graph identisch.
        ReClarifyClusterWorkflow.AddTo(b, boot.ClusterAgent, boot.ClusterGate, boot.ClusterRepair, boot.ClusterReview, boot.ClusterFinalize);
        b.AddEdge(boot.ClusterFinalize, boot.ClusterGateRequest);
        b.AddEdge(boot.ClusterGateRequest, boot.ClusterPort);
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
        b.AddEdge(op.IngestBridge, op.IngestResolve);
        PipelineComposedWorkflow.AddTo(b, op.IngestResolve, op.IngestGate, op.IngestRepair, op.IngestFinalize,
            op.IngestPort, op.IngestApply, op.DecisionScan, op.DecisionPort, op.DecisionApply,
            op.Bridge, op.PbiDerive, op.PbiMaker, op.PbiGate, op.PbiRepair,
            op.PbiAlign, op.PbiFinalize, op.PbiPort, op.PbiApply);

        // Gemeinsames Ende: beide Zweige -> Forward-Prep -> Snapshot -> Forward (+Gate) -> Apply (Dry-Run-Default)
        b.AddEdge(boot.Seed, fwd.BootstrapBridge);
        b.AddEdge(op.PbiApply, fwd.OperationalBridge);
        b.AddEdge(fwd.BootstrapBridge, fwd.Snapshot);
        b.AddEdge(fwd.OperationalBridge, fwd.Snapshot);
        b.AddEdge(fwd.Snapshot, fwd.Seed);
        GithubForwardHitlWorkflow.AddTo(b, fwd.Seed, fwd.Maker, fwd.Gate, fwd.Repair, fwd.Finalize, fwd.HumanGate, fwd.Apply);

        b.WithOutputFrom(front.LedgerSummary);     // Fehler-String (Gate/Step-Outputs) — Schritt 5 ②; v1-Wrapper hatte den Yield nie deklariert
        b.WithOutputFrom(boot.CoreBootstrap);      // Fehler-String (Core existiert) + Info-Output
        b.WithOutputFrom(boot.Seed);               // BacklogSeedOutput = Bootstrap-Mitte fertig
        b.WithOutputFrom(op.IngestBridge);         // Fehler-String (Core fehlt bei erzwungenem operational)
        b.WithOutputFrom(fwd.BootstrapBridge);     // ForwardSkipped
        b.WithOutputFrom(fwd.OperationalBridge);   // ForwardSkipped
        return b.Build();
    }
}
