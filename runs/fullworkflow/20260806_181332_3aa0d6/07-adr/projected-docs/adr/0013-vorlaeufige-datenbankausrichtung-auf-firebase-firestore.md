# ADR-0013: Vorläufige Datenbankausrichtung auf Firebase Firestore

Status: accepted
Core-Item: ARCH-39

## Kontext

Für die Persistenz der App musste eine Datenbanklösung eingeordnet werden. Das Material legt fest, dass Firebase Firestore als Datenbanklösung vorläufig vorgesehen ist, aber noch nicht endgültig festgelegt wurde; die Begründung beschreibt dies als dokumentierbare Technologieentscheidung mit offenem Finalisierungsgrad.

## Entscheidung

Wir richten die Datenbanklösung vorläufig auf Firebase Firestore aus, ohne die endgültige Festlegung bereits abzuschließen.

## Konsequenzen

Die Stack-Planung erhält eine konkrete Persistenzrichtung und kann auf Firestore ausgerichtet vorbereitet werden. Gleichzeitig bleibt die Entscheidung vorläufig, sodass spätere Bestätigung oder Anpassung weiterhin möglich ist und Unsicherheit für nachgelagerte Detailentscheidungen bestehen bleibt.

## Verwandte Core-Items

- REQ-55 — Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.
- REQ-70 — Firebase Firestore ist als Persistenztechnologie festgelegt; Alternativen werden nicht weiter diskutiert.
- ARCH-40 — Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]
- REQ-36 — Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen. [canon-lokaler-cache-klären]

