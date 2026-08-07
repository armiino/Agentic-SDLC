# ADR-0001: Festlegung auf reine Mobile-App

Status: accepted
Core-Item: ARCH-01

## Kontext

Die Lösung zur Förderung der Kommunikation wurde als digitale App konkretisiert. Laut Begründung ist die Festlegung auf eine reine Mobile-App eine Architekturentscheidung, die den Lösungsrahmen bestimmt und alle Arbeiten ausschließt, die einen Web-Client vorsehen würden.

## Entscheidung

Wir nutzen eine reine Mobile-App und sehen keinen Web-Client vor.

## Konsequenzen

Die Umsetzung fokussiert vollständig mobile Nutzungsszenarien und reduziert die Architektur auf einen mobilen Kanal. Gleichzeitig sind Funktionen, Oberflächen und technische Arbeiten für einen Web-Client ausgeschlossen oder müssten später durch eine neue Entscheidung neu eröffnet werden.

## Betrachtete Alternativen

Ein zusätzlicher oder alternativer Web-Client wird nicht genutzt.

## Verwandte Core-Items

- REQ-01 — Die angestrebte Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern bzw. fördern; die genaue Ausgestaltung dieses Produktziels ist noch nicht vollständig konkretisiert.
- ARCH-36 — Die App soll plattformübergreifend auf iPhone und Android laufen.
- REQ-37 — Die App muss plattformübergreifend auf iPhone und Android laufen.

