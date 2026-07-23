# Architekturüberblick für Kundenportal MVP

## 1. Systemkontext

Das Kundenportal dient als zentrale Plattform für Kunden, um Angebote zu erstellen, Rechnungen einzusehen und grundlegende Supportanfragen zu stellen. Es integriert SAP-Systeme zur Datenversorgung und erfordert eine sichere Authentifizierung (Login mit E-Mail/Passwort, optional SSO). Ziel ist ein MVP mit beschränktem Scope für den Start im DACH-Raum.

## 2. Wichtige Komponenten

- **Frontend:** Webbasiertes Kundenportal; Mobile App ist Folgephase.
- **Backend & API Layer:** Schnittstelle zu SAP, Authentifizierung, Business-Logik, Processing von Angeboten und Rechnungen.
- **Identity Management:** Login, Double-Opt-In, optionale SSO Integration (Azure AD, Google).
- **Rollenverwaltung & Berechtigungen:** Admin, Sales, Kunde, Support (mit beschränkter Funktion im MVP).
- **Audit- und Logging-Komponenten:** Minimaler Audit Trail zur Nachvollziehbarkeit mit Schutz personenbezogener Daten.
- **Hosting & Infrastruktur:** EU-konformes Managed Hosting ohne neue lokale DB-Server; Backup und Disaster Recovery.
- **Support-Komponente:** Kontaktformular ohne Ticketsystem im MVP.

## 3. Schnittstellen / Integrationspunkte

- **SAP:** Lesende Integration zur Datenversorgung von Produktdaten, Preisen und Rabatten.
- **Identity Provider:** OAuth-basierte Schnittstelle für SSO (optional).
- **API Gateway:** Vorgeschalteter API-Zugangspunkt, derzeit Warteliste - alternative Übergangslösung nötig.
- **Support-Kontaktformular:** Externe Infrastruktur für Kundenanfragen (ohne Persistenz im MVP).

## 4. Daten- und Sicherheitsaspekte

- Strikte Einhaltung der DSGVO, inklusive Double-Opt-In, Löschkonzepte, Datenschutz bei Audit-Logs.
- Keine Speicherung personenbezogener Daten in technischen Logs.
- Datenresidenz in der EU.
- Freigabeprozesse für Rabatte künftig vorgesehen (ab 15 % Rabatt).
- Security Review geplant, Zeitplan kritisch.

## 5. Offene Architekturentscheidungen

- Finaler Pilotkunde (Deutschland vs. Schweiz) und damit verbundene Datenschutz- und Währungsfragen.
- Umfang und Technik von SSO/OAuth Integration.
- Umgang mit SAP-Datenverfügbarkeit und Cache-Strategie.
- Support-Ticketsystem oder reines Kontaktformular.
- API Gateway Nutzung im MVP oder alternative Übergangslösung.
- Backup- und Monitoring-Lösung.
- Internationalisierung und Mehrwährungsfähigkeit (Folgephasen).

---

Dieser Architekturüberblick basiert auf dem Stakeholder-Transkript, dem strukturierten Projektkontext sowie den abgeleiteten Requirements und Risikobetrachtungen. Er dient als Grundlage für vertiefende architekturrelevante Entscheidungen im weiteren SDLC.