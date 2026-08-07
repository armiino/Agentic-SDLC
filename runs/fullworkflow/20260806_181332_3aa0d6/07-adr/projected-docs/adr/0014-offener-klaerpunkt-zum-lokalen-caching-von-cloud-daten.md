# ADR-0014: Offener Klärpunkt zum lokalen Caching von Cloud-Daten

Status: accepted
Core-Item: ARCH-40

## Kontext

Für den Umgang mit Daten auf dem Gerät liegt noch keine abgeschlossene Festlegung vor. Das Material hält ausdrücklich fest, dass geklärt werden muss, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen; die Begründung beschreibt dies als offenen Architekturklärpunkt.

## Entscheidung

Wir dokumentieren als offenen Architekturstand, dass zu klären ist, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen.

## Konsequenzen

Der aktuelle Entscheidungsstand macht transparent, dass die Datenhaltung auf dem Gerät noch nicht abschließend festgelegt ist. Gleichzeitig bleiben Auswirkungen auf Offline-Verhalten, Performance, Synchronisation und Implementierung bis zur späteren Klärung offen.

## Verwandte Core-Items

- REQ-36 — Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]
- ARCH-39 — Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.
- REQ-55 — Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.
- L3-REQ-004 — Bei offline bearbeiteten Konfliktfällen muss eine klare Konfliktanzeige erfolgen; nichts darf stillschweigend überschrieben werden, und der Nutzer entscheidet, welche Version gilt.

