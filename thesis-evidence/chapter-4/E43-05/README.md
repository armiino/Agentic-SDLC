# E43-05 — Unterschiedliche Reaktionen auf die Promptschärfung

Am 28.05. schreibt der Anforderungsagent ohne eigenen Leseaufruf. Am 29.05. liest er context.md, schreibt aber kein Requirements-Artefakt; die Validierung meldet dessen Fehlen.

**Belegstatus:** Originalbelege bereitgestellt; Textkorrektur mitgeliefert.

**Aussagegrenze:** Die bisherige Aussage eines unveränderten Zugriffsverhaltens in beiden Läufen wird durch den zweiten Lauf widerlegt. RequirementsPrompt4 wurde laut Notiz erst danach eingeführt; diese beiden Läufe belegen keinen Test von Prompt4.

Die Auswahl wurde am 2026-09-13 bereitgestellt. Kopierte Originaldateien sind bytegleich; Notizauszüge sind als solche gekennzeichnet. Vollständige Originalpfade, Dateihashes und die Auswahl innerhalb der Run-Ordner stehen im [Manifest](../manifest.json). Querverweise in Rohdateien behalten ihre historischen Pfade; die Tabelle ordnet die für diesen Beleg nötigen Dateien zu.

## Lesestart

- [../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl)
- [../E43-04/20260528_130843_589104/validation/phase2_1.report.json](../E43-04/20260528_130843_589104/validation/phase2_1.report.json)
- [20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl)
- [20260529_101357_d4d49c/validation/phase2_1.report.json](20260529_101357_d4d49c/validation/phase2_1.report.json)

## Vollständiger Dateiindex

| Datei | Funktion |
|---|---|
| [approval_20260528131545841.json](../E43-04/20260528_130843_589104/approvals/approval_20260528131545841.json) | Originalbeleg |
| [config.json](../E43-04/20260528_130843_589104/config.json) | Originalbeleg |
| [decisions.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ArchitectureAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ArchitectureAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ArchitectureAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](../E43-04/20260528_130843_589104/logs/agents/Phase2ArchitectureAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ContextAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ContextAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2ContextAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](../E43-04/20260528_130843_589104/logs/agents/Phase2ContextAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2OpenQuestionsAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2OpenQuestionsAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2OpenQuestionsAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](../E43-04/20260528_130843_589104/logs/agents/Phase2OpenQuestionsAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](../E43-04/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RisksAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RisksAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](../E43-04/20260528_130843_589104/logs/agents/Phase2RisksAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](../E43-04/20260528_130843_589104/logs/agents/Phase2RisksAgent/workflow-output.txt) | Originalbeleg |
| [changes.txt](../E43-04/20260528_130843_589104/logs/changes.txt) | Originalbeleg |
| [decision-log.jsonl](../E43-04/20260528_130843_589104/logs/decision-log.jsonl) | Originalbeleg |
| [20260528131009133_runs_phase2_1_20260528_130843_589104_state_context.md.diff](../E43-04/20260528_130843_589104/logs/diffs/20260528131009133_runs_phase2_1_20260528_130843_589104_state_context.md.diff) | Originalbeleg |
| [20260528131056411_docs_requirements.md.diff](../E43-04/20260528_130843_589104/logs/diffs/20260528131056411_docs_requirements.md.diff) | Originalbeleg |
| [20260528131244459_docs_risks.md.diff](../E43-04/20260528_130843_589104/logs/diffs/20260528131244459_docs_risks.md.diff) | Originalbeleg |
| [20260528131423197_docs_architecture.md.diff](../E43-04/20260528_130843_589104/logs/diffs/20260528131423197_docs_architecture.md.diff) | Originalbeleg |
| [20260528131540522_docs_open-questions.md.diff](../E43-04/20260528_130843_589104/logs/diffs/20260528131540522_docs_open-questions.md.diff) | Originalbeleg |
| [events.jsonl](../E43-04/20260528_130843_589104/logs/events.jsonl) | Originalbeleg |
| [otel-traces.raw.jsonl](20260528_130843_589104/logs/otel-traces.raw.jsonl) | Originalbeleg |
| [tool-discovery.json](../E43-04/20260528_130843_589104/logs/tool-discovery.json) | Originalbeleg |
| [architecture.md](../E43-04/20260528_130843_589104/snapshots/docs/architecture.md) | Originalbeleg |
| [open-questions.md](../E43-04/20260528_130843_589104/snapshots/docs/open-questions.md) | Originalbeleg |
| [requirements.md](../E43-04/20260528_130843_589104/snapshots/docs/requirements.md) | Originalbeleg |
| [risks.md](../E43-04/20260528_130843_589104/snapshots/docs/risks.md) | Originalbeleg |
| [context.md](../E43-04/20260528_130843_589104/state/context.md) | Originalbeleg |
| [phase2_1.report.json](../E43-04/20260528_130843_589104/validation/phase2_1.report.json) | Originalbeleg |
| [config.json](20260529_101357_d4d49c/config.json) | Originalbeleg |
| [decisions.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ArchitectureAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ArchitectureAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ArchitectureAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](20260529_101357_d4d49c/logs/agents/Phase2ArchitectureAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ContextAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ContextAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2ContextAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](20260529_101357_d4d49c/logs/agents/Phase2ContextAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](20260529_101357_d4d49c/logs/agents/Phase2OpenQuestionsAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/agents/Phase2OpenQuestionsAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2OpenQuestionsAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](20260529_101357_d4d49c/logs/agents/Phase2OpenQuestionsAgent/workflow-output.txt) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/workflow-output.txt) | Originalbeleg |
| [decisions.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RisksAgent/decisions.jsonl) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RisksAgent/events.jsonl) | Originalbeleg |
| [tool-calls.jsonl](20260529_101357_d4d49c/logs/agents/Phase2RisksAgent/tool-calls.jsonl) | Originalbeleg |
| [workflow-output.txt](20260529_101357_d4d49c/logs/agents/Phase2RisksAgent/workflow-output.txt) | Originalbeleg |
| [changes.txt](20260529_101357_d4d49c/logs/changes.txt) | Originalbeleg |
| [decision-log.jsonl](20260529_101357_d4d49c/logs/decision-log.jsonl) | Originalbeleg |
| [diagnosis.json](20260529_101357_d4d49c/logs/diagnosis.json) | Originalbeleg |
| [20260529101544297_runs_phase2_1_20260529_101357_d4d49c_state_context.md.diff](20260529_101357_d4d49c/logs/diffs/20260529101544297_runs_phase2_1_20260529_101357_d4d49c_state_context.md.diff) | Originalbeleg |
| [20260529101720473_docs_risks.md.diff](20260529_101357_d4d49c/logs/diffs/20260529101720473_docs_risks.md.diff) | Originalbeleg |
| [20260529101912909_docs_architecture.md.diff](20260529_101357_d4d49c/logs/diffs/20260529101912909_docs_architecture.md.diff) | Originalbeleg |
| [20260529102050936_docs_open-questions.md.diff](20260529_101357_d4d49c/logs/diffs/20260529102050936_docs_open-questions.md.diff) | Originalbeleg |
| [events.jsonl](20260529_101357_d4d49c/logs/events.jsonl) | Originalbeleg |
| [otel-traces.raw.jsonl](20260529_101357_d4d49c/logs/otel-traces.raw.jsonl) | Originalbeleg |
| [tool-discovery.json](20260529_101357_d4d49c/logs/tool-discovery.json) | Originalbeleg |
| [architecture.md](20260529_101357_d4d49c/snapshots/docs/architecture.md) | Originalbeleg |
| [open-questions.md](20260529_101357_d4d49c/snapshots/docs/open-questions.md) | Originalbeleg |
| [risks.md](20260529_101357_d4d49c/snapshots/docs/risks.md) | Originalbeleg |
| [context.md](20260529_101357_d4d49c/state/context.md) | Originalbeleg |
| [phase2_1.report.json](20260529_101357_d4d49c/validation/phase2_1.report.json) | Originalbeleg |
| [phase02-iteration-notes.md](notizen/promptschritte-und-beide-laeufe.md) | Wörtlicher Auszug aus Entwicklungsnotiz; historische Einordnung, keine neue Messung; Originalzeilen 275–475, vollständiger Quellhash im Manifest |
| [T9999_chaos.txt](../_kontext/input/transcripts/T9999_chaos.txt) | Quelltranskript des jeweiligen historischen Vergleichs; aktueller Dateiinhalt per Hash gesichert |
