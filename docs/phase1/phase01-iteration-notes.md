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

## Iteration 5..