Hier ist die Prüfung des Artefakts **risks.md** gegen das Original-Transkript.

## Befunde

### 1) „Plattformübergreifende Umsetzung mit Flutter/Dart“
- **Artefakt-Stelle:**  
  „### Plattformübergreifende Umsetzung mit Flutter/Dart“
- **Transkript-Beleg:**  
  Flutter/Dart wird nur in **„6. Dritte-Ausarbeitung-Studenten“** als teaminterne Technologieentscheidung genannt:  
  „… mit dem Framework **Flutter** und der Programmiersprache **Dart** …“
- **Fehlerart:** FALSE_CLAIM
- **Begründung:**  
  Im Stakeholder-Transkript wird nur der Wunsch nach **plattformübergreifender Nutzung** (iPhone/Android, evtl. Tablet) geäußert, aber **nicht** Flutter/Dart als stakeholderseitige Anforderung oder projektrelevanter Kontext im Risikoartefakt festgelegt. Das Artefakt stellt die Technologie zu faktisch in den Vordergrund, obwohl sie aus einer späteren internen Ausarbeitung stammt und nicht aus dem eigentlichen Stakeholderbedarf.

---

### 2) „Die Anforderungen nennen umfangreiche Funktionen (u.a. dynamische Erweiterbarkeit, Such- und Filterfunktionen)…“
- **Artefakt-Stelle:**  
  „Die Anforderungen nennen umfangreiche Funktionen (**u.a. dynamische Erweiterbarkeit, Such- und Filterfunktionen**)…“
- **Transkript-Beleg:**  
  Suchfunktion wird im Fachgespräch als Zusatz aufgenommen:  
  „… **sollte man nach einem Profil suchen können** …“  
  „… **das werden wir definitiv umsetzen** …“  
  Filteridee für Kommunikationsseiten ebenfalls später ergänzt.
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:**  
  Die dynamische Erweiterbarkeit ist breit abgestützt, die Suchfunktion ebenfalls stark angedeutet. Allerdings ist gerade die **konkrete Ausgestaltung** der Such-/Filterlogik im Transkript noch offen („man könnte…“, „Muster überlegen“, „eventuell…“). Das Artefakt formuliert dies als bereits fest umrissenen Anforderungsblock, statt die noch offene Ausgestaltung als Unsicherheit kenntlich zu machen.

---

### 3) Datenschutz als „noch nicht abschließend definiert“
- **Artefakt-Stelle:**  
  „Die Anforderungen weisen darauf hin, dass Datenschutzregelungen **noch nicht abschließend definiert** sind.“
- **Transkript-Beleg:**  
  „Was das Thema Datenschutz angeht, **werden wir nicht allzu tief ins Detail gehen**. Dieser Schritt wäre vermutlich ein zusätzliches Projekt.“  
  sowie mehrfach: Datenschutz ist schwierig/offen.
- **Fehlerart:** kein Fehler  
- **Begründung:**  
  Diese Aussage ist durch das Transkript gedeckt. **Kein Befund.**

---

### 4) Fehlendes Risiko: Rollen- und Rechtekonzept / differenzierter Zugriff
- **Artefakt-Stelle:**  
  Im Artefakt nur knapp: „Kontrollierte Nutzerkontenvergabe ohne öffentliche Registrierung“
- **Transkript-Beleg:**  
  Mehrfach relevant besprochen:
  - Admin / User / Bewohner-Account
  - Angehörige sollen Zugriff haben
  - Bewohner-Account nur auf eigenes Profil
  - Mitarbeiter sollen **nicht einrichtungsübergreifend** alle Profile sehen
- **Fehlerart:** MISSING_TOPIC
- **Begründung:**  
  Das Artefakt erwähnt zwar Kontenvergabe allgemein, aber das im Transkript sehr relevante Risiko **feingranularer Zugriffsrechte und Sichtbarkeitsbeschränkungen** fehlt als eigener Risikopunkt. Gerade bei sensiblen Bewohnerdaten ist das ein zentrales Risiko für dieses Artefakt.

---

### 5) Fehlendes Risiko: Offenheit bei einrichtungsbezogener Datenabgrenzung
- **Artefakt-Stelle:**  
  Kein eigener Risikopunkt vorhanden.
- **Transkript-Beleg:**  
  „… hat jeder Mitarbeiter in jeder Einrichtung Einsicht auf alle Profile … oder nur auf die in dem Haus…?“  
  Antwort: „… **so wird das gemacht**. Wir werden uns **einen Weg überlegen**, wie man das lösen könnte …“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:**  
  Die Frage, ob Profile **einrichtungsbezogen getrennt** werden, ist im Transkript ausdrücklich relevant und die Umsetzung noch offen. Das ist ein eigenständiges Datenschutz-/Berechtigungsrisiko und fehlt im Artefakt.

---

### 6) Fehlendes Risiko: Umgang mit Bild- und Videodaten / Einwilligungen
- **Artefakt-Stelle:**  
  Kein eigener Risikopunkt vorhanden.
- **Transkript-Beleg:**  
  „… mit den Angehörigen reden, ob es in Ordnung wäre, wenn wir **Bilder machen bzw. bekommen** …“  
  sowie Vorschläge zu **Videos** von Kommunikationssituationen.
- **Fehlerart:** MISSING_TOPIC
- **Begründung:**  
  Das Transkript macht klar, dass Bilder und Videos zentral für die Lösung sein könnten und deren Nutzung von Datenschutz/Einwilligung abhängt. Das Artefakt spricht Datenschutz nur allgemein an, aber das spezifische Risiko **Mediennutzung personenbezogener Daten** fehlt.

---

### 7) Fehlendes Risiko: Scope Creep durch Funktionsausweitung
- **Artefakt-Stelle:**  
  Nur indirekt enthalten unter „Balance zwischen Funktionsumfang und Benutzerfreundlichkeit“.
- **Transkript-Beleg:**  
  Im Verlauf werden viele Zusatzfunktionen diskutiert:  
  Dokumentation, Kalender, Medikamentenvergabe, Videos, No-Go-Seite, Suchfunktion, verschiedene Accounts, Popups, Einstellungen, Bewohner-Account etc.  
  Außerdem explizit:  
  „Je mehr sie leisten muss, desto schwerer wird es, desto länger dauert es…“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:**  
  Zwar wird Komplexität erwähnt, aber das sehr deutlich im Transkript angelegte Risiko einer **unkontrollierten Ausweitung des Umfangs** (Scope Creep) fehlt als klarer Projekt-/Umsetzungsrisikopunkt. Für eine Risikoanalyse ist das relevant.

---

### 8) Fehlendes Risiko: Akzeptanz und Nutzbarkeit digitaler Lösung bei analog geprägtem Arbeitsumfeld
- **Artefakt-Stelle:**  
  Nicht explizit vorhanden.
- **Transkript-Beleg:**  
  „… wir sind hier **sehr analog unterwegs** und haben noch nicht viel von Digitalisierung erlebt.“  
  Außerdem: klassische Akten werden teils nicht gelesen, Einarbeitung läuft stark erfahrungsbasiert.
- **Fehlerart:** MISSING_TOPIC
- **Begründung:**  
  Das Transkript zeigt ein relevantes Einführungsrisiko: selbst wenn die App fachlich sinnvoll ist, kann die Umstellung von analogen/erfahrungsbasierten Prozessen auf digitale Nutzung scheitern oder hohen Einführungsaufwand erzeugen. Das fehlt als eigenständiger Risikopunkt.

---

## Zusammenfassung

### Anzahl Befunde je Kategorie
- **FALSE_CLAIM:** 1
- **FALSE_CERTAINTY:** 1
- **MISSING_TOPIC:** 4

### Gesamteinschätzung
Das Artefakt ist **teilweise stimmig**, greift aber einige zentrale Risiken aus dem Transkript **nicht vollständig oder nicht präzise genug** auf. Besonders auffällig sind:
- eine zu konkrete Technologiefestlegung im Risikoteil,
- teilweise zu starke Verdichtung offener Punkte,
- und mehrere fehlende, transkriptnah wichtige Risiken rund um **Berechtigungen, Einrichtungstrennung, Medien-/Einwilligungsfragen, Scope Creep und Digitalisierungseinführung**.  

Insgesamt ist die Risikoanalyse brauchbar, aber **inhaltlich unvollständig** gegenüber der Ground Truth.