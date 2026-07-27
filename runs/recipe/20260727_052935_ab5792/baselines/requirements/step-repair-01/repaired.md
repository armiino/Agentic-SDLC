# Requirements

## Funktionale Anforderungen

- Die Lösung soll als digitales Computerprogramm bzw. als App die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern und fördern. [canonical-productvision-kommunikationsfoerderung]

- Die Lösung soll vorrangig dazu beitragen, Bewohner besser zu verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen. [canonical-decision-bewohner-besser-verstehen]

- Die App soll kein System sein, das als Schnittstelle natürliche Sprache bidirektional zwischen Bewohner und anderen Personen, z. B. Betreuern, in die individuelle Sprache einzelner Bewohner übersetzt. [canonical-constraint-kein-universal-uebersetzer]

- Die App soll nur für den internen Gebrauch der Einrichtung vorgesehen sein; beliebige Personen sollen sich nicht anmelden können. Mitarbeitende sollen ausschließlich die Profile der Bewohner ihrer eigenen Einrichtung sehen und nicht einrichtungsübergreifend. [canonical-constraint-interner-gebrauch-und-einrichtungsgrenzen]

- Die App soll einen Login-Screen mit Logo, E-Mail-Feld, Passwort-Feld und Login-Button bereitstellen; eine Registrierungsoption soll dort nicht vorhanden sein. [canonical-requirement-login-screen]

- Die App soll einen Login mit unterschiedlichen Account-Typen besitzen. Admins sollen weitere Accounts erstellen sowie Rechte verwalten können; User sollen Inhalte hinzufügen können. Es soll keine Selbstregistrierung geben, sodass neue Accounts nur von Admins erstellt werden. [canonical-decision-login-rollen-admin-user-keine-selbstregistrierung]

- Nach dem Login soll auf jeder Seite eine Appbar vorhanden sein, die mindestens Rücknavigation und Logout ermöglicht. Eine Einstellungsfunktion in der Appbar ist vorgesehen, aber noch nicht ausgearbeitet und soll weiter geklärt werden. [canonical-requirement-globale-appbar]

- Nach dem Login soll eine Profilübersicht mit einer Liste aller Profile angezeigt werden, von der aus in einzelne Profile navigiert werden kann. [canonical-decision-profiluebersicht-startseite]

- Auf der Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste vorhanden sein. Darunter sollen Profile als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung angezeigt werden, damit sie gezielt gefunden werden können. [canonical-requirement-profiluebersicht-mit-suche]

- Für die Profilübersicht soll eine Funktion zum Anlegen neuer Profile mit Bild, Name und Beschreibung vorgesehen werden, z. B. über ein klares Plus-Symbol unten auf dem Screen und ein erscheinendes Dialogfeld. Wer diese Funktion nutzen darf, ist noch nicht abschließend geklärt und soll weiter betrachtet werden. [canonical-open-requirement-profilanlage]

- Die App soll pro Profil mindestens die Bereiche About Me, Kommunikation, Kalender und No-Go bereitstellen. [canonical-requirement-hauptbereiche-pro-profil]

- Der About-Me-Bereich soll ein Informationsfeld mit persönlichen Basisdaten wie Name, Alter und Hobbys enthalten. Darunter soll eine erweiterbare Bildliste bzw. Foto-Timeline mit Beschreibungen angezeigt werden; neue Bilder sollen hinzugefügt werden können, und das aktuellste Bild soll immer ganz oben erscheinen. [canonical-requirement-about-me]

- Kommunikationswissen in der App soll dynamisch wachsen; neue Erfahrungen müssen hinzugefügt werden können. Die konkrete UI-Umsetzung, etwa über einen Plus-Button, ist noch weiter auszuarbeiten. [canonical-requirement-kommunikationswissen-dynamisch-erweiterbar]

- Es ist gewünscht, dass die App Wissen über die Kommunikationsweise einzelner Bewohner festhält, damit andere Personen bei Verständnisschwierigkeiten nachsehen können, was gemeint sein könnte. Die Detailausgestaltung bleibt offen. [canonical-requirement-kommunikationswissen-profilbezogen]

- Der Kommunikationsbereich soll in verbale und nonverbale Kommunikation unterteilt sein. Er soll erweiterbare Einträge zulassen, in denen Texte oder Bilder hinzugefügt werden können, und er soll eine Suchfunktion unterstützen, beispielsweise über eine Suchleiste oben im Bildschirm. Zudem soll geklärt werden, ob standardisierte Beschreibungs- bzw. Eingabemuster vorgegeben werden, etwa mit vorgeschriebenen Feldern wie zuerst Körperteil und dann Interpretation. [canonical-requirement-kommunikationsbereich-strukturiert]

- Die App soll Videos konkreter Situationen mit Beschreibung zur Wiedererkennung von Kommunikations- oder Verhaltensweisen bereitstellen. Diese Videofunktionalitäten sollen nicht als eigener Screen, sondern in die Kommunikationsseiten integriert werden. [canonical-requirement-video-in-kommunikation]

- Ein Kalenderbereich soll Termine abbilden. Ob und wie zusätzlich Medikamentengaben enthalten sein sollen, muss wegen der vertraulicheren Handhabung von Medikamenten noch geklärt werden. [canonical-open-requirement-kalender-medikation]

- Es ist gewünscht, dass die App eine No-Go-Seite enthält, auf der kritische Dinge festgehalten werden, die in Gegenwart eines Bewohners vermieden werden müssen. [canonical-requirement-no-go-seite]

- Es ist optional und noch offen, einen Settings-Bereich mit rollenabhängiger Sicht vorzusehen, in dem Admins Nutzeraccounts und Rechte verwalten können und alle Nutzer eigene Profileinstellungen wie Sprache anpassen können. [canonical-open-requirement-settings-rollenabhaengig]

- Es soll erwogen werden, einen speziellen Bewohner-Account bereitzustellen, der nur das eigene Profil in der Profilübersicht sehen darf und einen stark begrenzten Funktionsumfang besitzt, insbesondere Zugriff auf den About-Me-Bereich und das Hinzufügen eigener Bilder. Die genaue Ausgestaltung soll noch festgelegt werden, um Fehlbedienungen zu minimieren. [canonical-open-requirement-bewohner-account]

- Es ist gewünscht, dass auch Angehörige Zugriff auf die App erhalten und eigene Informationen ergänzen können, weil sie die Personen oft besonders gut kennen. Ob und wie dies umgesetzt wird, bleibt offen. [canonical-requirement-angehoerige-zugriff-und-mitwirkung]

## Nicht-funktionale Anforderungen

- Bei der Gestaltung soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und zurückhaltenden Farbeinsatz bzw. die Vermeidung von zu vielen Farben. [canonical-nfr-barrierefreiheit]

- Kommunikationsinhalte sollen im MVP nicht nur textuell, sondern auch in anderen visuellen Darstellungen abgebildet werden. [canonical-nfr-multimodale-darstellung]

- Ablenkende Animationen sollen im MVP vermieden bzw. zumindest nicht zu ablenkend gestaltet werden; zusätzliche Animationen sind derzeit nicht vorgesehen und nur optional zu betrachten. [canonical-constraint-keine-ablenkenden-animationen]

## Plattform und technische Produktvorgaben

- Die App soll plattformübergreifend auf iPhone und Android laufen. Ob zusätzlich Tablet-Unterstützung umgesetzt werden soll, muss noch als Anforderung geprüft werden. [canonical-open-requirement-plattform-und-tablet]

## Offene Anforderungen und Klärpunkte mit Produktbezug

- Es muss mit der Leitung der Einrichtung geklärt werden, ob die App die vollständige Dokumentation übernehmen soll. Dabei soll der Umfang eher begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation nicht verloren geht. [canonical-open-requirement-vollstaendige-dokumentation-offen]

- Bei neuen Uploads im About-Me-Bereich sollen allen mit der App verbundenen Nutzern derselben Einrichtung eine Popup-Benachrichtigung gesendet werden. [canonical-requirement-benachrichtigung-about-me-updates]

- Es soll optional und als mögliche spätere Erweiterung erwogen werden, beim ersten Login ein Tutorial oder einen Hilfebereich bereitzustellen, der die Nutzung der App sowie die wirksame Eingabe und Suche von Informationen erklärt. [canonical-open-requirement-tutorial-hilfe]

- Es soll optional erwogen werden, Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer zu unterstützen. [canonical-open-requirement-alternative-eingaben]

## Kontext, Constraints und zu klärende Rahmenbedingungen

- Zu den Bewohnern existieren bereits klassische Akten, in denen Informationen dokumentiert sind. [canonical-context-bewohnerakten-vorhanden]

- Es ist als unsicherer Risikokontext zu berücksichtigen, dass die bestehende schriftliche Dokumentation aus Sicht einzelner Nutzer oft zu umfangreich ist und gesuchte Informationen nicht schnell genug gefunden werden. [canonical-risk-dokumentation-schwer-nutzbar]

- Es muss im MVP geklärt werden, ob Projektbeteiligte aufgrund von Datenschutz Einsicht in Bewohnerakten erhalten können. [canonical-open-question-akteneinsicht-datenschutz]

- Es muss im MVP geklärt werden, ob Bilder gemacht bzw. eingeholt und testweise in die App eingepflegt werden dürfen und ob dafür die Zustimmung der Angehörigen vorliegt. [canonical-open-question-fotoeinwilligung]

- Die Einrichtung arbeitet derzeit überwiegend analog; zusätzliche Digitalisierung wird als vorteilhaft wahrgenommen. [canonical-context-analoger-ausgangszustand]

- Neue Mitarbeiter werden aktuell durch Dokumentation und begleitete Einarbeitung unterstützt, wobei erfahrene Mitarbeitende Kommunikationssituationen erklären, bis gegenseitiges Verständnis aufgebaut ist. [canonical-context-einarbeitung-ueber-erfahrene-mitarbeiter]

- Die vertiefte Datenschutzkonzeption wird vorerst nicht vollständig behandelt und ist als separates, zusätzliches Thema anzusehen. [canonical-process-datenschutz-nicht-im-detail]

- Für die Entwicklung soll Flutter mit Dart verwendet werden. [canonical-decision-flutter-dart]

- Firebase Firestore soll vorerst als primäre Datenbank verwendet werden. Im MVP muss noch geklärt werden, ob und wie Daten lokal auf dem Gerät gespeichert oder zwischengespeichert werden, statt bei jedem Seitenaufruf erneut aus der Cloud geladen zu werden. [canonical-open-requirement-firebase-und-lokalspeicherung]