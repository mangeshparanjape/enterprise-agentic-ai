namespace EnterpriseAiPortfolio.Ai;

public sealed class OllamaOptions
{
    public const string SectionName = "Ai:Ollama";

    public required string Endpoint { get; init; }

    public required string ModelId { get; init; }
}