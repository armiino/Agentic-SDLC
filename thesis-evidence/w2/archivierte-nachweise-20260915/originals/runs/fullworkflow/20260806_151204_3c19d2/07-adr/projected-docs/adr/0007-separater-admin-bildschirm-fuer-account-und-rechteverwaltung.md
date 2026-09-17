# ADR-0007: Separater Admin-Bildschirm fuer Account- und Rechteverwaltung

Status: accepted
Core-Item: ARCH-13

## Kontext

Fuer die Rechte- und Accountverwaltung wurde als Loesungszuschnitt festgehalten, dass ein separater Admin-Bildschirm erwogen werden soll. Auf diesem sollen nur Admins Accounts anlegen und Rollen vergeben koennen. Die Aussage dokumentiert eine konkrete Designoption fuer die Abgrenzung administrativer Funktionen.

## Entscheidung

Wir beruecksichtigen fuer die Rechte- und Accountverwaltung einen separaten Admin-Bildschirm, auf dem nur Admins Accounts anlegen und Rollen vergeben koennen.

## Konsequenzen

Administrative Funktionen koennen klar von regulaeren Nutzungsablaeufen getrennt werden, was die Abgrenzung sensibler Verwaltungsaufgaben erleichtert. Gleichzeitig ist dieser Zuschnitt noch als zu pruefende Ausgestaltung markiert und bindet die weitere UI- und Rechtekonzeption an eine gesonderte Admin-Flaeche, falls er uebernommen wird.

## Verwandte Core-Items

- REQ-41 — Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, auf dem nur Admins Accounts anlegen und Rollen vergeben können.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.

