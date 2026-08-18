# Input Context — StewardAgent

- **Run:** `20260817_120925_44a0f3`

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
| 0 | user | - | - | - | 178 |

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

- **Run:** `20260817_120925_44a0f3`

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
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 7
- **Roles:** `assistant=3`, `tool=1`, `user=3`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 11
- **Roles:** `assistant=5`, `tool=2`, `user=4`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 13
- **Roles:** `assistant=6`, `tool=2`, `user=5`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
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

## Chat Iteration 6 — StewardAgent

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 19
- **Roles:** `assistant=9`, `tool=4`, `user=6`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
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

## Chat Iteration 7 — StewardAgent

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 23
- **Roles:** `assistant=11`, `tool=5`, `user=7`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 27
- **Roles:** `assistant=13`, `tool=6`, `user=8`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 29
- **Roles:** `assistant=14`, `tool=6`, `user=9`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
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

## Chat Iteration 10 — StewardAgent

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 33
- **Roles:** `assistant=16`, `tool=7`, `user=10`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 35
- **Roles:** `assistant=17`, `tool=7`, `user=11`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 41
- **Roles:** `assistant=20`, `tool=9`, `user=12`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 43
- **Roles:** `assistant=21`, `tool=9`, `user=13`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |
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

## Chat Iteration 14 — StewardAgent

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 49
- **Roles:** `assistant=24`, `tool=11`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 177 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 948 |
| 47 | assistant | StewardAgent | - | - | 1192 |
| 48 | user | - | - | - | 6 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `resume_run` | - |
| #45 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 177 Zeichen |
| #46 | StewardAgent | 948 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 53
- **Roles:** `assistant=26`, `tool=12`, `user=15`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 177 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 948 |
| 47 | assistant | StewardAgent | - | - | 1192 |
| 48 | user | - | - | - | 6 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 202 |
| 51 | assistant | StewardAgent | - | - | 1078 |
| 52 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `resume_run` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 177 Zeichen |
| #46 | StewardAgent | 948 Zeichen |
| #50 | StewardAgent | 202 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 55
- **Roles:** `assistant=27`, `tool=12`, `user=16`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 177 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 948 |
| 47 | assistant | StewardAgent | - | - | 1192 |
| 48 | user | - | - | - | 6 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 202 |
| 51 | assistant | StewardAgent | - | - | 1078 |
| 52 | user | - | - | - | 4 |
| 53 | assistant | StewardAgent | - | - | 0 |
| 54 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `resume_run` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 177 Zeichen |
| #46 | StewardAgent | 948 Zeichen |
| #50 | StewardAgent | 202 Zeichen |
| #54 | - | 0 Zeichen |

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

- **Run:** `20260817_120925_44a0f3`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 59
- **Roles:** `assistant=29`, `tool=13`, `user=17`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 178 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 573 |
| 3 | assistant | StewardAgent | - | - | 3086 |
| 4 | user | - | - | - | 726 |
| 5 | assistant | StewardAgent | - | - | 2606 |
| 6 | user | - | - | - | 22 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 231 |
| 9 | assistant | StewardAgent | - | - | 760 |
| 10 | user | - | - | - | 4 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 145 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 728 |
| 17 | assistant | StewardAgent | - | - | 1018 |
| 18 | user | - | - | - | 8 |
| 19 | assistant | StewardAgent | ja | - | 0 |
| 20 | tool | StewardAgent | - | ja | 901 |
| 21 | assistant | StewardAgent | - | - | 1370 |
| 22 | user | - | - | - | 38 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1117 |
| 25 | assistant | StewardAgent | - | - | 2588 |
| 26 | user | - | - | - | 10 |
| 27 | assistant | StewardAgent | - | - | 0 |
| 28 | user | - | - | ja | 0 |
| 29 | assistant | StewardAgent | ja | - | 0 |
| 30 | tool | StewardAgent | - | ja | 214 |
| 31 | assistant | StewardAgent | - | - | 502 |
| 32 | user | - | - | - | 4 |
| 33 | assistant | StewardAgent | - | - | 0 |
| 34 | user | - | - | ja | 0 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 177 |
| 37 | assistant | StewardAgent | ja | - | 0 |
| 38 | tool | StewardAgent | - | ja | 867 |
| 39 | assistant | StewardAgent | - | - | 1466 |
| 40 | user | - | - | - | 4 |
| 41 | assistant | StewardAgent | - | - | 0 |
| 42 | user | - | - | ja | 0 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 177 |
| 45 | assistant | StewardAgent | ja | - | 0 |
| 46 | tool | StewardAgent | - | ja | 948 |
| 47 | assistant | StewardAgent | - | - | 1192 |
| 48 | user | - | - | - | 6 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 202 |
| 51 | assistant | StewardAgent | - | - | 1078 |
| 52 | user | - | - | - | 4 |
| 53 | assistant | StewardAgent | - | - | 0 |
| 54 | user | - | - | ja | 0 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 23 |
| 57 | assistant | StewardAgent | - | - | 2312 |
| 58 | user | - | - | - | 116 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `list_core_items` | - |
| #1 | StewardAgent | `search_rejections` | - |
| #7 | StewardAgent | `save_author_statements` | - |
| #13 | StewardAgent | `run_pipeline_from_delta` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #19 | StewardAgent | `get_run_status` | - |
| #23 | StewardAgent | `get_paused_gate` | - |
| #29 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #35 | StewardAgent | `resume_run` | - |
| #37 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `resume_run` | - |
| #45 | StewardAgent | `get_run_status` | - |
| #49 | StewardAgent | `get_paused_gate` | - |
| #55 | StewardAgent | `open_gate_ui` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 339 Zeichen |
| #2 | StewardAgent | 64 Zeichen |
| #2 | StewardAgent | 106 Zeichen |
| #8 | StewardAgent | 231 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 145 Zeichen |
| #16 | StewardAgent | 728 Zeichen |
| #20 | StewardAgent | 901 Zeichen |
| #24 | StewardAgent | 1117 Zeichen |
| #28 | - | 0 Zeichen |
| #30 | StewardAgent | 214 Zeichen |
| #34 | - | 0 Zeichen |
| #36 | StewardAgent | 177 Zeichen |
| #38 | StewardAgent | 867 Zeichen |
| #42 | - | 0 Zeichen |
| #44 | StewardAgent | 177 Zeichen |
| #46 | StewardAgent | 948 Zeichen |
| #50 | StewardAgent | 202 Zeichen |
| #54 | - | 0 Zeichen |
| #56 | StewardAgent | 23 Zeichen |

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

