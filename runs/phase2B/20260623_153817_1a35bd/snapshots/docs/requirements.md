## Functional Requirements

1. Die App soll individuelle Kommunikationsweisen der Bewohner speichern und bereitstellen, inkl. verbaler und nonverbaler Kommunikation.
2. Erstellung von "About Me"-Profilseiten mit persönlichen Informationen, Bildern und Hobbys der Bewohner.
3. Kommunikationsseiten mit Texten, Bildern und Videos zur Erklärung spezifischer Verhaltensweisen der Bewohner.
4. Kalenderfunktion zur Verwaltung von Terminen und Medikamenten.
5. "No-Go"-Liste, die Verhaltensweisen enthält, die vermieden werden müssen.
6. Mehrstufiges Accountsystem mit den Rollen Admin, User und Bewohner.
7. Unterstützung mehrerer Einrichtungen mit differenziertem Zugriff auf Daten.
8. Such- und Filterfunktionen zur schnellen Navigation innerhalb der App.
9. Datenschutz durch zentral zugewiesene Accounts ohne Selbstregistrierung.

## Non-functional Requirements

1. Die App muss plattformübergreifend auf iOS und Android verfügbar sein.
2. Die Bedienung muss benutzerfreundlich, intuitiv und barrierefrei sein, z.B. mit großem Kontrast und einfacher Navigation.
3. Einsatz von Flutter und Dart als Entwicklungsframework.
4. Verwendung von Firebase Firestore als Cloud-Datenbank mit lokaler Zwischenspeicherung.
5. Unterstützung insbesondere von Android 11 als erste Zielplattform.
6. Die Benutzeroberfläche soll visuelle Hilfsmittel wie Plus-Buttons zum Hinzufügen von Inhalten bereitstellen.
7. Bereitstellung von Hilfe- und Tutorialfunktionen für neue Nutzer.

## Constraints/Compliance

1. Datenschutzrechtliche Anforderungen müssen eingehalten werden; der Registrierungsprozess erfolgt strikt durch Administratoren.
2. Es ist sicherzustellen, dass Zugriffsrechte insbesondere bei mehreren Einrichtungen differenziert und sicher geregelt sind.
3. Die Nutzung von Firebase Firestore als Cloud-Datenbank ist vorgegeben.

## Assumptions and Open Points

1. Die genaue Umsetzung des Datenschutzes und die datenschutzrechtliche Absicherung sind noch offen.
2. Die konkreten Zugriffsrechte, insbesondere beim Zugriff auf Profile verschiedener Einrichtungen, müssen noch geklärt werden.
3. Die Balance zwischen Funktionsumfang und Benutzerfreundlichkeit ist noch nicht final festgelegt.
4. Die Einbindung von Videos in den Kommunikationsseiten wird noch abgestimmt und ist nicht final entschieden.

## Traceability

Diese Anforderungen basieren auf dem Projektkontext aus dem MAF Shared State, der Zusammenfassung aus Stakeholder-Interviews und dem Transkript "Interview-Einrichtung.txt". Die funktionalen und nicht-funktionalen Anforderungen wurden aus den dort beschriebenen Zielen, technischen Rahmenbedingungen und Designaspekten abgeleitet.