# Architecture

## Zielbild und Scope

- Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere aus Sicht anderer Personen wie Betreuer. [canon_support_understanding_residents]

- Die bevorzugte Lösungsrichtung ist eine digitale Lösung zur Verbesserung und Förderung der Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen; dieses Zielbild bleibt ein gewünschter Ansatz und ist nicht bereits vollständig verbindlich spezifiziert. [canon_app_goal_digital_comm_support]

- Die Lösung ist als App gedacht, die Wissen über die Kommunikationsweise einer Person aus Akten und Erfahrungen festhält, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]

- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen. [canon_new_staff_need_support]

- Ein System, das als Übersetzungs-Schnittstelle zwischen Bewohnern und Betreuern fungiert, gehört ausdrücklich nicht zum Lösungsumfang. [canon_exclude_translation_system]

- Ob die App vollständige Dokumentation übernehmen soll, ist noch mit der Leitung zu klären; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]

## Nutzer, Rollen und Zugriff

- Die App ist ausschließlich für den internen Gebrauch vorgesehen. [canon_internal_use_only]

- Die App muss rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen: Es gibt nur Login ohne Selbstregistrierung; nur Admins dürfen weitere Accounts anlegen und Rechte verwalten; User dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen. [canon_role_based_accounts_and_access_control]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen. [canon_employee_and_relative_access]

- Zusätzlich ist ein Bewohner-Account als gewünschte Ausprägung vorgesehen; dieser soll nur das eigene Profil sehen und nur eingeschränkte Funktionen erhalten, insbesondere Zugriff auf About Me und das Hinzufügen eigener Bilder, um Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Informationsarchitektur und Hauptfunktionen

- Nach dem Login soll eine Profilübersichtsseite mit einer Liste aller für den Nutzer zugänglichen Profile angezeigt werden. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, um Profile schnell zu finden. [canon_search_profiles]

- Auf der Profilübersichtsseite ist das Anlegen neuer Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung noch technisch/fachlich zu klären. [ADJ-GAP-AU-0123]

- Pro Person soll eine About-Me-Seite bereitgestellt werden, die einen persönlichen Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden statt als eigener Screen zu bestehen. [canon_video_support_in_communication]

- Für Videoinhalte ist dieselbe dynamische Hinzufügung und Sortierung mit neuesten Inhalten oben vorgesehen. [ADJ-GAP-AU-0053]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss noch ein standardisiertes Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]

- Pro Bewohner soll es eine No-Go-Seite mit kritischen Dingen geben, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]

- Die No-Go-Seite soll dynamisch erweiterbar sein, sodass neue No-Gos per Plus-Button ergänzt werden können. [ADJ-GAP-AU-0055]

- Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, bleibt wegen Vertraulichkeit und Umfang offen und muss geklärt werden. [canon_calendar_and_medication_open]

## Navigation und UI-Rahmen

- Der Login-Screen soll nur Login und keine Registrierung vorsehen. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]

- Der Login-Screen soll E-Mail-Feld, Passwort-Feld und Login-Button enthalten. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein, einschließlich eines Einstellungssymbols zur Navigation in die Einstellungen. [canon_consistent_appbar_after_login]

- Der Settings-Bereich soll rollenabhängige Unterschiede unterstützen: Admins können dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen; jeder Nutzer soll das eigene Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]

## Nichtfunktionale Anforderungen

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

- Beim Design muss auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

## Datenschutz und offene Klärungen

- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

## Technische Festlegungen und technische offene Punkte

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]

- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]

- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]

## Kontext und Risiken

- Für Bewohner existieren Akten bzw. dokumentierte Informationen, die als relevante Wissensquelle für die Lösung dienen können. [canon_resident_records_exist]

- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden und Wissen wird stattdessen mündlich an neue Mitarbeiter weitergegeben. [canon_existing_docs_hard_to_use]