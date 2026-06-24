# Projektkontext aus Stakeholder-Transkripten

## Projektziel
Das Projekt zielt darauf ab, eine digitale Lösung in Form einer App zu entwickeln, die die Kommunikation zwischen Menschen mit Beeinträchtigungen und ihren Betreuern sowie Angehörigen verbessert und unterstützt. Die App soll insbesondere helfen, die Verständigung des Personals und Dritter mit den Bewohnern zu erleichtern, die teilweise nur nonverbal oder in spezieller Kommunikationsweise kommunizieren können. Die Lösung soll benutzerfreundlich sein, plattformübergreifend (iOS, Android) nutzbar und dynamisch erweiterbar.

## Sprecherrollen
- Gesamtleiter der Einrichtung (Projektauftraggeber)
- Heilerziehungspfleger (Pflege- und Betreuungspersonal)
- Fachpersonal mit sozialpädagogischem und technischem Hintergrund
- Studierende der sozialen Arbeit und Informatik (Entwicklerteam)
- Angehörige der Bewohner

## Fachliche Schwerpunkte und App-Funktionen
- Speicherung und Bereitstellung individueller Kommunikationsweisen der Bewohner (verbal und nonverbal)
- "About Me"-Profilseiten mit persönlichen Informationen, Bildern und Hobbys
- Kommunikationsseiten mit Texten, Bildern und Videos zur Erklärung spezifischer Verhaltensweisen
- Kalenderfunktion mit Terminen und Medikamentenverwaltung
- "No-Go"-Liste mit Verhaltensweisen, die vermieden werden müssen
- Mehrstufiges Accountsystem mit Admin-, User- und Bewohnerrollen
- Unterstützung mehrerer Einrichtungen mit differenziertem Zugriff
- Such- und Filterfunktionen zur schnellen Navigation
- Datenschutz durch zentral zugewiesene Accounts, keine Selbstregistrierung

## Technische Rahmenbedingungen
- Entwicklung mit Flutter und Dart für plattformübergreifende Nutzung
- Einsatz von Firebase Firestore als Cloud-Datenbank mit lokaler Zwischenspeicherung
- Fokus auf Android 11 als erste Zielplattform
- Nutzung von Android Studio empfohlen

## Designaspekte
- Intuitive und zugängliche Benutzeroberfläche
- Klare Navigation mit Appbar und Suchleiste
- Visuelle Elemente wie Plus-Buttons zum Hinzufügen von Inhalten
- Barrierefreiheit mit großem Kontrast und einfacher Bedienung
- Hilfe- und Tutorialfunktionen für neue Nutzer

## Konflikte und Unsicherheiten
- Datenschutz: Umsetzung und datenschutzrechtliche Absicherung sind noch offen, Registrierungsprozess strikt durch Admin gesteuert
- Zugriffsrechte: Noch Klärungsbedarf, insbesondere zum Zugriff auf Profile unterschiedlicher Einrichtungen
- Umfang der App-Funktionen: Balance zwischen Funktionalität und Benutzerfreundlichkeit muss gefunden werden
- Einbindung von Videos in Kommunikationsseiten: Integration wird noch abgestimmt

## Quellenhinweise
Diese Zusammenfassung basiert auf einem Transkript aus mehreren Interviews und Diskussionen mit den Stakeholdern der Einrichtung und dem Entwicklerteam (Interview-Einrichtung.txt).
