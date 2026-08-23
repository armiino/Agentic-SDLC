# Anforderungsdokument — einrichtung-fresh

> Version: 16 · Stand: 2026-08-22 21:58 UTC · Core: 237 Items · Fingerabdruck: 17ce06b45604cad7
> Projektion aus der Projektwahrheit (Core) — GENERIERT, nie von Hand pflegen; jede Fassung entspricht exakt einem autorisierten Wahrheits-Stand.

## 1. Funktionale Anforderungen

### Produktziel und Lösungsrahmen (FC-01)

- Die angestrebte Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern bzw. fördern; die genaue Ausgestaltung dieses Produktziels ist noch nicht vollständig konkretisiert. (REQ-01)
- Die Unterstützung soll vorrangig darauf ausgerichtet sein, dass Betreuer oder andere Personen Bewohner besser verstehen können. (REQ-02)
- Als Kernansatz soll eine App Wissen über das Profil und die Kommunikationsweise einer Person bereitstellen, damit Nutzer bei Verständnisschwierigkeiten nachsehen können, was gemeint sein könnte; die konkrete Ausprägung dieses Ansatzes ist noch offen. (REQ-03)
- Ein System, das individuell zwischen Bewohner und Betreuer in beide Richtungen übersetzt, darf nicht Teil des Lösungsansatzes sein. (REQ-04)
- Kommunikation ist im Alltag zentral und das Projekt wird als Weiterentwicklungsmöglichkeit gesehen. (REQ-52)

### Zugang und Rechteverwaltung (FC-02)

- Beim Hinzufügen oder Bearbeiten von Inhalten sollte die App erfassen, welcher Account den Eintrag erstellt oder geändert hat und wann dies geschehen ist. (L3-REQ-001)
- Die App sollte nach einer konfigurierbaren Zeit ohne Aktivität automatisch eine erneute Authentifizierung verlangen, insbesondere auf gemeinsam genutzten Geräten. (L3-REQ-009)
- Passwörter sollten über einen geregelten Prozess durch Admins zurückgesetzt werden können; optional sollte ein sicherer Selbsthilfeprozess geprüft werden, falls organisatorisch gewünscht. (L3-REQ-010)
- Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen. (REQ-06)
- Zusätzlich soll es einen Bewohner-Account geben. Dieser darf nur das eigene Profil in der Profilübersicht sehen und nur eingeschränkte Funktionen nutzen, insbesondere Zugriff auf den About-Me-Bereich und gegebenenfalls das Hinzufügen eigener Bilder. (REQ-07)
- Ob Angehörige selbst Inhalte eintragen dürfen oder nur Leserechte erhalten, muss noch geklärt werden; die Rechtefrage bleibt bis dahin offen. (REQ-08, v2)
- Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf die Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten. (REQ-09)
- Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden. (REQ-10)
- Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können. (REQ-41)
- Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer dürfen dieses Protokoll nicht einsehen. (REQ-65)

### Login und Einstieg (FC-03)

- Nach dem Login sollte die App die zuletzt geöffnete Einrichtung oder den zuletzt betrachteten Bewohner optional schneller wieder zugänglich machen, sofern dies datenschutzkonform ist. (L3-REQ-005)
- Fehlermeldungen bei Login-Problemen sollten verständlich formuliert sein und keine unnötigen sicherheitskritischen Details über existierende Accounts preisgeben. (L3-REQ-011)
- Der Login-Screen soll ein zentriertes, gut sichtbares Logo im oberen Drittel enthalten; ein passendes Kommunikations-Logo ist noch zu erstellen. (REQ-11)
- Der Login-Screen soll E-Mail-Feld, Passwort-Feld und einen Login-Button enthalten; eine Registrierungsmöglichkeit darf dort nicht angeboten werden. (REQ-12)

### Globale Navigation und Hilfe (FC-04)

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. (REQ-14)
- Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und Logout unterstützen; genannt sind ein nach links zeigender Pfeil links, der Name der aktuellen Seite in der Mitte und ein Einstellungssymbol rechts. (REQ-15)
- Bei der Gestaltung soll Barrierefreiheit berücksichtigt werden, insbesondere große Schrift, ausreichender Kontrast und zurückhaltender Farbeinsatz. (REQ-38)
- Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour beim ersten Login und als später erneut aufrufbarer Hilfebereich. (REQ-46)

### Profilübersicht und Profilanlage (FC-05)

- Die Profilübersicht sollte zusätzlich zur Namenssuche eine Sortierung nach Name und optional nach zuletzt aktualisiert unterstützen. (L3-REQ-006)
- Für Bewohnerprofile sollte ein Status vorgesehen werden, der zwischen aktiv, archiviert und gegebenenfalls in Vorbereitung unterscheidet. (L3-REQ-007)
- Beim Anlegen eines neuen Bewohnerprofils sollte geprüft werden, ob ein Profil mit gleichem oder sehr ähnlichem Namen bereits existiert, um Dubletten zu vermeiden. (L3-REQ-008)
- Bei Benachrichtigungen und Übersichten sollte klar erkennbar sein, welche Inhalte seit dem letzten Besuch neu oder aktualisiert sind. (L3-REQ-013)
- Nach dem Login soll eine Profilübersicht angezeigt werden. (REQ-13)
- Auf der Profilübersicht soll eine anklickbare Liste der sichtbaren Bewohnerprofile angezeigt werden; jedes Profil führt zu einer weiteren Seite mit den Daten des gewählten Profils. (REQ-16)
- Auf der Profilübersicht muss eine Suchleiste vorhanden sein, um Profile schnell nach Namen zu finden. (REQ-17)
- Profile sollen in der Übersicht mit kleinem Vorschaubild, Name und kurzer Beschreibung dargestellt werden; ob dies als Liste oder als Kacheln erfolgt, muss noch geklärt werden. (REQ-18)
- In der Profilübersicht soll erwogen werden, das Anlegen neuer Profile per Plus-Symbol und Dialog für Bild, Name und Beschreibung zu ermöglichen. (REQ-45)
- Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein. (REQ-59)
- Zusätzlich zur bestehenden Sortierung nach Datum soll die Liste auch nach Dringlichkeit sortierbar sein. (REQ-78)

### Profil-Detail und Schnellzugriff (FC-06)

- Der Sofortinfo-Bereich in der Profil-Detailansicht muss auf maximal fünf Einträge begrenzt werden. (L3-REQ-002, v2)
- Die Detailansicht eines Profils soll das gewählte Profilbild größer anzeigen und die Hauptbereiche der App als interaktive Buttons anbieten; der genaue Zuschnitt beziehungsweise die genaue Zahl der Hauptbereiche ist noch nicht stabil. (REQ-19)
- Die App muss für jeden Bewohner eine About-Me-Seite bereitstellen, die für den ersten Eindruck der Person dient und persönliche Kurzinfos enthält, einschließlich Bildern sowie beschreibender Informationen wie Name, Alter und Hobbys. (REQ-20)
- Die App muss eine Kommunikationsansicht bereitstellen, die klar in verbale und nonverbale Kommunikation unterteilt ist. (REQ-24)
- No-Go-Einträge müssen bewohnerbezogen geführt werden und dürfen nicht global für alle Bewohner gelten. (REQ-32, v2)

### About-Me-Profilbereich (FC-07)

- Die App sollte Medienuploads vor dem Speichern automatisch komprimieren oder in geeignete Auflösungen umwandeln, um Speicherbedarf und Ladezeiten zu begrenzen. (L3-REQ-003)
- Für Medieninhalte sollte geprüft werden, ob Untertitel, Transkripte oder kurze Textzusammenfassungen unterstützt werden können, um Zugänglichkeit und Suchbarkeit zu verbessern. (L3-REQ-012)
- Die About-Me-Ansicht muss eine Foto-Timeline mit Beschreibungen bereitstellen. (REQ-21)
- Die Foto-Timeline muss dynamisch erweiterbar sein; neue Einträge müssen über einen Plus-Button hinzugefügt werden können. (REQ-22)
- Neu hinzugefügte Bilder müssen in der About-Me-Timeline automatisch ganz oben als neueste Einträge angezeigt werden. (REQ-23)
- Vor der Nutzung von Bildern in der App müssen Datenschutzfragen und Einwilligungen der Angehörigen beziehungsweise Berechtigten geklärt werden; dies gilt auch für Testbilder. (REQ-35)
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist als spätere mögliche Erweiterung vorgesehen. (REQ-40)
- Die About-Me-Seite darf im MVP in ihrer aktuellen Form nicht verändert werden. (REQ-66)

### Kommunikationswissen (FC-08)

- Die Trennung zwischen verbaler und nonverbaler Kommunikation soll visuell klar erkennbar sein, unter anderem durch Kennzeichnung mit Symbolen. (REQ-25)
- Einträge zu Kommunikationsweisen müssen dynamisch erweiterbar sein; neue Erfahrungen müssen hinzugefügt werden können. (REQ-26)
- Kommunikationsweisen müssen nicht nur als Text, sondern auch mit Bildern und weiteren Darstellungsformen erfasst und angezeigt werden können. (REQ-27)
- Auf den Kommunikationsseiten soll eine Suchfunktion vorgesehen werden; dafür muss ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit später besser gesucht und gefiltert werden kann. (REQ-28)
- Videos von Kommunikationssituationen müssen mit Beschreibungen erfasst werden können. (REQ-29)
- Die Videofunktionalität muss in die Kommunikationsseiten integriert sein und darf nicht als separater Screen umgesetzt werden. (REQ-30)
- Neue Videos sollen auf den Kommunikationsseiten per Plus-Button hinzugefügt werden können; neueste Videos sollen oben angezeigt werden. (REQ-31)

### No-Go-Wissen (FC-09)

- Die No-Go-Seite muss eine dynamisch erweiterbare Liste bieten, in die neue No-Gos per Plus-Button hinzugefügt werden können. (REQ-33)
- Die No-Go-Einträge sollen einfach zu durchforsten sein und nur die wichtigsten Informationen enthalten. (REQ-34)
- Für die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal ergänzt werden, damit kritische Inhalte sofort erkennbar sind. (REQ-56)
- No-Go-Einträge müssen nachträglich bearbeitet und gelöscht werden können. (REQ-57)
- Für die Anlage von No-Gos kann optional eine Vorlagen-Liste mit häufigen No-Gos angeboten werden; daraus darf jedoch nichts automatisch global gelten. (REQ-58)

### Medien, Datenschutz und Datenhaltung (FC-10)

- Bei offline bearbeiteten Konfliktfällen muss eine klare Konfliktanzeige erfolgen; nichts darf stillschweigend überschrieben werden, und der Nutzer entscheidet, welche Version gilt. (L3-REQ-004, v2)

### Plattform und Barrierefreiheit (FC-11)

- Die App muss plattformübergreifend auf iPhone und Android laufen. (REQ-37)
- Es soll evaluiert werden, ob die App zusätzlich auf Tablets nutzbar sein kann. (REQ-39)
- Alternative Eingabemethoden wie Sprachbefehle sollen als mögliche spätere Accessibility-Erweiterung berücksichtigt werden. (REQ-43)
- Animationen sollen nicht im Fokus stehen; falls sie verwendet werden, dürfen sie nicht ablenkend sein. (REQ-47)
- Für die Planung soll aktuell von einer Mindestunterstützung ab Android 10 ausgegangen werden; die finale Festlegung muss noch mit dem Träger geklärt werden. (REQ-67)
- Falls im Haus Geräte unter Android 10 vorhanden sind, besteht ein Risiko durch nötigen Geräteaustausch; dies ist vom Gerätebestand und der Abstimmung mit dem Träger abhängig. (REQ-68)
- Eine einstellbare Schriftgröße ist ein Wunsch zur Unterstützung älterer Kolleginnen, mit niedriger Priorität und eher späterer Umsetzung. (REQ-75)

### Dokumentationsintegration und Erweiterungen (FC-12)

- Es sollte geprüft werden, ob bestehende Informationen aus vorhandenen Akten teilweise strukturiert in die App übernommen werden können, um Doppeldokumentation zu reduzieren. (L3-REQ-014)
- Bei der Erfassung einer Medikamenten-Gabe sollen Pflegende optional eine kurze einnahmebezogene Bemerkung als Freitext mit maximal 200 Zeichen erfassen können. Wenn eine Bemerkung vorhanden ist, soll sie in der Übersicht der Medikamenten-Einnahmen für freigegebene Angehörige mit angezeigt werden. Die Bemerkung ist nur bei der Erfassung der Gabe angebbar. (REQ-42, v3)
- Eine Ausweitung der App auf weitere Dokumentationsfunktionen soll geprüft werden, jedoch nur in begrenztem Umfang und ohne den Fokus auf unterstützende Kommunikation zu verlieren. (REQ-44)
- Zu den Bewohnern existieren bereits klassische Akten und Dokumentationen, in denen Informationen, Erfahrungen und neues Wissen nach einem bestimmten Plan schriftlich festgehalten werden. (REQ-48)
- Es muss berücksichtigt werden, dass vorhandene Akten im Arbeitsalltag schwer nutzbar sein können, weil sie umfangreich sind und gesuchte Informationen nicht schnell gefunden werden; dies wurde jedoch nur als persönliche Einschätzung geäußert. (REQ-49)
- Übergabe-Notizen sollen bewohnerbezogen verlinkbar sein, sodass direkt auf das Profil eines Bewohners verwiesen werden kann. (REQ-62)
- Die Leitung soll wöchentlich einen PDF-Export aller Übergabe-Notizen der Woche erstellen können, um sie für Träger-Berichte abzulegen. (REQ-77)
- Die Einnahmen-Übersicht für Angehörige muss als Monatsübersicht der letzten dreißig Tage bereitgestellt werden; die bisherige Wochenansicht mit sieben Tagen darf dafür nicht weiter gelten. (REQ-80)
- Pflegende müssen am Ende ihrer Schicht eine Tages-Zusammenfassung aller von ihnen dokumentierten Medikamenten-Gaben sehen können, damit sie vor der Übergabe prüfen können, ob alles erfasst ist. (REQ-81)
- Übergabe-Notizen werden nach dreißig Tagen automatisch archiviert; die bisherige Vierzehn-Tage-Regel gilt nicht mehr. Archivierte Notizen bleiben weiterhin auffindbar. (REQ-85)
- Pflegende müssen in der Übersicht der Medikamenten-Einnahmen überfällige, noch nicht dokumentierte Gaben deutlich rot hervorgehoben sehen. (REQ-86)
- Übergabe-Notizen müssen bis zur Archivierung bearbeitet werden können; bei jeder Bearbeitung ist die zuletzt gültige Fassung mit Änderungszeitpunkt und bearbeitendem Account nachvollziehbar zu halten. (REQ-92)

### Forschung, Anforderungsanalyse und Pilotbewertung (FC-13A)

- Für den Pilotbetrieb sollten messbare Erfolgskriterien definiert werden, etwa schnellere Auffindbarkeit relevanter Informationen oder verbesserte Verständigung in ausgewählten Situationen. (L3-REQ-015)
- Die genauen Bedürfnisse und Funktionen des Systems müssen durch Anforderungsanalyse mit mehreren Beteiligten und künftigen Nutzern erarbeitet werden. (REQ-50)
- Zur Nutzerforschung müssen Vor-Ort-Termine in Einrichtungen durchgeführt werden, um reale Kommunikationssituationen und Bedürfnisse besser zu verstehen. (REQ-51)

### Technologie- und Architekturentscheidungen (FC-13B)

- Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen. (REQ-53)
- Das Paket GetX soll als technische Option berücksichtigt und in der Umsetzung erprobt werden. (REQ-54)

### Wochenübersicht und Angehörigenbeiträge (FC-14)

- Angehörige sollen zu Einträgen eigene Fotos hochladen können; pro Eintrag sind maximal fünf Fotos zulässig, und es dürfen nur JPG- oder PNG-Formate akzeptiert werden. (REQ-79, v2)

### Besuchsankündigung und Besuchskoordination (FC-15)

- Angehörige müssen Besuche bei ihrer Bezugsperson vorab in der App mit Datum und Uhrzeit ankündigen können. (REQ-82)
- Pflegende der Einrichtung müssen eine Übersicht der angekündigten Besuche sehen können. (REQ-83)
- Pflegende der jeweiligen Einrichtung müssen angekündigte Besuche bestätigen oder ablehnen können; bei Ablehnung ist eine kurze Begründung Pflicht. Angehörige sehen zu ihrer Ankündigung genau einen Status: angefragt, bestätigt oder abgelehnt. (REQ-87)
- Angehörige sollen einmal am Vortag um 18 Uhr per Push-Mitteilung an ihren bestätigten Besuch erinnert werden, damit Besuche nicht vergessen werden. (REQ-88, v2)
- Die Übersicht der angekündigten Besuche soll die Besuche der nächsten 14 Tage anzeigen. (REQ-89)
- Bereits erledigte oder nicht mehr aktive angekündigte Besuche sollen in der Übersicht durchgestrichen und ausgegraut dargestellt werden. (REQ-90)
- Angehörige müssen eine bereits angefragte oder bestätigte Besuchsankündigung bis zum Beginn des Besuchs ändern oder absagen können; nach einer Änderung muss der Besuch erneut den Status „angefragt“ erhalten, und nach einer Absage darf keine Erinnerung mehr versendet werden. (REQ-91)

### Ohne Feature-Zuordnung

- Es muss eine Suche nach Stichworten über alle Bewohnerprofile hinweg geben. (REQ-60)
- Es soll eine Übergabe-Notiz pro Schicht geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird. (REQ-61)
- Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat. (REQ-64)
- Zum Thema Piktogramme besteht ein noch nicht ausreichend konkretisierter Änderungswunsch; vor einer Umsetzung muss präzisiert werden, was mit „größer denken“ gemeint ist. (REQ-69)
- Firebase Firestore ist als Persistenztechnologie festgelegt; Alternativen werden nicht weiter diskutiert. (REQ-70)
- Es ist unklar, ob aus dem Hinweis auf die anstehende Übergabe eine eigenständige fachliche Anforderung zur Unterstützung schneller Übergabeprozesse abgeleitet werden soll. (REQ-71)
- Es ist unklar, welcher zuvor genannte Punkt aus Sicht der Angehörigen besonders wichtig ist und ob daraus ein eigener Claim folgt. (REQ-72)
- Es ist zu klären, ob Aufgabenverfolgung oder Terminplanung als eigene organisatorische Anforderungen an das Projekt festgehalten werden sollen. (REQ-73)
- Ein Dunkelmodus ist ein Wunsch, insbesondere für den Nachtdienst, mit niedriger Priorität und eher späterer Umsetzung. (REQ-74)
- Jeder Account und jedes Bewohnerprofil ist genau einer Einrichtung zugeordnet; diese Zuordnung wird serverseitig erzwungen. Einladungscodes geben die Einrichtungszuordnung bei der Registrierung unveränderlich mit. Die Selbstregistrierung für Angehörige per Einladungscode bleibt bestehen; für alle anderen Rollen bleibt der Zugang ausschließlich über intern vergebene Accounts möglich. (REQ-84, v2)

## 2. Nicht-funktionale Anforderungen

> Enthält die kategorisierten Items (`analystKategorie`); die Kategorisierung des Alt-Bestands steht aus — unkategorisierte Anforderungen stehen in Abschnitt 1 bei ihrem Feature.

### security

- Lokal auf dem Gerät zwischengespeicherte Bewohnerdaten und Medien müssen im Ruhezustand verschlüsselt gespeichert werden; nach Abmeldung oder Entzug der Berechtigung dürfen lokal vorhandene Daten dieses Kontexts nicht mehr lesbar sein. (REQ-36, v2)

## 3. Technische Rahmenbedingungen

- Die Unterstützung soll vorrangig darauf ausgerichtet sein, dass Betreuer oder andere Personen Bewohner besser verstehen können. (ARCH-02)
- Als Kernansatz soll die App Wissen über Profil und Kommunikationsweise einer Person bereitstellen, damit Nutzer bei Verständnisschwierigkeiten nachsehen können; die konkrete Ausprägung dieses Ansatzes ist noch weiter auszuarbeiten. (ARCH-03)
- Ein System, das individuell zwischen Bewohner und Betreuer in beide Richtungen übersetzt, ist als Lösungsansatz ausgeschlossen. (ARCH-04)
- Bestehende klassische Akten und Dokumentationen zu Bewohnern sind als Bestandssituation zu berücksichtigen. (ARCH-05)
- Die App darf keine Selbstregistrierung erlauben; Zugang erfolgt nur per Login mit intern vergebenen Accounts. (ARCH-07)
- Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen. (ARCH-08)
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten und Inhalte beziehungsweise Wissen beitragen können; unterschiedliche Rechte sind dabei vorgesehen, aber noch nicht konkret ausformuliert. (ARCH-09)
- Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil sehen darf und nur eingeschränkte Funktionen nutzen kann, insbesondere Zugriff auf About Me und gegebenenfalls das Hinzufügen eigener Bilder. (ARCH-10)
- Für die einrichtungsgebundene Stichwortsuche über alle Bewohnerprofile muss ein serverseitiger Suchrahmen festgelegt werden: Suchindizes dürfen nur Inhalte der jeweils zugeordneten Einrichtung enthalten oder nur innerhalb dieser Einrichtung abfragbar sein, damit Suchtreffer die Einrichtungsgrenze technisch nicht umgehen. (ARCH-11)
- Vor Nutzung von Bildern in der App müssen Datenschutzfragen und Einwilligungen der Angehörigen beziehungsweise Berechtigten geklärt werden, auch für Testbilder. (ARCH-14)
- Nach dem Login soll eine Profilübersicht mit anklickbarer Liste der sichtbaren Bewohnerprofile angezeigt werden. (ARCH-15)
- Auf der Profilübersicht soll eine Suchleiste vorhanden sein, um Profile schnell nach Namen zu finden. (ARCH-16)
- Profile sollen in der Übersicht mit Vorschaubild, Name und Kurzbeschreibung dargestellt werden; ob dies als Liste oder Kacheln erfolgt, ist noch offen. (ARCH-17)
- Die Detailansicht eines Profils soll das Profilbild größer zeigen und die Hauptbereiche der App als interaktive Buttons anbieten; der genaue Zuschnitt dieser Hauptbereiche ist noch nicht stabil. (ARCH-18)
- Die App soll je Bewohner eine About-Me-Seite mit persönlicher Kurzinfo, Bildern und beschreibenden Informationen für den ersten Eindruck bereitstellen. (ARCH-19)
- Die About-Me-Ansicht soll eine dynamisch erweiterbare Foto-Timeline mit Beschreibungen bereitstellen, bei der neue Einträge per Plus-Button hinzugefügt werden und die neuesten oben erscheinen. (ARCH-20)
- Die App soll eine Kommunikationsansicht mit klarer Unterteilung in verbale und nonverbale Kommunikation bereitstellen. (ARCH-21)
- Einträge zu Kommunikationsweisen müssen dynamisch erweiterbar sein und sollen nicht nur als Text, sondern auch mit Bildern und weiteren Darstellungsformen erfasst und angezeigt werden können. (ARCH-22)
- Videos von Kommunikationssituationen sollen mit Beschreibungen erfasst werden können und als Teil der Kommunikationsseiten statt als separater Screen integriert sein. (ARCH-23)
- Auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisiertes Beschreibungsmuster für Kommunikationseinträge definiert werden, damit Suche und spätere Filterung funktionieren. (ARCH-24)
- Die App soll eine No-Go-Seite mit dynamisch erweiterbarer Liste bereitstellen, auf der kritische Dinge festgehalten werden, die in Gegenwart des Bewohners vermieden werden müssen. (ARCH-25)
- Eine Kalenderfunktion soll erwogen werden; zusätzlich ist zu prüfen, ob auch Medikamentengaben integriert werden sollen, wobei die Vertraulichkeit dieser Daten besonders zu berücksichtigen ist. (ARCH-26)
- Es soll erwogen werden, in der Profilübersicht das Anlegen neuer Profile per Plus-Symbol und Dialog für Bild, Name und Beschreibung zu ermöglichen. (ARCH-27)
- Eine Ausweitung der App auf weitere Dokumentationsfunktionen soll geprüft werden, jedoch nur in begrenztem Umfang und ohne den Fokus auf unterstützende Kommunikation zu verlieren. (ARCH-28)
- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. (ARCH-29)
- Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und Logout unterstützen. (ARCH-30)
- Bei der Gestaltung soll auf Barrierefreiheit geachtet werden, insbesondere große Schrift, ausreichender Kontrast und zurückhaltender Farbeinsatz. (ARCH-31)
- Alternative Eingabemethoden wie Sprachbefehle sollen als mögliche Accessibility-Erweiterung berücksichtigt werden. (ARCH-32)
- Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour beim ersten Login und als später erneut aufrufbarer Hilfebereich. (ARCH-34)
- Animationen sollen nicht im Fokus stehen; falls sie verwendet werden, dürfen sie nicht ablenkend sein. (ARCH-35)
- Die App soll plattformübergreifend auf iPhone und Android laufen. (ARCH-36)
- Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen. (ARCH-37)
- Es soll evaluiert werden, ob die App zusätzlich auf Tablets nutzbar sein kann. (ARCH-41)
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden. (ARCH-42)
- Die genauen Bedürfnisse und Funktionen des Systems müssen durch Anforderungsanalyse mit mehreren Beteiligten und künftigen Nutzern erarbeitet werden. (ARCH-43)
- Zur Nutzerforschung sollen Vor-Ort-Termine in Einrichtungen durchgeführt werden, um reale Kommunikationssituationen und Bedürfnisse besser zu verstehen. (ARCH-44)
- Für Push-Benachrichtigungen an Angehörige und Pflegende ist Firebase Cloud Messaging als technische Umsetzung festgelegt. (ARCH-46)
- Für die revisionssichere Protokollierung von Einsichtnahmen muss ein unveränderlicher Audit-Log-Rahmen festgelegt werden: Zugriffsereignisse werden serverseitig als append-only protokolliert, nachträgliche Änderungen oder Löschungen sind fachlich und technisch ausgeschlossen, und das Leserecht auf diese Protokolle bleibt strikt auf Admins beschränkt. (ARCH-47)
- Für Besuchserinnerungen am Vortag um 18 Uhr muss ein serverseitiger Benachrichtigungsrahmen festgelegt werden: Die Erinnerung wird aus dem bestätigten Besuchsstatus heraus zentral geplant, bei Statusänderung oder Absage wieder zurückgezogen und darf nicht von der lokalen Verfügbarkeit oder den Hintergrundrestriktionen des Geräts der Angehörigen abhängen. (ARCH-48)
- Ergänzend zu ARCH-11 gilt für die allgemeine serverseitige Durchsetzung der Einrichtungsgrenze: Jede Collection trägt die Einrichtungs-ID, und jede Firestore Security Rule prüft diese ID gegen die dem angemeldeten Benutzer zugeordnete Einrichtungs-ID. (ARCH-50)

_Voller Architektur-Stand inkl. Entscheidungen (ADRs): docs/architecture.md_

## 4. Offene Entscheidungen und Fragen

- Sollen Übergabe-Notizen, etwa als PDF, für die Pflegedokumentation exportierbar sein? (DEC-001)
- Sollen Pfleger-Accounts zeitlich befristet sein? (DEC-002)
- Wie lange sollen einnahmebezogene Bemerkungen bei der Medikamenten-Gabe aufbewahrt werden? (DEC-004)
- Es muss geklärt werden, ob Angehörige ihre Push-Erinnerungen selbst deaktivieren können sollen. (DEC-006)
- Wie wird das Missbrauchsrisiko von Einladungscodes für Angehörige begrenzt, insbesondere hinsichtlich Weitergabe, Mehrfachnutzung, Ablauf, falscher Zuordnung zur Einrichtung und nachträglicher Sperrung bereits damit erzeugter Zugänge? (DEC-007)
- Wo und wie werden Bilder und Videos für About Me und Kommunikationsseiten technisch gespeichert und ausgeliefert? (DEC-011)
- SQL Injection beim Login. (DEC-012)
- Wie können Angehörige ihre bereits angekündigten Besuche in der App selbst einsehen und den aktuellen Status eines Besuchs nachvollziehen, insbesondere ob er noch ausstehend, bestätigt, abgelehnt oder bereits erledigt ist? (DEC-013)
