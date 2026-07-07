using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai;

#pragma warning disable SKEXP0070

public sealed class GeminiProvider : IAiProvider
{
    private readonly GeminiOptions _options;

    public GeminiProvider(IOptions<GeminiOptions> options)
    {
        _options = options.Value;
    }

    public string Name => "Gemini";

    public void ConfigureKernel(IKernelBuilder kernelBuilder)
    {
        kernelBuilder.AddGoogleAIGeminiChatCompletion(
            modelId: _options.ModelId,
            apiKey: _options.ApiKey);
    }
}