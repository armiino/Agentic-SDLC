# Archivierte Nachweise zur Evaluation und Architektur

Bereitgestellt am 15.09.2026. Die Originaldateien liegen unter `originals/` in ihrer ursprünglichen relativen Repository-Struktur. [Manifest](./manifest.json): Originalpfad, Kopie, Prüfsumme, Dateigröße, Beleggruppe und Zuordnung der Laufkennungen zu Kapitelquellen. Dies ist eine Belegauswahl, kein eingefrorenes oder erneut vollständig getestetes Gesamtprojekt.

## Hauptvergleich und Stufenanalyse

- [Eingaben](./originals/input/transcripts/) und [Gold, Matchregeln, Prüfsummen und Änderungshistorie](./originals/input/eval-labels/).
- [Originale der Ledger-Läufe](./originals/runs/ledger/) und [Arm-F-Läufe einschließlich der vier ausgewerteten Abbrüche](./originals/runs/arm-f/). Die in der Thesis ausgewerteten Erfolgsläufe und Abbrüche sind im Laufindex unten zugeordnet; Abbruch bedeutet keine gemessene semantische Abdeckung von null.
- [Aktuelle Ergebniszugänge](../eval/) und [Nachreview-Konsolidierung](../eval/w2-nachreview-konsolidierung.json). [Historische Bewertungs-/Verifikationsblätter](./originals/Thesis-Docs/aktiv/w2-validierung/) bleiben einschließlich später zurückgenommener Etiketten unverändert erhalten. Die aktuelle Thesis und die Konsolidierung begrenzen, welche Urteile menschlich geprüft wurden.
- [Analysebestand vom 07.09.](./originals/Thesis-Docs/Writing/claude-writing/kapitel-7/05-vergleichs-inspektion/00-LESEFADEN.md): gespiegelte Prompts und ausgewählte Implementierung, Vertragsvergleich sowie damalige Ausgaben. Zahlen im dortigen älteren Lesefaden ersetzen nicht Gold-v2, Nachreview und spätere Ergänzungen.
- [Z1-Verifikation](../Z1-Verifikationsprotokoll-2026-09-07.md) und [Assertion-Zuordnung](../z3-assertion-zuordnung.md): datierte Prüfberichte mit ihrem jeweiligen Prüfungsumfang. Die archivierten Auswertungsprogramme sind historische Instrumente; nicht jedes Programm erzeugt die später konsolidierten Urteile automatisch neu.

## Governance und technische Nachweise

- [F1](./originals/runs/fullworkflow/20260818_084712_765eea/): Vorlagen, Einzelentscheidungen, angewandte Auswahl, gespeicherte Zwischenstände und reale Issue-Projektion. [F3](./originals/runs/fullworkflow/20260804_121433_eb181a/): interaktive Entscheidungsauflösung; übrige AcceptAll-Gates und Dry-Run bleiben als Versuchsgrenzen bestehen.
- [HITL-Gegenprüfung](./originals/Thesis-Docs/Writing/claude-writing/P10-HITL-Governance-Nachweis-2026-09-15.md) und [Kettenabgleich](./originals/Thesis-Docs/Writing/claude-writing/P10-HITL-Kettenabgleich-2026-09-15/README.md) erschließen die damaligen Ereignisse und den späteren Analysebestand.
- [Resume-Demonstration](./originals/runs/fullworkflow/20260805_144056_dc711f/): vorhandene Eltern-Checkpoints und Ereignisse. Kein durch Dateianzahl allein geführter Zuverlässigkeitsbeleg.
- [Bootstrap-Lauf 1](./originals/runs/fullworkflow/20260727_052405_3c8e13/) und [Lauf 2](./originals/runs/fullworkflow/20260727_055348_bb9b1a/), mit [geparktem Core 1](./originals/state/core-b6-lauf1/project-state.json) und [Core 2](./originals/state/core-b6-lauf2/project-state.json). Die Zuordnung steht in der [damaligen Notiz](./originals/Thesis-Docs/thesis-story/2026-07-27/iteration-notes-bootstrap-graph-b6-ein-graph.md); kein neuer Lauf mit heutigen Modellversionen.
- [Facettenintegration vom 09.09.](./originals/runs/fullworkflow/20260909_190518_f58d6a/): Beleg der ausgeführten Vervollständigung, anschließend Pause am Ingest-Gate. Kein vollständig angewandter aktueller Transkript-zu-GitHub-Durchlauf.
- [Technischer Nachtrag vom 11.09.](./originals/Thesis-Docs/Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11.md) mit [TRX 701/701](./originals/Thesis-Docs/Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11/full-suite.trx), [Smoke-Protokoll 14/0](./originals/Thesis-Docs/Writing/claude-writing/Technische-Uebernahme-R75-B20-2026-09-11/smoke.log) und Standprüfsummen. Hier wurden diese Nachweise kopiert und geprüft, keine C#-Tests oder Systemläufe erneut ausgeführt. Die ältere Angabe 688/688 bleibt an ihren Bericht vom 07.09. gebunden; das TRX vom 11.09. wird nicht als dessen Original ausgegeben.

Die zum Kettenabgleich gelesenen 675 Quell-/Projekt-/Konfigurationsdateien stimmen mit dessen archivierten SHA-256-Angaben überein. Dieser Analysebestand macht die technischen Aussagen überprüfbar. Er ist kein eigenständig abgenommener Release und ersetzt keinen Abgleich nach späteren Codeänderungen. Historische Berichtskopien können auf Arbeitsunterlagen außerhalb dieser Auswahl verweisen; der vorliegende Index benennt die bereitgestellten Primärzugänge.

## Technische Nachrechnung

Für die Tokenaggregation genügt Python 3 mit Standardbibliothek, ausgeführt aus diesem Ordner:

```sh
python3 originals/thesis-evidence/w2/recompute-token-usage.py --repo originals
```

Das Skript liest ausschließlich die kopierten Zähler-/Span-Dateien und gibt JSON aus. Die Neuberechnung stimmt mit der [korrigierten Ergebnisdatei](../token-usage-correction-20260910.json) vollständig überein. Sie erneuert keine semantischen Gold- oder Claim-Urteile.

## Laufindex

55 eindeutige Kennungen sind erschlossen: 37 Laufordner hier kopiert, 17 über die bestehende Kapitel-4-Auswahl zugänglich und ein bereits dokumentierter fehlender Originalrun über seinen Ersatzbeleg. Die vollständige Kopie bezieht sich auf den bei der Bereitstellung vorhandenen Dateiinhalt des Laufordners; sie garantiert nicht, dass damals jedes Ereignis protokolliert wurde. `.DS_Store` und Python-Bytecode sind ausgeschlossen.

| Laufkennung | Zugang und Status |
|---|---|
| `20260220_165915_7d83ce` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260519_150331_b9fbd4` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260526_155811_f151d2` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260528_130843_589104` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260529_101357_d4d49c` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260617_162205_950a9a` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260623_175026_5e7d1c` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260630_145558_68149f` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260723_093443_a85a80` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260723_113115_35335a` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260723_142742_fce7b4` | [E48-02: Original fehlt, bekannter Ersatzbeleg](../../chapter-4/README.md) |
| `20260723_152440_f1e0f6` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260725_162437_790824` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260725_162437_790824) |
| `20260727_052405_3c8e13` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260727_052405_3c8e13) |
| `20260727_055348_bb9b1a` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260727_055348_bb9b1a) |
| `20260727_074919_b2c654` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260727_080638_9d4ae8` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260729_142120_1ac7c2` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260729_142120_1ac7c2) |
| `20260803_172914_4bfacc` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260804_074419_acab74` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260804_121433_eb181a` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260804_121433_eb181a) |
| `20260805_144056_dc711f` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260805_144056_dc711f) |
| `20260806_115016_01715f` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260806_115016_01715f) |
| `20260806_145812_c29ed2` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260806_145812_c29ed2) |
| `20260806_151204_3c19d2` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260806_151204_3c19d2) |
| `20260817_100135_03f491` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260817_102027_b9c264` | [Bestehendes Kapitel-4-Paket; Auswahl dort ausgewiesen](../../chapter-4/README.md) |
| `20260817_123342_f23f06` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260817_123342_f23f06) |
| `20260818_084712_765eea` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260818_084712_765eea) |
| `20260818_123055_a0b172` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260818_123055_a0b172) |
| `20260818_183319_b3d6e6` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260818_183319_b3d6e6) |
| `20260819_102959_b8e8f8` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260819_102959_b8e8f8) |
| `20260820_165512_73b1dc` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_165512_73b1dc) |
| `20260820_172043_802e63` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_172043_802e63) |
| `20260820_172921_1ca9d2` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_172921_1ca9d2) |
| `20260820_173141_6e75fa` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_173141_6e75fa) |
| `20260820_182614_a5f6b3` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_182614_a5f6b3) |
| `20260820_183855_b7747b` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_183855_b7747b) |
| `20260820_190220_895d18` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260820_190220_895d18) |
| `20260821_204808_042823` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260821_204808_042823) |
| `20260905_164206_e30646` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260905_164206_e30646) |
| `20260905_164650_8189ea` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260905_164650_8189ea) |
| `20260905_165151_2dde4d` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260905_165151_2dde4d) |
| `20260905_165658_1271bb` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260905_165658_1271bb) |
| `20260906_141518_12d046` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_141518_12d046) |
| `20260906_143052_6540e2` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_143052_6540e2) |
| `20260906_161815_87b89b` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_161815_87b89b) |
| `20260906_161816_2c354c` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260906_161816_2c354c) |
| `20260906_161949_037232` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_161949_037232) |
| `20260906_162749_11c840` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_162749_11c840) |
| `20260906_162750_aa7592` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260906_162750_aa7592) |
| `20260906_162959_2e34bf` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_162959_2e34bf) |
| `20260906_163431_f0d718` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/arm-f/20260906_163431_f0d718) |
| `20260906_164332_90a2c4` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/ledger/20260906_164332_90a2c4) |
| `20260909_190518_f58d6a` | [Vorhandener Laufordner vollständig kopiert](./originals/runs/fullworkflow/20260909_190518_f58d6a) |
