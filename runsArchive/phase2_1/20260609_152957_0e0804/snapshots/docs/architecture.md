# Architekturüberblick Kundenportal

## 1. Systemkontext
Das zu entwickelnde Kundenportal ist eine Web-basierte Plattform (Web-first), die später um mobile Zugänge erweitert werden soll. Kernaufgabe ist die effiziente Erstellung und Verwaltung von Angeboten sowie das Bereitstellen von Rechnungen für Kunden. Das Portal wird in engem Zusammenspiel mit dem SAP-System als Backend aufgebaut, wobei SAP primär Produkt-, Preis- und Rabattinformationen bereitstellt.

Die Systemumgebung sieht vor:
- Hosting ausschließlich in der EU unter Berücksichtigung der DSGVO.
- Nutzung von Managed Services zur Datenhaltung und Infrastruktur, ohne eigenen Datenbankserver.

## 2. Wichtige Komponenten

### 2.1 Web-Frontend
- Responsive Web-Anwendung für Kunden, Sales und Administratoren.
- Funktionen: Benutzer-Authentifikation, Anzeige und Erstellung von Angeboten, Rechnungsanzeige, Download von Dokumenten.

### 2.2 API Layer
- Vermittler zwischen Frontend und Backend.
- Zuständig für SAP-Integration, Authentifizierung, Autorisierung und Serviceabstraktion.
- Realisierung einer sicheren API mit OAuth als bevorzugtem Authentifizierungsverfahren (alternativ E-Mail/Passwort).

### 2.3 Backend SAP-Integration
- Primär lesender Zugriff auf SAP-Daten (Produkt, Preis, Rabatt).
- Verwendung von Caching und Fallback-Mechanismen zur Abmilderung von Verfügbarkeitsproblemen.

### 2.4 Authentifizierungs- und Rollenmanagement
- Verwaltung der Nutzerrollen (Admin, Sales, Manager, Support, Kunde).
- Unterstützung von Single Sign-On optional.
- Umsetzung von Sicherheitsrichtlinien entsprechend DSGVO (Double-Opt-In, Löschkonzepte).

### 2.5 Logging und Audit
- Differenzierung zwischen technischen Logs und audit-konformen Protokollen.
- Speicherung von Benutzeraktionen sowie Änderungshistorien zur Revisionssicherheit.

### 2.6 Backup und Security Review
- Backup der Kundendaten über Managed Services.
- Geplante Sicherheitsprüfungen, auch wenn MVP-Zeitrahmen diese erschwert.

## 3. Schnittstellen und Integrationspunkte

- API Gateway (zentraler Zugriffspunkt; Nutzung steht auf Warteliste, temporäre Alternativen bedenken).
- SAP-Backend (Datenquelle für Preise, Produkte, Rabatte).
- Identity Provider (optional für SSO, z.B. Azure AD, Google ID).

## 4. Daten- und Sicherheitsaspekte

- Einhaltung der DSGVO-Anforderungen, insbesondere bei personenbezogenen Daten (Login, Angebote, Rechnungen).
- EU-only Hosting, Sicherstellung der Datenresidenz.
- Datenminimierung: nur notwendige Kundendaten werden gespeichert, ergänzende Daten fließen dynamisch aus SAP.
- Rollenbasierte Zugriffskontrolle mit feingranularen Berechtigungen.
- Audit- und Löschkonzepte, Retention Policies abgestimmt auf gesetzliche und Compliance-Anforderungen.

## 5. Offene Architekturentscheidungen

- Nutzung und Termin der API Gateway Integration aufgrund der aktuellen Warteliste.
- Endgültige Authentifizierungsmethode: OAuth vs. einfacheres Login-Verfahren.
- Ausgestaltung und Umfang des Supportprozesses im MVP (Kontaktformular vs. Ticketsystem).
- Implementierung der Rabattfreigabeprozesse und Workflow-Status im Angebotssystem.
- Umgang mit Mehrwährung und Internationalisierung (initial DACH, Erweiterung unklar).
- Handhabung von Testdaten und Geheimnisverwaltung in Entwicklungs- und Testumgebungen.

## 6. Zusammenfassung
Diese Architektur bietet einen modularen Aufbau mit klarem Fokus auf ein MVP im Zeitrahmen von 8 Wochen unter Berücksichtigung der Compliance und technischer Restriktionen. Die erkannten Risiken und offenen Fragen sind transparent dokumentiert und werden durch iterative Abstimmung mit den Stakeholdern adressiert.

---

Diese erste Architekturübersicht basiert auf der Auswertung des vollständigen Transkripts input/transcripts/T9999_chaos.txt, den daraus abgeleiteten Requirements in docs/requirements.md sowie der Risikoanalyse in docs/risks.md.
