# Requirements

## Funktional

- Die Lösung soll primär dazu dienen, dass andere Personen, insbesondere Betreuer, Bewohner besser verstehen können. [canon_support_understanding_residents]
- Die Lösung soll als App Wissen aus Akten und aus Erfahrungen über die Art und Weise festhalten, wie eine Person kommuniziert, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]
- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll auch in Situationen unterstützen, in denen beeinträchtigte Personen das erste Mal allein mit jemandem sind und sich verständigen müssen. [canon_new_staff_need_support]
- Es wird eine digitale Lösung, etwa ein Computerprogramm oder Ähnliches, als bevorzugter Ansatz zur Verbesserung und Förderung der Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen angestrebt; der konkrete Zuschnitt dieser Vision bleibt offen. [canon_app_goal_digital_comm_support]

## Login, Konten und Zugriff

- Die App soll nur eine Login-Möglichkeit bereitstellen; eine Selbstregistrierung soll nicht möglich sein. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]
- Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen; ein Registrierungsbereich soll dort nicht vorhanden sein. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]
- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. [canon_role_based_accounts_and_access_control]
- Nur ein Account mit Admin-Rechten soll weitere Accounts anlegen und Rechte verwalten können. [canon_role_based_accounts_and_access_control]
- User-Accounts sollen Inhalte hinzufügen dürfen, aber keine Accounts erstellen und keine bereits hinzugefügten Daten löschen können. [canon_role_based_accounts_and_access_control]
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzendes Wissen und Daten hinzufügen können. [canon_employee_and_relative_access]
- Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann. [canon_resident_account_with_restrictions]
- Der Bewohner-Account soll nur eingeschränkte Funktionen erhalten, insbesondere Zugriff auf den About-Me-Bereich und das Hinzufügen eigener Bilder, um das Risiko von Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]
- Die App soll nur für den internen Gebrauch der KYOS vorgesehen sein. [canon_internal_use_only]
- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells soll noch ausgearbeitet werden. [canon_cross_facility_access_restricted]

## Navigation und Grundstruktur

- Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält. [canon_profile_overview_after_login]
- Jedes Profil in der Profilübersicht soll anklickbar sein und zu einer weiteren Seite führen. [canon_profile_overview_after_login]
- Nach dem Login soll auf jeder Seite oben eine konsistente Appbar vorhanden sein. [canon_consistent_appbar_after_login]
- Die Appbar soll ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist. [canon_consistent_appbar_after_login]
- Gewünscht ist, dass in der Appbar mittig der Titel des aktuellen Screens angezeigt wird, etwa „Profilübersicht“. [ADJ-GAP-AU-0121]
- Beim Öffnen eines Profils soll gewünscht eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt. [ADJ-GAP-AU-0124]

## Profilübersicht und Profile

- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, ohne lange scrollen zu müssen. [canon_search_profiles]
- Gewünscht ist, dass unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste angezeigt wird. [ADJ-GAP-AU-0121]
- Gewünscht ist, dass Profile unterhalb der Suchleiste als Kacheln oder Liste dargestellt werden und jeweils ein kleines Vorschaubild, den Namen und eine kurze Beschreibung des Bewohners zeigen. [ADJ-GAP-AU-0122]
- Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol möglich sein; beim Anklicken soll sich ein Dialogfeld zum Anlegen eines neuen Profils mit Bild, Name und Beschreibung öffnen. [ADJ-GAP-AU-0123]

## Bewohnerprofil: About Me

- Die App soll pro Person eine About-Me-Seite bereitstellen, die für einen persönlichen Ersteindruck sorgt und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält. [canon_about_me_page]
- Profilinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und weitere Inhalte hinzugefügt werden können. [canon_dynamic_media_growth]
- Für die Erweiterung von About-Me-Inhalten soll ein Plus-Button vorgesehen sein. [canon_dynamic_media_growth, canon_about_me_page]
- Neu hinzugefügte Bilder im About-Me-Bereich sollen oben als neuestes Bild angezeigt werden. [canon_about_me_page]
- Gewünscht ist, dass die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthält und darunter eine Foto-Timeline, die über einen Plus-Button am unteren Bildschirmrand neue Einträge ermöglicht; die neuesten Fotos sollen oben angezeigt werden. [ADJ-GAP-AU-0125]
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; diese Funktion ist für später möglich. [canon_about_me_notifications]

## Bewohnerprofil: Kommunikation

- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]
- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]
- Kommunikationsinhalte sollen dynamisch erweiterbar sein. [canon_dynamic_media_growth]
- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür soll noch ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich werden. [canon_search_communication_entries_with_pattern]
- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen. [canon_video_support_in_communication]
- Die Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen. [canon_video_support_in_communication]
- Auf der Videofunktion soll das Hinzufügen neuer Videos mit Beschreibungen über einen Plus-Button möglich sein. [ADJ-GAP-AU-0053]
- Neu hinzugefügte Videos sollen oben angezeigt werden. [ADJ-GAP-AU-0053]
- Gewünscht ist, die visuelle Trennung zwischen verbaler und nonverbaler Kommunikation mit Symbolen zu kennzeichnen; jeder Bereich soll einen klaren Button zu weiteren Informationen haben. [ADJ-GAP-AU-0126]
- Gewünscht ist, dass insbesondere für nonverbale Signale ein Plus-Button zum Hinzufügen von Videos und Beschreibungen vorgesehen wird. [ADJ-GAP-AU-0127]

## Bewohnerprofil: No-Go

- Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der Dinge festgehalten werden, die in Gegenwart des Bewohners absolut zu vermeiden sind. [canon_no_go_page]
- Die No-Go-Seite soll dynamisch erweiterbar sein; neue No-Gos sollen über einen Plus-Button hinzugefügt werden können. [ADJ-GAP-AU-0055]
- Die No-Go-Seite soll eine Liste von No-Go-Punkten bereitstellen, damit sich Nutzer im Vorhinein informieren können. [ADJ-GAP-AU-0055]
- Gewünscht ist, die No-Go-Seite kompakt zu gestalten, mit einfach zu durchforstenden Einträgen, die nur die wichtigsten Informationen enthalten. [ADJ-GAP-AU-0128]
- Gewünscht ist auf der No-Go-Seite ein starkes visuelles Symbol oben auf dem Screen, etwa ein rotes Stoppschild. [ADJ-GAP-AU-0128]

## Kalender und Einstellungen

- Ein Kalender mit Terminen soll berücksichtigt werden; ob auch Medikamentengaben integriert werden, muss wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]
- Der Kalender soll einfach und übersichtlich sein; eine klare monatliche Ansicht ist dafür vorgesehen. [canon_calendar_and_medication_open]
- Ein Touch auf ein Datum soll Details zum Termin oder zur Medikamentengabe öffnen; die Einbindung von Medikamentengaben bleibt jedoch offen. [canon_calendar_and_medication_open]
- Die Einstellungsseite soll rollenabhängig differenziert sein: Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen, während jeder Nutzer sein Profil ändern und Einstellungen wie die Sprache wählen können soll. [ADJ-GAP-AU-0130]

## Nicht-funktionale Anforderungen

- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]
- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]

## Offene Anforderungen und Klärungen

- Es muss noch geklärt werden, ob die App vollständige Dokumentation übernehmen soll; der Umfang soll dabei nicht so groß werden, dass der Fokus auf unterstützender Kommunikation verloren geht. [canon_full_documentation_scope_open]
- Es muss evaluiert werden, ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist. [canon_tablet_support_open]
- Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]
- Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, muss wegen Datenschutz geklärt werden. [canon_records_access_privacy_open]

## Scope-Abgrenzung

- Ein System, das zwischen Bewohnern und Betreuern als Übersetzungs-Schnittstelle fungiert und zwischen ihnen hin- und herübersetzt, darf nicht Teil des Lösungsumfangs sein. [canon_exclude_translation_system]

## Optionale spätere Funktionen

- Die App soll später möglichst eine Hilfe- bzw. Tutorial-Funktion vorsehen, etwa als kurze Tour beim ersten Login und als erneut aufrufbaren Hilfebereich. [canon_help_tutorial_features]
- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]
- Animationen sollen nicht aktiv eingeplant werden; falls dennoch welche vorhanden sind, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]

## Kontext und technische Rahmenbedingungen

- Für Bewohner existieren klassische Akten und weitere schriftlich festgehaltene Dokumentation, die als relevante Wissensquelle für die Lösung dienen können. [canon_resident_records_exist]
- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und Wissen wird neuen Mitarbeitern deshalb häufig mündlich erklärt. [canon_existing_docs_hard_to_use]
- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]
- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]
- Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen. [canon_flutter_dart_stack]
- Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein größeres Gesamtbild zu gewinnen. [canon_requirements_analysis_with_users]
- Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt. [canon_digitization_desired_broadly]