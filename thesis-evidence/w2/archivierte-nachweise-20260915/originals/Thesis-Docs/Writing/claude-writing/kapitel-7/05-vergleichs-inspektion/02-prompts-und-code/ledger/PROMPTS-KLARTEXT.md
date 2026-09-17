# Ledger-Stufen-Prompts im KLARTEXT (extrahiert 07.09.)

> Automatisch aus den C#-Raw-String-Literalen (`"""…"""`) der Stufen-Dateien extrahiert.
> Nur PROMPT-Konstanten (System-/Instruktions-Texte); JSON-SCHEMAS sind mit [SCHEMA — gekürzt]
> markiert, ihr Volltext steht in den .cs-Dateien in diesem Ordner. Quelle je Block angegeben.

## step-01 Kandidaten-Extraktion  
`units/UnitAwareSemanticLedgerExtractor.cs`

**`SystemPrompt`:**

```
Du extrahierst einen SEMANTIC SOURCE LEDGER aus deterministischen Atomic Units eines Stakeholder-Transkripts.

        Ziel:
        - Recall-first, aber nur SDLC-relevante, konkrete und pruefbare SourceClaims.
        - Bewahre Status, Modalitaet, Scope und TimeScope.
        - Jede extrahierte Aussage MUSS sourceUnitIds enthalten.
        - sourceUnitIds duerfen ausschliesslich IDs aus dem Input sein.
        - Wenn eine Aussage mehrere Units benoetigt, nenne alle relevanten IDs.
        - Nicht jede Unit muss verwendet werden; irrelevante Smalltalk-/Meta-Units duerfen ungenutzt bleiben.
        - Keine Status-, Scope- oder Modalitaetsverstaerkung.

        FACETTEN-TAXONOMIE (verbindlich, keine anderen Werte):
        status: decided | open | rejected | uncertain | required
          - required NUR fuer extern vorgeschriebene, nicht-verhandelbare Pflicht (Gesetz/Policy/Compliance).
          - Team-internes "wir muessen X" ist status=open|decided + modality=must.
        modality: must | must_clarify | must_consider | must_note | must_not | desired | optional

        DISPOSITIONS-VERGABE (verbindlich — die Disposition ist die Weiche, WOHIN ein Claim spaeter reist):
        - open-questions=required NUR fuer echte OFFENE Punkte: modality=must_clarify/must_consider ODER die
          Evidenz benennt die Offenheit woertlich ("noch offen", "klaeren wir mit ...", "weiss nicht genau").
        - Eine im Meeting FESTGELEGTE Sache (modality=must/must_not, zugesagt/beschlossen) bekommt
          open-questions=not_applicable — sie ist Anforderung, keine Frage. Doppel-Natur NUR, wenn ein explizit
          OFFENER Rest woertlich belegt ist (dann requirements=required UND open-questions=required, und notes
          benennt den offenen Rest).
        - representationMode der open-questions-Spur: question (bzw. open_decision, wenn die Quelle eine
          anstehende Entscheidung benennt).
        - Keine Dispositions-Verstaerkung: nicht mehr Spuren als die Quelle belegt.

        Antworte ausschliesslich mit JSON:
        {
          "entries": [
            {
              "id": "kurze stabile ID oder leer",
              "proposition": "...",
              "kind": "decision|requirement|constraint|risk|open_requirement|open_question|scope|compliance_constraint|process_constraint|non_functional_requirement",
              "status": "decided|open|rejected|uncertain|required",
              "modality": "must|must_clarify|must_consider|must_note|must_not|desired|optional",
              "scope": "kurzer_scope_string",
              "timeScope": "mvp|later_possible|mvp_or_later_unclear|null",
              "evidence": [
                { "source": "transcript", "quote": "Speaker: ..." }
              ],
              "disposition": {
                "requirements": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "architecture": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "risks": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" },
                "open-questions": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|consciously_omitted" }
              },
              "riskLevel": "high|medium|low",
              "notes": "kurz",
              "sourceUnitIds": ["AU-0001"]
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## step-01c Unused-Triage  
`units/UnusedUnitTriageReviewer.cs`

**`SystemPrompt`:**

```
Du sichtest Atomic Units, die von keinem Candidate Claim direkt referenziert wurden.

        Ziel dieses Schritts ist NUR Triage, kein Ledger-Vergleich.
        Entscheide grob, ob eine Unit offensichtlich irrelevant/noise ist oder potenziell fachlich relevant sein kann.

        Markiere konservativ:
        - Wenn die Unit eine konkrete SDLC-relevante Aussage, Frage, Unsicherheit, Entscheidung, Constraint,
          Risiko, fachliche Regel, Compliance-Aussage, Architekturhinweis oder Scope-Aussage enthaelt:
          triage = potentially_relevant.
        - Wenn sie nur Zustimmung, Rueckfrage ohne Inhalt, Gespraechsfuellung oder reine Wiederholung ist:
          triage = acknowledgement | smalltalk | low_signal | repetition.
        - Wenn unklar: potentially_relevant.

        Erlaubte triage-Werte:
        trash
        smalltalk
        acknowledgement
        repetition
        low_signal
        potentially_relevant

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "triage": "potentially_relevant|low_signal|repetition|acknowledgement|smalltalk|trash",
              "reason": "kurz",
              "keywords": ["SAP", "Rabattlogik"]
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## step-01d Unused-Compare  
`units/UnusedUnitLedgerComparer.cs`

**`SystemPrompt`:**

```
Du vergleichst potenziell relevante, aber nicht direkt referenzierte Atomic Units gegen einen bestehenden Candidate Ledger.

        Ziel:
        - Entscheide, ob die Unit semantisch bereits im Ledger enthalten ist oder ob ein Claim/Evidence fehlt.
        - Nicht den Ledger neu schreiben.
        - Wenn die Unit nur zusaetzliche Evidence/Detail fuer einen bestehenden Claim ist: attach_as_evidence.
        - Wenn die Unit voll semantisch enthalten ist: already_covered_indirectly.
        - Wenn eine eigenstaendige wichtige Aussage fehlt: missing_claim.
        - Wenn unklar: needs_human.

        Erlaubte verdict-Werte:
        already_covered_indirectly
        attach_as_evidence
        missing_claim
        needs_human

        Erlaubte suggestedAction-Werte:
        link_existing_candidate
        attach_evidence
        create_candidate
        human_review

        STRENGE REGEL (Pflicht):
        - attach_as_evidence und already_covered_indirectly sind NUR erlaubt, wenn relatedCandidateIds
          mindestens EINE existierende Candidate-ID aus existingCandidates enthaelt.
        - Kannst du keinen konkreten Kandidaten benennen, waehle needs_human (oder missing_claim,
          wenn eine eigenstaendige Aussage fehlt). NIEMALS Deckung ohne Referenz behaupten.

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "verdict": "already_covered_indirectly|attach_as_evidence|missing_claim|needs_human",
              "suggestedAction": "link_existing_candidate|attach_evidence|create_candidate|human_review",
              "reason": "kurz",
              "suggestedProposition": "nur bei missing_claim/needs_human, sonst null",
              "relatedCandidateIds": ["R1"]
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

**`RepairSystemPrompt`:**

```
Du hast unused Units gegen einen Candidate Ledger verglichen und fuer die folgenden Units Deckung
        behauptet (attach_as_evidence oder already_covered_indirectly), aber KEINE existierende Candidate-ID
        benannt. Das ist unzulaessig. Korrigiere JEDE dieser Units:
        - Traegt ein konkreter Kandidat die Deckung wirklich: nenne seine ID(s) aus existingCandidates in relatedCandidateIds.
        - Sonst stufe ehrlich um: missing_claim (mit suggestedProposition) oder needs_human.
        Antworte ausschliesslich mit demselben JSON-Format ({"items":[...]}) und denselben erlaubten Werten wie zuvor.
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

## step-01d Unused-Review (Einzelurteil)  
`units/UnusedUnitReviewer.cs`

**`SystemPrompt`:**

```
Du pruefst Atomic Units, die von keinem Candidate Claim referenziert wurden.

        Ziel:
        - Nicht den Ledger neu schreiben.
        - Nur klassifizieren, ob eine unused Unit fachlich wichtig sein koennte.
        - Konservativ sein: Wenn eine Unit SDLC-relevante Information enthaelt, markiere sie nicht als irrelevant.
        - Wenn sie schon indirekt durch bestehende Candidates abgedeckt ist, nenne relatedCandidateIds.
        - Wenn sie eine neue, wichtige Aussage enthaelt, markiere missing_claim.
        - Wenn unklar, markiere needs_human.

        Erlaubte verdict-Werte:
        irrelevant
        low_signal
        already_covered_indirectly
        missing_claim
        needs_human

        Erlaubte suggestedAction-Werte:
        ignore
        keep_for_context
        link_existing_candidate
        create_candidate
        human_review

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "unitId": "AU-0001",
              "verdict": "missing_claim|already_covered_indirectly|needs_human|irrelevant|low_signal",
              "suggestedAction": "create_candidate|link_existing_candidate|human_review|ignore|keep_for_context",
              "reason": "kurz",
              "suggestedProposition": "nur bei missing_claim/needs_human, sonst null",
              "relatedCandidateIds": ["C001"]
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## step-02 Kanonisierung  
`core/SemanticLedgerCanonicalizer.cs`

**`SystemPrompt`:**

```
Du normalisierst einen breit extrahierten Candidate Semantic Ledger aus einem Stakeholder-Transkript.

        Ziel:
        - Mache aus vielen richtigen, aber verteilten Kandidaten einen kanonischen Semantic Ledger.
        - Merge Eintraege, die gemeinsam EINEN fachlichen Claim bilden.
        - Erhalte und repariere Status, Modalitaet, Scope, TimeScope und Disposition.
        - Arbeite generisch fuer beliebige Transkripte. Nutze keine erwartete Fixture.

        Bekannte generische Fehlermodi, die du beheben sollst:
        1. Split-Claims:
           Ein fachlicher Claim ist auf mehrere Kandidaten verteilt.
           Beispielmuster: Login + Double-Opt-In, Rabattgrenze + Workflow-Status,
           Nutzerzahl-Spanne + Skalierungsfolge, kein Ticketsystem + Datenschutzrisiko.

        2. Abgeschwaechte Statusfacetten:
           "nie besprochen", "nicht entschieden", "offen", "vielleicht spaeter" muessen erhalten bleiben.
           Nicht zu "planned", "decided", "required" verstaerken.

        3. Scope-Verschiebung:
           "nicht im MVP", "spaeter moeglich", "MVP oder spaeter unklar" muessen getrennt bleiben.
           Nicht zu MVP-Anforderung machen.

        4. Modalitaets-Verschiebung:
           "gewuenscht", "optional", "muss geklaert werden", "muss beruecksichtigt werden"
           nicht miteinander verwechseln.

        5. Disposition:
           Entscheide pro Artefakt, ob der kanonische Claim required, optional, context oder not_applicable ist.
           Verwende fuer applicability NUR: required | optional | context | not_applicable.
           Verwende fuer representationMode NUR:
           requirement | constraint | open_decision | assumption | risk_reference | question | open_question | consciously_omitted.
           Verwende NIEMALS representationMode="decision"; nutze open_decision fuer offene Entscheidungen
           oder constraint/requirement fuer verbindliche Entscheidungen.
           Requirements: fachliche Anforderungen, Constraints, Scope-/MVP-Entscheidungen, offene Anforderungen.
           Architecture: Architekturentscheidungen, Integrations-/Skalierungs-/technische Offenheiten.
           Risks: Risiken, Tradeoffs, Compliance-/Datenschutz-/Terminrisiken.
           Open-questions: offene Entscheidungen/Klaerungsbedarfe.

        Was du NICHT tun sollst:
        - Keine wichtigen Kandidaten loeschen, nur weil sie unbequem sind.
        - Keine neuen fachlichen Claims ohne Candidate-Evidence erfinden.
        - Keine unabhängigen Claims zusammenwerfen, wenn dadurch Status/Scope unscharf wird.
        - Keine transkriptspezifischen Sonderregeln verwenden.

        Output:
        - 30-80 kanonische Eintraege.
        - Jeder Eintrag hat Evidence aus den zusammengefuehrten Kandidaten.
        - CLUSTER-TRACE (Pflicht, strukturiert):
          - candidateIds: ALLE Candidate-IDs, die in diesen kanonischen Eintrag eingeflossen sind
            (auch bei nur einem Kandidaten genau diese eine ID). KEINE Candidate-ID darf still verschwinden:
            jeder Eingangskandidat muss in genau einem kanonischen Eintrag unter candidateIds auftauchen.
          - assumedRelation: WIE die Kandidaten zusammengehoeren. Erlaubt:
            same_proposition (gleiche Aussage / Duplikat-Merge) |
            refines (ein Kandidat praezisiert den anderen) |
            temporal_sequence (zeitlicher/prozessualer Zusammenhang) |
            elaborates (ergaenzende Facette desselben Claims) |
            standalone (genau ein Kandidat, kein Merge).
        - notes weiterhin kurz fuer Facet-Repair-Hinweise (nicht fuer die Candidate-Liste, die steht in candidateIds).

        Antworte ausschliesslich mit JSON im exakt gleichen Schema:
        {
          "entries": [
            {
              "id": "canonical-stable-id",
              "proposition": "...",
              "kind": "decision|requirement|constraint|risk|open_requirement|open_question|scope|compliance_constraint|process_constraint|non_functional_requirement|meta",
              "status": "decided|open|rejected|uncertain|required",
              "modality": "must|must_clarify|must_consider|must_note|must_not|desired|optional",
              "scope": "...",
              "timeScope": "mvp|later_possible|mvp_or_later_unclear|null",
              "evidence": [{ "source": "...", "quote": "..." }],
              "disposition": {
                "requirements": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "architecture": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "risks": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" },
                "open-questions": { "applicability": "required|optional|context|not_applicable", "representationMode": "requirement|constraint|open_decision|assumption|risk_reference|question|open_question|consciously_omitted" }
              },
              "riskLevel": "high|medium|low",
              "notes": "facet repair: ...",
              "candidateIds": ["cand-id1", "cand-id2"],
              "assumedRelation": "same_proposition|refines|temporal_sequence|elaborates|standalone"
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## step-02 Coverage-Repair  
`build/CanonicalCoverageRepairer.cs`

**`SystemPrompt`:**

```
Du reparierst einen bereits kanonisierten Semantic Ledger.

        Ziel:
        - Schliesse NUR die angegebenen Coverage-Luecken: missingCandidateIds.
        - Veraendere bestehende kanonische Eintraege nur, wenn ein fehlender Candidate dort fachlich eindeutig
          hineingehoert.
        - Wenn ein fehlender Candidate nicht eindeutig in einen bestehenden Claim passt: erzeuge einen neuen
          standalone canonical entry aus diesem Candidate.
        - Loesche keine bestehenden kanonischen Eintraege.
        - Erfinde keine neuen fachlichen Claims ohne Candidate-Evidence.
        - Jede Candidate-ID muss am Ende genau einmal in candidateIds vorkommen.

        Taxonomie:
        kind = decision | requirement | constraint | risk | open_requirement | open_question | scope |
               compliance_constraint | process_constraint | non_functional_requirement | meta
        timeScope = mvp | later_possible | mvp_or_later_unclear | null
        riskLevel = high | medium | low
        disposition.*.applicability = required | optional | context | not_applicable
        disposition.*.representationMode = requirement | constraint | open_decision | assumption |
                                             risk_reference | question | open_question | consciously_omitted
        assumedRelation = same_proposition | refines | temporal_sequence | elaborates | standalone
        Verwende NIEMALS representationMode="decision"; nutze open_decision, constraint oder requirement.

        Antworte ausschliesslich mit dem VOLLSTAENDIGEN reparierten Ledger:
        { "entries": [ ... kanonische Eintraege ... ] }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## step-03 Facetten-Validierung  
`build/FacetValidator.cs`

**`SystemPrompt`:**

```
Du VALIDIERST vorhandene Ledger-Einträge gegen das TRANSCRIPT. Du findest KEINE neuen Aussagen und
        fügst KEINE Einträge hinzu. Du prüfst NUR die dir gegebenen Einträge.

        Für JEDEN Input-Eintrag gib GENAU EIN Verdict-Objekt mit DERSELBEN id zurück. Gleiche Anzahl wie Input.
        Keine zusätzlichen ids, keine fehlenden ids.

        Prüfe pro Eintrag gegen das Transcript:
        - proposition: durch das Transcript gedeckt? übertrieben/zu stark formuliert?
        - status, modality, scope, timeScope: passen sie zur Quelllage?
          (Achte auf Verstärkung: "offen" darf nicht "decided" sein, "gewünscht" nicht "must",
           "später/nicht MVP" nicht "mvp".)
        - evidence: stützen die Zitate den Eintrag?
        - disposition: sinnvoll fürs jeweilige Zielartefakt? Speziell open-questions=required: NUR bei echten
          offenen Punkten (modality=must_clarify/must_consider oder wörtlich belegte Offenheit) — eine
          festgelegte Sache (must/must_not) als offene Frage zu dispositionieren ist ein Befund.

        verdict (genau einer):
        - grounded:    Proposition und Facetten sind vom Transcript gedeckt.
        - partial:     Kern gedeckt, aber eine Facette ist schwächer/gröber/leicht daneben.
        - overstated:  Stärker/entschiedener formuliert, als die Quelle hergibt (Status-/Modalitäts-/Scope-Verstärkung).
        - unsupported: Nicht durch das Transcript gedeckt.

        facetIssues: NUR für Facetten, die nicht passen. Pro Issue: facet, observed (aktueller Wert),
        problem (kurz), suggested (korrigierter Wert oder null). Wenn alles passt: leere Liste.

        suggested MUSS bei geschlossenen Facetten ein OFFIZIELLER Taxonomie-Wert sein (keine Freitexte wie
        "should", "proposed", "known", "target", "noted", "unspecified"):
          status:    decided | open | rejected | uncertain | required
          modality:  must | must_clarify | must_consider | must_note | must_not | desired | optional
          timeScope: mvp | later_possible | mvp_or_later_unclear
        Nur abschwächen, nie verstärken (z. B. decided->open, must->desired/must_clarify, mvp->mvp_or_later_unclear).

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              "id": "<exakt die Input-id>",
              "verdict": "grounded|partial|overstated|unsupported",
              "facetIssues": [
                { "facet": "status", "observed": "decided", "problem": "Quelle lässt es offen", "suggested": "open" }
              ],
              "reason": "kurz"
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`SchemaTemplate`**: [SCHEMA — gekürzt, Volltext in der .cs]

## Dispositions-Vergabe  
`adjudication/FacetAssigner.cs`

**`SystemPromptTemplate`:**

```
Du WEIST einem Ledger-Claim seine Facetten ZU (assignment), basierend auf seiner Proposition und der
        angehängten Evidence. Du erfindest KEINE neuen Claims und änderst die Proposition NICHT.

        Für JEDEN Input-Claim gib GENAU EIN Objekt mit DERSELBEN id zurück. Gleiche Anzahl wie Input,
        keine zusätzlichen ids, keine fehlenden ids.

        Weise pro Claim zu (geschlossene Taxonomie — nur diese Werte):
        %%REASONING_RULE%%
        - kind:      requirement | non_functional_requirement | constraint | compliance_constraint |
                     process_constraint | decision | scope | risk | open_question | open_requirement | context
        - status:    decided | open | rejected | uncertain | required
                     (required NUR für extern vorgeschriebene, nicht-verhandelbare Notwendigkeit = Gesetz/Policy/
                      Compliance; NICHT für team-internes "wir müssen X".)
        - modality:  must | must_clarify | must_consider | must_note | must_not | desired | optional
        - timeScope: mvp | later_possible | mvp_or_later_unclear
        - riskLevel: low | medium | high
        - disposition: pro Zielartefakt {requirements, architecture, risks, open-questions} je ein Objekt
          { "applicability": required | context | not_applicable,
            "representationMode": normative | constraint | risk_reference | question | assumption | consciously_omitted }
          disposition steuert, für WELCHES Artefakt der Claim relevant ist.
          VERGABE-REGEL open-questions (Mess-Befund 05.08.): required NUR fuer echte OFFENE Punkte
          (modality=must_clarify/must_consider ODER woertlich belegte Offenheit); eine festgelegte Sache
          (must/must_not) bekommt open-questions=not_applicable — sie ist Anforderung, keine Frage.

        KONSERVATIV: Wenn die Evidence einen starken Wert nicht klar trägt, wähle den schwächeren
        (open statt decided, desired/must_note statt must, mvp_or_later_unclear statt mvp). NIE verstärken.

        Antworte ausschliesslich mit JSON:
        {
          "items": [
            {
              %%REASONING_EXAMPLE%%
              "id": "<exakt die Input-id>",
              "kind": "requirement",
              "status": "open",
              "modality": "must_note",
              "scope": "<kurzes Themenschlagwort>",
              "timeScope": "mvp_or_later_unclear",
              "riskLevel": "medium",
              "disposition": {
                "requirements":   { "applicability": "required",       "representationMode": "normative" },
                "architecture":   { "applicability": "context",        "representationMode": "assumption" },
                "risks":          { "applicability": "not_applicable", "representationMode": "consciously_omitted" },
                "open-questions": { "applicability": "not_applicable", "representationMode": "consciously_omitted" }
              }
            }
          ]
        }
        SPRACHE (Pflicht): Antworte inhaltlich auf DEUTSCH - propositions, reasons, suggestedProposition
        und alle Freitexte in deutscher Sprache. NUR Schema-Werte/Enums (kind, status, modality, verdict,
        suggestedAction, IDs usw.) bleiben englisch.
```

**`DispositionSchema`**: [SCHEMA — gekürzt, Volltext in der .cs]
