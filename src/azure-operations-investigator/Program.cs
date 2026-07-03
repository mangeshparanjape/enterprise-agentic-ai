using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// App services
builder.Services.AddSingleton<IAlertService, MockAlertService>();

// AI provider
builder.Services.AddSingleton<IAiProvider>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return AiProviderFactory.Create(configuration);
});

// Build Kernel through DI
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    var aiProvider = sp.GetRequiredService<IAiProvider>();

    aiProvider.Configure(kernelBuilder);

    kernelBuilder.Services.AddSingleton(sp.GetRequiredService<IAlertService>());

    kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");

    return kernelBuilder.Build();
});

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