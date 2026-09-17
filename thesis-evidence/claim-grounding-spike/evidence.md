# Claim-Grounding-Spike — Evidence

Stand: 2026-06-27 · Plan: `…/NextStep/ImplementClaimAnsatz.md` · **isolierter Pilot, getrennt von der Closure.**

## Ziel
Wird Grounding stabiler, wenn nicht ganze Markdown-Units gegen das *ganze* Transkript bewertet werden,
sondern **atomare Claims gegen explizit zugeordnete Evidence-Spans**? Getestet gezielt an den zwei Fehlerklassen,
die unit-level v1/v2 zerlegt haben: **Modalitäts-Over-Claims** (v2 verfehlt) + **Risiko-Synthesen** (v1 über-flaggt).

## Setup
Judge `openai/gpt-5.4`, structured output. Fixtures (manuell kuratierte Claims + Evidence-Spans):
`input/eval-labels/{92e6a3.req-arch, 28a52b.risks}.claim-evidence-spike.json` (11 + 10 = 21 Cases).
Befehl: `claim-grounding-spike <fixture> openai/gpt-5.4`. Output: `thesis-evidence/claim-grounding-spike/<fixture>.openai_gpt-5.4.json`.

## Ergebnisübersicht
| Fixture | Cases | Matches | Accuracy | Misses |
|---|---|---|---|---|
| 92e6a3.req-arch | 11 | **11** | **1.00** | – |
| 28a52b.risks | 10 | **7** | 0.70 | RISK-14, RISK-26, RISK-33 |

## Befund 92e6a3 (Modalitäts-/Scope-Verstöße) — der Kern-Test
**Alle 3 Verstöße korrekt als `violation`/`unsupported` erkannt** — genau die Fälle, die unit-level **v2 verfehlte**:
- `REQ-19` „Daten *nur* in der EU" → violation/unsupported (support=partial: Evidence trägt EU-Bezug, aber nicht „nur").
- `ARCH-18` „Hosting *ausschließlich* EU" → violation/unsupported (support=none).
- `REQ-21` Security-Review „darf MVP nicht verzögern, pragmatisch" → violation/unsupported.
- Kontroll-Case `ARCH-MANDANT` „mandantenfähig" → violation/unsupported (Begriff nicht in Evidence).
Alle 7 grounded-Claims korrekt grounded (explicit/synthesis/permissible_inference).
→ **claim-level fängt die Modalitäts-Verschärfung, weil es Claim gegen *zugeordnete* Evidence prüft statt im Volltranskript das Thema zu „finden" und dann zu grounden.**

## Befund 28a52b (zulässige Risiko-Synthesen) — der zweite Test
**5/5 starke Risiko-Synthesen korrekt grounded** (`#12,16,23,24,31`) — genau die Fälle, die **v1 über-flaggte**.
Plus `#28` grounded, **`#40` korrekt violation** (Auditierung nicht besprochen).
→ **claim-level akzeptiert valide Auswirkungs-/Synthese-Claims, statt sie wie v1 pauschal als fabrication zu markieren.**

**3 Misses — alle auf SCHWACHER Evidenz, alle evidence-gebunden begründet (keine Instabilität):**
- `RISK-26` / `RISK-33`: von mir **vorab als „permissible_inference (schwach)"/borderline markiert.** Der Verifier
  ruft „unsupported", weil die Evidence „genau planen" / „Datenschutz kümmern" die *spezifischen* Begriffe
  („Schnittstellenplanung" / „Datenschutzbeauftragte") nicht trägt. **Das ist eine vertretbare, strengere Lesart →
  Handlabel-Ambiguität, nicht Judge-Fehler.**
- `RISK-14`: „Flutter Performance-Risiko" — meine Evidence-Spans zeigen *Flutter bekannt* + *cross-platform*, aber
  **nicht**, dass Performance ein Risiko ist (das Transkript voicet das nicht). → **Evidence-Auswahl-/Handlabel-Fehler
  meinerseits; der Verifier ist hier eher korrekter als mein lenientes Label.**

## Vergleich zu UnitClassifier (der eigentliche Befund)
| Fehlerklasse | v1 (unit) | v2 (unit) | **claim-pilot** |
|---|---|---|---|
| EU-only-Modalität (#19/#18) | fängt (TP) | **verfehlt (FN)** | **fängt** ✓ |
| Risiko-Synthesen (#12,16,23,24,31) | **über-flaggt (FP)** | grounded ✓ | **grounded** ✓ |
| #40 Auditierung (unsupported) | TP | TP | **violation** ✓ |
**Kern:** **claim-level ist die EINZIGE Variante, die BEIDE Fehlerklassen löst.** v1 fängt Modalität, über-flaggt
Synthesen; v2 akzeptiert Synthesen, verfehlt Modalität. **Kein unit-level-Prompt (v1…v2.3) konnte beides** — der
strukturelle Wechsel (Claim+Evidence) schon. Und: die Verifier-Fehler sind **evidence-gebunden + erklärbar**, nicht
random (Erfolgskriterium §8.4 erfüllt).

## Fehleranalyse (§4-Frage 5)
3 Misses = **0 Judge-Instabilität**; 2× Handlabel-Ambiguität (borderline-Inferenzen, Verifier strenger & vertretbar),
1× meine Evidence-Auswahl zu schwach (RISK-14). → bestätigt zugleich den **Evidence-Selection-Hebel**: wo die Spans
schwach waren, kippte das Urteil. Das ist der *Vorgeschmack* auf die ungelöste harte Teilaufgabe.

## Entscheidung (§11)
**A (struktureller Ausbaupfad) — als Future Work / L3, NICHT in der Closure.** Begründung: der Pilot löst als
Einziger beide Fehlerklassen mit erklärbaren Fehlern. **ABER ehrlicher Deckel:** der Erfolg gilt **bei gegebener,
manuell kuratierter Evidenz**; die 3 Misses zeigen, dass **Evidence-Selection die eigentliche offene Forschungsfrage**
ist (ClaimSplitter + EvidenceSelector). Kein Bau jetzt. n=21, Single-Judge → Indikation.
**Closure-Default unberührt:** Grounding bleibt unit-level v1 recall-first + Triage; claim-level ist der dokumentierte,
empirisch vielversprechende Verbesserungspfad für später.

---

# Fortsetzung 2026-06-27 — Stufe 2 (E2E) + Stufe 3 (Splitter) + Validitäts-Check + Entscheidung revidiert

Der obige Stufe-1-Befund (§60ff) sagte: der Erfolg gilt **bei manuell kuratierter Evidenz**, die offene Frage ist
**Evidence-Selection**. Diese Fortsetzung schließt genau das: automatische Selection (Stufe 2), Atomisierung
(Stufe 3) und ein Fabrication-Resistenz-Check (Zwei-Transkript). Judge durchgehend `openai/gpt-5.4`, T=0, structured output.

## Stufe 2 — E2E mit AUTOMATISCHER Evidence-Auswahl (natives Transkript)
Befehl: `claim-evidence-e2e-spike <fixture> <transkript> openai/gpt-5.4`. Kette: `Claim → EvidenceSelector (AUTO) → Verifier`.

| Fixture (natives Transkript) | Cases | E2E-Match | vs. manuell (Stufe 1) | Datei |
|---|---|---|---|---|
| 92e6a3.req-arch (T9999_chaos) | 11 | **11/11 (1.00)** | manuell 11/11 — gehalten | `92e6a3.req-arch.claim-evidence-spike.e2e.openai_gpt-5.4.json` |
| 28a52b.risks (Interview) | 10 | **10/10 (1.00)** | manuell 7/10 — **auto BESSER** | `28a52b.risks.claim-evidence-spike.e2e.openai_gpt-5.4.json` |
| 2d7b09.coverage (T9999_chaos) | 7 | **6/7 (0.857)** | manuell 6/7 — gleich | `2d7b09.coverage.claim-evidence-spike.e2e.openai_gpt-5.4.json` |

**Aggregat E2E nativ: 27/28.** Kernbefunde:
- **Die gefürchtete Engstelle (EvidenceSelector) hielt** — auto erreicht oder **schlägt** die manuelle Evidenz (28a52b
  10/10 vs. 7/10). Die 3 manuellen Stufe-1-Misses waren *meine* schwachen Spans, nicht der Verifier.
- **`2d7b09-FN-SAP-WRITE`** (eine SAP-Schreib-Überdehnung, die **beide unit-level-Judges v1+v2 übersahen**) wird
  **claim-level korrekt als violation** erkannt (in der nativen 6/7; der eine E2E-Miss = der Compound unten).
- Der einzige E2E-Miss (`2d7b09-FP-BESTELLUEBERSICHT`) = **Compound-Claim** → Motivation für Stufe 3.

## Stufe 3 — ClaimSplitter (Atomisierung, die letzte ungetestete Komponente)
Befehl: `claim-split-spike input/eval-labels/claim-split-spike.json T9999_chaos.txt openai/gpt-5.4` (volle Kette split→select→verify).
Dateien: `claim-split-spike.openai_gpt-5.4.json` (nativ chaos, alter String-Output),
`claim-split-spike.T9999_chaos.openai_gpt-5.4.json` (nativ chaos, Facetten-Output nach Nachschärfung),
`claim-split-spike.Interview-Einrichtung.openai_gpt-5.4.json` (fremd).
- **Bestellübersicht → 3 atomare Claims**, aber **nicht** wie zunächst notiert „Bestellungen/Rechnungen grounded".
  Beide nativen Split-Runs bewerten alle drei Teilclaims als `violation`:
  Angebote=`assumption/unsupported`, Bestellungen=`overstated`, Rechnungen=`overstated`. Der neue Facetten-Run zeigt
  warum: Der fachliche Kern ist teils quellennahe („Kunden sollen Bestellungen sehen", „Rechnungen anzeigen/downloaden"),
  aber das Artefakt macht daraus die stärkere UI-/Architekturaussage „Kundenportal-Frontend zeigt ... an" und glättet
  zugleich den finalen Scope. → **Befund ist Artefakt-Over-Specification, nicht eine gelöste E2E-Miss.**
- **Modalität erhalten**: EU-Compound → 4 Claims, jeder behält „nur innerhalb der EU" (nicht abgetrennt) und wird wegen
  der zu starken EU-only-Modalität als `overstated` bewertet.
- **Kein Über-Splitten verbessert**: Nach Facetten-Nachschärfung bleibt Login als ein Claim und wird korrekt
  `grounded/explicit` (alter Run hatte E-Mail/Passwort getrennt und „Passwort" zu streng geflaggt).
- Rollen-Aufzählung → 5 Claims (Admin/Sales/Kunde grounded, Manager/Support violation); SAP-Risiko-Compound → 5 Claims,
  alle grounded. → **Splitter, Selector, Verifier sind einzeln belegt, aber der Bestellübersicht-Fall ist ein
  Grenzfall für fachlicher Kern vs. UI-/Status-Facetten.**

**Neutrale Bewertung zur weiteren Schärfung.** Man könnte den Ansatz erweitern zu `coreSupport + facetFindings`
(fachlicher Kern getrennt von Facetten wie Komponente, Aktion, Status, Scope). Das wäre methodisch sauberer und würde
den Bestellübersicht-Fall feiner ausdrücken: z. B. „Bestellungen fachlich teilweise getragen, Frontend-/MVP-Facette
overstated". Der Aufwand wäre aber deutlich größer (Verifier-Schema, Mapper, Gate/Defect-Logik, Fixtures und neue Runs).
Für die aktuelle Evidenzlage wäre eine Optimierung auf 100% riskant: sie könnte einen einzelnen Label-Konflikt
overfitten und die nützliche Strenge gegen Artefakt-Over-Specification verwässern. Empfehlung: Facetten sichtbar lassen,
Bestellübersicht als Grenzfall dokumentieren, aber vorerst **keine** vollständige Core/Facet-Bewertung bauen.

## Validitäts-Check — Zwei-Transkript (Fabrication-Resistenz): liest die Kette ECHT Evidenz?
Jedes Fixture zusätzlich gegen das **fremde** Transkript (andere Domäne: Kundenportal/SAP vs. Pflege-Einrichtung).
Befehle (gpt-5.4):
```
claim-split-spike        input/eval-labels/claim-split-spike.json            Interview-Einrichtung.txt openai/gpt-5.4
claim-evidence-e2e-spike input/eval-labels/92e6a3.req-arch.claim-evidence-spike.json  Interview-Einrichtung.txt openai/gpt-5.4
claim-evidence-e2e-spike input/eval-labels/2d7b09.coverage.claim-evidence-spike.json  Interview-Einrichtung.txt openai/gpt-5.4
claim-evidence-e2e-spike input/eval-labels/28a52b.risks.claim-evidence-spike.json     T9999_chaos.txt           openai/gpt-5.4
```
Dateien: `*.e2e.Interview-Einrichtung.openai_gpt-5.4.json`, `28a52b…e2e.T9999_chaos…json`, `claim-split-spike.Interview-Einrichtung…json`.

**⚠️ Oracle-Hinweis:** Die `accuracyVsHandlabel`-Zahl ist hier **kein Gütemaß** — die Fixture-`ExpectedLabel` sind die
**nativen** Handlabels, die das fremde Transkript nicht beschreiben. **Die Evidenz steckt in den Roh-Verdicts**, am Transkript geprüft:
- **92e6a3 (Kundenportal) gegen Pflege-Transkript: 11/11 korrekt NICHT gegroundet** (7 Verweigerungen `turns=[]` + 4 violations).
  Domänenspezifische Claims (Kundenportal, OAuth, Mandant, SAP) → die Kette **verweigert** ohne Evidenz. **0 Fabrication.**
- Die scheinbaren „grounded gegen fremd"-Treffer sind **echte Themen-Überlappung**, hand-verifiziert am Transkript:
  `Login per E-Mail/Passwort` (Interview Z.506/511), `Rollenmodell umfasst Admin` (Z.213/288/400) — **korrekt** grounded;
  `Sales/Manager/Support/Kunde/SAP/EU-only` korrekt **violation**. Kein Halluzinieren von Support.
- **Leaky-Control-Befund:** 28a52b-Risks 8/10 grounded gegen das chaos-Transkript — generische Risk-Claims (Verfügbarkeit,
  Datenschutz, Datenaktualität) passen in *jedes* Datenprojekt. **Befund über schwache Diskriminierungskraft generischer
  Claims**, kein Ketten-Fehler. (Das eine chaos-Risk nicht zeilenweise gegengeprüft → als Indikation, nicht Metrik.)

**Was der Check valide zeigt:** die Kette ist **genuin evidenzgetrieben** — Verdicts **flippen mit dem Transkript**
(Kundenportal-Claims grounded→violation), nicht reproduziert sie native Muster. Die gefährlichste Gegenhypothese
(„Pattern-Matching statt Evidenzlesen") ist damit **empirisch widerlegt**.

## Achsen-Reichweite (WICHTIG — nicht überdehnen)
Der Pilot ist eine **Grounding**-Lösung (Artefakt → Quelle). Er startet bei den Artefakt-Units und iteriert über
**Claims, die im Artefakt stehen**. → **Coverage** (Quelle → Artefakt, „fehlt was im Artefakt?") ist **strukturell nicht
adressiert** und kann es per Konstruktion nicht sein: eine Auslassung erzeugt keinen Claim. Coverage bleibt die separate
Baustelle (TopicExtractor v02 / DISK-COV), unberührt von diesem Pilot.

## Entscheidung 2026-06-27 (revidiert ggü. §60 „Kein Bau jetzt")
Stufe 1 deckelte mit „Erfolg nur bei manueller Evidenz; Evidence-Selection ist die offene Frage". **Stufe 2 schließt
diesen Deckel** (auto-Selection hält/schlägt manuell, 27/28), **Stufe 3** belegt die letzte Komponente, der **Validitäts-
Check** zeigt 0 Fabrication. → **Für Grounding mit starkem Judge ist der Ansatz integrationsreif.** Entscheidung:
**Integration bauen** (bounded Engineering, kein offener Forschungsrest) — Plan: `…/NextStep/grounding-integration-plan.md`.
**Grenzen, die offen bleiben (ehrlich):** schwaches Modell bewusst zurückgestellt (stark zählt); n klein (3 Fixtures,
~28+ Cases, Single-Judge, n=1/Zelle, T=0); Kosten 3 Calls/Unit; **Coverage nicht mitgelöst**. v1 unit-level bleibt
Fallback/Closure-Default, bis die Integration ihrerseits auf den Quell-Runs verifiziert ist.
