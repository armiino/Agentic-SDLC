# Offene Fragen und Klärungsbedarfe

## 1. Offene fachliche Fragen

- **Mobile‑Strategie**: Soll das MVP eine rein responsive Web‑Lösung bleiben oder in Zukunft eine native Mobile‑App erhalten? (Quelle: Projektkontext, Risiko "Unklare Mobile‑Strategie")
- **SSO / Identity Provider**: Wie soll die zukünftige Azure AD/Google‑SSO‑Integration technisch vorbereitet werden? Welche Änderungen am Auth‑Flow sind notwendig? (Quelle: Projektkontext, Risiko "SSO / Identity Provider")
- **Rabatt‑Freigabe‑Workflow (>15 % Rabatt)**: Welche konkreten Schritte und Rollen sind im Freigabe‑Prozess zu definieren? Wie wird die Dokumentation und Nachvollziehbarkeit sichergestellt? (Quelle: Risiken, Offene Punkte)
- **Support‑Ticket‑Lösung**: Wird ein strukturiertes Ticket‑System (z. B. Jira Service Management) eingesetzt oder bleibt das E‑Mail‑basierte Verfahren? Wie wird DSGVO‑Konformität bei E‑Mail‑Logs gewährleistet? (Quelle: Risiko "Support‑Ticket‑System")
- **Internationalisierung**: Welche Sprachen, Währungen und länderspezifischen Rechtsvorschriften (z. B. Schweiz vs. EU) sollen nach dem MVP unterstützt werden? (Quelle: Offene Punkte)
- **KPI‑Tracking‑Details**: Welche genauen Metriken (z. B. Conversion‑Rate, Angebotsdurchlaufzeit) werden erfasst und wie erfolgt das Reporting? (Quelle: FR‑8)

## 2. Offene technische Fragen

- **Datenbank / Persistenz‑Technologie**: Welcher Managed Service wird verwendet (PostgreSQL‑as‑a‑Service, Cloud‑SQL, NoSQL etc.) und wie wird das "Keine neue Datenbank"‑Constraint umgesetzt? (Quelle: Architektur, Offene Entscheidung 1)
- **API‑Gateway Auswahl**: Cloud‑Provider‑Gateway oder Open‑Source‑Lösung? Wie wird die Integration mit Auth‑Service und Rate‑Limiting realisiert? (Quelle: Architektur, Offene Entscheidung 2)
- **Rate‑Limiting & Missbrauchserkennung**: Welche konkreten Schwellenwerte (Requests/Minute, IP‑basiert) und Mechanismen (Burst‑Bucket, Token‑Bucket) werden verwendet? (Quelle: Risiken, Rate‑Limiting)
- **Backup & Disaster Recovery Details**: Wie lauten RPO, RTO, Aufbewahrungsfristen für unterschiedliche Datenklassen (Produktivdaten, Audit‑Logs, Buchhaltungsdaten)? Wie werden Test‑Restore‑Prozesse durchgeführt? (Quelle: Risiken, Backup & DR)
- **Audit‑Log‑Integrität**: Welche Technologie (WORM‑Bucket, digitale Signaturen) wird eingesetzt, um Unveränderlichkeit sicherzustellen? (Quelle: Architektur, Audit‑Log‑Service)
- **SAP‑Caching‑Strategie**: Wie groß ist der Cache, welche TTLs gelten, und wie erfolgt der Fallback bei SAP‑Ausfall? (Quelle: Architektur, SAP‑Read‑Adapter)
- **Secrets‑Management**: Wie werden API‑Keys, DB‑Zugriffsdaten und Zertifikate sicher verwaltet (z. B. Vault, Cloud‑KMS)? (Quelle: Non‑functional Requirements, Sicherheit)
- **Monitoring & Logging Trennung**: Welche konkreten Log‑Pipelines (z. B. Elasticsearch für Tech‑Logs, immutable Storage für Audit‑Logs) werden eingesetzt? (Quelle: Architektur, Monitoring & Logging)

## 3. Widersprüche, die geklärt werden müssen

- **Zeitplan vs. Umfang**: 8‑Wochen‑MVP vs. umfangreiche Sicherheits‑Review (6 Wochen) und weitere Features (Rate‑Limiting, Backup‑Details). Wie wird die Priorisierung konkret umgesetzt? (Risiko "Zeitplan vs. Umfang")
- **EU‑Only Hosting vs. Kosten**: EU‑Only Managed Services sind teurer, aber das Budget ist begrenzt. Welche Kompromisse sind zulässig? (Risiko "EU‑Only Hosting Kosten")
- **Keine neue Datenbank vs. Skalierbarkeit**: Verzicht auf eigene DB könnte Skalierbarkeit und Performance beeinträchtigen. Wie wird dies mitigiert? (Risiko "Keine neue Datenbank")
- **DSGVO‑Konformität vs. Aufbewahrungsfristen**: 30‑Tage‑Löschung ist vorgesehen, jedoch gibt es gesetzliche Aufbewahrungspflichten (z. B. Buchhaltungsdaten). Wie wird ein differenziertes Retention‑Modell umgesetzt? (Risiko "Aufbewahrungs‑ und Löschfristen")

## 4. Fehlende Informationen

- **Kosten‑Analyse EU‑Only Managed Service Provider** (verantwortlich: Farid).
- **Detail‑Planung Security Review (Scope, Meilensteine)** (verantwortlich: Ben).
- **Finaler Entscheid über Mobile‑Strategie (Responsive vs. Native)** (verantwortlich: Anna).
- **Finaler Entscheid über SSO‑Integration** (verantwortlich: Ben).
- **Auswahl und Lizenzierung des API‑Gateways** (verantwortlich: Ben).
- **Definition konkreter Rate‑Limiting‑Parameter** (verantwortlich: Ben).
- **Backup‑Strategie inkl. RPO/RTO, Test‑Restore‑Prozesse** (verantwortlich: Farid/IT‑Operations).
- **Audit‑Log‑Storage‑Technologie und Signatur‑Verfahren** (verantwortlich: Clara).
- **Test‑Daten‑Strategie (synthetisch vs. pseudonymisiert)** (verantwortlich: David/Farid).

## 5. Mögliche Ansprechpartner:innen / Rollen

- **Anna (Produkt‑/Business‑Owner)** – Entscheidungen zu Produktumfang, Mobile‑Strategie, Internationalisierung.
- **Ben (Technischer Lead/Architekt)** – Architekturentscheidungen (Datenbank, API‑Gateway, Rate‑Limiting, Security Review).
- **Clara (Datenschutz‑/Compliance‑Expertin)** – DSGVO‑Umsetzung, Audit‑Log‑Integrität, Retention‑Plan.
- **David (Customer‑Support‑Vertreter)** – Support‑Ticket‑Lösung, Test‑Daten‑Strategie.
- **Eva (Finance)** – Rabatt‑Freigabe‑Workflow, Aufbewahrungsfristen für Finanzdaten.
- **Farid (IT‑Operations)** – EU‑Only Hosting, Kostenanalyse, Backup & DR, Infrastruktur.

---
*Dieses Dokument fasst alle offenen Punkte aus den bereitgestellten Artefakten (requirements.md, risks.md, architecture.md) zusammen und dient als Basis für die weitere Klärung im Projekt.*