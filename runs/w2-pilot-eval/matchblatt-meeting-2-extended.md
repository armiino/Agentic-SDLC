# W2-Match-Blatt meeting-2-extended — VERBLINDET (Evaluator-Adjudikation nach Matchregeln §2–§4)

> Je Beobachtung: jede Aussage gegen die Gold-Units urteilen (voll/teilweise/kein;
> Merge/Split-Regeln §3). Beobachtungs-Codes sind anonymisiert; Auflösung liegt separat.

## Gold-Units (Referenz)

- **G-M2-001** [requirement] Die bestehende About-Me-Seite soll unverändert beibehalten werden.
- **G-M2-002** [requirement] Die About-Me-Seite soll weiterhin einen persönlichen ersten Eindruck des jeweiligen Bewohners vermitteln.
- **G-M2-003** [requirement] Die bestehende No-Go-Seite soll weiterhin als Liste mit Plus-Button gestaltet sein.
- **G-M2-004** [requirement] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal enthalten.
- **G-M2-005** [requirement] No-Go-Einträge sollen nachträglich bearbeitet werden können.
- **G-M2-006** [requirement] No-Go-Einträge sollen nachträglich gelöscht werden können.
- **G-M2-007** [decision] No-Gos bleiben bewohnerbezogen; eine globale Geltung für alle Bewohner wurde ausdrücklich verworfen.
- **G-M2-008** [requirement] Beim Anlegen eines No-Gos soll aus einer Vorlagen-Liste häufiger No-Gos ausgewählt werden können; Einträge daraus gelten nicht automatisch global.
- **G-M2-009** [requirement] Der bestehende Sofortinfo-Bereich soll auf der Profil-Detailansicht beibehalten werden.
- **G-M2-010** [requirement] Der Sofortinfo-Bereich soll auf maximal fünf Einträge begrenzt sein.
- **G-M2-011** [requirement] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden.
- **G-M2-012** [requirement] Noch nicht synchronisierte, offline erfasste Änderungen sollen für den Nutzer sichtbar gekennzeichnet werden.
- **G-M2-013** [requirement] Wenn zwei Personen denselben Eintrag offline geändert haben, soll ein Konflikt klar angezeigt werden.
- **G-M2-014** [requirement] Bei konkurrierenden Offline-Änderungen darf keine Version stillschweigend überschrieben werden.
- **G-M2-015** [requirement] Bei einem Konflikt zwischen Offline-Änderungen soll der Nutzer entscheiden, welche Version gilt.
- **G-M2-016** [requirement] Für jede Schicht soll eine Übergabe-Notiz erfasst werden können.
- **G-M2-017** [requirement] Die Übergabe-Notiz einer Schicht soll der nächsten Schicht beim Öffnen der App prominent angezeigt werden.
- **G-M2-018** [requirement] Übergabe-Notizen sollen mit Bewohnerprofilen verlinkt werden können.
- **G-M2-019** [requirement] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.
- **G-M2-020** [requirement] Archivierte Übergabe-Notizen sollen weiterhin auffindbar bleiben.
- **G-M2-021** [requirement] Die App soll eine Stichwortsuche über alle Bewohnerprofile hinweg ermöglichen.
- **G-M2-022** [requirement] Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
- **G-M2-023** [requirement] Das Zugriffsprotokoll über Einsichtnahmen in Bewohnerdaten muss revisionssicher sein.
- **G-M2-024** [requirement] Das Zugriffsprotokoll darf nur von Admins eingesehen werden; normale Nutzer dürfen keinen Zugriff darauf haben.
- **G-M2-025** [open_question] Ob Angehörige selbst Inhalte in der App eintragen dürfen oder nur lesenden Zugriff erhalten, ist noch nicht entschieden.
- **G-M2-026** [requirement] Ein Dunkelmodus soll als nachrangiger Wunsch berücksichtigt werden.
- **G-M2-027** [requirement] Eine einstellbare Schriftgröße soll als nachrangiger Wunsch berücksichtigt werden.
- **G-M2-028** [decision] Firebase Firestore wird verbindlich als Datenbanklösung beibehalten; die Diskussion über Alternativen wird geschlossen.
- **G-M2-029** [architecture] Android 10 ist die Mindestversion der App; unterstützt werden Android 10 und neuere Versionen.
- **G-M2-030** [open_question] Der aufgrund der Android-10-Mindestversion notwendige Austausch älterer Geräte im Haus ist noch mit dem Träger zu klären.
- **G-M2-031** [open_question] Wie Piktogramme „größer gedacht“ und konkret berücksichtigt werden sollen, bleibt offen.

## OBS-01 (34 Aussagen)

01.01 [AU-0004] Die About-Me-Seite soll inhaltlich unverändert bleiben.
01.02 [AU-0006] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
01.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet werden können.
01.04 [AU-0007] Einträge auf der No-Go-Seite müssen gelöscht werden können.
01.05 [AU-0009] No-Gos bleiben pro Bewohner und gelten nicht global für alle Bewohner.
01.06 [AU-0009] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden.
01.07 [AU-0009] Aus der Vorlagen-Liste ausgewählte No-Gos sollen nicht automatisch global für alle Bewohner gelten.
01.08 [AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt werden.
01.09 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden.
01.10 [AU-0014] Noch nicht synchronisierte offline erfasste Inhalte müssen als noch nicht synchronisiert erkennbar sein.
01.11 [AU-0014,AU-0015] Wenn zwei Personen denselben Eintrag offline geändert haben, muss eine klare Konfliktanzeige angezeigt werden.
01.12 [AU-0014] Bei Offline-Konflikten darf nichts stillschweigend überschrieben werden.
01.13 [AU-0015] Bei Offline-Konflikten soll der Nutzer entscheiden, welche Version gilt.
01.14 [AU-0016] Es soll pro Schicht eine Übergabe-Notiz geben.
01.15 [AU-0016] Die Übergabe-Notiz einer Schicht soll der nächsten Schicht beim Öffnen der App prominent angezeigt werden.
01.16 [AU-0017] Übergabe-Notizen müssen mit Bewohnern verlinkbar sein.
01.17 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.
01.18 [AU-0018] Archivierte Übergabe-Notizen müssen auffindbar bleiben.
01.19 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
01.20 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden.
01.21 [AU-0021] Das Zugriffsprotokoll muss festhalten, wer wann welches Profil angesehen hat.
01.22 [AU-0022] Das Zugriffsprotokoll muss revisionssicher sein.
01.23 [AU-0021] Die App darf ohne Umsetzung der Auflage zur Protokollierung von Einsichtnahmen nicht eingesetzt werden.
01.24 [AU-0023] Das Zugriffsprotokoll darf nur von Admins eingesehen werden.
01.25 [AU-0023] Normale Nutzer dürfen das Zugriffsprotokoll nicht einsehen.
01.26 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen.
01.27 [AU-0025] Die Frage, ob Angehörige Inhalte eintragen dürfen, soll mit der Datenschutzbeauftragten geklärt werden.
01.28 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch aufgenommen, hat aber keine hohe Priorität.
01.29 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch aufgenommen, hat aber keine hohe Priorität.
01.30 [AU-0029,AU-0030] Firebase Firestore ist als technische Lösung endgültig festgelegt.
01.31 [AU-0029] Alternativen zu Firebase Firestore werden nicht weiter diskutiert.
01.32 [AU-0031,AU-0032] Als Mindestanforderung werden Geräte mit Android 10 oder höher unterstützt.
01.33 [AU-0031,AU-0032] Ältere Geräte als Android 10 müssten ausgetauscht werden, falls die Mindestanforderung so umgesetzt wird.
01.34 [AU-0033,AU-0034] Zum Thema Piktogramme besteht ein noch unkonkreter Klärungsbedarf.

## OBS-02 (19 Aussagen)

02.01 [AU-0004] Die bestehende Gestaltung der About-Me-Seite soll unverändert bleiben.
02.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
02.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
02.04 [AU-0008,AU-0009] No-Gos dürfen nicht global für alle Bewohner gelten, sondern bleiben pro Bewohner getrennt.
02.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden, ohne dass diese automatisch global gelten.
02.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht wird auf maximal fünf Einträge begrenzt.
02.07 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
02.08 [AU-0014,AU-0015] Bei offline entstandenen konkurrierenden Änderungen am selben Eintrag muss eine klare Konfliktanzeige erfolgen; Einträge dürfen dabei nicht stillschweigend überschrieben werden, sondern der Nutzer entscheidet, welche Version gilt.
02.09 [AU-0016] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.
02.10 [AU-0017] Übergabe-Notizen müssen bewohnerbezogen auf Profile verlinkbar sein.
02.11 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden; archivierte Notizen müssen weiterhin auffindbar bleiben.
02.12 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
02.13 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
02.14 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen, normale Nutzer nicht.
02.15 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen und mit der Datenschutzbeauftragten zu klären.
02.16 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch für den Nachtdienst aufgenommen und hat nachrangige Priorität.
02.17 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch aufgenommen und hat nachrangige Priorität.
02.18 [AU-0029,AU-0030] Firebase Firestore ist als Datenbanktechnologie endgültig festgelegt; die Diskussion über Alternativen wird nicht weitergeführt.
02.19 [AU-0031,AU-0032] Als Mindestanforderung ist zunächst Unterstützung ab Android 10 aufwärts zu planen; die finale Festlegung hängt noch von der Klärung mit dem Träger und einem möglichen Geräteaustausch ab.

## OBS-03 (19 Aussagen)

03.01 [AU-0004] Die bestehende Gestaltung der About-Me-Seite soll unverändert bleiben.
03.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
03.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
03.04 [AU-0008,AU-0009] No-Gos dürfen nicht global für alle Bewohner gelten, sondern bleiben pro Bewohner getrennt.
03.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden, ohne dass diese automatisch global gelten.
03.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht wird auf maximal fünf Einträge begrenzt.
03.07 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
03.08 [AU-0014,AU-0015] Bei offline entstandenen konkurrierenden Änderungen am selben Eintrag muss eine klare Konfliktanzeige erfolgen; Einträge dürfen dabei nicht stillschweigend überschrieben werden, sondern der Nutzer entscheidet, welche Version gilt.
03.09 [AU-0016] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.
03.10 [AU-0017] Übergabe-Notizen müssen bewohnerbezogen auf Profile verlinkbar sein.
03.11 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden; archivierte Notizen müssen weiterhin auffindbar bleiben.
03.12 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
03.13 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
03.14 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen, normale Nutzer nicht.
03.15 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen und mit der Datenschutzbeauftragten zu klären.
03.16 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch für den Nachtdienst aufgenommen und hat nachrangige Priorität.
03.17 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch aufgenommen und hat nachrangige Priorität.
03.18 [AU-0029,AU-0030] Firebase Firestore ist als Datenbanktechnologie endgültig festgelegt; die Diskussion über Alternativen wird nicht weitergeführt.
03.19 [AU-0031,AU-0032] Als Mindestanforderung ist zunächst Unterstützung ab Android 10 aufwärts zu planen; die finale Festlegung hängt noch von der Klärung mit dem Träger und einem möglichen Geräteaustausch ab.

## OBS-04 (14 Aussagen)

04.01 [AU-0004] Die bestehende Gestaltung und Funktion der About-Me-Seite soll unverändert bleiben.
04.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
04.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
04.04 [AU-0008,AU-0009] No-Gos bleiben bewohnerbezogen; ein globales No-Go-Modell für alle Bewohner ist abgelehnt.
04.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagenliste mit häufigen No-Gos zur Auswahl angeboten werden, ohne dass diese automatisch global gelten.
04.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht muss auf maximal fünf Einträge begrenzt werden, und beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
04.07 [AU-0014,AU-0015] Wenn zwei Personen denselben Eintrag offline geändert haben, müssen Konflikte angezeigt werden; Änderungen dürfen nicht stillschweigend überschrieben werden.
04.08 [AU-0016,AU-0017,AU-0018] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird; diese Notizen müssen bewohnerbezogen auf Profile verlinkbar sein und nach sieben Tagen automatisch archiviert werden, danach aber auffindbar bleiben.
04.09 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
04.10 [AU-0021,AU-0022,AU-0023] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat; das Zugriffsprotokoll darf nur von Admins eingesehen werden, nicht von normalen Nutzern.
04.11 [AU-0024,AU-0025] Es ist noch offen, ob Angehörige selbst Inhalte eintragen dürfen oder nur Leserechte erhalten.
04.12 [AU-0027,AU-0028] Ein Dunkelmodus und eine einstellbare Schriftgröße sind gewünschte, nachrangige Verbesserungen der App-Darstellung.
04.13 [AU-0029,AU-0030] Firebase Firestore bleibt die endgültige Datenbanklösung; Alternativen werden nicht weiter verfolgt.
04.14 [AU-0031,AU-0032] Die App soll mindestens Android 10 aufwärts unterstützen; der dafür nötige Austausch älterer Geräte ist noch organisatorisch zu klären.

## OBS-05 (14 Aussagen)

05.01 [AU-0004] Die bestehende Gestaltung und Funktion der About-Me-Seite soll unverändert bleiben.
05.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
05.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
05.04 [AU-0008,AU-0009] No-Gos bleiben bewohnerbezogen; ein globales No-Go-Modell für alle Bewohner ist abgelehnt.
05.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagenliste mit häufigen No-Gos zur Auswahl angeboten werden, ohne dass diese automatisch global gelten.
05.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht muss auf maximal fünf Einträge begrenzt werden, und beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
05.07 [AU-0014,AU-0015] Wenn zwei Personen denselben Eintrag offline geändert haben, müssen Konflikte angezeigt werden; Änderungen dürfen nicht stillschweigend überschrieben werden.
05.08 [AU-0016,AU-0017,AU-0018] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird; diese Notizen müssen bewohnerbezogen auf Profile verlinkbar sein und nach sieben Tagen automatisch archiviert werden, danach aber auffindbar bleiben.
05.09 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
05.10 [AU-0021,AU-0022,AU-0023] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat; das Zugriffsprotokoll darf nur von Admins eingesehen werden, nicht von normalen Nutzern.
05.11 [AU-0024,AU-0025] Es ist noch offen, ob Angehörige selbst Inhalte eintragen dürfen oder nur Leserechte erhalten.
05.12 [AU-0027,AU-0028] Ein Dunkelmodus und eine einstellbare Schriftgröße sind gewünschte, nachrangige Verbesserungen der App-Darstellung.
05.13 [AU-0029,AU-0030] Firebase Firestore bleibt die endgültige Datenbanklösung; Alternativen werden nicht weiter verfolgt.
05.14 [AU-0031,AU-0032] Die App soll mindestens Android 10 aufwärts unterstützen; der dafür nötige Austausch älterer Geräte ist noch organisatorisch zu klären.

## OBS-06 (35 Aussagen)

06.01 [AU-0004] Die About-Me-Seite soll unverändert bleiben.
06.02 [AU-0006] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal erhalten.
06.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet werden können.
06.04 [AU-0007] Einträge auf der No-Go-Seite müssen gelöscht werden können.
06.05 [AU-0009] No-Gos bleiben pro Bewohner und gelten nicht global für alle Bewohner.
06.06 [AU-0009] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden.
06.07 [AU-0009] Einträge aus der Vorlagen-Liste für No-Gos sollen nicht automatisch global für alle Bewohner gelten.
06.08 [AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt werden.
06.09 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
06.10 [AU-0014] Noch nicht synchronisierte offline erfasste Inhalte müssen für Nutzer sichtbar gekennzeichnet werden.
06.11 [AU-0014,AU-0015] Bei Konflikten durch Offline-Änderungen desselben Eintrags durch zwei Personen muss eine klare Konfliktanzeige angezeigt werden.
06.12 [AU-0014] Bei Konflikten durch Offline-Änderungen darf nichts stillschweigend überschrieben werden.
06.13 [AU-0015] Bei Konflikten durch Offline-Änderungen soll der Nutzer entscheiden, welche Version gilt.
06.14 [AU-0016] Es muss pro Schicht eine Übergabe-Notiz geben.
06.15 [AU-0016] Die nächste Schicht soll die Übergabe-Notiz beim Öffnen der App prominent angezeigt bekommen.
06.16 [AU-0017] Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein.
06.17 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.
06.18 [AU-0018] Archivierte Übergabe-Notizen müssen auffindbar bleiben.
06.19 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
06.20 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden.
06.21 [AU-0021] Das Zugriffsprotokoll muss erfassen, wer wann welches Profil angesehen hat.
06.22 [AU-0022] Das Zugriffsprotokoll muss revisionssicher sein.
06.23 [AU-0021] Ohne Umsetzung des Zugriffsprotokolls darf die App nicht eingesetzt werden.
06.24 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen.
06.25 [AU-0023] Normale Nutzer dürfen das Zugriffsprotokoll nicht einsehen.
06.26 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen.
06.27 [AU-0025] Die Frage, ob Angehörige Inhalte eintragen dürfen, soll mit der Datenschutzbeauftragten geklärt werden.
06.28 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch für den Nachtdienst aufgenommen.
06.29 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch aufgenommen.
06.30 [AU-0028] Dunkelmodus und einstellbare Schriftgröße haben niedrigere Priorität als die anderen Punkte.
06.31 [AU-0029,AU-0030] Firebase Firestore ist als Technologie endgültig festgelegt.
06.32 [AU-0029] Die Diskussion über Alternativen zu Firebase Firestore wird nicht weitergeführt.
06.33 [AU-0031,AU-0032] Die Anwendung unterstützt mindestens Android 10 und höher.
06.34 [AU-0031] Geräte mit älteren Android-Versionen als Android 10 müssten ausgetauscht werden.
06.35 [AU-0033,AU-0034] Das Thema Piktogramme ist noch nicht konkret genug und bleibt vorerst offen.

## OBS-07 (20 Aussagen)

07.01 [AU-0004] Die About-Me-Seite soll in ihrer aktuellen Form unverändert bleiben.
07.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
07.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
07.04 [AU-0009] No-Gos bleiben bewohnerbezogen und dürfen nicht automatisch global für alle Bewohner gelten.
07.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden.
07.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt werden.
07.07 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden.
07.08 [AU-0014,AU-0015] Bei Offline-Konflikten muss eine klare Konfliktanzeige erfolgen; Änderungen dürfen nicht stillschweigend überschrieben werden, und der Nutzer entscheidet über die gültige Version.
07.09 [AU-0016] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.
07.10 [AU-0017] Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein, sodass direkt auf ein Bewohnerprofil verwiesen werden kann.
07.11 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden und danach weiterhin auffindbar bleiben.
07.12 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
07.13 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
07.14 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer dürfen keinen Zugriff darauf haben.
07.15 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen und muss mit der Datenschutzbeauftragten geklärt werden.
07.16 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch für den Nachtdienst vorgesehen, hat aber keine hohe Priorität.
07.17 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch vorgesehen, um einzelnen älteren Kolleginnen zu helfen, hat aber keine hohe Priorität.
07.18 [AU-0029,AU-0030] Firebase Firestore bleibt endgültig die Datenbanklösung; Alternativen werden nicht weiter diskutiert.
07.19 [AU-0031,AU-0032] Android 10 aufwärts soll als Mindestanforderung eingeplant werden; die endgültige Abstimmung zu betroffenen Altgeräten mit dem Träger ist noch ausstehend.
07.20 [AU-0033,AU-0034] Der Wunsch, das Thema Piktogramme größer zu denken, ist noch nicht konkret genug und muss vor einer Ableitung von Anforderungen präzisiert werden.

## OBS-08 (20 Aussagen)

08.01 [AU-0004] Die About-Me-Seite soll in ihrer aktuellen Form unverändert bleiben.
08.02 [AU-0006] Die No-Go-Seite muss ein rotes Stopp-Symbol als deutliches Warnsignal anzeigen.
08.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet und gelöscht werden können.
08.04 [AU-0009] No-Gos bleiben bewohnerbezogen und dürfen nicht automatisch global für alle Bewohner gelten.
08.05 [AU-0009,AU-0010] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden.
08.06 [AU-0011,AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt werden.
08.07 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend erfasst werden.
08.08 [AU-0014,AU-0015] Bei Offline-Konflikten muss eine klare Konfliktanzeige erfolgen; Änderungen dürfen nicht stillschweigend überschrieben werden, und der Nutzer entscheidet über die gültige Version.
08.09 [AU-0016] Es muss pro Schicht eine Übergabe-Notiz geben, die der nächsten Schicht beim Öffnen der App prominent angezeigt wird.
08.10 [AU-0017] Übergabe-Notizen müssen bewohnerbezogen verlinkbar sein, sodass direkt auf ein Bewohnerprofil verwiesen werden kann.
08.11 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden und danach weiterhin auffindbar bleiben.
08.12 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
08.13 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss revisionssicher protokolliert werden, einschließlich wer wann welches Profil angesehen hat.
08.14 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen; normale Nutzer dürfen keinen Zugriff darauf haben.
08.15 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen und muss mit der Datenschutzbeauftragten geklärt werden.
08.16 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch für den Nachtdienst vorgesehen, hat aber keine hohe Priorität.
08.17 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch vorgesehen, um einzelnen älteren Kolleginnen zu helfen, hat aber keine hohe Priorität.
08.18 [AU-0029,AU-0030] Firebase Firestore bleibt endgültig die Datenbanklösung; Alternativen werden nicht weiter diskutiert.
08.19 [AU-0031,AU-0032] Android 10 aufwärts soll als Mindestanforderung eingeplant werden; die endgültige Abstimmung zu betroffenen Altgeräten mit dem Träger ist noch ausstehend.
08.20 [AU-0033,AU-0034] Der Wunsch, das Thema Piktogramme größer zu denken, ist noch nicht konkret genug und muss vor einer Ableitung von Anforderungen präzisiert werden.

## OBS-09 (34 Aussagen)

09.01 [AU-0004] Die About-Me-Seite soll unverändert bleiben.
09.02 [AU-0006] Die No-Go-Seite soll ein rotes Stopp-Symbol als deutliches Warnsignal enthalten.
09.03 [AU-0007] Einträge auf der No-Go-Seite müssen nachträglich bearbeitet werden können.
09.04 [AU-0007] Einträge auf der No-Go-Seite müssen gelöscht werden können.
09.05 [AU-0009] No-Gos bleiben bewohnerbezogen und gelten nicht global für alle Bewohner.
09.06 [AU-0009] Beim Anlegen von No-Gos soll eine Vorlagen-Liste mit häufigen No-Gos zur Auswahl angeboten werden.
09.07 [AU-0009] Einträge aus der Vorlagen-Liste für No-Gos gelten nicht automatisch global.
09.08 [AU-0012,AU-0013] Der Sofortinfo-Bereich auf der Profil-Detailansicht soll auf maximal fünf Einträge begrenzt werden.
09.09 [AU-0012,AU-0013] Beim Anlegen eines Bewohners muss mindestens ein Sofortinfo-Eintrag verpflichtend sein.
09.10 [AU-0014] Nicht synchronisierte offline erfasste Inhalte müssen für den Nutzer erkennbar sein.
09.11 [AU-0014,AU-0015] Wenn zwei Nutzer denselben Eintrag offline geändert haben, muss eine klare Konfliktanzeige angezeigt werden.
09.12 [AU-0014] Bei Offline-Konflikten darf nichts stillschweigend überschrieben werden.
09.13 [AU-0015] Bei Offline-Konflikten soll der Nutzer entscheiden, welche Version gilt.
09.14 [AU-0016] Es soll pro Schicht eine Übergabe-Notiz geben.
09.15 [AU-0016] Die Übergabe-Notiz einer Schicht soll der nächsten Schicht beim Öffnen der App prominent angezeigt werden.
09.16 [AU-0017] Übergabe-Notizen müssen mit Bewohnern verlinkbar sein.
09.17 [AU-0018] Übergabe-Notizen sollen nach sieben Tagen automatisch archiviert werden.
09.18 [AU-0018] Archivierte Übergabe-Notizen müssen auffindbar bleiben.
09.19 [AU-0019,AU-0020] Es soll eine Suche über alle Bewohnerprofile hinweg nach Stichworten geben.
09.20 [AU-0021,AU-0022] Jede Einsichtnahme in Bewohnerdaten muss protokolliert werden.
09.21 [AU-0021] Das Zugriffsprotokoll muss erfassen, wer wann welches Profil angesehen hat.
09.22 [AU-0022] Das Zugriffsprotokoll muss revisionssicher sein.
09.23 [AU-0021] Wenn die Protokollierung der Einsichtnahmen nicht umgesetzt wird, darf die App nicht eingesetzt werden.
09.24 [AU-0023] Das Zugriffsprotokoll dürfen nur Admins einsehen.
09.25 [AU-0023] Normale Nutzer dürfen das Zugriffsprotokoll nicht einsehen.
09.26 [AU-0024,AU-0025] Ob Angehörige selbst Inhalte eintragen dürfen oder nur lesen dürfen, ist noch offen.
09.27 [AU-0025] Die Frage, ob Angehörige Inhalte eintragen dürfen, soll mit der Datenschutzbeauftragten geklärt werden.
09.28 [AU-0027,AU-0028] Ein Dunkelmodus ist als Wunsch aufgenommen, hat aber keine hohe Priorität.
09.29 [AU-0027,AU-0028] Eine einstellbare Schriftgröße ist als Wunsch aufgenommen, hat aber keine hohe Priorität.
09.30 [AU-0029,AU-0030] Firebase Firestore ist als Technologie endgültig festgelegt.
09.31 [AU-0029] Alternativen zu Firebase Firestore werden nicht weiter diskutiert.
09.32 [AU-0031,AU-0032] Die Mindestanforderung für unterstützte Android-Geräte ist Android 10 oder höher.
09.33 [AU-0031] Ältere Geräte als Android 10 müssten ausgetauscht werden.
09.34 [AU-0033,AU-0034] Zum Thema Piktogramme besteht ein unspezifischer Änderungswunsch, der noch nicht konkretisiert ist.
