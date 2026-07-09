# Architecture

## Zielbild und Scope

- Die Lösung soll primär als App bzw. digitale Lösung die Kommunikation verbessern, indem andere Personen – insbesondere Betreuer – Bewohner besser verstehen können und bei Verständnisproblemen Wissen über deren Kommunikationsweise nachschlagen können. [canon_app_goal_digital_comm_support, canon_support_understanding_residents, canon_app_as_knowledge_lookup]

- Ein Übersetzungssystem als Schnittstelle zwischen Bewohnern und Betreuern ist ausdrücklich nicht Teil des Lösungsumfangs. [canon_exclude_translation_system]

- Ob die App zusätzlich vollständige Dokumentation übernehmen soll, ist noch mit der Leitung zu klären; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]

## Nutzer, Zugriff und Berechtigungen

- Die App ist nur für den internen Gebrauch vorgesehen. [canon_internal_use_only]

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen: Es gibt nur Login ohne Selbstregistrierung; nur Admins dürfen weitere Accounts anlegen und Rechte verwalten; User dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen. [canon_role_based_accounts_and_access_control]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen. [canon_employee_and_relative_access]

- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen – insbesondere Zugriff auf About Me und Hinzufügen eigener Bilder – erhalten soll, um Fehlbedienungen zu minimieren; dies bleibt ein gewünschter Punkt. [canon_resident_account_with_restrictions]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Informationsarchitektur und Hauptfunktionen

- Nach dem Login soll eine Profilübersichtsseite mit einer Liste aller für den Nutzer zugänglichen Profile angezeigt werden. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, um Profile schnell zu finden. [canon_search_profiles]

- Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung möglich sein; die Ausgestaltung ist noch zu klären. [ADJ-GAP-AU-0123]

- Pro Person soll eine About-Me-Seite mit persönlichem Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter bereitgestellt werden. [canon_about_me_page]

- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Die About-Me-Inhalte sollen um neue Einträge erweiterbar sein; für Fotos ist eine Sortierung neuer Inhalte nach oben als gewünschte Ausgestaltung benannt. [ADJ-GAP-AU-0125]

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein systematisches Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden statt als eigener Screen zu bestehen. [canon_video_support_in_communication]

- Videoeinträge sollen dynamisch hinzufügbar sein; neueste Inhalte sollen oben angezeigt werden. [ADJ-GAP-AU-0053]

- Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]

- Die No-Go-Seite soll dynamisch erweiterbar sein, sodass neue No-Gos hinzugefügt werden können. [ADJ-GAP-AU-0055]

- Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, bleibt wegen Vertraulichkeit und Umfang offen und muss geklärt werden. [canon_calendar_and_medication_open]

## Navigation und UI-Struktur

- Der Login-Screen soll nur Login vorsehen, nicht Registrierung. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]

- Der Login-Screen soll E-Mail-Feld, Passwort-Feld und Login-Button enthalten. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein, die auch ein Einstellungssymbol zur Navigation in die Einstellungen enthält. [canon_consistent_appbar_after_login]

- Für den Settings-Screen ist eine rollenabhängige Ansicht vorgesehen: Admins können dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen; jeder Nutzer soll sein Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]

## Daten und Inhalte

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen. [canon_new_staff_need_support]

## Datenschutz und offene Klärungen

- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

## Plattform und technische Festlegungen

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]

- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]

- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]

## Qualitätsanforderungen

- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

## Spätere bzw. optionale architektonische Punkte

- Eine Hilfe- bzw. Tutorial-Funktion ist als gewünschte spätere Ergänzung vorgesehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbarer Hilfebereich. [canon_help_tutorial_features]

- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist als später möglicher Punkt benannt. [canon_about_me_notifications]