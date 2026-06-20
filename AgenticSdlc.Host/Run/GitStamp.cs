using System.Diagnostics;

namespace AgenticSdlc.Host.Run;

/// <summary>
/// Code-Stand-Stempel eines Runs: welcher Git-Commit lief, auf welchem Branch, und ob der
/// Arbeitsbaum „dirty" war (uncommittete Änderungen). Bei dirty werden zusätzlich der Diff der
/// getrackten Dateien und die Statusliste (inkl. neuer, noch ungetrackter Dateien) in den
/// Run-Ordner geschrieben: <c>runs/&lt;phase&gt;/&lt;runId&gt;/code-version/</c>.
/// </summary>
/// <remarks>
/// Zweck (Thesis-Reproduzierbarkeit): Jeder Run weiß, gegen welchen Code er lief — auch
/// uncommittete Zwischenstände. Damit ist <c>run → Code</c> rekonstruierbar, OHNE pro Run committen
/// zu müssen (siehe <c>commit-rules.md</c>).
///
/// Grenze, die ehrlich benannt gehört: <c>git diff HEAD</c> sieht nur Änderungen an bereits
/// getrackten Dateien. Inhalte ganz NEUER, noch nicht getrackter Dateien stehen NICHT im Diff; sie
/// werden in <c>git-status.txt</c> nur namentlich gelistet (<c>??</c>-Zeilen). Der harte
/// Reproduzierbarkeits-Anker bleibt ein Commit auf „clean" — der Stempel macht die Lücke nur sichtbar
/// und klein.
/// </remarks>
public sealed record GitStampInfo(
    bool Available,
    string? Commit = null,
    string? ShortCommit = null,
    string? Branch = null,
    bool Dirty = false,
    int ChangedFiles = 0,
    string? TrackedDiffFile = null,
    string? StatusFile = null);

public static class GitStamp
{
    /// <summary>Erfasst den aktuellen Git-Stand und schreibt bei dirty den Diff in den Run-Ordner.</summary>
    public static GitStampInfo Capture(RunContext run, string repoRoot)
    {
        var commit = RunGit(repoRoot, "rev-parse HEAD");
        if (commit is null)
            return new GitStampInfo(Available: false); // kein git / kein Repo → still ausgeben, nicht crashen

        var branch = RunGit(repoRoot, "rev-parse --abbrev-ref HEAD");
        var status = RunGit(repoRoot, "status --porcelain") ?? string.Empty;
        var dirty = !string.IsNullOrWhiteSpace(status);
        var changedFiles = dirty
            ? status.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length
            : 0;

        string? diffRel = null, statusRel = null;
        if (dirty)
        {
            var dir = Path.Combine(run.RunDir, "code-version");
            Directory.CreateDirectory(dir);

            var diffPath = Path.Combine(dir, "tracked.patch");
            File.WriteAllText(diffPath, RunGit(repoRoot, "diff HEAD") ?? string.Empty);
            diffRel = Path.GetRelativePath(run.RunDir, diffPath).Replace('\\', '/');

            var statusPath = Path.Combine(dir, "git-status.txt");
            File.WriteAllText(statusPath, status);
            statusRel = Path.GetRelativePath(run.RunDir, statusPath).Replace('\\', '/');
        }

        return new GitStampInfo(
            Available: true,
            Commit: commit,
            ShortCommit: commit.Length >= 7 ? commit[..7] : commit,
            Branch: branch,
            Dirty: dirty,
            ChangedFiles: changedFiles,
            TrackedDiffFile: diffRel,
            StatusFile: statusRel);
    }

    private static string? RunGit(string repoRoot, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", args)
            {
                WorkingDirectory = repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(psi);
            if (p is null) return null;

            var stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);
            return p.ExitCode == 0 ? stdout.TrimEnd('\n', '\r') : null;
        }
        catch
        {
            return null; // git fehlt / Timeout / kein Repo: Stempel ist best-effort, kein Run-Blocker
        }
    }
}
