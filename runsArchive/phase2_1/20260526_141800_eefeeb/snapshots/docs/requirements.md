# Requirements Document

## Functional Requirements
1. **Kundenportal**: Web‑first Plattform, später mobile Erweiterung (Responsive Design). Kernfunktionen:
   - Angebots‑Erstellung und -Versand für Sales.
   - Anzeige von Bestellungen und Download von Rechnungen für Kunden.
   - Login per E‑Mail und Passwort.
   - Optionales SSO (Azure AD / Google) – als Option für MVP.
   - Rollenbasierter Zugriff (Admin, Manager, Normal‑User, Support).
   - Integration von SAP‑Stammdaten (Produkt, Preis, Rabattlogik).
   - KPI‑Erfassung: Conversion Rate, Zeit bis Angebot (Analytics‑Hook optional, nicht MVP).
2. **API‑Layer**:
   - RESTful Schnittstelle für Frontend und SAP‑Integration.
   - Authentifizierung via OAuth 2.0 (Client‑Credentials + Authorization‑Code), Fallback API‑Key für interne Nutzung.
3. **Compliance‑Features**:
   - Double‑Opt‑In für Registrierungen und Push‑Notifications.
   - Einwilligungs‑Management für Marketing‑Nachrichten.
   - Lösch‑ und Auftragsverarbeitungs‑Konzept (Kunden können Daten löschen).
   - Audit‑Logging aller Änderungen (who, when, what).
4. **Security**:
   - TLS 1.2+ für sämtliche Kommunikation.
   - Verschlüsselung von ruhenden Daten (AES‑256) – Managed Service übernimmt.
   - Security Review (min. Threat‑Model, Pen‑Test) – muss bis MVP‑Launch abgeschlossen sein.
5. **Infrastructure**:
   - Nutzung von Managed Cloud Services (DB, Hosting) – kein neuer DB‑Server.
   - EU‑only Datenhosting (DSGVO‑Konform).
   - Backup & Disaster Recovery (RPO ≤ 4 h, RTO ≤ 8 h).
6. **Performance / Skalierbarkeit**:
   - Unterstützung von 200 bis 20 000 gleichzeitigen Nutzern.
   - Auto‑Scaling auf Managed Platform.
7. **Logging & Monitoring**:
   - Zentralisiertes Log‑Management (Audit‑Logs, Access‑Logs).
   - Grundlegende Health‑Checks (Uptime, Response‑Time).

## Non‑functional Requirements
- **Usability**: Intuitive UI, minimaler Klick‑Weg für Angebotserstellung (<5 Klicks).
- **Reliability**: Verfügbarkeit ≥ 99,5 % im Produktionsbetrieb.
- **Security**: TLS, OAuth, Auditing, DSGVO‑Konformität, Pen‑Test vor Go‑Live.
- **Performance**: Seiten‑Load‑Time < 2 s bei 5 k gleichzeitigen Nutzern.
- **Scalability**: Horizontal skalierbare Infrastruktur, automatisches Scaling.
- **Maintainability**: Dokumentierte API‑Spezifikation (OpenAPI), Rollen‑ und Berechtigungskonzept.
- **Compliance**: DSGVO‑konforme Datenhaltung, Double‑Opt‑In, Auftragsverarbeitungsverträge.
- **Backup**: Tägliche Snapshots, 30‑Tage‑Retention, Wiederherstellungstest monatlich.

## Constraints / Compliance
- **Zeitplan**: MVP in 8 Wochen – erfordert Priorisierung von Kern‑Features und minimale Compliance‑Mindestanforderungen.
- **Budget**: Keine neuen Datenbank‑Server, nur günstige Managed Services.
- **Legal**: Daten dürfen nur innerhalb der EU gespeichert werden (DSGVO).
- **Security Review**: Muss innerhalb des MVP‑Zeitrahmens abgeschlossen sein (ca. 6 Wochen).
- **Infrastructure**: Nutzung vorhandener IT‑Ressourcen, kein Eigen‑DB‑Deployment.

## Traceability
| Requirement ID | Quelle (Transkript/Context) | Beschreibung |
|----------------|-----------------------------|--------------|
| FR‑1 | Anna (Kundenportal, Angebote, Rechnungen) | Kundenportal mit Angebots‑ und Rechnungsfunktionalität |
| FR‑2 | Ben (API‑Layer, SAP‑Integration) | Bereitstellung einer API für Frontend und SAP |
| FR‑3 | Clara (DSGVO, Double‑Opt‑In) | Double‑Opt‑In und Consent‑Management |
| FR‑4 | Anna & Ben (Login, SSO) | E‑Mail/Passwort‑Login, optionales SSO |
| FR‑5 | Anna (Rollen) | Rollen‑ und Berechtigungskonzept (Admin, Manager, User, Support) |
| FR‑6 | Ben (KPI) | Erfassung von Conversion Rate und Zeit bis Angebot |
| NFR‑1 | Ben (Performance) | Load‑Time < 2 s, 5 k gleichzeitige Nutzer |
| NFR‑2 | Clara (Security) | TLS, Audit‑Logging, Pen‑Test vor Go‑Live |
| NFR‑3 | Ben (Scalability) | Unterstützung bis 20 000 Nutzer, Auto‑Scaling |
| CON‑1 | Kontext (8‑Wochen‑MVP) | MVP muss in 8 Wochen lieferbar sein |
| CON‑2 | Clara (EU‑Hosting) | Datenhosting EU‑only (DSGVO) |
| CON‑3 | Ben (Kein neuer DB‑Server) | Nutzung von Managed Services, keine eigene DB |
| CON‑4 | Clara (Backup) | Backup & Disaster Recovery erforderlich |

*Alle Anforderungen basieren ausschließlich auf dem bereitgestellten Transkript und dem Kontext.*