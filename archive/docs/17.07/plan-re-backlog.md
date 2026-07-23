# Plan 17.07 - RE Backlog Structuring (Analyse-Phase zwischen L4 und IssuePlanning)

Status: Entwurf / Diskussionsskizze. Noch kein Code. Arbeitsplan fuer den naechsten
fachlichen Hebel, wird gemeinsam iteriert.

Vorlaeufer: `17.07/IST-Zusatnd.md` · `15.07/l4-how-it-works.md` · `16.07/plan-repository-viewboundary.md`.
Diese Datei ersetzt den frueheren Ueberbau-Entwurf `plan-re-backlog-structuring.md`.

Rev. 2 (Kritik eingearbeitet): ein Clarification-Agent statt 4 Knoten; schlankes Kern-Set +
Volere/FURPS+ als Hintergrund-Heuristik; Zwei-Achsen-Resolve (Evidenz x Aufloesung); hybrides
Assemble (det. Kandidaten + agentische Clusterpruefung); Fit Criteria format-agnostisch;
Prioritaet/MVP nur als human-autorisierter Vorschlag; schlankes strukturelles Gate;
Erstdurchstich auf Kern-Feature-Auswahl. Zielarchitektur: Abschnitt 13.

Rev. 3 (2. Kritik): CUT als 5. Bewegung (Feature=Analyse- vs PBI=Liefergranularitaet,
1 Feature -> 1..n PBIs); Readiness dreistufig nach Scope-Relevanz (backlog_ready /
ready_with_nonblocking_questions / blocked_by_decision) - verfeinert die fruehere pauschale
"ready_with_open_questions"-Regel; PB geordnet/priorisiert (Rank/Prioritaetsklasse, nicht nur
mvp-Flag); Gegenmetriken in den Erfolgskriterien. PBI-ID-Stabilitaet: siehe plan-pb §10 Flag A.

---

## 1. Ziel (unverrueckt)

Aus einem messy Transkript **stabile, tief genug geklaerte Anforderungen** -> daraus
**stabile, vollstaendige Issues**, an denen ein Entwickler nicht raten muss.

"Vollstaendig/sauber" ehrlich definiert:

```text
sauberes PB = jedes Item ist ENTWEDER fertig spezifiziert (testbare Akzeptanzkriterien)
              ODER seine offenen Dimensionen stehen als scharfe, entscheidungsreife Fragen da
           =  KEINE stillen Luecken.   NICHT: null offene Fragen.
```

Sauber != null offene Fragen. In RE ist ein Backlog mit offenen Entscheidungen gesund
(DEEP: **E**mergent). Ziel ist: keine *versteckten* Luecken, jedes Item auf der zu seiner
Prioritaet passenden Reife.

## 2. Wurzeldiagnose (belegt am echten No-Go-Trace)

Nachverfolgt in `runs/l4-completion/20260716_104851_ded72d/completion/applied/canonical-requirements-baseline.json`:

Das "Feature No-Go" liegt als **6 unverbundene Requirements** in der L4-Baseline:

```text
CAN-REQ-027  No-Go-Seite existiert                                   active
CAN-REQ-028  Plus-Button, hinzufuegen als Liste                      active
CAN-REQ-029  visuelles Symbol, durchforstbar                         open_decision
CAN-REQ-003  Pflichtfelder/Formate/Laengen/Verhalten bei leer        needs_breakdown
CAN-REQ-005  Aufbewahrung/Korrektur/Archivierung/LOESCHEN/Entzug     open_decision
CAN-REQ-006  Rollen-/Einrichtungstrennung, Protokollierung           open_decision
```

Also: Loeschen steht in 005, Rechte in 006, Validierung/leere-Eingabe in 003 - jeweils
mit expliziter No-Go-Nennung im Text. **Die Tiefe ist NICHT verloren.**

Was schiefging:
- **L4 hat NICHT versagt.** Es hat alles bewahrt (Traceability intakt: 027<-REQ-15,
  005<-L3-REQ-005) UND die Luecke geflaggt (Completion-Notiz L4-CP-007: "in testbare
  Akzeptanzkriterien uebersetzen ... Validierungsfehler bei leeren Eingaben, Aktionen pro Rolle").
  Es hat nur nicht *aufgeloest*.
- **Fehlender Schritt:** niemand bindet die 6 Requirements zu einem Feature zusammen.
- **Readiness-Filter zerreisst:** er filtert pro *isoliertem* Requirement nach Status. 003
  (needs_breakdown), 005/006/029 (open_decision) werden zurueckgehalten -> IssuePlanning sah
  nur 027/028. Das Issue bekam die flache Haelfte, verlor die tiefe.

**Wurzel = fehlende Analyse-Phase + Filter auf falscher Granularitaet. Kein L4-Defekt.**

## 3. RE-Einordnung: die fehlende Analyse-Phase

```text
1 Elicitation    (sammeln)          -> L1/L2   ✓ closed world, reproduzierbar, traceable
2 Discovery      (Luecken/Breite)    -> L3      ✓ open-world Themen finden
3 Consolidation  (kanonisch machen)  -> L4      ✓ bewahrt + markiert, keine Aufloesung
4 ANALYSIS       (klaeren/schaerfen) -> FEHLT   <-- dieser Schritt
5 Specification  (aufschreiben)      -> IssuePlanning (heute zu frueh/flach)
6 Management      (lebend/traceable) -> ProjectState ✓
```

Der neue Schritt ist die **Analyse-Phase**: pro Feature zu Ende denken.

## 4. Wie generisch Tiefe entsteht (der Mechanismus)

Tiefe kommt NICHT aus einem laengeren, universellen Fragenkatalog (das erzeugt hohle
Checklisten = dokumentiertes L3-Versagensmuster). Tiefe kommt aus **Forcing Functions**,
die alle domaenenfrei sind:

```text
generisch tiefe  =  generische RE-Linsen (Abdeckung, nicht Tiefe)
                  × Testbarkeit/Beispiele erzwingen (die eigentliche Tiefe-Maschine)
                  × Erdung an der Quelle (gegen Checklisten-Verfall)
                  × offene Entscheidungen erzwingen (ehrliche Vollstaendigkeit)
                  × Klaertiefe nach Prioritaet (kein Over-Engineering)
```

Kern-Forcing-Function: "schreib ein konkretes Given/When/Then, das ein Entwickler
ausfuehren koennte". Das laesst sich nicht vage erfuellen -> zwingt Daten/Operation/Rolle/
Sonderfall heraus. Konkretheit ist die Tiefe, nicht die Zahl der Fragen.

### 4.1 Zwei Ebenen: erzwungene Kernfragen (schlank) vs. Hintergrund-Heuristik (breit)

Praezisierung (aus Kritik, uebernommen): trenne, was der Agent PRO FEATURE ZWINGEND
beantwortet, von dem breiten Vollstaendigkeitsraster, das er nur als Hintergrund heranzieht.
Sonst kippt es in checklistengetriebenes Abhaken (L3-Versagensmuster) und blaeht das
Output-Modell auf.

**Erzwungenes Kern-Set (5 Fragen, jedes Feature - das ist das INTERNE Analyse-Raster):**

```text
1 Zweck & Scope            was leistet das Feature, was gehoert NICHT dazu?
2 Verhalten & Daten        welche Aktionen/Operationen an welchen Daten/Feldern?
3 Regeln & Berechtigungen  Geschaeftsregeln, Validierung, wer darf?
4 Fehler & Lifecycle       Sonderfaelle, leer/Duplikat/Grenze, Aendern/Loeschen/Aufbewahrung?
5 Abnahme                  pruefbare Fit Criteria pro geklaertem Punkt
```

**Hintergrund-Heuristik (breit, NICHT persistiert):** Das validierte RE-Raster erdet die
Analyse, wird im Prompt + in der Evaluation referenziert, aber NICHT als Pflichtmatrix
ausgefuellt oder gespeichert:
- **Volere Requirements Shell** (Robertson & Robertson) - Pro-Requirement-Template mit dem
  Kern **Fit Criterion** (messbares Kriterium = Akzeptanzkriterium; format-agnostisch, siehe 4.2).
- **Volere / FURPS+ / ISO 25010 Requirement-Typen** als Abdeckungsdimensionen inkl. Design/UX:
  Funktional&Daten · Look-and-Feel · Usability · Performance · Operational · Security ·
  Maintainability/Legal/Cultural.

**Adaptive Vertiefung + Design-Vorbehalt:** Der Agent vertieft eine Hintergrund-Dimension nur,
wenn sie aus Featuretyp, Projektkontext oder vorhandenen Querschnitts-Requirements relevant
wird. AUSNAHME (Design-Vorbehalt): fuer nutzer-sichtbare Features MUSS **Look-and-Feel/Usability**
geprueft werden - sonst faellt der Design-Aspekt (No-Go: CAN-REQ-029) wieder durch, wie in der
frueheren 5-Linsen-Fassung. Das ist die eine Dimension, die trotz schlankem Kern-Set nicht
still wegfallen darf.

### 4.2 Wie eine Linse Fehlendes findet: enumerate-then-check

Eine Linse darf nicht deskriptiv sein ("welche Rollen gibt es?" -> listet nur Vorhandenes,
findet nichts Fehlendes). Sie traegt eine **Vollstaendigkeits-Heuristik**: der Agent leitet
den ERWARTUNGSSATZ fuer DIESES Requirement ab und prueft jedes Element einzeln
(vorhanden | fehlt | N/A). Die Abwesenheit IST der entdeckte Gap.

```text
Funktional/Daten:  enumeriere Attribute der Entitaet, je Pflicht/Format/Grenze/Leerzustand
Operationen:       instanziiere den VOLLEN Operationssatz fuer den Typ
                   (Daten: anlegen/anzeigen/aendern/loeschen/suchen; Dialog: Intents+Fehler;
                    Steuerung: Zustandsuebergaenge) - pruefe JEDE
Security/Rollen:   je Operation: welche Rolle darf/darf nicht? serverseitig erzwungen?
Regeln:            je Happy Path: Vorbedingung? Verletzung? (leer/Duplikat/Grenze/Abbruch/Undo)
Look&Feel/Usab.:   wie sieht es aus, wie navigiert/bedient man es? Barrierefreiheit?
```

Generisch, weil die Heuristik dem Agenten sagt WIE er den Erwartungssatz ableitet - keine
feste Liste. Er instanziiert pro Usecase (CRUD vs Intents vs Zustaende). Jede Linse hat ein
`not_applicable` mit Begruendung (wie L3, "kein Fuellstoff").

### 4.3 Warum das Tiefe liefert, wo L3/L4 Breite liefern

```text
L3/L4  = Discovery ueber die MENGE: "welche Requirements/Themen fehlen im Ganzen?"
         -> offene, freie Erfindung (open-world), ungleichmaessig (euer Goodhart-/Varianz-Befund)
Clarify = Completion INNERHALB eines Requirements gegen ein GESCHLOSSENES Standard-Schema
         -> enumerate-then-check pro Feld -> zuverlaessige Tiefe, keine freie Erfindung
```

Der Trick ist die **Granularitaet + das geschlossene, validierte Schema** (Volere-Typen),
nicht die Linse an sich. Ein geschlossenes Schema hat einen deterministischen Vollstaendigkeits-
balken (jeder Typ gefuellt ODER N/A); offene Breitensuche liefert Abdeckung, nicht Pro-Item-Tiefe.

### 4.4 No-Go durch die offizielle Taxonomie (inkl. Design)

```text
Funktional/Daten:  Attribute? Text da; Titel/Kategorie/Zeit? OFFEN
                   Operationen: anlegen✓ anzeigen✓ | aendern/loeschen/suchen? OFFEN
Look and Feel:     CAN-REQ-029 (rotes Stoppschild, durchforstbar) = genau dieser Typ.
                   Taxonomie ERZWINGT den Slot: "wie sieht die Liste aus, wie navigiert man?"
Usability:         schnelle Eingabe, Barrierefreiheit? OFFEN
Security/Rollen:   wer darf pflegen, Protokollierung? -> CAN-REQ-006
Legal/Operational: sensible Daten, Aufbewahrung -> CAN-REQ-005; sonst frueh N/A
Fit Criterion:     je geklaerter Punkt -> Given/When/Then
```

Deshalb greift der Step doppelt: (a) ASSEMBLE blendet CAN-REQ-029 als Querschnitt ein, der
Agent SIEHT die Design-Frage; (b) der Design-Vorbehalt (4.1) erzwingt Look-and-Feel-Pruefung
bei nutzer-sichtbaren Features auch dann, wenn kein 029 existiert. So faellt Design nicht
durch, ohne die volle Volere-Matrix persistieren zu muessen. (Dein Einwand, geloest.)

### 4.5 Over-Engineering-Schutz

Volere hat ~15 Typen. NICHT alle tief auf alle 69 Requirements zwingen:
- Tiefe **proportional zur Prioritaet** (DEEP-"D"): MVP-nah tief, Rest grob.
- **Grosszuegiges N/A** mit Begruendung; frueh sind Maintainability/Legal/Cultural meist N/A.
- Gewicht frueh auf Funktional/Daten + Look-and-Feel + Usability + Security - genau die No-Go-Gaps.

## 5. Der Step: EIN Agent, fuenf innere Bewegungen (keine 5 Knoten)

Wichtig (aus Kritik): Die Bewegungen sind das INNERE Denkmodell EINES
Requirements-Clarification-Agenten - NICHT eigene Workflow-Knoten mit Zwischenartefakten.
Danach das etablierte Muster. So bleibt die Architektur konsistent mit L3/L4
(Agent[Tools] -> Gate -> Finalize), kein neuer umfangreicher Subworkflow.

```text
Input: L4 canonical baseline (alle Feature-Teile koexistieren, volle Traceability)

Ein Clarification-Agent (mit Tools), intern:
  1 ASSEMBLE     Feature-Kontext zusammensetzen: direkte Feature-Requirements + relevante
                 Querschnitt-Requirements (No-Go: 027/028/029 + 003/005/006)
                 -> behebt Fragmentierung/Verlust
  2 INTERROGATE  Kern-Set (4.1) durchgehen, testbare Beispiele erzwingen
                 -> Luecke sichtbar, unabhaengig davon ob Tiefe existiert
  3 RESOLVE      jede geklaerte/offene Dimension entlang der ZWEI Achsen (siehe 6) einordnen
  4 CUT          Feature-Kontext in 1..n PBIs schneiden. Feature = ANALYSE-Granularitaet,
                 PBI = Liefer-/Priorisierungs-Granularitaet: vertikale, wertorientierte Slices,
                 unabhaengig priorisierbar + gemeinsam abnehmbar. KEIN 1-Feature=1-PBI-Automatismus
                 (No-Go: Anzeige+Erfassung als ein PBI, Lifecycle-Klaerung als eigener). Siehe plan-pb §3.
                 Begruendeter Schnitt-Vorschlag; Human Review darf ihn aendern.
  5 PRIORITIZE   je PBI: Prioritaetsklasse/Rank + MVP-Relevanz als VORSCHLAG (steuert Klaertiefe, DEEP);
                 echte MVP-/Scope-/Rank-Entscheidung NICHT allein - open-world markiert, human-autorisiert.
                 (PB = geordnet/priorisiert, nicht nur Inventar - Rank/Prioritaetsklasse, nicht nur mvp-Flag.)
  -> save (Tool)

Danach:  deterministisches Gate (schlank, 12) -> Human Review -> Apply -> PB-View (PBIs).

Durchgehend: Traceability erhalten. Der Step erfindet keine neue Wahrheit, er setzt
bereits rueckfuehrbare Items neu in Beziehung. Invariante bleibt: alles rueckfuehrbar
auf L1 - oder explizit als open-world/derived deklariert + human-autorisiert.
```

## 6. Resolve: ZWEI Achsen (Evidenz x Auflösung) statt einer Ladder

Praezisierung (aus Kritik): die fruehere 5-Zustand-Ladder vermischte Herkunft und Reifegrad.
Sauberer sind zwei orthogonale Achsen pro Dimension:

```text
Evidenz-Achse (woher):      stated | derived | not_stated | weakly_inferred
Aufloesungs-Achse (Reife):  resolved | partial | proposed_default | open_decision | not_applicable
```

Regel bleibt "erst maximal das Gesagte": Evidenz zuerst aus stated/derived schoepfen; ein
Vorschlag (proposed_default) oder eine offene Frage (open_decision) ist erst erlaubt, wenn
stated/derived fuer diese Dimension NACHWEISLICH erschoepft sind (kein faules Vorschlagen).

Die frueheren 5 Zustaende sind Spezialfaelle:

```text
stated              = evidence:stated     × resolution:resolved
derived_present     = evidence:derived    × resolution:resolved
absent_defaultable  = evidence:not_stated × resolution:proposed_default   -> Human Review
absent_stakeholder  = evidence:not_stated × resolution:open_decision      -> Stakeholder
not_applicable      =                       resolution:not_applicable      (+ Begruendung)
NEU ausdrueckbar:   = evidence:stated      × resolution:partial   (gesagt, aber unvollstaendig)
```

Die Interrogation (Beispiele) macht die Luecke sichtbar - auch bei "existiert nirgends".
Beispiel: Test fuer das 101. No-Go erzwingt "Limit pro Bewohner?"; nie gesagt, kein Req
-> evidence:not_stated × resolution:proposed_default.

Ehrliche Decke bei open_decision: der Step produziert KEINE erfundene Antwort, sondern eine
**entscheidungsreife offene Frage** (+ optionale MVP-Annahme). Mehr kann kein RE hier leisten.

## 7. Agent-Vorschlag + Human-Adjudikation (resolution: proposed_default / open_decision)

Der Step laedt nicht nur Fragen ab - der Agent schlaegt Aufloesungen vor. Muster wie
gehabt: Maker -> Gate -> HumanReview -> Apply (wie L4-Completion, Ledger-Adjudikation).

Schutz (Projektregel): **Vorschlag != Wahrheit.**
- als Vorschlag/Annahme markiert, nie als etabliertes Requirement
- `evidenceState = not_stated | weakly_inferred`
- geerdet/begruendet ("ueblicher Default fuer Listenpflege: Loeschen mit Bestaetigung - aber nicht gesagt")
- Human-Freigabe erzwingen; bis dahin bleibt es offene Entscheidung

Zwei Sorten offener Fragen (Agent klassifiziert):

```text
engineering_default   sicherer Default ("leerer Eintrag -> Validierungsfehler")
                      -> Agent schlaegt vor, Mensch nickt meist ab
stakeholder_decision  fachliche Scope-Frage ("duerfen Bewohner selbst editieren?")
                      -> Agent darf ANNAHME vorschlagen, MUSS "braucht Stakeholder" flaggen,
                         entscheidet NICHT
```

Angenommener Vorschlag -> `HUMAN_ACCEPTED_OPEN_WORLD`-Kriterium (Herkunft existiert im Modell
schon; 3 solche Promotions im State). Traceability: "vom Clarify-Agent vorgeschlagen, vom
Menschen am X akzeptiert". Keine neue Maschinerie, nur dein Open-World-Accept-Muster hier.

## 8. Wo das saubere PB entsteht und warum die Issues besser werden

**Wo:** Apply-Output des Steps = die **Product Backlog View** (abgeleitete View ueber dem
State, siehe 10). Angedockt NACH L4-Baseline, VOR Readiness. Kein neues Dokument, sondern
der State projiziert als **Feature-Einheiten** statt loser Requirements.

**Warum die Issues klarer/vollstaendiger werden - Kausalkette:**
Heute bekommt IssuePlanning 2 isolierte active-Fragmente. Nach dem Step bekommt es eine
zu Ende gedachte Feature-Einheit:
- alle geltenden Regeln verfuegbar (Validierung 003, Lifecycle 005, Rechte 006) -> echte Akzeptanzkriterien moeglich
- offene Entscheidungen am Feature angeheftet -> sichtbar im Issue statt in Separat-Requirements verschwunden
- Prioritaet -> MVP-Schnitt klar

**Depth-in -> Depth-out.** Der Planner bleibt treu; jetzt ist das, wozu er treu ist, tief.
Schaerfe entsteht im INPUT, nicht im Planner (IssuePlanning ist zu spaet, um RE zu reparieren).

Einordnung der Pipeline:
```text
L4 canonical baseline
  -> RE Backlog Structuring   NEU: Assemble/Interrogate/Resolve/Prioritize
  -> Readiness                arbeitet auf der Feature-Einheit (nicht mehr pro isoliertem Req)
  -> IssuePlanning            konsumiert PB-View
  -> GitHub Reconciliation
```

Living Loop: neues Sprint-Transkript -> L1-L4 Delta -> Clarify-Delta (nur geaenderte Features
neu verhoeren) -> PB-View + Readiness neu -> IssuePlanning-Delta -> GitHub Reconciliation.

## 9. Output-Modell (minimal, keine Ueber-Struktur)

Trennung (aus Kritik): das breite Analyse-Raster wird NICHT pro Feature persistiert. Der
PB-View traegt nur den schlanken fachlichen Output; die analytischen Zwischenschritte
(welche Linse/Dimension, enumerate-then-check) landen im Agententrace bzw. einem kompakten
`clarification-report`, nicht im View.

Persistierter PB-View pro Feature-Einheit, bewusst schlank:

```text
- assembledRequirementIds[]     welche CAN-REQ dieses Feature bilden (core + crosscutting)
- statement                     Story-Form: Als <Rolle> will ich <Ziel>, damit <Nutzen>
- scope                         { inScope[], outOfScope[] }
- fitCriteria[]                 testbar/ueberpruefbar, format-agnostisch (Given/When/Then ODER
                                messbares Kriterium ODER deklarative Regel), je mit Herkunft
- openDecisions[]               { question, kind(engineering_default|stakeholder_decision),
                                  proposedResolution?, evidence, resolution }   (zwei Achsen, 6)
- priority / mvpRelevance       leichtes Feld (Vorschlag, human-autorisiert), steuert Klaertiefe
- traceability                  auf CAN-REQ / L3 / L1-REQ / Claims
- readiness                     backlog_ready | ready_with_open_questions | needs_stakeholder_decision
```

Kein grosses verschachteltes Schema, keine gespeicherte Linsenmatrix. Die Tiefe steckt in
fitCriteria + openDecisions, nicht in Feld-Bueokratie.

## 10. Bewusste Abgrenzung (gegen Over-Engineering)

Jetzt:
- PB als **abgeleitete View** (rein projiziert, ProjectState unveraendert; kein neuer Item-Typ).
- Nur ein leichtes `mvpRelevance`-Feld.

Spaeter / NICHT jetzt:
- Volle MVP-/MMF-Auswahl, User Story Map, Narrative Flow (PDF 03 Teil 2).
- Deliverable-/Ziel-Sicht, Stakeholder, Personas, Rich Picture (PDF 02).
- Persistierter PBI-Item-Typ + Versionierung. (PBI-*-IDs + IssuePlanner-Integration werden aber
  frueher relevant als hier zunaechst gedacht -> Detail + ID-Stabilitaets-Entscheidung in `plan-pb.md`.)
- Absolute Vollstaendigkeits-/Gap-Beweise (open world, nicht valide messbar).

Der Kern ist die Analyse-Phase. Schema/View/IDs sind Verwaltungs-Beiwerk und duerfen den
Kern nicht ueberwuchern.

## 11. Offene Fragen fuer die naechste Iteration

Durch die Kritik entschieden (nicht mehr offen):
- Assemble = **deterministische Kandidaten** (L4-LINK, gemeinsame Entitaeten, Text-Referenz,
  Traceability) **+ agentische Cluster-Pruefung**; Agent entscheidet core vs. crosscutting-Kontext;
  Gate: nichts Aktives verloren, Querschnitt mehrfach referenzierbar ohne Dup.
- **Ein** Clarification-Agent (nicht Interrogator+Proposer getrennt).
- Given/When/Then **nicht** verpflichtend fuer jeden Typ; Anspruch = testbar/ueberpruefbar.
- Prioritaet/MVP = Agent-**Vorschlag**, human-autorisiert.

Entschieden (verfeinert durch 2. Kritik - ersetzt die pauschale "ready_with_open_questions"-Regel):
- Readiness NICHT pauschal `ready_with_open_questions`, sondern nach SCOPE-RELEVANZ der offenen
  Frage relativ zum GESCHNITTENEN PBI:
    backlog_ready                    keine offenen Punkte im aktuellen Scope
    ready_with_nonblocking_questions offene Frage AUSSERHALB des PBI-Scopes
                                     (z.B. spaeteres Loeschen, waehrend PBI nur Anzeige+Hinzufuegen umfasst)
    blocked_by_decision              offene Frage trifft KERN des PBI (Verhalten/Daten/Rechte/Abnahme)
                                     (z.B. ungeklaerte Rollenfrage BEIM SPEICHERN) -> NICHT umsetzungsbereit
- Der PBI-CUT (5) entscheidet mit ueber blockierend/nicht-blockierend: sauber abgegrenzter Scope
  macht viele Fragen nicht-blockierend.

Entschieden:
- Benennung/CLI: **`l4-re-clarify`** (Ordner `l4/reclarify/`, Agent `L4ReClarifyAgent`,
  Praefix `ReClarify`). Details: `plan-build.md`.

Noch offen:
1. Readiness: umbauen auf Feature-Granularitaet, oder Feature-Einheit nur zusaetzlich einspeisen?
2. Erstauswahl der Kern-Features fuer den Durchstich (siehe 12) - welche genau?
3. PBI-Modell / IssuePlanner-Integration / ID-Anker -> siehe `plan-pb.md`.

## 12. Vorgeschlagene Umsetzungsreihenfolge (nach Freeze dieses Plans)

Nicht alle 69 Requirements auf einmal. Erster Durchstich = kleine, priorisierte
**Kern-Feature-Auswahl** verschiedener Problemtypen: Login · Rollen/Berechtigungen ·
About-Me · No-Go · Kommunikation.

```text
1 Assemble: deterministische Kandidaten (Traceability/LINK/Entitaet/Text) + agentische
            Cluster-Pruefung; Gate "nichts Aktives verloren" -> Feature-Einheiten (read-only) [klein]
2 Clarification-Agent (ein Knoten): Kern-Set 4.1 + Erdung erzwungen; Output = PB-View-Kandidat
            + clarification-report (analytische Zwischenschritte, ephemer)
3 Gate SCHLANK (strukturell, nicht per-Linse):
            Source-IDs existieren · Feature-Scope klar · >=1 pruefbares Fit Criterion ·
            erkannte offene Dimensionen explizit behandelt · Vorschlaege korrekt markiert (evidence/resolution) ·
            Rank/Prioritaetsklasse gesetzt (Vorschlag) ·
            SCOPE-CHECK: trifft eine offene Entscheidung den Kern (Verhalten/Daten/Rechte/Abnahme)
            des geschnittenen PBI -> blocked_by_decision (nicht ready_for_dev); sonst durchlassen
5 HumanReview-Adapter (bestehende UI) + optional 1 Judge fuer semantische Qualitaet + Apply -> PB-View
6 Readiness auf PB-View / Feature-Einheit
7 IssuePlanning-Input auf PB-View umstellen
```

Erfolgskriterien fuer den Durchstich (messbar):
- vorhandenes verteiltes Wissen gelangt vollstaendig in den Featurekontext (recovery),
- relevante bisher ungeklaerte Punkte werden sichtbar,
- neue Aussagen sauber als Vorschlag/Entscheidung markiert (keine stille Wahrheit),
- resultierende Issues mit deutlich weniger Entwickler-Rueckfragen umsetzbar.

GEGENMETRIKEN (aus Kritik - gegen "mehr Text/mehr Fragen != besser"):
- Rate irrelevanter / nicht scope-relevanter Fragen (soll niedrig),
- Ablehnungsquote vorgeschlagener Defaults im Human Review (kalibriert Default-Guete),
- Umfang menschlicher Korrekturen (soll ueber Iterationen sinken).

Jeder Schritt: Ziel / erwartete Beobachtung / Exit-Kriterium / Evidenzpfad (CLAUDE.md-Disziplin).
Belegfall fuer alle Tests: der No-Go-Trace (6 Requirements) aus Abschnitt 2.

## 13. Zielarchitektur (Minimalfassung)

```text
EIN Requirements-Clarification-Agent
  arbeitet auf priorisierten Feature-Einheiten (Assemble: det. Kandidaten + agentische Clusterpruefung)
  nutzt ein kleines stabiles Kern-Set (4.1); Volere/FURPS+ = Hintergrund-Heuristik im Prompt + Eval
  erzeugt testbare Fit Criteria (format-agnostisch) + explizite offene Entscheidungen (2 Achsen)
  -> schlankes deterministisches Gate (strukturell, nicht per-Linse)
  -> Human Review (+ optional 1 Judge fuer Semantik)
  -> Apply materialisiert Product-Backlog-View
  -> Readiness + IssuePlanning arbeiten auf dem PB-View
```

Nicht der Anspruch: alle theoretisch denkbaren Anforderungen finden oder jedes Feature voll
ausmodellieren. Der Anspruch: welche Infos fehlen fuer eindeutige Umsetzung, Abnahme und
Scope-Abgrenzung eines PRIORISIERTEN Features - und diese sichtbar/entscheidungsreif machen.