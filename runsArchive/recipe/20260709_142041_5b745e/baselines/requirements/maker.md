# Requirements

## Funktionale Anforderungen

- Die Lösung soll primär dazu dienen, dass andere Personen, insbesondere Betreuer, Bewohner besser verstehen können. [canon_support_understanding_residents]

- Die Lösung soll Wissen aus Akten sowie allgemeines Wissen und Erfahrungen zur Art und Weise, wie eine beeinträchtigte Person kommuniziert, in einer App festhalten, sodass Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]

- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll außerdem in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind, sowie neue Erfahrungen unmittelbar teilbar machen. [canon_new_staff_need_support]

- Ein System, das als Übersetzungs-Schnittstelle zwischen Bewohnern und Betreuern hin und her übersetzt, darf nicht Teil des Lösungsumfangs sein. [canon_exclude_translation_system]

- Pro Person soll eine About-Me-Seite bereitgestellt werden, die für den ersten Eindruck der Person sorgt und persönliche Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]

- Inhalte in Profil- und Kommunikationsbereichen sollen dynamisch erweiterbar sein; neue Erfahrungen, Bilder, Videos, Beschreibungen, Kommunikationsinformationen und No-Gos sollen hinzugefügt werden können. [canon_dynamic_media_growth, ADJ-GAP-AU-0053, ADJ-GAP-AU-0055]

- Auf der About-Me-Seite soll das Hinzufügen neuer Inhalte über einen Plus-Button möglich sein; neu hinzugefügte Bilder sollen oben angezeigt werden. [canon_about_me_page, ADJ-GAP-AU-0125]

- Für Videos soll ein Bereich vorhanden sein, in dem neue Videos mit Beschreibungen über einen Plus-Button hinzugefügt werden können; das neueste Video soll oben angezeigt werden. [canon_video_support_in_communication, ADJ-GAP-AU-0053]

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]

- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die Kommunikationsseiten integriert werden statt als eigener separater Screen zu bestehen. [canon_video_support_in_communication]

- Pro Bewohner soll eine No-Go-Seite vorhanden sein, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht. [canon_no_go_page]

- Die No-Go-Seite soll über einen Plus-Button dynamisch um weitere No-Gos erweiterbar sein. [ADJ-GAP-AU-0055]

- Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen. [canon_profile_overview_after_login]

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, statt lange scrollen zu müssen. [canon_search_profiles]

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; sie soll ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist. [canon_consistent_appbar_after_login]

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. Es soll nur eine Login-Möglichkeit ohne Selbstregistrierung geben. Nur ein Account mit Admin-Rechten darf weitere Accounts anlegen und Rechte verwalten. User dürfen Inhalte hinzufügen, aber keine Accounts erstellen und keine hinzugefügten Daten löschen. [canon_role_based_accounts_and_access_control]

- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzendes Wissen und Daten hinzufügen können. [canon_employee_and_relative_access]

- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann. Dieser Account soll auf den About-Me-Bereich zugreifen und dort eigene Bilder hinzufügen können; die Funktionen sollen eingeschränkt sein, um das Risiko von Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

- Ein Kalender mit Terminen ist vorgesehen; ob darin auch Medikamentengaben integriert werden, soll wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorgesehen werden; dafür soll geklärt werden, welches systematische Beschreibungsmuster für Kommunikationsweisen und Einträge verwendet wird, damit Suche und Filterung möglich sind. [canon_search_communication_entries_with_pattern]

- Ob die App vollständige Dokumentation übernehmen soll, muss mit dem Leiter der Einrichtung geklärt werden; der Umfang soll dabei begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation nicht verloren geht. [canon_full_documentation_scope_open]

- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]

## Gewünschte oder offene UI-/Bedienanforderungen

- Eine digitale Lösung, etwa als Computerprogramm oder Ähnliches, ist das gewünschte Zielbild zur Verbesserung und Förderung der Kommunikation zwischen Beeinträchtigten und anderen Menschen; die konkrete verbindliche Ausgestaltung bleibt offen. [canon_app_goal_digital_comm_support]

- Für den Login-Screen soll ein Logo im oberen Bereich vorgesehen werden; das Logo selbst ist noch zu erstellen und soll etwas mit Kommunikation zu tun haben. [ADJ-GAP-AU-0117]

- Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und einem Login-Button bestehen; eine Registrierung soll dort nicht vorgesehen sein. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119, canon_role_based_accounts_and_access_control]

- In der Appbar soll der Titel des aktuellen Screens mittig angezeigt werden; auf der Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste vorgesehen werden. [ADJ-GAP-AU-0121]

- Die Profile auf der Profilübersicht sollen unterhalb der Suchleiste als Kacheln oder Liste mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners dargestellt werden, um einen schnellen Überblick zu geben. [ADJ-GAP-AU-0122]

- Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol mit einem Dialogfeld für Bild, Name und Beschreibung noch geklärt werden. [ADJ-GAP-AU-0123]

- Beim Öffnen eines Profils soll eine Detailansicht mit größerem Profilbild und interaktiven Buttons zu den vier Hauptbereichen der App angezeigt werden. [ADJ-GAP-AU-0124]

- Für die About-Me-Seite ist zusätzlich eine Infobox im oberen Bereich mit Angaben wie Alter und Hobbys sowie darunter eine Foto-Timeline mit Plus-Button und Anzeige der neuesten Fotos oben gewünscht. [ADJ-GAP-AU-0125]

- Auf der Kommunikationsseite ist gewünscht, die Trennung zwischen verbaler und nonverbaler Kommunikation visuell mit Symbolen zu kennzeichnen und pro Bereich einen klaren Button zu weiteren Informationen anzubieten. [ADJ-GAP-AU-0126]

- Für nonverbale Signale ist gewünscht, über einen Plus-Button Videos und Beschreibungen hinzufügen zu können. [ADJ-GAP-AU-0127]

- Für die No-Go-Seite sind klare, kompakte Einträge und ein starkes visuelles Symbol oben auf dem Screen gewünscht, etwa ein rotes Stoppschild. [ADJ-GAP-AU-0128]

- Für den Settings-Screen soll eine differenzierte Ansicht je nach Nutzerrolle vorgesehen werden: Admins können dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen; jeder Nutzer soll sein Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]

- Eine Hilfe- bzw. Tutorial-Funktion ist gewünscht, etwa als kurze Tour beim ersten Login sowie als erneut aufrufbarer Hilfebereich; dies ist später möglich. [canon_help_tutorial_features]

- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; dies ist später möglich. [canon_about_me_notifications]

- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]

## Nicht-funktionale Anforderungen

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]

- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung von zu vielen Farben. [canon_accessible_design]

- Animationen sollen nicht aktiv eingeplant werden; falls dennoch welche vorhanden sind, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]

## Zugriffs- und Datenschutz-Constraints

- Die App ist nur für den internen Gebrauch der KYOS vorgesehen. [canon_internal_use_only]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile der Bewohner ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Kontext, Risiken und Klärungspunkte

- Für Bewohner existieren klassische Akten und weitere dokumentierte Informationen; grundsätzlich wird alles nach einem bestimmten Plan dokumentiert, und auch Erfahrungen sowie neues Wissen werden schriftlich festgehalten. Diese Informationsquellen sind fachlich relevant. [canon_resident_records_exist]

- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und neues Personal wird stattdessen mündlich eingewiesen. Dies ist ein relevantes Risiko für die Nutzbarkeit der aktuellen Wissensbasis. [canon_existing_docs_hard_to_use]

- Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

- Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen; daraus folgt jedoch nicht automatisch ein bestimmter MVP-Umfang. [canon_digitization_desired_broadly]

## Technische und prozessuale Rahmenbedingungen

- Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein größeres Gesamtbild zu gewinnen. [canon_requirements_analysis_with_users]

- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]

- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]

- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]

- Das Team präferiert, dieselbe Entwicklungsumgebung zu nutzen, und bevorzugt dafür Android Studio. [canon_android_studio_preferred]

- Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um Integrationsprobleme zu vermeiden. [canon_align_versions]

- Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden. [canon_develop_for_android11_initially]