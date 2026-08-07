# ADR-0020: No-Go-Seite mit dynamischer Liste

Status: accepted
Core-Item: ARCH-25

## Kontext

Fuer kritische bewohnerbezogene Hinweise wurde festgelegt, dass die App eine No-Go-Seite mit dynamisch erweiterbarer Liste bereitstellt. Dort sollen Dinge festgehalten werden, die in Gegenwart des Bewohners vermieden werden muessen. Diese Festlegung schafft einen eigenen strukturierten Bereich fuer solche Informationen.

## Entscheidung

Wir stellen pro Bewohner eine No-Go-Seite mit dynamisch erweiterbarer Liste fuer kritische zu vermeidende Dinge bereit.

## Konsequenzen

Kritische Hinweise erhalten einen eigenen, klar abgegrenzten Bereich und koennen fortlaufend ergaenzt werden. Das unterstuetzt die schnelle Verfuegbarkeit solcher Informationen im Bewohnerkontext. Gleichzeitig muss die App diesen Bereich als eigenstaendige Struktur mit erweiterbarer Liste umsetzen.

## Verwandte Core-Items

- REQ-32 — No-Go-Einträge müssen bewohnerbezogen geführt werden und dürfen nicht global für alle Bewohner gelten.
- REQ-33 — Die No-Go-Seite muss eine dynamisch erweiterbare Liste bieten, in die neue No-Gos per Plus-Button hinzugefügt werden können.
- REQ-34 — Die No-Go-Einträge sollen einfach zu durchforsten sein und nur die wichtigsten Informationen enthalten.

