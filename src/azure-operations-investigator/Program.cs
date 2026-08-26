using EnterpriseAiPortfolio.Agents;
using EnterpriseAiPortfolio.Ai;
using EnterpriseAiPortfolio.Orchestration;
using EnterpriseAiPortfolio.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#pragma warning disable SKEXP0070

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Options
builder.Services
    .AddOptions<OllamaOptions>()
    .Bind(builder.Configuration.GetSection(OllamaOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<GeminiOptions>()
    .Bind(builder.Configuration.GetSection(GeminiOptions.SectionName))
    .ValidateOnStart();

builder.Services
    .AddOptions<OperationsAgentOptions>()
    .Bind(builder.Configuration.GetSection(OperationsAgentOptions.SectionName))
    .Validate(
        options => options.MaxHistoryTurns >= 0,
        "Agent:Operations:MaxHistoryTurns must be zero or greater.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ToolAuthorizationOptions>()
    .Bind(builder.Configuration.GetSection(ToolAuthorizationOptions.SectionName))
    .Validate(
        options => options.AllowedPlugins.Length > 0,
        "Ai:ToolAuthorization:AllowedPlugins must contain at least one plugin.")
    .Validate(
        options => options.AllowedPlugins.All(plugin => !string.IsNullOrWhiteSpace(plugin)),
        "Ai:ToolAuthorization:AllowedPlugins cannot contain blank plugin names.")
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<OllamaOptions>, OllamaOptionsValidator>();
builder.Services.AddSingleton<IValidateOptions<GeminiOptions>, GeminiOptionsValidator>();

// App services
builder.Services.AddSingleton<IAlertService, MockAlertService>();
builder.Services.AddSingleton<IKqlQueryService, MockKqlQueryService>();
builder.Services.AddSingleton<IRunbookSearchService, MockRunbookSearchService>();
builder.Services.AddSingleton<IOperationApprovalService, InMemoryOperationApprovalService>();

// AI providers
builder.Services.AddSingleton<OllamaProvider>();
builder.Services.AddSingleton<GeminiProvider>();
builder.Services.AddSingleton<IAiProviderFactory, AiProviderFactory>();

// Kernel factory
builder.Services.AddSingleton<IAiKernelFactory, AiKernelFactory>();

// Runtime
builder.Services.AddSingleton<IAiRuntime, SemanticKernelRuntime>();

// Orchestration
builder.Services.AddSingleton<IAiRequestOrchestrator, AiRequestOrchestrator>();

// Agent
builder.Services.AddSingleton<IOperationsAgent, OperationsAgent>();

using var host = builder.Build();

var agent = host.Services.GetRequiredService<IOperationsAgent>();
var approvalService = host.Services.GetRequiredService<IOperationApprovalService>();

Console.WriteLine("Commands: /pending, /approve <id>, /reject <id>. Approval commands execute outside the AI tool path.");

while (true)
{
    Console.Write("User > ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    if (string.Equals(userInput, "/pending", StringComparison.OrdinalIgnoreCase))
    {
        var pending = approvalService.GetPending();
        if (pending.Count == 0)
        {
            Console.WriteLine("System > No pending operation approvals.");
            continue;
        }

        foreach (var request in pending)
        {
            Console.WriteLine($"System > {request.Id} | {request.Operation} | {request.ResourceName} | {request.Reason}");
        }

        continue;
    }

    if (userInput.StartsWith("/approve ", StringComparison.OrdinalIgnoreCase))
    {
        ResolveApproval(userInput[9..], approve: true);
        continue;
    }

    if (userInput.StartsWith("/reject ", StringComparison.OrdinalIgnoreCase))
    {
        ResolveApproval(userInput[8..], approve: false);
        continue;
    }

    var response = await agent.ChatAsync(userInput);

    Console.WriteLine($"Assistant > {response}");
}

void ResolveApproval(string approvalId, bool approve)
{
    try
    {
        var result = approve
            ? approvalService.ApproveAndExecute(approvalId)
            : approvalService.Reject(approvalId);

        Console.WriteLine($"System > {result.Status}: {result.Result}");
    }
    catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
    {
        Console.WriteLine($"System > {ex.Message}");
    }
}
