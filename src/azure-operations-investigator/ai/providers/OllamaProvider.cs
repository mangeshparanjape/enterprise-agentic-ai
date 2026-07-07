using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace EnterpriseAiPortfolio.Ai;

#pragma warning disable SKEXP0070

public sealed class OllamaProvider : IAiProvider
{
    private readonly OllamaOptions _options;

    public OllamaProvider(IOptions<OllamaOptions> options)
    {
        _options = options.Value;
    }

    public string Name => "Ollama";

    public void ConfigureKernel(IKernelBuilder kernelBuilder)
    {
        kernelBuilder.AddOllamaChatCompletion(
            modelId: _options.ModelId,
            endpoint: new Uri(_options.Endpoint));
    }
}