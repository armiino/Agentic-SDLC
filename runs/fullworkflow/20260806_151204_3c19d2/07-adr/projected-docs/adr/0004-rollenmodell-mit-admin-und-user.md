# ADR-0004: Rollenmodell mit Admin und User

Status: accepted
Core-Item: ARCH-08

## Kontext

Fuer die App wurde mindestens ein Rollenmodell mit Admin und User festgelegt. Admins sollen Accounts und Rechte verwalten koennen, waehrend User Inhalte hinzufuegen koennen, aber nichts loeschen duerfen. Diese Entscheidung schafft einen verbindlichen Grundzuschnitt fuer Rollen und Berechtigungen.

## Entscheidung

Wir nutzen mindestens die Rollen Admin und User. Admins verwalten Accounts und Rechte, User duerfen Inhalte hinzufuegen, aber nichts loeschen.

## Konsequenzen

Die Rechte- und Accountverwaltung erhaelt eine klare Grundstruktur, und zentrale Verwaltungsaufgaben sind an die Admin-Rolle gebunden. Zugleich muessen Funktionen konsequent an dieses Rollenmodell angepasst werden. Fuer User sind destructive Aktionen eingeschraenkt, und weitergehende Rollen- oder Detailrechte bleiben davon unberuehrt spaeter zu konkretisieren.

## Verwandte Core-Items

- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

