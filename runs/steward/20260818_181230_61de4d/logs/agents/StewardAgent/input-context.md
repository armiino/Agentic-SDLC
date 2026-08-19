# Input Context — StewardAgent

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 1
- **Roles:** `user=1`
- **Agent-Namen im Kontext:** keine

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 7
- **Roles:** `assistant=3`, `tool=2`, `user=2`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 17
- **Roles:** `assistant=8`, `tool=5`, `user=4`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 23
- **Roles:** `assistant=11`, `tool=7`, `user=5`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 25
- **Roles:** `assistant=12`, `tool=7`, `user=6`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 27
- **Roles:** `assistant=13`, `tool=7`, `user=7`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 39
- **Roles:** `assistant=19`, `tool=10`, `user=10`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 47
- **Roles:** `assistant=23`, `tool=13`, `user=11`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 63
- **Roles:** `assistant=31`, `tool=17`, `user=15`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 69
- **Roles:** `assistant=34`, `tool=19`, `user=16`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 71
- **Roles:** `assistant=35`, `tool=19`, `user=17`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 75
- **Roles:** `assistant=37`, `tool=19`, `user=19`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 77
- **Roles:** `assistant=38`, `tool=19`, `user=20`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 85
- **Roles:** `assistant=42`, `tool=22`, `user=21`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 87
- **Roles:** `assistant=43`, `tool=22`, `user=22`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 89
- **Roles:** `assistant=44`, `tool=22`, `user=23`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 99
- **Roles:** `assistant=49`, `tool=25`, `user=25`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 101
- **Roles:** `assistant=50`, `tool=25`, `user=26`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 105
- **Roles:** `assistant=52`, `tool=26`, `user=27`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 111 |
| 103 | assistant | StewardAgent | - | - | 896 |
| 104 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `open_gate_ui` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |
| #102 | StewardAgent | 111 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

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
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 111 |
| 103 | assistant | StewardAgent | - | - | 896 |
| 104 | user | - | - | - | 20 |
| 105 | assistant | StewardAgent | ja | - | 198 |
| 106 | tool | StewardAgent | - | ja | 1143 |
| 107 | assistant | StewardAgent | ja | - | 272 |
| 108 | tool | StewardAgent | - | ja | 1755 |
| 109 | assistant | StewardAgent | - | - | 3228 |
| 110 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `open_gate_ui` | - |
| #105 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |
| #102 | StewardAgent | 111 Zeichen |
| #106 | StewardAgent | 1143 Zeichen |
| #108 | StewardAgent | 1755 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 113
- **Roles:** `assistant=56`, `tool=28`, `user=29`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 111 |
| 103 | assistant | StewardAgent | - | - | 896 |
| 104 | user | - | - | - | 20 |
| 105 | assistant | StewardAgent | ja | - | 198 |
| 106 | tool | StewardAgent | - | ja | 1143 |
| 107 | assistant | StewardAgent | ja | - | 272 |
| 108 | tool | StewardAgent | - | ja | 1755 |
| 109 | assistant | StewardAgent | - | - | 3228 |
| 110 | user | - | - | - | 8 |
| 111 | assistant | StewardAgent | - | - | 484 |
| 112 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `open_gate_ui` | - |
| #105 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |
| #102 | StewardAgent | 111 Zeichen |
| #106 | StewardAgent | 1143 Zeichen |
| #108 | StewardAgent | 1755 Zeichen |

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

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 115
- **Roles:** `assistant=57`, `tool=28`, `user=30`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 111 |
| 103 | assistant | StewardAgent | - | - | 896 |
| 104 | user | - | - | - | 20 |
| 105 | assistant | StewardAgent | ja | - | 198 |
| 106 | tool | StewardAgent | - | ja | 1143 |
| 107 | assistant | StewardAgent | ja | - | 272 |
| 108 | tool | StewardAgent | - | ja | 1755 |
| 109 | assistant | StewardAgent | - | - | 3228 |
| 110 | user | - | - | - | 8 |
| 111 | assistant | StewardAgent | - | - | 484 |
| 112 | user | - | - | - | 4 |
| 113 | assistant | StewardAgent | - | - | 240 |
| 114 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `open_gate_ui` | - |
| #105 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |
| #102 | StewardAgent | 111 Zeichen |
| #106 | StewardAgent | 1143 Zeichen |
| #108 | StewardAgent | 1755 Zeichen |
| #114 | - | 0 Zeichen |

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

## Chat Iteration 31 — StewardAgent

- **Run:** `20260818_181230_61de4d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 123
- **Roles:** `assistant=61`, `tool=31`, `user=31`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 322 |
| 1 | assistant | StewardAgent | ja | - | 618 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 248 |
| 4 | tool | StewardAgent | - | ja | 1563 |
| 5 | assistant | StewardAgent | - | - | 4194 |
| 6 | user | - | - | - | 828 |
| 7 | assistant | StewardAgent | ja | - | 248 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 206 |
| 10 | user | - | - | ja | 0 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 222 |
| 13 | assistant | StewardAgent | ja | - | 196 |
| 14 | tool | StewardAgent | - | ja | 728 |
| 15 | assistant | StewardAgent | - | - | 1106 |
| 16 | user | - | - | - | 4 |
| 17 | assistant | StewardAgent | ja | - | 168 |
| 18 | tool | StewardAgent | - | ja | 901 |
| 19 | assistant | StewardAgent | ja | - | 394 |
| 20 | tool | StewardAgent | - | ja | 1280 |
| 21 | assistant | StewardAgent | - | - | 2898 |
| 22 | user | - | - | - | 352 |
| 23 | assistant | StewardAgent | - | - | 1474 |
| 24 | user | - | - | - | 4 |
| 25 | assistant | StewardAgent | - | - | 256 |
| 26 | user | - | - | ja | 0 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 386 |
| 29 | assistant | StewardAgent | ja | - | 158 |
| 30 | tool | StewardAgent | - | ja | 1125 |
| 31 | assistant | StewardAgent | ja | - | 254 |
| 32 | tool | StewardAgent | - | ja | 1233 |
| 33 | assistant | StewardAgent | - | - | 2932 |
| 34 | user | - | - | - | 92 |
| 35 | assistant | StewardAgent | - | - | 278 |
| 36 | user | - | - | - | 4 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 397 |
| 41 | assistant | StewardAgent | ja | - | 172 |
| 42 | tool | StewardAgent | - | ja | 638 |
| 43 | assistant | StewardAgent | ja | - | 308 |
| 44 | tool | StewardAgent | - | ja | 1278 |
| 45 | assistant | StewardAgent | - | - | 2136 |
| 46 | user | - | - | - | 576 |
| 47 | assistant | StewardAgent | ja | - | 254 |
| 48 | tool | StewardAgent | - | ja | 36 |
| 49 | assistant | StewardAgent | - | - | 920 |
| 50 | user | - | - | - | 4 |
| 51 | assistant | StewardAgent | ja | - | 202 |
| 52 | tool | StewardAgent | - | ja | 231 |
| 53 | assistant | StewardAgent | - | - | 388 |
| 54 | user | - | - | - | 4 |
| 55 | assistant | StewardAgent | - | - | 0 |
| 56 | user | - | - | ja | 0 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 222 |
| 59 | assistant | StewardAgent | ja | - | 138 |
| 60 | tool | StewardAgent | - | ja | 727 |
| 61 | assistant | StewardAgent | - | - | 1000 |
| 62 | user | - | - | - | 4 |
| 63 | assistant | StewardAgent | ja | - | 142 |
| 64 | tool | StewardAgent | - | ja | 901 |
| 65 | assistant | StewardAgent | ja | - | 234 |
| 66 | tool | StewardAgent | - | ja | 992 |
| 67 | assistant | StewardAgent | - | - | 2486 |
| 68 | user | - | - | - | 6 |
| 69 | assistant | StewardAgent | - | - | 416 |
| 70 | user | - | - | - | 4 |
| 71 | assistant | StewardAgent | - | - | 290 |
| 72 | user | - | - | - | 20 |
| 73 | assistant | StewardAgent | - | - | 298 |
| 74 | user | - | - | - | 4 |
| 75 | assistant | StewardAgent | - | - | 0 |
| 76 | user | - | - | ja | 0 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 386 |
| 79 | assistant | StewardAgent | ja | - | 160 |
| 80 | tool | StewardAgent | - | ja | 1125 |
| 81 | assistant | StewardAgent | ja | - | 244 |
| 82 | tool | StewardAgent | - | ja | 1233 |
| 83 | assistant | StewardAgent | - | - | 2052 |
| 84 | user | - | - | - | 96 |
| 85 | assistant | StewardAgent | - | - | 588 |
| 86 | user | - | - | - | 4 |
| 87 | assistant | StewardAgent | - | - | 278 |
| 88 | user | - | - | ja | 0 |
| 89 | assistant | StewardAgent | ja | - | 0 |
| 90 | tool | StewardAgent | - | ja | 397 |
| 91 | assistant | StewardAgent | ja | - | 172 |
| 92 | tool | StewardAgent | - | ja | 807 |
| 93 | assistant | StewardAgent | - | - | 686 |
| 94 | user | - | - | - | 24 |
| 95 | assistant | StewardAgent | ja | - | 302 |
| 96 | tool | StewardAgent | - | ja | 895 |
| 97 | assistant | StewardAgent | - | - | 684 |
| 98 | user | - | - | - | 4 |
| 99 | assistant | StewardAgent | - | - | 0 |
| 100 | user | - | - | ja | 0 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 111 |
| 103 | assistant | StewardAgent | - | - | 896 |
| 104 | user | - | - | - | 20 |
| 105 | assistant | StewardAgent | ja | - | 198 |
| 106 | tool | StewardAgent | - | ja | 1143 |
| 107 | assistant | StewardAgent | ja | - | 272 |
| 108 | tool | StewardAgent | - | ja | 1755 |
| 109 | assistant | StewardAgent | - | - | 3228 |
| 110 | user | - | - | - | 8 |
| 111 | assistant | StewardAgent | - | - | 484 |
| 112 | user | - | - | - | 4 |
| 113 | assistant | StewardAgent | - | - | 240 |
| 114 | user | - | - | ja | 0 |
| 115 | assistant | StewardAgent | ja | - | 0 |
| 116 | tool | StewardAgent | - | ja | 396 |
| 117 | assistant | StewardAgent | ja | - | 154 |
| 118 | tool | StewardAgent | - | ja | 638 |
| 119 | assistant | StewardAgent | ja | - | 348 |
| 120 | tool | StewardAgent | - | ja | 3411 |
| 121 | assistant | StewardAgent | - | - | 2634 |
| 122 | user | - | - | - | 40 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #11 | StewardAgent | `run_pipeline_from_delta` | - |
| #13 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_paused_gate` | - |
| #27 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #29 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `read_run_report` | - |
| #43 | StewardAgent | `get_core_overview` | - |
| #43 | StewardAgent | `list_core_items` | - |
| #47 | StewardAgent | `list_paused_runs` | - |
| #51 | StewardAgent | `save_author_statements` | - |
| #57 | StewardAgent | `run_pipeline_from_delta` | - |
| #59 | StewardAgent | `get_run_status` | - |
| #63 | StewardAgent | `get_run_status` | - |
| #65 | StewardAgent | `get_paused_gate` | - |
| #77 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #79 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |
| #89 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #91 | StewardAgent | `get_run_status` | - |
| #95 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `open_gate_ui` | - |
| #105 | StewardAgent | `get_run_status` | - |
| #107 | StewardAgent | `get_paused_gate` | - |
| #115 | StewardAgent | `submit_paused_gate_decisions` | - |
| #117 | StewardAgent | `get_run_status` | - |
| #119 | StewardAgent | `read_run_report` | - |
| #119 | StewardAgent | `list_core_items` | - |
| #119 | StewardAgent | `get_core_overview` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 432 Zeichen |
| #4 | StewardAgent | 792 Zeichen |
| #4 | StewardAgent | 223 Zeichen |
| #4 | StewardAgent | 116 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #10 | - | 0 Zeichen |
| #12 | StewardAgent | 222 Zeichen |
| #14 | StewardAgent | 728 Zeichen |
| #18 | StewardAgent | 901 Zeichen |
| #20 | StewardAgent | 1280 Zeichen |
| #26 | - | 0 Zeichen |
| #28 | StewardAgent | 386 Zeichen |
| #30 | StewardAgent | 1125 Zeichen |
| #32 | StewardAgent | 1233 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 397 Zeichen |
| #42 | StewardAgent | 638 Zeichen |
| #44 | StewardAgent | 513 Zeichen |
| #44 | StewardAgent | 701 Zeichen |
| #44 | StewardAgent | 64 Zeichen |
| #48 | StewardAgent | 36 Zeichen |
| #52 | StewardAgent | 231 Zeichen |
| #56 | - | 0 Zeichen |
| #58 | StewardAgent | 222 Zeichen |
| #60 | StewardAgent | 727 Zeichen |
| #64 | StewardAgent | 901 Zeichen |
| #66 | StewardAgent | 992 Zeichen |
| #76 | - | 0 Zeichen |
| #78 | StewardAgent | 386 Zeichen |
| #80 | StewardAgent | 1125 Zeichen |
| #82 | StewardAgent | 1233 Zeichen |
| #88 | - | 0 Zeichen |
| #90 | StewardAgent | 397 Zeichen |
| #92 | StewardAgent | 807 Zeichen |
| #96 | StewardAgent | 895 Zeichen |
| #100 | - | 0 Zeichen |
| #102 | StewardAgent | 111 Zeichen |
| #106 | StewardAgent | 1143 Zeichen |
| #108 | StewardAgent | 1755 Zeichen |
| #114 | - | 0 Zeichen |
| #116 | StewardAgent | 396 Zeichen |
| #118 | StewardAgent | 638 Zeichen |
| #120 | StewardAgent | 2193 Zeichen |
| #120 | StewardAgent | 517 Zeichen |
| #120 | StewardAgent | 701 Zeichen |

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

