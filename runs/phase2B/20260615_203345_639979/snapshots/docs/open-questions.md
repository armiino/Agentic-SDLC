# Offene Fragen und Klärungsbedarfe

## 1. Offene fachliche Fragen

- **Frontend‑Strategie (Web‑first vs. Mobile‑first)**
  - *Quelle:* Projektkontext, Risiko „Unklare Frontend‑Strategie“
  - Welche UI‑Plattform hat Priorität für das MVP und welche Ressourcen werden dafür benötigt?
- **Auswahl des SSO‑Providers**
  - *Quelle:* Projektkontext, Requirements „optional SSO“
  - Soll Azure AD, Google oder kein SSO im MVP integriert werden? Welche Lizenz‑ und Kostenimplikationen ergeben sich?
- **Rabatt‑Freigabe‑Workflow Details**
  - *Quelle:* Requirements (Discount‑Freigaben), Risiko „Rabatt‑Freigabe‑Workflow unvollständig“
  - Wie genau soll die Genehmigungskette (Benachrichtigungen, Eskalationen) für >15 % bzw. >30 % Rabatt aussehen? Welche Rollen sind involviert?
- **Zielmarkt‑Definition (DE vs. CH)**
  - *Quelle:* Projektkontext, Risiko „Zielmarkt nicht geklärt“
  - Wird der Pilot zunächst in Deutschland oder in der Schweiz laufen? Wie beeinflusst das Währungen, Steuern und rechtliche Vorgaben?
- **Support‑Prozess Erweiterung**
  - *Quelle:* Risiko „Support‑Prozess nur Kontaktformular“, Projektkontext
  - Wird ein Ticket‑System in späteren Phasen eingeführt? Welche SLA‑Anforderungen gelten?
- **Audit‑Trail – Detailumfang**
  - *Quelle:* Requirements „Audit‑Trail“, Risiko „Trennung technischer Logs nicht garantiert"
  - Welche konkreten Felder (z. B. Nutzer‑ID, Aktion, betroffene Entitäten) müssen im Audit‑Log erfasst werden?

## 2. Offene technische Fragen

- **Verfügbarkeit und Nutzung des API‑Gateways**
  - *Quelle:* Architecture, Risiko „API‑Gateway Verzögerung"
  - Wie wird das Minimal‑API‑Layer implementiert, bis das zentrale Gateway nach ca. 6 Wochen verfügbar ist?
- **Echtzeit‑Preis‑Abruf aus SAP**
  - *Quelle:* Requirements, Risiko „SAP‑Preis‑Synchronisation nur nachts"
  - Ist ein direkter Echtzeit‑Abruf technisch machbar innerhalb der DSGVO‑Vorgaben? Welche Performance‑ und Sicherheitsaspekte gelten?
- **Rate‑Limiting‑Spezifikation**
  - *Quelle:* Risiko „Rate‑Limiting nicht spezifiziert", Projektkontext
  - Welche Schwellenwerte (Requests/min, IP‑basiert vs. Nutzer‑basiert) sollen gelten, insbesondere für Rechnungs‑Downloads?
- **Caching‑Strategie und Datenschutz**
  - *Quelle:* Architecture, Risiko „Caching‑Strategie birgt Datenschutz‑Risiko"
  - Wie wird sichergestellt, dass im Cache keine personenbezogenen Daten gespeichert werden? Welche TTL und Anonymisierungsmethoden sind vorgesehen?
- **Hosting‑Kosten‑Entscheidung**
  - *Quelle:* Risiko „Hosting‑Kosten und EU‑Only Constraint"
  - Welcher Managed‑Hosting‑Provider (EU‑Only) wird gewählt und welches Kosten‑Modell (Pay‑as‑you‑go vs. Festpreis) ist geplant?
- **Integrations‑Testdaten‑Pseudonymisierung**
  - *Quelle:* Architecture, Risiko „Testdaten‑Pseudonymisierung"
  - Wie wird die Pseudonymisierung für SAP‑Testdaten technisch umgesetzt (Tooling, Prozess)?

## 3. Widersprüche, die geklärt werden müssen

- **Zeitplan vs. DSGVO‑Umsetzung** – Das 8‑Wochen‑MVP steht im Konflikt mit einem 6‑Wochen‑Security‑Review. Wie wird die Einhaltung beider Vorgaben gewährleistet?
- **API‑Gateway Verfügbarkeit vs. MVP‑Launch** – Wenn das Gateway erst nach 6 Wochen bereitsteht, riskieren wir einen Launch‑Verzug. Gibt es eine offizielle Fallback‑Strategie?
- **Hosting‑Kosten vs. Budget** – EU‑Only Managed Service ist teurer, das Budget ist aber noch nicht finalisiert. Wie soll das Projektbudget angepasst werden?
- **Frontend‑Strategie vs. Ressourcen** – Ohne klare Entscheidung könnte das Team gleichzeitig für Web‑ und Mobile‑Entwicklung planen, was zu Scope‑Creep führt.

## 4. Fehlende Informationen

- **Endgültige Entscheidung zu SSO** (Provider, Integrationstiefe)
- **Definierte Schwellenwerte für Rate‑Limiting**
- **Detailierte Spezifikation des Audit‑Log‑Schemas**
- **Kosten‑Analyse verschiedener EU‑Only Hosting‑Provider**
- **Klare Zielmarkt‑Priorisierung (DE, CH, später US)**
- **AvV‑Abschlussstatus mit SAP‑ und Hosting‑Provider**
- **Geplante Architektur‑Roadmap für das API‑Gateway**

## 5. Mögliche Ansprechpartner / Rollen

| Frage / Thema | Mögliche Ansprechpartner* |
|---------------|---------------------------|
| Frontend‑Strategie | **Anna** (Product Owner), **Ben** (Technischer Lead) |
| SSO‑Provider Auswahl | **Anna**, **Ben** |
| Rabatt‑Freigabe‑Workflow | **Eva** (Finance), **Ben** (Technik) |
| Zielmarkt‑Definition | **Anna**, **David**, **Eva** |
| Support‑Prozess | **David** (Customer Support) |
| API‑Gateway & Minimal‑API | **Ben** (Architekt), **Farid** (Operations) |
| Echtzeit‑Preis‑Abruf | **Ben**, **Farid** |
| Rate‑Limiting Spezifikation | **David**, **Ben** |
| Caching‑Datenschutz | **Clara** (DSGVO), **Ben** |
| Hosting‑Kosten‑Entscheidung | **Farid**, **Anna** |
| AVV‑Status | **Clara**, **Farid** |
| Testdaten‑Pseudonymisierung | **Farid**, **Ben** |

*Rollen basieren auf den im Projektkontext definierten Sprecher:innen.
