# Requirements Dokument – Kundenportal (MVP)

## 1. Functional Requirements

| ID | Anforderung | Beschreibung | Quelle |
|----|-------------|--------------|--------|
| FR-1 | Login mit E‑Mail/Passwort | Nutzer können sich mit E‑Mail und Passwort anmelden. TLS‑Verschlüsselung muss verwendet werden. | Transkript Zeile 1‑4 |
| FR-2 | Double‑Opt‑In bei Registrierung | Nach Eingabe der E‑Mail wird ein Bestätigungslink gesendet, der bestätigt werden muss, bevor das Konto aktiviert wird. | Transkript Zeile 6‑8 |
| FR-3 | Rollen‑basiertes Zugriffskontrollmodell | Drei Rollen: **Admin**, **Sales**, **Kunde**. Jeder Rolle sind spezifische Berechtigungen zugewiesen (z. B. Admin kann Nutzer verwalten, Sales Angebote erstellen, Kunde seine Daten einsehen). | Transkript Zeile 27‑33 |
| FR-4 | Angebotserstellung (Read‑Only SAP) | Sales kann über das Portal Angebote erstellen. Die benötigten Produkt‑, Preis‑ und Rabattdaten werden aus SAP gelesen. Im MVP dürfen keine Sonderrabatte > Standard‑Rabatt angewendet werden. | Transkript Zeile 12‑20, 58‑66 |
| FR-5 | Angebot‑Status‑Workflow | Angebote durchlaufen die Stati **Draft** → **Sent**. Keine Freigabe‑Schritte im MVP. | Transkript Zeile 150‑168 |
| FR-6 | Rechnungs‑ und Bestellungs‑Download (PDF) | Kunden können ihre Rechnungen und Bestellungen als PDF herunterladen. PDF muss rechtliche Fußnoten und Hinweis auf Datenschutzhinweise enthalten. | Transkript Zeile 84‑96 |
| FR-7 | API‑Layer (REST) | Das System stellt eine REST‑API für Login, Angebotserstellung und Rechnungsdownload bereit. Authentifizierung erfolgt über Token‑Mechanismus (OAuth‑Ansatz geplant, jedoch nicht final). | Transkript Zeile 140‑152 |
| FR-8 | Backup & Disaster Recovery | Das System nutzt einen Managed Service mit täglichem Backup und Wiederherstellungs‑Option für die Produktionsumgebung. | Transkript Zeile 122‑128 |
| FR-9 | EU‑Only Hosting | Alle Daten werden ausschließlich in Rechenzentren innerhalb der EU gespeichert (DSGVO‑konform). | Transkript Zeile 102‑108 |
| FR-10 | Monitoring & Audit‑Log | Technisches Logging darf keine personenbezogenen Daten enthalten. Ein minimaler Audit‑Log zeichnet auf, *wer* welche *Änderungen* an Angeboten vorgenommen hat. | Transkript Zeile 108‑112, 176‑182 |
| FR-11 | Kontakt‑Formular (Support) | Ein einfaches Kontakt‑Formular ist im Portal verfügbar (keine Ticket‑Datenbank). | Transkript Zeile 130‑144 |
| FR-12 | Internationalisierung (i18n) | Das UI unterstützt mindestens Deutsch und Englisch. | Transkript Zeile 236‑244 |
| FR-13 | Rate‑Limiting & Pagination | Für Rechnungs‑Downloads wird ein Rate‑Limit (z. B. max. 100 Downloads pro Stunde) und Pagination implementiert. | Transkript Zeile 258‑266 |

## 2. Non‑functional Requirements

| ID | Anforderung | Beschreibung | Quelle |
|----|-------------|--------------|--------|
| NFR-1 | Sicherheit – TLS | Alle HTTP‑Verbindungen müssen TLS 1.2 oder höher verwenden. | Transkript Zeile 45‑48 |
| NFR-2 | Authentifizierungs‑Mechanismus | Passwort‑Hashing nach aktuellem Stand (z. B. Argon2). | Transkript Zeile 45‑48 |
| NFR-3 | Datenschutz – DSGVO | Keine personenbezogenen Daten in technischen Logs. Double‑Opt‑In, Lösch‑ und Auskunftskonzept als offene Punkte. | Transkript Zeile 6‑8, 108‑112 |
| NFR-4 | Skalierbarkeit | Das System muss bis zu **20 000** gleichzeitige Nutzer unterstützen (Rate‑Limiting, horizontale Skalierung). | Transkript Zeile 115‑120 |
| NFR-5 | Verfügbarkeit | 99,5 % Uptime im Produktionsbetrieb. SAP‑Verfügbarkeit ist kritische Abhängigkeit. | Transkript Zeile 140‑152 |
| NFR-6 | Performance | Angebots‑Erstellung und Rechnungs‑Download ≤ 3 Sekunden unter Last. | Transkript Zeile 140‑152 |
| NFR-7 | Backup‑Retention | Backup‑Daten mindestens 7 Tage aufbewahren, Wiederherstellung innerhalb 4 Stunden. | Transkript Zeile 122‑128 |
| NFR-8 | Hosting‑Kosten | EU‑Only Managed Service muss kostengünstig sein (Kosten‑Schätzung bis Freitag erforderlich). | Transkript Zeile 140‑146 |
| NFR-9 | Testdaten‑Management | Entwicklungs‑ und Testumgebungen dürfen keine echten Kundendaten enthalten (synthetische oder pseudonymisierte Daten). | Transkript Zeile 250‑258 |
| NFR-10 | Dokumentation | Minimaldokumentation für API, Auth, Backup, und DSGVO‑Maßnahmen. | Transkript Zeile 260‑268 |

## 3. Constraints / Compliance

- **DSGVO‑Konformität** (Double‑Opt‑In, Lösch‑/Auskunftskonzept, EU‑Datenresidenz). – *offen* → muss nach MVP umgesetzt werden.
- **Keine Sonderrabatte** > Standard‑Rabatt im MVP (Freigabe‑Workflow erst Phase 2). – *bewusste Einschränkung*.
- **Kein externes Ticket‑System** im MVP (nur Kontakt‑Formular). – *bewusste Einschränkung*.
- **Kein dediziertes API‑Gateway** (Warteliste 6 Wochen). MVP nutzt ggf. internen Mini‑Gateway mit Rate‑Limiting. – *bewusste Einschränkung*.
- **Keine neue Datenbank** – Managed Service wird für Persistenz verwendet (z. B. Managed PostgreSQL). – *bewusste Einschränkung*.
- **Kein SSO / OAuth im MVP** – optional, aber nicht verpflichtend. – *bewusste Einschränkung*.

## 4. Assumptions and Open Points

| ID | Annahme / Offene Frage | Status |
|----|------------------------|--------|
| A-1 | Kosten für EU‑Only Managed Hosting | Offen – Schätzung bis Freitag notwendig. |
| A-2 | Auswahl des Auth‑Providers (eigener OAuth‑Server vs. externer Provider) | Offen – Entscheidung nach MVP. |
| A-3 | Verfügbarkeit des API‑Gateways | Offen – ggf. interimäre Lösung. |
| A-4 | SAP‑Schreibzugriff (Bestellungs‑Export) | Nicht im MVP, später zu klären. |
| A-5 | Support‑Prozess (Ticket‑System) | Nicht im MVP, wird später definiert. |
| A-6 | Rabatt‑Freigabe‑Workflow (>15 % Rabatt) | Nicht im MVP, später zu implementieren. |
| A-7 | Mehrwährungs‑Support (CHF, USD) | Pilot‑Kunde Schweiz unklar – später. |
| A-8 | Daten‑Retention‑ und Löschregeln (gesetzliche Aufbewahrung) | Offen – muss mit Legal abgestimmt werden. |
| A-9 | Testdaten‑Strategie für SAP‑Testsystem | Offen – Pseudonymisierung erforderlich. |
| A-10 | Backup‑SLA (RPO/RTO) | Offen – muss definiert werden. |
| A-11 | Skalierbarkeit‑Plan (Horizontal vs. Vertikal) | Annahme: horizontale Skalierung über Managed Service. |
| A-12 | Internationalisierung über DACH hinaus (Sprachen, Recht) | Nicht im MVP, später. |

## 5. Traceability Matrix

| Requirement ID | Source (Transkript) |
|----------------|----------------------|
| FR-1 | Zeilen 1‑4 |
| FR-2 | Zeilen 6‑8 |
| FR-3 | Zeilen 27‑33 |
| FR-4 | Zeilen 12‑20, 58‑66 |
| FR-5 | Zeilen 150‑168 |
| FR-6 | Zeilen 84‑96 |
| FR-7 | Zeilen 140‑152 |
| FR-8 | Zeilen 122‑128 |
| FR-9 | Zeilen 102‑108 |
| FR-10 | Zeilen 108‑112, 176‑182 |
| FR-11 | Zeilen 130‑144 |
| FR-12 | Zeilen 236‑244 |
| FR-13 | Zeilen 258‑266 |
| NFR-1 | Zeilen 45‑48 |
| NFR-2 | Zeilen 45‑48 |
| NFR-3 | Zeilen 6‑8, 108‑112 |
| NFR-4 | Zeilen 115‑120 |
| NFR-5 | Zeilen 140‑152 |
| NFR-6 | Zeilen 140‑152 |
| NFR-7 | Zeilen 122‑128 |
| NFR-8 | Zeilen 140‑146 |
| NFR-9 | Zeilen 250‑258 |
| NFR-10 | Zeilen 260‑268 |

---
*Dieses Dokument entstand aus dem MAF‑Workflow‑Kontext (runs/phase2_1/20260529_120535_d8a666/state/context.md) und den in `input/transcripts/T9999_chaos.txt` enthaltenen Stakeholder‑Diskussionen.*
