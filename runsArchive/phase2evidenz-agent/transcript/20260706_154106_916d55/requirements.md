# Requirements

## Funktionale Anforderungen

### Benutzerverwaltung und Zugangskontrolle
- Es soll nur eine Login-Möglichkeit geben, keine Selbstregistrierung durch Nutzer.
- Es gibt mindestens drei Accounttypen: Admin, User (Mitarbeiter/Angehörige), Bewohner.
- Admins können neue Accounts anlegen, Rechte vergeben und Accounts verwalten.
- User dürfen Inhalte hinzufügen, aber keine Accounts anlegen oder Daten löschen.
- Bewohner haben eingeschränkte Rechte und dürfen nur ihr eigenes Profil sehen sowie eigene Inhalte im About Me-Bereich hinzufügen.
- Mitarbeiter haben nur Zugriff auf Profile der Bewohner der eigenen Einrichtung, nicht auf andere Einrichtungen.
- Angehörige sollen eigene Accounts erhalten und Inhalte beisteuern können.
- Nach dem Login wird eine Profilübersicht mit allen Profilen der Bewohner der jeweiligen Einrichtung angezeigt.
- Eine Suchleiste in der Profilübersicht ermöglicht das schnelle Finden eines Profils über Namen.
- Auf Kommunikationsseiten (verbal/nonverbal) soll ebenfalls eine Suchfunktion integriert werden.

### Profil- und Datenverwaltung
- Jedes Profil enthält die Hauptbereiche: About Me, Kommunikation, Video (integriert in Kommunikation), No-Go und Kalender/Medikamentenverwaltung.
- About Me:
  - Anzeige von Basisinformationen (Name, Alter, Hobbys etc.).
  - Dynamisch erweiterbare Foto-Timeline mit Bild und Beschreibung.
  - Neu hinzugefügte Fotos erscheinen oben in der Liste.
- Kommunikation:
  - Unterteilt in verbale und nonverbale Kommunikationsarten.
  - Dynamisch erweiterbar durch Texte, Bilder und Videos.
  - Videos sind Teil der Kommunikationsseiten und dienen zur Erklärung von Kommunikationsweisen.
- No-Go-Bereich:
  - Darstellung von Verhaltensweisen oder Situationen, die vermieden werden müssen.
  - Dynamisch erweiterbar durch neue Einträge.
- Kalender/Medikamentenverwaltung:
  - Übersichtliche Monatsansicht mit Terminen und Medikamenten.
  - Möglichkeit, Einträge anzulegen und Details einzusehen.
- Bei jeder Aktualisierung (z. B. neuer Eintrag im About Me) erhalten alle Nutzer der jeweiligen Einrichtung eine Popup-Benachrichtigung.

### Inhalte hinzufügen und bearbeiten
- Jeder Hauptbereich (About Me, Kommunikation, No-Go, Video, Kalender) hat einen Plus-Button zum Hinzufügen neuer Einträge.
- Beim Hinzufügen von Kommunikationsarten und Videos sollen Nutzer ein standardisiertes Beschreibungsmuster nutzen (z.B. erst Körperteil, dann Bedeutung).
- Videos können Situationen abbilden, die die Kommunikationsweise erklären.
- Angehörige und Mitarbeiter können Inhalte gleichermaßen hinzufügen.
- Bewohner können beschränkt eigene Inhalte in ihrem Profil ergänzen.

### Zugriff und Nutzung
- Die App soll plattformübergreifend funktionieren (iOS und Android).
- Die App soll sowohl auf Smartphones als auch Tablets nutzbar sein.
- Die App ist vor allem für interne Nutzung bei KYOS Einrichtungen vorgesehen.
- Neue Bewohner können bereits vor ihrem Einzug von Angehörigen im System angelegt und mit Inhalten versehen werden.
- Mitarbeiter und neue Kollegen nutzen die App zur schnellen Einarbeitung und besseren Kommunikation mit Beeinträchtigten.

### Sonstige funktionale Anforderungen
- Es soll eine konsistente Appbar auf allen Bildschirmseiten geben:
  - Links Zurück-Pfeil (Navigation zur vorherigen Seite).
  - Mitte: Titel der aktuellen Seite.
  - Rechts: Einstellungssymbol (Navigieren zu den Einstellungen).
- Es soll einen Einstellungsbildschirm geben mit differenzierten Einstellungen je Nutzerrolle.
- Die App soll eine einfache Kommunikation der Mitarbeitenden und Angehörigen erleichtern, um die Bedürfnisse der Beeinträchtigten besser zu verstehen und darauf einzugehen.

---

## Nicht-funktionale Anforderungen

### Bedienbarkeit und Benutzerfreundlichkeit
- Die App soll intuitiv und benutzerfreundlich gestaltet sein.
- Große Schrift und ausreichender Kontrast für Barrierefreiheit.
- Vermeidung von zu vielen Farben und ablenkenden Animationen.
- Ein Tutorial bzw. Hilfebereich soll beim ersten Login und danach jederzeit verfügbar sein, um Nutzer in die Funktionen einzuführen.
- Die App-Oberflächen (z.B. About Me Feed) sollen einer Social Media-ähnlichen Darstellung entsprechen (Timeline mit neuen Einträgen oben).
- Suchfunktionen sollen die Benutzerführung erleichtern.

### Datenschutz und Sicherheit
- Kein offenes Registrieren möglich, nur zugewiesene Accounts erhalten Zugang.
- Sensible Daten werden nur für die jeweilige Einrichtung freigegeben, kein Einrichtungsübergreifender Zugriff.
- Datenschutzbelange werden bedacht, Bilder und Videos nur mit Einwilligung der Angehörigen verwendet.
- Benutzerrechte strikt geregelt, damit Nutzer nur auf befugte Daten zugreifen können.
- Die App soll primär als interne Lösung der KYOS genutzt werden.

### Technische Rahmenbedingungen
- Verwendung einer plattformübergreifenden Programmiersprache (z.B. Flutter mit Dart) für iOS und Android.
- Entwicklung unter einer einheitlichen Entwicklungsumgebung (vorzugsweise Android Studio).
- Verwendung von Firebase Firestore als Cloud-Datenbank mit möglichem lokalem Caching (z.B. via NoSQLite).
- App-Entwicklung auf Android 11 als Mindestversion für Emulator/Tests.
- Integration eines zentralen Kommentarsystems und Coding-Regeln im Entwicklungsteam (Details noch festzulegen).
- Nutzung von unterstützenden Packages wie JetX zur Arbeitserleichterung bei der Entwicklung.
- Die App soll responsive auf Smartphones und Tablets laufen.
- Push-Benachrichtigungen oder Popup-Meldungen bei Aktualisierungen für Nutzer innerhalb einer Einrichtung.

---

## Offene/zu klärende Punkte und Vorschläge (nicht verpflichtend)

- Genaue Ausgestaltung der Rechteverwaltung und Administration.
- Ausgestaltung und Grenzen der Integration der Dokumentation in die App.
- Konkrete Inhalte und Aufbau der Kommunikationsseiten (z.B. exakter Aufbau der Beschreibungsmuster).
- Konkrete Gestaltungs- und UI-Details der Suchfunktionen und Filtermöglichkeiten.
- Umfang und Gestaltung der Videoseite bzw. Integration von Videos in Kommunikationsseiten.
- Umgang mit Datenschutz bei Aufnahme und Nutzung von Bildern und Videos (z.B. Einholung von Erlaubnissen).
- Ob und wie alternative Eingabemethoden oder Sprachsteuerung für beeinträchtigte Nutzer realisiert werden können.
- Ob weitere Funktionen wie Erinnerungen oder weitere Kalenderfunktionen eingebaut werden.
- Integration von Angehörigen als eigene Nutzergruppe mit beschränkten oder erweiterten Rechten.
- Unterstützung von mehreren Einrichtungen in der App mit klarer Abgrenzung der Daten.
- Umfang der Einstellungen für die Nutzer (z.B. Sprache, Profil), noch auszuarbeiten.
- Design des Logos und insbesondere die genaue grafische Umsetzung der App.

---

# Zusammenfassung (Kernpunkte)

- Entwicklung einer plattformübergreifenden App zur Unterstützung der Kommunikation zwischen beeinträchtigten Bewohnern und Betreuern/Angehörigen.
- Zentrale Funktionalitäten: Profilübersicht, About Me (Infos & Bilder), Kommunikation (verbale & nonverbale mit Texten/Bildern/Videos), No-Go-Liste, Kalender/Medikamentenverwaltung.
- Rollenbasierte Zugriffssteuerung: Admin, User (Betreuer/Angehörige), Bewohner mit differenzierten Rechten.
- Keine offene Registrierung, Accounts werden zentral verwaltet.
- Such- und Filterfunktionen in Profil- und Kommunikationsübersichten für schnelle Navigation.
- Integration von sozialen Medien ähnlichen Feeds und leicht bedienbarer Oberfläche.
- Datenschutz und eingeschränkter Zugang zu sensiblen Daten strikt vorgesehen.
- Entwicklung mit Flutter/Dart, Firebase Firestore und Android Studio geplant.
- Design mit Fokus auf Barrierefreiheit, Benutzerfreundlichkeit und konsistenter Navigation.
- Einbindung von Angehörigen als aktive Beitragsleistende mit Zugang zu eigenen Nutzeraccounts.
- Möglichkeit Videos zur besseren Erklärung der Kommunikationsweisen zu verwenden.
- Dynamische Erweiterung und ständiges Wachstum der gespeicherten Kommunikationsarten und Profileinträge.
- Popup-Benachrichtigungen bei neuen Einträgen für alle Nutzer einer Einrichtung.

