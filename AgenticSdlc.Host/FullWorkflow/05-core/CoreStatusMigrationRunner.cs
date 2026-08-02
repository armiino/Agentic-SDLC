using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Core;

// CLI: core-migrate-status [--apply]
//
// §5-S6: füllt die typisierten Status-Achsen aller Core-Items aus dem Alt-`Status`-String (verlustfrei, idempotent —
// s. CoreStatusMigration). Default = VERIFY (dry-run): migriert nur in-memory und BEWEIST
//   (a) Round-Trip je Item — der Alt-String ist aus den gefüllten Achsen bit-genau rekonstruierbar, und
//   (b) Konsumenten-Stabilität — ActiveBacklog / Archive / GithubSync sind vor↔nach byte-gleich.
// Schreibt NICHTS. --apply schreibt den migrierten Core (snapshot-geschützt via SaveAsync), NACHDEM die Verifikation
// grün ist. Governance/Gates bleiben unberührt: die Migration ändert keinen Status-INHALT, nur die Repräsentation.
public static class CoreStatusMigrationRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        var apply = args.Any(a => string.Equals(a, "--apply", StringComparison.OrdinalIgnoreCase));

        var repo = new JsonCoreRepository(repoRoot);
        if (!await repo.ExistsAsync().ConfigureAwait(false))
        {
            Console.Error.WriteLine("[core-migrate-status] Core fehlt - erst 'core-seed' fahren.");
            return 2;
        }
        var core = await repo.LoadAsync().ConfigureAwait(false);
        var migrated = core.MigrateStatusAxes();

        // (a) Round-Trip je Item: der Alt-String muss aus den gefüllten Achsen bit-genau rekonstruierbar sein.
        var roundtrip = migrated.Items
            .Where(i => !string.Equals(i.ReadStatus().ToLegacyString(), i.Status, StringComparison.Ordinal))
            .Select(i => $"{i.ItemId}: '{i.Status}' -> '{i.ReadStatus().ToLegacyString()}'")
            .ToList();

        // (b) Konsumenten-Stabilität. Die Migration füllt legitim neue Felder AN den Roh-Items (das ist ihr Zweck), also
        //     ist Roh-Serialisierung zu streng. Semantisch stabil = gleiche MITGLIEDSCHAFT je View (welche Items landen
        //     wo) + gleiche abgeleitete GithubSync-Projektion (reine Projektion, keine Roh-Items -> byte-gleich).
        static IEnumerable<string> Ids(IEnumerable<ProjectStateItem> items) => items.Select(i => i.ItemId);
        var viewDiffs = new List<string>();

        var ab0 = CoreViews.ActiveBacklog(core);
        var ab1 = CoreViews.ActiveBacklog(migrated);
        if (!Ids(ab0.Features).SequenceEqual(Ids(ab1.Features)) || !Ids(ab0.Pbis).SequenceEqual(Ids(ab1.Pbis))
            || !Ids(ab0.Requirements).SequenceEqual(Ids(ab1.Requirements)) || !Ids(ab0.OpenDecisions).SequenceEqual(Ids(ab1.OpenDecisions)))
            viewDiffs.Add("active-backlog");
        if (!Ids(CoreViews.Archive(core).Items).SequenceEqual(Ids(CoreViews.Archive(migrated).Items)))
            viewDiffs.Add("archive");
        if (!string.Equals(JsonSerializer.Serialize(CoreViews.GithubSync(core), Json),
                           JsonSerializer.Serialize(CoreViews.GithubSync(migrated), Json), StringComparison.Ordinal))
            viewDiffs.Add("github-sync");

        var newlyMigrated = migrated.Items.Count(i => i.Validity is not null) - core.Items.Count(i => i.Validity is not null);
        Console.WriteLine($"[core-migrate-status] items={core.Items.Count} neu-migriert={newlyMigrated} "
            + $"roundtrip-fehler={roundtrip.Count} view-diffs={viewDiffs.Count}");

        if (roundtrip.Count > 0)
        {
            Console.Error.WriteLine("[core-migrate-status] ROUND-TRIP NICHT VERLUSTFREI (Migration abgebrochen):");
            foreach (var r in roundtrip.Take(20)) Console.Error.WriteLine("  " + r);
            return 1;
        }
        if (viewDiffs.Count > 0)
        {
            Console.Error.WriteLine($"[core-migrate-status] KONSUMENTEN-DIFF (Migration abgebrochen) in: {string.Join(", ", viewDiffs)}");
            return 1;
        }

        if (!apply)
        {
            Console.WriteLine("[core-migrate-status] VERIFY grün: verlustfrei + konsumenten-stabil. Schreiben mit --apply.");
            return 0;
        }

        await repo.SaveAsync(migrated).ConfigureAwait(false);
        Console.WriteLine("[core-migrate-status] --apply: migrierter Core geschrieben (Vorstand als Snapshot in state/core/history/).");
        return 0;
    }
}
