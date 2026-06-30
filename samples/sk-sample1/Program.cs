using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

#pragma warning disable SKEXP0070

var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY")
    ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.Error.WriteLine("Set GOOGLE_API_KEY or GEMINI_API_KEY before running this sample.");
    return;
}

var modelId = Environment.GetEnvironmentVariable("GOOGLE_GEMINI_MODEL") ?? "gemini-2.5-flash";

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: modelId,
    apiKey: apiKey,
    apiVersion: GoogleAIVersion.V1_Beta,
    serviceId: "gemini"
);

kernelBuilder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Warning));

Kernel kernel = kernelBuilder.Build();
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

kernel.Plugins.AddFromType<LightsPlugin>("Lights");

var history = new ChatHistory();
var executionSettings = new GeminiPromptExecutionSettings
{
    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions
};

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
