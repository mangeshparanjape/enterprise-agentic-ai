using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0070

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(services =>
    services.AddConsole().SetMinimumLevel(LogLevel.Warning));

kernelBuilder.Services.AddSingleton<IAlertService, MockAlertService>();

IAiProvider aiProvider = AiProviderFactory.Create(configuration);

aiProvider.Configure(kernelBuilder);

kernelBuilder.Services.AddSingleton(aiProvider);

kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");

Kernel kernel = kernelBuilder.Build();

var chatCompletionService =
    kernel.GetRequiredService<IChatCompletionService>();

var executionSettings =
    aiProvider.CreateExecutionSettings();

var history = new ChatHistory();

while (true)
{
    Console.Write("User > ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
    {
        break;
    }

    history.AddUserMessage(userInput);

    Console.Write("Assistant > ");

    var response = string.Empty;

    await foreach (var chunk in chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatHistory: history,
        executionSettings: executionSettings,
        kernel: kernel))
    {
        Console.Write(chunk.Content);
        response += chunk.Content;
    }

    Console.WriteLine();

    if (!string.IsNullOrWhiteSpace(response))
    {
        history.AddAssistantMessage(response);
    }
}