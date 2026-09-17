# Zusatzbewertung: Agentenbeitrag und vollständige Planungsartefakte

Stand 16.09.2026 · Vorbereitung abgeschlossen, neue semantische Urteile noch offen. Keine System-/Modell-/GitHub-Ausführung, kein Codeeingriff, kein Freeze. Dieses Paket dient einer retrospektiven Ergänzung der vorhandenen Evaluation und ist kein neuer Benchmark.

## Einstieg

1. [A1–A10-Anspruchsmatrix und Arbeitsstatus](../../../Thesis-Docs/Writing/claude-writing/P10-Anspruchsmatrix-und-Bewertungsfreigabe-2026-09-16.md): gesamte Evaluation, unterschiedliche Messmaßstäbe und Wiederholungsentscheidungen.
2. [Bewertungsprotokoll v1.0](./protokoll-v1.md): feste Rubrik, Messpunkte, Einheiten, Urteilsrollen, Ablauf und Grenzen.
3. [Soll-Inhalte der drei Arbeitspakete](./soll-inhalte.md): Referenzinhalte vor der Qualitätsbewertung, ohne Ergebniswertung.
4. [Technische Materialprüfung](./material-checks.json): historische Kontextzuordnung und wörtliche Zustands-/Projektionsentsprechung; keine semantische Qualitätsnote.
5. [Unbewertete Ergebnistabelle](./ratings-template.json): Kriterium und Fall fest, Urteile bewusst leer. Die spätere Auswertung wird in einer neuen Datei gespeichert; die Vorlage bleibt erhalten.

## Fallblätter

- [AN-01](./dossiers/AN-01.md) · [vollständige Daten](./dossiers/AN-01.json)
- [AN-02](./dossiers/AN-02.md) · [vollständige Daten](./dossiers/AN-02.json)
- [AN-03](./dossiers/AN-03.md) · [vollständige Daten](./dossiers/AN-03.json)
- [AN-04](./dossiers/AN-04.md) · [vollständige Daten](./dossiers/AN-04.json)
- [AN-05](./dossiers/AN-05.md) · [vollständige Daten](./dossiers/AN-05.json)
- [AN-06](./dossiers/AN-06.md) · [vollständige Daten](./dossiers/AN-06.json)
- [AN-07](./dossiers/AN-07.md) · [vollständige Daten](./dossiers/AN-07.json)
- [AN-08](./dossiers/AN-08.md) · [vollständige Daten](./dossiers/AN-08.json)
- [AN-09](./dossiers/AN-09.md) · [vollständige Daten](./dossiers/AN-09.json)
- [AN-10](./dossiers/AN-10.md) · [vollständige Daten](./dossiers/AN-10.json)
- [AN-11](./dossiers/AN-11.md) · [vollständige Daten](./dossiers/AN-11.json)
- [AN-12](./dossiers/AN-12.md) · [vollständige Daten](./dossiers/AN-12.json)
- [AN-13](./dossiers/AN-13.md) · [vollständige Daten](./dossiers/AN-13.json)
- [AN-14](./dossiers/AN-14.md) · [vollständige Daten](./dossiers/AN-14.json)
- [AN-15](./dossiers/AN-15.md) · [vollständige Daten](./dossiers/AN-15.json)
- [AN-16](./dossiers/AN-16.md) · [vollständige Daten](./dossiers/AN-16.json)
- [AN-17](./dossiers/AN-17.md) · [vollständige Daten](./dossiers/AN-17.json)
- [PBI-003](./dossiers/PBI-003.md) · [vollständige Daten](./dossiers/PBI-003.json)
- [PBI-032](./dossiers/PBI-032.md) · [vollständige Daten](./dossiers/PBI-032.json)
- [PBI-046](./dossiers/PBI-046.md) · [vollständige Daten](./dossiers/PBI-046.json)

## Belegzugang und Nachrechnung

Vier zusammengehörige historische Laufordner sind vollständig und unverändert unter `originals/runs/` gesichert: Analyst, Steward-Sitzung, Folgekette und späterer Rückleselauf. 17 Berichtseinträge, vier Kritiker-Ablehnungen und drei gekoppelte PBI-/Issue-Paare sind fest ausgewählt. Der Vorbestand stimmt im projektseitigen Fingerabdruck mit dem Analyst-Bericht überein. Alle 56 expliziten Ankerverwendungen sind technisch auflösbar; ob sie die Herleitung tragen, bleibt Gegenstand der folgenden Bewertung.

[Originalmanifest](./material-manifest.json) enthält die Dateiidentität; [Paketinventar](./package-manifest.json) erschließt zusätzlich Protokoll, Dossiers und Vorlagen. Prüfung ohne Modellzugriff: `python3 verify-package.py`. Die .NET-Probe unter `fingerprint-probe/` rekonstruiert nur das dokumentierte Fingerabdruckverfahren. Das Python-Prüfprogramm verifiziert Originale, Quelldaten der Dossiers, die unbewertete Vorlage und die drei archivierten Projektionsvergleiche.

Die Protokolle enthalten keine ausformulierten Werkzeugrückgaben und keinen gesicherten historischen Analyst-Modell-/Promptstand. Mitkopierte Code-/Promptdateien sind der Lesestand vom 16.09., keine Behauptung einer identischen damaligen Ausführung. Die Stellungnahme zum fehlenden Benutzerwortlaut und zu nebenläufigen Linsen im Protokoll beachten. Ein historisches Gate-„apply“ ist keine heutige externe Qualitätsannotation.

Dieses Paket erweitert den zentralen Belegbestand. Es übernimmt dessen noch offenen Material-/Weitergabestatus; die lokale Archivierung schafft keine zusätzliche Freigabe. Die wissenschaftliche Quellenreferenz zur Rubrik bleibt in `protokoll-v1.md`; lizenzierte Normtexte werden hier nicht erneut verteilt.
