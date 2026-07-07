namespace EnterpriseAiPortfolio.Orchestration;

public sealed class AiExecutionResult
{
    public required string Response { get; init; }

    public required string ProviderName { get; init; }

    public string CorrelationId { get; init; } = string.Empty;

    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public Dictionary<string, object> Metadata { get; init; } = new();
}
