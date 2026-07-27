# Requirements

## Funktionale Anforderungen

- Die Lösung soll als digitale Anwendung die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern bzw. unterstützen; ob dies im MVP oder später umgesetzt wird, ist noch offen. [canonical-product-goal-digital-communication-support]

- Die App soll primär dabei helfen, Bewohner besser zu verstehen, indem Wissen über ihre individuelle Art zu kommunizieren als nachschlagbare Referenz bereitgestellt wird, einschließlich Informationen aus der Akte bzw. allgemeinem Wissen über die Kommunikationsweise einer beeinträchtigten Person. [canonical-support-understanding-residents-via-reference-app]

- Die App soll einen Login-Screen mit Logo, E-Mail-Feld, Passwort-Feld und Login-Button bereitstellen. Eine Registrierungsfunktion soll dort nicht angeboten werden. [canonical-login-screen-elements]

- Die App soll einen Login haben; eine Selbstregistrierung darf nicht angeboten werden, und Accounts dürfen ausschließlich durch Admins angelegt und verwaltet werden. [canonical-login-and-admin-managed-accounts]

- Es soll mindestens die Rollen Admin und User geben. User dürfen Inhalte hinzufügen, aber weder Accounts erstellen noch hinzugefügte Daten aus der App löschen; Admins können Accounts und Rechte verwalten. [canonical-role-rights-admin-user]

- Nach dem Login soll eine Profilübersicht mit einer Liste aller Bewohnerprofile angezeigt werden. Jedes Profil soll anklickbar sein und zu einer weiteren Detailseite führen. [canonical-profile-overview-after-login]

- Auf der Profilübersicht soll eine Suchleiste vorhanden sein, um Profile gezielt zu finden. [canonical-search-in-profile-overview]

- Die App soll je Bewohner eine About-Me-Seite bereitstellen, die für den ersten Eindruck Basisinformationen wie Name, Alter, Hobbys, Bilder und Beschreibungen enthält. [canonical-about-me-screen-and-timeline]

- Auf der About-Me-Seite soll darunter eine Foto-Timeline dargestellt werden. Neue Einträge sollen über einen Plus-Button am unteren Bildschirmrand hinzugefügt werden können, und die neuesten Fotos sollen oben angezeigt werden. [canonical-about-me-screen-and-timeline]

- Die App soll je Bewohner eine Kommunikationsseite enthalten, die in verbale und nonverbale Kommunikation unterteilt ist. [canonical-communication-screen-with-integrated-videos]

- Videos zu konkreten Kommunikationssituationen sollen mit Beschreibungen in die Kommunikationsseiten integriert werden und nicht als eigener separater Screen umgesetzt werden. [canonical-communication-screen-with-integrated-videos]

- Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein, um Kommunikationsinhalte gezielt zu finden; dafür muss noch ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen entwickelt und geklärt werden. [canonical-search-in-communication-content-with-description-pattern]

- Die App soll je Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht und unbedingt vermieden werden muss. [canonical-no-go-screen]

- Inhalte wie Bilder, Kommunikationsweisen und neue Erfahrungen müssen in der App fortlaufend ergänzt werden können, unter anderem über Plus-Buttons zur Erweiterung. [canonical-dynamic-growth-of-content]

- Angehörige sollen Zugriff auf die App erhalten und Inhalte hinzufügen können. Dafür müssen verschiedene Accounts für Mitarbeiter und Angehörige mit unterschiedlichen Rechten vorgesehen werden. [canonical-family-access-and-contribution-with-roles]

## Nicht-funktionale Anforderungen

- Kommunikationsinformationen sollen im MVP nicht nur als Text, sondern auch in anderen visuellen Darstellungen abgebildet werden. [canonical-multi-modal-representation]

- Bei der Gestaltung der App sollen Barrierefreiheitsaspekte wie große Schrift, ausreichender Kontrast und die Vermeidung von zu vielen Farben berücksichtigt werden; ob dies im MVP oder später konkretisiert wird, ist noch offen. [canonical-accessibility-considerations]

- Die App soll plattformübergreifend sowohl auf iPhone/iOS als auch auf Android laufen; dies soll für den MVP berücksichtigt werden. [canonical-cross-platform-ios-android]

## Navigations- und UI-Anforderungen

- Nach dem Login soll auf jeder Seite oben eine konsistente Appbar vorhanden sein, mit einem Zurück-Pfeil links, dem Namen der aktuellen Seite in der Mitte und einem Einstellungssymbol rechts. [canonical-global-appbar]

## Constraints

- Die Lösung darf kein System sein, das als Schnittstelle zwischen Bewohner und beispielsweise Betreuer dient und zwischen allgemeiner Sprache und der individuellen Sprache einzelner Bewohner hin- und herübersetzt. [canonical-no-direct-language-translator]

- Mitarbeiter dürfen nur die Profile der Bewohner sehen, die zu ihrer eigenen Einrichtung bzw. ihrem eigenen Haus gehören; ein einrichtungsübergreifender Zugriff ist nicht zulässig. [canonical-restrict-visibility-by-facility]

- Eine vollständige Übernahme der gesamten Dokumentation in die App ist nur später zu prüfen und darf den Fokus auf unterstützende Kommunikation nicht verdrängen. [canonical-full-documentation-out-of-core-scope]

## Offene Klärungen und Evaluationspunkte

- Wie und was die App im Detail können soll, muss in den nächsten Terminen weiter erarbeitet werden. [canonical-feature-set-to-be-elaborated]

- Vor der Nutzung von Bildern in der App müssen Datenschutzfragen und Einwilligungen mit Angehörigen geklärt werden, insbesondere ob Bilder gemacht oder bereitgestellt und später testweise in die App eingefügt werden dürfen. [canonical-clarify-image-consent]

- Es soll evaluiert werden, ob die App auch auf Tablets nutzbar sein soll bzw. umgesetzt werden kann; der Zeithorizont ist noch unklar. [canonical-tablet-support-evaluate]

- Ein Bewohner-Account soll später zusätzlich geprüft werden. Dieser soll nur das eigene Profil bzw. die eigenen Daten sehen dürfen; zu klären bleibt, ob er den About-Me-Bereich nutzen und dort eigene Bilder hinzufügen darf, wobei die nutzbaren Funktionen eng begrenzt sein müssen. [canonical-resident-account-own-profile-and-limited-edit]

## Kontext, Risiken und Annahmen

- Zu den Bewohnern existieren bereits klassische Akten, in denen Informationen dokumentiert sind. [canonical-existing-paper-records]

- Der derzeitige Einarbeitungsprozess für neue Mitarbeiter stützt sich auf Dokumentationen und begleitete Einarbeitung, bei der erfahrene Mitarbeiter Kommunikationssituationen erklären, bis Verständnis und Vertrauen aufgebaut sind. [canonical-new-staff-onboarding-current-process]

- Erfahrungen und neues Wissen werden zwar schriftlich dokumentiert, sind im Arbeitsalltag wegen ihres Umfangs und wegen schlechter Auffindbarkeit jedoch nicht immer praktisch nutzbar. [canonical-documentation-hard-to-use]

- Der Zugriff auf bestehende Bewohnerakten kann aufgrund von Datenschutz schwierig sein und muss vor einer Nutzung geklärt werden. [canonical-record-access-privacy-risk]

- Für die App muss der Datenschutz im Umgang mit sensiblen Bewohnerdaten grundsätzlich geklärt werden. [ADJ-GAP-AU-0059]

- Für die App-Entwicklung sollen Flutter und Dart verwendet werden. [canonical-tech-stack-flutter-dart]

- Als Datenbank soll zunächst Firebase Firestore verwendet werden; zu klären bleibt, ob und wie Daten aus der Cloud lokal auf dem Handy gespeichert werden können. [canonical-firebase-with-local-cache-open]

- In der Detailansicht eines ausgewählten Profils soll das Profilbild vergrößert angezeigt werden, und die vier Hauptbereiche der App sollen dort als interaktive Buttons erreichbar sein. [ADJ-GAP-AU-0124]

- Die App soll leicht zugängliche Datenschutzeinstellungen bieten, und eine Logout-Funktion soll jederzeit schnell erreichbar sein. [ADJ-GAP-AU-0131]

## Optionale bzw. gewünschte Erweiterungen

- Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung eine Popup-Benachrichtigung erhalten; der Einsatzzeitpunkt ist noch unklar. [canonical-popup-on-about-me-updates]

- Die Profilübersicht soll Profile als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen; außerdem soll das Anlegen neuer Profile über ein Plus-Symbol mit Dialog zum Anlegen eines neuen Profils mit Bild, Name und Beschreibung ermöglicht werden. Der Zeithorizont ist noch unklar. [canonical-profile-overview-content-and-add]

- Die App soll später möglicherweise einen Kalenderbereich mit klarer Monatsansicht enthalten, in dem Termine und gegebenenfalls Medikamentenhinweise pro Tag, etwa über kleine Icons oder Tags, dargestellt werden können. [canonical-calendar-with-appointments-and-medication]

- Es soll einen Einstellungsbereich geben, der je nach Nutzerrolle unterschiedliche Funktionen anbietet, insbesondere Account- und Rechteverwaltung für Admins; dies ist derzeit nur gewünscht und zeitlich noch unklar. [canonical-settings-role-dependent]

- Die App soll später möglicherweise eine Hilfe-Funktion bzw. ein Tutorial bieten, etwa eine kurze Einführung beim ersten Login und einen später wieder aufrufbaren Hilfebereich. [canonical-tutorial-and-help]

- Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer sollen geprüft werden. [canonical-alternative-input-methods]

- Ablenkende Animationen sollen nicht eingebaut werden; der genaue Einsatzzeitpunkt dieser Vorgabe ist noch unklar. [canonical-no-distracting-animations]