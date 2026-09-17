# W2-Vergleichs-Inspektion — Lesefaden

> Zweck: EIN Ort mit allem, was der Vergleich „freier Agent (F) vs. Ledger (LCR)" umfasst —
> zum Selbst-Prüfen der Fairness, bevor über Behalten/Umbauen/Entfernen entschieden wird.
> Alles hier sind KOPIEN (Stand 07.09.); Originale in runs/, input/eval-labels/, AgenticSdlc.Host/.

## Inhalt

| Ordner | Inhalt |
| --- | --- |
| `01-VERGLEICHSAUFBAU.md` | DAS Hauptdokument: was jede Ledger-Stufe tut, was F tut, Aufgaben-Gegenüberstellung, Messaufbau, Asymmetrie-Liste, Leitfragen |
| `02-prompts-und-code/` | F: Missions-Prompt (ArmFAgent2.txt) + Tools/Guard-Code · Ledger: KOMPLETTER 1:1-Spiegel von 01-ledger (53 .cs) — Einstieg über `ledger/00-STRUKTUR-KARTE.md` (welche Datei = gemessener Pfad / Messwerkzeug / HITL / Altlast) + `ledger/PROMPTS-KLARTEXT.md` (alle Stufen-Prompts lesbar) |
| `03-gold-und-regeln/` | beide Gold-Bestände, beide nummerierten Quellen (= gemeinsamer Input!), Matchregeln, Gold-Leitfaden, Hashes |
| `04-output-arm-f/` | Fs Einreichungen beider Fälle (output.json: Claims + Quellenbilanz) + Token-Metriken |
| `05-output-ledger/` | Ledger-Captures beider Fälle: L (vor QC) und LCR (nach QC, mit Miss-Signalen) |
| `06-messung/` | die vier Eval-Dateien (jedes Einzelurteil mit Beleg + Kippungs-Matchlog), W2-ERGEBNISSE.md, Manifest-Chronik |
| `07-ANSCHLUSSFAEHIGKEIT.md` | NEU 07.09.: deterministische Vertrags-Matrix — kann die Kette mit Fs Output arbeiten? (F erfüllt 1½/8 Abnehmer-Vertragsfelder; Adapter = Stufen-Nachbau; Rechtfertigungs-Typen je Stufengruppe) |

## Empfohlener Prüfweg (~1–2 h)

1. `01-VERGLEICHSAUFBAU.md` lesen (Abschnitte B, C, D) — sind die Aufträge äquivalent?
2. Beide Prompts im Original lesen: `arm-f/ArmFAgent2.txt` vs. Extraktor-/Kanonisierer-Prompts
   in den .cs-Dateien.
3. Stichprobe „Gold gegen beide Outputs": 5 Propositionen aus
   `03-gold-und-regeln/Interview-Einrichtung.w2-gold.json` nehmen, in
   `04-output-arm-f/stressfall-…` und `05-output-ledger/stressfall-…-LCR.json` suchen —
   deckt sich dein Eindruck mit den Urteilen in `06-messung/w2-official-*6540e2*.json`?
4. Fs Quellenbilanz anschauen (output.json, Teil quellenbilanz) vs. Ledger-Miss-Signale
   (LCR.json, missSignal) — dasselbe Konstrukt? (Abschnitt D, letzte Zeile)
5. Abschnitt F (Asymmetrien + Leitfragen) — Urteil bilden.

## Wichtig beim Urteilen

- Die Asymmetrie-Liste in Abschnitt F ist bewusst in BEIDE Richtungen geschrieben.
- „L ≡ LCR": die Ledger-Claims vor/nach QC sind inhaltsgleich — der Unterschied ist der
  Signal-Anbau. Für den Claim-Vergleich mit F ist daher LCR = L.
- Die Zahlen selbst (0,657 vs. 0,519 usw.) stehen mit voller Herleitung in
  `06-messung/W2-ERGEBNISSE.md`; jede Kippungs-Historie im Manifest.
