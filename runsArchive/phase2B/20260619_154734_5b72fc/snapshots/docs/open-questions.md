# Offene Fragen und Klärungsbedarfe

## Fachliche Fragen

| Frage | Quelle | Mögliche Ansprechpartner* | Priorität |
|------|--------|---------------------------|----------|
| Welche Region (Deutschland oder Schweiz) ist der Pilotkunde? Wie wirkt sich das auf Währung (EUR/CHF) und EU‑Only‑Hosting aus? | Kontext, Risks, Architecture | Anna (Product Owner), Farid (IT Operations) | Hoch |
| Welche Schwellenwerte und Rollen sind für die Rabatt‑Freigabelogik erforderlich? (z. B. >15 % Rabatt, Freigabe durch Finance‑ oder Admin‑Rolle) | Requirements, Risks | Eva (Finance), Ben (Architekt) | Hoch |
| Welche konkreten KPIs sollen gemessen werden (Conversion‑Rate, Time‑to‑Offer, etc.)? | Kontext, Requirements | Anna (Product Owner) | Mittel |
| Wie soll das Support‑Kontaktformular technisch umgesetzt werden und welche Daten werden dort gespeichert? | Requirements, Risks | David (Customer Support), Clara (Compliance) | Mittel |
| Welche Aufbewahrungsfristen (Retention‑Policy) gelten für Angebote, Rechnungen und Audit‑Logs? | Risks, Architecture | Clara (Compliance), Farid (IT Operations) | Hoch |
| Müssen für das MVP weitere Sprachen oder Währungen unterstützt werden (Englisch‑UI, CHF‑Preise)? | Kontext, Risks | Anna (Product Owner), Ben (Architekt) | Mittel |

## Technische Fragen

| Frage | Quelle | Mögliche Ansprechpartner* | Priorität |
|------|--------|---------------------------|----------|
| Welche kurzfristige Alternative zum geplanten API‑Gateway wird bis zum MVP‑Release eingesetzt (z. B. Nginx, Traefik)? | Architecture, Risks | Ben (Architekt), Farid (IT Operations) | Hoch |
| Welcher Managed‑DB‑Service (EU‑only) wird letztlich verwendet? Welche Kosten entstehen? | Constraints, Risks | Farid (IT Operations) | Hoch |
| Wie werden Backup‑Parameter finalisiert (RPO = 1 h, RTO = 4 h) und welche SLA‑Verträge werden benötigt? | NFR3, Risks | Farid (IT Operations) | Hoch |
| Wie wird das Rate‑Limiting im Ersatz‑Proxy umgesetzt (IP‑basiert, Nutzer‑basiert)? | NFR4, Architecture | Ben (Architekt) | Mittel |
| Wie wird die Trennung von technischem Monitoring und Audit‑Logs technisch garantiert (keine PII im Monitoring)? | NFR4, Architecture | Clara (Compliance), Ben (Architekt) | Hoch |
| Welche Testdaten‑Strategie (synthetische SAP‑Daten, Maskierung) wird in Dev/Test‑Umgebungen verwendet? | Risks, Architecture | Ben (Architekt), Eva (Finance) | Mittel |
| Wie wird das SSO‑Fallback‑Login ohne Azure AD/Google realisiert? | Requirements | Ben (Architekt) | Niedrig |

## Widersprüche, die geklärt werden müssen

- **Zeitplan vs. Security‑Review**: Das Security‑Review dauert >6 Wochen, kollidiert stark mit der 8‑Wochen‑MVP‑Deadline. Wie kann das Review beschleunigt oder parallelisiert werden? (Clara, Ben)
- **API‑Gateway‑Warteliste vs. MVP‑Start**: Der geplante API‑Gateway ist erst nach 6 Wochen verfügbar – das MVP muss mit einem Ersatz starten, was zusätzlichen Aufwand und mögliche Sicherheitslücken bedeutet. (Ben, Farid)
- **Budget vs. Managed Services**: Kosten für EU‑Only‑Hosting und Security‑Review sind noch nicht geschätzt – Gefahr von Budgetüberschreitung. (Anna, Farid)
- **Keine neue Datenbank vs. Persistenzanforderungen**: Nutzung eines bestehenden Managed‑DB‑Service kann bei steigendem Datenvolumen zu Performance‑Problemen führen. (Ben, Farid)

## Fehlende Informationen

- **Finale Kosten‑Schätzung** für Managed Services, Security‑Review und ggf. zusätzliche Infrastruktur (bis Freitag gefordert). 
- **Detailierte Retention‑Policy** für alle datenschutzrelevanten Artefakte. 
- **Konkretes SLA‑Modell** für Backup‑ und Disaster‑Recovery‑Service. 
- **Definition der KPI‑Metriken** für Erfolgsmessung. 
- **Auswahl und Konfiguration des Ersatz‑API‑Gateways** (Software, Sicherheitskonfiguration). 
- **Implementierungsdetails für das Support‑Kontaktformular** (Speicherort, Löschfristen). 

## Mögliche Ansprechpartner / Rollen

- **Anna** – Product Owner / Business Lead
- **Ben** – Architekt / Entwickler
- **Clara** – Datenschutz / Compliance
- **David** – Customer Support
- **Eva** – Finance
- **Farid** – IT Operations

*Hinweis: Die genannten Ansprechpartner basieren auf den im Kontext angegebenen Rollen und können je nach Frage variieren.*
