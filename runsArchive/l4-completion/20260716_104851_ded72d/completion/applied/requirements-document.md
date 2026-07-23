# Requirements Document

Project: `evidenz-agent-demo`
Baseline: `baseline-20260716_130307`
Created UTC: `2026-07-16T13:03:07.9658900Z`
Source ProjectState: `runs/project-state/20260715_l3_v2/project-state.json`

## Überblick

- Requirements gesamt: `69`
- Aktive Requirements: `42`
- Offene Entscheidungen: `27`
- Ready for issue planning: `31`
- Needs decision: `28`
- Needs breakdown: `8`
- Deferred / optional: `2`
- Traceability blockiert: `0`

Dieses Dokument ist eine deterministisch gerenderte Projektion der kanonischen L4-Baseline. Die maschinenlesbare Wahrheit bleibt `canonical-requirements-baseline.json`.

## Scope und Nicht-Ziele

### CAN-REQ-047 - App nur für internen Gebrauch vorsehen

Die App ist nur für den internen Gebrauch vorgesehen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-36`
- L4 operation: `KEEP` (`COP-047`)
- Ledger claims: `canon_internal_use_only`

### CAN-REQ-054 - Übersetzungssystem zwischen Bewohnern und Betreuern aus Scope ausschließen

Ein System, das als Schnittstelle zwischen Bewohnern und Betreuern hin und her übersetzt, ist ausdrücklich nicht Teil des Lösungsumfangs.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-44`
- L4 operation: `KEEP` (`COP-054`)
- Ledger claims: `canon_exclude_translation_system`

## Nutzerrollen und Berechtigungen

### CAN-REQ-016 - Angehörige sollen ergänzenden Zugriff erhalten können

Neben Mitarbeitern sollen auch Angehörige Zugriff auf die App erhalten können, damit sie ergänzende Daten und Wissen hinzufügen können, die den Mitarbeitern sonst erst bekannt werden müssten.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-04`
- L4 operation: `KEEP` (`COP-016`)
- Ledger claims: `canon_employee_and_relative_access`

### CAN-REQ-017 - Bewohner-Account mit eingeschränkten Funktionen vorsehen

Es soll zusätzlich einen Bewohner-Account geben, der nur das eigene Profil in der Profilübersicht sehen kann und nur eingeschränkte Funktionen erhält, insbesondere Zugriff auf den About-Me-Bereich und das Hinzufügen eigener Bilder; dies ist als gewünschte Anforderung vorgesehen, um Fehlbedienungen zu minimieren.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-05`
- L4 operation: `KEEP` (`COP-017`)
- Ledger claims: `canon_resident_account_with_restrictions`

### CAN-REQ-044 - Rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen

Die App soll rollenbasierte Accounts mit unterschiedlichen Rechten unterstützen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-33`
- L4 operation: `KEEP` (`COP-044`)
- Ledger claims: `canon_role_based_accounts_and_access_control`

### CAN-REQ-045 - Nur Admin darf Accounts anlegen und Rechte verwalten

Nur ein Account mit Admin-Rechten darf weitere Accounts anlegen und Rechte verwalten.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-34`
- L4 operation: `KEEP` (`COP-045`)
- Ledger claims: `canon_role_based_accounts_and_access_control`

### CAN-REQ-046 - User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen oder Daten löschen

User-Accounts dürfen Inhalte hinzufügen, aber keine Accounts erstellen und keine hinzugefügten Daten aus der App löschen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-35`
- L4 operation: `KEEP` (`COP-046`)
- Ledger claims: `canon_role_based_accounts_and_access_control`

### CAN-REQ-048 - Zugriffe einrichtungsbezogen beschränken

Zugriffe müssen einrichtungsbezogen beschränkt sein, sodass Mitarbeiter nur die Profile ihres eigenen Hauses sehen können; die technische Umsetzung dieses Berechtigungsmodells ist noch auszuarbeiten.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-37`
- L4 operation: `KEEP` (`COP-048`)
- Ledger claims: `canon_cross_facility_access_restricted`

### CAN-REQ-049 - Settings-Screen rollenabhängig differenzieren

Für den Settings-Screen ist festzuhalten, dass die Ansicht rollenabhängig differenziert sein soll: Ein Admin kann dort beispielsweise Nutzeraccounts hinzufügen und Rechte anpassen, während jeder Nutzer sein Profil ändern und Einstellungen wie die Sprache wählen können soll.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-38`
- L4 operation: `KEEP` (`COP-049`)
- Ledger claims: `ADJ-GAP-AU-0130`

## Funktionale Anforderungen

### CAN-REQ-013 - Lösung soll primär das Verstehen von Bewohnern unterstützen

Die Lösung soll primär dabei unterstützen, Bewohner besser zu verstehen, insbesondere damit Betreuer bei Verständnisproblemen nachschauen können, was mit einer Kommunikationsweise gemeint sein könnte.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-01`
- L4 operation: `KEEP` (`COP-013`)
- Ledger claims: `canon_app_as_knowledge_lookup`, `canon_support_understanding_residents`

### CAN-REQ-030 - Kommunikationsinformationen in verbale und nonverbale Bereiche unterteilen

Kommunikationsinformationen sollen strukturell in verbale und nonverbale Bereiche unterteilt werden.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-18`
- L4 operation: `KEEP` (`COP-030`)
- Ledger claims: `canon_communication_split_verbal_nonverbal`

### CAN-REQ-033 - Neue Videos leicht hinzufügbar machen und neuestes oben anzeigen

Auf der Videofunktion sollen neue Videos mit Beschreibungen leicht hinzufügbar sein, und das neueste Video soll jeweils oben angezeigt werden.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-21`
- L4 operation: `KEEP` (`COP-033`)
- Ledger claims: `ADJ-GAP-AU-0053`

### CAN-REQ-056 - Bestehende Akten und dokumentierte Erfahrungen als Wissensquelle berücksichtigen

Für Bewohner existieren klassische Akten sowie planmäßig dokumentierte Erfahrungen und neues Wissen; diese dokumentierten Informationen sind als relevante Wissensquelle für die Lösung zu berücksichtigen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-46`
- L4 operation: `KEEP` (`COP-056`)
- Ledger claims: `canon_resident_records_exist`

## UI/UX Anforderungen

### CAN-REQ-018 - Profilübersichtsseite nach Login anzeigen

Nach dem Login soll eine Profilübersichtsseite angezeigt werden, die eine Liste aller für den Nutzer zugänglichen Profile enthält; jedes Profil soll anklickbar sein und zu einer weiteren Seite führen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-06`
- L4 operation: `KEEP` (`COP-018`)
- Ledger claims: `canon_profile_overview_after_login`

### CAN-REQ-024 - About-Me-Seite pro Person bereitstellen

Die App soll pro Person eine About-Me-Seite bereitstellen, die für den ersten Eindruck informativ und persönlich ist und Basisinformationen wie Bilder, Beschreibungen, Hobbys, Name und Alter enthält.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-12`
- L4 operation: `KEEP` (`COP-024`)
- Ledger claims: `canon_about_me_page`

### CAN-REQ-025 - Profil- und Kommunikationsinhalte dynamisch erweiterbar machen

Profil- und Kommunikationsinhalte sollen dynamisch erweiterbar sein, damit neue Erfahrungen, Bilder und Kommunikationsinformationen hinzugefügt werden können.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-13`
- L4 operation: `KEEP` (`COP-025`)
- Ledger claims: `canon_dynamic_media_growth`

### CAN-REQ-027 - No-Go-Seite pro Bewohner bereitstellen

Die App soll pro Bewohner eine No-Go-Seite enthalten, auf der festgehalten wird, was in Gegenwart des Bewohners absolut gar nicht geht.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-15`
- L4 operation: `KEEP` (`COP-027`)
- Ledger claims: `canon_no_go_page`

### CAN-REQ-028 - No-Go-Seite dynamisch erweiterbar machen

Die No-Go-Seite soll über einen Plus-Button dynamisch erweiterbar sein, sodass neue No-Gos als Liste hinzugefügt werden können, über die sich Nutzer im Vorhinein informieren können.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-16`
- L4 operation: `KEEP` (`COP-028`)
- Ledger claims: `ADJ-GAP-AU-0055`

### CAN-REQ-032 - Videos mit Beschreibungen in Kommunikationsseiten integrieren

Die App soll Videos mit Beschreibungen für Kommunikationssituationen unterstützen; diese Videofunktion soll in die verbalen und nonverbalen Kommunikationsseiten integriert werden und nicht als eigener Screen bestehen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-20`
- L4 operation: `KEEP` (`COP-032`)
- Ledger claims: `canon_video_support_in_communication`

### CAN-REQ-036 - Konsistente Appbar nach Login auf jeder Seite bereitstellen

Nach dem Login soll auf jeder Seite eine konsistente Appbar vorhanden sein; sie soll auch ein Einstellungssymbol enthalten, über das die Einstellungsseite erreichbar ist.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-25`
- L4 operation: `KEEP` (`COP-036`)
- Ledger claims: `canon_consistent_appbar_after_login`

### CAN-REQ-038 - Nur Login ohne Selbstregistrierung vorsehen

Für den Login soll es nur eine Login-Möglichkeit geben; eine Selbstregistrierung soll nicht vorgesehen sein.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-27`
- L4 operation: `KEEP` (`COP-038`)
- Ledger claims: `canon_role_based_accounts_and_access_control`

### CAN-REQ-039 - Login-Screen mit Logo, E-Mail, Passwort und Login-Button gestalten

Der Login-Screen soll aus Logo, E-Mail-Feld, Passwort-Feld und Login-Button bestehen; eine Registrierung soll auf diesem Screen nicht vorhanden sein.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-28`
- L4 operation: `KEEP` (`COP-039`)
- Ledger claims: `ADJ-GAP-AU-0118`, `ADJ-GAP-AU-0119`

### CAN-REQ-040 - Logo auf Login-Screen sichtbar im oberen Drittel platzieren

Für den Login-Screen ist als gewünschte Anforderung vorgesehen, dass das Logo im oberen Drittel des Bildschirms zentriert und gut sichtbar platziert wird; das Logo selbst ist noch zu erstellen und soll etwas mit Kommunikation zu tun haben.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-29`
- L4 operation: `KEEP` (`COP-040`)
- Ledger claims: `ADJ-GAP-AU-0117`

### CAN-REQ-050 - Kommunikationsinhalte auch visuell darstellen

Kommunikationsinhalte sollen nicht nur textbasiert dargestellt werden, sondern auch visuelle Darstellungen wie Bilder unterstützen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-39`
- L4 operation: `KEEP` (`COP-050`)
- Ledger claims: `canon_non_textual_representation`

## Nicht-funktionale Anforderungen

### CAN-REQ-043 - Alternative Eingabemethoden für beeinträchtigte Nutzer berücksichtigen

Sprachbefehle oder alternative Eingabemethoden für beeinträchtigte Nutzer sollen berücksichtigt werden.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-32`
- L4 operation: `KEEP` (`COP-043`)
- Ledger claims: `canon_alternative_input_consideration`

### CAN-REQ-053 - Animationen nicht priorisieren und nicht ablenkend gestalten

Animationen sollen nicht aktiv eingeplant werden; falls es dennoch welche gibt, dürfen sie nicht ablenkend sein.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-43`
- L4 operation: `KEEP` (`COP-053`)
- Ledger claims: `canon_animations_not_priority`

## Plattform und technische Rahmenbedingungen

### CAN-REQ-052 - App plattformübergreifend auf iOS und Android betreiben

Die App soll plattformübergreifend auf iPhone/iOS und Android laufen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-41`
- L4 operation: `KEEP` (`COP-052`)
- Ledger claims: `canon_cross_platform_ios_android`

### CAN-REQ-060 - Flutter mit Dart als Technologie-Stack verwenden

Für die Entwicklung wurde Flutter mit Dart als Technologie-Stack festgelegt, um die plattformübergreifende App umzusetzen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-53`
- L4 operation: `KEEP` (`COP-060`)
- Ledger claims: `canon_flutter_dart_stack`

### CAN-REQ-062 - Versionen der Entwicklungswerkzeuge im Team angleichen

Alle Teammitglieder sollen dieselben Versionen der Entwicklungswerkzeuge installieren, um spätere Integrationsprobleme zu vermeiden.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-55`
- L4 operation: `KEEP` (`COP-062`)
- Ledger claims: `canon_align_versions`

### CAN-REQ-063 - Initiale Entwicklung und Tests zunächst gegen Android 11 ausrichten

Für die initiale Entwicklung und das Testen soll zunächst gegen Android 11 gearbeitet werden.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-56`
- L4 operation: `KEEP` (`COP-063`)
- Ledger claims: `canon_develop_for_android11_initially`

## Prozess- und Vorgehensanforderungen

### CAN-REQ-058 - Anforderungsanalyse mit Beteiligten und späteren Nutzern durchführen

Vor der konkreten Ausgestaltung der Lösung soll als gewünschte Vorgehensweise eine Anforderungsanalyse mit Beteiligten und späteren Nutzern durchgeführt werden; dazu können auch mehrere Einrichtungen besucht werden, um ein breiteres Verständnis zu gewinnen.

- Status: `active`
- Readiness: `ready_for_issue_planning`
- Source items: `REQ-50`
- L4 operation: `KEEP` (`COP-058`)
- Ledger claims: `canon_requirements_analysis_with_users`

## Anforderungen mit Breakdown-Bedarf

### CAN-REQ-002 - Vollständiges Rollen- und Berechtigungskonzept spezifizieren

Für Mitarbeiter-, Angehörigen-, Bewohner- und Admin-Accounts ist ein vollständiges Berechtigungskonzept zu spezifizieren, das je Rolle pro Funktionsbereich festlegt, wer Profile sehen, Inhalte anlegen, bearbeiten, freigeben, exportieren, verbergen oder löschen darf; dabei ist auch der einrichtungsbezogene Zugriff über Häusergrenzen hinweg eindeutig zu regeln.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `L3-REQ-002`
- L4 operation: `KEEP` (`COP-002`)
- L3 candidates: `CAND-002`
- Human decisions: `HDEC-002`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.
  - `completion_marked_needs_breakdown` (hold): L4 Completion HumanReview hat dieses Requirement zur weiteren Aufteilung/Praezisierung markiert.

### CAN-REQ-003 - Eingaberegeln und Beschreibungsschema für Inhalte definieren

Für die Erfassung von Kommunikationsweisen, No-Go-Inhalten, About-Me-Daten, Bildern und Kalendereinträgen sind Pflichtfelder, zulässige Formate, maximale Feldlängen sowie Verhalten bei unvollständigen oder leeren Eingaben zu definieren; insbesondere muss für Kommunikations-Einträge ein einheitliches Beschreibungsschema vorgegeben werden, damit Suche und Filterung zuverlässig funktionieren.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `L3-REQ-003`
- L4 operation: `KEEP` (`COP-003`)
- L3 candidates: `CAND-003`
- Human decisions: `HDEC-003`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.
  - `completion_marked_needs_breakdown` (hold): L4 Completion HumanReview hat dieses Requirement zur weiteren Aufteilung/Praezisierung markiert.

### CAN-REQ-015 - Lösung soll neue Mitarbeiter und Verständigungssituationen unterstützen

Die Lösung soll insbesondere neuen Mitarbeitern helfen, Bewohner schneller zu verstehen und Einarbeitungswissen verfügbar zu machen; sie soll auch in Situationen unterstützen, in denen Beeinträchtigte erstmals allein mit jemandem sind und sich verständigen müssen.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-03`
- L4 operation: `KEEP` (`COP-015`)
- Ledger claims: `canon_new_staff_need_support`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.

### CAN-REQ-019 - Suchfunktion auf der Profilübersichtsseite bereitstellen

Auf der Profilübersichtsseite soll eine Suchfunktion vorhanden sein, damit Profile schnell gefunden werden können, statt lange scrollen zu müssen.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-07`
- L4 operation: `KEEP` (`COP-019`)
- Ledger claims: `canon_search_profiles`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.

### CAN-REQ-035 - Suchfunktion auf Kommunikationsseiten mit Beschreibungsmuster ermöglichen

Auf den Kommunikationsseiten soll eine Suchfunktion vorhanden sein; dafür muss jedoch noch ein systematisches Beschreibungsmuster für die Eingabe von Kommunikationsweisen definiert werden, damit Suche und Filterung möglich sind.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-23`
- L4 operation: `KEEP` (`COP-035`)
- Ledger claims: `canon_search_communication_entries_with_pattern`
- Readiness notes:
  - `completion_marked_needs_breakdown` (hold): L4 Completion HumanReview hat dieses Requirement zur weiteren Aufteilung/Praezisierung markiert.

### CAN-REQ-051 - Barrierefreiheit im Design berücksichtigen

Beim Design soll auf Barrierefreiheit geachtet werden, insbesondere durch große Schrift, ausreichenden Kontrast und die Vermeidung von zu vielen Farben.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-40`
- L4 operation: `KEEP` (`COP-051`)
- Ledger claims: `canon_accessible_design`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.
  - `completion_marked_needs_breakdown` (hold): L4 Completion HumanReview hat dieses Requirement zur weiteren Aufteilung/Praezisierung markiert.

### CAN-REQ-057 - Schlechte Nutzbarkeit vorhandener Dokumentation bei Lösungsdesign berücksichtigen

Die vorhandene Dokumentation ist schwer lesbar und schwer durchsuchbar; Informationen werden oft nicht schnell gefunden, und Wissen zur Kommunikation wird neuen Mitarbeitern daher häufig mündlich erklärt. Dieses Risiko ist bei der Ausgestaltung der Lösung zu berücksichtigen.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-47`
- L4 operation: `KEEP` (`COP-057`)
- Ledger claims: `canon_existing_docs_hard_to_use`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.

### CAN-REQ-061 - Android Studio als bevorzugte Entwicklungsumgebung nutzen

Das Team präferiert, möglichst dieselbe Entwicklungsumgebung zu nutzen, und bevorzugt dafür Android Studio.

- Status: `active`
- Readiness: `needs_breakdown`
- Source items: `REQ-54`
- L4 operation: `KEEP` (`COP-061`)
- Ledger claims: `canon_android_studio_preferred`
- Readiness notes:
  - `operationalization_risk` (hold): Requirement enthält qualitative Begriffe; für Issue-Planung wahrscheinlich weiter aufzuteilen oder mit Akzeptanzkriterien zu präzisieren.

## Deferred / Optional

### CAN-REQ-041 - Optionale Hilfe- oder Tutorial-Funktion vorsehen

Eine Hilfe- bzw. Tutorial-Funktion ist als spätere, optionale Unterstützung gewünscht, etwa als kurze Tour beim ersten Login sowie als erneut aufrufbarer Hilfebereich.

- Status: `active`
- Readiness: `deferred_or_optional`
- Source items: `REQ-30`
- L4 operation: `KEEP` (`COP-041`)
- Ledger claims: `canon_help_tutorial_features`
- Readiness notes:
  - `deferred_or_optional_marker` (hold): Requirement ist als optional, spaeter oder perspektivisch markiert und sollte nicht automatisch in Sprint-Issues laufen.
  - `decision_marker_in_active_requirement` (hold): Requirement enthält Entscheidungsmarker; prüfen, ob es als offene Entscheidung statt aktive Anforderung geführt werden sollte.

### CAN-REQ-042 - Optionale Popup-Benachrichtigung bei neuen About-Me-Inhalten vorsehen

Wenn im About-Me-Bereich neue Inhalte hochgeladen werden, sollen als spätere optionale Funktion alle verbundenen Nutzer derselben Einrichtung per Popup benachrichtigt werden.

- Status: `active`
- Readiness: `deferred_or_optional`
- Source items: `REQ-31`
- L4 operation: `KEEP` (`COP-042`)
- Ledger claims: `canon_about_me_notifications`
- Readiness notes:
  - `deferred_or_optional_marker` (hold): Requirement ist als optional, spaeter oder perspektivisch markiert und sollte nicht automatisch in Sprint-Issues laufen.
  - `decision_marker_in_active_requirement` (hold): Requirement enthält Entscheidungsmarker; prüfen, ob es als offene Entscheidung statt aktive Anforderung geführt werden sollte.

## Offene Entscheidungen

### CAN-DISK-003 - MVP-Abnahmeschwellen für Verfügbarkeit und Wiederanlauf als Diskussionspunkt ergänzen

Zu den messbaren Qualitätszielen existiert mit CAN-REQ-009 bereits ein offener Rahmen für Performance, Last, Profilanzahl und Mediengrößen. Nicht gesagt ist jedoch, welche minimalen Erwartungen für Verfügbarkeit, Störungsdauer oder Wiederanlauf im MVP gelten sollen.

Vorgeschlagene Klaerung:
Als RE-Diskussionspunkt ergänzen, ob für den MVP minimale Zielwerte oder qualitative Leitplanken zu Verfügbarkeit, maximal tolerierbarer Ausfallzeit und Wiederanlauf nach Störungen festgelegt werden sollen.

Warum wichtig:
Auch bei kleinem MVP beeinflussen solche Betriebsziele Architektur, Monitoring, Supporterwartung und Abnahme. Die Ergänzung macht eine plausible, bislang nicht ausgesprochene Qualitätslücke sichtbar, ohne sie als beschlossene Wahrheit zu setzen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-009`
- L4 operation: `ADD_DISK_POINT` (`L4-CP-004`)
- L3 candidates: `CAND-009`
- Human decisions: `HDEC-009`, `l4-completion:L4-CP-004`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-OPEN-001 - Scope-Entscheidung mit expliziten Nicht-Zielen vor Projektstart abschließen

Die zentrale Abgrenzung, ob der MVP nur ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile formaler Dokumentation übernimmt, ist zwar als offene Entscheidung vorhanden, aber noch nicht in eine startklare, explizite Entscheidungsgrundlage mit Nicht-Zielen überführt. Dadurch bleibt unklar, welche nachgelagerten Anforderungen verbindlich im MVP liegen und welche ausdrücklich ausgeschlossen sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-001/CAN-REQ-055 eine formale offene Entscheidungsfrage mit genau zwei bis drei Scope-Optionen, den jeweiligen Konsequenzen für Datenarten, Rollen, Datenschutz und UI sowie einer Liste expliziter Nicht-Ziele für den MVP.

Warum wichtig:
Diese Entscheidung ist ein Startblocker für Funktionsumfang, Datenmodell, Datenschutzbedarf und Aufwandsschätzung. Ohne sie bleibt ein erheblicher Teil der übrigen Requirements interpretationsabhängig.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-001`, `REQ-45`
- L4 operation: `ADD_OPEN_DECISION` (`L4-CP-001`)
- Ledger claims: `canon_full_documentation_scope_open`
- L3 candidates: `CAND-001`
- Human decisions: `HDEC-001`, `l4-completion:L4-CP-001`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-OPEN-002 - Rechtsgrundlage und zulässige Datennutzung für echte Bewohnerdaten und Medien entscheiden

Die bestehende Datenschutzanforderung CAN-REQ-005 deckt den Datenlebenszyklus ab, benennt aber die startkritische Entscheidung zur Rechtsgrundlage und zum zulässigen Einsatz echter Bewohnerdaten, Bilder und Videos im Test- und Produktivkontext noch nicht als klar abgegrenzte Entscheidungsfrage.

Vorgeschlagene Klaerung:
Ergänze eine offene Entscheidung, welche Datenkategorien mit welcher Rechtsgrundlage verarbeitet werden dürfen, ob und unter welchen Bedingungen echte Bewohnerdaten/Bilder in Tests zulässig sind und welche Einwilligungs- oder Freigabeschritte dafür vorliegen müssen.

Warum wichtig:
Diese Entscheidung steuert, ob fachliche Tests mit Echtdaten zulässig sind und welche Prozesse, Datenmodelle und Betriebsregeln überhaupt umgesetzt werden dürfen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-005`
- L4 operation: `ADD_OPEN_DECISION` (`L4-CP-003`)
- L3 candidates: `CAND-005`
- Human decisions: `HDEC-005`, `l4-completion:L4-CP-003`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-OPEN-004 - GetX-Status als offene Technikentscheidung korrekt ausweisen

CAN-REQ-059 ist als aktive Anforderung geführt, enthält laut Readiness aber einen Entscheidungsmarker. Dadurch ist unklar, ob GetX bereits als gesetzte Rahmenbedingung gilt oder bewusst noch zur Auswahl steht.

Vorgeschlagene Klaerung:
Reklassifiziere den Inhalt von CAN-REQ-059 in eine explizite offene Technikentscheidung mit klaren Entscheidungsoptionen, Bewertungskriterien und einem Termin, bis zu dem die Wahl für den Projektstart getroffen werden muss.

Warum wichtig:
State-Management beeinflusst Architektur, Zustandsfluss, Testbarkeit und Implementierungsstruktur direkt. Ein uneindeutiger Status erzeugt vermeidbares Richtungsrisiko.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-52`
- L4 operation: `ADD_OPEN_DECISION` (`L4-CP-005`)
- Ledger claims: `canon_getx_consideration`
- Human decisions: `l4-completion:L4-CP-005`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-OPEN-005 - Minimalen Content-Lifecycle für nutzergenerierte Inhalte verbindlich entscheiden

Mit CAN-REQ-004 ist der Bearbeitungs- und Freigabeprozess zwar als offene Entscheidung erfasst, für den Projektstart fehlen aber eine minimal verbindliche Zielausprägung und die Abgrenzung, welche Zustände und Verantwortlichkeiten im MVP tatsächlich verpflichtend sind.

Vorgeschlagene Klaerung:
Ergänze zu CAN-REQ-004 eine offene Entscheidungsgrundlage für den MVP-Content-Lifecycle mit minimalen Statuswerten, verantwortlichen Rollen je Statusübergang, Regeln bei Zurückweisung/Korrektur und dem Umgang mit parallelen Änderungen.

Warum wichtig:
Ohne klaren Minimalprozess bleiben Verantwortlichkeiten, Moderation, Inhaltsqualität und Konfliktbehandlung im Kernprozess der Inhalteingabe unsicher.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-004`
- L4 operation: `ADD_OPEN_DECISION` (`L4-CP-006`)
- L3 candidates: `CAND-004`
- Human decisions: `HDEC-004`, `l4-completion:L4-CP-006`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-001 - MVP-Scope zwischen Unterstützungswerkzeug und Dokumentationsumfang festlegen

Vor Projektstart ist verbindlich festzulegen, ob der MVP ausschließlich ein Unterstützungswerkzeug zum schnellen Verstehen von Bewohnern bleibt oder zusätzlich Teile der formalen Pflegedokumentation übernehmen soll; für beide Varianten ist eine klare Scope-Abgrenzung einschließlich expliziter Nicht-Ziele zu dokumentieren.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-001`
- L4 operation: `KEEP` (`COP-001`)
- L3 candidates: `CAND-001`
- Human decisions: `HDEC-001`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-004 - Bearbeitungs- und Freigabeprozess für nutzergenerierte Inhalte festlegen

Für nutzergenerierte Inhalte ist ein Bearbeitungs- und Freigabeprozess festzulegen: Neue oder geänderte Beiträge von Mitarbeitern, Angehörigen oder Bewohnern müssen definierte Zustände wie Entwurf, eingereicht, freigegeben, zurückgewiesen oder archiviert durchlaufen, und bei gleichzeitigen Änderungen ist ein Konfliktverhalten einschließlich Versionserhalt festzulegen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-004`
- L4 operation: `KEEP` (`COP-004`)
- L3 candidates: `CAND-004`
- Human decisions: `HDEC-004`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-005 - Datenschutz- und Datenlebenszyklus vor Produktivstart festlegen

Vor Produktivstart ist ein Datenschutz- und Datenlebenszyklus festzulegen, der für Fotos, Videos, Bewohnerprofile, Kommunikationshinweise und No-Go-Inhalte Aufbewahrungsdauer, Korrektur, Archivierung, Löschung, Entzug von Einwilligungen sowie einen nachvollziehbaren Export pro Bewohner oder Einrichtung regelt.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-005`
- L4 operation: `KEEP` (`COP-005`)
- L3 candidates: `CAND-005`
- Human decisions: `HDEC-005`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-006 - Sicherheitsmaßnahmen für besonders sensible Inhalte festlegen

Es ist zu entscheiden, welche Sicherheitsmaßnahmen für besonders sensible Inhalte verbindlich vorgeschrieben sind; mindestens sind Regeln für abgesicherte Authentifizierung, serverseitig erzwungene Rollen- und Einrichtungstrennung sowie Protokollierung sicherheitsrelevanter Zugriffe und Änderungen für No-Go-Seiten, Bewohnerbilder und hausübergreifende Zugriffe festzulegen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-006`
- L4 operation: `KEEP` (`COP-006`)
- L3 candidates: `CAND-006`
- Human decisions: `HDEC-006`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-007 - Übernahme aus Akten und Drittsystemen entscheiden

Es ist zu entscheiden, ob und wie Bewohnergrunddaten, Termine oder weitere Inhalte aus bestehenden Akten bzw. Drittsystemen übernommen werden; dafür sind Quelle, Datenumfang, Übertragungsweg, Verantwortlichkeit und der Umgang mit Medienbrüchen oder manueller Doppelpflege festzulegen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-007`
- L4 operation: `KEEP` (`COP-007`)
- L3 candidates: `CAND-007`
- Human decisions: `HDEC-007`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-008 - Offline- und Synchronisationsverhalten für Firestore festlegen

Vor Projektstart ist zu entscheiden, welches Betriebsverhalten bei fehlender oder instabiler Internetverbindung verbindlich unterstützt werden muss; dabei sind lokale Zwischenspeicherung, Synchronisationszeitpunkte, Konfliktauflösung nach Wiederverbindung sowie Datensicherung und Wiederherstellung für den Firestore-basierten Betrieb festzulegen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-008`
- L4 operation: `KEEP` (`COP-008`)
- L3 candidates: `CAND-008`
- Human decisions: `HDEC-008`
- Replaces: `REQ-51`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-009 - Messbare Qualitätsziele vor Projektstart festlegen

Vor Projektstart sind messbare Qualitätsziele festzulegen, mindestens für maximale Such- und Ladezeiten bei Profilen, erwartete Anzahl gleichzeitiger Nutzer pro Einrichtung, Anzahl verwalteter Bewohnerprofile sowie akzeptable Mediengrößen für Bilder und Videos.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-009`
- L4 operation: `KEEP` (`COP-009`)
- L3 candidates: `CAND-009`
- Human decisions: `HDEC-009`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.
  - `completion_marked_needs_breakdown` (hold): L4 Completion HumanReview hat dieses Requirement zur weiteren Aufteilung/Praezisierung markiert.

### CAN-REQ-010 - Nutzungskontext, Tablet-Support und Barrierefreiheitskriterien festlegen

Der Nutzungskontext ist verbindlich festzulegen, insbesondere ob der MVP neben Smartphones auch Tablets unterstützen muss, in welchen Arbeitssituationen die App bedienbar sein soll und welche konkreten Bedienbarkeits- und Barrierefreiheitskriterien dafür als Abnahmekriterien gelten.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-010`
- L4 operation: `KEEP` (`COP-010`)
- L3 candidates: `CAND-010`
- Human decisions: `HDEC-010`
- Replaces: `REQ-42`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-011 - Rechtsgrundlage und Freigaben für echte Bewohnerdaten und Bilder festlegen

Vor Verwendung echter Bewohnerdaten und Bilder ist verbindlich festzulegen, auf welcher Rechtsgrundlage Analyse, Test, inhaltliche Erstbefüllung und Produktivbetrieb erfolgen, wie Einwilligungen dokumentiert werden und welche organisatorischen Rollen die datenschutzrechtliche Freigabe verantworten.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-011`
- L4 operation: `KEEP` (`COP-011`)
- L3 candidates: `CAND-011`
- Human decisions: `HDEC-011`
- Replaces: `REQ-48`, `REQ-49`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-012 - Kalender-Scope und Umgang mit medizinisch sensiblen Informationen entscheiden

Es ist fachlich zu entscheiden, ob der Kalender im MVP ausschließlich allgemeine Termine abbildet oder auch medizinisch sensible Informationen wie Medikamentengaben enthalten darf; falls letzteres gewünscht ist, sind eigener Schutzbedarf, Zugriffsregeln und Scope-Folgen gesondert freizugeben.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `L3-REQ-012`
- L4 operation: `KEEP` (`COP-012`)
- L3 candidates: `CAND-012`
- Human decisions: `HDEC-012`
- Replaces: `REQ-24`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-014 - Digitale Lösung zur Kommunikationsförderung als gewünschtes Zielbild

Es wird als gewünschtes, noch offenes Zielbild eine digitale Lösung, etwa als Computerprogramm oder ähnliche Anwendung, bevorzugt, um die Kommunikation zwischen betreuten Menschen mit Beeinträchtigungen und anderen Personen zu verbessern und zu fördern.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-02`
- L4 operation: `KEEP` (`COP-014`)
- Ledger claims: `canon_app_goal_digital_comm_support`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-020 - Sichtbare Suchleiste unter der Appbar auf Profilübersicht vorsehen

Als gewünschte, noch offene Ausgestaltung soll unter der Appbar auf der Profilübersicht eine gut sichtbare Suchleiste vorgesehen werden.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-08`
- L4 operation: `KEEP` (`COP-020`)
- Ledger claims: `ADJ-GAP-AU-0121`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-021 - Profile als Kacheln oder Liste mit Vorschaudaten darstellen

Als gewünschte, noch offene Ausgestaltung sollen die Profile in der Profilübersicht als Kacheln oder Liste unterhalb der Suchleiste dargestellt werden, jeweils mit kleinem Vorschaubild, Name und kurzer Beschreibung des Bewohners für einen schnellen Überblick.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-09`
- L4 operation: `KEEP` (`COP-021`)
- Ledger claims: `ADJ-GAP-AU-0122`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-022 - Anlegen neuer Profile über Profilübersicht klären

Das Anlegen neuer Profile über ein Plus-Symbol auf der Profilübersichtsseite mit anschließendem Dialog zur Erfassung von Bild, Name und Beschreibung soll geklärt werden.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-10`
- L4 operation: `KEEP` (`COP-022`)
- Ledger claims: `ADJ-GAP-AU-0123`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-023 - Profil-Detailansicht mit vier Hauptbereichen vorsehen

Beim Öffnen eines Profils soll als gewünschte, noch offene Ausgestaltung eine Detailansicht erscheinen, die das gewählte Profilbild größer zeigt und die vier Hauptbereiche der App als interaktive Buttons darstellt.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-11`
- L4 operation: `KEEP` (`COP-023`)
- Ledger claims: `ADJ-GAP-AU-0124`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-026 - About-Me-Seite mit Infobox und Foto-Timeline ausgestalten

Als gewünschte, noch offene Ausgestaltung soll die About-Me-Seite oben eine Infobox mit Angaben wie Alter und Hobbys enthalten; darunter soll eine Foto-Timeline liegen, in die über einen Plus-Button am unteren Bildschirmrand neue Einträge aufgenommen werden können; die neuesten Fotos sollen oben angezeigt werden.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-14`
- L4 operation: `KEEP` (`COP-026`)
- Ledger claims: `ADJ-GAP-AU-0125`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-029 - No-Go-Seite mit starkem visuellen Symbol ausstatten

Als gewünschte, noch offene Ausgestaltung soll die No-Go-Seite ein starkes visuelles Symbol, etwa ein rotes Stoppschild, oben auf dem Screen zeigen; die Einträge sollen leicht zu durchforsten sein und nur die wichtigsten Informationen enthalten.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-17`
- L4 operation: `KEEP` (`COP-029`)
- Ledger claims: `ADJ-GAP-AU-0128`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-031 - Trennung der Kommunikationsbereiche visuell kennzeichnen

Als gewünschte, noch offene Ausgestaltung soll diese Trennung auf der Kommunikationsseite visuell mit Symbolen gekennzeichnet werden; jeder Bereich soll einen klaren Button zu weiterführenden Informationen haben.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-19`
- L4 operation: `KEEP` (`COP-031`)
- Ledger claims: `ADJ-GAP-AU-0126`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-034 - Plus-Button für nonverbale Videos und Beschreibungen vorsehen

Als gewünschte, noch offene Ausgestaltung soll im Bereich nonverbaler Signale ein Plus-Button vorgesehen werden, um Videos und Beschreibungen hinzuzufügen.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-22`
- L4 operation: `KEEP` (`COP-034`)
- Ledger claims: `ADJ-GAP-AU-0127`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-037 - Aktuellen Screen-Titel mittig in der Appbar anzeigen

Als gewünschte, noch offene Ausgestaltung soll in der Appbar mittig der Titel des aktuellen Screens angezeigt werden, auf der Profilübersicht also „Profilübersicht“.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-26`
- L4 operation: `KEEP` (`COP-037`)
- Ledger claims: `ADJ-GAP-AU-0121`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-055 - Offene Entscheidung zur vollständigen Dokumentationsübernahme erhalten

Ob die App eine vollständige Dokumentation übernehmen soll, ist offen und mit dem Leiter der Einrichtung zu klären; der Umfang soll nicht so groß werden, dass der eigentliche Sinn der unterstützenden Kommunikation verloren geht.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-45`
- L4 operation: `KEEP` (`COP-055`)
- Ledger claims: `canon_full_documentation_scope_open`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

### CAN-REQ-059 - Einsatz von GetX als technische Rahmenbedingung prüfen

Der Einsatz von GetX soll als technische Rahmenbedingung geprüft werden, ist aber noch nicht entschieden.

- Status: `active`
- Readiness: `needs_decision`
- Source items: `REQ-52`
- L4 operation: `KEEP` (`COP-059`)
- Ledger claims: `canon_getx_consideration`
- Readiness notes:
  - `decision_marker_in_active_requirement` (blocker): Requirement enthält Entscheidungsmarker; prüfen, ob es als offene Entscheidung statt aktive Anforderung geführt werden sollte.

### CAN-REQ-064 - Digitalisierung grundsätzlich als vorteilhaft ansehen

Eine stärkere Digitalisierung der bislang analogen Arbeitsweise wird grundsätzlich als vorteilhaft angesehen, ohne dass daraus automatisch MVP-Umfang folgt.

- Status: `open_decision`
- Readiness: `needs_decision`
- Source items: `REQ-57`
- L4 operation: `KEEP` (`COP-064`)
- Ledger claims: `canon_digitization_desired_broadly`
- Readiness notes:
  - `open_decision_status` (blocker): Requirement ist eine offene Entscheidung und darf nicht direkt als Umsetzungs-Issue geplant werden.

## Traceability Summary

| Requirement | Project Items | Ledger Claims | L3 Candidates | Human Decisions |
|---|---|---|---|---|
| `CAN-DISK-003` | `L3-REQ-009` |  | `CAND-009` | `HDEC-009`, `l4-completion:L4-CP-004` |
| `CAN-OPEN-001` | `L3-REQ-001`, `REQ-45` | `canon_full_documentation_scope_open` | `CAND-001` | `HDEC-001`, `l4-completion:L4-CP-001` |
| `CAN-OPEN-002` | `L3-REQ-005` |  | `CAND-005` | `HDEC-005`, `l4-completion:L4-CP-003` |
| `CAN-OPEN-004` | `REQ-52` | `canon_getx_consideration` |  | `l4-completion:L4-CP-005` |
| `CAN-OPEN-005` | `L3-REQ-004` |  | `CAND-004` | `HDEC-004`, `l4-completion:L4-CP-006` |
| `CAN-REQ-001` | `L3-REQ-001` |  | `CAND-001` | `HDEC-001` |
| `CAN-REQ-002` | `L3-REQ-002` |  | `CAND-002` | `HDEC-002` |
| `CAN-REQ-003` | `L3-REQ-003` |  | `CAND-003` | `HDEC-003` |
| `CAN-REQ-004` | `L3-REQ-004` |  | `CAND-004` | `HDEC-004` |
| `CAN-REQ-005` | `L3-REQ-005` |  | `CAND-005` | `HDEC-005` |
| `CAN-REQ-006` | `L3-REQ-006` |  | `CAND-006` | `HDEC-006` |
| `CAN-REQ-007` | `L3-REQ-007` |  | `CAND-007` | `HDEC-007` |
| `CAN-REQ-008` | `L3-REQ-008` |  | `CAND-008` | `HDEC-008` |
| `CAN-REQ-009` | `L3-REQ-009` |  | `CAND-009` | `HDEC-009` |
| `CAN-REQ-010` | `L3-REQ-010` |  | `CAND-010` | `HDEC-010` |
| `CAN-REQ-011` | `L3-REQ-011` |  | `CAND-011` | `HDEC-011` |
| `CAN-REQ-012` | `L3-REQ-012` |  | `CAND-012` | `HDEC-012` |
| `CAN-REQ-013` | `REQ-01` | `canon_app_as_knowledge_lookup`, `canon_support_understanding_residents` |  |  |
| `CAN-REQ-014` | `REQ-02` | `canon_app_goal_digital_comm_support` |  |  |
| `CAN-REQ-015` | `REQ-03` | `canon_new_staff_need_support` |  |  |
| `CAN-REQ-016` | `REQ-04` | `canon_employee_and_relative_access` |  |  |
| `CAN-REQ-017` | `REQ-05` | `canon_resident_account_with_restrictions` |  |  |
| `CAN-REQ-018` | `REQ-06` | `canon_profile_overview_after_login` |  |  |
| `CAN-REQ-019` | `REQ-07` | `canon_search_profiles` |  |  |
| `CAN-REQ-020` | `REQ-08` | `ADJ-GAP-AU-0121` |  |  |
| `CAN-REQ-021` | `REQ-09` | `ADJ-GAP-AU-0122` |  |  |
| `CAN-REQ-022` | `REQ-10` | `ADJ-GAP-AU-0123` |  |  |
| `CAN-REQ-023` | `REQ-11` | `ADJ-GAP-AU-0124` |  |  |
| `CAN-REQ-024` | `REQ-12` | `canon_about_me_page` |  |  |
| `CAN-REQ-025` | `REQ-13` | `canon_dynamic_media_growth` |  |  |
| `CAN-REQ-026` | `REQ-14` | `ADJ-GAP-AU-0125` |  |  |
| `CAN-REQ-027` | `REQ-15` | `canon_no_go_page` |  |  |
| `CAN-REQ-028` | `REQ-16` | `ADJ-GAP-AU-0055` |  |  |
| `CAN-REQ-029` | `REQ-17` | `ADJ-GAP-AU-0128` |  |  |
| `CAN-REQ-030` | `REQ-18` | `canon_communication_split_verbal_nonverbal` |  |  |
| `CAN-REQ-031` | `REQ-19` | `ADJ-GAP-AU-0126` |  |  |
| `CAN-REQ-032` | `REQ-20` | `canon_video_support_in_communication` |  |  |
| `CAN-REQ-033` | `REQ-21` | `ADJ-GAP-AU-0053` |  |  |
| `CAN-REQ-034` | `REQ-22` | `ADJ-GAP-AU-0127` |  |  |
| `CAN-REQ-035` | `REQ-23` | `canon_search_communication_entries_with_pattern` |  |  |
| `CAN-REQ-036` | `REQ-25` | `canon_consistent_appbar_after_login` |  |  |
| `CAN-REQ-037` | `REQ-26` | `ADJ-GAP-AU-0121` |  |  |
| `CAN-REQ-038` | `REQ-27` | `canon_role_based_accounts_and_access_control` |  |  |
| `CAN-REQ-039` | `REQ-28` | `ADJ-GAP-AU-0118`, `ADJ-GAP-AU-0119` |  |  |
| `CAN-REQ-040` | `REQ-29` | `ADJ-GAP-AU-0117` |  |  |
| `CAN-REQ-041` | `REQ-30` | `canon_help_tutorial_features` |  |  |
| `CAN-REQ-042` | `REQ-31` | `canon_about_me_notifications` |  |  |
| `CAN-REQ-043` | `REQ-32` | `canon_alternative_input_consideration` |  |  |
| `CAN-REQ-044` | `REQ-33` | `canon_role_based_accounts_and_access_control` |  |  |
| `CAN-REQ-045` | `REQ-34` | `canon_role_based_accounts_and_access_control` |  |  |
| `CAN-REQ-046` | `REQ-35` | `canon_role_based_accounts_and_access_control` |  |  |
| `CAN-REQ-047` | `REQ-36` | `canon_internal_use_only` |  |  |
| `CAN-REQ-048` | `REQ-37` | `canon_cross_facility_access_restricted` |  |  |
| `CAN-REQ-049` | `REQ-38` | `ADJ-GAP-AU-0130` |  |  |
| `CAN-REQ-050` | `REQ-39` | `canon_non_textual_representation` |  |  |
| `CAN-REQ-051` | `REQ-40` | `canon_accessible_design` |  |  |
| `CAN-REQ-052` | `REQ-41` | `canon_cross_platform_ios_android` |  |  |
| `CAN-REQ-053` | `REQ-43` | `canon_animations_not_priority` |  |  |
| `CAN-REQ-054` | `REQ-44` | `canon_exclude_translation_system` |  |  |
| `CAN-REQ-055` | `REQ-45` | `canon_full_documentation_scope_open` |  |  |
| `CAN-REQ-056` | `REQ-46` | `canon_resident_records_exist` |  |  |
| `CAN-REQ-057` | `REQ-47` | `canon_existing_docs_hard_to_use` |  |  |
| `CAN-REQ-058` | `REQ-50` | `canon_requirements_analysis_with_users` |  |  |
| `CAN-REQ-059` | `REQ-52` | `canon_getx_consideration` |  |  |
| `CAN-REQ-060` | `REQ-53` | `canon_flutter_dart_stack` |  |  |
| `CAN-REQ-061` | `REQ-54` | `canon_android_studio_preferred` |  |  |
| `CAN-REQ-062` | `REQ-55` | `canon_align_versions` |  |  |
| `CAN-REQ-063` | `REQ-56` | `canon_develop_for_android11_initially` |  |  |
| `CAN-REQ-064` | `REQ-57` | `canon_digitization_desired_broadly` |  |  |

