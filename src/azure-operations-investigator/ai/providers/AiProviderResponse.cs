namespace EnterpriseAiPortfolio.Ai;

public sealed class AiProviderResponse
{
    public required string Content { get; init; }

    public required string ProviderName { get; init; }

    public bool Success { get; init; } = true;

    public string? ErrorMessage { get; init; }

    public Dictionary<string, object> Metadata { get; init; } = new();
}