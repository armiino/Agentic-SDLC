# Architektur

## Architekturtreiber und offene Architekturentscheidungen

- Das MVP soll eine Authentifizierung per E-Mail und Passwort vorsehen; dabei ist Double-Opt-In verpflichtend und als architektonische Randbedingung zu berücksichtigen.
- Für die SAP-Integration ist Schreibzugriff nicht entschieden; für das MVP ist eher ein lesender Zugriff vorgesehen. Die Architektur muss diese Entscheidung daher offenhalten und darf keinen fest zugesagten SAP-Schreibpfad im MVP voraussetzen.
- Die erwartete Nutzerzahl ist unklar und liegt nach aktuellem Stand zwischen ca. 200 und 20.000 Nutzern. Skalierbarkeit und Performance müssen daher in der Architektur berücksichtigt werden, ohne eine feste Lastannahme zu unterstellen.
- Analytics bzw. KPIs sind gewünscht, jedoch sind Messkonzept, technische Umsetzung, Tracking und Datenschutz derzeit offen. Die Architektur muss diesen Punkt als offene Entscheidung behandeln.

## Fachlich geprägte Architekturanforderungen

- Für Angebote mit mehr als 15 Prozent Rabatt ist ein Freigabeprozess erforderlich.
- Dafür werden Workflow-Status für Angebote benötigt: `draft`, `pending approval`, `approved`, `sent`, `accepted`, `rejected`.

## Compliance- und Datenhaltungsaspekte

- Datenresidenz für das Hosting ist offen zu klären. EU-only Hosting und allgemein DSGVO-konformes Hosting sind nicht automatisch dasselbe; diese Unterscheidung ist in Architekturentscheidungen explizit zu berücksichtigen.
- Der Umgang mit Preis- und Rabattschwankungen bei SAP-Daten sowie die Nutzung von Cache oder Fallback sind offen und müssen architektonisch geklärt werden.
- Caching oder Fallbacks für SAP-Daten müssen im Spannungsfeld von Datenaktualität, Datenschutz und Retention betrachtet werden.

## Betriebsnahe Randbedingungen

- Wenn im MVP kein Ticketsystem vorgesehen ist, ist das Risiko unstrukturierter E-Mail-Bearbeitung mit Datenschutz- und Support-Auswirkungen als architekturnahe Randbedingung zu beachten.
