# Plan — ProjectState als Projektwahrheit & Core-Ingestion (das Eingangstor)

Status: **PLAN / DESIGN (geschärft, Rev 2).** Noch nicht gebaut. Adressiert das Meeting-N-Szenario: der
ProjectState soll die **vollständige, versionierte Projektwahrheit** werden, gegen die jedes neue Meeting
aufgelöst wird — **bevor** L4/PB/GitHub arbeiten.

Rev-2-Schärfungen (aus Review): (a) ProjectState = **volle Wahrheit** (Hub), nicht „besserer Requirement-Index";
(b) `identityKey` = Such-**Anker**, nicht Identitäts-**Urteil**; (c) reiches **Operations-Set** (StateChangePlan)
statt „REFINE=neue Version"; (d) Downstream bekommt eine **affected-view**, nicht Roh-Delta; (e) **Retrieval-
Leiter** — jetzt „alle zeigen", Embeddings/Vector-DB erst später hinter einem Port; (f) Zielmodell vollständig,
Umsetzung **inkrementell** (Requirement-Ingestion zuerst).

Verwandt: `IST-Zusatnd.md` (§5–§7 DB-Lücken), `Github-Agent.md` (operative Reconciliation, entlastet),
`13.7-github-plan.md` §11 (Autorität) + §14 (zweites Meeting / Delta / Impact-Agent — hier konkretisiert).

---

## 1. Problem (das Meeting-N-Szenario)

Meeting 20 kommt. Erneut „No-Go", plus ein neues Feature. L1–L3 laufen wie immer. Frage: **Wo/wie stellt das
System fest, ob das schon bekannt ist?** Wiederholung, Verfeinerung, Widerspruch oder wirklich neu?

Heute wird das nirgendwo systematisch beantwortet: `project-state.json` liegt unter `runs/project-state/<run>/…`
= ein **pro-Lauf neu gebautes** L1–L3-Ergebnis, kein fortgeschriebener Speicher. Keine stabile Identität über
Meetings, keine Versionierung, kein Index. Die Wahrheit ist über Schichten verstreut (project-state / canonical /
product-backlog / github-mappings). Ohne Änderung landet die „bekannt/neu?"-Frage beim GitHub-Agenten am Ende —
die falsche Stelle (er müsste in einem Jahr den ganzen Bestand absuchen).

---

## 2. Zieldefinition — ProjectState = die Projektwahrheit (Hub)

Der ProjectState ist **nicht** nur L1–L3. Er ist die **vollständige, versionierte Wahrheit** und der zentrale
Hub. Es gibt **Eingangskanäle** (bringen Neues herein → brauchen **Ingestion** = Identitätsauflösung) und
**Projektionen** (fließen aus der Wahrheit heraus → keine Identitätsfrage).

```text
        EINGANGSKANÄLE (Ingestion: gegen Wahrheit auflösen)
        ───────────────────────────────────────────────────
  Meetings/Transkripte ─┐   (Requirements)          ┌──────────────────────────────┐
  Stakeholder-Antworten ─┼─► (Open Decisions) ─────► │   PROJECT STATE / CORE       │
  GitHub-Realität ───────┘   (Status/Drift)          │   = versionierte Wahrheit    │
                                                     │                              │
        PROJEKTIONEN (aus Wahrheit heraus)           │  Evidence / Claims           │
        ─────────────────────────────────           │  Requirements (L1/L2/L3)     │
  Canonical Baseline  ◄──────────────────────────────┤  Canonical Requirements      │
  Feature-Cluster / PBIs ◄───────────────────────────┤  Feature-Cluster             │
  IssuePlan / GitHub-Issues ◄────────────────────────┤  Product-Backlog-Items       │
  Living-Requirements-Doc ◄──────────────────────────┤  Open Decisions              │
                                                     │  GitHub-Mappings + Status    │
                                                     │  Human Decisions             │
                                                     │  Relations · Versions ·      │
                                                     │  Provenance                  │
                                                     └──────────────────────────────┘
```

**Drei Ingestion-Tore** (jedes löst NUR sein Thema auf — kein Tor holt die Arbeit eines anderen nach):

```text
1. Requirement-Ingestion   Meetings → Requirements     ← ERSTER Increment (dieser Plan, §6)
2. Decision-Ingestion      Stakeholder → Open Decisions ← späterer Increment (§13), gleiches Muster
3. GitHub-Feedback-Ingestion GitHub → Status/Drift      ← der GitHub-Agent (Github-Agent.md), Rückkanal
```

**Projektionen** (Canonical, Cluster, PBIs, Issues, Living-Doc) sind **kein** Ingestion — sie sind Ableitungen
aus der Wahrheit. Grenzfall PBIs: Projektion, aber mit **stabiler Identität/Versionen** → sie gehören als
Entität in den Core, werden aber nicht wie externes Material „ingested".

---

## 3. Begriffe (festziehen — sonst bleibt unklar, was Wahrheit ist)

```text
MeetingDelta / ExtractionResult  = Output eines neuen Meetings (L1–L3-Kandidaten). NOCH NICHT Wahrheit.
ProjectState / Core              = akzeptierte, versionierte Wahrheit (der Hub oben).
View                             = kontrollierter Ausschnitt der Wahrheit für Agenten/Downstream
                                   (z.B. CanonicalRequirementsView, ProductBacklogView, affected_items_view).
StateChangePlan                  = die vom Resolver vorgeschlagenen OPERATIONEN auf dem Core (§6), vor Apply.
```

---

## 4. Datenmodell des Core (typisierte Wahrheit, ein Store hinter dem Port)

Ein adressierbarer Store hinter dem bestehenden Repository-/View-Port. JSON jetzt, DB später.

```text
CoreEntity
  entityId        stabile ID — geprägt NACH Resolver+Gate+Human, danach unveränderlich
  identityKey     normalisierter Such-ANKER (nur Retrieval-Hilfe, NIE das Identitäts-Urteil)
  kind            evidence | requirement | canonical_requirement | feature | pbi | decision | github_mapping
  currentVersion  Zeiger auf aktive Version
  status          active | done | superseded | retired | open_decision | resolved   (Lifecycle, Rev 4)
  relations       [ refines, supersedes, duplicates, contradicts, blocks, part_of_feature,
                    realized_by_pbi, implemented_by_issue, decided_by, ... ]

CoreEntityVersion
  versionId       vN
  statement       Text dieser Version
  provenance      { meetingId, runId, claimIds[], origin, humanDecisionId? }
  createdUtc

CoreMapping   (kind=github_mapping als erstklassige Relation, NICHT als Run-Artefakt)
  entityId/pbiId ↔ issueNumber  + operationalStatus (open/closed/drift)

CoreIndex     (Retrieval-Hilfe, §7 — austauschbar, KEIN Identitäts-Entscheider)
  identityKey → entityId (Kandidaten)      | später: embedding → Top-K
```

Kernregeln:
- **`identityKey` findet Kandidaten, entscheidet nichts.** Zwei verschiedene Requirements können denselben Key
  haben. Identität = Resolver + Gate + HumanReview → dann erst `entityId`.
- **Historie bleibt.** Verfeinerung/Widerspruch = neue Version; alte Version bleibt mit Provenienz erhalten.
- **PBIs + GitHub-Mappings sind Teil des Core** (Views darauf), nicht separate Run-Dateien.
- **Features sind erstklassige Entitäten** (`kind=feature`, stabile ID + identityKey) — NICHT lose Strings.
  `part_of_feature` zeigt auf die Feature-Entität. (Rev-3-Schärfung, Review nach Block D: ein String-featureKey
  reproduziert die Identitätsfrage auf Feature-Ebene bei Synonymen/Umbenennung. Feature-Identität = dasselbe
  retrieval-then-judge-Muster eine Ebene höher, landet mit dem Cluster-Eintritt in den Core. Increment 1 nutzt
  übergangsweise einen normalisierten String, der beim Cluster-Eintritt gegen bestehende Cluster-IDs aufgelöst wird.)
- **Core = versionierter Projektgraph, keine endlose Liste** (Rev 4, Review). Erledigtes/Ersetztes bleibt erhalten,
  aber ueber **Lifecycle-Status** eingeordnet (active/done/superseded/retired). Agenten bekommen NICHT den ganzen
  Core, sondern **Views**: `active-backlog-view` (nur active) · `affected-items-view` (Blast-Radius eines Deltas) ·
  `archive-view` (done/superseded/retired) · `github-sync-view` (PBIs + mappings + status). Der Core ist die volle
  Wahrheit mit Historie; Views sind die Arbeitsausschnitte. Details + Blast-Radius-Regel: `plan-increment1c-*.md`.

---

## 5. Was „Ingestion" heißt (allgemein)

**Ingestion = neues Material hereinnehmen und gegen die bestehende Wahrheit AUFLÖSEN — nicht anhängen.**

```text
Anhängen (heute):   neues Meeting → neue Items liegen neben den alten → Duplikate/Widersprüche
Ingestion (Ziel):   neues Meeting → "kenne ich das?" → an bestehende Wahrheit ANDOCKEN (Operation + Version)
```

Alle drei Tore (§2) sind Ingestion; sie teilen dasselbe Muster: **Retrieval → Agent-Urteil (StateChangePlan) →
Gate → HumanReview → deterministischer Apply.** Dieser Plan baut Tor 1 (Requirement-Ingestion) durch.

---

## 6. Requirement-Ingestion (erster Increment) — der konkrete Baustein

Eigene Stufe **direkt nach L3, vor L4** (Flag I entschieden). Auflösung auf **REQ-/Item-Ebene** (Flag II
entschieden): die Einheit, in der Meetings „dasselbe nochmal" sagen; Claims bleiben Provenienz darunter,
CAN-REQ Projektion darüber.

```text
1. RETRIEVAL (§7): pro eingehendem Requirement-Kandidat → bounded Kandidatenmenge bestehender Requirements.

2. RESOLVER-AGENT urteilt nur über die Kandidaten → StateChangePlan (Operationen, je mit Beleg):
     RESTATE      identisch → an bestehende Entität anhängen (neue Version, gleiche Aussage)
     REFINE       konkretisiert DIESELBE Anforderung → neue Version, alte in Historie
     NEW_RELATED  fachlich NEU, aber im selben Feature (z.B. "No-Go bearbeiten" neben "No-Go anzeigen")
                  → neue Entität + Relation part_of_feature
     NEW          nichts Passendes → neue Entität
     SUPERSEDE    ersetzt eine alte Anforderung → alte auf status=superseded, Relation supersedes
     CONTRADICT   widerspricht bestehender Wahrheit → Open Decision, KEIN stilles Überschreiben
     (SPLIT / MERGE: zunächst human-getrieben im Review, nicht autonom)
   Beleg-Pflicht (Anti-Halluzination): jede Operation nennt Ziel-Entität(en) + claimIds.

3. GATE (deterministisch): jede Operation hat Beleg; MATCH/REFINE/SUPERSEDE haben ein Ziel;
   CONTRADICT landet als Open Decision; keine widersprüchlichen Operationen auf derselben Entität.

4. HUMANREVIEW (generische UI): Merges/Refinements/Konflikte + SPLIT/MERGE bestätigen/ablehnen/editieren.
   Das ist die Stelle, an der ein Mensch die Wahrheit autorisiert (analog cluster-/backlog-review).

5. APPLY (deterministisch): Upsert-by-Identity in den Core — neue Entitäten/Versionen/Relationen, stabile IDs
   erst hier prägen, Provenienz + Index aktualisieren. Ausgabe = Core-Delta (added/changed/unchanged/conflicts).
```

Muster = **Agent[Tools] (Maker) → det. Gate → HumanReview → det. Apply**, identisch zum Rest des Systems.
Der StateChangePlan ist **kein neues Konstrukt**, sondern das bereits gebaute operationsbasierte Muster
(`ClusterOperation` in cluster-review) an der Ingress-Grenze wiederverwendet.

Hinweis Operations-Set: **klein starten** und aus echten Meeting-2-Daten wachsen; das Vokabular oben ist der
Rahmen, nicht die Pflicht-Vollausbaustufe.

---

## 7. Retrieval-Leiter — „nicht die ganze DB durchsuchen" (Embeddings SPÄTER)

„Retrieval-then-judge" ist das Muster; *wie* Kandidaten gefunden werden, ist austauschbar hinter einem
`ICandidateRetriever`-Port. Embeddings/Vector-DB sind die **teuerste, späteste** Stufe, kein Core-Bestandteil.

```text
Stufe 0  "Alle zeigen":  dem Resolver die volle Liste bestehender Requirement-Statements geben.
         Der LLM IST der semantische Matcher.                         ← reicht bis ~einige hundert Items → JETZT
Stufe 1  Lexikalischer Vorfilter (identityKey-Normalisierung + Token-Overlap;
         Scoring-Basis wie GithubIssueCandidateMatcher).              ← wenn die Liste zu groß zum Zeigen wird
Stufe 2  Embeddings + ANN-Index (JSON/SQLite).                        ← wenn Lexik zu viele Paraphrasen verpasst
Stufe 3  Vector-DB (pgvector o.ä.).                                   ← bei Tausenden Items / Latenz / Concurrency
```

Begründung „jetzt Stufe 0": bei ~69 Requirements kann der Agent **alles sehen** → volles semantisches Matching
**gratis, ohne Infrastruktur**. Embeddings werden erst nötig, wenn man dem Agenten **nicht mehr alles zeigen**
kann (~viele hundert+). Vector-DB-Territorium beginnt bei ~10.000+. Zusatznutzen für die Thesis: der sichtbare
**Agenten-Judge** über einer gezeigten Liste ist beobachtbar/promptbar/prüfbar — wertvoller als eine opake
Embedding-Cosine-Schwelle. Stufenwechsel = Adapter-Tausch hinter dem Port, ohne Resolver/Gate/Apply anzufassen.

---

## 8. Downstream bekommt eine affected-view, nicht Roh-Delta

Damit L4/re-clarify entscheiden kann „PBI erweitern/splitten/versionieren/neu", braucht es das Delta **plus
Umgebung**:

```text
affected_items_view = geänderte/neue Items
                    + betroffenes Feature-Cluster
                    + verwandte bestehende Requirements & PBIs
                    + relevante Open Decisions
                    + Relationen (+ frühere Versionen bei Bedarf)
```

Nicht Roh-Delta (zu wenig Kontext) und nicht der ganze Archiv-State (zu viel). Eine View auf den Core, um den
Blast-Radius eines Deltas.

---

## 9. Was BLEIBT, was wird ANGEPASST

**Bleibt:** L1–L3 (zustandslos, Kandidaten); L4/Cluster/Clarify/IssuePlanning/GitHub-Write (arbeiten künftig auf
dem Core/affected-view statt auf Pro-Lauf-Snapshot); das Repository-/View-Port-Konzept; das Maker→Gate→Human→
Apply-Muster.

**Wird angepasst:**
- `project-state` = **persistenter, fortgeschriebener Core** (Upsert statt Rebuild); Writes **über den
  Repository-Write-Port** (nicht `File.WriteAllText`).
- **Neue Ingress-Stufe** zwischen L3 und L4 (§6).
- **PBIs + GitHub-Mappings** werden Teil des Core (Views darauf), nicht Run-Artefakte daneben.
- **affected_items_view** für Downstream (§8).
- **Retrieval-Port** (Stufe 0 jetzt; Embeddings später, §7).
- **Naming** (§3): ExtractionResult/MeetingDelta vs. ProjectState/Core vs. Views.
- **Stabile PBI-Identität** (plan-pb Flag A) fällt ab, weil der Core Identität kann.

---

## 10. DB-later — die Ingestion IST die DB-Vorbereitung

Core als *Entitäten + Versionen + Relationen + Index + Mappings* mit **Upsert-by-Identity** schließt die vier
DB-Lücken (IST-Zusatnd §6): persistente Objekte statt Run-Views · stabile IDs · Writes über den Port · Scope
query-basiert. → DB-Wechsel später = **Adapter-Tausch hinter dem Port**, kein Umbau. Kein Bedarf, jetzt eine DB
einzuführen — aber alles wird DB-fähig gebaut.

---

## 11. Abgrenzung zum GitHub-Agenten

Nach der Requirement-Ingestion ist „bekannt/neu?" **auf Requirements-Seite beantwortet und gespeichert**. Der
GitHub-Agent (Tor 3, Github-Agent.md) bekommt ein **aufgelöstes Core-Delta** + persistiertes Mapping und macht
nur den **operativen** Abgleich gegen GitHubs messy Realität (menschliche Issues, geschlossen-aber-nicht-erfüllt,
Titel-Paraphrasen). Er sucht **nicht** die ganze Wahrheit ab. „Gleiche Sache, andere Worte" bleibt beim Agenten
nur noch auf der **GitHub-Seite**, nicht auf der Requirements-Seite.

---

## 12. Entscheidungen (entschieden)

- **Flag I — Ankerstelle:** **eigene Stufe nach L3, vor L4.** L4 bleibt reiner Konsolidierer; die Auflösung ist
  ein eigener, testbarer, human-reviewbarer Schritt.
- **Flag II — Granularität:** **REQ-/Item-Ebene.** Claims bleiben Provenienz darunter, CAN-REQ Projektion darüber.

---

## 13. Bauschritte (inkrementell)

```text
Increment 1 — Requirement-Ingestion (dieser Plan):
  1. Core-Datenmodell (Entität/Version/Relation/Provenance/Mapping) + Repository-Write-Ops über den Port.
  2. Einmaliger Seed: aktuellen Stand in den Core heben (Items → Entitäten + identityKeys).
  3. ICandidateRetriever (Stufe 0 "alle zeigen").
  4. Resolver-Agent + StateChangePlan-Operationen (§6) + Gate + HumanReview-Adapter + Upsert-Apply.
  5. affected_items_view; L4/re-clarify darauf umstellen.
  6. Naming-Umstellung (§3).
  7. Zweiter Meeting-Testlauf (echtes Delta) → belegen: bekannt erkannt, neu angelegt, Konflikt geflaggt.

Spätere Increments (gleiches Muster, anderes Tor):
  - Decision-Ingestion (Stakeholder beantwortet Open Decision → resolved + PBI-Status-Update).
  - GitHub-Feedback-Ingestion = GitHub-Agent-Vollausbau (Rückkanal, gated Write).
  - Retrieval Stufe 1→3 bei Bedarf (Skalen-getrieben, port-versteckt).
```

---

## 14. Erwartete Wirkung (zu belegen, nicht vorwegnehmen)

- Meeting 20 „No-Go erneut": bestehende Requirements **wiedererkannt** (kein Duplikat); „No-Go bearbeiten" als
  **NEW_RELATED** im selben Feature; das neue Feature als **NEW**; ein Widerspruch als **Open Decision** geflaggt.
- Der Core bleibt **eine** aktuelle Wahrheit mit Historie; Downstream propagiert nur die affected-view.
- Der GitHub-Agent arbeitet auf einem sauberen Delta → der Vergleichspfad (Github-Agent.md §8) misst dann das
  Richtige (operative Reconciliation), nicht ungelöste Identität.
