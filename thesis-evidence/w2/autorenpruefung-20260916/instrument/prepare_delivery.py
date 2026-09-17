from pathlib import Path
import shutil,json,zipfile,gzip,hashlib
H=Path(__file__).resolve().parent;P=H/'paket';D=H/'lieferung'
D.mkdir(exist_ok=True)
for p in P.iterdir():
 if p.is_file() and p.name!='pruefdaten.json':shutil.copy2(p,D/p.name)
(D/'pruefdaten.json.gz').write_bytes(gzip.compress((P/'pruefdaten.json').read_bytes(),compresslevel=9,mtime=0))
with zipfile.ZipFile(D/'belege.zip','w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
 for p in sorted((P/'belege').iterdir()):z.write(p,str(p.relative_to(P)))
txt=(D/'README.md').read_text().replace('`pruefdaten.json`:','`pruefdaten.json.gz` (komprimierte JSON-Datei):').replace('`belege/`: unveränderte Originalkopien.','`belege.zip`: unveränderte Originalkopien unter den im Manifest bezeichneten `belege/`-Pfaden. Bei Bedarf entpacken; die Oberfläche zeigt sie bereits ohne Entpacken.')
(D/'README.md').write_text(txt)
for fn in ['build.py','finish.py','prepare_delivery.py']:
 (D/'instrument').mkdir(exist_ok=True);shutil.copy2(H/fn,D/'instrument'/fn)
shutil.copy2(H/'qa/browser-check.json',D/'browserpruefung.json')
# Do not copy test answers or browser profiles into the deliverable.
assert not any('TEST-' in p.name for p in D.rglob('*'))
d=json.loads(gzip.decompress((D/'pruefdaten.json.gz').read_bytes()))
with zipfile.ZipFile(D/'belege.zip') as z:
 for s in d['sources'].values():assert hashlib.sha256(z.read(s['file'])).hexdigest()==s['sha256']
manifest={str(p.relative_to(D)):hashlib.sha256(p.read_bytes()).hexdigest() for p in sorted(D.rglob('*')) if p.is_file() and p.name!='liefermanifest.json'}
(D/'liefermanifest.json').write_text(json.dumps({'files':manifest,'human_answers':0,'scope':'prepared author review; no author judgments completed'},ensure_ascii=False,indent=2)+'\n')
print(json.dumps({'delivery':str(D),'files':len(manifest),'size_MB':round(sum(p.stat().st_size for p in D.rglob('*') if p.is_file())/1e6,1),'human_answers':0}))
