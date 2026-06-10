# Grober Architekturüberblick für das Kundenportal MVP

## 1. Systemkontext

Das Kundenportal dient als Plattform für Kunden, Sales und Admins zur schnellen Angebotserstellung und Einsicht in Rechnungen. Es integriert SAP-Daten lesend und bietet Login-Funktionalität mit Rollensteuerung. Das System ist als Webportal konzipiert (Web-first, mobile Nutzung später möglich).

### Externe Systeme / Integrationen
- **SAP ERP System:** Bereitstellung von Kundenstammdaten, Produktdaten, Preisen und Rabattlogik (nur lesender Zugriff im MVP).
- **Identity Provider (optional in Folgephasen):** SSO via Azure AD oder Google, aktuell Login mit E-Mail/Passwort.
- **API Gateway (zentrales Gateway der IT):** Vorgesehen zur API-Absicherung, Verfügbarkeit im MVP unsicher.
- **E-Mail-System:** Für Double-Opt-In und Kontaktformular zur Support-Kommunikation.

## 2. Wichtige Komponenten

### 2.1 Frontend
- Web-basiertes Kundenportal mit responsiver Oberfläche.
- Rollenbasierte Zugriffsteuerung für Admin, Sales und Kunden.

### 2.2 Backend
- **Authentifizierungsmodul:** Nutzerverwaltung, Login, Rollen und Double-Opt-In-Mechanismen.
- **API Layer:** Schnittstelle für Frontend und Integrationen (SAP, ggf. Identity Provider).
- **Angebotserstellungskomponente:** Logik für Erstellung und Anzeige von Angeboten basierend auf SAP-Daten.
- **Rechnungsmodul:** Anzeige und PDF-Download von Rechnungen.
- **Audit- und Loggingmodul:** Minimaler Audit Trail zur Nachvollziehbarkeit, Trennung technischer und personenbezogener Logs.
- **Backup- und Recovery-Komponente:** Managed Service zur Datensicherung und Wiederherstellung.

### 2.3 Infrastruktur
- EU-only Hosting in einer DSGVO-konformen Cloud-Umgebung.
- Managed Services für Datenbank und Speicher (kein eigener Datenbankserver).
- Umgebungen für Entwicklung, Test (mit pseudonymisierten oder synthetischen Daten) und Produktion.
- Secrets Management für sichere Verwaltung von Zugangsdaten und API-Schlüsseln.

## 3. Schnittstellen und Integrationspunkte

- **SAP REST-/SOAP-API:** Lesender Zugriff auf Kundendaten, Produkte, Preise und Rabattinformationen.
- **API Gateway:** (Optional, ggf. in Folgephasen) Sicherung der APIs mittels OAuth bzw. API Keys.
- **E-Mail-Service:** Versand von Double-Opt-In E-Mails und Support-Kontaktanfragen.
- **Optional Identity Provider APIs:** Für SSO-Funktionalitäten.

## 4. Daten- und Sicherheitsaspekte

- DSGVO-konforme Speicherung und Verarbeitung von personenbezogenen Daten.
- Double-Opt-In für Registrierung.
- Löschkonzepte für Kundendaten.
- Keine Speicherung personenbezogener Daten in technischen Logs.
- Audit-Trails für Zugriffe und Änderungen an Angeboten und Rechnungen.
- EU-only Datenresidenz mit nachweisbarer Datenlokation.
- Backup und Disaster Recovery, vorwiegend durch Managed Services garantiert.
- API Security bevorzugt mittels OAuth, aber erste MVP-Lösung möglicherweise einfacher.
- Secrets Management und API Rate Limiting zur Sicherung und Stabilität.

## 5. Offene Architekturentscheidungen

- Wahl des Identity Providers und Art der Integration (SSO optional und noch nicht final).
- Verfügbarkeit des zentralen API Gateways und COVID19-Maßnahmen bei Wartelisten.
- Detaillierung des Freigabeprozesses für Rabatte (außerhalb MVP).
- Unterstützung von Mehrwährungen (CHF, USD) und Mehrsprachigkeit (Deutsch, Englisch) im Folgeprojekt.
- Umsetzung von Supportprozessen und Ticketsystemen außerhalb des MVP.
- Umgang mit SAP Preisdaten, insbesondere Fallback bei Nichtverfügbarkeit und Cache-Strategien.
- Konkrete Technologien für Hosting und Managed Services noch offen (keine proprietäre Produktentscheidung vor Providerwahl).
- Klärung und Implementierung von Aufbewahrungsfristen und Retention Policies.
- Umgang mit realen Kundendaten in Testumgebungen und Datenschutzkonforme Testdatenstrategie.

---

*Dieser grobe Architekturüberblick basiert auf dem bestehenden Anforderungs- und Kontextwissen aus dem Stakeholder-Transkript (input/transcripts/T9999_chaos.txt) und weiteren analysierten Dokumenten. Er bildet die Grundlage für vertiefende Architekturarbeiten und technische Spezifikationen in den nächsten Phasen.*