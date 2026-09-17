# Aktueller Bewertungsnachtrag – 16.09.2026

Diese Konsolidierung folgt auf die KI-Gegenprüfung und die Rückmeldungen des Autors. Sie korrigiert zwei zu großzügige Claim-Urteile, präzisiert den menschlichen Prüfstatus und ändert keine Systemausgabe, Quelle oder Goldaussage. Für die hier genannten Kennzahlen ist dieser Nachtrag neuer als die Auswertungen vom 15.09. und der erste Gegenprüfungsbericht vom 16.09. Deren Dateien bleiben als historische Stände unverändert.

## 1. Zwei symmetrisch korrigierte Aussagen

Die Interviewquelle enthält Präferenzen für Android Studio, lässt die individuelle IDE-Wahl ausdrücklich frei und nennt einen Wechsel nur für den Bedarfsfall. Sie belegt damit die angestrebte gemeinsame Orientierung, aber keine abgeschlossene gemeinsame Vereinbarung. Bereits die bisherigen Bewertungen behandelten Veränderungen des Planungsstands als relevant. Diese Regel ist auch auf eine Verstärkung anzuwenden.

| Fall | Behauptung | Bisher | Aktuell |
|---|---|---|---|
| Ledger A41, `2dde4d` | Android Studio wurde als gemeinsame Umgebung „festgelegt“ | Keine Verfälschung; inferenziell vollständig gestützt | Verfälschung des Einigungsstands; teilweise gestützt |
| F C-086, `6540e2` | Android Studio wurde als bevorzugte Umgebung „vereinbart“ | Vollständig korrekt und gestützt | Nicht vollständig korrekt; teilweise gestützt |

„Möglichst“ im ersten Satzteil von A41 hebt die selbständige Festlegungsbehauptung im zweiten nicht auf. „Bevorzugt“ in C-086 begrenzt die Verbindlichkeit der Wahl, behauptet zusammen mit „vereinbart“ aber weiterhin eine Einigung. Die Bewertung von F wird deshalb nach demselben Maßstab berichtigt. Diese Urteile sind nachträgliche KI-gestützte Konsolidierungen, keine neu erhobenen menschlichen Einzelentscheidungen. Die großzügigere Autorenantwort zu A01 bleibt unverändert dokumentiert.

Demgegenüber formulieren R37 und B36 eine angestrebte gemeinsame Nutzung, ohne zu behaupten, dass eine Einigung bereits erfolgt sei. Bei Flutter/Dart und Firestore enthält die Quelle ausdrückliche Bestätigungen. Die vorhandenen Fehlerbefunde zur erneut geöffneten Bewohnerberechtigung (A23) und zur abgeschwächten Popup-Vorgabe (B27) bleiben bestehen. `vergleichsfaelle.json` nennt die geprüften Gegenfälle und die Gründe. Das ist ein gezielter Konsistenzabgleich, keine neue Vollannotation aller Planungsformulierungen.

## 2. Auswirkung auf die Kennzahlen

| Ausgabe | Korrektheit bisher → aktuell | Vollständige Quellenstützung bisher → aktuell | Abdeckung |
|---|---:|---:|---|
| Ledger R | 47/47 → 47/47 | 45/47 → 45/47 | 54 full, 29 partial, 26 none |
| Ledger A | 46/47 → **45/47** | 44/47 → **43/47** | 57 full, 32 partial, 20 none |
| Ledger B | 40/41 → 40/41 | 38/41 → 38/41 | 54 full, 25 partial, 30 none |
| F, Stressfall | 116/117 → **115/117** | 116/117 → **115/117** | 69 full, 23 partial, 17 none |

115/117 entspricht gerundet 0,983. Für die drei Ledger-Ausgaben beträgt der Median der Korrektheit jetzt 40/41 (0,976), bei einer Spanne von 45/47 (0,957) bis 1,0. Der Median der vollständigen Quellenstützung ist 38/41 (0,927), die Spanne 43/47 (0,915) bis 45/47 (0,957). Die Thesis verwendet die laufbezogenen Zähler und Nenner. Keine neue Modellwiederholung, Signifikanz oder Verbesserung des Artefakts folgt daraus.

Die geänderten IDE-Aussagen besitzen kein eigenes Gegenstück im Goldbestand. Die bisherigen Coverage-Zähler werden durch diese zwei Claim-Korrekturen nicht verändert. Auch die historische Stufenanalyse und technische Referenzzählungen bleiben unverändert.

## 3. Was der Autor tatsächlich erklärt hat

Auf die Frage nach Umfang und Vorgehen erklärte der Autor:

> ich habe alles immer gecheckt. ud angeschaut und nie einfach akzeptiert.

Auf die konkrete Unterscheidung zwischen einem Originalquellen-/Ausgabe-/Kriterienabgleich je Urteil und der Durchsicht bereitgestellter Unterlagen antwortete er:

> ich habe mir das was du mir zu verfügung gestellt hast html komplett angeschaut

Zusammen mit seiner zuvor erklärten Zustimmung wird dies als **vollständige Durchsicht und Bestätigung der bereitgestellten HTML-Prüfunterlagen laut Autor** dokumentiert. Die KI-Ersturteile waren sichtbar. Der Bestand umfasste 1859 Karten und 2503 Felder; diese Zahlen beschreiben das Material, nicht gezählte neue menschliche Einzelantworten. Ein ausgefüllter vollständiger Antwortexport wurde nicht vorgelegt. Deshalb werden weder 2503/2503 Zustimmungen noch eine vollständige kriteriumsweise Prüfung aller Originalbelege oder eine unabhängige Annotation behauptet.

Die beiden zuvor einzeln beantworteten Grenzfälle sind weiterhin als solche dokumentiert. Eine Zustimmung zum gesamten Bericht oder eine vollständig erklärte Durchsicht wird nicht in tausende künstliche Einzelantworten umgewandelt. Die zwei neuen Maßstabskorrekturen erfolgten nach dieser Durchsicht und werden nicht rückwirkend zu von ihr erfassten Autorenurteilen erklärt. Die Protokollierung bewertet nicht die Ehrlichkeit des Autors, sondern begrenzt die Aussage auf das tatsächlich angegebene Vorgehen.

## 4. Weitere Textpräzisierungen

- **F3:** Der Eingang stellt die globale oder bewohnerbezogene Geltung von No-Gos zur Klärung. Erst die spätere menschliche Entscheidung legt globale Geltung fest. Der Abschnitt 7.5 wird entsprechend präzisiert.
- **C-020 im F-Meeting:** Der alte `gold_escape`-Eintrag der Prüfkarte ist überholt. Der aktuelle Gold-v2-Abgleich ordnet C-020 zusammen mit C-019 G-M2-021 zu. Die Thesiszahl von drei verbleibenden Gold-Escapes war bereits richtig. Das ursprüngliche Prüfungspaket bleibt unverändert; dieser Vermerk korrigiert seine Lesart.

## 5. Belegzugang und Nachrechnung

- `leitfaden.md`: explizite Bewertungsunterscheidungen dieser retrospektiven Konsolidierung.
- `originals/` und `input-manifest.json`: unveränderte Eingänge samt Prüfsummen.
- `revisionen.json`: zwei Claim-Fälle, vier geänderte Bewertungsfelder mit Alt-/Neuwerten und Begründung.
- `vergleichsfaelle.json`: unveränderte Gegenfälle und Kontextabgrenzungen.
- `autorenprotokoll.json`: echte Antworten und Reichweite der Autorenangabe.
- `claim-quality-current.json`: 135 Ledger-Claim-Fälle im neuen Stand.
- `f-claim-revisions.json`: F-Korrektur auf dem historischen Sammelbefund. Es werden keine fehlenden alten Einzelbegründungen erfunden.
- `results.json`: aktuelle laufbezogene Zählungen und deskriptive Aggregate.
- `nachrechnen.py`: schreibfreie Nachrechnung aus den gesicherten Alturteilen und dem Korrekturprotokoll. Prüft außerdem, dass andere Ledger-Urteile unverändert sind.

Aufruf: `python3 nachrechnen.py` aus diesem Paket. Die Prüfung reproduziert Identitäten, Korrekturanwendung und Zähler. Sie entscheidet nicht, ob ein semantisches Urteil richtig ist, und bestätigt keine menschliche Tätigkeit.

Die älteren Dateien mit `fully_author_rated_cases: 0` oder `all_fields_human_verified: false` dokumentieren den jeweiligen früheren Prüfumfang. Sie werden nicht umgeschrieben. Auch der frühere Gegenprüfungsbericht mit „keine Kennzahländerung“ bleibt als historischer Stand erhalten; **diese Aussage ist durch die vorliegende Konsolidierung überholt**.
