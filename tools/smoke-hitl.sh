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

# --- Stufe: ingestion ---
echo "== [1/5] ingest-requirements-hitl =="
o=$(r ingest-requirements-hitl start "$EMPTY_DELTA"); rid=$(ridof "$o")
grepq "$o" "PAUSIERT" && ok "ingestion start -> Pause ($rid)" || bad "ingestion start"
CLEANUP+=("runs/ingestion/$rid")
o=$(r ingest-requirements-hitl resume "$rid" --accept-all)
grepq "$o" "APPLIED" && ok "ingestion resume -> Apply" || bad "ingestion resume"

# --- Stufe: pbi-update ---
echo "== [2/5] pbi-update-hitl =="
o=$(r pbi-update-hitl start _smoke-pbisrc); rid=$(ridof "$o")
grepq "$o" "PAUSIERT" && ok "pbi-update start -> Pause ($rid)" || bad "pbi-update start"
CLEANUP+=("runs/pbi-update/$rid")
o=$(r pbi-update-hitl resume "$rid" --accept-all)
grepq "$o" "APPLIED" && ok "pbi-update resume -> Apply" || bad "pbi-update resume"

# --- Stufe: decision ---
echo "== [3/5] decision-resolve-hitl =="
o=$(r decision-resolve-hitl start --input "$DEC_IN"); rid=$(ridof "$o")
grepq "$o" "PAUSIERT" && ok "decision start -> Pause ($rid)" || bad "decision start"
CLEANUP+=("runs/decision/$rid")
o=$(r decision-resolve-hitl resume "$rid" --accept-all)
grepq "$o" "APPLIED" && ok "decision resume -> Apply" || bad "decision resume"

# --- Stufe: github-forward (dry-run, kein Write) ---
echo "== [4/5] github-forward-hitl =="
o=$(r github-forward-hitl start _smoke-fwdsrc); rid=$(ridof "$o")
grepq "$o" "PAUSIERT" && ok "github-forward start -> Pause ($rid)" || bad "github-forward start"
CLEANUP+=("runs/github-forward/$rid")
o=$(r github-forward-hitl resume "$rid" --accept-all)
grepq "$o" "DRY-RUN" && ok "github-forward resume -> dry-run Apply" || bad "github-forward resume"

# --- Super-Workflow: pipeline (2 Gates) ---
echo "== [5/5] pipeline-hitl (2 Human-Gates) =="
o=$(r pipeline-hitl start "$EMPTY_DELTA"); rid=$(ridof "$o")
grepq "$o" "PAUSIERT an Gate 1" && ok "pipeline start -> Gate 1 ($rid)" || bad "pipeline start"
CLEANUP+=("runs/pipeline/$rid")
o=$(r pipeline-hitl resume "$rid" --accept-all)
grepq "$o" "naechsten Gate" && ok "pipeline resume 1 -> Gate 2" || bad "pipeline resume 1"
o=$(r pipeline-hitl resume "$rid" --accept-all)
grepq "$o" "PIPELINE FERTIG" && ok "pipeline resume 2 -> fertig" || bad "pipeline resume 2"

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
