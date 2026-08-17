#!/usr/bin/env bash
# ============================================================================
# Smoke-Netz — Regressions-Test fuer den LEBENDEN HITL-Strang (S-8/S-9).
# Deterministisch, KEIN LLM, KEINE Core-Mutation (Leer-/Trivial-Inputs -> 0 Ops).
# Zweck: VOR und NACH jedem Refactoring-Schritt laufen lassen -> "nichts kaputt".
# PASS wenn: Build gruen, jede Stufe start->PAUSE->resume->Apply, Core-Item-Zahl unveraendert.
# Nutzung:  bash tools/smoke-hitl.sh        (Exit 0 = alles gruen)
# ============================================================================
set -uo pipefail
cd "$(dirname "$0")/.." || exit 2
HOST="AgenticSdlc.Host/AgenticSdlc.Host.csproj"
CORE="state/core/project-state.json"
FIX="runs/_smoke"
BACKUP="$FIX/core-backup.json"
pass=0; fail=0
declare -a CLEANUP

ok()   { echo "  PASS  $1"; pass=$((pass+1)); }
bad()  { echo "  FAIL  $1"; fail=$((fail+1)); }
items(){ python3 -c "import json;print(len(json.load(open('$CORE'))['items']))"; }
grepq(){ echo "$1" | grep -q "$2"; }
ridof(){ echo "$1" | grep -oE "runId=[0-9_a-f]+" | head -1 | cut -d= -f2; }
r()    { dotnet run --project "$HOST" --no-build -- "$@" 2>&1; }

mkdir -p "$FIX"
echo "== BUILD =="
if dotnet build "$HOST" --no-restore >/dev/null 2>&1; then ok "build gruen"; else bad "build"; echo "ABBRUCH"; exit 1; fi

[ -f "$CORE" ] || { echo "Core fehlt: $CORE"; exit 2; }
cp "$CORE" "$BACKUP"; I0=$(items); echo "== Core-Baseline: $I0 items =="

# --- Fixtures (leer -> 0 Ops, kein LLM) ---
EMPTY_DELTA="$FIX/empty-delta.json"
echo '{ "projectId":"_smoke", "schemaVersion":3, "createdUtc":"2026-01-01T00:00:00Z", "sources":[], "items":[], "relations":[], "provenance":[], "proposals":[] }' > "$EMPTY_DELTA"
# pbi-update braucht einen ingestion-run mit applied/delta.json
mkdir -p "runs/ingestion/_smoke-pbisrc/plan/applied"
echo '{ "schemaVersion":1, "planId":"_smoke", "operations":[] }' > "runs/ingestion/_smoke-pbisrc/plan/plan.json"
echo '{ "applied":[], "skipped":[], "delta":{} }' > "runs/ingestion/_smoke-pbisrc/plan/applied/delta.json"
CLEANUP+=("runs/ingestion/_smoke-pbisrc")
# github-forward braucht einen pbi-update-run mit github-sync-delta.json
mkdir -p "runs/pbi-update/_smoke-fwdsrc/plan/applied"
echo '{ "schemaVersion":1, "planId":"_smoke", "operations":[] }' > "runs/pbi-update/_smoke-fwdsrc/plan/pbi-change-plan.json"
echo '{ "newPbis":[], "updatedPbis":[], "entries":[] }' > "runs/pbi-update/_smoke-fwdsrc/plan/applied/github-sync-delta.json"
CLEANUP+=("runs/pbi-update/_smoke-fwdsrc")
# decision --input (leere resolutions)
DEC_IN="$FIX/dec-input.json"; echo '{ "resolutions":[] }' > "$DEC_IN"

# --- Stufe: ingestion (R-50: leeres Gate wird LAUT übersprungen — kein Pause/Resume mehr nötig) ---
echo "== [1/5] ingest-requirements-hitl =="
o=$(r ingest-requirements-hitl start "$EMPTY_DELTA"); rid=$(ridof "$o")
grepq "$o" "Human-Gate übersprungen" && ok "ingestion leeres Gate -> R-50-Skip (laut) ($rid)" || bad "ingestion R-50-Skip"
CLEANUP+=("runs/ingestion/$rid")
grepq "$o" "APPLIED" && ok "ingestion Skip -> Apply im selben Lauf" || bad "ingestion Skip-Apply"

# --- Stufe: pbi-update (R-50: leeres Gate wird LAUT übersprungen — kein Pause/Resume mehr nötig) ---
echo "== [2/5] pbi-update-hitl =="
o=$(r pbi-update-hitl start _smoke-pbisrc); rid=$(ridof "$o")
grepq "$o" "Human-Gate übersprungen" && ok "pbi-update leeres Gate -> R-50-Skip (laut) ($rid)" || bad "pbi-update R-50-Skip"
CLEANUP+=("runs/pbi-update/$rid")
grepq "$o" "APPLIED" && ok "pbi-update Skip -> Apply im selben Lauf" || bad "pbi-update Skip-Apply"

# --- Stufe: decision ---
echo "== [3/5] decision-resolve-hitl =="
o=$(r decision-resolve-hitl start --input "$DEC_IN"); rid=$(ridof "$o")
grepq "$o" "PAUSIERT" && ok "decision start -> Pause ($rid)" || bad "decision start"
CLEANUP+=("runs/decision/$rid")
o=$(r decision-resolve-hitl resume "$rid" --accept-all)
grepq "$o" "APPLIED" && ok "decision resume -> Apply" || bad "decision resume"

# --- Stufe: github-forward (R-50: leeres Gate wird LAUT übersprungen; dry-run, kein Write) ---
echo "== [4/5] github-forward-hitl =="
o=$(r github-forward-hitl start _smoke-fwdsrc); rid=$(ridof "$o")
grepq "$o" "Human-Gate übersprungen" && ok "github-forward leeres Gate -> R-50-Skip (laut) ($rid)" || bad "github-forward R-50-Skip"
CLEANUP+=("runs/github-forward/$rid")
grepq "$o" "DRY-RUN" && ok "github-forward Skip -> dry-run Apply im selben Lauf" || bad "github-forward Skip-Apply"

# --- Super-Workflow: pipeline-full --from-delta (Schritt 5 ③, 05.08.: ersetzt pipeline-hitl) ---
# Prueft den EIN-Graph Delta-Einstieg + den ECHTEN Checkpoint->Resume-Zyklus (echte Factories!).
# R-50: LEERE Gates rufen nie mehr (Skips laut, [1]-[4]) — der deterministische Pause-Anker des Ein-Graph-Tests
# ist deshalb eine FIXTURE-DEC (nach dem Core-Backup injiziert; das Restore am Ende räumt sie garantiert weg):
# der Lauf skippt ingest/arch leer, pausiert am decision-gate, resume vertagt sie, skippt pbi/forward -> FERTIG.
python3 - <<'PYEOF'
import json
p='state/core/project-state.json'
c=json.load(open(p))
c['items'].append({"itemId":"DEC-SMOKE","itemType":"decision","text":"Smoke-Fixture: deterministischer Pause-Anker (R-50).","origin":"MEETING_OPEN_QUESTION","version":1,"sourceRunId":"_smoke","metadata":{},"validity":"Active","blocker":"None"})
json.dump(c,open(p,'w'),ensure_ascii=False)
PYEOF
echo "== [5/5] pipeline-full --from-delta (Ein-Graph, Pause->Resume via Fixture-DEC) =="
o=$(r pipeline-full run --from-delta "$EMPTY_DELTA" --policy interactive); rid=$(ridof "$o")
grepq "$o" "Einstieg: DELTA" && ok "pipeline-full Delta-Einstieg (Front uebersprungen)" || bad "pipeline-full Delta-Einstieg"
grepq "$o" "PAUSIERT am Gate 'decision-gate'" && ok "pipeline-full run -> Pause decision-gate ($rid)" || bad "pipeline-full run/Pause"
CLEANUP+=("runs/fullworkflow/$rid")
o=$(r pipeline-full resume "$rid" --accept-all)
grepq "$o" "Forward übersprungen" && ok "pipeline-full resume -> FERTIG (R-50: leere Gates übersprungen, Forward-Skip)" || bad "pipeline-full resume"
# Fixture-DEC vor dem Zähl-Check entfernen (das Core-Restore am Ende räumt ohnehin — dies hält den Check ehrlich):
python3 - <<'PYEOF'
import json
p='state/core/project-state.json'
c=json.load(open(p))
c['items']=[i for i in c['items'] if i['itemId']!='DEC-SMOKE']
json.dump(c,open(p,'w'),ensure_ascii=False)
PYEOF

# --- Core-Integritaet + Cleanup ---
echo "== Core-Integritaet =="
IN=$(items); [ "$IN" = "$I0" ] && ok "Core-Item-Zahl unveraendert ($IN)" || bad "Core-Item-Zahl $I0 -> $IN"
cp "$BACKUP" "$CORE"; ok "Core aus Backup restauriert"
for d in "${CLEANUP[@]}"; do rm -rf "$d"; done
rm -rf "$FIX"

echo "============================================================"
echo "SMOKE-ERGEBNIS:  $pass PASS  |  $fail FAIL"
echo "============================================================"
[ "$fail" -eq 0 ]
