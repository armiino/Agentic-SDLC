using System.Collections.Concurrent;
using AgenticSdlc.Host.Configuration;
using AgenticSdlc.Host.FullWorkflow.Delta;
using AgenticSdlc.Host.FullWorkflow.Pipeline;
using AgenticSdlc.Host.FullWorkflow.Recipes;
using AgenticSdlc.Host.Run;
using Microsoft.Agents.AI.Workflows;
using Xunit;

namespace AgenticSdlc.Tests.FullWorkflow;

// R-75: runs the actual baseline executor and MAF output contract, without a model call.
public sealed class BaselineStageOutputRegressionTests
{
    private sealed class Downstream(ConcurrentBag<ProjectStateBuildRequest> hits)
        : Executor<ProjectStateBuildRequest>("DownstreamProbe")
    {
        public override ValueTask HandleAsync(ProjectStateBuildRequest input,
            IWorkflowContext context, CancellationToken ct = default)
        { hits.Add(input); return ValueTask.CompletedTask; }
    }

    [Fact]
    public async Task Missing_ledger_yields_terminal_failure_without_forwarding_to_delta()
    {
        var parent = new RunContext(RunId.New(), "test-r75");
        parent.EnsureFolders();
        var settings = HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());
        var baseline = new BaselineStageExecutor(settings, null, parent, Directory.GetCurrentDirectory());
        var hits = new ConcurrentBag<ProjectStateBuildRequest>();
        var builder = new WorkflowBuilder(baseline);
        builder.AddEdge(baseline, new Downstream(hits));
        builder.WithOutputFrom(baseline);
        var run = await InProcessExecution.Default.RunAsync(builder.Build(),
            new ConsumableLedgerOutput(Path.Combine(parent.RunDir, "missing-consumable.json"), 0),
            parent.RunId, CancellationToken.None);

        var errors = run.OutgoingEvents.Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e switch {
                ExecutorFailedEvent f => f.Data?.Message,
                WorkflowErrorEvent w => w.Exception?.Message,
                _ => e.ToString()
            }).ToList();
        Assert.True(errors.Count == 0, string.Join("\n", errors));
        var output = Assert.Single(run.OutgoingEvents.OfType<WorkflowOutputEvent>());
        Assert.Contains("02-baselines fehlgeschlagen (exit 2)", Assert.IsType<string>(output.Data));
        Assert.Empty(hits);
    }

    [Theory]
    [InlineData("HumanReview")]
    [InlineData("MaxIterationsReached")]
    public async Task Nonpass_report_yields_terminal_notice_without_forwarding_to_delta(string decision)
    {
        var parent = new RunContext(RunId.New(), "test-r75-review");
        parent.EnsureFolders();
        var recipeRun = new RunContext(RunId.New(), "test-r75-recipe");
        recipeRun.EnsureFolders();
        var artifactDir = Path.Combine(recipeRun.RunDir, "baselines", "requirements");
        Directory.CreateDirectory(artifactDir);
        await File.WriteAllTextAsync(Path.Combine(artifactDir, "artifact.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(artifactDir, BaselineFinalReport.FileName),
            System.Text.Json.JsonSerializer.Serialize(new { decision, pass = false }));
        var settings = HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());
        var baseline = new BaselineStageExecutor(settings, null, parent, Directory.GetCurrentDirectory(),
            (_, _, _, _, _, _) => Task.FromResult(new RecipeRunner.RecipeExecution(0, recipeRun)));
        var hits = new ConcurrentBag<ProjectStateBuildRequest>();
        var builder = new WorkflowBuilder(baseline);
        builder.AddEdge(baseline, new Downstream(hits));
        builder.WithOutputFrom(baseline);
        var run = await InProcessExecution.Default.RunAsync(builder.Build(),
            new ConsumableLedgerOutput("scripted-recipe-result", 1), parent.RunId, CancellationToken.None);
        var errors = run.OutgoingEvents.Where(e => e is ExecutorFailedEvent or WorkflowErrorEvent)
            .Select(e => e switch {
                ExecutorFailedEvent f => f.Data?.Message,
                WorkflowErrorEvent w => w.Exception?.Message,
                _ => e.ToString()
            }).ToList();
        Assert.True(errors.Count == 0, string.Join("\n", errors));
        var output = Assert.Single(run.OutgoingEvents.OfType<WorkflowOutputEvent>());
        var text = Assert.IsType<string>(output.Data);
        Assert.Contains($"Checker-Verdikt '{decision}'", text);
        Assert.Contains(Path.Combine(artifactDir, BaselineFinalReport.FileName), text);
        Assert.Empty(hits);
        Assert.False(File.Exists(Path.Combine(parent.RunDir, "02-baselines", "baseline-stage.json")));
        Assert.Contains("STAGE_BASELINES_NEEDS_HUMAN", await File.ReadAllTextAsync(parent.EventsPath));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Pass_or_legacy_missing_report_keeps_existing_forwarding_behavior(bool writePassReport)
    {
        var parent = new RunContext(RunId.New(), "test-r75-forward");
        parent.EnsureFolders();
        var recipeRun = new RunContext(RunId.New(), "test-r75-recipe");
        recipeRun.EnsureFolders();
        var artifactDir = Path.Combine(recipeRun.RunDir, "baselines", "requirements");
        Directory.CreateDirectory(artifactDir);
        var artifactPath = Path.Combine(artifactDir, "artifact.json");
        await File.WriteAllTextAsync(artifactPath, "{}");
        if (writePassReport)
            await File.WriteAllTextAsync(Path.Combine(artifactDir, BaselineFinalReport.FileName),
                "{\"decision\":\"Pass\",\"pass\":true}");
        var settings = HostSettings.FromRuntimeConfig(new RunConfig(), Directory.GetCurrentDirectory());
        var baseline = new BaselineStageExecutor(settings, null, parent, Directory.GetCurrentDirectory(),
            (_, _, _, _, _, _) => Task.FromResult(new RecipeRunner.RecipeExecution(0, recipeRun)));
        var hits = new ConcurrentBag<ProjectStateBuildRequest>();
        var builder = new WorkflowBuilder(baseline);
        builder.AddEdge(baseline, new Downstream(hits));
        builder.WithOutputFrom(baseline);
        var run = await InProcessExecution.Default.RunAsync(builder.Build(),
            new ConsumableLedgerOutput("scripted-recipe-result", 1), parent.RunId, CancellationToken.None);
        Assert.DoesNotContain(run.OutgoingEvents, e => e is ExecutorFailedEvent or WorkflowErrorEvent);
        Assert.Empty(run.OutgoingEvents.OfType<WorkflowOutputEvent>());
        Assert.Single(hits);
        var events = await File.ReadAllTextAsync(parent.EventsPath);
        Assert.Contains("STAGE_BASELINES_DONE", events);
        if (!writePassReport) Assert.Contains("STAGE_BASELINES_VERDICT_MISSING", events);
    }
}
