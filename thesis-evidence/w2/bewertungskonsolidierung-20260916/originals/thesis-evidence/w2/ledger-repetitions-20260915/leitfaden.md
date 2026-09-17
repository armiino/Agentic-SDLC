# Ergänzende Ledger-Nachauswertung: Leitfaden v1, 15.09.2026

Vor der semantischen Bewertung der beiden zusätzlichen Ausgaben festgehalten. Die Systemausgaben entstanden am 05.09.; ihre Größe und die bisherigen Hauptbefunde sind bekannt. Dies ist eine retrospektive, nicht verblindete KI-gestützte Auswertung durch Codex, keine unabhängige menschliche Annotation, keine neue Systemausführung und keine prospektive Registrierung des damaligen Experiments. Eine spätere Autorenprüfung wird gesondert protokolliert, nur nach tatsächlicher Antwort.

## 1. Frage, Auswahl und Grenze

Wie unterscheiden sich Abdeckung und Quellentreue in den drei erhaltenen Ledger-Ausgaben desselben Stressfalls unter den vergleichbaren protokollierten Bedingungen? Bestehender Hauptlauf: `20260905_164650_8189ea`. Neu zu bewerten, vollständig und in dieser Reihenfolge: `20260905_165151_2dde4d`, danach `20260905_165658_1271bb`. Beide Ausgaben stammen vom Messpunkt `capture/lcr-machine.json` vor menschlicher Adjudikation. Kein Erfolgsfilter, keine Ersetzung und keine Zusammenführung der drei Ausgaben zu einem fiktiven Systemergebnis.

Gold v2 mit allen 109 Aussagen bleibt unverändert. Je neuer Ausgabe werden 109 Coverage-Urteile und alle 47 bzw. 41 Claim-Urteile erhoben. Der ursprüngliche Hauptstand wird aus der Konsolidierung vom 08.09. übernommen und ausdrücklich als früher bewertet gekennzeichnet. Wiederholungsstreuung und Bewertungsabhängigkeit sind nicht vollständig voneinander getrennt. Abweichungen zu vorhandenen Einzelurteilen werden offengelegt und nicht still in die alte Auswertung geschrieben. Drei Läufe liefern eine deskriptive Beobachtung, keine allgemeine Stabilitätsgarantie; keine Signifikanztests gegen F mit N=1.

## 2. Verbindliche Bewertungsgrundlage

Bestehende `w2-matchregeln.md`, Gold-v2-Änderungslog, tatsächliches Korrektheitsverfahren (§6b des Messprotokolls), dokumentierte Auslegungen aus D.8 und dem Entscheidungsbogen vom 08.09. Ihre Originale bleiben unverändert und werden gehasht. Historische Leitfaden-Zusagen zu menschlicher Annotation beschreiben nicht automatisch das tatsächlich durchgeführte Verfahren.

## 3. Coverage auf Referenzaussagenebene

- `full`: Gegenstand, Kerninhalt und alle wesentlichen Einschränkungen der Referenzaussage sind im Claim-Text enthalten; sinngleiche Formulierung genügt.
- `partial`: Kern getroffen, mindestens eine wesentliche Einschränkung fehlt oder ist verändert. Fehlender/falscher Zahlwert höchstens partial.
- `none`: kein entsprechender Kerninhalt; insbesondere fehlende oder falsche Negation des geforderten Verbots. Das Fehlen eines Verbots ist dabei keine Aussage, die gegenteilige Erlaubnis sei implementiert.
- Widerspruchsfreie Vereinigung mehrerer Claims darf eine Referenzaussage vollständig tragen. Die belegenden Claim-IDs sind sämtlich anzugeben. Vor none/partial wird der gesamte Claim-Bestand durchsucht und inhaltlich berücksichtigt, nicht allein Claims mit passenden AU-IDs.
- Gespeicherte Belegzitate, Facetten, Status und Hinweise zählen nicht als zusätzlicher Claim-Text. Ein Quellenverweis eröffnet Nachlesen, ersetzt aber keine fehlende Aussage.
- Bekannte Auslegungen: Admin-Anlage plus fehlende Selbstregistrierung ersetzt kein Verbot der Account-Anlage durch normale User. Rollenabhängige Funktion ersetzt nicht ausschließliche Sichtbarkeit. Die erhaltene offene Alternative „Liste oder Kacheln“ genügt ohne das Pflichtwort „offen“. Die Nichtaufnahme von Animationen/Feedback und die bedingte Ablenkungsgrenze sind unterschiedliche Referenzgegenstände. Eine Grenze nur für Animationen trägt die Feedback-Klausel nicht vollständig. Funktion/Bereich ohne Button-Darstellung trägt die entsprechende Darstellungsanforderung nur teilweise.
- Jede Referenz erhält Urteil, Claim-IDs, kurze konkrete Begründung und bei partial/none den fehlenden Bestandteil. Grenzfälle erhalten eine alternative plausible Lesart; diese wird nicht zum stillen Wechsel des Hauptmaßstabs.

## 4. Zwei getrennte Claim-Prüfungen

1. **Inhaltliche Korrektheit:** Aussage gegen die gesamte nummerierte Rohquelle. `no_distortion_found` oder `distortion_found`; entscheidungsrelevante Fälle mit Begründung. Dies ist die bestehende operationalisierte Verfälschungsfreiheit, keine Gold-Precision. Bloß plausible, unbelegte Ergänzungen werden nicht allein deshalb als Quellenwiderspruch gewertet; sie scheitern gegebenenfalls an der Stützungsprüfung.
2. **Semantische Stützung:** Nur die Vereinigung der tatsächlich referenzierten Units trägt das Urteil. `directly_supported`, `inferentially_supported` (ohne zusätzliche domänenspezifische Annahmen), `partially_supported` oder `unsupported`. Eine fehlende wesentliche Klausel verhindert volle Stützung. Kontext außerhalb der referenzierten Units darf das Urteil nicht heimlich aufwerten.

Prüfgegenstand ist jeweils die Proposition, keine Facetten- oder Zitatqualitätsprüfung. Zu jedem Claim werden Quell-IDs, Urteil und konkrete Begründung festgehalten. Exakt identische Paare aus Proposition und normalisierter AU-Menge bei identischer Quelle erhalten innerhalb dieser Nachauswertung dasselbe Urteil; Wiederverwendung ist kenntlich. Gemeldete technische Referenzgültigkeit und -präsenz werden deterministisch geprüft. Unentscheidbare Fälle werden sichtbar und mit möglicher Zahlenwirkung ausgewiesen, nicht als Erfolg gezählt.

## 5. Berechnung, Kontrolle und Bericht

Full Coverage = full/109; Touched = (full+partial)/109. Korrektheit = Claims ohne festgestellte Quellenverfälschung / alle Claims. Stützung = (direct+inferential) / Claims mit mindestens einer auflösbaren Referenz. Partial/unsupported gesondert zählen. Keine neue Gold-Precision, Hinweisgüte, Tokenbilanz, Facettenwirkung oder Post-HITL-Wirkung aus dieser Untersuchung ableiten.

Alle Eingaben werden mit Originalpfad und SHA-256 gesichert. Jede ID kommt genau einmal vor; Claim-Zitate müssen existieren; Referenzauflösung, Zähler, Nenner und Tabellenwerte werden maschinell validiert. Nach dem ersten vollständigen Urteilssatz erfolgt ein eigener Durchgang innerhalb derselben Bewertung: Begründungs-/Quellenabgleich einschließlich der Volltreffer, Konsistenz identischer Inhalte und Grenzfälle. Das ist keine unabhängige Zweitannotation. Änderungen gegenüber den Ersturteilen bleiben datiert erhalten.

Ergebnis je Lauf sowie Median und Spannweite der drei Läufe. Zusätzlich gemeinsame bzw. wechselnde Abdeckung derselben Referenzaussagen beschreiben; Referenzaussagen sind keine unabhängigen Modellwiederholungen. Kein Mittel über alle Claims als Ersatz für die drei laufbezogenen Kennzahlen. Vorherige Sensitivitätsfälle und neue Grenzfälle getrennt berichten. Ausgewählte bedeutende Grenzfälle dem Autor ohne vorgegebenes endgültiges Urteil zur tatsächlichen Gegenprüfung vorlegen; bis dahin nur KI-gestützte Auswertung behaupten.
