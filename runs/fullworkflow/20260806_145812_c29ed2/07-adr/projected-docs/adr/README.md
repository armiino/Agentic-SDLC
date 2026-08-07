# Architecture Decision Records

> Projektion aus dem Core (nie von Hand editieren — Änderungen laufen über die Meeting-/Decision-Bahn).

| ADR | Titel/Fakt | Status | Core-Item |
|---|---|---|---|
| ADR-0001 | Die Loesung bleibt eine digitale App zur Foerderung der Kommunikation und wird als reine M… | accepted | ARCH-01 |
| ADR-0002 | Als Kernansatz soll die App Wissen über Profil und Kommunikationsweise einer Person bereit… | accepted | ARCH-03 |
| ADR-0003 | Die App darf keine Selbstregistrierung erlauben; Zugang erfolgt nur per Login mit intern v… | accepted | ARCH-07 |
| ADR-0004 | Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, … | accepted | ARCH-08 |
| ADR-0005 | Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten und Inhalte beziehu… | accepted | ARCH-09 |
| ADR-0006 | Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil sehen darf und … | accepted | ARCH-10 |
| ADR-0007 | Mitarbeiter dürfen nicht einrichtungsübergreifend auf alle Profile zugreifen, sondern nur … | accepted | ARCH-11 |
| ADR-0008 | Die konkrete Ausgestaltung der Rechteverwaltung ist noch nicht festgelegt und muss später … | accepted | ARCH-12 |
| ADR-0009 | Für die Rechte- und Accountverwaltung soll ein separater Admin-Bildschirm erwogen werden, … | accepted | ARCH-13 |
| ADR-0010 | Nach dem Login soll eine Profilübersicht mit anklickbarer Liste der sichtbaren Bewohnerpro… | accepted | ARCH-15 |
| ADR-0011 | Auf der Profilübersicht soll eine Suchleiste vorhanden sein, um Profile schnell nach Namen… | accepted | ARCH-16 |
| ADR-0012 | Profile sollen in der Übersicht mit Vorschaubild, Name und Kurzbeschreibung dargestellt we… | accepted | ARCH-17 |
| ADR-0013 | Die Detailansicht eines Profils soll das Profilbild größer zeigen und die Hauptbereiche de… | accepted | ARCH-18 |
| ADR-0014 | Die App soll je Bewohner eine About-Me-Seite mit persönlicher Kurzinfo, Bildern und beschr… | accepted | ARCH-19 |
| ADR-0015 | Die About-Me-Ansicht soll eine dynamisch erweiterbare Foto-Timeline mit Beschreibungen ber… | accepted | ARCH-20 |
| ADR-0016 | Die App soll eine Kommunikationsansicht mit klarer Unterteilung in verbale und nonverbale … | accepted | ARCH-21 |
| ADR-0017 | Einträge zu Kommunikationsweisen müssen dynamisch erweiterbar sein und sollen nicht nur al… | accepted | ARCH-22 |
| ADR-0018 | Videos von Kommunikationssituationen sollen mit Beschreibungen erfasst werden können und a… | accepted | ARCH-23 |
| ADR-0019 | Auf Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss ein standardisi… | accepted | ARCH-24 |
| ADR-0020 | Die App soll eine No-Go-Seite mit dynamisch erweiterbarer Liste bereitstellen, auf der kri… | accepted | ARCH-25 |
| ADR-0021 | Eine Kalenderfunktion soll erwogen werden; zusätzlich ist zu prüfen, ob auch Medikamenteng… | accepted | ARCH-26 |
| ADR-0022 | Es soll erwogen werden, in der Profilübersicht das Anlegen neuer Profile per Plus-Symbol u… | accepted | ARCH-27 |
| ADR-0023 | Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein. | accepted | ARCH-29 |
| ADR-0024 | Die Appbar soll Rücknavigation sowie schnellen Zugriff auf Seitentitel, Einstellungen und … | accepted | ARCH-30 |
| ADR-0025 | Alternative Eingabemethoden wie Sprachbefehle sollen als mögliche Accessibility-Erweiterun… | accepted | ARCH-32 |
| ADR-0026 | Der Login-Screen soll ein zentriertes, gut sichtbares Logo im oberen Drittel enthalten; ei… | accepted | ARCH-33 |
| ADR-0027 | Eine Hilfe-Funktion oder ein Tutorial soll vorgesehen werden, idealerweise als kurze Tour … | accepted | ARCH-34 |
| ADR-0028 | Die App soll plattformübergreifend auf iPhone und Android laufen. | accepted | ARCH-36 |
| ADR-0029 | Für die Entwicklung soll Flutter mit Dart verwendet werden, um die plattformübergreifende … | accepted | ARCH-37 |
| ADR-0030 | Das Paket GetX soll als technische Option berücksichtigt und in der Umsetzung erprobt werd… | accepted | ARCH-38 |
| ADR-0031 | Firebase Firestore ist als Datenbanklösung vorläufig vorgesehen, jedoch noch nicht endgült… | accepted | ARCH-39 |
| ADR-0032 | Es muss geklärt werden, ob und wie Cloud-Daten lokal auf dem Gerät gespeichert oder gecach… | accepted | ARCH-40 |
| ADR-0033 | Es soll evaluiert werden, ob die App zusätzlich auf Tablets nutzbar sein kann. | accepted | ARCH-41 |
| ADR-0034 | Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselb… | accepted | ARCH-42 |
