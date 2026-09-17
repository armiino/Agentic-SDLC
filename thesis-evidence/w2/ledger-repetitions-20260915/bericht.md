# Ledger-Wiederholungen: Ergebnis der ergänzenden Auswertung

**Ergebnis:** Die drei historischen Stressfall-Ausgaben erreichen ähnliche Gesamtwerte der vollständigen Abdeckung, bewahren aber teilweise unterschiedliche Aussagen. 54, 57 und 54 von 109 Referenzaussagen sind vollständig enthalten; nur 43 davon sind in allen drei Ausgaben vollständig erhalten. Technisch sind sämtliche 87, 92 und 79 Quellenreferenzen auflösbar. Die erneute inhaltliche Prüfung findet jedoch einzelne unvollständig gestützte Propositionen und zwei Abschwächungen des Planungsstands. Die Zahlen beruhen auf den unten offengelegten Inhaltsurteilen und charakterisieren diesen bekannten Quellenfall unter den aufgezeichneten Bedingungen; sie sind kein allgemeiner Zuverlässigkeitsnachweis.

## 1. Was wurde tatsächlich untersucht?

R = `20260905_164650_8189ea` (bisheriger Hauptlauf), A = `20260905_165151_2dde4d`, B = `20260905_165658_1271bb`. Alle entstanden am 05.09.2026. Bewertet ist jeweils der gespeicherte maschinelle Claim-Bestand vor menschlicher Adjudikation. Der gemeinsame Referenzbestand Gold v2 umfasst 109 Aussagen aus 136 Units. In den drei Läufen stimmen Units byteweise, aufgezeichnete Konfiguration ohne Laufkennung/Zeit, vollständige erste Modellanfrage und die Menge der Systemprompts überein. Modellname `openai/gpt-5.4`, protokollierte Temperatur 0. Die Zahl der Modellaufrufe beträgt 12/11/12; spätere Anfragen hängen von vorherigen Ausgaben ab. Ein vollständig rekonstruierter damaliger Code-/Umgebungsstand oder unveränderliche Providergewichte ist damit nicht belegt.

Der [Leitfaden](./leitfaden.md) wurde vor den neuen semantischen Bewertungen gespeichert und gehasht; Quellenstand, ursprüngliche Ergebnisse und Ausgabegrößen waren zuvor bekannt. Die Runde ist retrospektiv und nicht verblindet. Die gültigen Matchregeln, Gold-v2-Präzisierungen und dokumentierten Fallauslegungen wurden beibehalten; Gold, Originaloutputs und historische Urteilsdateien wurden nicht geändert.

Neu erhoben wurden **218 Coverage-Urteile** für A und B. Die 109 Coverage-Urteile von R stammen unverändert aus der Konsolidierung vom 08.09. Zusätzlich wurden zunächst alle 88 Claims von A/B auf Quellenverfälschung und Stützung geprüft. Wegen eines auch im Hauptlauf vorkommenden Referenzkontextproblems folgte ein [dokumentierter Umfangsnachtrag](./umfangsnachtrag.md): auch alle 47 Claims von R wurden nach demselben heutigen Verfahren nachgeprüft. Insgesamt sind es **135 Claim-Fälle mit je zwei getrennten Urteilen**. Das zusätzliche Lesen des ursprünglichen Bestands ist keine neue unabhängige Systembeobachtung.

Die Urteile entstanden KI-gestützt durch Codex. Ein separater Backend-Modellstand des Bewertungsdialogs wurde nicht als ausführbarer Evaluationslauf archiviert. Drei konkrete Modalitätsfälle wurden anschließend vom Autor anhand einer Quellen-/Claim-Gegenüberstellung ohne vorgegebenes KI-Verdikt beurteilt. Der Autor ist zugleich Entwickler und kein unabhängiger Zweitannotator. Die Antworten gelten für diese Fälle, nicht als menschliche Vollprüfung des übrigen Bestands.

## 2. Abdeckung: ähnliche Quote, teilweise andere Inhalte

| Lauf | Claims | full / partial / none | Vollständig abgedeckt | Vollständig oder teilweise abgedeckt |
|---|---:|---:|---:|---:|
| R – bisheriger Hauptlauf | 47 | 54 / 29 / 26 | 49,5 % | 76,1 % |
| A – erste zusätzliche Ausgabe | 47 | 57 / 32 / 20 | 52,3 % | 81,7 % |
| B – zweite zusätzliche Ausgabe | 41 | 54 / 25 / 30 | 49,5 % | 72,5 % |

Median der Full Coverage: 49,5 %; beobachtete Spanne 49,5–52,3 % (drei Referenzaussagen bzw. 2,8 Prozentpunkte). Median der Touched Coverage: 76,1 %; Spanne 72,5–81,7 %. Drei Beobachtungen erlauben keine belastbare Schätzung allgemeiner Varianz oder Ausfallwahrscheinlichkeit. Die Referenzaussagen sind keine 109 unabhängigen Wiederholungen des Systems.

**Auf Aussageebene:** 43 Referenzaussagen sind in allen drei Ausgaben full; 25 wechseln zwischen full und nicht full. Weitere sechs wechseln nur zwischen partial und none. Damit haben 78 von 109 Aussagen in allen drei Ausgaben denselben Status. Insgesamt sind 68 Aussagen in mindestens einer Ausgabe full; diese Vereinigung ist keine tatsächlich erzeugte gemeinsame Ausgabe und wird nicht als Systemabdeckung ausgegeben.

Konkrete Unterschiede, nach dem jeweiligen Claim-Text:

- **User-Rechte:** Lösch- und Account-Anlageverbote sind in A22/B17 ausdrücklich enthalten (G-IE-023/G-IE-025 jeweils full); im ursprünglichen R19 fehlen sie. Kein Rückschluss vom Admin-Recht allein auf ein ausdrückliches User-Verbot.
- **Umfang/Datenschutz:** R12 und R26 erhalten die funktionale Umfangsbegrenzung und die Auslagerung vertiefter Datenschutzkonzeption (G-IE-012/G-IE-042 full). Beide fehlen in A/B. A12 bewahrt immerhin die Klärung vollständiger Dokumentationsübernahme; B enthält diese nicht.
- **UI:** A13/A28/A31/A34 erhalten zusätzliche About-Me-, Profil- und Kalenderdetails. B25 bewahrt dagegen die drei Appbar-Positionen ausdrücklich. Gleichartige Quoten entstehen somit nicht aus exakt denselben erhaltenen Inhalten.

Der ursprüngliche direkte F-Vergleich wird nicht neu berechnet oder durch einen günstiger gewählten Ledger-Lauf ersetzt. Seine 69 vollständig erfassten Stressfall-Aussagen liegen weiterhin über den hier beobachteten Ledger-Ausgaben. Die neue Claim-Stützungsprüfung wurde nicht auch für F durchgeführt; aus ihren Zahlen wird deshalb kein aktualisierter Qualitätsvergleich mit F abgeleitet. Ein gepoolter N=3-Ledger-vs.-N=1-F-Test wäre hier nicht sinnvoll. Die Nachauswertung belegt weder allgemeine Ledger-Überlegenheit noch die Wirkung von HITL; sie erweitert die Beschreibung der maschinellen Evidenzerzeugung.

## 3. Quellenverweise, Korrektheit und Stützung getrennt

| Lauf | Technisch gültige Referenzen | Claims ohne festgestellte Quellenverfälschung | Vollständig direkt oder inferentiell gestützt | Nur teilweise gestützt |
|---|---:|---:|---:|---:|
| R | 87/87 | 47/47 = 100,0 % | 45/47 = 95,7 % | 2 |
| A | 92/92 | 46/47 = 97,9 % | 44/47 = 93,6 % | 3 |
| B | 79/79 | 40/41 = 97,6 % | 38/41 = 92,7 % | 3 |

Jeder Claim besitzt mindestens eine gültige Referenz. Keine Proposition wurde als vollständig ungestützt eingestuft. Teilweise Stützung bedeutet, dass nicht der gesamte Propositionstext aus der Vereinigung der angegebenen Units folgt. Inhaltliche Korrektheit prüft dagegen Quellenverfälschung gegenüber dem gesamten Quelltext; sie ist ausdrücklich keine Gold-Precision. Korrektheit bedeutet außerdem nicht vollständige Abdeckung aller Quelleninformationen.

**Notwendige Berichtigung des bisherigen Stützungsurteils:** Der alte Hauptbericht nennt für R eine semantische Stützung von 47/47. Die erneute Claim-Prüfung kommt auf 45/47. Es handelt sich um dieselben unveränderten Systemausgaben; geändert ist das Bewertungsurteil. R05 nennt als Zweck der Kennenlerntermine das Verständnis der Kommunikationssituation, referenziert aber nur AU-0009; der spezielle Zweck steht in AU-0008. R47 nennt Animationen, referenziert jedoch nur die anaphorische Antwort in AU-0135. Der Gegenstand steht in AU-0134. Die fehlenden Inhalte sind in der ursprünglichen Quelle vorhanden und über den Transkriptkontext auffindbar; der engere referenzierte Unit-Satz trägt sie allein nicht vollständig. Deshalb ist die Aussage „alle Claims sind durch ihre referenzierten Units semantisch gestützt“ im bisherigen Stressfall-Haupttext nachzuführen. Die vollständige technische Auflösbarkeit bleibt davon unberührt.

Weitere Fälle: A11 ergänzt die mögliche Akteneinsicht für die **Befüllung**, was AU-0020 nicht aussagt. A39/B32 haben dieselbe Anapherngrenze wie R47; ihr identisches Proposition-/Referenzpaar erhält dasselbe Urteil. B05 verweist nur auf die bevorzugte Richtung in AU-0011, während die explizite Gegenrichtung in AU-0010/12 steht. Diese Fälle sind keine Behauptung, die ursprüngliche Quelle sei verschwunden oder der Core grundsätzlich nicht nachvollziehbar.

Zwei inhaltliche Abschwächungen sind am Propositionstext sichtbar: A23 macht den vorgesehenen eigenen About-Me-Zugriff erneut zu einer Prüffrage; B27 formuliert die geplante Popup-Funktion nur als ideale Möglichkeit. Sie betreffen vorgelegte maschinelle Aussagen vor menschlicher Adjudikation. Eine tatsächliche spätere Korrektur durch System-HITL wurde hier nicht gemessen.

## 4. Drei Autorenprüfungen und verbleibende Auslegungsabhängigkeit

Die [Originalantworten](./human-review.json) bleiben wörtlich erhalten:

- **A23:** Der Autor beurteilt den Inhalt als teilweise richtig und teilweise bereits geklärt. Das stützt die Teilbewertung des Funktions-/Planungsstands.
- **B27:** Der Autor bestätigt, dass „idealerweise“ etwas Wünschenswertes, noch nicht vollständig Verlangtes bezeichnet. Die Beurteilung als Abschwächung bleibt erhalten.
- **B28:** Der Autor hält „optional“ wegen der konjunktivischen Quelle für passend. Diese Lesart wurde transparent übernommen: Die Quelle sagt „könnte helfen“ bzw. „wäre sinnvoll“. Die Ersturteile zu Korrektheit/Stützung sowie G-IE-100/G-IE-101 wurden entsprechend geändert. Eine allgemeine Regel „Konjunktiv bedeutet optional“ wird daraus nicht abgeleitet.

Diese gezielte Prüfung ist für Grenzfälle nützlich, aber keine repräsentative Zufallsstichprobe und kein unabhängiger Ratervergleich. Es wird keine Interrater-Reliabilität aus drei ausgewählten Fällen berechnet. Alle Ersturteile und Änderungen bleiben erhalten. Im Konsistenzdurchgang wurden 17 Coverage-Ersturteile nach den überlieferten Fallauslegungen geändert. Ein Stützungsurteil B04 wurde zunächst strenger gelesen und nach erneuter Abgrenzung zum spezifischeren R05 auf sein begründetes inferentielles Ersturteil zurückgeführt; beide Schritte sind protokolliert.

**Sensitivitäten, jeweils getrennt anwenden:** Bei strengerer Lesart des optionalen Hilfefalls B28 fiele B auf 52/109 full, 39/41 ohne Verfälschungsbefund und 37/41 vollständig gestützt. Wird A29s reine Namenssuche als ausreichend für G-IE-048 gelesen, erhält A einen weiteren Volltreffer (58/109). Wird für G-IE-064 eine ausdrückliche Rollenvergabe bei der Account-Anlage statt der historischen Bündellesart verlangt, muss dies symmetrisch auch R betreffen: 53/56/53 full. Bei strengerer Kontextlesart von B04 sinkt Bs Stützung von 38/41 auf 37/41. Diese Alternativen sind keine Konfidenzintervalle; die angegebenen Effekte werden nicht unbemerkt kombiniert. Maschinenlesbar in [sensitivities.json](./results/sensitivities.json).

## 5. Wissenschaftliche Aussage und nächste Textfolge

Die Ergänzung stärkt P1/A3 durch eine vollständig bezeichnete Wiederholungsreihe mit Einzelfallbelegen. Sie zeigt, dass hohe technische Referenzgültigkeit mit unvollständiger inhaltlicher Erfassung und einzelnen unvollständigen Belegbezügen vereinbar ist. Die erhaltenen Quellenwege ermöglichen es, diese Unterschiede konkret zu prüfen. Ein geringerer menschlicher Prüfaufwand, eine allgemeine Zuverlässigkeit oder ein exklusiver Vorteil gegenüber F folgt daraus nicht.

Das Ergebnis ist mit der explorativen Entwicklungslinie vereinbar: Modelle erzeugen semantische Vorschläge; Quellenbindung, Prüfungen und menschliche Entscheidungen erfüllen eigene Aufgaben. Die Wiederholungsreihe erklärt diese Aufgabenverteilung anhand beobachteter Grenzen genauer. Sie beweist weder, dass jede zusätzliche Stufe notwendig ist, noch dass ihre Qualitätswirkung schon durch die Existenz der Stufe belegt wäre. Governance, Core-Fortschreibung, Projektion und Wiederaufnahme behalten ihre gesonderten Nachweise. Als nächste inhaltliche Zusatzprüfung bleibt Herkunft/Einordnung/Fortschreibung vorgesehen.

**Textpaket noch offen:** Die historische Herkunft der Hauptwerte erhalten und die Ergänzung knapp in 7.2/7.3 mit Detailbeleg im Evaluationsanhang einordnen. Das alte pauschale Stützungsurteil des Stressfalls muss den neuen Bewertungsstand berücksichtigen; Zahlen-/Ergebnisprofil in 7.7 und betreffende Zusammenfassungen konsistent nachziehen. Die Kennzahlen weder als neuer aktueller Systemlauf noch als unabhängige menschliche Vollprüfung darstellen. Der Befund zur alten Stützungsangabe wird unter B-59 geführt. Keine Produktionscodeänderung oder Anpassung der historischen Runs aus dieser Auswertung.

## 6. Nachprüfbarkeit

39 unveränderte Originaldateien sind mit Pfad und SHA-256 bereitgestellt. [recompute.py](./recompute.py) prüft Quelltextgleichheit, ID-/Referenzintegrität, die eingefrorenen Grundlagen, Capture-Prüfsummen und protokollierte Bedingungen und berechnet alle Haupttabellen aus den gespeicherten Urteilen. [Ergebnis-JSON](./results/results.json), [Coverage-Details](./results/coverage-details.json), [Claim-Details](./results/claim-quality-details.json). Das Skript reproduziert Zahlen, nicht semantische Entscheidungen. Die [abschließende Dateiprüfung](./verification.json) dokumentiert zusätzlich die unveränderten Originalkopien, die vollständig nachvollzogenen Urteilsrevisionen und die identische Neuberechnung in einem zweiten Ausgabeverzeichnis. Literatur, Referenzbestand, Produktionscode und Universitätstemplate wurden nicht geändert; diese Runde erzeugt noch keinen TeX-Sync.
