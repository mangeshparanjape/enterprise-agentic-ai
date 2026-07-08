using EnterpriseAiPortfolio.Ai;

namespace EnterpriseAiPortfolio.Orchestration;

public sealed class AiRequestContext
{
    public required string UserMessage { get; init; }

    public string? SystemMessage { get; init; }

    public string? AgentName { get; init; }

    public string CorrelationId { get; init; } = Guid.NewGuid().ToString("N");

    public IReadOnlyCollection<AiConversationMessage> ConversationHistory { get; init; } = [];

    public Dictionary<string, object> Metadata { get; init; } = new();
}
