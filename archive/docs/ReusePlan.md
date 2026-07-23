# Evidenz-Agent (Kapitel B) — Reuse-Plan

> **Zweck.** Kapitel B) = „Ist der Ledger nachweislich nützlicher als das Rohtranskript?" (der Nutzennachweis).
> Dieser Plan zeigt, wie wir das **mit dem bestehenden 4-Agenten-DAG** bauen — als **neuer config-gesteuerter
> Modus**, der sich additiv einklinkt, **ohne** den alten Code umzubauen. Alte Läufe (phase1, phase2_1, phase2B,
> `ledger-*`) bleiben unverändert lauffähig.
>
> Bezug: `LedgerProgressDocumentation/NeedToFinish.md` (B-Minimal-Bar), `HowTheLedgerWorks.md` (was der Ledger
> liefert), `HumanInTheLoop-Adjust.md` (Consumer-Contract-Prosa).

---

## Einordnung: das hier ist das EXPERIMENT, nicht das Endziel

```
ENDZIEL        Das volle agentische System (4-Agenten-DAG) erledigt die echte SDLC-Arbeit —
               alle Artefakte (requirements, risks, architecture, open-questions), produktiv.
JETZIGER PUNKT UNTERSUCHUNG: führt der Ledger zu besseren / besser reviewbaren Artefakten?
               = ein Experiment, das die Architektur-Entscheidung informiert:
                 "routen wir das Produktivsystem durch den Ledger — ja/nein?"
MINIMAL-BAR    nur requirements / 1 Transkript / Treue-Metrik = die BILLIGSTE valide Messung
               dieser Frage. NICHT das Endprodukt.
```

Die enge Minimal-Bar ist **methodische Sparsamkeit** (saubere, bezahlbare Messung), kein Ziel. Steht der Befund
(„Ledger verbessert Treue messbar"), ist die **Design-Entscheidung** getroffen — die Produktiv-Agenten
konsumieren den Ledger. **Dann skaliert genau dieser additive Modus auf den vollen DAG** (alle 4 Agenten, alle
Artefakte, ledger-geführt): kein Umbau, nur „aufdrehen" (`evidenceAgent.artifact` erweitern, ggf. Kontext-
Strategie/Shared-State wieder aktivieren, s. §4b). Der ganze Plan ist so geschnitten, dass der Weg vom
Experiment zum produktiven Voll-System **kontinuierlich** ist.

### Kapitel-Abfolge (Roadmap)

```text
A)  Evidence-Ledger gebaut + intrinsisch sauber                    ✅ hergestellt
B)  Bringt der Ledger dem EINZELNEN Agenten treuere Artefakte?     ← DIESES Kapitel (per-Agent, Treue-Metrik)
C)  MULTI-AGENTEN-SYSTEM: wie sind die Agenten zu orchestrieren?   ← DANACH (Systemebene)
    (DAG-Koordination, Kontext-Strategien A/B/C, Shared State §4b, ggf. Manager/Repair-Loops)
```

**B misst pro Agent** (macht der Ledger *ein* Artefakt treuer); **C fragt auf Systemebene** (wie die Agenten
zusammenarbeiten). C ist der Ort, an dem die Kernfrage der Arbeit — *„welche agentischen Mechanismen sind
nützlich, nötig oder Overengineering?"* — auf der **Orchestrierungsebene** wieder aufschlägt. Dieser Plan
(Kapitel B) baut bewusst so, dass er C **nicht verbaut**, sondern vorbereitet (additiver Modus, orthogonale
Achsen, Ledger als möglicher ProjectState).

---

## 0. Leitprinzip: einklinken, nicht umbauen

Die Lösung ist bereits modular: `agentPhase` wählt die Phase, `phase2ContextStrategy` die Variante, Prompts
werden per Config selektiert, der Run-Ordner wird aus der Phase abgeleitet. **Genau dieses Muster nutzen wir
weiter.** Kapitel B ist ein **weiterer Modus**, kein neuer Baukasten.

- **Neuer `agentPhase`-Wert** → eigener Dispatch-Zweig, eigener Runner.
- **Runs** unter `runs/phase2evidenz-agent/<arm>/<runId>` (Split nach Arm).
- **Prompts additiv** (neue Versionen, alte nie überschreiben — config-selektiert).
- **Alter Code:** unangetastet. `phase1`/`phase2_1`/`phase2B` + alle `ledger-*`-Kommandos laufen wie bisher.

---

## 1. Was 1:1 wiederverwendet wird (kein/kaum Code)

| Baustein | Datei/Ort | Nutzung |
|---|---|---|
| 4-Agenten-DAG + Bauart | `Phase2Workflow.cs`, `Phase2AgentFactory.cs` (`chat.AsAIAgent`, `WorkflowBuilder`) | Agenten + Graph unverändert; nur Prompts + Input tauschen |
| Prompt-Loading | `Prompts/<phase>/<agent>/<name>.txt` + Config-Selektoren | neue Prompt-Versionen anlegen, per Config wählen |
| Run-Infra / Observability | `Run/RunContext.cs`, `Observability/AgentChatPipelineBuilder`, `OtelRunExporters` | identisch — neue Runs, gleiche Logs/Snapshots |
| Chat-Client / Modell | `Llm/ChatClientFactory`, `HostSettings` | identisch |
| NLI-/Prüf-Muster | `Ledger/FacetValidator.cs` (Batch, fixer Nenner, JSON-Schema, temp=0) | Vorlage für den Treue-Checker |
| Ledger-Output | `runs/ledger/<runId>/step-03b-adjudicated/consumable.json` | Input für Arm B (schon fertig: Interview 018865) |

**Kernaussage:** Die Agenten-/Workflow-Logik steht. Neu sind im Wesentlichen **Prompts + ein Input-Adapter +
der Treue-Checker** — nicht der Agentenapparat.

---

## 2. Config-Steuerung (`run-config.json`) — der einzige „Bedien-Hebel"

Analog zu `ledger.adjudicationMode`. Neuer Block:

```jsonc
"agentPhase": "phase2_evidence",          // NEUER Modus (statt phase1/phase2_1)
"evidenceAgent": {
  "source":     "ledger",                 // "ledger" (Arm B) | "transcript" (Arm A)
  "artifact":   "requirements",           // B-Minimal-Bar: vorerst NUR requirements
  "ledgerRun":  "runs/ledger/20260704_131614_018865/step-03b-adjudicated/consumable.json",
  "repetitions": 1                        // k=3-5 für die Verteilungs-Messung (M2-gekoppelt)
}
```

- `source=transcript` → **Arm A** (frei, Baseline).
- `source=ledger` → **Arm B** (ledger-geführt).
- Alles andere (Modell, Prompt-Selektoren, Jury) bleibt wie gehabt.

---

## 3. Der additive Eingriff — GENAU 3 Stellen

**(a) `RunConfig.cs` / `HostSettings.cs`** — neuen Block `evidenceAgent` einlesen (wie `LedgerConfig`).

**(b) `Program.cs` → `ResolveRunFolder(...)`** — neuer Zweig:
```csharp
settings.AgentPhase == "phase2_evidence"
    ? $"phase2evidenz-agent/{settings.EvidenceSource}"   // -> runs/phase2evidenz-agent/ledger|transcript/<runId>
    : …
```
(RunContext bildet `runs/<PhaseSelector>/<runId>` — der Sub-Split fällt gratis ab.)

**(c) `Program.cs` → `AgentPhase switch`** — neuer Zweig `"phase2_evidence" => EvidenceAgentRunner.RunAsync(...)`.

Mehr Berührung mit altem Code gibt es nicht. `skip`-Analogie: ohne `agentPhase=phase2_evidence` ändert sich **nichts**.

---

## 4. Die zwei Arme = der A/B-Vergleich (der DAG-Reuse)

Beide Arme fahren **denselben** DAG (bzw. für die Minimal-Bar denselben Requirements-Agenten). Unterschied =
**Input + Prompt-Kontrakt + Output-Format**:

| | Arm A (`transcript`) | Arm B (`ledger`) |
|---|---|---|
| Input | Rohtranskript | `consumable.json` (approved-only) |
| Prompt | „schreibe requirements aus dem Transkript" (≈ bestehender Prompt) | „schreibe requirements aus dem Ledger; **keine Verstärkung**; führe `sourceClaimIds` mit" |
| Output | `requirements.md` (ohne IDs) | `requirements.md` **+ pro Aussage `sourceClaimIds`** |
| Ziel | Baseline (frei) | Testarm (ledger-geführt) |

Der **Consumer-Contract** (open→decided verboten, desired→must verboten, disposition-Routing, IDs mitführen)
wird für Arm B als **konkrete Prompt-Spec** verschriftlicht (bisher nur Prosa in NeedToFinish §2).

---

## 4b. Verhältnis zu Phase 2.1B (Shared State) — orthogonal, NICHT geerbt

Es gibt zwei DAG-Implementierungen: **Variante A** (`Phase2Workflow.cs`, message_passing, Agenten direkt am
WorkflowBuilder) und **Variante B** (`Phase2B/`, artifact_state, custom Executoren + MAF Shared State
`Phase2BProjectState`, geseedet aus `context.md`, plus StatePolicy/StateAccessLogger/ArtifactQualityGate). Die
aktive Config nutzt B. **Wichtig für diesen Modus:**

- **Zwei orthogonale Achsen** — nicht vermischen:
  - *Kontext-Strategie* (A/B/C): wie die 5 Agenten Kontext **untereinander** weitergeben.
  - *Source-Arm* (`evidenceAgent.source` = transcript | ledger): woher der Agent seine **Quelle** nimmt.
  `evidenceAgent.source` ist **NICHT** `phase2ContextStrategy`.
- **Der Shared-State-Apparat wird NICHT geerbt.** Er ist an `agentPhase=phase2_1 + phase2ContextStrategy=artifact_state`
  gebunden. Der neue `agentPhase=phase2_evidence` triggert ihn nicht → keine versehentliche Kopplung.
- **Minimal-Bar (nur `requirements` = im Kern EIN Agent):** keine Inter-Agenten-Kontextweitergabe → die A/B-Strategie
  ist **moot**. Daher **bewusst schlicht**: Einzel-Requirements-Agent, Quelle als direkter Input (A-Stil / plain
  invocation), **ohne** Phase2B-Maschinerie. Das ist die geringste Kopplung.
- **Konzeptioneller Bonus / Thesis-Punkt:** Die `consumable.json` **IST** die Weiterentwicklung der
  `artifact_state`-Idee — ein strukturierter, evidenzgebundener, ID-tragender ProjectState statt `context.md`.
  Arm B liest also aus genau so einem Zustand — nur reicher.
- **Spätere Erweiterung (volle 4 Artefakte):** *dann* wird die Kontext-Strategie wieder relevant. Optional lässt
  sich Phase2B-Shared-State wiederverwenden, indem der **Ledger in den ProjectState geseedet** wird (statt
  `context.md`). Erweiterungs-Entscheidung, **kein** Teil der Minimal-Bar.

---

## 5. Neue Bausteine (klein, additiv) — im Ordner `Evidenz-Agent/`

```text
Evidenz-Agent/
  EvidenceAgentRunner.cs        Dispatch: liest source/artifact, baut Arm A|B über Phase2AgentFactory,
                                schreibt runs/phase2evidenz-agent/<arm>/<runId>/requirements.md
  SourceContextAdapter.cs       liefert dem Agenten den Kontext: Transkript (A) ODER consumable-Projektion (B,
                                approved-only, als lesbare + ID-tragende Sicht)
  FaithfulnessChecker.cs        NLI-Treue-Checker (reuse FacetValidator-Muster): pro Artefakt-Aussage
                                entailment|neutral|contradiction gegen die beanspruchte Quelle; + Eigen-Validierung
  AbTestRunner.cs               fährt k Läufe/Arm, wendet den Checker an, berichtet DESKRIPTIV (Median+Spanne)
  iteration-notes.md            Doku wie gewohnt (Beobachtung -> Evidenz -> Interpretation -> Learning -> next)
  ReusePlan.md                  dieses Dokument
Prompts/phase2_evidence/requirements/…   neue Prompt-Versionen (A + B), config-selektiert
```

Neue CLI-Kommandos (analog `ledger-*`, damit einzeln testbar):
```text
evidence-agent            (bzw. Default-Lauf via agentPhase=phase2_evidence)  -> ein Artefakt-Lauf (Arm A|B)
evidence-faithfulness     <artifact.md> <quelle>                              -> Treue-Check eines Artefakts
evidence-ab               <artifact> [k]                                      -> A/B über k Läufe, deskriptiver Vergleich
```

---

## 6. Scope-Deckel (B-Minimal-Bar — VOR dem Bauen fix)

```text
1 Artefakttyp   nur requirements (nicht risks/architecture/open-questions)
1 Transkript    Interview-consumable (018865) — ist schon adjudiziert/fertig
1 Reviewer      Autor (Zirkularität über die Metrik umgangen)
Metrik          TREUE, nicht Güte: Nicht-Entailment-Rate gegen die beanspruchte Quelle
Statistik       k=3-5 Läufe/Arm (temp>0), DESKRIPTIV berichten ("B 9% [7-12] vs A 34% [29-40]"), NIE "signifikant"
```
Der volle 4-Agenten-DAG (alle Artefakttypen) + Multi-Transkript = **spätere Erweiterung**, nicht die Kern-Bar.

---

## 7. Meilensteine (jeder für sich lauffähig/verifizierbar)

```text
E0  Gerüst: Config-Block + ResolveRunFolder + EvidenceAgentRunner (leerer Durchstich, Build grün, runs-Ordner entsteht)
E1  Arm A: transcript -> requirements.md   (Reuse bestehender Requirements-Agent, minimal)
E2  Consumer-Contract als Prompt-Spec + Arm B: ledger -> requirements.md MIT sourceClaimIds
E3  FaithfulnessChecker (NLI) + dessen EIGEN-Validierung (wie L3-B: erst Checker belegen, dann nutzen)
E4  A/B über k Läufe -> deskriptiver Treue-Vergleich  (= der eigentliche B-Befund)
```
Parallel (Mess-Track, kein Blocker): M1 zweiter Labeler, M2 Wiederholungen.

---

## 8. Was bewusst NICHT passiert (Reuse-Garantie)

- **Kein Umbau** von `Ledger/`, `Phase2B/`, `Evaluation/` (Jury), `Phase2Workflow`/`Phase2AgentFactory`.
- Alte Modi bleiben wählbar: `agentPhase = phase1 | phase2_1 (+ artifact_state/message_passing) `; alle
  `ledger-*`-Kommandos unverändert → **du kannst weiter alte Runs fahren**.
- Alte Prompts werden **nie überschrieben** (neue Versionen additiv, per Config gewählt).
- Die alte Güte-Jury bleibt als Werkzeug bestehen — sie ist aber **nicht** das B-Instrument (B misst Treue).

---

## 9. Doku-Disziplin (in diesem Ordner)

- **`iteration-notes.md`** hier führen (wie `ledger-iteration-notes.md`): pro Iteration Beobachtung → Evidenz →
  Interpretation → Learning → nächster Schritt; volle Run-ID + exakte Datei; Commit-Hash nachtragen.
- Nach jedem Meilenstein den User zum **Anker-Commit** auffordern (Commits nur durch den User).
- Statusbezug: dieser Plan + `NeedToFinish.md` B-Bar synchron halten.

---

## 10. Ein-Satz-Zusammenfassung

**Kapitel B klinkt sich als neuer `agentPhase=phase2_evidence`-Modus additiv ein (3 Stellen), fährt den
bestehenden Agenten-DAG in zwei Armen (Transkript vs. Ledger-consumable) mit angepassten Prompts + einem
`sourceClaimIds`-Output, und misst per NLI-Treue-Checker, welcher Arm seine Quelle weniger verfälscht —
alles config-gesteuert, Runs unter `runs/phase2evidenz-agent/<arm>/`, ohne den alten Code umzubauen.**
