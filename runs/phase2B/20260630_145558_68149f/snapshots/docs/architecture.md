# Architekturüberblick

## Ziel und Einordnung
Dieses Dokument beschreibt einen frühen Architekturüberblick für das geplante System zur digitalen Unterstützung der Kommunikation im Betreuungsalltag. Die Architektur leitet sich aus dem fachlichen Kern ab: vorhandenes Erfahrungswissen über individuelle Kommunikationsweisen von Bewohnern strukturiert erfassen, alltagstauglich aufbereiten und für berechtigte Nutzer schnell verfügbar machen.

Das System ist ausdrücklich **keine automatische Übersetzungs- oder Interpretationslösung**. Es dient der Dokumentation, Einordnung und Auffindbarkeit von bereits vorhandenem Wissen.

## Systemkontext
Das System ist als mobile, alltagsnahe Anwendung für berechtigte Nutzer in Betreuungseinrichtungen gedacht. Es unterstützt vor allem Mitarbeitende, perspektivisch möglicherweise auch Angehörige und optional Bewohner mit stark eingeschränkten Rechten.

### Zentrale Akteure
- **Administratoren** verwalten Nutzer, Rollen und organisatorische Zuordnungen.
- **Reguläre Nutzer** wie Mitarbeitende pflegen und nutzen Bewohnerinformationen im Rahmen ihrer Berechtigung.
- **Angehörige** können optional als gesondert berechtigte Nutzergruppe eingebunden werden.
- **Bewohner** könnten optional einen stark eingeschränkten Zugang zu ihrem eigenen Profil erhalten.

### Fachliche Umgebung
Das System steht neben bereits vorhandener Dokumentation und ersetzt diese in der frühen Betrachtung nicht zwingend vollständig. Sein Mehrwert liegt vor allem in:
- schnellerem Zugriff auf alltagsrelevante Informationen,
- kompakterer Darstellung,
- besserer Auffindbarkeit von Kommunikationshinweisen,
- Unterstützung der Wissensweitergabe an neue oder fachfremde Personen.

## Architekturprinzipien
Aus Requirements und Risiken ergeben sich folgende Leitprinzipien:

1. **Fokus auf Kernnutzen**  
   Priorität haben Bewohnerprofile, Kommunikationswissen, Suche und No-Go-Hinweise. Nachrangige Funktionen wie Kalender oder Medikamenteninformationen werden architektonisch nur als optionale Erweiterungen betrachtet.

2. **Security und Datenschutz by Design**  
   Da hochsensible personenbezogene Daten verarbeitet werden, müssen Berechtigung, Bereichstrennung und kontrollierter Medienzugriff zentrale Architekturthemen sein.

3. **Mobile und schnelle Nutzung**  
   Die Informationsarchitektur muss kurze Nutzungssituationen unterstützen: wenige Schritte, klare Navigation, kompakte Ansichten.

4. **Trennung von gesicherten Stammdaten und kontextabhängigem Erfahrungswissen**  
   Kommunikationshinweise sind nicht als objektive Wahrheit zu behandeln. Das System sollte beobachtungsbezogene Inhalte als ergänzbares, aktualisierbares Wissen abbilden.

5. **Erweiterbarkeit ohne Überfrachtung**  
   Optionale Funktionen und Medienarten sollen ergänzbar sein, ohne den Kern der Anwendung zu destabilisieren.

## Wichtige Komponenten

### 1. Client / Benutzeroberfläche
Die Benutzeroberfläche ist der mobile Zugangspunkt für berechtigte Nutzer.

**Aufgaben:**
- Login und Zugriff auf freigegebene Inhalte
- Profilübersicht und Navigation zu Bewohnerprofilen
- Darstellung von Kommunikationshinweisen, Profilinformationen und No-Go-Hinweisen
- Suche und ggf. Filterung innerhalb von Profilinhalten
- Erfassung und Pflege neuer Beobachtungen
- Bereitstellung von Hilfen/Tutorials
- barrierearme Darstellung mit klarer Navigation, gut lesbarer Schrift und reduziertem Layout

**Architekturelle Bedeutung:**
Die Oberfläche ist wesentlich für den Projekterfolg, weil das Kernproblem nicht fehlende Daten, sondern mangelnde Alltagstauglichkeit bestehender Dokumentation ist.

### 2. Authentifizierungs- und Autorisierungskomponente
Diese Komponente steuert Anmeldung, Rollen und Zugriffsrechte.

**Aufgaben:**
- geschlossener Zugang ohne offene Selbstregistrierung
- Zuordnung von Nutzern zu Rollen
- Prüfung, welche Profile und Inhalte ein Nutzer sehen oder bearbeiten darf
- Durchsetzung organisatorischer Grenzen, z. B. nach Einrichtung oder Zuständigkeitsbereich

**Architekturelle Bedeutung:**
Diese Komponente ist sicherheitskritisch, weil unterschiedliche Nutzergruppen und sensible Daten kombiniert werden.

### 3. Bewohnerprofil-Komponente
Diese Komponente verwaltet die fachliche Klammer pro Bewohner.

**Aufgaben:**
- Verwaltung grundlegender Profilinformationen
- kompakte persönliche Einführung („About Me“)
- Zusammenführung der zugehörigen Kommunikations-, Medien- und Warninformationen
- Einstiegspunkt für schnelle Orientierung über eine Person

**Typische Inhalte:**
- Name
- Alter
- Hobbys
- allgemeine persönliche Informationen
- ggf. Bilder mit Beschreibung

### 4. Kommunikationswissens-Komponente
Dies ist der fachliche Kern des Systems.

**Aufgaben:**
- Erfassung individueller Kommunikationsweisen pro Bewohner
- Unterscheidung zwischen verbalen und nonverbalen Hinweisen
- Pflege beschreibender Texte
- Verknüpfung mit Bildern und optional Videos
- fortlaufende Ergänzung neuer Beobachtungen
- möglichst kontextbezogene, aktualisierbare Wissensdarstellung

**Architekturelle Bedeutung:**
Die Komponente sollte so modelliert sein, dass Beobachtungen, Interpretationen und erläuternde Medien nicht als starre oder absolut gültige Aussage erscheinen.

### 5. No-Go- und Warnhinweis-Komponente
Diese Komponente stellt kritische Informationen mit hoher Sichtbarkeit bereit.

**Aufgaben:**
- Verwaltung von Triggern, Warnungen und zu vermeidenden Reizen
- schnelle, gut erkennbare Darstellung im Bewohnerkontext
- Zugriff nur für berechtigte Rollen

**Architekturelle Bedeutung:**
Da diese Informationen im Alltag besonders relevant und sensibel sind, sollten sie prominent, aber zugriffsgeschützt eingebunden werden.

### 6. Such- und Auffindbarkeits-Komponente
Diese Komponente unterstützt den schnellen Zugriff auf relevante Inhalte.

**Aufgaben:**
- Suche nach Bewohnerprofilen
- optional Filter oder Suche innerhalb von Kommunikationsinhalten
- Unterstützung kompakter Ergebnisdarstellung

**Architekturelle Bedeutung:**
Sie ist ein Kernbestandteil des Nutzens, da das System vorhandenes Wissen schneller auffindbar machen soll als klassische Akten.

### 7. Medienverwaltung
Diese Komponente verwaltet Bilder und optional Videos als unterstützende Inhalte.

**Aufgaben:**
- Zuordnung von Medien zu Profilen oder Kommunikationshinweisen
- kontrollierte Bereitstellung je nach Berechtigung
- Berücksichtigung sensibler Freigaben und organisatorischer Regeln

**Architekturelle Bedeutung:**
Medien sollten von der fachlichen Struktur her unterstützt werden, aber Videos nur als optionale Ausbaustufe, da ihre Zulässigkeit und Einbindung noch offen sind.

### 8. Administrationskomponente
Diese Komponente unterstützt die organisatorische Steuerung des Systems.

**Aufgaben:**
- Nutzerverwaltung
- Rollenzuweisung
- Zuordnung zu Einrichtungen oder Zuständigkeitsbereichen
- ggf. Freigabe- oder Pflegeprozesse

### 9. Persistenz- / Datenhaltungskomponente
Diese Komponente speichert die fachlichen Inhalte und Sicherheitszuordnungen.

**Aufgaben:**
- Speicherung von Bewohnerprofilen
- Speicherung von Kommunikationshinweisen und No-Go-Informationen
- Verwaltung von Rollen, Rechten und organisatorischer Zuordnung
- Speicherung von Medienreferenzen und optional Medienmetadaten
- Nachvollziehbarkeit von Pflege- und Aktualisierungsvorgängen, soweit erforderlich

## Schnittstellen und Integrationspunkte

## Interne Schnittstellen
Zwischen den Komponenten ergeben sich mindestens folgende interne Schnittstellen:
- Benutzeroberfläche ↔ Authentifizierung/Autorisierung
- Benutzeroberfläche ↔ Bewohnerprofil-Komponente
- Benutzeroberfläche ↔ Kommunikationswissens-Komponente
- Benutzeroberfläche ↔ Suche
- Kommunikationswissens-Komponente ↔ Medienverwaltung
- alle fachlichen Komponenten ↔ Datenhaltung
- Administrationskomponente ↔ Rollen- und Rechteverwaltung

## Externe oder organisatorische Integrationspunkte
Auf Basis des Kontexts sind Integrationen noch nicht abschließend definiert. Erkennbar sind jedoch mögliche Integrationsfelder:
- **bestehende Dokumentation/Akten**: unklar, ob Inhalte übernommen, referenziert oder manuell parallel gepflegt werden
- **Identitäts- oder Nutzerverwaltung der Einrichtung**: denkbarer Integrationspunkt, fachlich aber nicht bestätigt
- **Medienfreigabe-/Einwilligungsprozesse**: organisatorischer Integrationspunkt für Bild- und Videoeinsatz

Da hierzu keine gesicherten Anforderungen vorliegen, sollte die Architektur diese Integrationen nur als optionale Anschlussstellen vorsehen.

## Datenaspekte

### Zentrale Datenobjekte
Erkennbar sind insbesondere folgende fachliche Datenobjekte:
- **Nutzer**
- **Rolle**
- **Einrichtung / Zuständigkeitsbereich**
- **Bewohnerprofil**
- **Profilbasisinformationen**
- **Kommunikationshinweis**
- **Kommunikationskategorie** (verbal / nonverbal)
- **Beobachtung / Ergänzung**
- **No-Go-Hinweis / Warnhinweis**
- **Medienobjekt** (Bild, optional Video)
- **Berechtigungszuordnung**

### Datenmodellierende Grundidee
Fachlich sinnvoll ist eine Trennung zwischen:
- **stabileren Profildaten** wie Name oder Hobbys,
- **dynamischem Erfahrungswissen** wie Kommunikationsbeobachtungen,
- **kritischen Warninformationen** mit besonderer Sichtbarkeit,
- **Medieninhalten** mit erhöhtem Schutzbedarf.

### Datenqualität
Da Kommunikationswissen interpretativ und veränderlich sein kann, sollte die Architektur Raum für Kontextinformationen lassen, z. B.:
- wann ein Hinweis erfasst wurde,
- von wem er stammt,
- ob es sich um Beobachtung, Erläuterung oder ergänzende Interpretation handelt.

Dies ist keine endgültige Detailmodellierung, aber eine wichtige Leitidee zur Risikoreduktion.

## Sicherheitsaspekte
Sicherheits- und Datenschutzanforderungen prägen die Architektur wesentlich.

### Wesentliche Sicherheitsanforderungen
- Zugriff nur für berechtigte Nutzer
- keine offene Selbstregistrierung
- rollenbasierter Zugriff
- Bereichstrennung nach Einrichtung oder Zuständigkeit
- restriktiver Umgang mit besonders sensiblen Inhalten
- kontrollierter Zugriff auf Bilder und insbesondere Videos

### Architekturelle Konsequenzen
- Berechtigungsprüfung darf nicht nur in der Oberfläche, sondern muss zentral in der fachlichen Zugriffsschicht durchgesetzt werden.
- Datenzugriffe sollten standardmäßig restriktiv sein.
- Medienzugriffe benötigen dieselben oder strengere Freigaberegeln wie Textdaten.
- Optionale Funktionen mit höherem Schutzbedarf, z. B. Medikamente oder Videos, sollten als getrennt freischaltbare Erweiterungen gedacht werden.

## Grobes logisches Architekturmodell
Das System lässt sich in einer frühen Sicht grob als mehrschichtige Fachanwendung beschreiben:

1. **Präsentationsschicht**  
   Mobile Benutzeroberfläche für Suche, Profilansicht, Pflege und Administration.

2. **Fachlogikschicht**  
   Fachmodule für Profile, Kommunikationswissen, No-Go-Hinweise, Suche, Rollen und Administration.

3. **Sicherheits- und Zugriffssteuerungsschicht**  
   Authentifizierung, Rollenprüfung, organisatorische Bereichstrennung.

4. **Datenhaltungsschicht**  
   Persistenz für Profile, Hinweise, Rechte und Medienzuordnungen.

Diese Struktur unterstützt die Trennung zwischen fachlichem Kern, Sicherheitslogik und optionalen Erweiterungen.

## Architektonische Priorisierung für einen frühen MVP
Zur Risikoreduktion und Wahrung des Fokus sollte ein erstes System architektonisch auf wenige Kernfähigkeiten zugeschnitten sein:
- Login und geschlossener Zugriff
- Nutzer-/Rollenmodell mit Einrichtungsbezug
- Bewohnerprofilübersicht mit Suche
- Bewohnerprofil mit persönlicher Kurzübersicht
- Kommunikationsdokumentation mit verbal/nonverbal und Text/Bild
- No-Go-Hinweise
- einfache Pflege neuer Beobachtungen

Bewusst nachrangig bzw. optional:
- Videos
- Angehörigenbeiträge in größerem Umfang
- Bewohner-Accounts
- Kalender/Termine
- Medikamenteninformationen

## Offene Architekturentscheidungen
Mehrere Punkte sind fachlich noch nicht entschieden und müssen als offene Architekturentscheidungen festgehalten werden:

1. **Rollen- und Rechtezuschnitt im Detail**  
   Welche Rolle welche Inhalte sehen, pflegen oder freigeben darf, ist noch nicht final definiert.

2. **Granularität der organisatorischen Bereichstrennung**  
   Offen ist, wie fein Einrichtung oder Zuständigkeit modelliert werden müssen.

3. **Umgang mit bestehender Dokumentation**  
   Unklar ist, ob die App führendes System für bestimmte Informationen wird oder nur ergänzend arbeitet.

4. **Einbindung von Videos**  
   Offen ist sowohl die fachliche Platzierung als auch die datenschutzrechtliche Freigabefähigkeit.

5. **Schutzbedarf und Freigabemodell für sensible Inhalte**  
   Besonders für No-Go-Hinweise, Medien und mögliche spätere Medikamenteninformationen sind Schutzstufen noch auszuarbeiten.

6. **Beteiligung externer Akteure**  
   Angehörige und optionale Bewohner-Accounts erhöhen Nutzenpotenzial, aber auch Governance- und Sicherheitskomplexität.

7. **Abgrenzung des MVP gegenüber späteren Ausbaustufen**  
   Der fachliche Fokus muss verbindlich festgelegt werden, damit die Architektur nicht früh durch optionale Funktionen überlastet wird.

## Risiken mit direkter Architekturwirkung
Folgende Risiken beeinflussen die Architektur unmittelbar:
- **Datenschutz und Einwilligung** bestimmen, welche Datenarten und Medien überhaupt unterstützt werden können.
- **Unklare Rechtevergabe** erfordert eine Architektur, in der Autorisierung zentral und erweiterbar modelliert ist.
- **Alltagstauglichkeit** verlangt eine informationszentrierte statt dokumentationszentrierte Struktur.
- **Interpretationsrisiko** verlangt ein Datenmodell, das Beobachtungen als kontextabhängig behandelt.
- **Überfrachtung** spricht für modularen Zuschnitt und MVP-Fokus.

## Zusammenfassung
Die erkennbare Zielarchitektur ist eine mobile, rollenbasierte Fachanwendung zur strukturierten Bereitstellung von Erfahrungswissen über Bewohner. Im Zentrum stehen Bewohnerprofile, Kommunikationsdokumentation, schnelle Auffindbarkeit und kritische Warnhinweise. 

Architektonisch besonders wichtig sind:
- eine klare Trennung von Fachlogik, Sicherheitslogik und Datenhaltung,
- ein restriktives und organisationsbezogenes Zugriffskonzept,
- ein Datenmodell für dynamisches, kontextabhängiges Kommunikationswissen,
- ein modularer Zuschnitt, damit optionale Funktionen den Kernnutzen nicht verdrängen.

Die Architektur sollte daher bewusst konservativ starten: fokussiert auf den Kommunikationskern, erweiterbar für Medien und weitere Rollen, aber ohne offene Konzeptoptionen vorschnell als festen Kernbestandteil anzunehmen.
