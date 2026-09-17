# Architektur

## Zugangs- und Berechtigungsmodell

- Die Architektur muss ein geschlossenes Zugangsmodell unterstützen: Zugriff auf die App ist nur per Login mit zuvor zugewiesenem Account möglich; eine offene Selbstregistrierung ist ausgeschlossen.
- Die Architektur muss Autorisierung so durchsetzen, dass ein Bewohner-Account ausschließlich das eigene Profil sehen kann.
- Die Architektur muss die Sichtbarkeit für Mitarbeiter auf die jeweils zuständige Einrichtung begrenzen; einrichtungsübergreifender Zugriff auf Profile ist nicht zulässig.
- Die konkrete technische Umsetzung der einrichtungsbezogenen Zugriffsbeschränkung ist noch offen und muss in der Architektur weiter ausgearbeitet werden.

## Datenschutz- und Compliance-Rahmen

- Falls Bilder von Bewohnern in der App verwendet werden, muss die Architektur die noch zu klärenden Datenschutz- bzw. Einwilligungsanforderungen berücksichtigen; die Einwilligungslage ist derzeit offen.
- Der Zugriff des Projektteams auf bestehende Bewohnerakten ist aus Datenschutzgründen unklar; die Architektur soll nicht voraussetzen, dass solcher Zugriff verfügbar ist.
- Die Architektur muss berücksichtigen, dass Datenschutz im Projekt vorerst nicht vollständig im Detail gelöst wird und damit Restrisiken bzw. spätere Vertiefungen bestehen.

## Ausgeschlossene Lösungsart

- Die Architektur darf nicht auf einem System basieren, das zwischen Bewohner und Betreuer bidirektional übersetzt.

## Struktur des Profilbereichs

- Die Architektur muss den Kalender als eigenen App-Bereich bzw. Bildschirm pro Profil vorsehen, neben About Me, Kommunikation und Video.
- Für den Kalender ist eine einfache, übersichtliche Monatsansicht mit anklickbaren Tagen und Einträgen architektonisch zu unterstützen.

## Kalender und vertrauliche Inhalte

- Falls im Kalender Medikamentengaben oder -erinnerungen abgebildet werden, muss die Architektur einen erhöhten Schutzbedarf für Medikationsinhalte gegenüber normalen Terminen berücksichtigen.

## Optionale spätere Erweiterung

- Optional und für später möglich kann die Architektur Benachrichtigungen unterstützen, wenn im About-Me-Bereich neue Inhalte hochgeladen werden und verbundene Nutzer derselben Einrichtung informiert werden sollen.