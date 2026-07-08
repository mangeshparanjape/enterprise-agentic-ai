using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace EnterpriseAiPortfolio.Ai;

#pragma warning disable SKEXP0070

public sealed class SemanticKernelRuntime : IAiRuntime
{
    private readonly IAiKernelFactory _kernelFactory;

    public SemanticKernelRuntime(IAiKernelFactory kernelFactory)
    {
        _kernelFactory = kernelFactory;
    }

    public async Task<AiProviderResponse> ExecuteAsync(
        IAiProvider provider,
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var kernel = _kernelFactory.CreateKernel(provider);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();

        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            chatHistory.AddSystemMessage(request.SystemMessage);
        }

        foreach (var message in request.ConversationHistory)
        {
            AddConversationMessage(chatHistory, message);
        }

        chatHistory.AddUserMessage(request.UserMessage);

        PromptExecutionSettings executionSettings = provider.Name.ToLowerInvariant() switch
        {
            "gemini" => new GeminiPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            },

            "ollama" => new OllamaPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            },

            _ => throw new InvalidOperationException(
                $"Unsupported provider '{provider.Name}' for Semantic Kernel runtime.")
        };

        var response = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel: kernel,
            cancellationToken: cancellationToken);

        return new AiProviderResponse
        {
            Content = response.Content ?? string.Empty,
            ProviderName = provider.Name,
            Success = true,
            Metadata =
            {
                ["agentName"] = request.AgentName ?? "unknown",
                ["historyMessageCount"] = request.ConversationHistory.Count
            }
        };
    }

    private static void AddConversationMessage(
        ChatHistory chatHistory,
        AiConversationMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            return;
        }

        switch (message.Role.ToLowerInvariant())
        {
            case "user":
                chatHistory.AddUserMessage(message.Content);
                break;

            case "assistant":
                chatHistory.AddAssistantMessage(message.Content);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported conversation role '{message.Role}'.");
        }
    }
}
