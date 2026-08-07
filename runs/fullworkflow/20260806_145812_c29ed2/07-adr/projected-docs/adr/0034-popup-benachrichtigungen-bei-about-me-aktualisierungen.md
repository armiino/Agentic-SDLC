# ADR-0034: Popup-Benachrichtigungen bei About-Me-Aktualisierungen

Status: accepted
Core-Item: ARCH-42

## Kontext

Fuer neue Inhalte im About-Me-Bereich wurde festgelegt, dass verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden sollen. Damit ist eine konkrete Benachrichtigungslogik fuer Aktualisierungen in diesem Bereich beschrieben.

## Entscheidung

Wir benachrichtigen verbundene Nutzer derselben Einrichtung per Popup, wenn im About-Me-Bereich neue Inhalte hochgeladen werden.

## Konsequenzen

Aktualisierungen in About Me werden fuer relevante Nutzergruppen sichtbar und koennen schneller wahrgenommen werden. Gleichzeitig muss die Umsetzung eine einrichtungsbezogene Zuordnung der Benachrichtigungen sowie die Ausloesung bei neuen Uploads unterstuetzen.

## Verwandte Core-Items

- REQ-40 — Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist als spätere mögliche Erweiterung vorgesehen.
- L3-REQ-013 — Bei Benachrichtigungen und Übersichten sollte klar erkennbar sein, welche Inhalte seit dem letzten Besuch neu oder aktualisiert sind.

