# Phase 1 – Iteration Notes 

## Iteration 1 
**Ziel:** End-to-end Run + MCP nutzung + Agent start.

**Beobachtung:** Agent “beschreibt” Toolcalls als JSON im Text, aber es entstehen keine echten Toolcall Events für `fs_write`/`request_approval`; `docs/` bleibt leer.

**Learning:** Toolcalls im Text sind kein Beleg für Tool-Ausfürhung. Evidenz muss aus `events.jsonl` kommen.

---

## Iteration 2 – Tool Invocation Pipeline aktivieren
**Hypothese:** Function/Tool Invocation ist in der Chat-Pipeline nicht aktiv.

**Änderung:** Wechsel auf `IChatClient` + `ChatClientBuilder(...).UseFunctionInvocation()` (OllamaSharp), damit Toolcalls automatisch ausgeführt werden.

**Evidenz:** In `events.jsonl` erscheinen echte `TOOL_CALL_STARTED/FINISHED` Einträge für `fs_write` und später `request_approval`.

**Learning:** Für agentischen Tool-Use ist Function Invocation in der Pipeline ein technisches Muss.

---

## Iteration 3 – Policy: relative Pfade
**Beobachtung:** `fs_list` gab absolute Pfade zurück; RootPolicy blockt absolute Pfade (relative-only). Folge: Toolcalls scheitern mit isError=true.

**Änderung:** `fs_list` gibt repo-relative Pfade zurück (z. B. `input/transcripts/T0001.txt`).

**Evidenz:** `fs_read` funktioniert stabil, keine isError=true Fehler.

**Learning:** Root policies bleiben strikt; Toolausgaben müssen policy-kompatibel sein.

---

## Iteration 4 – Approval Tool stabilisieren
**Beobachtung:** `request_approval` scheiterte durch Contract mismatch.

**Änderung:** Contract stabilisiert auf `request_approval(action,payload)`; Payload enthält `runId`, MCP Server schreibt Approval nach `runs/<runId>/approvals/`.

**Evidenz:** “Approval request recorded.” + Datei `approval_*.json`.

**Learning:** Governance muss technisch enforced sein; Approval ist ein Run-Artefakt, kein “Prompt-Versprechen”.

## Iteration 5 Test mit langem, unstrukturiertem Transkript (T9999_chaos)

**RunId:** `20260225_123947_560ab9`  
**Ziel:** Phase 1 (Single-Pass) bewusst mit hoher Komplexität/Unklarheit belasten, um Grenzen sichtbar zu machen 

### Setup
- Input: `input/transcripts/T9999_chaos.txt` (3 Sprecher, Widersprüche, Prioritätskonflikte, Kontextsprünge)
- Erwartung: Agent erzeugt alle Pflichtartefakte `docs/requirements.md`, `docs/open-questions.md`, `docs/risks.md`, `docs/architecture.md` und erstellt genau **eine** Approval-Request am Ende.

### Beobachtungen (Evidence)
Aus `runs/20260225_123947_560ab9/logs/events.jsonl`:

1) **Tool-Autonomie und Tool-Execution funktionieren**
- Agent nutzt MCP Tools real: `fs_list`, `fs_read`, `fs_write`, `request_approval`.
- Das zeigt: Toolchain (Host - MCP - Tools) ist grundsätzlich stabil.

2) **Instabilität: mehrfaches Schreiben derselben Artefakte**
- `docs/requirements.md` wurde in diesem Run **mehrfach überschrieben** (mehrere `fs_write`-Calls auf denselben Pfad zu verschiedenen Zeitpunkten).
- Gleiches gilt für `docs/open-questions.md` und `docs/risks.md`.
  **Interpretation:** Single-Pass erzeugt "Drafts" ohne explizit zu konsolidieren bzw finalisieren.

3) **Governance-Instabilität: mehrfaches `request_approval`**
- `request_approval` wurde **dreimal** aufgerufen → 3 Approval-Dateien:
    - `approval_20260225124059.json`
    - `approval_20260225124336.json`
    - `approval_20260225124347.json`
      **Interpretation:** Es fehlt ein deterministischer step der das ganze offiziell beendet.. der Agent “schließt mehrfach ab”

4) **Output-Contract im Run verletzt: `docs/architecture.md` nicht aktualisiert**
- Im Eventlog gibt es **keinen** `fs_write` auf `docs/architecture.md`.
- Im Repo existiert `docs/architecture.md`, aber das File-Datum ist **älter** (von früherem Run).
  **Konsequenz:** Die sichtbaren Artefakte im `docs/` Ordner sind ohne Run-Snapshot/RunId-Zuordnung nicht eindeutig dem aktuellen Run zuordenbar → Risiko für Reproduzierbarkeit.

5) **Drift.. vllt als overload zu implmenetieren (noch unklar ob tiefere analyse des verhaltens hier nötig..)**
- Transkript nennt Userzahlen “200 / 2000 / 20.000”.
- Output enthält “750 bis 20.000” (nicht im Input enthalten).
  **Interpretation:** Bei langen unstrukturierten Inputs steigt die Gefahr, dass Details nicht stabil übernommen werden.

6) **Traceability nicht umgesetzt**
- Output enthält Formulierung “Traceability … coming soon” bzw. keine Chunk-IDs/Marker.
  **Interpretation:** Ohne expliziten Schritt “Chunking + IDs” liefert Single-Pass keine belastbare Nachvollziehbarkeit.

### Learning / Schlussfolgerung
- Phase 1 ist geeignet als Minimal-Durchstich (Tooling, Governance-Grundstruktur, Observability).
- Unter Stress (lange, widersprüchliche Inputs) zeigt Single-Pass strukturelle Defizite:
    - fehlende deterministische Finalisierung (einmal schreiben, einmal approval)
    - fehlende stabile Traceability
    - fehlende Konflikt-/Widerspruchsbehandlung und Konsolidierung
    - erhöhte Drift bei Fakten/Details

**Explorative Konsequenz:** nächste Phase ist gerechtfertigt, um die Verarbeitung zu strukturieren (Workflow/Sequenzierung, Rollen/Specialists), sodass:
- erst extrahiert (Chunking, Fakten/Constraints),
- dann konsolidiert (Konflikte/Trade-offs),
- dann final geschrieben wird (genau einmal pro Artefakt),
- und Governance als letzter Step exakt einmal erfolgt.