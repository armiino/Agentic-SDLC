# ADR-0006: Offene Detailausgestaltung der Rechteverwaltung

Status: accepted
Core-Item: ARCH-12

## Kontext

Zum aktuellen Architekturstand ist festgehalten, dass die konkrete Ausgestaltung der Rechteverwaltung noch nicht entschieden ist und spaeter festgelegt werden muss. Dokumentiert ist damit kein ausformulierter Detailzuschnitt, sondern der derzeit offene Stand dieses Architekturthemas.

## Entscheidung

Wir halten fest, dass die konkrete Ausgestaltung der Rechteverwaltung noch offen ist und zu einem spaeteren Zeitpunkt entschieden werden muss.

## Konsequenzen

Das bestehende Rollen- und Berechtigungsmodell ist nur in Grundzuegen belastbar, und Detailentscheidungen zur Rechtevergabe koennen noch nicht als stabil vorausgesetzt werden. Das schafft Spielraum fuer spaetere Konkretisierung, bindet aber nachfolgende Ausarbeitung und Implementierung an eine noch ausstehende Entscheidung.

## Verwandte Core-Items

- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.
- ARCH-08 — Es muss mindestens die Rollen Admin und User geben; Admins verwalten Accounts und Rechte, User können Inhalte hinzufügen, aber nichts löschen.
- ARCH-10 — Zusätzlich soll es einen Bewohner-Account geben, der nur das eigene Profil sehen darf und nur eingeschränkte Funktionen nutzen kann, insbesondere Zugriff auf About Me und gegebenenfalls das Hinzufügen eigener Bilder.

