# Architecture

## Zielbild & Scope

- Die Lösung soll primär als digitale App die Kommunikation unterstützen, indem sie anderen Personen – insbesondere Betreuern und neuen Mitarbeitenden – hilft, Bewohner besser zu verstehen und bei Verständnisproblemen nachzuschlagen, wie eine Person kommuniziert. Dieses Zielbild ist gewünscht bzw. beschlossen, der konkrete Zuschnitt bleibt teilweise noch auszuarbeiten. [canon_app_goal_digital_comm_support, canon_support_understanding_residents, canon_app_as_knowledge_lookup, canon_new_staff_need_support]

- Ein Übersetzungssystem als Schnittstelle zwischen Bewohnern und Betreuern ist ausdrücklich nicht Teil des Lösungsumfangs. [canon_exclude_translation_system]

- Ob die App auch vollständige Dokumentation übernehmen soll, ist noch offen und mit der Leitung zu klären; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]

## Nutzer, Zugriff & Berechtigungen

- Die App ist nur für den internen Gebrauch vorgesehen. [canon_internal_use_only]

- Die App muss rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. Es gibt nur Login ohne Selbstregistrierung; nur Admins dürfen weitere Accounts anlegen und Rechte verwalten; User dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen. [canon_role_based_accounts_and_access_control]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzendes Wissen beitragen können. [canon_employee_and_relative_access]

- Es ist gewünscht, zusätzlich einen Bewohner-Account vorzusehen, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen – insbesondere Zugriff auf About Me und das Hinzufügen eigener Bilder – erhält, um Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeitende nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Informationsarchitektur & Navigation

- Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die alle für den Nutzer zugänglichen Profile auflistet. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite muss eine Suchfunktion vorhanden sein, um Profile schnell zu finden. [canon_search_profiles]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; diese enthält auch ein Einstellungssymbol zur Navigation in die Einstellungen. [canon_consistent_appbar_after_login]

- Rollenabhängige Einstellungen sind vorgesehen: Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen; jeder Nutzer soll sein Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]

- Auf der Profilübersichtsseite ist das Anlegen neuer Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung noch technisch/funktional auszuarbeiten. [ADJ-GAP-AU-0123]

## Profil- und Bewohnerinformationen

- Pro Person muss eine About-Me-Seite bereitgestellt werden, die einen persönlichen Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Profil- und Kommunikationsinhalte müssen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Pro Bewohner soll eine No-Go-Seite mit kritischen Dingen vorhanden sein, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]

- Die No-Go-Seite muss dynamisch erweiterbar sein, sodass weitere No-Gos hinzugefügt werden können. [ADJ-GAP-AU-0055]

## Kommunikationswissen

- Kommunikationsinformationen müssen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Die App muss Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion ist in die verbalen und nonverbalen Kommunikationsseiten zu integrieren statt als eigener Screen zu bestehen. Der genaue Umsetzungszeitpunkt bleibt unklar. [canon_video_support_in_communication]

- Für Videoinhalte ist dieselbe dynamische Hinzufügung und Sortierung neuester Inhalte nach oben vorgesehen. [ADJ-GAP-AU-0053]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss noch ein standardisiertes Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]

## Datenschutz & Datenzugang

- Datenschutz und Zustimmung der Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

## Termine & weitere Funktionsbereiche

- Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, bleibt wegen Vertraulichkeit und Umfang offen und muss geklärt werden. [canon_calendar_and_medication_open]

## Plattform & technische Leitplanken

- Die App muss plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]

## Qualitätsanforderungen

- Beim Design muss auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

- Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, gewünschte Ergänzung vorgesehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbarer Hilfebereich. [canon_help_tutorial_features]