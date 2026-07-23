# Projektkontext – Zusammenfassung

## 1. Projektziel
- **Primäres Ziel:** Schnelle Erstellung und Bereitstellung von Kundenangeboten über ein zentrales Kundenportal (MVP in 8 Wochen). 
- **Unterziele:**
  - Login‑Mechanismus (E‑Mail/Passwort, Double‑Opt‑In, optionale SSO).
  - Anzeige von Rechnungen und Download‑Funktion.
  - Integration mit dem bestehenden SAP‑System (Lesen von Produkt‑ und Preis‑Daten).
  - Minimaler Rollen‑ und Berechtigungskonzept (Admin, Sales, Kunde).
  - Grundlegender Audit‑Trail (Erstellung, Änderung, Freigabe von Angeboten).
  - EU‑only Managed Hosting, Backup/DR, und grundlegende DSGVO‑Konformität.

## 2. Sprecherrollen
| Sprecher | Rolle im Projekt (aus Aussage) |
|----------|--------------------------------|
| **Anna** | Produkt‑/Projekt‑Owner, treibt Business‑Anforderungen (Portal, MVP‑Ziel, KPIs) vor. |
| **Ben** | Technischer Lead/Architekt, fokussiert auf Backend, API‑Layer, Integration, Sicherheit. |
| **Clara** | Datenschutz‑ und Compliance‑Expertin (DSGVO, Logging, Löschkonzept, Audit). |
| **David** | Customer‑Support‑Vertreter (Support‑Prozesse, Tickets, Kontaktformulare). |
| **Eva** | Finanz‑/Controlling‑Vertreterin (Angebots‑Freigabe, Rabatt‑Logik, rechtliche Vorgaben). |
| **Farid** | IT‑Operations (Hosting, Daten‑Residenz, Managed Services, Infrastruktur). |

## 3. Fachliche Themen (aus dem Dialog) 
- **Kundenportal / Plattform** – Web‑first, Mobile‑Option, MVP‑Scope.
- **Login & Identity** – E‑Mail‑Login, Double‑Opt‑In, optionale SSO (Azure AD, Google). 
- **SAP‑Integration** – Lese‑Zugriff (Produkt‑, Preis‑, Kundendaten), später Schreib‑Zugriff für Aufträge.
- **DSGVO / Compliance** – Double‑Opt‑In, Löschkonzept, Audit‑Trails, Daten‑Minimierung, Auftrags‑Verarbeitungs‑Vertrag.
- **Rollen‑ und Berechtigungskonzept** – Admin, Sales, Manager, Support, unterschiedliche Sichtbarkeit von Preisen/Rabatten.
- **Freigabe‑Workflow** – Angebots‑Status (Draft, Pending, Approved …), Rabatt‑Freigabe ab 15 % (Manager) / 30 % (Finance). 
- **KPI‑Messung** – Conversion Rate, Zeit bis Angebot.
- **Backup / Disaster Recovery** – Pflicht aufgrund personenbezogener Daten.
- **Hosting & Datenresidenz** – EU‑only Managed Service, Kosten‑Unsicherheit, Policy‑Konflikt.
- **API‑Layer / Gateway** – OAuth‑Bevorzugt, API‑Keys als Alternative, Warteliste beim zentralen API‑Gateway (6 Wochen).
- **Analytics / Tracking** – Push‑Notifications, Consent, fehlende Analytics‑Infrastruktur.
- **Internationalisierung** – Sprachen (Deutsch/Englisch), Währungen (EUR, CHF, später USD), rechtliche Unterschiede (Schweiz, USA).
- **PDF‑Export & Dokumenten‑Management** – Angebote als rechtssichere PDFs, Templates, Versionierung.
- **Support‑Prozess** – Kontaktformular vs. Ticket‑System, Datenschutz bei Support‑Daten, Zuordnung zu Kundenkonto.
- **Rate‑Limiting, Monitoring & Logging** – Technische Logs ohne PII, Audit‑Logs, Missbrauchserkennung.
- **Entwicklungs‑Umgebungen** – Dev/Test/Prod, Testdaten‑Pseudonymisierung, SAP‑Testsystem‑Problematik.
- **Kosten‑Schätzung** – Vorstandspräsentation bis Freitag, aber fehlende Architektur‑ und Infrastruktur‑Entscheidungen.

## 4. Konflikte & Offene Fragen
| Thema | Konflikt / Unklarheit | Hinweis aus Transkript |
|-------|-----------------------|-----------------------|
| Frontend‑Strategie | Web‑first vs. Mobile‑first, Budget für zwei Frontends | Anna & Ben diskutieren (Zeile 1‑4) |
| SSO & Identity Provider | Azure AD, Google, kein zentrales IAM, Kosten | Anna & Ben (Zeile 9‑15) |
| Backend‑Readiness | API‑Ready? Noch kein API‑Layer | Ben (Zeile 5‑7) |
| DSGVO‑Umsetzung vs. MVP‑Zeitplan | Double‑Opt‑In, Logging, Security Review (6 Wochen) vs. 8‑Wochen‑MVP | Clara & Anna (Zeile 12‑22) |
| Hosting‑Kosten & EU‑Only | EU‑Only erforderlich, aber teurer; Policy‑Konflikt | Farid & Anna (Zeile 112‑124) |
| API‑Gateway Warteliste | 6‑Wochen Verzögerung, kritische Abhängigkeit | Farid & Ben (Zeile 150‑158) |
| Rabatt‑Freigabe | Wer kann >15 % Rabatt geben? Workflow noch unklar | Eva (Zeile 210‑226) |
| Support‑Prozess | Kein Ticket‑System, nur Kontaktformular, aber Kunden benötigen Nachverfolgung | David (Zeile 250‑268) |
| Pilot‑Kunde (DE vs. CH) | Unklare Zielgruppe, beeinflusst Währung, Datenschutz | Anna, David, Eva (Zeile 280‑300) |
| Preis‑Aktualität | SAP‑Preise nächtlich, Echtzeit‑Angebote nötig | Ben (Zeile 320‑328) |
| PDF‑Export & Rechtskonformität | Templates, Versionierung, rechtliche Fußnoten | Eva & Farid (Zeile 340‑360) |
| Monitoring & Logging | Keine PII in technischen Logs, getrennte Audit‑Logs | Clara (Zeile 380‑390) |
| Kosten‑Schätzung | Fehlen klarer Scope‑Entscheidungen, Vorstandspitch bis Freitag | Anna (Zeile 400‑410) |
| Umgebung & Testdaten | SAP‑Testsystem enthält echte Kundendaten – Pseudonymisierung nötig | Farid & Ben (Zeile 420‑430) |
| Skalierbarkeit vs. Over‑Engineering | Erwartete Nutzerzahl von 200 bis 20 000, aber keine Over‑Engineering‑Lösung | Ben & Anna (Zeile 440‑460) |
| Rate‑Limiting / Missbrauch | Gefahr durch massiven Rechnungs‑Download | David & Ben (Zeile 470‑490) |
| Offline‑Fallback bei SAP‑Ausfall | Cache‑Strategie diskutiert, aber Datenschutz‑Risiko | Ben & Clara (Zeile 500‑520) |

## 5. Quellenhinweise
- Das gesamte Transkript **T9999_chaos.txt** (input/transcripts/T9999_chaos.txt) diente als primäre Quelle für diese Zusammenfassung. Alle genannten Punkte leiten sich direkt aus den Dialogen der dort aufgeführten Stakeholder ab.

---
*Erstellt von Phase 2.1 ContextAgent, Run 20260615_203345_639979*