# ADR-0003: Interne Accountvergabe ohne Selbstregistrierung

Status: accepted
Core-Item: ARCH-07

## Kontext

Fuer den Zugang zur App wurde festgelegt, dass keine Selbstregistrierung erlaubt ist. Stattdessen erfolgt der Zugang ausschliesslich per Login mit intern vergebenen Accounts. Diese Festlegung betrifft unmittelbar Accountverwaltung, Login-Prozess und Sicherheitszuschnitt.

## Entscheidung

Wir erlauben keine Selbstregistrierung und stellen den Zugang ausschliesslich ueber Login mit intern vergebenen Accounts bereit.

## Konsequenzen

Die Kontrolle ueber den Nutzerzugang bleibt bei der Organisation, was den geregelten Zugang und die Verwaltung von Berechtigungen unterstuetzt. Gleichzeitig entsteht organisatorischer Aufwand fuer Anlage und Pflege von Accounts, und nutzerseitige Registrierungsablaeufe werden bewusst nicht angeboten.

## Betrachtete Alternativen

Eine offene Selbstregistrierung wurde nicht uebernommen.

## Verwandte Core-Items

- REQ-05 — Die App darf keine Selbstregistrierung erlauben; der Zugang erfolgt ausschließlich per Login mit intern vergebenen Accounts.
- REQ-12 — Der Login-Screen soll E-Mail-Feld, Passwort-Feld und einen Login-Button enthalten; eine Registrierungsmöglichkeit darf dort nicht angeboten werden.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

