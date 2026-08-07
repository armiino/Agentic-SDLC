# 06-backlog — re-clarify: vom Core-Bestand zum Backlog (Cluster → PBIs)

> Status: LEBEND (README-bei-Code, B3 07.08.2026).

## Rolle in der Kette

Die Backlog-Stufe formt aus Kern-Anforderungen die ARBEITS-Struktur: **Cluster** (fachliche Gruppen →
Features) und **PBIs** (mit Titel/Statement/AK + `RequirementIds`-Coverage). Sie ist die NACHFOLGERIN der
alten L4-Kette (consolidation/completion/issuplanning — ersetzt 17.07., dormant archiviert; Klärung:
`docs/aktiv/backlog-genealogie.md`). Live-State-Besonderheit: `runs/l4-re-clarify/` — der NEUESTE Run wird
als Input gelesen (nie „aufräumen").

## Form (R-33: kanonische GateLoop-Checker-Repair-Form)

Beide Stufen (`reclarify/` Cluster + Backlog) laufen als Maker → **deterministisches Gate** (Coverage:
UNCOVERED_CORE u. a.) → bei repairable **Repair-Executor mit wörtlichem GateFeedback** → Loop (bounded,
`--max-attempts`) → Finalize → HumanReview (cluster-review-gate · backlog-review-gate im Ein-Graph).
`GateAttempt`-Historie je Versuch = W2-Messdraht. Der Cluster-Kritiker läuft nur bei Pass.
Geteilte `AddTo`-Kanten-Quelle: CLI-Bahn UND pipeline-full bauen DENSELBEN Graphen.

## UI-Besonderheiten (B1/B2)

Der Backlog-Review kann Fehlzuordnungen eines NEW_PBI korrigieren: auf ein bestehendes Feature (B1) ODER
ein im Plan vorgeschlagenes neues (B2, via Op-Konvertierung in die NEW_FEATURE-Gruppe) — Feature-Landkarte
rechts (referenceTarget-Konvention der Review-Schicht).

## Nähte

`ReClarifyTraceabilityEnricher` (Traceability deterministisch aus RequirementIds — geteilter Apply-Kern
CLI+Graph) · Seed in den Core über den CoreBacklogSeeder-Pfad (Feature-Anlage O4) · Coverage-Begriff =
`covers`-Relation (Kangal-bewacht).

## Beweise

Repair real gefeuert: Run `20260804_074419_acab74` (70× UNCOVERED_CORE → REPAIR → Pass, mini-Modell-
Experiment) · Cluster/Clarify gpt-5.4 je Pass Attempt 1 (Läufe 073649/073840).
