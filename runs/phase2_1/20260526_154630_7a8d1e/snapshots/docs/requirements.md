# Requirements Document

## Functional Requirements
1. **Kundenportal**
   - Web‑basierte Plattform (MVP) für Kunden zur Ansicht von Bestellungen und Rechnungen.
   - Möglichkeit zur Erstellung und Anzeige von Angeboten durch das Vertriebsteam.
   - Login per E‑Mail und Passwort (SSO via Azure AD / Google optional, nicht zwingend für MVP).
   - Rollenbasierte Zugriffskontrolle (Admin, User, Manager, Support).
   - Integration von SAP‑Stammdaten für Produkt‑, Preis‑ und Rabattinformationen.
   - Export / Download von Rechnungen als PDF.
   - KPI‑Dashboard (Conversion Rate, Zeit bis Angebot) – grob implementiert für MVP.
2. **API‑Layer**
   - RESTful API zur Anbindung des Portals an SAP und zukünftige Mobile‑Clients.
   - Authentifizierung über OAuth 2.0 (Access‑Token) – optional API‑Key als Fallback.
3. **DSGVO‑Compliance**
   - Double‑Opt‑In bei Registrierung.
   - Möglichkeit zur Datenlöschung auf Kunden‑Anfrage.
   - Logging aller relevanten Aktionen (Login, Datenänderungen) mit Zeitstempel und Nutzer‑ID.
   - Audit‑Trail für Änderungen an Stammdaten und Angeboten.
4. **Security & Infrastructure**
   - TLS 1.2+ für sämtliche Datenübertragungen.
   - Nutzung von Managed Services (Datenbank, Hosting) – keine eigene DB‑Instanz.
   - Datenhosting ausschließlich in EU‑Regionen.
   - Backup & Disaster Recovery für Kundendaten.
5. **Performance & Skalierbarkeit**
   - Unterstützung von 200 bis 20 000 simultanen Nutzern (Auto‑Scaling von Managed Services).
   - Antwortzeit < 2 s für Kern‑CRUD‑Operationen im Portal.

## Non‑functional Requirements
- **Verfügbarkeit:** 99,5 % monatliche Verfügbarkeit des Portals.
- **Sicherheit:** Durchführung eines Security Reviews vor Produktions‑Go‑Live (kann parallel zum MVP erfolgen).
- **Usability:** Intuitive UI, responsive Design (mobile Geräte unterstützen, aber nicht primär fokussiert).
- **Maintainability:** Dokumentation des Rollen‑ und Berechtigungskonzepts, API‑Spezifikation (OpenAPI), Deployment‑Skripte.
- **Compliance:** Vollständige DSGVO‑Konformität (Datenhost, Löschkonzept, Einwilligungen).
- **Kosten:** Nutzung kostengünstiger Managed Services, keine eigenen Server.

## Constraints / Compliance
- **Zeit:** MVP muss innerhalb von 8 Wochen lieferbar sein.
- **Budget:** Keine Anschaffung neuer Datenbank‑Server; Managed Services bevorzugt.
- **Regulatorisch:** Daten ausschließlich in EU‑Regionen speichern, DSGVO‑Anforderungen (Logging, Audit, Lösch‑ und Rollen‑konzept).
- **Technisch:** Backend muss über eine API‑Schicht verfügen; kein vorhandenes API‑Ready Backend zum Projektstart.

## Traceability
| Anforderung | Quelle |
|-------------|--------|
| Kundenportal (Web) | T9999_chaos.txt – Anna
| Login per E‑Mail/Passwort | T9999_chaos.txt – Anna
| Optional SSO (Azure/Google) | T9999_chaos.txt – Anna/Ben
| Rollen (Admin, User, Manager, Support) | T9999_chaos.txt – Anna
| SAP‑Integration (Stammdaten) | T9999_chaos.txt – Ben
| API‑Layer nötig | T9999_chaos.txt – Ben
| OAuth‑2.0 Auth | T9999_chaos.txt – Clara
| DSGVO‑Compliance (Double‑Opt‑In, Logging) | T9999_chaos.txt – Clara
| Backup & DR | T9999_chaos.txt – Clara/Ben
| Skalierbarkeit (200‑20 000 Nutzer) | T9999_chaos.txt – Ben
| MVP Zeitrahmen 8 Wochen | T9999_chaos.txt – Anna
| Managed Services, keine neue DB | T9999_chaos.txt – Ben/Anna
| EU‑only Hosting | T9999_chaos.txt – Clara/Anna
| KPI‑Dashboard (Conversion Rate, Zeit bis Angebot) | T9999_chaos.txt – Anna
| Security Review notwendig | T9999_chaos.txt – Clara/Ben
