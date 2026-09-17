namespace AgenticSdlc.Host.FullWorkflow.ArmF;

public static class ArmFCommands
{
    public static void Register(IDictionary<string, AgenticSdlc.Host.CommandHandler> map)
        => map["arm-f"] = (args, settings, repoRoot) => ArmFRunner.RunAsync(args, settings, repoRoot);
}
