Hier ist die Prüfung des Artefakts **risks.md** gegen das Transkript.

## Befunde

### 1) „Plattformübergreifende Umsetzung mit Flutter/Dart“
- **Artefakt-Stelle:**  
  „### Plattformübergreifende Umsetzung mit Flutter/Dart“
- **Transkript-Beleg:**  
  Flutter/Dart wird nur in **„6. Dritte-Ausarbeitung-Studenten“** von Studierenden als gewählte Technologie besprochen:  
  „… mit dem Framework **Flutter** und der Programmiersprache **Dart** …“
- **Fehlerart:** `FALSE_CLAIM`
- **Begründung:**  
  Im Stakeholder-Transkript wird nur der Bedarf an plattformübergreifender Unterstützung für iPhone/Android/Tablets genannt. **Flutter/Dart** ist keine Stakeholder-Aussage, sondern eine spätere studentische Technologieentscheidung. Für ein Risikoartefakt, das gegen das originale Stakeholder-Transkript geprüft wird, ist die konkrete Technologie hier erfunden bzw. nicht durch Stakeholder belegt.

---

### 2) „Die Anforderungen nennen … Such- und Filterfunktionen“
- **Artefakt-Stelle:**  
  „Die Anforderungen nennen umfangreiche Funktionen (u.a. dynamische Erweiterbarkeit, **Such- und Filterfunktionen**) …“
- **Transkript-Beleg:**  
  Suchfunktion wird später im **Interview-Fachpersonal** konkret angesprochen:  
  „… sollte man nach einem Profil suchen können … eine Art Suchleiste …“  
  sowie  
  „… auch bei den Kommunikationsseiten möglich sein …“
- **Fehlerart:** `FALSE_CERTAINTY`
- **Begründung:**  
  Suchfunktion ist im Transkript diskutiert und befürwortet, aber als **später ergänzte Idee/Anforderung** aus einem Fachgespräch, nicht als bereits feststehender Ausgangspunkt „die Anforderungen nennen …“. Das Artefakt formuliert hier zu bestimmt und pauschal. Die Dynamik der Entstehung und der noch offene Charakter werden nicht kenntlich gemacht.

---

### 3) Risiko „kontrollierte Nutzerkontenvergabe ohne öffentliche Registrierung“ ohne Hinweis auf offene Rollen-/Zugriffsfragen
- **Artefakt-Stelle:**  
  „### Kontrollierte Nutzerkontenvergabe ohne öffentliche Registrierung …“
- **Transkript-Beleg:**  
  Login ohne offene Registrierung ist belegt:  
  „… man sich **nicht registrieren** können soll … nur ein Account mit Admin-Rechten weitere Accounts erstellen kann …“  
  Aber Zugriffsmodell war an mehreren Stellen **offen bzw. im Fluss**:  
  - Angehörige sollen Zugriff haben  
  - Bewohner-Account wird später zusätzlich vorgeschlagen  
  - Zugriff zwischen Einrichtungen war erst noch zu klären
- **Fehlerart:** `FALSE_CERTAINTY`
- **Begründung:**  
  Das Artefakt beschreibt die Nutzerkontenvergabe so, als sei das Zugriffskonzept im Wesentlichen stabil geklärt. Im Transkript sind aber zentrale Fragen zu Rollen, Rechten und Sichtbarkeiten noch in Entwicklung.

---

### 4) Fehlendes Risiko: rollen- und einrichtungsbezogene Zugriffsbeschränkung
- **Artefakt-Stelle:**  
  **Fehlt komplett**
- **Transkript-Beleg:**  
  Im Interview Fachpersonal:  
  „… hat jeder Mitarbeiter in jeder Einrichtung Einsicht auf alle Profile … oder nur auf die in dem Haus …?“  
  Antwort/Entscheidung:  
  „Aufgrund von Datenschutz sollte es so geregelt sein, dass es **nicht übergreifend** ist …“
- **Fehlerart:** `MISSING_TOPIC`
- **Begründung:**  
  Für eine Risikoanalyse ist das ein klar relevantes Thema: falsche Profil-Sichtbarkeit über Einrichtungen hinweg birgt Datenschutz- und Berechtigungsrisiken. Dieses konkrete Risiko fehlt im Artefakt.

---

### 5) Fehlendes Risiko: Einbindung und Berechtigungen für Angehörige
- **Artefakt-Stelle:**  
  **Fehlt komplett**
- **Transkript-Beleg:**  
  Heilerziehungspflegerin:  
  „… wenn auch die **Angehörigen Zugriff** drauf hätten …“  
  „… auch ihre Daten da hinzufügen können …“  
  Studentenseite: verschiedene Accounts/Rechte für Angehörige und Mitarbeiter.
- **Fehlerart:** `MISSING_TOPIC`
- **Begründung:**  
  Die Beteiligung von Angehörigen ist fachlich und datenschutzseitig riskant/relevant: zusätzliche Nutzergruppe, Rechtevergabe, Datenqualität, Verantwortlichkeiten. Das ist für eine Risikoanalyse relevant und fehlt.

---

### 6) Fehlendes Risiko: Bewohner-Account mit eingeschränkten Rechten
- **Artefakt-Stelle:**  
  **Fehlt komplett**
- **Transkript-Beleg:**  
  Interview Fachpersonal:  
  „… wieso sollte der Bewohner nicht auch einen Account … haben?“  
  „… nur sein eigenes Profil … sehen …“
- **Fehlerart:** `MISSING_TOPIC`
- **Begründung:**  
  Ein eigener Bewohner-Account ist ein wesentliches fachliches und sicherheitsrelevantes Thema: Usability, Fehlbedienung, Rechtebegrenzung, Datenschutz. Das ist im Transkript besprochen und für Risiken relevant, im Artefakt aber nicht enthalten.

---

### 7) Fehlendes Risiko: Umgang mit Bildern/Videos und Einwilligungen
- **Artefakt-Stelle:**  
  **Fehlt teilweise als konkretes Thema**
- **Transkript-Beleg:**  
  „… mit den Angehörigen reden, ob es in Ordnung wäre, wenn wir **Bilder** machen bzw. bekommen …“  
  „… in Form von **Videos** gewisse Szenarien abzufilmen …“
- **Fehlerart:** `MISSING_TOPIC`
- **Begründung:**  
  Das Artefakt nennt nur allgemein „unklare Datenschutzregelungen“. Das spezifische Risiko rund um Bild-/Videoaufnahmen, Einwilligungen und sensible mediale Inhalte ist im Transkript explizit und für die Risikoanalyse sehr relevant, fehlt aber als eigener Risikopunkt.

---

### 8) Fehlendes Risiko: Scope Creep durch zusätzliche Funktionen außerhalb des Kommunikationskerns
- **Artefakt-Stelle:**  
  Nur indirekt angedeutet durch „Balance zwischen Funktionsumfang und Benutzerfreundlichkeit“
- **Transkript-Beleg:**  
  Heilerziehungspflegerin/Studierende diskutieren zusätzliche Funktionen wie  
  - komplette Dokumentation  
  - Medikamentenvergabe  
  - Terminkalender  
  - No-Go-Seite  
  Dabei wird explizit gewarnt:  
  „Je mehr sie leisten muss, desto schwerer wird es … desto weniger Freude …“  
  und  
  „… dass … nicht der eigentliche Sinn der unterstützenden Kommunikation verloren gehen soll.“
- **Fehlerart:** `MISSING_TOPIC`
- **Begründung:**  
  Das Artefakt erwähnt Komplexität allgemein, aber nicht das im Transkript klar erkennbare Risiko einer **funktionalen Überdehnung** weg vom eigentlichen Projektziel. Das ist ein eigenständiges fachlich-projektbezogenes Risiko.

---

## Zusammenfassung

- `FALSE_CLAIM`: **1**
- `FALSE_CERTAINTY`: **2**
- `MISSING_TOPIC`: **5**

### Gesamteinschätzung
Das Artefakt trifft einige zentrale Risikothemen grundsätzlich richtig, vor allem Usability, Datenschutz und technische Machbarkeit. Es ist aber **teilweise zu generisch** und übernimmt an einer Stelle sogar eine **nicht stakeholder-validierte Technologieentscheidung (Flutter/Dart)**. Außerdem fehlen mehrere **transkriptspezifische Risikothemen**, insbesondere zu **Rollen/Rechten, einrichtungsbezogenen Zugriffen, Angehörigen-/Bewohner-Accounts sowie Bild-/Videoeinwilligungen**. Insgesamt also **brauchbare Basis, aber inhaltlich nicht sauber am Ground Truth ausgerichtet**.