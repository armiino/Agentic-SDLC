# W2-Pilotkampagne — Bericht (05.09.2026)

> Status: MOMENTAUFNAHME 05.09. + ★★-NACHTRAG 06.09. — Pilot-Teil auf Vor-Hash-②-Stand
> (P1/P2 der Abschnitte oben sind HEURISTISCHE Bänder, §6.4). Die VORLÄUFIGE adjudizierte
> Hauptauswertung (KI-adjudiziert + KI-verifiziert, menschliche Prüfung AUSSTEHEND) steht im
> ★★-Nachtrag am Ende und ersetzt die Bänder; Gold weiterhin „KI-Erstentwurf,
> autor-konsolidiert" (Zweitannotation/Hash ② ausstehend). Lebender Stand: runs/w2-official-manifest.txt

## Rahmen

- 12 Generierungsläufe (2 Fälle × [3× Arm F + 3× ledger-build-units mit sofortigem
  `ledger-capture`]), Modell durchgängig `openai/gpt-5.4` (jury.judgeModel), OTel an.
- Manifest mit allen RunIds: `runs/w2-pilot-manifest.txt` · Rohdaten: `runs/arm-f/2026‌0905_16*`,
  `runs/ledger/20260905_16*` · Auswertung: dieser Ordner (deterministic-report.json,
  proposal-metrics.json, matchblatt-*.md + .aufloesung.json).
- Alle 12 Läufe fehlerfrei (jedes Ledger-Gate pass 0 Errors; jede F-Einreichung gültig).

## Harte (deterministische) Ergebnisse

| Größe | meeting-2 (36 AUs, Gold 31) | Interview (136 AUs, Gold 108) |
| --- | --- | --- |
| F-Aussagen je Lauf | 34 · 35 · 34 | 197 · 183 · 203 |
| L=LCR-Claims je Lauf | 20 · 14 · 19 | 47 · 47 · 41 |
| Reference Presence | 1.0 überall | 1.0 überall |
| **P3 Reference Validity** | **1.0 überall** | **1.0 überall** |
| F-Tokens (in/out, je Lauf ≈) | 18,5k / 3,4k | 100–103k / 17–19k |
| Ledger-Tokens (in/out, je Lauf ≈) | 23–27k / 12–14k | 135–136k / 40–42k |
| F-Prozessprofil (alle 6 Läufe) | 4 Runden · 1 Re-Read · 1 Validierung · 0 Selbstrevisionen · submitted | identisch |
| canonical-gate Attempts | 1 · 1 · 1 | 1 · 1 · 1 |
| Referenz-Repair-Downgrades | 0 | 0 |
| Miss-Signal-Items (LCR) | 3 · 3 · 2 | 48 · 40 · 53 |

**Strukturbefund 1 — L ≡ LCR (programmatisch verifiziert, alle 6 Ledger-Läufe):** Die
Claim-Bestände vor und nach der maschinellen QC sind INHALTSGLEICH (kanonischer Loop überall
Pass in Attempt 1, 0 Downgrades). Der maschinelle Beitrag der Prüf-/Reparaturstufe bestand in
dieser Kampagne ausschließlich aus DETEKTIONS-Flags für den Menschen (Miss-Signal) — exakt das
in Konzept §14.1/§14.5 vorregistrierte Ergebnisbild („Loop greift nicht ein, Nachprüfung
produziert Flags").

**Strukturbefund 2 — Kompressions-Kontrast am Stressfall:** Das Interview-Gold trägt 51/108
Units im Design-Teil (AU-0117–0136, atomare UI-Propositionen). Der Ledger bildet diesen Teil in
nur ~8 breiten Sammel-Claims ab (Positions-/Detail-Qualifikationen fehlen); Arm F erzeugt
183–203 feingranulare Aussagen. → Am langen Fall trennen sich die Arme wie im
Coverage-Stressfall-Design erwartet; die Richtung F≫L bei Coverage ist robust, auch wenn die
exakten Zahlen erst die Adjudikation liefert.

**Strukturbefund 3 — Ledger-Varianz:** meeting-2-Lauf `56639f` kanonisierte 20 Kandidaten zu
nur 14 Claims (die Schwester-Läufe: 20→20, 20→19) — sichtbare Run-zu-Run-Varianz der
Kanonisierung bei identischem Input (Gate pass, kein Fehler; W2-§13.4-Ergebnisbild
„Run-zu-Run-Varianz").

## Heuristische Vorschlags-Bänder (strict … optimistisch) — UNADJUDIZIERT

Median je Bedingung (Einzelwerte: proposal-metrics.json; 372 Grenzfälle fürs Evaluator-Urteil):

| Fall · Arm | P1 Gold-Coverage | P2 Gold-Precision |
| --- | --- | --- |
| meeting-2 · F | 0.29 … 0.74 | 0.27 … 0.68 |
| meeting-2 · L=LCR | 0.32 … 0.68 | 0.40 … 0.90 |
| Interview · F | 0.12 … 0.56 | 0.06 … 0.35 |
| Interview · L=LCR | 0.03 … 0.21 | 0.07 … 0.43 |

**Heuristik-Grenzen (wichtig):** Token-/Ähnlichkeits-Matching mit Naiv-Stemming; deutsche
Paraphrasen werden systematisch UNTERSCHÄTZT (die wahren Werte liegen eher am oberen Bandrand
oder darüber), Sammel-Claims können je Gold-Unit nur 1:1 gutgeschrieben werden (die
Merge-/Split-Regeln der Adjudikation sind großzügiger). Die BÄNDER taugen zur Orientierung und
Priorisierung — nicht als Ergebnis. Vorläufige Lesart, die die Adjudikation prüfen muss:
meeting-2 = F und Ledger nahe beieinander bei Coverage, Ledger präziser; Interview = F deutlich
höhere Coverage bei deutlich mehr Aussagen (Precision-Frage offen), Ledger komprimiert stark.

## Befund-Einordnung (05.09., nach Output-Inspektion — Mechanismen statt nur Zahlen)

**B1 · Kompression by design, nicht Blindheit:** Der Ledger verliert atomare Coverage primär
durch VERDICHTUNG (Kanonisierung clustert bewusst für die Abnehmer Baselines/Backlog/Core):
Beispiel Login-Screen — 10 atomare Gold-Units ↔ EIN Ledger-Sammel-Claim („…aus Logo,
E-Mail-Feld, Passwort-Feld und Login-Button bestehen…"); F spiegelt dagegen mit 12 Einzelsätzen
die Gold-Granularität. Kette: 136 Units → ~54 Kandidaten → 41–47 kanonische Claims.

**B2 · Ausgelassene Themen werden geflaggt — Wirkung liegt hinter der Messgrenze:** Die
Appbar-Definition (AU-0120) fehlt in den Claims, liegt aber im Miss-Signal
(`attach_as_evidence`, teils überkonfidente Begründung) — im Betrieb reviewbar am
Adjudikations-Gate; die Vor-HITL-Messung darf das designgemäß nicht gutschreiben (§12.0-4).
F besitzt KEIN Rest-Konzept: keine Bilanz, keine Flags, Vollständigkeit unbezifferbar.

**B3 · Der Preis der Freiheit ist Rauschen:** F-Interview enthält u. a. 14 reine
Kontext-Aussagen (§3d-Klasse: „es existieren Akten", Einarbeitung heute, Kennenlern-Termin …)
→ gold_escape; meeting-2 zeigt die Kehrseite: Ledger-P2-Band (0.40…0.90) über F (0.27…0.68).

**B4 · NEU erkannte Systemgrenze (berichtbar, kein Bug):** Detailverlust INNERHALB verwendeter
Units ist unbilanziert — die Wächter fangen unbenutzte Units, aber nicht weggefallene
Qualifikationen benutzter Units (Positions-/Gestaltungs-Details). Plus Kanonisierungs-VARIANZ
(20→14 Claims bei identischem Input, `56639f`).

**B5 · Konstrukt-Einordnung des Vergleichs (Fairness-Bilanz):** Ausführung regelkonform und
gleichgestellt (gleiche Quelle/Locators/Modell/Ziel, vorregistrierte Erwartungen, P2-Asymmetrie
gegen Fragment-Gaming). ZWEI bewusste, dokumentierte Konstrukt-Grenzen prägen die Zahlen:
① Der Maßstab (atomare Reproduktion) entspricht Fs NATIVER Outputform; die native Produktform
des Ledgers ist der verdichtete kanonische Claim — gemessen wird der PREIS der
Verdichtungs-Mission auf einer atomaren Metrik, nicht die Mission selbst (§12.6:
Konfigurationsvergleich, keine Kausalisolation). ② Die HITL-Messgrenze schneidet dem Ledger
seinen designierten Qualitätsschritt (Mensch hebt Flags) bewusst ab — fair für den
Maschinenvergleich, aber die Systemantwort ist erst der §13.3b-Befund. Beide Grenzen waren
VOR der Messung registriert (§12.0-4, §26-Erwartung 6: „Mehrwert eher Traceability/
Fortschreibung/Integrität als dramatisch bessere Einzeltexte" — eingetreten). FOLGE:
Der Vergleich beantwortet „Was kostet/bringt Struktur auf der Extraktions-Stufe?" — er
beantwortet NICHT „War der Ledger nötig?" (dessen Auftrag messen E1/E4/E5/E6 + §13.3b).
**KEINE Treatment-Änderung aus diesen Zahlen (§27-Vorregistrierung)** — Granularitäts-Tuning
der Kanonisierung wäre Goodhart und ist höchstens Nach-W2-Ausblick. Optional (Autor-⚖, vor
offizieller Kampagne offen zu legen): eine SEKUNDÄRE Themen-/Topic-Coverage-Diagnose neben der
atomaren P1 — als pilot-motivierte Ergänzung deklariert, nie als Ersatz der Primärmetrik.

## E2b-Ergebnisse (05.09., analytisch — Heuristik-Basis, Details: e2b-report.json)

**Themen-Coverage (Breite statt Atome; Gold-Units zu 18 bzw. 29 Themenblöcken gruppiert):**

| | meeting-2 (18 Themen) | Interview (29 Themen) |
| --- | --- | --- |
| F | 0.78 · 0.83 · 0.78 | 0.79 · 0.76 · 0.72 |
| L | **0.83 · 0.83 · 0.83** | 0.52 · 0.59 · 0.52 |

→ Auf THEMEN-Ebene ist der Ledger auf meeting-2 gleichauf/leicht vorn; am Interview schrumpft
der Abstand dramatisch (0.52–0.59 vs. 0.72–0.79 — statt 0.03 vs. 0.12 atomar-strict). Der
atomare Rückstand ist überwiegend TIEFE (Detailverlust), nicht Breite (Themenblindheit).

**Kompressionsfaktor (Aussagen je Gold-Unit, Median):** F 1.1 (m2) / 1.82 (IE) ·
L 0.61 / 0.44 — der Formunterschied als Pflicht-Kennzahl neben jeder Tabelle.

**★ Gate-Potenzial (der Kernbefund — Interview):** Von ~82–85 heuristischen L-Gold-Misses je
Lauf sind **62–71 über Gate-Items erreichbar** (Miss-Signal-Flags + review_required-Claims) —
nur **14–20 BLIND** (Blind-Quote 0.17–0.24). Rechnerische Potenzial-Obergrenze der Kette
„Maschine + perfekt bedientes Gate": ~0.80–0.87 Gold-Abdeckung. **Die Design-Behauptung
„Fehler werden expliziter Prüfbedarf statt unsichtbar" ist damit quantifiziert bestätigt** —
der Ledger holt seine Vollständigkeit konstruktionsgemäß erst AM Gate, nicht davor.
meeting-2: Blind-Quote ~0.46–0.60 (kleine Zahlen; viele „Misses" sind dort Detail-Partials
bereits angerissener Units, die kein Unused-Flag erzeugen können — Adjudikation klärt).

**Stufen-Verlust (je Gold-Unit, Interview):** kanonisch voll 2–4 · angerissen 19–24 ·
**Kanonisierungs-Verlust 7–12** (Kandidat hatte es, kanonischer Claim nicht mehr) ·
„Extraktion übersehen" 72–78 (Heuristik-Etikett; enthält Detail-Partials — Adjudikation
differenziert). Der Kanonisierungs-Verlust ist damit erstmals als eigene Größe beziffert.

**Provenienz-Audit: 6/6 Läufe = 100 %** — jeder kanonische Claim trägt die vollständige Kette
candidateIds→sourceUnitIds mit ausschließlich existierenden Zielen (20/20 · 14/14 · 19/19 ·
47/47 · 47/47 · 41/41). Die Transformations-Nachvollziehbarkeit ist lückenlos belegt.

**Kaveate:** alles auf Heuristik-Deckungsurteilen (≥0.45 angerissen / ≥0.72 voll);
Gate-Erreichbarkeit = OBERGRENZE (Flag ≠ garantierte Wiederherstellung); Blind-Listen je Lauf
im e2b-report.json für die qualitative Fehleranalyse.

## Nachtrag: F-v2-Probeläufe (05.09. spät — missionsgleicher Vertrag, je 1 Lauf pro Fall)

Läufe `20260905_184204_3cfc38` (meeting-2) + `20260905_184431_4c1c63` (Interview), Variante
`F_mission_v2` (kanonischer Bestand + Pflicht-Quellenbilanz, Konzept §11 v2):

- **Vertrags-Mechanik: fehlerfrei im Erstversuch.** Beide Bilanzen lückenlos (36/36 bzw.
  136/136 AUs), alle Claim-IDs konsistent, keine Nachbesserungs-Schleifen. Prozessprofil
  unverändert gleichförmig (4 Runden · 1 Re-Read · 1 Validierung · 0 Revisionen).
- **Kanonisierungs-Effekt des Auftrags:** Interview 128 Aussagen (v1: 183–203) — der
  Dedup-/Kanonik-Auftrag verdichtet Fs Output um ~35 %.
- **★ DER SELBSTDIAGNOSE-BEFUND: `unresolved` = LEER, in BEIDEN Fällen.** F deklariert auf
  dem 136-Unit-Stressfall 112 used + 24 non_relevant und NULL Unsicherheit — kein einziger
  der 32 Gold-AUs aus den Ledger-Blind-Regionen wurde als unresolved markiert. Die
  strukturelle Bilanz-PFLICHT erzwingt Vollständigkeit der Form, nicht Wahrhaftigkeit des
  Inhalts: Fs Rechenschaft ist ERZÄHLT (Selbsturteil ohne Prüfpfad), die des Ledgers
  ERRECHNET (Unit-Arithmetik + Prüfstufen + Gate-Zulauf). Vorbehaltlich der offiziellen
  Adjudikation zeichnet sich ab: Fs Silent-Miss-Rate → nahe 1,0 (alles, was ihm fehlt, fehlt
  lautlos), Ledger ~0,17–0,24. Das ist der messbare Kern des Workflow-Nutzens auf der
  Selbstdiagnose-Achse — exakt die Forschungsfrage aus §11 v2.

## ★ GESAMTSCHAU: Direktvergleich Agent (F) vs. Workflow (Ledger)

### Was wie getestet wurde (Kurzmethodik)

- **Aufgabe (beide identisch):** dasselbe kanonisch nummerierte Transkript (AU-Locators aus dem
  geteilten deterministischen Segmenter), dasselbe Modell (gpt-5.4), dieselbe Zieldefinition
  (fachliche Essenz vollständig + belegt), bewertet gegen denselben autor-konsolidierten
  Gold-Referenzbestand (meeting-2: 31 Units · Interview: 108 Units).
- **Bedingungen:** F = freier Agent (Tool-Loop: lesen/prüfen/einreichen, deterministische
  Formvalidierung, Budget 40) · L = Ledger-Maker-Stand vor maschineller QC (capture) ·
  LCR = danach, vor Mensch. Je Fall N=3 → 18 Beobachtungen.
- **Metriken:** E2a formneutral (P1/P2 via Ähnlichkeits-HEURISTIK als Band — offizielle Werte
  nach Evaluator-Adjudikation; P3/Presence deterministisch; Tokens aus OTel) + E2b mechanisch
  (Themen-Coverage, Stufen-Verlust, Gate-Zulauf/-Potenzial, blinde Misses, Provenienz-Audit —
  deterministisch). Verdichten wird NICHT bestraft (Split-Regel); gemessen wird Inhaltserhalt.
- **Nachträge:** F-v2-Probeläufe (missionsgleicher Vertrag inkl. Pflicht-Quellenbilanz, je 1×)
  für die Selbstdiagnose-Achse.

### Das Duell in acht Dimensionen (* = Heuristik-Band, unadjudiziert)

| Dimension | F (Agent) | Ledger (Workflow) | Befund |
| --- | --- | --- | --- |
| Atomare Abdeckung, kurzer Fall* | ~0.29–0.74 | ~0.32–0.68 | Unentschieden |
| Atomare Abdeckung, langer Fall* | ~0.12–0.56 | ~0.03–0.21 | **F klar** |
| Themen-Abdeckung, langer Fall | 0.72–0.79 | 0.52–0.59 | F, deutlich knapper |
| Präzision, kurzer Fall* | ~0.27–0.68 | ~0.40–0.90 | **Ledger klar** |
| Rauschen | ~14 Kontext-Aussagen + Grenzfälle | gering | Ledger |
| Kosten (Tokens, langer Fall) | ~101k/18k | ~136k/41k | F (≈40 % günstiger) |
| Referenz-Gültigkeit (P3) | 1.0 | 1.0 | beide perfekt |
| **Selbstdiagnose („weiß, was fehlt?")** | **nein** — `unresolved`=leer (F-v2, beide Fälle); 0/32 der härtesten Stellen erkannt; Silent-Miss → ~1.0* | **ja, überwiegend** — ~80 % der eigenen Misses als Gate-Zettel; blind 17–24 % | **Ledger, haushoch** |
| Herkunft/Transformation beweisbar | Referenzen ja, Transformationskette nein | **100 % lückenlose Kette (6/6 Läufe)** | **Ledger** |

Zwei Null-Befunde mit Aussagekraft: **L≡LCR** (die maschinelle QC-Stufe greift inhaltlich nie
ein — sie ist Detektor fürs Human Gate, §26-Erwartung 6) und **F nutzt seine Freiheit nicht**
(alle 8 Läufe identisches Minimal-Muster: 4 Runden · 1 Re-Read · 1 Validierung · 0 Revisionen).

### Der Nutzen, präzise formuliert (Einsatzprofil statt Siegerliste)

**Der Agent findet mehr und kostet weniger — der Workflow weiß, was er tut.** F liefert auf
langem, unstrukturiertem Input mehr atomaren Inhalt zu geringeren Kosten, aber als
unbelegbare Vollständigkeits-BEHAUPTUNG: Was ihm fehlt, fehlt lautlos (Bilanz formal perfekt,
inhaltlich ungeprüft erzählt). Der Ledger liefert weniger, präziser, verdichtet — mit
ERRECHNETER Rechenschaft: ~80 % der eigenen Lücken selbst signalisiert, jede Aussage mit
lückenloser Entstehungskette, Reste als konkrete Prüf-Zettel für den Menschen.

**Einsatzregel (die Antwort auf die Forschungsfrage in einem Absatz):** Für eine
EINMAL-Analyse, deren Ergebnis ein Mensch ohnehin vollständig liest, ist der freie Agent die
effizientere Wahl. Für eine EVIDENZBASIS, auf der ungeprüft weitergebaut wird
(Baselines → Backlog → Issues) und die über Zeit fortgeschrieben werden muss, ist der
strukturierte Workflow die Voraussetzung — nicht weil er mehr findet, sondern weil nur er
belegen kann, WAS er hat, WOHER es kommt und WAS NOCH FEHLT. (Zulässige Aussageform gemäß
§25/§26; Konstrukt-Grenzen: Abschnitt Befund-Einordnung B5.)

## Nächste Schritte (★ erweitert 05.09. um den E2a/E2b-Schnitt, Konzept §13.3c)

1. Evaluator-Adjudikation (E2a): verblindete `matchblatt-*.md` (Auflösung separat) — Autor
   urteilt voll/teilweise/kein je Aussage; 372 vorgefilterte Grenzfälle zuerst.
2. Danach exakte P1/P2 (+P4-Blätter) aus den Urteilen rechnen — IMMER mit Kompressionsfaktor
   und Konstrukt-Einordnung (B5) daneben.
3. **E2b (NEU, pilot-motiviert deklariert):** Stufen-Verlust-Analyse (inkl. L_kand) ·
   Miss-Signal Detection-P/R · Human-Gate-Wirkung (Autor ohne Gold-Sicht, Bias als Obergrenze
   ausgewiesen) · Provenienz-/Transformations-Audit — Ergebnisform: PROFIL statt Siegerliste.
4. Zweitannotation/Adjudikation/Hash ② bleiben für die OFFIZIELLEN Zahlen Voraussetzung;
   Gold-Änderungen ab jetzt NUR aus der Zweitannotation, nie aus Arm-Ergebnissen.

---

## ★★ NACHTRAG 06.09. (korrigiert): Vorläufige adjudizierte Hauptauswertung

**Verfahrens-Offenlegung (Korrektur 06.09.):** Alle 139+139 Gold-Urteile wurden KI-adjudiziert
(Claude, Regeln v3.1, jedes Urteil mit Beleg) und anschließend in einem SEPARATEN KI-gestützten
Verifikationsdurchgang (Kollegen-KI) anhand der vorab festgelegten Blätter kontrolliert
(8 Kippungen, Übereinstimmung 90,9–100 %, Eskalationsschwelle nie gerissen). Ein früheres
Etikett „human-verifiziert / unabhängiger Zweitprüfer" war FALSCH und ist zurückgenommen —
**es gab noch KEINE menschliche Prüfung.** Zwei unabhängige KI-Durchgänge senken das Risiko
einzelner Fehlurteile, ersetzen aber keinen Menschen (Projektregel: KI zählt nicht als
Zweitannotator). Menschliche Prüfung (Autor über die vorhandenen Blätter) bleibt VOR der
Thesis-Verwendung Pflicht.

**Aussagegrenzen:** ① Gold = „KI-Erstentwurf, autor-konsolidiert" (Zweitannotation/Hash ②
ausstehend) → noch keine endgültigen Thesis-Zahlen. ② Je Fall und Arm wurde EIN vorab per
deklarierter Regel ausgewählter Lauf vollständig adjudiziert (F: offizieller v2-Einzellauf;
Ledger: chronologisch erster Pilot-Lauf) — alle Aussagen gelten für DIESE Läufe; stabiles
Laufverhalten erst nach N=3. ③ ERLEDIGT 06.09.: Der Stille-Misses-Check ist durch die Autor-Stichprobe
bestätigt (Prüfbasis, 16 Leer-Schnitte) — die Silent-Miss-Rate 0.308 steht ohne Vorbehalt.

### Ergebnisse der adjudizierten Läufe (ersetzen die *-Bänder oben)

| | meeting-2: F | Ledger | Interview: F | Ledger |
| --- | --- | --- | --- | --- |
| Claims geliefert | 37 | 20 | 117 | 47 |
| Full Coverage | 0.903 | 0.871 | 0.657 | 0.519 |
| Touched Coverage | 0.935 | 0.903 | 0.852 | 0.778 |
| Strict Precision | 1.0 | 1.0 | 0.991 | 1.0 |
| Lücken (partial+none) | 3 | 4 | 37 | 52 |
| davon signalisiert | 0 | 0 | 0 | 36 |
| Silent-Miss-Rate | 1.0 | 1.0 | 1.0 | 0.308 |
| Addressierbarkeit (Full + signalisierte Lücken) | = Full | = Full | 0.657 | 0.852 |
| Tokens in/out (DIESELBEN Läufe) | 23,0k/5,7k | 26,7k/13,5k | 105,2k/19,6k | 182,2k/41,1k |

**Signal-Qualität (NEU, deterministisch AU-basiert):** Die 36 signalisierten Lücken kosten
Aufmerksamkeit: Von den 48 Interview-Prüf-Zetteln sitzen nur 18 auf Quell-Units echter Lücken
(Signal-Miss-Precision 0.375); 6 adressieren bereits Gedecktes, 24 gold-freie Kontext-Units.
Der Mensch am Gate muss also ~2,7 Zettel je lückenrelevantem Zettel sichten (attach_as_evidence-
Zettel haben dabei eigenen Evidenz-Wert, aber keinen Lücken-Ertrag). Belegt ist damit, DASS der
Ledger Lücken sichtbar macht — die Lenkung der Aufmerksamkeit ist messbar unpräzise.

### Was in diesen Läufen klar geworden ist

1. **Die Heuristik hat den Ledger massiv unterschätzt** (Band 0.03–0.21 → adjudiziert 0.519):
   Coverage verdichteter Bestände ist nur semantisch messbar, nicht per Token-Overlap.
2. **Präzision trennt die Arme nicht** (0.991/1.0 vs. 1.0). Lehrreiches Detail: F bog eine
   fehlerhafte Quellstelle plausibel zurecht („Dart 3.1.5"), der Ledger-Claim zur selben Stelle
   lässt die Zahlen weg statt zu raten.
3. **Der zentrale Unterschied ist die Lücken-SICHTBARKEIT:** F meldet 0 von 37 Lücken (trotz
   formal perfekter Pflicht-Quellenbilanz), der Ledger adressiert 36 von 52 als Prüf-Zettel.
4. **Addressierbarkeit ist PRÜFPOTENZIAL, keine erreichte Coverage:** Ein Signal stellt den
   fehlenden Inhalt nicht wieder her — es macht ihn auffindbar. Dass die Ledger-
   Addressierbarkeit (0.852) numerisch Fs Touched Coverage (0.852) gleicht, ist Koinzidenz
   zweier VERSCHIEDENER Konstrukte, keine Gleichwertigkeit.
5. **Die stillen Ledger-Lücken haben eine präzise Fehlerklasse:** alle 20 (4 meeting-2 [3 none + 1 partial] + 16 Interview) sind
   Detailverluste INNERHALB verwendeter Units — dort greift die Unused-Unit-Mechanik
   konstruktionsbedingt nicht. Vorregistrierte Grenze, jetzt gemessen.
6. **Am gutmütigen Fall verschwindet der Unterschied** (beide fast perfekt, beide still) —
   die Differenzierung entsteht erst unter Last; das Zwei-Fälle-Design war nötig.
7. **Kosten (dieselben adjudizierten Läufe):** F braucht im Stressfall ~58 % der Input- und
   ~48 % der Output-Tokens des Ledgers. Der Ledger-Mehraufwand kauft Bilanz + Signale +
   Provenienz, nicht mehr Inhalt.

### Was diese Auswertung belegt — und was nicht (stufen-ehrlich)

NICHT belegt ist ein Vorteil einzelner Stufen: Für die reine EXTRAKTION ist kein Vorteil
nachgewiesen (F ≥ Ledger); Checker/Repair veränderte keinen Claim (L≡LCR — Detektor, kein
Korrektor); die Kanonisierung verursachte einen Teil der Detailverluste; die Segmentierung
war herauskontrolliert (beide erhielten dieselben AUs). BELEGT ist die Daseinsberechtigung
der Kontroll-Mechanismen: deterministische Unit-Bilanz, explizite Behandlung ungenutzter
Units, Miss-Signale (mit gemessener, verbesserungswürdiger Präzision), nachvollziehbare
Kandidat→Claim→Quellen-Kette, strukturierter validierter Consumable.

**Einsatzprofil (zulässige Formulierung):** In den adjudizierten Läufen konnte ein starkes
freies Modell die reine Extraktionsaufgabe mindestens ebenso gut lösen — günstiger und mit
höherer atomarer Abdeckung. Der Ledger rechtfertigt seinen Mehraufwand nicht durch bessere
Einzeltexte, sondern dadurch, dass er eine extern überprüfbare Verarbeitungsbilanz erzeugt
und im untersuchten Stressfall einen erheblichen Anteil seiner inhaltlichen Lücken (69 %,
vorbehaltlich ③) als konkreten Prüfbedarf sichtbar macht — während die Lücken des freien
Agenten vollständig unsichtbar bleiben. Kontrollierbare Verarbeitung statt bessere Texte.

### Offen vor Thesis-Verwendung (Stand 06.09. spät)

1. ✅ ERLEDIGT: Menschliche STICHPROBEN-Verifikation durch den Autor (Seed-Auswahl: 16 Blöcke,
   davon 5 Golds beidseitig, + Stille-Misses-Prüfbasis; alle bestätigt, 0 Kippungen) — Etikett
   der Interview-Messungen jetzt „KI-adjudiziert + KI-verifiziert + Autor-Stichprobe";
   meeting-2 bleibt ohne menschliche Stichprobe. 2. ✅ Stille-Misses-Prüfbasis bestätigt.
3. OFFEN: Gold-Zweitannotation + Hash ② (externer Mensch) — bis dahin „vorläufige adjudizierte
   Hauptauswertung". 4. Optional: N=3 (Varianz), Fault-Injection. (Modellsensitivität: ✅ Matrix
   4×2 vorhanden, sekundär/unverifiziert.)

### Erledigt/Offen gegenüber „Nächste Schritte" oben

Schritt 1–2: ✅ in schärferer Form (Urteils-Verfahren v3.1, KI-Doppel-Durchgang statt
Matchblätter). Schritt 3 (E2b): Kern ✅ (Detection/Silent-Miss/Addressierbarkeit/Signal-
Qualität); Human-Gate-WIRKUNG offen. Schritt 4 (Zweitannotation/Hash ②): OFFEN.

### Modellsensitivität (06.09., sekundär: gpt-4.1-mini, nur Stressfall — KI-adjudiziert, unverifiziert, N=1)

| Interview · gpt-4.1-mini | Arm F | Ledger (LCR) |
| --- | --- | --- |
| Ergebnis | **TOTALAUSFALL 0/2 Läufen** (Erzähl-Text statt Tool-Calls; Wiederholung: 1 Read, keine Einreichung) | 13 Claims, alle Gates pass |
| Full/Touched Coverage | — (0 Aussagen) | 0.194 / 0.315 |
| Strict Precision | — | 0.923 (CAN-004 trägt die später verworfene Videoseite) |
| Detection / Silent-Miss | — (kein Artefakt, keine Bilanz) | **0.897 / 0.103** (117/136 Units unbenutzt → 104 Signale) |
| Addressierbarkeit | — | 0.917 |
| Tokens in/out | 15,2k/2,5k + 34,1k/1,3k (ohne Ertrag) | 92,5k/28,6k |

Befund: **Die Struktur macht das schwache Modell nicht gut — sie macht sein Versagen
beobachtbar.** Der freie Arm hängt an einem freiwilligen Tool-Protokoll und kollabiert ohne
jedes Artefakt; die Kette degradiert kontrolliert (Gates pass, Referenz-Repair feuerte erstmals
real 2×) und dokumentiert ihren eigenen Einbruch. Dabei zeigt sich eine UMKEHR-Beziehung zur
Kernmessung: je schwächer das Modell, desto höher die Detection (0.897 vs. 0.692) — schwache
Modelle verlieren GANZE Units (bilanzierbar), starke Modelle verlieren Details INNERHALB
verwendeter Units (blinde Klasse). Aussagegrenze: sekundäre Analyse, ein Lauf je Arm
(+1 dokumentierte F-Wiederholung), KI-adjudiziert ohne Verifikationsdurchgang.

### ★ Modellsensitivitäts-MATRIX komplett (06.09. spät — 4 Modelle × 2 Arme, Interview-Stressfall; KI-adjudiziert, unverifiziert, N=1 je Zelle, F-Ausfälle je 1× wiederholt)

| Modell | F: Full/Touched | F: Lücken gemeldet | Ledger: Full/Touched | Ledger: Detection / Silent-Miss | Ledger: Adressierbarkeit |
| --- | --- | --- | --- | --- | --- |
| gpt-5.4 (stark) | 0.657 / 0.852 | 0 von 37 | 0.519 / 0.778 | 0.692 / 0.308 | 0.852 |
| gpt-5-mini | 0.574 / 0.852 | 0 von 46 | 0.176 / 0.417 | 0.775 / 0.225 | 0.815 |
| gpt-oss-120b | **AUSFALL 0/2** | — (kein Artefakt) | 0.185 / 0.315 | 0.898 / 0.102 | 0.917 |
| gpt-4.1-mini | **AUSFALL 0/2** | — (kein Artefakt) | 0.194 / 0.315 | 0.897 / 0.103 | 0.917 |

Vier Befunde aus der Matrix (alle Läufe/Urteile: w2-modellsensitivitaet-eval-*.json):

1. **Fs Sollbruchstelle ist Tool-Following, nicht Preis:** gpt-oss-120b und gpt-4.1-mini
   „kennen" die Aufgabe (beide schrieben den Claim-Bestand als TEXT in die Antwort — einer
   sogar als Markdown-Tabelle), reichen aber nie ein → 0 Artefakte in 4 Läufen. gpt-5-mini
   (tool-trainiert) liefert dagegen 53 Aussagen mit Touched 0.852 = Niveau des starken Modells.
2. **Fs einziger deterministischer Baustein rettete den gpt-5-mini-Lauf:** 3 Selbstrevisionen,
   alle vom ValidateBilanz-Guard erzwungen — selbst im „freien" Arm kam die Rettung von der
   Struktur. Und trotzdem: unresolved=0, wieder NULL gemeldete Lücken (0/46).
3. **Die Ledger-Kette schleuste JEDES Modell durch** (alle Gates pass, auch beim Modell mit
   F-Totalausfall). Extraktion degradiert massiv (0.52 → 0.18), die Buchführung NICHT:
   Detection steigt sogar (0.69 → 0.90), weil schwache Modelle GANZE Units liegen lassen
   (bilanzierbar), während starke Modelle Details INNERHALB verwendeter Units verlieren
   (blinde Klasse). Die FORMALE Bilanzierbarkeit blieb modellübergreifend erhalten
   (Adressierbarkeit 0.82–0.92); semantische Qualität und Umfang der Prüfhinweise variierten
   modellabhängig (schwache Modelle reichen viele Units pauschal als Prüfbedarf weiter).
4. **Preis-Ehrlichkeit:** Schwache Ledger-Läufe zeigen erstmals Precision-Fehler (erfundene
   'Angehörigen-Accounts', verfestigtes Caching, konservierter Video-Widerspruch C2↔C6) und
   die Zettel-Flut wächst (83–104 Signale). Adressierbarkeit bleibt PRÜFPOTENZIAL: ein
   Mensch muss pro Zettel arbeiten.

Satz fürs Kapitel: **Je schwächer das Modell, desto größer der relative Wert der Struktur —
beim freien Agenten entscheidet die Agent-Tauglichkeit des Modells über ALLES ODER NICHTS,
bei der Kette nur über die Ausbeute; die Rechenschaft bleibt.**
