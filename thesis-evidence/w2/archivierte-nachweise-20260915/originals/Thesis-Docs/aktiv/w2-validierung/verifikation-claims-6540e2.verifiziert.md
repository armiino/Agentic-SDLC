# Verifikation Interview (Lauf 6540e2) — Claim-Blatt (P2 Precision / P4 Semantic Support)

> Verifiziert am 06.09.2026 anhand der jeweils abgedruckten Quell-Units. Bei C-088 bleiben beide
> positiven Eigenschafts-Checkboxen bewusst leer; die Begründung ist direkt am Block ergänzt.

> Hier prüfst du in die ANDERE Richtung: F-Aussage gegen ihre zitierten Quell-Units.
> Zwei Fragen je Block: ① STÜTZUNG — folgt die Aussage aus den zitierten Units (keine Erfindung,
> keine Übersteigerung)? ② PRÄZISION — ist sie korrekt (Gold-Treffer ODER als gestützter
> Zusatz [gold_escape] in Ordnung)? Eskalation: >10 % Fehlurteile → Stichprobe verdoppeln.

---

### PFLICHT · C-088 [architecture] — mein Urteil: **NICHT quellentreu**

  F-AUSSAGE:
    Als zu installierende Versionen wurden Flutter 3.13.9 und Dart 3.1.5 genannt.
  ZITIERTE QUELL-UNITS (AU-0102):
    [AU-0102] Speaker 1: Ich habe in der Zwischenzeit überprüft, welche Versionen die aktuellsten
    für Flutter und Dart sind und bin dabei auf Flutter Version 3.13.9 und Flutter 3.1.5 gekommen.
    Wichtig ist, dass wir dabei alle dieselbe Versionen installieren, damit wir später keine
    Probleme haben, wenn wir mit der Entwicklung beginnen.
  BEFUND: einziger Nicht-voll-Claim: C-088 behauptet 'Dart 3.1.5' — die Quelle (AU-0102) nennt fälschlich zweimal 'Flutter'; plausible Korrektur, aber nicht quellentreu
  - [ ] ① gestützt bestätigt    - [ ] ② präzise bestätigt
  - Notiz: **① nein / ② nein.** AU-0102 nennt zwar 3.13.9 und 3.1.5, bezeichnet aber beide
    ausdrücklich als Flutter-Versionen. Die Zuordnung von 3.1.5 zu Dart ist eine plausible
    Korrektur, folgt jedoch weder aus der zitierten Unit noch aus dem Gold-Wortlaut.

## Stichprobe (geschichtete Seed-Auswahl, 15 % je Claim-Typ → 20 von 116)

---

### STICHPROBE · C-025 [architecture] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Neue Mitarbeitende werden derzeit durch Dokumentation und begleitete Einarbeitung unterstützt,
    bis gegenseitiges Verständnis und Vertrauen aufgebaut sind.
  ZITIERTE QUELL-UNITS (AU-0022):
    [AU-0022] Speaker 2: Das läuft recht unterschiedlich ab. Im Normalfall gibt es erstmals
    natürlich die Dokumentationen. Ansonsten werden die neuen Mitarbeiter so eingearbeitet, dass
    immer jemand mit dabei ist, bis der neue Mitarbeiter und die Bewohner sich gut genug
    kennengelernt haben und sich weitestgehend verstehen. Da spielt natürlich auch das Vertrauen,
    das erst mal aufgebaut werden muss, eine große Rolle. Aber das heißt, im Endeffekt erklären wir
    den Neuen immer, was gemeint ist, wenn sie mit den Bewohnern versuchen zu kommunizieren.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-014 [architecture] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Es existieren klassische Akten, in denen Informationen über Bewohner dokumentiert werden.
  ZITIERTE QUELL-UNITS (AU-0007):
    [AU-0007] Speaker 2: Ja, es gibt natürlich klassische Akten, wo alles über die Bewohner der
    Einrichtung festgehalten und dokumentiert wird.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-086 [decision] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Als bevorzugte Entwicklungsumgebung wurde Android Studio vereinbart.
  ZITIERTE QUELL-UNITS (AU-0097, AU-0098, AU-0101):
    [AU-0097] Speaker 2: Ich habe Flutter und Dart in Android Studio entwickelt und kenne mich nur
    damit aus, da es eher IntelliJ ähnelt.
    [AU-0098] Speaker 1: Mir ist ein IntelliJ ähnliches System lieber und ich würde deswegen auch
    Android Studio verwenden.
    [AU-0101] Speaker 2: Ja, ich bleibe dabei und wechsle notfalls auch auf Android Studio.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-041 [decision] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Zunächst wurde eine eigenständige Videoseite als Teil der App-Struktur vorgesehen.
  ZITIERTE QUELL-UNITS (AU-0032, AU-0033, AU-0053):
    [AU-0032] Speaker 2: Das ist eine sehr gute Idee. Machen Sie noch eine Videoseite, wo man so
    etwas machen kann. Ich kann mir gut vorstellen, dass es gerade Neubetreuern helfen würde, aber
    natürlich auch allen anderen.
    [AU-0033] Speaker 1: Okay, danke für den Input. Ich wiederhole mal. Es wird schon mal drei
    Buttons geben, die zu bestimmten Seiten führen. Einmal about me, also über mich. Diese Seite
    verschafft dem Benutzer einen ersten Eindruck über die Person und bietet ein Infofeld, wo
    bestimmte Informationen gelistet sind. Darunter wird eine Liste von Bildern sein, die an Social
    Media erinnern sollen. Diese Bilder werden mit einer Beschreibung versehen. Es gibt also
    irgendwo, am besten unten in der Mitte, einen Button mit einem Plus-Symbol, welcher einen Dialog
    öffnet. Der es ermöglicht, ein Foto hinzuzufügen und einen Text dafür zu schreiben. Das neueste
    hinzugefügte Foto wird immer oben angezeigt. Die Liste der Fotos wächst also nach oben, sodass
    man immer das aktuelle Bild sehen kann. Ein zweiter Button wird zu einer Seite führen, die eine
    Kommunikationsseite ist. Dort gibt es eine weitere Unterteilung in verbal und nonverbal.
    Wahrscheinlich macht es hier auch Sinn, noch zwei Buttons zu erstellen, die dann zu den
    jeweiligen Seiten navigieren, wenn man draufklickt. Hier werden die verbalen und nonverbalen
    Kommunikationsarten durch Texte, Bilder etc. festgehalten, genauso wie beim About Me Screen.
    Wird auch hier die Möglichkeit sein, diese Liste an Kommunikationsarten dynamisch zu erweitern.
    Also durch einen Button am Boden, der die Seite, der dann wiederum einen Dialog öffnet, in dem
    man Texte schreiben kann und Bilder hinzufügen natürlich. Der dritte Button führt zu einer
    Videoseite. Diese Seite soll den Sinn haben, Videos zu bestimmen. Videos zu bestimmten
    Situationen zu enthalten. Das bedeutet also, dass wenn man ein unbekanntes Verhalten bzw. eine
    unbekannte Kommunikationsart des Beeinträchtigten sieht, man sie dort als Video finden kann.
    Auch wenn man eine neue Situation erkennt und eine neue Erfahrung der Kommunikation macht, soll
    diese hinzugefügt werden können. Wie gewohnt durch einen Button am Ende des Bildschirms, der
    wohl die Funktionalität hat, ein neues Video mit einer Beschreibung hinzuzufügen.
    [AU-0053] Speaker 1: Auf der Videoseite könnten wir einen Bereich einrichten, wo neue Videos
    leicht hinzuzufügen sind. Ein Plus-Button unten auf dem Bildschirm ermöglicht das Hinzufügen
    neuer Videos mit Beschreibungen. Auch hier sollte das neueste Video oben angezeigt werden.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-015 [decision] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Ein Kennenlern-Tag in einer oder mehreren Einrichtungen soll stattfinden, um die
    Kommunikationssituation und Bedürfnisse vor Ort besser zu verstehen.
  ZITIERTE QUELL-UNITS (AU-0008, AU-0009):
    [AU-0008] Speaker 1: Das ist schon mal gut zu wissen. Wäre es eventuell möglich, eine
    beeinträchtigte Person kennenzulernen? Also das, was wir als Team gemeinsam in Begleitung eines
    Heileziehungspflegers in eine ihrer Einrichtungen fahren, um die Situation besser einschätzen zu
    können. Wir denken, es wäre vom Vorteil zu sehen, wie die Kommunikation aktuell abläuft und wie
    und wo die Bedürfnisse allgemein liegen.
    [AU-0009] Speaker 2: Ja, wir können gerne einen Termin für so einen Kennenlern-Tag ausmachen.
    Wir können auch gerne in mehrere Einrichtungen fahren, um ein größeres Gesamtbild der
    Einrichtung-NoName zu schaffen.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-068 [open_question] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Es soll geprüft werden, ob die App auch auf Tablets nutzbar sein soll.
  ZITIERTE QUELL-UNITS (AU-0067, AU-0068):
    [AU-0067] Speaker 1: Das wäre ideal. Dann können die App mehr Leute nutzen. Wird es auch auf
    einem Tablet laufen? Also könnte man die App auch auf einem Tablet laden und nutzen. Dadurch
    würde sich auch die Datenschutzthematik leichter gestalten, weil dann zum Beispiel einfach die
    Tablets oder zwei, die in der Einrichtung zur Verfügung stehen, genutzt werden.
    [AU-0068] Speaker 2: Auch das nehmen wir zu den Anforderungen auf und evaluieren, ob das
    umsetzbar sein wird oder nicht.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-116 [open_question] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt
    werden.
  ZITIERTE QUELL-UNITS (AU-0134):
    [AU-0134] Speaker 1: Absolut. Wir sollten vielleicht auch Sprachbefehle oder alternative
    Eingabemethoden für beeinträchtigte Nutzer berücksichtigen. Was ist mit Animationen oder
    Feedback? Sollten wir das einbauen?
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-097 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Der Login-Screen soll einen Login-Button enthalten.
  ZITIERTE QUELL-UNITS (AU-0119):
    [AU-0119] Speaker 2: Das macht Sinn. Und der Login-Button sollte direkt unter den zwei Feldern
    sein und hervorstechen. Wir wollen keine Registrierung auf dem Screen, nur den Login, um es
    schlicht zu halten. Also der Login-Screen besteht aus Logo, E-Mail und Passwort-Feld und einem
    Login-Button.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-096 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Der Login-Screen soll E-Mail- und Passwort-Felder enthalten.
  ZITIERTE QUELL-UNITS (AU-0118, AU-0119):
    [AU-0118] Speaker 1: Ja, das Logo oben ist gut. Direkt darunter sollten wir das E-Mail- und
    Passwort-Feld anordnen. Sie müssen intuitiv und benutzerfreundlich sein, vielleicht mit einem
    leichten Schattenwurf, aber lassen wir das noch offen.
    [AU-0119] Speaker 2: Das macht Sinn. Und der Login-Button sollte direkt unter den zwei Feldern
    sein und hervorstechen. Wir wollen keine Registrierung auf dem Screen, nur den Login, um es
    schlicht zu halten. Also der Login-Screen besteht aus Logo, E-Mail und Passwort-Feld und einem
    Login-Button.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-109 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Jeder Nutzer soll im Settings-Screen Spracheinstellungen ändern können.
  ZITIERTE QUELL-UNITS (AU-0130):
    [AU-0130] Speaker 1: Richtig. Und für den Settings-Screen brauchen wir wirklich eine
    differenzierte Ansicht, je nach Nutzerrolle. Ein Admin kann dort beispielsweise Nutzeraccounts
    hinzufügen und Rechte anpassen. Aber jeder Nutzer sollte sein Profil ändern und Einstellungen
    wie die Sprache wählen können.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-050 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Es soll mindestens die Account-Typen Admin und User geben.
  ZITIERTE QUELL-UNITS (AU-0043, AU-0060):
    [AU-0043] Speaker 1: Ja, beim Thema Login sollten wir verschiedene Account-Typen haben. User und
    Admin wäre mal ein Anfang. Der Admin hat dabei mehr Rechte und kann neue Accounts anlegen und
    diese verwalten, während die User-Accounts nur User-Rechte haben. Als Betreuer und Angehörige
    wären solche User und die können nur Inhalte hinzufügen, aber nichts löschen. Was hältst du
    davon?
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
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-053 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Es soll keine Selbstregistrierung geben.
  ZITIERTE QUELL-UNITS (AU-0044, AU-0060, AU-0088):
    [AU-0044] Speaker 2: Das klingt gut. Wie wir den Login-Screen dann vom Design her gestalten,
    klären wir wann anders. Lass uns erstmal auf die Funktionalitäten beschränken und dann
    weiterschauen. Es wird also nur eine Login-Möglichkeit da sein. Wir haben schon besprochen, dass
    man sich nicht registrieren können soll und nur ein Account mit Admin-Rechten weitere Accounts
    erstellen kann und die Rechte verwaltet. Wo kommen wir nach dem Login hin?
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
    [AU-0088] Speaker 1: Ich denke, es wäre echt sicherer, wenn nur der Admin die Accounts erstellt
    und verwaltet. Das verhindert das unbefugte Zugang erhalten. Wir können die Registrierung intern
    halten, um die Sicherheit und Kontrolle über die App zu bewahren. Wir könnten uns einen Admin-
    Bildschirm überlegen. Also eine Extra-Seite, die nur ein Account mit Admin-Rechten sehen kann.
    Auf dieser Seite können wir dann die Funktionalität einbauen, dass neue Accounts angelegt werden
    können und dafür die drei Rechte Admin, User und Bewohner vergeben werden können. Natürlich
    müssen wir uns später dann noch überlegen, wie wir das mit der Rechteverwaltung lösen.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-062 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Die App soll nach dem Login auf jeder Seite eine Appbar anzeigen.
  ZITIERTE QUELL-UNITS (AU-0054, AU-0120):
    [AU-0054] Speaker 2: Das wird super nützlich sein, um bestimmte Verhaltensweisen oder
    Kommunikationsarten visuell festzuhalten. Wir sollten nicht vergessen, eine Appbar oben im
    Bildschirm zu programmieren. Mit der kann man dann zurücknavigieren oder sich ausloggen über ein
    Logout-Symbol. Diese Appbar sollte nach dem Login auf jeder Seite oben sein. Vielleicht wäre
    auch ein Einstellungssymbol in der Appbar sinnvoll, damit man zu den Einstellungen kommen kann.
    Da müssen wir uns auch noch was überlegen.
    [AU-0120] Speaker 1: Genau. Wenn der Nutzer sich eingeloggt hat, leiten wir ihn auf die
    Profilseite weiter. Nach dem Login sollte auf jeder Seite ganz oben eine Appbar sein. Also eine
    konsistente Leiste, die drei Hauptfunktionalitäten hat. Links auf der Appbar sollte ein nach
    links zeigender Pfeil sein. Klickt man den Pfeil an, navigiert man auf die vorherige Seite
    zurück. In der Mitte der Appbar sollte der Name der aktuellen Seite stehen und rechts ein
    Einstellungssymbol. Wenn man auf das Einstellungssymbol klickt, landet man auf der
    Einstellungsseite. Brauchen wir eine konsistente Appbar an der Spitze?
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-013 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Die fachliche Zielrichtung der Lösung ist, dass Betreuende Bewohner besser verstehen können,
    nicht umgekehrt.
  ZITIERTE QUELL-UNITS (AU-0010, AU-0011, AU-0012):
    [AU-0010] Speaker 1: Perfekt. Da das Thema unterstützende Kommunikation ist, stellen wir uns
    auch die Frage, aus wessen Sicht die Unterstützung kommen soll. Also sollen die Beeinträchtigten
    unterstützt werden, um beispielsweise die Betreuer besser zu verstehen oder andersherum? Ich
    denke, logischerweise wäre es besser, wenn man ein System hätte, das einem hilft, den Bewohner
    besser zu verstehen als andersherum. Aber wie sehen Sie das?
    [AU-0011] Speaker 2: Ja, wenn man sich entscheiden müsste, wäre es auf jeden Fall sinnvoller,
    wenn man einen Weg finden würde, die Bewohner besser zu verstehen.
    [AU-0012] Speaker 1: Wir glauben auch, dass es schwierig wäre, einen Weg zu finden, dass alle
    Beeinträchtigten Techniken oder Systeme nutzen, die es ermöglichen, die Betreuer zu verstehen.
    Dafür müsste man wahrscheinlich für jeden individuell etwas entwickeln. Also haben wir dabei
    einen Weg zu finden, wie der Betreuer jeden Bewohner besser verstehen kann. Was halten Sie von
    der Idee, eine App zu entwickeln, aus der Daten, aus der Akte bzw. allgemeines Wissen der Art
    und Weise, wie eine beeinträchtigte Person kommuniziert festgehalten wird? Diese App wäre für
    jeden, der mit einer Person, die er nicht verstehen kann, eine Lösung, um nachzuschauen, was
    gemeint sein könnte. Die App sollte mehrere Anforderungen erfüllen, die wir noch erarbeiten
    müssen. Aber als Vorschlag könnte die App so aufgebaut sein, dass es einen Teil gibt, der als
    allgemeines Kennenlernenfeld betrachtet werden kann, also wo man quasi draufklickt und durch
    mehrere Bilder mit Texten einfach einen ersten Eindruck der Person bekommt und sie so etwas
    kennenlernt. Wir denken da auch an Hobbys und alles, was die Person etwas näher beschreibt. Die
    weiteren Teile befassen sich dann damit, irgendwie die Kommunikation zu unterstützen. Das muss
    dann noch weiter erarbeitet werden.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-094 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Der Login-Screen soll ein Logo enthalten.
  ZITIERTE QUELL-UNITS (AU-0117, AU-0119):
    [AU-0117] Speaker 2: Ok, lasst uns mit dem Design für den Login-Screen anfangen. Ich finde,
    unser Logo sollte sofort ins Auge fallen, vielleicht im oberen Drittel des Bildschirms
    zentriert. Am besten ein rundes Logo, aber das Logo müssen wir noch erstellen. Es sollte etwas
    mit Kommunikation zu tun haben.
    [AU-0119] Speaker 2: Das macht Sinn. Und der Login-Button sollte direkt unter den zwei Feldern
    sein und hervorstechen. Wir wollen keine Registrierung auf dem Screen, nur den Login, um es
    schlicht zu halten. Also der Login-Screen besteht aus Logo, E-Mail und Passwort-Feld und einem
    Login-Button.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-115 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Zur Barrierefreiheit sollen große Schrift, ausreichender Kontrast und eine zurückhaltende
    Farbverwendung beitragen.
  ZITIERTE QUELL-UNITS (AU-0133):
    [AU-0133] Speaker 2: Bei all diesen Screens sollten wir auch die Barrierefreiheit im Auge
    behalten. Große Schrift, ausreichend Kontrast und die Vermeidung von zu vielen Farben helfen
    dabei, die App für alle zugänglich zu machen.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-111 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten.
  ZITIERTE QUELL-UNITS (AU-0086, AU-0131, AU-0132):
    [AU-0086] Speaker 1: Das klingt praktisch. Wir sollten darüber nachdenken, wie wir diese
    Beschreibungsmuster am besten formulieren. Vielleicht könnten wir sogar ein Tutorial oder eine
    Hilfe-Seite in der App einbauen, die den Nutzern zeigt, wie sie Informationen am effektivsten
    eingeben und suchen können.
    [AU-0131] Speaker 2: Genau. Wir müssen darauf achten, dass die Datenschutzeinstellungen leicht
    zugänglich sind und das Ausloggen sollte immer schnell erreichbar sein. Denkst du, wir sollten
    auch eine Hilfe-Funktion oder ein Tutorial einbauen?
    [AU-0132] Speaker 1: Auf jeden Fall. Eine kurze Tour durch die App beim ersten Login könnte
    helfen, die Nutzer mit den Funktionen vertraut zu machen. Und ein Hilfebereich, den man immer
    wieder aufrufen kann, wäre auch sinnvoll.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-110 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Datenschutzeinstellungen sollen leicht zugänglich sein.
  ZITIERTE QUELL-UNITS (AU-0131):
    [AU-0131] Speaker 2: Genau. Wir müssen darauf achten, dass die Datenschutzeinstellungen leicht
    zugänglich sind und das Ausloggen sollte immer schnell erreichbar sein. Denkst du, wir sollten
    auch eine Hilfe-Funktion oder ein Tutorial einbauen?
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-032 [requirement] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Die About-Me-Seite soll erweiterbar sein, sodass neue Bilder mit Beschreibung hinzugefügt werden
    können.
  ZITIERTE QUELL-UNITS (AU-0025, AU-0033, AU-0047, AU-0048):
    [AU-0025] Speaker 1: Ja, das wäre natürlich auch möglich. Jedoch wäre es wichtig, die App mit
    Grenzen zu betrachten. Je mehr sie leisten muss, desto schwerer wird es, desto länger dauert es
    und ich vermute, desto weniger Freude wird man an dieser App haben. Dennoch werden wir das mit
    in unsere Anforderungen packen und mit dem Leiter des Einrichtung-NoName, der am Ende auch alles
    absegnen muss, klären, ob eine volle Dokumentation von der App übernommen werden soll. Auch wenn
    ich hier schon mal sagen werde, dass es nicht so einen großen Umfang haben sollte, da nicht der
    eigentliche Sinn der unterstützenden Kommunikation verloren gehen soll. Hätten Sie sonst noch
    Vorschläge, was wir mit der App noch machen sollen? Wir haben uns schon mal zwei Punkte
    überlegt, die die App leisten soll. Erstens soll es einen Button geben, der zu einem Feld führt,
    dem wir als About Me Seite ausgearbeitet haben. Also eine Seite, die erstmal für den ersten
    Eindruck der Person sorgt, mit Bildern, Beschreibungen, Hobbys, Name, Alter und so weiter. Die
    Bilder sollen etwas Social Media ähneln und erweiterbar sein. Also man kann immer neue Bilder
    mit einer Beschreibung hinzufügen. Später hat man eine lange Liste, wie zum Beispiel in
    Instagram und kann nach unten scrollen und sich die Bilder anschauen und so etwas über die
    Person erfahren. Daher auch der Name About Me. Wir wollen dadurch erreichen, dass ein erster
    Eindruck entsteht. Natürlich auch für neue Betreuer, die sich im Vorhinein eben ein gutes Bild
    über den Bewohner machen können. Der zweite Button führt zu einem Kommunikationsscreen. Diesen
    wollen wir in verbal und nonverbal unterteilen. Unser Ziel ist da, dann die Daten über das
    Verständnis verschiedener Kommunikationsweisen zu speichern und dann für alle Benutzer jederzeit
    abrufbar zu machen. Und wir haben uns überlegt, da die App immer bei einem in der Hosentasche
    ist, eventuell noch eine Medikamentenvergabe und Terminkalenderseite zu bauen, um immer abrufbar
    zu haben, was die Person braucht oder für Termine hat. Wie regeln Sie das sonst?
    [AU-0033] Speaker 1: Okay, danke für den Input. Ich wiederhole mal. Es wird schon mal drei
    Buttons geben, die zu bestimmten Seiten führen. Einmal about me, also über mich. Diese Seite
    verschafft dem Benutzer einen ersten Eindruck über die Person und bietet ein Infofeld, wo
    bestimmte Informationen gelistet sind. Darunter wird eine Liste von Bildern sein, die an Social
    Media erinnern sollen. Diese Bilder werden mit einer Beschreibung versehen. Es gibt also
    irgendwo, am besten unten in der Mitte, einen Button mit einem Plus-Symbol, welcher einen Dialog
    öffnet. Der es ermöglicht, ein Foto hinzuzufügen und einen Text dafür zu schreiben. Das neueste
    hinzugefügte Foto wird immer oben angezeigt. Die Liste der Fotos wächst also nach oben, sodass
    man immer das aktuelle Bild sehen kann. Ein zweiter Button wird zu einer Seite führen, die eine
    Kommunikationsseite ist. Dort gibt es eine weitere Unterteilung in verbal und nonverbal.
    Wahrscheinlich macht es hier auch Sinn, noch zwei Buttons zu erstellen, die dann zu den
    jeweiligen Seiten navigieren, wenn man draufklickt. Hier werden die verbalen und nonverbalen
    Kommunikationsarten durch Texte, Bilder etc. festgehalten, genauso wie beim About Me Screen.
    Wird auch hier die Möglichkeit sein, diese Liste an Kommunikationsarten dynamisch zu erweitern.
    Also durch einen Button am Boden, der die Seite, der dann wiederum einen Dialog öffnet, in dem
    man Texte schreiben kann und Bilder hinzufügen natürlich. Der dritte Button führt zu einer
    Videoseite. Diese Seite soll den Sinn haben, Videos zu bestimmen. Videos zu bestimmten
    Situationen zu enthalten. Das bedeutet also, dass wenn man ein unbekanntes Verhalten bzw. eine
    unbekannte Kommunikationsart des Beeinträchtigten sieht, man sie dort als Video finden kann.
    Auch wenn man eine neue Situation erkennt und eine neue Erfahrung der Kommunikation macht, soll
    diese hinzugefügt werden können. Wie gewohnt durch einen Button am Ende des Bildschirms, der
    wohl die Funktionalität hat, ein neues Video mit einer Beschreibung hinzuzufügen.
    [AU-0047] Speaker 1: About Me sollte wirklich informativ und persönlich sein. Wir könnten ein
    Informationsfeld mit Hobbys, Namen, Alter und so weiter einrichten. Dazu Bilder mit
    Beschreibungen und natürlich den Plus-Button am unteren Ende des Bildschirms, um neue Bilder
    hinzuzufügen. Das aktuellste Bild sollte immer ganz oben stehen und man kann dann klassisch
    runterscrollen und den Rest anschauen.
    [AU-0048] Speaker 2: Klingt super. Also der Plus-Button soll dann sozusagen ein weiteres Fenster
    öffnen, wo man alles hinzufügen kann, wie zum Beispiel Bilder und die Beschreibung. Diese werden
    dann automatisch ganz oben als neuestes Bild angezeigt und so weiter. Und für die Kommunikation,
    wie teilen wir das auf?
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt

---

### STICHPROBE · C-023 [risk] — mein Urteil: **valid + quellengestützt**

  F-AUSSAGE:
    Vorhandene Dokumentationen werden aus Sicht einer Fachkraft nicht gelesen, weil sie zu
    umfangreich sind und gesuchte Informationen oft schwer auffindbar sind.
  ZITIERTE QUELL-UNITS (AU-0018):
    [AU-0018] Speaker 2: Grundsätzlich wird alles nach einem bestimmten Plan dokumentiert. Auch
    Erfahrungen und neues Wissen wird schriftlich festgehalten. Ob sie gelesen werden, kann ich
    nicht persönlich beantworten, weil ich sie definitiv nicht lese. Es ist einfach zu viel zu lesen
    und oft findet man auch nicht das, wonach man sucht. Aber der Grund dafür, dass ich es nicht
    lese, ist meine Erfahrung.
  - [x] ① gestützt bestätigt    - [x] ② präzise bestätigt
