# Architecture

## Zielbild und Scope

- Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere für Betreuer und andere Personen, die die Kommunikationsweise einer Person noch nicht kennen. [canon_support_understanding_residents, canon_app_as_knowledge_lookup]

- Eine digitale Lösung wird hierfür als bevorzugtes Zielbild gesucht; dieses Zielbild ist gewünscht, aber noch nicht abschließend spezifiziert. [canon_app_goal_digital_comm_support]

- Die Lösung ist als Wissensspeicher bzw. Nachschlagewerk für Kommunikationsweisen gedacht, in dem Wissen aus Akten und Erfahrungen festgehalten wird, damit bei Verständnisproblemen nachgeschaut werden kann. [canon_app_as_knowledge_lookup]

- Ein System, das als Übersetzungs-Schnittstelle zwischen Bewohnern und Betreuern fungiert, gehört ausdrücklich nicht zum Lösungsumfang. [canon_exclude_translation_system]

- Ob die App vollständige Dokumentation übernehmen soll, ist noch mit der Leitung zu klären; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]

## Nutzer, Zugriff und Berechtigungen

- Die App ist nur für den internen Gebrauch vorgesehen. [canon_internal_use_only]

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. Es gibt nur Login ohne Selbstregistrierung; nur Admins dürfen weitere Accounts anlegen und Rechte verwalten; User dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen. [canon_role_based_accounts_and_access_control]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen. [canon_employee_and_relative_access]

- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen – insbesondere Zugriff auf About Me und das Hinzufügen eigener Bilder – erhalten soll; dies ist als gewünschte Ausprägung vorgesehen. [canon_resident_account_with_restrictions]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Informationsarchitektur und Hauptfunktionen

- Nach dem Login soll eine Profilübersichtsseite mit einer Liste aller für den Nutzer zugänglichen Profile angezeigt werden. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, um Profile schnell zu finden. [canon_search_profiles]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein, die auch ein Einstellungssymbol zur Navigation in die Einstellungen enthält. [canon_consistent_appbar_after_login]

- Pro Person soll eine About-Me-Seite bereitgestellt werden, die einen persönlichen Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden statt als eigener Screen zu bestehen. [canon_video_support_in_communication]

- Pro Bewohner soll eine No-Go-Seite mit kritischen Dingen vorhanden sein, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]

- Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, bleibt wegen Vertraulichkeit und Umfang offen und muss geklärt werden. [canon_calendar_and_medication_open]

## Inhaltspflege und Darstellungsprinzipien

- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Für Videoinhalte soll dieselbe dynamische Hinzufügung gelten; neue Videos mit Beschreibungen sollen hinzugefügt werden können und neueste Inhalte oben erscheinen. [ADJ-GAP-AU-0053]

- Die No-Go-Seite soll ebenfalls dynamisch erweiterbar sein, sodass neue No-Gos hinzugefügt werden können. [ADJ-GAP-AU-0055]

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss noch ein standardisiertes Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]

## Datenschutz und Informationsquellen

- Für Bewohner existieren Akten bzw. dokumentierte Informationen, die als relevante Wissensquelle für die Lösung dienen können. [canon_resident_records_exist]

- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden und Wissen wird daher teilweise mündlich an neue Mitarbeiter weitergegeben. Dies ist eine relevante Rahmenbedingung für die Ausgestaltung der Lösung. [canon_existing_docs_hard_to_use]

- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

## Qualitätsanforderungen und Plattform

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

- Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]

- Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, gewünschte Ergänzung vorgesehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbarer Hilfebereich. [canon_help_tutorial_features]

## Technische Festlegungen und offene Technikpunkte

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]

- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]

- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]

## Delivery- und Einführungsrelevanz

- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; dieser Nutzenfall ist MVP-relevant. [canon_new_staff_need_support]