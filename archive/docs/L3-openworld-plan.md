# L3 — Open-World-Ableitung: Plan (Reframe + MAF-nativer, agentischer Human-in-the-Loop)

_FUTURE PLAN, 2026-07-13. Nicht implementiert. Fasst zwei Bewertungsrunden (`L3-Openworld-bewertung.md` + Folge-Review)
ein: 4-Klassen-Routing mit präziser Semantik, Gen/Anker-Trennung, deterministische Anker-Aggregation, strukturierter
Judge, Human-Decision-Provenienz, zwei-phasiger (prepare/apply) Human-Loop. v1-Scope bleibt bewusst minimal. Erweitert die
Grenz-Notiz in `capstone-A-demonstration.md` §5._

## 0. Das Reframe (warum L3 nicht „außer Scope" bleibt)

Bisher: L1 = verankerte Elaboration (gebaut), L3 = open-world Gap (un-verankerbar → zurückgestellt). **Einsicht:**
„open-world" ist meist ein Missname. Im **geschichteten** Umfeld ist das meiste, was nach „neuem Requirement" aussieht,
verankert: steht da „Login-Screen", folgen daraus Eingabefelder, Validierung, Sperr-Logik — die **hängen an** dem
Login-Item (L1/L2), sind nicht open-world.

**Präzise (Folge-Review):** „open-world" ist NICHT „dockt an gar nichts an". Ein Vorschlag kann thematisch an ein Item
anknüpfen und trotzdem zentrale **neue, nicht getragene** Festlegungen enthalten (Hardware-Token-MFA dockt thematisch an
„Login-Screen" an, wird von ihm aber nicht getragen). **Open-World-Anteil = die Teile eines Vorschlags, die durch keinen
bestehenden Anker semantisch getragen werden — unabhängig von loser thematischer Nähe.**

**Der eigentliche Beitrag** ist NICHT „mehr Requirements erzeugen", sondern die **explizite Einordnung des epistemischen
Status** jedes Vorschlags (getragen / schwach / unverankert / widersprüchlich) plus die kontrollierte Übergabe des
unbeweisbaren Rests an den Menschen. L3 = **Proposal-, Anchor-Resolution- und Human-Adjudication-Pipeline**, kein
„Open-World-Generator".

## 1. Agentisch UND kontrolliert (Grundhaltung — unverändert)

**So viel Agency wie fachlich sinnvoll, so viel Determinismus/externe Kontrolle wie für Nachvollziehbarkeit nötig.**
Arbeitsteilung (Mikro-Agency vs. Makro-Orchestrierung, wie im account-verify/Reflect-Motor):

| Aufgabe | Wer | Charakter |
|---|---|---|
| Umwelt explorieren, Kandidaten generieren, Anker suchen, Revision formulieren | **Agent** | agentisch |
| Anker-IDs auf Existenz/Zulässigkeit prüfen | deterministischer Executor | deterministisch |
| semantische Tragfähigkeit **je Anker** bewerten | unabhängiger Judge (InferenceChecker-Kern) | bounded LLM |
| **Anker-Urteile aggregieren + Routing-Klasse vergeben** | deterministischer Routing-Executor | deterministisch |
| open-world-Inhalt **autorisieren** | **Mensch** | human-in-the-loop |
| Entscheidung speichern, IDs vergeben, apply | deterministischer Executor | deterministisch |

Der Agent hat echte Freiheit bei Exploration/Generierung/Ankersuche/Revision. Die **Autorisierung neuer Projektwahrheit**
bleibt außerhalb des Agenten — die Nachvollziehbarkeits-Invariante. Ein Modell darf seine eigenen unbelegten Vorschläge
NICHT zugleich erzeugen, bewerten UND autorisieren.

## 2. Das Statusmodell — 4 Klassen, präzise Semantik

Ein *existierender* Anker beweist nur eine Verbindung, nicht dass er die Aussage **trägt** („Passwort-Reset" zitiert
„Login-Screen" — formal gültig, semantisch nicht zwingend; = der `PROBE-DECOY`-Fall). Zwei Prüfungen:

```text
1. Existiert/zulässig der referenzierte Anker?   → deterministisch (Anker-Validator)
2. Trägt der Anker die konkrete Ableitung?       → semantisch je Anker (Judge)
```

### 2.1 Fehlend ≠ ungültig (Folge-Review)
- **`MISSING_ANCHOR`** (nach Anchor-Resolution kein tragfähiger Anker gefunden) = **erwartbarer** Open-World-Zustand →
  Open-World-Human-Review.
- **`UNKNOWN_ANCHOR`** (referenziert eine **nicht existierende** ID = Halluzination/Defekt) = **kein** Open-World, sondern
  Anker-Defekt → **Repair/Reflect zuerst**: der Agent entfernt/ersetzt den defekten Anker. Erst wenn danach keine
  tragfähige Referenz bleibt, wird der Kandidat als unverankert eingestuft.

### 2.2 `Unclear` ≠ `Unrelated` (Folge-Review)
- **`Unclear`** = möglicherweise tragfähig, Info reicht für keine sichere Entscheidung → echte Unsicherheit.
- **`Unrelated`** = der Anker trägt die Aussage NICHT (dekorativ/thematisch ähnlich) → näher an „unverankert" als an
  „schwach".

→ **`WEAK_OR_UNCERTAIN` basiert ausschließlich auf `Unclear`.** Ein Kandidat, dessen Anker ausschließlich `Unrelated`
sind, gilt als **faktisch unverankert** und geht in den Open-World-Pfad (nicht in „schwach").

### 2.3 Die vier Klassen (Routing)
| Klasse | Bedeutung | Route (v1) |
|---|---|---|
| `SUPPORTED_ANCHORED` | ≥1 Anker trägt, kein Widerspruch | normaler Derived-Item-Pfad |
| `CONTRADICTED` | ≥1 autoritativer Anker echter Widerspruch | **menschliche Konfliktentscheidung** (nicht auto-reject) |
| `WEAK_OR_UNCERTAIN` | kein Anker trägt eindeutig, aber ≥1 `Unclear` | Human-Review (Reflect nur auf `NEEDS_REVISION`) |
| `UNREFERENCED` (open-world) | kein gültiger Anker ODER alle `Unrelated` | Open-World-Human-Review |

### 2.4 Aggregationsregel bei mehreren Ankern (deterministisch im Routing-Executor)
Ein Kandidat kann mehrere Anker mit unterschiedlichen Einzelurteilen haben (REQ trägt, ARCH trägt, RISK unrelated). Dann
ist NICHT automatisch das ganze Item schwach — der dekorative Anker wird entfernt, die tragenden bleiben. Verbindliche
Regel (im Executor, NICHT im Prompt):
```text
CONTRADICTED        wenn ≥1 autoritativer Anker echten Widerspruch enthält
SUPPORTED_ANCHORED  sonst, wenn ≥1 Anker trägt (Supported)          → nicht-tragende Anker verwerfen
WEAK_OR_UNCERTAIN   sonst, wenn ≥1 Anker Unclear
UNREFERENCED        sonst (keine gültigen Anker ODER alle Unrelated)
```

## 3. Pipeline-Phasen (Generierung von Verankerung getrennt — verbindlich in v1)

Wird der Agent gezwungen, schon beim Generieren Anker zu nennen, hängt er jedem Vorschlag *irgendeine* Referenz an → der
Open-World-Zweig schrumpft künstlich (dekoratives Ankern). **Die Trennung ist kein optionales Feature, sondern die
Voraussetzung, um dekoratives Ankern überhaupt messen zu können — daher konsequent schon in v1** (Korrektur der früheren
„erst bei Beobachtung"-Formulierung, die sich mit dem v1-Durchstich widersprach).

```text
A  Kandidaten generieren       (Agent; KEIN Pflicht-Anker)
B  Anker suchen                (Agent; Datenvertrag unten)
C  Anker deterministisch prüfen (Executor: existiert? zulässig? aktiv? für Zieltyp erlaubt?)
D  Tragfähigkeit je Anker       (Judge; strukturierter Output, §4)
E  Aggregation + Routing        (Executor; Regel §2.4)
```

**Anchor-Resolution-Datenvertrag (§B).** Der Resolver liefert je Kandidat nicht nur IDs, sondern je Anker die **behauptete
Beziehung** + **Begründung**; findet er keinen, begründet er das explizit:
```json
{ "candidateId": "CAND-001",
  "proposedAnchors": [
    { "itemId": "REQ-12", "relation": "elaborates", "reason": "konkretisiert den Login-Screen" },
    { "itemId": "ARCH-03", "relation": "depends_on", "reason": "…" } ],
  "noAnchorReason": null }
```
Relationstypen (v1-Set): `elaborates | depends_on | mitigates | conflicts_with | relates_to`. Das verbessert Judge-Qualität
UND Auditierbarkeit.

## 4. Der L3-Judge — strukturiert, je Anker, mit eigener Probe

Wiederverwendung des `InferenceChecker`-Kerns (temp=0, bounded), aber **eigener L3-Maßstab** (Prompt, 4. Config-Achse wie
cite-fidelity). Der Judge bewertet **je (Kandidat, vorgeschlagener Anker)** die Tragfähigkeit — er entscheidet NICHT, ob
lose thematische Ähnlichkeit „reicht", sondern nur, ob der **bereits vorgeschlagene** Anker die Aussage trägt.

**Strukturierter Output (Folge-Review) — geht über den heutigen 4-Verdikt-Checker hinaus (ehrlich: echte Erweiterung, kein
reiner Reuse):** statt nur `verdict` zusätzlich `supportedClaims`, `unsupportedClaims`, `assumptions`, `contradictions`,
`rationale`. Grund: Requirements sind oft teils getragen/teils offen; das gibt dem Reflect-Agenten gezieltes Feedback und
dem menschlichen Reviewer eine bessere Grundlage. **v1 darf mit `verdict`+`rationale` starten** und die Felder wachsen
lassen.

**Eigen-Probe (wie cite-fidelity `--probe`):** kontrollierte L3-Testfälle — klar gestützte Ableitung, unsichere
Erweiterung, dekorativer Anker, echter Widerspruch — belegen, dass der L3-Judge diese Fälle unterscheidet.

## 5. MAF-native Topologie — Batch + Finalize + zwei Phasen

Typisierte Executors + konditionale Kanten, Bauform wie der bestehende `ReflectGraphWorkflow` (konditionale `AddEdge<T>`,
Loop-Back, Iterationsschranke in der Message), `BindAsExecutor`-fähig.

### 5.1 Batch-Modell
Generation erzeugt **mehrere** Kandidaten; B–E arbeiten pro Kandidat; der Routing-Knoten **zerlegt die Menge** in
Teilmengen je Klasse. Human-Review bekommt EIN gemeinsames Paket; Entscheidungen werden **kandidatenweise** gespeichert.

### 5.2 Graph + Finalize-Join
```text
EnvDiscovery ─► CandidateGen ─► AnchorResolution ─► AnchorValidation ─► SupportJudge ─► Routing ─┐
   (UNKNOWN_ANCHOR ──► Repair/Reflect ──► zurück zu AnchorValidation)                             │
   ┌──────────────────────┬───────────────────────┬────────────────────────┬─────────────────────┘
   ▼ SUPPORTED_ANCHORED    ▼ WEAK_OR_UNCERTAIN     ▼ UNREFERENCED           ▼ CONTRADICTED
   DerivedItemGate         HumanReviewPrep         OpenWorldReviewPrep      ConflictReviewPrep
        └───────────────────────┴───────────────► FinalizeL3Run ◄──────────────┘
```
- Verzweigung über konditionale Kanten auf die Klasse eines typisierten `RoutedCandidate`.
- **`FinalizeL3Run`-Join (Folge-Review):** führt alle Routen zu EINEM Laufresultat zusammen — Gesamtbericht, finale
  Artefakte, Promotion-Mappings, offene Fälle. Ohne diesen Knoten zerfällt der Lauf.

### 5.3 Zwei-Phasen (prepare/apply) — der Human-Loop ist NICHT synchron
Der MAF-Graph bleibt NICHT über eine stunden-/tagelange menschliche Wartezeit aktiv (Folge-Review). Zwei Workflows:
```text
Workflow 1 (prepare):  EnvDiscovery … Routing … *ReviewPrep → human-review-package.json + Zwischenzustand persistieren → ENDE
   ⟨ Mensch bewertet das Paket später (AgenticSdlc.HumanReview-UI) ⟩
Workflow 2 (apply):    human-decisions.json lesen → deterministisch anwenden
                       ODER bei NEEDS_REVISION einen bounded Reflect-Workflow starten (y_t + fb_t)
```
Das ist exakt die vorhandene Adjudikations-`prepare`/`apply`-Mechanik (A-Strang).

## 6. Human-in-the-Loop + Human Decision als Provenienzquelle

**Human Decision = eigenes, auditierbares Artefakt.** Provenienz eines akzeptierten open-world-Items:
```text
REQ-27  →  HDEC-001  →  CAND-001  →  Agentenlauf + damaliger Projektzustand
```
So wird aus einer unbelegten Agentenaussage eine nachvollziehbare **menschliche** Projektentscheidung — NICHT als
transkript-abgeleitet getarnt.

**v1-Operationen — präzise (Folge-Review):**
- **`accept`** = **Autorisierung eines verbindlichen Projektartefakts** (eindeutig, nicht „nur Vorschlag"). Erzeugt Human
  Decision + stabile ID, downstream nutzbar.
- **`edit`** = die menschlich bearbeitete Fassung wird verbindlich; der Agent darf sie danach **nicht** eigenmächtig ändern.
- **`reject`** = auditierbar archiviert, aus aktiven Downstream-Prozessen ausgeschlossen.

**`CONTRADICTED` wird NICHT automatisch verworfen** (Folge-Review): es geht in eine **menschliche Konfliktentscheidung** —
daraus wird später *Ablehnung* ODER ein *Change Request*. Grund: echte Projekt-Weiterentwicklungen dürfen nicht als
Modellfehler behandelt werden. (v1: nur flaggen + an Mensch; voller Change-Request-Pfad = v2.)

**Deferred (v2):** `ACCEPT_AS_PROPOSAL` (Idee vs. verbindliche Anforderung), `LINK_AND_ACCEPT`, `DEFER`,
`REQUEST_STAKEHOLDER_CLARIFICATION` (→ Open Question), `NEEDS_HUMAN_RESOLUTION`.

## 7. Persistente IDs (referenzierbar für Issues) — v1 pragmatisch

Ziel: jedes geschriebene Item bekommt eine **stabile, eindeutige ID** → referenzierbar für Issue-Orchestrator, Reviews,
Cross-Links. Fundament: `ArtifactItem.ItemId` ist bereits persistent.
- **v1:** Promotion-Beziehung `OREQ-05 → promotedTo REQ-27`; Kandidaten-ID bleibt auditierbar, Finale-Item stabil,
  Human-Edit erhöht **Version**, nicht Identität.
- **Deferred (v2):** `entityId` (ULID) + `displayId` + `version` (volle Trennung technischer/fachlicher Identität).

## 8. Wiederverwendung vs. Neu

| Baustein | Status | Rolle |
|---|---|---|
| Explorer/`--env` + `list_artifacts/search_items/get_item` | gebaut | EnvDiscovery / CandidateGen-Umwelt |
| DerivationSpec (config-Ziel: reqs/risks/…) | gebaut | CandidateGeneration, config-gesteuert |
| Anker-Validierung (`MISSING`/`UNKNOWN_ANCHOR`) | gebaut | AnchorValidation (§2.1) |
| `InferenceChecker` (Supported/Contradicts/Unrelated/Unclear) | gebaut | SupportJudge-Kern (§4) |
| Reflect-Graph (konditionale Kanten, Loop-Back, `BindAsExecutor`) | gebaut | Topologie + UNKNOWN-Repair + NEEDS_REVISION |
| HumanReview-UI + prepare/apply | gebaut (A-Strang) | zwei-Phasen-Human-Loop (§5.3/§6) |
| `ArtifactItem.ItemId` persistent | gebaut | ID-Fundament (§7) |
| **4-Klassen-Routing + Aggregationsregel (deterministisch)** | **neu** | der L3-Kern (§2) |
| **CandidateGen ohne Pflicht-Anker + AnchorResolution + Datenvertrag** | **neu** | Gen/Anker-Trennung (§3) |
| **strukturierter L3-Judge-Output + eigene Probe** | **neu (erweitert InferenceChecker)** | §4 |
| **`search_anchor_candidates`** (v1: Keyword; v2: semantisch) | **neu** | §3 B — Retrieval-Lücke ehrlich benannt |
| **Human-Decision-Artefakt + Provenienzkette + FinalizeL3Run** | **neu (teilw. A-Strang)** | §5.2/§6 |

## 9. Die verbleibende ehrliche Grenze (Forschungsergebnis)

Open-World bleibt **maschinell nicht garantierbar.** Die Maschine kann generieren, explorieren, Anker suchen/validieren,
semantisch bewerten, unsichere/unverankerte Fälle sichtbar isolieren und Feedback in Revisionen umsetzen. Sie kann NICHT
garantieren, dass alle fehlenden Requirements gefunden, unverankerte richtig, oder der offene Raum vollständig untersucht
sind.

> Open-World-Vollständigkeit bleibt nicht maschinell beweisbar. Ein agentisches System kann den offenen Raum strukturiert
> explorieren, Vorschläge erzeugen, ihren epistemischen Status klassifizieren und nicht belegbare Inhalte kontrolliert an
> eine menschliche Autorität übergeben.

Der Mehrwert liegt in der nachvollziehbaren **Arbeitsteilung** (Agent · Determinismus · unabhängiger Judge · Mensch), nicht
in einer vorgetäuschten Vollständigkeitsgarantie.

## 10. v1-Festlegungen (offene Design-Entscheidungen entschieden)

- **`WEAK_OR_UNCERTAIN` → standardmäßig direkt an den Menschen.** Reflect startet NUR, wenn der Mensch `NEEDS_REVISION`
  wählt — sonst optimiert das Modell Unsicherheit bloß sprachlich weg.
- **`UNREFERENCED` rein strukturell:** nach abgeschlossener Anchor-Resolution existiert kein gültiger vorgeschlagener Anker
  (oder alle `Unrelated`). Der Judge beurteilt nur Tragfähigkeit vorgeschlagener Anker, nicht „reicht lose Ähnlichkeit".
- **L3-Judge:** eigener Maßstab + eigene Probe-Fälle (§4).
- **Erster Durchstich endet NICHT beim Routing-Report,** sondern erzeugt bereits ein verständliches Human-Review-Paket
  (damit die praktische Nutzbarkeit der Klassifikation prüfbar ist).

## 11. v1-Scope + Implementierungsreihenfolge

**Kleinster fachlich vollständiger v1-Durchstich (nur Requirements):**
```text
(synthetische/vorgegebene Kandidaten) → AnchorResolution → deterministische Validierung → L3-Judge
→ Aggregation + 4-Klassen-Routing → Routing-Report → dateibasiertes Human-Review-Paket
```
Apply + Reflect-Rücklauf danach. Dateibasiert (`runs/l3/<runId>/`: `candidates.json`, `anchor-resolution-report.json`,
`support-verdicts.json`, `routing-report.json`, `human-review-package.json`, `human-decisions.json`, `promoted-items.json`,
`reflect-archive/`), reproduzierbar, ohne DB.

**Reihenfolge (Folge-Review — Inkonsistenz behoben):**
1. **Routing-Komponente mit kontrollierten Test-Kandidaten** (isolierter Komponententest, so benannt — nicht „fachlich
   erster Schritt").
2. CandidateGeneration (config-Ziel = requirements) + AnchorResolution anbinden.
3. Erster **End-to-End**-Lauf bis Human-Review-Paket.
4. Deterministisches `apply` (accept/edit/reject) + Promotion-ID.
5. `NEEDS_REVISION` an den bounded Reflect-Loop.

**Bewusst deferred (Overengineering-Vermeidung = Thesenkriterium):** voller Change-Request-Pfad (`CONTRADICTED`), Dual-ID,
semantisches Anker-Retrieval, 5 weitere Human-Ops, weitere Zieltypen (risks …), `get_item_history`/Multi-Sprint.

## 12. Status

FUTURE PLAN. Vor Implementierung sind die Semantiken oben verbindlich (Gen/Anker-Trennung, `Unclear`≠`Unrelated`,
`MISSING`≠`UNKNOWN`, Multi-Anker-Aggregation, `accept`-Bedeutung, Konfliktpfad, Finalize-Join, zwei-Phasen-Loop). Der
kleinste MAF-native Anfang ist die Routing-Komponente (§2.4) mit Test-Kandidaten.
