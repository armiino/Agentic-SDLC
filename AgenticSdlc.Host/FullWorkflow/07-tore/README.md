# 07-tore — Die Tore: jede Wahrheits-Mutation durch ein Human-Gate

> Status: LEBEND (README-bei-Code, B3 07.08.2026) — bei neuen Toren/Gates mitpflegen.

## Rolle in der Kette

„Tore" sind die Stufen, an denen der Core MUTIERT wird — jede Mutation läuft Maker→Gate→HumanReview→Apply
(Gates sind heilig; accept-all/replay = deklarierte Experiment-Modi). Die Unterordner sind PARALLELE Tore,
kein Nacheinander-Zwang (deshalb teilen sie die 07-Nummer).

## Die Tore (Betriebs-Reihenfolge im Ein-Graph)

| Ordner | Tor / Gate | Kurz |
|---|---|---|
| `ingestion/` | **Tor 1 requirements** (`ingest-gate`) + **Tor 1 architecture** (`arch-ingest-gate`, R-11 A1) | Delta-Items gegen den Core auflösen: RESTATE/REFINE/SUPERSEDE/CONTRADICT/NEW (+9g OPEN_QUESTION). **Aspekt-Naht `AspectIngestionProfile`** (req/arch als Daten, ein Kern) · Aspekt-Router · **② E-R4:** Quer-LESE-Tool (`TruthPartner`), NUR CONTRADICT quert (aktives Wahrheits-Ziel), `CROSS_ASPECT_FORBIDDEN` · R-35 `search_rejections` · Apply münzt CONTRADICT→DEC in den EINEN Topf und schreibt den **Lauf-Report-Vertrag `applied/run-report.json`** fort (R-40) |
| `archclassify/` | **Rollen-Gate** (`arch-classify-gate`, R-11 A2/①) | arch-Items → Konsum-Rollen constraint/work/design + **Ziel-PBIs** (U2v2-UI: ReferenceList-Chips + PBI-Detail-Panel); Apply schreibt Payload + `constrained_by`-Relationen (nur constraint, idempotent) |
| `adr/` | **ADR-Gate** (`adr-gate`, R-11 A5) | design-Items → MADR-ADR-Entwürfe (Autor-Agent) → U5-Abnahme (editierbar, Datei-Vorschau, Bulk) → Apply: `docs/adr/NNNN-*.md` + Index + `docs/architecture.md` + `AdrId/AdrStatus` im Payload; Ablösung → Status-Folge „superseded by" deterministisch |
| `decision/` | **Tor 2** (`decision-gate`, R-14) | offene DECs (Widersprüche/Fragen) auflösen: KEEP/REFINE/**ADOPT_NEW** (prägt im ASPEKT des Ziels, ② aspekt-generisch; Swaps über RequirementSwap/ConstraintSwap); accept-all VERTAGT laut (Wahrheits-Konflikte entscheidet nur der Mensch) |
| `pbiupdate/` | **pbi-gate** | deterministische Ableitung (E-8: Wahrheits-Link = covers ∪ constrained_by → MARK_CHANGED/SUPERSEDE_PBI/BLOCK_PBI) + Placement-Agent für Unplaced (**A4:** work-archs als `aspekt=architecture`, arch-aktiv-gescoped) + Align-Agent (R-26-C) → Apply (covers→Wahrheits-Item, Seed-Regel req-only) |
| `github/` | **Tor 3** (`github-forward-gate`) + Reverse | Forward: Snapshot (**`GithubSnapshotGuard`**, R-16: fremde/ungestempelte Snapshots laut ablehnen) → deterministischer Plan → Review → Execute; Body = `GithubIssueBodySections` (Statement/AK/Reqs + „Technische Rahmenbedingungen" + „Umgesetzte Architektur-Arbeit"). Reverse: Status/done-Prüfung |

## Gemeinsame Muster

R-33-GateLoop (Repair mit wörtlichem Feedback, bounded, GateAttempt-Historie) · Datei-Verträge je Gate
(`plan.json` · `*-request.json` ⇄ `*-decisions.json` · Passagier/lane bei Bahnen-Strips) · Review-UIs über
die geteilte `HumanReview`-Schicht (E0-Endstand; U2v2-ReferenceList; Hover-Glossare) · Events je Schritt.

## Spec-Referenz

Relations-Semantik & Lebenszyklen: `Thesis-Docs/aktiv/core-relationen-konzept.md` (Spec zuerst, Kangal folgt).
