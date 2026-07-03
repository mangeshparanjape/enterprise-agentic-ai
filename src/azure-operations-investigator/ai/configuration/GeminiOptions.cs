namespace EnterpriseAiPortfolio.Ai;

public sealed class GeminiOptions
{
    public const string SectionName = "Ai:Gemini";

    public required string ApiKey { get; init; }

    public required string ModelId { get; init; }
}