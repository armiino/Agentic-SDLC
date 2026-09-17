# `input/` — Eingaben: Transkripte, Deltas, Referenz-Labels

> Status: LEBEND (README-bei-Code, angelegt 17.09.2026) — bei neuen Eingabe-Arten mitpflegen.

## `transcripts/` — die sechs Meeting-Transkripte

| Datei | Rolle | Belegte Verwendung |
|---|---|---|
| `meeting-2-extended.txt` | **der Benchmark** der Evaluation (W2) | `run-config.json → evidenceAgent.transcript`; Gold-Labels in `eval-labels/`; drei `tools/eval/`-Skripte |
| `meeting-4-ux-block-l.txt` | Standard-Transkript des Betriebswegs | `run-config.json → fullworkflow.transcript` — das nimmt `pipeline-full run` ohne Argument |
| `Interview-Einrichtung.txt` | zweite Quelle der W2-Messung (Interview statt Meeting) | Gold-Labels + fünf `tools/eval/`-Skripte |
| `meeting-3-medikamente.txt` | Betriebs-Demo der Feature-Zuordnung | historische Ledger-Läufe, z. B. `runs/ledger/20260729_142120_9ca536/` |
| `meeting-2-delta.txt` | Folge-Meeting zu meeting-2 (Betriebszweig: Delta statt Erstaufbau) | historische Läufe, z. B. `runs/ledger/20260804_121433_986128/` |
| `T9999_chaos.txt` | früher Stress-/Chaos-Fall aus der Explorationsphase | Test-Fixture (`PipelineFullMessagesTests`), alte Topic-Audits |

Der Ordner ist Konvention, kein Zwang: `pipeline-full run <pfad>` nimmt jeden Pfad, relative
Pfade gelten ab Repo-Wurzel.

## `deltas/` — vorbereitete MeetingDeltas

| Datei | Zweck |
|---|---|
| `leer-delta.json` | das **Leer-Delta** für gezielte Gate-Läufe: der Lauf hält am Entscheidungs-Checkpoint, ohne neue Inhalte einzuspeisen. Der Steward nutzt es für „löse DEC-xxx" (DEC-Auflöse-Rezept im Steward-Prompt) |
| `session2-leer-delta.json` · `session2-teilb-delta.json` | Fixtures der UX-Abnahme-Kampagne vom August 2026 |

## `eval-labels/` — Referenz-Labels und Auswertungsregeln der Messungen

Gold-Standards und Regelwerke, gegen die die Kapitel-7-Zahlen gerechnet werden: die
W2-Gold-Dateien beider Quellen (`*.w2-gold.json`), Matchregeln (`w2-matchregeln.md`),
Annotations-Leitfaden, Prüfsummen (`benchmark-checksums.txt`), dazu ältere Spike-Labels
(`*.grounding-*labels.md`, `*.claim-evidence-spike.json`) und der Referenz-Ledger des
Chaos-Falls. Konsumiert von `tools/eval/`-Skripten und Tests.

> Stand 17.09.2026: 20 der 35 Dateien sind versioniert; die W2-Gold-Dateien sind noch
> nicht committet (bekannter offener Punkt der Abgabe-Vorbereitung).

## Explorations-Reste — bewusst liegen gelassen

Diese beiden Ordner gehören zur **archivierten Evaluations-Ära (S-3, Frühjahr 2026)**.
Kein heutiger Code liest sie; ihre Erzeuger liegen unter `archive/evaluation/` außerhalb
des Builds. Sie bleiben als Beleg der Exploration erhalten.

| Ordner | Was es war |
|---|---|
| `topics/` | Topic-Extraktion/-Audit/-Relevanz je Transkript (`ExtractTopicsRunner`, `TopicAuditRunner`, `ClassifyTopicRelevanceRunner` — alle in `archive/evaluation/PerItem/`) |
| `transcripts_backup/` | frühe Test-Transkripte (`T0001–T0003`, `T8002_tutoring`, `T9999_chaos`) aus der Zeit vor den echten Meeting-Transkripten |
