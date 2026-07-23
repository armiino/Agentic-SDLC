## STATUS (2026-07-19): B1 GEBAUT

Der empfohlene `core-bootstrap-first-transcript`-Command ist gebaut (Stufe 1: Modus-Erkennung +
project-state-build → core-seed → core-baseline). Details/Belege: `plan-core-bootstrap-first-transcript.md` +
`iteration-notes-ingestion-core.md` (Abschnitt „Cold-Start / Bootstrap … B1 GEBAUT"). Offen/bewusst nicht drin:
Transkript→L1-L3-Artefakte (Ledger-Capstone), agentische Backlog-Gates (behalten Human-in-the-Loop), Re-Bootstrap.
Der Rest dieses Dokuments ist die ursprüngliche Analyse (weiterhin gültig).

## Ausgangslage

Der aktuelle Core-/Ingestion-Stand ist auf einen lebenden Projektzustand ausgelegt: Ein neues MeetingDelta wird
gegen einen bestehenden Core aufgeloest, danach entstehen affected-view, PBI-Update und github-sync-Delta.

Das funktioniert ab dem zweiten Transkript sehr gut, setzt aber voraus, dass bereits ein Core existiert.

Heute gilt:

```text
ingest-requirements <meeting-delta>
→ erwartet bestehenden Core
→ bricht ab, wenn state/core/project-state.json fehlt
```

Der erste Projektstand wird aktuell ueber einen Bootstrap/Seed aufgebaut:

```text
erstes Transkript
→ Ledger / L1-L3 / project-state-build
→ core-seed <project-state.json>
→ core-baseline / re-clarify
→ core-seed-backlog <re-clarify-run>
→ Core enthaelt Requirements + Features + PBIs
```

Das ist fachlich korrekt, aber noch kein eigener, runder Start-bei-null-Pfad.

## TODO

Spaeter sollte ein expliziter Bootstrap-Modus gebaut oder dokumentiert werden, z.B.:

```text
core-bootstrap-first-transcript
```

oder:

```text
ingest-requirements --bootstrap
```

Dieser Modus soll unterscheiden:

```text
kein Core vorhanden
→ initiale Projektwahrheit anlegen
→ alle validen initialen Requirements als NEW in den Core heben
→ Core-IDs praegen
→ danach re-clarify / core-seed-backlog fuer Features/PBIs
→ ab dann normaler Delta-Modus
```

Wichtig: Der Requirement-Resolver kann im ersten Lauf nicht gegen historische Wahrheit entscheiden, weil es noch
keine gibt. Seine Rolle waere dort kleiner: Er kann innerhalb des ersten Transkripts Dubletten oder Widersprueche
erkennen, aber keine RESTATE/REFINE/SUPERSEDE-Entscheidung gegen alte Core-Items treffen.

## Zielbild

Langfristig sollte der Ablauf so klar getrennt sein:

```text
Erstes Transkript:
  Bootstrap → initialer Core → initialer Backlog-Core

Alle weiteren Transkripte:
  Delta-Ingestion → Core-Update → affected-view → PBI-Update → github-sync-Delta
```

Damit bleibt der aktuelle Core weiterhin die Wahrheit, aber der Start bei null wird sauber und reproduzierbar.

## Einschaetzung Umsetzbarkeit

Der Bootstrap-Pfad ist voraussichtlich **leicht bis mittel** einzubauen. Die wesentlichen Bausteine existieren
bereits:

```text
project-state-build
core-seed
core-baseline
re-clarify
core-seed-backlog
```

Es fehlt vor allem eine saubere Orchestrierung, die den Modus erkennt:

```text
Core fehlt
→ Bootstrap fahren

Core existiert
→ normaler Delta-/Ingestion-Modus
```

Empfehlung: zuerst einen separaten Command bauen, z.B.:

```text
core-bootstrap-first-transcript <input-artifacts|run>
```

Dieser Command verbindet die bestehenden Schritte und gibt klare Validierungs-/Fehlermeldungen aus. Das ist
sauberer, als den Requirement-Resolver direkt mit einem `--bootstrap`-Sonderfall zu ueberladen.

Der Resolver ist im ersten Lauf fachlich nicht zwingend noetig, weil noch kein historischer Core existiert. Er kann
spaeter optional genutzt werden, um Dubletten oder Widersprueche innerhalb des ersten Transkripts zu erkennen. Fuer
den ersten Ausbau reicht aber ein deterministischer Bootstrap, der die initiale Projektwahrheit reproduzierbar
anlegt. Danach laeuft der normale Delta-Modus.
