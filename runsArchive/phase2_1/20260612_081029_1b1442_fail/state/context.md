# Projektkontext Kundenportal MVP

## Projektziel
Entwicklung eines Kundenportals, primär zur schnelleren Angebotserstellung und Rechnungsanzeige. Ziel ist ein funktionierender MVP in 8 Wochen mit folgenden Kernfunktionen: Login, Angebotserstellung mit SAP-Lesedaten, Rechnungsdownload, minimaler Rollenumfang, Audit-Logging, EU-konformes Managed Hosting sowie Backup.

## Sprecherrollen
- Anna (Projektleitung / Sales - Schwerpunkt Anforderungen & Scope)
- Ben (Technik / Architektur - Schwerpunkt APIs, Frontend, Backend)
- Clara (Datenschutz & Compliance)
- David (Kundensupport)
- Eva (Finance / Controlling)
- Farid (IT Operations / Hosting)

## Fachliche Themen und Anforderungen
- Kundenportal als zentrale Plattform, primär Web-First, Mobile später
- Angebotserstellung inkl. Rabattlogik aus SAP, aber ohne manuelle Sonderrabatte im MVP
- Rechnungsanzeige und Download für Kunden
- Login mit Mail/Passwort, Double-Opt-In, optionale SSO (Azure AD, Google)
- Rollen: Admin, Sales (Vertrieb), Kunde, ggf. Manager und Support (ohne Ticketsystem im MVP)
- Minimaler Audit-Trail und Logging zur Nachvollziehbarkeit (wer hat was wann geändert)
- SAP-Integration lesend, Fokus auf Datenabruf, keine Schreibzugriffe im MVP
- API Layer als zentrale Integrationsschicht, OAuth bevorzugt
- EU-only Hosting mittels Managed Services, keine neue DB Server Installation
- Backup / Disaster Recovery vorgesehen
- KPI-Messung (Conversion Rate, Zeit bis Angebot)

## Konflikte und Risiken
- Zeitdruck für MVP (8 Wochen) im Widerspruch zu notwendigem Security Review und Architekturentscheidungen
- Unklare Supportprozesse, kein Ticketsystem im MVP, Kontaktformular ohne Persistenz
- Offene Fragen zu Pilotkunde, Mehrwährung (EUR, CHF, ggf. USD), Internationalisierung
- Unzureichende oder fehlende Freigabeprozesse für Rabatte (mindestens ab 15% Rabatt in späterer Phase)
- Datenschutzrisiken bei Audit-Logs und Supportdaten (keine personenbezogenen Daten in technischen Logs)
- SAP-Verfügbarkeit als kritische Abhängigkeit, Risiko für korrekte Angebots- und Preisangaben
- API Gateway Warteliste als Blocker für MVP-Rollout
- Einschränkungen in Testumgebungen durch SAP-Testsystem mit echten Daten
- Datenresidenz und DSGVO-Konformität werden pragmatisch umgesetzt, mit kostlichen Kompromissen
- Gefahr von Overengineering bei notwendigen Compliance-, Sicherheits- und Freigabeprozessen

## Unsicherheiten
- Endgültiger Pilotkunde (Schweiz vs. Deutschland) beeinflusst Datenschutz und Währung
- Genaue KPI-Definitionen und Analytics-Infrastruktur noch offen
- Mobile Nutzung und nativer App-Umfang unsicher
- OAuth-Integration und SSO noch nicht final entschieden
- Backup- und Monitoring-Details unklar

## Quellenhinweise
Der Kontext basiert vollständig auf dem Transkript der Stakeholder-Diskussion aus input/transcripts/T9999_chaos.txt, das eine intensive Diskussion zu Scope, Anforderungen, Risiken und organisatorischen Rahmenbedingungen dokumentiert.
