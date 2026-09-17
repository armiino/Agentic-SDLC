# Z1 — Separater Verifikationsdurchgang

**Datum:** 07.09.2026 · **Prüfer:** ChatGPT/Codex, KI · **Lauf:** `20260905_164650_8189ea`

**Gesamturteil:** Die beiden Statusänderungen (V1) und alle zehn ausgewählten unveränderten Paare (V2) werden bestätigt: **12/12 Paare, 24/24 Seitenurteile ohne abweichende Einstufung**. Die fehlenden normalen User-Rechte sind im vollständigen B-Propositionsbestand geprüft und der Zusammenführung DEC-019 + DEC-020 → CAN-019 zuordenbar. Es ergibt sich aus dieser Stichprobenverifikation kein Anlass, die geprüften Urteile oder die daraus berichteten Kennzahlen zu ändern. Ein kleiner Fehler im Ergebnisbericht betrifft die behauptete Wortgleichheit von REQ-048 und CAN-030; siehe unten.

## Durchführung und Aussagegrenze

Maßstab waren der mitgelieferte Gold v2 mit 109 Aussagen sowie die Inhalts- und Bündelungsregeln aus `w2-matchregeln(3).md`, insbesondere §§1–3. Gelesen wurden alle 54 A-Propositionen und alle 47 B-Propositionen. Bewertet wurde die in den Propositionen ausgedrückte Bedeutung; Quellzitate, Facetten, interne Validierungsurteile und Miss-Signale wurden nicht als zusätzliche Claim-Inhalte angerechnet. Mehrere Propositionen innerhalb desselben Bestands durften gemeinsam decken. Die eigenen zwölf A-/B-Urteilspaare wurden schriftlich festgehalten, bevor die Einzelurteile des zu prüfenden JSON geöffnet und verglichen wurden.

**Verfahrensabweichung:** Die im Auftrag verlangte Lesereihenfolge wurde nicht vollständig eingehalten: Der Ergebnisbericht wurde bereits in der ersten Lesebatch mitgelesen. Die beiden Verlustbefunde waren zudem aus der vorangegangenen Diskussion bekannt. Es handelt sich daher um eine **separate, nicht verblindete KI-Nachprüfung**, nicht um eine verblindete oder menschliche Zweitannotation. Diese Abweichung ist zusammen mit dem Prüfergebnis zu dokumentieren; eine vollständig eingehaltene Lesereihenfolge darf nicht bescheinigt werden.

## V1 — Beide Statusänderungen

| Gold-ID | Eigenes Urteil A → B | Ergebnis | Beleg und Begründung |
| --- | --- | --- | --- |
| **G-IE-021** | full → partial | **bestätigt** | DEC-019: „User können Inhalte hinzufügen“. CAN-019 enthält Account-Typen und Admin-Rechte, jedoch kein entsprechendes User-Recht. CAN-013 nennt das Hinzufügen neuer Erfahrungen, CAN-023 neuer Kommunikationsweisen; die Bindung an normale User fehlt. CAN-043 betrifft Profiländerung/Sprache und liefert diese fachliche Hinzufügefreigabe ebenfalls nicht. Im gesamten B-Bestand keine vollständige Deckung. |
| **G-IE-023** | full → none | **bestätigt** | DEC-019: „User können Inhalte hinzufügen, aber nichts löschen“. Keine der 47 B-Propositionen enthält das Löschverbot für normale User. Allgemeine Rollenverwaltung und eingeschränkte Bewohnerrechte ersetzen die fehlende Negation nicht. `CAN-019.candidateIds` enthält exakt DEC-019 und DEC-020. |

Die Zuordnung zum **Gesamtübergang A→B** ist damit gestützt. Der beigefügte B-Capture dokumentiert gleichzeitig `qc.canonicalGate.pass = true`, Attempt 1, null Fehler und 54 referenzierte Kandidaten ohne verwaiste, unbekannte oder doppelte Kandidatenverweise. Der referenzielle Check passierte somit trotz der beiden nachgewiesenen semantischen Abdeckungsverluste. Die Dateien erlauben keine isolierte Kausalzuweisung an einen einzelnen internen Modellaufruf.

## V2 — Zehnerstichprobe in Seed-Reihenfolge

| Gold-ID | Eigenes Urteil A / B | Ergebnis | Beleg und Begründung |
| --- | --- | --- | --- |
| **G-IE-080** | none / none | **bestätigt** | DEC-047/CAN-042 nennen den Login-Button lediglich in der Liste der Screen-Bestandteile. NFR-052/CAN-045 verlangen allgemeinen ausreichenden Kontrast, keine optische Hervorhebung gerade dieses Buttons. |
| **G-IE-083** | partial / partial | **bestätigt; Grenzfall** | DEC-026/CAN-025 erwähnen neben der Appbar zusätzliche, noch auszugestaltende Einstellungen; REQ-050/CAN-043 beschreiben die Einstellungsseite. Der allgemeine Einstellungsbezug trägt die Teildeckung. Rechtes Symbol und konkreter Navigationsweg fehlen; keine Vollabdeckung. |
| **G-IE-048** | partial / partial | **bestätigt; Grenzfall** | REQ-032 + REQ-048 beziehungsweise CAN-030 nennen die Suchleiste zum Finden von Profilen. Name/Beschreibung sind als Anzeigeinhalte genannt, nicht als Suchattribute. Die Durchsuchbarkeit dieser Attribute wird nicht aus ihrer bloßen Anzeige ergänzt. |
| **G-IE-082** | partial / partial | **bestätigt** | DEC-026/CAN-025: „Appbar mit Rücknavigation und Logout“. Rücknavigation vorhanden; linke Position und nach links zeigendes Pfeilsymbol fehlen in beiden Beständen. |
| **G-IE-017** | partial / partial | **bestätigt** | REQ-017/CAN-017 erlauben Angehörigen, Daten beizutragen. „Vorab“ und der Bezug auf neue Bewohner fehlen als wesentliche Bedingungen. |
| **G-IE-044** | full / full | **bestätigt** | CON-028/CAN-027: „für den internen Gebrauch der Einrichtung“. DEC-020/CAN-019 ergänzen zugewiesene Accounts und keine beliebige Selbstregistrierung. Zusammen sind interner Einsatz und Ausschluss beliebiger Außenstehender gedeckt. |
| **G-IE-012** | full / full | **bestätigt** | OQ-012/CAN-012 verlangen begrenzten Umfang, damit der Fokus auf unterstützender Kommunikation erhalten bleibt. Die daneben offene Frage zur vollständigen Dokumentation hebt diese Vorgabe nicht auf. |
| **G-IE-041** | none / none | **bestätigt; Abgrenzungsfall** | REQ-018/CAN-018 nennen die No-Go-Seite und deren Inhalt, aber keine Funktion zum Hinzufügen neuer No-Go-Einträge. Allgemeine Kommunikations-Erweiterung und der About-Me-Plus-Button dürfen nicht zu einem No-Go-Eingabepfad zusammengesetzt werden. Es fehlt die konkrete Einfügefunktion, nicht bloß ihr Symbol. |
| **G-IE-057** | full / full | **bestätigt** | REQ-037/CAN-035 beschränken den Bewohner-Account ausdrücklich auf das eigene sichtbare Profil. Diese allgemeine Sichtbeschränkung umfasst die Profilübersicht. |
| **G-IE-003** | full / full | **bestätigt** | DEC-006/CAN-006 sind wortgleich: „primär dabei helfen, Bewohner besser zu verstehen, statt primär Bewohner beim Verstehen anderer zu unterstützen“. Die Priorisierungsbedingung ist mit der Gold-Richtungsentscheidung vereinbar. |

## Ergänzende Struktur- und Zahlenprüfung

- **Referenz/Bestände:** 109 eindeutige Gold-IDs, genau dieselben 109 IDs im Urteilsblatt; 54 eindeutige A-IDs und 47 eindeutige B-IDs.
- **Übergangstabelle:** Aus allen gespeicherten Urteilen neu berechnet und identisch mit der Berichtstabelle. A: 57 full / 29 partial / 23 none; B: 55 / 30 / 24. Strict: 57/109 → 55/109; Touched: 86/109 → 85/109. Nur G-IE-021/023 wechseln die Klasse. Diese Aggregationsprüfung ist kein erneuter semantischer Voll-Audit aller 109 Paare.
- **Seed:** Die zehn niedrigsten SHA-256-Werte von `id+'w2seed42'` unter den 107 unveränderten Paaren ergeben exakt die oben geprüfte Reihenfolge.
- **Merge-Struktur:** Sieben Zweiergruppen plus 40 Einzelzuordnungen; alle 54 Kandidaten genau einmal referenziert. Gruppen: CAN-019, 027, 030, 032, 035, 037, 038. Beide bestätigten Verluste betreffen CAN-019. „6/7 ohne Gold-Statusverlust“ ist mit der vorliegenden vollständigen Urteilsmatrix konsistent; eine darüber hinausgehende Verlustfreiheit sämtlicher Inhalte wurde nicht geprüft.
- **Signalzuordnung:** Die vorgegebene Unit-Schnittmengenregel reproduziert 18/48 zugeordnete Signale und 35 unterschiedliche B-Lücken. Die ergänzende semantische Zuordnung G-IE-028 ← AU-0124 ist durch den konkreten Hinweistext zu Profilauswahl und Detailansicht nachvollziehbar; insgesamt 36/54. Die 18 Signale verteilen sich auf sechs `already_covered_indirectly`, elf `attach_as_evidence` und ein `missing_claim`. Dies bestätigt die operationalisierte Zuordnung, nicht 36 explizite Warnungen oder erfolgreiche menschliche Behebung.
- **Stille Lücken:** Die 48 Signal-Units überschneiden sich nicht mit den 75 verwendeten A-Units. Alle Quell-IDs der nach dieser Zuordnung verbleibenden 18 stillen Gold-Lücken gehören zur verwendeten Unit-Menge. Die Erzeugungshistorie der Signale über nicht mitgelieferte Zwischenstufen wurde nicht nochmals nachverfolgt.

## Kleine Berichtskorrektur und nicht geprüfter Zusatzumfang

In Z1 §5 ist **`REQ-048≡CAN-030` als Beispiel für Wortgleichheit falsch**: CAN-030 ergänzt am Ende „damit Profile schnell gefunden werden können“ aus REQ-032. Die entsprechenden Texte sind nicht wortgleich. Dies ändert das bestätigte G-IE-048-Urteil nicht. Den Beispielsatz auf **„Bei wortgleichen Propositionen, etwa DEC-054/CAN-047 und REQ-050/CAN-043, wurde derselbe Maßstab angewandt“** begrenzen; die genannten Ersatzpaare sind wortgleich.

Der fachliche Pflichtumfang V1/V2 ist mit den oben genannten Verfahrensgrenzen bearbeitet. Gold- und Urteilsdateien wurden nicht verändert. Die optionalen vier Gold-v2-Delta-Urteilsblöcke und F-Ausgaben lagen nicht als vollständiges Prüfmaterial bei und wurden nicht zusätzlich verifiziert. Ebenso wurden kein neuer Gold-Quellenaudit und keine menschliche Zweitannotation durchgeführt.

## Identifikation der geprüften Dateien

SHA-256 der Eingangsdateien; die Upload-Suffixe gehören zu den hier tatsächlich geprüften Dateinamen.

| Datei | SHA-256 |
| --- | --- |
| PRUEFAUFTRAG-Z1-VERIFIKATION.md | `fb52b660dce9d3644d86839af04d7d7f65c3edbadb27fe74dbaedd33eb7cc238` |
| Interview-Einrichtung.w2-gold(3).json | `665a84ac80244f6b234083613ffc1ba95d101d9ea3251e130b8b1311ff11429a` |
| stressfall-A-kandidaten-54.json | `1445b81f57db90a88135064f61d2502d8831bb6c8dbb306a273c76bbef147a5f` |
| stressfall-B-final-47-LCR.json | `e2656232b0ff5856b58e244edec47c9cadea7db45977321cadb5f2e882013b9f` |
| w2-matchregeln(3).md | `611fd769d6b150d50aa3c8bbcf2ca14b25f887e2a0dd3cddfa508dae2046ad8b` |
| w2-z1-stufenanalyse-8189ea(1).json | `311b5760ea4779f776393670ecb2bf5611707d03e11891ee4af7e997dcc2b23f` |
| z1-stufenanalyse(1).md | `74606351a72490c9ceb3dc648cfaee6ff33b45ed25375ffa4a420f83b3ea27bd` |
