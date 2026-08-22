using AgenticSdlc.Host.FullWorkflow.Delta;

namespace AgenticSdlc.Host.FullWorkflow.Analyst;

/// <summary>
/// 1g-B: Funde → Delta im Meeting-Ketten-Vertrag — der EXISTIERENDE `--from-delta`-Eingang ist der einzige
/// Weg in die Wahrheit (Tor 1 entscheidet; der Analyst prägt NIE). Spiegel des AuthorFrontDeltaBuilder mit
/// ehrlicher Herkunft: Origin `CoreAnalyst` (§2: Hypothese, nie „aus dem Meeting"), Metadata trägt
/// Linse + Kategorie (§4-1, strukturiert das Anforderungsdokument) + wörtliche Herleitung (Beleg).
/// </summary>
public static class AnalystDeltaBuilder
{
    public const string Origin = "CoreAnalyst";

    public static ProjectStateDocument Build(IReadOnlyList<AnalystFinding> findings, string analysisRunId)
    {
        var sourceId = $"core-analysis:{analysisRunId}";
        var items = findings.Select((f, i) => new ProjectStateItem(
            $"CA-{i + 1}", ItemTypeOf(f.Disposition), f.Statement.Trim(), Origin, "core_analyst", 1,
            analysisRunId, sourceId, "analyst-finding", null, null, [], [],
            BuildMeta(f)).WithStatus(CoreStatus.From("baseline"))).ToList();

        return new ProjectStateDocument("agentic-sdlc", ProjectStateDocument.CurrentSchemaVersion, DateTime.UtcNow,
            [new ProjectStateSource(sourceId, "analyst-finding", $"runs/core-analysis/{analysisRunId}", analysisRunId,
                "Lücken-Hypothesen des Core-Analysten (erschlossen aus dem Bestand — Autor entscheidet an Tor 1)")],
            items, [],
            items.Select(i => new ProjectStateProvenance(i.ItemId,
                [new ProjectStateProvenanceLink("analyst-finding", i.ItemId, "derived_from_core", sourceId,
                    new Dictionary<string, string> { ["ankerIds"] = i.Metadata["ankerIds"] })])).ToList(),
            []);
    }

    private static string ItemTypeOf(string disposition)
        => string.Equals(disposition, "question", StringComparison.Ordinal) ? "open_question" : disposition;

    private static Dictionary<string, string> BuildMeta(AnalystFinding f)
    {
        var meta = new Dictionary<string, string>
        {
            ["quelle"] = "core-analyst (erschlossen, nicht gesagt)",
            ["linse"] = f.Linse,
            [Core.RequirementsDocumentProjection.KategorieKey] = f.Kategorie,
            ["herleitung"] = f.Herleitung,
            ["ankerIds"] = string.Join(",", f.AnkerIds),
        };
        // C4-Kreislauf (22.08.): eine FRAGE der arch-Linse IST eine Architektur-Unklarheit — die Färbung
        // bringt sie (als DEC) automatisch in die §3-Lücken-Projektion des C4, egal ob sie je ein ?-Kasten war.
        if (string.Equals(f.Disposition, "question", StringComparison.Ordinal)
            && string.Equals(f.Linse, "arch", StringComparison.Ordinal))
            meta[Core.DecisionAspectMeta.Key] = Core.DecisionAspectMeta.Architecture;
        return meta;
    }
}
