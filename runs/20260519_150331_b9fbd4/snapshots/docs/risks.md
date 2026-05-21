# Risiken

## Technische Risiken
- **API‑Layer nicht API‑ready**: Das Backend ist noch nicht API‑ready, was die Implementierung verzögern kann.
- **Keine eigene DB‑Instanz**: Abhängigkeit von Managed Services kann zu Lieferverzögerungen führen.
- **Security Review Zeitrahmen**: Security Review wird mit 6 Wochen veranschlagt und könnte das 8‑Wochen‑MVP überschreiten.
- **OAuth‑Implementierung**: OAuth ist komplexer als API‑Keys und könnte den Zeitplan gefährden.
- **Skalierbarkeit vs. Over‑Engineering**: Die erwartete Nutzerzahl variiert stark (200–20 000), was zu Fehlentscheidungen bei der Architektur führen kann.

## Datenschutz‑Risiken
- **DSGVO‑Konformität**: Fehlende Double‑Opt‑In‑Umsetzung, unzureichendes Lösch‑ und Audit‑Konzept können zu Rechtsverstößen führen.
- **Datenhosting**: Nicht‑EU‑Hosting oder unklare Datenlokation kann DSGVO‑Verstöße auslösen.
- **Logging & Audit‑Trail**: Unvollständige Protokollierung kann Compliance‑Prüfungen gefährden.

## Projekt‑Risiken
- **Unklare Zieldefinition**: Unterschiedliche Prioritäten (Portal vs. Angebote vs. KPI‑Messung) können zu Scope‑Creep führen.
- **Ressourcen‑Mangel**: Fehlender Architekt, begrenztes Budget und keine klare Entscheidung zu Mobile/Native App.
- **Abhängigkeit von SAP**: Unvollständige Stammdaten in SAP können die Angebotsfunktion beeinträchtigen.
- **Stakeholder‑Einigkeit**: Unterschiedliche Vorstellungen (z. B. Push‑Notifications) können zu Verzögerungen führen.

## Maßnahmen
- Frühzeitige Definition eines Minimal‑Scope für das MVP.
- Nutzung von Managed‑Service‑Anbietern mit EU‑Region‑Garantie.
- Schnell‑Prototyping des API‑Layers mit Mock‑Endpoints.
- Parallel laufendes, leichtgewichtiges Security‑Review (z. B. Threat‑Modeling).
- Einbindung eines externen Datenschutz‑Beraters für DSGVO‑Check.
