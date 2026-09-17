# Verifikation LEDGER Interview (Lauf 8189ea) — Claim-Blatt (P2/P4) · v3.1

> Claim gegen zitierte Quell-Units: ① gestützt? ② präzise (Gold-Treffer oder gestützter Zusatz)?
> Eskalation: >10 % Fehlurteile → Stichprobe verdoppeln.

## Stichprobe (Seed-Schichtung 15 % je kind → 12 von 47; keine Nicht-voll-Claim-Urteile in diesem Lauf)

---

### STICHPROBE · CAN-027 [constraint/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende sollen nur
    die Profile der Bewohner ihrer eigenen Einrichtung sehen können.
  ZITIERTE QUELL-UNITS (AU-0060, AU-0063, AU-0064):
    [AU-0060] Speaker 2: Was das Thema Datenschutz angeht, werden wir nicht allzu tief ins Detail
    gehen. Dieser Schritt wäre vermutlich ein zusätzliches Projekt. Wir haben uns das aber so
    überlegt, dass wir eine Login-Seite programmieren, auf der man sich nur einloggen kann, wenn man
    einen Account zugewiesen bekommen hat. Man hat also nicht die Möglichkeit, sich die App
    runterzuladen und sich zu registrieren und sich so einen Account anzulegen. Warum? Damit ist
    auch klar, dass diese App nur für den internen Gebrauch der KYOS wie gedacht ist und sich nicht
    beliebig jeder anmelden kann. Ein Account, der mit Adminrechten versehen ist, kann dann neue
    Accounts anlegen und Rechte verwalten. Es gibt also den Power-Nutzer-Admin und den Nutzer-User.
    Der User hat logischerweise weniger Rechte und kann keine Accounts erstellen oder hinzugefügte
    Daten aus der App löschen, während der Admin alles kann.
    [AU-0063] Speaker 1: Aufgrund von Datenschutz sollte es so geregelt sein, dass es nicht
    übergreifend ist. Also ein Mitarbeiter aus dem Salzburger sollte nur diese Profile sehen von den
    Bewohnern, die auch tatsächlich im Salzburger Weg sind.
    [AU-0064] Speaker 2: Alles klar, so wird das gemacht. Wir werden uns einen Weg überlegen, wie
    man das lösen könnte und bis zum nächsten Treffen erarbeiten.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-020 [decision/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Nach dem Login soll die App auf eine Profilübersicht mit anklickbarer Liste aller sichtbaren
    Profile führen.
  ZITIERTE QUELL-UNITS (AU-0045):
    [AU-0045] Speaker 1: Ja, so umgehen wir etwas das Datenschutz-Thema und es kann sich nicht jeder
    beliebige bei der App registrieren und auf Daten zugreifen. Ich würde sagen, nach dem Login
    landet man auf einer Profilübersicht-Seite. Dort zeigen wir eine Liste aller Profile an. Jedes
    Profil ist anklickbar und führt zu einer weiteren Seite. Also speziell nur die weiteren Daten
    von dem gewählten Profil.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-041 [decision/must_consider] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Das JetX-Package soll im Projekt ausprobiert und als zusätzliche technische Rahmenbedingung
    erwogen werden.
  ZITIERTE QUELL-UNITS (AU-0114, AU-0115):
    [AU-0114] Speaker 1: Ich hätte noch eine Empfehlung. Wir sollten JetX Package verwenden. Habe da
    gelesen, dass es einen sehr viel Arbeit abnehmen soll.
    [AU-0115] Speaker 2: Ja, JetX soll tatsächlich sehr gut sein. Nehmen wir das zu den
    Rahmenbedingungen und schauen dann, wie es damit läuft. Kenne mich sonst nicht damit aus.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-045 [non_functional_requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Bei allen Screens soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift,
    ausreichenden Kontrast und die Vermeidung zu vieler Farben.
  ZITIERTE QUELL-UNITS (AU-0133):
    [AU-0133] Speaker 2: Bei all diesen Screens sollten wir auch die Barrierefreiheit im Auge
    behalten. Große Schrift, ausreichend Kontrast und die Vermeidung von zu vielen Farben helfen
    dabei, die App für alle zugänglich zu machen.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-010 [open_question/must_clarify] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Es muss geklärt werden, ob und unter welchen Datenschutzbedingungen Einsicht in Bewohnerakten
    möglich ist.
  ZITIERTE QUELL-UNITS (AU-0020):
    [AU-0020] Speaker 2: So würde ich es auch nicht direkt betiteln. Das war nur meine persönliche
    Meinung zu der Frage. Das Einsehen so einer Akte könnte eher schwierig aufgrund von Datenschutz
    werden. Aber ich werde mich mal umhören und euch Bescheid geben, falls es doch möglich sein
    sollte.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-025 [open_requirement/must_clarify] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout vorhanden sein;
    zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden.
  ZITIERTE QUELL-UNITS (AU-0054):
    [AU-0054] Speaker 2: Das wird super nützlich sein, um bestimmte Verhaltensweisen oder
    Kommunikationsarten visuell festzuhalten. Wir sollten nicht vergessen, eine Appbar oben im
    Bildschirm zu programmieren. Mit der kann man dann zurücknavigieren oder sich ausloggen über ein
    Logout-Symbol. Diese Appbar sollte nach dem Login auf jeder Seite oben sein. Vielleicht wäre
    auch ein Einstellungssymbol in der Appbar sinnvoll, damit man zu den Einstellungen kommen kann.
    Da müssen wir uns auch noch was überlegen.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-037 [process_constraint/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Das Team soll möglichst dieselbe Entwicklungsumgebung verwenden, konkret Android Studio, und
    einheitliche Flutter- und Dart-Versionen installieren, um Entwicklungsprobleme zu vermeiden.
  ZITIERTE QUELL-UNITS (AU-0100, AU-0101, AU-0102):
    [AU-0100] Speaker 1: Naja, es kann schon zu Problemen führen und dann liegt es in deiner
    Verantwortung, diese zu lösen. Aber es ist natürlich deine freie Entscheidung, obwohl ich
    trotzdem vorschlagen würde, dass wir alle dieselbe Entwicklungsumgebung nutzen.
    [AU-0101] Speaker 2: Ja, ich bleibe dabei und wechsle notfalls auch auf Android Studio.
    [AU-0102] Speaker 1: Ich habe in der Zwischenzeit überprüft, welche Versionen die aktuellsten
    für Flutter und Dart sind und bin dabei auf Flutter Version 3.13.9 und Flutter 3.1.5 gekommen.
    Wichtig ist, dass wir dabei alle dieselbe Versionen installieren, damit wir später keine
    Probleme haben, wenn wir mit der Entwicklung beginnen.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-043 [requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Die Einstellungsseite soll rollenabhängig differenziert sein; Admins sollen dort Nutzeraccounts
    hinzufügen und Rechte anpassen können, während alle Nutzer ihr Profil ändern und etwa die
    Sprache wählen können.
  ZITIERTE QUELL-UNITS (AU-0130):
    [AU-0130] Speaker 1: Richtig. Und für den Settings-Screen brauchen wir wirklich eine
    differenzierte Ansicht, je nach Nutzerrolle. Ein Admin kann dort beispielsweise Nutzeraccounts
    hinzufügen und Rechte anpassen. Aber jeder Nutzer sollte sein Profil ändern und Einstellungen
    wie die Sprache wählen können.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-044 [requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim ersten
    Login und als später erneut aufrufbaren Hilfebereich.
  ZITIERTE QUELL-UNITS (AU-0131, AU-0132):
    [AU-0131] Speaker 2: Genau. Wir müssen darauf achten, dass die Datenschutzeinstellungen leicht
    zugänglich sind und das Ausloggen sollte immer schnell erreichbar sein. Denkst du, wir sollten
    auch eine Hilfe-Funktion oder ein Tutorial einbauen?
    [AU-0132] Speaker 1: Auf jeden Fall. Eine kurze Tour durch die App beim ersten Login könnte
    helfen, die Nutzer mit den Funktionen vertraut zu machen. Und ein Hilfebereich, den man immer
    wieder aufrufen kann, wäre auch sinnvoll.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-001 [requirement/must] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Die Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen
    Personen verbessern und fördern.
  ZITIERTE QUELL-UNITS (AU-0001):
    [AU-0001] Speaker 2: Wir sind die EINRICHTUNG und wir betreuen Einrichtungen und
    Wohngemeinschaften für Menschen mit Beeinträchtigungen. Ich bin der Gesamtleiter dieser
    Einrichtung und vertrete diese heute in unserem Kick-Off, um eine Idee zu erarbeiten. Die
    Menschen, die von uns betreut werden, haben unterschiedliche Beeinträchtigungen und werden
    meistens nur von langjährigen Mitarbeitern verstanden. Die Mitarbeiter sind meistens
    ausgebildete Heilerziehungspfleger, Pädagogen oder normale Krankenpfleger. Bei den Bewohnern ist
    es so, dass sie nur verbal, aber auf einer eigenen Sprache kommunizieren können oder nur
    nonverbal durch eine Art Gebärdensprache bzw. Ähnliches. Wir suchen nach einer Lösung, die
    Kommunikation zwischen den Beeinträchtigten und anderen Menschen zu verbessern und zu fördern.
    Wir sind auch im Zeitalter der Digitalisierung angekommen. Auch wenn es bei uns etwas langsamer
    voranschreitet, wollen wir mit der Zeit gehen. Deswegen würden wir natürlich eine digitale
    Lösung für unsere Anliegen bevorzugen, also ein Computerprogramm oder Ähnliches.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-009 [risk/must_note] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Bestehende Dokumentationen können für Mitarbeitende schwer nutzbar sein, weil sie umfangreich
    sind und gesuchte Informationen oft nicht schnell gefunden werden.
  ZITIERTE QUELL-UNITS (AU-0018, AU-0020):
    [AU-0018] Speaker 2: Grundsätzlich wird alles nach einem bestimmten Plan dokumentiert. Auch
    Erfahrungen und neues Wissen wird schriftlich festgehalten. Ob sie gelesen werden, kann ich
    nicht persönlich beantworten, weil ich sie definitiv nicht lese. Es ist einfach zu viel zu lesen
    und oft findet man auch nicht das, wonach man sucht. Aber der Grund dafür, dass ich es nicht
    lese, ist meine Erfahrung.
    [AU-0020] Speaker 2: So würde ich es auch nicht direkt betiteln. Das war nur meine persönliche
    Meinung zu der Frage. Das Einsehen so einer Akte könnte eher schwierig aufgrund von Datenschutz
    werden. Aber ich werde mich mal umhören und euch Bescheid geben, falls es doch möglich sein
    sollte.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

---

### STICHPROBE · CAN-008 [scope/must_note] — mein Urteil: **valid + quellengestützt**

  CLAIM:
    Es existieren klassische Akten, in denen Informationen über Bewohner dokumentiert sind.
  ZITIERTE QUELL-UNITS (AU-0007):
    [AU-0007] Speaker 2: Ja, es gibt natürlich klassische Akten, wo alles über die Bewohner der
    Einrichtung festgehalten und dokumentiert wird.
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt

