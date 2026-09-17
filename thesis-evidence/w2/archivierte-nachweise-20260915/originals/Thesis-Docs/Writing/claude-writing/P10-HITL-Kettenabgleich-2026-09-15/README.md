# Reproduktion der Bestandsprüfung

Ausführung aus diesem Verzeichnis (Python 3, keine Zusatzbibliotheken):

```sh
python3 inspect.py --repo /Pfad/zu/Agentic-SDLC --tested-manifest tested-files-20260911.json --out /Pfad/zu/neuem-Ausgabeordner
```

Das Repository und der historische Git-Commit müssen lesbar sein. Das Skript schreibt nur in `--out`; dafür einen neuen Ordner verwenden. Die mitgelieferte Prüfsummenliste ist aus dem gesicherten Testmanifest vom 11.09. und den dokumentierten fünf Dateiübernahmen abgeleitet. Aktuelle Unterschiede werden berichtet, nicht ausgeglichen.

`paired-claims.json` enthält 60 Claim-Vergleiche einschließlich eines **abgeleiteten** Zwischenzustands; `decision-items.json` umfasst alle 52 Entscheidungen. `semanticRating: not_performed` bedeutet, dass die fachliche Bewertung noch aussteht. Die Dateien enthalten Quellmaterial und sind interne Belegaufbereitung; ihre Bereitstellung folgt dem gesonderten Material-/Freigabeabschluss der Thesis.

`source-manifest.json` hält die gelesenen Quellen fest. `current-tested-comparison.json` und die datierten Testergebnisse unterscheiden Standzuordnung von neuer Testausführung. Es wurden keine Modelle oder Systemworkflows gestartet. Aus den abgelegten Mengen folgt keine semantische Erfolgsquote.
