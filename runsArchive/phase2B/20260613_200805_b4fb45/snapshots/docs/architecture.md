# Architekturüberblick (frühe SDLC‑Phase)

## Systemkontext
Das Kundenportal wird als web‑basiertes Frontend für **Sales‑Mitarbeiter** und **Kunden** bereitgestellt. Es greift auf interne Systeme (SAP für Produkt‑, Preis‑ und Kundendaten) sowie auf Infrastruktur‑Services (Authentifizierungs‑/Autoriserungs‑Service, Audit‑Log‑Service, Backup/DR) zu. Das System muss EU‑only gehostet werden, DSGVO‑konform sein und innerhalb von 8 Wochen ein MVP liefern.

## Wichtige Komponenten
| Komponente | Aufgabe |
|------------|---------|
| **Frontend (Web UI)** | Login, rollen‑basiertes UI, Angebotserstellung, Rechnungsanzeige, Profilverwaltung |
| **API‑Layer / Backend** | Geschäftslogik, Validierung, Aufruf SAP‑Connector, Erstellung PDF‑Rechnungen |
| **Auth‑Service** | Authentifizierung (OAuth 2.0 oder API‑Key) und Rollen‑basiertes Autorisationsmodell |
| **SAP‑Connector** | Lesender Zugriff auf Produkt‑, Preis‑ und Kundendaten; Schreibzugriff für Angebote optional |
| **Audit‑Log‑Service** | Aufnahme sicherheitsrelevanter Ereignisse ohne personenbezogene Daten in technischen Logs |
| **Backup & DR Service** | Tägliche Snapshots, EU‑Only Managed Service, Wiederherstellungstest |
| **Rate‑Limiting / Abuse‑Protection** | Middleware‑basiertes Schutz‑Mechanismus (API‑Gateway nicht im MVP) |
| **Notification Service (optional)** | Push‑Benachrichtigungen bei neuen Angeboten / Rechnungen |
| **Support‑Kontaktformular** | Einfache Kontaktaufnahme, Speicherung DSGVO‑konform |

## Schnittstellen / Integrationspunkte
- **Frontend ↔ API‑Layer**: HTTPS/REST JSON, TLS 1.2+ 
- **API‑Layer ↔ Auth‑Service**: Token‑Validierung (OAuth‑Introspektion oder API‑Key‑Lookup) 
- **API‑Layer ↔ SAP‑Connector**: OData/REST Aufrufe, mögliches Caching 
- **API‑Layer ↔ Audit‑Log‑Service**: Asynchrone Log‑Einträge (z.B. via Message Queue) 
- **API‑Layer ↔ Backup Service**: Periodische Snapshots über Cron‑Jobs 
- **Frontend ↔ Notification Service**: Web‑Push API (optional) 
- **Frontend ↔ Support‑Formular**: E‑Mail‑Versand 

## Daten‑ und Sicherheitsaspekte
- **DSGVO**: Double‑Opt‑In, Recht auf Vergessenwerden, getrennte technische und Audit‑Logs, Datenminimierung. 
- **Verschlüsselung**: TLS 1.2+ für Daten in Transit, AES‑256 at‑rest für Datenbanken und Backups. 
- **Authentifizierung**: Entscheidung zwischen OAuth 2.0 (empfohlen) und API‑Key noch offen; beide benötigen sicheres Secrets‑Management. 
- **Autorisation**: Rollen‑basiertes RBAC (Admin, Sales, Kunde). 
- **Rate‑Limiting**: Eigen‑implementierte Middleware oder Cloud‑WAF, Schwellenwerte müssen definiert werden. 
- **Backup & DR**: EU‑Only Managed Service, tägliche Snapshots, Aufbewahrung ≥ 30 Tage, regelmäßige Wiederherstellungstests. 
- **Logging**: Technische Logs ohne personenbezogene Daten, Audit‑Logs pseudonymisiert.

## Offene Architekturentscheidungen
1. **Authentifizierungsmechanismus** – OAuth 2.0 vs. API‑Key. 
2. **Rate‑Limiting‑Strategie** – Middleware, Cloud‑WAF oder Service‑Mesh. 
3. **SAP‑Schreibzugriff** – Schreibrechte für Angebotserstellung noch zu klären. 
4. **Währungsunterstützung** – CHF und ggf. USD im MVP noch offen. 
5. **Backup‑ und DR‑Details** – Frequenz, Retention, Test‑Procedures sind noch zu spezifizieren. 
6. **Notification Service** – ob Push‑Benachrichtigungen im MVP enthalten sind. 
7. **Hosting‑Provider‑Details** – EU‑Only Managed Service, Kosten‑ und Leistungsdetails fehlen.

*Dies ist der erste Entwurf des Architektur‑Überblicks und dient als Grundlage für weitere Detail‑Design‑ und Entscheidungs‑Schritte.*