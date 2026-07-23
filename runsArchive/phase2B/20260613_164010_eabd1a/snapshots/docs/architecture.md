# Architekturüberblick — Kunden-/Angebotsportal (Initial Draft)

Datum: 2026-06-13
Quelle: MAF Shared State (input/transcripts/T9999_chaos.txt), docs/requirements.md, docs/risks.md
run_id: 20260613_164010_eabd1a

Kurzbeschreibung
Dieses Dokument liefert einen frühen, technikneutralen Architekturüberblick für das geplante Kunden-/Angebotsportal (MVP-Scope). Es fasst Systemkontext, Hauptkomponenten, Integrationspunkte, Daten- und Sicherheitsaspekte sowie offene Architekturentscheidungen zusammen. Ziel ist eine gemeinsame Grundlage für Detaillierung, Risiko-Minderung und operative Planung.

1. Systemkontext

Akteure/Identitäten:
- Sales (erzeugt Angebote, Editier-Rechte)
- Kunde (sichtet Bestellungen, Rechnungen, lädt PDFs)
- Admin (Benutzer- und Rollenverwaltung)
- Support / Finance (eingeschränkte Einsicht / Aktionen im MVP)
- Externe Systeme: SAP (Quelle für Produkt-/Preis-/Kundendaten), E-Mail-Service (Double-Opt-In, Benachrichtigungen), (später) zentrales API-Gateway, Identity Provider (SSO-Optionen)

Kurzfunktionalität (MVP-Annäherung):
- Login via E-Mail/Passwort mit Double-Opt-In; SSO optional später
- Angebots-Erstellung, Draft/Statuswechsel, PDF-Export
- Rechnungsanzeige / PDF-Download
- Anzeige von Bestellungen (read-only aus SAP)
- Minimales Rollenmodell: Admin, Sales, Kunde

2. Wichtige Komponenten (logische Sicht)

- Frontend (Web UI)
  - Single-Page-App oder serverseitig gerenderte Webapp
  - Verantwortlich für Nutzer-Interaktion, Formularvalidierung, PDF-Download-Trigger
  - Verwendet HTTPS für alle Kommunikation

- Backend / API Layer (BFF oder Microservice-Layer)
  - Authentifizierung & Autorisierung
  - Angebots- und Dokumenten-Services (Angebote CRUD, Statusmanagement, PDF-Rendering)
  - Business-Logik (Rabattregeln, minimale Freigaben nach MVP-Einschränkungen)
  - Audit-Logging API (schreibt Audit-Einträge getrennt von technischen Logs)
  - Schnittstellen zu Integrationskomponenten (SAP-Adapter, E-Mail-Service, ggf. API-Gateway)

- Integrationsadapter / Connector-Layer
  - SAP-Read-Adapter (read-only für Produkt-, Preis-, Kundenstammdaten)
  - Optionale Wege: über zentrales API-Gateway (wenn verfügbar) oder temporäre Direkt-API / BFF-Proxy / Mock-Service für MVP

- Datenhaltung
  - Relationale/NoSQL-Datenbank für User, Angebote, Status, Metadaten (in EU-Region)
  - Separate, append-only Audit-Log-Datenbank/Storage (Zugriffs- und Änderungsnachweis)
  - Objekt-Storage für generierte PDFs/Belege (EU-Region, verschlüsselt)

- Auth & Identity
  - E-Mail/Passwort-Flow mit Double-Opt-In (MVP)
  - SSO-Schnittstellen (Azure AD / Google / anderes) = optional / spätere Iteration
  - Rollen- und Berechtigungs-Subsystem (Admin, Sales, Kunde; erweiterbar)

- Infrastruktur & Betrieb
  - EU-Only Hosting (Compute, DB, Object Storage, Backups)
  - Secrets Management (KMS/SecretStore)
  - Backup & DR-Prozesse (regelmäßige Backups, Restore-Tests)
  - Monitoring, Alerting, Health-Checks, Logging-Pipeline

- Security & Compliance Utilities
  - Maskierung/Redaction für technische Logs
  - Separierte Audit-Logs mit strengem Zugriff
  - Process für Löschanfragen vs. gesetzlicher Aufbewahrung (Anonymisierung/Retention)

3. Schnittstellen und Integrationspunkte

- Frontend ↔ Backend
  - HTTPS (TLS) REST/GraphQL/Web API; Auth-Token (short-lived)

- Backend ↔ SAP
  - Primär: read-only Endpoints für Produkt-/Preis-/Kundendaten
  - Mögliche Integrationspfade:
    - Über zentrales API-Gateway (langfristig angestrebt)
    - Direktadapter / BFF-Proxy, falls Gateway nicht rechtzeitig verfügbar
    - Mock-Service für Test/Pilot oder Fallback mit Caching/Degraded-Mode
  - Anforderungen: definierte Fehlermodi (UI-Hinweis bei fehlender Live-Preis-Verfügbarkeit)

- Backend ↔ E-Mail-Service
  - Double-Opt-In, Benachrichtigungen, System-Mails (Transaktional)

- Backend ↔ Storage/PDF-Generator
  - Service zur Generierung und persistente Ablage von Angebots-/Rechnungs-PDFs

- Backend ↔ Ops-Services
  - Secrets-KMS, Monitoring/Observability, Backup-System

4. Daten- und Sicherheitsaspekte

- Datenresidenz
  - Alle Produktionsdaten und Backups müssen in EU-Regionen verbleiben (C-02). Hosting-Anbieter und Region sind noch zu wählen (AOP-08).

- Verschlüsselung
  - TLS für alle Verbindungen; ruhende Daten verschlüsselt mittels KMS
  - Objekt-Storage (PDFs) verschlüsselt at-rest

- Logging & Audit
  - Audit-Logs: Zeitstempel, Benutzer-ID, Aktion, Kontext; getrennter Zugriff und längere Retention (gesetzliche Anforderungen berücksichtigen)
  - Technische Logs: keine PII; Maskierung/Redaction implementieren
  - Log-Retention-Dauer ist offen und muss mit Legal geklärt werden (AOP-06)

- Datenschutz & Löschung
  - Prozesse zur Verarbeitung von Betroffenenrechten (Auskunft, Berichtigung, Löschung)
  - Löschanfragen müssen gegen gesetzliche Aufbewahrungspflichten geprüft werden; mögliche Implementierung: Ersetzungs-/Anonymisierungsmodus statt vollständiger Löschung für archivpflichtige Datensätze
  - Testdaten-Strategie: keine produktiven Kundendaten in Test; synthetische/anon. Datensets für Entwicklung

- Zugangskontrollen
  - Minimales RBAC im MVP; feinere Rollen (Support, Finance, Manager) später
  - Admin- bzw. temporäre Sonderfreigaben (z. B. für Rabatte) nur mit striktem Audit

5. Betriebsanforderungen und Nicht-Funktionales

- Verfügbarkeit und DR
  - Backup Plan und Restore-Tests vor Produktionsfreigabe
  - MVP-Verfügbarkeitsziel als Annahme: 99% (NFR-03) — final zu bestätigen

- Skalierbarkeit
  - Design für horizontale Skalierung (stateless App-Instances, managed DB/Autoscaling)
  - Annahme: MVP-Startlastszenario klein/medium; Kapazitätsplanung erforderlich (AOP-07)

- Sicherheitstests
  - Risikobasierter Security-Review; Priorisierung kritischer Pfade zuerst (um Time-to-Market nicht vollständig zu blockieren)

6. Wichtige Architektur-Entscheidungen (offen / zu treffen)

- API-Gateway vs. temporäre Direktintegration/BFF/Mock
  - Risiko: API-Gateway-Warteliste (~6 Wochen) ist kritischer Pfad. Empfehlung: Parallelpfad definieren (BFF-Proxy oder Mock-Service) für MVP; Gateway-Integration mittelfristig.

- SSO: ja/nein und Provider-Auswahl
  - MVP: SSO ist optional und kann nach Start ergänzt werden. Konkreter Provider (Azure AD, Google) offen (AOP-04).

- SAP-Scope (Read-only bleibt Annahme)
  - MVP-Annäherung: nur read-only. Schreibzugriffe sollten erst in späteren Phasen implementiert.

- Freigabemechanik für Rabatte
  - MVP: Keine manuellen Sonderrabatte; wenn temporäre Ausnahme erforderlich, nur Admin-Override mit Audit.
  - Konkrete Schwellen (15% / 20% / sonstige) sind offen (AOP-05).

- Retention und Löschprozess
  - Konkrete Regeln zur Aufbewahrung vs. Löschung sind noch ausstehend; Legal/Compliance-Anforderung hat hohe Priorität.

- Hosting-Provider und EU-Region
  - Anbieter/Region noch offen; Auswahl muss EU-only-Auswahl, Vertragsnachweise und technische Geo-Restrictions unterstützen.

7. Architekturmuster & Empfehlungen (ohne feste Technologieangaben)

- Use a layered architecture with a thin frontend and a backend that encapsulates business logic and integrations.
- For SAP integration: implement an adapter pattern so the underlying integration path can be swapped (Gateway vs. direct) with minimal change.
- Separate audit logging from technical logs; apply retention & access controls per policy.
- Use managed services (DB, object storage, KMS) in EU regions to reduce operational burden and accelerate compliance.
- Implement a feature-flagged discount/freigabe mechanism to enable controlled rollouts and emergency overrides (with audit trail).

8. Kurzfristige Maßnahmen (für MVP-Run)

- Sofort: Klären und priorisieren API-Gateway-Access oder implementieren temporäre BFF/Mock-Adapter (höchste Priorität).
- Sicherheits-/Compliance-Pfad: Parallelisiertes, risikobasiertes Security-Review; harte Kontrollen für Prod-Promotion.
- Operational: Auswahl eines EU-Region Managed-Provider mit KMS und Backup-Fähigkeit, Einrichtung Secrets-Management.
- Daten: Erstellen einer Testdatenstrategie (synthetische/anon. Datensätze) und Logging-Policy-Template (Maskierung/Retention).
- Governance: Finalisierung der Rollenmatrix für MVP (Admin, Sales, Kunde; temporäre Support/Finance-Ausnahmeprozesse dokumentieren).

9. Offene Fragen (zur Entscheidungs- und Risikopriorisierung)

- Welcher Pilotkunde / Zielmarkt (Schweiz vs. DACH)? Auswirkungen: Währung, Vertragsanforderungen, ggf. lokale Hosting-Verträge (AOP-01).
- API-Gateway-Availability: Gibt es eine priorisierte Zuteilung oder muss eine temporäre Direktintegration umgesetzt werden? (AOP-03)
- Security-Review Umfang und Zeitplan — welche Prüfungen sind für MVP zwingend? (Risiko: Verzögerung durch Review)
- SSO-Provider: Azure AD, Google oder beides? (AOP-04)
- SAP: Confirm read-only for MVP and availability of test systems with synthetic data. (AOP-02)
- Freigabeschwellen für Rabatte — final decision needed (AOP-05).
- Retention vs. Löschung — Legal input required for implementable process (AOP-06).
- Exact log/audit retention durations and access rules for audit logs (NFR-05).

10. Nächste Schritte (empfohlen)

- Technische Validierung: Prototyp einer SAP-Adapter-Implementierung (Mock + BFF) und Test der UI-Workflows mit anonymisierten Daten.
- Governance: Entscheidungsgremium (Steering Board) priorisiert API-Gateway und Security-Review-Aufgaben; Legal klärt Retention/Löschung.
- Operations: Auswahl eines EU-Region Managed-Provider; initiale Setup-Checklist (TLS, KMS, Backups, Secrets).
- Architekturarbeit: Detaillierte Komponentendiagramme, Datenmodelle (Angebot, Audit, User) und Schnittstellenspezifikationen für Sprint-Planung.

Evidence / Quellen
- Transkript (input/transcripts/T9999_chaos.txt), docs/requirements.md (initial requirements), docs/risks.md (initial risks). run_id: 20260613_164010_eabd1a

---
Hinweis: Dieses Dokument ist ein initialer Architekturüberblick; konkrete Technologieentscheidungen sind bewusst offen gelassen, solange keine Ableitbarkeit aus dem bereitgestellten Kontext besteht. Weitere Detaillierung erfordert Entscheidungen zu den unter "Offene Fragen" genannten Punkten.