# Grober Architekturüberblick für das Kundenportalprojekt

## 1. Systemkontext
Das geplante System ist ein Kundenportal, das primär zur schnelleren Angebotserstellung und Rechnungsanzeige für Kunden dient. Es integriert Produkt- und Preisdaten aus dem bestehenden SAP-System und bietet verschiedene Nutzerrollen (z.B. Admin, Sales, Kunde, Manager, Support) mit differenzierten Berechtigungen. Das System wird als Webanwendung realisiert, mobile Unterstützung ist für spätere Phasen vorgesehen.

## 2. Wichtige Komponenten

### 2.1 Kundenportal (Frontend)
Das Kundenportal stellt die zentrale Benutzeroberfläche bereit. Funktionen umfassen Login (E-Mail/Passwort, optional SSO), Angebotserstellung, Rechnungsdownload und das Kontaktformular für Supportanfragen. Die Benutzerverwaltung bildet Rollenkonzepte ab (Admin, Sales, Manager, Support).

### 2.2 SAP-Integration
Das System greift lesend auf SAP-Daten zu, insbesondere Produktbeschreibungen, Preise und Rabattlogik. Schreibzugriffe auf SAP, wie Online-Akzeptanz von Angeboten, sind im MVP ausgeschlossen. SAP-Verfügbarkeiten und Wartungsfenster beeinflussen die Integration und erfordern ggf. Fallback- oder Caching-Strategien.

### 2.3 API Layer
Ein API-Layer wird benötigt, um Backend-Services sicher und konsistent bereitzustellen. Die Authentifizierung soll mittels OAuth realisiert werden, wobei dieser Aspekt noch offen ist. Der Zugang über ein zentrales API Gateway ist vorgesehen, allerdings mit möglichen Verzögerungen in der Verfügbarkeit.

### 2.4 PDF-Generierung
Das System generiert rechtsgültige PDF-Dokumente für Angebote inklusive rechtlicher Fußnoten und Versionshistorie zur revisionssicheren Nachvollziehbarkeit.

### 2.5 Supportkontakt
Für den MVP ist ein Kontaktformular für Supportanfragen vorgesehen. Ein vollwertiges Ticketsystem wird nicht implementiert. Dies stellt eine bewusste Einschränkung und ein Risiko dar.

### 2.6 Hosting und Betrieb
Das Hosting erfolgt in der EU mit Managed Services für Infrastruktur und Datenbanken, um DSGVO-Konformität sicherzustellen. Backup- und Disaster-Recovery-Konzepte sind Teil der Architektur.

### 2.7 Logging und Auditing
Ein Log- und Audit-System wird eingerichtet, um Benutzeraktionen nachvollziehbar zu machen. Es wird darauf geachtet, personenbezogene Daten im technischen Logging zu vermeiden und compliancekonforme Audit-Trails zu gewährleisten.

## 3. Schnittstellen und Integrationspunkte
- SAP-System: Lesende Datenintegration für Produkt- und Preisinformationen
- Identity Provider (SSO): Optional in späteren Phasen, noch nicht final entschieden
- API Gateway: Zentrale Schnittstelle für Backend-APIs, Verfügbarkeit kann zeitlich verzögert sein
- Supportkontakt: E-Mail-basiertes Kontaktformular ohne Persistenz

## 4. Daten- und Sicherheitsaspekte
- Strikte DSGVO-Konformität wird durch Datenresidenz in der EU, Double-Opt-In, Löschkonzepte und Auditierung angestrebt.
- Zugriffskontrolle und Rollenmanagement sind essentiell, insbesondere zur Trennung von Zugriffsrechten auf sensible Daten.
- Verschlüsselung erfolgt mindestens über TLS; Ende-zu-Ende-Verschlüsselung wird nicht erreicht.
- Backup und Disaster Recovery sind als Managed Services konzipiert.

## 5. Offene Architekturentscheidungen
- Auswahl und Umsetzung der Authentifizierung (SSO Provider, OAuth vs. API Keys)
- Nutzung und Zeitplan für das zentrale API Gateway
- Ausgestaltung des Rabatt-Freigabeprozesses und dessen technische Umsetzung
- Langfristige Lösung für Support-Ticketmanagement
- Fallback- und Caching-Strategien bei Nichtverfügbarkeit von SAP
- Detaillierte Umsetzung der Auditlogik und deren Datenschutzkonformität
- Hosting-Provider und konkrete Cloud-Technologien

## 6. Zusammenfassung
Die Architektur basiert auf einem modularen Ansatz mit klaren Komponenten für Frontend, Backend-API, SAP-Integration und Betrieb. Datenschutz und Sicherheit sind integraler Bestandteil der Architektur, ohne dabei grundlegende technische Machbarkeit und MVP-Timeline aus den Augen zu verlieren. Offene Fragen und Risiken sind dokumentiert und erfordern weitere Entscheidungen im Verlauf der SDLC-Phase.

---

*Diese Architektur kann als Grundlage für die weitere Planung und Diskussion im Projektteam verwendet werden.*