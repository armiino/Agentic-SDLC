# Verifikation LEDGER Interview (Lauf 8189ea) — Gold-Blatt (P1-Coverage) · v3.1

> **Prüfregeln (verbindlich, aus w2-matchregeln.md):**
> 1. Mehrere Claims dürfen GEMEINSAM eine Gold-Aussage decken; ein Claim darf mehrere Gold-Aussagen decken.
> 2. `partial` = Kern da, aber WESENTLICHE Qualifikation fehlt (Bedingung/Negation/Position/Zahlenwert/
>    Verpflichtungsgrad/Offenheitsstatus). 3. Falsche Negation/Gegenentscheidung = `none`.
> 4. Quellenrichtigkeit zählt hier NICHT — nur Inhalt (→ Claim-Blatt).
> **Suchraum:** NONE-Urteile = Suche im GESAMTEN Claim-Bestand; Gegenprobe: Anhang A.
> **Eskalation:** >10 % Fehlurteile → Full-Stichprobe verdoppeln.

---

### PFLICHT · G-008 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die App soll neuen Mitarbeitenden und anderen bislang unvertrauten Personen ermöglichen,
    Bewohner schneller zu verstehen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-011] Neue Mitarbeitende werden derzeit durch Dokumentationen und begleitete Einarbeitung
    unterstützt, bis gegenseitiges Verständnis und Vertrauen aufgebaut sind.
  BEGRÜNDUNG: KEIN Claim zum Nutzen für neue Mitarbeitende (CAN-011 beschreibt nur Status quo)
  - [ ] Urteil bestätigt   — **eher PARTIAL:** CAN-007 beschreibt, dass die App anderen Personen
    durch Nachschlagen beim Verstehen eines Bewohners helfen soll; CAN-011 stellt zusätzlich den
    Bezug zu neuen Mitarbeitenden her. Nicht ausdrücklich enthalten sind die vollständige
    Zielgruppe „neue Mitarbeitende und andere bislang unvertraute Personen“ sowie „schneller“.

---

### PFLICHT · G-009 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die App soll den Einarbeitungsaufwand für neue Mitarbeitende verringern.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur Verringerung des Einarbeitungsaufwands
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-011 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der About-Me-Bereich soll einen ersten persönlichen Eindruck des Bewohners vermitteln.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-012] Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation
    übernehmen soll; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender
    Kommunikation erhalten bleibt.
  BEGRÜNDUNG: KEIN Claim zum Zweck 'erster persönlicher Eindruck' (AU-0025 von CAN-012 konsumiert)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-020 [requirement] — mein Urteil: **NONE**

  GOLD:
    Mitarbeiter und Angehörige sollen als normale User-Accounts geführt werden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
  BEGRÜNDUNG: KEIN Claim ordnet Mitarbeiter+Angehörige der User-Rolle zu (CAN-019 nennt nur Rollentypen)
  - [ ] Urteil bestätigt   — **eher PARTIAL:** CAN-017 erfasst Angehörige mit Zugriff und
    Beitragsrechten, CAN-027 erfasst Mitarbeitende und CAN-019 die Rollen Admin/User. Die Zuordnung
    beider Gruppen zur normalen User-Rolle fehlt, der Rollen-/Gruppenkern ist aber vorhanden.

---

### PFLICHT · G-023 [requirement] — mein Urteil: **NONE**

  GOLD:
    Normale User-Accounts sollen hinzugefügte Daten nicht löschen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
    detaillierte Behandlung wäre ein separates Folgeprojekt.
    [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
    sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
  BEGRÜNDUNG: KEIN Claim zum Lösch-Verbot für normale User
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-037 [requirement] — mein Urteil: **NONE**

  GOLD:
    Im Kalender sollen zu einzelnen Tagen Einträge hinzugefügt werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-024] Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und
    Medikamentengaben bereitstellen.
  BEGRÜNDUNG: KEIN Claim zum Hinzufügen von Einträgen an einzelnen Tagen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-038 [requirement] — mein Urteil: **NONE**

  GOLD:
    Unter der Monatsansicht des Kalenders soll eine Liste mit Medikamenten angezeigt werden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-024] Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und
    Medikamentengaben bereitstellen.
  BEGRÜNDUNG: KEIN Claim zur Medikamentenliste unter der Monatsansicht
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-041 [requirement] — mein Urteil: **NONE**

  GOLD:
    Neue No-Go-Einträge sollen über einen Plus-Button dynamisch hinzugefügt werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-018] Die App soll eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart
    eines Bewohners unbedingt vermieden werden muss.
  BEGRÜNDUNG: KEIN Claim zum Plus-Button auf der No-Go-Seite (nur CAN-018 Existenz)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-043 [requirement] — mein Urteil: **NONE**

  GOLD:
    Admin-Accounts sollen hinzugefügte Daten löschen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
    detaillierte Behandlung wäre ein separates Folgeprojekt.
    [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
    sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
  BEGRÜNDUNG: KEIN Claim zum Löschrecht der Admins
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-061 [open_question] — mein Urteil: **NONE**

  GOLD:
    Ob der Hilfebereich zusätzlich erläutern soll, wie Informationen effektiv eingegeben und gesucht
    werden, bleibt offen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-044] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim
    ersten Login und als später erneut aufrufbaren Hilfebereich.
  BEGRÜNDUNG: KEIN Claim zur offenen Frage Eingabe-/Such-Erläuterung im Hilfebereich (CAN-044 nur Hilfe-Existenz)
  - [ ] Urteil bestätigt   — **eher PARTIAL:** CAN-044 deckt die Existenz eines Hilfebereichs bzw.
    Tutorials. Es fehlen jedoch der spezielle Inhalt „effektiv eingeben und suchen“ sowie dessen
    Offenheitsstatus. Das entspricht demselben Grenzfall wie beim F-Blatt.

---

### PFLICHT · G-063 [open_question] — mein Urteil: **NONE**

  GOLD:
    Die detaillierte Rechteverwaltung der vorgesehenen Account-Rollen ist noch festzulegen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
  BEGRÜNDUNG: KEIN Claim trägt die OFFENHEIT der detaillierten Rechteverwaltung (CAN-019/043 behaupten sie als gesetzt)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-073 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Login-Logo soll vorzugsweise zentriert im oberen Bildschirmbereich platziert werden.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur Logo-Platzierung (zentriert oben)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-074 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Login-Logo soll einen inhaltlichen Bezug zum Thema Kommunikation haben.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum Kommunikations-Bezug des Logos
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-075 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Login-Logo soll vorzugsweise rund gestaltet sein.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur runden Gestaltung
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-076 [open_question] — mein Urteil: **NONE**

  GOLD:
    Ob die Login-Eingabefelder einen leichten Schattenwurf erhalten, bleibt offen.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur offenen Schattenwurf-Frage
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-078 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Design der App soll intuitiv und benutzerfreundlich sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-045] Bei allen Screens soll auf Barrierefreiheit geachtet werden, insbesondere durch große
    Schrift, ausreichenden Kontrast und die Vermeidung zu vieler Farben.
  BEGRÜNDUNG: KEIN Claim zu intuitiv/benutzerfreundlich (CAN-045 = Barrierefreiheit, anderes Konstrukt)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-080 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der Login-Button soll optisch hervorstechen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-042] Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen
    und keine Registrierung anbieten.
  BEGRÜNDUNG: KEIN Claim zum optischen Hervorstechen des Login-Buttons
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-081 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die Appbar soll den Namen der aktuell angezeigten Seite mittig darstellen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-030] Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile
    als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile
    schnell gefunden werden können.
  BEGRÜNDUNG: KEIN Claim zum mittigen Seitennamen in der Appbar
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-088 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die Detailansicht eines ausgewählten Profils soll das Profilbild größer zeigen.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum größeren Profilbild in der Detailansicht — GENAU von Signal AU-0124 (missing_claim) gemeldet
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-090 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der verbale und der nonverbale Kommunikationsbereich sollen visuell durch Symbole voneinander
    unterschieden werden.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur visuellen Unterscheidung durch Symbole
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-091 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der No-Go-Screen soll ein starkes visuelles Warnsymbol verwenden; ein rotes Stoppschild wird als
    mögliche Ausprägung genannt.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum Warnsymbol/Stoppschild
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-092 [requirement] — mein Urteil: **NONE**

  GOLD:
    No-Go-Einträge sollen einfach zu überblicken beziehungsweise zu durchforsten sein.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur Überblickbarkeit der No-Gos
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-093 [requirement] — mein Urteil: **NONE**

  GOLD:
    No-Go-Einträge sollen sich auf die wichtigsten Informationen beschränken.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zur Beschränkung auf wichtigste Informationen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-094 [requirement] — mein Urteil: **NONE**

  GOLD:
    Ein Antippen eines Datums im Kalender soll Details zu Terminen oder Medikamentengaben öffnen.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum Datum-Antippen → Details
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-095 [requirement] — mein Urteil: **NONE**

  GOLD:
    Medikamentenerinnerungen sollen in der Kalenderansicht durch kleine Icons oder Tags an den
    betreffenden Tagen kenntlich gemacht werden.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zu Medikamenten-Icons/Tags
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-099 [requirement] — mein Urteil: **NONE**

  GOLD:
    Datenschutzeinstellungen sollen leicht zugänglich sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-044] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim
    ersten Login und als später erneut aufrufbaren Hilfebereich.
  BEGRÜNDUNG: KEIN Claim zu leicht zugänglichen Datenschutzeinstellungen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-108 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Design der App soll ein klares Branding aufweisen.
  LEDGER-KANDIDATEN: — KEINE — (Gesamtbestand durchsucht; Gegenprobe: Anhang A)
  BEGRÜNDUNG: KEIN Claim zum klaren Branding
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-006 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Gespeichertes Kommunikationswissen soll für Nutzer jederzeit und schnell nachschlagbar sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-006] Wenn priorisiert werden muss, soll die App primär dabei helfen, Bewohner besser zu
    verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen.
    [CAN-007] Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person
    festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein
    könnte.
    [CAN-012] Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation
    übernehmen soll; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender
    Kommunikation erhalten bleibt.
  BEGRÜNDUNG: CAN-007: Nachschlagen ✓, aber 'jederzeit und schnell' fehlt (Qualitäts-Qualifikation)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-014 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Nutzer sollen neue Kommunikationsweisen dynamisch über einen Plus-Button im unteren Bereich der
    Kommunikationsseiten hinzufügen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-023] Die Kommunikationsseite soll in verbale und nonverbale Kommunikation unterteilt sein
    und in beiden Bereichen das Hinzufügen neuer Kommunikationsweisen mit Texten oder Bildern
    ermöglichen.
  BEGRÜNDUNG: CAN-023: Hinzufügen ✓, aber Plus-Button + Position unten fehlen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-015 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der verbale und der nonverbale Bereich sollen über je einen eigenen Button erreichbar sein, der
    zur jeweiligen Unterseite navigiert.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-023] Die Kommunikationsseite soll in verbale und nonverbale Kommunikation unterteilt sein
    und in beiden Bereichen das Hinzufügen neuer Kommunikationsweisen mit Texten oder Bildern
    ermöglichen.
  BEGRÜNDUNG: CAN-023: Zwei Bereiche ✓, aber je eigener Navigations-Button fehlt
  - [ ] Urteil bestätigt   — **eher NONE:** CAN-023 erfasst lediglich die Unterteilung in verbal
    und nonverbal sowie das Hinzufügen von Inhalten. Eigene Navigations-Buttons und die Navigation
    auf jeweilige Unterseiten — der Kern dieser Gold-Aussage — fehlen vollständig. Dies entspricht
    der Einstufung desselben Gold-Falls im F-Blatt.

---

### PFLICHT · G-017 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Angehörige sollen bei neuen Bewohnern bereits vorab Daten in die App einpflegen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-017] Auch Angehörige sollen Zugriff auf die App erhalten und Wissen beziehungsweise Daten
    beitragen können.
  BEGRÜNDUNG: CAN-017: Beitragen ✓, aber 'vorab bei neuen Bewohnern' fehlt (Bedingung)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-021 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Normale User-Accounts sollen Inhalte hinzufügen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
    detaillierte Behandlung wäre ein separates Folgeprojekt.
    [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
    sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
    [CAN-013] Kommunikationswissen in der App muss dynamisch erweitert werden können; neue
    Erfahrungen sollen hinzugefügt werden können.
  BEGRÜNDUNG: CAN-013: Hinzufügen-Fähigkeit ✓, aber Rollen-Bindung an normale User fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-025 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Normale User-Accounts sollen keine neuen Accounts erstellen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
    detaillierte Behandlung wäre ein separates Folgeprojekt.
    [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
    sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
  BEGRÜNDUNG: CAN-019: Admin-Anlage + keine Selbstregistrierung ✓, aber Exklusivität (User KÖNNEN NICHT anlegen) nicht ausgesagt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-028 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Ein Profil der Profilübersicht soll auswählbar sein und zur Detailansicht des gewählten Profils
    führen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-020] Nach dem Login soll die App auf eine Profilübersicht mit anklickbarer Liste aller
    sichtbaren Profile führen.
  BEGRÜNDUNG: CAN-020: anklickbar ✓, aber Ziel Detailansicht fehlt — GENAU das meldet Signal AU-0124 (missing_claim)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-030 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der About-Me-Bereich soll ganz oben ein Informationsfeld mit Angaben wie Name, Alter und Hobbys
    enthalten.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-022] Die About-Me-Seite soll Personendaten sowie eine erweiterbare Fotoliste mit
    Beschreibungen enthalten; neue Bilder sollen per Plus-Button hinzugefügt und die neuesten oben
    angezeigt werden.
  BEGRÜNDUNG: CAN-022: 'Personendaten' ✓, aber Position oben + Informationsfeld fehlen (wie F-Seite)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-031 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die About-Me-Foto-Timeline soll einem Social-Media-Feed ähneln: Neu hinzugefügte Bilder
    erscheinen oben, ältere Bilder bleiben darunter scrollbar.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-022] Die About-Me-Seite soll Personendaten sowie eine erweiterbare Fotoliste mit
    Beschreibungen enthalten; neue Bilder sollen per Plus-Button hinzugefügt und die neuesten oben
    angezeigt werden.
  BEGRÜNDUNG: CAN-022: 'neueste oben' ✓, aber Feed-Analogie + 'ältere darunter scrollbar' fehlen (Zweitprüfer-Präzedenz F-Seite)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-032 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Im About-Me-Bereich sollen neue Bilder dynamisch über einen Plus-Button am unteren
    Bildschirmrand und einen Eingabedialog hinzugefügt werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-022] Die About-Me-Seite soll Personendaten sowie eine erweiterbare Fotoliste mit
    Beschreibungen enthalten; neue Bilder sollen per Plus-Button hinzugefügt und die neuesten oben
    angezeigt werden.
  BEGRÜNDUNG: CAN-022: Plus-Button ✓, aber Position unten + Eingabedialog fehlen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-035 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Kalender soll eine übersichtliche Monatsansicht bieten.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-024] Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und
    Medikamentengaben bereitstellen.
  BEGRÜNDUNG: CAN-024: übersichtlich ✓, aber Monatsansicht fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-048 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Auf der Profilübersicht soll über eine Suchleiste nach dem Namen oder anderen relevanten
    Profildaten gesucht werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-030] Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile
    als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile
    schnell gefunden werden können.
    [CAN-032] Auch auf Kommunikationsseiten soll eine Suchfunktion verfügbar sein; dafür muss ein
    einheitliches Beschreibungsmuster für Kommunikationsbeschreibungen definiert werden, damit
    Einträge gezielt gefunden werden können.
  BEGRÜNDUNG: CAN-030: Suchleiste ✓, aber Suchkriterien Name/Profildaten fehlen (Zweitprüfer-Präzedenz F-Seite G-048)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-049 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Auf den Kommunikationsseiten soll über eine Suchleiste oben im Bildschirm nach
    Kommunikationsweisen beziehungsweise Merkmalen wie einem Körperteil gesucht oder gefiltert
    werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-030] Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile
    als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile
    schnell gefunden werden können.
    [CAN-032] Auch auf Kommunikationsseiten soll eine Suchfunktion verfügbar sein; dafür muss ein
    einheitliches Beschreibungsmuster für Kommunikationsbeschreibungen definiert werden, damit
    Einträge gezielt gefunden werden können.
    [CAN-033] Die Videofunktionalität soll nicht als eigener Screen bestehen, sondern in die
    Kommunikationsseiten für verbale und nonverbale Kommunikation integriert werden.
  BEGRÜNDUNG: CAN-032: Suchfunktion ✓, aber Position oben + Merkmals-Beispiele fehlen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-054 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Neu hinzugefügte Erfahrungen sollen unmittelbar über die App mit anderen berechtigten Nutzern
    geteilt werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-013] Kommunikationswissen in der App muss dynamisch erweitert werden können; neue
    Erfahrungen sollen hinzugefügt werden können.
    [CAN-034] Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer
    derselben Einrichtung eine Popup-Benachrichtigung erhalten.
  BEGRÜNDUNG: CAN-013+CAN-034 gebündelt: Hinzufügen + About-Me-Benachrichtigung ✓, aber unmittelbares Teilen ALLER Erfahrungen mit Berechtigten fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-066 [architecture] — mein Urteil: **PARTIAL**

  GOLD:
    Im Entwicklungsteam sollen für Flutter und Dart einheitliche Versionen installiert werden; im
    Gespräch werden die Versionsnummern 3.13.9 und 3.1.5 genannt.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-037] Das Team soll möglichst dieselbe Entwicklungsumgebung verwenden, konkret Android
    Studio, und einheitliche Flutter- und Dart-Versionen installieren, um Entwicklungsprobleme zu
    vermeiden.
  BEGRÜNDUNG: CAN-037: einheitliche Versionen ✓, aber Versionsnummern 3.13.9/3.1.5 fehlen (Zahlenwert)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-069 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Ob eine NoSQLite-Datenbank zur lokalen Speicherung oder Zwischenspeicherung von Firebase-Daten
    eingesetzt wird, bleibt offen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-038] Für die Datenhaltung wird zunächst Firebase Firestore verwendet; es muss noch geklärt
    werden, wie Cloud-Daten lokal gespeichert und wiederverwendet werden sollen.
  BEGRÜNDUNG: CAN-038: Klärung offen ✓, aber die konkrete NoSQLite-Option fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-072 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Login-Screen soll ein deutlich sichtbares Logo enthalten.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-042] Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen
    und keine Registrierung anbieten.
  BEGRÜNDUNG: CAN-042: Logo ✓, aber 'deutlich sichtbar' fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-077 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Unter dem Logo sollen Felder für E-Mail und Passwort angeordnet sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-042] Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen
    und keine Registrierung anbieten.
  BEGRÜNDUNG: CAN-042: Felder ✓, aber Anordnung unter dem Logo fehlt (Position)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-079 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Login-Button soll unter den Eingabefeldern angeordnet sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-042] Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen
    und keine Registrierung anbieten.
  BEGRÜNDUNG: CAN-042: Button ✓, aber Position unter den Feldern fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-082 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die Appbar soll links eine Zurück-Navigation zur vorherigen Seite bereitstellen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-025] Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout
    vorhanden sein; zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden.
  BEGRÜNDUNG: CAN-025: Rücknavigation ✓, aber Position links fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-083 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die Appbar soll rechts ein Einstellungssymbol enthalten, das zur Einstellungsseite führt.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-025] Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout
    vorhanden sein; zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden.
  BEGRÜNDUNG: CAN-025: Einstellungsfunktionen als offen deklariert, aber Einstellungssymbol rechts → Einstellungsseite nicht ausgesagt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-086 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Ob die Profile in der Profilübersicht als Liste oder als Kacheln dargestellt werden, bleibt
    offen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-030] Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile
    als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile
    schnell gefunden werden können.
  BEGRÜNDUNG: CAN-030: 'Liste oder Kacheln' als Optionen ✓, aber der OFFENHEITS-Status der Frage fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-087 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Neue Profile sollen über ein Plus-Symbol am unteren Bildschirmrand und einen Dialog mit Bild,
    Name und Beschreibung angelegt werden können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-031] Auf der Profilübersicht soll es eine Funktion zum Anlegen neuer Profile mit Bild, Name
    und Beschreibung geben.
  BEGRÜNDUNG: CAN-031: Anlegen mit Bild/Name/Beschreibung ✓, aber Plus-Symbol unten + Dialog fehlen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-089 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die Detailansicht eines Profils soll die vier Hauptbereiche der App als interaktive Buttons
    darstellen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-021] Jedes Bewohnerprofil soll mindestens die Bereiche About Me, Kommunikation und Kalender
    enthalten.
  BEGRÜNDUNG: CAN-021: Bereiche existieren ✓ (aber nur drei genannt), interaktive Buttons in der Detailansicht fehlen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### PFLICHT · G-105 [decision] — mein Urteil: **PARTIAL**

  GOLD:
    Animationen beziehungsweise zusätzliches Feedback sollen nicht in die App aufgenommen werden.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-046] Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer
    sollen geprüft werden.
    [CAN-047] Ablenkende Animationen sollen nicht eingebaut werden.
  BEGRÜNDUNG: CAN-047: Verbots-Richtung ✓ (anders als F!), aber nur 'ablenkende' statt aller Animationen — Reichweite der Negation fehlt
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

## Signal-Zuordnungs-Checks (PFLICHT)

- [x] Detektions-Regel: Signal-AU ∩ Gold-Quell-AUs (deterministisch, skriptgeprüft) + 1 semantische Zuordnung G-028←AU-0124 ('Anforderung an die Profile-Detailansicht') — semantische Zuordnung korrekt?
- [ ] Stille Misses (16): G-011, G-020, G-021, G-023, G-025, G-037, G-038, G-043, G-066, G-069, G-079, G-080, G-086, G-087, G-099, G-105 — für KEINEN dieser Einträge existiert ein Signal auf seinen Quell-Units. Stichprobe: 3 davon gegen die Signal-Liste im Capture prüfen. — **Nicht unabhängig prüfbar:** In den vier bereitgestellten Blättern ist die vollständige Capture-Signalliste nicht enthalten. Für die verlangte Drei-Fälle-Gegenprobe wird diese Liste beziehungsweise das Capture-Artefakt benötigt.
- [x] 'already_covered_indirectly'-Vorsicht: AU-0084→G-048, AU-0086→G-061, AU-0125→G-030/31/32, AU-0136→G-078/108 behaupten Deckung, Urteil ist partial/none — Signal als 'hinschauen', nicht 'gedeckt' gewertet. Einverstanden?

## Full-Stichprobe (Seed-Schichtung 15 % je Typ → 9 von 56)

---

### STICHPROBE · G-046 [architecture] — mein Urteil: **FULL**

  GOLD:
    Die App soll auf iOS- und Android-Smartphones lauffähig sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-028] Die App soll möglichst plattformübergreifend auf iPhone und Android laufen.
  BEGRÜNDUNG: CAN-028: iPhone+Android ('möglichst' = quellennäherer Verpflichtungsgrad, gleicher Gehalt)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-003 [decision] — mein Urteil: **FULL**

  GOLD:
    Die Unterstützung soll primär anderen Personen helfen, Bewohner besser zu verstehen, statt
    Bewohner beim Verstehen der Betreuer zu unterstützen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-006] Wenn priorisiert werden muss, soll die App primär dabei helfen, Bewohner besser zu
    verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen.
    [CAN-007] Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person
    festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein
    könnte.
  BEGRÜNDUNG: CAN-006: primär Bewohner-Verstehen priorisiert
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-068 [open_question] — mein Urteil: **FULL**

  GOLD:
    Wie Daten aus Firebase lokal auf dem Gerät gespeichert beziehungsweise zwischengespeichert
    werden, ist noch nicht abschließend geklärt.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-038] Für die Datenhaltung wird zunächst Firebase Firestore verwendet; es muss noch geklärt
    werden, wie Cloud-Daten lokal gespeichert und wiederverwendet werden sollen.
  BEGRÜNDUNG: CAN-038: lokale Speicherung muss geklärt werden
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-044 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App ist für den internen Gebrauch der Einrichtung vorgesehen und soll nicht beliebigen
    externen Personen offenstehen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
    unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während
    es keine Selbstregistrierung für beliebige Nutzer geben darf.
    [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
    detaillierte Behandlung wäre ein separates Folgeprojekt.
    [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
    sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
  BEGRÜNDUNG: CAN-027: interner Gebrauch
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-012 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App soll ihren Schwerpunkt auf unterstützende Kommunikation behalten und nicht durch einen
    zu großen Funktionsumfang davon abweichen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-012] Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation
    übernehmen soll; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender
    Kommunikation erhalten bleibt.
  BEGRÜNDUNG: CAN-012: 'Fokus auf unterstützender Kommunikation erhalten'
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-057 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Bewohner-Account darf in der Profilübersicht nur das eigene Profil sehen.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-035] Es soll einen Bewohner-Account mit speziellen, stark begrenzten Rechten geben, der nur
    das eigene Profil sehen sowie auf About Me zugreifen und dort eigene Bilder hinzufügen kann, um
    Fehlbedienungen zu minimieren.
  BEGRÜNDUNG: CAN-035: nur eigenes Profil sehen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-007 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App soll die Kommunikation zwischen einer beeinträchtigten Person und anderen Personen
    unterstützen, ermöglichen oder verbessern.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-004] Es soll eine Anforderungsanalyse mit beteiligten Personen und künftigen Nutzern
    durchgeführt werden, um Bedürfnisse und Funktionen des Systems genauer zu erarbeiten.
    [CAN-007] Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person
    festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein
    könnte.
    [CAN-001] Die Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen
    und anderen Personen verbessern und fördern.
  BEGRÜNDUNG: CAN-001
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-060 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Bewohner-Account soll im eigenen About-Me-Bereich eigene Bilder hinzufügen können.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-035] Es soll einen Bewohner-Account mit speziellen, stark begrenzten Rechten geben, der nur
    das eigene Profil sehen sowie auf About Me zugreifen und dort eigene Bilder hinzufügen kann, um
    Fehlbedienungen zu minimieren.
  BEGRÜNDUNG: CAN-035: eigene Bilder hinzufügen
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

### STICHPROBE · G-040 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die Ausloggen-Funktion soll schnell erreichbar sein.
  LEDGER-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [CAN-025] Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout
    vorhanden sein; zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden.
    [CAN-044] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim
    ersten Login und als später erneut aufrufbaren Hilfebereich.
  BEGRÜNDUNG: CAN-025: Logout in der Appbar jeder Seite = schnell erreichbar (Entailment)
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → Matchlog)

---

## Anhang A — kompletter Ledger-Claim-Bestand

- [CAN-001] Die Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und
- anderen Personen verbessern und fördern.
- [CAN-002] Eine digitale Lösung, etwa als Computerprogramm oder ähnliche Software, wird bevorzugt.
- [CAN-003] Die Lösung darf kein System sein, das individuelle Sprache direkt zwischen Bewohnern und
- anderen Personen automatisch übersetzt.
- [CAN-004] Es soll eine Anforderungsanalyse mit beteiligten Personen und künftigen Nutzern
- durchgeführt werden, um Bedürfnisse und Funktionen des Systems genauer zu erarbeiten.
- [CAN-005] Das Team soll Kennenlern- und Beobachtungstermine in einer oder mehreren Einrichtungen
- durchführen können, um die aktuelle Kommunikationssituation besser zu verstehen.
- [CAN-006] Wenn priorisiert werden muss, soll die App primär dabei helfen, Bewohner besser zu
- verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen.
- [CAN-007] Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person
- festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein könnte.
- [CAN-008] Es existieren klassische Akten, in denen Informationen über Bewohner dokumentiert sind.
- [CAN-009] Bestehende Dokumentationen können für Mitarbeitende schwer nutzbar sein, weil sie
- umfangreich sind und gesuchte Informationen oft nicht schnell gefunden werden.
- [CAN-010] Es muss geklärt werden, ob und unter welchen Datenschutzbedingungen Einsicht in
- Bewohnerakten möglich ist.
- [CAN-011] Neue Mitarbeitende werden derzeit durch Dokumentationen und begleitete Einarbeitung
- unterstützt, bis gegenseitiges Verständnis und Vertrauen aufgebaut sind.
- [CAN-012] Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation übernehmen
- soll; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten
- bleibt.
- [CAN-013] Kommunikationswissen in der App muss dynamisch erweitert werden können; neue Erfahrungen
- sollen hinzugefügt werden können.
- [CAN-014] Die App soll Kommunikationsinhalte nicht nur als Text, sondern auch in anderen
- Darstellungsformen wie Bildern visualisieren.
- [CAN-015] Die Datenschutzlage und die Zustimmung der Angehörigen zur Nutzung von Bildern für
- Testzwecke in der App müssen geklärt werden.
- [CAN-016] Die App soll Videos zu Kommunikationssituationen mit Beschreibung erfassen und
- bereitstellen können.
- [CAN-017] Auch Angehörige sollen Zugriff auf die App erhalten und Wissen beziehungsweise Daten
- beitragen können.
- [CAN-018] Die App soll eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart
- eines Bewohners unbedingt vermieden werden muss.
- [CAN-019] Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen
- unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während es
- keine Selbstregistrierung für beliebige Nutzer geben darf.
- [CAN-020] Nach dem Login soll die App auf eine Profilübersicht mit anklickbarer Liste aller
- sichtbaren Profile führen.
- [CAN-021] Jedes Bewohnerprofil soll mindestens die Bereiche About Me, Kommunikation und Kalender
- enthalten.
- [CAN-022] Die About-Me-Seite soll Personendaten sowie eine erweiterbare Fotoliste mit
- Beschreibungen enthalten; neue Bilder sollen per Plus-Button hinzugefügt und die neuesten oben
- angezeigt werden.
- [CAN-023] Die Kommunikationsseite soll in verbale und nonverbale Kommunikation unterteilt sein und
- in beiden Bereichen das Hinzufügen neuer Kommunikationsweisen mit Texten oder Bildern ermöglichen.
- [CAN-024] Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und
- Medikamentengaben bereitstellen.
- [CAN-025] Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout vorhanden
- sein; zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden.
- [CAN-026] Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine
- detaillierte Behandlung wäre ein separates Folgeprojekt.
- [CAN-027] Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende
- sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
- [CAN-028] Die App soll möglichst plattformübergreifend auf iPhone und Android laufen.
- [CAN-029] Es muss evaluiert werden, ob die App auch auf Tablets lauffähig und sinnvoll nutzbar
- ist.
- [CAN-030] Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile
- als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile schnell
- gefunden werden können.
- [CAN-031] Auf der Profilübersicht soll es eine Funktion zum Anlegen neuer Profile mit Bild, Name
- und Beschreibung geben.
- [CAN-032] Auch auf Kommunikationsseiten soll eine Suchfunktion verfügbar sein; dafür muss ein
- einheitliches Beschreibungsmuster für Kommunikationsbeschreibungen definiert werden, damit
- Einträge gezielt gefunden werden können.
- [CAN-033] Die Videofunktionalität soll nicht als eigener Screen bestehen, sondern in die
- Kommunikationsseiten für verbale und nonverbale Kommunikation integriert werden.
- [CAN-034] Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer
- derselben Einrichtung eine Popup-Benachrichtigung erhalten.
- [CAN-035] Es soll einen Bewohner-Account mit speziellen, stark begrenzten Rechten geben, der nur
- das eigene Profil sehen sowie auf About Me zugreifen und dort eigene Bilder hinzufügen kann, um
- Fehlbedienungen zu minimieren.
- [CAN-036] Für die App-Entwicklung sollen Flutter und Dart verwendet werden.
- [CAN-037] Das Team soll möglichst dieselbe Entwicklungsumgebung verwenden, konkret Android Studio,
- und einheitliche Flutter- und Dart-Versionen installieren, um Entwicklungsprobleme zu vermeiden.
- [CAN-038] Für die Datenhaltung wird zunächst Firebase Firestore verwendet; es muss noch geklärt
- werden, wie Cloud-Daten lokal gespeichert und wiederverwendet werden sollen.
- [CAN-039] Für die erste Entwicklung und Tests soll Android 11 als gemeinsame Zielversion verwendet
- werden.
- [CAN-040] Im nächsten Teamtreffen müssen Regeln für das Programmieren, etwa Kommentarregeln,
- festgelegt werden.
- [CAN-041] Das JetX-Package soll im Projekt ausprobiert und als zusätzliche technische
- Rahmenbedingung erwogen werden.
- [CAN-042] Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen und
- keine Registrierung anbieten.
- [CAN-043] Die Einstellungsseite soll rollenabhängig differenziert sein; Admins sollen dort
- Nutzeraccounts hinzufügen und Rechte anpassen können, während alle Nutzer ihr Profil ändern und
- etwa die Sprache wählen können.
- [CAN-044] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim
- ersten Login und als später erneut aufrufbaren Hilfebereich.
- [CAN-045] Bei allen Screens soll auf Barrierefreiheit geachtet werden, insbesondere durch große
- Schrift, ausreichenden Kontrast und die Vermeidung zu vieler Farben.
- [CAN-046] Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer sollen
- geprüft werden.
- [CAN-047] Ablenkende Animationen sollen nicht eingebaut werden.
