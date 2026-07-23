# Grober Architekturüberblick für das Kundenportal MVP

## 1. Systemkontext

Das Kundenportal ist eine webbasierte Plattform, die primär die schnellen Angebotserstellungen und Rechnungsanzeige für Kunden unterstützt. Es integriert sich lesend in ein bestehendes SAP-System, um Produktdaten, Preise und Rabatte bereitzustellen. Das Kundenportal soll DSGVO-konform in der EU gehostet werden und Managed Services ohne eigene Datenbankserver nutzen. 

Stakeholder sind Sales (Angebotserstellung), Kunden (Nutzung des Portals), Finance (Freigaberegeln in späteren Phasen), Support (Kontaktformular ohne Persistenz), Datenschutz (DSGVO-Anforderungen) und IT Operations (Hosting, Backup, Monitoring).

## 2. Wichtige Komponenten

- **Frontend:** Web-Portal, initial responsive Web-App mit Login (E-Mail/Passwort + Double-Opt-In). Mobile Native App nicht Teil des MVP.
- **Backend / Business Logic:** API Layer zur Integration mit SAP (lesender Zugriff), Authentifizierung und Rollenmanagement (Admin, Sales, Kunde), Angebotserstellung mit einfachem Statusmodell.
- **Datenhaltung:** Keine neuen Datenbanken, Nutzung von Managed Services (z.B. Cloud-Storage für Dokumente, Auditing, Backup).
- **Sicherheits- und Datenschutz-Komponenten:** Double-Opt-In Mechanismen, Audit Logging, Löschkonzepte und Zugriffskontrolle.
- **Integrationsschnittstellen:** SAP-Schnittstellen (lesend), optional zentraler API Gateway (wenn verfügbar), E-Mail-Kontaktformular für Support.
- **Infrastruktur:** EU-Only Hosting, Backup und Disaster Recovery, Secrets Management, Monitoring (Trennung zwischen Audit- und technischen Logs).

## 3. Schnittstellen und Integrationspunkte

- **SAP:** Lesende Schnittstelle für Produktdaten, Preise, Rabattlogik.
- **API Layer:** Einheitliche REST-API für interne und externe Integration; Sicherheitsmechanismus vorerst OAuth bevorzugt, aber noch offen.
- **API Gateway:** Zentrales Gateway wird langfristig genutzt, aber im MVP wegen Warteliste möglicherweise Umgehungslösung.
- **Support-System:** Einfaches Kontaktformular per E-Mail, kein Ticketsystem im MVP.

## 4. Daten- und Sicherheitsaspekte

- **DSGVO-Konformität:** Speicherung und Verarbeitung personenbezogener Daten nur mit Einwilligung (Double-Opt-In), Löschkonzepte und gesetzliche Aufbewahrungspflichten müssen beachtet werden.
- **Audit Logging:** Nachvollziehbarkeit wer wann welche Daten oder Angebote geändert hat, getrennt von technischen Logs.
- **Verschlüsselung:** TLS für Datenübertragung, keine Ende-zu-Ende Verschlüsselung im MVP.
- **Zugriffskontrolle:** Rollen- und Berechtigungskonzepte, mit minimalen Rollen (Admin, Sales, Kunde) im MVP.
- **Hosting:** EU-only mit Nachweis der Datenresidenz, Vermeidung von Datenexporten außerhalb EU.
- **Backup & Disaster Recovery:** Nutzung von Managed Services, IT-Policy konform.
- **Secrets Management:** Verwaltung von API-Schlüsseln, OAuth Tokens, Passwörtern.

## 5. Offene Architekturentscheidungen

- **Single Sign-On (SSO):** Ob und wie SSO (z.B. Azure AD, Google) integriert wird, ist noch offen und nicht Teil des MVP.
- **API-Sicherheit:** OAuth wird bevorzugt, finale Entscheidung steht noch aus.
- **API Gateway:** Zentrales Gateway ist vorgesehen, aber zeitliche Verfügbarkeit unklar, Zwischenlösungen werden geprüft.
- **Support-System:** Kein Ticketsystem im MVP, Risiko und Komplexität der Supportprozesse weiterhin offen.
- **Rabattfreigabe & Workflow:** Komplexe Freigabeprozesse für Rabatte und Angebotsstatus sind ausgelagert und nicht Teil des MVP.
- **Pilotkunde & Mehrwährung:** Entscheidung über Pilotkundenland beeinflusst Datenschutz, Währung und Scope, noch nicht final geklärt.
- **Skalierbarkeit:** Ziel sind 200 bis 20.000 Nutzer, Architektur muss skalierbar sein, wie genau bleibt offen.
- **Cache & Fallback SAP:** Umgang mit temporärer Nichtverfügbarkeit von SAP-Daten ist noch offen (Cache vs. unverbindliche Angebote).
- **Monitoring & Logging:** Detailtiefe der Audit- und Anwendungslogs und deren Datenschutz sind noch im Diskussionsprozess.

## 6. Zusammenfassung

Der Architekturentwurf orientiert sich strikt an den inhaltlichen Zielen eines MVP mit Fokus auf schnelle Angebotserstellung, einfache Rechnungsansicht und DSGVO-konforme Datenverarbeitung. Viele technische und organisatorische Herausforderungen werden bewusst für spätere Phasen verschoben, um den engen Zeitrahmen einzuhalten. Risiken und offene Fragen bleiben klar benannt, um in späteren Schritten adressiert zu werden.

---

*Erstellungsdatum: 2026-06-01*
