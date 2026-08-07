# 08-pipeline — pipeline-full: die GANZE Kette als EIN durabler MAF-Graph

> Status: LEBEND (README-bei-Code, B3 07.08.2026).

## Rolle

`pipeline-full` ist der EIN-GRAPH: Transkript ODER Delta rein → komplette Kette → Forward — als EIN
MAF-Workflow mit Checkpoints (Pause/Resume an jedem Human-Gate), zentralem Gate-Responder und metrics.json.
CLI: `pipeline-full run <transkript>` · `run --from-delta <delta.json>` · `resume <runId> [--accept-all]` ·
`status` (pointer.json existiert NUR während Pause).

## Aufbau (die EINE Bauzeit-Stelle: `PipelineFullWorkflow.Assemble`)

```
Entry (typisiert: Transkript|LoadBaselineRequest|Delta) → Front (LedgerIntake [Drift-Guard] →
[LedgerCapsule, BindGateFree] → Summary → Adjudikation) → Baselines/Recipe → Branch (Auto: Core da?)
├─ Bootstrap-Ast: CoreBootstrap → Classify-Spiegel → ADR-Spiegel → Cluster/Backlog → Forward
└─ Betriebs-Ast:  Ingest → [ingest-gate] → Apply → ArchIngest-Strip → [arch-ingest-gate] →
   Classify-Strip → [arch-classify-gate] → ADR-Strip → [adr-gate] → DecisionScan → [decision-gate] →
   PbiUpdate (E-8/A4/Align) → [pbi-gate] → Forward-Snapshot → [github-forward-gate] → Execute/DryRun
```

**10 Human-Gates** (RequestPorts): adjudication · cluster · backlog · ingest · arch-ingest · arch-classify ·
adr · decision · pbi · github-forward. Zweig-Verdrahtung per `AddTo` aus den Stufen-Workflows (eine
Kanten-Quelle für CLI UND Graph — keine Kopien).

## Schlüssel-Mechaniken

- **Zentraler Gate-Responder** (PortId-Dispatch): Politik je Gate aus run-config (accept-all | interactive |
  replay); interactive liest die jeweilige `*-decisions.json` MIT VORRANG vor --accept-all; decision-gate
  vertagt unter accept-all IMMER (R-14-Governance).
- **Bahnen-Strips** (Classify/ADR): zwei dünne Bridges (Betrieb arch-aktiv-gescoped [Smoke-Schutz!] ·
  Bootstrap-Spiegel) speisen EINEN geteilten Strip; Passagier überlebt den Port-Roundtrip per Datei-Vertrag
  (passenger-*.json + lane.json) und wird TYP-GENAU re-emittiert.
- **Lauf-Report-Vertrag** (R-40): `07-ingest/applied/run-report.json` — req-Apply schreibt initial,
  arch-Apply schreibt FORT, Decision-Stufe liest genau diese Datei („ein Konzept = EIN Vertrag").
- **Kapsel-Regel** (R-38): unverdrahtete Ports in Sub-Workflows deadlocken STILL → `BindGateFree`-Wächter
  (shared/) macht daraus einen Konstruktions-Fehler; HITL-in-Kapsel geht offiziell via ForwardMessage.
- **`PipelineAgents.Factory`** — die EINE Agent-Fabrik der Stufen (Prompt + Middleware: ToolCallLogger,
  otel/gen_ai, input-context/response-text; W1a-Denk-Faden am `captureReasoning`-Schalter).

## Beweise (Auswahl)

Ledger-Kapsel-Resume `144056` · R-14-Zyklus `121433` · R-11-E2E `145812`/`151204` · Pause/Resume aller
neuen Gates `181332`. Artefakt-Anatomie: `runs/README.md`.
