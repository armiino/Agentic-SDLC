# T1–T4 — Kontrolltests am realen Apply-Pfad (Messprotokoll §5 / Umsetzungsplan §5.2)

> Status: ERLEDIGT 07.09.2026 — PROSPEKTIV: Soll stand VOR der Ausführung fest (§5.2-Tabelle
> „Benötigter Nachweis", wörtlich übernommen). Neue Tests:
> `AgenticSdlc.Tests/FullWorkflow/IngestionApplyGovernanceTests.cs` (4 Methoden).
> Beleg-Lauf 07.09.: Filter-Lauf **4/4** · volle Suite **688/688** (vorher 684; nur Tests
> ergänzt, KEIN Produktionscode geändert — freeze-konform §7.2).

## Prüfgegenstand und Testumgebung (ausgewiesen)

Geprüft wird der ECHTE persistierende Apply-Pfad von Tor 1:
`IngestionApplyExec.ExecuteAsync` → `IngestionApply.Apply` → `JsonCoreRepository.SaveAsync`
(inkl. CoreKangal-Naht, Snapshot-Mechanik, applied.marker). KEINE Mocks: jeder Test seedet
einen realen Core in einem Temp-Root (`JsonCoreRepository.SaveAsync`), legt einen realen
Plan-Ordner mit MeetingDelta-Datei und Entscheid-Datei (`human-decisions.json`, UI-Vertrag) an
und liest den Core-Zustand NACH der Ausführung aus dem Dateisystem zurück. Externe Dienste
kommen auf diesem Pfad nicht vor (GitHub liegt hinter dem separaten Forward-Pfad) — es war
keine Testgegenstelle nötig. Grenze: Der Einstieg erfolgt an der geteilten Exec-Naht (die von
CLI-Runner UND MAF-HITL-Executor genutzt wird), nicht über den CLI-Prozess selbst.

## Ergebnisse je Fall (Soll → Ist)

| Fall | Test | Soll (§5.2) | Ist (Assertions) |
| --- | --- | --- | --- |
| **T1 / G05 fehlende Freigabe** | `G05_Op_ohne_Freigabe_wird_nicht_uebernommen…` | keine fachliche Übernahme ohne Freigabe; definierte Zurückstellung | ✅ Report.Applied leer · Core-Items identisch (nur REQ-1) · keine Proposals · KEIN Snapshot (inhaltsgleicher Save) |
| **T2 / G06 abgelehnte Änderung** | `G06_Abgelehnte_Aenderung_wird_nicht_uebernommen…` | keine Übernahme; Ablehnung bleibt nachvollziehbar | ✅ Report.Applied leer · fachlicher Item-Bestand unverändert · GENAU 1 `ingest_rejection`-Proposal mit wörtlicher Begründung + Statement (Audit ≠ fachliche Mutation, getrennt geprüft) |
| **T3 / Replay bereits angewendeter Plan** | `Replay_desselben_Plans_fuehrt_zu_keinem_zweiten_Apply…` | keine zweite fachliche Anwendung, keine zusätzlichen Objekte | ✅ zweiter Aufruf liefert den persistierten Erst-Report (gleiche EntityId) · Core-Datei BYTE-identisch · Snapshot-Zahl unverändert (kein zweiter Save) · exakt 2 Items (kein Duplikat) |
| **T4 / gültiger Kontrollfall** | `Kontrollfall_Freigegebene_Aenderung_wird_korrekt_uebernommen…` | vorgesehene Änderung korrekt übernommen | ✅ neues Item mit erwartetem Text · sourceRunId = Auslöser-Lauf (R-31/I7-Freigabebezug) · 1 History-Snapshot des Alt-Stands · applied.marker + delta.json + core-before.json vorhanden |

Identitäts-/Freigabe-Semantik am Pfad (Code-verifiziert): Nur `decision=="apply"` gelangt in
die accepted-Menge (`AcceptedFromDecisions`); Ops ohne apply-Entscheid werden im Apply nie
iteriert. Replay-Identität = `plan.PlanId` via `applied/applied.marker` + `delta.json`.

## Grenzen (unverändert gültig)

- **G04 (Writer-Bypass außerhalb der Schreibnaht) bleibt UNGETESTET** — Aussagen bleiben auf
  das belegte Architektur-/Ausführungsmodell begrenzt (eine Schreibnaht, 9 inventarisierte
  Writer; kein adversarialer Bypass-Test).
- Die P2a-Begründungspflicht bei Ablehnung ist eine Review-UI-Invariante (bestehender Test
  `Verwerfen_braucht_Begruendung_defer_nicht`) — der Apply-Pfad selbst validiert die
  Begründung nicht erneut (T2 belegt Nicht-Übernahme + Gedächtnis, nicht die UI-Pflicht).
- Geprüft ist der Ingest-Apply-Pfad (Tor 1) als BEZEICHNETER Pfad; die analogen Marker-Pfade
  (pbi-update/decision) sind baugleich (S3-analog), aber nicht separat getestet.
- Fehlversuche: keine — alle 4 Tests bestanden im ersten Lauf (Suite 688/688).
