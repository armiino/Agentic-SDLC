namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent;

// R3a (2026-07-22): geteilte Accept-Aufloesung der Human-Gates — die Dreifach-Logik
// (--accept-all | --accept ID,ID | human-decisions.json) existierte 4x kopiert in den *-hitl-Runnern.
// Die STUFEN-POLICY bleibt bewusst bei der Stufe (Callbacks): was "alle" heisst (IncomingItemId vs. op-{i}),
// und was "akzeptiert laut Datei" heisst (Default-Apply bei pbi-update vs. NUR explizites apply bei decision).
public static class HumanDecisions
{
    public static async Task<IReadOnlyList<string>?> ResolveAcceptedAsync(
        bool acceptAll, string? acceptList, string decisionsPath,
        Func<IReadOnlyList<string>> allIds,
        Func<string, Task<IReadOnlyList<string>>> acceptedFromFile)
    {
        if (acceptAll) return allIds();
        if (!string.IsNullOrWhiteSpace(acceptList))
            return acceptList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        if (File.Exists(decisionsPath)) return await acceptedFromFile(decisionsPath).ConfigureAwait(false);
        return null;
    }
}
