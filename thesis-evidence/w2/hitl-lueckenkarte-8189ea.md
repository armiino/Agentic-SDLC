# Die vollständige Lücken-Karte des Stresslaufs — alle 109 Referenzaussagen, Status und Weg

> Status: MOMENTAUFNAHME 08.09.2026 — ÜBERHOLT durch die Nachreview-Konsolidierung.
> Aktueller Stand: runs/w2-nachreview-konsolidierung.json (+ Anhang D.8). ÜBERHOLTE Kernaussagen
> dieser Datei: die Obergrenzen-Treppe (0,835 ist Adressierbarkeit, KEINE Obergrenze menschlicher
> Behebbarkeit) und „Liste C/D = nie vorgelegt" (Claim-Review-Zitate sind ein zweiter
> Expositionskanal, z. B. CAN-030/031). Die Einzel-Listen A–E bleiben als Rohmaterial gültig,
> die Urteile sind auf dem ALTEN Stand (55/30/24).

> Status: ERSTELLT 08.09.2026 — die EINE Datei zur Frage „was ist gedeckt, was liegt dem
> Menschen vor, was geht unter und warum". Generiert aus: Gold v2 (109 Aussagen, Hash ②) ·
> v2-Urteilsblatt `runs/w2-goldv2-eval-ledger-8189ea.json` · 01d-Signale
> `runs/ledger/20260905_164650_8189ea/step-01d-unused-unit-ledger-compare/output.json` ·
> echte Queue `thesis-evidence/w2/hitl-queue-8189ea/queue.json` (45 Items, deterministisch) ·
> Claim-Bestand `capture/lcr-machine.json` (47 Claims → Liste E).
> Lauf: `20260905_164650_8189ea` (Interview-Stressfall, gpt-5.4). Detail-Analyse:
> `hitl-queue-analyse.md` (§0–§7). Kopie: `kapitel-7/04-material-stufenanalyse/`.

## 1 · Was passiert im System — der Weg einer Aussage (Kurz-Erklärung)

1. **Extraktion (step-01):** Das Modell zerlegt die 136 Quell-Units in 54 Kandidaten. Was
   hier nicht erfasst wird, ist die Hauptquelle der Lücken (52 der 54 finalen Lücken
   bestehen schon nach diesem Schritt).
2. **Kanonisierung (step-02):** 54 Kandidaten → 47 Claims (7 Zweier-Merges). EIN Merge
   verlor Inhalt (betrifft G-IE-021/023); der deterministische Übergangs-Check prüft nur
   Referenzen, nicht Inhalt — er meldete nichts.
3. **Facetten-Validierung (step-03):** prüft/repariert Facetten und stellt 18 Claims auf
   `review_required` — diese landen in der HITL-Queue, können dort aber nur Facetten
   korrigieren, KEINE fehlenden Inhalte ergänzen.
4. **Unit-Bilanz + Signale (01b→01d):** 61 unverwendete Units → 48 vom LLM-Agenten
   `UnusedUnitLedgerComparer` beurteilte Signale: `missing_claim` (1) · `needs_human` (2) ·
   `attach_as_evidence` (24) · `already_covered_indirectly` (21).
5. **Queue-Filter (deterministisch, `LedgerAdjudicationAdapter`):** Nur
   missing_claim/needs_human/attach kommen in die Adjudikations-Queue (45 Items = 18
   review_required + 27 Signale). **`already_covered` wird verworfen** — bei
   Fehlklassifikation (8 Gold-Lücken, Liste C) geht die Lücke damit unsichtbar unter.
6. **HITL:** Der Mensch kann vorgelegte Units per `accept_gap`/`promote_to_claim` zu neuen
   Claims machen (Coverage-Gewinn möglich); `attach_evidence` heftet nur an und schließt
   keine Inhaltslücke. Was nicht vorgelegt ist, kann er nicht ergänzen.

**Obergrenzen-Treppe (strict):** maschinell 55/109 = 0,505 → perfekter Mensch auf der
echten Queue 83/109 = **0,761** → zusätzlich perfekte 01d-Klassifikation 91/109 = **0,835**
(= exakt die Adressierbarkeit) → strukturell unerreichbar bleiben 18/109 = 16,5 % (Liste D).
Die 8 aus Liste C sind also KEINE Konstruktionsgrenze, sondern Folge einer behebbaren
Fehlklassifikations-mal-Filter-Kette; die 18 aus Liste D sind die echte Konstruktionsgrenze
der Unused-Unit-Bilanz (intra-unit-Verluste).

## 2 · Zählung

| Status | n |
| --- | --- |
| A · GEDECKT — vollständig im Ledger extrahiert | 55 |
| B · LÜCKE, dem Menschen VORGELEGT (in der echten Queue; schließbar) | 28 (3 explizit · 25 Kontext) |
| C · LÜCKE, VERLOREN durch already_covered-Fehlklassifikation (nie vorgelegt) | 8 |
| D · LÜCKE, STILL verloren — Units verwendet, kein Signal (nie vorgelegt) | 18 |
| Summe | 109 |

Dazu Liste E: der komplette extrahierte Bestand (47 Claims) mit Rück-Zuordnung zu den
Gold-Aussagen — die Gegenperspektive zu den Listen A–D.

## 3 · Liste A — GEDECKT (55): vollständig im Ledger extrahiert

| Gold | Referenzaussage | Ledger-Beleg (v2-Urteilsblatt) |
| --- | --- | --- |
| G-IE-001 | Die bevorzugte Lösung soll digital umgesetzt werden. | CAN-002: digitale Lösung bevorzugt |
| G-IE-002 | Ein direktes Übersetzungssystem zwischen Standardsprache und der individuellen Kommunikationsweise einzelner Bewohner wird als Lösungsansatz ausgeschlossen. | CAN-003: must_not direktes Übersetzungssystem |
| G-IE-003 | Die Unterstützung soll primär anderen Personen helfen, Bewohner besser zu verstehen, statt Bewohner beim Verstehen der Betreuer zu unterstützen. | CAN-006: primär Bewohner-Verstehen priorisiert |
| G-IE-004 | Als Lösungsform wird eine digitale App verfolgt. | CAN-007: 'Lösung soll als App…' |
| G-IE-005 | Die App soll Wissen über die individuelle Kommunikationsweise eines Bewohners speichern. | CAN-007: Wissen über individuelle Kommunikationsweise festhalten |
| G-IE-007 | Die App soll die Kommunikation zwischen einer beeinträchtigten Person und anderen Personen unterstützen, ermöglichen oder verbessern. | CAN-001 |
| G-IE-010 | Ob die App die vollständige bestehende Bewohnerdokumentation übernehmen soll, ist noch mit der Einrichtungsleitung zu klären. | CAN-012: Klärung mit Leitung + Umfangsbegrenzung — DECKT die F-Lücke |
| G-IE-012 | Die App soll ihren Schwerpunkt auf unterstützende Kommunikation behalten und nicht durch einen zu großen Funktionsumfang davon abweichen. | CAN-012: 'Fokus auf unterstützender Kommunikation erhalten' |
| G-IE-013 | Der Kalender soll Medikamentengaben abbilden. | CAN-024: Kalender mit Terminen und Medikamentengaben |
| G-IE-016 | Auch Angehörige sollen Zugriff auf die App erhalten. | CAN-017 |
| G-IE-018 | Die App soll einen No-Go-Bereich enthalten, in dem für einen Bewohner festgehalten wird, was in seiner Gegenwart unbedingt zu vermeiden ist. | CAN-018 |
| G-IE-019 | Ein Admin soll neue Accounts anlegen können. | CAN-019: 'Admins legen Accounts an' |
| G-IE-022 | Ein Admin soll die Rechte beziehungsweise Rollen von Accounts verwalten können. | CAN-019: 'verwalten Rechte' |
| G-IE-024 | Die App soll nur mit zuvor zugewiesenen Accounts nutzbar sein; eine öffentliche Selbstregistrierung soll nicht angeboten werden. | CAN-019 + CAN-042: zugewiesene Accounts, keine (Selbst-)Registrierung |
| G-IE-026 | Die Profilübersicht soll eine Übersicht der Bewohnerprofile anzeigen. | CAN-020: anklickbare Liste = spezifische Form der geforderten Übersicht; 'Liste' folgt AU-0045 wörtlich (Grenzfall #1) |
| G-IE-027 | Nach erfolgreichem Login soll der Nutzer auf die Profilübersicht weitergeleitet werden. | CAN-020: nach Login auf Profilübersicht |
| G-IE-029 | Der About-Me-Bereich soll Bilder mit Beschreibungen enthalten. | CAN-022: erweiterbare Fotoliste mit Beschreibungen |
| G-IE-033 | Der Kommunikationsbereich soll in verbale und nonverbale Kommunikation getrennt sein. | CAN-023 |
| G-IE-034 | Kommunikationsweisen sollen mit Texten und Bildern dokumentiert werden können. | CAN-023: mit Texten oder Bildern (gleicher Fähigkeitsumfang) |
| G-IE-036 | Die App soll einen Kalender für Termine enthalten. | CAN-024 |
| G-IE-039 | Nach dem Login soll auf jeder Seite eine konsistente Appbar angezeigt werden. | CAN-025: Appbar auf jeder Seite |
| G-IE-040 | Die Ausloggen-Funktion soll schnell erreichbar sein. | CAN-025: Logout in der Appbar jeder Seite = schnell erreichbar (Entailment) |
| G-IE-042 | Eine vertiefte Datenschutzkonzeption wird im aktuellen Projekt nicht ausgearbeitet und gegebenenfalls als separates Folgeprojekt behandelt. | CAN-026 |
| G-IE-044 | Die App ist für den internen Gebrauch der Einrichtung vorgesehen und soll nicht beliebigen externen Personen offenstehen. | CAN-027: interner Gebrauch |
| G-IE-045 | Mitarbeiter sollen nur Profile von Bewohnern der Einrichtung sehen können, in der sie tätig sind. | CAN-027: nur Profile der eigenen Einrichtung |
| G-IE-046 | Die App soll auf iOS- und Android-Smartphones lauffähig sein. | CAN-028: iPhone+Android ('möglichst' = quellennäherer Verpflichtungsgrad, gleicher Gehalt) |
| G-IE-047 | Ob die App auch auf Tablets unterstützt werden soll, ist noch zu evaluieren. | CAN-029 |
| G-IE-050 | Für Kommunikationsweisen soll ein systematisches Beschreibungsmuster verwendet werden, damit Einträge besser gesucht und gefiltert werden können. | CAN-032: einheitliches Beschreibungsmuster für gezieltes Finden |
| G-IE-051 | Die genaue Ausgestaltung des Beschreibungsmusters für Kommunikationsweisen ist noch festzulegen. | CAN-032: must_clarify = Offenheit ausgesagt |
| G-IE-052 | Die zuvor separat geplante Videoseite wird verworfen; Videofunktionen sollen in die verbalen und nonverbalen Kommunikationsseiten integriert werden. | CAN-033: kein eigener Screen, Integration in Kommunikationsseiten |
| G-IE-053 | Im Kommunikationsbereich sollen Videos mit Beschreibungen zu Verhaltensweisen oder Kommunikationsarten hinzugefügt werden können. | CAN-016: Videos mit Beschreibung erfassen |
| G-IE-055 | Wenn im About-Me-Bereich etwas Neues hochgeladen wird, sollen alle mit der App verbundenen Nutzer derselben Einrichtung eine Popup-Benachrichtigung erhalten. | CAN-034 |
| G-IE-056 | Ein eigener Bewohner-Account soll vorgesehen werden. | CAN-035 |
| G-IE-057 | Ein Bewohner-Account darf in der Profilübersicht nur das eigene Profil sehen. | CAN-035: nur eigenes Profil sehen |
| G-IE-058 | Für Bewohner-Accounts werden zunächst der Zugriff auf das eigene Profil und die eigenen About-Me-Funktionen vorgesehen; diese Beschränkung soll Fehlbedienungen reduzieren. | CAN-035 deckt die v2-Engfassung passgenau: eigenes Profil + About-Me-Zugriff + eigene Bilder + Fehlbedienungs-Zweck |
| G-IE-059 | Ein Bewohner-Account soll auf den eigenen About-Me-Bereich zugreifen können. | CAN-035: About-Me-Zugriff |
| G-IE-060 | Ein Bewohner-Account soll im eigenen About-Me-Bereich eigene Bilder hinzufügen können. | CAN-035: eigene Bilder hinzufügen |
| G-IE-062 | Der Admin-Bereich soll nur für Admin-Accounts sichtbar sein. | CAN-043: rollenabhängige Differenzierung + Admin-Funktionen (Entailment: nur Admins sehen Admin-Funktionen) |
| G-IE-064 | Bei der Account-Anlage im Admin-Bereich sollen die drei Rollen Admin, User und Bewohner vergeben werden können. | GEBÜNDELT CAN-019 (Admin+User, Admins legen an) + CAN-035 (Bewohner-Account) = drei Rollen |
| G-IE-065 | Für die plattformübergreifende Entwicklung werden Flutter und Dart eingesetzt. | CAN-036 |
| G-IE-067 | Firebase Firestore wird zunächst als Datenbanklösung verwendet. | CAN-038: Firestore zunächst |
| G-IE-068 | Wie Daten aus Firebase lokal auf dem Gerät gespeichert beziehungsweise zwischengespeichert werden, ist noch nicht abschließend geklärt. | CAN-038: lokale Speicherung muss geklärt werden |
| G-IE-070 | Für die erste Android-Entwicklung und das Testen wird Android 11 als Zielversion verwendet. | CAN-039 |
| G-IE-071 | Das JetX-Package wird in die technischen Rahmenbedingungen aufgenommen und zunächst erprobt. | CAN-041 |
| G-IE-084 | Die Suchleiste der Profilübersicht soll gut sichtbar unter der Appbar platziert sein. | CAN-030: gut sichtbare Suchleiste unter der Appbar |
| G-IE-096 | Der Settings-Screen soll abhängig von der Nutzerrolle eine differenzierte Ansicht beziehungsweise unterschiedliche Funktionen anbieten; für Admins sind dort das Hinzufügen von Accounts und das Anpassen von Rechten vorgesehen. | CAN-043 deckt auch die v2-Verortung: Admins sollen DORT (Einstellungsseite) Accounts hinzufügen und Rechte anpassen |
| G-IE-097 | Jeder Nutzer soll sein eigenes Profil bearbeiten können. | CAN-043: Profil ändern |
| G-IE-098 | Jeder Nutzer soll persönliche Einstellungen wie die Sprache ändern können. | CAN-043: Sprache wählen |
| G-IE-100 | Beim ersten Login soll eine kurze Einführungstour durch die App angeboten werden. | CAN-044: kurze Tour beim ersten Login |
| G-IE-101 | Die App soll einen jederzeit wieder aufrufbaren Hilfebereich enthalten. | CAN-044: erneut aufrufbarer Hilfebereich |
| G-IE-102 | Die App soll große Schrift verwenden. | CAN-045 |
| G-IE-103 | Die App soll ausreichenden Kontrast bieten. | CAN-045 |
| G-IE-104 | Die App soll nicht zu viele Farben verwenden. | CAN-045 |
| G-IE-106 | Ob Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigt werden, bleibt zu prüfen. | CAN-046 |
| G-IE-107 | Falls Animationen oder zusätzliches Feedback dennoch eingesetzt werden, dürfen sie nicht zu ablenkend sein. | CAN-047: ablenkende nicht einbauen ≡ falls doch, nicht ablenkend |

## 4 · Liste B — VORGELEGT (28): Lücke steht in der Adjudikations-Queue, der Mensch kann sie schließen

Vorlage-Art: EXPLIZIT = missing_claim-Item mit fertigem Vorschlagstext, die Lücke wird ausdrücklich
beanstandet · KONTEXT = attach/needs_human-Item, der fehlende Inhalt steckt in der vorgelegten Unit,
der Mensch muss ihn selbst erkennen und per promote_to_claim/accept_gap formulieren.

| Gold | Urteil | Referenzaussage | Was fehlt (v2-Beleg) | Queue-Weg (Unit [Verdikt]) | Vorlage |
| --- | --- | --- | --- | --- | --- |
| G-IE-008 | partial | Die App soll neuen Mitarbeitenden und anderen bislang unvertrauten Personen ermöglichen, Bewohner schneller zu verstehen. | CAN-007 'damit andere bei Verständigungsproblemen nachschauen können' deckt den Kern (andere verstehen Bewohner); Zielgruppe 'neue Mitarbeitende' + 'schneller' fehlen | AU-0024 [attach_as_evidence] | KONTEXT |
| G-IE-009 | none | Die App soll den Einarbeitungsaufwand für neue Mitarbeitende verringern. | KEIN Claim zur Verringerung des Einarbeitungsaufwands | AU-0024 [attach_as_evidence] | KONTEXT |
| G-IE-015 | none | Der verbale und der nonverbale Bereich sollen über je einen eigenen Button erreichbar sein, der zur jeweiligen Unterseite navigiert. | Bereichs-Trennung ist bereits G-033 gutgeschrieben; der EIGENSTÄNDIGE Gehalt von G-015 (je eigener Navigations-Button zur Unterseite) fehlt komplett — partial wäre Doppel-Kredit | AU-0126 [attach_as_evidence] | KONTEXT |
| G-IE-017 | partial | Angehörige sollen bei neuen Bewohnern bereits vorab Daten in die App einpflegen können. | CAN-017: Beitragen ✓, aber 'vorab bei neuen Bewohnern' fehlt (Bedingung) | AU-0077 [attach_as_evidence] | KONTEXT |
| G-IE-028 | partial | Ein Profil der Profilübersicht soll auswählbar sein und zur Detailansicht des gewählten Profils führen. | CAN-020: anklickbar ✓, aber Ziel Detailansicht fehlt — GENAU das meldet Signal AU-0124 (missing_claim) | AU-0124 [missing_claim (semantische Zuordnung)] | EXPLIZIT |
| G-IE-035 | partial | Der Kalender soll eine übersichtliche Monatsansicht bieten. | CAN-024: übersichtlich ✓, aber Monatsansicht fehlt | AU-0129 [attach_as_evidence] | KONTEXT |
| G-IE-041 | none | Neue No-Go-Einträge sollen über einen Plus-Button dynamisch hinzugefügt werden können. | KEIN Claim zum Plus-Button auf der No-Go-Seite (nur CAN-018 Existenz) | AU-0055 [attach_as_evidence] | KONTEXT |
| G-IE-049 | partial | Auf den Kommunikationsseiten soll über eine Suchleiste oben im Bildschirm nach Kommunikationsweisen beziehungsweise Merkmalen wie einem Körperteil gesucht oder gefiltert werden können. | CAN-032: Suchfunktion ✓, aber Position oben + Merkmals-Beispiele fehlen | AU-0127 [attach_as_evidence] | KONTEXT |
| G-IE-054 | partial | Neu hinzugefügte Erfahrungen sollen unmittelbar über die App mit anderen berechtigten Nutzern geteilt werden können. | CAN-013+CAN-034 gebündelt: Hinzufügen + About-Me-Benachrichtigung ✓, aber unmittelbares Teilen ALLER Erfahrungen mit Berechtigten fehlt | AU-0077 [attach_as_evidence] | KONTEXT |
| G-IE-063 | partial | Die detaillierte Rechteverwaltung der vorgesehenen Account-Rollen ist noch festzulegen. | analog G-061 (gleiche Regel-Logik): CAN-019/CAN-043 tragen den Rechteverwaltungs-Kern, der OFFENHEITS-Status ('noch festzulegen') fehlt | AU-0088 [attach_as_evidence] | KONTEXT |
| G-IE-072 | partial | Der Login-Screen soll ein deutlich sichtbares Logo enthalten. | CAN-042: Logo ✓, aber 'deutlich sichtbar' fehlt | AU-0117 [attach_as_evidence] | KONTEXT |
| G-IE-073 | none | Das Login-Logo soll vorzugsweise zentriert im oberen Drittel des Bildschirms platziert werden. | kein Claim zur Logo-Platzierung (CAN-042 nennt nur die Login-Elemente) | AU-0117 [attach_as_evidence], AU-0118 [attach_as_evidence] | KONTEXT |
| G-IE-074 | none | Das Login-Logo soll einen inhaltlichen Bezug zum Thema Kommunikation haben. | KEIN Claim zum Kommunikations-Bezug des Logos | AU-0117 [attach_as_evidence] | KONTEXT |
| G-IE-075 | none | Das Login-Logo soll vorzugsweise rund gestaltet sein. | KEIN Claim zur runden Gestaltung | AU-0117 [attach_as_evidence] | KONTEXT |
| G-IE-076 | none | Ob die Login-Eingabefelder einen leichten Schattenwurf erhalten, bleibt offen. | KEIN Claim zur offenen Schattenwurf-Frage | AU-0118 [attach_as_evidence] | KONTEXT |
| G-IE-077 | partial | Unter dem Logo sollen Felder für E-Mail und Passwort angeordnet sein. | CAN-042: Felder ✓, aber Anordnung unter dem Logo fehlt (Position) | AU-0118 [attach_as_evidence] | KONTEXT |
| G-IE-078 | none | Das Design der App soll intuitiv und benutzerfreundlich sein. | KEIN Claim zu intuitiv/benutzerfreundlich (CAN-045 = Barrierefreiheit, anderes Konstrukt) | AU-0118 [attach_as_evidence] | KONTEXT |
| G-IE-081 | none | Die Appbar soll den Namen der aktuell angezeigten Seite mittig darstellen. | KEIN Claim zum mittigen Seitennamen in der Appbar | AU-0120 [attach_as_evidence] | KONTEXT |
| G-IE-082 | partial | Die Appbar soll links eine Zurück-Navigation zur vorherigen Seite bereitstellen; als Symbol ist ein nach links zeigender Pfeil vorgesehen. | CAN-025: Rücknavigation ✓; Position links und (v2) Pfeilsymbol fehlen | AU-0120 [attach_as_evidence] | KONTEXT |
| G-IE-083 | partial | Die Appbar soll rechts ein Einstellungssymbol enthalten, das zur Einstellungsseite führt. | CAN-025: Einstellungsfunktionen als offen deklariert, aber Einstellungssymbol rechts → Einstellungsseite nicht ausgesagt | AU-0120 [attach_as_evidence] | KONTEXT |
| G-IE-088 | none | Die Detailansicht eines ausgewählten Profils soll das Profilbild größer zeigen. | KEIN Claim zum größeren Profilbild in der Detailansicht — GENAU von Signal AU-0124 (missing_claim) gemeldet | AU-0124 [missing_claim] | EXPLIZIT |
| G-IE-089 | partial | Die Detailansicht eines Profils soll die vier Hauptbereiche der App als interaktive Buttons darstellen. | CAN-021: Bereiche existieren ✓ (aber nur drei genannt), interaktive Buttons in der Detailansicht fehlen | AU-0124 [missing_claim] | EXPLIZIT |
| G-IE-090 | none | Der verbale und der nonverbale Kommunikationsbereich sollen visuell durch Symbole voneinander unterschieden werden. | KEIN Claim zur visuellen Unterscheidung durch Symbole | AU-0126 [attach_as_evidence] | KONTEXT |
| G-IE-091 | none | Der No-Go-Screen soll ein starkes visuelles Warnsymbol verwenden; ein rotes Stoppschild wird als mögliche Ausprägung genannt. | KEIN Claim zum Warnsymbol/Stoppschild | AU-0128 [attach_as_evidence] | KONTEXT |
| G-IE-092 | none | No-Go-Einträge sollen einfach zu überblicken beziehungsweise zu durchforsten sein. | KEIN Claim zur Überblickbarkeit der No-Gos | AU-0128 [attach_as_evidence] | KONTEXT |
| G-IE-093 | none | No-Go-Einträge sollen sich auf die wichtigsten Informationen beschränken. | KEIN Claim zur Beschränkung auf wichtigste Informationen | AU-0128 [attach_as_evidence] | KONTEXT |
| G-IE-094 | none | Ein Antippen eines Datums im Kalender soll Details zu Terminen oder Medikamentengaben öffnen. | KEIN Claim zum Datum-Antippen → Details | AU-0129 [attach_as_evidence] | KONTEXT |
| G-IE-095 | none | Medikamentenerinnerungen sollen in der Kalenderansicht kenntlich gemacht werden; als mögliche Gestaltung werden kleine Icons oder Tags an den betreffenden Tagen vorgeschlagen. | CAN-024 deckt Medikamente-im-Kalender (= G-013); der v2-Kern 'Erinnerungen kenntlich gemacht' (Icons/Tags als mögliche Gestaltung) fehlt | AU-0129 [attach_as_evidence] | KONTEXT |

## 5 · Liste C — VERLOREN durch already_covered (8): Signal existierte, wurde aber fehlklassifiziert und vom Queue-Filter verworfen

Entscheidungsort: Der LLM-Agent `UnusedUnitLedgerComparer` (Stufe 01d) vergab `already_covered_indirectly`,
obwohl der Prompt-Maßstab das nur bei VOLLER semantischer Deckung erlaubt; der deterministische Queue-Filter
(`LedgerAdjudicationAdapter.DecidableCompareVerdicts`) verwirft diese Verdikte kommentarlos. Korrekt wäre in
7 von 8 Fällen `attach_as_evidence` gewesen (Kern gedeckt, Qualifikation fehlt) → die Fälle wären in der Queue;
bei G-IE-108 ist die Begründung faktisch falsch (kein Kandidat trägt Branding). Nachprüfung: hitl-queue-analyse.md §7.

| Gold | Urteil | Referenzaussage | Was fehlt (v2-Beleg) | Unit | LLM-Begründung (wörtlich) |
| --- | --- | --- | --- | --- | --- |
| G-IE-006 | partial | Gespeichertes Kommunikationswissen soll für Nutzer jederzeit und schnell nachschlagbar sein. | CAN-007: Nachschlagen ✓, aber 'jederzeit und schnell' fehlt (Qualitäts-Qualifikation) | AU-0023 | Das Festhalten und schnelle Abrufen von Kommunikationswissen per App für neue Mitarbeitende ist bereits durch den Kernfunktions-Claim abgedeckt. |
| G-IE-014 | partial | Nutzer sollen neue Kommunikationsweisen dynamisch über einen Plus-Button im unteren Bereich der Kommunikationsseiten hinzufügen können; der Button öffnet einen Dialog zum Hinzufügen von Texten und Bildern. | CAN-023: Hinzufügen mit Texten/Bildern ✓; Plus-Button, Position unten und der (v2 ergänzte) Eingabedialog fehlen | AU-0033 | Die beschriebenen Screens und Hinzufügefunktionen sind bereits in den Kandidaten zu About Me, Kommunikation und Videofunktionalität enthalten. |
| G-IE-030 | partial | Der About-Me-Bereich soll ganz oben ein Informationsfeld mit Angaben wie Name, Alter und Hobbys enthalten. | CAN-022: 'Personendaten' ✓, aber Position oben + Informationsfeld fehlen (wie F-Seite) | AU-0125 | Infobox, Foto-Timeline, Plus-Button und neueste Fotos oben sind semantisch bereits in der About-Me-Anforderung enthalten. |
| G-IE-031 | partial | Die About-Me-Foto-Timeline soll einem Social-Media-Feed ähneln: Neu hinzugefügte Bilder erscheinen oben, ältere Bilder bleiben darunter scrollbar. | CAN-022: 'neueste oben' ✓, aber Feed-Analogie + 'ältere darunter scrollbar' fehlen (Zweitprüfer-Präzedenz F-Seite) | AU-0125 | Infobox, Foto-Timeline, Plus-Button und neueste Fotos oben sind semantisch bereits in der About-Me-Anforderung enthalten. |
| G-IE-032 | partial | Im About-Me-Bereich sollen neue Bilder dynamisch über einen Plus-Button am unteren Bildschirmrand und einen Eingabedialog hinzugefügt werden können. | CAN-022: Plus-Button ✓, aber Position unten + Eingabedialog fehlen | AU-0125 | Infobox, Foto-Timeline, Plus-Button und neueste Fotos oben sind semantisch bereits in der About-Me-Anforderung enthalten. |
| G-IE-048 | partial | Auf der Profilübersicht soll über eine Suchleiste nach dem Namen oder anderen relevanten Profildaten gesucht werden können. | CAN-030: Suchleiste ✓, aber Suchkriterien Name/Profildaten fehlen (Zweitprüfer-Präzedenz F-Seite G-048) | AU-0084 | Die beschriebene Suchfunktion für Namen und andere Details entspricht der bestehenden Anforderung an die Profilsuche. |
| G-IE-061 | partial | Ob der Hilfebereich zusätzlich erläutern soll, wie Informationen effektiv eingegeben und gesucht werden, bleibt offen. | CAN-044 (Hilfe/Tutorial) trägt den Kern Hilfebereich; die OFFENE Teilfrage Eingabe-/Such-Erläuterung fehlt | AU-0086 | Die Idee einer Hilfe-Seite oder Anleitung zur effektiven Nutzung ist bereits durch die Tutorial-/Hilfefunktion abgedeckt. |
| G-IE-108 | none | Das Design der App soll ein klares Branding aufweisen. | KEIN Claim zum klaren Branding | AU-0136 | Zugänglichkeit, intuitives Design, Branding, Hilfe-Funktionen und Benutzerfreundlichkeit sind bereits durch bestehende UI-, Hilfe- und Barrierefreiheitskandidaten abgedeckt. |

## 6 · Liste D — STILL verloren (18): Units wurden VERWENDET, es entsteht konstruktionsbedingt kein Signal

Der genannte Claim konsumierte die Quell-Unit, ließ aber das Detail fallen; die Unused-Unit-Bilanz prüft
verwendete Units nicht erneut. Kein Signal, kein Queue-Item, kein regulärer Weg — dafür bräuchte es eine
sub-unit-Deckungsprüfung (Kapitel-8-Ausblick). Enthält die beiden Kanonisierungs-Merge-Verluste (G-IE-021/023).

| Gold | Urteil | Referenzaussage | Was fehlt (v2-Beleg) | Unit → konsumierender Claim |
| --- | --- | --- | --- | --- |
| G-IE-011 | none | Der About-Me-Bereich soll einen ersten persönlichen Eindruck des Bewohners vermitteln. | KEIN Claim zum Zweck 'erster persönlicher Eindruck' (AU-0025 von CAN-012 konsumiert) | AU-0025 → CAN-012 |
| G-IE-020 | partial | Mitarbeiter und Angehörige sollen als normale User-Accounts geführt werden. | GEBÜNDELT CAN-019 (User-Rollentyp) + CAN-017 (Angehörigen-Zugriff): Substanz angerissen, explizite Zuordnung Mitarbeiter/Angehörige→User fehlt | AU-0043 → CAN-019 |
| G-IE-021 | partial | Normale User-Accounts sollen Inhalte hinzufügen können. | CAN-013: Hinzufügen-Fähigkeit ✓, aber Rollen-Bindung an normale User fehlt | AU-0043 → CAN-019; AU-0060 → CAN-019,CAN-026,CAN-027 |
| G-IE-023 | none | Normale User-Accounts sollen hinzugefügte Daten nicht löschen können. | KEIN Claim zum Lösch-Verbot für normale User | AU-0043 → CAN-019; AU-0060 → CAN-019,CAN-026,CAN-027 |
| G-IE-025 | partial | Normale User-Accounts sollen keine neuen Accounts erstellen können. | CAN-019: Admin-Anlage + keine Selbstregistrierung ✓, aber Exklusivität (User KÖNNEN NICHT anlegen) nicht ausgesagt | AU-0044 → CAN-019; AU-0060 → CAN-019,CAN-026,CAN-027 |
| G-IE-037 | none | Im Kalender sollen zu einzelnen Tagen Einträge hinzugefügt werden können. | KEIN Claim zum Hinzufügen von Einträgen an einzelnen Tagen | AU-0051 → CAN-024 |
| G-IE-038 | none | Unter der Monatsansicht des Kalenders soll eine Liste mit Medikamenten angezeigt werden. | KEIN Claim zur Medikamentenliste unter der Monatsansicht | AU-0051 → CAN-024 |
| G-IE-043 | none | Admin-Accounts sollen hinzugefügte Daten löschen können. | KEIN Claim zum Löschrecht der Admins | AU-0060 → CAN-019,CAN-026,CAN-027 |
| G-IE-066 | partial | Im Entwicklungsteam sollen für Flutter und Dart einheitliche Versionen installiert werden; im Gespräch werden die Versionsnummern 3.13.9 und 3.1.5 genannt. | CAN-037: einheitliche Versionen ✓, aber Versionsnummern 3.13.9/3.1.5 fehlen (Zahlenwert) | AU-0102 → CAN-037 |
| G-IE-069 | partial | Ob eine NoSQLite-Datenbank zur lokalen Speicherung oder Zwischenspeicherung von Firebase-Daten eingesetzt wird, bleibt offen. | CAN-038: Klärung offen ✓, aber die konkrete NoSQLite-Option fehlt | AU-0106 → CAN-038; AU-0107 → CAN-038 |
| G-IE-079 | partial | Der Login-Button soll unter den Eingabefeldern angeordnet sein. | CAN-042: Button ✓, aber Position unter den Feldern fehlt | AU-0119 → CAN-042 |
| G-IE-080 | none | Der Login-Button soll optisch hervorstechen. | KEIN Claim zum optischen Hervorstechen des Login-Buttons | AU-0119 → CAN-042 |
| G-IE-085 | partial | Die Profilübersicht soll für jedes Profil ein Vorschaubild, den Namen und eine kurze Beschreibung anzeigen; die Profile werden unterhalb der Suchleiste angezeigt. | CAN-030 nennt Suchleiste und Profil-Vorschau (Bild/Name/Beschreibung ✓), drückt die v2-Positionsrelation 'Profile unterhalb der Suchleiste' aber nicht aus (Grenzfall #2) | AU-0122 → CAN-030 |
| G-IE-086 | partial | Ob die Profile in der Profilübersicht als Liste oder als Kacheln dargestellt werden, bleibt offen. | CAN-030: 'Liste oder Kacheln' als Optionen ✓, aber der OFFENHEITS-Status der Frage fehlt | AU-0122 → CAN-030 |
| G-IE-087 | partial | Neue Profile sollen über ein Plus-Symbol am unteren Bildschirmrand und einen Dialog mit Bild, Name und Beschreibung angelegt werden können. | CAN-031: Anlegen mit Bild/Name/Beschreibung ✓, aber Plus-Symbol unten + Dialog fehlen | AU-0123 → CAN-031 |
| G-IE-099 | none | Datenschutzeinstellungen sollen leicht zugänglich sein. | KEIN Claim zu leicht zugänglichen Datenschutzeinstellungen | AU-0131 → CAN-044 |
| G-IE-105 | partial | Animationen beziehungsweise zusätzliches Feedback sollen nicht in die App aufgenommen werden. | CAN-047: Verbots-Richtung ✓ (anders als F!), aber nur 'ablenkende' statt aller Animationen — Reichweite der Negation fehlt | AU-0134 → CAN-046; AU-0135 → CAN-047 |
| G-IE-109 | partial | Als noch nicht abschließend beschlossener Ablauf wird vorgeschlagen, beim Aufruf einer Seite Daten aus Firebase zu laden und lokal zu speichern, um wiederholtes Laden zu vermeiden. | CAN-038 erfasst lokale Speicherung/Wiederverwendung als OFFENE Frage; der konkrete vorgeschlagene Ablauf 'beim Seitenaufruf laden und lokal speichern' fehlt | AU-0106 → CAN-038; AU-0107 → CAN-038 |

## 7 · Liste E — Der extrahierte Bestand (47 Claims): was das Ledger tatsächlich enthält

Gegenperspektive zu A–D: jeder finale Claim mit Text, Quell-Units, HITL-Vorlage (✓ = als
`review_required` in der Queue, dort nur Facetten-Korrektur möglich) und der Rück-Zuordnung,
welche Gold-Aussagen ihn im v2-Urteilsblatt als Beleg nennen (voll = trägt die Deckung ·
teilweise = trägt den Kern einer partial-Lücke · Lücken-Kontext = wird im Beleg einer
none-Lücke als NICHT-deckend genannt). Claims ohne Gold-Nennung sind nicht automatisch
Gold-fremd — das Urteilsblatt nennt Claims nur, wo sie für das Urteil tragend waren;
Gold-Fremdheit (gold_escapes) wurde in diesem Fall nicht erhoben (Messprotokoll §6b).
Status = Gesprächsstatus der AUSSAGE im Meeting (decided 25 · open 20 · rejected 1
[CAN-003: die verworfene Übersetzungssystem-Idee, als must_not festgehalten] ·
uncertain 1) — NICHT der Bearbeitungszustand des Claims.

| Claim | Status | HITL | Claim-Text | Quell-Units | Gold-Bezug (aus dem Urteilsblatt) |
| --- | --- | --- | --- | --- | --- |
| CAN-001 | open | — | Die Lösung soll die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen verbessern und fördern. | AU-0001 | voll: G-IE-007 |
| CAN-002 | open | — | Eine digitale Lösung, etwa als Computerprogramm oder ähnliche Software, wird bevorzugt. | AU-0001 | voll: G-IE-001 |
| CAN-003 | rejected | — | Die Lösung darf kein System sein, das individuelle Sprache direkt zwischen Bewohnern und anderen Personen automatisch übersetzt. | AU-0004, AU-0005 | voll: G-IE-002 |
| CAN-004 | decided | ✓ review_required | Es soll eine Anforderungsanalyse mit beteiligten Personen und künftigen Nutzern durchgeführt werden, um Bedürfnisse und Funktionen des Systems genauer zu erarbeiten. | AU-0004, AU-0014 | — (keine Nennung im Urteilsblatt) |
| CAN-005 | decided | ✓ review_required | Das Team soll Kennenlern- und Beobachtungstermine in einer oder mehreren Einrichtungen durchführen können, um die aktuelle Kommunikationssituation besser zu verstehen. | AU-0009 | — (keine Nennung im Urteilsblatt) |
| CAN-006 | decided | — | Wenn priorisiert werden muss, soll die App primär dabei helfen, Bewohner besser zu verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen. | AU-0011, AU-0012 | voll: G-IE-003 |
| CAN-007 | open | ✓ review_required | Die Lösung soll als App Wissen über die individuelle Kommunikationsweise einer Person festhalten, damit andere bei Verständigungsproblemen nachschauen können, was gemeint sein könnte. | AU-0012, AU-0014, AU-0015 | voll: G-IE-004, G-IE-005; teilweise: G-IE-006, G-IE-008 |
| CAN-008 | decided | — | Es existieren klassische Akten, in denen Informationen über Bewohner dokumentiert sind. | AU-0007 | — (keine Nennung im Urteilsblatt) |
| CAN-009 | uncertain | ✓ review_required | Bestehende Dokumentationen können für Mitarbeitende schwer nutzbar sein, weil sie umfangreich sind und gesuchte Informationen oft nicht schnell gefunden werden. | AU-0018, AU-0020 | — (keine Nennung im Urteilsblatt) |
| CAN-010 | open | — | Es muss geklärt werden, ob und unter welchen Datenschutzbedingungen Einsicht in Bewohnerakten möglich ist. | AU-0020 | — (keine Nennung im Urteilsblatt) |
| CAN-011 | decided | — | Neue Mitarbeitende werden derzeit durch Dokumentationen und begleitete Einarbeitung unterstützt, bis gegenseitiges Verständnis und Vertrauen aufgebaut sind. | AU-0022 | — (keine Nennung im Urteilsblatt) |
| CAN-012 | open | — | Es muss mit der Leitung geklärt werden, ob die App vollständige Dokumentation übernehmen soll; der Umfang soll begrenzt bleiben, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. | AU-0025 | voll: G-IE-010, G-IE-012; Lücken-Kontext: G-IE-011 |
| CAN-013 | open | ✓ review_required | Kommunikationswissen in der App muss dynamisch erweitert werden können; neue Erfahrungen sollen hinzugefügt werden können. | AU-0026, AU-0027 | teilweise: G-IE-021, G-IE-054 |
| CAN-014 | open | ✓ review_required | Die App soll Kommunikationsinhalte nicht nur als Text, sondern auch in anderen Darstellungsformen wie Bildern visualisieren. | AU-0027, AU-0028 | — (keine Nennung im Urteilsblatt) |
| CAN-015 | open | — | Die Datenschutzlage und die Zustimmung der Angehörigen zur Nutzung von Bildern für Testzwecke in der App müssen geklärt werden. | AU-0029, AU-0030 | — (keine Nennung im Urteilsblatt) |
| CAN-016 | decided | — | Die App soll Videos zu Kommunikationssituationen mit Beschreibung erfassen und bereitstellen können. | AU-0031, AU-0032 | voll: G-IE-053 |
| CAN-017 | decided | — | Auch Angehörige sollen Zugriff auf die App erhalten und Wissen beziehungsweise Daten beitragen können. | AU-0036, AU-0037 | voll: G-IE-016; teilweise: G-IE-017, G-IE-020 |
| CAN-018 | decided | — | Die App soll eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart eines Bewohners unbedingt vermieden werden muss. | AU-0039, AU-0040 | voll: G-IE-018; Lücken-Kontext: G-IE-041 |
| CAN-019 | decided | — | Die App soll einen Login mit zugewiesenen Accounts und verschiedenen Account-Typen unterstützen, mindestens Admin und User; Admins legen Accounts an und verwalten Rechte, während es keine Selbstregistrierung für beliebige Nutzer geben darf. | AU-0043, AU-0044, AU-0045, AU-0060 | voll: G-IE-019, G-IE-022, G-IE-024, G-IE-064; teilweise: G-IE-020, G-IE-025, G-IE-063 |
| CAN-020 | decided | — | Nach dem Login soll die App auf eine Profilübersicht mit anklickbarer Liste aller sichtbaren Profile führen. | AU-0045 | voll: G-IE-026, G-IE-027; teilweise: G-IE-028 |
| CAN-021 | decided | — | Jedes Bewohnerprofil soll mindestens die Bereiche About Me, Kommunikation und Kalender enthalten. | AU-0046 | teilweise: G-IE-089 |
| CAN-022 | decided | — | Die About-Me-Seite soll Personendaten sowie eine erweiterbare Fotoliste mit Beschreibungen enthalten; neue Bilder sollen per Plus-Button hinzugefügt und die neuesten oben angezeigt werden. | AU-0047, AU-0048 | voll: G-IE-029; teilweise: G-IE-030, G-IE-031, G-IE-032 |
| CAN-023 | decided | — | Die Kommunikationsseite soll in verbale und nonverbale Kommunikation unterteilt sein und in beiden Bereichen das Hinzufügen neuer Kommunikationsweisen mit Texten oder Bildern ermöglichen. | AU-0049 | voll: G-IE-033, G-IE-034; teilweise: G-IE-014 |
| CAN-024 | open | ✓ review_required | Die App soll einen einfachen und übersichtlichen Kalender mit Terminen und Medikamentengaben bereitstellen. | AU-0051, AU-0052 | voll: G-IE-013, G-IE-036; teilweise: G-IE-035; Lücken-Kontext: G-IE-095 |
| CAN-025 | open | — | Nach dem Login soll auf jeder Seite eine Appbar mit Rücknavigation und Logout vorhanden sein; zusätzliche Einstellungsfunktionen müssen noch konkretisiert werden. | AU-0054 | voll: G-IE-039, G-IE-040; teilweise: G-IE-082, G-IE-083 |
| CAN-026 | decided | — | Die Datenschutzthematik wird im Projekt zunächst nicht tiefgehend ausgearbeitet; eine detaillierte Behandlung wäre ein separates Folgeprojekt. | AU-0060 | voll: G-IE-042 |
| CAN-027 | decided | — | Die App ist für den internen Gebrauch der Einrichtung vorgesehen, und Mitarbeitende sollen nur die Profile der Bewohner ihrer eigenen Einrichtung sehen können. | AU-0060, AU-0063, AU-0064 | voll: G-IE-044, G-IE-045 |
| CAN-028 | open | ✓ review_required | Die App soll möglichst plattformübergreifend auf iPhone und Android laufen. | AU-0066, AU-0067 | voll: G-IE-046 |
| CAN-029 | open | — | Es muss evaluiert werden, ob die App auch auf Tablets lauffähig und sinnvoll nutzbar ist. | AU-0067, AU-0068 | voll: G-IE-047 |
| CAN-030 | open | ✓ review_required | Die Profilübersicht soll unter der Appbar eine gut sichtbare Suchleiste sowie Profile als Liste oder Kacheln mit Vorschaubild, Name und Kurzbeschreibung anzeigen, damit Profile schnell gefunden werden können. | AU-0069, AU-0070, AU-0121, AU-0122 | voll: G-IE-084; teilweise: G-IE-048, G-IE-085, G-IE-086 |
| CAN-031 | open | ✓ review_required | Auf der Profilübersicht soll es eine Funktion zum Anlegen neuer Profile mit Bild, Name und Beschreibung geben. | AU-0123 | teilweise: G-IE-087 |
| CAN-032 | open | — | Auch auf Kommunikationsseiten soll eine Suchfunktion verfügbar sein; dafür muss ein einheitliches Beschreibungsmuster für Kommunikationsbeschreibungen definiert werden, damit Einträge gezielt gefunden werden können. | AU-0070, AU-0071, AU-0072, AU-0075 | voll: G-IE-050, G-IE-051; teilweise: G-IE-049 |
| CAN-033 | decided | — | Die Videofunktionalität soll nicht als eigener Screen bestehen, sondern in die Kommunikationsseiten für verbale und nonverbale Kommunikation integriert werden. | AU-0074, AU-0075 | voll: G-IE-052 |
| CAN-034 | decided | — | Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen verbundene Nutzer derselben Einrichtung eine Popup-Benachrichtigung erhalten. | AU-0078 | voll: G-IE-055; teilweise: G-IE-054 |
| CAN-035 | decided | ✓ review_required | Es soll einen Bewohner-Account mit speziellen, stark begrenzten Rechten geben, der nur das eigene Profil sehen sowie auf About Me zugreifen und dort eigene Bilder hinzufügen kann, um Fehlbedienungen zu minimieren. | AU-0079, AU-0080, AU-0082, AU-0083 | voll: G-IE-056, G-IE-057, G-IE-058, G-IE-059, G-IE-060, G-IE-064 |
| CAN-036 | decided | — | Für die App-Entwicklung sollen Flutter und Dart verwendet werden. | AU-0093, AU-0095 | voll: G-IE-065 |
| CAN-037 | decided | ✓ review_required | Das Team soll möglichst dieselbe Entwicklungsumgebung verwenden, konkret Android Studio, und einheitliche Flutter- und Dart-Versionen installieren, um Entwicklungsprobleme zu vermeiden. | AU-0100, AU-0101, AU-0102 | teilweise: G-IE-066 |
| CAN-038 | open | — | Für die Datenhaltung wird zunächst Firebase Firestore verwendet; es muss noch geklärt werden, wie Cloud-Daten lokal gespeichert und wiederverwendet werden sollen. | AU-0104, AU-0105, AU-0106, AU-0107 | voll: G-IE-067, G-IE-068; teilweise: G-IE-069, G-IE-109 |
| CAN-039 | decided | ✓ review_required | Für die erste Entwicklung und Tests soll Android 11 als gemeinsame Zielversion verwendet werden. | AU-0111, AU-0112 | voll: G-IE-070 |
| CAN-040 | open | — | Im nächsten Teamtreffen müssen Regeln für das Programmieren, etwa Kommentarregeln, festgelegt werden. | AU-0113 | — (keine Nennung im Urteilsblatt) |
| CAN-041 | decided | — | Das JetX-Package soll im Projekt ausprobiert und als zusätzliche technische Rahmenbedingung erwogen werden. | AU-0114, AU-0115 | voll: G-IE-071 |
| CAN-042 | decided | — | Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen und keine Registrierung anbieten. | AU-0119 | voll: G-IE-024; teilweise: G-IE-072, G-IE-077, G-IE-079; Lücken-Kontext: G-IE-073 |
| CAN-043 | open | ✓ review_required | Die Einstellungsseite soll rollenabhängig differenziert sein; Admins sollen dort Nutzeraccounts hinzufügen und Rechte anpassen können, während alle Nutzer ihr Profil ändern und etwa die Sprache wählen können. | AU-0130 | voll: G-IE-062, G-IE-096, G-IE-097, G-IE-098; teilweise: G-IE-063 |
| CAN-044 | decided | ✓ review_required | Die App soll eine Hilfe-Funktion oder ein Tutorial enthalten, etwa als kurze Tour beim ersten Login und als später erneut aufrufbaren Hilfebereich. | AU-0131, AU-0132 | voll: G-IE-100, G-IE-101; teilweise: G-IE-061 |
| CAN-045 | open | ✓ review_required | Bei allen Screens soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung zu vieler Farben. | AU-0133 | voll: G-IE-102, G-IE-103, G-IE-104; Lücken-Kontext: G-IE-078 |
| CAN-046 | open | ✓ review_required | Sprachbefehle oder andere alternative Eingabemethoden für beeinträchtigte Nutzer sollen geprüft werden. | AU-0134 | voll: G-IE-106 |
| CAN-047 | decided | ✓ review_required | Ablenkende Animationen sollen nicht eingebaut werden. | AU-0135 | voll: G-IE-107; teilweise: G-IE-105 |
