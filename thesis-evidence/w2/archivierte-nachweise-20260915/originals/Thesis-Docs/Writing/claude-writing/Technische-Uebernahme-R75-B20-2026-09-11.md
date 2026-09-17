# Übernahme R-75 und B-20

Stand: 11.09.2026. **Beide freigegebenen Korrekturen sind im Originalprojekt übernommen. Vollständige Testsuite 701/701, Offline-Smoke 14/0; Build ohne Warnungen oder Fehler.** B-03/R-75 und B-20 sind im beschriebenen technischen Umfang geschlossen. Die drei TeX-Nachträge sind im PDF vom 11.09. bestätigt; die D.6-Tabelle ist vollständig lesbar. Die Abnahme ist im M10-Bericht dokumentiert.

## Änderungen und Gegenprüfung

- **R-75:** `[YieldsOutput(typeof(string))]` ergänzt den Baseline-Executor. Das vorhandene `WithOutputFrom(front.Baselines)` bleibt erhalten. Beide Deklarationen sind nötig: erlaubter Ausgabetyp am Executor und registrierter Ausgang des Workflows. Fehlendes Ledger sowie HumanReview/MaxIterationsReached liefern in den gezielten Tests eine terminale Meldung ohne Typfehler und ohne Delta-Weitergabe. HumanReview bleibt ein dokumentierter Abschluss, keine neu eingeführte Anfrage.
- **Testzugang geprüft:** Der optionale interne Delegate fällt bei `null` unmittelbar auf `RecipeRunner.ExecuteAsync` zurück. Reihenfolge und Werte aller sechs Argumente sind unverändert: Rezept, Pfad, bereinigtes Baseline-Modell, `false` für dryRun, Ledger-Settings und Repo-Root. Der einzige produktive Konstruktoraufruf übergibt weiterhin nur die bisherigen vier Argumente. Der Fehlendes-Ledger-Test verwendet den echten Default-Aufruf; die Checker-Fälle verwenden vorbereitete Rezept-Ergebnisse und lesen echte Reportdateien. Kein geändertes Produktionsrouting, keine veränderte Modellkonfiguration, kein Prompt-Eingriff.
- **B-20 / R-76:** Der Sprecherparser akzeptiert Bindestriche zwischen Buchstabenfolgen im Format `Name:`. Im F1-Originaltranskript werden nun vier Beiträge der Angehörigen-Vertreterin getrennt: 17 statt 13 Units; Besuchsplanung bei neuer Segmentierung AU-0016. Die übrigen fünf geprüften Transkripte einschließlich beider W2-Eingaben liefern vollständig identische serialisierte Units. Historische AU-0012, Belegtexte und Messungen bleiben unverändert.

Übernommen wurden **zwei Anwendungsdateien und drei Testdateien** aus dem zuvor geprüften Paket. Der [Code-Diff](./Technische-Uebernahme-R75-B20-2026-09-11/uebernommener-code.patch) zeigt alle Änderungen. Das Ledger-README beschreibt zusätzlich das unterstützte Rollenformat und die Konsequenz für positionale IDs.

## Vollständige Testsuite und Smoke

Die Tests liefen in einer frischen isolierten Kopie des aktuellen Quellstands einschließlich vorhandener Autorenänderungen. Nach erfolgreicher Prüfung wurden exakt diese fünf Dateien in das Originalprojekt übernommen und ihre Prüfsummen verglichen. Der übrige erfasste Bestand blieb unverändert. Keine Übernahme alter Dateien über zwischenzeitliche Änderungen hinweg.

1. Prozessprüfung: keine übrig gebliebenen `dotnet test`-/`testhost`-Prozesse. Der ältere MSBuild-Prozess war dem laufenden Rider zugeordnet und wurde nicht pauschal beendet. Standard-Buildserver geordnet heruntergefahren.
2. Offline-Restore aus dem vorhandenen Paketcache. **Ein Build** von `AgenticSdlc.Tests` samt Host/HumanReview mit `--no-restore -m:1 -p:UseSharedCompilation=false -nodeReuse:false`: **0 Warnungen, 0 Fehler**. Das separate MCP-Server-Projekt war nicht betroffen und wurde nicht zusätzlich gebaut.
3. Vollständige Testsuite mit `--no-build --no-restore`: **701 bestanden, 0 fehlgeschlagen, 0 übersprungen**. Darin enthalten sind die acht neuen Fälle und sämtliche 693 vorhandenen Fälle. [TRX-Ergebnis](./Technische-Uebernahme-R75-B20-2026-09-11/full-suite.trx), [Build-Protokoll](./Technische-Uebernahme-R75-B20-2026-09-11/build.log).
4. Vorhandener `tools/smoke-hitl.sh` in der isolierten Kopie: **14 PASS / 0 FAIL**. Nur seine Build-Voraussetzung wurde angepasst: Die bereits erfolgreich gebauten Binärdateien wurden per Prüfsumme bestätigt; kein zweiter Build. Alle CLI-Aufrufe, Fixtures und funktionalen Assertions blieben wortgleich. [Transparenter Anpassungs-Diff](./Technische-Uebernahme-R75-B20-2026-09-11/smoke-prebuilt-adaptation.patch), [Smoke-Ausgabe](./Technische-Uebernahme-R75-B20-2026-09-11/smoke.log).

Der Smoke prüft die leeren Ingestion-/PBI-/Forward-Gates, den Decision-Pause-/Resume-Weg und den tatsächlichen Ein-Graph-Einstieg `--from-delta` mit Pause am Decision-Gate und Fortsetzung. Er läuft mit kopiertem Core, deaktivierten Außenwirkungen, ohne originale `.env`, mit Dummy-Schlüssel und Modell-Endpunkten auf Loopback-Port 9. **Core (237 Items) und Testkonfiguration sind anschließend bytegenau identisch zum vorherigen Stand.** Auch die gebauten Binärdateien blieben unverändert. Produktiver Core und produktive Konfiguration wurden nie vom Smoke beschrieben. [Prüfsummen und Smoke-Protokoll](./Technische-Uebernahme-R75-B20-2026-09-11/smoke-protocol.json).

**Prüfgrenzen:** Der Smoke überspringt die Transkript-/Ledger-/Baseline-Front. R-75 wird zusätzlich durch die echten Executor-/MAF-Stufentests abgesichert, B-20 durch Parsertests einschließlich des Originaltranskripts. Das ist keine vollständige neue Ende-zu-Ende-Ausführung des Transkripteingangs, keine W2-Wiederholung und kein genereller Wirksamkeits- oder Zitattreuenachweis. Der neue Code belegt Implementierung und getestetes Verhalten; die historische Evaluation bleibt an ihre damaligen Stände gebunden.

## Bereits bestätigter Overleaf-Sync: drei Dateien

| Lokale Datei | Overleaf-Ziel | Änderung |
|---|---|---|
| `chap5/chap5.9/chap5.9.tex` | gleicher Kapitelort | Segmentierungsursache beim historischen Besuchsbeispiel knapp erklärt; spätere Korrektur ändert dessen Beleg nicht. |
| `kapitel-7/tex-v2/chap7.8/chap7.8.tex` | bisheriger Kapitelort für 7.8 | Die beiden Korrekturen vom 11.09. als gesondert technisch geprüften späteren Stand abgegrenzt. |
| `anhang/anhang-evaluation.tex` | bisheriger Anhangort | Eine neue D.6-Belegzeile: 8 Regressionstestfälle, Suite 701/701, Smoke 14/0 samt Prüfgrenzen. |

Keine Tabellen-Einzeldatei, Bib-, PDF-Grafik- oder Draw.io-Datei synchronisieren. Die D.6-Tabelle steht in der Anhangdatei selbst. Der Quellenfall und seine historischen Kennungen werden nicht durch neu segmentierte IDs ersetzt. Kein neuer Textabschnitt zur gesamten Systemdokumentation.

Arbeitsplan, Befundliste, R-75/R-76 im E2E-Runbook und technischer Statusnachtrag sind aktualisiert. Der ältere Prüfbericht bleibt als ausdrücklich historischer Stand erhalten. B-29/B-30 bleiben nachrangig. Nach dem Sync die drei Textstellen und die verlängerte D.6-Tabelle im neuen PDF abnehmen, dann zur geplanten Thesis-Überarbeitung zurückkehren; Einleitung und Kurzfassung behalten ihren geschützten Zeitrahmen.

[Maschinenlesbares Übernahmeprotokoll](./Technische-Uebernahme-R75-B20-2026-09-11/uebernahmeprotokoll.json).


**PDF-Abnahme abgeschlossen, 11.09.2026:** 5.9, 7.8 und D.6 sind im Export mit SHA-256 `198513aecbb9ab9120672ac161777e8424808c642baf5833fc88abc5392ac823` bestätigt. Kein erneuter Sync allein aus dieser historischen Dreierliste. D.6 gehört wegen einer weiteren, getrennten Präzisierung des Resume-Belegs erneut zum M10-Paket; die technische Nachtragszeile bleibt wortgleich. [M10-Abnahme und aktuelle Sync-Liste](./M10-Umsetzung-2026-09-11.md).
