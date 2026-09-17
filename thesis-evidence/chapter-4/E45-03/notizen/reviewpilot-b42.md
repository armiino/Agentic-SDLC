## B42 — Review-Agent vs. zerlegte Pipeline: Qualität vs. Stabilität gemessen (Maker-Checker-Frage)

**Ziel:** die Frage *„reicht ein einzelner starker Review-Agent statt der 5-Schritt-Pipeline?"* **messen**
(Thesis-Kriterium: Mechanismus nötig oder Overengineering?). **Art:** echter MAF-`AIAgent` (`review-agent`,
fs_read/MCP, gpt-5.4) auf Run `28a52b` — 1× alle 4 Artefakte (Qualität) + 2× `risks` (Stabilität).
Reviewpilot-Runs: `5e7d1c` (4 Artefakte), `3edf9d`, `969dc5` (je risks).

**Beobachtung — Stabilität (3× dasselbe `risks.md`).** Befunde gesamt **8 / 6 / 7**. Drei Instabilitäten:
(1) **Kategorie-Flips** — „Flutter/Dart" FALSE_CLAIM↔FALSE_CERTAINTY, „Such-/Filter" umgekehrt;
(2) **Granularität** — Rollen-Thema mal 3 Findings (L1), mal 1 (L2/L3); (3) **lauf-spezifische Themen** —
„Pflegequalität" nur L2, „Akzeptanz analog→digital" nur L3, „Scope Creep" L1+L3. Kern ~70 % stabil, ~30 %
wackelt (Count/Kategorie/Granularität).

**Beobachtung — Qualität.** Der Agent macht einen **holistischen** Job (≠ per-item) und ist **schärfer:**
fing „Flutter/Dart im risks-Doc" als nicht-stakeholder-validierte Technologie (Pipeline flaggte das nicht),
benannte fehlende Risiko-*Kategorien* präzise mit Zitaten, gab in L3 sogar ein Nicht-Problem explizit frei
(„kein Fehler, durch Transkript gedeckt"). **Aber** er enumeriert die Boilerplate-Bullets nicht → **kein
sauberer per-claim-Precision-Vergleich** gegen die Hand-Labels.

**Befund.** Trade-off **gemessen:** Agent = scharf/actionable, aber **instabil**; Pipeline = **stabil**, aber
mechanisch. **Modellstärke behebt die Instabilität NICHT** (B30 mit starkem Modell reproduziert). 8/6/7 +
Kategorie-Flips disqualifizieren den freien Agenten als *numerisches Vergleichs*-Instrument.

**Learning.** „Review" hat **zwei Rollen**, die wir vermischt hatten: **Review-für-Repair** (Agent,
actionable — gehört in L3) vs. **Review-für-Messung** (Pipeline, reproduzierbar — Forschungs-Evidenz). Für
A/B/C zählt **Konsistenz** (gleiches Instrument auf A/B/C), nicht absolute Precision. Einordnung in
`InitDokument.txt`: Modellwahl ist **kein Forschungsgegenstand**; Artefaktqualität beurteilt **der Autor**;
das **eigentliche Ziel ist L3** (Repair-Loop). → zurück zum Hauptpfad.

**Verifikation:** 3 reale `review-agent`-Läufe, LLM-frei ausgewertet (Reviews gelesen, Befunde gezählt/
verglichen). Evidenz: `runs/reviewpilot/{5e7d1c,3edf9d,969dc5}/reviews/`.

**Evidenz-Anker (nachgetragen 2026-06-24):** volle Run-IDs `20260623_175026_5e7d1c` (4 Artefakte),
`20260623_175430_3edf9d`, `20260623_175521_969dc5` (je risks); Dateien `runs/reviewpilot/<runId>/reviews/risks.review.md`.
8/6/7 (Body-Zählregel) + Selbst-Unterzählung in `969dc5` reproduzierbar via `python3 tools/eval/eval_review.py`.
→ `faktenblatt-review.md` **F7**.

