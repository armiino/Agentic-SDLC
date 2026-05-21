using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgenticSdlc.McpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<FileSystemTools>();
   // .WithTools<ApprovalTools>();   //aktuell auskommentiert weil DoD von Host..

await builder.Build().RunAsync();