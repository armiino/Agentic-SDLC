# Requirements

## MVP-Funktionsumfang

- Das MVP muss Login per E-Mail und Passwort unterstützen.
- Für den Login per E-Mail ist Double-Opt-In verpflichtend.
- SAP-Schreibzugriff ist für das MVP nicht entschieden; derzeit ist im MVP eher ein nur lesender SAP-Zugriff vorgesehen.
- Push Notifications sind aktuell nicht für das MVP entschieden und gelten allenfalls als spätere Option, nicht als MVP-Anforderung.

## Fachliche Regeln und Workflows

- Angebote mit mehr als 15 Prozent Rabatt müssen einen Freigabeprozess durchlaufen.
- Dafür müssen Angebots-Workflow-Status mindestens `draft`, `pending approval`, `approved`, `sent`, `accepted` und `rejected` berücksichtigt werden.

## Nichtfunktionale Anforderungen und Randbedingungen

- Skalierbarkeit und Performance müssen für das MVP berücksichtigt werden, da die erwartete Nutzerzahl zwischen etwa 200 und 20.000 schwankt und derzeit unklar ist.
- Analytics bzw. KPIs sind gewünscht, insbesondere für Conversion, Angebotsdauer und Nutzung.
- Das Messkonzept, die technische Umsetzung des Trackings sowie Datenschutzfragen für Analytics sind noch offen.

## Compliance und Datenhaltung

- Die Anforderung an Datenresidenz muss für das MVP geklärt werden; EU-only Hosting und allgemein DSGVO-konformes Hosting sind nicht automatisch dasselbe.
- Der Umgang mit Preis- und Rabattschwankungen bei SAP-Daten sowie der Einsatz von Cache oder Fallbacks ist offen und muss geklärt werden.
- Bei Cache- oder Fallback-Lösungen für SAP-Daten müssen Datenaktualität, Datenschutz und Retention mitberücksichtigt werden.

## Support und Betriebsrisiken

- Für das MVP ist kein Ticketsystem vermerkt; daraus resultiert das Risiko unstrukturierter E-Mail-Bearbeitung mit Datenschutz- und Supportproblemen, das berücksichtigt werden muss.
