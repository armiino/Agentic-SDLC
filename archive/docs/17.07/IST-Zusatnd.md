# IST-Zustand (Stand 2026-07-18) — Wo stehen wir, wie funktionieren wir, wie stabil, DB ja/nein

Dieses Dokument ersetzt die frühere IST-Diagnose (17.07), die den fehlenden RE-/Backlog-Schritt aufgedeckt hat.
Dieser Schritt (`l4-re-clarify`) ist inzwischen **gebaut und end-to-end belegt**. Der frühere Kernbefund
("traceable Requirements ≠ stabiler Product Backlog") ist damit adressiert. Hier geht es jetzt um eine
**objektive Standort- und Stabilitätsbestimmung vor dem GitHub-Agenten** — insbesondere die Frage:

```text
Muss der DB-Schritt VOR dem GitHub-Agenten passieren,
oder reicht die aktuelle JSON-/ProjectState-Umgebung erst mal aus?
```

---

## 0. Kurzfazit vorab (die Antwort)

- Die Kette **Transkript → … → GitHub-Issue** ist end-to-end gebaut und real belegt (Lauf
  `20260717_210758_f37eac`: 12 Cluster → 27 PBIs → **26 Issues #40–#65**).
- Die **Rückverfolgbarkeit bis L1/Claim ist jetzt deterministisch** im PBI verankert (Traceability-Enrichment,
  siehe §4) — nicht mehr agenten-abhängig.
- **Der GitHub-Reconciliation-Agent v1 ist bereits gebaut** (`github-reconciliation agent`) und als MAF-Workflow
  verdrahtet. Ein DB-Schritt war und ist **KEINE Voraussetzung**. Grund: die Agenten lesen
  über **Repository-/View-Ports** (JSON jetzt, DB später austauschbar) — genau dafür ist die Boundary da.
- Einordnung: v1 beweist den plan-only Agentenpfad; die **volle Zielrolle** ist der Umwelt-Agent zwischen
  wachsendem ProjectState/ProductBacklog/Traceability und lebendem GitHub (MCP/Issues/Kommentare/Status).
- Vor dem **lebenden Delta-Betrieb** (Sprint 2 aktualisiert bestehende Issues statt Duplikate) fehlen **zwei
  kleine JSON-Nachbesserungen** (stabile PBI-IDs + persistiertes Mapping) — **weiterhin kein DB nötig**.
- Eine **echte DB-Migration** ist heute noch **nicht sauber möglich** (Write-Seite + Persistenz gehen noch an
  Dateien vorbei am Port), aber sie ist auch **nicht nötig**, um weiterzumachen. Details §6/§7.
- Der noch offene Teil ist nicht "GitHub-Agent bauen?", sondern **GitHub-Agent objektiv messen**: Drift-Fixtures,
  Gold-Labels, Vergleichsharness und mehrere Agentenläufe.

---

## 1. Wo stehen wir — die gebaute Kette

```text
Transkript / Ledger
  → L1/L2  belegte Requirement-/Architektur-Items      (ProjectState: REQ-*, ARCH-*)
  → L3     Gap-Detection / Open-World                   (L3-REQ-*, CAND-*, HDEC-*)
  → ProjectState (JSON, Repository-/View-Port)
  → L4     Canonical Requirements Baseline              (CAN-REQ-*, CAN-OPEN-*)
  → l4-re-clarify:
       (a) Cluster        Agent+CoverageGate+ReviewAgent → FeatureCluster (FC-*)
       (b) cluster-review/apply (operationsbasiert, HumanReview-UI)
       (c) Clarify        Agent → Product-Backlog-Items (PBI-FC###-##)
       (d) backlog-review/apply → ProductBacklogView (applied) + det. Traceability-Enrichment
  → IssuePlanning  Projektion PBI→IssuePlanItem (IPLAN-*)
  → GitHub Reconciliation  Agent-v1 ODER deterministischer Seed (GHACT-*) → Gate → HumanReview → gated Write
  → GitHub Issues (#…)  + Mapping pbiId↔issueNumber
```

**Was wir können (belegt):** aus messy Transkript eine kanonische, geclusterte, zu testbaren PBIs geklärte
Backlog-Sicht erzeugen, deterministisch auf Issues projizieren, gegen ein reales Repo abgleichen und
kontrolliert schreiben — mit durchgehender ID-Kette bis zurück zum Claim.

**Warum der GitHub-Agent trotzdem wichtig ist:** Beim ersten Transkript kann der deterministische Pfad reichen.
Beim zweiten/folgenden Transkript wächst der ProjectState: neue Aussagen konkretisieren, widersprechen oder
wiederholen frühere Punkte; GitHub enthält bereits Issues, Kommentare, Status und menschliche Änderungen. Dort
soll der Agent explorieren: "gibt es dazu schon ein Issue?", "wurde das fachlich schon behandelt?", "ist dieses
geschlossene Issue wirklich erledigt?", "muss ich erstellen, aktualisieren, verlinken oder wieder öffnen?".
Das ist die eigentliche agentische Forschungsstelle; die aktuelle v1 ist der erste plan-only Durchstich.

**Muster durchgängig:** `Agent[Tools] (Maker) → deterministisches Gate → HumanReview → deterministischer Apply`.

---

## 2. Wie wir funktionieren — MAF-Nativität (Agent / Workflow / Determinismus)

| Knoten | Typ | MAF-/Technik-Konstrukt |
|---|---|---|
| L1/L2 Extraction | Agent + det. Validierung | ChatClient + Checker |
| L3 Coverage | Agent (routed) + Gate + Human | Agent-Tools, det. Anchor/Judge |
| **Cluster** | **Agent** in **MAF-Workflow** | `Executor<T>` + `AddEdge` + `[SendsMessage]`/`[YieldsOutput]`, Tools via `AIFunctionFactory` |
| Cluster-Gate | **deterministisch** (kein LLM) | Coverage-Prüfung |
| Cluster-Review | Agent (ReviewAgent) + Human | operationsbasiert, generische HumanReview-UI |
| **Clarify** | **Agent** in **MAF-Workflow** | Cluster→PBIs, Tools + DoR-Gate-Vorpass |
| Backlog-Gate (DoR) | **deterministisch** | Coverage + NO_TESTABILITY |
| Backlog-Apply | **deterministisch** | Materialisiert ProductBacklogView |
| **Traceability-Enrichment** | **deterministisch** | aus Provenienz, kein LLM (§4) |
| IssuePlan-Projektion | **deterministisch** | 1 PBI → 1 IssuePlanItem |
| GitHub-Reconciliation | **Agent v1 gebaut + det. Baseline gebaut** | `GithubReconciliationWorkflow`: Agent[Tools] → Gate → Finalize; Seed-Scorer als Baseline |
| GitHub-Write | **deterministisch** | Preflight + Quality-Gate + schmaler Client |

**Ehrliche Einordnung:** Agentisch dort, wo Bedeutung/Ambiguität regiert (Cluster, Clarify, Reconciliation).
Deterministisch dort, wo es mechanisch + sicherheitskritisch ist (Gates, Projektion, Enrichment, Write).
Der **Workflow-Rahmen** (Cluster/Clarify/GitHub-Reconciliation) ist MAF-nativ (`WorkflowBuilder`, Executors,
Edges, Tools). Die
Governance-/Materialisierungs-Schritte sind bewusst Host-seitig deterministisch.

---

## 3. Wie alles rückverfolgbar ist — wo IDs entstehen

```text
Ebene              ID              entsteht durch            Stabilität
─────────────────────────────────────────────────────────────────────────────
Transkript-Aussage claim-id        Ledger (L1-Extraktion)    deterministisch, stabil
L1/L2 Item         REQ-*, ARCH-*   Extraktion → ProjectState deterministisch, stabil
L3 Promotion       L3-REQ-*,CAND-* L3 + Human-Accept         deterministisch, stabil
Canonical Req      CAN-REQ-*       L4 Baseline-Builder       deterministisch, stabil
Feature-Cluster    FC-*            Cluster-Agent (identityKey)  agenten-vorgeschlagen (identityKey = Anker)
Product-Backlog-Item PBI-FC###-##  Clarify-Agent             ⚠ agenten-vergeben (NICHT re-run-stabil)
IssuePlanItem      IPLAN-*         det. Projektion           deterministisch, stabil
GitHub-Aktion      GHACT-*         Reconciliation            deterministisch
GitHub-Issue       #<n>            GitHub                    extern
Mapping            pbiId↔#<n>      Write                     ⚠ nur run-Artefakt, nicht im Store
```

**Wie man die Kette läuft (belegt an No-Go):**

```text
Issue #… "No-Go-Seite anzeigen"
  → pbiId PBI-FC006-01  (Issue-Body / Mapping)
  → PBI.requirementIds  [CAN-REQ-027, …]
  → CAN-REQ-027.sourceItemIds  [REQ-15, REQ-17]          (canonical baseline)
  → ProjectState REQ-15.sourceClaimIds  [canon_no_go_page] (origin=Extracted, sourceRunId=…)
  → Claim canon_no_go_page → L1-Extraktionslauf → Transkript
```

Jeder Sprung hat **explizite IDs**. Die meisten sind **deterministisch und stabil**; zwei Stellen sind
agenten-/laufabhängig (PBI-ID, Mapping-Ort) — das ist der Hebel für den *lebenden* Betrieb (§6/§7).

---

## 4. Traceability-Enrichment (neu) — PBI selbst-enthaltend bis L1

Befund: das agenten-gefüllte `PBI.traceability`-Feld war unpräzise (`l1Req`=CAN-REQ, `claims`=Prosa).
Fix (`ReClarifyTraceabilityEnricher`, beim `backlog-apply`, **kein LLM**): setzt die Traceability
deterministisch aus der Provenienz: `CAN-REQ → baseline.sourceItemIds → item.sourceClaimIds`.

```text
PBI-FC006-01 (applied):  l1Req=[REQ-15, REQ-17]  l3=[L3-REQ-002/006/011]  claims=[…, canon_no_go_page]
```

Damit ist das PBI **selbst-enthaltend** bis L1/Claim rückverfolgbar. Die rohe `product-backlog.json` behält das
Agentenfeld (= Roh-Output); die **applied** ProductBacklogView (autoritativ) ist angereichert.

---

## 5. ProjectState / JSON-ID — Handhabung & Stabilität

**Wie gehandhabt:** JSON-first (`project-state.json`: Items + Relations + Provenance) hinter zwei Ports:
- `IProjectStateViewRepository` — typisierte, fachliche **Views** (`CanonicalRequirementsView`,
  `ProductBacklogView`) über `ProjectScope`.
- `IProjectStateRepository` — speicher-neutrale Ops (u. a. `SaveProposal`/`ApplyDecision`).
- Implementierung: `JsonProjectStateRepository` / `JsonProjectStateViewRepository`.

**Was stabil ist:**
- IDs L1→CAN-REQ sind deterministisch, Provenienz (Run/Artefakt/Claim/Candidate/HumanDecision) bleibt erhalten.
- **Lesen** läuft sauber über den View-Port → DB-tauschbar (der Kern der Boundary funktioniert).
- Innerhalb eines Laufs reproduzierbar; Gates/Projektion/Enrichment/Apply deterministisch.

**Was (noch) nicht Store-fest ist — belegt am Code:**
- Die re-clarify-Ergebnisse (**PBIs, Cluster, Mapping**) sind **Run-Artefakte** (`runs/l4-re-clarify/<run>/…`),
  **keine persistenten ProjectState-Entitäten**. Es gibt keinen `project-state.json`-Write dieser Objekte.
- **Schreiben umgeht den Port:** `ReClarifyBacklogApplyRunner` schreibt via `File.WriteAllTextAsync` in
  Run-Ordner — **nicht** über `IProjectStateRepository`-Write-Ops.
- **`ProjectScope` ist pfad-gekoppelt:** Auflösung via `ProjectScope.FromSourcePath(<pfad/runId>)` +
  Verzeichnis-Heuristiken (`ResolveBacklogDir`). Eine DB würde per Query auflösen, nicht per Pfad.
- **PBI-IDs agenten-vergeben** → über Re-Runs nicht stabil (keine Identitätsfunktion/Registry aktiv; plan-pb Flag A).

**Stabilitätsurteil ProjectState:** als **Read-Backbone + Audit** stabil und DB-vorbereitet; als **persistenter,
über Sprints lebender PBI-/Mapping-Store** noch nicht — das ist bewusst „DB-later", aber es ist der Punkt, der
den *lebenden* Delta-Betrieb heute begrenzt.

---

## 6. DB-Migration — ist sie jetzt möglich? Was fehlt?

**Read-Seite:** ja, migrierbar. Ports + Views kapseln die Quelle; eine DB-Implementierung hinter
`IProjectStateViewRepository`/`IProjectStateRepository` würde die Knoten nicht anfassen.

**Write-/Persistenz-Seite:** **noch nicht** sauber migrierbar. Vier konkrete Lücken:

```text
1. PBIs / Cluster / Mapping sind Run-Views, keine persistenten Entitäten     → DB bräuchte Tabellen/Collections.
2. PBI-ID nicht stabil (agenten-vergeben)                                    → DB-Identität/Upsert nicht möglich.
3. Writes gehen an File.WriteAllText, nicht über Repository-Write-Ops        → Write-Port wird umgangen.
4. ProjectScope pfad-gekoppelt (FromSourcePath + Verzeichnis-Heuristik)      → DB-Scope wäre query-basiert.
```

**Urteil:** Eine **vollständige** DB-Migration ist heute **nicht ohne Vorarbeit** möglich (Punkte 1–4). Aber
keiner dieser Punkte ist **jetzt** nötig, und keiner erzwingt eine DB — sie sind auch in **JSON** lösbar.

---

## 7. Muss die DB VOR dem GitHub-Agenten kommen? — Objektive Entscheidung

**Nein.** Begründung:

- Der GitHub-Reconciliation-Agent (v1) braucht zwei **Read**-Umwelten: den akzeptierten IssuePlan bzw. die
  PB-/State-View (über den Port — JSON jetzt) + GitHub-Snapshot/Mappings. Er **liest** über die Boundary;
  ob dahinter JSON oder DB liegt, ist ihm egal. Genau **dafür** existiert die Repository-/View-Boundary →
  die DB ist **deferrable**.
- v1 macht **Maker → Gate → HumanReview → gated Write** und **keinen** Write-Back in den State. Er braucht also
  **weder DB noch Write-Port-Vollausbau**.
- Der **Vergleichspfad** (Github-Agent.md §8, agentisch vs. deterministisch) kann komplett auf der
  **aktuellen JSON-Umgebung** laufen — das Testbett (`armiino/Agentic-GitHub`, 26 Issues + Mapping-Artefakt)
  existiert. Was noch fehlt, ist der Mess-Harness/Gold-Standard, nicht die DB.

**Was v1 NICHT braucht:** DB, Write-Back-to-State, ProjectScope-Entkopplung.

**Was v1 aktuell noch NICHT beweist:** stabiler Sprint-Delta-Betrieb und die volle Umwelt-Exploration.
Ohne persistierte PBI-Identität und Mapping-Registry kann ein Folgelauf vorhandene Issues noch nicht zuverlässig
als dieselbe fachliche Einheit erkennen, wenn IDs/Issue-Bodies driften. Außerdem liest v1 heute primär
AcceptedIssuePlan + Snapshot/Mappings; die Zielausbaustufe braucht direkte ProjectState-/PBI-/Traceability-Tools
und optional GitHub-MCP-Read-Tools.

**Was der spätere LEBENDE Delta-Betrieb braucht — zwei kleine JSON-Schritte, kein DB:**

```text
a) Stabile PBI-Identität:  identityKey → stabile pbiId über Re-Runs (plan-pb Flag A) — damit Sprint 2
                           dasselbe PBI wiedererkennt statt neu zu nummerieren.
b) Mapping persistieren:   pbiId↔issueNumber an einen adressierbaren Ort heben (heute nur Run-Artefakt) —
                           Basis für Dedup/Update im nächsten Lauf.
```

**DB erst dann, wenn ein realer Engpass eintritt:** Mehr-Projekt-/Mehr-Sprint-Historie, Query über viele
Läufe, Nebenläufigkeit/Concurrency, Transaktionssicherheit beim Write-Back. Nichts davon trifft heute zu.
Wenn es soweit ist, macht die Boundary es zum **Austausch**, nicht zum Umbau — **vorausgesetzt** die vier
Punkte aus §6 sind bis dahin sauber (Writes über den Port, Scope entkoppelt, PBI/Mapping als Entitäten).

---

## 8. Gesamt-Stabilität & Baubarkeit des GitHub-Agenten

**Deterministischer Kern:** stabil, reproduzierbar (Gates, Projektion, Apply, Enrichment, Write-Preflight).
Build durchgängig 0 Fehler.

**Agentische Knoten:** dokumentierte **Varianz** (Cluster/Clarify — z. B. 12 vs. 16 Cluster über Läufe;
Clarify über-blockiert als `stakeholder_decision`). Das ist **Forschungsgegenstand**, kein Blocker; Mess-Hygiene
(mehrere Läufe/Median) und Prompt-Kalibrierung stehen aus.

**GitHub-Agent baubar?** **Ja — v1 ist bereits gebaut und verdrahtet.** Empfohlene Vorarbeit ist **nicht** die DB,
sondern die zwei JSON-Schritte aus §7, **falls** man direkt den Delta-/Dedup-Fall belastbar messen will. Für den
reinen Vergleichspfad (§8 der Github-Agent.md) reicht der Ist-Zustand technisch aus; methodisch fehlen noch
Drift-Fixtures, Gold-Labels, Scoring und Mehrfachläufe.

---

## 9. Empfohlene Reihenfolge

```text
1. re-clarify-/GitHub-Reconciliation-Strang ist mit Commit `5e5cbe942a066f3b93b30d8c4c648c21a43d415d`
   eingefroren (ohne `.gitignore`, docs bleiben lokal).
2. (klein, optional, JSON — KEIN DB) stabile PBI-Identität + Mapping-Persistenz → macht Delta testbar.
3. GitHub-Agent-Vergleichspfad fertigstellen (Github-Agent.md §8): Drift-Fixtures, Gold-Labels, Compare-Harness.
4. Prompt-Kalibrierung Clarify + Mess-Hygiene (parallel/danach).
5. DB erst bei echtem Engpass (§7) — dann als Port-Austausch, nachdem §6.1–6.4 sauber sind.
```

**Kernaussage:** Der aktuelle Stand ist stabil genug, um den GitHub-Agenten zu bauen. Die DB ist bewusst
verschoben und **nicht** blockierend — die Repository-/View-Boundary ist genau das Instrument, das diese
Verschiebung erlaubt. Was vor dem *lebenden* Betrieb zählt, ist Identitäts-/Mapping-Stabilität in JSON, nicht
die Datenbank selbst.
