using Microsoft.Extensions.Configuration;

namespace EnterpriseAiPortfolio.Ai;

public sealed class AiProviderFactory : IAiProviderFactory
{
    private readonly IConfiguration _configuration;
    private readonly OllamaProvider _ollamaProvider;
    private readonly GeminiProvider _geminiProvider;

    public AiProviderFactory(
        IConfiguration configuration,
        OllamaProvider ollamaProvider,
        GeminiProvider geminiProvider)
    {
        _configuration = configuration;
        _ollamaProvider = ollamaProvider;
        _geminiProvider = geminiProvider;
    }

    public IAiProvider CreateProvider()
    {
        var provider = _configuration["Ai:Provider"];

        return provider?.ToLowerInvariant() switch
        {
            "ollama" => _ollamaProvider,
            "gemini" => _geminiProvider,

            _ => throw new InvalidOperationException(
                $"Unknown AI provider '{provider}'.")
        };
    }
}