# W2-Gold-Leitfaden — die EINE Anleitung für die Hand-Annotation (v01, ENTWURF zur Fixierung)

> Status: MOMENTAUFNAHME 04.09.2026 — wird VOR Beginn der Annotation vom Autor abgenommen und
> zusammen mit Gold + Matchregeln gehasht (`benchmark-checksums.txt`). Danach unveränderlich.
> Protokoll: `Thesis-Docs/Writing/W2-Evaluationskonzept-Kapitel-7.md` (§6, §12.0-5, §29).

Dieser Leitfaden enthält ALLES, woran du dich bei M-4 orientierst: ① die harten Herkunftsregeln,
② das Gold-Format mit SYNTHETISCHEM Beispiel (bewusst kein Benchmark-Material — sonst kennte der
Zweitannotator die erwarteten Labels), ③ die Annotations-Regeln je Typ, ④ den Ablauf inkl.
unabhängiger Zweitannotation, ⑤ die Matchregeln-Essenz, ⑥ den armneutralen Ziel-Vertrag
(bewusst OHNE die Arm-Prompts — Gold darf sich an keinem Arm ausrichten).

---

## 1. Die harten Herkunftsregeln (§12.0-5 — nicht verhandelbar)

1. **Nur die Rohquelle (v01-Fix2, präzisiert — Widerspruch zu §1.2 aufgelöst):** Die fachliche
   Validierung/Annotation erfolgt AUSSCHLIESSLICH gegen die nummerierten Quelldateien (unten).
   Außer dem OFFENGELEGTEN KI-gestützten Gold-Erstentwurf (§1.2) dürfen keine System-, Ledger-
   oder Arm-Outputs eingesehen werden: kein Ledger-Output, kein Referenz-Ledger
   (`*.reference-ledger.json` = modellgeneriertes SILBER — tabu!), kein Arm-F-Lauf,
   keine capture/-Dateien.
2. **Zweistufiger Freeze (v01-Fix2, Kollegen-P0 — ehrliche Formulierung):** **Hash-Stufe ①**
   fixiert den finalen Gold-Leitfaden, die Matchregeln und die nummerierten Quellen VOR der
   primären menschlichen Validierung und Finalisierung des Referenzbestands sowie vor der
   offiziellen W2-Auswertung. Soweit ein KI-gestützter ERSTENTWURF des Referenzbestands bereits
   zuvor existierte (Interview-Fall), wird diese Abweichung über die Provenienzangabe der
   Gold-Datei und die Threats to Validity (§24.1) offengelegt — Hash ① behauptet dann NICHT,
   dass die Regeln jedem Entwurf vorausgingen, sondern dass Validierung, Zweitannotation und
   Auswertung unter eingefrorenen Regeln stattfanden. **Hash-Stufe ②** nach Validierung +
   Zweitannotation + Gold-Adjudikation: finaler Goldbestand. **Terminologie in der Thesis:**
   „human-validierter Referenzbestand mit KI-gestütztem Erstentwurf", nicht pauschal „Hand-Gold".
3. **Zweitannotation = unabhängige QUELL-Annotation, kein Review deiner Units — mit EXAKTER
   Samplingregel (v01-Fix, kein „bzw." mehr):** Je Fall werden VOR Beginn der Zweitannotation
   **drei zusammenhängende Fenster** festgelegt — je eines im Anfangs-, Mittel- und End-Drittel
   der Quelle, DETERMINISTISCH beginnend bei 10 %, 45 % und 80 % der AU-Indizes (aufgerundet).
   Die Fenster werden so lang gewählt, dass ihre Ziel-AUs zusammen **mindestens 20 % aller AUs
   (mindestens 10 Ziel-AUs)** umfassen — gleichverteilt auf die drei Fenster. Die Auswahl wird
   VOR Beginn gespeichert (Datei neben dem Gold). Unmittelbar angrenzende AUs dürfen als
   Kontext angezeigt werden; ANNOTIERT und VERGLICHEN werden nur die Ziel-AUs. Der Kollege
   annotiert diese Fenster mit DIESEM Leitfaden komplett selbst — erst danach Vergleich mit
   deiner Erstannotation (fehlende/zusätzliche Propositionen, Granularität,
   Typ/Qualifikationen). Uneinigkeiten werden protokolliert und per Gold-Adjudikation
   aufgelöst (Protokoll = Beleg).
4. **Nach Hash ② nie wieder anfassen** — auch nicht, wenn später ein Arm etwas Richtiges
   findet, das im Gold fehlt (das wird als `gold_escape`-Diagnose berichtet, Matchregeln §4).
5. **Präzise Unabhängigkeits-Formel (für Kapitel 7):** Die fachliche Propositionierung und
   Goldabgrenzung erfolgt unabhängig von den Outputs der Vergleichsarme; zur einheitlichen
   Quellenadressierung verwenden Gold und alle Arme dieselbe deterministisch erzeugte
   Quellsegmentierung (AU-Locators). Nicht behaupten: „vollständig workflowunabhängig".

**Arbeitsgrundlage (deterministisch erzeugt, identische Nummerierung wie in allen Armen):**

- `input/eval-labels/meeting-2-extended.numbered.txt` (36 Units) — Treue-/Ceiling-Fall
- `input/eval-labels/Interview-Einrichtung.numbered.txt` (136 Units) — Coverage-Stressfall

---

## 2. Das Gold-Format

Je Fall EINE Datei: `input/eval-labels/meeting-2-extended.w2-gold.json` bzw.
`Interview-Einrichtung.w2-gold.json`. **ID-Schema (v01-Fix2):** `G-M2-nnn` (meeting-2) ·
`G-IE-nnn` (Interview), dreistellig und EINDEUTIG; dokumentierte Lücken (per §3d gestrichene
Units, im `excludedByRule`-Block belegt) sind zulässig — bewusst KEINE Neu-Nummerierung, weil
Notes und Review-Protokolle per ID quer-referenzieren und eine Umnummerierung die Audit-Spur
korrumpieren würde.

**Das Beispiel unten ist bewusst SYNTHETISCH** (erfundenes Mini-Meeting einer Vereins-App,
Fantasie-IDs AU-9xxx) — Benchmark-Passagen dürfen im Leitfaden nicht vor-annotiert werden,
sonst kennte der Zweitannotator für diese Stellen bereits die erwarteten Labels.

Synthetischer Quell-Ausschnitt:

```text
[AU-9001] Kassenwart: Bevor wir starten — die Heizung im Vereinsheim klemmt mal wieder.
[AU-9002] Vorstand: Zur Mitgliederliste: Wir brauchen einen Export als CSV-Datei.
[AU-9003] Kassenwart: Und Mahnungen sollen automatisch rausgehen, spätestens 14 Tage nach Fälligkeit.
[AU-9004] Vorstand: Zum Vereinslogo im Briefkopf hatten wir zwei Vorschläge. Wir nehmen das blaue — das grüne ist damit vom Tisch. Und lasst uns zusätzlich eine Druckvorlage anbieten, in der das Logo schon eingebettet ist.
[AU-9005] Kassenwart: Gut, dann machen wir das so mit dem blauen Logo.
```

Daraus wird dieses Gold:

```json
{
  "source": "vereins-beispiel (synthetisch — NUR Leitfaden-Illustration)",
  "annotator": "Autor",
  "leitfadenVersion": "v01",
  "units": [
    {
      "id": "G-BSP-001",
      "statement": "Die Mitgliederliste kann als CSV-Datei exportiert werden.",
      "type": "requirement",
      "sourceUnitIds": ["AU-9002"],
      "note": ""
    },
    {
      "id": "G-BSP-002",
      "statement": "Mahnungen werden automatisch versendet, spätestens 14 Tage nach Fälligkeit.",
      "type": "requirement",
      "sourceUnitIds": ["AU-9003"],
      "note": "Zahlwert '14 Tage' ist WESENTLICH — ohne ihn wäre ein Match höchstens teilweise."
    },
    {
      "id": "G-BSP-003",
      "statement": "Als Vereinslogo wurde der blaue Vorschlag festgelegt; der grüne wurde verworfen.",
      "type": "decision",
      "sourceUnitIds": ["AU-9004", "AU-9005"],
      "note": "Entscheidung inkl. verworfener Alternative; AU-9005 = Bestätigung als Zusatz-Referenz."
    },
    {
      "id": "G-BSP-004",
      "statement": "Es wird eine Druckvorlage mit bereits eingebettetem Logo angeboten.",
      "type": "requirement",
      "sourceUnitIds": ["AU-9004"],
      "note": "Zweiter Sachverhalt AUS AU-9004 — Split: eine Unit, zwei Gold-Einträge."
    }
  ]
}
```

Nicht annotiert: AU-9001 (Logistik/Smalltalk — die Heizung ist kein Sachverhalt des Produkts).

---

## 3. Annotations-Regeln

### 3a. Was eine Gold-Unit ist

- **Atomar:** eine Unit = EIN fachlicher Sachverhalt. Sagt eine AU zwei Dinge (wie AU-9004:
  Logo-Entscheidung + Druckvorlagen-Idee) → ZWEI Gold-Units.
- **Wesentliche Qualifikationen gehören INS Statement:** Bedingungen, Zahlwerte („spätestens
  14 Tage"), Negationen, Träger/Geltungsbereich. Grund: Die Matchregeln verlangen sie für einen
  VOLL-Match — steht die Qualifikation nicht im Gold, kann sie nie gemessen werden.
- **sourceUnitIds:** alle Units, die zur VOLLSTÄNDIGEN Interpretation der Aussage benötigt
  werden (bei Entscheidungen auch die verworfene Alternative). Reine Wiederholungs-/
  Zustimmungsäußerungen sind als Zusatz-Referenz OPTIONAL — kein Zwang zur Maximalmenge.
- **Deutsch, eigenständig lesbar,** ohne „siehe oben"-Bezüge.

### 3b. Typwahl (genau EINER je Unit — Beispiele bewusst synthetisch)

| Typ | Wann | Synthetisches Beispiel |
| --- | --- | --- |
| `requirement` | Gefordertes/gewünschtes Verhalten oder Merkmal des Produkts (auch bestätigter Bestand, auch NFR) | „Die Mitgliederliste kann als CSV exportiert werden." |
| `architecture` | Technischer Rahmen/Constraint, der die LÖSUNG bindet (nicht das WAS, sondern das WOMIT/WORIN) | „Die Daten bleiben auf dem Vereinsserver, keine Cloud." |
| `decision` | Im Gespräch GETROFFENE Festlegung, inkl. verworfener Alternativen | „Das blaue Logo wurde festgelegt, das grüne verworfen." |
| `open_question` | Ausdrücklich offen Gebliebenes / vertagter Klärungsbedarf | „Ob Gastmitglieder Zugriff bekommen, klären wir mit dem Vorstand." |
| `risk` | Geäußerte Sorge/Gefahr, die im Gespräch Sorge BLEIBT (keine beschlossene Gegenmaßnahme) | „Wenn die Beitragsdaten veralten, mahnen wir falsche Leute an." — sobald daraus eine Forderung wird („das System muss … verhindern"), ist es requirement |

### 3c. Entscheidungsbaum für Grenzfälle

1. Wurde es festgelegt/verworfen? → `decision`. **Doppelungs-Sperre (v01-Fix, Kollegen-Punkt):
   Derselbe Sachverhalt wird NICHT allein wegen eines anderen Typs doppelt aufgenommen.** Eine
   ZUSÄTZLICHE decision-Unit neben einer requirement-Unit ist nur zulässig, wenn der
   Entscheidungscharakter selbst zusätzliche fachliche Information trägt (verworfene
   Alternative, Verbindlichkeit, expliziter Entscheidungsabschluss) — z. B. requirement
   „Das Logo erscheint im Briefkopf" + decision „Das blaue Logo wurde festgelegt, das grüne
   verworfen" sind ZWEI Informationen; „X gilt" und „es wurde entschieden, dass X gilt" sind EINE.
2. Fordert es Produktverhalten? → `requirement` (auch „nicht verhandelbare Auflagen" von außen:
   requirement; nur wenn sie die technische LÖSUNG binden → `architecture`).
3. Blieb es ausdrücklich offen? → `open_question`. 4. Bleibt es eine unadressierte Sorge? → `risk`.
5. Im Zweifel: Typ nach bestem Urteil + `note` mit dem Zweifel — der Typ kostet ohnehin keine
   P1/P2-Punkte (nur Diagnose, Matchregeln §5).

### 3d. Was NICHT ins Gold gehört (= die vorab dokumentierte Ausschlussregel, P1-Nenner!)

- Smalltalk/Logistik (Kaffee, defekte Heizung, „ich muss gleich weg").
- Moderation („Dann fangen wir an", „Nächster Punkt").
- **Ist-Zustands-, Begründungs- und Projektorganisations-Aussagen (v01-Fix, Review-Fund):**
  Gold umfasst PRODUKT-/VORHABENSBEZOGENE Sachverhalte (Anforderungen, technischer Rahmen,
  Entscheidungen, offene Fragen, Risiken des zu bauenden Systems). Reiner Kontext gehört NICHT
  hinein: wie heute gearbeitet wird („es gibt klassische Akten"), Begründungen/Motivationen,
  Team-/Termin-Organisation („Kennenlern-Tag vereinbaren", „jeder installiert bis zum nächsten
  Treffen"), Werkzeug-Tipps ohne Festlegung. **Technik-Grenze (v01-Fix2):** Werkzeug-/
  Technik-Festlegungen, die das PRODUKT binden (Framework, Datenbank, Zielplattform/-version,
  Packages, einheitliche Versionsstände) = `architecture`-Gold; reine TEAM-Werkzeuge und
  Arbeitsregeln (IDE-Wahl, Kommentarregeln, Installations-Aufgaben) = ausgeschlossen.
  Diese Regel gilt für ALLE Arme gleich — Arm-Aussagen
  dieser Klassen werden im Matching einheitlich als gold_escape geführt, nie mal so, mal so.
- Reine Zustimmung ohne eigenen Inhalt — aber als optionale ZUSÄTZLICHE Referenz an der
  bestätigten Unit erlaubt.
- Eigene Ideen, die NICHT im Transkript stehen (Gold = was gesagt/entschieden wurde, keine
  RE-Vervollständigung — das wäre die Analyst-Aufgabe, nicht die Gold-Aufgabe).

**Ende-Kriterium:** „Quelle vollständig durchgearbeitet und Selbst-Durchsicht abgeschlossen" —
es gibt bewusst KEINE Zielgröße (Zahlen ankern; Aufwandsschätzungen stehen nur in der
Projektplanung, nicht hier).

---

## 4. Ablauf (Schritt für Schritt)

1. Diesen Leitfaden final lesen → Abnahme (ggf. Korrekturen JETZT, danach eingefroren).
2. **Hash-Stufe ①:** Leitfaden + `w2-matchregeln.md` + die zwei .numbered.txt →
   `benchmark-checksums.txt`. **ERST DANACH beginnt die primäre menschliche
   Referenzbestands-Bildung — beim Interview: die aktive Unit-für-Unit-Validierung des
   KI-Kandidaten (bestätigen/ändern/streichen), bei meeting-2: die Annotation.** (Reihenfolge
   HART: nicht erst validieren und dann hashen — sonst verliert Hash ① seinen Sinn.)
3. Fall 1 (meeting-2, der kleine — als Kalibrierung zuerst): nummerierte Quelle durchgehen,
   Unit für Unit fragen: „Steckt hier ein fachlicher Sachverhalt? Schon erfasst? Neue Gold-Unit?"
4. Fall 2 (Interview) genauso — in Etappen; auch implizit in Erzählform Verstecktes gehört
   hinein, wenn es im Transkript GESAGT ist (keine eigene Vervollständigung, 3d).
5. Selbst-Durchsicht: Atomarität, Qualifikationen im Statement, Referenzen vollständig.
6. **Unabhängige Zweitannotation (Kollege, je Fall):** nach der EXAKTEN Samplingregel aus §1.3
   (drei deterministische Fenster bei 10 %/45 %/80 %, Ziel-AUs ≥20 %/min. 10, Auswahl vorab
   gespeichert). Der Kollege annotiert die Fenster KOMPLETT SELBST mit diesem Leitfaden (er
   sieht deine Units vorher NICHT) — dann Abgleich; Uneinigkeiten protokollieren + per
   Gold-Adjudikation auflösen (Protokoll aufbewahren).
7. **Hash-Stufe ②:** finale Gold-Dateien → `benchmark-checksums.txt` ergänzen. Ab jetzt
   unveränderlich; erst DANACH erste offizielle W2-Läufe.

---

## 5. Matchregeln-Essenz (Vollversion: `w2-matchregeln.md` — die gilt)

- Gematcht wird der **fachliche Sachverhalt** (Paraphrase zählt), in Stufen **voll / teilweise /
  kein Match**; falsche Zahl = höchstens teilweise, falsche Negation = kein Match. P1/P2 primär
  strict (nur voll).
- **Asymmetrie:** P1 (Coverage) darf mehrere Arm-Aussagen zur Deckung EINER Gold-Unit bündeln;
  P2 (Precision) beurteilt jede Output-Aussage EINZELN — Verbund-Teilaussagen bleiben dort
  „teilweise".
- **P4 auf Aussagenebene:** die VEREINIGUNG der auflösbaren referenzierten Units muss die
  Aussage tragen; der Evaluator urteilt unabhängig vom Selbst-Label des Arms.
- **Typ = reine Diagnose** (nie P1/P2-Punkte); beim Ledger nur aus `kind` gemappt, Facetten
  bleiben komplett draußen (R-8/9h-Leitplanke).
- Richtiges ohne Gold-Gegenstück = `gold_escape`-Taxonomie (Diagnose — Zahl/Rate wird immer
  berichtet, Gold bleibt zu): `gold_escape` · `supported_gold_escape` (P4-gestützt,
  arm-übergreifend) · `supported_derived_gold_escape` (F-Untertyp).
- Verfahren: verblindete Listen, Matchlog mit Begründungen, Zweitblick auf Grenzfälle —
  alles **Evaluator-Adjudikation**, strikt getrennt von deiner Ledger-Adjudikation.

**Konsequenz fürs Annotieren:** Schreibe Gold-Statements so, wie ein VOLL-Match aussehen muss —
präzise, mit den messbaren Qualifikationen. Ein schwammiges Gold-Statement macht die strengste
Metrik weich.

---

## 6. Der armneutrale Ziel-Vertrag (v01-Fix: Arm-Prompts gehören NICHT in die Annotations-Anleitung)

Das Gold richtet sich an KEINEM Arm aus — deshalb steht hier bewusst nicht, WIE irgendein Arm
instruiert ist (der Arm-F-Prompt liegt eingefroren in
`AgenticSdlc.Host/Prompts/armf/ArmFAgent/ArmFAgent1.txt` und wird beim Tag mit gehasht; Outputs
bleiben bis nach Hash-Stufe ② ungesehen). Was der Annotator wissen muss, ist nur die
**gemeinsame bewertbare Auswertungssicht** (v01-Fix — konsistent zu Matchregeln §1):

> **Messgegenstand: atomare deutsche Aussage · Quellenreferenzen auf die kanonischen
> AU-Locators.** Der fachliche Typ (requirement | architecture | decision | open_question |
> risk) wird armübergreifend nur SEKUNDÄR diagnostisch ausgewertet. `derivation` und
> `uncertainty` sind, soweit ein Arm sie nativ ausgibt, zusätzliche Diagnosefelder —
> keine Voraussetzung für P1–P4. **v1.2 (05.09.):** Zusätzlich liefern beide Konfigurationen
> eine vollständige QUELLENBILANZ (je AU: used→Claim-IDs | non_relevant | unresolved) —
> deterministisch validiert; sie beeinflusst P1–P4 nicht, sondern speist die symmetrische
> Selbstdiagnose-Bewertung (Detection-P/R, Silent-Miss-Rate; Matchregeln §1).

Das Gold definiert unabhängig davon, WAS es in der Quelle zu finden gab — die Arme zeigen
später, wer wie viel davon findet und belegt.
