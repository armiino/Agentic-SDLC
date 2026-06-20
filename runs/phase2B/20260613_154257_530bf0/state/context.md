# Projektkontext aus Stakeholder-Transkript

## Projektziel
Das Hauptziel ist die schnelle Erstellung von Angeboten über ein Kundenportal oder eine Plattform, ergänzt durch Funktionen wie Rechnungsdownload, Kundenlogin, Rollenmanagement und SAP-Integration. Das Portal soll als MVP in ca. 8 Wochen starten. Weitere Funktionen wie Support-Ticketsystem, Freigabeprozesse für Rabattangebote und Mehrwährungsfähigkeit sind geplant, aber zunächst ausgeschlossen oder auf spätere Projektphasen verschoben.

## Sprecherrollen
- Anna (vermutlich Produktmanagement/Projektleitung)
- Ben (Technik/Architektur)
- Clara (Datenschutz/Compliance)
- David (Customer Support)
- Eva (Finanzen)
- Farid (IT-Operations)

## Fachliche Themen
- Kundenportal als Web-Frontend, Mobile später
- Anmeldung: Login per E-Mail/Passwort, SSO optional
- Rollen: Admin, Sales, Manager, Support (eingeschränkt)
- SAP-Integration: Lesender Zugriff, keine Schreiboperationen im MVP
- Angebotserstellung: Rabattlogik, Freigabeprozesse (erst später)
- Rechnungsdownload
- Audit- und Logging-Anforderungen (minimal im MVP)
- Hosting: EU-only, DSGVO-konform, Managed Services
- Backup und Disaster Recovery
- API-Layer und API-Security (OAuth bevorzugt)
- KPIs: Conversion Rate, Zeit bis Angebot

## Konflikte und Unsicherheiten
- Performance und Skalierbarkeit bei unklarem Nutzerumfang
- MVP-Zeitrahmen vs. notwendige Security Reviews und Compliance
- API Gateway-Verfügbarkeit verzögert Integration
- Supportprozesse: kein Ticketsystem im MVP, nur Kontaktformular
- Datenschutz vs. Funktionalität: Double-Opt-In, Löschkonzepte, Auditierung
- Kosten für EU-Datenhosting und Managed Services unklar
- Rabattfreigabeprozess offen und komplex
- Preisaktualität aus SAP zeitverzögert, Risiko falscher Angebote
- Mehrwährung und internationale Kunden als Zukunftsthema
- Testdaten und Entwicklungsumgebung inkl. Datenschutz
- Logging und Monitoring: Trennung technischer Logs und Audit Logs
- Offene Fragen zu Pilotkunde und deren Standort

## Quellenhinweise
Die Informationen basieren vollständig auf dem Stakeholder-Transkript input/transcripts/T9999_chaos.txt, welches ein umfangreiches Gespräch zwischen verschiedenen Fach- und IT-Abteilungen dokumentiert.

## Anmerkungen
Viele Anforderungen und technische Details sind noch nicht final entschieden. Offene Punkte und Risiken sind explizit gekennzeichnet, um Transparenz und Nachvollziehbarkeit sicherzustellen. Ziel ist eine realistische, belastbare Kontextbasis für weitere Projektphasen.
