# W2: Tokenkorrektur und Nachrechnung vom 10.09.2026

Dieser Nachtrag berichtigt Tabelle 7.10 und die davon abhängigen Verhältnisse in Kapitel 7.3.4/7.7.1. Er enthält keine neue Systemausführung oder semantische Neubewertung. Ausgangspunkt ist die PDF vom 10.09.2026 mit SHA-256 `60a4aefe492d9d5df865500672257f4da4f80fb2c9b6db9bdb0d07d32865a6e9`.

Maßgeblicher Tokenstand: [token-usage-correction-20260910.json](token-usage-correction-20260910.json). Die ursprünglichen Logs und früheren Urteilsblätter werden durch diesen Nachtrag nicht geändert. Ihre dortigen Kosten-Blöcke sind für die Tokenangaben der Hauptläufe durch diesen Nachtrag überholt.

| Konfiguration und Fall | Lauf | Input-Tokens | Output-Tokens | Darin enthaltene Cache-Read-Input-Tokens |
|---|---|---:|---:|---:|
| Ledger, Stressfall | 20260905_164650_8189ea | **135.592** | 41.133 | 46.592 |
| Ledger, Treue-Fall | 20260905_164206_e30646 | 26.731 | 13.524 | 0 |
| F, Stressfall | 20260906_143052_6540e2 | 105.166 | 19.555 | 62.976 |
| F, Treue-Fall | 20260906_141518_12d046 | 23.040 | 5.708 | 11.776 |

Der bisherige Inputwert des Ledger-Stresslaufs (182.184) entspricht exakt 135.592 + 46.592. Die Cache-Untermenge erklärt somit den falschen Gesamtwert; der historische Rechenweg wurde nicht rekonstruiert. Die übrigen sieben Werte der Input-/Output-Tabelle werden bestätigt.

## Erhebungsregel und Reichweite

- Die Dateien `logs/otel-metrics.jsonl` der vier Hauptläufe enthalten kumulative Histogrammwerte. Je beobachteter Tagkombination wird der letzte Wert von `gen_ai.client.token.usage` verwendet. Periodische Exporte werden nicht aufaddiert. Die Nachrechnung prüft die Monotonie der Zähler und stoppt bei einem erkannten Reset.
- Gegenprüfung: Summe von `gen_ai.usage.input_tokens` und `gen_ai.usage.output_tokens` über die unterschiedlichen Chat-Spans in `logs/otel-traces.jsonl`. Metrikzählung und Spananzahl stimmen überein: 12/7 Chat-Aufrufe für die beiden Ledger-Läufe, jeweils 4 für F.
- `gen_ai.usage.cache_read.input_tokens` ist ein Detail des Inputverbrauchs und wird nicht zum Gesamtinput addiert. Bei F wird der übergeordnete `orchestrate_tools`-Gesamtwert ebenfalls nicht zusätzlich zu seinen Chat-Aufrufen gezählt. Die F-Gesamtwerte stimmen zudem mit den jeweiligen `metrics.json` überein.
- Die Bedeutung der Cache-Untermenge entspricht der [OpenTelemetry-Attributdefinition](https://opentelemetry.io/docs/specs/semconv/registry/attributes/gen-ai/) und dem Usage-Format der [OpenRouter-Dokumentation](https://openrouter.ai/docs/guides/best-practices/prompt-caching), im Review am 10.09.2026 geprüft. Hieraus wird kein Geldkostenvergleich abgeleitet.
- Metriken und Traces sind zwei Darstellungen derselben Instrumentierung. Ihre Übereinstimmung beweist weder vollständige Instrumentierung historischer Pfade noch die Richtigkeit einer Anbieterrechnung. Dieses Dokument beschreibt den protokollierten Modellverbrauch innerhalb der Messgrenze der vier Hauptläufe.

## Korrigierte Vergleichsverhältnisse im Stressfall

| Verhältnis | Nachrechnung | Darstellung im Text |
|---|---:|---|
| Ledger/F Input | 135.592 / 105.166 = 1,289314… | rund 1,3-fach |
| F/Ledger Input | 105.166 / 135.592 = 0,775606… | rund 78 % |
| Ledger/F Output | 41.133 / 19.555 = 2,103452… | rund 2,1-fach, unverändert |
| F/Ledger Output | 19.555 / 41.133 = 0,475409… | rund 48 %, unverändert |

## Reproduktion

Aus dem Repository-Verzeichnis, mit Python 3 und ohne weitere Pakete:

```sh
python3 thesis-evidence/w2/recompute-token-usage.py
```

Das Skript gibt JSON aus. Ein anderer Checkout kann mit `--repo /pfad/zum/repository` angegeben werden. `--output /pfad/zu/einer/neuen-datei.json` speichert das Ergebnis ausdrücklich in einer Datei. Ohne diese Option werden keine Dateien geschrieben. Für die Nachrechnung müssen die im JSON mit relativen Pfaden und SHA-256 identifizierten Laufprotokolle vorhanden sein. Das Skript verwendet weder Netzwerkzugriffe noch Modellaufrufe.

Die JSON-Datei enthält den letzten Zähler samt Zeilennummer pro Metrikserie, die einzelnen Chat-Spans, die acht Tokenwerte, die Verhältnisse sowie Hashes aller eingelesenen Quelldateien. Diese Angaben erlauben die Prüfung gegen den archivierten Bestand; ein später abweichender Quelldatei-Hash bezeichnet eine andere Prüfbasis.

## Zuordnung zu den Inhaltsurteilen und zur Laufwahl

Die Tokenkorrektur verändert keine Coverage-Urteile. Für die endgültigen Stressfall-Inhaltsurteile ist [eval/w2-nachreview-konsolidierung.json](eval/w2-nachreview-konsolidierung.json), Konsolidierung vom 08.09., maßgeblich. Die dortigen 109 Zeilen ergeben Ledger 54/29/26 und F 69/23/17 (vollständig/teilweise/nicht enthalten). Die älteren `w2-goldv2-eval-*.json` bleiben Ausgangsbelege vor diesem Nachreview; die beiden Treue-Fall-Urteilsblätter gelten fort. D.1 nennt diese unterschiedlichen Ergebnisstände ausdrücklich.

Die Regel „chronologisch erster vollständiger Lauf“ steht im [Laufmanifest](w2-official-manifest.txt). Das [Messprotokoll vom 07.09.](messprotokoll-kapitel-7.md) entstand nach der Hauptauswertung vom 06.09. und kennzeichnet dies selbst. Ein archivierter Nachweis der zeitlichen Vorabfixierung der Laufwahl wurde im Review nicht gefunden; die betroffenen Dateien besitzen im geprüften lokalen Git-Verlauf keine nachvollziehbare Versionsgeschichte. 7.2 und D.1 beschreiben deshalb die dokumentierte Auswahlregel ohne eine nachgewiesene Vorabregistrierung zu behaupten. Das ist keine Feststellung, die Auswahl sei tatsächlich erst nach Kenntnis der Ergebnisse beschlossen worden.
