# Evidenz-Agent (Kapitel B) — Code- & Datei-Landkarte

> **Zweck.** Genau auflisten, **wo welche Datei liegt** und **welche Rolle** sie im Evidenz-Agent-Modus spielt —
> getrennt nach: neu gebaut · geänderte Anknüpfungspunkte · wiederverwendete Bausteine (unverändert) · Ein-/Ausgaben.
> Alle Pfade relativ zum Repo-Root. Stand: E0–E2 (2026-07-06). Plan: `ReusePlan.md`, Verlauf: `iteration-notes.md`.

---

## 1. Neu gebaut (dieses Kapitel)

**Code**
```text
AgenticSdlc.Host/Phases/Phase2/Evidenz-Agent/
  EvidenceAgentRunner.cs      Der dünne Runner: baut den Agenten aus wiederverwendeten Bausteinen (Direkt-Chat,
                              KEINE MCP-Tools), löst die Quelle je Arm auf, komponiert Prompt (Header + geteilter
                              Kern), ruft den Agenten, schreibt requirements.md. Enthält ProjectLedger() (Arm B).
  ReusePlan.md                Der Plan/die Struktur des Kapitels (A/B-Arme, Scope, Meilensteine E0–E4, §4b Shared-State).
  iteration-notes.md          Verlaufs-Doku (E0/E1/E2 …), Format Beobachtung→Evidenz→Interpretation→Learning→next.
  CodeMap.md                  Dieses Dokument.
```

**Prompts** (Textdateien, config-selektiert — „wie gehabt")
```text
AgenticSdlc.Host/Prompts/phase2_evidence/EvidenceRequirementsAgent/
  _shared-core.txt              GETEILTER KERN (Aufgabe + Format + Treue-Regeln) — byte-identisch für BEIDE Arme.
  RequirementsFromTranscript1.txt  Arm-A-Header (Quelle = Transkript).
  RequirementsFromLedger1.txt      Arm-B-Header = Consumer-Contract (approved-only, nicht verstärken, id zitieren).
```
> Der Runner lädt `Header + _shared-core` und hängt sie zusammen → Prompt-Parität beweisbar (Diff = nur der Header).

---

## 2. Geänderte Anknüpfungspunkte (bestehender Code, additiv)

```text
AgenticSdlc.Host/Configuration/RunConfig.cs      + Klasse EvidenceAgentConfig (source/artifact/transcript/
                                                   ledgerRun/repetitions) + Property EvidenceAgent.
AgenticSdlc.Host/Configuration/HostSettings.cs   + Felder EvidenceSource/Artifact/Transcript/LedgerRun/Repetitions;
                                                   + Parsing in FromRuntimeConfig; BuildPromptSelection wählt für
                                                   phase2_evidence die Section des aktiven Arms (evidenceSource).
AgenticSdlc.Host/Program.cs                       + ResolveRunFolder-Zweig -> "phase2evidenz-agent/<arm>";
                                                   + AgentPhase-switch-Zweig "phase2_evidence" -> RunPhase2EvidenceAsync;
                                                   + UnknownPhase-Liste erweitert.
AgenticSdlc.Host/Run/RunContext.cs               ~ PhaseSelector darf Unter-Ordner enthalten (pro Segment normalisieren)
                                                   -> nested Runs. Einzelsegment-Selektoren (phase2_1/phase2B) unverändert.
run-config.json                                   + Block "evidenceAgent"; + prompts-Section "phase2_evidence" (pro Arm).
```
> Diese fünf Stellen sind der **gesamte** Eingriff in Bestehendes. `Phase2Runner`, `Ledger/`, `Phase2B/`, die Jury
> sind **unangetastet** — alte Modi/Runs bleiben lauffähig.

---

## 3. Wiederverwendete Bausteine (UNVERÄNDERT — worauf sich der Runner stützt)

```text
AgenticSdlc.Host/Llm/ChatClientFactory.cs                       Chat-Client aus HostSettings (Provider/Modell).
AgenticSdlc.Host/Observability/AgentChatPipelineBuilder.cs      Observability-Pipeline (Input/Decision/Tool-Logs).
AgenticSdlc.Host/Observability/ToolCallLoggerMiddleware.cs      Tool-Call-Logging (hier ohne Tools, aber Pipeline-konform).
AgenticSdlc.Host/Prompts/PromptProvider.cs                      Lädt Prompts nach Prompts/<phase>/<agent>/<name>.txt.
AgenticSdlc.Host/Run/RunContext.cs                              Run-Ordner/Logs/Events (auch §2 geändert, s.o.).
```

**Datenmodell für Arm B** (Ledger einlesen — nicht neu definiert, wiederverwendet)
```text
AgenticSdlc.Host/Phases/Phase2/Ledger/AdjudicationModels.cs                 record ConsumableLedger (claims[]).
AgenticSdlc.Host/Phases/Phase2/Evaluation/PerItem/EvidenceFirstSpikeModels.cs  record SemanticLedgerEntry (id/
                                                                            proposition/facetten/disposition/…).
```

**MAF/AI**
```text
Microsoft.Agents.AI            chat.AsAIAgent(...), AIAgent.RunAsync(...)  (NuGet, wie im Rest des Hosts).
Microsoft.Extensions.AI        IChatClient, ChatMessage, ChatRole.
```

---

## 4. Eingaben (Quellen der beiden Arme)

```text
Arm A (transcript):  input/transcripts/Interview-Einrichtung.txt            (run-config: evidenceAgent.transcript)
Arm B (ledger):      runs/ledger/20260704_131614_018865/step-03b-adjudicated/consumable.json
                                                                            (run-config: evidenceAgent.ledgerRun)
```
> Beide MÜSSEN zusammenpassen: das Transkript (Arm A) ist dasselbe, aus dem der Ledger (Arm B) gebaut wurde —
> sonst ist der A/B-Vergleich nicht valide.

---

## 5. Ausgaben (was der Modus produziert)

```text
runs/phase2evidenz-agent/transcript/<runId>/requirements.md   Arm A (Baseline, ohne Quellenangaben).
runs/phase2evidenz-agent/ledger/<runId>/requirements.md       Arm B (mit [sourceClaimId]-Zitaten).
runs/phase2evidenz-agent/<arm>/<runId>/{config.json,logs/,…}  Standard-Run-Snapshot + Observability (via RunContext).
```
> Bei k>1 (repetitions): `requirements.01.md`, `requirements.02.md`, … pro Arm.

---

## 6. Config-Kette (wie ein Lauf gesteuert wird)

```text
run-config.json
  "agentPhase": "phase2_evidence"                  -> aktiviert den Modus
  "evidenceAgent": { source, artifact, transcript, ledgerRun, repetitions }
  "prompts": { "phase2_evidence": { "<arm>": { "EvidenceRequirementsAgent": "<promptName>" } } }
        │
        ▼  RunConfig -> HostSettings.FromRuntimeConfig (Felder + BuildPromptSelection[arm])
        ▼  Program.cs: ResolveRunFolder -> runs/phase2evidenz-agent/<arm> ; switch -> EvidenceAgentRunner
        ▼  EvidenceAgentRunner: Header+Kern laden, Quelle auflösen, Agent bauen, requirements.md schreiben
```

---

## 7. Noch NICHT gebaut (kommt in E3/E4)

```text
Evidenz-Agent/FaithfulnessChecker.cs   NLI-Treue-Checker (entailment|neutral|contradiction) + Eigen-Validierung.  [E3]
Evidenz-Agent/AbTestRunner.cs (o.ä.)   k Läufe/Arm + deskriptiver A/B-Vergleich.                                   [E4]
```
