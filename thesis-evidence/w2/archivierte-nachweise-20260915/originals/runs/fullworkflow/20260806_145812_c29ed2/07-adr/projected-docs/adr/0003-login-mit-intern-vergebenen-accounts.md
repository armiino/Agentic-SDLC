# ADR-0003: Login mit intern vergebenen Accounts

Status: accepted
Core-Item: ARCH-07

## Kontext

Fuer den Zugang zur App wurde festgelegt, dass keine Selbstregistrierung erlaubt ist. Stattdessen erfolgt der Zugang ausschliesslich per Login mit intern vergebenen Accounts. Diese Festlegung betrifft die Konto- und Login-Umsetzung sowie den Sicherheitsrahmen des Zugangs.

## Entscheidung

Wir erlauben keine Selbstregistrierung und gewaehren Zugang zur App nur ueber intern vergebene Accounts mit Login.

## Konsequenzen

Der Zugang zur App bleibt kontrolliert und an interne Vergabeprozesse gebunden. Das unterstuetzt eine gesteuerte Benutzerverwaltung. Gleichzeitig muss die Organisation Accounts aktiv anlegen und verwalten, und ein oeffentlicher Registrierungsprozess wird nicht umgesetzt.

## Betrachtete Alternativen

Eine Selbstregistrierung wird nicht angeboten.

## Verwandte Core-Items

- REQ-05 — Die App darf keine Selbstregistrierung erlauben; der Zugang erfolgt ausschließlich per Login mit intern vergebenen Accounts.
- REQ-12 — Der Login-Screen soll E-Mail-Feld, Passwort-Feld und einen Login-Button enthalten; eine Registrierungsmöglichkeit darf dort nicht angeboten werden.

