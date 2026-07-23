# Requirements

## Produktziel und Umfang
- Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere aus Sicht von Betreuern und anderen Personen, die mit ihnen kommunizieren. [canon_support_understanding_residents]
- Die Lösung soll als App Wissen über die Art und Weise, wie eine Person kommuniziert, aus Akten bzw. allgemeinem Erfahrungswissen festhalten, sodass Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canon_app_as_knowledge_lookup]
- Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen, Einarbeitungswissen verfügbar zu machen und neue Erfahrungen unmittelbar teilbar zu machen; sie soll auch in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind. [canon_new_staff_need_support]
- Eine digitale Lösung, etwa als Computerprogramm oder Ähnliches, ist als bevorzugter Ansatz zur Verbesserung und Förderung der Kommunikation gewünscht; der genaue Verbindlichkeits- und Umsetzungsumfang bleibt offen. [canon_app_goal_digital_comm_support]
- Die App darf kein System sein, das als Schnittstelle zwischen Bewohner und z. B. Betreuer hin- und herübersetzt. [canon_exclude_translation_system]
- Ob die App vollständige Dokumentation übernehmen soll, ist mit der Leitung zu klären; der Umfang soll dabei so begrenzt bleiben, dass der Fokus auf unterstützender Kommunikation nicht verloren geht. [canon_full_documentation_scope_open]

## Nutzer, Zugriff und Berechtigungen
- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen. [canon_role_based_accounts_and_access_control]
- Es soll nur eine Login-Möglichkeit geben; eine Selbstregistrierung darf nicht vorgesehen sein. [canon_role_based_accounts_and_access_control, ADJ-GAP-AU-0119]
- Nur ein Account mit Admin-Rechten soll weitere Accounts erstellen und Rechte verwalten dürfen. [canon_role_based_accounts_and_access_control]
- User-Accounts sollen Inhalte hinzufügen dürfen, aber keine Accounts erstellen und keine hinzugefügten Daten löschen können. [canon_role_based_accounts_and_access_control]
- Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzendes Wissen bzw. Daten hinzufügen können. [canon_employee_and_relative_access]
- Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann. [canon_resident_account_with_restrictions]
- Der Bewohner-Account soll auf den About-Me-Bereich zugreifen und dort eigene Bilder hinzufügen können. [canon_resident_account_with_restrictions]
- Der Bewohner-Account soll darüber hinaus nur eingeschränkte Funktionen erhalten, um das Risiko von Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]
- Die App soll nur für den internen Gebrauch der KYOS vorgesehen sein. [canon_internal_use_only]
- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

## Login, Navigation und Grundstruktur
- Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und einem Login-Button bestehen. [ADJ-GAP-AU-0118, ADJ-GAP-AU-0119]
- Die E-Mail- und Passwort-Felder auf dem Login-Screen sollen intuitiv und benutzerfreundlich angeordnet sein; Details wie ein möglicher leichter Schattenwurf bleiben offen. [ADJ-GAP-AU-0118]
- Auf dem Login-Screen ist ein auffälliges Logo im oberen Drittel gewünscht; das Logo soll noch erstellt werden und etwas mit Kommunikation zu tun haben. [ADJ-GAP-AU-0117]
- Nach dem Login soll eine Profilübersichtsseite als Startseite angezeigt werden. [canon_profile_overview_after_login]
- Auf der Profilübersichtsseite soll eine Liste aller für den Nutzer zugänglichen Profile angezeigt werden; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen. [canon_profile_overview_after_login]
- Nach dem Login soll auf jeder Seite oben eine konsistente Appbar vorhanden sein. [canon_consistent_appbar_after_login]
- Die Appbar soll ein Einstellungssymbol enthalten, über das zur Einstellungsseite navigiert werden kann. [canon_consistent_appbar_after_login]
- Gewünscht ist, dass in der Appbar der Titel des aktuellen Screens mittig angezeigt wird; für die Profilübersicht wäre dies „Profilübersicht“. [ADJ-GAP-AU-0121]
- Für den Settings-Screen sollen rollenabhängig differenzierte Ansichten unterstützt werden: Admins sollen dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen können, während jeder Nutzer sein Profil ändern und Einstellungen wie die Sprache wählen können soll. [ADJ-GAP-AU-0130]

## Profilübersicht und Profile
- Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, um Profile schnell zu finden und langes Scrollen zu vermeiden. [canon_search_profiles]
- Gewünscht ist, dass unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste angezeigt wird. [ADJ-GAP-AU-0121]
- Gewünscht ist, dass die Profile unterhalb der Suchleiste als Kacheln oder Liste angezeigt werden und jeweils ein kleines Vorschaubild, den Namen und eine kurze Beschreibung des Bewohners enthalten. [ADJ-GAP-AU-0122]
- Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol möglich sein; beim Anklicken soll ein Dialogfeld zum Anlegen eines neuen Profils mit Bild, Name und Beschreibung erscheinen. [ADJ-GAP-AU-0123]
- Beim Öffnen eines Profils soll gewünscht eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt. [ADJ-GAP-AU-0124]

## About Me
- Die App soll pro Person eine About-Me-Seite bereitstellen, die einen persönlichen Ersteindruck vermittelt. [canon_about_me_page]
- Die About-Me-Seite soll Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthalten. [canon_about_me_page]
- About Me soll informativ und persönlich gestaltet sein. [canon_about_me_page]
- Profilinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Informationen hinzugefügt werden können. [canon_dynamic_media_growth]
- Für About Me soll ein Plus-Button das Hinzufügen weiterer Inhalte wie z. B. Bilder und Beschreibungen ermöglichen. [canon_about_me_page, canon_dynamic_media_growth]
- Neu hinzugefügte Bilder auf der About-Me-Seite sollen automatisch ganz oben als neuestes Bild angezeigt werden. [canon_about_me_page]
- Gewünscht ist, dass die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthält und darunter eine Foto-Timeline zeigt. [ADJ-GAP-AU-0125]
- Gewünscht ist, dass neue Foto-Einträge in der About-Me-Foto-Timeline über einen Plus-Button am unteren Bildschirmrand hinzugefügt werden können und die neuesten Fotos oben angezeigt werden. [ADJ-GAP-AU-0125]
- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden; diese Funktion ist nur später möglich. [canon_about_me_notifications]

## Kommunikation
- Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden. [canon_communication_split_verbal_nonverbal]
- Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen. [canon_non_textual_representation]
- Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen und Kommunikationsinformationen hinzugefügt werden können. [canon_dynamic_media_growth]
- Gewünscht ist, dass die visuelle Trennung zwischen verbaler und nonverbaler Kommunikation mit Symbolen gekennzeichnet wird und jeder Bereich einen klaren Button zu weiteren Informationen erhält. [ADJ-GAP-AU-0126]
- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; hierfür muss ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich werden. [canon_search_communication_entries_with_pattern]
- Die Suchfunktion auf den Kommunikationsseiten soll über eine Suchleiste oben im Bildschirm ermöglicht werden; die genaue standardisierte Formulierung der Einträge ist noch zu klären. [canon_search_communication_entries_with_pattern]

## Videos im Kommunikationsbereich
- Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen. [canon_video_support_in_communication]
- Die Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen. [canon_video_support_in_communication]
- Die Videofunktion soll eine Suchfunktion berücksichtigen; der genaue Suchstandard hängt von der noch zu klärenden Beschreibungssystematik ab. [canon_video_support_in_communication, canon_search_communication_entries_with_pattern]
- Auf der Videofunktion sollen neue Videos mit Beschreibungen über einen Plus-Button hinzugefügt werden können. [ADJ-GAP-AU-0053]
- Auf der Videofunktion soll das neueste Video oben angezeigt werden. [ADJ-GAP-AU-0053]
- Gewünscht ist, dass insbesondere für nonverbale Signale ein Plus-Button zum Hinzufügen von Videos und Beschreibungen bereitsteht. [ADJ-GAP-AU-0127]

## No-Go-Bereich
- Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind. [canon_no_go_page]
- Die No-Go-Seite soll dynamisch erweiterbar sein, sodass über einen Plus-Button weitere No-Gos hinzugefügt werden können und eine wachsende Liste entsteht. [ADJ-GAP-AU-0055]
- Gewünscht ist, dass die No-Go-Seite oben ein starkes visuelles Symbol, beispielsweise ein rotes Stoppschild, zeigt. [ADJ-GAP-AU-0128]
- Gewünscht ist, dass die Einträge auf der No-Go-Seite einfach zu durchforsten sind und nur die wichtigsten Informationen enthalten. [ADJ-GAP-AU-0128]

## Kalender
- Ein Kalender mit Terminen soll berücksichtigt werden. [canon_calendar_and_medication_open]
- Der Kalender sollte einfach und übersichtlich sein; eine klare monatliche Ansicht ist vorgesehen. [canon_calendar_and_medication_open]
- Ein Touch auf ein Datum soll Details zum Termin öffnen. [canon_calendar_and_medication_open]
- Ob Medikamentengaben im Kalender integriert werden, ist wegen Vertraulichkeit und Umfang offen und muss geklärt werden. [canon_calendar_and_medication_open]
- Falls Medikamentengaben im Kalender berücksichtigt werden, könnten sie an den entsprechenden Tagen mit kleinen Icons oder Tags dargestellt werden; dies ist noch nicht entschieden. [canon_calendar_and_medication_open]

## Plattform und Qualitätsanforderungen
- Die App soll plattformübergreifend auf iPhone/iOS und Android laufen. [canon_cross_platform_ios_android]
- Ob die App zusätzlich auf Tablets laufen soll und ob dies umsetzbar ist, muss noch evaluiert werden. [canon_tablet_support_open]
- Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz. [canon_accessible_design]
- Gewünscht ist eine Hilfe- bzw. Tutorial-Funktion, etwa als kurze Tour beim ersten Login sowie als erneut aufrufbarer Hilfebereich; diese Funktion ist nur später möglich. [canon_help_tutorial_features]
- Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden. [canon_alternative_input_consideration]
- Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein. [canon_animations_not_priority]

## Offene Klärungen und Kontext
- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App müssen vorab geklärt werden. [canon_privacy_clearance_for_images]
- Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]
- Für Bewohner existieren klassische Akten und fortlaufend dokumentierte Informationen; diese stellen eine relevante Wissensquelle für die Lösung dar. [canon_resident_records_exist]
- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden und Wissen wird daher neuen Mitarbeitern häufig mündlich erklärt. [canon_existing_docs_hard_to_use]
- Firebase Firestore ist als Datenbankansatz vorgesehen; wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, muss noch konkretisiert werden. [canon_firebase_with_local_cache_open]
- Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden. [canon_getx_consideration]
- Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Gesamtbild zu gewinnen. [canon_requirements_analysis_with_users]
- Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt. [canon_digitization_desired_broadly]