# P10-3b / B-47: Versionierte Nachauswertung der Herkunft

Stand: 13.09.2026. **Audit v4 umgesetzt, geprüft und auf dem unveränderten historischen Bestand reproduziert. B-47 bleibt für den zusammenhängenden Textnachzug offen.** Zugehörig zum bestehenden [Phase-10-Plan](./Phase-10-Gesamtabnahme-2026-09-13.md). Keine Produktionsänderung, kein Modelllauf und noch kein TeX-/Overleaf-Sync.

## 1. Bedeutung für das Ziel der Arbeit

Die vorhandene Nachvollziehbarkeit lässt sich konkreter belegen als durch die alte 137/38/62-Verteilung. **Alle 104 gespeicherten Ledger-Herkunftskanten sind im ausdrücklich bezeichneten Ursprungskontext auflösbar.** Die zusätzlich untersuchten 158 Unit-Verweise der 71 eindeutig aufgelösten Claim-Definitionen führen zu vorhandenen Units; deren normalisierte Texte sind im jeweils gebundenen Transkript auffindbar. Das stützt den ursprünglichen Ledger-Gedanken: Eine referenzierte Aussage kann zu ihrem Quellenmaterial zurückverfolgt werden.

Auch weitere Eingänge sind belegbar: Bei **39 Core-Items** ist ein konkretes Eingangsobjekt identifiziert, bei **37 davon** einschließlich Plan-/Apply-Zuordnung zur Core-ID. Zwei ältere Mini-Meeting-Items haben nur den Metadaten-/Delta-Zugang. Autor-, GitHub- und Analyst-Eingänge benötigen dabei keine künstliche Ledger-Claim-Definition.

**Das ist kein Nachweis vollständiger Herkunft aller aktuellen Inhalte.** Bei 33 Items bleiben insgesamt 64 aktuell eingetragene Einzelreferenzen im vorgesehenen Kontext unaufgelöst. Ein vorhandener anderer Herkunftsweg verdeckt diese Referenzen nicht. Bei 38 Items mit gespeicherter Version > 1 werden aktuelle Änderungsbelege gesondert betrachtet. Der alte Ursprung erklärt nicht automatisch die neueste Fassung.

Die Ergebnisse belegen technische Quellenzugänge und bestimmte Übernahmeketten. Sie messen weder einen kausalen Precision-/Recall-Gewinn noch die fachliche Richtigkeit sämtlicher Ableitungen, Relationstypen oder menschlicher Entscheidungen.

## 2. Verfahren und Absicherung

Die Nachauswertung ist eine **nachträgliche Verbesserung des Auswertungsinstruments**, motiviert durch P10-1. Sie ist kein rückwirkend als vorab geplant dargestelltes W2-Experiment. Der vor dem ersten Gesamtlauf angelegte [Prüfvertrag](./P10-3b-Herkunftsaudit-v4-2026-09-13/pruefvertrag.md) trennt Definition/Eingang/Beitrag, Einzelreferenzen, Änderungsbeleg und Zitat-/Unit-Prüfung. Die [Verfahrensentwicklung](./P10-3b-Herkunftsaudit-v4-2026-09-13/verfahrensentwicklung.md) dokumentiert die nach den Probeläufen präzisierten Regeln. Keine Regel enthält besondere Erfolgsbehandlungen für einzelne Core-IDs.

Der Resolver verfolgt konkrete Run-/Dateiverknüpfungen, typisierte Eingänge und gerichtete Beziehungen. Eine globale Suche nach gleichlautenden IDs oder ähnlichen Texten ist keine Erfolgsregel. Ein eigener früherer Artefaktstand wird als Dokumentzugang ausgewiesen und aus dem engeren Referenz-/Eingangsmaß ausgeschlossen. Kandidat, menschliche Entscheidung und freigegebenes PBI werden nicht allein wegen gemeinsamer Kennungen gleichgesetzt. Mehrdeutige Definitionen auf derselben maßgeblichen Stufe bleiben mehrdeutig.

**Prüfung:** 34 künstliche positive und negative Kontrollen bestanden. Zwei Gesamtläufe erzeugten byteidentische Ergebnis- und Manifestdateien. 414 gelesene Originaldateien blieben unverändert. 1.617 ausgegebene JSON-Fundstellen und 677 eingebettete Objektvorkommen wurden gegen die Originale geprüft; diese Zählungen enthalten Wiederholungen und sind keine Anzahl unabhängiger Fälle. Acht konkrete Akzeptanzfälle greifen die frühere Diagnose auf. Sie bilden keine neue zufällige oder unabhängige Inhaltsannotation. Keine JSON-Lesefehler und keine mehrdeutigen Eingangs-IDs wurden im untersuchten Bestand festgestellt.

| Grundlage | SHA-256 |
|---|---|
| Historischer Core, unverändert | `afcba4bac58ba82fb30b4a413d977f6474be384a377765c0bdb0dafc7bcba22c` |
| Historisches Audit v3, unverändert | `0a540e2fc9bda50afe12ee589b7f488914a6ceb670b9fdcdd093c579b6298d04` |
| Separater Resolver v4 | `f8188d72f4cd1603e2eb9449557bfc65e4e054cd238f6b16e48afdac9b33f86f` |
| Ergebnis v4 | `24427eebbae088fa994abc8b4b10540e95f27b4fc57f77b5f50fd2e4c15df606` |

[Ausführung und Reproduktion](./P10-3b-Herkunftsaudit-v4-2026-09-13/README.md) · [Einzelergebnisse](./P10-3b-Herkunftsaudit-v4-2026-09-13/Einzelfaelle.md) · [Maschinelle Ergebnisse](./P10-3b-Herkunftsaudit-v4-2026-09-13/final-result/audit.json) · [Prüfbestätigung](./P10-3b-Herkunftsaudit-v4-2026-09-13/verification.json).

## 3. Ergebnisse getrennt nach Aussage

### 3.1 Definition, Eingang und Beitrag

| Frage | Ergebnis | Zulässige Lesart |
|---|---:|---|
| Mindestens eine aktuelle Referenz führt zu einer Definition oder einem exakt zugeordneten Eingangsobjekt | 147/237 Items | Mindestens ein direkter Bezug; nicht alle Referenzen oder Inhaltsbestandteile. |
| Ein Eingangsobjekt ist zur Core-ID identifiziert | 39/237 Items | 37 mit Plan/Apply, zwei nur über Metadaten und Mini-Delta. Diese Menge überschneidet sich mit der vorherigen. |
| Direkter Referenz-/Eingangsweg oder gerichteter Beitrag zu einem solchen Ziel | 234/237 Items | Der indirekte Beitrag kann nur einen Teil des Items erklären. Keine vollständige Ursprungs- oder Änderungsabdeckung. |
| Mindestens ein Erzeugungsartefakt ist auffindbar | 147/237 Items | Dokumentzugang; kann eine frühere Fassung sein und zählt allein nicht als vorgelagerte Quelle. |
| Eigener Erzeugungsbeleg ist der verbleibende Zugang im engeren Maß | 3 Items | ARCH-40, REQ-08, REQ-32; siehe unten. |

Die Gesamtzahl 237, die im JSON für die Vereinigung einschließlich eigener Erzeugungsartefakte steht, bedeutet deshalb **nicht „237 vollständig nachgewiesene Herkünfte“**. Die drei Items ARCH-40, REQ-08 und REQ-32 werden im engeren Maß nicht positiv gerechnet. ARCH-40 besitzt den in P10-1 manuell verifizierten Textmarker, aber keine strukturierte Claim-Angabe. Bei REQ-08/32 zeigt der aktuelle Quellenlauf noch auf die frühere Erzeugung; die historisch gefundenen Änderungsoperationen sind nicht über eine ausreichende explizite Run-Kette erschlossen. Ihre Diagnose wird durch die neue Auswertung nicht rückwirkend aufgehoben.

| Alte v3-Kategorie | Items | v4: Referenz-/Eingangs-/Beitragspfad | Davon mit mindestens einer weiterhin offenen aktuellen Einzelreferenz |
|---|---:|---:|---:|
| Bis Zitat | 137 | 137 | 1 |
| Bis Definitionsobjekt | 38 | 38 | 2 |
| Offen | 62 | 59 | 30 |
| Gesamt | 237 | 234 | 33 |

**Die Endpunkte sind verändert.** Diese Gegenüberstellung ist keine neue Zitatquote und kein Systemleistungsgewinn zwischen zwei Systemständen. Die zusätzlichen Wege bestanden schon; das Instrument berücksichtigt sie nun. Die alte 137/38/62-Verteilung bleibt reproduzierbar, benötigt aber eine präzisere Interpretation und einen transparenten Nachtrag.

### 3.2 Weitere Eingänge

| Identifiziertes Eingangsobjekt | Core-Items |
|---|---:|
| Autor-/Steward-Eingabe | 15 |
| Analyst-Eingabe | 9 |
| GitHub-Eingabe | 4 |
| Eingang mit Ledger-/Ableitungsbezug | 6 |
| Vorbereitetes Delta einschließlich älterer Mini-Meetings | 5 |
| Gesamt | 39 |

Diese Einordnung folgt dem **tatsächlich gefundenen Eingangsobjekt**, nicht allein dem gespeicherten `origin`-Label. Sie ist keine vollständige Inventur aller Aufrufe oder möglichen Eingangspfade. Ein Analyst-Ergebnis ist eine maschinelle Analyse, ein Autor-Delta eine dokumentierte Eingabe; das Audit bestätigt nicht automatisch ihre vorgelagerten Aussagen. Die 15 späteren Autor-Eingänge sind insbesondere nicht mit den 15 älteren `HUMAN_ACCEPTED_OPEN_WORLD`-Items gleichzusetzen.

### 3.3 Einzelreferenzen, Ledger-Kanten und Quellenmaterial

Von **232 aktuellen Referenzvorkommen** sind 156 als Definition, elf über einen exakt zugeordneten Eingang und eines als interner Decision-Bezug aufgelöst; **64 bleiben unaufgelöst**, verteilt auf 33 Items. Gezählt wird eine Referenz je Item/Feld/Kennung, nicht die Zahl global verschiedener Claim-Namen. Die 64 offenen Vorkommen betreffen Claim-Felder. Gerade ältere PBI-Referenzen scheitern an der dokumentierten Bootstrap-/Run-Kette; zusätzlich gibt es ungeklärte Referenzen aus vorbereiteten Deltas und historischen Änderungen. Verfügbarkeit an anderer Stelle ersetzt keinen belegten Laufkontext.

Alle **104 `evidenced_by_ledger_claim`-Kanten** sind über die ausdrückliche `produced_by`-Angabe im Herkunftsregister auflösbar. Das ist ein Befund über **den Ursprungskontext dieser Kanten**. Bei fünf Items weichen ihre Zielmengen von den aktuellen Claim-Feldern ab: ARCH-11, REQ-05, REQ-08, REQ-32 und REQ-42. Die Regel lautet „vorhandene Kanten und Mengenungleichheit“; eine zusätzliche Versionsbedingung wird nicht verwendet. Keine semantische Fehlerquote.

Bei 71 eindeutig aufgelösten Claim-Definitionen aus aktuellen/historischen Referenzen und Herkunftskanten wurden 173 Evidenzzitate geprüft:

| Zitatstatus | Vorkommen |
|---|---:|
| Vollständig normalisiert im gebundenen Transkript vorhanden | 52 |
| Nur Körper nach frühem Doppelpunkt passend; vollständiger Vorspann passt nicht | 104 |
| Kein vollständiger normalisierter Treffer | 14 |
| Unter 40 normalisierten Zeichen | 3 |

Die 104 Körper-Treffer dürfen **nicht pauschal als falsche Sprecherzuordnungen** bezeichnet werden: Abweichende Sprecher-/Formatnotation kann ebenfalls dazu führen. Sie sind aber auch keine vollständigen wörtlichen Treffer einschließlich Vorspann. Die in P10-1 bestätigte falsche Sprecherangabe bleibt ein konkreter Einzelbefund. Zitate mit Auslassungen können fachlich zulässig sein, bestehen jedoch diesen bewusst strikten vollständigen Textvergleich nicht. Diese Zitatvorkommen sind eine andere Zähleinheit als die 137 früher positiv klassifizierten Core-Items.

Die 158 eingetragenen Unit-Verweise dieser Claim-Definitionen sind sämtlich im zugeordneten Ledger auflösbar; die Unit-Texte sind im gebundenen Transkript auffindbar. **Ob jede Unit die gesamte Proposition semantisch trägt, wurde nicht neu annotiert.**

### 3.4 Änderungen

38 Items besitzen eine gespeicherte Version > 1. Für ihren jeweils jüngsten gespeicherten Übergang findet v4:

- **22:** einen Plan-/Apply-Beleg und Übereinstimmung des primär verglichenen Textfelds: Ingest-Statement beziehungsweise PBI-Titel.
- **2:** einen PBI-Plan-/Apply-Beleg ohne Übereinstimmung dieses Titels. Bei PBI-016 stimmen die Akzeptanzkriterien überein; bei PBI-049 zusätzlich die Zielbeschreibung. Der Befund ist damit kein vollständiges Fehlen eines übernommenen Inhalts, aber auch keine vollständige Gleichheit aller Planfelder.
- **14:** keinen solchen aktuellen Plan-/Apply-Anschluss aus den unterstützten expliziten Run-Ankern. Darunter sieben Decision-Items, fünf Requirements und zwei PBIs. Bei neun dieser 14 ist der gespeicherte Haupttext unverändert; Versionierung kann also andere Zustandsänderungen betreffen. Daraus folgt weder, dass 14 Quellen verloren sind, noch dass 14 inhaltliche Änderungen fehlerhaft ausgeführt wurden.

Das Instrument deckt hier gezielt Ingest- und PBI-Änderungsberichte ab. Es erschließt weder sämtliche Decision-Auflösungsberichte über beliebige Querbezüge noch die vollständige Folge aller Status-/Payloadänderungen, Bestätigungen und Archiv-Snapshots. Ein kontrolliertes „kein Anschluss nach diesen Regeln“ bleibt ein Auswertungsbefund. P10-3a belegt daneben konkrete aktuelle Grenzen einzelner Writer; diese beiden Ebenen bleiben getrennt.

## 4. Durchgehendes Beispiel und Forschungsbezug

Der reale Besuchsfall eignet sich für die Darstellung: **REQ-82/83 führen zum Ledger und zum Transkript; PBI-047 verbindet REQ-83 mit späteren Ergänzungen REQ-89/90 aus GitHub.** Seine jüngste Änderung ist über PBI-Plan und Übernahmebericht verknüpft. So lässt sich zeigen, warum der ursprüngliche Transkriptbezug allein die später hinzugefügte 14-Tage-Frist und visuelle Kennzeichnung nicht erklärt. [Konkrete Fundstellen und Darstellungsskizze](./P10-3b-Herkunftsaudit-v4-2026-09-13/Besuchsbeispiel.md).

REQ-42 bleibt ein separater Verfeinerungsfall: Der aktuelle Eingang AF-1 ist über Plan/Apply zugeordnet; die ursprüngliche Ledger-Quelle ist in History und Herkunftskante erhalten. Das ist nachvollziehbare Fortschreibung mit unterschiedlichen Belegorten, keine Notwendigkeit, ein Diktat nachträglich zu einem Transkript-Claim umzudeuten.

Für die Forschungsantwort trägt das Ergebnis die bewusste Aufgabenverteilung: Modelle erschließen Inhalte; Anwendungsschritte erzeugen und verarbeiten explizite Verbindungen; Workflows und Freigaben ordnen Übernahmen; Archiv und Historie ermöglichen nachträgliche Prüfung. Die MAF-spezifische Realisierung dieser Aufgabenverteilung wird durch Code und Kapitel 6 belegt. Diese Auswertung isoliert keinen kausalen Vorteil von MAF gegenüber anderen Frameworks und keinen Vorteil gegenüber freier Extraktion.

## 5. Konsequenzen und nächste Schritte

**B-47: Auswertungsimplementierung und Nachauswertung abgeschlossen; Textfolge offen.** Kapitel 7 muss Prüfgegenstand, Endpunkte, Zitatstatus und Grenzen präzisieren; die Verbindung zur Forschungsantwort gehört in Kapitel 8. Kapitel 5 benötigt höchstens eine knappe gemeinsame Erklärung des Herkunftsprinzips, keine vollständige Dokumentation aller Parser oder Dateien. Einleitung/Kurzfassung und Anhang werden nur dort nachgezogen, wo ihre Aussagen betroffen sind.

**B-51 bleibt gesondert zu entscheiden.** Ein bestehender Archivbezug mit umständlicher Rekonstruktion erzwingt keinen Codefix. Ein tatsächlich unzureichend auffindbarer Beleg einer zentralen unterstützten Änderung kann eine gezielte Verbesserung begründen. Die generelle Reihenfolge „erst sämtliche Writer ändern, dann auswerten“ ist durch dieses Paket ersetzt; eine pauschale Herkunftsfreigabe folgt daraus nicht. Keine historischen Daten bereinigen und keine zusätzliche Vollannotation oder Modellkampagne allein für günstigere Zahlen beginnen.

Als Nächstes: die konkreten Textfolgen aus diesem Paket zusammenhängend abgrenzen und den verbleibenden B-51-Entscheid mit dem Autor besprechen; danach P10-2 zu Ledger/F, Modellbedingungen und MCR fortsetzen. Belegpaket B-46 und Materialstatus B-50 bleiben eigenständige Abschlussaufgaben. Dieses Paket ersetzt sie nicht.

**Geändert:** separater Audit-/Belegordner, dieser Bericht und die drei bestehenden Planungsdateien. Produktionscode, alter Resolver, historischer Core, altes Audit, TeX, PDF und Draw.io unverändert. **Noch keine Dateien für Overleaf zu synchronisieren.**
