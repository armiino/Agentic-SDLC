using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Tore.Github.Inbound;
using System.Text.Json.Serialization;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>Warn-Note „ungeerntete GitHub-Arbeit" — Ergebnis von <see cref="GithubUnharvested.Compute"/>.</summary>
public sealed record GithubUnharvestedNote(
    [property: JsonPropertyName("neu")] int Neu,
    [property: JsonPropertyName("geaendert")] int Geaendert,
    [property: JsonPropertyName("text")] string Text);

// 13.08. — die Auflösung des conditional-edge-Disk-Punkts (aufgefallen.md): der Autor soll AM WRITE-MOMENT
// wissen, ob auf GitHub UNGEERNTETE Arbeit liegt („erst ernten?") — ohne Graph-Umbau. Der SnapshotExecutor
// zieht vor jedem Forward ohnehin den frischen Snapshot; diese pure Funktion legt die GETEILTE Detect-Engine
// (GithubInboundDetect — dieselbe wie Ernte/Zwischenbahn) darüber und zählt die erntbaren Funde:
//   neu = UnmappedNew (fremdes Issue ohne Core-Anker) · geaendert = MappedDrift + AdoptedDrift (Mensch-Edit).
// NiC-Opt-outs und Unchanged zählen NICHT (kein Rauschen); UnknownStamp ebenfalls nicht (kein sicherer Fund).
// GRENZE (dokumentiert): Kommentare zählt sie nicht — das bräuchte einen zweiten API-Pull; die Ernte
// (--from-github) sammelt sie trotzdem ein. 0 Funde ⇒ null (keine Note, kein leeres Warnen).
public static class GithubUnharvested
{
    public static GithubUnharvestedNote? Compute(ProjectStateDocument core, IReadOnlyList<GithubIssueSnapshot> issues)
    {
        var report = GithubInboundDetect.Detect(core, issues);
        var neu = report.Finds.Count(f => f.Category == GithubInboundCategory.UnmappedNew);
        var geaendert = report.Finds.Count(f =>
            f.Category is GithubInboundCategory.MappedDrift or GithubInboundCategory.AdoptedDrift);
        if (neu + geaendert == 0) return null;
        return new(neu, geaendert,
            $"GitHub-seitig UNGEERNTET: {neu} neue(s) Issue(s), {geaendert} geaendert — wenn das in die Wahrheit soll: "
            + "betroffene Ops skippen, Ernte fahren (pipeline-full run --from-github), Rest danach via reproject.");
    }
}
