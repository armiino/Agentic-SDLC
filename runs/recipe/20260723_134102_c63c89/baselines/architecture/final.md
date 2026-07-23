# Architecture

## Datenmodell & Fachliche Grenzen
- No-Gos sind strikt bewohnerbezogen und dürfen nicht global für alle Bewohner gelten. [no-go-pro-bewohner-nicht-global]
- Die About-Me-Seite bleibt im MVP in ihrer aktuellen Form unverändert. [about-me-unveraendert]
- Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden. [sofortinfo-mindestens-eins]
- Der Sofortinfo-Bereich in der Profil-Detailansicht ist auf maximal fünf Einträge zu begrenzen. [sofortinfo-max-fuenf]

## No-Go-Bereich
- Für die No-Go-Seite ist ein rotes Stopp-Symbol als deutliches Warnsignal vorgesehen; die konkrete Umsetzung ist jedoch noch nicht verbindlich entschieden. [no-go-warnsymbol]
- Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können; die konkrete Ausgestaltung ist noch offen. [no-go-bearbeiten-loeschen]

## Suche & Auffindbarkeit
- Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben; der Umsetzungszeitpunkt ist noch nicht präzisiert. [suche-ueber-alle-bewohnerprofile]

## Offline-Verhalten
- Wenn zwei Personen denselben Eintrag offline geändert haben, muss eine klare Konfliktanzeige erfolgen; nichts darf stillschweigend überschrieben werden, und der Nutzer entscheidet, welche Version gilt. [offline-konfliktbehandlung]

## Übergabe-Notizen
- Es wird pro Schicht eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird; die technische Ausarbeitung ist noch offen. [uebergabe-notizen-schichtworkflow]
- Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein; die technische Ausarbeitung ist noch offen. [uebergabe-notizen-bewohnerverlinkung]
- Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden und im Archiv weiterhin auffindbar bleiben; die Umsetzung ist noch auszuarbeiten. [uebergabe-notizen-archivierung-und-auffindbarkeit]

## Zugriff, Rollen & Compliance
- Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat. [zugriffsprotokoll-pflicht-revisionssicher-adminsicht]
- Einsicht in das Zugriffsprotokoll ist ausschließlich Admins erlaubt; normale Nutzer dürfen dieses Protokoll nicht einsehen. [zugriffsprotokoll-pflicht-revisionssicher-adminsicht]
- Ob Angehörige selbst Inhalte eintragen dürfen oder nur Leserechte erhalten, ist noch offen und muss mit der Datenschutzbeauftragten geklärt werden. [angehoerige-schreibrechte-offen]

## Plattform & Persistenz
- Für die Planung wird aktuell von einer Mindestunterstützung ab Android 10 ausgegangen; die finale Festlegung muss noch mit dem Träger geklärt werden. [android-10-mindestanforderung-offen]
- Firebase Firestore ist als Persistenztechnologie festgelegt; Alternativen werden nicht weiter betrachtet. [firestore-fixiert]