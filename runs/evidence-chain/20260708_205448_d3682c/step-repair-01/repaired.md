# Requirements

## Funktionale Anforderungen

- Die Lösung soll primär dazu dienen, dass andere Personen, insbesondere Betreuer, Bewohner besser verstehen können. [canon_support_understanding_residents]
- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll auch in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind und sich verständigen müssen. [canon_new_staff_need_support]
- Die Lösung soll als App Wissen über die Art und Weise, wie eine Person kommuniziert, aus Akten bzw. allgemeinem Erfahrungswissen festhalten, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]
- Es soll eine Login-Funktion geben; eine Registrierung durch Nutzer soll nicht vorgesehen sein. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]
- Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]
- Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen. [canon_profile_overview_after_login]
- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, ohne lange scrollen zu müssen. [canon_search_profiles]
- Auf der Profilübersichtsseite soll eine Suchleiste angezeigt werden. [ADJ-GAP-AU-0121]
- Die Profile auf der Profilübersichtsseite sollten angezeigt werden, um einen schnellen Überblick zu geben. [ADJ-GAP-AU-0122]
- Beim Öffnen eines Profils sollte eine Detailansicht das gewählte Profilbild größer anzeigen und die vier Hauptbereiche der App als interaktive Buttons darstellen. [ADJ-GAP-AU-0124]
- Die App soll pro Person eine About-Me-Seite bereitstellen, die dem ersten Eindruck dient und persönliche Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]
- Die About-Me-Seite sollte Angaben wie Alter und Hobbys sowie eine Foto-Timeline enthalten. [ADJ-GAP-AU-0125]
- Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]
- Auf der About-Me-Seite soll das Hinzufügen weiterer Inhalte über einen Plus-Button möglich sein; neu hinzugefügte Bilder sollen oben als neueste Inhalte angezeigt werden. [canon_about_me_page, ADJ-GAP-AU-0125]
- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]
- Die Trennung zwischen verbaler und nonverbaler Kommunikation sollte erfolgen. [ADJ-GAP-AU-0126]
- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen. [canon_video_support_in_communication]
- Auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich werden. [canon_search_communication_entries_with_pattern]
- Für nonverbale Signale sollte Unterstützung zum Hinzufügen von Videos und Beschreibungen vorhanden sein. [ADJ-GAP-AU-0127]
- Auf dem Videobereich sollen neue Videos mit Beschreibungen über einen Plus-Button hinzugefügt werden können; das neueste Video soll oben angezeigt werden. [ADJ-GAP-AU-0053]
- Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut vermieden werden soll. [canon_no_go_page]
- Die No-Go-Seite soll über einen Plus-Button dynamisch um weitere No-Gos erweiterbar sein. [ADJ-GAP-AU-0055]
- Die No-Go-Seite sollte einfach zu durchforsten sein und nur die wichtigsten Informationen enthalten. [ADJ-GAP-AU-0128]
- Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, muss wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzendes Wissen und Daten hinzufügen können. [canon_employee_and_relative_access]
- Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen, insbesondere Zugriff auf About Me und das Hinzufügen eigener Bilder, erhalten soll, um Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

## Rollen, Zugriff und Datenschutz-bezogene Anforderungen

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. [canon_role_based_accounts_and_access_control]
- Nur ein Account mit Admin-Rechten soll weitere Accounts erstellen und Rechte verwalten dürfen. [canon_role_based_accounts_and_access_control]
- User-Accounts sollen Inhalte hinzufügen dürfen, aber keine Accounts erstellen und keine hinzugefügten Daten löschen dürfen. [canon_role_based_accounts_and_access_control]
- Die App soll nur für den internen Gebrauch der KYOS vorgesehen sein. [canon_internal_use_only]
- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells muss noch ausgearbeitet werden. [canon_cross_facility_access_restricted]
- Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]
- Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, muss wegen Datenschutz geklärt werden. [canon_records_access_privacy_open]

## UI- und Navigationsanforderungen

- Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. [canon_consistent_appbar_after_login]
- Die Appbar soll ein Einstellungssymbol enthalten, über das zur Einstellungsseite navigiert werden kann. [canon_consistent_appbar_after_login]
- In der Appbar sollte der Titel des aktuellen Screens mittig angezeigt werden, beispielsweise „Profilübersicht“. [ADJ-GAP-AU-0121]
- Für den Settings-Screen soll es eine differenzierte Ansicht je nach Nutzerrolle geben: Admins können dort Nutzeraccounts hinzufügen und Rechte anpassen; jeder Nutzer soll das eigene Profil ändern und Einstellungen wie die Sprache wählen können. [ADJ-GAP-AU-0130]
- Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung geklärt werden. [ADJ-GAP-AU-0123]

## Nicht-funktionale Anforderungen

- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]
- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]
- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

## Offene Punkte und gewünschte Erweiterungen

- Ob die App vollständige Dokumentation übernehmen soll, ist offen und mit der Leitung zu klären; der Umfang soll dabei so begrenzt bleiben, dass der Fokus auf unterstützender Kommunikation nicht verloren geht. [canon_full_documentation_scope_open]
- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]
- Die App soll eine Hilfe- bzw. Tutorial-Funktion vorsehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbaren Hilfebereich. [canon_help_tutorial_features]
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden. [canon_about_me_notifications]
- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]
- Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]
- Eine digitale Lösung wie ein Computerprogramm oder Ähnliches wird als bevorzugter Ansatz gewünscht, um die Kommunikation zwischen beeinträchtigten Personen und anderen Menschen zu verbessern und zu fördern. [canon_app_goal_digital_comm_support]

## Constraints und Abgrenzungen

- Ein System, das zwischen Bewohnern und Betreuern als Übersetzungs-Schnittstelle fungiert, darf nicht Teil des Lösungsumfangs sein. [canon_exclude_translation_system]

## Kontext

- Für Bewohner existieren klassische Akten, in denen Informationen, Erfahrungen und neues Wissen nach einem bestimmten Plan dokumentiert werden. [canon_resident_records_exist]
- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und neues Personal wird deshalb häufig mündlich eingewiesen. [canon_existing_docs_hard_to_use]
- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]
- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden. [canon_getx_consideration]
- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]
- Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt. [canon_digitization_desired_broadly]