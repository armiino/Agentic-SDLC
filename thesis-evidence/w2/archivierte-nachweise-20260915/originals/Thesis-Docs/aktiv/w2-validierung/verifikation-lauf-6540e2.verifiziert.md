# Verifikation Interview (Lauf 6540e2) — Gold-Blatt (P1-Coverage) · v2 gehärtet

> Verifiziert am 06.09.2026 anhand des Regelkopfs, des vollständigen F-Bestands in Anhang A
> und des eingefrorenen Gold-Referenzbestands. Nicht bestätigte Urteile bleiben unmarkiert und
> tragen eine Korrekturanmerkung.

> **Prüfregeln (verbindlich, aus w2-matchregeln.md):**
> 1. Mehrere F-Aussagen dürfen GEMEINSAM eine Gold-Aussage decken; ein F-Claim darf mehrere Gold-Aussagen decken.
> 2. `partial` = der Kern ist da, aber eine WESENTLICHE Qualifikation fehlt: Bedingung, Negation, Position,
>    Zahlenwert, Verpflichtungsgrad oder Offenheitsstatus.
> 3. Falsche Negation oder ENTGEGENGESETZTE Entscheidung = `none` (kein Match, egal wie ähnlich der Wortlaut).
> 4. Quellenrichtigkeit zählt hier NICHT — nur der Inhalt. (Quellen werden im Claim-Blatt geprüft.)
>
> **Suchraum:** Die NONE-Urteile beruhen auf einer Suche im GESAMTEN F-Bestand (nicht nur der Quell-Region).
> Der Block zeigt Region + genannte Kandidaten; bei Zweifel: Anhang A (kompletter F-Bestand) durchsehen.
>
> **Eskalation:** Sind >10 % deiner geprüften Urteile falsch, wird die Full-Stichprobe verdoppelt.

---

### PFLICHT · G-015 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der verbale und der nonverbale Bereich sollen über je einen eigenen Button erreichbar sein, der
    zur jeweiligen Unterseite navigiert.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
    [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
    [C-034] Die Kommunikationsfunktion soll in verbal und nonverbal unterteilt sein.
    [C-036] Die Kommunikationsfunktion soll dynamisch wachsen, sodass neue Erfahrungen hinzugefügt
    werden können.
    [C-037] Für Kommunikationsinhalte sollen nicht nur Texte, sondern auch visuelle Darstellungen
    verwendet werden.
    [C-039] Die App soll Kommunikationssituationen auch per Video mit Beschreibung festhalten
    können.
    [C-040] Videoeinträge sollen insbesondere helfen, unbekannte Verhaltensweisen oder
    Kommunikationsarten wiederzuerkennen.
    [C-041] Zunächst wurde eine eigenständige Videoseite als Teil der App-Struktur vorgesehen.
    [C-104] Die Kommunikationsseite soll verbale und nonverbale Kommunikation visuell getrennt und
    mit Symbolen kennzeichnen.
  BEGRÜNDUNG: Buttons je Bereich→Unterseite: kein Claim (C-034 nur Trennung, C-104 nur Symbole)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-038 [requirement] — mein Urteil: **NONE**

  GOLD:
    Unter der Monatsansicht des Kalenders soll eine Liste mit Medikamenten angezeigt werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-058] Die App soll einen Kalenderbereich enthalten.
    [C-059] Der Kalender soll Termine enthalten können.
    [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
    [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
    eingesehen werden können.
  BEGRÜNDUNG: Medikamenten-Liste unter Monatsansicht: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-042 [decision] — mein Urteil: **NONE**

  GOLD:
    Eine vertiefte Datenschutzkonzeption wird im aktuellen Projekt nicht ausgearbeitet und
    gegebenenfalls als separates Folgeprojekt behandelt.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts
    und den internen Gebrauch zu beschränken.
  BEGRÜNDUNG: Datenschutz-Scope-ENTSCHEIDUNG (nicht vertieft/Folgeprojekt): kein Claim — AU-0059 von F als non_relevant deklariert!
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-043 [requirement] — mein Urteil: **NONE**

  GOLD:
    Admin-Accounts sollen hinzugefügte Daten löschen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts
    und den internen Gebrauch zu beschränken.
  BEGRÜNDUNG: Admin-Löschrecht: nicht ausgedrückt (nur User-Verbot C-052)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-073 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Login-Logo soll vorzugsweise zentriert im oberen Bildschirmbereich platziert werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-095] Das Logo für die App ist noch zu erstellen und soll thematisch etwas mit Kommunikation
    zu tun haben.
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
  BEGRÜNDUNG: Logo zentriert oben: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-075 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Login-Logo soll vorzugsweise rund gestaltet sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-095] Das Logo für die App ist noch zu erstellen und soll thematisch etwas mit Kommunikation
    zu tun haben.
  BEGRÜNDUNG: Logo rund: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-076 [open_question] — mein Urteil: **NONE**

  GOLD:
    Ob die Login-Eingabefelder einen leichten Schattenwurf erhalten, bleibt offen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
  BEGRÜNDUNG: Schattenwurf offen: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-078 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Design der App soll intuitiv und benutzerfreundlich sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
    [C-114] Bei allen Screens soll auf Barrierefreiheit geachtet werden.
  BEGRÜNDUNG: intuitiv/benutzerfreundlich: kein Claim (C-114 nur Barrierefreiheit)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-080 [requirement] — mein Urteil: **NONE**

  GOLD:
    Der Login-Button soll optisch hervorstechen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
    [C-097] Der Login-Screen soll einen Login-Button enthalten.
    [C-098] Der Login-Screen soll keine Registrierungsfunktion anzeigen.
  BEGRÜNDUNG: Button hervorstechen: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-084 [requirement] — mein Urteil: **NONE**

  GOLD:
    Die Suchleiste der Profilübersicht soll gut sichtbar unter der Appbar platziert sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-069] Die Profilübersicht soll eine Suchleiste zum Finden von Profilen enthalten.
    [C-099] Die Appbar soll in der Mitte den Namen der aktuellen Seite anzeigen.
  BEGRÜNDUNG: Suchleiste unter Appbar: Position nirgends (AU-0084 sogar non_relevant deklariert)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-086 [open_question] — mein Urteil: **NONE**

  GOLD:
    Ob die Profile in der Profilübersicht als Liste oder als Kacheln dargestellt werden, bleibt
    offen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-100] Profile in der Profilübersicht sollen ein Vorschaubild, einen Namen und eine
    Kurzbeschreibung anzeigen.
    [C-056] Die Profilübersicht soll eine Liste aller Profile anzeigen.
  BEGRÜNDUNG: Liste-oder-Kacheln offen: kein Claim (C-056 setzt Liste als fest)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-092 [requirement] — mein Urteil: **NONE**

  GOLD:
    No-Go-Einträge sollen einfach zu überblicken beziehungsweise zu durchforsten sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-105] Der No-Go-Screen soll ein starkes visuelles Symbol, beispielsweise ein rotes
    Stoppschild, enthalten.
  BEGRÜNDUNG: überblicken/durchforsten: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-093 [requirement] — mein Urteil: **NONE**

  GOLD:
    No-Go-Einträge sollen sich auf die wichtigsten Informationen beschränken.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-105] Der No-Go-Screen soll ein starkes visuelles Symbol, beispielsweise ein rotes
    Stoppschild, enthalten.
  BEGRÜNDUNG: nur wichtigste Infos: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-095 [requirement] — mein Urteil: **NONE**

  GOLD:
    Medikamentenerinnerungen sollen in der Kalenderansicht durch kleine Icons oder Tags an den
    betreffenden Tagen kenntlich gemacht werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
    [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
    eingesehen werden können.
  BEGRÜNDUNG: Icons/Tags für Erinnerungen: kein Claim
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-105 [decision] — mein Urteil: **NONE**
> ⚠ Kollegen-Review 06.09. (KI-Urteil gekippt)

  GOLD:
    Animationen beziehungsweise zusätzliches Feedback sollen nicht in die App aufgenommen werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-116] Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen
    berücksichtigt werden.
    [C-117] Animationen sollen, wenn überhaupt, nicht ablenkend sein.
  BEGRÜNDUNG: C-117 impliziert MÖGLICHE Aufnahme ('wenn überhaupt') = entgegengesetzte Entscheidung zur Gold-Ablehnung → Negations-Regel: kein Match
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-108 [requirement] — mein Urteil: **NONE**

  GOLD:
    Das Design der App soll ein klares Branding aufweisen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-114] Bei allen Screens soll auf Barrierefreiheit geachtet werden.
  BEGRÜNDUNG: klares Branding: kein Claim (AU-0136 nur als Barrierefreiheit erfasst)
  Prüffrage: Enthält WIRKLICH keine F-Aussage (auch Anhang A) diesen Inhalt?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-010 [open_question] — mein Urteil: **PARTIAL**
> ⚠ Kollegen-Review 06.09. (KI-Urteil gekippt)

  GOLD:
    Ob die App die vollständige bestehende Bewohnerdokumentation übernehmen soll, ist noch mit der
    Einrichtungsleitung zu klären.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
    [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
    [C-020] Die About-Me-Funktion soll Informationen wie Hobbys, Name und Alter enthalten.
    [C-030] Die Einrichtung arbeitet bislang stark analog und erwartet großen Nutzen von
    Digitalisierung.
    [C-031] Eine vollständige Übernahme der gesamten Dokumentation in die App ist in Betracht
    gezogen, soll aber wegen Umfang und Fokus sorgfältig begrenzt werden.
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-034] Die Kommunikationsfunktion soll in verbal und nonverbal unterteilt sein.
    [C-035] Kommunikationsinhalte sollen für alle berechtigten Nutzenden jederzeit abrufbar sein.
  BEGRÜNDUNG: C-031: Erwägung+Begrenzung ✓, aber die OFFENE KLÄRUNG mit der Einrichtungsleitung fehlt
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-012 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die App soll ihren Schwerpunkt auf unterstützende Kommunikation behalten und nicht durch einen
    zu großen Funktionsumfang davon abweichen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
    [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
    [C-020] Die About-Me-Funktion soll Informationen wie Hobbys, Name und Alter enthalten.
    [C-031] Eine vollständige Übernahme der gesamten Dokumentation in die App ist in Betracht
    gezogen, soll aber wegen Umfang und Fokus sorgfältig begrenzt werden.
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-034] Die Kommunikationsfunktion soll in verbal und nonverbal unterteilt sein.
    [C-035] Kommunikationsinhalte sollen für alle berechtigten Nutzenden jederzeit abrufbar sein.
  BEGRÜNDUNG: C-031 nur Doku-Begrenzung, nicht der generelle Fokus-Erhalt
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-014 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Nutzer sollen neue Kommunikationsweisen dynamisch über einen Plus-Button im unteren Bereich der
    Kommunikationsseiten hinzufügen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
    [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
    [C-034] Die Kommunikationsfunktion soll in verbal und nonverbal unterteilt sein.
    [C-036] Die Kommunikationsfunktion soll dynamisch wachsen, sodass neue Erfahrungen hinzugefügt
    werden können.
    [C-037] Für Kommunikationsinhalte sollen nicht nur Texte, sondern auch visuelle Darstellungen
    verwendet werden.
    [C-039] Die App soll Kommunikationssituationen auch per Video mit Beschreibung festhalten
    können.
    [C-040] Videoeinträge sollen insbesondere helfen, unbekannte Verhaltensweisen oder
    Kommunikationsarten wiederzuerkennen.
    [C-041] Zunächst wurde eine eigenständige Videoseite als Teil der App-Struktur vorgesehen.
  BEGRÜNDUNG: C-036 ohne Plus-Button/Position
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-020 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Mitarbeiter und Angehörige sollen als normale User-Accounts geführt werden.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-044] Für unterschiedliche Nutzergruppen sollen verschiedene Accounts mit unterschiedlichen
    Rechten vorgesehen werden.
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
  BEGRÜNDUNG: Gruppen→User-Rolle nur implizit (C-044/C-050)
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-025 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Normale User-Accounts sollen keine neuen Accounts erstellen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-049] Die App soll einen Login-Screen besitzen.
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts
    und den internen Gebrauch zu beschränken.
  BEGRÜNDUNG: nur als Vereinigung C-051/053/054 implizit
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-030 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der About-Me-Bereich soll ganz oben ein Informationsfeld mit Angaben wie Name, Alter und Hobbys
    enthalten.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
    [C-020] Die About-Me-Funktion soll Informationen wie Hobbys, Name und Alter enthalten.
  BEGRÜNDUNG: C-020 ohne 'ganz oben'
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-031 [requirement] — mein Urteil: **PARTIAL**
> ⚠ Kollegen-Review 06.09. (KI-Urteil gekippt)

  GOLD:
    Die About-Me-Foto-Timeline soll einem Social-Media-Feed ähneln: Neu hinzugefügte Bilder
    erscheinen oben, ältere Bilder bleiben darunter scrollbar.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
  BEGRÜNDUNG: C-032/C-033: 'neue oben' ✓, aber Feed-Analogie + 'ältere bleiben darunter scrollbar' fehlen (wesentlicher Inhaltsteil)
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-032 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Im About-Me-Bereich sollen neue Bilder dynamisch über einen Plus-Button am unteren
    Bildschirmrand und einen Eingabedialog hinzugefügt werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung
    hinzugefügt werden können.
    [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
  BEGRÜNDUNG: C-032 ohne Plus-Button-unten/Dialog-Mechanik
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-035 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Kalender soll eine übersichtliche Monatsansicht bieten.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-058] Die App soll einen Kalenderbereich enthalten.
    [C-059] Der Kalender soll Termine enthalten können.
    [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
    [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
    eingesehen werden können.
  BEGRÜNDUNG: C-061 erwähnt Monat, 'übersichtliche Monatsansicht' fehlt
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-041 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Neue No-Go-Einträge sollen über einen Plus-Button dynamisch hinzugefügt werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-046] Die App soll eine No-Go-Seite enthalten.
    [C-048] Die No-Go-Liste soll dynamisch um neue Einträge erweitert werden können.
  BEGRÜNDUNG: C-048 ohne Plus-Button
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-049 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Auf den Kommunikationsseiten soll über eine Suchleiste oben im Bildschirm nach
    Kommunikationsweisen beziehungsweise Merkmalen wie einem Körperteil gesucht oder gefiltert
    werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-069] Die Profilübersicht soll eine Suchleiste zum Finden von Profilen enthalten.
    [C-070] Auch auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein.
    [C-074] Die eigenständige Videoseite wurde verworfen zugunsten einer Integration der
    Videofunktion in die Kommunikationsseiten.
    [C-075] In den Kommunikationsseiten soll es bei verbal und nonverbal möglich sein, Videos mit
    Beschreibung hinzuzufügen.
  BEGRÜNDUNG: C-070 ohne Position 'oben'/Merkmal-Beispiel
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-054 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Neu hinzugefügte Erfahrungen sollen unmittelbar über die App mit anderen berechtigten Nutzern
    geteilt werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-028] Die Lösung soll neuen Mitarbeitenden helfen, Bewohner besser zu verstehen.
    [C-045] Angehörigen-Accounts sollen bei neuen Bewohnern ermöglichen, schon vorab Daten zu
    erfassen.
    [C-035] Kommunikationsinhalte sollen für alle berechtigten Nutzenden jederzeit abrufbar sein.
  BEGRÜNDUNG: Unmittelbarkeit des Teilens nur angenähert (C-035/036)
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-061 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Ob der Hilfebereich zusätzlich erläutern soll, wie Informationen effektiv eingegeben und gesucht
    werden, bleibt offen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-111] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten.
  BEGRÜNDUNG: C-111 generisch, spezifische Offenheit fehlt
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-069 [open_question] — mein Urteil: **PARTIAL**

  GOLD:
    Ob eine NoSQLite-Datenbank zur lokalen Speicherung oder Zwischenspeicherung von Firebase-Daten
    eingesetzt wird, bleibt offen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-089] Firebase Firestore wurde als Datenbank vorgeschlagen und vorläufig gewählt.
    [C-090] Es ist noch zu klären, wie Cloud-Daten lokal auf dem Gerät gespeichert oder
    zwischengespeichert werden sollen.
    [C-091] Als Ansatz für lokale Speicherung wurde genannt, Daten beim Aufrufen einer Seite aus
    Firebase zu laden und lokal zu speichern, statt sie jedes Mal erneut abzurufen.
  BEGRÜNDUNG: C-091 nennt Ansatz, aber NICHT NoSQLite
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-072 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Login-Screen soll ein deutlich sichtbares Logo enthalten.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-095] Das Logo für die App ist noch zu erstellen und soll thematisch etwas mit Kommunikation
    zu tun haben.
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
    [C-097] Der Login-Screen soll einen Login-Button enthalten.
    [C-098] Der Login-Screen soll keine Registrierungsfunktion anzeigen.
  BEGRÜNDUNG: C-094 ohne 'deutlich sichtbar'
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-077 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Unter dem Logo sollen Felder für E-Mail und Passwort angeordnet sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
    [C-097] Der Login-Screen soll einen Login-Button enthalten.
    [C-098] Der Login-Screen soll keine Registrierungsfunktion anzeigen.
  BEGRÜNDUNG: C-096 ohne 'unter dem Logo'
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-079 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Der Login-Button soll unter den Eingabefeldern angeordnet sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-094] Der Login-Screen soll ein Logo enthalten.
    [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
    [C-097] Der Login-Screen soll einen Login-Button enthalten.
    [C-098] Der Login-Screen soll keine Registrierungsfunktion anzeigen.
  BEGRÜNDUNG: C-097 ohne Position
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-082 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die Appbar soll links eine Zurück-Navigation zur vorherigen Seite bereitstellen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-055] Nach dem Login soll eine Profilübersicht angezeigt werden.
    [C-062] Die App soll nach dem Login auf jeder Seite eine Appbar anzeigen.
    [C-063] Die Appbar soll eine Rücknavigation ermöglichen.
    [C-065] Die Appbar soll einen Zugang zu Einstellungen bieten.
    [C-099] Die Appbar soll in der Mitte den Namen der aktuellen Seite anzeigen.
  BEGRÜNDUNG: C-063 ohne 'links'/Pfeil
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-083 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Die Appbar soll rechts ein Einstellungssymbol enthalten, das zur Einstellungsseite führt.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-055] Nach dem Login soll eine Profilübersicht angezeigt werden.
    [C-062] Die App soll nach dem Login auf jeder Seite eine Appbar anzeigen.
    [C-063] Die Appbar soll eine Rücknavigation ermöglichen.
    [C-065] Die Appbar soll einen Zugang zu Einstellungen bieten.
    [C-099] Die Appbar soll in der Mitte den Namen der aktuellen Seite anzeigen.
  BEGRÜNDUNG: C-065 ohne 'rechts'/Symbol
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-087 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Neue Profile sollen über ein Plus-Symbol am unteren Bildschirmrand und einen Dialog mit Bild,
    Name und Beschreibung angelegt werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-101] Neue Profile sollen über ein Plus-Symbol und ein Dialogfeld mit Bild, Name und
    Beschreibung angelegt werden können.
  BEGRÜNDUNG: C-101 ohne 'unterer Bildschirmrand'
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### PFLICHT · G-094 [requirement] — mein Urteil: **PARTIAL**

  GOLD:
    Ein Antippen eines Datums im Kalender soll Details zu Terminen oder Medikamentengaben öffnen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
    [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
    eingesehen werden können.
  BEGRÜNDUNG: C-061 'eingesehen' ≈ Details öffnen, Mechanik grob
  Prüffrage: Fehlt tatsächlich eine wesentliche Qualifikation gemäß Regel 2 — und ist der Kern da?
  - [ ] Urteil bestätigt   — **eher FULL:** C-059 + C-060 + C-061 decken gemeinsam Termine,
    Medikamentengaben sowie das Anzeigen der Einträge über anklickbare Monatstage. Die Formulierung
    „eingesehen werden“ ist hier eine sinngemäße Deckung von „Details öffnen“, kein wesentlicher
    Informationsverlust.

---

## Bilanz-/Signal-Checks (PFLICHT)

- [x] Fs unresolved-Liste ist LEER → 'keine Lücke signalisiert' korrekt?
- [x] F deklarierte AU-0059 als non_relevant — aber: trägt G-042 (Datenschutz-Scope-Entscheidung). Bilanz-Fehler bestätigt?
- [x] F deklarierte AU-0125 als non_relevant — aber: trägt G-030/031/032 (About-Me-Design-Details). Bilanz-Fehler bestätigt?
- [x] F deklarierte AU-0127 als non_relevant — aber: trägt G-049/053 (Video/Suche nonverbal). Bilanz-Fehler bestätigt?
- [x] F deklarierte AU-0084 als non_relevant — aber: trägt G-048-Region (Suchleisten-Nutzen). Bilanz-Fehler bestätigt?
- [x] F deklarierte AU-0086 als non_relevant — aber: trägt G-061 (Hilfe-Inhalt). Bilanz-Fehler bestätigt?

## Full-Stichprobe (geschichtete Seed-Auswahl: SHA-256(id+'w2seed42'), 15 % je Gold-Typ → 12 von 71)

---

### STICHPROBE · G-046 [architecture] — mein Urteil: **FULL**

  GOLD:
    Die App soll auf iOS- und Android-Smartphones lauffähig sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-067] Die App soll plattformübergreifend auf iPhone und Android laufen.
  BEGRÜNDUNG: via C-067
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-003 [decision] — mein Urteil: **FULL**

  GOLD:
    Die Unterstützung soll primär anderen Personen helfen, Bewohner besser zu verstehen, statt
    Bewohner beim Verstehen der Betreuer zu unterstützen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-013] Die fachliche Zielrichtung der Lösung ist, dass Betreuende Bewohner besser verstehen
    können, nicht umgekehrt.
    [C-016] Die Grundidee ist eine App, die Wissen aus Akten und Erfahrungswissen über die
    Kommunikationsweise einer Person festhält.
    [C-017] Die App soll es ermöglichen, bei Verständigungsproblemen nachzuschauen, was eine Person
    gemeint haben könnte.
    [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
    [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
    [C-020] Die About-Me-Funktion soll Informationen wie Hobbys, Name und Alter enthalten.
  BEGRÜNDUNG: via C-013
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-063 [open_question] — mein Urteil: **FULL**

  GOLD:
    Die detaillierte Rechteverwaltung der vorgesehenen Account-Rollen ist noch festzulegen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-077] Es soll einen speziellen Bewohner-Account geben.
    [C-082] Es soll eine nur für Admins sichtbare Admin-Seite geben.
    [C-083] Auf der Admin-Seite sollen neue Accounts angelegt und die Rollen Admin, User und
    Bewohner vergeben werden können.
    [C-084] Die konkrete Ausgestaltung der Rechteverwaltung ist noch offen.
  BEGRÜNDUNG: via C-084
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-048 [requirement] — mein Urteil: **FULL**

  GOLD:
    Auf der Profilübersicht soll über eine Suchleiste nach dem Namen oder anderen relevanten
    Profildaten gesucht werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-069] Die Profilübersicht soll eine Suchleiste zum Finden von Profilen enthalten.
    [C-070] Auch auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein.
  BEGRÜNDUNG: via C-069
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [ ] Urteil bestätigt   — **eher PARTIAL:** C-069 enthält Suchleiste und Zweck
    „Profile finden“, nennt aber weder die Suche nach Namen noch nach anderen Profildaten. Diese
    Suchgegenstände sind ein wesentlicher Bestandteil der Gold-Aussage.

---

### STICHPROBE · G-017 [requirement] — mein Urteil: **FULL**

  GOLD:
    Angehörige sollen bei neuen Bewohnern bereits vorab Daten in die App einpflegen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-028] Die Lösung soll neuen Mitarbeitenden helfen, Bewohner besser zu verstehen.
    [C-043] Angehörige sollen Daten in die App einpflegen können.
    [C-044] Für unterschiedliche Nutzergruppen sollen verschiedene Accounts mit unterschiedlichen
    Rechten vorgesehen werden.
    [C-045] Angehörigen-Accounts sollen bei neuen Bewohnern ermöglichen, schon vorab Daten zu
    erfassen.
  BEGRÜNDUNG: via C-045
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-044 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App ist für den internen Gebrauch der Einrichtung vorgesehen und soll nicht beliebigen
    externen Personen offenstehen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts
    und den internen Gebrauch zu beschränken.
  BEGRÜNDUNG: via C-054
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-057 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Bewohner-Account darf in der Profilübersicht nur das eigene Profil sehen.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-077] Es soll einen speziellen Bewohner-Account geben.
    [C-078] Ein Bewohner-Account soll nur das eigene Profil sehen können.
  BEGRÜNDUNG: via C-078
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-021 [requirement] — mein Urteil: **FULL**

  GOLD:
    Normale User-Accounts sollen Inhalte hinzufügen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-044] Für unterschiedliche Nutzergruppen sollen verschiedene Accounts mit unterschiedlichen
    Rechten vorgesehen werden.
    [C-050] Es soll mindestens die Account-Typen Admin und User geben.
    [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
    [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
    [C-053] Es soll keine Selbstregistrierung geben.
    [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts
    und den internen Gebrauch zu beschränken.
  BEGRÜNDUNG: via C-052
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-037 [requirement] — mein Urteil: **FULL**

  GOLD:
    Im Kalender sollen zu einzelnen Tagen Einträge hinzugefügt werden können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-058] Die App soll einen Kalenderbereich enthalten.
    [C-059] Der Kalender soll Termine enthalten können.
    [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
    [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
    eingesehen werden können.
  BEGRÜNDUNG: via C-061
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-007 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die App soll die Kommunikation zwischen einer beeinträchtigten Person und anderen Personen
    unterstützen, ermöglichen oder verbessern.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-010] Es soll eine Anforderungsanalyse mit mehreren künftigen Nutzenden und Beteiligten
    durchgeführt werden, um Bedürfnisse und Systemumfang zu ermitteln.
    [C-016] Die Grundidee ist eine App, die Wissen aus Akten und Erfahrungswissen über die
    Kommunikationsweise einer Person festhält.
    [C-017] Die App soll es ermöglichen, bei Verständigungsproblemen nachzuschauen, was eine Person
    gemeint haben könnte.
    [C-021] Die App soll möglicherweise um weitere Funktionen wie Medikamentenvergabe und
    Terminkalender erweitert werden.
    [C-006] Es wird eine Lösung gesucht, die die Kommunikation zwischen beeinträchtigten Personen
    und anderen Menschen verbessert und fördert.
  BEGRÜNDUNG: via C-006
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-060 [requirement] — mein Urteil: **FULL**

  GOLD:
    Ein Bewohner-Account soll im eigenen About-Me-Bereich eigene Bilder hinzufügen können.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-079] Ein Bewohner-Account soll auf den About-Me-Bereich des eigenen Profils zugreifen können.
    [C-080] Ein Bewohner-Account soll im About-Me-Bereich eigene Bilder hinzufügen können.
  BEGRÜNDUNG: via C-080
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

### STICHPROBE · G-040 [requirement] — mein Urteil: **FULL**

  GOLD:
    Die Ausloggen-Funktion soll schnell erreichbar sein.
  F-KANDIDATEN (Quell-Region ∪ im Urteil genannte):
    [C-062] Die App soll nach dem Login auf jeder Seite eine Appbar anzeigen.
    [C-063] Die Appbar soll eine Rücknavigation ermöglichen.
    [C-064] Die Appbar soll eine Logout-Funktion enthalten.
    [C-065] Die Appbar soll einen Zugang zu Einstellungen bieten.
    [C-110] Datenschutzeinstellungen sollen leicht zugänglich sein.
    [C-111] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten.
  BEGRÜNDUNG: via C-064
  Prüffrage: Decken die Kandidaten ALLE Bestandteile der Gold-Aussage (inkl. Qualifikationen)?
  - [x] Urteil bestätigt   (sonst: Notiz dahinter → geht ins Matchlog)

---

## Anhang A — kompletter F-Bestand (für NONE-Gegenproben)

- [C-001] Die Einrichtung betreut Einrichtungen und Wohngemeinschaften für Menschen mit
- Beeinträchtigungen.
- [C-002] Die betreuten Menschen haben unterschiedliche Beeinträchtigungen.
- [C-003] Viele Bewohner werden meist nur von langjährigen Mitarbeitenden verstanden.
- [C-004] Bewohner kommunizieren teils verbal in einer eigenen Sprache.
- [C-005] Bewohner kommunizieren teils nonverbal, etwa über eine Art Gebärdensprache oder Ähnliches.
- [C-006] Es wird eine Lösung gesucht, die die Kommunikation zwischen beeinträchtigten Personen und
- anderen Menschen verbessert und fördert.
- [C-007] Eine digitale Lösung wird bevorzugt, etwa ein Computerprogramm oder eine App.
- [C-008] Der Wunsch nach einer Lösung kam nicht primär von den Mitarbeitenden.
- [C-009] Kommunikation ist im Betreuungsalltag täglich ein zentraler Faktor, um auf Bedürfnisse der
- betreuten Personen eingehen zu können.
- [C-010] Es soll eine Anforderungsanalyse mit mehreren künftigen Nutzenden und Beteiligten
- durchgeführt werden, um Bedürfnisse und Systemumfang zu ermitteln.
- [C-011] Ein System, das zwischen Bewohnern und Betreuenden wechselseitig übersetzt, wird
- ausgeschlossen.
- [C-012] Als Begründung für den Ausschluss eines Übersetzungssystems wird genannt, dass dies weder
- realistisch noch kosteneffizient und nicht allgemein nutzbringend wäre.
- [C-013] Die fachliche Zielrichtung der Lösung ist, dass Betreuende Bewohner besser verstehen
- können, nicht umgekehrt.
- [C-014] Es existieren klassische Akten, in denen Informationen über Bewohner dokumentiert werden.
- [C-015] Ein Kennenlern-Tag in einer oder mehreren Einrichtungen soll stattfinden, um die
- Kommunikationssituation und Bedürfnisse vor Ort besser zu verstehen.
- [C-016] Die Grundidee ist eine App, die Wissen aus Akten und Erfahrungswissen über die
- Kommunikationsweise einer Person festhält.
- [C-017] Die App soll es ermöglichen, bei Verständigungsproblemen nachzuschauen, was eine Person
- gemeint haben könnte.
- [C-018] Die App soll eine About-Me-Funktion für einen ersten Eindruck der Person bereitstellen.
- [C-019] Die About-Me-Funktion soll Bilder und beschreibende Texte enthalten.
- [C-020] Die About-Me-Funktion soll Informationen wie Hobbys, Name und Alter enthalten.
- [C-021] Die App soll möglicherweise um weitere Funktionen wie Medikamentenvergabe und
- Terminkalender erweitert werden.
- [C-022] Die App soll Dokumentation von Erfahrungen und neuem Wissen zur Kommunikation
- unterstützen.
- [C-023] Vorhandene Dokumentationen werden aus Sicht einer Fachkraft nicht gelesen, weil sie zu
- umfangreich sind und gesuchte Informationen oft schwer auffindbar sind.
- [C-024] Der Zugriff auf bestehende Akten für Einsicht oder Beispielmaterial ist aufgrund von
- Datenschutz potenziell schwierig.
- [C-025] Neue Mitarbeitende werden derzeit durch Dokumentation und begleitete Einarbeitung
- unterstützt, bis gegenseitiges Verständnis und Vertrauen aufgebaut sind.
- [C-026] Erfahrene Mitarbeitende besitzen relevantes Erfahrungswissen über die Kommunikationsweisen
- der Bewohner.
- [C-027] Die Lösung soll Wissen so festhalten, dass es schnell abrufbar ist.
- [C-028] Die Lösung soll neuen Mitarbeitenden helfen, Bewohner besser zu verstehen.
- [C-029] Die App soll nach Möglichkeit Zeitaufwand für Einarbeitung reduzieren.
- [C-030] Die Einrichtung arbeitet bislang stark analog und erwartet großen Nutzen von
- Digitalisierung.
- [C-031] Eine vollständige Übernahme der gesamten Dokumentation in die App ist in Betracht gezogen,
- soll aber wegen Umfang und Fokus sorgfältig begrenzt werden.
- [C-032] Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung hinzugefügt
- werden können.
- [C-033] Auf der About-Me-Seite sollen neu hinzugefügte Bilder oben angezeigt werden.
- [C-034] Die Kommunikationsfunktion soll in verbal und nonverbal unterteilt sein.
- [C-035] Kommunikationsinhalte sollen für alle berechtigten Nutzenden jederzeit abrufbar sein.
- [C-036] Die Kommunikationsfunktion soll dynamisch wachsen, sodass neue Erfahrungen hinzugefügt
- werden können.
- [C-037] Für Kommunikationsinhalte sollen nicht nur Texte, sondern auch visuelle Darstellungen
- verwendet werden.
- [C-038] Es soll geklärt werden, ob Angehörige dem Einsatz von Fotos in der App zustimmen.
- [C-039] Die App soll Kommunikationssituationen auch per Video mit Beschreibung festhalten können.
- [C-040] Videoeinträge sollen insbesondere helfen, unbekannte Verhaltensweisen oder
- Kommunikationsarten wiederzuerkennen.
- [C-041] Zunächst wurde eine eigenständige Videoseite als Teil der App-Struktur vorgesehen.
- [C-042] Auch Angehörige sollen Zugriff auf die App erhalten.
- [C-043] Angehörige sollen Daten in die App einpflegen können.
- [C-044] Für unterschiedliche Nutzergruppen sollen verschiedene Accounts mit unterschiedlichen
- Rechten vorgesehen werden.
- [C-045] Angehörigen-Accounts sollen bei neuen Bewohnern ermöglichen, schon vorab Daten zu
- erfassen.
- [C-046] Die App soll eine No-Go-Seite enthalten.
- [C-047] Auf der No-Go-Seite sollen Dinge festgehalten werden, die in Gegenwart eines Bewohners
- unbedingt zu vermeiden sind.
- [C-048] Die No-Go-Liste soll dynamisch um neue Einträge erweitert werden können.
- [C-049] Die App soll einen Login-Screen besitzen.
- [C-050] Es soll mindestens die Account-Typen Admin und User geben.
- [C-051] Admin-Accounts sollen neue Accounts anlegen und verwalten können.
- [C-052] User-Accounts sollen Inhalte hinzufügen, aber nichts löschen können.
- [C-053] Es soll keine Selbstregistrierung geben.
- [C-054] Das Login-Konzept dient dazu, den Zugriff auf sensible Daten auf zugewiesene Accounts und
- den internen Gebrauch zu beschränken.
- [C-055] Nach dem Login soll eine Profilübersicht angezeigt werden.
- [C-056] Die Profilübersicht soll eine Liste aller Profile anzeigen.
- [C-057] Jedes Profil in der Profilübersicht soll anklickbar sein und zu einer Detailansicht des
- gewählten Profils führen.
- [C-058] Die App soll einen Kalenderbereich enthalten.
- [C-059] Der Kalender soll Termine enthalten können.
- [C-060] Der Kalender soll auch Medikamentengaben enthalten können.
- [C-061] Kalendereinträge sollen über anklickbare Tage des Monats erfasst beziehungsweise
- eingesehen werden können.
- [C-062] Die App soll nach dem Login auf jeder Seite eine Appbar anzeigen.
- [C-063] Die Appbar soll eine Rücknavigation ermöglichen.
- [C-064] Die Appbar soll eine Logout-Funktion enthalten.
- [C-065] Die Appbar soll einen Zugang zu Einstellungen bieten.
- [C-066] Mitarbeitende sollen nur Profile aus der eigenen Einrichtung sehen können, nicht
- einrichtungsübergreifend.
- [C-067] Die App soll plattformübergreifend auf iPhone und Android laufen.
- [C-068] Es soll geprüft werden, ob die App auch auf Tablets nutzbar sein soll.
- [C-069] Die Profilübersicht soll eine Suchleiste zum Finden von Profilen enthalten.
- [C-070] Auch auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein.
- [C-071] Für Kommunikationsbeschreibungen soll ein standardisiertes Eingabemuster verwendet werden,
- um Suche und Filterung zu verbessern.
- [C-072] Ein mögliches Beschreibungsmuster ist, zuerst das Körperteil und dann die Beschreibung
- oder Aktion zu erfassen.
- [C-073] Die genaue Formulierung des Beschreibungsmusters ist noch festzulegen beziehungsweise
- zunächst eigenständig zu implementieren.
- [C-074] Die eigenständige Videoseite wurde verworfen zugunsten einer Integration der Videofunktion
- in die Kommunikationsseiten.
- [C-075] In den Kommunikationsseiten soll es bei verbal und nonverbal möglich sein, Videos mit
- Beschreibung hinzuzufügen.
- [C-076] Neue Inhalte im About-Me-Bereich sollen eine Popup-Benachrichtigung an verbundene Nutzer
- der Einrichtung auslösen.
- [C-077] Es soll einen speziellen Bewohner-Account geben.
- [C-078] Ein Bewohner-Account soll nur das eigene Profil sehen können.
- [C-079] Ein Bewohner-Account soll auf den About-Me-Bereich des eigenen Profils zugreifen können.
- [C-080] Ein Bewohner-Account soll im About-Me-Bereich eigene Bilder hinzufügen können.
- [C-081] Die Rechte eines Bewohner-Accounts sollen auf spezifische Funktionen beschränkt werden, um
- Fehlbedienungen zu minimieren.
- [C-082] Es soll eine nur für Admins sichtbare Admin-Seite geben.
- [C-083] Auf der Admin-Seite sollen neue Accounts angelegt und die Rollen Admin, User und Bewohner
- vergeben werden können.
- [C-084] Die konkrete Ausgestaltung der Rechteverwaltung ist noch offen.
- [C-085] Für die Umsetzung wurde Flutter mit Dart als plattformübergreifender Technologie-Stack
- festgelegt.
- [C-086] Als bevorzugte Entwicklungsumgebung wurde Android Studio vereinbart.
- [C-087] Im Team sollen möglichst einheitliche Tool- und Versionsstände verwendet werden, um
- Entwicklungsprobleme zu vermeiden.
- [C-088] Als zu installierende Versionen wurden Flutter 3.13.9 und Dart 3.1.5 genannt.
- [C-089] Firebase Firestore wurde als Datenbank vorgeschlagen und vorläufig gewählt.
- [C-090] Es ist noch zu klären, wie Cloud-Daten lokal auf dem Gerät gespeichert oder
- zwischengespeichert werden sollen.
- [C-091] Als Ansatz für lokale Speicherung wurde genannt, Daten beim Aufrufen einer Seite aus
- Firebase zu laden und lokal zu speichern, statt sie jedes Mal erneut abzurufen.
- [C-092] Für Entwicklung und Tests soll zunächst Android 11 als gemeinsame Zielversion verwendet
- werden.
- [C-093] Das JetX-Package soll ausprobiert beziehungsweise in die Rahmenbedingungen aufgenommen
- werden.
- [C-094] Der Login-Screen soll ein Logo enthalten.
- [C-095] Das Logo für die App ist noch zu erstellen und soll thematisch etwas mit Kommunikation zu
- tun haben.
- [C-096] Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
- [C-097] Der Login-Screen soll einen Login-Button enthalten.
- [C-098] Der Login-Screen soll keine Registrierungsfunktion anzeigen.
- [C-099] Die Appbar soll in der Mitte den Namen der aktuellen Seite anzeigen.
- [C-100] Profile in der Profilübersicht sollen ein Vorschaubild, einen Namen und eine
- Kurzbeschreibung anzeigen.
- [C-101] Neue Profile sollen über ein Plus-Symbol und ein Dialogfeld mit Bild, Name und
- Beschreibung angelegt werden können.
- [C-102] Die Profil-Detailansicht soll das gewählte Profilbild größer anzeigen.
- [C-103] Die Profil-Detailansicht soll vier interaktive Hauptbereiche der App anzeigen.
- [C-104] Die Kommunikationsseite soll verbale und nonverbale Kommunikation visuell getrennt und mit
- Symbolen kennzeichnen.
- [C-105] Der No-Go-Screen soll ein starkes visuelles Symbol, beispielsweise ein rotes Stoppschild,
- enthalten.
- [C-106] Der Settings-Screen soll je nach Nutzerrolle differenzierte Ansichten bereitstellen.
- [C-107] Admins sollen im Settings-Screen Nutzeraccounts hinzufügen und Rechte anpassen können.
- [C-108] Jeder Nutzer soll im Settings-Screen das eigene Profil ändern können.
- [C-109] Jeder Nutzer soll im Settings-Screen Spracheinstellungen ändern können.
- [C-110] Datenschutzeinstellungen sollen leicht zugänglich sein.
- [C-111] Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten.
- [C-112] Beim ersten Login soll eine kurze Tour durch die App angeboten werden.
- [C-113] Es soll einen dauerhaft aufrufbaren Hilfebereich geben.
- [C-114] Bei allen Screens soll auf Barrierefreiheit geachtet werden.
- [C-115] Zur Barrierefreiheit sollen große Schrift, ausreichender Kontrast und eine zurückhaltende
- Farbverwendung beitragen.
- [C-116] Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen
- berücksichtigt werden.
- [C-117] Animationen sollen, wenn überhaupt, nicht ablenkend sein.
