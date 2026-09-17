# ADR-0001: Mobile-Only-App ohne Web-Client

Status: accepted
Core-Item: ARCH-01

## Kontext

Die Loesung ist als digitale App zur Foerderung der Kommunikation zwischen betreuten Menschen und anderen Personen bestaetigt. Zusaetzlich wurde konkretisiert, dass die Umsetzung als reine Mobile-App erfolgen soll. Diese Festlegung grenzt die betroffenen Umsetzungen klar auf mobile Nutzung ein und schliesst einen Web-Client aus.

## Entscheidung

Wir nutzen fuer die Loesung eine reine Mobile-App und setzen keinen Web-Client um.

## Konsequenzen

Die Umsetzung bleibt auf mobile Endgeraete fokussiert, was den Plattformzuschnitt vereinfacht und Entscheidungen fuer mobile Interaktion klar ausrichtet. Gleichzeitig sind Nutzungsszenarien ueber einen Browser ausgeschlossen, und Funktionen, Bedienkonzepte sowie Bereitstellung muessen konsequent fuer mobile Clients ausgelegt werden.

## Betrachtete Alternativen

Ein Web-Client wurde als Alternative nicht weiterverfolgt.

## Verwandte Core-Items

- REQ-01 — Die angestrebte Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern bzw. fördern; die genaue Ausgestaltung dieses Produktziels ist noch nicht vollständig konkretisiert.
- REQ-37 — Die App muss plattformübergreifend auf iPhone und Android laufen.

