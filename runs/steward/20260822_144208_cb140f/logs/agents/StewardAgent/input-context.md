# Input Context — StewardAgent

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 5
- **Roles:** `assistant=2`, `tool=1`, `user=2`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 23
- **Roles:** `assistant=11`, `tool=6`, `user=6`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 33
- **Roles:** `assistant=16`, `tool=9`, `user=8`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 35
- **Roles:** `assistant=17`, `tool=9`, `user=9`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 37
- **Roles:** `assistant=18`, `tool=9`, `user=10`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 63
- **Roles:** `assistant=31`, `tool=16`, `user=16`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
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

## Chat Iteration 17 — StewardAgent

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

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
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |
| 67 | assistant | StewardAgent | - | - | 178 |
| 68 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |
| #68 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 73
- **Roles:** `assistant=36`, `tool=18`, `user=19`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |
| 67 | assistant | StewardAgent | - | - | 178 |
| 68 | user | - | - | ja | 0 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 162 |
| 71 | assistant | StewardAgent | - | - | 1130 |
| 72 | user | - | - | - | 54 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |
| #69 | StewardAgent | `save_authored_doc` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |
| #68 | - | 0 Zeichen |
| #70 | StewardAgent | 162 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 75
- **Roles:** `assistant=37`, `tool=18`, `user=20`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |
| 67 | assistant | StewardAgent | - | - | 178 |
| 68 | user | - | - | ja | 0 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 162 |
| 71 | assistant | StewardAgent | - | - | 1130 |
| 72 | user | - | - | - | 54 |
| 73 | assistant | StewardAgent | - | - | 318 |
| 74 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |
| #69 | StewardAgent | `save_authored_doc` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |
| #68 | - | 0 Zeichen |
| #70 | StewardAgent | 162 Zeichen |
| #74 | - | 0 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 81
- **Roles:** `assistant=40`, `tool=20`, `user=21`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |
| 67 | assistant | StewardAgent | - | - | 178 |
| 68 | user | - | - | ja | 0 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 162 |
| 71 | assistant | StewardAgent | - | - | 1130 |
| 72 | user | - | - | - | 54 |
| 73 | assistant | StewardAgent | - | - | 318 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 222 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 1246 |
| 79 | assistant | StewardAgent | - | - | 508 |
| 80 | user | - | - | - | 18 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |
| #69 | StewardAgent | `save_authored_doc` | - |
| #75 | StewardAgent | `run_reproject` | - |
| #77 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |
| #68 | - | 0 Zeichen |
| #70 | StewardAgent | 162 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 222 Zeichen |
| #78 | StewardAgent | 1246 Zeichen |

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

- **Run:** `20260822_144208_cb140f`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 85
- **Roles:** `assistant=42`, `tool=21`, `user=22`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 328 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1208 |
| 3 | assistant | StewardAgent | - | - | 6004 |
| 4 | user | - | - | - | 72 |
| 5 | assistant | StewardAgent | ja | - | 274 |
| 6 | tool | StewardAgent | - | ja | 231 |
| 7 | assistant | StewardAgent | - | - | 170 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 222 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 706 |
| 13 | assistant | StewardAgent | - | - | 862 |
| 14 | user | - | - | - | 14 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 1244 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 1898 |
| 19 | assistant | StewardAgent | - | - | 3064 |
| 20 | user | - | - | - | 10 |
| 21 | assistant | StewardAgent | - | - | 300 |
| 22 | user | - | - | ja | 0 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 391 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 1253 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 8444 |
| 29 | assistant | StewardAgent | - | - | 12026 |
| 30 | user | - | - | - | 82 |
| 31 | assistant | StewardAgent | - | - | 1622 |
| 32 | user | - | - | - | 146 |
| 33 | assistant | StewardAgent | - | - | 2510 |
| 34 | user | - | - | - | 196 |
| 35 | assistant | StewardAgent | - | - | 416 |
| 36 | user | - | - | ja | 0 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 397 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 1246 |
| 41 | assistant | StewardAgent | - | - | 1394 |
| 42 | user | - | - | - | 34 |
| 43 | assistant | StewardAgent | ja | - | 332 |
| 44 | tool | StewardAgent | - | ja | 1391 |
| 45 | assistant | StewardAgent | - | - | 0 |
| 46 | user | - | - | ja | 0 |
| 47 | assistant | StewardAgent | ja | - | 0 |
| 48 | tool | StewardAgent | - | ja | 396 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 2197 |
| 51 | assistant | StewardAgent | - | - | 746 |
| 52 | user | - | - | - | 8 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 4234 |
| 55 | assistant | StewardAgent | - | - | 1534 |
| 56 | user | - | - | - | 36 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 36 |
| 59 | assistant | StewardAgent | - | - | 756 |
| 60 | user | - | - | - | 4 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |
| 63 | assistant | StewardAgent | ja | - | 0 |
| 64 | tool | StewardAgent | - | ja | 5168 |
| 65 | assistant | StewardAgent | - | - | 9912 |
| 66 | user | - | - | - | 4 |
| 67 | assistant | StewardAgent | - | - | 178 |
| 68 | user | - | - | ja | 0 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 162 |
| 71 | assistant | StewardAgent | - | - | 1130 |
| 72 | user | - | - | - | 54 |
| 73 | assistant | StewardAgent | - | - | 318 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 222 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 1246 |
| 79 | assistant | StewardAgent | - | - | 508 |
| 80 | user | - | - | - | 18 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 12996 |
| 83 | assistant | StewardAgent | - | - | 398 |
| 84 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #5 | StewardAgent | `save_author_statements` | - |
| #9 | StewardAgent | `run_pipeline_from_delta` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `get_paused_gate` | - |
| #23 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #25 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #37 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #47 | StewardAgent | `submit_paused_gate_decisions` | - |
| #49 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `read_run_report` | - |
| #49 | StewardAgent | `list_core_items` | - |
| #53 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_paused_runs` | - |
| #63 | StewardAgent | `draft_authored_doc` | - |
| #69 | StewardAgent | `save_authored_doc` | - |
| #75 | StewardAgent | `run_reproject` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #2 | StewardAgent | 1107 Zeichen |
| #2 | StewardAgent | 65 Zeichen |
| #6 | StewardAgent | 231 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 222 Zeichen |
| #12 | StewardAgent | 706 Zeichen |
| #16 | StewardAgent | 1244 Zeichen |
| #18 | StewardAgent | 1898 Zeichen |
| #22 | - | 0 Zeichen |
| #24 | StewardAgent | 391 Zeichen |
| #26 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 8444 Zeichen |
| #36 | - | 0 Zeichen |
| #38 | StewardAgent | 397 Zeichen |
| #40 | StewardAgent | 1246 Zeichen |
| #44 | StewardAgent | 1391 Zeichen |
| #46 | - | 0 Zeichen |
| #48 | StewardAgent | 396 Zeichen |
| #50 | StewardAgent | 902 Zeichen |
| #50 | StewardAgent | 1231 Zeichen |
| #50 | StewardAgent | 64 Zeichen |
| #54 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2472 Zeichen |
| #54 | StewardAgent | 1123 Zeichen |
| #58 | StewardAgent | 36 Zeichen |
| #62 | - | 0 Zeichen |
| #64 | StewardAgent | 5168 Zeichen |
| #68 | - | 0 Zeichen |
| #70 | StewardAgent | 162 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 222 Zeichen |
| #78 | StewardAgent | 1246 Zeichen |
| #82 | StewardAgent | 12996 Zeichen |
| #84 | - | 0 Zeichen |

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

