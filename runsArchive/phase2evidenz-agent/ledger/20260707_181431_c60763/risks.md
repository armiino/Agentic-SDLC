# Risks

## Datenschutz und Zugriff

- Datenschutz und Zustimmung von Angehörigen für Bilder zur testweisen oder produktiven Nutzung in der App sind vorab noch zu klären; damit ist die Bildnutzung derzeit offen. [canon_privacy_clearance_for_images]

- Ob für Analyse, Tests oder die inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss noch geklärt werden. [canon_records_access_privacy_open]

- Zugriffe müssen auf die jeweils eigene Einrichtung beschränkt werden, damit Mitarbeitende nur die Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten. [canon_cross_facility_access_restricted]

- Die App ist nur für den internen Gebrauch vorgesehen, während zugleich auch Angehörige Zugriff erhalten können; damit besteht ein abzustimmender Grenzfall bei der vorgesehenen Nutzergruppe und dem internen Nutzungsrahmen. [canon_internal_use_only, canon_employee_and_relative_access]

- Für den vorgesehenen Bewohner-Account sollen stark eingeschränkte Rechte gelten, insbesondere nur Zugriff auf das eigene Profil sowie auf About Me mit eigener Bildhinzufügung; die Einschränkung dient ausdrücklich dazu, das Risiko von Fehlbedienungen zu minimieren. [canon_resident_account_with_restrictions]

## Inhalte und Fachlichkeit

- Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden und Wissen wird stattdessen mündlich an neue Mitarbeitende weitergegeben. [canon_existing_docs_hard_to_use]

- Pro Bewohner ist eine No-Go-Seite mit kritischen Dingen vorgesehen, die in Gegenwart des Bewohners zu vermeiden sind; ohne diese festgehaltenen Hinweise bleibt ein als sehr wichtig beschriebenes Wissensfeld leicht unterschätzt. [canon_no_go_page]

- Für die Suchfunktion auf den Kommunikationsseiten muss erst ein systematisches Beschreibungsmuster für Kommunikations-Einträge entwickelt werden; ohne diese Klärung bleibt die Suche in diesem Bereich fachlich offen. [canon_search_communication_entries_with_pattern]

- Für den Kalender ist offen, ob auch Medikamentengaben integriert werden sollen; dies muss wegen Vertraulichkeit und Umfang noch geklärt werden. [canon_calendar_and_medication_open]

## Technik und Architektur

- Für den vorgesehenen Firebase-Firestore-Ansatz ist noch offen, wie Cloud-Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden; dieser Cache-/Lokalspeicher-Aspekt ist noch zu konkretisieren. [canon_firebase_with_local_cache_open]

## Team und Entwicklungsprozess

- Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um spätere Probleme beim Entwicklungsstart zu vermeiden; abweichende Versionen sind damit ein konkreter Integrationsrisikopunkt. [canon_align_versions]