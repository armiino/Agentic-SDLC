# Requirements

## Funktionale Anforderungen

- Die Lösung soll primär dazu dienen, dass andere Personen, insbesondere Betreuer, Bewohner besser verstehen können. [canon_support_understanding_residents]
- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen. [canon_new_staff_need_support]
- Die Lösung soll als App Wissen über die Kommunikationsweise einer Person aus Akten und Erfahrungen festhalten, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]
- Eine digitale Lösung zur Verbesserung und Förderung der Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen ist als bevorzugter Ansatz gewünscht; ob und in welchem Umfang dies umgesetzt wird, bleibt offen. [canon_app_goal_digital_comm_support]

### Zugriff, Login und Nutzerrollen

- Die App soll nur eine Login-Möglichkeit bereitstellen; eine Selbstregistrierung soll nicht möglich sein. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]
- Der Login-Screen soll ein Logo, ein E-Mail-Feld, ein Passwort-Feld und einen Login-Button enthalten. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]
- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. [canon_role_based_accounts_and_access_control]
- Nur Admins sollen weitere Accounts anlegen und Rechte verwalten dürfen. [canon_role_based_accounts_and_access_control]
- User sollen Inhalte hinzufügen dürfen, aber weder Accounts erstellen noch Daten löschen. [canon_role_based_accounts_and_access_control]
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen. [canon_employee_and_relative_access]
- Zusätzlich ist ein Bewohner-Account gewünscht, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen, insbesondere den Zugriff auf About Me und das Hinzufügen eigener Bilder, erhalten soll; ob und wann dies umgesetzt wird, bleibt unklar. [canon_resident_account_with_restrictions]

### Navigation und Übersichten

- Nach dem Login soll eine Profilübersichtsseite mit einer Liste aller für den Nutzer zugänglichen Profile angezeigt werden. [canon_profile_overview_after_login]
- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, um Profile schnell zu finden. [canon_search_profiles]
- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein, die ein Einstellungssymbol zur Navigation in die Einstellungen enthält. [canon_consistent_appbar_after_login]
- Es ist gewünscht, dass in der Appbar der Titel des aktuellen Screens angezeigt wird und dass unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste platziert wird; die genaue Ausgestaltung bleibt offen. [ADJ-GAP-AU-0121]
- Es ist gewünscht, dass Profile in der Profilübersicht als Kacheln oder Liste mit Vorschaubild, Name und kurzer Beschreibung angezeigt werden; die konkrete Darstellung bleibt offen. [ADJ-GAP-AU-0122]
- Es soll geklärt werden, ob auf der Profilübersichtsseite neue Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung angelegt werden können. [ADJ-GAP-AU-0123]
- Es ist gewünscht, dass beim Öffnen eines Profils eine Detailansicht mit vergrößertem Profilbild und Navigationsbuttons zu den vier Hauptbereichen angezeigt wird; die genaue Ausgestaltung bleibt offen. [ADJ-GAP-AU-0124]

### Profilinhalte

- Die App soll pro Person eine About-Me-Seite mit persönlichem Ersteindruck und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter bereitstellen. [canon_about_me_page]
- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]
- Es ist gewünscht, die About-Me-Seite mit einer Infobox für Angaben wie Alter und Hobbys sowie einer Foto-Timeline mit Plus-Button auszugestalten; die neuesten Fotos sollen oben angezeigt werden. Die genaue Ausgestaltung bleibt offen. [ADJ-GAP-AU-0125]
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist für später gewünscht. [canon_about_me_notifications]

### Kommunikationsinhalte

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]
- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]
- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen. [canon_video_support_in_communication]
- Die Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden statt als eigener Screen zu bestehen. [canon_video_support_in_communication]
- Auf der Videofunktion sollen neue Videos mit Beschreibungen hinzufügbar sein; das jeweils neueste Video soll oben angezeigt werden. [ADJ-GAP-AU-0053]
- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisiertes Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen. [canon_search_communication_entries_with_pattern]
- Es ist gewünscht, die visuelle Trennung zwischen verbaler und nonverbaler Kommunikation mit Symbolen zu kennzeichnen und für beide Bereiche klare Buttons zu weiteren Informationen vorzusehen; die konkrete Ausgestaltung bleibt offen. [ADJ-GAP-AU-0126]
- Es ist gewünscht, im nonverbalen Bereich einen Plus-Button zum Hinzufügen von Videos und Beschreibungen vorzusehen; die konkrete Ausgestaltung bleibt offen. [ADJ-GAP-AU-0127]

### No-Go-Informationen

- Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]
- Die No-Go-Seite soll dynamisch erweiterbar sein, sodass über einen Plus-Button weitere No-Gos hinzugefügt werden können. [ADJ-GAP-AU-0055]
- Es ist gewünscht, die No-Go-Seite mit einem starken visuellen Symbol und kompakten, leicht durchforstbaren Einträgen zu gestalten; die konkrete Ausgestaltung bleibt offen. [ADJ-GAP-AU-0128]

### Kalender und Einstellungen

- Ein Kalender mit Terminen soll berücksichtigt werden; ob auch Medikamentengaben integriert werden, muss wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]
- Aus den Einstellungen soll eine Navigation über das Einstellungssymbol in der Appbar erreichbar sein. [canon_consistent_appbar_after_login]
- Nutzer sollen ihr Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]

### Hilfe und Eingabeunterstützung

- Eine Hilfe- bzw. Tutorial-Funktion ist für später gewünscht, etwa als kurze Tour beim ersten Login und als erneut aufrufbarer Hilfebereich. [canon_help_tutorial_features]
- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]

## Nicht-funktionale Anforderungen

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]
- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]
- Die App soll nur für den internen Gebrauch vorgesehen sein. [canon_internal_use_only]
- Animationen sollen nicht aktiv eingeplant werden; falls dennoch welche verwendet werden, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]
- Für den Login-Screen ist ein klar wahrnehmbares Branding mit Logo gewünscht; das konkrete Logo ist noch offen. [ADJ-GAP-AU-0117]

## Constraints und Scope

- Ein System, das zwischen Bewohnern und Betreuern als Übersetzungs-Schnittstelle fungiert, darf nicht Teil des Lösungsumfangs sein. [canon_exclude_translation_system]
- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells soll noch geklärt werden. [canon_cross_facility_access_restricted]
- Ob die App vollständige Dokumentation übernehmen soll, ist offen und mit der Leitung zu klären; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]
- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

## Kontext und offene Klärungen mit Relevanz für die Anforderungen

- Für Bewohner existieren Akten bzw. dokumentierte Informationen als relevante Wissensquelle für die Lösung. [canon_resident_records_exist]
- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden daher oft nicht schnell gefunden und Wissen wird teilweise mündlich an neue Mitarbeiter weitergegeben. [canon_existing_docs_hard_to_use]
- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]
- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]
- Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen; daraus folgt jedoch kein festgelegter MVP-Umfang. [canon_digitization_desired_broadly]

## Prozess- und technische Rahmenbedingungen

- Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Verständnis zu gewinnen. [canon_requirements_analysis_with_users]
- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt. [canon_flutter_dart_stack]
- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]
- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden. [canon_getx_consideration]
- Das Team präferiert Android Studio als gemeinsame Entwicklungsumgebung; die Präferenz bleibt offen. [canon_android_studio_preferred]
- Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um Integrationsprobleme zu vermeiden. [canon_align_versions]
- Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden. [canon_develop_for_android11_initially]