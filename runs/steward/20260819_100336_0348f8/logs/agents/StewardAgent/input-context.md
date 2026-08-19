# Input Context — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 11
- **Roles:** `assistant=5`, `tool=3`, `user=3`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 15
- **Roles:** `assistant=7`, `tool=4`, `user=4`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 21
- **Roles:** `assistant=10`, `tool=6`, `user=5`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 27
- **Roles:** `assistant=13`, `tool=8`, `user=6`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 29
- **Roles:** `assistant=14`, `tool=8`, `user=7`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 35
- **Roles:** `assistant=17`, `tool=10`, `user=8`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 39
- **Roles:** `assistant=19`, `tool=11`, `user=9`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 43
- **Roles:** `assistant=21`, `tool=11`, `user=11`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 55
- **Roles:** `assistant=27`, `tool=15`, `user=13`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 59
- **Roles:** `assistant=29`, `tool=16`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 61
- **Roles:** `assistant=30`, `tool=16`, `user=15`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
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

## Chat Iteration 15 — StewardAgent

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
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

## Chat Iteration 16 — StewardAgent

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
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

## Chat Iteration 17 — StewardAgent

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |

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

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
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

## Chat Iteration 20 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 89
- **Roles:** `assistant=44`, `tool=23`, `user=22`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |

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

## Chat Iteration 21 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 91
- **Roles:** `assistant=45`, `tool=23`, `user=23`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |

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

## Chat Iteration 22 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 95
- **Roles:** `assistant=47`, `tool=24`, `user=24`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |

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

## Chat Iteration 23 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 101
- **Roles:** `assistant=50`, `tool=26`, `user=25`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |

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

## Chat Iteration 24 — StewardAgent

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |

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

## Chat Iteration 25 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 107
- **Roles:** `assistant=53`, `tool=27`, `user=27`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |

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

## Chat Iteration 26 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 111
- **Roles:** `assistant=55`, `tool=28`, `user=28`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |
| 107 | assistant | StewardAgent | ja | - | 0 |
| 108 | tool | StewardAgent | - | ja | 111 |
| 109 | assistant | StewardAgent | - | - | 728 |
| 110 | user | - | - | - | 12 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `open_gate_ui` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |
| #108 | StewardAgent | 111 Zeichen |

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

## Chat Iteration 27 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 115
- **Roles:** `assistant=57`, `tool=29`, `user=29`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |
| 107 | assistant | StewardAgent | ja | - | 0 |
| 108 | tool | StewardAgent | - | ja | 111 |
| 109 | assistant | StewardAgent | - | - | 728 |
| 110 | user | - | - | - | 12 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1245 |
| 113 | assistant | StewardAgent | - | - | 838 |
| 114 | user | - | - | - | 28 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `open_gate_ui` | - |
| #111 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |
| #108 | StewardAgent | 111 Zeichen |
| #112 | StewardAgent | 1245 Zeichen |

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

## Chat Iteration 28 — StewardAgent

- **Run:** `20260819_100336_0348f8`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |
| 107 | assistant | StewardAgent | ja | - | 0 |
| 108 | tool | StewardAgent | - | ja | 111 |
| 109 | assistant | StewardAgent | - | - | 728 |
| 110 | user | - | - | - | 12 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1245 |
| 113 | assistant | StewardAgent | - | - | 838 |
| 114 | user | - | - | - | 28 |
| 115 | assistant | StewardAgent | ja | - | 0 |
| 116 | tool | StewardAgent | - | ja | 1522 |
| 117 | assistant | StewardAgent | - | - | 2444 |
| 118 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `open_gate_ui` | - |
| #111 | StewardAgent | `get_run_status` | - |
| #115 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |
| #108 | StewardAgent | 111 Zeichen |
| #112 | StewardAgent | 1245 Zeichen |
| #116 | StewardAgent | 1522 Zeichen |

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

## Chat Iteration 29 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 121
- **Roles:** `assistant=60`, `tool=30`, `user=31`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |
| 107 | assistant | StewardAgent | ja | - | 0 |
| 108 | tool | StewardAgent | - | ja | 111 |
| 109 | assistant | StewardAgent | - | - | 728 |
| 110 | user | - | - | - | 12 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1245 |
| 113 | assistant | StewardAgent | - | - | 838 |
| 114 | user | - | - | - | 28 |
| 115 | assistant | StewardAgent | ja | - | 0 |
| 116 | tool | StewardAgent | - | ja | 1522 |
| 117 | assistant | StewardAgent | - | - | 2444 |
| 118 | user | - | - | - | 10 |
| 119 | assistant | StewardAgent | - | - | 278 |
| 120 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `open_gate_ui` | - |
| #111 | StewardAgent | `get_run_status` | - |
| #115 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |
| #108 | StewardAgent | 111 Zeichen |
| #112 | StewardAgent | 1245 Zeichen |
| #116 | StewardAgent | 1522 Zeichen |
| #120 | - | 0 Zeichen |

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

## Chat Iteration 30 — StewardAgent

- **Run:** `20260819_100336_0348f8`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 129
- **Roles:** `assistant=64`, `tool=33`, `user=32`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 701 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 36 |
| 5 | assistant | StewardAgent | - | - | 1060 |
| 6 | user | - | - | - | 514 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 8391 |
| 9 | assistant | StewardAgent | - | - | 3878 |
| 10 | user | - | - | - | 60 |
| 11 | assistant | StewardAgent | ja | - | 224 |
| 12 | tool | StewardAgent | - | ja | 231 |
| 13 | assistant | StewardAgent | - | - | 184 |
| 14 | user | - | - | ja | 0 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 222 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 728 |
| 19 | assistant | StewardAgent | - | - | 648 |
| 20 | user | - | - | - | 32 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 1114 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 4282 |
| 25 | assistant | StewardAgent | - | - | 6252 |
| 26 | user | - | - | - | 176 |
| 27 | assistant | StewardAgent | - | - | 302 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 386 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1253 |
| 33 | assistant | StewardAgent | - | - | 910 |
| 34 | user | - | - | - | 78 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 2108 |
| 37 | assistant | StewardAgent | - | - | 3444 |
| 38 | user | - | - | - | 22 |
| 39 | assistant | StewardAgent | - | - | 258 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 397 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 639 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 979 |
| 49 | assistant | StewardAgent | - | - | 1346 |
| 50 | user | - | - | - | 40 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 778 |
| 53 | assistant | StewardAgent | - | - | 1338 |
| 54 | user | - | - | - | 336 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 6511 |
| 57 | assistant | StewardAgent | - | - | 3136 |
| 58 | user | - | - | - | 4 |
| 59 | assistant | StewardAgent | - | - | 954 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 231 |
| 63 | assistant | StewardAgent | - | - | 184 |
| 64 | user | - | - | - | 6 |
| 65 | assistant | StewardAgent | - | - | 0 |
| 66 | user | - | - | ja | 0 |
| 67 | assistant | StewardAgent | ja | - | 0 |
| 68 | tool | StewardAgent | - | ja | 222 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 728 |
| 71 | assistant | StewardAgent | - | - | 648 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | ja | - | 0 |
| 74 | tool | StewardAgent | - | ja | 1114 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 2236 |
| 77 | assistant | StewardAgent | - | - | 3274 |
| 78 | user | - | - | - | 10 |
| 79 | assistant | StewardAgent | - | - | 296 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | - | - | 0 |
| 82 | user | - | - | ja | 0 |
| 83 | assistant | StewardAgent | ja | - | 0 |
| 84 | tool | StewardAgent | - | ja | 386 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 1253 |
| 87 | assistant | StewardAgent | - | - | 838 |
| 88 | user | - | - | - | 28 |
| 89 | assistant | StewardAgent | - | - | 248 |
| 90 | user | - | - | - | 4 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 2108 |
| 93 | assistant | StewardAgent | - | - | 406 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 397 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 807 |
| 99 | assistant | StewardAgent | - | - | 840 |
| 100 | user | - | - | - | 8 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 1002 |
| 103 | assistant | StewardAgent | - | - | 666 |
| 104 | user | - | - | - | 30 |
| 105 | assistant | StewardAgent | - | - | 268 |
| 106 | user | - | - | ja | 0 |
| 107 | assistant | StewardAgent | ja | - | 0 |
| 108 | tool | StewardAgent | - | ja | 111 |
| 109 | assistant | StewardAgent | - | - | 728 |
| 110 | user | - | - | - | 12 |
| 111 | assistant | StewardAgent | ja | - | 0 |
| 112 | tool | StewardAgent | - | ja | 1245 |
| 113 | assistant | StewardAgent | - | - | 838 |
| 114 | user | - | - | - | 28 |
| 115 | assistant | StewardAgent | ja | - | 0 |
| 116 | tool | StewardAgent | - | ja | 1522 |
| 117 | assistant | StewardAgent | - | - | 2444 |
| 118 | user | - | - | - | 10 |
| 119 | assistant | StewardAgent | - | - | 278 |
| 120 | user | - | - | ja | 0 |
| 121 | assistant | StewardAgent | ja | - | 0 |
| 122 | tool | StewardAgent | - | ja | 396 |
| 123 | assistant | StewardAgent | ja | - | 0 |
| 124 | tool | StewardAgent | - | ja | 639 |
| 125 | assistant | StewardAgent | ja | - | 0 |
| 126 | tool | StewardAgent | - | ja | 3449 |
| 127 | assistant | StewardAgent | - | - | 1850 |
| 128 | user | - | - | - | 142 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `get_core_overview` | - |
| #3 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `list_paused_runs` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #7 | StewardAgent | `list_core_items` | - |
| #11 | StewardAgent | `save_author_statements` | - |
| #15 | StewardAgent | `run_pipeline_from_delta` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #21 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `get_paused_gate` | - |
| #43 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #47 | StewardAgent | `read_run_report` | - |
| #47 | StewardAgent | `list_core_items` | - |
| #51 | StewardAgent | `get_core_overview` | - |
| #51 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `list_paused_runs` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `search_rejections` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #55 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `save_author_statements` | - |
| #67 | StewardAgent | `run_pipeline_from_delta` | - |
| #69 | StewardAgent | `get_run_status` | - |
| #73 | StewardAgent | `get_run_status` | - |
| #75 | StewardAgent | `get_paused_gate` | - |
| #83 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #85 | StewardAgent | `get_run_status` | - |
| #91 | StewardAgent | `get_paused_gate` | - |
| #95 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `open_gate_ui` | - |
| #111 | StewardAgent | `get_run_status` | - |
| #115 | StewardAgent | `get_paused_gate` | - |
| #121 | StewardAgent | `submit_paused_gate_decisions` | - |
| #123 | StewardAgent | `get_run_status` | - |
| #125 | StewardAgent | `read_run_report` | - |
| #125 | StewardAgent | `list_core_items` | - |
| #125 | StewardAgent | `get_core_overview` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 701 Zeichen |
| #4 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 36 Zeichen |
| #8 | StewardAgent | 1194 Zeichen |
| #8 | StewardAgent | 363 Zeichen |
| #8 | StewardAgent | 3402 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 64 Zeichen |
| #8 | StewardAgent | 347 Zeichen |
| #8 | StewardAgent | 2793 Zeichen |
| #12 | StewardAgent | 231 Zeichen |
| #14 | - | 0 Zeichen |
| #16 | StewardAgent | 222 Zeichen |
| #18 | StewardAgent | 728 Zeichen |
| #22 | StewardAgent | 1114 Zeichen |
| #24 | StewardAgent | 4282 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 386 Zeichen |
| #32 | StewardAgent | 1253 Zeichen |
| #36 | StewardAgent | 2108 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 397 Zeichen |
| #46 | StewardAgent | 639 Zeichen |
| #48 | StewardAgent | 683 Zeichen |
| #48 | StewardAgent | 296 Zeichen |
| #52 | StewardAgent | 742 Zeichen |
| #52 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 36 Zeichen |
| #56 | StewardAgent | 4342 Zeichen |
| #56 | StewardAgent | 1739 Zeichen |
| #56 | StewardAgent | 330 Zeichen |
| #56 | StewardAgent | 64 Zeichen |
| #62 | StewardAgent | 231 Zeichen |
| #66 | - | 0 Zeichen |
| #68 | StewardAgent | 222 Zeichen |
| #70 | StewardAgent | 728 Zeichen |
| #74 | StewardAgent | 1114 Zeichen |
| #76 | StewardAgent | 2236 Zeichen |
| #82 | - | 0 Zeichen |
| #84 | StewardAgent | 386 Zeichen |
| #86 | StewardAgent | 1253 Zeichen |
| #92 | StewardAgent | 2108 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 397 Zeichen |
| #98 | StewardAgent | 807 Zeichen |
| #102 | StewardAgent | 1002 Zeichen |
| #106 | - | 0 Zeichen |
| #108 | StewardAgent | 111 Zeichen |
| #112 | StewardAgent | 1245 Zeichen |
| #116 | StewardAgent | 1522 Zeichen |
| #120 | - | 0 Zeichen |
| #122 | StewardAgent | 396 Zeichen |
| #124 | StewardAgent | 639 Zeichen |
| #126 | StewardAgent | 2190 Zeichen |
| #126 | StewardAgent | 517 Zeichen |
| #126 | StewardAgent | 742 Zeichen |

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

