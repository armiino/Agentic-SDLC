# Fallkarten zur Core-Inhaltsprüfung

Stand: 15.09.2026. Kriterien siehe [Protokoll](../protokoll.md). Zusätzliche Fälle N01–N08 und bekannte Kontrollen K01–K04 bleiben getrennt. Gültigkeitsurteile beziehen sich auf den jeweils genannten Gegenstand; keine Gesamtquote. Die Karten sind Bewertungsprodukte, die verlinkten Kopien sind Belege.

## N01: Medikamentennachweis: Eingang erhalten, Weiterplatzierung ausgeblieben

Rolle: additional; Lauf `20260729_142120_1ac7c2`; Eingangs-ID `REQ-04`; Operation `NEW`.

**Eingangstext:** Gewünscht ist, dass für jede Medikamentengabe ein Nachweis mit Zeitpunkt dokumentiert wird, damit nachvollziehbar ist, ob die Gabe tatsächlich erfolgt ist; die konkrete Umsetzung bzw. Entscheidung dazu ist noch offen.

**Agentenvorschlag:** Gewünscht ist, dass für jede Medikamentengabe ein Nachweis mit Zeitpunkt dokumentiert wird, damit nachvollziehbar ist, ob die Gabe tatsächlich erfolgt ist; die konkrete Umsetzung bzw. Entscheidung dazu ist noch offen.

**Quelle und Einordnung:** AU-0005 verlangt aus Sicht der Angehörigen einen Nachweis pro Gabe mit Zeitpunkt; AU-0006 nimmt ihn in die Zusammenfassung auf. Der Consumable-Claim kombiniert eine Muss-Proposition mit status=open/modality=desired. Die Baseline macht daraus einen Wunsch mit offener Umsetzung bzw. Entscheidung. Die heutige Autorenantwort legt den Verpflichtungsgrad nicht sicher fest.

**Beitrag des Agenten:** NEW ist gegenüber dem damaligen Bestand vertretbar. REQ-42 betrifft dort nur eine mögliche spätere Medikamentenintegration; eine konkrete Dokumentation je Gabe liegt noch nicht vor. Der Einordnungsagent übernimmt den Delta-Text unverändert.

**Entscheidung:** Ingest und PBI-Tor: GATE_ANSWERED/policy=AcceptAll. Keine menschliche Einzelautorisierung dieses Falls belegt, trotz policyProfile=Interactive in der Konfiguration.

**Gespeicherte Wirkung:** REQ-81 v1 enthält den Vorschlag einschließlich Zweck/Zeitpunkt. Die anschließende NEW_FEATURE-Platzierung für REQ-81 wird laut Apply-Bericht mangels akzeptierten Create-Drafts übersprungen. Kein neues PBI, keine neue Feature-Verbindung, kein GitHub-Write aus dieser Operation.

**Grenzen:** Historischer Juli-Stand. REQ-81 bezeichnet hier einen anderen Inhalt als in späteren August-Ständen. Quellenwiedergabe hinsichtlich Modalität bleibt mehrdeutig; keine heutige Systemstörung oder allgemeine Platzierungsquote aus diesem Fall ableiten.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | mehrdeutig: Sachgehalt getragen; offen/gewünscht gegenüber Zusammenfassung auslegungsbedürftig. | mehrdeutig: Die gleiche Modalität wurde gespeichert; Autor konnte sie anhand der Vorlage nicht sicher entscheiden. |
| K2 | getragen: Eigenständiger Nachweis ist im damaligen Requirement-Bestand nicht konkret geregelt; NEW vertretbar. | getragen: Neue Anforderung angelegt. |
| K3 | getragen: Alle fachlichen Bestandteile des ausgewählten Delta-Items bleiben im Vorschlag. | getragen: Vorschlag und gespeichertes REQ-81 sind textgleich; Rohquellenmodalität separat K1. |
| K4 | — | getragen: Nachgewiesen ist automatische Annahme und genau diese Speicherung, kein menschlicher Qualitätsentscheid. |
| K5 | — | teilweise getragen: Geplante Weiterplatzierung nicht erreicht; dokumentierter Skip wegen fehlendem Create-Draft. |

**Belege:** [Objektauszüge](dossiers/N01.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260729_142120_1ac7c2/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260729_142120_1ac7c2/logs/events.jsonl).

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260729_142120_1ac7c2/07-github/applied/github-forward-apply-report.json)

[Zusätzlicher Quellenbeleg: output.json](evidence/runs/ledger/20260729_142120_9ca536/step-00-atomic-units/output.json)

## N02: Übergabe-Notiz als Bestätigung des Bestands

Rolle: additional; Lauf `20260725_162437_790824`; Eingangs-ID `REQ-07`; Operation `RESTATE`.

**Eingangstext:** Pro Schicht wird im MVP eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.

**Agentenvorschlag:** Pro Schicht wird im MVP eine Übergabe-Notiz benötigt, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.

**Quelle und Einordnung:** AU-0016 des historischen Ledger-Laufs verlangt die pro Schicht angelegte und bei App-Öffnung prominent gezeigte Übergabe-Notiz. Incoming REQ-07 und Bestands-REQ-61 stimmen in dieser Aufgabe überein.

**Beitrag des Agenten:** RESTATE auf REQ-61 vermeidet eine zweite Anforderung für denselben Inhalt. Weitere Notizanforderungen des Meetings sind andere Operationen und kein fehlender Teil dieses ausgewählten Satzes.

**Entscheidung:** AcceptAll im historischen Ingest; keine menschliche Einzelprüfung belegt.

**Gespeicherte Wirkung:** REQ-61 behält Text, Version 1 und bisherigen sourceRunId. Als einzige Item-Änderung kommt canonical-shift-hand-over-note in sourceClaimIds hinzu. Vorhandene covers-/Feature-Beziehungen bleiben. Andere PBI-Änderungen im Lauf stammen von anderen Operationen.

**Grenzen:** Der neue Claim hat am Item keinen eigenen zusätzlichen Laufbezug; die Bestätigung lässt sich über das jetzt gesicherte Laufpaket zurückverfolgen, nicht als vollständige neue Herkunftskette allein aus dem Item. Bekannte B-51-Reichweite; kein neuer Ursprungsschaden.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Quelleninhalt getragen. | teilweise getragen: Inhalt und alter Ursprung erhalten; neuer Claim ohne eigenen Laufbezug am Item. |
| K2 | getragen: Pro-Schicht-Notiz und prominente Anzeige stimmen überein. | getragen: Bestätigung statt Neuanlage. |
| K3 | getragen: Kein zusätzlicher fachlicher Inhalt erfordert Ersetzung. | getragen: Text und Version bleiben exakt gleich. |
| K4 | — | getragen: Auto-Annahme und Quellenkennungs-Ergänzung belegt. |
| K5 | — | getragen: Bestehende PBI-/Feature-Verbindungen erhalten; keine neue Projektion dieser Bestätigung behauptet. |

**Belege:** [Objektauszüge](dossiers/N02.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260725_162437_790824/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260725_162437_790824/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260725_162437_790824/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260725_162437_790824/logs/events.jsonl).

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260725_162437_790824/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260725_162437_790824/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260725_162437_790824/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260725_162437_790824/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260725_162437_790824/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260725_162437_790824/07-github/applied/github-forward-apply-report.json)

[Zusätzlicher Quellenbeleg: output.json](evidence/runs/ledger/20260725_162437_d9208d/step-00-atomic-units/output.json)

## N03: Unvollständige Besuchsstatus-Vorlage wird abgewiesen

Rolle: additional; Lauf `20260818_183319_b3d6e6`; Eingangs-ID `AF-1`; Operation `NEW_RELATED`.

**Eingangstext:** Pflegende müssen angekündigte Besuche bestätigen oder ablehnen können, und Angehörige müssen den Bearbeitungsstatus ihrer Besuchsankündigung in der App sehen können. Angehörige sollen wissen, ob ihr Besuch passt — das spart Telefonate. Fertig heißt: Pflegende können bestätigen oder ablehnen, der Status ist für Angehörige sichtbar, bei Ablehnung ist eine kurze Begründung Pflicht. Nur Pflegende der jeweiligen Einrichtung dürfen bestätigen oder ablehnen; Fristen gibt es keine. Angehörige sehen genau einen Status: angefragt, bestätigt oder abgelehnt.

**Agentenvorschlag:** Pflegende müssen angekündigte Besuche bestätigen oder ablehnen können, und Angehörige müssen den Bearbeitungsstatus ihrer Besuchsankündigung in der App sehen können; bei Ablehnung ist eine kurze Begründung Pflicht. Nur Pflegende der jeweiligen Einrichtung dürfen bestätigen oder ablehnen.

**Quelle und Einordnung:** Das Autor-Delta nennt Bestätigen/Ablehnen, sichtbaren Status, Begründungspflicht, Einrichtungsschranke, keine Fristen sowie genau einen der drei Zustände angefragt/bestätigt/abgelehnt.

**Beitrag des Agenten:** NEW_RELATED zum Besuchsthema ist gegenüber REQ-82/83 plausibel. Der Vorschlag behält zentrale Funktionen, lässt aber die exakte Statusmenge und die Aussage ohne Fristen weg.

**Entscheidung:** Historische Ablehnung: „Detail aus dem Diktat fehlt — Angehörige sehen genau einen Status aus: angefragt, bestätigt oder abgelehnt. Bitte vollständige Fassung neu aufnehmen.“

**Gespeicherte Wirkung:** Keine der Operationen dieses Ingests übernommen; Items, Relationen und Proposals im Ingest-Vorzustand und vor dem PBI-Schritt sind gleich. Keine unvollständige neue Besuchsanforderung entstanden.

**Grenzen:** Die Ablehnung schützt hier vor der Übernahme einer unvollständigen Vorlage. Das ist kein Nachweis allgemeinen Qualitätsgewinns. In den beiden erhaltenen Core-Snapshots fehlt eine neue REJ-Proposal dieses Falls; die Ablehnung ist in der separaten Entscheidungsdatei belegt. Nicht mit einer späteren Neuaufnahme gleichsetzen.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | teilweise getragen: Vorschlag stammt aus dem Diktat, lässt aber ausdrücklich genannte Details weg. | nicht anwendbar: Kein neuer Requirement-Inhalt übernommen. |
| K2 | getragen: Ergänzung zur vorhandenen Besuchsankündigung/-übersicht vertretbar. | nicht anwendbar: Abgewiesen. |
| K3 | teilweise getragen: Exakte drei Zustände und fehlende Fristen fehlen im Vorschlag. | nicht anwendbar: Keine neue Anforderung; Nichtübernahme unter K4. |
| K4 | — | getragen: Ablehnung und ausbleibende inhaltliche Übernahme an zwei Zuständen belegt. |
| K5 | — | nicht anwendbar: Nach Abweisung keine neue fachliche Verbindung zu erwarten. |

**Belege:** [Objektauszüge](dossiers/N03.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260818_183319_b3d6e6/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260818_183319_b3d6e6/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-pbi-update/applied/github-sync-delta.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260818_183319_b3d6e6/07-decision/decision-gate-decisions.json)

## N04: Besuchserinnerung auf 18 Uhr präzisiert

Rolle: additional; Lauf `20260819_102959_b8e8f8`; Eingangs-ID `AF-1`; Operation `REFINE`.

**Eingangstext:** Angehörige sollen einmal am Vortag um 18 Uhr per Push-Mitteilung an ihren bestätigten Besuch erinnert werden, damit Besuche nicht vergessen werden.

**Agentenvorschlag:** Angehörige sollen einmal am Vortag um 18 Uhr per Push-Mitteilung an ihren bestätigten Besuch erinnert werden, damit Besuche nicht vergessen werden.

**Quelle und Einordnung:** Autor-Delta ergänzt an der bereits vorgesehenen einmaligen Push-Erinnerung am Vortag ausschließlich die Uhrzeit 18 Uhr.

**Beitrag des Agenten:** REFINE auf REQ-88 benennt dieselbe Handlung und erhält Angehörige, bestätigten Besuch, Kanal, Häufigkeit und Zweck. Der PBI-Schritt markiert PBI-049 als geändert und schlägt die inhaltliche Angleichung vor.

**Entscheidung:** Ingest apply, anschließend PBI-/Alignment-Annahme. Die ausgefüllten edited-Felder enthalten dieselben Texte wie der Vorschlag; daraus folgt keine eigenständige menschliche Textverbesserung.

**Gespeicherte Wirkung:** REQ-88 v2 enthält 18 Uhr, die History erhält v1. PBI-049 wird mit passendem Zieltext und 18-Uhr-Akzeptanzkriterium als Projektionsobjekt ausgegeben. Bestehende covers- und Feature-Verbindungen bleiben; kein realer GitHub-Write (dryRun=true, would-create).

**Grenzen:** Ein vollständiger Core-Snapshot nach PBI-Apply fehlt innerhalb dieses Laufs. Die Inhaltsangleichung ist am erzeugten github-sync-delta belegt; damit werden nicht alle späteren Core-Metadaten oder ein externes Issue bestätigt. Frühere Kriterienliste wird kürzer; Akteur, bestätigter Besuch und Zweck stehen weiter im PBI-Zieltext.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Neu hinzugefügte Uhrzeit entspricht Autor-Delta. | getragen: Gespeicherte Anforderung führt aktuellen Lauf und frühere Textfassung. |
| K2 | getragen: Gleiche Anforderung wird präzisiert. | getragen: Gleiche REQ-ID, Version erhöht. |
| K3 | getragen: Alter fachlicher Inhalt und neue Uhrzeit erhalten. | getragen: Anforderung und nachgewiesenes PBI-Projektionsobjekt erhalten den ausgewählten Bedeutungsumfang. |
| K4 | — | getragen: Zwei Freigaben und ihre jeweilige Wirkung belegt; keine besondere Redaktionsleistung aus Formularfeldern abgeleitet. |
| K5 | — | getragen: Vorhandener PBI-/Feature-Bezug und inhaltlich angeglichene Projektionsausgabe belegt; externe Ausführung ausdrücklich nicht. |

**Belege:** [Objektauszüge](dossiers/N04.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260819_102959_b8e8f8/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260819_102959_b8e8f8/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/human-decisions.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-pbi-update/human-decisions.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-github/applied/github-forward-apply-report.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260819_102959_b8e8f8/07-decision/decision-gate-decisions.json)

## N05: PDF-Testkriterium am GitHub-Issue wird nicht Produktanforderung

Rolle: additional; Lauf `20260820_182614_a5f6b3`; Eingangs-ID `GH-43`; Operation `NEW`.

**Eingangstext:** Die Tages-Zusammenfassung der dokumentierten Medikamenten-Gaben muss als PDF exportierbar sein.

**Agentenvorschlag:** Die Tages-Zusammenfassung der dokumentierten Medikamenten-Gaben muss als PDF exportierbar sein.

**Quelle und Einordnung:** Archiviertes Issue 43 enthält zusätzlich „Testkriterium: Export der Übersicht als PDF möglich.“ Der Eingang stammt aus dem Issue-Text, nicht aus einem Kommentar.

**Beitrag des Agenten:** Der Agent bildet eine separate Exportanforderung. Die bestehende Anzeigeanforderung REQ-81 wird nicht verkürzt ersetzt; die Begründung nennt die sonst verloren gehenden Angaben. NEW ist vertretbar; NEW_RELATED wäre ebenfalls eine mögliche Einordnung.

**Entscheidung:** Autor lehnt ab: „Abnahme-Testzeile — kein echter Bedarf.“ Damit ist die fehlende fachliche Verbindlichkeit der Testzeile dokumentiert.

**Gespeicherte Wirkung:** Keine neue Requirement-Entität. REJ-018 unter core.proposals hält Vorlagetext, Grund, Incoming-ID und Planpfad fest; bestehende Items bleiben unverändert.

**Grenzen:** Dies ist eine Testeingabe und kein unabhängiger Feldbefund. Die Umformung zum verbindlichen Bedarf ist ohne die Gate-Entscheidung nicht legitimiert. Die Ablehnung belegt Kontrolle über Scope, nicht dass der Agent einen echten Bedarf korrekt erkannt hätte.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | teilweise getragen: PDF-Inhalt stammt aus dem Issue, Teststatus wird im normativen Vorschlag nicht erhalten. | nicht anwendbar: Nicht als neue Anforderung übernommen. |
| K2 | getragen: Separater Export statt verlustbehafteter Ersetzung der Anzeige ist fachlich vertretbar. | nicht anwendbar: Abgewiesen. |
| K3 | teilweise getragen: Exportinhalt erhalten; Testkennzeichnung fehlt. Bestehende REQ-81 bleibt im Plan unberührt. | nicht anwendbar: Keine neue Anforderung. |
| K4 | — | getragen: Ablehnung, unveränderte Items und REJ-018 mit Vorlagetext/Grund belegt. |
| K5 | — | nicht anwendbar: Keine aktive neue PBI-/Feature-Verbindung nach Abweisung. |

**Belege:** [Objektauszüge](dossiers/N05.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260820_182614_a5f6b3/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260820_182614_a5f6b3/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-pbi-update/applied/github-sync-delta.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260820_182614_a5f6b3/07-decision/decision-gate-decisions.json)

## N06: Mehrteilige Push-Frage nur teilweise als schon erfasst eingeordnet

Rolle: additional; Lauf `20260820_165512_73b1dc`; Eingangs-ID `GH-12`; Operation `ALREADY_DECIDED`.

**Eingangstext:** Es ist zu klären, ob Angehörige Push-Benachrichtigungen zur Wochenübersicht erhalten sollen und, falls ja, welche Ereignisse benachrichtigt werden sowie ob die Benachrichtigungen pro Angehörigem ein- und ausschaltbar sein sollen.

**Agentenvorschlag:** Es ist zu klären, ob Angehörige Push-Benachrichtigungen zur Wochenübersicht erhalten sollen und, falls ja, welche Ereignisse benachrichtigt werden sowie ob die Benachrichtigungen pro Angehörigem ein- und ausschaltbar sein sollen.

**Quelle und Einordnung:** Kommentar 5313971856 fragt nach Push zur Wochenübersicht, auslösenden Ereignissen und Ein-/Ausschaltbarkeit. DEC-006 nennt nur die Selbstdeaktivierung von Push-Erinnerungen.

**Beitrag des Agenten:** ALREADY_DECIDED wird mit einer enthaltenen Teilfrage begründet. Die Zielentscheidung trägt die Teilüberlappung, aber nicht das vollständige Fragenbündel. Das Vokabular bezeichnet hier eine bereits erfasste offene Entscheidung, nicht eine bereits fachlich beantwortete Frage.

**Entscheidung:** Historisch reject mit „System-Echo: dito, Frage ist als DEC-006 bereits erfasst.“ Heutige Erinnerung: „glauube aus test gründen verworfen“. Diese unsichere Erinnerung wird nicht zur bestätigten damaligen Absicht oder vollständigen Vorabdeckung aufgewertet.

**Gespeicherte Wirkung:** DEC-006 bleibt textlich unverändert; keine ausgewählte Operation angewandt. REJ-015 erhält den ganzen Vorschlag samt Ablehnungsgrund und Planpfad. Aus der Nichtübernahme folgt weder nachgewiesener Datenverlust eines echten Bedarfs noch vollständige Abdeckung aller Teilfragen.

**Grenzen:** Hintergrund der Test-/Echo-Einordnung nicht abschließend rekonstruierbar. Der archivierte Kommentar ist eine ausformulierte Frage, kein sichtbarer Verarbeitungsmarker. Nicht behaupten, der Mensch habe hier einen erkannten Semantikfehler korrigiert.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Vorschlag gibt die Frage sinngemäß wieder. | nicht anwendbar: Abgewiesen; vollständiger Vorlagetext im Ablehnungseintrag. |
| K2 | teilweise getragen: Nur eine Teilfrage im bezeichneten Ziel; vollständige Gleichsetzung nicht gedeckt. | nicht anwendbar: Keine Bestätigung am Ziel angewandt. |
| K3 | teilweise getragen: Vorschlagstext vollständig, vorgesehene Bestandszuordnung würde weitere Teilfragen nicht in den Entscheidungstext übernehmen. | nicht anwendbar: Abgewiesen; kein fachlich unvollständiger Ersetzungstext gespeichert. |
| K4 | — | getragen: Ablehnung und ausbleibende Zieländerung belegt; Qualitätsmotiv bleibt offen. |
| K5 | — | nicht anwendbar: Keine neuen fachlichen Relationen dieser abgewiesenen Operation. |

**Belege:** [Objektauszüge](dossiers/N06.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260820_165512_73b1dc/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260820_165512_73b1dc/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-pbi-update/applied/github-sync-delta.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260820_165512_73b1dc/07-decision/decision-gate-decisions.json)

## N07: Agentische Ableitung wächst nachvollziehbar in den Besuchsbestand

Rolle: additional; Lauf `20260821_204808_042823`; Eingangs-ID `CA-2`; Operation `NEW_RELATED`.

**Eingangstext:** Angehörige müssen eine bereits angefragte oder bestätigte Besuchsankündigung bis zum Beginn des Besuchs ändern oder absagen können; nach einer Änderung muss der Besuch erneut den Status „angefragt“ erhalten, und nach einer Absage darf keine Erinnerung mehr versendet werden.

**Agentenvorschlag:** Angehörige müssen eine bereits angefragte oder bestätigte Besuchsankündigung bis zum Beginn des Besuchs ändern oder absagen können; nach einer Änderung muss der Besuch erneut den Status „angefragt“ erhalten, und nach einer Absage darf keine Erinnerung mehr versendet werden.

**Quelle und Einordnung:** CoreAnalyst leitet Ändern/Absagen aus REQ-82/87/88 ab. Der Report, die ausgewählte Delta-Fassung und der spätere Core-Text stimmen überein. Das ist als „erschlossen, nicht gesagt“ gekennzeichnete neue Entwurfsregel, keine angebliche Transkriptaussage.

**Beitrag des Agenten:** Der Einordnungsagent prüft den Bestand und schlägt NEW_RELATED vor. PBIPlacement ordnet die Anforderung der bestehenden Ankündigungsaufgabe PBI-046 zu; Alignment ergänzt deren bisherigen Inhalt um Änderungs-/Absagefälle.

**Entscheidung:** Autor apply für CA-2, PBI-Tor/Alignment akzeptiert. Die eingetragenen PBI-Formulartexte entsprechen dem Agentenvorschlag; keine eigenständige menschliche Redaktion nachgewiesen.

**Gespeicherte Wirkung:** REQ-91 speichert Herkunft CoreAnalyst, Verarbeitungslauf und Ableitungsbegründung. PBI-046 v2 enthält alten Inhalt (vorab ankündigen, Datum/Uhrzeit) und neuen Inhalt (angefragt/bestätigt, bis Beginn, Änderung→angefragt, Absage→keine Erinnerung). Neue Kanten PBI-046 covers REQ-91 und REQ-91 part_of_feature FC-15 mit source=pbi-update stehen im Core-Snapshot vor Forward. Update von Issue 44 wird als tatsächlich ausgeführt protokolliert.

**Grenzen:** Die Regel ist plausibel und autorisiert, aber nicht die einzig mögliche Produktentscheidung. Aus Werkzeugausführung folgt keine vollständige Suche; aus diesem Einzelfall weder Vollständigkeit aller Relationen noch besserer Projektbetrieb. Externer Erfolg nach gespeichertem Ausführungsbericht, keine heutige Live-Abfrage.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Ableitung und Ausgangsanforderungen explizit benannt; keine falsche Quote-Herkunft. | getragen: Aktuelle Anforderung führt Verarbeitungs- und Ursprungsbezug sowie Ableitungsgrund. |
| K2 | getragen: Neue Regel ergänzt den vorhandenen Besuchslebenszyklus. | getragen: Separate REQ-91 im passenden vorhandenen Besuchsfeature. |
| K3 | getragen: Delta-Inhalt und alte Ankündigungsanforderung bleiben erhalten. | getragen: Gespeicherte REQ/PBI-Texte und Kriterien decken alle ausgewählten Einschränkungen. |
| K4 | — | getragen: Auswahl und Ausführung an Entscheidungen sowie Core-Nachzustand belegt. |
| K5 | — | getragen: Zwei neue fachlich passende Kanten, vorhandene Issue-Zuordnung und protokolliertes Issue-Update. |

**Belege:** [Objektauszüge](dossiers/N07.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260821_204808_042823/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260821_204808_042823/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260821_204808_042823/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260821_204808_042823/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260821_204808_042823/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260821_204808_042823/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260821_204808_042823/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/human-decisions.json](evidence/runs/fullworkflow/20260821_204808_042823/07-pbi-update/human-decisions.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260821_204808_042823/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260821_204808_042823/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260821_204808_042823/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/core-before.json](evidence/runs/fullworkflow/20260821_204808_042823/07-github/applied/core-before.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260821_204808_042823/07-github/applied/github-forward-apply-report.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260821_204808_042823/07-decision/decision-gate-decisions.json)

[Zusätzlicher Quellenbeleg: report.json](evidence/runs/core-analysis/20260821_203951_db6050/analysis/report.json)

[Zusätzlicher Quellenbeleg: delta-auswahl.json](evidence/runs/core-analysis/20260821_203951_db6050/analysis/delta-auswahl.json)

[Zusätzlicher Quellenbeleg: tool-calls.jsonl](evidence/runs/core-analysis/20260821_203951_db6050/logs/agents/CoreAnalystAgent/tool-calls.jsonl)

## N08: Lokale Speicherung: Konflikt erkannt, ursprüngliche Eingabeherkunft offen

Rolle: additional; Lauf `20260806_115016_01715f`; Eingangs-ID `MA-REQ-X1`; Operation `CONTRADICT`.

**Eingangstext:** Aus Datenschutzgruenden muessen alle Bewohnerdaten ausschliesslich lokal auf Geraeten der Einrichtung gespeichert werden; eine Cloud-Datenbank ist nicht zulaessig.

**Agentenvorschlag:** Aus Datenschutzgruenden muessen alle Bewohnerdaten ausschliesslich lokal auf Geraeten der Einrichtung gespeichert werden; eine Cloud-Datenbank ist nicht zulaessig.

**Quelle und Einordnung:** Das archivierte Delta fordert ausschließlich lokale Bewohnerdatenspeicherung und verbietet Cloud-Datenbanknutzung. Der ursprünglich konfigurierte temporäre Input fehlt. Übernommene Metadaten verweisen inhaltlich auf ein No-Go-Thema. Der Autor weiß nicht mehr, wie die Eingabe erstellt wurde.

**Beitrag des Agenten:** CONTRADICT auf die bestehende Firestore-Festlegung REQ-70 ist anhand des Delta-Texts vertretbar. Der Agent entscheidet den Konflikt nicht selbst.

**Entscheidung:** Ingest AcceptAll im historischen Versuch; anschließend Pause am Entscheidungstor. Keine menschliche Auflösung dokumentiert.

**Gespeicherte Wirkung:** REQ-70 bleibt geltend und textgleich. DEC-001 enthält die neue Aussage als Konflikt, Kante contradicts zeigt auf REQ-70. Die erhaltene Kette endet vor PBI/Forward.

**Grenzen:** Delta→Konflikt mechanisch und inhaltlich prüfbar, Ursprung vor Delta nicht. Nicht sicher als manuell erzeugten Test oder als vom System verursachten Quellenfehler klassifizieren. Keine fingierte semantische Stützung durch die sachfremde Claim-ID.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | nicht beurteilbar: Vorgelagerte Rohquelle fehlt, Metadaten sachfremd; Autor erinnert Entstehung nicht. | teilweise getragen: Konflikt trägt Delta-Text und Verarbeitungsbezug; kein tragfähiger vorgelagerter Inhaltsbeleg. |
| K2 | getragen: Cloud-Verbot stellt die bestehende Persistenzfestlegung infrage. | getragen: Offene Entscheidung statt automatischer Ersetzung. |
| K3 | getragen: Beide unvereinbaren Aussagen werden getrennt gehalten. | getragen: Alttext erhalten; neue Aussage vollständig in DEC-001. |
| K4 | — | getragen: Automatische Ingest-Annahme und tatsächliche Pause belegt, keine menschliche Konfliktentscheidung. |
| K5 | — | getragen: contradicts-Kante passt; Ende am Entscheidungstor, kein Folgezustand behauptet. |

**Belege:** [Objektauszüge](dossiers/N08.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260806_115016_01715f/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260806_115016_01715f/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260806_115016_01715f/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260806_115016_01715f/logs/events.jsonl).

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260806_115016_01715f/07-ingest/applied/affected-view.json)

## K01: Bekannte Kontrolle F1: REQ-42 vor Verkürzung geschützt

Rolle: known_control; Lauf `20260818_084712_765eea`; Eingangs-ID `REQ-01`; Operation `REFINE`.

**Eingangstext:** Freigegebene Angehörige müssen die Übersicht der Medikamenten-Einnahmen ihrer Bezugsperson sehen können.

**Agentenvorschlag:** Freigegebene Angehörige müssen die Übersicht der Medikamenten-Einnahmen ihrer Bezugsperson sehen können.

**Quelle und Einordnung:** Eingehende Aussage bekräftigt Angehörigensicht; Bestands-REQ-42 v3 regelt zusätzlich optionale Bemerkung mit maximal 200 Zeichen und nur bei Erfassung.

**Beitrag des Agenten:** REFINE würde den umfangreicheren Bestandsinhalt durch die Sichtbarkeitsaussage ersetzen. Die Teilüberlappung trägt keine vollständige Ersetzung.

**Entscheidung:** Human-UI: skip mit ausdrücklichem Verweis auf verdrängte Bemerkungs-Präzisierung.

**Gespeicherte Wirkung:** REQ-42 v3 bleibt zwischen Ingest-Vorzustand und vor PBI text-/versionsgleich; REJ-002 bewahrt Vorschlag und Begründung. PBI-028 wird im selben Lauf wegen einer anderen Operation (REQ-80) geändert.

**Grenzen:** Bekannter Kontrollfall, kein neuer Erfolg. Andere Änderungen desselben PBI nicht dieser abgewiesenen Operation zurechnen.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Sichtbarkeitsaussage durch Eingang getragen. | getragen: Bestehender Text und dessen Herkunft erhalten. |
| K2 | abweichend: REFINE ersetzt zu viel; bestehende Einzelanforderung enthält zusätzliche Regeln. | nicht anwendbar: Abgewiesene Zuordnung. |
| K3 | teilweise getragen: Sichtbarkeit bleibt, Bemerkungsregeln würden fehlen. | getragen: Nichtübernahme erhält Bestandsinhalt. |
| K4 | — | getragen: Begründete Ablehnung und Nichtverlust konkret belegt. |
| K5 | — | getragen: Keine Kantenwirkung dieser Operation; Vorlagetext und Grund als REJ-002 nachvollziehbar. |

**Belege:** [Objektauszüge](dossiers/K01.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260818_084712_765eea/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260818_084712_765eea/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260818_084712_765eea/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260818_084712_765eea/logs/events.jsonl).

[07-ingest/human-decisions.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-ingest/human-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/human-decisions.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-pbi-update/human-decisions.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/core-before.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-github/applied/core-before.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-github/applied/github-forward-apply-report.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260818_084712_765eea/07-decision/decision-gate-decisions.json)

## K02: Bekannte Kontrolle F2: autorisierte Medikationskonkretisierung

Rolle: known_control; Lauf `20260817_123342_f23f06`; Eingangs-ID `AF-1`; Operation `REFINE`.

**Eingangstext:** Freigegebene Angehörige sollen für ihre jeweilige Bezugsperson eine Übersicht der Medikamenten-Einnahmen der letzten 7 Tage einsehen können, damit telefonische Rückfragen im Haus ersetzt werden. Pro Eintrag werden nur der Status `verabreicht` oder `nicht verabreicht` sowie der Zeitpunkt angezeigt; Dosierungsdetails sind nicht sichtbar. Angehörige haben ausschließlich Leserechte und können Einnahmen weder bestätigen noch korrigieren.

**Agentenvorschlag:** Freigegebene Angehörige sollen für ihre jeweilige Bezugsperson eine Übersicht der Medikamenten-Einnahmen der letzten 7 Tage einsehen können; pro Eintrag werden nur Status (verabreicht/nicht verabreicht) und Zeitpunkt angezeigt, ohne Dosierungsdetails, und Angehörige haben hierbei ausschließlich Leserechte ohne Bestätigungs- oder Korrekturmöglichkeit.

**Quelle und Einordnung:** Autor-Delta legt den siebentägigen, ausschließlich lesenden Angehörigenzugriff mit Status/Zeitpunkt ohne Dosisdetails fest; alter Text nennt eine Kalenderprüfung und mögliche Medikamentenintegration.

**Beitrag des Agenten:** REFINE realisiert den konkreten neuen Medikamenteninhalt. Der zuvor ebenfalls genannte Prüfauftrag zum Kalender steht danach nur noch in der History.

**Entscheidung:** Ingest und PBI-Alignment angenommen. Edited-Texte stimmen mit Vorschlag überein; ausgefüllte Felder sind keine belegte inhaltliche Verbesserung.

**Gespeicherte Wirkung:** REQ-42 v2 und PBI-028 bewahren die neuen Einschränkungen; History erhält v1. Projektionsobjekt entspricht der Entscheidung; externer Write ist nur would-update.

**Grenzen:** Keine pauschale vollständige Erhaltung aller Altinhalte: Rücknahme/Erledigung des Kalender-Prüfauftrags ist in der Auswahlentscheidung nicht eigens begründet. Kontext und Herkunftsmetadaten des historischen REFINE unterliegen den bekannten Grenzen.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Neue fachliche Festlegung stammt aus Autor-Delta. | teilweise getragen: Neuer Inhalt belegbar; historische Herkunftsfelder nicht vollständig auf Autorquelle umgestellt. |
| K2 | teilweise getragen: Bezug auf Medikamentenfunktion plausibel, vollständige Ersetzung eines gemischten Alttexts nicht allein fachlich begründet. | teilweise getragen: Ersetzung tatsächlich autorisiert; Kalender-Teilfrage nicht gesondert entschieden. |
| K3 | teilweise getragen: Neue Einschränkungen vollständig, alter Kalender-Prüfauftrag fällt weg. | teilweise getragen: Neue Aussage erhalten; alte Kalenderfrage nur in History, keine bestätigte Fortgeltung oder Rücknahme. |
| K4 | — | getragen: Annahme und gespeicherte Wirkung belegt; daraus keine pauschale Qualitätswirkung des Menschen ableiten. |
| K5 | — | getragen: Angeglichener PBI-Inhalt und lokales Projektionsobjekt, kein externer Write. |

**Belege:** [Objektauszüge](dossiers/K02.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260817_123342_f23f06/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260817_123342_f23f06/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260817_123342_f23f06/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260817_123342_f23f06/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/human-decisions.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-pbi-update/human-decisions.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260817_123342_f23f06/07-github/applied/github-forward-apply-report.json)

## K03: Bekannte Kontrolle F3: Konfliktauflösung mit begrenzter Folgeangleichung

Rolle: known_control; Lauf `20260804_121433_eb181a`; Eingangs-ID `REQ-05`; Operation `CONTRADICT`.

**Eingangstext:** Es muss fachlich geklärt werden, ob No-Gos bewohnerübergreifend global für alle gelten sollen oder pro Bewohner geführt werden.

**Agentenvorschlag:** Es muss fachlich geklärt werden, ob No-Gos bewohnerübergreifend global für alle gelten sollen oder pro Bewohner geführt werden.

**Quelle und Einordnung:** Delta stellt globale gegenüber bewohnerbezogenen No-Gos zur Klärung. Der konkrete globale Zieltext stammt aus der späteren menschlichen Entscheidung, nicht aus angeblichem Konsens der Rohquelle.

**Beitrag des Agenten:** CONTRADICT macht die infrage gestellte Festlegung als offene Entscheidung sichtbar.

**Entscheidung:** Ingest automatisch; Entscheidungstor interaktiv mit ADOPT_NEW und neuem globalem Text im deklarierten Live-Beleg-Experiment.

**Gespeicherte Wirkung:** REQ-32 wird superseded, REQ-78 erhält menschlichen Text, DEC-001 resolved. PBI-012/023 werden auf REQ-78 umgehängt. Mindestens die PBI-Texte bleiben weiter klärungsbedürftig; keine vollständige semantische Folgeangleichung oder Live-Projektion.

**Grenzen:** Bekannte Kontrolle: Relationswechsel und vollständige Inhaltskonvergenz sind verschieden. Herkunft des menschlichen Zieltexts über DEC prüfen, nicht als schon vorher festgelegte Transkriptaussage.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Prüfung begrenzt auf archiviertes Delta und dokumentierten menschlichen Zieltext; keine neue Vollprüfung der Rohquelle. | getragen: Zieltext und sourceDecisionId im neuen Requirement belegbar. |
| K2 | getragen: Infragestellung bestehender Festlegung wird als Konflikt behandelt. | getragen: Auflösung entspricht ADOPT_NEW. |
| K3 | getragen: Alte und neue Position bis zur Entscheidung getrennt. | getragen: Autorisierten neuen Text übernommen und alte Fassung historisiert. |
| K4 | — | getragen: Automatischen Ingest und interaktive fachliche Auflösung ausdrücklich getrennt. |
| K5 | — | teilweise getragen: Supersedes-/covers-Wechsel belegt; PBI-Inhalt/Readiness nicht vollständig konsolidiert, needs_clarify und Dry-Run-Grenze. |

**Belege:** [Objektauszüge](dossiers/K03.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260804_121433_eb181a/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260804_121433_eb181a/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260804_121433_eb181a/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260804_121433_eb181a/logs/events.jsonl).

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-github/applied/github-forward-apply-report.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-decision/decision-gate-decisions.json)

[07-decision/applied/decision-apply-report.json](evidence/runs/fullworkflow/20260804_121433_eb181a/07-decision/applied/decision-apply-report.json)

## K04: Bekannte Kontrolle: GitHub ergänzt den 14-Tage-Besuchszeitraum

Rolle: known_control; Lauf `20260820_172043_802e63`; Eingangs-ID `GH-45`; Operation `NEW_RELATED`.

**Eingangstext:** Die Übersicht der angekündigten Besuche soll die Besuche der nächsten 14 Tage anzeigen.

**Agentenvorschlag:** Die Übersicht der angekündigten Besuche soll die Besuche der nächsten 14 Tage anzeigen.

**Quelle und Einordnung:** Kommentar 5359314065 schlägt die nächsten 14 Tage für die Besuchsübersicht vor. Seine Übernahme erhält den Vorschlagsinhalt; die Verbindlichkeit entsteht durch Annahme.

**Beitrag des Agenten:** NEW_RELATED lässt die bestehende REQ-83 für die Pflegeübersicht stehen; PBI-047 wird um REQ-89 ergänzt.

**Entscheidung:** Ingest- und PBI-Annahme; weiteres Wetterthema ausdrücklich abgewiesen. Keine zusätzliche menschliche Textverbesserung allein aus den edited-Feldern.

**Gespeicherte Wirkung:** REQ-89, covers von PBI-047, Feature-Bezug zu FC-15 und inhaltlich erweiterter PBI belegt. Tatsächliches Update von Issue 45 protokolliert.

**Grenzen:** Bereits bekannter Kontrollfall. Quelle, Autorisierung und technische Ausführung sind getrennte Belege; kein Vollständigkeitsnachweis für beliebige GitHub-Kommentare.

| Kriterium | Vorschlag / Bezug | Gespeichertes Ergebnis / Bezug |
|---|---|---|
| K1 | getragen: Kommentarinhalt und vorgeschlagene Ergänzung passen. | getragen: Quellenkommentar und Verarbeitungslauf sind zuordenbar. |
| K2 | getragen: Ergänzung einer bestehenden Übersicht ohne Verlust der Grundanforderung. | getragen: Separate Anforderung und passender PBI-Bezug. |
| K3 | getragen: Bisherige Übersicht für Pflegende plus neuer Zeitraum erhalten. | getragen: Gespeicherter PBI enthält beide Inhalte. |
| K4 | — | getragen: Annahmen und Ausführung dokumentiert. |
| K5 | — | getragen: Neue covers-/Feature-Kanten und ausgeführtes Issue-Update belegt. |

**Belege:** [Objektauszüge](dossiers/K04.json), [Eingangsobjekt](evidence/runs/fullworkflow/20260820_172043_802e63/04-delta/project-state.json), [Vorschlagsplan](evidence/runs/fullworkflow/20260820_172043_802e63/07-ingest/plan.json), [Vorbestand](evidence/runs/fullworkflow/20260820_172043_802e63/07-ingest/applied/core-before.json), [Ausführungsereignisse](evidence/runs/fullworkflow/20260820_172043_802e63/logs/events.jsonl).

[07-ingest/ingest-gate-decisions.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-ingest/ingest-gate-decisions.json)

[07-ingest/applied/affected-view.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-ingest/applied/affected-view.json)

[07-pbi-update/pbi-change-plan.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-pbi-update/pbi-change-plan.json)

[07-pbi-update/human-decisions.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-pbi-update/human-decisions.json)

[07-pbi-update/applied/core-before.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-pbi-update/applied/core-before.json)

[07-pbi-update/applied/pbi-update-apply-report.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-pbi-update/applied/pbi-update-apply-report.json)

[07-pbi-update/applied/github-sync-delta.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-pbi-update/applied/github-sync-delta.json)

[07-github/applied/core-before.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-github/applied/core-before.json)

[07-github/applied/github-forward-apply-report.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-github/applied/github-forward-apply-report.json)

[07-decision/decision-gate-decisions.json](evidence/runs/fullworkflow/20260820_172043_802e63/07-decision/decision-gate-decisions.json)

