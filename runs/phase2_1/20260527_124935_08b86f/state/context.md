# Projektkontext – Kundenportal (MVP)

## Projektziel (aus Stakeholder‑Statements)
- **Kernziel:** Schnellere Angebotserstellung und -versand für das Sales‑Team (Conversion Rate Offer → Order) sowie ein Kunden‑Self‑Service‑Portal für Bestellungen, Rechnungsdownload und Login.
- **MVP‑Scope (explizit vereinbart):**
  1. Login‑Mechanismus (E‑Mail + Passwort, Double‑Opt‑In, TLS‑Verschlüsselung).  
  2. Angebots‑Erstellung basierend auf SAP‑Produkt‑ und Preis­daten (nur Lese‑Zugriff).  
  3. Rechnungs‑ und Bestell‑Anzeige/Download (PDF).  
  4. Rollen‑Modell *Admin, Sales, Kunde* (minimal, ohne komplexe Berechtigungen).  
  5. Minimaler Audit‑Trail (wer hat Angebot erstellt/geändert, Login‑Log).  
  6. EU‑only Managed Hosting (DSGVO‑konform, nachweisbare Datenresidenz).  
  7. Backup/Disaster‑Recovery (Basis‑Backup, keine komplexen RTO/RPO).  
- **Abgegrenzte Features (bewusste Einschränkungen):**
  - Keine native Mobile‑App (erst später, ggf. responsive Web).  
  - Keine SSO‑Integration (Azure AD/Google SSO optional für spätere Phase).  
  - Keine Sonder‑Rabatt‑Freigabe‑Workflows im MVP (Standard‑Rabatt nur).  
  - Kein vollwertiges Ticket‑System (nur simples Kontakt‑Formular).  
  - Keine mehrsprachige Internationalisierung über DE/EN hinaus.
  - Keine vollständige API‑Gateway‑Integration (direkter Service‑Aufruf, weil Gateway‑Warteliste 6 Wochen).  
  - Keine vollständige Preis‑Cache‑Lösung; SAP‑Verfügbarkeit ist kritische Abhängigkeit.

## Sprecherrollen & ihre Anliegen
| Sprecher | Rolle | Hauptanliegen |
|----------|-------|----------------|
| Anna | Product Owner / Business | Kundenportal, schnelle Angebote, MVP‑Zeitrahmen (8 Wochen), Kosten‑Schätzung, Priorisierung. |
| Ben | Lead Engineer / Architekt | Technische Machbarkeit, API‑Layer, Backend‑Readiness, Security‑Review, Integration SAP, Performance, Rate‑Limiting, Backup. |
| Clara | Compliance / Datenschutz | DSGVO‑Pflichten (Double‑Opt‑In, Löschkonzept, Audit‑Logs, keine personenbezogenen Daten in Logs, Datenminimierung, Retention‑ und Löschregeln). |
| David | Customer Support | Support‑Prozess, Ticket‑/Kontakt‑Formular, Zuordnung zu Kundenkonto, Datenschutz bei Support‑Daten. |
| Eva | Finance | Freigabe‑Workflow für Rabatte, PDF‑Templates mit rechtlichen Hinweisen, Mehrwährung (EUR, CHF), rechtliche Aufbewahrungspflichten. |
| Farid | IT Operations | EU‑only Hosting, Managed Services, API‑Gateway‑Warteliste, Secrets‑Management, Monitoring ohne personenbezogene Daten. |
| (Weitere) | – | – |

## Fachliche Themen & Anforderungen
- **Login & Authentifizierung:** E‑Mail/Passwort, Double‑Opt‑In, TLS, optional SSO später.  
- **Angebots‑Workflow:** Draft → Pending‑Approval (nicht im MVP) → Approved → Sent. Im MVP nur Draft/Approved (Standard‑Rabatt).  
- **SAP‑Integration:** Lese‑Zugriff auf Produkt‑, Preis‑ und Kundendaten; kritische Abhängigkeit – System muss offline‑fähig (Fallback = Fehlermeldung).  
- **Rechnungs‑ und Bestell‑Download:** PDF‑Export, rechtlich geprüfte Fußnoten, Versions‑/Audit‑Information.  
- **Rollen & Berechtigungen:** Admin (voll), Sales (Angebote + Kundendaten einsehen), Kunde (eigene Daten, Rechnungen). Minimaler Rollen‑Check.  
- **Audit‑Logging:** Wer hat was geändert, Zeitstempel, getrennt von techn. Logs (keine personenbez. Daten).  
- **Backup & DR:** Basis‑Backup, EU‑Data‑Center, Wiederherstellungs‑Test ≥ 1‑Mal pro Woche.  
- **Compliance:** DSGVO‑Konformität, Double‑Opt‑In, Lösch‑ und Aufbewahrungskonzept (Handelsrecht ≥ X Jahre), Datenminimierung, keine personenbez. Daten in Monitoring‑Logs.  
- **Performance & Skalierbarkeit:** Erwartete User‑Spanne 200 – 20 000, daher Rate‑Limiting, Pagination, Grund‑Caching nur für Produktdaten (keine Kundendaten).  
- **Internationalisierung:** DE/EN im MVP, spätere Unterstützung für CH (CHF) und ggf. US‑Markt.  
- **Kosten‑Schätzung:** EU‑Only Managed Service, Grund‑Hosting + Backup, keine zusätzlichen DB‑Instanz, grobe Schätzung bis Freitag gefordert.  

## Offene Fragen / Risiken (als Klartext gekennzeichnet)
- **SAP‑Verfügbarkeit** – kritische Abhängigkeit, kein Fallback‑Cache für preiskritische Daten.  
- **Sonderrabatte / Freigabe‑Workflow** – im MVP nicht implementiert, Risiko von manuellen Fehlern bei Sales.  
- **Support‑Prozess** – kein Ticket‑System, nur Kontakt‑Formular; Risiko für SLA‑Einhalten und Datenschutz bei E‑Mails.  
- **API‑Gateway** – 6‑Wochen‑Warteliste, daher direkte Service‑Aufrufe im MVP; könnte spätere Integration erschweren.  
- **Hosting‑Kosten EU‑only** – noch nicht quantifiziert, kann das Budget sprengen.  
- **Multi‑Währung / Schweiz** – Pilot‑Kunde aus CH nicht final, Währungs‑Support (CHF) unsicher.  
- **Datenschutz‑Logs vs. technische Logs** – klare Trennung nötig, sonst Compliance‑Verstoß.  
- **Retention & Löschkonzept** – rechtliche Aufbewahrungspflicht vs. Right‑to‑Be‑Forgotten muss definiert werden.  
- **Umgebungen & Testdaten** – SAP‑Testsystem enthält reale Daten → Pseudonymisierung nötig, sonst Datenschutz‑Risiko.  
- **Secrets‑Management & Monitoring** – muss implementiert werden, sonst Sicherheits‑Lücke.  
- **Rate‑Limiting / Missbrauchserkennung** – Grund‑Rate‑Limit vorgesehen, aber kein umfassendes Abuse‑Monitoring.  
- **Kosten‑Schätzung für Vorstand** – fehlt konkrete Zahlen, nur grobe Annahmen.  

## Quellenangaben (Belege aus Transkript)
- **Portal‑ und MVP‑Zeitplan:** Zeilen 1‑4, 31‑34, 157‑162.  
- **Login & Double‑Opt‑In:** Zeilen 1‑4, 9‑12, 84‑90.  
- **SAP‑Integration & Daten:** Zeilen 24‑30, 84‑88, 191‑196.  
- **DSGVO‑Pflichten (Logging, Löschung, Opt‑In):** Zeilen 12‑15, 63‑70, 229‑235.  
- **Rollen‑ und Berechtigungskonflikt:** Zeilen 42‑48, 158‑165, 210‑218.  
- **Support‑Problematik:** Zeilen 140‑147, 177‑184, 247‑254.  
- **Finance‑Freigabe & Rabatt‑Logik:** Zeilen 232‑250, 268‑285.  
- **Hosting‑EU‑Only & Kosten‑Unsicherheit:** Zeilen 286‑304, 311‑322.  
- **API‑Gateway‑Wartezeit:** Zeilen 331‑340.  
- **Backup / DR:** Zeilen 276‑284.  
- **Internationalisierung / Mehrwährung:** Zeilen 354‑368, 378‑390.  
- **PDF‑Export & Templates:** Zeilen 398‑410, 420‑430.  
- **Audit‑Trail & Logging‑Unterschied:** Zeilen 438‑452, 470‑482.  
- **Rate‑Limiting & Monitoring:** Zeilen 492‑506, 514‑528.

---
*Diese Zusammenfassung stellt den aktuell extrahierten Projektkontext dar. Alle genannten Punkte sind direkt aus den jeweiligen Transkript‑Abschnitten ableitbar. Offene Fragen und Risiken sind bewusst hervorgehoben, weil sie im Gespräch nicht abschließend geklärt wurden.*