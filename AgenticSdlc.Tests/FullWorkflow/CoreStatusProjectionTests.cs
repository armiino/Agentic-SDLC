using System.Text.Json;
using AgenticSdlc.Host.FullWorkflow;
using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// §5-S7 (Option A): der KNACKPUNKT-Beweis. `ProjectStateItem.Status` ist eine berechnete get-only-Projektion aus den
// typisierten Achsen — KEINE gespeicherte Wahrheit. Diese Tests sperren genau das ein:
//   (a) status wird in die JSON geschrieben (Ausgabeformat),
//   (b) beim Deserialisieren gewinnen die ACHSEN — der status-String im JSON ist KEINE Quelle (der entscheidende Test),
//   (c) ein Item ohne Achsen scheitert LAUT (kein stiller From-Fallback, keine Rekursion).
public sealed class CoreStatusProjectionTests
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json;

    private static ProjectStateItem Bare() => new(
        "X", "requirement", "t", "o", null, 1, null, null, null, null, null, [], [],
        new Dictionary<string, string>());

    [Fact]
    public void Status_wird_als_Projektion_in_die_JSON_geschrieben()
    {
        var item = Bare().WithStatus(CoreStatus.From("superseded"));
        Assert.Equal("superseded", item.Status);                                       // Projektion == Achsen→String
        Assert.Contains("\"status\": \"superseded\"", JsonSerializer.Serialize(item, Json));   // steht in der JSON
    }

    // DER entscheidende Test: JSON mit WIDERSPRÜCHLICHEM status ("active") aber validity "Superseded" — die Achse gewinnt,
    // der status-String wird beim Lesen IGNORIERT. Beweist: status ist Ausgabeformat, nicht Quelle.
    [Fact]
    public void Deserialisieren_Achsen_gewinnen_ueber_den_status_String()
    {
        var json = JsonSerializer.Serialize(Bare().WithStatus(CoreStatus.From("superseded")), Json);
        var tampered = json.Replace("\"status\": \"superseded\"", "\"status\": \"active\"");
        Assert.Contains("\"status\": \"active\"", tampered);           // die Fälschung ist wirklich drin ...
        Assert.Contains("\"validity\": \"Superseded\"", tampered);     // ... und die Achse steht dagegen

        var back = JsonSerializer.Deserialize<ProjectStateItem>(tampered, Json)!;
        Assert.Equal(Validity.Superseded, back.Validity);             // Achse aus der JSON gelesen
        Assert.Equal("superseded", back.Status);                      // Projektion folgt der ACHSE, nicht dem status-String
    }

    // Ein Item ohne Achsen (nie durch WithStatus/From gelaufen) muss LAUT scheitern — kein stiller Fallback, keine Rekursion.
    [Fact]
    public void Item_ohne_Achsen_scheitert_LAUT_statt_still_zu_raten()
    {
        var axisless = Bare();   // KEIN WithStatus → Validity null
        Assert.Throws<InvalidOperationException>(() => axisless.ReadStatus());
        Assert.Throws<InvalidOperationException>(() => _ = axisless.Status);   // die Projektion wirft ebenso (kein Stack-Overflow)
    }

    // Guarded: der echte Produktiv-Core (nach S6-Migration) muss unter Option A vollständig laden — jedes Item trägt Achsen,
    // jede Status-Projektion greift (kein Guard-Wurf), die Views rechnen. Übernimmt die reale Lade-Abdeckung des entfernten
    // CoreStatusMigrationTests. Läuft nur, wenn die Datei im Repo liegt (sonst grün durchgelassen).
    [Fact]
    public void Echter_Core_laedt_unter_Option_A_und_jede_Status_Projektion_greift()
    {
        var coreFile = FindRepoFile(Path.Combine("state", "core", "project-state.json"));
        if (coreFile is null) return;

        var core = JsonSerializer.Deserialize<ProjectStateDocument>(File.ReadAllText(coreFile), Json)!;
        Assert.NotEmpty(core.Items);
        Assert.All(core.Items, i =>
        {
            Assert.NotNull(i.Validity);                                 // migriert (Achsen gesetzt)
            Assert.False(string.IsNullOrEmpty(i.Status));              // Projektion greift (kein Guard-Wurf)
            Assert.Equal(i.Status, i.ReadStatus().ToLegacyString());   // konsistent
        });
        _ = CoreViews.ActiveBacklog(core);   // die Konsumenten-Views rechnen ohne Fehler
        _ = CoreViews.Archive(core);
        _ = CoreViews.GithubSync(core);
    }

    private static string? FindRepoFile(string relative)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}
