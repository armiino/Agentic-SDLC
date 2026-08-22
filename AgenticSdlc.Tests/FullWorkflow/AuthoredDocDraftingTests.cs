using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Endform der Autor-Artefakte (21.08.): der Entwurf kommt vom Drafting-Agenten über die geteilte Naht —
// deterministischer Input (Digest + Art-Spezifika + aktueller Stand + Hinweise), je Art eine Prompt-Datei.
public sealed class AuthoredDocDraftingTests
{
    private static ProjectStateItem Item(string id, string type, string text) => new ProjectStateItem(
        id, type, text, "MEETING", null, 1, "r", null, null, null, null, [], [],
        new Dictionary<string, string>()).WithStatus(CoreStatus.From("accepted"));

    [Fact]
    public async Task BuildInput_traegt_Stand_Hinweise_Digest_und_fuer_c4_die_Arch_Wahrheit()
    {
        var root = Path.Combine(Path.GetTempPath(), "draft-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs", "adr"));
        File.WriteAllText(Path.Combine(root, "docs", "adr", "0001-db.md"), "# ADR 0001: PostgreSQL");
        try
        {
            var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
                [Item("REQ-1", "requirement", "Angehörige sehen Besuche."),
                 Item("ARCH-1", "architecture", "PostgreSQL als Datenbank.")], [], [], []);

            var personas = AuthoredDocument.Resolve("personas")!;
            var erst = AuthoredDocDrafting.BuildInput(root, core, personas, hinweise: null);
            Assert.Contains("Erst-Entwurf", erst);
            Assert.Contains("REQ-1", erst);                                     // Digest ist die Grundlage
            Assert.DoesNotContain("ADR 0001", erst);                            // ADRs nur für c4

            await AuthoredDocument.SaveAsync(root, "personas", "## Persona Anna");
            var update = AuthoredDocDrafting.BuildInput(root, core, personas, "Anna kürzer fassen.");
            Assert.Contains("UPDATE-Fall", update);
            Assert.Contains("Persona Anna", update);                            // aktueller Stand reist mit
            Assert.Contains("AUTOR-HINWEISE", update);
            Assert.Contains("Anna kürzer fassen.", update);

            var c4 = AuthoredDocDrafting.BuildInput(root, core, AuthoredDocument.Resolve("c4")!, null);
            Assert.Contains("ARCH-1: PostgreSQL als Datenbank.", c4);           // volle Arch-Texte (Beleg-Pflicht)
            Assert.Contains("ADR 0001", c4);
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task C4FrischeNotiz_Beleg_Stand_Zyklus_am_echten_Save()   // R-71
    {
        var root = Path.Combine(Path.GetTempPath(), "c4fresh-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs", "adr"));
        try
        {
            var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [],
                [Item("ARCH-1", "architecture", "PostgreSQL.")], [], [], []);

            Assert.Null(AuthoredDocDrafting.C4FrischeNotiz(root, core));        // kein C4 = kein Genörgel

            // Autor-Save stempelt den Beleg-Stand (ohne Core im Temp-Root gäbe es keinen Stempel — Core anlegen).
            await new JsonCoreRepository(root).SaveAsync(core);
            await AuthoredDocument.SaveAsync(root, "c4", "Kasten DB — Beleg: ARCH-1");
            Assert.Null(AuthoredDocDrafting.C4FrischeNotiz(root, core));        // Stand = Bestand ⇒ Ruhe

            // Bestand wächst (neues ARCH + neues ADR) ⇒ NUR das Delta wird gemeldet.
            var groesser = core with { Items = [.. core.Items,
                Item("ARCH-2", "architecture", "On-Premise.")] };
            File.WriteAllText(Path.Combine(root, "docs", "adr", "0002-sync.md"), "# ADR");
            var notiz = AuthoredDocDrafting.C4FrischeNotiz(root, groesser)!;
            Assert.Contains("2 NEUE", notiz);
            Assert.Contains("ARCH-2", notiz);
            Assert.Contains("adr:0002-sync.md", notiz);
            Assert.DoesNotContain("ARCH-1", notiz.Replace("ARCH-1|", ""));      // Altbestand nie gemeldet

            // Nächster Autor-Save stempelt neu ⇒ wieder Ruhe.
            await new JsonCoreRepository(root).SaveAsync(groesser);
            await AuthoredDocument.SaveAsync(root, "c4", "Kasten DB — Beleg: ARCH-1, ARCH-2");
            Assert.Null(AuthoredDocDrafting.C4FrischeNotiz(root, groesser));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void Jede_Artefakt_Art_hat_ihre_Prompt_Datei()
    {
        var repoRoot = Directory.GetCurrentDirectory();
        while (!Directory.Exists(Path.Combine(repoRoot, "AgenticSdlc.Host", "Prompts")))
            repoRoot = Path.GetDirectoryName(repoRoot) ?? throw new InvalidOperationException("Repo-Wurzel nicht gefunden");
        foreach (var art in AuthoredDocument.Arten)
            Assert.True(File.Exists(Path.Combine(repoRoot, "AgenticSdlc.Host", "Prompts", "phase2_evidence",
                    AuthoredDocDrafting.AgentName, AuthoredDocDrafting.PromptNameFor(art.Key) + ".txt")),
                $"Prompt-Datei fehlt für Art '{art.Key}' — neue Whitelist-Art braucht ihr Drafting-Rezept.");
    }
}
