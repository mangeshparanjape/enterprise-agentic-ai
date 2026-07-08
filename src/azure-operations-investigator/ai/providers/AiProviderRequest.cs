namespace EnterpriseAiPortfolio.Ai;

public sealed class AiProviderRequest
{
    public required string UserMessage { get; init; }

    public string? SystemMessage { get; init; }

    public string? AgentName { get; init; }

    public IReadOnlyCollection<AiConversationMessage> ConversationHistory { get; init; } = [];

    public Dictionary<string, object> Metadata { get; init; } = new();
}
