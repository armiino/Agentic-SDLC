# Risiken

## Technische Risiken
- **API‑Layer nicht API‑ready**: Backend ist noch nicht API‑ready, was die Implementierung verzögern kann.
- **Security Review Zeitplan**: Security Review wird mit 6 Wochen veranschlagt und überschneidet sich mit dem 8‑Wochen‑MVP‑Zeitplan.
- **Managed Services Auswahl**: Fehlende Entscheidung für einen Managed‑Service‑Provider kann zu Verzögerungen und Compliance‑Problemen führen.
- **Skalierbarkeit vs. Over‑Engineering**: Unterschiedliche Nutzer‑Erwartungen (200 bis 20 000) können zu Fehlentscheidungen bei Architektur und Ressourcen führen.
- **OAuth‑Implementierung**: OAuth ist komplexer als API‑Keys und könnte den Zeitplan gefährden.

## Compliance‑Risiken
- **DSGVO‑Konformität**: Fehlende Double‑Opt‑In‑Implementierung, unklare Löschkonzepte und fehlende Auftragsverarbeitungsverträge können zu rechtlichen Problemen führen.
- **Daten‑Hosting EU‑Only**: Unsichere Auswahl des Cloud‑Providers könnte zu Daten‑Hosting außerhalb der EU führen.
- **Audit‑Trail**: Unzureichendes Logging kann die Audit‑Pflicht verletzen.

## Projekt‑Risiken
- **Unklare Zieldefinition**: Unterschiedliche Prioritäten (Portal vs. Angebote vs. KPI‑Messung) können zu Scope‑Creep führen.
- **Ressourcen‑Mangel**: Kein Architekt im Team, was zu mangelhafter Dokumentation und Architekturentscheidungen führt.
- **Budget‑Beschränkungen**: Kostengünstige Lösungen könnten die Qualitäts‑ und Sicherheitsanforderungen beeinträchtigen.
- **Stakeholder‑Abstimmung**: Unterschiedliche Erwartungen von Sales, Marketing und Compliance können zu Konflikten führen.

## Risikominderungs‑Maßnahmen
- Frühzeitige Definition eines minimalen MVP‑Umfangs (Portal + Angebot + Login).
- Nutzung von Managed‑Security‑Services für OAuth/Identity‑Provider‑Integration.
- Einbindung eines externen Datenschutz‑Beraters für DSGVO‑Check.
- Klare Dokumentation von Rollen‑ und Berechtigungskonzept.
- Auswahl eines Cloud‑Providers mit garantierter EU‑Only‑Region.
- Iteratives Review‑ und Test‑Setup, um Security‑Review parallel zum MVP‑Entwicklungs‑Sprint zu ermöglichen.