# Projektkontext Kundenportal MVP

## Projektziel
- Entwicklung eines Kundenportals als MVP innerhalb von 8 Wochen
- Hauptzweck: Schnelleres Erstellen von Angeboten durch Sales
- Zusätzliche Funktionen: Anzeige von Rechnungen, Login für Kunden
- Integration mit SAP für Produktdaten, Preise und Rabatte (lesend)
- Minimaler Audit Trail und Rollenmodell (Admin, Sales, Kunde)
- Hosting: EU-Only, DSGVO-konform
- Einsatz von Managed Services, kein neuer Datenbankserver

## Stakeholder und Rollen
- Anna (Sales / Produktverantwortung)
- Ben (Technik / Architektur)
- Clara (Datenschutz / Compliance)
- David (Customer Support)
- Eva (Finance)
- Farid (IT Operations)

## Fachliche Themen und Anforderungen
### Funktionalität
- Login mit E-Mail/Passwort, Double-Opt-In Pflicht für DSGVO
- Optionales SSO (z.B. Azure AD, Google) noch nicht final entschieden
- Erstellung und Verwaltung von Angeboten mit Status (draft, pending approval etc.) in späteren Phasen
- Anzeige und Download von Rechnungen im Portal
- Integration von SAP-Schnittstellen zur Datenbereitstellung
- Rollen & Berechtigungen: Admin, Sales, Kunde, evtl. Manager und Support in späteren Phasen
- Kein Ticketsystem im MVP, Kontaktformular als Support-Alternative mit Datenschutzrisiken

### Datenschutz und Compliance
- DSGVO-konformität ist zwingend
- Datenschutzpflichten: Double-Opt-In, Löschkonzepte, Auditierbarkeit (wer hat was wann geändert)
- Speicherung und Zugriff auf personenbezogene Daten strikt regeln
- EU-Only Hosting mit Nachweis der Datenresidenz
- Umgang mit Löschanfragen vs. gesetzliche Aufbewahrungspflichten
- Pseudonymisierung oder synthetische Testdaten für Entwicklung und Test

### Technische Anforderungen
- API-Layer als Voraussetzung für Integrationen, bevorzugt OAuth-Sicherung
- Vermeidung von Overengineering, Fokus auf MVP-Implementierung
- Nutzung zentraler API Gateway, das derzeit 6 Wochen Warteliste hat
- Backup und Disaster Recovery notwendig auch im MVP
- Performance und Skalierbarkeit für unklare Nutzerzahlen (200 bis 20.000) berücksichtigen
- Keine neuen Datenbankserver, Einsatz von Managed Services
- Monitoring mit Trennung von Audit-Logs und technischen Logs (keine personenbezogenen Daten in technische Logs)
- Secrets Management und Rate Limiting für APIs

### Offene Fragen und Risiken
- Unklare endgültige Pilotkunden (Schweiz vs. Deutschland) und daraus folgende Mehrwährungs- und Datenschutzfragen
- Rabattfreigabeprozesse und deren Integration fehlen im MVP, kein Erstellen von Rabatten über Standardsatz
- Supportprozess unklar, kein Ticketsystem im MVP (Risiko der manuellen Bearbeitung per E-Mail)
- SAP-Verfügbarkeit als kritische Abhängigkeit für Angebotserstellung
- Mangel an Architekturressourcen (kein Architekt vorhanden)
- Zeitliche Restriktionen und Abhängigkeiten zu Security Review und API-Gateway-Warteliste
- Kompromisse bei Features für MVP notwendig (z.B. kein Online-Akzeptieren von Angeboten, kein erweitertes Tracking, keine Push Notifications)
- Data Retention und Audit-Log Anforderungen im Spannungsfeld Löschung vs. gesetzliche Vorgaben
- Technische und organisatorische Komplexitäten im Bereich Rollen- und Berechtigungskonzepte
- Risiko von Overengineering vs. notwendiger Skalierbarkeit

## Quellenhinweise
- Transkript "input/transcripts/T9999_chaos.txt" (Stakeholder-Meeting mit Diskussion zu Scope, Technik, Datenschutz und organisatorischen Herausforderungen)

---

_Erstellungsdatum: 2026-06-01_
