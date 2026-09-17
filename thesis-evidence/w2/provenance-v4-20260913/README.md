# Herkunftsaudit v4 – selbstständig lesbares Belegpaket

Instrument und Ergebnis: 13.09.2026. Zentrale Bereitstellung und technische Wiederholung: 15.09.2026.

- [Prüfvertrag](./instrument/pruefvertrag.md), [Verfahrensentwicklung und Grenzen](./instrument/verfahrensentwicklung.md).
- [Ergebnis für 237 historische Core-Items](./instrument/final-result/audit.json), [Einzelfälle](./instrument/Einzelfaelle.md), [Besuchsbeispiel](./instrument/Besuchsbeispiel.md).
- [414 tatsächlich gelesene Originaldateien mit SHA-256](./instrument/final-result/sources.json); die Dateien liegen unverändert unter [originals/](./originals/) in ihrer Repository-Struktur. Zusätzlich ist dort das historische v3-Skript für dessen Identitätsprüfung enthalten.
- [Unverändertes Instrument](./instrument/audit_v4.py), [34 künstliche Kontrollfälle](./instrument/test_audit_v4.py), [Prüfergebnis der Bereitstellung](./verification-20260915/checks.json) und [neues Kontrollprotokoll](./verification-20260915/controls.log).

Die Werte gelten für den archivierten Core mit SHA-256 `afcba4bac58ba82fb30b4a413d977f6474be384a377765c0bdb0dafc7bcba22c`. Sie werden weder auf spätere Bestände übertragen noch zu einem Nachweis allgemeiner semantischer Richtigkeit erweitert. Die Auflösungsregeln und Ausnahmen stehen im Prüfvertrag.

## Wiederholen ohne Zugriff auf die Arbeitsordner

Python ab 3.10, Standardbibliothek; aus diesem Paketordner:

```sh
python3 instrument/audit_v4.py --repo originals --core state/core/project-state.json --old-audit runs/e2e-evidenz/traceability-audit.json --out /tmp/thesis-provenance-v4-result
```

Das Ausgabeziel darf noch nicht existieren. Der Aufruf schreibt nur dorthin. Die erzeugten Dateien `audit.json` und `sources.json` können byteweise mit `instrument/final-result/` verglichen werden. Genau diese Wiederholung wurde bei der Bereitstellung durchgeführt: Beide Dateien sind byteidentisch. Außerdem wurden alle 414 Quellen, 1.617 JSON-Fundstellen und 677 eingebettete Objektvorkommen geprüft; die 34 künstlichen Kontrollen bestanden erneut. Diese Zählungen sind technische Prüfoperationen, keine unabhängigen empirischen Fälle.

Die ursprüngliche Instrument-README und `verify_result.py` bleiben unverändert erhalten. Letzteres erwartet eine daneben liegende `reproduction/` und schreibt `verification.json`; für eigene Prüfungen daher eine Arbeitskopie des Instrumentordners verwenden. Die bereitgestellten Originalergebnisse sollen erhalten bleiben. Der obige Audit-Aufruf genügt zur portablen Ergebnisreproduktion; er ruft keine Modelle auf.
