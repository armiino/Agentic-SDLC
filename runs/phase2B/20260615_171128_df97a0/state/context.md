# Projektkontext – Kundenportal & Angebots‑MVP

## Projektziel
- **Hauptziel**: Schnellere Angebotserstellung für den Vertrieb (Conversion Rate erhöhen) und Bereitstellung eines Kundenportals für Angebots‑ und Rechnungs‑Einblick. Das MVP soll in **8 Wochen** lieferbar sein.
- **Unterziele**: 
  - Login (E‑Mail + Double‑Opt‑In, optional SSO/Azure AD/Google)
  - Rollen‑ und Berechtigungskonzept (Admin, Sales, Kunde, ggf. Manager)
  - Angebots‑Workflow (Draft → Pending Approval → Approved → Sent)
  - Rechnungs‑Download
  - Integration zu SAP (Read‑Only für Produkt‑/Preis‑Daten, später Schreib‑Zugriff)
  - Minimaler Audit‑Trail & Logging (ohne personenbezogene Daten in technischen Logs)
  - EU‑only Managed Hosting, Backup & Disaster‑Recovery
  - DSGVO‑Konformität (Double‑Opt‑In, Lösch‑ und Auftragsverarbeitungs‑Konzept)

## Stakeholder & Rollen
| Name  | Rolle / Verantwortungsbereich |
|------|--------------------------------|
| Anna | Product Owner / Business Lead (Vision, Priorisierung, MVP‑Definition) |
| Ben  | Lead Engineer / Architecture (Technische Umsetzung, API‑Layer, Integration) |
| Clara| Compliance & Data‑Protection (DSGVO, Audit, Logging, Löschkonzept) |
| David| Customer Support (Support‑Prozesse, Kontakt‑Formular) |
| Eva  | Finance (Freigabe‑Workflow für Rabatte, PDF‑Export, rechtliche Vorgaben) |
| Farid| IT Operations (Hosting, EU‑Data‑Residency, Managed Services, Monitoring) |
| (weitere) | ggf. weitere Fachbereiche (Marketing, Sales) |

## Fachliche Themen (aus dem Transkript)
- **Kundenportal** (Web‑first, Mobile optional) 
- **Angebots‑ und Rechnungs‑Management** 
- **Login‑Mechanismen** (E‑Mail + Double‑Opt‑In, optional SSO) 
- **Rollen‑ und Berechtigungskonzept** (Admin, Sales, Manager, Support) 
- **SAP‑Integration** (Lesen von Produkt‑/Preis‑Daten, zukünftiger Schreib‑Zugriff) 
- **DSGVO / Compliance** (Double‑Opt‑In, Lösch‑ & Auftrags‑Verarbeitungs‑Verträge, Audit‑Trails) 
- **Security Review / Verschlüsselung** (TLS, Logging, kein E2E) 
- **Managed Services / Hosting** (EU‑only, Kosten‑Unsicherheit) 
- **Backup & Disaster‑Recovery** 
- **API‑Layer & Gateway** (OAuth bevorzugt, aber komplex) 
- **Mehrsprachigkeit** (Deutsch, Englisch) 
- **Mehrwährung** (EUR, CHF, später USD) 
- **KPI‑Messung** (Conversion‑Rate, Time‑to‑Quote) 
- **Push‑Notifications / Tracking** (DSGVO‑Konformitäts‑Frage) 
- **PDF‑Export & Dokumenten‑Versionierung** 
- **Audit‑Log / Event‑History** (Wer hat was geändert) 
- **Retention‑ und Lösch‑Regeln** (Handelsrecht vs. Recht‑auf‑Löschung) 
- **Support‑Prozess** (Kontakt‑Formular vs. Ticket‑System) 
- **Umgebungen** (Dev/Test/Prod, Test‑Daten‑Pseudonymisierung) 
- **Rate‑Limiting & Missbrauchserkennung** (Download‑Limits, API‑Rate‑Limits) 
- **Caching** (Produkt‑ vs. Kundendaten‑Cache) 
- **Verfügbarkeit von SAP** (kritische Abhängigkeit) 

## Offene Fragen / Unsicherheiten
- **Mobile‑First vs. Web‑First** – welche Priorität bekommt das mobile Frontend im MVP?
- **SSO / Identity Provider** – Azure AD, Google oder beides? Aufwand vs. Nutzen?
- **API‑Gateway‑Warteliste** – 6 Wochen Verzögerung, soll das MVP warten oder ein interimistisches Gateway verwenden?
- **Kosten für EU‑only Hosting** – wie hoch sind die Aufwände im Vergleich zu Standard‑Hosting?
- **Pilot‑Kunde** – Schweiz (Müller AG) oder Deutschland (Hansa GmbH)? Einfluss auf Währung, Datenschutz (Schweiz ≠ EU).
- **Rabatt‑Freigabe** – Schwellenwert (15 % / 20 % / 30 %); wer darf welche Rabatte sehen und freigeben?
- **Support‑Integration** – Kontakt‑Formular ohne Persistenz vs. Ticket‑System (Datenschutz, Aufwand).
- **Backup‑ und DR‑Strategie** – gewünschte RPO/RTO, Kosten und Verantwortlichkeit.
- **Logging vs. Audit‑Logging** – Trennung, Aufbewahrungsfristen, keine personenbezogenen Daten in technischen Logs.
- **Test‑Daten in SAP** – echte Kundendaten vs. synthetische Daten, rechtliche Implikationen.
- **Rate‑Limiting & Missbrauch** – konkrete Schwellenwerte für Download‑ und API‑Nutzung.
- **Caching‑Strategie** – welche Daten dürfen gecacht werden ohne DSGVO‑Risiko?
- **Mehrsprachigkeit & Internationalisierung** – außer DE/EN (z. B. FR, IT) später?
- **Mehrwährung** – CHF‑Support jetzt oder erst später? Vertrags‑ und Steuer‑Implikationen.
- **PDF‑Template‑Management** – Versionierung, rechtliche Fußnoten, Revisionssicherheit.

## Konflikte / Risiken (kritisch für Scope‑Entscheidung)
- **Zeit vs. Umfang**: 8‑Wochen‑MVP vs. umfangreiche Anforderungen (SSO, API‑Gateway, Multi‑Währung, Backup, Security Review).
- **Security Review (6 Wochen) vs. MVP‑Deadline** – mögliche Auslieferung ohne vollständiges Review.
- **Budget für neue DB / Managed Service** – „keine neue DB“ vs. Bedarf an skalierbarem Backend.
- **SAP‑Verfügbarkeit** – kritische Abhängigkeit, kein Fallback definiert.
- **Compliance vs. Geschwindigkeit** – Double‑Opt‑In, Löschkonzept, Audit‑Trail nötig, aber erhöhen Aufwand.
- **Support‑Prozess** – fehlendes Ticket‑System erschwert Kundenservice und erzeugt Datenschutz‑Risiko.
- **Rabatt‑Freigabe** – ohne klaren Prozess können falsche Angebote entstehen (Finanz‑Risiko).
- **Hosting‑Kosten EU‑only** – Unsicherheit über Kosten kann Budget‑Planung blockieren.
- **API‑Gateway‑Verzögerung** – kann Integration und Rate‑Limiting verhindern.
- **Umgang mit Test‑Daten** – Risiko von Datenschutz‑Verstößen während Entwicklung.

## Quellen
- Transkript **T9999_chaos.txt** (Stakeholder‑Dialog vom 2026‑06‑15) – sämtliche genannten Punkte und Zitate stammen aus diesem Dokument.

---
*Diese Kontext‑Zusammenfassung dient als neutrale, aus den Quellen ableitbare Basis für die weitere Anforderungs‑ und Architektur‑Arbeit.*