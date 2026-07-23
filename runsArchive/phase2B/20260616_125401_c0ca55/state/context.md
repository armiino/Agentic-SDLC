# Projektkontext

## Projektziel
- Schnelle Angebotserstellung für Kunden über ein **Kundenportal** (MVP innerhalb von 8 Wochen).
- Möglichkeit für Kunden, ihre Bestellungen und Rechnungen einzusehen und herunterzuladen.
- Grundlegende **DSGVO‑Konformität** (Double‑Opt‑In, Löschkonzept, Audit‑Log, Datenminimierung).
- Minimaler Funktionsumfang für **Login**, **Rollen** (Admin, Sales, Kunde), **Audit‑Trail** und **SAP‑Read‑Integration**.

## Sprecherrollen (aus den Transkripten)
- **Anna** – Produkt‑/Business‑Owner, treibt die Geschäftsziele (Angebote, Portal) voran.
- **Ben** – Technischer Lead/Architekt, fokussiert auf Architektur, APIs, Sicherheit und Integration.
- **Clara** – Datenschutz‑/Compliance‑Expertin, stellt DSGVO‑Anforderungen sicher.
- **David** – Customer‑Support‑Vertreter, sorgt für Support‑Prozesse.
- **Eva** – Finance, definiert Freigabe‑ und Rabatt‑Regeln.
- **Farid** – IT‑Operations, behandelt Hosting, EU‑Only‑Anforderungen und Infrastruktur.

## Fachliche Themen (aus den Transkripten)
- **Kundenportal / Plattform** (Web‑first, optional Mobile, später Internationalisierung)
- **Login‑Mechanismen** (E‑Mail/Passwort, Double‑Opt‑In, optional SSO über Azure/Google)
- **Rollen‑ und Berechtigungskonzept** (Admin, Sales, Manager, Support)
- **DSGVO / Compliance** (Double‑Opt‑In, Löschkonzept, Audit‑Log, Datenresidenz, Einwilligungen)
- **SAP‑Integration** (Lesen von Produkt‑/Preisdaten, Stammdaten, Rabattlogik)
- **API‑Layer / OAuth vs API‑Keys** (Security, Gateway‑Warteliste)
- **Managed Services & EU‑Only Hosting** (Kosten, Datenresidenz)
- **Backup & Disaster Recovery**
- **KPI‑Tracking** (Conversion Rate, Zeit bis Angebot)
- **Push‑Notifications / Tracking / Analytics** (optional, späteres MVP)
- **Mehrwährung & Länder** (EUR, CHF, ggf. USD, EU‑ vs Schweiz‑Datenschutz)
- **PDF‑Export & Vertrags‑Workflow** (Angebots‑Status, Freigabeprozess >15 % Rabatt)
- **Support‑Prozess** (Kontaktformular, Ticket‑Handling, Datenlöschung)
- **Monitoring & Logging** (Technische Logs, Audit‑Logs, Rate‑Limiting)
- **Umgebungen & Testdaten** (Dev/Test/Prod, Pseudonymisierung, Secrets‑Management)
- **Caching & SAP‑Verfügbarkeit** (kritische Abhängigkeit, Fallback‑Strategie)

## Konflikte, Unsicherheiten & Risiken
- **Zeit vs. Umfang**: 8‑Wochen‑MVP vs. umfangreiche Anforderungen (Security Review, API‑Gateway‑Warteliste, Mehrwährung, Internationalisierung).
- **Mobile vs. Web‑First**: Unklare Priorisierung, Einfluss auf Budget.
- **SSO / Identity‑Provider**: Wunsch nach Azure AD/Google, aber fehlendes zentrales IAM und zusätzlicher Aufwand.
- **Budget**: Keine neue DB, Managed Services sollen günstig sein, aber EU‑Only‑Hosting kann teurer werden.
- **Security Review**: 6 Wochen Aufwand kollidiert mit MVP‑Deadline.
- **Support‑Ticket‑System**: Bedarf versus MVP‑Entscheidung, Datenschutz bei E‑Mail‑basiertem Support.
- **Rabatt‑Freigabe**: Bedarf an Workflow für >15 % Rabatt, im MVP bewusst ausgeschlossen.
- **SAP‑Verfügbarkeit**: Kritische Abhängigkeit, fehlende Echtzeit‑Preise, mögliche Caching‑Risiken.
- **Datenschutz‑Log‑ und Monitoring‑Anforderungen**: Trennung von technischen Logs und Audit‑Logs, Aufbewahrungsfristen.
- **Umgebungs‑ und Testdaten**: Nutzung von SAP‑Testsystem mit echten Kundendaten – Risiko für Datenschutz.
- **Rate‑Limiting & Missbrauchserkennung**: Notwendig, aber Umsetzungs‑Aufwand > MVP‑Zeit.
- **Mehrwährung / Länder‑Spezifika** (Schweiz, USA) – rechtliche und technische Komplexität, im MVP bewusst ausgerückt.

## Offene Fragen / Offene Punkte (für weitere Phasen)
- Definitive Entscheidung zu **Mobile**‑Strategie und **SSO**.
- Auswahl und Kostenabschätzung für **EU‑Only Managed Service Provider**.
- Detaillierte **Freigabe‑ und Rabatt‑Workflow**‑Definition.
- **Support‑Ticket‑Lösung** (Strukturiertes System vs. E‑Mail).
- **Internationalisierung** (Sprachen, Währungen, Datenschutz).
- **Backup‑Strategie** und **Disaster‑Recovery**‑Details.
- **API‑Gateway**‑Verfügbarkeit und konkrete **Rate‑Limiting**‑Mechanismus.
- **Retention‑ und Löschkonzepte** vs. gesetzliche Aufbewahrungspflichten.
- **Umgebungskonzept** inkl. **Secrets‑Management** und **synthetische Testdaten**.

## Quellen
- Transkriptdatei: `input/transcripts/T9999_chaos.txt` (gesamter Dialog zwischen Anna, Ben, Clara, David, Eva, Farid).