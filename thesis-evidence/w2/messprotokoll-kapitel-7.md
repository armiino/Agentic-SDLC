# Messprotokoll Kapitel 7 (Schritt 1 des Umsetzungsplans)

> Status: FESTGELEGT 07.09.2026, VOR Beginn der Zusatzanalysen (Schritte 2–4). Regeln gemäß
> `Kapitel-7-Konkreter-Umsetzungsplan.md` §3; Rollen der Dokumente gemäß Schreibplan v2.4.
> ENTSTEHUNGS-VERMERK (Pflicht, 7.2): Die Hauptauswertung (Ledger/F-Qualität) wurde am 06.09.
> VOR diesem Protokoll durchgeführt (damalige Regeln: W2-Protokoll §28 + Urteilsverfahren
> v3.1, Hashes ① v1.2 — unverändert gültig). Dieses Protokoll legt die Regeln der NEUEN,
> RETROSPEKTIVEN Zusatzanalysen und der NEUEN Tests fest. Nichts wird rückdatiert.

## 1 · Verwendete Läufe und Systemstände

### 1.1 Historisch (bereits ausgewertet — Ergebnisse werden ÜBERNOMMEN, Stand je Beleg)

| Zweck | Lauf | Stand/Modell | Bewerteter Stand | Ergebnisquelle |
| --- | --- | --- | --- | --- |
| Ledger-Qualität Stressfall | `runs/ledger/20260905_164650_8189ea` | Pilot-Stand 05.09., gpt-5.4 | LCR (`capture/lcr-machine.json`; L≡LCR auf Claim-Ebene) | `runs/w2-official-ledger-eval-8189ea.json` |
| Ledger-Qualität Treue-Fall | `runs/ledger/20260905_164206_e30646` | dito | dito | `…-eval-e30646.json` |
| Sekundär: F-Vergleichsbedingung | `runs/arm-f/20260906_143052_6540e2`, `…141518_12d046` | 06.09., F_mission_v2, gpt-5.4 | Einreichung (output.json) | `w2-official-eval-*.json` |
| Sekundär: Modellreihe (inkl. 4 F-Fehlläufe) | 8 Läufe 06.09. | gpt-5-mini/oss-120b/4.1-mini | wie oben | `w2-modellsensitivitaet-eval-*.json` |
| E2E-Demonstrationen | Frischlauf 23.07. · R-14-Zyklus 04.08. (`…121433_eb181a`) · Block K 17.–18.08. · Resume `…144056_dc711f` 05.08. | HISTORISCHE Entwicklungsstände (je Lauf eigener Stand!) | Laufartefakte | E2E-RUNBOOK, Block-K-Testplan, STATUS |
| Provenienzaudit | Core-Snapshot 06.09. (SHA im Audit-JSON) | Audit-Skript v3 (T1–T9) | 237 Items | `runs/e2e-evidenz/traceability-audit.json` |
| Kontrolltests (Kartierung) | Beleg-Lauf 06.09., 36 Testmethoden | Commit c7e7fcd1 + W2-Arbeitsstand | Testausgänge | `runs/e2e-evidenz/fehlerklassen-matrix.md` |

Referenzbestand: Gold ① v1.2 (`input/eval-labels/`, SHA-256 in `benchmark-checksums.txt`),
KI-Erstentwurf autor-konsolidiert, OHNE unabhängige Zweitannotation. Beide Transkripte sind
ENTWICKLUNGSBEKANNT (waren Werkstatt-Material) — wird in 7.2 ausgewiesen.
**NACHTRAG 07.09. (Gold v2 / Hash ②):** Nach zwei externen gold-bewussten KI-Vollprüfungen
(Reviews 07.09., verifiziert) + Autor-Adjudikation + **Autor-Volldurchsicht des v2-Bestands
ohne Befund (07.09. — Autor = Konsolidierer, daher KEINE unabhängige Zweitannotation, aber
menschliche Komplett-Lektüre)** existiert der finale Referenzbestand **v2**
(Interview 109 IDs inkl. neuer G-IE-109 · meeting-2 31; `w2-gold-aenderungslog-v2.md`, Hash ②).
Die §1.1-Werte bleiben etikettiert „Stand Hash ① v1.2". **Neuberechnung gegen v2 ✅ ERLEDIGT
07.09.** (`neuberechnung-goldv2.md` + `runs/w2-goldv2-eval-*.json`): Interview F 0.651/0.853 ·
L 0.505/0.780 · meeting-2 F 0.871/0.935 · L 0.839/0.903 — ab jetzt sind die v2-Dateien die
zitierfähige Kanonik; Z1 arbeitet gegen v2/Nenner 109. Delta-Urteile nur KI-adjudiziert
(Verifikation offen); Modellmatrix nicht neu beurteilt (nur Hash-①-Etikett). P2 (verblindete
Fenster-Annotation) entfallen — als gold-bewusste Vollprüfung ausweisen; P3 („zentral"-
Teilmenge) gestrichen.
Urteilsverfahren der übernommenen Werte: dreistufig (KI-Adjudikation mit Beleg je Urteil →
regelgebundener, nicht verblindeter KI-Verifikationsdurchgang, 11 dokumentierte Kippungen →
menschliche Autor-Stichprobe nur Stressfall). Systeminterne Checker sind NIE Bewertungsmaßstab.

### 1.2 Neue RETROSPEKTIVE Zusatzanalysen (Regeln: dieses Protokoll, §3)

Z1 Stufenanalyse · Z2 fachliche Fallprüfung (historische Fälle) · Z3 Assertion-Zuordnung
der 36 Testmethoden. KEINE neue Systemausführung; die Adjudikation in Z1 ist LLM-gestützt
(gleiches dreistufiges Verfahren wie die Hauptauswertung).

### 1.3 Neue PROSPEKTIVE Ausführungen (Soll vorab in diesem Protokoll, §4–5)

T1–T4 Kontrolltests (G05, G06, Replay, gültiger Kontrollfall) · OPTIONAL (Autor-⚖ offen):
ein finaler Integrationslauf (Golden Path) — falls ausgeführt, gilt §5.3.

## 2 · Messpunkte und Messgrenzen (verbindlich je Bereich)

| Bereich | Bewerteter Stand | Menschlicher Eingriff |
| --- | --- | --- |
| Ledger-/F-Qualität (7.3) | maschineller Bestand VOR Adjudikation/Freigabe | nicht Teil der gemessenen Qualität |
| Stufenanalyse Z1 (7.3) | vollständige Bestände VOR/NACH dem bezeichneten Übergang | keiner (Artefakte belegen das) |
| Kontrolltests T1–T4 (7.4) | Rückmeldung + Ereignisse + fachlicher Core VOR/NACH Aufruf | Freigabe/Ablehnung = gesetzte Testbedingung |
| Fallprüfung Z2 (7.5) | GANZER Pfad inkl. menschlicher Entscheidung u. Projektion | sichtbar mitprotokolliert, getrennt zugerechnet |
| Resume/Steward (7.6) | dokumentierter Pfad je Szenario | je Szenario angegeben |

Die vorsegmentierte Quelle ist Eingabe BEIDER Konfigurationen; die Segmentierungsgüte wird
nicht bewertet.

## 3 · Z1 — Stufenanalyse (ergebnisoffen; Regeln VOR der Auswertung)

- **Gegenstand:** ausschließlich Stresslauf `8189ea`. Zwischenstände (verifiziert 07.09.):
  Bestand A = `step-01-candidate/output.json` (54 Kandidaten-Einträge) · Bestand B =
  kanonischer Endbestand (47 Claims, identisch mit dem bereits adjudizierten LCR-Claim-Bestand).
- **Übergangs-Definition (ehrlich):** Zwischen A und B liegen Kanonisierung + CanonicalCheck
  (+ Facetten-Validierung ohne Claim-Änderung, L≡LCR). Befunde werden dem GESAMTÜBERGANG
  A→B zugeschrieben, keiner Einzelkomponente.
- **Verfahren:** Jede der 108 Referenzaussagen wird in Bestand A voll/teilweise/nicht bewertet
  — mit EXAKT den Matchregeln v1.2 (Bündelung erlaubt; fehlende wesentliche Qualifikation =
  teilweise; widersprechende Negation/Entscheidung = nicht; Suche im GESAMTEN Bestand A).
  Für Bestand B werden die vorhandenen adjudizierten Urteile ÜBERNOMMEN (nicht neu bewertet).
  Bei jeder Statusänderung A→B werden BEIDE Seiten mit Textbeleg nachgeprüft (gleicher
  Maßstab beidseitig; Kippungen mit Matchlog).
- **Ergebnisform:** 3×3-Übergangstabelle (jede Referenzaussage genau eine Zelle;
  Zeilen-/Spaltensummen = Qualitätszählungen) + Listen der Verschlechterungen UND
  Verbesserungen mit Belegen. Eine kleinere Claim-Zahl ist KEIN Verlustnachweis.
- **Hinweis-Herkunft:** je der 48 Miss-Signale: erzeugende Stufe aus den Laufartefakten
  (`step-01c…/step-01d…/output.json`), Quellbezug (unitId), candidateIds; nicht
  Rekonstruierbares wird als solches ausgewiesen (keine Zuschreibung an Checker/Repair).
- **Gütesicherung:** KI-Adjudikation mit Beleg je Urteil → KI-Verifikationsdurchgang
  (Pflicht: alle Statusänderungen A→B + Seed-Stichprobe der unveränderten;
  Seed `SHA-256(id+'w2seed42')`) → Autor-Stichprobe optional (⚖).
- **Abschluss:** vollständige Tabelle + Hinweiszuordnung ODER dokumentierte Begrenzung.

## 4 · Z2 — Fachliche Fallprüfung (5 Falltypen; historisch = retrospektive Nachprüfung)

Fallblatt = Umsetzungsplan §6.2 (10 Punkte; Urteile: erfüllt / abweichend / nicht beurteilbar
/ nicht anwendbar, je begründet; „nicht beurteilbar" ≠ „abweichend").

| Falltyp | Vorgesehenes Material (historisch, Stand je Lauf ausweisen) |
| --- | --- |
| Neue Anforderung | Meeting-4-Verarbeitung mit Real-Writes (Block K, gh#43–45) |
| Präzisierung | Block-K-/Steward-Fall mit Bestands-Fortschreibung (Auswahl beim Ausfüllen begründen) |
| Widerspruch | R-14-Zyklus `20260804_121433_eb181a` (CONTRADICT→DEC→ADOPT→supersedes→Align) |
| Wiederholte Information | Block-K „Wiederholungs-Ehrlichkeit" (≠ Replay derselben Plan-ID!) |
| Offene Frage + GitHub-Rückweg | 9g-Fragen-Fall (offen bleibt offen) · der dokumentierte Kommentar-RÜCKWEG (Block K) als externer Eingang — STRIKT: Kommentar-Eingang ≠ Issue-Feld-Änderungserkennung ≠ Drift-Erkennung |

Kriterien je Fall werden VOR dem Ausfüllen des Fallblatts aus den fachlichen Verträgen
abgeleitet (retrospektive Prüfung, so gekennzeichnet). Fehlende Spuren = Befund. Es wird
KEINE Erfolgsquote aus fünf gezielt gewählten Fällen gebildet.

## 5 · T1–T4 — Kontrolltests (prospektiv; Soll VOR Ausführung)

- Spezifikation exakt nach Umsetzungsplan §5.2 (Tabelle G05/G06/Replay/Kontrollfall inkl.
  „Benötigter Nachweis"-Spalte). Prüfungen am BEZEICHNETEN realen Anwendungs-/Apply-Pfad;
  gemockte Antworten allein genügen nicht; externe Dienste ggf. durch protokollierende
  Testgegenstellen ersetzt (als Testumgebung ausgewiesen). Fachlicher Core-Zustand vor/nach
  als Beleg. G04 bleibt ausdrücklich UNGETESTET (Aussagen entsprechend begrenzen).
- Zusätzlich Z3: die 36 bestehenden Testmethoden werden anhand ihrer TATSÄCHLICHEN Assertions
  den Fehlerklassen zugeordnet (Namensfragmente genügen nicht); die Challenge-Matrix wird
  danach aktualisiert — keine vorweggenommene Bilanz.
- **5.3 (nur falls Golden-Path-⚖ = ja):** Systemstand (Commit/Tag), Eingaben, Soll je Falltyp
  und Auswahlregeln werden VOR dem Lauf in einem Nachtrag zu diesem Protokoll fixiert.

## 6 · Kennzahlen und Zählregeln

Verbindlich: Umsetzungsplan §3.4 (G, N_full/partial/none, C, H, N_signaled, H_relevant;
Hauptkennzahlen-Tabelle; Nenner 0 = nicht definiert; Ausfall ≠ Nullwert; Bündelung beidseitig
erlaubt, Referenzaussage zählt einmal; pauschale Hinweise zählen nicht automatisch;
Quellenstützung = zentrale Treuemetrik, Strict Precision ergänzend definiert; Touched/
Adressierbarkeit nur ergänzend; F1 entfällt; kein Gesamtscore). Die HISTORISCHE
Hinweiszuordnungs-Regel (deterministisch: Signal-Unit ∩ Gold-Quell-Units, 1 dokumentierte
semantische Ausnahme) bleibt unverändert und wird genau so berichtet.

## 6b · Dokumentierte Abweichung: Präzisions-Kennzahl (Nachtrag 07.09., Kollegen-Review)

Die eingefrorenen Matchregeln (Hash ① v1.2, §4) definieren als P2 eine strikte
Gold-Precision (nur einzeln voll gold-gematchte Claims zählen; quellengetragene Inhalte
außerhalb des Golds = `gold_escape`, kein Treffer). **Tatsächlich berechnet und in allen
Eval-Blättern berichtet wurde eine andere Kennzahl** („inhaltliche Korrektheit"): je Claim
wurde gegen die gesamte nummerierte Quelle geprüft, ob der Inhalt ihr widerspricht oder sie
verfälscht; Zähler = Claims ohne solchen Befund, Nenner = alle gelieferten Claims;
gold_escapes werden gezählt und separat berichtet, nicht als Fehler. Die Matchregeln-P2
wurde NICHT berechnet. Abweichung für beide Konfigurationen identisch angewandt; das
eingefrorene Regeldokument bleibt unverändert; Kapitel 7.2 legt die Abweichung offen.
(Abgrenzung zur semantischen Stützung: diese prüft die Belegqualität der ZITIERTEN Units,
die Korrektheit die Verfälschungsfreiheit gegenüber der Quelle.)
**Ergänzung 07.09. (Reviewerbericht-26 R3):** Bewertungsgegenstand der Korrektheits-Kennzahl
ist der AUSSAGENTEXT der Claims; Facetten/Status/übrige Felder sind nicht Gegenstand
(interne Prüferurteile wie `review_required` sind kein Ersatz für das externe Urteil).
Plausible, aber unbelegte Ergänzungen zählen nicht als quellenwidrig — sie werden über die
semantische Stützung erfasst (P4; im untersuchten Bestand keine beobachtet). gold_escapes
wurden nur für F-meeting-2 beziffert (3, supported); für die übrigen 3 Hauptläufe NICHT
gezählt (offene Dokumentationslücke, in Kapitel 7/Anhang D.3 ausgewiesen).

## 7 · Nicht Gegenstand (Abgrenzung)

Keine neuen freien Agenten oder Vergleichsarchitekturen · keine Segmentierungs-Bewertung ·
kein semantischer Voll-Audit aller Artefakte · keine Nutzer-/Zeitstudie · kein allgemeiner
Ausfallsicherheits- oder Umgehungsfreiheits-Nachweis · Modellreihe bleibt unverändert
sekundär/unverifiziert. Ein negatives oder nicht rekonstruierbares Ergebnis beendet die
jeweilige Untersuchung mit Dokumentation — der Umfang wird nicht ausgedehnt.
