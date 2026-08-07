# ADR-0008: Offene Ausgestaltung der Rechteverwaltung

Status: accepted
Core-Item: ARCH-12

## Kontext

Im Material ist festgehalten, dass die konkrete Ausgestaltung der Rechteverwaltung noch nicht festgelegt ist und spaeter entschieden werden muss. Dokumentiert ist damit eine bestehende Architektur-Offenheit im Rechtemodell, obwohl grundlegende Rollen und einzelne Zugriffsregeln bereits benannt sind.

## Entscheidung

Wir halten die konkrete Ausgestaltung der Rechteverwaltung zum jetzigen Stand offen und treffen dazu in diesem Schritt keine weitergehende Festlegung.

## Konsequenzen

Bereits bekannte Mindestfestlegungen zu Rollen und einzelnen Zugriffsbeschraenkungen koennen dokumentiert werden, ohne das Rechtemodell weiter zu verengen. Gleichzeitig bleibt Detailarbeit zur Rechteverwaltung ausstehend und muss spaeter separat entschieden werden.

## Verwandte Core-Items

- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- ARCH-09 — Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten und Inhalte beziehungsweise Wissen beitragen können; unterschiedliche Rechte sind dabei vorgesehen, aber noch nicht konkret ausformuliert.
- ARCH-10 — Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil sehen darf und nur eingeschränkte Funktionen nutzen kann, insbesondere Zugriff auf About Me und gegebenenfalls das Hinzufügen eigener Bilder.
- ARCH-11 — Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten.

