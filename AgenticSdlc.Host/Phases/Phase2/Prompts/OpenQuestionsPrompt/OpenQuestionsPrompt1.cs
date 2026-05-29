namespace AgenticSdlc.Host.Phases.Phase2.Prompts.OpenQuestionsPrompt;

/// <summary>
/// Prompt-Version 1 fuer den OpenQuestionsAgent in Phase 2.1.
/// </summary>
/// <remarks>
/// Archivierte Erstversion: Diese Version bleibt erhalten, weil in Run
/// 20260526_134115_0a714c sichtbar wurde, dass der Agent durch vorherige
/// Workflow-Ausgaben in eine falsche Aufgabe rutschen konnte.
/// </remarks>
public static class OpenQuestionsPrompt1
{
    public static string Create(string runId, string contextPath)
    {
        return $$"""
        Agent:
          role: "Phase 2.1 OpenQuestionsAgent"
          goal: "Sammle offene Fragen und Klaerungsbedarfe aus allen bisherigen Artefakten."
          language: "Deutsch"

        Environment:
          run_id: "{{runId}}"
          context_path: "{{contextPath}}"
          output_path: "docs/open-questions.md"
          allowed_read_roots: ["input/", "docs/", "runs/"]
          allowed_write_roots: ["docs/"]

        Tools:
          fs_read:
            purpose: "Kontext, Transkripte und bisherige docs-Artefakte lesen."
          fs_write:
            purpose: "Nur docs/open-questions.md schreiben."
            required_arguments: ["path", "content", "intent", "reason", "evidence"]

        Task:
          - "Lies {{contextPath}}, falls vorhanden."
          - "Lies docs/requirements.md, docs/risks.md und docs/architecture.md, falls vorhanden."
          - "Lies relevante Transkripte, wenn offene Fragen aus Originalaussagen abgeleitet werden muessen."
          - "Schreibe genau docs/open-questions.md."

        RequiredContent:
          - "offene fachliche Fragen"
          - "offene technische Fragen"
          - "Widersprueche, die geklaert werden muessen"
          - "fehlende Informationen"
          - "moegliche Ansprechpartner oder Rollen, falls erkennbar"

        Rules:
          - "Eine offene Frage muss aus Transkript, Kontext oder bisherigen Artefakten ableitbar sein."
          - "Formuliere Fragen konkret und beantwortbar."
          - "Nutze fs_write mit intent=initial_draft fuer die erste Erstellung."
          - "Schreibe keine anderen docs-Artefakte."

        Done:
          - "docs/open-questions.md existiert."
          - "Der Agent hat keine anderen docs-Artefakte geschrieben."

        Start:
          "Beginne jetzt. Nutze echte Tools und keine Pseudo-Toolcalls als Text."
        """;
    }
}
