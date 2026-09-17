# F1: Korrektur der Itemzählung, 15.09.2026

Dieser datierte Nachtrag berichtigt ausschließlich die Itemzählung im Fallblatt Z2-F1 zu Lauf `20260818_084712_765eea`. Er ersetzt die dortige Angabe „+14 Items (REQ-80–86, …)“ und die beiden anschließenden Bezugnahmen auf alle 14 Items. Der historische Berichtstext und die Originalruns bleiben erhalten. Inhaltliche Einzelurteile werden durch diesen Nachtrag nicht neu bewertet.

**Korrektes Ergebnis:** Zwischen dem Zustand vor dem Anforderungs-Ingest und dem Zustand vor der GitHub-Projektion entstehen elf neue Core-Items. Im Vergleich dieser gespeicherten Stände wird kein Item entfernt.

| Gespeicherter Zustand | Anzahl | Gegenüber dem vorherigen Zustand hinzugekommen |
|---|---:|---|
| vor `07-ingest` | 201 | Ausgangsbestand |
| vor `07-arch-ingest` | 207 | REQ-80–83, DEC-004, DEC-005 |
| vor `07-pbi-update` | 208 | ARCH-46 |
| vor `07-github` | 212 | FC-15, PBI-045–047 |

Grundlage ist jeweils `applied/core-before.json`; geprüft wurden die Item-ID-Mengen, nicht nur die Differenz der Gesamtzahlen. Die im alten Fallblatt zusätzlich genannten REQ-84–86 sind in keinem dieser vier Zustände enthalten. Wann diese Kennungen in anderen Abläufen belegt wurden, ist kein Gegenstand dieser Korrektur. Der anschließende Forward-Bericht weist drei erzeugte GitHub-Issues aus; diese werden getrennt von Core-Items gezählt. Die Vermutung einer ursprünglichen Vermischung von Issues und Items ist durch den Wortlaut des alten Berichts nicht belegt.

Die Begrenzung des F1-Urteils bleibt bestehen: Die verfolgten Besuchsketten und belegten Operationen wurden untersucht, nicht sämtliche neuen Inhalte fachlich vollständig geprüft. Auch elf neue Items sind keine Qualitäts- oder Erfolgsquote. Die zwölf Ingest-Vorschläge, acht Übernahmen und vier Ablehnungen haben andere Bezugsgrößen und bleiben unverändert.

Sechs unveränderte Originaldateien sind im [Belegpaket mit Originalpfaden und SHA-256](./f1-zaehlkorrektur-20260915/manifest.json) bereitgestellt. Die Dateien liegen dort unter `originals/runs/fullworkflow/20260818_084712_765eea/` mit ihren ursprünglichen Unterverzeichnissen. Das Manifest nennt alle hinzugekommenen IDs sowie die separate Forward-/Mapping-Bilanz.
