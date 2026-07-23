# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | promote_to_claim | attach_evidence | merge_existing | mark_covered_by | reject | apply_repair | defer`
(promote_to_claim = unit als eigener Claim; attach_evidence/merge_existing/mark_covered_by brauchen `referenceTarget` (attach: Fallback claimId); apply_repair nutzt alle systemSuggestions.)

## RR::canon-solution-constraint-no-direct-translation  [review_required_claim / normal]
- proposition: The solution must not be a direct translation interface between general language and each resident's individual communication, because such an approach is considered infeasible and not cost-efficient.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: ... ausschließen, dass es sich um eine Art System handeln wird, das als Schnittstelle dient und zwischen Bewohner und zum Beispiel Betreuer hin und her übersetzt ... weder möglich noch kosteneffizient ... | Speaker 2: Das ist natürlich richtig und auch nicht so, wie ich es mir vorstelle.
- reason: verdict=partial; Die inhaltliche Abgrenzung gegen eine direkte Übersetzungsschnittstelle ist klar gedeckt und wirkt entschieden, aber nicht spezifisch als nur MVP-bezogen.
- ACTION: ____   REASON: ____

## RR::canon-process-requirements-elicitation-with-facilities-and-caregivers  [review_required_claim / normal]
- proposition: Requirements must be refined through further analysis involving facility visits and additional stakeholders such as caregivers.
- systemSuggestions: facet_repair status: decided->open | facet_repair modality: must->must_consider | facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: ... eine Art Anforderungsanalyse mit Ihnen und vielen weiteren Personen, die mit dem System arbeiten werden, durchführen ... | Speaker 2: Ja, wir können gerne einen Termin für so einen Kennenlern-Tag ausmachen. Wir können auch gerne in mehrere Einrichtungen fahren ... | Speaker 2: Im nächsten Termin werde ich dafür sorgen, dass ein Heilerziehungspfleger mit dabei ist ...
- reason: verdict=overstated; Die weitere Anforderungsanalyse mit Besuchen und Beteiligten ist klar vorgesehen, aber der Eintrag formuliert dies zu verbindlich und zu entschieden.
- ACTION: ____   REASON: ____

## RR::canon-product-direction-help-others-understand-resident  [review_required_claim / normal]
- proposition: If prioritization is needed, the solution should primarily help other people understand the resident better rather than helping residents understand caregivers.
- systemSuggestions: facet_repair modality: must->desired | facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Ja, wenn man sich entscheiden müsste, wäre es auf jeden Fall sinnvoller, wenn man einen Weg finden würde, die Bewohner besser zu verstehen. | Speaker 1: ... haben wir dabei einen Weg zu finden, wie der Betreuer jeden Bewohner besser verstehen kann.
- reason: verdict=overstated; Die Produktstoßrichtung ist deutlich unterstützt, aber als Präferenz/Priorisierung formuliert, nicht als zwingende Muss-Anforderung.
- ACTION: ____   REASON: ____

## RR::canon-core-concept-app-for-resident-communication-knowledge  [review_required_claim / normal]
- proposition: The core solution should be an app that stores per-resident communication knowledge so users can quickly look up what behaviors or utterances might mean, especially to support onboarding and unfamiliar situations.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Was halten Sie von der Idee, eine App zu entwickeln, aus der Daten, aus der Akte bzw. allgemeines Wissen der Art und Weise, wie eine beeinträchtigte Person kommuniziert festgehalten wird? | Speaker 1: ... die Person X aber nicht verstehen kann, in der App nachschauen kann, was gemeint sein könnte ... | Speaker 2: Diese Idee hört sich super an.
- reason: verdict=partial; Die App als Kernkonzept ist klar aus den Gesprächen ableitbar und positiv bestätigt, aber die Quelle lässt noch Ausarbeitung offen statt bereits eine harte Muss-Festlegung zu setzen.
- ACTION: ____   REASON: ____

## RR::canon-feature-about-me-page  [review_required_claim / normal]
- proposition: The app must provide an 'About Me' page per resident with basic personal information and a growing image-based feed with descriptions.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: ... einen Teil gibt, der als allgemeines Kennenlernenfeld betrachtet werden kann ... mehrere Bilder mit Texten ... Hobbys und alles, was die Person etwas näher beschreibt. | Speaker 1: Erstens soll es einen Button geben, der zu einem Feld führt, dem wir als About Me Seite ausgearbeitet haben ... mit Bildern, Beschreibungen, Hobbys, Name, Alter und so weiter. | Speaker 1: ... einen Dialog öffnet. Der es ermöglicht, ein Foto hinzuzufügen und einen Text dafür zu schreiben. Das neueste hinzugefügte Foto wird immer oben angezeigt.
- reason: verdict=partial; Die Funktion ist detailliert beschrieben und gestützt, jedoch eher als konkretisierte Lösungsidee als als bereits verbindlich festgelegte Muss-Anforderung.
- ACTION: ____   REASON: ____

## RR::canon-nfr-visual-content-not-text-only  [review_required_claim / normal]
- proposition: Communication-related information should not be text-only and should include visual representations such as images.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Wir dachten uns aber, dass es Sinn machen würde, visuelle Darstellungen zu präsentieren. Bilder, Texte und so weiter. | Speaker 2: Ja, es wäre auf jeden Fall vom Vorteil, wenn da nicht nur Texte stehen würden, sondern andere mögliche Darstellungen.
- reason: verdict=partial; Der Kern ist gedeckt: Kommunikationsinfos sollen nicht nur aus Text bestehen, sondern visuelle Darstellungen enthalten. Die Verbindlichkeit ist jedoch schwächer als 'must'.
- ACTION: ____   REASON: ____

## RR::canon-requirement-relatives-access-and-contribution  [review_required_claim / normal]
- proposition: Relatives should have access to the app and be able to contribute resident data, with role-specific rights.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Ich fände es sehr sinnvoll, wenn auch die Angehörigen Zugriff drauf hätten ... Wenn die auch ihre Daten da hinzufügen können, ist es von Vorteil ... | Speaker 1: ... dann machen wir das so, dass es verschiedene Accounts gibt. Also Mitarbeiter und Angehörige, die vermutlich auch verschiedene Rechte haben werden.
- reason: verdict=partial; Zugriff und Beitragsmöglichkeit für Angehörige mit unterschiedlichen Rechten sind inhaltlich klar gedeckt, aber die Formulierung ist eher wünschenswert als zwingend.
- ACTION: ____   REASON: ____

## RR::canon-open-standardized-description-pattern  [review_required_claim / normal]
- proposition: A standardized description pattern for communication entries must be defined to improve later search and filtering, especially for nonverbal communication.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: ... ein Muster überlegen ... wie man seine Beschreibung ... formulieren soll, damit man später besser suchen kann. | Speaker 2: Eventuell könnten Sie sich eine genauere Art der Formulierung überlegen und uns diese dann nennen ...
- reason: verdict=partial; Die Notwendigkeit eines standardisierten Beschreibungsmusters ist gedeckt, aber eine feste MVP-Zuordnung ist aus der Quelle nicht ableitbar.
- ACTION: ____   REASON: ____

## RR::canon-open-question-record-access-under-privacy  [review_required_claim / normal]
- proposition: Access to resident records for analysis and testing is uncertain because privacy or data-protection concerns may prevent inspection.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Das Einsehen so einer Akte könnte eher schwierig aufgrund von Datenschutz werden. Aber ich werde mich mal umhören und euch Bescheid geben, falls es doch möglich sein sollte.
- reason: verdict=partial; Die Unsicherheit des Aktenzugriffs wegen Datenschutz ist gedeckt, aber die zeitliche Einordnung als MVP ist nicht explizit belegt.
- ACTION: ____   REASON: ____

## RR::canon-requirement-global-appbar-and-settings  [review_required_claim / normal]
- proposition: After login, each page must have a consistent top app bar with back navigation, the current page title, settings access, and quick logout access.
- systemSuggestion: facet_repair modality: must->must_consider
- evidence: Speaker 2: Diese Appbar sollte nach dem Login auf jeder Seite oben sein. Vielleicht wäre auch ein Einstellungssymbol in der Appbar sinnvoll ... | Speaker 1: Nach dem Login sollte auf jeder Seite ganz oben eine Appbar sein ... Links ... Pfeil ... In der Mitte ... Name der aktuellen Seite ... rechts ein Einstellungssymbol. | Speaker 2: ... das Ausloggen sollte immer schnell erreichbar sein.
- reason: verdict=partial; Eine globale Appbar nach Login ist klar vorgesehen; Settings und schneller Logout sind jedoch eher vorgeschlagen bzw. als sinnvoll beschrieben als eindeutig verpflichtend festgelegt.
- ACTION: ____   REASON: ____

## RR::canon-optional-later-alternative-inputs  [review_required_claim / normal]
- proposition: Alternative input methods such as voice commands should be considered for impaired users as a possible later enhancement.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 1: Wir sollten vielleicht auch Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen.
- reason: verdict=partial; Alternative Eingabemethoden werden als zu berücksichtigende Möglichkeit genannt, aber nicht ausdrücklich als späteres Feature terminiert.
- ACTION: ____   REASON: ____

## RR::canon-constraint-avoid-distracting-animations  [review_required_claim / normal]
- proposition: Animations should not be a focus feature, and any animations used must not be distracting.
- systemSuggestion: facet_repair modality: must_not->must_consider
- evidence: Speaker 2: Nee, ich denke nicht, dass wir das einbauen sollen. Aber wir sollten darauf achten, dass sie nicht zu ablenkend sind.
- reason: verdict=partial; Die Quelle sagt nicht kategorisch, dass Animationen verboten sind, sondern dass sie wohl nicht eingebaut werden sollen und jedenfalls nicht ablenkend sein dürfen.
- ACTION: ____   REASON: ____

## US::AU-0002  [unit_signal / unit]
- proposition: Confirms stakeholder involvement and explicitly asks to include employees' ideas when shaping the system.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: field-observation-and-stakeholder-involvement
- reason: Confirms stakeholder involvement and explicitly asks to include employees' ideas when shaping the system.
- ACTION: ____   REASON: ____

## US::AU-0003  [unit_signal / unit]
- proposition: Provides background that the initiative did not originate from employees, that communication is a daily critical factor, and that the project is seen as an opportunity for development; this supports the product vision and problem context.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: goal-digital-communication-support
- reason: Provides background that the initiative did not originate from employees, that communication is a daily critical factor, and that the project is seen as an opportunity for development; this supports the product vision and problem context.
- ACTION: ____   REASON: ____

## US::AU-0006  [unit_signal / unit]
- proposition: Introduces the question whether communication style, diagnosis, and similar information are documented somewhere, which supports the candidate about existing resident records as an information source.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: use-existing-resident-records
- reason: Introduces the question whether communication style, diagnosis, and similar information are documented somewhere, which supports the candidate about existing resident records as an information source.
- ACTION: ____   REASON: ____

## US::AU-0017  [unit_signal / unit]
- proposition: Describes the observed difficulty that first-time viewers cannot communicate with the resident and asks whether tacit knowledge is documented; this strengthens the onboarding-support need.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-new-staff-onboarding | use-existing-resident-records
- reason: Describes the observed difficulty that first-time viewers cannot communicate with the resident and asks whether tacit knowledge is documented; this strengthens the onboarding-support need.
- ACTION: ____   REASON: ____

## US::AU-0019  [unit_signal / unit]
- proposition: Explicitly states that current documentation is less helpful afterwards and asks to inspect it, supporting both the pain point and the privacy-related uncertainty of access.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: documentation-hard-to-use | records-access-limited-by-privacy
- reason: Explicitly states that current documentation is less helpful afterwards and asks to inspect it, supporting both the pain point and the privacy-related uncertainty of access.
- ACTION: ____   REASON: ____

## US::AU-0047  [unit_signal / unit]
- proposition: Adds concrete content and ordering details for the About Me page, including hobbies, names, age, pictures with descriptions, plus-button, and newest-first ordering.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about-me-page
- reason: Adds concrete content and ordering details for the About Me page, including hobbies, names, age, pictures with descriptions, plus-button, and newest-first ordering.
- ACTION: ____   REASON: ____

## US::AU-0048  [unit_signal / unit]
- proposition: Adds interaction detail for About Me content creation via a separate add window and newest-first display, which supports the About Me feature design.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about-me-page
- reason: Adds interaction detail for About Me content creation via a separate add window and newest-first display, which supports the About Me feature design.
- ACTION: ____   REASON: ____

## US::AU-0049  [unit_signal / unit]
- proposition: Provides concrete structure for the communication section: split into verbal/nonverbal, separate navigation targets, and plus-button-based expansion with text or image entries.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: communication-page-verbal-nonverbal | visual-not-text-only
- reason: Provides concrete structure for the communication section: split into verbal/nonverbal, separate navigation targets, and plus-button-based expansion with text or image entries.
- ACTION: ____   REASON: ____

## US::AU-0051  [unit_signal / unit]
- proposition: Adds detail for the calendar feature: simple month-day interaction, adding entries, and a medication list beneath.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: calendar-medication-feature
- reason: Adds detail for the calendar feature: simple month-day interaction, adding entries, and a medication list beneath.
- ACTION: ____   REASON: ____

## US::AU-0053  [unit_signal / unit]
- proposition: Specifies the video page interaction model: add videos with descriptions via plus-button and show newest video first.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: video-support-for-scenarios
- reason: Specifies the video page interaction model: add videos with descriptions via plus-button and show newest video first.
- ACTION: ____   REASON: ____

## US::AU-0055  [unit_signal / unit]
- proposition: Adds concrete UI behavior for the No-Go section and gives an example of a resident-specific no-go.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no-go-page
- reason: Adds concrete UI behavior for the No-Go section and gives an example of a resident-specific no-go.
- ACTION: ____   REASON: ____

## US::AU-0059  [unit_signal / unit]
- proposition: Raises the sensitivity of data and the need to address data protection, reinforcing the existing project-scope/privacy clarification candidates.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: privacy-not-in-depth-now | records-access-limited-by-privacy
- reason: Raises the sensitivity of data and the need to address data protection, reinforcing the existing project-scope/privacy clarification candidates.
- ACTION: ____   REASON: ____

## US::AU-0062  [unit_signal / unit]
- proposition: Shows that cross-facility access handling was an open design question at that point, supporting the later access-control decision.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-facility-access-restricted
- reason: Shows that cross-facility access handling was an open design question at that point, supporting the later access-control decision.
- ACTION: ____   REASON: ____

## US::AU-0077  [unit_signal / unit]
- proposition: Explains why the app is valuable for new staff and first encounters, and adds that relatives could prefill data for new residents, supporting both onboarding and relatives' contribution.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-new-staff-onboarding | relatives-should-have-access
- reason: Explains why the app is valuable for new staff and first encounters, and adds that relatives could prefill data for new residents, supporting both onboarding and relatives' contribution.
- ACTION: ____   REASON: ____

## US::AU-0084  [unit_signal / unit]
- proposition: Provides rationale and concrete usage for search to simplify navigation across many profiles.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: search-profile-and-communication
- reason: Provides rationale and concrete usage for search to simplify navigation across many profiles.
- ACTION: ____   REASON: ____

## US::AU-0085  [unit_signal / unit]
- proposition: Adds detailed motivation and example structure for the standardized description pattern, including body-part-first and interpretation fields, supporting standardization and search/filtering.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: standardized-description-pattern | search-profile-and-communication
- reason: Adds detailed motivation and example structure for the standardized description pattern, including body-part-first and interpretation fields, supporting standardization and search/filtering.
- ACTION: ____   REASON: ____

## US::AU-0089  [unit_signal / unit]
- proposition: Provides rationale that admin-managed access is important for control in a sensitive-data environment, reinforcing the no-self-registration decision.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-only-no-self-registration | admin-user-roles
- reason: Provides rationale that admin-managed access is important for control in a sensitive-data environment, reinforcing the no-self-registration decision.
- ACTION: ____   REASON: ____

## US::AU-0092  [unit_signal / unit]
- proposition: Frage nach Cross-Platform für iOS und Android stützt den bestehenden Plattform-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-platform-ios-android
- reason: Frage nach Cross-Platform für iOS und Android stützt den bestehenden Plattform-Claim.
- ACTION: ____   REASON: ____

## US::AU-0093  [unit_signal / unit]
- proposition: Flutter/Dart wird als Mittel genannt, um iOS und Android gemeinsam abzudecken; das ist technische Evidenz zum bestehenden Cross-Platform-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-platform-ios-android
- reason: Flutter/Dart wird als Mittel genannt, um iOS und Android gemeinsam abzudecken; das ist technische Evidenz zum bestehenden Cross-Platform-Claim.
- ACTION: ____   REASON: ____

## US::AU-0095  [unit_signal / unit]
- proposition: Technology stack and toolchain choices for implementation (programming language/framework versions and IDE) should be defined consistently for the team.
- systemSuggestion: compare_classification needs_human
- reason: Enthält potenziell neue technische Festlegungen zu Programmiersprache/Versionierung/IDE, aber im Ledger gibt es dazu keinen konkreten Kandidaten und die Aussage ist teils vorläufig.
- ACTION: ____   REASON: ____

## US::AU-0096  [unit_signal / unit]
- proposition: The team should choose a shared development environment, with Visual Studio Code proposed as one option.
- systemSuggestion: compare_classification needs_human
- reason: Vorschlag für Visual Studio Code als IDE ist eine neue technische Detailentscheidung, die im Ledger bisher nicht abgebildet ist.
- ACTION: ____   REASON: ____

## US::AU-0097  [unit_signal / unit]
- proposition: Android Studio is a viable proposed development environment for the Flutter/Dart implementation.
- systemSuggestion: compare_classification needs_human
- reason: Begründet Android Studio als bekannte IDE für Flutter/Dart, aber es gibt keinen bestehenden Candidate für IDE-Entscheidungen.
- ACTION: ____   REASON: ____

## US::AU-0098  [unit_signal / unit]
- proposition: The team should use Android Studio as the development environment.
- systemSuggestion: compare_classification needs_human
- reason: Präferenz für Android Studio als gemeinsame Entwicklungsumgebung ist ein möglicher neuer Claim ohne vorhandenen Ledger-Eintrag.
- ACTION: ____   REASON: ____

## US::AU-0100  [unit_signal / unit]
- proposition: All team members should use the same development environment to reduce setup and compatibility problems.
- systemSuggestion: compare_classification needs_human
- reason: Enthält eine teamweite Vorgabe zur einheitlichen Entwicklungsumgebung; das ist im Ledger noch nicht als Claim vorhanden.
- ACTION: ____   REASON: ____

## US::AU-0101  [unit_signal / unit]
- proposition: The team is converging on Android Studio as a shared development environment.
- systemSuggestion: compare_classification needs_human
- reason: Persönliche Zustimmung zu Android Studio ist eher Evidence für eine mögliche IDE-Entscheidung, aber ohne bestehenden Candidate nicht direkt zuordenbar.
- ACTION: ____   REASON: ____

## US::AU-0102  [unit_signal / unit]
- proposition: All developers should install the same Flutter and Dart versions to avoid compatibility problems during development.
- systemSuggestion: compare_classification needs_human
- reason: Enthält konkrete Versionsfestlegungen und die wichtige Aussage, dass alle dieselben Versionen installieren sollen; dafür existiert kein Candidate.
- ACTION: ____   REASON: ____

## US::AU-0103  [unit_signal / unit]
- proposition: A database will likely be needed for the app and its selection must be decided.
- systemSuggestion: compare_classification needs_human
- reason: Leitet das Thema Datenbank ein, ist aber selbst nur eine Gesprächsfrage ohne eigenständigen belastbaren Claim.
- ACTION: ____   REASON: ____

## US::AU-0104  [unit_signal / unit]
- proposition: Firebase Firestore is the proposed backend database for the app because it is easy to use with Flutter/Dart and sufficient for the expected needs.
- systemSuggestion: compare_classification missing_claim
- reason: Konkrete Empfehlung von Firebase Firestore als Datenbanktechnologie ist eine eigenständige technische Architekturentscheidung, die im Ledger fehlt.
- ACTION: ____   REASON: ____

## US::AU-0105  [unit_signal / unit]
- proposition: It must be clarified how cloud-stored data is cached or stored locally on the device for app usage.
- systemSuggestion: compare_classification missing_claim
- reason: Klären von Cloud-zu-lokalem Speichern ist ein eigenständiges Architektur-/Offline-Verhaltensthema, das im Ledger nicht erfasst ist.
- ACTION: ____   REASON: ____

## US::AU-0106  [unit_signal / unit]
- proposition: The app should load data from Firebase when needed and store it locally on the device to avoid reloading everything repeatedly.
- systemSuggestion: compare_classification missing_claim
- reason: Beschreibt ein konkretes Datenkonzept mit Firebase plus lokaler Speicherung/Caching; das ist eine neue technische Aussage im Ledger.
- ACTION: ____   REASON: ____

## US::AU-0107  [unit_signal / unit]
- proposition: Bestätigt vorläufige Fokussierung auf Firebase und vertagt lokale Speicherungsdetails; das stützt die vorgeschlagene Firebase-Datenbankwahl.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-platform-ios-android
- reason: Bestätigt vorläufige Fokussierung auf Firebase und vertagt lokale Speicherungsdetails; das stützt die vorgeschlagene Firebase-Datenbankwahl.
- ACTION: ____   REASON: ____

## US::AU-0108  [unit_signal / unit]
- proposition: Hardware and test device conditions for development should be defined.
- systemSuggestion: compare_classification needs_human
- reason: Nur Themenwechsel zu Hardware, kein eigenständiger fachlicher Claim.
- ACTION: ____   REASON: ____

## US::AU-0109  [unit_signal / unit]
- proposition: The team should define which mobile OS version to target in the emulator/test setup.
- systemSuggestion: compare_classification needs_human
- reason: Präzisiert das Hardwarethema auf Emulator-/OS-Version, aber ohne bestehenden Candidate für Entwicklungs-Hardwarevorgaben.
- ACTION: ____   REASON: ____

## US::AU-0111  [unit_signal / unit]
- proposition: The team should initially develop and test against Android 11 as a common baseline environment.
- systemSuggestion: compare_classification missing_claim
- reason: Empfiehlt Android 11 als gemeinsame Entwicklungs-/Testbasis; das ist eine eigenständige technische Rahmenbedingung, die im Ledger fehlt.
- ACTION: ____   REASON: ____

## US::AU-0113  [unit_signal / unit]
- proposition: The team should define shared coding conventions such as comment rules before implementation proceeds.
- systemSuggestion: compare_classification needs_human
- reason: Teilweise Prozesshinweis, teilweise möglicher neuer Claim zu Coding-Regeln; im Ledger jedoch nicht konkret vorhanden und noch nicht ausformuliert.
- ACTION: ____   REASON: ____

## US::AU-0114  [unit_signal / unit]
- proposition: The team should consider using the JetX package to simplify implementation effort.
- systemSuggestion: compare_classification missing_claim
- reason: Vorschlag, das JetX-Paket zu verwenden, ist eine neue technische Entscheidung bzw. zu prüfende Architekturkomponente.
- ACTION: ____   REASON: ____

## US::AU-0115  [unit_signal / unit]
- proposition: JetX is accepted as part of the technical framework to be tried in the implementation.
- systemSuggestion: compare_classification missing_claim
- reason: Nimmt JetX in die technischen Rahmenbedingungen auf; das geht über bloße Erwähnung hinaus und fehlt im Ledger.
- ACTION: ____   REASON: ____

## US::AU-0117  [unit_signal / unit]
- proposition: The login screen should prominently show a communication-themed logo in the upper area.
- systemSuggestion: compare_classification needs_human
- reason: Login-Screen-Layout und Logo-Idee könnten bestehende UI-Claims zum Login konkretisieren, aber es gibt keinen spezifischen Candidate für Logo-/Layoutdetails des Login-Screens.
- ACTION: ____   REASON: ____

## US::AU-0118  [unit_signal / unit]
- proposition: The login screen should place intuitive email and password fields directly below the logo.
- systemSuggestion: compare_classification needs_human
- reason: Detailliert Anordnung und Gestaltung von E-Mail-/Passwort-Feldern; kein passender bestehender Candidate für diese UI-Details.
- ACTION: ____   REASON: ____

## US::AU-0119  [unit_signal / unit]
- proposition: Bestätigt, dass der Login-Screen nur Login ohne Registrierung enthält; das ist direkte Evidenz für den bestehenden Authentifizierungs-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-only-no-self-registration
- reason: Bestätigt, dass der Login-Screen nur Login ohne Registrierung enthält; das ist direkte Evidenz für den bestehenden Authentifizierungs-Claim.
- ACTION: ____   REASON: ____

## US::AU-0121  [unit_signal / unit]
- proposition: Beschreibt Appbar mit Titel sowie Suchleiste auf der Profilübersicht und konkretisiert damit bestehende Navigations- und Such-Claims.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: appbar-and-settings | search-profile-and-communication | profile-overview-after-login
- reason: Beschreibt Appbar mit Titel sowie Suchleiste auf der Profilübersicht und konkretisiert damit bestehende Navigations- und Such-Claims.
- ACTION: ____   REASON: ____

## US::AU-0122  [unit_signal / unit]
- proposition: Konkretisiert die Profilübersicht als Kachel/Liste mit Bild, Name und Kurzbeschreibung; das stützt den bestehenden Profilübersichts-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: profile-overview-after-login
- reason: Konkretisiert die Profilübersicht als Kachel/Liste mit Bild, Name und Kurzbeschreibung; das stützt den bestehenden Profilübersichts-Claim.
- ACTION: ____   REASON: ____

## US::AU-0123  [unit_signal / unit]
- proposition: The profile overview should allow creating a new resident profile via a prominent plus button that opens a dialog for image, name, and description.
- systemSuggestion: compare_classification missing_claim
- reason: Plus-Symbol und Dialog zum Anlegen neuer Profile ist eine eigenständige Funktion, die im Ledger bisher nicht explizit enthalten ist.
- ACTION: ____   REASON: ____

## US::AU-0124  [unit_signal / unit]
- proposition: Konkretisiert die Detailansicht nach Auswahl eines Profils und passt zur bestehenden Navigation von Profilübersicht zu Resident-Details.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: profile-overview-after-login
- reason: Konkretisiert die Detailansicht nach Auswahl eines Profils und passt zur bestehenden Navigation von Profilübersicht zu Resident-Details.
- ACTION: ____   REASON: ____

## US::AU-0125  [unit_signal / unit]
- proposition: Beschreibt die About-Me-Seite als Infobox plus Foto-Timeline mit neuestem Inhalt oben; das konkretisiert den bestehenden About-Me-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about-me-page
- reason: Beschreibt die About-Me-Seite als Infobox plus Foto-Timeline mit neuestem Inhalt oben; das konkretisiert den bestehenden About-Me-Claim.
- ACTION: ____   REASON: ____

## US::AU-0126  [unit_signal / unit]
- proposition: Konkretisiert die Kommunikationsseite mit visueller Trennung von verbal/nonverbal und klaren Buttons; das stützt den bestehenden Kommunikationsseiten-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: communication-page-verbal-nonverbal
- reason: Konkretisiert die Kommunikationsseite mit visueller Trennung von verbal/nonverbal und klaren Buttons; das stützt den bestehenden Kommunikationsseiten-Claim.
- ACTION: ____   REASON: ____

## US::AU-0127  [unit_signal / unit]
- proposition: Spezifiziert den bestehenden Claim zu nonverbaler Kommunikation mit Video/Beschreibung und Suchbarkeit für Gesten/Aktionen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: communication-page-verbal-nonverbal | video-support-for-scenarios | search-profile-and-communication
- reason: Spezifiziert den bestehenden Claim zu nonverbaler Kommunikation mit Video/Beschreibung und Suchbarkeit für Gesten/Aktionen.
- ACTION: ____   REASON: ____

## US::AU-0128  [unit_signal / unit]
- proposition: Liefert UI-Details für den bestehenden No-Go-Bereich: starkes visuelles Symbol und reduzierte, leicht durchsuchbare Einträge.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no-go-page
- reason: Liefert UI-Details für den bestehenden No-Go-Bereich: starkes visuelles Symbol und reduzierte, leicht durchsuchbare Einträge.
- ACTION: ____   REASON: ____

## US::AU-0129  [unit_signal / unit]
- proposition: Verfeinert den bestehenden Kalender-Claim um Monatsansicht, Icons/Tags für Medikamente und Detailansicht per Tap.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: calendar-medication-feature
- reason: Verfeinert den bestehenden Kalender-Claim um Monatsansicht, Icons/Tags für Medikamente und Detailansicht per Tap.
- ACTION: ____   REASON: ____

## US::AU-0130  [unit_signal / unit]
- proposition: Konkretisiert bestehende Rollen- und Settings-Anforderungen: differenzierte Settings nach Nutzerrolle, Admin verwaltet Accounts/Rechte, alle Nutzer können Profil und Sprache anpassen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: admin-user-roles | admin-screen-and-three-roles | appbar-and-settings
- reason: Konkretisiert bestehende Rollen- und Settings-Anforderungen: differenzierte Settings nach Nutzerrolle, Admin verwaltet Accounts/Rechte, alle Nutzer können Profil und Sprache anpassen.
- ACTION: ____   REASON: ____

## US::AU-0136  [unit_signal / unit]
- proposition: Fasst bereits vorhandene Leitlinien zu zugänglichem/intuitivem Design und Hilfe-Funktionen zusammen, ohne klar neuen eigenständigen Claim zu ergänzen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: accessibility-design | help-or-tutorial
- reason: Fasst bereits vorhandene Leitlinien zu zugänglichem/intuitivem Design und Hilfe-Funktionen zusammen, ohne klar neuen eigenständigen Claim zu ergänzen.
- ACTION: ____   REASON: ____

