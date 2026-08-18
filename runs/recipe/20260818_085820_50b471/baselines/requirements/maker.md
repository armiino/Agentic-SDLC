# Requirements

## Funktionale Anforderungen

- Freigegebene Angehörige müssen die Übersicht der Medikamenten-Einnahmen ihrer Bezugsperson sehen können. [med-overview-relatives-visible]

- Die Einnahmen-Übersicht für Angehörige muss als Monatsübersicht der letzten dreißig Tage bereitgestellt werden; die bisherige Wochenansicht mit sieben Tagen darf dafür nicht weiter gelten. [med-intake-overview-30-days]

- Pflegende müssen am Ende ihrer Schicht eine Tages-Zusammenfassung aller von ihnen dokumentierten Medikamenten-Gaben sehen können, damit sie vor der Übergabe prüfen können, ob alles erfasst ist. [shift-end-medication-summary]

- Angehörige müssen Besuche bei ihrer Bezugsperson vorab in der App mit Datum und Uhrzeit ankündigen können. [visit-planning-announcement]

- Pflegende der Einrichtung müssen eine Übersicht der angekündigten Besuche sehen können. [visit-planning-announcement]

- Angehörige müssen zu Einträgen eigene Fotos hochladen können; pro Eintrag sind maximal fünf Fotos zulässig, und es dürfen nur JPG- oder PNG-Formate akzeptiert werden. [weekly-entry-photo-limit-format]

## Zugangs- und Registrierungs-Constraints

- Eine Selbstregistrierung für Angehörige darf derzeit nicht angeboten werden; der Zugang für Angehörige muss ausschließlich über intern vergebene Accounts erfolgen. [no-self-registration-existing-decision]

- Es muss geklärt werden, ob Angehörige sich künftig selbst mit einem Einladungscode der Einrichtung registrieren können und ob diese mögliche Neuregelung die bisherige Festlegung ohne Selbstregistrierung ersetzt. [self-registration-invite-code-open]

## Offene Klärungen und Kontext

- Die Aufbewahrungsdauer einnahmebezogener Bemerkungen bei der Medikamenten-Gabe soll mit dem Träger geklärt werden, da dabei Datenschutz-Fragen zu berücksichtigen sind. [remark-retention-open]

## Technischer Kontext

- Für Push-Benachrichtigungen an Angehörige ist Firebase Cloud Messaging als technische Ausrichtung festgelegt. [push-fcm-technology]