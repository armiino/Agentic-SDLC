# Response Text — BranchMaker-requirements

## Model Round 1

- No tool calls (standalone text response)
- Text length: 6089 chars

> # Requirements
> 
> ## Funktionale Anforderungen
> 
> - Es muss eine digitale Lösung, z. B. ein Computerprogramm oder Ähnliches, bereitgestellt werden, um die Kommunikation zwischen beeinträchtigten Menschen und anderen Personen zu verbessern und zu fördern. [CAN-0001]
> - Es muss eine App entwickelt werden, die Informationen über die Art und Weise enthält, wie eine Person kommuniziert, sodass Betreuer bei Verständnisschwierigkeiten zu einer bestimmten Person in der App nachschauen können, was gemeint sein könnte. [CAN-0002]
> - Die App muss mindestens die folgenden Hauptbereiche enthalten: **About Me** für Informationen und Bilder, eine **Kommunikationsseite** für verbale und nonverbale Kommunikation, eine **Videoseite** zur Dokumentation von Kommunikationssituationen sowie eine Seite für **Terminkalender und Medikamentengabe**. [CAN-0003]
> - Die App muss verschiedene Account-Typen unterstützen: **Admin** mit erweiterten Rechten, **User** für Mitarbeiter und Angehörige mit eingeschränkten Rechten sowie **Bewohner**, die nur auf das eigene Profil zugreifen können. [CAN-0004]
> - Die App muss eine Suchleiste bereitstellen, mit der Profile sowie Kommunikationsarten, insbesondere verbal und nonverbal, schnell gefunden werden können. Die Erfassung der Kommunikationsweisen muss dabei durch ein standardisiertes Beschreibungsmuster unterstützt werden, z. B. in der Form „Körperteil“ gefolgt von „Aktion“. [CAN-0005]
> - Die App muss eine Login-Funktion bereitstellen, bei der sich nur Personen mit bereits zugewiesenem Account anmelden können. Nur Admins dürfen neue Accounts anlegen und Rechte verwalten. User dürfen Inhalte hinzufügen, aber weder Accounts erstellen noch Daten löschen. [CAN-0007]
> - Die App muss Daten aus der Cloud über Firebase beim Aufrufen einer Seite laden und lokal speichern, damit nicht bei jedem Zugriff alle Daten erneut geladen werden. [CAN-0008]
> 
> ## Nicht-funktionale Anforderungen
> 
> - Die App muss plattformübergreifend für **iOS** und **Android** entwickelt werden, vorzugsweise mit **Flutter** und **Dart**. [CAN-0006]
> - Das Design der App muss barrierefrei und intuitiv sein. Dazu gehören insbesondere große Schrift, ausreichender Kontrast, klares Branding, Hilfe-Funktionen und ein Fokus auf Benutzerfreundlichkeit. [CAN-0009]
> 
> ## Kontext, Constraints und offene Klärungen
> 
> - Die Einbindung der Mitarbeitermeinungen und ihrer Vorstellungen zur Systemgestaltung ist für die weitere Entwicklung der Kommunikationslösung relevant und soll geklärt werden. [ADJ-GAP-AU-0002]
> - Kommunikation ist in der Einrichtung ein zentraler täglicher Faktor; das Vorhaben ist aus einer Weiterentwicklungsmöglichkeit im Rahmen des Digiprosa-Projekts entstanden. Dieser Kontext ist für die Einordnung der Lösung zu berücksichtigen. [ADJ-GAP-AU-0003]
> - Es ist festzuhalten, dass das System nicht als individuelle Sprachübersetzungs-Schnittstelle zwischen Bewohnern und Betreuern ausgelegt werden soll, da dies als weder möglich noch kosteneffizient und nicht allgemein nutzbar beschrieben wurde. [ADJ-GAP-AU-0004]
> - Das Entwicklungsteam führt eine Anforderungsanalyse gemeinsam mit den späteren Nutzern durch und besteht aus zwei Informatikern sowie zwei Studierenden der sozialen Arbeit. [ADJ-GAP-AU-0006]
> - In der Einrichtung existieren bereits klassische Akten, in denen Informationen über die Bewohner dokumentiert werden. [ADJ-GAP-AU-0007]
> - Es soll geklärt werden, wie regelmäßig bestehende Dokumentationen erweitert und tatsächlich gelesen werden, da umfangreiche Dokumentation im Alltag nicht immer genutzt wird. [ADJ-GAP-AU-0017]
> - Der Zugriff auf bestehende Dokumentationen ist datenschutzbezogen zu prüfen. [ADJ-GAP-AU-0019]
> - Für die Einarbeitung neuer Mitarbeiter ist relevant, dass diese durch Dokumentationen und Begleitung unterstützt werden, bis sie Bewohner verstehen und Vertrauen aufgebaut ist; die genaue Ausgestaltung soll weiter erhoben werden. [ADJ-GAP-AU-0021]
> - Es besteht der Wunsch, Dokumentation und Prozesse stärker zu digitalisieren, da die Einrichtung bisher überwiegend analog arbeitet. [ADJ-GAP-AU-0024]
> - Für die Nutzung von Bildern in der App muss die Datenschutzthematik mit Angehörigen geklärt und es müssen Einwilligungen eingeholt werden, bevor Bilder testweise in die App eingepflegt werden. [ADJ-GAP-AU-0029]
> - Es ist offen, ob zusätzlich zu den bereits benannten Hauptbereichen ein **No-Go-Bildschirm** aufgenommen werden soll, der festhält, was in Gegenwart des Bewohners absolut nicht erlaubt ist. [ADJ-GAP-AU-0039]
> - Es ist als offene Ausgestaltung notiert, dass nach dem Login eine Profilübersicht mit anklickbaren Profilen vorgesehen sein könnte, um den Zugriff auf Profildaten auf berechtigte Nutzer zu beschränken. [ADJ-GAP-AU-0045]
> - Es ist offen, ob die App nach dem Login auf jeder Seite eine Appbar mit Rücknavigation, Logout-Funktion und Zugang zu Einstellungen enthalten soll. [ADJ-GAP-AU-0054]
> - Es ist offen, ob eine No-Go-Seite dynamisch über einen Plus-Button erweitert werden können soll. [ADJ-GAP-AU-0055]
> - Die datenschutzkonforme Einschränkung des Zugriffs auf Bewohnerprofile auf Mitarbeiter der jeweils eigenen Einrichtung ist zu klären. [ADJ-GAP-AU-0061]
> - Es ist offen, ob ein Tutorial oder eine Hilfe-Seite aufgenommen werden soll, die Nutzern zeigt, wie Informationen effektiv eingegeben und gesucht werden können. [ADJ-GAP-AU-0086]
> - Es ist offen, ob die Videofunktionalität statt einer separaten Videoseite in die Kommunikationsseiten für verbal und nonverbal integriert werden soll, um Navigation zu vereinfachen und Videos besser auffindbar zu machen. [ADJ-GAP-AU-0074]
> - Es ist offen, ob die App so ausgestaltet werden soll, dass sie dynamisch wächst und Angehörige bereits vorab Daten für neue Bewohner einpflegen können. [ADJ-GAP-AU-0077]
> - Es ist offen, ob bei neuen Uploads im About-Me-Bildschirm Popup-Nachrichten an alle verbundenen Nutzer derselben Einrichtung gesendet werden sollen. [ADJ-GAP-AU-0078]
> - Zur besseren Einschätzung der aktuellen Kommunikationssituation soll ein Kennenlern-Tag mit beeinträchtigten Personen in einer oder mehreren Einrichtungen eingeplant werden. [ADJ-GAP-AU-0009]

---

