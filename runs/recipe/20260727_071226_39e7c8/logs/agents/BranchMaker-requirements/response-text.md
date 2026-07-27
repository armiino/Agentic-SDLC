# Response Text — BranchMaker-requirements

## Model Round 1

- No tool calls (standalone text response)
- Text length: 9168 chars

> # Requirements
> 
> ## Funktionale Anforderungen
> 
> - Die Lösung soll bevorzugt als digitale Lösung, zum Beispiel als Computerprogramm oder Ähnliches, die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern oder unterstützen. [canonical-produktvision-digitale-kommunikationshilfe]
> 
> - Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, statt Bewohner dabei zu unterstützen, andere besser zu verstehen. [canonical-fokus-bewohner-besser-verstehen]
> 
> - Die App soll Wissen über die Kommunikationsweisen einzelner Bewohner enthalten, damit Nutzer bei Verständnisproblemen nachschauen können, was gemeint sein könnte. [canonical-kernfunktion-kommunikationsnachschlagewerk]
> 
> - Nach dem Login soll die App auf eine Profilübersicht mit einer Liste aller Profile führen. [canonical-startseite-profiluebersicht]
> 
> - Auf der Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste vorhanden sein. Darunter sollen Profile als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung schnell auffindbar angezeigt werden. [canonical-profiluebersicht-suche-und-darstellung]
> 
> - Beim Öffnen eines Bewohnerprofils soll eine Detailansicht mit größerem Profilbild erscheinen, die interaktive Zugänge zu den Hauptbereichen About Me, Kommunikation, Kalender und No-Go bietet. [canonical-profilstruktur-hauptbereiche]
> 
> - Der About-Me-Bereich soll grundlegende personenbezogene Informationen, insbesondere Hobbys, Namen und Alter, sowie eine erweiterbare Bildliste mit Beschreibungen enthalten. Neu hinzugefügte Bilder sollen oben angezeigt werden. [canonical-about-me-bereich]
> 
> - Die App soll eine No-Go-Seite enthalten, auf der wichtige Dinge festgehalten werden, die im Umgang mit einem Bewohner unbedingt zu vermeiden sind. [canonical-no-go-seite]
> 
> - Die Kommunikationsseiten sollen eine Suchfunktion über eine Suchleiste oben im Bildschirm bereitstellen, damit Kommunikationsweisen gezielt auffindbar sind. [canonical-suche-kommunikationsseiten]
> 
> - Die Lösung soll Videos mit Beschreibungen zur visuellen Dokumentation von Situationen oder Kommunikationsweisen unterstützen. Diese Videofunktionalität soll in die Kommunikationsseiten integriert werden und nicht als eigener Bereich bestehen. [canonical-video-in-kommunikation]
> 
> - Es soll ein Rollenmodell mit mindestens Admin-, User-, Angehörigen- und Bewohner-Accounts geben. Unterschiedliche Rollen sollen unterschiedliche Rechte erhalten. Admins sollen Accounts intern anlegen und verwalten sowie Rechte vergeben und verwalten. User sollen Inhalte hinzufügen, aber nichts löschen können. [canonical-rollen-und-accountverwaltung]
> 
> - Auch Angehörige sollen Zugriff auf die App erhalten können, weil sie zusätzliches Wissen über die Bewohner beitragen können. [canonical-angehoerige-zugriff]
> 
> - Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen alle mit der App verbundenen Nutzer, die Teil derselben Einrichtung sind, eine Popup-Nachricht erhalten. [canonical-popup-benachrichtigungen-about-me]
> 
> - Auf der Profilübersicht soll ein Plus-Symbol berücksichtigt werden, über das per Dialog ein neues Profil mit Bild, Name und Beschreibung angelegt werden kann. [canonical-neue-profile-anlegen]
> 
> ## Offene bzw. zu klärende Anforderungen
> 
> - Die Kommunikationsseite soll in verbale und nonverbale Kommunikation unterteilt sein. Nutzer sollen dort neue Kommunikationsweisen, zum Beispiel per Text oder Bild, hinzufügen können. Dafür soll ein standardisiertes Beschreibungsmuster verwendet werden; die genaue Formulierung dieses Musters ist noch festzulegen. [canonical-kommunikationsseite-struktur]
> 
> - Ein Kalender mit Terminen und möglicherweise auch Medikamentengaben soll berücksichtigt werden. Die konkrete Umsetzung und der Umfang der Medikamentenfunktion sollen wegen der vertraulicheren Handhabung von Medikamentendaten evaluiert werden. [canonical-offener-kalender-und-medikamente]
> 
> - Die App soll plattformübergreifend auf iPhone und Android-Handys laufen. Ob zusätzlich Tablet-Unterstützung umgesetzt wird, muss noch bewertet werden. [canonical-plattform-mobile-und-tablet]
> 
> - Als zentrale Datenbank soll zunächst Firebase verwendet werden. Es muss noch geklärt werden, wie Daten aus der Cloud lokal auf dem Gerät gespeichert oder zwischengespeichert werden können, damit Inhalte beim Aufrufen einer Seite geladen und nicht immer erneut aus der Cloud nachgeladen werden müssen. [canonical-datenhaltung-firebase-und-lokale-speicherung]
> 
> - Ob die App die vollständige Dokumentation übernehmen soll, ist noch offen und mit der Leitung zu klären. Dabei soll ein zu großer Umfang vermieden werden, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. [canonical-offener-umfang-vollstaendige-dokumentation]
> 
> - Es soll zusätzlich einen Bewohner-Account mit speziellen, begrenzten Rechten geben; dies ist noch unsicher. Falls umgesetzt, soll dieser Account nur das eigene Profil sehen können, auf den About-Me-Bereich zugreifen und dort eigene Bilder hinzufügen dürfen. Die Funktionen sollen bewusst eingeschränkt werden, um Fehlbedienungen zu minimieren. [canonical-bewohner-account-und-rechte]
> 
> - Eine Hilfe-Funktion oder ein Tutorial zur Eingabe und Nutzung der App soll berücksichtigt werden, zum Beispiel als kurze Tour durch die App beim ersten Login oder als Hilfebereich. [canonical-offene-hilfe-und-tutorial]
> 
> - Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer sollen als mögliche spätere Erweiterung geprüft werden. [canonical-offene-alternative-eingaben]
> 
> ## Nicht-funktionale Anforderungen
> 
> - Die App soll barrierearm gestaltet werden, insbesondere mit großer Schrift, ausreichendem Kontrast und zurückhaltender Farbverwendung. [canonical-barrierefreiheit]
> 
> - Kommunikationsinhalte sollen nicht nur textuell, sondern auch über andere visuelle Darstellungen, insbesondere Bilder, abgebildet werden. [canonical-visuelle-darstellungen-kommunikation]
> 
> ## Constraints
> 
> - Die Lösung darf kein Übersetzungssystem sein, das als Schnittstelle zwischen Bewohner und anderen Personen dient und zwischen allgemeiner Sprache und der individuellen Sprache einzelner Bewohner hin- und herübersetzt. [canonical-kein-uebersetzungssystem]
> 
> - Die App soll nur eine Login-Möglichkeit ohne Selbstregistrierung bieten. Zugriff soll nur mit zugewiesenem Account möglich sein. Die App ist für den internen Gebrauch vorgesehen. Nur Admin-Accounts dürfen weitere Accounts anlegen und Rechte verwalten. Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen. [canonical-login-und-interner-zugang]
> 
> - Nach dem Login soll auf jeder Seite oben eine Appbar vorhanden sein. Diese soll mindestens Rücknavigation und Logout ermöglichen. Ein Einstieg zu Einstellungen soll berücksichtigt werden. [canonical-appbar-global]
> 
> - Mitarbeiter sollen nur die Profile der Bewohner sehen können, die zu ihrer jeweiligen Einrichtung gehören. Einrichtungsübergreifender Zugriff soll nicht bestehen. [canonical-zugriff-pro-einrichtung]
> 
> - Ablenkende Animationen sollen vermieden werden. [canonical-keine-ablenkenden-animationen]
> 
> ## Kontext und offene Klärungen
> 
> - Zu Bewohnern existieren bereits klassische Akten, in denen Informationen dokumentiert sind. [canonical-kontext-klassische-akten]
> 
> - Neue Mitarbeiter werden aktuell durch vorhandene Dokumentationen und begleitete Einarbeitung unterstützt; erfahrene Mitarbeitende erklären dabei Kommunikationssituationen und deren Bedeutung. [canonical-kontext-onboarding-neue-mitarbeiter]
> 
> - Die bestehende schriftliche Dokumentation ist für den schnellen Informationszugriff nur eingeschränkt nutzbar, weil sie umfangreich ist und gesuchte Inhalte oft schwer auffindbar sind. [canonical-risiko-dokumentation-schwer-nutzbar]
> 
> - Ob Projektbeteiligte Einsicht in Bewohnerakten erhalten können, ist wegen Datenschutz offen und soll geklärt werden. [canonical-offene-frage-akteneinsicht-datenschutz]
> 
> - Ob Bilder von Bewohnern für die App erhoben und testweise verwendet werden dürfen, muss datenschutzrechtlich und mit Angehörigen geklärt werden. [canonical-offene-frage-bildfreigaben]
> 
> ## Technischer und prozessualer Kontext
> 
> - Für die Entwicklung soll Flutter mit Dart verwendet werden. [canonical-tech-flutter-dart]
> 
> - Für die erste Entwicklung und Testung soll zunächst Android 11 als gemeinsamer Zielstand verwendet werden. [canonical-target-android-11]
> 
> - Alle Teammitglieder sollen dieselben Versionen von Flutter und Dart installieren. [canonical-einheitliche-versionen-flutter-dart]
> 
> - Für die Entwicklung soll nach Möglichkeit dieselbe Entwicklungsumgebung verwendet werden; bevorzugt wird Android Studio. [canonical-entwicklungsumgebung-android-studio]
> 
> - Der Einsatz des JetX-Pakets soll als technischer Rahmenpunkt berücksichtigt und erprobt werden. [canonical-offene-technik-jetx]
> 
> - Konkrete Regeln für den Programmierprozess, zum Beispiel Kommentarregeln, müssen noch festgelegt werden. [canonical-offene-frage-coding-regeln]
> 
> - Zur Anforderungsanalyse soll ein Kennenlern- bzw. Beobachtungstermin in einer oder mehreren Einrichtungen durchgeführt werden, um die aktuelle Kommunikation besser kennenzulernen. [canonical-prozess-vor-ort-beobachtung]

---

