# Vergleichsprotokoll Phase 2.1 A/B/C

**Version:** 1.0
**Stand:** 2026-06-10
**Zweck:** Methodisch saubere Grundlage für den Vergleich der drei Kontext-Strategien.
Dieses Dokument fixiert alle Variablen, damit Unterschiede in den Ergebnissen auf die
Kontextstrategie zurückgeführt werden können und nicht auf andere Faktoren.

---

## 1. Forschungsfrage

> Welche Kontextstrategie ist für einen MAF-basierten SDLC-Workflow am besten geeignet —
> gemessen an der Fehlerhäufigkeit in den erzeugten Artefakten (nach Qualitätsrubrik v2.0)?

---

## 2. Unabhängige Variable (was variiert)

**Einzige Variable:** `phase2ContextStrategy` in `run-config.json`

| Strategie | Wert | Status |
|---|---|---|
| A | `message_passing` | implementiert |
| B | `artifact_state` | noch nicht implementiert |
| C | `independent_source_reads` | noch nicht implementiert |

**Alle anderen Parameter sind über alle Vergleichs-Runs identisch** (siehe §3).

---

## 3. Kontrollvariablen (was fest ist)

Diese Werte dürfen zwischen A-, B- und C-Runs **nicht** abweichen.

### 3a. run-config.json für Vergleichs-Runs

```json
{
  "agentPhase": "phase2_1",
  "phase2ContextStrategy": "<A|B|C>",
  "llmProvider": "openrouter",
  "agentModel": "openai/gpt-4.1-mini",
  "prompts": {
    "Phase2ContextAgent":       "ContextPrompt3",
    "Phase2RequirementsAgent":  "RequirementsPrompt5",
    "Phase2RisksAgent":         "RisksPrompt3",
    "Phase2ArchitectureAgent":  "ArchitecturePrompt3",
    "Phase2OpenQuestionsAgent": "OpenQuestionsPrompt3"
  },
  "observability": {
    "enableOtel": true,
    "enableOtelSensitive": true,
    "enableOtelRaw": true,
    "innerCycleLogging": true
  },
  "llmPreview": {
    "chars": 1200
  }
}
```

### 3b. Weitere fixierte Parameter

| Parameter | Wert | Begründung |
|---|---|---|
| Transkript | `input/transcripts/T9999_chaos.txt` | einzige Datengrundlage |
| Provider | `openrouter` | stabil, kein free-Modell |
| Modell | `openai/gpt-4.1-mini` | stabiles Verhalten (FORSCH-3) |
| `innerCycleLogging` | `true` | bessere Evidenz; muss für alle Runs gleich sein |
| Prompts | Version 3/5/3/3/3 | keine Prompt-Änderungen zwischen Strategien |

---

## 4. Anzahl Runs pro Strategie

**Mindestens 2 valide Runs pro Strategie.**

Grund: LLMs sind nicht-deterministisch. Ein einziger Run könnte durch Modell-Varianz
ein Ausreißer sein. Mit 2 Runs lässt sich ein grobes Stabilitätsbild zeichnen.
Wenn Run 1 und Run 2 einer Strategie stark voneinander abweichen (>2 Kategorie-Fehler
Differenz), ist ein dritter Run empfohlen.

---

## 5. Kriterien für einen validen Run (Einschlusskriterien)

Ein Run darf in den Vergleich eingehen, wenn:

1. `validation/phase2_1.report.json` → `Passed: true`
2. Alle 5 Pflichtartefakte existieren (requirements, risks, architecture, open-questions, context.md)
3. Jeder Agent hat in `tool-calls.jsonl` mindestens 1 `TOOL_CALL_STARTED` (hat tatsächlich gearbeitet)
4. Kein Agent hat `CHAT_FAILED` in `events.jsonl` (kein Streaming-Abbruch)
5. `config.json` des Runs zeigt den korrekten Wert für `phase2ContextStrategy`

Runs die einen dieser Punkte nicht erfüllen werden **verworfen und notiert** (nicht ignoriert).

---

## 6. Jury-Parameter

| Parameter | Wert | Begründung |
|---|---|---|
| Jury-Modell | `openai/gpt-4.1` | Stärker als Mini → weniger Fehlkategorisierung (§8.3 Rubrik) |
| Jury-Prompt | qualitaetsrubrik.md §4 (v4.0, 2026-06-12) | dreistufige Verifikation, prompt-versioniert |
| Jury-Runs pro Artefakt | 1 | validiert gegen manuelle Referenz (0e0804) |
| Temperatur Jury | 0,0 | maximale Reproduzierbarkeit |
| Konfiguration | `run-config.json → jury.judgeModel` | überschreibbar ohne Code-Änderung |

**Jury-Modell-Vorbehalt:** Gleiches Modell bewertet seinen eigenen Output — möglicher Bias
(FORSCH-4). Gegenmaßnahme: manuelle Stichproben (§5 dieses Protokolls).

**Bekannte Instrumentgrenzen (relevante Auswirkungen auf Vergleich):**
- EU-Hosting FALSE_CLAIM statt FALSE_CERTAINTY: Score-Auswirkung 0, Kategorie falsch.
  Wenn Kategorie-Counts über Strategien verglichen werden, diesen Punkt explizit nennen.
- HERABGESTUFT (partial)-Findings zählen mit reduziertem Gewicht. In der Fehlertabelle §7
  erscheinen sie als eigene Spalte, damit transparent ist, ob ein Score-Unterschied auf echte
  MISSING_TOPIC-Findings oder auf herabgestufte Findings zurückgeht.
- Stochastizität ±1–2 Findings pro Run. Für Vergleich: Differenz über Strategien ist Signal,
  Absolutwert nicht. Mindestens 2 Runs pro Strategie (§4).
Details: qualitaetsrubrik.md §9.
In der Thesis explizit benennen (→ FORSCH-4). Gegenmaßnahme: manuelle Stichproben-Validierung
(mindestens 1 Artefakt pro Strategie manuell nachprüfen).

**Wann ein Jury-Ergebnis verworfen wird:**
- Jury gibt keine valide Fehlerliste zurück (strukturierter Output fehlt)
- Jury meldet > 0 Fehler in einem Artefakt, das manuell als fehlerfrei eingestuft wurde
  (Threshold: 2 abweichende Schwere-Einschätzungen → Jury-Prompt überarbeiten)

---

## 7. Auswertungslogik

Pro Strategie und Artefakt werden Fehler nach Kategorie und Schwere gezählt. Ab v4.0 der
Jury (2026-06-12) gibt es für MISSING_TOPIC drei Outcomes: unveraendert, herabgestuft, verworfen.
Die Fehlertabelle differenziert das, damit Score-Unterschiede zwischen Strategien sauber
zugeordnet werden können.

```
Strategie A — Run-IDs: [...]
Strategie B — Run-IDs: [...]
Strategie C — Run-IDs: [...]

Aggregiert über valide Runs:

                    Kat.1 (FC)     Kat.2 (FCe)    Kat.3 MISSING (unveraendert | herabgestuft)
                 K    M    G    K    M    G       K    M    G  | Hgst  | Verw.
requirements     _    _    _    _    _    _       _    _    _  |  _    |  _
risks            _    _    _    _    _    _       _    _    _  |  _    |  _
architecture     _    _    _    _    _    _       _    _    _  |  _    |  _
open-questions   _    _    _    _    _    _       _    _    _  |  _    |  _

(K=KRITISCH, M=MITTEL, G=GERING; Mittelwert über valide Runs)
(Hgst = Anzahl herabgestufter Findings; Verw. = verworfen/sufficient)
```

**Was als bedeutsamer Unterschied gilt:**
- Kategorie-2-Differenz (Falsche Sicherheit): ≥ 2 MITTEL oder ≥ 1 KRITISCH zwischen Strategien
- Kategorie-3-Differenz (Fehlende Themen): ≥ 2 Themen Differenz
- Kategorie-1-Differenz (Falsche Behauptung): jede KRITISCH-Differenz

Kleinere Differenzen (1 GERING) werden dokumentiert aber nicht als Strategie-Unterschied gewertet.
Sie können Modell-Varianz sein.

---

## 8. Prozess-Ablauf

```
Schritt 1: Vergleichsprotokoll finalisiert (dieses Dokument, V1 = jetzt)
           → Strategie B und C implementieren (P21-3, P21-4)

Schritt 2: Jury-Mechanismus implementieren
           → Eigener Run-Typ oder Post-Processing-Skript
           → Strukturierter JSON-Output pro Fehler

Schritt 3: Für jede Strategie 2 valide Runs durchführen
           → run-config.json auf die jeweilige Strategie setzen
           → run-config.json ansonsten unverändert lassen (§3)
           → Run-IDs notieren

Schritt 4: Jury für alle Runs ausführen
           → Pro Run: 4 Artefakte × Jury-Prompt = 4 Jury-Aufrufe
           → Ergebnisse in Fehlertabelle (§7) eintragen

Schritt 5: Manuelle Stichprobe
           → Mindestens 1 Artefakt pro Strategie manuell prüfen
           → Mit Jury-Ergebnis abgleichen

Schritt 6: Auswertung und Interpretation
           → Fehlerlisten vergleichen
           → Erwartete Muster prüfen (weniger Kat.2 bei C? weniger Kat.3 bei A?)
           → Befunde mit Prozess-Evidenz verknüpfen
             (tool-calls.jsonl: eigene Reads? / input-context.md: Sichtbarkeit?)

Schritt 7: Ergebnis dokumentieren
           → Neue Einträge in phase02-iteration-notes.md (je Strategie + Gesamtauswertung)
           → Kernaussage für Thesis formulieren
```

---

## 9. Erwartete Befunde (Hypothesen vor den Runs)

Diese Hypothesen werden vor den Runs formuliert, damit klar ist was gemessen wird.

| Hypothese | Basis |
|---|---|
| A zeigt mehr Kat.2-Fehler als C | Specialists lesen Transkript nicht selbst → offene Punkte seltener als offen erkennbar |
| C zeigt mehr Kat.3-Treffer als A | Eigener Quellenzugriff → bessere Themenabdeckung |
| B liegt zwischen A und C | Expliziter State gibt mehr Kontext als Message-Passing, aber weniger als direktes Lesen |
| Kat.1 ähnlich über alle Strategien | Halluzinationen hängen mehr vom Modell als von der Kontextstrategie ab |

Diese Hypothesen sind **nicht vorentschieden** — sie beschreiben was zu erwarten wäre,
können aber widerlegt werden. Das ist der Wert des Vergleichs.

---

## 10. Offene Punkte vor dem Start

Vor Beginn der ersten Vergleichs-Runs müssen folgende Punkte geklärt sein:

- [ ] Jury-Mechanismus implementiert und getestet (P21-2)
- [ ] Strategie B implementiert (P21-3)
- [ ] Strategie C implementiert (P21-4)
- [ ] Dieses Protokoll als V1 finalisiert (erledigt)

---

## Änderungshistorie

| Version | Datum | Änderung |
|---|---|---|
| 1.0 | 2026-06-10 | Erstversion, alle Kontrollvariablen aus letztem stabilen Run (0e0804) |
| 1.1 | 2026-06-12 | Jury-Modell-Korrektur (gpt-4.1-mini → gpt-4.1); §6 Instrumentgrenzen; §7 Fehlertabelle um HERABGESTUFT/VERWORFEN erweitert |