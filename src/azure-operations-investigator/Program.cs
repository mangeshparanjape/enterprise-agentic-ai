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

builder.Services.AddSingleton<IValidateOptions<OllamaOptions>, OllamaOptionsValidator>();
builder.Services.AddSingleton<IValidateOptions<GeminiOptions>, GeminiOptionsValidator>();

// App services
builder.Services.AddSingleton<IAlertService, MockAlertService>();

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

while (true)
{
    Console.Write("User > ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    var response = await agent.ChatAsync(userInput);

    Console.WriteLine($"Assistant > {response}");
}
