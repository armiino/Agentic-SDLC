# Input Context — StewardAgent

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |

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

- **Run:** `20260820_182221_663b63`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 37
- **Roles:** `assistant=18`, `tool=11`, `user=8`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1113 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 1707 |
| 35 | assistant | StewardAgent | - | - | 3116 |
| 36 | user | - | - | - | 116 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #33 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |
| #32 | StewardAgent | 1113 Zeichen |
| #34 | StewardAgent | 1707 Zeichen |

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

- **Run:** `20260820_182221_663b63`

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
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1113 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 1707 |
| 35 | assistant | StewardAgent | - | - | 3116 |
| 36 | user | - | - | - | 116 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #33 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |
| #32 | StewardAgent | 1113 Zeichen |
| #34 | StewardAgent | 1707 Zeichen |
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

## Chat Iteration 10 — StewardAgent

- **Run:** `20260820_182221_663b63`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 47
- **Roles:** `assistant=23`, `tool=14`, `user=10`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1113 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 1707 |
| 35 | assistant | StewardAgent | - | - | 3116 |
| 36 | user | - | - | - | 116 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 386 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 1253 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 2854 |
| 45 | assistant | StewardAgent | - | - | 4668 |
| 46 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #33 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |
| #32 | StewardAgent | 1113 Zeichen |
| #34 | StewardAgent | 1707 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 386 Zeichen |
| #42 | StewardAgent | 1253 Zeichen |
| #44 | StewardAgent | 2854 Zeichen |

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

- **Run:** `20260820_182221_663b63`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 49
- **Roles:** `assistant=24`, `tool=14`, `user=11`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1113 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 1707 |
| 35 | assistant | StewardAgent | - | - | 3116 |
| 36 | user | - | - | - | 116 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 386 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 1253 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 2854 |
| 45 | assistant | StewardAgent | - | - | 4668 |
| 46 | user | - | - | - | 20 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #33 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |
| #32 | StewardAgent | 1113 Zeichen |
| #34 | StewardAgent | 1707 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 386 Zeichen |
| #42 | StewardAgent | 1253 Zeichen |
| #44 | StewardAgent | 2854 Zeichen |
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

## Chat Iteration 12 — StewardAgent

- **Run:** `20260820_182221_663b63`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 57
- **Roles:** `assistant=28`, `tool=17`, `user=12`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 46 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 1058 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 714 |
| 10 | user | - | - | - | 8 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 742 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 672 |
| 15 | assistant | StewardAgent | - | - | 908 |
| 16 | user | - | - | - | 88 |
| 17 | assistant | StewardAgent | ja | - | 0 |
| 18 | tool | StewardAgent | - | ja | 36 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 222 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 514 |
| 25 | assistant | StewardAgent | - | - | 486 |
| 26 | user | - | - | - | 8 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 728 |
| 29 | assistant | StewardAgent | - | - | 736 |
| 30 | user | - | - | - | 20 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 1113 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 1707 |
| 35 | assistant | StewardAgent | - | - | 3116 |
| 36 | user | - | - | - | 116 |
| 37 | assistant | StewardAgent | - | - | 0 |
| 38 | user | - | - | ja | 0 |
| 39 | assistant | StewardAgent | ja | - | 0 |
| 40 | tool | StewardAgent | - | ja | 386 |
| 41 | assistant | StewardAgent | ja | - | 0 |
| 42 | tool | StewardAgent | - | ja | 1253 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 2854 |
| 45 | assistant | StewardAgent | - | - | 4668 |
| 46 | user | - | - | - | 20 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 397 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 639 |
| 53 | assistant | StewardAgent | ja | - | 0 |
| 54 | tool | StewardAgent | - | ja | 3566 |
| 55 | assistant | StewardAgent | - | - | 1896 |
| 56 | user | - | - | - | 24 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `read_run_report` | - |
| #17 | StewardAgent | `list_paused_runs` | - |
| #21 | StewardAgent | `run_pipeline_from_github` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_run_status` | - |
| #31 | StewardAgent | `get_run_status` | - |
| #33 | StewardAgent | `get_paused_gate` | - |
| #39 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #41 | StewardAgent | `get_run_status` | - |
| #43 | StewardAgent | `get_paused_gate` | - |
| #49 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #51 | StewardAgent | `get_run_status` | - |
| #53 | StewardAgent | `read_run_report` | - |
| #53 | StewardAgent | `get_core_overview` | - |
| #53 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 742 Zeichen |
| #14 | StewardAgent | 672 Zeichen |
| #18 | StewardAgent | 36 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 222 Zeichen |
| #24 | StewardAgent | 514 Zeichen |
| #28 | StewardAgent | 728 Zeichen |
| #32 | StewardAgent | 1113 Zeichen |
| #34 | StewardAgent | 1707 Zeichen |
| #38 | - | 0 Zeichen |
| #40 | StewardAgent | 386 Zeichen |
| #42 | StewardAgent | 1253 Zeichen |
| #44 | StewardAgent | 2854 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 397 Zeichen |
| #52 | StewardAgent | 639 Zeichen |
| #54 | StewardAgent | 2717 Zeichen |
| #54 | StewardAgent | 785 Zeichen |
| #54 | StewardAgent | 64 Zeichen |

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

