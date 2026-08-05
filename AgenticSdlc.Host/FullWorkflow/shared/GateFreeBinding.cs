using Microsoft.Agents.AI.Workflows;

namespace AgenticSdlc.Host.FullWorkflow;

/// <summary>
/// Schritt 5 ② (05.08.) — der LAUTE Wächter an der Sub-Workflow-Naht: <see cref="BindGateFree"/> ersetzt
/// das nackte <c>BindAsExecutor</c> an ALLEN Bind-Stellen des Projekts.
/// </summary>
/// <remarks>
/// Spike-Befund (SubworkflowPortSpikeTests, MAF 1.15.0; korrigiert nach Doku-/Quellen-Check am selben Tag):
/// ein RequestPort in einer Kapsel wird NICHT automatisch durchgereicht — die Kapsel emittiert den Request
/// als <c>ExternalRequest</c>-NACHRICHT, und UNVERDRAHTET wird die lautlos gedroppt (stiller Deadlock).
/// Durchreichen IST möglich, erfordert aber die bewusste Forward-Verdrahtung des offiziellen Musters
/// (Sample 09: <c>ForwardMessage&lt;ExternalRequest&gt;(kapsel → eltern-port)</c> + Response zurück).
/// Alle UNSERE Kapseln sind Gate-frei; dieser Wächter macht die stille Falle zum Konstruktions-Fehler
/// (R-18-Stil). Wer je eine Gate-tragende Kapsel will (aufgefallen 9j), baut sie mit expliziter
/// Forward-Verdrahtung und bindet dort bewusst nackt — nicht über diesen Wächter.
/// </remarks>
public static class GateFreeBinding
{
    /// <summary>Bindet <paramref name="workflow"/> als Kapsel-Knoten — wirft LAUT, wenn er (auch verschachtelt)
    /// einen RequestPort enthält, statt zur Laufzeit still zu deadlocken.</summary>
    public static ExecutorBinding BindGateFree(this Microsoft.Agents.AI.Workflows.Workflow workflow, string id)
    {
        var portIds = new List<string>();
        CollectPorts(workflow, portIds);
        if (portIds.Count > 0)
            throw new InvalidOperationException(
                $"PORT_IN_CAPSULE: Workflow '{workflow.Name ?? id}' enthält RequestPort(s) [{string.Join(", ", portIds)}]. " +
                "Unverdrahtete Kapsel-Ports werden von MAF 1.15.0 LAUTLOS gedroppt (stiller Deadlock; Spike-Befund " +
                "SubworkflowPortSpikeTests). Entweder Gate-Stufe flach verdrahten (AddTo, Haus-Muster) ODER die Kapsel " +
                "BEWUSST mit ForwardMessage<ExternalRequest/ExternalResponse>-Kanten an einen Eltern-Port bauen " +
                "(offizielles Sample-09-Muster) und dann nackt binden.");
        return workflow.BindAsExecutor(id);
    }

    // Offizielle Introspektions-API (Workflow.ReflectPorts/ReflectExecutors) — rekursiv durch verschachtelte Kapseln.
    private static void CollectPorts(Microsoft.Agents.AI.Workflows.Workflow workflow, List<string> portIds)
    {
        portIds.AddRange(workflow.ReflectPorts().Keys);
        foreach (var binding in workflow.ReflectExecutors().Values)
            if (binding is SubworkflowBinding sub)
                CollectPorts(sub.WorkflowInstance, portIds);
    }
}
