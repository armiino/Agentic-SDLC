# ADR-0007: Einrichtungsgebundener Profilzugriff

Status: accepted
Core-Item: ARCH-11

## Kontext

Fuer Mitarbeiter wurde festgelegt, dass sie nicht einrichtungsuebergreifend auf alle Profile zugreifen duerfen. Stattdessen ist der Zugriff auf die Profile der Einrichtung beschraenkt, in der sie taetig sind. Laut Material ist die technische Umsetzung dieser Beschraenkung noch zu erarbeiten.

## Entscheidung

Wir beschraenken den Zugriff von Mitarbeitern auf die Profile ihrer eigenen Einrichtung und erlauben keinen einrichtungsuebergreifenden Vollzugriff.

## Konsequenzen

Die Sichtbarkeit von Bewohnerprofilen wird entlang von Einrichtungen begrenzt und folgt damit einem klaren Mandanten- beziehungsweise Berechtigungsprinzip. Das reduziert unnoetigen Datenzugriff. Gleichzeitig muss die technische Umsetzung dieser Beschraenkung noch ausgearbeitet werden und bindet das spaetere Rechtemodell sowie Datenzugriffe.

## Betrachtete Alternativen

Ein einrichtungsuebergreifender Zugriff auf alle Profile wird ausgeschlossen.

## Verwandte Core-Items

- REQ-09 — Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf die Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten.

