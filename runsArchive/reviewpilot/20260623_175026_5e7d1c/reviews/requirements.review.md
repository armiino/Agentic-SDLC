Hier ist die Prüfung des Artefakts gegen das Transkript.

## Befunde

### 1) „Eine Kalenderfunktion für Termine und Medikamentengaben wird integriert.“
- **Artefakt-Stelle:** Functional Requirements, Bullet „Eine Kalenderfunktion für Termine und Medikamentengaben wird integriert.“
- **Transkript-Beleg:** Das Thema wird mehrfach als Idee/Vorschlag diskutiert, z. B.:
  - „Man könnte auch die Medikamentenvergabe als Terminkalender hinzufügen …?“
  - „… eventuell noch eine Medikamentenvergabe und Terminkalenderseite zu bauen …“
  - später: „Der Kalender sollte … nicht nur Termine, sondern auch Medikamentengaben enthalten …“
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Im Stakeholder-Teil ist die Kalender-/Medikationsfunktion zunächst als Möglichkeit bzw. Vorschlag formuliert. Sie wird im Verlauf zwar positiv aufgenommen, aber nicht als final entschiedene Muss-Anforderung mit stabiler fachlicher Klärung dokumentiert. Zusätzlich wird gesagt: „Medikamente werden etwas vertraulicher gehandhabt.“ Das Artefakt formuliert hier zu entschieden.

### 2) „Die technische Umsetzung soll plattformübergreifend erfolgen, bevorzugt mit Flutter/Dart.“
- **Artefakt-Stelle:** Non-functional Requirements, Bullet „… bevorzugt mit Flutter/Dart.“
- **Transkript-Beleg:** Flutter/Dart kommt nur in den **studentischen Ausarbeitungen/Implementierungsdiskussionen** vor:
  - „… mit dem Framework Flutter und der Programmiersprache Dart …“
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** Im eigentlichen Stakeholder-Transkript ist nur plattformübergreifend für iPhone/Android und evtl. Tablet genannt. Eine Präferenz der Stakeholder für Flutter/Dart ist **nicht vorhanden**. Das ist eine technische Lösung des Teams, keine aus dem Interview abgeleitete fachliche Anforderung.

### 3) „Es wird angenommen, dass Flutter/Dart als Technologie … genutzt wird, da dies angedacht ist.“
- **Artefakt-Stelle:** Assumptions and Open Points, erster Bullet
- **Transkript-Beleg:** Flutter/Dart nur in interner Studentendiskussion, nicht als Stakeholder-Aussage.
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** Auch als Annahme ist diese Aussage nicht aus dem Stakeholder-Interview herleitbar. Für die Prüfung gegen das Ground-Truth-Transkript ist das eine nicht stakeholderseitig belegte Technologiebehauptung.

### 4) Fehlende Einschränkung der Profilsicht nach Einrichtung
- **Artefakt-Stelle:** gesamtes Artefakt – kein entsprechender Punkt vorhanden
- **Transkript-Beleg:**  
  - „… aufgrund von Datenschutz sollte es so geregelt sein, dass es nicht übergreifend ist.“
  - „Also ein Mitarbeiter aus dem Salzburger sollte nur diese Profile sehen … die auch tatsächlich im Salzburger Weg sind.“
  - „Alles klar, so wird das gemacht.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Die Beschränkung, dass Mitarbeiter nur Profile ihrer eigenen Einrichtung sehen dürfen, ist im Transkript ein relevantes fachliches Thema zu Zugriff und Datenschutz. Im Requirements-Artefakt fehlt dieser Punkt komplett.

### 5) Fehlende Push-/Popup-Benachrichtigung bei neuen About-Me-Inhalten
- **Artefakt-Stelle:** gesamtes Artefakt – kein entsprechender Punkt vorhanden
- **Transkript-Beleg:**  
  - „Jedes Mal, wenn etwas Neues im About Me Bildschirm hochgeladen wird, sollen alle User, die mit der App verbunden sind und Teil der Einrichtung sind, eine Popup-Nachricht erhalten.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Das ist eine konkrete, im Transkript formulierte Funktionsidee mit erkennbarem Nutzwert. Für ein requirements.md ist sie relevant, fehlt aber vollständig.

### 6) Fehlende Beschränkung des Bewohner-Accounts auf eigenes Profil / spezielle Rechte
- **Artefakt-Stelle:** Functional Requirements nennt Bewohnerrolle allgemein, aber ohne diese konkrete Einschränkung
- **Transkript-Beleg:**  
  - „Dieser Bewohner-Account sollte aber spezielle Rechte haben und auch nur sein eigenes Profil in der Profilübersicht … sehen.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Das Artefakt erwähnt zwar Bewohnerrollen und eingeschränkte Bedienbarkeit allgemein, aber die zentrale fachliche Einschränkung „nur eigenes Profil sichtbar“ fehlt komplett. Das ist ein wichtiger konkreter Zugriffsaspekt.

### 7) Videofunktion als eigener Bereich statt Integration in Kommunikationsseiten
- **Artefakt-Stelle:** Functional Requirements, Bullet „… Kommunikationsarten in Text, Bild und Video.“ sowie implizit eigenständige Video-Unterstützung
- **Transkript-Beleg:**  
  - Zunächst separate Videoseite als Idee
  - später explizite Änderung: „… mehr Sinn machen würde, einfach den Videoscreen und die Kommunikationsscreens zusammenzufügen …“
  - Antwort: „Ich finde, so macht es am meisten Sinn. Machen Sie es genauso …“
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** Im finaleren Gesprächsstand wird die separate Videoseite zugunsten einer Integration in die Kommunikationsseiten geändert. Das Artefakt hält diese Klärung nicht fest und suggeriert eher weiterhin einen separaten Videoaspekt.

## Zusammenfassung

- **FALSE_CLAIM:** 3
- **FALSE_CERTAINTY:** 1
- **MISSING_TOPIC:** 3

**Gesamteinschätzung:**  
Das Artefakt trifft die Grundidee der App recht gut, enthält aber mehrere Probleme: technische Entscheidungen aus internen Teamgesprächen werden als Anforderungen aus dem Transkript dargestellt, einzelne offene Punkte werden zu verbindlich formuliert, und einige wichtige fachliche Anforderungen aus späteren Klärungen fehlen. Insgesamt als erste Verdichtung brauchbar, aber noch nicht sauber auf den tatsächlichen Stakeholder-Stand zurückgeführt.