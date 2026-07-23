# Requirements

## Funktionale Anforderungen

- Die Lösung soll als digitale App realisiert werden, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern, wobei dieser digitale Ansatz bevorzugt wird, auch wenn der genaue MVP-Umfang noch offen ist. [canon_app_goal_digital_comm_support]

- Die Lösung soll neue Mitarbeiter insbesondere darin unterstützen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar machen. [canon_new_staff_need_support]

- Für jeden Bewohner soll eine About-Me-Seite bereitgestellt werden, die einen persönlichen Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, so dass neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt, sondern auch visuelle Darstellungen wie Bilder unterstützt werden. [canon_non_textual_representation]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen, wobei die Videofunktion in die verbalen und nonverbalen Kommunikationsseiten integriert wird und keinen eigenen Screen bildet. [canon_video_support_in_communication]

- Nach dem Login soll eine Profilübersichtsseite mit einer Suchfunktion zur schnellen Auffindbarkeit aller für den Nutzer zugänglichen Profile angezeigt werden. [canon_profile_overview_after_login, canon_search_profiles]

- Die Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Die App soll pro Bewohner eine No-Go-Seite enthalten, die kritische Dinge aufführt, die in Gegenwart des Bewohners zu vermeiden sind; diese Seite soll dynamisch erweiterbar sein und praxisnahe Beispiele enthalten. [canon_no_go_page, ADJ-GAP-AU-0055]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen. [canon_employee_and_relative_access]

- Die App soll rollenbasierte Benutzerkonten mit differenzierten Zugriffsrechten unterstützen: Es gibt nur Login ohne Selbstregistrierung, Admins dürfen weitere Accounts anlegen und Rechte verwalten, User dürfen Inhalte hinzufügen, jedoch keine Accounts anlegen oder Daten löschen. [canon_role_based_accounts_and_access_control]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar mit einem Einstellungssymbol zur Navigation in die Einstellungen vorhanden sein. [canon_consistent_appbar_after_login]

- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen und eingeschränkte Funktionen, insbesondere Zugriff auf About Me und Hinzufügen eigener Bilder, nutzen kann, um Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

## Nicht-funktionale Anforderungen

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Design und Bedienoberfläche sollen barrierefrei gestaltet sein, mit besonderem Fokus auf große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt. [canon_flutter_dart_stack]

- Die App ist ausschließlich für den internen Gebrauch vorgesehen. [canon_internal_use_only]

## Offene und zu klärende Anforderungen

- Ob die App vollständige Dokumentation übernehmen und anzeigen soll, ist offen und muss mit der Leitung geklärt werden; der Umfang der Dokumentation soll begrenzt bleiben, um den Fokus auf unterstützende Kommunikation zu bewahren. [canon_full_documentation_scope_open]

- Datenschutzrechtliche Fragen und die Zustimmung von Angehörigen für die Nutzung von Bildern in der App (testweise oder produktiv) müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

- Technische und organisatorische Aspekte der Berechtigungsimplementierung sollen klären, dass der Zugriff auf Profile einrichtungsbezogen beschränkt wird, sodass Mitarbeitende nur Profile ihres eigenen Hauses sehen können. [canon_cross_facility_access_restricted]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies technisch umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

- Die konkrete Ausgestaltung einer Admin-Seite zur Account- und Rechteverwaltung ist optional und offen. [canon_admin_screen_open]

- Firebase Firestore ist als Datenbankansatz vorgesehen; Details zur lokalen Speicherung und Zwischenspeicherung der Cloud-Daten müssen noch definiert werden. [canon_firebase_with_local_cache_open]

- Der Einsatz von GetX als technischer Rahmen ist zu prüfen und noch unentschieden. [canon_getx_consideration]

- Ob Zugang zu Bewohnerakten für Analyse, Tests oder Befüllung der App-Inhalte möglich ist, muss aufgrund Datenschutz noch geklärt werden. [canon_records_access_privacy_open]

- Ein Kalender mit Terminen soll vorgesehen sein; die Integration von Medikamentengaben steht offen und muss noch entschieden werden. [canon_calendar_and_medication_open]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; hierzu ist die Definition eines standardisierten Beschreibungsmusters für Kommunikations-Einträge erforderlich. [canon_search_communication_entries_with_pattern]

- UI-Details zum Login-Screen (E-Mail- und Passwortfelder, keine Registrierung) sind bereits konkretisiert, einzelne Details sowie Branding-Ideen sind offen und gewünscht. [ADJ-GAP-AU-0117, ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]

- Die profilbezogene Detailansicht beim Öffnen eines Profils soll eine vergrößerte Darstellung des Profilbildes und Navigationsbuttons zu den vier Hauptbereichen der App enthalten; diese Umsetzung ist noch als Wunsch formuliert. [ADJ-GAP-AU-0124]

- Die visuelle Ausgestaltung der getrennten Bereiche verbale und nonverbale Kommunikation ist noch weiter zu konkretisieren. [ADJ-GAP-AU-0126]

- Im Kommunikationsbereich soll die Unterstützung von Videos mit Beschreibungen um ein Plus-Button UI-Element für Hinzufügungen ergänzt werden, ebenso ist die Suchfunktion dort weiter zu planen. [ADJ-GAP-AU-0127]

- Die Gestaltung und Kompaktheit der No-Go-Seite sind weiter auszugestalten. [ADJ-GAP-AU-0128]

- Die Profilübersicht soll neben Suchleiste und Listenansicht auch die Möglichkeit bieten, über ein Plus-Symbol mit Eingabedialog neue Profile mit Bild, Name und Beschreibung anzulegen. [ADJ-GAP-AU-0123, ADJ-GAP-AU-0122, ADJ-GAP-AU-0121]

## Prozess- und Entwicklungsanforderungen

- Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden, einschließlich Besuchen mehrerer Einrichtungen, um ein breiteres Verständnis sicherzustellen. [canon_requirements_analysis_with_users]

- Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um Integrationsprobleme zu vermeiden. [canon_align_versions]

- Alle Teammitglieder sollen die Entwicklungsumgebung installieren und testen; detaillierte Programmier- und Kommentierungsregeln sollen in einem nächsten Teamtreffen festgelegt werden. [ADJ-GAP-AU-0113]

- Die Nutzung von Android Studio als Entwicklungsumgebung wird bevorzugt, ist aber noch ungeklärt. [canon_android_studio_preferred]

- Die initiale Entwicklung und das Testen sollen voraussichtlich gegen Android 11 erfolgen; dies ist ein Wunsch, aber noch nicht entschieden. [canon_develop_for_android11_initially]

- Bei der Planung soll über Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer nachgedacht werden. [canon_alternative_input_consideration]

- Animationen sollen nicht prioritär umgesetzt werden; falls vorhanden, dürfen sie nicht ablenken. [canon_animations_not_priority]

- Bei neuen Inhalten im About-Me-Bereich sollen verbundene Nutzer derselben Einrichtung per Popup-Benachrichtigung informiert werden; dies ist eine optionale Anforderung für spätere Versionen. [canon_about_me_notifications]

- Die App soll als Wissensdatenbank die Kommunikationsweise einer Person aus Akten und Erfahrungen festhalten, damit Nutzer bei Verständnisproblemen nachschlagen können; dies ist ein gewünschtes Feature mit noch unklarem MVP-Status. [canon_app_as_knowledge_lookup]

- UI-Details zum Login-Screen und Branding-Ideen sind teils offen und als Wunsch gekennzeichnet. [ADJ-GAP-AU-0117]

---

*Alle Anforderungen wurden ausschließlich aus den vorliegenden, freigegebenen Claims abgeleitet und mit den entsprechenden Quellenkennungen versehen.*