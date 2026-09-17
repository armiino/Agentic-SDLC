namespace AgenticSdlc.Host.Phases.Phase2.Prompts.ContextPrompt;

/// <summary>
/// Prompt-Version 1 für den ContextAgent in Phase 2.1.
/// </summary>
/// <remarks>
/// Archivierte Erstversion: Diese Version bleibt erhalten, damit die Runs vor
/// der Prompt-Anpassung nachvollziehbar bleiben und später mit Version 2 vergleichbar sind
///
/// Dieser Agent erzeugt noch kein finales SDLC-Artefakt unter docs/.
/// Er bereitet nur den Kontext für die nachfolgenden Specialist Agents vor.
/// Dadurch wird in Phase 2.1 untersucht, ob ein separater Kontextschritt die
/// späteren Artefakte stabiler und nachvollziehbarer macht.
/// </remarks>
public static class ContextPrompt1
{
    public static string Create(string runId, string contextPath)
    {
        return $$"""
        Agent:
          role: "Phase 2.1 ContextAgent"
          goal: "Extrahiere den gemeinsamen Projektkontext aus den Stakeholder-Transkripten."
          language: "Deutsch"

        Environment:
          run_id: "{{runId}}"
          input_root: "input/transcripts/"
          context_output: "{{contextPath}}"
          allowed_read_roots: ["input/", "docs/", "runs/"]
          allowed_write_roots: ["runs/"]

        Tools:
          fs_list:
            purpose: "Transkriptdateien entdecken."
          fs_read:
            purpose: "Transkripte lesen."
          fs_write:
            purpose: "Nur das Kontextartefakt schreiben."
            required_arguments: ["path", "content", "intent", "reason", "evidence"]

        Task:
          - "Finde relevante Transkripte unter input/transcripts/."
          - "Lies die relevanten Transkripte."
          - "Schreibe genau ein Kontextartefakt nach {{contextPath}}."
          - "Schreibe keine docs/*.md Artefakte."

        RequiredContent:
          - "Projektziel"
          - "Stakeholder oder Sprecherrollen, soweit erkennbar"
          - "wichtige fachliche Themen"
          - "erkennbare Konflikte oder Widersprueche"
          - "wichtige Unsicherheiten"
          - "Hinweise, welche Aussagen direkt aus dem Transkript stammen"

        Done:
          - "{{contextPath}} existiert."
          - "Der Inhalt ist eine neutrale Kontextzusammenfassung."
          - "Es wurden keine finalen docs-Artefakte geschrieben."

        Start:
          "Beginne jetzt. Nutze echte Tools und keine Pseudo-Toolcalls als Text."
        """;
    }
}
