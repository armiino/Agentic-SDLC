# 02-baselines — freigegebene Claims → geprüfte Artefakt-Baselines (Evidence-Agent „Arm B")

Zweite Stufe der Produktkette. Input: `consumable.json` aus 01-ledger (via `run-config.json →
evidenceAgent.ledgerRun`; `source=ledger` ist Pflicht, sonst Exit 4). Output:
`runs/recipe/<runId>/baselines/{typ}/artifact.json` — die Artefakt-Baselines, aus denen 04-delta den
ProjectState baut. **Arm B heißt: Der Generator sieht NIE das Transkript, nur die freigegebene
Evidenzschicht** (Beweis der Ablösung von Arm A: 59/59 vs. 0/114).

## Der Lauf: `recipe <recipe.json> [model] [--dry-run]`

EIN MAF-Workflow nach Rezept (Beispiele unter `recipe/examples/`):

```jsonc
{ "baseline": { "mode": "build" | "load", "artifacts": ["requirements", …], "fromRun": "<runId bei load>" },
<!-- 9g (05.08.): der Ein-Graph bestellt ["requirements","open-questions"] (BaselineStageExecutor.OrderedArtifacts) —
     die Fragen-Spur speist via Delta→Tor 1 den DEC-Topf (Frage→DEC, MeetingQuestionMint). architecture folgt mit
     R-11 (erst Konsument, dann Bestellung). CLI-Läufe: open-questions bei Bedarf explizit mitbestellen. -->
  "derivations": [ { "spec": "derived-risks-multi" }, … ] }
```

| Schritt | Knoten/Baustein | LLM? | Was passiert |
|---|---|---|---|
| Projektion | `EvidenceLedgerProjection` | nein | Claims → Prompt-Text: je Claim `status/modality/timeScope` + Disposition des Zieltyps + Evidenz-Zitate. Byte-identisch für Chat-Runner und MAF-Executor |
| Baseline-Maker (je Typ, Fan-out) | `BaselineFanOutWorkflow` / Maker | ja | erzeugt das Artefakt (z. B. requirements) NUR aus den Claims; `sourceClaimIds` je Item. **R-37 (05.08., MAF-native Endform):** der Dispatch prägt je Spur eine TYPISIERTE `BranchSource` mit der spur-eigenen, dispositions-gekeyten Projektion (inkl. deterministischem not_applicable-Filter in `EvidenceLedgerProjection`); Kanten-Prädikate routen — vorher sahen ALLE Zweige die requirements-gekeyte Projektion (R-Log R-37) |
| Checker (MC0/C3/C7) | `makerchecker/ContractChecker` | ja+det. | Fidelity-Vertrag: Zitat-Deckung, **C3 = harte Sprache trotz weicher Facette verboten** (hart↔weich-Grenze der Adjudikation!), Halluzinations-Checks |
| Critic/Repair | `ContractCritic`/`ContractRepair` | ja | bestätigt Heuristik-Kandidaten, repariert reparable Verstöße (max. Iterationen) |
| Finalize | `CheckerRepair`-Workflow | nein | **Endzustand ist IMMER `decision=HumanReview, pass=false`** — der Checker zertifiziert nie selbst; die Autorisierung ist die Weiterverwendung durch den Autor (A4). `BRANCH_IDS_ASSIGNED` vergibt stabile Item-IDs (REQ-…, ARCH-…) |
| Derivations (Fan-out je Spec) | `SelectBaselineExecutor` → Derivation-Workflow | ja | Ableitungen aus Baseline-Teilmengen (z. B. `derived-risks-multi`, `requirements-gap`) + Inference-Check. **⚠ R-10 (2026-07-23): läuft seit dem Refactoring nicht mehr an (0 Events, leere Ordner) — offene Diagnose; Repro billig via `mode:load`** |

Konsole: `genModel`/`checkModel` (Checker nutzt `jury.judgeModel` — seit 2026-07-23 gpt-5.4) und
`claims=N` (muss zum konfigurierten consumable passen). Logs: `logs/events.jsonl`
(BRANCH_MAKER_PRODUCED → CHECK_COMPLETED → FINALIZED → BRANCH_IDS_ASSIGNED), OTel-Traces mit Token-Zahlen.

## Weitere Bausteine im Ordner (Forschungs-/Messachse, je eigenes Kommando)

- `makerchecker/` — Maker-Checker-Mechanik (auch standalone: `contract-check/-critic/-repair`, `checker-repair-workflow`).
- `derivation/` — Ableitungs-Familie (`derive`, `derive-review`, `derive-risks`, `inference-check`, `derive-metrics`).
- `chain/` (`evidence-chain`), `fanout/` (`baseline-fanout`), `branch/` (`artifact-branch`): Bausteine, aus denen das Rezept komponiert.
- `fidelity/` (`ledger-cite-fidelity`): Messwerkzeug Zitat-Treue (Thesis-Achse B0).
- `load/`, `assign-artifact-ids`: Laden/IDs.

Beleg-Läufe: frisch `runs/recipe/20260723_113115_35335a` (req=55/arch=45, deutsch, HumanReview) ·
alt `runsArchive/recipe/20260709_143733_299d8c` (inkl. funktionierender Derivations — R-10-Referenz).
