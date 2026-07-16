# Canonical Requirements Baseline

Baseline: `baseline-20260716_130307`
Project: `evidenz-agent-demo`
Created UTC: `2026-07-16T13:03:07.9658900Z`
Requirements: `69`
Open decisions: `27`

## active

### CAN-REQ-002 - Vollständiges Rollen- und Berechtigungskonzept spezifizieren

Für Mitarbeiter-, Angehörigen-, Bewohner- und Admin-Accounts ist ein vollständiges Berechtigungskonzept zu spezifizieren, das je Rolle pro Funktionsbereich festlegt, wer Profile sehen, Inhalte anlegen, bearbeiten, freigeben, exportieren, verbergen oder löschen darf; dabei ist auch der einrichtungsbezogene Zugriff über Häusergrenzen hinweg eindeutig zu regeln.

Source items: `L3-REQ-002`
Origin: `KEEP`

### CAN-REQ-003 - Eingaberegeln und Beschreibungsschema für Inhalte definieren

Für die Erfassung von Kommunikationsweisen, No-Go-Inhalten, About-Me-Daten, Bildern und Kalendereinträgen sind Pflichtfelder, zulässige Formate, maximale Feldlängen sowie Verhalten bei unvollständigen oder leeren Eingaben zu definieren; insbesondere muss für Kommunikations-Einträge ein einheitliches Beschreibungsschema vorgegeben werden, damit Suche und Filterung zuverlässig funktionieren.

Source items: `L3-REQ-003`
Origin: `KEEP`

### CAN-REQ-013 - Lösung soll primär das Verstehen von Bewohnern unterstützen

Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere damit Betreuer bei Verständnisproblemen nachschauen können, was mit einer Kommunikationsweise gemeint sein könnte.

Source items: `REQ-01`
Origin: `KEEP`

### CAN-REQ-015 - Lösung soll neue Mitarbeiter und Verständigungssituationen unterstützen

Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll auch in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind und sich verständigen müssen.

Source items: `REQ-03`
Origin: `KEEP`

### CAN-REQ-016 - Angehörige sollen ergänzenden Zugriff erhalten können

Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzende Daten und Wissen hinzufügen können, die den Mitarbeitern sonst erst bekannt werden müssten.

Source items: `REQ-04`
Origin: `KEEP`

### CAN-REQ-017 - Bewohner-Account mit eingeschränkten Funktionen vorsehen

Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann und nur eingeschränkte Funktionen erhält, insbesondere Zugriff auf den About-Me-Bereich und das Hinzufügen eigener Bilder; dies ist als gewünschte Anforderung vorgesehen, um Fehlbedienungen zu minimieren.

Source items: `REQ-05`
Origin: `KEEP`

### CAN-REQ-018 - Profilübersichtsseite nach Login anzeigen

Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen.

Source items: `REQ-06`
Origin: `KEEP`

### CAN-REQ-019 - Suchfunktion auf der Profilübersichtsseite bereitstellen

Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, statt lange scrollen zu müssen.

Source items: `REQ-07`
Origin: `KEEP`

### CAN-REQ-024 - About-Me-Seite pro Person bereitstellen

Die App soll pro Person eine About-Me-Seite bereitstellen, die für den ersten Eindruck informativ und persönlich ist und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält.

Source items: `REQ-12`
Origin: `KEEP`

### CAN-REQ-025 - Profil- und Kommunikationsinhalte dynamisch erweiterbar machen

Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können.

Source items: `REQ-13`
Origin: `KEEP`

### CAN-REQ-027 - No-Go-Seite pro Bewohner bereitstellen

Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht.

Source items: `REQ-15`
Origin: `KEEP`

### CAN-REQ-028 - No-Go-Seite dynamisch erweiterbar machen

Die No-Go-Seite soll über einen Plus-Button dynamisch erweiterbar sein, sodass neue No-Gos als Liste hinzugefügt werden können, über die sich Nutzer im Vorhinein informieren können.

Source items: `REQ-16`
Origin: `KEEP`

### CAN-REQ-030 - Kommunikationsinformationen in verbale und nonverbale Bereiche unterteilen

Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden.

Source items: `REQ-18`
Origin: `KEEP`

### CAN-REQ-032 - Videos mit Beschreibungen in Kommunikationsseiten integrieren

Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen.

Source items: `REQ-20`
Origin: `KEEP`

### CAN-REQ-033 - Neue Videos leicht hinzufügbar machen und neuestes oben anzeigen

Auf der Videofunktion sollen neue Videos mit Beschreibungen leicht hinzufügbar sein, und das neueste Video soll jeweils oben angezeigt werden.

Source items: `REQ-21`
Origin: `KEEP`

### CAN-REQ-035 - Suchfunktion auf Kommunikationsseiten mit Beschreibungsmuster ermöglichen

Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss jedoch noch ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich sind.

Source items: `REQ-23`
Origin: `KEEP`

### CAN-REQ-036 - Konsistente Appbar nach Login auf jeder Seite bereitstellen

Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; sie soll auch ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist.

Source items: `REQ-25`
Origin: `KEEP`

### CAN-REQ-038 - Nur Login ohne Selbstregistrierung vorsehen

Für den Login soll es nur eine Login-Möglichkeit geben; eine Selbstregistrierung soll nicht vorgesehen sein.

Source items: `REQ-27`
Origin: `KEEP`

### CAN-REQ-039 - Login-Screen mit Logo, E-Mail, Passwort und Login-Button gestalten

Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen; eine Registrierung soll auf diesem Screen nicht vorhanden sein.

Source items: `REQ-28`
Origin: `KEEP`

### CAN-REQ-040 - Logo auf Login-Screen sichtbar im oberen Drittel platzieren

Für den Login-Screen ist als gewünschte Anforderung vorgesehen, dass das Logo im oberen Drittel des Bildschirms zentriert und gut sichtbar platziert wird; das Logo selbst ist noch zu erstellen und soll etwas mit Kommunikation zu tun haben.

Source items: `REQ-29`
Origin: `KEEP`

### CAN-REQ-041 - Optionale Hilfe- oder Tutorial-Funktion vorsehen

Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, optionale Unterstützung gewünscht, etwa als kurze Tour beim ersten Login sowie als erneut aufrufbarer Hilfebereich.

Source items: `REQ-30`
Origin: `KEEP`

### CAN-REQ-042 - Optionale Popup-Benachrichtigung bei neuen About-Me-Inhalten vorsehen

Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen als spätere optionale Funktion alle verbundenen Nutzer derselben Einrichtung per Popup benachrichtigt werden.

Source items: `REQ-31`
Origin: `KEEP`

### CAN-REQ-043 - Alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen

Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden.

Source items: `REQ-32`
Origin: `KEEP`

### CAN-REQ-044 - Rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen

Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen.

Source items: `REQ-33`
Origin: `KEEP`

### CAN-REQ-045 - Nur Admin darf Accounts anlegen und Rechte verwalten

Nur ein Account mit Admin-Rechten darf weitere Accounts anlegen und Rechte verwalten.

Source items: `REQ-34`
Origin: `KEEP`

### CAN-REQ-046 - User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen

User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen und keine hinzugefügten Daten aus der App löschen.

Source items: `REQ-35`
Origin: `KEEP`

### CAN-REQ-047 - App nur für internen Gebrauch vorsehen

Die App ist nur für den internen Gebrauch vorgesehen.

Source items: `REQ-36`
Origin: `KEEP`

### CAN-REQ-048 - Zugriffe einrichtungsbezogen beschränken

Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten.

Source items: `REQ-37`
Origin: `KEEP`

### CAN-REQ-049 - Settings-Screen rollenabhängig differenzieren

Für den Settings-Screen ist festzuhalten, dass die Ansicht rollenabhängig differenziert sein soll: Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen, während jeder Nutzer sein Profil ändern und Einstellungen wie die Sprache wählen können soll.

Source items: `REQ-38`
Origin: `KEEP`

### CAN-REQ-050 - Kommunikationsinhalte auch visuell darstellen

Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen.

Source items: `REQ-39`
Origin: `KEEP`

### CAN-REQ-051 - Barrierefreiheit im Design berücksichtigen

Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung von zu vielen Farben.

Source items: `REQ-40`
Origin: `KEEP`

### CAN-REQ-052 - App plattformübergreifend auf iOS und Android betreiben

Die App soll plattformübergreifend auf iPhone/iOS und Android laufen.

Source items: `REQ-41`
Origin: `KEEP`

### CAN-REQ-053 - Animationen nicht priorisieren und nicht ablenkend gestalten

Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein.

Source items: `REQ-43`
Origin: `KEEP`

### CAN-REQ-054 - Übersetzungssystem zwischen Bewohnern und Betreuern aus Scope ausschließen

Ein System, das als Schnittstelle zwischen Bewohnern und Betreuern hin und her übersetzt, ist ausdrücklich nicht Teil des Lösungsumfangs.

Source items: `REQ-44`
Origin: `KEEP`

### CAN-REQ-056 - Bestehende Akten und dokumentierte Erfahrungen als Wissensquelle berücksichtigen

Für Bewohner existieren klassische Akten sowie planmäßig dokumentierte Erfahrungen und neues Wissen; diese dokumentierten Informationen sind als relevante Wissensquelle für die Lösung zu berücksichtigen.

Source items: `REQ-46`
Origin: `KEEP`

### CAN-REQ-057 - Schlechte Nutzbarkeit vorhandener Dokumentation bei Lösungsdesign berücksichtigen

Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und Wissen zur Kommunikation wird neuen Mitarbeitern daher häufig mündlich erklärt. Dieses Risiko ist bei der Ausgestaltung der Lösung zu berücksichtigen.

Source items: `REQ-47`
Origin: `KEEP`

### CAN-REQ-058 - Anforderungsanalyse mit Beteiligten und späteren Nutzern durchführen

Vor der konkreten Ausgestaltung der Lösung soll als gewünschte Vorgehensweise eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Verständnis zu gewinnen.

Source items: `REQ-50`
Origin: `KEEP`

### CAN-REQ-059 - Einsatz von GetX als technische Rahmenbedingung prüfen

Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden.

Source items: `REQ-52`
Origin: `KEEP`

### CAN-REQ-060 - Flutter mit Dart als Technologie-Stack verwenden

Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen.

Source items: `REQ-53`
Origin: `KEEP`

### CAN-REQ-061 - Android Studio als bevorzugte Entwicklungsumgebung nutzen

Das Team präferiert, möglichst dieselbe Entwicklungsumgebung zu nutzen, und bevorzugt dafür Android Studio.

Source items: `REQ-54`
Origin: `KEEP`

### CAN-REQ-062 - Versionen der Entwicklungswerkzeuge im Team angleichen

Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um spätere Integrationsprobleme zu vermeiden.

Source items: `REQ-55`
Origin: `KEEP`

### CAN-REQ-063 - Initiale Entwicklung und Tests zunächst gegen Android 11 ausrichten

Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden.

Source items: `REQ-56`
Origin: `KEEP`

## open_decision

### CAN-DISK-003 - MVP-Abnahmeschwellen für Verfügbarkeit und Wiederanlauf als Diskussionspunkt ergänzen

Zu den messbaren Qualitätszielen existiert mit CAN-REQ-009 bereits ein offener Rahmen für Performance, Last, Profilanzahl und Mediengrößen. Nicht gesagt ist jedoch, welche minimalen Erwartungen für Verfügbarkeit, Störungsdauer oder Wiederanlauf im MVP gelten sollen.

Vorgeschlagene Klaerung:
Als RE-Diskussionspunkt ergänzen, ob für den MVP minimale Zielwerte oder qualitative Leitplanken zu Verfügbarkeit, maximal tolerierbarer Ausfallzeit und Wiederanlauf nach Störungen festgelegt werden sollen.

Warum wichtig:
Auch bei kleinem MVP beeinflussen solche Betriebsziele Architektur, Monitoring, Supporterwartung und Abnahme. Die Ergänzung macht eine plausible, bislang nicht ausgesprochene Qualitätslücke sichtbar, ohne sie als beschlossene Wahrheit zu setzen.

Source items: `L3-REQ-009`
Origin: `L4_COMPLETION_DISK`

### CAN-OPEN-001 - Scope-Entscheidung mit expliziten Nicht-Zielen vor Projektstart abschließen

Die zentrale Abgrenzung, ob der MVP nur ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile formaler Dokumentation übernimmt, ist zwar als offene Entscheidung vorhanden, aber noch nicht in eine startklare, explizite Entscheidungsgrundlage mit Nicht-Zielen überführt. Dadurch bleibt unklar, welche nachgelagerten Anforderungen verbindlich im MVP liegen und welche ausdrücklich ausgeschlossen sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-001/CAN-REQ-055 eine formale offene Entscheidungsfrage mit genau zwei bis drei Scope-Optionen, den jeweiligen Konsequenzen für Datenarten, Rollen, Datenschutz und UI sowie einer Liste expliziter Nicht-Ziele für den MVP.

Warum wichtig:
Diese Entscheidung ist ein Startblocker für Funktionsumfang, Datenmodell, Datenschutzbedarf und Aufwandsschätzung. Ohne sie bleibt ein erheblicher Teil der übrigen Requirements interpretationsabhängig.

Source items: `L3-REQ-001`, `REQ-45`
Origin: `L4_COMPLETION_OPEN_DECISION`

### CAN-OPEN-002 - Rechtsgrundlage und zulässige Datennutzung für echte Bewohnerdaten und Medien entscheiden

Die bestehende Datenschutzanforderung CAN-REQ-005 deckt den Datenlebenszyklus ab, benennt aber die startkritische Entscheidung zur Rechtsgrundlage und zum zulässigen Einsatz echter Bewohnerdaten, Bilder und Videos im Test- und Produktivkontext noch nicht als klar abgegrenzte Entscheidungsfrage.

Vorgeschlagene Klaerung:
Ergänze eine offene Entscheidung, welche Datenkategorien mit welcher Rechtsgrundlage verarbeitet werden dürfen, ob und unter welchen Bedingungen echte Bewohnerdaten/Bilder in Tests zulässig sind und welche Einwilligungs- oder Freigabeschritte dafür vorliegen müssen.

Warum wichtig:
Diese Entscheidung steuert, ob fachliche Tests mit Echtdaten zulässig sind und welche Prozesse, Datenmodelle und Betriebsregeln überhaupt umgesetzt werden dürfen.

Source items: `L3-REQ-005`
Origin: `L4_COMPLETION_OPEN_DECISION`

### CAN-OPEN-004 - GetX-Status als offene Technikentscheidung korrekt ausweisen

CAN-REQ-059 ist als aktive Anforderung geführt, enthält laut Readiness aber einen Entscheidungsmarker. Dadurch ist unklar, ob GetX bereits als gesetzte Rahmenbedingung gilt oder bewusst noch zur Auswahl steht.

Vorgeschlagene Klaerung:
Reklassifiziere den Inhalt von CAN-REQ-059 in eine explizite offene Technikentscheidung mit klaren Entscheidungsoptionen, Bewertungskriterien und einem Termin, bis zu dem die Wahl für den Projektstart getroffen werden muss.

Warum wichtig:
State-Management beeinflusst Architektur, Zustandsfluss, Testbarkeit und Implementierungsstruktur direkt. Ein uneindeutiger Status erzeugt vermeidbares Richtungsrisiko.

Source items: `REQ-52`
Origin: `L4_COMPLETION_OPEN_DECISION`

### CAN-OPEN-005 - Minimalen Content-Lifecycle für nutzergenerierte Inhalte verbindlich entscheiden

Mit CAN-REQ-004 ist der Bearbeitungs- und Freigabeprozess zwar als offene Entscheidung erfasst, für den Projektstart fehlen aber eine minimal verbindliche Zielausprägung und die Abgrenzung, welche Zustände und Verantwortlichkeiten im MVP tatsächlich verpflichtend sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-004 eine offene Entscheidungsgrundlage für den MVP-Content-Lifecycle mit minimalen Statuswerten, verantwortlichen Rollen je Statusübergang, Regeln bei Zurückweisung/Korrektur und dem Umgang mit parallelen Änderungen.

Warum wichtig:
Ohne klaren Minimalprozess bleiben Verantwortlichkeiten, Moderation, Inhaltsqualität und Konfliktbehandlung im Kernprozess der Inhalteingabe unsicher.

Source items: `L3-REQ-004`
Origin: `L4_COMPLETION_OPEN_DECISION`

### CAN-REQ-001 - MVP-Scope zwischen Unterstützungswerkzeug und Dokumentationsumfang festlegen

Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile der formalen Pflegedokumentation übernehmen soll; für beide Varianten ist eine klare Scope-Abgrenzung einschließlich expliziter Nicht-Ziele zu dokumentieren.

Source items: `L3-REQ-001`
Origin: `KEEP`

### CAN-REQ-004 - Bearbeitungs- und Freigabeprozess für nutzergenerierte Inhalte festlegen

Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue oder geänderte Beiträge von Mitarbeitern, Angehörigen oder Bewohnern müssen definierte Zustände wie Entwurf, eingereicht, freigegeben, zurückgewiesen oder archiviert durchlaufen, und bei gleichzeitigen Änderungen ist ein Konfliktverhalten einschließlich Versionserhalt festzulegen.

Source items: `L3-REQ-004`
Origin: `KEEP`

### CAN-REQ-005 - Datenschutz- und Datenlebenszyklus vor Produktivstart festlegen

Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte Aufbewahrungsdauer, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen sowie einen nachvollziehbaren Export pro Bewohner oder Einrichtung regelt.

Source items: `L3-REQ-005`
Origin: `KEEP`

### CAN-REQ-006 - Sicherheitsmaßnahmen für besonders sensible Inhalte festlegen

Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte verbindlich vorgeschrieben sind; mindestens sind Regeln für abgesicherte Authentifizierung, serverseitig erzwungene Rollen- und Einrichtungstrennung sowie Protokollierung sicherheitsrelevanter Zugriffe und Änderungen für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe festzulegen.

Source items: `L3-REQ-006`
Origin: `KEEP`

### CAN-REQ-007 - Übernahme aus Akten und Drittsystemen entscheiden

Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen übernommen werden; dafür sind Quelle, Datenumfang, Übertragungsweg, Verantwortlichkeit und der Umgang mit Medienbrüchen oder manueller Doppelpflege festzulegen.

Source items: `L3-REQ-007`
Origin: `KEEP`

### CAN-REQ-008 - Offline- und Synchronisationsverhalten für Firestore festlegen

Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder instabiler Internetverbindung verbindlich unterstützt werden muss; dabei sind lokale Zwischenspeicherung, Synchronisationszeitpunkte, Konfliktauflösung nach Wiederverbindung sowie Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb festzulegen.

Source items: `L3-REQ-008`
Origin: `KEEP`

### CAN-REQ-009 - Messbare Qualitätsziele vor Projektstart festlegen

Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such- und Ladezeiten bei Profilen, erwartete Anzahl gleichzeitiger Nutzer pro Einrichtung, Anzahl verwalteter Bewohnerprofile sowie akzeptable Mediengrößen für Bilder und Videos.

Source items: `L3-REQ-009`
Origin: `KEEP`

### CAN-REQ-010 - Nutzungskontext, Tablet-Support und Barrierefreiheitskriterien festlegen

Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben Smartphones auch Tablets unterstützen muss, in welchen Arbeitssituationen die App bedienbar sein soll und welche konkreten Bedienbarkeits- und Barrierefreiheitskriterien dafür als Abnahmekriterien gelten.

Source items: `L3-REQ-010`
Origin: `KEEP`

### CAN-REQ-011 - Rechtsgrundlage und Freigaben für echte Bewohnerdaten und Bilder festlegen

Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher Rechtsgrundlage Analyse, Test, inhaltliche Erstbefüllung und Produktivbetrieb erfolgen, wie Einwilligungen dokumentiert werden und welche organisatorischen Rollen die datenschutzrechtliche Freigabe verantworten.

Source items: `L3-REQ-011`
Origin: `KEEP`

### CAN-REQ-012 - Kalender-Scope und Umgang mit medizinisch sensiblen Informationen entscheiden

Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine abbildet oder auch medizinisch sensible Informationen wie Medikamentengaben enthalten darf; falls letzteres gewünscht ist, sind eigener Schutzbedarf, Zugriffsregeln und Scope-Folgen gesondert freizugeben.

Source items: `L3-REQ-012`
Origin: `KEEP`

### CAN-REQ-014 - Digitale Lösung zur Kommunikationsförderung als gewünschtes Zielbild

Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als Computerprogramm oder ähnliche Anwendung, bevorzugt, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern und zu fördern.

Source items: `REQ-02`
Origin: `KEEP`

### CAN-REQ-020 - Sichtbare Suchleiste unter der Appbar auf Profilübersicht vorsehen

Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste vorgesehen werden.

Source items: `REQ-08`
Origin: `KEEP`

### CAN-REQ-021 - Profile als Kacheln oder Liste mit Vorschaudaten darstellen

Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als Kacheln oder Liste unterhalb der Suchleiste dargestellt werden, jeweils mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners für einen schnellen Überblick.

Source items: `REQ-09`
Origin: `KEEP`

### CAN-REQ-022 - Anlegen neuer Profile über Profilübersicht klären

Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit anschließendem Dialog zur Erfassung von Bild, Name und Beschreibung soll geklärt werden.

Source items: `REQ-10`
Origin: `KEEP`

### CAN-REQ-023 - Profil-Detailansicht mit vier Hauptbereichen vorsehen

Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt.

Source items: `REQ-11`
Origin: `KEEP`

### CAN-REQ-026 - About-Me-Seite mit Infobox und Foto-Timeline ausgestalten

Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthalten; darunter soll eine Foto-Timeline liegen, in die über einen Plus-Button am unteren Bildschirmrand neue Einträge aufgenommen werden können; die neuesten Fotos sollen oben angezeigt werden.

Source items: `REQ-14`
Origin: `KEEP`

### CAN-REQ-029 - No-Go-Seite mit starkem visuellen Symbol ausstatten

Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles Symbol, etwa ein rotes Stoppschild, oben auf dem Screen zeigen; die Einträge sollen leicht zu durchforsten sein und nur die wichtigsten Informationen enthalten.

Source items: `REQ-17`
Origin: `KEEP`

### CAN-REQ-031 - Trennung der Kommunikationsbereiche visuell kennzeichnen

Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der Kommunikationsseite visuell mit Symbolen gekennzeichnet werden; jeder Bereich soll einen klaren Button zu weiterführenden Informationen haben.

Source items: `REQ-19`
Origin: `KEEP`

### CAN-REQ-034 - Plus-Button für nonverbale Videos und Beschreibungen vorsehen

Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein Plus-Button vorgesehen werden, um Videos und Beschreibungen hinzuzufügen.

Source items: `REQ-22`
Origin: `KEEP`

### CAN-REQ-037 - Aktuellen Screen-Titel mittig in der Appbar anzeigen

Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des aktuellen Screens angezeigt werden, auf der Profilübersicht also „Profilübersicht“.

Source items: `REQ-26`
Origin: `KEEP`

### CAN-REQ-055 - Offene Entscheidung zur vollständigen Dokumentationsübernahme erhalten

Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter der Einrichtung zu klären; der Umfang soll nicht so groß werden, dass der eigentliche Sinn der unterstützenden Kommunikation verloren geht.

Source items: `REQ-45`
Origin: `KEEP`

### CAN-REQ-064 - Digitalisierung grundsätzlich als vorteilhaft ansehen

Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.

Source items: `REQ-57`
Origin: `KEEP`

## Open Decisions

- `OPEN-001` (CAN-DISK-003): Zu den messbaren Qualitätszielen existiert mit CAN-REQ-009 bereits ein offener Rahmen für Performance, Last, Profilanzahl und Mediengrößen. Nicht gesagt ist jedoch, welche minimalen Erwartungen für Verfügbarkeit, Störungsdauer oder Wiederanlauf im MVP gelten sollen.

Vorgeschlagene Klaerung:
Als RE-Diskussionspunkt ergänzen, ob für den MVP minimale Zielwerte oder qualitative Leitplanken zu Verfügbarkeit, maximal tolerierbarer Ausfallzeit und Wiederanlauf nach Störungen festgelegt werden sollen.

Warum wichtig:
Auch bei kleinem MVP beeinflussen solche Betriebsziele Architektur, Monitoring, Supporterwartung und Abnahme. Die Ergänzung macht eine plausible, bislang nicht ausgesprochene Qualitätslücke sichtbar, ohne sie als beschlossene Wahrheit zu setzen.
- `OPEN-002` (CAN-OPEN-001): Die zentrale Abgrenzung, ob der MVP nur ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile formaler Dokumentation übernimmt, ist zwar als offene Entscheidung vorhanden, aber noch nicht in eine startklare, explizite Entscheidungsgrundlage mit Nicht-Zielen überführt. Dadurch bleibt unklar, welche nachgelagerten Anforderungen verbindlich im MVP liegen und welche ausdrücklich ausgeschlossen sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-001/CAN-REQ-055 eine formale offene Entscheidungsfrage mit genau zwei bis drei Scope-Optionen, den jeweiligen Konsequenzen für Datenarten, Rollen, Datenschutz und UI sowie einer Liste expliziter Nicht-Ziele für den MVP.

Warum wichtig:
Diese Entscheidung ist ein Startblocker für Funktionsumfang, Datenmodell, Datenschutzbedarf und Aufwandsschätzung. Ohne sie bleibt ein erheblicher Teil der übrigen Requirements interpretationsabhängig.
- `OPEN-003` (CAN-OPEN-002): Die bestehende Datenschutzanforderung CAN-REQ-005 deckt den Datenlebenszyklus ab, benennt aber die startkritische Entscheidung zur Rechtsgrundlage und zum zulässigen Einsatz echter Bewohnerdaten, Bilder und Videos im Test- und Produktivkontext noch nicht als klar abgegrenzte Entscheidungsfrage.

Vorgeschlagene Klaerung:
Ergänze eine offene Entscheidung, welche Datenkategorien mit welcher Rechtsgrundlage verarbeitet werden dürfen, ob und unter welchen Bedingungen echte Bewohnerdaten/Bilder in Tests zulässig sind und welche Einwilligungs- oder Freigabeschritte dafür vorliegen müssen.

Warum wichtig:
Diese Entscheidung steuert, ob fachliche Tests mit Echtdaten zulässig sind und welche Prozesse, Datenmodelle und Betriebsregeln überhaupt umgesetzt werden dürfen.
- `OPEN-004` (CAN-OPEN-004): CAN-REQ-059 ist als aktive Anforderung geführt, enthält laut Readiness aber einen Entscheidungsmarker. Dadurch ist unklar, ob GetX bereits als gesetzte Rahmenbedingung gilt oder bewusst noch zur Auswahl steht.

Vorgeschlagene Klaerung:
Reklassifiziere den Inhalt von CAN-REQ-059 in eine explizite offene Technikentscheidung mit klaren Entscheidungsoptionen, Bewertungskriterien und einem Termin, bis zu dem die Wahl für den Projektstart getroffen werden muss.

Warum wichtig:
State-Management beeinflusst Architektur, Zustandsfluss, Testbarkeit und Implementierungsstruktur direkt. Ein uneindeutiger Status erzeugt vermeidbares Richtungsrisiko.
- `OPEN-005` (CAN-OPEN-005): Mit CAN-REQ-004 ist der Bearbeitungs- und Freigabeprozess zwar als offene Entscheidung erfasst, für den Projektstart fehlen aber eine minimal verbindliche Zielausprägung und die Abgrenzung, welche Zustände und Verantwortlichkeiten im MVP tatsächlich verpflichtend sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-004 eine offene Entscheidungsgrundlage für den MVP-Content-Lifecycle mit minimalen Statuswerten, verantwortlichen Rollen je Statusübergang, Regeln bei Zurückweisung/Korrektur und dem Umgang mit parallelen Änderungen.

Warum wichtig:
Ohne klaren Minimalprozess bleiben Verantwortlichkeiten, Moderation, Inhaltsqualität und Konfliktbehandlung im Kernprozess der Inhalteingabe unsicher.
- `OPEN-006` (CAN-REQ-001): Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile der formalen Pflegedokumentation übernehmen soll; für beide Varianten ist eine klare Scope-Abgrenzung einschließlich expliziter Nicht-Ziele zu dokumentieren.
- `OPEN-007` (CAN-REQ-004): Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue oder geänderte Beiträge von Mitarbeitern, Angehörigen oder Bewohnern müssen definierte Zustände wie Entwurf, eingereicht, freigegeben, zurückgewiesen oder archiviert durchlaufen, und bei gleichzeitigen Änderungen ist ein Konfliktverhalten einschließlich Versionserhalt festzulegen.
- `OPEN-008` (CAN-REQ-005): Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte Aufbewahrungsdauer, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen sowie einen nachvollziehbaren Export pro Bewohner oder Einrichtung regelt.
- `OPEN-009` (CAN-REQ-006): Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte verbindlich vorgeschrieben sind; mindestens sind Regeln für abgesicherte Authentifizierung, serverseitig erzwungene Rollen- und Einrichtungstrennung sowie Protokollierung sicherheitsrelevanter Zugriffe und Änderungen für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe festzulegen.
- `OPEN-010` (CAN-REQ-007): Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen übernommen werden; dafür sind Quelle, Datenumfang, Übertragungsweg, Verantwortlichkeit und der Umgang mit Medienbrüchen oder manueller Doppelpflege festzulegen.
- `OPEN-011` (CAN-REQ-008): Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder instabiler Internetverbindung verbindlich unterstützt werden muss; dabei sind lokale Zwischenspeicherung, Synchronisationszeitpunkte, Konfliktauflösung nach Wiederverbindung sowie Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb festzulegen.
- `OPEN-012` (CAN-REQ-009): Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such- und Ladezeiten bei Profilen, erwartete Anzahl gleichzeitiger Nutzer pro Einrichtung, Anzahl verwalteter Bewohnerprofile sowie akzeptable Mediengrößen für Bilder und Videos.
- `OPEN-013` (CAN-REQ-010): Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben Smartphones auch Tablets unterstützen muss, in welchen Arbeitssituationen die App bedienbar sein soll und welche konkreten Bedienbarkeits- und Barrierefreiheitskriterien dafür als Abnahmekriterien gelten.
- `OPEN-014` (CAN-REQ-011): Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher Rechtsgrundlage Analyse, Test, inhaltliche Erstbefüllung und Produktivbetrieb erfolgen, wie Einwilligungen dokumentiert werden und welche organisatorischen Rollen die datenschutzrechtliche Freigabe verantworten.
- `OPEN-015` (CAN-REQ-012): Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine abbildet oder auch medizinisch sensible Informationen wie Medikamentengaben enthalten darf; falls letzteres gewünscht ist, sind eigener Schutzbedarf, Zugriffsregeln und Scope-Folgen gesondert freizugeben.
- `OPEN-016` (CAN-REQ-014): Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als Computerprogramm oder ähnliche Anwendung, bevorzugt, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern und zu fördern.
- `OPEN-017` (CAN-REQ-020): Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste vorgesehen werden.
- `OPEN-018` (CAN-REQ-021): Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als Kacheln oder Liste unterhalb der Suchleiste dargestellt werden, jeweils mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners für einen schnellen Überblick.
- `OPEN-019` (CAN-REQ-022): Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit anschließendem Dialog zur Erfassung von Bild, Name und Beschreibung soll geklärt werden.
- `OPEN-020` (CAN-REQ-023): Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt.
- `OPEN-021` (CAN-REQ-026): Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthalten; darunter soll eine Foto-Timeline liegen, in die über einen Plus-Button am unteren Bildschirmrand neue Einträge aufgenommen werden können; die neuesten Fotos sollen oben angezeigt werden.
- `OPEN-022` (CAN-REQ-029): Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles Symbol, etwa ein rotes Stoppschild, oben auf dem Screen zeigen; die Einträge sollen leicht zu durchforsten sein und nur die wichtigsten Informationen enthalten.
- `OPEN-023` (CAN-REQ-031): Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der Kommunikationsseite visuell mit Symbolen gekennzeichnet werden; jeder Bereich soll einen klaren Button zu weiterführenden Informationen haben.
- `OPEN-024` (CAN-REQ-034): Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein Plus-Button vorgesehen werden, um Videos und Beschreibungen hinzuzufügen.
- `OPEN-025` (CAN-REQ-037): Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des aktuellen Screens angezeigt werden, auf der Profilübersicht also „Profilübersicht“.
- `OPEN-026` (CAN-REQ-055): Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter der Einrichtung zu klären; der Umfang soll nicht so groß werden, dass der eigentliche Sinn der unterstützenden Kommunikation verloren geht.
- `OPEN-027` (CAN-REQ-064): Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.

