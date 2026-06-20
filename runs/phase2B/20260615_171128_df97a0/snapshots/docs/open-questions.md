# Offene Fragen und Klärungsbedarfe

## 1. Fachliche Fragen

| Frage | Quelle / Auslöser | Mögliche Ansprechpartner* |
|-------|-------------------|---------------------------|
| **Mobile‑First vs. Web‑First** – Welche Priorität hat ein mobiles Frontend im MVP? | Projektkontext & Risks (Widerspruch) | Anna (Product Owner), David (Support) |
| **SSO / Identity Provider** – Soll Azure AD, Google oder beide unterstützt werden? Welche Lizenz‑ und Integrationskosten entstehen? | Requirements & Architecture (optional) | Ben (Lead Engineer), Clara (Compliance) |
| **Pilot‑Kunde Auswahl** – Müller AG (CH) oder Hansa GmbH (DE)? Welche Auswirkungen hat dies auf Währung (CHF) und Datenschutz (Schweiz vs. EU)? | Risiken & Architecture (offene Entscheidung) | Eva (Finance), Farid (IT Operations) |
| **Rabatt‑Freigabe‑Prozess** – Welcher Schwellenwert (15 % / 20 % / 30 %) gilt und wer darf welche Rabatte freigeben? | Risks (unklare Prozesse) | Eva (Finance), Anna (Product Owner) |
| **Support‑Integration** – Soll das Kontaktformular persistente Ticket‑Funktionalität erhalten? Welche Datenschutz‑Anforderungen ergeben sich? | Requirements & Risks (Support‑Integration) | David (Customer Support), Clara (Compliance) |
| **Mehrsprachigkeit** – Sollen nach MVP weitere Sprachen (FR, IT) unterstützt werden? Wie beeinflusst das die UI‑Design‑ und Content‑Strategie? | Architecture (optional) | Anna (Product Owner), Ben (Lead Engineer) |
| **Mehrwährung** – Wird CHF bereits im MVP unterstützt oder erst später? Welche steuer‑ und rechtskonformen Anpassungen sind nötig? | Requirements (CHF) & Risks (Pilot‑Kunde) | Eva (Finance), Farid (IT Operations) |
| **Push‑Notifications / Tracking** – Welche Arten von Benachrichtigungen sind geplant und wie kann die DSGVO‑Konformität sichergestellt werden? | Projektkontext (Tracking) | Clara (Compliance), Ben (Lead Engineer) |

## 2. Technische Fragen

| Frage | Quelle / Auslöser | Mögliche Ansprechpartner* |
|-------|-------------------|---------------------------|
| **API‑Gateway‑Strategie** – Welches interimistische Gateway wird eingesetzt und wie wird die Migration nach 6 Wochen geplant? | Architecture (offene Entscheidung) | Ben (Lead Engineer), Farid (IT Operations) |
| **Rate‑Limiting‑Parameter** – Welche konkreten Schwellenwerte (Requests/Minute, Download‑Limits) sollen gelten? | Risks & Architecture (Rate‑Limiting) | Ben (Lead Engineer), Clara (Compliance) |
| **Caching‑Strategie** – Welche Kundendaten dürfen gecached werden, und welche DSGVO‑Prüfungen sind nötig? | Risks & Architecture (Caching) | Ben (Lead Engineer), Clara (Compliance) |
| **Backup‑ & DR‑Zielwerte** – Welche konkreten RPO/RTO‑Werte sollen umgesetzt werden? Wer ist für das Testing verantwortlich? | Risks & Architecture (Backup) | Farid (IT Operations), Clara (Compliance) |
| **Logging vs. Audit‑Logging** – Wie wird die Trennung technisch umgesetzt und welche Aufbewahrungsfristen gelten? | Risks (Logging) | Clara (Compliance), Ben (Lead Engineer) |
| **Test‑Daten in SAP** – Sollen reale oder synthetische Daten in Test‑Umgebungen verwendet werden? Wie wird Pseudonymisierung gewährleistet? | Risks (Test‑Daten) | Ben (Lead Engineer), Clara (Compliance) |
| **Hosting‑Kostenanalyse** – Wie hoch sind die erwarteten Kosten für EU‑only Managed Hosting im Vergleich zu Standard‑Hosting? | Risks (Kosten) | Farid (IT Operations), Finance (budget) |
| **Datenbank‑Strategie** – Wie wird das „keine neue DB“-Constraint umgesetzt (Shared‑DB, Managed‑Service, Schema‑Erweiterungen)? | Architecture (DB‑Constraint) | Ben (Lead Engineer), Farid (IT Operations) |
| **Interim‑Proxy Umsetzung** – Welche Technologie (Nginx+Lua, Envoy, etc.) wird für das Interim‑Gateway verwendet? | Architecture (Interim‑Gateway) | Ben (Lead Engineer) |

## 3. Widersprüche, die geklärt werden müssen

- **Zeitplan vs. Umfang** – 8‑Wochen‑MVP vs. umfangreiche Optionen (SSO, API‑Gateway, Multi‑Währung, Backup, Security Review). Wie wird der Scope reduziert bzw. priorisiert?
- **Security Review Dauer (6 Wochen)** – Wie kann das Review parallel zur Entwicklung laufen, um das Release‑Datum einzuhalten?
- **Mobile‑First vs. Web‑First** – Konflikt zwischen Stakeholder‑Wünschen und Ressourcen im MVP.
- **SAP‑Abhängigkeit ohne Fallback** – Was ist der Notfall‑Plan bei SAP‑Ausfall?
- **Hosting‑Kosten vs. Budget** – Unklare Kosten könnten zu Kompromissen bei Skalierbarkeit führen.

## 4. Fehlende Informationen

- **Konkrete RPO/RTO‑Ziele** für Backup & Disaster Recovery.
- **Detaillierte Berechtigungs‑Matrix** für Rollen (Admin, Sales, Kunde, Manager, Support).
- **Aufbewahrungsfristen** für Audit‑Logs und technische Logs.
- **Kosten‑Kalkulation** für EU‑only Managed Hosting und ggf. API‑Gateway‑Lizenz.
- **Vertragliche Vorgaben** für Datenverarbeitung mit Schweizer Pilot‑Kunden (z. B. DPA, Auftragsverarbeitung).
- **Finaler Entscheid** zu SSO‑Provider und Anzahl der unterstützten Identity Provider.
- **Definition der Rate‑Limiting‑Schwellenwerte** (z. B. 100 Req/min, Download‑Limit 50 MB/h).
- **Klarheit über PDF‑Template‑Management** (Versionierung, rechtliche Fußnoten).

*Hinweis: Ansprechpartner*innen sind aus den vorhandenen Stakeholder‑Tabellen abgeleitet; konkret kann die Zuordnung im Projekt angepasst werden.*
