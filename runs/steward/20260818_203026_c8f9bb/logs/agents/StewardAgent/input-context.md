# Input Context — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 51
- **Roles:** `assistant=25`, `tool=14`, `user=12`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 53
- **Roles:** `assistant=26`, `tool=14`, `user=13`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 57
- **Roles:** `assistant=28`, `tool=15`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 59
- **Roles:** `assistant=29`, `tool=15`, `user=15`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 65
- **Roles:** `assistant=32`, `tool=17`, `user=16`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 67
- **Roles:** `assistant=33`, `tool=17`, `user=17`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 73
- **Roles:** `assistant=36`, `tool=19`, `user=18`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 79
- **Roles:** `assistant=39`, `tool=21`, `user=19`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 81
- **Roles:** `assistant=40`, `tool=21`, `user=20`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |

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

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 83
- **Roles:** `assistant=41`, `tool=21`, `user=21`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |

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

## Chat Iteration 11 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 91
- **Roles:** `assistant=45`, `tool=24`, `user=22`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |

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

## Chat Iteration 12 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 93
- **Roles:** `assistant=46`, `tool=24`, `user=23`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |

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

## Chat Iteration 13 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 99
- **Roles:** `assistant=49`, `tool=26`, `user=24`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |

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

## Chat Iteration 14 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 103
- **Roles:** `assistant=51`, `tool=27`, `user=25`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |

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

## Chat Iteration 15 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 105
- **Roles:** `assistant=52`, `tool=27`, `user=26`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |
| 103 | assistant | StewardAgent | - | - | 0 |
| 104 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |
| #104 | - | 0 Zeichen |

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

## Chat Iteration 16 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 109
- **Roles:** `assistant=54`, `tool=28`, `user=27`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |
| 103 | assistant | StewardAgent | - | - | 0 |
| 104 | user | - | - | ja | 0 |
| 105 | assistant | StewardAgent | ja | - | 0 |
| 106 | tool | StewardAgent | - | ja | 111 |
| 107 | assistant | StewardAgent | - | - | 812 |
| 108 | user | - | - | - | 12 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |
| #105 | StewardAgent | `open_gate_ui` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |
| #104 | - | 0 Zeichen |
| #106 | StewardAgent | 111 Zeichen |

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

## Chat Iteration 17 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 115
- **Roles:** `assistant=57`, `tool=30`, `user=28`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |
| 103 | assistant | StewardAgent | - | - | 0 |
| 104 | user | - | - | ja | 0 |
| 105 | assistant | StewardAgent | ja | - | 0 |
| 106 | tool | StewardAgent | - | ja | 111 |
| 107 | assistant | StewardAgent | - | - | 812 |
| 108 | user | - | - | - | 12 |
| 109 | assistant | StewardAgent | ja | - | 0 |
| 110 | tool | StewardAgent | - | ja | 1246 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1378 |
| 113 | assistant | StewardAgent | - | - | 2518 |
| 114 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |
| #105 | StewardAgent | `open_gate_ui` | - |
| #109 | StewardAgent | `get_run_status` | - |
| #111 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |
| #104 | - | 0 Zeichen |
| #106 | StewardAgent | 111 Zeichen |
| #110 | StewardAgent | 1246 Zeichen |
| #112 | StewardAgent | 1378 Zeichen |

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

## Chat Iteration 18 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 117
- **Roles:** `assistant=58`, `tool=30`, `user=29`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |
| 103 | assistant | StewardAgent | - | - | 0 |
| 104 | user | - | - | ja | 0 |
| 105 | assistant | StewardAgent | ja | - | 0 |
| 106 | tool | StewardAgent | - | ja | 111 |
| 107 | assistant | StewardAgent | - | - | 812 |
| 108 | user | - | - | - | 12 |
| 109 | assistant | StewardAgent | ja | - | 0 |
| 110 | tool | StewardAgent | - | ja | 1246 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1378 |
| 113 | assistant | StewardAgent | - | - | 2518 |
| 114 | user | - | - | - | 10 |
| 115 | assistant | StewardAgent | - | - | 330 |
| 116 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |
| #105 | StewardAgent | `open_gate_ui` | - |
| #109 | StewardAgent | `get_run_status` | - |
| #111 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |
| #104 | - | 0 Zeichen |
| #106 | StewardAgent | 111 Zeichen |
| #110 | StewardAgent | 1246 Zeichen |
| #112 | StewardAgent | 1378 Zeichen |

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

## Chat Iteration 19 — StewardAgent

- **Run:** `20260818_203026_c8f9bb`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 119
- **Roles:** `assistant=59`, `tool=30`, `user=30`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 162 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1224 |
| 3 | assistant | StewardAgent | - | - | 2224 |
| 4 | user | - | - | - | 234 |
| 5 | assistant | StewardAgent | - | - | 718 |
| 6 | user | - | - | - | 14 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 36 |
| 9 | assistant | StewardAgent | ja | - | 200 |
| 10 | tool | StewardAgent | - | ja | 231 |
| 11 | assistant | StewardAgent | - | - | 212 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 222 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 894 |
| 18 | user | - | - | - | 20 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 1114 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1450 |
| 23 | assistant | StewardAgent | - | - | 3382 |
| 24 | user | - | - | - | 258 |
| 25 | assistant | StewardAgent | - | - | 292 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 1253 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 1924 |
| 34 | user | - | - | - | 30 |
| 35 | assistant | StewardAgent | - | - | 300 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 639 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 1604 |
| 46 | user | - | - | - | 262 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 295 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 258 |
| 51 | assistant | StewardAgent | - | - | 1296 |
| 52 | user | - | - | - | 192 |
| 53 | assistant | StewardAgent | ja | - | 264 |
| 54 | tool | StewardAgent | - | ja | 3418 |
| 55 | assistant | StewardAgent | - | - | 1930 |
| 56 | user | - | - | - | 220 |
| 57 | assistant | StewardAgent | - | - | 1110 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | ja | - | 0 |
| 60 | tool | StewardAgent | - | ja | 36 |
| 61 | assistant | StewardAgent | ja | - | 208 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 218 |
| 64 | user | - | - | - | 4 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 796 |
| 72 | user | - | - | - | 16 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 989 |
| 77 | assistant | StewardAgent | - | - | 2506 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 284 |
| 80 | user | - | - | - | 4 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | ja | - | 0 |
| 88 | tool | StewardAgent | - | ja | 1233 |
| 89 | assistant | StewardAgent | - | - | 2120 |
| 90 | user | - | - | - | 20 |
| 91 | assistant | StewardAgent | - | - | 300 |
| 92 | user | - | - | ja | 0 |
| 93 | assistant | StewardAgent | ja | - | 0 |
| 94 | tool | StewardAgent | - | ja | 397 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 807 |
| 97 | assistant | StewardAgent | - | - | 718 |
| 98 | user | - | - | - | 36 |
| 99 | assistant | StewardAgent | ja | - | 0 |
| 100 | tool | StewardAgent | - | ja | 1002 |
| 101 | assistant | StewardAgent | - | - | 790 |
| 102 | user | - | - | - | 6 |
| 103 | assistant | StewardAgent | - | - | 0 |
| 104 | user | - | - | ja | 0 |
| 105 | assistant | StewardAgent | ja | - | 0 |
| 106 | tool | StewardAgent | - | ja | 111 |
| 107 | assistant | StewardAgent | - | - | 812 |
| 108 | user | - | - | - | 12 |
| 109 | assistant | StewardAgent | ja | - | 0 |
| 110 | tool | StewardAgent | - | ja | 1246 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1378 |
| 113 | assistant | StewardAgent | - | - | 2518 |
| 114 | user | - | - | - | 10 |
| 115 | assistant | StewardAgent | - | - | 330 |
| 116 | user | - | - | - | 4 |
| 117 | assistant | StewardAgent | - | - | 0 |
| 118 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #9 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `search_rejections` | - |
| #53 | StewardAgent | `search_rejections` | - |
| #59 | StewardAgent | `list_paused_runs` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #87 | StewardAgent | `get_paused_gate` | - |
| #93 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #99 | StewardAgent | `get_run_status` | - |
| #105 | StewardAgent | `open_gate_ui` | - |
| #109 | StewardAgent | `get_run_status` | - |
| #111 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 1080 Zeichen |
| #2 | StewardAgent | 80 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #10 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 222 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 1114 Zeichen |
| #22 | StewardAgent | 1450 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1253 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 639 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 103 Zeichen |
| #54 | StewardAgent | 3418 Zeichen |
| #60 | StewardAgent | 36 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 989 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #88 | StewardAgent | 1233 Zeichen |
| #92 | - | 0 Zeichen |
| #94 | StewardAgent | 397 Zeichen |
| #96 | StewardAgent | 807 Zeichen |
| #100 | StewardAgent | 1002 Zeichen |
| #104 | - | 0 Zeichen |
| #106 | StewardAgent | 111 Zeichen |
| #110 | StewardAgent | 1246 Zeichen |
| #112 | StewardAgent | 1378 Zeichen |
| #118 | - | 0 Zeichen |

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

