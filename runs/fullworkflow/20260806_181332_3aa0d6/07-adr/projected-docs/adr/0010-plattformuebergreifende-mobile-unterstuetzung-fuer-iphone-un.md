# ADR-0010: Plattformübergreifende Mobile-Unterstützung für iPhone und Android

Status: accepted
Core-Item: ARCH-36

## Kontext

Für den technischen Zielrahmen der App musste festgelegt werden, auf welchen mobilen Plattformen die Lösung laufen soll. Das Material legt fest, dass die App plattformübergreifend auf iPhone und Android laufen soll; die Begründung beschreibt dies als grundlegende Architekturentscheidung für technische und UX-seitige Umsetzung.

## Entscheidung

Wir entwickeln die App plattformübergreifend für iPhone und Android.

## Konsequenzen

Die Lösung adressiert beide relevanten mobilen Plattformen und kann in heterogenen Gerätesituationen eingesetzt werden. Gleichzeitig muss die Umsetzung Plattformunterschiede in Technik, Bedienung und Test berücksichtigen und ist an die Unterstützung beider Plattformen gebunden.

## Verwandte Core-Items

- REQ-37 — Die App muss plattformübergreifend auf iPhone und Android laufen.
- ARCH-37 — Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen.
- REQ-53 — Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende Umsetzung zu unterstützen.
- ARCH-01 — Die Lösung bleibt eine digitale App zur Förderung der Kommunikation und wird als reine Mobile-App ohne Web-Client konkretisiert.

