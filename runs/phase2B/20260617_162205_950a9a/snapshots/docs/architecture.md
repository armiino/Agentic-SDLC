# Architekturüberblick (frühe SDLC‑Phase)

## 1. Systemkontext
- **Kunden** greifen über einen Browser (Web‑First, später Mobile‑First) auf das **Kundenportal** zu.
- Das Portal kommuniziert über HTTPS mit einem **API‑Layer**, der in einem EU‑only Managed Service gehostet wird.
- **Authentifizierung** erfolgt per E‑Mail/Passwort mit Double‑Opt‑In; SSO (Azure AD, Google) ist für spätere Phasen geplant.
- Das System muss **SAP** für Produkt‑ und Preisdaten lesend anbinden (Schreibzugriff erst in späteren Phasen).
- **Managed Services** (z. B. Cloud‑Functions, Object‑Storage) werden genutzt, keine eigene relational‑DB.
- **Compliance‑Umgebung**: DSGVO‑Konformität, EU‑Datenresidenz, getrennte technische/Audit‑Logs, Backup & DR.

## 2. Wichtige Komponenten
| Komponente | Aufgabe | Hinweis |
|------------|----------|--------|
| **Responsive Web‑Frontend** | UI für Kunden (Angebote, Bestellungen, Rechnungen) | Web‑First, später responsive für Mobile |
| **API‑Layer / Proxy** | Exponiert REST‑Endpoints für Frontend; übernimmt Routing, Rate‑Limiting, Auth‑Checks | Temporärer Server‑less Proxy bis API‑Gateway verfügbar |
| **Auth‑Service** | Benutzerverwaltung, Double‑Opt‑In, Passwort‑Reset, zukünftige SSO‑Anbindung | DSGVO‑konforme Speicherung von Anmeldedaten |
| **Role & Permission Service** | Rollen (Admin, Sales, Manager, Support) & feingranulare Berechtigungen (z. B. Support ohne Preis‑Details) | ABAC‑Ansatz empfohlen |
| **Offer‑Workflow Service** | Zustandsmaschine für Draft → Pending Approval → Approved → Sent; Rabatt‑Freigabe‑Logik | Schwellenwert (15 %) noch zu finalisieren |
| **PDF‑Export Service** | Generiert rechtlich konforme PDFs mit Footer‑Hinweisen für Angebote & Rechnungen | Nutzt Managed PDF‑Generator |
| **Currency Service** | Unterstützt EUR, CHF (Pilot), später USD; Währungs‑Umrechnung & Formatierung |
| **SAP‑Read Adapter** | Lesezugriff auf Produkt‑ und Preisdaten via SAP OData/REST | Caching‑Strategie offen (Performance vs. DSGVO) |
| **Analytics / KPI Service** | Erfasst Conversion‑Rate, Zeit bis Angebot, Nutzungsstatistiken; erfordert Consent‑Management |
| **Logging & Audit Service** | Technische Logs vs. Audit‑Logs, Aufbewahrung nach Vorgaben, Verschlüsselung |
| **Backup & Disaster Recovery** | Tägliche Snapshots, 24‑h RTO (MVP‑Minimal), EU‑Region Storage |
| **Monitoring & Alerting** | Health‑Checks, Latency‑Monitoring, automatisierte Alerts |

## 3. Schnittstellen / Integrationspunkte
- **Frontend ↔ API‑Layer**: HTTPS/REST, JWT‑basiertes Auth‑Token.
- **API‑Layer ↔ Auth‑Service**: Aufruf von Login, Double‑Opt‑In‑Flows.
- **API‑Layer ↔ Role/Permission Service**: Authorisierungs‑Check pro Request.
- **API‑Layer ↔ Offer‑Workflow Service**: CRUD‑Operationen für Angebote, Freigabe‑Logik.
- **API‑Layer ↔ PDF‑Export Service**: Aufruf zur PDF‑Erstellung, Rückgabe eines Signed URLs.
- **API‑Layer ↔ SAP‑Read Adapter**: GET‑Requests für Produkt‑/Preis‑Daten; ggf. temporärer Proxy bis API‑Gateway verfügbar.
- **API‑Layer ↔ Analytics Service**: Event‑Senden (nach Einwilligung).
- **Logging/Audit Service ↔ All Services**: Zentraler Log‑Sink (z. B. Cloud‑Log‑Service) mit getrennten Streams.
- **Backup Service ↔ Managed Storage**: Automatisierte Snapshots, Verschlüsselung.

## 4. Daten‑ und Sicherheitsaspekte
- **Transport**: Durchgängig TLS 1.2+.
- **At‑Rest**: Verschlüsselung aller gespeicherten Daten (Objekt‑Storage, Logs).
- **DSGVO**: Double‑Opt‑In für Benutzerregistrierung, Audit‑Trail für alle kritischen Aktionen, automatisiertes Löschkonzept (Richtlinien‑basiert, z. B. 30 Tage).
- **Zugriffskontrolle**: Role‑Based + Attribute‑Based Access Control (z. B. Support‑Rolle darf Kundendaten, aber nicht Preis/Daten sehen).
- **Logging**: Trennung von technischen Logs (Performance) und Audit‑Logs (Datenschutz‑relevant). Aufbewahrungsfristen gemäß Compliance.
- **Backup/DR**: EU‑Only Storage, tägliche Snapshots, Wiederherstellungszeit < 24 h, RPO = 4 h (MVP‑Minimal).
- **Monitoring**: Separate Pipelines für Betriebs‑ und Sicherheits‑Alerts.

## 5. Offene Architekturentscheidungen (MVP‑Status)
1. **Mobile‑First vs. Web‑First** – Klärung im UI‑Workshop, beeinflusst UI‑Framework und spätere native App‑Strategie.
2. **SSO‑Integration** – Zeitpunkt und Provider (Azure AD, Google) nach MVP‑Release.
3. **Cache‑Strategie für SAP‑Daten** – Direktzugriff vs. anonymisierter Produkt‑Cache (Performance vs. Datenminimierung).
4. **API‑Gateway Auswahl** – Temporärer Proxy bis offizielles Gateway nach 6 Wochen bereitsteht.
5. **Rabatt‑Freigabe‑Schwelle & Verantwortlichkeiten** – 15 % vorgeschlagen, muss final mit Finance & Management abgestimmt werden.
6. **Pilotkunde‑Region** – Schweiz (CHF) vs. Deutschland (EUR) beeinflusst Währungs‑ und Rechtsaspekte.
7. **Consent‑Management‑Tool** – Auswahl einer DSGVO‑konformen Lösung vor Aktivierung von Analytics.
8. **Backup‑Strategie (RPO/RTO)** – Minimal‑MVP‑Definition finalisieren mit IT‑Operations.
9. **Monitoring‑Tooling** – Cloud‑Native vs. Drittanbieter‑Lösungen.
10. **Logging‑Pipeline** – Zentraler Log‑Service (z. B. Elastic, CloudWatch) vs. spezialisierte Audit‑Log‑Store.

## 6. Constraints / Annahmen
- **EU‑only Hosting** – Alle Dienste laufen in EU‑Regionen.
- **Managed Services Only** – Keine eigene DB, Nutzung von Cloud‑Storage & Functions.
- **DSGVO‑Compliance** ist ein Muss (Double‑Opt‑In, Audit‑Log, Löschkonzept).
- **MVP‑Umfang** beschränkt sich auf Web‑Portal, E‑Mail/Passwort‑Login, Rollen‑ und Angebots‑Workflow, SAP‑Read‑Integration, PDF‑Export, Grund‑Analytics (nach Consent).

*Dieses Dokument stellt den frühen Architekturüberblick dar und dient als Ausgangspunkt für weitere Detail‑Design‑ und Entscheidungs‑Workshops.*
