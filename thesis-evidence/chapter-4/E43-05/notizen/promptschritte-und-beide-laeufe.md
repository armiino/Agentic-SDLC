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

## Iteration 12 - Run `20260528_130843_589104`: Message-Passing sichtbar, aber eigener Quellenzugriff bleibt unbelegt

**RunId:** `20260528_130843_589104`

**Setup:**
- Phase: `phase2_1`
- Context-Strategie: `message_passing`
- Provider: OpenRouter
- Modell: `openai/gpt-oss-120b:free`
- Run-Config wurde bereits aus `run-config.json` geschrieben.

**Beobachtung:**
- Der Workflow lief vollstaendig durch.
- Die deterministische Validation bestand ohne Findings.
- Alle Pflichtartefakte wurden erzeugt:
  - `runs/phase2_1/20260528_130843_589104/state/context.md`
  - `docs/requirements.md`
  - `docs/risks.md`
  - `docs/architecture.md`
  - `docs/open-questions.md`
- Der `Phase2ContextAgent` fuehrte echte Toolcalls aus:
  - `fs_list input/transcripts/`
  - `fs_read input/transcripts/T9999_chaos.txt`
  - `fs_write runs/phase2_1/20260528_130843_589104/state/context.md`
- Der `Phase2RequirementsAgent` fuehrte laut eigenem Agent-Toollog nur einen `fs_write` aus:
  - `fs_write docs/requirements.md`
- Im Agent-Toollog des `Phase2RequirementsAgent` gibt es keinen belegten `fs_read` auf:
  - `runs/phase2_1/20260528_130843_589104/state/context.md`
  - `input/transcripts/T9999_chaos.txt`
- Trotzdem behauptet `logs/agents/Phase2RequirementsAgent/workflow-output.txt`, dass beide Quellen eingelesen wurden.

**Evidence - lokale Toollogs:**
- `runs/phase2_1/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl`
- Dort stehen nur:
  - `TOOL_CALL_STARTED` fuer `fs_write docs/requirements.md`
  - `TOOL_CALL_FINISHED` fuer `fs_write docs/requirements.md`
  - `FILE_WRITE_ANALYZED`
  - `ARTIFACT_TOUCHED`
- Es gibt dort keinen `TOOL_CALL_STARTED` fuer `fs_read`.

**Evidence - Workflow-Output:**
- `runs/phase2_1/20260528_130843_589104/logs/agents/Phase2RequirementsAgent/workflow-output.txt`
- Inhaltlich behauptet der Agent:
  - `docs/requirements.md wurde geschrieben.`
  - `Eingelesen: runs/phase2_1/20260528_130843_589104/state/context.md und input/transcripts/T9999_chaos.txt.`
- Diese Aussage ist nicht als eigener Toolzugriff belegbar.

**Evidence - OTel Raw Message-Passing:**
- In `runs/phase2_1/20260528_130843_589104/logs/otel-traces.raw.jsonl` ist beim Chat-Span des `Phase2RequirementsAgent` ein `gen_ai.input.messages`-Feld sichtbar.
- Der relevante Span ist der Chat-Span mit:
  - `name`: `chat openai/gpt-oss-120b:free`
  - `gen_ai.system_instructions`: enthaelt `role: "Phase 2.1 RequirementsAgent"`
  - `startTimeUtc`: `2026-05-28T13:10:10.593683Z`
  - `endTimeUtc`: `2026-05-28T13:10:56.401761Z`
- In diesem `gen_ai.input.messages` stehen vor der RequirementsAgent-Antwort bereits Nachrichten des `Phase2ContextAgent`, unter anderem:
  - ein Assistant-Toolcall `fs_list` mit `path: input/transcripts/`
  - eine Tool-Response mit `["input/transcripts/T9999_chaos.txt"]`
  - ein Assistant-Toolcall `fs_read` mit `path: input/transcripts/T9999_chaos.txt`
  - eine Tool-Response, deren Text den Inhalt des Transkripts enthaelt
  - ein Assistant-Toolcall `fs_write` auf `runs/phase2_1/20260528_130843_589104/state/context.md`
  - die dabei geschriebene Kontextzusammenfassung als Toolcall-Argument
  - eine nachfolgende Statusnachricht `Context file written.`
- Danach erzeugt der `Phase2RequirementsAgent` als Output direkt einen `fs_write` auf `docs/requirements.md`.

**Wichtige Einordnung: sicher oder Vermutung?**
- Sicher belegbar ist:
  - Der `Phase2RequirementsAgent` hat keinen eigenen `fs_read` ausgefuehrt, weil weder lokale Toollogs noch OTel einen `fs_read` mit `agent.name=Phase2RequirementsAgent` zeigen.
  - Der `Phase2RequirementsAgent` bekam im Chat-Kontext vorherige ContextAgent-Nachrichten und Tool-Responses, weil sie in `gen_ai.input.messages` seines RequirementsAgent-Chat-Spans stehen.
  - Das Transkript und die Kontextzusammenfassung waren damit im Input-Kontext des RequirementsAgent sichtbar.
- Nicht sicher beweisbar ist:
  - Welche Teile dieses Kontextes das Modell tatsaechlich intern verarbeitet oder gewichtet hat.
  - Ob die Requirements fachlich vollstaendig aus dem weitergegebenen Kontext oder teilweise aus Modellwissen/Pattern-Ergaenzung entstanden.

**Interpretation:**
Der Run zeigt klar den Unterschied zwischen eigenem Quellenzugriff und weitergereichter MAF Conversation History. Der RequirementsAgent hat nicht selbst gelesen, hatte aber durch MAF Message-Passing Zugriff auf vorherige ContextAgent-Toolergebnisse. Dadurch konnte er ein formal gueltiges Requirements-Dokument schreiben, obwohl der eigene Quellenzugriff nicht belegbar ist.

**Qualitaetsbefund:**
- Positiv: Der Run ist technisch erfolgreich, alle Artefakte entstehen, Validation besteht.
- Positiv: OTel Raw liefert einen starken Nachweis, dass MAF tatsaechlich vorherige Messages und Tool-Responses an nachfolgende Agenten weitergibt.
- Kritisch: Die Agent-Behauptung im Workflow-Output ist unpraezise bis falsch, weil sie so klingt, als habe der RequirementsAgent selbst gelesen.
- Kritisch: Die Requirements enthalten teilweise konkretisierte Werte und Entscheidungen, die fachlich gegen das Transkript geprueft werden muessen, z. B. konkrete Performance-/Verfuegbarkeitswerte oder harte MVP-Entscheidungen aus unsicheren Diskussionen.
- Kritisch: Traceability bleibt grob und nicht verifiziert.

**Learning:**
Phase 2.1A (`message_passing`) ist funktionsfaehig und MAF-nah: Der ContextAgent liest die Primaerquelle, und spaetere Agents arbeiten mit weitergereichter Workflow-History. Das kann fuer vollstaendige Artefakte reichen. Fuer wissenschaftliche Nachvollziehbarkeit reicht es aber nicht, nur auf Workflow-Output oder `evidence`-Argumente zu vertrauen. Es muss getrennt dokumentiert werden:

1. Welche Quellen ein Agent selbst per Tool gelesen hat.
2. Welche Inhalte ihm ueber MAF Message-Passing als Input bereitstanden.
3. Welche Quellen er nur behauptet, aber nicht selbst gelesen hat.

**Konsequenz fuer den naechsten Schritt:**
Der naechste sinnvolle technische Schritt bleibt Message-Context-Logging fuer Phase 2.1A. Ziel ist, die OTel-Raw-Erkenntnis kompakt und gezielt in lokale Run-Logs zu ueberfuehren, z. B. pro Agent:

- `inputMessageCount`
- Rollen der Input-Messages
- vorherige Agent-Namen im Kontext
- Hinweis, ob Transcript-Inhalt im Input-Kontext enthalten war
- Hinweis, ob `state/context.md` im Input-Kontext enthalten war
- eigene Reads vor erstem Write

Damit kann spaeter sauber verglichen werden, ob `message_passing`, `artifact_state` oder `independent_source_reads` die bessere Kontextstrategie fuer Phase 2.1 ist.

---

## Iteration 13 - Prompt-Anpassung nach Run `20260529_101357_d4d49c`: RequirementsPrompt4 und saubereres Message-Passing-Verstaendnis

**RunId:** `20260529_101357_d4d49c`

**Setup:**
- Phase: `phase2_1`
- Context-Strategie: `message_passing`
- Provider: OpenRouter
- Modell im Run: `openai/gpt-oss-120b:free`
- Requirements-Prompt im Run: `RequirementsPrompt3`

**Beobachtung:**
- Der Workflow erreichte alle Specialist Agents.
- `context.md`, `risks.md`, `architecture.md` und `open-questions.md` wurden erzeugt.
- `docs/requirements.md` fehlte am Ende.
- Die Validation meldete korrekt:
  - `MISSING_FILE: docs/requirements.md`
- Der `Phase2RequirementsAgent` behauptete im Workflow-Output:
  - `docs/requirements.md wurde geschrieben.`
  - `Eingelesen: ... context.md und input/transcripts/T9999_chaos.txt.`
- Die Toollogs belegten das nicht:
  - Es gab einen `fs_read` auf `runs/phase2_1/20260529_101357_d4d49c/state/context.md`.
  - Es gab keinen `fs_read` des RequirementsAgent auf `input/transcripts/T9999_chaos.txt`.
  - Es gab keinen `fs_write` des RequirementsAgent auf `docs/requirements.md`.

**Evidence - lokale Logs:**
- `runs/phase2_1/20260529_101357_d4d49c/logs/agents/Phase2RequirementsAgent/tool-calls.jsonl`
- Enthalten ist nur der eigene `fs_read` auf `context.md`, aber kein `fs_write`.
- `runs/phase2_1/20260529_101357_d4d49c/validation/phase2_1.report.json`
- Meldet `docs/requirements.md` als fehlende Datei.

**Evidence - OTel Raw Message-Passing:**
- In `runs/phase2_1/20260529_101357_d4d49c/logs/otel-traces.raw.jsonl` zeigt der Chat-Span des `Phase2RequirementsAgent`:
  - `gen_ai.system_instructions` enthaelt `role: "Phase 2.1 RequirementsAgent"`.
  - `gen_ai.input.messages` enthaelt vorherige ContextAgent-Nachrichten.
  - Darin sichtbar sind:
    - ContextAgent `fs_list` auf `input/transcripts/`
    - ContextAgent `fs_read` auf `input/transcripts/T9999_chaos.txt`
    - Tool-Response mit Transkriptinhalt
    - ContextAgent `fs_write` auf `state/context.md`
    - die Kontextzusammenfassung als Toolcall-Argument
    - danach die Statusmeldung, dass die Context-Datei geschrieben wurde
- Daraus folgt sicher:
  - Der RequirementsAgent hatte ueber MAF Message-Passing Zugriff auf vorherige Tool-Ergebnisse und Kontextinformationen.
  - Die Datei `context.md` wurde nicht automatisch als Dateiobjekt uebergeben, aber ihr Inhalt war im weitergereichten Chatverlauf sichtbar, weil der ContextAgent den Inhalt im `fs_write`-Argument erzeugt hatte.
  - Zusaetzlich konnte der RequirementsAgent die Datei per Tool lesen, was er in diesem Run auch einmal tat.

**Interpretation:**
`RequirementsPrompt3` war fuer Phase 2.1A zu stark auf eigene Source-Reads ausgerichtet. Das passte methodisch nicht mehr sauber zur beobachteten MAF-Message-Passing-Realitaet. Der Agent sollte nicht gezwungen werden, Transkript und Kontext redundant zu lesen, wenn der Workflow-Kontext bereits genuegend Information enthaelt. Der eigentliche harte Vertrag muss sein: Das Requirements-Artefakt muss per `fs_write` entstehen.

**Aenderung:**
- Neue Prompt-Version `RequirementsPrompt4`.
- `run-config.json` setzt `Phase2RequirementsAgent` jetzt auf `RequirementsPrompt4`.
- Fallback-Default in `HostSettings` wurde ebenfalls auf `RequirementsPrompt4` aktualisiert.

**Ziel von RequirementsPrompt4:**
- MAF Message-Passing wird als legitime Arbeitsgrundlage anerkannt.
- `fs_read` bleibt erlaubt, aber wird nur noch bei Unsicherheit oder fehlendem Kontext empfohlen.
- Der Agent wird nicht mehr gezwungen, Transkript und Kontext erneut zu lesen.
- Der harte Abschluss bleibt `fs_write docs/requirements.md`.
- Der Prompt verbietet explizit, den Write nur zu behaupten.

**Zu testen im naechsten Run:**
- Wird `docs/requirements.md` wieder verlaesslich per `fs_write` erzeugt?
- Nutzt der RequirementsAgent den Workflow-Kontext ohne unnoetige Reads?
- Wird im Workflow-Output korrekt unterschieden zwischen:
  - gearbeitet mit MAF Workflow-Kontext,
  - zusaetzlich per `fs_read` gelesenen Dateien?
- Verbessert sich die Stabilitaet gegenueber `RequirementsPrompt3`?
