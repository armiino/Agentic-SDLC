# Belege zur Masterarbeit

Stand der Bereitstellung: 15.09.2026; Ergänzung der Bewertungsunterlagen am 16.09.2026. Dieser Index erschließt die Projektevidenz zur Arbeit „Einsatz von KI-Agenten in den frühen Phasen des Software Development Lifecycle“. Er führt von einer Aussage zum zugehörigen Verfahren, Ergebnis und Originalbeleg. Die Ablage ist eine lokale Vorbereitung des Abgabepakets; der finale Artefakt-Commit und der Material-/Nutzungsstatus sind noch offen.

## Einstieg nach Prüfgegenstand

| Prüfgegenstand | Belegzugang | Aussageweite |
|---|---|---|
| Explorative Beobachtungen und Designentscheidungen, Kapitel 4 / Anhang A | [Kapitel-4-Paket mit 19 E-IDs](./chapter-4/README.md) | Ausgewählte formative Beobachtungen, einschließlich benannter Teil- und Ersatzbelege; keine allgemeine Wirksamkeitsmessung. |
| Ledger/F-Hauptvergleich, Stufenanalyse und Modellsensitivität, 7.2/7.3 / D.1–D.5 | [Originale und Zuordnung](./w2/archivierte-nachweise-20260915/README.md), [Auswertungsdateien](./w2/eval/), [Nachreview-Konsolidierung](./w2/eval/w2-nachreview-konsolidierung.json) | Ausgaben, Goldstand, Einzelurteile, Konfigurationen und Logs der angegebenen historischen Läufe. Die Hauptauswertung wird nicht rückwirkend zu einer kontrollierten Messung des finalen Codes. |
| Wiederholungen des Ledger-Stressfalls / D.11 | [Bericht und Einzelurteile](./w2/ledger-repetitions-20260915/bericht.md), [Paket und Berechnung](./w2/ledger-repetitions-20260915/README.md) | Drei vorhandene Ausgaben desselben Falls; ergänzende Inhaltsprüfung und gezielte Autorenurteile. Keine unabhängige Zweitannotation oder allgemeine Stabilitätsgarantie. |
| HITL, Fortschreibung, Pause/Resume, Bootstrap und Architekturpfad / D.6 | [Lauf- und Kontrollbelege](./w2/archivierte-nachweise-20260915/README.md#governance-und-technische-nachweise) | Historische Entscheidungen und gespeicherte Wirkungen, getrennt von Experimentmodi, Dry-Run und späteren technischen Prüfungen. |
| Herkunft und Beziehungen im historischen Core, 7.4 / D.10 | [Herkunftsaudit v4 mit Originaldateien](./w2/provenance-v4-20260913/README.md) | Technische Auflösbarkeit der definierten Pfade im untersuchten Bestand; keine pauschale semantische Richtigkeit und keine vollständige Änderungshistorie. |
| Inhaltliche Prüfung ausgewählter Einordnungsfälle, 7.5 / D.12 | [Ergebnisse und Belegketten](./w2/core-content-audit-20260915/evaluation/README.md), [gesicherte Auswahl](./w2/core-content-audit-20260915/README.md) | Acht regelgebunden ausgewählte zusätzliche Fälle und vier getrennte bekannte Kontrollen; positive, begrenzte und ungeklärte Ergebnisse. Keine repräsentative Erfolgsquote. |
| Tokenverbrauch der vier Hauptläufe | [Korrektur und Rechenregel](./w2/token-usage-correction-20260910.md), [Rechenskript](./w2/recompute-token-usage.py), [portable Ausführung](./w2/archivierte-nachweise-20260915/README.md#technische-nachrechnung) | Auswertung archivierter Zähler, geprüft gegen einzelne Chat-Spans; keine Rechnungskontrolle oder neue Modellmessung. |
| Agentenbeitrag und ganze PBIs/Issues: KI-Erstauswertung vom 16.09. | [Ergebnisbericht, Einzelurteile und Autorenprüfung](./w2/agent-artifact-evaluation-20260916/evaluation/README.md) | 17 Vorschläge, drei PBI-/Issue-Paare; nachrechenbare Zählungen und begründete Ersturteile. Sechs punktuelle Autorenrückmeldungen und spätere HTML-Durchsicht (siehe Bewertungsnachtrag), keine unabhängige Annotation. |
| Aktueller Bewertungsnachtrag vom 16.09. | [Maßstabskorrekturen und Autorenprotokoll](./w2/bewertungskonsolidierung-20260916/README.md) | Zwei korrigierte IDE-Urteile (Ledger A41 und F C-086), neue Zählungen und vollständige HTML-Durchsicht laut Autor. Keine erfundene menschliche Einzelannotation; historische Stände bleiben erhalten. |
| R3: Entwurfsanker, Governance-Modi und Standzuordnung | [Prüfung, Primärbelege und Integrationsprotokoll](./r3-standabgleich-20260916/README.md) | Inventur aller 143 Fullworkflow-Verzeichnisse; 685 Dateien entsprechen dem Teststand vom 11.09. Kein neuer Lauf und noch kein finaler Integrationsnachweis. |
| Finaler Abgabestand | [Vorbereitete Abschlussfelder](./ABSCHLUSS-VORBEREITUNG.md) | Noch kein Freeze. Historische Prüfstände und der spätere finale Commit bleiben getrennt. |

## Belege richtig lesen

Die Originalkopien behalten ihren Inhalt und ihre ursprünglichen Kennungen. Paketmanifeste nennen Originalpfad, Ablagepfad und SHA-256. Ein Hashvergleich sichert die Identität der Datei; er belegt weder ihre fachliche Wahrheit noch die Vollständigkeit der damaligen Instrumentierung.

Maßgeblich für die aktuell berichteten Kennzahlen sind die in der Thesis bezeichneten Auswertungsfassungen einschließlich Nachreview und datierter Ergänzungen. Ältere Messprotokolle, Inspektionskopien und Manifeste dokumentieren die Entwicklung: Sie enthalten teils frühere Goldstände, später korrigierte Tokenwerte oder zurückgenommene Prüferetiketten. Das Datum eines Ordners allein macht solche Angaben nicht zur aktuellen Ergebnisfassung. Insbesondere sind KI-Gegenprüfungen keine unabhängigen menschlichen Bewertungen.

Die vollständigen kopierten Laufordner im neuen Ergänzungspaket enthalten auch ihre vorhandenen Zwischenstände und Checkpoints. Ein Checkpoint dokumentiert einen gespeicherten Workflowzustand; seine bloße Anzahl belegt weder fachliche Qualität noch erfolgreiche Wiederaufnahme. Dafür sind die jeweils ausgewerteten Ereignisse und Zustandsübergänge heranzuziehen.

## Integrität und Grenzen der Bereitstellung

[Aktualisiertes Gesamtdateiverzeichnis mit Prüfsummen](./file-manifest-20260916-r3-v1.json) und [lokale Integritätsprüfung](./verify-evidence.py) erlauben die Prüfung dieser bereitgestellten Auswahl ohne Zugriff auf die ursprünglichen Arbeitsordner. Aufruf aus dem Repository: `python3 thesis-evidence/verify-evidence.py --manifest file-manifest-20260916-r3-v1.json`. Die Prüfung verändert keine Belege. Nach späteren bewussten Paketänderungen muss das datierte Verzeichnis neu erstellt werden.

Die technische Wiederholung des Herkunftsaudits und der Tokenaggregation ist [protokolliert](./preparation-checks-20260915.json). Dabei wurden keine Modelle aufgerufen, kein produktiver Core verändert und keine neuen fachlichen Urteile erzeugt. Die Integritätsprüfung ersetzt weder eine semantische Neubewertung noch eine Ausführung des Gesamtsystems.

**Materialstatus B-50:** [Herkunft, Autorenangaben und offener Nutzungsumfang](./MATERIALSTATUS-20260916.md). Nachgespielte Gespräche auf Grundlage eines realen Hochschulprojekts; der Bewohnername ist nach der Autorenklärung fiktiv/ersetzt, die Projektbezeichnung real. Vollständige Anonymisierung und dokumentierte Nutzungsumfänge werden nicht behauptet. Der Treue-Fall meeting-2-extended ist durch [archivierte Entwicklungsunterlagen](./materialherkunft-20260916/README.md) als gezielt konstruiertes Testgespräch ausgewiesen. Die Herkunft der beiden quantitativen Quellen ist damit beschrieben; der Nachweis des tatsächlichen Nutzungsumfangs bleibt offen. Historische Prüfquellen wurden nicht verändert.

Die älteren Vorstudienordner bleiben erhalten. Ihr Einstieg erfolgt über die zugeordneten E-IDs im Kapitel-4-Paket. Der [frühere Hauptindex](./historisch/README-vor-P10-5-20260915.md) ist unverändert archiviert; seine pauschalen Aussagen zu Modellrobustheit oder struktureller Überlegenheit sind keine aktuellen Gesamtergebnisse.
