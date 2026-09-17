using AgenticSdlc.Host.FullWorkflow.Core;
using System.Security.Cryptography;
using System.Text;

namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>
/// C2a-2 (08.08.2026, c2-inbound-plan §7) — die EINE Drift-Quelle: „hat ein MENSCH dieses Issue angefasst?"
/// Gemessen wird gegen den LETZTEN SCHREIB-STEMPEL (projectedBody/TitleHash in der Mapping-Metadata, vom
/// gated Forward-Apply gesetzt) — NICHT gegen den aktuellen Core-Render. Nur so ist menschlicher Edit von
/// legitimer Core-Weiterentwicklung unterscheidbar (Konvergenz: nach jedem Apply gilt Snapshot==Stempel).
/// Konsumenten: Forward-Seed (Drift-Sperre, C2a-3) UND Inbound-Detect (Ernte, C2a-4).
/// </summary>
public static class GithubDriftCheck
{
    public static GithubDrift Check(GithubMappingRecord mapping, GithubIssueSnapshot issue)
    {
        if (mapping.ProjectedBodyHash is null || mapping.ProjectedTitleHash is null)
            return GithubDrift.Unknown;   // Alt-Issue ohne Stempel (heilt beim nächsten Apply) → warnen, nicht blocken
        return GithubProjectionHash.Compute(issue.Body) == mapping.ProjectedBodyHash
            && GithubProjectionHash.Compute(issue.Title) == mapping.ProjectedTitleHash
            ? GithubDrift.None
            : GithubDrift.HumanEdited;
    }
}

/// <summary>Drift-Status eines gemappten Issues gegenüber dem letzten eigenen Schreib-Stand.</summary>
public enum GithubDrift
{
    /// <summary>Snapshot == letzter Schreib-Stempel — niemand hat es angefasst.</summary>
    None,
    /// <summary>Ein Mensch hat Titel/Body seit unserem letzten Write verändert → Ernte-Fall (F1) + Forward-Sperre.</summary>
    HumanEdited,
    /// <summary>Kein Stempel vorhanden (Alt-Issue vor C2a-2) — Drift nicht bestimmbar; erster Apply-Zyklus heilt.</summary>
    Unknown,
}

/// <summary>
/// Der Projektions-Hash: normalisiert Zeilenenden (GitHub liefert Web-Edits mit \r\n, unser Render schreibt \n)
/// und hängende Leerzeilen, damit NUR echte Inhalts-Änderung als Drift zählt. 16 Hex-Zeichen SHA-256 —
/// kompakt für die Mapping-Metadata, mehr als kollisionsfest genug für Drift-Erkennung.
/// </summary>
public static class GithubProjectionHash
{
    public static string Compute(string? text)
    {
        // Normalisierung = nur Informationsloses (Autor-⚖ 20.08., R-60-Nachwehe): CRLF, Datei-Ende-Rest
        // UND Trailing-Whitespace je Zeile — ein Leerzeichen am Zeilenende ist kein „menschlicher Edit"
        // und darf keine Drift-Sperre auslösen. Eigene Render tragen nie Trailing-Whitespace, bestehende
        // Stempel bleiben dadurch gültig. INHALTLICHE Abweichungen (auch Leerzeilen MITTEN im Text via
        // Zeilenzahl) zählen weiter.
        var normalized = string.Join('\n', (text ?? "").Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd()))
            .TrimEnd('\n');
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)))[..16];
    }
}
