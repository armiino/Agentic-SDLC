# ADR-0005: Verzicht auf separaten Message-Broker für interne Events

Status: accepted
Core-Item: ARCH-49

## Kontext

Für die interne Event-Verarbeitung des aktuellen Systems stand die Festlegung an, ob ein separater externer Message-Broker eingesetzt werden soll oder ob Ereignisse direkt im Server-Backend verarbeitet werden. Laut Item-Text wurde diese Frage bewusst entschieden: Im aktuellen Systemzuschnitt besteht kein nachgewiesener Bedarf für einen separaten Broker. Als mögliche Auslöser für eine spätere Einführung werden insbesondere Lastspitzen oder systemübergreifende Ereignisverarbeitung genannt.

## Entscheidung

Wir verarbeiten interne Events direkt im Server-Backend und setzen dafür aktuell keinen separaten Message-Broker ein. Einen externen Broker führen wir erst ein, wenn dafür ein nachweisbarer Bedarf besteht, insbesondere bei Lastspitzen oder systemübergreifender Ereignisverarbeitung.

## Konsequenzen

Positiv ist, dass die Architektur im aktuellen Stand einfacher bleibt und keine zusätzliche Broker-Infrastruktur betrieben, integriert und überwacht werden muss. Die Event-Verarbeitung bleibt eng am Server-Backend und kann ohne zusätzliche verteilte Infrastruktur umgesetzt werden. Einschränkend folgt daraus, dass die interne Event-Verarbeitung zunächst an das Server-Backend gebunden ist und keine entkoppelte Broker-Schicht bereitsteht. Wenn Lastspitzen auftreten oder Ereignisse systemübergreifend verarbeitet werden sollen, muss die Architektur überprüft und gegebenenfalls um einen externen Broker erweitert werden.

## Betrachtete Alternativen

Als erkennbare Alternative stand der Einsatz eines separaten externen Message-Brokers für interne Events im Raum; diese Alternative wird aktuell bewusst nicht gewählt und erst bei nachweisbarem Bedarf erneut betrachtet.

## Verwandte Core-Items

- ARCH-49 — Für interne Events wird bewusst kein separater Message-Broker eingesetzt. Events laufen direkt im Server-Backend. Ein externer Broker wird erst eingeführt, wenn dafür ein nachweisbarer Bedarf besteht, insbesondere bei Lastspitzen oder systemübergreifender Ereignisverarbeitung.

