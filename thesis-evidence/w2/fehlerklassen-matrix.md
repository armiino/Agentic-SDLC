# Fehlerklassen-Matrix — Beleg für 7.1-Claims „Checker/Repair" und „Mutationsnaht"

> Status: MOMENTAUFNAHME 06.09.2026. Kartierung der BESTEHENDEN deterministischen Wächter-Tests
> als Evaluations-Beleg (0 neue Tests — die Suite enthielt den Katalog bereits).
> Beleg-Lauf: `dotnet test --filter "CoreKangalTests|LedgerCanonicalLoop|UnusedUnitCompareRepair"`
> → **21/21 bestanden, 06.09.2026**, Commit c7e7fcd1 (+ W2-Arbeitsstand). Aussageform je Zeile:
> DETERMINISTISCH belegt (Test) vs. LLM-GEPRÜFT (Prompt-Regel, deterministisch NICHT abgesichert).

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
| Abgelehnte Mutation | `LedgerAdjudicationReviewAdapterTests.Verwerfen_braucht_Begruendung_defer_nicht` (UI-Schicht) | belegt NUR die Begründungspflicht — dass ein Reject den Core unverändert lässt, ist NICHT separat getestet (Grenze) |
| Fremd-/Stale-Snapshot am Forward-Pfad | `GithubSnapshotGuardTests.Passender_Stempel_passiert_fremder_und_fehlender_werden_LAUT_abgelehnt` | LAUTE Ablehnung |
| „Nicht autorisierte Mutation" | **ARCHITEKTONISCH** (eine Schreibnaht `SaveAsync`+Kangal, 9 inventarisierte Writer) — deterministisch belegt ist die Naht-Blockade; ein Writer-Bypass-Angriffstest existiert NICHT (Grenze) | teilbelegt |

## C · G01–G12-Vollbilanz (Blaupausen-Challenge-Katalog §7.8, Status je Fall — 06.09.)

| Nr | Challenge | Beleg | Status |
| --- | --- | --- | --- |
| G01 | unbekannte Relation | Kangal `I1_Relation_auf_String_Ziel_ist_Fehler` | **getestet** (blockiert) |
| G02 | fehlendes Relationsziel | Kangal `I1_Quelle_muss_Core_Item_sein` | **getestet** (blockiert) |
| G03 | unzulässige 1:1-Beziehung | Kangal `I5_…`/`I5b_…` (PBI↔Issue doppelt) | **getestet** (blockiert) |
| G04 | direkte Mutation außerhalb der Naht (Writer-Bypass) | — (architektonisch: EINE Schreibnaht, 9 inventarisierte Writer) | **nicht getestet/offen** |
| G05 | Apply ohne fachliche Autorisierung | — (Gate-Architektur; kein adversarialer Test) | **nicht getestet/offen** |
| G06 | abgelehnte externe Änderung | `Verwerfen_braucht_Begruendung_defer_nicht` (Prüfschicht) | **teilweise** (Begründungspflicht ✓; Core-Unverändertheit nicht separat) |
| G07 | Projektion ohne inhaltliche Änderung | `Identischer_Save_erzeugt_KEINEN_Snapshot` + Projektions-Idempotenz-Tests | **getestet** |
| G08 | Drift Core↔GitHub | `GithubSnapshotGuardTests` (fremder/fehlender Stempel LAUT abgelehnt) | **teilweise** (Stempel-Wächter ✓; vollständige Drift-ERKENNUNG nicht) |
| G09 | Rückweg externer Änderung | Kommentar-Rundweg Block K (Live-Demonstration, kein det. Test) | **teilweise** (Demonstration [D]) |
| G10 | Snapshot vor Mutation | `Save_auf_bestehendem_Core_legt_Snapshot_des_alten_Stands_ab` | **getestet** |
| G11 | Supersede statt Löschen | Kangal `I3_superseded_…`-Ausnahme + `I4_covers_auf_superseded`-Warnung | **teilweise** (Invarianten ✓; Kein-Lösch-Pfad = architektonisch) |
| G12 | manipulierte deterministische Projektion | `Fingerabdruck_ist_deterministisch_und_aendert_sich_mit_der_Wahrheit` | **teilweise** (Fingerabdruck-Abweichung erkennbar; kein adversarialer Manipulationstest) |

Bilanz: 5 getestet · 5 teilweise · 2 nicht getestet/offen.

## Zulässige Kapitel-Aussage

Die deterministischen Wächter behandeln die getesteten Fehlerklassen nachweislich (21/21,
inkl. Save-Blockade ohne Teilschreiben); Verstärkungs- und Dispositions-Prüfung sind
LLM-Stufen (exemplarisch, nicht deterministisch belegt); Detailverlust in verwendeten Units
ist die bekannte, in W2 vermessene blinde Klasse. Repair-WIRKSAMKEIT ist LLM-abhängig und
nur exemplarisch belegt (Live-Läufe `074419`, oss-Referenz-Repair 2×).
