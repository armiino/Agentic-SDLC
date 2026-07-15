# Canonical Requirements Baseline

Baseline: `baseline-20260715_162556`
Project: `evidenz-agent-demo`
Created UTC: `2026-07-15T16:25:56.0318950Z`
Requirements: `69`
Open decisions: `23`

## active

### CAN-REQ-002 - Für Mitarbeiter-, Angehörigen-, Bewohner- und Admin-Accounts ist ein vollständiges...

Für Mitarbeiter-, Angehörigen-, Bewohner- und Admin-Accounts ist ein vollständiges Berechtigungskonzept zu spezifizieren, das je Rolle pro Funktionsbereich festlegt, wer Profile sehen, Inhalte anlegen, bearbeiten, freigeben, exportieren, verbergen oder löschen darf; dabei ist auch der einrichtungsbezogene Zugriff über Häusergrenzen hinweg eindeutig zu regeln.

Source items: `L3-REQ-002`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-003 - Für die Erfassung von Kommunikationsweisen, No-Go-Inhalten, About-Me-Daten, Bildern und...

Für die Erfassung von Kommunikationsweisen, No-Go-Inhalten, About-Me-Daten, Bildern und Kalendereinträgen sind Pflichtfelder, zulässige Formate, maximale Feldlängen sowie Verhalten bei unvollständigen oder leeren Eingaben zu definieren; insbesondere muss für Kommunikations-Einträge ein einheitliches Beschreibungsschema vorgegeben werden, damit Suche und Filterung zuverlässig funktionieren.

Source items: `L3-REQ-003`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-013 - Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere...

Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere damit Betreuer bei Verständnisproblemen nachschauen können, was mit einer Kommunikationsweise gemeint sein könnte.

Source items: `REQ-01`
Origin: `Extracted`

### CAN-REQ-015 - Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen...

Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll auch in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind und sich verständigen müssen.

Source items: `REQ-03`
Origin: `Extracted`

### CAN-REQ-016 - Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie...

Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzende Daten und Wissen hinzufügen können, die den Mitarbeitern sonst erst bekannt werden müssten.

Source items: `REQ-04`
Origin: `Extracted`

### CAN-REQ-017 - Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil in der...

Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann und nur eingeschränkte Funktionen erhält, insbesondere Zugriff auf den About-Me-Bereich und das Hinzufügen eigener Bilder; dies ist als gewünschte Anforderung vorgesehen, um Fehlbedienungen zu minimieren.

Source items: `REQ-05`
Origin: `Extracted`

### CAN-REQ-018 - Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller...

Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen.

Source items: `REQ-06`
Origin: `Extracted`

### CAN-REQ-019 - Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile...

Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, statt lange scrollen zu müssen.

Source items: `REQ-07`
Origin: `Extracted`

### CAN-REQ-024 - Die App soll pro Person eine About-Me-Seite bereitstellen, die für den ersten Eindruck...

Die App soll pro Person eine About-Me-Seite bereitstellen, die für den ersten Eindruck informativ und persönlich ist und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält.

Source items: `REQ-12`
Origin: `Extracted`

### CAN-REQ-025 - Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue...

Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können.

Source items: `REQ-13`
Origin: `Extracted`

### CAN-REQ-027 - Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in...

Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht.

Source items: `REQ-15`
Origin: `Extracted`

### CAN-REQ-028 - Die No-Go-Seite soll über einen Plus-Button dynamisch erweiterbar sein, sodass neue...

Die No-Go-Seite soll über einen Plus-Button dynamisch erweiterbar sein, sodass neue No-Gos als Liste hinzugefügt werden können, über die sich Nutzer im Vorhinein informieren können.

Source items: `REQ-16`
Origin: `Extracted`

### CAN-REQ-030 - Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche...

Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden.

Source items: `REQ-18`
Origin: `Extracted`

### CAN-REQ-032 - Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese...

Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen.

Source items: `REQ-20`
Origin: `Extracted`

### CAN-REQ-033 - Auf der Videofunktion sollen neue Videos mit Beschreibungen leicht hinzufügbar sein, und...

Auf der Videofunktion sollen neue Videos mit Beschreibungen leicht hinzufügbar sein, und das neueste Video soll jeweils oben angezeigt werden.

Source items: `REQ-21`
Origin: `Extracted`

### CAN-REQ-035 - Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss jedoch...

Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss jedoch noch ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich sind.

Source items: `REQ-23`
Origin: `Extracted`

### CAN-REQ-037 - Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; sie soll...

Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; sie soll auch ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist.

Source items: `REQ-25`
Origin: `Extracted`

### CAN-REQ-039 - Für den Login soll es nur eine Login-Möglichkeit geben; eine Selbstregistrierung soll...

Für den Login soll es nur eine Login-Möglichkeit geben; eine Selbstregistrierung soll nicht vorgesehen sein.

Source items: `REQ-27`
Origin: `Extracted`

### CAN-REQ-040 - Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen...

Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen; eine Registrierung soll auf diesem Screen nicht vorhanden sein.

Source items: `REQ-28`
Origin: `Extracted`

### CAN-REQ-041 - Für den Login-Screen ist als gewünschte Anforderung vorgesehen, dass das Logo im oberen...

Für den Login-Screen ist als gewünschte Anforderung vorgesehen, dass das Logo im oberen Drittel des Bildschirms zentriert und gut sichtbar platziert wird; das Logo selbst ist noch zu erstellen und soll etwas mit Kommunikation zu tun haben.

Source items: `REQ-29`
Origin: `Extracted`

### CAN-REQ-042 - Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, optionale Unterstützung gewünscht...

Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, optionale Unterstützung gewünscht, etwa als kurze Tour beim ersten Login sowie als erneut aufrufbarer Hilfebereich.

Source items: `REQ-30`
Origin: `Extracted`

### CAN-REQ-043 - Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen als spätere optionale...

Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen als spätere optionale Funktion alle verbundenen Nutzer derselben Einrichtung per Popup benachrichtigt werden.

Source items: `REQ-31`
Origin: `Extracted`

### CAN-REQ-044 - Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen...

Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden.

Source items: `REQ-32`
Origin: `Extracted`

### CAN-REQ-045 - Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen.

Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen.

Source items: `REQ-33`
Origin: `Extracted`

### CAN-REQ-046 - Nur ein Account mit Admin-Rechten darf weitere Accounts anlegen und Rechte verwalten.

Nur ein Account mit Admin-Rechten darf weitere Accounts anlegen und Rechte verwalten.

Source items: `REQ-34`
Origin: `Extracted`

### CAN-REQ-047 - User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen und keine...

User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen und keine hinzugefügten Daten aus der App löschen.

Source items: `REQ-35`
Origin: `Extracted`

### CAN-REQ-048 - Die App ist nur für den internen Gebrauch vorgesehen.

Die App ist nur für den internen Gebrauch vorgesehen.

Source items: `REQ-36`
Origin: `Extracted`

### CAN-REQ-049 - Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile...

Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten.

Source items: `REQ-37`
Origin: `Extracted`

### CAN-REQ-050 - Für den Settings-Screen ist festzuhalten, dass die Ansicht rollenabhängig differenziert...

Für den Settings-Screen ist festzuhalten, dass die Ansicht rollenabhängig differenziert sein soll: Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen, während jeder Nutzer sein Profil ändern und Einstellungen wie die Sprache wählen können soll.

Source items: `REQ-38`
Origin: `Extracted`

### CAN-REQ-051 - Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch...

Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen.

Source items: `REQ-39`
Origin: `Extracted`

### CAN-REQ-052 - Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift...

Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung von zu vielen Farben.

Source items: `REQ-40`
Origin: `Extracted`

### CAN-REQ-053 - Die App soll plattformübergreifend auf iPhone/iOS und Android laufen.

Die App soll plattformübergreifend auf iPhone/iOS und Android laufen.

Source items: `REQ-41`
Origin: `Extracted`

### CAN-REQ-054 - Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch...

Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden.

Source items: `REQ-42`
Origin: `Extracted`

### CAN-REQ-055 - Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen...

Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein.

Source items: `REQ-43`
Origin: `Extracted`

### CAN-REQ-056 - Ein System, das als Schnittstelle zwischen Bewohnern und Betreuern hin und her...

Ein System, das als Schnittstelle zwischen Bewohnern und Betreuern hin und her übersetzt, ist ausdrücklich nicht Teil des Lösungsumfangs.

Source items: `REQ-44`
Origin: `Extracted`

### CAN-REQ-058 - Für Bewohner existieren klassische Akten sowie planmäßig dokumentierte Erfahrungen und...

Für Bewohner existieren klassische Akten sowie planmäßig dokumentierte Erfahrungen und neues Wissen; diese dokumentierten Informationen sind als relevante Wissensquelle für die Lösung zu berücksichtigen.

Source items: `REQ-46`
Origin: `Extracted`

### CAN-REQ-059 - Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen...

Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und Wissen zur Kommunikation wird neuen Mitarbeitern daher häufig mündlich erklärt. Dieses Risiko ist bei der Ausgestaltung der Lösung zu berücksichtigen.

Source items: `REQ-47`
Origin: `Extracted`

### CAN-REQ-060 - Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder...

Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden.

Source items: `REQ-48`
Origin: `Extracted`

### CAN-REQ-061 - Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich...

Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden.

Source items: `REQ-49`
Origin: `Extracted`

### CAN-REQ-062 - Vor der konkreten Ausgestaltung der Lösung soll als gewünschte Vorgehensweise eine...

Vor der konkreten Ausgestaltung der Lösung soll als gewünschte Vorgehensweise eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Verständnis zu gewinnen.

Source items: `REQ-50`
Origin: `Extracted`

### CAN-REQ-063 - Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem...

Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden.

Source items: `REQ-51`
Origin: `Extracted`

### CAN-REQ-064 - Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch...

Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden.

Source items: `REQ-52`
Origin: `Extracted`

### CAN-REQ-065 - Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die...

Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen.

Source items: `REQ-53`
Origin: `Extracted`

### CAN-REQ-066 - Das Team präferiert, möglichst dieselbe Entwicklungsumgebung zu nutzen, und bevorzugt...

Das Team präferiert, möglichst dieselbe Entwicklungsumgebung zu nutzen, und bevorzugt dafür Android Studio.

Source items: `REQ-54`
Origin: `Extracted`

### CAN-REQ-067 - Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren...

Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um spätere Integrationsprobleme zu vermeiden.

Source items: `REQ-55`
Origin: `Extracted`

### CAN-REQ-068 - Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet...

Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden.

Source items: `REQ-56`
Origin: `Extracted`

## open_decision

### CAN-REQ-001 - Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein...

Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile der formalen Pflegedokumentation übernehmen soll; für beide Varianten ist eine klare Scope-Abgrenzung einschließlich expliziter Nicht-Ziele zu dokumentieren.

Source items: `L3-REQ-001`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-004 - Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue...

Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue oder geänderte Beiträge von Mitarbeitern, Angehörigen oder Bewohnern müssen definierte Zustände wie Entwurf, eingereicht, freigegeben, zurückgewiesen oder archiviert durchlaufen, und bei gleichzeitigen Änderungen ist ein Konfliktverhalten einschließlich Versionserhalt festzulegen.

Source items: `L3-REQ-004`
Origin: `HUMAN_ACCEPTED_OPEN_WORLD`

### CAN-REQ-005 - Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für...

Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte Aufbewahrungsdauer, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen sowie einen nachvollziehbaren Export pro Bewohner oder Einrichtung regelt.

Source items: `L3-REQ-005`
Origin: `HUMAN_ACCEPTED_OPEN_WORLD`

### CAN-REQ-006 - Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte...

Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte verbindlich vorgeschrieben sind; mindestens sind Regeln für abgesicherte Authentifizierung, serverseitig erzwungene Rollen- und Einrichtungstrennung sowie Protokollierung sicherheitsrelevanter Zugriffe und Änderungen für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe festzulegen.

Source items: `L3-REQ-006`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-007 - Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus...

Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen übernommen werden; dafür sind Quelle, Datenumfang, Übertragungsweg, Verantwortlichkeit und der Umgang mit Medienbrüchen oder manueller Doppelpflege festzulegen.

Source items: `L3-REQ-007`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-008 - Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder...

Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder instabiler Internetverbindung verbindlich unterstützt werden muss; dabei sind lokale Zwischenspeicherung, Synchronisationszeitpunkte, Konfliktauflösung nach Wiederverbindung sowie Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb festzulegen.

Source items: `L3-REQ-008`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-009 - Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such-...

Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such- und Ladezeiten bei Profilen, erwartete Anzahl gleichzeitiger Nutzer pro Einrichtung, Anzahl verwalteter Bewohnerprofile sowie akzeptable Mediengrößen für Bilder und Videos.

Source items: `L3-REQ-009`
Origin: `HUMAN_ACCEPTED_OPEN_WORLD`

### CAN-REQ-010 - Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben...

Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben Smartphones auch Tablets unterstützen muss, in welchen Arbeitssituationen die App bedienbar sein soll und welche konkreten Bedienbarkeits- und Barrierefreiheitskriterien dafür als Abnahmekriterien gelten.

Source items: `L3-REQ-010`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-011 - Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher...

Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher Rechtsgrundlage Analyse, Test, inhaltliche Erstbefüllung und Produktivbetrieb erfolgen, wie Einwilligungen dokumentiert werden und welche organisatorischen Rollen die datenschutzrechtliche Freigabe verantworten.

Source items: `L3-REQ-011`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-012 - Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine...

Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine abbildet oder auch medizinisch sensible Informationen wie Medikamentengaben enthalten darf; falls letzteres gewünscht ist, sind eigener Schutzbedarf, Zugriffsregeln und Scope-Folgen gesondert freizugeben.

Source items: `L3-REQ-012`
Origin: `HUMAN_ACCEPTED_ANCHORED`

### CAN-REQ-014 - Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als...

Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als Computerprogramm oder ähnliche Anwendung, bevorzugt, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern und zu fördern.

Source items: `REQ-02`
Origin: `Extracted`

### CAN-REQ-020 - Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht...

Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste vorgesehen werden.

Source items: `REQ-08`
Origin: `Extracted`

### CAN-REQ-021 - Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als...

Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als Kacheln oder Liste unterhalb der Suchleiste dargestellt werden, jeweils mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners für einen schnellen Überblick.

Source items: `REQ-09`
Origin: `Extracted`

### CAN-REQ-022 - Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit...

Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit anschließendem Dialog zur Erfassung von Bild, Name und Beschreibung soll geklärt werden.

Source items: `REQ-10`
Origin: `Extracted`

### CAN-REQ-023 - Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine...

Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt.

Source items: `REQ-11`
Origin: `Extracted`

### CAN-REQ-026 - Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit...

Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthalten; darunter soll eine Foto-Timeline liegen, in die über einen Plus-Button am unteren Bildschirmrand neue Einträge aufgenommen werden können; die neuesten Fotos sollen oben angezeigt werden.

Source items: `REQ-14`
Origin: `Extracted`

### CAN-REQ-029 - Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles...

Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles Symbol, etwa ein rotes Stoppschild, oben auf dem Screen zeigen; die Einträge sollen leicht zu durchforsten sein und nur die wichtigsten Informationen enthalten.

Source items: `REQ-17`
Origin: `Extracted`

### CAN-REQ-031 - Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der...

Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der Kommunikationsseite visuell mit Symbolen gekennzeichnet werden; jeder Bereich soll einen klaren Button zu weiterführenden Informationen haben.

Source items: `REQ-19`
Origin: `Extracted`

### CAN-REQ-034 - Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein...

Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein Plus-Button vorgesehen werden, um Videos und Beschreibungen hinzuzufügen.

Source items: `REQ-22`
Origin: `Extracted`

### CAN-REQ-036 - Ein Kalender mit Terminen ist vorgesehen; ob darin auch Medikamentengaben integriert...

Ein Kalender mit Terminen ist vorgesehen; ob darin auch Medikamentengaben integriert werden sollen, ist wegen Vertraulichkeit und Umfang noch zu klären.

Source items: `REQ-24`
Origin: `Extracted`

### CAN-REQ-038 - Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des...

Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des aktuellen Screens angezeigt werden, auf der Profilübersicht also „Profilübersicht“.

Source items: `REQ-26`
Origin: `Extracted`

### CAN-REQ-057 - Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter...

Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter der Einrichtung zu klären; der Umfang soll nicht so groß werden, dass der eigentliche Sinn der unterstützenden Kommunikation verloren geht.

Source items: `REQ-45`
Origin: `Extracted`

### CAN-REQ-069 - Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als...

Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.

Source items: `REQ-57`
Origin: `Extracted`

## Open Decisions

- `OPEN-001` (CAN-REQ-001): Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile der formalen Pflegedokumentation übernehmen soll; für beide Varianten ist eine klare Scope-Abgrenzung einschließlich expliziter Nicht-Ziele zu dokumentieren.
- `OPEN-002` (CAN-REQ-004): Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue oder geänderte Beiträge von Mitarbeitern, Angehörigen oder Bewohnern müssen definierte Zustände wie Entwurf, eingereicht, freigegeben, zurückgewiesen oder archiviert durchlaufen, und bei gleichzeitigen Änderungen ist ein Konfliktverhalten einschließlich Versionserhalt festzulegen.
- `OPEN-003` (CAN-REQ-005): Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte Aufbewahrungsdauer, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen sowie einen nachvollziehbaren Export pro Bewohner oder Einrichtung regelt.
- `OPEN-004` (CAN-REQ-006): Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte verbindlich vorgeschrieben sind; mindestens sind Regeln für abgesicherte Authentifizierung, serverseitig erzwungene Rollen- und Einrichtungstrennung sowie Protokollierung sicherheitsrelevanter Zugriffe und Änderungen für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe festzulegen.
- `OPEN-005` (CAN-REQ-007): Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen übernommen werden; dafür sind Quelle, Datenumfang, Übertragungsweg, Verantwortlichkeit und der Umgang mit Medienbrüchen oder manueller Doppelpflege festzulegen.
- `OPEN-006` (CAN-REQ-008): Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder instabiler Internetverbindung verbindlich unterstützt werden muss; dabei sind lokale Zwischenspeicherung, Synchronisationszeitpunkte, Konfliktauflösung nach Wiederverbindung sowie Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb festzulegen.
- `OPEN-007` (CAN-REQ-009): Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such- und Ladezeiten bei Profilen, erwartete Anzahl gleichzeitiger Nutzer pro Einrichtung, Anzahl verwalteter Bewohnerprofile sowie akzeptable Mediengrößen für Bilder und Videos.
- `OPEN-008` (CAN-REQ-010): Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben Smartphones auch Tablets unterstützen muss, in welchen Arbeitssituationen die App bedienbar sein soll und welche konkreten Bedienbarkeits- und Barrierefreiheitskriterien dafür als Abnahmekriterien gelten.
- `OPEN-009` (CAN-REQ-011): Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher Rechtsgrundlage Analyse, Test, inhaltliche Erstbefüllung und Produktivbetrieb erfolgen, wie Einwilligungen dokumentiert werden und welche organisatorischen Rollen die datenschutzrechtliche Freigabe verantworten.
- `OPEN-010` (CAN-REQ-012): Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine abbildet oder auch medizinisch sensible Informationen wie Medikamentengaben enthalten darf; falls letzteres gewünscht ist, sind eigener Schutzbedarf, Zugriffsregeln und Scope-Folgen gesondert freizugeben.
- `OPEN-011` (CAN-REQ-014): Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als Computerprogramm oder ähnliche Anwendung, bevorzugt, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern und zu fördern.
- `OPEN-012` (CAN-REQ-020): Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste vorgesehen werden.
- `OPEN-013` (CAN-REQ-021): Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als Kacheln oder Liste unterhalb der Suchleiste dargestellt werden, jeweils mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners für einen schnellen Überblick.
- `OPEN-014` (CAN-REQ-022): Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit anschließendem Dialog zur Erfassung von Bild, Name und Beschreibung soll geklärt werden.
- `OPEN-015` (CAN-REQ-023): Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt.
- `OPEN-016` (CAN-REQ-026): Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthalten; darunter soll eine Foto-Timeline liegen, in die über einen Plus-Button am unteren Bildschirmrand neue Einträge aufgenommen werden können; die neuesten Fotos sollen oben angezeigt werden.
- `OPEN-017` (CAN-REQ-029): Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles Symbol, etwa ein rotes Stoppschild, oben auf dem Screen zeigen; die Einträge sollen leicht zu durchforsten sein und nur die wichtigsten Informationen enthalten.
- `OPEN-018` (CAN-REQ-031): Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der Kommunikationsseite visuell mit Symbolen gekennzeichnet werden; jeder Bereich soll einen klaren Button zu weiterführenden Informationen haben.
- `OPEN-019` (CAN-REQ-034): Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein Plus-Button vorgesehen werden, um Videos und Beschreibungen hinzuzufügen.
- `OPEN-020` (CAN-REQ-036): Ein Kalender mit Terminen ist vorgesehen; ob darin auch Medikamentengaben integriert werden sollen, ist wegen Vertraulichkeit und Umfang noch zu klären.
- `OPEN-021` (CAN-REQ-038): Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des aktuellen Screens angezeigt werden, auf der Profilübersicht also „Profilübersicht“.
- `OPEN-022` (CAN-REQ-057): Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter der Einrichtung zu klären; der Umfang soll nicht so groß werden, dass der eigentliche Sinn der unterstützenden Kommunikation verloren geht.
- `OPEN-023` (CAN-REQ-069): Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.

