# Input Context — StewardAgent

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 21
- **Roles:** `assistant=10`, `tool=5`, `user=6`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
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

## Chat Iteration 7 — StewardAgent

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 36 |
| 45 | assistant | StewardAgent | - | - | 480 |
| 46 | user | - | - | - | 4 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |
| #43 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |
| #44 | StewardAgent | 36 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 36 |
| 45 | assistant | StewardAgent | - | - | 480 |
| 46 | user | - | - | - | 4 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |
| #43 | StewardAgent | `list_paused_runs` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |
| #44 | StewardAgent | 36 Zeichen |
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

- **Run:** `20260820_130927_e70aa7`

## Fakten

> Diese Werte stammen direkt aus den ChatMessage-Objekten (Reflection auf
> FunctionCallContent, FunctionResultContent, ChatMessage.Role / .Name).
> Kein Regex, kein Text-Matching.

- **Input Messages:** 55
- **Roles:** `assistant=27`, `tool=14`, `user=14`
- **Agent-Namen im Kontext (ChatMessage.Name):** `StewardAgent`

### Nachrichten

| # | Role | Von Agent | Tool-Call | Tool-Result | Text-Länge |
|---|------|-----------|-----------|-------------|------------|
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 36 |
| 45 | assistant | StewardAgent | - | - | 480 |
| 46 | user | - | - | - | 4 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 222 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 514 |
| 53 | assistant | StewardAgent | - | - | 488 |
| 54 | user | - | - | - | 20 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |
| #43 | StewardAgent | `list_paused_runs` | - |
| #49 | StewardAgent | `run_pipeline_from_github` | - |
| #51 | StewardAgent | `get_run_status` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |
| #44 | StewardAgent | 36 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 222 Zeichen |
| #52 | StewardAgent | 514 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 36 |
| 45 | assistant | StewardAgent | - | - | 480 |
| 46 | user | - | - | - | 4 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 222 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 514 |
| 53 | assistant | StewardAgent | - | - | 488 |
| 54 | user | - | - | - | 20 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 1244 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1889 |
| 59 | assistant | StewardAgent | - | - | 2994 |
| 60 | user | - | - | - | 62 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |
| #43 | StewardAgent | `list_paused_runs` | - |
| #49 | StewardAgent | `run_pipeline_from_github` | - |
| #51 | StewardAgent | `get_run_status` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |
| #44 | StewardAgent | 36 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 222 Zeichen |
| #52 | StewardAgent | 514 Zeichen |
| #56 | StewardAgent | 1244 Zeichen |
| #58 | StewardAgent | 1889 Zeichen |

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

- **Run:** `20260820_130927_e70aa7`

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
| 0 | user | - | - | - | 56 |
| 1 | assistant | StewardAgent | ja | - | 0 |
| 2 | tool | StewardAgent | - | ja | 36 |
| 3 | assistant | StewardAgent | - | - | 480 |
| 4 | user | - | - | ja | 0 |
| 5 | assistant | StewardAgent | ja | - | 0 |
| 6 | tool | StewardAgent | - | ja | 222 |
| 7 | assistant | StewardAgent | ja | - | 0 |
| 8 | tool | StewardAgent | - | ja | 514 |
| 9 | assistant | StewardAgent | - | - | 710 |
| 10 | user | - | - | - | 20 |
| 11 | assistant | StewardAgent | ja | - | 0 |
| 12 | tool | StewardAgent | - | ja | 1244 |
| 13 | assistant | StewardAgent | ja | - | 0 |
| 14 | tool | StewardAgent | - | ja | 1472 |
| 15 | assistant | StewardAgent | - | - | 2378 |
| 16 | user | - | - | - | 30 |
| 17 | assistant | StewardAgent | - | - | 2086 |
| 18 | user | - | - | - | 72 |
| 19 | assistant | StewardAgent | - | - | 0 |
| 20 | user | - | - | ja | 0 |
| 21 | assistant | StewardAgent | ja | - | 0 |
| 22 | tool | StewardAgent | - | ja | 391 |
| 23 | assistant | StewardAgent | ja | - | 0 |
| 24 | tool | StewardAgent | - | ja | 1253 |
| 25 | assistant | StewardAgent | - | - | 1082 |
| 26 | user | - | - | - | 98 |
| 27 | assistant | StewardAgent | ja | - | 0 |
| 28 | tool | StewardAgent | - | ja | 2854 |
| 29 | assistant | StewardAgent | - | - | 0 |
| 30 | user | - | - | ja | 0 |
| 31 | assistant | StewardAgent | ja | - | 0 |
| 32 | tool | StewardAgent | - | ja | 397 |
| 33 | assistant | StewardAgent | ja | - | 0 |
| 34 | tool | StewardAgent | - | ja | 639 |
| 35 | assistant | StewardAgent | ja | - | 0 |
| 36 | tool | StewardAgent | - | ja | 3295 |
| 37 | assistant | StewardAgent | - | - | 1074 |
| 38 | user | - | - | - | 46 |
| 39 | assistant | StewardAgent | - | - | 684 |
| 40 | user | - | - | - | 98 |
| 41 | assistant | StewardAgent | - | - | 2020 |
| 42 | user | - | - | - | 60 |
| 43 | assistant | StewardAgent | ja | - | 0 |
| 44 | tool | StewardAgent | - | ja | 36 |
| 45 | assistant | StewardAgent | - | - | 480 |
| 46 | user | - | - | - | 4 |
| 47 | assistant | StewardAgent | - | - | 0 |
| 48 | user | - | - | ja | 0 |
| 49 | assistant | StewardAgent | ja | - | 0 |
| 50 | tool | StewardAgent | - | ja | 222 |
| 51 | assistant | StewardAgent | ja | - | 0 |
| 52 | tool | StewardAgent | - | ja | 514 |
| 53 | assistant | StewardAgent | - | - | 488 |
| 54 | user | - | - | - | 20 |
| 55 | assistant | StewardAgent | ja | - | 0 |
| 56 | tool | StewardAgent | - | ja | 1244 |
| 57 | assistant | StewardAgent | ja | - | 0 |
| 58 | tool | StewardAgent | - | ja | 1889 |
| 59 | assistant | StewardAgent | - | - | 2994 |
| 60 | user | - | - | - | 62 |
| 61 | assistant | StewardAgent | - | - | 0 |
| 62 | user | - | - | ja | 0 |

### Tool-Calls im Kontext (FunctionCallContent)

| Msg # | Von Agent | Tool | Pfad |
|-------|-----------|------|------|
| #1 | StewardAgent | `list_paused_runs` | - |
| #5 | StewardAgent | `run_pipeline_from_github` | - |
| #7 | StewardAgent | `get_run_status` | - |
| #11 | StewardAgent | `get_run_status` | - |
| #13 | StewardAgent | `get_paused_gate` | - |
| #21 | StewardAgent | `submit_ingest_gate_decisions` | - |
| #23 | StewardAgent | `get_run_status` | - |
| #27 | StewardAgent | `get_paused_gate` | - |
| #31 | StewardAgent | `submit_decision_gate_resolutions` | - |
| #33 | StewardAgent | `get_run_status` | - |
| #35 | StewardAgent | `read_run_report` | - |
| #35 | StewardAgent | `get_core_overview` | - |
| #35 | StewardAgent | `list_core_items` | - |
| #43 | StewardAgent | `list_paused_runs` | - |
| #49 | StewardAgent | `run_pipeline_from_github` | - |
| #51 | StewardAgent | `get_run_status` | - |
| #55 | StewardAgent | `get_run_status` | - |
| #57 | StewardAgent | `get_paused_gate` | - |

### Tool-Results im Kontext (FunctionResultContent)

| Msg # | Von Agent | Länge |
|-------|-----------|-------|
| #2 | StewardAgent | 36 Zeichen |
| #4 | - | 0 Zeichen |
| #6 | StewardAgent | 222 Zeichen |
| #8 | StewardAgent | 514 Zeichen |
| #12 | StewardAgent | 1244 Zeichen |
| #14 | StewardAgent | 1472 Zeichen |
| #20 | - | 0 Zeichen |
| #22 | StewardAgent | 391 Zeichen |
| #24 | StewardAgent | 1253 Zeichen |
| #28 | StewardAgent | 2854 Zeichen |
| #30 | - | 0 Zeichen |
| #32 | StewardAgent | 397 Zeichen |
| #34 | StewardAgent | 639 Zeichen |
| #36 | StewardAgent | 2446 Zeichen |
| #36 | StewardAgent | 785 Zeichen |
| #36 | StewardAgent | 64 Zeichen |
| #44 | StewardAgent | 36 Zeichen |
| #48 | - | 0 Zeichen |
| #50 | StewardAgent | 222 Zeichen |
| #52 | StewardAgent | 514 Zeichen |
| #56 | StewardAgent | 1244 Zeichen |
| #58 | StewardAgent | 1889 Zeichen |
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

