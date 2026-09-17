# Vorbereitung des letzten Artefaktstands

Stand: 16.09.2026, nach Gesamtlektüre und gezielter Abnahme der Evaluationsergänzungen. Diese Liste konkretisiert P10-5/P10-6 des bestehenden Abschlussplans. Sie setzt keinen neuen Termin und keinen Freeze.

## Bereits vorbereitet

- Zentrale Belegzugänge, unveränderte Originalkopien und Prüfsummen: [Index](./README.md).
- Historische Integration vom 18.08., spätere Facettenintegration vom 09.09. und technische Regression vom 11.09. sind getrennt erreichbar. Die späteren Nachauswertungen vom 13./15.09. verändern diese Ausführungsstände nicht.
- Der technische Übernahmebericht enthält das originale TRX (701/701), Smoke-Protokoll (14/0) und Standprüfsummen. Diese Zahlen bleiben datiert; sie sind noch keine Bestätigung eines später veränderten Abgabestands.
- Die aktuelle PDF ist im Arbeitsplan identifiziert. Die zusammenhängende Gesamtlektüre ist erfolgt; gezielte Nachabnahmen der Ergänzungen und der letzte Export bleiben Teil des Abschlusses.

## Vor dem letzten Commit erledigen

1. **B-50 abschließen:** Herkunft und die beiden verbliebenen Bezeichnungen sind im [Materialprotokoll](./MATERIALSTATUS-20260916.md) geklärt. Die Interviewdarstellung ist im PDF bestätigt; die belegte Herkunft des konstruierten Treue-Falls ist jetzt zusätzlich in 7.2/D.1 ergänzt. Offen bleibt der tatsächliche dokumentierte Nutzungsumfang; eine konkrete Nachfrage ist noch unbeantwortet. Keine Freigabe oder vollständige Anonymisierung behaupten.
2. Gesamtlektüre P10-7 ist erledigt; die späteren Evaluationsergänzungen sind gezielt abgenommen. Der begrenzte Abschlussabgleich zu Methodik und Ergebnisbilanz benötigt noch Sync/PDF-Prüfung. Allgemeine Universitätsformatierung erhalten.
3. Vor der Abgabe die tatsächlichen KI-Beiträge beim Schreiben und Auswerten entsprechend den Hochschulvorgaben erklären. Repository-Bedienungsdokumentation und vereinbarte Bereinigung durchführen. Benötigte Belege erhalten; nur abgestimmte Dateien entfernen.
4. Tatsächlichen Code-/Konfigurations-/Paketstand gegen die geprüfte Basis abgleichen. Nur wenn sich Verhalten oder Testgrundlage geändert haben, erforderliche technische Nachprüfungen festlegen. Ein neuer Modell-E2E-Lauf folgt nicht automatisch aus Dokumentationsänderungen.
5. Bereitgestellte Dateien auf Vollständigkeit, Prüfsummen und Aufnahme in den abzugebenden Repository-Bestand prüfen. Unversionierte Dateien sind durch ihre lokale Existenz noch nicht Teil eines späteren Commits. Das Gesamtdateiverzeichnis bei bewussten Paketänderungen datiert erneuern.

## Beim endgültigen Artefaktabschluss eintragen

| Thesis-Stelle | Einzutragende Tatsache | Prüfbasis |
|---|---|---|
| 6.1, `chap6/chap6.1/chap6.1.tex` | Tatsächlicher finaler Artefakt-Commit oder Tag und Datum; relevante Framework-Versionen gegen diesen Stand prüfen. | Finaler Repository-Stand, Projekt-/Paketdateien. Der aktuelle HEAD einer noch geänderten Arbeitskopie genügt nicht. |
| 6.9, `chap6/chap6.9/chap6.9.tex` | Zugeordneter Testumfang und Ergebnis mit Ausführungsdatum; gegebenenfalls Nachprüfungen nach Codeänderungen. | Originales TRX/Smoke-Protokoll und Vergleich des tatsächlich getesteten Bestands. Keine undatierte Übernahme der 701/701. |
| D.9, `anhang/anhang-evaluation.tex` | Finaler Artefaktbezug, getrennte Daten-/Auswertungsstände und Belegzugang. | Endgültiger Artefaktbezug plus zentrale Manifeste. Historische Erzeugungscommits nur nennen, soweit nachgewiesen. |

Den finalen Artefakt-Commit erst nach diesen Arbeiten bilden und dessen tatsächliche Kennung in Overleaf eintragen. Die Kennung bezeichnet das Artefakt; die danach erzeugte Thesis-PDF ist separat durch Datum und Prüfsumme zu identifizieren. Ein Repository kann seine eigene künftige Commit-Kennung nicht vorab in einer darin enthaltenen Datei festschreiben. Danach genau die drei betroffenen TeX-Dateien synchronisieren und die Endangaben im PDF nachprüfen.

## Weiter offene Grenzen

B-50, B-05 und die bekannten Grafik-/Satzpunkte bleiben offen. Der fehlende historische Forward-Lauf E48-02 bleibt ein ausdrücklich benannter Ersatzbeleg. Die Bereitstellung schließt keine semantische Nachweislücke, erzeugt keine unabhängige Annotation und erhöht keine Erfolgsquote.
