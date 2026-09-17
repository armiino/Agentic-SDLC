# Evidence-first Semantic-Ledger-Spike

## Run

```text
dotnet run --project AgenticSdlc.Host -- \
  evidence-first-spike \
  /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-evidence-first-spike.json \
  open-questions \
  phase2_1 \
  20260612_133345_2d7b09 \
  openai/gpt-5.4
```

## Inputs

```text
Fixture:      /Users/armino/devProjects/Agentic-SDLC/input/eval-labels/semantic-ledger-evidence-first-spike.json
Old artifact: /Users/armino/devProjects/Agentic-SDLC/runs/phase2_1/20260612_133345_2d7b09/snapshots/docs/open-questions.md
Artifact:     open-questions
Model:        openai/gpt-5.4
```

## Outputs

```text
Generated artifact: /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.md
Generated claims:   /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.claims.json
Old claims:         /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.old-claims.json
Verification:       /Users/armino/devProjects/Agentic-SDLC/thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.verification.json
```

## Metrics

Old artifact:

```json
{
  "total": 10,
  "artifactClaims": 28,
  "requiredDispositions": 6,
  "semanticRepresentationCoverage": 0.8,
  "sourceIdDispositionCoverage": 0.4,
  "requiredSourceIdDispositionCoverage": 0,
  "semanticPreservation": 0,
  "preserved": 0,
  "partial": 1,
  "omitted": 2,
  "misrepresented": 6,
  "contradicted": 0,
  "unclear": 1,
  "statusShifts": 3,
  "scopeShifts": 4,
  "unsupportedAssumptions": 28
}
```

Evidence-first artifact:

```json
{
  "total": 10,
  "artifactClaims": 7,
  "requiredDispositions": 6,
  "semanticRepresentationCoverage": 0.8,
  "sourceIdDispositionCoverage": 1,
  "requiredSourceIdDispositionCoverage": 1,
  "semanticPreservation": 0.7,
  "preserved": 7,
  "partial": 0,
  "omitted": 2,
  "misrepresented": 1,
  "contradicted": 0,
  "unclear": 0,
  "statusShifts": 0,
  "scopeShifts": 1,
  "unsupportedAssumptions": 0
}
```

Completeness issues:

```text
Old artifact verifier issues:        1
Evidence-first verifier issues:      0
```

## Interpretation Template

Dieser Spike misst Semantic Preservation und Disposition Coverage gegen eine bestaetigte Fixture.
Er misst nicht die Vollstaendigkeit der automatischen Ledger-Extraction.

## Vier-Artefakt-Nachtest mit Multi-Claim-Verifier

Nach dem ersten `requirements`-Run wurde der Verifier erweitert:

```text
SourceClaim -> mehrere ArtifactClaims
```

Damit kann ein SourceClaim korrekt als erhalten gelten, wenn seine Facetten auf mehrere Artefaktclaims verteilt sind
und gemeinsam proposition/status/modality/scope/timeScope erhalten.

Ausgefuehrte Runs:

```text
dotnet run --project AgenticSdlc.Host -- evidence-first-spike input/eval-labels/semantic-ledger-evidence-first-spike.json requirements phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
dotnet run --project AgenticSdlc.Host -- evidence-first-spike input/eval-labels/semantic-ledger-evidence-first-spike.json architecture phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
dotnet run --project AgenticSdlc.Host -- evidence-first-spike input/eval-labels/semantic-ledger-evidence-first-spike.json risks phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
dotnet run --project AgenticSdlc.Host -- evidence-first-spike input/eval-labels/semantic-ledger-evidence-first-spike.json open-questions phase2_1 20260612_133345_2d7b09 openai/gpt-5.4
```

Outputs:

```text
thesis-evidence/evidence-first-spike/evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.*
thesis-evidence/evidence-first-spike/evidence-first-v2.architecture.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.*
thesis-evidence/evidence-first-spike/evidence-first-v2.risks.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.*
thesis-evidence/evidence-first-spike/evidence-first-v2.open-questions.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.*
```

### Gesamtmetriken

```text
Artifact        Old preserved  Old omitted  Old misrep  New preserved  New omitted  New misrep
requirements    0/10           3/10         2/10        10/10          0/10         0/10
architecture    0/10           3/10         5/10         9/10          1/10         0/10
risks           0/10           1/10         4/10        10/10          0/10         0/10
open-questions  0/10           2/10         6/10         7/10          2/10         1/10
```

Diese Gesamtmetriken enthalten auch `optional` und `not_applicable` Dispositions. Deshalb ist fuer harte Bewertung
die required-only Sicht relevanter.

### Required-only Sicht

```text
Artifact        Required  Old preserved  Old omitted  Old misrep  New preserved  New omitted  New misrep
requirements    10        0/10           3/10         2/10        10/10          0/10         0/10
architecture     6        0/6            2/6          4/6          6/6           0/6          0/6
risks            8        0/8            1/8          3/8          8/8           0/8          0/8
open-questions   6        0/6            2/6          3/6          5/6           0/6          1/6
```

Status-/Scope-Shifts required-only:

```text
Artifact        Old status  Old scope  New status  New scope
requirements    2           1          0           0
architecture    2           1          0           0
risks           3           0          0           0
open-questions  0           3          0           1
```

### Interpretation

Der Vier-Artefakt-Test stuetzt die Evidence-first-Hypothese deutlich:

- `requirements`, `architecture` und `risks` erreichen in der required-only Sicht 100 % Preservation.
- `open-questions` verbessert sich stark, bleibt aber nicht perfekt: 5/6 required preserved, 1/6 misrepresented.
- Alle neuen Artefakte haben `sourceIdDispositionCoverage=1`; alte freie Artefakte haben erwartbar keine echte
  Source-ID-Traceability.
- Die alten Artefakte enthalten viele semantische Verschiebungen und Auslassungen, obwohl sie thematisch oft nah sind.

Der wichtigste verbleibende Fehler liegt bei `open-questions`:

```text
SC-RISK-PRICE-CACHE-015
Gold-Semantik: Caching/Fallbacks fuer SAP-Daten kollidieren mit Datenaktualitaet und Datenschutz.
Evidence-first open-questions: thematisiert Cache/Fallback, bildet den Risikokonflikt mit Datenaktualitaet und
Datenschutz aber nicht voll als offene Frage ab.
Resultat: MISREPRESENTED / SCOPE_SHIFT
```

Zwei weitere neue `OMITTED`-Faelle in `open-questions` sind keine harten Fehler:

```text
SC-REQ-LOGIN-006: disposition.open-questions = not_applicable
SC-RISK-SUPPORT-013: disposition.open-questions = optional
```

### Aktualisierte Entscheidung

```text
Evidence-first/Semantic-Ledger ist jetzt nicht nur ein requirements-spezifischer Befund.
Der Ansatz funktioniert im Spike fuer 3 von 4 Artefakten required-only perfekt und fuer open-questions fast perfekt.
Der offene Punkt ist nicht mehr primaer die Transformation bekannter Claims, sondern:
1. automatische Ledger-Vollstaendigkeit,
2. Disposition-Qualitaet pro Artefakt,
3. offene-Fragen-Formulierung fuer Risiko-/Konfliktclaims,
4. Stabilitaet ueber Wiederholungsruns und groessere Fixtures.
```

## Human-Artefakt aus Evidence-first Claims

Naechster Test:

```text
claims.json
-> lesbares Human-Artefakt mit sichtbaren [SC-...] Referenzen
-> nachtraegliche ArtifactClaim-Extraktion
-> SemanticPreservationVerifier gegen dieselbe Ledger-Fixture
```

Run:

```text
dotnet run --project AgenticSdlc.Host -- \
  human-artifact-spike \
  input/eval-labels/semantic-ledger-evidence-first-spike.json \
  thesis-evidence/evidence-first-spike/evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.claims.json \
  requirements \
  openai/gpt-5.4
```

Outputs:

```text
thesis-evidence/evidence-first-spike/human-artifact.requirements.evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.md
thesis-evidence/evidence-first-spike/human-artifact.requirements.evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.verification.json
```

### Ergebnis

Traceability:

```text
missingVisibleRefs: 0
unknownVisibleRefs: 0
```

Das Human-Artefakt ist lesbar und enthaelt alle erwarteten SourceClaim-IDs sichtbar im Markdown, z. B.:

```text
- Ob SAP-Schreibzugriff erforderlich ist, ist noch nicht entschieden. [SC-REQ-SAP-WRITE-007]
- Push Notifications sind derzeit nicht fuer das MVP entschieden. [SC-REQ-PUSH-010]
```

Semantic-Verifier nach erneuter Claim-Extraktion:

```text
semanticRepresentationCoverage: 1.0
sourceIdDispositionCoverage:   1.0
semanticPreservation:          0.0
preserved:                     0/10
partial:                       3/10
misrepresented:                7/10
statusShifts:                  5
unsupportedAssumptions:        0
```

### Interpretation

Dieser Test zeigt zwei Dinge gleichzeitig:

1. Humanization selbst kann ein brauchbares, lesbares Artefakt mit vollstaendigen Source-Referenzen erzeugen.
2. Die nachtraegliche Re-Extraktion aus Markdown ist wieder ein semantischer Verlustpunkt.

Der zweite Punkt ist wichtig: Der Human-Markdown sieht fachlich korrekt aus, aber der generische
`ArtifactClaimExtractor` interpretiert Formulierungen wie "muss beruecksichtigt werden" teils als staerkere
Modalitaet/Status-Aussage als im Ledger gemeint. Dadurch entstehen kuenstliche `STATUS_SHIFT`/
`MODALITY_SHIFT`-Befunde.

Konsequenz:

```text
Produktiv sollte der Verifier nicht aus dem Human-Markdown erneut Claims rekonstruieren muessen.
Die maschinenlesbaren claims.json bleiben die primaere Semantikschicht.
Das Human-Artefakt ist eine lesbare Projektion mit SourceClaim-Refs.
```

Bessere Zielarchitektur:

```text
Semantic Ledger
-> ArtifactClaims JSON        = pruefbare Semantik
-> Human Markdown [SC-refs]   = lesbare Darstellung
-> Verifier prueft JSON gegen Ledger
-> Markdown-Checker prueft nur:
   - alle SourceRefs sichtbar,
   - keine unbekannten SourceRefs,
   - keine unreferenzierten fachlichen Bullets,
   - Lesbarkeit/Artefakttyp.
```

Damit wird verhindert, dass eine zweite freie Claim-Extraktion aus Markdown neue semantische Fehler einfuehrt.

## Projection-Checker statt Re-Extraction

Daraufhin wurde ein nicht-generativer Projection-Checker gebaut:

```text
claims.json + human.md
-> pruefe entlang bekannter SourceClaim-Refs
```

Run:

```text
dotnet run --project AgenticSdlc.Host -- \
  human-artifact-projection \
  thesis-evidence/evidence-first-spike/evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.claims.json \
  thesis-evidence/evidence-first-spike/human-artifact.requirements.evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.md
```

Output:

```text
thesis-evidence/evidence-first-spike/human-artifact.requirements.evidence-first-v2.requirements.phase2_1_20260612_133345_2d7b09.openai_gpt-5.4.openai_gpt-5.4.projection.json
```

Ergebnis:

```text
sourceRefCoverage:       1.0
unknownRefCount:         0
unreferencedBulletCount: 0
projectedOk:             17/21
projectedWeak:            4/21
projectedMissing:         0/21
projectedChanged:         0/21
```

Interpretation:

- Alle erwarteten SourceClaim-IDs sind im Markdown sichtbar.
- Keine unbekannten SourceClaim-IDs wurden eingefuehrt.
- Keine fachlichen Bullets ohne Source-Referenz.
- Keine fehlende oder geaenderte Projektion.
- Die 4 `projectedWeak` sind heuristische Grenzfaelle der Statussignal-Pruefung, nicht beobachtete semantische
  Aenderungen; die betroffenen Zeilen sind wortgleich beziehungsweise stark deckungsgleich vorhanden.

Damit ist der richtige Pruefpfad bestaetigt:

```text
Harte Semantik:
Semantic Ledger -> ArtifactClaims JSON

Lesbare Darstellung:
ArtifactClaims JSON -> Human Markdown

Projection Check:
Human Markdown enthaelt die bekannten SourceRefs und keine unreferenzierten fachlichen Aussagen.
```

Nicht mehr sinnvoll:

```text
Human Markdown -> freie Claim-Re-Extraction -> Semantikurteil
```

## Semantic-Ledger-Extraction-Spike

Naechster Test:

```text
Transkript
-> automatisch extrahierter facettierter Semantic Ledger
-> Recall-/Facet-Match gegen bestaetigte 10er-Fixture
```

Run:

```text
dotnet run --project AgenticSdlc.Host -- \
  semantic-ledger-extraction-spike \
  input/eval-labels/semantic-ledger-evidence-first-spike.json \
  input/transcripts/T9999_chaos.txt \
  openai/gpt-5.4
```

Outputs:

```text
thesis-evidence/evidence-first-spike/semantic-ledger-extraction.T9999_chaos.openai_gpt-5.4.extracted.json
thesis-evidence/evidence-first-spike/semantic-ledger-extraction.T9999_chaos.openai_gpt-5.4.matches.json
```

Der Extractor nutzte einen starken, facettenorientierten Prompt:

```text
- konkrete pruefbare SourceClaims statt grober Topics
- Status/Modalitaet/Scope/TimeScope explizit
- negative und offene Aussagen priorisieren
- keine Status-/Scope-/Modalitaetsverstaerkung
- Disposition pro Artefakt
- Recall-first, maximal 80 Ledger-Eintraege
```

### Ergebnis

```text
Extracted ledger entries: 87
Expected fixture entries: 10

exact:              2/10
partial:            8/10
missed:             0/10
exactRecall:        20 %
exactOrPartial:    100 %
```

Facet exact:

```text
proposition: 40 %
status:      40 %
modality:    40 %
scope:       20 %
timeScope:   70 %
evidence:    70 %
disposition: 20 %
```

### Interpretation

Der Test ist wichtig, weil er die offene Hauptfrage jetzt isoliert misst:

```text
Findet der automatische Ledger die kritischen Quellclaims?
```

Antwort:

```text
Ja, in diesem 10er-Litmus-Sample geht kein kritischer Claim komplett verloren.
Aber: Die exakte facettierte Ledger-Form ist noch nicht stabil genug.
```

Hauptfehlermodus:

```text
Der Extractor findet den fachlichen Kern,
verteilt ihn aber oft auf mehrere Ledger-Eintraege
oder modelliert Status/Scope/Disposition anders als die Fixture.
```

Beispiele:

```text
SC-REQ-SAP-WRITE-007:
exact. SAP-Schreibzugriff offen/undecided und MVP eher read-only wurden gefunden.

SC-REQ-PUSH-010:
partial. Push als Marketingwunsch/spaeter moeglich wurde gefunden,
aber "nie besprochen" und "nicht im MVP entschieden" wurden abgeschwaecht.

SC-ARCH-SCALABILITY-001:
partial. Nutzerzahl/Performance offen und Skalierung beruecksichtigen wurden gefunden,
aber auf zwei Eintraege verteilt.

SC-REQ-LOGIN-006:
partial. Login und Double-Opt-In wurden gefunden,
aber als getrennte Eintraege und mit nicht exakt passender decided/must-Facette.

SC-RISK-SUPPORT-013:
partial. Kein Ticketsystem und E-Mail-/Datenschutzrisiko wurden gefunden,
aber kausale Aussage und entschiedenes Risiko sind verteilt.
```

### Aktualisierte Entscheidung

Die Pipeline nach dem Ledger ist stark:

```text
Semantic Ledger -> ArtifactClaims JSON -> Human Markdown
```

Der neue Engpass ist die Ledger-Normalisierung:

```text
Transcript -> Candidate Semantic Ledger -> Canonical/merged Semantic Ledger
```

Der automatische Extractor erreicht im Litmus-Sample 100 % exact-or-partial Recall, aber nur 20 % exact. Damit ist er
als Candidate-Ledger brauchbar, aber noch nicht als alleiniger finaler Semantic Contract.

Naechster sinnvoller Schritt:

```text
Candidate Ledger
-> Merge/Canonicalization Step
-> Facet repair/check gegen Evidence
```

Ziel waere nicht mehr, den Extractor-Prompt beliebig zu verschaerfen, sondern die Forschungslinie weiter zu verfolgen:
erst breit extrahieren, dann normalisieren/adjudizieren, dann evidence-first generieren.

## Semantic-Ledger-Canonicalization-Fix

Auf Basis des obigen Fehlermodus wurde ein generischer Fix gebaut:

```text
Transcript
-> Candidate Semantic Ledger
-> Canonical Semantic Ledger
-> Recall-/Facet-Match gegen Fixture
```

Der Fix ist bewusst generisch:

- keine Fixture-IDs im Prompt,
- keine transkriptspezifischen Sonderregeln,
- Merge verteilter Claims,
- Reparatur von Status/Modalitaet/Scope/TimeScope/Disposition,
- keine neuen Claims ohne Candidate-Evidence.

Implementierung:

```text
AgenticSdlc.Host/Phases/Phase2/Evaluation/PerItem/SemanticLedgerCanonicalizer.cs
```

Run:

```text
dotnet run --project AgenticSdlc.Host -- \
  semantic-ledger-extraction-spike \
  input/eval-labels/semantic-ledger-evidence-first-spike.json \
  input/transcripts/T9999_chaos.txt \
  openai/gpt-5.4
```

Outputs:

```text
thesis-evidence/evidence-first-spike/semantic-ledger-extraction.T9999_chaos.openai_gpt-5.4.extracted.json
thesis-evidence/evidence-first-spike/semantic-ledger-extraction.T9999_chaos.openai_gpt-5.4.canonical.json
thesis-evidence/evidence-first-spike/semantic-ledger-extraction.T9999_chaos.openai_gpt-5.4.matches.json
```

### Ergebnis nach Canonicalization

```text
Candidate entries:  80
Canonical entries:  74

exact:              4/10
partial:            6/10
missed:             0/10
exactRecall:        40 %
exactOrPartial:    100 %
```

Vergleich vorher/nachher:

```text
Before canonicalization:
exact:           2/10
partial:         8/10
missed:          0/10

After canonicalization:
exact:           4/10
partial:         6/10
missed:          0/10
```

Facet exact vorher/nachher:

```text
Facet        Before  After
proposition  40 %    80 %
status       40 %    50 %
modality     40 %    30 %
scope        20 %    70 %
timeScope    70 %    90 %
evidence     70 %    70 %
disposition  20 %    30 %
```

### Interpretation

Der Fix wirkt:

- Proposition-Matching steigt deutlich.
- Scope-Matching steigt stark.
- TimeScope wird fast immer korrekt.
- Exact steigt von 2/10 auf 4/10.
- Kein kritischer Claim geht verloren.

Aber der Fix ist noch nicht ausreichend:

- `status` steigt nur leicht.
- `modality` wird sogar schlechter, weil der Canonicalizer teils "open/must_clarify/desired/must_consider" anders
  modelliert als die Fixture.
- `disposition` bleibt schwach.
- Einige Claims bleiben als mehrere kanonische Eintraege statt als ein finaler Contract-Eintrag verteilt.

Aktuelle Lage:

```text
Candidate Ledger:
gut fuer Recall.

Canonical Ledger:
besser fuer Proposition/Scope,
aber noch nicht gut genug als finaler Semantic Contract.

Nochmaliger freier Artefaktreview:
nicht der richtige naechste Hebel.
```

Naechster technischer Fix waere eine gezielte Facet-Repair-Stufe:

```text
Canonical Ledger
-> Facet Validator/Repair
   prueft pro Eintrag direkt gegen Evidence:
   status, modality, scope, timeScope, disposition
```

Dieser Schritt sollte nicht mehr neu extrahieren, sondern nur die vorhandenen kanonischen Eintraege gegen ihre
Evidence stabilisieren.

## Zielarchitektur nach Spike-Befunden

Die Zielarchitektur wird als typisierter Executor-Workflow dokumentiert:

```text
AgenticSdlc.Host/Phases/Phase2/Evaluation/NextStep/LedgerExecutorWorkflow.md
```

Kernentscheidung:

```text
Kein freier Ledger-Agent als Hauptpfad.
Stattdessen ein LedgerBuilderWorkflow aus spezialisierten Executors mit gecachten Zwischenartefakten.
```

Grund:

- der Ledger ist Vertrags- und Messobjekt,
- die Fehlermodi sind bekannt und isolierbar,
- LLMs werden nur fuer semantische Schritte eingesetzt,
- Gruppierung, Quality Gates, Routing und Aggregation bleiben moeglichst deterministisch,
- jeder Executor kann einzeln nachgeschaerft werden, ohne die gesamte Kette teuer neu auszufuehren.

Naechster minimaler Baustein laut Zielarchitektur:

```text
TranscriptSegmenter
-> SegmentCoverageManifest
-> IndependentGapAuditExecutor
```

Begruendung:

Die bisherigen Runs zeigen, dass ein stabiler Semantic Ledger die nachgelagerte
Artefakterzeugung und Review-Pruefung deutlich verbessert. Die offene Kernfrage
liegt aber eine Stufe davor: Ob die initiale Ledger-Kandidatenmenge vollstaendig
genug ist. Deshalb ist der naechste Baustein nicht sofort Clustering oder weitere
Kanonisierung, sondern ein segmentbasierter Gap-Audit gegen das Transkript.

Der Audit bekommt pro Segment den Originaltext und die bereits zugeordneten
Candidate Claims. Er prueft gezielt, ob im Segment noch eine pruefbare Aussage
enthalten ist, die durch keinen Candidate abgedeckt wird. Damit wird die
Vollstaendigkeitsfrage explizit und reproduzierbar gemacht, statt den gleichen
Extractor nur erneut nach "alles gefunden?" zu fragen.

Forschungsbezug:

- Claim-basierte Zerlegung und Coverage-Bewertung: https://arxiv.org/abs/2502.10855
- Komponenten getrennt evaluieren statt nur End-to-End bewerten: https://arxiv.org/html/2408.08067v2
- Evidence/Attribution vor beziehungsweise waehrend der Generierung mitfuehren: https://aclanthology.org/2024.acl-long.182/
- Automatische Attribution/Judging bleibt fehleranfaellig und braucht klare Evidence: https://aclanthology.org/2024.findings-acl.886/

Verknuepfte Zielarchitektur:

- `AgenticSdlc.Host/Phases/Phase2/Evaluation/NextStep/LedgerExecutorWorkflow.md`
- Zukuenftige Run-Logs: `thesis-evidence/ledger-executor-workflow/runs/<workflowRunId>/`

## Generalisierungstest (zweites Transkript) — Schritt 1: Kandidaten-Ledger

Stand: 2026-06-28. Zweck laut Entscheidungsnotiz
`AgenticSdlc.Host/Phases/Phase2/Evaluation/NextStep/ZUSATZ-entscheidungsnotiz-ledger-review.md`
(Abschnitt „Offene Risiken" + E1): Das evidence-first-Substrat ist bisher auf **einem** Transkript
(T9999) bestaetigt. Bevor darauf L3 gebaut wird, muss geprueft werden, ob es **ausserhalb von T9999**
traegt. Schritt 1 ist die Vorbereitung: ein **Kandidaten-Ledger** fuer das zweite Transkript, aus dem
manuell eine bestaetigte Fixture abgeleitet wird (Kontrollbedingung — der Autor bestaetigt, nicht das
Modell).

### Code-Aenderung (begruendet)

Der bestehende `semantic-ledger-extraction-spike` verlangt zwingend eine Fixture und matcht direkt
dagegen — fuer ein NEUES Transkript (noch keine Fixture) ein Henne-Ei-Problem. Daher ein minimaler
Extract-only-Befehl ergaenzt (= Aufgabe des ersten Executors im Zielbild, ohne Match):

```text
Neu:     AgenticSdlc.Host/Phases/Phase2/Evaluation/PerItem/SemanticLedgerExtractRunner.cs
Dispatch: AgenticSdlc.Host/Program.cs  (Befehl: semantic-ledger-extract)
Build:    dotnet build AgenticSdlc.Host/AgenticSdlc.Host.csproj  -> 0 Warnungen, 0 Fehler
```

### Run

```text
dotnet run --project AgenticSdlc.Host -- \
  semantic-ledger-extract \
  input/transcripts/Interview-Einrichtung.txt \
  openai/gpt-5.4
```

Modell bewusst `openai/gpt-5.4` (gleiches Modell wie die T9999-Baseline → fairer Vergleich).

### Inputs / Outputs

```text
Transkript: input/transcripts/Interview-Einrichtung.txt  (44 KB, Domaene: App fuer Pflegeeinrichtung)
Extracted:  thesis-evidence/evidence-first-spike/semantic-ledger-extract.Interview-Einrichtung.openai_gpt-5.4.extracted.json
Canonical:  thesis-evidence/evidence-first-spike/semantic-ledger-extract.Interview-Einrichtung.openai_gpt-5.4.canonical.json
Run-Log:    (Konsolenausgabe) extracted=60 -> canonical=53
```

### Befund

```text
Extrahiert:  60 Eintraege
Canonical:   53 Eintraege   (Merge entfernte 7 Duplikate/Splits)

Verteilung canonical:
  kind:      requirement 25 · compliance_constraint 7 · open_requirement 5 · constraint 5 ·
             decision 4 · open_question 3 · non_functional_requirement 2 · risk 1 · process_constraint 1
  status:    desired 22 · decided 16 · open 10 · uncertain 2 · required 1 · optional 1 · rejected 1
  modality:  desired 20 · must 12 · must_clarify 7 · must_note 6 · must_consider 6 · optional 2
  riskLevel: low 27 · medium 16 · high 10
  required-Dispositions: requirements 32 · architecture 29 · risks 15 · open-questions 10
```

**Interpretation:**

- Die Domaene ist **deutlich anders als T9999** (chaotischer Software-Kickoff): hier ein Interview ueber
  eine App fuer eine **Pflegeeinrichtung** — Bewohnerprofile, Datenschutz/Einwilligung, Login/
  Zugriffsbeschraenkung, Mandanten-Isolation, Medikationsvertraulichkeit. Damit ist es als
  Generalisierungs-Transkript geeignet (nicht dieselbe Themenstruktur).
- Der Extractor produziert dieselben Fehlermodi wie bei T9999 (`evidence.md`, Abschnitt
  „Semantic-Ledger-Extraction-Spike"): **gute Recall-Breite, aber Facetten nicht stabil** — viele
  `desired`/`must_*`-Abstufungen, die genau die in E5 der Entscheidungsnotiz beschriebene strukturelle
  Decke (Status/Modality/Disposition) treffen. Das ist **erwartetes** Verhalten, kein neuer Fehler.
- Die harten Facettentypen sind im Sample vorhanden und damit testbar: `decided/must` (Zugriffsregeln),
  `open/must_clarify` (offene Umsetzung), `uncertain/must_consider` (Aktenzugriff), **`rejected`**
  (bidirektionale Uebersetzung ausgeschlossen), `optional`/`desired+later_possible` (Kalender/
  Notifications) — also genau die Shift-anfaelligen Faelle, die die Fixture pruefen soll.

### Bestaetigte Fixture (Stand fuer Schritt 2)

Aus dem Kandidaten-Ledger wurde eine Fixture kuratiert und vom Autor geprueft/korrigiert:

```text
input/eval-labels/semantic-ledger-interview-spike.json   (11 Eintraege, bestaetigt)
```

Spread ueber die harten Facetten: `decided/must` (Login 001, Eigen-Profil 002, Tenant-Isolation 003,
Kalender-Bereich 010), `open/must_clarify` (Tenant-Umsetzung 004, Bild-Einwilligung 005), `uncertain`
(Aktenzugriff 006), `open/must_note` Risiko (Datenschutz-Tiefe 007), **`rejected`** (Uebersetzungs-
loesung 008), `desired/optional spaeter` (Benachrichtigungen 009, Medikationsinhalte 011).

Kuratierung: 12 Kandidaten -> 10 (2 redundante/schwache verworfen), dann **Autor-Review mit 3
Korrekturen** -> 11:

```text
1. SC-ACCESS-TENANT-ISOLATION-003: open-questions optional -> not_applicable
   (Entscheidung getroffen; die offene Umsetzung ist separat in 004 modelliert).
2. SC-PRIVACY-MEDIA-CONSENT-005: requirements representationMode constraint -> open_decision
   (Einwilligung noch zu klaeren, nicht geloest).
3. SC-FEAT-CALENDAR-OPTIONAL-010 -> Split:
   010 Kalender als eigener App-Bereich (decided/must; im Transkript ab Z.228 strukturell als
       Profil-Bildschirm behandelt, nicht bloss optionale Erweiterung)
   011 Medikationsinhalte/vertrauliche Termine (desired/must_consider, risks+open-questions required).
```

Alle Evidence-Quotes wurden gegen das Transkript **verbatim verifiziert** (0 nicht-matchende Fragmente
ueber alle 11 Eintraege).

Naechster Schritt: Schritt 2 = `evidence-first-spike` fuer die 4 Artefakte gegen diese Fixture, mit
Vergleichs-Run `phase2B/20260623_154552_28a52b` (das B40/B41-Interview-Run) -> Generalisierungs-Vergleich
gegen die T9999-Zahlen.

### Was (noch) NICHT belegt ist

- Dies ist bisher **nur** Extraktion + Fixture-Kuratierung — **kein** Fixture-Match, also noch **keine**
  Recall-/Facet-/Preservation-Zahl fuer Interview. „Substrat generalisiert" ist **vorbereitet, nicht belegt**.
- **Validitaets-Vorbehalt:** Die Fixture wurde modell-extrahiert und (mangels zweitem Labeler)
  modell-kuratiert + autor-korrigiert. Single-Labeler bleibt Limitation (E5): spaetere Facet-Zahlen
  messen teils Modell-/Autor-Auslegung, nicht reine Korrektheit.

### Runner-Fix (Nebenbefund)

`EvidenceFirstSpikeRunner` schrieb seine Pro-Run-Zusammenfassung in den **festen** Namen
`thesis-evidence/evidence-first-spike/evidence.md` und ueberschrieb damit bei jedem Lauf diese
kumulative Datei. Behoben: Zusammenfassung jetzt **scoped** (`{prefix}.evidence.md`); die kumulative
`evidence.md` wird vom Runner nicht mehr angefasst.

## Generalisierungstest — Schritt 2: A-vs-B (Interview, 4 Artefakte)

Stand: 2026-06-28. Frage: **Generalisiert der evidence-first-Vorsprung (B) ueber das freie Artefakt (A)
auf ein zweites, anderes Transkript?** Beide Arme gegen dieselbe bestaetigte 11er-Interview-Fixture,
gleicher Verifier.

### Run

```text
for a in requirements architecture risks open-questions; do
  dotnet run --project AgenticSdlc.Host -- evidence-first-spike \
    input/eval-labels/semantic-ledger-interview-spike.json \
    $a phase2B 20260623_154552_28a52b openai/gpt-5.4
done
```

A = altes freies Artefakt aus `runs/phase2B/20260623_154552_28a52b/snapshots/docs/<artifact>.md`
(B40/B41-Interview-Run). B = aus der Fixture generiert. ~16 LLM-Calls, alle exit 0.
Outputs je Artefakt: `thesis-evidence/evidence-first-spike/evidence-first-v2.<artifact>.phase2B_20260623_154552_28a52b.openai_gpt-5.4.{md,claims.json,old-claims.json,verification.json,evidence.md}`.

### Gesamtmetriken (alle Dispositions)

```text
Artifact        Preserv OLD->NEW   omit O/N   misrep O/N   statShift O/N   scopeShift O/N   srcIdCov O/N   unsupported O/N
requirements    0.00 -> 0.82       5/0        5/0          3/0             3/0              0.36/1.00      47/0
architecture    0.00 -> 0.91       3/0        8/0          7/0             4/0              0.36/1.00      35/0
risks           0.00 -> 0.91       5/0        1/0          2/0             0/0              0.27/1.00      37/0
open-questions  0.00 -> 0.45       3/5        1/0          5/0             0/0              0.55/1.00      19/0
```

### Required-only Sicht (das ehrliche Lineal)

```text
Artifact        Required  OLD preserved/partial/omit/misrep   NEW preserved/partial/omit/misrep
requirements    7         0 / 1 / 3 / 3                        5 / 2 / 0 / 0
architecture    7         0 / 0 / 1 / 6                        6 / 1 / 0 / 0
risks           8         0 / 1 / 2 / 1                        8 / 0 / 0 / 0
open-questions  5         0 / 2 / 0 / 0                        4 / 1 / 0 / 0
```

Status-/Scope-Shifts required-only sind in B **durchgaengig 0** (OLD: req 0/0… arch hatte in der
Gesamtsicht 7 statusShifts, in required-only werden diese als MISREPRESENTED gezaehlt).

### Interpretation

- **Der Mechanismus generalisiert auf die zweite Domaene.** Fuer `requirements`, `architecture`,
  `risks` schlaegt B das freie Artefakt klar: alle required-Eintraege sind preserved-or-partial,
  **0 misrepresented, 0 Status-/Scope-Shifts**, `sourceIdDispositionCoverage` 0.27–0.36 → **1.00**,
  `unsupportedAssumptions` 35–47 → **0**. `risks` ist required-only perfekt (8/8 preserved).
- **Die alten freien Artefakte sind required-only durchgaengig 0 preserved** — mit vielen
  Misrepresentations (architecture **6/7**!) und Auslassungen. Genau das T9999-Muster, reproduziert.
- **open-questions ist NICHT die Schwachstelle, die die Gesamtsicht suggeriert.** Die Gesamtsicht zeigt
  `omitted 3→5` (scheinbare Regression), aber **required-only** ist B `4 preserved + 1 partial / 5,
  0 misrep, 0 Shifts`. Die 5 „omitted" sind die fuer open-questions `not_applicable`/`optional`
  gesetzten Eintraege (Access-Entscheidungen 001/002/003, Ausschluss 008, Kalender-Bereich 010) — also
  **korrekt nicht** als offene Frage dargestellt, kein Fehler. (Gleiche Nuance wie bei T9999.)

### Erfolgskriterien (Vorschlag_28_6_umsetzung §Erfolgskriterien)

```text
Disposition Coverage nicht schlechter:   erfuellt (srcIdCov 0.27–0.55 -> 1.00 ueberall)
Status-/Scope-Shifts -50%:               weit uebererfuellt (-> 0)
Unsupported Assumptions -50%:            weit uebererfuellt (-> 0)
Semantic Preservation +10pp:             weit uebererfuellt (0 -> 0.45–0.91)
UNCLEAR nicht hoeher:                    erfuellt (0)
Artefakte lesbar:                        Sichtpruefung der .md offen
```

### Vergleich zu T9999 (erste Domaene)

Gleiches Muster, beide Domaenen: required-only erreichen `requirements/architecture/risks` ~100 %
preserved-or-partial mit 0 harten Fehlern; `open-questions` ist required-only sauber (Interview sogar
ohne den 1 misrep, den T9999 noch hatte). Der Befund ist damit **nicht T9999-spezifisch**.

### Was das (weiterhin) NICHT belegt

- **Transformation eines bestaetigten Vertrags**, nicht Ledger-Auto-Vollstaendigkeit. Die Fixture ist
  bestaetigt/eingefroren; ob ein automatischer Ledger so gut waere, ist hier nicht gemessen.
- **Asymmetrie by design:** B hatte den Vertrag als Input, A nicht. Der Test misst „ledger-gefuehrt vs.
  frei", nicht zwei gleich informierte Generatoren.
- **MODELL-CONFOUND (wichtig):** Das alte Artefakt A wurde mit **`openai/gpt-4.1-mini`** erzeugt
  (`runs/phase2B/20260623_154552_28a52b/config.json`), B mit **`gpt-5.4`**. Der Vergleich aendert also
  **zwei** Variablen (Mechanismus + Generatormodell). Folge fuer die Interpretation:
  - **Modellunabhaengig (haelt):** `sourceIdDispositionCoverage 0->1` und `unsupportedAssumptions ->0`
    sind strukturell — freie Generation hat per Konstruktion keine sourceClaimIds, auch mit gpt-5.4.
  - **Confounded (teils Modell):** Preservation-/Shift-Magnituden — ein staerkeres Modell driftet auch
    frei weniger. Richtung = Mechanismus, Groesse nicht sauber zuordenbar.
  - **Derselbe Confound betrifft den T9999-Abschnitt** oben (altes Artefakt `2d7b09` = ebenfalls
    `gpt-4.1-mini`).
  - **Sauberer Fix (offen):** freies Artefakt mit `gpt-5.4` neu erzeugen (A') -> A' vs. B isoliert den
    Mechanismus.

## Generalisierungstest — Schritt 3: Modell-Confound aufgeloest (A vs A' vs B)

Stand: 2026-06-30. Um den Modell-Confound aus Schritt 2 zu entfernen, wurde das freie Artefakt **mit
demselben Modell wie B (gpt-5.4)** neu erzeugt = **A'**. Damit ist beim Vergleich A' vs B die **einzige**
Variable der Mechanismus (frei vs. ledger-gefuehrt).

### A'-Generierung (Config identisch zu A, nur Modell getauscht)

```text
Run A':   runs/phase2B/20260630_145558_68149f   (model openai/gpt-5.4, strategy artifact_state,
          Prompts …PromptB1 — identische Config wie 28a52b, nur agentModel mini->gpt-5.4)
Isolation: input/transcripts/ waehrend des Laufs auf NUR Interview-Einrichtung.txt reduziert
           (Agenten entdecken Transkripte per fs_list; T9999 temporaer ausgelagert, danach restauriert)
A'-Artefakte deutlich umfangreicher als A (10–17 KB vs 2–4 KB).
```

### Required-only (preserved / partial / omitted / misrepresented)

```text
Artifact        req | A  mini-frei            | A' gpt5.4-frei          | B  gpt5.4-ledger
requirements     7  | P0 part1 om3 mis3       | P0 part5 om0 mis2       | P5 part2 om0 mis0
architecture     7  | P0 part0 om1 mis6       | P0 part2 om0 mis5       | P5 part1 om0 mis1
risks            8  | P0 part1 om2 mis1 (oth4)| P0 part4 om0 mis2 (oth2)| P7 part1 om0 mis0
open-questions   5  | P0 part2 om0 mis0 (oth3)| P0 part4 om0 mis0 (oth1)| P3 part0 om0 mis2
```

### Gesamt (alle Dispositions): preservation | statusShift/scopeShift | sourceIdCov | unsupported

```text
Artifact        A mini-frei            A' gpt5.4-frei           B gpt5.4-ledger
requirements    0.00 | 3/3 | .36 | 47  0.00 | 3/1 | .36 | 66    0.82 | 0/0 | 1.00 | 0
architecture    0.00 | 7/4 | .36 | 35  0.00 | 5/3 | .36 | 174   0.82 | 0/0 | 1.00 | 0
risks           0.00 | 2/0 | .27 | 37  0.00 | 4/3 | .27 | 133    0.82 | 0/0 | 1.00 | 0
open-questions  0.00 | 5/0 | .55 | 19  0.00 | 3/1 | .55 | 52     0.36 | 1/0 | 1.00 | 0
```

### Interpretation — der Confound bricht, der Befund haelt

- **A' (gpt-5.4 frei) erreicht required-only weiterhin 0 PRESERVED** auf allen 4 Artefakten. Das
  staerkere Modell hat das Problem **nicht** geloest. Nur **B (gpt-5.4 ledger)** erreicht echte
  Preservation (P5/P5/P7/P3). → Bei **gleichem Modell** ist der Vorsprung der **Mechanismus**, nicht das
  Modell. Der Confound aus Schritt 2 ist damit ausgeraeumt.
- **Was das staerkere Modell frei aendert (A -> A'):** weniger Auslassungen, mehr `partial` (es erwaehnt
  mehr Themen), aber **keine** echte Treue — **0 preserved**, weiter Misrepresentations. Modell hilft der
  **Coverage**, nicht der **Faithfulness**.
- **Grounding wird durch ein staerkeres Modell sogar SCHLECHTER:** A' schreibt umfangreicher und erzeugt
  damit **mehr** unbelegte Aussagen (`unsupportedAssumptions` 35–47 -> **52–174**), `sourceIdCoverage`
  bleibt 0.27–0.55. B hat 0 / 1.00 — strukturell, modellunabhaengig (frei erzeugt keine Quell-IDs).
- **B-Schwachstelle bleibt open-questions** (P3, 2 misrep von 5) — konsistent ueber Runs.

### Nebenbefund: B-Streuung run-zu-run (Wiederholung)

B wurde in Schritt 2 (gegen 28a52b) und Schritt 3 (gegen 68149f) je neu generiert -> zwei Messungen
desselben Mechanismus:

```text
required preserved   Schritt2-B   Schritt3-B
requirements         5            5
architecture         6            5
risks                8            7
open-questions       4            3
```

B variiert ~1 Eintrag/Artefakt run-zu-run (LLM-Nichtdeterminismus trotz temp 0). Stark, aber **nicht
perfekt stabil** — bestaetigt die offene O4-/Wiederholungs-Limitation. Fuer belastbare Zahlen braucht es
mehrere Wiederholungen; fuer die **Richtung** (frei 0 vs. ledger hoch) ist der Effekt robust.

### Fazit Schritt 3

```text
Der evidence-first-Vorteil ist der MECHANISMUS, nicht das Modell:
bei gleichem gpt-5.4 erreicht freie Generierung 0 preserved (required-only) + mehr unbelegte Aussagen,
ledger-gefuehrte Generierung erreicht hohe Preservation + volle Quell-ID-Abdeckung + 0 Shifts.
```
- **Validitaet:** LLM-Verifier gegen **autor-korrigierte Single-Labeler-Fixture**; misst teils
  Modell-/Autor-Auslegung. **n=2 Transkripte, 1 Modell, keine Wiederholungsruns** — offen: O4-Streuung,
  zweites Judge-Modell.
- Die `partial`-Faelle (req 2, arch 1, open-q 1) sind nicht perfekt erhalten, nur kein harter Fehler.

---

## Verwandte Dokumentation / Querverweise

Dieser Spike steht am Ende einer Kette. Vollstaendiger Ueberblick: `thesis-evidence/README.md`.

### Vorlaeufer-Evidenz (warum es diesen Spike gibt)

```text
thesis-evidence/D1-direct-vs-topic/evidence.md          TopicCoverage modell-robust, Direct verrauscht (B45)
thesis-evidence/grounding-spotcheck/evidence.md         Grounding = Sieb, kein Messwert; kein LLM ist GT (B46-B50)
thesis-evidence/claim-grounding-spike/evidence.md       Claim+Evidence loest beide Fehlerklassen; EvidenceSelector
                                                        validiert (27/28, 0 Fabrication) (B47/B51)
thesis-evidence/source-claim-coverage-spike/evidence.md WURZEL-URSACHE: freie Generierung ohne Provenienz
                                                        -> begruendet den Ledger/evidence-first-Pfad
```

### Plan-/Entscheidungs-Dokumente (privat, gitignored, unter NextStep/)

```text
Vorschlag_28_6_umsetzung.md                  Spike-Design (Hypothese, Datenmodell, Erfolgskriterien)
LedgerExecutorWorkflow.md                    grosse Zielarchitektur (12 Stufen) — Backlog
LedgerExecutorWorkflow-smallVersion.md (v2)  verschlankter, offizieller Bauplan (3 Executoren + Gate +
                                             Human Gate + Vertrags-/Freigabesemantik)
ZUSATZ-entscheidungsnotiz-ledger-review.md   E1-E6: Vertrag != Builder, Wichtig vs Vorteil, Grenzen
```

### Fixtures (bestaetigte Ledger = Messlatte)

```text
input/eval-labels/semantic-ledger-evidence-first-spike.json   10 Eintraege (T9999_chaos)
input/eval-labels/semantic-ledger-interview-spike.json        11 Eintraege (Interview-Einrichtung, autor-korrigiert)
```

### Runs (rohe Lauf-Daten unter runs/)

```text
runs/phase2_1/20260612_133345_2d7b09   T9999  — altes freies Artefakt A (gpt-4.1-mini)
runs/phase2B/20260623_154552_28a52b    Interview — altes freies Artefakt A (gpt-4.1-mini, B40/B41)
runs/phase2B/20260630_145558_68149f    Interview — A' freies Artefakt (gpt-5.4)  -> Commit c34b9a0
```

### Commit-Anker dieses Strangs

```text
01ef8d3  Code: Evidence-first + Semantic-Ledger + Human-Artifact-Spikes
670f17c  Daten: thesis-evidence/evidence-first-spike/* + Fixtures + README
c34b9a0  A'-Run runs/phase2B/20260630_145558_68149f (Modell-Confound-Isolierung)
(Dispatch-Wiring Program.cs + .gitignore: separater Commit — s. git log)
```

### Code (Spike-Klassen, AgenticSdlc.Host/Phases/Phase2/Evaluation/PerItem/)

```text
SemanticLedgerExtractor.cs / SemanticLedgerCanonicalizer.cs   Transcript -> Candidate/Canonical Ledger
SemanticLedgerExtractRunner.cs                                Extract-only (Fixture-Vorbereitung)
SemanticLedgerExtractionSpikeRunner.cs / SemanticLedgerRecallMatcher.cs   Extraktion + Recall/Facet-Match
EvidenceFirstArtifactGenerator.cs / EvidenceFirstSpikeRunner.cs   Ledger -> evidence-first Artefakt + Claims
SemanticPreservationVerifier.cs                              Ledger-Entry vs ArtifactClaim (Facet-Erhaltung)
ArtifactClaimExtractor.cs                                    freies Artefakt -> Claims (Arm A)
HumanArtifact{Generator,ProjectionChecker,ProjectionRunner,SpikeRunner}.cs   lesbare Markdown-Projektion
```
Wiederverwendbar fuer den Ledger-Bau: der validierte **ClaimSplitter + EvidenceSelector** aus
`claim-grounding-spike` (Evidence-Bindung der Candidates).
