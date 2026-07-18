using System.Text.Json;

namespace AgenticSdlc.Host.Phases.Phase2.EvidenzAgent.L4;

public static class GithubWriteDryRunRunner
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static async Task<int> RunAsync(string[] args, string repoRoot)
    {
        if (args.Length < 3)
        {
            Usage();
            return 2;
        }

        if (args[1].Equals("execute", StringComparison.OrdinalIgnoreCase))
            return await RunExecuteAsync(args, repoRoot).ConfigureAwait(false);
        if (!args[1].Equals("dry-run", StringComparison.OrdinalIgnoreCase))
        {
            Usage();
            return 2;
        }

        var acceptedPlanPath = ResolveAcceptedPlanPath(repoRoot, args[2]);
        if (acceptedPlanPath is null)
        {
            Console.Error.WriteLine($"[github-write] accepted-github-action-plan nicht gefunden: {args[2]}");
            return 2;
        }

        var outDir = ResolveOutputDir(repoRoot, args, acceptedPlanPath, "github-write-dry-run");
        Directory.CreateDirectory(outDir);
        var acceptedPlan = await LoadAsync<GithubActionPlanDocument>(acceptedPlanPath).ConfigureAwait(false);
        var dryRun = GithubWriteDryRunFactory.Create(acceptedPlan);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-write-dry-run.json"), JsonSerializer.Serialize(dryRun, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-write-dry-run-summary.json"), JsonSerializer.Serialize(dryRun.Summary, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-write] dry-run actions={dryRun.Summary.Actions} create={dryRun.Summary.Create} update={dryRun.Summary.Update} link={dryRun.Summary.Link} reopen={dryRun.Summary.Reopen} noChange={dryRun.Summary.NoChange} needsReview={dryRun.Summary.NeedsReview} blocked={dryRun.Summary.Blocked}");
        Console.WriteLine($"[github-write] readyForExecute={dryRun.ReadyForExecute}");
        Console.WriteLine($"[github-write] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return dryRun.ReadyForExecute ? 0 : 1;
    }

    private static async Task<int> RunExecuteAsync(string[] args, string repoRoot)
    {
        if (!args.Contains("--confirm-write", StringComparer.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("[github-write] Echte GitHub-Writes brauchen --confirm-write.");
            return 2;
        }

        var acceptedPlanPath = ResolveAcceptedPlanPath(repoRoot, args[2]);
        if (acceptedPlanPath is null)
        {
            Console.Error.WriteLine($"[github-write] accepted-github-action-plan nicht gefunden: {args[2]}");
            return 2;
        }

        var acceptedPlan = await LoadAsync<GithubActionPlanDocument>(acceptedPlanPath).ConfigureAwait(false);
        var repository = ParseOption(args, "--repo") ?? acceptedPlan.Repository;
        if (string.IsNullOrWhiteSpace(repository))
        {
            Console.Error.WriteLine("[github-write] Repository fehlt. Nutze --repo owner/name oder setze repository im accepted plan.");
            return 2;
        }
        if (!repository.Contains('/', StringComparison.Ordinal) || repository.Split('/').Length != 2)
        {
            Console.Error.WriteLine("[github-write] Repository muss owner/name sein.");
            return 2;
        }

        var tokenEnv = ParseOption(args, "--token-env") ?? "GITHUB_TEST_TOKEN";
        var token = Environment.GetEnvironmentVariable(tokenEnv);
        if (string.IsNullOrWhiteSpace(token) && string.Equals(tokenEnv, "GITHUB_TEST_TOKEN", StringComparison.Ordinal))
        {
            var legacyToken = Environment.GetEnvironmentVariable("githubtoken");
            if (!string.IsNullOrWhiteSpace(legacyToken))
            {
                token = legacyToken;
                tokenEnv = "githubtoken";
            }
        }
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Error.WriteLine($"[github-write] Token fehlt. Setze Umgebungsvariable {tokenEnv}. Token wird nicht persistiert.");
            return 2;
        }

        var outDir = ResolveOutputDir(repoRoot, args, acceptedPlanPath, "github-write-execution");
        Directory.CreateDirectory(outDir);
        var executionPath = Path.Combine(outDir, "github-write-execution.json");
        if (File.Exists(executionPath) && !args.Contains("--allow-repeat", StringComparer.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine($"[github-write] Execution-Artefakt existiert bereits: {Path.GetRelativePath(repoRoot, executionPath)}. Nutze --allow-repeat nur bewusst.");
            return 2;
        }

        var dryRun = GithubWriteDryRunFactory.Create(acceptedPlan);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-write-preflight-dry-run.json"), JsonSerializer.Serialize(dryRun, Json)).ConfigureAwait(false);
        if (!dryRun.ReadyForExecute)
        {
            Console.Error.WriteLine($"[github-write] Preflight blockiert: blocked={dryRun.Summary.Blocked}. Kein GitHub-Write ausgefuehrt.");
            await File.WriteAllTextAsync(Path.Combine(outDir, "github-write-execution-summary.json"), JsonSerializer.Serialize(new
            {
                executed = false,
                reason = "preflight_blocked",
                dryRun.Summary,
                timestampUtc = DateTime.UtcNow
            }, Json)).ConfigureAwait(false);
            return 1;
        }

        Console.WriteLine($"[github-write] EXECUTE repository={repository} actions={acceptedPlan.Actions.Count}");
        Console.WriteLine("[github-write] Token wird nur aus der Umgebung gelesen und nicht gespeichert.");

        using var http = new HttpClient();
        var client = new GithubRestIssueClient(http, token, "Agentic-SDLC");
        GithubWriteExecutionDocument result;
        try
        {
            result = await GithubWriteExecutor.ExecuteAsync(
                acceptedPlan,
                client,
                new GithubWriteExecuteOptions(repository, token, "Agentic-SDLC"),
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[github-write] Execute fehlgeschlagen: {ex.Message}");
            return 2;
        }

        await File.WriteAllTextAsync(executionPath, JsonSerializer.Serialize(result, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-write-execution-summary.json"), JsonSerializer.Serialize(result.Summary, Json)).ConfigureAwait(false);
        await File.WriteAllTextAsync(Path.Combine(outDir, "github-mappings-created.json"), JsonSerializer.Serialize(result.Mappings, Json)).ConfigureAwait(false);

        Console.WriteLine($"[github-write] executed success={result.Success} created={result.Summary.Created} updated={result.Summary.Updated} linked={result.Summary.Linked} reopened={result.Summary.Reopened} skipped={result.Summary.Skipped} failed={result.Summary.Failed}");
        Console.WriteLine($"[github-write] mappings={result.Mappings.Count}");
        Console.WriteLine($"[github-write] -> {Path.GetRelativePath(repoRoot, outDir)}");
        return result.Success ? 0 : 1;
    }

    private static async Task<T> LoadAsync<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, Json)
               ?? throw new InvalidOperationException($"Datei konnte nicht gelesen werden: {path}");
    }

    private static string? ResolveAcceptedPlanPath(string repoRoot, string token)
    {
        var full = ResolvePath(repoRoot, token);
        if (File.Exists(full) && Path.GetFileName(full).Equals("accepted-github-action-plan.json", StringComparison.OrdinalIgnoreCase))
            return full;
        if (Directory.Exists(full))
        {
            var direct = Path.Combine(full, "accepted-github-action-plan.json");
            if (File.Exists(direct)) return direct;
            var applied = Path.Combine(full, "applied", "accepted-github-action-plan.json");
            if (File.Exists(applied)) return applied;
            var planApplied = Path.Combine(full, "plan", "applied", "accepted-github-action-plan.json");
            if (File.Exists(planApplied)) return planApplied;
        }

        var root = Path.Combine(repoRoot, "runs", "github-reconciliation");
        if (!Directory.Exists(root)) return null;
        foreach (var dir in Directory.EnumerateDirectories(root).Where(d => Path.GetFileName(d).Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            var candidate = Path.Combine(dir, "plan", "applied", "accepted-github-action-plan.json");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static string ResolveOutputDir(string repoRoot, string[] args, string acceptedPlanPath, string defaultDirectoryName)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].Equals("--out", StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) throw new ArgumentException("--out braucht einen Wert.");
            return ResolvePath(repoRoot, args[i + 1]);
        }
        return Path.Combine(Path.GetDirectoryName(acceptedPlanPath) ?? repoRoot, defaultDirectoryName);
    }

    private static string? ParseOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            if (i + 1 >= args.Length) throw new ArgumentException($"{name} braucht einen Wert.");
            return args[i + 1];
        }
        return null;
    }

    private static string ResolvePath(string repoRoot, string path)
        => Path.IsPathRooted(path) ? path : Path.Combine(repoRoot, path);

    private static void Usage()
    {
        Console.Error.WriteLine("Usage: github-write dry-run <accepted-github-action-plan.json|github-reconciliation-runId> [--out <dir>]");
        Console.Error.WriteLine("       github-write execute <accepted-github-action-plan.json|github-reconciliation-runId> --confirm-write [--repo owner/name] [--token-env GITHUB_TEST_TOKEN] [--out <dir>] [--allow-repeat]");
    }
}
