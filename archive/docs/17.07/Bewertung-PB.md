# Kritische Qualitaetsbewertung: Product Backlog (l4-re-clarify)

Bewertet: `runs/l4-re-clarify/20260717_210758_f37eac/backlog/product-backlog.json` (27 PBIs, gpt-5.4).
Massstab: PDF `03_AnforderungenUndMVP` (identifizierbar · pruefbar · schaetzbar) + plan-pb/plan-re-backlog.

## Gesamturteil

**Deutlich ueber der Ausgangslage; fachlich tragfaehiges, ehrliches, priorisiertes Backlog.**
Die verbleibenden Schwaechen sind **Kalibrierung** (Prompt-Ebene), NICHT Architektur — der Mechanismus
(Cluster -> Clarify -> Gate) traegt.

## Stark (belegt)

- **Vollstaendigkeit:** 69/69 kanonische Requirements in PBIs abgedeckt (Coverage-Gate pass). Kein stiller Verlust.
- **Ehrlichkeit der offenen Punkte:** 50/51 offene Entscheidungen `evidence=stated` — also im Transkript
  belegt, NICHT open-world erfunden. Genau das gewuenschte "nichts halluzinieren".
- **Querschnitt korrekt gezogen:** Rollen (CAN-REQ-002) in 8 PBIs, Datenschutz (005) in 7, (011) in 6 —
  Querschnittsregeln haengen an den Features, die sie betreffen (Mehrfach-Mitgliedschaft, keine Dublette).
- **Priorisiert, nicht nur Inventar:** `priorityRank` 1..26 gesetzt (DEEP-P).
- **Sinnvoller Schnitt:** 27 PBIs aus 12 Clustern (1-3 je Cluster), keine Ueber-Zerlegung.
- **CRUD/Lifecycle (verwalten=CRUD, PDF):** No-Go zerfaellt in anzeigen/hinzufuegen mit testbaren Kriterien
  (inkl. "leere Eingaben werden nicht gespeichert") + edit/delete/Rechte/Legal als EXPLIZITE offene Entscheidungen.
- **RE-Bar:** identifizierbar ✓ (pbiId+requirementIds); pruefbar ✓ (26/27 mit Akzeptanzkriterien);
  schaetzbar: die 12 ready-Items ja, die 15 blockierten bewusst noch nicht (ehrlich).

## Schwach / kritisch (die 3 echten Punkte)

1. **Agent ueber-klassifiziert als "Stakeholder-Entscheidung" und nutzt den Proposer kaum.**
   50/51 offene Punkte = `stakeholder_decision`, nur 1 `engineering_default`, `proposedResolution` fast immer leer.
   Folge: der Agent LADET Fragen ab, statt wo sicher moeglich einen Default VORzuschlagen (z.B. "leere Eingabe
   -> Validierungsfehler" gehoert als engineering_default mit Vorschlag, nicht als Stakeholder-Frage). Das ist
   der groesste Qualitaetsverlust — der Zwei-Achsen-/Vorschlags-Mehrwert wird verschenkt.

2. **15/27 `blocked_by_decision` — Block-Quote sehr hoch.** Haengt an (1): weil fast alles als blockierende
   Stakeholder-Frage gilt, ist mehr als die Haelfte des Backlogs blockiert. Einige Blocks sind berechtigt
   (Rechtsgrundlage, MVP-Scope), andere wirken ueber-vorsichtig (koennte add mit einem Default starten?).
   Sicher, aber es kann das Backlog unnoetig ausbremsen.

3. **`type="pbi"` bei allen 27 -> delivery vs. clarification nicht trennbar.** Mehrere PBIs sind reine
   KLAERUNGS-Items ("Kalender-Scope entscheiden", "Uebernahme entscheiden", FC001-01 MVP-Scope, FC004-03).
   `PBI-FC004-03` hat sogar 0 Akzeptanzkriterien (passiert das Gate nur ueber openDecisions) — ein
   clarification-PBI, das als Delivery-PBI durchlaeuft. Ehrlich, aber die Klassifikation ist unscharf.

Kleiner: 1 Akzeptanzkriterium (FC001-01) ist faktisch ein Statement ("Als … will ich …") statt ein
testbares Kriterium — seltener Ausrutscher (1 von ~100).

## Empfohlene Kalibrierung (Prompt, nicht Architektur)

- **Proposer schaerfen:** Prompt anweisen, sichere Engineering-Defaults AKTIV als `engineering_default` +
  `proposedResolution` zu liefern; `stakeholder_decision` nur fuer echte Produkt-/Scope-/Rechts-Fragen.
  Erwartung: mehr `ready_with_nonblocking_questions`, weniger `blocked_by_decision`.
- **Type erzwingen:** `type` aus {delivery|clarification|deferred|out_of_scope} (Prompt schon gefixt fuer
  naechsten Lauf) -> reine Klaerungs-Items werden als `clarification` sichtbar, nicht als leere Delivery-PBIs.
- **AK-Qualitaet:** Akzeptanzkriterien duerfen keine Wiederholung des Statements sein (testbar/ueberpruefbar).
- **Mess-Hygiene:** wie L3 mehrere Laeufe (Median), da LLM-Varianz (Cluster: mal 12 mal 16; ReviewAgent: mal
  approve mal 5 Findings). Block-Quote + Default-Ablehnungsquote als Metriken.

## Fazit fuer die Forschungsfrage

Der agentische Teil (Cluster/Clarify) liefert echten Mehrwert (Tiefe, Querschnitt-Zusammenfuehrung,
belegte offene Punkte) — die deterministischen Gates halten Coverage/DoR. Die Schwaechen sind Kalibrierung,
kein Grundsatzproblem: **Agent dort, wo Bedeutung regiert; deterministisch dort, wo Mechanik regiert** —
und die naechste Verbesserung ist Prompt-Feintuning + Mess-Hygiene, nicht mehr Architektur.
