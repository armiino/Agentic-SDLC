# W2-Gold Änderungslog v1.2 → v2 (07.09.2026)

> Status: ERLEDIGT/HISTORIE — dokumentiert den Übergang auf den adjudizierten Gold-Stand v2
> (Hash ② in `benchmark-checksums.txt`). Ab Hash ② ist v2 unveränderlich.

## Herkunftskette (Provenienz)

1. **Externe Gold-Reviews** (Kollege, 07.09.2026, nur Quelle+Gold, ohne Leitfaden/Outputs):
   `Thesis-Docs/Writing/claude-writing/kapitel-7/02-gold-pruefung/Gold-Review-Interview-Einrichtung-2026-09-07.md`
   + `Gold-Review-Meeting-2-Extended-2026-09-07.md` — beide gegen Kanonik verifiziert
   (Hashes identisch, Zahlen exakt, Zitate wortgetreu; Protokoll im Adjudikations-Arbeitsblatt §0).
2. **Adjudikations-Arbeitsblatt** mit KI-Entscheidungsvorschlägen (als solche gekennzeichnet):
   `…/02-gold-pruefung/ADJUDIKATION-Gold-Review-Interview-finish.md`.
3. **Autor-Übernahme:** 07.09.2026, wörtlich „übernommen wie eingetragen" (Chat) — erst diese
   Übernahme macht die Einträge zur Autorentscheidung. Keine Änderung wurde anhand von
   Arm-/Systemergebnissen ausgewählt; bisherige Leistungswerte waren kein Auswahlkriterium.

## A. Formulierungspräzisierungen (Statements geändert, IDs stabil)

| ID | Befund | Änderung |
| --- | --- | --- |
| G-IE-058 | K1 | Verallgemeinerung „für ihre Rolle freigegebene Funktionen" ersetzt durch enge Fassung nach AU-0083 (eigenes Profil + eigene About-Me-Funktionen); Spannung zu AU-0130 in Note |
| G-IE-026 | K2 | „Liste" → „Übersicht der Bewohnerprofile" (Layout bleibt in G-IE-086 offen) |
| G-IE-095 | K5 | Icons/Tags jetzt IM Statement als „mögliche Gestaltung" (Entwurfsstatus AU-0129) |
| G-IE-014 | M1 | Eingabedialog für Texte/Bilder ergänzt (wörtlich in AU-0033) |
| G-IE-082 | M2 | Symbolmerkmal „nach links zeigender Pfeil" ergänzt (AU-0120) |
| G-IE-085 | M3 | Position „unterhalb der Suchleiste" ergänzt (AU-0122), Layout offen |
| G-IE-096 | M4 | Admin-Aktionen ausdrücklich im Settings-Screen verortet (AU-0130) |
| G-IE-073 | M7 | „oberer Bildschirmbereich" → „oberes Drittel" (AU-0117), weiche Präferenz erhalten |
| G-M2-016 | M2-K1 | „soll erfasst werden können" → „Pro Schicht wird eine Übergabe-Notiz benötigt; die App soll deren Erfassung ermöglichen" (AU-0016) |
| G-M2-021 | M2-K2 | No-Go-Suchfall + Bewohner-Zuordnung ins Statement (AU-0019) |
| G-M2-029 | M2-K3 | „Für die weitere Planung wird … angesetzt" (Planungsgrundlage per AU-0032) |

## B. Neue / entfernte Gold-Aussagen

| ID | Befund | Änderung |
| --- | --- | --- |
| **G-IE-109 (NEU)** | M6 | Unbeschlossener Ablaufvorschlag „bei Seitenaufruf aus Firebase laden + lokal speichern" (AU-0106/0107), Typ `architecture`, Vorläufigkeit im Statement. **Interview-Nenner 108 → 109.** |
| — | M5 | BEWUSST keine neue ID: Beschreibungsmuster-Beispiel nur als Note an G-IE-050 (Beispiel einer bereits erfassten offenen Ausgestaltung, G-IE-051) |

Entfernt: keine. meeting-2-Nenner unverändert 31.

## C. Geänderte Interpretation (Notes/Regeln; keine Nenner-Wirkung)

| ID(s) | Befund | Änderung |
| --- | --- | --- |
| G-IE-105/107 | K3 | Notes: Aufteilung = Interpretation der gespannten AU-0135, nicht einzig zwingende Lesart; Output, der die Spannung benennt, gleichwertig |
| G-IE-040 | K4 | Note: Fortschreibung ungeklärt; bloße Nichterwähnung ≠ Widerruf; Output mit frührem Logout-Symbol nicht abwerten |
| G-IE-062/064/091 | K5 | Notes: interner Entwurfsstatus (AU-0088/0089 bzw. AU-0128) |
| G-IE-055 | K5 | Note: Reichweite von „User" unklar |
| G-IE-050 | M5 | Note: Beispiel Körperteil→Aktion/Interpretation, getrennte Felder, unbeschlossen |
| G-IE-032 | M8 | Note: frühere weiche Präferenz „unten in der Mitte" nicht durch Nichterwähnung aufgehoben |
| G-M2-022 | M2-E1 | Note: Auflage der Heimaufsicht + Einsatzvoraussetzung (AU-0021), keine externe Rechtsbestätigung |
| G-M2-030 | M2-K3 | Note: Pronomen-Vorbehalt („Das klären wir" benennt Gegenstand nicht erneut) |

**Adjudizierte Regeln (gelten für das gesamte Matching, beide Fälle):**

1. **Fortschreibungsregel:** Explizite Ersetzung, Präzisierung und bloße Nichterwähnung werden
   unterschieden; **bloße spätere Nichterwähnung widerruft eine frühere Aussage nicht.**
   (Autor-Abweichung von der ursprünglichen Empfehlung, die AU-0120 als geschlossene
   Neuspezifikation lesen wollte — der Aufzählung fehlt der ausdrückliche Ausschluss.)
2. **Entwurfsstatus:** Das Gold umfasst interne, vorläufige Entwurfsanforderungen; ihr Status
   ist bewertungsrelevant und steht in Statement oder Note. Keine mechanische Abschwächung
   aller Soll-Sätze.
3. **Bewertungsrelevante Notes:** Notes mit Status/Modalität oder Matching-Leitplanken sind
   verbindlicher Teil des Bewertungsvertrags: G-IE-026, 032, 040, 050, 055, 058, 062, 064,
   091, 095, 105, 107 · G-M2-016, 021, 022, 029, 030. Übrige Notes sind Kontext.
4. **P2 (verblindete Fenster-Annotation): nicht durchgeführt/entfallen** — stattdessen erfolgte
   je Fall eine vollständige gold-bewusste KI-Prüfung (alle Aussagen vorwärts, alle Units
   rückwärts). So ausweisen, NICHT als „übererfüllt".
5. **P3 („zentral"-Teilmenge): gestrichen**, nicht nachgefordert. Die ungewichtete Coverage wird
   als Abdeckung des operationalen Bestands ausgewiesen (37/109 Interview-IDs aus dem
   Design-Abschnitt = deklarierte Zusammensetzungs-Grenze); Ankündigungen einer
   „zentral"-Zweitlesart entfallen.

## D. Veränderte Quellenzuordnung

| ID | Befund | Änderung |
| --- | --- | --- |
| G-IE-058 | K1 | + AU-0130 (trägt die in der Note dokumentierte Spannung) |
| G-IE-026 | K2 | + AU-0122 (spätere Layoutpräzisierung) |

Gesamtrefs Interview: 180 → 184 (2 Zuordnungen + 2 der neuen G-IE-109); meeting-2: 45 unverändert.

## Konsequenz für Messwerte

Alle vor v2 berechneten Urteile/Kennzahlen tragen das Etikett „Stand Hash ① v1.2" und werden
nicht mit v2-Werten vermischt. Betroffene Urteile beider Arme werden nach den Matchregeln gegen
v2 neu gefällt (Statement-Änderungen A, neue ID B, Regel-widersprechende Alt-Urteile aus C);
alle Interview-Quoten wechseln auf Nenner 109. Der Effekt ist ergebnisoffen.
