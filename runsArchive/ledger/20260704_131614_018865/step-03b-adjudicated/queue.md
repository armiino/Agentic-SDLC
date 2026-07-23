# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | merge_existing | mark_covered_by | reject | apply_repair | defer`
(merge_existing/mark_covered_by brauchen `referenceTarget`; apply_repair nutzt die systemSuggestion.)

## RR::canon_requirements_analysis_with_users  [review_required_claim / normal]
- proposition: Vor der konkreten Ausgestaltung der Lösung soll eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Verständnis zu gewinnen.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 1: Grundsätzlich werden wir jetzt eine Art Anforderungsanalyse mit Ihnen und vielen weiteren Personen, die mit dem System arbeiten werden, durchführen, um herauszufinden, wo genau die Bedürfnisse liegen und was genau das System eigentlich können soll. | Speaker 1: Im Rahmen der Anforderungsanalyse, um die Anforderungen an die App genau zu besprechen, ist es für uns sowieso am besten, wenn wir alle oder zumindest einige der Beteiligten und Nutzer der App kennenlernen könnten, damit auch ihre Bedürfnisse berücksichtigt werden. | Speaker 2: Ja, wir können gerne einen Termin für so einen Kennenlern-Tag ausmachen. Wir können auch gerne in mehrere Einrichtungen fahren, um ein größeres Gesamtbild der Einrichtung-NoName zu schaffen.
- reason: verdict=partial; Der Kern ist gedeckt: Anforderungsanalyse mit Nutzern und Besuche mehrerer Einrichtungen sind vorgesehen. Die Facetten sind aber etwas zu stark formuliert.
- ACTION: ____   REASON: ____

## RR::canon_support_understanding_residents  [review_required_claim / normal]
- proposition: Die Lösung soll primär dazu dienen, dass andere Personen – insbesondere Betreuer – Bewohner besser verstehen können.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Ja, wenn man sich entscheiden müsste, wäre es auf jeden Fall sinnvoller, wenn man einen Weg finden würde, die Bewohner besser zu verstehen. | Speaker 1: Also haben wir dabei einen Weg zu finden, wie der Betreuer jeden Bewohner besser verstehen kann.
- reason: verdict=partial; Die primäre Zielrichtung, Bewohner besser zu verstehen, ist klar gedeckt. Die Modalität ist jedoch etwas stärker als die Quelle.
- ACTION: ____   REASON: ____

## RR::canon_app_as_knowledge_lookup  [review_required_claim / normal]
- proposition: Die Lösung soll als App Wissen über die Kommunikationsweise einer Person aus Akten und Erfahrungen festhalten, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 1: Was halten Sie von der Idee, eine App zu entwickeln, aus der Daten, aus der Akte bzw. allgemeines Wissen der Art und Weise, wie eine beeinträchtigte Person kommuniziert festgehalten wird? | Speaker 1: Diese App wäre für jeden, der mit einer Person, die er nicht verstehen kann, eine Lösung, um nachzuschauen, was gemeint sein könnte. | Speaker 1: Eine Idee wäre es, eine App zu programmieren, die genaues über die Art und Weise, wie eine Person kommuniziert, enthält, sodass zum Beispiel jeder Betreuer die Möglichkeit hätte, wenn er mit Person X kommunizieren möchte, die Person X aber nicht verstehen kann, in der App nachschauen kann, was gemeint sein könnte, oder?
- reason: verdict=partial; Der Kern der App als Wissensnachschlagewerk ist deutlich angelegt, aber noch als Vorschlag bzw. Idee in Ausarbeitung, nicht bereits fest entschieden.
- ACTION: ____   REASON: ____

## RR::canon_records_access_privacy_open  [review_required_claim / normal]
- proposition: Ob für Analyse, Tests oder inhaltliche Befüllung Einsicht in Bewohnerakten möglich ist, ist wegen Datenschutz unklar und muss geklärt werden.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Das Einsehen so einer Akte könnte eher schwierig aufgrund von Datenschutz werden. Aber ich werde mich mal umhören und euch Bescheid geben, falls es doch möglich sein sollte.
- reason: verdict=partial; Die Datenschutzfrage zum Aktenzugang ist klar offen und klärungsbedürftig. Nur der konkrete Zeitbezug auf MVP ist nicht belegt.
- ACTION: ____   REASON: ____

## RR::canon_digitization_desired_broadly  [review_required_claim / normal]
- proposition: Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 2: Und ehrlich gesagt sind wir hier sehr analog unterwegs und haben noch nicht viel von Digitalisierung erlebt. Alles, was digitalisiert werden kann, wäre für uns von großem Vorteil.
- reason: verdict=partial; Die Grundaussage ist gedeckt, aber der Zeithorizont ist im Transcript nicht als später verortet.
- ACTION: ____   REASON: ____

## RR::canon_employee_and_relative_access  [review_required_claim / normal]
- proposition: Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, um ergänzendes Wissen beizutragen.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 2: Ich fände es sehr sinnvoll, wenn auch die Angehörigen Zugriff drauf hätten. | Speaker 2: Wenn die auch ihre Daten da hinzufügen können, ist es von Vorteil, da sie oftmals Sachen wissen, die wir erst erfahren müssten.
- reason: verdict=partial; Der Kern ist gut belegt: Angehörige sollen Zugriff haben und Wissen beitragen können. Die Formulierung ist aber etwas stärker als die Quelle.
- ACTION: ____   REASON: ____

## RR::canon_no_go_page  [review_required_claim / normal]
- proposition: Die App soll pro Bewohner eine No-Go-Seite mit kritischen Dingen enthalten, die in Gegenwart des Bewohners zu vermeiden sind.
- systemSuggestion: facet_repair scope: sicherheits- und umgangshinweise pro bewohner->umgangshinweise pro bewohner
- evidence: Speaker 1: Wie wäre es neben dem About Me, Kommunikation, Video und Kalenderbildschirm noch ein No-Go Bildschirm zu haben? Da sollte man dann festhalten, was absolut gar nicht geht in Gegenwart des Bewohners. | Speaker 2: Diese No-Go Seite ist wirklich sehr wichtig. Die Thematik wird generell sehr unterschätzt.
- reason: verdict=partial; Eine No-Go-Seite pro Bewohner ist gut belegt; der Scope ist nur leicht weiter formuliert als die Quelle.
- ACTION: ____   REASON: ____

## RR::canon_calendar_and_medication_open  [review_required_claim / normal]
- proposition: Ein Kalender mit Terminen ist vorgesehen; ob auch Medikamentengaben integriert werden, bleibt wegen Vertraulichkeit und Umfang offen und muss geklärt werden.
- systemSuggestion: facet_repair modality: must_clarify->must_consider
- evidence: Speaker 2: Man könnte auch die Medikamentenvergabe als Terminkalender hinzufügen und die App generell für mehrere Sachen hernehmen? | Speaker 2: Medikamente werden etwas vertraulicher gehandhabt. | Speaker 1: Der Kalender sollte einfach und übersichtlich sein. Er könnte nicht nur Termine, sondern auch Medikamentengaben enthalten, was enorm wichtig ist.
- reason: verdict=partial; Der Eintrag trifft den offenen Charakter der Medikamentenfunktion gut. Die Facette bündelt jedoch Kalender und Medikation zusammen, obwohl der Kalender selbst schon deutlich vorgesehen ist.
- ACTION: ____   REASON: ____

## RR::canon_cross_platform_ios_android  [review_required_claim / normal]
- proposition: Die App soll plattformübergreifend auf iPhone/iOS und Android laufen.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 2: Wir könnten das so machen, dass wir eine plattformübergreifende Programmiersprache nutzen, die sowohl auf einem iPhone als auch auf einem Android-Handy laufen wird. | Speaker 1: Das wäre ideal. Dann können die App mehr Leute nutzen.
- reason: verdict=overstated; Plattformübergreifend für iPhone und Android wird befürwortet, aber im Stakeholder-Interview nur als vorgeschlagene Umsetzung formuliert.
- ACTION: ____   REASON: ____

## RR::canon_search_communication_entries_with_pattern  [review_required_claim / normal]
- proposition: Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisiertes Beschreibungsmuster für Kommunikations-Einträge definiert werden, um Suche und Filterung zu ermöglichen.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Vielleicht sollte diese Suchfunktion mittels einer Suchleiste oben im Bildschirm auch bei den Kommunikationsseiten möglich sein. | Speaker 1: Ich finde, so macht es am meisten Sinn. Machen Sie es genauso, wie Sie es gerade beschrieben haben. Natürlich mit der Suchfunktion. | Speaker 1: Dafür könnte man sich ein Muster überlegen. Wie man seine Beschreibung beim Hinzufügen des Bildes oder Ähnlichem formulieren soll, damit man später besser suchen kann.
- reason: verdict=partial; Suchfunktion auf Kommunikationsseiten und Beschreibungsmuster sind klar im Gespräch, aber nicht explizit zeitlich auf MVP festgelegt.
- ACTION: ____   REASON: ____

## RR::canon_about_me_notifications  [review_required_claim / normal]
- proposition: Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung per Popup benachrichtigt werden.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Jedes Mal, wenn etwas Neues im About Me Bildschirm hochgeladen wird, sollen alle User, die mit der App verbunden sind und Teil der Einrichtung sind, eine Popup-Nachricht erhalten. | Speaker 1: Das ist eine sehr schöne Idee.
- reason: verdict=partial; Popup-Benachrichtigungen werden konkret vorgeschlagen und positiv aufgenommen, aber nicht eindeutig als verpflichtend festgezurrt.
- ACTION: ____   REASON: ____

## RR::canon_resident_account_with_restrictions  [review_required_claim / normal]
- proposition: Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen kann und nur eingeschränkte Funktionen – insbesondere Zugriff auf About Me und Hinzufügen eigener Bilder – erhalten soll, um Fehlbedienungen zu minimieren.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Wieso sollte der Bewohner nicht auch einen Account bei der App haben? | Speaker 2: Sie haben recht. Das ist auch eine sehr gute Idee. | Speaker 2: Dieser Bewohner-Account sollte aber spezielle Rechte haben und auch nur sein eigenes Profil in der Profilübersicht, wo alle Profile gelistet sind, sehen.
- reason: verdict=partial; Bewohner-Account mit eingeschränkten Rechten und eigenem Profil ist gut gedeckt; die Verbindlichkeit ist jedoch eher als gewünschte Lösung formuliert.
- ACTION: ____   REASON: ____

## RR::canon_flutter_dart_stack  [review_required_claim / normal]
- proposition: Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen.
- systemSuggestion: facet_repair status: decided->uncertain
- evidence: Speaker 2: Ja, ich habe mal mit dem Framework Flutter und der Programmiersprache Dart eine App entwickelt und bin der Meinung, dass sich das recht schnell lernen lässt und wir damit schon mal abdecken können, dass es sowohl auf einem iOS Handy als auch auf einem Android läuft. | Speaker 2: Ideal, dann haben wir fürs Erste schon mal geklärt, welche Programmiersprache wir verwenden.
- reason: verdict=partial; Flutter/Dart wird im Team als aktueller Stack festgelegt, aber ausdrücklich nur vorläufig bzw. fürs Erste.
- ACTION: ____   REASON: ____

## RR::canon_android_studio_preferred  [review_required_claim / normal]
- proposition: Das Team möchte möglichst dieselbe Entwicklungsumgebung nutzen und präferiert dafür Android Studio.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 1: Mir ist ein IntelliJ ähnliches System lieber und ich würde deswegen auch Android Studio verwenden. | Speaker 1: ...obwohl ich trotzdem vorschlagen würde, dass wir alle dieselbe Entwicklungsumgebung nutzen. | Speaker 2: Ja, ich bleibe dabei und wechsle notfalls auch auf Android Studio.
- reason: verdict=partial; Präferenz für Android Studio und Wunsch nach einheitlicher IDE sind belegt, aber als endgültige Teamentscheidung etwas zu stark.
- ACTION: ____   REASON: ____

## RR::canon_align_versions  [review_required_claim / normal]
- proposition: Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um Integrationsprobleme zu vermeiden.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 1: Wichtig ist, dass wir dabei alle dieselbe Versionen installieren, damit wir später keine Probleme haben, wenn wir mit der Entwicklung beginnen.
- reason: verdict=partial; Der Kern ist gut gestützt; die Formulierung als bereits entschieden ist leicht zu stark.
- ACTION: ____   REASON: ____

## RR::canon_develop_for_android11_initially  [review_required_claim / normal]
- proposition: Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 2: Ja, dann würde ich empfehlen, dass wir erst mal alle für Android 11 entwickeln, da ich ein echtes Android mit dieser Version besitze... | Speaker 1: Das hört sich perfekt an.
- reason: verdict=partial; Inhaltlich gedeckt, aber Status und Modalität sind stärker formuliert als im Transcript.
- ACTION: ____   REASON: ____

## RR::canon_consistent_appbar_after_login  [review_required_claim / normal]
- proposition: Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein, die auch ein Einstellungssymbol zur Navigationsmöglichkeit in die Einstellungen enthält.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 1: Nach dem Login sollte auf jeder Seite ganz oben eine Appbar sein. | Speaker 2: Diese Appbar sollte nach dem Login auf jeder Seite oben sein. | Speaker 2: Vielleicht wäre auch ein Einstellungssymbol in der Appbar sinnvoll, damit man zu den Einstellungen kommen kann.
- reason: verdict=partial; Die konsistente Appbar ist gut gedeckt; die Einbeziehung des Einstellungssymbols ist etwas weniger verbindlich.
- ACTION: ____   REASON: ____

## RR::canon_accessible_design  [review_required_claim / normal]
- proposition: Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz.
- systemSuggestion: facet_repair status: decided->open
- evidence: Speaker 2: Bei all diesen Screens sollten wir auch die Barrierefreiheit im Auge behalten. Große Schrift, ausreichend Kontrast und die Vermeidung von zu vielen Farben helfen dabei, die App für alle zugänglich zu machen.
- reason: verdict=partial; Barrierefreiheit ist klar gewünscht und begründet, aber die Verbindlichkeit ist etwas überschätzt.
- ACTION: ____   REASON: ____

## RR::canon_alternative_input_consideration  [review_required_claim / normal]
- proposition: Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 1: Wir sollten vielleicht auch Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen.
- reason: verdict=partial; Die Aussage ist als zu berücksichtigende offene Idee gedeckt, aber ein späterer Zeitpunkt wird nicht genannt.
- ACTION: ____   REASON: ____

## RR::canon_animations_not_priority  [review_required_claim / normal]
- proposition: Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein.
- systemSuggestion: facet_repair modality: must_not->must_consider
- evidence: Speaker 2: Nee, ich denke nicht, dass wir das einbauen sollen. Aber wir sollten darauf achten, dass sie nicht zu ablenkend sind.
- reason: verdict=partial; Kern ist gedeckt: Animationen sind nicht vorgesehen und sollen nicht ablenken; die Formulierung als striktes Verbot und MVP-Festlegung ist aber zu stark.
- ACTION: ____   REASON: ____

## US::AU-0003  [unit_signal / unit]
- proposition: Enthält zusätzliche Begründung für den Bedarf: Kommunikation ist im Alltag zentral; außerdem kam der Impuls nicht von Mitarbeitern, sondern aus dem Projektkontext.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: app_goal_digital_comm_support | digitization_desired_broadly
- reason: Enthält zusätzliche Begründung für den Bedarf: Kommunikation ist im Alltag zentral; außerdem kam der Impuls nicht von Mitarbeitern, sondern aus dem Projektkontext.
- ACTION: ____   REASON: ____

## US::AU-0017  [unit_signal / unit]
- proposition: Liefert starke Evidenz für den Bedarf nach dokumentiertem Kommunikationswissen und beschreibt, dass implizites Wissen von Mitarbeitern für Außenstehende nicht zugänglich ist.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: app_as_knowledge_lookup | resident_records_exist | new_staff_need_support
- reason: Liefert starke Evidenz für den Bedarf nach dokumentiertem Kommunikationswissen und beschreibt, dass implizites Wissen von Mitarbeitern für Außenstehende nicht zugänglich ist.
- ACTION: ____   REASON: ____

## US::AU-0021  [unit_signal / unit]
- proposition: Beschreibt den Use Case neuer Mitarbeiter als Anlass für Kommunikationsunterstützung; der Claim dazu existiert bereits.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: new_staff_need_support
- reason: Beschreibt den Use Case neuer Mitarbeiter als Anlass für Kommunikationsunterstützung; der Claim dazu existiert bereits.
- ACTION: ____   REASON: ____

## US::AU-0034  [unit_signal / unit]
- proposition: Die Frage nach Zugriff stützt die bereits erfassten Anforderungen zu Rollen, Rechten und Benutzergruppen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: employee_and_relative_access | role_based_accounts
- reason: Die Frage nach Zugriff stützt die bereits erfassten Anforderungen zu Rollen, Rechten und Benutzergruppen.
- ACTION: ____   REASON: ____

## US::AU-0035  [unit_signal / unit]
- proposition: Belegt den Ausgangspunkt, dass zunächst nur Mitarbeiter als Nutzer vorgesehen waren, bevor weitere Gruppen ergänzt wurden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: employee_and_relative_access
- reason: Belegt den Ausgangspunkt, dass zunächst nur Mitarbeiter als Nutzer vorgesehen waren, bevor weitere Gruppen ergänzt wurden.
- ACTION: ____   REASON: ____

## US::AU-0048  [unit_signal / unit]
- proposition: Beschreibt UI-Details zum Hinzufügen von Bildern und zur Sortierung neuer Inhalte nach oben; das stützt die dynamische Erweiterbarkeit.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: dynamic_media_growth | about_me_page
- reason: Beschreibt UI-Details zum Hinzufügen von Bildern und zur Sortierung neuer Inhalte nach oben; das stützt die dynamische Erweiterbarkeit.
- ACTION: ____   REASON: ____

## US::AU-0053  [unit_signal / unit]
- proposition: Spezifiziert für die Videoseite dieselbe dynamische Hinzufügung und Sortierung neuester Inhalte nach oben.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: video_examples_for_communication | dynamic_media_growth
- reason: Spezifiziert für die Videoseite dieselbe dynamische Hinzufügung und Sortierung neuester Inhalte nach oben.
- ACTION: ____   REASON: ____

## US::AU-0055  [unit_signal / unit]
- proposition: Ergänzt die No-Go-Seite um dynamische Erweiterbarkeit und liefert ein konkretes Beispiel aus der Praxis.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no_go_page | dynamic_media_growth
- reason: Ergänzt die No-Go-Seite um dynamische Erweiterbarkeit und liefert ein konkretes Beispiel aus der Praxis.
- ACTION: ____   REASON: ____

## US::AU-0062  [unit_signal / unit]
- proposition: Zeigt, dass die konkrete Ausgestaltung der einrichtungsbezogenen Zugriffsregeln zum Zeitpunkt der Diskussion noch offen war.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: technical_solution_for_facility_scoping_open
- reason: Zeigt, dass die konkrete Ausgestaltung der einrichtungsbezogenen Zugriffsregeln zum Zeitpunkt der Diskussion noch offen war.
- ACTION: ____   REASON: ____

## US::AU-0077  [unit_signal / unit]
- proposition: Liefert eine zusammenfassende Nutzenargumentation für neue Mitarbeiter, dynamisch wachsende Inhalte und Beiträge von Angehörigen vor Aufnahme neuer Bewohner.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: new_staff_need_support | dynamic_media_growth | employee_and_relative_access
- reason: Liefert eine zusammenfassende Nutzenargumentation für neue Mitarbeiter, dynamisch wachsende Inhalte und Beiträge von Angehörigen vor Aufnahme neuer Bewohner.
- ACTION: ____   REASON: ____

## US::AU-0084  [unit_signal / unit]
- proposition: Begründet den Nutzen der Suchleiste auf der Profilübersicht mit besserer Navigation bei vielen Profilen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: search_profiles
- reason: Begründet den Nutzen der Suchleiste auf der Profilübersicht mit besserer Navigation bei vielen Profilen.
- ACTION: ____   REASON: ____

## US::AU-0089  [unit_signal / unit]
- proposition: Liefert Begründung für Admin-kontrollierte Accounts wegen sensibler Daten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login_only_no_self_registration | internal_use_only
- reason: Liefert Begründung für Admin-kontrollierte Accounts wegen sensibler Daten.
- ACTION: ____   REASON: ____

## US::AU-0096  [unit_signal / unit]
- proposition: Im Team besteht zumindest bei einem Mitglied Erfahrung bzw. Präferenz für Visual Studio Code als Entwicklungsumgebung; die endgültige Wahl der IDE war zu diesem Zeitpunkt noch nicht vollständig vereinheitlicht.
- systemSuggestion: compare_classification missing_claim
- evidence: android_studio_preferred
- reason: Die Präferenz bzw. vorhandene Kompetenz für Visual Studio Code als mögliche Entwicklungsumgebung ist im Ledger nicht abgebildet; aktuell ist nur Android Studio als Teampräferenz erfasst.
- ACTION: ____   REASON: ____

## US::AU-0097  [unit_signal / unit]
- proposition: Stützt die spätere Festlegung auf Flutter/Dart und erklärt die Präferenz für Android Studio aus vorhandener Erfahrung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: flutter_dart_stack | android_studio_preferred
- reason: Stützt die spätere Festlegung auf Flutter/Dart und erklärt die Präferenz für Android Studio aus vorhandener Erfahrung.
- ACTION: ____   REASON: ____

## US::AU-0106  [unit_signal / unit]
- proposition: Konkretisiert den bestehenden Datenbank- und Caching-Ansatz durch Nennung von lokaler Speicherung nach dem Laden aus Firebase.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: firebase_with_local_cache_open
- reason: Konkretisiert den bestehenden Datenbank- und Caching-Ansatz durch Nennung von lokaler Speicherung nach dem Laden aus Firebase.
- ACTION: ____   REASON: ____

## US::AU-0109  [unit_signal / unit]
- proposition: Präzisiert, dass sich die Hardwarefrage auf Emulator bzw. Betriebssystemversion bezieht, und stützt damit die bestehende Testumgebungsentscheidung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: develop_for_android11_initially
- reason: Präzisiert, dass sich die Hardwarefrage auf Emulator bzw. Betriebssystemversion bezieht, und stützt damit die bestehende Testumgebungsentscheidung.
- ACTION: ____   REASON: ____

## US::AU-0113  [unit_signal / unit]
- proposition: Alle Teammitglieder sollen ihre Entwicklungsumgebung installieren und testen; konkrete Programmier- und Kommentierungsregeln sollen im nächsten Teamtreffen festgelegt werden.
- systemSuggestion: compare_classification missing_claim
- reason: Enthält eigenständige Projektvorgehens-Aussagen zu gemeinsamer Installation/Setup-Prüfung und zu geplanten Coding-Regeln im nächsten Treffen, die im Ledger noch fehlen.
- ACTION: ____   REASON: ____

## US::AU-0117  [unit_signal / unit]
- proposition: Liefert UI-Details zum Login-Screen und zusätzlich Branding-Ideen, wobei Login-Struktur und Branding/Benutzerfreundlichkeit bereits im Ledger angelegt sind.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login_only_no_self_registration | accessible_design
- reason: Liefert UI-Details zum Login-Screen und zusätzlich Branding-Ideen, wobei Login-Struktur und Branding/Benutzerfreundlichkeit bereits im Ledger angelegt sind.
- ACTION: ____   REASON: ____

## US::AU-0118  [unit_signal / unit]
- proposition: Konkretisiert das Layout des Login-Screens mit E-Mail- und Passwortfeldern und stützt damit den bestehenden Login-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login_only_no_self_registration
- reason: Konkretisiert das Layout des Login-Screens mit E-Mail- und Passwortfeldern und stützt damit den bestehenden Login-Claim.
- ACTION: ____   REASON: ____

## US::AU-0119  [unit_signal / unit]
- proposition: Konkretisiert den Login-Screen und bekräftigt explizit, dass keine Registrierung vorgesehen ist.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login_only_no_self_registration
- reason: Konkretisiert den Login-Screen und bekräftigt explizit, dass keine Registrierung vorgesehen ist.
- ACTION: ____   REASON: ____

## US::AU-0121  [unit_signal / unit]
- proposition: Präzisiert die bereits erfasste konsistente Appbar sowie die Suchleiste auf der Profilübersicht.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: consistent_appbar_after_login | search_profiles | profile_overview_after_login
- reason: Präzisiert die bereits erfasste konsistente Appbar sowie die Suchleiste auf der Profilübersicht.
- ACTION: ____   REASON: ____

## US::AU-0122  [unit_signal / unit]
- proposition: Ergänzt Darstellungsdetails zur Profilübersichtsliste, die den bestehenden Claim zur Profilübersicht konkretisieren.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: profile_overview_after_login
- reason: Ergänzt Darstellungsdetails zur Profilübersichtsliste, die den bestehenden Claim zur Profilübersicht konkretisieren.
- ACTION: ____   REASON: ____

## US::AU-0123  [unit_signal / unit]
- proposition: Auf der Profilübersichtsseite soll das Anlegen neuer Profile über ein Plus-Symbol mit Eingabedialog für Bild, Name und Beschreibung möglich sein.
- systemSuggestion: compare_classification missing_claim
- reason: Die Möglichkeit, neue Profile über ein Plus-Symbol/Dialog mit Bild, Name und Beschreibung anzulegen, ist als eigenständige Funktion noch nicht im Ledger enthalten.
- ACTION: ____   REASON: ____

## US::AU-0124  [unit_signal / unit]
- proposition: Beim Öffnen eines Profils soll eine Detailansicht mit vergrößertem Profilbild und Navigationsbuttons zu den vier Hauptbereichen der App angezeigt werden.
- systemSuggestion: compare_classification missing_claim
- reason: Beschreibt eine eigenständige Detailansicht eines Profils mit großem Profilbild und Navigation zu vier Hauptbereichen; diese Struktur fehlt bislang.
- ACTION: ____   REASON: ____

## US::AU-0125  [unit_signal / unit]
- proposition: Konkretisiert die About-Me-Seite mit Infobox, Foto-Timeline, Plus-Button und Sortierung neuer Fotos; die Grundfunktion und dynamische Erweiterbarkeit sind bereits erfasst.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about_me_page | dynamic_media_growth
- reason: Konkretisiert die About-Me-Seite mit Infobox, Foto-Timeline, Plus-Button und Sortierung neuer Fotos; die Grundfunktion und dynamische Erweiterbarkeit sind bereits erfasst.
- ACTION: ____   REASON: ____

## US::AU-0126  [unit_signal / unit]
- proposition: Konkretisiert die visuelle Ausgestaltung der bereits festgelegten Trennung zwischen verbaler und nonverbaler Kommunikation.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: communication_split_verbal_nonverbal
- reason: Konkretisiert die visuelle Ausgestaltung der bereits festgelegten Trennung zwischen verbaler und nonverbaler Kommunikation.
- ACTION: ____   REASON: ____

## US::AU-0127  [unit_signal / unit]
- proposition: Ergänzt die bestehende Unterstützung für Videos/Beschreibungen und Suchbarkeit im Kommunikationsbereich um ein UI-Detail via Plus-Button.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: video_examples_for_communication | search_communication_entries | dynamic_media_growth
- reason: Ergänzt die bestehende Unterstützung für Videos/Beschreibungen und Suchbarkeit im Kommunikationsbereich um ein UI-Detail via Plus-Button.
- ACTION: ____   REASON: ____

## US::AU-0128  [unit_signal / unit]
- proposition: Konkretisiert die Gestaltung und Kompaktheit der bereits vorhandenen No-Go-Seite.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no_go_page
- reason: Konkretisiert die Gestaltung und Kompaktheit der bereits vorhandenen No-Go-Seite.
- ACTION: ____   REASON: ____

## US::AU-0129  [unit_signal / unit]
- proposition: Präzisiert die offene Kalender-/Medikationsfunktion durch Monatsansicht, Tags/Icons und Detailöffnung per Datum.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: calendar_and_medication_open
- reason: Präzisiert die offene Kalender-/Medikationsfunktion durch Monatsansicht, Tags/Icons und Detailöffnung per Datum.
- ACTION: ____   REASON: ____

## US::AU-0130  [unit_signal / unit]
- proposition: Stützt die bereits erfassten rollenabhängigen Einstellungen und Admin-Rechte mit konkreten Beispielen wie Sprache, Nutzeraccounts und Rechteanpassung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: role_based_accounts | settings_access | admin_screen_open
- reason: Stützt die bereits erfassten rollenabhängigen Einstellungen und Admin-Rechte mit konkreten Beispielen wie Sprache, Nutzeraccounts und Rechteanpassung.
- ACTION: ____   REASON: ____

## US::AU-0131  [unit_signal / unit]
- proposition: Konkretisiert bestehende Hilfe-/Tutorial-Überlegungen und ergänzt UI-Details für Datenschutz und Logout im Settings-Kontext, ohne klar neuen eigenständigen Muss-Claim zu begründen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: help_tutorial_features | settings_access
- reason: Konkretisiert bestehende Hilfe-/Tutorial-Überlegungen und ergänzt UI-Details für Datenschutz und Logout im Settings-Kontext, ohne klar neuen eigenständigen Muss-Claim zu begründen.
- ACTION: ____   REASON: ____

## US::AU-0136  [unit_signal / unit]
- proposition: Fasst bereits vorhandene Designziele wie Benutzerfreundlichkeit, Branding und Hilfe-Funktionen zusammen und verweist auf den Start der Prototypen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: help_tutorial_features | accessible_design
- reason: Fasst bereits vorhandene Designziele wie Benutzerfreundlichkeit, Branding und Hilfe-Funktionen zusammen und verweist auf den Start der Prototypen.
- ACTION: ____   REASON: ____

