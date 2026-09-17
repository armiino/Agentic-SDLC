# Belegpaket B-47: Herkunftsaudit v4

Stand 13.09.2026. [Ergebnis und Einordnung](../P10-3b-Herkunftsaudit-v4-2026-09-13.md). Separates Auswertungsinstrument; keine Änderung des Produktionssystems oder alter Ergebnisdateien.

- `audit_v4.py`: Resolver mit explizitem Repository-/Eingabe-/Ausgabepfad, Python ≥ 3.10, nur Standardbibliothek.
- `test_audit_v4.py`: 34 künstliche Kontrollen. `controls-final.log` dokumentiert die letzte Ausführung.
- `pruefvertrag.md`, `verfahrensentwicklung.md`: Messgegenstand, Grenzen und nachträglich präzisierte Regeln.
- `final-result/audit.json`: alle 237 Items, Einzelreferenzen, Eingangsbelege, historische Bezüge, Zitat-/Unit-Prüfung, jüngster Versionswechsel und Auswertungsgruppen.
- `final-result/sources.json`: SHA-256/Größe aller 414 gelesenen Originaldateien. Das Manifest ist kein Archiv ihrer vollständigen Inhalte.
- `Einzelfaelle.md`: lesbare Übersicht mit separaten Spalten; keine pauschale Gesamterfüllungsquote.
- `Besuchsbeispiel.md`: reale Quellen- und Änderungsketten als Grundlage für den späteren Textnachzug.
- `verify_result.py`, `verification.json`: Quellen-/Fundstellenprüfung, Vergleich zweier Ausführungen und gezielte Akzeptanzfälle aus P10-1.
- `final-result.log`, `reproduction.log`: Konsolenergebnisse der beiden identischen Gesamtläufe.

## Reproduktion

Voraussetzung ist das Repository mit **den im Manifest bezeichneten Originaldateien** am selben Stand. Weder ein frischer Checkout allein noch das Core-JSON allein genügt. Die Auswertung liest auch historische Runs und archivierte Deltas. Es werden keine Netzwerk-, Modell- oder GitHub-Aufrufe durchgeführt. Historischer Core und altes Audit müssen ihre im Bericht angegebenen Prüfsummen behalten.

Zuerst dieses gesamte Belegpaket in einen neuen beschreibbaren Arbeitsordner kopieren. Die datierten Ergebnisdateien nicht überschreiben. Von dort aus, mit dem tatsächlichen Repositorypfad:

```sh
python3 -m unittest -v test_audit_v4
python3 audit_v4.py --repo /pfad/zum/Agentic-SDLC --core state/core/project-state.json --old-audit runs/e2e-evidenz/traceability-audit.json --out reproduction
python3 verify_result.py --repo /pfad/zum/Agentic-SDLC
```

`reproduction` darf vor dem Aufruf nicht existieren. Das Instrument liest die Eingaben und erzeugt nur den neuen Ausgabeordner. Der Prüfer vergleicht `reproduction/audit.json` und `sources.json` bytegenau mit `final-result/`, prüft die Originaldateien und die ausgegebenen JSON-Pointer und schreibt `verification.json` nur in dieser Arbeitskopie neu. Die Fixture-Tests verwenden eigene temporäre künstliche Repositories.

Die Skriptprüfsumme gehört zur Ergebnisdatei. Jede Änderung am Resolver erzeugt deshalb einen neuen Auswertungsstand; nicht nur Resultatdateien kopieren und als unveränderte Reproduktion ausgeben. Die historischen v3-Dateien liegen unverändert im Repository bzw. zusätzlich im P10-1-Belegpaket.

## Grenzen

Die 234 Items mit einem Referenz-/Eingangs-/Beitragspfad sind keine 234 vollständig erklärten aktuellen Inhalte. Indirekte Beiträge können nur Teile erklären; eine Definition ohne passenden Quelleninhalt ist kein semantischer Nachweis. Das separate Feld `anyDefinitionOrContribution` schließt eigene Erzeugungsartefakte ein und darf nicht als vollständige Quellenabdeckung verwendet werden. Einzelreferenzen bleiben separat offen.

Die Änderungsprüfung behandelt den jüngsten gespeicherten Übergang bei Version > 1 und kennt vor allem Ingest- und PBI-Berichte. Sie ist kein Voll-Audit aller Entscheidungsausgänge, Status-/Payloadänderungen, Bestätigungen oder Freigaben. Die Quellenklassen-Tabelle zeigt identifizierte Belege, keine vollständige Testabdeckung aller möglichen Eingänge.

Die Zitatprüfung ist ein normalisierter Textvergleich. Ein Körper-Treffer mit abweichendem Vorspann bedeutet weder ein vollständiges wörtliches Zitat noch automatisch eine falsche Personenzuordnung. Vorhandene Units und Zitate beweisen nicht, dass jede Proposition inhaltlich vollständig gestützt ist. Es fand keine neue unabhängige semantische Annotation statt.
