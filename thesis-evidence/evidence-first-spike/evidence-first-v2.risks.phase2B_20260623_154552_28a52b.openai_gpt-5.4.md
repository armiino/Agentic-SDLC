# Risiken

## Hohe Risiken

- **Unzulässiger Zugriff durch das Zugangsmodell**: Die App ist nur für Login mit zugewiesenen Accounts vorgesehen; eine offene Selbstregistrierung ist nicht vorgesehen. Jede Abweichung davon würde ein hohes Datenschutz- und Zugriffsrisiko erzeugen.
- **Unzulässige Einsicht in fremde Bewohnerprofile**: Bewohner-Accounts sollen ausschließlich das eigene Profil sehen können. Fehler in der Rechtevergabe oder UI-Sichtbarkeit würden zu einem hohen Privacy-Risiko führen.
- **Einrichtungsübergreifender Datenzugriff**: Mitarbeitende sollen Profile nicht einrichtungsübergreifend sehen können. Falls die Isolation zwischen Einrichtungen fehlerhaft ist, besteht ein hohes Datenschutzrisiko.
- **Offene technische Umsetzung der Einrichtungsisolation**: Das Ziel der einrichtungsbezogenen Zugriffsbeschränkung ist entschieden, die technische Umsetzung ist jedoch noch offen. Dadurch besteht ein Umsetzungs- und Datenschutzrisiko, bis ein belastbares Konzept vorliegt.
- **Ungeklärte Einwilligungen für Bewohnerbilder**: Für Bilder in der App ist eine Datenschutz- bzw. Einwilligungsklärung mit Angehörigen noch zu klären. Solange dies offen ist, besteht ein hohes rechtliches und operatives Risiko für bildbezogene Funktionen.
- **Unklarer Zugriff auf bestehende Bewohnerakten**: Der Zugriff des Projektteams auf bestehende Akten ist aus Datenschutzgründen schwierig und derzeit unklar. Dies kann Analyse, Datenübernahme oder fachliche Validierung behindern.
- **Bewusste Datenschutz-Tiefe außerhalb des aktuellen Projektumfangs**: Datenschutz wird vorerst nicht vollständig im Detail behandelt; eine umfassende Lösung wird eher als separates Zusatzprojekt gesehen. Dadurch verbleibt ein hohes Restrisiko bezüglich späterer Compliance-Absicherung.
- **Erhöhter Schutzbedarf bei Medikationsinhalten im Kalender**: Medikationsinhalte sollen im Kalender berücksichtigt werden und werden als vertraulicher behandelt als normale Termine. Ohne angemessene Schutzmaßnahmen besteht ein hohes Datenschutz- und Fehlzugriffsrisiko.

## Mittlere Risiken

- **Spätere Benachrichtigungsfunktion kann Sichtbarkeitsregeln verletzen**: Optionale Popup-Benachrichtigungen für verbundene Nutzer derselben Einrichtung sind nur als gewünschte spätere Möglichkeit genannt. Falls diese Funktion später umgesetzt wird, muss sie mit Einrichtungsgrenzen und Datenschutzvorgaben vereinbar bleiben.
- **Ausgeschlossene Übersetzungs-Schnittstellenlösung darf nicht versehentlich in den Lösungsraum zurückkehren**: Ein bidirektionales Übersetzungssystem zwischen Bewohner und Betreuer ist ausdrücklich nicht Ziel der Lösung. Eine spätere Fehlinterpretation des Scopes könnte zu Aufwand in eine bewusst ausgeschlossene Richtung führen.

## Risikorelevante Beobachtungen mit geringem Risiko

- **Kalender als eigener Profilbereich**: Der Kalender ist als eigener Bereich pro Profil entschieden. Daraus ergibt sich laut Ledger kein eigenständiges hohes Risiko, die Funktion ist aber für nachgelagerte Zugriffs- und Vertraulichkeitsfragen relevant, insbesondere wenn dort sensible Inhalte erscheinen.