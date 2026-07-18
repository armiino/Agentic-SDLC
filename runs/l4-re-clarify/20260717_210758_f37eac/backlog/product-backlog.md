# Product Backlog

- backlogId: `product-backlog-20260718_114941` · projectId: `evidenz-agent-demo` · baselineId: `baseline-20260716_130307`
- Quelle: akzeptierte View (nach HumanReview/Apply) · Stand: 2026-07-18 11:49Z
- PBIs: **27** · Wahrheit bleibt `product-backlog.json` (dies ist die lesbare Projektion).

## Uebersicht

| PBI | Titel | MVP | Readiness | Rank | AK | offen |
|-----|-------|-----|-----------|------|----|-------|
| PBI-FC002-02 | Account-Verwaltung im Settings-Bereich für Admins bereitstellen | required_for_mvp | backlog_ready | 3 | 4 | 0 |
| PBI-FC009-01 | Bestehende Akten als Wissensquelle fachlich berücksichtigen | required_for_mvp | backlog_ready | 17 | 2 | 0 |
| PBI-FC003-03 | Optionale Hilfe- und Tutorial-Unterstützung planen | later | backlog_ready | 20 | 2 | 0 |
| PBI-FC005-03 | Optionale Benachrichtigung bei neuen About-Me-Inhalten planen | later | backlog_ready | 21 | 2 | 0 |
| PBI-FC011-01 | Technische Basis mit Flutter/Dart und gemeinsamen Tool-Versionen festlegen | required_for_mvp | backlog_ready | 23 | 4 | 0 |
| PBI-FC003-01 | Login-Screen ohne Selbstregistrierung bereitstellen | required_for_mvp | ready_with_nonblocking_questions | 4 | 5 | 3 |
| PBI-FC003-02 | Konsistenten App-Rahmen nach dem Login bereitstellen | required_for_mvp | ready_with_nonblocking_questions | 5 | 3 | 1 |
| PBI-FC004-01 | Profilübersicht mit zugänglichen Bewohnerprofilen bereitstellen | required_for_mvp | ready_with_nonblocking_questions | 6 | 5 | 2 |
| PBI-FC004-02 | Suchfunktion auf der Profilübersicht bereitstellen | required_for_mvp | ready_with_nonblocking_questions | 7 | 4 | 2 |
| PBI-FC007-01 | Kommunikationswissen in verbale und nonverbale Bereiche gliedern | required_for_mvp | ready_with_nonblocking_questions | 13 | 4 | 1 |
| PBI-FC010-01 | Qualitätsziele, Nutzungskontext und Barrierefreiheitskriterien festlegen | required_for_mvp | ready_with_nonblocking_questions | 19 | 6 | 3 |
| PBI-FC010-03 | Alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen | later | ready_with_nonblocking_questions | 22 | 2 | 1 |
| PBI-FC001-01 | MVP-Scope und Nicht-Ziele verbindlich festlegen | required_for_mvp | blocked_by_decision | 1 | 6 | 3 |
| PBI-FC002-01 | Rollen- und einrichtungsbezogene Zugriffsregeln festlegen | required_for_mvp | blocked_by_decision | 2 | 6 | 3 |
| PBI-FC005-01 | About-Me-Seite mit persönlichen Basisinformationen anzeigen | required_for_mvp | blocked_by_decision | 8 | 4 | 2 |
| PBI-FC005-02 | About-Me-Inhalte und Fotos dynamisch ergänzen | required_for_mvp | blocked_by_decision | 9 | 4 | 3 |
| PBI-FC006-01 | No-Go-Seite je Bewohner anzeigen | required_for_mvp | blocked_by_decision | 10 | 4 | 3 |
| PBI-FC006-02 | No-Go-Einträge dynamisch ergänzen | required_for_mvp | blocked_by_decision | 11 | 4 | 3 |
| PBI-FC004-03 | Anlegen neuer Bewohnerprofile fachlich klären | undecided | blocked_by_decision | 12 | 0 | 3 |
| PBI-FC007-02 | Kommunikationsvideos mit Beschreibungen integrieren | required_for_mvp | blocked_by_decision | 14 | 5 | 3 |
| PBI-FC007-03 | Suche in Kommunikationsinhalten mit Beschreibungsschema ermöglichen | required_for_mvp | blocked_by_decision | 15 | 4 | 2 |
| PBI-FC008-01 | Kalender-Scope und Schutzbedarf für Termine entscheiden | undecided | blocked_by_decision | 16 | 4 | 3 |
| PBI-FC009-02 | Übernahme aus Akten und Drittsystemen entscheiden | undecided | blocked_by_decision | 18 | 4 | 3 |
| PBI-FC010-02 | Offline- und Synchronisationsverhalten für instabile Verbindungen festlegen | required_for_mvp | blocked_by_decision | 20 | 5 | 2 |
| PBI-FC011-02 | State-Management-Entscheidung zu GetX explizit klären | required_for_mvp | blocked_by_decision | 24 | 3 | 1 |
| PBI-FC012-01 | Eingaberegeln und minimalen Content-Lifecycle für nutzergenerierte Inhalte festlegen | required_for_mvp | blocked_by_decision | 25 | 5 | 2 |
| PBI-FC012-02 | Rechtsgrundlage und Datenlebenszyklus für echte Bewohnerdaten festlegen | required_for_mvp | blocked_by_decision | 26 | 4 | 2 |

## Details

### PBI-FC002-02 — Account-Verwaltung im Settings-Bereich für Admins bereitstellen

`type=pbi` · `mvp=required_for_mvp` · `readiness=backlog_ready` · `rank=3`

**Statement:** Als Admin will ich im Settings-Bereich Nutzerkonten anlegen und Rechte verwalten, damit der Zugriff auf die App kontrolliert administriert werden kann.

**In Scope:**
- Rollenabhängigen Settings-Screen vorsehen
- Admin kann Nutzeraccounts anlegen
- Admin kann Rechte bestehender Accounts verwalten
- Jeder Nutzer kann eigenes Profil und persönliche Einstellungen wie Sprache ändern

**Out of Scope:**
- Selbstregistrierung
- Löschen fachlicher Inhalte
- Definition des vollständigen Rechtekonzepts über diese Verwaltungsoberfläche hinaus

**Akzeptanzkriterien:**
- Auf dem Settings-Screen sieht ein Admin Funktionen zum Anlegen neuer Accounts und zum Anpassen von Rechten.
- Nicht-Admin-Nutzer sehen diese administrativen Funktionen nicht.
- Jeder Nutzer kann im Settings-Bereich mindestens eigene Profileinstellungen und Spracheinstellungen ändern.
- Es gibt keine Selbstregistrierungsfunktion für neue Konten innerhalb der App.

**Traceability:** requirementIds: CAN-REQ-045, CAN-REQ-049, CAN-REQ-044
 · dependencies: PBI-FC002-01, PBI-FC003-01

### PBI-FC009-01 — Bestehende Akten als Wissensquelle fachlich berücksichtigen

`type=pbi` · `mvp=required_for_mvp` · `readiness=backlog_ready` · `rank=17`

**Statement:** Als Fachteam will ich bestehende Akten und dokumentierte Erfahrungen als relevante Wissensquelle berücksichtigen, damit wertvolles Bewohnerwissen nicht bei null neu erfasst werden muss.

**In Scope:**
- Bestehende Akten und dokumentierte Erfahrungen als fachliche Quelle benennen
- Relevante Inhaltsarten für spätere Übernahme oder manuelle Überführung identifizieren

**Out of Scope:**
- Technische Datenmigration
- Automatische Synchronisation
- Verbindliche Auswahl von Drittsystemen

**Akzeptanzkriterien:**
- Die Fachbeschreibung benennt bestehende Akten und dokumentierte Erfahrungen als zulässige Wissensquellen für Bewohnerinformationen.
- Es ist transparent dokumentiert, welche Informationsarten aus diesen Quellen fachlich relevant sind.

**Traceability:** requirementIds: CAN-REQ-056
 · dependencies: PBI-FC001-01

### PBI-FC003-03 — Optionale Hilfe- und Tutorial-Unterstützung planen

`type=pbi` · `mvp=later` · `readiness=backlog_ready` · `rank=20`

**Statement:** Als neuer Nutzer will ich bei Bedarf eine kurze Hilfe oder Tour erhalten, damit ich die App schneller verstehe.

**In Scope:**
- Kurze Tour beim ersten Login als spätere Option beschreiben
- Erneut aufrufbaren Hilfebereich als spätere Option beschreiben

**Out of Scope:**
- Umsetzung im MVP, solange nicht priorisiert
- Fachliche Schulungsinhalte außerhalb der App

**Akzeptanzkriterien:**
- Das Backlog enthält die spätere Option einer Erstlogin-Tour und eines erneut aufrufbaren Hilfebereichs als klar abgegrenzte Funktion.
- Das PBI ist gegenüber dem MVP als späterer Ausbau gekennzeichnet.

**Traceability:** requirementIds: CAN-REQ-041
 · dependencies: PBI-FC003-02

### PBI-FC005-03 — Optionale Benachrichtigung bei neuen About-Me-Inhalten planen

`type=pbi` · `mvp=later` · `readiness=backlog_ready` · `rank=21`

**Statement:** Als verbundener Nutzer einer Einrichtung will ich optional über neue About-Me-Inhalte informiert werden, damit ich relevante neue Informationen nicht verpasse.

**In Scope:**
- Popup-Benachrichtigung bei neuen About-Me-Inhalten als spätere Funktion beschreiben
- Benachrichtigung auf Nutzer derselben Einrichtung begrenzen

**Out of Scope:**
- MVP-Umsetzung
- Einrichtungsübergreifende Benachrichtigungen
- Komplexe Notification-Präferenzen

**Akzeptanzkriterien:**
- Die Backlog-Beschreibung begrenzt die Benachrichtigung auf verbundene Nutzer derselben Einrichtung.
- Die Funktion ist als spätere optionale Erweiterung markiert.

**Traceability:** requirementIds: CAN-REQ-042
 · dependencies: PBI-FC005-02

### PBI-FC011-01 — Technische Basis mit Flutter/Dart und gemeinsamen Tool-Versionen festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=backlog_ready` · `rank=23`

**Statement:** Als Entwicklungsteam will ich eine gemeinsame technische Basis und abgestimmte Werkzeuge nutzen, damit die plattformübergreifende Entwicklung konsistent und integrationsarm starten kann.

**In Scope:**
- Flutter mit Dart als gesetzten Stack nutzen
- Android Studio als bevorzugte Entwicklungsumgebung dokumentieren
- Gemeinsame Versionen der Entwicklungswerkzeuge festlegen
- Initiale Entwicklung und Tests zunächst gegen Android 11 ausrichten

**Out of Scope:**
- Fachliche App-Features
- Endgültige CI/CD-Definition
- Langfristige OS-Matrix jenseits der initialen Testausrichtung

**Akzeptanzkriterien:**
- Der Technologie-Stack ist als Flutter mit Dart dokumentiert.
- Die bevorzugte Entwicklungsumgebung Android Studio ist für das Team festgehalten.
- Es existiert eine abgestimmte Liste der zu verwendenden Werkzeugversionen.
- Die initiale Testausrichtung gegen Android 11 ist dokumentiert.

**Traceability:** requirementIds: CAN-REQ-060, CAN-REQ-061, CAN-REQ-062, CAN-REQ-063

### PBI-FC003-01 — Login-Screen ohne Selbstregistrierung bereitstellen

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=4`

**Statement:** Als Nutzer will ich mich über einen einfachen Login-Screen anmelden, damit ich ohne Umwege auf die für mich freigegebenen Inhalte zugreifen kann.

**In Scope:**
- Login-Möglichkeit mit E-Mail und Passwort
- Kein Registrierungsweg auf dem Login-Screen
- Logo sichtbar im oberen Drittel platzieren
- Barrierearme Gestaltung mit gut lesbaren Feldern und ausreichendem Kontrast

**Out of Scope:**
- Passwort-Reset, sofern nicht separat gefordert
- Selbstregistrierung
- Mehrfaktor-Authentifizierung, solange nicht verbindlich entschieden

**Akzeptanzkriterien:**
- Der Login-Screen enthält ein sichtbares Logo, ein E-Mail-Feld, ein Passwort-Feld und einen Login-Button.
- Auf dem Login-Screen gibt es keine Möglichkeit zur Selbstregistrierung.
- Logo und Eingabeelemente sind so angeordnet, dass das Logo im oberen Drittel zentriert sichtbar ist.
- Texte und Eingabefelder erfüllen mindestens die festgehaltenen Barrierefreiheitsprinzipien: gut lesbare Schrift und ausreichender Kontrast.
- Bei ungültigen Zugangsdaten wird kein Zugriff gewährt und der Nutzer bleibt auf dem Login-Screen.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche konkreten Barrierefreiheits- und Nutzungskontextkriterien gelten verbindlich für Smartphone und ggf. Tablet?
- [stakeholder_decision/open_decision/stated] Welche abgesicherte Authentifizierung ist für sensible Inhalte mindestens verpflichtend?
- [engineering_default/proposed_default/stated] Das endgültige Logo ist noch nicht erstellt; für den MVP wird ein Platzhalter-Branding benötigt.

**Traceability:** requirementIds: CAN-REQ-038, CAN-REQ-039, CAN-REQ-040, CAN-REQ-002, CAN-REQ-006, CAN-REQ-010, CAN-REQ-051
 · dependencies: PBI-FC002-01

### PBI-FC003-02 — Konsistenten App-Rahmen nach dem Login bereitstellen

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=5`

**Statement:** Als Nutzer will ich nach dem Login auf jeder Seite einen konsistenten App-Rahmen sehen, damit ich mich sicher orientieren und die Einstellungen jederzeit erreichen kann.

**In Scope:**
- Konsistente Appbar auf allen Seiten nach dem Login
- Einstellungssymbol in der Appbar
- Optional mittiger Titel des aktuellen Screens

**Out of Scope:**
- Inhalte einzelner Fachseiten
- Tutorial-Funktion
- Definition tiefer Navigationshierarchien jenseits der Appbar

**Akzeptanzkriterien:**
- Nach erfolgreichem Login wird auf jeder fachlichen Hauptseite dieselbe Grundstruktur der Appbar verwendet.
- Die Appbar enthält ein Einstellungssymbol, über das die Einstellungsseite erreichbar ist.
- Der aktuelle Screen bleibt für Nutzer eindeutig erkennbar; falls ein mittiger Titel umgesetzt wird, zeigt er den Seitentitel korrekt an.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Ist ein mittig angezeigter Screen-Titel in der Appbar verbindlich oder nur eine optionale Ausgestaltung?

**Traceability:** requirementIds: CAN-REQ-036, CAN-REQ-037
 · dependencies: PBI-FC003-01

### PBI-FC004-01 — Profilübersicht mit zugänglichen Bewohnerprofilen bereitstellen

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=6`

**Statement:** Als Mitarbeiter will ich nach dem Login eine übersichtliche Liste zugänglicher Bewohnerprofile sehen und ein Profil öffnen können, damit ich benötigte Informationen schnell finde statt in schwer lesbarer Dokumentation zu suchen.

**In Scope:**
- Profilübersichtsseite mit allen für den Nutzer zugänglichen Profilen
- Jedes Profil ist anklickbar
- Darstellung mit Vorschaudaten wie Bild, Name und Kurzbeschreibung
- Übergang in eine Profil-Detailansicht
- Barrierearme, schnell erfassbare Darstellung

**Out of Scope:**
- Anlegen neuer Profile
- Implementierung der vier Detailbereiche selbst
- Hausübergreifender Zugriff außerhalb definierter Rechte

**Akzeptanzkriterien:**
- Nach dem Login wird eine Profilübersicht mit allen für den Nutzer berechtigten Profilen angezeigt.
- Jeder Eintrag kann geöffnet werden und führt zu einer Detailansicht des gewählten Profils.
- Jeder angezeigte Profileintrag enthält mindestens Name sowie, sofern vorhanden und zulässig, Vorschaubild und kurze Beschreibung.
- Die Übersicht ist so gestaltet, dass Profile schneller auffindbar sind als in unstrukturierter Dokumentation.
- Schriftgröße, Kontrast und visuelle Komplexität entsprechen den festgehaltenen Barrierefreiheitsprinzipien.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Sollen Profile als Kacheln oder als Liste dargestellt werden?
- [stakeholder_decision/open_decision/stated] Soll die Profil-Detailansicht explizit vier interaktive Hauptbereichs-Buttons im ersten MVP zeigen?

**Traceability:** requirementIds: CAN-REQ-018, CAN-REQ-021, CAN-REQ-023, CAN-REQ-057, CAN-REQ-002, CAN-REQ-010, CAN-REQ-051
 · dependencies: PBI-FC003-02, PBI-FC002-01

### PBI-FC004-02 — Suchfunktion auf der Profilübersicht bereitstellen

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=7`

**Statement:** Als Nutzer will ich Profile über eine Suche schnell finden, damit ich nicht lange durch Listen scrollen muss.

**In Scope:**
- Suchfunktion auf der Profilübersicht
- Suche filtert die angezeigten zugänglichen Profile
- Sichtbare Positionierung der Suche nahe der Appbar als bevorzugte Ausgestaltung

**Out of Scope:**
- Volltextsuche innerhalb aller Fachinhalte
- Suche über nicht zugängliche Profile
- Leistungszusagen ohne definierte Qualitätsziele

**Akzeptanzkriterien:**
- Auf der Profilübersicht kann der Nutzer einen Suchbegriff eingeben und die sichtbare Profilmenge wird entsprechend gefiltert.
- Die Suche arbeitet nur auf Profilen, für die der Nutzer berechtigt ist.
- Bei leerem Suchbegriff werden alle zugänglichen Profile angezeigt.
- Wenn keine Treffer vorliegen, wird ein eindeutiger Leernachricht-Zustand angezeigt.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Ist die Suchleiste verbindlich direkt unter der Appbar zu platzieren?
- [stakeholder_decision/open_decision/stated] Welche maximalen Such- und Ladezeiten gelten als Abnahmekriterium?

**Traceability:** requirementIds: CAN-REQ-019, CAN-REQ-020, CAN-REQ-009
 · dependencies: PBI-FC004-01

### PBI-FC007-01 — Kommunikationswissen in verbale und nonverbale Bereiche gliedern

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=13`

**Statement:** Als Mitarbeiter will ich Kommunikationswissen eines Bewohners in klar getrennten verbalen und nonverbalen Bereichen sehen, damit ich Hinweise schneller einordnen und anwenden kann.

**In Scope:**
- Kommunikationsseite in verbale und nonverbale Bereiche unterteilen
- Bereiche klar visuell kennzeichnen
- Textuelle und visuelle Darstellungen unterstützen
- Buttons oder gleichwertige Elemente zu weiterführenden Informationen

**Out of Scope:**
- Hinzufügen von Videos
- Suche innerhalb der Kommunikationsinhalte
- Freigabeprozess für neue Inhalte

**Akzeptanzkriterien:**
- Die Kommunikationsansicht trennt Inhalte in einen verbalen und einen nonverbalen Bereich.
- Nutzer können klar erkennen, zu welchem Bereich ein Inhalt gehört.
- Kommunikationsinhalte können nicht nur textlich, sondern auch visuell dargestellt werden.
- Von beiden Bereichen ist der Zugang zu den zugehörigen Detailinformationen eindeutig möglich.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche visuellen Symbole oder Kennzeichnungen sollen im MVP für die Bereichstrennung verbindlich verwendet werden?

**Traceability:** requirementIds: CAN-REQ-030, CAN-REQ-031, CAN-REQ-050, CAN-REQ-002
 · dependencies: PBI-FC004-01, PBI-FC002-01

### PBI-FC010-01 — Qualitätsziele, Nutzungskontext und Barrierefreiheitskriterien festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=ready_with_nonblocking_questions` · `rank=19`

**Statement:** Als Produktteam will ich messbare Qualitätsziele und den Nutzungskontext des MVP festlegen, damit Abnahme, UX und Plattformverhalten verbindlich bewertbar sind.

**In Scope:**
- Messbare Ziele für Such- und Ladezeiten definieren
- Annahmen zu gleichzeitigen Nutzern, Profilanzahl und Mediengrößen festlegen
- Entscheidung Smartphone und ggf. Tablet-Support festhalten
- Konkrete Bedienbarkeits- und Barrierefreiheitskriterien definieren
- Plattformziel iOS und Android als Randbedingung dokumentieren
- Nicht ablenkende Animationen als Gestaltungsregel festhalten

**Out of Scope:**
- Implementierung einzelner Performance-Maßnahmen
- Detaillierte Betriebsarchitektur
- Festlegung alternativer Eingabemethoden im Detail

**Akzeptanzkriterien:**
- Es existiert ein dokumentierter Satz messbarer Qualitätsziele für Profilsuche/-laden, gleichzeitige Nutzer, Profilanzahl sowie zulässige Mediengrößen.
- Der Nutzungskontext beschreibt, in welchen Arbeitssituationen die App bedienbar sein muss und ob Tablets im MVP unterstützt werden.
- Die Abnahmekriterien enthalten konkrete Barrierefreiheitsleitplanken, mindestens zu Schriftgröße, Kontrast und Farbverwendung.
- Die Produktdefinition hält fest, dass die App auf iOS und Android laufen soll.
- Die UX-Leitplanken dokumentieren, dass Animationen nicht priorisiert und, falls vorhanden, nicht ablenkend gestaltet werden.
- Es ist festgehalten, ob minimale Zielwerte oder qualitative Leitplanken für Verfügbarkeit, Ausfallzeit und Wiederanlauf im MVP ergänzt werden.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche konkreten numerischen Grenzwerte gelten für Such- und Ladezeiten, Nutzerlast, Profilanzahl und Mediengrößen?
- [stakeholder_decision/open_decision/stated] Muss der MVP neben Smartphones auch Tablets unterstützen?
- [stakeholder_decision/open_decision/stated] Sollen für den MVP minimale Betriebsziele für Verfügbarkeit, maximal tolerierbare Ausfallzeit und Wiederanlauf festgelegt werden?

**Traceability:** requirementIds: CAN-REQ-009, CAN-REQ-010, CAN-REQ-051, CAN-REQ-052, CAN-REQ-053, CAN-DISK-003
 · dependencies: PBI-FC001-01

### PBI-FC010-03 — Alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen

`type=pbi` · `mvp=later` · `readiness=ready_with_nonblocking_questions` · `rank=22`

**Statement:** Als Produktteam will ich alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen, damit die Lösung möglichst zugänglich geplant wird.

**In Scope:**
- Sprachbefehle oder alternative Eingaben als zu prüfende Option im Backlog festhalten

**Out of Scope:**
- Verbindliche MVP-Umsetzung ohne weitere Entscheidung
- Spezifikation konkreter Assistive-Technology-Schnittstellen

**Akzeptanzkriterien:**
- Das Backlog enthält die Berücksichtigung alternativer Eingabemethoden als explizite Funktion oder Prüffrage.
- Die Anforderung ist gegenüber dem Kern-MVP abgegrenzt, solange keine verbindliche Priorisierung erfolgt.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche alternativen Eingabemethoden sind für den MVP oder spätere Releases tatsächlich erforderlich?

**Traceability:** requirementIds: CAN-REQ-043
 · dependencies: PBI-FC010-01

### PBI-FC001-01 — MVP-Scope und Nicht-Ziele verbindlich festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=1`

**Statement:** Als Projektverantwortliche will ich den MVP zwischen Unterstützungswerkzeug und möglicher Dokumentationsübernahme verbindlich abgrenzen, damit Produkt, Datenschutz und Aufwand auf einer klaren fachlichen Grundlage geplant werden können.

**In Scope:**
- Entscheidungsgrundlage mit 2-3 Scope-Optionen für den MVP dokumentieren
- Primären Produktzweck als Unterstützung zum schnellen Verstehen von Bewohnern festhalten
- Explizite Nicht-Ziele dokumentieren, insbesondere kein Übersetzungssystem zwischen Bewohnern und Betreuern
- Auswirkungen je Option auf Datenarten, Rollen, Datenschutz und UI beschreiben
- Beteiligte und spätere Nutzer in die Anforderungsanalyse einbeziehen

**Out of Scope:**
- Umsetzung einzelner Fachfeatures
- Festlegung technischer Architektur
- Übernahme vollständiger formaler Pflegedokumentation ohne gesonderte Entscheidung

**Akzeptanzkriterien:**
- Als Projektverantwortliche will ich eine freigabefähige Scope-Entscheidung mit mindestens zwei klar unterscheidbaren MVP-Optionen erhalten, damit der Projektstart verbindlich eingegrenzt ist.
- Für jede Scope-Option sind fachlicher Zweck, enthaltene Datenarten, betroffene Rollen, Datenschutzfolgen und UI-Folgen dokumentiert.
- Explizite Nicht-Ziele des MVP sind benannt, darunter der Ausschluss eines Übersetzungssystems zwischen Bewohnern und Betreuern.
- Die priorisierte Zielsetzung 'Bewohner besser verstehen' sowie die Unterstützung neuer Mitarbeiter und Erstbegegnungssituationen sind ausdrücklich beschrieben.
- Die Entscheidungsgrundlage benennt, welche offenen Folgeanforderungen erst nach der Scope-Entscheidung verbindlich werden.
- Die Anforderungsanalyse mit Beteiligten und späteren Nutzern ist als Arbeitsschritt geplant oder durchgeführt und im Scope-Artefakt referenziert.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche der dokumentierten Scope-Optionen wird für den MVP autorisiert?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Bleibt die Lösung im MVP ausschließlich Unterstützungswerkzeug oder übernimmt sie später Teile formaler Dokumentation?
- [stakeholder_decision/open_decision/stated] Soll das gewünschte Zielbild als App/Computerprogramm formal bestätigt werden?

**Traceability:** requirementIds: CAN-OPEN-001, CAN-REQ-001, CAN-REQ-013, CAN-REQ-014, CAN-REQ-015, CAN-REQ-054, CAN-REQ-055, CAN-REQ-058, CAN-REQ-064

### PBI-FC002-01 — Rollen- und einrichtungsbezogene Zugriffsregeln festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=2`

**Statement:** Als Admin will ich ein klares Rollen- und Rechtekonzept für Mitarbeiter, Angehörige und Bewohner nutzen, damit nur berechtigte Personen die für ihre Einrichtung zulässigen Inhalte sehen und bearbeiten können.

**In Scope:**
- Rollen Mitarbeiter, Angehörige, Bewohner und Admin beschreiben
- Je Rolle Rechte für Sehen, Anlegen, Bearbeiten, Freigeben, Exportieren, Verbergen und Löschen pro Funktionsbereich festlegen
- Einrichtungsbezogene Zugriffstrennung über Häusergrenzen definieren
- Festhalten, dass User-Inhalte hinzufügen dürfen, aber keine Accounts erstellen und keine Daten löschen dürfen
- Bewohner-Account auf eigenes Profil und eingeschränkte Funktionen begrenzen
- Interne Nutzung der App als Rahmenbedingung dokumentieren

**Out of Scope:**
- Implementierung des Logins
- Technische Umsetzung einzelner Sicherheitsmechanismen
- Benutzeroberfläche der Settings-Seite zur Administration

**Akzeptanzkriterien:**
- Für die Rollen Admin, Mitarbeiter, Angehörige und Bewohner liegt eine Rechte-Matrix pro Funktionsbereich vor.
- Die Rechte-Matrix definiert explizit, welche Rollen Profile sehen, Inhalte anlegen, bearbeiten, freigeben, exportieren, verbergen oder löschen dürfen.
- Die Matrix enthält eine verbindliche Regel, dass Mitarbeiter nur Profile der eigenen Einrichtung sehen dürfen, sofern keine ausdrücklich autorisierte Ausnahme definiert ist.
- Für Bewohner ist festgelegt, dass nur das eigene Profil sichtbar ist und nur die eingeschränkten Funktionen des About-Me-Bereichs einschließlich eigener Bilder nutzbar sind.
- Für Nicht-Admin-Rollen ist festgelegt, dass keine Accounts erstellt werden dürfen und kein Löschen bereits hinzugefügter Daten erlaubt ist.
- Die App ist als internes System beschrieben; externe öffentliche Registrierung oder öffentlicher Zugriff sind ausgeschlossen.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche zusätzlichen Sicherheitsmaßnahmen sind für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe verbindlich?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Unter welcher Rechtsgrundlage und mit welchen Einwilligungen dürfen echte Bewohnerdaten und Bilder von den definierten Rollen genutzt werden?
- [stakeholder_decision/partial/stated] Wie sind Export, Korrektur, Archivierung und Löschung je Rolle im Datenlebenszyklus genau freigegeben?

**Traceability:** requirementIds: CAN-REQ-002, CAN-REQ-006, CAN-REQ-016, CAN-REQ-017, CAN-REQ-044, CAN-REQ-046, CAN-REQ-047, CAN-REQ-048, CAN-REQ-005, CAN-REQ-011
 · dependencies: PBI-FC001-01

### PBI-FC005-01 — About-Me-Seite mit persönlichen Basisinformationen anzeigen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=8`

**Statement:** Als Mitarbeiter oder berechtigter Angehöriger will ich eine persönliche About-Me-Seite je Bewohner sehen, damit ich schnell einen ersten informativen Eindruck der Person erhalte.

**In Scope:**
- About-Me-Seite pro Person anzeigen
- Basisinformationen wie Name, Alter, Hobbys, Bilder und Beschreibung darstellen
- Infobox als bevorzugte Ausgestaltung berücksichtigen

**Out of Scope:**
- Hochladen neuer Inhalte
- Benachrichtigungen über neue Inhalte
- Freigabe-Workflow für Änderungen

**Akzeptanzkriterien:**
- Für jeden berechtigten Bewohner kann eine About-Me-Seite geöffnet werden.
- Die Seite zeigt die verfügbaren Basisinformationen des Bewohners in einer persönlichen, informativen Form.
- Mindestens Name und Beschreibung sind sichtbar, weitere Felder wie Alter, Hobbys und Bilder werden angezeigt, sofern vorhanden und freigegeben.
- Die Darstellung ist auf schnellen Erstüberblick ausgelegt und klar von Kommunikations- und No-Go-Inhalten getrennt.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Ist die Infobox mit Alter und Hobbys im MVP verbindlich oder nur bevorzugtes Layout?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Unter welcher Rechtsgrundlage und mit welcher Freigabe dürfen echte Bilder und personenbezogene Daten angezeigt werden?

**Traceability:** requirementIds: CAN-REQ-024, CAN-REQ-026, CAN-REQ-002, CAN-REQ-011
 · dependencies: PBI-FC004-01, PBI-FC002-01

### PBI-FC005-02 — About-Me-Inhalte und Fotos dynamisch ergänzen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=9`

**Statement:** Als berechtigter Nutzer will ich neue About-Me-Inhalte und Fotos zu einem Bewohner ergänzen, damit das Profil mit neuen Erfahrungen aktuell gehalten wird.

**In Scope:**
- Neue Bilder und ergänzende About-Me-Inhalte hinzufügen
- Neueste Foto- oder Timeline-Einträge oben anzeigen
- Plus-Button als bevorzugte Ausgestaltung für neue Einträge

**Out of Scope:**
- Automatische Benachrichtigung an andere Nutzer
- Endgültiger Freigabeprozess über alle Rollenkonstellationen
- Endgültige Löschregeln und Archivierungslogik

**Akzeptanzkriterien:**
- Berechtigte Nutzer können einem Bewohner neue About-Me-Inhalte hinzufügen.
- Neu hinzugefügte Foto- oder Timeline-Einträge werden in absteigender Aktualität angezeigt, das neueste Element steht oben.
- Leere oder unvollständige Eingaben werden gemäß den festgelegten Eingaberegeln nicht als gültiger Eintrag übernommen.
- Wenn ein Bild oder Inhalt erfolgreich hinzugefügt wurde, ist er auf der About-Me-Seite sichtbar oder als noch nicht freigegeben gekennzeichnet.

**Offene Entscheidungen:**
- [stakeholder_decision/partial/stated] Welche minimalen Pflichtfelder und zulässigen Medienformate gelten für neue About-Me-Einträge?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welchen minimalen Bearbeitungs- und Freigabeprozess durchlaufen neue oder geänderte About-Me-Inhalte im MVP?
- [stakeholder_decision/open_decision/stated] Welche Aufbewahrungs-, Korrektur-, Archivierungs- und Löschregeln gelten für Fotos und About-Me-Inhalte?

**Traceability:** requirementIds: CAN-REQ-025, CAN-REQ-026, CAN-REQ-003, CAN-REQ-004, CAN-REQ-005
 · dependencies: PBI-FC005-01, PBI-FC012-01

### PBI-FC006-01 — No-Go-Seite je Bewohner anzeigen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=10`

**Statement:** Als Mitarbeiter will ich vor einer Interaktion schnell sehen, was bei einem Bewohner unbedingt zu vermeiden ist, damit ich kritische Situationen verhindern kann.

**In Scope:**
- No-Go-Seite pro Bewohner anzeigen
- Einträge als leicht durchforstbare Liste darstellen
- Starkes visuelles Warnsymbol als bevorzugte Ausgestaltung berücksichtigen
- Nur wichtigste Informationen darstellen

**Out of Scope:**
- Hinzufügen neuer No-Go-Einträge
- Komplexe Kategorisierung
- Öffentliche Sichtbarkeit für unberechtigte Nutzer

**Akzeptanzkriterien:**
- Für berechtigte Nutzer ist pro Bewohner eine No-Go-Seite erreichbar.
- Die Seite zeigt eine klar durchsuchbare oder scanbare Liste der vorhandenen No-Go-Einträge.
- Die Darstellung priorisiert knappe, wichtige Informationen gegenüber langen Fließtexten.
- Nicht berechtigte Rollen erhalten keinen Zugriff auf die No-Go-Seite.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Ist ein starkes visuelles Symbol wie ein rotes Stoppschild verbindlicher Bestandteil des MVP?
- [stakeholder_decision/open_decision/stated] Welche zusätzlichen Sicherheitsmaßnahmen und Protokollierungen sind für den Zugriff auf No-Go-Inhalte Pflicht?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Unter welcher Rechtsgrundlage dürfen sensible No-Go-Inhalte mit echten Bewohnerdaten angezeigt werden?

**Traceability:** requirementIds: CAN-REQ-027, CAN-REQ-029, CAN-REQ-002, CAN-REQ-006, CAN-REQ-011
 · dependencies: PBI-FC004-01, PBI-FC002-01

### PBI-FC006-02 — No-Go-Einträge dynamisch ergänzen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=11`

**Statement:** Als berechtigter Nutzer will ich neue No-Go-Hinweise zu einem Bewohner hinzufügen, damit andere sich vorab über kritische Themen informieren können.

**In Scope:**
- Plus-Button oder gleichwertige Aktion zum Hinzufügen neuer No-Go-Einträge
- Dynamisch erweiterbare Liste
- Validierung leerer oder unvollständiger Eingaben

**Out of Scope:**
- Endgültige Löschfunktion
- Finaler Freigabe- und Archivierungsprozess über alle Rollen
- Automatische Risiko-Klassifizierung

**Akzeptanzkriterien:**
- Berechtigte Nutzer können neue No-Go-Einträge zu einem Bewohner anlegen.
- Ein neu angelegter Eintrag erscheint in der No-Go-Liste oder ist als noch nicht freigegeben gekennzeichnet.
- Leere Eingaben werden nicht gespeichert.
- Der Eintrag ist in einer Form erfasst, die für andere Nutzer leicht vorab lesbar ist.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Welche Pflichtfelder und maximale Textlängen gelten für No-Go-Einträge?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welcher minimale Freigabeprozess gilt für neue oder geänderte No-Go-Inhalte im MVP?
- [stakeholder_decision/open_decision/stated] Welche Archivierungs- und Löschregeln gelten für No-Go-Inhalte?

**Traceability:** requirementIds: CAN-REQ-028, CAN-REQ-003, CAN-REQ-004, CAN-REQ-005
 · dependencies: PBI-FC006-01, PBI-FC012-01

### PBI-FC004-03 — Anlegen neuer Bewohnerprofile fachlich klären

`type=pbi` · `mvp=undecided` · `readiness=blocked_by_decision` · `rank=12`

**Statement:** Als Admin oder berechtigter Nutzer will ich klären, ob und wie neue Bewohnerprofile aus der Profilübersicht angelegt werden, damit Verantwortlichkeiten und Mindestdaten für neue Profile eindeutig sind.

**In Scope:**
- Entscheidung, ob es ein Plus-Symbol zum Anlegen neuer Profile gibt
- Definition der minimal zu erfassenden Angaben Bild, Name, Beschreibung bzw. Pflichtdaten
- Rollenklärung, wer Profile anlegen darf

**Out of Scope:**
- Technische Import-Schnittstellen
- Massenerstellung von Profilen
- Workflow für spätere Löschung oder Archivierung

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Dürfen neue Bewohnerprofile im MVP manuell über die Profilübersicht angelegt werden?
- **[BLOCKS]** [stakeholder_decision/open_decision/derived] Welche Rolle darf neue Profile anlegen: nur Admin oder weitere berechtigte Nutzer?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche Pflichtfelder und Validierungen gelten beim Anlegen eines neuen Profils?

**Traceability:** requirementIds: CAN-REQ-022, CAN-REQ-003, CAN-REQ-002
 · dependencies: PBI-FC002-01

### PBI-FC007-02 — Kommunikationsvideos mit Beschreibungen integrieren

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=14`

**Statement:** Als berechtigter Nutzer will ich Videos mit Beschreibungen zu Kommunikationssituationen in die verbalen oder nonverbalen Bereiche einpflegen und ansehen, damit Wissen anschaulich vermittelt wird.

**In Scope:**
- Videos mit Beschreibungen in Kommunikationsbereiche integrieren
- Neue Videos mit Beschreibung hinzufügen
- Neueste Videos oben anzeigen
- Plus-Button im nonverbalen Bereich als bevorzugte Ausgestaltung berücksichtigen

**Out of Scope:**
- Eigenständiger Video-Screen
- Automatische Videoanalyse
- Finale Mediengrößen- und Performancezusagen ohne Qualitätsentscheidung

**Akzeptanzkriterien:**
- Videos mit zugehöriger Beschreibung können innerhalb eines Kommunikationsbereichs angezeigt werden.
- Berechtigte Nutzer können neue Videos mit Beschreibung hinzufügen.
- Neu hinzugefügte Videos werden in absteigender Aktualität angezeigt, das neueste Video steht oben.
- Die Videofunktion erscheint nicht als separater Hauptscreen, sondern eingebettet im jeweiligen Kommunikationsbereich.
- Leere Beschreibungen oder fehlende Pflichtangaben werden gemäß den Eingaberegeln nicht als vollständiger Eintrag übernommen.

**Offene Entscheidungen:**
- [stakeholder_decision/open_decision/stated] Ist ein eigener Plus-Button speziell im nonverbalen Bereich im MVP verbindlich?
- [stakeholder_decision/open_decision/stated] Welche zulässigen Mediengrößen, Formate und Ladezeitgrenzen gelten für Kommunikationsvideos?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welcher minimale Freigabeprozess gilt für neue oder geänderte Videoeinträge im MVP?

**Traceability:** requirementIds: CAN-REQ-032, CAN-REQ-033, CAN-REQ-034, CAN-REQ-003, CAN-REQ-004, CAN-REQ-005, CAN-REQ-009
 · dependencies: PBI-FC007-01, PBI-FC012-01

### PBI-FC007-03 — Suche in Kommunikationsinhalten mit Beschreibungsschema ermöglichen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=15`

**Statement:** Als Mitarbeiter will ich Kommunikationshinweise durchsuchen können, damit ich in Verständigungssituationen schnell passende Informationen finde.

**In Scope:**
- Suchfunktion auf Kommunikationsseiten
- Suchbarkeit anhand eines einheitlichen Beschreibungsmusters vorbereiten
- Filterung oder Auffindung passender Kommunikationsinhalte

**Out of Scope:**
- Semantische KI-Suche
- Suche ohne definiertes Beschreibungsschema
- Einrichtungsübergreifende globale Suche

**Akzeptanzkriterien:**
- Auf Kommunikationsseiten kann der Nutzer nach Kommunikationsinhalten suchen.
- Die Suchfunktion basiert auf einem festgelegten Beschreibungsschema oder einer gleichwertig strukturierten Eingabegrundlage.
- Bei leerem Suchbegriff bleibt die vollständige Menge der zugänglichen Kommunikationsinhalte sichtbar.
- Wenn das Beschreibungsschema noch nicht final ist, darf die Umsetzung nur die bereits verbindlich definierten Felder durchsuchen.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Wie lautet das verbindliche Beschreibungsschema für Kommunikations-Einträge, damit Suche und Filterung zuverlässig funktionieren?
- [stakeholder_decision/open_decision/stated] Welche maximalen Such- und Ladezeiten gelten für die Kommunikationssuche?

**Traceability:** requirementIds: CAN-REQ-035, CAN-REQ-003, CAN-REQ-009
 · dependencies: PBI-FC012-01, PBI-FC007-01

### PBI-FC008-01 — Kalender-Scope und Schutzbedarf für Termine entscheiden

`type=pbi` · `mvp=undecided` · `readiness=blocked_by_decision` · `rank=16`

**Statement:** Als Projektverantwortliche will ich entscheiden, ob der Kalender im MVP nur allgemeine Termine oder auch medizinisch sensible Informationen enthält, damit Schutzbedarf, Rechte und Umfang vor Umsetzung geklärt sind.

**In Scope:**
- Entscheidung allgemeine Termine vs. medizinisch sensible Termine
- Folgen für Zugriffsregeln und Schutzbedarf dokumentieren
- Folgen für Eingaberegeln und Datenlebenszyklus benennen

**Out of Scope:**
- Implementierung einer Kalenderfunktion
- Medizinische Dokumentation ohne gesonderte Freigabe
- Schnittstellen zu externen Kalendersystemen

**Akzeptanzkriterien:**
- Es liegt eine dokumentierte Entscheidungsvorlage für den Kalender-MVP vor.
- Die Vorlage unterscheidet mindestens die Optionen 'nur allgemeine Termine' und 'inklusive medizinisch sensible Informationen'.
- Für jede Option sind Schutzbedarf, notwendige Rollen- und Zugriffsregeln sowie Scope-Folgen beschrieben.
- Wenn medizinisch sensible Informationen enthalten sein sollen, ist der zusätzliche Freigabebedarf ausdrücklich benannt.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Enthält der MVP-Kalender ausschließlich allgemeine Termine oder auch medizinisch sensible Informationen wie Medikamentengaben?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche spezifischen Sicherheitsmaßnahmen und Protokollierungen gelten bei medizinisch sensiblen Terminen?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Unter welcher Rechtsgrundlage dürfen echte sensible Termindaten verarbeitet werden?

**Traceability:** requirementIds: CAN-REQ-012, CAN-REQ-002, CAN-REQ-003, CAN-REQ-005, CAN-REQ-006, CAN-REQ-011
 · dependencies: PBI-FC001-01, PBI-FC002-01

### PBI-FC009-02 — Übernahme aus Akten und Drittsystemen entscheiden

`type=pbi` · `mvp=undecided` · `readiness=blocked_by_decision` · `rank=18`

**Statement:** Als Projektverantwortliche will ich entscheiden, ob und wie Bewohnerdaten aus Akten oder Drittsystemen übernommen werden, damit Medienbrüche, Verantwortlichkeiten und Datenschutz vor Umsetzung geklärt sind.

**In Scope:**
- Entscheidung über manuelle oder systemgestützte Übernahme aus bestehenden Quellen
- Quelle, Datenumfang, Übertragungsweg und Verantwortlichkeit beschreiben
- Umgang mit Medienbrüchen und Doppelpflege festlegen

**Out of Scope:**
- Technische Umsetzung konkreter Schnittstellen
- Vollständige Ablösung bestehender Dokumentationssysteme ohne Scope-Entscheidung
- Migration aller historischen Daten

**Akzeptanzkriterien:**
- Es liegt eine dokumentierte Entscheidungsgrundlage zur Datenübernahme vor.
- Die Grundlage benennt je betrachteter Quelle den vorgesehenen Datenumfang, den Übertragungsweg und die fachliche Verantwortung.
- Die Grundlage beschreibt, ob im MVP manuelle Doppelpflege, einmalige Übernahme oder laufende Synchronisation vorgesehen ist.
- Datenschutz- und Freigabefolgen der gewählten Variante sind benannt.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Werden Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen im MVP übernommen?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche konkreten Quellen und welche Datenkategorien sind für eine Übernahme zugelassen?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Dürfen echte Daten aus Bestandsquellen bereits in Test- und Erstbefüllungskontexten verwendet werden?

**Traceability:** requirementIds: CAN-REQ-007, CAN-REQ-001, CAN-REQ-005, CAN-REQ-011
 · dependencies: PBI-FC001-01, PBI-FC012-02

### PBI-FC010-02 — Offline- und Synchronisationsverhalten für instabile Verbindungen festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=20`

**Statement:** Als Produkt- und Betriebsteam will ich verbindlich festlegen, wie sich die App bei fehlender oder instabiler Internetverbindung verhält, damit Nutzungssicherheit, Datensicherung und Konfliktbehandlung vor Implementierung klar sind.

**In Scope:**
- Entscheidung, ob lokale Zwischenspeicherung im MVP unterstützt wird
- Synchronisationszeitpunkte nach Wiederverbindung definieren
- Konfliktauflösung bei konkurrierenden Änderungen nach Wiederverbindung festlegen
- Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb beschreiben

**Out of Scope:**
- Technische Implementierung der Offline-Architektur
- Detailliertes Monitoring
- Optimierung einzelner Fachscreens für Offline-Nutzung ohne Grundsatzentscheidung

**Akzeptanzkriterien:**
- Es liegt eine dokumentierte Offline-/Online-Betriebsentscheidung für den MVP vor.
- Die Entscheidung beschreibt, ob Inhalte lokal zwischengespeichert werden und welche Datenarten davon betroffen sind.
- Die Entscheidung legt fest, wann Synchronisation nach Wiederverbindung erfolgt.
- Die Entscheidung beschreibt, wie Konflikte nach Offline-Änderungen behandelt werden.
- Die Entscheidung enthält Leitplanken für Datensicherung und Wiederherstellung im Firestore-basierten Betrieb.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Muss der MVP bei fehlender oder instabiler Internetverbindung lokale Zwischenspeicherung unterstützen?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Wie werden Konflikte behandelt, wenn nach Wiederverbindung mehrere Änderungen auf denselben Inhalt treffen?

**Traceability:** requirementIds: CAN-REQ-008
 · dependencies: PBI-FC001-01, PBI-FC012-01

### PBI-FC011-02 — State-Management-Entscheidung zu GetX explizit klären

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=24`

**Statement:** Als Entwicklungsteam will ich vor Projektstart explizit entscheiden, ob GetX als State-Management-Rahmen verwendet wird, damit Architektur- und Testentscheidungen nicht auf einer uneindeutigen Annahme beruhen.

**In Scope:**
- GetX als offene Technikentscheidung ausweisen
- Entscheidungsoptionen und Bewertungskriterien dokumentieren
- Entscheidungstermin für Projektstart festlegen

**Out of Scope:**
- Implementierung fachlicher Features
- Vergleich aller möglichen Architekturthemen jenseits State-Management

**Akzeptanzkriterien:**
- Die Anforderung zu GetX ist im Backlog als explizite Technikentscheidung und nicht als stillschweigende Festlegung beschrieben.
- Die Entscheidungsgrundlage enthält mindestens die Optionen 'GetX verwenden' und 'Alternative verwenden'.
- Für die Entscheidung sind Bewertungskriterien und ein Entscheidungstermin dokumentiert.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Wird GetX für das Projekt verbindlich gewählt oder wird eine Alternative eingesetzt?

**Traceability:** requirementIds: CAN-OPEN-004, CAN-REQ-059
 · dependencies: PBI-FC011-01

### PBI-FC012-01 — Eingaberegeln und minimalen Content-Lifecycle für nutzergenerierte Inhalte festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=25`

**Statement:** Als Fach- und Produktteam will ich verbindliche Eingaberegeln und einen minimalen Lifecycle für nutzergenerierte Inhalte definieren, damit About-Me-, No-Go- und Kommunikationsinhalte konsistent, prüfbar und verantwortbar gepflegt werden können.

**In Scope:**
- Pflichtfelder, zulässige Formate und maximale Feldlängen für Inhalte festlegen
- Verhalten bei leeren oder unvollständigen Eingaben definieren
- Einheitliches Beschreibungsschema für Kommunikations-Einträge definieren
- Minimalen Lifecycle mit Status und verantwortlichen Rollen festlegen
- Regeln für Zurückweisung, Korrektur und parallele Änderungen beschreiben

**Out of Scope:**
- Technische Workflow-Engine
- Vollständige Governance für alle späteren Inhaltstypen
- Detailgestaltung einzelner Fachseiten

**Akzeptanzkriterien:**
- Für About-Me-, No-Go- und Kommunikationsinhalte sind Pflichtfelder, zulässige Formate und maximale Feldlängen dokumentiert.
- Für leere oder unvollständige Eingaben ist ein verbindliches Verhalten beschrieben.
- Für Kommunikations-Einträge liegt ein einheitliches Beschreibungsschema vor, das Suche und Filterung ermöglicht.
- Der minimale MVP-Content-Lifecycle definiert Statuswerte, verantwortliche Rollen je Statusübergang sowie Regeln für Zurückweisung oder Korrektur.
- Der Umgang mit parallelen Änderungen und Versionserhalt ist festgehalten.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche minimalen Statuswerte und Rollenübergänge sind im MVP verpflichtend?
- [stakeholder_decision/open_decision/stated] Wie genau wird bei gleichzeitigen Änderungen verfahren?

**Traceability:** requirementIds: CAN-OPEN-005, CAN-REQ-003, CAN-REQ-004, CAN-REQ-024, CAN-REQ-027, CAN-REQ-030
 · dependencies: PBI-FC002-01

### PBI-FC012-02 — Rechtsgrundlage und Datenlebenszyklus für echte Bewohnerdaten festlegen

`type=pbi` · `mvp=required_for_mvp` · `readiness=blocked_by_decision` · `rank=26`

**Statement:** Als Produktverantwortliche will ich die Rechtsgrundlage, Einwilligungen und den Datenlebenszyklus für echte Bewohnerdaten und Medien verbindlich festlegen, damit Analyse, Test, Erstbefüllung und Produktivbetrieb datenschutzkonform erfolgen können.

**In Scope:**
- Zulässige Datenkategorien und Rechtsgrundlagen definieren
- Regeln für Nutzung echter Bewohnerdaten, Bilder und Videos in Analyse, Test, Erstbefüllung und Produktivbetrieb festlegen
- Dokumentation von Einwilligungen und verantwortlichen Freigaberollen festlegen
- Aufbewahrung, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen und Export regeln

**Out of Scope:**
- Technische Implementierung eines Consent-Management-Systems
- Fachliche Ausgestaltung einzelner Inhaltsfeatures
- Verhandlung externer Verträge mit Drittsystemen

**Akzeptanzkriterien:**
- Es liegt eine dokumentierte Zuordnung zulässiger Datenkategorien zu ihrer Rechtsgrundlage vor.
- Die Regeln unterscheiden ausdrücklich zwischen Analyse, Test, inhaltlicher Erstbefüllung und Produktivbetrieb.
- Die organisatorischen Rollen für datenschutzrechtliche Freigaben und die Dokumentation von Einwilligungen sind benannt.
- Für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte sind Aufbewahrung, Korrektur, Archivierung, Löschung, Widerruf und Export beschrieben.

**Offene Entscheidungen:**
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche Rechtsgrundlage gilt je Datenkategorie und unter welchen Bedingungen dürfen echte Bewohnerdaten, Bilder und Videos in Test und Produktivbetrieb verwendet werden?
- **[BLOCKS]** [stakeholder_decision/open_decision/stated] Welche organisatorische Rolle erteilt die datenschutzrechtliche Freigabe verbindlich?

**Traceability:** requirementIds: CAN-OPEN-002, CAN-REQ-005, CAN-REQ-011, CAN-REQ-007
 · dependencies: PBI-FC001-01

