# P10: HITL als Governance – Belegabgleich und Textnachzug

Stand: 15.09.2026. **Im aktualisierten PDF inhaltlich abgenommen. B-56 geschlossen; Satzrest Anhang C unter B-12.**
Fortsetzung der [HITL-Inventur](./P10-HITL-Inventur-2026-09-15.md), geführt im [Phase-10-Plan](./Phase-10-Gesamtabnahme-2026-09-13.md). Keine zusätzliche Aufgabenliste.

## 1. Erkenntnisziel und methodische Einordnung

Die menschliche Entscheidungshoheit ist der untersuchte Governance-Zweck. Menschen können Vorschläge prüfen, korrigieren und verbindlich über ihre fachliche Wirkung entscheiden. Eine fehlerfreie Entscheidung oder eine durchschnittliche Qualitätssteigerung durch HITL wird daraus nicht abgeleitet. Der Kontrollanspruch gilt für die beschriebenen Fortschreibungspfade; der ungegatete initiale Seed bleibt als Grenze der A6-Erfüllung erhalten.

Kapitel 3 sieht ausdrücklich ausgewählte historische Fälle und ergänzende Kontrolltests vor. Der jetzige Schritt präzisiert den Freigabebezug zweier bereits ausgewählter Fälle. Auswahl und Prüffragen sind retrospektiv; F1/F3 werden weder zu neuen unabhängigen Fällen noch zu einem aktuellen Integrationslauf. Vorhandene T1–T4 werden anhand von Prüfbericht und Testcode zugeordnet, nicht erneut ausgeführt. Die Facettenbewertung bleibt ein anderer Prüfgegenstand; kein neuer zentraler 5/4/1-Block entsteht.

## 2. F1: Auswahl und tatsächliche Anwendung

Lauf `runs/fullworkflow/20260818_084712_765eea`.

- `07-ingest/plan.json`: zwölf eindeutige Eingangsoperationen; `human-decisions.json`: dieselben zwölf Kennungen, acht `apply`, vier `skip` mit Begründung. Das historische `skip` wird im Text als begründete Ablehnung beschrieben, nicht als fremder Dateiwert `reject` ausgegeben.
- `07-ingest/applied/delta.json`: Die Menge der acht angewandten Kennungen entspricht exakt der freigegebenen Menge. Keine der vier abgewiesenen Kennungen ist angewendet. Das leere Feld `skipped` dieses Berichts ist **nicht** die menschliche Ablehnungsbilanz: Die abgewiesenen Operationen gelangten nicht in die angewandte Auswahl.
- Eingangsoperationen REQ-04/05 führen zu den neuen Core-Anforderungen REQ-82/83; deren Text ist mit dem jeweiligen Vorschlag identisch. Eingangskennung, Claim und Gesamtlauf sind am gespeicherten Item nachvollziehbar.
- Die abgewiesene Eingangsoperation REQ-01 war vom Agenten als **REFINE auf REQ-42** vorgeschlagen. Der Mensch bezeichnete sie als bloße Bestätigung und begründete die Ablehnung mit dem drohenden Verlust vorhandener Angaben zu einnahmebezogenen Bemerkungen. 5.6 unterscheidet nun Modellvorschlag und menschliches Urteil.
- REQ-42 ist vor und nach diesem Ingest als vollständiges Item objektgleich: Text, Version 3 und übrige Angaben bleiben erhalten. Die vier Ablehnungen sind zusätzlich als REJ-001 bis REJ-004 mit passender Eingangskennung, Vorlagetext und wortgleicher Begründung im Core gespeichert.

Verwendete Core-Zwischenstände: `07-ingest/applied/core-before.json` vor der Anforderungseinordnung und `07-arch-ingest/applied/core-before.json` vor der folgenden Architekturanwendung. Der zweite ist ein gespeicherter späterer Zwischenstand, keine neu erzeugte oder als eigenständige historische After-Datei ausgegebene Rekonstruktion. Ereignislog und interaktive Antwort passen zum Übergang. Nicht pauschal geprüft wurden sämtliche späteren Wirkungen aller zwölf Inhalte.

Die bereits belegte Issue-Projektion bleibt erhalten. 7.5 unterscheidet jetzt ausdrücklich die Wortgleichheit **Vorschlag → REQ-82** von der inhaltlichen Erhaltung im anders formulierten Issue-Titel. Der nicht separat archivierte Ausführungsumschalter bleibt als Grenze stehen.

## 3. F3: Menschlich festgelegte Konfliktlösung

Lauf `runs/fullworkflow/20260804_121433_eb181a`.

- Die Eingangsoperation REQ-05 ist `CONTRADICT` zu REQ-32. Die Entscheidungsvorlage enthält den tatsächlichen damaligen Bestandstext: No-Gos bewohnerbezogen, nicht global.
- `decision-gate-decisions.json` enthält die menschliche ADOPT_NEW-Entscheidung und den neuen Text. Ausgang, Text und Begründung stimmen mit `decision-plan.json` überein.
- `decision-apply-report.json` nennt die neue REQ-78, die Ablösung von REQ-32 und die umgestellten PBIs. Der anschließend gespeicherte Core bestätigt den menschlichen Text wortgleich in REQ-78, den Bezug auf DEC-001, die erhaltene alte Anforderung mit Status `superseded`, die aufgelöste DEC und die umgestellten `covers`-Beziehungen von PBI-012/023.
- Vorher: `07-decision/applied/core-before.json`; danach: `07-pbi-update/applied/core-before.json`. Der Ereignisstrom ordnet die Entscheidungsanwendung nach der interaktiven Antwort ein.
- **Nur das DEC-Gate war hier interaktiv.** Adjudikation, Ingest, PBI und Forward wurden als AcceptAll im Experiment beantwortet. Dies steht jetzt direkt in 7.5. Es entsteht kein Nachweis von fünf menschlich beantworteten Gates.
- Die PBI-Inhaltsangleichung blieb als `needs_clarify` offen; reale Projektion war nicht Untersuchungsgegenstand. Die abweichende Herkunftsanzeige und der nur berichtete Core-Restore bleiben benannte Grenzen. Historische Kennungen werden nicht gegen heute anders belegte Live-Items validiert.

## 4. Zusammenspiel der Nachweise

| Aussage | Geeigneter Beleg | Reichweite |
|---|---|---|
| Vorgesehene menschliche Entscheidung wirkt auf Übernahme | F1: Menge der Entscheidungen und Anwendungen, gespeicherte Ablehnungen, ausgewählte Item-Vergleiche | Ein bekannter historischer Fall; acht/vier ist keine Genauigkeitsquote |
| Fachliche Konfliktlösung stammt vom Menschen und wird umgesetzt | F3: Vorlage, Antwort, Plan und späterer Core-Zwischenstand | Nur DEC-Gate interaktiv; PBI-Angleichung offen |
| Fehlende oder abgelehnte Freigabe verhindert fachliche Übernahme | Vorhandene T1/T2 am realen Ingest-Apply-Pfad | Definierte Kontrollfälle, nicht alle zehn Gates |
| Gültige Freigabe ermöglicht Änderung; Wiederholung erzeugt keine zweite fachliche Wirkung | Vorhandene T4/T3, einschließlich Datei- und Historienprüfungen | Keine neue Testausführung und kein allgemeiner Schutz aller externen Schreibvorgänge |

Die maschinell ausgewerteten Feldvergleiche und Quell-Hashes stehen im [Prüfprotokoll](./P10-HITL-Governance-Nachweis-2026-09-15.json). Ihre Anzahl ist eine technische Unterteilung dieser Nachprüfung, kein neuer Benchmark und keine HITL-Erfolgsquote. Die 61 Grundlagen der früheren Inventur waren vor dem Textedit unverändert.

## 5. Textumfang und Sync

| Lokale Datei unter `claude-writing/` | Overleaf-Ziel laut lokaler Hauptdatei | Inhalt |
|---|---|---|
| `chap4/chap4.6.tex` | `Kapitel/04/chap4.6.tex` | Begründung menschlicher Festlegung aus dem Nutzungsziel; kein erfundener früher Fehlversuch |
| `chap5/chap5.6/chap5.6.tex` | `Kapitel/05/05.6.tex` | Zweck, vorgelegte Informationen, Entscheidung, Anwendung; F1 präzisiert; zentraler DEC-Schritt; kurze Abgrenzung PBI/Projektion/Evidenz |
| `kapitel-7/tex-v2/chap7.4/chap7.4.tex` | `Kapitel/07-v02/chap7.4.tex` | Prüfkriterium menschlicher Entscheidungshoheit an Zustandswirkung |
| `kapitel-7/tex-v2/chap7.5/chap7.5.tex` | `Kapitel/07-v02/chap7.5.tex` | Datierter historischer Belegabgleich F1/F3 und jeweilige Grenzen |
| `chap8/chap8.1/chap8.1.tex` | `Kapitel/08/chap8.1.tex` | Einordnung der tatsächlich belegten Entscheidungshoheit in den Forschungsbeitrag |
| `anhang/anhang-gate-matrix.tex` | `Kapitel/05/anhang-gate-matrix.tex` | Zehn Entscheidungspunkte der Hauptkette, Vorlagen/Optionen/Wirkungen und kurze Abgrenzung separater Prüfwege |

B-56 ist lokal bearbeitet: Anhang C beansprucht nicht sämtliche separaten Review-Wege; bei DEC-Auflösung ist die Begründungspflicht fallabhängig beschrieben. Der Haupttext vertieft Anforderungseinordnung und Konfliktauflösung, nicht zehn vollständige Abläufe. Kapitel 3 und 6.4/6.6 tragen den methodischen beziehungsweise technischen Anschluss bereits und wurden nicht erweitert. Die bestehenden Grenzen in 8.2 genügen.

Keine Änderung an Code, Tests, historischen Läufen, Grafiken/Draw.io, Bibliografie oder Hauptdatei/Universitätsformatierung. Bestehende Labels, Zitate und Grafikeinbindungen bleiben erhalten. Die Tabelle liegt vollständig in der Anhangsdatei; keine zusätzliche Tabellendatei ist zu synchronisieren.

## 6. Abnahme und nächster Schritt

Alle gezielten Feldvergleiche stimmen mit den beschriebenen Kriterien überein. Die statische TeX-Prüfung bestätigt ausgeglichene Klammern/Umgebungen, erhaltene Labels/Zitate und vorhandene Verweisziele. Kein lokaler TeX-Compiler verfügbar: Der neue Satz, insbesondere die Gate-Tabelle, muss nach dem Overleaf-Sync im PDF geprüft werden. Dies ist keine vollständige PDF-Abnahme.

Zum Umsetzungszeitpunkt folgten Sync und PDF-Abnahme; deren Ergebnis steht im folgenden Nachtrag. Keine neue Messkampagne aus diesem Paket ableiten.


## 7. PDF-Abnahme und Gegenprüfung der Kollegenanmerkungen

Geprüft am 15.09.2026: `Master_Thesis_Agentic_SDLC-aktuell.pdf`, 1.314.714 Bytes, 131 physische Seiten. Haupttext S. 1–98, Anhang A beginnt auf S. 99. SHA-256: `514d8a97659640e041a7d8ce9c1129ecb6d2961693b45258c21fc5d2026aab8f`.

Textprüfung und visuelle Prüfung der relevanten Seiten bestätigen die sechs synchronisierten Dateien:

| Stelle | PDF-Seite (gedruckte Nummer) | Abnahme |
|---|---|---|
| 4.6 | 36 | Menschliche Festlegung aus dem erweiterten Nutzungsziel hergeleitet; kein rückwirkend erfundener Versuch |
| 5.6 | 53–55 | Zweck, vorgelegte Informationen, F1/REQ-42, deterministische DEC-Zusammenstellung, typabhängige Begründungspflicht, Abgrenzungen und bekannte Grenzen vorhanden |
| 7.4 | 84 | Entscheidungshoheit als Prüfgegenstand und Trennung von menschlicher Urteilsqualität übernommen |
| 7.5 | 88–89 | Datierter Belegabgleich, acht/vier, gespeicherte Ablehnungen, unveränderte REQ-42 sowie menschlicher F3-Text und Experimentreichweite vorhanden |
| 8.1 | 96 | Governance-Beitrag an F1/F3 und Kontrolltests angeschlossen; keine behauptete durchschnittliche Fehlerreduktion |
| Anhang C | 105–106 | Hauptkettenumfang, zehn Zeilen und Entscheidungsspielräume korrekt übernommen |

Die Verweise in den ergänzten Passagen sind aufgelöst; kein `??`-Treffer im extrahierten PDF-Text. Die Aussagen des Pakets sind im geprüften Umfang inhaltlich abgenommen. Die älteren vier Redaktionskorrekturen und 6.4/6.6 waren bereits separat abgenommen; sie sind keine sechs erneut offenen Sync-Dateien. Es entsteht keine neue Zwölf-Dateien-Liste.

**Satzrest B-12:** Anhang C trennt Einleitung auf S. 105 und vollständige Tabelle auf S. 106; dadurch entsteht viel Leerraum auf der Einleitungsseite. Die Tabelle ist vollständig lesbar, hat aber ungünstige Umbrüche/lange Bezeichnungen und weite Wortabstände, besonders bei der externen Freigabe. Im abschließenden Satzdurchgang kompakt und möglichst gemeinsam anordnen; keine globale Universitätsformatierung ändern. Die inhaltliche B-56-Abnahme ist davon getrennt.

**Anmerkung REQ-42:** Der Fall steht bereits in 5.6 auf S. 54 (Ablehnungsgrund und drohender Verlust) und 7.5 auf S. 88 (unveränderter gespeicherter Bestand). Die Behauptung, er stehe nur im Bericht, trifft auf dieses PDF nicht zu. Ein kurzer Begründungsauszug direkt in 7.5 könnte den Fall beim isolierten Lesen leichter verständlich machen. Das ist eine begrenzte redaktionelle Ergänzung, kein fehlender Grundnachweis. Zusätzlicher Originalbeleg: `07-ingest/ingestion-gate-report.json` des F1-Laufs hat `pass:true`, keine Fehler und vier Warnungen zu anderen Aspekten; die bedrohte Inhaltspräzisierung wurde nicht als Fehler zurückgewiesen. Korrekte Formulierung: bestandene formale Planprüfung, nicht pauschal fachlich korrekte Operation. Der drohende Verlust betrifft den aktuellen Anforderungstext; die frühere Fassung würde durch REFINE historisiert. Nichtverlust ist tatsächlich beobachtet, der unautorisierte Ersatz wurde nicht als neuer Gegenversuch ausgeführt. „Einziger“ bzw. „stärkster“ Beleg ist keine abgesicherte Rangfolge.

**Anmerkung REJ:** Die gespeicherten Begründungen stehen bereits in 7.5. Abschnitt 5.5 beschreibt zusätzlich die Abfragbarkeit früherer Ablehnungen und ihre Rolle bei erneuter Einordnung. Ein kurzer Anschluss an die Bilanzierung nicht übernommener Inhalte wäre sinnvoll: Als Gestaltungsprinzip bleibt auch sichtbar, was nicht übernommen wurde und aus welchem Grund. Die Mechanismen sind verschieden: Ledger-Hinweise betreffen Quellenverarbeitung, REJ-Vermerke konkrete menschliche Übernahmeentscheidungen. Weder dieselbe Vollständigkeit noch derselbe Autorisierungsstatus folgt aus der Analogie.

**Empfehlung für den nächsten Leserdurchgang:** Gegebenenfalls in 7.5 den bereits dokumentierten Ablehnungsgrund kurz anführen und bei den REJ-Vermerken den Vorlagetext als Gegenstand mitnennen. Keine weitere Auswertung, keine Wiederholung der sechs schon synchronisierten Dateien. In dieser Abnahme wurden keine TeX-Dateien geändert. Danach gemäß Phase 10 Material-/Standabschluss und Leser-/Satz-/Grafikprüfung fortführen; B-50 und B-05 bleiben offen.
