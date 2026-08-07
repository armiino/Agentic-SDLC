# runs/ — Die Run-Artefakt-Karte

> Status: LEBEND (README-bei-Code) — bei neuen Run-Arten/Artefakt-Verträgen mitpflegen. Angelegt 06.08.2026 (9l).

Diese Karte erklärt einem neuen Entwickler in 5 Minuten, wie Läufe in diesem Projekt funktionieren,
was in einem Run-Ordner liegt, und was die vielen Unterordner sind.

---

## 1. Die drei Grundprinzipien

**① Ein Run ist eine EPISODE, der Core ist die WAHRHEIT.**
Jeder Lauf bekommt einen Ordner `runs/<art>/<runId>/` (`runId` = Zeitstempel + Suffix, z. B.
`20260806_151204_3c19d2`). Der Lauf LIEST und SCHREIBT die lebendige Projektwahrheit — aber die liegt
**nicht hier**, sondern in **`state/core/project-state.json`** (Repo-Wurzel), ausschließlich über
`ICoreRepository` (Kangal-Wachhund am Save, History-Snapshots in `state/core/history/`). Der Run-Ordner
hält nur die Episode fest: was wurde vorgeschlagen, geprüft, entschieden, angewendet.

**② Wissen reist über KANTEN, Dateien haben drei andere Jobs.**
Im laufenden MAF-Graphen fließt alles als typisierte Message (stateless Executors, R-33-Form). Die
JSON-Dateien im Run-Ordner sind:
1. **Gate-Roundtrip-Verträge** — wenn der Graph an einem Human-Gate pausiert, trägt die Port-Antwort nur
   deinen Entscheid; die Arbeitsdaten überleben als Datei (`plan.json`, `*-request.json`, `run-report.json`).
   Invariante seit R-40: **ein Konzept = EIN Vertrag** (nie zwei Dateien für dieselbe Sache).
2. **Beleg-Artefakte** — Thesis-Evidenz: jede Stufe hinterlässt inspizierbar, was sie tat
   (`gate-report`, `*-attempts.json`, `core-before.json`, `applied/delta.json`).
3. **UI-Verträge** — die Review-CLIs lesen `*-request.json` und schreiben `*-decisions.json`;
   der Resume liest die decisions und beantwortet damit das Gate.

**③ Gescheiterte und Experiment-Läufe werden NIE gelöscht.**
Sie sind Beweise (Reibungs-Log R-1…R-40 verweist auf sie). Experiment-Läufe, die den Core mutieren,
laufen mit Backup + byte-identischem SHA-Restore; der Run-Ordner bleibt als Beleg.

---

## 2. Anatomie eines `fullworkflow/`-Runs (der Haupt-Fall)

`pipeline-full run …` erzeugt EINEN Ordner, dessen Unterordner die Ketten-Stufen spiegeln
(gleiche Nummern wie der Code unter `AgenticSdlc.Host/FullWorkflow/`):

```
runs/fullworkflow/<runId>/
├── config.json                Momentaufnahme der run-config dieses Laufs
├── 01-ledger/                 Transkript → Claims (Ledger-Kapsel; ledger-run.json = H1-Anker)
├── 04-delta/                  das Meeting-Delta (project-state.json) — der Betriebs-Eingang
├── 06-backlog/                re-clarify/Backlog-Stufe (cluster/backlog-Gates)
├── 07-ingest/                 Tor 1 requirements: plan.json · gate-report · attempts ·
│   └── applied/               delta.json (req-Beleg) · run-report.json (DER Lauf-Report-Vertrag,
│                              von jedem Aspekt-Apply fortgeschrieben — R-40) · core-before.json
├── 07-arch-ingest/            Tor 1 architecture (8. Gate) — gleiche Dateiform
├── 07-arch-classify/          Rollen-Gate (9. Gate): plan · classify-attempts ·
│                              arch-classify-request.json ⇄ classify-decisions.json (UI-Vertrag)
├── 07-adr/                    ADR-Stufe (10. Gate): plan · adr-attempts · adr-request.json ⇄
│                              adr-decisions.json · passenger-*.json/lane.json (Bahnen-Plumbing) ·
│                              projected-docs/ (Beleg-Kopie der geschriebenen docs/adr + architecture.md
│                              bei Experiment-Läufen mit Restore)
├── 07-decision/               Tor 2 (decision-gate): Auflösungs-Plan ⇄ decision-gate-decisions.json
├── 07-pbi-update/             Placement/Align (pbi-gate): pbi-plan.json ⇄ decisions
├── 07-github/                 Tor 3 Forward: Snapshot, Plan, forward-decisions
├── checkpoints/               MAF-Checkpoints (Pause/Resume; pointer.json existiert NUR während Pause)
├── logs/
│   ├── events.jsonl           DER Faden: jedes Stufen-/Gate-/Tool-Ereignis (erste Anlaufstelle!)
│   ├── agents/<AgentName>/    je LLM-Agent: input-context.md · response-text.md (Denk-Faden,
│   │                          W1a-Schalter) · tool-calls.jsonl (die Beweis-Goldader) · events.jsonl
│   ├── otel-traces.jsonl      GenAI-Telemetrie (gen_ai.*); .raw enthält auch Modell-Reasoning
│   ├── decision-log.jsonl     Gate-Entscheide chronologisch
│   └── metrics.json           Stufen-Metriken (Dauer/Tokens) — W2-Messdraht
└── snapshots/                 Zwischenstände (z. B. github-snapshot)
```

**Praktische Einstiege:**
- „Was ist passiert?" → `logs/events.jsonl` (grep nach `GATE_ANSWERED`, `*_APPLIED`, `*_SKIPPED`).
- „Was hat der Agent WIRKLICH getan?" → `logs/agents/<Name>/tool-calls.jsonl`.
- „Wo pausiert etwas?" → `dotnet run --project AgenticSdlc.Host -- pipeline-full status`.
- Pausiert ein Gate, nennt die Konsole wörtlich den Review-Befehl (`adr-review <runId>` usw.).

---

## 3. Die Unterordner-Karte (was ist das alles?)

**Aktive Run-Arten** (entstehen durch heutige Befehle):

| Ordner | Was | Entsteht durch |
|---|---|---|
| `fullworkflow/` | **Haupt-Ordner:** der Ein-Graph (komplette Kette) | `pipeline-full run/resume` |
| `ledger/` | isolierte Ledger-Stufe (Transkript→Claims) | `ledger-build` |
| `recipe/` | Baseline-Rezepte (Ledger→Baselines-Artefakte) | `recipe`-Befehle |
| `ingestion/` | isoliertes Tor 1 requirements (CLI-Bahn) | `ingest-hitl` |
| `arch-ingestion/` | isoliertes Tor 1 architecture (CLI-Bahn, A1d) | `ingest-architecture-hitl` |
| `decision/` | isolierte Tor-2-Auflösung | decision-CLIs |
| `pbi-update/` | isolierte Placement-/Align-Stufe | `pbi-update`-CLIs |
| `github-forward/` · `github-snapshot/` | Tor 3 (Issues-Projektion / Repo-Snapshot) | forward-/snapshot-CLIs |
| `bootstrap/` | Bootstrap-Ast (B0–B6: frischer Core aus Baselines) | bootstrap-Befehle |
| `l4-re-clarify/` | ⚠️ **LIVE-Backlog-State** — der NEUESTE Run wird als Input gelesen! Nicht aufräumen. | re-clarify |
| `_ui-sandbox/` | LLM-freie UI-Sandboxes je Review (feste Beispiel-Requests zum UI-Ansehen) | von Hand gepflegt |

**Beleg-/Experiment-Ordner** (`_`-Präfix = kein Run-Zähler, von Hand angelegt):

| Ordner | Was |
|---|---|
| `_e2e-backup/` · `_experiment-9g-graph/` · `_messpunkt-9g/` | Core-Backups + Evidenz bestimmter Experimente (SHA-Restore-Disziplin) |
| `core-heal-features/` | Audit des R-36-Einmal-Heilwerkzeugs (03.08.; Werkzeug danach entfernt) |
| `project-state/` | benannte Consumables/Deltas (z. B. `e2e-meeting2`) als Eingaben |

**Historisch:** abgeschlossene Ären liegen komplett in **`runsArchive/`** (Struktur 1:1, Thesis-Evidenz) —
u. a. `phase1/`, `phase2*/`, `l3/`, `l4*/`, `derivation/`, und seit 06.08. auch `baseline-fanout/`,
`contract-critic/`, `ledger-validate/`, `ledger-adjudicate-refine/` (Aufräum-Runde per Archiv-Regel §4;
das leere `pipeline/` wurde gelöscht — Ex-`pipeline-hitl`, ersetzt in Schritt 5 ③).

---

## 4. Lauf → Erzählung: der EVIDENZ-INDEX

Rohe Run-Ordner sind DATEN — ihre Bedeutung steht in den (lokalen) Docs: Reibungs-Log (E2E-RUNBOOK),
iteration notes, r11-Plan. Der Rückweg ist generiert: **`runs/EVIDENZ-INDEX.md`** listet JEDEN Lauf mit
seinen Doc-Referenzen (oder „unreferenziert"). Neu erzeugen: `python3 tools/eval/evidenz-index.py`.

**Lese-Pfad für die Thesis:** `PROJEKT-CHRONIK` (Story) → R-Log/iteration note (Befund) →
EVIDENZ-INDEX (runId) → roher Ordner (Beweis). Nie andersherum anfangen.

**Archiv-Regel:** Nach `runsArchive/` (Struktur 1:1) wandern nur GANZE Ordner-Arten abgeschlossener Ären —
nie einzelne Läufe herauspicken (Configs/Docs referenzieren Pfade). NIE verschieben: `l4-re-clarify/`
(Live-Input!), `_ui-sandbox/`, `project-state/`, `ledger/`+`recipe/` (Consumable-Inputs der run-config)
und alles, was der Index als referenziert führt, ohne die Referenz-Doku mitzuziehen.

---

## 5. Steuerung (Kurzfassung)

Alles Schaltbare wohnt in **`run-config.json`** (Wurzel): Modelle (`agentModel`, `jury.judgeModel`) ·
Gate-Politik je Port (`gates.<name>.policy` = accept-all | interactive | replay) ·
`observability.captureReasoning` (off|optional|enforced) · `evidenceAgent.{transcript,ledgerRun}` ·
`fullworkflow.repo`/`tokenEnv`. Secrets in `.env`. Verifikation: `dotnet test` + `bash tools/smoke-hitl.sh`.
