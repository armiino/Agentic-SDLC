# Requirements

## Funktionale Anforderungen

- Die Lösung soll primär dazu dienen, dass andere Personen, insbesondere Betreuer, Bewohner besser verstehen können. [canon_support_understanding_residents]

- Die Lösung soll als App Wissen aus Akten sowie Erfahrungswissen über die Art und Weise, wie eine Person kommuniziert, festhalten, sodass Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]

- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; neue Erfahrungen sollen dabei sofort geteilt werden können. [canon_new_staff_need_support]

- Die App soll neben Mitarbeitern auch Angehörigen Zugriff geben, damit diese ergänzende Daten und Wissen hinzufügen können. [canon_employee_and_relative_access]

- Nach dem Login soll die App eine Profilübersichtsseite mit einer Liste aller für den Nutzer zugänglichen Profile anzeigen; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, statt lange scrollen zu müssen. [canon_search_profiles]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; die Appbar soll ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist. [canon_consistent_appbar_after_login]

- Die App soll pro Person eine About-Me-Seite bereitstellen, die einen persönlichen Ersteindruck vermittelt und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Inhalte in Profil- und Kommunikationsbereichen sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Auf der Videofunktion soll das Hinzufügen neuer Videos mit Beschreibungen möglich sein; neu hinzugefügte Videos sollen oben angezeigt werden. [ADJ-GAP-AU-0053]

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen. [canon_video_support_in_communication]

- Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]

- Die No-Go-Seite soll dynamisch erweiterbar sein, sodass über einen Plus-Button weitere No-Gos hinzugefügt werden können. [ADJ-GAP-AU-0055]

## Login, Rollen und Zugriff

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. [canon_role_based_accounts_and_access_control]

- Die App soll nur eine Login-Möglichkeit bereitstellen; eine Selbstregistrierung soll nicht möglich sein. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]

- Nur ein Account mit Admin-Rechten soll weitere Accounts erstellen und Rechte verwalten dürfen. [canon_role_based_accounts_and_access_control]

- User-Accounts sollen Inhalte hinzufügen dürfen, aber keine Accounts erstellen und keine hinzugefügten Daten aus der App löschen können. [canon_role_based_accounts_and_access_control]

- Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen; eine Registrierung soll dort nicht angeboten werden. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]

- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen erhalten soll; insbesondere soll er auf den About-Me-Bereich zugreifen und dort eigene Bilder hinzufügen können, um das Risiko von Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

## Nicht-funktionale Anforderungen

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

## Constraints und Scope

- Die App ist nur für den internen Gebrauch vorgesehen. [canon_internal_use_only]

- Ein System, das zwischen Bewohnern und Betreuern als Übersetzungs-Schnittstelle fungiert, ist nicht Teil des Lösungsumfangs. [canon_exclude_translation_system]

## Offene Anforderungen und Klärungen

- Eine digitale Lösung, etwa ein Computerprogramm, ist der gewünschte Ansatz zur Verbesserung und Förderung der Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen; die Vision ist gewünscht, aber noch nicht verbindlich spezifiziert. [canon_app_goal_digital_comm_support]

- Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation übernehmen soll; der Umfang soll dabei begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation nicht verloren geht. [canon_full_documentation_scope_open]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

- Es muss evaluiert werden, ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist. [canon_tablet_support_open]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]

- Für die Profilübersichtsseite soll geklärt werden, ob neue Profile über ein Plus-Symbol und einen Eingabedialog mit Bild, Name und Beschreibung angelegt werden können. [ADJ-GAP-AU-0123]

- Für den Kalender ist ein einfacher und übersichtlicher Terminkalender vorgesehen; ob zusätzlich Medikamentengaben integriert werden, muss wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]

## Gewünschte oder optionale Ergänzungen

- Für Admins könnte eine separate Admin-Seite zur Account- und Rechteverwaltung vorgesehen werden; die konkrete Ausgestaltung ist noch offen. [canon_admin_screen_open]

- Die App soll eine Hilfe- bzw. Tutorial-Funktion vorsehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbaren Hilfebereich. [canon_help_tutorial_features]

- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden. [canon_about_me_notifications]

- Ein zusätzlicher Bewohner-Account ist gewünscht, falls er mit speziellen Rechten und stark eingeschränkten Funktionen umgesetzt wird. [canon_resident_account_with_restrictions]

- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]

- Animationen sollen nicht aktiv eingeplant werden; falls dennoch welche vorhanden sind, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]