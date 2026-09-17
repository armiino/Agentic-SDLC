# Belegpaket Kapitel 4

Stand: 2026-09-14 (Bereitstellung vom 13.09., E45-02 ergänzt). Alle 19 E-IDs aus Anhang A sind zugeordnet. Dies ist eine nachträgliche Bereitstellung vorhandener formativer Belege, keine neue Messkampagne und kein System-Freeze.

Lesen Sie zuerst den jeweiligen E-ID-Hinweis. Er benennt Beobachtung, Originaldateien und Grenzen. Die drei bereits vorhandenen Vorstudien bleiben am bisherigen Ort in `thesis-evidence/`; ihre ausgewählten Dateien sind im Manifest gehasht und müssen zusammen mit diesem Paket bereitgestellt werden. `chapter-4/` allein ist daher kein vollständiges Versandarchiv. Weitere erforderliche Eingaben liegen unter `_kontext/`.

**Auswahl:** ausgewählte vollständige Originaldateien statt pauschaler Run-Vollkopien. Bei Aussagen über fehlende Werkzeugaufrufe ist das vollständige relevante Ereignis-/Agentprotokoll enthalten. `manifest.json` inventarisiert auch die nicht übernommenen Dateien der ausgewählten Run-Ordner. Notizauszüge enthalten unveränderten Wortlaut (UTF-8, vereinheitlichte Zeilenenden) mit originalen Zeilenangaben im Manifest; ihre damaligen Interpretationen sind keine neuen Wirkungsnachweise. Auch Formulierungen wie „universell“ oder „Modellrobustheit“ in den unveränderten Vorstudien gelten nicht über die in der Thesis benannten Grenzen hinaus.

**Grenzen:** E43-01 enthält keinen archivierten Modell-Rohtext und keine eindeutige Run-ID in der frühen Notiz. Bei E48-02 ist der ursprüngliche Forward-Lauf gelöscht; Runbook und Alt-Snapshot bilden einen ausdrücklich bezeichneten Ersatzbeleg. E43-05 und E48-03 verlangten eine Präzisierung des bisherigen Thesistexts; die Originale enthalten auch Gegenbefunde. E44-05 trennt autorbestätigte Teilreferenz, unbestätigte Vollreferenz und Adjudikationsimplementierung.

| E-ID | Thema | Status |
|---|---|---|
| [E43-01](E43-01/README.md) | Ankündigung und Werkzeugausführung | Teilbeleg: Laufprotokoll plus Entwicklungsnotiz |
| [E43-02](E43-02/README.md) | Selbstauskunft über die Prüfung | Originalbelege bereitgestellt |
| [E43-03](E43-03/README.md) | Direkte Erzeugung ohne eigenen Quellzugriff | Originalbelege bereitgestellt |
| [E43-04](E43-04/README.md) | Erkundete Kontextstrategien | Originalbelege bereitgestellt |
| [E43-05](E43-05/README.md) | Unterschiedliche Reaktionen auf die Promptschärfung | Originalbelege bereitgestellt; Textkorrektur mitgeliefert |
| [E44-01](E44-01/README.md) | Freie und themengebundene Prüfung | Originalbelege bereitgestellt |
| [E44-02](E44-02/README.md) | Nachträgliche Zuordnung zu Quellen | Originalbelege bereitgestellt |
| [E44-03](E44-03/README.md) | Erzeugung aus einer Evidenzstruktur | Originalbelege bereitgestellt |
| [E44-04](E44-04/README.md) | Urteilsvariation und Per-Item-Prototyp | Originalbelege bereitgestellt |
| [E44-05](E44-05/README.md) | Referenzentscheidung und Adjudikationsstufe | Originalbelege bereitgestellt |
| [E44-06](E44-06/README.md) | Reale Quellenkette zu REQ-13 | Originalbelege bereitgestellt |
| [E45-02](E45-02/README.md) | Strukturelle Prüfung und Reparatur | Originalbelege bereitgestellt |
| [E45-03](E45-03/README.md) | Variabilität des freien Reviewers | Originalbelege bereitgestellt |
| [E46-01](E46-01/README.md) | Fehlerhafte Zuordnungen und Bereinigung | Originalbelege bereitgestellt |
| [E47-01](E47-01/README.md) | Pause und Wiederaufnahme | Originalbelege bereitgestellt |
| [E47-02](E47-02/README.md) | Kontrollierte Ablösung im Experimentlauf | Originalbelege bereitgestellt |
| [E48-01](E48-01/README.md) | Initiale Projektion und gezielte Aktualisierung | Originalbelege bereitgestellt |
| [E48-02](E48-02/README.md) | Veralteter Außenbestand | Dokumentierter Ersatzbeleg; ursprünglicher Forward-Lauf fehlt |
| [E48-03](E48-03/README.md) | Kommentar durch den kontrollierten Rückweg | Originalbelege bereitgestellt; Textkorrektur mitgeliefert |

**Nachtrag P10-4b:** E45-02 enthält zusätzlich das vollständige OTel-Protokoll mit Reparaturauftrag und dem Entwurfsvergleich innerhalb des Reparaturversuchs. Die 19 E-IDs bleiben unverändert; die ursprüngliche ZIP-Fassung vom 13.09. bleibt als historischer Stand erhalten.

## Weitergabe

Das zusätzlich bereitgestellte Archiv `chapter-4-belegpaket-2026-09-14.zip` enthält diesen Ordner und alle 119 eingebundenen Originaldateien der drei Vorstudien in ihrer relativen Ordnerstruktur. Nach dem Entpacken lässt sich `thesis-evidence/chapter-4/verify.py` ohne Zugriff auf das Entwicklungsrepository ausführen. Nur `--originals` benötigt zusätzlich die ursprünglichen Laufarchive und Arbeitsdateien. Das Archiv ist eine Materialauswahl, kein ausführbarer vollständiger Systemstand.

## Prüfung

`python3 verify.py` prüft Paketdateien und die eingebundenen Vorstudien relativ zum Repository. `python3 verify.py --originals` vergleicht zusätzlich alle Originaldateien und Notizauszüge mit ihrem ausgewiesenen Stand. Die historischen JSON-Pfade werden nicht umgeschrieben. `SHA256SUMS` sichert die bereitgestellten Dateien; `pruefung.json` enthält die zusätzlich ausgeführten inhaltlichen Ankerkontrollen. Eine erfolgreiche Hashprüfung beweist Dateiintegrität, keine fachliche Richtigkeit.

Die ausgewählten Transkripte und Entwicklungsnotizen sind Forschungs-/Projektmaterial. Das Paket trifft keine zusätzliche Aussage über Freigaben oder vollständige Anonymisierung; B-50 bleibt separat.
