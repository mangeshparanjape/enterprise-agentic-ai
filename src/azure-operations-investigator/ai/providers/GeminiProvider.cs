using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Google;

namespace EnterpriseAiPortfolio.Ai;

#pragma warning disable SKEXP0070

public sealed class GeminiProvider : IAiProvider
{
    private readonly GeminiOptions _options;
    private readonly IAiKernelFactory _kernelFactory;

    public GeminiProvider(
        IOptions<GeminiOptions> options,
        IAiKernelFactory kernelFactory)
    {
        _options = options.Value;
        _kernelFactory = kernelFactory;
    }

    public string Name => "Gemini";

    public void ConfigureKernel(IKernelBuilder kernelBuilder)
    {
        kernelBuilder.AddGoogleAIGeminiChatCompletion(
            modelId: _options.ModelId,
            apiKey: _options.ApiKey);
    }

    public async Task<AiProviderResponse> ExecuteAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var kernel = _kernelFactory.CreateKernel(this);

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var chatHistory = new ChatHistory();

        if (!string.IsNullOrWhiteSpace(request.SystemMessage))
        {
            chatHistory.AddSystemMessage(request.SystemMessage);
        }

        chatHistory.AddUserMessage(request.UserMessage);
        var executionSettings = new GeminiPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
        var response = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings,
            kernel: kernel,
            cancellationToken: cancellationToken);

        return new AiProviderResponse
        {
            Content = response.Content ?? string.Empty,
            ProviderName = Name,
            Success = true,
            Metadata =
            {
                ["agentName"] = request.AgentName ?? "unknown"
            }
        };
    }
}