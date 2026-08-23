# ADR-0006: Serverseitige Einrichtungsgrenze in Firestore

Status: accepted
Core-Item: ARCH-50

## Kontext

Ergänzend zum bereits festgelegten serverseitigen Suchrahmen über Einrichtungsgrenzen hinweg musste die allgemeine technische Durchsetzung der Einrichtungsgrenze für Firestore konkretisiert werden. Das vorliegende Design-Item legt dafür fest, dass jede Collection eine Einrichtungs-ID trägt und dass jede Firestore Security Rule diese ID gegen die dem angemeldeten Benutzer zugeordnete Einrichtungs-ID prüft. Laut Begründung handelt es sich um eine konkrete Architekturentscheidung zur mandantenfähigen serverseitigen Durchsetzung, die zugleich alle Arbeiten zur Benutzerzuordnung, zu Rollen und Rechten sowie zur fachlichen Einrichtungsgrenze einschränkt.

## Entscheidung

Wir nutzen in Firestore eine serverseitig erzwungene Einrichtungsgrenze, indem jede relevante Collection eine Einrichtungs-ID trägt und jede Firestore Security Rule diese Einrichtungs-ID gegen die dem angemeldeten Benutzer zugeordnete Einrichtungs-ID prüft.

## Konsequenzen

Die Einrichtungsgrenze wird technisch konsistent und serverseitig durchgesetzt und kann nicht allein durch Client-Logik umgangen werden. Die Lösung passt den bereits festgelegten Suchrahmen an eine allgemeine Mandantentrennung über Collections und Zugriffsregeln an. Zugleich bindet sie Datenmodell, Benutzerzuordnung und Regeldefinitionen: Collections müssen die Einrichtungs-ID zuverlässig mitführen, Authentifizierungs- und Accountdaten müssen die Benutzer einer Einrichtung eindeutig zuordnen, und jede Firestore Security Rule muss diese Prüfung explizit enthalten und pflegen. Fehler oder Lücken in dieser Zuordnung oder in einzelnen Regeln können unmittelbar zu unzulässigen Zugriffen oder zu blockierten legitimen Zugriffen führen.

## Verwandte Core-Items

- ARCH-11 — Für die einrichtungsgebundene Stichwortsuche über alle Bewohnerprofile muss ein serverseitiger Suchrahmen festgelegt werden: Suchindizes dürfen nur Inhalte der jeweils zugeordneten Einrichtung enthalten oder nur innerhalb dieser Einrichtung abfragbar sein, damit Suchtreffer die Einrichtungsgrenze technisch nicht umgehen.
- REQ-09 — Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur auf die Profile der Einrichtung, in der sie tätig sind; die technische Umsetzung dieser Beschränkung ist noch zu erarbeiten.
- REQ-84 — Jeder Account und jedes Bewohnerprofil ist genau einer Einrichtung zugeordnet; diese Zuordnung wird serverseitig erzwungen. Einladungscodes geben die Einrichtungszuordnung bei der Registrierung unveränderlich mit. Die Selbstregistrierung für Angehörige per Einladungscode bleibt bestehen; für alle anderen Rollen bleibt der Zugang ausschließlich über intern vergebene Accounts möglich.

