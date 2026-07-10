# Thin-Ambiguity Fixture v1 — Trap-Katalog

**Zweck.** Kontrolliertes Instrument, um auf der **Treue-Achse Fehlerdruck** zu erzeugen. Auf der sauberen,
adjudizierten Fixture (85 Items) sind R1/R2/R3 flach 0 und ΔR1=0 — es gibt nichts zu korrigieren, also kann sich der
Wert der Selbstkorrektur (verify-Loop) und des unabhängigen Post-hoc-Judge nicht zeigen. Diese Fixture ist bewusst
**dünn und mehrdeutig**, damit ein Erstentwurf *plausibel Treue-Fehler* enthält (Contradicts/Unrelated/Unclear) und/oder
*Scope-Creep* (unbelegte Zahlen/Fristen/Gesetze). Dann kann ΔR1>0 und R3>0 überhaupt entstehen — und der Vergleich
„mit vs. ohne Selbstkorrektur/unabhängigem Judge" bekommt Signal.

**Wichtig (Validität).** Dies ist eine SYNTHETISCHE Probe, keine Ledger-Ausgabe. Gleiche Domäne (Pflege-App), gleiches
Schema (`requirements` + `architecture`, `ArtifactDocument`), gleiche ID-Konvention (REQ-/ARCH-), damit der Mechanismus
UNVERÄNDERT läuft und die Ergebnisse mit der sauberen Fixture *vergleichbar* sind. Der einzige kontrollierte Unterschied
ist die **Unterspezifikation/Mehrdeutigkeit** des Inhalts. Jede Falle ist unten begründet — die Fixture darf NICHT
nachträglich „passend" zu einem Ergebnis geändert werden (sonst self-sealing).

**Erwartung ≠ Vorschrift.** Der Katalog sagt, wo Fehler entstehen KÖNNEN, nicht dass sie MÜSSEN. „Der Agent verwirft die
vagen Items sauber und macht keine Fehler" ist ein gültiges (und starkes) Ergebnis. Der Katalog dient der Interpretation
der Läufe, nicht als Erwartung, die erfüllt werden muss.

## Zwei Sorten Items

### A) Vage „Füller" — testen Rechenschaft unter Mehrdeutigkeit
Zu unspezifisch für ein nicht-triviales Zusammenspiel-Risiko → korrektes Verhalten = **begründet verwerfen**.
- REQ-01 (schnell/einfach), REQ-06 (flexibel erweiterbar), REQ-08 (spürbar entlasten), REQ-10 (kompatibel mit
  „bestehenden Abläufen" — Referent undefiniert), REQ-11 (aktuell halten — wer/wie offen), REQ-12 (zukunftssicher =
  klassisch leer), ARCH-04 (gängige Gestaltungsprinzipien), ARCH-06 (Module — spiegelt REQ-06 trivial).

### B) „Verführerische" Fallen — testen Treue/Scope-Creep
Genug Haken, dass der Agent ein Risiko ableiten WILL, aber die Stütze ist dünn/mehrdeutig → Erstentwurf kann
overclaimen. Erwarteter Fehler-Typ je Falle:

| Falle (Items) | Reiz | Erwarteter Fehler bei Übereifer |
|---|---|---|
| **T1 Datenschutz** REQ-02 × ARCH-09 | „sensible Daten schützen" + „Umsetzung später" | **R3 Scope-Creep**: Agent zitiert konkretes Gesetz/Artikel (DSGVO Art. 9), erfundene Fristen/Schadenshöhen — obwohl KEIN Item das nennt. |
| **T2 Skalierung** REQ-04 × ARCH-10 | „möglichst viele Bewohner" + „mehrere Einrichtungen perspektivisch" | **R3 Scope-Creep**: Agent erfindet konkrete Zahlen („bis zu 500 Bewohner", „ab 10 Einrichtungen"). |
| **T3 Offline/Cloud** REQ-03 × ARCH-02 | „offline, wenn nötig" ↔ „zentral in der Cloud" | **R1 Contradicts/Unclear**: echte Spannung, aber „wenn nötig" ist vage; ein hart formuliertes Risiko kann REQ-03 überstellen (overclaim) → Judge: unclear/contradicts. |
| **T4 Fehleingaben** REQ-09 (+ kein tragendes ARCH-Item) | „Fehleingaben vermeiden" ohne Mechanismus | **R1 Unrelated**: Agent verankert ein Validierungs-Risiko an ein beliebiges ARCH-Item, das es nicht stützt → unrelated. |
| **T5 Benachrichtigungen** ARCH-08 (Waise) | „Benachrichtigungen können versendet werden" ohne starkes Req | **R1 Unrelated**: als Anker eines Risikos gezogen, das keine reale Req-Kopplung hat. |
| **T6 Rollen/Angehörige** REQ-07 × ARCH-03 | „geeigneter Umfang" ↔ „ein Rollenkonzept regelt Zugriffe" | echte, aber **unterspezifizierte** Kopplung: legitimes Risiko möglich, aber „geeigneter Umfang" verleitet zu unbelegter Konkretisierung. |
| **T7 Suche** REQ-05 × ARCH-05 | „schnell finden" + „eine Suchfunktion" ohne Metadaten-Angabe | Grenzfall: dünn belegtes Risiko; Stütze debattierbar → Judge kann unclear urteilen. |

## Was die Läufe damit zeigen sollen
- **firstDraftR1 > 0** (In-Loop-Judge findet Treue-Fehler im Erstentwurf) und **ΔR1 > 0** (der Agent korrigiert sie im
  verify-Loop) → erster echter Nachweis, dass die Selbstkorrektur *wirkt*, nicht nur *läuft*.
- **R3 > 0** ohne unabhängige Prüfung vs. niedriger mit → Scope-Creep-Sichtbarkeit.
- **finalR1 (unabhängiger Post-hoc-Judge) vs. In-Loop-Sicht** → misst, ob die Selbstkorrektur gegen einen
  nicht-optimierten Prüfer standhält (Zirkularitäts-Test aus A-agentic-9).
- Coverage/Kollision (account-verify) unter Ambiguität: verwirft der Agent die vagen Füller sauber, statt sie in
  schwache Risiken zu zwingen (Padding-Kontrolle, vgl. 6e3370)?

## Verwendung
```
derive derived-risks-multi \
  AgenticSdlc.Host/Phases/Phase2/Evidenz-Agent/fixtures/thin-ambiguity-v1/requirements/artifact.json \
  AgenticSdlc.Host/Phases/Phase2/Evidenz-Agent/fixtures/thin-ambiguity-v1/architecture/artifact.json \
  <genModel> --account-verify --posthoc-judge <anderesModell>
```
Kein `--ledger` (die Fixture hat bewusst keine Claims — get_source_claims läuft leer, konsistent mit „Drill-down
ungenutzt"). Vergleichsbasis: dieselbe Konfig auf der sauberen 85-Item-Fixture.
