# Projektkontext – Kundenportal (MVP)

**Projektziel (aus den Gesprächen)**
- Schnellere Angebotserstellung für Sales (Conversion Rate ↑) und Möglichkeit für Kunden, Bestellungen und Rechnungen im Portal einzusehen und runterzuladen.  
- Das Portal ist das **Mittel**, um das Ziel zu erreichen; kein eigenständiges Endprodukt.

**Kern‑Features des MVP**
1. **Login** – E‑Mail + Passwort, Double‑Opt‑In, TLS‑Verschlüsselung (keine E2E‑Verschlüsselung).  
2. **Angebotserstellung** – SAP‑Lesezugriff für Produkt‑/Preisdaten, Erstellung von Angeboten ohne Sonderrabatte (nur Standard‑ bzw. vordefinierte Rabatte).  
3. **Rechnungs‑/Bestellungs‑Download** – Kunden können eigene Rechnungen einsehen und PDF‑Download nutzen.  
4. **Rollenmodell (minimal)** – Admin, Sales (Angebote erstellen), Kunde (nur Self‑Service).  
5. **Audit‑Trail** – Minimaler Log, wer wann welches Angebot geöffnet/geändert hat; keine personenbezogenen Daten in technische Logs.  
6. **EU‑only Managed Hosting** – DSGVO‑konform, nachweisbare Datenresidenz, Backup/Disaster‑Recovery.  
7. **API‑Layer** – REST‑API (OAuth‑Preferenz, noch nicht final) für Front‑ und Backend‑Integration.  
8. **Backup** – tägliche Backups, Wiederherstellungs‑SLA ≥ 24 h.

**Explizit ausgeschlossene / bewusst eingeschränkte Bereiche (MVP‑Scope)**
- **Mobile‑App** – zunächst Web‑First, responsive; mobile native optional nach MVP.  
- **SSO / Identity‑Provider** – Optional, nicht im MVP implementiert.  
- **Support‑Ticket‑System** – Nur Kontaktformular, keine persistente Ticket‑Datenbank.  
- **Preis‑/Rabatt‑Freigabe‑Workflow** – Keine manuellen Sonderrabatte; Freigabe‑Logik wird erst in Phase 2 definiert.  
- **Mehrwährungs‑Support** – Nur EUR (Deutsch‑sprachiger DACH‑Start).  
- **Internationale Kunden / USA‑Datenschutz** – Nicht Teil des MVP.  
- **API‑Gateway** – Nutzung des Unternehmens‑Gateways ist wegen 6‑Wochen‑Warteliste nicht realisierbar im 8‑Wochen‑Zeitplan; ad‑hoc‑Auth‑Proxies werden später eingesetzt.  
- **Komplexe Analytics / Tracking** – Keine Analytics‑Integration im MVP, lediglich manuelle KPI‑Erfassung (Conversion‑Rate, Zeit bis Angebot).  
- **PDF‑Template‑Versionierung** – Statische Vorlage, keine dynamische Versionierung.  
- **Langfristige Retention‑ und Löschkonzept** – Minimaler Retentionsplan (gesetzliche Aufbewahrung für Rechnungen, keine automatischen Löschungen).  
- **CI/CD‑Umgebungen / Testdaten** – Development/Prod‑Umgebung ohne echte Kundendaten; synthetische Testdaten werden verwendet.

**Identifizierte Konflikte & offene Fragen (müssen nach MVP geklärt werden)**
| Bereich | Konflikt / Unsicherheit | Hinweis für Weiterarbeit |
|---|---|---|
| **SAP‑Integration** | Nur Lese‑Zugriff, keine Schreib‑Rechte für Aufträge | Klärung der kritischen Abhängigkeit und Fallback‑Strategie (Cache vs. Fehlermeldung) |
| **Rollen & Berechtigungen** | Support‑Team vs. Sales‑Rabatt‑Sichtbarkeit | Bewusste Einschränkung im MVP, später detailliertes Rollen‑/Berechtigungskonzept |
| **Datenschutz** | DSGVO‑Konformität vs. technische Log‑Inhalte | Technische Logs ohne personenbezogene Daten, Audit‑Logs getrennt, Double‑Opt‑In implementiert |
| **Hosting‑Kosten** | EU‑only Managed Service teuer vs. Budget | Grobe Kostenschätzung bis Freitag nötig; Entscheidung über Provider offen |
| **API‑Gateway** | 6‑Wochen‑Warteliste vs. 8‑Wochen‑MVP | Temporärer Direkt‑Expose der API, später Migration zum Gateway geplant |
| **Backup / DR** | Umfang & Kosten noch unklar | Minimal‑Backup vereinbart, genauer SLA wird nach MVP definiert |
| **Mehrsprachigkeit** | Englisch + Deutsch später, aber UI‑Texte bereits zweisprachig? | MVP‑UI nur Deutsch, Internationalisierung später |
| **Kunden‑ und Angebots‑Retention** | Rechtliche Aufbewahrung vs. Recht auf Löschung | Minimaler Retentionsplan (Rechnungen 7 Jahre), Ausnahmen für personenbezogene Daten werden später geprüft |
| **Performance / Skalierbarkeit** | Erwartete Nutzerzahl 200 – 20 000 | MVP auf 200‑500 User ausgelegt, Skalierbarkeit muss später berücksichtigt werden |
| **Preis‑/Rabatt‑Logik** | Echtzeit‑Preis aus SAP vs. nächtliche Berechnung | MVP nutzt nur SAP‑Stammdaten, Preis‑Cache ohne kundenspezifische Rabatte |

**Risiken (Kurz‑Übersicht)**
- **Zeitplan** – 8‑Wochen‑MVP stark restriktiert; viele Scope‑Reduktionen nötig.  
- **Technische Abhängigkeiten** – SAP‑Verfügbarkeit, fehlendes API‑Gateway, fehlende SSO‑Lösung.  
- **Compliance** – DSGVO‑Pflichten (Double‑Opt‑In, Logging, Datenresidenz) müssen zwingend umgesetzt werden.  
- **Kosten** – EU‑only Managed Service und ggf. zusätzliche Provider‑Kosten.  
- **Support‑Prozess** – Kein strukturiertes Ticketsystem → manuelle E‑Mail‑Bearbeitung, potenzieller Datenschutz‑Fall.  
- **Datenqualität** – Unvollständige Stammdaten in SAP beeinträchtigen Angebotserstellung.  
- **Future‑Scope** – Mehrwährung, Internationalisierung, Freigabe‑Workflow, SSO, Mobile App, umfangreiche Analytics – müssen nach MVP geplant werden.

**Quellen / Evidenz** – Alle nachstehenden Punkte stammen aus dem Transkript `input/transcripts/T9999_chaos.txt` (Zeilen‑Auszug, Dialog zwischen Anna, Ben, Clara, David, Eva, Farid, etc.).

---
*Dieses Dokument stellt den neutralen Projekt‑ und Risiko‑Kontext dar, der aus den bereitgestellten Stakeholder‑Transkripten extrahiert wurde.*