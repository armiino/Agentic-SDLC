#!/usr/bin/env python3
"""Read-only field comparison: archived Core -> sync entry -> plan -> issue readback.

This checks three coupled cases, not semantic quality or a new system execution.
"""
from pathlib import Path
from hashlib import sha256
import argparse
import json
import re

E = Path(__file__).resolve().parent
ap = argparse.ArgumentParser(description=__doc__)
ap.add_argument('--package-root', type=Path, default=E.parent)
K = ap.parse_args().package_root.resolve()
F = Path('originals/runs/fullworkflow/20260821_204808_042823')
G = Path('originals/runs/fullworkflow/20260821_211304_bfbbd7')
paths = {
    'core': F/'07-github/applied/core-before.json',
    'sync': F/'07-pbi-update/applied/github-sync-delta.json',
    'plan': F/'07-github/github-forward-plan.json',
    'readback': G/'07-github/github-snapshot.json',
}
data = {name: json.loads((K/path).read_text()) for name, path in paths.items()}
items = {item['itemId']: item for item in data['core']['items']}
entries = {item['pbiId']: item for item in data['sync']['entries']}
readback = {item['issueNumber']: item for item in data['readback']}

def parts(body):
    chunks = re.split(r'^### (.+)\n\n', body, flags=re.M)
    result = {'statement': chunks[0].strip()}
    for header, content in zip(chunks[1::2], chunks[2::2]):
        result[header] = content.split('\n---\n', 1)[0].strip()
    return result

def bullets(section):
    lines = section.splitlines()
    assert all(line.startswith('> - ') or line in ['>', '> &nbsp;'] for line in lines)
    return [line[4:] for line in lines if line.startswith('> - ')]

cases = []
for case_id in ['PBI-003', 'PBI-032', 'PBI-046']:
    core_item = items[case_id]
    pbi = core_item['pbi']
    entry = entries[case_id]
    ops = [op for op in data['plan']['operations'] if op.get('pbiId') == case_id and op['kind'] == 'UPDATE_ISSUE']
    assert len(ops) == 1
    op = ops[0]
    issue = readback[op['targetIssueNumber']]
    body = parts(op['body'])
    rels = [rel for rel in data['core']['relations'] if rel['fromId'] == case_id]
    covered = [rel['toId'] for rel in rels if rel['relationType'] == 'covers']
    req_ids = [id for id in covered if items[id]['itemType'] != 'architecture']
    arch = sorted(f"{id} — {items[id]['text']}" for id in covered if items[id]['itemType'] == 'architecture')
    constraints = sorted(f"{rel['toId']} — {items[rel['toId']]['text']}" for rel in rels if rel['relationType'] == 'constrained_by')
    checks = {
        'core_title_to_sync': pbi['title'] == entry['title'],
        'core_goal_to_sync': pbi['goal'] == entry['statement'],
        'core_criteria_to_sync': pbi['acceptanceCriteria'] == entry['acceptanceCriteria'],
        'core_covers_to_sync': req_ids == entry['coveredRequirementIds'],
        'core_constraints_to_sync': constraints == (entry.get('constraints') or []),
        'core_architecture_work_to_sync': arch == (entry.get('coveredArchitecture') or []),
        'sync_title_to_plan': entry['title'] == op['title'],
        'sync_statement_to_plan': entry['statement'] == body['statement'],
        'sync_criteria_to_plan': entry['acceptanceCriteria'] == bullets(body['Akzeptanzkriterien']),
        'sync_constraints_to_plan': (entry.get('constraints') or []) == bullets(body['Technische Rahmenbedingungen']),
        'sync_architecture_work_to_plan': (entry.get('coveredArchitecture') or []) == bullets(body.get('Umgesetzte Architektur-Arbeit', '')),
        'sync_requirement_ids_to_plan': entry['coveredRequirementIds'] == re.findall(r'`([^`]+)`', body['Abgedeckte Requirements']),
        'plan_title_to_readback': op['title'] == issue['title'],
        'plan_complete_body_to_readback': op['body'] == issue['body'],
    }
    cases.append({
        'case_id': case_id,
        'issue_number': op['targetIssueNumber'],
        'plan_origin': op['origin'],
        'checks': checks,
        'readiness_values_not_semantically_evaluated': {
            'pbi_field': pbi.get('readiness'),
            'core_blocker_axis': core_item.get('blocker'),
            'sync_entry': entry.get('readiness'),
        },
    })

result = {
    'date': '2026-09-16',
    'scope': 'Selected stored content fields and relation-derived context of three dependent Core-PBI/Issue pairs; complete plan/readback body correspondence. No semantic judgment, new run, or general projection reliability claim. Labels, comments, document exports and meaning of readiness are not evaluated.',
    'input_files': {name: {'file': '../'+str(path), 'sha256': sha256((K/path).read_bytes()).hexdigest()} for name, path in paths.items()},
    'cases': cases,
    'passed': all(all(case['checks'].values()) for case in cases),
}
expected = E/'projection-chain.json'
if expected.exists():
    assert json.loads(expected.read_text()) == result, 'Recorded result differs from recomputation'
print(json.dumps(result, ensure_ascii=False, indent=2))
raise SystemExit(0 if result['passed'] else 1)
