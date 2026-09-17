# Adjudikations-Bogen: Nachreview-Runde Kapitel 7 (Ledger 8189ea / F 6540e2)

> **Ausgefüllt am 08.09.2026  im Auftrag des Autors.** Dies sind retrospektive KI-Adjudikationsentscheidungen zu den vorgelegten Bewertungsfragen, keine unabhängige menschliche Zweitannotation und keine Ledger-HITL-Bearbeitung. Die vom Nutzer delegierte Bearbeitung wird nicht als persönliche Einzelprüfung des Autors ausgegeben. Gold, Systemausgaben und historische Urteilsdateien wurden nicht geändert.
>
> **Arbeitsstand für den Kollegen:** Alle 13 Entscheidungsfelder sind beantwortet. G-IE-105 wird im Hauptstand für beide Konfigurationen mit none bewertet; F-G-IE-089 wird auf partial gesetzt. Die übrigen hier genannten Bestätigungen und Änderungen stehen jeweils im Entscheidungsfeld. Begründungen und alternative Zahlen in den ursprünglichen Fragen sind als Fragestand erhalten; für die Überarbeitung gelten die ausgefüllten Entscheidungen und die konsolidierte Tabelle am Ende. Frühere Review-Empfehlungen werden an diesen Stellen ausdrücklich präzisiert.

> Stand: 08.09.2026. Zweck: Deine Entscheidungen zu den Reviewer-Vorschlägen, BEVOR ich mit dem
> Überarbeitungsauftrag (Auftrag-Kapitel-7-Coverage-Ledger-F.md, §11 Schritt 2) beginne.
> Regeln: Gold v2 bleibt unangetastet — hier werden nur MATCHURTEILE über dieselben Ausgaben
> neu entschieden (neuer Auswertungsstand, dokumentiert als eigene retrospektive Runde).
> Jede Entscheidung wird mit Begründung, Entscheider und Datum ins Änderungsprotokoll übernommen.
> Trage bei jeder Frage dein Verdikt in die Zeile **ENTSCHEID:** ein (Option übernehmen /
> ablehnen / eigene Variante mit Begründung).

---

## Block 1 — Die 7 Matchurteil-Vorschläge

### 1.1 Ledger G-IE-025: partial → none
Referenz: „Normale User-Accounts sollen keine neuen Accounts erstellen können."
Befund: CAN-019 sagt „Admins legen Accounts an" + keine Selbstregistrierung — das ist NICHT
das Verbot für angemeldete normale User. Matchregeln-Negationsregel: fehlende Negation → none.
Hinweis: Das gespeicherte CAN-019-ZITAT enthält die Ausschließlichkeit („nur ein Account mit
Admin-Rechten…") — zählt aber nur für die separate Evidenz-Betrachtung, nicht für P1.
Empfehlung Review: übernehmen (regelkonform).
**ENTSCHEID:** Übernehmen: **none**. CAN-019 gewährt Admins die Accountanlage und schließt Selbstregistrierung aus. Ein bereits angemeldeter User könnte damit weiterhin andere Accounts erstellen; das wird im Claim-Text nicht ausgeschlossen. Die Verbotsklausel aus AU-0060 fehlt. Nach §2 der Matchregeln ist das kein Match, ohne dass damit eine gegenteilige Erlaubnis behauptet wäre. Die ausdrückliche Ausschließlichkeit im gespeicherten Zitat ist nur in der separaten Evidenzbetrachtung anzurechnen.

### 1.2 Ledger G-IE-062: full → partial
Referenz: „Der Admin-Bereich soll nur für Admin-Accounts sichtbar sein."
Befund: CAN-043 beschreibt rollenabhängige Funktionen/Ansichten; die ausdrückliche
AUSSCHLIESSLICHE Sichtbarkeit („nur für Admins sichtbar", AU-0088) folgt daraus nicht zwingend
(sichtbar ≠ bedienbar). Achtung: F behält hier full (C-082 sagt wörtlich „nur für Admins
sichtbar") — gleiches Prüfmaß, unterschiedlicher Wortlaut, unterschiedliches Ergebnis.
Empfehlung Review: übernehmen.
**ENTSCHEID:** Übernehmen: **partial**. CAN-043 enthält die Rollenabhängigkeit und Admin-Funktionen, aber keine ausschließliche Sichtbarkeit des Admin-Bereichs. Ein sichtbarer, für User gesperrter Bereich wäre mit dem Claim vereinbar und würde die Referenz dennoch verfehlen. F-C-082 enthält dagegen ausdrücklich „nur für Admins sichtbare“ und bleibt deshalb full.

### 1.3 Ledger G-IE-086: partial → full
Referenz: „Ob die Profile als Liste oder als Kacheln dargestellt werden, bleibt offen."
Befund: CAN-030 hält „Liste oder Kacheln" ausdrücklich als Alternative fest; die Quelle
(AU-0122) drückt die Offenheit ebenfalls NUR über diese Alternative aus — eine Zusatzformel
„bleibt offen" ist kein Pflichtwortlaut. Achtung: F bleibt hier none (C-056 legt sich auf
„Liste" fest, Alternative verloren) — wieder gleiches Maß, anderes Ergebnis.
Empfehlung Review: übernehmen.
**ENTSCHEID:** Übernehmen: **full**. CAN-030 nennt „Profile als Liste oder Kacheln“. Damit bleibt dieselbe nicht aufgelöste Alternative wie in AU-0122 erhalten. Ein zusätzliches Pflichtwort „offen“ ist für den fachlichen Match nicht erforderlich; der Metadatenstatus wird für P1 nicht benötigt. F-C-056/C-100 enthalten keine Kachelalternative; F bleibt none für diese offene Frage, nicht für die Profilübersicht insgesamt.

### 1.4 Ledger G-IE-107: full → partial
Referenz: „Falls Animationen ODER ZUSÄTZLICHES FEEDBACK dennoch eingesetzt werden, dürfen sie
nicht zu ablenkend sein."
Befund: CAN-047 nennt nur Animationen; „zusätzliches Feedback" fehlt im Claim-Text.
Empfehlung Review: übernehmen (gleiche Eingrenzung wie bei F, s. 1.7).
**ENTSCHEID:** Übernehmen: **partial**. CAN-047 bewahrt die Beschränkung ablenkender Animationen. Der ebenfalls referenzierte Gegenstand „zusätzliches Feedback“ fehlt. Es geht um einen sachlich engeren Gegenstand, nicht um den fehlenden Wortlaut. Keine weitere Herabstufung wegen des Wortes „Ablenkung“: Die bedingte Begrenzung ist für Animationen erkennbar erhalten.

### 1.5 F G-IE-025: partial → none
Dieselbe Negationsregel wie 1.1, symmetrisch angewandt: C-051/053/054 (Admin-Anlage, keine
Selbstregistrierung, Zugriffsbeschränkung) ersetzen das User-Verbot nicht. Anders als beim
Ledger existiert bei F KEIN gespeichertes Zitat mit der Ausschließlichkeitsklausel.
Empfehlung Review: übernehmen. Konsistenz-Hinweis: 1.1 und 1.5 sollten gleich entschieden werden.
**ENTSCHEID:** Übernehmen: **none**. Wie bei 1.1 ersetzen C-051/053/054 die ausdrücklich geforderte Verbotsklausel für normale User nicht. Auch C-082/083 (Admin-Seite und dortige Accountanlage) schließen eine Accountanlage auf einem anderen Weg durch User nicht ausdrücklich aus. Quellen-IDs zu AU-0044/0060 liefern den Rückweg zur Quelle, aber keinen zusätzlichen P1-Inhalt. Das Urteil bedeutet weder eine gegenteilige Erlaubnis noch ein endgültiges Verschwinden aus der Quelle.

### 1.6 F G-IE-105: none → partial  ← DIE eigentliche Auslegungsfrage, siehe Block 2
Referenz: „Animationen bzw. zusätzliches Feedback sollen nicht aufgenommen werden."
Befund: C-117 („Animationen sollen, wenn überhaupt, nicht ablenkend sein") ist KEINE
Einbau-Entscheidung (alte Begründung „entgegengesetzte Entscheidung" überzogen), trägt aber
die einschränkende Richtung — vergleichbar mit CAN-047, das beim Ledger partial bekam.
Empfehlung Review: übernehmen ALS Harmonisierung — Entscheidung hängt an Block 2.
**ENTSCHEID:** Vorschlag **nicht übernehmen**; F bleibt **none**, entsprechend Option B in Block 2. C-117 enthält eine bedingte Ablenkungsgrenze, aber keine Entscheidung gegen den Einbau. Die frühere Begründung „entgegengesetzte Entscheidung“ ist trotzdem zu korrigieren: Es fehlt die Ablehnung; eine positive Einbauentscheidung wird ebenfalls nicht behauptet. Beim Ledger wird G-IE-105 aus demselben Grund von partial auf none gesetzt.

### 1.7 F G-IE-107: full → partial
Wie 1.4: C-117 betrifft nur Animationen, nicht zusätzliches Feedback.
Empfehlung Review: übernehmen. Konsistenz-Hinweis: 1.4 und 1.7 gehören zusammen.
**ENTSCHEID:** Übernehmen: **partial**. C-117 bewahrt die Ablenkungsgrenze für Animationen, aber nicht für zusätzliches Feedback. Gleiche Gegenstandsabgrenzung wie bei CAN-047 in 1.4. Diese Entscheidung ist von der fehlenden grundsätzlichen Ablehnung in G-IE-105 getrennt.

---

## Block 2 — Auslegungswahl G-IE-105 (MUSS für beide Arme gleich sein)

Die Matchregel „fehlende Negation → none" ist bei GANZ fehlendem Verbot eindeutig (Fall 025).
Bei einem EINGESCHRÄNKT erhaltenen Verbot („keine ablenkenden Animationen" statt „keine
Animationen/Feedback") sind zwei Auslegungen vertretbar:

**Option A — partial für beide (Review-Empfehlung beider Reviews):**
der erhaltene einschränkende Teil zählt als Teilerhalt.
→ Zahlen: Ledger 54/30/25 (49,5 % / 77,1 %) · F 70/23/16 (64,2 % / 85,3 %)

**Option B — none für beide (strenge Negationsregel auf jede eingeschränkte Negation):**
→ Zahlen: Ledger 54/29/26 (49,5 % / 76,1 % touched) · F 70/22/17 (64,2 % / 84,4 % touched)

In beiden Optionen: Full-Differenz F−Ledger = 14,7 Prozentpunkte; die nicht gewählte Option
wird als Sensitivität im Anhang mitberichtet. KEINE Auswahl nach günstigerer Quote — bitte
inhaltlich begründen (z. B.: „Teilerhalt der Verbotsrichtung ist bewertungsrelevant" für A,
oder „Negationsregel kennt keine Grade" für B).

**ENTSCHEID (A oder B + ein Satz Begründung):** **B: none für beide im Hauptstand.** Der in G-IE-105 bewertete Kern ist die Entscheidung gegen den Einbau; „keine ablenkenden Animationen“ beziehungsweise „wenn überhaupt, nicht ablenkend“ erhalten stattdessen die separat in G-IE-107 erfasste Ablenkungsbeschränkung. Eine unter dieser Bedingung zugelassene Animation wäre mit beiden Outputs vereinbar, aber nicht mit der im Gold ausgewiesenen Ablehnungsentscheidung. Die tatsächliche Ablehnung ist in keinem Claim-Text ausdrücklich vorhanden; auch die in der Gold-Note zugelassene Darstellung beider Seiten samt Spannung fehlt dort.

Dies präzisiert die Auslegung des bestehenden Kriteriums am Fall; es führt keine allgemeine Regel „Negationen kennen keine Grade“ ein. Option A bleibt als offengelegte alternative Teilmatch-Lesart im Anhang. **Korrektur meiner bisherigen Empfehlung:** Die frühere Teilerhalt-Begründung war vertretbar als Sensitivität, trennt die zwei Gold-Gegenstände aber weniger klar. Die Wahl von B erfolgt nach dem Aussageinhalt, nicht nach der Quote. Die oben gedruckten Zahlen der Optionen setzen noch F-089 = full voraus; nach Entscheidung 3.2 gelten die konsolidierten Zahlen am Ende dieses Bogens.

---

## Block 3 — Drei offengelegte Grenzfälle (bestätigen oder enger fassen)

### 3.1 F G-IE-008: full NUR im Verbund (C-017 + C-027 + C-028 + C-029)
Der bisherige F-Volltreffer hält nur, wenn man vier Claims als funktionale Paraphrase bündelt
(Verständnis + schneller Abruf + neue Mitarbeitende + verringerte Einarbeitung); C-028 allein
trägt ihn nicht. Matchregeln erlauben Mehr-Claim-Deckung ausdrücklich.
Optionen: **bestätigen (full, Verbund dokumentiert)** ← Review-Linie · ODER enger: partial
(Bündelung zu interpretativ). Wirkung bei Herabstufung: F-Full −1 (→ 69 bzw. 63,3 %).
**ENTSCHEID:** **Full bestätigen, ausschließlich als dokumentierter Verbund C-017 + C-027 + C-028 + C-029.** C-017 enthält die Hilfe bei Verständigungsproblemen ohne Beschränkung auf bereits vertraute Mitarbeitende; C-028 benennt neue Mitarbeitende; C-027 den schnellen Wissensabruf; C-029 die verringerte Einarbeitungszeit. Zusammen drücken sie den quellenbezogenen Zweck aus, unvertrauten Personen das Verständnis schneller zu ermöglichen. §3 erlaubt die Vereinigung mehrerer Aussagen. Hier wird eine formulierte Nutzenanforderung als funktionale Paraphrase beurteilt, keine tatsächliche Zeitwirkung nachgewiesen. C-028 allein wäre partial. Die engere Lesart partial bleibt als Sensitivität mit ihrer einzelnen Zahlenwirkung ausgewiesen.

### 3.2 F G-IE-089: full als funktionale Paraphrase (C-103)
„Vier Hauptbereiche als interaktive Buttons" — C-103 beschreibt vier interaktive Hauptbereiche
der Detailansicht ohne das Wort „Buttons". Review wertet als Paraphrase (kein Widget-Typ
gefordert). Optionen: **bestätigen (full)** ← Review-Linie · ODER enger: partial.
Wirkung bei Herabstufung: F-Full −1.
**ENTSCHEID:** **Enger fassen: partial.** C-103 erhält Anzahl, Interaktivität und Ort, aber nicht die Darstellung der Hauptbereiche als Buttons. „Interaktiv“ kann auch andere Bedienformen bezeichnen. Die Referenz und AU-0124 nennen Buttons ausdrücklich; gleichwertig wären etwa „Schaltflächen“, nicht zwingend das englische Wort. Auch die übrigen F-Aussagen liefern diese konkrete vierteilige Button-Darstellung in der Detailansicht nicht.

**Korrektur der früheren Review-Begründung:** „Kein Widget-Typ gefordert“ war zu weitgehend, weil die Quelle gerade eine Button-Darstellung vorgibt. Das fehlende Merkmal ist als Darstellung/Bediengestaltung (U) zu klassifizieren. Es fehlt damit keine komplette Profil-Detailansicht. Dieses gleiche Prüfmaß gilt auch für den Ledger; dessen G-IE-089 bleibt partial.

### 3.3 Ledger G-IE-049: Zitatlesart in der SEPARATEN Evidenz-Betrachtung
(Betrifft NICHT P1 — dort bleibt 049 partial.) In der Zusatzauswertung „Claim + gespeicherte
Evidenz": CAN-032-Zitat enthält die Suchleiste oben; das „Körperteil" ist im Gold als BEISPIEL
formuliert. Zählt der Evidenz-Match als voll (Beispiel ≠ Pflichtwort) oder bleibt er partial
(strengere Lesart)? → Zusatzkennzahl „Claim+Evidenz": **60/109 (voll-Lesart)** ← Review-Linie
oder **59/109 (strenge Lesart)**. Die nicht gewählte Lesart wird als Sensitivität ausgewiesen.
**ENTSCHEID:** **Full in der separaten Betrachtung „Claim + gespeicherte Evidenz“ bestätigen; P1 bleibt partial.** CAN-032 enthält die Suche nach Kommunikationsbeschreibungen sowie ein einheitliches Beschreibungsmuster zum gezielten Finden. Das zugehörige gespeicherte Zitat ergänzt die Suchleiste oben im Bildschirm auf Kommunikationsseiten. Der Gold-Wortlaut formuliert das Körperteil mit „wie“ als Beispiel; er verpflichtet nicht zu einem eigenen Körperteilfeld oder einem wörtlichen Fuß-Beispiel. Die Merkmals-/Beschreibungsorientierung ist im Verbund erhalten.

Damit bleibt die zusätzliche Full-Erhaltung unter den übrigen Review-Urteilen bei **60/109 = 55,0 %**; die engere Beispiel-Lesart ergibt **59/109 = 54,1 %** und wird als Sensitivität dokumentiert. Der Zugriff auf nicht gespeicherten Originalkontext wird dabei nicht mitgezählt. Keine Übertragung dieser sechs zusätzlichen Evidenztreffer in P1 und keine Addition zur Queue-Menge.

---

## Block 4 — Zwei Bestätigungen zum Vorgehen (kein Urteil, nur Abnicken/Einspruch)

### 4.1 HITL-Darstellung = faktisch „Option B"
Der Auftrag lässt die Post-HITL-Frage offen: EIN bedingtes Szenario (Q-Zuordnung, nach
Neuberechnung voraussichtlich (54+29)/109 = 76,1 %, formal nachzurechnen), kein simulierter
oder behaupteter HITL-Erfolg; die reale Adjudikations-Session bleibt als möglicher späterer
Schritt benannt (inkl. Gold-Kenntnis-Protokoll). Die frühere Obergrenzen-Treppe
(0,761→0,835→18) wird NICHT in die Thesis übernommen — die Reviews haben die 0,835-Stufe an
konkreten Fällen widerlegt (AU-0125 trägt Scrollen/Dialog nicht; AU-0024 trägt 008/009 nicht
allein). Der Design-Befund „beruhigende already_covered-Fehlklassifikation × Filter" bleibt
als Befund erhalten.
**OK / EINSPRUCH:** **OK zum einen bedingten Szenario, mit folgenden Präzisierungen.** Die Bezeichnung „Option B“ hier bezeichnet die HITL-Darstellung und ist nicht mit der Negationsauslegung in Block 2 zu verwechseln. Eine tatsächliche HITL-Session wird weder durchgeführt noch als Pflicht oder Voraussetzung für die Fertigstellung des Kapitels angekündigt.

Die Zuordnung wurde hier anhand aller 48 vorhandenen Miss-Signale, der Gold-Quellreferenzen und des bereits dokumentierten Filters nachgerechnet: Unter dem konsolidierten Ledger-Urteil ergeben sich **29** statt 28 nicht vollständig gedeckte Gold-IDs mit zugeordnetem Unit-Signal im zugelassenen Kanal. Neu hinzu kommt **G-IE-062 über AU-0088 (`attach_as_evidence`)**. Mit der beibehaltenen semantischen Ergänzung G-IE-028 → AU-0124 gilt somit rechnerisch **(54 + 29)/109 = 76,1 %**. Das bestätigt die ID-Zuordnung unter der dokumentierten Filterregel, nicht eine historische UI-Anzeige oder ausreichenden Kontext in jedem Item.

Die Annahmen bleiben zwingend: sämtlicher fehlender Gehalt wird erkannt, erforderlicher Kontext ist zugänglich, zulässige Bearbeitung stellt eine vollständige Proposition her, bisherige Volltreffer bleiben erhalten. Der Rest von 26/109 = 23,9 % ist unter diesem Szenario nicht ergänzt angesetzt, nicht grundsätzlich unerreichbar. Bereits vorhandene Claim-Reviews sind ein anderer Zugang.

**Die 0,835-Stufe entfällt als behauptete HITL-Obergrenze.** Die vorhandene **Adressierbarkeit** von (54 + 37)/109 = 83,5 % bleibt als anders definierte Kennzahl gültig: Sie zählt Claim-Volltreffer plus alle zugeordneten Hinweise, einschließlich ausgefilterter Signale. Nicht die Arithmetik ist widerlegt, sondern die Gleichsetzung mit menschlicher Behebbarkeit. Der Befund zum Zusammenspiel von already_covered-Urteilen und Queue-Filter bleibt mit seinen Einzelfallbelegen erhalten.

### 4.2 Meeting-2-Konsistenzprüfung (begrenzt)
Die Regelauslegungen aus dieser Runde (Negationsregel, 105/107-Systematik „Sammelbegriff
Animationen/Feedback") werden GEZIELT auf entsprechende meeting-2-Fälle geprüft (nur
Konsistenz, kein Voll-Review; Prüfumfang wird dokumentiert). Meeting-2-Werte ändern sich nur
bei tatsächlichem Fund.
**OK / EINSPRUCH:** **OK, gezielt und begrenzt.** Im vorhandenen Meeting-2-Gold und den zugehörigen Ausgaben nach vergleichbaren Fällen suchen: fehlende Verbots-/Ausschließlichkeitsklauseln, erhaltene Alternativen, nur teilweise erfasste Sammelgegenstände sowie konkret geforderte Bedienformen. Die Regeln sachlich übertragen, nicht nach dem Vorkommen des Wortes „Animation“ suchen. Positive wie negative bisherige Matchurteile können betroffen sein. IDs, Such-/Prüfumfang, Belege und tatsächliche Änderungen dokumentieren. Dies beauftragt keinen neuen Systemlauf und kein vollständiges Meeting-Review. Die Durchführung durch den Kollegen bleibt offen; dieser Bogen behauptet sie nicht als bereits erfolgt.

---

## Was nach deinen Antworten passiert (Reihenfolge §11 des Auftrags)

1. Entscheidungshistorie als neue datierte Bewertungsrunde dokumentieren (7.2.2 + Anhang).
2. EINE maschinenlesbare Beziehungstabelle je Gold-ID erzeugen (Lauf, Alt-/Endurteil,
   Claims, fehlender Bestandteil, Typ, Evidenzurteil, Signal, Queue-Kanal, Szenario) —
   daraus ALLE Zahlen ableiten (Full/Touched, Silent-Miss, Signalrelevanz, Adressierbarkeit,
   Queue-Zuordnung, Szenario, A→B, gepaarter Vergleich, F/U/Q/O).
3. Anhangsteil `anh:coverage-nachreview` + Korrekturen (AU-0086-Vermerk, drei
   F-Bilanzinkonsistenzen, D.7-Zeilen).
4. Haupttext 7.1–7.8 gemäß Auftrag §5, dann K3–6/Kurzfassung/Captions gezielt.

Offen zu verifizieren (mache ich, keine Entscheidung nötig): Autorennamen der drei Paper in
Auftrag §9 vor Bibliographie-Eintrag; alle Kontrollbasis-Tabellen des Auftrags werden aus den
ID-Mengen selbst nachgerechnet.


---

## Konsolidiertes Ergebnis dieser Entscheidungsrunde

Die nachfolgenden Werte wurden aus den vorhandenen vollständigen 109er-Urteilsbeständen und den oben entschiedenen Änderungen neu gezählt. Die übrigen Urteile wurden unverändert aus den Reviews übernommen; eine dritte vollständige semantische Neubewertung aller Aussagen ist damit nicht behauptet. Der Kollege kann diesen Stand in das gemeinsame maschinenlesbare Änderungsprotokoll übernehmen und die noch ausstehenden Folgeauswertungen daran anschließen.

| Konfiguration / Hauptstand | full | partial | none | Full Coverage | Touched Coverage | Nicht vollständig gedeckt |
|---|---:|---:|---:|---:|---:|---:|
| Ledger 8189ea | 54 | 29 | 26 | 49,5 % | 76,1 % | 55 |
| F 6540e2 | 69 | 23 | 17 | 63,3 % | 84,4 % | 40 |

**Full-Differenz F minus Ledger:** 15/109 = **13,8 Prozentpunkte**. Die im unbefüllten Block 2 genannten 14,7 Prozentpunkte gelten nach Herabstufung von F-G-IE-089 nicht mehr. Der direkte Coverage-Vergleich bleibt zugunsten von F; andere Ledger-Eigenschaften sind anhand ihrer eigenen Belege einzuordnen.

### Änderungen gegenüber den bisherigen Gold-v2-Matchurteilen

| Konfiguration | ID | Bisher → entschieden |
|---|---|---|
| Ledger | G-IE-025 | partial → none |
| Ledger | G-IE-062 | full → partial |
| Ledger | G-IE-086 | partial → full |
| Ledger | G-IE-105 | partial → none |
| Ledger | G-IE-107 | full → partial |
| F | G-IE-025 | partial → none |
| F | G-IE-089 | full → partial |
| F | G-IE-107 | full → partial |

Das sind **acht geänderte Matchurteile in dieser neuen Runde**: fünf beim Ledger, drei bei F. F-G-IE-105 bleibt beim alten none; die Begründung wird berichtigt. Diese acht Änderungen gehören nicht zur früheren Historie „acht Verifikationskorrekturen plus drei Pilotkorrekturen“. Die Historien getrennt datieren und zählen.

### Gepaarter Vergleich

| Ledger \ F | full | partial | none |
|---|---:|---:|---:|
| full | 49 | 2 | 3 |
| partial | 9 | 20 | 0 |
| none | 11 | 1 | 14 |

Damit: **49 bei beiden full, 20 nur bei F full, fünf nur beim Ledger full und 35 bei keinem full**. „Bei keinem full“ schließt teilweise erhaltene Inhalte ein. Es bedeutet keinen vollständigen beidseitigen Informationsverlust.

### Nachgerechnete Hinweis- und Szenariogrößen des Ledgers

Prüfbasis: die 109 Gold-Quellreferenzen und `missSignal` aus `stressfall-B-final-47-LCR.json`. Match bei mindestens einer gemeinsamen AU-ID; zusätzlich unverändert die bereits dokumentierte semantische Zuordnung G-IE-028 → AU-0124. Zugelassene Unit-Signal-Verdikte gemäß dokumentierter Queue-Regel: `attach_as_evidence`, `needs_human`, `missing_claim`. `already_covered_indirectly` bleibt für diesen Queue-Kanal ausgeschlossen. Jede Gold-ID wird einmal gezählt.

Die Rekonstruktion reproduziert zunächst den alten Stand mit 36 zugeordneten Lücken, 28 Queue-Zuordnungen und 18 relevanten Signalen. Unter den entschiedenen Urteilen ergibt sie:

| Größe | Konsolidierter Wert | Bedeutung |
|---|---:|---|
| Gold-Lücken mit irgendeinem zugeordneten Signal | 37/55 = 67,3 % | Hinweiszuordnung, keine explizite Erkennung aller fehlenden Inhalte |
| Silent-Miss nach dieser Signalregel | 18/55 = 32,7 % | Kein Signal zugeordnet; andere Zugänge nicht ausgeschlossen |
| Lückenrelevante Signale | 18/48 = 37,5 % | Ein Signal kann mehrere Lücken betreffen |
| Adressierbarkeit | (54 + 37)/109 = 83,5 % | Bestehende Volltreffer plus Hinweiszuordnung |
| Gold-Lücken mit zugeordnetem zugelassenem Unit-Signal | 29/55 = 52,7 % | 13 partial + 16 none; Zuordnung unter dem dokumentierten Filter |
| Bedingtes HITL-Szenario | (54 + 29)/109 = 76,1 % | Nur unter vollständiger erfolgreicher Ergänzung dieser 29 Gold-IDs und Erhalt bisheriger Volltreffer |
| Im Szenario nicht vollständig ergänzt angesetzt | 26/109 = 23,9 % | Keine pauschale Aussage über dauerhafte Unauffindbarkeit oder Schwere |

Die 37 Hinweiszuordnungen bestehen aus 36 direkten Unit-Zuordnungen und einer semantischen Ergänzung. Ohne diese Ergänzung wären es 36 zugeordnete Lücken, 28 Queue-Zuordnungen und ein Szenariowert von 82/109 = 75,2 %. Die Zahl relevanter Signale bleibt 18, weil AU-0124 auch direkte Gold-Bezüge besitzt. Diese bereits vorgesehene Zuordnungssensitivität ist von einer Änderung der Matchurteile zu unterscheiden.

Neue Queue-Zuordnung gegenüber dem Altstand: ausschließlich **G-IE-062 → AU-0088**. Die Herabstufung von G-IE-107 erzeugt eine Lücke ohne Miss-Signal-Zuordnung; CAN-047 hat jedoch einen Claim-Review-Kontext. G-IE-086 entfällt als Lücke ohne Miss-Signal-Zuordnung. Die Änderung von G-IE-105 betrifft partial/none und damit weder die Menge nicht vollständig gedeckter IDs noch den Unit-Signal-Zähler.

**Zwei verschiedene 76,1-%-Werte nicht verwechseln:** Ledger-Touched zählt 54 full + 29 partial. Das HITL-Szenario zählt 54 full + 29 mit Unit-Signal verknüpfte Lücken. Die beiden 29er-Mengen sind nicht identisch: Die zweite enthält 13 partial und 16 none. Die gleiche Quote ist hier ein zahlenmäßiges Zusammentreffen, keine Identität der Metriken.

Dies ist eine überprüfte Zuordnungsrechnung unter der überlieferten Filterregel, kein unabhängiger Audit der historischen Oberfläche oder des Apply-Pfads. Queue-Erzeugung/Adapterstand weiterhin mit den Originalartefakten des Kollegen belegen; keine tatsächlich erfolgte menschliche Inhaltsänderung berichten. F hat unter dem bestehenden `unresolved`-Kanal weiterhin keine zugeordneten Meldungen; 40/40 = 100 % Silent-Miss gilt nur für diese Operationalisierung, nicht als Beweis fehlender menschlicher Auffindbarkeit.

### Sensitivitäten: jeweils nur eine Entscheidung ändern

Alle Zeilen beziehen sich auf den oben entschiedenen Hauptstand; Alternativen nicht unbemerkt miteinander kombinieren.

| Alternative | Wirkung |
|---|---|
| G-IE-105 partial statt none bei beiden | Ledger 54/30/25, Touched 77,1 %; F 69/24/16, Touched 85,3 %. Full, Lückenmengen und HITL-Szenario unverändert. |
| F-G-IE-008 partial statt full | F 68/24/17; Full 62,4 %, Touched 84,4 %. |
| F-G-IE-089 full statt partial | F 70/22/17; Full 64,2 %, Touched 84,4 %. |
| Ledger-G-IE-049 partial statt full im zusätzlichen Evidenzumfang | Full im Umfang Claim + gespeicherte Evidenz 59/109 statt 60/109; P1 und Unit-Signal-Szenario unverändert. |

Diese Alternativen quantifizieren offengelegte Auslegungsabhängigkeit. Sie sind keine statistischen Konfidenzintervalle und kein weiterer Systemversuch. Im Haupttext genügt ein kurzer Verweis auf die entscheidungsbezogene Sensitivität im Anhang.

### Verbleibende Ausführung durch den Kollegen

Die inhaltlichen Entscheidungsfragen dieses Bogens sind beantwortet. Noch auszuführen sind der gezielte Meeting-2-Abgleich, die konsistente A→B-Nachbewertung unter den hier gewählten Auslegungen, die Ableitung der aktualisierten Fehlertypentabellen, die Übernahme in den zentralen Auswertungsstand sowie Text-, Anhangs- und Buildänderungen. Die Literaturprüfung aus dem Bogen bleibt eine bibliographische Prüfung; sie ist keine Voraussetzung für diese quellen- und regelbasierten Einzelentscheidungen. Es bleibt bei vorhandenen Systemausgaben und einem ausdrücklich hypothetischen HITL-Szenario.
