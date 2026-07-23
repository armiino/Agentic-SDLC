# Architecture Map — Evidenz-Agent / Kapitel C (MAF-Komposition)

> **Lebendes Dokument, editierbar.** Momentaufnahme der Bau-Architektur. Der **maßgebliche aktuelle Stand +
> Verlauf** steht in `docs/iteration-notes.md` (letzter Eintrag zählt); Planung/Scope in `docs/IST_Soll08.07.md`,
> Vision in `docs/issueOrchesterPlan.md`. Bei Widerspruch gelten die iteration-notes.
>
> **Stand bei Erstellung:** letzter iteration-note = **E-Chain** (volle MAF-Komposition Ledger→Fan-out→Derivation,
> E2E gelaufen, run d3682c). **Reifegrad ehrlich:** mechanisch stark + E2E-lauffähig; Qualität weitgehend
> UNGEMESSEN (n=1, Einzelläufe) — s. iteration-notes „Reifegrad/Mess-Hygiene".

---

## 1. MAF-Bausteine (was ist was)

```text
AGENT      ein AIAgent (LLM + Prompt/Persona). Generiert/urteilt semantisch. Läuft IN einem Executor.
EXECUTOR   Executor<TInput> = EIN Knoten im Graphen. Typisierter Eingang, sendet typisierte Messages.
           Kapselt: einen Agenten ODER einen bounded LLM-Call ODER deterministischen Code.
WORKFLOW   Graph aus Executoren + Kanten (WorkflowBuilder). Kann SELBST per BindAsExecutor zu einem Knoten werden.
KANTEN     AddEdge (normal) · AddEdge<T>(cond) (konditional) · AddFanOutEdge · AddFanInBarrierEdge (Join)
OUTPUT     Ein Workflow deklariert seine Ausgabe mit WithOutputFrom(<terminaler Executor>) — NÖTIG, damit ein
           gebundener Sub-Workflow seinen Output an die Eltern-Kante weiterreicht (sonst läuft er, aber Downstream
           wird nie getriggert; verifiziert bei E-c).
```

**Rollen-Regel (MAF-nativ, so umgesetzt):** generieren → Agent · deterministisch → Executor-Code · urteilen →
bounded Judge (kein autonomer Agent) · Tragweite/Unsicherheit → Human-in-the-Loop.

---

## 2. Unsere Workflows (6) + ihre Executoren

```text
1. CheckerRepairWorkflow        (Zyklus + konditionale Kanten)          CLI: checker-repair-workflow
     CheckerExecutor ─(Decision==Repair)─► RepairExecutor ─►(zurück) CheckerExecutor
                     └─(sonst)───────────► FinalizeExecutor
2. ArtifactBranchWorkflow       (linear)                                 CLI: artifact-branch
     ArtifactBranchMakerExecutor ─► [CheckerRepair] ─► ArtifactAssignIdsExecutor
3. BaselineFanOutWorkflow       (fan-out + barrier)                      CLI: baseline-fanout
     BaselineFanOutDispatchExecutor ─fanOut─► [ArtifactBranch]×N ─barrier─► BaselineCollectorExecutor
4. DerivationWorkflow           (linear, config-gesteuerte Familie)      CLI: derive <specId>
     DerivationGenerateExecutor ─► DerivationAnchorExecutor ─► DerivationCheckExecutor
5. EvidenceChainWorkflow        (volle Komposition)                      CLI: evidence-chain <specId>
     [BaselineFanOut] ─► SelectBaselineExecutor ─► [Derivation]
6. LedgerBuilderWorkflow        (Fundament, vorbestehend)                CLI: ledger-build
     CandidateExtraction ─► Canonicalization ─► CoverageRepair ─► FacetValidation [─► HumanAdjudication]
```
`[X]` = per **BindAsExecutor** gebundener Sub-Workflow (tritt als EIN Knoten auf).

---

## 3. Was steckt IN jedem Executor? (Agent / Judge / deterministisch)

```text
Executor                          Inhalt                          Typ
──────────────────────────────────────────────────────────────────────────────────
ArtifactBranchMakerExecutor       Evidence-Agent (Ledger→Artefakt) ← ECHTER AIAgent
DerivationGenerateExecutor        Derivation-Agent (leitet ab)     ← ECHTER AIAgent
CheckerExecutor                   ContractChecker (MC0)            ← deterministisch (kein LLM)
                                + ContractCritic (C7, k-Vote)      ← bounded LLM-Judge
RepairExecutor                    ContractRepair                   ← bounded LLM (chirurgisch, subtraktiv)
DerivationAnchorExecutor          Anker-Validierung (C1'/C2')      ← deterministisch (kein LLM)
DerivationCheckExecutor           InferenceChecker                 ← bounded LLM-Judge
ArtifactAssignIdsExecutor         ArtifactIdGate                   ← deterministisch (kein LLM)
SelectBaselineExecutor            Baseline von Disk wählen         ← deterministisch (kein LLM)
Dispatch / Collector / Finalize   Routing / Barrier / Abschluss    ← deterministisch (kein LLM)
```
Nur **2** echte Agenten (Maker, Deriver). Alles andere: kontrollierter Code oder enger Judge.

---

## 4. Das große Bild — die volle Kette (E-Chain), verschachtelt

```text
evidence-chain = EvidenceChainWorkflow                          ◄── EIN InProcessExecution.RunAsync
│
├─ [BaselineFanOut]                                    (gebundener Workflow)
│    ├─ BaselineFanOutDispatchExecutor        (det.)
│    ├─ [ArtifactBranch requirements]                  (gebundener Workflow)
│    │    ├─ ArtifactBranchMakerExecutor      (Executor → AIAgent)   ← generiert requirements
│    │    ├─ [CheckerRepair]                            (gebundener Workflow, ZYKLUS)
│    │    │    ├─ CheckerExecutor   (MC0 det. + C7 Judge)
│    │    │    ├─ RepairExecutor    (bounded LLM)
│    │    │    └─ FinalizeExecutor  (det.)
│    │    └─ ArtifactAssignIdsExecutor        (det. ID-Gate)
│    └─ BaselineCollectorExecutor             (Barrier-Ziel)         → VerifiedBaselineSet
│
├─ SelectBaselineExecutor                     (det.)                 → requirements ArtifactDocument
│
└─ [Derivation]                                        (gebundener Workflow)
     ├─ DerivationGenerateExecutor            (Executor → AIAgent)   ← leitet Risiken/Reqs ab
     ├─ DerivationAnchorExecutor              (det. Anker)
     └─ DerivationCheckExecutor               (Executor → InferenceChecker Judge)
```
Typen passen ohne Adapter an den Kanten: `VerifiedBaselineSet → ArtifactDocument → DerivationResult`.

---

## 5. CLI-Einstiegspunkte (jeder Baustein einzeln + komponiert)

```text
ledger-build              Transkript → consumable.json (Ledger)
checker-repair-workflow   Check→Repair→Recheck auf einem Artefakt (standalone)
assign-artifact-ids       Artefakt → stabile Item-IDs (artifact.json)
artifact-branch           EIN Zweig: Agent → [CheckerRepair] → IDs
baseline-fanout           Ledger → alle Artefakttypen parallel → Verified Baseline Set
derive <specId>           config-gesteuerte Ableitung (derived-risks | requirements-gap)
inference-check           semantischer Relevanz-/Widerspruchs-Check (standalone, alt)
derive-review             Human-Review der Ableitungen (generisches HumanReview-UI)
evidence-chain <specId>   VOLLE Kette: Ledger → Fan-out → Select → Derivation
```

Alle config-/prompt-/modell-steuerbar (run-config.json; jeder Run schreibt config.json-Snapshot).

---

## 6. Datenverträge (Kern)

```text
ConsumableLedger / SemanticLedgerEntry   die freigegebenen Claims (Ledger)
ArtifactItem { itemId, origin(Extracted|Derived), text, sourceClaimIds, sourceArtifactItemIds,
               assumptions?, derivationRationale? }
ArtifactDocument { artifactId, artifactType, version, stage(evidence_baseline|derivation), producer, items[] }
DerivationSpec { id, sourceType, targetType, agent, prompt, itemIdPrefix }   (config-gesteuerte Familie)
CheckerRepairResult · VerifiedBaselineSet · DerivationResult                 (Workflow-Outputs)
```
Provenienz-Kette (EIN Pfad): `derived item → sourceArtifactItemIds → baseline item → sourceClaimIds → Ledger-Evidenz`.

---

## 7. Was NICHT Teil dieser Kette ist (Abgrenzung)

```text
- Phase2Workflow / Phase2BWorkflow   = alte Phase-2.1-A/B/C-Arbeit (Transkript-DAG, Shared State) — separat.
- Human-Review (derive-review)        = KEIN Workflow-Knoten, sondern separater Schritt (Mensch als Halt).
- Integration-Layer (GitHub-Issues)   = Schicht 5, NOCH NICHT gebaut.
```

---

## 7b. Detaillierter Gesamtgraph (alles, mit Knotentyp + Message-Typen auf den Kanten)

```text
LEGENDE:  (A)=AIAgent   (J)=bounded LLM-Judge   (D)=deterministisch (kein LLM)   (H)=Mensch
          ┌═ WORKFLOW ═┐   [bound] = per BindAsExecutor eingebundener Sub-Workflow
          ═►TYP═►  = typisierte Message auf der Kante

input/transcripts/*.txt
      │  (ledger-build)
      ▼
┌═ LedgerBuilderWorkflow ══════════════════════════════════════════════════════════════┐
│  CandidateExtraction(J) ═► Canonicalization(J) ═► CoverageRepair(J) ═► FacetValidation(J)│
│                                                      [→ HumanAdjudication (H), optional] │
└══════════════════════════════════════════════════════════════════════════► consumable.json
      │   (= geprüfte, freigegebene Claims;  in der Kette als Text-Projektion übergeben)
      │
      ▼  evidence-chain <spec>
╔═ EvidenceChainWorkflow ═══════════════════════════════════════════════════════════════════════╗
║ (string: Ledger-Projektion)                                                                    ║
║   │                                                                                            ║
║   ▼                                                                                            ║
║ ┌═ [BaselineFanOut] ════════════════════════════════════════════════════════════════════════┐ ║
║ │ BaselineFanOutDispatch(D)                                                                   │ ║
║ │   │ ─AddFanOutEdge─► (string) an JEDEN Zweig (parallel, jeder liest die Quelle unabhängig)  │ ║
║ │   ├──────────────────────────────┬──────────────────────────────┬─────────────────────┐   │ ║
║ │   ▼                              ▼                              ▼                     ▼   │ ║
║ │ ┌═[ArtifactBranch requirements]┐ ┌═[…risks]═┐  ┌═[…architecture]═┐ ┌═[…open-questions]═┐ │ ║
║ │ │ Maker(A)                     │ │ (analog) │  │   (analog)      │ │    (analog)       │ │ ║
║ │ │   │ ═CheckArtifactMessage═►  │ └──────────┘  └─────────────────┘ └───────────────────┘ │ ║
║ │ │   ▼                          │       je Zweig identischer Aufbau ↓                      │ ║
║ │ │ ┌═[CheckerRepair] (ZYKLUS)═┐ │                                                          │ ║
║ │ │ │ Checker(D:MC0 + J:C7)    │ │   Checker prüft, entscheidet Decision:                   │ ║
║ │ │ │   │ ═CheckVerdictMessage═► │     Pass|HumanReview|MaxIter ─► Finalize                 │ ║
║ │ │ │   ├─(Decision==Repair)─► Repair(J) ═CheckArtifactMessage(iter+1)═► (zurück) Checker    │ ║
║ │ │ │   └─(sonst)───────────► Finalize(D) ═CheckerRepairResult═►                            │ ║
║ │ │ └──────────────────────────┘ │                                                          │ ║
║ │ │   │ ═CheckerRepairResult═►    │                                                          │ ║
║ │ │   ▼                          │                                                          │ ║
║ │ │ AssignIds(D)  ═ArtifactDocument═►                                                        │ ║
║ │ └──────────────────────────────┘                                                          │ ║
║ │   │ ═ArtifactDocument (1 pro Zweig)═►                                                       │ ║
║ │   ▼  AddFanInBarrierEdge (wartet, bis ALLE Zweige geliefert haben)                         │ ║
║ │ BaselineCollector(D) ═VerifiedBaselineSet═►   (schreibt je Zweig {typ}.artifact.json)      │ ║
║ └────────────────────────────────────────────────────────────────────────────────────────┘ ║
║   │ ═VerifiedBaselineSet═►                                                                    ║
║   ▼                                                                                           ║
║ SelectBaseline(D)  ═ArtifactDocument (der gewählte Quelltyp, z.B. requirements)═►             ║
║   │                                                                                           ║
║   ▼                                                                                           ║
║ ┌═ [Derivation <spec>] ═══════════════════════════════════════════════════════════════════┐ ║
║ │ Generate(A) ═GeneratedDerivation═► Anchor(D) ═AnchoredDerivation═► Check(J)               │ ║
║ │   (leitet ab)          (Anker gültig?)              (Relevanz/Widerspruch)                │ ║
║ └──────────────────────────────────────────────────────────────────► DerivationResult ────┘ ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════╝
      │  (Disk-Outputs im Run)
      ▼
   {typ}.artifact.json (Baselines) · {ziel}.derived.json (Ableitungen) · baseline-set.json
   · inference-check-report.json · derivation-report.json · final-report.json · step-check/-repair-NN
      │
      ▼  derive-review   (SEPARATER Schritt, blockierend)
┌═ AgenticSdlc.HumanReview (generisches UI) ═┐
│  je Item: I-c-Verdikt + Anker + Annahmen   │  Mensch(H): approve | reject | needs_revision
└────────────────────────────────────────────┘ ═► approved-derived-risks.json + review-decisions.json
```

**Wie man den Graphen liest:** die dicken Rahmen `╔═╗ / ┌═┐` sind **Workflows**; `[X]` heißt „dieser Workflow ist
per BindAsExecutor als EIN Knoten eingebunden" (deshalb die Verschachtelung). Die `═TYP═►` sind die typisierten
Messages, die MAF an den Kanten routet — sie MÜSSEN zusammenpassen (Ausgabetyp = Eingangstyp des nächsten Knotens).
Nur `(A)` sind echte Agenten; `(J)` sind enge LLM-Richter; `(D)` ist Code ohne LLM; `(H)` ist der Mensch.

**Inventar in Zahlen:** 6 Workflows · ~13 eigene Executoren · 2 echte Agenten (Maker, Deriver) · 3 bounded Judges
(C7, Repair, Inference) · Rest deterministisch · 1 generisches Human-Review-UI (separat).

---

## 8. Offene Bau-Punkte (Zeiger auf iteration-notes)

```text
- Integration-Layer: GitHub plan_only → create_issue  (issueOrchesterPlans-smaller-idea.md)
- Human-Review MAF-nativ in die Kette einbetten (statt separatem Schritt)
- Inline-Checker-Prompts (ContractCritic, InferenceChecker) externalisieren → mehr Tauschbarkeit
- MESS-HYGIENE (wichtig): echter Human-Review-Lauf, k-Wiederholungen, Inference-Check-FN-Rate, 2. Transkript
```

---

## 9. Wiederverwendung über Szenarien (Kickoff vs. Sprint) — Vorausblick

**Kernidee:** die Fach-Knoten sind wiederverwendbar; pro Szenario tauscht man die **Orchestrierung** (den Graphen),
nicht die Bausteine. Kickoff = „alles neu erzeugen"; Sprint-Transkript = „nur das Betroffene aktualisieren".

### Welcher Baustein wie wiederverwendbar ist
```text
Baustein               Kickoff                Sprint-Transkript                          Reuse-Grad
──────────────────────────────────────────────────────────────────────────────────────────────────
Ledger (ledger-build)  Transkript→Claims       Sprint-Transkript→Delta-Claims             ✅ 1:1 (neuer Input)
Evidence-Agent (Maker) CreateInitial           UPDATE (neue Claims einmischen)            ⚙️ Modus "Update" (noch offen)
CheckerRepair          prüft Artefakt          prüft aktualisiertes Artefakt              ✅ 1:1
ID-Gate                frische IDs REQ-01..N    bestehende IDs BEHALTEN, nur neue vergeben ⚙️ Modus "preserve" (offen)
Derivation (derive)    aus ALLEN Requirements   aus NUR den geänderten Requirements        ✅ Knoten 1:1, Input gefiltert
InferenceChecker       Relevanz-Check           dito                                        ✅ 1:1
Human-Review-UI        Ableitungen freigeben    + Reconciliation bestätigen (neuer Adapter) ✅ UI 1:1
evidence-chain (Graph) fan-out alles erzeugen   ANDERE Orchestrierung (s.u.)                ❌ Graph anders, Knoten gleich
```

### Neu für den Sprint (nur Klebstoff + 2 Modi — designt in issueOrchesterPlans-smaller-idea.md, NICHT gebaut)
```text
LoadSnapshot   bestehenden Stand (Baselines + IDs + Issue-Mappings) laden
Reconcile      „ist neuer Claim = bestehendes Requirement?" → Mensch bestätigt
ImpactRouter   feste Regel: welche Agenten/Ableitungen sind vom Delta betroffen? → nur die laufen
SaveSnapshot   neuen Stand speichern
+ Maker "Update"-Modus · ID-Gate "IDs erhalten"-Modus
```

### Gleiche Knoten, zwei Graphen
```text
KICKOFF (evidence-chain, gebaut):
   Ledger ─► [Fan-out: ALLE Baselines neu] ─► Select ─► [Derivation: aus allem]

SPRINT (IncrementalSprintWorkflow, noch NICHT gebaut):
   Sprint-Ledger ─► LoadSnapshot ─► Reconcile(Mensch) ─► ImpactRouter
                        ├─► [Evidence-Agent: UPDATE] ─► [CheckerRepair] ─► [ID-Gate: preserve]
                        └─► [Derivation: nur betroffene Items] ─► InferenceCheck ─► Review
                    ─► SaveSnapshot ─► (GitHub: comment/create)
```
→ dieselben `[…]`-Knoten, andere Reihenfolge, + Snapshot-Laden/-Speichern + Router.

### Warum das schon vorbereitet ist (Forward-Compat, IST_Soll §7.2)
```text
R1 Knoten adressierbar/bindbar   → Router kann Teilmenge auswählen
R2 Agenten config/modus-gesteuert → derselbe Agent im Kickoff- UND Sprint-Modus
R3 operation als Parameter        → CreateInitial jetzt, Update später ohne Agenten-Neubau
D1/D3 stabile persistierte IDs + Marker → Wiedererkennung über Läufe
```
**Fazit:** der Sprint braucht KEINE neuen Agenten — nur 2 Modi + 4 kleine Klebstoff-Executoren + eine andere
Orchestrierung. Das ist der ganze Unterschied Kickoff ↔ Sprint.

## 10. Rezept-gesteuerte Orchestrierung (Derivation-Familie generalisiert) — ENTWURF, nicht gebaut

**Kernidee / Problem:** wir wollen NICHT einen handgeschriebenen Workflow pro Use-Case („req→req", „req+risks→risks",
„nur der req-Agent"). Das dupliziert Graphen und skaliert nicht. Stattdessen: EIN generischer Zusammenbauer, der aus
einem **deklarativen Rezept** (Config) zur Laufzeit den MAF-Graphen montiert. Begründung MAF-nativ: `WorkflowBuilder`
ist gewöhnlicher C#-Code — Knoten/Kanten werden in einer Schleife hinzugefügt. Das tun wir HEUTE schon in
`BaselineFanOutRunner` (liest `--artifacts`, baut N Zweige dynamisch, dann `Build()`). Der Graph ist also bereits
Daten-getrieben; §10 verallgemeinert dieses Prinzip von der Fan-out-Achse auf die Derivation-Achse.

### Das Rezept (drei freie Achsen)
```text
Rezept:
  baseline:
    mode:      build | load            # build = frisch aus Ledger (Fan-out) | load = vorhandene Artefakte lesen
    artifacts: [requirements, risks, architecture, open-questions]   # Teilmenge 1..N (auch nur [requirements])
    fromRun:   runs/…/<runId>          # nur bei mode=load
  derivations:                          # 0..N Ableitungen (leer = "nur Baseline", z. B. nur der req-Agent)
    - sources: [requirements, architecture, risks]   # 1..N Quellen  → Multi-Source
      target:  user-stories                          # beliebiger Ziel-Typ
      prompt:  UserStoriesFromReqArch1
    - sources: [architecture]
      target:  architecture                          # "Replanner": arch → bessere arch (gleicher Mechanismus wie req→req)
      prompt:  ArchitectureReplan1
```

### Derselbe Graph-Bauplan, aus dem Rezept montiert
```text
[ Baseline-Quelle ]                     # mode=build → BaselineFanOut(N)   |   mode=load → LoadBaseline(fromRun)
        │  (VerifiedBaselineSet)
        ├─► SelectSubset(sources₁) ─► [ Derivation₁ ]   (BindAsExecutor, target₁/prompt₁)
        └─► SelectSubset(sources₂) ─► [ Derivation₂ ]   (BindAsExecutor, target₂/prompt₂)
```
`SelectSubset` filtert aus dem `VerifiedBaselineSet` genau die im Rezept genannten Quell-Artefakte. Mehrere Ableitungen
= mehrere gebundene Knoten. `derivations: []` ⇒ nur die Baseline läuft.

### Use-Cases → Rezept-Abbildung (Nachweis „eine Maschine, viele Fälle")
```text
Wunsch                                   baseline.artifacts        derivations
──────────────────────────────────────────────────────────────────────────────────────────────
nur der req-Agent (Meeting = nur Reqs)   [requirements]            []
req → neues req (Gap/Verfeinerung)       [requirements]            [{[requirements] → requirements}]
req + risks → neue risks                 [requirements, risks]     [{[requirements,risks] → risks}]
req + risks + arch → user-stories        [req, risks, arch]        [{[...] → user-stories}]
arch → bessere arch (Replanner)          [architecture]            [{[architecture] → architecture}]
Ableitung, Artefakte schon da (später)   mode:load, fromRun=<run>  [{… gewünschte Ableitung}]
```
„Am Anfang festlegen" vs. „später entscheiden" ist KEIN Feature-Unterschied: derselbe Assembler, einmal `mode:build`
(voller Durchstich), einmal `mode:load` (Ableitung auf bereits vorhandene Artefakte, z. B. nach dem nächsten Meeting mit
anderer `derivations`-Liste). Nicht doppelt zu bauen.

### Was das an EINMALIGEM Bau kostet (danach ist alles Weitere Config/Prompt)
```text
1. DerivationSpec: SourceArtifactType → SourceArtifactTypes (Liste)   [Multi-Source]
   + DerivationGenerateExecutor nimmt ein SET von Dokumenten statt einem.
   Anker-/Inference-Check bleiben — prüfen dann gegen die IDs ALLER Quellen (Provenienz-Kette wird breiter, nicht anders).
2. SelectSubset-Executor  — filtert VerifiedBaselineSet auf die Rezept-Quellen.
3. LoadBaseline-Executor  — mode:load: liest vorhandene {type}.artifact.json → emittiert VerifiedBaselineSet
   (Rest des Graphen bleibt identisch). Single-Source-Vorläufer existiert: CLI `derive <specId> <source.artifact.json>`.
+ dünner Rezept-Assembler (die foreach-Verdrahtung aus dem Fan-out, um die Derivation-Achse erweitert).
```

### Forschungs-Warnschild (Scope-Disziplin, WICHTIG)
Die Engineering-Fähigkeit „beliebige Quellen → beliebiges Ziel, dynamisch" ist billig und sauber. Die **Forschungs-
Aussage ist es nicht**: jede neue Quell-Kombination ist eine EIGENE zu validierende Behauptung. „Risks aus 4 Quellen" ist
nachweislich NICHT „Risks aus 1 Quelle" — mehr Quellen = mehr open-world-Inferenz, schwächere Verankerung, schwerer
messbare Treue (NLI-Checker strenger gefordert). Regel: **Mechanismus generisch (1..N), evaluierte Menge klein** — die
Maschine kann alles, gemessen und als Aussage geschrieben werden nur 1–2 gezielte Kombinationen. Sonst mächtige Maschine
+ n=1 pro Pfad → keine tragfähige Aussage.

### Manager-Agent (Forward-Compat)
Das Rezept IST exakt die Struktur, die ein späterer Manager-Agent ausgeben würde. Heute schreibt der Mensch die Config,
morgen emittiert ein Manager-Agent dieselbe Struktur — die Grund-Config bleibt in beiden Fällen die Wahrheit; der Manager
ersetzt nur die Hand, die sie schreibt, nicht den Mechanismus. Passt auf IST_Soll §7.2 R1 (Knoten adressierbar/bindbar)
+ R2 (config/modus-gesteuert) + R3 (operation als Parameter).

### Abgrenzung (was §10 NICHT deckt)
```text
✅ gedeckt:  Baseline(Teilmenge extrahieren) → Ableitung(aus Teilmenge), 1..N Quellen, beliebiges Ziel, build|load
❌ offen:    mehrstufige Ketten (Ableitung AUF einer Ableitung in EINEM Lauf)
❌ offen:    Cross-Run-Merge / Sprint-Reconcile (→ §9, eigener Klebstoff)
```
