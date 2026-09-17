# Requirements

## Funktionale Anforderungen

- Die Pflege-App muss eine Wochenübersicht bereitstellen, in der Pflegende alle dokumentierten Ereignisse einer Woche pro Bewohner sehen können. [canonical-wochenuebersicht-ereignisse-pro-woche]
- Die Wochenübersicht muss nach Bewohner filterbar sein. [canonical-wochenuebersicht-bewohner-filter]
- Es muss einen monatlichen PDF-Export mit allen dokumentierten Ereignissen geben, der von der Leitung angestoßen wird. [canonical-monatlicher-pdf-export-durch-leitung]

## Zugriffs- und Berechtigungsanforderungen

- Angehörige müssen die Wochenübersicht ihrer Bezugsperson lesen können, dürfen sie aber nicht ändern. [canonical-angehoerige-lesezugriff-ohne-aenderung]

## Constraints

- Der Dateiname des PDF-Exports muss das Bewohnerkürzel und den Monat enthalten, damit die Ablage eindeutig bleibt. [canonical-export-dateiname-bewohnerkuerzel-monat]

## Offene Klärungen / Kontext

- Mit dem Datenschutz muss geklärt werden, ob Angehörige dem PDF-Export ihrer Bezugsperson vorab zustimmen müssen. [canonical-offene-frage-zustimmung-angehoerige-export]
- Gewünscht ist ein gedämpfter Dunkelmodus für die Wochenübersicht für die Nachtschicht, damit das Display im Bewohnerzimmer nicht blendet; die Umsetzung ist zeitlich noch unklar und kann MVP oder später betreffen. [canonical-dunkelmodus-wochenuebersicht]