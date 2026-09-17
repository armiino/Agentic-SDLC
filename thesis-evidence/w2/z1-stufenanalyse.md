# Z1 — Stufenanalyse des Stresslaufs (A: 54 Kandidaten → B: 47 kanonische Claims)

> Status: Auswertung erstellt 07.09.2026 (KI-Adjudikation); **separater Verifikationsdurchgang
> ERLEDIGT 07.09. (Kollegen-KI): 12/12 Paare, 24/24 Seitenurteile BESTÄTIGT** — als „separate,
> nicht verblindete KI-Nachprüfung" ausgewiesen (Verfahrensabweichung dokumentiert: Bericht
> früh mitgelesen, Verlustbefunde vorbekannt). Protokoll:
> `kapitel-7/04-material-stufenanalyse/Z1-Verifikationsprotokoll-2026-09-07.md` (Hashes =
> exakt dieses Paket, nachgeprüft). Autor-Stichprobe bleibt optional (⚖).
> Regeln: Messprotokoll §3 · Gold v2 (109 IDs) · Lauf `20260905_164650_8189ea` ·
> Urteilsblatt + Belege: `runs/w2-z1-stufenanalyse-8189ea.json` (A- UND B-Urteil je
> Referenzaussage, mit Beleg). REVISION 07.09. spät nach Kollegen-Review (Plan-/Rechen-/
> Logikprüfung): Merge-Zählung faktisch korrigiert (§3), Hinweis-Relevanz auf v2-Basis
> nachgerechnet (§4), Formulierungen präzisiert — Kernzahlen und Kernbefunde unverändert.

## 1 · Übergangstabelle (jede Referenzaussage genau eine Zelle)

| A ↓ / B → | full | partial | none | Σ A |
| --- | --- | --- | --- | --- |
| **full** | 55 | **1** (G-IE-021) | **1** (G-IE-023) | 57 |
| **partial** | 0 | 29 | 0 | 29 |
| **none** | 0 | 0 | 23 | 23 |
| **Σ B** | 55 | 30 | 24 | 109 |

Strict-Coverage der Stufen: **A = 57/109 (0.523) · B = 55/109 (0.505)**. Touched Coverage:
**A = 86/109 (0.789) · B = 85/109 (0.780)**. Es gibt **keine Verbesserungen** A→B (keine
Zelle unterhalb der Diagonale). 107 von 109 Referenzaussagen behalten ihre Klasse; die
vollständige Abdeckung sinkt um 1,83 Prozentpunkte. Das ist eine deskriptive Aussage über
DIESEN untersuchten Übergang, keine allgemeine Wirksamkeitsbewertung der Kanonisierung.

## 2 · Die zwei Verluste (beide Seiten textbelegt)

Beide stecken im selben Kanonisierungs-Merge: Kandidat **DEC-019** sagt wörtlich
„…User können Inhalte hinzufügen, **aber nichts löschen**". Der kanonische **CAN-019**
(candidateIds: DEC-019 + DEC-020) übernimmt Login/Rollen/Admin-Rechte/keine
Selbstregistrierung — **lässt die User-Rechte-Klausel aber weg**; kein anderer kanonischer
Claim trägt sie.

| ID | Referenzaussage | A | B | Wirkung |
| --- | --- | --- | --- | --- |
| G-IE-021 | Normale User-Accounts sollen Inhalte hinzufügen können | full (DEC-019 wörtlich) | partial (nur CAN-013 „Kommunikationswissen muss dynamisch erweitert werden können" — Hinzufügen-Fähigkeit ohne Rollen-Bindung an normale User) | Abschwächung |
| G-IE-023 | Normale User-Accounts sollen Daten nicht löschen können | full (DEC-019 wörtlich) | none | Totalverlust |

Die Feststellung „kein anderer kanonischer Claim trägt die Klausel" wurde gegen den
GESAMTEN B-Bestand geprüft (alle 47 Claims durchsucht; nächstliegende Kandidaten CAN-013 und
CAN-043 „alle Nutzer ihr Profil ändern" tragen sie nicht) — Beleg im Urteilsblatt-JSON.

**Ergebnisformulierung (für 7.3, nach der noch offenen Verifikation):** Im untersuchten
Übergang blieben die Abdeckungsklassen von 107 der 109 Referenzaussagen unverändert. Zwei
zuvor vollständig enthaltene Aussagen wurden nur noch teilweise beziehungsweise nicht mehr
abgedeckt; beide Veränderungen betrafen dieselbe kanonische Zusammenführung. Der
referenzielle Übergangscheck meldete dabei keinen Fehler — für diese beiden Inhalte war eine
gültige Kandidatenverknüpfung somit kein ausreichender Nachweis ihrer semantischen Erhaltung.
(Zuschreibung an den GESAMTÜBERGANG A→B — Kanonisierung + CanonicalCheck —, nicht an einen
isolierten Modellaufruf.)

## 3 · Einordnung der B-Lücken nach Entstehungsstufe (neu möglich durch Z1)

Von den 54 B-Misses (non-full) bestehen **52 bereits in Bestand A** (Extraktions-/
Detaillierungsgrenze von step-01) und **2 entstehen erst im Übergang A→B** (Kanonisierung).

**Merge-Struktur (KORRIGIERT 07.09. nach Kollegen-Review — jetzt aus den candidateIds
BERECHNET statt angenommen; Erstfassung „5 der 7" war nicht ableitbar und falsch):** Die
Mengenänderung 54→47 ist kein eigener Verlustnachweis. Tatsächlich existieren exakt
**7 Zweier-Merges** (alle 54 Kandidaten referenziert, keiner doppelt, keiner verworfen):
CAN-019←DEC-019+020 · CAN-027←CON-028+DEC-029 · CAN-030←REQ-032+048 · CAN-032←REQ-033+OQ-034 ·
CAN-035←REQ-037+RISK-038 · CAN-037←DEC-040+PROC-041 · CAN-038←DEC-042+OQ-043. Die beiden
Gold-Statusverluste liegen BEIDE in EINER Gruppe (CAN-019) — **6 der 7 Merge-Gruppen sind
ohne Gold-Statusverlust** („verlustfrei" hier strikt bezogen auf die 109 geprüften
Referenzinhalte, keine darüber hinausgehende Inhaltsgleichheits-Behauptung).

## 4 · Hinweis-Herkunft der 48 Miss-Signale — UND getrennt davon: ihre Relevanz

**Herkunft (deterministisch rekonstruiert — erklärt, WIE die Signale entstehen):** Kette
**01b** unit-coverage (Skript: 136 Units → 75 used / 61 unused) → **01c** Triage der 61
(48 potentially_relevant · 4 smalltalk · 6 acknowledgement · 3 low_signal) → **01d**
Ledger-Compare der 48 (21 already_covered_indirectly · 24 attach_as_evidence · 2 needs_human ·
1 missing_claim). Die 48 LCR-Miss-Signale sind **exakt die 48 01d-Items** (unitId-Mengen
identisch, geprüft); nichts war unrekonstruierbar.

**Relevanz (auf Gold-v2-Basis NACHGERECHNET 07.09., nicht aus Hash ① fortgeschrieben):**
Nach der festgelegten deterministischen Zuordnungsregel (Signal-Unit ∩ Gold-Quell-Units)
treffen **18 der 48 Signale** mindestens eine unvollständig abgedeckte Referenzaussage
(Signal-Relevanz 18/48 = 0.375 — unverändert gegenüber Hash ①); sie erreichen 35 der 54
Lücken, plus die eine dokumentierte semantische Zuordnung (G-IE-028←AU-0124) = die 36
„detektierten" Misses der Hauptauswertung. **Vorsichtige Lesart (statt „36 konkrete
Prüfaufträge"):** 36 der 54 unvollständig abgedeckten Referenzaussagen ließ sich mindestens
ein relevanter Hinweis zuordnen; die daraus berechnete Adressierbarkeit bezeichnet
PRÜFPOTENZIAL — eine tatsächliche menschliche Erkennung oder Behebung wurde nicht gemessen.
Zur Signalqualität: Von den 18 relevanten Signalen ist nur **1** ein explizites
`missing_claim`; 17 tragen beruhigende Verdikte (6 already_covered / 11 attach_as_evidence) —
ein Unit-Treffer ist also nicht automatisch eine explizite Warnung.

Konsequenz (als Befund DIESES Laufs und dieser Zuordnung, keine universelle Vorhersage):
Die Signale adressieren ausschließlich unverwendete Units; alle 18 stillen v2-Lücken (inkl.
der 2 Kanonisierungs-Verluste) liegen in verwendeten Units — das Design prüft verwendete
Units über diesen Hinweisweg nicht erneut.

## 5 · Gütesicherung und Grenzen

- Urteile: KI-Adjudikation (Claude, 07.09.) mit Beleg je Aussage; beide Statusänderungen
  beidseitig textgeprüft (§2). Seed-Stichprobe nach Protokollregel `SHA-256(id+'w2seed42')`
  (10 niedrigste unveränderte Paare: G-IE-080/083/048/082/017/044/012/041/057/003).
- **Separater KI-Verifikationsdurchgang ERLEDIGT (Pflicht laut Messprotokoll §3):**
  Kollegen-KI, 07.09. — V1 (beide Statusänderungen, inkl. eigener Vollbestands-Suche mit
  zusätzlichem Kandidaten CAN-023) und V2 (alle 10 Seed-Paare) **12/12 bestätigt**; Struktur-,
  Seed-, Merge- und Signalzahlen unabhängig reproduziert. Ausweis: „separate, NICHT
  verblindete KI-Nachprüfung" (Bericht früh mitgelesen, Befunde vorbekannt — dokumentierte
  Verfahrensabweichung; keine menschliche Zweitannotation). Beleg:
  `Z1-Verifikationsprotokoll-2026-09-07.md`. Autor-Stichprobe optional (⚖).
- Gleicher-Maßstab-Disziplin: Bei wortgleichen Propositionen (z. B. DEC-054≡CAN-047,
  REQ-050≡CAN-043) wurde derselbe Maßstab beidseitig angewandt. KORREKTUR aus der
  Verifikation: das frühere Beispiel „REQ-048≡CAN-030" war FALSCH — CAN-030 ergänzt die
  Zweck-Klausel „damit Profile schnell gefunden werden können" aus REQ-032 (am Text
  nachgeprüft); das G-IE-048-Urteil selbst bleibt davon unberührt.

## 6 · Kreuzanalyse: Wo landen die Lücken — und findet F sie? (07.09., aus den v2-Urteilsblättern)

**Signal-Auffindbarkeit der 54 Ledger-Lücken:** 36 sind SIGNALISIERT (stehen mit Unit-Bezug,
Verdikt und Vorschlag im Miss-Signal-Topf), 18 sind STILL. Die Trennlinie ist exakt die
Konstruktionslinie aus §4: Signalisierte Lücken leben in UNVERWENDETEN Units — vor allem die
späten Design-Units (AU-0117/0118/0120/0124–0129/0136 sind sämtlich unused und signalisiert),
deren Gold (G-IE-072ff/088ff) step-01 nicht in Claims überführte. Stille Lücken sind
Detailverluste INNERHALB verwendeter Units: der Claim konsumierte die Unit, ließ aber eine
Qualifikation fallen (Versionsnummern G-066, NoSQLite-Name G-069, Login-Anordnung G-079/080,
Positionsrelation G-085, Kalender-Details G-037/038, Merge-Verluste G-021/023, u. a. —
vollständige 18er-Liste im Urteilsblatt-JSON).

**F-Gegenprobe (Interview, v2):** F deckt 20 der 54 Ledger-Lücken voll (13 der 36
signalisierten, 7 der 18 stillen) — F ist bei UI-Details granularer (117 Aussagen vs. 47
Claims). ABER: F hat selbst 38 Lücken (davon 4, wo der Ledger voll ist; 12, wo BEIDE nichts
haben), und alle 38 sind zu 100 % still (0 gemeldet). Beim Ledger ist 36 der 54 Lücken
mindestens ein relevanter Hinweis zuordenbar (Adressierbarkeit 0.835 = PRÜFPOTENZIAL, keine
erreichte Erkennung — vs. F 0.651; direkte Abdeckung und Prüfpotenzial bleiben getrennte
Größen). Auf den 18 stillen Ledger-Lücken ist F keine verlässliche Rettung: nur 7 voll,
5 fehlen auch bei F ganz.
