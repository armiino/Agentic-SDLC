using AgenticSdlc.Host.Run;
using System.Diagnostics;
using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubIssueSnapshotRunner
{
    private static readonly JsonSerializerOptions Json = JsonFiles.Json; // R3a: geteilte Optionen

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 2)
        {
            Usage();
            return 2;
        }

        return args[1].ToLowerInvariant() switch
        {
            "issues" => await RunIssuesAsync(args, repoRoot).ConfigureAwait(false),
            "close-open" => await RunCloseOpenAsync(args, repoRoot).ConfigureAwait(false),
            _ => UnknownMode(args[1])
        };
    }

    private static async Task<int> RunCloseOpenAsync(string[] args, string repoRoot)
    {
        var options = ParseCloseOptions(args, startIndex: 2);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Usage();
            return 2;
        }

        if (!options.ConfirmClose)
        {
            Console.Error.WriteLine("[github-snapshot] close-open braucht --confirm-close.");
            return 2;
        }

        if (string.IsNullOrWhiteSpace(options.Repository))
        {
            Console.Error.WriteLine("[github-snapshot] --repo owner/name fehlt.");
            return 2;
        }

        var token = Environment.GetEnvironmentVariable(options.TokenEnv);
        if (string.IsNullOrWhiteSpace(token) && string.Equals(options.TokenEnv, "GITHUB_TEST_TOKEN", StringComparison.Ordinal))
            token = Environment.GetEnvironmentVariable("githubtoken");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Error.WriteLine($"[github-snapshot] Token fehlt. Setze Umgebungsvariable {options.TokenEnv}. Token wird nicht persistiert.");
            return 2;
        }

        IReadOnlyList<GithubIssueSnapshot> issues;
        try
        {
            issues = await LoadWithGitHubApiAsync(options.Repository, options.Limit, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-snapshot] Issues konnten vor close-open nicht gelesen werden: {ex.Message}");
            return 2;
        }

        var openIssues = issues
            .Where(i => i.State.Equals("open", StringComparison.OrdinalIgnoreCase))
            .OrderBy(i => i.IssueNumber)
            .ToArray();

        var results = new List<GithubCloseOpenResult>();
        using var http = new HttpClient();
        var client = new GithubRestIssueClient(http, token, "Agentic-SDLC");
        foreach (var issue in openIssues)
        {
            try
            {
                var result = await client.CloseIssueAsync(options.Repository, issue.IssueNumber, CancellationToken.None).ConfigureAwait(false);
                results.Add(new GithubCloseOpenResult(
                    IssueNumber: issue.IssueNumber,
                    Title: issue.Title,
                    Status: "closed",
                    ResultState: result.State,
                    ResultUrl: result.IssueUrl,
                    Message: null));
            }
            catch (Exception ex)
            {
                results.Add(new GithubCloseOpenResult(
                    IssueNumber: issue.IssueNumber,
                    Title: issue.Title,
                    Status: "failed",
                    ResultState: null,
                    ResultUrl: null,
                    Message: ex.Message));
            }
        }

        var run = new RunContext(RunId.New(), "github-snapshot");
        run.EnsureFolders();
        var outDir = options.Output is null ? run.OutputDir("close-open") : ResolvePath(repoRoot, options.Output);
        Directory.CreateDirectory(outDir);
        var summary = new
        {
            schemaVersion = 1,
            repository = options.Repository,
            openBefore = openIssues.Length,
            closed = results.Count(r => r.Status.Equals("closed", StringComparison.OrdinalIgnoreCase)),
            failed = results.Count(r => r.Status.Equals("failed", StringComparison.OrdinalIgnoreCase)),
            timestampUtc = DateTime.UtcNow,
            results
        };

        await File.WriteAllTextAsync(Path.Combine(outDir, "github-close-open-summary.json"), JsonSerializer.Serialize(summary, Json)).ConfigureAwait(false);
        run.WriteConfig(new
        {
            command = "github-snapshot close-open",
            repository = options.Repository,
            limit = options.Limit,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            timestampUtc = DateTime.UtcNow
        });
        run.AppendEvent(new { type = "GITHUB_OPEN_ISSUES_CLOSED", runId = run.RunId, repository = options.Repository, openBefore = openIssues.Length, timestampUtc = DateTime.UtcNow });

        Console.WriteLine($"[github-snapshot] close-open repository={options.Repository} openBefore={openIssues.Length} closed={summary.closed} failed={summary.failed}");
        Console.WriteLine($"[github-snapshot] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return summary.failed == 0 ? 0 : 1;
    }

    private static async Task<int> RunIssuesAsync(string[] args, string repoRoot)
    {
        var options = ParseOptions(args, startIndex: 2);
        if (options.Error is not null)
        {
            Console.Error.WriteLine(options.Error);
            Usage();
            return 2;
        }

        IReadOnlyList<GithubIssueSnapshot> issues;
        string provider;
        try
        {
            if (!string.IsNullOrWhiteSpace(options.FilePath))
            {
                provider = "file";
                issues = await LoadIssueFileAsync(ResolvePath(repoRoot, options.FilePath)).ConfigureAwait(false);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(options.Repository))
                {
                    Console.Error.WriteLine("[github-snapshot] --repo owner/name fehlt.");
                    return 2;
                }

                provider = options.Provider;
                issues = provider.Equals("gh", StringComparison.OrdinalIgnoreCase)
                    ? await LoadWithGhAsync(options.Repository, options.Limit).ConfigureAwait(false)
                    : await LoadWithGitHubApiAsync(options.Repository, options.Limit).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-snapshot] Snapshot fehlgeschlagen: {ex.Message}");
            return 2;
        }

        var run = new RunContext(RunId.New(), "github-snapshot");
        run.EnsureFolders();
        var outDir = options.Output is null ? run.OutputDir("issues") : ResolvePath(repoRoot, options.Output);
        Directory.CreateDirectory(outDir);
        var snapshotPath = Path.Combine(outDir, "github-issues-snapshot.json");
        var summaryPath = Path.Combine(outDir, "github-issues-snapshot-summary.json");
        var summary = new
        {
            schemaVersion = 1,
            provider,
            repository = options.Repository,
            sourceFile = options.FilePath,
            issues = issues.Count,
            open = issues.Count(i => i.State.Equals("open", StringComparison.OrdinalIgnoreCase)),
            closed = issues.Count(i => i.State.Equals("closed", StringComparison.OrdinalIgnoreCase)),
            timestampUtc = DateTime.UtcNow
        };

        await File.WriteAllTextAsync(snapshotPath, JsonSerializer.Serialize(issues, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(summaryPath, JsonSerializer.Serialize(summary, Json)).ConfigureAwait(false);
        run.WriteConfig(new
        {
            command = "github-snapshot issues",
            provider,
            repository = options.Repository,
            sourceFile = options.FilePath,
            requestedProvider = options.Provider,
            limit = options.Limit,
            outDir = Path.GetRelativePath(repoRoot, outDir),
            timestampUtc = DateTime.UtcNow
        });
        run.AppendEvent(new { type = "GITHUB_ISSUE_SNAPSHOT_WRITTEN", runId = run.RunId, provider, issues = issues.Count, timestampUtc = DateTime.UtcNow });

        Console.WriteLine($"[github-snapshot] provider={provider} issues={issues.Count} open={summary.open} closed={summary.closed}");
        Console.WriteLine($"[github-snapshot] -> {Path.GetRelativePath(repoRoot, snapshotPath)}");
        Console.WriteLine("[github-snapshot] Danach: github-reconciliation agent ... --issues <snapshot>");
        return 0;
    }

    private static async Task<IReadOnlyList<GithubIssueSnapshot>> LoadIssueFileAsync(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Issue-Snapshot-Datei nicht gefunden.", path);

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(path).ConfigureAwait(false));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Issue-Snapshot-Datei muss ein JSON-Array sein.");

        var issues = new List<GithubIssueSnapshot>();
        foreach (var item in document.RootElement.EnumerateArray())
            issues.Add(ParseIssue(item));
        return issues;
    }

    private static async Task<IReadOnlyList<GithubIssueSnapshot>> LoadWithGhAsync(string repository, int limit)
    {
        var args = new[]
        {
            "issue", "list",
            "--repo", repository,
            "--state", "all",
            "--limit", limit.ToString(),
            "--json", "number,url,title,body,state,labels,milestone,updatedAt"
        };

        var start = new ProcessStartInfo
        {
            FileName = "gh",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var arg in args)
            start.ArgumentList.Add(arg);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("gh konnte nicht gestartet werden.");
        var stdout = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var stderr = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);
        await process.WaitForExitAsync().ConfigureAwait(false);
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"gh issue list fehlgeschlagen ({process.ExitCode}): {stderr.Trim()}");

        using var document = JsonDocument.Parse(stdout);
        var issues = new List<GithubIssueSnapshot>();
        foreach (var item in document.RootElement.EnumerateArray())
            issues.Add(ParseIssue(item));
        return issues;
    }

    private static async Task<IReadOnlyList<GithubIssueSnapshot>> LoadWithGitHubApiAsync(string repository, int limit, string? explicitToken = null)
    {
        if (repository.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length != 2)
            throw new InvalidOperationException("--repo muss die Form owner/name haben.");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("AgenticSdlc/1.0");
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        var token = explicitToken ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
            token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var issues = new List<GithubIssueSnapshot>();
        var page = 1;
        while (issues.Count < limit)
        {
            var remaining = limit - issues.Count;
            var perPage = Math.Clamp(remaining, 1, 100);
            var url = $"https://api.github.com/repos/{repository}/issues?state=all&per_page={perPage}&page={page}";
            using var response = await client.GetAsync(url).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"GitHub API fehlgeschlagen ({(int)response.StatusCode}): {body}");

            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new InvalidOperationException("GitHub API lieferte kein Issue-Array.");

            var pageIssues = 0;
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("pull_request", out _))
                    continue;
                issues.Add(ParseIssue(item));
                pageIssues++;
                if (issues.Count >= limit)
                    break;
            }

            if (pageIssues == 0 || document.RootElement.GetArrayLength() < perPage)
                break;
            page++;
        }

        return issues;
    }

    private static GithubIssueSnapshot ParseIssue(JsonElement item)
    {
        var number = ReadInt(item, "issueNumber") ?? ReadInt(item, "number");
        if (number is null)
            throw new InvalidOperationException("Issue ohne issueNumber/number gefunden.");

        var labels = ReadLabels(item);
        return new GithubIssueSnapshot(
            IssueNumber: number.Value,
            Url: ReadString(item, "html_url") ?? ReadString(item, "url") ?? ReadString(item, "issueUrl"),
            Title: ReadString(item, "title") ?? "",
            Body: ReadString(item, "body"),
            State: NormalizeState(ReadString(item, "state")),
            Labels: labels,
            Milestone: ReadMilestone(item),
            UpdatedUtc: ReadDateTime(item, "updatedUtc") ?? ReadDateTime(item, "updatedAt") ?? ReadDateTime(item, "updated_at"));
    }

    private static IReadOnlyList<string> ReadLabels(JsonElement item)
    {
        if (!item.TryGetProperty("labels", out var labels) || labels.ValueKind != JsonValueKind.Array)
            return [];

        var values = new List<string>();
        foreach (var label in labels.EnumerateArray())
        {
            var value = label.ValueKind switch
            {
                JsonValueKind.String => label.GetString(),
                JsonValueKind.Object => ReadString(label, "name"),
                _ => null
            };
            if (!string.IsNullOrWhiteSpace(value))
                values.Add(value.Trim());
        }
        return values.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }

    private static string? ReadMilestone(JsonElement item)
    {
        if (!item.TryGetProperty("milestone", out var milestone) || milestone.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            return null;
        if (milestone.ValueKind == JsonValueKind.String)
            return milestone.GetString();
        if (milestone.ValueKind == JsonValueKind.Object)
            return ReadString(milestone, "title") ?? ReadString(milestone, "name");
        return null;
    }

    private static string NormalizeState(string? state)
    {
        var normalized = (state ?? "unknown").Trim().ToLowerInvariant();
        return normalized switch
        {
            "open" => "open",
            "closed" => "closed",
            _ => normalized
        };
    }

    private static string? ReadString(JsonElement item, string property)
        => item.TryGetProperty(property, out var value) && value.ValueKind != JsonValueKind.Null
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    private static int? ReadInt(JsonElement item, string property)
        => item.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;

    private static DateTime? ReadDateTime(JsonElement item, string property)
    {
        var value = ReadString(item, property);
        return DateTime.TryParse(value, out var parsed) ? parsed.ToUniversalTime() : null;
    }

    private static GithubSnapshotCliOptions ParseOptions(string[] args, int startIndex)
    {
        string? repository = null;
        string? filePath = null;
        string? output = null;
        var provider = "api";
        var limit = 100;

        for (var i = startIndex; i < args.Length; i++)
        {
            var arg = args[i];
            if (TryReadOption(arg, "--repo", args, ref i, out var repoValue))
            {
                repository = repoValue;
                continue;
            }
            if (TryReadOption(arg, "--file", args, ref i, out var fileValue))
            {
                filePath = fileValue;
                continue;
            }
            if (TryReadOption(arg, "--out", args, ref i, out var outValue))
            {
                output = outValue;
                continue;
            }
            if (TryReadOption(arg, "--provider", args, ref i, out var providerValue))
            {
                provider = providerValue?.Trim().ToLowerInvariant() ?? "api";
                if (provider is not ("api" or "gh"))
                    return new GithubSnapshotCliOptions(null, null, null, "api", 0, "[github-snapshot] --provider erlaubt nur api oder gh.");
                continue;
            }
            if (TryReadOption(arg, "--limit", args, ref i, out var limitValue))
            {
                if (!int.TryParse(limitValue, out limit) || limit <= 0)
                    return new GithubSnapshotCliOptions(null, null, null, "api", 0, "[github-snapshot] --limit muss eine positive Zahl sein.");
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
                return new GithubSnapshotCliOptions(null, null, null, "api", 0, $"[github-snapshot] unbekanntes Argument: {arg}");

            if (repository is null) repository = arg;
            else return new GithubSnapshotCliOptions(null, null, null, "api", 0, $"[github-snapshot] unerwartetes Argument: {arg}");
        }

        return new GithubSnapshotCliOptions(repository, filePath, output, provider, limit, null);
    }

    private static GithubCloseOpenCliOptions ParseCloseOptions(string[] args, int startIndex)
    {
        string? repository = null;
        string? output = null;
        var limit = 100;
        var tokenEnv = "GITHUB_TEST_TOKEN";
        var confirmClose = false;

        for (var i = startIndex; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg.Equals("--confirm-close", StringComparison.OrdinalIgnoreCase))
            {
                confirmClose = true;
                continue;
            }
            if (TryReadOption(arg, "--repo", args, ref i, out var repoValue))
            {
                repository = repoValue;
                continue;
            }
            if (TryReadOption(arg, "--out", args, ref i, out var outValue))
            {
                output = outValue;
                continue;
            }
            if (TryReadOption(arg, "--token-env", args, ref i, out var tokenEnvValue))
            {
                tokenEnv = string.IsNullOrWhiteSpace(tokenEnvValue) ? "GITHUB_TEST_TOKEN" : tokenEnvValue;
                continue;
            }
            if (TryReadOption(arg, "--limit", args, ref i, out var limitValue))
            {
                if (!int.TryParse(limitValue, out limit) || limit <= 0)
                    return new GithubCloseOpenCliOptions(null, null, 0, "GITHUB_TEST_TOKEN", false, "[github-snapshot] --limit muss eine positive Zahl sein.");
                continue;
            }
            if (arg.StartsWith("--", StringComparison.Ordinal))
                return new GithubCloseOpenCliOptions(null, null, 0, "GITHUB_TEST_TOKEN", false, $"[github-snapshot] unbekanntes Argument: {arg}");

            if (repository is null) repository = arg;
            else return new GithubCloseOpenCliOptions(null, null, 0, "GITHUB_TEST_TOKEN", false, $"[github-snapshot] unerwartetes Argument: {arg}");
        }

        return new GithubCloseOpenCliOptions(repository, output, limit, tokenEnv, confirmClose, null);
    }

    private static bool TryReadOption(string arg, string name, string[] args, ref int index, out string? value)
    {
        value = null;
        if (!string.Equals(arg, name, StringComparison.OrdinalIgnoreCase)) return false;
        if (index + 1 >= args.Length) throw new ArgumentException($"Option {name} braucht einen Wert.");
        value = args[++index];
        return true;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static int UnknownMode(string mode)
    {
        Console.Error.WriteLine($"[github-snapshot] unbekannter mode: {mode}");
        Usage();
        return 2;
    }

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-snapshot issues <owner/name|--repo owner/name> [--provider api|gh] [--limit 100] [--out <dir>]");
        Console.Error.WriteLine("       github-snapshot issues --file <issues.json> [--repo owner/name] [--out <dir>]");
        Console.Error.WriteLine("       github-snapshot close-open <owner/name|--repo owner/name> --confirm-close [--token-env GITHUB_TEST_TOKEN] [--limit 100] [--out <dir>]");
    }

    private sealed record GithubSnapshotCliOptions(
        string? Repository,
        string? FilePath,
        string? Output,
        string Provider,
        int Limit,
        string? Error);

    private sealed record GithubCloseOpenCliOptions(
        string? Repository,
        string? Output,
        int Limit,
        string TokenEnv,
        bool ConfirmClose,
        string? Error);

    private sealed record GithubCloseOpenResult(
        int IssueNumber,
        string Title,
        string Status,
        string? ResultState,
        string? ResultUrl,
        string? Message);
}
