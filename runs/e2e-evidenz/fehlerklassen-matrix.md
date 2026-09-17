# Fehlerklassen-Matrix — Beleg für 7.1-Claims „Checker/Repair" und „Mutationsnaht"

> Status: MOMENTAUFNAHME 07.09.2026 (v2). Kartierung der deterministischen Wächter-Tests als
> Evaluations-Beleg. **Update 07.09. (T1–T4 + Z3):** 4 NEUE Kontrolltests am realen Apply-Pfad
> (`IngestionApplyGovernanceTests`, PERSISTENZ-Tiefe; Doku `thesis-evidence/w2/
> t1-t4-kontrolltests.md`) — G05 und der Core-Teil von G06 damit GESCHLOSSEN; die 36
> Bestandsmethoden per Z3 an den TATSÄCHLICHEN Assertions kartiert
> (`thesis-evidence/w2/z3-assertion-zuordnung.md`; 2 Funde: Adr-Apply-Test prüft nur
> In-Memory-files · G01/G02-Zuordnung war falsch, s. Korrektur unten).
> Beleg-Läufe: 21/21 (06.09., Commit c7e7fcd1) · Governance-Filter 4/4 + volle Suite
> **688/688** (07.09.). **Korrektur 07.09. (Kollegen-Review Z3):** G01/G02-Zuordnung
> berichtigt — G01 (unbekannter Relationstyp) ist NUR Warnung + ungetestet, G02 wird vom
> Ziel-Test gedeckt; Bilanz 6/4/2 statt 7/4/1; Prüftiefen 28 OBJEKT / 5 PERSISTENZ /
> 2 EXCEPTION / 1 kombiniert. Aussageform je Zeile: DETERMINISTISCH belegt (Test) vs.
> LLM-GEPRÜFT (Prompt-Regel, deterministisch NICHT abgesichert).

## A · Ledger-Prüfmechanik (Claim: „behandelt definierte Fehlerklassen")

| Fehlerklasse | Wächter | Beleg | Verhalten |
| --- | --- | --- | --- |
| Verlorener Kandidat / Coverage-Lücke nach Kanonisierung | `EvaluateCanonicalStage` (det.) | `LedgerCanonicalLoopTests.EvaluateCanonicalStage_findet_verlorenen_Candidate…` | erkannt; Gate Fail |
| Reparierbare Lücke | Repair-Loop (LLM) + det. Re-Check | `…Repair_schliesst_die_Luecke_und_der_zweite_Versuch_besteht` | Repair → Pass (Attempt 2) |
| Wirkungsloser Repair | GateLoop MaxAttempts | `…Wirkungsloser_Repair_endet_LAUT_terminal_nach_MaxAttempts` | LAUTER Terminal-Stopp, kein stilles Weiterlaufen |
| Nicht-reparierbare Fehlerklasse | Verdict-Routing | `…Nicht_reparierbarer_Fehler_geht_ohne_Repair_Call_zum_Menschen` | direkt zum Human Gate |
| Negativkontrolle: sauberer Draft | ganze Kette | `…Sauberer_Draft_passiert_im_ersten_Versuch_unveraendert` | Pass, inhaltsgleich |
| Kaputte/halluzinierte Claim-Referenz | Unused-Compare-Referenzprüfung (det.) | `UnusedUnitCompareRepairTests.Deckungs_Urteil_ohne_Referenz…` + `Nur_halluzinierte_Referenzen…` | Verstoß erkannt |
| Unehrliche Selbstheilung | Downgrade-Mechanik | `…Downgrade_stuft_ehrlich_zu_needs_human_und_hinterlaesst_Spur` | Downgrade + Audit-Spur |
| Verstärkung (offen→decided, desired→must) | FacetValidator | **LLM-GEPRÜFT** (Prompt-Regel) — deterministisch nicht abgesichert | Grenze; Live-Beleg exemplarisch (Pilot: 0 Downgrades nötig) |
| Dispositions-Fehlvergabe (open-questions=required ohne echte Offenheit) | Extraktoren+FacetAssigner+FacetValidator | **LLM-GEPRÜFT** (Regel an Quelle + Prüfregel) — Wirkung live verifiziert (Lauf `105454`: 5/5 Rauschen → not_applicable) | Grenze: kein det. Test |
| Detailverlust INNERHALB verwendeter Units | — | **KEIN Wächter (bekannte blinde Klasse)** — gemessen in W2 (20/20 stille Lücken: 4 meeting-2 + 16 Interview) | dokumentierte Systemgrenze |

## B · Mutationsnaht Core (Claim: „blockiert definierte ungültige Änderungen") — `CoreKangalTests`, 12 Fälle

| Angriff | Verhalten | Beleg |
| --- | --- | --- |
| I1: Relation auf Nicht-Core-Ziel / Quelle kein Core-Item | **Abbruch** | `I1_Relation_auf_String_Ziel_ist_Fehler`, `I1_Quelle_muss_Core_Item_sein` |
| I1 am Save-Pfad | **wirft und schreibt NICHTS** | `SaveAsync_wirft_bei_I1_und_schreibt_NICHTS` |
| I5/I5b: doppelte Issue-Mappings (PBI↔Issue) | **Abbruch** | `I5_zwei_Issue_Mappings…`, `I5b_ein_Issue_von_zwei_PBIs…` |
| I2/I3/I4/I6: Deckungs-/Supersede-/Decision-Verstöße | **laute Warnung** (kein Block, by design) | `I2_und_I3_sind_Warnungen…`, `I4_covers_auf_superseded…`, `I6_nicht_offene_Decision…` |
| Negativkontrollen | gesunder Core, Extern-Ziele, superseded-Ausnahme, ECHTER Core: 0/0 | 4 Passier-Tests |

## B2 · Kompakte Governance-Challenge (Kollegen-Katalog [5 Fälle] + 2 verwandte Wächter = 7 Zeilen — via bestehende Tests; Beleg-Lauf 06.09.: **36/36 grün**)

Befehl des Beleg-Laufs (nach Build gemäß Test-Rezept):
`dotnet test AgenticSdlc.Tests/AgenticSdlc.Tests.csproj --no-build --filter "FullyQualifiedName~CoreKangalTests|FullyQualifiedName~LedgerCanonicalLoop|FullyQualifiedName~UnusedUnitCompareRepair|FullyQualifiedName~JsonCoreRepositoryHistory|FullyQualifiedName~AdrProjectionTests|FullyQualifiedName~RequirementsDocumentProjection|FullyQualifiedName~CoreStatusProjection|FullyQualifiedName~GithubSnapshotGuard"`

| Challenge-Fall | Beleg | Verhalten |
| --- | --- | --- |
| Ungültige Relation | Kangal I1-Tests (s. o.) | Abbruch, schreibt nichts |
| Gültige Mutation → Snapshot | `JsonCoreRepositoryHistoryTests.Save_auf_bestehendem_Core_legt_Snapshot_des_alten_Stands_ab` | Snapshot des Alt-Stands |
| Idempotenter Save | `…Identischer_Save_erzeugt_KEINEN_Snapshot` | kein Snapshot-Spam |
| Idempotente/deterministische Projektion | `AdrProjectionTests.Apply…ist_idempotent` · `RequirementsDocumentProjectionTests.Fingerabdruck_ist_deterministisch…` · `CoreStatusProjectionTests` (2) | idempotent/deterministisch |
| Abgelehnte Mutation | `Verwerfen_braucht_Begruendung_defer_nicht` (UI-Begründungspflicht) + **NEU 07.09.: `G06_Abgelehnte_Aenderung_wird_nicht_uebernommen…`** (realer Apply-Pfad: Item-Bestand unverändert, Rejection mit Grund im Gedächtnis) | Reject mutiert NICHT + bleibt nachvollziehbar (PERSISTENZ) |
| Apply ohne Freigabe | **NEU 07.09.: `G05_Op_ohne_Freigabe_wird_nicht_uebernommen…`** (realer Apply-Pfad, leere accepted-Menge) | keine Übernahme, kein Snapshot (PERSISTENZ) |
| Replay desselben Plans | **NEU 07.09.: `Replay_desselben_Plans_fuehrt_zu_keinem_zweiten_Apply…`** (applied.marker-Identität) | Core byte-identisch, kein Duplikat (PERSISTENZ) |
| Gültiger Kontrollfall | **NEU 07.09.: `Kontrollfall_Freigegebene_Aenderung_wird_korrekt_uebernommen…`** | Differenz + Herkunft (R-31/I7) + Snapshot + Marker (PERSISTENZ) |
| Fremd-/Stale-Snapshot am Forward-Pfad | `GithubSnapshotGuardTests.Passender_Stempel_passiert_fremder_und_fehlender_werden_LAUT_abgelehnt` | LAUTE Ablehnung |
| „Nicht autorisierte Mutation" | **ARCHITEKTONISCH** (eine Schreibnaht `SaveAsync`+Kangal, 9 inventarisierte Writer) — deterministisch belegt: Naht-Blockade (Save-Test, PERSISTENZ) + seit 07.09. der Gate-Freigabe-Filter am Apply-Pfad (G05); ein Writer-BYPASS-Angriffstest existiert weiterhin NICHT (=G04-Grenze) | teilbelegt |

## C · G01–G12-Vollbilanz (Blaupausen-Challenge-Katalog §7.8, Status je Fall — 06.09.)

| Nr | Challenge | Beleg | Status |
| --- | --- | --- | --- |
| G01 | unbekannte Relation (unbekannter Relationstyp) | — **KORRIGIERT 07.09. (Kollegen-Fund):** der Kangal WARNT nur (`UNKNOWN_RELATION_TYPE`, „nicht geprüft"), blockiert NICHT; kein positiver Test | **nicht getestet/offen** (Code-Verhalten: laute Warnung, kein Block) |
| G02 | fehlendes Relationsziel | **KORRIGIERT 07.09.:** Kangal `I1_Relation_auf_String_Ziel_ist_Fehler` (I1_TARGET_INVALID = Ziel kein gültiges Core-Item); ZUSÄTZLICH gedeckt: fehlende RelationsQUELLE via `I1_Quelle_muss_Core_Item_sein` (I1_SOURCE_MISSING — eigener Wächter, keine Katalog-Zeile) | **getestet** (blockiert) |
| G03 | unzulässige 1:1-Beziehung | Kangal `I5_…`/`I5b_…` (PBI↔Issue doppelt) | **getestet** (blockiert) |
| G04 | direkte Mutation außerhalb der Naht (Writer-Bypass) | — (architektonisch: EINE Schreibnaht, 9 inventarisierte Writer) | **nicht getestet/offen** |
| G05 | Apply ohne fachliche Autorisierung | **`G05_Op_ohne_Freigabe…` (NEU 07.09., realer Apply-Pfad, PERSISTENZ)** | **getestet** (keine Übernahme) |
| G06 | abgelehnte externe Änderung | `Verwerfen_braucht_Begruendung_defer_nicht` (UI-Pflicht) + **`G06_Abgelehnte_Aenderung…` (NEU 07.09., Core-Unverändertheit + Gedächtnis)** | **getestet** |
| G07 | Projektion ohne inhaltliche Änderung | `Identischer_Save_erzeugt_KEINEN_Snapshot` + Projektions-Idempotenz-Tests | **getestet** |
| G08 | Drift Core↔GitHub | `GithubSnapshotGuardTests` (fremder/fehlender Stempel LAUT abgelehnt) | **teilweise** (Stempel-Wächter ✓; vollständige Drift-ERKENNUNG nicht) |
| G09 | Rückweg externer Änderung | Kommentar-Rundweg Block K (Live-Demonstration, kein det. Test) | **teilweise** (Demonstration [D]) |
| G10 | Snapshot vor Mutation | `Save_auf_bestehendem_Core_legt_Snapshot_des_alten_Stands_ab` | **getestet** |
| G11 | Supersede statt Löschen | Kangal `I3_superseded_…`-Ausnahme + `I4_covers_auf_superseded`-Warnung | **teilweise** (Invarianten ✓; Kein-Lösch-Pfad = architektonisch) |
| G12 | manipulierte deterministische Projektion | `Fingerabdruck_ist_deterministisch_und_aendert_sich_mit_der_Wahrheit` | **teilweise** (Fingerabdruck-Abweichung erkennbar; kein adversarialer Manipulationstest) |

Bilanz (Stand 07.09. KORRIGIERT nach Kollegen-Review): **6 getestet · 4 teilweise ·
2 nicht getestet/offen (G01, G04).** Verlauf: 06.09. „5/5/2" (enthielt die falsche
G01-Zuordnung) → T1–T4 schließen G05 + G06-Core-Teil → G01-Korrektur (Warnung statt Block,
ungetestet). Zusätzlich neu belegt (außerhalb G01–G12): Replay-Plan-Idempotenz + gültiger
Kontrollfall am Apply-Pfad (B2-Zeilen; KEINE allgemeine Absturz-Doppel-Write-Sicherheit).
Aussagegrenze: T1–T4 (Freigabe-Governance am Apply-Pfad) und Kangal-SaveAsync (strukturelle
Naht-Blockade) sind VERSCHIEDENE Nachweise, die sich ergänzen.

## Zulässige Kapitel-Aussage

Die deterministischen Wächter behandeln die getesteten Fehlerklassen nachweislich (21/21,
inkl. Save-Blockade ohne Teilschreiben); Verstärkungs- und Dispositions-Prüfung sind
LLM-Stufen (exemplarisch, nicht deterministisch belegt); Detailverlust in verwendeten Units
ist die bekannte, in W2 vermessene blinde Klasse. Repair-WIRKSAMKEIT ist LLM-abhängig und
nur exemplarisch belegt (Live-Läufe `074419`, oss-Referenz-Repair 2×).
