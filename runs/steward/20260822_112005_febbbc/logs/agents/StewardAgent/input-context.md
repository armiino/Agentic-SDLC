# Input Context — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 37
- **Roles:** `assistant=18`, `tool=10`, `user=9`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 2 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 41
- **Roles:** `assistant=20`, `tool=11`, `user=10`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 3 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 45
- **Roles:** `assistant=22`, `tool=12`, `user=11`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 4 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 47
- **Roles:** `assistant=23`, `tool=12`, `user=12`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 5 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 49
- **Roles:** `assistant=24`, `tool=12`, `user=13`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 6 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 51
- **Roles:** `assistant=25`, `tool=12`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |
| 49 | assistant | StewardAgent | - | - | 0 |
| 50 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |
| #50 | - | 0 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 7 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 57
- **Roles:** `assistant=28`, `tool=14`, `user=15`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |
| 49 | assistant | StewardAgent | - | - | 0 |
| 50 | user | - | - | ja | 0 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 398 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 1246 |
| 55 | assistant | StewardAgent | - | - | 702 |
| 56 | user | - | - | - | 48 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |
| #51 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #53 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |
| #50 | - | 0 Zeichen |
| #52 | StewardAgent | 398 Zeichen |
| #54 | StewardAgent | 1246 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 8 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 61
- **Roles:** `assistant=30`, `tool=15`, `user=16`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |
| 49 | assistant | StewardAgent | - | - | 0 |
| 50 | user | - | - | ja | 0 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 398 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 1246 |
| 55 | assistant | StewardAgent | - | - | 702 |
| 56 | user | - | - | - | 48 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1701 |
| 59 | assistant | StewardAgent | - | - | 2576 |
| 60 | user | - | - | - | 28 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |
| #51 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |
| #50 | - | 0 Zeichen |
| #52 | StewardAgent | 398 Zeichen |
| #54 | StewardAgent | 1246 Zeichen |
| #58 | StewardAgent | 1701 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 9 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 63
- **Roles:** `assistant=31`, `tool=15`, `user=17`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |
| 49 | assistant | StewardAgent | - | - | 0 |
| 50 | user | - | - | ja | 0 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 398 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 1246 |
| 55 | assistant | StewardAgent | - | - | 702 |
| 56 | user | - | - | - | 48 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1701 |
| 59 | assistant | StewardAgent | - | - | 2576 |
| 60 | user | - | - | - | 28 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |
| #51 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |
| #50 | - | 0 Zeichen |
| #52 | StewardAgent | 398 Zeichen |
| #54 | StewardAgent | 1246 Zeichen |
| #58 | StewardAgent | 1701 Zeichen |
| #62 | - | 0 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

## Chat Iteration 10 — StewardAgent

- **Run:** `20260822_112005_febbbc`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 69
- **Roles:** `assistant=34`, `tool=17`, `user=18`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |
| 7 | assistant | StewardAgent | - | - | 772 |
| 8 | user | - | - | - | 4 |
| 9 | assistant | StewardAgent | ja | - | 662 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 788 |
| 18 | user | - | - | - | 68 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1939 |
| 23 | assistant | StewardAgent | - | - | 0 |
| 24 | user | - | - | ja | 0 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 386 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 1253 |
| 29 | assistant | StewardAgent | - | - | 970 |
| 30 | user | - | - | - | 58 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 5358 |
| 33 | assistant | StewardAgent | - | - | 1628 |
| 34 | user | - | - | - | 80 |
| 35 | assistant | StewardAgent | - | - | 2038 |
| 36 | user | - | - | - | 40 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 1373 |
| 39 | assistant | StewardAgent | - | - | 864 |
| 40 | user | - | - | - | 46 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 10168 |
| 43 | assistant | StewardAgent | - | - | 4194 |
| 44 | user | - | - | - | 272 |
| 45 | assistant | StewardAgent | - | - | 2812 |
| 46 | user | - | - | - | 100 |
| 47 | assistant | StewardAgent | - | - | 1462 |
| 48 | user | - | - | - | 36 |
| 49 | assistant | StewardAgent | - | - | 0 |
| 50 | user | - | - | ja | 0 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 398 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 1246 |
| 55 | assistant | StewardAgent | - | - | 702 |
| 56 | user | - | - | - | 48 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1701 |
| 59 | assistant | StewardAgent | - | - | 2576 |
| 60 | user | - | - | - | 28 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 396 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 902 |
| 67 | assistant | StewardAgent | - | - | 688 |
| 68 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #25 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `list_paused_runs` | - |
| #41 | StewardAgent | `get_paused_gate` | - |
| #51 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `get_paused_gate` | - |
| #63 | StewardAgent | `submit_paused_gate_decisions` | - |
| #65 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1939 Zeichen |
| #24 | - | 0 Zeichen |
| #26 | StewardAgent | 386 Zeichen |
| #28 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 5358 Zeichen |
| #38 | StewardAgent | 1373 Zeichen |
| #42 | StewardAgent | 10168 Zeichen |
| #50 | - | 0 Zeichen |
| #52 | StewardAgent | 398 Zeichen |
| #54 | StewardAgent | 1246 Zeichen |
| #58 | StewardAgent | 1701 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 396 Zeichen |
| #66 | StewardAgent | 902 Zeichen |

## Abgeleitete Signale

> Aus den Fakten strukturell abgeleitet — kein Regex.
> Einschränkung: 'Read im Kontext' bedeutet der FunctionCallContent ist sichtbar.
> Ob der fs_read erfolgreich war und das Ergebnis korrekt ist → tool-calls.jsonl prüfen.

- **Transkript-Read im Kontext:** — NEIN
- **Context.md-Read im Kontext:** — NEIN

## Was diese Analyse nicht beweist

- **Nicht beweisbar:** Ob das Modell sichtbare Inhalte intern verarbeitet oder gewichtet hat.
  Das ist die fundamentale LLM-Black-Box-Grenze.
- **Nicht beweisbar:** Ob ein Tool-Result den erwarteten Inhalt enthält.
  Für Tool-Erfolgsverifikation: `tool-calls.jsonl` dieses Agenten prüfen.

---

