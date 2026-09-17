namespace AgenticSdlc.Host.FullWorkflow.Derivation;

/// <summary>
/// Rollen-basiertes Ausgabe-Layout eines Ableitungs-Ordners (<c>derivations/&lt;spec&gt;/</c>). Macht „woher kommt was"
/// auf einen Blick sichtbar — Single-Source für die Unterordner-Namen (kein Magic-String-Drift über die Schreiber).
///
/// <code>
/// derivations/&lt;spec&gt;/
/// ├── derived.json            ← AGENT: das Deliverable (bleibt oben)
/// ├── derivation-report.json  ← HOST: Master-Index/Roll-up (bleibt oben)
/// ├── agent/                  ← was der AGENT selbst tat (In-Loop-Selbstcheck)
/// ├── checks/                 ← die UNABHÄNGIGEN Host-Prüfungen (Judge · Rechenschaft · DoD)
/// └── reflect/                ← die Reflect-Loop-Provenienz (Runden-Snapshots + Item-Diffs)
/// </code>
/// </summary>
internal static class DerivationLayout
{
    public const string Agent = "agent";       // was der Agent selbst produzierte (verify-round)
    public const string Checks = "checks";     // unabhängige Host-Prüfungen (inference-check/coverage/dod)
    public const string Reflect = "reflect";   // Loop-Provenienz (derived.round-NN + revise-Diffs)

    /// <summary>Stellt den Unterordner sicher und gibt den vollen Pfad einer Datei darin zurück.</summary>
    public static string File(string outDir, string sub, string fileName)
    {
        var dir = Path.Combine(outDir, sub);
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
