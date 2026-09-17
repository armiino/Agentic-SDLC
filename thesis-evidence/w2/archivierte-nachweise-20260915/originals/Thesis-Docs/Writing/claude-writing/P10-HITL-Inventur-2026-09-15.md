# P10: HITL-Inventur und Auswahl der tragenden Nachweise

**Fortschreibung vom 15.09.:** Der hier ausgewählte F1/F3-Abgleich und das begrenzte Governance-Textpaket sind inzwischen lokal umgesetzt. [Nachweisbericht und Sync-Liste](./P10-HITL-Governance-Nachweis-2026-09-15.md). Im aktualisierten PDF inhaltlich bestätigt; Satzrest Anhang C unter B-12. Die folgende Inventur dokumentiert ihre ursprüngliche Prüfgrundlage.

Stand: 15.09.2026. Geprüft: aktuelle lokale TeX-Fassung, Request-Port-Verdrahtung der Hauptkette, Review-Adapter, zugehörige Runner und registrierte Zusatzwege. Anschluss an den [Phase-10-Plan](./Phase-10-Gesamtabnahme-2026-09-13.md). Keine neue Modellausführung, kein Eingriff in Code, historische Läufe, TeX oder Grafiken.

**Ergebnis:** Für die zentrale Governance-Aussage sind Anforderungseinordnung, Auflösung offener Entscheidungen, PBI-Fortschreibung und externe Freigabe die tragenden Beispiele. Der FacetValidator ist Teil der vorgelagerten Evidenzprüfung. Er ist weder das zentrale Human Gate noch eine eigenständige Freigabe jeder fachlichen Änderung. Die vorhandene Hauptkette enthält zehn unterschiedliche Request-Port-Kennungen; mehrere separat bedienbare Prüfwege und Werkzeugzustimmungen kommen hinzu. Zehn Ports sind keine zehn unabhängigen Wirksamkeitsnachweise und werden nicht in jedem Lauf vollständig durchlaufen.

## 1. Ziel und Auswahlregel

Die Forschungsfrage verlangt eine begründete Aufgabenverteilung: Welche semantische Deutung oder Vorschlagserzeugung übernimmt ein Modell beziehungsweise Agent, welche Festlegung bleibt dem Menschen, und welche Regeln setzt die Anwendung deterministisch um? HITL ist dabei mehr als Fehlerkorrektur. Ein fachlich plausibler Vorschlag benötigt ebenfalls eine verantwortliche Entscheidung, wenn er Anforderungen, Architektur, Arbeitsvorrat oder externe Veröffentlichungen verändert.

Die Inventur fragt je Entscheidungspunkt nach Auslöser, Vorlage, menschlichem Spielraum und Wirkung. Für eine Vertiefung im Haupttext zählen die Nähe zur Forschungsfrage, der Eingriff in den Projektbestand, der Anschluss an das durchgehende Beispiel und die verfügbare Belegkette. Die Auswahl erfolgt nicht nach einem möglichst positiven Ergebnis. Begrenzungen und Gegenbefunde der ausgewählten Fälle bleiben erhalten.

Die bisherige Auswertung von zehn Facettenkorrekturen beantwortet eine andere Frage: ihre Nähe zum ursprünglichen Transkript nach der damaligen Bearbeitung. Ihre durch den Autor bestätigten Urteile 5/4/1 werden nicht zur Erfolgsquote menschlicher Entscheidungen oder zum zentralen Governance-Nachweis umgedeutet. Sie bleiben im [Prüfbericht](./P10-HITL-Inhaltspruefung-2026-09-15.md) verfügbar; daraus folgt kein eigener großer Textblock in Kapitel 7.

## 2. Zehn Entscheidungspunkte der Hauptkette

Die folgende Inventur beschreibt die interaktive Nutzung. Experimentmodi, Leerfälle und die Initialisierungsausnahme sind unten getrennt aufgeführt. Die Wirkung bezeichnet den vorgesehenen Folgepfad; die heutige Codelektüre allein ist kein neuer Ausführungsnachweis für jeden Eintrag.

| Entscheidungspunkt und Auslöser | Was wird vorgelegt? | Was entscheidet der Mensch? | Warum und welche Wirkung? |
|---|---|---|---|
| **Evidenz-Adjudikation**: beanstandete Claims und entscheidbare Hinweise nach dem Ledger | Aussage, Quellenstellen, Prüfbegründung und gegebenenfalls Facettenvorschlag bzw. ungenutzte Unit | Je Vorlagentyp übernehmen, Facetten ändern, Evidenz anhängen, neuen Claim aufnehmen, verwerfen oder offen lassen | Bestimmt die verwendbare Evidenz für die Ableitung. Unbeanstandete Claims laufen ohne Einzelentscheidung weiter; noch keine fachliche Core-Autorisierung. |
| **Themenbündelung**: vorgeschlagene Änderungen an Feature-Clustern im Bootstrap-/Re-Clarify-Pfad | Cluster, zugeordnete Anforderungen und vorgeschlagene Verschiebungen, Zusammenführungen, Abspaltungen oder Querschnittszuordnungen | Die vorgeschlagenen Strukturänderungen anwenden oder überspringen | Macht die fachliche Zusammengehörigkeit von Anforderungen prüfbar; angewandte Cluster bilden die Basis für den Arbeitspaket-Zuschnitt. |
| **Arbeitspaket-Zuschnitt**: erzeugte PBIs im Bootstrap-/Re-Clarify-Pfad | PBI-Texte, Akzeptanzkriterien, zugehörige Anforderungen und Prüfbefunde | Akzeptieren, bearbeiten oder verwerfen; bearbeitbar sind unter anderem Text, Kriterien, Umfang, Priorität und Readiness | Menschlich bestimmter Arbeitsvorrat vor der weiteren Übernahme. Anwendung und erneute Struktur-/Readiness-Prüfung sind eigene Schritte. |
| **Anforderungseinordnung**: neuer Eingang gegen vorhandenen Core, formal bestandener nicht leerer Plan | Operation und Begründung, neuer Inhalt, gegebenenfalls heutiger und vorgeschlagener Bestandstext, betroffene PBIs und früheres Ablehnungswissen | Je Operation übernehmen oder begründet ablehnen; die Oberfläche dieses Gates ist kein allgemeiner Freitexteditor des Plans | Agent deutet neu, verwandt, verfeinernd oder widersprüchlich; Mensch autorisiert die vorgeschlagene fachliche Wirkung. Anwendung erzeugt die entsprechenden Items, Versionen, Entscheidungen und Beziehungen. |
| **Architektureinordnung**: architekturbezogene Eingänge im separaten Aspektpfad | Analoger Operationsplan für Architekturinhalte | Übernehmen oder begründet ablehnen | Derselbe Grundmechanismus für einen anderen fachlichen Gegenstand. Die zusätzliche Port-Kennung ist keine neue generelle Governance-Idee. |
| **Architekturrollen**: zur Klassifikation vorgelegte Architekturelemente | Agentenvorschlag für Rollen, Begründung und gegebenenfalls Ziel-PBIs | Rollen und Ziele bestätigen oder korrigieren; vertagen | Unterscheidet technische Rahmenbedingungen, auszuführende Arbeit und dokumentierbare Architekturentscheidungen. Die Festlegung steuert Beziehungen und nachgelagerte PBI-/ADR-Verarbeitung. |
| **Entscheidungsauflösung (DEC)**: offene Entscheidungen aus dem Core, einschließlich zuvor vertagter | Bestehender Inhalt, entgegenstehende/neue Aussage bzw. offene Frage, Herkunft und betroffene PBIs | Bei zielbezogenen Konflikten Original behalten, Neues übernehmen, verfeinern oder vertagen; bei Übernahme/Verfeinerung neuen Text festlegen. Ziellose Fragen und Architekturklärungen haben engere Optionen | Hier entsteht die fachliche Auflösung unmittelbar durch den Menschen. Im Hauptgraphen sammelt ein deterministischer Scan die offenen Fälle; kein neuer Resolver-Agent bestimmt ihren Ausgang. Anwendung führt die gewählte Auflösung und Folgeänderungen aus. |
| **PBI-Fortschreibung und Angleichung**: Auswirkungen neuer oder geänderter Anforderungen auf Arbeitspakete | Betroffene Anforderungen/PBIs, Zuordnungsbegründung, geplante Features sowie aktuelle und vorgeschlagene PBI-Texte/Kriterien | Strukturwirkung und inhaltliche Angleichung getrennt freigeben; Inhalte bearbeiten, Zuordnung gegebenenfalls ändern, überspringen oder Klärung beantragen | Verhindert, dass semantische Platzierung oder Neufassung automatisch zur gültigen Arbeitsplanung wird. Deckt Beziehungen, Feature-Zuordnung und tatsächlichen Arbeitsinhalt ab. Optionen hängen vom Item-Typ ab. |
| **ADR-Abnahme**: Entwürfe zu entsprechend klassifizierten Architekturentscheidungen | Kontext, Entscheidung, Konsequenzen, gegebenenfalls Alternativen und verwandte Core-Items | Entwurf bearbeiten und freigeben oder vertagen | Kontrolliert die dokumentierte Darstellung einer Architekturentscheidung; nach Freigabe erfolgen ADR-Ablage und zugehörige Core-Vermerke. |
| **Externe Freigabe**: geplanter GitHub-Forward | Vorgesehene Issue-/Dokumentoperationen, Begründung, verfügbare Vorher-/Nachher-Sicht und Drift-Hinweise | Operation übernehmen oder überspringen; bei passendem Drift-Fall bewusst überschreiben | Kontrolliert die Außenwirkung. Eine fachliche Core-Freigabe ist noch keine Veröffentlichungsfreigabe; zusätzlich gilt die Ausführungseinstellung für reale Schreibvorgänge. |

**Verdrahtungsbeleg:** `PipelineFullRunner.cs:402–497` instanziiert `arch-classify-gate`, `adr-gate`, `adjudication-gate`, `cluster-review-gate`, `backlog-review-gate`, `ingest-gate`, `arch-ingest-gate`, `decision-gate`, `pbi-gate` und `github-forward-gate`. `PipelineFullWorkflow.cs` verbindet Requests mit den Antwort-/Apply-Schritten. Der ältere Kommentar „7 Human-Gates“ ist kein aktuelles Inventar; gezählt wurden die tatsächlichen Instanziierungen.

## 3. Zusätzliche Wege und andere Arten menschlicher Mitwirkung

Eine reine Portzählung erfasst das System nicht vollständig. Folgende zusätzliche Review-Zugänge sind in den jeweiligen Command-Registrierungen erreichbar; ihre Registrierung ist kein neuer Beleg heutiger vollständiger Ausführung.

| Zusätzlicher Zugang | Vorlage und Entscheidung | Einordnung für die Thesis |
|---|---|---|
| Prüfung abgeleiteter Risiken (`derive-review`) | Abgeleitete Risiken mit Prüfbefunden; genehmigen, verwerfen oder Überarbeitung verlangen. Ergebnis: ausgewählte Risiken und Entscheidungsprotokoll | Separat bedienbarer Ableitungs-/Reviewpfad. Nicht mit dem DEC-Gate oder sämtlichen Risikoeingängen gleichsetzen. Kein zusätzlicher Schwerpunkt ohne eigenen Forschungszweck. |
| L3-Hinweisreview (`l3-review`) | Geroutete fachliche Hinweise mit Kontext; annehmen, Text ändern, verwerfen oder Revision anfordern | Ergänzende fachliche Bearbeitung, teils mit Rückmeldung an die Erzeugung. Gehört zur Übersicht, nicht automatisch in einen weiteren Haupttextfall. |
| Separate Issue-Planung (`l4-issuplanning-review`) | Issue-Plan samt Operationen, Inhalt und Prüfbefunden; annehmen, bearbeiten, verwerfen oder Revision verlangen | Spezialisierter Planungsweg; nicht als elftes Gate desselben Hauptgraphen zählen. |
| GitHub-Statusrückweg (`github-reverse-review`) | Zustandsdifferenz zwischen Issue-Snapshot und Core, etwa Vorschlag zum PBI-Abschluss; übernehmen/verifizieren oder überspringen | Eigenständiger Kontrollzweck: Ein geschlossenes Issue soll den fachlichen Abschluss nicht selbst autorisieren. Der Plan entsteht hier deterministisch, ohne Agent. Nicht mit der semantischen Kommentar-Ernte verwechseln. |
| Separate Entscheidungsauflösung (`decision-review` / Decision-HITL) | Auflösungsplan nach einer freien Stakeholder-Antwort; anwenden oder überspringen | Variante derselben fachlichen Entscheidungsklasse. Hier kann ein Resolver-Agent die freie Antwort in typisierte Vorschläge übersetzen. Das unterscheidet sich von der direkten menschlichen Auswahl am DEC-Gate des Hauptgraphen. |

**Steward und Browser sind Zugänge zu Entscheidungen, keine jeweils neuen fachlichen Gate-Arten.** `StewardGateTools` reicht im Hauptgraphen Antworten für Anforderungs-/Architektureinordnung, DEC und Forward ein; PBI-Entscheidungen werden außerdem über registrierte wartende Vorschläge bearbeitet. Andere Gates erhalten nicht automatisch denselben Chat-Zugang. Das Einreichungswerkzeug benötigt eine eigene hostseitige Zustimmung. Ein normales Gespräch ist noch keine gespeicherte Gate-Antwort.

**Werkzeugzustimmung, fachliche Entscheidung und Integritätsprüfung bleiben getrennt.** Die Zustimmung zum Start eines kosten- oder wirkungsträchtigen Werkzeugs ist eine Ausführungserlaubnis; ein Human Gate entscheidet über den vorgelegten fachlichen Gegenstand; CoreKangal prüft strukturelle Bedingungen. Ein deterministischer Prüfer mit „Gate“ im Namen und ein Ergebnisstatus `HumanReview` sind für sich noch kein menschlicher Entscheidungspunkt.

## 4. Was die Arbeit bereits abdeckt und was knapp zu präzisieren ist

- **5.6** begründet die Autorisierung der Fortschreibung, trennt Kontrollarten und zeigt F1s tatsächliche Auswahl. Der Hauptzweck ist damit bereits vorhanden.
- **5.5/5.9** erklären Auflösung, Beziehungen und die Fortschreibung des Besuchsfalls. Eine erneute komplette Ablaufbeschreibung wäre redundant.
- **6.4/6.6** beschreiben Request Ports, Pause/Wiederaufnahme, Browserbedienung und Steward. Der Betriebsmechanismus braucht kein neues Kapitel.
- **Anhang C** enthält die zehn Entscheidungspunkte der Hauptkette. Er ist eine gute Grundlage, aber keine vollständige Inventur sämtlicher separat bedienbarer Review-Wege. Titel/Einleitung sollten den Hauptkettenumfang kenntlich machen; eine kurze Abgrenzung der Zusatzwege reicht.
- **7.4** prüft unter anderem fehlende/abgelehnte Freigabe, gültige Übernahme und wiederholte Anwendung am realen Ingest-Apply-Pfad. Daraus folgt kein identischer Teststatus aller Gates.
- **7.5** enthält F1 (Auswahl und Projektion), F2 (Verfeinerung), F3 (menschliche Konfliktauflösung) und den externen Kommentar-Rückweg. Vorhandene Grenzen bleiben: etwa offene PBI-Angleichung und Herkunftsanzeige in F3 sowie menschliche Textbearbeitung in F2.

**B-56, konkrete Darstellungspräzisierung:** 5.6 bezeichnet eine zusätzliche Begründung bei Entscheidungsauflösung pauschal als optional. Das stimmt für die gewöhnliche zielbezogene Konfliktauflösung, aber nicht für alle DEC-Typen: `PipelineDecisionReviewAdapter.Resolved` verlangt bei der Erledigung zielloser Fragen und beim bewussten Verzicht auf eine Architekturfestlegung eine Begründung. Ein kurzer Zusatz zum fallabhängigen Umfang genügt. Gemeinsam mit dem Hauptketten-Scoping in Anhang C ist dies Textarbeit, kein festgestellter neuer Codefehler.

**FacetValidator:** Seine Rolle als Quellenprüfung bestehender Aussagen und Einordnungen kann in 5.3 mit wenigen Sätzen klarer motiviert werden. Er prüft mehrere Facetten; die nachgelagerte Facettenvervollständigung neuer Claims ist ein anderer Schritt. Diese Erklärung rechtfertigt keinen eigenen umfassenden HITL-Evaluationsblock.

## 5. Auswahl und nächster Arbeitsschritt

**Hauptnachweis:** F1 für Vorschlag → menschliche Auswahl → Anwendung → Projektion verwenden, weil der Fall bereits durch die Arbeit führt. Die zwölf Operationen und acht Übernahmen/vier Ablehnungen sind eine Auswahlbilanz, keine Modellgenauigkeit. Die konkrete Ablehnung einer Bestandsbestätigung mit drohendem Verlust einer vorhandenen Präzisierung zeigt den Entscheidungssinn.

**Ergänzung:** F3 für die direkte menschliche Konfliktauflösung nutzen. Dabei ausdrücklich zeigen, dass am zentralen DEC-Gate keine weitere Modellwahl den Ausgang vorgibt. Der Agent kann den Konflikt zuvor erkennen und vorschlagen; die Auflösung und gegebenenfalls der gültige neue Text stammen vom Menschen. Die belegten Grenzen nicht weglassen.

**Verifikation:** Vorhandene T1–T4-Kontrollen als technischen Gegenpart zu diesen Fallbeobachtungen erhalten. Den Prüfpfad exakt benennen. Für die konkret ausgewählte Falloperation sollen Vorlage, Entscheidung und Ergebnis sichtbar zueinander passen. Die heutige Inventur hat vorhandene Text-/Beleganschlüsse zugeordnet, diese einzelnen Laufdateien jedoch nicht erneut vollständig nachgerechnet.

Damit lautet der nächste begrenzte Schritt: **F1/F3 und ihre Kontrollnachweise gezielt auf einen noch fehlenden Anschluss zwischen Vorschlag, menschlicher Entscheidung und Wirkung prüfen; anschließend nur fehlende Erläuterungen und B-56 im bestehenden Text nachziehen.** Eine weitere allgemeine HITL-Messung oder eine Begutachtung aller Gate-Entscheidungen ist daraus nicht automatisch abzuleiten. Inhaltliche Gegenprüfungen durch den Autor werden nur für konkret übernommene semantische Urteile angefragt.

Die bereits durchgeführte Facettenprüfung wird nicht verworfen oder versteckt. Ihr Erkenntnisumfang und die Bestätigung des Autors bleiben dokumentiert. Sie wird nach Forschungsrelevanz eingeordnet, nicht allein aufgrund ihres Aufwands in den Haupttext übernommen.

## 6. Reichweite und Codeanker

Der aktuelle statische Abgleich umfasst zehn Ports im Hauptgraphen sowie die fünf oben genannten zusätzlichen/alternativen Review-Zugänge. Pro UI wurde der angebotene Gegenstand und Entscheidungsspielraum gelesen; relevante Verdrahtungen, Dateiverträge und exemplarische Apply-Übergänge wurden gegengeprüft. Kein vollständiger Sicherheits-, Ausführungs- oder Usabilitytest sämtlicher Oberflächen. Keine Aussage, dass alle Stellen in jedem Eingang durchlaufen oder durch denselben Test abgesichert werden.

Bekannte Grenzen bleiben: ungegateter initialer Core-Seed; selektive Ledger-Adjudikation; pfadspezifische Leerfälle und Experimentantworten; `HumanReview` ohne Portanfrage bei bestimmten terminalen Prüfergebnissen; keine automatische Gleichsetzung menschlicher Autorisierung mit Quellentreue. Auch das direkte Setzen eines offenen Status ist noch keine separate menschliche Entscheidung.

Die Prüfgrundlage wird im [Quellenmanifest](./P10-HITL-Inventur-2026-09-15-quellen.json) mit SHA-256 festgehalten. Pfade darin sind relativ zum Repository. Zentrale Anker:

- `AgenticSdlc.Host/FullWorkflow/08-pipeline/fullworkflow/PipelineFullRunner.cs:402` und `PipelineFullWorkflow.cs:117`: tatsächliche Ports und Verbindungen.
- `AgenticSdlc.Host/FullWorkflow/08-pipeline/fullworkflow/PipelineFullRunner.EventPump.cs:80`: Antwortverfahren und Betriebsarten.
- `AgenticSdlc.Host/FullWorkflow/07-tore/ingestion/IngestionReviewAdapter.cs:129`: Auswahl, Begründung und Vorher-/Nachher-Sicht.
- `AgenticSdlc.Host/FullWorkflow/07-tore/decision/PipelineDecisionReviewAdapter.cs:32` und `:110`: typabhängige Entscheidungen und Begründungsbedingungen.
- `AgenticSdlc.Host/FullWorkflow/08-pipeline/fullworkflow/DecisionStageExecutors.cs:38`: deterministischer Scan offener Entscheidungen.
- `AgenticSdlc.Host/FullWorkflow/07-tore/pbiupdate/PbiUpdateReviewAdapter.cs:142`: getrennte Struktur-/Inhaltsauswahl, Feature-Zuordnung und Bearbeitung.
- `AgenticSdlc.Host/FullWorkflow/07-tore/github/GithubReverseWorkflow.cs:20`: deterministische Vorschlagserzeugung des Statusrückwegs.
- `AgenticSdlc.Host/FullWorkflow/07-tore/decision/DecisionResolutionWorkflow.cs:25`: agentische Übersetzung freier Stakeholder-Antworten im separaten Pfad.
- `AgenticSdlc.Host/Steward/StewardGateTools.cs:40`: angebotene Lese-/Einreichungswerkzeuge und Zustimmung.

**Kein neuer Overleaf-Sync:** In diesem Schritt werden nur die Inventur, Quellenzuordnung und aktive Arbeitsführung gespeichert. Die TeX-Präzisierungen sind noch nicht umgesetzt.
