Der L3-Agent sollte als übergeordneter Coverage- und Gap-Analysis-Agent für die frühe Anforderungsarbeit umgesetzt werden. Seine Aufgabe besteht nicht nur darin, weitere Requirements zu erzeugen, sondern den bereits vorhandenen Projektstand als Ganzes zu untersuchen, bestehende Inhalte sinnvoll zu erweitern und bislang nicht ausreichend adressierte Themen sichtbar zu machen. Damit übernimmt L3 die Rolle eines kontrollierten Lückenschließers zwischen den bereits extrahierten und abgeleiteten Artefakten aus L1 und L2 sowie einem reviewfähigen Requirements-Stand.

Damit der Agent diese Aufgabe erfüllen kann, benötigt er Zugriff auf Werkzeuge zur Exploration und Kontextauflösung. Entscheidend ist jedoch nicht eine möglichst große Anzahl von Tools, sondern ein kleiner, klar abgegrenzter Werkzeugsatz, der ihm erlaubt, den Projektgraphen selbstständig zu untersuchen, relevante Informationen nachzuladen und vor der Behauptung einer Lücke zu prüfen, ob ein Thema tatsächlich noch nicht behandelt wurde.

Der Agent sollte zunächst einen Überblick über die vorhandene Projektumwelt erhalten. Dafür wird ein Tool wie `list_artifacts()` benötigt. Dieses Tool liefert keine vollständigen Inhalte, sondern eine strukturierte Übersicht über die vorhandenen Artefakttypen, beispielsweise Requirements, Architekturartefakte, Risiken, offene Fragen und menschliche Entscheidungen, einschließlich ihrer Anzahl und optional ihrer Statusverteilung. Dadurch kann der Agent erkennen, welche Wissensbereiche im Projektgraphen grundsätzlich vorhanden sind, ohne dass der vollständige Graph in den initialen Prompt geladen werden muss.

Für die eigentliche Recherche benötigt der Agent ein generisches Suchwerkzeug wie `search_items(query, artifactTypes?, statuses?, origins?, topK?)`. Dieses Tool soll relevante Items anhand fachlicher Begriffe und semantischer Ähnlichkeit auffinden. Gerade für die Gap-Analyse ist diese Funktion zentral. Bevor der Agent beispielsweise behauptet, dass Regeln zur Datenlöschung fehlen, muss er gezielt nach Begriffen wie Löschung, Archivierung, Aufbewahrung oder Datenlebenszyklus suchen können. Ohne diese Suchmöglichkeit würde der Agent Lücken nur anhand des begrenzten Prompt-Kontexts vermuten und könnte Themen fälschlich als unbehandelt kennzeichnen, obwohl sie bereits an anderer Stelle im Projektgraphen vorkommen.

Ergänzend wird ein Tool `get_item(itemId)` benötigt, mit dem ein einzelnes Artefakt vollständig gelesen werden kann. Das Suchwerkzeug sollte zunächst nur kompakte Treffer liefern, während `get_item` die vollständigen fachlichen Inhalte, den Status, den Ursprung und relevante Metadaten bereitstellt. Dadurch kann der Agent gezielt vertiefen, statt den gesamten Graphen pauschal zu laden.

Ein weiteres sinnvolles Werkzeug ist `get_related_items(itemId)`. Dieses Tool soll direkte fachliche Beziehungen eines Items sichtbar machen, beispielsweise abgeleitete Requirements, zugehörige Risiken, Architekturentscheidungen, offene Fragen oder frühere menschliche Entscheidungen. Gerade für einen Coverage-Agenten ist diese Sicht wichtig, weil ein Thema häufig nicht in einem einzelnen Item vollständig abgebildet ist, sondern über mehrere Artefakte verteilt wird. Der Agent kann dadurch erkennen, ob ein Bereich bereits durch verschiedene bestehende Bausteine abgedeckt ist oder tatsächlich eine Lücke besteht.

Das bereits vorhandene Tool `resolve_provenance(itemId)` sollte weiterhin verfügbar sein. Es dient dazu, die Herkunft eines Artefakts über `sourceArtifactItemIds` und `sourceClaimIds` rückwärts aufzulösen. Der Agent kann damit nachvollziehen, aus welchen Requirements, Architekturpunkten oder ursprünglichen Ledger-Claims ein Item entstanden ist. Diese Funktion ist insbesondere dann relevant, wenn der Oberflächentext eines Items verdichtet oder mehrdeutig ist und die ursprüngliche fachliche Motivation verstanden werden muss. Die Nutzung sollte jedoch nicht als vollständiger Pflicht-Vollscan über jedes einzelne Umweltitem umgesetzt werden. Sinnvoller ist, Provenienz für diejenigen Items aufzulösen, die der Agent tatsächlich für eine Erweiterung oder Gap-Beurteilung als relevant ausgewählt hat.

Die mechanische Provenienzauflösung selbst sollte langfristig nicht ausschließlich als agentische Tätigkeit verstanden werden. Der Rückwärts-Walk durch stabile IDs ist deterministisch und kann host-seitig ausgeführt oder gecacht werden. Für die produktive Zielarchitektur empfiehlt sich daher, relevante Provenienzinformationen einmalig aufzubereiten und anschließend sowohl dem Candidate Generator als auch dem Anchor Resolver und dem Judge zur Verfügung zu stellen. Die wertvolle agentische Leistung liegt nicht in der technischen Graphtraversierung, sondern in der Entscheidung, welche Items relevant sind, welche Zusammenhänge untersucht werden müssen und welche neuen Requirements oder Lücken daraus entstehen.

Für die eigentliche Coverage-Analyse sollte L3 einen strukturierten Zwischenschritt erhalten. Der Agent soll nicht unmittelbar nach der Exploration Kandidaten erzeugen, sondern zunächst bewerten, welche fachlichen Bereiche vollständig, teilweise oder gar nicht abgedeckt sind. Diese Zwischenrepräsentation kann entweder als strukturierter Output eines eigenen Workflow-Knotens oder über eine Funktion wie `save_coverage_assessment(assessment)` persistiert werden. Ein Coverage-Eintrag sollte mindestens den untersuchten Bereich, den Abdeckungsstatus, bereits vorhandene Items, fehlende Aspekte und eine Begründung enthalten.

Ein mögliches Format wäre:

```json
{
  "area": "data_lifecycle",
  "status": "partial",
  "coveredBy": ["REQ-08", "ARCH-04"],
  "missingAspects": ["retention_period", "deletion", "archiving"],
  "rationale": "Datenerfassung und Änderung sind beschrieben, die Behandlung nach Abschluss eines Vorgangs jedoch nicht."
}
```

Die Coverage-Analyse sollte anhand definierter Betrachtungsperspektiven erfolgen. Dazu zählen mindestens fachliche Ziele und Kernabläufe, Akteure und Rollen, Eingaben und Validierung, Fehler- und Leerzustände, Zustände und Übergänge, Daten und Datenlebenszyklus, Sicherheit und Missbrauchsfälle, externe Systeme und Integrationen, Betrieb und Wiederherstellung, Qualitätsanforderungen, Nutzungskontext sowie ungeklärte Entscheidungen und Annahmen. Diese Perspektiven sind Prüflinsen und keine Aufforderung, künstlich zu jeder Kategorie einen Kandidaten zu erzeugen.

Nach der Coverage-Analyse erzeugt der Agent Kandidaten. Diese Kandidaten müssen weiterhin klar zwischen `extension` und `gap` unterscheiden. Eine `extension` ist eine Folgeanforderung, die plausibel aus mindestens einem bestehenden Artefakt hervorgeht und später auf tragfähige Anker geprüft werden kann. Ein `gap` ist ein wichtiges, bislang nicht oder nicht ausreichend behandeltes Thema, dessen Relevanz aus dem Gesamtprojekt sichtbar wird, das jedoch nicht als zwingende Ableitung aus einem bestehenden Item ausgegeben werden darf.

Das Feld `basedOn` soll dokumentieren, welche Items der Agent bei seiner Recherche herangezogen hat. Es handelt sich dabei ausdrücklich nicht um finale Anker. `basedOn` beschreibt, wodurch eine Idee ausgelöst oder eine Lücke sichtbar wurde. Die endgültige Verankerung wird in einem getrennten Anchor-Resolution-Schritt durchgeführt.

Der Kandidatenvertrag sollte mindestens folgende Felder enthalten:

```json
{
  "text": "Das System muss eine Regel für die Löschung oder Archivierung abgeschlossener Vorgänge bereitstellen.",
  "targetType": "requirement",
  "intent": "gap",
  "gapCategory": "data_lifecycle",
  "basedOn": ["REQ-08", "ARCH-04"],
  "rationale": "Das System verarbeitet persistente Vorgangsdaten, beschreibt aber nicht deren Behandlung nach Abschluss.",
  "impactIfMissing": "Ohne Regel bleiben Aufbewahrung, Löschung und rechtliche Verantwortung unklar.",
  "assumptions": [],
  "requiresHumanDecision": true
}
```

Das Feld `impactIfMissing` ist besonders wichtig. Es zwingt den Agenten zu begründen, warum eine Lücke für das konkrete Projekt relevant ist. Dadurch wird verhindert, dass allgemeine Best Practices ohne tatsächlichen Projektbezug als neue Requirements vorgeschlagen werden.

Die Kandidaten sollten nicht direkt als finale Projektartefakte gespeichert werden. Der Agent benötigt lediglich eine kontrollierte Persistenzfunktion wie `save_candidates(candidates)` oder alternativ einen typisierten Workflow-Output. Anschließend durchlaufen die Kandidaten die bestehende Pipeline aus Anchor Resolution, deterministischer ID-Validierung, semantischem Judge, Routing und Human Review.

Für die Anchor Resolution sollte der Agent über dieselben Such- und Lesewerkzeuge verfügen. Er soll plausible Ankerkandidaten finden und zu jedem vorgeschlagenen Anker eine behauptete Relation und eine kurze Begründung liefern. Die endgültige Entscheidung, ob der Anker die Aussage tatsächlich trägt, bleibt beim unabhängigen Judge. Der Anchor Resolver ist damit ein semantischer Retriever und Vorschlagsgenerator, nicht die letzte Fidelity-Instanz.

Ein möglicher Vertrag für Anchor Resolution lautet:

```json
{
  "candidateId": "CAND-017",
  "proposedAnchors": [
    {
      "itemId": "REQ-08",
      "relation": "elaborates",
      "reason": "REQ-08 fordert die persistente Verarbeitung von Vorgangsdaten; der Kandidat konkretisiert deren Behandlung nach Abschluss."
    }
  ],
  "noAnchorReason": null
}
```

Die erlaubten Relationen sollten fachlich klar definiert werden. `elaborates`, `depends_on`, `mitigates` und `conflicts_with` können als konkrete Beziehungen verwendet werden. Eine generische Relation wie `relates_to` sollte nicht als tragender Anker ausreichen, da bloße thematische Nähe keine Ableitung beweist. Sie kann optional als Kontextrelation gespeichert werden, darf aber nicht zur Klassifikation `SUPPORTED_ANCHORED` führen.

Die finale Toolausstattung für einen ersten produktiven L3-Durchstich sollte daher bewusst klein bleiben:

```text
list_artifacts()
search_items(query, artifactTypes?, statuses?, origins?, topK?)
get_item(itemId)
get_related_items(itemId)
resolve_provenance(itemId)
```

Zusätzlich werden zwei kontrollierte Schreibschnittstellen beziehungsweise typisierte Workflow-Ausgaben benötigt:

```text
save_coverage_assessment(assessment)
save_candidates(candidates)
```

Die Schreibfunktionen dürfen ausschließlich Kandidaten und Analyseartefakte persistieren. Der Agent darf keine finalen Requirements direkt erzeugen, keine stabilen Projekt-IDs selbst vergeben und keine menschliche Autorisierung ersetzen.

Ein spezielles Tool wie `check_topic_coverage(topic, aspects?)` sollte zunächst nicht gebaut werden. Die vorhandene generische Suche reicht für den ersten Durchstich aus. Ein solches Tool wird erst dann sinnvoll, wenn empirisch beobachtet wird, dass der Agent wiederholt Themen als fehlend markiert, obwohl sie bereits im Graphen vorhanden sind. In diesem Fall könnte eine retrieval-basierte Coverage-Funktion relevante Items sammeln und zwischen vollständig, teilweise und nicht gefundenen Aspekten unterscheiden.

Bestimmte Aufgaben gehören ausdrücklich nicht in den agentischen Werkzeugraum. Die Existenz und Zulässigkeit von IDs, die Vergabe stabiler Identitäten, die Versionserhöhung, das Routing der Kandidaten, die Aggregation von Judge-Ergebnissen sowie das finale Persistieren akzeptierter Projektartefakte müssen deterministisch im Host oder Repository erfolgen. Auch die Autorisierung echter Open-World-Lücken bleibt beim Menschen.

Die empfohlene Gesamtarchitektur lautet damit:

```text
Project Overview
        ↓
agentische Exploration
        ↓
Coverage Assessment
        ↓
Extension- und Gap-Kandidaten
        ↓
Anchor Resolution
        ↓
deterministische Anchor Validation
        ↓
unabhängiger Support Judge
        ↓
Human Review
        ↓
deterministisches Apply
        ↓
Requirements-Closure-Gate
```

Für größere Projektstände sollte langfristig eine hybride Variante verwendet werden. Der Host liefert einen kompakten, verlässlichen Projektüberblick und gegebenenfalls vorberechnete Provenienzinformationen. Der Agent entscheidet anschließend selbstständig, welche Bereiche er vertieft, welche Suchanfragen er ausführt und welche Lücken oder Erweiterungen er vorschlägt. Dadurch bleibt die semantisch wertvolle Agency erhalten, während mechanische Traversierung, Konsistenz und Reproduzierbarkeit deterministisch abgesichert werden.

Der L3-Agent benötigt somit bestimmte Tools, aber nicht deshalb, weil ein agentisches System möglichst viele Werkzeuge besitzen sollte. Er benötigt sie, weil seine zentrale Aufgabe nur dann belastbar erfüllt werden kann, wenn er vor einer Lückenbehauptung aktiv nach vorhandenen Informationen suchen, relevante Artefakte vollständig lesen, Beziehungen untersuchen und bei Bedarf die Herkunft eines Items nachvollziehen kann. Die Qualität des L3-Agenten hängt dabei stärker von einer kohärenten Projektumwelt und klaren Werkzeugverträgen ab als von der reinen Anzahl verfügbarer Funktionen.
