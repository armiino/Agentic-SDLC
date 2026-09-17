namespace AgenticSdlc.Host.FullWorkflow.Tore.Github;

/// <summary>Ergebnis eines echten GitHub-Issue-Writes (Create/Update/State). Typ-Schnitt 2026-07-23:
/// aus research/github-write in die Live-Heimat gehoben (letzte Live-Abhängigkeit auf research/).</summary>
internal sealed record GithubIssueWriteResult(
    int IssueNumber,
    string? IssueUrl,
    string State,
    string Title);
