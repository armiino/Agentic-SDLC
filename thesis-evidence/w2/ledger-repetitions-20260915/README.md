# Ergänzende Ledger-Auswertung: drei historische Stressfall-Läufe

Stand: 15.09.2026. Nachträgliche Auswertung vorhandener Systemausgaben, keine neuen Modellläufe. Auftrag: die beobachtete Streuung und Quellentreue der Evidenzerzeugung genauer untersuchen. Hauptbericht: [Ergebnisse und Interpretation](./bericht.md).

- [Bewertungsleitfaden](./leitfaden.md) und [vor den neuen Urteilen festgehaltene Prüfsummen](./protocol-freeze.json).
- [Begründeter Umfangsnachtrag](./umfangsnachtrag.md): auch die 47 Claims des ursprünglichen Laufs unter demselben Stützungsmaßstab nachprüfen; [zeitlicher Vermerk](./umfangsnachtrag-freeze.json).
- [Alle 327 Coverage-Einträge](./results/coverage-details.json): 109 historische konsolidierte Urteile und 218 neue; [vergleichbare Übersicht](./results/coverage-grid.tsv).
- [Alle 135 neu geprüften Claim-Fälle](./results/claim-quality-details.json), jeweils mit zwei getrennten Urteilen; [Tabellenansicht](./results/claim-quality.tsv).
- [Drei tatsächliche Autorenantworten](./human-review.json); keine unabhängige Zweitannotation. [Daraus folgende Änderungen](./human-revision-log.json).
- [Konsistenzkorrekturen nach der Erstbewertung](./revision-log.json) und [abschließende Differenzierung von B04/R05](./final-consistency-log.json). Ersturteile, Konsistenzstand und endgültiger Stand bleiben als getrennte TSV-Dateien erhalten; Schlüssel sind im Leitfaden und unten erklärt.
- [Maschinenlesbare Ergebnisse](./results/results.json), [Sensitivitäten](./results/sensitivities.json) und [Unverändert kopierte Originale](./originals-manifest.json).

[Abschlussprüfung](./verification.json): Originalkopien, Rechenreproduktion und Urteilsrevisionen. Das [Paketmanifest](./package-manifest.json) bezeichnet die ausgelieferten Dateien.

## Reproduktion

Mit Python 3 aus dem Paketverzeichnis ausführen:

```sh
python3 recompute.py . /tmp/ledger-repetitions-check
```

Das Skript prüft die eingefrorenen Quellen, Referenz-/Claim-IDs, den kanonischen Quelltext, Capture-Prüfsummen und protokollierte Bedingungen. Es berechnet Zähler, Nenner, Überschneidungen und deskriptive Zusammenfassungen aus den gespeicherten Urteilen neu. Es führt keine Modelle aus und ersetzt keine inhaltliche Bewertung. Ein identischer Zahlenoutput belegt rechnerische Reproduktion, nicht die objektive Richtigkeit der semantischen Urteile.

Die Dateien unter `originals/` behalten ihre ursprünglichen Repository-Unterpfade. Das [Manifest](./originals-manifest.json) enthält 39 Originalkopien mit Pfad, Größe und SHA-256. Darunter liegen die Eingaben, drei vollständige bewertete Claim-Bestände, protokollierte Modellanfragen, Capture-Belege und die tatsächlich verwendeten Regel-/Alturteilsdokumente. Kein vollständiger historischer Code-/Umgebungsstand oder unveränderlicher Modellanbieter-Snapshot wird damit behauptet.

## Kennungen und Urteilsschlüssel

R = ursprünglicher Lauf `20260905_164650_8189ea`; A = `20260905_165151_2dde4d`; B = `20260905_165658_1271bb`. Die kurzen IDs R01/A01/B01 sind ausschließlich Kennungen dieses Bewertungsbogens in der Originalreihenfolge der Claims; die tatsächlichen IDs stehen in den Ergebnisdateien. Sie sind keine neuen Core- oder Ledger-Kennungen.

Coverage-TSV: Nummer der Gold-v2-Aussage, F/P/N (full/partial/none), relevante gelesene Claim-IDs, Begründung. Bei `none` nennt das dritte Feld gegebenenfalls den nächstliegenden geprüften Claim, keinen anerkannten Treffer. Die Ergebnis-JSON trennt deshalb `inspected_claim_ids` von `matched_claim_ids`.

Claim-TSV: Review-ID, N/X (keine Quellenverfälschung festgestellt / Verfälschung festgestellt), D/I/P/U (direkt / inferentiell / teilweise gestützt / ungestützt), Begründung. Eine fehlende Qualifikation kann die Coverage senken, ohne den verbleibenden Claim falsch zu machen. Eine tatsächlich veränderte Planungsentscheidung wird dagegen am Propositionstext bewertet; interne Facetten-/Checker-Labels sind hierfür kein Urteil.
