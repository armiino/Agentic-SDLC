# ADR-0010: Profiluebersicht als Einstieg nach dem Login

Status: accepted
Core-Item: ARCH-15

## Kontext

Nach erfolgreichem Login soll laut Festlegung eine Profiluebersicht angezeigt werden. Diese Uebersicht dient als Einstieg in die sichtbaren Bewohnerprofile und bindet die Ausgestaltung des Startpunkts nach der Authentifizierung.

## Entscheidung

Wir zeigen nach dem Login eine Profiluebersicht als Einstieg in die App an.

## Konsequenzen

Der Navigationsfluss nach der Anmeldung ist klar festgelegt und fuehrt Nutzer direkt zu den verfuegbaren Bewohnerprofilen. Gleichzeitig richtet sich die Gestaltung des App-Einstiegs an dieser Uebersicht aus und alternative direkte Einstiegsziele nach dem Login werden nicht priorisiert.

## Verwandte Core-Items

- REQ-13 — Nach dem Login soll eine Profilübersicht angezeigt werden.
- REQ-16 — Auf der Profilübersicht soll eine anklickbare Liste der sichtbaren Bewohnerprofile angezeigt werden; jedes Profil führt zu einer weiteren Seite mit den Daten des gewählten Profils.

