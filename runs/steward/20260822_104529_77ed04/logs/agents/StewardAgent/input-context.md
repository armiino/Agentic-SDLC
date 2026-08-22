# Input Context — StewardAgent

- **Run:** `20260822_104529_77ed04`

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
| 0 | user | - | - | - | 338 |

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

- **Run:** `20260822_104529_77ed04`

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
| 0 | user | - | - | - | 338 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 283 |
| 5 | assistant | StewardAgent | - | - | 2438 |
| 6 | user | - | - | - | 24 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |

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

- **Run:** `20260822_104529_77ed04`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 9
- **Roles:** `assistant=4`, `tool=2`, `user=3`
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

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 64 Zeichen |
| #4 | StewardAgent | 76 Zeichen |
| #4 | StewardAgent | 79 Zeichen |

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

- **Run:** `20260822_104529_77ed04`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 13
- **Roles:** `assistant=6`, `tool=3`, `user=4`
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

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `list_core_items` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #3 | StewardAgent | `search_rejections` | - |
| #9 | StewardAgent | `save_author_statements` | - |

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

- **Run:** `20260822_104529_77ed04`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 19
- **Roles:** `assistant=9`, `tool=5`, `user=5`
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

- **Run:** `20260822_104529_77ed04`

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

- **Run:** `20260822_104529_77ed04`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 31
- **Roles:** `assistant=15`, `tool=9`, `user=7`
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

- **Run:** `20260822_104529_77ed04`

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

