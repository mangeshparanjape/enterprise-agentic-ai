using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EnterpriseAiPortfolio.Ai;

#pragma warning disable SKEXP0070

public sealed class OllamaProvider : IAiProvider
{
    private readonly OllamaOptions _options;
    private readonly IAiKernelFactory _kernelFactory;

    public OllamaProvider(
        IOptions<OllamaOptions> options,
        IAiKernelFactory kernelFactory)
    {
        _options = options.Value;
        _kernelFactory = kernelFactory;
    }

    public string Name => "Ollama";

    public void ConfigureKernel(IKernelBuilder kernelBuilder)
    {
        kernelBuilder.AddOllamaChatCompletion(
            modelId: _options.ModelId,
            endpoint: new Uri(_options.Endpoint));
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

        var response = await chatCompletionService.GetChatMessageContentAsync(
            chatHistory,
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