# Projektkontext – Kundenportal (MVP)

## Projektziel (aus Stakeholder‑Transkripten)
- **Kernziel:** Schnellere Angebotserstellung für das Sales‑Team und Bereitstellung von Rechnungs‑ und Bestellungs‑Einblicken für Kunden (Kundenportal). Das Portal dient als **Mittel**, um das Ziel „Angebote schneller erstellen und konvertieren“ zu erreichen.
- **MVP‑Umfang (definiert von Anna):**
  1. Login mit Double‑Opt‑In (E‑Mail + Passwort) – SSO optional, aber nicht zwingend.
  2. Angebotserstellung über SAP‑Lesedaten (Produkt‑, Preis‑ und Rabattinformationen). Keine manuellen Sonderrabatte – nur Standard‑Rabatte.
  3. Rechnungs‑ und Bestellungs‑Download für Kunden.
  4. Rollen‑Modell (Admin, Sales, Kunde). Minimaler Berechtigungs‑ und Audit‑Trail.
  5. EU‑only Managed Hosting (DSGVO‑konform) ohne neuen Datenbank‑Server.
  6. Backup‑ und Disaster‑Recovery‑Konzept (mindestens für Kundendaten).
  7. Grundlegende API‑Layer (REST, OAuth‑basiert bevorzugt) – Integration zu SAP (lesend) und zum Frontend.
- **Schlüsseleffizienz‑KPIs:** Conversion‑Rate (Angebot → Bestellung), Zeit bis Angebot, Nutzer‑Akzeptanz‑Rate.

## Sprecher‑ und Rollenübersicht
| Sprecher | Rolle / Verantwortungsbereich | Hauptanliegen |
|----------|------------------------------|----------------|
| Anna | Product Owner / Business | Definition des MVP, Priorisierung von Funktionen, Zeitplan (8 Wochen) |
| Ben | Software‑Architekt / Backend | Technische Machbarkeit, API‑Layer, Security, Integration zu SAP, Hosting & Infrastruktur |
| Clara | Datenschutz / Compliance | DSGVO‑Pflichten, Logging, Lösch‑ und Aufbewahrungskonzepte, Audit‑Trail |
| David | Customer Support | Support‑Prozess, Kontakt‑Formular, Daten‑Löschung, Impact auf Kundenservice |
| Eva | Finance / Controlling | Rabatt‑Freigabe, Finanz‑Risiken, Mehrwährung, PDF‑Export & rechtliche Vorgaben |
| Farid | IT Operations / Infrastruktur | EU‑Only Hosting, Managed Services, Monitoring, Secrets Management |
| (weitere) | – | – |

## Fachliche Themen & Anforderungen
- **Kundenportal‑Funktionen**: Login, Rollen‑basiertes UI, Anzeige/Download von Rechnungen, Angebotserstellung, Double‑Opt‑In, optional SSO (Azure AD / Google).
- **SAP‑Integration**: Nur Leserechte für Produkt‑ und Preis‑Daten, ggf. später Schreibrechte für Auftrags‑ und Bestellungs‑Export.
- **Rabatt‑ und Freigabe‑Logik**: Im MVP keine Sonderrabatte > Standard‑Rabatt; Freigabe‑Workflow (Ab‑15 % bzw. 20 % je nach Entscheidung) wird als zukünftiger Umfang markiert.
- **Mehrwährung & Internationalisierung**: MVP: EUR (Deutsch) + optional CHF (Schweiz) für Pilotkunde; Sprachen DE/EN.
- **Compliance & Sicherheit**:
  - DSGVO‑Konformität (Double‑Opt‑In, Lösch‑ und Aufbewahrungsregeln, Datenminimierung).
  - Auditing (Wer hat welches Angebot geöffnet/geändert) – minimaler Audit‑Trail.
  - TLS für Transport, keine End‑to‑End‑Verschlüsselung im MVP.
  - Keine personenbezogenen Daten in technischen Logs, getrennte Audit‑ und Application‑Logs.
  - Rate‑Limiting & Grund‑Security (OAuth‑basiert, ggf. API‑Key als Fallback).
- **Backup & Disaster Recovery**: Mindestens tägliche Backups, Aufbewahrung gemäß EU‑Richtlinien, Wiederherstellungs‑SLAs‑Definition.
- **Monitoring**: Zentralisiertes Monitoring ohne personenbezogene Daten; Secrets‑Management erforderlich.
- **Umgebungen**: Dev / Test / Prod, Testdaten pseudonymisiert oder synthetisch – kein Einsatz echter Kundendaten.
- **Support‑Prozess**: Kontaktformular (keine vollwertige Ticket‑Lösung im MVP) – bewusst eingeschränkter Support.
- **Dokumentation & Architektur‑Entscheidungen**: Klar dokumentierte bewusste Einschränkungen, offene Fragen und Risiken werden explizit gelistet.

## Identifizierte Konflikte, Unsicherheiten & Offene Fragen
| Thema | Konflikt / Unsicherheit | Bewusste Einschränkung / Risikomarkierung |
|-------|--------------------------|--------------------------------------------|
| Mobile Frontend vs. Web‑First | Unterschiedliche Erwartungen von Sales (mobile) vs. MVP‑Plan (Web) | Mobile wird auf später verschoben (Phase 2). |
| SSO / Identity Provider | Azure AD / Google AD vs. fehlendes zentrales IAM | SSO optional, nicht im MVP. |
| Backend‑Readiness | Backend ist nicht API‑ready | API‑Layer wird im MVP gebaut (lesend). |
| Datenschutz vs. Time‑to‑Market | Security Review (6 Wochen) > MVP‑Frist (8 Wochen) | Minimal‑Security (TLS, Double‑Opt‑In) wird umgesetzt; Voll‑Security Review als Risiko gekennzeichnet. |
| Hosting‑Region | EU‑only gefordert, aber teurer | EU‑only Managed Service wird verwendet, Kosten‑Schätzung offen. |
| API‑Gateway Warteliste (6 Wochen) | Konflikt mit 8‑Wochen‑MVP | Direktes Hosting des API‑Layers ohne zentrales Gateway (Risiko). |
| Rabatt‑Freigabe | Finanz‑Risiko bei Sonderrabatten | Keine Sonderrabatte im MVP; Freigabe‑Workflow als zukünftiger Scope. |
| Support‑Ticket‑System | Bedarf an strukturiertem Support vs. MVP‑Ausklammerung | Kontakt‑Formular ohne Persistenz; Risiko für Support‑Effizienz. |
| Daten‑Löschung & Aufbewahrung | Recht auf Löschung vs. gesetzliche Aufbewahrungspflichten | Minimaler Retention‑Plan definiert, detaillierte Regeln offen. |
| Mehrwährung (CHF, USD) | Pilotkunde Schweiz vs. MVP‑Budget | CHF optional nur wenn Pilotkunde bestätigt; sonst EUR‑Only. |
| SAP‑Verfügbarkeit | Kritische Abhängigkeit, Wartungsfenster am Wochenende | SLA‑Definition und Fallback‑Strategie (Cache‑Ansatz) als Risiko notiert. |
| Test‑Daten (echt vs. synthetisch) | Nutzung von SAP‑Testsystem mit echten Daten | Pseudonymisierung / synthetische Daten obligatorisch – Risiko bei Fehlinterpretation. |
| Backup & DR | Aufwand vs. MVP‑Zeitplan | Grund‑Backup umgesetzt, detaillierte DR‑Planung später. |
| Internationalisierung | USA‑Datenschutz, weitere Sprachen | DACH‑Start‑Scope, Internationalisierung später. |
| PDF‑Template‑Management | Rechtskonforme PDFs und Versionierung | Minimal‑Template für MVP, Versionierung später. |
| Rate‑Limiting & Missbrauchserkennung | Notwendig für Sicherheit, Aufwand | Basis‑Rate‑Limiting implementiert (Gateway‑Team später). |

## Quellenhinweise
- Alle Angaben stammen ausschließlich aus dem Transkript **input/transcripts/T9999_chaos.txt** (Zeilen‑Auszug im Log). Die einzelnen Statements der Sprecher wurden paraphrasiert, jedoch nicht verändert.
- Die Strukturierung des Kontextes folgt den im Dialog genannten Prioritäten und offenen Punkten.

## Zusammenfassung
Der aktuelle Projektkontext ist stark fragmentiert: Es gibt ein klares **MVP‑Ziel** (schnelle Angebotserstellung & Rechnungszugriff), aber zahlreiche **technische, regulatorische und organisatorische** Einschränkungen, die aktiv als **Risiken** markiert und dokumentiert wurden. Die wichtigsten **Entscheidungen für das MVP** sind die Beschränkung auf einen Web‑First‑Login, SAP‑Lese‑Integration, minimale Rollen‑ und Audit‑Funktionen sowie ein EU‑konformes Managed‑Hosting‑Setup. Alle offenen Fragen (z. B. SSO, Sonderrabatte, Support‑Ticket‑System, vollständige Security‑Review) werden bewusst außerhalb des 8‑Wochen‑Scope gelassen und müssen in späteren Phasen adressiert werden.

---
*Dieses Artefakt dient als neutrale Kontext‑Zusammenfassung für nachfolgende Requirement‑ und Design‑Arbeiten.*