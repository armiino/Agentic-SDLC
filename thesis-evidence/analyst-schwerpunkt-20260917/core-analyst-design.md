# Design-Beschluss: Core-Analyst + Anforderungsdokument („Agent für das Ungesagte")

> Status: LEBEND (Design-Beschluss ⚖ Autor 19.08.2026 — Bau VOR dem Einfrieren; Nachfolge-Diskussion offen, s. §6)
> Herkunft: Parkplatz 9f (04./05.08.) + Autor-Session 19.08. („B ist Teil meiner Thesis — W2 ist Nachweis, kein Zweck").
> Subsumiert: Genealogie §5 #2 (Kapitel-Adequacy-Audit) · L3-Open-World-Erbe („nicht so wie gewollt").

## 1. Warum (Problem · Relevanz · Timing · Trade-off)

**Problem:** Das System deckt die frühen RE-Tätigkeiten für das GESAGTE vollständig ab (Gewinnung/Analyse/
Dokumentation/Prüfung mit Evidenz-Treue) — aber die Rolle des erfahrenen Requirements Engineers, der
Checklisten und Domänen-Muster gegen den Bestand hält und sagt **was FEHLT**, ist unbesetzt. Nach drei
Meetings gibt es 100 belegte REQs und niemanden, der „euch fehlen NFRs / der Absage-Fall / die
Rechte-Story" sagt.
**Relevanz (Thesis-Kern):** Die Forschungsfrage ist „Agenten in frühen SDLC-Phasen einsetzen". Der Graph
beweist Agenten für das Gesagte (Treue), der Analyst beweist sie für das UNGESAGTE (Kreativität) — die
inhärent nicht-deterministische Aufgabe = der stärkste Agenten-Use-Case. Macht das Graph-vs-Agent-Endbild
symmetrisch: **deterministische Orchestrierung wo Evidenz ist · agentische Exploration wo keine ist —
beide unter DERSELBEN Governance.**
**Timing:** vor dem Einfrieren als eigenes, klein geschnittenes Vorhaben (kein W2-Messgegenstand — die
Meeting-Kette bleibt unberührt; optionaler späterer Messarm „Elicitation-Vollständigkeit" möglich).
**Trade-off:** bewusst v1-klein (1–2 Sessions) statt der vollen 9f-Vision; alles Weitere nach W2.

## 2. Die zwei Vollständigkeits-Begriffe (die Grenze, die NIE verwischen darf)

- **Vollständig relativ zum GESAGTEN** — garantiert die Kette heute (jedes Item → Transkript-Zitat/Diktat).
- **Vollständig relativ zum PROBLEM** — kann niemand garantieren; der Analyst NÄHERT sie an.
Analyst-Items sind **Hypothesen mit eigener Herkunft** (`CoreAnalyst`), bis der Autor sie am Tor adoptiert —
die Evidenz ist dann ehrlich „Autor-Freigabe auf Analyst-Vorschlag", nie „aus dem Meeting".
(Zwei-Quellen-Prinzip 05.08.: GESAGT = Evidenz-Spur · ERSCHLOSSEN = Bestands-Spur; beide je Aspekt in EIN Zuhause.)

## 3. Endbild v1 (beschlossen 19.08.)

**A) Anforderungsdokument = Core-Projektion** (Muster: GitHub-Issues, architecture.md, ADR-Index):
deterministischer Renderer aus dem Core → Markdown; Steward-Fähigkeit („erstelle das Anforderungsdokument")
+ CLI. Struktur professionell (s. §4-1). Das Dokument weist seine eigene Offenheit aus: Abschnitt
„Offene Entscheidungen/Fragen" (DEC-Topf) + optional „Lücken-Hypothesen des Analysten" (offene Vorschläge).

**B) Core-Analyst = Maker-Agent, KEINE neue Kette:**
- **Lese-Tools** auf den ganzen Core (REQs, ARCHs, PBIs, Features, DECs, Rejections) + Checklisten-Prompt.
- **Output = Statements im Autor-Front-Format** (disposition requirement|architecture|question) mit
  Pflicht-Begründung („abgeleitet aus REQ-12/FC-3: Lebenszyklus-Loch …") + Herkunft `CoreAnalyst`.
- **Weg in die Wahrheit = der EXISTIERENDE `--from-delta`-Eingang** → Tor 1 (req + arch) → alle Gates →
  Kangal. Governance byte-identisch; der Analyst ist nur ein neuer PRODUZENT am bestehenden Eingang
  (Zwei-Bahnen-Regel: geteilte Naht, deklarierter Besteller).
- **Aufruf = bewusster Akt über den Steward** („analysiere den Core auf Lücken") — NICHT in Reihe der
  Meeting-Kette (Vollständigkeit ist Eigenschaft des Core-STANDES; Scope-Überraschungs-Lektion Catch-up).
  Der Steward DARF nach Läufen im Kompass eine Analyse-Runde ANBIETEN (Hinweis in Reihe, Arbeit auf Zuruf).
- **Bediener-Clou:** die Analyst-FRAGEN (9g-DEC-Rampe, existiert) sind die Agenda fürs nächste
  Stakeholder-Meeting — der SDLC-Kreis schließt sich (Meeting → Core → Analyst → Fragen → Meeting).

## 4. Die zwei Autor-Fragen vom 19.08. (beantwortet als Design-Punkte)

**4-1 · NFRs „sauber und professionell":** v1 führt KEINEN neuen Aspekt ein (Ausblick bleibt), aber die
Klassifikation wird EXPLIZIT: jedes Analyst-Statement trägt ein Kategorie-Metadatum
`analystKategorie: functional | nfr:<merkmal> | process`, Merkmale ISO-25010-nah (performance, security,
privacy, availability, usability, maintainability, compliance). Damit gilt: NFR-Items reiten die
vorhandenen Bahnen (Anforderung ODER Rahmen/constraint — die A3-Mechanik trägt sie in die Issues), und
das **Anforderungsdokument rendert professionelle Abschnitte**: 1. Funktionale Anforderungen (je Feature) ·
2. Nicht-funktionale Anforderungen (je Qualitätsmerkmal) · 3. Technische Rahmenbedingungen (ARCH-
constraints) · 4. Offene Entscheidungen/Fragen · 5. (optional) Lücken-Hypothesen. Sauber heißt: die
NFR-Natur steht als Metadatum IN der Wahrheit und strukturiert das Dokument — nicht als vager Fließtext.
**⚖ offen:** ob `analystKategorie` später auch für Meeting-REQs nachgezogen wird (Dokument-Qualität).

**4-2 · Architektur:** Der Analyst ist **aspekt-übergreifend** — er sieht den GANZEN Core und darf alle
drei Dispositionen vorschlagen (fehlender Rahmen → architecture; fehlende Persistenz-Entscheidung →
question/design). Die Autor-Front trägt das heute schon (disposition architecture → 8. Gate).
**Befund zur Steward-Arch-Fähigkeit heute (code-verifiziert 19.08.):** Tor 1 arch = Chat ✓ (Diktat,
arch-ingest-gate) · arch-Sweep ✓ (collect_arch_katalog) · ABER die arch-FOLGE-Gates (arch-classify-gate,
adr-gate) sind CLI/UI-Grenzen (open_gate_ui nennt nur den Standalone-Befehl) — das Autor-Gefühl „der
Steward kann nur req" kommt DAHER. → eigener ⚖ in §6 (9k(c)-Durchstich).

## 5. Bauplan v1 (Reihenfolge)

### 5.0 Technische Gestalt (Autor-Frage 19.08. „Workflow oder Agent?")

**Beides — nach dem Kernprinzip des Projekts:** „Der Analyst" ist die FÄHIGKEIT; technisch ein kleiner
deterministischer MAF-WORKFLOW, dessen Maker-Knoten Agenten sind (Muster: Ledger-Kapsel, Classify-Strip):
`[Collect det.] → Fan-out [4 Linsen-Maker] → Fan-in [Merge/Dedup det.] → [Checker det. → Repair] →
[KRITIKER (LLM-Richter)] → [Persist: Funde-Report + Delta]`. Der Analyse-Lauf hat KEIN Gate (er mutiert
nichts — nur Lese-Zugriff + Artefakte); die Wahrheits-Wirkung kommt ausschließlich im getrennten Tor-Lauf.
**Kritiker-Stufe (Autor-Frage 19.08. „gibt es eine Instanz, die Rauschen entfernt?"):** der det. Checker
prüft nur FORM (Core-Anker, Kategorie) — Substanz prüft ein zweiter, adversarialer LLM-Aufruf über die
gemergten Funde („ist das projektspezifisch abgeleitet oder generische Floskel? Würde ein RE das dem Team
vorlegen?") — Präzedenz: Cluster-Kritiker (R-33). Vom Kritiker Aussortiertes erscheint im Report unter
„aussortiert (Kritiker, Grund)" — kein stiller Cap; ins Delta geht nur, was Checker UND Kritiker passiert.
Rauschen-Trichter damit vierstufig: Form-Checker → Substanz-Kritiker → Human-Gate (Tor 1) → R-35-Gedächtnis.
**Such-Algorithmus zweischichtig** („durchsucht er jedes Mal alles?" — ja zum Anspruch [Vollständigkeit =
Eigenschaft des GANZEN Standes, darum bewusster ⚿-Akt], nein zur Methode):
① **Deterministische Kollektoren** (LLM-frei; erweitert das bewiesene ArchGapCollector-Muster): berechenbare
Lücken-Arten (PBIs ohne Rahmen · design ohne ADR · REQs ohne deckendes PBI · Features ohne NFR-Berührung)
+ kompakter Core-Digest (Projektion: IDs/Texte/Feature/Relations-Zähler — bei ~220 Items token-tauglich).
② **Linsen-Maker** erhalten Digest + Kollektor-Funde im Prompt UND die bestehenden Lese-Tools
(CoreQuery-Naht) für gezielte Tiefen-Blicke — agentisches Nachbohren statt Blind-Lesen.
Erweiterbar: `scope`-Parameter (z. B. je Feature) = triviale Collect-Erweiterung, kein Umbau.

**Blick vs. Gedächtnis (Autor-Fragen 19.08.):** Der Analyse-BLICK (Digest) = NUR die AKTIVE Wahrheit
(gilt heute; inkl. offener DECs + needs_clarify — abgelöste Versionen/Historie nicht). Das
DEDUP-GEDÄCHTNIS = auch das Inaktive (aufgelöste DECs „schon geklärt", REJ „schon abgelehnt" — R-35).
**Analyse-Gedächtnis ist ERGEBNIS-basiert, kein Item-Marker und KEIN privates Agent-Memory:**
① übernommen ⇒ im Core (Lücke zu, Dedup sieht's) · abgelehnt ⇒ REJ · vertagt ⇒ DEC — alles auditierbar
im Blackboard. ② Ein „analysiert"-Marker je Item wäre falsch (Lücken sind RELATIONAL — ein neues Item
kann eine Lücke an einem längst geprüften aufreißen; Autor-Gegenbeispiel 19.08.). ③ Privates MAF-Memory
am Analysten = dieselbe Falle versteckt im Agenten + eine zweite, unauditierbare Wissensquelle mit
Core-Drift-Risiko + Replay-Bruch; frische Augen sind ein Feature (kein Ankern auf alten Schlüssen).
Der Steward hat Memory, weil er GESPRÄCHSPARTNER ist; der Analyst ist Batch-Arbeiter.
④ Wiederfinden einer IGNORIERTEN Lücke ist KORREKT (sie existiert ja noch) — Komfort dafür: der Fan-in
vergleicht gegen den VORGÄNGER-Report und teilt Funde in **NEU vs. WEITERHIN OFFEN (seit <Datum>)**.
**Skalierungs-Pfad (Projekt wächst):** heute Digest-Volltext (~200–500 Items token-tauglich); darüber
liefert Collect Feature-Summaries + die Analyse fannt je Feature aus (`scope` wird vom Komfort- zum
Skalierungs-Mechanismus) — Architektur bleibt, nur Digest-Dichte ändert sich. Einordnung: die meisten
Stufen sind LAUF-lokal; der Analyst ist der erste Voll-Core-LLM-Konsument (deshalb steht der Pfad hier).
**Dokument-Ehrlichkeit:** solange der Kategorien-Nachzug (§6) aussteht, weist das Req-Dokument aus:
„NFR-Sektion enthält die kategorisierten Items; Kategorisierung des Alt-Bestands steht aus."
**CLI-Parität (Autor-Einwand 19.08., K13-Regel):** geteilter Runner-Kern → CLI `core-analysis run` UND
Steward-Seil auf DIESELBE Naht (Steward-only wäre ein Paritäts-Bruch der drei Bedienwege gewesen).

1. ✅ **GEBAUT 19.08.** — **A/Renderer:** `RequirementsDocumentProjection` (det., aus Core; Struktur §4-1) + CLI + Steward-Tool
   (read-only; Sandbox-Probe). Pins: Abschnitts-Struktur, NFR-Gruppierung, Offenheits-Abschnitt.
   **Kopfzeile (Autor 19.08.): Version + Stand** — `Version N` (fortlaufend, aus der Kopfzeile der
   Vorgänger-Datei +1) · `Stand: <generatedUtc>` · Core-Fingerabdruck (Item-Zahl + Snapshot-Hash) —
   damit ist jede Dokument-Fassung eindeutig einem Wahrheits-Stand zuordenbar (Beleg-Tauglichkeit).
2. ✅ **GEBAUT 19.08.** (FullWorkflow/09-analyst/, eigenes LEBENDES README; Checker lebt als
   check-vor-save IM Linsen-Werkzeug = Ebene-1-Muster, Präzisierung zu §5.0) — **B/Workflow+Agenten:** Collect-Stufe (Kollektoren + Digest) · 4 Linsen-Prompts (ISO-25010 + Lebenszyklus/
   CRUD/Rollen/Fehlerpfade + „stelle Fragen, wo du nicht formulieren kannst") · Lese-Tools (CoreQuery-Naht) ·
   Save-Once-Tool im AF-Format mit `origin=CoreAnalyst` + `analystKategorie` + Pflicht-Herleitung ·
   **Checker (R-33-Form): Fund OHNE existierende Core-ID in der Herleitung oder mit ungültiger Kategorie
   wird maschinell abgewiesen → Repair-Loop** (filtert Lehrbuch-Floskeln VOR dem Menschen).
3. ✅ **GEBAUT 19.08.** (synchron wie open_gate_ui, Ergebnis selbstbeschreibend; CLI `core-analysis run`
   auf derselben Naht) — **Steward-Seil:** `run_core_analysis` (⚿) startet einen ECHTEN Lauf (eigener Run-Ordner
   `runs/core-analysis/<id>/`, Events, Konsolen-Weiche — wie jeder andere Lauf; der Analyst liefert NICHT
   „Antworten in den Steward-Chat", sondern Artefakte: Funde-Report + Delta-Datei). Der Steward ist nur
   Manager: starten ⚿ → Funde aus dem Report PRÄSENTIEREN → run_pipeline_from_delta ANBIETEN (zweites ⚿,
   bewusst getrennt: der Autor liest erst die Funde, dann entscheidet er den Tor-Lauf; Kosten-Transparenz).
   **Fan-in-Pflicht:** Dedup nicht nur linsen-intern, sondern GEGEN DEN BESTAND — IdentityKey-Abgleich mit
   Core-Items UND Rejections (nie vorschlagen, was existiert oder begründet abgelehnt wurde; R-35-Note reist mit).
4. **Abnahme:** Live-Runde am 220er-Core (erwartbar: NFR-Lücken real vorhanden) — Playbook wie gewohnt.

**0. VORPROBE (Autor-Einsicht 19.08., VOR dem Bau, ~10 Min, fast kostenlos):** eine Steward-Brainstorm-
Session am heutigen Stand („nenne die 5 größten Lücken im Core, je mit Herleitung aus konkreten Items") —
der Steward KANN das heute schon (Core-Lese-Tools). Beantwortet die Kernsorge empirisch, BEVOR gebaut
wird: findet das Modell auf DIESEM Core substanzielle Lücken? Substanz ⇒ Bau-Risiko klein · Floskeln ⇒
erst Checklisten/Grounding nachdenken. (Der Analyst = industrialisierte Form des Brainstorms: Systematik
[Checklisten statt Tagesform] · Beleg [Report+Delta statt Chat] · Wiederholbarkeit/Messbarkeit · Batch
statt Dialog-Zeit. Kosten-Schätzung Analyse-Lauf: ~Größenordnung einer Tor-1-Resolver-Runde — 4 Linsen ×
[Digest + Nachbohren] + Kritiker; Stellschrauben: scope, Linsen-Zahl, Digest-Dichte.)

## 6. ⚖s — ENTSCHIEDEN 19.08. (Autor, „einverstanden mit Empfehlung")

- ✅ **Checklisten-Umfang v1:** ISO-25010-Merkmale + Lebenszyklus/CRUD/Rollen/Fehlerpfade — schmal
  starten, erweiterbar.
- ✅ **Dokument:** EINE Fassung, Stakeholder-lesbar, IDs in Klammern (Entwickler-Anker).
- ✅ **9k(c)-Durchstich GEBAUT 19.08.:** arch-classify/adr steward-bedienbar (open_gate_ui-Fähigkeitsliste,
  Resume explizit gekettet) — der Analyst-Output schickt den Autor in KEIN Fremd-Terminal mehr.
- ✅ **9k(a)-Rest als Beifang:** Rahmen-Sektion in den deterministischen Ernte-Diff (klein);
  arch-Ernte-Live-Beweis ins 1g-Abnahme-Playbook.
- ⏭ **NACH W2:** **Bootstrap-Integration als OPT-IN-Schalter** (`bootstrap --with-analysis`, Muster
  --arch-catchup — NICHT „geht nicht beim Bootstrap", sondern gestufte Adoption: Inline spart exakt EINEN
  Handgriff [Analyse-Aufruf direkt nach dem Bootstrap], kostet aber Default-LLM-Stufe im teuersten Lauf +
  Graph-Änderung, BEVOR der Agent sich einmal bewiesen hat; erst Abnahme, dann Schalter) ·
  `analystKategorie`-Nachzug für Meeting-Items (braucht LLM-Klassifikations-Lauf + Gate über den Bestand;
  das Doc weist Unkategorisiertes v1 ehrlich aus).

### 6.1 Rest-Unsicherheit + Eskalationsleiter (Autor-Frage 19.08. „was, wenn es nicht gut ist?")

**Warum die Unsicherheit prinzipiell besteht:** „Was fehlt?" hat kein Gold-Label (Open-World) — ob Funde
core-spezifisch treffen statt Lehrbuch-Floskel, ist nur messbar, nicht vorab beweisbar. Vierfache
Einhegung: ① det. Checker (Core-Anker-Pflicht, s. §5-2) ② Bestand+Rejection-Dedup im Fan-in ③ das
R-35-Gedächtnis wirkt automatisch (abgelehntes Analyst-Rauschen wird REJ + Wiedervorlage-Warnung — er
lernt über die normale Governance) ④ Human-Gate. **Eskalationsleiter bei schlechter Abnahme** (Messgröße:
Präzision der Funde, Vorbild 9g-Funnel 7/12→5/5): Stufe 1 Linsen-Prompts schärfen (erlaubt — Maker-
VERHALTEN, keine Governance; Leitsatz) → Stufe 2 Checker härten (mehr det. Ablehngründe) → Stufe 3 Scope
ehrlich reduzieren (v1 nur `question`-Disposition: Analyst STELLT Lücken-Fragen via 9g-Rampe — sofort
nützlich, minimales Rausch-Risiko) → Stufe 4 parken MIT Messbefund (auch ein ehrliches Thesis-Ergebnis).

## 7. ⚖ BESCHLOSSEN 19.08.: Agenten-Zuschnitt — Linsen-Fan-out, kein A2A, Steward als Manager

**Beschluss:** v1 = EIN bestellbarer Analyst-WORKFLOW (kleiner MAF-Graph) mit **vier Linsen im Fan-out**
(funktionale Lücken · NFR/Qualität · Architektur-Rahmen · Risiko) — gleiche Core-Lese-Tools,
unterschiedliche Checklisten-Prompts, parallel — dann **deterministischer Fan-in** (Dedup per IdentityKey,
Zusammenführung) → EIN Delta → Tor 1. **Risiko-Linse:** Output = offene FRAGEN über die 9g-Rampe
(„Risiko erkannt: … — Umgang klären?"), denn Risiko ist heute kein Wahrheits-Typ; der echte Risk-Aspekt
(inkl. Bestellung des nie bestellten `risks`-Zweigs auf der GESAGT-Bahn) = NACH W2 (Aspekt-Modell-Ausblick).
**Erwartungs-Rahmen (Autor-Frage 19.08. „erwarte ich dann VIELE Risiko-DECs?"):** Ziel ist SUBSTANZ, nicht
Menge (Handvoll je Linse, Kritiker filtert) — je Linse geht ein DEKLARIERTES Top-N ins Delta, der Rest
steht im Report als „weitere Kandidaten" (kein stiller Cap). Geparkt wird nichts automatisch: DECs prägt
NUR die Autor-Freigabe an Tor 1. **Schwellwert-Trigger für den Risk-Typ:** wächst der Risiko-Anteil im
DEC-Topf spürbar (~zweistellig; Wiedervorlage-Last am decision-gate trotz Bulk), ist DAS das
Auslöse-Kriterium, Risiken ihr eigenes Zuhause zu geben (statt „irgendwann").
**Steward = Manager im K7-Sinn** (Delegation = Workflow-Start ⚿, Präsentation, Gate-Führung — nie
Entscheider). **Handoff-Spike-Szenario = Steward→Analyst** (beweist die letzte MAF-Matrix-Zelle UND
demonstriert das Manager-Muster in einem Bau).

### 7.1 Begründung „Linsen statt getrennter A2A-Agenten" — klipp und klar

**Erkenntnistheoretisch ehrlich vorweg: Das ist eine BEGRÜNDETE Architektur-Entscheidung, kein
empirischer Beweis.** Es gibt keinen Messwert „Linsen schlagen A2A" — es gibt vier Argumente, warum die
Beweislast beim A2A-Design läge, plus einen Weg, die Frage später empirisch zu machen (7.2).

1. **Rolle ≠ Agent (Zuschnitts-Argument):** Der Requirements Engineer ist EINE Rolle mit mehreren
   BRILLEN. Die Spezialisierung liegt in den Checklisten-Prompts (Linsen), nicht in getrennten
   Identitäten. Zwei Agenten „ReqAnalyst" und „ArchAnalyst" hätten identische Tools, identischen
   Wahrheits-Zugang und identischen Output-Weg — getrennt wäre nur der Gesprächsfaden. Das dupliziert
   Infrastruktur, ohne Fähigkeit hinzuzufügen.
2. **Blackboard-Prinzip (Architektur-Argument, K10-Befund 07.08.):** Das System IST bereits ein
   Multi-Agenten-System — seine Agenten (Extraktoren, Resolver, Placement, Analyst …) koordinieren über
   das BLACKBOARD: Core + typisierte Artefakte + Gates. Jede Information, die ein Agent einem anderen
   „sagen" will, hat einen governeten Ort (Fan-in-Artefakt, Delta, Core nach Freigabe). Ein direkter
   A2A-Kanal wäre ein ZWEITER Koordinationsweg NEBEN dem Blackboard — Redundanz mit Drift-Risiko
   (welcher Kanal gilt, wenn sie sich widersprechen?).
3. **Governance-/Evidenz-Argument:** Autor-Einwand korrekt: A2A-Chat KANN geloggt und ausgewertet
   werden (Transkript als Artefakt). Aber Auswertbarkeit ≠ Governance: Der Blackboard-Weg erzwingt, dass
   jede tragende Information TYPISIERT und GATED fließt (Kangal, Events, Replay); ein A2A-Faden trägt
   Einfluss formlos — er kann Vorschläge prägen, ohne dass die Prägung als Evidenz-Kante existiert.
   Für ein System, dessen Kernversprechen „jede Wahrheit rückverfolgbar" ist, wäre das die erste
   nicht-typisierte Einflussquelle. Nicht unmöglich zu bauen — aber ein NEUES Governance-Problem ohne
   erkennbaren Fähigkeits-Gewinn (s. 1).
4. **Determinismus-/Kosten-Argument:** Fan-out/Fan-in ist das im Projekt BEWIESENE Muster
   (Extraktions-Stufe; parallel, budget-planbar, W2-replay-freundlich: n Aufrufe, deterministische
   Zusammenführung). A2A-Dialoge sind länglich, nicht-deterministisch in der Rundenzahl und schwerer
   zu wiederholen. Für die Messkampagnen-Ära ist das planbare Muster überlegen.

**These fürs Thesis-Kapitel (so begründen):** „Selbst die kreative Exploration wird deterministisch
orchestriert — die Agenten kommunizieren über die gemeinsame, gated Wahrheit (Blackboard), nicht
miteinander. A2A wurde bewusst NICHT eingesetzt, weil es gegenüber dem Blackboard keinen
Fähigkeits-Gewinn bietet, aber einen zweiten, nicht-typisierten Koordinationskanal einführen würde."

### 7.1b Zwei Nachträge (Autor-Prüffragen 19.08.)

- **Maker-Anzahl = STELLSCHRAUBE, keine Architektur-Frage:** Ein „einfacher Agent" (ein Maker, alle
  Checklisten) wäre funktional ausreichend — die Workflow-HÜLLE (Collect/Checker/Persist/CLI-Parität)
  braucht es so oder so; innen sind 4 fokussierte Linsen vs. 1 Maker derselbe Executor mit anderen
  Prompts. Grund für 4: Aufmerksamkeits-Verdünnung bei Riesen-Checklisten (Vermutung — die Abnahme
  prüft sie; Rückbau auf 1 bzw. Ausbau trivial).
- **KEIN Vor-Gate am Analyse-Lauf:** Der Entscheidungspunkt existiert — Tor 1 (jeder Fund einzeln, mit
  Warnungen/Ziel-Diff). Ein Vor-Gate wäre Doppel-Review desselben Inhalts UND würde das R-35-Gedächtnis
  umgehen (Ablehnungen stempeln NUR an Tor 1). Der frühe Blick kommt gratis: Steward präsentiert den
  Report VOR dem Tor-Lauf; Tor-Lauf nicht starten = null Kosten.

### 7.1c Prüfungs-Antwort „Warum kein Handoff zwischen Req-/Risk-/Arch-Agenten?" (19.08., nach Spike)

Kurzform: **Handoff routet NUTZER-Gespräche** (Doku: customer support, „receiving agent takes full
ownership", antwortet dem Nutzer) — meine Spezialisten führen keine Gespräche, sie transformieren Evidenz
zu Artefakten in einer governeten Strecke. Dafür nennt dieselbe Doku das passende Muster: **Concurrent**
(diverse perspectives) = mein Linsen-Fan-out. Handoff wurde EMPIRISCH erprobt (HandoffSpikeTests, 3
Befunde: Declaration-only-Tool · Routing hängt an Description-TEXTEN · Output-Filterung=Konsumenten-Pflicht)
und begründet verworfen: modell-entschiedener Verantwortungswechsel = zweiter, nicht-typisierter
Kontrollfluss — gegen das Kernversprechen (typisiert, gated, replay-fähig). Koordination existiert über
das BLACKBOARD (Core+Artefakte+Gates, auditierbar). Entscheidung falsifizierbar (7.2-Ablation).
Konter „wäre agentischer gewesen?": Autonomie beim INHALT (Tool-Wahl, Exploration, Checker-Iteration —
eigene Mess-Achse W2), Kontrolle beim KONTROLLFLUSS — die Trennung ist Befund, nicht Schwäche.

### 7.2 Falsifizierbarkeit (macht die Entscheidung wissenschaftlich sauber)

Die Frage „bringt A2A doch etwas?" ist EMPIRISCH prüfbar und als Nach-W2-Ablation notiert: derselbe
Analyst-Auftrag als (a) Linsen-Fan-out vs. (b) zwei dialogisierende Agenten — Vergleich auf
Fund-Qualität/Recall, Kosten, Reproduzierbarkeit. Damit ist der Beschluss kein Dogma, sondern eine
begründete Default-Wahl mit definiertem Prüfweg. (K7-Beschluss „A2A nur als Experiment" bleibt gültig.)

## 7.3 MAF-Abgleich (quellenbasiert, offizielle Learn-Doku geprüft 19.08. — K-Methodik)

**① Das Linsen-Muster IST das offizielle Muster.** Die Doku zu *Concurrent Orchestration* benennt unseren
Use-Case wörtlich: „well-suited for scenarios where **diverse perspectives** or solutions are valuable,
such as brainstorming, ensemble reasoning" — inkl. der Bausteine, die wir planen: Custom Agent Executors
(unsere Linsen mit Tools) + **Custom Aggregator** (unser deterministischer Fan-in/Merge) +
`AddFanOutEdge`/Fan-in-Edges. Geprüfte Alternative: `AgentWorkflowBuilder.BuildConcurrent` (Convenience) —
bewusst NICHT gewählt: seine Aggregation ist ChatMessage-förmig; unsere Funde sind TYPISIERTE Records mit
Checker-Vertrag (Codebasis-Idiom: typed messages wie `ArchClassifyWork`).
**② Shared State geprüft, Message-Passing gewählt.** MAF bietet `QueueStateUpdateAsync`/`ReadStateAsync`
mit Scopes für Daten, „where direct message passing is **not feasible or efficient**". Bei uns ist Passing
beides: der Fan-out trägt EINE typisierte Nachricht (Digest+Kollektor-Funde) an N Linsen — compile-geprüfte
Verträge statt string-gescopter State-Keys. Shared State bliebe das Mittel der Wahl, wenn viele Executors
OHNE direkte Kanten dieselben Daten bräuchten (dokumentierte Alternative, kein Bedarf v1).
**③ Memory-Frage — die offizielle Doku stützt unsere Position:** MAFs Memory-Stack (Agent Threads /
ChatHistoryProvider / AIContextProvider) ist für KONVERSATIONS-Kontinuität gebaut; die State-Doku warnt
ausdrücklich, dass über Läufe persistierte Agent-Threads bei task-förmiger Nutzung zu „unintended state
sharing" führen — Empfehlung: **frische Instanzen je Task** (genau unser Batch-Worker-Design; §5.0).
Domänen-Zustand cachen ist NICHT die Aufgabe von Agent-Memory: der Core ist EINE lokale JSON-Datei,
Neu-Lesen kostet Millisekunden und garantiert Frische (ein Cache wäre ein Staleness-Risiko ohne Gewinn).
**④ State-Isolation-Guidance erfüllt:** Doku empfiehlt Workflow-Bau je Task via Helper (keine Instanz-
Wiederverwendung) — exakt das BuildGraph-je-Lauf-Muster des Projekts; der Analyst folgt ihm.
**Quellen (offizielle Microsoft-Learn-Doku, abgerufen 19.08.2026) — je Behauptung:**
- zu ① (Linsen-Muster = offizielles Muster; Custom Executors + Custom Aggregator; BuildConcurrent-Alternative):
  https://learn.microsoft.com/en-us/agent-framework/workflows/orchestrations/concurrent
  (Zitat: „well-suited for scenarios where diverse perspectives or solutions are valuable, such as
  brainstorming, ensemble reasoning, or voting systems")
- zu ② (Shared State: Zweck, Scopes, Superstep-Sichtbarkeit; Einsatz „where direct message passing is
  not feasible or efficient") und ④ (State-Isolation: Workflow je Task frisch bauen; Agent-Threads
  persistieren über Läufe derselben Instanz → „unintended state sharing", Empfehlung Helper-Methode):
  https://learn.microsoft.com/en-us/agent-framework/workflows/state
- zu ③ (Memory-Stack = Konversations-Kontinuität: Agent Threads / ChatHistoryProvider / AIContextProvider;
  Provider-Instanz session-übergreifend → kein Session-Zustand im Provider):
  https://learn.microsoft.com/en-us/agent-framework/agents/conversations/context-providers ·
  https://learn.microsoft.com/en-us/agent-framework/get-started/memory
- Kanten/Fan-out-Referenz: https://learn.microsoft.com/en-us/agent-framework/workflows/edges

## 8. Ausblick (NACH W2, notiert 19.08.)

- **Recherche-Fähigkeit (Autor-Idee 19.08.):** Internet-Recherche für Fragen, die die Agenten aus dem
  Core nicht beantworten können (z. B. „übliche Compliance-Anforderungen für Pflege-Apps"). Richtiger
  Zuschnitt nach der Rolle≠Agent-Logik: ein **WERKZEUG** (web_search-Tool, z. B. MCP) am Analysten —
  kein eigener Agent. Braucht VOR dem Bau ein Evidenz-Design: Web-Funde sind eine NEUE Quell-Art
  (Herkunft = URL + Abrufdatum, Verlässlichkeits-Kennzeichnung, nie Auto-Wahrheit — dieselbe
  Herkunfts-Ehrlichkeit wie origin CoreAnalyst). = eigener ⚖ + Slice nach W2.
- **Doc-Grundsatz (Autor-Klärung 19.08.): Dokumente ERSTELLEN nie Inhalt** — Inhalt entsteht nur durch
  die Tore in den Core; jedes Doc (Req/Risks/ADR) ist regenerierbare ANSICHT des aktuellen Stands. Neues
  Doc in 2 Wochen = Render des DANN gültigen Cores (offene Risiken wieder drin, geklärte ehrlich raus);
  nichts lebt „nur im Doc". Risks-DOC mit Substanz setzt den Risk-Typ voraus; v1-Zwischenstand: Risiko-
  Funde sind Frage-DECs (arbeitbar: Parkplatz, decision-gate, Klärung) und erscheinen im Anforderungs-
  dokument unter „Offene Entscheidungen/Fragen". Alte Doc-Fassungen = git-Schnappschüsse, nie Quellen.
- **Risk-Aspekt vollwertig — Erweiterungs-Anatomie (Autor-Frage 19.08. „wie modular sind wir?"):**
  Die R-11-Schienen machen neue Aspekte billig in der ROHRLEITUNG: ① Typ+Spec (Kangal-Regeln, klein) ·
  ② AspectIngestionProfile + Router-Registrierung (A1-Muster; Unregistriertes parkt LAUT — nichts geht
  still unter) · ③ Produzenten UNABHÄNGIG wählbar: Rezept-Zweig (GESAGT) und/oder Analyst-Linse/AF-
  Disposition (ERSCHLOSSEN) — **das Risks-Doc braucht das Rezept NICHT** (Projektion rendert, was im Core
  steht, egal woher) · ④ **der teure Teil ist die WIRKUNG** (blockt ein Risk? weckt es PBIs? wird
  Mitigation eine DEC?) = Design-Session, kein Mechanik-Problem · ⑤ Projektion billig · ⑥ Tools
  größtenteils generisch (itemType-Filter). **User Stories = KEIN neuer Aspekt** — PBIs SIND die
  User-Story-förmigen Items; Story-FORMAT wäre eine Render-Frage der Projektion (Duplikat-Falle!).
  **Wegwerfbarkeit des Analysten (Autor 19.08.):** Zuruf-Design ⇒ nichts hängt von ihm ab (keine Kette,
  kein Konsument, Ein-Graph kennt ihn nicht) — Enttäuschung kostet einen Ordner + behält einen Messbefund;
  Steward-Brainstorm und Analyst koexistieren dauerhaft (interaktiv vs. systematisch-belegt).
- A2A-Ablation (7.2) · Bootstrap-Schritt · Kategorien-Nachzug (§6).
- **„Resolve-to-Truth" am decision-gate (Autor-Gedanke 20.08.):** ein Auflöse-Ausgang, der die Antwort
  einer Frage-DEC DIREKT als Wahrheits-Kandidat prägt (statt Klärung + separatem Diktat). Heute bewusst
  zweistufig (Diktat-Bahn = volle Tor-Strecke inkl. Resolver-Matching); wächst der Fragen-Anteil, wäre
  der Direkt-Ausgang der nächste Reibungs-Abbau — Design-Frage: Minting am decision-Apply vs. Delta-Erzeugung.
