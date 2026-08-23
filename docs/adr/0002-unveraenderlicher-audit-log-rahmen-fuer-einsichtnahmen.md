# ADR-0002: Unveränderlicher Audit-Log-Rahmen für Einsichtnahmen

Status: accepted
Core-Item: ARCH-47

## Kontext

Die revisionssichere Protokollierung von Einsichtnahmen musste konkret festgelegt werden. Das zugrunde liegende Design-Item beschreibt als bindenden Rahmen, dass Zugriffsereignisse serverseitig protokolliert werden, nur anhängend erfasst werden dürfen und nachträgliche Änderungen oder Löschungen fachlich und technisch ausgeschlossen sind. Zusätzlich ist festgelegt, dass das Leserecht auf diese Protokolle strikt auf Admins beschränkt bleibt. Damit wird die Umsetzung der revisionssicheren Einsichtnahme-Protokollierung unmittelbar fachlich und technisch eingegrenzt.

## Entscheidung

Wir nutzen für Einsichtnahmen einen serverseitigen, append-only Audit-Log. Wir schließen nachträgliche Änderungen und Löschungen der Protokolle fachlich und technisch aus. Wir beschränken das Leserecht auf diese Protokolle strikt auf Admins.

## Konsequenzen

Die Protokollierung von Einsichtnahmen wird revisionssicher und zentral nachvollziehbar. Manipulationen an Protokolleinträgen werden ausgeschlossen, was Nachweisbarkeit und Kontrolle stärkt. Die Umsetzung muss jedoch serverseitige Erfassung und unveränderliche Speicherung sicherstellen. Einsicht in die Protokolle ist organisatorisch und technisch an die Admin-Rolle gebunden; normale Nutzer erhalten keinen Zugriff. Betriebs- und Supportprozesse müssen diese restriktive Zugriffsregel berücksichtigen.

## Verwandte Core-Items

- REQ-64 — Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
- REQ-65 — Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer dürfen dieses Protokoll nicht einsehen.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.

