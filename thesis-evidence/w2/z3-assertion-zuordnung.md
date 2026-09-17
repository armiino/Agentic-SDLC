# Z3 — Assertion-Zuordnung der 36 Testmethoden (Messprotokoll §5)

> Status: ERLEDIGT 07.09.2026 — jede Methode wurde anhand ihrer TATSÄCHLICHEN Assertions
> geprüft (Methodenkörper gelesen; Namensfragmente genügen nicht). Prüftiefe je Methode:
> **PERSISTENZ** (echter Datei-/Repository-Zustand) · **EXCEPTION** (Wurf + ggf. Zustand) ·
> **OBJEKT** (In-Memory-Rückgabe/Report). Beleg-Lauf 07.09.: Suite 688/688.

## 1 · Zuordnung (36 Methoden, nach Klassen)

| Methode | Fehlerklasse (Matrix-Zeile) | Assertions prüfen wirklich | Tiefe |
| --- | --- | --- | --- |
| Kangal.Gesunder_Core_passiert… | Negativkontrolle | Pass==true, Warnings leer | OBJEKT |
| Kangal.I1_Relation_auf_String_Ziel… | **G02 fehlendes/ungültiges RelationsZIEL** (korrigiert 07.09.) | Errors enthält I1_TARGET_INVALID (Ziel kein gültiges Core-Item) | OBJEKT |
| Kangal.I1_Quelle_muss_Core_Item… | **fehlende RelationsQUELLE** (eigener Wächter — KEINE G01/G02-Zeile; korrigiert 07.09.) | Errors enthält I1_SOURCE_MISSING (Quelle kein Core-Item) | OBJEKT |
| Kangal.Externe_Ziele_still | Negativkontrolle (Exempt) | Pass==true bei gh#/claim/HDEC/CAND | OBJEKT |
| Kangal.I5_zwei_Issue_Mappings… | G03 unzulässige 1:1 | Errors I5_MULTIPLE_ISSUE_MAPPINGS | OBJEKT |
| Kangal.I5b_ein_Issue_von_zwei_PBIs… | G03 | Errors I5B_ISSUE_MAPPED_TWICE (gh#7) | OBJEKT |
| Kangal.I2_und_I3_sind_Warnungen… | I2/I3 Warn-Klasse | Pass==true UND Warn-Codes vorhanden | OBJEKT |
| Kangal.I3_superseded_brauchen_keine_Deckung | G11-Anteil | DoesNotContain I3-Warnung | OBJEKT |
| Kangal.I4_covers_auf_superseded… | G11-Anteil | Warnings I4_COVERS_SUPERSEDED | OBJEKT |
| Kangal.I6_nicht_offene_Decision… | I6 Warn-Klasse | Warnings I6_CONTRADICTS_LIFECYCLE | OBJEKT |
| **Kangal.SaveAsync_wirft_bei_I1_und_schreibt_NICHTS** | Naht-Blockade (G01 am Save-Pfad) | Exception „CORE_KANGAL" + Core-Datei UND history existieren NICHT; gesunder Save persistiert danach | **EXCEPTION+PERSISTENZ** |
| Kangal.Echter_Core_passiert… | Grün-Nachweis realer Core | Pass/Warnings am geladenen echten Core | OBJEKT |
| CanonicalLoop.EvaluateCanonicalStage_findet_verlorenen_Candidate… | Coverage-Lücke erkannt | Pass==false + CANDIDATE_SILENTLY_DROPPED; Gegenprobe Pass | OBJEKT |
| CanonicalLoop.Repair_schliesst_die_Luecke… | reparierbare Lücke | Repair-Call==1, canonical-gate.json attempt 2/source repair, output.json existiert | PERSISTENZ |
| CanonicalLoop.Wirkungsloser_Repair_endet_LAUT… | MaxAttempts-Terminal | Output „MaxAttemptsReached", output.json existiert | PERSISTENZ |
| CanonicalLoop.Nicht_reparierbarer_Fehler_zum_Menschen | Verdict-Routing | Repair-Calls==0, Output „HumanReview" | OBJEKT |
| CanonicalLoop.Sauberer_Draft_passiert… | Negativkontrolle | Calls==0, gate.json gatePass true | PERSISTENZ |
| UnusedRepair.Deckungs_Urteil_ohne_Referenz… | kaputte Referenz | boolescher Validator true | OBJEKT |
| UnusedRepair.Nur_halluzinierte_Referenzen… | halluzinierte Referenz | true bei Ghost-Ref, false mit gültiger | OBJEKT |
| UnusedRepair.Nicht_Deckungs_Verdicts… | Negativkontrolle | false für missing_claim/needs_human | OBJEKT |
| UnusedRepair.Downgrade_stuft_ehrlich… | unehrliche Selbstheilung | needs_human + Spur-Prefix + Original-Grund | OBJEKT |
| History.Save_legt_Snapshot_ab | G10 Snapshot vor Mutation | echte Snapshot-Datei mit Alt-Inhalt | PERSISTENZ |
| History.Identischer_Save_KEIN_Snapshot | G07 Projektion ohne Änderung | history-Ordner existiert NICHT | PERSISTENZ |
| Adr.Pending_nur_aktive_design… | ADR-Auswahl | Pending-Liste + NextAdrNumber | OBJEKT |
| Adr.Gate_prueft_Coverage… | ADR-Gate | Fehlercodes + Repairable-Flags | OBJEKT |
| Adr.Apply_schreibt_MADR…idempotent | G07-Anteil (Idempotenz) | **NUR In-Memory-files-Liste** (s. §2-Fund) | OBJEKT |
| Adr.Abloesung_zieht_Status_nach | G11-Anteil | StatusFollowUps + README-Inhalt (in-memory) | OBJEKT |
| Adr.Uebersicht_gruppiert… | Projektion | Render-String-Inhalte | OBJEKT |
| ReqDoc.Rendert_die_professionelle_Struktur | Projektion aktiver Wahrheit | Abschnitte + Abgelöstes fehlt | OBJEKT |
| ReqDoc.Version_zaehlt_fort | Versionierung | NextVersion 1 bzw. 4 | OBJEKT |
| ReqDoc.Fingerabdruck_deterministisch… | G12-Anteil | gleich/ungleich je Wahrheit | OBJEKT |
| Status.Status_wird_als_Projektion_geschrieben | §5-Projektion | JSON-String enthält status | OBJEKT |
| Status.Achsen_gewinnen_ueber_status_String | Manipulations-Resistenz | Validity gewinnt über manipulierten String | OBJEKT |
| Status.Item_ohne_Achsen_scheitert_LAUT | lauter Guard | beide Zugriffe werfen | EXCEPTION |
| Status.Echter_Core_laedt_unter_Option_A | Grün-Nachweis realer Core | Achsen je Item konsistent, Views fehlerfrei | OBJEKT |
| SnapshotGuard.Passender_Stempel…LAUT_abgelehnt | G08-Anteil (Stempel) | Throws MISMATCH/UNSTAMPED, passend passiert | EXCEPTION |

**Zählung:** 12+5+4+2+5+3+4+1 = 36 ✓ · **Tiefe (korrigiert 07.09., Kollegen-Nachzählung):
28 nur OBJEKT · 5 nur PERSISTENZ · 2 nur EXCEPTION · 1 EXCEPTION+PERSISTENZ (Kangal-SaveAsync)
= 36.** Persistenzanteil damit 6 Methoden, Exception-Anteil 3. (Erstfassung „4/3/29" widersprach
der eigenen Tabelle — korrigiert.)

## 2 · Befunde der Verifikation (Name vs. tatsächliche Assertions)

1. **`AdrProjectionTests.Apply_schreibt_MADR_Datei…`: der Name verspricht mehr als die
   Assertions.** Geprüft wird die ZURÜCKGEGEBENE In-Memory-files-Liste (Inhalt + Idempotenz),
   KEIN echter Disk-Write. Die Matrix-Aussage „idempotent" bleibt gestützt, die
   Persistenz-Behauptung des Namens nicht — Zuordnung entsprechend auf OBJEKT begrenzt.
2. Die beiden „Echter_Core"-Tests lesen reale Dateien, ihre Asserts zielen aber auf
   Report/geladenes Objekt (kein Dateizustands-Nachweis) — als Grün-Nachweise korrekt, nicht
   als Persistenz-Tests zählen.
3. **Der einzige Test, der die STRUKTURELLE Save-Naht-Blockade am echten Dateizustand beweist,
   ist `SaveAsync_wirft_bei_I1_und_schreibt_NICHTS`.** Die vier neuen T1–T4-Tests sind ein
   ANDERER Nachweis (Freigabe/Ablehnung/Wiederholung am Apply-Pfad, ebenfalls Persistenz-Tiefe)
   — kein identischer Blockade-Nachweis, beide ergänzen sich.
4. **G01/G02-Fehlzuordnung der Matrix (Fund des Kollegen-Reviews 07.09., am Code verifiziert —
   die Erstfassung dieses Blatts hatte sie fälschlich „bestätigt", obwohl der Umsetzungsplan
   §5.4 genau diese Prüfung vorregistriert hatte):**
   `I1_Relation_auf_String_Ziel` prüft das ZIEL (I1_TARGET_INVALID) → deckt **G02**;
   `I1_Quelle_muss_Core_Item` prüft die QUELLE (I1_SOURCE_MISSING) → eigener Wächter, deckt
   WEDER G01 noch G02 wie beschriftet. **G01 („unbekannte Relation"): der Kangal behandelt
   unbekannte Relationstypen nur als WARNUNG (`UNKNOWN_RELATION_TYPE`, „nicht geprüft" —
   CoreKangal.cs:89), KEIN Block — und KEIN Test der Suite prüft dieses Verhalten positiv**
   (nur zwei DoesNotContain-Negativchecks anderswo). G01 ist damit NICHT getestet, und die
   frühere Behauptung „getestet (blockiert)" war doppelt falsch.
5. Mit T1–T4 schließen sich G05 und der Core-Teil von G06 (weitergehende Außenwirkungen der
   G06-Zeile bleiben ungedeckt); Replay belegt die Plan-Idempotenz am Apply-Pfad, KEINE
   allgemeine Absturz-Doppel-Write-Sicherheit. **Korrigierte Bilanz: 6 getestet · 4 teilweise ·
   2 nicht getestet/offen (G01, G04).**
