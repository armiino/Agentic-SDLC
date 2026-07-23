# Adjudikations-Queue (lesbar) — fülle die Aktionen in der .queue.json aus

Aktionen: `accept_gap | merge_existing | mark_covered_by | reject | apply_repair | defer`
(merge_existing/mark_covered_by brauchen `referenceTarget`; apply_repair nutzt die systemSuggestion.)

## RR::mvp-baseline-scope  [review_required_claim / normal]
- proposition: Der aktuelle MVP-Schnitt umfasst Login mit Double-Opt-In, Angebotserstellung mit SAP-Lesedaten, Rechnungsdownload, Rollen Admin/Sales/Kunde, minimalen Audit Trail, EU Managed Hosting und Backup.
- systemSuggestion: facet_repair status: decided->open
- evidence: Anna: Okay. Dann MVP: 1. Login mit Double-Opt-In 2. Angebotserstellung mit SAP-Lesedaten 3. Rechnungsdownload 4. Rollen Admin/Sales/Kunde 5. Minimaler Audit Trail 6. EU Managed Hosting 7. Backup
- reason: verdict=partial; Der genannte MVP-Schnitt ist im Transcript genannt, aber nicht als final entschiedener verbindlicher Scope abgesichert.
- ACTION: ____   REASON: ____

## RR::mvp-deadline-8-weeks  [review_required_claim / normal]
- proposition: Ein MVP wurde für 8 Wochen zugesagt.
- systemSuggestion: facet_repair status: decided->uncertain
- evidence: Anna: Ja aber das darf uns jetzt nicht blockieren. MVP in 8 Wochen wurde versprochen.
- reason: verdict=partial; Die 8-Wochen-Vorgabe ist klar erwähnt, aber eher als versprochener Termin unter erheblichem Risiko als als gesicherte Entscheidung.
- ACTION: ____   REASON: ____

## RR::api-layer-and-gateway-policy  [review_required_claim / normal]
- proposition: Für Integrationen wird eine API-Layer benötigt, und neue externe Portale müssen laut interner Policy über das zentrale API Gateway laufen.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Ben: Wir brauchen definitiv eine API Layer. Sonst können wir nichts integrieren. | Farid: Noch ein Punkt: Wir haben eine interne Policy, dass neue externe Portale über das zentrale API Gateway laufen müssen.
- reason: verdict=partial; Inhaltlich klar gedeckt und als Vorgabe/Constraint plausibel, nur die Zuordnung explizit zum MVP ist etwas zu stark.
- ACTION: ____   REASON: ____

## RR::sso-optional-later-open  [review_required_claim / normal]
- proposition: SSO ist wünschenswert, aber optional und eher später möglich; die konkrete IdP-Integration ist offen und es darf keine Providerentscheidung daraus abgeleitet werden.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Anna: SSO wäre schön. Vielleicht Azure AD. Oder Google? Oder beides? | Anna: ... SSO optional ... | Farid: Architektur darf keine konkreten Cloud-Produkte erfinden, solange wir keinen Provider gewählt haben.
- reason: verdict=partial; SSO als optional und IdP offen ist gedeckt, ebenso dass keine Providerentscheidung abgeleitet werden darf; 'eher später' ist jedoch nur indirekt nahegelegt, nicht klar festgelegt.
- ACTION: ____   REASON: ____

## RR::rbac-concept-and-mvp-roles  [review_required_claim / normal]
- proposition: Ein Rollen- und Berechtigungskonzept ist erforderlich; für das MVP sind die Rollen Admin, Sales und Kunde vorgesehen.
- systemSuggestion: facet_repair status: decided->required
- evidence: Clara: Dann brauchen wir Audit Trails. Und Zugriffskontrolle. Und Rollenmodelle. | Clara: Und ein Rollen- und Berechtigungskonzept. | Anna: Okay. Dann MVP: ... 4. Rollen Admin/Sales/Kunde ...
- reason: verdict=partial; Rollen- und Berechtigungskonzept ist klar erforderlich; Admin/Sales/Kunde wird in Annas MVP-Schnitt genannt, aber die Rollenlage ist insgesamt noch nicht vollständig geklärt.
- ACTION: ____   REASON: ____

## RR::sap-masterdata-quality-risk  [review_required_claim / normal]
- proposition: Stammdaten in SAP sind teilweise unvollständig, was die Angebotserstellung beeinträchtigen kann.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Ben: Stammdaten liegen in SAP. Aber die sind teilweise unvollständig.
- reason: verdict=partial; Die unvollständigen SAP-Stammdaten sind klar belegt; der konkrete Bezug nur auf MVP ist jedoch nicht ausdrücklich gemacht.
- ACTION: ____   REASON: ____

## RR::sap-write-and-online-acceptance-outside-current-mvp  [review_required_claim / normal]
- proposition: SAP-Schreibzugriff ist noch nicht entschieden und liegt nicht im aktuellen MVP-Schnitt; entsprechend ist Online-Akzeptanz von Angeboten im MVP eher ausgeschlossen und eher nur Angebotsdownload vorgesehen.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Eva: Und dann entsteht eine Bestellung. Das muss sauber nach SAP zurück. | Ben: SAP-Schreibzugriff hatten wir noch gar nicht entschieden. Vorhin war eher lesend. | Anna: Dann vielleicht nur Angebotsdownload, keine Online-Akzeptanz im MVP.
- reason: verdict=partial; SAP-Schreibzugriff ist offen, und Anna tendiert zu Angebotsdownload statt Online-Akzeptanz im MVP; 'later_possible' ist etwas spezifischer als die Quelle.
- ACTION: ____   REASON: ____

## RR::online-acceptance-compliance-requirements-later  [review_required_claim / normal]
- proposition: Falls Angebote später online akzeptiert werden sollen, sind Nachweis der Zustimmung, Zeitstempel und ggf. AGB-Akzeptanz erforderlich.
- systemSuggestion: facet_repair status: open->required
- evidence: Clara: Dann brauchen wir Nachweis der Zustimmung, Zeitstempel, eventuell AGB-Akzeptanz.
- reason: verdict=partial; Clara nennt die Anforderungen klar für den Fall einer späteren Online-Akzeptanz; offen ist eher die Feature-Entscheidung, nicht der Bedarf dieser Punkte.
- ACTION: ____   REASON: ____

## RR::offers-contain-personal-data  [review_required_claim / normal]
- proposition: Angebote enthalten personenbezogene Daten.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Ben: Aber Angebot selbst enthält personenbezogene Daten.
- reason: verdict=partial; Die Aussage zu personenbezogenen Daten in Angeboten ist gedeckt, aber der explizite MVP-Zeitbezug ist in der zitierten Stelle nicht belegt.
- ACTION: ____   REASON: ____

## RR::tls-transport-encryption-sufficient  [review_required_claim / normal]
- proposition: TLS wird als ausreichende Transportverschlüsselung angesehen; Ende-zu-Ende-Verschlüsselung wird als unrealistisch bewertet.
- systemSuggestion: facet_repair status: decided->open
- evidence: Anna: Also brauchen wir Verschlüsselung. Ende-zu-Ende? Oder reicht TLS? | Ben: TLS reicht. E2E ist unrealistisch.
- reason: verdict=overstated; Ben bewertet TLS als ausreichend und E2E als unrealistisch, aber das ist im Gespräch keine klar beschlossene Muss-Vorgabe.
- ACTION: ____   REASON: ____

## RR::avoid-overengineering  [review_required_claim / normal]
- proposition: Die Lösung soll kein Overengineering verursachen.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Anna: Aber bitte kein Overengineering.
- reason: verdict=partial; Der Kern ist gedeckt, aber die zeitliche Eingrenzung nur auf MVP ist im Transcript nicht eindeutig.
- ACTION: ____   REASON: ____

## RR::customer-deletion-and-deletion-concept-required  [review_required_claim / normal]
- proposition: Wenn ein Kunde die Löschung seiner Daten verlangt, muss dies unterstützt werden; dafür ist ein Löschkonzept erforderlich.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Clara: Aber wenn ein Kunde seine Daten löschen will? | Anna: Dann muss das gehen. | Clara: DSGVO. Wir speichern Kundendaten. ... Dann brauchen wir ... Löschkonzepte.
- reason: verdict=partial; Die Pflicht zur Unterstützung von Löschung und zu einem Löschkonzept ist gedeckt, aber die Zuordnung fest ins MVP ist nicht eindeutig abgesichert.
- ACTION: ____   REASON: ____

## RR::discount-approval-policy-open-but-not-in-mvp  [review_required_claim / normal]
- proposition: Für Angebote mit Rabatt oberhalb einer Schwelle wird ein Freigabeprozess benötigt, mindestens Manager-Freigabe ist genannt; die genaue Schwelle ist zwischen 15 und 20 Prozent unklar, eine zusätzliche Finance-Freigabe ab 30 Prozent ist offen, und der Freigabeprozess ist im aktuellen MVP eher nicht enthalten.
- systemSuggestion: facet_repair timescope: later_possible->mvp_or_later_unclear
- evidence: Eva: Angebote müssen freigegeben werden, wenn der Rabatt über 15 Prozent liegt. | Eva: Mindestens eine Freigabe durch Manager ab 15 Prozent Rabatt. | Anna: Oder ab 20 Prozent. Ich bin mir nicht sicher.
- reason: verdict=partial; Kern ist gedeckt: Freigabebedarf, unklare Schwellen und ungelöster MVP-Zuschnitt. 'later_possible' ist etwas zu fest.
- ACTION: ____   REASON: ____

## RR::offer-workflow-statuses-needed  [review_required_claim / normal]
- proposition: Für Angebote werden Workflow-Status benötigt: draft, pending approval, approved, sent, accepted, rejected.
- systemSuggestion: facet_repair modality: must->must_clarify
- evidence: Ben: Dann brauchen wir Workflow-Status für Angebote: draft, pending approval, approved, sent, accepted, rejected.
- reason: verdict=overstated; Die Statusliste wird genannt, aber als offene technische Folgerung, nicht als festes Muss mit klarem Zeithorizont.
- ACTION: ____   REASON: ____

## RR::market-scope-dach-then-eu-possible  [review_required_claim / normal]
- proposition: Der Startmarkt ist DACH; später ist eine Ausweitung auf EU möglich, USA nur eventuell später.
- systemSuggestion: facet_repair status: decided->open
- evidence: Anna: Zum Start DACH. Später EU. Vielleicht USA, wenn es gut läuft.
- reason: verdict=partial; Start DACH, später EU, USA eventuell ist gedeckt; als entschieden ist es zu stark.
- ACTION: ____   REASON: ____

## RR::i18n-german-english-required  [review_required_claim / normal]
- proposition: Deutsch und Englisch sollen unterstützt werden.
- systemSuggestion: facet_repair status: decided->open
- evidence: Anna: Deutsch und Englisch sollten schon drin sein. | David: Support braucht mindestens Deutsch und Englisch.
- reason: verdict=overstated; Deutsch/Englisch wird klar gewünscht und von Support gestützt, aber nicht als final verpflichtend beschlossen.
- ACTION: ____   REASON: ____

## RR::offer-pdf-export-required  [review_required_claim / normal]
- proposition: Für Angebote wird ein PDF-Export benötigt.
- systemSuggestion: facet_repair status: decided->required
- evidence: Anna: Was ist mit PDF-Export von Angeboten? | Eva: Finance braucht PDF mit rechtlichen Fußnoten und Versionsnummer. | Anna: PDF ist wichtig für Sales.
- reason: verdict=partial; Der Bedarf an PDF-Export ist stark belegt, jedoch nicht als ausdrücklich beschlossene Entscheidung formuliert.
- ACTION: ____   REASON: ____

## RR::environment-separation-required  [review_required_claim / normal]
- proposition: Es werden die Umgebungen Dev, Test und Prod benötigt.
- systemSuggestion: facet_repair status: decided->required
- evidence: Farid: Noch eins: Wir brauchen Umgebungen. Dev, Test, Prod.
- reason: verdict=partial; Der Bedarf nach Dev/Test/Prod ist klar belegt, aber der Eintrag formuliert den Status etwas zu entschieden.
- ACTION: ____   REASON: ____

## RR::secrets-management-required  [review_required_claim / normal]
- proposition: CI/CD benötigt Secrets Management.
- systemSuggestion: facet_repair status: decided->required
- evidence: Farid: Und CI/CD braucht Secrets Management. | Farid: Und "Secrets Management".
- reason: verdict=partial; Secrets Management für CI/CD wird klar gefordert, aber 'decided' ist stärker als die Quelle hergibt.
- ACTION: ____   REASON: ____

## RR::rate-limits-pagination-download-limits-and-audit  [review_required_claim / normal]
- proposition: Für Rechnungs- und API-Zugriffe werden Rate Limits sowie Pagination und Download-Limits benötigt; Audit muss dabei mindestens berücksichtigt werden.
- systemSuggestion: facet_repair modality: must->must_consider
- evidence: Ben: Wir brauchen auch Rate Limits für API. | David: Kunden laden manchmal hunderte Rechnungen auf einmal herunter. | Ben: Dann brauchen wir Pagination und Download-Limits.
- reason: verdict=partial; Der Kern ist gedeckt, aber die Verbindlichkeit für alle aufgezählten Elemente ist etwas zu stark vereinheitlicht.
- ACTION: ____   REASON: ____

## RR::abuse-detection-desired-later  [review_required_claim / normal]
- proposition: Missbrauchserkennung wurde gefordert, ist aber als zu viel für den aktuellen Scope bewertet und eher später möglich.
- systemSuggestion: facet_repair modality: desired->must_consider
- evidence: Ben: Wir brauchen Missbrauchserkennung. | Anna: Das ist zu viel.
- reason: verdict=partial; Dass Missbrauchserkennung genannt und für den aktuellen Scope als zu viel bewertet wird, ist gedeckt; 'desired' ist etwas zu interpretativ.
- ACTION: ____   REASON: ____

## RR::sap-unavailable-offer-policy-open  [review_required_claim / normal]
- proposition: Wenn SAP nicht erreichbar ist, ist offen, ob keine Angebote erstellt werden dürfen oder ob ein Fallback bzw. Cache genutzt werden soll.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: David: Was passiert, wenn SAP nicht erreichbar ist? | Ben: Angebote können dann nicht aktuell berechnet werden. | Eva: Dann darf kein Angebot erstellt werden, oder es muss als unverbindlich markiert sein.
- reason: verdict=partial; Kernaussage ist gedeckt: Bei SAP-Ausfall ist offen, ob blockiert wird oder Fallback/Cache genutzt wird.
- ACTION: ____   REASON: ____

## RR::cache-personal-data-and-invalidation-risk  [review_required_claim / normal]
- proposition: Ein Cache könnte kundenindividuelle Rabatte bzw. Kundendaten enthalten und erzeugt damit Datenschutz-, Retention- und Invalidierungsrisiken.
- systemSuggestion: facet_repair status: decided->open
- evidence: Clara: Cache enthält Kundendaten? | Ben: Teilweise Produkt- und Preisdaten, vielleicht kundenindividuelle Rabatte. | Clara: Dann wieder Datenschutz und Retention.
- reason: verdict=overstated; Die Risiken eines möglichen Caches sind belegt, aber die Facette 'decided' verstärkt die Quelllage.
- ACTION: ____   REASON: ____

## RR::minimal-retention-plan-required  [review_required_claim / normal]
- proposition: Datenklassifikation und Retention Rules werden benötigt; für den MVP ist mindestens ein minimaler Retention-Plan vorgesehen.
- systemSuggestion: facet_repair status: open->decided
- evidence: Ben: Also brauchen wir Datenklassifikation und Retention Rules. | Anna: Das klingt nicht nach MVP. | Clara: Aber ohne Konzept riskieren wir Fehler.
- reason: verdict=partial; Need für Datenklassifikation/Retention und ein minimaler MVP-Plan sind gedeckt; nur der Status ist zu schwach.
- ACTION: ____   REASON: ____

## RR::offer-retention-legal-open  [review_required_claim / normal]
- proposition: Die gesetzliche Aufbewahrungsdauer für Angebote muss durch Legal geprüft werden.
- systemSuggestion: facet_repair timescope: mvp->mvp_or_later_unclear
- evidence: Anna: Wie lange müssen wir Angebote speichern? | Eva: Handelsrechtlich möglicherweise mehrere Jahre, aber das muss Legal prüfen.
- reason: verdict=partial; Die notwendige Legal-Klärung der Aufbewahrungsdauer ist klar belegt.
- ACTION: ____   REASON: ____

## US::AU-0019  [unit_signal / unit]
- proposition: Die Aussage präzisiert die Datenschutzbewertung eines bereits genannten KPI und stützt den bestehenden KPI-Claim, ohne einen neuen eigenständigen Claim zu erzeugen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: kpis-defined
- reason: Die Aussage präzisiert die Datenschutzbewertung eines bereits genannten KPI und stützt den bestehenden KPI-Claim, ohne einen neuen eigenständigen Claim zu erzeugen.
- ACTION: ____   REASON: ____

## US::AU-0024  [unit_signal / unit]
- proposition: Stützt die bestehenden Claims zu minimalem Audit Trail/Logging und dem Prinzip, Komplexität bzw. Overengineering zu vermeiden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: minimal-audit-trail-mvp | avoid-overengineering
- reason: Stützt die bestehenden Claims zu minimalem Audit Trail/Logging und dem Prinzip, Komplexität bzw. Overengineering zu vermeiden.
- ACTION: ____   REASON: ____

## US::AU-0030  [unit_signal / unit]
- proposition: Formuliert den bereits festgehaltenen Zielkonflikt zwischen Kosten/Einfachheit und Skalierbarkeit nur als zusätzliche Begründung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: scalability-needs-clarification | avoid-overengineering
- reason: Formuliert den bereits festgehaltenen Zielkonflikt zwischen Kosten/Einfachheit und Skalierbarkeit nur als zusätzliche Begründung.
- ACTION: ____   REASON: ____

## US::AU-0039  [unit_signal / unit]
- proposition: Bekräftigt den bereits dokumentierten Zusammenhang zwischen Komplexität und Zeit-/MVP-Risiko.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: avoid-overengineering | mvp-not-feasible-as-discussed
- reason: Bekräftigt den bereits dokumentierten Zusammenhang zwischen Komplexität und Zeit-/MVP-Risiko.
- ACTION: ____   REASON: ____

## US::AU-0052  [unit_signal / unit]
- proposition: Zusätzliche Formulierung des bereits erfassten Spannungsfelds zwischen Skalierbarkeit und Overengineering.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: scalability-needs-clarification | avoid-overengineering
- reason: Zusätzliche Formulierung des bereits erfassten Spannungsfelds zwischen Skalierbarkeit und Overengineering.
- ACTION: ____   REASON: ____

## US::AU-0054  [unit_signal / unit]
- proposition: Die Zieldefinition des Vorhabens ist noch nicht klar genug festgelegt.
- systemSuggestion: compare_classification missing_claim
- evidence: goal-angebote-schneller | board-estimate-risk
- reason: Ein expliziter Claim, dass die Zieldefinition noch unklar ist, fehlt; das ist breiter als nur KPIs und relevant für Scope/Planung.
- ACTION: ____   REASON: ____

## US::AU-0065  [unit_signal / unit]
- proposition: Verantwortlichkeiten bzw. Ownership für Architektur und Umsetzung sind noch nicht geklärt.
- systemSuggestion: compare_classification missing_claim
- evidence: documentation-needed-for-review | mvp-not-feasible-as-discussed
- reason: Die ungeklärte Verantwortlichkeit bzw. Ownership für die Umsetzung ist als eigenständiges Projektrisiko noch nicht im Ledger enthalten.
- ACTION: ____   REASON: ____

## US::AU-0066  [unit_signal / unit]
- proposition: Für das Vorhaben wird Architekturverantwortung bzw. ein Architekt benötigt.
- systemSuggestion: compare_classification missing_claim
- evidence: documentation-needed-for-review | mvp-not-feasible-as-discussed
- reason: Der Bedarf nach einem Architekten ist ein eigenständiger Ressourcen-/Governance-Claim, der im Ledger noch fehlt.
- ACTION: ____   REASON: ____

## US::AU-0067  [unit_signal / unit]
- proposition: Stützt den fehlenden Architektur-/Ownership-Punkt als Ressourcengap, sollte aber eher Evidenz zu diesem fehlenden Claim sein als eigener separater Claim.
- systemSuggestion: compare_classification attach_as_evidence
- reason: Stützt den fehlenden Architektur-/Ownership-Punkt als Ressourcengap, sollte aber eher Evidenz zu diesem fehlenden Claim sein als eigener separater Claim.
- ACTION: ____   REASON: ____

## US::AU-0072  [unit_signal / unit]
- proposition: Liefert starke Begründung dafür, warum fehlendes Ticketing aus Support-Sicht problematisch ist, ohne den bestehenden Support-Claim grundlegend zu verändern.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-ticketing-out-of-mvp | support-process-unclear-risk
- reason: Liefert starke Begründung dafür, warum fehlendes Ticketing aus Support-Sicht problematisch ist, ohne den bestehenden Support-Claim grundlegend zu verändern.
- ACTION: ____   REASON: ____

## US::AU-0076  [unit_signal / unit]
- proposition: Untermauert den bereits festgehaltenen Konflikt zwischen Ticket-Persistenz und dem Verbot einer neuen Datenbank.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no-new-db-server | support-process-unclear-risk
- reason: Untermauert den bereits festgehaltenen Konflikt zwischen Ticket-Persistenz und dem Verbot einer neuen Datenbank.
- ACTION: ____   REASON: ____

## US::AU-0077  [unit_signal / unit]
- proposition: Bestätigt nur nochmals, dass Managed Services als Option akzeptabel sind.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: managed-services-allowed
- reason: Bestätigt nur nochmals, dass Managed Services als Option akzeptabel sind.
- ACTION: ____   REASON: ____

## US::AU-0078  [unit_signal / unit]
- proposition: Stützt bestehende offene Punkte zu Providerentscheidung sowie Support-Ticket-Speicherung/Persistenz.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: no-provider-decision | support-process-unclear-risk
- reason: Stützt bestehende offene Punkte zu Providerentscheidung sowie Support-Ticket-Speicherung/Persistenz.
- ACTION: ____   REASON: ____

## US::AU-0083  [unit_signal / unit]
- proposition: Unterstützt die bereits dokumentierte Einordnung des Freigabeprozesses eher nachgelagert bzw. nicht im aktuellen MVP.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: discount-approval-not-in-mvp | freigaben-not-solved
- reason: Unterstützt die bereits dokumentierte Einordnung des Freigabeprozesses eher nachgelagert bzw. nicht im aktuellen MVP.
- ACTION: ____   REASON: ____

## US::AU-0084  [unit_signal / unit]
- proposition: Begründet die Wichtigkeit eines Freigabeprozesses und das Risiko falscher Angebote; die Freigabethematik ist bereits erfasst.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: discount-approval-required | freigaben-not-solved | price-risk-not-accepted
- reason: Begründet die Wichtigkeit eines Freigabeprozesses und das Risiko falscher Angebote; die Freigabethematik ist bereits erfasst.
- ACTION: ____   REASON: ____

## US::AU-0086  [unit_signal / unit]
- proposition: Zusätzliche Evidenz für das bestehende Prinzip, Komplexität im MVP zu begrenzen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: avoid-overengineering
- reason: Zusätzliche Evidenz für das bestehende Prinzip, Komplexität im MVP zu begrenzen.
- ACTION: ____   REASON: ____

## US::AU-0090  [unit_signal / unit]
- proposition: Präzisiert den bereits bestehenden Bedarf an Rollen- und Berechtigungslogik sowie den Support-Sichtbarkeitskonflikt.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: rbac-concept-required | support-visibility-conflict
- reason: Präzisiert den bereits bestehenden Bedarf an Rollen- und Berechtigungslogik sowie den Support-Sichtbarkeitskonflikt.
- ACTION: ____   REASON: ____

## US::AU-0093  [unit_signal / unit]
- proposition: Konkretisiert den bestehenden Konflikt über die Sichtbarkeit von Details für Support.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-visibility-conflict | support-role-open
- reason: Konkretisiert den bestehenden Konflikt über die Sichtbarkeit von Details für Support.
- ACTION: ____   REASON: ____

## US::AU-0094  [unit_signal / unit]
- proposition: Direkte Evidenz für den bereits dokumentierten Zielkonflikt zwischen eingeschränkter Support-Sicht und effektiver Hilfeleistung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-visibility-conflict
- reason: Direkte Evidenz für den bereits dokumentierten Zielkonflikt zwischen eingeschränkter Support-Sicht und effektiver Hilfeleistung.
- ACTION: ____   REASON: ____

## US::AU-0097  [unit_signal / unit]
- proposition: Liefert wichtige technische Begründung zum bestehenden EU-Residency- und Backup-Replikationsrisiko.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: eu-residency-required | backup-required-mvp
- reason: Liefert wichtige technische Begründung zum bestehenden EU-Residency- und Backup-Replikationsrisiko.
- ACTION: ____   REASON: ____

## US::AU-0104  [unit_signal / unit]
- proposition: Stützt die bereits festgehaltenen Risiken durch fehlende Architekturentscheidung und belastete Schätzung/Zeitschiene.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: board-estimate-risk | mvp-not-feasible-as-discussed
- reason: Stützt die bereits festgehaltenen Risiken durch fehlende Architekturentscheidung und belastete Schätzung/Zeitschiene.
- ACTION: ____   REASON: ____

## US::AU-0105  [unit_signal / unit]
- proposition: Passt als Evidenz zum bestehenden Claim, dass nur grob geschätzt werden soll bzw. grobe Schätzung trotz Unsicherheit erfolgt.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: hosting-cost-open | board-estimate-risk
- reason: Passt als Evidenz zum bestehenden Claim, dass nur grob geschätzt werden soll bzw. grobe Schätzung trotz Unsicherheit erfolgt.
- ACTION: ____   REASON: ____

## US::AU-0109  [unit_signal / unit]
- proposition: Unterstützt den bestehenden Punkt, dass für den Vorstand eine Kostenschätzung/Zahlen benötigt werden.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: hosting-cost-open | board-estimate-risk
- reason: Unterstützt den bestehenden Punkt, dass für den Vorstand eine Kostenschätzung/Zahlen benötigt werden.
- ACTION: ____   REASON: ____

## US::AU-0115  [unit_signal / unit]
- proposition: Präzisiert die Produktvision, dass eine Übergangslösung nicht als integriertes Portal gelten würde.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: manual-upload-interim-open | portal-remains-target
- reason: Präzisiert die Produktvision, dass eine Übergangslösung nicht als integriertes Portal gelten würde.
- ACTION: ____   REASON: ____

## US::AU-0129  [unit_signal / unit]
- proposition: Liefert Begründung dafür, warum USA erst später bzw. mit anderem Scope betrachtet werden sollte.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: market-scope-dach-start
- reason: Liefert Begründung dafür, warum USA erst später bzw. mit anderem Scope betrachtet werden sollte.
- ACTION: ____   REASON: ____

## US::AU-0130  [unit_signal / unit]
- proposition: Stützt den bestehenden Internationalisierungsbedarf bei Ausweitung über DACH hinaus.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: i18n-de-en | market-scope-dach-start
- reason: Stützt den bestehenden Internationalisierungsbedarf bei Ausweitung über DACH hinaus.
- ACTION: ____   REASON: ____

## US::AU-0134  [unit_signal / unit]
- proposition: Zusätzliche Begründung für den offenen Mehrwährungsbedarf und dessen Komplexität mit Rabattlogik und SAP.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: currency-support-open
- reason: Zusätzliche Begründung für den offenen Mehrwährungsbedarf und dessen Komplexität mit Rabattlogik und SAP.
- ACTION: ____   REASON: ____

## US::AU-0136  [unit_signal / unit]
- proposition: Ergänzt die offene Pilot-/Schweiz-Frage um Datenschutzimplikationen für CH.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: pilot-customer-open | market-scope-dach-start
- reason: Ergänzt die offene Pilot-/Schweiz-Frage um Datenschutzimplikationen für CH.
- ACTION: ____   REASON: ____

## US::AU-0137  [unit_signal / unit]
- proposition: Präzisiert EU-Hosting im Kontext Schweizer Kunden und verweist auf nötige Vertragsprüfung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: eu-residency-required | pilot-customer-open
- reason: Präzisiert EU-Hosting im Kontext Schweizer Kunden und verweist auf nötige Vertragsprüfung.
- ACTION: ____   REASON: ____

## US::AU-0138  [unit_signal / unit]
- proposition: Unterstützt die bestehenden offenen Anforderungen zu Länder- und Währungsregeln.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: currency-support-open | market-scope-dach-start
- reason: Unterstützt die bestehenden offenen Anforderungen zu Länder- und Währungsregeln.
- ACTION: ____   REASON: ____

## US::AU-0144  [unit_signal / unit]
- proposition: Begründet den bereits festgehaltenen massiven Scope-Einfluss durch internationale Ausweitung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: market-scope-dach-start | mvp-not-feasible-as-discussed
- reason: Begründet den bereits festgehaltenen massiven Scope-Einfluss durch internationale Ausweitung.
- ACTION: ____   REASON: ____

## US::AU-0145  [unit_signal / unit]
- proposition: Ergänzt Datenschutzprüfungsbedarf bei Scope-Ausweitung, semantisch durch bestehende Datenschutz-/Review-Kandidaten abgedeckt.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: dsgvo-mandatory | security-review-required
- reason: Ergänzt Datenschutzprüfungsbedarf bei Scope-Ausweitung, semantisch durch bestehende Datenschutz-/Review-Kandidaten abgedeckt.
- ACTION: ____   REASON: ____

## US::AU-0146  [unit_signal / unit]
- proposition: Kurzbeleg für zusätzlichen Währungsbedarf als Scope-Treiber.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: currency-support-open
- reason: Kurzbeleg für zusätzlichen Währungsbedarf als Scope-Treiber.
- ACTION: ____   REASON: ____

## US::AU-0149  [unit_signal / unit]
- proposition: Unterstützt das Hauptziel, Angebote schnell erstellen zu können.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: goal-angebote-schneller
- reason: Unterstützt das Hauptziel, Angebote schnell erstellen zu können.
- ACTION: ____   REASON: ____

## US::AU-0154  [unit_signal / unit]
- proposition: Ergänzt Audit-Relevanz falscher Preise; passt als Detail zu Preis- und Audit-Risiken.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: price-risk-not-accepted | auditability-required | offer-contains-personal-data
- reason: Ergänzt Audit-Relevanz falscher Preise; passt als Detail zu Preis- und Audit-Risiken.
- ACTION: ____   REASON: ____

## US::AU-0155  [unit_signal / unit]
- proposition: Stützt die Aussage, dass Fehler bei Angeboten Supportaufwand und Tickets erzeugen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-process-unclear-risk
- reason: Stützt die Aussage, dass Fehler bei Angeboten Supportaufwand und Tickets erzeugen.
- ACTION: ____   REASON: ____

## US::AU-0158  [unit_signal / unit]
- proposition: Prüfen, ob hier auf einen zuvor nicht erfassten fachlichen Punkt verwiesen wird.
- systemSuggestion: compare_classification needs_human
- reason: Reiner Gesprächsmarker ohne klaren eigenständigen Sachclaim; Kontext fehlt.
- ACTION: ____   REASON: ____

## US::AU-0167  [unit_signal / unit]
- proposition: Unterstreicht die bereits zentrale MVP-Scope-Abgrenzung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: mvp-scope-baseline | scope-must-show-exclusions-and-risks
- reason: Unterstreicht die bereits zentrale MVP-Scope-Abgrenzung.
- ACTION: ____   REASON: ____

## US::AU-0169  [unit_signal / unit]
- proposition: Bringt den Online-Akzeptanz-Wunsch in die Diskussion ein; der Themenkomplex ist bereits im Ledger erfasst.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: online-acceptance-out-of-mvp-current | online-acceptance-compliance-open
- reason: Bringt den Online-Akzeptanz-Wunsch in die Diskussion ein; der Themenkomplex ist bereits im Ledger erfasst.
- ACTION: ____   REASON: ____

## US::AU-0170  [unit_signal / unit]
- proposition: Bestärkt den Wunsch nach Online-Akzeptanz, ohne neuen eigenständigen Claim zu schaffen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: online-acceptance-compliance-open
- reason: Bestärkt den Wunsch nach Online-Akzeptanz, ohne neuen eigenständigen Claim zu schaffen.
- ACTION: ____   REASON: ____

## US::AU-0171  [unit_signal / unit]
- proposition: Begründet, warum Online-Akzeptanz ein separates Vertrags-/Compliance-Thema ist.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: online-acceptance-compliance-open | online-acceptance-out-of-mvp-current
- reason: Begründet, warum Online-Akzeptanz ein separates Vertrags-/Compliance-Thema ist.
- ACTION: ____   REASON: ____

## US::AU-0176  [unit_signal / unit]
- proposition: Liefert Begründung aus KPI-/Tracking-Sicht für die Online-Akzeptanz-Debatte.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: online-acceptance-compliance-open | analytics-mvp-open | kpis-defined
- reason: Liefert Begründung aus KPI-/Tracking-Sicht für die Online-Akzeptanz-Debatte.
- ACTION: ____   REASON: ____

## US::AU-0178  [unit_signal / unit]
- proposition: Unterstützt den Bedarf nach Scope-Entscheidung und klarer Abgrenzung.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: scope-must-show-exclusions-and-risks
- reason: Unterstützt den Bedarf nach Scope-Entscheidung und klarer Abgrenzung.
- ACTION: ____   REASON: ____

## US::AU-0179  [unit_signal / unit]
- proposition: Unterstützt den dokumentationsbezogenen Governance-Bedarf.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: documentation-needed-for-review | scope-must-show-exclusions-and-risks
- reason: Unterstützt den dokumentationsbezogenen Governance-Bedarf.
- ACTION: ____   REASON: ____

## US::AU-0182  [unit_signal / unit]
- proposition: Zusätzlicher Beleg für unterschiedliche Aufbewahrungsfristen und Retention-Regeln.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: retention-rules-required | offer-retention-legal-open
- reason: Zusätzlicher Beleg für unterschiedliche Aufbewahrungsfristen und Retention-Regeln.
- ACTION: ____   REASON: ____

## US::AU-0189  [unit_signal / unit]
- proposition: Konkretisiert den Support-Bedarf nach Einsicht in ältere Angebote.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-visibility-conflict | support-role-open
- reason: Konkretisiert den Support-Bedarf nach Einsicht in ältere Angebote.
- ACTION: ____   REASON: ____

## US::AU-0190  [unit_signal / unit]
- proposition: Es muss geklärt werden, ob Finance Einsicht in historische Angebote benötigt und wie dies im Rollen- und Berechtigungskonzept abgebildet wird.
- systemSuggestion: compare_classification missing_claim
- evidence: rbac-concept-required | retention-rules-required
- reason: Finance-Einsicht in alte Angebote ist im Ledger nicht explizit als Berechtigungs-/Zugriffsanforderung erfasst.
- ACTION: ____   REASON: ____

## US::AU-0195  [unit_signal / unit]
- proposition: Bestätigt den bereits erfassten offenen bzw. ungelösten Freigabeprozess.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: freigaben-not-solved | freigabeprozess-fehlt | approval-threshold-conflict
- reason: Bestätigt den bereits erfassten offenen bzw. ungelösten Freigabeprozess.
- ACTION: ____   REASON: ____

## US::AU-0196  [unit_signal / unit]
- proposition: Bestätigt, dass der Support-Aspekt im MVP-Schnitt noch fehlt bzw. unklar ist.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: support-process-unclear-risk | support-role-open
- reason: Bestätigt, dass der Support-Aspekt im MVP-Schnitt noch fehlt bzw. unklar ist.
- ACTION: ____   REASON: ____

## US::AU-0213  [unit_signal / unit]
- proposition: Unterstreicht direkt den bereits erfassten Claim, dass der Scope klar dokumentiert werden muss.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: scope-must-show-exclusions-and-risks
- reason: Unterstreicht direkt den bereits erfassten Claim, dass der Scope klar dokumentiert werden muss.
- ACTION: ____   REASON: ____

## US::AU-0219  [unit_signal / unit]
- proposition: Begründet, warum das Testdaten-Thema sofort relevant ist, wenn Entwicklung darauf zugreift.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: sap-test-data-blocker | no-real-customer-data-in-test
- reason: Begründet, warum das Testdaten-Thema sofort relevant ist, wenn Entwicklung darauf zugreift.
- ACTION: ____   REASON: ____

## US::AU-0225  [unit_signal / unit]
- proposition: Die Lösung soll keine schlechte User Experience verursachen; UX ist bei Scope- und Architekturentscheidungen zu berücksichtigen.
- systemSuggestion: compare_classification missing_claim
- reason: Ein explizites UX-/Usability-Prinzip ist im Ledger bislang nicht als eigener Claim enthalten.
- ACTION: ____   REASON: ____

## US::AU-0226  [unit_signal / unit]
- proposition: Präzisiert die bereits erfassten Anforderungen und Limits rund um Rechnungsdownload durch den Hinweis auf rechtliche Relevanz bei Massenexporten.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: invoice-download-in-scope | rate-limits-pagination-download-limits
- reason: Präzisiert die bereits erfassten Anforderungen und Limits rund um Rechnungsdownload durch den Hinweis auf rechtliche Relevanz bei Massenexporten.
- ACTION: ____   REASON: ____

## US::AU-0227  [unit_signal / unit]
- proposition: Ergänzt dieselbe Argumentation um den Sicherheitsaspekt und stützt damit bestehende Download-Limit- und Schutzanforderungen.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: rate-limits-pagination-download-limits | security-review-required
- reason: Ergänzt dieselbe Argumentation um den Sicherheitsaspekt und stützt damit bestehende Download-Limit- und Schutzanforderungen.
- ACTION: ____   REASON: ____

## US::AU-0231  [unit_signal / unit]
- proposition: Ist eine Umsetzungsoption für den bereits erfassten Gateway-Claim, keine neue eigenständige Aussage.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: api-gateway-policy | api-gateway-waitlist-risk
- reason: Ist eine Umsetzungsoption für den bereits erfassten Gateway-Claim, keine neue eigenständige Aussage.
- ACTION: ____   REASON: ____

## US::AU-0233  [unit_signal / unit]
- proposition: Bewertet den bereits erfassten Wartelistenpunkt als Zeitproblem und stützt den bestehenden Zeitplan-Risiko-Claim.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: api-gateway-waitlist-risk | mvp-deadline-8-weeks
- reason: Bewertet den bereits erfassten Wartelistenpunkt als Zeitproblem und stützt den bestehenden Zeitplan-Risiko-Claim.
- ACTION: ____   REASON: ____

## US::AU-0237  [unit_signal / unit]
- proposition: Stützt den bestehenden Claim, dass Kostenschätzungen bzw. Angebote ohne klaren Scope problematisch sind.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: board-estimate-risk
- reason: Stützt den bestehenden Claim, dass Kostenschätzungen bzw. Angebote ohne klaren Scope problematisch sind.
- ACTION: ____   REASON: ____

## US::AU-0242  [unit_signal / unit]
- proposition: Ist eine konkrete Ausprägung der bereits offenen Caching-/Datenminimierungsfrage.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: data-minimization-open | sap-unavailable-offer-policy-open | cache-personal-data-risk
- reason: Ist eine konkrete Ausprägung der bereits offenen Caching-/Datenminimierungsfrage.
- ACTION: ____   REASON: ____

## US::AU-0243  [unit_signal / unit]
- proposition: Begründet das bereits erfasste Risiko, dass Caching wegen kundenspezifischer Rabattlogik personenbezogene bzw. kundenspezifische Daten betreffen kann.
- systemSuggestion: compare_classification attach_as_evidence
- evidence: cache-personal-data-risk | data-minimization-open
- reason: Begründet das bereits erfasste Risiko, dass Caching wegen kundenspezifischer Rabattlogik personenbezogene bzw. kundenspezifische Daten betreffen kann.
- ACTION: ____   REASON: ____

## US::AU-0255  [unit_signal / unit]
- proposition: Risiken sind nicht nur technisch zu bewerten, sondern systematisch auch finanziell und organisatorisch zu erfassen.
- systemSuggestion: compare_classification missing_claim
- reason: Der Ledger enthält viele einzelne nichttechnische Risiken, aber kein explizites Querschnittsprinzip, Risiken auch finanziell und organisatorisch systematisch zu betrachten.
- ACTION: ____   REASON: ____

