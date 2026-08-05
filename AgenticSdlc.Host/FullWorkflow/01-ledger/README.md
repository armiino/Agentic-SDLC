# 01-ledger — Transkript → belegte, facettierte Claims (+ Human-Adjudikation)

Erste Stufe der Produktkette (siehe `../README.md`). Input: rohes Transkript. Output: `consumable.json`
— die vom Autor freigegebene Evidenzschicht, von der ALLES Weitere zehrt (02-baselines liest sie via
`run-config.json → evidenceAgent.ledgerRun`). Kernprinzip: kein Claim ohne Evidenz, kein finaler Claim
ohne Freigabe-Weg.

## Ordner

| Ordner | Inhalt |
|---|---|
| `core/` | Basismodelle (`SemanticLedgerEntry` + Evidence/Disposition), deterministischer `TranscriptSegmenter`, Extraktor/Canonicalizer, `SemanticLedgerRecallMatcher` (Messwerkzeug) |
<!-- 05.08. (9g-Mess-Befund): DISPOSITIONS-VERGABE ist jetzt ANGELEITET (vorher nur Schema-Pflichtfeld) — Regel in
     beiden Extraktoren + FacetAssigner + FacetValidator-Prüfregel: open-questions=required NUR bei echter Offenheit
     (must_clarify/must_consider bzw. wörtlich belegte Offenheit); Festgelegtes (must) = not_applicable.
     Wirkung verifiziert (Lauf 20260805_105454: Rauschen 5/5 weg, Gold 4/4). Details: R-Log R-37-Umfeld + aufgefallen 9g. -->
| `build/` | die beiden Build-Runner + `FacetValidationModels` (EntryValidation/ValidatedLedgerEntry) |
| `units/` | High-Coverage-Pfad: Atomic Units, Unused-Pipeline (Triage/Compare/Repair), Traces |
| `gate/` | `LedgerQualityGate` — deterministische Pipeline-Invarianten I0–I6 (kein LLM) |
| `adjudication/` | Queue-Bau, Review-UI-Adapter, Apply + `AdjudicationCompletenessGate`, Refine (A10) |
| `measure/` | Recall/Eval-Werkzeuge gegen Referenz-Fixtures (Thesis-Messachse, nicht Produktpfad) |

## Der Lauf: `ledger-build-units <transcript>` (aktueller Pfad)

EIN MAF-Workflow (WorkflowBuilder-Graph); jeder Step schreibt `runs/ledger/<runId>/step-*/output.json`.
Modellwahl: `jury.judgeModel` aus run-config (Vorrang vor `agentModel`!), Override als 2. CLI-Argument.

**Zwei Bahnen, EIN Kern (seit Schritt 5 ②, 05.08.):** Montage (`LedgerBuildUnitsRunner.CreateWorkflow`,
Clients+Graph) und Auswertung (`EvaluateAsync`, Gate/Trace/Diagnose) sind die geteilten Nähte.
**CLI-Bahn** = `ledger-build-units` (eigener otel-Scope, eigene Ausführung, wie hier beschrieben).
**Graph-Bahn** = pipeline-full bindet DENSELBEN Workflow als sichtbare Kapsel in den Ein-Graph
(`Intake → [LedgerCapsule, BindGateFree] → Summary`): die 8 Steps sind dort Eltern-Supersteps mit
eigenen Checkpoints (Absturz kostet einen Step, nicht die Stufe); Sub-Run-Anker `01-ledger/ledger-run.json`
im Faden hält `runs/ledger/<id>` über Resume konstant; die Ledger-Token laufen dort in den Faden-otel
(metrics.json des Pipeline-Laufs) statt in den Sub-Run. Beleg: Run `20260805_144056_dc711f`
(Kapsel-Sub-Run `_c939cd`, 13 Checkpoints bis zur Adjudikations-Pause).

| Step (Ordner im Run) | Knoten | LLM? | Was passiert |
|---|---|---|---|
| `step-00-atomic-units` | AtomicUnitSegmentationExecutor | ja | Transkript → nummerierte Atomic Units (AU-nnnn) als bounded Coverage-Anker |
| `step-01-candidate` | UnitAwareCandidateExtractionExecutor | ja | Units → Candidate-Claims mit `sourceUnitIds` (Evidenz-Erdung) |
| `step-01b-unit-coverage` | UnitCoverageGateExecutor | nein | Welche Units wurden verwendet? used/unused-Bilanz |
| `step-01c-unused-unit-triage` | UnusedUnitTriageExecutor | ja | unused Units → potentiallyRelevant vs. noise |
| `step-01d-unused-unit-ledger-compare` | UnusedUnitLedgerCompareExecutor | ja | relevante unused Units gegen Candidate-Ledger: `already_covered_indirectly` \| `attach_as_evidence` \| `missing_claim` \| `needs_human` (30er-Batches). **Deckungs-Urteile brauchen ≥1 existierende Candidate-ID** — seit R-1 (2026-07-23) dreifach gesichert: STRENGE-REGEL im Prompt → gezielter Nachfrage-Pass nur für Verstoß-Units → deterministisches Downgrade auf `needs_human` (`UnusedUnitCompareRepair`, Konsole: `[unused-compare] reference-repair: …`) |
| `step-02-canonical` | Canonicalization | ja | Candidates → kanonische Claims mit Cluster-Trace (`candidateIds` + `assumedRelation`) |
| (coverage-repair) | CanonicalCoverageRepairExecutor | ja | heilt `missing_claim`-Funde aus 01d in den kanonischen Ledger |
| `step-03-facet-validation` | FacetValidation | ja | Per-Claim-Verdict gegen das Transkript: `grounded`→approved, sonst `review_required` (deterministisch abgeleitet, `ValidatedLedgerEntry.DeriveStatus`) |
| `gate/` | LedgerQualityGate + Traces | nein | Invarianten I0–I6 (IDs eindeutig, kein Candidate verschwindet still, Evidence-/Trace-Pflicht, Taxonomie, validated-Deckung) + `unused-unit-trace` (Deckungs-Urteile → existierender Ziel-Claim). **error ⇒ GATE FAILED ⇒ nicht weiterfahren** |

Getestet (deterministische Kerne): `TranscriptSegmenterTests`, `LedgerQualityGateTests`,
`LedgerVerdictParsingTests`, `UnusedUnitCompareRepairTests`.

## Danach: Adjudikation (das Human-Gate dieser Stufe)

```text
ledger-adjudicate <step-03…/output.json> <step-01d…/output.json>   # Queue: review_required + missing_claim/needs_human/attach — MISS-SIGNAL NICHT VERGESSEN (R-2)
ledger-adjudicate-ui <step-03b-adjudicated/queue.json>             # DEINE Entscheidungen (Review-UI, Finish → apply)
→ step-03b-adjudicated/{adjudicated-ledger, consumable, gate}.json # AdjudicationCompletenessGate projiziert das Ergebnis
ledger-adjudicate-refine <consumable.json> <transcript>            # nur bei pending>0: neu geminteten Claims Facetten geben (A10)
```

`consumable.json` (pending=0) ist das Übergabe-Artefakt an 02-baselines.

### Die Adjudikations-UI im Detail (identisch im i-Button der UI hinterlegt)

Du bist die letzte Instanz vor dem consumable: **Angenommenes wird Faktenbasis** (Baselines → Core →
Backlog → Issues), **Abgelehntes verlässt den Produktpfad endgültig** (bleibt im Audit). Zwei Item-Typen:
`RR::` = Claim mit nicht-grounded Facetten-Verdict · `US::` = Transkript-Aussage ohne Ledger-Deckung
(missing_claim/needs_human/attach).

**Aktion → Wirkung beim Apply** (Quelle: `AdjudicationCompletenessGate.Project` — bei Code-Änderung hier + i-Button nachziehen!):

| Aktion | erlaubt bei | Wirkung |
|---|---|---|
| `accept_gap` | RR + US | Claim AS-IS in den consumable; bei US: neuer Claim mit `facetStatus=pending` → refine |
| `apply_repair` | RR | Claim MIT den Repair-Feldern korrigiert übernommen |
| `promote_to_claim` | US | Aussage wird EIGENER neuer Claim (`facetStatus=pending` → refine) |
| `attach_evidence` | US (+Ziel) | Zitat + sourceUnitId als zusätzliche Evidenz an den Ziel-Claim; KEIN neuer Claim |
| `merge_existing` / `mark_covered_by` | (+Ziel) | NUR Audit („gehört zu X" / „durch X gedeckt") — consumable unverändert |
| `reject` | alle | NICHT in den consumable — endgültig raus (Audit bleibt) |
| `defer` | alle | keine Entscheidung → `pending` zählt hoch; Ziel des Runbooks: pending=0 |

**Repair-Felder** (nur `apply_repair`; vorbelegt mit dem Validator-Vorschlag, immer übersteuerbar):

- **Status** (Entscheidungsstand): `decided` · `open` · `rejected` · `uncertain` · `required` (= extern
  verpflichtend — **Rasierklingen-Regel:** verlangt Modalität `must`/`must_not`, sonst Gate-Error).
- **Modalität** (Verbindlichkeit): `must` (Pflicht) · `must_not` (Verbot) · `must_clarify` (muss geklärt
  werden) · `must_consider` (muss berücksichtigt werden) · `must_note` (muss festgehalten werden) ·
  `desired` (gewünscht) · `optional`.
- **Zeitbezug:** `mvp` · `later_possible` · `mvp_or_later_unclear`.

**Wer die Facetten WIRKLICH liest (belegt, Stand 2026-07-23):** ① Sie stehen dem Baseline-Maker
wörtlich als Claim-Kontext im Prompt (`EvidenceLedgerProjection`). ② Der **Contract-Checker (C3)**
zieht eine harte Grenze hart↔weich: bei weichem Status (`open`/`uncertain`), weicher Modalität
(`desired`/`optional`/`must_clarify`/`must_consider`/`must_note`) oder weichem Zeitbezug
(`later_possible`/`mvp_or_later_unclear`) darf das Artefakt NICHT „muss/entschieden/MVP" behaupten.
③ Gates prüfen Taxonomie-Gültigkeit + Rasierklinge. **Feinunterschiede innerhalb „weich" haben
derzeit keinen maschinellen Konsumenten** — sie wirken nur über den Prompt-Text und als
Doku/Audit (06-backlog/04-delta enthalten keine Facetten-Logik; Grep-Befund, R-8).
- **Geltungsbereich:** Freitext (für wen/wo gilt der Claim); leer = unverändert.
- **Referenz-Ziel:** Pflicht bei attach/merge/mark — ID eines existierenden Claims (Autocomplete).

Das harte Gate beim Apply lehnt unentschiedene Zeilen und fehlende Referenz-Ziele ab; die UI zeigt
dieselbe Regel live als „resolved".

**Herkunft der Items & Vorschläge:** `RR::`-Items samt Repair-Vorschlägen stammen aus der
Facetten-Validierung (step-03: Prüf-LLM je Claim gegen das Transkript, `observed → suggested`);
`US::`-Items aus der Unused-Pipeline (step-01c Triage → step-01d Compare). Die Vorschläge sind immer
Modell-Urteile — die deterministische Mechanik (Unit-Bilanz, Traces, Gates) garantiert „nichts geht
unbilanziert verloren", nicht „das Urteil stimmt". Genau deshalb entscheidest DU.

**Grenzen (was die Adjudikation NICHT ändert):**

- `merge_existing`/`mark_covered_by` = reine Audit-Einträge, kein Effekt auf den consumable.
- Sie bestimmt, WAS Fakt ist — nicht die spätere FORMULIERUNG (Paraphrase machen die Folgestufen;
  Treue prüfen deren eigene Gates: Fidelity/Checker).
- Drei Wege laufen bewusst am Autor vorbei (sonst wäre jede der ~136 Units einzeln zu reviewen):
  `grounded`-Claims (direkt approved), als `noise` triagierte Units (step-01c), `already_covered`
  mit gültiger Referenz (seit R-1 referenz-erzwungen). Alle drei sind in den Run-Artefakten auditierbar
  — wer zweifelt, prüft dort stichprobenartig.

## Varianten & Messachse

- `ledger-build` = älterer Pfad ohne Atomic Units (Fixture-Match optional als 3. Argument).
- `measure/`: `ledger-reference-template` (Annotations-Vorlage) → handgebaute Referenz →
  `ledger-reference-recall[-fast]`, `facet-validation-eval` — Completeness/Recall-Messung für die Thesis.

Beleg-Läufe: `runs/ledger/20260723_090059_7b4e24` (frischer E2E, gate pass 0/0) ·
`runsArchive/ledger/20260704_131614_018865` (Alt-Beleg). Gescheiterte R-1-Belege: `runs/ledger/20260723_08*`.
