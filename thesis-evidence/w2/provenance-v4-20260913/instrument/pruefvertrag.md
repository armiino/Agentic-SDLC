# B-47 / Audit v4: Prüfvertrag vor der Neuauswertung

Festgelegt am 13.09.2026, vor dem ersten vollständigen v4-Lauf. Nachträgliche Verfahrensänderungen werden in einem Änderungsprotokoll begründet. Explorative Instrumentkorrektur nach der Diagnose P10-1, kein vorab geplantes ursprüngliches W2-Experiment.

## Ausgangsbasis und Umfang

Unveränderter historischer Core (237 Items), SHA-256 `afcba4bac58ba82fb30b4a413d977f6474be384a377765c0bdb0dafc7bcba22c`; altes Audit `0a540e2fc9bda50afe12ee589b7f488914a6ceb670b9fdcdd093c579b6298d04`. Alte Zahlen und Auswertungsdateien werden erhalten. Alle gelesenen Quellen werden mit Prüfsummen erfasst und am Ende auf Unverändertheit geprüft. Keine Modellaufrufe und keine rückwirkende Datenreparatur.

## Getrennte Ergebnisdimensionen

1. **Definierender oder beitragender Herkunftspfad:** Gibt es für ein Core-Item mindestens eine explizite Kette zu einer externen Definition, einem zugeordneten Eingangsobjekt oder über eine gerichtete `covers`-/`part_of_feature`-Beziehung zu einem belegten Bestandteil? Diese drei Nachweisarten bleiben getrennt. Ein Beitrag ist keine vollständige Inhaltserklärung.
2. **Einzelreferenzen:** Jede aktuelle `sourceClaimIds`-/`sourceArtifactItemIds`-/Candidate-/Decision-Referenz wird gesondert im bezeichneten Laufkontext geprüft. Fehlende und mehrdeutige Verweise bleiben trotz eines anderen erfolgreichen Pfads sichtbar. Historische und zusätzliche Herkunftskanten werden gesondert ausgewiesen; sie werden nicht stillschweigend zu Quellen des aktuellen Inhalts erklärt. Keine pauschale Erfolgsquote über heterogene Belegarten.
3. **Änderungsbeleg:** Für gespeicherte Versionen > 1: Ist die konkrete Übernahme/Änderung am referenzierten Lauf und Item identifizierbar? Historiennotizen und vorherige Versionen können den Lauf ausdrücklich verbinden. Ingest-Plan plus Apply-Zuordnung bzw. PBI-Plan plus Apply-Bericht werden getrennt vom bloßen Vorhandensein eines alten Ursprungs bewertet. Wenn Textvergleich möglich ist, wird er separat vermerkt. Keine Vollständigkeitsbehauptung für alle Status-/Payloadänderungen oder Bestätigungen, die keine gespeicherte Version erzeugten.
4. **Zitat und Units:** Alle vorhandenen Evidenzzitate an aufgelösten Claim-Definitionen: vollständig normalisiert im zugeordneten Transkript, nur Zitatkörper passend bei abweichendem Präfix, zu kurz (< 40 normalisierte Zeichen), nicht passend oder ohne eindeutige Transkriptbindung. Keine stille Präfixentfernung. Unit-IDs werden im zugeordneten Ledger auf Existenz geprüft; dies ist keine semantische Stützungsannotation.

## Zulässige Wege

- Start sind explizite Itemfelder, History-Felder/-Notizen, itembezogene Metadaten und Core-Relationen. Kein repo-weites Suchen nach gleichlautender ID oder ähnlichem Text als Erfolgsregel.
- Run-Ordner werden anhand ihrer exakten Kennung gefunden; mehrdeutige Run-Ordner sind keine stillschweigend auswählbare Quelle.
- Konfigurationen, Run-Reports und typisierte Stage-Manifeste stellen explizite Datei-/Run-Verbindungen her. Der normale Quellenkontext umfasst maximal drei Upstream-Hops; Überschreitungen bleiben sichtbar. Eine direkt konfigurierte Datei ist eine genaue Quelle, keine Erlaubnis zur globalen ID-Suche.
- Claim-Definitionen in ausdrücklich zugeordneten Consumables haben Vorrang gegenüber früheren Bearbeitungsständen. Unterschiedliche Definitionen auf derselben maßgeblichen Stufe ergeben Mehrdeutigkeit; identische Kopien werden mit ihren Fundorten dokumentiert.
- Ingest-Apply verbindet Core-ID mit Eingangs-ID; der zugehörige Plan referenziert das Eingangsdelta. Ein konfigurierter `fromDelta`-Eingang oder ein typisierter vorbereiteter Delta-Eingang darf als Quelle dienen. `04-delta/project-state.json` ist nur mit explizitem Eingangsvertrag zulässig; Core-Sicherungen und Checkpoints niemals als unabhängige Erzeugungsbelege.
- Metadaten wie `ingestedFrom` und `legacyPbiId` werden nur im ausdrücklich bezeichneten Eingangs-/Erzeugungskontext ausgewertet. Aktueller und ursprünglicher Kontext bleiben getrennt.
- GitHub-/Autor-/Analyst-Definitionen sind legitime Eingangsbelege, aber kein automatischer Nachweis ihrer vorgelagerten Realität bzw. vollständigen semantischen Treue. Ein Sitzungslabel ist kein geprüftes Gesprächsprotokoll. Keine Online-Abfrage.

## Auswertung und Interpretation

Alle 237 Items werden bearbeitet, einschließlich der bisher positiven. Ergebnisse nach Itemtyp sowie tatsächlich identifiziertem Eingangsbeleg ausweisen; `origin` allein ist keine sichere Klassifikation des letzten Eingangs. Versionierte Änderungen haben einen eigenen Nenner. Die alte 137/38/62-Verteilung wird gegenübergestellt, aber nicht durch eine neue undifferenzierte Erfolgszahl ersetzt. Auswertungstabellen sind deskriptiv für genau diesen Bestand.

Präzisierung während der Instrumententwicklung: Das bloße Wiederfinden eines eigenen Erzeugungsartefakts wird als Dokumentzugang ausgewiesen, aber aus dem engeren Referenz-/Eingangsmaß ausgeschlossen. Ein Artefakt mit gleichem Inhalt ist nicht bereits dessen vorgelagerte Quelle. Exakt typisierte interne Decision-Bezüge und Laufnamen ohne Datumsformat sind ebenfalls zulässig; ihr Kontext und ihre Endpunktart bleiben ausgewiesen.

## Kontrollen und Abschluss

Vor der Ergebnisfreigabe prüfen: unverbundene gleiche ID; zwei unterschiedliche Definitionen; gültige plus fehlende Zusatzreferenz; falsches Sprecherpräfix; verändertes Zitatende; kurzes und fehlendes Zitat; falsche Relationsrichtung; Fallback trotz ungelöster direkter Referenz; Autor-Eingang über Apply; falsche Eingangszuordnung; fehlender Apply-Beleg; alte Quelle bei neuem Inhalt; Versionswechsel; Quellenzugang nur über Snapshot; unbekannte Unit-ID. Positive Gegenfälle gehören dazu. Fehlende Dateien/Parserfehler werden gemeldet, nicht stillschweigend verworfen.

Abschluss: ausführbarer separater Resolver mit Tests, Einzelergebnisse und Quellenmanifest, reproduzierbare Gegenüberstellung, begrenzte Aussageempfehlung und konkrete Restbefunde. Produktionscode und TeX werden in diesem Paket nicht verändert; nötiger Textnachzug wird anschließend zusammenhängend vorgenommen.
