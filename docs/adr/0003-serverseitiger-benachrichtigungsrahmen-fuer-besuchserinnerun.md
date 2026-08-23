# ADR-0003: Serverseitiger Benachrichtigungsrahmen für Besuchserinnerungen

Status: accepted
Core-Item: ARCH-48

## Kontext

Für Besuchserinnerungen am Vortag um 18 Uhr musste festgelegt werden, wie die Erinnerung verlässlich ausgelöst wird. Das zugrunde liegende Design-Item bestimmt, dass die Erinnerung zentral aus dem bestätigten Besuchsstatus heraus geplant wird, bei Statusänderung oder Absage wieder zurückgezogen werden muss und nicht von lokaler Verfügbarkeit oder Hintergrundrestriktionen auf dem Gerät der Angehörigen abhängen darf. Damit wird die Umsetzung der Push-Erinnerung an bestätigte Besuche architektonisch festgelegt.

## Entscheidung

Wir planen Besuchserinnerungen serverseitig und zentral aus dem bestätigten Besuchsstatus heraus. Wir ziehen eine geplante Erinnerung bei Statusänderung oder Absage wieder zurück. Wir gestalten die Erinnerung so, dass sie nicht von lokaler Verfügbarkeit oder Hintergrundrestriktionen des Geräts der Angehörigen abhängt.

## Konsequenzen

Die Erinnerung wird unabhängig vom lokalen Gerätezustand zentral verwaltet und bleibt dadurch für bestätigte Besuche konsistent. Änderungen am Besuchsstatus wirken direkt auf die Erinnerungsplanung, sodass veraltete Erinnerungen vermieden werden. Die Umsetzung wird an eine serverseitige Planungs- und Rückzugslogik gebunden. Reine clientseitige oder nur lokal terminierte Erinnerungen genügen dafür nicht.

## Betrachtete Alternativen

Erkennbar verworfen ist eine lokale, geräteabhängige Planung der Erinnerung, da die Zustellung nicht von lokaler Verfügbarkeit oder Hintergrundrestriktionen abhängen darf.

## Verwandte Core-Items

- REQ-88 — Angehörige sollen einmal am Vortag um 18 Uhr per Push-Mitteilung an ihren bestätigten Besuch erinnert werden, damit Besuche nicht vergessen werden.
- REQ-87 — Pflegende der jeweiligen Einrichtung müssen angekündigte Besuche bestätigen oder ablehnen können; bei Ablehnung ist eine kurze Begründung Pflicht. Angehörige sehen zu ihrer Ankündigung genau einen Status: angefragt, bestätigt oder abgelehnt.
- ARCH-46 — Für Push-Benachrichtigungen an Angehörige und Pflegende ist Firebase Cloud Messaging als technische Umsetzung festgelegt.

