using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// R-50 (17.08., Testplan Block G7): „Ein Human-Gate ruft nur, wenn es etwas zu entscheiden gibt — ein Skip
/// ist immer LAUT, nie still." Vorher pausierte eine „alles abgelehnt"-Runde ZWEIMAL an Gates mit 0 Operationen.
/// MAF-Form: TYP-ROUTING (das etablierte Entry-/Branch-Muster) — der Finalize sendet bei 0 Ops einen EIGENEN
/// Marker-Typ statt des Review-Requests; die Kanten routen typgenau: Request → RequestPort (Mensch) ·
/// Marker → dieser Responder → leere Antwort auf dem NORMALEN, bewährten Apply-Pfad. (Prädikat-Kanten auf
/// RequestPorts sind KEINE Option: der Port ignoriert die Bedingung — Doppel-Zustellung, Fund 17.08.)
/// Der Responder ENTSCHEIDET nichts (null Ops ⇒ die leere Antwort ist eine Identität, keine Governance-
/// Ausnahme) und ist als eigener Graph-Knoten + GATE_SKIPPED_EMPTY-Event sichtbar (E0.9). Präzedenz:
/// ARCH_INGEST_SKIPPED (no-incoming) — die pbi-/forward-Strips ziehen auf denselben Standard nach.
/// </summary>
public abstract class EmptyGateAutoResponder<TMarker, TResp>(RunContext run, string gate, Func<TMarker, TResp> emptyResponse)
    : Executor<TMarker>($"EmptyGate:{gate}") where TResp : notnull
{
    public override async ValueTask HandleAsync(TMarker marker, IWorkflowContext context, CancellationToken ct = default)
    {
        run.AppendEvent(new { type = "GATE_SKIPPED_EMPTY", runId = run.RunId, gate, timestampUtc = DateTime.UtcNow });
        Console.WriteLine($"[pipeline] {gate}: 0 Operationen — Human-Gate übersprungen (R-50, laut); leere Antwort läuft den normalen Apply-Pfad.");
        await context.SendMessageAsync(emptyResponse(marker)).ConfigureAwait(false);
    }
}

/// <summary>R-50-Marker: das pbi-gate hätte 0 Operationen — Typ-Routing am Finalize statt Port-Pause.</summary>
public sealed record PbiUpdateGateEmpty(PbiUpdate.PbiUpdateReviewRequest Request);

/// <summary>R-50-Marker: das github-forward-gate hätte 0 Operationen.</summary>
public sealed record GithubForwardGateEmpty(Tore.Github.ForwardReviewRequest Request);

/// <summary>R-50: leeres pbi-gate — leere Antwort (0 accepted, keine Alignments) an den bewährten Apply.</summary>
[SendsMessage(typeof(PbiUpdate.PbiUpdateReviewResponse))]
public sealed class PbiUpdateEmptyGateResponder(RunContext run)
    : EmptyGateAutoResponder<PbiUpdateGateEmpty, PbiUpdate.PbiUpdateReviewResponse>(
        run, "pbi-gate", _ => new([], "auto (leeres Gate — R-50)"));

/// <summary>R-50: leeres github-forward-gate — leere Antwort (0 accepted, Execute egal ⇒ false) an den Apply.</summary>
[SendsMessage(typeof(Tore.Github.ForwardReviewResponse))]
public sealed class GithubForwardEmptyGateResponder(RunContext run)
    : EmptyGateAutoResponder<GithubForwardGateEmpty, Tore.Github.ForwardReviewResponse>(
        run, "github-forward-gate", _ => new([], Execute: false, "auto (leeres Gate — R-50)"));

// ---- R-50-Vervollständigung (17.08., Audit „sitzt der Knoten überall?"): die drei restlichen Sender ----

/// <summary>R-50-Marker: das ingest-/arch-ingest-gate hätte 0 Operationen (profilbewusst via GateName).</summary>
public sealed record IngestionGateEmpty(Core.IngestionReviewRequest Request, string GateName);

/// <summary>R-50-Marker: die Adjudikations-Queue ist leer (perfekter Ledger — nichts zu adjudizieren).</summary>
public sealed record AdjudicationGateEmpty(Pipeline.AdjudicationReviewRequest Request);

/// <summary>R-50-Marker: das cluster-review-gate hätte 0 Korrektur-Operationen.</summary>
public sealed record ClusterGateEmpty(Pipeline.ClusterReviewRequest Request);

/// <summary>R-50: leeres ingest-/arch-ingest-gate — leere Antwort an den bewährten Ingest-Apply.
/// EINE Klasse für beide Profile (Executor-Id + Event via gateName der Instanz).</summary>
[SendsMessage(typeof(Core.IngestionReviewResponse))]
public sealed class IngestionEmptyGateResponder(RunContext run, string gateName)
    : EmptyGateAutoResponder<IngestionGateEmpty, Core.IngestionReviewResponse>(
        run, gateName, _ => new([], "auto (leeres Gate — R-50)"));

/// <summary>R-50: leere Adjudikations-Queue — leere Aktions-Map an den Apply (consumable = validated).</summary>
[SendsMessage(typeof(Pipeline.AdjudicationReviewResponse))]
public sealed class AdjudicationEmptyGateResponder(RunContext run)
    : EmptyGateAutoResponder<AdjudicationGateEmpty, Pipeline.AdjudicationReviewResponse>(
        run, "adjudication-gate", _ => new(new Dictionary<string, string>(), "auto (leeres Gate — R-50)"));

/// <summary>R-50: leeres cluster-review-gate — 0 akzeptierte Ops an den Apply (Coverage-Recheck läuft normal).</summary>
[SendsMessage(typeof(Pipeline.ClusterReviewResponse))]
public sealed class ClusterEmptyGateResponder(RunContext run)
    : EmptyGateAutoResponder<ClusterGateEmpty, Pipeline.ClusterReviewResponse>(
        run, "cluster-review-gate", _ => new([], "auto (leeres Gate — R-50)"));
