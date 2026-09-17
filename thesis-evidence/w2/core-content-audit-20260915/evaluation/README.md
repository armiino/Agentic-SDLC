# Auswertung: inhaltliche Herkunft und Fortschreibung

15.09.2026. **Die acht fest ausgewählten zusätzlichen Fälle und vier bekannten Kontrollen sind geprüft.** Historische Originale und Auswahl unverändert; Urteile und drei begrenzte Autorenantworten sind getrennt dokumentiert.

Einstieg: [Ergebnisbericht](bericht.md). Danach [Fallkarten](fallkarten.md), [Einzelurteile](assessments.json) und [Autorenprüfung](authors.json). Das [vorher festgelegte Protokoll](../protokoll.md) und die [Auswahl](../selected-cases.json) liegen weiterhin unverändert eine Ebene darüber. Die dortige Vorbereitungs-README und deren Manifest bleiben als Stand vor den Inhaltsurteilen erhalten. Dieses Unterpaket besitzt sein eigenes Quellen- und Dateimanifest.

Der Zusatz prüft ausgewählte inhaltliche Verbindungen und Zustandswirkungen; er liefert keine repräsentative Erfolgsquote. N07 zeigt eine zusammenhängende Ableitung/Freigabe/Relations-/Projektionskette, N03 eine wirksame Ablehnung einer unvollständigen Vorlage. Mehrdeutigkeit, historische Ausführungsgrenzen und ungeklärte Herkunft bleiben sichtbar. Kein Codeeingriff und kein neuer Modelllauf.

750 Quelldateien liegen unter `evidence/`, mit Pfad-/Hashnachweis in [evidence-manifest.json](evidence-manifest.json). Die gesamte Kopie ist kein vollständiger finaler System-Freeze. `dossiers/` enthält zwölf daraus erzeugte Objektauszüge. [verification.json](verification.json) dokumentiert die rein technische Konsistenzprüfung; deren Erfolg bestätigt keine unabhängigen semantischen Urteile.

Portable, lesende Prüfung mit Python 3:

```text
python3 scripts/verify.py
```

Aus dem übergeordneten Vorbereitungsordner ist zusätzlich `python3 scripts/verify_selection.py` ausführbar. Die Prüfungen benötigen die jeweiligen Paketdateien, keine laufende Anwendung und keine Zugangsdaten. Weitere Arbeit bleibt im bestehenden Phase-10-Plan: knapper Textanschluss und gezielte PDF-Abnahme, danach die bereits vereinbarten Abschlussaufgaben.
