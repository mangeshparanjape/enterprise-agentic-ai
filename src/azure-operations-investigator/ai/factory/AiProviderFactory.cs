using Microsoft.Extensions.Configuration;

public static class AiProviderFactory
{
    public static IAiProvider Create(
        IConfiguration configuration)
    {
        var provider =
            configuration["AI:Provider"];

        return provider switch
        {
            "Gemini" => new GeminiProvider(),

            "Ollama" => new OllamaProvider(),

            _ => throw new InvalidOperationException(
                $"Unknown provider {provider}")
        };
    }
}