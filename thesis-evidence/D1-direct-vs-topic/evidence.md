# D1 — Direct- vs. TopicCoverage-Review (Evidenz)

**Frage:** Braucht die Coverage-Achse die Topic-Fixture, oder findet ein starkes Modell direkt gegen das
Transkript dieselben echten Lücken? Plan: `…/NextStep/D1-direct-vs-topic-plan.md`.

**Setup (fix für alle Varianten):**
- Run: `runs/phase2_1/20260612_133345_2d7b09` (T9999_chaos), Artefakt-Snapshots `…/snapshots/docs/`.
- Transkript: `input/transcripts/T9999_chaos.txt`.
- Judge: `openai/gpt-5.4` (stark — der Judge soll nicht selbst Rauschen einbringen).
- **Litmus (aus B44/F9):** `architecture.md` (46 Z.) enthält **kein** Skalierbarkeits-/Analytics-Thema →
  `SCALABILITY` und `ANALYTICS` sind echte Gaps. Findet R0 sie OHNE Fixture? Mit wie viel Fehlalarm?

## Varianten
- **R0 DirectTranscriptReview** — `review-direct phase2_1 20260612_133345_2d7b09 openai/gpt-5.4 T9999_chaos.txt`
  → `<artefakt>.direct.openai_gpt-5.4.run{1,2}.review.json` (+ `.gate.json`). **✅ gelaufen 2×.**
- **R1 TopicCoverage (Baseline, hier geseedet)** — `<artefakt>.R1-topiccoverage.openai_gpt-5.4.review.json`.
  Quelle: B44-Test-D, v01-Gate (Frozen-Fixture), kopiert aus
  `runs/phase2_1/20260612_133345_2d7b09/jury/_units/<artefakt>.topic-coverage.openai_gpt-5.4.review.json`.
  *Hinweis:* R1 hier = Coverage-Achse (gpt-5.4). Eine vollständige R1 inkl. Grounding-Achse bei gpt-5.4 wäre
  ein `review …`-Lauf; für den D1-Litmus (Coverage-Gaps) genügt die Coverage-Achse.
- **R2 Hybrid** — optional, R1-Result als Kontext in den R0-Prompt (kleiner Adapter), nur wenn R0 vs. R1 es nahelegt.

## R1-Werte (Sorte A, direkt aus den geseedeten JSONs; B44-Bezug)
- `architecture`: v01-Gate relevant=20, **missing=5** (u. a. **SCALABILITY-011, ANALYTICS-020** als `Coverage.MissingTopic`).
- `open-questions` / `risks` / `requirements`: missing=6 / 4 / 1. Details in den JSONs (`defects[].sourceQuote`).

## Ergebnis (B45, 2026-06-26)

**R0 gelaufen 2× (run1/run2, gpt-5.4).** Alle `Succeeded`, 0 Parse-Fehler.

**Stabilität R0 run1↔run2:** Anzahl `missing` sehr stabil (req 6→5, risks 5→5, arch 5→5, open-q 10→10);
Themen-Wiederkehr req 5/5 · risks 4/5 · arch 3/5 · open-q 8/10 (Swaps zwischen gleich realen Themen). **Severity/Gate
instabil:** risks Repair(crit2)→PassWithWarnings(crit0); arch crit 1→2; open-q crit 1→2.

**Methode R0↔R1 (Litmus):**
- **ANALYTICS:** R1 findet es, **R0 verfehlt es in BEIDEN Läufen** → reproduzierbarer blinder Fleck.
- **SCALABILITY:** R1 = Architektur-Gap; R0 konsistent als open-question, nie Architektur.
- sonst **komplementär**: R0 findet Logs-Trennung · Umgebungen/Secrets · API-Schutz · SAP-Verfügbarkeit/Schreibzugriff ·
  Freigabeprozess (alle transkript-belegt, kein erkennbarer Fehlalarm); R1 die Checklisten-Topics.

**Entscheidung (Plan §7):** **TopicCoverage bleibt A/B/C-Messinstrument** (systematisch + reproduzierbar; vergisst
Analytics nie). **Direct → L3-Critic / R2-Hybrid**, kein Ersatz. **Gate verrauscht → A/B/C auf Defects, nicht Gate.**
Details: Iteration **B45**, Ledger-Zeile 5 in `…/NextStep/ReviewWorkflow-Evidence.md`.

## Doppeltest Modellstärke — 2×2 Methode × Modell (B45, gpt-5.4 + gpt-4.1-mini)

Motivation: Modellstärke ist laut B41/F6 der Haupthebel bei *offener* Generierung. Frage: hält sich der R0-Befund
(Blindspot/Instabilität) über Modellstärken, und ist R1 modell-robust?

| | **R0 Direct** (`missing` run1/run2; crit-Instabilität) | **R1 TopicCoverage** (`missing` je Artefakt) |
|---|---|---|
| **stark gpt-5.4** | req 6/5 · risks 5/5 · **arch 5/5** · oq 10/10; arch crit 1/2 | req1 · risks4 · arch5 · oq6 |
| **schwach gpt-4.1-mini** | req 9/8 · risks 8/10 · **arch 10/10** · oq 8/10; **arch crit 6/2** | req1 · risks2 · arch3 · oq5 |

**Litmus über Modelle (verifizierte Gaps SCALABILITY/ANALYTICS):**
- **R1 fängt beide bei STARK *und* SCHWACH** (SCAL: arch+oq · ANALYTICS: oq) — die enge „ist Topic X abgedeckt?"-Frage
  ist modell-robust.
- **R0 instabil/lückenhaft, modellabhängig:** stark → Analytics in BEIDEN Läufen verfehlt, SCAL nur als open-question;
  schwach → über-flaggt (8–10 missing), Analytics nur als vager Risk-Klumpen (nie im Architektur-Artefakt), SCAL **nur
  in run2** (run1 verfehlt) → unstabil; Severity wild (arch crit **6→2** bei identischer Eingabe).

**Schlussfolgerung (das ist der stützbare Kern):** R1s Vollständigkeit/Reproduzierbarkeit ist eine Eigenschaft der
**Methode** (feste, gegatete Checkliste → enge Frage) und **robust gegen Modellstärke**. R0s Verlässlichkeit ist
**modellabhängig + verrauscht** (offene Generierung, B41/F6). → Für ein A/B/C-Messinstrument (muss über Runs/Varianten/
Modelle vergleichbar sein) ist **R1 die richtige Wahl**; R0 = L3-Critic (Über-Flaggen/Actionability dort akzeptabel).

**Modellvergleich als Thesis-Befund:** Der schwache Judge (`gpt-4.1-mini`) ist bei der offenen Direct-Aufgabe deutlich
schlechter: mehr Missing-Findings, mehr Über-Flagging und instabile Severity (`architecture` crit 6→2 bei identischer
Eingabe). Bei TopicCoverage bleibt die enge Frage dagegen brauchbar: R1 findet die Litmus-Gaps SCALABILITY und
ANALYTICS auch mit schwachem Judge. Das stuetzt die Entscheidung, den ReviewWorkflow fuer A/B/C in kleinere,
topicbasierte Pruefschritte zu zerlegen, statt schwache Modelle offene Review-Generierung leisten zu lassen.

## Datei-Index (die „Logs" = strukturierte Outputs, jeder Defect mit description/severity/sourceQuote)
```text
thesis-evidence/D1-direct-vs-topic/
  <artefakt>.direct.openai_gpt-5.4.run{1,2}.review.json        + .gate.json   (R0 stark)
  <artefakt>.direct.openai_gpt-4.1-mini.run{1,2}.review.json   + .gate.json   (R0 schwach)
  <artefakt>.R1-topiccoverage.openai_gpt-5.4.review.json                      (R1 stark)
  <artefakt>.R1-topiccoverage.openai_gpt-4.1-mini.review.json                 (R1 schwach)
  evidence.md   (diese Datei)
```
`<artefakt>` ∈ {requirements, risks, architecture, open-questions}. R1-Rohquelle zusätzlich getrackt unter
`runs/phase2_1/20260612_133345_2d7b09/jury/_units/`. Konsolen-Ausgaben sind nur Zusammenfassungen — die **JSONs sind
die belastbare Evidenz** (Sorte A: jeder Befund mit Feldern). Abgeleitete Zähler reproduzierbar via Python über diese JSONs.
