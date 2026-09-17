# Response Text — Checker-architecture

## Model Round 1

- No tool calls (standalone text response)
- Text length: 827 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert den beschlossenen Zugriffsclaim vollständig und ohne Zusatzdetails.","ref":"L5","verdict":"supported","rationale":"Lesezugriff für Angehörige auf die Wochenübersicht ihrer Bezugsperson sowie das Verbot von Änderungen sind direkt durch Proposition und Evidence gedeckt.","unsupportedSpan":""},{"reasoning":"Der monatliche PDF-Export mit allen dokumentierten Ereignissen ist gedeckt, aber die Zeile lässt das konkrete Detail weg, dass er von der Leitung angestoßen wird; das ist keine Übertreibung und kein Widerspruch.","ref":"L11","verdict":"supported","rationale":"Die Aussage zum monatlichen PDF-Export mit allen dokumentierten Ereignissen ist direkt durch die Proposition gedeckt; das Weglassen des Details zur Leitung macht die Zeile nicht ungedeckt.","unsupportedSpan":""}]}

---

## Model Round 2

- No tool calls (standalone text response)
- Text length: 733 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert vollständig den entschiedenen Zugriffsclaim ohne zusätzliche Details.","ref":"L5","verdict":"supported","rationale":"Lesezugriff für Angehörige auf die Wochenübersicht ihrer Bezugsperson sowie das Verbot von Änderungen sind explizit in Proposition und Evidence enthalten.","unsupportedSpan":""},{"reasoning":"Die Zeile lässt das Detail zum Anstoß durch die Leitung weg, fügt aber keine neuen ungedeckten Details hinzu.","ref":"L11","verdict":"supported","rationale":"Monatlicher PDF-Export mit allen dokumentierten Ereignissen für die Einrichtung ist in der Proposition explizit enthalten; das Weglassen der Rollenbeschränkung ist als Zusammenfassung zulässig.","unsupportedSpan":""}]}

---

## Model Round 3

- No tool calls (standalone text response)
- Text length: 927 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert vollständig den entschiedenen Zugriffsclaim mit Leserecht und Änderungsverbot für Angehörige.","ref":"L5","verdict":"supported","rationale":"Alle konkreten Details der Zeile sind durch Proposition und Evidence gedeckt: Angehörige, Wochenübersicht der Bezugsperson, Lesezugriff und keine Änderungen.","unsupportedSpan":""},{"reasoning":"Der monatliche PDF-Export mit allen dokumentierten Ereignissen ist gedeckt, aber die Zeile lässt das im Claim festgelegte Detail aus, dass der Export von der Leitung angestoßen wird, ohne etwas Falsches hinzuzufügen.","ref":"L11","verdict":"supported","rationale":"Die Zeile ist durch die Proposition gedeckt: monatlicher PDF-Export mit allen dokumentierten Ereignissen für die Einrichtung. Dass der Anstoß durch die Leitung erfolgt, ist ein zusätzliches Claim-Detail, dessen Weglassen die Zeile nicht unstimmig macht.","unsupportedSpan":""}]}

---

