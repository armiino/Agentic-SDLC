# Projektkontext – Zusammenfassung aus Stakeholder‑Transkripten

## 1. Projektziel (aus Sicht von *Anna*)
- Schnellere Angebotserstellung (Conversion Rate‑Steigerung, Zeit bis Angebot reduzieren) 
- Bereitstellung eines **Kundenportals** für Kunden, um Angebote, Bestellungen und Rechnungen einzusehen und herunterzuladen
- Login‑Mechanismus (E‑Mail + Passwort, Double‑Opt‑In, optionale SSO‑Integration)
- Minimal‑MVP innerhalb **8 Wochen**
- Grundlegende **DSGVO‑Konformität** (Logging, Audit‑Trail, Löschkonzept, Double‑Opt‑In)
- Nutzung von **Managed Services** (keine neue DB) und **EU‑only Hosting**

## 2. Sprecherrollen
| Sprecher | Rolle / Verantwortungsbereich |
|----------|------------------------------|
| **Anna** | Business‑Owner / Product Owner (Anforderungen, Priorisierung) |
| **Ben**  | Technischer Lead / Architektur (Frontend, API‑Layer, Integration) |
| **Clara**| Datenschutz / Compliance (DSGVO, Logging, Audit, Löschkonzept) |
| **David**| Customer Support (Support‑Prozesse, Kontaktformular) |
| **Eva**  | Finance / Controlling (Angebots‑Freigabe, Rabatt‑Limits, PDF‑Export) |
| **Farid**| IT Operations / Infrastruktur (Hosting, Backup, Monitoring) |

## 3. Fachliche Themen (Auszug)
- **Kundenportal** (Web‑first, mobile später, ggf. native App) 
- **Login**: E‑Mail/Passwort, Double‑Opt‑In, optionale SSO (Azure AD, Google) 
- **Rollen & Berechtigungen**: Admin, Sales, Manager, Support, ggf. weitere Rollen 
- **DSGVO**: Double‑Opt‑In, Logging, Audit‑Trail, Löschkonzept, Datenminimierung 
- **SAP‑Integration**: Lesen von Produkt‑/Preisdaten, fehlender API‑Readiness, später Schreib‑Zugriff für Aufträge 
- **Angebots‑Workflow**: Draft → Pending Approval → Approved → Sent … (Freigabe bei > 15 % Rabatt) 
- **Rechnungs‑Download** und **PDF‑Export** (rechtlich konforme Fußnoten) 
- **KPIs**: Conversion‑Rate, Zeit bis Angebot, Nutzung‑Statistiken (Analytics, Tracking) 
- **Mehrwährung**: EUR (Standard), CHF (Schweiz), später USD 
- **Hosting & Infrastruktur**: EU‑only Managed Service, Backup / Disaster Recovery, keine neue DB, API‑Gateway (6‑Wochen‑Warteliste) 
- **Security**: TLS ausreichend, Security‑Review (6 Wochen) vs. MVP‑Zeitplan 
- **Monitoring & Logging**: Trennung von technischen Logs, Audit‑Logs, Aufbewahrungsfristen 
- **Rate‑Limiting / Pagination** für Downloads 
- **Support‑Prozess**: Kontaktformular vs. Ticket‑System, Datenschutz bei Support‑Anfragen 
- **Umgebungen**: Dev / Test / Prod, pseudonymisierte Testdaten, Secrets‑Management 

## 4. Konflikte & offene Fragen
- **Web‑first vs. Mobile‑first** – keine klare Entscheidung, beeinflusst Aufwand und UX. 
- **Native App vs. Responsive Web** – Budget‑ und Zeitdruck. 
- **SSO‑Integration** (Azure AD, Google) vs. MVP‑Umfang. 
- **Security Review** (6 Wochen) kollidiert mit 8‑Wochen‑MVP. 
- **EU‑only Hosting** kostenintensiv vs. Budget‑Vorgaben. 
- **API‑Gateway‑Warteliste** (6 Wochen) vs. gewünschte API‑Layer‑Implementierung. 
- **Rollen‑Konflikt**: Support‑Mitarbeiter sollen Kundendaten einsehen, dürfen aber keine Preis‑/Rabatt‑Details sehen. 
- **Rabatt‑Freigabe**: Schwellenwert (15 % / 20 % / 30 %) und Verantwortlichkeiten (Manager, Finance) unklar. 
- **Pilotkunde**: Schweiz (CHF, Datenschutz) oder Deutschland – Entscheidung beeinflusst Währungs‑ und Rechts‑Richtlinien. 
- **Daten‑Minimierung vs. Performance**: Echtzeit‑SAP‑Abfrage vs. Cache (Datenschutz‑Risiko). 
- **Backup / Disaster Recovery** – muss im MVP enthalten sein, aber Ressourcen‑Umfang unklar. 
- **Support‑Prozess**: Kein Ticket‑System im MVP, aber Support‑Team fordert strukturierte Nachverfolgung. 
- **Analytics / Tracking**: Bedarf für KPIs, aber noch keine Infrastruktur (Consent‑Management). 
- **Test‑Daten**: Nutzung von SAP‑Testsystem mit echten Kundendaten → Datenschutz‑Problem. 
- **Mehrwährung & Internationalisierung** – DACH‑Start, später EU/USA, beeinflusst UI & Backend. 

## 5. Unsicherheiten / Risiken (bewusste Einschränkungen im MVP)
- **Zeitplan**: 8 Wochen‑MVP möglicherweise unrealistisch wegen Security Review, API‑Gateway, EU‑Hosting. 
- **Kosten**: Managed Service & EU‑Only Hosting – noch keine Schätzung. 
- **Scope‑Abgrenzung**: Mobile App, Mehrwährung, Full‑Ticket‑System, automatisierte Rabatt‑Freigabe werden bewusst für spätere Phasen ausgesetzt. 
- **Compliance**: Double‑Opt‑In, Löschkonzept, Audit‑Log werden als minimal implementiert gekennzeichnet, aber Detail‑Umsetzung offen. 
- **SAP‑Verfügbarkeit**: Kritische Abhängigkeit, keine Fallback‑Strategie definiert. 
- **Support‑ und Datenzugriff**: Konflikt zwischen Support‑Bedarf und Datenschutz‑Anforderungen bleibt offen. 

## 6. Quellen (Transkript‑Referenz)
- Gesamtes Transkript `T9999_chaos.txt` (Zeilen 1‑~400) liefert alle oben genannten Aussagen von Anna, Ben, Clara, David, Eva, Farid.
- Konkrete Zitate (z. B. *„Wir brauchen definitiv ein Kundenportal...“*, *„DSGVO… Double‑Opt‑In…“*, *„Security Review dauert 6 Wochen…“*) wurden aus diesem Dokument entnommen.

---
*Dieses Dokument fasst den derzeitigen Projektkontext zusammen, markiert bewusste MVP‑Einschränkungen und listet offene Risiken für die weitere Anforderungs‑ und Architektur‑Entwicklung auf.*