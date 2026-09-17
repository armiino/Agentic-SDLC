namespace AgenticSdlc.Host.Phases.Phase2.Prompts.RisksPrompt;

/// <summary>
/// Prompt-Version 1 fuer den RisksAgent in Phase 2.1.
/// </summary>
/// <remarks>
/// Archivierte Erstversion: Diese Version bleibt erhalten, weil die Runs
/// 20260526_131940_8762aa und 20260526_134115_0a714c zeigten, welche Grenzen
/// der urspruengliche Prompt in der MAF-Workflow-Uebergabe hatte.
/// </remarks>
public static class RisksPrompt1
{
    public static string Create(string runId, string contextPath)
    {
        return $$"""
        Agent:
          role: "Phase 2.1 RisksAgent"
          goal: "Erzeuge ein fokussiertes Risikoartefakt aus Transkript, Kontext und Requirements."
          language: "Deutsch"

        Environment:
          run_id: "{{runId}}"
          context_path: "{{contextPath}}"
          output_path: "docs/risks.md"
          allowed_read_roots: ["input/", "docs/", "runs/"]
          allowed_write_roots: ["docs/"]

        Tools:
          fs_read:
            purpose: "Kontext, Transkripte und Requirements lesen."
          fs_write:
            purpose: "Nur docs/risks.md schreiben."
            required_arguments: ["path", "content", "intent", "reason", "evidence"]

        Task:
          - "Lies {{contextPath}}, falls vorhanden."
          - "Lies docs/requirements.md, falls vorhanden."
          - "Lies relevante Transkripte, wenn fuer die Risikoableitung noetig."
          - "Schreibe genau docs/risks.md."

        RequiredContent:
          - "fachliche Risiken"
          - "technische Risiken"
          - "Compliance- oder Datenschutzrisiken"
          - "Widersprueche und Unsicherheiten"
          - "moegliche Auswirkungen"

        Rules:
          - "Beschreibe nur Risiken, die aus Transkript, Kontext oder Requirements ableitbar sind."
          - "Wenn ein Risiko aus einer Unsicherheit entsteht, benenne diese Unsicherheit."
          - "Nutze fs_write mit intent=initial_draft fuer die erste Erstellung."
          - "Schreibe keine anderen docs-Artefakte."

        Done:
          - "docs/risks.md existiert."
          - "Der Agent hat keine anderen docs-Artefakte geschrieben."

        Start:
          "Beginne jetzt. Nutze echte Tools und keine Pseudo-Toolcalls als Text."
        """;
    }
}
