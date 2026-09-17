# Anforderungen

## 1. Zielbild MVP

- Das MVP muss einen Login per E-Mail und Passwort bereitstellen. [SC-REQ-LOGIN-006]
- Für den Login per E-Mail ist Double-Opt-In verpflichtend. [SC-REQ-LOGIN-006]

## 2. Angebots- und Rabatt-Workflow

- Angebote mit einem Rabatt von über 15 Prozent müssen einen Freigabeprozess durchlaufen. [SC-ARCH-DISCOUNT-WORKFLOW-005]
- Für Angebote sind folgende Workflow-Status erforderlich: `draft`, `pending approval`, `approved`, `sent`, `accepted`, `rejected`. [SC-ARCH-DISCOUNT-WORKFLOW-005]

## 3. SAP-Anbindung und MVP-Abgrenzung

- Ob SAP-Schreibzugriff erforderlich ist, ist noch nicht entschieden. [SC-REQ-SAP-WRITE-007]
- Für das MVP ist derzeit eher ein nur lesender SAP-Zugriff vorgesehen. [SC-REQ-SAP-WRITE-007]
- Im MVP ist offen, ob statt Online-Akzeptanz zunächst nur ein Angebotsdownload unterstützt wird. [SC-REQ-SAP-WRITE-007]

## 4. Skalierbarkeit und Performance

- Skalierbarkeit und Performance müssen für das MVP berücksichtigt werden. [SC-ARCH-SCALABILITY-001]
- Die erwartete Nutzerzahl ist derzeit unsicher und liegt in einer Spannweite von etwa 200 bis 20.000 Nutzern; daraus darf keine feste Lastannahme abgeleitet werden. [SC-ARCH-SCALABILITY-001]

## 5. Hosting und Datenresidenz

- Die Anforderungen an Datenresidenz für Hosting müssen geklärt werden. [SC-OQ-EU-RESIDENCE-018]
- EU-only Hosting und allgemein DSGVO-konformes Hosting sind nicht gleichzusetzen. [SC-OQ-EU-RESIDENCE-018]

## 6. Analytics und KPI-Messung

- Analytics beziehungsweise KPIs sind gewünscht, insbesondere für Conversion, Angebotsdauer und Nutzung. [SC-ARCH-ANALYTICS-002]
- Messkonzept, technische Umsetzung, Tracking und Datenschutz für Analytics sind noch offen. [SC-ARCH-ANALYTICS-002]

## 7. SAP-Daten, Preise, Caching und Fallbacks

- Der Umgang mit Preis- und Rabattschwankungen bei SAP-Daten ist offen. [SC-OQ-PRICE-CACHE-019]
- Es muss geklärt werden, ob und wie Cache oder Fallbacks für SAP-Daten eingesetzt werden. [SC-OQ-PRICE-CACHE-019] [SC-RISK-PRICE-CACHE-015]
- Bei Caching oder Fallbacks für SAP-Daten sind Datenaktualität, Datenschutz und Retention ausdrücklich zu berücksichtigen. [SC-RISK-PRICE-CACHE-015]
- Cache-Invalidierung ist dabei als Aufwands- und Risikofaktor zu berücksichtigen. [SC-OQ-PRICE-CACHE-019] [SC-RISK-PRICE-CACHE-015]

## 8. Support-Rahmen im MVP

- Für das MVP ist kein Ticketsystem als gesetzte Randbedingung zu berücksichtigen. [SC-RISK-SUPPORT-013]
- Die daraus entstehende unstrukturierte E-Mail-Bearbeitung ist als Datenschutz- und Support-Risiko zu dokumentieren. [SC-RISK-SUPPORT-013]

## 9. Nicht festgelegte beziehungsweise spätere Themen

- Push Notifications sind derzeit nicht für das MVP entschieden. [SC-REQ-PUSH-010]
- Push Notifications wurden als später mögliche Option eingebracht, bleiben aber offen und sind kein MVP-Requirement. [SC-REQ-PUSH-010]