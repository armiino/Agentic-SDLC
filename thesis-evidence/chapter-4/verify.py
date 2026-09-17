#!/usr/bin/env python3
"""Read-only integrity check. Does not execute archived code, runs or checkpoints."""
import argparse
import hashlib
import json
import re
from pathlib import Path


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def inside(root, rel):
    p = (root / rel).resolve()
    if not p.is_relative_to(root.resolve()):
        raise ValueError('Path outside expected root: ' + rel)
    return p


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--repo-root', type=Path)
    parser.add_argument('--originals', action='store_true', help='Additionally check originals and full run inventories; requires source repository.')
    args = parser.parse_args()
    package = Path(__file__).resolve().parent
    repo = (args.repo_root or package.parents[1]).resolve()
    m = json.loads((package / 'manifest.json').read_text())
    errors = []
    totals = dict(package_files=0, records=0, originals=0, inventory_files=0, local_links=0)

    def check(condition, message):
        if not condition:
            errors.append(message)

    expected = {}
    for line in (package / 'SHA256SUMS').read_text().splitlines():
        checksum, rel = line.split('  ', 1)
        check(rel not in expected, 'Duplicate checksum path: ' + rel)
        expected[rel] = checksum
        p = inside(package, rel)
        check(p.is_file() and digest(p) == checksum, 'Package hash: ' + rel)
        totals['package_files'] += 1
    actual = {p.relative_to(package).as_posix() for p in package.rglob('*') if p.is_file() and p.name != 'SHA256SUMS'}
    check(actual == set(expected), 'Package file inventory differs from SHA256SUMS')
    ids = [e['id'] for e in m['entries']]
    check(len(ids) == len(set(ids)) == 19, 'Expected 19 unique E-IDs')
    used = set()
    for e in m['entries']:
        check(bool(e['files']), 'Empty entry: ' + e['id'])
        for key in e['files']:
            used.add(key)
            check(key in m['files'] and e['id'] in m['files'][key]['eids'], 'Anchor membership: ' + key)
    check(used == set(m['files']), 'Unassigned manifest records')
    for key, f in m['files'].items():
        if f['mode'] == 'reference':
            p = inside(repo, f['original'])
        else:
            p = inside(package, f['package_path'])
        check(p.is_file() and p.stat().st_size == f['bytes'] and digest(p) == f['sha256'], 'Manifest file: ' + key)
        totals['records'] += 1
        if args.originals:
            original = inside(repo, f['original'])
            wanted = f.get('original_sha256', f['sha256'])
            check(original.is_file() and digest(original) == wanted, 'Original source: ' + f['original'])
            totals['originals'] += 1
            if f['mode'] == 'excerpt' and original.is_file():
                lines = original.read_text(encoding='utf-8-sig').splitlines(keepends=True)
                excerpt = ''.join(lines[f['line_start'] - 1:f['line_end']]).encode('utf-8')
                check(hashlib.sha256(excerpt).hexdigest() == f['sha256'], 'Excerpt range: ' + key)
    if args.originals:
        for root, inventory in m['run_inventories'].items():
            for f in inventory:
                p = inside(repo, root + '/' + f['path'])
                check(p.is_file() and digest(p) == f['sha256'], 'Run inventory: ' + str(p))
                totals['inventory_files'] += 1
    # Only generated reader guides; historical documents deliberately retain old paths.
    for guide in [package / 'README.md'] + [package / eid / 'README.md' for eid in ids]:
        for link in re.findall(r'\]\(([^)]+)\)', guide.read_text()):
            if link.startswith(('http:', 'https:', '#')):
                continue
            target = (guide.parent / link.split('#')[0]).resolve()
            if not target.is_relative_to(package):
                # Existing study directories are siblings in thesis-evidence/.
                rel = target.relative_to(package.parent)
                target = inside(repo / 'thesis-evidence', str(rel))
            check(target.exists(), 'Reader-guide link: ' + str(guide) + ' -> ' + link)
            totals['local_links'] += 1
    checks = json.loads((package / 'pruefung.json').read_text())['checks']
    check(all(c['passed'] for c in checks), 'A recorded content check did not pass')
    result = dict(passed=not errors, totals=totals, recorded_content_checks=len(checks), errors=errors,
                  scope='Hashes, original/excerpt identity when requested, inventory and reader-guide links; no new system execution or semantic evaluation.')
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0 if not errors else 1


if __name__ == '__main__':
    raise SystemExit(main())
