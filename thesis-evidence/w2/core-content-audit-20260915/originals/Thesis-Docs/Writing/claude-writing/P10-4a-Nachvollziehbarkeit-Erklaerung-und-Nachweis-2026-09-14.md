# P10-4a: Nachvollziehbarkeit erklären, am Besuchsfall zeigen und belegen

Stand: 14.09.2026. **Lokal umgesetzt und geprüft; Overleaf-Sync und PDF-Abnahme stehen aus.** [Umsetzung und Sync-Liste](./P10-4a-Umsetzung-2026-09-14.md). Teil von P10-4/B-47 im [Phase-10-Abschlussplan](./Phase-10-Gesamtabnahme-2026-09-13.md), keine zusätzliche Reviewserie. Reihenfolge: dieses Paket einschließlich PDF-Abnahme, danach P10-2 zu Ledger/F, Modellbedingungen und Maker–Checker–Repair. Die bekannte B.1-Korrektur bleibt B-06/B-12 im abschließenden Grafikpaket und blockiert die Textarbeit nicht.

## 1. Ziel, Problem und Umfang

Ein Leser soll ohne Codekenntnis erklären können, **wie unterschiedlich eingehende Informationen zu kontrolliert übernommenen, miteinander verbundenen Projektinhalten werden, wie dieses Netz weiterwächst und welche Nachvollziehbarkeit tatsächlich nachgewiesen ist**. Die Erklärung trägt insbesondere A3, A5, A7 und A8; A6 erklärt die Freigabegrenze. Die bisherige A-/P-Zuordnung wird nicht neu erfunden.

Die Thesis enthält die nötigen Bausteine bereits: Gesamtarchitektur, Ledger, Core-Objekte, Einordnung, Governance, Fortschreibung und den realen Besuchsfall. Die Leserführung lässt den Zusammenhang ihrer Beiträge zur Entstehung des Netzes jedoch teilweise offen. Zudem beschreibt Kapitel 7 noch Audit v3, während die differenzierte Nachauswertung v4 vorliegt. Es gibt also zwei zusammenhängende Aufgaben: den Mechanismus verständlicher darstellen und seine empirische Reichweite korrekt berichten.

**Leitgedanke für die Redaktion:** Unterschiedliche Eingänge werden passend aufbereitet. Im regulären Betrieb werden ihre Inhalte gegen den bestehenden Core eingeordnet. Modelle schlagen fachliche Änderungen und Zuordnungen vor; Menschen entscheiden über deren Übernahme; die jeweiligen deterministischen Anwendungsschritte erzeugen den nächsten Zustand einschließlich der vorgesehenen Verbindungen. Strukturprüfungen, Historie und archivierte Laufartefakte unterstützen dessen Kontrolle. Die Herkunftsdetails und ihre Absicherung sind dabei nicht für alle Eingänge identisch oder vollständig.

**Autorenpräzisierung zur Bedienung und gemeinsamen Verarbeitung:** Zugangsform und fachlichen Eingang unterscheiden. Direktzugang und Steward sind Arten, vorhandene Systemfähigkeiten zu bedienen; Transkript, dialogische Eingabe und GitHub-Rückmeldung sind fachliche Eingänge. Der Steward interpretiert Anliegen, wählt passende Fähigkeiten und vermittelt Entscheidungen (bestehende Erklärung in 5.8/6.6). Er besitzt keinen eigenen fachlichen Schreibweg. Die Darstellung erklärt, wo die Aufbereitung unterschiedlich ist und ab dem Delta im regulären Fortschreibungsbetrieb die vorgesehene gemeinsame Einordnung mit fachabhängigen Zweigen sowie Prüf-, Freigabe- und Anwendungspfaden anschließt. Besondere Wiedereintritte bleiben ausgenommen. Als Architekturbeitrag herausstellen: flexible Zugänge nutzen dieselben vorgesehenen fachlichen Kontrollmechanismen und denselben Core. Das belegt keine gemessene Verbesserung der Bedienqualität und keine identische Herkunftssicherung sämtlicher Pfade. Diese Präzisierung gehört in das bestehende Wort-/Grafikbudget; kein zusätzlicher Bedienungsleitfaden.

Der Zusatz bleibt eine Erklärung der forschungsrelevanten Architektur. Kein vollständiges Datenmodell, kein Werkzeugkatalog, kein neuer generischer Herkunftsvertrag und keine Beschreibung aller Workflowknoten. Vorhandene Absätze werden umgeordnet oder ersetzt, bevor neue hinzukommen. Der ursprüngliche Orientierungsrahmen lag bei etwa 400–650 zusätzlichen Wörtern netto und einer kleinen Grafik. Die anschließend gemeinsam abgestimmte Durchlaufdarstellung erweitert diesen Umfang; der aktuelle Zuwachs ist im Umsetzungsbericht ausgewiesen. Maßgeblich bleibt die verständliche Erklärung der ausgewählten Mechanismen, kein starres Seitenziel. Die wissenschaftlich nötigen Ergebnisse des Audits erhalten in Kapitel 7 und im Anhang eigenen Platz.

**B-51-Entscheidung:** Der vorgeschlagene Bestätigungszusatz in `core.provenance` wird vorerst zurückgestellt. Dieses Paket ändert keinen Produktionscode, keine Prompts und keine historischen Daten. Die technische Möglichkeit bleibt in der [früheren Entscheidungsvorlage](./B46-PDF-Abnahme-und-B51-Entscheidungsvorlage-2026-09-13.md) erhalten. Textpräzisierung behebt eine Zusage-Wirkungs-Differenz, aber nicht die verbleibenden technischen Grenzen von Bestätigung, Versionszuordnung oder Quellenprüfung. B-51 wird deshalb nicht als technisch behoben geschlossen.

## 2. Ausgangsstand und Belegbasis

Die am 14.09. für diesen Plan geprüfte PDF hat 121 physische Seiten, 1.214.236 Bytes und SHA-256 `3af56690dad0b8e353964cbd1137e27cb2dd7c42f4ccfdde6b773b4a7c28471a`. Die relevanten Architekturseiten wurden gelesen und visuell geprüft; das ist keine neue Gesamtabnahme und keine automatische Schließung des noch separat geführten B-46-Tabellenanschlusses.

Maßgeblich sind die Live-TeX-Dateien, die [Prüfung der aktuellen Herkunftssicherung](./P10-3a-Aktuelle-Herkunftssicherung-2026-09-13.md), [Audit v4 mit Verfahren und Ergebnissen](./P10-3b-Herkunftsaudit-v4-2026-09-13.md) sowie dessen [Besuchsbeispiel mit Originalfundstellen](./P10-3b-Herkunftsaudit-v4-2026-09-13/Besuchsbeispiel.md). Der historische Core hat SHA-256 `afcba4bac58ba82fb30b4a413d977f6474be384a377765c0bdb0dafc7bcba22c`. Vor der Umsetzung die betroffenen Dateien erneut identifizieren und Änderungen seit diesen Prüfungen berücksichtigen. Ein alter Git-Commit allein beschreibt den lokal geänderten Codebestand nicht.

## 3. Reihenfolge und Arbeitsergebnisse

### Schritt 1: Aussage- und Belegabgleich vor dem Schreiben

- [x] Alle Live-TeX über die Kapitel einschließlich Einleitung, Kurzfassung, Tabellen und Anhänge nach Herkunftsversprechen, Bestätigung, Quellenlauf, Audit-v3-Zahlen und betroffenen Labels durchsuchen. Kommentare und archivierte Fassungen getrennt behandeln.
- [x] Für jeden unten vorgesehenen Mechanismus die konkrete Code-/Artefaktstelle und seine Grenze in einer kurzen Arbeitsmatrix notieren. Kein neuer Voll-Audit des Systems.
- [x] Für jeden Pfeil der neuen Grafik den historischen Ausgangs-/Folgezustand und den erzeugenden Schritt belegen. Aktueller Core und Writer-Label allein beweisen keinen damaligen Schreibzeitpunkt.

**Ergebnis:** Kleine belegte Schreibgrundlage mit Aussage, Fundstelle, zulässiger Formulierung und Grenze. Fehlende Belege führen zu einer engeren Darstellung oder gezielter Prüfung, nicht zu erfundenen Verbindungen.

### Schritt 2: Kapitel 5 als zusammenhängende Erklärung bearbeiten

| Abschnitt / lokale Datei | Konkrete Änderung | Funktion und Grenze |
|---|---|---|
| 5.2 — `chap5/chap5.2/chap5.2.tex` | Den Absatz zur Eingangskonvergenz präzisieren: unterschiedliche Aufbereitung → strukturierter Eingang **Delta** → Einordnung gegen den Bestand → konkreter Operationsplan. Delta vor seiner Verwendung erklären. Zugangsformen (Direktzugang/Steward) von fachlichen Eingängen unterscheiden; den Beitrag des Stewards mit Verweis auf 5.8 erklären. Eingangstabelle nur bei tatsächlichem Erklärbedarf anpassen. | Delta ist noch keine autorisierte Änderung. Gemeinsame Verarbeitung im Fortschreibungsbetrieb, mit fachabhängiger Verzweigung; nicht jeder Weg durchläuft den Ledger. Bootstrap und besondere Wiedereintritte bleiben getrennt. |
| 5.5 — `chap5/chap5.5/chap5.5.tex` | Vorhandene Erklärung behutsam ergänzen und nur nötigenfalls lokal umordnen: Gegenstand des Core → Nachschlagen und Einordnen → Vorschlag und Anwendung → Entstehung fachlicher Beziehungen → abhängige Arbeit → Kennungen/Herkunft und Grenzen. Bestehende Erklärungen zu Status und Entscheidungen sinnvoll anschließen. | Die Suche stellt vorhandene Inhalte, offene Fragen und Ablehnungswissen bereit; das Modell schlägt die fachliche Zuordnung vor. Keine garantierte Duplikat- oder Widerspruchserkennung. |
| 5.5 — `chap5/chap5.5/chap5.5table1.tex` | Herkunftszeile auf tatsächliche Quellformen und Reichweite prüfen; eine pauschale Ledger-Herkunft aller Requirements/Architekturelemente vermeiden. Beziehungstabelle als knappe Bedeutungsübersicht erhalten. | Drei Erklärgruppen unterscheiden: Herkunft/Übernahme, fachliche Beziehungen/Fortschreibung, externe Zuordnung. Keine neue vollständige Ontologie und keine siebenfeldrige Referenzpflicht. |
| 5.6 — `chap5/chap5.6/chap5.6.tex` | Zwischen Freigabe und Speicherung kurz erläutern, dass die Anwendung den nächsten Zustand einschließlich operationstypischer Verbindungen vorbereitet; danach prüft die gemeinsame Speicheroperation die definierten Strukturregeln. | Kangal erzeugt keine Beziehungen, autorisiert keine Inhalte und prüft nicht sämtliche externen Herkunftsziele auf Auflösbarkeit. Unterschiedliche Apply-Schritte bleiben erkennbar. |
| 5.9 — `chap5/chap5.9/chap5.9.tex` und `chap5/chap5.9/chap5.9table1.tex` | F1-Stationstabelle gezielt schärfen: Welche Information beziehungsweise Verbindung kommt bei Ableitung, Übernahme, Arbeitspaketbildung und Projektion hinzu? Anschließend die kleine Netzgrafik und einen kurzen Absatz zur späteren GitHub-Ergänzung einfügen. Redundante Relationsaufzählungen ersetzen. | Tabelle erklärt die Entstehung; Grafik zeigt die verbundenen Ergebnisse und das Wachstum. Initialisierung nicht nochmals neu erzählen. REQ-42 bleibt separater Verfeinerungsfall. |

**PBI-Fortschreibung konkret erklären:** Ein vorhandenes Arbeitspaket kann um eine zusätzliche Anforderung ergänzt werden. Der vorgeschlagene Bezug wird nach Freigabe als `covers` gespeichert; die vorgesehene Zuordnung zum Feature wird ergänzt. Eine gesondert vorgeschlagene und freigegebene Angleichung kann Titel, Beschreibung oder Akzeptanzkriterien nachziehen. Feature-Bildung, Beziehungsergänzung und Inhaltsangleichung nicht als einen automatisch erledigten Schritt darstellen. Fachliche Beziehungen und Herkunftsverweise haben verschiedene Bedeutungen und Entstehungsorte.

**Herkunft konkret erklären:** Beim Transkript beginnt der Rückweg mit Lauf-/Claim- und Unit-Bezügen. Diktat, GitHub und Analyst haben eigene Eingangsartefakte und Übernahmebelege. Die Zuordnung des Ursprungs erklärt nicht automatisch den gesamten aktuellen Inhalt nach späteren Änderungen. `origin` ist kein verlässlicher alleiniger Schalter für die heute benötigten Felder; `sourceRunId` ist nicht in jedem Fall der ursprüngliche Erzeugungslauf. Keine pauschale Behauptung, alle Angaben blieben bei Fortschreibung unverändert oder würden einheitlich aktualisiert.

### Schritt 3: Bestehende Zusagen und Kapitelanschlüsse nachziehen

- [x] `chap4/chap4.7.tex`: Bestätigungs-Herkunft als Entwurfsabsicht beziehungsweise begrenzt realisierte Wirkung kenntlich machen; keine nachträgliche Erfindung explorativer Beobachtungen und keine Umdefinition von A7, um einen Befund verschwinden zu lassen.
- [x] In 5.5 die Formel „eine Bestätigung [ergänzt] seine Herkunftsangaben“ präzisieren: Zusammenführen von Claim-Kennungen ist noch kein eigener eindeutig auflösbarer Beleganschluss des Bestätigungslaufs. Die Grenze einmal erklären, an weiteren Stellen verweisen.
- [x] `chap6/chap6.2/chap6.2.tex` und `chap6/chap6.7/chap6.7.tex` gegenlesen: MAF organisiert die beschriebenen Schritte; Anwendungslogik erzeugt die fachlichen Zustandsänderungen. Nur fehlende Anschlüsse ergänzen, vorhandene technische Erklärung nicht duplizieren.

- [x] `chap5/chap5.8/chap5.8.tex` und `chap6/chap6.6/chap6.6.tex`: vorhandene Steward-Erklärung als Anschluss nutzen; nur bei einem konkreten Widerspruch oder fehlenden Verweis ändern. Keine doppelte Erklärung seines Werkzeugraums und keine Behauptung, jede Steward-Handlung durchlaufe den gesamten Eingangsweg.

**Ergebnis:** Eine zusammenhängende Architekturpassage, deren Begriffe und Zusagen mit Exploration und Realisierung vereinbar sind.

## 4. Grafikauftrag

### Vorhandene Grafiken

- **Abb. 5.1, Gesamtarchitektur:** für dieses Paket beibehalten. Sie zeigt Eingangskonvergenz und Seed-Ausnahme bereits; Delta im Text erklären. Keine zusätzliche Box mit neuem Layoutbedarf allein für diese Präzisierung.
- **Abb. 5.2, Ledger-Modell** (Datei `chap5/chap5.3/Abbildung5.3.pdf`): beibehalten. Sie erklärt den transkriptgebundenen Herkunftspfad und soll nicht alle anderen Eingänge aufnehmen.
- **Abb. 5.3–5.6:** MCR, Entscheidungsauflösung, Mutation und Außenkopplung bleiben eigenständige Ablaufdarstellungen. Kein Ersatz durch eine größere Gesamtgrafik.
- **Abb. B.1:** bestehende Pflichtkorrektur B-06: Erst-Seed ohne Einzelautorisierung und nachgelagerte Strukturvorschläge getrennt darstellen; Caption-Kollision mit Seitenzahl unter B-12 beseitigen. Vor Bearbeitung die maßgebliche Draw.io-Quelle eindeutig bestimmen; keine ähnlich benannte archivierte Datei ungeprüft überschreiben. Umsetzung im bereits vorgesehenen abschließenden Grafikpaket.

### Integrierte Grafik in 5.9: „Schrittweise Fortschreibung am Besuchsbeispiel“

**Aktueller Stand v03, 14.09.2026:** Auf Nutzerwunsch ersetzt eine schrittweise Ablauf- und Ergebnisdarstellung die ursprünglich geplanten zwei nebeneinanderstehenden Zustände. Sie vertieft den Betriebspfad der Gesamtarchitektur bei vorhandenem Core. Der ausgewählte Inhalt ist weiterhin die Besuchsübersicht, nicht das vollständige Feature FC-15.

- **Links:** GitHub-Kommentar fachlich aufbereiten → gegen den Bestand einordnen → nach Freigabe REQ-89 übernehmen → Arbeitspaketerweiterung und Inhaltsangleichung vorschlagen → nach Freigabe Verbindungen und Inhalt anwenden → nach Veröffentlichungsfreigabe Issue #45 aktualisieren. Abruf und Sammlung durch die GitHub-Ernte werden im Begleittext vor der fachlichen Aufbereitung eingeordnet.
- **Rechts:** vorhandener Bestand; danach Ergebnis nach Schritt 5 mit FC-15, PBI-047, REQ-83 und REQ-89; darunter Issue #45 nach Schritt 6. „Ergänzende Anforderung“ bedeutet neues Core-Element ab Schritt 3. Der konkrete ursprüngliche Beleg ist der Kommentar zu Issue #45, erschlossen über Eingang GH-45 und seinen Lauf.
- **Gemeinsamer Mechanismus:** Die drei externen Eingänge werden unterschiedlich aufbereitet und münden bei vorhandenem Core in die gemeinsame Einordnung. Der Steward ist zugleich Bedienebene für verschiedene Abläufe. Welche Folgeoperationen erforderlich sind, hängt vom Inhalt ab; Bootstrap und interne Wiedereintritte werden nicht als identische Pfade dargestellt.

**Belegbasis:** Vorhandene Besuchsstruktur aus F1 (`20260818_084712_765eea`); GitHub-Ergänzung aus `20260820_172043_802e63`. Die beiden Läufe bleiben getrennt. Die geprüften Pläne, Freigaben, gespeicherten Relationen und Projektionsberichte tragen REQ-89, PBI-Erweiterung/Inhaltsangleichung und die Aktualisierung von Issue #45. Der Code wurde für GitHub-Ernte/Delta und Graph-Konvergenz gegengelesen; keine neue Systemausführung.

**Zeichen- und Textregeln:**

1. Die sechs Schritte fassen fachliche Stationen zusammen; sie behaupten keine sechs identischen Codeknoten für jeden Eingang.
2. Linke Pfeile: Ablauf. Rechte Pfeile: geplante Umsetzung und operative Issue-Darstellung. Der Themenrahmen fasst die Feature-Zugehörigkeit zusammen.
3. Der gestrichelte Pfeil bezeichnet jetzt den **Lesezugriff auf den Core**. Die alte Erklärung als Herkunftspfeil ist entfernt. Quellenhinweise an den Anforderungen werden über ihre archivierten Belegwege erklärt.
4. Blau/Ocker/Grün und H unterscheiden modellgestützte Verarbeitung, deterministische Anwendung, Core und menschliche Freigaben. Kennungen und Zeitpunkte stehen zusätzlich im Text.
5. `source` an den dargestellten Beziehungen kennzeichnet ihren Erzeugungsmechanismus (`core-seed-backlog`/`pbi-update`); das ersetzt weder Laufzuordnung noch vollständige Änderungshistorie.
6. Der hochformatige Ausschnitt erhält eine passende höhenbegrenzte Einbindung und kurze Caption. Finale Größe und Lesbarkeit werden im Overleaf-Satz abgenommen.

**Integrierte Dateien:** `chap5/chap5.9/chap5.9.tex`, `chap5/chap5.9/core-netz-besuchsbeispiel.drawio` und der entsprechende PDF-Export. Overleaf-Ziele und Label bleiben unverändert: `Kapitel/05/05.9.tex`, `figures/05/core-netz-besuchsbeispiel.pdf`, `fig:core-netz-besuchsbeispiel`. Siehe [Umsetzungsbericht](./P10-4a-Umsetzung-2026-09-14.md). Die PDF-Abnahme der gesamten Thesis steht noch aus.

## 5. Schritt 4: Audit v4 wissenschaftlich integrieren

Nach dem Mechanismus folgt dessen Nachweis. Der Nachtrag ersetzt die aktuelle v3-Ergebnisdarstellung zusammenhängend; die alte Auswertung bleibt als historisches Verfahren reproduzierbar und im Anhang eingeordnet. Keine bloße Ersetzung „62 offen“ durch „3 offen“, weil die Endpunkte verschieden sind.

| Datei / Funktion | Geplante Änderung |
|---|---|
| `kapitel-7/tex-v2/chap7.5/chap7.5.tex` | Im bestehenden Unterabschnitt mit Label `sec:eval-provenienz` Gegenstand, nachträgliche Verfahrensverbesserung, mehrere getrennte Prüfdimensionen, Kernergebnisse und Grenzen erläutern. |
| `kapitel-7/tex-v2/chap7.1/chap7.1table1.tex` | P6-Frage an die tatsächlich getrennt ausgewiesenen Herkunfts-/Referenzprüfungen angleichen; vorhandene A-Zuordnung und Kennung erhalten. |
| `kapitel-7/tex-v2/chap7.7/chap7.7table1.tex` | P6-Ergebniszeile auf v4-Endpunkte und dieselben Grenzen wie 7.5 bringen. |
| `kapitel-7/tex-v2/chap7.8/chap7.8.tex` | Nachträgliche Auswertungsentwicklung und begrenzte Prüftiefe bei den passenden Gültigkeitsgrenzen verankern. |
| `anhang/anhang-evaluation.tex` | Verfahren, Snapshot, Instrument-/Ergebnisstand, Reproduktion, vollständige Zahlen und zugängliche Belegpfade aufnehmen. v3/v4 unterscheidbar halten; aus der PDF auffindbare Belegzuordnung. |
| `chap8/chap8.1/chap8.1.tex` | Beitrag und Grenzen anhand der neuen differenzierten Ergebnisse interpretieren, die alte 62/237-Schlussfolgerung ersetzen. |
| `chap8/chap8.2/chap8.2.tex`, `chap8/chap8.3/chap8.3.tex`, Kapitel 1/Kurzfassung | Anschlussprüfung; nur bei betroffenem Anspruch ändern. Kein zweiter vollständiger Architekturbericht. |

**Verbindliche Zahlen- und Reichweitenregeln aus v4:**

- 147/237 Items mit mindestens einem direkten Definitions-/Eingangsbezug; 234/237 mit direktem Bezug oder gerichtetem Beitragspfad. Letzteres ist **keine Quote vollständig nachvollziehbarer aktueller Inhalte**.
- 39 identifizierte Eingangsobjekt-Zuordnungen, davon 37 mit Plan/Apply; diese Mengen sind nicht disjunkt zu den vorherigen.
- 104/104 gespeicherte Ledger-Herkunftskanten sind im ausgewiesenen Ursprungskontext auflösbar. Das beweist nicht die vollständige Herkunft jeder aktuellen Fassung.
- 232 aktuelle Einzelreferenzvorkommen: 168 aufgelöst, 64 offen, verteilt auf 33 Items. Offene Referenzen werden nicht durch einen anderen erfolgreichen Pfad verdeckt.
- 71 Claim-Definitionen, 173 Zitatvorkommen: 52 vollständige normalisierte Treffer, 104 nur Körper-Treffer, 14 ohne vollständigen Treffer, drei unter der Längenschwelle. Keine Gleichsetzung aller Körper-Treffer mit falschen Sprechern.
- 158 untersuchte Unit-Verweise auflösbar und Unit-Texte im gebundenen Transkript auffindbar; keine neue semantische Vollannotation.
- 38 Items mit Version > 1: 22 aktuelle Plan-/Apply-Anschlüsse mit primärem Textvergleich, zwei PBI-Teilübereinstimmungen, 14 ohne Anschluss unter den unterstützten Regeln. Bei neun dieser 14 ist der Haupttext unverändert; kein pauschaler Verlust von 14 inhaltlichen Änderungen.
- 34 Instrumentkontrollen, zwei byteidentische Auswertungen, 414 Originaldateien unverändert. Das sind Nachweise des Auditverfahrens, keine neue vollständige Systemtestsuite oder Modellstabilitätsmessung.

Im Haupttext die für die Aussage erforderlichen Ergebnisse auswählen; Detailverteilungen im Anhang vollständig zugänglich machen. Nutzen für Quellenprüfung und verknüpfte Fortschreibung erklären, aber weder Precision-/Recall-Gewinn, geringeren menschlichen Prüfaufwand noch kausale MAF-Überlegenheit daraus ableiten. Die formative Entstehungsgeschichte bleibt von dieser späteren Nachauswertung getrennt.

## 6. Gezielte technische Beleganker

Die folgenden Dateien dienen der Gegenprüfung, nicht als Klasseninventar für den Haupttext. Pfade relativ zu `AgenticSdlc.Host/FullWorkflow/`:

| Frage | Gegenprüfstellen |
|---|---|
| Wo laufen die regulären Eingänge zusammen? | `08-pipeline/fullworkflow/PipelineFullWorkflow.cs`, `PipelineFullRunner.cs` im selben Ordner; `08-pipeline/PipelineComposedWorkflow.cs` |
| Wie werden Eingänge und ihre Herkunft vorbereitet? | `04-delta/ProjectStateBuilder.cs`, `04-delta/AuthorFrontDelta.cs`, `07-tore/github/inbound/GithubInboundDrafting.cs`, `09-analyst/AnalystDeltaBuilder.cs` |
| Wie werden Bestand und frühere Ablehnungen berücksichtigt? | `07-tore/ingestion/IngestionTools.cs`, `IngestionGate.cs` im selben Ordner |
| Wo entstehen Core-Inhalte und operationstypische Verbindungen? | `07-tore/ingestion/IngestionApply.cs`, `IngestionApplyExec.cs`; `05-core/CoreBacklogSeeder.cs` |
| Wie werden Arbeitspakete erweitert und Konflikte aufgelöst? | `07-tore/pbiupdate/PbiUpdateApply.cs`, `07-tore/decision/DecisionResolutionApply.cs` |
| Was kontrolliert das Speichern, und was nicht? | `05-core/JsonCoreRepository.cs`, `05-core/CoreKangal.cs` |
| Wo wird die Issue-Zuordnung ergänzt? | `07-tore/github/GithubForwardApply.cs` und konkrete historische Projektionsberichte |

## 7. Schritte 5 und 6: Konsistenzprüfung, Übergabe und PDF-Abnahme

- [x] Alle betroffenen Aussagen und Begriffe einmal über den Live-Text prüfen: Delta ≠ Operationsplan; modellgestützte Zuordnung ≠ deterministische Anwendung; fachliche Beziehung ≠ Herkunftsbeleg; Ursprung ≠ jüngste Änderung; geplante Arbeit ≠ implementierte Funktion.
- [x] Neu-/Alttext gegen Belegmatrix und v4-Ergebnisse prüfen; Querverweise, Labels und Zähleinheiten konsistent halten. Veraltete Aussagen nicht nur im Fließtext, sondern auch in Tabellen, Captions und Kurzfassung suchen.
- [x] Kürzen, wenn Text die Tabelle/Grafik nur aufzählt. Ein Erstleser soll die Rollen ohne sieben verschiedene Feldnamen verstehen. Vorhandene Kapiteltrennung 4/5/6/7/8 erhalten.
- [x] Nur tatsächlich geänderte Dateien mit exaktem Overleaf-Ziel melden. Die Tabellenziele aus den aktuellen `input`-Anweisungen übernehmen; keine alten Kommentarpfade als Sync-Ziel ausgeben. Abgeleitete Grafik-PDF synchronisieren, Draw.io separat als bearbeitbare Quelle liefern. Uni-Präambel und allgemeine Formatierung bleiben erhalten.
- [ ] Nach dem Sync die neue PDF identifizieren und geänderte Seiten samt Übergängen visuell prüfen: Grafik, Beschriftungen, Lesbarkeit, Nummerierung, Tabellen, Anhang, Verweise. Keine Platzhaltergrafik als abgeschlossen melden.

**Abnahmekriterium aus Lesersicht:** Ohne Code und ohne dieses Planpapier lässt sich beantworten: (1) Wie unterscheiden sich Bedienung über Direktzugang/Steward und fachliche Eingänge; was kommt aus deren Aufbereitung an? (2) Wo wird gegen den Bestand eingeordnet? (3) Wer schlägt vor, wer entscheidet und wer erzeugt die Verbindungen? (4) Wie wächst ein bestehendes Arbeitspaket? (5) Wie geht man zur Herkunft zurück? (6) Was schützt Kangal? (7) Was belegt die Evaluation, und welche Lücken bleiben? Dafür müssen Haupttext, Beispiel und Ergebnisdarstellung zusammenpassen; eine neue Grafik allein reicht nicht.

**Abschlussstatus:** B-47 erst nach korrekter TeX-Integration, Belegzugang und PDF-Abnahme schließen. B-51 als nicht technisch behobene, zutreffend begrenzte Systemeigenschaft führen; die Reichweitenbehandlung gesondert abschließen. B-06/B-12 bleiben bis zur Korrektur/Abnahme von B.1 offen. Danach P10-2 fortsetzen, nicht erneut das gesamte System inventarisieren.

## 8. Umgang mit unerwarteten Punkten

Eine nicht belegte Grafikverbindung zunächst am Lauf prüfen; bei weiterhin fehlendem Nachweis weglassen oder ausdrücklich als offene Rekonstruktion behandeln. Ein neuer Darstellungsfehler wird im Paket korrigiert. Ein bisher unbekannter relevanter Funktionsfehler wird mit Ursache, betroffener Aussage, kleinstem Eingriff, Risiko und Testbedarf vorgelegt; er löst keinen stillen Codepatch aus. Nur davon abhängige Aussagen pausieren. Reine Robustheits-/Refactoringideen gehen in die bestehende Sammlung. Ein neuer Modellvergleich, eine Neuannotation oder ein größerer Systemumbau braucht eine eigene begründete Umfangsentscheidung.

**Nächster ausführbarer Schritt:** Die 18 Sync-Dateien aus dem [Umsetzungsbericht](./P10-4a-Umsetzung-2026-09-14.md) nach Overleaf übernehmen und die aktualisierte PDF bereitstellen. Danach Abnahme von Textanschlüssen, Nummerierung, Grafik, Tabellen und Anhang; bei erfolgreicher Abnahme B-47 schließen und P10-2 beginnen. B-51 bleibt technisch ungelöst, sein begrenzter Anspruch wird gesondert abgenommen.
