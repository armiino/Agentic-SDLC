# Neuberechnung der Hauptauswertung gegen Gold v2 (07.09.2026)

> Status: ERLEDIGT 07.09.2026 — Delta-Neubeurteilung nach der Gold-Adjudikation
> (`input/eval-labels/w2-gold-aenderungslog-v2.md`, Hash ②). Alt-Dateien unverändert
> (Stand Hash ① v1.2); v2-Ergebnisdateien: `runs/w2-goldv2-eval-*.json`.

## Verfahren

Neu gefällt wurden NUR die von der Adjudikation betroffenen Urteile (Evaluator-Adjudikation
Claude, Matchregeln v1.2 + Regeln §C des Änderungslogs): je Arm 8 geänderte Interview-Statements
+ neue G-IE-109 + 3 meeting-2-Statements; dazu ein dokumentierter Regel-Check der
Leitplanken-relevanten Alt-Urteile (alle bestätigt). Alle übrigen Urteile wurden unverändert
übernommen — inkl. ihres Verifikations-Status; **die Delta-Urteile selbst sind nur
KI-adjudiziert (kein separater Verifikationsdurchgang, keine Autor-Stichprobe)** — bei Bedarf
nachholen. Wirkungsrichtung war ergebnisoffen; Belege je Urteil in den v2-Dateien.

## Kernzahlen alt (Hash ①, Nenner 108/31) → neu (v2, Nenner 109/31)

| Metrik | F Interview | Ledger-LCR Interview | F meeting-2 | Ledger meeting-2 |
| --- | --- | --- | --- | --- |
| Coverage strict | 0.657 → **0.651** | 0.519 → **0.505** | 0.903 → **0.871** | 0.871 → **0.839** |
| Coverage lenient | 0.852 → **0.853** | 0.778 → **0.780** | 0.935 → **0.935** | 0.903 → **0.903** |
| Precision strict | 0.991 → 0.991 | 1.0 → 1.0 | 1.0 → 1.0 (Escapes 4→**3**) | 1.0 → 1.0 |
| Misses gesamt | 37 → **38** | 52 → **54** | 3 → **4** | 4 → **5** |
| Detection strict | 0.0 | 0.692 → **0.667** | 0.0 | 0.0 |
| Silent-Miss strict | 1.0 | 0.308 → **0.333** | 1.0 | 1.0 |
| Addressable | 0.657 → **0.651** | 0.852 → **0.835** | 0.903 → **0.871** | 0.871 → **0.839** |

## Die gekippten Urteile (vollständig)

| ID | F | Ledger | Grund |
| --- | --- | --- | --- |
| G-IE-085 | full→**partial** | full→**partial** | v2-Qualifikation „Profile unterhalb der Suchleiste" wird von keinem Arm ausgedrückt |
| G-IE-109 (neu) | **full** (C-091: Ablauf + Vorschlags-Charakter komplett) | **partial** (CAN-038: nur die offene Frage, nicht der Ablauf) | neue Gold-ID (M6) |
| G-M2-021 | bleibt **full** (Bündel C-019+**C-020**; C-020 verliert gold_escape-Status) | full→**partial** (Suchfall No-Go-Inhalte + Zuordnung fehlt) | v2-Suchfall (M2-K2) |
| G-M2-029 | full→**partial** (C-035 ohne Planungsvorbehalt; uncertainty-Flag zählt nicht für P1) | bleibt **full** („eingeplant … Abstimmung ausstehend") | v2-Planungsstatus (M2-K3) |

Alle übrigen Delta-IDs (G-IE-014/026/058/073/082/095/096, G-M2-016) behalten ihr Verdikt —
teils mit passgenauerer v2-Begründung (058: CAN-035 bzw. F-Bündel C-078–081; 096: CAN-043 bzw.
C-106+C-107 decken die Verortung explizit). Regel-Check: kein Alt-Urteil widersprach den neuen
Leitplanken (Details in den v2-Dateien).

## Befunde

1. **Die Neuberechnung war ergebnisoffen und hat BEIDE Arme gekostet** — strict sinkt überall
   leicht (mehr Qualifikationen im Gold), lenient bleibt praktisch konstant. Die Rangfolge
   der Arme ändert sich in keiner Zelle.
2. **Der intra-unit-Befund verstärkt sich:** alle NEUEN stillen Ledger-Lücken (G-IE-085,
   G-IE-109, G-M2-021) sind wieder Detailverluste INNERHALB verwendeter Units — Interview
   jetzt 18/18, meeting-2 5/5 (vorher 16/16 bzw. „alle").
3. **G-IE-109 trennt die Arme charakteristisch:** F trägt den unbeschlossenen Ablaufvorschlag
   als eigene Aussage (C-091, full); der Ledger fasst ihn in die offene Frage (partial).
   Spiegelbildlich G-M2-029: der Ledger trägt den Planungsstatus im Claim, F nur als
   uncertainty-Flag (zählt per Matchregeln nicht für P1).
4. **Altdaten-Fund:** die Hash-①-misses-Liste von F-meeting-2 enthielt fälschlich G-009
   (im Verifikationsdurchgang auf full gekippt, Liste nicht nachgezogen; missesGesamt=3 war
   korrekt) — in der v2-Datei bereinigt dokumentiert.

## Reichweite / offen

- **Modellsensitivitäts-Matrix (sekundär, unverifiziert) NICHT neu beurteilt** — ihre
  qualitativen Kernaussagen (F-Ausfälle 0/4 bei oss/4.1-mini, ValidateBilanz-Selbstrevisionen)
  hängen nicht am Gold; die gpt-5-mini-Coverage-Zahlen (0.574/0.852 u. a.) sind bis zu einer
  Neubeurteilung NUR mit Etikett „Stand Hash ① v1.2, Nenner 108" zitierbar.
- Kapitel-7-Texte/Blätter (W2-ERGEBNISSE, PILOT-BERICHT, chap7-Steinbruch) zitieren noch
  Hash-①-Zahlen — beim Schreiben (Umsetzungsplan Schritte 5–6) auf die v2-Dateien umstellen.
- Z1 (Stufenanalyse) arbeitet ab jetzt gegen Gold v2 / Nenner 109.
