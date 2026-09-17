# R3 – Beleg- und Standabgleich

Stand: 16.09.2026. Erneute Prüfung vorhandener Daten und aktueller Implementierung, **keine neue Systemausführung oder Inhaltsannotation**. Dieser Index ergänzt das vorhandene Belegpaket; er ersetzt keine historischen Originale.

## Ergebnis

- Alle **143** Verzeichnisse unter `runs/fullworkflow` anhand von Ereignissen und Konfiguration inventarisiert, ohne Datumsstichprobe; keine fehlenden Ereignisdateien oder JSON-Lesefehler. 113 terminale Exit-0-Ereignisse sind **keine** Erfolgsquote vollständiger Ketten. Unterschiedliche Eingänge, Testmodi und Teilabläufe bleiben getrennt.
- Der vollständige historische Transkript-zu-GitHub-Fall `20260818_084712_765eea` bleibt gültig. Der jüngste geeignete solche Fall in der Inventur stammt vom 18.08. Für den nachträglich korrigierten Stand ist kein gemeinsamer Gesamtdurchlauf vorhanden. Die Septemberläufe zeigen Teilstrecken: `f58d6a` endet an der Ingest-Pause, `506d47` mit Baseline-HumanReview (Exit 3).
- **685/685** Dateien des dokumentierten Testbestands vom 11.09. sind bytegleich; keine neuen Dateien innerhalb der zusätzlich abgeglichenen Host-/HumanReview-/Test-Verzeichnisse mit den damals erfassten Endungen. Das ist ein Standvergleich, keine erneute Suite und keine Rekonstruktion des historischen Laufstands. Arbeitskopie einschließlich unversionierter Bestandteile bleibt vom HEAD zu unterscheiden.

[Inventur](./fullworkflow-inventory.json) · [Kurzbilanz](./summary.json) · [Dateiabgleich](./current-tested-comparison.json) · [ausgewählte Ereignisse und Entscheidungs-/Apply-Dateien](./governance-artifacts.json) · [gelesene Quellen mit Prüfsummen](./source-manifest.json).

## Nachweiszugänge und Rekonstruktionsgrenzen

| Aussage / Stand | Primärzugang im bereitgestellten Paket | Was genau gesichert ist |
|---|---|---|
| Formative A10-Herleitung | [Steward-Vorlauf A2/A3](./originals/Thesis-Docs/aktiv/steward/steward-vorlauf.md), [Iterationsnotizen I-3 bis I-5](./originals/Thesis-Docs/aktiv/steward/steward-iteration-notes.md) | Vollständige unveränderte Kopien der heutigen Notizdateien mit datierten historischen Abschnitten. Kein behaupteter Versions-Snapshot vom 07.08.; Planungen und Eigenbewertungen sind keine Laufbelege oder Messungen des Bedienaufwands. Die dortigen A2/A3 sind lokale Arbeitsschritte, nicht Thesis-Anforderungen. |
| Formative Fälle allgemein | [19 Evidenzanker](../chapter-4/README.md) | Ausgewählte Originale und deklarierte Ersatz-/Teilbelege; keine nachträglich prospektive Messkampagne. |
| Hauptvergleich, Quellen und Referenz | [Archiv mit Laufindex](../w2/archivierte-nachweise-20260915/README.md), [Quellen](../w2/archivierte-nachweise-20260915/originals/input/transcripts/), [Referenzstände und Prüfsummen](../w2/archivierte-nachweise-20260915/originals/input/eval-labels/) | Laufkonfigurationen, Modellausgaben, 31/109-Gold-v2-Referenz, dokumentierte Änderungen. Laufkonfigurationen enthalten nicht notwendig alle Umgebungswerte oder genaue Anbieterrevisionen. |
| Prompts und Komponenten des Hauptvergleichs | [Analysebestand vom 07.09.](../w2/archivierte-nachweise-20260915/originals/Thesis-Docs/Writing/claude-writing/kapitel-7/05-vergleichs-inspektion/00-LESEFADEN.md) und die darin zugeordneten Kopien | Gesicherte Analysefassung und laufbezogene Protokolle. Kein vollständiger, unverändert ausführbarer Quellcode-Snapshot zu jedem historischen Lauf nachgewiesen. Neu gesicherte Prompts würden diese Grenze nicht rückwirkend schließen. |
| Aktuelle Urteilsstände | [Konsolidierung vom 16.09.](../w2/bewertungskonsolidierung-20260916/README.md), [Ledger-Wiederholungen](../w2/ledger-repetitions-20260915/README.md), [Core-Inhalte](../w2/core-content-audit-20260915/evaluation/README.md), [Analyst/PBI](../w2/agent-artifact-evaluation-20260916/evaluation/README.md) | Die Thesis verwendet diese datierten Bewertungsfassungen; historische Zwischenberichte bleiben unverändert, auch wenn frühere Urteile später präzisiert wurden. Keine unabhängige menschliche Vollannotation. |
| F1–F5 und technische Nachträge | [Archivierte Laufordner](../w2/archivierte-nachweise-20260915/originals/runs/fullworkflow/), [Übernahmebericht vom 11.09.](../w2/archivierte-nachweise-20260915/originals/Thesis-Docs/Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11.md) | Entscheidungen, Ereignisse, Apply-Berichte und ausgewählte Core-Zwischenstände; TRX 701/701 und Smoke 14/0 bleiben datierte Prüfungen. `506d47` ist ergänzend hier mit Konfiguration/Ereignissen archiviert. |
| Bestätigungsmarker / aktueller Code | [CoreStatus](./originals/AgenticSdlc.Host/FullWorkflow/04-delta/CoreStatus.cs), [CoreViews](./originals/AgenticSdlc.Host/FullWorkflow/05-core/CoreViews.cs), weitere [Codekopien](./originals/AgenticSdlc.Host/FullWorkflow/) | Implementierte Status- und Auswahlregeln, keine fachliche Wirksamkeit und kein Beweis tatsächlicher menschlicher Einzelentscheidung. |

## Bestätigungsmarker: geprüfte Wirkung

`CoreStatus.From` setzt bei `baseline` aktive Gültigkeit plus `Agent`, bei `accepted` aktive Gültigkeit plus `Human`. `ProjectStateBuilder` erzeugt Baseline-Items; `CoreSeeder` erhält deren Status. `ShowAllRequirementRetriever`, `CoreViews.ActiveBacklog` und `AnalystCollect` verwenden Gültigkeit/Archivierung, nicht `ConfirmedBy` als Zulassung. `CoreBacklogSeeder` übernimmt gültige verknüpfbare Core-Referenzen ohne Human-Filter; `CoreViews.GithubSync` und der Forward-Vorfilter behandeln Blockaden/Mapping, nicht den Bestätigungsmarker als allgemeine Freigabesperre.

Neue Übernahmen in `IngestionApply.NewRequirement` bekommen `accepted` ohne Unterscheidung, ob die Annahme interaktiv oder automatisch erzeugt wurde. Der GateResponder kann dieselbe Apply-Strecke durch AcceptAll bedienen. Deshalb ist der Marker eine deklarierte Bestätigungsart, kein Authentifizierungsnachweis und keine über alle Schritte fortgeschriebene vollständige Freigabehistorie. Aussagen über echte menschliche Autorisierung benötigen Entscheidungsdatei, Ereigniszuordnung und tatsächliche Wirkung. Keine separate Autorisierungsquote aus dem Marker berechnet.

## Governance: maßgebliche Präzisierungen

- **F1:** zwölf Anforderungsoperationen; acht angewandt, vier begründet abgewiesen. PBI und Forward sind menschliche Sammelfreigaben im UI, kein automatischer AcceptAll-Modus und kein Nachweis inhaltlicher Einzelprüfung. Das Forward-Ereignis nennt `execute:true`; der Übergang von der Startkonfiguration `false` wird nicht gesondert archiviert.
- **F2:** interaktive Ingest-Freigabe über Steward-Chat und PBI-Angleichung über UI. Bereits in der Vorbewertung korrigiert: Die `edited…`-Felder enthalten die übernommenen Vorschläge, keine eigenständige menschliche Textredaktion. Dry-Run.
- **F3:** nur Entscheidungs-Gate menschlich, vier andere Gate-Ereignisse `AcceptAll`. Der gespeicherte `policyProfile:Interactive` der Startkonfiguration ist allein unzureichend; die Gate-Ereignisse belegen den tatsächlichen Modus.
- **F4:** Entgegen der verkürzten älteren Fallnotiz nicht ausschließlich leere Tore: Entscheidungen und Ereignisse zeigen drei angenommene Anforderungsoperationen und zusätzlich eine Architektur-Bestätigung. Die Anforderungs-Delta-Datei enthält drei Operationen, der zusammengeführte Bericht zusätzlich ARCH-S1. Erst die PBI-Stufe bleibt ohne Operationen. Fachliche Texte wurden hier nicht neu vollständig verglichen.
- **F5b:** Der konkrete 14-Tage-Eingang ist `802e63`; `6e75fa` ist die folgende Team-Antwort. Beide besitzen interaktive Gate-Ereignisse, Entscheidungsdateien und reale Forward-Berichte. `b7747b` enthält einen ausdrücklich fehlgeschlagenen Vermerk; daraus keine ungebrochene Erfolgskette machen.

**Widerspruch in F1-Dateien:** `07-decision/decision-gate-decisions.json` enthält zwei Auflösungsantworten, das globale Ereignis in Zeile 214 meldet dagegen fünf zurückgestellte Entscheidungen. Ohne passenden Apply-Nachweis werden diese Antworten nicht als damals ausgeführte Auflösungen gewertet. Die verfolgte DEC-004 bleibt in beiden Belegen zurückgestellt. Keine ungesicherte Ursache (etwa spätere Überschreibung) unterstellen. Die bereits geprüften Ingest-, REQ-42- und Projektionsbelege bleiben davon getrennt gültig.

## Nachrechnen und Offenbleibendes

`inspect.py --repo <Repository> --out <neuer Ausgabeordner>` inventarisiert die vorhandenen Dateien ohne Systemausführung. Es benötigt die Arbeitskopie einschließlich der historischen Runs; die bereitgestellten JSON-Ergebnisse und Primärbelege lassen sich zusätzlich ohne vollständige Ausführung lesen. Die Inventur aller 143 Verzeichnisse bedeutet nicht, dass alle 143 vollständigen Laufordner neu ins Belegpaket aufgenommen wurden.

Das [Integrationsprotokoll](./integrationsprotokoll.md) ist vorbereitet, **nicht durchgeführt**. Es prüft einen identifizierten späteren Transkriptpfad; kein erneuter Ledger/F-Vergleich und keine Usability-Studie. Der endgültige Freeze, Materialfreigaben und die gebündelte PDF-Abnahme bleiben offen.
