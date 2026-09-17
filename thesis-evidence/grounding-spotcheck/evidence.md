# Grounding-Spotcheck — Precision + Recall, stark/schwach (B46–B50)

**Frage:** Wie verlässlich ist die Grounding-Achse von Haus aus? Plan: `…/NextStep/grounding-spotcheck-plan.md`.
**Setup:** Run `phase2_1/20260612_133345_2d7b09` (T9999), v1-Default, Judge gpt-5.4 (stark) + gpt-4.1-mini (schwach).

## Dateien
```text
<artefakt>.grounding.openai_gpt-5.4.json        (stark, v1)
<artefakt>.grounding.openai_gpt-4.1-mini.json   (schwach, v1)
2d7b09.grounding-handlabels.md                  (Flag-Precision, 12 Flags, autor-verifiziert)
2d7b09.architecture.grounding-recall-labels.md  (Voll-Label architecture → Precision + Recall)
```
`<artefakt>` ∈ {requirements, risks, architecture, open-questions}.

## Ergebnis (autor-verifiziert)
- **Stark, Flag-Precision (alle Artefakte): ~63 %** (5 correct / 3 borderline / 3 false). FP = Über-Flaggen von Verdichtungen.
- **Stark, architecture voll gelabelt:** TP=1(#7) FP=2(#1,#16) FN=2(#3,#4) TN=20 → **Precision 1/3, Recall 1/3.**
  3 echte Verstöße, Judge fing nur #7; #3 in requirements geflaggt, in architecture übersehen → **inkonsistent**.
- **Schwach (mini): unbrauchbar** — 2 Flags (borderline) + 9 unclassified; Richtung ggü. F5/F6 gedreht.
- **n winzig (3 Verstöße) → Indikation, keine Metrik.** Qualität der Befunde > Prozente.

## Befund + Folge-Tests
Grounding = **Sieb, kein Messwert**: nützlich als triagiertes Kandidaten-Signal, unzuverlässig als Zahl.
Die Folge-Tests zeigen keinen linearen "besseren Prompt", sondern einen **Precision/Recall-Regler**:

- **B47/F13 — v2 Evidence-first:** auf `2d7b09` reduziert v2 die v1-FPs (FP 3→0) bei gleichem Recall.
  Das zeigte: ein Teil der v1-Schwäche war Implementierung/Prompt-Profil, nicht nur Modellgrenze.
- **B48/F14 — Interview-risks:** v1 über-flaggte Risiko-Auswirkungen/Gegenmaßnahmen; v2 war hier besser.
- **B49/F15 — 92e6a3 req/arch:** v2 verfehlte dafür Modalitäts-Overclaims ("nur/ausschließlich EU" als entschieden,
  obwohl offen). v1 fing diese Fälle. Damit ist "v2 Default" nicht haltbar.
- **B50/F16 — v2.2 artefaktspezifische Profile:** v2.2 fixt die EU-FNs und bleibt bei `risks` deutlich toleranter
  als v2.1, ist bei `requirements`/`architecture` aber noch zu streng.

## Aktueller Stand

```text
v1   = recall-first / strenger Overclaim-Sieb, viele FP
v2   = precision-stärker, aber Modalitäts-Blindspot
v2.1 = fixt Modalität, überkorrigiert
v2.2 = artefaktspezifisch besserer Hebel, aber req/arch noch nicht final kalibriert
```

Konsequenz: Grounding bleibt verwendbar als **triagiertes Diagnosesignal**, nicht als absoluter Score. Für A/B/C trägt
weiterhin TopicCoverage; Grounding ergänzt als Artifact→Source-Sieb.

**Meta-Befund:** der LLM-Label-Entwurf (Claude) war bei #4 selbst zu mild → erst der menschliche Reviewer korrigierte
→ Beleg, dass kein LLM Ground Truth ist (stützt Human-in-the-Loop, D3).
