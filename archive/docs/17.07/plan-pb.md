# Plan 17.07 - Product Backlog (PBI-Modell, Typen, IssuePlanner-Integration)

Status: Entwurf / Diskussionsskizze. Noch kein Code. **Companion** zu `plan-re-backlog.md`:

```text
plan-re-backlog.md = der PROZESS   (wie die Clarification-Analyse Tiefe erzeugt:
                                     Assemble / Interrogate / Resolve / Prioritize)
plan-pb.md         = das PRODUKT    (was ein PBI ist, PBI-Typen, Granularitaet,
                                     IssuePlanner-Integration, ID-Kette, Autoritaet)
```

Basiert auf User-Vorschlag (17.07). Bewertung + Verfeinerungen in Abschnitt 10.

Rev. 2: Flag A + Flag B **entschieden** (10); `version` + `derivationHistory` + `priorityRank` +
`blocksScope` ergaenzt; Readiness dreistufig (mit `plan-re-backlog` Rev.3 synchronisiert).

---

## 1. Drei getrennte Konzepte (Kernabgrenzung)

```text
Requirement            beschreibt, WAS fachlich gelten soll (Fakt/Randbedingung)
Product Backlog Item   beschreibt eine PRIORISIERBARE fachliche Produkt- oder Klaerungseinheit
GitHub Issue           beschreibt die konkrete OPERATIVE Umsetzung
```

Autoritaet klar getrennt:

```text
Requirements  autoritativ fuer:  fachliche Aussagen, Randbedingungen
Product Backlog autoritativ fuer: Feature-Schnitt, Prioritaet, MVP-Zuordnung, Scope, Backlog-Reife
GitHub        autoritativ fuer:  Umsetzung, Assignees, Labels, Milestones, technischer Status
```

Folge: **geschlossenes GitHub-Issue != PBI abgeschlossen != Requirement erfuellt.** Diese
fachliche Interpretation bleibt Aufgabe eines Reconciliation-Schritts oder menschlicher Abnahme.

## 2. Volle ID-Kette

```text
Claim -> Requirement -> Canonical Requirement -> Product Backlog Item -> IssuePlan -> GitHub Issue
```

Der **PBI ist die stabile fachliche Planungseinheit** in der Mitte. IssuePlan referenziert
primaer die `PBI-*`-IDs; die zugrunde liegenden `CAN-REQ-*` werden zusaetzlich mitgefuehrt
(volle Traceability). Nach dem GitHub-Write wird eine explizite `PBI <-> GitHub-Issue`-
Zuordnung gespeichert.

## 3. Zwei Granularitaeten: Feature-Analyse vs. PBI-Schnitt

Wichtige Unterscheidung (aus dem Vorschlag, uebernommen):

```text
Feature-Ebene = ANALYSE-Granularitaet
  Dort werden alle Requirements, Querschnittsregeln (Rollen/Validierung/Datenschutz/Audit/
  Lifecycle), Fehlerfaelle und offenen Entscheidungen GEMEINSAM sichtbar.
  (= der ASSEMBLE/INTERROGATE-Kontext aus plan-re-backlog)

PBI-Ebene = PRIORISIERUNGS-/PLANUNGS-Einheit
  Der analysierte Feature-Kontext wird in kleinere, wertorientierte VERTIKALE Slices geschnitten.
```

Granularitaets-Regel fuer PBIs:
- kleiner als ein Grossfeature, groesser als ein technischer Task
- repraesentiert klaren Nutzer-/Produktnutzen ODER eine notwendige Produktentscheidung
- unabhaengig priorisierbar UND gemeinsam abnehmbar (INVEST: Small + Valuable)
- technische Schichten (Frontend / API / DB) gehoeren NICHT auf PBI-Ebene -> das ist Issue/Subtask

Beispiel: gutes PBI = "No-Go-Eintraege erfassen und anschliessend anzeigen"; die Zerlegung in
FE/API/DB kommt spaeter als Issues/Subtasks.

## 4. PBI-Typen + Coverage-Invariante

Ein gemeinsames Backlog statt zweier isolierter Welten (Umsetzung vs. Klaerung):

```text
Delivery-PBI       umsetzbare Produktfunktionalitaet
Clarification-PBI  notwendige Klaerungs-/Breakdown-Arbeit (schedulbare Entscheidung)
Deferred-PBI       bewusst zurueckgestellt, sichtbar, nicht in Delivery-Issues
Out-of-scope-PBI   bewusst ausgeschlossen, sichtbar
```

**Coverage-Invariante (checkbare Backlog-Vollstaendigkeit):**
jedes relevante Requirement ist in GENAU EINER Einordnung platziert - Delivery-PBI,
Clarification-PBI, Deferred oder Out-of-scope. Kein Requirement bleibt verwaist.

Wichtig, ehrlich: **Platzierung != Aufloesung.** "Out-of-scope" ist eine gueltige Platzierung,
kein geloester Punkt. Diese Invariante ist strukturell pruefbar (siehe Gate), sie behauptet
KEINE absolute fachliche Vollstaendigkeit.

## 5. PBI-Datenmodell (Entwurf, schlank)

```jsonc
{
  "pbiId": "PBI-007",                    // stabile ID, deterministisch aus identityKey (Flag A)
  "identityKey": "no-go-anzeigen-erfassen", // stabiler Schluessel -> ID ueberlebt Re-Laeufe
  "version": 1,                          // hochzaehlen bei Re-Ableitung / veraendertem Zuschnitt
  "type": "delivery",                    // delivery | clarification | deferred | out_of_scope
  "title": "No-Go-Eintraege eines Bewohners anzeigen und erfassen",
  "goal": "Als Betreuer will ich ..., damit ...",
  "scope": { "inScope": [...], "outOfScope": ["bearbeiten", "loeschen"] },
  "requirementIds": ["CAN-REQ-027","CAN-REQ-028","CAN-REQ-003","CAN-REQ-006"],
  "acceptanceCriteria": [ ... ],         // testbar/ueberpruefbar, format-agnostisch (Fit Criterion)
  "openDecisions": [                      // zwei Achsen (siehe plan-re-backlog §6)
    { "question": "duerfen Bewohner selbst editieren?",
      "kind": "stakeholder_decision", "evidence": "not_stated", "resolution": "open_decision",
      "blocksScope": false }             // trifft die Frage den PBI-Kern? -> steuert readiness
  ],
  "mvp": "required_for_mvp",             // mvp | required_for_mvp | later | out_of_scope | undecided (Vorschlag)
  "priorityRank": 3,                     // geordnetes Backlog, nicht nur Inventar (Vorschlag, human-autorisiert)
  "dependencies": ["PBI-011"],           // andere PBIs (z.B. Rechtemodell)
  "readiness": "backlog_ready",          // backlog_ready | ready_with_nonblocking_questions | blocked_by_decision
  "traceability": { "canonicalRequirementIds": [...], "l3": [...], "l1Req": [...], "claims": [...] },
  "provenance": { "featureContextId": "FC-nogo", "clarifyRunId": "...", "humanDecisionIds": [...],
                  "derivationHistory": [ { "version": 1, "runId": "...", "change": "created" } ] }
}
```

Kein verschachteltes Linsen-Schema, keine analytischen Zwischenschritte im PBI (die liegen im
`clarification-report`/Trace, siehe plan-re-backlog §9).

## 6. Direkt PBI-*, keine Zwischenebenen

Apply erzeugt DIREKT stabile `PBI-*`-Eintraege. KEINE Kaskade FeatureUnit -> BacklogCandidate
-> PBI. Diese Ebenen wuerden das System unnoetig aufblaehen. Der Feature-Kontext ist die
ANALYSE-Sicht (transient, im Agenten/Report), das PBI ist das persistierte Ergebnis.

## 7. MVP als leichtes Feld + abgeleitete MVP-Sicht

Keine grosse MVP-Agentenphase. Jeder PBI traegt `mvp`-Feld (5.). Der Agent SCHLAEGT eine
Einordnung vor; die endgueltige MVP-Auswahl ist eine Produktentscheidung und wird menschlich
autorisiert. Eine MVP-Sicht wird deterministisch aus den Feldern projiziert (kein eigener
Agent). Volle User Story Map / Narrative Flow bleibt spaeter (plan-re-backlog §10).

## 8. IssuePlanner-Integration

Der bestehende Issue Planner wird auf das Product Backlog umgestellt. Er konsumiert nicht mehr
einzelne `CAN-REQ`, sondern akzeptierte PBIs. Er klaert NICHT mehr fachlich und rekonstruiert
KEIN Backlog - er entscheidet nur die operative Abbildung:

```text
ein PBI          -> ein Issue
ein PBI          -> mehrere Issues
mehrere kleine PBIs -> ein gemeinsames Issue
kein Issue       -> NO_CHANGE | deferred | out_of_scope
```

Konkrete Aenderung (Integrationspunkt): `IssuePlanItem` bekommt `pbiId`(s) als PRIMAERE
Quelle; `sourceRequirementIds` bleiben SEKUNDAER erhalten (heute sind sie primaer). Nach dem
GitHub-Write: `PBI <-> Issue`-Mapping persistieren (Basis fuer spaetere Reconciliation).

## 9. Gate + Human Review (bewaehrtes Muster)

```text
Requirements Clarification Agent -> deterministisches Gate -> Human Review -> Apply -> Product Backlog
```

Gate = schlank, strukturell (KEINE absolute Vollstaendigkeit):
- gueltige Source-Requirement-IDs
- klarer Feature-Scope (inScope/outOfScope gesetzt)
- >= 1 pruefbares Fit Criterion je Delivery-PBI
- erkannte offene Dimensionen explizit behandelt (nicht still in Acceptance Criteria eingebaut)
- nicht belegte Vorschlaege korrekt markiert (evidence/resolution)
- nachvollziehbare Prioritaets-/MVP-Einordnung inkl. `priorityRank` (Vorschlag, human-autorisiert)
- **Coverage-Invariante (4.)**: jedes relevante Requirement genau einmal platziert
- Readiness per SCOPE-CHECK: trifft eine offene Entscheidung den Kern (Verhalten/Daten/Rechte/
  Abnahme) des geschnittenen PBI -> `blocked_by_decision`; sonst `backlog_ready` /
  `ready_with_nonblocking_questions` (siehe plan-re-backlog §11/§12)

Human Review konzentriert sich auf das NEUE/Unsichere: vorgeschlagene Defaults, Scope-Grenzen,
Rollenentscheidungen, MVP-Zuordnungen, Open-World-Ergaenzungen. Bereits belegte Requirements
muessen nicht erneut geprueft werden. Optional 1 Judge fuer semantische Qualitaet.

## 10. Bewertung des Vorschlags + Verfeinerungen

**Gesamturteil: stark und tragfaehig.** Er vervollstaendigt plan-re-backlog auf der PRODUKT-
Seite, ohne die Architektur zu ersetzen, und bleibt MAF-konsistent (ein Agent, bewaehrtes
Gate/Review/Apply). Uebernommen: Drei-Konzept-Abgrenzung, ID-Kette, Feature-vs-PBI-Granularitaet,
PBI-Typen + Coverage-Invariante, direkt-PBI (keine Zwischenebenen), MVP-Feld, IssuePlanner-
Mapping, Autoritaetstrennung.

Zwei Punkte ENTSCHIEDEN:

**Flag A - PBI-ID-Stabilitaet: ENTSCHIEDEN.**
Erstdurchstich = abgeleitete View + **deterministische Identitaets-Funktion**: die PBI-ID wird
aus einem stabilen `identityKey` (Anker-Requirement / Feature-Slug) abgeleitet - KEIN neuer
persistierter Item-Typ. Jeder PBI traegt zusaetzlich `version` + `derivationHistory` (aus 2.
Kritik), damit Re-Laeufe, veraenderte Zuschnitte oder neue Cluster nachvollziehbar bleiben und
bestehende Issue-Mappings stabil bleiben. Ein persistiertes PBI-Register kommt erst, wenn
Delta/Reconciliation ueber Sprints real startet. So bleibt "View zuerst" gewahrt, und die
IssuePlanner-Integration (pbiId primaer) ist trotzdem moeglich.
Grenze (ehrlich): aendert sich der Feature-Zuschnitt fachlich STARK, kann derselbe identityKey
nicht mehr passen -> dann bewusste Neu-Vergabe + `supersedes`-Vermerk in `derivationHistory`
(nicht still). Das haelt das Mapping ehrlich statt es unbemerkt brechen zu lassen.

**Flag B - Clarification-PBI vs. openDecision am Delivery-PBI: ENTSCHIEDEN (Regel).**
```text
openDecision am Delivery-PBI   Scope-/Detailfrage INNERHALB eines lieferbaren Slices,
                               klein, blockiert die Lieferung nicht (z.B. Leer-Eingabe-Verhalten)
                               -> lebt als openDecision.blocksScope=false am PBI
eigener Clarification-PBI      wenn die Klaerung selbst eine SCHEDULBARE Arbeits-/Entscheidungseinheit ist
                               (spannt mehrere Features / blockiert mehrere PBIs / L4 markierte needs_breakdown)
                               -> type=clarification, eigener PBI, ggf. als dependency verlinkt
```
Human Review darf die Zuordnung anpassen.

## 11. Erstdurchstich + Erfolgskriterien

Nicht alle 69 Requirements. Erste Auswahl MVP-naher Features verschiedener Problemtypen:
Login · Rollen/Berechtigungen · About-Me · No-Go · verbale/nonverbale Kommunikation.

Erfolg wird NICHT an der Zahl der Fragen/PBIs gemessen, sondern:
- vorhandenes verteiltes Wissen vollstaendig im Feature-Kontext (recovery),
- bislang nie geklaerte umsetzungs-/abnahmerelevante Punkte sichtbar (nicht: irrelevante Fragen),
- neue Vorschlaege korrekt als Vorschlag/Entscheidung markiert (keine stille Wahrheit),
- abgeleitete Issues fuer Entwickler verstaendlicher/vollstaendiger, weniger Rueckfragen.

## 12. Zielarchitektur (volle Kette)

```text
L4 Canonical Requirements Baseline
  -> Requirements Clarification / RE Backlog Structuring   (plan-re-backlog: ein Agent)
  -> Gate (schlank, strukturell)
  -> Human Review
  -> Product Backlog Apply
  -> ProductBacklogView (PBI-*)
  -> Backlog Readiness
  -> Issue Planning (konsumiert PBIs)
  -> GitHub Reconciliation
  -> GitHub Write (+ PBI<->Issue-Mapping)
```

Bestehende Architektur wird nicht ersetzt, sondern fachlich vervollstaendigt: L1-L4 bleiben
fuer Evidenz/Discovery/Konsolidierung; der neue Schritt liefert Analyse + Feature-Zusammenfuehrung
+ Backlog-Strukturierung; Issue Planning bleibt operative Zerlegung; GitHub bleibt Umsetzung.

Gewuenschtes Ergebnis: ein stabiler, rueckverfolgbarer Product Backlog, in dem jedes relevante
Requirement durch einen Delivery-PBI, einen Clarification-PBI, eine Deferred- oder eine
Out-of-scope-Entscheidung eingeordnet ist; jeder priorisierte PBI hat genug geklaerten Scope +
pruefbare Kriterien ODER macht seine offenen Entscheidungen explizit sichtbar. Der Issue Planner
erzeugt daraus belastbare Issues, ohne selbst noch Requirements Engineering reparieren zu muessen.
