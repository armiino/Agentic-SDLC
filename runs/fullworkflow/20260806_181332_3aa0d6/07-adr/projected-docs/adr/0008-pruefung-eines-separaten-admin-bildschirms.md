# ADR-0008: Prüfung eines separaten Admin-Bildschirms

Status: accepted
Core-Item: ARCH-13

## Kontext

Für die Rechte- und Accountverwaltung wurde im Material eine konkrete Umsetzungsoption benannt. Es soll erwogen werden, einen separaten Admin-Bildschirm vorzusehen, auf dem nur Admins Accounts anlegen und Rollen vergeben können; die Begründung ordnet dies als noch nicht gesicherte Designidee ein.

## Entscheidung

Wir dokumentieren einen separaten Admin-Bildschirm für Account- und Rechteverwaltung als zu prüfende Lösungsoption, auf den nur Admins zugreifen und in dem sie Accounts anlegen sowie Rollen vergeben können.

## Konsequenzen

Die Option schafft eine erkennbare Richtung für die Bündelung administrativer Funktionen. Gleichzeitig ist diese Ausgestaltung noch nicht endgültig festgelegt und darf nicht als bereits abschließend entschiedene Detailumsetzung missverstanden werden.

## Verwandte Core-Items

- REQ-41 — Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.
- ARCH-12 — Die konkrete Ausgestaltung der Rechteverwaltung ist noch nicht festgelegt und muss später entschieden werden.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

