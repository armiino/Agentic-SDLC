# Requirements

## Funktional

- Die bestehende Gestaltung der About-Me-Seite darf nicht geändert werden und soll unverändert bleiben. [about-me-unveraendert]
- Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen. [no-go-stopp-symbol]
- No-Gos dürfen nicht global für alle Bewohner gelten, sondern müssen pro Bewohner geführt werden. [no-gos-pro-bewohner]
- Beim Anlegen von No-Gos muss eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl bereitstehen; aus dieser Vorlagen-Liste darf nichts automatisch global gelten. [no-go-vorlagenliste]
- Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können. [no-go-bearbeiten-loeschen]
- Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden. [sofortinfo-mindestens-eins]
- Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird. [uebergabe-notiz-pro-schicht]
- Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein, sodass direkt auf ein Bewohnerprofil verwiesen werden kann. [uebergabe-bewohnerverlinkung]
- Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden; archivierte Notizen müssen weiterhin auffindbar bleiben. [uebergabe-archivierung]
- Es soll eine Suche über alle Bewohnerprofile nach Stichworten geben. [globale-sucheinwohnerprofile]

## Nicht-funktional

- Der Sofortinfo-Bereich muss auf maximal fünf Einträge begrenzt werden. [sofortinfo-max-fuenf]
- Bei konkurrierenden Offline-Änderungen am selben Eintrag müssen Konflikte angezeigt werden; es darf nichts stillschweigend überschrieben werden. [offline-konfliktanzeige]
- Eine einstellbare Schriftgröße ist als Wunsch vorgesehen und kann später berücksichtigt werden; sie hat keine hohe Priorität. [schriftgroesse-wunsch]
- Ein Dunkelmodus ist als Wunsch vorgesehen, insbesondere für den Nachtdienst; der Zeithorizont ist unklar und die Priorität ist niedrig. [dark-mode-wunsch]

## Compliance und Zugriff

- Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden, einschließlich wer wann welches Profil angesehen hat. [zugriffsprotokoll-pflicht]
- Das Zugriffsprotokoll darf nur von Admins eingesehen werden, nicht von normalen Nutzern. [zugriffsprotokoll-nur-admins]

## Constraints und Kontext

- Als Mindestanforderung soll Android 10 oder höher unterstützt werden; die Auswirkungen auf ältere Geräte sollen noch mit dem Träger geklärt werden. [android10-mindestanforderung]
- Firebase Firestore ist als Datenbanktechnologie festgelegt; Alternativen werden nicht weiter diskutiert. [firestore-fixiert]

## Offene Klärungen

- Es muss geklärt werden, ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen; die Klärung erfolgt mit der Datenschutzbeauftragten. [angehoerige-schreibrechte-offen]
- Der Wunsch, Piktogramme größer zu denken, ist noch zu unkonkret und muss später präzisiert werden. [piktogramme-unklar]