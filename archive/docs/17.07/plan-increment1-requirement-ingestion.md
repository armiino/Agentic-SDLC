# Plan — Increment 1: Requirement-Ingestion (Detail-Bauplan)

Status: **DETAIL-PLAN / DESIGN.** Noch nicht gebaut. Umsetzt Tor 1 aus `plan-core-ingestion.md` §6/§13.
Ziel: ein neues Meeting-Delta wird auf **Requirement-/Item-Ebene** gegen den **persistenten Core** aufgelöst
(bekannt/verfeinert/verwandt-neu/neu/ersetzt/widersprüchlich), human-reviewt, deterministisch eingearbeitet.

---

## 0. DONE-Kriterium (des ganzen Increments)

```text
Ein zweites Meeting (Delta) läuft durch:  MeetingDelta → Resolve → Gate → HumanReview → Apply → Core+
belegt, dass:
  - bekannte Anforderungen WIEDERERKANNT werden (kein Duplikat),
  - "No-Go bearbeiten" als NEW_RELATED im Feature No-Go landet,
  - ein echtes neues Feature als NEW angelegt wird,
  - ein Widerspruch als OPEN DECISION geflaggt wird (kein stilles Überschreiben),
  - der Core danach EINE aktuelle, versionierte Wahrheit mit Historie ist,
  - ein Core-Delta + affected_items_view als Artefakt entsteht.
Idempotenz: dasselbe Meeting erneut ingesten = reine RESTATE-No-Ops (keine neuen Versionen).
```

---

## 1. Design-Grundentscheidung: Core = erweiterte `ProjectStateDocument` (REUSE, kein Parallelmodell)

Das bestehende `projectstate/ProjectStateModels.cs` ist bereits fast der Core:
`ProjectStateItem` hat `ItemId, ItemType, Text, Status, Origin, Version, SourceClaimIds, Metadata`;
`ProjectStateDocument` hat `Items, Relations, Provenance, Proposals, Sources`.

→ **Wir bauen kein neues `CoreEntity`, sondern befördern `ProjectStateDocument` zum persistenten Core und
erweitern es minimal** (SchemaVersion 1→2):

```text
ProjectStateItem  + identityKey : string        (Retrieval-ANKER, nicht Identitäts-Urteil)
                  + history      : ItemVersion[] (frühere Fassungen: text + provenance + createdUtc)
                  (Version int bleibt = Zeiger auf aktuelle Fassung)
```

Alles andere ist schon da: Relations (part_of_feature, supersedes, contradicts, …), Provenance, Proposals
(für Open Decisions nutzbar). PBIs/GitHub-Mappings passen später als `ItemType=pbi` + Relations rein — **Increment
1 fasst nur `ItemType=requirement` an.** Damit bleibt die Wahrheit EIN Modell (DB-later gilt unverändert).

**Naming:** die per-Meeting L1–L3-Ausgabe ist die **MeetingDelta** (ein `ProjectStateDocument` mit den neuen
Items dieses Meetings); der persistente, fortgeschriebene Store ist der **Core** (= ProjectState im wörtlichen
Sinn). L1–L3 bleiben unverändert.

**ID-Autorität (Befund D4):** `project-state-build` ist ein deterministischer **Rebuild aus den übergebenen
Artefakten** (keine Historien-Aggregation); die `ItemId`s kommen **aus den Artefakten**, nicht vom Builder.
→ MeetingDelta = `project-state-build` nur auf den Artefakten des neuen Transkripts. Weil eine frische L1/L2-
Extraktion **eigene** REQ-Nummern vergibt, sind eingehende `ItemId`s **meeting-lokal/provisorisch**. **Der Core
ist die ID-Autorität:** RESTATE/REFINE/SUPERSEDE mappen incoming→bestehende Core-ID, NEW/NEW_RELATED prägen eine
neue Core-ID (`REQ-<max+1>`). Die Zuordnung incoming↔Core geschieht im Resolver (Vorschlag) + Apply (verbindlich).

---

## 2. Datenfluss

```text
neues Transkript
  → L1–L3 (unverändert)  →  MeetingDelta (ProjectStateDocument, nur neue Items)   [runs/…/meeting-delta.json]
  → ingest-requirements  →  Resolver-Agent liest Core (Stufe-0-Retrieval) + MeetingDelta
                            → StateChangePlan (Operationen je Item, mit Beleg)     [runs/ingestion/<run>/plan.json]
                            → IngestionGate (deterministisch)                       [gate-report.json]
  → ingest-review        →  HumanReview-UI über die Operationen                     [human-decisions.json]
  → ingest-apply         →  Upsert-by-Identity in den Core (über Write-Port)        [state/core/project-state.json]
                            + Core-Delta (added/changed/unchanged/conflicts)        [runs/ingestion/<run>/delta.json]
                            + affected_items_view                                    [affected-view.json]
```

---

## 3. Sub-Phasen (jede eigenständig baubar + verifizierbar)

### 1.0 Core-Store + Write-Port
- **Ziel:** persistenter Core an fester Stelle + Schreib-Ops über einen Port (nicht `File.WriteAllText`).
- **Bauen:** `core/ICoreRepository.cs` (Load, Save, UpsertItem, AppendVersion, AddRelation, AddProposal) +
  `core/JsonCoreRepository.cs`. Modell-Erweiterung `identityKey` + `history` (SchemaVersion 2, abwärtskompatibel:
  fehlende Felder → default).
- **Ort (Entscheidung D1):** `state/core/project-state.json` (repo-root, EIN lebender File, außerhalb `runs/`).
  Historie liegt (a) im Item (`history[]`) und (b) als Audit-Snapshot pro Ingestion unter `runs/ingestion/<run>/`.
- **Exit:** Core lädt/speichert reproduzierbar; Roundtrip-Test grün. **Evidence:** `state/core/project-state.json`.

### 1.1 Core-Seed (einmalig)
- **Ziel:** aktuellen Stand als Ausgangs-Wahrheit in den Core heben.
- **Bauen:** `core/CoreSeeder.cs` + CLI `core-seed <project-state.json>`. Liest
  `runs/project-state/20260715_l3_v2/project-state.json` (105 Items), berechnet `identityKey` je Item,
  `history=[]`, `Version=1`. Deterministisch, kein LLM.
- **Exit:** Core mit 105 Entitäten (69 requirement / 36 architecture), alle mit identityKey.
  **Evidence:** `state/core/project-state.json` + `core-seed-report.json`.

### 1.2 Retrieval Stufe 0 (`ICandidateRetriever`)
- **Ziel:** Kandidaten für ein eingehendes Item liefern — jetzt „alle zeigen".
- **Bauen:** `core/ICandidateRetriever.cs` + `core/ShowAllRequirementRetriever.cs` (gibt alle
  `ItemType=requirement`-Entitäten als Kandidaten zurück). Port-Grenze für spätere Stufen 1–3 (Embeddings).
- **Exit:** Retriever liefert die 69 Requirement-Kandidaten. **Evidence:** Unit-artiger Selbsttest im Lauf-Log.

### 1.3 Resolver-Agent + StateChangePlan (MAF-Workflow)
- **Ziel:** pro eingehendem Requirement eine Operation vorschlagen, mit Beleg.
- **Bauen:** `ingestion/IngestionModels.cs` (`StateChangePlanDocument`, `StateChangeOperation`),
  `ingestion/IngestionTools.cs` (`get_incoming_items`, `list_core_requirements`/`get_core_entity` [Retriever],
  `check_state_change_plan` [det. Vorpass], `save_state_change_plan`),
  `ingestion/IngestionResolveWorkflow.cs` (MAF: `ResolveExecutor`(Agent) → `GateExecutor` → `FinalizeExecutor`,
  Muster wie `ReClarifyClusterWorkflow`), Prompt `Prompts/phase2_evidence/RequirementIngestionAgent/…txt`.
- **Operationen (Rahmen, klein starten):**
  `RESTATE · REFINE · NEW_RELATED · NEW · SUPERSEDE · CONTRADICT` (SPLIT/MERGE zunächst nur im Human-Review).
  `StateChangeOperation = { incomingItemId, kind, targetEntityId?, featureKey?, statement, claimIds[], rationale }`.
- **Beleg-Pflicht:** MATCH/REFINE/SUPERSEDE/CONTRADICT nennen `targetEntityId`; alle nennen `claimIds`.
- **Exit:** Lauf erzeugt einen validen `plan.json`. **Evidence:** `runs/ingestion/<run>/plan.json` + tool-calls.

### 1.4 IngestionGate (deterministisch)
- **Ziel:** kein stiller Fehler, kein Halluzinations-Match.
- **Bauen:** `ingestion/IngestionGate.cs`. Prüfungen:
  `COVERAGE` (jedes MeetingDelta-Item genau eine Operation) ·
  `UNKNOWN_TARGET` (targetEntityId existiert im Core) ·
  `TARGET_REQUIRED`/`TARGET_FORBIDDEN` (REFINE/SUPERSEDE mit Ziel; NEW ohne) ·
  `CONTRADICT_NEEDS_DECISION` (CONTRADICT ⇒ Open-Decision-Proposal) ·
  `MISSING_EVIDENCE` (claimIds leer) ·
  `CONFLICTING_OPS` (keine zwei widersprüchlichen Ops auf demselben Ziel).
- **Exit:** Gate blockt fehlerhafte Pläne, passiert saubere. **Evidence:** `gate-report.json`.

### 1.5 HumanReview-Adapter + CLI
- **Ziel:** Mensch autorisiert Merges/Refinements/Konflikte; kann Operation überschreiben.
- **Bauen:** `ingestion/IngestionReviewAdapter.cs` (1 `ReviewItem` je Operation; Felder: `decision`
  apply/skip/edit, `kind`-Override, bei Merge incoming↔target nebeneinander), CLI `ingest-review <run>`.
  Wiederverwendung `AgenticSdlc.HumanReview` (`LocalReviewServerHost`) wie `ReClarifyClusterReviewAdapter`.
- **Exit:** UI zeigt Operationen mit echter Entscheidungsmöglichkeit; `human-decisions.json` entsteht.
  **Evidence:** `runs/ingestion/<run>/human-decisions.json`.

### 1.6 Apply — Upsert-by-Identity (deterministisch)
- **Ziel:** die autorisierten Operationen in den Core einarbeiten.
- **Bauen:** `ingestion/IngestionApply.cs` + CLI `ingest-apply <run>`. Semantik:
  `RESTATE` → keine Änderung außer Provenienz-Append (Idempotenz) ·
  `REFINE` → aktuelle Fassung nach `history`, neuer `Text`, `Version++`, Provenienz ·
  `NEW`/`NEW_RELATED` → neue Entität, **stabile ID erst hier prägen** (`REQ-<max+1>`), NEW_RELATED + Relation
  `part_of_feature` ·
  `SUPERSEDE` → Ziel `Status=superseded` + Relation `supersedes`; neue Entität für die ersetzende Aussage ·
  `CONTRADICT` → `Proposal`/Item `ItemType=decision` (Open Decision) + Relation `contradicts`, **kein**
  Überschreiben.
  Danach: Core über `ICoreRepository` schreiben, `identityKey`/Index aktualisieren.
- **Exit:** Core aktualisiert; Re-Run idempotent. **Evidence:** `delta.json` + neuer `state/core/…` + Audit-Snapshot.

### 1.7 affected_items_view (emittieren)
- **Ziel:** Downstream bekommt Delta + Blast-Radius, nicht Roh-Delta.
- **Bauen:** `ingestion/AffectedItemsView.cs`: geänderte/neue Items + betroffenes Feature-Cluster + verwandte
  Requirements + relevante Open Decisions + Relationen. Als View-Artefakt.
- **Exit:** `affected-view.json` entsteht. **Downstream-Konsum (L4/re-clarify) ist NICHT Teil von Inc 1**
  (Entscheidung D3) → eigener Folge-Increment 1b.
- **Evidence:** `runs/ingestion/<run>/affected-view.json`.

### 1.8 Naming-Umstellung (leichtgewichtig)
- **Ziel:** Begriffe sichtbar, ohne großes Rename-Churn.
- **Bauen:** neue Typen/Kommandos tragen die Begriffe (MeetingDelta, Core, Ingestion); ein kurzer Doc-/README-
  Hinweis im `core/`-Ordner. **Physisches Umbenennen bestehender Klassen: bewusst DEFERRED** (Risiko/Nutzen).
- **Exit:** Begriffe konsistent in neuem Code + Ordner-README.

### 1.9 Kontrollierter Zwei-Meeting-Test → Beleg
- Siehe §6. **Exit = DONE-Kriterium (§0).**

---

## 4. Datei-Landkarte (neu)

```text
Evidenz-Agent/core/
  ICoreRepository.cs · JsonCoreRepository.cs      (persistenter Store + Write-Ops, Port)
  CoreSeeder.cs                                   (einmaliger Seed aus ProjectState)
  ICandidateRetriever.cs · ShowAllRequirementRetriever.cs   (Retrieval Stufe 0)
  IdentityKey.cs                                  (deterministische Normalisierung)
  CorePaths.cs                                    (state/core/ auflösen)
  README.md                                       (Begriffe: MeetingDelta/Core/View)
Evidenz-Agent/ingestion/
  IngestionModels.cs                              (StateChangePlan, StateChangeOperation, Delta, AffectedView)
  IngestionTools.cs                               (Agent-Tools)
  IngestionResolveWorkflow.cs                     (MAF: Resolve→Gate→Finalize)
  IngestionGate.cs                                (deterministisch)
  IngestionReviewAdapter.cs                       (HumanReview-Mapping)
  IngestionApply.cs                               (Upsert + Delta + affected-view)
Prompts/phase2_evidence/RequirementIngestionAgent/…txt
(model-erweiterung in projectstate/ProjectStateModels.cs: identityKey + history, SchemaVersion 2)
```

## 5. CLI (Program.cs)

```text
core-seed <project-state.json>          einmalig: Core aus aktuellem Stand
ingest-requirements <meeting-delta>     Resolver-Workflow → StateChangePlan
ingest-review <ingestion-run>           HumanReview-UI über Operationen
ingest-apply <ingestion-run>            deterministischer Upsert in den Core + Delta + affected-view
```

## 6. Test-Fixture (vorhanden: `Interview-Einrichtung.txt`, `T9999_chaos.txt`)

- **Meeting A** = `Interview-Einrichtung.txt` → schon verarbeitet → **Seed** des Core (§1.1).
- **Meeting B** = neue kleine Fixture `input/transcripts/meeting-2-delta.txt` (bewusst konstruiert), enthält:
  (i) **Wiederholung** „No-Go pro Bewohner anzeigen" → erwartet **RESTATE**;
  (ii) **„No-Go-Einträge bearbeiten"** → erwartet **NEW_RELATED** (Feature No-Go);
  (iii) **echtes neues Feature** (z.B. „Schichtübergabe-Notiz") → erwartet **NEW**;
  (iv) **Widerspruch** zu einer bestehenden Aussage → erwartet **CONTRADICT → Open Decision**.
- Ablauf: L1–L3 auf Meeting B → MeetingDelta → `ingest-requirements` → Plan zeigt die vier Operationsarten
  korrekt → Review → Apply. **Robustheits-Zusatzlauf optional:** `T9999_chaos.txt` als messy Delta.
- **Beleg** = §0-DONE (die vier Operationsarten korrekt + Idempotenz-Re-Run).

## 7. MAF-Nativität

- Resolver = **MAF-Workflow** (`Executor<T>` + `AddEdge` + `[SendsMessage]`/`[YieldsOutput]`), Tools via
  `AIFunctionFactory`, Chat-Pipeline via `AgentChatPipelineBuilder` — identisch zu Cluster/Clarify.
- Gate/Apply/Seed/Retriever(Stufe 0) = deterministische Host-Logik (bewusst, sicherheits-/reproduzierbarkeits-
  kritisch). Retriever hinter Port → spätere Embeddings-Stufe MAF-neutral.
- StateChangePlan = **Wiederverwendung** des operationsbasierten Musters (`ClusterOperation`), nicht neu erfunden.

## 8. Offene Entscheidungen (bitte bestätigen)

- **D1 Core-Ort/Historie:** `state/core/project-state.json` (ein lebender File) + `history[]` im Item + Audit-
  Snapshot je Ingestion unter `runs/ingestion/<run>/`. *(Empfehlung; Alternative: Snapshot-Verzeichnis statt
  In-Item-History.)*
- **D2 Modell:** Core = erweiterte `ProjectStateDocument` (Reuse) statt Parallelmodell. *(Empfehlung, §1.)*
- **D3 Downstream-Scope in Inc 1:** nur **affected-view emittieren**; L4/re-clarify darauf umstellen = **Inc 1b**.
  *(Empfehlung — hält Inc 1 klein/testbar.)*
- **D4 — GEKLÄRT:** `project-state-build` = deterministischer Rebuild aus den *übergebenen* Artefakten (keine
  Historien-Aggregation), ItemIds stammen aus den Artefakten. → **MeetingDelta = `project-state-build` auf den
  Artefakten des neuen Transkripts**; keine bestehende Aggregation zu ersetzen. Zusatz: **Core = ID-Autorität**,
  eingehende itemIds sind meeting-lokal (siehe §1). Kein Blocker.

## 9. Risiken

- **Identitäts-Fehlurteil** (Agent merged fälschlich) → Beleg-Pflicht + Gate + HumanReview + `history` (reversibel).
- **Feature-Zuordnung** bei NEW_RELATED unscharf → `featureKey` gegen bestehende Cluster prüfen (Gate-Warnung).
- **Idempotenz** verletzt (RESTATE erzeugt Version) → expliziter Gleichheits-Check im Apply (Text-Normalisierung).
- **Scope-Creep** (Inc 1 zieht PBIs/Mappings rein) → strikt nur `ItemType=requirement` in Inc 1.

## 10. Review-Schärfungen (Kollege, nach Block D) + Prüfstein-Reihenfolge

Block D belegte den Resolver isoliert (5/5, Idempotenz NEW=0). Review benennt drei Lücken zwischen „Mechanismus
verifiziert" und „trägt den ganzen Workflow" — alle zutreffend:

- **P1 — Integration Extraktion → MeetingDelta (härtester Prüfstein).** Block D nutzt eine autoren-gestellte
  MeetingDelta. Echter Beweis: neues Transkript → Extraktion → `project-state-build` auf DESSEN Artefakten
  = MeetingDelta → Ingestion. Zusatzwert: testet Resolver-Robustheit gegen **verrauschten** Extraktions-Output
  (nicht nur kuratierte Sätze). Falle: frische Extraktion vergibt eigene IDs → Delta-Build darf NICHT mit
  Meeting 1 aggregieren (Core = ID-Autorität fängt es ab, aber die saubere Delta-Erzeugung ist zu verifizieren).
  **Machbarkeit geprüft:** Kette existiert (aktueller Core: `recipe/…requirements.artifact.json` +
  `architecture.artifact.json` → `project-state-build`). P1 = Orchestrierung + LLM-Calls, keine neue Fähigkeit.
  **Scope-Schärfung (Fund):** MeetingDelta = **nur L1/L2-Extraktion** (geerdet, „was gesagt wurde"). **L3/Open-
  World gehört NICHT in die Per-Meeting-Delta** — Gap-Detection ohne Core-Kontext halluziniert; Open-World wird
  core-bewusst an einer späteren Stelle behandelt (nach der Ingestion, gegen den Core).
- **P2 — Open Decisions dem Resolver sichtbar machen (zwei getrennte Dinge):**
  - **P2a (klein, sofort):** Retriever/Tools zeigen nur `itemType=requirement`. Nach CONTRADICT ist die `DEC-*`
    beim Re-Lauf unsichtbar → Resolver schlägt erneut vor (beobachtet: M-5 kippte). Fix: Retriever um Decisions/
    superseded erweitern + Ausgang „already_open_decision". Entfernt die Re-Run-Wart.
  - **P2b (eigener Increment):** echte **Decision-Ingestion (Tor 2)** — Stakeholder beantwortet Open Decision →
    auflösen + betroffene Items updaten.
- **P3 — `part_of_feature` zeigt auf losen String, langfristig zu schwach.** Reproduziert die Identitätsfrage auf
  Feature-Ebene (Synonyme/Umbenennung). Ziel: Features als **erstklassige Entitäten** (`itemType=feature`,
  stabile ID + identityKey), `part_of_feature` zeigt darauf. = dasselbe retrieval-then-judge-Muster eine Ebene
  höher. Für Inc 1 String ok; darf nicht ossifizieren. Interim: featureKey normalisieren, beim Cluster-Eintritt
  gegen bestehende Cluster-IDs auflösen. (Ergänzt `plan-core-ingestion.md` §4.)

Eigene Ergänzungen (nicht vom Review): **Mess-Hygiene** — „5/5" ist eine Stichprobe; N≥3 für Median/Varianz.
**SUPERSEDE agentisch unbewiesen** — Block D hatte keinen SUPERSEDE-Fall. **affected-view unkonsumiert** —
„trägt den Workflow" erst mit Inc 1b end-to-end wahr.

### Empfohlene Reihenfolge (begründet)
1. **P1 — echte L1-L3-Integration** (NÄCHSTER Bau). Load-bearing: isoliert→integriert; **Voraussetzung für Inc 1b**
   (Downstream soll auf einem real gefütterten Core sitzen, nicht auf autoren-Deltas); testet echte Robustheit.
   Vorab kurze Machbarkeit prüfen: wie erzeugt man L1/L2/L3-Artefakte für ein neues Transkript heute?
2. **P2a — Decision-Sichtbarkeit** (klein, danach/parallel; macht Idempotenz-Re-Runs ehrlich).
3. **Inc 1b — L4/re-clarify auf affected-view** (macht „trägt den Workflow" end-to-end).
4. **P3 — Feature-als-Entität** (landet mit dem Cluster-Eintritt in den Core).
5. Begleitend **N≥3 Mess-Hygiene** + SUPERSEDE im echten Lauf.
