# Risks

## Datenschutz / Zugriffsrechte

- Es ist ungeklärt, ob und wie für die Nutzung von Bildern in der App die datenschutzrechtliche Zustimmung der Angehörigen vorliegt und eingeholt werden kann. Dies blockiert die Verwendung von Mediendaten in der App bis zur Klärung. [canon_privacy_clearance_for_images]

- Es ist offen, ob und in welchem Umfang Einsicht in die Bewohnerakten für Analyse, Tests oder inhaltliche Befüllung der App möglich ist, da dies aufgrund von Datenschutz fraglich sein kann. Dies stellt einen wesentlichen Unsicherheitsfaktor dar. [canon_records_access_privacy_open]

- Die Umsetzung eines Berechtigungsmodells, das den Zugriff von Mitarbeitern strikt auf die Profile ihrer eigenen Einrichtung beschränkt, ist noch nicht final technisch geklärt. Das birgt das Risiko von unklaren oder unausgereiften Zugriffsbeschränkungen. [canon_cross_facility_access_restricted]

- Die App soll nur intern genutzt werden, doch die genauen Mechanismen zur Sicherstellung und Durchsetzung dieser Einschränkung sowie die Handhabung von Accounts sind Detailfragen, deren Umsetzung Risiken bergen kann. [canon_internal_use_only], [canon_role_based_accounts_and_access_control]

- Es ist offen, ob und wie ein Admin-Screen für Account- und Rechteverwaltung gestaltet wird, was Risiko für unklare oder unzureichende Administration der Zugriffsrechte darstellt. [canon_admin_screen_open]

## Technische Umsetzung / Architektur

- Die geplante Datenbank-Lösung mit Firebase Firestore ist prinzipiell beschlossen, jedoch ist ungeklärt, wie Cloud-Daten auf dem Gerät lokal gespeichert und zwischengespeichert werden, was Risiken für Performance, Offline-Nutzung oder Datenkonsistenz darstellt. [canon_firebase_with_local_cache_open], [ADJ-GAP-AU-0106]

- Es ist noch nicht entschieden, ob die App zusätzlich auf Tablets lauffähig sein wird, was Unsicherheit hinsichtlich der Zielplattformen und möglicher Entwicklungsaufwände bedeutet. [canon_tablet_support_open]

- Die Nutzung des State-Management-Frameworks GetX ist noch offen und offen für Prüfungen, wodurch sich weitere technische Abhängigkeiten oder Komplexitätsrisiken ergeben können. [canon_getx_consideration]

- Die Wahl der Entwicklungsumgebung ist noch nicht abschließend vereinheitlicht; ein Teammitglied favorisiert Visual Studio Code, andere Android Studio, was potenziell Integrations- oder Koordinationsrisiken birgt. [canon_android_studio_preferred], [ADJ-GAP-AU-0096]

## Funktionale Ausgestaltung / Produktumfang

- Ob die App eine vollständige Übernahme der Dokumentation leisten soll oder der Fokus auf unterstützender Kommunikation liegt, ist offen und muss mit der Leitung geklärt werden. Diese Unklarheit kann zu Zielkonflikten oder Umfangsausweitungen führen. [canon_full_documentation_scope_open]

- Ob und in welchem Umfang ein Kalender auch Medikamentengaben enthalten soll, ist aufgrund der Vertraulichkeit und des Umfangs noch offen und muss geklärt werden, was Risiko für Umsetzungsumfang und Datenschutz bedeutet. [canon_calendar_and_medication_open]

- Eine systematische und standardisierte Struktur für die Beschreibung von Kommunikations-Einträgen (z.B. zur Suche und Filterung) ist noch nicht definiert, hier besteht Unklarheit, die die Suchfunktionalität der Kommunikationsseiten beeinträchtigen kann. [canon_search_communication_entries_with_pattern]

- Eine alternative Eingabemethode wie Sprachbefehle für beeinträchtigte Nutzer ist noch nicht konkretisiert und bleibt eine noch zu bewertende Option, wodurch mögliche Barrieren für Nutzer bestehen bleiben könnten. [canon_alternative_input_consideration]

- Die Ausgestaltung eines Bewohner-Accounts mit eingeschränkten Rechten ist gewünscht, jedoch besteht Unsicherheit hinsichtlich der genauen Funktionalitäten und Sicherheit gegen Fehlbedienung. [canon_resident_account_with_restrictions]

- Die Gestaltung der Such- und Navigationsfunktionalitäten, insbesondere auf Profilübersicht und Kommunikationsseiten, ist noch offen hinsichtlich Detail-UI und standardisierter Datenhaltung. [ADJ-GAP-AU-0121], [ADJ-GAP-AU-0123], [canon_search_communication_entries_with_pattern]

## Prozess / Organisation

- Eine verbindliche Anforderungsanalyse mit Beteiligten und späteren Nutzern ist zwar erwünscht, aber der genaue Umfang, beteiligte Einrichtungen und der Terminplan sind noch offen, was Risiken für die Vollständigkeit und Repräsentativität der Anforderungen birgt. [canon_requirements_analysis_with_users]

- Die Teams müssen noch einheitlich Entwicklungswerkzeuge und Programmierregeln (einschließlich Versionen) abstimmen, um Integrationsprobleme zu vermeiden. [canon_align_versions], [ADJ-GAP-AU-0113]