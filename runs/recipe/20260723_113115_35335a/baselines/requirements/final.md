# Requirements

## Produktziel und Lösungsrahmen
- Die angestrebte Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern bzw. fördern; die genaue Ausgestaltung dieses Produktziels ist noch nicht vollständig konkretisiert. [canon-produktziel-kommunikation-verbessern]
- Die Unterstützung soll vorrangig darauf ausgerichtet sein, dass Betreuer oder andere Personen Bewohner besser verstehen können. [canon-priorisierung-bewohner-besser-verstehen]
- Als Kernansatz soll eine App Wissen über das Profil und die Kommunikationsweise einer Person bereitstellen, damit Nutzer bei Verständnisschwierigkeiten nachsehen können, was gemeint sein könnte; die konkrete Ausprägung dieses Ansatzes ist noch offen. [canon-app-profilwissen-kommunikationshilfe]
- Ein System, das individuell zwischen Bewohner und Betreuer in beide Richtungen übersetzt, darf nicht Teil des Lösungsansatzes sein. [canon-ausschluss-individual-uebersetzer]

## Benutzerkonten, Rollen und Zugriff
- Die App darf keine Selbstregistrierung erlauben; der Zugang erfolgt ausschließlich per Login mit intern vergebenen Accounts. [canon-login-ohne-selbstregistrierung]
- Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen. [canon-rollen-admin-user]
- Zusätzlich soll es einen Bewohner-Account geben. Dieser darf nur das eigene Profil in der Profilübersicht sehen und nur eingeschränkte Funktionen nutzen, insbesondere Zugriff auf den About-Me-Bereich und gegebenenfalls das Hinzufügen eigener Bilder. [canon-bewohner-account-mit-beschraenkung]
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten und Inhalte beziehungsweise Wissen beitragen können; unterschiedliche Rechte sind dabei vorgesehen, die konkrete Ausgestaltung bleibt jedoch zu klären. [canon-angehoerige-zugriff-und-beitrag, canon-rechteverwaltung-noch-offen]
- Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf die Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten. [canon-zugriff-nach-einrichtung-beschraenkt]
- Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden. [canon-rechteverwaltung-noch-offen]

## Login und Navigation
- Der Login-Screen soll ein zentriertes, gut sichtbares Logo im oberen Drittel enthalten; ein passendes Kommunikations-Logo ist noch zu erstellen. [ADJ-GAP-AU-0117]
- Der Login-Screen soll E-Mail-Feld, Passwort-Feld und einen Login-Button enthalten; eine Registrierungsmöglichkeit darf dort nicht angeboten werden. [canon-login-ohne-selbstregistrierung]
- Nach dem Login soll eine Profilübersicht angezeigt werden. [canon-profiluebersicht-nach-login]
- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. [canon-appbar-konsistent]
- Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und Logout unterstützen; genannt sind ein nach links zeigender Pfeil links, der Name der aktuellen Seite in der Mitte und ein Einstellungssymbol rechts. [canon-appbar-funktionen]

## Profilübersicht und Profildetail
- Auf der Profilübersicht soll eine anklickbare Liste der sichtbaren Bewohnerprofile angezeigt werden; jedes Profil führt zu einer weiteren Seite mit den Daten des gewählten Profils. [canon-profiluebersicht-nach-login]
- Auf der Profilübersicht muss eine Suchleiste vorhanden sein, um Profile schnell nach Namen zu finden. [canon-suche-profile]
- Profile sollen in der Übersicht mit kleinem Vorschaubild, Name und kurzer Beschreibung dargestellt werden; ob dies als Liste oder als Kacheln erfolgt, muss noch geklärt werden. [canon-profiluebersicht-darstellung]
- Die Detailansicht eines Profils soll das gewählte Profilbild größer anzeigen und die Hauptbereiche der App als interaktive Buttons anbieten; der genaue Zuschnitt beziehungsweise die genaue Zahl der Hauptbereiche ist noch nicht stabil. [canon-profil-detail-mit-hauptbereichen]

## About Me
- Die App muss für jeden Bewohner eine About-Me-Seite bereitstellen, die für den ersten Eindruck der Person dient und persönliche Kurzinfos enthält, einschließlich Bildern sowie beschreibender Informationen wie Name, Alter und Hobbys. [canon-about-me-seite]
- Die About-Me-Ansicht muss eine Foto-Timeline mit Beschreibungen bereitstellen. [canon-about-me-seite, canon-about-me-fototimeline-erweiterbar]
- Die Foto-Timeline muss dynamisch erweiterbar sein; neue Einträge müssen über einen Plus-Button hinzugefügt werden können. [canon-about-me-seite, canon-about-me-fototimeline-erweiterbar]
- Neu hinzugefügte Bilder müssen in der About-Me-Timeline automatisch ganz oben als neueste Einträge angezeigt werden. [canon-about-me-seite, canon-about-me-fototimeline-erweiterbar]

## Kommunikation
- Die App muss eine Kommunikationsansicht bereitstellen, die klar in verbale und nonverbale Kommunikation unterteilt ist. [canon-kommunikationsansicht-verbal-nonverbal]
- Die Trennung zwischen verbaler und nonverbaler Kommunikation soll visuell klar erkennbar sein, unter anderem durch Kennzeichnung mit Symbolen. [canon-kommunikationsansicht-verbal-nonverbal]
- Einträge zu Kommunikationsweisen müssen dynamisch erweiterbar sein; neue Erfahrungen müssen hinzugefügt werden können. [canon-kommunikationseintraege-dynamisch-multimodal]
- Kommunikationsweisen müssen nicht nur als Text, sondern auch mit Bildern und weiteren Darstellungsformen erfasst und angezeigt werden können. [canon-kommunikationseintraege-dynamisch-multimodal]
- Auf den Kommunikationsseiten soll eine Suchfunktion vorgesehen werden; dafür muss ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit später besser gesucht und gefiltert werden kann. [canon-suche-kommunikation-und-beschreibungsmuster]
- Videos von Kommunikationssituationen müssen mit Beschreibungen erfasst werden können. [canon-videos-in-kommunikation]
- Die Videofunktionalität muss in die Kommunikationsseiten integriert sein und darf nicht als separater Screen umgesetzt werden. [canon-videos-in-kommunikation]
- Neue Videos sollen auf den Kommunikationsseiten per Plus-Button hinzugefügt werden können; neueste Videos sollen oben angezeigt werden. [canon-videos-in-kommunikation]

## No-Go-Bereich
- Die App muss eine No-Go-Seite bereitstellen, auf der kritische Dinge festgehalten werden, die in Gegenwart des Bewohners vermieden werden müssen. [canon-no-go-seite]
- Die No-Go-Seite muss eine dynamisch erweiterbare Liste bieten, in die neue No-Gos per Plus-Button hinzugefügt werden können. [canon-no-go-seite]
- Die No-Go-Einträge sollen einfach zu durchforsten sein und nur die wichtigsten Informationen enthalten. [canon-no-go-seite]

## Datenschutz, Compliance und offene technische Klärungen
- Vor der Nutzung von Bildern in der App müssen Datenschutzfragen und Einwilligungen der Angehörigen beziehungsweise Berechtigten geklärt werden; dies gilt auch für Testbilder. [canon-datenschutz-bilder-einwilligungen]
- Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]

## Plattform und Qualitätsanforderungen
- Die App muss plattformübergreifend auf iPhone und Android laufen. [canon-plattform-ios-android]
- Bei der Gestaltung soll Barrierefreiheit berücksichtigt werden, insbesondere große Schrift, ausreichender Kontrast und zurückhaltender Farbeinsatz. [canon-barrierefreiheit-beruecksichtigen]

## Optionale bzw. später mögliche Anforderungen
- Es soll evaluiert werden, ob die App zusätzlich auf Tablets nutzbar sein kann. [canon-tablet-support-pruefen]
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist als spätere mögliche Erweiterung vorgesehen. [canon-benachrichtigungen-about-me-updates]
- Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können. [canon-admin-screen-pruefen]
- Eine Kalenderfunktion soll erwogen werden. Zusätzlich ist zu prüfen, ob auch Medikamentengaben integriert werden sollen; die Vertraulichkeit dieser Daten ist dabei besonders zu berücksichtigen. [canon-kalender-und-medikamente-pruefen]
- Alternative Eingabemethoden wie Sprachbefehle sollen als mögliche spätere Accessibility-Erweiterung berücksichtigt werden. [canon-alternative-eingabemethoden-pruefen]
- Eine Ausweitung der App auf weitere Dokumentationsfunktionen soll geprüft werden, jedoch nur in begrenztem Umfang und ohne den Fokus auf unterstützende Kommunikation zu verlieren. [canon-erweiterung-dokumentation-pruefen]
- In der Profilübersicht soll erwogen werden, das Anlegen neuer Profile per Plus-Symbol und Dialog für Bild, Name und Beschreibung zu ermöglichen. [canon-neue-profile-anlegen-pruefen]
- Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour beim ersten Login und als später erneut aufrufbarer Hilfebereich. [canon-hilfe-tutorial-pruefen]
- Animationen sollen nicht im Fokus stehen; falls sie verwendet werden, dürfen sie nicht ablenkend sein. [canon-animationen-nicht-im-fokus]

## Kontext und Rahmenbedingungen
- Zu den Bewohnern existieren bereits klassische Akten und Dokumentationen, in denen Informationen, Erfahrungen und neues Wissen nach einem bestimmten Plan schriftlich festgehalten werden. [canon-ist-situation-dokumentation]
- Es muss berücksichtigt werden, dass vorhandene Akten im Arbeitsalltag schwer nutzbar sein können, weil sie umfangreich sind und gesuchte Informationen nicht schnell gefunden werden; dies wurde jedoch nur als persönliche Einschätzung geäußert. [canon-risiko-akten-schwer-nutzbar]
- Die genauen Bedürfnisse und Funktionen des Systems müssen durch Anforderungsanalyse mit mehreren Beteiligten und künftigen Nutzern erarbeitet werden. [canon-anforderungsanalyse-mit-nutzern, ADJ-GAP-AU-0002]
- Zur Nutzerforschung müssen Vor-Ort-Termine in Einrichtungen durchgeführt werden, um reale Kommunikationssituationen und Bedürfnisse besser zu verstehen. [canon-beobachtung-vor-ort]
- Kommunikation ist im Alltag zentral und das Projekt wird als Weiterentwicklungsmöglichkeit gesehen. [ADJ-GAP-AU-0003]
- Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen. [canon-technologiestack-flutter-dart]
- Das Paket GetX soll als technische Option berücksichtigt und in der Umsetzung erprobt werden. [canon-getx-pruefen]
- Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt. [canon-firebase-firestore-vorlaeufig]