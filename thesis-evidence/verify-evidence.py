#!/usr/bin/env python3
"""Verify the dated evidence inventory, read-only; Python standard library only.

Run: python3 thesis-evidence/verify-evidence.py
This checks file identity, not semantic accuracy, system behaviour or usage permission.
Extra files are reported separately; they are not part of the dated inventory.
"""
from pathlib import Path
import argparse,hashlib,json,sys

def verify(root,manifest_name):
    root=root.resolve()
    manifest=json.loads((root/manifest_name).read_text(encoding='utf-8'))
    bad=[];byte_count=0
    for rel,meta in manifest['files'].items():
        p=(root/rel).resolve()
        if not p.is_relative_to(root):
            bad.append({'file':rel,'error':'path_outside_package'});continue
        if not p.is_file():
            bad.append({'file':rel,'error':'missing'});continue
        raw=p.read_bytes();byte_count+=len(raw)
        if len(raw)!=meta['bytes'] or hashlib.sha256(raw).hexdigest()!=meta['sha256']:
            bad.append({'file':rel,'error':'content_changed'})
    actual={p.relative_to(root).as_posix() for p in root.rglob('*')
            if p.is_file() and p.name!='.DS_Store' and '__pycache__' not in p.parts}
    extras=sorted(actual-set(manifest['files'])-{manifest_name})
    return {'manifest':manifest_name,'inventory_date':manifest['date'],
            'files_checked':len(manifest['files']),'bytes_read':byte_count,
            'passed':not bad,'problems':bad,'extra_files_not_covered':extras,
            'scope':'File identity only; no semantic review or new system execution.'}

def main():
    ap=argparse.ArgumentParser(description=__doc__)
    ap.add_argument('--root',type=Path,default=Path(__file__).resolve().parent)
    ap.add_argument('--manifest',default='file-manifest-20260915.json')
    a=ap.parse_args();result=verify(a.root,a.manifest)
    print(json.dumps(result,ensure_ascii=False,indent=2));return 0 if result['passed'] else 1

if __name__=='__main__':sys.exit(main())
