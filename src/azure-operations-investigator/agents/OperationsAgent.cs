using EnterpriseAiPortfolio.Ai;
using EnterpriseAiPortfolio.Orchestration;
using Microsoft.Extensions.Options;

namespace EnterpriseAiPortfolio.Agents;

public sealed class OperationsAgent : IOperationsAgent
{
    private readonly IAiRequestOrchestrator _orchestrator;
    private readonly OperationsAgentOptions _options;
    private readonly List<AiConversationMessage> _conversationHistory = [];

    public OperationsAgent(
        IAiRequestOrchestrator orchestrator,
        IOptions<OperationsAgentOptions> options)
    {
        _orchestrator = orchestrator;
        _options = options.Value;
    }

    public async Task<string> ChatAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        var request = new AiRequestContext
        {
            UserMessage = userMessage,
            AgentName = nameof(OperationsAgent),
            SystemMessage = """
                You are an enterprise operations assistant.
                Use available plugins whenever appropriate to answer questions accurately.
                """,
            ConversationHistory = _conversationHistory.AsReadOnly()
        };

        var result = await _orchestrator.ExecuteAsync(
            request,
            cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"AI request failed. Provider: {result.ProviderName}. Error: {result.ErrorMessage}");
        }

        _conversationHistory.Add(AiConversationMessage.User(userMessage));
        _conversationHistory.Add(AiConversationMessage.Assistant(result.Response));

        TrimConversationHistory();

        return result.Response;
    }

    private void TrimConversationHistory()
    {
        var maxHistoryMessages = _options.MaxHistoryTurns * 2;
        var messagesToRemove = _conversationHistory.Count - maxHistoryMessages;

        if (messagesToRemove > 0)
        {
            _conversationHistory.RemoveRange(0, messagesToRemove);
        }
    }
}
