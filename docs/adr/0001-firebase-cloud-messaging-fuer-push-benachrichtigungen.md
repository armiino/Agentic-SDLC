# ADR-0001: Firebase Cloud Messaging für Push-Benachrichtigungen

Status: accepted
Core-Item: ARCH-46

## Kontext

Für Push-Benachrichtigungen an Angehörige ist Firebase Cloud Messaging als technische Umsetzung festgelegt. Das Material beschreibt dies als konkrete Technologieentscheidung; fachlich betroffen ist die Benachrichtigungserweiterung für neue Inhalte im About-Me-Bereich.

## Entscheidung

Wir verwenden Firebase Cloud Messaging als technische Umsetzung für Push-Benachrichtigungen an Angehörige.

## Konsequenzen

Positiv schafft dies eine klare technische Festlegung für Push-Benachrichtigungen und reduziert Offenheit im Benachrichtigungsstack. Einschränkend müssen alle Arbeiten zu dieser Funktion auf Firebase Cloud Messaging ausgerichtet werden; alternative Push-Technologien sind dafür nicht vorgesehen.

## Verwandte Core-Items

- ARCH-42 — Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden.
- REQ-40 — Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist als spätere mögliche Erweiterung vorgesehen.

