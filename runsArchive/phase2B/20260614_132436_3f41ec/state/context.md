# Projektkontext – Kundenportal (MVP)

## 1. Projektziel
- **Hauptziel:** Schnellere Angebotserstellung für das Vertriebsteam über ein Kundenportal, mit anschließender Rechnungs‑ und Bestellungsansicht für Kunden.
- **MVP‑Umfang (8 Wochen):**
  - Login per E‑Mail/Passwort (Double‑Opt‑In) + optionales SSO (Azure AD/Google) – nur als zukünftige Erweiterung gekennzeichnet.
  - Angebots‑Erstellung (Daten aus SAP lesend) und PDF‑Export.
  - Rechnungs‑Download für Kunden.
  - Rollenmodell (Admin, Sales, Kunde) – Basisberechtigungen.
  - Minimaler Audit‑Log (Wer hat was geändert/gesehen). 
  - EU‑Only Managed‑Hosting, Backup & Disaster‑Recovery.
  - Grundlegende DSGVO‑Konformität (Logging, Löschkonzept‑Hinweis, Double‑Opt‑In).
- **Ausgeschlossene Funktionen im MVP (bewusste Einschränkungen):**
  - Full‑Featured SSO‑Integration, Mobile‑App, Push‑Notifications, komplexe Rabatt‑Freigabe‑Workflows, umfassendes Support‑Ticket‑System, mehrsprachige UI (nur DE/EN als Platzhalter), Mehr‑Währungs‑Support (nur EUR + CHF‑Hinweis), API‑Gateway (wegen 6‑Wochen‑Warteliste), umfangreiche Monitoring‑ und Rate‑Limiting‑Features.

## 2. Stakeholder‑Rollen & Interessen
| Rolle | Name | Kerninteresse / Verantwortung |
|-------|------|--------------------------------|
| Product Owner | **Anna** | Definiert MVP‑Ziele, priorisiert Features, balanciert Zeitplan vs. Umfang. |
| Entwickler | **Ben** | Technische Umsetzung, API‑Layer, Integration mit SAP, Performance‑ und Sicherheitsaspekte. |
| Datenschutz | **Clara** | DSGVO‑Compliance, Double‑Opt‑In, Logging, Lösch‑ und Auskunftspflichten, Audit‑Trail. |
| Customer Support | **David** | Erwartet Support‑Möglichkeit (Kontaktformular) und Kundenzugriff auf Rechnungen, warnt vor manuellen Excel‑Listen. |
| Finance | **Eva** | Freigabe‑Workflow für Rabatte (> 15 %), PDF‑Rechtshinweise, Finanz‑Risiken bei fehlerhaften Angeboten. |
| IT Operations | **Farid** | Hosting‑Anforderungen (EU‑Only), Managed Services, Infrastruktur‑Policies (API‑Gateway, Secrets‑Management). |

## 3. Fachliche Themen & Anforderungen
- **Portal‑Funktionalität**: Login, Rollen‑basiertes UI, Angebots‑Erstellung, Rechnungs‑Download.
- **Integration**: SAP‑Leseschnittstelle für Produkt‑/Preis‑ und Rabatt‑Daten; keine Schreib‑Schnittstelle im MVP.
- **Sicherheit & Compliance**: TLS, Double‑Opt‑In, Minimal‑Audit‑Log, Backup, EU‑Only Hosting, keine personenbezogenen Daten in technischen Logs.
- **Architektur‑Entscheidungen (offen)**: Managed Service Provider (Kosten / EU‑Compliance), API‑Gateway (warte 6 Wochen), OAuth vs. API‑Keys, Daten‑Residenz, Secrets‑Management.
- **Performance & Skalierbarkeit**: Erwarteter Nutzerzahl‑Spanne 200 – 20 000, Rate‑Limiting (Basis), Pagination für Rechnungs‑Downloads.
- **Reporting / KPIs**: Conversion Rate (Angebot → Bestellung), Time‑to‑Offer; KPI‑Messung erst nach MVP geplant.
- **Internationalisierung**: DE/EN UI, EUR‑Standard‑Währung, optional CHF für Schweiz‑Pilot, keine USA‑Kompatibilität im MVP.

## 4. Konflikte & offene Fragen (mit Hinweis auf Unsicherheit)
- **Zeitplan vs. Sicherheits‑Review**: 8‑Wochen‑MVP kollidiert mit erwarteten 6‑Wochen‑Security‑Review (Clara). *Unsicherheit*: Wie lässt sich ein minimaler Security‑Check sicherstellen?
- **Budget für neue Datenbank**: Ben weist auf fehlendes Budget hin, Farid betont Managed Services als Alternative. *Unsicherheit*: Welche Managed‑DB‑Lösung erfüllt EU‑Only‑Anforderung kostengünstig?
- **API‑Gateway Warteliste**: 6‑Wochen‑Verzögerung (Farid). *Offene Entscheidung*: MVP ohne Gateway oder alternativer leichter Proxy?
- **Rabatt‑Freigabe**: Eva fordert Freigabe‑Workflow (> 15 % Rabatt), im MVP wird dieser **ausgeschlossen**. *Risiko*: Finanz‑Risiko bei unkontrollierten Rabatten.
- **Support‑Prozess**: David fordert Ticket‑System; MVP nur Kontaktformular (unsicher, ob Datenschutz‑konform). 
- **Pilot‑Kunde & Währungen**: Unklar, ob Schweiz‑Pilot (CHF) oder rein DACH (EUR). *Unsicherheit*: Wahl beeinflusst Multi‑Währungs‑ und Datenschutz‑Anforderungen.
- **Hosting‑Kosten EU‑Only**: Farid schlägt EU‑Only Hosting vor, Anna fragt nach Kosten. *Unsicherheit*: Kosten‑Schätzung fehlt für Vorstandspräsentation.
- **Daten‑Retention vs. Löschrecht**: Clara weist auf Konflikt zwischen gesetzlicher Aufbewahrung und Recht auf Löschung hin. *Offene Regelung*: Retention‑Policy muss definiert werden.
- **Testing‑Umgebungen**: Farid fordert getrennte Dev/Test/Prod mit pseudonymisierten Testdaten; Ben weist auf reale Kundendaten im SAP‑Testsystem hin. *Risiko*: Datenschutz‑Verstoß im Test.
- **Backup & Disaster Recovery**: Erforderlich (Clara), aber Aufwand nicht im aktuellen Aufwandspuffer.

## 5. Quellen (Beweis)
- Das komplette Transkript **T9999_chaos.txt** (Input‑Verzeichnis) enthält sämtliche genannten Aussagen von Anna, Ben, Clara, David, Eva, Farid.

---
*Erstellt von Phase 2.1 ContextAgent, Run 20260614_132436_3f41ec.*