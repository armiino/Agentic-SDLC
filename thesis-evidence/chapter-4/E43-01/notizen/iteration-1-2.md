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

