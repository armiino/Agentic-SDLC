using System;
using System.IO;

namespace AgenticSdlc.McpServer.Infrastructure;

public static class RootPolicy
{
    public static string RepoRootPath => RepoRoot;

    private const long MaxReadBytes = 5 * 1024 * 1024; //erst testweise auf 5mb begrenzt

    private static readonly string RepoRoot = FindRepoRoot();
    

    private static readonly string[] ReadRoots =
    {
        Path.Combine(RepoRoot, "input"),
        Path.Combine(RepoRoot, "docs"),
        Path.Combine(RepoRoot, "runs")
    };

    private static readonly string[] WriteRoots =
    {
        Path.Combine(RepoRoot, "docs"),
        Path.Combine(RepoRoot, "runs")
    };

    private static string FindRepoRoot()
    {
        // nötig um das passende repo zu finden
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        for (int i = 0; i < 10 && dir is not null; i++)
        {
            var hasGit = Directory.Exists(Path.Combine(dir.FullName, ".git"));
            var hasSlnx = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.slnx"));
            var hasSln = File.Exists(Path.Combine(dir.FullName, "Agentic-SDLC.sln"));

            if (hasGit || hasSlnx || hasSln)
                return dir.FullName;

            dir = dir.Parent;
        }

        // Fallback
        return Directory.GetCurrentDirectory();
    }

    //absicherung für relative pfade -> absolut blocken.
    private static string EnforceRelative(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is required.");

        if (Path.IsPathRooted(path))
            throw new UnauthorizedAccessException($"Absolute paths are not allowed: {path}");

        return path.Replace('\\', Path.DirectorySeparatorChar);
    }

    public static string EnforceRead(string path)
    {
        path = EnforceRelative(path);

        var full = Path.GetFullPath(Path.Combine(RepoRoot, path));
        if (!IsWithinRoots(full, ReadRoots))
            throw new UnauthorizedAccessException($"Read denied: {full}");

        var info = new FileInfo(full);
        if (info.Exists && info.Length > MaxReadBytes)
            throw new UnauthorizedAccessException($"Read denied (file too large): {full}");

        return full;
    }

    public static string EnforceWrite(string path)
    {
        path = EnforceRelative(path);

        var full = Path.GetFullPath(Path.Combine(RepoRoot, path));
        if (!IsWithinRoots(full, WriteRoots))
            throw new UnauthorizedAccessException($"Write denied: {full}");

        return full;
    }

    private static bool IsWithinRoots(string path, string[] roots)
    {
        foreach (var root in roots)
        {
            var rootFull = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
            if (path.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}