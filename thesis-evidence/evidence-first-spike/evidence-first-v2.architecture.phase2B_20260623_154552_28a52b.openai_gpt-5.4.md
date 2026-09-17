# Architecture

## Zugangs- und Sichtbarkeitsmodell

- Die Architektur muss ein geschlossenes Zugangsmodell vorsehen: Zugriff auf die App nur per Login mit zuvor zugewiesenem Account; eine offene Selbstregistrierung ist nicht vorgesehen.
- Die Architektur muss für Bewohner-Accounts eine strikte Profilisolation vorsehen, sodass ein Bewohner ausschließlich das eigene Profil einsehen kann.
- Die Architektur muss für Mitarbeiter eine einrichtungsbezogene Zugriffstrennung vorsehen, sodass Profile nicht einrichtungsübergreifend sichtbar sind.
- Die konkrete technische Umsetzung der einrichtungsbezogenen Zugriffstrennung ist noch offen und muss architektonisch ausgearbeitet werden.

## Datenschutz- und Compliance-Rahmen

- Falls Bilder von Bewohnern in der App verarbeitet oder angezeigt werden, muss die Architektur die noch zu klärende Einwilligungs- bzw. Datenschutzlage berücksichtigen; diese Voraussetzung ist derzeit offen.
- Ein Zugriff des Projektteams auf bestehende Bewohnerakten ist aus Datenschutzgründen derzeit unklar und darf architektonisch nicht als gesichert verfügbar vorausgesetzt werden.
- Die Architektur steht unter der Projektgrenze, dass Datenschutz vorerst nicht vollständig im Detail ausgearbeitet wird; daraus ergibt sich ein verbleibendes Datenschutzrisiko.

## Ausgeschlossene Lösungsart

- Die Architektur darf kein System als Zielbild vorsehen, das bidirektional zwischen Bewohner und Betreuer übersetzt; diese Lösungsart ist ausdrücklich ausgeschlossen.

## Fachliche Struktur mit Architekturwirkung

- Die App-Struktur umfasst pro Profil einen eigenen Kalender-Bereich/Bildschirm neben About Me, Kommunikation und Video.
- Für den Kalender ist architektonisch eine klare Monatsansicht mit anklickbaren Tagen und zugehörigen Einträgen zu unterstützen.
- Medikationsinformationen im Kalender sind als vertraulicher als normale Termine zu behandeln und benötigen in der Architektur besondere Berücksichtigung des Schutzbedarfs.
- Popup-Benachrichtigungen bei neuen Inhalten im About-Me-Bereich für verbundene Nutzer derselben Einrichtung sind als optionale spätere Erweiterung zu berücksichtigen, nicht als verpflichtender MVP-Bestandteil.
