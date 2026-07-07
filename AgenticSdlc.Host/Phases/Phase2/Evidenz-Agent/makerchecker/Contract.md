# Maker-Checker Contract

> Zweck: schlanker Vertrag zwischen Checker, Workflow-Condition und Repair. Kein neuer allgemeiner
> ReviewWorkflow. Der Contract gilt zuerst nur fuer `requirements.md` gegen `consumable.json`.

---

## 1. Ziel

Der Maker-Checker prueft ein generiertes Artefakt nicht offen gegen das Rohtranskript, sondern gegen den
freigegebenen Ledger:

```text
requirements.md + consumable.json -> ContractCheckReport
```

Der Report muss drei Fragen beantworten:

1. Darf das Artefakt weiter?
2. Gibt es automatisch reparierbare Verstossstellen?
3. Muss ein Mensch entscheiden?

---

## 2. Output des Checkers

Der Checker liefert genau ein `ContractCheckReport`:

```csharp
public sealed record ContractCheckReport(
    bool Pass,
    int Iteration,
    ContractDecision Decision,
    IReadOnlyList<ContractViolation> Violations,
    IReadOnlyList<RepairItem> RepairItems,
    IReadOnlyDictionary<string, object> Checks);
```

`Pass=true` bedeutet: keine blockierenden Vertragsverstoesse. Warnungen duerfen existieren, aber nicht repair-
pflichtig sein.

---

## 3. Decision

Die Decision wird deterministisch aus den Violations abgeleitet:

```csharp
public enum ContractDecision
{
    Pass,
    Repair,
    HumanReview,
    MaxIterationsReached
}
```

Regel:

```text
Keine error-Violations
  -> Pass

Mindestens ein repairable error und iteration < maxIterations
  -> Repair

Mindestens ein error, aber kein sicher repairbarer error
  -> HumanReview

Mindestens ein error und iteration >= maxIterations
  -> MaxIterationsReached
```

Diese Decision ist die Grundlage fuer die MAF-Condition.

---

## 4. Violation

Eine Violation ist die kleinste pruefbare Vertragsverletzung:

```csharp
public sealed record ContractViolation(
    string Code,
    ContractSeverity Severity,
    bool Repairable,
    string Message,
    int? LineNumber,
    string? ArtifactQuote,
    IReadOnlyList<string> ClaimIds,
    IReadOnlyDictionary<string, string> LedgerFacets,
    string? SuggestedAction);
```

Severity:

```csharp
public enum ContractSeverity
{
    Info,
    Warning,
    Error
}
```

Nur `Error` blockiert den Pass. `Warning` ist sichtbar, aber kein automatischer Repair-Grund.

---

## 5. Violation Codes

Erster, bewusst kleiner Satz:

| Code | Bedeutung | Default |
|---|---|---|
| `MISSING_CITATION` | fachliche Requirement-Zeile ohne `[claimId]` | `Error`, meist nicht sicher repairable |
| `UNKNOWN_CLAIM_ID` | zitierte ID existiert nicht im `consumable.json` | `Error`, HumanReview |
| `REQUIRED_CLAIM_UNUSED` | `requirements.required` Claim wurde nicht zitiert | `Error`, repairable |
| `WRONG_DISPOSITION` | Claim ist fuer `requirements` nicht required/context/optional nutzbar | `Error`, HumanReview oder entfernen |
| `FACET_OVERSTATED` | Artefakt verstaerkt Ledger-Facetten | `Error`, repairable |
| `EVIDENCE_UNSUPPORTED_DETAIL` | Zeile zitiert Claim-ID, enthaelt aber ein Detail, das im Claim-Paket nicht gedeckt ist | `Error`, repairable oder HumanReview |
| `UNREFERENCED_FACTUAL_LINE` | fachliche Zeile ohne verwertbare Quelle | `Error`, entfernen oder HumanReview |

MC0 startet mit den deterministischen Codes:

```text
MISSING_CITATION
UNKNOWN_CLAIM_ID
REQUIRED_CLAIM_UNUSED
WRONG_DISPOSITION
```

`FACET_OVERSTATED` startet als einfache Heuristik und kann spaeter durch einen bounded Critic verbessert werden.

`EVIDENCE_UNSUPPORTED_DETAIL` ist **datentechnisch nicht** Teil von MC0 (MC0 ist rein deterministisch; C7 braucht
einen LLM-Critic). **Wichtig fuer die Prioritaet:** dieser Code ist **nicht optional**, sobald der Arm-B-Generator
die volle `consumable.json` bekommt und `evidence` + `notes` fuer Details **nutzen** darf. Denn genau dann kann er
Details ableiten oder dazuerfinden — und die `claimId` beweist nur **Herkunft**, nicht **Deckung**. Es entstehen
zwei getrennte Treue-Ebenen:

```text
Nachvollziehbarkeit  = "Wo kommt es her?"            -> MISSING_CITATION / UNKNOWN_CLAIM_ID   (deterministisch)
Detail-Treue         = "Ist DIESES Detail gedeckt?"  -> EVIDENCE_UNSUPPORTED_DETAIL            (bounded Critic)
```

Der Code wird durch einen bounded Critic geprueft, der nur die Artefakt-Zeile und die zitierten Claim-Pakete sieht
(**kein** Rohtranskript-Review — die `claimId` lokalisiert die Verifikation, das macht den Check bezahlbar):

```text
Artefakt-Zeile
+ proposition/facets/evidence/notes/sourceUnitIds der zitierten Claims
-> supported | evidence_unsupported_detail | facet_overstated | unclear
```

Die sichtbare Referenz im Artefakt bleibt trotzdem nur die `claimId`. Die `claimId` referenziert das komplette
Claim-Paket, nicht nur die Proposition. Vollstaendige Begruendung: `Plan.md` §4.4.

---

## 6. RepairItem

Der Repair-Executor bekommt nicht den ganzen Report als offene Aufgabe, sondern eine kleine Liste von Items:

```csharp
public sealed record RepairItem(
    string Id,
    string ViolationCode,
    int? LineNumber,
    string CurrentText,
    IReadOnlyList<string> ClaimIds,
    string Instruction,
    IReadOnlyDictionary<string, string> AllowedFacetBounds,
    IReadOnlyList<string> EvidenceQuotes);
```

Prinzip:

```text
Repair patcht nur betroffene Zeilen.
Repair schreibt nicht das ganze Artefakt neu.
Nach jedem Repair laeuft der Checker erneut.
```

Beispiele:

```text
FACET_OVERSTATED:
  Instruction = "Schwaeche die Formulierung so ab, dass status/modality/timeScope nicht verstaerkt werden."

REQUIRED_CLAIM_UNUSED:
  Instruction = "Ergaenze eine Requirement-Zeile fuer diesen Claim mit Source-ID."
```

---

## 7. MAF-Condition

Der `ContractCheckerExecutor` routet anhand der `Decision`:

```text
Decision.Pass
  -> FinalArtifactMessage(maxIterationsReached=false, needsHumanReview=false)

Decision.Repair
  -> RepairRequestMessage(report.RepairItems, iteration + 1)

Decision.HumanReview
  -> FinalArtifactMessage(needsHumanReview=true)

Decision.MaxIterationsReached
  -> FinalArtifactMessage(maxIterationsReached=true)
```

Damit ist die Workflow-Condition klein und stabil. Sie haengt nicht an Prosa, sondern an einem geschlossenen
Enum.

---

## 8. Checks in MC0

MC0 soll nur das bauen, was deterministisch und sofort nuetzlich ist:

1. Markdown-Requirement-Zeilen parsen.
2. `[claimId]` aus jeder fachlichen Zeile extrahieren.
3. IDs gegen `consumable.json` pruefen.
4. Alle `requirements.required` Claims gegen verwendete IDs pruefen.
5. Disposition der zitierten Claims fuer `requirements` pruefen.
6. `contract-report.json` schreiben.

Kein LLM. Kein Repair. Kein Workflow-Zwang.

Spaeterer Ausbau:

```text
MC3/Critic:
  Prueft FACET_OVERSTATED und EVIDENCE_UNSUPPORTED_DETAIL gegen die zitierten Claim-Pakete.
  Kein Rohtranskript-Review, keine offene Fehlersuche.
```

Der Critic laeuft **nicht pro Zeile**: deterministischer Trichter zuerst (Citation/Known-ID/Delta-Vorfilter,
0 LLM), dann **gebuendelte** Batch-Calls nur auf die Zeilen mit echtem Detail-Delta, plus Caching ueber die
Repair-Iterationen. Kostenmodell im Detail: `Plan.md` §4.6. Abgrenzung „kein open-world-Rueckfall": `Plan.md` §4.5.

---

## 9. Grenze

Dieser Contract beweist nicht, dass der Ledger absolute Wahrheit ist. Er prueft nur:

```text
Haelt das Artefakt den Ledger-Consumer-Vertrag ein?
```

Open-world Coverage gegen das Rohtranskript bleibt Aufgabe des Ledger-Baus und der Human-in-the-loop-
Adjudikation, nicht des Maker-Checker-Subworkflows.
