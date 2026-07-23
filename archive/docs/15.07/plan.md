# Plan 15.07 — L3 qualifizieren, Project State aufbauen, L4 konsolidieren, GitHub anschliessen

Ziel dieses Plans: Aus dem aktuellen L1/L2/L3-Stand wird ein sauberer, rueckverfolgbarer und spaeter GitHub-faehiger
Requirements-Prozess. Die Reihenfolge ist bewusst fachlich: erst Projektwahrheit qualifizieren und konsolidieren, dann
operationalisieren.

Kernkette:

```text
L1 Evidence Baseline
  -> L2 anchored/structured project artifacts
  -> L3 gap analysis + human review
  -> ProjectStateRepository
  -> L4 canonical requirements baseline
  -> Issue Planning plan-only
  -> SQLite persistence
  -> GitHub read/write integration
```

Leitregeln:

- `agentic-coverage` wird der bevorzugte produktive L3-Hauptpfad.
- `coverage-measure` und `coverage-repair` bleiben reproduzierbare Baselines/Forschungsarme.
- Keine fruehe Scheingenerik: Config-Felder nur dann aktivieren, wenn sie im aktuellen technischen Stand wirklich wirken.
- Agenten bekommen fachliche Tools, keine freien Dateisystem-Primitiven.
- Projektwahrheit entsteht kontrolliert: Proposal -> Gate/Check -> Human Review -> deterministic Apply.
- Markdown ist eine Projektion. Die maschinenlesbare Baseline ist autoritativ.

---

## Meilenstein 1 — L3 qualifizieren und akzeptierten L3-Stand erzeugen

**Ziel.** Belegen, dass `agentic-coverage` stabil und nuetzlich die wesentlichen Requirements-Luecken findet, und die
aktuellen Open-World-Kandidaten menschlich entscheiden.

**Status 2026-07-15.**

- 1.1 erledigt: strenger A/B-Vergleich auf `requirements + architecture` aus
  `runs/recipe/20260709_125328_2c13b5` dokumentiert.
- 1.2 erledigt: drei `--agentic-coverage`-Laeufe auf derselben Umwelt dokumentiert.
- 1.5 erledigt: Promptregel fuer `requiresHumanDecision` geschaerft, `check_lens_adequacy` mit kleinem Rubric erweitert,
  Kontrollrun `20260715_142625_fdc325` dokumentiert.
- 1.3 technisch vorbereitet: bestehende `AgenticSdlc.HumanReview`-UI fuer L3 gehaertet; Re-Launch aus
  `human-decisions.json`, CLI-Overrides und bessere L3-Kontexte sind umgesetzt.
- 1.3/1.4 fuer `20260715_142625_fdc325` fachlich abgeschlossen: 3 Human-Review-Entscheidungen akzeptiert und per
  `l3-apply` zu `L3-REQ-001..003` promoviert.
- Optionale Kontrollschicht umgesetzt: `l3-review --scope all` zeigt alle Kandidaten, SupportedAnchored sind
  vorakzeptiert/resolved, aber editierbar.
- Ergebnis: `agentic-coverage` bleibt Hauptpfad. Coverage und Kernluecken sind stabil; nach der Schaerfung ist das
  Human-Review-Paket kleiner und sauberer.
- Naechster umsetzbarer Schritt: bei Bedarf All-Scope-Review auf dem echten Run ausfuehren; danach mit Meilenstein 2
  ProjectStateRepository + L4 fortfahren.

### 1.1 Gleicher Input fuer Variantenvergleich

Der bisherige Vergleich ist qualitativ stark, aber nicht streng A/B, weil unterschiedliche Umwelten verwendet wurden.

Umsetzen:

- Eine feste Vergleichsumwelt definieren, z. B. `requirements + architecture + risks` oder bewusst `requirements +
  architecture`.
- Alle Varianten mit derselben Umwelt und denselben Modellen fahren:
  - `--coverage-measure`
  - `--coverage-repair`
  - `--agentic-coverage`
- Run-IDs, Kommandos und Resultate in `iteration-notes-l3-v2.md` dokumentieren.

Messen:

- Kandidatenzahl
- Coverage
- SupportedAnchored / Unreferenced / Contradicted / WeakOrUncertain
- `requiresHumanDecision`
- Tool-Nutzung
- wiederkehrende Kernluecken
- Human-Review-Nutzen

DoD:

- [x] Vergleichstabelle dokumentiert.
- [x] Entscheidung dokumentiert, ob `agentic-coverage` Hauptpfad bleibt.

### 1.2 Agentic-L3 Wiederholungen

Mindestens 3 `--agentic-coverage`-Runs auf exakt derselben Umwelt.

Auswerten:

- Welche Luecken tauchen stabil wieder auf?
- Welche Luecken sind Modellvarianz?
- Bleibt Tool-Nutzung substanziell?
- Bleibt Coverage vollstaendig?
- Bleibt das Human-Review-Paket fachlich wertvoll?

DoD:

- [x] Stabilitaetsnotiz in `iteration-notes-l3-v2.md`.
- [x] Liste stabiler Kernluecken.
- [x] Liste varianzbehafteter Kandidaten.

### 1.3 Human Review der aktuellen L3-Unreferenced

Ausgangsrun fuer ersten UI-Durchstich: `runs/l3/20260715_142625_fdc325`

Wiederverwendungsregel:

- Keine neue L3-Spezial-UI bauen.
- `AgenticSdlc.HumanReview` als domaenenagnostischen Core wiederverwenden.
- L3-spezifischer Adapter:
  - `human-review-package.json` -> `ReviewSession`
  - Autosave in eine L3-Decision-Queue/`l3-human-decisions.json`
  - lazy Context fuer Kandidat, Routinggruende, Anchors und ggf. source items
  - deterministic Apply bleibt ausserhalb der UI

Aktueller Kontrollrun `20260715_142625_fdc325`:

- `CAND-004` Freigabe-/Aenderungsprozess
- `CAND-005` Datenschutz-/Datenlebenszyklus
- `CAND-009` Qualitaetsziele/Mengengeruest

Historischer Referenzrun: `runs/l3/20260714_184553_1fdc14`

Zu pruefen:

- `CAND-004` Freigabe-/Aenderungsprozess
- `CAND-007` Audit/Protokollierung
- `CAND-010` Qualitaetsziele/Mengengeruest
- `CAND-013` MVP-Entscheidung Angehoerige/Bewohner

Entscheidungen:

- accept
- edit
- reject
- revise

DoD:

- [x] `human-decisions.json` liegt vor.
- [x] Jede Entscheidung ist begruendet.
- [x] Echte Open-World-Uebernahmen sind menschlich autorisiert.
- [x] L3 Human Review nutzt `AgenticSdlc.HumanReview` ueber einen Adapter, nicht eine eigene UI.
- [x] Re-Launch aus vorhandener `human-decisions.json` funktioniert.
- [x] UI-Entscheidungsdatei ist kompatibel mit `l3-apply`.
- [x] Optionaler Kontrollmodus `--scope all` zeigt alle Kandidaten editierbar.

### 1.4 L3 Apply ausfuehren

Umsetzen:

- `l3-apply` auf den entschiedenen Run ausfuehren.
- Promoted Items pruefen.
- Herkunft korrekt setzen:
  - `HUMAN_ACCEPTED_OPEN_WORLD`
  - `HUMAN_ACCEPTED_ANCHORED`

DoD:

- [x] `decision-records.json`
- [x] `promoted-items.json`
- [x] `rejected-candidates.json`
- [x] `promotion-mappings.json`
- [x] `apply-report.json`
- [x] Provenienzkette Kandidat -> Decision -> promoted Item intakt.

### 1.5 L3-Prompt/Tool klein schaerfen

Nicht gross umbauen. Nur beobachtete Schwachstellen korrigieren.

Umsetzen:

- Promptregel: echte Open-World-Uebernahmen, Scope-, Rollen-, Datenschutz-, Betriebs- und Prioritaetsentscheidungen
  grundsaetzlich `requiresHumanDecision=true`.
- `check_lens_adequacy` klein verbessern:
  - `substantive`
  - `thin`
  - `missing`
  - `uncertain`
  - `not_applicable`
- Kein grosses neues Judge-System bauen.

DoD:

- [x] Neuer Run zeigt sauberere Human-Decision-Markierung.
- [x] Adequacy bleibt Feedback fuer den Agenten, kein Ersatz fuer Human Review.

### 1.6 Minimal wirksame L3-Config

Nur Config-Achsen umsetzen, die jetzt wirklich wirken.

Jetzt umsetzen:

- `coverageSpec` per Config, default `early-phase`.
- `analysisType`, default `requirements-gap`.
- klar typisierter `L3Result` weiter stabilisieren.

Noch nicht voll aktivieren:

- `inputArtifactTypes`
- `inputStatuses`
- `outputCandidateType`

Diese Felder koennen im Vertrag vorbereitet werden, wirken aber erst richtig mit dem ProjectStateRepository.

DoD:

- [ ] `coverageSpec` kann gewaehlt werden.
- [ ] Unbekannte Spec-ID bricht frueh mit klarer Fehlermeldung ab.
- [ ] Keine Scheingenerik durch Filter, die bei CLI-Dateipfaden fachlich kaum wirken.

---

## Meilenstein 2 — JSON-first ProjectStateRepository und L4 canonical baseline

**Ziel.** Projektwissen nicht mehr aus verteilten Run-Pfaden rekonstruieren, sondern ueber stabile Items, Statuswerte,
Relationen und Provenienz als fachlichen Projektzustand laden. Darauf erzeugt L4 eine kanonische Requirements-Baseline.

**Status 2026-07-15.**

- 2.1 erster Durchstich erledigt: `IProjectStateRepository` mit fachlichen Operationen angelegt.
- 2.2 erster JSON-first Loader erledigt: `project-state-build` erzeugt aus Requirements + Architecture + akzeptiertem
  L3-Run einen `project-state.json` Snapshot.
- Aktueller Snapshot nach `l3-apply --scope all`: `runs/project-state/20260715_l3_v2/project-state.json`, 105 Items,
  140 Relationen, 105 Provenienz-Eintraege.
- Proposal-Schicht ergaenzt: 12 L3-Kandidaten als `l3_candidate` Proposals, alle `accepted`.
- 2.6 erster Durchstich erledigt: `l4-baseline` erzeugt aus dem ProjectState eine reproduzierbare
  `canonical-requirements-baseline.json` plus Markdown-Projektionen.
- 2.5 erster Durchstich erledigt: `ConsolidationPlan`-Vertrag, Identity-Seed und deterministisches Gate sind umgesetzt.
- 2.5 MAF-Agent-Durchstich erledigt: `l4-consolidation agent` baut einen Workflow
  `ConsolidationAgent[Tools] -> Gate[det] -> Finalize`.
- 2.5 echter Agentenlauf erledigt: Run `runs/l4/20260715_165408_8d7645/consolidation`, Gate `pass`, 72 Operationen
  mit 64 `KEEP`, 4 `DEPRECATE`, 4 `LINK`.
- L4 HumanReview vorbereitet: `l4-review` projiziert Planoperationen in die generische Review-UI; Scopes
  `needs-human`, `changes`, `all`.
- L4 HumanReview ausgefuehrt: 8 nicht-triviale Operationen akzeptiert.
- L4 Apply erledigt: `runs/l4/20260715_165408_8d7645/consolidation/applied`, 64 Requirements, 22 Open Decisions,
  182 TraceLinks.
- L4 Provenienzprojektion erledigt: `provenance-map.json` zeigt pro `CAN-REQ-*` L4-Operation, ProjectState-Items,
  L3-Kandidaten, Human-Decisions, Ledger-Claims, Replacements und Link-Cluster.
- Noch offen: Baseline qualitativ reviewen und ggf. Requirements-Dokument/Traceability final polieren.

### 2.1 ProjectStateRepository-Interface definieren

Schnittstelle als fachliche Operationen, nicht als Dateizugriffe.

Vorgeschlagene Operationen:

```text
list_items(types?, statuses?, origins?)
get_item(itemId)
search_items(query, types?, statuses?)
get_relations(itemId)
get_provenance(itemId)
save_proposal(proposal)
apply_decision(decision)
load_source_artifact_set(selection)
```

DoD:

- [x] Interface/Service im Host angelegt.
- [x] Keine SQLite-Abhaengigkeit.
- [x] Erste Repository-Operationen sind speicherneutral definiert.
- [ ] Agenten/Runner muessen keine konkreten Dateipfade kennen.

### 2.2 JSON-Implementierung bauen

Die erste Implementierung darf weiter JSON lesen.

Quellen:

- L1/L2 `*.artifact.json`
- L3 `l3-agentic-candidates.json`
- `routing-report.json`
- `human-review-package.json`
- `decision-records.json`
- `promoted-items.json`
- `promotion-mappings.json`

DoD:

- [x] Repository kann den aktuellen L1/L2/L3-Stand laden.
- [x] Akzeptierte/promotete L3-Items sind als Projektitems sichtbar.
- [x] Vorgeschlagene/nicht akzeptierte L3-Items bleiben als Vorschlaege oder Review-Artefakte getrennt.

### 2.3 L3-Tools optional ueber Repository-Adapter betreiben

Die aktuellen L3-Tools sind schon fachlich geschnitten. Toolnamen und Prompts sollen stabil bleiben.

Umsetzen:

- Zunaechst weiter `SourceArtifactSet` als Tool-Backing erlauben.
- Repository-Adapter vorbereiten, der dieselben Operationen liefert:
  - list
  - search
  - get item
  - relations
  - provenance

DoD:

- Kein Prompt-/Tool-Namensbruch.
- Spaetere Ablösung des `SourceArtifactSet`-Backings ist moeglich.

### 2.4 L4-Rolle definieren

L4 ist kein Markdown-Generator. L4 erzeugt einen kanonischen Requirements-Zustand.

Aufgaben:

- L1/L2/L3 zusammenfuehren.
- Dubletten erkennen.
- Ergaenzungen von eigenstaendigen Anforderungen unterscheiden.
- Widersprueche oder ersetzte Items behandeln.
- Status setzen.
- Open Decisions separat halten.
- MVP/spaeter/verworfen markieren.
- Traceability erhalten.

DoD:

- L4-Spezifikation dokumentiert.
- Input-/Output-Vertrag definiert.

### 2.5 L4 ConsolidationPlan einfuehren

Agentische Konsolidierung, aber deterministisches Apply.

Pattern:

```text
L4 Consolidation Agent
  -> ConsolidationPlan
  -> deterministic checks
  -> semantic gate
  -> Human Review
  -> deterministic Apply
  -> canonical-requirements-baseline.json
```

Operationen:

```text
KEEP
ADD
MERGE
SPLIT
REVISE
DEPRECATE
LINK
MARK_OPEN_DECISION
```

Deterministische Checks:

- IDs existieren.
- Operationen sind erlaubt.
- Statusuebergaenge sind erlaubt.
- Provenienz bleibt erhalten.
- Keine doppelten Ziel-IDs.
- Open Decisions werden nicht als akzeptierte Requirements getarnt.

DoD:

- [x] `ConsolidationPlan`-JSON-Vertrag.
- [x] Checks erzeugen verwertbares Feedback.
- [x] Agentischer Planer ist als MAF-Workflow-Knoten baubar.
- [x] Echter Agentenlauf erzeugt gate-faehigen Plan.
- [x] HumanReview-Adapter fuer Planoperationen nutzt generische Review-UI.
- [x] Apply ist deterministisch.
- [x] Vollstaendige Provenienzprojektion fuer L4-Baseline erzeugt.

### 2.6 Canonical Requirements Baseline erzeugen

Hauptoutput:

```text
canonical-requirements-baseline.json
```

Weitere Projektionen:

```text
requirements-document.md
requirements-analysis.md
traceability-matrix.json
open-decisions.json
```

Minimaler Item-Vertrag:

```json
{
  "id": "REQ-123",
  "stableItemId": "REQ-123",
  "version": 1,
  "title": "...",
  "statement": "...",
  "type": "functional|nonfunctional|constraint|open_decision",
  "status": "accepted|proposed|needs_decision|rejected|deprecated",
  "origin": "extracted|derived|human_accepted_open_world|human_accepted_anchored",
  "sourceClaimIds": [],
  "sourceArtifactItemIds": [],
  "sourceCandidateIds": [],
  "sourceDecisionIds": [],
  "rationale": "...",
  "acceptanceHints": [],
  "dependencies": [],
  "mvp": true,
  "operationalizable": true
}
```

DoD:

- [x] Eine erste Baseline aus aktuellem L1/L2/L3-Stand wird erzeugt.
- [x] Markdown ist aus JSON reproduzierbar renderbar.
- [x] Traceability bleibt bis L1/Ledger bzw. L3-Decision erhalten.
- [ ] Semantische Konsolidierung ueber `ConsolidationPlan` statt 1:1-Projektion.

---

## Meilenstein 3 — Issue Planning plan-only validieren

**Ziel.** Aus der kanonischen, akzeptierten Requirements-Baseline werden Issue-Plaene. Noch keine echten GitHub-Issues.

### 3.1 Issue Planning nur auf canonical baseline

Der Issue Planning Agent darf nicht rohe L1/L2/L3-Runs interpretieren.

Input:

- aktive Requirements
- akzeptierter Status
- operationalisierbar
- noch kein GitHub-Mapping oder Mapping braucht Update

DoD:

- Repository-Abfrage liefert nur geeignete Items.
- Issue Planning muss keine Konsolidierung mehr leisten.

### 3.2 IssuePlan-Vertrag bauen

Beispiel:

```json
{
  "planId": "IPLAN-001",
  "operation": "CREATE",
  "title": "Implement role based access matrix",
  "description": "...",
  "sourceItemIds": ["REQ-123", "ARCH-010"],
  "acceptanceCriteria": [],
  "labels": ["backend", "security"],
  "dependencies": [],
  "reason": "...",
  "requiresHumanReview": true
}
```

Operationen:

```text
CREATE
UPDATE
LINK
REOPEN
NO_CHANGE
NEEDS_REVIEW
```

DoD:

- `IssuePlan`-Schema versioniert.
- Plan-only Output-Verzeichnis.

### 3.3 Issue Planning Agent mit fachlichen Tools

Tools:

- `list_operationalizable_items`
- `get_project_item`
- `get_related_project_items`
- `get_unmapped_items`
- `get_github_mapping`
- `search_github_issues` spaeter read-only
- `save_issue_plan`
- `mark_not_operationalized`

DoD:

- Agent schreibt nur Issue Plans.
- Keine freien Dateipfade.
- Keine GitHub-Schreiboperationen.

### 3.4 Deterministische und semantische Validierung

Deterministisch:

- source IDs existieren.
- Items sind akzeptiert und operationalisierbar.
- Kein Mappingkonflikt.
- Genau eine Operation.
- Titel/Beschreibung vorhanden.
- Acceptance Criteria vorhanden oder Reviewbedarf begruendet.

Semantisch:

- Issue-Schnitt sinnvoll?
- Quellen korrekt?
- Keine Duplikate?
- Zu gross/zu klein?
- Acceptance Criteria durch Quellen gedeckt?

Pattern:

```text
Issue Planning Agent
  -> IssuePlan
  -> Gate Feedback
  -> bounded Reflect
  -> Human Review
  -> approved IssuePlan
```

DoD:

- Issue Plan kann failen und mit Feedback revidiert werden.
- Genehmigter Issue Plan ist reproduzierbar.

---

## Meilenstein 4 — SQLite und kontrollierte GitHub-Integration

**Ziel.** Persistenz und GitHub erst einfuehren, wenn L4-Baseline und Issue Planning fachlich stabil sind.

### 4.1 SQLite-Repository

Warum erst jetzt:

- Datenbank loest keine fachlichen Fragen wie Granularitaet, Merge-Regeln oder Issue-Schnitt.
- Nach L4/IssuePlan sind die benoetigten Relationen empirisch klarer.

Minimales Schema:

- project_items
- item_versions
- relations
- evidence_links
- human_decisions
- runs
- issue_plans
- github_mappings

Wichtig:

- stabile Entitaet von Version trennen.
- GitHub-Mapping auf stabile Item-Identitaet beziehen.
- IssuePlan als Ursprung des GitHub-Mappings speichern.

DoD:

- JSON-first Repository kann auf SQLite-Backend wechseln.
- Bestehende Agenten-Tools bleiben stabil.

### 4.2 GitHub read-only Reconciliation

GitHub ist operative Ebene, nicht fachliche Wahrheit.

Read-only Ziele:

- bestehende Issues suchen
- Titel/Beschreibung/Labels/Status lesen
- Duplikate erkennen
- Update/Reopen/No-Change planen

DoD:

- Issue Planning kann GitHub-Zustand lesen, aber nicht schreiben.
- `CREATE|UPDATE|LINK|NO_CHANGE|REOPEN|NEEDS_REVIEW` werden als Plan erzeugt.

### 4.3 GitHub write nach Freigabe

Erst nach genehmigtem IssuePlan.

Umsetzen:

- Issues erstellen/aktualisieren.
- Mapping speichern.
- Idempotenz sicherstellen.
- Wiederholter Apply erzeugt kein doppeltes Issue.

DoD:

- GitHub-Write ist deterministisch angewandt.
- Lokales Mapping stabil.
- Human approval fuer schreibende Operation liegt vor.

### 4.4 Reconciliation von geschlossenen Issues

Ein geschlossenes GitHub-Issue bedeutet nicht automatisch: Requirement erfuellt.

Trennung:

- GitHub autoritativ fuer Issue-Status, Assignees, Labels, Milestones, operative Kommentare.
- Project State autoritativ fuer Requirements, Provenienz, Versionen, Relationen, fachlichen Status.

DoD:

- Geschlossene Issues werden deterministisch synchronisiert.
- Semantische Abschlussinterpretation laeuft separat ueber Reconciliation Agent oder Mensch.

---

## Empfohlene Reihenfolge

1. L3 auf identischem Input vergleichen.
2. `agentic-coverage` mehrfach wiederholen.
3. Aktuelle L3-Unreferenced menschlich entscheiden.
4. `l3-apply` ausfuehren.
5. Kleine L3-Schaerfungen: `requiresHumanDecision`, Adequacy, `coverageSpec`.
6. `agentic-coverage` als Hauptpfad einfrieren, Baselines behalten.
7. JSON-first `ProjectStateRepository`.
8. L4 `ConsolidationPlan` + Gate + Apply.
9. `canonical-requirements-baseline.json` + Markdown/Traceability rendern.
10. Issue Planning plan-only.
11. SQLite-Backend.
12. GitHub read-only.
13. GitHub write nach Freigabe.

---

## Naechster konkreter Arbeitsschritt

Der naechste sinnvolle Schritt ist **Meilenstein 1.1 bis 1.4**:

1. Strengen A/B-Input festlegen.
2. `coverage-measure`, `coverage-repair`, `agentic-coverage` darauf fahren.
3. `agentic-coverage` mehrfach wiederholen.
4. Die aktuellen 4 Unreferenced aus `20260714_184553_1fdc14` human-reviewen und per `l3-apply` anwenden.

Erst danach sollten wir weitere Generik oder Repository-Arbeit beginnen.

---

## Fortschreibung 2026-07-15 L4

Erledigt:

- `ProjectStateRepository` JSON-first eingefuehrt.
- `l4-consolidation agent` als MAF-naher Knoten aufgebaut:
  `ProjectStateDocument -> Tool-Agent -> deterministisches Gate -> Finalize`.
- `l4-review` mit wiederverwendeter HumanReview-UI eingefuehrt.
- `l4-apply` eingefuehrt:
  - Human Decisions deterministisch anwenden
  - effective plan erneut gaten
  - `canonical-requirements-baseline.json`
  - `requirements-baseline.md`
  - `traceability-matrix.md`
  - `open-decisions.json`
  - `provenance-map.json`
- `l4-quality` eingefuehrt:
  - deterministischer Qualitaetsreport ueber Baseline und Provenienz
  - harte Fehler fuer strukturelle Traceability-/Provenienzbrueche
  - Warnungen fuer Entscheidungsmarker, Breakdown-Bedarf und Duplicate-Risiken
- `requirements-readiness` eingefuehrt:
  - deterministischer Post-L4-Knoten nach `L4Quality`
  - erzeugt `requirements-readiness.json`
  - erzeugt `issue-planning-input.json`
  - filtert offene Entscheidungen, Breakdown-Bedarf, optionale/deferred Items und Traceability-Blocker aus dem
    direkten Issue-Planning-Input

Aktueller Qualitaetsstand des L4-Durchstichs:

```text
Run: 20260715_165408_8d7645
l4-quality: pass
Errors: 0
Warnings: 10
Requirements: 64
Ready: 32
Needs breakdown: 7
Needs decision: 25
Duplicate risk: 0
```

Aktueller Readiness-Stand:

```text
Run: 20260715_165408_8d7645
Requirements: 64
Ready for issue planning: 32
Needs decision: 23
Needs breakdown: 7
Deferred or optional: 2
Blocked by traceability: 0
Issue planning input: 32
```

Bewertung:

- Die Kette L1/L2/L3/L4 erzeugt nun eine kanonische, rueckfuehrbare Requirements-Baseline.
- L4 verliert die Nachvollziehbarkeit nicht: L3-Kandidaten, Human Decisions, ProjectState-Items, Ledger-Claims,
  Replacements und LINK-Cluster bleiben sichtbar.
- Die Baseline ist strukturell stabil genug fuer den naechsten Schritt.
- Vor einem GitHub-Issue-Planning-Agenten muss aber die fachliche Operationalisierbarkeit sauber behandelt werden:
  22 Requirements sind offene Entscheidungen, 3 aktive Requirements enthalten noch Entscheidungs-/Optionalitaetsmarker
  und 7 aktive Requirements brauchen wahrscheinlich Breakdown oder Akzeptanzkriterien.
- `requirements-readiness` setzt diese Behandlung bereits deterministisch um und liefert einen sauberen, eingeschraenkten
  Input fuer den naechsten Agenten.

Naechster sinnvoller Schritt:

1. `IssuePlan`-Schema versionieren.
2. `l4-issuplanning plan-only` als MAF-naher Agentenknoten bauen.
3. Agent arbeitet nur auf `issue-planning-input.json` bzw. spaeter Repository-Abfrage `list_issue_planning_ready_items`.
4. Deterministisches IssuePlan-Gate einfuehren.
5. Danach HumanReview fuer IssuePlans wieder ueber `AgenticSdlc.HumanReview` anbinden.

## Fortschreibung 2026-07-15 Issue Planning

Umgesetzt:

- `IssuePlan`-Schema versioniert:
  - `IssuePlanDocument`
  - `IssuePlanItem`
  - `IssuePlanGateReport`
- plan-only Operationen fuer Stufe 1:
  - `CREATE`
  - `LINK`
  - `NO_CHANGE`
  - `NEEDS_REVIEW`
- `IssuePlanGate`:
  - prueft SchemaVersion
  - prueft Project/Baseline
  - prueft erlaubte Operationen
  - prueft SourceRequirementIds gegen `issue-planning-input.json`
  - prueft Pflichtfelder
  - warnt bei fehlenden Acceptance Criteria oder zu grossen IssuePlans
  - warnt bei nicht abgedeckten ready Requirements
- `L4IssuePlanningAgent` als MAF-naher Workflow-Knoten:

```text
IssuePlanningInput
  -> L4IssuePlanningAgent[Tools]
  -> IssuePlanGate[det]
  -> IssuePlanningFinalize
```

Tools:

```text
list_issue_planning_items
search_issue_planning_items
get_issue_planning_item
get_seed_issue_plan
check_issue_plan
save_issue_plan
```

Deterministische Testlaeufe:

```text
l4-issuplanning seed 20260715_165408_8d7645
  items=32
  gate=pass
  errors=0
  warnings=0

l4-issuplanning check 20260715_165408_8d7645 issue-plan-seed.json
  gate=pass
  errors=0
  warnings=0

l4-issuplanning agent 20260715_165408_8d7645 --dry-run
  Graph Build()-bar
```

Erster echter Agentenlauf:

```text
l4-issuplanning agent 20260715_165408_8d7645
  run: 20260716_094748_91fba7
  gate=pass
  errors=0
  warnings=1
  readyInputItems=32
  issuePlanItems=15
  coveredReadyItems=32
  uncoveredReadyItems=0
  operations:
    CREATE=9
    NEEDS_REVIEW=4
    NO_CHANGE=2
```

Bewertung:

- Meilenstein 3 hat jetzt den ersten plan-only Durchstich.
- Der Knoten hat einen ersten echten LLM-Lauf mit Gate `pass`.
- Der deterministische Seed ist bewusst konservativ: ein `CREATE` pro ready Requirement.
- Der Agent verdichtet 32 ready Requirements auf 15 IssuePlanItems und nutzt `NEEDS_REVIEW`/`NO_CHANGE`.
- Die einzige Gate-Warnung betrifft ein grosses Rollen-/Rechte-Issue und sollte im HumanReview geprueft werden.
- GitHub-Read und GitHub-Write sind bewusst noch nicht enthalten.

Naechster Schritt:

1. Agentenplan gegen Seed vergleichen:
   - bessere Gruppierung?
   - weniger/mehr Issues?
   - sinnvolle Acceptance Criteria?
   - keine Quellenverluste?
2. IssuePlan HumanReview ueber `AgenticSdlc.HumanReview` anbinden.
3. Runner haerten: Provider-/Workflowfehler muessen non-zero zurueckgeben, falls kein GateReport entsteht.
4. Danach erst GitHub read-only/Mappings vorbereiten.

## Architekturentscheidung: Readiness mit ProjectState/DB

`requirements-readiness` ist langfristig kein Dateiskript und kein GitHub-Hilfsfilter, sondern ein generischer
Operationalisierungs-Gate-Knoten zwischen fachlicher Projektwahrheit und operativer Planung.

Zielbild:

```text
ProjectState / DB
  -> Canonical Requirements View
  -> Quality / Readiness Gate
  -> Planning Input
  -> L4IssuePlanningAgent / GitHub integration
```

Verantwortung:

- Readiness beantwortet: Darf dieses fachliche Item jetzt operativ geplant werden?
- IssuePlanning beantwortet: Welche operative Arbeit muss fuer dieses ready Item geplant werden?
- GitHub/Reconciliation beantwortet: Gibt es dazu bereits Issues, Mappings oder Statusaenderungen?

Readiness darf spaeter nicht blind die ganze DB durchsuchen. Der Knoten bekommt immer einen expliziten Scope vom
aufrufenden Graphen/Workflow. Der Knoten bewertet also einen fachlich definierten Ausschnitt des Projektzustands.

Beispiel-Input fuer einen spaeteren Repository-/DB-basierten Knoten:

```json
{
  "projectId": "pflege-app",
  "scope": "affected_items",
  "itemIds": ["REQ-123", "REQ-124"],
  "baselineId": "BASE-2026-07-15",
  "includeMappings": true,
  "includeLatestQuality": true
}
```

Typische Modi:

```text
initial-analysis
  scope = current_canonical_baseline

sprint-ingest
  scope = affected_items_from_transcript

requirements-refresh
  scope = changed_or_stale_requirements

issue-planning-only
  scope = ready_candidates_without_issue_mapping

reconciliation
  scope = items_with_changed_github_state

full-audit
  scope = all_active_requirements
```

Repository-Operationen statt freier DB-Suche:

```text
list_canonical_requirements(scope)
get_requirement_version(itemId)
get_latest_quality_assessment(itemId)
get_provenance(itemId)
get_issue_mapping(itemId)
get_decisions(itemId)
save_readiness_assessment(assessment)
```

Fuer Sprint-Transkripte bedeutet das:

```text
SprintTranscriptIngest
  -> L1/L2 Delta Extraction
  -> ProjectState update
  -> affected item ids
  -> canonical view refresh fuer betroffene Items
  -> quality/readiness nur fuer betroffene Items
  -> issue planning fuer betroffene ready Items
```

Readiness wird in einer spaeteren DB als versionierter Zustand gespeichert, nicht nur als fluechtiger Report:

```text
readiness_assessments
- item_id
- item_version_id
- readiness
- reasons
- source_quality_report_id
- created_by_run_id
- superseded_by
```

Wichtig:

- Readiness haengt an einer Requirement-Version.
- Wenn ein Sprint-Meeting ein Requirement aendert, entsteht eine neue Requirement-Version und eine neue
  Readiness-Bewertung.
- Alte Assessments bleiben fuer Nachvollziehbarkeit erhalten.
- Der L4IssuePlanningAgent darf nur auf Items arbeiten, die durch Readiness freigegeben oder explizit als
  planungsbeduerftig markiert wurden.
- Der L4IssuePlanningAgent prueft danach getrennt, ob es bereits IssuePlans, GitHub-Mappings, offene Issues,
  geschlossene Issues oder no-change Faelle gibt.

## Spaeterer Knoten: Sprint Delta / Impact Analysis

Fuer spaetere Sprint-Transkripte reicht es nicht, nur Readiness, Issue Planning oder GitHub Reconciliation zu starten.
Neue Aussagen muessen zuerst gegen den bestehenden ProjectState gematcht werden.

Beispiel:

```text
Sprint-Transkript spricht Thema X an.
Thema X gehoert zu einem Bereich, der fachlich bereits umgesetzt und operativ ohne offene Issues ist.
```

In diesem Fall darf nicht automatisch ein neues Requirement oder neues GitHub-Issue entstehen.

Zielbild:

```text
SprintTranscript
  -> L1/L2 Delta Extraction
  -> SprintDeltaImpactAgent
  -> affected ProjectState items
  -> L4 refresh fuer betroffenen Scope
  -> Quality/Readiness refresh fuer betroffene Item-Versionen
  -> L4IssuePlanningAgent nur fuer betroffene ready Items
  -> GitHub Reconciliation
```

Verantwortung:

- `SprintDeltaImpactAgent` erkennt, ob eine neue Aussage ein bestehendes Item betrifft.
- L4 aktualisiert spaeter nur den betroffenen kanonischen Scope, nicht blind die ganze Historie.
- Readiness bewertet nur betroffene neue/geaenderte Requirement-Versionen.
- L4IssuePlanning plant nur Items, die nach Readiness operativ relevant sind.
- GitHub Reconciliation prueft erst danach, ob es offene, geschlossene oder fehlende Issues/Mappings gibt.

Moegliche Impact-Klassifikationen:

```text
NO_CHANGE
CLARIFICATION
CHANGE_REQUEST
REOPEN_NEEDED
OBSOLETE
NEW_REQUIREMENT
NEEDS_REVIEW
```

Wichtig:

- Der GitHub-Agent darf nicht allein entscheiden, ob Thema X fachlich neu ist.
- Der L4IssuePlanningAgent weiss nur, was sein Readiness-Input enthaelt.
- Der ProjectState-/Impact-Schritt ist die Stelle, an der neue Sprint-Aussagen mit bestehender fachlicher Wahrheit,
  Versionen, Relationen und GitHub-Mappings verbunden werden.
- Ein geschlossenes GitHub-Issue fuehrt nicht automatisch zu `done`; die semantische Einordnung erfolgt ueber
  ProjectState/Impact/Reconciliation.

## L4 Requirements Document Projection

Nach L4 Apply braucht es neben der maschinenlesbaren Baseline ein grosses, fachlich lesbares Requirements-Dokument.
Dieser Schritt ist kein eigener Wahrheitsproduzent, sondern eine deterministische Projektion.

Input:

```text
canonical-requirements-baseline.json
provenance-map.json
requirements-readiness.json
```

Output:

```text
requirements-document.md
```

Verantwortung:

- Requirements in fachlich sinnvolle Abschnitte gliedern.
- Funktionale, nicht-funktionale, UI/UX-, Rollen-, Scope-, Prozess- und offene Entscheidungsanteile sichtbar machen.
- Pro Requirement Status, Readiness und Traceability anzeigen.
- Offene Entscheidungen und Breakdown-Bedarf explizit im Dokument halten.
- Die kanonische JSON-Baseline nicht ersetzen.

Aktueller Command:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-requirements-doc 20260715_165408_8d7645
```

Aktueller Output:

```text
runs/l4/20260715_165408_8d7645/consolidation/applied/requirements-document.md
```

Zukunft:

- Ein Open-World-L4-/L5-Review-Agent kann dieses Dokument als kompakte Arbeitsansicht verwenden.
- Die eigentliche Bewertung "ausreichend gedeckt / nicht ausreichend gedeckt" muss gegen ProjectState, Baseline,
  Provenance, Readiness und offene Entscheidungen laufen.
- Damit bleibt das Dokument reviewfreundlich, waehrend die DB/JSON-Baseline autoritativ bleibt.

## Geplanter L4 Completion Loop: Adequacy Feedback und DISK-Punkte

Der aktuelle L4-Stand konsolidiert vorhandene Items und erzeugt daraus eine kanonische Baseline. Das reicht fuer
Traceability und erste Issue-Planung, ist aber noch nicht zwingend genug fuer die Frage:

```text
Ist dieses Requirements-Dokument als Projektstartpunkt fachlich vollstaendig genug?
```

Der naechste fachliche Ausbau soll deshalb kein weiterer Renderer sein, sondern ein L4-Feedbacksignal, das den
L4-Agenten zu kontrollierter Vervollstaendigung animiert.

Ziel:

- Kapitelweise erkennen, ob der konsolidierte Stand substanziell genug ist.
- Duenne, fehlende, unklare oder widerspruechliche Bereiche sichtbar machen.
- Den L4-Agenten dazu bringen, daraus Vorschlaege, offene Entscheidungen oder Diskussionspunkte zu erzeugen.
- Open-World-Ergaenzungen erlauben, aber klar als nicht belegte Requirements-Engineering-Vorschlaege markieren.
- Human Review bleibt die Stelle, an der solche Vorschlaege angenommen, angepasst oder abgelehnt werden.

Wichtige Trennung:

```text
belegt                = aus Transkript/Ledger/L1/L2/L3 nachvollziehbar
abgeleitet            = aus vorhandenen Quellen fachlich ableitbar
open_decision         = Entscheidung fehlt und blockiert Klarheit
diskussion_point/DISK = wurde nie gesagt, ist aber als RE-Vorschlag plausibel/noetig
```

`DISK` steht fuer Diskussionspunkt. Ein `DISK`-Item ist keine Projektwahrheit und darf nicht direkt in IssuePlanning
oder GitHub-Write laufen. Es ist ein sichtbar markierter Vorschlag des Requirements Engineers.

Beispiel:

```json
{
  "id": "DISK-PLAT-001",
  "category": "platform",
  "title": "Zielplattformen verbindlich festlegen",
  "problem": "Die bisherigen Anforderungen nennen Flutter/Dart und Tablet-Support, legen aber keine verbindlichen Zielplattformen, Mindestversionen oder Geraeteklassen fest.",
  "suggestedResolution": "Primaere Zielplattform Android-Tablet festlegen; iOS und Smartphone-Support explizit als optional oder out-of-scope entscheiden.",
  "whyItMatters": "Ohne diese Entscheidung sind UI-Konzept, Testmatrix, Deployment und Aufwand nicht stabil planbar.",
  "evidenceState": "not_stated",
  "sourceIds": ["CAN-REQ-062", "CAN-REQ-063"],
  "requiresHumanDecision": true
}
```

Vorgeschlagener Ablauf:

```text
L4 Consolidation Agent
  -> Draft ConsolidationPlan
  -> Deterministic Gate
  -> Draft Baseline Projection
  -> Requirements Document Projection
  -> L4 Adequacy Feedback Agent
  -> L4 Completion/Revision Agent
  -> Gate
  -> Human Review
  -> Deterministic Apply
  -> Requirements Readiness
  -> L4IssuePlanning
```

Der Adequacy Feedback Agent bewertet nicht einzelne Requirements isoliert, sondern Kapitel und Gesamtfitness:

```text
functional
roles_access
ui_ux
data_privacy
non_functional
platform
process
acceptance_criteria
open_decisions
traceability
```

Moegliche Adequacy-Klassen:

```text
substantive
thin
missing
uncertain
conflicting
blocked_by_decision
not_applicable
```

Der Completion/Revision Agent darf nur kontrollierte Operationen vorschlagen:

```text
ADD_OPEN_DECISION
ADD_DISK_POINT
MARK_NEEDS_BREAKDOWN
REVISE_REQUIREMENT
SPLIT_REQUIREMENT
LINK_RELATED_ITEMS
DEFER
NO_CHANGE
```

Gates:

- `DISK` muss `evidenceState=not_stated|weakly_inferred` tragen.
- `DISK` muss `requiresHumanDecision=true` tragen.
- `DISK` darf nicht `ready_for_issue_planning` werden.
- Neue echte Requirements brauchen entweder belegte/abgeleitete Quellen oder Human Acceptance.
- Open-World-Ergaenzungen muessen im Dokument sichtbar von belegten Requirements getrennt bleiben.
- Der IssuePlanner darf nur akzeptierte, aktive und readiness-freigegebene Items konsumieren.

Warum dieser Schritt vor IssuePlanning gehoert:

- Sonst plant der IssuePlanner Arbeitspakete aus einem Stand, der kapitelweise noch fachlich duenn sein kann.
- Plattform, Datenschutz, NFRs, Akzeptanzkriterien oder Prozessfragen koennen sonst erst waehrend GitHub-Planung
  auffallen.
- Ein echter Requirements Engineer wuerde genau an dieser Stelle sagen: "Das ist noch nicht startklar, hier fehlen
  klaerende Entscheidungen oder fachlich plausible Diskussionspunkte."

MAF-Einordnung:

```text
Maker:       L4 Consolidation/Completion Agent
Checker:     L4 Adequacy Feedback Agent + deterministic Gate
Reflection:  bounded L4 Revision gegen Feedback
Human Gate:  Review UI
Apply:       deterministic ProjectState/Baseline update
```

Offene Designentscheidung vor Implementierung:

- Soll `DISK` als eigener Item-Typ im ProjectState landen oder zunaechst als Teil eines `l4-completion-proposals.json`?
- Empfehlung fuer den ersten Durchstich: `l4-completion-proposals.json` mit Gate und Human Review; erst akzeptierte
  Entscheidungen werden in ProjectState/Baseline angewandt.

Aktueller Implementierungsstand:

- `l4-completion` ist als eigener CLI-Knoten angelegt.
- Der Knoten konsumiert einen L4-Applied-Stand mit:

```text
canonical-requirements-baseline.json
provenance-map.json
requirements-readiness.json
requirements-document.md
```

- Der Graph ist buildbar:

```text
L4AdequacyFeedbackAgent[Tools]
  -> L4CompletionAgent[Tools]
  -> L4CompletionGate[det]
  -> Finalize
```

- Dry-Run:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-completion agent 20260715_165408_8d7645 --dry-run
```

Naechste Schritte:

1. Echten `l4-completion agent` Lauf fahren.
2. `l4-adequacy-report.json` und `l4-completion-proposals.json` fachlich pruefen.
3. HumanReview-Adapter fuer Completion-Proposals anbinden.
4. Deterministischen Apply fuer akzeptierte Completion-Proposals bauen.
5. Danach Readiness und IssuePlanning erneut auf dem erweiterten L4-Stand laufen lassen.

Status:

- Schritt 1 erledigt: echter Lauf `20260716_104851_ded72d`, Gate pass.
- Schritt 2 erledigt: 12 Adequacy-Findings, 8 Completion-Proposals, 1 Gate-Warnung zu UI/UX ohne Proposal.
- Schritt 3 erledigt: `l4-completion-review` nutzt die generische HumanReview-UI.

Naechster offener Schritt:

```text
Deterministischer l4-completion-apply:
  human-decisions.json
  + l4-completion-proposals.json
  + canonical-requirements-baseline.json
  -> erweiterter/aktualisierter L4-Stand
```

Status:

- `l4-completion-apply` ist umgesetzt.
- Lauf `20260716_104851_ded72d` wurde angewandt.
- Ergebnis:

```text
requirements=69
openDecisions=27
traceLinks=215
quality=pass
accepted=8
addedOpenDecisions=5
markedNeedsBreakdown=7
```

- Readiness auf dem erweiterten Stand:

```text
ready=31
needsDecision=28
needsBreakdown=8
deferredOrOptional=2
blockedByTraceability=0
```

Naechste Schritte:

1. Requirements-Dokument aus der erweiterten Completion-Applied-Baseline rendern. `[done]`
2. IssuePlanning auf dem neuen Readiness-Input neu laufen lassen. `[done]`
3. Vergleichen, ob Completion die IssuePlanning-Menge sinnvoll konservativer gemacht hat. `[done]`

Ergebnis des neuen IssuePlanning-Laufs:

```text
runId=20260716_132545_823a03
readyInputItems=31
issuePlanItems=19
coveredReadyItems=31
uncoveredReadyItems=0
CREATE=9
NEEDS_REVIEW=4
NO_CHANGE=6
warnings=1 large_issue_plan
```

Erkenntnis:

- Completion macht IssuePlanning konservativer, weil Breakdown-/Decision-Themen aus dem ready Input herausfallen.
- Der Planner muss dennoch jedes ready Requirement bewusst behandeln.
- Deshalb ist `unplanned_ready_requirement` jetzt ein Gate-Fehler.
- `NO_CHANGE` ist ein erlaubter plan-only Ausgang fuer Requirements, die fachlich wichtig sind, aber kein eigenes
  operatives GitHub-Issue erzeugen sollen.

Naechster offener Schritt:

```text
l4-issuplanning-review:
  issue-plan.json
  + issue-plan-gate-report.json
  -> HumanReview Session fuer CREATE/NEEDS_REVIEW/NO_CHANGE/LINK

l4-issuplanning-apply:
  human-decisions.json
  + issue-plan.json
  -> accepted-issue-plan.json / issue-plan-apply-report.json
```

Ziel:

- Der IssuePlanner bleibt ein eigener MAF-naher Knoten nach L4/Readiness.
- Er schreibt keine GitHub-Issues.
- HumanReview entscheidet den Issue-Schnitt, grobe Buendel, `NO_CHANGE` und `NEEDS_REVIEW`.
- Erst ein spaeterer GitHub-Agent konsumiert den akzeptierten IssuePlan und fuehrt Read-Only-Reconciliation bzw.
  spaeter kontrollierte Writes aus.

Status:

- `l4-issuplanning-review` ist umgesetzt.
- File-Mode validiert:

```text
scope=all -> 19 IssuePlanItems
scope=needs-human -> 4 IssuePlanItems
```

Implementierter Apply-Schritt:

```text
l4-issuplanning-apply
  issue-plan.json
  + human-decisions.json
  + issue-plan-gate-report.json
  -> accepted-issue-plan.json
  -> issue-plan-apply-report.json
```

Apply-Regeln:

- `accept`: Original-IssuePlanItem uebernehmen.
- `edit`: editierte Fassung uebernehmen und danach erneut mit `IssuePlanGate` pruefen.
- `reject`: Item nicht uebernehmen.
- `revise`: Item nicht in den akzeptierten Plan uebernehmen, sondern als `revisionRequested` im Report dokumentieren.
- Der akzeptierte Plan darf nur Gate-pass sein.
- GitHub bleibt weiterhin ausserhalb dieses Schritts.

Status:

- `l4-issuplanning-apply` ist umgesetzt.
- Der Knoten verweigert Apply ohne `human-decisions.json`.
- Er schreibt nach erfolgreichem Apply:
  - `applied/accepted-issue-plan.json`
  - `applied/accepted-issue-plan-gate-report.json`
  - `applied/issue-plan-apply-report.json`

Naechster operativer Schritt:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-issuplanning-review 20260716_132545_823a03 --interactive --scope all
```

Danach:

```bash
dotnet run --project AgenticSdlc.Host/AgenticSdlc.Host.csproj -- l4-issuplanning-apply 20260716_132545_823a03
```
