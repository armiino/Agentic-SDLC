## Iteration 10 - Run `20260526_155811_f151d2`: Grosser Stress-Test mit erweitertem Transkript

**RunId:** `20260526_155811_f151d2`

**Setup:**
- Phase: `phase2_1`
- Provider: OpenRouter
- Modell: `openai/gpt-oss-120b:free`
- Input: stark erweitertes `input/transcripts/T9999_chaos.txt`
- Run-Notiz: "stress test mit neuem transksirpt"

**Beobachtung:**
- Der Workflow lief vollstaendig durch.
- Alle Specialist Agents schrieben jeweils ihr eigenes Artefakt.
- Die deterministische Validation bestand ohne Findings.
- Die Artefakte greifen viele neue Stress-Themen auf: Support, Finance, Rabattfreigabe, EU-only Hosting, API-Gateway-Warteliste, SAP-Verfuegbarkeit, Testdaten, Secrets, Monitoring, Retention und Loeschkonflikte.

**Evidence:**
- `runs/phase2_1/20260526_155811_f151d2/logs/events.jsonl`
- `runs/phase2_1/20260526_155811_f151d2/validation/phase2_1.report.json`
- `runs/phase2_1/20260526_155811_f151d2/snapshots/docs/`

**Qualitaetsbefund:**
- Positiv: Rollenverhalten und Toolnutzung bleiben auch bei deutlich laengerem Transkript stabil.
- Positiv: Die Artefakte sind strukturiert und decken viele Konfliktfelder ab.
- Kritisch: Einzelne Inhalte werden zu stark konkretisiert, obwohl sie im Transkript nur offen diskutiert wurden, z. B. konkrete Technologiebeispiele, eigen-hosted Proxy als MVP-Loesung, PostgreSQL-aaS, konkrete RPO/RTO-Werte und Retention-Zeitraeume.
- Kritisch: Die Traceability wirkt belastbar, verwendet aber generische oder ungenaue Zeilenangaben. Diese Zeilenangaben sind nicht automatisch verifiziert.
- Kritisch: Einige Widersprueche werden in Entscheidungen verwandelt, obwohl sie fachlich noch offen sind.

**Learning:** Phase 2.1 skaliert technisch besser als Phase 1 und erzeugt trotz Stress-Test vollstaendige Artefakte. Gleichzeitig zeigt sich die Grenze reiner Specialist-Workflows: Die Agents koennen offene Konflikte teilweise zu frueh entscheiden oder konkrete Architekturdetails ableiten, ohne dass ein Reviewer diese Ableitung prueft. Das liefert eine gute Begruendung fuer eine naechste Phase mit Review-/Kritik-/Repair-Schicht.

---

## Iteration 11 - RequirementsPrompt3 fuer belegbaren Quellenzugriff

**Ausloeser:** Im Run `20260526_155811_f151d2` schrieb der `Phase2RequirementsAgent` direkt `docs/requirements.md`, ohne in seinen eigenen Agent-Logs `fs_read` auf `context.md` oder das Transkript auszufuehren.

**Beobachtung:**
- Der Workflow war technisch erfolgreich.
- Die Requirements waren vorhanden und strukturiert.
- In `logs/agents/Phase2RequirementsAgent/tool-calls.jsonl` war jedoch nur `fs_write` sichtbar.
- Damit ist nicht belegbar, dass der RequirementsAgent die Quellen selbst gelesen hat. Wahrscheinlich arbeitete er mit der von MAF weitergereichten Chat-/Tool-Historie des ContextAgent.

**Aenderung:**
- Neue Prompt-Version `RequirementsPrompt3`.
- Die Factory verwendet fuer neue Runs `RequirementsPrompt3`.
- Der RequirementsAgent soll vor `fs_write` explizit:
  - `context.md` per `fs_read` lesen,
  - mindestens ein relevantes Transkript unter `input/transcripts/` per `fs_read` lesen,
  - in `evidence` nur Quellen nennen, die er selbst gelesen hat.

**Warum:** Diese Version testet, ob ein Specialist Agent seine Quellen selbst lesen kann und ob dieser Quellenzugriff in den Agent-Logs belegbar wird. Das ist methodisch wichtig, weil Phase 2.1 nicht nur funktionierende Artefakte, sondern nachvollziehbare Agentenarbeit untersuchen soll.

**Zu testen im naechsten Run:**
- Enthalten die Logs des `Phase2RequirementsAgent` nun `fs_read` auf `context.md`?
- Enthalten die Logs des `Phase2RequirementsAgent` nun `fs_read` auf `input/transcripts/T9999_chaos.txt`?
- Bleibt der Workflow trotzdem stabil?
- Verbessert sich die Nachvollziehbarkeit oder entstehen hoehere Kosten/Laufzeiten ohne klaren Qualitaetsgewinn?

---

