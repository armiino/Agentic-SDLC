# Anforderungen

## Funktionale Anforderungen
- Kundenportal (Web-first, mobile später)
- Login per E-Mail und Passwort (MVP)
- Optional: Single Sign-On (SSO) mit Azure AD und/oder Google
- Rollenmodell: Admin, normaler User, Manager, Support (Support evtl. später)
- Anzeige von Bestellungen und Rechnungen für Kunden
- Angebotserstellung durch Sales (Hauptziel)
- Push Notifications (optional, später)
- Integration mit SAP für Stammdaten, Produktdaten, Preise, Rabattlogik
- API Layer zur Integration und Kommunikation
- Löschkonzept für Kundendaten (DSGVO)

## Nicht-funktionale Anforderungen
- DSGVO-Konformität (Datenhosting in EU oder DSGVO-konform)
- Logging und Audit Trails (wer hat wann was geändert)
- Sicherheit: TLS-Verschlüsselung, OAuth für API-Sicherheit
- Backup und Disaster Recovery
- Skalierbarkeit für 200 bis 20.000 Nutzer
- Performance angemessen für erwartete Nutzerzahlen
- Managed Services für Datenbank (kein neuer DB-Server)
- Dokumentation ausreichend für Security Review

## Einschränkungen/Compliance
- MVP in 8 Wochen
- Kein neuer Datenbankserver, nur Managed Services
- Security Review verpflichtend, aber zeitlich herausfordernd
- DSGVO-Anforderungen: Double-Opt-In, Einwilligungen, Löschkonzept, Auditierbarkeit
- Hosting in der EU oder DSGVO-konform

## Rückverfolgbarkeit
- Kundenportal, Login, Rollen, Angebote, Rechnungen: Anna (Stakeholder)
- DSGVO, Audit Trails, Löschkonzept: Clara (Compliance)
- API Layer, Managed Services, Sicherheit, Performance: Ben (Technik)
- MVP-Zeitplan und Priorisierung: Anna, Ben, Clara (Diskussion im Transcript T9999_chaos.txt)
