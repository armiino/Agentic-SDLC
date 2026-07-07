# Requirements

## Zielbild / Zweck
- Die Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen unterstützen, ermöglichen oder verbessern.
- Die Lösung soll bevorzugt digital umgesetzt werden, z. B. als App oder Computerprogramm.
- Die Unterstützung soll vorrangig darauf ausgerichtet sein, Bewohner besser zu verstehen, nicht umgekehrt.
- Es soll **nicht** Ziel der Lösung sein, als generische Übersetzungsschnittstelle zwischen individueller Kommunikationsweise eines Bewohners und Standardsprache zu fungieren; ein solches Konzept wurde im Gespräch als nicht sinnvoll bzw. nicht kosteneffizient eingeordnet.

## Nutzergruppen
- Die App soll von Mitarbeitenden der Einrichtung nutzbar sein.
- Angehörige sollen Zugriff auf die App erhalten können.
- Ein spezieller Bewohner-Account ist gewünscht, damit ein Bewohner auf sein eigenes Profil zugreifen kann.
- Die genauen Bedürfnisse weiterer Beteiligter und Nutzer sollen in weiteren Gesprächen erhoben werden.

## Funktionale Anforderungen

### Zugriff und Accounts
- Die App soll einen Login bereitstellen.
- Die App soll keine Selbstregistrierung anbieten.
- Accounts sollen ausschließlich durch berechtigte Stellen bzw. einen Admin angelegt werden.
- Die App soll mindestens unterschiedliche Rollen unterstützen:
  - Admin
  - User
- Zusätzlich ist ein Bewohner-Account gewünscht; dieser soll in die Rollen- und Rechteverwaltung aufgenommen werden.
- Ein Admin soll neue Accounts anlegen können.
- Ein Admin soll Rechte von Accounts verwalten können.
- User sollen Inhalte hinzufügen können.
- User sollen keine Accounts anlegen können.
- User sollen vorhandene Daten nicht löschen können.
- Admins sollen Inhalte löschen können.
- Der Bewohner-Account soll nur das eigene Profil sehen können.
- Der Bewohner-Account soll auf den „About Me“-Bereich des eigenen Profils zugreifen können.
- Es ist gewünscht, dass der Bewohner-Account im „About Me“-Bereich eigene Bilder hinzufügen kann.
- Die konkrete Rechteverwaltung soll noch weiter ausgearbeitet werden.

### Mandanten-/Einrichtungsbezug
- Mitarbeitende sollen nur Zugriff auf die Profile der Bewohner der Einrichtung erhalten, in der sie tätig sind.
- Ein einrichtungsübergreifender Zugriff für Mitarbeitende soll aus Datenschutzgründen vermieden werden.
- Die technische Umsetzung der einrichtungsbezogenen Zugriffsbeschränkung soll noch erarbeitet werden.

### Profilübersicht und Navigation
- Nach erfolgreichem Login soll die App auf eine Profilübersicht führen.
- Die Profilübersicht soll eine Liste aller für den Nutzer sichtbaren Bewohnerprofile anzeigen.
- Jedes Profil in der Profilübersicht soll auswählbar sein und zu einer Detailansicht des gewählten Profils führen.
- In der Profilübersicht soll eine Suchfunktion vorhanden sein.
- Die Suchfunktion in der Profilübersicht soll es ermöglichen, Profile über den Namen zu finden.
- In der Profilübersicht sollen Profile mit Vorschaubild, Name und kurzer Beschreibung angezeigt werden.
- Ein Hinzufügen neuer Profile über die Profilübersicht wurde in der Design-Ausarbeitung beschrieben; ob dies fachlich gewünscht und für welche Rollen erlaubt ist, soll geklärt werden.

### Profilbereiche
- Ein Profil soll mindestens die Bereiche „About Me“, „Kommunikation“ und „No-Go“ enthalten.
- Ein Kalenderbereich ist als zusätzliche Funktion gewünscht; der Umfang soll noch abgestimmt werden.
- Eine separate Videoseite wurde zunächst vorgeschlagen, später jedoch zugunsten einer Integration in die Kommunikationsbereiche verworfen.
- Videos zu Kommunikationssituationen sollen in die Kommunikationsbereiche integriert werden.

### About Me
- Der Bereich „About Me“ soll einen ersten Eindruck über die Person vermitteln.
- „About Me“ soll allgemeine Informationen zur Person enthalten, z. B. Name, Alter, Hobbys und ähnliche persönliche Angaben.
- „About Me“ soll Bilder mit Beschreibungen enthalten.
- Neue Bilder sollen im „About Me“-Bereich hinzugefügt werden können.
- Das Hinzufügen neuer Bilder soll über einen Plus-Button erfolgen, der einen Dialog zum Hinzufügen von Bild und Beschreibung öffnet.
- Neu hinzugefügte Bilder sollen in der Liste als neueste Einträge oben erscheinen.
- Die Bildliste im „About Me“-Bereich soll dynamisch wachsen.
- Angehörige sollen Inhalte zum Profil beitragen können; dies soll insbesondere bei neuen Bewohnern ein initiales Befüllen der Daten ermöglichen.

### Kommunikation
- Der Kommunikationsbereich soll Informationen zur Art und Weise enthalten, wie ein Bewohner kommuniziert.
- Der Kommunikationsbereich soll in „verbal“ und „nonverbal“ unterteilt sein.
- Für verbale und nonverbale Kommunikation sollen Inhalte gespeichert und jederzeit abrufbar sein.
- Im Kommunikationsbereich sollen neue Kommunikationseinträge hinzugefügt werden können.
- Das Hinzufügen neuer Kommunikationseinträge soll über einen Plus-Button erfolgen.
- Kommunikationseinträge sollen nicht nur textuell, sondern auch visuell dargestellt werden können, z. B. mit Bildern.
- Videos zu bestimmten Kommunikationssituationen oder Verhaltensweisen sollen innerhalb der verbalen bzw. nonverbalen Kommunikationsbereiche hinterlegt werden können.
- Zu Videos sollen Beschreibungen hinterlegt werden können.
- Neue Videos zu neu erkannten Situationen sollen hinzugefügt werden können.
- Die Kommunikationsbereiche sollen eine Suchfunktion bereitstellen.
- Die Suchfunktion im Kommunikationsbereich soll nach Beschreibungen bzw. Begriffen filtern können, z. B. nach Körperteilen wie „Fuß“.
- Für die Beschreibung von Kommunikationseinträgen ist eine standardisierte Eingabestruktur gewünscht, damit spätere Suche und Filterung verbessert werden.
- Die genaue Form dieser Standardisierung soll noch abgestimmt werden.
- Es wurde angeregt, Eingabedialoge so zu gestalten, dass strukturierte Felder vorgegeben werden; dies ist noch zu konkretisieren.

### No-Go
- Die App soll einen „No-Go“-Bereich bereitstellen.
- Im „No-Go“-Bereich sollen Dinge festgehalten werden, die in Gegenwart des Bewohners unbedingt zu vermeiden sind.
- Im „No-Go“-Bereich sollen neue Einträge hinzugefügt werden können.
- Die „No-Go“-Einträge sollen dynamisch wachsen.

### Kalender / Termine / Medikamente
- Ein Kalenderbereich ist gewünscht.
- Der Kalenderbereich soll Termine eines Bewohners darstellen können.
- Es ist gewünscht, im Kalenderbereich auch Medikamentengaben abzubilden.
- Die Funktion zur Medikamentenverwaltung wurde als möglich und nützlich beschrieben, zugleich aber als sensibel/vertraulich eingeordnet.
- Ob die App die vollständige Dokumentation oder Medikamentenverwaltung übernehmen soll, ist noch mit der Leitung zu klären.
- Der Kalender soll übersichtlich gestaltet sein.
- Tage im Kalender sollen auswählbar sein.
- Für auswählbare Tage sollen Einträge hinzugefügt werden können.
- Die konkrete Ausgestaltung der Medikamentenfunktion soll noch geklärt werden.

### Benachrichtigungen
- Wenn im „About Me“-Bereich neue Inhalte hochgeladen werden, sollen alle mit der jeweiligen Einrichtung verbundenen Nutzer eine Popup-Nachricht erhalten.

### Einstellungen / Hilfe
- Nach dem Login soll auf allen Seiten eine Appbar vorhanden sein.
- Die Appbar soll eine Rücknavigation ermöglichen.
- Über die Appbar soll ein Logout möglich sein.
- Ein Einstellungsbereich ist vorgesehen.
- Für Admins soll ein spezieller Bereich vorgesehen werden, in dem Accounts angelegt und Rechte vergeben werden können.
- Jeder Nutzer soll eigene Profileinstellungen ändern können; dies wurde im Design vorgeschlagen und soll fachlich bestätigt werden.
- Eine Sprachauswahl in den Einstellungen wurde im Design vorgeschlagen; ob diese benötigt wird, ist noch zu klären.
- Eine Hilfe-Funktion bzw. ein Hilfebereich ist gewünscht.
- Ein Tutorial bzw. eine kurze Einführung beim ersten Login ist gewünscht.

## Nicht-funktionale Anforderungen

### Bedienbarkeit
- Die App soll benutzerfreundlich und intuitiv nutzbar sein.
- Die App soll Informationen schneller auffindbar machen als die aktuelle papierbasierte Dokumentation.
- Die App soll Inhalte in einer Form bereitstellen, die für neue Mitarbeitende und andere unbekannte Kontaktpersonen eine schnelle Orientierung ermöglicht.
- Die App soll dynamisch erweiterbar sein, damit neue Erfahrungen und neues Wissen laufend ergänzt werden können.

### Darstellung
- Neben Texten sollen alternative Darstellungen wie Bilder und Videos unterstützt werden.
- Die Darstellung soll so gewählt werden, dass Kommunikationsmuster und persönliche Informationen schnell erfassbar sind.

### Barrierefreiheit
- Das Design soll Barrierefreiheit berücksichtigen.
- Große Schrift ist gewünscht.
- Ausreichender Kontrast ist gewünscht.
- Die Verwendung zu vieler Farben soll vermieden werden.
- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer wurden vorgeschlagen; ob dies umgesetzt werden soll, ist noch offen.

### Datenschutz und Sicherheit
- Der Zugriff auf sensible Daten soll durch Login und zugewiesene Accounts beschränkt werden.
- Die App ist für den internen Gebrauch gedacht.
- Datenschutz ist ein wesentliches Thema, wurde im Projekt jedoch noch nicht im Detail ausgearbeitet.
- Die Einbindung von Bildern und ggf. Videos realer Bewohner soll nur nach Klärung mit Datenschutz und Angehörigen erfolgen.
- Die Einsicht in bestehende Akten ist aufgrund von Datenschutz potenziell eingeschränkt und muss gesondert geklärt werden.

## Technische Anforderungen / Rahmenbedingungen
- Die Lösung soll nach Möglichkeit plattformübergreifend auf iOS und Android lauffähig sein.
- Eine Nutzung auf Tablets ist gewünscht; die Umsetzbarkeit soll evaluiert werden.
- Für die technische Umsetzung wurde Flutter/Dart im Team vorgeschlagen; dies stammt aus der internen Ausarbeitung und ist als derzeitige Umsetzungsabsicht zu verstehen, nicht als fachlich bestätigte Muss-Anforderung.
- Für Datenspeicherung wurden Firebase/Firestore sowie lokale Speicherung diskutiert; dies ist als technischer Vorschlag zu verstehen und noch nicht fachlich festgelegt.

## Offene Punkte / Klärungsbedarf
- Es soll geklärt werden, welche Inhalte aus bestehenden Akten in die App übernommen werden dürfen und sollen.
- Es soll geklärt werden, ob die App lediglich Kommunikationsunterstützung bieten oder zusätzlich Teile der allgemeinen Dokumentation übernehmen soll.
- Es soll geklärt werden, in welchem Umfang Kalender- und Medikamentenfunktionen enthalten sein sollen.
- Es soll geklärt werden, ob neue Profile in der App angelegt werden dürfen und welche Rollen dies dürfen.
- Es soll geklärt werden, wie die Rollen und Rechte im Detail ausgestaltet werden.
- Es soll geklärt werden, wie die einrichtungsbezogene Zugriffsbeschränkung technisch umgesetzt wird.
- Es soll geklärt werden, wie die standardisierte Beschreibung von Kommunikationseinträgen konkret aussehen soll.
- Es soll geklärt werden, ob eine Sprachauswahl, Sprachbefehle oder alternative Eingabemethoden tatsächlich benötigt werden.
- Es soll geklärt werden, ob und unter welchen Bedingungen Bilder und Videos von Bewohnern für Tests und Betrieb verwendet werden dürfen.
- Es sollen weitere Anforderungen gemeinsam mit zusätzlichen Nutzergruppen, insbesondere Fachpersonal und weiteren Beteiligten, erhoben werden.