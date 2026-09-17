# ADR-0004: Mindestrollen Admin und User

Status: accepted
Core-Item: ARCH-08

## Kontext

Fuer das Zugriffsmodell wurde festgelegt, dass es mindestens die Rollen Admin und User geben muss. Admins verwalten Accounts und Rechte, waehrend User Inhalte hinzufuegen koennen, aber nichts loeschen duerfen. Diese Festlegung strukturiert das Rechtemodell grundlegend.

## Entscheidung

Wir setzen mindestens die Rollen Admin und User um. Admins verwalten Accounts und Rechte, User duerfen Inhalte hinzufuegen, aber nichts loeschen.

## Konsequenzen

Das Rechtemodell erhaelt eine klare Mindeststruktur fuer Verwaltung und fachliche Nutzung. Verantwortlichkeiten fuer Account- und Rechteverwaltung sind den Admins zugeordnet. Gleichzeitig wird die Umsetzung der Inhaltsbearbeitung an die Einschraenkung gebunden, dass User keine Loeschungen ausfuehren duerfen.

## Verwandte Core-Items

- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

