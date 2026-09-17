# SourceClaim-Coverage-Spike — Evidence

Stand: 2026-06-27 · Status: **isolierter Spike, nicht produktive Achse**.

## Ziel

Pruefen, ob die Coverage-Seite analog zum Claim-Grounding verbessert werden kann:

```text
Quelle -> SourceClaim + Evidence + erwartete Artefaktpflicht -> Artefakt-Coverage
```

Vergleichsfrage:

- Ist SourceClaimCoverage konkreter/robuster als `TopicCoverage`?
- Findet sie bekannte Blindspots von `DirectReview`?
- Ist der Mehrwert gross genug, um spaeter eine Integration zu rechtfertigen?

## Setup

Fixture:
`input/eval-labels/source-claim-coverage-spike.json`

Run:

```text
source-claim-coverage-spike input/eval-labels/source-claim-coverage-spike.json phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-coverage-spike.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Fixture-Groesse:

```text
20 SourceClaims
5 architecture
5 requirements
5 risks
5 open-questions
```

Verdikte:

```text
covered | partial | missing | contradicted | not_applicable
```

Die Fixture ist bewusst groesser als ein Mini-Litmus-Test und mischt:

- bekannte D1-Litmus-Gaps (`SCALABILITY`, `ANALYTICS`),
- Open-World-/Topic-Blindspots (`PUSH`),
- Status-/Modalitaetsfaelle (`SAP-Schreibzugriff offen vs. geplant`),
- Kontrollfaelle (`Login`, `API Gateway`, `Support`, `EU-Residenz`),
- Partial-Faelle (`Rabattworkflow`, `Preis/Cache`, `Env/Secrets`).

## Ergebnis

Rohwertung gegen initiale Fixture-Labels:

```text
15/20 = 75.0 %
architecture:   4/5
requirements:   4/5
risks:          4/5
open-questions: 3/5
```

Confusion:

```text
covered -> covered:         8
missing -> missing:         5
partial -> partial:         1
contradicted -> contradicted: 1
missing -> partial:         2
covered -> missing:         1
covered -> partial:         1
partial -> covered:         1
```

## Abweichungen / Adjudikation

Die 5 Misses sind nicht einfach Judge-Fehler; sie zeigen, dass die initiale Fixture bei mehreren Cases zu grob oder
zu streng war:

| Case | Fixture | Judge | Lesart |
|---|---|---|---|
| `SC-ARCH-ENV-SECRETS-004` | missing | partial | Artefakt nennt Testdatenstrategie, aber nicht Dev/Test/Prod + Secrets. `partial` ist plausibler als `missing`. |
| `SC-REQ-ANALYTICS-008` | partial | covered | Requirements nennen KPI-Messungen und technische Umsetzung explizit als ausstehend. `covered` ist plausibel. |
| `SC-RISK-PUSH-012` | missing | partial | Risiken nennen Einwilligung/DSGVO allgemein, aber nicht Push + spaeterer Scope. `partial` ist genauer. |
| `SC-OQ-MOBILE-016` | covered | missing | `open-questions.md` enthaelt Mobile/native-vs-responsive tatsaechlich nicht. Fixture war falsch. |
| `SC-OQ-PRICE-CACHE-019` | covered | partial | Preis-/Rabattaktualitaet ist offen, aber Fallback/Cache/Invalidierung fehlt. `partial` ist genauer. |

Adjudizierte Lesart:

```text
Der Spike findet nicht nur Coverage, sondern korrigiert die SourceClaim-Fixture selbst feiner.
Roh 75 % ist deshalb konservativ; nach menschlicher Adjudikation wirken die 5 Abweichungen ueberwiegend plausibel.
```

Kein harter neuer Accuracy-Claim, weil Adjudikation nachtraeglich und Single-Reviewer.

## Vergleich zu alten Verfahren

### Gegen TopicCoverage

TopicCoverage R1 (`thesis-evidence/D1-direct-vs-topic/*R1-topiccoverage.openai_gpt-5.4.review.json`) erzeugte:

```text
architecture:   13 Defects, coverageScore 5
requirements:   13 Defects, coverageScore 1
risks:          16 Defects, coverageScore 4
open-questions: 14 Defects, coverageScore 6
```

Staerken von TopicCoverage:

- findet bekannte Missing-Gaps wie `SCALABILITY` und `ANALYTICS`,
- gibt einen reproduzierbaren A/B/C-Nenner,
- ist als Checkliste breit.

Schwaechen gegenueber SourceClaimCoverage:

- Defects sind oft grober und zahlreicher,
- Status/Modalitaet werden weniger fein geprueft,
- Open-World-Blindspots bleiben, wenn ein Topic fehlt (`PUSH`),
- `relevantFor` bleibt ein hartes Gate,
- `contradicted`/Status-Overcommit wie `SAP-Schreibzugriff offen vs. geplant` ist nicht der Kern von TopicCoverage.

SourceClaimCoverage Mehrwert:

- prueft einzelne Quellpflichten statt aggregierter Topics,
- unterscheidet `covered`, `partial`, `missing`, `contradicted`,
- liefert konkretere Missing-Facets,
- kann Open-World-Faelle testen, wenn SourceClaims kuratiert sind.

### Gegen DirectReview

DirectReview R0 stark run1 (`thesis-evidence/D1-direct-vs-topic/*.direct.openai_gpt-5.4.run1.review.json`) erzeugte:

```text
architecture:   5 Coverage-Defects
requirements:   6 Coverage-Defects
risks:          5 Coverage-Defects
open-questions: 10 Coverage-Defects
```

Staerken von DirectReview:

- keine TopicFixture noetig,
- findet holistische Defekte,
- fand z. B. Env/Testdaten/Secrets im Architekturkontext.

Schwaechen gegenueber SourceClaimCoverage:

- D1 zeigte stabilen Analytics-Blindspot bei DirectReview,
- Severity und Befundmenge schwanken,
- weniger reproduzierbarer Nenner fuer A/B/C,
- schwerer zu debuggen, weil keine feste SourceClaim-Liste.

SourceClaimCoverage Mehrwert:

- fand `ANALYTICS` und `SCALABILITY` gezielt,
- bleibt evidence-native und kontrolliert,
- gibt klare Vergleichseinheiten fuer A/B/C,
- bleibt weniger offen als DirectReview.

## Entscheidung

SourceClaimCoverage ist **methodisch vielversprechend** und waere die naechste saubere Evolutionsstufe der
Coverage-Achse:

```text
ClaimGrounding:       Artifact -> Source
SourceClaimCoverage:  Source -> Artifact
```

Der Spike reicht aber noch **nicht** fuer sofortige Integration, weil:

- SourceClaims wurden manuell kuratiert; automatische SourceClaim-Extraction ist unbewiesen.
- Artifact-Obligation (`requirements` vs. `risks` vs. `architecture` vs. `open-questions`) bleibt anspruchsvoll.
- Initiale Fixture-Labels waren selbst fehleranfaellig; es braucht Adjudikation.
- Integration wuerde neue Mapper/Gate-Logik und neue Fixtures erfordern.

Empfehlung:

```text
Nicht in die Closure integrieren.
Als Future-Work / L3-Ausbaupfad dokumentieren.
TopicCoverage bleibt Default fuer Coverage-Messung.
SourceClaimCoverage wird als naechster plausibler Verbesserungsstrang festgehalten.
```

## Fortsetzung — SourceClaim-Extraction-E2E (entscheidender Engpass)

Nachfrage: Der manuelle SourceClaim-Spike beantwortet nur, ob die Coverage-Pruefung funktioniert, **wenn** gute
SourceClaims vorliegen. Fuer eine echte Einbauentscheidung muss die automatische SourceClaim-Extraction getestet
werden. Deshalb wurde ein zweiter isolierter Runner gebaut:

```text
source-claim-extraction-spike input/eval-labels/source-claim-coverage-spike.json T9999_chaos.txt phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
```

Kette:

```text
Transcript -> SourceClaimExtractor pro Artefakt -> RecallMatcher gegen 20 Hand-Cases -> Coverage fuer gematchte Claims
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-extraction-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Extraktion:

```text
architecture:   15 Claims
requirements:   15 Claims
risks:          14 Claims
open-questions: 14 Claims
```

Recall gegen die 20 kuratierten Hand-Cases:

```text
exact:         8/20 = 40 %
exact+partial: 13/20 = 65 %
missed:        7/20 = 35 %
```

E2E gegen die Fixture-Verdikte (nur gematchte Claims koennen ueberhaupt treffen):

```text
3/20 = 15 %
```

Wichtig: Die niedrige E2E-Zahl ist nicht als endgueltige Accuracy zu lesen, weil automatisch extrahierte Claims
oft anders geschnitten sind als die Hand-Fixture. Sie zeigt aber den praktischen Integrationsengpass: Ohne stabile
Extraction gleicher Granularitaet kann SourceClaimCoverage nicht direkt als reproduzierbare Messachse dienen.

Kritische verfehlte Hand-Cases:

```text
SC-ARCH-SCALABILITY-001
SC-ARCH-ANALYTICS-002
SC-REQ-ANALYTICS-008
SC-REQ-SCALABILITY-009
SC-REQ-PUSH-010
SC-OQ-PUSH-017
SC-OQ-ANALYTICS-020
```

Das ist inhaltlich schwerwiegend, weil darunter genau die Litmus-Gaps liegen, fuer die SourceClaimCoverage
eigentlich besonders attraktiv waere (`SCALABILITY`, `ANALYTICS`, `PUSH`). Der Extractor extrahiert zwar viele
plausible Claims, aber nicht verlaesslich die fuer die Coverage-Messung entscheidenden SourceClaims.

### Vergleich zur ClaimGrounding-Entscheidung

ClaimGrounding wurde integrationsreif, weil die offenen Komponenten in Stufen hielten:

```text
manuelle Evidence -> Auto-Evidence 27/28 -> Splitter/Facetten -> Fremdtranskript-Check
```

Bei SourceClaimCoverage ist die analoge Stufe **nicht** bestanden:

```text
manuelle SourceClaims -> Coverage vielversprechend
Auto-SourceClaimExtraction -> Recall nur 40 % exact / 65 % exact+partial
```

Damit ist die Lage klar anders als bei ClaimGrounding.

### Revidierte Entscheidung

SourceClaimCoverage bleibt methodisch interessant, aber **nicht integrationsnah**.

```text
Nicht einbauen.
Nicht als Coverage-Default.
Nicht als Closure-Blocker.
```

Begruendung:

- Der entscheidende automatische Schritt (SourceClaimExtraction) verfehlt zu viele Litmus-Cases.
- Die Claim-Granularitaet ist nicht stabil genug fuer A/B/C-Messung.
- Eine Integration wuerde neue Forschung erfordern: bessere Extraction, Claim-Dedup, Obligation-Klassifikation,
  Recall-Audit und Mapping.

Aktuelle beste Linie:

```text
Coverage v1: TopicCoverage + recall-first Fixture bleibt Default.
ClaimGrounding: ClaimEvidence integrieren.
SourceClaimCoverage: Future Work, nur wenn Extraction deutlich verbessert wird.
```

### Thesis-Satz nach Extraction-Test

```text
Ein SourceClaim-Coverage-Spike zeigt, dass claim-native Coverage bei kuratierten SourceClaims feiner urteilen kann
als aggregierte TopicCoverage. Der automatische Extraction-E2E-Test verfehlt jedoch mehrere zentrale Litmus-Claims
und erreicht nur 40 % exact Recall bzw. 65 % exact-or-partial Recall gegen die Hand-Fixture. Daher bleibt
SourceClaimCoverage ein plausibler Ausbaupfad, aber TopicCoverage bleibt fuer die aktuelle Arbeit die stabilere
Coverage-Achse.
```

## Fortsetzung — Wide-Recall-Extraction (ohne kleines Top-N-Cap)

Korrekturfrage: Der bounded Extraction-Test vermischte zwei Dinge:

```text
1. Kann das Modell den SourceClaim grundsaetzlich finden?
2. Waehlt es ihn unter einem engen Top-N-Cap aus?
```

Deshalb wurde derselbe Extraction-E2E-Spike im `wide`-Modus wiederholt:

```text
source-claim-extraction-spike input/eval-labels/source-claim-coverage-spike.json T9999_chaos.txt phase2_1 20260612_133345_2d7b09 openai/gpt-5.4 wide
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-extraction-spike.wide-recall.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Extraktion:

```text
architecture:   115 Claims
requirements:   124 Claims
risks:           82 Claims
open-questions: 104 Claims
gesamt:         425 Claims
```

Recall gegen dieselben 20 Hand-Cases:

```text
exact:         17/20 = 85 %
exact+partial: 20/20 = 100 %
missed:        0/20
```

E2E gegen Fixture-Verdikte:

```text
7/20 = 35 %
```

Die vorher verfehlten Litmus-Cases (`SCALABILITY`, `ANALYTICS`, `PUSH`) wurden im Wide-Modus gefunden. Damit ist der
bounded Befund zu praezisieren:

```text
Nicht: Das Modell kann die SourceClaims grundsaetzlich nicht finden.
Sondern: Ein enger Top-N-Extractor ist fuer Coverage-Recall ungeeignet.
```

### Interpretation

Wide Extraction beantwortet die Grundlagenfrage positiv:

```text
Das Modell kann die relevanten SourceClaims grundsaetzlich extrahieren.
```

Aber es zeigt zugleich den Produktisierungsengpass:

```text
425 Claims fuer 4 Artefakte sind fuer Inline-Coverage ohne Dedup/Selection/Gating zu viel.
```

Die niedrige E2E-Quote (7/20) liegt nicht primaer an fehlendem Recall, sondern an:

- anderer Claim-Granularitaet als die Hand-Fixture,
- sehr vielen ueberlappenden Claims,
- fehlender Dedup-/Clustering-Stufe,
- fehlender Artifact-Obligation-/Priority-Auswahl,
- teilweise zu streng/grob gesetzten Fixture-Verdikten.

### Revidierte Entscheidung nach Wide-Test

SourceClaimCoverage ist nach Wide-Test **staerker einzuschaetzen** als nach bounded Test:

```text
Manuell kuratierte SourceClaims: Coverage-Pruefung vielversprechend.
Bounded Extraction: zu wenig Recall wegen Top-N-Cap.
Wide Extraction: sehr guter Recall, aber zu viele Claims.
```

Damit ist die naechste offene Frage nicht mehr „findet das Modell die Claims?", sondern:

```text
Kann man Wide Extraction sinnvoll deduplizieren, priorisieren und artefaktspezifisch gaten,
ohne die Litmus-Claims wieder zu verlieren?
```

Einbauentscheidung fuer die aktuelle Closure bleibt trotzdem:

```text
Nicht jetzt integrieren.
TopicCoverage bleibt Default.
SourceClaimCoverage = ernsthafter Coverage-v2-Kandidat.
```

Warum nicht sofort integrieren:

- 425 Claims erzeugen hohe Kosten und viel Rauschen.
- Es fehlt eine Dedup-/Clustering-Schicht.
- Es fehlt eine robuste Artifact-Obligation-Schicht.
- Mapper/Gate/Repair muessten neue Kategorien wie `Coverage.PartialClaim` und `Coverage.ContradictedClaim` tragen.

### Aktualisierter Thesis-Satz

```text
Ein SourceClaim-Coverage-Spike zeigt, dass claim-native Coverage bei kuratierten SourceClaims feiner urteilen kann
als aggregierte TopicCoverage. Ein bounded Extraction-Test verfehlte zentrale Litmus-Claims wegen Top-N-Priorisierung;
ein Wide-Recall-Test fand dagegen alle 20 Hand-Cases exact-or-partial (17/20 exact), erzeugte aber 425 Claims fuer
vier Artefakte. SourceClaimCoverage ist damit ein ernsthafter Coverage-v2-Kandidat, erfordert aber vor Integration
Deduplication, Priorisierung und Artifact-Obligation-Gating. Fuer die aktuelle Closure bleibt TopicCoverage die
stabilere, reproduzierbare Coverage-Achse.
```

Wenn spaeter gebaut wird, dann bounded:

1. SourceClaimExtractor mit Evidence-Spans.
2. ArtifactObligationClassifier statt hartem `relevantFor`.
3. SourceClaimCoverageClassifier.
4. Mapper zu `Coverage.MissingClaim`, `Coverage.PartialClaim`, `Coverage.ContradictedClaim`.
5. Vergleich gegen TopicCoverage auf 2-3 Domaenen.

## Fortsetzung — Global-Ledger-Extraction (Transkript einmal lesen)

Kostenfrage: Der Wide-Test war teuer, weil das komplette Transkript viermal gelesen wurde, einmal pro Zielartefakt.
Deshalb wurde eine guenstigere Ledger-Variante gebaut:

```text
source-claim-ledger-spike input/eval-labels/source-claim-coverage-spike.json T9999_chaos.txt phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
```

Kette:

```text
Transcript -> globaler SourceClaim-Ledger mit relevantFor -> RecallMatcher -> Coverage fuer gematchte Claims
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-ledger-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Extraktion:

```text
global ledger: 98 Claims
```

Zum Vergleich:

```text
bounded pro Artefakt: 58 Claims gesamt, aber Recall zu niedrig
wide pro Artefakt:    425 Claims gesamt, sehr guter Recall, aber teuer/noisy
global ledger:         98 Claims gesamt, sehr guter Recall, deutlich guenstiger
```

Recall gegen dieselben 20 Hand-Cases:

```text
exact:         14/20 = 70 %
exact+partial: 20/20 = 100 %
missed:        0/20
```

E2E gegen Fixture-Verdikte:

```text
8/20 = 40 %
```

`relevantFor` war noch sehr breit:

```text
architecture:   76 Kandidaten
requirements:   87 Kandidaten
risks:          76 Kandidaten
open-questions: 31 Kandidaten
```

### Interpretation

Die Einmal-Lesen-Variante beantwortet die Kosten-/Machbarkeitsfrage besser als der Wide-Test:

```text
Das Modell kann aus einem einzigen Transkript-Pass einen globalen Ledger bauen und verliert dabei keinen der
20 Litmus-Cases vollstaendig.
```

Der Engpass verschiebt sich:

```text
Nicht mehr: SourceClaims finden.
Sondern: relevantFor/Artifact-Obligation scharf genug setzen und Claims deduplizieren/priorisieren.
```

Der globale Ledger ist methodisch naeher an einem Evidence Ledger als die vierfache Wide-Extraction. Er strukturiert
das Transkript einmal in quellengebundene Claims mit Evidence und Artefaktbezug. Fuer direkte Integration reicht er
aber noch nicht, weil die Artefaktzuordnung zu breit ist und die E2E-Fixture-Labels/Claim-Granularitaet weiterhin
Rauschen erzeugen.

### Aktualisierte Entscheidung

SourceClaimCoverage ist nach dem Global-Ledger-Test staerker als reine Future-Idee:

```text
Machbarkeit: hoch.
Direkte Integration: noch nein.
Naechster sinnvoller Schritt: Dedup + scharfer ArtifactObligationClassifier auf dem globalen Ledger.
```

TopicCoverage bleibt fuer die aktuelle Closure Default, aber der wahrscheinlich beste Coverage-v2-Pfad ist jetzt:

```text
Transcript einmal -> Global SourceClaim Ledger -> Dedup/Priority -> ArtifactObligation -> SourceClaimCoverage
```

## Fortsetzung — ArtifactObligation-Spike

Naechster Test: Ersetzt ein grobes `relevantFor` durch eine artefaktspezifische Pflichtklassifikation:

```text
source-claim-obligation-spike \
  thesis-evidence/source-claim-coverage-spike/source-claim-ledger-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json \
  input/eval-labels/source-claim-coverage-spike.json \
  openai/gpt-5.4
```

Kette:

```text
Global SourceClaim Ledger (98 Claims)
-> ArtifactObligationClassifier pro Artefakt
-> RecallMatcher gegen dieselben 20 Hand-Cases
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-obligation-spike.source-claim-ledger-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.json`

Kandidatenvergleich:

```text
architecture:    relevantFor=76, required=60, actionable=87
requirements:    relevantFor=87, required=54, actionable=71
risks:           relevantFor=76, required=33, actionable=82
open-questions:  relevantFor=31, required=22, actionable=56
```

`actionable` = `required | optional | unclear`. Dieser Wert kann groesser als altes `relevantFor` sein, weil der
Obligation-Classifier alle 98 Ledger-Claims pro Artefakt neu bewertet und nicht nur die alte `relevantFor`-Menge
filtert.

Recall gegen dieselben 20 Hand-Cases:

```text
actionable:    exact=16/20, exact+partial=20/20, missed=0/20
requiredOnly:  exact=8/20,  exact+partial=15/20, missed=5/20
```

### Interpretation

Der Spike bestaetigt die Richtung, aber auch die Grenze:

```text
ArtifactObligation hilft, aber ein hartes required-only Gate ist zu aggressiv.
```

`requiredOnly` reduziert Kandidaten deutlich, verliert aber wichtige Litmus-Cases:

- `SC-ARCH-ANALYTICS-002`
- `SC-REQ-SCALABILITY-009`
- `SC-REQ-PUSH-010`
- `SC-RISK-PUSH-012`
- `SC-OQ-EU-RESIDENCE-018`

`actionable` haelt dagegen 20/20 exact-or-partial Recall, ist aber fuer architecture/risks/open-questions noch
zu breit. Die naechste Verbesserung ist daher nicht ein noch haerteres Gate, sondern eine feinere Nutzung von
`necessity + expectedRepresentation + importance`:

```text
Coverage-v2 sollte required nicht allein als Filter verwenden.
Sinnvoller ist:
required + high/medium optional + erwartete Representation,
danach Dedup/Priority und Candidate Matching.
```

Aktualisierte Entscheidung:

```text
SourceClaimCoverage bleibt starker Coverage-v2-Kandidat.
ArtifactObligation ist notwendig, aber in der ersten Form noch nicht ausreichend scharf.
Direkter Einbau bleibt verfrueht; der VorschlagSourceClaimCoverageGenerated-Plan bleibt als Zielbild plausibel.
```

## Fortsetzung — Kleiner SourceObligation-Extractor

Nach dem kleinen Vorschlag wurde ein direkterer Spike gebaut:

```text
Transcript einmal
-> SourceObligationExtractor
-> artefaktspezifische Obligations mit Evidence + expectedRepresentation
-> RecallMatcher gegen dieselben 20 Hand-Cases
```

Ziel: Pruefen, ob man generische SourceClaims + nachgelagerten ArtifactObligation-Schritt ersetzen kann durch einen
einzigen direkten Extraktionsschritt:

```text
SourceObligation = SourceClaim + Zielartefakt + erwartete Repräsentation + Evidence
```

Implementiert:

```text
source-obligation-extraction-spike input/eval-labels/source-claim-coverage-spike.json T9999_chaos.txt <model>
```

Ein erster `openai/gpt-5.4`-Run wurde zunaechst vom Anbieter mit `402 Payment Required` abgelehnt. Nach erneuter
Freischaltung lief derselbe Test erfolgreich durch.

Zusaetzlich lief ein Ersatzlauf mit `openai/gpt-4.1-mini`:

```text
Output:
thesis-evidence/source-claim-coverage-spike/source-obligation-extraction-spike.T9999_chaos.openai_gpt-4.1-mini.json
```

Extraktion:

```text
gesamt:          19 Obligations
architecture:     5
requirements:    10
risks:            3
open-questions:   1
```

Recall gegen die 20 Hand-Cases:

```text
exact:         3/20
exact+partial: 3/20
missed:       17/20
```

Der spaetere `openai/gpt-5.4`-Run:

```text
Output:
thesis-evidence/source-claim-coverage-spike/source-obligation-extraction-spike.T9999_chaos.openai_gpt-5.4.json
```

Extraktion:

```text
gesamt:          74 Obligations
architecture:     7
requirements:    38
risks:           12
open-questions:  17
```

Recall gegen die 20 Hand-Cases:

```text
exact:          7/20
exact+partial: 14/20
missed:         6/20
```

Voll verfehlt wurden:

- `SC-ARCH-SCALABILITY-001`
- `SC-ARCH-ANALYTICS-002`
- `SC-REQ-SCALABILITY-009`
- `SC-REQ-PUSH-010`
- `SC-RISK-PUSH-012`
- `SC-OQ-EU-RESIDENCE-018`

### Interpretation

Der Mini-Lauf war kein fairer Ersatz fuer gpt-5.4, zeigte aber denselben Fehlermodus staerker. Auch gpt-5.4
bestaetigt den Kernbefund:

```text
Ein direkter SourceObligationExtractor kann zu stark komprimieren und dadurch genau die Recall-Faelle verlieren,
die Coverage-v2 finden soll.
```

Damit ist der kleine SourceObligation-Ansatz in dieser Form **nicht besser** als der Global-Ledger-Pfad:

```text
Global Ledger gpt-5.4:        98 Claims, 20/20 exact-or-partial
SourceObligation gpt-5.4:     74 Obligations, 14/20 exact-or-partial
```

Der aktuelle beste belegte Stand bleibt deshalb:

```text
Global SourceClaim Ledger:
98 Claims, 20/20 exact-or-partial

ArtifactObligation nachgelagert:
actionable 20/20, requiredOnly 15/20

Direkter SourceObligationExtractor:
mit gpt-5.4 14/20 exact-or-partial, verliert aber zentrale Litmus-Cases
```

## Fortsetzung — Conservative Selection auf Global Ledger

Naechster Test aus dem groesseren Vorschlag: nicht erneut extrahieren, sondern den erfolgreichen Global Ledger
konservativ pro Artefakt selektieren.

```text
source-claim-selection-spike \
  thesis-evidence/source-claim-coverage-spike/source-claim-ledger-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json \
  input/eval-labels/source-claim-coverage-spike.json \
  openai/gpt-5.4
```

Kette:

```text
Global SourceClaim Ledger (98 Claims)
-> SourceClaimSelectionClassifier pro Artefakt
-> must_check / should_check / context_only / skip
-> RecallMatcher gegen dieselben 20 Hand-Cases
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-selection-spike.source-claim-ledger-spike.T9999_chaos.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.json`

Kandidatenvergleich:

```text
architecture:    oldRelevantFor=76, must=52, must+should=79
requirements:    oldRelevantFor=87, must=51, must+should=80
risks:           oldRelevantFor=76, must=61, must+should=84
open-questions:  oldRelevantFor=31, must=31, must+should=50
```

Recall gegen dieselben 20 Hand-Cases:

```text
must only:    exact=10/20, exact+partial=17/20, missed=3/20
must+should:  exact=14/20, exact+partial=20/20, missed=0/20
```

### Interpretation

Der Selection-Spike zeigt denselben Grundkonflikt:

```text
Recall-sicher = zu breit.
Scharf reduziert = verliert Litmus-Cases.
```

`must+should` haelt 20/20 exact-or-partial, reduziert aber kaum:

- Architecture wird sogar von 76 auf 79 Kandidaten breiter.
- Risks wird von 76 auf 84 breiter.
- Open-Questions wird von 31 auf 50 breiter.

`must only` reduziert staerker, verliert aber:

- `SC-ARCH-ANALYTICS-002`
- `SC-REQ-SCALABILITY-009`
- `SC-REQ-PUSH-010`

Damit ist auch Conservative Selection in dieser Form **noch kein produktiver Filter**. Der beste belegte Coverage-v2
Kern bleibt:

```text
Global Ledger kann Recall.
Aber eine LLM-only Selection/Obligation-Schicht reduziert noch nicht verlaesslich ohne Recall-Verlust.
```

Konsequenz:

```text
SourceClaimCoverage nicht jetzt integrieren.
Falls weiterverfolgt: nicht weitere freie Selector-Prompts, sondern kleinere deterministische/heuristische
Vorfilter + gezielte CoverageJudge-Pruefung, oder gezielte manuelle/adjudizierte Reference-Ledger fuer Evaluation.
```

## Fortsetzung — Coverage-Matrix statt Vorfilter

Forschungsstand-Hypothese: `ArtifactObligation` sollte nicht als unsichtbares Gate vor der Pruefung laufen. Stattdessen
soll jeder SourceClaim gegen jedes Artefakt ein sichtbares Relation-Verdict bekommen:

```text
covered | partial | missing | contradicted | not_applicable | unclear
```

Test:

```text
source-claim-coverage-matrix \
  input/eval-labels/source-claim-coverage-spike.json \
  phase2_1 \
  20260612_133345_2d7b09 \
  openai/gpt-5.4
```

Kette:

```text
20 kuratierte SourceClaims
× 4 Artefakte
= 80 Coverage-Relationen
```

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-coverage-matrix.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Ergebnis:

```text
Target-Artefakt vs Fixture: 14/20 = 70 %
Non-Target not_applicable: 4/60 = 6.7 %

Verdict-Verteilung ueber 80 Zellen:
missing:         11
partial:         40
covered:         24
not_applicable:   4
contradicted:     1
```

### Interpretation

Die Matrix bestaetigt den Kernbefund gegen harte Vorfilter:

```text
Viele SourceClaims sind nicht genau einem Artefakt zuordenbar.
Sie erzeugen je Artefakt unterschiedliche Relationen.
```

Beispiele aus dem Lauf:

- `SC-REQ-PUSH-010`: requirements=`missing`, risks=`partial`, open-questions=`partial`, architecture=`not_applicable`.
- `SC-ARCH-SCALABILITY-001`: architecture=`missing`, requirements=`partial`, risks=`covered`, open-questions=`partial`.
- `SC-OQ-ANALYTICS-020`: open-questions=`missing`, architecture=`partial`, requirements=`partial`, risks=`covered`.

Damit wird klar, warum `relevantFor`, `requiredOnly`, `mustOnly` und direkte SourceObligations instabil waren:

```text
Das Problem ist keine binaere Artefaktzuordnung.
Es ist eine Relation-Matrix SourceClaim -> Artifact.
```

Der Matrix-Ansatz loest die stille-Gate-Problematik besser:

```text
Kein Claim verschwindet vor der Pruefung.
not_applicable wird ein sichtbares Urteil.
Cross-Artefact-Partial/Missing wird sichtbar.
```

Aber er bringt neue Kosten/Noise:

```text
20 Claims -> 80 Urteile.
98 Global-Ledger-Claims -> 392 Urteile.
```

Der naechste sinnvolle Ausbau waere daher nicht weitere Extraction/Selection, sondern:

```text
1. CoverageJudge/Matrix auf adjudiziertem Reference-Ledger bewerten.
2. Batching/Kosten pruefen.
3. TopicCoverage als Residual-Trigger behalten.
4. Erst danach ueber produktive Coverage-v2-Integration entscheiden.
```

## Fortsetzung — Matrix-v2 mit getrenntem Applicability/Coverage-Urteil

Auf Basis der adjudizierten 80-Zellen-Matrix wurde ein gezielter v2-Judge gebaut:

```text
source-claim-coverage-matrix-v2 \
  input/eval-labels/source-claim-coverage-spike.json \
  input/eval-labels/source-claim-coverage-matrix-adjudication.json \
  phase2_1 \
  20260612_133345_2d7b09 \
  openai/gpt-5.4
```

Anpassungen:

- `applicability` und `coverage` getrennt ausgeben.
- `partial` nur, wenn konkrete Teilfacetten im Artefakt nachweisbar vorhanden sind.
- Status-/Modalitaets-/Scope-Abweichungen explizit als `contradicted` pruefen.
- `context`/`not_applicable` nicht als stilles Gate, sondern als sichtbares Urteil.

Output:
`thesis-evidence/source-claim-coverage-spike/source-claim-coverage-matrix-v2.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json`

Ergebnis gegen die adjudizierte 80-Zellen-Matrix:

```text
Applicability Accuracy:        72/80 = 90.0 %
Coverage Accuracy all cells:   62/80 = 77.5 %
Both correct:                  58/80 = 72.5 %
Coverage Accuracy applicable:        80.5 %

Partial predicted: 27
Partial gold:      29
Partial recall:    68.97 %
Partial overuse:   7
```

Confusion:

```text
missing -> missing:             18
partial -> partial:             20
covered -> covered:             22
contradicted -> contradicted:    2

covered -> partial:              6
partial -> contradicted:         3
partial -> missing:              3
partial -> covered:              3
not_applicable -> missing:       2
not_applicable -> partial:       1
```

### Interpretation

Der gezielte Move hat den wichtigsten Fehlermodus reduziert:

```text
v1 Matrix: partial=40/80
v2 Matrix: partial=27/80
Gold:      partial=29/80
```

V2 ist damit deutlich naeher an der adjudizierten Matrix als der erste Matrix-Judge. Besonders positiv:

- `applicability` ist mit 90 % brauchbar.
- `missing` wird deutlich besser erkannt.
- Status-Widersprueche wie SAP-Schreibzugriff werden in architecture/requirements korrekt als `contradicted`
  erkannt.

Restfehler:

- v2 ueberkorrigiert teils `partial -> contradicted` oder `partial -> missing`.
- Einige `covered`-Faelle werden zu streng als `partial` bewertet.
- `not_applicable` bleibt schwierig, weil viele Claims echte Cross-Artefact-Bezuege haben.

Aktualisierte Entscheidung:

```text
Der Matrix-Ansatz ist nun der staerkste getestete Coverage-v2-Pruefansatz.
Er ist noch nicht produktionsreif, aber erstmals messbar verbesserbar gegen eine lokale Reference-Matrix.
Naechster sinnvoller Schritt waere kein neuer Extractor/Selector, sondern:
1. Reference-Matrix ggf. zweit-reviewen/adjudizieren,
2. v2-Regeln punktuell gegen Over-strictness justieren,
3. Kosten/Batching fuer 98x4 abschaetzen.
```

## Fortsetzung — Matrix-Batch-Kostentest fuer `requirements`

Motivation:

Der Matrix-v2-Run ist diagnostisch stark, aber teuer:

```text
20 SourceClaims x 4 Artefakte = 80 LLM-Calls
98 Ledger-Claims x 4 Artefakte = 392 LLM-Calls
```

Daher wurde isoliert getestet, ob eine guenstigere Batch-Form tragfaehig ist:

```text
alle SourceClaims x genau ein Artefakt = 1 LLM-Call
```

Als Artefakt wurde bewusst `requirements` gewaehlt, weil es im Einzelzellen-v2-Run der fehleranfaelligste
Artefakttyp war:

```text
requirements Einzelzellen-v2:
Applicability: 18/20
Coverage:      13/20
Both:          11/20
```

Reproduktionsbefehl:

```text
dotnet run --project AgenticSdlc.Host -- \
  source-claim-coverage-matrix-batch \
  input/eval-labels/source-claim-coverage-spike.json \
  input/eval-labels/source-claim-coverage-matrix-adjudication.json \
  phase2_1 \
  20260612_133345_2d7b09 \
  requirements \
  openai/gpt-5.4
```

Inputs:

```text
input/eval-labels/source-claim-coverage-spike.json
input/eval-labels/source-claim-coverage-matrix-adjudication.json
runs/phase2_1/20260612_133345_2d7b09/snapshots/docs/requirements.md
```

Output:

```text
thesis-evidence/source-claim-coverage-spike/source-claim-coverage-matrix-batch.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.json
```

Ergebnis:

```text
Batch requirements:
Applicability Accuracy: 16/20 = 80.0 %
Coverage Accuracy:      14/20 = 70.0 %
Both correct:           13/20 = 65.0 %
```

Vergleich:

```text
Einzelzellen-v2 requirements: both=11/20, coverage=13/20, applicability=18/20
Batch requirements:          both=13/20, coverage=14/20, applicability=16/20
```

Confusion:

```text
covered -> covered:       9
missing -> missing:       3
partial -> partial:       2

partial -> covered:       2
partial -> missing:       2
partial -> contradicted:  1
contradicted -> covered:  1
```

### Interpretation

Der Batch-Ansatz ist als Kostenoptimierung plausibel, aber nicht als alleinige Qualitaetsloesung:

- positiv: weniger Calls, JSON-Output stabil, `both` fuer `requirements` steigt von 11/20 auf 13/20.
- positiv: einige Over-strictness-Faelle des Einzelzellen-v2-Runs werden korrigiert, z. B. Analytics/API-Gateway.
- negativ: `applicability` sinkt von 18/20 auf 16/20.
- negativ: der Batch gewichtet den Artefaktzweck teils zu stark und stuft erforderliche Claims als `optional` ab.
- negativ: Status-/Scope-Verschiebungen bleiben riskant, z. B. `undecided` -> `planned` kann weiterhin als
  `covered` fehlklassifiziert werden.

Beispiele fuer Restfehler:

```text
SC-ARCH-SCALABILITY-001 -> requirements
Gold: required + partial
Batch: optional + covered
Problem: architektonische Offenheit wurde aus Requirements-Sicht zu stark abgewertet.

SC-REQ-SAP-WRITE-007 -> requirements
Gold: required + contradicted
Batch: required + covered
Problem: "nicht entschieden" wurde mit "spaeter geplant" gleichgesetzt.

SC-RISK-PRICE-CACHE-015 -> requirements
Gold: required + partial
Batch: optional + missing
Problem: Zielkonflikt Datenaktualitaet/Datenschutz wurde als eher optional statt requirements-relevant bewertet.
```

Aktualisierte Entscheidung:

```text
1 Call pro Artefakt ist fuer produktive Kostenkontrolle vielversprechend.
Der Batch sollte aber nicht blind als finaler Judge dienen.
Sinnvoller Zielpfad: Batch pro Artefakt + gezielte Einzelzellen-Nachpruefung fuer riskante Klassen.
```

Riskante Nachpruefklassen:

```text
partial
contradicted
unclear
optional + missing
required + covered bei Status-/Scope-/Modalitaetsclaims
```

Forschungsbezug:

Der Befund passt zu claim-/evidence-basierten Evaluationsansaetzen: lokale Claim-Relationen sind besser messbar als
globale Dokumenturteile, aber groessere Batches reduzieren Kosten auf Kosten von Aufmerksamkeit, Grenzfallstabilitaet
und klarer Fehlerlokalisierung. Fuer wissenschaftliche Auswertung bleibt die adjudizierte Reference-Matrix der
belastbare Nenner; der Batch ist eine Operationalisierungsvariante, kein Ersatz fuer Referenzurteile.

## Zwischenfazit Coverage-v2 nach Matrix-v2 und Batch

Stand der belastbaren Erkenntnisse:

```text
1. TopicCoverage bleibt ein guenstiger recall-first Baseline-Check.
2. SourceClaim/Matrix ist fachlich staerker fuer konkrete Coverage-Defects.
3. Global Ledger zeigt: relevante SourceClaims lassen sich aus dem Transkript finden, wenn nicht zu frueh priorisiert wird.
4. Matrix-v2 zeigt: Missing ist im adjudizierten Sample sehr stark erkennbar (18/18).
5. Die eigentliche Problemzone ist semantische Grenzziehung:
   covered vs partial vs contradicted,
   required vs optional/not_applicable,
   Status/Scope/Modalitaet.
6. Batch reduziert Call-Kosten, verschiebt aber Fehler in Applicability.
```

Root-Cause-Einordnung:

```text
Das Wurzelproblem ist nicht nur Reviewer-Qualitaet.
Das Wurzelproblem ist freie Artefaktgeneration ohne durchgaengige Provenienz:
Transkriptsemantik wird verdichtet, umformuliert, ausgelassen oder im Status veraendert.
Der nachtraegliche Reviewer muss diese Transformation rekonstruieren.
```

Daraus folgt als naechster forschungsbasierter Testpfad:

```text
Transcript
-> Semantic Source Ledger mit Evidence, Status, Scope, Modalitaet
-> evidence-first Artefaktgeneration mit sourceClaimIds
-> lokale Verifikation der Transformationen
-> gezielte Reparatur nur fuer eindeutig belegte Abweichungen
```

Dieser Pfad wird in
`AgenticSdlc.Host/Phases/Phase2/Evaluation/NextStep/Vorschlag28_6.md`
ausformuliert. Er soll nicht die Matrix-Erkenntnisse ersetzen, sondern das erkannte Problem frueher im Prozess
adressieren: weniger nachtraegliche Rekonstruktion, mehr Traceability by construction.

## Thesis-Satz

```text
Ein isolierter SourceClaim-Coverage-Spike zeigt, dass claim-/evidence-native Coverage konkrete Quellpflichten
feiner pruefen kann als aggregierte TopicCoverage und reproduzierbarer ist als freie DirectReview. Der Ansatz ist
methodisch naheliegend, wird aber wegen manueller SourceClaim-Fixture, offener Artifact-Obligation-Klassifikation
und Integrationsaufwand als spaeterer Ausbau behandelt; TopicCoverage bleibt fuer die aktuelle Closure die
reproduzierbare recall-first Checkliste.
```
