# Projektkontext

## Projektziel
- **Hauptziel**: Schnellere Erstellung von Kundenangeboten über ein digitales Portal (MVP innerhalb von 8 Wochen).
- Unterstützende Ziele: Kunden können Bestellungen einsehen, Rechnungen herunterladen, ggf. mobile Nutzung, KPI‑Messung (Conversion Rate, Zeit bis Angebot).

## Stakeholder & Rollen
- **Anna (Produkt/Business)** – Fokus auf Kundenportal, Angebots‑Workflow, Rollen (Admin, User, Manager, Support), KPI‑Definition, Zeitplan.
- **Ben (Technik/Architektur)** – Concern über Backend‑Readiness, API‑Layer, Integration zu SAP, Sicherheit (OAuth, TLS), Skalierbarkeit, Infrastruktur (keine neue DB, Managed Services, Disaster Recovery).
- **Clara (Compliance/Datenschutz)** – DSGVO‑Anforderungen (Double‑Opt‑In, Logging, Audit‑Trail, Lösch‑/Richtlinien, EU‑Host), Rollen‑ und Berechtigungskonzept, Datenschutz bei Push‑Notifications.

## Fachliche Themen
- **Kundenportal / Plattform** – Web‑first, Mobile später (Responsive oder Native). 
- **Authentication** – Login per E‑Mail/Passwort, optional SSO (Azure AD, Google). 
- **API‑Layer** – Muss für SAP‑Integration und zukünftige Services bereitgestellt werden (OAuth bevorzugt, ggf. API‑Keys). 
- **SAP Integration** – Stammdaten, Produkt‑/Preis‑ und Rabattlogik; Bedarf an Audit‑Trails und Zugriffskontrollen. 
- **DSGVO / Compliance** – Double‑Opt‑In, Consent für Push‑Notifications, Logging, Rollen‑/Berechtigungskonzept, Daten‑Lösch‑ und Auftragsverarbeitungsverträge, EU‑only Hosting. 
- **Security** – TLS für Transport, Logging, Auditability, Security Review (ca. 6 Wochen) – Konflikt mit 8‑Wochen‑MVP. 
- **Infrastructure** – Keine neue DB, Nutzung von Managed Services, Backup/Disaster Recovery nötig, Skalierbarkeit (200‑20 000 Nutzer). 
- **KPI‑Messung** – Conversion Rate, Zeit bis Angebot; Eventuell Analytics später. 

## Konflikte & Unsicherheiten
- **Zeit vs. Sicherheit**: 8‑Wochen‑MVP vs. notwendiger Security Review und DSGVO‑Compliance.
- **Frontend‑Strategie**: Web‑first vs. Mobile‑first vs. Responsive vs. Native.
- **Budget & Infrastruktur**: Keine neue DB, Managed Services sollen günstig sein, aber Skalierbarkeit vs. Kosten.
- **Scope**: Kernfunktionalität (Angebote, Rechnungen, Login) vs. optionale Features (Push‑Notifications, Analytics, Support‑Ticketsystem).
- **Rollen‑ und Berechtigungskonzept**: Wer definiert und implementiert es? – Fehlender Architekt.
- **Performance‑Erwartungen**: Unklare Nutzerzahl (200‑20 000).

## Quellenhinweise
- Transkript `input/transcripts/T9999_chaos.txt` (Gespräch zwischen Anna, Ben, Clara, Datum nicht angegeben). 

## Offene Punkte (zur Klärung in Phase 3)
1. Priorisierung von Security‑Review/Compliance‑Maßnahmen gegenüber MVP‑Zeitplan.
2. Entscheidung über Frontend‑Strategie (Responsive Web vs. Native App).
3. Klare Definition des Rollen‑ und Berechtigungskonzepts inkl. Verantwortlicher.
4. Auswahl des Identity Providers (SSO) und Auth‑Mechanismus.
5. Umfang des API‑Layers und konkrete Schnittstellen zu SAP.
6. Speicherort (EU‑only) und Kosten‑Betrachtung von Managed Services.
7. Definition konkreter KPI‑Messmethoden und Analyse‑Tooling.

---
*Erstellt von Phase 2.1 ContextAgent, basierend auf den verfügbaren Stakeholder‑Transkripten.*