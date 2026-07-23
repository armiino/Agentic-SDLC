# Projektkontext aus Stakeholder-Transkript

## Projektziel
Das Hauptziel des Projekts ist die Entwicklung eines Kundenportals (bzw. einer Kundenplattform) mit Fokus auf schneller Angebotserstellung. Dieses Portal soll folgende Kernfunktionen bieten:
- Login mit E-Mail und Passwort (Möglichkeit für SSO optional)
- Angebote erstellen (Lesender Zugriff auf Produkt- und Preisdaten aus SAP)
- Rechnungen anzeigen und herunterladen
- Rollenbasierte Zugriffssteuerung (Admin, Sales, Kunde mindestens)
- Einhaltung der DSGVO-Anforderungen (z.B. Double-Opt-In, Löschkonzepte, Audit-Trails)
- Hosting EU-only oder mindestens DSGVO-konform
- Backup und Disaster Recovery
- MVP soll in 8 Wochen fertig sein

## Relevante Sprecherrollen
- Anna (vermutlich Produktmanagement oder Projektleitung)
- Ben (vermutlich technischer Verantwortlicher / Entwickler)
- Clara (Datenschutz und Compliance)
- David (Customer Support)
- Eva (Finance)
- Farid (IT Operations)

## Fachliche Themen und Anforderungen
- Kundenportal mit Web-Frontend (Mobile App optional für später)
- SAP-Integration vorwiegend lesend, Preis- und Produktdaten, Rabattlogik
- Rollen- und Berechtigungskonzepte inkl. Admin, Sales, Kunde, Manager, Support (Support begrenzt im MVP)
- Datenschutz (DSGVO): Double-Opt-In, Löschkonzepte, Auditierbarkeit, Datenminimierung
- Security Review erforderlich, muss aber zeitsensitiv sein
- Managed Services und keine neue Datenbank in Eigenbetrieb
- API Layer zwingend für Integrationen, OAuth bevorzugt
- Backup und Disaster Recovery sind Pflicht
- KPIs: Conversion Rate, Angebotszeit
- Unterstützung Mehrwährung (EUR, CHF, USD später), Internationalisierung (DACH, später EU und USA)
- PDF-Export von Angeboten mit Versions- und Audit-Informationen
- Angebotsworkflow mit Status (Draft, Pending Approval, Approved, etc.), Freigabeprozesse (Rabatte ab bestimmten Schwellenwerten)
- Support via Kontaktformular, kein Ticketsystem im MVP (Risiko und bewusste Einschränkung)
- Datenhosting: EU-only mit nachweisbarer Datenresidenz, kein globales Backup
- Test- und Produktionsumgebungen mit Pseudonymisierung oder synthetischen Testdaten
- Monitoring und Logging mit Trennung von technischen Logs und Audit Logs
- Risiko: SAP-Verfügbarkeit kritisch für Angebotserstellung

## Konflikte und Unsicherheiten
- Mobile App technisch und budgetär nicht sofort machbar
- Zeitrahmen 8-Wochen MVP vs. notwendige Security Reviews und API Gateway Warteliste (6 Wochen Warteliste)
- KPI-Tracking und Analytics im MVP unklar
- Rabattfreigabeprozess und Rabattlogik komplex, oft nicht synchron mit SAP
- Supportprozess unklar, keine Persistenz im MVP für Supportanfragen
- EU-only Hosting wahrscheinlich teurer, Kosten sind unklar, Kostenschätzung wird bis Freitag erwartet
- Datenminimierung kontra Performance bei SAP-Datenabruf
- Internationalisierung und Mehrwährung noch offen, Pilotkunde unklar (Schweiz vs. Deutschland)
- Backup und Disaster Recovery ja, aber Aufwand und Umfang noch offen
- Keine klare Architekturentscheidung, Cloud-Anbieter und IAM unklar
- Datenschutz und Aufbewahrungsfristen kollidieren mit Löschanfragen
- Unterscheidung zwischen Audit Logs, Security Logs und Application Logs nötig
- Keine finale Entscheidung zu OAuth oder API Keys
- SAP-Schnittstellen meist lesend, Schreibzugriffe unklar

## Offene Fragen und Risiken
- Pilotkunde (Schweiz oder Deutschland) beeinflusst Datenschutz, Währung und Scope
- Rabattfreigabeprozesse und Staffelungen müssen noch definiert werden
- Support ohne Ticketpersistenz ist ein Risiko, da Support nicht strukturiert arbeiten kann
- API Gateway-Verfügbarkeit kritisches Zeitrisiko
- Datenresidenz und DSGVO Konformität müssen nachweisbar gewährleistet werden
- Sicherheit und Backup ohne zusätzliche DB sollen sichergestellt werden
- Testdaten und CI/CD mit Secret Management müssen organisiert werden
- Handling von Preis-Updates aus SAP (nächtliche Aktualisierung) und Caching-Lösungen unklar
- PDF-Templates und Dokumentationsaufwand müssen minimal bleiben, aber revisionssicher sein
- Abschließende Architektur- und Technologieentscheidungen stehen noch aus

---

Diese Zusammenfassung wurde aus dem Transkript input/transcripts/T9999_chaos.txt erstellt. Es sind Widersprüche, Risiken und offene Punkte bewusst dokumentiert, um ein realistisches Bild des Projektkontexts zu geben und falsche Sicherheit zu vermeiden.