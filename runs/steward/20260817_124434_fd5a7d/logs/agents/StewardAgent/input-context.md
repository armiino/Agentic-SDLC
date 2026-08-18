# Input Context — StewardAgent

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 31
- **Roles:** `assistant=15`, `tool=8`, `user=8`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 33
- **Roles:** `assistant=16`, `tool=8`, `user=9`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 53
- **Roles:** `assistant=26`, `tool=13`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 69
- **Roles:** `assistant=34`, `tool=18`, `user=17`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
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

## Chat Iteration 20 — StewardAgent

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 0 | user | - | - | - | 34 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |
| 89 | assistant | StewardAgent | - | - | 0 |
| 90 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |
| #90 | - | 0 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |
| 89 | assistant | StewardAgent | - | - | 0 |
| 90 | user | - | - | ja | 0 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 219 |
| 93 | assistant | StewardAgent | - | - | 0 |
| 94 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |
| #91 | StewardAgent | `submit_ingest_gate_decisions` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |
| #90 | - | 0 Zeichen |
| #92 | StewardAgent | 219 Zeichen |
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

## Chat Iteration 25 — StewardAgent

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |
| 89 | assistant | StewardAgent | - | - | 0 |
| 90 | user | - | - | ja | 0 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 219 |
| 93 | assistant | StewardAgent | - | - | 0 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 177 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 1015 |
| 99 | assistant | StewardAgent | - | - | 2322 |
| 100 | user | - | - | - | 50 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |
| #91 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #95 | StewardAgent | `resume_run` | - |
| #97 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |
| #90 | - | 0 Zeichen |
| #92 | StewardAgent | 219 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 177 Zeichen |
| #98 | StewardAgent | 1015 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |
| 89 | assistant | StewardAgent | - | - | 0 |
| 90 | user | - | - | ja | 0 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 219 |
| 93 | assistant | StewardAgent | - | - | 0 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 177 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 1015 |
| 99 | assistant | StewardAgent | - | - | 2322 |
| 100 | user | - | - | - | 50 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 212 |
| 103 | assistant | StewardAgent | - | - | 1172 |
| 104 | user | - | - | - | 12 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |
| #91 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #95 | StewardAgent | `resume_run` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |
| #90 | - | 0 Zeichen |
| #92 | StewardAgent | 219 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 177 Zeichen |
| #98 | StewardAgent | 1015 Zeichen |
| #102 | StewardAgent | 212 Zeichen |

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

- **Run:** `20260817_124434_fd5a7d`

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
| 2 | tool | StewardAgent | - | ja | 1161 |
| 3 | assistant | StewardAgent | ja | - | 0 |
| 4 | tool | StewardAgent | - | ja | 1761 |
| 5 | assistant | StewardAgent | - | - | 4662 |
| 6 | user | - | - | - | 52 |
| 7 | assistant | StewardAgent | - | - | 0 |
| 8 | user | - | - | ja | 0 |
| 9 | assistant | StewardAgent | ja | - | 0 |
| 10 | tool | StewardAgent | - | ja | 226 |
| 11 | assistant | StewardAgent | - | - | 0 |
| 12 | user | - | - | ja | 0 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 177 |
| 15 | assistant | StewardAgent | ja | - | 0 |
| 16 | tool | StewardAgent | - | ja | 639 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 3419 |
| 19 | assistant | StewardAgent | - | - | 2564 |
| 20 | user | - | - | - | 406 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 264 |
| 23 | assistant | StewardAgent | - | - | 2826 |
| 24 | user | - | - | - | 402 |
| 25 | assistant | StewardAgent | ja | - | 0 |
| 26 | tool | StewardAgent | - | ja | 231 |
| 27 | assistant | StewardAgent | - | - | 818 |
| 28 | user | - | - | - | 76 |
| 29 | assistant | StewardAgent | - | - | 890 |
| 30 | user | - | - | - | 276 |
| 31 | assistant | StewardAgent | - | - | 0 |
| 32 | user | - | - | ja | 0 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 145 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 728 |
| 37 | assistant | StewardAgent | - | - | 978 |
| 38 | user | - | - | - | 8 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 901 |
| 41 | assistant | StewardAgent | - | - | 1332 |
| 42 | user | - | - | - | 40 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 795 |
| 45 | assistant | StewardAgent | - | - | 2464 |
| 46 | user | - | - | - | 36 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 214 |
| 51 | assistant | StewardAgent | - | - | 0 |
| 52 | user | - | - | ja | 0 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 177 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 639 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1617 |
| 59 | assistant | StewardAgent | - | - | 1434 |
| 60 | user | - | - | - | 52 |
| 61 | assistant | StewardAgent | ja | - | 0 |
| 62 | tool | StewardAgent | - | ja | 36 |
| 63 | assistant | StewardAgent | - | - | 1472 |
| 64 | user | - | - | - | 406 |
| 65 | assistant | StewardAgent | ja | - | 0 |
| 66 | tool | StewardAgent | - | ja | 249 |
| 67 | assistant | StewardAgent | - | - | 2048 |
| 68 | user | - | - | - | 22 |
| 69 | assistant | StewardAgent | ja | - | 0 |
| 70 | tool | StewardAgent | - | ja | 231 |
| 71 | assistant | StewardAgent | - | - | 850 |
| 72 | user | - | - | - | 4 |
| 73 | assistant | StewardAgent | - | - | 0 |
| 74 | user | - | - | ja | 0 |
| 75 | assistant | StewardAgent | ja | - | 0 |
| 76 | tool | StewardAgent | - | ja | 145 |
| 77 | assistant | StewardAgent | ja | - | 0 |
| 78 | tool | StewardAgent | - | ja | 706 |
| 79 | assistant | StewardAgent | - | - | 888 |
| 80 | user | - | - | - | 8 |
| 81 | assistant | StewardAgent | ja | - | 0 |
| 82 | tool | StewardAgent | - | ja | 1050 |
| 83 | assistant | StewardAgent | - | - | 1440 |
| 84 | user | - | - | - | 4 |
| 85 | assistant | StewardAgent | ja | - | 0 |
| 86 | tool | StewardAgent | - | ja | 858 |
| 87 | assistant | StewardAgent | - | - | 2978 |
| 88 | user | - | - | - | 34 |
| 89 | assistant | StewardAgent | - | - | 0 |
| 90 | user | - | - | ja | 0 |
| 91 | assistant | StewardAgent | ja | - | 0 |
| 92 | tool | StewardAgent | - | ja | 219 |
| 93 | assistant | StewardAgent | - | - | 0 |
| 94 | user | - | - | ja | 0 |
| 95 | assistant | StewardAgent | ja | - | 0 |
| 96 | tool | StewardAgent | - | ja | 177 |
| 97 | assistant | StewardAgent | ja | - | 0 |
| 98 | tool | StewardAgent | - | ja | 1015 |
| 99 | assistant | StewardAgent | - | - | 2322 |
| 100 | user | - | - | - | 50 |
| 101 | assistant | StewardAgent | ja | - | 0 |
| 102 | tool | StewardAgent | - | ja | 212 |
| 103 | assistant | StewardAgent | - | - | 1172 |
| 104 | user | - | - | - | 12 |
| 105 | assistant | StewardAgent | - | - | 0 |
| 106 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #3 | StewardAgent | `get_paused_gate` | - |
| #9 | StewardAgent | `submit_paused_gate_decisions` | - |
| #13 | StewardAgent | `resume_run` | - |
| #15 | StewardAgent | `get_run_status` | - |
| #17 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `get_core_overview` | - |
| #17 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `list_core_items` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #21 | StewardAgent | `search_rejections` | - |
| #25 | StewardAgent | `save_author_statements` | - |
| #33 | StewardAgent | `run_pipeline_from_delta` | - |
| #35 | StewardAgent | `get_run_status` | - |
| #39 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #53 | StewardAgent | `resume_run` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `read_run_report` | - |
| #57 | StewardAgent | `get_core_overview` | - |
| #57 | StewardAgent | `list_core_items` | - |
| #61 | StewardAgent | `list_paused_runs` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `list_core_items` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #65 | StewardAgent | `search_rejections` | - |
| #69 | StewardAgent | `save_author_statements` | - |
| #75 | StewardAgent | `run_pipeline_from_delta` | - |
| #77 | StewardAgent | `get_run_status` | - |
| #81 | StewardAgent | `get_run_status` | - |
| #85 | StewardAgent | `get_paused_gate` | - |
| #91 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #95 | StewardAgent | `resume_run` | - |
| #97 | StewardAgent | `get_run_status` | - |
| #101 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 1161 Zeichen |
| #4 | StewardAgent | 1761 Zeichen |
| #8 | - | 0 Zeichen |
| #10 | StewardAgent | 226 Zeichen |
| #12 | - | 0 Zeichen |
| #14 | StewardAgent | 177 Zeichen |
| #16 | StewardAgent | 639 Zeichen |
| #18 | StewardAgent | 2186 Zeichen |
| #18 | StewardAgent | 683 Zeichen |
| #18 | StewardAgent | 550 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 64 Zeichen |
| #22 | StewardAgent | 82 Zeichen |
| #22 | StewardAgent | 54 Zeichen |
| #26 | StewardAgent | 231 Zeichen |
| #32 | - | 0 Zeichen |
| #34 | StewardAgent | 145 Zeichen |
| #36 | StewardAgent | 728 Zeichen |
| #40 | StewardAgent | 901 Zeichen |
| #44 | StewardAgent | 795 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 214 Zeichen |
| #52 | - | 0 Zeichen |
| #54 | StewardAgent | 177 Zeichen |
| #56 | StewardAgent | 639 Zeichen |
| #58 | StewardAgent | 683 Zeichen |
| #58 | StewardAgent | 700 Zeichen |
| #58 | StewardAgent | 234 Zeichen |
| #62 | StewardAgent | 36 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 64 Zeichen |
| #66 | StewardAgent | 55 Zeichen |
| #66 | StewardAgent | 66 Zeichen |
| #70 | StewardAgent | 231 Zeichen |
| #74 | - | 0 Zeichen |
| #76 | StewardAgent | 145 Zeichen |
| #78 | StewardAgent | 706 Zeichen |
| #82 | StewardAgent | 1050 Zeichen |
| #86 | StewardAgent | 858 Zeichen |
| #90 | - | 0 Zeichen |
| #92 | StewardAgent | 219 Zeichen |
| #94 | - | 0 Zeichen |
| #96 | StewardAgent | 177 Zeichen |
| #98 | StewardAgent | 1015 Zeichen |
| #102 | StewardAgent | 212 Zeichen |
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

