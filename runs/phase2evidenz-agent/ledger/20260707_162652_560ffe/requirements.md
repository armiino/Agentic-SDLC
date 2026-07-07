# Requirements

## Functional Requirements

- Die Lösung soll bevorzugt als digitale Anwendung (Computer‑Programm o. Ä.) die Kommunikation zwischen Menschen mit Beeinträchtigungen und anderen Personen unterstützen. *(modality = desired)* [canon_app_goal_digital_comm_support]  
- Die App muss neuen Mitarbeitenden helfen, Bewohner schneller zu verstehen und On‑boarding‑Wissen verfügbar zu machen. *(modality = must)* [canon_new_staff_need_support]  
- Jede Person erhält eine **About‑Me‑Seite** mit persönlichem Ersteindruck: Bild, Beschreibung, Hobbys, Name und Alter. *(modality = must)* [canon_about_me_page]  
- Profil‑ und Kommunikationsinhalte müssen **dynamisch erweiterbar** sein (z. B. neue Erfahrungen, Bilder, Kommunikationsarten). *(modality = must)* [canon_dynamic_media_growth]  
- Kommunikationsinhalte dürfen nicht nur textbasiert, sondern auch **visuell** (Bilder) dargestellt werden. *(modality = must)* [canon_non_textual_representation]  
- Die App muss **Videos mit Beschreibungen** für Kommunikationssituationen unterstützen; die Videofunktion wird in die bestehenden Kommunikationsseiten integriert, nicht als separater Screen. *(modality = must)* [canon_video_support_in_communication]  
- Die Anwendung muss **Rollen‑basierte Accounts** bieten:  
  - Admin‑Accounts können weitere Accounts anlegen und Rechte verwalten.  
  - User‑Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen. *(modality = must)* [canon_role_based_accounts_and_access_control]  
- Die App ist **nur für internen Gebrauch** der Einrichtung vorgesehen (keine öffentliche Registrierung). *(modality = must)* [canon_internal_use_only]  
- Nach dem Login muss eine **Profil‑Übersichtsseite** angezeigt werden, die eine anklickbare Liste aller für den eingeloggten Nutzer zugänglichen Profile enthält. *(modality = must)* [canon_profile_overview_after_login]  
- Kommunikationsinformationen sind **in verbale und nonverbale Bereiche** zu strukturieren. *(modality = must)* [canon_communication_split_verbal_nonverbal]  
- Auf der Profil‑Übersichtsseite ist eine **Suchfunktion** zu integrieren, die das schnelle Auffinden von Profilen ermöglicht (z. B. nach Name). *(modality = must)* [canon_search_profiles]  
- Die Lösung muss primär ermöglichen, dass Betreuer / Mitarbeitende Bewohner besser verstehen können. *(modality = must)* [canon_support_understanding_residents]  
- Die App soll als **Wissens‑Lookup** dienen und Kommunikationsweisen einer Person aus Akten und Erfahrungswissen festhalten, sodass Nutzer bei Verständnisproblemen nachsehen können. *(modality = must)* [canon_app_as_knowledge_lookup]  
- **Mitarbeitende und Angehörige** erhalten Zugriff auf die App, um ergänzendes Wissen beizutragen. *(modality = must)* [canon_employee_and_relative_access]  
- Für jede Person ist eine **No‑Go‑Seite** bereitzustellen, die kritische Verhaltensweisen/Handlungen auflistet, die in Gegenwart des Bewohners zu vermeiden sind. *(modality = must)* [canon_no_go_page]  
- Die App muss **plattformübergreifend** auf iOS‑ und Android‑Geräten laufen. *(modality = must)* [canon_cross_platform_ios_android]  
- Die Anwendung muss **barrierefrei** gestaltet sein (große Schrift, ausreichender Kontrast, zurückhaltender Farbeinsatz). *(modality = must)* [canon_accessible_design]  
- Nach dem Login ist auf **jeder Seite** eine konsistente **App‑Bar** mit einem Einstellungssymbol zur Navigation zu den Einstellungen anzuzeigen. *(modality = must)* [canon_consistent_appbar_after_login]  
- **Resident‑Accounts** dürfen nur das eigene Profil sehen und lediglich About‑Me‑Funktionen (z. B. eigene Bilder hinzufügen) nutzen, um Fehlbedienungen zu minimieren. *(modality = desired)* [canon_resident_account_with_restrictions]  
- Auf den Kommunikationsseiten ist eine **Suchfunktion** vorgesehen; dafür muss ein standardisiertes **Beschreibungsmuster** für Einträge definiert werden. *(status = open, modality = must_clarify)* [canon_search_communication_entries_with_pattern]  
- Für die **Video‑Seite** ist dieselbe dynamische Hinzufügungs‑ und Sortierungslogik wie bei anderen Medien zu implementieren (neueste Videos oben). *(modality = must)* [ADJ-GAP-AU-0053]  
- Die **No‑Go‑Seite** soll durch einen Plus‑Button dynamisch erweiterbar sein; neue No‑Go‑Punkte können jederzeit hinzugefügt werden. *(modality = must)* [ADJ-GAP-AU-0055]  

## Non‑Functional Requirements

- Die App soll **visuell** zwischen verbaler und nonverbaler Kommunikation trennen (z. B. mit Symbolen und klaren Buttons). *(modality = must)* [ADJ-GAP-AU-0126]  
- **Animationen** werden nicht aktiv eingeplant; falls verwendet, dürfen sie nicht ablenkend sein. *(modality = must_consider, optional)* [canon_animations_not_priority]  

## Constraints

- Ein **Übersetzungssystem** (Schnittstelle zwischen Bewohnern und Betreuern) darf **nicht** Teil des Lösungsumfangs sein. *(modality = must_not)* [canon_exclude_translation_system]  
- Der Zugriff muss **einrichtungsbezogen beschränkt** sein: Mitarbeitende sehen nur Profile des eigenen Hauses. Die konkrete technische Umsetzung ist noch zu klären. *(status = open, modality = must_clarify)* [canon_cross_facility_access_restricted]  
- Die Möglichkeit, die App auf **Tablets** zu betreiben, muss noch evaluiert werden. *(status = open, modality = must_clarify)* [canon_tablet_support_open]  
- Ob die App **vollständige Dokumentation** übernehmen soll, ist offen und muss mit der Leitung geklärt werden; der Umfang soll begrenzt bleiben, um den Fokus auf unterstützende Kommunikation zu wahren. *(status = open, modality = must_clarify)* [canon_full_documentation_scope_open]  
- Der **Kalender** soll einfach und übersichtlich sein; ob er **Medikamentengaben** integriert, ist wegen Vertraulichkeit unklar und muss geklärt werden. *(modality = must_consider, required)* [canon_calendar_and_medication_open]  
- Für die **Datenschutz‑Einwilligung** von Bildern (z. B. bei Test‑ oder Produktivnutzung) muss vorab mit Angehörigen geklärt werden. *(status = open, modality = must_clarify, context)* [canon_privacy_clearance_for_images]  
- Der Zugriff auf **Bewohner‑Akten** zu Analyse‑ oder Testzwecken ist wegen Datenschutzes unklar und muss geklärt werden. *(status = open, modality = must_clarify, context)* [canon_records_access_privacy_open]  

## Open / Unclear Issues (to be clarified)

- **Umfang der Dokumentation**: Klärung, ob die App die gesamte Dokumentation übernehmen soll. *(must_clarify)* [canon_full_documentation_scope_open]  
- **Einrichtungsbezogene Zugriffsbeschränkung**: Technische Ausgestaltung noch offen. *(must_clarify)* [canon_cross_facility_access_restricted]  
- **Tablet‑Support**: Machbarkeit und Umfang noch zu prüfen. *(must_clarify)* [canon_tablet_support_open]  
- **Beschreibungsmuster für Kommunikations‑Einträge**: Definition erforderlich, um Suche/Filter zu ermöglichen. *(must_clarify)* [canon_search_communication_entries_with_pattern]  
- **Konsistente App‑Bar und Suchleiste** auf der Profil‑Übersichtsseite sowie deren genaue Gestaltung sind noch zu spezifizieren. *(desired, open)* [ADJ-GAP-AU-0121]  
- **Darstellung der Profil‑Übersichts‑Liste** (Kacheln/Liste, Vorschaubild, Name, Kurzbeschreibung) muss konkretisiert werden. *(desired, open)* [ADJ-GAP-AU-0122]  
- **Anlegen neuer Profile** über ein Plus‑Symbol mit Dialog für Bild, Name und Beschreibung soll ermöglicht werden; Details zur UI sind noch zu klären. *(must_clarify)* [ADJ-GAP-AU-0123]  
- **Detailansicht eines Profils**: Darstellung vergrößertes Bild + Navigation zu vier Hauptbereichen muss spezifiziert werden. *(desired, open)* [ADJ-GAP-AU-0124]  
- **About‑Me‑Seite**: Infobox, Foto‑Timeline, Plus‑Button und Sortierung neuer Fotos (neueste oben) sind vorgesehen; UI‑Details sind noch offen. *(desired, open)* [ADJ-GAP-AU-0125]  
- **Visuelle Trennung** zwischen verbaler und nonverbaler Kommunikation (Symbole, Buttons) ist zu definieren. *(desired, open)* [ADJ-GAP-AU-0126]  
- **Videos hinzufügen**: Plus‑Button für Video‑ und Beschreibungs‑Einträge auf Kommunikationsseiten muss spezifiziert werden. *(desired, open)* [ADJ-GAP-AU-0127]  
- **No‑Go‑Seite**: Visuelle Gestaltung (z. B. rotes Stoppsymbol) und kompakte Darstellung der Einträge sind noch zu klären. *(desired, open)* [ADJ-GAP-AU-0128]  
- **Rollen‑abhängige Settings‑Screen‑Beispiele** (z. B. Sprache, Nutzeraccounts, Rechteanpassung) sollen konkretisiert werden. *(must)* [ADJ-GAP-AU-0130]  
- **Login‑Screen**: Gestaltung mit Logo, E‑Mail‑ und Passwort‑Feld sowie Login‑Button ist festgelegt; keine Registrierung soll angeboten werden. *(must)* [ADJ-GAP-AU-0118] [ADJ-GAP-AU-0119]  
- **Login‑Screen Branding**: Logo‑Platzierung und Bildstil sind angedacht, aber nicht final definiert. *(desired, open)* [ADJ-GAP-AU-0117]  