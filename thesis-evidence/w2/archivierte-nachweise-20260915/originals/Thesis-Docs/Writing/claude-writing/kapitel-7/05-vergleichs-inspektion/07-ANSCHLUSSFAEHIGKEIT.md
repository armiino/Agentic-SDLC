# Anschluss- und Übergabefähigkeit: Schnittstellen-Analyse (v2)

> Status: MOMENTAUFNAHME 07.09., v2 nach Kollegen-Review (Beweissprache zurückgenommen).
> WAS DIESE ANALYSE IST: eine deterministische Untersuchung der ABHÄNGIGKEITEN INNERHALB DER
> GEWÄHLTEN ARCHITEKTUR — welche Felder die gebauten Abnehmer-Stufen lesen und welche Artefakte
> sie liefern. WAS SIE NICHT IST: ein Beweis, dass genau diese Verarbeitungsschritte notwendig
> sind (die Abnehmer wurden für diese Repräsentation gebaut; ob dieselben Felder mit weniger
> Schritten oder einem entsprechend beauftragten Agenten plus Validierung entstehen könnten,
> wurde nicht untersucht und ist nicht Gegenstand der Fragestellung).
> v1-Fehler korrigiert: kein „x von 8"-Leistungsscore mehr (zählte Verschiedenartiges und
> teils nicht Beauftragtes zusammen); keine „Adapter landet zwangsläufig bei meiner
> Architektur"-Behauptung.

## A · Qualitative Vertrags-Matrix (Code-Belege im gespiegelten Bestand `02-prompts-und-code/`)

| Vertragsfeld / Bedarf | Abnehmer (Beleg) | Ledger-Consumable | F-Output | Einordnung |
| --- | --- | --- | --- | --- |
| Claims + statement + sourceUnitIds + Typ | alle Abnehmer | ✓ | ✓ | gemeinsame Schnittmenge — hier fand der Qualitätsvergleich statt |
| Dispositionen (Routing je Zielartefakt) | `02-baselines/EvidenceAgentRunner.cs:32/80` (Artefakt→Dispositions-Key); Fragen-Rampe; Tor-1-Profile | ✓ | — | NICHT beauftragt bei F → Vertragsdifferenz, keine Fähigkeitsaussage |
| Facetten status/modality/scope | Baseline-/Fragen-Konsumenten | ✓ | — | NICHT beauftragt → Vertragsdifferenz |
| Miss-Signale für die Adjudikations-Queue | `adjudication/HumanAdjudicationExecutor.cs:25/37` | ✓ | — (Analogon `unresolved` beauftragt, blieb leer) | teils beauftragt: die SIGNAL-Funktion war Fs Bilanz-Auftrag — ihr Leerbleiben ist ein BEFUND (s. C) |
| Wörtliche Evidenz-Zitate | Adjudikations-UI; Zitat-Traceability | ✓ | — | nicht beauftragt → Vertragsdifferenz |
| Kanonische Claim-Identitäten | `05-core/CoreBacklogSeeder.cs:73`, `CoreToBaseline.cs:66`, Relationstyp `evidenced_by_ledger_claim` | ✓ | lauf-lokale ids | Konventions-Differenz |
| Kandidaten-/Stufen-Trace | Traceability-Audit | ✓ | — (keine Zwischenstände) | Konstruktions-Differenz: bei einstufiger Erzeugung existiert keine Stufen-Historie |
| Quellen-Rechenschaft | Messachse B | strukturell (Code zählt) | formal ✓ / inhaltlich leer | BEAUFTRAGT bei F → echter Befund (s. C) |

**Zulässige Aussage:** Das Ledger-Consumable erfüllt den Vertrag der gebauten Abnehmer
(nachgewiesen dadurch, dass die Kette in den E2E-Szenarien real damit lief); der F-Output
erfüllt die gemeinsame Claim-Schnittmenge, nicht aber die Routing-, Beleg- und
Identitätsfelder — überwiegend, weil sie nicht Teil seines Auftrags waren. Eine Nutzung des
F-Outputs in der bestehenden Kette würde eine zusätzliche Anreicherung erfordern
(semantisch: Facetten/Dispositionen; deterministisch: Identitäten, Signal-Buchführung);
ob dies einfacher wäre als die bestehenden Stufen, wurde nicht untersucht.

## B · Was deterministisch nachrüstbar wäre — als offene Design-Option, nicht als Urteil

| Feld | Nachrüstbar? | Anmerkung |
| --- | --- | --- |
| Dispositionen + Facetten | nur semantisch (LLM-Stufe) | Regeln existieren (FacetAssigner/-Validator) |
| Signal-Buchführung | deterministisch möglich | strukturelle Prüfung einer Agent-Bilanz wäre baubar |
| Evidenz-Zitate | deterministisch (AU-Text) | schwächer als stellengenaue Zitate |
| Identitäten | deterministisch | Konventionssache |
| Stufen-Trace | nein | ohne Zwischenstände nicht rekonstruierbar |

## C · Die zwei ECHTEN empirischen Befunde an dieser Schnittstelle (Zahlen zusammen!)

1. **Beauftragte, formal gültige, inhaltlich leere Selbstbilanz:** F sollte `unresolved`
   ausweisen; in allen erfolgreichen Läufen blieb das Feld leer, obwohl im vollständig
   adjudizierten Stressfall 37 Gold-Propositionen unvollständig abgedeckt waren
   (Silent-Miss 1,0) — bei zugleich HÖHERER direkter Inhaltsabdeckung als der Workflow.
2. **Strukturelle Signalisierung mit gemessenem Preis:** Der Workflow adressierte 36 seiner
   52 Lücken; 16 blieben unsichtbar (innerunitäre Detailverluste), und nur 18 von 48
   Hinweisen waren lückenrelevant. Welcher einzelne Workflow-Schritt den Unterschied
   verursacht, ist nicht isoliert (→ Stufenbeitrags-Analyse, Baustein B).

## D · Rechtfertigungs-Typen je Stufengruppe (Sprachregelung für Kapitel 7/8)

- **Unit-Bilanz/Unused-Kette:** empirisch GESTÜTZT im untersuchten Lauf (Befunde C1+C2) —
  kein allgemeiner Kontrollbeweis.
- **Facetten/Dispositionen/Identitäten:** Abhängigkeit innerhalb der gewählten Architektur;
  ihre Funktion ist per E2E demonstriert, ihre Notwendigkeit gegenüber Alternativen offen.
- **Kanonisierung:** architektonisch begründet (Verdichtung/Identität für Abnehmer);
  auf der atomaren Qualitätsmetrik kostete sie messbar.
- **Checker/Repair:** Design-Rationale; auf Naturdaten Nullbefund (Detektorrolle), Verhalten
  in konstruierten Fällen deterministisch getestet — Reparatur-Wirksamkeit nur exemplarisch.
