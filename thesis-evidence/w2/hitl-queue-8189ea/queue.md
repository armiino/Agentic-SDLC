# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | promote_to_claim | attach_evidence | merge_existing | mark_covered_by | reject | apply_repair | defer`
(promote_to_claim = unit als eigener Claim; attach_evidence/merge_existing/mark_covered_by brauchen `referenceTarget` (attach: Fallback claimId); apply_repair nutzt alle systemSuggestions.)

## RR::CAN-004  [review_required_claim / normal]
- proposition: Es soll eine Anforderungsanalyse mit beteiligten Personen und künftigen Nutzern durchgeführt werden, um Bedürfnisse und Funktionen des Systems genauer zu erarbeiten.
- systemSuggestion: facet_repair modality: must->must_note
- evidence: Speaker 1: Grundsätzlich werden wir jetzt eine Art Anforderungsanalyse mit Ihnen und vielen weiteren Personen, die mit dem System arbeiten werden, durchführen, um herauszufinden, wo genau die Bedürfnisse liegen und was genau das System eigentlich können soll. | Speaker 1: Im Rahmen der Anforderungsanalyse, um die Anforderungen an die App genau zu besprechen, ist es für uns sowieso am besten, wenn wir alle oder zumindest einige der Beteiligten und Nutzer der App kennenlernen könnten, damit auch ihre Bedürfnisse berücksichtigt werden.
- reason: verdict=partial; Die Anforderungsanalyse mit Beteiligten und künftigen Nutzern wird klar beschrieben, jedoch eher als Vorgehensplanung des Teams als als verbindliche fachliche Muss-Anforderung.
- ACTION: ____   REASON: ____

## RR::CAN-005  [review_required_claim / normal]
- proposition: Das Team soll Kennenlern- und Beobachtungstermine in einer oder mehreren Einrichtungen durchführen können, um die aktuelle Kommunikationssituation besser zu verstehen.
- systemSuggestion: facet_repair modality: must->optional
- evidence: Speaker 2: Ja, wir können gerne einen Termin für so einen Kennenlern-Tag ausmachen. Wir können auch gerne in mehrere Einrichtungen fahren, um ein größeres Gesamtbild der Einrichtung-NoName zu schaffen.
- reason: verdict=partial; Termine in einer oder mehreren Einrichtungen sind ausdrücklich möglich, jedoch nur als angebotene Option und nicht als zwingende Anforderung.
- ACTION: ____   REASON: ____

## RR::CAN-007  [review_required_claim / normal]
- proposition: Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein könnte.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Was halten Sie von der Idee, eine App zu entwickeln, aus der Daten, aus der Akte bzw. allgemeines Wissen der Art und Weise, wie eine beeinträchtigte Person kommuniziert festgehalten wird? | Speaker 1: Diese App wäre für jeden, der mit einer Person, die er nicht verstehen kann, eine Lösung, um nachzuschauen, was gemeint sein könnte. | Speaker 1: Sie wollen eine App, die die Kommunikation zwischen einer beeinträchtigten Person und einer beliebigen anderen Person unterstützt, ermöglicht oder verbessert.
- reason: verdict=overstated; Der Kern der App-Idee ist gedeckt, aber die Formulierung als Muss ist stärker als die Quelle, die zunächst eine vielversprechende Idee beschreibt.
- ACTION: ____   REASON: ____

## RR::CAN-009  [review_required_claim / normal]
- proposition: Bestehende Dokumentationen können für Mitarbeitende schwer nutzbar sein, weil sie umfangreich sind und gesuchte Informationen oft nicht schnell gefunden werden.
- systemSuggestion: facet_repair proposition: Bestehende Dokumentationen können für Mitarbeitende schwer nutzbar sein, weil sie umfangreich sind und gesuchte Informationen oft nicht schnell gefunden werden.->
- evidence: Speaker 2: Ob sie gelesen werden, kann ich nicht persönlich beantworten, weil ich sie definitiv nicht lese. Es ist einfach zu viel zu lesen und oft findet man auch nicht das, wonach man sucht. | Speaker 2: Das war nur meine persönliche Meinung zu der Frage.
- reason: verdict=partial; Kern ist belegt, aber die Verallgemeinerung auf Mitarbeitende insgesamt ist gegenüber der Quelle etwas zu breit.
- ACTION: ____   REASON: ____

## RR::CAN-013  [review_required_claim / normal]
- proposition: Kommunikationswissen in der App muss dynamisch erweitert werden können; neue Erfahrungen sollen hinzugefügt werden können.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 2: Neue Erfahrungen müssen hinzugefügt werden können. | Speaker 1: Ja, wir haben uns gedacht, dass man das eben auch durch einen Plus-Button alles erweitern kann.
- reason: verdict=partial; Die Anforderung ist inhaltlich gedeckt, der Status ist aber zu offen angesetzt.
- ACTION: ____   REASON: ____

## RR::CAN-014  [review_required_claim / normal]
- proposition: Die App soll Kommunikationsinhalte nicht nur als Text, sondern auch in anderen Darstellungsformen wie Bildern visualisieren.
- systemSuggestions: facet_repair status: open->decided | facet_repair modality: must->desired
- evidence: Speaker 1: Wir dachten uns aber, dass es Sinn machen würde, visuelle Darstellungen zu präsentieren. Bilder, Texte und so weiter. | Speaker 2: Ja, es wäre auf jeden Fall vom Vorteil, wenn da nicht nur Texte stehen würden, sondern andere mögliche Darstellungen.
- reason: verdict=overstated; Der Kern ist belegt, aber als Muss zu stark formuliert.
- ACTION: ____   REASON: ____

## RR::CAN-024  [review_required_claim / normal]
- proposition: Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und Medikamentengaben bereitstellen.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Der Kalender sollte einfach und übersichtlich sein. Er könnte nicht nur Termine, sondern auch Medikamentengaben enthalten, was enorm wichtig ist. | Speaker 2: Absolut, das vereinfacht die tägliche Routine.
- reason: verdict=partial; Ein Kalender wird positiv besprochen, aber nicht so klar verbindlich festgelegt wie andere Funktionen.
- ACTION: ____   REASON: ____

## RR::CAN-028  [review_required_claim / normal]
- proposition: Die App soll möglichst plattformübergreifend auf iPhone und Android laufen.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 2: Wir könnten das so machen, dass wir eine plattformübergreifende Programmiersprache nutzen, die sowohl auf einem iPhone als auch auf einem Android-Handy laufen wird. | Speaker 1: Das wäre ideal. Dann können die App mehr Leute nutzen.
- reason: verdict=partial; iPhone- und Android-Unterstützung wird als gewünschte und vorgesehene Lösung besprochen; der offene Status ist etwas zu schwach passend.
- ACTION: ____   REASON: ____

## RR::CAN-030  [review_required_claim / normal]
- proposition: Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile schnell gefunden werden können.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 1: Wenn man auf der Profilseite ist, wo man eine Liste der Profile hat, sollte man nach einem Profil suchen können, um nicht ewig zu scrollen. | Speaker 2: Okay, das hört sich sinnvoll an und das werden wir definitiv umsetzen. | Speaker 2: Unter der Appbar wollen wir eine gut sichtbare Suchleiste einfügen.
- reason: verdict=partial; Die Suchfunktion in der Profilübersicht ist klar belegt; die konkrete Darstellung als Liste oder Kacheln ist eher UI-Ausgestaltung als gesicherte Anforderung.
- ACTION: ____   REASON: ____

## RR::CAN-031  [review_required_claim / normal]
- proposition: Auf der Profilübersicht soll es eine Funktion zum Anlegen neuer Profile mit Bild, Name und Beschreibung geben.
- systemSuggestions: facet_repair status: open->uncertain | facet_repair modality: must->optional
- evidence: Speaker 2: Und für das Hinzufügen neuer Profile brauchen wir ein klares Plus-Symbol unten auf dem Screen. Ein Dialogfeld sollte erscheinen, wenn man draufklickt, um ein neues Profil mit Bild, Name und Beschreibung anzulegen.
- reason: verdict=overstated; Ein Plus-Symbol zum Anlegen neuer Profile erscheint nur in der studentischen Designausarbeitung und ist nicht als verbindliche Anforderung belegt.
- ACTION: ____   REASON: ____

## RR::CAN-035  [review_required_claim / normal]
- proposition: Es soll einen Bewohner-Account mit speziellen, stark begrenzten Rechten geben, der nur das eigene Profil sehen sowie auf About Me zugreifen und dort eigene Bilder hinzufügen kann, um Fehlbedienungen zu minimieren.
- systemSuggestion: facet_repair status: decided->uncertain
- evidence: Speaker 1: Wieso sollte der Bewohner nicht auch einen Account bei der App haben? | Speaker 2: Dieser Bewohner-Account sollte aber spezielle Rechte haben und auch nur sein eigenes Profil in der Profilübersicht, wo alle Profile gelistet sind, sehen. | Speaker 1: Sie sollten auf den About Me Screen zugreifen können und dort eigene Bilder hinzufügen.
- reason: verdict=partial; Die Funktion ist inhaltlich gedeckt, aber der Festlegungsgrad ist etwas zu stark.
- ACTION: ____   REASON: ____

## RR::CAN-037  [review_required_claim / normal]
- proposition: Das Team soll möglichst dieselbe Entwicklungsumgebung verwenden, konkret Android Studio, und einheitliche Flutter- und Dart-Versionen installieren, um Entwicklungsprobleme zu vermeiden.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: obwohl ich trotzdem vorschlagen würde, dass wir alle dieselbe Entwicklungsumgebung nutzen. | Speaker 2: Ja, ich bleibe dabei und wechsle notfalls auch auf Android Studio. | Speaker 1: Wichtig ist, dass wir dabei alle dieselbe Versionen installieren, damit wir später keine Probleme haben, wenn wir mit der Entwicklung beginnen.
- reason: verdict=overstated; Der Eintrag verstärkt den Verbindlichkeitsgrad für Android Studio gegenüber der Quelle.
- ACTION: ____   REASON: ____

## RR::CAN-039  [review_required_claim / normal]
- proposition: Für die erste Entwicklung und Tests soll Android 11 als gemeinsame Zielversion verwendet werden.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Ja, dann würde ich empfehlen, dass wir erst mal alle für Android 11 entwickeln | Speaker 1: Das hört sich perfekt an.
- reason: verdict=overstated; Die Proposition ist gedeckt, aber die Modalität ist stärker als im Transcript.
- ACTION: ____   REASON: ____

## RR::CAN-043  [review_required_claim / normal]
- proposition: Die Einstellungsseite soll rollenabhängig differenziert sein; Admins sollen dort Nutzeraccounts hinzufügen und Rechte anpassen können, während alle Nutzer ihr Profil ändern und etwa die Sprache wählen können.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 1: Und für den Settings-Screen brauchen wir wirklich eine differenzierte Ansicht, je nach Nutzerrolle. Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen. Aber jeder Nutzer sollte sein Profil ändern und Einstellungen wie die Sprache wählen können.
- reason: verdict=partial; Der Inhalt ist gedeckt, aber der Status ist eher festgehalten als offen.
- ACTION: ____   REASON: ____

## RR::CAN-044  [review_required_claim / normal]
- proposition: Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim ersten Login und als später erneut aufrufbaren Hilfebereich.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Denkst du, wir sollten auch eine Hilfe-Funktion oder ein Tutorial einbauen? | Speaker 1: Auf jeden Fall. Eine kurze Tour durch die App beim ersten Login könnte helfen, die Nutzer mit den Funktionen vertraut zu machen. Und ein Hilfebereich, den man immer wieder aufrufen kann, wäre auch sinnvoll.
- reason: verdict=overstated; Der Kern ist gedeckt, aber die Modalität ist stärker als die Quelle hergibt.
- ACTION: ____   REASON: ____

## RR::CAN-045  [review_required_claim / normal]
- proposition: Bei allen Screens soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung zu vieler Farben.
- systemSuggestions: facet_repair status: open->decided | facet_repair modality: must->desired
- evidence: Speaker 2: Bei all diesen Screens sollten wir auch die Barrierefreiheit im Auge behalten. Große Schrift, ausreichend Kontrast und die Vermeidung von zu vielen Farben helfen dabei, die App für alle zugänglich zu machen.
- reason: verdict=partial; Die Richtung stimmt, aber Status und Modalität sind etwas zu stark bzw. unpassend gesetzt.
- ACTION: ____   REASON: ____

## RR::CAN-046  [review_required_claim / normal]
- proposition: Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer sollen geprüft werden.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 1: Wir sollten vielleicht auch Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen.
- reason: verdict=partial; Das Prüfen solcher Eingabemethoden ist gedeckt, aber der Zeithorizont ist nicht belegt.
- ACTION: ____   REASON: ____

## RR::CAN-047  [review_required_claim / normal]
- proposition: Ablenkende Animationen sollen nicht eingebaut werden.
- systemSuggestion: facet_repair modality: must_not->must_consider
- evidence: Speaker 2: Nee, ich denke nicht, dass wir das einbauen sollen. Aber wir sollten darauf achten, dass sie nicht zu ablenkend sind.
- reason: verdict=overstated; Die Aussage verschärft die Quelle zu einem generellen Verbot statt einer Vorsicht bei ablenkenden Animationen.
- ACTION: ____   REASON: ____

## US::AU-0003  [unit_signal / unit]
- proposition: Die Aussage unterstreicht die Relevanz täglicher Kommunikation als Kernproblem und stützt damit das Produktziel sowie den Kommunikationsfokus.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-001 | REQ-007
- reason: Die Aussage unterstreicht die Relevanz täglicher Kommunikation als Kernproblem und stützt damit das Produktziel sowie den Kommunikationsfokus.
- ACTION: ____   REASON: ____

## US::AU-0006  [unit_signal / unit]
- proposition: Die Unit konkretisiert, welche Informationen in bestehenden Dokumentationen erwartet werden, ohne einen neuen Requirement-Claim zu bilden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: SCOPE-008
- reason: Die Unit konkretisiert, welche Informationen in bestehenden Dokumentationen erwartet werden, ohne einen neuen Requirement-Claim zu bilden.
- ACTION: ____   REASON: ____

## US::AU-0013  [unit_signal / unit]
- proposition: Die Erwähnung der Medikamentenvergabe stützt die Kalenderfunktion; der allgemeine Wunsch nach Mehrzwecknutzung ergänzt zudem die offene Umfangsfrage.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-025 | OQ-012
- reason: Die Erwähnung der Medikamentenvergabe stützt die Kalenderfunktion; der allgemeine Wunsch nach Mehrzwecknutzung ergänzt zudem die offene Umfangsfrage.
- ACTION: ____   REASON: ____

## US::AU-0017  [unit_signal / unit]
- proposition: Die Unit belegt, dass relevantes Kommunikationswissen bei erfahrenen Mitarbeitenden vorhanden ist und fragt nach seiner Dokumentation in Akten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-007 | SCOPE-008 | RISK-009
- reason: Die Unit belegt, dass relevantes Kommunikationswissen bei erfahrenen Mitarbeitenden vorhanden ist und fragt nach seiner Dokumentation in Akten.
- ACTION: ____   REASON: ____

## US::AU-0024  [unit_signal / unit]
- proposition: Die Unit liefert zusätzliche Motivation für Digitalisierung insgesamt und speziell für eine mögliche Digitalisierung von Dokumentation.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-002 | OQ-012
- reason: Die Unit liefert zusätzliche Motivation für Digitalisierung insgesamt und speziell für eine mögliche Digitalisierung von Dokumentation.
- ACTION: ____   REASON: ____

## US::AU-0035  [unit_signal / unit]
- proposition: Die Unit dokumentiert einen frühen Zwischenstand zur Zielgruppe, der die spätere Erweiterung um weitere Nutzergruppen kontextualisiert.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-017 | DEC-019
- reason: Die Unit dokumentiert einen frühen Zwischenstand zur Zielgruppe, der die spätere Erweiterung um weitere Nutzergruppen kontextualisiert.
- ACTION: ____   REASON: ____

## US::AU-0050  [unit_signal / unit]
- proposition: Die Unit zeigt, dass der Kalender als eigener Themenblock relevant ist, fügt aber keine zusätzliche konkrete Anforderung hinzu.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-025
- reason: Die Unit zeigt, dass der Kalender als eigener Themenblock relevant ist, fügt aber keine zusätzliche konkrete Anforderung hinzu.
- ACTION: ____   REASON: ____

## US::AU-0055  [unit_signal / unit]
- proposition: Die Unit konkretisiert die No-Go-Seite durch eine dynamisch wachsende Liste und einen Beispielanwendungsfall.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-018
- reason: Die Unit konkretisiert die No-Go-Seite durch eine dynamisch wachsende Liste und einen Beispielanwendungsfall.
- ACTION: ____   REASON: ____

## US::AU-0062  [unit_signal / unit]
- proposition: Die Unit ist ein Gesprächsbeitrag zur gemeinsamen Festlegung der sinnvollsten Zugriffsregel und stützt die spätere Entscheidung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-029
- reason: Die Unit ist ein Gesprächsbeitrag zur gemeinsamen Festlegung der sinnvollsten Zugriffsregel und stützt die spätere Entscheidung.
- ACTION: ____   REASON: ____

## US::AU-0077  [unit_signal / unit]
- proposition: Die Unit stützt den Nutzen für neue Mitarbeitende, das dynamische Wachstum sowie den Beitrag von Angehörigen bei neuen Bewohnern.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-007 | REQ-013 | REQ-017
- reason: Die Unit stützt den Nutzen für neue Mitarbeitende, das dynamische Wachstum sowie den Beitrag von Angehörigen bei neuen Bewohnern.
- ACTION: ____   REASON: ____

## US::AU-0085  [unit_signal / unit]
- proposition: Die Unit liefert ein konkretes Beispiel für das noch zu definierende Beschreibungsmuster mit strukturierten Eingabefeldern.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: OQ-034 | REQ-033
- reason: Die Unit liefert ein konkretes Beispiel für das noch zu definierende Beschreibungsmuster mit strukturierten Eingabefeldern.
- ACTION: ____   REASON: ____

## US::AU-0088  [unit_signal / unit]
- proposition: Die Unit präzisiert die interne Accountverwaltung durch Admins sowie die Rollen Admin, User und Bewohner.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-019 | DEC-020 | REQ-037
- reason: Die Unit präzisiert die interne Accountverwaltung durch Admins sowie die Rollen Admin, User und Bewohner.
- ACTION: ____   REASON: ____

## US::AU-0089  [unit_signal / unit]
- proposition: Die Unit liefert Begründung für die restriktive Accountverwaltung in einer Umgebung mit sensiblen Daten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-019 | DEC-020
- reason: Die Unit liefert Begründung für die restriktive Accountverwaltung in einer Umgebung mit sensiblen Daten.
- ACTION: ____   REASON: ____

## US::AU-0094  [unit_signal / unit]
- proposition: Kenntnisse in Flutter/Dart sowie Hilfsbereitschaft stützen die Umsetzbarkeit der bereits festgehaltenen Technologieentscheidung, sind aber kein eigener fachlicher Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-039
- reason: Kenntnisse in Flutter/Dart sowie Hilfsbereitschaft stützen die Umsetzbarkeit der bereits festgehaltenen Technologieentscheidung, sind aber kein eigener fachlicher Claim.
- ACTION: ____   REASON: ____

## US::AU-0096  [unit_signal / unit]
- proposition: Es wurde im Team auch Visual Studio Code als mögliche Entwicklungsumgebung vorgeschlagen, bevor Android Studio als gemeinsame Umgebung festgelegt wurde.
- systemSuggestion: compare_classification needs_human
- reason: Die Unit dokumentiert eine Einzelpräferenz für Visual Studio Code, während im Ledger Android Studio als gemeinsame Entwicklungsumgebung entschieden ist; unklar ist, ob dies nur verworfen oder als relevante Alternativdiskussion festgehalten werden soll.
- ACTION: ____   REASON: ____

## US::AU-0097  [unit_signal / unit]
- proposition: Die Erfahrung mit Flutter/Dart in Android Studio stützt die bestehenden Entscheidungen zu Technologie-Stack und Entwicklungsumgebung, ohne einen neuen eigenständigen Claim zu bilden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-039 | DEC-040
- reason: Die Erfahrung mit Flutter/Dart in Android Studio stützt die bestehenden Entscheidungen zu Technologie-Stack und Entwicklungsumgebung, ohne einen neuen eigenständigen Claim zu bilden.
- ACTION: ____   REASON: ____

## US::AU-0098  [unit_signal / unit]
- proposition: Die Unit begründet die Verwendung von Android Studio mit dessen IntelliJ-Ähnlichkeit und ergänzt damit die bestehende Entscheidung zur Entwicklungsumgebung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-040
- reason: Die Unit begründet die Verwendung von Android Studio mit dessen IntelliJ-Ähnlichkeit und ergänzt damit die bestehende Entscheidung zur Entwicklungsumgebung.
- ACTION: ____   REASON: ____

## US::AU-0108  [unit_signal / unit]
- proposition: Die Hardware- und Geräteparameter für Entwicklung und Test der App müssen festgelegt werden.
- systemSuggestion: compare_classification needs_human
- reason: Die Unit benennt nur allgemein das Thema Hardware; ohne die nachfolgenden Konkretisierungen ist unklar, ob daraus ein eigenständiger relevanter Claim ableitbar ist.
- ACTION: ____   REASON: ____

## US::AU-0109  [unit_signal / unit]
- proposition: Die Unit präzisiert, dass mit Hardware die Betriebssystemversion im Emulator gemeint ist, und stützt damit die spätere Festlegung auf Android 11.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-044
- reason: Die Unit präzisiert, dass mit Hardware die Betriebssystemversion im Emulator gemeint ist, und stützt damit die spätere Festlegung auf Android 11.
- ACTION: ____   REASON: ____

## US::AU-0117  [unit_signal / unit]
- proposition: Logo auf dem Login-Screen ist bereits im Ledger enthalten; die genauere Platzierung und der Hinweis auf ein noch zu erstellendes kommunikatives Logo sind Detailausgestaltung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-047
- reason: Logo auf dem Login-Screen ist bereits im Ledger enthalten; die genauere Platzierung und der Hinweis auf ein noch zu erstellendes kommunikatives Logo sind Detailausgestaltung.
- ACTION: ____   REASON: ____

## US::AU-0118  [unit_signal / unit]
- proposition: Die Unit ergänzt die vorhandene Login-Screen-Entscheidung um Layout- und Usability-Details für E-Mail- und Passwortfeld.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-047
- reason: Die Unit ergänzt die vorhandene Login-Screen-Entscheidung um Layout- und Usability-Details für E-Mail- und Passwortfeld.
- ACTION: ____   REASON: ____

## US::AU-0120  [unit_signal / unit]
- proposition: Weiterleitung nach Login zur Profilseite ist bereits abgedeckt, ebenso die konsistente Appbar; Seitentitel und Settings-Icon konkretisieren die bestehende Navigationsentscheidung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: DEC-021 | DEC-026 | REQ-050
- reason: Weiterleitung nach Login zur Profilseite ist bereits abgedeckt, ebenso die konsistente Appbar; Seitentitel und Settings-Icon konkretisieren die bestehende Navigationsentscheidung.
- ACTION: ____   REASON: ____

## US::AU-0124  [unit_signal / unit]
- proposition: Bei Auswahl eines Profils soll eine Detailansicht erscheinen, die das Profilbild vergrößert zeigt und die vier Hauptbereiche der App als interaktive Buttons anbietet.
- systemSuggestion: compare_classification missing_claim
- reason: Die Unit beschreibt eine konkrete Anforderung an die Profile-Detailansicht nach Auswahl eines Profils, die über die bisher festgehaltene Profilübersicht und Bereichsdefinition hinausgeht.
- ACTION: ____   REASON: ____

## US::AU-0126  [unit_signal / unit]
- proposition: Die Trennung in verbal und nonverbal ist bereits abgedeckt; Symbole und klar erkennbare Buttons sind zusätzliche Ausgestaltung der Oberfläche.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-024
- reason: Die Trennung in verbal und nonverbal ist bereits abgedeckt; Symbole und klar erkennbare Buttons sind zusätzliche Ausgestaltung der Oberfläche.
- ACTION: ____   REASON: ____

## US::AU-0127  [unit_signal / unit]
- proposition: Das Hinzufügen von Videos und Beschreibungen für nonverbale Signale ergänzt bestehende Kommunikations- und Videoclaims; die Suchmotivation passt zu der bereits erfassten Suchfunktion.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-016 | REQ-033 | DEC-035
- reason: Das Hinzufügen von Videos und Beschreibungen für nonverbale Signale ergänzt bestehende Kommunikations- und Videoclaims; die Suchmotivation passt zu der bereits erfassten Suchfunktion.
- ACTION: ____   REASON: ____

## US::AU-0128  [unit_signal / unit]
- proposition: Die No-Go-Seite ist bereits festgehalten; visuelles Stoppsymbol und knappe, leicht durchsuchbare Einträge sind zusätzliche UI-Details.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-018
- reason: Die No-Go-Seite ist bereits festgehalten; visuelles Stoppsymbol und knappe, leicht durchsuchbare Einträge sind zusätzliche UI-Details.
- ACTION: ____   REASON: ____

## US::AU-0129  [unit_signal / unit]
- proposition: Die Unit ergänzt die vorhandene Kalenderanforderung um konkrete Darstellungs- und Interaktionsdetails für Monatsansicht, Markierungen und Detailaufruf.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: REQ-025
- reason: Die Unit ergänzt die vorhandene Kalenderanforderung um konkrete Darstellungs- und Interaktionsdetails für Monatsansicht, Markierungen und Detailaufruf.
- ACTION: ____   REASON: ____

