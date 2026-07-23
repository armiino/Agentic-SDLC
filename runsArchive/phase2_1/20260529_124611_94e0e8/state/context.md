# Projektkontext (aus Transkript T9999_chaos.txt)

## Projektziel
- Schnellere Angebotserstellung für Kunden über ein Web‑Portal (MVP).  
- Zusätzlich sollen Kunden Bestellungen sehen und Rechnungen downloaden können.  
- Das Portal soll später ggf. um Mobile‑Support, SSO, Push‑Benachrichtigungen und mehrsprachige Angebote erweitert werden.  
- Kernmetrik: Conversion Rate (Angebot → Bestellung) und Zeit bis Angebot.

## Sprecherrollen (aus Transkript)
- **Anna** – Produkt/Projektleitung, treibt das Portal und MVP voran.  
- **Ben** – Technik/Architektur, fokussiert auf API, Integration, Sicherheit, Performance.  
- **Clara** – Datenschutz/Compliance (DSGVO, Audit, Löschkonzepte).  
- **David** – Customer Support, betont Support‑ und Ticket‑Bedarf.  
- **Eva** – Finance/Controlling, betont Rabatt‑Freigabe, Finanzrisiken, Währung, PDF‑Anforderungen.  
- **Farid** – IT Operations/Hosting, Themen: EU‑Hosting, API‑Gateway, Monitoring, Secrets‑Management, Backup.  

## Fachliche Themen (aus Transkript)
- **Portal vs. Plattform** (Web‑first, Mobile später).  
- **Login & Authentifizierung** (E‑Mail/Passwort, SSO, Azure AD/Google, Double‑Opt‑In).  
- **Rollen & Berechtigungen** (Admin, Manager, Sales, Support, Kunde).  
- **SAP‑Integration** (Lesen von Produkt‑/Preisdaten, später möglicher Schreibzugriff, API‑Layer, Verfügbarkeit, Wartungsfenster).  
- **Angebotsworkflow** (Entwurf, Freigabe abhängig von Rabattsätzen, Status‑Tracking, PDF‑Export, Versions‑/Revisionssicherheit).  
- **Rechnungsdownload** (PDF mit rechtlichen Hinweisen, Mehrwährung, Steuerrecht).  
- **Datenschutz & Compliance** (DSGVO, Löschkonzept, Audit‑Trail, Datenminimierung, Hosting nur EU, Datenresidenz, kein personenbezogener Daten in Logs).  
- **Sicherheit** (TLS, ggf. E2E, OAuth vs. API‑Keys, Security Review, Geheimnis‑Management, Rate‑Limiting, Missbrauchserkennung).  
- **Performance & Skalierbarkeit** (Erwartete Nutzerzahlen 200‑20.000, Pagination, Download‑Limits, Caching, Fallback bei SAP‑Ausfall).  
- **Support & Tickets** (Kein vollständiges Ticketsystem im MVP, Kontaktformular, Zuordnung zu Kundenkonto, E‑Mail‑Fallback).  
- **Monitoring & Logging** (Unterschied zwischen Application‑, Audit‑ und Security‑Logs, Aufbewahrungsfristen).  
- **Backup & Disaster Recovery** (Notwendig bei Kundendaten).  
- **Test‑ und Datenstrategie** (Keine echten Kundendaten in Testsystemen, Pseudonymisierung, synthetische Daten).  
- **Internationalisierung** (Sprachen DE/EN, Währungen EUR/CHF/USD, länderspezifische Rechts‑ und Steuerregeln).  
- **KPIs & Reporting** (Conversion Rate, Zeit bis Angebot, später ggf. Analytics).  
- **Dokumentation** (Notwendig für Security Review, aber nicht over‑engineered).  

## Konflikte / Spannungsfelder (aus Transkript)
- **MVP‑Umfang vs. 8‑Wochen‑Zeitplan** – Viele Features (SSO, Mobile, Push, Mehrwährung, PDF‑Freigabe, Support‑Tickets, API‑Gateway, EU‑Hosting, Backup) überschreiten das vorgegebene Zeitfenster.  
- **Sicherheit & Compliance vs. Speed** – DSGVO‑Anforderungen (Double‑Opt‑In, Löschkonzept, Audit, Datenresidenz) und Security Review werden als blockierend gesehen, während das Management ein MVP in 8 Wochen drängt.  
- **Technische Architektur vs. Organizational Constraints** – API‑Gateway hat sechs‑wöchige Warteliste; Managed Services bevorzugt, aber unklar welche; kein vorhandenes IAM für SSO.  
- **Rollen‑ und Berechtigungskonflikt** – Support benötigt Einblick in Angebote/Rechnungen, darf jedoch keine Preise oder Sonderkonditionen sehen (Vertraulichkeit).  
- **Datenhaltung vs. Datenminimierung** – Caching von Produkt‑/Preisdaten könnte kundenindividuelle Rabatte enthalten; Trade‑off zwischen Performance und Datenschutz.  
- **Funktionsumfang vs. Benutzererfahrung** – Rate‑Limits, Pagination und Download‑Limits nötig für Sicherheit/Performance, könnten jedoch UX verschlechtern.  
- **Finanzriskon vs. Prozessflexibilität** – Rabattfreigabe ab 15 % (oder 20/30 %) erforderlich, aber Sales wünscht Flexibilität; Gefahr falscher Angebote ohne Workflow.  
- **Hosting & Datenresidenz** – EU‑only Hosting erhöht Kosten; globale Replikation von Backups verletzt DSGVO, wenn nicht explizit deaktiviert.  
- **Testdaten und SAP** – SAP‑Testsystem enthält echte alte Kundendaten; Nutzung ohne Pseudonymisierung birgt Compliance‑Risiko.  

## Offene Fragen / Unsicherheiten (aus Transkript)
- **Genauer Umfang des MVP** – Welche Features definitiv in Phase 1 (Login, Angebots‑SAP‑Lesung, Rechnungsdownload, minimale Rollen, minimale Audit‑Trail, EU‑Managed‑Hosting, Backup) und welche in spätere Phasen (SSO, Mobile, Push, Mehrwährung, PDF‑Freigabe, Support‑Ticketssystem, API‑Gateway, internationales Roll‑out).  
- **Entscheidung über Authentifizierung** – SSO (Azure AD/Google) vs. einfache E‑Mail/Passwort mit Double‑Opt‑In; noch keine Wahl getroffen.  
- **Freigabeprozesse für Rabatte** – Schwellenwert (15 %, 20 %, 30 %) und welche Rollen (Manager, Finance) involviert sind.  
- **Support‑Prozess** – Ob reines Kontaktformular ausreicht oder ein leichtes Ticket‑Tracking nötig ist; implications for data handling.  
- **Hosting‑Provider und Kosten** – Keine konkrete Wahl; EU‑only vs. globale Replikation noch offen.  
- **API‑Gateway Verfügbarkeit** – Sechs‑wöchige Wartelite; Möglichkeit für Zwischenlösung?  
- **Umgang mit SAP‑Verfügbarkeit** – Fallback, Cache, unverbindliche Angebote bei Ausfall noch nicht festgelegt.  
- **Internationalisierung & Währung** – Ob CHF und USD im MVP benötigt werden, insbesondere falls Schweizer Pilotkunde.  
- **Rechtliche Aufbewahrungsfristen** – Wie lange Angebote/Rechnungen aufbewahrt werden müssen im Verhältnis zum Löschrecht.  
- **Testdatenstrategie** – Wie synthetische oder pseudonymisierte Testdaten bereitgestellt werden, damit SAP‑Tests möglich sind, ohne echte Kundendaten zu exponieren.  

## Quellen (Auszug aus Transkript)
- Projektziel & MVP‑Überlegungen: Anna, Ben, Clara, Eva – z.B. *„Schneller Angebote erstellen … Portal ist nur Mittel zum Zweck“* (Anna).  
- Sprecherrollen: Erkennbar durch Namensprefixe im Transkript.  
- Themen & Konflikte: Aus den einzelnen Redebeiträgen (z.B. Clara zu DSGVO & Löschkonzept, Ben zu API‑Layer & Security Review, David zu Support‑Tickets, Eva zu Rabatt‑Freigabe & PDF‑Anforderungen, Farid zu EU‑Hosting & Backup).  
- Unsicherheiten: Explizit genannt als offene Fragen, z.B. Anna *„Wir haben jetzt sehr viele offene Punkte“* und nachfolgende Auflistung von Ben, Clara, Eva, Farid, David.  

*Dieses Dokument fasst denkonkreten, aus dem Transkript ableitbaren Projektkontext neutral zusammen und bewahrt widersprüchliche Aussagen sowie offene Punkte, wie vom Agentenvertrag gefordert.* 