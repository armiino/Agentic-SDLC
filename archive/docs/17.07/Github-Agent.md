# GitHub Reconciliation Agent - Stand + Vergleichspfad (17.07/18.07)

Status: **v1 GEBAUT + VERGLEICHSPFAD OFFEN.** Der deterministische Pfad
(`github-reconciliation seed` -> Gate -> HumanReview -> kontrollierter Write) ist GEBAUT und bleibt die BASELINE.
Der agentische Pfad ist inzwischen ebenfalls als `github-reconciliation agent` gebaut:
`GithubReconciliationAgent[Tools] -> GithubActionPlanGate[det] -> Finalize`.
Offen ist nicht mehr der Grundbau, sondern **Abschnitt 8**: der messbare Vergleich agentisch vs.
deterministisch mit Drift-Fixtures, Gold-Labels, Scoring und Mehrfachläufen.

Commit-Anker des aktuellen eingefrorenen Stands:

```text
5e5cbe942a066f3b93b30d8c4c648c21a43d415d
```

Verwandt: `plan-pb.md` (§9 Autoritaet, ProductBacklogView), `plan-build.md` (View/Repository-Boundary),
`16.07/plan-repository-viewboundary.md` ("Alternative: direkter GitHub-MCP-Agent als Vergleichspfad").

---

## 0. Warum dieser Agent ueberhaupt wichtig ist

Der Ursprung dieses Dokuments ist ein Architektur-Dilemma:

```text
Wir koennen aus ProductBacklog/IssuePlan deterministisch GitHub-Issues erzeugen.
Aber die Forschungsfrage lautet nicht: "Wie automatisiere ich API-Calls?"
Sie lautet: "Wie kann man Agenten mit MAF in fruehen SDLC-Phasen sinnvoll einsetzen?"
```

Der GitHub-Teil ist deshalb nicht nur eine operative Schreibstrecke. Er ist der erste Ort, an dem ein
**echter Umwelt-Agent** seinen Mehrwert zeigen kann:

```text
Umwelt A: wachsender ProjectState / ProductBacklog / Traceability
Umwelt B: lebendes GitHub mit Issues, Kommentaren, Status, Labels, menschlichen Aenderungen
```

Bei einem ersten Transkript reicht ein deterministischer Pfad oft aus: PBIs werden zu Issues. Interessant wird
es beim **naechsten Transkript**: Der ProjectState waechst, alte Aussagen werden konkretisiert oder korrigiert,
GitHub-Issues existieren bereits, manche wurden manuell veraendert oder geschlossen. Dann braucht das System
eine Instanz, die exploriert und entscheidet:

- Gibt es zu diesem neuen/veraenderten PBI bereits ein Issue?
- Wurde das Thema im ProjectState schon einmal behandelt?
- Ist ein bestehendes Issue fachlich dasselbe, nur anders formuliert?
- Muss ein Issue aktualisiert, verlinkt, wieder geoeffnet, geschlossen oder neu erstellt werden?
- Ist ein geschlossenes Issue wirklich erledigt, oder widerspricht der ProjectState?
- Gibt es mehrere Issues zu einem PBI oder mehrere PBIs zu einem operativen GitHub-Thema?

Genau hier soll MAF sichtbar werden: nicht als zufaelliger Prompt um einen API-Call, sondern als
Agent/Workflow-System mit Tools, kontrollierter Umwelt, Gedächtnis ueber IDs, Plan-Erzeugung, Gate,
HumanReview und separatem Write. Determinismus bleibt bewusst dort, wo Garantien zaehlen; der Agent sitzt
dort, wo semantische Exploration, Abgleich und Entscheidung ueber zwei Umwelten noetig sind.

Wichtig: Die aktuelle v1 (`github-reconciliation agent`) ist ein erster plan-only Agent auf
AcceptedIssuePlan + lokalen Snapshots/Mappings. Sie ist **noch nicht die volle Zielausbaustufe**. Die volle
Zielausbaustufe gibt dem Agenten direkte ProjectState-/PBI-/Traceability-Tools und GitHub-MCP-Read-Tools.
Der Write bleibt trotzdem gated.

> **Abgrenzung — Identitäts-Resolution ist UPSTREAM, nicht hier (`plan-core-ingestion.md`).**
> Die Frage "wurde das Thema im ProjectState schon behandelt?" gehört an den **Ingress** jedes neuen Meetings
> (Auflösung der eingehenden Requirements gegen den lebenden Core mit Index) — nicht an den GitHub-Agenten am
> Ende. Er bekommt daher ein **bereits aufgelöstes Core-Delta** (added/changed/unchanged) + persistiertes
> `pbi↔issue`-Mapping und macht nur die **operative** Reconciliation gegen GitHubs messy Realität (menschliche
> Issues, geschlossen-aber-nicht-erfüllt, Titel-Paraphrasen). Er sucht NICHT die ganze Wahrheit ab — sonst
> jagt man ihn in einem Jahr über den ganzen Bestand. Die zwei "gleiche Sache, andere Worte"-Fragen bleiben
> beim Agenten nur noch auf der **GitHub-Seite** (Issue-Titel-Drift), nicht auf der Requirements-Seite.

## 1. Forschungs-Einordnung (warum das KEIN Widerspruch ist)

Die Thesis-Frage ist nicht "wie agentisch kann das System werden?", sondern
**"welche agentischen Mechanismen sind nuetzlich, noetig oder Overengineering?"**.

- Deterministisch dort, wo es mechanisch + sicherheitskritisch ist (Coverage-Gate, DoR-Gate, PBI->Issue-
  Projektion, Write-Governance) = die *ehrliche* Antwort "hier reicht/gewinnt Determinismus".
- Agentisch dort, wo Bedeutung, Ambiguitaet und Kontext regieren (Cluster, Clarify, **GitHub-Reconciliation**)
  = "hier verdient ein Agent seinen Platz".

Genau diese **Unterscheidung** ist der Beitrag. Der GitHub-Reconciliation-Agent ist ein STARKER
"echter Agent"-Kandidat, weil er ueber ZWEI lebende, messy Umwelten mit Drift und Ambiguitaet arbeitet -
dort brechen deterministische Regeln.

## 2. Die zwei Umwelten des Agenten

```text
Zielbild:
Umwelt A: Fachliche Wahrheit  = ProjectState / ProductBacklogView / Traceability / Mappings
                                (Repository-/View-Port; spaeter DB)
Umwelt B: Operative Ebene     = GitHub-Issues/-Kommentare/-Labels
                                (GitHub-MCP read; Write nur gated)

v1 heute:
Umwelt A: AcceptedIssuePlan + aus PBI mitgefuehrte IDs/Metadata
Umwelt B: read-only GitHub-Snapshot + lokales Mapping-Artefakt
```

Der Agent SIEHT beide gleichzeitig und denkt sie zusammen - das ist die Kraft, die ein 1:1-Token-Matcher
nicht hat. Beide sind kontrollierte Read-Surfaces. In v1 liest der Agent lokale Snapshots/Mappings und schreibt
nur einen Plan; der externe GitHub-Write bleibt der separate, gated Apply-Schritt.

## 3. Was der Agent tut (staerker als der deterministische Matcher)

- **Semantisches Mapping:** erkennt "GitHub-Issue #27 IST PBI-FC006-02", auch bei anderem Wortlaut,
  Umbenennung, Split/Merge. (Deterministisch: Token-/ID-Overlap verpasst Paraphrasen.)
- **Drift-Analyse ueber beide Welten:** geschlossenes Issue, dessen Requirement im PB noch offen ist;
  Issue ohne PB-Pendant (Waise); mehrere Issues -> ein PBI (Konsolidierung); PBI ohne Issue (Luecke).
- **Messy real world:** menschlich angelegte Issues, Kommentare, Label-Konventionen, Teilueberlappungen.
- **Vorschlag eines Reconciliation-Plans** mit Aktionen: CREATE | UPDATE | CLOSE | LINK | NO_CHANGE |
  CONSOLIDATE | SPLIT | FLAG_DRIFT - je mit Beleg (welches PBI/Requirement, welches Issue, warum).

## 4. "Voraus gedacht" - was das konkret heisst

Der Wert liegt nicht nur im Abgleich, sondern in Antizipation:

- **Sprint-Delta:** neues Transkript -> PB-Delta -> welche Issues neu/update/close, ohne Duplikate.
- **Lifecycle-Ehrlichkeit:** "geschlossenes Issue != Requirement erfuellt" -> Agent flaggt Divergenz.
- **Reihenfolge/Blocker:** "diese offene Stakeholder-Entscheidung blockiert 4 Issues -> zuerst klaeren".
- **Konsolidierung/Struktur:** viele kleine Issues -> ein Epic; oder zu grosses Issue -> Split-Vorschlag.
- **Dependencies:** PBI-Dependencies -> Issue-Reihenfolge/Verlinkung.

## 5. Position im Muster (Sicherheit - der entscheidende Guardrail)

```text
Agent (AcceptedIssuePlan + Snapshot/Mapping read) = MAKER   -> schlaegt Reconciliation-Plan vor
Deterministisches Gate                            = CHECKER -> Struktur/Beleg/Coverage/Policy
HumanReview                                       = CHECKER -> autorisiert
Kontrollierter Write (schmaler Client)            = APPLY
```

**KEIN autonomer Write durch den Agenten.** Gruende:
- Write ist extern + kaum reversibel - der gefaehrlichste Schritt.
- LLM-Varianz (real beobachtet, z.B. ReviewAgent approve/revise schwankte) darf nicht auf
  "lege Issue an / schliesse Issue" sitzen.
- Autoritaetsrichtung bleibt **PB/State -> GitHub**; ein geschlossenes Issue aendert NIE still die Wahrheit.

Die Analyse-Kraft des Agenten kommt voll zum Tragen (Vorschlag), die Reproduzierbarkeit/Sicherheit
bleibt (Gate + Human + gated Write).

## 6. Tools / aktueller Implementierungsstand

```text
Gebaut in v1:
AcceptedIssuePlan:   list_accepted_issue_plan_items, get_issue_plan_item
GitHub Snapshot:     search_existing_issues, find_candidate_issue_matches
Mapping:             list_existing_mappings
Seed/Baseline:       get_seed_github_action_plan
Plan:                check_github_action_plan (det. Vorpass), save_github_action_plan (einmal)

Optional spaeter:
GitHub-MCP read:     list_issues, get_issue, search_issues, get_comments, get_labels
GitHub-MCP write:    create/update/close -> weiterhin NUR ueber gated Apply, nicht im Maker frei
PB/State direkt:     list_pbis, get_pbi, get_pbi_traceability, get_requirement
```

Beleg-Pflicht als Anti-Halluzination: jede MATCH/UPDATE/CLOSE-Aktion muss den konkreten Anker nennen
(pbiId + requirementIds im Issue-Body, oder ein bestehendes Mapping) - kein Match "aus dem Bauch".

## 7. Autoritaet / Regeln

- ProjectState/ProductBacklogView = fachliche Wahrheit. GitHub = operative Projektion.
- geschlossenes/verwaistes Issue -> FLAG, nicht stille Wahrheits-Aenderung.
- Write nur nach Gate + HumanReview. Read breit ok; Write ausschliesslich ueber den kontrollierten Apply.
- Mapping `pbiId <-> issueNumber` existiert aktuell als Run-Artefakt (`github-mappings-created.json`).
  Fuer echten Delta-Betrieb muss es an einen stabil adressierbaren JSON-/ProjectState-Ort gehoben werden
  (Basis fuer den naechsten Delta-Lauf; plan-pb Flag A).

## 8. Vergleichspfad — konkretes Experiment (die eigentliche Forschungsaussage)

Ziel: "Agent vs. deterministisch bei GitHub-Reconciliation" EMPIRISCH beantworten, nicht nur argumentativ.
Beide Pfade erzeugen dasselbe Ausgabeformat (`GithubActionPlanDocument`) -> direkt vergleichbar.

### 8.1 Testbett (bereits real vorhanden)
`armiino/Agentic-GitHub` enthaelt jetzt 26 Issues (#40-#65) + `github-mappings-created.json`
(IPLAN/pbiId <-> issueNumber). Damit ist Reconciliation NICHT-trivial: ein erneuter Lauf desselben (oder
leicht geaenderten) Backlogs MUSS die vorhandenen Issues erkennen (NO_CHANGE/UPDATE) statt 26 Duplikate zu
erzeugen. Genau das Dedup-Szenario, in dem sich Agent vs. Scorer zeigt.

### 8.2 Kontrollierte DRIFT-Fixtures (der eigentliche Haertetest)
Bewusst Drift erzeugen, den ein Token-/ID-Matcher schwer faengt, ein semantischer Agent aber schon:
- einige Issues UMBENENNEN (Titel-Paraphrase, gleiche Sache) -> Titel-/Wort-Overlap sinkt.
- Requirement-IDs aus 1-2 Issue-Bodies ENTFERNEN -> der deterministische +6-ID-Anker faellt weg.
- ein Issue SCHLIESSEN, dessen Requirement offen bleibt -> Divergenz/Drift.
- ein neues, echt anderes Issue -> darf NICHT als Match gelten (Precision-Falle).

### 8.3 Zwei Pfade, gleicher Input
- **Deterministisch (Baseline, gebaut):** `github-reconciliation seed <plan> --issues <snapshot>` -> Plan_det.
- **Agentisch (gebaut, im Experiment zu fahren):** `github-reconciliation agent` liest AcceptedIssuePlan +
  Snapshot/Mappings -> Plan_agent (gleiches Schema).
- Beide auf DEMSELBEN accepted-issue-plan + DEMSELBEN Repo-Snapshot (Fairness).

### 8.4 Gold-Standard + Metriken
Mensch labelt je geplantem Issue die WAHRE Aktion gegen den Snapshot (CREATE/UPDATE/NO_CHANGE/LINK + Ziel-Issue).
Beide Plaene gegen Gold scoren:
- **Dedup-Recall:** vorhandene Issues korrekt erkannt (NO_CHANGE/UPDATE statt CREATE) - v.a. unter Drift.
- **Precision:** falsche Matches (sagt "gleich", ist aber neu) -> Fehlzuordnung.
- **Varianz:** agentischen Lauf N>=3x wiederholen -> Stabilitaet des Plans (Mess-Hygiene wie L3).
- **Kosten/Latenz** und **Human-Korrektur-Umfang** im Review.

### 8.5 Erwartete Aussage (Hypothese, zu belegen - NICHT vorwegnehmen)
- Ohne Drift / mit sauberen ID-Ankern: deterministisch >= agentisch (Agent = Overkill, teurer/variabel).
- Mit Drift (Paraphrase, fehlende IDs, Konsolidierung): agentisch faengt Matches, die deterministisch verpasst
  -> hoeherer Dedup-Recall, aber Precision/Varianz beobachten. -> begruendet den **Hybrid** (§9).
- Ergebnis ist die belegte Antwort auf "wo verdient der Agent seinen Platz" - egal in welche Richtung es faellt.

### 8.6 Was ist gebaut, was fehlt noch?

Gebaut:
- **`github-reconciliation agent`**: MAF-Workflow mit `GithubReconciliationAgent[Tools]` ->
  `GithubActionPlanGate[det]` -> `Finalize`.
- **Agent-Tools** fuer AcceptedIssuePlan, lokale Mappings, read-only GitHub-Snapshots, deterministische
  Kandidatensuche, Seed-Plan, Gate-Vorpass und einmaliges Speichern.
- **Gemeinsames Ausgabeformat** (`GithubActionPlanDocument`) fuer Seed und Agent.
- **Gated Write-Pfad** bleibt getrennt (`github-reconciliation-review` -> `github-reconciliation-apply` ->
  `github-write dry-run/execute`).

Noch offen:
1. **Harness `github-reconcile-compare <plan> --snapshot <s> [--gold <g>]`:** faehrt beide Pfade, schreibt
   Plan_det + Plan_agent + Vergleichs-Report (Dedup-Recall/Precision/Varianz/Kosten).
2. **Drift-Fixtures (8.2)** reproduzierbar erzeugen oder dokumentieren.
3. **Gold-Standard-Datei** fuer wahre Matches/Aktionen gegen den Snapshot.
4. **N>=3 Agent-Laeufe** fuer Varianz und Stabilitaet.
5. Optional danach: GitHub-MCP-read statt Snapshot-read vergleichen; Write bleibt gated.

## 9. Empfohlene Hybrid-Architektur (nuetzlich statt Overengineering)

```text
1. Deterministischer Vorfilter: eindeutige ID-/Mapping-Matches sofort aufloesen (billig, sicher).
2. Agent nur fuer den REST: Ambiguitaet, Drift, Waisen, Konsolidierung, Antizipation.
3. Gemeinsamer Reconciliation-Plan -> ein Gate -> HumanReview -> kontrollierter Write.
```

So arbeitet der Agent genau dort, wo er Mehrwert hat, ohne den einfachen Fall zu verteuern/verrauschen.

## 10. Risiken / offene Fragen

- LLM-Varianz an operativen Entscheidungen -> deshalb gated + deterministischer Vorfilter.
- Halluzinierte Matches -> Beleg-Pflicht (§6) + Gate + Human.
- MCP-Dependency (Auth-Token, Prozess, Rate-Limits) vs. schmaler Direktclient (Wartung) - MAF-vs-custom abwaegen.
- "Voraus denken" darf nicht in Scope-Creep kippen (Agent plant Produkt statt nur zu reconcilen) -> enger Auftrag.

## 11. Scope / naechster Schritt

Der agentische Pfad + die Messung (§8) sind der naechste konkrete Bau-Schritt (nach Commit des re-clarify-
Strangs und angestossener PB/RE-Kalibrierung). Reihenfolge:
1. **Drift-Fixtures** auf `armiino/Agentic-GitHub` (§8.2) - reproduzierbar, read + gezielte Aenderungen.
2. **Gold-Labels** fuer den Snapshot erstellen.
3. **`github-reconcile-compare`**-Harness (§8.6) - beide Pfade + deterministischer Scorer + Report.
4. **Lauf + Report + Interpretation** -> belegte Aussage "Agent vs. deterministisch, wo lohnt es".
- Begleitend: MAF-vs-custom-Eintrag "GitHub-Read: Snapshot/Client vs. MCP-Agent-Tools; Write: gated Client".

Dieser Vergleich ist die *empirische* Antwort auf "warum nicht einfach Agenten?": nicht behaupten, sondern
an einem echten, driftbaren Testbett messen, wo Agent-Power Mehrwert bringt und wo Determinismus reicht.

---

## 12. Neuer Ankerplan: ProjectState Steward + Delta vor GitHub

Status: **Diskussionsanker, noch nicht entschieden/gebaut.** Dieser Abschnitt haelt die naechste
Architekturfrage fest, bevor wir den GitHub-Agenten weiter ausbauen:

```text
Wer pflegt den wachsenden ProjectState, wenn neue Transkripte oder GitHub-Aenderungen eintreffen?
```

Vorlaeufige Antwort:

```text
Nicht der GitHub-Agent.
```

Der GitHub-Agent soll die zwei Umwelten explorieren und Reconciliation-/Operationsvorschlaege machen.
Die fachliche Wahrheit im ProjectState soll eine eigene Instanz pflegen: ein **ProjectState Steward**.

### 12.1 Zielbild

```text
Neues Sprint-/Meeting-Transkript
  -> Ledger
  -> L1/L2/L3/L4
  -> ProductBacklog-/Requirement-Delta
  -> ProjectState Steward
       -> prueft, versioniert, verlinkt, materialisiert State-Operationen
       -> erzeugt scoped Views fuer Downstream-Agenten
  -> GitHub-Agent
       -> gleicht Delta/View mit GitHub ab
       -> erzeugt GitHubActionPlan
  -> Gate
  -> HumanReview
  -> GitHub Write
```

Von GitHub kommend:

```text
GitHub Snapshot / Kommentar / neues Issue / Statusaenderung
  -> GitHub-Agent
       -> erkennt operative Bedeutung und Drift
       -> erzeugt StateChangeProposal oder GithubActionPlan
  -> ProjectState Steward
       -> prueft fachliche Autoritaet, Provenienz, Ziel-IDs, Statusregeln
       -> schreibt ProjectState nur nach Gate/HumanReview
```

Damit bleibt die Autoritaet getrennt:

```text
ProjectState Steward = Besitzer der fachlichen Projektwahrheit
GitHub-Agent          = Umwelt-/Reconciliation-Agent zwischen State-View und GitHub
GitHub Write          = kontrollierter externer Apply
```

### 12.2 Warum diese Trennung wichtig ist

Der GitHub-Agent darf GitHub und ProjectState explorieren, aber er sollte den State nicht direkt pflegen.
Sonst vermischen sich operative Signale mit fachlicher Wahrheit:

- Ein GitHub-Kommentar ist nicht automatisch ein Requirement.
- Ein geschlossenes Issue ist nicht automatisch fachlich erledigt.
- Ein manuell angelegtes GitHub-Issue ist nicht automatisch ProjectScope.
- Ein Statuswechsel in GitHub darf nicht still Ledger/Requirements/PBIs umschreiben.

Der GitHub-Agent soll deshalb Vorschlaege liefern:

```text
LINK_ISSUE_TO_PBI
UPDATE_ISSUE_FROM_PBI
CREATE_ISSUE_FOR_PBI
REOPEN_ISSUE
FLAG_GITHUB_DRIFT
PROPOSE_STATE_CHANGE_FROM_GITHUB
```

Der ProjectState Steward verarbeitet fachliche State-Operationen:

```text
ADD_ITEM
ADD_RELATION
ADD_PROVENANCE
MARK_SUPERSEDED
MARK_RESOLVED
MARK_DONE
REOPEN_DECISION
VERSION_PBI
REGISTER_GITHUB_MAPPING
```

### 12.3 Wie der State wachsen sollte

Grundregel:

```text
Archiv/Evidenz waechst append-only.
Aktive Views werden versioniert/projiziert.
```

Ledger-Claims, alte Run-Artefakte und historische Requirements werden nicht geloescht. Wenn ein neues
Transkript etwas korrigiert oder konkretisiert, entsteht keine stille Ueberschreibung, sondern eine neue
Operation mit Provenienz:

```text
REQ-12 bleibt im Archiv.
REQ-45 konkretisiert/ersetzt REQ-12.
Relation: REQ-45 supersedes REQ-12
REQ-12 status = superseded
REQ-45 sourceClaimIds += neuer Claim
HumanDecision dokumentiert die fachliche Autorisierung.
```

Damit gibt es zwei Ebenen:

```text
Historischer State / Archiv:
  alles, was je belegt, abgeleitet, vorgeschlagen oder entschieden wurde

Aktiver ProjectState View:
  current_baseline, open_items, changed_items, active_pbis, github_reconciliation_view
```

Der Agent soll normalerweise **nicht den ganzen historischen State inhalieren**. Er bekommt eine scoped View
plus Such-/Lookup-Tools:

```text
list_changed_pbis()
list_open_requirements()
list_existing_github_mappings()
search_project_archive(query)
get_traceability(id)
get_related_state_items(id)
```

Nur auf expliziten Auftrag oder bei unklarem Match wird das Archiv exploriert.

### 12.4 Neuer State oder wachsender State?

Offene Designentscheidung, aber aktuelle Tendenz:

```text
Ein wachsender ProjectState mit Versionen/Snapshots, nicht jedes Mal ein isolierter neuer State.
```

Praktisch bedeutet das:

```text
project-state.json bleibt/entwickelt sich als Event-/Snapshot-nahe Wahrheit.
Jeder Lauf erzeugt zunaechst Proposal-/Delta-Artefakte.
Der ProjectState Steward materialisiert akzeptierte Operationen in einen neuen State-Snapshot.
Der alte Snapshot bleibt als Historie referenzierbar.
```

Also eher:

```text
project-state-v1.json
  + delta-run-20260720
  + accepted-state-operations
  -> project-state-v2.json
```

nicht:

```text
neues Transkript -> komplett unabhaengiger project-state.json ohne Beziehung zum alten
```

Und auch nicht:

```text
eine endlos wachsende flache Liste ohne Status/Relation/Version
```

Die Liste darf wachsen, aber nur mit klaren Relationen und aktiven Views:

```text
neu
konkretisiert
ersetzt
widerspricht
dupliziert
implementiert
erledigt
deferred
out_of_scope
```

### 12.5 Delta-Ablauf fuer ein neues Transkript

Vorschlag fuer den spaeteren Ablauf:

```text
1. Neues Transkript -> Ledger Delta
   Neue Claims bekommen neue Claim-IDs. Alte Claims bleiben unveraendert.

2. L1/L2/L3 erzeugen Candidate-/Proposal-Artefakte
   Neue REQ-/ARCH-/L3-Kandidaten werden nicht blind in die aktive Wahrheit geschrieben.

3. State Impact / Steward-Vorpass
   Bestehenden State durchsuchen:
   - ist das neu?
   - konkretisiert es ein bestehendes Item?
   - widerspricht es?
   - ist es nur Wiederholung?
   - betrifft es einen erledigten PBI/ein geschlossenes Issue?

4. StateChangePlan
   Der Output ist ein Operationsplan, nicht direkt ein neuer State:
   ADD_ITEM, UPDATE_ITEM, SUPERSEDE, LINK, MARK_DONE, REOPEN, NO_CHANGE.

5. Gate + HumanReview
   Deterministische Checks: IDs, Quellen, keine stillen Deletes, Gueltigkeit der Relationen,
   Statusuebergaenge, Provenienz.

6. Apply
   Erzeugt neuen ProjectState-Snapshot plus Views:
   current_baseline, changed_items, active_pbis, github_reconciliation_view.

7. GitHub-Agent
   Bekommt primär changed_items/active_pbis + Mappings + GitHub-Umwelt.
   Er entscheidet operative GitHub-Aktionen, nicht fachliche State-Wahrheit.
```

### 12.6 GitHub-Aenderungen zurueck in den State

Auch GitHub-Input sollte nicht direkt den ProjectState veraendern:

```text
GitHub-Agent liest Issue/Kommentar/Status
  -> erkennt moegliche Bedeutung
  -> erzeugt StateChangeProposal
  -> ProjectState Steward prueft
  -> HumanReview falls fachliche Autoritaet betroffen ist
  -> Apply schreibt Provenienz + Relation/Status
```

Beispiele:

```text
GitHub-Issue geschlossen
  -> nicht automatisch PBI done
  -> Vorschlag: MARK_IMPLEMENTED oder FLAG_REVIEW_REQUIRED

GitHub-Kommentar enthaelt neue fachliche Aussage
  -> nicht automatisch Requirement
  -> Vorschlag: ADD_CLAIM_FROM_GITHUB_COMMENT + HumanReview

GitHub-Issue manuell erstellt
  -> nicht automatisch Scope
  -> Vorschlag: LINK_TO_EXISTING oder PROPOSE_NEW_BACKLOG_ITEM
```

### 12.7 Was vor einer DB geklaert werden muss

Eine DB ist erst sinnvoll, wenn diese State-Operationen stabil genug sind. Vorher reicht JSON, aber JSON muss
sauberer werden:

```text
1. StateOperation-Modell definieren.
2. StateChangePlan + Gate + HumanReview + Apply bauen.
3. PBI-IdentityKey -> stabile pbiId materialisieren.
4. GitHubMappingRegistry an stabilen JSON-/ProjectState-Ort heben.
5. Scoped Views definieren:
   current_delta_view
   github_reconciliation_view
   active_product_backlog_view
   archive_lookup_view
6. ProjectScope von Pfadheuristik in Richtung fachlicher Scope entwickeln.
```

Erst danach DB:

```text
DbProjectStateRepository
DbProjectStateViewRepository
Mapping/Relation/Provenance-Tabellen
Query statt Pfad-Resolver
```

### 12.8 Offene Fragen fuer die naechste Diskussion

- Ist der ProjectState eher Event-Log + Snapshot oder nur Snapshot mit History-Feldern?
- Welche Statuswerte brauchen Requirements/PBIs wirklich (`active`, `superseded`, `done`, `implemented`,
  `deferred`, `out_of_scope`, `needs_review`)?
- Wer darf `done` setzen: GitHub-Status, HumanReview, Testnachweis, explizite Product-Entscheidung?
- Soll GitHub-Kommentar-Evidenz in den Ledger aufgenommen werden oder als eigene `origin=github` Provenienz?
- Wie klein muss die `github_reconciliation_view` sein, damit der Agent nicht den ganzen State laden muss?
- Brauchen wir zuerst einen deterministischen StateChangePlan-Seed, bevor ein State Impact Agent kommt?
- Reicht fuer Sprint 2 eine JSON-Registry, oder brauchen wir schon einen echten append-only OperationStore?

---

## 13. Initialer Plan: ProjectState als zentrale Instanz

Status: **Initialer Funktionsplan, Diskussionsgrundlage.** Dieses Kapitel beschreibt, wie der ProjectState
kuenftig verstanden werden sollte, was wir aktuell haben, was fuer den Zielprozess fehlt und wie neue
Transkripte/GitHub-Aenderungen funktional verarbeitet werden sollen. Es beschreibt bewusst noch keine
Code-Klassen oder Tabellen. Die Logik soll lokal mit JSON funktionieren und spaeter hinter derselben
fachlichen Schnittstelle in eine DB migrierbar sein.

### 13.1 Aktueller Stand: was `project-state.json` heute wirklich ist

Heute ist `project-state.json` ein wichtiger, aber engerer Zustand:

```text
L1/L2 Artefakte
  + akzeptierte L3 Promotions
  -> project-state.json
```

Er enthaelt im Kern:

```text
Items        REQ-*, ARCH-*, L3-REQ-* usw.
Relations    evidenced_by, derived_from, accepted_by usw.
Provenance   Run, Artifact, Claim, Candidate, HumanDecision
Proposals    vor allem L3-Kandidaten/Entscheidungsstatus
Sources      verwendete Artefakte/Runs
```

Das ist stabil und wertvoll fuer:

```text
Traceability bis Claim/Transkript
L4-Input
Auditierbarkeit der L1/L2/L3-Ebene
Repository-/View-Boundary als DB-later-Vorbereitung
```

Aber es ist heute noch **nicht** der vollstaendige Projektzustand:

```text
Canonical Requirements sind nachgelagerte L4-Projektionen.
ProductBacklog/PBIs sind Run-Artefakte, nicht State-Entitaeten.
GitHub-Mappings sind Run-Artefakte, nicht State-Entitaeten.
Lifecycle-Status wie active/superseded/done ist noch nicht sauber ueber alle Ebenen modelliert.
Delta-Operationen zwischen altem und neuem Meeting sind noch nicht als eigener Prozess vorhanden.
```

Arbeitsbegriff:

```text
Heute: project-state.json = Evidence-/Requirement-State-Snapshot bis L3
Ziel:  ProjectState       = vollstaendige, versionierte Projektwahrheit
```

### 13.2 Zielbild: was der ProjectState werden muss

Der ProjectState soll die zentrale fachliche Wahrheit werden. Nicht jede View ist gleich Wahrheit,
aber jede relevante Wahrheit muss dort adressierbar, versioniert und rueckverfolgbar sein.

Zielinhalt:

```text
Evidence
  Ledger-Claims, Transkript-Referenzen, GitHub-Kommentar-Evidenz (falls akzeptiert)

Requirements / Architecture
  REQ-*, ARCH-*, L3-REQ-*, CAN-REQ-*, Status, Versionen, Herkunft

Product / Planning
  PBIs, identityKey, version, Scope, MVP, Priority, Readiness, Open Decisions

Decisions
  HumanReview-Entscheidungen, offene Entscheidungen, geloeste Entscheidungen

Operational Links
  IssuePlanItems, GitHub-Issue-Mappings, GitHub-Statussignale

Relations
  derived_from, refines, supersedes, duplicates, contradicts, blocks, implements,
  belongs_to_feature, maps_to_github

Provenance
  Claim, Run, Artifact, Candidate, HumanDecision, GitHub-Issue/-Kommentar

Snapshots / Views
  current state, previous states, changed items, active backlog, GitHub reconciliation view
```

Der Product Backlog ist damit kein zweiter Wahrheitsort neben dem ProjectState, sondern eine View/Teilmenge:

```text
ProductBacklogView = aktive, priorisierbare Produkt-/Klaerungssicht aus dem ProjectState
```

GitHub bleibt ebenfalls keine fachliche Wahrheit, sondern eine operative Aussenwelt:

```text
GitHub = operative Projektion / Ausfuehrungsumwelt
ProjectState = fachliche Wahrheit
Mapping = explizite Relation zwischen beiden
```

### 13.3 Zentrale Instanz: ProjectState Steward

Der ProjectState soll nicht von beliebigen Agenten direkt veraendert werden. Dafuer braucht es eine
fachliche Instanz:

```text
ProjectState Steward
```

Funktionale Verantwortung:

```text
1. Neue Analyseoutputs gegen bestehenden State einordnen.
2. StateChangePlans erzeugen oder pruefen.
3. IDs, Relationen, Status und Provenienz konsistent halten.
4. Akzeptierte Operationen deterministisch materialisieren.
5. Scoped Views fuer L4, ProductBacklog und GitHub-Agenten bereitstellen.
6. Archiv und Current View trennen.
```

Der Steward ist nicht zwingend ein einzelner LLM-Agent. Sinnvoller ist ein Hybrid:

```text
deterministischer Seed / Matcher
  -> optional StateImpactAgent fuer semantisch schwierige Faelle
  -> deterministisches Gate
  -> HumanReview
  -> deterministischer Apply
```

Damit bleibt das bestehende Muster erhalten:

```text
Agent/Tools wo Bedeutung noetig ist
Determinismus wo Garantien noetig sind
HumanReview wo fachliche Autoritaet noetig ist
```

### 13.4 Wie ein neues Transkript funktional verarbeitet wird

Ein neues Meeting soll die bestehenden Phasen weiter nutzen. Die Levels muessen nicht neu erfunden werden.
Geaendert wird, wie deren Outputs in den bestehenden State eingeordnet werden.

Vorgeschlagener Ablauf:

```text
1. Neues Transkript kommt rein.

2. Ledger Delta entsteht.
   Neue Claims bekommen neue IDs. Alte Claims bleiben unveraendert.

3. L1/L2/L3 laufen auf dem neuen Material.
   Ergebnis sind neue Artefakte/Kandidaten/Promotions aus diesem Meeting.

4. Diese Outputs werden NICHT blind zur aktiven Wahrheit.
   Sie bilden einen Delta-Input fuer den ProjectState Steward.

5. Steward vergleicht Delta gegen aktuellen ProjectState.
   Fragen:
   - ist das neu?
   - konkretisiert es etwas?
   - widerspricht es etwas?
   - ist es eine Wiederholung?
   - betrifft es ein erledigtes/geschlossenes Thema?
   - betrifft es bestehende PBIs oder offene Entscheidungen?

6. Steward erzeugt einen StateChangePlan.
   Operationen:
   ADD_ITEM, LINK_TO_EXISTING, REFINE_ITEM, SUPERSEDE_ITEM, CONTRADICT_ITEM,
   MARK_DUPLICATE, REOPEN_DECISION, VERSION_PBI, NO_CHANGE.

7. Gate + HumanReview.
   Keine stillen Deletes, keine ungueltigen IDs, keine unbelegte fachliche Wahrheit.

8. Apply erzeugt neuen ProjectState-Snapshot.
   Der alte Snapshot bleibt erhalten.

9. L4/Re-Clarify laeuft auf einer Current/Affected View des neuen State.

10. GitHub-Agent bekommt eine GitHub-Reconciliation-View, nicht den ganzen State.
```

Wichtig: L4 sollte im Delta-Betrieb nicht isoliert nur das neue Meeting sehen. L4 braucht den aktuellen
State-Kontext, sonst entstehen doppelte Requirements/PBIs oder falsche Schnitte.

### 13.5 Was L4 im Delta-Betrieb konsumieren sollte

Heute konsumiert L4 den ProjectState-Snapshot. Im Zielbild konsumiert L4 eine fachliche View aus dem
aktuellen State:

```text
Initiallauf:
  L4 konsumiert current_baseline_view aus project-state-v1

Delta-Lauf:
  L4/Re-Clarify konsumiert affected_items_view aus project-state-v2
  plus relevante bestehende Kontexte
```

Diese View enthaelt nicht den ganzen Archivbestand, sondern:

```text
neue/geaenderte Requirements
direkt verwandte bestehende Requirements
relevante Canonical Requirements
betroffene PBIs
offene Entscheidungen
relevante Status/Relationen
```

Bei Bedarf kann ein Agent ueber Tools ins Archiv schauen:

```text
search_project_archive(query)
get_traceability(id)
get_related_items(id)
get_previous_versions(id)
```

Beispiel Meeting 20:

```text
Neue Aussage: "No-Go-Eintraege sollen archiviert werden."

Steward/View findet:
  Feature no-go
  CAN-REQ-027/028/029
  bestehende PBI-NOGO-*
  offene Lifecycle-/Archivierungsentscheidung
  GitHub-Mapping zu bestehenden Issues

Moegliche StateChangePlan-Operation:
  ADD_ITEM neues Requirement
  LINK/REFINE bestehendes No-Go Requirement
  RESOLVE oder UPDATE offene Archivierungsentscheidung
  VERSION_PBI oder ADD_PBI fuer Archivierungs-Slice
```

### 13.6 Wie der State wachsen sollte

Der State sollte weder als isolierte Neudatei pro Meeting noch als rohe Endlosliste verstanden werden.
Ziel ist:

```text
Append-only Historie + versionierte Current Snapshots + aktive Views
```

Funktional:

```text
project-state-v1
  + delta meeting 2
  + accepted StateChangePlan
  -> project-state-v2

project-state-v2
  + delta meeting 3
  + accepted StateChangePlan
  -> project-state-v3
```

Alte Items bleiben referenzierbar:

```text
REQ-12 bleibt im Archiv.
REQ-45 refines/supersedes REQ-12.
REQ-12 ist nicht geloescht, sondern nicht mehr active oder wurde superseded.
```

Aktive Views bestimmen, was normale Agenten sehen:

```text
current_baseline_view
affected_items_view
active_product_backlog_view
github_reconciliation_view
archive_lookup_view
```

### 13.7 Wie GitHub-Aenderungen in den State zurueckfliessen

Auch GitHub-Aenderungen sollen nicht direkt den State veraendern. Der GitHub-Agent erkennt operative Bedeutung
und erzeugt Vorschlaege.

```text
GitHub Snapshot / MCP Event / Kommentar
  -> GitHub-Agent analysiert
  -> StateChangeProposal oder GithubActionPlan
  -> ProjectState Steward prueft fachliche Bedeutung
  -> Gate/HumanReview
  -> Apply in ProjectState
```

Beispiele:

```text
GitHub-Issue geschlossen
  -> Vorschlag: MARK_IMPLEMENTED oder FLAG_REVIEW_REQUIRED
  -> nicht automatisch PBI done

GitHub-Kommentar enthaelt neue fachliche Aussage
  -> Vorschlag: ADD_EVIDENCE_FROM_GITHUB_COMMENT
  -> HumanReview entscheidet, ob daraus ein Requirement/OpenDecision wird

GitHub-Issue manuell erstellt
  -> Vorschlag: LINK_TO_EXISTING_PBI oder PROPOSE_NEW_BACKLOG_ITEM
  -> nicht automatisch Scope
```

### 13.8 Lokale JSON-Phase und spaetere DB

Die Logik soll nicht davon abhaengen, ob die Persistenz lokal JSON oder spaeter DB ist.

Jetzt lokal:

```text
ProjectState-Snapshots als JSON
StateChangePlans als JSON
StateChangeApply-Reports als JSON
GitHubMappingRegistry als JSON
Views durch Repository/View-Resolver
```

Spaeter DB:

```text
gleiche fachliche Operationen
gleiche Views
gleiche Gate-Regeln
andere Persistenz hinter Repository/View-Ports
```

Deshalb sollte vor der DB nicht zuerst ein Tabellenmodell, sondern das funktionale State-Modell stabil werden:

```text
Welche Entitaeten gibt es?
Welche Relationen gibt es?
Welche Statusuebergaenge sind erlaubt?
Welche Operationen darf ein StateChangePlan enthalten?
Welche Views brauchen die Agenten?
Welche Dinge bleiben Archiv, welche sind current?
```

Die DB sollte spaeter aus diesen Antworten folgen:

```text
StateItems / ItemVersions
Relations
ProvenanceLinks
StateOperations
HumanDecisions
PBI/PBIVersions
GithubMappings
Snapshots / CurrentViews
```

### 13.9 Minimaler naechster Bauschritt

Bevor wir groessere DB- oder GitHub-MCP-Arbeit machen, sollte der naechste funktionale Schritt sein:

```text
1. StateChangePlan-Datenvertrag festlegen.
2. Operationen fuer neues Meeting definieren:
   ADD_ITEM, LINK_TO_EXISTING, REFINE_ITEM, SUPERSEDE_ITEM, CONTRADICT_ITEM,
   MARK_DUPLICATE, REOPEN_DECISION, VERSION_PBI, REGISTER_GITHUB_MAPPING, NO_CHANGE.
3. Gate-Regeln fuer diese Operationen festlegen.
4. Views definieren:
   affected_items_view, active_product_backlog_view, github_reconciliation_view.
5. Entscheiden, ob PBIs als echte State-Items oder als versionierte ProductState-Subentitaeten gefuehrt werden.
```

Erst danach sollte entschieden werden, ob der erste Steward-Durchstich rein deterministisch startet oder direkt
einen StateImpactAgent fuer semantische Matching-Faelle bekommt.
