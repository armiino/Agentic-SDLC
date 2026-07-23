# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | promote_to_claim | attach_evidence | merge_existing | mark_covered_by | reject | apply_repair | defer`
(promote_to_claim = unit als eigener Claim; attach_evidence/merge_existing/mark_covered_by brauchen `referenceTarget` (attach: Fallback claimId); apply_repair nutzt alle systemSuggestions.)

## RR::canon-anforderungsanalyse-mit-nutzern  [review_required_claim / normal]
- proposition: Die genauen Bedürfnisse und Funktionen des Systems müssen durch Anforderungsanalyse mit mehreren Beteiligten und künftigen Nutzern erarbeitet werden.
- systemSuggestions: facet_repair modality: must->must_note | facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: Grundsätzlich werden wir jetzt eine Art Anforderungsanalyse mit Ihnen und vielen weiteren Personen, die mit dem System arbeiten werden, durchführen, um herauszufinden, wo genau die Bedürfnisse liegen und was genau das System eigentlich können soll. | Speaker 1: Wie und was die App genau können soll, werden wir in den nächsten Terminen erarbeiten.
- reason: verdict=partial; Der Kern ist gedeckt: Bedürfnisse und Funktionen sollen mit mehreren Beteiligten erarbeitet werden. Die Facetten sind aber etwas zu stark bzw. zu konkret.
- ACTION: ____   REASON: ____

## RR::canon-beobachtung-vor-ort  [review_required_claim / normal]
- proposition: Zur Nutzerforschung sollen Vor-Ort-Termine in Einrichtungen durchgeführt werden, um reale Kommunikationssituationen und Bedürfnisse besser zu verstehen.
- systemSuggestions: facet_repair modality: must->must_note | facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: Wäre es eventuell möglich, eine beeinträchtigte Person kennenzulernen? | Speaker 2: Ja, wir können gerne einen Termin für so einen Kennenlern-Tag ausmachen. Wir können auch gerne in mehrere Einrichtungen fahren, um ein größeres Gesamtbild der Einrichtung-NoName zu schaffen.
- reason: verdict=partial; Vor-Ort-Termine zur besseren Einschätzung der Situation sind klar gedeckt. Die Einordnung als Muss im MVP ist jedoch stärker als die Quelle.
- ACTION: ____   REASON: ____

## RR::canon-priorisierung-bewohner-besser-verstehen  [review_required_claim / normal]
- proposition: Die Unterstützung soll vorrangig darauf ausgerichtet sein, dass Betreuer oder andere Personen Bewohner besser verstehen können.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Ja, wenn man sich entscheiden müsste, wäre es auf jeden Fall sinnvoller, wenn man einen Weg finden würde, die Bewohner besser zu verstehen. | Speaker 1: Also haben wir dabei einen Weg zu finden, wie der Betreuer jeden Bewohner besser verstehen kann.
- reason: verdict=overstated; Die Priorisierung, Bewohner besser zu verstehen, ist gedeckt. Als Muss ist es aber zu stark formuliert; es wird eher als sinnvoll bzw. bevorzugt beschrieben.
- ACTION: ____   REASON: ____

## RR::canon-app-profilwissen-kommunikationshilfe  [review_required_claim / normal]
- proposition: Als Kernansatz soll eine App Wissen über Profil und Kommunikationsweise einer Person bereitstellen, damit Nutzer bei Verständnisschwierigkeiten nachsehen können.
- systemSuggestions: facet_repair status: open->decided | facet_repair modality: must->desired
- evidence: Speaker 1: Was halten Sie von der Idee, eine App zu entwickeln, aus der Daten, aus der Akte bzw. allgemeines Wissen der Art und Weise, wie eine beeinträchtigte Person kommuniziert festgehalten wird? | Speaker 1: Die App wäre für jeden, der mit einer Person, die er nicht verstehen kann, eine Lösung, um nachzuschauen, was gemeint sein könnte. | Speaker 1: Sie wollen eine App, die die Kommunikation zwischen einer beeinträchtigten Person und einer beliebigen anderen Person unterstützt, ermöglicht oder verbessert.
- reason: verdict=overstated; Die App als Kernansatz ist klar im Gespräch entwickelt und positiv bestätigt. Der Eintrag ist aber bei den Facetten uneinheitlich: Status ist eher festgehalten, während die Modalität als Muss stärker ist als die Quelle hergibt.
- ACTION: ____   REASON: ____

## RR::canon-erweiterung-dokumentation-pruefen  [review_required_claim / normal]
- proposition: Eine Ausweitung der App auf weitere Dokumentationsfunktionen soll geprüft werden, jedoch nur in begrenztem Umfang und ohne den Fokus auf unterstützende Kommunikation zu verlieren.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 2: Alles, was digitalisiert werden kann, wäre für uns von großem Vorteil. Somit auch eventuell die ganze Dokumentation? | Speaker 1: Dennoch werden wir das mit in unsere Anforderungen packen und mit dem Leiter des Einrichtung-NoName, der am Ende auch alles absegnen muss, klären, ob eine volle Dokumentation von der App übernommen werden soll. | Speaker 1: Auch wenn ich hier schon mal sagen werde, dass es nicht so einen großen Umfang haben sollte, da nicht der eigentliche Sinn der unterstützenden Kommunikation verloren gehen soll.
- reason: verdict=partial; Der Kern ist gedeckt: Dokumentationsfunktionen sollen geprüft werden, aber begrenzt und ohne Fokusverlust. Nur die Einordnung als später ist aus dem Transcript nicht ableitbar.
- ACTION: ____   REASON: ____

## RR::canon-rollen-admin-user  [review_required_claim / normal]
- proposition: Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- systemSuggestion: facet_repair proposition: Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.->es muss mindestens die rollen admin und user geben; zusätzlich wurde später auch eine bewohner-rolle aufgenommen. admins verwalten accounts und rechte, user können inhalte hinzufügen, aber nichts löschen.
- evidence: Speaker 1: Ja, beim Thema Login sollten wir verschiedene Account-Typen haben. User und Admin wäre mal ein Anfang. Der Admin hat dabei mehr Rechte und kann neue Accounts anlegen und diese verwalten, während die User-Accounts nur User-Rechte haben. | Speaker 1: Als Betreuer und Angehörige wären solche User und die können nur Inhalte hinzufügen, aber nichts löschen. | Speaker 2: Ein Account, der mit Adminrechten versehen ist, kann dann neue Accounts anlegen und Rechte verwalten. Es gibt also den Power-Nutzer-Admin und den Nutzer-User.
- reason: verdict=partial; Admin- und User-Rolle samt Rechten sind klar belegt, aber das Rollenmodell wurde später um einen Bewohner-Account erweitert.
- ACTION: ____   REASON: ____

## RR::canon-suche-kommunikation-und-beschreibungsmuster  [review_required_claim / normal]
- proposition: Auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisiertes Beschreibungsmuster für Kommunikationseinträge definiert werden, damit Suche und spätere Filterung funktionieren.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Vielleicht sollte diese Suchfunktion mittels einer Suchleiste oben im Bildschirm auch bei den Kommunikationsseiten möglich sein. | Speaker 1: Ich finde, so macht es am meisten Sinn. Machen Sie es genauso, wie Sie es gerade beschrieben haben. Natürlich mit der Suchfunktion. | Speaker 1: Dafür könnte man sich ein Muster überlegen. Wie man seine Beschreibung beim Hinzufügen des Bildes oder Ähnlichem formulieren soll, damit man später besser suchen kann.
- reason: verdict=partial; Die Aussage ist inhaltlich gut gedeckt: Suchfunktion auf Kommunikationsseiten und standardisiertes Beschreibungsmuster werden explizit besprochen. Der Umsetzungszeitpunkt für MVP ist aber nicht eindeutig belegt.
- ACTION: ____   REASON: ____

## RR::canon-bewohner-account-mit-beschraenkung  [review_required_claim / normal]
- proposition: Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil sehen darf und nur eingeschränkte Funktionen nutzen kann, insbesondere Zugriff auf About Me und gegebenenfalls das Hinzufügen eigener Bilder.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 1: Wieso sollte der Bewohner nicht auch einen Account bei der App haben? | Speaker 2: Sie haben recht. Das ist auch eine sehr gute Idee. Dieser Bewohner-Account sollte aber spezielle Rechte haben. | Speaker 2: Dieser Bewohner-Account sollte aber spezielle Rechte haben und auch nur sein eigenes Profil in der Profilübersicht, wo alle Profile gelistet sind, sehen.
- reason: verdict=partial; Der Bewohner-Account mit Beschränkung auf das eigene Profil ist klar gestützt. Die weitergehenden Funktionsdetails, insbesondere eigene Bilder hinzufügen, werden in der späteren Ausarbeitung eher als sinnvolle Idee diskutiert als verbindlich festgelegt.
- ACTION: ____   REASON: ____

## RR::canon-entwicklungsumgebung-vereinheitlichen  [review_required_claim / normal]
- proposition: Das Team soll nach Möglichkeit dieselbe Entwicklungsumgebung und dieselben Tool-Versionen verwenden; Android Studio ist dafür vorgesehen.
- systemSuggestions: facet_repair status: decided->open | facet_repair modality: must->must_consider
- evidence: Speaker 1: obwohl ich trotzdem vorschlagen würde, dass wir alle dieselbe Entwicklungsumgebung nutzen. | Speaker 2: Ja, ich bleibe dabei und wechsle notfalls auch auf Android Studio. | Speaker 1: Wichtig ist, dass wir dabei alle dieselbe Versionen installieren, damit wir später keine Probleme haben, wenn wir mit der Entwicklung beginnen.
- reason: verdict=partial; Die Nutzung gleicher Tool-Versionen ist klar gewollt. Bei der Entwicklungsumgebung selbst ist Android Studio zwar favorisiert, aber nicht eindeutig als verbindliche Teamfestlegung abgesichert.
- ACTION: ____   REASON: ____

## RR::canon-firebase-firestore-vorlaeufig  [review_required_claim / normal]
- proposition: Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgültig festgelegt.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 1: Für die Datenbank würde ich Firebase Firestore von Google empfehlen | Speaker 2: Aber bleiben wir erst mal bei Firebase und dann können wir noch weiter schauen, wie wir das umsetzen.
- reason: verdict=partial; Firebase Firestore ist als vorläufige Datenbanklösung gut belegt und noch nicht endgültig festgelegt. Der genaue Zeithorizont bis MVP bleibt jedoch offen.
- ACTION: ____   REASON: ____

## RR::canon-lokaler-cache-klären  [review_required_claim / normal]
- proposition: Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecacht werden sollen.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Speaker 2: Wir müssen klären, ob die Daten aus der Cloud dann lokal auf dem Handy gespeichert werden können oder wie das läuft. | Speaker 1: Ich habe mir das so vorgestellt, dass die App beim Aufrufen einer Seite die Sachen aus Firebase lädt und dann lokal speichert und nicht immer alles erneut aus Firebase lädt.
- reason: verdict=partial; Die Klärung zu lokalem Speichern/Caching ist ausdrücklich genannt; der Zeitpunkt innerhalb von MVP ist jedoch nicht belegt.
- ACTION: ____   REASON: ____

## RR::canon-android-11-startziel  [review_required_claim / normal]
- proposition: Für die initiale Entwicklung und Tests soll zunächst Android 11 als gemeinsame Zielversion verwendet werden.
- systemSuggestion: facet_repair modality: must->desired
- evidence: Speaker 2: Ja, dann würde ich empfehlen, dass wir erst mal alle für Android 11 entwickeln
- reason: verdict=partial; Android 11 wird als gemeinsames Startziel empfohlen und im Team positiv aufgenommen, aber nicht als harte Pflicht festgelegt.
- ACTION: ____   REASON: ____

## RR::canon-appbar-funktionen  [review_required_claim / normal]
- proposition: Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und Logout unterstützen.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 2: Mit der kann man dann zurücknavigieren oder sich ausloggen über ein Logout-Symbol. | Speaker 1: Links auf der Appbar sollte ein nach links zeigender Pfeil sein. | Speaker 1: In der Mitte der Appbar sollte der Name der aktuellen Seite stehen und rechts ein Einstellungssymbol.
- reason: verdict=overstated; Rücknavigation, Seitentitel, Einstellungen und Logout sind im Transcript konkret benannt; der Eintrag macht dies fälschlich offener als die Quelle.
- ACTION: ____   REASON: ____

## RR::canon-neue-profile-anlegen-pruefen  [review_required_claim / normal]
- proposition: Es soll erwogen werden, in der Profilübersicht das Anlegen neuer Profile per Plus-Symbol und Dialog für Bild, Name und Beschreibung zu ermöglichen.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 2: Und für das Hinzufügen neuer Profile brauchen wir ein klares Plus-Symbol unten auf dem Screen. Ein Dialogfeld sollte erscheinen, wenn man draufklickt, um ein neues Profil mit Bild, Name und Beschreibung anzulegen.
- reason: verdict=partial; Das Anlegen neuer Profile per Plus-Symbol und Dialog wird genannt, aber eine Einordnung als nur später möglich ist nicht belegt.
- ACTION: ____   REASON: ____

## RR::canon-profil-detail-mit-hauptbereichen  [review_required_claim / normal]
- proposition: Die Detailansicht eines Profils soll das Profilbild größer zeigen und die Hauptbereiche der App als interaktive Buttons anbieten.
- systemSuggestion: facet_repair status: open->decided
- evidence: Speaker 1: Wenn man ein Profil auswählt, sollte die Datenansicht, die Detailansicht unserer Profiles-Overview das gewählte Profilbild größer zeigen und die vier Hauptbereiche unserer App als interaktive Buttons darstellen.
- reason: verdict=partial; Die Detailansicht mit großem Profilbild und interaktiven Hauptbereichen ist im Design klar beschrieben; offen ist daran nichts erkennbar.
- ACTION: ____   REASON: ____

## RR::canon-hilfe-tutorial-pruefen  [review_required_claim / normal]
- proposition: Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour beim ersten Login und als später erneut aufrufbarer Hilfebereich.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Speaker 1: Vielleicht könnten wir sogar ein Tutorial oder eine Hilfe-Seite in der App einbauen | Speaker 1: Auf jeden Fall. Eine kurze Tour durch die App beim ersten Login könnte helfen, die Nutzer mit den Funktionen vertraut zu machen. Und ein Hilfebereich, den man immer wieder aufrufen kann, wäre auch sinnvoll.
- reason: verdict=partial; Die Idee einer Hilfe-Funktion bzw. eines Tutorials mit Tour beim ersten Login und erneut aufrufbarem Hilfebereich ist gedeckt; nur die zeitliche Einordnung als später ist zu stark.
- ACTION: ____   REASON: ____

## RR::canon-animationen-nicht-im-fokus  [review_required_claim / normal]
- proposition: Animationen sollen nicht im Fokus stehen; falls sie verwendet werden, dürfen sie nicht ablenkend sein.
- systemSuggestions: facet_repair status: decided->open | facet_repair modality: must_not->must_consider
- evidence: Speaker 2: Nee, ich denke nicht, dass wir das einbauen sollen. Aber wir sollten darauf achten, dass sie nicht zu ablenkend sind.
- reason: verdict=partial; Der Kern ist gedeckt: Animationen sollen nicht im Vordergrund stehen und nicht ablenken. Der Eintrag formuliert dies aber zu verbindlich als entschiedene harte Einschränkung.
- ACTION: ____   REASON: ____

## US::AU-0002  [unit_signal / unit]
- proposition: Die Unit belegt die geplante Einbindung von Mitarbeitern in die Anforderungsanalyse und das Einholen ihrer Vorstellungen zu einer möglichen Lösung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: anforderungsanalyse-mit-nutzern
- reason: Die Unit belegt die geplante Einbindung von Mitarbeitern in die Anforderungsanalyse und das Einholen ihrer Vorstellungen zu einer möglichen Lösung.
- ACTION: ____   REASON: ____

## US::AU-0003  [unit_signal / unit]
- proposition: Die Unit liefert zusätzliche Begründung für das Produktziel: Kommunikation ist im Alltag zentral und das Projekt wird als Weiterentwicklungsmöglichkeit gesehen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: problem-kommunikation-verbessern
- reason: Die Unit liefert zusätzliche Begründung für das Produktziel: Kommunikation ist im Alltag zentral und das Projekt wird als Weiterentwicklungsmöglichkeit gesehen.
- ACTION: ____   REASON: ____

## US::AU-0006  [unit_signal / unit]
- proposition: Die Aussage stützt den bereits erfassten Anspruch, Anforderungen gemeinsam zu erarbeiten, und leitet zur Frage nach vorhandener Dokumentation über.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: anforderungsanalyse-mit-nutzern | dokumentation-existiert
- reason: Die Aussage stützt den bereits erfassten Anspruch, Anforderungen gemeinsam zu erarbeiten, und leitet zur Frage nach vorhandener Dokumentation über.
- ACTION: ____   REASON: ____

## US::AU-0010  [unit_signal / unit]
- proposition: Die Unit konkretisiert die bereits festgehaltene Zielrichtung, dass Unterstützung vor allem dabei helfen soll, den Bewohner besser zu verstehen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: fokus-bewohner-besser-verstehen
- reason: Die Unit konkretisiert die bereits festgehaltene Zielrichtung, dass Unterstützung vor allem dabei helfen soll, den Bewohner besser zu verstehen.
- ACTION: ____   REASON: ____

## US::AU-0013  [unit_signal / unit]
- proposition: Die Unit bestätigt den Lösungsansatz einer App und nennt zusätzlich Kalender bzw. Medikamentenvergabe als mögliche Erweiterung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: app-profilwissen-kommunikation | kalender-und-medikamente | digitale-erweiterung-doku
- reason: Die Unit bestätigt den Lösungsansatz einer App und nennt zusätzlich Kalender bzw. Medikamentenvergabe als mögliche Erweiterung.
- ACTION: ____   REASON: ____

## US::AU-0017  [unit_signal / unit]
- proposition: Die Beobachtung, dass implizites Kommunikationswissen bei erfahrenen Personen vorhanden ist und Akten relevant sein könnten, stützt bestehende Claims zu Vor-Ort-Beobachtung und Dokumentation.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: beobachtung-vor-ort | dokumentation-existiert
- reason: Die Beobachtung, dass implizites Kommunikationswissen bei erfahrenen Personen vorhanden ist und Akten relevant sein könnten, stützt bestehende Claims zu Vor-Ort-Beobachtung und Dokumentation.
- ACTION: ____   REASON: ____

## US::AU-0019  [unit_signal / unit]
- proposition: Die Aussage bewertet die bestehende Dokumentation als nachträglich wenig hilfreich und passt damit als zusätzliche Evidenz zur eingeschränkten Nutzbarkeit vorhandener Akten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: akten-schwer-nutzbar
- reason: Die Aussage bewertet die bestehende Dokumentation als nachträglich wenig hilfreich und passt damit als zusätzliche Evidenz zur eingeschränkten Nutzbarkeit vorhandener Akten.
- ACTION: ____   REASON: ____

## US::AU-0021  [unit_signal / unit]
- proposition: Die Frage nach dem Umgang mit neuen Mitarbeitern liefert Kontext für das Problem, dass Kommunikationswissen an neue Personen übergeben werden muss.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: problem-kommunikation-verbessern | app-profilwissen-kommunikation
- reason: Die Frage nach dem Umgang mit neuen Mitarbeitern liefert Kontext für das Problem, dass Kommunikationswissen an neue Personen übergeben werden muss.
- ACTION: ____   REASON: ____

## US::AU-0022  [unit_signal / unit]
- proposition: Die Unit beschreibt den aktuellen Einarbeitungsprozess über Dokumentation, Begleitung und persönliche Erklärung und ist damit starke Evidenz für bestehende Ist-Situations-Claims.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: dokumentation-existiert | app-profilwissen-kommunikation
- reason: Die Unit beschreibt den aktuellen Einarbeitungsprozess über Dokumentation, Begleitung und persönliche Erklärung und ist damit starke Evidenz für bestehende Ist-Situations-Claims.
- ACTION: ____   REASON: ____

## US::AU-0023  [unit_signal / unit]
- proposition: Die Unit formuliert explizit den Nutzen einer App zum Festhalten und schnellen Abrufen von Kommunikationswissen für neue Mitarbeiter.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: app-profilwissen-kommunikation | problem-kommunikation-verbessern
- reason: Die Unit formuliert explizit den Nutzen einer App zum Festhalten und schnellen Abrufen von Kommunikationswissen für neue Mitarbeiter.
- ACTION: ____   REASON: ____

## US::AU-0034  [unit_signal / unit]
- proposition: Die Frage nach dem Zugriff stützt den bereits vorhandenen Themenblock zu Benutzerkreis und Zugriffsrechten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: angehoerige-zugriff | rollen-admin-user | zugriff-pro-einrichtung
- reason: Die Frage nach dem Zugriff stützt den bereits vorhandenen Themenblock zu Benutzerkreis und Zugriffsrechten.
- ACTION: ____   REASON: ____

## US::AU-0035  [unit_signal / unit]
- proposition: Die Aussage zeigt, dass initial nur Mitarbeiter als Nutzer vorgesehen waren, bevor weitere Nutzergruppen diskutiert wurden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: angehoerige-zugriff
- reason: Die Aussage zeigt, dass initial nur Mitarbeiter als Nutzer vorgesehen waren, bevor weitere Nutzergruppen diskutiert wurden.
- ACTION: ____   REASON: ____

## US::AU-0042  [unit_signal / unit]
- proposition: Die Unit markiert den Einstieg in die gemeinsame Ausarbeitung der App-Struktur inklusive Login-Screen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-nur-zugewiesene-accounts
- reason: Die Unit markiert den Einstieg in die gemeinsame Ausarbeitung der App-Struktur inklusive Login-Screen.
- ACTION: ____   REASON: ____

## US::AU-0046  [unit_signal / unit]
- proposition: Die Unit bestätigt die vorgesehene Profilstruktur mit About Me, Kommunikation, Video und Kalender als Navigationsbereiche.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about-me-seite | kommunikationsseite-verbal-nonverbal | kalender-und-medikamente
- reason: Die Unit bestätigt die vorgesehene Profilstruktur mit About Me, Kommunikation, Video und Kalender als Navigationsbereiche.
- ACTION: ____   REASON: ____

## US::AU-0047  [unit_signal / unit]
- proposition: Die Unit liefert konkrete Details zur About-Me-Seite und zum Hinzufügen von Bildern, die bereits als Anforderungen erfasst sind.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: about-me-seite | dynamische-erweiterung-bilder
- reason: Die Unit liefert konkrete Details zur About-Me-Seite und zum Hinzufügen von Bildern, die bereits als Anforderungen erfasst sind.
- ACTION: ____   REASON: ____

## US::AU-0048  [unit_signal / unit]
- proposition: Die Unit ergänzt Bedienungsdetails zum Hinzufügen von Bildern in About Me und leitet zur Kommunikationsstruktur über.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: dynamische-erweiterung-bilder | kommunikationsseite-verbal-nonverbal
- reason: Die Unit ergänzt Bedienungsdetails zum Hinzufügen von Bildern in About Me und leitet zur Kommunikationsstruktur über.
- ACTION: ____   REASON: ____

## US::AU-0049  [unit_signal / unit]
- proposition: Die Unit deckt die Unterteilung in verbal/nonverbal sowie die dynamische Erweiterbarkeit von Kommunikationseinträgen mit Texten und Bildern ab.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: kommunikationsseite-verbal-nonverbal | kommunikation-dynamisch-erweiterbar | nicht-nur-text-darstellungen
- reason: Die Unit deckt die Unterteilung in verbal/nonverbal sowie die dynamische Erweiterbarkeit von Kommunikationseinträgen mit Texten und Bildern ab.
- ACTION: ____   REASON: ____

## US::AU-0050  [unit_signal / unit]
- proposition: Die Nachfrage zum Kalender ist zusätzliche Evidenz dafür, dass eine Kalenderfunktion als Bestandteil der App mitgedacht wird.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: kalender-und-medikamente
- reason: Die Nachfrage zum Kalender ist zusätzliche Evidenz dafür, dass eine Kalenderfunktion als Bestandteil der App mitgedacht wird.
- ACTION: ____   REASON: ____

## US::AU-0052  [unit_signal / unit]
- proposition: Die Nachfrage zur Videoseite ist zusätzliche Evidenz für die geplante Video-Unterstützung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: videos-fuer-kommunikationssituationen
- reason: Die Nachfrage zur Videoseite ist zusätzliche Evidenz für die geplante Video-Unterstützung.
- ACTION: ____   REASON: ____

## US::AU-0053  [unit_signal / unit]
- proposition: Die Unit konkretisiert die Videoseite mit Plus-Button, Beschreibungen und Sortierung nach neuesten Einträgen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: videos-fuer-kommunikationssituationen
- reason: Die Unit konkretisiert die Videoseite mit Plus-Button, Beschreibungen und Sortierung nach neuesten Einträgen.
- ACTION: ____   REASON: ____

## US::AU-0059  [unit_signal / unit]
- proposition: Die Unit bringt ausdrücklich die Datenschutzproblematik sensibler Daten ein und stützt damit die offenen Klärungsbedarfe.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: datenschutz-bilder-angehoerige
- reason: Die Unit bringt ausdrücklich die Datenschutzproblematik sensibler Daten ein und stützt damit die offenen Klärungsbedarfe.
- ACTION: ____   REASON: ____

## US::AU-0061  [unit_signal / unit]
- proposition: Die Unit formuliert die offene Frage nach dem Zugriff zwischen Einrichtungen und stützt damit bestehende Kandidaten zu einrichtungsbezogenen Rechten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: zugriff-pro-einrichtung | umsetzung-einrichtungsrechte-offen
- reason: Die Unit formuliert die offene Frage nach dem Zugriff zwischen Einrichtungen und stützt damit bestehende Kandidaten zu einrichtungsbezogenen Rechten.
- ACTION: ____   REASON: ____

## US::AU-0065  [unit_signal / unit]
- proposition: Die Unit ist zusätzliche Evidenz für die Anforderung, iPhone und Android zu unterstützen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-platform-ios-android
- reason: Die Unit ist zusätzliche Evidenz für die Anforderung, iPhone und Android zu unterstützen.
- ACTION: ____   REASON: ____

## US::AU-0077  [unit_signal / unit]
- proposition: Die Unit beschreibt den zentralen Nutzen der App für neue Mitarbeiter und Erstkontakte sowie die dynamische Erweiterung und mögliche Vorabeingabe durch Angehörige.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: app-profilwissen-kommunikation | kommunikation-dynamisch-erweiterbar | angehoerige-zugriff
- reason: Die Unit beschreibt den zentralen Nutzen der App für neue Mitarbeiter und Erstkontakte sowie die dynamische Erweiterung und mögliche Vorabeingabe durch Angehörige.
- ACTION: ____   REASON: ____

## US::AU-0081  [unit_signal / unit]
- proposition: Die Unit ist direkte Evidenz für die Diskussion eines speziellen Bewohner-Accounts als neues Feature.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: bewohner-account | bewohner-account-beschraenkt
- reason: Die Unit ist direkte Evidenz für die Diskussion eines speziellen Bewohner-Accounts als neues Feature.
- ACTION: ____   REASON: ____

## US::AU-0084  [unit_signal / unit]
- proposition: Die Unit beschreibt den Nutzen einer Suchleiste für viele Profile und passt direkt zum vorhandenen Such-Claim für die Profilübersicht.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: suche-profile
- reason: Die Unit beschreibt den Nutzen einer Suchleiste für viele Profile und passt direkt zum vorhandenen Such-Claim für die Profilübersicht.
- ACTION: ____   REASON: ____

## US::AU-0087  [unit_signal / unit]
- proposition: Die Unit stellt die Alternative Selbstregistrierung versus Admin-Verwaltung zur Diskussion und stützt damit den bereits festgehaltenen Verzicht auf Selbstregistrierung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-nur-zugewiesene-accounts | admin-screen
- reason: Die Unit stellt die Alternative Selbstregistrierung versus Admin-Verwaltung zur Diskussion und stützt damit den bereits festgehaltenen Verzicht auf Selbstregistrierung.
- ACTION: ____   REASON: ____

## US::AU-0089  [unit_signal / unit]
- proposition: Die Unit begründet die kontrollierte Account-Verwaltung mit dem sensiblen Datenkontext.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-nur-zugewiesene-accounts | rollen-admin-user
- reason: Die Unit begründet die kontrollierte Account-Verwaltung mit dem sensiblen Datenkontext.
- ACTION: ____   REASON: ____

## US::AU-0092  [unit_signal / unit]
- proposition: Die Unit leitet die Technologieentscheidung aus der Cross-Plattform-Anforderung für iOS und Android her.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cross-platform-ios-android | flutter-dart
- reason: Die Unit leitet die Technologieentscheidung aus der Cross-Plattform-Anforderung für iOS und Android her.
- ACTION: ____   REASON: ____

## US::AU-0094  [unit_signal / unit]
- proposition: Die Unit ergänzt den bereits entschiedenen Einsatz von Flutter/Dart um Teamunterstützung und Lernhinweise, ist aber kein eigenständiger Produkt- oder Prozessclaim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: flutter-dart
- reason: Die Unit ergänzt den bereits entschiedenen Einsatz von Flutter/Dart um Teamunterstützung und Lernhinweise, ist aber kein eigenständiger Produkt- oder Prozessclaim.
- ACTION: ____   REASON: ____

## US::AU-0096  [unit_signal / unit]
- proposition: Visual Studio Code wurde als alternative Entwicklungsumgebung empfohlen, obwohl Android Studio teamweit festgelegt wurde.
- systemSuggestion: compare_classification needs_human
- reason: Die Empfehlung von Visual Studio Code steht in Spannung zur bestehenden Festlegung auf Android Studio als teamweite Entwicklungsumgebung. Unklar ist, ob dies nur eine persönliche Präferenz oder ein relevanter Gegenclaim ist.
- ACTION: ____   REASON: ____

## US::AU-0097  [unit_signal / unit]
- proposition: Die Unit begründet die Wahl von Android Studio mit vorhandener Erfahrung und Ähnlichkeit zu IntelliJ und stützt damit den bestehenden Kandidaten zur teamweiten Entwicklungsumgebung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: android-studio-teamweit
- reason: Die Unit begründet die Wahl von Android Studio mit vorhandener Erfahrung und Ähnlichkeit zu IntelliJ und stützt damit den bestehenden Kandidaten zur teamweiten Entwicklungsumgebung.
- ACTION: ____   REASON: ____

## US::AU-0098  [unit_signal / unit]
- proposition: Die Aussage liefert eine weitere Begründung für Android Studio über die Präferenz für ein IntelliJ-ähnliches System und ergänzt damit den bestehenden Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: android-studio-teamweit
- reason: Die Aussage liefert eine weitere Begründung für Android Studio über die Präferenz für ein IntelliJ-ähnliches System und ergänzt damit den bestehenden Claim.
- ACTION: ____   REASON: ____

## US::AU-0108  [unit_signal / unit]
- proposition: Die Nachfrage zur Hardware leitet die Klärung der Entwicklungs- und Testumgebung ein und ist als Kontext/Evidence für die spätere Festlegung einer gemeinsamen Zielumgebung relevant.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: android-11-entwicklungsziel
- reason: Die Nachfrage zur Hardware leitet die Klärung der Entwicklungs- und Testumgebung ein und ist als Kontext/Evidence für die spätere Festlegung einer gemeinsamen Zielumgebung relevant.
- ACTION: ____   REASON: ____

## US::AU-0109  [unit_signal / unit]
- proposition: Die Präzisierung, dass es um die Betriebssystemversion des Emulator-Handys geht, ist Kontext zur bestehenden Festlegung auf Android 11 als gemeinsame Zielversion.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: android-11-entwicklungsziel
- reason: Die Präzisierung, dass es um die Betriebssystemversion des Emulator-Handys geht, ist Kontext zur bestehenden Festlegung auf Android 11 als gemeinsame Zielversion.
- ACTION: ____   REASON: ____

## US::AU-0113  [unit_signal / unit]
- proposition: Alle Teammitglieder sollen die vereinbarte Entwicklungsumgebung zeitnah installieren und bis zum nächsten Treffen prüfen, ob alles technisch funktioniert; konkrete Coding-Regeln werden in einem folgenden Treffen festgelegt.
- systemSuggestion: compare_classification missing_claim
- reason: Die Unit enthält eine eigenständige Prozessvereinbarung: alle sollen die Entwicklungsumgebung installieren und bis zum nächsten Treffen prüfen, ob alles funktioniert; außerdem werden Coding-Regeln erst im nächsten Treffen geklärt.
- ACTION: ____   REASON: ____

## US::AU-0117  [unit_signal / unit]
- proposition: Der Login-Screen soll ein zentriertes, gut sichtbares Logo im oberen Drittel enthalten; ein passendes Kommunikations-Logo ist noch zu erstellen.
- systemSuggestion: compare_classification missing_claim
- reason: Die Unit enthält eigenständige UI-Anforderungen für den Login-Screen und das Branding, die im Ledger noch nicht konkret erfasst sind.
- ACTION: ____   REASON: ____

## US::AU-0118  [unit_signal / unit]
- proposition: Die Platzierung von E-Mail- und Passwort-Feld unter dem Logo ergänzt die bereits vorhandene Login-Screen-Festlegung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-nur-zugewiesene-accounts
- reason: Die Platzierung von E-Mail- und Passwort-Feld unter dem Logo ergänzt die bereits vorhandene Login-Screen-Festlegung.
- ACTION: ____   REASON: ____

## US::AU-0119  [unit_signal / unit]
- proposition: Die Aussage präzisiert den Login-Screen mit Login-Button und ohne Registrierung und stützt damit den bestehenden Kandidaten direkt.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: login-nur-zugewiesene-accounts
- reason: Die Aussage präzisiert den Login-Screen mit Login-Button und ohne Registrierung und stützt damit den bestehenden Kandidaten direkt.
- ACTION: ____   REASON: ____

## US::AU-0121  [unit_signal / unit]
- proposition: Die Unit präzisiert bestehende Kandidaten zur konsistenten Appbar und zur Suchleiste auf der Profilübersicht durch Positionierung und Screen-Titel.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: konsistente-appbar | suche-profile
- reason: Die Unit präzisiert bestehende Kandidaten zur konsistenten Appbar und zur Suchleiste auf der Profilübersicht durch Positionierung und Screen-Titel.
- ACTION: ____   REASON: ____

## US::AU-0127  [unit_signal / unit]
- proposition: Die Unit ergänzt die Kommunikationsansicht um einen Plus-Button für Videos und Beschreibungen und passt zu bereits vorhandenen Claims zu Videos und dynamischer Erweiterbarkeit.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: videos-fuer-kommunikationssituationen | kommunikation-dynamisch-erweiterbar | video-in-kommunikation-integrieren
- reason: Die Unit ergänzt die Kommunikationsansicht um einen Plus-Button für Videos und Beschreibungen und passt zu bereits vorhandenen Claims zu Videos und dynamischer Erweiterbarkeit.
- ACTION: ____   REASON: ____

## US::AU-0128  [unit_signal / unit]
- proposition: Die Aussage konkretisiert die visuelle Gestaltung und Informationsdichte der bestehenden No-Go-Seite, ohne einen neuen eigenständigen Claim zu bilden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no-go-seite | no-go-dynamisch-erweiterbar
- reason: Die Aussage konkretisiert die visuelle Gestaltung und Informationsdichte der bestehenden No-Go-Seite, ohne einen neuen eigenständigen Claim zu bilden.
- ACTION: ____   REASON: ____

## US::AU-0129  [unit_signal / unit]
- proposition: Die Unit ergänzt den bestehenden Kalender-Claim um Monatsansicht, Kennzeichnung von Medikamentenerinnerungen und Detailansicht per Tap.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: kalender-und-medikamente
- reason: Die Unit ergänzt den bestehenden Kalender-Claim um Monatsansicht, Kennzeichnung von Medikamentenerinnerungen und Detailansicht per Tap.
- ACTION: ____   REASON: ____

## US::AU-0130  [unit_signal / unit]
- proposition: Die Aussage konkretisiert das bestehende Rollen- und Rechtesystem sowie Einstellungen nach Nutzerrolle, insbesondere Admin-Funktionen und allgemeine Profil-/Spracheinstellungen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: rollen-admin-user | admin-screen
- reason: Die Aussage konkretisiert das bestehende Rollen- und Rechtesystem sowie Einstellungen nach Nutzerrolle, insbesondere Admin-Funktionen und allgemeine Profil-/Spracheinstellungen.
- ACTION: ____   REASON: ____

## US::AU-0131  [unit_signal / unit]
- proposition: Die leicht zugänglichen Datenschutzeinstellungen und das schnell erreichbare Ausloggen ergänzen bestehende Kandidaten zu Einstellungen/Appbar; die Hilfe-Funktion ist bereits separat im Ledger enthalten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: appbar-navigation-settings-logout | hilfe-oder-tutorial
- reason: Die leicht zugänglichen Datenschutzeinstellungen und das schnell erreichbare Ausloggen ergänzen bestehende Kandidaten zu Einstellungen/Appbar; die Hilfe-Funktion ist bereits separat im Ledger enthalten.
- ACTION: ____   REASON: ____

## US::AU-0136  [unit_signal / unit]
- proposition: Die Zusammenfassung bestätigt bestehende Leitlinien zu intuitivem Design, Hilfe-Funktionen und Benutzerfreundlichkeit und kündigt den Start der Prototypen an, ohne neue fachliche Anforderungen einzuführen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: hilfe-oder-tutorial | barrierefreiheit
- reason: Die Zusammenfassung bestätigt bestehende Leitlinien zu intuitivem Design, Hilfe-Funktionen und Benutzerfreundlichkeit und kündigt den Start der Prototypen an, ohne neue fachliche Anforderungen einzuführen.
- ACTION: ____   REASON: ____

