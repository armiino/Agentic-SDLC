# Requirements

## MVP-Funktionsanforderungen

- Das MVP muss einen Login per E-Mail und Passwort bereitstellen.
- Für den Login per E-Mail ist Double-Opt-In verpflichtend.
- Angebote mit einem Rabatt von über 15 Prozent müssen einen Freigabeprozess durchlaufen.
- Für Angebote sind Workflow-Status erforderlich: `draft`, `pending approval`, `approved`, `sent`, `accepted`, `rejected`.

## SAP-Integration

- Ob SAP-Schreibzugriff erforderlich ist, ist noch nicht entschieden.
- Für das MVP ist derzeit eher ein nur lesender SAP-Zugriff vorgesehen.
- Im MVP ist offen, ob statt Online-Akzeptanz zunächst nur ein Angebotsdownload unterstützt wird.

## Nichtfunktionale Anforderungen und Randbedingungen

- Skalierbarkeit und Performance müssen für das MVP berücksichtigt werden.
- Die erwartete Nutzerzahl ist derzeit unsicher und liegt in einer Spannweite von etwa 200 bis 20.000 Nutzern; daraus darf keine feste Lastannahme abgeleitet werden.
- Die Anforderungen an Datenresidenz für Hosting müssen geklärt werden.
- EU-only Hosting und allgemein DSGVO-konformes Hosting sind nicht gleichzusetzen.

## Analytics

- Analytics beziehungsweise KPIs sind gewünscht, insbesondere für Conversion, Angebotsdauer und Nutzung.
- Messkonzept, technische Umsetzung, Tracking und Datenschutz für Analytics sind noch offen.

## Offene Punkte zu SAP-Daten, Preisen und Caching

- Der Umgang mit Preis- und Rabattschwankungen bei SAP-Daten ist offen.
- Es muss geklärt werden, ob und wie Cache oder Fallbacks für SAP-Daten eingesetzt werden.
- Bei Caching oder Fallbacks für SAP-Daten sind Datenaktualität, Datenschutz und Retention ausdrücklich zu berücksichtigen.
- Cache-Invalidierung ist dabei als Aufwands- und Risikofaktor zu berücksichtigen.

## Support-Randbedingungen im MVP

- Für das MVP ist kein Ticketsystem als gesetzte Randbedingung zu berücksichtigen.
- Die daraus entstehende unstrukturierte E-Mail-Bearbeitung ist als Datenschutz- und Support-Risiko zu dokumentieren.

## Außerhalb des MVP / offene spätere Scope-Optionen

- Push Notifications sind derzeit nicht für das MVP entschieden.
- Push Notifications wurden als später mögliche Option eingebracht, bleiben aber offen und sind kein MVP-Requirement.