# Ergänzende Core-Inhaltsprüfung: Vorbereitung

Stand: 15.09.2026. **Auswahl und Kriterien gesichert; keine neuen Inhaltsurteile und keine neuen Qualitätszahlen.**

Das [Protokoll](protokoll.md) konkretisiert den bestehenden Phase-10-Auftrag. Es ergänzt den technischen Herkunftsnachweis um die inhaltliche Prüfung ausgewählter Einordnungs- und Fortschreibungsketten. Die Auswertung ist retrospektiv und entwicklungsbekannt; sie setzt keinen Methodenwechsel voraus.

## Inventar und Auswahl

Inventarisiert: 143 Laufverzeichnisse, 136 Konfigurationen, 65 Anforderungspläne (47 mit Operationen, 18 leer), 154 vorgeschlagene Operationen. Die Hauptdaten stehen in [inventory.json](inventory.json). Keine Erfolgszählung.

Ausgewählt: 8 zusätzliche Fälle und 4 bekannte Kontrollfälle. Die [Regeln](selection-rules.json) wurden vor der Auswahl gesichert; [selected-cases.json](selected-cases.json) enthält Rangfolge und etwaige leere Fächer. Die Auswahl nutzt weder Apply-Berichte noch Inhaltsurteile.

| Fall | Eingang | Lauf | Eingangs-ID | Vorgeschlagene Art |
|---|---|---|---|---|
| N01 | transcript | `20260729_142120_1ac7c2` | `REQ-04` | `NEW` |
| N02 | transcript | `20260725_162437_790824` | `REQ-07` | `RESTATE` |
| N03 | author_delta | `20260818_183319_b3d6e6` | `AF-1` | `NEW_RELATED` |
| N04 | author_delta | `20260819_102959_b8e8f8` | `AF-1` | `REFINE` |
| N05 | github | `20260820_182614_a5f6b3` | `GH-43` | `NEW` |
| N06 | github | `20260820_165512_73b1dc` | `GH-12` | `ALREADY_DECIDED` |
| N07 | other_delta | `20260821_204808_042823` | `CA-2` | `NEW_RELATED` |
| N08 | other_delta | `20260806_115016_01715f` | `MA-REQ-X1` | `CONTRADICT` |
| K01 | transcript | `20260818_084712_765eea` | `REQ-01` | `REFINE` |
| K02 | author_delta | `20260817_123342_f23f06` | `AF-1` | `REFINE` |
| K03 | transcript | `20260804_121433_eb181a` | `REQ-05` | `CONTRADICT` |
| K04 | github | `20260820_172043_802e63` | `GH-45` | `NEW_RELATED` |

## Prüfbarkeit und nächster Schritt

Die [Originaldateien](originals-manifest.json) sichern die Konfigurationen, Pläne, bekannten Fallberichte und die bezeichnete aktuelle Kriteriengrundlage mit Originalpfad und SHA-256. Diese Sicherung enthält noch nicht alle Rohquellen, damaligen Zielbestände und Nachzustände für die spätere Inhaltsprüfung. Deren Dateiexistenz wurde erst nach Auswahl erfasst ([Präsenzliste](evidence-presence-after-selection.json)); Existenz belegt weder eine vollständige Kette noch einen Erfolg. Die Fallauswahl wird wegen fehlender Belege nicht ausgetauscht. Ein späteres Belegpaket muss die tatsächlich benötigten Quellen-/Zustandsartefakte vor Bewertung ergänzen und einzeln hashen.

Als Nächstes Quellen und historische Zustandsketten für diese Fälle sichern, lesbare Gegenüberstellungen erstellen und K1–K5 anwenden. Fachlich relevante Mehrdeutigkeiten dem Autor ohne vorgegebenes KI-Verdikt vorlegen. Das [leere Bewertungsgerüst](assessment-template.json) enthält noch keine Urteile. Die bekannte Entwicklungssituation und fehlende unabhängige Annotation bleiben ausgewiesen. Aus Kontrollen und zusätzlich ausgewählten Fällen wird keine gemeinsame Erfolgsquote gebildet.

Die Datei `scripts/verify_selection.py` prüft die gesicherten Originalhashes, rekonstruiert die Inventur aus den Kopien und berechnet die Auswahl erneut. Sie bewertet keine Inhalte. Der Erzeuger unter `scripts/build_preparation.py` dokumentiert die einmalige Vorbereitung mit lokalen Pfaden; für die portable Prüfung ist ausschließlich `verify_selection.py` vorgesehen.

Der [aktive Phase-10-Plan](../../../Thesis-Docs/Writing/claude-writing/Phase-10-Gesamtabnahme-2026-09-13.md) bleibt die einzige Aufgabenführung. Aus dieser Vorbereitung entsteht kein Overleaf-Sync, Modelllauf oder Codeeingriff.
