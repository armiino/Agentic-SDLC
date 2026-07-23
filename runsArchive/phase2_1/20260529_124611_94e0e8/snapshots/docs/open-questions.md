# Offene Fragen und Klarungsbedarfe
*(Aus Transkript, Kontext, Anforderungen und Architektur abgeleitet)*

## Fachliche Fragen
1. **Genauer MVP‑Umfang**  
   Welche Features gehören definitiv zu Phase 1 (Login, Angebots‑SAP‑Lesung, Rechnungsdownload, minimale Rollen, minimaler Audit‑Trail, EU‑managed Hosting, Backup) und welche werden in spätere Phasen verschoben (SSO, Mobile, Push, Mehrwährung, PDF‑Freigabe, Support‑Ticketssystem, API‑Gateway, internationales Roll‑out)?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext.*

2. **Authentifizierungsentscheidung**  
   Sollen wir für das MVP E‑Mail/Passwort mit Double‑Opt‑In verwenden oder bereits SSO (Azure AD/Google) anstreben? Welche Aufwände und Abhängigkeiten sind mit der SSO‑Integration verbunden?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; Architektur‑Entscheidungspunkt.*

3. **Freigabeprozesse für Rabatte**  
   Welche konkreten Schwellenwerte gelten für die Rabatt‑Freigabe (15 %, 20 %, 30 %?) und welche Rollen (Manager, Finance, ggf. Senior‑Management) sind jeweils verantwortlich?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; FR‑4 und Constraints C‑8.*

4. **Support‑Prozess**  
   Ist ein reines Kontaktformular ausreichend oder benötigen wir ein leichtes Ticket‑Tracking (z. B. Status, Zuweisung)? Wie wirken sich diese Alternationen auf die Datenverarbeitung (Zuordnung zum Kundenkonto, Löschkonzept) aus?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; FR‑8/9.*

5. **Internationale Pilotkunden & Währung**  
   Falls ein Schweizer Pilotkunde (Müller AG) bestätigt wird, müssen CHF und ggf. weitere Währungen sowie landesspezifische Steuer‑ und Rechtsvorschriften bereits im MVP berücksichtigt werden. Wie wird die Entscheidung über den Pilotkunden getroffen und welcher Zeitpunkt ist für die Integration relevant?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; FR‑15, Constraints C‑13.*

6. **Rechtliche Aufbewahrungsfristen vs. Löschrecht**  
   Wie lange müssen Angebote und Rechnungen aufbewahrt werden (Handelsrecht, steuerrechtliche Vorgaben) und wie verhält sich das zum Recht auf Löschung nach DSGVO? Welche konkreten Fristen legt das Legal‑Team fest?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; NFR‑8.*

## Technische Fragen
7. **API‑Gateway Verfügbarkeit**  
   Das zentrale unternehmensweite API‑Gateway hat eine sechs‑wöchige Warteliste. Können wir innerhalb des 8‑Wochen‑Fensters ein eigenes Gateway bereitstellen, das später ersetzt wird? Wie sieht die Migrations‑ bzw. Fallback‑Strategie aus?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; Architektur‑Entscheidungspunkt.*

8. **Umgang mit SAP‑Verfügbarkeit**  
   Welche Strategie soll bei kurz‑ oder mittel‑fristiger SAP‑Unverfügbarkeit angewendet werden (Read‑Through‑Cache, Fallback auf zuletzt bekannte Preise, Blockierung der Angebotserstellung, Kennzeichnung als „unverbindlich“)? Wie wirkt sich das auf die Datenkonsistenz und das Benutzererlebnis aus?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; Architektur‑Entscheidungspunkt.*

9. **Testdatenstrategie**  
   Wie werden synthetische oder pseudonymisierte Testdaten bereitgestellt, damit Entwicklungs‑ und Testsysteme keine echten Kundendaten enthalten, jedoch realistische Produkt‑, Preis‑ und Rabattszenarien abbilden? Welche Tools bzw. Prozesse werden dafür eingesetzt?  
   *Quelle: Offene Fragen / Unsicherheiten im Kontext; FR‑17.*

10. **Monitoring‑ und Logging‑Stack**  
    Sollen wir einen eigenen ELK‑Stack aufbauen oder managed Cloud‑Logging/Monitoring‑Dienste (z. B. Azure Monitor, AWS CloudWatch) nutzen? Welches Skill‑Set liegt im Team vor und welche Kosten entstehen jeweils?  
    *Quelle: Architektur‑Entscheidungspunkt.*

11. **PDF‑Generierungstechnologie**  
    Welche Bibliothek bzw. Dienstleistung soll für die Erzeugung rechtssicherer PDF‑Angebote und -Rechnungen verwendet werden (wkhtmltopdf, Puppeteer, kommerzielle PDF‑Lib)? Wie soll das Template‑Management und die Versionierung erfolgen?  
    *Quelle: Architektur‑Entscheidungspunkt.*

12. **Backup‑ und Disaster‑Recovery‑Strategie**  
    Welche konkrete RPO/RTO (aktuell angenommen 4 h) ist vertraglich vereinbart und wie häufig werden Wiederherstellungstests durchgeführt? Wie stellen wir sicher, dass Backups nicht außerhalb der EU repliziert werden (falls nicht explizit deaktiviert)?  
    *Quelle: Offene Fragen / Unsicherheiten im Kontext; NFR‑7.*

13. **Rate‑Limiting‑Granularität**  
    Soll das Limit pro Benutzer, pro IP‑Adresse oder eine Kombination gelten? Wie unterscheiden wir zwischen legitimen Massen‑Downloads (z. B. Einkaufsabteilung) und möglichem Missbrauch?  
    *Quelle: Architektur‑Entscheidungspunkt.*

14. **Caching‑Strategie bei SAP‑Ausfall**  
    Welche Daten (rein produktbezogene Preise, ggf. kundenindividuelle Rabatte) dürfen gecacht werden, ohne gegen die Datenminimierung bzw. DSGVO zu verstoßen? Wie erfolgt die Cache‑Invalidierung bei Preisänderungen in SAP?  
    *Quelle: Architektur‑Entscheidungspunkt.*

## Widersprüche, die geklärt werden müssen
15. **MVP‑Umfang vs. 8‑Wochen‑Zeitplan**  
    Viele geforderte Features (SSO, Mobile, Push, Mehrwährung, PDF‑Freigabe, Support‑Tickets, API‑Gateway, EU‑Hosting, Backup) überschreiten das vorgegebene Zeitfenster. Wie entscheiden wir, welche Elemente wir streichen bzw. in spätere Phasen verschieben, ohne das Hauptziel „schnellere Angebotsstellung“ zu gefährden?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

16. **Sicherheit & Compliance vs. Speed**  
    DSGVO‑Anforderungen (Double‑Opt‑In, Löschkonzept, Audit, Datenresidenz) und der geforderte Security Review werden als blockierend wahrgenommen, während das Management ein MVP in 8 Wochen drängt. Wie erreichen wir ein ausreichendes Schutzniveau ohne den Zeitplan zu sprengen?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

17. **Rollen‑ und Berechtigungskonflikt**  
    Support benötigt Einblick in Angebote/Rechnungen, darf jedoch keine Preise oder Sonderkonditionen sehen (Vertraulichkeit). Wie feinkörnig muss das Berechtigungsmodell gestaltet sein, um sowohl Supportfähigkeit als auch Datenvertraulichkeit zu gewährleisten?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

18. **Datenhaltung vs. Datenminimierung**  
    Das Caching von Produkt‑/Preisdaten könnte kundenindividuelle Rabatte enthalten, wodurch persönliche Daten im Cache landen würden. Wie balancieren wir Performance‑Verbesserungen mit der Pflicht zur Datenminimierung?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

19. **Funktionsumfang vs. Benutzererfahrung**  
    Rate‑Limits, Pagination und Download‑Limits sind nötig für Sicherheit/Performance, können jedoch die UX verschlechtern (lange Wartezeiten, abgeblockte Aktionen). Wie finden wir eine akzeptable Balance?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

20. **Finanzrisiko vs. Prozessflexibilität**  
    Rabattfreigabe ab einem bestimmten Schwellenwert ist erforderlich, um finanzielle Fehlanreize zu vermeiden, während Sales maximale Flexibilität bei Rabatten verlangt. Wie definieren wir die Schwellenwerte und die Verantwortlichen, um beiderseitige Anforderungen zu erfüllen?  
    *Quelle: Konflikte / Spannungsfelder im Kontext.*

## Fehlende Informationen
21. **Kosten für EU‑only‑Hosting und Managed Services**  
    Aktuell liegen keine konkreten Preisangebote vor (z. B. verwaltete Datenbank, Object Storage, API‑Gateway). Welche monatlichen bzw. jährlichen Kosten sind zu erwarten, und bleibt das Budget für das MVP?  
    *Quelle: Offene Fragen / Unsicherheiten im Kontext (Hosting‑Provider und Kosten).*

22. **Gesetzliche Aufbewahrungsfristen**  
    Das Legal‑ bzw. Compliance‑Team muss die genauen Aufbewahrungsdauern für Angebote, Rechnungen und zugehörige Audit‑Logs festlegen. Ohne diese Werte kann weder eine Rückhalte‑ noch eine Löschstrategie finalisiert werden.  
    *Quelle: Offene Fragen / Unsicherheiten im Kontext; NFR‑8.*

23. **SAP‑Schnittstellenleistung und Verfügbarkeitsgarantie**  
    Welche konkrete Antwortzeit, Durchsatz und SLA bietet die vorhandene SAP‑OData/REST‑ bzw. RFC‑Schnittstelle? Wie häufig treten geplante Wartungsfenster auf, und wie wirkt sich das auf die Verfügbarkeit des Portals aus?  
    *Quelle: Fachliche Themen (SAP‑Integration) und Offene Fragen (Umgang mit SAP‑Verfügbarkeit).*

24. **Identität‑Provider für spätere SSO‑Phase**  
    Soll Azure AD, Google Identity oder ein anderer Anbieter genutzt werden? Welche Lizenzkosten und Integrationsaufwand sind damit verbunden?  
    *Quelle: Offene Fragen / Unsicherheiten im Kontext; Architektur‑Entscheidungspunkt.*

25. **Konkrete Pilotkundenentscheidung**  
    Welcher Kunde (Müller AG Schweiz, Hansa GmbH Deutschland oder andere) wird als Pilot ausgewählt, und welcher zeitliche Rahmen liegt dafür vor? Diese Entscheidung beeinflusst Anforderungen an Währung, Sprache und landesspezifische Rechtsvorschriften.  
    *Quelle: Fachliche Themen (Internationalisierung) und Offene Fragen (Internationale Pilotkunden & Währung).*

## Mögliche Ansprechpartner oder Rollen (aus Transkript erkennbar)
- **Anna** – Produkt/Projektleitung, MVP‑Umfang, Entscheidungen über Scope und Priorisierung.  
- **Ben** – Technik/Architektur, API‑Layer, SAP‑Adapter, Sicherheitsaspekte, Performance.  
- **Clara** – Datenschutz/Compliance (DSGVO, Löschkonzept, Audit, Datenresidenz).  
- **David** – Customer Support, Anforderungen an Kontaktformular/Ticketing und Unterstützung bei Support‑Prozessen.  
- **Eva** – Finance/Controlling, Rabatt‑Freigabe, Finanzrisiken, Währung, PDF‑Anforderungen.  
- **Farid** – IT Operations/Hosting, EU‑Hosting, API‑Gateway‑Verfügbarkeit, Backup, Secrets‑Management, Monitoring.  
- **Legal / Compliance Team** (nicht namentlich genannt, aber von Clara erwähnt) – Aufbewahrungsfristen, DSGVO‑Auslegung, Vertragsgestaltung für Datenresidenz.  
- **Identity‑Provider‑Team / Security‑Team** (impliziert durch Diskussion über SSO und Security Review) – Evaluation von Azure AD/Google, OIDC/OAuth2‑Implementierung.  
- **SAP‑Basis‑ bzw. Schnittstellen‑Team** – Bereitstellung und Leistung der SAP‑OData/REST‑ bzw. RFC‑Schnittstellen, Wartungsfenster.  

*Alle Punkte sind aus dem Transkript `input/transcripts/T9999_chaos.txt`, dem konsolidierten Kontext (`runs/phase2_1/20260529_94e0e8/state/context.md`), den Anforderungen (`docs/requirements.md`) und der Architektur (`docs/architecture.md`) abgeleitet.* 