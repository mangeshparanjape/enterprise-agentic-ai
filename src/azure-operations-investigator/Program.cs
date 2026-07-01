using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
#pragma warning disable SKEXP0070

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
IAiProvider provider =new OllamaProvider();
provider.Configure(kernelBuilder);
kernelBuilder.Services.AddSingleton(provider);


kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Warning));
kernelBuilder.Services.AddSingleton<IAlertService, MockAlertService>();
kernelBuilder.Plugins.AddFromType<AzureAlertPlugin>("azure_alerts");

Kernel kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var history = new ChatHistory();
var executionSettings =
    provider.CreateExecutionSettings();
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

    try
    {
        await foreach (var chunk in chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: history,
            executionSettings: executionSettings,
            kernel: kernel))
        {
            Console.Write(chunk.Content);
            response += chunk.Content;
        }
    }
    catch (HttpOperationException ex)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Gemini request failed: {(int?)ex.StatusCode} {ex.StatusCode}");
        Console.Error.WriteLine(ex.Message);

        if (!string.IsNullOrWhiteSpace(ex.ResponseContent))
        {
            Console.Error.WriteLine("Response body:");
            Console.Error.WriteLine(ex.ResponseContent);
        }

        if (ex.Data["Url"] is { } requestUrl)
        {
            Console.Error.WriteLine($"Request URI: {requestUrl}");
        }

        if (ex.Data["Data"] is { } requestPayload)
        {
            Console.Error.WriteLine("Request payload:");
            Console.Error.WriteLine(requestPayload);
        }

        history.RemoveAt(history.Count - 1);
        continue;
    }

    Console.WriteLine();

    if (!string.IsNullOrWhiteSpace(response))
    {
        history.AddAssistantMessage(response);
    }
}
