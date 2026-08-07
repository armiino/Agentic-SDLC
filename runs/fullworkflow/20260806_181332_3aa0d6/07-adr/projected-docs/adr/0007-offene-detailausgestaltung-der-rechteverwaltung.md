# ADR-0007: Offene Detailausgestaltung der Rechteverwaltung

Status: accepted
Core-Item: ARCH-12

## Kontext

Im Material ist die Rechteverwaltung auf Grundsatzebene angelegt, aber nicht vollständig ausformuliert. Es ist ausdrücklich festgehalten, dass die konkrete Ausgestaltung der Rechteverwaltung noch nicht festgelegt ist und später entschieden werden muss; die Begründung beschreibt dies als dokumentierbaren offenen Architekturstand.

## Entscheidung

Wir halten fest, dass die konkrete Ausgestaltung der Rechteverwaltung noch offen ist und in einer späteren Entscheidung festgelegt werden muss.

## Konsequenzen

Der aktuelle Stand macht transparent, dass das Zugriffsmodell über die bereits festgelegten Mindestrollen hinaus noch nicht abschließend definiert ist. Gleichzeitig bleiben Detailentscheidungen zur Rechtevergabe offen und können für spätere Umsetzungsarbeit noch keine vollständige Verbindlichkeit beanspruchen.

## Verwandte Core-Items

- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.
- ARCH-13 — Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können.
- REQ-41 — Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können.

