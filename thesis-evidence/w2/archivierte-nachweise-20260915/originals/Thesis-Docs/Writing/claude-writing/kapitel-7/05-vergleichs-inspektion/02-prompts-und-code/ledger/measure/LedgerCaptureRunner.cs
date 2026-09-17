using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AgenticSdlc.Host.FullWorkflow.Ledger;

/// <summary>
/// W2 L/LCR-Capture (Messachse, KEIN Produktpfad): projiziert aus den BEREITS persistierten
/// Artefakten eines ledger-build-units-Laufs die beiden Messpunkte des Evaluationskonzepts
/// (§11/§12.0-3, architektonische Grenze — nicht effekt-getrieben):
///
/// L-machine   = vollständiger Maker-Zustand nach Erzeugung + regulären formalen Prüfungen,
///               VOR aller nachgelagerten maschinellen Quality Control
///               → step-02-canonical-draft/output.json (kanonische Claims samt zugewiesener
///               Facetten, VOR CanonicalCheck⇄Repair; Facetten-VERGABE ist Maker-Arbeit,
///               Facetten-VALIDIERUNG ist QC).
/// LCR-machine = derselbe Zustand nach ALLEN maschinellen Prüf-/Repairmechanismen, VOR
///               menschlicher Adjudikation → step-03-facet-validation/output.json (Claims mit
///               Verdikten) + step-01d-Miss-Signal (Unused-Strang-Klassifikationen) +
///               QC-Protokolle (canonical-gate-Attempts, Quality-Gate, Downgrade-Zählung).
///
/// Rein LESEND und deterministisch (kein LLM, kein Schreiben außerhalb von capture/) — das
/// gemessene System bleibt unberührt. HITL-Messgrenze (§12.0-4) ist strukturell erzwungen:
/// gelesen werden AUSSCHLIESSLICH Artefakte der Steps 00–03 + gate/ — nie step-03b-adjudicated.
/// </summary>
public static class LedgerCaptureRunner
{
    public const string BoundaryL =
        "L-machine: vollstaendiger maschineller Maker-Zustand nach Erzeugung und den bis dahin regulaer "
        + "vorgesehenen formalen Pruefungen (Units, Kandidaten, kanonische Claims inkl. Facetten-Vergabe), "
        + "VOR saemtlichen nachgelagerten maschinellen Qualitaetskontroll- und Reparaturmechanismen "
        + "(CanonicalCheck/Repair, FacetValidation, Unused-Unit-Triage/Compare/Repair). "
        + "Quelle: step-02-canonical-draft/output.json.";

    public const string BoundaryLcr =
        "LCR-machine: derselbe Ausgangszustand nach Abschluss ALLER im Produktivpfad vorgesehenen "
        + "maschinellen Pruef-/Reparaturmechanismen, weiterhin VOR menschlicher Adjudikation. "
        + "Quellen: step-03-facet-validation/output.json (Claims + Verdikte), "
        + "step-01d-unused-unit-ledger-compare/output.json (Miss-Signal), gate/*.json (QC-Protokolle).";

    public sealed record CaptureArtifact(string Role, string Path, string Sha256);
    public sealed record CaptureReport(
        string RunId, DateTime CapturedUtc, bool PreAdjudication,
        string BoundaryLMachine, string BoundaryLcrMachine,
        IReadOnlyList<CaptureArtifact> Artifacts, IReadOnlyList<string> Notes,
        int LEntries, int LcrEntries, int MissItems, int ReferenceRepairDowngrades, int? CanonicalAttempts);

    /// <summary>CLI-Haut: <c>ledger-capture &lt;runId&gt;</c> (deterministisch, kein LLM).</summary>
    public static Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        { Console.Error.WriteLine("Usage: ledger-capture <ledgerRunId>   (L-/LCR-Messpunkte aus einem ledger-build-units-Lauf exportieren)"); return Task.FromResult(2); }
        try
        {
            var report = Capture(repoRoot, args[1]);
            Console.WriteLine($"[ledger-capture] {report.RunId}: L={report.LEntries} Claims (Draft) · "
                + $"LCR={report.LcrEntries} Claims + {report.MissItems} Miss-Signal-Items "
                + $"({report.ReferenceRepairDowngrades} Downgrades) · canonical Attempts={(report.CanonicalAttempts?.ToString() ?? "kein Protokoll")} "
                + $"— runs/ledger/{report.RunId}/capture/");
            foreach (var n in report.Notes) Console.WriteLine($"[ledger-capture]   Hinweis: {n}");
            return Task.FromResult(0);
        }
        catch (InvalidOperationException ex)
        { Console.Error.WriteLine($"[ledger-capture] FEHLER: {ex.Message}"); return Task.FromResult(2); }
    }

    /// <summary>Der Kern (testbar): liest die Step-Artefakte, schreibt capture/{l-machine,lcr-machine,capture-report}.json.</summary>
    public static CaptureReport Capture(string repoRoot, string runId)
    {
        var runDir = Path.Combine(repoRoot, "runs", "ledger", runId);
        if (!Directory.Exists(runDir)) throw new InvalidOperationException($"Ledger-Lauf fehlt: {runDir}");

        var artifacts = new List<CaptureArtifact>();
        var notes = new List<string>();

        var draft = ReadRequired(runDir, "step-02-canonical-draft/output.json", "l-source", artifacts);
        var validated = ReadRequired(runDir, "step-03-facet-validation/output.json", "lcr-claims", artifacts);
        var miss = ReadRequired(runDir, "step-01d-unused-unit-ledger-compare/output.json", "lcr-miss-signal", artifacts);
        var canonicalGate = ReadOptional(runDir, "gate/canonical-gate.json", "lcr-qc-canonical", artifacts,
            notes, "gate/canonical-gate.json fehlt (Lauf vor der R-33-Loop-Form 05.08.) — Attempts unbelegt.");
        var qualityGate = ReadOptional(runDir, "gate/ledger-quality.json", "lcr-qc-quality", artifacts,
            notes, "gate/ledger-quality.json fehlt — Quality-Gate-Protokoll unbelegt.");

        var lEntries = draft["entries"]?.AsArray().Count
            ?? throw new InvalidOperationException("step-02-canonical-draft/output.json ohne 'entries'.");
        var lcrEntries = validated["entries"]?.AsArray().Count
            ?? throw new InvalidOperationException("step-03-facet-validation/output.json ohne 'entries'.");
        var missItems = miss["items"]?.AsArray()
            ?? throw new InvalidOperationException("step-01d-…/output.json ohne 'items'.");
        var downgrades = missItems.Count(i =>
            i?["reason"]?.GetValue<string>()?.StartsWith(UnusedUnitCompareRepair.DowngradePrefix, StringComparison.Ordinal) == true);
        var attempts = canonicalGate?["attempts"]?.AsArray().Count;

        var captureDir = Path.Combine(runDir, "capture");
        Directory.CreateDirectory(captureDir);

        // l-machine/lcr-machine bewusst OHNE Zeitstempel: bit-stabil bei Wiederholung → hashbar (Freeze-Disziplin).
        WriteJson(Path.Combine(captureDir, "l-machine.json"), new JsonObject
        {
            ["runId"] = runId,
            ["boundary"] = BoundaryL,
            ["entries"] = draft["entries"]!.DeepClone(),
        });
        WriteJson(Path.Combine(captureDir, "lcr-machine.json"), new JsonObject
        {
            ["runId"] = runId,
            ["boundary"] = BoundaryLcr,
            ["entries"] = validated["entries"]!.DeepClone(),
            ["missSignal"] = missItems.DeepClone(),
            ["qc"] = new JsonObject
            {
                ["canonicalGate"] = canonicalGate?.DeepClone(),
                ["qualityGate"] = qualityGate?.DeepClone(),
                ["referenceRepairDowngrades"] = downgrades,
            },
        });

        var report = new CaptureReport(runId, DateTime.UtcNow, PreAdjudication: true,
            BoundaryL, BoundaryLcr, artifacts, notes, lEntries, lcrEntries, missItems.Count, downgrades, attempts);
        File.WriteAllText(Path.Combine(captureDir, "capture-report.json"), JsonSerializer.Serialize(report, JsonFiles.Json));
        return report;
    }

    private static JsonNode ReadRequired(string runDir, string relative, string role, List<CaptureArtifact> artifacts)
        => ReadOptional(runDir, relative, role, artifacts, notes: null, note: null)
           ?? throw new InvalidOperationException($"{relative} fehlt — ohne dieses Artefakt ist der Messpunkt nicht belegbar.");

    private static JsonNode? ReadOptional(string runDir, string relative, string role,
        List<CaptureArtifact> artifacts, List<string>? notes, string? note)
    {
        var path = Path.Combine(runDir, relative.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            if (note is not null) notes?.Add(note);
            return null;
        }
        var bytes = File.ReadAllBytes(path);
        artifacts.Add(new CaptureArtifact(role, relative, Convert.ToHexString(SHA256.HashData(bytes))));
        return JsonNode.Parse(bytes) ?? throw new InvalidOperationException($"{relative} ist kein JSON.");
    }

    private static void WriteJson(string path, JsonObject obj)
        => File.WriteAllText(path, obj.ToJsonString(JsonFiles.Json));
}
