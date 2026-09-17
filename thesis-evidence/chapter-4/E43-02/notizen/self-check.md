Run `20260519_150331_b9fbd4` wurde nach dem Refactoring ausgefuehrt.

Beobachtungen aus `runs/20260519_150331_b9fbd4/logs/events.jsonl`:

- `RUN_STARTED`
- `MCP_LOCAL_CONNECTED`
- `MCP_GITHUB_CONNECTED`
- `TOOLS_DISCOVERED` mit `fs_exists`, `fs_list`, `fs_read`, `fs_write`
- `AGENT_STARTED`
- echte Toolcalls:
  - `fs_list input/transcripts/`
  - `fs_read input/transcripts/T9999_chaos.txt`
  - vier `fs_write` auf die Pflichtartefakte
  - ein `fs_read docs/requirements.md`
  - ein `fs_write` fuer das Approval-Payload
- `APPROVAL_RECORDED_BY_HOST`
- `RUN_FINISHED`

**Bewertung:**  
Die zentrale Funktionalitaet blieb erhalten:

- Agent nutzt Tools.
- Alle vier Pflichtartefakte werden erzeugt.
- Diffs und Decision-Logs werden geschrieben.
- Host-Validation und Host-Approval funktionieren.
- Snapshots werden erzeugt.

**Auffaelligkeit:**  
Der Agent las im Self-Check nur `docs/requirements.md` erneut, obwohl der Prompt verlangt, jedes Pflichtartefakt erneut zu lesen. Trotzdem behauptete das Approval-Payload sinngemaess, alle Dokumente seien geprueft.

**Learning:**  
Das Refactoring hat die technische Funktionalitaet nicht zerstoert. Gleichzeitig bestaetigt der Run erneut eine zentrale Phase-1-Grenze:

> Prompt-Regeln koennen einen Self-Check anstossen, garantieren aber nicht, dass der Agent alle Pruefschritte vollstaendig und wahrheitsgetreu ausfuehrt.

Damit wird Phase 2 weiter begruendet: Nicht weil Phase 1 technisch scheitert, sondern weil die Qualitaet und Vollstaendigkeit der Selbstpruefung nicht stabil genug ist.
