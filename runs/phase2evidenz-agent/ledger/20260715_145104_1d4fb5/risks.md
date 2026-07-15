# Risks

## Datenschutz und Zugriff

- Datenschutz und die Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App sind vorab noch zu klären. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden. [canon_records_access_privacy_open]

- Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

- Die App ist nur für den internen Gebrauch vorgesehen, zugleich sollen neben Mitarbeitern auch Angehörige Zugriff erhalten können; diese Nutzungsgrenze und der vorgesehene Zugriff durch zusätzliche Nutzergruppen erzeugen einen abzustimmenden Zugriffskontext. [canon_internal_use_only, canon_employee_and_relative_access]

- Ein Bewohner-Account mit Zugriff nur auf das eigene Profil und stark eingeschränkten Funktionen ist nur als gewünschte Ausprägung benannt; damit bleibt offen, ob und wie diese Beschränkung umgesetzt wird, obwohl sie zur Minimierung von Fehlbedienungen vorgesehen ist. [canon_resident_account_with_restrictions]

## Inhalte und fachlicher Umfang

- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar, sodass Informationen oft nicht schnell gefunden werden und Wissen stattdessen mündlich an neue Mitarbeiter weitergegeben wird. [canon_existing_docs_hard_to_use]

- Ob die App vollständige Dokumentation übernehmen soll, ist offen und mit der Leitung zu klären; zugleich soll der Umfang begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canon_full_documentation_scope_open]

- Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind; ohne diese Informationen fehlt ein ausdrücklich als sehr wichtig bezeichneter Funktionsbereich. [canon_no_go_page]

- Für den vorgesehenen Kalender ist offen, ob auch Medikamentengaben integriert werden, da dies wegen Vertraulichkeit und Umfang geklärt werden muss. [canon_calendar_and_medication_open]

## Technik und Umsetzung

- Für den vorgesehenen Firebase-Firestore-Ansatz ist noch offen, wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden. [canon_firebase_with_local_cache_open]

- Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um spätere Integrationsprobleme zu vermeiden; abweichende Versionen sind damit ein benannter Risikopunkt für den Entwicklungsstart. [canon_align_versions]

- Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten, Login ohne Selbstregistrierung sowie Admin-gesteuerte Accountanlage und Rechteverwaltung unterstützen; dieser Zugriffskontrollrahmen ist für die Lösung zentral und muss entsprechend umgesetzt werden. [canon_role_based_accounts_and_access_control]