# Gegenprüfung: vorhandene vollständige Workflow-Läufe

Stand: 15.09.2026. **Ein historischer Gesamtdurchlauf vom Transkript bis zur realen GitHub-Projektion ist vorhanden: `20260818_084712_765eea`.** Er ist bereits der F1-/Besuchsbeleg in der Thesis. Eine Aussage, es gebe grundsätzlich keinen vollständigen Kettenlauf, wäre falsch. Die bisher diskutierte Nachweislücke betrifft den später veränderten aktuellen Codebestand; diese Einschränkung darf den vorhandenen historischen Integrationsnachweis nicht verdecken.

## Umfang der Nachprüfung

Alle **143** Verzeichnisse unter `runs/fullworkflow` und ihre globalen Ereignisdateien wurden inventarisiert, ohne Auswahl nach Änderungsdatum oder Beschränkung auf die jüngsten Läufe. Keine fehlende Ereignisdatei, kein JSON-Lesefehler. 113 Verzeichnisse enthalten ein terminales `PIPELINE_RUN_DONE` mit Exit 0. Das ist **keine Erfolgsquote vollständiger Ende-zu-Ende-Verarbeitung**: Eingänge, Entwicklungsstände, Teilabläufe und Außenwirkungen unterscheiden sich. Für `765eea` wurden zusätzlich Entscheidungen und gespeicherte Anwendungsergebnisse gelesen. Keine Neuausführung und keine neue semantische Vollbewertung.

[Reproduzierbare Inventur](./P10-E2E-Bestandspruefung-2026-09-15/inspect.py) · [Ergebnis mit Dateiprüfsummen und Ereignisankern](./P10-E2E-Bestandspruefung-2026-09-15/inventory.json).

## Was `765eea` tatsächlich trägt

Die folgenden Zeilen beziehen sich auf `runs/fullworkflow/20260818_084712_765eea/logs/events.jsonl`:

| Teil der Kette | Konkreter Nachweis |
|---|---|
| Gemeinsamer Graph, Transkripteingang | Zeilen 1–4: `u2-ein-graph`, `entry: transcript`, Start und erfolgreicher Abschluss der Ledger-Stufe. |
| Evidenzentscheidung und Ableitung | Zeilen 5–10: Adjudikationsanfrage, Pause, Antwort, Fortsetzung; danach drei Baseline-Artefakte. |
| Betriebspfad | Zeilen 12–13: vorhandener Core mit 201 Items, operational-Zweig. Dieser Lauf ist kein Bootstrap aus leerem Bestand. |
| Anforderungen und Architektur | Zeilen 81–86 und 137–142: Gates, Antworten und Anwendung. Acht freigegebene Anforderungsoperationen, anschließend eine Architekturoperation. Der abschließende Ingest-Bericht enthält zusammen neun Ergebniszeilen; acht Entscheidungen und neun aggregierte Ergebnisse sind unterschiedliche Bezugsgrößen. |
| Rollen und ADR | Zeilen 172–177 und 207–212: Freigaben, eine Klassifikation und eine abgelegte ADR. 17 weitere ADR-Vorschläge wurden zurückgestellt. |
| Entscheidungsprüfung | Zeilen 213–215: fünf offene Entscheidungen gesammelt zurückgestellt. Keine inhaltliche Auflösung dieser fünf Entscheidungen behaupten. |
| Arbeitspakete | Zeilen 294–303 sowie Apply-Bericht: beantwortetes PBI-Gate, drei neue und vier aktualisierte PBIs; Bericht mit zwölf ergänzten Beziehungen. Die acht PBI-Operationen wurden per dokumentierter Sammelfreigabe angenommen. |
| Reale Projektion und reguläres Ende | Zeilen 313–317 und GitHub-Apply-Bericht: Ausführung freigegeben; `dryRun:false`, `executed:true`, `success:true`. Drei Issues erzeugt (#43–#45), zwei aktualisiert, zwei Operationen bereits angewandt. Danach Pausenzeiger gelöscht und Exit 0. |

**Durabilitätsbeobachtung:** Sieben tatsächliche Pausen an sieben unterschiedlichen Gates; neun Resume-Ereignisse, darunter zwei erfolglose Fortsetzungsversuche, die weiter `PIPELINE_STILL_PAUSED` melden. Acht `GATE_ANSWERED`-Ereignisse, weil zusätzlich die Entscheidungsprüfung mit Sammel-Zurückstellung antwortet, ohne einen weiteren Pausezyklus. Eine Fortsetzung nach Ende eines eigenständigen Prozesses wird aus diesen Ereignissen allein nicht neu behauptet.

**Korrekturen an den Begleitzahlen des Kollegen:** Im Checkpoint-Verzeichnis liegen 70 Checkpoint-Dateien und eine Indexdatei, insgesamt 71 Dateien. Von elf Stufenverzeichnissen sind zehn befüllt; `06-backlog` bleibt leer, da hier die Fortschreibung über `07-pbi-update` läuft. Diese Zählkorrekturen ändern den positiven Durchlaufbefund nicht. Die vier hervorgehobenen Apply-Ereignisse sind außerdem keine vollständige Zählung aller Zustandswirkungen: PBI-Änderungen und GitHub-Projektion kommen hinzu.

Der Lauf und die bereits vorhandenen Feldvergleiche zum Besuchsfall belegen einen integrierten historischen Verarbeitungspfad. Nicht abgedeckt sind dadurch alle Eingangsarten, alle Entscheidungszweige, die vollständige fachliche Richtigkeit sämtlicher erzeugter Inhalte oder eine allgemeine Zuverlässigkeit.

## Weitere Läufe und die Septemberfälle

Vom 20.–22.08. enthält die Inventur 16 regulär beendete Delta-Einstiege mit `GITHUB_FWD_DONE`; weitere Läufe starten über Reprojektion oder Klärung. Diese Inventurzählung enthält unterschiedliche Betriebsmodi und ersetzt keine Einzelprüfung realer Schreibwirkungen. Sie ist keine Serie von Transkript-zu-GitHub-Läufen und kein Stabilitätsbenchmark.

Der frühere Septembervergleich bleibt wie bereits korrigiert:

- `20260909_190518_f58d6a`: Facettenvervollständigung und Baseline-Ableitung ausgeführt, anschließend Pause am Ingest-Gate; keine Anwendung dokumentiert.
- `20260909_190157_506d47`: nach der Facettenvervollständigung bereits `HumanReview` aus der Baseline-Stufe, terminaler Exit 3. Die erneute Aussage des Kollegen, beide Septemberläufe blieben am Ingest-Tor stehen, ist falsch.
- `20260822_215220_57ea6b`: vorhandener Ingest-Apply-Bericht mit null Operationen; der jüngste nichtleere Ingest-Bericht ist `20260822_160709_aa313a`.

## Folgerung für Abgabestand und zusätzliche Evaluation

**Die Wahl lautet nicht zwingend „alles auf August zurücksetzen oder einen neuen Lauf machen“.** Die Arbeit unterscheidet bereits historische Evaluationsstände, nachträgliche Prüfungen und den beschriebenen späteren Artefaktstand. Diese Zuordnung kann beibehalten werden, sofern die Reichweite je Aussage stimmt. Ein bloß gesetztes August-Datum rekonstruiert zudem noch keinen ausführbaren historischen Code-/Konfigurationsstand.

Spätere Änderungen sind tatsächlich relevant. Die Facetten-Vervollständigung wurde nach dem Augustlauf eingebunden und besitzt einen gesonderten Ausführungsbeleg. Die Sprechersegmentierung verändert das F1-Transkript von 13 auf 17 Units; deshalb ist der alte Lauf kein unveränderter Wiederholungsnachweis der heutigen Front. R-75 betrifft den Ausgang von Baseline-Fehler-/HumanReview-Fällen. [Die Übernahmeprüfung vom 11.09.](./Technische-Uebernahme-R75-B20-2026-09-11.md) dokumentiert gezielte Regressionen, die Suite 701/701 und den Offline-Smoke 14/0; dieser Smoke überspringt die Transkriptfront. Die Resultate sind datierte Nachweise, keine hier erneut ausgeführten Tests. Der zusätzliche F-Arm ist die Vergleichsbedingung und kein notwendiger Verarbeitungsschritt des produktiven Gesamtgraphen.

**Nächster Schritt:** Vorhandenen E2E-Beleg in der Nachweisbilanz ausdrücklich erhalten. Die späteren für die Kernkette relevanten Änderungen ihren betroffenen Aussagen und vorhandenen Tests zuordnen. Nur wenn danach eine tragende gegenwärtige Funktionsaussage ohne passenden Nachweis bleibt, einen genau darauf begrenzten neuen Versuch vorschlagen. Der breite Integrationskandidat wird bis zu dieser Entscheidung zurückgestellt; sein Nutzen ist nicht mehr mit der vermeintlichen Abwesenheit eines historischen Gesamtlaufs zu begründen.

Eine etwaige nachträgliche vertiefte Bewertung von `765eea` wäre eine Nachauswertung des bekannten Falls, keine neue unabhängige Replikation. Sie muss einen über die bereits durchgeführten F1-/Provenienzprüfungen hinausgehenden Erkenntnisgewinn benennen. Die Leser-/Methodikprüfung und die Klärung von Material und Abgabestand werden fortgesetzt.

**Änderungsumfang:** dieser Bericht, seine Inventur und der aktive Abschlussplan. Keine Änderung an Code, Runs, TeX, PDF oder Grafiken; kein zusätzlicher Overleaf-Sync.
