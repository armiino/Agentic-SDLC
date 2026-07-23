# 03-gap — Open-World-Lücken (l3): finden, verankern, vom Autor promoten lassen

Dritte Stufe (optionaler Zusatz laut Kette, im frischen E2E bewusst gefahren — Autor-Entscheid
23.07.). Aufgabe: die **geführte Öffnung** der geschlossenen Welt — ein Agent schlägt Punkte vor, die
in den Artefakten FEHLEN könnten (nie explizit gesagt), jeder Vorschlag wird gegen die
Umwelt-Artefakte **verankert** und geroutet; in die Wahrheit gelangt NUR, was der Autor promotet.

## Ablauf (3 Kommandos, Muster Prepare → Human → Apply)

```text
l3 <env1.artifact.json> [env2 …]        # [LLM] Kandidaten generieren → verankern → routen
l3-review <runId> --interactive          # [DU]  Review-UI (Scope default: needs-human)
l3-apply <runId>                         # det.  accept/edit → promoted-items (L3-REQ-…)
l3-revise <runId>                        # [LLM] optional: revise-Entscheidungen überarbeiten lassen
```

**Routing-Klassen** (`L3Class`): `SupportedAnchored` = schon durch die Artefakte gedeckt → auto,
in der Review default AUSGEBLENDET, kein Handlungsbedarf · `Unreferenced` = echte Lücken-Kandidatin →
Autor entscheidet · `Contradicted` = widerspricht den Artefakten → Autor entscheidet (besondere
Aufmerksamkeit!). Review-Policy (test-fixiert): `edit` braucht `editedText`.

**Artefakte** (`runs/l3/<runId>/`): `routing-report.json`, `human-review-package.json` (nur
needs-human), nach Review/Apply: `human-decisions.json`, `promoted-items.json` (stabile `L3-REQ-…`-IDs),
`decision-records.json`, `promotion-mappings.json` (Kandidat → promoted ID).

## Wohin die Promotions fließen

`project-state-build … --l3-run <runId|path>` (bzw. `core-bootstrap-first-transcript … --l3-run …`)
nimmt die `promoted-items` zusätzlich zu den Baselines in den State auf — so kamen sie historisch in
den Core (Seed `20260715_l3_v2`) und im frischen E2E via A4.

## Experiment-Arme (Thesis-Messachse, NICHT der Default-Pfad)

`l3` hat Forschungs-Flags (`--agentic-coverage`, `--coverage[-measure|-repair]`, `--research`,
`--exhaustive|--selective`, `--provenance`, `--graph`, `--candidates`): verschiedene
Coverage-/Provenance-Strategien aus der l3-Iterationsforschung. Der belegte Produkt-Pfad ist der
Default (`candidateMode=selective`, `mode: generate`) — genau so liefen die historischen Seeds und der
frische E2E-Lauf.

Beleg-Läufe: frisch `runs/l3/20260723_122625_136540` (35 Kandidaten → 15/15/5; 20 reviewt, 15
promotet, deutsch) · historisch `runsArchive/l3/20260715_*` (Basis des Alt-Core-Seeds).
