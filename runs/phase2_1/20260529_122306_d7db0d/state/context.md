# Projektkontext – Kundenportal (MVP)

## Projektziel (aus den Statements)
- **Hauptziel:** Schnellere Angebotserstellung für den Vertrieb und Bereitstellung von Rechnungs‑ und Bestellübersicht für Kunden. Das Kundenportal ist das zentrale Mittel, um dieses Ziel zu erreichen.
- **MVP‑Umfang:** Login (Double‑Opt‑In), Angebots‑Erstellung (nur Standard‑Rabatte, keine Sonderrabatte), Rechnungs‑Download, Rollen‑Modell (Admin, Sales, Kunde), minimaler Audit‑Trail, EU‑Managed‑Hosting, Backup.
- **Zeitplan:** MVP in **8 Wochen**.

## Sprecher & Rollen
- **Anna** – Produkt‑Owner / Business‑Stakeholder (Kundenportal, Priorisierung, MVP‑Definition)
- **Ben** – Technischer Lead / Architektur (API‑Layer, Integration, Security, SAP‑Anbindung)
- **Clara** – Datenschutz / Compliance (DSGVO, Logging, Löschkonzept, Auditability)
- **David** – Customer Support (Support‑Prozess, Kontaktformular, Datenlöschung)
- **Eva** – Finance (Rabatt‑Freigabe, PDF‑Export, rechtliche Vorgaben)
- **Farid** – IT Operations (Hosting, EU‑Datenresidenz, Monitoring, Backup)
- **Weitere:** (Namen nur genannt, keine eigenen Statements)

## Fachliche Themen & Anforderungen
| Thema | Beschreibung | Relevante Stakeholder |
|-------|--------------|-----------------------|
| **Login & Auth** | E‑Mail/Passwort, Double‑Opt‑In, optional SSO (Azure AD / Google) – muss aber im MVP optional bleiben. | Anna, Clara |
| **Angebots‑Workflow** | Erstellung aus SAP‑Produkt‑ & Preisdaten, **kein** Sonderrabatt über Standard‑Rate, Status‑Workflow (Draft → Sent). | Anna, Ben, Eva |
| **Rechnungs‑Download** | Kunden können Rechnungen einsehen und als PDF herunterladen. | Anna, Eva |
| **Rollen & Berechtigungen** | Admin, Sales, Kunde (und im Hintergrund Support‑Rolle ohne Zugriff auf Preise). | Anna, Clara, Eva |
| **Audit‑Trail** | Minimaler Log: wer Angebot erstellt/ändert, wer Rechnung herunterlädt. Trennung von techn. Logs und Audit‑Logs. | Clara, Ben |
| **DSGVO / Compliance** | Double‑Opt‑In, Lösch‑ und Auskunftskonzept, keine personenbezogenen Daten in technischen Logs, Datenminimierung (nur notwendige SAP‑Daten). | Clara, David |
| **SAP‑Integration** | Lese‑Zugriff für Produkt‑/Preis‑Daten, ggf. Cache für Produkt‑Stammdaten (keine Kundendaten). Schreib‑Zugriff nur für spätere Phasen. | Ben, Eva |
| **API‑Layer** | REST‑API, OAuth‑Preferred, aber im MVP noch nicht finalisiert. | Ben |
| **Hosting & Infrastruktur** | EU‑only Managed Service, Backup/Disaster‑Recovery, Monitoring ohne personenbezogene Daten. | Farid |
| **Internationalisierung** | Deutsch + Englisch (im MVP), spätere EU/USA‑Erweiterung. | Anna, David |
| **Mehrwährung** | EUR (Standard), CHF als Pilot, USD später – im MVP nur EUR. | Eva |
| **Support‑Prozess** | Kontaktformular (keine vollwertige Ticket‑Lösung), Zuordnung zum Kundenkonto, Risiko‑Hinweis. | David |
| **PDF‑Templates** | Angebots‑PDF mit rechtlichen Fußnoten, Versionsnummer, minimaler Revisions‑Log. | Eva |
| **Rate‑Limiting & Monitoring** | Grund‑Rate‑Limiting über API‑Gateway (falls verfügbar) oder Anwendung, Abuse‑Erkennung als Risiko. | Ben, Clara |
| **Environments** | Dev / Test / Prod, synthetische Testdaten, keine echten Kundendaten im Test. | Ben, Clara |
| **Retention / Löschkonzept** | Minimaler Retention‑Plan (Handelsrechtliche Aufbewahrung, Konflikt mit Löschrecht wird als offenes Risiko vermerkt). | Clara, Eva |

## Offene Fragen / Konflikte (bewusste Einschränkungen im MVP)
- **Sonderrabatte & Freigabe‑Workflow** – nicht im MVP, erst Phase 2 (Eva).
- **Vollwertiges Support‑Ticket‑System** – nur Kontaktformular, Datenlöschung unklar (David, Clara).
- **API‑Gateway** – 6‑Wochen‑Warteliste, daher nicht im 8‑Wochen‑MVP implementiert (Ben, Farid).
- **Mehrwährung & Schweiz‑Support** – CHF nur optional, kein MVP (Eva, Anna).
- **Online‑Angebots‑Akzeptanz** – nicht Bestandteil des MVP (Anna, Ben).
- **Backup & Disaster Recovery** – Grundlegende Backup‑Lösung muss vorhanden sein, detaillierte RTO/RPO wird später definiert (Farid).
- **Caching von SAP‑Daten** – nur Produkt‑Daten (wenn überhaupt), Kundenspezifische Rabatte nicht gecached (Ben, Clara).
- **Umfassende Security Review** – zeitlich nicht machbar, minimaler Audit‑Trail und TLS‑Verschlüsselung (Ben, Clara).
- **Internationalisierung** – nur DE/EN, weitere Sprachen später (Anna, David).
- **Testdaten‑Strategie** – synthetische Daten, kein echtes SAP‑Produktiv‑Dump (Ben, Clara).
- **Secrets Management** – muss existieren, konkrete Lösung wird nach MVP gewählt (Ben, Farid).
- **Monitoring ohne personenbezogene Daten** – definiert, konkrete Umsetzung später (Farid, Clara).

## Quellen (Belegexzerpte)
- **Login & Double‑Opt‑In** – Anna (Zeile 1‑4, 21‑24).
- **SAP‑Daten & Integration** – Ben (Zeile 6‑9, 45‑48).
- **DSGVO‑Pflichten** – Clara (Zeile 13‑20, 52‑58, 124‑131).
- **Rollen & Berechtigungen** – Anna (Zeile 31‑34, 70‑73).
- **Audit‑Log & Logging** – Clara (Zeile 86‑92, 140‑144).
- **Backup / DR** – Ben & Clara (Zeile 96‑100, 127‑130).
- **Hosting EU‑Only** – Farid (Zeile 166‑172).
- **Support‑Prozess** – David (Zeile 106‑112, 190‑196).
- **Rabatt‑Freigabe** – Eva (Zeile 124‑134, 152‑158).
- **Mehrwährung** – Eva (Zeile 188‑192).
- **Internationalisierung** – Anna & David (Zeile 206‑214).
- **API‑Layer / OAuth** – Ben (Zeile 115‑124, 232‑236).
- **Rate‑Limiting** – Ben & Clara (Zeile 238‑246).
- **Retention / Löschkonzept** – Clara (Zeile 138‑144, 224‑230).

## Zusammenfassung
Der Kontext zeigt ein **komplexes Set an Anforderungen** mit vielen **technischen, rechtlichen und organisatorischen Wechselwirkungen**. Für das **8‑Wochen‑MVP** wird ein stark fokussierter Funktionsumfang definiert, während zahlreiche **Risiken und offene Fragen** bewusst als *nicht umgesetzt* markiert werden. Diese bewussten Einschränkungen müssen später in den Projektphasen adressiert werden.
