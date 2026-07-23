# Architecture

## Produktfokus und Lösungsraum

- Die Lösung ist als digitale App zur Verbesserung bzw. Förderung der Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen angestrebt; die konkrete Ausgestaltung ist noch nicht vollständig konkretisiert. [canon-produktziel-kommunikation-verbessern]

- Die Unterstützung soll vorrangig darauf ausgerichtet sein, dass Betreuer oder andere Personen Bewohner besser verstehen können. [canon-priorisierung-bewohner-besser-verstehen]

- Als Kernansatz soll die App Wissen über Profil und Kommunikationsweise einer Person bereitstellen, damit Nutzer bei Verständnisschwierigkeiten nachsehen können; die konkrete Ausprägung dieses Ansatzes ist noch weiter auszuarbeiten. [canon-app-profilwissen-kommunikationshilfe]

- Ein System, das individuell zwischen Bewohner und Betreuer in beide Richtungen übersetzt, ist als Lösungsansatz ausgeschlossen. [canon-ausschluss-individual-uebersetzer]

- Bestehende klassische Akten und Dokumentationen zu Bewohnern sind als Bestandssituation zu berücksichtigen. [canon-ist-situation-dokumentation]

- Es muss berücksichtigt werden, dass vorhandene Akten im Arbeitsalltag schwer nutzbar sein können, weil sie umfangreich sind und gesuchte Informationen nicht schnell gefunden werden; dies ist jedoch nur als persönliche Einschätzung belegt. [canon-risiko-akten-schwer-nutzbar]

## Rollen, Zugriff und Sicherheit

- Die App darf keine Selbstregistrierung erlauben; Zugang erfolgt nur per Login mit intern vergebenen Accounts. [canon-login-ohne-selbstregistrierung]

- Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen. [canon-rollen-admin-user]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten und Inhalte beziehungsweise Wissen beitragen können; unterschiedliche Rechte sind dabei vorgesehen, aber noch nicht konkret ausformuliert. [canon-angehoerige-zugriff-und-beitrag]

- Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil sehen darf und nur eingeschränkte Funktionen nutzen kann, insbesondere Zugriff auf About Me und gegebenenfalls das Hinzufügen eigener Bilder. [canon-bewohner-account-mit-beschraenkung]

- Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten. [canon-zugriff-nach-einrichtung-beschraenkt]

- Die konkrete Ausgestaltung der Rechteverwaltung ist noch nicht festgelegt und muss später entschieden werden. [canon-rechteverwaltung-noch-offen]

- Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können. [canon-admin-screen-pruefen]

- Vor Nutzung von Bildern in der App müssen Datenschutzfragen und Einwilligungen der Angehörigen beziehungsweise Berechtigten geklärt werden, auch für Testbilder. [canon-datenschutz-bilder-einwilligungen]

## Informationsarchitektur und Kernfunktionen

- Nach dem Login soll eine Profilübersicht mit anklickbarer Liste der sichtbaren Bewohnerprofile angezeigt werden. [canon-profiluebersicht-nach-login]

- Auf der Profilübersicht soll eine Suchleiste vorhanden sein, um Profile schnell nach Namen zu finden. [canon-suche-profile]

- Profile sollen in der Übersicht mit Vorschaubild, Name und Kurzbeschreibung dargestellt werden; ob dies als Liste oder Kacheln erfolgt, ist noch offen. [canon-profiluebersicht-darstellung]

- Die Detailansicht eines Profils soll das Profilbild größer zeigen und die Hauptbereiche der App als interaktive Buttons anbieten; der genaue Zuschnitt dieser Hauptbereiche ist noch nicht stabil. [canon-profil-detail-mit-hauptbereichen]

- Die App soll je Bewohner eine About-Me-Seite mit persönlicher Kurzinfo, Bildern und beschreibenden Informationen für den ersten Eindruck bereitstellen. [canon-about-me-seite]

- Die About-Me-Ansicht soll eine dynamisch erweiterbare Foto-Timeline mit Beschreibungen bereitstellen, bei der neue Einträge per Plus-Button hinzugefügt werden und die neuesten oben erscheinen. [canon-about-me-fototimeline-erweiterbar]

- Die App soll eine Kommunikationsansicht mit klarer Unterteilung in verbale und nonverbale Kommunikation bereitstellen. [canon-kommunikationsansicht-verbal-nonverbal]

- Einträge zu Kommunikationsweisen müssen dynamisch erweiterbar sein und sollen nicht nur als Text, sondern auch mit Bildern und weiteren Darstellungsformen erfasst und angezeigt werden können. [canon-kommunikationseintraege-dynamisch-multimodal]

- Videos von Kommunikationssituationen sollen mit Beschreibungen erfasst werden können und als Teil der Kommunikationsseiten statt als separater Screen integriert sein. [canon-videos-in-kommunikation]

- Auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisiertes Beschreibungsmuster für Kommunikationseinträge definiert werden, damit Suche und spätere Filterung funktionieren. [canon-suche-kommunikation-und-beschreibungsmuster]

- Die App soll eine No-Go-Seite mit dynamisch erweiterbarer Liste bereitstellen, auf der kritische Dinge festgehalten werden, die in Gegenwart des Bewohners vermieden werden müssen. [canon-no-go-seite]

- Eine Kalenderfunktion soll erwogen werden; zusätzlich ist zu prüfen, ob auch Medikamentengaben integriert werden sollen, wobei die Vertraulichkeit dieser Daten besonders zu berücksichtigen ist. [canon-kalender-und-medikamente-pruefen]

- Es soll erwogen werden, in der Profilübersicht das Anlegen neuer Profile per Plus-Symbol und Dialog für Bild, Name und Beschreibung zu ermöglichen. [canon-neue-profile-anlegen-pruefen]

- Eine Ausweitung der App auf weitere Dokumentationsfunktionen soll geprüft werden, jedoch nur in begrenztem Umfang und ohne den Fokus auf unterstützende Kommunikation zu verlieren. [canon-erweiterung-dokumentation-pruefen]

## UI- und UX-Leitplanken

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. [canon-appbar-konsistent]

- Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und Logout unterstützen. [canon-appbar-funktionen]

- Bei der Gestaltung soll auf Barrierefreiheit geachtet werden, insbesondere große Schrift, ausreichender Kontrast und zurückhaltender Farbeinsatz. [canon-barrierefreiheit-beruecksichtigen]

- Alternative Eingabemethoden wie Sprachbefehle sollen als mögliche Accessibility-Erweiterung berücksichtigt werden. [canon-alternative-eingabemethoden-pruefen]

- Der Login-Screen soll ein zentriertes, gut sichtbares Logo im oberen Drittel enthalten; ein passendes Kommunikations-Logo ist noch zu erstellen. [ADJ-GAP-AU-0117]

- Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour beim ersten Login und als später erneut aufrufbarer Hilfebereich. [canon-hilfe-tutorial-pruefen]

- Animationen sollen nicht im Fokus stehen; falls sie verwendet werden, dürfen sie nicht ablenkend sein. [canon-animationen-nicht-im-fokus]

## Plattform und technische Leitplanken

- Die App soll plattformübergreifend auf iPhone und Android laufen. [canon-plattform-ios-android]

- Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen. [canon-technologiestack-flutter-dart]

- Das Paket GetX soll als technische Option berücksichtigt und in der Umsetzung erprobt werden. [canon-getx-pruefen]

- Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt. [canon-firebase-firestore-vorlaeufig]

- Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]

- Es soll evaluiert werden, ob die App zusätzlich auf Tablets nutzbar sein kann. [canon-tablet-support-pruefen]

## Spätere mögliche Erweiterungen

- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden. [canon-benachrichtigungen-about-me-updates]

## Prozessrelevanter Architekturkontext

- Die genauen Bedürfnisse und Funktionen des Systems müssen durch Anforderungsanalyse mit mehreren Beteiligten und künftigen Nutzern erarbeitet werden. [canon-anforderungsanalyse-mit-nutzern, ADJ-GAP-AU-0002]

- Zur Nutzerforschung sollen Vor-Ort-Termine in Einrichtungen durchgeführt werden, um reale Kommunikationssituationen und Bedürfnisse besser zu verstehen. [canon-beobachtung-vor-ort]

- Das Produktziel ist zusätzlich dadurch gerahmt, dass Kommunikation im Alltag zentral ist und das Projekt als Weiterentwicklungsmöglichkeit gesehen wird. [ADJ-GAP-AU-0003]