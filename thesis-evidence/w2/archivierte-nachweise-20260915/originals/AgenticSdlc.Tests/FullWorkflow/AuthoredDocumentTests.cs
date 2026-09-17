using AgenticSdlc.Host.FullWorkflow.Core;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// Autor-Artefakte (Slice S, 21.08.): EINE generische Naht mit fester Whitelist — freigegebene Fassung
// wörtlich, Version/Herkunfts-Kopf automatisch, Publikation über die Doc-Publish-Liste.
public sealed class AuthoredDocumentTests
{
    [Fact]
    public async Task Save_versioniert_mit_Herkunft_und_Publish_nimmt_jede_Art_mit()
    {
        var root = Path.Combine(Path.GetTempPath(), "authored-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(Path.Combine(root, "docs"));
        try
        {
            var (path, v1) = await AuthoredDocument.SaveAsync(root, "personas", "## Persona A\nBelegt: REQ-1");
            Assert.Equal(1, v1);
            var content = File.ReadAllText(path);
            Assert.Contains("# Personas", content);
            Assert.Contains("Version: 1", content);
            Assert.Contains("Belegt: REQ-1", content);                                    // Freigabe wörtlich
            Assert.Contains($"Entwurf: {AuthoredDocument.EntwurfAutor}", content);        // Default = Diktat

            var (_, v2) = await AuthoredDocument.SaveAsync(root, "personas", "Neu.", AuthoredDocument.EntwurfSteward);
            Assert.Equal(2, v2);                                                          // Save ersetzt + zählt hoch
            Assert.Contains($"Entwurf: {AuthoredDocument.EntwurfSteward}", File.ReadAllText(path));

            Assert.Equal("# Personas", AuthoredDocument.Read(root, "personas")!.Split('\n')[0]);
            Assert.Null(AuthoredDocument.Read(root, "vision"));                           // noch nicht erstellt

            // R-69: übernimmt der Entwurf den System-Kopf „wörtlich" mit, wird er beim Save abgestreift —
            // nie ein Doppel-Kopf, egal wie oft der Zyklus läuft.
            var (_, v3) = await AuthoredDocument.SaveAsync(root, "personas",
                "# Personas\n\n> Version: 2 · Stand: x\n> Freigabe: Autor (Steward-Chat) · Entwurf: y\n\n## Persona B");
            Assert.Equal(3, v3);
            var mitKopf = File.ReadAllText(path);
            Assert.Single(System.Text.RegularExpressions.Regex.Matches(mitKopf, "# Personas"));
            Assert.DoesNotContain("Stand: x", mitKopf);                       // Alt-Stempel weg
            Assert.Contains("## Persona B", mitKopf);                          // Inhalt unangetastet

            // Whitelist: unbekannte Art = LAUTER Fehler, nie freies Datei-Schreiben.
            await Assert.ThrowsAsync<ArgumentException>(() => AuthoredDocument.SaveAsync(root, "beliebig", "x"));

            // Doc-Publish nimmt jede existierende Art mit (Whitelist ist die EINE Quelle).
            await AuthoredDocument.SaveAsync(root, "c4", "```mermaid\nC4Context\n```");
            var core = new ProjectStateDocument("p", 4, DateTime.UnixEpoch, [], [], [], [], []);
            var ops = GithubDocPublish.SeedOps(root, core);
            Assert.Contains(ops, o => o.Title == "docs/personas.md");
            Assert.Contains(ops, o => o.Title == "docs/c4.md");
        }
        finally { Directory.Delete(root, true); }
    }
}
