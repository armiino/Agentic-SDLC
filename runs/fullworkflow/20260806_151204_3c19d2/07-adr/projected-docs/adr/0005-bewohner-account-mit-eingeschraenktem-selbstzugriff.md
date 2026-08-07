# ADR-0005: Bewohner-Account mit eingeschraenktem Selbstzugriff

Status: accepted
Core-Item: ARCH-10

## Kontext

Zusaetzlich zu den bisherigen Rollen wurde ein Bewohner-Account vorgesehen. Dieser soll nur das eigene Profil sehen duerfen und nur eingeschraenkte Funktionen nutzen koennen, insbesondere den Zugriff auf About Me und gegebenenfalls das Hinzufuegen eigener Bilder. Die Entscheidung wirkt direkt auf Rollenmodell, Sichtbarkeit und erlaubte Interaktionen.

## Entscheidung

Wir fuehren einen Bewohner-Account mit stark eingeschraenkten Rechten ein. Dieser darf nur das eigene Profil sehen und nur begrenzte Funktionen nutzen, insbesondere den About-Me-Bereich und gegebenenfalls das Hinzufuegen eigener Bilder.

## Konsequenzen

Die App kann einen begrenzten Selbstzugang fuer Bewohner unterstuetzen und das Rollenmodell gezielt auf diese Nutzergruppe erweitern. Gleichzeitig muessen Sichtbarkeit, Navigation und Berechtigungen streng auf das eigene Profil begrenzt werden. Dadurch steigt die Komplexitaet des Rechtemodells und der Absicherung von Profilzugriffen.

## Verwandte Core-Items

- REQ-07 — Zusätzlich soll es einen Bewohner-Account geben. Dieser darf nur das eigene Profil in der Profilübersicht sehen und nur eingeschränkte Funktionen nutzen, insbesondere Zugriff auf den About-Me-Bereich und gegebenenfalls das Hinzufügen eigener Bilder.
- REQ-10 — Die konkrete Ausgestaltung der Rechteverwaltung muss später entschieden werden.

