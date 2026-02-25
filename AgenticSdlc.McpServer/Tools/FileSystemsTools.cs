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
        return File.ReadAllText(full);
    }

    [McpServerTool]
    public void FsWrite(string path, string content)
    {
        var full = RootPolicy.EnforceWrite(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
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