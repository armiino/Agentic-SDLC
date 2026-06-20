# Projektkontext – Kundenportal (MVP)

## 1. Projektziel (aus Sicht der Stakeholder)
- **Hauptziel:** Schnellere Angebotserstellung und Rechnungseinsicht für Kunden (Angebote → Bestellung → Rechnung). Das Portal ist ein *Mittel* zur Erreichung dieses Ziels. (vgl. Anna)
- **MVP‑Umfang:** Login (E‑Mail/Passwort, Double‑Opt‑In), Angebotserstellung (ohne Sonderrabatte > Standard), Rechnungsdownload, SAP‑Leseschnittstelle für Produkt‑/Preis‑Daten, minimale Rollen (Admin, Sales, Kunde), Basis‑Audit‑Trail, EU‑Managed‑Hosting, Backup.
- **Zeitplan:** 8 Wochen bis MVP‑Release (kritisch wegen Vorstandspräsentation). (vgl. Anna)

## 2. Sprecherrollen & Verantwortungsbereiche
| Sprecher | Rolle / Zuständigkeitsbereich |
|----------|------------------------------|
| Anna | Product Owner / Business‑Stakeholder, definiert Ziele, Prioritäten und Nutzer‑Stories. |
| Ben | Technical Lead / Architekt, kümmert sich um Backend, API‑Layer, SAP‑Integration, Security, Performance. |
| Clara | Datenschutz & Compliance, überwacht DSGVO‑Anforderungen, Logging, Audit, Löschkonzepte. |
| David | Customer Support, stellt Anforderungen an Support‑Workflow, Ticket‑Handling und Datenlöschung. |
| Eva | Finance, definiert Freigabe‑ und Rabatt‑Workflows, rechtliche Vorgaben für Angebote und Rechnungen. |
| Farid | IT Operations, betreut Hosting‑ und Infrastruktur‑Policy (EU‑only, Managed Services, Monitoring). |
| Weitere (optional) | **Nicht aktiv im Transcript**: weitere Stakeholder wie Sales, Marketing, Legal (implizit erwähnt). |

## 3. Fachliche Themen (Hauptthemen)
1. **Portal‑Funktionalität** – Login, Rollen, Angebots‑Workflow, Rechnungs‑Download, ggf. Push‑Notifications.
2. **SAP‑Integration** – Lesender Zugriff für Produkt‑ und Preisdaten, mögliche Schreib‑Zugriffe (später).
3. **DSGVO / Compliance** – Double‑Opt‑In, Logging, Audit‑Trail, Lösch‑ und Daten‑Minimierungs‑Strategien, Datenresidenz (EU‑only), Aufbewahrungspflichten.
4. **Security & Architecture** – API‑Layer (OAuth vs. API‑Keys), Managed Services, Backup / Disaster Recovery, Monitoring, Secrets Management.
5. **Rollen‑ & Berechtigungskonzept** – Admin, Sales, Kunde, Support (eingeschränkter Zugriff), Freigabeverfahren für Rabatte.
6. **Performance & Skalierbarkeit** – Erwartete Nutzerzahlen (200‑20 000), Rate‑Limiting, Pagination, Cache‑Strategie (kritisch bei SAP‑Ausfall).
7. **Mehrwährung & Internationalisierung** – EUR, CHF, evtl. USD, Sprache (Deutsch/Englisch), EU‑ vs. Schweiz‑Datenschutz.
8. **Reporting & KPIs** – Conversion‑Rate, Zeit bis Angebot, Monitoring‑Metriken (ohne personenbezogene Daten).
9. **Dokumente & Templates** – PDF‑Export, Versionierung, rechtliche Fußnoten, Revisionssicherheit.
10. **Support‑Workflow** – Kontaktformular vs. Ticket‑System, Datenschutz bei Support‑Anfragen.

## 4. Konflikte & Kontroversen (Beobachtete Spannungen)
- **Umfang vs. Zeitplan:** Viele Anforderungen (Security Review, API‑Gateway, Backup, Multi‑Währung) kollidieren mit dem 8‑Wochen‑MVP‑Ziel. (Anna, Ben)
- **Security vs. MVP:** Security Review (6 Wochen) überschneidet sich mit MVP‑Deadline; Diskussion, ob MVP ohne Review gehen darf. (Clara, Ben)
- **Rollen‑ und Berechtigungskonflikt:** Support‑Mitarbeiter benötigen Zugriff auf Angebots‑Daten, dürfen aber keine Rabatt‑Details sehen. (Eva, David)
- **Kosten vs. EU‑only Hosting:** EU‑only Hosting ist teurer; Vorstand verlangt Kostenschätzung, aber keine klare Entscheidung. (Farid, Anna)
- **API‑Gateway Warteliste:** 6‑Wochen‑Wartezeit verhindert Nutzung im MVP, jedoch nötig für zentralen API‑Access. (Farid, Ben)
- **Daten‑Minimierung vs. SAP‑Lesen:** Direkter SAP‑Zugriff erhöht Datenexposition, widerspricht Daten‑Minimierungs‑Prinzipien. (Clara, Ben)
- **Feature‑Priorisierung:** Diskussion, welche Funktionen (Push‑Notifications, Support‑Tickets, Rabatt‑Freigabe) im MVP enthalten sein sollen. (Alle)

## 5. Unsicherheiten & Offene Fragen (zu klärende Punkte)
- **Pilot‑Kunde & Länderwahl:** Deutschland vs. Schweiz (Einfluss auf Währung, Datenschutz, Hosting). (Anna, Eva, David)
- **Rabatt‑Freigabeprozess:** Schwelle für Freigaben (15 % vs. 20 % vs. 30 %), Rollen‑Zuweisung und Sichtbarkeit. (Eva, Ben, Clara)
- **Backup‑ und Disaster‑Recovery‑Strategie:** Wie detailliert muss sie für MVP sein? (Ben, Clara)
- **Support‑Prozess:** Ob ein Kontakt‑Formular ausreichend ist oder ein Ticket‑System benötigt wird. (David, Clara)
- **Retention‑ und Lösch‑Regeln:** Wie lange Angebote, Rechnungen und Log‑Daten aufzubewahren vs. Recht auf Vergessenwerden. (Clara, Eva)
- **API‑Gateway Verfügbarkeit:** Alternative Lösung für MVP, falls das zentrale Gateway nicht rechtzeitig verfügbar ist. (Farid, Ben)
- **Kosten‑Schätzung für EU‑only Managed Services:** Noch nicht quantifiziert. (Farid, Anna)
- **Test‑Umgebungen & Daten:** Wie gehen wir mit echten SAP‑Testdaten um (Pseudonymisierung, synthetische Daten). (Clara, Ben)
- **Rate‑Limiting & Missbrauchserkennung:** Wie robust muss das im MVP sein? (Ben, Clara)

## 6. Quellenangaben (Beweismaterial)
- Transkript **T9999_chaos.txt** (vollständiger Dialog aller oben genannten Stakeholder). (Siehe `input/transcripts/T9999_chaos.txt`)

---
*Hinweis:* Alle dargestellten Punkte leiten sich direkt aus den Aussagen im oben genannten Transkript ab. Unsicherheiten und Konflikte wurden bewusst beibehalten, um die aktuelle Projektlage neutral wiederzugeben.

---
**Intent:** Bereitstellung einer neutralen, strukturierten Kontext‑Zusammenfassung für nachfolgende Phasen.
**Reason:** Der Kontext‑Artefakt ist erforderlich, um Projektziele, Rollen, Themen, Konflikte und offene Fragen klar zu dokumentieren.
**Evidence:** Direktes Zitat und Paraphrasierung aus `input/transcripts/T9999_chaos.txt`.
