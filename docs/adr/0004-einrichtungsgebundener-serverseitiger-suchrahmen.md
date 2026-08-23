# ADR-0004: Einrichtungsgebundener serverseitiger Suchrahmen

Status: accepted
Core-Item: ARCH-11

## Kontext

Fuer die Stichwortsuche ueber alle Bewohnerprofile musste festgelegt werden, wie die Einrichtungsgrenze technisch abgesichert wird. Laut Entscheidungs-Item duerfen Suchtreffer die Einrichtungsgrenze nicht umgehen. Die Begruendung ordnet dies als wesentliche Mandanten- und Zugriffsentscheidung ein, die klare Grenzen fuer Rechte- und Zugriffsumsetzung setzt.

## Entscheidung

Wir nutzen fuer die bewohnerprofiluebergreifende Stichwortsuche einen serverseitigen Suchrahmen, bei dem Suchindizes nur Inhalte der jeweils zugeordneten Einrichtung enthalten oder nur innerhalb dieser Einrichtung abfragbar sind. Dadurch werden Suchtreffer technisch auf die eigene Einrichtung begrenzt.

## Konsequenzen

Die Suche ueber Bewohnerprofile bleibt mit der Einrichtungszuordnung und dem Mandantenkonzept konsistent. Die technische Umsetzung von Rechten und Zugriffen erhaelt eine klare serverseitige Grenze, sodass Suchtreffer keine einrichtungsfremden Daten offenlegen koennen. Gleichzeitig bindet die Entscheidung die Implementierung von Indexierung und Suchabfragen an die Einrichtungszuordnung und schliesst globale oder einrichtungsuebergreifende Suchindizes beziehungsweise Abfragen ohne entsprechende Trennung aus.

## Betrachtete Alternativen

Im Material ist als technische Auspraegung erkennbar, dass Suchindizes entweder nur Inhalte der jeweils zugeordneten Einrichtung enthalten oder nur innerhalb dieser Einrichtung abfragbar sind.

## Verwandte Core-Items

- REQ-09 — Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf die Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten.
- REQ-60 — Es muss eine Suche nach Stichworten über alle Bewohnerprofile hinweg geben.

