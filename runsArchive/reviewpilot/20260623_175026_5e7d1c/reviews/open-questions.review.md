Hier ist die Prüfung des Artefakts **open-questions.md** gegen das Stakeholder-Transkript.

## Befunde

### 1) „Welche spezifischen Anforderungen bestehen für die unterschiedlichen Nutzerrollen (Admin, Betreuer, Bewohner) hinsichtlich Bedienbarkeit und Rechte?“
- **Artefakt-Stelle:** Fachliche Fragen, Punkt 3
- **Transkript-Beleg:** Nutzerrollen wurden bereits konkret benannt und teilweise geklärt:
  - „Es gibt also den Power-Nutzer-Admin und den Nutzer-User.“  
  - „Wenn Sie das möchten, dann machen wir das so, dass es verschiedene Accounts gibt. Also Mitarbeiter und Angehörige, die vermutlich auch verschiedene Rechte haben werden.“
  - „Dieser Bewohner-Account sollte aber spezielle Rechte haben und auch nur sein eigenes Profil … sehen.“
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Die Formulierung stellt das Thema als generell offene Grundsatzfrage dar, obwohl Rollen und Rechte im Transkript schon in wesentlichen Teilen festgelegt wurden. Offen sind eher Details der Rechteverwaltung, nicht die Existenz bzw. Grundidee der Rollen selbst.

---

### 2) „Wer sind die fachlichen Ansprechpartner, insbesondere im Bereich unterstützende Kommunikation, für weitere Detailklärungen?“
- **Artefakt-Stelle:** Fachliche Fragen, Punkt 4
- **Transkript-Beleg:** Ansprechpartner sind bereits benannt bzw. zugesagt:
  - Gesamtleiter der Einrichtung
  - „Im nächsten Termin werde ich dafür sorgen, dass ein Heilerziehungspfleger mit dabei ist“
  - Interview mit Fachpersonal/Heilerziehungspflegerin fand bereits statt
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Das Artefakt behandelt die Ansprechpartner wie ungeklärt, obwohl im Transkript schon klar ist, dass Gesamtleiter, Heilerziehungspfleger/Fachpersonal und teils auch Angehörige relevante Ansprechpartner sind.

---

### 3) „Wie wird die dynamische Erweiterbarkeit technisch realisiert? Gibt es bereits Konzepte für modulare Architekturen oder Plugin-Systeme?“
- **Artefakt-Stelle:** Technische Fragen, Punkt 1
- **Transkript-Beleg:** Dynamische Erweiterbarkeit wird über Plus-Button/Dialoge zum Hinzufügen von Inhalten diskutiert; **Plugin-Systeme oder modulare Architekturen** werden nirgends erwähnt.
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** Der Verweis auf „modulare Architekturen oder Plugin-Systeme“ ist im Transkript nicht belegt und führt neue, erfundene technische Konzepte ein.

---

### 4) „Welche Backend-Architektur wird für Speicherung, Authentifizierung und Rechteverwaltung angestrebt oder empfohlen?“
- **Artefakt-Stelle:** Technische Fragen, Punkt 3
- **Transkript-Beleg:** Technische Umsetzungsansätze wurden bereits konkret diskutiert:
  - Flutter/Dart
  - Firebase Firestore
  - Login nur mit zugewiesenem Account
  - Admin legt Accounts an und verwaltet Rechte
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Die Frage ist zu allgemein-offen formuliert, obwohl im Transkript bereits klare technische Richtungen für Speicherung und Authentifizierung genannt werden. Offen sind höchstens Detailentscheidungen, nicht die gesamte Architekturfrage.

---

### 5) „Datenschutzanforderungen sind noch offen und können die Architektur und Umsetzung wesentlich beeinflussen.“
- **Artefakt-Stelle:** Widersprüche und Unklarheiten, Punkt 1
- **Transkript-Beleg:** Datenschutz ist tatsächlich offen; zugleich wurden aber bereits konkrete Schutzmaßnahmen festgelegt bzw. vorgeschlagen:
  - keine öffentliche Registrierung
  - Accounts nur zugewiesen
  - Admin verwaltet Rechte
  - Einrichtungsbezogene Sichtbarkeit wurde entschieden
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Der Punkt ist teilweise richtig, aber zu pauschal. Das Artefakt lässt offen erscheinen, was im Transkript schon konkret beschlossen wurde. Datenschutz insgesamt ist offen, aber nicht völlig ungeklärt.

---

### 6) „Unklare Details zur Rechteverwaltung und kontrollierten Nutzerkontenvergabe ohne öffentliche Registrierung.“
- **Artefakt-Stelle:** Widersprüche und Unklarheiten, Punkt 3
- **Transkript-Beleg:** 
  - „man sich nicht registrieren können soll“
  - „nur ein Account mit Admin-Rechten weitere Accounts erstellen kann und die Rechte verwaltet“
  - Rollen: Admin, User, Bewohner; Angehörige/Mitarbeiter als User-Kontext
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** „ohne öffentliche Registrierung“ ist nicht unklar, sondern bereits entschieden. Unklar sind eher Feindetails der Rechteverwaltung.

---

### 7) „Detaillierte Definition und Abgrenzung der Nutzerrollen und deren Berechtigungen.“
- **Artefakt-Stelle:** Fehlende Informationen, Punkt 3
- **Transkript-Beleg:** Rollen und einige Berechtigungen sind vorhanden:
  - Admin: Accounts anlegen/verwalten, löschen
  - User: Inhalte hinzufügen, nicht löschen, keine Accounts erstellen
  - Bewohner: nur eigenes Profil sehen, eingeschränkte Funktionen
- **Fehlerart:** FALSE_CERTAINTY
- **Begründung:** Als komplett „fehlende Information“ ist das überzogen. Es fehlen Details, aber eine grundlegende Definition und Abgrenzung ist im Transkript bereits vorhanden.

---

### 8) „Datenschutzbeauftragter für Datenschutz- und Compliancefragen.“
- **Artefakt-Stelle:** Ansprechpartner und Rollen, Punkt 1
- **Transkript-Beleg:** **nicht vorhanden**
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** Ein Datenschutzbeauftragter wird im Transkript nicht genannt.

---

### 9) „Technische Architekten und Entwickler für Architektur und Machbarkeit.“
- **Artefakt-Stelle:** Ansprechpartner und Rollen, Punkt 3
- **Transkript-Beleg:** Im Transkript treten Studierende/Projektteam auf, aber keine „technischen Architekten“ als benannte Ansprechpartner.
- **Fehlerart:** FALSE_CLAIM
- **Begründung:** „Technische Architekten“ ist eine erfundene Rolle, die im Transkript nicht vorkommt.

---

### 10) Fehlendes offenes Thema: einrichtungsbezogene Zugriffsabgrenzung
- **Artefakt-Stelle:** fehlt komplett
- **Transkript-Beleg:** 
  - „hat jeder Mitarbeiter in jeder Einrichtung Einsicht auf alle Profile … oder nur auf die in dem Haus, in dem er tätig ist?“
  - „Aufgrund von Datenschutz sollte es so geregelt sein, dass es nicht übergreifend ist.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Für ein Open-Questions-/Klärungsartefakt relevant ist die Frage der Zugriffsabgrenzung zwischen Einrichtungen. Auch wenn eine Richtung beschlossen wurde, bleibt die konkrete Umsetzung ein klärungsrelevantes Thema und fehlt hier vollständig.

---

### 11) Fehlendes offenes Thema: Tablet-Nutzung/Umsetzbarkeit
- **Artefakt-Stelle:** fehlt komplett
- **Transkript-Beleg:** 
  - „Wird es auch auf einem Tablet laufen?“
  - „Auch das nehmen wir zu den Anforderungen auf und evaluieren, ob das umsetzbar sein wird oder nicht.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Die Nutzbarkeit auf Tablets ist explizit offen und relevant, taucht im Artefakt aber nicht als offene Frage auf.

---

### 12) Fehlendes offenes Thema: Suchfunktion und Beschreibungsschema
- **Artefakt-Stelle:** fehlt komplett
- **Transkript-Beleg:** 
  - Suchleiste für Profile
  - Suche in Kommunikationsseiten
  - offenes Muster zur Formulierung von Beschreibungen („zuerst die Art, wie das Körperteil und dann die Beschreibung“ / „Eventuell könnten Sie sich eine genauere Art der Formulierung überlegen“)
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Das standardisierte Erfassungsschema für Kommunikationsweisen ist ein klarer Klärungsbedarf aus dem Transkript und für dieses Artefakt sehr relevant.

---

### 13) Fehlendes offenes Thema: Zusammenführung von Video- und Kommunikationsscreen
- **Artefakt-Stelle:** fehlt komplett
- **Transkript-Beleg:** 
  - „ich denke gerade auch, dass es vielleicht mehr Sinn machen würde, einfach den Videoscreen und die Kommunikationsscreens zusammenzufügen“
  - „Wir sollten die Videofunktionalitäten in die Kommunikationsseiten einbauen.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Das ist eine relevante fachlich-strukturelle Klärung zur App-Struktur. Im Artefakt fehlt dieser Punkt vollständig.

---

### 14) Fehlendes offenes Thema: Benachrichtigungen bei neuen About-Me-Inhalten
- **Artefakt-Stelle:** fehlt komplett
- **Transkript-Beleg:** 
  - „Jedes Mal, wenn etwas Neues im About Me Bildschirm hochgeladen wird, sollen alle User … eine Popup-Nachricht erhalten.“
- **Fehlerart:** MISSING_TOPIC
- **Begründung:** Dieses Thema wurde besprochen und ist für offene Fragen/weiteren Klärungsbedarf relevant, erscheint aber im Artefakt nicht.

## Zusammenfassung

- **FALSE_CLAIM:** 3
- **FALSE_CERTAINTY:** 6
- **MISSING_TOPIC:** 5

**Gesamteinschätzung:**  
Das Artefakt greift einige echte offene Themen auf, insbesondere Datenschutz und generelle Nutzungsfragen. Es ist jedoch in mehreren Punkten zu pauschal oder verschiebt bereits diskutierte/teilweise entschiedene Inhalte wieder in den Status „offen“. Zusätzlich enthält es einige nicht belegte Annahmen (z. B. Plugin-Systeme, Datenschutzbeauftragter, technische Architekten) und lässt mehrere tatsächlich relevante Klärungspunkte aus dem Transkript weg. Insgesamt daher **inhaltlich nur teilweise transkripttreu**.