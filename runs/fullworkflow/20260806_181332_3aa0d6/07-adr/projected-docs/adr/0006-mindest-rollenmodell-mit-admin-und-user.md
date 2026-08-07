# ADR-0006: Mindest-Rollenmodell mit Admin und User

Status: accepted
Core-Item: ARCH-08

## Kontext

Für das Zugriffsmodell der App musste ein Basisschnitt der Rollen und Rechte festgelegt werden. Das Material verlangt mindestens die Rollen Admin und User; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen. Die Begründung beschreibt dies als Architekturentscheidung für das Zugriffsmodell.

## Entscheidung

Wir nutzen mindestens die Rollen Admin und User. Admins verwalten Accounts und Rechte, User dürfen Inhalte hinzufügen, aber nichts löschen.

## Konsequenzen

Die App erhält ein klares Mindestmodell für Rollen und Grundrechte, das Account- und Inhaltsverwaltung strukturiert. Gleichzeitig ist die Umsetzung an diese Mindesttrennung gebunden, und Löschrechte für normale Nutzer sind ausgeschlossen.

## Verwandte Core-Items

- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.
- ARCH-07 — Die App darf keine Selbstregistrierung erlauben; Zugang erfolgt nur per Login mit intern vergebenen Accounts.
- REQ-05 — Die App darf keine Selbstregistrierung erlauben; der Zugang erfolgt ausschließlich per Login mit intern vergebenen Accounts.
- ARCH-12 — Die konkrete Ausgestaltung der Rechteverwaltung ist noch nicht festgelegt und muss später entschieden werden.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

