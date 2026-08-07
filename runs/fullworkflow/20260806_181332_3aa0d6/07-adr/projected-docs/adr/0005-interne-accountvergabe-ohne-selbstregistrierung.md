# ADR-0005: Interne Accountvergabe ohne Selbstregistrierung

Status: accepted
Core-Item: ARCH-07

## Kontext

Für Zugang und Betrieb der App musste festgelegt werden, wie Accounts entstehen. Das Material bestimmt, dass keine Selbstregistrierung erlaubt ist und der Zugang nur per Login mit intern vergebenen Accounts erfolgt; die Begründung ordnet dies als Sicherheits- und Betriebsentscheidung ein.

## Entscheidung

Wir erlauben keine Selbstregistrierung und gewähren Zugang ausschließlich über intern vergebene Accounts mit Login.

## Konsequenzen

Die Accountvergabe bleibt organisatorisch kontrolliert und der Zugriff kann zentral gesteuert werden. Gleichzeitig entsteht Aufwand für interne Bereitstellung und Verwaltung von Accounts, und spontane eigenständige Registrierung durch Nutzer ist ausgeschlossen.

## Betrachtete Alternativen

Eine Selbstregistrierung wird nicht angeboten.

## Verwandte Core-Items

- REQ-05 — Die App darf keine Selbstregistrierung erlauben; der Zugang erfolgt ausschließlich per Login mit intern vergebenen Accounts.
- REQ-12 — Der Login-Screen soll E-Mail-Feld, Passwort-Feld und einen Login-Button enthalten; eine Registrierungsmöglichkeit darf dort nicht angeboten werden.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- REQ-06 — Es muss mindestens die Rollen Admin und User geben. Admins müssen Accounts anlegen und Rechte verwalten können; User können Inhalte hinzufügen, aber nichts löschen.

