# L3 CoverageSpec — Prüflinsen-Katalog (Instrument)

**Version 1 · 2026-07-14 · Strang: L3 Coverage/Gap (v10 → CoverageSpec-Ausbau)**

Dies ist das **Erwartungs-Instrument** für die L3-Abdeckungsprüfung — analog zur `qualitaetsrubrik.md` für den Jury.
Es definiert, *was* ein Requirements-Engineer in der frühen Planung abdecken würde, aufgeschlüsselt in **Prüflinsen**.
Der Katalog ist die **eine Quelle** für zwei Konsumenten (kein Drift):

1. **Generator-Prompt** (`L3CandidateGenCoverage*`): die Prompt-Beschreibung je Linse.
2. **`LensCoverageGate`** (deterministisch): der Repair-Hinweis + das „was zählt als adressiert"-Kriterium je Linse.

Die Linsen-`id` (kebab-case) **ist zugleich der `gapCategory`-Wert** im Kandidatenvertrag → der Gate rechnet Abdeckung
mechanisch aus den erzeugten `gapCategory`-Feldern, ohne LLM.

---

## 1. Zweck, Scope, Ehrlichkeits-Vorbehalt

- **Zweck:** L3 soll nicht „ein paar" Folge-Requirements liefern, sondern das **Gesamtbild systematisch absuchen** —
  bestehende Items erweitern UND unausgesprochene Themen/Lücken sichtbar machen. Der Katalog macht „systematisch"
  überhaupt erst prüfbar.
- **Scope:** **frühe SDLC-Planung / Elicitation** (Kickoff-/Stakeholder-Transkripte → frühe Artefakte), NICHT eine
  vollständige SRS. Daher liegt der Fokus auf den Dimensionen, die in der frühen Phase entschieden/geklärt werden —
  inkl. Scope, Stakeholder, Constraints, offene Entscheidungen (die eine reine Spät-SRS-Taxonomie unterschlägt).
- **Ehrlichkeits-Vorbehalt (thesis-tragend):** Der Katalog liefert **relative** Vollständigkeit — „vollständig
  **gegenüber diesem RE-Referenzrahmen**", NICHT absolute Vollständigkeit. Man kann alle Linsen abdecken und trotzdem
  das projektspezifische Killer-Requirement verfehlen, das keine generische Linse benennt. Das Gate prüft
  **Abdeckung-des-Rahmens**, nicht Abdeckung-der-Realität. Diese Grenze ist bewusst und wird so berichtet.
- **Abgrenzung zu bestehenden Checkern:** Der `SupportJudge`/`InferenceChecker` prüft **Treue** (trägt der Anker den
  Kandidaten?). Dieser Katalog + Gate prüfen **Abdeckung** (ist jede Dimension verrechnet?). Zwei getrennte Achsen.
- **Nicht Reqs, sondern Dimensionen:** Das Ziel ist nicht „produziere alle Reqs" (open-world, unprüfbar), sondern
  „**jede Dimension wurde berücksichtigt** und ist adressiert ODER explizit `N/A` mit Grund". Dimensionen sind
  aufzählbar, Reqs nicht.
- **Gate = BLIND-SPOT-DETEKTOR, kein Vollständigkeits-Beweis.** Eine LEERE Linse ist ein Signal, dass eine relevante
  Perspektive möglicherweise übersehen wurde → gezielter Repair-Pass. Eine NICHT-leere Linse bedeutet NUR „mindestens
  einmal berührt" — sie beweist NICHT, dass die Kategorie in ausreichender Tiefe untersucht wurde (ein Kandidat in
  `security-misuse` lässt Rechteausweitung, Session-Missbrauch, Datenabfluss, Auditierbarkeit … evtl. offen). „Alle
  Linsen adressiert" darf daher NIE als „alle wesentlichen Lücken gefunden" gelesen werden. Tiefe wird durch die Qualität/
  Vielfalt der Kandidaten (Prompt-Tiefenregel), die spätere Adäquanz-Schicht und die menschliche Pro-Linsen-Bewertung
  bestimmt — nicht durch den Existenz-Gate.

## 2. Quellen & Methode

Der Katalog konvergiert drei etablierte RE-Referenzen (Konsens statt Einzelquelle):

- **FURPS+** — explizit als *Vollständigkeits-Checkliste für Requirements-Coverage in der Elicitation* konzipiert:
  **F**unctional, **U**sability, **R**eliability, **P**erformance, **S**upportability **+** Constraints, Interface,
  Business Rules. Kompakt, deckungsgleich mit unserem Zweck.
- **ISO/IEC/IEEE 29148:2018** — der RE-Standard; feinere Klassen als funktional/nicht-funktional
  (functional/performance, quality, usability, interface, business/mission, stakeholder, constraints). Gibt dem Katalog
  normative Deckung.
- **Volere Requirements Shell** — steuert die **frühen** „external factors" bei: Stakeholder, Constraints,
  Abhängigkeiten, **offene Fragen/Annahmen**, Scope/Abgrenzung, Glossar.

**Methode:** je Linse ein Primär-Mapping auf ≥1 Referenz; frühe-Phase-Dimensionen (Scope, Stakeholder, Constraints,
open decisions) aus Volere/29148 explizit aufgenommen, auch wenn FURPS+ sie nur implizit führt.

**Änderungen ggü. dem ad-hoc-Katalog (v10-Run, 11 Linsen):**
1. **`security-misuse` wird Pflicht-Linse** (war im Run lautlos = 0 Kandidaten → genau die Lücke, die das Gate fangen soll).
2. **Stakeholder/Scope explizit** — vorher in `business-goals`/`actors-permissions` versteckt, jetzt in deren
   Beschreibung ausgewiesen.
3. **NEU: `constraints-compliance`** — rechtlich/regulatorisch/Standards. In der frühen Phase kritisch (bei einer
   Pflege-/Gesundheitsdaten-App z. B. DSGVO), vorher gar nicht abgedeckt. Quelle: FURPS+ Constraints + 29148 + Volere.

→ 12 Linsen (die 11 + `constraints-compliance`), `gapCategory`-Werte bleiben stabil (nur additiv).

## 3. Der Prüflinsen-Katalog

Je Linse: **Quelle** · **Generator-Beschreibung** (in den Prompt) · **Repair-Hinweis** (aus dem Gate bei leerer Linse) ·
**Adressiert-Kriterium** (für die spätere optionale Adäquanz-Schicht; der deterministische Gate v1 prüft nur Existenz ≥1).

### 3.1 `business-goals` — Fachliche Ziele, Mission & Scope
- **Quelle:** 29148 (business/mission) · Volere (purpose, scope/boundary) · FURPS Functional.
- **Generator:** Kernzweck, Nutzenversprechen und **Abgrenzung** (was gehört dazu, was ausdrücklich NICHT). Welche
  fachlichen Kernabläufe stiftet das System?
- **Repair-Hinweis:** „Ziele/Scope-Abgrenzung nicht adressiert — was leistet das System im Kern, und wo ist die Grenze?"
- **Adressiert, wenn:** ≥1 Kandidat benennt ein konkretes fachliches Ziel ODER eine Scope-Grenze (nicht nur eine Floskel).

### 3.2 `actors-permissions` — Stakeholder, Akteure, Rollen & Berechtigungen
- **Quelle:** 29148 (stakeholder requirements) · Volere (stakeholders) · FURPS+ Constraints (access).
- **Generator:** Wer nutzt/betrifft das System (Rollen, **Stakeholder**, Dritte wie Angehörige)? Welche Rolle darf was
  lesen/ändern/weitergeben?
- **Repair-Hinweis:** „Akteure/Rollen/Berechtigungen unklar — welche Stakeholder gibt es, und wer darf was?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine Rolle/Stakeholder ODER eine rollenbezogene Zugriffsregel.

### 3.3 `input-validation` — Eingaben, Validierung, Fehler- & Leerzustände
- **Quelle:** FURPS Functional/Reliability · 29148 quality (robustness).
- **Generator:** Pflichtangaben, Wertebereiche, Formatregeln, Verhalten bei fehlerhaften/unvollständigen/leeren Daten.
- **Repair-Hinweis:** „Eingabe-/Validierungsregeln fehlen — was passiert bei ungültigen oder fehlenden Eingaben?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine Validierungs-/Fehler-/Leerzustandsregel.

### 3.4 `state-transitions` — Zustände, Übergänge, Nebenwirkungen, Nebenläufigkeit
- **Quelle:** FURPS Functional · 29148 (system states, behavior).
- **Generator:** Relevante Objekt-/Vorgangszustände, erlaubte Übergänge, Nebenwirkungen, gleichzeitige/zeitversetzte
  Änderungen (Konflikte, Synchronisation).
- **Repair-Hinweis:** „Zustände/Übergänge/Nebenläufigkeit unklar — welche Zustände gibt es, und wie werden Konflikte behandelt?"
- **Adressiert, wenn:** ≥1 Kandidat benennt einen Zustandsübergang, eine Nebenwirkung oder ein Konflikt-/Sync-Verhalten.

### 3.5 `data-lifecycle` — Daten, Datenschutz, Aufbewahrung & Löschung
- **Quelle:** 29148 quality (data) · Volere (data) · DSGVO-Relevanz.
- **Generator:** Welche (sensiblen) Daten entstehen; wie lange werden sie gespeichert; wann/wie gelöscht, anonymisiert,
  exportiert, archiviert?
- **Repair-Hinweis:** „Datenlebenszyklus fehlt — wie werden Daten aufbewahrt, gelöscht, anonymisiert oder exportiert?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine Aufbewahrungs-/Lösch-/Export-/Anonymisierungsregel.

### 3.6 `security-misuse` — Sicherheit & Missbrauchsfälle **(Pflicht-Linse)**
- **Quelle:** FURPS Reliability + Constraints · 29148 quality (security) · risk-driven abuse cases.
- **Generator:** Plausible Missbrauchs-/Bedrohungsszenarien, Schutz vor unbefugtem Zugriff, Datenschutzverletzungen,
  Manipulation. (Zugriffs-*rechte* liegen in `actors-permissions`; hier die *adversarielle* Sicht.)
- **Repair-Hinweis:** „Sicherheit/Missbrauch nicht betrachtet — welche Bedrohungs-/Missbrauchsfälle sind plausibel, und
  wie schützt das System sensible Daten?"
- **Adressiert, wenn:** ≥1 Kandidat benennt ein Bedrohungs-/Missbrauchsszenario ODER eine Schutzmaßnahme.
- **Hinweis:** Diese Linse darf NICHT lautlos leer bleiben — leer ⇒ der Agent muss explizit `N/A` mit Grund erklären.

### 3.7 `external-integration` — Externe Systeme, Schnittstellen & Integrationen
- **Quelle:** FURPS+ Interface · 29148 (interface requirements) · Volere (external interfaces).
- **Generator:** Mit welchen bestehenden Systemen/Datenquellen muss zusammengearbeitet werden; lesend/bidirektional;
  Datenformate, Medienbrüche.
- **Repair-Hinweis:** „Integrationen unklar — mit welchen Systemen/Datenquellen arbeitet die Lösung zusammen und wie?"
- **Adressiert, wenn:** ≥1 Kandidat benennt ein externes System, eine Schnittstelle oder eine Integrationsanforderung.

### 3.8 `resilience-operations` — Ausfall, Wiederherstellung, Offline & Betrieb
- **Quelle:** FURPS Reliability + Supportability · 29148 quality (availability, recoverability).
- **Generator:** Verhalten bei Störungen (Cloud/Verbindung), Offline-Fähigkeit, Wiederherstellung, Betrieb/Wartung,
  Monitoring.
- **Repair-Hinweis:** „Ausfall-/Betriebsverhalten fehlt — was passiert bei Störungen, und welche Aufgaben bleiben möglich?"
- **Adressiert, wenn:** ≥1 Kandidat benennt ein Ausfall-/Offline-/Wiederherstellungs-/Betriebsverhalten.

### 3.9 `quality-attributes` — Performance, Verfügbarkeit, Skalierbarkeit
- **Quelle:** FURPS Performance + Reliability · 29148 (performance/quality).
- **Generator:** Mengengerüst (Nutzer/Daten/Einrichtungen), Antwortzeiten, Verfügbarkeit, Skalierungsziele.
- **Repair-Hinweis:** „Qualitätsziele fehlen — welche Größenordnung/Performance/Verfügbarkeit muss erreicht werden?"
- **Adressiert, wenn:** ≥1 Kandidat benennt ein messbares Qualitäts-/Mengen-/Performanceziel (oder den Klärungsbedarf dazu).

### 3.10 `usage-context` — Geräte-, Zugriffs- & Nutzungskontext, Bedienbarkeit
- **Quelle:** FURPS Usability · 29148 (usability) · Volere (usability, users).
- **Generator:** Einsatzbedingungen (mobil, Schichtbetrieb, Umgebung), Bedienbarkeit im realen Arbeitskontext,
  Barrierefreiheit.
- **Repair-Hinweis:** „Nutzungskontext fehlt — unter welchen realen Bedingungen muss die Lösung zuverlässig bedienbar sein?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine Einsatzbedingung oder ein Bedienbarkeits-/Zugänglichkeitskriterium.

### 3.11 `constraints-compliance` — Rand-/Rahmenbedingungen, Recht, Regulatorik, Standards **(NEU)**
- **Quelle:** FURPS+ Constraints · 29148 (constraints) · Volere (constraints, relevant facts/regulations).
- **Generator:** Rechtliche/regulatorische Vorgaben (z. B. DSGVO, Gesundheits-/Pflegevorschriften), einzuhaltende
  Standards, technische/organisatorische Zwänge (Plattform, Budget, Fristen).
- **Repair-Hinweis:** „Rahmenbedingungen/Compliance nicht betrachtet — welche rechtlichen, regulatorischen oder
  technischen Zwänge gelten?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine rechtliche/regulatorische/standard-/plattformbezogene Randbedingung.

### 3.12 `open-decisions` — Ungeklärte Annahmen, offene Entscheidungen & Abhängigkeiten
- **Quelle:** Volere (open issues, assumptions, dependencies) · 29148 (assumptions/dependencies).
- **Generator:** Was ist noch nicht entschieden, worauf beruhen unbelegte Annahmen, welche externen Abhängigkeiten
  bestehen? (Als Requirement + `requiresHumanDecision=true` formulieren, keine unbelegte Muss-Anforderung.)
- **Repair-Hinweis:** „Offene Entscheidungen/Annahmen nicht ausgewiesen — welche Klärungen stehen noch aus?"
- **Adressiert, wenn:** ≥1 Kandidat benennt eine offene Entscheidung, Annahme oder Abhängigkeit (mit Klärungsbedarf).

## 4. Wie der Katalog konsumiert wird (Vorschau Schritt 2–4)

- **Schritt 2 — `CoverageSpec` (Code):** dieser Katalog wird ein injizierbares Objekt (Analogon zu `DerivationSpec`).
  Es liefert je Linse `id` · `generatorDescription` · `repairHint` · `addressedCriterion`. Der Generator-Prompt rendert
  die `generatorDescription`-Liste; der Gate nutzt `id` + `repairHint`.
- **Schritt 3 — `LensCoverageGate` (deterministisch):** zählt je Linse die Kandidaten mit `gapCategory=id`.
  `addressed` (≥1) · `explicitlyNA` (Agent hat Linse mit Grund als N/A markiert) · sonst `unaddressed`. Report =
  `unaddressedLenses[]`. **Kein LLM**, reproduzierbar, nicht gamebar auf Existenz. Mirror von `DerivationCoverage`.
- **Schritt 4 — Repair-Schleife (`ReflectGraph`-Muster), zwei Modi:**
  - `measure`: Gate stempelt nur (kein Loop) → misst native Breite. Aussage: „Agent deckt X von sich aus ab."
  - `repair`: `unaddressedLenses` → gezielter zweiter Pass, Repair-Hinweise aus dem Spec; Agent darf `N/A`+Grund geben.
    Aussage: „mit N Repair-Runden erreicht das System Abdeckung X." **Delta measure↔repair = der Befund.**

## 5. Offene Punkte

- **Adäquanz-Schicht (zurückgestellt):** der Gate v1 prüft nur Existenz ≥1 je Linse (fängt „lautlos leer", ist aber auf
  Existenz gamebar via Alibi-Kandidat). Das `addressedCriterion` je Linse ist bereits notiert, damit eine spätere,
  optionale, **ehrlich als LLM-Urteil markierte** Adäquanz-Bewertung „substanziell vs. Alibi" ansetzen kann.
- **Katalog-Validierung:** ob 12 Linsen die frühe Phase gut treffen, ist selbst empirisch zu prüfen (Test-Retest über
  mehrere Transkripte; ggf. Linse zusammenlegen/spalten). Der Katalog ist Version 1, nicht endgültig.
- **Domänen-Bindung:** die Beschreibungen sind bewusst generisch (Pflege-App-Beispiele nur illustrativ). Für andere
  Domänen bleibt der Katalog gültig; nur die illustrativen Beispiele wären zu tauschen.

---

## Quellen

- FURPS / FURPS+ (Grady, HP; als Coverage-Checkliste): <https://en.wikipedia.org/wiki/FURPS> ·
  Larman, *Understanding Requirements*: <https://sites.cs.ucsb.edu/~mikec/cs48/project/RequirementsLarman.pdf>
- ISO/IEC/IEEE 29148:2018 — Requirements engineering: <https://standards.ieee.org/ieee/29148/6937/> ·
  Templates: <https://www.reqview.com/doc/iso-iec-ieee-29148-templates/>
- Volere Requirements Specification Template: <https://www.volere.org/templates/volere-requirements-specification-template/>
