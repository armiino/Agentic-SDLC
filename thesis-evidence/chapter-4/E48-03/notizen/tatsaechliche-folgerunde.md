**Befund (Lauf `20260817_102027_b9c264` — eine „alles abgelehnt"-Ernte-Runde):** nach dem reject des einzigen
Items pausierte der Lauf ZWEIMAL sinnlos: ① **pbi-gate mit `operations: 0`** (Derive 0, Maker 0, Gate Pass —
trotzdem HUMAN_GATE + Checkpoint-Pause; `open_gate_ui` kehrt „ok" zurück und zeigt NICHTS an [„keine
Operationen"] — für den Autor wirkt die UI kaputt) ② danach **forward-gate mit 0 Ops**: der pbi-Apply schreibt
auch bei 0 Änderungen ein LEERES `github-sync-delta.json`, die `OperationalForwardBridge` prüft nur
**Datei-Existenz** statt `entries > 0` → ForwardPrep(0) → Snapshot-Pull (unnötiger API-Call) → Seed 0 Ops →
wieder Human-Pause. Ausweg war je 2× `resume --accept-all` über leere Gates (harmlos, aber Experiment-Flag für
NICHTS). **Das ist der 9g-Parkplatz „Leer-Spur-Skip" — jetzt mit doppeltem Live-Beleg.**
