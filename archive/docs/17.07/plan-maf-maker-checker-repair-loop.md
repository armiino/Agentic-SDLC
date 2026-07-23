# Plan — MAF Maker/Checker/Repair Loop (bounded, conditional, gate-driven)

Status: **PLAN / DESIGN.** Noch nicht gebaut. Ziel: die Core-/Tor-Agenten von „Maker → Gate → Finalize" auf ein
MAF-natives, begrenztes **Maker → Checker/Gate → Repair → Checker/Gate → Finalize**-Pattern heben. DoD ist nicht
„Agent hat etwas gespeichert", sondern **Gate pass** oder ein sauber dokumentierter, nicht reparierbarer Abbruch.

---

## 1. Warum

Die Core-/Tor-Schicht ist inzwischen MAF-nativ als Executor-Workflow modelliert:

```text
GithubForward:   Seed → Maker → Gate → Finalize
PBI Update:      Derive → Maker → Gate → Finalize
Decision:        Maker/Derive → Gate → Finalize
GithubReverse:   Seed → Gate → Finalize
```

Das ist bereits deutlich besser als ein Inline-Agent-Call. Was noch fehlt, ist der naechste MAF-Schritt: **Gate-
Feedback soll den Agenten kontrolliert reparieren lassen**, statt nur einen Gate-Report zu schreiben und den Lauf
als failed/needs review zu beenden.

Ein Gate ist fachlich ein **conditional control point**:

```text
Gate pass
  → Finalize

Gate fail, reparierbar
  → Repair-Agent / Maker-Retry mit GateFeedback

Gate fail, nicht reparierbar oder maxAttempts erreicht
  → Finalize failed + HumanReview/manual
```

Damit wird die Agentik staerker: Der Agent bekommt echtes Feedback, darf aber nur begrenzt und regelgebunden
iterieren. Keine offene Dauerschleife.

## 2. Bestehende Patterns im Projekt

### L4 re-clarify: Maker + Checker + fachlicher Review-Agent

Bereits vorhanden:

```text
ClusterAgent
→ ClusterGate
→ ClusterReviewAgent
→ Finalize
```

Der `ClusterAgent` ist Maker: Er gruppiert Requirements semantisch. `ClusterGate` ist deterministischer Checker
(Coverage). `ClusterReviewAgent` ist ein zweiter fachlicher Checker/Reflector: Er kritisiert Cluster, erkennt falsche
Merges/Splits/Crosscutting-Zuordnung und erzeugt Korrektur-Operationen.

Wichtig: Das ist schon ein Maker/Checker/Reflection-Pattern, aber noch kein automatischer Repair-Loop. Die Review-
Operationen laufen ueber Mensch/Apply, nicht automatisch zurueck in den Maker.

### L4 Completion: Evaluator + Maker + Gate

Bereits vorhanden:

```text
AdequacyFeedbackAgent
→ CompletionAgent
→ CompletionGate
→ Finalize
```

`AdequacyFeedbackAgent` bewertet den Stand als fachlicher Checker/Evaluator. `CompletionAgent` erzeugt daraufhin
Vorschlaege. `CompletionGate` prueft deterministisch. Auch hier existiert Reflexion, aber kein Gate-gesteuerter
Repair-Loop.

### Tool-Self-Check in mehreren Agenten

Viele Agenten haben bereits `check_*` Tools:

```text
check_consolidation_plan
check_clusters
check_completion_proposals
check_state_change_plan
check_forward_plan
```

Das ist **Reflection-light**: Der Agent kann seinen Plan im eigenen Lauf pruefen und korrigieren, bevor er speichert.
Der neue Plan macht daraus ein explizites MAF-Workflow-Pattern mit eigenem GateFeedback und begrenztem Retry.

## 3. Zielpattern

```text
Input / Seed / Derive
  → MakerAgent
  → GateExecutor
      pass
        → FinalizeExecutor
      fail + repairable + attempt < maxAttempts
        → RepairExecutor / MakerRetryExecutor
        → GateExecutor
      fail + hard OR attempt >= maxAttempts
        → FinalizeExecutor(failed, gateReport, attemptHistory)
```

Empfohlener Default:

```text
maxRepairAttempts = 2
```

DoD:

```text
Gate pass
```

oder, falls nicht erreichbar:

```text
finaler Gate-Report + attemptHistory + klarer Grund, warum nicht repariert wurde
```

## 4. Repairability-Klassifizierung

Nicht jeder Gate-Fehler darf automatisch repariert werden. Jedes Gate soll Fehler in Klassen ausgeben:

```text
repairable
hard
needs_human
```

### Reparierbar

```text
PBI_NOT_ADDRESSED
CREATE_WITHOUT_SEARCH_EVIDENCE
MISSING_EVIDENCE
DUPLICATE_OP
UNKNOWN_TARGET, wenn gueltige Ziele per Tool auffindbar sind
LINK_TARGET_NOT_IN_SNAPSHOT, wenn search/get erneut laufen kann
```

### Nicht reparierbar / hard

```text
Core fehlt
Snapshot fehlt
Decision existiert nicht
Input-Datei fehlt
Schema ungueltig
```

### Needs human

```text
fachlich widerspruechliche Antwort
mehrere gleich plausible Targets
ADOPT_NEW ohne fachlich belastbare neue Aussage
GitHub Drift mit unklarer Autoritaet
```

## 5. Datenmodell fuer Attempts

Jeder repair-faehige Workflow soll eine Attempt-Historie schreiben:

```text
attemptHistory[]
  attemptNumber
  source: maker | repair
  planPath
  gateReportPath
  gatePass
  errors[]
  repairedFromAttempt?
  repairPromptSummary?
  timestampUtc
```

Das ist wichtig fuer Traceability und Thesis-Auswertung: Man sieht, ob der Agent GateFeedback genutzt hat und wann
er stabil wurde.

## 6. Erster Einbaukandidat: GithubForwardAgent

Der beste erste Kandidat ist `GithubForward`, weil die Repair-Regeln konkret und gut testbar sind.

Aktuell:

```text
GithubForwardSeed
→ GithubForwardMaker
→ GithubForwardGate
→ GithubForwardFinalize
```

Ziel:

```text
GithubForwardSeed
→ GithubForwardMaker
→ GithubForwardGate
   ├─ pass → GithubForwardFinalize
   └─ fail repairable → GithubForwardRepair(max 2)
                         → GithubForwardGate
                         → Finalize
```

Typische Repairs:

```text
CREATE_WITHOUT_SEARCH_EVIDENCE
  → Repair muss search_issues ausfuehren und searchEvidence nachtragen
  → oder CREATE in LINK/HOLD/FLAG umwandeln

PBI_NOT_ADDRESSED
  → Repair muss fuer fehlendes Delta-PBI eine Operation ergaenzen

LINK_TARGET_NOT_IN_SNAPSHOT
  → Repair muss gueltiges Issue waehlen oder CREATE/HOLD/FLAG vorschlagen

MISSING_EVIDENCE
  → Repair muss Anchors / searchedQueries / rationale ergaenzen
```

Wichtig: Auch im Repair gibt es **keinen Write**. Repair erzeugt nur einen neuen `GithubForwardDraft`.

## 7. Danach uebertragbar auf weitere Agenten

Nach GithubForward kann dasselbe Pattern auf andere agentische Knoten uebertragen werden:

```text
PbiPlacementAgent:
  Gate fail PBI_NOT_PLACED / UNKNOWN_PBI / INVALID_FEATURE
  → Repair ergaenzt EXTEND_PBI oder NEW_PBI

DecisionResolverAgent:
  Gate fail fehlende neue Aussage bei ADOPT_NEW/REFINE
  → Repair formuliert neue Aussage oder waehlt KEEP_ORIGINAL

RequirementResolverAgent:
  Gate fail unplaced incoming / duplicate op / missing evidence
  → Repair ergaenzt Operation oder Evidence
```

Nicht-agentische Flows wie `GithubReverse` brauchen keinen Repair-Agenten, solange der Fehler deterministisch/human
ist. Dort reicht Gate → Finalize failed.

## 8. MAF-Nativitaet

Der Repair-Loop soll als echter Workflow-Graph modelliert werden, nicht als ad-hoc Schleife im Runner.

Ziel in MAF-Begriffen:

```text
MakerExecutor      [Sends Draft]
GateExecutor       [Sends GateVerdict]
RepairExecutor     [Sends Draft]
FinalizeExecutor   [Yields Result]
```

Der Graph ist conditional:

```text
GateVerdict.Pass == true
  → Finalize

GateVerdict.Pass == false && GateVerdict.HasRepairableErrors && Attempt < MaxAttempts
  → Repair

GateVerdict.Pass == false && !repairable
  → Finalize failed
```

Falls die konkrete Workflow-API Conditional Edges nicht komfortabel genug ausdrueckt, darf ein kleiner
`RepairLoopExecutor` als Orchestrator-Knoten genutzt werden. Wichtig ist dann: Attempts/GateReports bleiben explizit
als Artefakte sichtbar, und Maker/Gate/Repair bleiben als getrennte Rollen modelliert.

## 9. Bauschritte

```text
R1  [DONE] Modelle: Repairability {repairable|hard|needs_human}, GateDecision, GithubForwardAttempt, maxAttempts=2.
R2  [DONE] GithubForwardGate: repairability je Fehlercode + Decide() (Form wie CheckerExecutor.Decide).
R3  [DONE] GithubForwardRepairExecutor: Maker-Retry mit GateFeedback (kein Write), Attempt+1, Loop-Back.
R4  [DONE] GithubForwardWorkflow conditional: Gate --Decision==Repair--> Repair --> Gate (Loop) / sonst Finalize.
           Belegt: Pattern des CheckerRepairWorkflow wiederverwendet (conditional edges + Loop-Back + bounded).
R5  [DONE] Loop-Mechanik deterministisch belegt (dry-run: Bound-Pfad + Pass-Pfad). „Repair erholt sich real zu pass"
           per echtem LLM-Lauf (gpt-5.4) belegt: att1 maker PBI_NOT_ADDRESSED → att2 repair Gate PASS (Maker-Task
           temporaer abgeschwaecht zum Erzwingen, danach revertet; kein GitHub-Write).
R6  [DONE, angepasst] Hard-Failures (Core/Snapshot/Delta fehlt) brechen VOR dem Workflow mit klaren Guards ab;
           needs_human/hard im Gate -> Decision=HumanReview -> Finalize ohne Repair. Bound-Pfad = R5-Beleg.
R7  [DONE] Loop-Primitive geteilt in core/GateLoop.cs; Pattern auf pbi-update (PbiUpdateRepairExecutor),
           Decision (DecisionRepairExecutor, nur agentischer Graph) und Requirement-Resolver/Ingestion
           (IngestionRepairExecutor) uebertragen. --max-attempts (Default 2), *-attempts.json. pbi-update/decision
           deterministisch belegt; Ingestion Build()-bar. GithubReverse bleibt ohne Repair (deterministisch).
```

## 10. Erwartete Wirkung

- Agenten bekommen echtes GateFeedback und koennen begrenzt selbst korrigieren.
- DoD wird klarer: **Gate pass**, nicht nur „Plan gespeichert".
- Menschliche Reviews starten mit hoeherer Planqualitaet.
- Runs bleiben reproduzierbar, weil jeder Attempt + GateReport gespeichert wird.
- Die Architektur wird MAF-nativer: conditional loops, austauschbare Executor-Knoten, begrenzte agentische Iteration.

## 11. Grenzen

- Kein unbounded Auto-Loop.
- Kein autonomer Write im Maker oder Repair.
- Kein Repair fuer harte Infrastruktur-/Inputfehler.
- HumanReview bleibt Pflicht vor State-Mutation und externen GitHub-Writes.
