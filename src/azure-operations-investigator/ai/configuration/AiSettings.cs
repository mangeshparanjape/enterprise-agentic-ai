public sealed class AiSettings
{
    public AiProviderType Provider { get; init; } = AiProviderType.Ollama;
}
public enum AiProviderType
{
    Ollama,

    Gemini,

    AzureOpenAI
}