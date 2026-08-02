namespace AgenticSdlc.Host.FullWorkflow.Delta;

// =====================================================================================================================
// §5 Statusmodell-Refactor — S6: Einmal-Migration der Status-Achsen.
//
// FÜLLT die typisierten Felder (validity/progress/blocker/confirmedBy/confirmedInRun/decision) aller Core-Items aus dem
// Alt-`Status`-String — VERLUSTFREI (via CoreStatus.From, per Round-Trip bewiesen) und IDEMPOTENT (schon migrierte Items
// bleiben unangetastet). Bis hierher waren die Felder null und ReadStatus leitete transitorisch aus dem Alt-String ab;
// nach der Migration sind sie gesetzt, sodass S7 den Alt-String + den From-Fallback ersatzlos löschen kann.
//
// KEINE History-Notiz: die Migration ist reine REPRÄSENTATIONS-Umstellung, kein Statuswechsel. Der Status-INHALT bleibt
// identisch (ToLegacyString reproduziert den Alt-String bit-genau), also gibt es nichts zu protokollieren — die
// History-Stärkung (E-10) greift nur bei echten Wechseln über WithStatus.
//
// GOVERNANCE-RECOVERY inklusive: baseline→Agent, accepted→Human (E-10). Diese Info steckt heute implizit im Alt-String;
// ohne Migration ginge sie beim S7-Löschen verloren. `active`/needs_clarify/superseded tragen ehrlich Unmarked.
//
// Fliegt (mit dem transitorischen CoreStatus.From) in S7 raus — kein bleibender Adapter.
// =====================================================================================================================
public static class CoreStatusMigration
{
    /// <summary>Migriert alle Items des Cores (verlustfrei, idempotent). Reihenfolge/IDs/Relationen unverändert.
    /// Stempelt <see cref="ProjectStateDocument.CurrentSchemaVersion"/> (v4) — die gefüllten Achsen MACHEN den Core zu v4,
    /// die Datei muss das ehrlich etikettieren (Konvention der übrigen Apply-Pfade).</summary>
    public static ProjectStateDocument MigrateStatusAxes(this ProjectStateDocument core)
        => core with { SchemaVersion = ProjectStateDocument.CurrentSchemaVersion, Items = core.Items.Select(MigrateItem).ToList() };

    /// <summary>Migriert ein Item: leitet die Achsen aus dem Alt-<c>Status</c> (+ SourceRunId) ab, wenn noch nicht
    /// migriert. Idempotent — ein bereits gefülltes <see cref="ProjectStateItem.Validity"/> bleibt unangetastet.</summary>
    public static ProjectStateItem MigrateItem(this ProjectStateItem item)
    {
        if (item.Validity is not null) return item;   // schon migriert (z. B. via WithStatus in S3) — nichts tun
        var cs = CoreStatus.From(item);               // aus Alt-Status + SourceRunId; wirft LAUT bei unbekanntem Wert
        return item with
        {
            Validity = cs.Validity,
            Progress = cs.Progress,
            Blocker = cs.Blocker,
            ConfirmedBy = cs.ConfirmedBy,
            ConfirmedInRun = cs.ConfirmedInRun,
            Decision = cs.Decision,
        };
    }
}
