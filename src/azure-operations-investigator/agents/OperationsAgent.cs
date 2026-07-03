using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

public sealed class OperationsAgent : IOperationsAgent
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly PromptExecutionSettings _executionSettings;
    private readonly ChatHistory _history = new();

    public OperationsAgent(
        Kernel kernel,
        IAiProvider aiProvider)
    {
        _kernel = kernel;
        _chatCompletionService =
            kernel.GetRequiredService<IChatCompletionService>();

        _executionSettings = aiProvider.CreateExecutionSettings();
    }

    public async Task<string> ChatAsync(string message)
    {
        _history.AddUserMessage(message);

        var response = string.Empty;

        await foreach (var chunk in _chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: _history,
            executionSettings: _executionSettings,
            kernel: _kernel))
        {
            response += chunk.Content;
        }

        if (!string.IsNullOrWhiteSpace(response))
        {
            _history.AddAssistantMessage(response);
        }

        return response;
    }
}