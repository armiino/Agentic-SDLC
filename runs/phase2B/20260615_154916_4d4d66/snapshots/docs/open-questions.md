# Offene Fragen und Klärungsbedarfe

## 1. Offene fachliche Fragen
- **Rabatt‑Freigabeprozess**: Welche Schwelle gilt (15 %, 20 % oder 30 %) und welche Rollen dürfen Freigaben erteilen? *(Quelle: docs/requirements.md – A2, docs/risks.md – Unklare Rabatt‑Freigabeprozesse)*
- **Pilot‑Kunde & Länderwahl**: Wird der Pilot in Deutschland oder der Schweiz durchgeführt? Welche Auswirkungen hat dies auf Währung, Datenschutz und Hosting? *(Quelle: Projektkontext – 5, A1)*
- **Support‑Workflow**: Reicht ein einfaches Kontakt‑Formular oder ist ein Ticket‑System (z. B. Jira Service Management) erforderlich? Welche Berechtigungen benötigen Support‑Mitarbeiter? *(Quelle: Projektkontext – 5, A4)*
- **Multi‑Währungs‑ und Internationalisierungs‑Unterstützung**: Welche Währungen (EUR, CHF, ggf. USD) und Sprachen (Deutsch/Englisch) sollen im MVP unterstützt werden? *(Quelle: Projektkontext – 7, A7)*
- **Retention‑ und Lösch‑Regeln**: Wie lange müssen Angebote, Rechnungen und Log‑Daten aufbewahrt werden? Wie wird das Recht auf Vergessenwerden umgesetzt? *(Quelle: docs/risks.md – Retention‑ und Lösch‑Regeln, A5)*

## 2. Offene technische Fragen
- **API‑Gateway‑Strategie**: Wird ein leichtes API‑Proxy / Service‑Mesh als Übergangslösung eingesetzt, oder bleibt die direkte SAP‑Anbindung bestehen? *(Quelle: docs/architecture.md – Offene Entscheidung 1, C2)*
- **Authentifizierungs‑Mechanismus**: OAuth2 mit Authorization‑Code‑Flow vs. einfacheres JWT‑basiertes Session‑Management? *(Quelle: docs/architecture.md – Offene Entscheidung 2)*
- **Hosting‑Kosten**: Welche konkrete Kostenschätzung für die EU‑only Managed‑Hosting‑Umgebung liegt vor? Wer muss die Freigabe erteilen? *(Quelle: C7, A8)*
- **Backup‑ und Disaster‑Recovery‑Detailgrad**: Welche RPO‑ und Aufbewahrungsfristen gelten für unterschiedliche Datentypen (Datenbank, Logs, PDFs)? *(Quelle: docs/requirements.md – A3, docs/architecture.md – Offene Entscheidung 7)*
- **Rate‑Limiting‑Parameter**: Welche genauen Schwellenwerte, Burst‑Handling‑Strategien und Strafmechanismen sollen implementiert werden? *(Quelle: NFR5, A6)*
- **PDF‑Generierungstechnologie**: wkhtmltopdf, Puppeteer oder ein Cloud‑Service? Welche Anforderungen an Layout und Signatur bestehen? *(Quelle: docs/architecture.md – Offene Entscheidung 8)*
- **Caching‑Strategie für SAP‑Daten**: In‑Memory (Redis) vs. API‑seitiges Caching? Eviktionspolicy? *(Quelle: docs/architecture.md – Offene Entscheidung 9)*

## 3. Widersprüche, die geklärt werden müssen
- **Security Review (6 Wochen) vs. MVP‑Deadline (8 Wochen)**: Kann das MVP ohne vollständige Security‑Review veröffentlicht werden, oder muss ein temporärer Review‑Plan definiert werden? *(Quelle: Risiken – Security vs. MVP)*
- **Datenminimierung vs. direkter SAP‑Zugriff**: Der direkte Lesezugriff auf SAP kann mehr Daten exponieren als nötig – wie lässt sich das Prinzip der Datenminimierung gewährleisten? *(Quelle: Risiken – Datenminimierung vs. SAP‑Zugriff)*
- **Kosten‑Unsicherheit EU‑Only Hosting vs. Budget**: Fehlende Kostenschätzung könnte zu Budget‑Überschreitungen führen – wie wird das Budget freigegeben? *(Quelle: Risiken – Kosten‑Unsicherheit)*
- **Rollen‑ und Berechtigungskonflikt Support vs. Rabatt‑Details**: Support‑Mitarbeiter benötigen Zugriff auf Angebotsdaten, dürfen aber keine Rabatt‑Details sehen – wie wird die feingranulare Berechtigung technisch umgesetzt? *(Quelle: Konflikte – Rollen‑ und Berechtigungskonflikt)*

## 4. Fehlende Informationen
- **Genauer technische Stack für Frontend** (React vs. Angular) und damit verbundene Bibliotheken. *(Quelle: docs/architecture.md – Offene Entscheidung 0)*
- **Details zur Monitoring‑ und Alerting‑Konfiguration** (Metriken, Schwellenwerte, Integration in bestehende IT‑Ops). *(Quelle: docs/architecture.md – Offene Entscheidung 5)*
- **Prozess für Test‑Daten aus SAP**: Nutzung von pseudonymisierten oder synthetischen Daten? Wie wird die Datenqualität sichergestellt? *(Quelle: A9)*
- **Konkrete SLA‑Metriken für SAP‑Connector** (z. B. maximale Antwortzeit, Fehlertoleranz). *(Quelle: NFR4 – SAP‑Datenabruf ≤ 3 s)*

## 5. Mögliche Ansprechpartner / Rollen
| Thema | Ansprechpartner (Stakeholder) |
|-------|-------------------------------|
| Projektziele & Priorisierung | **Anna** – Product Owner / Business‑Stakeholder |
| Technische Architektur & Backend | **Ben** – Technical Lead / Architekt |
| Datenschutz & Compliance | **Clara** – Datenschutz & Compliance |
| Support‑Prozess & Kundenservice | **David** – Customer Support |
| Finanz‑ und Rabatt‑Freigabe | **Eva** – Finance |
| Hosting & Infrastruktur | **Farid** – IT Operations |
| Fragen zu Rollen & Berechtigungen | **Ben** (technisch) & **Eva** (geschäftlich) |
| Kosten‑ und Budget‑Freigabe | **Farid** & **Anna** |

**Hinweis:** Alle Fragen leiten sich direkt aus den bereitgestellten Artefakten (Projektkontext, docs/requirements.md, docs/risks.md, docs/architecture.md).