# ADR-0015: Dynamische Foto-Timeline in About Me

Status: accepted
Core-Item: ARCH-20

## Kontext

Fuer die About-Me-Ansicht wurde festgelegt, dass sie eine dynamisch erweiterbare Foto-Timeline mit Beschreibungen bereitstellt. Neue Eintraege sollen per Plus-Button hinzugefuegt werden koennen und die neuesten Eintraege oben erscheinen. Diese Vorgaben konkretisieren die Bedien- und Inhaltsstruktur des Bereichs.

## Entscheidung

Wir setzen die About-Me-Ansicht als dynamisch erweiterbare Foto-Timeline mit Beschreibungen um; neue Eintraege werden per Plus-Button hinzugefuegt und neueste Eintraege oben angezeigt.

## Konsequenzen

About Me unterstuetzt fortlaufend erweiterbare bildgestuetzte Informationen in einer klaren zeitlich geordneten Darstellung. Das vereinfacht das Hinzufuegen neuer Inhalte und macht aktuelle Eintraege schnell sichtbar. Gleichzeitig ist die Umsetzung an diese Interaktions- und Sortierlogik gebunden.

## Verwandte Core-Items

- REQ-21 — Die About-Me-Ansicht muss eine Foto-Timeline mit Beschreibungen bereitstellen.
- REQ-22 — Die Foto-Timeline muss dynamisch erweiterbar sein; neue Einträge müssen über einen Plus-Button hinzugefügt werden können.
- REQ-23 — Neu hinzugefügte Bilder müssen in der About-Me-Timeline automatisch ganz oben als neueste Einträge angezeigt werden.

