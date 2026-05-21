using System.Diagnostics;
using System.IO;
using AgenticSdlc.McpServer.Infrastructure;
using ModelContextProtocol.Server;

namespace AgenticSdlc.McpServer.Tools;

public class FileSystemTools
{
    [McpServerTool]
    public string FsRead(string path)
    {
        var full = RootPolicy.EnforceRead(path);
        var content = File.ReadAllText(full);

        var activity = Activity.Current;
        activity?.SetTag("fs.path", path);
        activity?.SetTag("fs.full_path", full);
        activity?.SetTag("fs.exists", File.Exists(full));

        return content;
    }

    [McpServerTool]
    public void FsWrite(string path, string content, string? intent = null, string? reason = null, string? evidence = null)
    {
        var full = RootPolicy.EnforceWrite(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);

        var activity = Activity.Current;
        activity?.SetTag("fs.path", path);
        activity?.SetTag("fs.full_path", full);
        // erweiterung: Die Begründungsfelder bleiben Teil des Tool-Contracts, blockieren Phase-1-Runs aber nicht mehr hart.
        activity?.SetTag("fs.write.intent", intent);
        activity?.SetTag("fs.write.reason", reason);
        activity?.SetTag("fs.write.evidence", evidence);
        activity?.SetTag("fs.write.rationale_complete", HasCompleteRationale(intent, reason, evidence));
    }

    private static bool HasCompleteRationale(string? intent, string? reason, string? evidence)
    {
        // Erklärungsquali messbar.. zumindest theoretisch je nach modelstärke..
        return !string.IsNullOrWhiteSpace(intent) &&
               !string.IsNullOrWhiteSpace(reason) &&
               !string.IsNullOrWhiteSpace(evidence);
    }

    [McpServerTool]
    public string[] FsList(string path)
    {
        var full = RootPolicy.EnforceRead(path);
        var entries = Directory.GetFileSystemEntries(full);

        return entries
            .Select(p => Path.GetRelativePath(RootPolicy.RepoRootPath, p).Replace('\\', '/'))
            .ToArray();
    }

    [McpServerTool]
    public bool FsExists(string path)
    {
        var full = RootPolicy.EnforceRead(path);
        return File.Exists(full) || Directory.Exists(full);
    }
}
