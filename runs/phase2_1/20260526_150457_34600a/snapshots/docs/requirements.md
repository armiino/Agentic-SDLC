# Requirements Document

## Functional Requirements

1. **Kundenportal (Web‑first)**
   - Authentifizierung per E‑Mail & Passwort.
   - Optionales SSO (Azure AD, Google) als Erweiterung.
   - Rollenbasierte Zugriffskontrolle: Admin, Manager, Normal‑User, Support.
2. **Angebots‑Management**
   - Erstellung, Speicherung und Versand von Angeboten.
   - Integration von Produkt‑, Preis‑ und Rabatt‑Daten aus SAP.
   - KPI‑Tracking für Conversion Rate und Zeit bis Angebot (Nur Erfassung, Auswertung optional).
3. **Bestell‑ & Rechnungs‑View**
   - Kunden können ihre Bestellungen einsehen.
   - Rechnungen können als PDF heruntergeladen werden.
4. **API‑Layer**
   - REST‑API zur Anbindung von Front‑end und SAP.
   - Authentifizierung via OAuth 2.0 (Bearer‑Token). Fallback: API‑Key (für interne Nutzung).
5. **SAP‑Integration**
   - Synchronisation von Stammdaten und Produktdaten.
   - Nur lesender Zugriff für Angebote; Schreibzugriff für Admin‑Rollen.
6. **Logging & Auditing**
   - Ereignis‑Logging für Login, Datenänderungen und API‑Aufrufe.
   - Audit‑Trail für Änderungen an Kundendaten und Angeboten.
7. **Datenschutz‑Features**
   - Double‑Opt‑In bei Registrierung.
   - Rechtskonformes Lösch‑ und Export‑Feature für Kundendaten.
   - TLS‑Verschlüsselung für Datenübertragung.
8. **Backup & Disaster Recovery**
   - Tägliches Backup der Daten in EU‑Region.
   - Wiederherstellungs‑SLA: max. 4 Stunden.
9. **Performance / Skalierbarkeit (MVP)**
   - Unterstützung von 200 bis 20 000 gleichzeitigen Nutzern (Managed Service, auto‑scaling).
10. **Documentation**
    - Minimal‑Dokumentation für API, Sicherheits‑ und Datenschutz‑Konzept.

## Non‑functional Requirements

- **Security**: TLS 1.2+ für alle Verbindungen, OAuth 2.0 für API, rollenbasierte Zugriffskontrolle, regelmäßiges (mind. wöchentliches) Security Review.
- **Compliance**: Vollständige DSGVO‑Konformität (DSGVO‑konforme Hosting‑Region EU, Datenlöschkonzept, Audit‑Logs, Double‑Opt‑In).
- **Reliability**: Verfügbarkeit ≥ 99,5 % (Managed Service), automatisierte Backups, Disaster‑Recovery‑Plan.
- **Usability**: Responsive Web‑UI, intuitive Benutzeroberfläche, lokalisierbare Texte (Deutsch/Englisch).
- **Performance**: Antwortzeit < 2 s für API‑Calls unter Normallast (≤ 200 Requests/s), skalierbar auf 5 000 Requests/s.
- **Maintainability**: Code‑Basis in gängigen Frameworks (z. B. React Frontend, Node.js/Java Spring Backend), CI/CD‑Pipeline, automatisierte Tests >= 70 % Coverage.
- **Cost**: Nutzung von Managed Services mit Kostenoptimierung, keine neue Datenbank‑Instanz.

## Constraints / Compliance

- **Hosting**: Daten dürfen ausschließlich in EU‑Regionen gespeichert werden (DSGVO‑konform).
- **Infrastructure**: Keine eigenständige Datenbank; Nutzung von Managed Database‑Service (z. B. Azure SQL oder AWS RDS EU).
- **Budget**: Lösung muss kostengünstig sein, Over‑Engineering vermeiden.
- **Timebox**: MVP muss in 8 Wochen fertiggestellt sein.
- **Security Review**: Muss vor Go‑Live abgeschlossen sein – kann parallel zum MVP‑Rollout erfolgen, aber nicht vollständig ausgelassen.

## Traceability

| ID | Anforderung | Quelle |
|----|--------------|--------|
| FR‑001 | Kundenportal mit E‑Mail/Passwort‑Login | T9999_chaos.txt (Anna) |
| FR‑002 | Optionales SSO (Azure AD/Google) | T9999_chaos.txt (Anna) |
| FR‑003 | Rollen: Admin, Manager, User, Support | T9999_chaos.txt (Anna) |
| FR‑004 | Angebotserstellung & -versand | T9999_chaos.txt (Anna) |
| FR‑005 | SAP‑Produkt‑/Preis‑Integration | T9999_chaos.txt (Ben) |
| FR‑006 | Rechnungs‑Download | T9999_chaos.txt (Anna) |
| FR‑007 | API‑Layer (REST) | T9999_chaos.txt (Ben) |
| FR‑008 | OAuth 2.0 Authentifizierung | T9999_chaos.txt (Clara) |
| FR‑009 | Logging & Audit‑Trail | T9999_chaos.txt (Clara) |
| FR‑010 | Double‑Opt‑In bei Registrierung | T9999_chaos.txt (Clara) |
| FR‑011 | Datenlösch‑ und Export‑Feature | T9999_chaos.txt (Clara) |
| FR‑012 | TLS‑Verschlüsselung | T9999_chaos.txt (Ben) |
| FR‑013 | Backup & DR in EU | T9999_chaos.txt (Clara) |
| FR‑014 | Skalierbarkeit für 200‑20 000 Nutzer | T9999_chaos.txt (Ben) |
| NFR‑001 | TLS 1.2+ | T9999_chaos.txt (Ben) |
| NFR‑002 | DSGVO‑Konformität | T9999_chaos.txt (Clara) |
| NFR‑003 | Verfügbarkeit ≥ 99,5 % | T9999_chaos.txt (Ben) |
| NFR‑004 | Antwortzeit < 2 s | T9999_chaos.txt (Ben) |
| NFR‑005 | Kostenoptimierung, keine neue DB | T9999_chaos.txt (Anna/Ben) |
| NFR‑066? | Timebox 8 Wochen | T999"