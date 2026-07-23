# Response Text — Checker-requirements

## Model Round 1

- No tool calls (standalone text response)
- Text length: 2266 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile entspricht der Proposition vollständig: No-Gos sind pro Bewohner zu führen und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"facet_overstated","rationale":"Das Paket enthält zwar den UI-Wunsch und dass er noch nicht verbindlich entschieden ist, aber die Zeile formuliert den Inhalt als feste Soll-Anforderung statt als offenen Bedarf/Wunsch.","unsupportedSpan":""},{"ref":"L7","verdict":"facet_overstated","rationale":"Das Paket ist status=open. Die Zeile ergänzt zwar korrekt, dass der Punkt noch offen ist, formuliert den Inhalt aber dennoch als bereits feststehende Muss-Anforderung.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die optionale Vorlagen-Liste sowie der Ausschluss einer automatischen globalen Geltung sind im Claim-Paket ausdrücklich enthalten.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist gedeckt; auch dass der Umsetzungszeitpunkt noch nicht präzisiert ist, steht in den Notes/Facets.","unsupportedSpan":""},{"ref":"L12","verdict":"facet_overstated","ratio ...(truncated)
> *...[truncated]*

---

## Model Round 2

- No tool calls (standalone text response)
- Text length: 2293 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Gos bleiben pro Bewohner und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"supported","rationale":"Das rote Stopp-Symbol als deutliches Warnsignal mit sofortiger Erkennbarkeit ist in Proposition/Evidence enthalten; „Dieser Punkt ist noch offen“ wird durch den offenen Status in den Notes gedeckt.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Bearbeiten und Löschen von No-Go-Einträgen ist wörtlich im Claim enthalten; der Zusatz, dass der Punkt noch offen ist, passt zum offenen Status.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die optionale Vorlagen-Liste mit häufigen No-Gos sowie die fehlende automatische globale Geltung sind vollständig im Claim-Paket enthalten.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist belegt; der nicht präzisierte Umsetzungszeitpunkt entspricht dem unklaren timeScope und den Notes.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Pro Schicht eine Ü ...(truncated)
> *...[truncated]*

---

## Model Round 3

- No tool calls (standalone text response)
- Text length: 2425 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Go-Einträge bleiben pro Bewohner und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"facet_overstated","rationale":"Das Paket enthält zwar den Bedarf nach einem roten Stopp-Symbol, aber der Status ist offen/noch nicht verbindlich entschieden. Die Formulierung „soll ... erhalten“ stellt es zu stark als Anforderung dar; nur der Nachsatz zum offenen Punkt ist facet-treu.","unsupportedSpan":""},{"ref":"L7","verdict":"facet_overstated","rationale":"Das Paket markiert den Punkt als offen. Die Zeile formuliert die Funktion mit „müssen ... können“ als feststehende Anforderung und verstärkt damit den Entscheidungsstand über die Facette hinaus, obwohl sie danach „noch offen“ ergänzt.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die Zeile entspricht der Proposition: optionale Vorlagen-Liste mit häufigen No-Gos, ohne automatische globale Geltung.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist im Paket enthalten; der Umsetzungszeitpu ...(truncated)
> *...[truncated]*

---

## Model Round 4

- No tool calls (standalone text response)
- Text length: 2073 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert exakt die Proposition: No-Gos sind pro Bewohner zu führen und dürfen nicht global gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"facet_overstated","rationale":"Das Paket hat modality=must bei status=open; die Zeile spricht nur von einem 'Wunsch' statt des im Paket formulierten Soll-/Muss-Bedarfs. 'noch offen' ist zwar passend, aber die Modality wird abgeschwächt.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Bearbeiten und Löschen nachträglich sowie der offene Status sind durch Proposition und Notes gedeckt.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Optionale Vorlagen-Liste und keine automatische globale Geltung entsprechen genau der Proposition.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist gedeckt; der nicht präzisierte Umsetzungszeitpunkt entspricht timeScope und Notes.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Bedarf nach Übergabe-Notiz pro Schicht mit prominenter Anzeige für die nächste Schicht beim Öffnen der App sowie offener ...(truncated)
> *...[truncated]*

---

## Model Round 5

- No tool calls (standalone text response)
- Text length: 2463 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Go-Einträge bleiben pro Bewohner und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"facet_overstated","rationale":"Das Paket hat status=open und notes sagen, der UI-Bedarf ist noch nicht verbindlich entschieden. Die Zeile formuliert zusätzlich einen 'Wunsch' und 'noch offen', aber behauptet zugleich 'besteht' dieser Punkt im Sinne einer bestehenden Anforderung; die Facette ist offen und nicht als feststehender Wunsch entschieden. Vor allem ist modality=must, nicht bloß Wunsch.","unsupportedSpan":"besteht der Wunsch"},{"ref":"L7","verdict":"supported","rationale":"Bearbeiten und Löschen nachträglich ist in Proposition/Evidence enthalten; 'Dieser Punkt ist noch offen' ist durch status=open gedeckt.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die Zeile entspricht der Proposition: optionale Vorlagen-Liste mit häufigen No-Gos, ohne automatische globale Geltung.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche über alle Bewohnerprofile hinweg nach Stichworten ist belegt; der nich ...(truncated)
> *...[truncated]*

---

## Model Round 6

- No tool calls (standalone text response)
- Text length: 2143 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Go-Einträge bleiben pro Bewohner und dürfen nicht global gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"facet_overstated","rationale":"Das Paket hat modality=must und notes sagen, der Bedarf ist noch nicht verbindlich entschieden. Die Zeile formuliert dies als bloßen Wunsch statt als Muss/Anforderung, obwohl die Offenheit nur den Entscheidungsstatus betrifft.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Bearbeiten und Löschen von No-Go-Einträgen ist im Paket enthalten; 'Dieser Punkt ist noch offen' entspricht status=open.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die optionale Vorlagen-Liste und die fehlende automatische globale Geltung sind vollständig im Claim-Paket enthalten.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile ist enthalten; der nicht präzisierte Umsetzungszeitpunkt entspricht timeScope=mvp_or_later_unclear und den Notes.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Die Zeile deckt die benötigte Über ...(truncated)
> *...[truncated]*

---

## Model Round 7

- No tool calls (standalone text response)
- Text length: 2237 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Go-Einträge bleiben pro Bewohner und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"supported","rationale":"Rotes Stopp-Symbol, deutliches Warnsignal, sofortige Erkennbarkeit kritischer Inhalte und der offene Status sind durch Proposition, Evidence und Notes gedeckt.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Die Zeile entspricht dem offenen Bedarf, dass No-Go-Einträge nachträglich bearbeitet und gelöscht werden können.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Optionale Vorlagen-Liste mit häufigen No-Gos sowie der Ausschluss einer automatischen globalen Geltung sind vollständig im Claim-Paket enthalten.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist gedeckt; dass der Umsetzungszeitpunkt noch nicht präzisiert ist, entspricht dem timeScope und den Notes.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Bedarf nach einer Übergabe-Notiz pro Schicht mit prominenter Anzeige ...(truncated)
> *...[truncated]*

---

## Model Round 8

- No tool calls (standalone text response)
- Text length: 2217 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile entspricht inhaltlich der Proposition: No-Gos bleiben pro Bewohner und dürfen nicht global für alle Bewohner gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"supported","rationale":"Rotes Stopp-Symbol, Warnsignal, sofortige Erkennbarkeit kritischer Inhalte sind durch Proposition/Evidence gedeckt; 'noch offen' passt zu status=open.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Nachträgliches Bearbeiten und Löschen ist explizit im Claim enthalten; 'noch offen' ist durch status=open gedeckt.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Optionale Vorlagen-Liste mit häufigen No-Gos sowie keine automatische globale Geltung sind direkt durch die Proposition gedeckt.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist explizit enthalten; dass der Umsetzungszeitpunkt noch nicht präzisiert ist, wird in den Notes/timeScope gestützt.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Übergabe-Notiz pro Schicht und prominente Anzeige beim Öffnen der App für die nächste Schicht s ...(truncated)
> *...[truncated]*

---

## Model Round 9

- No tool calls (standalone text response)
- Text length: 2184 chars *(truncated to 1215)*

> {"items":[{"ref":"L5","verdict":"supported","rationale":"Die Zeile paraphrasiert die Proposition vollständig: No-Gos müssen pro Bewohner geführt werden und dürfen nicht global gelten.","unsupportedSpan":""},{"ref":"L6","verdict":"supported","rationale":"Rotes Stopp-Symbol, Funktion als Warnsignal, sofortige Erkennbarkeit kritischer Inhalte und der offene Status sind durch Proposition, Evidence und Notes gedeckt.","unsupportedSpan":""},{"ref":"L7","verdict":"supported","rationale":"Die Zeile gibt den offenen Bedarf korrekt wieder: Einträge sollen nachträglich bearbeitet und gelöscht werden können; die Notes halten die Entscheidung noch offen.","unsupportedSpan":""},{"ref":"L8","verdict":"supported","rationale":"Die optionale Vorlagen-Liste mit häufigen No-Gos und die fehlende automatische globale Geltung sind exakt durch das Claim-Paket gedeckt.","unsupportedSpan":""},{"ref":"L11","verdict":"supported","rationale":"Suche nach Stichworten über alle Bewohnerprofile hinweg ist gedeckt; auch dass der Umsetzungszeitpunkt noch nicht präzisiert ist, steht in den Notes/TimeScope.","unsupportedSpan":""},{"ref":"L12","verdict":"supported","rationale":"Pro-Schicht-Übergabe-Notiz, prominente An ...(truncated)
> *...[truncated]*

---

