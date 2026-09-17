# Qualitätsbewertung für SDLC-Artefakte (Phase 2.1 A/B/C-Vergleich)

**Stand:** 2026-06-15 — v5.0 (Kategoriespezifische Verifikation, konfigurierbar; einheitliches
Verdikt confirmed / partial / rejected; FALSE_CERTAINTY-Verifier + Open-Marker-Precheck — DISK-7)
**Zweck:** Fehlerbasierte, transkript-gebundene Bewertung der SDLC-Artefakte für den
A/B/C-Vergleich (message_passing / artifact_state / independent_source_reads).

Verwandte Dateien:
- `Architekturentscheidungen.md` — warum dieser Ansatz, welche Alternativen diskutiert wurden.
- `vergleichsprotokoll.md` — Kontrollvariablen, valide Runs, Auswertungslogik.
- `AgenticSdlc.Host/Phases/Phase2/Evaluation/Evaluator.cs` — Implementierung.
- `AgenticSdlc.Host/Phases/Phase2/Evaluation/juryExplained.md` — vollständige End-to-End-Erklärung
  der Jury (2-Ebenen-Design, Calls, Scoring, Verifikation, Custom, Config, Dateien).

---

## 1. Ansatz: Fehler suchen, nicht Qualität schätzen

Die Jury vergleicht jede wesentliche Aussage im Artefakt gegen das Transkript und sucht
nach Fehlern. Keine Gesamtnote — nur eine nachprüfbare Fehlerliste + deterministischer Score.

**Warum dieser Ansatz:**
- Jeder Befund ist verifizierbar (Zitat aus Artefakt + Transkript-Beleg nebeneinander).
- Kein subjektives Qualitätsurteil → kein Fluency Bias.
- Direkt für die Thesis zitierfähig.
- Für den A/B/C-Vergleich: Fehlerlisten pro Strategie vergleichen, nicht Scores.
- ErrorScore als deterministischer Trigger für Phase-3-Repair (aus verifizierten Counts).

**Methodik-Vorbehalt (für Thesis):**
- Die Jury ist ein LLM — model-declared bewertet model-declared.
- Jury-Findings sind ein Signal, kein Beweis.
- Manuelle Validierung: mindestens ein Artefakt pro Strategie manuell nachprüfen.

---

## 2. Drei Fehlerkategorien (v3.0)

### Kategorie 1 — FALSE_CLAIM (Falsche Behauptung)
Eine Aussage im Artefakt widerspricht dem Transkript ODER kommt darin gar nicht vor
(erfunden: Aussagen, Zahlen, Technologien, Fakten, Entscheidungen, Risiken).

**Nicht hierher gehören:** „zu sicher formuliert" (→ FALSE_CERTAINTY), „fehlt" (→ MISSING_TOPIC).

### Kategorie 2 — FALSE_CERTAINTY (Falsche Sicherheit)
Das Artefakt stellt etwas als entschieden/gesetzt dar, obwohl das Transkript dieselbe
Sache explizit offen, strittig oder ungeklärt ließ.

**Schutze-Regel:** Wenn das Artefakt die Aussage SELBST als offen, bevorzugt, „noch nicht
final" oder als Annahme/offenen Punkt kennzeichnet, ist es KEIN FALSE_CERTAINTY-Fehler.
(→ verhindert den OAuth/DSGVO-Fehlalarm aus der Kalibrierung, siehe §8.)

### Kategorie 3 — MISSING_TOPIC (Fehlendes wichtiges Thema)
Ein für das Artefakt relevantes Transkript-Thema fehlt im Artefakt vollständig (Status ABSENT).

**Open-Points-Schutz:** Ein Thema gilt als VORHANDEN, wenn es irgendwo im Artefakt vorkommt
— auch sinngemäß/paraphrasiert, in Assumptions, Open Points, Constraints, Risks, Traceability.
Nur wenn nirgends eine sinngemäße Entsprechung steht, ist es ABSENT.
(→ verhindert den Backup/Retention/Secrets-Fehlalarm aus der Kalibrierung, siehe §8.)

---

## 3. Schwere pro Fehler

| Schwere | Bedeutung |
|---|---|
| KRITISCH | Kernanforderung/Compliance/Datenschutz/Security betroffen oder führt zu falschen Folgeentscheidungen |
| MITTEL | Fachlich relevant, beeinflusst Planung oder Verständnis |
| GERING | Kleinere Ungenauigkeit, wenig Auswirkung |

**Empirische Einschränkung (belegt):** gpt-4.1-mini ankert stark auf MITTEL; gpt-4.1 nutzt
das Spektrum breiter. Severity ist ein weiches Signal. Der ErrorScore (§5) ist robuster.

---

## 4. Zwei-Call-Jury-Mechanismus (v3.0)

Das Jury-Instrument besteht aus zwei aufeinanderfolgenden LLM-Calls:

### Call 1 — Hauptbewertung
- Input: Synthese-Prompt (Kern + artefakt-spezifisches Profil, §6) + Transkript + Artefakt.
- Output: JSON `{ "findings": [{category, severity, artifactQuote, transcriptEvidence, reason}] }`.
- Modell: gpt-4.1 (Judge-Override via Spike-Argument; Generierungs-Modell bleibt unberührt).
- Temperatur: 0,0 (maximale Reproduzierbarkeit).

### Call 2+ — Kategoriespezifischer Verifikations-Pass (DISK-7, v5.0 ab 2026-06-15)

Bis v4.0 war nur MISSING_TOPIC verifiziert. Ab v5.0 ist die Verifikation **pro Fehlerkategorie
konfigurierbar** (`run-config.json → jury.verification.{falseClaim,falseCertainty,missingTopic}`).

- Pro **aktivierter** Kategorie ein eigener Batch-Call — aber nur, wenn es Kandidaten dieser
  Kategorie gibt. Eine Kategorie auf `false` → kein Call; ihre Findings zählen unverifiziert.
- Einheitliches Verdikt für alle Kategorien (löst das alte sufficient/partial/none ab):
  Output JSON `{ "results": [{index, verdict, evidence}] }`.

| verdict | Bedeutung | Aktion |
|---|---|---|
| `rejected` | Finding ist False Positive | **verworfen** (INFORMATIONAL sichtbar, zählt nicht) |
| `partial` | Finding teilweise gültig | **behalten, Severity eine Stufe niedriger** (KRITISCH→MITTEL→GERING) |
| `confirmed` | Finding ist real | **unverändert behalten** (volles Gewicht) |

Mapping des alten MISSING-Vokabulars: `sufficient→rejected`, `partial→partial`, `none→confirmed`.

**Welcher Kontext geht in welchen Call (Token-relevant):**

| Kategorie | Frage | Re-Input an den Verifier | Transkript erneut? |
|---|---|---|---|
| MISSING_TOPIC | „Ist das Thema doch (auch paraphrasiert) abgedeckt?" | Artefakt + Kandidaten | nein |
| FALSE_CERTAINTY | „Markiert das Artefakt die Unsicherheit selbst?" | Artefakt + Kandidaten | nein |
| FALSE_CLAIM | „Stützt das Transkript die Aussage?" | **Transkript** + Kandidaten | **ja** |

Nur FALSE_CLAIM liest das (lange) Transkript erneut → teurer; daher Default `false`.
Calls pro Artefakt = 1 (Synthese) + 1 je aktivierter Kategorie *mit* Kandidaten.

**K5 — deterministischer Open-Marker-Precheck (nur FALSE_CERTAINTY):** Vor dem LLM-Call wird der
`artifactQuote` selbst auf Offenheitsmarker geprüft (`offen`, `optional`, `nicht final`,
`Annahme`, `ausstehend`, `TBD`, …). Treffer → `rejected` **ohne** LLM-Call. Geprüft wird
ausschließlich das Zitat (= die Claim-Stelle), NICHT der gesamte Artefakttext (sonst würden
entfernte Open-Point-Abschnitte fälschlich entlasten). Sind alle Kandidaten per Precheck
erledigt, entfällt der FALSE_CERTAINTY-Call ganz.

**Konservativ bei Fehlern (K3):** Parse-Fehler oder fehlende Verdikte im Verifikations-Pass →
betroffene Kandidaten gelten als `confirmed` (kein stilles Verwerfen eines evtl. echten Fehlers).
Der FALSE_CLAIM-Verifier ist zusätzlich prompt-seitig konservativ: `rejected` nur bei klarer
Transkript-Deckung, im Zweifel `confirmed` → echte Halluzinationen werden nicht maskiert.

**Warum dreistufiges MISSING (v4.0, beibehalten):**
Runs c9943e/92e6a3 (2026-06-12): das Syntheseschema kennt nur ABSENT vs. VORHANDEN, nicht
„erwähnt aber nicht ausgearbeitet". Bsp. requirements.md 92e6a3: „Mehrsprachigkeit" →
`artifactQuote="nicht vorhanden"`, obwohl Artefakt „Mehrwährungen und Mehrsprachigkeit sind noch
offen" enthält → MITTEL false positive. `partial` (jetzt) löst das (HERABGESTUFT statt verworfen).

**Warum kategoriespezifisch statt ein Universal-Verifier (DISK-7):**
Die drei Fragen sind semantisch verschieden. Ein breiter zweiter Prompt würde die bereits
bewährte MISSING-Verifikation verwässern. Belegt: ein vermischter Call (§8.4) ist schwächer.

---

## 5. ErrorScore (deterministischer Repair-Trigger)

```
ErrorScore = Σ(3·KRITISCH + 2·MITTEL + 1·GERING)  — nur über verifizierte Findings
needsRepair = (KRITISCH > 0) OR (ErrorScore ≥ Schwelle)
```

Schwelle: Default 4 (konfigurierbar in Phase 3 als Workflow-Policy).

Der Score ist in `EvaluationResult` als `NumericMetric("ErrorScore")` exponiert, mit
`Metadata["needsRepair"]` und `Metadata["criticalCount"]`. Das ist der Trigger für die
Phase-3-Conditional-Edge → Repair-Back-Edge.

**Wichtig:** Nicht deterministisch über Läufe (LLM ≠ deterministisch). Für belastbare
Schwellen-Kalibrierung mehrere Läufe pro Artefakt nötig.

---

## 6. Artefakt-spezifische Profile (v3.0)

Statt eines generischen Prompts bekommt der Synthese-Prompt ein artefakt-spezifisches
Profil-Block. Auswahl erfolgt automatisch per Dateiname in `Evaluator.ArtifactProfile()`.

**Warum per-Artefakt-Profile:**
Ein generischer Prompt transferiert nicht über Artefakttypen hinweg (belegt: risks.md ohne
Profil → 12 MISSING, mit Profil + Verifikation → 5; §8.4). Die Fehlerkategorien haben für
unterschiedliche Artefakte unterschiedliche Bedeutungen.

| Artefakttyp | Profil | MISSING_TOPIC-Semantik |
|---|---|---|
| requirements.md | `ProfileRequirements` | fehlende ANFORDERUNG |
| risks.md | `ProfileRisks` | fehlendes RISIKO/KONFLIKT (besonders paraphrasen-sensitiv) |
| architecture.md | `ProfileArchitecture` | fehlende ARCHITEKTUR-/BETRIEBS-Anforderung |
| open-questions.md | `ProfileOpenQuestions` | fehlende OFFENE FRAGE |
| andere | `ProfileGeneric` | allgemein |

---

## 7. Jury-Parameter (v3.0)

| Parameter | Wert | Begründung |
|---|---|---|
| Judge-Modell | `openai/gpt-4.1-mini` (konfigurierbar) | Mit DISK-7-Verifikation ausreichend; gpt-4.1/gpt-5-mini optional für zitierfähige Stichproben |
| Calls pro Artefakt | 1 (Synthese) + 1 je aktivierter Kategorie *mit* Kandidaten | Default-Policy → max. 3 (Synthese + missing + falseCertainty) |
| Verifikations-Policy | `missingTopic=true, falseCertainty=true, falseClaim=false` | konservative DISK-7-Defaults; im Config-Snapshot reproduzierbar |
| Kosten pro Artefakt | ~$0,01–0,03 | vernachlässigbar; FALSE_CLAIM teurer (Transkript-Re-Read) |
| Temperatur | 0,0 | maximale Reproduzierbarkeit |
| Schema | Synthese (3 Kat.) | empirisch sauberste Variante (belegt §8.2) |
| Strategie-Blindheit | ja | Jury bekommt nur Artefakt + Transkript, keine Workflow-History |
| Prompt-Version | v5.0 (Synthese + kategoriespezifische Verifier) | ausgelagert: `AgenticSdlc.Host/Prompts/jury/*.txt` (geladen via `JuryPromptLoader`) |
| Custom-Verifier | optional (`jury.verification.custom`, Default false) | ein eigener Prompt `verify-custom.txt` ersetzt die 3 Verifier — nur Experiment, nicht für A/B/C |

**A/B/C-Fairness:** Die `jury.verification`-Flags müssen über alle Vergleichsruns identisch sein
(sie stehen im `config.json`-Snapshot jedes Runs). Verschiedene Policies dürfen nicht zwischen
Strategien verglichen werden — das Instrument wird vor dem Vergleich einmal eingefroren.

**Jury-Modell-Vorbehalt (für Thesis):** Gleiches Modell kann eigene Outputs bewerten (möglicher
Bias — FORSCH-4). Gegenmaßnahme: manuelle Stichproben (mindestens 1 Artefakt pro Strategie).

---

## 8. Kalibrierungsgeschichte (2026-06-11)

Diese Sektion dokumentiert die empirische Entwicklung des Jury-Instruments — was versucht
wurde, warum es geändert wurde und was dabei gelernt wurde.

### 8.1 Ausgangspunkt: manuelle Referenzbewertung Run 0e0804
Vor dem Bau des Instruments wurde Run `20260609_152957_0e0804` (Strategie A, gpt-4.1-mini)
manuell bewertet als Ground Truth (Rubrik v2.0, §8):

```
requirements.md:  Kat.1=0  Kat.2=2  Kat.3=2  (Kritisch=0, Mittel=3, Gering=1)
risks.md:         keine wesentlichen Fehler
architecture.md:  Kat.1=0  Kat.2=2  Kat.3=1
open-questions.md: keine wesentlichen Fehler
```

Diese Referenz hat sich in der Kalibrierung als **zu konservativ** herausgestellt — die Jury
findet mehr echte Lücken, die die manuelle Analyse übersehen hat (PDF-Export, Double-Opt-In,
Rate Limits, Secrets Management, SAP-Fallback, Auftragsverarbeitungsverträge).

### 8.2 Schema-Vergleich (3 / 2 / 1 Kategorien) mit gpt-4.1-mini

**Ergebnis requirements.md, gpt-4.1-mini:**

| Schema | FALSE_CLAIM | FALSE_CERTAINTY | MISSING | gesamt | echte Fehler |
|---|---|---|---|---|---|
| 3-Kat (Prompt v1) | 6 | 2 | 5 | 13 | ~3 (Kat.1 fehlkategorisiert) |
| 3-Kat (Prompt v2) | 3 | 3 | 3 | 9 | teils echt, teils falsch |
| 2-Kat | 5 | 8 | — | 13 | viele Paraphrasen-Falsch-Positive |
| 1-Liste | 21 | — | — | 21 | starke Überfeuerung, Duplikate |

**Kernlehrung:** Weniger Kategorien = mehr Lärm. Kategorien sind Leitplanken, kein Overhead.
Die 1-Liste lieferte sogar korrekte Aussagen als Fehler (8-Wochen-MVP dreifach, EU-only).
Prompt-Wording allein bringt bei gpt-4.1-mini diminishing returns.

Resultat: **3-Kategorien-Schema beibehalten.**

### 8.3 Modell-Vergleich: gpt-4.1-mini vs. gpt-4.1

**Gleiche Config, Modell als einzige Variable — requirements.md, 3-Kat:**

| Modell | FALSE_CLAIM | FALSE_CERTAINTY | MISSING | echte Fehler | Severity-Spektrum |
|---|---|---|---|---|---|
| gpt-4.1-mini | 2 | 3 | 5 | ~5/10 | alles MITTEL |
| gpt-4.1 | 1 | 3 | 5 | ~7/9 | KRITISCH erscheint |

Preisunterschied: mini ~$0,40/1M, gpt-4.1 ~$2/1M Input (5×). Für Jury-Calls (kleine Inputs):
~$0,002 vs. ~$0,01 pro Artefakt — beide vernachlässigbar.

**Befund:** Es liegt NICHT nur am Modell. Schwache Prompt-Regeln produzieren bei gpt-4.1 fast
gleich viele Falsch-Positive. Aber gpt-4.1 kategorisiert präziser (weniger Kat.1-Fehlbuchungen)
und nutzt Severity-Spektrum. → **gpt-4.1 als Judge gewählt** (Preis/Qualität-Kompromiss).

### 8.4 Phasen-geführter 4-Kat-Prompt (gescheiterter Ansatz)

Getestet: eigener Prompt (4 Kategorien: FALSE_CLAIM / FALSE_CERTAINTY / MISSING_TOPIC /
INCOMPLETE_COVERAGE) mit Phase-1/2/3/4-Struktur (Themen → Präsenzprüfung → Status → Finding).

**Ergebnis:** 0 / 0 / 0 / **17 INCOMPLETE_COVERAGE** — alles lief in die neue Kategorie.
INCOMPLETE ist ein Lärm-Magnet: fast jede Anforderung „könnte vollständiger sein" gegenüber
einem reichhaltigen Transkript. Gleichzeitig verschwanden die echten MISSING-Findings
(PDF-Export, Rate Limiting, Rollen) komplett. **Gescheiterter Ansatz, dokumentiert als
Warnung:** Eine „weiche" Zusatzkategorie kann ein Bewertungsschema kollabieren lassen.

### 8.5 Synthese-Prompt + per-Artefakt-Profile (aktueller Stand)

**requirements.md mit Synthese-Prompt (gpt-4.1, ohne Verifikation):**

| FALSE_CLAIM | FALSE_CERTAINTY | MISSING_TOPIC | echt | falsch |
|---|---|---|---|---|
| 3 | 2 | 5 | 7 | 3 |

Verbliebene Falsch-Positive: Backup + Retention (standen in Open Points), OAuth (Artefakt hedged
selbst → „noch nicht final"). → Schutzregeln im Prompt korrekt (OAuth/DSGVO Fehlalarm weg),
aber risks.md mit generischem Prompt: **12 MISSING**, 7 davon Falsch-Positive (Paraphrasen).

**Ursache:** Ein generischer Prompt kennt den Unterschied nicht zwischen „fehlt" in
requirements.md (= fehlende Anforderung) vs. „fehlt" in risks.md (= fehlendes Risiko/Konflikt,
anders strukturiert, anders formuliert).

→ Lösung: **per-Artefakt-Profile** (§6). risks.md mit eigenem Profil: 12 → 5 nach Verifikation.

### 8.6 Batch-Verifikations-Pass (robuster Fix)

**Kernerkenntnis:** Prompt-Wording allein kann Paraphrasen-Falsch-Positive nicht eliminieren.
Der Judge im ersten Call macht eine komplexe Doppelaufgabe (Thema identifizieren + Präsenz
prüfen). Das produziert systematisch Falsch-Positive (belegt an 3 Prompt-Varianten).

**Lösung:** Zweiter, spezialisierter Call mit klarer Binär-Aufgabe (ist es abgedeckt ja/nein?
→ coveringQuote). Konservatives Fallback: bei Parse-Fehler nichts verwerfen (alle behalten).

**Ergebnisse nach Verifikation:**

| Artefakt | MISSING vor Verif. | nach Verif. | verworfen | Verwerfungsqualität |
|---|---|---|---|---|
| requirements.md | 17 (generisch) / 14 (Synthese) | 11 | 6 | 6/6 korrekt |
| risks.md | 12 | 5–6 | 6–7 | 6/7 korrekt (1 borderline) |

**Trefferquote der behaltenen Findings (manuell verifiziert):**
- requirements.md: 10/11 echt (91 %)
- risks.md: 3 klar echt + 2 borderline (60–80 %)
- Falsch-Positive nach Verifikation: 0 klare

**Nicht-Determinismus:** Gleicher Spike, gleiche Config → ±1–2 Findings je Lauf (LLM).
Der Score streut entsprechend. Für belastbare Absolut-Werte: mehrere Läufe nötig. Für
relativen A/B/C-Vergleich: gleiche Schwäche über alle Strategien → Differenz ist das Signal.

### 8.7 Finale Kalibrierungsergebnisse (2026-06-11)

Spike-Befehl:
```
dotnet run --project AgenticSdlc.Host -- jury-spike phase2_1 20260609_152957_0e0804 \
  requirements.md T9999_chaos.txt openai/gpt-4.1
```

**requirements.md — Endstand:**
```
FALSE_CLAIM=0  FALSE_CERTAINTY=0  MISSING_TOPIC=11  ErrorScore=22
Verworfen: 6 (alle korrekt, mit Beleg-Zitat)
Echt (verifiziert): Double-Opt-In, Auftragsverarbeitungsverträge, PDF-Export,
  Datenschutzhinweise-PDF, Rate Limits, Pagination/Download-Limits, Missbrauchserkennung,
  Secrets Management, SAP-Fallback/Cache, Kontaktformular-Kundenzuordnung (10/11)
```

**risks.md — Endstand:**
```
FALSE_CLAIM=0  FALSE_CERTAINTY=0  MISSING_TOPIC=5–6  ErrorScore=10–11
Verworfen: 6–7 (alle korrekt)
Echt (verifiziert): Pagination/Download-Limits, kein dedizierter Architekt,
  verfrühte Cloud-Produkt-Entscheidungen (3 klar + 2 borderline)
```

Befunddateien:
- `runs/phase2_1/20260609_152957_0e0804/jury/requirements.synth.openai_gpt-4.1.rubric.json`
- `runs/phase2_1/20260609_152957_0e0804/jury/risks.synth.openai_gpt-4.1.rubric.json`
- Alle Schema-Vergleichsdateien: `requirements.3cat.*.rubric.json`, `*.2cat.*`, `*.1list.*`,
  `*.phased.*` (vollständige Kalibrierungshistorie auf Platte)

---

## 9. Bekannte Instrumentgrenzen (Stand 2026-06-12)

Dieser Abschnitt dokumentiert bekannte Unschärfen und Fehlermuster des Jury-Instruments,
die nach Runs c9943e, 92e6a3 und 2d7b09 (Strategie A) identifiziert und manuell verifiziert
wurden. Grenzen, die durch Prompt-Änderungen behebbar waren, wurden behoben (§8.6, §4 v4.0).
Verbleibende Grenzen werden hier festgehalten, damit sie in der Thesis korrekt eingeordnet
werden können.

### 9.1 EU-Hosting: stabile Fehlklassifikation FALSE_CLAIM statt FALSE_CERTAINTY

**Beobachtet in:** Runs c9943e, 92e6a3, 2d7b09 (alle Artefakte mit EU-Hosting-Aussage).

**Muster:** Artefakt-Aussagen wie `"Hosting ausschließlich in der EU mit gesicherter Datenresidenz"`
werden als FALSE_CLAIM (KRITISCH) klassifiziert, obwohl das Thema im Transkript vorhanden ist
(Farid, Clara, Anna diskutieren EU-Hosting ausführlich). Nach der Abgrenzungsregel müsste es
FALSE_CERTAINTY sein (Thema vorhanden, nur als entschieden dargestellt).

**Warum trotz Abgrenzungsregel:** Das Modell liest `"ausschließlich in der EU mit gesicherter
Datenresidenz"` als direkten Widerspruch zum Transkript (`"EU-only ist nicht einfach, noch nicht
gesichert"`), nicht als Gewissheitsproblem. Die Abgrenzungsregel ist ein Hinweis, keine
bindende Bedingung.

**Score-Auswirkung:** Null — KRITISCH kostet 3 Punkte in beiden Kategorien.
**Thesis-Einordnung:** Kategorienfehler ohne Scoring-Effekt. Erfordert keine Korrektur für
den A/B/C-Vergleich, muss aber benannt werden wenn Kategorie-Counts verglichen werden.

---

### 9.2 FALSE_CERTAINTY: lokales statt globales Lesen

**Beobachtet in:** Run 92e6a3, requirements.md.

**Muster:** Finding `"Das System muss innerhalb von 8 Wochen als MVP bereitgestellt werden"`
als FALSE_CERTAINTY (MITTEL) klassifiziert. Das Artefakt räumt das Zeitrisiko aber selbst
ein: `"Security Review darf den MVP nicht verzögern"` und `"API Gateway Wartezeit könnte den
MVP gefährden"` (andere Abschnitte desselben Dokuments).

**Warum:** Der erste LLM-Pass bewertet Aussagen lokal (Satz-Ebene), ohne das gesamte Artefakt
nach entlastenden Formulierungen zu durchsuchen. Der Verifikations-Pass läuft nur für
MISSING_TOPIC, nicht für FALSE_CERTAINTY.

**Score-Auswirkung:** −2 Punkte (MITTEL false positive).
**Nicht behoben weil:** Ein FALSE_CERTAINTY-Verifikations-Pass wäre ein weiterer LLM-Call pro
Artefakt, der Nutzen ist marginal (1–2 Fälle pro Run), und die Priorität liegt bei den
Kontextstrategie-Implementierungen B und C. Dokumentiert in Architekturentscheidungen.md §Xb.

---

### 9.3 Verifikations-Pass: 0 VERWORFEN (sufficient) als Monitoring-Signal

**Beobachtet in:** Run 2d7b09 — kein einziges `VERWORFEN (sufficient)` in allen 4 Artefakten.

**Muster:** Die strengere Definition von `sufficient` (v4.0: erfordert konkrete themenbezogene
Textstelle, nicht generische Formulierungen) führt dazu, dass für spärliche Artefakte keine
`sufficient`-Verwerfungen ausgelöst werden. Stattdessen viele `HERABGESTUFT (partial)`.

**Einordnung:** Das ist korrekt für spärliche Artefakte — aber als Signal zu beobachten: Wenn
auch bei inhaltlich reichhaltigen Artefakten 0 VERWORFEN auftauchen, ist die `sufficient`-
Schwelle möglicherweise zu hoch. Bisher kein falsches Verhalten belegt.

---

### 9.4 Stochastizität des Instruments

**Nicht run-spezifisch — strukturelles LLM-Merkmal.**

Gleicher Prompt, gleiches Artefakt, gleiche Config → ±1–2 Findings je Lauf (belegt in §8.7).
Der ErrorScore streut entsprechend. Für den A/B/C-Vergleich bedeutet das: die Differenz zwischen
Strategien ist das Signal, nicht der Absolutwert. Gleiche Varianz über alle drei Strategien
(gleiche Jury-Config) → die Stochastizität hebt sich im Vergleich teilweise auf.

**Gegenmaßnahme:** Mindestens 2 valide Runs pro Strategie (vergleichsprotokoll.md §4).

---

### 9.5 FALSE_CERTAINTY-Verifikation: implementiert (DISK-7, 2026-06-15)

**Revidiert:** ursprünglich (2026-06-12) bewusst zurückgestellt; am 2026-06-15 umgesetzt.

**Auslöser:** Die Runs 4d4d66 / 807b7d (Judge gpt-4.1-mini) zeigten, dass FALSE_CERTAINTY der
größte False-Positive-Treiber ist: das Artefakt markiert ein Thema bereits als
`offen`/`optional`/`Annahme`, der Judge zählt es trotzdem als „zu sicher". Der ursprünglich
unterstellte „marginale Nutzen (1–2 FP)" war zu niedrig geschätzt — in architecture.md waren es
~10 Findings, fast alle mit Offenheitsmarker im eigenen Zitat. Das verfälscht den ErrorScore und
wäre für automatische Repair-Loops gefährlich (ein Repair-Agent würde korrekte
Offenheitsmarkierungen „reparieren").

**Lösung (§4 Call 2+):** kategoriespezifischer FALSE_CERTAINTY-Verifier mit deterministischem
Open-Marker-Precheck (K5) — billig, modell-unabhängig für die klaren Fälle. Adressiert §9.2.
Das Thesis-Argument aus Punkt 4 bleibt für die verbleibende Unschärfe gültig (der LLM-Verifier
hat dieselben epistemischen Grenzen) — aber die *systematischen* FP durch Open-Marker werden
nun strukturell gefangen, nicht nur per Prompt-Bitte.

---

## 10. Nächste Schritte

1. Strategien B und C implementieren (P21-3, P21-4).
2. Für jede Strategie mindestens 2 valide Runs durchführen (`vergleichsprotokoll.md` §4).
3. Jury für alle Runs: 4 Artefakte × 2 Calls × 2 Runs × 3 Strategien = ~48 Calls.
4. Jury-Ergebnisse gegen manuelle Stichproben validieren (mind. 1 Artefakt/Strategie).
6. A/B/C-Fehlerlisten vergleichen, Hypothesen prüfen (`vergleichsprotokoll.md` §9).
7. Phase-3-Repair-Loop: Schwelle `RepairScoreThreshold` als Workflow-Policy setzen.
