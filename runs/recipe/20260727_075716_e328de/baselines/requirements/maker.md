# Requirements

## Funktionale Anforderungen

- Es muss eine digitale Lösung in Form einer App oder eines ähnlichen Computerprogramms bereitgestellt werden, um die Kommunikation zwischen beeinträchtigten Menschen und anderen Personen zu verbessern und zu fördern. [CAN-001]
- Das System darf keine individuelle Übersetzungs-Schnittstelle bereitstellen, die zwischen einem einzelnen Bewohner und z. B. einem Betreuer hin und her übersetzt. [CAN-002]
- Die App muss Informationen darüber speichern, wie eine Person kommuniziert, und diese für Betreuer und andere Nutzer jederzeit abrufbar machen, damit bei Verständnisschwierigkeiten nachgeschaut werden kann, was gemeint sein könnte. [CAN-003]
- Die App muss mindestens die Hauptbereiche „About Me“ und „Kommunikationsseiten“ enthalten; zusätzlich muss sie einen „No-Go“-Bereich enthalten. [CAN-004, CAN-006]
- Der Bereich „About Me“ muss Informationen und Bilder zur Person enthalten. [CAN-004]
- Die Kommunikationsseiten müssen verbale und nonverbale Kommunikation abdecken und erweiterbar sein. [CAN-004]
- Die Videofunktionalität muss in die verbalen und nonverbalen Kommunikationsseiten integriert werden, sodass Videos dort direkt zusammen mit Beschreibungen hinzugefügt und gesucht werden können. [CAN-014]
- Die App muss eine „No-Go“-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht bzw. unbedingt zu vermeiden ist. [CAN-006]
- Die App muss eine Suchleiste auf der Profilübersicht bereitstellen, damit nach Profilen gesucht werden kann. [CAN-012]
- Die App muss eine Suchleiste auf den Kommunikationsseiten bereitstellen, damit Kommunikationsarten schnell und gezielt gefunden werden können. [CAN-012]
- Die App muss einen speziellen Bewohner-Account bereitstellen, der nur Zugriff auf das eigene Profil hat. [CAN-015]
- Mit dem Bewohner-Account muss mindestens der „About Me“-Bereich des eigenen Profils nutzbar sein, um eigene Bilder hinzuzufügen. [CAN-015]

## Benutzer- und Rechtemodell

- Die App muss verschiedene Account-Typen unterstützen, mindestens Mitarbeiter und Angehörige. [CAN-005]
- Mitarbeiter- und Angehörigen-Accounts müssen unterschiedliche Zugriffsrechte und Funktionen haben. [CAN-005]
- Angehörige müssen Zugriff auf die App erhalten können. [CAN-005]
- Angehörige müssen Daten hinzufügen können. [CAN-005]
- Die App muss eine Login-Funktion mit mindestens den Account-Typen Admin und User bereitstellen. [CAN-007]
- Admin-Accounts müssen neue Accounts anlegen und verwalten können. [CAN-007]
- User-Accounts dürfen Inhalte hinzufügen, aber keine Inhalte löschen. [CAN-007]
- Nutzer dürfen sich nur mit einem zugewiesenen Account einloggen. [CAN-008]
- Eine Selbstregistrierung durch Nutzer darf nicht möglich sein. [CAN-008]
- Mitarbeiter dürfen nur auf die Profile der Bewohner ihrer eigenen Einrichtung zugreifen. [CAN-009]

## Plattform und Technologie

- Die App muss plattformübergreifend lauffähig sein und sowohl auf iPhones als auch auf Android-Handys funktionieren. [CAN-010]
- Für die Entwicklung der App müssen Flutter und die Programmiersprache Dart verwendet werden. [CAN-016]
- Das Team muss als einheitliche Entwicklungsumgebung Android Studio verwenden. [CAN-017]
- Das Team muss einheitlich Flutter in Version 3.13.9 verwenden. [CAN-017]
- Das Team muss einheitlich Dart in Version 3.1.5 verwenden. [CAN-017]

## Nicht-funktionale Anforderungen

- Das Design der App muss benutzerfreundlich, barrierefrei und intuitiv sein. [CAN-019]
- Die App muss eine klare Navigation unterstützen. [CAN-019]
- Die App muss ausreichende Hilfe-Funktionen bereitstellen. [CAN-019]
- Bei der Gestaltung der App muss Barrierefreiheit berücksichtigt werden, insbesondere große Schrift und ausreichender Kontrast. [CAN-019]

## Offene Punkte und zu klärende Anforderungen

- Für die Eingabe von Kommunikationsweisen muss ein standardisiertes Muster definiert werden, um Suche und Filterung zu erleichtern; als genannter Ansatz wurde genannt, zuerst die Art bzw. das Körperteil und dann die Beschreibung zu erfassen. Die genaue Formulierung ist noch zu klären. [CAN-013]
- Es soll geprüft werden, ob die App auch auf Tablets lauffähig sein soll und ob dies umsetzbar ist. [CAN-011]
- Es muss berücksichtigt werden, ob Firebase Firestore als Datenbank verwendet und durch lokale Speicherung ergänzt werden soll, sodass Daten aus Firebase geladen und lokal gespeichert werden können. Dies ist noch offen. [CAN-018]

## Kontext und dokumentierte Hinweise

- Als offener, noch nicht festgelegter Punkt ist eine Profilübersicht nach dem Login beschrieben, die eine Liste aller Profile zeigt und von der aus jedes Profil zu einer Detailseite führt. [ADJ-GAP-AU-0045]
- Als offener, noch nicht festgelegter Punkt ist eine Appbar nach dem Login auf jeder Seite beschrieben, mit Zurücknavigation, Logout-Symbol und möglicherweise einem Einstellungssymbol; die Ausgestaltung der Einstellungen ist noch offen. [ADJ-GAP-AU-0054]
- Die App wird im Nutzungskontext als Unterstützung für neue Mitarbeiter, neue Bewohner und erstmalige Kommunikationssituationen gesehen; außerdem wurde eine Vorab-Datenpflege durch Angehörige beschrieben. [ADJ-GAP-AU-0077]
- Als offener, noch nicht festgelegter Punkt wurde eine Popup-Nachricht für alle verbundenen Nutzer einer Einrichtung bei neuen Uploads im „About Me“-Bereich genannt. [ADJ-GAP-AU-0078]
- Als offener, noch nicht festgelegter Punkt wurde eine Hilfe- oder Tutorial-Seite genannt, die zeigt, wie Informationen effektiv eingegeben und gesucht werden können. [ADJ-GAP-AU-0086]
- Als offener, noch nicht festgelegter Punkt wurde die Verwendung des JetX Package zur Reduktion des Entwicklungsaufwands genannt. [ADJ-GAP-AU-0114, ADJ-GAP-AU-0115]
- Für den Login-Screen wurden als offene Designideen ein zentriertes rundes Logo im oberen Drittel sowie darunter angeordnete E-Mail- und Passwort-Felder genannt; optionale visuelle Effekte wie leichter Schattenwurf sind noch offen. [ADJ-GAP-AU-0117, ADJ-GAP-AU-0118]
- Für die Profilübersicht wurden als offene Darstellungsoptionen Kacheln oder eine Liste unterhalb der Suchleiste mit Vorschaubild, Name und kurzer Beschreibung genannt. [ADJ-GAP-AU-0122]
- Für das Hinzufügen neuer Profile wurde als offener Punkt ein Plus-Symbol am unteren Bildschirmrand mit anschließendem Dialogfeld für Bild, Name und Beschreibung genannt. [ADJ-GAP-AU-0123]
- Für die Detailansicht eines Profils wurde als offener Punkt beschrieben, das Profilbild vergrößert anzuzeigen und die vier Hauptbereiche der App als interaktive Buttons darzustellen. [ADJ-GAP-AU-0124]
- Für einen Kalender wurde als offener Punkt eine monatliche Ansicht mit Icons oder Tags für Medikamentenerinnerungen sowie eine Detailansicht bei Datumsauswahl genannt. [ADJ-GAP-AU-0129]
- Als offene Überlegung zur Barrierefreiheit wurden Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer genannt. [ADJ-GAP-AU-0134]
- Für Animationen wurde als offener Hinweis genannt, dass sie nicht zu ablenkend sein sollten. [ADJ-GAP-AU-0135]