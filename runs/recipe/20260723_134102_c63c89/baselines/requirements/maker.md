# Requirements

## Funktional

- No-Go-Einträge müssen pro Bewohner geführt werden und dürfen nicht global für alle Bewohner gelten. [no-go-pro-bewohner-nicht-global]
- Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal erhalten, damit sofort erkennbar ist, dass die Inhalte kritisch sind. Dieser Punkt ist noch offen. [no-go-warnsymbol]
- Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können. Dieser Punkt ist noch offen. [no-go-bearbeiten-loeschen]
- Für die Anlage von No-Gos kann optional eine Vorlagen-Liste mit häufigen No-Gos angeboten werden; daraus darf jedoch nichts automatisch global gelten. [no-go-vorlagenliste-optional]
- Der Sofortinfo-Bereich in der Profil-Detailansicht muss auf maximal fünf Einträge begrenzt werden. [sofortinfo-max-fuenf]
- Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein. [sofortinfo-mindestens-eins]
- Es muss eine Suche nach Stichworten über alle Bewohnerprofile hinweg geben. Der Umsetzungszeitpunkt ist noch nicht präzisiert. [suche-ueber-alle-bewohnerprofile]
- Es wird pro Schicht eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird. Dieser Punkt ist noch offen. [uebergabe-notizen-schichtworkflow]
- Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein, sodass direkt auf das Profil eines Bewohners verwiesen werden kann. Dieser Punkt ist noch offen. [uebergabe-notizen-bewohnerverlinkung]
- Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden; archivierte Notizen müssen jedoch weiterhin auffindbar bleiben. Dieser Punkt ist noch offen. [uebergabe-notizen-archivierung-und-auffindbarkeit]

## Datenzugriff und Compliance

- Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat. [zugriffsprotokoll-pflicht-revisionssicher-adminsicht]
- Einsicht in das Zugriffsprotokoll dürfen nur Admins haben; normale Nutzer dürfen dieses Protokoll nicht einsehen. [zugriffsprotokoll-pflicht-revisionssicher-adminsicht]
- Wenn zwei Personen denselben Eintrag offline geändert haben, muss eine klare Konfliktanzeige erfolgen. Es darf nichts stillschweigend überschrieben werden, und der Nutzer entscheidet, welche Version gilt. Der Umsetzungszeitpunkt ist noch nicht präzisiert. [offline-konfliktbehandlung]

## Scope und Änderungsgrenzen

- Die About-Me-Seite darf im MVP in ihrer aktuellen Form nicht verändert werden. [about-me-unveraendert]

## Offene Klärungen und Kontext

- Es muss mit der Datenschutzbeauftragten geklärt werden, ob Angehörige selbst Inhalte eintragen dürfen, zum Beispiel auf der About-Me-Seite, oder nur Leserechte erhalten. Bis dahin bleibt diese Rechtefrage unentschieden. [angehoerige-schreibrechte-offen]
- Für die Planung soll aktuell von einer Mindestunterstützung ab Android 10 ausgegangen werden; die finale Festlegung muss jedoch noch mit dem Träger geklärt werden. [android-10-mindestanforderung-offen]
- Falls im Haus Geräte unter Android 10 vorhanden sind, müssten diese ausgetauscht werden; dieses Risiko ist abhängig vom Gerätebestand und der Abstimmung mit dem Träger. [altgeraete-austausch-risiko]
- Zum Thema Piktogramme besteht ein noch nicht ausreichend konkretisierter Änderungswunsch; vor einer Umsetzung muss präzisiert werden, was mit „größer denken“ gemeint ist. [piktogramme-unklar]
- Firebase Firestore ist als Persistenztechnologie für die Lösung festgelegt; Alternativen werden nicht weiter diskutiert. [firestore-fixiert]
- Es ist unklar, ob aus dem Hinweis auf die anstehende Übergabe eine eigenständige fachliche Anforderung zur Unterstützung schneller Übergabeprozesse abgeleitet werden soll. [ADJ-GAP-AU-0002]
- Es ist unklar, welcher zuvor genannte Punkt aus Sicht der Angehörigen besonders wichtig ist und ob daraus ein eigener Claim folgt. [ADJ-GAP-AU-0026]
- Gegebenenfalls ist zu klären, ob Aufgabenverfolgung oder Terminplanung als eigene organisatorische Anforderungen an das Projekt festgehalten werden sollen. [ADJ-GAP-AU-0035]

## Optionale Wünsche

- Ein Dunkelmodus ist ein Wunsch, insbesondere für den Nachtdienst, hat jedoch keine hohe Priorität und ist eher später möglich. [dark-mode-wunsch]
- Eine einstellbare Schriftgröße ist ein Wunsch zur Unterstützung älterer Kolleginnen, hat jedoch keine hohe Priorität und ist eher später möglich. [schriftgroesse-einstellbar-wunsch]