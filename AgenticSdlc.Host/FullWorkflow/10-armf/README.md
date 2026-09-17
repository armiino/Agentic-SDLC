# 10-armf — Arm F: der freie Ziel-Agent (W2-Mess-Arm)

> Status: LEBEND (README-bei-Code — bei Änderungen mitpflegen)

**KEIN Kettenglied.** Arm F ist der Baseline-Arm der W2-Evaluation (Protokoll:
`Thesis-Docs/Writing/W2-Evaluationskonzept-Kapitel-7.md` §11/§12): ein ernsthaft gebauter,
frei arbeitender Agent, der DIESELBE fachliche Aufgabe wie die Ledger-Stufe löst —
Transkript → belegte atomare Aussagen — aber ohne vorgegebene Stufen (keine Units→Claims→
Checker-Rezeptur im Prompt). Er liest nichts aus dem Core, schreibt keine Wahrheit, hat kein
Gate. Das gemessene System bleibt von diesem Ordner unberührt.

## Aufruf

```
dotnet run --project AgenticSdlc.Host -- arm-f run input/transcripts/meeting-2-extended.txt [--model <id>] [--tool-budget <n>]
```

Bewusster LLM-Akt (Kosten!). Modell-Präferenz wie die Ledger-Runner: `--model` >
`jury.judgeModel` > `agentModel` — für den W2-Vergleich MUSS dasselbe Modell wie in den
Ledger-Läufen wirken.

## Fairness-Verdrahtung (Konzept §12.0)

- **Gleiche kanonische Quelle:** Das Transkript wird mit dem GETEILTEN deterministischen
  Segmenter (`AtomicUnitSegmenter`, `AU-####`-Locators) nummeriert — identisch zur
  Ledger-Nummerierung; abgelegt als `source-units.json`.
- **Neutraler Outputvertrag** (`ArmFModels.cs`): statement · type
  (requirement|architecture|decision|open_question|risk) · sourceUnitIds (AU-Locators) ·
  derivation (explicit|derived) · uncertainty (none|uncertain). Keine Ledger-Interna.
- **Freiheit + Selbstkontrolle:** Tools `read_transcript` (beliebiges Nachlesen),
  `validate_draft` (deterministische Prüfung, beliebig oft), `submit_result` (GENAU EINMAL
  gültig). Budget großzügig (Default 40 Tool-Calls); `submit_result` wird NIE geblockt.
- **W1a/Middleware wie überall:** `AgentChatPipelineBuilder` (OTel, Logging),
  `ToolCallLoggerMiddleware`, `ReasoningSchema.ToolAgentPromptAppendix`
  (`observability.captureReasoning`).

## Artefakte je Lauf (`runs/arm-f/<runId>/`)

| Datei | Inhalt |
| --- | --- |
| `source-units.json` | kanonisch nummerierte Quelle (AU-Locators) — Referenz für P3/P4 |
| `output.json` | der eingereichte Endstand (= F-machine-Messpunkt, Ebene A) |
| `process-profile.json` | Ebene C (§14.5): Modellrunden, Tool-Calls, Re-Reads, Validierungen, Selbstrevisionen (Draft-Hash-Wechsel), Stop-Grund |
| `metrics.json` | Ebene B: WallMs, Aussagen-Zahl, Tokens aus OTel (`gen_ai.usage.*`; null ohne OTel) |
| `logs/` | normale Run-Logs (events, agents/, otel-traces bei ENABLE_OTEL) |

Exit-Codes: 0 = gültig eingereicht · 2 = ohne Einreichung geendet (auch Budget) · 3 = Fehler.

## Bausteine

- `ArmFModels.cs` — Outputvertrag + Prozessprofil + Metrics (Records).
- `ArmFTools.cs` — Werkzeugkasten mit harter det. Validierung (Submit-Once, Analyst-Muster)
  und Ebene-C-Zählern; §14.5-Messregel Selbstrevision = beobachtbarer Draft-Hash-Wechsel.
- `ArmFAgents.cs` — Agent-Fabrik (Klon von `PipelineAgents.Factory` mit eigener Prompt-Phase
  `armf`) + `ModelRoundCounter` (zählt Modellrunden am Basis-Client).
- `ArmFRunner.cs` — CLI-Haut + Lauf-Orchestrierung + Artefakt-Schreiben.
- Prompt: `AgenticSdlc.Host/Prompts/armf/ArmFAgent/ArmFAgent1.txt` (deutsch, R-6-Sprachblock;
  vor W2 einfrieren, danach keine Optimierung — §12.0-7).

Tests: `AgenticSdlc.Tests/FullWorkflow/ArmFTests.cs` (Vertrag/Validierung, Budget,
Selbstrevisions-Zählung, LLM-freier Tool-Loop mit Scripted-Client).
