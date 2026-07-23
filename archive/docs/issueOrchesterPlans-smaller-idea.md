# Smaller Issue-Orchestration Idea: Open-Issue Context statt voller Project State

**Dokumenttyp:** begrenzter Architekturvorschlag  
**Status:** kleine, thesis-taugliche Erweiterungsidee  
**Ziel:** GitHub Issues als aktuellen Arbeitskontext nutzen, ohne einen vollständigen Project State,
bidirektionalen Sync oder globale Historienpflege zu bauen.

**Bezug zum großen Plan (`issueOrchesterPlan.md`):** Dieser Ansatz baut ausschließlich die **Integration-Layer**
(Schicht 5) der 5-Schichten-Zielarchitektur — und davon nur die **create/comment**-Teilmenge. Er delegiert die
harten Bausteine des großen Plans bewusst: **Reconciliation → Mensch** (bestätigt den Match), **Project State →
bounded OpenIssueContext** (nur offene Issues, kein globaler Store), **Impact Planning → feste Routing-Regeln**,
**bidirektionaler Sync → entfällt** (GitHub ist Ziel, nicht Ground Truth). Er ist damit ein sauber abgegrenzter,
später erweiterbarer Ausschnitt — nicht der Continuous-Loop.

---

## 1. Grundidee

Dieser Ansatz baut **keinen vollständigen Project State**.

Stattdessen wird nur ein kleiner, aktueller Kontext aus offenen GitHub Issues gehalten:

```text
OpenIssueContext
= Snapshot der aktuell offenen Issues, die fuer neue Evidence relevant sein koennen.
```

Der Zweck ist pragmatisch:

```text
neue Meeting-Evidence
-> pruefen, ob sie zu einem offenen Issue passt
-> wenn ja: Kommentar / Follow-up am bestehenden Issue
-> wenn nein: neues Issue vorschlagen
```

Damit wird nicht behauptet, dass das System die gesamte Projekthistorie versteht. Es nutzt nur die offenen
Arbeitsobjekte als bounded Zielmenge fuer Matching und Issue-Operationen.

---

## 2. Was bewusst NICHT gebaut wird

Nicht in Scope:

```text
- vollstaendiger Project Store
- globale Entity Resolution ueber alle Artefakte und alle historischen Runs
- bidirektionaler GitHub-Sync
- automatische Uebernahme manueller GitHub-Aenderungen in Ledger/Artefakte
- automatische Konfliktaufloesung
- vollstaendige Sprint-uebergreifende Wahrheitspflege
- autonome Langzeitpflege des Projekts
```

Dieser Ansatz ist also **kein Continuous-SDLC-System**.

Er ist ein kleiner Issue-Linking- und Issue-Operation-Planer auf Basis:

```text
freigegebene Meeting-Evidence + offene Issues + Human Confirmation
```

---

## 3. Minimaler Workflow

```text
1. Offene GitHub Issues importieren
2. Daraus OpenIssueContext erzeugen — dabei MARKER parsen (s. §6b):
     - system-erstellte Issues tragen <!-- agentic-sdlc: artifactItemId=… --> -> DETERMINISTISCHER Link
     - nur marker-lose (mensch-hinzugefuegte) Issues brauchen spaeter semantisches Matching
3. Sprint-/Meeting-Transkript durch Ledger laufen lassen
4. Neue Evidence erzeugen und PRUEFEN -> VERIFIED ArtifactItems (nach CheckerRepair, stabile ID);
     gematcht werden VERIFIED Items, NICHT rohe Claims/Maker-Output
5. Idempotenz-Vorfilter (s. §6b): Item bereits an ein Issue angewendet? -> skip (kein Doppel-Kommentar)
6. Verbleibende Items gegen OpenIssueContext matchen (nur marker-lose Kandidaten -> IssueMatcherAgent)
7. Operation planen:
     - existing issue comment
     - existing issue follow-up
     - new issue
     - skip
     - unclear -> Human Review
8. Human bestaetigt die Operation (im ersten Stand IMMER, s. §6b Confidence)
9. GitHubWriter fuehrt nur bestaetigte Operationen aus -> und setzt bei create den Marker in den Body
```

Wichtig: Der GitHubWriter schreibt nicht frei. Er fuehrt nur validierte und freigegebene Operationen aus.

---

## 4. Minimaler Datenvertrag

### OpenIssueContext

```json
{
  "projectId": "care-app",
  "repository": "org/repo",
  "loadedAt": "2026-07-08T12:00:00Z",
  "issues": [
    {
      "issueContextId": "GH-87",
      "githubNumber": 87,
      "title": "Kalenderansicht umsetzen",
      "status": "open",
      "labels": ["requirement", "calendar"],
      "summary": "Kalenderansicht mit Monatsansicht und Detailoeffnung.",
      "linkedArtifactId": "REQ-0012",
      "linkedArtifactItemIds": ["REQ-0012-R01"],
      "lastSeenAt": "2026-07-08T12:00:00Z"
    }
  ]
}
```

**Provenienz — EIN Pfad, nicht zwei parallele:** das primäre Mapping ist `Issue → Artifact(Item) → Claim →
Evidence`. Es wird KEIN unabhängiger `Issue → Claim`-Pfad gepflegt (er würde vom Artifact-Pfad divergieren).
Claim-IDs dürfen im Kommentar/Metadaten sichtbar sein, sind aber **abgeleitet** über die Items, kein zweiter
Mapping-Pfad. Zwei Identitätsebenen (s. §6b): `linkedArtifactId` = *welches Issue repräsentiert das fachliche
Arbeitsobjekt*; `linkedArtifactItemIds` = *welche konkreten Aussagen sind bereits auf dieses Issue angewendet*.

### Neues Evidence Item

```json
{
  "artifactItemId": "REQ-0012-R04",
  "origin": "extracted",
  "text": "Die Kalenderansicht soll optional auch eine Wochenansicht bekommen.",
  "sourceClaimIds": ["canon_calendar_week_view_open"],
  "producerRunId": "RUN-0002"
}
```

### Issue Match Result

```json
{
  "artifactItemId": "REQ-0012-R04",
  "candidateIssueId": "GH-87",
  "relation": "extends",
  "confidence": 0.82,
  "reason": "Das neue Item erweitert die bestehende Kalenderansicht um eine Wochenansicht.",
  "humanConfirmationRequired": true
}
```

### GitHub Operation Plan

```json
{
  "operationId": "GHOP-0007",
  "operationType": "comment_existing_issue",
  "targetIssueId": "GH-87",
  "sourceArtifactItemIds": ["REQ-0012-R04"],
  "sourceClaimIds": ["canon_calendar_week_view_open"],
  "body": "Neuer Meeting-Befund: Die Kalenderansicht soll optional auch eine Wochenansicht bekommen. Quelle: REQ-0012-R04 / canon_calendar_week_view_open.",
  "requiresHumanApproval": true
}
```

---

## 5. Rollenverteilung

### Custom Executors

```text
LoadOpenIssuesExecutor
  -> zieht offene Issues aus GitHub oder aus einem vorbereiteten lokalen Export.

BuildOpenIssueContextExecutor
  -> normalisiert die Issues in einen kleinen Kontext UND parst den agentic-sdlc-Marker aus dem Body:
     gefundene artifactItemId -> deterministischer linkedArtifactItemIds-Eintrag (kein LLM noetig).

BuildIssueMatchCandidatesExecutor
  -> reduziert den Suchraum; uebergibt dem Matcher NUR marker-lose Issues (die eigenen sind schon verlinkt).

CheckOperationIdempotencyExecutor
  -> deterministisch: ist (artifactItemId -> Ziel-Issue) bereits per Marker/applied-operations.json angewendet?
     -> wenn ja skip. Verhindert Doppel-Kommentare bei Re-Runs.

ValidateIssueOperationExecutor
  -> prueft deterministisch: Ziel-Issue existiert, Operationstyp erlaubt, Quellen-IDs vorhanden.

GitHubIssueWriterExecutor
  -> fuehrt nur freigegebene create/comment Operationen aus; bei create schreibt er den Marker in den Body
     und traegt die Operation in applied-operations.json ein.
```

### Agents

```text
IssueMatcherAgent
  -> klassifiziert die Beziehung zwischen neuem ArtifactItem und Kandidaten-Issues.

GitHubIssuePlannerAgent
  -> formuliert aus bestaetigtem Match eine konkrete GitHub-Operation.
```

### Human Review

Human Review bestaetigt mindestens:

```text
- existing issue wirklich passend?
- comment statt new issue?
- Operationstext akzeptabel?
- skip bei irrelevanten Items?
```

Damit muss die Thesis keine perfekte automatische Entity Resolution loesen.

---

## 6. Erlaubte Match-Klassen

```text
same
  Das neue Item ist im Kern bereits im Issue enthalten.

extends
  Das neue Item erweitert das bestehende Issue sinnvoll.

related
  Das neue Item ist thematisch verbunden, aber nicht direkt dieselbe Arbeit.

contradicts
  Das neue Item widerspricht dem bestehenden Issue oder stellt es in Frage.

unrelated
  Kein passendes offenes Issue gefunden.

unclear
  Agent kann es nicht sicher entscheiden.
```

Routing:

```text
same       -> optional Kommentar / skip
extends    -> Kommentar oder Follow-up am bestehenden Issue
related    -> Human entscheidet Kommentar vs neues Issue
contradicts-> Human Review   (WERTVOLLES Signal: das Meeting hat eine Entscheidung gekippt — kein Randfall)
unrelated  -> neues Issue vorschlagen
unclear    -> Human Review
```

---

## 6b. Schärfungen: Marker, Idempotenz, Staleness, Confidence

### Marker (deterministischer Self-Link — ZWEI Identitätsebenen)

Jedes vom System erstellte Issue trägt einen maschinenlesbaren Marker im Body — mit **Artifact- UND Item-Ebene**:

```text
<!-- agentic-sdlc: artifactId=REQ-0012 artifactItemIds=REQ-0012-R01,REQ-0012-R03 runId=RUN-0002 -->
```

Warum beide Ebenen (wichtige Präzisierung):
```text
artifactId (Arbeitsobjekt): WELCHES Issue repräsentiert das fachliche Objekt „REQ-0012".
   → ein SPÄTER neu erzeugtes Item (z. B. REQ-0012-R04) ist damit DETERMINISTISCH Kandidat für Issue #87,
     obwohl sein eigener Item-Marker die Beziehung noch nicht kennt. (Sonst würde jedes neue Item eines
     bestehenden Artefakts fälschlich als „unbekannt/neu" behandelt.)
artifactItemIds (Aussagen):  WELCHE konkreten Aussagen sind bereits auf dieses Issue angewendet.
   → Basis für Provenienz UND Idempotenz (Item schon angewendet → skip).
```

Beim Reload liest `BuildOpenIssueContextExecutor` den Marker zurück → der Link Issue↔Artifact↔Items ist
**deterministisch**, ohne LLM. Semantisches Matching (`IssueMatcherAgent`) läuft dadurch **nur** gegen
**marker-lose** Issues (= von Menschen über die UI hinzugefügt) UND für neue Items, deren `artifactId` noch auf kein
Issue zeigt. Das reduziert die semantische — und damit fehleranfällige — Arbeit auf genau diese Fälle.

### Idempotenz (kein Doppel-Kommentar bei Re-Runs)

Vor jeder Operation prüft `CheckOperationIdempotencyExecutor` deterministisch, ob `(artifactItemId → Ziel-Issue)`
bereits angewendet wurde — über den Marker im Issue **oder** eine `applied-operations.json`:

```json
{ "operationId": "GHOP-0007", "artifactItemId": "REQ-0012-R04", "targetIssueId": "GH-87",
  "operationType": "comment_existing_issue", "appliedAtRunId": "RUN-0002" }
```

Bereits angewendet → `skip`. Ohne diesen Guard würde jeder erneute Lauf denselben Kommentar erneut schreiben.

### Staleness (frischer Kandidat beim Matchen)

Der `OpenIssueContext` ist ein Snapshot (`loadedAt`/`lastSeenAt`). Beim tatsächlichen Matchen wird der **konkrete
Kandidat** frisch nachgezogen (oder die Snapshot-Aktualität explizit als Grenze dokumentiert), damit nicht gegen
veralteten Issue-Text gematcht wird.

### Confidence ist nur beratend

Das `confidence`-Feld im Match-Result ist **anzeigend**, kein Auto-Trigger. Im ersten Stand bestätigt der Mensch
**jede** Match-/Operation-Entscheidung. Kein „auto-approve ab Schwelle X" — genau das würde die Falle des
zuverlässigen automatischen Matchings wieder öffnen (falsche/verpasste Matches → falsche Kommentare/Duplikate).

---

## 7. Drei Reifestufen (Pflicht vs. optional sauber getrennt)

`create_issue` und `comment_existing_issue` sind NICHT gleich komplex: ein neues Issue braucht **keine** semantische
Zuordnung; ein Kommentar setzt Kandidatensuche + Matching + Human-Confirmation + Staleness + Kommentar-Idempotenz
voraus. Deshalb drei klar getrennte Stufen — der wissenschaftliche Kern bleibt auch dann vollständig, wenn Stufe 3
aus Zeitgründen entfällt:

```text
Stufe 1  plan_only            GitHub-Operationsplan erzeugen, KEIN Schreibzugriff (operations.json).   [immer zuerst]
Stufe 2  create_issue         neues Issue aus verified ArtifactItem, Marker setzen, idempotent.        [PFLICHT-Nachweis]
                              → keine semantische Open-Issue-Zuordnung nötig.
Stufe 3  comment_existing_issue  Match gegen offene Issues + Human-Confirmation + Kommentar.            [OPTIONALER Stretch]
```

Nie im ersten Stand (alle Stufen):
```text
edit issue body · close/reopen/rename issue · acceptance criteria automatisch ändern
sync manueller GitHub-Änderungen zurück in ArtifactItems
```
Kommentare sind sicherer als Body-Updates (auditierbar, überschreiben nichts) — aber sie stehen erst in Stufe 3, weil
das Matching der eigentliche Aufwand ist.

---

## 8. Warum das thesis-tauglich ist

Der kleine Ansatz zeigt trotzdem einen echten Nutzen:

```text
Evidence Ledger
-> verified ArtifactItems
-> bounded Matching gegen offene Issues
-> Human-approved GitHub Operation
```

Damit wird gezeigt:

```text
- Agenten erzeugen nicht nur Dokumente, sondern actionable Arbeitsobjekte.
- Provenienz bleibt erhalten: Issue-Kommentar -> ArtifactItem -> Claim -> Evidence.
- GitHub wird angebunden, ohne GitHub zur Ground Truth zu machen.
- Der Mensch bleibt an der Stelle, an der semantisches Matching unsicher ist.
```

Die Aussage bleibt ehrlich:

```text
Das System unterstuetzt kontrollierte Issue-Orchestrierung fuer offene Arbeitsobjekte.
Es loest nicht die vollstaendige langfristige Projektzustandspflege.
```

Zwei Grenzen, die explizit benannt bleiben müssen:

```text
1. KEIN vollständiger inkrementeller Sprint-Loop.
   Der Ansatz überführt neue geprüfte Evidence in eine kontrollierte ISSUE-OPERATION —
   er pflegt NICHT automatisch alle internen Baseline-Artefakte, Versionen und Ableitungen weiter.
   Das Ergebnis ist eine Issue-Operation, nicht zwingend ein aktualisierter interner Projektzustand.

2. Kommentar ≠ interne Artefakt-Aktualisierung.
   Ein GitHub-Kommentar heißt nur: „eine neue geprüfte Erkenntnis wurde an ein operatives Arbeitsobjekt
   angehängt". Daraus folgt NICHT, dass requirements.md/risks.md versioniert aktualisiert wurde.
   Im kleinen Plan bleibt der interne Zustand entweder unverändert ODER speichert ein separates neues
   ArtifactItem; der Kommentar ist reine operative Weitergabe. Erst ein echter Project-State-Loop
   hielte intern + GitHub dauerhaft konsistent (= Future Work).
```

---

## 9. Empfohlene Minimal-Reihenfolge

```text
PFLICHT (create-Pfad, keine semantische Zuordnung):
  1. ArtifactItem-IDs einfuehren (stabil + persistiert) + artifact.json
  2. plan_only: GitHubOperationPlan erzeugen -> operations.json (KEIN Schreibzugriff)
  3. create_issue aktivieren: neues Issue aus verified Item, Marker (artifactId+itemIds) setzen, idempotent

OPTIONAL (comment-Pfad, semantisches Matching):
  4. OpenIssueContext laden + Marker parsen (eigene Issues deterministisch verlinkt)
  5. IssueMatcherAgent NUR gegen marker-lose Kandidaten / Items ohne bekannte artifactId->Issue-Zuordnung
  6. Human Confirmation fuer Match/Operation
  7. comment_existing_issue aktivieren
```

Der erste Test läuft ohne echten GitHub-Schreibzugriff (`plan_only -> operations.json`). Erst wenn die Operationen
plausibel sind, wird der Writer aktiviert. Der PFLICHT-Teil (1–3) ist ein vollständiger Integrationsnachweis auch
ohne den optionalen Match-Teil (4–7).

---

## 10. Verhältnis zum großen Issue-Orchesterplan

Dieser Smaller Plan ist eine bewusst begrenzte Teilmenge:

```text
Großer Plan:
  Project State + Reconciliation + Impact Planning + GitHub Sync + Sprint Loop

Smaller Plan:
  Open Issues als Kontext + neues Evidence Item + Match + Human Approval + create/comment
```

Er kann spaeter in den großen Plan wachsen, muss es aber nicht. Jede `Simple*`-Komponente ist der minimale Keim
ihrer großen Schwester — die Interfaces sind so geschnitten, dass sie überleben:

```text
OpenIssueContext            -> ProjectStore              (Datei/offene-Issues  ->  DB/voller State)
IssueMatcherAgent (+Mensch) -> ClaimReconciliationAgent  (bounded, human-bestätigt  ->  global/automatisch)
feste Routing-Regeln (§6)   -> ImpactPlanner             (Regeln  ->  Planung)
create/comment (einseitig)  -> GitHub Sync               (Ziel  ->  bidirektional)
```

Ehrlich bleibt dabei: die menschlich delegierten Teile (Matching, Konflikt) sind im großen Plan die eigentliche
Härte — der kleine Ansatz demonstriert das Gerüst, löst diese Kernprobleme aber NICHT (er umgeht sie).

Wenn der kleine Ansatz funktioniert, ist er ein guter Capstone:

```text
fruehe SDLC-Phasen
-> evidenzgebundene Artefakte
-> kontrollierte Ableitung
-> konkrete, nachvollziehbare GitHub-Arbeitseintraege
```

Wenn er nicht weiter ausgebaut wird, bleibt er trotzdem ein sauber abgegrenzter Nachweis.
