# ADR-0014: Firebase Firestore als vorlaeufige Datenbankloesung

Status: accepted
Core-Item: ARCH-39

## Kontext

Als Datenbankloesung wurde Firebase Firestore vorlaeufig vorgesehen, zugleich aber ausdruecklich noch nicht endgueltig festgelegt. Damit ist eine konkrete Technologiepraeferenz dokumentiert, ohne dass bereits eine unumkehrbare Festlegung behauptet wird.

## Entscheidung

Wir sehen Firebase Firestore vorlaeufig als Datenbankloesung vor, ohne die endgueltige Festlegung bereits abzuschliessen.

## Konsequenzen

Die technische Ausarbeitung kann sich zunaechst an Firestore orientieren und entsprechende Integrationsannahmen vorbereiten. Gleichzeitig bleibt die Entscheidung vorlaeufig, sodass weitere Architektur- und Implementierungsarbeit von einer spaeteren finalen Klaerung abhaengt und Anpassungen erforderlich werden koennen.

## Betrachtete Alternativen

Andere Datenbankloesungen sind noch nicht endgueltig ausgeschlossen, weil Firestore nur vorlaeufig vorgesehen ist.

## Verwandte Core-Items

- REQ-55 — Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.
- REQ-70 — Firebase Firestore ist als Persistenztechnologie festgelegt; Alternativen werden nicht weiter diskutiert.

