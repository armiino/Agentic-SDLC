# ADR-0015: Offene Entscheidung zum lokalen Cloud-Daten-Cache

Status: accepted
Core-Item: ARCH-40

## Kontext

Zum Umgang mit Cloud-Daten ist festgehalten, dass noch geklaert werden muss, ob und wie diese Daten lokal auf dem Geraet gespeichert oder gecacht werden sollen. Dokumentiert ist damit ein offener Architekturpunkt, der den Datenzugriff und die lokale Datenhaltung betrifft.

## Entscheidung

Wir halten fest, dass die Entscheidung zu lokaler Speicherung oder Caching von Cloud-Daten auf dem Geraet noch aussteht und separat geklaert werden muss.

## Konsequenzen

Fuer Datenzugriff, Offline-Verhalten und lokale Persistenz kann noch kein verbindlicher technischer Zuschnitt vorausgesetzt werden. Das erhaelt Entscheidungsspielraum, bedeutet aber auch, dass nachgelagerte Architektur- und Implementierungsarbeit von einer noch offenen Klaerung abhaengt.

## Verwandte Core-Items

- REQ-36 — Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]
- ARCH-39 — Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.

