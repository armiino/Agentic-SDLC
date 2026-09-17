# W2-ERGEBNISSE — konsolidiertes Ergebnisblatt (F vs. Ledger)

> Status: MOMENTAUFNAHME 06.09.2026 — Abschlussstand der AUSWERTUNGSARBEITEN; die Werte selbst
> bleiben per Etikett „vorläufige adjudizierte Hauptauswertung" (Goldstand autor-konsolidiert,
> Zweitannotation per Autor-Entscheid entfallen). Die EINE Ergebnis-Datei
> fürs Kapitel 7. Chronik/Beleg: `runs/w2-official-manifest.txt` · Erzähl-Fassung:
> `runs/w2-pilot-eval/PILOT-BERICHT.md` (★★-Nachtrag) · Methode: `W2-Evaluationskonzept-Kapitel-7.md` §28.

## 1 · Was gemessen wurde (ein Absatz)

Zwei Konfigurationen mit derselben Mission und demselben Ergebnisvertrag — freie agentische
Bearbeitung (Arm F, `F_mission_v2` mit Pflicht-Quellenbilanz) vs. gestufte Ledger-Verarbeitung
(LCR = Stand vor menschlichem Eingriff) — auf derselben kanonisch nummerierten Quelle, demselben
Modell (gpt-5.4), bewertet gegen denselben Gold-Referenzbestand (meeting-2: 31 Units Treue-Fall ·
Interview: 108 Units Coverage-Stressfall). Achse A: Qualität des gelieferten Claim-Bestands.
Achse B: Wie zuverlässig werden tatsächliche Auslassungen als konkrete Prüfbedarfe sichtbar?
Review-Hinweise werden NICHT als erreichte Coverage gutgeschrieben.

## 2 · Kernvergleich (adjudizierte Läufe, Auswahl vorab deklariert)

| | meeting-2: F | Ledger | Interview: F | Ledger |
| --- | --- | --- | --- | --- |
| Claims geliefert | 37 | 20 | 117 | 47 |
| Full Coverage | 0.903 | 0.871 | 0.657 | 0.519 |
| Touched Coverage | 0.935 | 0.903 | 0.852 | 0.778 |
| Strict Precision | 1.0 | 1.0 | 0.991 | 1.0 |
| Lücken (partial+none) | 3 | 4 | 37 | 52 |
| davon signalisiert | 0 | 0 | **0** | **36** |
| Silent-Miss-Rate | 1.0 | 1.0 | **1.0** | **0.308** |
| Adressierbarkeit (= Prüfpotenzial!) | = Full | = Full | 0.657 | **0.852** |
| Tokens in/out (dieselben Läufe) | 23,0k/5,7k | 26,7k/13,5k | 105,2k/19,6k | 182,2k/41,1k |

Abgeleitet: `F1(strict)` aus Strict Precision und Full Coverage (als Recall-Proxy):
meeting-2 F 0.949 · Ledger 0.931 · Interview F 0.790 · Ledger 0.683. €-Kosten erst nach
Autor-Preisgrundlage (freeze-manifest.json §preisgrundlage); bis dahin Token-Zahlen.

Lesehilfen: `Touched = Full + Partial` · `Adressierbarkeit = (Full + signalisierte unvollständige
Gold-Propositionen) / |Gold|` — zwei VERSCHIEDENE Konstrukte, numerische Gleichheit ist kein
Gleichstand. „Claims geliefert" ist eine deskriptive Größenangabe, keine Qualitätsmetrik (die
Arme verdichten unterschiedlich stark).

Signal-Qualität (Interview-Ledger): Von 48 Review-Items waren 18 für mindestens eine tatsächliche
Gold-Lücke relevant (signalbezogene Präzision 0.375; 6 adressieren Gedecktes, 24 gold-freie
Kontext-Units). Im Mittel musste ein lückenrelevanter Hinweis unter ~2,7 geprüften Zetteln
gefunden werden; bezogen auf die 36 erkannten Gold-Lücken rechnerisch 48/36 = 1,33 Zettel je
erkannter Lücke (Many-to-many-Zuordnung).

## 3 · Modellsensitivitäts-Matrix (sekundär, KI-adjudiziert, unverifiziert, N=1 je Zelle)

| Modell | F: Full/Touched | F: Lücken gemeldet | Ledger: Full/Touched | Detection/Silent-Miss | Adressierbarkeit |
| --- | --- | --- | --- | --- | --- |
| gpt-5.4 | 0.657 / 0.852 | 0 von 37 | 0.519 / 0.778 | 0.692 / 0.308 | 0.852 |
| gpt-5-mini | 0.574 / 0.852 | 0 von 46 | 0.176 / 0.417 | 0.775 / 0.225 | 0.815 |
| gpt-oss-120b | AUSFALL 0/2 | — | 0.185 / 0.315 | 0.898 / 0.102 | 0.917 |
| gpt-4.1-mini | AUSFALL 0/2 | — | 0.194 / 0.315 | 0.897 / 0.103 | 0.917 |

## 4 · Die sieben Befunde

1. **Extraktion: kein Ledger-Vorteil.** Ein starkes freies Modell löst die reine Extraktion
   mindestens ebenso gut (Stressfall: 0.657 vs. 0.519 Full) — und günstiger: F verbrauchte
   57,7 % der Ledger-Input- und 47,5 % der Ledger-Output-Tokens (also 42,3 % bzw. 52,5 % weniger
   als der Ledger).
2. **Präzision: praktisch gleich, beide exzellent.** Lehr-Detail: F „reparierte" eine fehlerhafte
   Quellstelle plausibel (Dart 3.1.5), der Ledger ließ die Zahlen weg statt zu raten.
3. **Der zentrale Unterschied ist Lücken-Sichtbarkeit.** In allen ERFOLGREICHEN F-Läufen blieb
   `unresolved` leer (Pflicht-Bilanz formal perfekt) — Silent-Miss 1.0; vier weitere Versuche
   mit zwei Modellen erzeugten auch nach Wiederholung kein gültiges Artefakt (Lückenmeldung
   dort n/a, nicht null). Der Ledger adressierte im Stressfall 36 von 52 Lücken als Prüf-Zettel.
4. **Adressierbarkeit = Prüfpotenzial, nicht erreichte Coverage.** Die numerische Gleichheit
   Ledger-Adressierbarkeit 0.852 = F-Touched 0.852 ist Koinzidenz verschiedener Konstrukte.
5. **Die stille Fehlerklasse ist präzise benannt:** ALLE 20 stillen Ledger-Lücken (4 meeting-2 [3 none + 1 partial] + 16 Interview) sind Detailverluste INNERHALB verwendeter Units — dort greift die Unused-Unit-Bilanz
   konstruktionsbedingt nicht.
6. **Modellsensitivität: Fs Sollbruchstelle ist Tool-Following, nicht Preis.** Zwei schwächere
   Modelle lieferten im freien Arm 0/4 Artefakte (schrieben die Arbeit als Text); das
   tool-trainierte gpt-5-mini erreichte Touched 0.852 — gerettet durch 3 Selbstrevisionen des
   deterministischen Bilanz-Guards (Struktur im freien Arm!). Die Kette schleuste JEDES Modell
   durch. Alle getesteten Ledger-Konfigurationen erzeugten ein schema-gültiges Artefakt mit
   vollständiger formaler Unit-Bilanz. Die semantisch adjudizierte Adressierbarkeit lag zwischen
   0.815 und 0.917; bei schwächeren Modellen entstand sie allerdings teilweise dadurch, dass sehr
   viele Units pauschal als Prüfbedarf weitergereicht wurden. Formale Bilanzierbarkeit erwies
   sich damit als stabil, semantische Extraktions- und Signalqualität nicht.
7. **Stufen-Ehrlichkeit:** Belegt sind die KONTROLL-Mechanismen (Unit-Bilanz, Unused-Behandlung,
   Miss-Signale, Provenienz-Kette, validierter Consumable) — NICHT ein Vorteil einzelner
   LLM-Stufen (Extraktion ohne Vorteil; Checker/Repair veränderte keinen Claim [L≡LCR];
   Kanonisierung verursachte Teil der Detailverluste; Segmentierung herauskontrolliert).

## 4b · L vs. LCR — der explizite Nullbefund (für Blaupause 7.7)

In allen sechs Ledger-Läufen waren die Claim-Bestände vor und nach Checker/Repair
inhaltsgleich (programmatisch verifiziert; paarweise sauber, da derselbe Maker-Output
weiterverarbeitet wurde). Der Kontrollschritt änderte somit keine Claims, erzeugte jedoch
Miss-Signale für die nachgelagerte Prüfung — der geplante dritte Arm ist nicht entfallen,
sondern ergab empirisch **L_Claims ≡ LCR_Claims** — bezogen auf die Claim-Bestände; die
Konfigurationen unterscheiden sich in den zusätzlich erzeugten Kontroll-/Prüfinformationen
(Checker/Repair wirkte als Detektor, nicht als Korrektor; der Reparaturpfad blieb in den
Hauptläufen ungenutzt und löste erst bei schwachen Modellen aus).

## 5 · Kernaussage fürs Kapitel (Kollegen-Endfassung 06.09.)

Ein starkes freies Modell konnte die reine Extraktionsaufgabe im ausgewählten Stressfall besser
und mit geringerem Tokenaufwand lösen. Dem höheren Tokenaufwand des Ledgers standen gegenüber:
eine deterministisch überprüfbare Verarbeitungsbilanz, eine vollständige formale Provenienz innerhalb
der ausgewerteten Ledger-Artefakte (Scope-Hinweis: Diese Aussage betrifft NUR den W2-Bestand
der Ledger-Läufe [Reference Validity 1.0]; die kern-weite Kettenauflösung über ALLE
Eingangspfade misst der separate Traceability-Audit mit 137/38/62 — zwei verschiedene
Gegenstände, kein Widerspruch, s. E2E-EVIDENZ §3) und
dadurch, dass er 36 von 52 unvollständig abgedeckten Gold-Propositionen als Prüfbedarf
adressierte. Die formale Bilanzierbarkeit blieb auch bei den weiteren getesteten Modellen
erhalten; die semantische Qualität der direkten Extraktion und der Prüfhinweise blieb jedoch
modellabhängig. Der nachgewiesene Nutzen liegt somit in kontrollierbarer Verarbeitung, nicht in
grundsätzlich besseren Einzeltexten.

## 6 · Verfahren & Gütesicherung (fürs Methodik-Unterkapitel)

- Urteils-Verfahren v3.1 (§28 P.4): KI-Adjudikation (jedes Urteil mit Beleg dokumentiert) →
  separater, REGELGEBUNDENER KI-Verifikationsdurchgang (Kollegen-KI; sah jeweils das
  vorgeschlagene Urteil und bestätigte oder kippte es — KEINE unabhängige/verblindete
  Annotation; 11 Kippungen in beide Richtungen (automatisiert aus den Matchlog-Feldern der vier Eval-Dateien gezählt), >10 %-Eskalationsschwelle nie gerissen;
  Übereinstimmung 90,9–100 %) → menschliche STICHPROBEN-
  Verifikation durch den Autor (16 Seed-Blöcke + Stille-Misses-Prüfbasis; alle bestätigt).
- Etiketten: Interview-Kernvergleich „KI-adjudiziert + KI-verifiziert + Autor-Stichprobe" ·
  meeting-2 ohne Autor-Stichprobe · Modellmatrix unverifiziert.
- Anti-Gaming: Auswahlregeln vorab deklariert (Läufe chronologisch, Stichproben per
  SHA-256-Seed), keine Treatment-Änderung aus Messzahlen (§27), Korrekturen nur mit Matchlog.

## 7 · Limitationen (MÜSSEN so ins Kapitel)

1. **Goldstand:** „KI-Erstentwurf, autor-konsolidiert" (Hash ① v1.2). Die vorgesehene
   menschliche Zweitannotation entfällt per Autor-Entscheid (06.09.) → Risiko korrelierter
   blinder Flecken zwischen Gold-Erstellung und Adjudikation bleibt unkontrolliert und wird
   offen ausgewiesen; alle Werte heißen „vorläufige adjudizierte Hauptauswertung".
2. **N=1 je Fall/Arm/Modellzelle** (F-Ausfälle je 1× wiederholt): Aussagen gelten für die
   vorab ausgewählten, vollständig adjudizierten Läufe; keine Varianz-Aussagen. **Dokumentierte
   PROTOKOLLABWEICHUNG:** Das Konzept sah N=3 je Zelle vor (§28 P.3); semantisch voll
   adjudiziert wurde N=1 (Aufwands-Entscheid des Autors); die N=3-Generierungsläufe des Piloten
   existieren als Rohmaterial.
3. Der Vergleich beginnt auf der gemeinsam bereitgestellten AU-Ebene (Segmentierung separat
   charakterisiert, nicht Teil des Effekts); HITL-Wirkung liegt designgemäß hinter der
   Messgrenze; Fault-Injection-Suite nicht gefahren.
4. Adjudikation und Verifikation sind KI-gestützt (zwei aufeinanderfolgende, regelgebundene und
   nicht verblindete Durchgänge sowie eine menschliche Stichprobe); eine menschliche
   Volladjudikation fand nicht statt.

## 8 · Beleg-Verzeichnis

- Kernvergleich: `runs/w2-official-eval-{12d046,6540e2}.json` (F) ·
  `runs/w2-official-ledger-eval-{e30646,8189ea}.json` (Ledger) — alle Einzelurteile mit Beleg/Matchlog.
- Modellmatrix: `runs/w2-modellsensitivitaet-eval-{mini,f-gpt5mini,ledger-gpt5mini,ledger-gptoss}.json`.
- Verifikation: `Thesis-Docs/aktiv/w2-validierung/` (Blätter, Prüfkopien, Prüfbasis, ★-Autor-Stichprobe).
- Läufe: `runs/arm-f/*`, `runs/ledger/*` (inkl. der 4 F-Fehlläufe als Beleg) · Chronik:
  `runs/w2-official-manifest.txt` (inkl. Korrektur-Einträgen — nichts wurde still umgeschrieben).
