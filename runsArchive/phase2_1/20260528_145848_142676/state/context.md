# Projektkontext – Kundenportal (MVP)

## Zieldefinition (aus den Transkripten)
- **Hauptziel:** Schnellere Angebotserstellung für den Vertrieb (Conversion Rate: Angebot → Bestellung) und Bereitstellung von Rechnungs‑ und Bestell‑Einblicken für Kunden.
- **MVP‑Umfang:**
  1. **Login** (E‑Mail/Passwort, Double‑Opt‑In, Grund‑SSO‑Optionen – Azure AD / Google, optional).
  2. **Angebotserstellung** – Nutzung von SAP‑Produkt‑ und Preis‑Daten (Lesender Zugriff). Keine manuellen Sonderrabatte im MVP.
  3. **Rechnungs‑Download** für Kunden.
  4. **Rollenmodell (minimal):** Admin, Sales (Angebotserstellung), Kunde (Einblick). 
  5. **Audit‑Trail** – Protokollierung von Angebots‑ und Login‑Aktivitäten (kein technisches Logging mit personenbezogenen Daten).
  6. **EU‑only Managed Hosting** (DSGVO‑konform, nachweisbare Datenresidenz).
  7. **Backup / Disaster Recovery** (Mindest‑Level, um Kundendaten zu schützen).

## Stakeholder & Rollen
- **Anna (Product Owner / Vertrieb):** Fokus auf schnelle Angebote, Kundenportal, KPI‑Messung, Priorisierung von Mobile‑Optionen.
- **Ben (Entwicklung / Architektur):** Technische Machbarkeit, API‑Layer, Integration zu SAP, Security‑Review, OAuth vs. API‑Keys, Backup, Rate‑Limiting.
- **Clara (Compliance / Datenschutz):** DSGVO‑Pflichten (Double‑Opt‑In, Lösch‑/Auskunfts‑Konzept, Audit‑Logs, keine personenbezogenen Daten in technischen Logs, Retention‑Regeln).
- **David (Customer Support):** Bedarf an Support‑Workflow (Kontaktformular, keine vollwertige Ticket‑Lösung im MVP), Datenlösch‑Anforderungen.
- **Eva (Finance):** Freigabe‑Prozess für Rabatte > 15 % (im MVP keine Sonderrabatte), PDF‑Export mit rechtlichen Hinweisen, Mehrwährungs‑Bedarf (EUR, ggf. CHF).
- **Farid (IT Operations):** EU‑Hosting, API‑Gateway‑Warteliste (6 Wochen), Monitoring‑Anforderungen, Secrets‑Management.

## Fachliche Themen & Anforderungen
| Thema | Anforderungen (MVP) | Offene Fragen / Risiken |
|-------|----------------------|--------------------------|
| **Login & Auth** | E‑Mail/Passwort, Double‑Opt‑In, optional SSO (Azure AD/Google) | SSO‑Implementierung optional, Aufwand unbekannt |
| **Angebotserstellung** | SAP‑Lesezugriff für Produkt‑/Preis‑Daten, Angebot‑Draft → *sent* (keine Sonderrabatte) | Preis‑Updates nachts, Cache‑Strategie, SAP‑Verfügbarkeit (kritische Abhängigkeit) |
| **Rechnungs‑Download** | PDF‑Export, DSGVO‑Hinweise | Keine detaillierte Rechtsprüfung im MVP |
| **Rollen & Berechtigungen** | Admin, Sales, Kunde – minimale Zugriffskontrolle | Feine Rollen‑/Berechtigungsmodell (Support, Finance) fehlt |
| **Audit‑Logging** | Log von Angebotserstellung, Änderungen, Logins (keine personenbezogenen Daten) | Aufbewahrungsfrist, Trennung von technischen Logs ↔ audit logs |
| **DSGVO** | Double‑Opt‑In, Löschkonzept (eingeschränkt), Datenminimierung, EU‑Hosting | Vollständiges Lösch‑/Auskunfts‑Verfahren, Retention‑Rules, Rechtsgrundlagen |
| **Backup & DR** | Regelmäßiges Backup, Wiederherstellungs‑Test | Detail‑Plan und SLA noch offen |
| **Hosting** | Managed Service, EU‑only, nachweisbare Datenresidenz | Kosten‑Schätzung, Provider‑Auswahl, Impact auf Budget |
| **API‑Layer** | Grund‑API für Frontend ↔ SAP, OAuth‑Vorzug | API‑Gateway‑Warteliste (6 Wochen) – Konflikt mit 8‑Wochen‑MVP |
| **Monitoring** | Separate Audit‑Log, keine personenbezogenen Daten in System‑Logs | Secrets‑Management, Rate‑Limiting, Missbrauchserkennung (nur Basis) |
| **Mehrwährung** | EUR (Standard), optional CHF für Pilot (Schweiz) | Währungslogik, rechtliche Prüfungen – evtl. später |
| **Internationalisierung** | Deutsch + Englisch UI | Weitere Sprachen & länderspezifische Datenschutz‑Regeln (USA) später |
| **Support‑Workflow** | Kontaktformular, Zuordnung zu Kundenkonto, E‑Mail‑Benachrichtigung | Kein Ticket‑System im MVP → bewusste Einschränkung, Risiko für Support‑Effizienz |
| **Freigabe‑Prozess für Rabatte** | Keine manuellen Sonderrabatte; Standard‑Rabatt‑Logik aus SAP | Freigabe‑Mechanismus (> 15 % Rabatt) wird erst nach MVP definiert |
| **PDF‑Template‑Management** | Grund‑Template für Angebote & Rechnungen, Versionierung minimal | Vollständiges Template‑Management, Rechts‑Fußnoten später |
| **Umgebungen** | Dev / Test / Prod, synthetische Testdaten, keine echten Kundendaten | SAP‑Testsystem enthält reale Daten → Pseudonymisierung nötig |
| **Rate‑Limiting / Download‑Limits** | Basis‑Rate‑Limit über API‑Gateway (wenn verfügbar) | Fehlender Gateway → muss ggf. intern implementiert werden |

## Offene Punkte & bewusste Ausschlüsse (MVP‑Risiken)
- **Special‑Rabatte & Freigabe‑Workflow** – im MVP nicht enthalten; später zu definieren.
- **Vollständiges Support‑Ticket‑System** – wird im MVP nur durch ein Kontaktformular abgebildet (bewusste Einschränkung).
- **API‑Gateway** – 6‑Wochen‑Warteliste; MVP muss ggf. ohne Gateway starten (erhöhtes Risiko für Security & Rate‑Limiting).
- **Mehrwährung & Schweiz‑Spezifisches** – optional, nur wenn Pilotkunde Schweiz ist; sonst später.
- **Internationalisierung über DACH** – nur DE/EN im MVP, weitere Sprachen später.
- **Komplexe Rollen‑ & Berechtigungskonzept** – minimal im MVP, erweiterbar.
- **Retention‑ und Löschkonzept** – Basis‑Konzept, detaillierte Umsetzung nach MVP.
- **Monitoring & Secrets Management** – Grundlegende Implementierung, erweiterte Funktionen nach MVP.
- **Cache‑Strategie für SAP‑Preise** – noch nicht definiert; MVP verzichtet auf Cache.
- **Backup‑Strategie Details** – Grund‑Backup, genaue RPO/RTO später.
- **Testdaten‑Umgebung** – Real‑Daten im SAP‑Testsystem müssen pseudonymisiert werden.

## Quellenangaben (Transkript‑IDs)
- Alle Informationen stammen aus dem Transkript **input/transcripts/T9999_chaos.txt** (Stakeholder‑Dialog zwischen Anna, Ben, Clara, David, Eva, Farid, und weiteren).

## Zusammenfassung
Der aktuelle Projektkontext ist stark von widersprüchlichen Anforderungen und zeitkritischen Abhängigkeiten (SAP‑Verfügbarkeit, API‑Gateway‑Warteliste, 8‑Wochen‑MVP) geprägt. Das definierte MVP fokussiert auf ein funktionales Kundenportal zum **Login**, **Angebotserstellung** (ohne Sonderrabatte) und **Rechnungs‑Download** mit minimaler Rollen‑ und Audit‑Log‑Unterstützung, das **EU‑konform** gehostet wird und ein Basis‑Backup besitzt. Alle nicht‑kritischen oder zu ressourcenintensiven Features (z. B. vollständiger Support‑Ticket‑Workflow, Sonderrabatt‑Freigabe, erweiterte Mehrwährungs‑ und Internationalisierungs‑Optionen) werden bewusst als **Aus‑ und Einschränkungen** gekennzeichnet, wobei die damit verbundenen Risiken dokumentiert sind.

*Dieses Dokument ist als neutrale Kontext‑Zusammenfassung zu verstehen und dient als Grundlage für die weitere Anforderungs‑ und Architektur‑Ausarbeitung.*