## Anforderungen für das Kundenportal (MVP)

### Functional Requirements
1. **Login**
   - Nutzer können sich per E‑Mail und Passwort anmelden.
   - Double‑Opt‑In per E‑Mail wird für die Registrierung verlangt (DSGVO‑Konformität).
   - TLS‑Verschlüsselung für alle Verbindungen.
2. **Angebotserstellung**
   - Sales‑Nutzer können Angebote im Portal erstellen.
   - Das System liest Produkt‑ und Preisdaten über einen lesenden SAP‑API‑Layer.
   - Im MVP dürfen keine Sonderrabatte über den Standard‑Satz hinaus vergeben werden.
   - Angebote können als PDF exportiert werden; jede PDF enthält rechtlichen Hinweis, Versions‑ID und wird im Audit‑Log erfasst.
3. **Rechnungs‑ und Bestellungs‑Download**
   - Kunden können ihre Rechnungen und Bestellungen im Portal einsehen und als PDF herunterladen.
4. **Rollen‑ und Berechtigungskonzept**
   - Rollen: **Admin**, **Sales**, **Kunde** (und optional **Support** mit reinen Leserechten).
   - Rechte sind strikt nach Rollen getrennt (z. B. kein Preis‑ oder Rabatt‑Einblick für Support).
5. **Audit‑Trail**
   - Kern‑Events (Login, Angebotserstellung, Angebots‑Export, Rechnung‑Download, Rollen‑Änderungen) werden protokolliert.
   - Audit‑Logs enthalten keine personenbezogenen Daten, nur Nutzer‑ID, Event‑Typ, Timestamp und betroffene Objekt‑ID.
6. **EU‑Only Managed Hosting & Backup**
   - Das System wird in einer EU‑Region gehostet (DSGVO‑konform).
   - Tägliche Snapshots mit 30‑Tage‑Retention; Wiederherstellung wird regelmäßig getestet.
7. **API‑Layer**
   - Ein interner API‑Layer ermöglicht das Lesen von SAP‑Daten.
   - OAuth‑Ansatz ist geplant, aber nicht final im MVP; ein simpler Token‑Mechanismus wird vorerst verwendet.
8. **KPI‑Tracking (nicht personenbezogen)**
   - Conversion‑Rate (Angebot → Bestellung).
   - Zeit bis Angebotserstellung.

### Non‑functional Requirements
- **Performance**: Antwortzeit < 2 s für Angebots‑Erstellung bei bis zu 2 000 gleichzeitigen Nutzern (Skalierbarkeit für höhere Last wird später berücksichtigt).
- **Sicherheit**: TLS 1.2+ für alle Kommunikation, getrennte Audit‑ und technische Logs, keine PII in Application‑Logs, Secrets‑Management (z. B. Vault) für API‑Keys.
- **Compliance**: DSGVO‑Konformität (Double‑Opt‑In, Löschkonzept, Daten‑Residenz EU‑Only, Auftrags‑Verarbeitungs‑Verträge).
- **Verfügbarkeit**: 99,5 % Uptime im Produktions‑Umfeld, Backup‑ und Disaster‑Recovery‑Plan aktiv.
- **Scalability**: Nutzung von Managed Services, um später horizontale Skalierung zu ermöglichen.
- **Usability**: Interface in Deutsch und Englisch, responsive Design (Web‑first, Mobile‑Support optional nach MVP).

### Constraints / Compliance
- Keine SSO‑Integration im MVP (Azure AD / Google SSO später).
- Keine vollwertige Ticket‑System‑Integration für Support – nur Kontaktformular und E‑Mail‑Weiterleitung (bewusste Risiko‑Einschränkung).
- Keine Sonderrabatte > Standard‑Satz; Freigabe‑Workflow für Rabatte wird erst in Phase 2 umgesetzt.
- Kein API‑Gateway (6‑Wochen‑Warteliste); ein einfacher interner Proxy wird verwendet.
- Mehr‑Währung (CHF, USD) wird im MVP nicht unterstützt, nur EUR (evtl. CHF‑Pilot später).
- Keine umfangreiche Daten‑Retention‑Policy im MVP; minimaler rechtlicher Aufbewahrungszeitraum wird später definiert.
- Entwicklungs‑ und Testumgebungen verwenden ausschließlich synthetische Daten – kein direkter Zugriff auf produktive SAP‑Daten.

### Assumptions and Open Points
- **SAP‑Verfügbarkeit** ist kritische Abhängigkeit – kein Fallback‑Cache im MVP (Risiko). 
- **Kosten‑Schätzung** für EU‑Only Managed Hosting wird bis Freitag geliefert (offener Punkt). 
- **Support‑Prozess** bleibt unklar; derzeit nur Kontaktformular (Risiko). 
- **Rabatt‑Freigabe‑Logik** wird später definiert (15 % / 30 % Schwellenwerte). 
- **Internationalisierung** (Sprachen, Rechtsrahmen) wird nach MVP geplant. 
- **Backup‑Provider** Auswahl muss noch erfolgen. 
- **API‑Gateway** Beschaffung – 6‑Wochen‑Verzögerung, ggf. interimslösung. 
- **Retention vs. Löschkonzept** – Konflikt zwischen gesetzlicher Aufbewahrungspflicht und DSGVO‑Löschrecht (zu klären). 
- **Rate‑Limiting** für PDF‑Downloads wird später implementiert (aktueller MVP ohne). 

### Traceability
| Anforderung | Quelle | Status |
|-------------|--------|--------|
| Login mit Double‑Opt‑In | Transkript (Anna, Clara) | Implementiert (MVP) |
| SAP‑lesender API‑Layer | Transkript (Ben) | Implementiert (MVP) |
| Keine Sonderrabatte im MVP | Transkript (Eva) | Implementiert (MVP) |
| Rollen: Admin, Sales, Kunde | Transkript (Anna) | Implementiert (MVP) |
| Audit‑Trail (Core Events) | Transkript (Clara) | Implementiert (MVP) |
| EU‑Only Hosting & Backup | Transkript (Farid) | Implementiert (MVP) |
| KPI‑Tracking (Conversion, Time‑to‑Offer) | Transkript (Anna) | Implementiert (MVP) |
| Double‑Opt‑In & DSGVO | Transkript (Clara) | Implementiert (MVP) |
| Kontaktformular für Support | Transkript (David) | Implementiert (MVP) |
| OAuth‑Planung (nicht final) | Transkript (Ben) | Geplant (Phase 2) |
| SSO‑Integration | Transkript (Anna) | Excluded (MVP) |
| API‑Gateway | Transkript (Farid) | Excluded (MVP) |
| Mehr‑Währung | Transkript (Eva) | Excluded (MVP) |
| Internationalisierung | Transkript (Anna) | Excluded (MVP) |

*Alle genannten Punkte leiten sich aus den Stakeholder‑Transkripten `input/transcripts/T9999_chaos.txt` und dem zusammengefassten Kontext `runs/phase2_1/20260529_110440_365ef4/state/context.md` ab.*