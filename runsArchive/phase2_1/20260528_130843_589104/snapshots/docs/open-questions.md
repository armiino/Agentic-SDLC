# Offene Fragen und Klärungsbedarfe

## 1. Fachliche Fragen
- **Zieldefinition Portal vs. Mobile App** – Welche Plattform hat Priorität nach dem MVP? (Quelle: Transkript, Diskussion zwischen Anna und Ben)  
- **Rabatt‑Freigabe‑Prozess** – Ab welchem Rabattprozentsatz ist eine Freigabe notwendig? 15 % oder 20 %? Wer (Manager, Finance) entscheidet? (Quelle: Eva, Ben)  
- **Support‑Prozess** – Wie soll das Support‑Kontaktformular technisch umgesetzt werden, damit Anfragen Zuordnungen zu Kunden ermöglichen, ohne ein vollständiges Ticket‑System? (Quelle: David, Clara)  
- **Mehrwährungs‑Support** – Muss das MVP CHF unterstützen, weil ein Schweizer Pilotkunde (Müller AG) möglich ist, oder reicht EUR? (Quelle: Eva, Farid, Anna)  
- **KPI‑Definition** – Wie werden Conversion‑Rate und „Zeit bis Angebot“ exakt gemessen und gespeichert? (Quelle: Anna)  
- **Rollback‑Strategie bei SAP‑Ausfall** – Welche konkreten Fallback‑Mechanismen (z. B. Cache, statische Preislisten) sind akzeptabel? (Quelle: Ben, Farid)  
- **Aufbewahrungs‑ und Löschfristen** – Wie lange müssen Angebote, Rechnungen und Kundendaten aufbewahrt werden, und welche Ausnahmen gelten für gesetzliche Vorgaben? (Quelle: Clara)  
- **Rollen‑ und Berechtigungskonflikt Support vs. Finance** – Welche Daten darf das Support‑Team sehen (z. B. Angebote ohne Rabatte) und wie wird das technisch umgesetzt? (Quelle: David, Eva)  
- **Reporting‑ und Analyse‑Bedarf** – Welche KPIs sollen nach dem MVP erfasst werden (z. B. Download‑Statistiken, Nutzung‑Metriken) und in welchem Tool? (Quelle: Anna)  

## 2. Technische Fragen
- **API‑Readiness des Backends** – Welche konkrete API‑Spezifikation (Endpoints, Datenmodelle) wird benötigt, um das Frontend zu starten? (Quelle: Ben)  
- **API‑Gateway‑Entscheidung** – Welcher Managed‑API‑Service wird kurzfristig eingesetzt (Eigenständige Instanz vs. zentrales Unternehmens‑Gateway mit 6‑Wochen‑Warteliste)? (Quelle: Farid, Ben)  
- **Managed DB‑Auswahl** – Welches konkrete Datenbank‑Produkt (PostgreSQL, MySQL, etc.) wird als EU‑Only Managed Service verwendet? (Quelle: Anna, Ben)  
- **OAuth‑Implementierung** – Welche OAuth‑Flows (Authorization Code, Client Credentials) sind für das MVP erforderlich, und ist ein Fallback‑API‑Key‑Mechanismus nötig? (Quelle: Ben)  
- **Backup‑ und DR‑Detailierung** – Wie häufig (täglich, hourly) und wie lange (30 Tage, 90 Tage) werden Backups aufbewahrt, und wie wird die Wiederherstellung getestet? (Quelle: Clara)  
- **Monitoring‑ und Logging‑Aufbau** – Wie werden Application‑Logs, Audit‑Logs und Monitoring‑Metriken getrennt und welche Retention‑Policies gelten? (Quelle: Clara, Farid)  
- **Secrets‑Management** – Welcher Service wird zum sicheren Umgang mit Datenbank‑, SAP‑ und E‑Mail‑Credentials genutzt? (Quelle: Farid)  
- **Rate‑Limiting & Missbrauchserkennung** – Welche konkreten Grenzen (Requests/min per User) und welche Mechanismen (IP‑Blocking, CAPTCHA) sollen im MVP implementiert werden? (Quelle: Ben, Clara)  
- **Test‑Daten‑Strategie** – Wie werden synthetische bzw. pseudonymisierte Testdaten erzeugt, um das SAP‑Testsystem ohne echte Kundendaten zu nutzen? (Quelle: Ben, Clara)  
- **Internationalisierung (i18n)** – Welche Lokalisierungs‑Frameworks werden verwendet und wie werden Texte/Labels für DE + EN verwaltet? (Quelle: Anna)  

## 3. Compliance & Datenschutz
- **Double‑Opt‑In‑Workflow** – Welche konkreten Schritte (E‑Mail‑Inhalt, Bestätigungs‑Link, Ablaufzeit) sind zu implementieren, um DSGVO‑Konformität zu gewährleisten? (Quelle: Clara, Anna)  
- **Auftragsverarbeitungsverträge (AVV)** – Welche Provider benötigen AVVs (Managed DB, Hosting, E‑Mail‑Service) und bis wann müssen diese vor MVP‑Launch abgeschlossen sein? (Quelle: Clara)  
- **Datenresidenz für Schweizer Kunden** – Müssen Daten von Schweizer Kunden ausschließlich in der Schweiz gespeichert werden, oder reicht EU‑Only Hosting aus? (Quelle: Farid, Clara)  
- **Pseudonymisierung im Monitoring** – Wie wird sichergestellt, dass Monitoring‑Metriken keinerlei personenbezogene Daten enthalten? (Quelle: Clara, Farid)  
- **Retention‑Policy für Audit‑Logs** – Wie lange müssen Audit‑Logs aufbewahrt werden, um sowohl DSGVO‑ als auch Finanz‑Compliance zu erfüllen? (Quelle: Clara)  
- **Rechtsgrundlage für Datenverarbeitung** – Welche Einwilligungen bzw. Verträge (z. B. Art. 6 Abs. 1 Lit. a DSGVO) liegen für die einzelnen Verarbeitungstätigkeiten vor? (Quelle: Clara)  

## 4. Organisatorische/Projekt‑fragen
- **Kosten‑Schätzung für EU‑Only Managed Hosting** – Welche Preis‑Bandbreite ist zu erwarten, und wer ist für die Freigabe des Budgets verantwortlich? (Quelle: Farid, Anna)  
- **Ressourcen‑Planung für Security Review** – Wie kann ein kompakter Security‑Review innerhalb des 8‑Wochen‑Zeitfensters durchgeführt werden? (Quelle: Ben, Clara)  
- **Entscheidungsträger für Architektur‑Entscheidungen** – Wer (Rolle) trifft die finale Wahl für API‑Gateway, DB‑Produkt und Hosting‑Provider? (Quelle: Kontext)  
- **Zeitplan für AVV‑Abschluss** – Bis wann müssen AVVs mit allen externen Dienstleistern vor dem MVP‑Go‑Live unterzeichnet sein? (Quelle: Clara)  
- **Priorisierung der offenen Risiken** – Welche Risiken (z. B. Security‑Review, Hosting‑Kosten) haben höchste Priorität für das kommende Sprint‑Planning? (Quelle: Risikobericht)  

*Alle aufgeführten Fragen leiten sich eindeutig aus den in `input/transcripts/T9999_chaos.txt`, `runs/phase2_1/20260528_130843_589104/state/context.md`, `docs/requirements.md` und `docs/risks.md` dokumentierten Aussagen ab.*