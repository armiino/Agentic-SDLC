# Response Text — Checker-requirements

## Model Round 1

- No tool calls (standalone text response)
- Text length: 1480 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition korrekt und markiert die Ausgestaltung als offen, ohne den offenen Status unzulässig zu verhärten.","ref":"L7","verdict":"supported","rationale":"Alle Details sind im Claim-Paket enthalten: rotes Stopp-Symbol, deutliches Warnsignal; die Offenheit der konkreten Ausgestaltung ist durch den offenen Status bzw. die Notes gedeckt.","unsupportedSpan":""},{"reasoning":"Die Zeile entspricht inhaltlich exakt dem Claim und ergänzt nur die zulässige Klarstellung, dass die Umsetzung noch offen ist.","ref":"L9","verdict":"supported","rationale":"Bearbeiten und Löschen nachträglich ist direkt in Proposition/Evidence enthalten; die offene konkrete Umsetzung ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile verschärft die Modalität von 'soll' zu 'muss' und überzieht damit den im Paket belegten Stand.","ref":"L11","verdict":"facet_overstated","rationale":"Das Claim-Paket formuliert die Proposition mit 'soll', während die Zeile 'muss' sagt; damit wird die Verbindlichkeit gegenüber dem Paket verstärkt.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt die gewünschte globale statt bewohnerbezogene Führung als Wunsch und offene Scope-Entscheidung korrekt wieder.","ref":"L15","verdict":"supported","rationale":"Die Proposition ist als desired markiert und die Notes stellen klar, dass es eine Präferenz mit offener Scope-Entscheidung ist; genau das sagt die Zeile.","unsupportedSpan":""}]}

---

## Model Round 2

- No tool calls (standalone text response)
- Text length: 1510 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition und markiert die Ausgestaltung als offen, was durch den offenen Status gedeckt ist.","ref":"L7","verdict":"supported","rationale":"Rotes Stopp-Symbol als Warnsignal ist wörtlich belegt; die Formulierung, dass die konkrete Ausgestaltung noch offen ist, ist mit dem offenen Status vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile entspricht dem Claim zu Bearbeiten und Löschen und ergänzt nur zulässig, dass die Umsetzung noch offen ist.","ref":"L9","verdict":"supported","rationale":"Nachträgliches Bearbeiten und Löschen ist direkt in Proposition/Evidence enthalten; 'konkrete Umsetzung ist noch offen' passt zum offenen Status.","unsupportedSpan":""},{"reasoning":"Die Zeile deckt die geforderte Notiz pro Schicht und die Anzeige für die nächste Schicht ab; die Offenheit der Ausgestaltung ist nicht überzogen.","ref":"L11","verdict":"supported","rationale":"Die fachliche Funktion 'pro Schicht' plus Anzeige an die nächste Schicht ist vollständig im Paket enthalten; 'konkrete Ausgestaltung ist noch offen' ist mit status=open kompatibel.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt die gewünschte globale statt bewohnerbezogene Führung korrekt als Wunsch und offene Scope-Entscheidung wieder.","ref":"L15","verdict":"supported","rationale":"Die Proposition ist als desired formuliert, und die Notes stellen klar, dass es eine Präferenz mit offener Scope-Entscheidung ist; genau das sagt die Zeile aus.","unsupportedSpan":""}]}

---

## Model Round 3

- No tool calls (standalone text response)
- Text length: 1340 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition korrekt und markiert die Ausgestaltung als offen, was zum offenen Status passt.","ref":"L7","verdict":"supported","rationale":"Alle konkreten Details sind durch Proposition/Evidence gedeckt; „konkrete Ausgestaltung ist noch offen“ ist mit dem offenen Status vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile übernimmt Bearbeiten und Löschen nachträglich als Muss und ergänzt nur zulässig, dass die Umsetzung noch offen ist.","ref":"L9","verdict":"supported","rationale":"Bearbeiten und Löschen nachträglich sind explizit im Paket enthalten; die offene konkrete Umsetzung widerspricht dem Paket nicht.","unsupportedSpan":""},{"reasoning":"Die Funktion ist gedeckt, aber die Zeile verschärft „soll“ zu „muss“ ohne entsprechende must-Facette im Paket.","ref":"L11","verdict":"facet_overstated","rationale":"Das Paket formuliert die Proposition als „soll“, während die Zeile „muss“ sagt; das verstärkt die Verbindlichkeit über das Paket hinaus.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt die desired-Präferenz als Wunsch und die Scope-Entscheidung als offen korrekt wieder.","ref":"L15","verdict":"supported","rationale":"„als Wunsch eingebracht“ entspricht modality=desired, und die offene Scope-Entscheidung ist durch die Notes gedeckt.","unsupportedSpan":""}]}

---

## Model Round 4

- No tool calls (standalone text response)
- Text length: 1502 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition und markiert die Ausgestaltung als offen, ohne zusätzliche ungedeckte Details einzuführen.","ref":"L7","verdict":"supported","rationale":"Das rote Stopp-Symbol als Warnsignal ist direkt in Proposition/Evidence enthalten; die Ergänzung, dass die konkrete Ausgestaltung noch offen ist, ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile entspricht inhaltlich genau dem Claim und hält die Umsetzung ausdrücklich offen.","ref":"L9","verdict":"supported","rationale":"Nachträgliches Bearbeiten und Löschen ist direkt durch Proposition/Evidence gedeckt; 'konkrete Umsetzung ist noch offen' ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile übernimmt den fachlichen Inhalt des Claims und formuliert die Offenheit passend zum offenen Status.","ref":"L11","verdict":"supported","rationale":"Eine Schichtübergabe-Notiz pro Schicht mit Anzeige für die nächste Schicht steht so in Proposition/Evidence; die offene Ausgestaltung überzieht die Facette nicht.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt die gewünschte globale statt bewohnerbezogene Führung korrekt als Wunsch mit offener Scope-Entscheidung wieder.","ref":"L15","verdict":"supported","rationale":"Die globale statt pro-Bewohner-Führung ist als desired in Proposition/Evidence enthalten; die Formulierung 'als Wunsch eingebracht' und 'noch offen' entspricht den Notes und verstärkt den Status nicht.","unsupportedSpan":""}]}

---

## Model Round 5

- No tool calls (standalone text response)
- Text length: 1503 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition und markiert die Ausgestaltung korrekt als noch offen, ohne zusätzliche ungedeckte Details.","ref":"L7","verdict":"supported","rationale":"Das rote Stopp-Symbol als deutliches Warnsignal ist direkt durch Proposition/Evidence gedeckt; „konkrete Ausgestaltung ist noch offen“ ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile entspricht inhaltlich dem Claim und ergänzt nur zulässig, dass die konkrete Umsetzung noch offen ist.","ref":"L9","verdict":"supported","rationale":"Nachträgliches Bearbeiten und Löschen von No-Go-Einträgen ist direkt durch Proposition/Evidence gedeckt; die offene konkrete Umsetzung ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt den Claim vollständig wieder und überzieht den offenen Stand nicht.","ref":"L11","verdict":"supported","rationale":"Eine Schichtübergabe-Notiz pro Schicht mit Anzeige für die nächste Schicht ist direkt durch Proposition/Evidence/Notes gedeckt; „konkrete Ausgestaltung ist noch offen“ passt zu status=open.","unsupportedSpan":""},{"reasoning":"Die Zeile stellt die Aussage korrekt als Wunsch bzw. Präferenz mit offener Scope-Entscheidung dar.","ref":"L15","verdict":"supported","rationale":"Die gewünschte globale statt bewohnerbezogene Führung von No-Gos ist durch Proposition/Evidence gedeckt; die Formulierung als Wunsch und offene Scope-Entscheidung entspricht modality=desired und den Notes.","unsupportedSpan":""}]}

---

## Model Round 6

- No tool calls (standalone text response)
- Text length: 1470 chars

> {"items":[{"reasoning":"Die Zeile paraphrasiert die Proposition treu und markiert die Ausgestaltung als offen, was zum offenen Status passt.","ref":"L7","verdict":"supported","rationale":"Alle konkreten Details sind im Claim-Paket enthalten; die Ergänzung, dass die konkrete Ausgestaltung noch offen ist, ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile übernimmt Bearbeiten und Löschen korrekt aus dem Claim und kennzeichnet die Umsetzung passend als noch offen.","ref":"L9","verdict":"supported","rationale":"Bearbeiten und Löschen sind gemeinsam im Paket belegt; die Aussage, dass die konkrete Umsetzung noch offen ist, ist durch den offenen Status gedeckt.","unsupportedSpan":""},{"reasoning":"Die Zeile entspricht inhaltlich exakt der Proposition und fügt nur die zum offenen Status passende Offenheit der Ausgestaltung hinzu.","ref":"L11","verdict":"supported","rationale":"Pro Schicht, Schichtübergabe-Notiz und Anzeige an die nächste Schicht sind vollständig im Paket enthalten; 'konkrete Ausgestaltung ist noch offen' ist mit status=open vereinbar.","unsupportedSpan":""},{"reasoning":"Die Zeile gibt die gewünschte globale statt bewohnerbezogene Führung korrekt als Wunsch mit offener Scope-Entscheidung wieder.","ref":"L15","verdict":"supported","rationale":"Die Formulierung 'als Wunsch eingebracht' entspricht modality=desired, und die offene Scope-Entscheidung ist ausdrücklich durch die Notes gedeckt.","unsupportedSpan":""}]}

---

